using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyDocnoSeqReset
{
    public class Service
    {
        public void Run() {
            using (WorkSession session = new WorkSession()) {
                var DBwork = session.UnitOfWork;

                DBwork.BeginTransaction();
                try
                {
                    var repo = new Repository(DBwork);

                    bool isReseted = repo.CheckReseted();

                    if (isReseted) {
                        return;
                    }

                    // 0. 更改increment 為 1
                    int afrs = repo.SetIncrement("1");

                    // 1. 取得seq下一筆
                    string seq = repo.GetNestSeq();

                    // 2. 更改increment 為 seq * -1
                    afrs = repo.SetIncrement(string.Format("-{0}", seq));

                    // 3. 執行nextval 以重設
                    seq = repo.GetNestSeq();

                    // 4. 更改increment為1
                    afrs = repo.SetIncrement("1");

                    // 5. 更改PARAM_D
                    afrs = repo.UpdateParamD();

                    DBwork.Commit();
                }
                catch (Exception e) {
                    DBwork.Rollback();
                }
            }
        }
    }
}
