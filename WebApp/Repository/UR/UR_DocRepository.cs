using System.Data;
using JCLib.DB;
using Dapper;
using System.Collections.Generic;
using WebApp.Models;

namespace WebApp.Repository.UR
{
    public class UR_DocRepository : JCLib.Mvc.BaseRepository
    {
        public UR_DocRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public bool CheckExists(string id)
        {
            string sql = @"SELECT 1 FROM UR_DOC WHERE DK=:DK";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DK = id }, DBWork.Transaction) == null);
        }

        public int Create(UR_DOC ur_doc)
        {
            var sql = @"INSERT INTO UR_DOC (DK, DN, DD, UK, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                                VALUES (:DK, :DN, :DD, :UK, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, ur_doc, DBWork.Transaction);
        }

        public int Update(UR_DOC ur_doc)
        {
            var sql = @"UPDATE UR_DOC SET DN = :DN, DD = :DD, UK = :UK,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP 
                        WHERE DK = :DK";
            return DBWork.Connection.Execute(sql, ur_doc, DBWork.Transaction);
        }

        public int Delete(UR_DOC ur_doc)
        {
            var sql = @"DELETE FROM UR_DOC WHERE DK = :DK";
            return DBWork.Connection.Execute(sql, ur_doc, DBWork.Transaction);
        }

        public IEnumerable<UR_DOC> GetAll(string dk, string dn, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT DK, DN, DD, UK, 
                CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP 
                FROM UR_DOC WHERE 1=1 ";

            if (dk != "")
            {
                sql += " AND DK LIKE :p0 ";
                p.Add(":p0", string.Format("{0}%", dk));
            }
            if (dn != "")
            {
                sql += " AND DN LIKE :p1 ";
                p.Add(":p1", string.Format("%{0}%", dn));
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<UR_DOC>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<UR_DOC> Get(string id)
        {
            var sql = @"SELECT DK, DN, DD, UK, 
                CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP 
                FROM UR_DOC WHERE DK = :DK";
            return DBWork.Connection.Query<UR_DOC>(sql, new { DK = id }, DBWork.Transaction);
        }

        public object GetUploadKey(string dk)
        {
            var sql = @"SELECT UK FROM UR_DOC WHERE DK =:DK";
            return DBWork.Connection.ExecuteScalar(sql, new { DK = dk }, DBWork.Transaction);
        }
    }
}