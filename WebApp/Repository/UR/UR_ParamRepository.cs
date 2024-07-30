using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JCLib.DB;
using Dapper;
using WebApp.Models;

namespace WebApp.Repository.UR
{
    public class UR_ParamRepository : JCLib.Mvc.BaseRepository
    {
        public UR_ParamRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        //=== UR_PARAM_M ===

        public int Create(UR_PARAM_M param_m)
        {
            string sql = @"INSERT INTO UR_PARAM_M (ID, NAME)  
                           VALUES (@ID, @NAME)";
            return DBWork.Connection.Execute(sql, param_m, DBWork.Transaction);
        }

        public int Update(UR_PARAM_M param_m)
        {
            string sql = @"UPDATE UR_PARAM_M 
                           SET NAME=@NAME 
                           WHERE ID=@ID";
            return DBWork.Connection.Execute(sql, param_m, DBWork.Transaction);
        }

        public int Delete(string id)
        {
            string sql = @"DELETE FROM UR_PARAM_M WHERE ID=@ID";
            return DBWork.Connection.Execute(sql, new { ID = id }, DBWork.Transaction);
        }

        public IEnumerable<UR_PARAM_M> Get(string id)
        {
            string sql = @"SELECT ID, NAME   
                            FROM UR_PARAM_M WHERE ID=@ID";
            return DBWork.Connection.Query<UR_PARAM_M>(sql, new { ID = id }, DBWork.Transaction);
        }

        public IEnumerable<UR_PARAM_M> Query(int page_index, int page_size, string sorters, string id, string name)
        {
            DynamicParameters p = new DynamicParameters();

            string sql = @"SELECT ID, NAME   
                        FROM UR_PARAM_M WHERE 1=1 ";
            if (id != "")
            {
                sql += " AND ID LIKE @p0 ";
                p.Add("@p0", string.Format("%{0}%", id));
            }
            if (name != "")
            {
                sql += " AND NAME LIKE @p1 ";
                p.Add("@p1", string.Format("%{0}%", name));
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<UR_PARAM_M>(GetPagingStatement(sql, sorters), p);
        }

        //=== UR_PARAM_D ===

        public IEnumerable<UR_PARAM_D> ListD(string m_id)
        {
            string sql = @"SELECT M_ID, ID, NAME, VALUE FROM UR_PARAM_D WHERE M_ID=@M_ID";
            return DBWork.Connection.Query<UR_PARAM_D>(sql, new { M_ID = m_id }, DBWork.Transaction);
        }

        public int CreateD(UR_PARAM_D param_d)
        {
            string sql = @"INSERT INTO UR_PARAM_D (M_ID, ID, NAME, VALUE)  
                                VALUES (@M_ID, @ID, @NAME, @VALUE)";
            return DBWork.Connection.Execute(sql, param_d, DBWork.Transaction);
        }

        public int UpdateD(UR_PARAM_D param_d)
        {
            string sql = @"UPDATE UR_PARAM_D 
                            SET NAME=@NAME, VALUE=@VALUE  
                            WHERE M_ID=@M_ID AND ID=@ID";
            return DBWork.Connection.Execute(sql, param_d, DBWork.Transaction);
        }

        public int DeleteD(UR_PARAM_D param_d)
        {
            string sql = @"DELETE FROM UR_PARAM_D WHERE M_ID=@M_ID AND ID=@ID";
            return DBWork.Connection.Execute(sql, param_d, DBWork.Transaction);
        }

        public IEnumerable<UR_PARAM_D> GetD(UR_PARAM_D param_d)
        {
            string sql = @"SELECT M_ID, ID, NAME, VALUE  
                            FROM UR_PARAM_D WHERE M_ID=@M_ID AND ID=@ID";
            return DBWork.Connection.Query<UR_PARAM_D>(sql, new { M_ID = param_d.M_ID, ID = param_d.ID }, DBWork.Transaction);
        }

        public IEnumerable<UR_PARAM_D> QueryD(int page_index, int page_size, string sorters, string f1, string d1, string d2)
        {
            DynamicParameters p = new DynamicParameters();
            string sql = @"SELECT M_ID, ID, NAME, VALUE  
                        FROM UR_PARAM_D WHERE M_ID=@p0 ";
            p.Add("@p0", f1);
            if (d1 != "")
            {
                sql += " AND NAME LIKE @p1 ";
                p.Add("@p1", string.Format("%{0}%", d1));
            }
            if (d2 != "")
            {
                sql += " AND VALUE LIKE @p2 ";
                p.Add("@p2", string.Format("%{0}%", d2));
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<UR_PARAM_D>(GetPagingStatement(sql, sorters), p);
        }
    }
}