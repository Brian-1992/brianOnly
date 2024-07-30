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
    public class UR_UIRRepository : JCLib.Mvc.BaseRepository
    {
        public UR_UIRRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public int Create(UR_UIR ur_uir)
        {
            var sql = @"INSERT INTO UR_UIR (TUSER,RLNO,UIR_CREATE_BY) VALUES (@TUSER,@RLNO,@UIR_CREATE_BY)";
            return DBWork.Connection.Execute(sql, ur_uir, DBWork.Transaction);
        }

        public int Delete(UR_UIR ur_uir)
        {
            var sql = @"DELETE FROM UR_UIR WHERE TUSER=@TUSER AND RLNO=@RLNO";
            return DBWork.Connection.Execute(sql, ur_uir, DBWork.Transaction);
        }

        public int DeleteByUser(string tuser)
        {
            var sql = @"DELETE FROM UR_UIR WHERE TUSER=@TUSER";
            return DBWork.Connection.Execute(sql, new { TUSER = tuser }, DBWork.Transaction);
        }

        public int DeleteByRole(string rlno)
        {
            var sql = @"DELETE FROM UR_UIR WHERE RLNO=@RLNO";
            return DBWork.Connection.Execute(sql, new { RLNO = rlno }, DBWork.Transaction);
        }

        public bool CheckUserInRole(string rlno, string tuser)
        {
            var result = false;

            var sql = @"SELECT COUNT(1) FROM UR_UIR WHERE RLNO=@RLNO AND TUSER=@TUSER";
            var obj = DBWork.Connection.ExecuteScalar(sql, new { RLNO = rlno, TUSER = tuser }, DBWork.Transaction);
            if (obj != null)
                if (obj.ToString() != "0")
                    result = true;

            return result;
        }

        public IEnumerable<UR_UIR> GetUsersInRole(int page_index, int page_size, string rlno)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT RLNO,TUSER,
                        (SELECT UNA FROM UR_ID WHERE UR_ID.TUSER =  UR_UIR.TUSER) UNA
                        FROM UR_UIR WHERE RLNO=@RLNO";
            p.Add("RLNO", rlno);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<UR_UIR>(GetPagingStatement(sql), p, DBWork.Transaction);
        }

        public IEnumerable<UR_ID> GetUsersNotInRole(int page_index, int page_size, string rlno, string tuser, string una)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT TUSER,UNA,IDDESC FROM UR_ID WHERE TUSER NOT IN (SELECT TUSER FROM UR_UIR WHERE RLNO=@p0) ";
            p.Add("@p0", rlno);
            if (tuser != "")
            {
                sql += " AND TUSER LIKE @p1 ";
                p.Add("@p1", string.Format("%{0}%", tuser));
            }
            if (una != "")
            {
                sql += " AND UNA LIKE @p2 ";
                p.Add("@p2", string.Format("%{0}%", una));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<UR_ID>(GetPagingStatement(sql), p, DBWork.Transaction);

        }

        public IEnumerable<UR_ROLE> GetRoles(int page_index, int page_size, string rlno, string rlna, string login_user)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT RLNO,RLNA,RLDESC,
                        TO_CHAR(ROLE_CREATE_DATE,'YYYY/MM/DD HH24:MI:SS') ROLE_CREATE_DATE,
                        (SELECT UNA FROM UR_ID WHERE TUSER = ROLE_CREATE_BY) ROLE_CREATE_BY
                        TO_CHAR(ROLE_MODIFY_DATE,'YYYY/MM/DD HH24:MI:SS')
                        (SELECT UNA FROM UR_ID WHERE TUSER = ROLE_MODIFY_BY) ROLE_MODIFY_BY
                        FROM UR_ROLE 
                        WHERE 1=1 ";
            if (rlno != "%%")
            {
                sql += " AND RL_NO LIKE @p0 ";
                p.Add("@p0", rlno);
            }
            if (rlna != "%%")
            {
                sql += " AND RL_NA LIKE @p1 ";
                p.Add("@p1", rlna);
            }
            sql += @" AND EXISTS(SELECT TUSER FROM UR_ROLE_AUTH_DETAIL DETAIL 
                            WHERE DETAIL.TUSER = TUSER AND DETAIL.RLNO=UR_ROLE.RLNO)";
            p.Add("TUSER", login_user);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<UR_ROLE>(GetPagingStatement(sql), p, DBWork.Transaction);

        }
    }
}