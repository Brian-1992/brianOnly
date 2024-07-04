using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;
using JCLib.DB;
using Dapper;
using WebApp.Models;

namespace WebApp.Repository.UR
{
    public class UR_RoleRepository : JCLib.Mvc.BaseRepository
    {
        public UR_RoleRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public bool CheckExists(UR_ROLE ur_role)
        {
            string sql = @"SELECT 1 FROM UR_ROLE WHERE RLNO=:RLNO";
            return !(DBWork.Connection.ExecuteScalar(sql, ur_role, DBWork.Transaction) == null);
        }

        public int Create(UR_ROLE ur_role)
        {
            var sql = @"INSERT INTO UR_ROLE (RLNO,RLNA,RLDESC,ROLE_CREATE_BY)  VALUES (:RLNO,:RLNA,:RLDESC,:ROLE_CREATE_BY)";
            return DBWork.Connection.Execute(sql, ur_role, DBWork.Transaction);
        }

        public int Update(UR_ROLE ur_role)
        {
            var sql = @"UPDATE UR_ROLE SET RLNA=:RLNA,RLDESC=:RLDESC, 
                            ROLE_MODIFY_DATE=sysdate,
                            ROLE_MODIFY_BY=:ROLE_MODIFY_BY
                            WHERE RLNO=:RLNO";
            return DBWork.Connection.Execute(sql, ur_role, DBWork.Transaction);
        }

        public int Delete(string rlno)
        {
            var sql = @"DELETE FROM UR_ROLE WHERE RLNO =:RLNO";
            return DBWork.Connection.Execute(sql, new { RLNO = rlno }, DBWork.Transaction);
        }

        public IEnumerable<UR_ROLE> Get(string rlno)
        {
            var sql = @"SELECT RLNO,RLNA,RLDESC,
                        TO_CHAR(ROLE_CREATE_DATE,'YYYY/MM/DD HH24:MI:SS') ROLE_CREATE_DATE,
                        (SELECT UNA FROM UR_ID WHERE TUSER = ROLE_CREATE_BY) ROLE_CREATE_BY,
                        TO_CHAR(ROLE_MODIFY_DATE,'YYYY/MM/DD HH24:MI:SS') ROLE_MODIFY_DATE,
                        (SELECT UNA FROM UR_ID WHERE TUSER = ROLE_MODIFY_BY) ROLE_MODIFY_BY 
                        FROM UR_ROLE WHERE RLNO = :RLNO ";
            return DBWork.Connection.Query<UR_ROLE>(sql, new { RLNO = rlno }, DBWork.Transaction);
        }

        public IEnumerable<UR_ROLE> GetAll(string rlno, string rlna)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT RLNO,RLNA,RLDESC,
                        TO_CHAR(ROLE_CREATE_DATE,'YYYY/MM/DD HH24:MI:SS') ROLE_CREATE_DATE,
                        (SELECT UNA FROM UR_ID WHERE TUSER = ROLE_CREATE_BY) ROLE_CREATE_BY,
                        TO_CHAR(ROLE_MODIFY_DATE,'YYYY/MM/DD HH24:MI:SS') ROLE_MODIFY_DATE,
                        (SELECT UNA FROM UR_ID WHERE TUSER = ROLE_MODIFY_BY) ROLE_MODIFY_BY 
                        FROM UR_ROLE WHERE 1=1 ";
            if (rlno != "")
            {
                sql += " AND RLNO LIKE :p0 ";
                p.Add(":p0", string.Format("%{0}%", rlno));
            }
            if (rlna != "")
            {
                sql += " AND RLNA LIKE :p1 ";
                p.Add(":p1", string.Format("%{0}%", rlna));
            }

            return DBWork.PagingQuery<UR_ROLE>(sql, p, DBWork.Transaction);

        }

        public IEnumerable<UR_ROLE> GetByUser(string rlno, string rlna, string tuser)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT RLNO,RLNA,RLDESC,
                        TO_CHAR(ROLE_CREATE_DATE,'YYYY/MM/DD HH24:MI:SS') ROLE_CREATE_DATE,
                        (SELECT UNA FROM UR_ID WHERE TUSER = ROLE_CREATE_BY) ROLE_CREATE_BY,
                        TO_CHAR(ROLE_MODIFY_DATE,'YYYY/MM/DD HH24:MI:SS') ROLE_MODIFY_DATE,
                        (SELECT UNA FROM UR_ID WHERE TUSER = ROLE_MODIFY_BY) ROLE_MODIFY_BY 
                        FROM UR_ROLE WHERE ROLE_CREATE_BY=:TUSER ";
            if (rlno != "")
            {
                sql += " AND RLNO LIKE :p0 ";
                p.Add(":p0", string.Format("%{0}%", rlno));
            }
            if (rlna != "")
            {
                sql += " AND RLNA LIKE :p1 ";
                p.Add(":p1", string.Format("%{0}%", rlna));
            }
            p.Add("TUSER", tuser);

            return DBWork.PagingQuery<UR_ROLE>(sql, p, DBWork.Transaction);

        }
    }
}