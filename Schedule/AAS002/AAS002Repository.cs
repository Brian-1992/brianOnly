using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace AAS002
{
    public class AAS002Repository : JCLib.Mvc.BaseRepository
    {
        public AAS002Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public int ClearCfmScan() {
            string sql = @"
                update CR_DOC
                   set cfmScan = null,
                       scanTime = null
                 where cr_status = 'I'
                   and scanTime < (sysdate - 1/48)
            ";
            return DBWork.Connection.Execute(sql, DBWork.Transaction);
        }
    }
}
