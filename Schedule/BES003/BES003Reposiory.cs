using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace BES003
{
    public class BES003Reposiory : JCLib.Mvc.BaseRepository
    {
        public BES003Reposiory(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public int UpdateStatus() {
            string sql = @"
                update CR_DOC
                   set cr_status = 'P',
                       update_time = sysdate,
                       update_user = '排程'
                 where twn_date(sysdate) > twn_date(reqdate)
                   and cr_status in ('A','B','E','F','G','H')
        
            ";

            return DBWork.Connection.Execute(sql, DBWork.Transaction);
        }
    }
}
