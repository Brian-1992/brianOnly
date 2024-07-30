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
    public class AA0065Repository : JCLib.Mvc.BaseRepository
    {
        public AA0065Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        // GET api/<controller>
        public IEnumerable<ME_DOCM> GetAll(string apptime_bg, string apptime_ed, string mclass, string flowid, string mmcode, string wh_userId, int page_index, int page_size, string sorters)
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

                    //  rde += ",";
                    strInPar += "@p4_" + i;
                    rde += "'" + flowidSplit[i] + "'";
                    p.Add("@p4_" + i, "'" + flowidSplit[i] + "'");

                }
                sql += " AND A.FLOWID in (" + rde + ")";
                //sql += " AND mi_whmast.wh_grade = :p4 ";
                //p.Add(":p4", grade);
            }
            if (mmcode != "" && mmcode != null)
            {
                sql += " and B.MMCODE = :p5 ";
                //p.Add(":p7", mmcode);
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
                                   (select avg_price from MI_WHCOST where data_ym = a.post_time_ym and mmcode = a.mmcode)
                               else
                                   (select avg_price from MI_WHCOST where data_ym = a.apptime_ym and mmcode = a.mmcode)
                       end) avg_price,
                       a.DOCNO,a.M_AGENNO ,a.APPLY_NOTE
                  from temp_datas a
            ", sql);

            //p.Add(":p6", ym);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCM>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public DataTable GetExcel(string apptime_bg, string apptime_ed, string mclass, string flowid, string mmcode, string wh_userId)
        {

            var p = new DynamicParameters();
            // DataTable dt = new DataTable();

            //var sql = @"    SELECT
            //                    A.DOCNO as 報廢號碼,
            //                    B.SEQ as 項次,
            //                    (select FLOWNAME from ME_FLOW where DOCTYPE = 'SP1' and FLOWID = A.FLOWID) as 申請單狀態,
            //                    (select MAT_CLASS || ' ' || MAT_CLSNAME from MI_MATCLASS where MAT_CLASS = A.MAT_CLASS) as 物料分類,
            //                    (SELECT INID || ' ' || INID_NAME FROM UR_INID WHERE INID=A.APPDEPT) 申請單位,
            //                    USER_NAME(A.APPID) as 申請人,
            //                    to_char(A.APPTIME, 'YYYY/MM/DD') as 申請日期,
            //                    A.APPLY_NOTE as 申請單備註,
            //                    B.MMCODE as 院內碼,
            //                    (select MMNAME_E from MI_MAST where MI_MAST.MMCODE = B.MMCODE) as 品名,
            //                    B.APPQTY as 報廢數量,
            //                    (select BASE_UNIT from MI_MAST where MI_MAST.MMCODE = B.MMCODE) as 單位,
            //                    (select AVG_PRICE from MI_WHCOST where MI_WHCOST.MMCODE = B.MMCODE and SET_YM = (SELECT SET_YM FROM (select SET_YM from MI_WHCOST order by SET_YM desc) WHERE ROWNUM = 1)) as 平均單價
            //                FROM
            //                    ME_DOCM A, ME_DOCD B
            //                WHERE
            //                    A.DOCNO = B.DOCNO
            //                    and A.DOCTYPE = 'SP1'
            //                    and A.FRWH = WHNO_MM1
            //                    and A.MAT_CLASS in (select MAT_CLASS from MI_MATCLASS where MAT_CLSID = WHM1_TASK(:wh_userId))
            //                    and A.FLOWID in (select FLOWID from ME_FLOW where DOCTYPE = 'SP1')
            //                ";

            //p.Add(":wh_userId", wh_userId);
            var sql = @"SELECT B.MMCODE,C.MAT_CLASS,C.MMNAME_C,C.MMNAME_E,C.BASE_UNIT,
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

                    //  rde += ",";
                    strInPar += "@p4_" + i;
                    rde += "'" + flowidSplit[i] + "'";
                    p.Add("@p4_" + i, "'" + flowidSplit[i] + "'");

                }
                sql += " and A.FLOWID in (" + rde + ")";
                //sql += " AND mi_whmast.wh_grade = :p4 ";
                //p.Add(":p4", grade);
            }
            if (mmcode != "" && mmcode != null)
            {
                sql += " and B.MMCODE = :p5 ";
                //p.Add(":p7", mmcode);
            }


            sql += " order by B.MMCODE";
            p.Add(":p5", mmcode);
            p.Add(":p6", ym);

            sql = string.Format(@"
                with temp_datas as (
                    {0}
                )
                select a.MMCODE 院內碼,a.MAT_CLASS 物料分類,a.MMNAME_C 中文品名,a.MMNAME_E 英文品名,a.BASE_UNIT 計量單位, a.APPQTY 報廢數量,
                       nvl(twn_date(a.post_time), twn_date(a.apptime)) as 報廢日期,
                       (case when a.post_time_ym is not null 
                               then
                                   (select avg_price from MI_WHCOST where data_ym = a.post_time_ym and mmcode = a.mmcode)
                               else
                                   (select avg_price from MI_WHCOST where data_ym = a.apptime_ym and mmcode = a.mmcode)
                       end) 庫存平均單價,
                       a.DOCNO 申請單號,a.M_AGENNO 廠商碼 ,a.APPLY_NOTE 申請單備註
                  from temp_datas a
            ", sql);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
        public IEnumerable<ME_DOCM> GetReport(string apptime_bg, string apptime_ed, string mclass, string flowid, string mmcode, string wh_userId)
        {
            var p = new DynamicParameters();

            //var sql = @"    SELECT
            //                    A.DOCNO,
            //                    B.SEQ,
            //                    (select FLOWNAME from ME_FLOW where DOCTYPE = 'SP1' and FLOWID = A.FLOWID) as FLOWID,
            //                    (select MAT_CLASS || ' ' || MAT_CLSNAME from MI_MATCLASS where MAT_CLASS = A.MAT_CLASS) as MAT_CLASS,
            //                    USER_NAME(A.APPID) as APPID,
            //                    to_char(A.APPTIME, 'YYYY/MM/DD') as APPTIME,
            //                    A.APPLY_NOTE,
            //                    B.MMCODE,
            //                    (select MMNAME_E from MI_MAST where MI_MAST.MMCODE = B.MMCODE) as MMNAME_E,
            //                    B.APPQTY,
            //                    (select BASE_UNIT from MI_MAST where MI_MAST.MMCODE = B.MMCODE) as BASE_UNIT,
            //                    (select AVG_PRICE from MI_WHCOST where MI_WHCOST.MMCODE = B.MMCODE and SET_YM = (SELECT SET_YM FROM (select SET_YM from MI_WHCOST order by SET_YM desc) WHERE ROWNUM = 1)) as AVG_PRICE
            //                FROM
            //                    ME_DOCM A, ME_DOCD B
            //                WHERE
            //                    A.DOCNO = B.DOCNO
            //                    and A.DOCTYPE = 'SP1'
            //                    and A.FRWH = WHNO_MM1
            //                    and A.MAT_CLASS in (select MAT_CLASS from MI_MATCLASS where MAT_CLSID = WHM1_TASK(:wh_userId))
            //                    and A.FLOWID in (select FLOWID from ME_FLOW where DOCTYPE = 'SP1')
            //                ";

            //p.Add(":wh_userId", wh_userId);
            var sql = @"SELECT B.MMCODE,C.MAT_CLASS,C.MMNAME_C,C.MMNAME_E,C.BASE_UNIT,
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

                    //  rde += ",";
                    strInPar += "@p4_" + i;
                    rde += "'" + flowidSplit[i] + "'";
                    p.Add("@p4_" + i, "'" + flowidSplit[i] + "'");

                }
                sql += " and A.FLOWID in (" + rde + ")";
                //sql += " AND mi_whmast.wh_grade = :p4 ";
                //p.Add(":p4", grade);
            }
            if (mmcode != "" && mmcode != null && mmcode != "undefined")
            {
                sql += " and B.MMCODE = :p5 ";
                //p.Add(":p7", mmcode);
            }


            sql += " order by B.MMCODE";
            p.Add(":p5", mmcode);
            p.Add(":p6", ym);

            sql = string.Format(@"
                with temp_datas as (
                    {0}
                )
                select a.MMCODE,a.MAT_CLASS,a.MMNAME_C,a.MMNAME_E,a.BASE_UNIT, a.APPQTY,
                       nvl(twn_date(a.post_time), twn_date(a.apptime)) as post_time,
                       (case when a.post_time_ym is not null 
                               then
                                   (select avg_price from MI_WHCOST where data_ym = a.post_time_ym and mmcode = a.mmcode)
                               else
                                   (select avg_price from MI_WHCOST where data_ym = a.apptime_ym and mmcode = a.mmcode)
                       end) avg_price,
                       a.DOCNO,a.M_AGENNO ,a.APPLY_NOTE
                  from temp_datas a
            ", sql);

            return DBWork.Connection.Query<ME_DOCM>(sql, p, DBWork.Transaction);
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