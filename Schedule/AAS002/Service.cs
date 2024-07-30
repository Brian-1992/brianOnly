using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAS002
{
    public class Service
    {
        LogConteoller logController = new LogConteoller("AAS002", DateTime.Now.ToString("yyyy-MM"), "AAS002");

        public void Run()
        {
            using (WorkSession session = new WorkSession())
            {

                var DBWork = session.UnitOfWork;

                DBWork.BeginTransaction();
                try
                {
                    AAS002Repository repo = new AAS002Repository(DBWork);

                    int afrs = repo.ClearCfmScan();
                    DBWork.Commit();
                }
                catch (Exception ex)
                {
                    logController.AddLogs(string.Format("error：{0}", ex.Message));
                    logController.CreateLogFile();
                    DBWork.Rollback();
                    throw;
                }

            }

        }
    }
}
