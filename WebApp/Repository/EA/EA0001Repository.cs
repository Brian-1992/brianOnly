using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JCLib.DB;
using Dapper;
using WebApp.Models;

namespace WebApp.Repository.EA
{
    public class EA0001Repository : JCLib.Mvc.BaseRepository
    {
        public EA0001Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<EA0001Chart> GetChartData(string wh_no, int perg)
        {
            var sql = @"SELECT MAT_CLSID, PERG_TYPE, COUNT(1) C FROM (
                SELECT MAT_CLSID, CASE WHEN PERG < 0 THEN 2 WHEN PERG >= 0 AND PERG < :PERG THEN 1 WHEN PERG >= :PERG THEN 0 END PERG_TYPE 
                FROM V_BI_INVCOST1 WHERE WH_NO=:WH_NO
                ) V GROUP BY MAT_CLSID, PERG_TYPE
                ORDER BY MAT_CLSID, PERG_TYPE";

            return DBWork.Connection.Query<EA0001Chart>(sql, new { WH_NO = wh_no, PERG = perg }, DBWork.Transaction);
        }
        public IEnumerable<EA0001DataRow> GetTableData(string wh_no, int mat_clsid, int perg, int perg_type)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT V.wh_no, V.MAT_CLSID, V.MMCODE, V.INV_QTY, V.SAFE_QTY, V.PERG, V.PERG_TYPE,
                               --(SELECT MMNAME_E FROM MI_MAST WHERE MMCODE = V.MMCODE) MMNAME_E 
                               (SELECT (case when MAT_CLASS='01' then MMNAME_E
                                             else MMNAME_C
                                        end)     
                                  FROM MI_MAST 
                                 WHERE MMCODE = V.MMCODE) MMNAME_E
                        FROM 
                          (SELECT VBI.*, 
                                  (CASE WHEN PERG < 0 THEN 2 
                                        WHEN PERG >= 0 AND PERG < :PERG THEN 1 
                                        WHEN PERG >= :PERG THEN 0
                                   END) PERG_TYPE 
                             FROM V_BI_INVCOST1 VBI 
                            WHERE WH_NO=:WH_NO {0} {1} ORDER BY PERG {2}
                          ) V
                        WHERE ROWNUM < 11";

            p.Add("PERG", perg);
            p.Add("WH_NO", wh_no);

            string sqlMatClsid = "";
            if (mat_clsid > 0)
            {
                sqlMatClsid = " AND MAT_CLSID =:MAT_CLSID ";
                p.Add("MAT_CLSID", mat_clsid);
            }

            string sqlOrder = "";
            string sqlPerg = "";

            switch (perg_type)
            {
                case 0:
                    //大於等於安全存量 + perg%
                    sqlPerg = " AND PERG > :PERG ";
                    sqlOrder = "DESC";
                    p.Add("PERG", perg);
                    break;
                case 1:
                    //接近安全存量
                    sqlPerg = " AND PERG >= 0 AND PERG < :PERG ";
                    p.Add("PERG", perg);
                    break;
                case 2:
                    //小於安全存量
                    sqlPerg = " AND PERG < 0 ";
                    break;
            }

            sql = string.Format(sql, sqlMatClsid, sqlPerg, sqlOrder);

            return DBWork.Connection.Query<EA0001DataRow>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetWhnos() {
            string sql = @"
                select 1 extra1, whno_mm1 as value, wh_name(whno_mm1) as text from dual
                union
                select 2 extra1, whno_me1 as value, wh_name(whno_me1) as text from dual
                union
                select rownum+2 as extra1,  wh_no as value, wh_name as text from MI_WHMAST where wh_kind='0' and wh_grade='2'
            ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, DBWork.Transaction);
        }
    }

    public class EA0001Chart
    {
        public int MAT_CLSID;
        public int PERG_TYPE;
        public int C;
    }

    public class EA0001DataRow
    {
        public int MAT_CLSID;
        public string MMCODE;
        public string MMNAME_E;
        public int INV_QTY;
        public int SAFE_QTY;
        public int PERG;
        public int PERG_TYPE;
    }

}