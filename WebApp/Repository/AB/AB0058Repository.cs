using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using System.Linq;
using TSGH.Models;

namespace WebApp.Repository.AB
{
    public class AB0058Repository : JCLib.Mvc.BaseRepository
    {
        public AB0058Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ME_DOCM> GetAll(string apptime_bg, string apptime_ed, string mclass, string flowid, string mmcode, int page_index, int page_size, string sorters, string wh_userId)
        {
            var p = new DynamicParameters();

            var sql = @"select A.DOCNO, A.APPTIME, CASE WHEN A.DOCTYPE='AJ1' then '其他調帳' When A.DOCTYPE='TR1' then '調撥至其他衛星庫' When A.DOCTYPE='RN1' then '繳回中央庫房' end as DOCTYPE ,WH_NAME(A.TOWH) as TOWH, (select MAT_CLASS || ' ' || MAT_CLSNAME from MI_MATCLASS where MAT_CLASS = A.MAT_CLASS) as MAT_CLASS , B.MMCODE,
                        C.mmname_C || C.mmname_E as MMNAME_E , B.APPQTY, (select base_unit from mi_mast where mi_mast.mmcode=B.MMCODE) as base_unit , C.M_CONTPRICE
                        FROM ME_DOCM A, ME_DOCD B, MI_MAST C WHERE A.DOCNO= B.DOCNO AND B.MMCODE = C.MMCODE AND A.FRWH= WHNO_MM1 and (A.FLOWID = '3' or A.FLOWID ='0299') ";

            if (apptime_bg != "")
            {
                sql += " AND TWN_DATE(A.APPTIME) >= Substr(:p1, 1, 10) ";
                p.Add(":p1", apptime_bg);
            }
            if (apptime_ed != "")
            {
                sql += " AND TWN_DATE(A.APPTIME) <= Substr(:p2, 1, 10) ";
                p.Add(":p2", apptime_ed);
                //p.Add(":p6")
            }
            if (mclass != "" && mclass != null)
            {
                sql += " AND A.MAT_CLASS = :p3 ";
                p.Add(":p3", mclass);
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
                sql += " AND A.DOCTYPE in (" + rde + ")";

            }
            else
            {
                sql += " AND A.DOCTYPE in ('RN1','TR1','AJ1') ";
            }
            if (mmcode != "" && mmcode != "null")
            {
                sql += " AND B.mmcode = :p5 ";
                p.Add(":p5", mmcode);
            }

            sql += " order by APPTIME";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCM>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public DataTable GetExcel(string APPTIME1, string APPTIME2, string mclass, string flowid, string mmcode)
        {
            var p = new DynamicParameters();

            var sql = @"select A.DOCNO as 單據號碼, A.APPTIME as 申請日期, CASE WHEN A.DOCTYPE='AJ1' then '其他調帳' When A.DOCTYPE='TR1' then '調撥至其他衛星庫' When A.DOCTYPE='RN1' then '繳回中央庫房' end as 出貨種類 ,WH_NAME(A.TOWH) as 入庫房, (select MAT_CLASS || ' ' || MAT_CLSNAME from MI_MATCLASS where MAT_CLASS = A.MAT_CLASS) as 物料分類 , B.MMCODE as 院內碼,
                        C.mmname_C || C.mmname_E as 品名 , B.APPQTY as 出貨數量, (select base_unit from mi_mast where mi_mast.mmcode=B.MMCODE) as 單位 , C.M_CONTPRICE as 平均單價
                        FROM ME_DOCM A, ME_DOCD B, MI_MAST C WHERE A.DOCNO= B.DOCNO AND B.MMCODE = C.MMCODE AND A.FRWH= WHNO_MM1 and (A.FLOWID = '3' or A.FLOWID ='0299') ";



            if (APPTIME1 != "" && APPTIME1 != null) //日期
            {
                sql += "AND to_char(A.APPTIME,'yyyy-mm-dd') >= Substr(:d0, 1, 10) AND SUBSTR(TWN_DATE(A.APPTIME),1,5) is NOT NULL ";
                p.Add(":d0", APPTIME1);
            }
            if (APPTIME2 != "" && APPTIME2 != null)
            {
                sql += "AND to_char(A.APPTIME,'yyyy-mm-dd') <= Substr(:d1, 1, 10) ";
                p.Add(":d1", APPTIME2);
            }

            if (mclass != "" && mclass != null)  //物料分類
            {
                sql += "AND A.MAT_CLASS IN :MAT_CLASS ";
                p.Add(":MAT_CLASS", mclass);
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
                sql += " AND A.DOCTYPE in (" + rde + ")";

            }
            else
            {
                sql += " AND A.DOCTYPE in ('RN1','TR1','AJ1') ";
            }

            if (mmcode != "" && mmcode != "null")
            {
                sql += " AND B.mmcode = :p5 ";
                p.Add(":p5", mmcode);
            }

            sql += " order by APPTIME";



            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<ME_DOCM> GetReport(string apptime_bg, string apptime_ed, string mclass, string flowid, string mmcode)
        {
            var p = new DynamicParameters();

            var sql = @"select A.DOCNO, A.APPTIME, CASE WHEN A.DOCTYPE='AJ1' then '其他調帳' When A.DOCTYPE='TR1' then '調撥至其他衛星庫' When A.DOCTYPE='RN1' then '繳回中央庫房' end as DOCTYPE ,WH_NAME(A.TOWH) as TOWH, (select MAT_CLASS || ' ' || MAT_CLSNAME from MI_MATCLASS where MAT_CLASS = A.MAT_CLASS) as MAT_CLASS , B.MMCODE,
                        C.mmname_C || C.mmname_E as MMNAME_E , B.APPQTY, (select base_unit from mi_mast where mi_mast.mmcode=B.MMCODE) as base_unit , C.M_CONTPRICE
                        FROM ME_DOCM A, ME_DOCD B, MI_MAST C WHERE A.DOCNO= B.DOCNO AND B.MMCODE = C.MMCODE AND A.FRWH= WHNO_MM1 and (A.FLOWID = '3' or A.FLOWID ='0299') ";

            if (apptime_bg != "")
            {
                sql += " AND TWN_DATE(A.APPTIME) >= Substr(:p1, 1, 10) ";
                p.Add(":p1", apptime_bg);
            }
            if (apptime_ed != "")
            {
                sql += " AND TWN_DATE(A.APPTIME) <= Substr(:p2, 1, 10) ";
                p.Add(":p2", apptime_ed);
                //p.Add(":p6")
            }
            if (mclass != "" && mclass != null)
            {
                sql += " AND A.MAT_CLASS = :p3 ";
                p.Add(":p3", mclass);
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
                sql += " AND A.DOCTYPE in (" + rde + ")";

            }
            else
            {
                sql += " AND A.DOCTYPE in ('RN1','TR1','AJ1') ";
            }
            if (mmcode != "" && mmcode != "null")
            {
                sql += " AND B.mmcode = :p5 ";
                p.Add(":p5", mmcode);
            }

            sql += " order by APPTIME";

            return DBWork.Connection.Query<ME_DOCM>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<ComboModel> GetMatClass(string userId)
        {
            string sql = @"SELECT DISTINCT MAT_CLASS as VALUE, " +
                "MAT_CLASS || ' '|| MAT_CLSNAME as COMBITEM from MI_MATCLASS WHERE MAT_CLSID IN ('1','2','3')";
            return DBWork.Connection.Query<ComboModel>(sql, new { USRID = userId }, DBWork.Transaction);
        }
    }
}