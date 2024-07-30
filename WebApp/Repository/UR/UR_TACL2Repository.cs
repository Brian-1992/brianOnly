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
    public class UR_TACL2Repository : JCLib.Mvc.BaseRepository
    {
        public UR_TACL2Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<UR_TACL2> GetUsers(string tcbLimit, string tuser, string una, string inid, string inidLimit, string aduser)
        {
            string sql = "";
            var p = new DynamicParameters();

            if (tcbLimit == "")
            {
                sql = @"SELECT * FROM (
                        SELECT TUSER, UNA, :TUSER TACL_CREATE_BY, :UNA TACL_CREATE_UNA, ADUSER FROM UR_ID WHERE FL=1
                        UNION 
                        SELECT TUSER, (SELECT UNA FROM UR_ID WHERE TUSER=A.TUSER), 
                        TACL_CREATE_BY, (SELECT UNA FROM UR_ID WHERE TUSER=A.TACL_CREATE_BY) TACL_CREATE_UNA,
                        (SELECT ADUSER FROM UR_ID WHERE TUSER=A.TUSER) ADUSER FROM UR_TACL2 A
                        WHERE A.TACL_CREATE_BY != :TUSER ) V WHERE 1=1 ";
            }
            else
            {
                sql = @"SELECT TUSER, UNA, :TUSER TACL_CREATE_BY, :UNA TACL_CREATE_UNA, ADUSER FROM UR_ID WHERE FL = 1 ";
            }
            p.Add("TUSER", DBWork.UserInfo.UserId);
            p.Add("UNA", DBWork.UserInfo.UserName);

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
                sql += " AND ADUSER LIKE :p5 ";
                p.Add(":p5", string.Format("%{0}%", aduser));
            }

            return DBWork.PagingQuery<UR_TACL2>(sql, p, DBWork.Transaction);
        }

        public int Copy(string tuser_src, string tuser_des)
        {
            int _afrs = 0;

            string sql = @"INSERT INTO UR_TACL2 (FG, TUSER, V, R, U, P) 
                    SELECT FG, :TUSER_DES, V, R, U, P FROM UR_TACL WHERE RLNO = :TUSER_SRC";
            _afrs += DBWork.Connection.Execute(sql, new { RLNO_DES = tuser_des, RLNO_SRC = tuser_src }, DBWork.Transaction);

            return _afrs;
        }

        public int Update(string tuser, string manager, UR_TACL2[] ur_tacl2)
        {
            string del_sql = @"DELETE FROM UR_TACL2 WHERE TUSER=:TUSER AND TACL_CREATE_BY=:TACL_CREATE_BY AND FG=:FG";
            string ins_sql = @"INSERT INTO UR_TACL2 (FG, TUSER, V, R, U, P, TACL_CREATE_BY, TACL_CREATE_DATE) 
                    VALUES (:FG, :TUSER, :V, :R, :U, :P, :TACL_CREATE_BY, SYSDATE)";

            int _afrs = 0;
            foreach (var item in ur_tacl2)
            {
                item.TUSER = tuser;
                item.TACL_CREATE_BY = manager;
                _afrs += DBWork.Connection.Execute(del_sql, item, DBWork.Transaction);
                if (item.V == 1 || item.R == 1 || item.U == 1 || item.P == 1)
                {
                    _afrs += DBWork.Connection.Execute(ins_sql, item, DBWork.Transaction);
                }
            }
            return _afrs;
        }

        public int DeleteByUser(string tuser)
        {
            string sql = @"DELETE FROM UR_TACL2 WHERE TUSER=:TUSER";
            return DBWork.Connection.Execute(sql, new { TUSER = tuser }, DBWork.Transaction);
        }

        public int DeleteByFG(string fg)
        {
            var _afrs = 0;
            var sql = @"DELETE FROM UR_TACL2 WHERE FG=:FG";

            _afrs = DBWork.Connection.Execute(sql, new { FG = fg }, DBWork.Transaction);

            return _afrs;
        }
    }
}