using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace DailyDocnoSeqReset
{
    class Repository : JCLib.Mvc.BaseRepository
    {
        public Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public bool CheckReseted() {
            string sql = @"
                select 1 from PARAM_D a
                 where a.grp_code = 'DAILY_DOCNO_SEQ'
                   and a.data_name = 'MAXDATE'
                   and twn_date(sysdate) > nvl(a.data_value, '1')
            ";
            return DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction) == null;
        }

        public string GetNestSeq() {
            string sql = @"
                select daily_docno_seq.nextval from dual 
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }

        public int SetIncrement(string increment) {
            string sql = string.Format(@"
              alter sequence MMSADM.DAILY_DOCNO_SEQ increment by {0} minvalue 0
            ", increment);

            return DBWork.Connection.Execute(sql, DBWork.Transaction);
        }

        public int UpdateParamD() {
            string sql = @"
                update PARAM_D
                   set data_value = twn_date(sysdate)
                 where grp_code = 'DAILY_DOCNO_SEQ'
                   and data_name = 'MAXDATE'
            ";

            return DBWork.Connection.Execute(sql, DBWork.Transaction);
        }

    }
}
