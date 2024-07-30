using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.F
{
    public class FA0016Repository : JCLib.Mvc.BaseRepository
    {
        public FA0016Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ME_DOCD> GetAll(string MAT_CLASS,string APVTIME_B, string APVTIME_E, string MMCODE, bool isFlowid6only, bool isApvqtynot0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select a.towh as appdept, 
                               INID_NAME(a.towh) as appdeptname,
                               b.mmcode,
                               a.mat_class,
                               (select mmname_c from MI_MAST where mmcode = b.mmcode) as mmname_c,
                               (select mmname_e from MI_MAST where mmcode = b.mmcode) as mmname_e,
                               (select base_unit from MI_MAST where mmcode = b.mmcode) as base_unit,
                               TWN_TIME(b.dis_time) as apvtime,
                               b.apvqty, 
                               AVG_PRICE(NVL(TWN_DATE(b.apvtime),TWN_DATE(b.dis_time)), b.mmcode) m_contprice,
                               a.docno,
                               (select flowid||' '||flowname from ME_FLOW 
                                 where doctype=a.doctype 
                                   and flowid=a.flowid) as FLOWID

                          from ME_DOCM a,ME_DOCD b
                          where a.docno=b.docno 
                            AND a.doctype IN ('MR1','MR2','MR3','MR4')
                            AND a.flowid IN ('3','4','5','6', '51')
                            and a.mat_class in ('02','03','04','05','06','07','08')
                            and TWN_DATE(b.dis_time)>=:APVTIME_B
                            and TWN_DATE(b.dis_time)<=:APVTIME_E
                         ";

            p.Add(":APVTIME_B", string.Format("{0}", APVTIME_B));
            p.Add(":APVTIME_E", string.Format("{0}", APVTIME_E));


            if (MAT_CLASS != "")
            {
                if (MAT_CLASS == "38")
                {
                    sql += " AND a.mat_class in ('03','04','05','06','07','08') ";
                }
                else
                {
                    sql += " AND a.mat_class  = :MAT_CLASS ";
                    p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS));
                }
            }
            
            if (MMCODE != "")
            {
                sql += " AND b.mmcode  = :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}", MMCODE));
            }
            if (isFlowid6only) {
                sql += "  and a.flowid = '6' ";
            }
            if (isApvqtynot0)
            {
                sql += " and (select b.apvqty from dual where a.flowid='6') || (select b.expt_distqty from dual where flowid <> '6') > 0 ";
            }

            sql += @" order by a.towh,a.mat_class, b.mmcode ";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCD>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<ComboItemModel> GetMATCombo(bool matUserID)
        {
            string sql= @"Select MAT_CLASS||' '||MAT_CLSNAME TEXT, MAT_CLASS VALUE from MI_MATCLASS 
                             where MAT_CLSID IN ('2','3') 
                            ORDER BY VALUE";
            

            return DBWork.Connection.Query<ComboItemModel>(sql, new { userID=DBWork.ProcUser }, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT FROM MI_MAST A WHERE 1=1 ";


            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(A.MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(A.MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("%{0}%", p0));

                sql += " OR A.MMNAME_E LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR A.MMNAME_C LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX, MMCODE", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY A.MMCODE ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public DataTable GetExcel(string MAT_CLASS, string APVTIME_B, string APVTIME_E, string MMCODE, bool isFlowid6only, bool isApvqtynot0)
        {
            var p = new DynamicParameters();

            var sql = @"select a.towh as 責任中心,
                               INID_NAME(a.towh) as 單位名稱,
                               b.mmcode 院內碼,
                               a.mat_class 物料分類,
                               (select mmname_c from MI_MAST where mmcode=b.mmcode) as 中文品名,
                               (select mmname_e from MI_MAST where mmcode=b.mmcode) as 英文品名,
                               (select base_unit from MI_MAST where mmcode=b.mmcode) as 計量單位,
                               TWN_TIME(b.dis_time) as 核撥日期,
                               (select b.apvqty from dual where a.flowid='6')
                                       ||(select b.expt_distqty from dual where flowid<>'6') as 數量,
                               AVG_PRICE(NVL(TWN_DATE(b.apvtime),TWN_DATE(b.dis_time)), b.mmcode) 庫存平均單價,
                               a.docno as 申請單號 ,
                               (select flowid||' '||flowname from ME_FLOW 
                                        where doctype=a.doctype 
                                          and flowid=a.flowid) as 申請單狀態
                          from ME_DOCM a,ME_DOCD b
                         where a.docno=b.docno 
                           and a.doctype in ('MR1','MR2','MR3','MR4')
                           and a.flowid in ('3','4','5','6', '51')
                           and a.mat_class in ('02','03','04','05','06','07','08')
                           and TWN_DATE(b.apvtime)>=:APVTIME_B
                           and TWN_DATE(b.apvtime)<=:APVTIME_E
                         ";

            p.Add(":APVTIME_B", string.Format("{0}", APVTIME_B));
            p.Add(":APVTIME_E", string.Format("{0}", APVTIME_E));


            if (MAT_CLASS != "")
            {
                if (MAT_CLASS == "38")
                {
                    sql += " AND a.mat_class in ('03','04','05','06','07','08') ";
                }
                else
                {
                    sql += " AND a.mat_class  = :MAT_CLASS ";
                    p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS));
                }
            }
            
            if (MMCODE != "")
            {
                sql += " and b.mmcode  = :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}", MMCODE));
            }
            if (isFlowid6only) {
                sql += "  and a.flowid = '6' ";
            }
            if (isApvqtynot0)
            {
                sql += " and (select b.apvqty from dual where a.flowid='6') || (select b.expt_distqty from dual where flowid <> '6') > 0 ";
            }

            sql += @" order by a.towh, a.mat_class, b.mmcode ";


            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public DataTable Report(string MAT_CLASS, string APVTIME_B, string APVTIME_E, string MMCODE, bool isFlowid6only, bool isApvqtynot0)
        {
            var p = new DynamicParameters();

            var sql = @"select a.towh as appdept, 
                               INID_NAME(a.towh) as appdeptname,
                               b.mmcode,
                               a.mat_class,
                               (select mmname_c from MI_MAST where mmcode = b.mmcode) as mmname_c,
                               (select mmname_e from MI_MAST where mmcode = b.mmcode) as mmname_e,
                               (select base_unit from MI_MAST where mmcode = b.mmcode) as base_unit,
                               TWN_TIME(b.dis_time) as apvtime,
                               (select b.apvqty from dual where a.flowid='6')
                                       || (select b.expt_distqty from dual where flowid<>'6') as apvqty,
                               AVG_PRICE(NVL(TWN_DATE(b.apvtime),TWN_DATE(b.dis_time)), b.mmcode) m_contprice,
                               a.docno,
                               (select flowid||' '||flowname from ME_FLOW 
                                 where doctype=a.doctype 
                                   and flowid=a.flowid) as FLOWID

                          from ME_DOCM a,ME_DOCD b
                          where a.docno=b.docno 
                            AND a.doctype IN ('MR1','MR2','MR3','MR4')
                            AND a.flowid IN ('3','4','5','6','51')
                            and a.mat_class in ('02','03','04','05','06','07','08')
                            and TWN_DATE(b.apvtime)>=:APVTIME_B
                            and TWN_DATE(b.apvtime)<=:APVTIME_E
                         ";

            p.Add(":APVTIME_B", string.Format("{0}", APVTIME_B));
            p.Add(":APVTIME_E", string.Format("{0}", APVTIME_E));


            if (MAT_CLASS != "")
            {
                if (MAT_CLASS == "38")
                {
                    sql += " AND a.mat_class in ('03','04','05','06','07','08') ";
                }
                else
                {
                    sql += " AND a.mat_class  = :MAT_CLASS ";
                    p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS));
                }
            }

            if (MMCODE != "")
            {
                sql += " AND b.mmcode  = :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}", MMCODE));
            }
            if (isFlowid6only)
            {
                sql += "  and a.flowid = '6' ";
            }
            if (isApvqtynot0)
            {
                sql += " and (select b.apvqty from dual where a.flowid='6') || (select b.expt_distqty from dual where flowid <> '6') > 0 ";
            }

            sql += @" order by a.towh,a.mat_class, b.mmcode ";


            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public string GetReportWH_NAME()
        {
            string sql = @"select WH_NAME(WHNO_MM1) from DUAL";
            var str = DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction);
            return str==null? "":str.ToString();
        }
    }
}