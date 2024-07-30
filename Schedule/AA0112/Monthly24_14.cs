using JCLib.DB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Models;

namespace AA0112
{
    public class Monthly24_14
    {

        LogConteoller logController = new LogConteoller("Monthly24_14", DateTime.Now.ToString("yyyy-MM"), "Monthly24_14");
        string set_ym = string.Empty;
        public void Run()
        {
            using (WorkSession session = new WorkSession())
            {
                //排程每天執行，檢查是否為月結日隔天，若是則開立全院盤點單(新增CHK_MAST、CHK_DETAIL)，並修改盤點月份檔資料(CHK_MNSET)
                var DBWork = session.UnitOfWork;
                Monthly24_14Repository repo = new Monthly24_14Repository(DBWork);
                bool done = true;
                string chk_no = "";

                //判斷是否為月結日隔天，是則繼續執行，否則結束
                logController.AddLogs("判斷是否為月結日隔天");
                if (repo.IsOpenDay() != "1")
                {
                    logController.AddLogs("非月結日隔天 結束");
                    logController.CreateLogFile();
                    return;
                }
                logController.AddLogs("為月結日隔天 繼續執行");

                //產生盤點單與明細-1.取得全院庫房
                logController.AddLogs("產生盤點單與明細-取得全院庫房");
                IEnumerable<ChkWh> whnos = repo.GetWhnos();
                logController.AddLogs(string.Format("共{0}個庫房", whnos.Count()));
                string hospCode = repo.GetHospCode();
                foreach (ChkWh wh in whnos)
                {
                    DBWork.BeginTransaction();
                    try
                    {
                        chk_no = wh.CHK_YM;
                        set_ym = wh.CHK_YM;
                        //產生盤點單與明細-2.檢查是否已開單，若已開單，跳過本筆資料繼續執行下筆
                        logController.AddLogs(string.Format("庫房代碼:{0}", wh.WH_NO));
                        logController.AddLogs("確認是否已開單");
                        if (repo.CheckExists(wh.WH_NO, wh.CHK_YM, "M"))
                        {
                            logController.AddLogs("已開單，跳過，繼續執行");
                            DBWork.Rollback();
                            continue;
                        }
                        logController.AddLogs("未開單，產生盤點單");

                        //產生盤點單與明細-3.取得盤點單號
                        string currentSeq = GetCurrentSeq(wh.WH_NO, wh.CHK_YM);
                        string getChkNo = string.Format("{0}{1}{2}{3}{4}", wh.WH_NO, wh.CHK_YM, "A", "X", currentSeq);
                        logController.AddLogs(string.Format("取得chk_no:{0}", getChkNo));

                        //產生盤點單與明細-4.取得準備 insert CHK_MASY 資料
                        CHK_MAST mast = GetChkMast(wh);
                        mast.CHK_NO = getChkNo;
                        mast.CHK_YM = wh.CHK_YM;

                        logController.AddLogs("CHK_MAST設定完成");

                        //產生盤點單與明細-5.insert CHK_MAST
                        int temp1 = repo.InsertMaster(mast);
                        logController.AddLogs(JsonConvert.SerializeObject(mast));
                        logController.AddLogs(string.Format("insert CHK_MAST完成: {0}筆", temp1));
                        //產生盤點單與明細-6.1 insert CHK_DETAIL 
                        //依庫房級別選擇開單品項若(A所得wh_grade = 1)
                        //                      或(A所得wh_grade='2' and A所得wh_kind='0')或(A所得wh_name = '供應中心')
                        // 20240216: chk_qty預帶為電腦量
                        if (wh.WH_GRADE == "1" || (wh.WH_GRADE == "2" && wh.WH_KIND == "0") || wh.WH_NAME.Contains("供應中心"))
                        {
                            int tempA = repo.InsertDetail_A(mast);
                            logController.AddLogs(string.Format("新增detail_A完成: {0}筆", tempA));
                        }
                        else
                        //產生盤點單與明細-6.2 insert CHK_DETAIL 依庫房級別選擇開單品項若(其他=衛星單位衛材庫)
                        // 20240216: chk_qty預帶為電腦量
                        {
                            int tempB = repo.InsertDetail_B(mast);
                            logController.AddLogs(string.Format("新增detail_B完成: {0}筆", tempB));
                        }

                        // 若為北投，科室病房都轉消耗
                        if (hospCode == "818" && wh.WH_GRADE == "2" && wh.WH_KIND == "1")
                        {
                            logController.AddLogs(string.Format("處理科室病房庫存轉消耗"));
                            int tempUpdateUse = repo.UpdateChkDetailUse(mast.CHK_NO);

                            logController.AddLogs(string.Format("處理MI_WINVMON"));
                            int tempMiWinvmon = repo.UpdateMiwinvmonUse(mast.CHK_YM, wh.WH_NO, mast.CHK_NO);

                            logController.AddLogs(string.Format("處理MI_WHTRNS"));
                            int tempMiWhtrns = repo.InsertMiwhtrns(wh.WH_NO, mast.CHK_NO);

                            logController.AddLogs(string.Format("處理MI_WHINV"));
                            int tempMiWhinv = repo.UpdateMiwhinv(wh.WH_NO, mast.CHK_NO);
                        }

                        logController.AddLogs(string.Format("取得所有院內碼"));
                        IEnumerable<string> mmcodes = repo.GetChkDetailMmcodes(mast.CHK_NO);

                        foreach (string mmcode in mmcodes)
                        {
                            logController.AddLogs(string.Format("院內碼: {0}", mmcode));
                            logController.AddLogs(string.Format("處理MI_WEXPINV"));
                            // 檢查是否有批號效期資料
                            List<CHK_EXPLOC> exps = repo.GetExpinvs(mast.CHK_NO, wh.WH_NO, mmcode).ToList<CHK_EXPLOC>();
                            string use_qty = repo.GetUseQty(mast.CHK_NO, mmcode);
                            double use_qty_d = double.Parse(use_qty);

                            // 檢查電腦量與批號效期總量是否相符
                            string exp_diff = repo.GetExpInvDiff(mast.CHK_NO, mmcode);
                            if (exp_diff != "0")
                            {
                                // 不相符，修改9991231 TMPLOT
                                repo.MergeExpinv(wh.WH_NO, mmcode, exp_diff);
                            }

                            // 若為北投科室病房，多處理耗用的批號
                            if (hospCode == "818" && wh.WH_GRADE == "2" && wh.WH_KIND == "1")
                            {
                                // 重取
                                exps = repo.GetExpinvs(mast.CHK_NO, wh.WH_NO, mmcode).ToList<CHK_EXPLOC>();

                                // 取未過期資料
                                exps = exps.Where(x => x.IsExpired == "N").Select(x => x).ToList();

                                foreach (CHK_EXPLOC exp in exps)
                                {
                                    double ori_qty_d = double.Parse(exp.ORI_INV_QTY);
                                    if (ori_qty_d <= 0)
                                    {
                                        // 無存量，跳過
                                        continue;
                                    }

                                    if (ori_qty_d >= use_qty_d && use_qty_d > 0)
                                    {
                                        exp.TRN_QTY = use_qty;
                                        use_qty_d = use_qty_d - double.Parse(exp.TRN_QTY);
                                    }
                                    else if (ori_qty_d < use_qty_d && use_qty_d > 0)
                                    {
                                        exp.TRN_QTY = ori_qty_d.ToString();
                                        use_qty_d = use_qty_d - double.Parse(exp.TRN_QTY);
                                    }

                                    int tempChkExp = repo.InsertChkExpinvTrns(exp);
                                    if (repo.CheckMiwexpinvExists(exp))
                                    {
                                        repo.UpdateMiwexpinv(exp);
                                    }
                                    else
                                    {
                                        repo.InsertMiwexpinv(exp);
                                    }
                                }
                            }


                            logController.AddLogs(string.Format("處理MI_WLOCINV"));
                            // 檢查是否有儲位資料
                            List<CHK_EXPLOC> locs = repo.GetLocinvs(mast.CHK_NO, wh.WH_NO, mmcode).ToList<CHK_EXPLOC>();

                            // 檢查電腦量與儲位總量是否相符
                            string loc_diff = repo.GetLocInvDiff(mast.CHK_NO, mmcode);
                            if (loc_diff != "0")
                            {
                                // 不相符，修改TMPLOC
                                repo.MergeLocinv(wh.WH_NO, mmcode, loc_diff);
                            }

                            // 若為北投科室病房，多處理耗用的效期
                            if (hospCode == "818" && wh.WH_GRADE == "2" && wh.WH_KIND == "1")
                            {
                                // 重取
                                locs = repo.GetLocinvs(mast.CHK_NO, wh.WH_NO, mmcode).ToList<CHK_EXPLOC>();

                                use_qty_d = double.Parse(use_qty);

                                foreach (CHK_EXPLOC loc in locs)
                                {
                                    double ori_qty_d = double.Parse(loc.ORI_INV_QTY);
                                    if (ori_qty_d <= 0)
                                    {
                                        // 無存量，跳過
                                        continue;
                                    }

                                    if (ori_qty_d >= use_qty_d && use_qty_d > 0)
                                    {
                                        loc.TRN_QTY = use_qty;
                                        use_qty_d = use_qty_d - double.Parse(loc.TRN_QTY);
                                    }
                                    else if (ori_qty_d < use_qty_d && use_qty_d > 0)
                                    {
                                        loc.TRN_QTY = ori_qty_d.ToString();
                                        use_qty_d = use_qty_d - double.Parse(loc.TRN_QTY);
                                    }

                                    int tempChkExp = repo.InsertChkLocinvTrns(loc);
                                    if (repo.CheckMiwlocinvExists(loc))
                                    {
                                        repo.UpdateMiwlocinv(loc);
                                    }
                                    else
                                    {
                                        repo.InsertMiwlocinv(loc);
                                    }
                                }
                            }


                        }



                        //產生盤點單與明細-7.update master chk_mast
                        int temp2 = repo.UpdateMaster(mast);
                        logController.AddLogs(string.Format("更新master chk_num完成: {0}筆", temp2));

                        DBWork.Commit();
                    }
                    catch (Exception ex)
                    {
                        done = false;
                        logController.AddLogs(string.Format("error：{0}", ex.Message));
                        DBWork.Rollback();
                        logController.CreateLogFile();
                    }
                }

                // 補藥盤點轉檔
                try
                {
                    DBWork.BeginTransaction();
                    // 取得月結區間
                    COMBO_MODEL mnset = repo.GetMnset();
                    // 取得最後一筆資料
                    // 庫存量=最後一筆資料之使用單位現存量(INV_QTY)+核可補藥量(APVQTY)
                    //20240227: 電腦量 = 上次盤點結存量+本月核可補藥量
                    IEnumerable<DGMISS_ITEM> dgmiss_items = repo.GetDgmissItem(mnset.EXTRA1, mnset.EXTRA2, set_ym);
                    foreach (DGMISS_ITEM item in dgmiss_items)
                    {
                        item.IP = DBWork.ProcIP;
                        item.DATA_YM = mnset.VALUE;
                        int i = repo.InsertDgmissChk(item);
                    }
                    DBWork.Commit();
                }
                catch (Exception ex)
                {
                    done = false;
                    logController.AddLogs(string.Format("補藥 error：{0}", ex.Message));
                    DBWork.Rollback();
                    logController.CreateLogFile();
                }

                //20240216: 新增至CHK_DETAILTOT
                try
                {

                    //取得全部盤點單
                    IEnumerable<CE0040> datas_1 = repo.GetChkMast(set_ym);
                    foreach (CE0040 item_1 in datas_1)
                    {
                        DBWork.BeginTransaction();

                        try
                        {
                            item_1.CREATE_USER = "system";
                            item_1.UPDATE_USER = "system";

                            item_1.WH_NO = item_1.CHK_WH_NO;

                            //新增至CHK_DETAILTOT(結束全院盤點)
                            repo.AddChkDetailtot(item_1);

                            DBWork.Commit();

                        }
                        catch (Exception ex)
                        {
                            done = false;
                            logController.AddLogs(string.Format("CHK_DETAILTOT insert error：{0}", ex.Message));
                            DBWork.Rollback();
                            logController.CreateLogFile();
                        }
                    }



                }
                catch (Exception ex)
                {
                    done = false;
                    logController.AddLogs(string.Format("CHK_DETAILTOT error：{0}", ex.Message));
                    DBWork.Rollback();
                    logController.CreateLogFile();
                }

                if (done == true)
                {
                    //update CHK_MNSET + 新增CHK_MNSET
                    logController.AddLogs("產生單子無錯誤，將 update CHK_MNSET + insert CHK_MNSET");
                    //開單完成 更新CHK_MNSET狀態並新增
                    DBWork.BeginTransaction();
                    try
                    {
                        // 20240227 檢查是否有本月CHK_MNSET，有更新，沒有新增
                        if (repo.CheckChkmnsetExists(chk_no))
                        {
                            logController.AddLogs("有CHK_MNSET，update CHK_MNSET");
                            int temp3 = repo.UpdateChkmnset(chk_no);
                            logController.AddLogs(string.Format("update CHK_MNSET 完成: {0}筆", temp3));
                        }
                        else
                        {
                            logController.AddLogs("沒有CHK_MNSET，insert CHK_MNSET");
                            int temp = repo.InsertChkmnset(chk_no);
                            logController.AddLogs(string.Format("insert 下月CHK_MNSET 完成: {0}筆", temp));
                        }

                        DBWork.Commit();
                    }
                    catch (Exception ex)
                    {
                        logController.AddLogs(string.Format("error：{0}", ex.Message));
                        DBWork.Rollback();
                        logController.CreateLogFile();
                    }
                }
                else
                {
                    logController.AddLogs("產生單子有錯誤 不更新chk_mnset");
                }
                logController.CreateLogFile();
            }
        }

        private CHK_MAST GetChkMast(ChkWh chkwh)
        {
            return new CHK_MAST()
            {
                CHK_WH_NO = chkwh.WH_NO,
                CHK_WH_GRADE = chkwh.WH_GRADE,
                CHK_WH_KIND = chkwh.WH_KIND,
                CHK_CLASS = chkwh.WH_KIND == "1" ? "02" : "01",
                CHK_PERIOD = "M",
                CHK_TYPE = "X",
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

        private string GetCurrentSeq(string wh_no, string ym)
        {
            string maxNo = string.Empty;
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new Monthly24_14Repository(DBWork);
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
