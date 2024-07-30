using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UridDeactivate
{
    public class Repository : JCLib.Mvc.BaseRepository
    {
        public Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<Item> GetList() {
            string sql = @"
                select tuser, twn_date(login_date) as login_date
                  from (select a.tuser, max(b.login_date) as login_date
                          from UR_ID a, UR_LOGIN b
                         where a.fl = '1'   -- 是否有效(1有效0無效)
                           and a.tuser = b.tuser
                           and a.tuser <> 'admin'
                         group by a.tuser
                        )
                 where 1=1 
                   and twn_date(login_date) <= twn_date(sysdate-30)
                union
                select c.tuser, '從未登入' as login_date
                  from UR_ID c
                 where c.fl = '1'
                   and c.tuser <> 'admin'
                   and not exists (select 1 from UR_LOGIN where tuser = c.tuser)
            ";
            return DBWork.Connection.Query<Item>(sql, DBWork.Transaction);
        }

        public int UpdateUrId(string tuser) {
            string sql = @"
                update UR_ID
                   set fl = '0',  -- 是否有效(1有效0無效)
                       update_time = sysdate,
                       update_user = 'SYSTEM',
                       update_ip = '系統停用'
                 where tuser = :tuser
            ";
            return DBWork.Connection.Execute(sql, new { tuser }, DBWork.Transaction);
        }
    }
}
