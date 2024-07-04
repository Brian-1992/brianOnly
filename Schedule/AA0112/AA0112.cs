using JCLib.DB;
using JCLib.DB.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Models;

namespace AA0112
{
    public class AA0112
    {

        IEnumerable<string> storeids = new List<string>() { "0", "1" };

        LogConteoller logController = new LogConteoller("Monthly24Ward", DateTime.Now.ToString("yyyy-MM"), "Monthly24Ward");

        // 0:非庫備 1:庫備 3:小額採購
        string chkym = string.Empty;
        string preym = string.Empty;
        string postym = string.Empty;
        public void Run()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;

                AA0112Repository repo = new AA0112Repository(DBWork);
                bool done = true;

                logController.AddLogs("取得盤點開單年月日");
                CHK_MNSET chkmnset = repo.GetChkmnset();
                chkym = chkmnset.CHK_YM;
                preym = chkmnset.PRE_YM;
                postym = chkmnset.POST_YM;
                string openDayString = chkmnset.SET_CTIME;
                logController.AddLogs(string.Format("本次年月:{0}", chkym));
                logController.AddLogs(string.Format("上次年月:{0}", preym));
                logController.AddLogs(string.Format("下次年月:{0}", postym));
                logController.AddLogs(string.Format("盤點開單年月日: {0}", openDayString));

                // 判斷是否為月結日
                logController.AddLogs("判斷是否為開單日");
                if (IsOpenDay(openDayString) == false)
                {
                    logController.AddLogs("非開單年月日 結束");
                    logController.CreateLogFile();
                    return;
                }
                logController.AddLogs("為開單年月日 繼續執行");

                // 全院衛材庫二級庫強迫點收
                DBWork.BeginTransaction();
                logController.AddLogs("執行強迫點收");
                IEnumerable<string> whno_strings = repo.GetWhNosGrade2();
                logController.AddLogs(string.Format("需強迫點收庫房數量(全院衛材二級庫): {0}", whno_strings.Count()));
                foreach (string whno in whno_strings) {
                    logController.AddLogs(string.Format("庫房代碼:{0}", whno));
                    SP_MODEL sp = repo.PostUack(chkym, whno);

                    if (sp.O_RETID == "Y")
                    {
                        logController.AddLogs("強迫點收成功");
                    }
                    if (sp.O_RETID == "N")
                    {
                        logController.AddLogs("強迫點收失敗");
                        session.Result.msg = whno + " " + sp.O_ERRMSG;
                        logController.AddLogs(string.Format("錯誤訊息：", (whno + " " + sp.O_ERRMSG)));
                    }
                }
                DBWork.Commit();


                // 產生 庫備/非庫備 盤點單
                foreach (string storeid in storeids)
                {
                    logController.AddLogs(string.Format("產生 {0} 盤點單，取得庫房代碼", storeid == "0" ? "非庫備" : "庫備"));
                    logController.AddLogs("(非上月庫存為0且本月無異動)");

                    IEnumerable<ChkWh> whnos = repo.GetWhnos(storeid, preym);
                    logController.AddLogs(string.Format("共{0}個庫房", whnos.Count()));
                    foreach (ChkWh wh in whnos)
                    {
                        DBWork.BeginTransaction();
                        try
                        {
                            logController.AddLogs(string.Format("庫房代碼:{0}", wh.WH_NO));
                            

                            logController.AddLogs("確認是否已開單");
                            if (repo.CheckExists(wh.WH_NO, chkym, storeid, "02", "M")) {
                                logController.AddLogs("已開單，跳過，繼續執行");
                                DBWork.Rollback();
                                continue;
                            }
                            logController.AddLogs("未開單，產生盤點單");
                            CHK_MAST mast = GetChkMast(wh, storeid);
                            logController.AddLogs("CHK_MAST設定完成");
                            string period = "A";
                            string currentSeq = GetCurrentSeq(wh.WH_NO, chkym);
                            mast.CHK_NO = string.Format("{0}{1}{2}{3}{4}", wh.WH_NO, chkym, period, storeid, currentSeq);
                            //mast.CHK_NO = GetChkNo(wh.WH_NO, storeid);
                            logController.AddLogs(string.Format("取得chk_no:{0}", mast.CHK_NO));
                            mast.CHK_NO1 = mast.CHK_NO;
                            // 新增master
                            repo.InsertMaster(mast);
                            logController.AddLogs("新增master完成");
                            // 新增detail
                            int temp1 = repo.InsertDetail(mast, preym);
                            logController.AddLogs(string.Format("新增detail完成: {0}筆", temp1));
                            //int temp2 = repo.InsertDetailNoPre(mast, preym);
                            //logController.AddLogs(string.Format("新增detail(本月新申領上月不存在品項)完成: {0}筆", temp2));
                            repo.UpdateMaster(mast);
                            logController.AddLogs("更新master chk_num完成");
                            // 新增可盤點人員檔
                            logController.AddLogs("新增chk_nouid");
                            repo.InsertNouid(mast);
                            logController.AddLogs("新增chk_nouid完成");
                            DBWork.Commit();
                        }
                        catch (Exception ex)
                        {
                            done = false;
                            logController.AddLogs(string.Format("error：{0}", ex.Message));
                            DBWork.Rollback();
                            Console.Write(ex.Message);
                        }
                    }
                }

                // 產生小額採購盤點單
                logController.AddLogs(string.Format("產生小額採購盤點單"));
                IEnumerable<ChkWh> whs = repo.GetWhno3s(preym);
                logController.AddLogs(string.Format("共{0}個庫房", whs.Count()));
                foreach (ChkWh wh in whs)
                {
                    DBWork.BeginTransaction();
                    try
                    {
                        logController.AddLogs(string.Format("庫房代碼:{0}", wh.WH_NO));
                        logController.AddLogs("確認是否已開單");
                        if (repo.CheckExists(wh.WH_NO, chkym, "3", "02", "M"))
                        {
                            logController.AddLogs("已開單，跳過，繼續執行");
                            DBWork.Rollback();
                            continue;
                        }
                        logController.AddLogs("未開單，產生盤點單");
                        CHK_MAST mast = GetChkMast(wh, "3");
                        logController.AddLogs("CHK_MAST設定完成");

                        string period = "A";
                        string currentSeq = GetCurrentSeq(wh.WH_NO, chkym);
                        mast.CHK_NO = string.Format("{0}{1}{2}{3}{4}", wh.WH_NO, chkym, period, "3", currentSeq);
                        //mast.CHK_NO = GetChkNo(wh.WH_NO, "3");

                        logController.AddLogs(string.Format("取得chk_no:{0}", mast.CHK_NO));
                        mast.CHK_NO1 = mast.CHK_NO;

                        // 新增master
                        logController.AddLogs("新增master");
                        repo.InsertMaster(mast);
                        logController.AddLogs("新增master完成");
                        // 新增detail.
                        logController.AddLogs("新增detail");
                        int temp3 = repo.InsertDetailType3(mast, preym);
                        logController.AddLogs(string.Format("新增detail完成: {0}筆", temp3));
                        //int temp4 = repo.InsertDetailNoPreType3(mast, preym);
                        //logController.AddLogs(string.Format("新增detail(本月新申領上月不存在品項)完成: {0}筆", temp4));
                        repo.UpdateMaster(mast);
                        logController.AddLogs("更新master chk_num完成");
                        // 新增可盤點人員檔
                        logController.AddLogs("新增chk_nouid");
                        repo.InsertNouid(mast);
                        logController.AddLogs("新增chk_nouid完成");
                        DBWork.Commit();

                    }
                    catch (Exception ex)
                    {
                        done = false;
                        logController.AddLogs(string.Format("error：{0}", ex.Message));
                        DBWork.Rollback();
                        Console.Write(ex.Message);
                    }
                }


                if (done == true)
                {
                    logController.AddLogs("產生單子無錯誤 更新chk_mnset");
                    //開單完成 更新CHK_MNSET狀態並新增
                    DBWork.BeginTransaction();
                    try
                    {
                        logController.AddLogs("更新chk_mnset狀態:N → Y");
                        repo.UpdateChkmnset();
                        logController.AddLogs("更新chk_mnset狀態完成");
                        logController.AddLogs("新增chk_mnset");
                        CHK_MNSET set = new CHK_MNSET();
                        set.CHK_YM = postym;
                        set.UPDATE_IP = DBWork.ProcIP;
                        repo.InsertChkmnset(set);
                        logController.AddLogs("新增chk_mnset 完成");
                        DBWork.Commit();
                    }
                    catch (Exception ex)
                    {
                        logController.AddLogs(string.Format("error：{0}", ex.Message));
                        DBWork.Rollback();
                        Console.Write(ex.Message);
                    }
                }
                else {
                    logController.AddLogs("產生單子有錯誤 不更新chk_mnset");
                }


                logController.CreateLogFile();

            }
        }

        public bool IsOpenDay(string openDayString) {
            return DateTime.Now.ToString("yyyy-MM-dd") == openDayString;
        }

        private string GetChkym()
        {
            string yyy = (DateTime.Now.Year - 1911).ToString();
            int m = DateTime.Now.Month;
            string mm = m > 9 ? m.ToString() : m.ToString().PadLeft(2, '0');
            return string.Format("{0}{1}", yyy, mm);
        }
        private string GetPreym()
        {
            int m;
            string yyy;
            if (DateTime.Now.Month - 1 == 0)
            {
                yyy = (DateTime.Now.Year - 1911 - 1).ToString();
                m = 12;
            }
            else
            {
                yyy = (DateTime.Now.Year - 1911).ToString();
                m = DateTime.Now.Month - 1;
            }

            string mm = m > 9 ? m.ToString() : m.ToString().PadLeft(2, '0');
            return string.Format("{0}{1}", yyy, mm);
        }
        private CHK_MAST GetChkMast(ChkWh chkwh, string storeid)
        {
            return new CHK_MAST()
            {
                CHK_WH_NO = chkwh.WH_NO,
                CHK_YM = chkym,
                CHK_WH_GRADE = chkwh.WH_GRADE,
                CHK_WH_KIND = "1",
                CHK_CLASS = "02",
                CHK_PERIOD = "M",
                CHK_TYPE = storeid,
                CHK_LEVEL = "1",
                CHK_NUM = "0",
                CHK_TOTAL = chkwh.CHK_TOTAL.ToString(),
                CHK_STATUS = "1",
                CHK_KEEPER = chkwh.INID,
                CHK_NO1 = string.Empty,
                CREATE_USER = "BATCH",
                UPDATE_USER = "",
                UPDATE_IP = string.Empty
            };
        }

        private string GetChkNo(string wh_no, string chk_type)
        {
            string period = "A";

            string currentSeq = GetCurrentSeq(wh_no, chkym);
            return string.Format("{0}{1}{2}{3}{4}", wh_no, chkym, period, chk_type, currentSeq);
        }
        private string GetCurrentSeq(string wh_no, string ym)
        {
            string maxNo = string.Empty;
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0112Repository(DBWork);
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
