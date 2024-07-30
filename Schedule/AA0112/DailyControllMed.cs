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
    public class DailyControllMed
    {
        LogConteoller logController = new LogConteoller("DailyControllMed", "DailyControllMed");

        IEnumerable<SelectType> types = new List<SelectType>() {
            new SelectType(){ CodeStart = "1", CodeEnd = "3"},
            new SelectType(){ CodeStart = "4", CodeEnd = "4"},
        };
        string chkym = string.Empty;
        public void Run()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;

                DailyControllMedRepository repo = new DailyControllMedRepository(DBWork);

                logController.AddLogs("取得月結年月");
                MI_MNSET mnset = repo.GetCurrentSetym();
                // 查無資料，表示月結未結束，不開單
                if (mnset == null) {
                    logController.AddLogs(string.Format("月結尚未結束，取消開單"));
                    logController.CreateLogFile();
                    return;
                }

                logController.AddLogs(string.Format("目前月結年月:{0}", mnset.SET_YM));

                logController.AddLogs("判斷是否為開單日：目前月份同月結年月且未開過單");
                // 判斷今日年月是否同月結年月
                if (IsOpenMonth(mnset.SET_YM) == false) {
                    logController.AddLogs(string.Format("月結年月({0})與目前月份({1})不同 結束", 
                                                          mnset.SET_YM, 
                                                          string.Format("{0}{1}", DateTime.Now.Year - 1911, DateTime.Now.Month)));
                    logController.CreateLogFile();
                    return;
                }
                // 判斷是否已開單
                if (repo.IsOpened(mnset.SET_YM))
                {
                    logController.AddLogs("本月已開過單 結束");
                    logController.CreateLogFile();
                    return;
                }
                logController.AddLogs("為開單年月日 繼續執行");
                // 取得盤點日
                logController.AddLogs("取得盤點日");
                chkym = GetChkym();
                logController.AddLogs(string.Format("盤點日:{0}", chkym));

                foreach (SelectType type in types)
                {
                    logController.AddLogs(string.Format("產生type {0}~{1}級管制藥", type.CodeStart, type.CodeEnd));

                    logController.AddLogs("取得庫房代碼");
                    IEnumerable<ChkWh> whnos = repo.GetWhnos(type.CodeStart, type.CodeEnd);
                    logController.AddLogs(string.Format("共 {0} 個", whnos.Count()));
                    foreach (ChkWh wh in whnos)
                    {
                        DBWork.BeginTransaction();
                        try
                        {
                            logController.AddLogs(string.Format("產生mast"));
                            CHK_MAST mast = GetChkMast(wh, type.CodeEnd);
                            string period = "D";
                            string chk_ym = period != "D" ? chkym : chkym.Substring(0, 5);
                            string currentSeq = GetCurrentSeq(wh.WH_NO, chk_ym);
                            mast.CHK_NO = string.Format("{0}{1}{2}{3}{4}", wh.WH_NO, chk_ym, period, type.CodeEnd, currentSeq);
                            //mast.CHK_NO = GetChkNo(wh.WH_NO, type.CodeEnd);
                            mast.CHK_NO1 = mast.CHK_NO;

                            // 新增master
                            logController.AddLogs(string.Format("新增master"));
                            repo.InsertMaster(mast);
                            logController.AddLogs(string.Format("新增master 完成"));
                            // 新增detail
                            logController.AddLogs(string.Format("新增detail"));
                            int detailResulr = repo.InsertDetail(mast);
                            logController.AddLogs(string.Format("新增detail 完成"));

                            logController.AddLogs(string.Format("更新master chk_num"));
                            repo.UpdateMaster(mast);
                            logController.AddLogs(string.Format("更新master chk_num 完成"));
                            // 新增可盤點人員檔
                            logController.AddLogs(string.Format("新增可盤點人員檔"));
                            repo.InsertNouid(mast);
                            logController.AddLogs(string.Format("新增可盤點人員檔 完成"));
                            DBWork.Commit();
                        }
                        catch (Exception ex)
                        {
                            logController.AddLogs(string.Format("error：{0}", ex.Message));
                            logController.AddLogs(string.Format("trace：{0}", ex.StackTrace));
                            DBWork.Rollback();
                            Console.Write(ex.Message);
                        }
                    }
                }
            }
        }

        private bool IsOpenMonth(string openMonthString)
        {
            return (string.Format("{0}{1}", DateTime.Now.Year - 1911 
                                          , DateTime.Now.Month.ToString().PadLeft(2, '0') )
                   ) == openMonthString;
        }

        private string GetChkym()
        {
            //DateTime now = new DateTime(2019, 1, 1);
            //string yyy = (now.Year - 1911).ToString();
            //int m = now.Month;
            //int d = now.Day;
            string yyy = (DateTime.Now.Year - 1911).ToString();
            int m = DateTime.Now.Month;
            int d = DateTime.Now.Day;
            string mm = m > 9 ? m.ToString() : m.ToString().PadLeft(2, '0');
            string dd = d > 9 ? d.ToString() : d.ToString().PadLeft(2, '0');
            return string.Format("{0}{1}{2}", yyy, mm, dd);
        }
        private CHK_MAST GetChkMast(ChkWh chkwh, string codeEnd)
        {
            return new CHK_MAST()
            {
                CHK_WH_NO = chkwh.WH_NO,
                CHK_YM = chkym,
                CHK_WH_GRADE = chkwh.WH_GRADE,
                CHK_WH_KIND = "0",
                CHK_CLASS = "01",
                CHK_PERIOD = "D",
                CHK_TYPE = codeEnd,
                CHK_LEVEL = "1",
                CHK_NUM = "0",
                CHK_TOTAL = chkwh.CHK_TOTAL.ToString(),
                CHK_STATUS = "1",
                CHK_KEEPER = chkwh.WH_NO,
                CHK_NO1 = string.Empty,
                CREATE_USER = "BATCH",
                UPDATE_USER = "",
                UPDATE_IP = string.Empty
            };
        }

        private string GetChkNo(string wh_no, string chk_type)
        {
            string period = "D";
            string chk_ym = period != "D" ? chkym : chkym.Substring(0, 5);
            string currentSeq = GetCurrentSeq(wh_no, chk_ym);
            return string.Format("{0}{1}{2}{3}{4}", wh_no, chk_ym, period, chk_type, currentSeq);
        }
        private string GetCurrentSeq(string wh_no, string ym)
        {
            string maxNo = string.Empty;
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new DailyControllMedRepository(DBWork);
                    maxNo = repo.GetCurrentSeq(wh_no, ym);
                }
                catch
                {
                    throw;
                }
                return maxNo;
            }
        }
    }
}
