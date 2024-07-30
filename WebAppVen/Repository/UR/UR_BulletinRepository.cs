using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JCLib.DB;
using Dapper;
using WebAppVen.Models;

namespace WebAppVen.Repository.UR
{
    public class UR_BulletinRepository : JCLib.Mvc.BaseRepository
    {
        public UR_BulletinRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public int Create(UR_BULLETIN bulletin)
        {
            string id_sql = "SELECT ISNULL(MAX(ID+1), 0) NEW_ID FROM UR_BULLETIN";
            var objID = DBWork.Connection.ExecuteScalar(id_sql, null, DBWork.Transaction);
            bulletin.ID = objID.ToString();

            string sql = @"INSERT INTO UR_BULLETIN (ID, TITLE, CONTENT, TARGET, 
                           ON_DATE, OFF_DATE, VALID, CREATE_BY, CREATE_DT)  
                           VALUES (@ID, @TITLE, @CONTENT, @TARGET, 
                           @ON_DATE, @OFF_DATE, @VALID, @CREATE_BY, @CREATE_DT)";
            return DBWork.Connection.Execute(sql, bulletin, DBWork.Transaction);
        }

        public int Update(UR_BULLETIN param_m)
        {
            string sql = @"UPDATE UR_BULLETIN 
                           SET TITLE=@TITLE, CONTENT=@CONTENT, TARGET=@TARGET, 
                           ON_DATE=@ON_DATE, OFF_DATE=@OFF_DATE, VALID=@VALID, 
                           UPDATE_BY=@UPDATE_BY, UPDATE_DT=@UPDATE_DT  
                           WHERE ID=@ID";
            return DBWork.Connection.Execute(sql, param_m, DBWork.Transaction);
        }

        public int Delete(string id)
        {
            string sql = @"DELETE FROM UR_BULLETIN WHERE ID=@ID";
            return DBWork.Connection.Execute(sql, new { ID = id }, DBWork.Transaction);
        }

        public IEnumerable<UR_BULLETIN> Get(string id)
        {
            string sql = @"SELECT ID, TITLE, CONTENT, TARGET, VALID, 
                        CONVERT(VARCHAR, ON_DATE, 111) ON_DATE, CONVERT(VARCHAR, OFF_DATE, 111) OFF_DATE, 
                        (SELECT UNA FROM UR_ID WHERE TUSER=A.CREATE_BY) CREATE_BY, CREATE_DT, 
                        (SELECT UNA FROM UR_ID WHERE TUSER=A.UPDATE_BY) UPDATE_BY, UPDATE_DT, 
                        (SELECT COUNT(1) FROM UR_UPLOAD WHERE UK=('URBLT,'+CAST(A.ID AS VARCHAR))) ATTACH_CNT 
                        FROM UR_BULLETIN A WHERE ID=@ID";
            return DBWork.Connection.Query<UR_BULLETIN>(sql, new { ID = id }, DBWork.Transaction);
        }

        public IEnumerable<UR_BULLETIN> Query(int page_index, int page_size, string sorters, string id, string name)
        {
            DynamicParameters p = new DynamicParameters();

            string sql = @"SELECT ID, TITLE, CONTENT, TARGET, VALID, 
                        CONVERT(VARCHAR, ON_DATE, 111) ON_DATE, CONVERT(VARCHAR, OFF_DATE, 111) OFF_DATE, 
                        (SELECT UNA FROM UR_ID WHERE TUSER=A.CREATE_BY) CREATE_BY, CREATE_DT, 
                        (SELECT UNA FROM UR_ID WHERE TUSER=A.UPDATE_BY) UPDATE_BY, UPDATE_DT, 
                        (SELECT COUNT(1) FROM UR_UPLOAD WHERE UK=('URBLT,'+CAST(A.ID AS VARCHAR))) ATTACH_CNT 
                        FROM UR_BULLETIN A WHERE 1=1 ";
            if (id != "")
            {
                sql += " AND TITLE LIKE @p0 ";
                p.Add("@p0", string.Format("%{0}%", id));
            }
            if (name != "")
            {
                sql += " AND CONTENT LIKE @p1 ";
                p.Add("@p1", string.Format("%{0}%", name));
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<UR_BULLETIN>(GetPagingStatement(sql, sorters), p);
        }

        public IEnumerable<UR_BULLETIN> Show(int page_index, int page_size, string sorters, string id, string name, string uid)
        {
            DynamicParameters p = new DynamicParameters();

            string sql = @"SELECT ID, TITLE, CONTENT, TARGET, VALID, 
                        CONVERT(VARCHAR, ON_DATE, 111) ON_DATE, CONVERT(VARCHAR, OFF_DATE, 111) OFF_DATE, 
                        (SELECT UNA FROM UR_ID WHERE TUSER=A.CREATE_BY) CREATE_BY, CREATE_DT, 
                        (SELECT UNA FROM UR_ID WHERE TUSER=A.UPDATE_BY) UPDATE_BY, UPDATE_DT, 
                        (SELECT COUNT(1) FROM UR_UPLOAD WHERE UK=('URBLT,'+CAST(A.ID AS VARCHAR))) ATTACH_CNT 
                        FROM UR_BULLETIN A WHERE VALID='1' 
                        AND (ON_DATE IS NULL OR GETDATE() > ON_DATE) AND (ON_DATE IS NULL OR GETDATE() < OFF_DATE)";
            p.Add("@TUSER", uid);
            if (id != "")
            {
                sql += " AND TITLE LIKE @p0 ";
                p.Add("@p0", string.Format("%{0}%", id));
            }
            if (name != "")
            {
                sql += " AND CONTENT LIKE @p1 ";
                p.Add("@p1", string.Format("%{0}%", name));
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<UR_BULLETIN>(GetPagingStatement(sql, sorters), p);
        }
    }
}