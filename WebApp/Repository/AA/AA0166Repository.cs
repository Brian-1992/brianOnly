using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using System.Linq;
using TSGH.Models;

namespace WebApp.Repository.AA
{
    public class AA0166_MODEL : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; } //院內碼
        public string MAT_CLASS { get; set; } //物料分類
        public string MMNAME_C { get; set; } // 品名中文
        public string MMNAME_E { get; set; } // 品名英文
        public string BASE_UNIT { get; set; } // 單位
        public string APPQTY { get; set; } // 申請繳回數量
        public string POST_TIME { get; set; } // 申請繳回數量
        public string DISC_CPRICE { get; set; } // 優惠合約單價
        public string DOCNO { get; set; } // 單據號碼
        public string M_AGENNO { get; set; } 
        public string APPLY_NOTE { get; set; } // 申請單備註
    }
    public class AA0166Repository : JCLib.Mvc.BaseRepository
    {
        public AA0166Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        // GET api/<controller>
        public IEnumerable<AA0166_MODEL> GetAll(AA0166_MODEL v, string apptime_bg, string apptime_ed, string mclass, string flowid, string mmcode, string wh_userId, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = @"SELECT B.MMCODE,C.MAT_CLASS,C.MMNAME_C,C.MMNAME_E,C.BASE_UNIT,
                               B.APPQTY ,
                              -- AVG_PRICE(NVL(TWN_DATE(A.POST_TIME),TWN_DATE(A.APPTIME)),B.MMCODE) AVG_PRICE ,
                              -- NVL(TWN_DATE(A.POST_TIME),TWN_DATE(A.APPTIME)) POST_TIME ,
                                a.POST_TIME, a.APPTIME,
                                (select set_ym from MI_MNSET
                                  where a.post_time between set_btime and set_ctime) as post_time_ym,
                                (select set_ym from MI_MNSET
                                  where a.apptime between set_btime and set_ctime) as apptime_ym,
                               A.DOCNO,C.M_AGENNO ,A.APPLY_NOTE
                          FROM ME_DOCM A,ME_DOCD B,MI_MAST C
                         WHERE A.DOCNO=B.DOCNO AND B.MMCODE=C.MMCODE
                               AND A.DOCTYPE='SP1' AND A.FLOWID='3'
            ";
            var ym = "";

            if (apptime_bg != "")
            {
                sql += " AND (NVL(TWN_DATE(A.POST_TIME),TWN_DATE(A.APPTIME)) >= :p1) ";
                p.Add(":p1", apptime_bg);
            }
            if (apptime_ed != "")
            {
                sql += "  AND (NVL(TWN_DATE(A.POST_TIME),TWN_DATE(A.APPTIME)) <= :p2) ";
                p.Add(":p2", apptime_ed);
                ym = apptime_ed.Substring(0, 5);
            }
            if (mclass != "" && mclass != null)
            {
                if (mclass == "38")
                {
                    sql += " AND A.MAT_CLASS in ('03','04','05','06','07','08') ";
                }
                else
                {
                    sql += " and A.MAT_CLASS = :p3 ";
                    p.Add(":p3", mclass);
                }
            }
            if (flowid != "" && flowid != null)
            {
                string strInPar = "";
                string rde = "";
                string[] flowidSplit = flowid.Split(',');
                for (int i = 0; i < flowidSplit.Length; i++)
                {
                    if (!rde.Equals(""))
                        rde += ",";

                    strInPar += "@p4_" + i;
                    rde += "'" + flowidSplit[i] + "'";
                    p.Add("@p4_" + i, "'" + flowidSplit[i] + "'");

                }
                sql += " AND A.FLOWID in (" + rde + ")";
            }
            if (mmcode != "" && mmcode != null)
            {
                sql += " and B.MMCODE = :p5 ";
            }


            sql += " order by B.MMCODE";
            p.Add(":p5", mmcode);

            sql = string.Format(@"
                with temp_datas as (
                    {0}
                )
                select a.MMCODE,a.MAT_CLASS,a.MMNAME_C,a.MMNAME_E,a.BASE_UNIT, a.APPQTY,
                       nvl(twn_date(a.post_time), twn_date(a.apptime)) as post_time,
                       (case when a.post_time_ym is not null 
                               then
                                   (select DISC_CPRICE from MI_WHCOST where data_ym = a.post_time_ym and mmcode = a.mmcode)
                               else
                                   (select DISC_CPRICE from MI_WHCOST where data_ym = a.apptime_ym and mmcode = a.mmcode)
                       end) DISC_CPRICE,
                       a.DOCNO,a.M_AGENNO ,a.APPLY_NOTE
                  from temp_datas a
            ", sql);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0166_MODEL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public DataTable GetExcel(string apptime_bg, string apptime_ed, string mclass, string flowid, string mmcode, string wh_userId)
        {

            var p = new DynamicParameters();
            var sql = @"SELECT  B.MMCODE AS 院內碼,
                                C.MAT_CLASS AS 物料分類,
                                C.MMNAME_C AS 中文品名,
                                C.MMNAME_E AS 英文品名,
                                C.BASE_UNIT AS 計量單位,
                                B.APPQTY AS 報廢數量,
                                (case when a.post_time is not null 
                                   then
                                       (select DISC_CPRICE from MI_WHCOST 
                                         where data_ym = (select set_ym from MI_MNSET
                                                            where a.post_time between set_btime and set_ctime) and mmcode = b.mmcode)
                                    else
                                       (select DISC_CPRICE from MI_WHCOST 
                                         where data_ym = (select set_ym from MI_MNSET
                                                           where a.apptime between set_btime and set_ctime) and mmcode = b.mmcode)
                                end) 優惠合約單價,
                                NVL(TWN_DATE(A.POST_TIME), TWN_DATE(A.APPTIME)) 報廢日期 ,
                                A.DOCNO AS 申請單號,
                                C.M_AGENNO AS 廠商碼,
                                A.APPLY_NOTE AS 申請單備註 
                          FROM ME_DOCM A,ME_DOCD B,MI_MAST C
                         WHERE A.DOCNO=B.DOCNO AND B.MMCODE=C.MMCODE
                               AND A.DOCTYPE='SP1' AND A.FLOWID='3'
            ";
            var ym = apptime_ed.Substring(0, 5);

            if (apptime_bg != "")
            {
                sql += " AND (NVL(TWN_DATE(A.POST_TIME),TWN_DATE(A.APPTIME)) >= :p1) ";
                p.Add(":p1", apptime_bg);
            }
            if (apptime_ed != "")
            {
                sql += "  AND (NVL(TWN_DATE(A.POST_TIME),TWN_DATE(A.APPTIME)) <= :p2) ";
                p.Add(":p2", apptime_ed);
                ym = apptime_ed.Substring(0, 5);
                //p.Add(":p6")
            }
            if (mclass != "" && mclass != null)
            {
                if (mclass == "38")
                {
                    sql += " AND A.MAT_CLASS in ('03','04','05','06','07','08') ";
                }
                else
                {
                    sql += " and A.MAT_CLASS = :p3 ";
                    p.Add(":p3", mclass);
                }
            }
            if (flowid != "" && flowid != null)
            {
                string strInPar = "";
                string rde = "";
                string[] flowidSplit = flowid.Split(',');
                for (int i = 0; i < flowidSplit.Length; i++)
                {
                    if (!rde.Equals(""))
                        rde += ",";

                    strInPar += "@p4_" + i;
                    rde += "'" + flowidSplit[i] + "'";
                    p.Add("@p4_" + i, "'" + flowidSplit[i] + "'");

                }
                sql += " and A.FLOWID in (" + rde + ")";
            }
            if (mmcode != "" && mmcode != null)
            {
                sql += " and B.MMCODE = :p5 ";
            }

            sql += " order by B.MMCODE";
            p.Add(":p5", mmcode);
            p.Add(":p6", ym);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
        public IEnumerable<AA0166_MODEL> GetReport(string apptime_bg, string apptime_ed, string mclass, string flowid, string mmcode, string wh_userId)
        {
            var p = new DynamicParameters();
            
            var sql = @"SELECT B.MMCODE,C.MAT_CLASS,C.MMNAME_C,C.MMNAME_E,C.BASE_UNIT,
                               B.APPQTY ,
                                (case when a.post_time is not null 
                                   then
                                       (select DISC_CPRICE from MI_WHCOST 
                                         where data_ym = (select set_ym from MI_MNSET
                                                           where a.post_time between set_btime and set_ctime) and mmcode = b.mmcode)
                                    else
                                       (select DISC_CPRICE from MI_WHCOST 
                                         where data_ym =  (select set_ym from MI_MNSET
                                                            where a.apptime between set_btime and set_ctime) and mmcode = b.mmcode)
                                end) DISC_CPRICE,
                               NVL(TWN_DATE(A.POST_TIME),TWN_DATE(A.APPTIME)) POST_TIME ,
                               A.DOCNO,C.M_AGENNO ,A.APPLY_NOTE
                          FROM ME_DOCM A,ME_DOCD B,MI_MAST C
                         WHERE A.DOCNO=B.DOCNO AND B.MMCODE=C.MMCODE
                               AND A.DOCTYPE='SP1' AND A.FLOWID='3'
            ";
            var ym = apptime_ed.Substring(0, 5);

            if (apptime_bg != "")
            {
                sql += " AND (NVL(TWN_DATE(A.POST_TIME),TWN_DATE(A.APPTIME)) >= :p1) ";
                p.Add(":p1", apptime_bg);
            }
            if (apptime_ed != "")
            {
                sql += "  AND (NVL(TWN_DATE(A.POST_TIME),TWN_DATE(A.APPTIME)) <= :p2) ";
                p.Add(":p2", apptime_ed);
                ym = apptime_ed.Substring(0, 5);
            }
            if (mclass != "" && mclass != null)
            {
                if (mclass == "38")
                {
                    sql += " AND A.MAT_CLASS in ('03','04','05','06','07','08') ";
                }
                else
                {
                    sql += " and A.MAT_CLASS = :p3 ";
                    p.Add(":p3", mclass);
                }
            }
            if (flowid != "" && flowid != null)
            {
                string strInPar = "";
                string rde = "";
                string[] flowidSplit = flowid.Split(',');
                for (int i = 0; i < flowidSplit.Length; i++)
                {
                    if (!rde.Equals(""))
                        rde += ",";

                    strInPar += "@p4_" + i;
                    rde += "'" + flowidSplit[i] + "'";
                    p.Add("@p4_" + i, "'" + flowidSplit[i] + "'");

                }
                sql += " and A.FLOWID in (" + rde + ")";
            }
            if (mmcode != "" && mmcode != null && mmcode != "undefined")
            {
                sql += " and B.MMCODE = :p5 ";
            }

            sql += " order by B.MMCODE";
            p.Add(":p5", mmcode);
            p.Add(":p6", ym);

            return DBWork.Connection.Query<AA0166_MODEL>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, string p1, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} MMCODE, MMNAME_C, MMNAME_E, MAT_CLASS, BASE_UNIT FROM MI_MAST A WHERE 1=1 ";

            if (p1 != "")
            {
                if (p1 == "38")
                {
                    sql += " AND MAT_CLASS in ('03','04','05','06','07','08') ";
                }
                else
                {
                    sql += " AND MAT_CLASS = :p1 ";
                    p.Add(":p1", string.Format("{0}", p1));
                }
            }
            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}%", p0));

                sql += " OR MMNAME_E LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR MMNAME_C LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY MMCODE ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

    }
}