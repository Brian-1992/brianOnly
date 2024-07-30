using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UridDeactivate
{
    public class Service
    {
        LogController logController = new LogController("UridDeactivate", "UridDeactivate");
        public void Run()
        {
            logController.AddLogs(string.Format("{0} 開始處理", DateTime.Now.ToString("yyyy-MM-dd")));
            using (WorkSession session = new WorkSession()) {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try {
                    var repo = new Repository(DBWork);

                    IEnumerable<Item> list = repo.GetList();
                    logController.AddLogs(string.Format("取得需停用帳號：共{0}筆", list.Count()));
                    int i = 0;
                    foreach (Item item in list) {
                        i++;
                        logController.AddLogs(string.Format("{0}. tuser = {1}，前次login時間日期 = {2}", i, item.TUSER, item.LOGIN_DATE));

                        repo.UpdateUrId(item.TUSER);
                    }

                    logController.DeletePastLogs("UridDeactivate", "UridDeactivate");

                    DBWork.Commit();

                    logController.AddLogs(string.Format("{0} 處理完成", DateTime.Now.ToString("yyyy-MM-dd")));

                } catch (Exception ex) {
                    logController.AddLogs(string.Format("error：{0}", ex.Message));
                    logController.AddLogs(string.Format("trace：{0}", ex.StackTrace));
                    DBWork.Rollback();
                    Console.Write(ex.Message);
                }
                
            }
            logController.CreateLogFile();

        }
    }
}
