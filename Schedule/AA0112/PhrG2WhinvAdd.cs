using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Models;

namespace AA0112
{
    class PhrG2WhinvAdd
    {
        LogConteoller logController = new LogConteoller("PhrG2WhinvAdd", "PhrG2WhinvAdd");
        public void Run() {
            using (WorkSession session = new WorkSession()) {
                var DBWork = session.UnitOfWork;

                PhrG2WhinvAddRepository repo = new PhrG2WhinvAddRepository(DBWork);
                DBWork.BeginTransaction();
                try
                {
                    logController.AddLogs("取得本期年月");
                    string set_ym = repo.GetMnSet();
                    logController.AddLogs(string.Format("取得本期年月: {0}", set_ym));

                    logController.AddLogs("取得未開始盤點藥局初盤盤點單chk_status=0)");
                    IEnumerable<CHK_MAST> chk_masts = repo.GetChknos(set_ym);
                    logController.AddLogs(string.Format("數量: {0}", chk_masts.Count()));
                    logController.AddLogs("檢查是否有需新增項目");
                    foreach (CHK_MAST chk_mast in chk_masts) {
                        logController.AddLogs(string.Format("單號:{0}，庫房代碼:{1}，目前品項數量:{2}", chk_mast.CHK_NO, chk_mast.CHK_WH_NO, chk_mast.CHK_TOTAL));
                        IEnumerable<CHK_DETAIL> add_details = new List<CHK_DETAIL>();
                        if (chk_mast.CHK_PERIOD == "S")
                        {
                            add_details = repo.GetSItems(chk_mast.CHK_WH_NO, chk_mast.CHK_NO);
                        }
                        else {
                            add_details = repo.GetPItems(set_ym, chk_mast.CHK_WH_NO, chk_mast.CHK_NO);
                        }
                        logController.AddLogs(string.Format("需新增數量: {0}", add_details.Count()));
                        string chk_pre_date = repo.GetChkPreDate(chk_mast.CHK_NO);
                        int afrs = 0;
                        foreach (CHK_DETAIL detail in add_details) {
                            afrs = 0;
                            logController.AddLogs(string.Format("新增院內碼: {0}", detail.MMCODE));
                            string seq = repo.GetSeq(chk_mast.CHK_NO);
                            logController.AddLogs(string.Format("序號: {0}", seq));
                            CHK_G2_WHINV whinv = new CHK_G2_WHINV()
                            {
                                CHK_NO = chk_mast.CHK_NO,
                                WH_NO = chk_mast.CHK_WH_NO,
                                MMCODE = detail.MMCODE,
                                CREATE_USER = "BATCH",
                                UPDATE_USER = "BATCH",
                                UPDATE_IP = "0.0.0.0",
                                SEQ = seq,
                                CHK_PRE_DATE = chk_pre_date
                            };
                            afrs += repo.InsertChkG2Whinv(whinv);
                        }
                        logController.AddLogs(string.Format("共新增: {0}", afrs));
                    }
                    logController.AddLogs(string.Format("新增完成"));
                    
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
    }
}
