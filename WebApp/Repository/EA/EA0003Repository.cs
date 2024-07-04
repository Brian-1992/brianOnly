using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JCLib.DB;
using Dapper;
using WebApp.Models;

namespace WebApp.Repository.EA
{
    public class EA0003Repository : JCLib.Mvc.BaseRepository
    {
        public EA0003Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<Object> GetSeriesData(string inid, string mmCode)
        {
            var p = new DynamicParameters();
            var sql = "";
            if (string.IsNullOrEmpty(mmCode))
            {
                sql = @"SELECT '' MMNAME,
                    (SELECT SUM(ICOST) FROM V_BI_INVCOST3 WHERE MAT_CLSID='1' AND INID=:INID) ICOST_1,
                    (SELECT SUM(UCOST) FROM V_BI_INVCOST3 WHERE MAT_CLSID='1' AND INID=:INID) UCOST_1,
                    (SELECT SUM(ICOST) FROM V_BI_INVCOST3 WHERE MAT_CLSID='2' AND INID=:INID) ICOST_2,
                    (SELECT SUM(UCOST) FROM V_BI_INVCOST3 WHERE MAT_CLSID='2' AND INID=:INID) UCOST_2,
                    (SELECT SUM(ICOST) FROM V_BI_INVCOST3 WHERE MAT_CLSID='3' AND INID=:INID) ICOST_3,
                    (SELECT SUM(UCOST) FROM V_BI_INVCOST3 WHERE MAT_CLSID='3' AND INID=:INID) UCOST_3
                     FROM DUAL";

                p.Add("INID", inid);
            }
            else
            {
                sql = @"SELECT 
                    (SELECT MMNAME_C FROM MI_MAST WHERE MMCODE=:MMCODE) MMNAME,
                    (SELECT SUM(ICOST) FROM V_BI_INVCOST3 WHERE MAT_CLSID='1' AND INID=:INID AND MMCODE=:MMCODE) ICOST_1,
                    (SELECT SUM(UCOST) FROM V_BI_INVCOST3 WHERE MAT_CLSID='1' AND INID=:INID AND MMCODE=:MMCODE) UCOST_1,
                    (SELECT SUM(ICOST) FROM V_BI_INVCOST3 WHERE MAT_CLSID='2' AND INID=:INID AND MMCODE=:MMCODE) ICOST_2,
                    (SELECT SUM(UCOST) FROM V_BI_INVCOST3 WHERE MAT_CLSID='2' AND INID=:INID AND MMCODE=:MMCODE) UCOST_2,
                    (SELECT SUM(ICOST) FROM V_BI_INVCOST3 WHERE MAT_CLSID='3' AND INID=:INID AND MMCODE=:MMCODE) ICOST_3,
                    (SELECT SUM(UCOST) FROM V_BI_INVCOST3 WHERE MAT_CLSID='3' AND INID=:INID AND MMCODE=:MMCODE) UCOST_3
                     FROM DUAL";

                p.Add("INID", inid);
                p.Add("MMCODE", mmCode);
            }

            return DBWork.Connection.Query(sql, p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetWhnos() {
            string sql = @"
                select inid as value, inid_name as text, rownum as extra1
                  from UR_INID a 
                 where inid=wh_inid(whno_mm1)
                union
                select inid as value, inid_name as text, rownum+1 as extra1 from UR_INID a 
                 where exists (select 1 from MI_WHMAST where wh_kind='0' and wh_grade='2' and inid=a.inid)
                order by extra1
            ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, DBWork.Transaction);
        }
    }
}