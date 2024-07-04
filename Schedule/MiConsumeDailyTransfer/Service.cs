using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiConsumeDailyTransfer
{

    public class Service
    {
        LogController logController = new LogController("MiConsumeDailyTransfer", "MiConsumeDailyTransfer");

        public void Run()
        {

            using (WorkSession session = new WorkSession()) {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new MiConsumeDailyTransferRepository(DBWork);

                    // 1. 檢查是否月結中
                    logController.AddLogs("檢查是否月結中");
                    if (repo.CheckMiMnSet() == false) {
                        logController.AddLogs("月結中，結束排程");
                        logController.CreateLogFile();
                        return;
                    }

                    logController.AddLogs("非月結中，繼續執行");

                    // 2. 檢查是否有前日資料
                    logController.AddLogs("檢查是否有前日資料");
                    if (repo.CheckPreData() == true) {
                        logController.AddLogs("有前日資料，結束排程");
                        logController.CreateLogFile();
                        return;
                    }
                    logController.AddLogs("無前日資料，繼續執行");

                    // 3. 刪除35天前資料
                    logController.AddLogs("刪除35天前資料");
                    session.Result.afrs = repo.DeleteOldData();
                    logController.AddLogs(string.Format("共刪除{0}筆", session.Result.afrs));

                    // 4. 新增昨天資料
                    // 前日是否為月節日
                    logController.AddLogs("新增昨天資料");
                    if (repo.CheckSetCtimeToday())
                    {
                        logController.AddLogs("前日為月結日，從INVMON新增");
                        string set_ym = repo.GetSetYm();
                        session.Result.afrs = repo.InsertNewDataFromInvmon(set_ym);
                    }
                    else {
                        logController.AddLogs("前日非月結日，從WHINV新增");
                        session.Result.afrs = repo.InsertNewData();
                    }

                    
                    logController.AddLogs(string.Format("共新增{0}筆", session.Result.afrs));

                    DBWork.Commit();

                    logController.CreateLogFile();
                }
                catch (Exception ex) {
                    logController.AddLogs(string.Format("error：{0}", ex.Message));
                    logController.AddLogs(string.Format("trace：{0}", ex.StackTrace));
                    DBWork.Rollback();

                    logController.CreateLogFile();

                }
            }
        }
    }
}
