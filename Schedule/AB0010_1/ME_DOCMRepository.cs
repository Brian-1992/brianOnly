using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JCLib.DB;
using Dapper;

namespace AB0010_1.Repository
{
    class ME_DOCMRepository : JCLib.Mvc.BaseRepository
    {
        public ME_DOCMRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public int UpdateME_DOCM()
        {
            var sql = @"UPDATE ME_DOCM
                          SET FLOWID = '0102'
                          WHERE FLOWID = '0100' ";

            return DBWork.Connection.Execute(sql, DBWork.Transaction);
        }

        public IEnumerable<string> GetDocnos() {
            string sql = @"
                select docno from ME_DOCM
                 where flowId = '0100'
            ";
            return DBWork.Connection.Query<string>(sql, DBWork.Transaction);
        }

        public int UpdateDocdApl_contime(string docno) {
            string sql = @"
                update ME_DOCD
                   set apl_contime = sysdate,
                       APVQTY=APPQTY,
                       update_time = sysdate,
                       update_user = 'SYSTEM' 
                 where docno = :docno
            ";
            return DBWork.Connection.Execute(sql, new { docno }, DBWork.Transaction);
        }
    }
}
