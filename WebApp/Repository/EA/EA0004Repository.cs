using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JCLib.DB;
using Dapper;
using WebApp.Models;

namespace WebApp.Repository.EA
{
    public class EA0004Repository : JCLib.Mvc.BaseRepository
    {
        public EA0004Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<Object> GetSeriesData(string inid)
        {
            var p = new DynamicParameters();
            var roc_year = (DateTime.Now.Year - 1911).ToString();
            //roc_year = "108";
            var sb = new System.Text.StringBuilder();
            
            sb.Append("SELECT * FROM (SELECT * FROM V_BI_INVCOST4 WHERE DATA_YM LIKE :DATA_YM) PIVOT (SUM(ICOST) FOR DATA_YM IN (");
            for (var i = 0; i < 12; i++)
            {
                if (i != 0) sb.Append(',');
                sb.AppendFormat("'{0}{1}' as M{1}", roc_year, (i+1).ToString().PadLeft(2, '0'));
            }
            sb.Append(")) WHERE INID = :INID ORDER BY INID, MAT_CLSID");

            p.Add("DATA_YM", roc_year + "%");
            p.Add("INID", inid);

            return DBWork.Connection.Query(sb.ToString(), p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetWhnos()
        {
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

    public class EA0004Chart
    {
        public int MAT_CLSID;
        public int PERG_TYPE;
        public int C;
    }

    public class EA0004DataRow
    {
        public int MAT_CLSID;
        public int ICOST;
    }
}