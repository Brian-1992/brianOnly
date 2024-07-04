using System;
using System.Collections.Generic;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Text;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;
using TSGH.Models;

namespace WebApp.Repository.AA
{
    public class AB0063Repository : JCLib.Mvc.BaseRepository
    {
        public AB0063Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        // AA0072Repository的GetSqlstr和GetApplyData有取WH_NO全部選項做條件的處理,如這邊的規則有調整,另外兩個地方也需要連帶做調整
        public IEnumerable<MI_WHMAST> GetWH(string vtpe)
        {
            string sql = @"SELECT DISTINCT WH_NO, (SELECT WH_NAME FROM MI_WHMAST WHERE WH_NO = A.WH_NO) WH_NAME " +
                        " FROM MI_WHID A " +
                        " WHERE WH_USERID = '" + DBWork.UserInfo.UserId + "' " +
                        " and (select 1 from MI_WHMAST where WH_NO=A.WH_NO and WH_KIND='1')=1 " +
                        " UNION " +
                        " SELECT DISTINCT WH_NO, WH_NAME " +
                        " FROM MI_WHMAST " +
                        " WHERE INID = USER_INID('" + DBWork.UserInfo.UserId + "') AND WH_KIND = '1' ";
            if (vtpe == "Y")
            {
                sql += " UNION  SELECT 'ALL_A' AS WH_NO,'台北門診全部' AS WH_NAME FROM DUAL ";
            }

            sql += " ORDER BY 1 ";
            return DBWork.Connection.Query<MI_WHMAST>(sql, new { WH_USERID = DBWork.UserInfo.UserId }, DBWork.Transaction);
        }
        public IEnumerable<COMBO_MODEL> GetMatClassCombo(string pWhNo)
        {
            var p = new DynamicParameters();
            string sql = string.Empty;

            if (pWhNo == "ALL_A" || pWhNo == "全部" || pWhNo == "")
            {
                sql = @" select MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM 
                                    from MI_MATCLASS where mat_clsid in ('2', '3', '6')";
            }
            else
            {
                sql = @"SELECT MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, " +
                       "MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM " +
                       "FROM MI_MATCLASS " +
                         "WHERE MAT_CLSID in ('2', '3', '6') ";

                p.Add(":WHNO", pWhNo);
            }
            return DBWork.Connection.Query<COMBO_MODEL>(sql, p, DBWork.Transaction);
        }
        public IEnumerable<MI_MAST> GetMmCodeCombo(string p0, int page_index, int page_size, string sorters)
        {

            var p = new DynamicParameters();

            var sql = @"
                        SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E
                          FROM MI_MAST A 
                         WHERE 1=1 
                           AND nvl(A.CANCEL_ID, 'N') <> 'Y' ";
            sql += " {1} ";
            if (p0 != "")
            {
                sql = string.Format(sql,
                     "(NVL(INSTR(UPPER(A.MMCODE), UPPER(:MMCODE_I)), 1000) + NVL(INSTR(UPPER(MMNAME_E), UPPER(:MMNAME_E_I)), 100) * 10 + NVL(INSTR(UPPER(MMNAME_C), UPPER(:MMNAME_C_I)), 100) * 10) IDX,",
                     @"   AND (UPPER(A.MMCODE) LIKE UPPER(:MMCODE) OR UPPER(MMNAME_E) LIKE UPPER(:MMNAME_E) OR UPPER(MMNAME_C) LIKE UPPER(:MMNAME_C))");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);
                p.Add(":MMCODE", string.Format("{0}%", p0));
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX, mmcode", sql);
            }
            else
            {
                sql = string.Format(sql, "", "");
                sql += " ORDER BY MMCODE ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public string GetUridInid(string id)
        {
            string sql = @"SELECT INID FROM UR_ID WHERE TUSER=:TUSER ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { TUSER = id }, DBWork.Transaction).ToString();
            return rtn;
        }
        public bool Checkwhno(string id)
        {
            string sql = @"SELECT 1 FROM PARAM_D WHERE GRP_CODE='AB0063' AND DATA_NAME='WH_NO' AND DATA_VALUE=:DATA_VALUE ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DATA_VALUE = id }, DBWork.Transaction) == null);
        }
    }
}