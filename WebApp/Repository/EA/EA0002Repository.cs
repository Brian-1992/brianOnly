using System;
using System.Collections.Generic;
using JCLib.DB;
using Dapper;
using WebApp.Models;

namespace WebApp.Repository.EA
{
    public class EA0002Repository : JCLib.Mvc.BaseRepository
    {
        public EA0002Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public object GetMmName(string mmCode)
        {
            var p = new DynamicParameters();
            string sql = @"SELECT (case when MAT_CLASS='01' then MMNAME_E
                                         else MMNAME_C
                                    end) as MMNAME_C
                             FROM MI_MAST WHERE MMCODE =:MMCODE";
            p.Add("MMCODE", mmCode);

            return DBWork.Connection.ExecuteScalar(sql, p, DBWork.Transaction)??"";
        }

        public IEnumerable<Object> GetAvgSeriesData(string wh_no, string mmCode)
        {
            var p = new DynamicParameters();
            var roc_year = (DateTime.Now.Year - 1911).ToString();
            //roc_year = "108";
            var sb = new System.Text.StringBuilder();

            if (string.IsNullOrEmpty(mmCode))
            {
                sb.Append("SELECT * FROM ");
                sb.Append("(SELECT DATA_YM,round(SUM(AQTY), 5) QTY from V_BI_INVCOST2A WHERE WH_NO=:WH_NO AND DATA_YM LIKE :DATA_YM GROUP BY DATA_YM ORDER BY DATA_YM) ");
                sb.Append("PIVOT (SUM(QTY) FOR DATA_YM IN (");
                for (var i = 0; i < 12; i++)
                {
                    if (i != 0) sb.Append(',');
                    sb.AppendFormat("'{0}{1}' as M{1}", roc_year, (i + 1).ToString().PadLeft(2, '0'));
                }
                sb.Append("))");

                p.Add("WH_NO", wh_no);
            }
            else
            {
                sb.Append("SELECT * FROM ");
                sb.Append("(SELECT DATA_YM,round(SUM(AQTY), 5) QTY from V_BI_INVCOST2A WHERE WH_NO=:WH_NO AND MMCODE=:MMCODE AND DATA_YM LIKE :DATA_YM GROUP BY DATA_YM ORDER BY DATA_YM) ");
                sb.Append("PIVOT (SUM(QTY) FOR DATA_YM IN (");
                for (var i = 0; i < 12; i++)
                {
                    if (i != 0) sb.Append(',');
                    sb.AppendFormat("'{0}{1}' as M{1}", roc_year, (i + 1).ToString().PadLeft(2, '0'));
                }
                sb.Append("))");

                p.Add("WH_NO", wh_no);
                p.Add("MMCODE", mmCode);
            }
            p.Add("DATA_YM", roc_year + "%");

            return DBWork.Connection.Query(sb.ToString(), p, DBWork.Transaction);
        }

        public IEnumerable<Object> GetCurSeriesData(string wh_no, string mmCode)
        {
            var p = new DynamicParameters();
            var roc_year = (DateTime.Now.Year - 1911).ToString();
            //roc_year = "108";
            var sb = new System.Text.StringBuilder();

            if (string.IsNullOrEmpty(mmCode))
            {
                sb.Append("SELECT * FROM ");
                sb.Append("(SELECT DATA_YM,round(SUM(OQTY), 5) QTY from V_BI_INVCOST2 WHERE WH_NO=:WH_NO AND DATA_YM LIKE :DATA_YM GROUP BY DATA_YM ORDER BY DATA_YM) ");
                sb.Append("PIVOT (SUM(QTY) FOR DATA_YM IN (");
                for (var i = 0; i < 12; i++)
                {
                    if (i != 0) sb.Append(',');
                    sb.AppendFormat("'{0}{1}' as M{1}", roc_year, (i + 1).ToString().PadLeft(2, '0'));
                }
                sb.Append("))");

                p.Add("WH_NO", wh_no);
            }
            else
            {
                sb.Append("SELECT * FROM ");
                sb.Append("(SELECT DATA_YM,round(SUM(OQTY), 5) QTY from V_BI_INVCOST2 WHERE WH_NO=:WH_NO AND MMCODE=:MMCODE AND DATA_YM LIKE :DATA_YM GROUP BY DATA_YM ORDER BY DATA_YM) ");
                sb.Append("PIVOT (SUM(QTY) FOR DATA_YM IN (");
                for (var i = 0; i < 12; i++)
                {
                    if (i != 0) sb.Append(',');
                    sb.AppendFormat("'{0}{1}' as M{1}", roc_year, (i + 1).ToString().PadLeft(2, '0'));
                }
                sb.Append("))");

                p.Add("WH_NO", wh_no);
                p.Add("MMCODE", mmCode);
            }
            p.Add("DATA_YM", roc_year + "%");

          return DBWork.Connection.Query(sb.ToString(), p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetWhnos()
        {
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
}