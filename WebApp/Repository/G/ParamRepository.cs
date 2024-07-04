using System.Collections.Generic;
using JCLib.DB;
using Dapper;
using WebApp.Models;

namespace WebApp.Repository.GB
{
    public class ParamRepository : JCLib.Mvc.BaseRepository
    {
        public ParamRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        //=== PARAM_M ===
        public bool CheckExists(PARAM_M param_m)
        {
            string sql = @"SELECT 1 FROM PARAM_M WHERE GRP_CODE=:GRP_CODE";
            return !(DBWork.Connection.ExecuteScalar(sql, param_m, DBWork.Transaction) == null);
        }

        public int Create(PARAM_M param_m)
        {
            string sql = @"INSERT INTO PARAM_M (GRP_CODE, GRP_DESC, GRP_USE)  
                           VALUES (:GRP_CODE, :GRP_DESC, :GRP_USE)";
            return DBWork.Connection.Execute(sql, param_m, DBWork.Transaction);
        }

        public int Update(PARAM_M param_m)
        {
            string sql = @"UPDATE PARAM_M 
                           SET GRP_DESC=:GRP_DESC, GRP_USE=:GRP_USE 
                           WHERE GRP_CODE=:GRP_CODE";
            return DBWork.Connection.Execute(sql, param_m, DBWork.Transaction);
        }

        public int Delete(string id)
        {
            string sql = @"DELETE FROM PARAM_M WHERE GRP_CODE=:GRP_CODE";
            return DBWork.Connection.Execute(sql, new { GRP_CODE = id }, DBWork.Transaction);
        }

        public IEnumerable<PARAM_M> Get(string id)
        {
            string sql = @"SELECT GRP_DESC, GRP_USE   
                            FROM PARAM_M WHERE GRP_CODE=:GRP_CODE";
            return DBWork.Connection.Query<PARAM_M>(sql, new { GRP_CODE = id }, DBWork.Transaction);
        }

        public IEnumerable<PARAM_M> Query(int page_index, int page_size, string sorters, string id, string name)
        {
            DynamicParameters p = new DynamicParameters();

            string sql = @"SELECT GRP_CODE, GRP_DESC, GRP_USE  
                        FROM PARAM_M WHERE 1=1 ";
            if (id != "")
            {
                sql += " AND GRP_CODE LIKE :p0 ";
                p.Add(":p0", string.Format("%{0}%", id));
            }
            if (name != "")
            {
                sql += " AND GRP_DESC LIKE :p1 ";
                p.Add(":p1", string.Format("%{0}%", name));
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<PARAM_M>(GetPagingStatement(sql, sorters), p);
        }

        //=== PARAM_D ===
        public bool CheckExists(PARAM_D param_d)
        {
            string sql = @"SELECT 1 FROM PARAM_D WHERE GRP_CODE=:GRP_CODE AND DATA_SEQ=:DATA_SEQ";
            return !(DBWork.Connection.ExecuteScalar(sql, param_d, DBWork.Transaction) == null);
        }

        public IEnumerable<PARAM_D> ListD(string m_id)
        {
            string sql = @"SELECT GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC, DATA_REMARK FROM PARAM_D WHERE M_ID=:M_ID";
            return DBWork.Connection.Query<PARAM_D>(sql, new { GRP_CODE = m_id }, DBWork.Transaction);
        }

        public int CreateD(PARAM_D param_d)
        {
            /*
            string id_sql = "SELECT ISNULL(MAX(DATA_SEQ+1), 1) NEW_ID FROM PARAM_D WHERE GRP_CODE=@GRP_CODE";
            var objID = DBWork.Connection.ExecuteScalar(id_sql, param_d, DBWork.Transaction);
            param_d.DATA_SEQ = int.Parse(objID.ToString());
            */

            string sql = @"INSERT INTO PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC, DATA_REMARK)  
                                VALUES (:GRP_CODE, :DATA_SEQ, :DATA_NAME, :DATA_VALUE, :DATA_DESC, :DATA_REMARK)";
            return DBWork.Connection.Execute(sql, param_d, DBWork.Transaction);
        }

        public int UpdateD(PARAM_D param_d)
        {
            string sql = @"UPDATE PARAM_D 
                            SET DATA_SEQ=:DATA_SEQ, DATA_NAME=:DATA_NAME, DATA_VALUE=:DATA_VALUE, 
                            DATA_DESC=:DATA_DESC, DATA_REMARK=:DATA_REMARK  
                            WHERE GRP_CODE=:GRP_CODE AND DATA_SEQ=:DATA_SEQ_O";
            return DBWork.Connection.Execute(sql, param_d, DBWork.Transaction);
        }

        public int DeleteD(string id)
        {
            string sql = @"DELETE FROM PARAM_D WHERE GRP_CODE=:GRP_CODE";
            return DBWork.Connection.Execute(sql, new { GRP_CODE = id }, DBWork.Transaction);
        }

        public int DeleteD(PARAM_D param_d)
        {
            string sql = @"DELETE FROM PARAM_D WHERE GRP_CODE=:GRP_CODE AND DATA_SEQ=:DATA_SEQ";
            return DBWork.Connection.Execute(sql, param_d, DBWork.Transaction);
        }

        public IEnumerable<PARAM_D> GetD(PARAM_D param_d)
        {
            string sql = @"SELECT GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC, DATA_REMARK 
                            FROM PARAM_D WHERE GRP_CODE=:GRP_CODE AND DATA_SEQ=:DATA_SEQ";
            return DBWork.Connection.Query<PARAM_D>(sql, param_d, DBWork.Transaction);
        }

        public IEnumerable<PARAM_D> QueryD(int page_index, int page_size, string sorters, string f1, string d1, string d2)
        {
            DynamicParameters p = new DynamicParameters();
            string sql = @"SELECT GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC, DATA_REMARK  
                        FROM PARAM_D WHERE GRP_CODE=:p0 ";
            p.Add(":p0", f1);
            if (d1 != "")
            {
                sql += " AND DATA_NAME LIKE :p1 ";
                p.Add(":p1", string.Format("%{0}%", d1));
            }
            if (d2 != "")
            {
                sql += " AND DATA_VALUE LIKE :p2 ";
                p.Add(":p2", string.Format("%{0}%", d2));
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<PARAM_D>(GetPagingStatement(sql, sorters), p);
        }
        
        //=== COMBO ===

        public IEnumerable<ComboItemModel> GetCombo(string grp_code, string data_name)
        {
            //System.Web.Security.FormsAuthentication.SignOut();

            DynamicParameters p = new DynamicParameters();
            string sql = @"SELECT DATA_VALUE AS VALUE, DATA_VALUE||' '||DATA_DESC AS TEXT 
                        FROM PARAM_D WHERE GRP_CODE=:GRP_CODE ";
            p.Add(":GRP_CODE", grp_code);
            if (data_name != "")
            {
                //sql += " AND DATA_NAME LIKE :DATA_NAME ";
                //p.Add(":DATA_NAME", string.Format("%{0}%", data_name));
                sql += " AND DATA_NAME=:DATA_NAME ";
                p.Add(":DATA_NAME", data_name);
            }

            sql += " ORDER BY DATA_SEQ ";

            return DBWork.Connection.Query<ComboItemModel>(sql, p);
        }

        public bool GetVIEWALL(string id)
        {
            var sql = @"SELECT 1 FROM PARAM_D WHERE GRP_CODE='VIEWALL' AND DATA_VALUE IN (SELECT RLNO FROM UR_UIR WHERE lower(TUSER)=lower(:TUSER))";
            var result = DBWork.Connection.ExecuteScalar(sql, new { TUSER = id }, DBWork.Transaction);

            return (result != null);
        }

    }
}