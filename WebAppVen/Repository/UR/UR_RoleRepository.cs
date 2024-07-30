using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;
using JCLib.DB;
using Dapper;
using WebAppVen.Models;

namespace WebAppVen.Repository.UR
{
    public class UR_RoleRepository : JCLib.Mvc.BaseRepository
    {
        public UR_RoleRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public bool CheckExists(UR_ROLE ur_role)
        {
            string sql = @"SELECT 1 FROM UR_ROLE WHERE RLNO=@RLNO";
            return !(DBWork.Connection.ExecuteScalar(sql, ur_role, DBWork.Transaction) == null);
        }

        public int Create(UR_ROLE ur_role)
        {
            var sql = @"INSERT INTO UR_ROLE (RLNO,RLNA,RLDESC,ROLE_CREATE_BY)  VALUES (@RLNO,@RLNA,@RLDESC,@ROLE_CREATE_BY)";
            return DBWork.Connection.Execute(sql, ur_role, DBWork.Transaction);
        }

        public int Update(UR_ROLE ur_role)
        {
            string sql = @"UPDATE UR_ROLE SET RLNA=@RLNA,RLDESC=@RLDESC, 
                            ROLE_MODIFY_DATE=Getdate(),
                            ROLE_MODIFY_BY=@ROLE_MODIFY_BY
                            WHERE RLNO=@RLNO";
            return DBWork.Connection.Execute(sql, ur_role, DBWork.Transaction);
        }

        public int DeleteD(string id)
        {
            var sql = @"DELETE FROM UR_ROLE WHERE RLNO =@RLNO";
            return DBWork.Connection.Execute(sql, new { RLNO = id }, DBWork.Transaction);
        }

        public IEnumerable<UR_ROLE> Get(string id)
        {
            string sql = @"SELECT RLNO,RLNA,RLDESC,
                        CONVERT(VARCHAR, ROLE_CREATE_DATE, 120) ROLE_CREATE_DATE,
                        (SELECT UNA FROM UR_ID WHERE TUSER = ROLE_CREATE_BY) ROLE_CREATE_BY,
                        CONVERT(VARCHAR, ROLE_MODIFY_DATE, 120) ROLE_MODIFY_DATE,
                        (SELECT UNA FROM UR_ID WHERE TUSER = ROLE_MODIFY_BY) ROLE_MODIFY_BY 
                        FROM UR_ROLE WHERE RLNO = @RLNO ";
            return DBWork.Connection.Query<UR_ROLE>(sql, new { RLNO = id }, DBWork.Transaction);
        }

        public IEnumerable<UR_ROLE> GetAllD(int page_index, int page_size, string rlno, string rlna)
        {
            var p = new DynamicParameters();

            string sql = @"SELECT RLNO,RLNA,RLDESC,
                        CONVERT(VARCHAR, ROLE_CREATE_DATE, 120) ROLE_CREATE_DATE,
                        (SELECT UNA FROM UR_ID WHERE TUSER = ROLE_CREATE_BY) ROLE_CREATE_BY,
                        CONVERT(VARCHAR, ROLE_MODIFY_DATE, 120) ROLE_MODIFY_DATE,
                        (SELECT UNA FROM UR_ID WHERE TUSER = ROLE_MODIFY_BY) ROLE_MODIFY_BY 
                        FROM UR_ROLE WHERE 1=1 ";
            if (rlno != "")
            {
                sql += " AND RLNO LIKE @p0 ";
                p.Add("@p0", string.Format("%{0}%", rlno));
            }
            if (rlna != "")
            {
                sql += " AND RLNA LIKE @p1 ";
                p.Add("@p1", string.Format("%{0}%", rlna));
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<UR_ROLE>(GetPagingStatement(sql), p, DBWork.Transaction);

        }
    }
}