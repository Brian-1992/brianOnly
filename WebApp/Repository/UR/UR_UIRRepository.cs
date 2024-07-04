using System.Collections.Generic;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System;

namespace WebApp.Repository.UR
{
    public class UR_UIRRepository : JCLib.Mvc.BaseRepository
    {
        public UR_UIRRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        // UR1025使用者權限明細
        public IEnumerable<UR_TACL2> GetUserDetail(string tuser)
        {
            var par = new DynamicParameters();

            var sql = @" select B.RLNO, C.RLNA, C.RLDESC
                    from UR_ID A, UR_UIR B, UR_ROLE C
                    where A.TUSER = B.TUSER
                    and B.RLNO = C.RLNO ";

            if (tuser != "" && tuser != null)
            {
                sql += " and A.TUSER = :p0 ";
                par.Add(":p0", tuser);
            }
            
            return DBWork.PagingQuery<UR_TACL2>(sql, par, DBWork.Transaction);
        }
        // UR1025使用者權限明細
        public IEnumerable<UR_TACL2> GetFgDetail(string fg)
        {
            var par = new DynamicParameters();

            var sql = @" select distinct D.R, D.U, D.P, B.RLNO, C.RLNA, C.RLDESC
                    from UR_UIR B, UR_ROLE C, UR_TACL D
                    where  B.RLNO = C.RLNO
                    and B.RLNO = D.RLNO
                    and (D.R = '1' or D.U = '1' or D.P = '1') ";

            if (fg != "" && fg != null)
            {
                sql += " and D.FG = :p0 ";
                par.Add(":p0", fg);
            }

            return DBWork.PagingQuery<UR_TACL2>(sql, par, DBWork.Transaction);
        }

        public int Create(UR_UIR ur_uir)
        {
            var sql = @"INSERT INTO UR_UIR (TUSER,RLNO,UIR_CREATE_BY) VALUES (:TUSER,:RLNO,:UIR_CREATE_BY)";
            return DBWork.Connection.Execute(sql, ur_uir, DBWork.Transaction);
        }

        public int Delete(UR_UIR ur_uir)
        {
            var sql = @"DELETE FROM UR_UIR WHERE TUSER=:TUSER AND RLNO=:RLNO";
            return DBWork.Connection.Execute(sql, ur_uir, DBWork.Transaction);
        }

        public int DeleteByUser(string tuser)
        {
            var sql = @"DELETE FROM UR_UIR WHERE TUSER=:TUSER";
            return DBWork.Connection.Execute(sql, new { TUSER = tuser }, DBWork.Transaction);
        }

        public int DeleteByRole(string rlno)
        {
            var sql = @"DELETE FROM UR_UIR WHERE RLNO=:RLNO";
            return DBWork.Connection.Execute(sql, new { RLNO = rlno }, DBWork.Transaction);
        }

        public bool CheckUserInRole(string rlno, string tuser)
        {
            var result = false;

            var sql = @"SELECT COUNT(1) FROM UR_UIR WHERE RLNO=:RLNO AND TUSER=:TUSER";
            var obj = DBWork.Connection.ExecuteScalar(sql, new { RLNO = rlno, TUSER = tuser }, DBWork.Transaction);
            if (obj != null)
                if (obj.ToString() != "0")
                    result = true;

            return result;
        }

        public IEnumerable<UR_UIR> GetUsersInRole(string rlno)
        {
            var sql = @"SELECT RLNO,TUSER,
                        (SELECT RLNA FROM UR_ROLE WHERE RLNO = UR_UIR.RLNO) RLNA,
                        (SELECT UNA FROM UR_ID WHERE UR_ID.TUSER =  UR_UIR.TUSER) UNA
                        FROM UR_UIR WHERE RLNO=:RLNO";

            return DBWork.Connection.Query<UR_UIR>(sql, new { RLNO = rlno }, DBWork.Transaction);
        }

        public IEnumerable<UR_ID> GetUsersNotInRole(string rlno, string tuser, string una, string inid, string aduser, string inidLimit = "")
        {
            var p = new DynamicParameters();

            var sql = @"SELECT TUSER,UNA,IDDESC,ADUSER FROM UR_ID WHERE FL=1 ";
            if (rlno != "")
            {
                sql += " AND TUSER NOT IN (SELECT TUSER FROM UR_UIR WHERE RLNO=:p0)  ";
                p.Add(":p0", rlno);
            }
            if (tuser != "")
            {
                sql += " AND TUSER LIKE :p1 ";
                p.Add(":p1", string.Format("%{0}%", tuser));
            }
            if (una != "")
            {
                sql += " AND UNA LIKE :p2 ";
                p.Add(":p2", string.Format("%{0}%", una));
            }
            if (inid != "")
            {
                sql += " AND TUSER IN (SELECT TUSER FROM UR_ID WHERE INID LIKE :p3)  ";
                p.Add(":p3", string.Format("{0}%", inid));
            }
            if (inidLimit != "")
            {
                sql += " AND TUSER IN (SELECT TUSER FROM UR_ID WHERE INID LIKE :p4)  ";
                p.Add(":p4", string.Format("{0}%", inidLimit));
            }
            if (aduser != "")
            {
                sql += " AND ADUSER LIKE :p5  ";
                p.Add(":p5", string.Format("%{0}%", aduser));
            }

            return DBWork.PagingQuery<UR_ID>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<UR_UIR> GetRolesInUser(string tuser, string tuserLimit = "")
        {
            var p = new DynamicParameters();
            var sql = @"SELECT RLNO,TUSER,
                        (SELECT RLNA FROM UR_ROLE WHERE RLNO = UR_UIR.RLNO) RLNA,
                        (SELECT UNA FROM UR_ID WHERE UR_ID.TUSER =  UR_UIR.TUSER) UNA
                        FROM UR_UIR WHERE TUSER=:TUSER";
            p.Add("TUSER", tuser);
            if (tuserLimit != "")
            {
                sql += " AND RLNO IN (SELECT RLNO FROM UR_ROLE WHERE ROLE_CREATE_BY = :p2) ";
                p.Add(":p2", tuserLimit);
            }

            return DBWork.Connection.Query<UR_UIR>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<UR_ROLE> GetRolesNotInUser(string tuser, string rlno, string rlna, string tuserLimit = "")
        {
            var p = new DynamicParameters();
            var sql = @"SELECT RLNO,RLNA,RLDESC FROM UR_ROLE 
                        WHERE RLNO NOT IN (SELECT RLNO FROM UR_UIR WHERE TUSER=:TUSER)";
            p.Add("TUSER", tuser);
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
            if (tuserLimit != "")
            {
                sql += " AND ROLE_CREATE_BY = :p2 ";
                p.Add(":p2", tuserLimit);
            }

            return DBWork.PagingQuery<UR_ROLE>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<UR_ROLE> GetRoles(string rlno, string rlna, string login_user)
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
                sql += " AND RLNO LIKE :p0 ";
                p.Add("p0", rlno);
            }
            if (rlna != "%%")
            {
                sql += " AND RLNA LIKE :p1 ";
                p.Add("p1", rlna);
            }
            sql += @" AND EXISTS(SELECT TUSER FROM UR_ROLE_AUTH_DETAIL DETAIL 
                            WHERE DETAIL.TUSER = TUSER AND DETAIL.RLNO=UR_ROLE.RLNO)";
            p.Add("TUSER", login_user);

            return DBWork.PagingQuery<UR_ROLE>(sql, p, DBWork.Transaction);

        }
    }
}