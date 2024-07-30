using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Models;
using WebApp.Models.MI;

namespace AA0112
{
    class PhrWhinvCheck
    {
        LogConteoller logController = new LogConteoller("PhrWhinvCheck", "PhrWhinvCheck");

        public void Run()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
               
                try
                {
                    PhrWhinvCheckRepository repo = new PhrWhinvCheckRepository(DBWork);

                    logController.AddLogs("取得月結年月日");
                    string closeDayString = repo.GetMnSetDate();
                    logController.AddLogs(string.Format("取得月結年月日: {0}", closeDayString));

                    // 判斷是否為月結日
                    logController.AddLogs("判斷是否為月結日");
                    if (IsCloseDay(closeDayString) == false)
                    {
                        logController.AddLogs("非月結年月日 結束");
                        logController.CreateLogFile();
                        return;
                    }
                    logController.AddLogs("為月結年月日 繼續執行");

                    MI_MNSET mnset = repo.GetMnset();
                    logController.AddLogs("取得民國年月結年月");

                    IEnumerable<CHK_MAST> masts = repo.GetPhrChkMasts(mnset.SET_YM);
                    logController.AddLogs("取得藥局初盤單");

                    foreach (CHK_MAST mast in masts)
                    {
                        logController.AddLogs(string.Format("盤點單號：{0}", mast.CHK_NO));
                        IEnumerable<string> mmcodes = repo.GetNotExists(mast.CHK_NO);
                        logController.AddLogs(string.Format("查無MI_WHINV {0} 筆", mmcodes.Count()));
                        foreach (string mmcode in mmcodes)
                        {
                            logController.AddLogs(mmcode);
                        }

                        int afrs = repo.InsertWhinv(mast.CHK_NO);
                        logController.AddLogs(string.Format("新增WHINV {0} 筆", afrs));
                    }

                    logController.AddLogs("新增MI_WHINV完成");

                    DBWork.Commit();
                }
                catch (Exception ex) {
                    DBWork.Rollback();
                    //Console.Write(ex.Message);

                    logController.AddLogs(string.Format("error：{0}", ex.Message));
                    logController.AddLogs(string.Format("trace：{0}", ex.StackTrace));
                    logController.CreateLogFile();
                }

                logController.CreateLogFile();
            }
        }

        private bool IsCloseDay(string closeDayString)
        {
            DateTime closeDay = DateTime.Parse(closeDayString);
            logController.AddLogs(string.Format("轉換月結年月closeDay: {0}", closeDay.ToString()));

            logController.AddLogs(string.Format("是否同天: DateTime.Now:{0} closeDay:{1}", DateTime.Now.ToString("yyyy-MM-dd"), closeDay.ToString("yyyy-MM-dd")));
            return DateTime.Now.ToString("yyyy-MM-dd") == closeDay.ToString("yyyy-MM-dd");
        }

    }
}
