using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.F
{
    public class FA0076Repository : JCLib.Mvc.BaseRepository
    {
        public FA0076Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="MAT_CLASS"></param>
        /// <param name="APPTIME_B"></param>
        /// <param name="APPTIME_E"></param>
        /// <param name="P4"></param>   庫備類別
        /// <param name="P5"></param>  是否核撥
        /// <param name="P6"></param>  申請狀態
        /// <param name="page_index"></param>
        /// <param name="page_size"></param>
        /// <param name="sorters"></param>
        /// <returns></returns>
        public IEnumerable<FA0076> GetAll_A(string MAT_CLASS, string APPTIME_B, string APPTIME_E, string P4, string P5, string P6, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select a.appdept
                        ,(select inid_name from UR_INID where inid=a.appdept) as appdeptname 
                        ,TWN_DATE(a.apptime) as appdate
                        ,COUNT(distinct a.docno) as docno_cnt
                        ,COUNT(b.mmcode) as mmcode_cnt
                        ,SUM(b.appqty*(select DISC_CPRICE from MI_WHCOST where data_ym=substr(TWN_DATE(a.apptime),1,5)
                        and mmcode=b.mmcode)) as amt_sum
                        ,SUM(((select b.apvqty from dual where b.apvqty is not null and b.apvqty<>0)
                        ||(select b.expt_distqty from dual where b.apvqty is null or b.apvqty=0))
                        *(select DISC_CPRICE from MI_WHCOST where data_ym=substr(TWN_DATE(b.apvtime),1,5)
                        and mmcode=b.mmcode)) as apv_sum
                                from ME_DOCM a,ME_DOCD b
                               where a.docno=b.docno  
                                 and TWN_DATE(a.apptime)>=:APPTIME_B
                                 and TWN_DATE(a.apptime)<=:APPTIME_E
                         ";

            p.Add(":APPTIME_B", string.Format("{0}", APPTIME_B));
            p.Add(":APPTIME_E", string.Format("{0}", APPTIME_E));


            if (MAT_CLASS != "")
            {
                sql += " AND A.MAT_CLASS  = :MAT_CLASS ";
                p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS));
            }
            if (P4 == "1" || P4 == "0")
            {
                sql += "  and(select m_storeid from MI_MAST where mmcode = b.mmcode)=:p4 ";
                p.Add(":p4", string.Format("{0}", P4));
            }
            if (P6 == "1" || P6 == "2")
            {
                sql += "  and a.apply_kind=:p6 ";
                p.Add(":p6", string.Format("{0}", P6));
            }
            if (P5 == "1")
            {
                sql += @"  and ((a.doctype='MR' and a.flowid>='0103')
                                or(a.doctype = 'MS' and a.flowid >= '0602')
                                or(a.doctype in ('MR1', 'MR2', 'MR3', 'MR4','MR5','MR6') and a.flowid >= '3')) ";
            }
            else
            {
                sql += @"  and ((a.doctype='MR' and a.flowid<'0103')
                                or(a.doctype = 'MS' and a.flowid < '0602')
                                or(a.doctype in ('MR1', 'MR2', 'MR3', 'MR4','MR5','MR6') and a.flowid = '2')) ";
            }

            sql += @"   group by a.appdept,TWN_DATE(a.apptime)
                       order by a.appdept,TWN_DATE(a.apptime) ";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<FA0076>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<FA0076> GetAll_M(string MAT_CLASS, string APPTIME_B, string APPTIME_E, string P4, string P5, string P6, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @" select b.mmcode
                         ,(select mmname_c from MI_MAST where mmcode=b.mmcode) as mmname_c
                         ,(select mmname_e from MI_MAST where mmcode=b.mmcode) as mmname_e
                         ,(select m_contprice from MI_MAST where mmcode=b.mmcode) as m_contprice
                         ,SUM(b.appqty) as app_sum
                         ,SUM((select b.apvqty from dual where b.apvqty is not null and b.apvqty<>0)
                         ||(select b.expt_distqty from dual where b.apvqty is null or b.apvqty=0)) as apv_sum
                                from ME_DOCM a,ME_DOCD b
                               where a.docno=b.docno 
                                 and TWN_DATE(a.apptime)>=:APPTIME_B
                                 and TWN_DATE(a.apptime)<=:APPTIME_E
                         ";

            p.Add(":APPTIME_B", string.Format("{0}", APPTIME_B));
            p.Add(":APPTIME_E", string.Format("{0}", APPTIME_E));


            if (MAT_CLASS != "")
            {
                sql += " AND A.MAT_CLASS  = :MAT_CLASS ";
                p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS));
            }
            if (P4 == "1" || P4 == "0")
            {
                sql += "  and(select m_storeid from MI_MAST where mmcode = b.mmcode)=:p4 ";
                p.Add(":p4", string.Format("{0}", P4));
            }
            if (P6 == "1" || P6 == "2")
            {
                sql += "  and a.apply_kind=:p6 ";
                p.Add(":p6", string.Format("{0}", P6));
            }
            if (P5 == "1")
            {
                sql += @"  and ((a.doctype='MR' and a.flowid>='0103')
                                or(a.doctype = 'MS' and a.flowid >= '0602')
                                or(a.doctype in ('MR1', 'MR2', 'MR3', 'MR4','MR5','MR6') and a.flowid >= '3')) ";
            }
            else
            {
                sql += @"  and ((a.doctype='MR' and a.flowid<'0103')
                                or(a.doctype = 'MS' and a.flowid < '0602')
                                or(a.doctype in ('MR1', 'MR2', 'MR3', 'MR4','MR5','MR6') and a.flowid = '2')) ";
            }

            sql += @"  group by b.mmcode
                       order by b.mmcode ";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            sorters = "[{\"property\":\"mmcode\",\"direction\":\"ASC\"}]";
            return DBWork.Connection.Query<FA0076>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<ComboItemModel> GetMATCombo()
        {
            string sql = @"Select MAT_CLASS||' '||MAT_CLSNAME TEXT, MAT_CLASS VALUE from MI_MATCLASS 
                             where mat_class <> '01' 
                            ORDER BY VALUE";

            return DBWork.Connection.Query<ComboItemModel>(sql, new { userID = DBWork.ProcUser }, DBWork.Transaction);
        }


        public DataTable GetExcel_A(string MAT_CLASS, string APPTIME_B, string APPTIME_E, string P4, string P5, string P6)
        {
            var p = new DynamicParameters();

            var sql = @"select a.appdept 成本碼
                        ,(select inid_name from UR_INID where inid=a.appdept) as 單位名稱 
                        ,TWN_DATE(a.apptime) as 申請日期  
                        ,COUNT(distinct a.docno) as 申請張數  
                        ,COUNT(b.mmcode) as 申請項數  
                        ,SUM(b.appqty*(select DISC_CPRICE from MI_WHCOST where data_ym=substr(TWN_DATE(a.apptime),1,5)
                        and mmcode=b.mmcode)) as 申請金額   
                        ,SUM(((select b.apvqty from dual where b.apvqty is not null and b.apvqty<>0)
                        ||(select b.expt_distqty from dual where b.apvqty is null or b.apvqty=0))
                        *(select DISC_CPRICE from MI_WHCOST where data_ym=substr(TWN_DATE(b.apvtime),1,5)
                        and mmcode=b.mmcode)) as 撥發金額  
                                from ME_DOCM a,ME_DOCD b
                               where a.docno=b.docno  
                                 and TWN_DATE(a.apptime)>=:APPTIME_B
                                 and TWN_DATE(a.apptime)<=:APPTIME_E
                         ";

            p.Add(":APPTIME_B", string.Format("{0}", APPTIME_B));
            p.Add(":APPTIME_E", string.Format("{0}", APPTIME_E));


            if (MAT_CLASS != "")
            {
                sql += " AND A.MAT_CLASS  = :MAT_CLASS ";
                p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS));
            }
            if (P4 == "1" || P4 == "0")
            {
                sql += "  and(select m_storeid from MI_MAST where mmcode = b.mmcode)=:p4 ";
                p.Add(":p4", string.Format("{0}", P4));
            }
            if (P6 == "1" || P6 == "2")
            {
                sql += "  and a.apply_kind=:p6 ";
                p.Add(":p6", string.Format("{0}", P6));
            }
            if (P5 == "1")
            {
                sql += @"  and ((a.doctype='MR' and a.flowid>='0103')
                                or(a.doctype = 'MS' and a.flowid >= '0602')
                                or(a.doctype in ('MR1', 'MR2', 'MR3', 'MR4','MR5','MR6') and a.flowid >= '3')) ";
            }
            else
            {
                sql += @"  and ((a.doctype='MR' and a.flowid<'0103')
                                or(a.doctype = 'MS' and a.flowid < '0602')
                                or(a.doctype in ('MR1', 'MR2', 'MR3', 'MR4','MR5','MR6') and a.flowid = '2')) ";
            }

            sql += @"   group by a.appdept,TWN_DATE(a.apptime)
                       order by a.appdept,TWN_DATE(a.apptime) ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public DataTable GetExcel_M(string MAT_CLASS, string APPTIME_B, string APPTIME_E, string P4, string P5, string P6)
        {
            var p = new DynamicParameters();

            var sql = @" select b.mmcode  院內碼
                         ,(select mmname_c from MI_MAST where mmcode=b.mmcode) as 中文品名 
                         ,(select mmname_e from MI_MAST where mmcode=b.mmcode) as 英文品名 
                         ,(select m_contprice from MI_MAST where mmcode=b.mmcode) as 合約單價 
                         ,SUM(b.appqty) as 申請總量  
                         ,SUM((select b.apvqty from dual where b.apvqty is not null and b.apvqty<>0) 
                         ||(select b.expt_distqty from dual where b.apvqty is null or b.apvqty=0)) as 核撥總量 
                                from ME_DOCM a,ME_DOCD b
                               where a.docno=b.docno 
                                 and TWN_DATE(a.apptime)>=:APPTIME_B
                                 and TWN_DATE(a.apptime)<=:APPTIME_E
                         ";

            p.Add(":APPTIME_B", string.Format("{0}", APPTIME_B));
            p.Add(":APPTIME_E", string.Format("{0}", APPTIME_E));


            if (MAT_CLASS != "")
            {
                sql += " AND A.MAT_CLASS  = :MAT_CLASS ";
                p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS));
            }
            if (P4 == "1" || P4 == "0")
            {
                sql += "  and(select m_storeid from MI_MAST where mmcode = b.mmcode)=:p4 ";
                p.Add(":p4", string.Format("{0}", P4));
            }
            if (P6 == "1" || P6 == "2")
            {
                sql += "  and a.apply_kind=:p6 ";
                p.Add(":p6", string.Format("{0}", P6));
            }
            if (P5 == "1")
            {
                sql += @"  and ((a.doctype='MR' and a.flowid>='0103')
                                or(a.doctype = 'MS' and a.flowid >= '0602')
                                or(a.doctype in ('MR1', 'MR2', 'MR3', 'MR4','MR5','MR6') and a.flowid >= '3')) ";
            }
            else
            {
                sql += @"  and ((a.doctype='MR' and a.flowid<'0103')
                                or(a.doctype = 'MS' and a.flowid < '0602')
                                or(a.doctype in ('MR1', 'MR2', 'MR3', 'MR4','MR5','MR6') and a.flowid = '2')) ";
            }

            sql += @"  group by b.mmcode
                       order by b.mmcode ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
    }
}