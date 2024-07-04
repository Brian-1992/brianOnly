using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Models;

namespace AAS001
{
    public class Chk
    {

        LogController log = new LogController("Chk", "Chk");
        public void Run() {

            using (WorkSession session = new WorkSession()) {
                var DBWork = session.UnitOfWork;
                ChkRepository repo = new ChkRepository(DBWork);
                log.AddLogs("開始Chk");
                DBWork.BeginTransaction();
                try
                {
                    log.AddLogs("取得所有未讀取盤點單主檔");
                    IEnumerable<LIS_CHK_MAST> masts = repo.GetLisChkmasts();
                    log.AddLogs(string.Format("未讀取主檔數量：{0}", masts.Count()));
                    foreach (LIS_CHK_MAST lis_mast in masts)
                    {
                        log.AddLogs(string.Format("chk_no：{0}  chk_ym：{1}", lis_mast.CHK_NO, lis_mast.CHK_YM));
                        MI_WHMAST whmast = repo.GetMiwhmast(lis_mast.CHK_WH_NO);

                        CHK_MAST chk_mast = new CHK_MAST();
                        chk_mast.CHK_NO = lis_mast.CHK_NO;
                        chk_mast.CHK_YM = lis_mast.CHK_YM;
                        chk_mast.CHK_WH_NO = lis_mast.CHK_WH_NO;
                        chk_mast.CHK_WH_GRADE = whmast.WH_GRADE;
                        chk_mast.CHK_WH_KIND = whmast.WH_KIND;
                        chk_mast.CHK_PERIOD = "M";
                        chk_mast.CHK_TYPE = "X";
                        chk_mast.CHK_LEVEL = "1";
                        chk_mast.CHK_STATUS = "3";
                        chk_mast.CHK_NO1 = chk_mast.CHK_NO;
                        log.AddLogs("新增chk_mast");
                        repo.InsertChkmast(chk_mast);
                        log.AddLogs("新增chk_mast 成功");

                        log.AddLogs("讀取 LIS_CHK_DETAILTOT");
                        IEnumerable<LIS_CHK_DETAILTOT> lis_tots = repo.GetLischkDetailtots(chk_mast.CHK_NO);
                        log.AddLogs(string.Format("LIS_CHK_DETAILTOT 數量：{0}", lis_tots.Count()));

                        foreach (LIS_CHK_DETAILTOT lis_tot in lis_tots)
                        {
                            CHK_DETAILTOT chk_tot = new CHK_DETAILTOT();
                            log.AddLogs(string.Format("mmcode：{0}", lis_tot.MMCODE));
                            log.AddLogs(string.Format("取得MI_MAST"));
                            MI_MAST mimast = repo.GetMimast(lis_tot.MMCODE);
                            log.AddLogs(string.Format("MI_MAST是否為null：{0}", mimast == null ? "Y" : "N"));

                            chk_tot.CHK_NO = lis_tot.CHK_NO;
                            chk_tot.MMCODE = lis_tot.MMCODE;
                            chk_tot.MMNAME_C = string.IsNullOrEmpty(lis_tot.MMNAME_C) == false ? lis_tot.MMNAME_C : (mimast == null ? string.Empty: mimast.MMNAME_C);
                            chk_tot.MMNAME_E = string.IsNullOrEmpty(lis_tot.MMNAME_E) == false ? lis_tot.MMNAME_E : (mimast == null ? string.Empty : mimast.MMNAME_E);
                            chk_tot.BASE_UNIT = string.IsNullOrEmpty(lis_tot.BASE_UNIT) == false ? lis_tot.BASE_UNIT : (mimast == null ? string.Empty : mimast.BASE_UNIT);
                            chk_tot.M_CONTPRICE = mimast == null ? string.Empty : mimast.M_CONTPRICE;
                            chk_tot.WH_NO = chk_mast.CHK_WH_NO;
                            chk_tot.STORE_LOC = string.Empty;
                            chk_tot.MAT_CLASS = mimast == null ? string.Empty : mimast.MAT_CLASS;
                            chk_tot.M_STOREID = mimast == null ? string.Empty : mimast.M_STOREID;
                            chk_tot.STORE_QTY = repo.GetSTORE_QTY(chk_tot.WH_NO, chk_tot.MMCODE);
                            chk_tot.STORE_QTYC = lis_tot.STORE_QTY;
                            chk_tot.STORE_QTYM = "0";
                            chk_tot.STORE_QTYS = "0";
                            chk_tot.LAST_QTY = repo.GetLAST_QTYC(chk_mast.CHK_YM, chk_tot.MMCODE, chk_tot.WH_NO);
                            chk_tot.LAST_QTYC = chk_tot.LAST_QTY;
                            chk_tot.LAST_QTYM = "0";
                            chk_tot.LAST_QTYS = "0";

                            chk_tot.CHK_QTY1 = lis_tot.CHK_QTY1;
                            chk_tot.GAP_T = Convert.ToString(Convert.ToDouble(chk_tot.CHK_QTY1) - Convert.ToDouble(chk_tot.STORE_QTYC)); ;
                            chk_tot.GAP_C = chk_tot.GAP_T;
                            chk_tot.GAP_M = "0";
                            chk_tot.GAP_S = "0";
                            chk_tot.APL_OUTQTY = repo.GetAPL_OUTQTY(chk_tot.MMCODE, chk_tot.WH_NO);
                            if (mimast != null) {
                                log.AddLogs(string.Format("mimast.M_TRNID：{0}", mimast.M_TRNID));
                            }
                            if (mimast != null && mimast.M_TRNID == "2")    // 不扣庫，算消耗
                            {
                                log.AddLogs(string.Format("mmcode：{0} 不扣庫，算消耗", chk_tot.MMCODE));
                                chk_tot.CONSUME_QTY = chk_tot.GAP_T;
                                chk_tot.CONSUME_AMOUNT = chk_tot.M_CONTPRICE == string.Empty ? "0" : Convert.ToString(Convert.ToDouble(chk_tot.CONSUME_QTY) * Convert.ToDouble(chk_tot.M_CONTPRICE)); ;
                                chk_tot.MISS_PER = "0";
                                chk_tot.MISS_PERC = "0";
                            }
                            else
                            {                                          // 扣庫，算盤盈虧
                                log.AddLogs(string.Format("mmcode：{0} 扣庫，算盤盈虧", chk_tot.MMCODE));
                                chk_tot.PRO_LOS_QTY = chk_tot.GAP_T;
                                chk_tot.PRO_LOS_AMOUNT = chk_tot.M_CONTPRICE == string.Empty ? "0" : Convert.ToString(Convert.ToDouble(chk_tot.PRO_LOS_QTY) * Convert.ToDouble(chk_tot.M_CONTPRICE)); ;

                                if (chk_tot.APL_OUTQTY != "0" && chk_tot.APL_OUTQTY != null)
                                {
                                    chk_tot.MISS_PER = Convert.ToString(Convert.ToDouble(chk_tot.PRO_LOS_QTY) / Convert.ToDouble(chk_tot.APL_OUTQTY));
                                    chk_tot.MISS_PERC = Convert.ToString((Convert.ToDouble(chk_tot.CHK_QTY1) - Convert.ToDouble(chk_tot.STORE_QTYM) - Convert.ToDouble(chk_tot.STORE_QTYS)) / Convert.ToDouble(chk_tot.APL_OUTQTY));
                                }
                                else
                                {
                                    chk_tot.MISS_PER = "0";
                                    chk_tot.MISS_PERC = "0";
                                }
                            }

                            chk_tot.STATUS_TOT = "1";
                            chk_tot.CREATE_USER = "BATCH";
                            chk_tot.UPDATE_USER = "BATCH";

                            log.AddLogs(string.Format("新增 CHK_DETAILTOT"));
                            repo.InsertDetiltot(chk_tot);
                            log.AddLogs(string.Format("新增 CHK_DETAILTOT 完成"));
                        }

                        log.AddLogs(string.Format("更新 LIS_CHK_DETAILTOT"));
                        repo.UpdateLisChkDetailtot(chk_mast.CHK_NO);
                        log.AddLogs(string.Format("更新 LIS_CHK_DETAILTOT 完成"));
                        log.AddLogs(string.Format("更新 CHK_MAST 數量"));
                        repo.UpdateChkmastCount(chk_mast.CHK_NO);
                        log.AddLogs(string.Format("更新 CHK_MAST 數量完成"));
                        log.AddLogs(string.Format("更新 LIS_CHK_MAST"));
                        repo.UpdateLisChkMast(chk_mast.CHK_NO);
                        log.AddLogs(string.Format("更新 LIS_CHK_MAST 完成"));
                    }

                    DBWork.Commit();
                    log.AddLogs("完成轉檔");

                    log.CreateLogFile();
                }
                catch(Exception ex) {
                    log.AddLogs(string.Format("error：{0}", ex.Message));
                    log.CreateLogFile();
                    DBWork.Rollback();
                    throw;
                }
            }
                
        }

        
    }
}
