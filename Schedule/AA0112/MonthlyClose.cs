using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Models;
using WebApp.Repository.C;


namespace AA0112
{
    public class MonthlyClose
    {
        LogConteoller logController = new LogConteoller("MonthlyClose",DateTime.Now.ToString("yyyy-MM"), "MonthlyClose");

        public void Run()
        {
            try
            {

                logController.AddLogs("開始執行月盤單鎖定");
                using (WorkSession session = new WorkSession())
                {
                    var DBWork = session.UnitOfWork;

                    MonthlyCloseRepository repo = new MonthlyCloseRepository(DBWork);

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

                    string chkym = GetChkym();
                    logController.AddLogs("取得民國年月結年月");
                    // 取得所有MASTER
                    IEnumerable<CHK_MAST> ori_masts = repo.GetUndoneMasts(chkym);
                    logController.AddLogs("取得所有MASTER");

                    IEnumerable<CHK_MAST_EXTEND> mast_extends = GetMastExtends(ori_masts);
                    logController.AddLogs(string.Format("取得所有轉換MASTER 共{0}個", mast_extends.Count()));

                    logController.AddLogs("foreach開始");
                    foreach (CHK_MAST_EXTEND extend in mast_extends)
                    {
                        logController.AddLogs(" ");
                        DBWork.BeginTransaction();
                        try
                        {
                            logController.AddLogs(string.Format("CHK_NO1:{0} MAX_CHK_LEVEL:{1} MAX_CHK_LEVEL_STATUS:{2}", extend.CHK_NO1, extend.MAX_CHK_LEVEL, extend.MAX_CHK_LEVEL_STATUS));
                            // 最後一次盤點狀態為P → 已過仗，不處理
                            if (extend.MAX_CHK_LEVEL_STATUS == "P")
                            {
                                logController.AddLogs("最後一次盤點狀態為P → 已過仗，不處理");
                                DBWork.Rollback();
                                continue;
                            }
                            // 最後一次盤點狀態為3 → 已鎖單(已完成盤點)，不處理
                            if (extend.MAX_CHK_LEVEL_STATUS == "3")
                            {
                                logController.AddLogs("最後一次盤點狀態為3 → 已鎖單(已完成盤點)，不處理");
                                DBWork.Rollback();
                                continue;
                            }
                            // 最後一次盤點狀態為0 → 盤點單未開立完成，刪除盤點單
                            if (extend.MAX_CHK_LEVEL_STATUS == "0")
                            {
                                logController.AddLogs("最後一次盤點狀態為0 → 盤點單未開立完成，刪除盤點單");
                                repo.DeleteChkDetail(extend.LAST_CHK_MAST.CHK_NO);
                                repo.DeleteChkDetailTemp(extend.LAST_CHK_MAST.CHK_NO);
                                repo.DeleteChkMast(extend.LAST_CHK_MAST.CHK_NO);

                                // 不為初盤，復原前一盤狀態
                                if (int.Parse(extend.LAST_CHK_MAST.CHK_LEVEL) > 1)
                                {
                                    logController.AddLogs("不為初盤，復原前一盤狀態");
                                    string preLevel = (int.Parse(extend.LAST_CHK_MAST.CHK_LEVEL) - 1).ToString();
                                    repo.UpdatePreMaster(extend.LAST_CHK_MAST.CHK_NO1, preLevel);
                                }

                                DBWork.Commit();

                                continue;
                            }

                            // 處理DETAIL狀態
                            logController.AddLogs("處理DETAIL狀態");
                            IEnumerable<CHK_DETAIL> details = repo.GetDetails(extend.LAST_CHK_MAST.CHK_NO);
                            foreach (CHK_DETAIL detail in details)
                            {
                                logController.AddLogs(string.Format("detail: chk_no: {0} mmcode: {1} status_ini: {2}", detail.CHK_NO, detail.MMCODE, detail.STATUS_INI));
                                // 已確認盤點量(status_ini = 2)：修改status_ini = 3
                                if (detail.STATUS_INI == "2")
                                {
                                    logController.AddLogs("已確認盤點量(status_ini = 2)：修改status_ini = 3");
                                    repo.UpdateChkDetail2(detail.CHK_NO, detail.MMCODE);
                                    continue;
                                }
                                // 未確認盤點量(status_ini = 1)
                                //      有輸入盤點量：以已輸入盤點量為主，修改status_ini = 3
                                //      未輸入盤點量：以當下庫存量為主，更新chk_qty、store_qty_n、store_qty_update_time、status_ini = 3

                                if (string.IsNullOrEmpty(detail.CHK_TIME) == false && string.IsNullOrEmpty(detail.CHK_QTY) == false)
                                {        //有輸入
                                    logController.AddLogs("有輸入盤點量(status_ini = 1)：修改status_ini = 3");
                                    repo.UpdateChkDetail2(detail.CHK_NO, detail.MMCODE);
                                    continue;
                                }
                                logController.AddLogs("以當下庫存量為主，更新chk_qty、store_qty_n、store_qty_update_time、status_ini = 3");
                                repo.UpdateChkDetail1(detail.CHK_NO, detail.WH_NO, detail.MMCODE);

                            }

                            // 寫入CHK_DETAIL_TOT
                            //      CHK_LEVEL = 1：初盤，insert
                            //      CHK_LEVEL = 2 or 3：複盤 or 三盤，update
                            logController.AddLogs("寫入CHK_DETAIL_TOT");
                            logController.AddLogs(string.Format("最終盤點階段: {0}", extend.MAX_CHK_LEVEL == "1" ? "初盤" : (extend.MAX_CHK_LEVEL == "2" ? "複盤" : "三盤")));
                            if (extend.MAX_CHK_LEVEL == "1")
                            {
                                logController.AddLogs("進入SetDetailtotLevel1");
                                SetDetailtotLevel1(DBWork, extend.LAST_CHK_MAST, chkym);
                            }
                            else
                            {
                                logController.AddLogs("進入SetDetailtotLevel23");
                                SetDetailtotLevel23(DBWork, extend.LAST_CHK_MAST, chkym);
                            }

                            logController.AddLogs("更新master chk_status = 3");
                            repo.UpdateMaster(extend.LAST_CHK_MAST.CHK_NO);
                            logController.AddLogs("更新master 完成");

                            DBWork.Commit();

                            
                        }
                        catch (Exception ex)
                        {
                            DBWork.Rollback();
                            //Console.Write(ex.Message);

                            logController.AddLogs(string.Format("error：{0}", ex.Message));
                            logController.AddLogs(string.Format("trace：{0}", ex.StackTrace));
                            logController.CreateLogFile();
                        }
                    }

                    logController.CreateLogFile();
                }
            }
            catch (Exception ex)
            {
                logController.AddLogs(string.Format("error：{0}", ex.Message));
                logController.AddLogs(string.Format("trace：{0}", ex.StackTrace));
                logController.CreateLogFile();
               // Console.Write(ex.Message);
            }
        }

        private bool IsCloseDay(string closeDayString)
        {
            DateTime closeDay = DateTime.Parse(closeDayString);
            logController.AddLogs(string.Format("轉換月結年月closeDay: {0}", closeDay.ToString()));

            logController.AddLogs(string.Format("是否同天: DateTime.Now:{0} closeDay:{1}", DateTime.Now.ToString("yyyy-MM-dd"), closeDay.ToString("yyyy-MM-dd")));
            return DateTime.Now.ToString("yyyy-MM-dd") == closeDay.ToString("yyyy-MM-dd");
        }

        private string GetChkym()
        {
            string yyy = (DateTime.Now.Year - 1911).ToString();
            int m = DateTime.Now.Month;
            string mm = m > 9 ? m.ToString() : m.ToString().PadLeft(2, '0');
            return string.Format("{0}{1}", yyy, mm);
        }

        private IEnumerable<CHK_MAST_EXTEND> GetMastExtends(IEnumerable<CHK_MAST> ori_masts)
        {
            List<CHK_MAST_EXTEND> list = new List<CHK_MAST_EXTEND>();
            var o = from a in ori_masts
                    group a by new
                    {
                        CHK_NO1 = a.CHK_NO1
                    } into g
                    select new CHK_MAST_EXTEND
                    {
                        CHK_NO1 = g.Key.CHK_NO1,
                        masts = g.ToList().OrderByDescending(x => x.CHK_LEVEL)
                    };

            list = o.ToList<CHK_MAST_EXTEND>();

            foreach (CHK_MAST_EXTEND item in list)
            {
                item.MAX_CHK_LEVEL = item.masts.FirstOrDefault().CHK_LEVEL;
                item.MAX_CHK_LEVEL_STATUS = item.masts.FirstOrDefault().CHK_STATUS;
                item.LAST_CHK_MAST = item.masts.FirstOrDefault();
            }

            return list;
        }

        private CHK_DETAILTOT GetGrade1Kind0Level1Tot(UnitOfWork DBWork, CHK_DETAILTOT tot, string chkym, CHK_MAST mast, MI_MAST mi_mast) {
            MonthlyCloseRepository repoBatch = new MonthlyCloseRepository(DBWork);
            CE0004Repository ce0004Repo = new CE0004Repository(DBWork);

            string spec_wh_no = ce0004Repo.GetSpecWhNo(mast.CHK_NO);

            tot.STORE_QTYC = ce0004Repo.GetSTORE_QTYC_PH1S(tot.CHK_NO, tot.MMCODE, "004" + tot.MMCODE.Substring(3, tot.MMCODE.Length - 3), spec_wh_no, mast.CHK_WH_GRADE);
            tot.STORE_QTYM = ce0004Repo.GetSTORE_QTYM(mast.CHK_NO, tot.MMCODE, "004" + tot.MMCODE.Substring(3, tot.MMCODE.Length - 3), spec_wh_no);
            tot.STORE_QTYS = "0";
            tot.STORE_QTY = tot.STORE_QTYC;
            logController.AddLogs(string.Format("mmcode: {0} STORE_QTYC: {1} STORE_QTYM: {2}, STORE_QTYS: {3}, STORE_QTY: {4}", tot.MMCODE, tot.STORE_QTYC, tot.STORE_QTYM, tot.STORE_QTYS, tot.STORE_QTY));

            tot.LAST_QTYC = ce0004Repo.GetLAST_QTYC(chkym, tot.MMCODE, tot.WH_NO);
            tot.LAST_QTYM = ce0004Repo.GetLAST_QTYM(mast.CHK_YM, "004" + tot.MMCODE.Substring(3, tot.MMCODE.Length - 3), spec_wh_no);
            tot.LAST_QTYS = "0";
            tot.LAST_QTY = tot.LAST_QTYC;
            logController.AddLogs(string.Format("mmcode: {0} LAST_QTYC: {1} LAST_QTYM: {2}, LAST_QTYS: {3}, LAST_QTY: {4}", tot.MMCODE, tot.LAST_QTYC, tot.LAST_QTYM, tot.LAST_QTYS, tot.LAST_QTY));

            tot.CHK_QTY1 = repoBatch.GetCHK_QTY(mast.CHK_NO, tot.MMCODE);    
            logController.AddLogs(string.Format("mmcode: {0} CHK_QTY1: {1}", tot.MMCODE, tot.CHK_QTY1));

            tot.GAP_T = Convert.ToString(Convert.ToDouble(tot.CHK_QTY1) - Convert.ToDouble(tot.STORE_QTYC));    // 藥庫盤的是醫院量 不含戰備
            logController.AddLogs(string.Format("mmcode: {0} GAP_T: {1}", tot.MMCODE, tot.GAP_T));
            tot.GAP_C = tot.GAP_T;
            tot.GAP_M = "0";
            tot.GAP_S = "0";
            logController.AddLogs(string.Format("mmcode: {0} GAP_C: {1}", tot.MMCODE, tot.GAP_C));

            tot.APL_OUTQTY = repoBatch.GetAPL_OUTQTY(tot.MMCODE, tot.WH_NO);
            logController.AddLogs(string.Format("mmcode: {0} APL_OUTQTY: {1}", tot.MMCODE, tot.APL_OUTQTY));

            return tot;
        }
        private CHK_DETAILTOT GetGrade1Kind1Level1Tot(UnitOfWork DBWork, CHK_DETAILTOT tot, string chkym, CHK_MAST mast, MI_MAST mi_mast)
        {
            MonthlyCloseRepository repoBatch = new MonthlyCloseRepository(DBWork);
            CE0004Repository ce0004Repo = new CE0004Repository(DBWork);

            string spec_wh_no = ce0004Repo.GetSpecWhNo(mast.CHK_NO);

            tot.STORE_QTYC = ce0004Repo.GetSTORE_QTYC_GRADE1(mast.CHK_NO, tot.MMCODE, tot.MMCODE, spec_wh_no, mast.CHK_WH_GRADE);
            tot.STORE_QTYM = ce0004Repo.GetSTORE_QTYM(mast.CHK_NO, tot.MMCODE, tot.MMCODE, spec_wh_no);
            tot.STORE_QTYS = "0";
            tot.STORE_QTY = Convert.ToString(Convert.ToDouble(tot.STORE_QTYC) + Convert.ToDouble(tot.STORE_QTYM) + Convert.ToDouble(tot.STORE_QTYS));
            logController.AddLogs(string.Format("mmcode: {0} STORE_QTYC: {1} STORE_QTYM: {2}, STORE_QTYS: {3}, STORE_QTY: {4}", tot.MMCODE, tot.STORE_QTYC, tot.STORE_QTYM, tot.STORE_QTYS, tot.STORE_QTY));

            tot.LAST_QTYC = ce0004Repo.GetLAST_QTYC(chkym, tot.MMCODE, tot.WH_NO);
            tot.LAST_QTYM = ce0004Repo.GetLAST_QTYM(mast.CHK_YM, tot.MMCODE, spec_wh_no);
            tot.LAST_QTYS = "0";
            tot.LAST_QTY = Convert.ToString(Convert.ToDouble(tot.LAST_QTYC) + Convert.ToDouble(tot.LAST_QTYM) + Convert.ToDouble(tot.LAST_QTYS));
            logController.AddLogs(string.Format("mmcode: {0} LAST_QTYC: {1} LAST_QTYM: {2}, LAST_QTYS: {3}, LAST_QTY: {4}", tot.MMCODE, tot.LAST_QTYC, tot.LAST_QTYM, tot.LAST_QTYS, tot.LAST_QTY));

            tot.CHK_QTY1 = repoBatch.GetCHK_QTY(mast.CHK_NO, tot.MMCODE);    // 盤點量
            logController.AddLogs(string.Format("mmcode: {0} CHK_QTY1: {1}", tot.MMCODE, tot.CHK_QTY1));
            tot.GAP_T = Convert.ToString(Convert.ToDouble(tot.CHK_QTY1) - Convert.ToDouble(tot.STORE_QTY));     
            logController.AddLogs(string.Format("mmcode: {0} GAP_T: {1}", tot.MMCODE, tot.GAP_T));
            tot.GAP_C = tot.GAP_T;                                          // 中央庫房盤點包含中央庫房與戰備
            logController.AddLogs(string.Format("mmcode: {0} GAP_C: {1}", tot.MMCODE, tot.GAP_C));
            tot.GAP_M = "0";
            tot.GAP_S = "0";

            tot.APL_OUTQTY = repoBatch.GetAPL_OUTQTY(tot.MMCODE, tot.WH_NO);
            logController.AddLogs(string.Format("mmcode: {0} APL_OUTQTY: {1}", tot.MMCODE, tot.APL_OUTQTY));
            
            return tot;
        }
        private CHK_DETAILTOT GetGrade1KindCELevel1Tot(UnitOfWork DBWork, CHK_DETAILTOT tot, string chkym, CHK_MAST mast, MI_MAST mi_mast)
        {
            MonthlyCloseRepository repoBatch = new MonthlyCloseRepository(DBWork);
            CE0004Repository ce0004Repo = new CE0004Repository(DBWork);

            string spec_wh_no = ce0004Repo.GetSpecWhNo(mast.CHK_NO);

            tot.STORE_QTYC = ce0004Repo.GetSTORE_QTYC_1(mast.CHK_NO, tot.MMCODE);
            tot.STORE_QTYM = ce0004Repo.GetSTORE_QTYM_1(mast.CHK_NO, tot.MMCODE);
            tot.STORE_QTYS = ce0004Repo.GetSTORE_QTYS_1(mast.CHK_NO, tot.MMCODE);
            tot.STORE_QTY = Convert.ToString(Convert.ToDouble(tot.STORE_QTYC) + Convert.ToDouble(tot.STORE_QTYM) + Convert.ToDouble(tot.STORE_QTYS));
            logController.AddLogs(string.Format("mmcode: {0} STORE_QTYC: {1} STORE_QTYM: {2}, STORE_QTYS: {3}, STORE_QTY: {4}", tot.MMCODE, tot.STORE_QTYC, tot.STORE_QTYM, tot.STORE_QTYS, tot.STORE_QTY));

            tot.LAST_QTYC = ce0004Repo.GetLAST_QTYC(mast.CHK_YM, tot.MMCODE, tot.WH_NO);
            tot.LAST_QTYM = ce0004Repo.GetLAST_QTYM_1(mast.CHK_YM, tot.MMCODE);
            tot.LAST_QTYS = ce0004Repo.GetLAST_QTYS_1(mast.CHK_YM, tot.MMCODE);
            tot.LAST_QTY = Convert.ToString(Convert.ToDouble(tot.LAST_QTYC) + Convert.ToDouble(tot.LAST_QTYM) + Convert.ToDouble(tot.LAST_QTYS));
            logController.AddLogs(string.Format("mmcode: {0} LAST_QTYC: {1} LAST_QTYM: {2}, LAST_QTYS: {3}, LAST_QTY: {4}", tot.MMCODE, tot.LAST_QTYC, tot.LAST_QTYM, tot.LAST_QTYS, tot.LAST_QTY));

            tot.CHK_QTY1 = repoBatch.GetCHK_QTY(mast.CHK_NO, tot.MMCODE);    // 盤點量
            logController.AddLogs(string.Format("mmcode: {0} CHK_QTY1: {1}", tot.MMCODE, tot.CHK_QTY1));
            tot.GAP_T = Convert.ToString(Convert.ToDouble(tot.CHK_QTY1) - Convert.ToDouble(tot.STORE_QTY));
            logController.AddLogs(string.Format("mmcode: {0} GAP_T: {1}", tot.MMCODE, tot.GAP_T));
            tot.GAP_C = tot.GAP_T;                                          // 能設通信盤點包含醫院、戰備、學院
            logController.AddLogs(string.Format("mmcode: {0} GAP_C: {1}", tot.MMCODE, tot.GAP_C));
            tot.GAP_M = "0";
            tot.GAP_S = "0";

            tot.APL_OUTQTY = repoBatch.GetAPL_OUTQTY(tot.MMCODE, tot.WH_NO);
            logController.AddLogs(string.Format("mmcode: {0} APL_OUTQTY: {1}", tot.MMCODE, tot.APL_OUTQTY));

            return tot;
        }
        private CHK_DETAILTOT GetGradeOtherLevel1Tot(UnitOfWork DBWork,  CHK_DETAILTOT tot, string chkym, CHK_MAST mast, MI_MAST mi_mast)
        {
            MonthlyCloseRepository repoBatch = new MonthlyCloseRepository(DBWork);

            tot.STORE_QTY = repoBatch.GetSTORE_QTY(tot.CHK_NO, tot.MMCODE, mast.CHK_WH_GRADE);
            logController.AddLogs(string.Format("mmcode: {0} STORE_QTY: {1}", tot.MMCODE, tot.STORE_QTY));
            tot.STORE_QTYC = tot.STORE_QTY;
            tot.STORE_QTYM = "0";
            tot.STORE_QTYS = "0";
            tot.LAST_QTYC = repoBatch.GetLAST_QTYC(chkym, tot.MMCODE, tot.WH_NO);
            logController.AddLogs(string.Format("mmcode: {0} LAST_QTYC: {1}", tot.MMCODE, tot.LAST_QTYC));
            tot.LAST_QTYC = tot.LAST_QTY;
            tot.LAST_QTYM = "0";
            tot.LAST_QTYS = "0";

            tot.CHK_QTY1 = repoBatch.GetCHK_QTY(tot.CHK_NO, tot.MMCODE);
            logController.AddLogs(string.Format("mmcode: {0} CHK_QTY1: {1}", tot.MMCODE, tot.CHK_QTY1));
            tot.GAP_T = Convert.ToString(Convert.ToDouble(tot.CHK_QTY1) - Convert.ToDouble(tot.STORE_QTY));
            logController.AddLogs(string.Format("mmcode: {0} GAP_T: {1}", tot.MMCODE, tot.GAP_T));
            tot.GAP_C = tot.GAP_T;                                          // 科室病房僅有醫院 無戰備與學院
            logController.AddLogs(string.Format("mmcode: {0} GAP_C: {1}", tot.MMCODE, tot.GAP_C));


            tot.APL_OUTQTY = repoBatch.GetAPL_OUTQTY(tot.MMCODE, tot.WH_NO);
            logController.AddLogs(string.Format("mmcode: {0} APL_OUTQTY: {1}", tot.MMCODE, tot.APL_OUTQTY));

            if (mi_mast.M_TRNID == "1")
            {
                logController.AddLogs(string.Format("mmcode: {0} 扣庫 算盤盈虧", tot.MMCODE));

                tot.PRO_LOS_QTY = Convert.ToString(tot.GAP_T);
                logController.AddLogs(string.Format("mmcode: {0} PRO_LOS_QTY: {1}", tot.MMCODE, tot.PRO_LOS_QTY));
                tot.PRO_LOS_AMOUNT = Convert.ToString(Convert.ToDouble(tot.PRO_LOS_QTY) * Convert.ToDouble(tot.M_CONTPRICE));
                logController.AddLogs(string.Format("mmcode: {0} PRO_LOS_AMOUNT: {1}", tot.MMCODE, tot.PRO_LOS_AMOUNT));

                if (tot.APL_OUTQTY != "0" && tot.APL_OUTQTY != null)
                {
                    logController.AddLogs(string.Format("mmcode: {0} APL_OUTQTY != 0 & APL_OUTQTY != null 計算誤差量", tot.MMCODE));
                    tot.MISS_PER = Convert.ToString(Convert.ToDouble(tot.GAP_T) / Convert.ToDouble(tot.APL_OUTQTY));
                    logController.AddLogs(string.Format("mmcode: {0} MISS_PER: {1}", tot.MMCODE, tot.MISS_PER));
                    tot.MISS_PERC = Convert.ToString(Convert.ToDouble(tot.GAP_C) / Convert.ToDouble(tot.APL_OUTQTY));
                    logController.AddLogs(string.Format("mmcode: {0} MISS_PERC: {1}", tot.MMCODE, tot.MISS_PERC));
                }
                else
                {
                    tot.APL_OUTQTY = "0";
                    logController.AddLogs(string.Format("mmcode: {0} APL_OUTQTY = 0 || APL_OUTQTY = null 誤差量無法計算 設為0", tot.MMCODE));
                    tot.MISS_PER = "0";
                    logController.AddLogs(string.Format("mmcode: {0} MISS_PER: {1}", tot.MMCODE, tot.MISS_PER));
                    tot.MISS_PERC = "0";
                    logController.AddLogs(string.Format("mmcode: {0} MISS_PERC: {1}", tot.MMCODE, tot.MISS_PERC));
                }
            }
            if (mi_mast.M_TRNID == "2")
            {
                logController.AddLogs(string.Format("mmcode: {0} 不扣庫 算消耗", tot.MMCODE));

                tot.CONSUME_QTY = Convert.ToDouble(tot.GAP_T).ToString();
                logController.AddLogs(string.Format("mmcode: {0} CONSUME_QTY: {1}", tot.MMCODE, tot.CONSUME_QTY));
                tot.CONSUME_AMOUNT = Convert.ToString(Convert.ToDouble(tot.CONSUME_QTY) * Convert.ToDouble(tot.M_CONTPRICE));
                logController.AddLogs(string.Format("mmcode: {0} CONSUME_AMOUNT: {1}", tot.MMCODE, tot.CONSUME_AMOUNT));
                tot.MISS_PER = "0";
                logController.AddLogs(string.Format("mmcode: {0} MISS_PER: {1}", tot.MMCODE, tot.MISS_PER));
                tot.MISS_PERC = "0";
                logController.AddLogs(string.Format("mmcode: {0} MISS_PERC: {1}", tot.MMCODE, tot.MISS_PERC));
            }


            return tot;
        }

        private int SetDetailtotLevel1(UnitOfWork DBWork, CHK_MAST mast, string chkym)
        {
            logController.AddLogs(string.Format("chk_no: {0}", mast.CHK_NO));
            try
            {
                int afrs = 0;
                MonthlyCloseRepository repoBatch = new MonthlyCloseRepository(DBWork);

                logController.AddLogs("取得所有detail");
                IEnumerable<CHK_DETAIL> details = repoBatch.GetDetails(mast.CHK_NO);
                logController.AddLogs(string.Format("共 {0} 個", details.Count()));

                foreach (CHK_DETAIL detail in details)
                {
                    logController.AddLogs(string.Format("mmcode: {0}", detail.MMCODE));
                    CHK_DETAILTOT tot = new CHK_DETAILTOT();
                    tot.CHK_NO = mast.CHK_NO;
                    tot.MMCODE = detail.MMCODE;

                    MI_MAST mi_mast = repoBatch.GetMiMast(detail.MMCODE);
                    logController.AddLogs(string.Format("mmcode: {0} M_CONTPRICE:{1}", detail.MMCODE, mi_mast.M_CONTPRICE));
                    if (detail.M_CONTPRICE == null || detail.M_CONTPRICE == string.Empty)
                    {
                        logController.AddLogs(string.Format("mmcode: {0} M_CONTPRICE為null 改為0", detail.MMCODE));
                        detail.M_CONTPRICE = "0";
                    }
                    tot.STATUS_TOT = "1";


                    // 一級庫(中央庫房、藥庫、藥局、能設通信)都算盤盈虧
                    if (mast.CHK_WH_GRADE == "1")
                    {
                        if (mast.CHK_WH_KIND == "0")    // 藥品庫
                        {
                            tot = GetGrade1Kind0Level1Tot(DBWork, tot, mast.CHK_YM, mast, mi_mast);
                        }
                        else if (mast.CHK_WH_KIND == "1")   //  衛材庫
                        {
                            tot = GetGrade1Kind1Level1Tot(DBWork, tot, mast.CHK_YM, mast, mi_mast);
                        }
                        else {          //能設通信
                            tot = GetGrade1KindCELevel1Tot(DBWork, tot, mast.CHK_YM, mast, mi_mast);
                        }

                        logController.AddLogs(string.Format("mmcode: {0} 一級庫 算盤盈虧", detail.MMCODE));

                        tot.PRO_LOS_QTY = tot.GAP_T;
                        logController.AddLogs(string.Format("mmcode: {0} PRO_LOS_QTY: {1}", tot.MMCODE, tot.PRO_LOS_QTY));
                        tot.PRO_LOS_AMOUNT = Convert.ToString(Convert.ToDouble(tot.PRO_LOS_QTY) * Convert.ToDouble(detail.M_CONTPRICE));
                        logController.AddLogs(string.Format("mmcode: {0} PRO_LOS_AMOUNT: {1}", tot.MMCODE, tot.PRO_LOS_AMOUNT));

                        if (tot.APL_OUTQTY != "0" && tot.APL_OUTQTY != null)
                        {
                            logController.AddLogs(string.Format("mmcode: {0} APL_OUTQTY != 0 & APL_OUTQTY != null 計算誤差量", detail.MMCODE));
                            tot.MISS_PER = Convert.ToString(Convert.ToDouble(tot.GAP_T) / Convert.ToDouble(tot.APL_OUTQTY));
                            logController.AddLogs(string.Format("mmcode: {0} MISS_PER: {1}", detail.MMCODE, tot.MISS_PER));
                            tot.MISS_PERC = Convert.ToString(Convert.ToDouble(tot.GAP_T) / Convert.ToDouble(tot.APL_OUTQTY));
                            logController.AddLogs(string.Format("mmcode: {0} MISS_PERC: {1}", detail.MMCODE, tot.MISS_PERC));
                        }
                        else
                        {
                            logController.AddLogs(string.Format("mmcode: {0} APL_OUTQTY = 0 || APL_OUTQTY = null 誤差量無法計算 設為0", detail.MMCODE));
                            tot.MISS_PER = "0";
                            logController.AddLogs(string.Format("mmcode: {0} MISS_PER: {1}", detail.MMCODE, tot.MISS_PER));
                            tot.MISS_PERC = "0";
                            logController.AddLogs(string.Format("mmcode: {0} MISS_PERC: {1}", detail.MMCODE, tot.MISS_PERC));
                        }
                    }
                    else
                    {  
                        tot = GetGradeOtherLevel1Tot(DBWork, tot, mast.CHK_YM, mast, mi_mast);
                    }

                    tot.CREATE_USER = "BATCH";
                    tot.UPDATE_USER = "BATCH";
                    tot.UPDATE_IP = null;
                    logController.AddLogs(string.Format("新增 mmcode: {0} 到chk_detailtot", detail.MMCODE));
                    afrs += repoBatch.InsertDetailTot(tot);
                    logController.AddLogs(string.Format("新增 mmcode: {0} 到chk_detailtot 成功", detail.MMCODE));
                }
                return afrs;
            }
            catch (Exception ex)
            {
                logController.AddLogs(string.Format("新增到chk_detailtot 失敗"));
                logController.AddLogs(string.Format("error: {0}", ex.Message));
                logController.AddLogs(string.Format("trace: {0}", ex.StackTrace));
                throw;
            }

        }


        private CHK_DETAILTOT GetGrade1Kind0Level23Tot(UnitOfWork DBWork, CHK_DETAILTOT tot, string chkym, CHK_MAST mast, MI_MAST mi_mast)
        {
            MonthlyCloseRepository repoBatch = new MonthlyCloseRepository(DBWork);
            CE0004Repository ce0004Repo = new CE0004Repository(DBWork);

            string spec_wh_no = ce0004Repo.GetSpecWhNo(mast.CHK_NO);

            tot.STORE_QTYC = ce0004Repo.GetSTORE_QTYC_PH1S(mast.CHK_NO, tot.MMCODE, "004" + tot.MMCODE.Substring(3, tot.MMCODE.Length - 3), spec_wh_no, mast.CHK_WH_GRADE);
            tot.STORE_QTYM = ce0004Repo.GetSTORE_QTYM(mast.CHK_NO, tot.MMCODE, "004" + tot.MMCODE.Substring(3, tot.MMCODE.Length - 3), spec_wh_no);
            tot.STORE_QTYS = "0";
            tot.STORE_QTY = tot.STORE_QTYC;

            if (mast.CHK_LEVEL == "2")
            {
                tot.STATUS_TOT = "2";
                tot.CHK_QTY2 = repoBatch.GetCHK_QTY(mast.CHK_NO, tot.MMCODE);    // 盤點量
                tot.GAP_T = Convert.ToString(Convert.ToDouble(tot.CHK_QTY2) - Convert.ToDouble(tot.STORE_QTYC));
                tot.GAP_C = tot.GAP_T;
                logController.AddLogs(string.Format("mmcode: {0} CHK_QTY2: {1}", tot.MMCODE, tot.CHK_QTY2));

                tot.PRO_LOS_QTY = tot.GAP_T;
                logController.AddLogs(string.Format("mmcode: {0} PRO_LOS_QTY: {1}", tot.MMCODE, tot.PRO_LOS_QTY));

            }
            else if (mast.CHK_LEVEL == "3")
            {
                tot.STATUS_TOT = "3";
                tot.CHK_QTY3 = repoBatch.GetCHK_QTY(mast.CHK_NO, tot.MMCODE);    // 盤點量
                tot.GAP_T = Convert.ToString(Convert.ToDouble(tot.CHK_QTY3) - Convert.ToDouble(tot.STORE_QTYC));
                tot.GAP_C = tot.GAP_T;
                logController.AddLogs(string.Format("mmcode: {0} CHK_QTY3: {1}", tot.MMCODE, tot.CHK_QTY3));

                tot.PRO_LOS_QTY = tot.GAP_T;
                logController.AddLogs(string.Format("mmcode: {0} PRO_LOS_QTY: {1}", tot.MMCODE, tot.PRO_LOS_QTY));
            }

            tot.APL_OUTQTY = repoBatch.GetAPL_OUTQTY(tot.MMCODE, tot.WH_NO);
            logController.AddLogs(string.Format("mmcode: {0} APL_OUTQTY: {1}", tot.MMCODE, tot.APL_OUTQTY));
            tot.PRO_LOS_AMOUNT = Convert.ToString(Convert.ToDouble(tot.PRO_LOS_QTY) * Convert.ToDouble(tot.M_CONTPRICE));
            logController.AddLogs(string.Format("mmcode: {0} PRO_LOS_AMOUNT: {1}", tot.MMCODE, tot.PRO_LOS_AMOUNT));

            if (tot.APL_OUTQTY != "0" && tot.APL_OUTQTY != null)
            {
                logController.AddLogs(string.Format("mmcode: {0} APL_OUTQTY != 0 & APL_OUTQTY != null 計算誤差量", tot.MMCODE));
                tot.MISS_PER = Convert.ToString(Convert.ToDouble(tot.GAP_T) / Convert.ToDouble(tot.APL_OUTQTY));
                logController.AddLogs(string.Format("mmcode: {0} MISS_PER: {1}", tot.MMCODE, tot.MISS_PER));
                tot.MISS_PERC = Convert.ToString(Convert.ToDouble(tot.GAP_T) / Convert.ToDouble(tot.APL_OUTQTY));
                logController.AddLogs(string.Format("mmcode: {0} MISS_PERC: {1}", tot.MMCODE, tot.MISS_PERC));
            }
            else
            {
                logController.AddLogs(string.Format("mmcode: {0} APL_OUTQTY = 0 || APL_OUTQTY = null 誤差量無法計算 設為0", tot.MMCODE));
                tot.MISS_PER = "0";
                logController.AddLogs(string.Format("mmcode: {0} MISS_PER: {1}", tot.MMCODE, tot.MISS_PER));
                tot.MISS_PERC = "0";
                logController.AddLogs(string.Format("mmcode: {0} MISS_PERC: {1}", tot.MMCODE, tot.MISS_PERC));
            }

            return tot;
        }
        private CHK_DETAILTOT GetGrade1Kind1Level23Tot(UnitOfWork DBWork, CHK_DETAILTOT tot, string chkym, CHK_MAST mast, MI_MAST mi_mast)
        {
            MonthlyCloseRepository repoBatch = new MonthlyCloseRepository(DBWork);
            CE0004Repository ce0004Repo = new CE0004Repository(DBWork);
            
            string spec_wh_no = ce0004Repo.GetSpecWhNo(mast.CHK_NO);

            tot.STORE_QTYC = ce0004Repo.GetSTORE_QTYC_GRADE1(mast.CHK_NO, tot.MMCODE, tot.MMCODE, spec_wh_no, mast.CHK_WH_GRADE);
            tot.STORE_QTYM = ce0004Repo.GetSTORE_QTYM(mast.CHK_NO, tot.MMCODE, tot.MMCODE, spec_wh_no);
            tot.STORE_QTYS = "0";
            tot.STORE_QTY = Convert.ToString(Convert.ToDouble(tot.STORE_QTYC) + Convert.ToDouble(tot.STORE_QTYM) + Convert.ToDouble(tot.STORE_QTYS));
            logController.AddLogs(string.Format("mmcode: {0} LAST_QTYM: {1}", tot.MMCODE, tot.LAST_QTYC));

            if (mast.CHK_LEVEL == "2")
            {
                tot.STATUS_TOT = "2";
                tot.CHK_QTY2 = repoBatch.GetCHK_QTY(mast.CHK_NO, tot.MMCODE);    // 盤點量
                tot.GAP_T = Convert.ToString(Convert.ToDouble(tot.CHK_QTY2) - Convert.ToDouble(tot.STORE_QTY));
                tot.GAP_C = tot.GAP_T;
                logController.AddLogs(string.Format("mmcode: {0} CHK_QTY2: {1}", tot.MMCODE, tot.CHK_QTY2));

                tot.PRO_LOS_QTY = tot.GAP_T;
                logController.AddLogs(string.Format("mmcode: {0} PRO_LOS_QTY: {1}", tot.MMCODE, tot.PRO_LOS_QTY));

            }
            else if (mast.CHK_LEVEL == "3")
            {
                tot.STATUS_TOT = "3";
                tot.CHK_QTY3 = repoBatch.GetCHK_QTY(mast.CHK_NO, tot.MMCODE);    // 盤點量
                tot.GAP_T = Convert.ToString(Convert.ToDouble(tot.CHK_QTY3) - Convert.ToDouble(tot.STORE_QTY));
                tot.GAP_C = tot.GAP_T;
                logController.AddLogs(string.Format("mmcode: {0} CHK_QTY3: {1}", tot.MMCODE, tot.CHK_QTY3));

                tot.PRO_LOS_QTY = tot.GAP_T;
                logController.AddLogs(string.Format("mmcode: {0} PRO_LOS_QTY: {1}", tot.MMCODE, tot.PRO_LOS_QTY));
            }

            tot.APL_OUTQTY = repoBatch.GetAPL_OUTQTY(tot.MMCODE, tot.WH_NO);
            logController.AddLogs(string.Format("mmcode: {0} APL_OUTQTY: {1}", tot.MMCODE, tot.APL_OUTQTY));
            tot.PRO_LOS_AMOUNT = Convert.ToString(Convert.ToDouble(tot.PRO_LOS_QTY) * Convert.ToDouble(tot.M_CONTPRICE));
            logController.AddLogs(string.Format("mmcode: {0} PRO_LOS_AMOUNT: {1}", tot.MMCODE, tot.PRO_LOS_AMOUNT));

            if (tot.APL_OUTQTY != "0" && tot.APL_OUTQTY != null)
            {
                logController.AddLogs(string.Format("mmcode: {0} APL_OUTQTY != 0 & APL_OUTQTY != null 計算誤差量", tot.MMCODE));
                tot.MISS_PER = Convert.ToString(Convert.ToDouble(tot.GAP_T) / Convert.ToDouble(tot.APL_OUTQTY));
                logController.AddLogs(string.Format("mmcode: {0} MISS_PER: {1}", tot.MMCODE, tot.MISS_PER));
                tot.MISS_PERC = Convert.ToString(Convert.ToDouble(tot.GAP_T) / Convert.ToDouble(tot.APL_OUTQTY));
                logController.AddLogs(string.Format("mmcode: {0} MISS_PERC: {1}", tot.MMCODE, tot.MISS_PERC));
            }
            else
            {
                logController.AddLogs(string.Format("mmcode: {0} APL_OUTQTY = 0 || APL_OUTQTY = null 誤差量無法計算 設為0", tot.MMCODE));
                tot.MISS_PER = "0";
                logController.AddLogs(string.Format("mmcode: {0} MISS_PER: {1}", tot.MMCODE, tot.MISS_PER));
                tot.MISS_PERC = "0";
                logController.AddLogs(string.Format("mmcode: {0} MISS_PERC: {1}", tot.MMCODE, tot.MISS_PERC));
            }

            return tot;
        }
        private CHK_DETAILTOT GetGrade1KindCELevel23Tot(UnitOfWork DBWork, CHK_DETAILTOT tot, string chkym, CHK_MAST mast, MI_MAST mi_mast)
        {
            MonthlyCloseRepository repoBatch = new MonthlyCloseRepository(DBWork);
            CE0004Repository ce0004Repo = new CE0004Repository(DBWork);

            string spec_wh_no = ce0004Repo.GetSpecWhNo(mast.CHK_NO);

            tot.STORE_QTYC = ce0004Repo.GetSTORE_QTYC_1(mast.CHK_NO, tot.MMCODE);
            tot.STORE_QTYM = ce0004Repo.GetSTORE_QTYM_1(mast.CHK_NO, tot.MMCODE);
            tot.STORE_QTYS = ce0004Repo.GetSTORE_QTYS_1(mast.CHK_NO, tot.MMCODE);
            tot.STORE_QTY = Convert.ToString(Convert.ToDouble(tot.STORE_QTYC) + Convert.ToDouble(tot.STORE_QTYM) + Convert.ToDouble(tot.STORE_QTYS));

            if (mast.CHK_LEVEL == "2")
            {
                tot.STATUS_TOT = "2";
                tot.CHK_QTY2 = repoBatch.GetCHK_QTY(mast.CHK_NO, tot.MMCODE);    // 盤點量
                tot.GAP_T = Convert.ToString(Convert.ToDouble(tot.CHK_QTY2) - Convert.ToDouble(tot.STORE_QTY));
                tot.GAP_C = Convert.ToString(tot.GAP_T);
                logController.AddLogs(string.Format("mmcode: {0} CHK_QTY2: {1}", tot.MMCODE, tot.CHK_QTY2));
            }
            else if (mast.CHK_LEVEL == "3")
            {
                tot.STATUS_TOT = "3";
                tot.CHK_QTY3 = repoBatch.GetCHK_QTY(mast.CHK_NO, tot.MMCODE);    // 盤點量
                tot.GAP_T = Convert.ToString(Convert.ToDouble(tot.CHK_QTY3) - Convert.ToDouble(tot.STORE_QTY));
                tot.GAP_C = Convert.ToString(tot.GAP_T);
                logController.AddLogs(string.Format("mmcode: {0} CHK_QTY3: {1}", tot.MMCODE, tot.CHK_QTY3));
            }

            tot.PRO_LOS_QTY = tot.GAP_T;
            logController.AddLogs(string.Format("mmcode: {0} PRO_LOS_QTY: {1}", tot.MMCODE, tot.PRO_LOS_QTY));
            tot.APL_OUTQTY = repoBatch.GetAPL_OUTQTY(tot.MMCODE, tot.WH_NO);
            logController.AddLogs(string.Format("mmcode: {0} APL_OUTQTY: {1}", tot.MMCODE, tot.APL_OUTQTY));
            tot.PRO_LOS_AMOUNT = Convert.ToString(Convert.ToDouble(tot.PRO_LOS_QTY) * Convert.ToDouble(tot.M_CONTPRICE));
            logController.AddLogs(string.Format("mmcode: {0} PRO_LOS_AMOUNT: {1}", tot.MMCODE, tot.PRO_LOS_AMOUNT));

            if (tot.APL_OUTQTY != "0" && tot.APL_OUTQTY != null)
            {
                logController.AddLogs(string.Format("mmcode: {0} APL_OUTQTY != 0 & APL_OUTQTY != null 計算誤差量", tot.MMCODE));
                tot.MISS_PER = Convert.ToString(Convert.ToDouble(tot.PRO_LOS_QTY) / Convert.ToDouble(tot.APL_OUTQTY));
                logController.AddLogs(string.Format("mmcode: {0} MISS_PER: {1}", tot.MMCODE, tot.MISS_PER));
                tot.MISS_PERC = Convert.ToString(Convert.ToDouble(tot.PRO_LOS_QTY) / Convert.ToDouble(tot.APL_OUTQTY));
                logController.AddLogs(string.Format("mmcode: {0} MISS_PERC: {1}", tot.MMCODE, tot.MISS_PERC));
            }
            else
            {
                logController.AddLogs(string.Format("mmcode: {0} APL_OUTQTY = 0 || APL_OUTQTY = null 誤差量無法計算 設為0", tot.MMCODE));
                tot.MISS_PER = "0";
                logController.AddLogs(string.Format("mmcode: {0} MISS_PER: {1}", tot.MMCODE, tot.MISS_PER));
                tot.MISS_PERC = "0";
                logController.AddLogs(string.Format("mmcode: {0} MISS_PERC: {1}", tot.MMCODE, tot.MISS_PERC));
            }

            return tot;
        }
        private CHK_DETAILTOT GetGradeOtherLevel23Tot(UnitOfWork DBWork, CHK_DETAILTOT tot, string chkym, CHK_MAST mast, MI_MAST mi_mast)
        {
            MonthlyCloseRepository repoBatch = new MonthlyCloseRepository(DBWork);

            tot.STORE_QTYC = tot.STORE_QTY;
            tot.STORE_QTYM = "0";
            tot.STORE_QTYS = "0";
            logController.AddLogs(string.Format("mmcode: {0} LAST_QTYC: {1}", tot.MMCODE, tot.LAST_QTYC));
            tot.APL_OUTQTY = repoBatch.GetAPL_OUTQTY(tot.MMCODE, tot.WH_NO);
            logController.AddLogs(string.Format("mmcode: {0} APL_OUTQTY: {1}", tot.MMCODE, tot.APL_OUTQTY));

            if (mast.CHK_LEVEL == "2")
            {
                tot.CHK_QTY2 = repoBatch.GetCHK_QTY(tot.CHK_NO, tot.MMCODE);
                logController.AddLogs(string.Format("mmcode: {0} CHK_QTY2: {1}", tot.MMCODE, tot.CHK_QTY2));
                tot.GAP_T = Convert.ToString(Convert.ToDouble(tot.CHK_QTY2) - Convert.ToDouble(tot.STORE_QTY));
                logController.AddLogs(string.Format("mmcode: {0} GAP_T: {1}", tot.MMCODE, tot.GAP_T));
                tot.GAP_C = Convert.ToString(tot.GAP_T);
                logController.AddLogs(string.Format("mmcode: {0} GAP_C: {1}", tot.MMCODE, tot.GAP_C));
            }
            else {
                tot.CHK_QTY3 = repoBatch.GetCHK_QTY(tot.CHK_NO, tot.MMCODE);
                logController.AddLogs(string.Format("mmcode: {0} CHK_QTY3: {1}", tot.MMCODE, tot.CHK_QTY3));
                tot.GAP_T = Convert.ToString(Convert.ToDouble(tot.CHK_QTY3) - Convert.ToDouble(tot.STORE_QTY));
                logController.AddLogs(string.Format("mmcode: {0} GAP_T: {1}", tot.MMCODE, tot.GAP_T));
                tot.GAP_C = Convert.ToString(tot.GAP_T);
                logController.AddLogs(string.Format("mmcode: {0} GAP_C: {1}", tot.MMCODE, tot.GAP_C));
            }

            if (mi_mast.M_TRNID == "1")
            {
                logController.AddLogs(string.Format("mmcode: {0} 扣庫 算盤盈虧", tot.MMCODE));

                //tot.PRO_LOS_QTY = Convert.ToString(Convert.ToDouble(tot.CHK_QTY1) - Convert.ToDouble(tot.STORE_QTY));
                tot.PRO_LOS_QTY = tot.GAP_T;
                logController.AddLogs(string.Format("mmcode: {0} PRO_LOS_QTY: {1}", tot.MMCODE, tot.PRO_LOS_QTY));
                tot.PRO_LOS_AMOUNT = Convert.ToString(Convert.ToDouble(tot.PRO_LOS_QTY) * Convert.ToDouble(tot.M_CONTPRICE));
                logController.AddLogs(string.Format("mmcode: {0} PRO_LOS_AMOUNT: {1}", tot.MMCODE, tot.PRO_LOS_AMOUNT));

                if (tot.APL_OUTQTY != "0" && tot.APL_OUTQTY != null)
                {
                    logController.AddLogs(string.Format("mmcode: {0} APL_OUTQTY != 0 & APL_OUTQTY != null 計算誤差量", tot.MMCODE));
                    tot.MISS_PER = Convert.ToString(Convert.ToDouble(tot.PRO_LOS_QTY) / Convert.ToDouble(tot.APL_OUTQTY));
                    logController.AddLogs(string.Format("mmcode: {0} MISS_PER: {1}", tot.MMCODE, tot.MISS_PER));
                    tot.MISS_PERC = Convert.ToString(Convert.ToDouble(tot.PRO_LOS_QTY) / Convert.ToDouble(tot.APL_OUTQTY));
                    logController.AddLogs(string.Format("mmcode: {0} MISS_PERC: {1}", tot.MMCODE, tot.MISS_PERC));
                }
                else
                {
                    logController.AddLogs(string.Format("mmcode: {0} APL_OUTQTY = 0 || APL_OUTQTY = null 誤差量無法計算 設為0", tot.MMCODE));
                    tot.MISS_PER = "0";
                    logController.AddLogs(string.Format("mmcode: {0} MISS_PER: {1}", tot.MMCODE, tot.MISS_PER));
                    tot.MISS_PERC = "0";
                    logController.AddLogs(string.Format("mmcode: {0} MISS_PERC: {1}", tot.MMCODE, tot.MISS_PERC));
                }
            }
            if (mi_mast.M_TRNID == "2")
            {
                logController.AddLogs(string.Format("mmcode: {0} 不扣庫 算消耗", tot.MMCODE));

                tot.CONSUME_QTY = Convert.ToDouble(tot.GAP_T).ToString();
                logController.AddLogs(string.Format("mmcode: {0} CONSUME_QTY: {1}", tot.MMCODE, tot.CONSUME_QTY));
                tot.CONSUME_AMOUNT = Convert.ToString(Convert.ToDouble(tot.CONSUME_QTY) * Convert.ToDouble(tot.M_CONTPRICE));
                logController.AddLogs(string.Format("mmcode: {0} CONSUME_AMOUNT: {1}", tot.MMCODE, tot.CONSUME_AMOUNT));
                tot.MISS_PER = "0";
                logController.AddLogs(string.Format("mmcode: {0} MISS_PER: {1}", tot.MMCODE, tot.MISS_PER));
                tot.MISS_PERC = "0";
                logController.AddLogs(string.Format("mmcode: {0} MISS_PERC: {1}", tot.MMCODE, tot.MISS_PERC));
            }


            return tot;
        }


        private int SetDetailtotLevel23(UnitOfWork DBWork, CHK_MAST mast, string chkym)
        {
            logController.AddLogs(string.Format("chk_no: {0}", mast.CHK_NO));
            try
            {
                int afrs = 0;
                MonthlyCloseRepository repoBatch = new MonthlyCloseRepository(DBWork);

                logController.AddLogs("取得所有detail");
                IEnumerable<CHK_DETAIL> details = repoBatch.GetDetails(mast.CHK_NO);
                logController.AddLogs(string.Format("共 {0} 個", details.Count()));

                foreach (CHK_DETAIL detail in details)
                {
                    logController.AddLogs(string.Format("mmcode: {0}", detail.MMCODE));
                    CHK_DETAILTOT tot = new CHK_DETAILTOT();
                    tot.CHK_NO = mast.CHK_NO;
                    tot.MMCODE = detail.MMCODE;
                    tot.STORE_QTY = detail.STORE_QTY_N;

                    MI_MAST mi_mast = repoBatch.GetMiMast(detail.MMCODE);

                    tot.MMCODE = detail.MMCODE;
                    tot.M_CONTPRICE = mi_mast.M_CONTPRICE == "" ? "0" : mi_mast.M_CONTPRICE;
                    tot.WH_NO = mast.CHK_WH_NO;


                    if (mast.CHK_WH_GRADE == "1")
                    {
                        if (mast.CHK_WH_KIND == "0")    // 藥品庫
                        {
                            tot = GetGrade1Kind0Level23Tot(DBWork, tot, mast.CHK_YM, mast, mi_mast);
                        }
                        else if (mast.CHK_WH_KIND == "1")   //  衛材庫
                        {
                            tot = GetGrade1Kind1Level23Tot(DBWork, tot, mast.CHK_YM, mast, mi_mast);
                        }
                        else
                        {          //能設通信
                            tot = GetGrade1KindCELevel23Tot(DBWork, tot, mast.CHK_YM, mast, mi_mast);
                        }
                    }
                    else {
                        tot = GetGradeOtherLevel23Tot(DBWork, tot, mast.CHK_YM, mast, mi_mast);
                    }

                    tot.CHK_NO = mast.CHK_NO1;
                    tot.UPDATE_USER = "BATCH";
                    tot.UPDATE_IP = null;

                    // 不存在CHK_DETAILTOT ：自行新增品項(科室病房)，需新增
                    if (repoBatch.IsExists(mast.CHK_NO1, detail.MMCODE) == false)
                    {
                        if (tot.STATUS_TOT == "2")
                        {
                            tot.CHK_QTY1 = "0";
                            tot.CHK_QTY3 = "0";
                        }
                        else {
                            tot.CHK_QTY1 = "0";
                            tot.CHK_QTY2 = "0";
                        }
                        logController.AddLogs(string.Format("新增 mmcode: {0} 到chk_detailtot", detail.MMCODE));
                        afrs += repoBatch.InsertDetailTot23(tot, mast.CHK_NO1, mast.CHK_NO);
                        logController.AddLogs(string.Format("新增 mmcode: {0} 到chk_detailtot 成功", detail.MMCODE));
                    }
                    else {
                        logController.AddLogs(string.Format("更新 chk_no: {1}(chk_no1:{2}) mmcode: {0} 到chk_detailtot", detail.MMCODE, mast.CHK_NO, mast.CHK_NO1));
                        afrs += repoBatch.UpdateDetailTot(tot, mast);
                        logController.AddLogs(string.Format("更新 chk_no: {1}(chk_no1:{2}) mmcode: {0} 到chk_detailtot 成功", detail.MMCODE, mast.CHK_NO, mast.CHK_NO1));
                    }
                }
                return afrs;
            }
            catch(Exception ex)
            {
                logController.AddLogs(string.Format("更新chk_detailtot 失敗"));
                logController.AddLogs(string.Format("error: {0}", ex.Message));
                logController.AddLogs(string.Format("trace: {0}", ex.StackTrace));
                throw;
            }

        }
    }
}
