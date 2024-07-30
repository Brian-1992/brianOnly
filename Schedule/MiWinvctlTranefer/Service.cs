using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiWinvctlTranefer
{
    public class Service
    {
        public void Run() {
            // 1. 取得有MI_WHINV無MI_WINVCTL的衛材
            using (WorkSession session = new WorkSession()) {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new MiWinvctlTransferRepository(DBWork);
                    repo.InsertWinvctl02();

                    repo.InsertWinvctl01();

                    DBWork.Commit();
                }
                catch (Exception e) {
                    DBWork.Rollback();
                }
            }
        }
    }
}
