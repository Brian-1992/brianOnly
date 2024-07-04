using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JCLib.DB;
using Dapper;
using WebApp.Models;

namespace WebApp.Repository.UR
{
    public class UR_LOGINRepository : JCLib.Mvc.BaseRepository
    {
        public UR_LOGINRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<UR_LOGIN> Query(string una, DateTime? loginDate_b, DateTime? loginDate_e, string userIp)
        {
            var p = new DynamicParameters();
            
            var sql = @"SELECT TUSER, LOGIN_DATE, LOGOUT_DATE, USER_IP, AP_IP,  
                        (SELECT UNA FROM UR_ID WHERE TUSER=UL.TUSER) UNA 
                        FROM UR_LOGIN UL where 1=1 ";

            if (una != null)
            {
                sql += " AND TUSER IN (SELECT TUSER FROM UR_ID WHERE UNA LIKE :p0) ";
                p.Add(":p0", string.Format("%{0}%", una));
            }
            if (loginDate_b != null)
            {
                sql += " AND TRUNC(LOGIN_DATE) >= TRUNC(:p1) ";
                p.Add(":p1", loginDate_b);
            }
            if (loginDate_e != null)
            {
                sql += " AND TRUNC(LOGIN_DATE) <= TRUNC(:p1_1) ";
                p.Add(":p1_1", loginDate_e);
            }
            if (userIp != null)
            {
                sql += " AND USER_IP like :p2 ";
                p.Add(":p2", string.Format("%{0}%", userIp));
            }

            return DBWork.PagingQuery<UR_LOGIN>(sql, p, DBWork.Transaction);
        }
    }
}