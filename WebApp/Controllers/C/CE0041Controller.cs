using JCLib.DB;
using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using WebApp.Models;
using WebApp.Models.C;
using WebApp.Models.MI;
using WebApp.Repository.C;


namespace WebApp.Controllers.C
{
    public class CE0041Controller : SiteBase.BaseApiController
    {

        // 查詢
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
        {
            var chk_no = form.Get("chk_no"); // 盤點單號
            var chk_ym = form.Get("chk_ym"); // 盤點年月
            var chk_wh_no = form.Get("wh_no"); // 庫房
            var mmcode = form.Get("mmcode"); // 院內碼
            var mmname_e = form.Get("mmname_e"); // 英文品名
            var store_loc = form.Get("store_loc"); // 儲位
            var disc_cprice = form.Get("disc_cprice"); // 價格
            var drug_kind = form.Get("drug_kind");
            var isIV = form.Get("isIV"); // 價格
            var CB_1 = form.Get("CB_1"); // (近6個月進貨)或(近6個月醫令耗用)或(庫量<>0)或(庫存=0且無作廢)
            var CB_2 = form.Get("CB_2"); // (期初庫存<>0)或(期初=0但有進出)
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0041Repository(DBWork);

                    //檢查是否有庫房全縣
                    //if (repo.CheckWhidExists(DBWork.UserInfo.UserId, chk_wh_no) == false) {
                    //    session.Result.success = false;
                    //    session.Result.msg = "無此庫房權限，請重新確認";
                    //    return session.Result;
                    //}

                    session.Result.etts = repo.GetAllM(chk_no, chk_ym, chk_wh_no, mmcode, mmname_e, store_loc, disc_cprice, isIV, drug_kind, CB_1, CB_2, page, limit, sorters);
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpGet]
        public ApiResponse GetSetYm()
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0041Repository(DBWork);
                    session.Result.etts = repo.GetMnSet();
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetChkNoSt(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0041Repository(DBWork);
                    session.Result.etts = repo.GetChkNoSt(p0, p1);
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetSetymCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0041Repository repo = new CE0041Repository(DBWork);
                    var hospCode = repo.GetHospCode();
                    var ettsResult = repo.GetSetymCombo();

                    //寫入醫院CODE
                    foreach (var item in ettsResult)
                    {
                        item.HOSP_CODE = hospCode;
                    }
                    session.Result.etts = ettsResult;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetWhNoCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0041Repository repo = new CE0041Repository(DBWork);
                    session.Result.etts = repo.GetWhNoCombo(User.Identity.Name);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMmCodeCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("chk_wh_kind");
            var chk_no = form.Get("chk_no");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0041Repository repo = new CE0041Repository(DBWork);
                    session.Result.etts = repo.GetMmCodeCombo(p0, p1, chk_no, page, limit, "");
                }
                catch(Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetDrugKindCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0041Repository repo = new CE0041Repository(DBWork);
                    session.Result.etts = repo.GetDrugKindCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse UpdateDetails(FormDataCollection form)
        {
            var itemString = form.Get("list");

            string chk_no = string.Empty;
            string wh_no_c = string.Empty;

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    CE0041Repository repo = new CE0041Repository(DBWork);

                    IEnumerable<CHK_DETAIL> items = JsonConvert.DeserializeObject<IEnumerable<CHK_DETAIL>>(itemString);

                    foreach (CHK_DETAIL item in items)
                    {
                        chk_no = item.CHK_NO;
                        wh_no_c = item.WH_NO_C;

                        bool flowIdValid = repo.ChceckChkStatus(item.CHK_NO); // a.檢查盤點當狀態是否已變更，若盤點單狀態不為1則跳出錯誤訊息「盤點單狀態已變更，請重新查詢」
                        if (flowIdValid == false)
                        {
                            session.Result.msg = "盤點單狀態已變更，請重新查詢";
                            session.Result.success = false;
                            return session.Result;
                        }

                        //// b.針對花蓮衛星庫房檢查是否 盤點量+下月預估申請量 > 單位請領基準量。
                        //// 若大於單位請領基準量則跳出提示「盤點量(盤點量數字) + 下月預估申請量(下月預估申請量數字) 超過 單位請領基準量(單位請領基準量數字)，請重新確認」
                        //if (
                        //    repo.is花蓮且衛星庫房(this, item.CHK_NO) &&
                        //    is盤點量_加_下月預估申請量_大於_單位請領基準量(
                        //        item.CHK_QTY, // 盤點量
                        //        item.EST_APPQTY, // 下月預估申請量
                        //        item.G34_MAX_APPQTY // 單位請領基準量
                        //    )
                        //)
                        //{
                        //    session.Result.msg = "盤點量(盤點量數字) + 下月預估申請量(下月預估申請量數字) 超過 單位請領基準量(單位請領基準量數字)，請重新確認";
                        //    session.Result.success = false;
                        //    return session.Result;
                        //}

                        item.CHK_UID = DBWork.UserInfo.UserId;
                        item.UPDATE_USER = DBWork.UserInfo.UserId;
                        item.UPDATE_IP = DBWork.ProcIP;
                        item.STATUS_INI = "1";

                        session.Result.afrs = repo.UpdateChkDetail(item); // b.更新CHK_DETAIL盤點量、調整消耗
                    }

                        // c.判斷庫房更新CHK_DERAIL計算消耗及差異
                        session.Result.afrs = repo.UpdateChkDetail_c1(chk_no); // --1.一級庫計算差異 = 盤盈虧
                        session.Result.afrs = repo.UpdateChkDetail_c2(chk_no, wh_no_c); // --2.供應中心計算差異 = 盤盈虧
                        session.Result.afrs = repo.UpdateChkDetail_c3(chk_no); //-- 3.藥局可調整消耗，需重新計算差異量
                        session.Result.afrs = repo.UpdateChkDetail_c4(chk_no, wh_no_c); //-- 4.單位若chk_qty <= store_qtyc，消耗 = store_qtyc - chk_qty；若否，消耗 = 0，差異 = (chk_qty - store_qtyc)

                        session.Result.afrs = repo.UpdateChkNum(chk_no);

                    foreach (CHK_DETAIL item in items)
                    {
                        // 20240216: 增加更新CHK_DETAILTOT、MI_WINVMON、MI_WHINV
                        // 1. 更新CHK_DETAILTOT
                        session.Result.afrs = repo.UpdateChkDetailtot(item);

                        // 2. 更新MI_WINVMON (先修改回原資料)
                        session.Result.afrs = repo.UpdateMiwinvmonRollback(item);

                        // 3.insert MI_WHTRNS (先修改回原資料)
                        session.Result.afrs = repo.InsertWhtrnsRollback(item);

                        // 4. 更新MI_WHINV (先修改回原資料)
                        session.Result.afrs = repo.UpdateMiwhinvRollback(item);

                        //
                        item.WH_NAME = repo.GetWhname(item.CHK_WH_NO);
                        //取得庫存異動量
                        item.GAP_T = repo.GetTrinvqty(item);
                        item.USE_QTY_AF_CHK = repo.GetUseQtyAfChk(item);
                        item.INVENTORY = repo.GetInventory(item);
                        item.USE_QTY = repo.GetUseQty(item);
                        item.ALTERED_USE_QTY = repo.GetAlteredUseQty(item);

                        // 5. 以新資料更新MI_WINVMON
                        session.Result.afrs = repo.UpdateMiWinvmon(item);

                        // 6. insert MI_WHTRNS
                        session.Result.afrs = repo.InsertWhtrns(item);

                        // 7. 以新資料更新MI_WHINV
                        session.Result.afrs = repo.UpdateMiwhinv(item);

                        // 8. 檢查是否有CHK_EXPINV_TRNS，若有則先還原MI_WEXPINV
                        IEnumerable<CHK_EXPLOC> exps = repo.GetChkexpinvtrns(item.CHK_NO, item.MMCODE);
                        foreach (CHK_EXPLOC exp in exps) {
                            // 還原MI_WEXPINV
                            session.Result.afrs = repo.UpdateMiwexpinvRollback(exp);
                        }
                        session.Result.afrs = repo.DeleteChkexpinvtrns(item.CHK_NO, item.MMCODE);

                        // 9. 取得MI_WEXPINV資料
                        // 檢查是否有批號效期資料
                        exps = repo.GetExpinvs(item.CHK_NO, item.CHK_WH_NO, item.MMCODE).ToList<CHK_EXPLOC>();
                        string use_qty = repo.GetNewUseQty(item);

                        // 檢查電腦量與批號效期總量是否相符
                        string exp_diff = repo.GetExpInvDiff(item.CHK_NO, item.MMCODE);
                        if (exp_diff != "0")
                        {
                            // 不相符，修改9991231 TMPLOT
                            repo.MergeExpinv(item.CHK_WH_NO, item.MMCODE, exp_diff);
                        }
                        // 重取
                        exps = repo.GetExpinvs(item.CHK_NO, item.CHK_WH_NO, item.MMCODE).ToList<CHK_EXPLOC>();

                        double use_qty_d = double.Parse(use_qty);

                        if (use_qty_d < 0) {
                            // 若無9991231 TMPLOT則新增
                            CHK_EXPLOC temp = new CHK_EXPLOC() {
                                WH_NO = item.CHK_WH_NO,
                                MMCODE = item.MMCODE,
                                EXP_DATE ="9991231",
                                LOT_NO = "TMPLOT",
                                TRN_QTY = "0"
                            };
                            if (repo.CheckMiwexpinvExists(temp) == false) {
                                repo.InsertMiwexpinv(temp);
                            }
                        }

                        // 重取
                        exps = repo.GetExpinvs(item.CHK_NO, item.CHK_WH_NO, item.MMCODE).ToList<CHK_EXPLOC>();

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

                            if (ori_qty_d >= use_qty_d && use_qty_d != 0)
                            {
                                exp.TRN_QTY = use_qty;
                                use_qty_d = use_qty_d - double.Parse(exp.TRN_QTY);
                            }
                            else if (ori_qty_d < use_qty_d && use_qty_d != 0)
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
                        // 10. 檢查是否有CHK_LOCINV_TRNS，若有則先還原MI_WLOCINV
                        IEnumerable<CHK_EXPLOC> locs = repo.GetChklocinvtrns(item.CHK_NO, item.MMCODE);
                        foreach (CHK_EXPLOC loc in locs)
                        {
                            // 還原MI_WLOCINV
                            session.Result.afrs = repo.UpdateMiwlocinvRollback(loc);
                        }
                        session.Result.afrs = repo.DeleteChklocinvtrns(item.CHK_NO, item.MMCODE);
                        // 11. 取得MI_WLOCINV資料
                        // 檢查是否有儲位資料
                        locs = repo.GetLocinvs(item.CHK_NO, item.CHK_WH_NO, item.MMCODE).ToList<CHK_EXPLOC>();

                        // 檢查電腦量與儲位總量是否相符
                        string loc_diff = repo.GetLocInvDiff(item.CHK_NO, item.MMCODE);
                        if (loc_diff != "0")
                        {
                            // 不相符，修改TMPLOC
                            repo.MergeLocinv(item.CHK_WH_NO, item.MMCODE, loc_diff);
                        }
                        // 重取
                        locs = repo.GetLocinvs(item.CHK_NO, item.CHK_WH_NO, item.MMCODE).ToList<CHK_EXPLOC>();

                        use_qty_d = double.Parse(use_qty);

                        if (use_qty_d < 0)
                        {
                            // 若無9991231 TMPLOT則新增
                            CHK_EXPLOC temp = new CHK_EXPLOC()
                            {
                                WH_NO = item.CHK_WH_NO,
                                MMCODE = item.MMCODE,
                                STORE_LOC = "TMPLOC",
                                TRN_QTY = "0"
                            };
                            if (repo.CheckMiwlocinvExists(temp) == false)
                            {
                                repo.InsertMiwlocinv(temp);
                            }
                        }

                        foreach (CHK_EXPLOC loc in locs)
                        {
                            double ori_qty_d = double.Parse(loc.ORI_INV_QTY);
                            if (ori_qty_d <= 0)
                            {
                                // 無存量，跳過
                                continue;
                            }

                            if (ori_qty_d >= use_qty_d && use_qty_d != 0)
                            {
                                loc.TRN_QTY = use_qty;
                                use_qty_d = use_qty_d - double.Parse(loc.TRN_QTY);
                            }
                            else if (ori_qty_d < use_qty_d && use_qty_d != 0)
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
                    DBWork.Commit();
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetWhNoData(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0041Repository(DBWork);
                    session.Result.etts = repo.GetWhNoData(p0);
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

        #region " 一鍵符合 按鈕 "

        [HttpPost]
        public ApiResponse BtnOneKeyMatch(FormDataCollection form)
        {
            var itemString = form.Get("list");

            IEnumerable<CHK_DETAIL> items = JsonConvert.DeserializeObject<IEnumerable<CHK_DETAIL>>(itemString);

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    CE0041Repository repo = new CE0041Repository(DBWork);

                    foreach (CHK_DETAIL item in items)
                    {
                        item.CHK_UID = DBWork.UserInfo.UserId;
                        item.UPDATE_USER = DBWork.UserInfo.UserId;
                        item.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.BtnOneKeyMatch(item); // b.更新CHK_DETAIL盤點量、調整消耗
                        repo.UpdateChkNum(item.CHK_NO);
                        break; // 因為用盤點單號一次更新，所以不需要每一筆
                    }
                    DBWork.Commit();
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        } // 
        #endregion

        [HttpPost]
        public ApiResponse CreateMMCodeChk(FormDataCollection form)
        {
            string mmcode = form.Get("mmcode");
            string ym = form.Get("ym");
            string chk_no = form.Get("chk_no");
            string wh_no = form.Get("wh_no");
            string chk_wh_kind = form.Get("chk_wh_kind");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0041Repository repo = new CE0041Repository(DBWork);

                    // 檢查是否為花蓮
                    if (repo.GetHospCode() != "805")
                    {
                        session.Result.success = false;
                        session.Result.msg = "只有花蓮可以新增院內碼";
                        return session.Result;
                    }

                    if (repo.GetCHK_DETAIL(chk_no, mmcode) != null)
                    {
                        session.Result.success = false;
                        session.Result.msg = "院內碼已存在";
                        return session.Result;
                    }

                    DBWork.BeginTransaction();
                    MI_MAST mi_mast = repo.GetMI_MAST(mmcode);
                    
                    #region create chk_detail
                    CHK_DETAIL chk_detail = new CHK_DETAIL();
                    chk_detail.CHK_NO = chk_no;
                    chk_detail.MMCODE = mi_mast.MMCODE;
                    chk_detail.MMNAME_C = mi_mast.MMNAME_C;
                    chk_detail.MMNAME_E = mi_mast.MMNAME_E;
                    chk_detail.BASE_UNIT = mi_mast.BASE_UNIT;
                    chk_detail.M_CONTPRICE = mi_mast.M_CONTPRICE;
                    chk_detail.WH_NO = wh_no;
                    chk_detail.MAT_CLASS = mi_mast.MAT_CLASS;
                    chk_detail.M_STOREID = "0"; 
                    chk_detail.STORE_QTYC = "0";
                    chk_detail.STORE_QTYM = "0";
                    chk_detail.STORE_QTYS = "0";
                    chk_detail.STATUS_INI = "1";
                    chk_detail.CREATE_USER = session.UnitOfWork.ProcUser;
                    chk_detail.UPDATE_USER = session.UnitOfWork.ProcUser;
                    chk_detail.PYM_INV_QTY = "0";
                    chk_detail.APL_INQTY = "0";
                    chk_detail.APL_OUTQTY = "0";
                    chk_detail.TRN_INQTY = "0";
                    chk_detail.TRN_OUTQTY = "0";
                    chk_detail.ADJ_INQTY = "0";
                    chk_detail.ADJ_OUTQTY = "0";
                    chk_detail.BAK_INQTY = "0";
                    chk_detail.BAK_OUTQTY = "0";
                    chk_detail.REJ_OUTQTY = "0";
                    chk_detail.DIS_OUTQTY = "0";
                    chk_detail.EXG_INQTY = "0";
                    chk_detail.EXG_OUTQTY = "0";
                    chk_detail.MIL_INQTY = "0";
                    chk_detail.MIL_OUTQTY = "0";
                    chk_detail.USE_QTY = "0";
                    chk_detail.MAT_CLASS_SUB = mi_mast.MAT_CLASS_SUB;
                    chk_detail.DISC_CPRICE = mi_mast.DISC_CPRICE;
                    chk_detail.PYM_CONT_PRICE = "0";
                    chk_detail.PYM_DISC_CPRICE = "0";
                    chk_detail.WAR_QTY = "0";
                    chk_detail.UNITRATE = "0";
                    chk_detail.M_CONTID = "0";
                    chk_detail.E_SOURCECODE = "0";
                    chk_detail.E_RESTRICTCODE = "0";
                    chk_detail.COMMON = mi_mast.COMMON;
                    chk_detail.FASTDRUG = mi_mast.FASTDRUG;
                    chk_detail.DRUGKIND = mi_mast.DRUGKIND;
                    chk_detail.TOUCHCASE = mi_mast.TOUCHCASE;
                    chk_detail.ORDERKIND = mi_mast.ORDERKIND;
                    chk_detail.SPDRUG = mi_mast.SPDRUG;
                    chk_detail.EST_APPQTY = "0";
                    chk_detail.G34_MAX_APPQTY = "0";
                    repo.CreateCHK_DETAIL(chk_detail);
                    #endregion

                    #region create mi_winvmon
                    if (repo.CheckMiWinvmonExists(ym, wh_no, mi_mast.MMCODE) == false) {
                        MI_WINVMON mi_winvmon = new MI_WINVMON();
                        mi_winvmon.DATA_YM = ym;
                        mi_winvmon.WH_NO = wh_no;
                        mi_winvmon.MMCODE = mi_mast.MMCODE;
                        mi_winvmon.INV_QTY = "0";
                        mi_winvmon.APL_INQTY = "0";
                        mi_winvmon.APL_OUTQTY = "0";
                        mi_winvmon.TRN_INQTY = "0";
                        mi_winvmon.TRN_OUTQTY = "0";
                        mi_winvmon.ADJ_INQTY = "0";
                        mi_winvmon.ADJ_OUTQTY = "0";
                        mi_winvmon.BAK_INQTY = "0";
                        mi_winvmon.BAK_OUTQTY = "0";
                        mi_winvmon.REJ_OUTQTY = "0";
                        mi_winvmon.DIS_OUTQTY = "0";
                        mi_winvmon.EXG_INQTY = "0";
                        mi_winvmon.EXG_OUTQTY = "0";
                        mi_winvmon.MIL_INQTY = "0";
                        mi_winvmon.MIL_OUTQTY = "0";
                        mi_winvmon.INVENTORYQTY = "0";
                        mi_winvmon.TUNEAMOUNT = "0";
                        mi_winvmon.USE_QTY = "0";
                        mi_winvmon.TURNOVER = "0";
                        mi_winvmon.SAFE_QTY = "0";
                        mi_winvmon.OPER_QTY = "0";
                        mi_winvmon.SHIP_QTY = "0";
                        mi_winvmon.DAVG_USEQTY = "0";
                        mi_winvmon.ONWAY_QTY = "0";
                        mi_winvmon.SAFE_DAY = "15";
                        mi_winvmon.OPER_DAY = "15";
                        mi_winvmon.SHIP_DAY = "15";
                        mi_winvmon.HIGH_QTY = "0";
                        mi_winvmon.ORI_INV_QTY = "0";
                        mi_winvmon.ORI_USE_QTY = "0";
                        repo.CreateMI_WINVMON(mi_winvmon);
                    }
                    #endregion

                    #region create mi_whinv
                    if (repo.CheckMiWhinvExists(wh_no, mmcode) == false) {
                        MI_WHINV mi_whinv = new MI_WHINV();
                        mi_whinv.WH_NO = wh_no;
                        mi_whinv.MMCODE = mmcode;
                        mi_whinv.INV_QTY = "0";
                        mi_whinv.ONWAY_QTY = "0";
                        mi_whinv.APL_INQTY = "0";
                        mi_whinv.APL_OUTQTY = "0";
                        mi_whinv.TRN_INQTY = "0";
                        mi_whinv.TRN_OUTQTY = "0";
                        mi_whinv.ADJ_INQTY = "0";
                        mi_whinv.ADJ_OUTQTY = "0";
                        mi_whinv.BAK_INQTY = "0";
                        mi_whinv.BAK_OUTQTY = "0";
                        mi_whinv.REJ_OUTQTY = "0";
                        mi_whinv.DIS_OUTQTY = "0";
                        mi_whinv.EXG_INQTY = "0";
                        mi_whinv.EXG_OUTQTY = "0";
                        mi_whinv.MIL_INQTY = "0";
                        mi_whinv.MIL_OUTQTY = "0";
                        mi_whinv.INVENTORYQTY = "0";
                        mi_whinv.TUNEAMOUNT = "0";
                        mi_whinv.USE_QTY = "0";
                        mi_whinv.TRANS_ID = "U";
                        repo.CreateMI_WHINV(mi_whinv);
                    }
                    #endregion

                    #region create mi_winvctl
                    if (repo.CheckMiWinvctlExists(wh_no, mi_mast.MMCODE) == false) {
                        // 特殊轉換 上級庫
                        var supply_whno = repo.GetWhno_mm1();
                        if (chk_wh_kind == "0")
                        {
                            supply_whno = repo.GetWhno_me1();
                        }

                        MI_WINVCTL mi_winvctl = new MI_WINVCTL();
                        mi_winvctl.WH_NO = wh_no;
                        mi_winvctl.MMCODE = mmcode;
                        mi_winvctl.SAFE_DAY = "15";
                        mi_winvctl.OPER_DAY = "15";
                        mi_winvctl.SHIP_DAY = "0";
                        mi_winvctl.SAFE_QTY = 0;
                        mi_winvctl.OPER_QTY = 0;
                        mi_winvctl.SHIP_QTY = 0;
                        mi_winvctl.DAVG_USEQTY = 0;
                        mi_winvctl.HIGH_QTY = 1000;
                        mi_winvctl.LOW_QTY = 0;
                        mi_winvctl.MIN_ORDQTY = 1;
                        mi_winvctl.SUPPLY_WHNO = supply_whno;
                        repo.CreateMI_WINVCTL(mi_winvctl);
                    }

                    #endregion

                    repo.UpdateChkTotal(chk_no);

                    DBWork.Commit();
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    session.Result.success = false;
                    session.Result.msg = e.Message;
                    throw;
                }
                return session.Result;
            }
        }

        //匯入檢核
        [HttpPost]
        public ApiResponse SendExcel()
        {
            using (WorkSession session = new WorkSession(this))
            {

                List<CHK_DETAIL> list = new List<CHK_DETAIL>();
                List<CHK_DETAIL> final_list = new List<CHK_DETAIL>();
                UnitOfWork DBWork = session.UnitOfWork;
                var HttpPostedFile = HttpContext.Current.Request.Files["file"];
                var chk_no = HttpContext.Current.Request.Form["chk_no"];
                var wh_no = HttpContext.Current.Request.Form["wh_no"];
                IWorkbook workBook;

                try
                {
                    CE0041Repository repo = new CE0041Repository(DBWork);
                    bool checkPassed = true; //檢核有沒有通過 有通過就true 沒通過就false

                    #region 檢查檔案格式
                    if (Path.GetExtension(HttpPostedFile.FileName).ToLower() == ".xls")
                    {
                        workBook = new HSSFWorkbook(HttpPostedFile.InputStream); //讀取xls檔
                    }
                    else
                    {
                        workBook = new XSSFWorkbook(HttpPostedFile.InputStream); //讀取xlsx檔
                    }

                    var sheet = workBook.GetSheetAt(0); //讀取EXCEL的第一個分頁
                    //IRow headerRow = sheet.GetRow(0); //由第一列取標題做為欄位名稱
                    IRow headerRow = sheet.GetRow(0); //由第二列取標題做為欄位名稱
                    int cellCount = headerRow.LastCellNum; //欄位數目
                    //int i, j;

                    List<HeaderItem> headerItems = new List<HeaderItem>() {
                       new HeaderItem("盤點單號", "CHK_NO"),
                        new HeaderItem("院內碼", "MMCODE"),
                       new HeaderItem("盤點量","CHK_QTY"),
                       new HeaderItem("調整消耗","ALTERED_USE_QTY"),
                       new HeaderItem("單位請領基準量","G34_MAX_APPQTY"),
                       new HeaderItem("下月預估申請量","EST_APPQTY"),
                       new HeaderItem("備註","CHK_REMARK"),
                    };

                    headerItems = SetHeaderIndex(headerItems, headerRow);

                    bool is805Grade2Kind1 = repo.is花蓮且衛星庫房(this, chk_no);
                    bool isPhr = repo.CheckIsPhrByChkno(chk_no);

                    if (is805Grade2Kind1 == false)
                    {
                        headerItems.RemoveAt(5);
                        headerItems.RemoveAt(4);
                    }
                    if (isPhr == false)
                    {
                        headerItems.RemoveAt(3);
                    }


                    // 確定該有的欄位都有，沒有的回傳錯誤訊息
                    string errMsg = string.Empty;
                    foreach (HeaderItem item in headerItems)
                    {
                        if (item.Index == -1)
                        {
                            // 單位請領基準量 與 下月預估申請量 僅花蓮衛星庫房有，其餘跳過此判斷
                            if (is805Grade2Kind1 == false
                                && (item.Name == "單位請領基準量" || item.Name == "下月預估申請量"))
                            {
                                continue;
                            }

                            // 調整消耗 僅藥局有，其餘跳過判斷
                            if (isPhr == false && (item.Name == "調整消耗"))
                            {
                                continue;
                            }

                            if (errMsg == string.Empty)
                            {
                                errMsg += item.Name;
                            }
                            else
                            {
                                errMsg += string.Format("、{0}", item.Name);
                            }
                        }
                    }

                    if (errMsg != string.Empty)
                    {
                        session.Result.success = false;
                        session.Result.msg = string.Format("欄位錯誤，缺：{0}", errMsg);
                        return session.Result;
                    }

                    for (int i = 1; i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row is null) { continue; }

                        CHK_DETAIL temp = new CHK_DETAIL();
                        foreach (HeaderItem item in headerItems)
                        {
                            string value = row.GetCell(item.Index) == null ? string.Empty : row.GetCell(item.Index).ToString();
                            temp.GetType().GetProperty(item.FieldName).SetValue(temp, value, null);
                        }
                        list.Add(temp);
                    }

                    #endregion

                    bool flowIdValid = repo.ChceckChkStatus(chk_no);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "盤點單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    int j;
                    double d;
                    foreach (CHK_DETAIL item in list)
                    {
                        item.UPDATE_USER = DBWork.UserInfo.UserId;
                        // 盤點量未填 表示跳過不更新
                        if (string.IsNullOrEmpty(item.CHK_QTY))
                        {
                            continue;
                        }

                        // 檢查盤點單號是否有填
                        if (string.IsNullOrEmpty(item.CHK_NO))
                        {
                            item.CHECK_RESULT = "無盤點單號";
                            item.IsPass = false;
                            checkPassed = false;
                        }
                        // 盤點單號是否存在
                        else if (repo.CheckExistsCHK_NO(item.CHK_NO) == false)
                        {

                            item.CHECK_RESULT = "盤點單號錯誤";
                            item.IsPass = false;
                            checkPassed = false;
                        }
                        // 檢查盤點單號是否與選擇單號相同
                        else if (item.CHK_NO != chk_no)
                        {
                            item.CHECK_RESULT = "盤點單號與所選單號不同";
                            item.IsPass = false;
                            checkPassed = false;
                        }
                        // 檢查院內碼是否有填
                        else if (string.IsNullOrEmpty(item.MMCODE))
                        {
                            item.CHECK_RESULT = "無院內碼";
                            item.IsPass = false;
                            checkPassed = false;
                        }
                        // 檢查院內碼是否存在於盤點單中
                        else if (repo.CheckExistsMMCODE(item.MMCODE) == false)
                        {
                            item.CHECK_RESULT = "院內碼不存在";
                            item.IsPass = false;
                            checkPassed = false;
                        }
                        else if (repo.CheckChkExistsMMCODE(item.CHK_NO, item.MMCODE) == false && is805Grade2Kind1 == false) {
                            item.CHECK_RESULT = "院內碼不存在於盤點單中";
                            item.IsPass = false;
                            checkPassed = false;
                        }
                        // 檢查院內碼是否存在於盤點單中
                        else if (list.Count(x => x.MMCODE == item.MMCODE) > 1)
                        {
                            item.CHECK_RESULT = "院內碼重複";
                            item.IsPass = false;
                            checkPassed = false;
                        }
                        else if (repo.CheckExistsWhmast(item.UPDATE_USER, item.CHK_NO) == false)
                        {
                            item.CHECK_RESULT = "無此庫房權限";
                            item.IsPass = false;
                            checkPassed = false;
                        }
                        else if (double.TryParse(item.CHK_QTY, out d) == false)
                        {
                            item.CHECK_RESULT = "盤點量需為數字";
                            item.IsPass = false;
                            checkPassed = false;
                        }
                        //else if (double.Parse(item.CHK_QTY) < 0 || double.Parse(item.CHK_QTY) % 1 != 0)
                        //{
                        //    item.CHECK_RESULT = "盤點量需為整數且大於等於0";
                        //    item.IsPass = false;
                        //    checkPassed = false;
                        //}
                        else if (int.TryParse(item.G34_MAX_APPQTY, out j) == false && is805Grade2Kind1)
                        {
                            item.CHECK_RESULT = "單位請領基準量量需為數字";
                            item.IsPass = false;
                            checkPassed = false;
                        }
                        else if (is805Grade2Kind1 && (int.Parse(item.G34_MAX_APPQTY) < 0 || int.Parse(item.G34_MAX_APPQTY) % 1 != 0))
                        {
                            item.CHECK_RESULT = "單位請領基準量需為整數且大於等於0";
                            item.IsPass = false;
                            checkPassed = false;
                        }
                        else if (is805Grade2Kind1 && int.TryParse(item.EST_APPQTY, out j) == false)
                        {
                            item.CHECK_RESULT = "下月預估申請量量需為數字";
                            item.IsPass = false;
                            checkPassed = false;
                        }
                        else if (is805Grade2Kind1 && (int.Parse(item.EST_APPQTY) < 0 || int.Parse(item.EST_APPQTY) % 1 != 0))
                        {
                            item.CHECK_RESULT = "下月預估申請量需為整數且大於等於0";
                            item.IsPass = false;
                            checkPassed = false;
                        }
                        else if (isPhr && string.IsNullOrEmpty(item.ALTERED_USE_QTY) == false  
                                       && int.TryParse(item.ALTERED_USE_QTY, out j) == false)
                        {
                            item.CHECK_RESULT = "調整消耗量需為數字";
                            item.IsPass = false;
                            checkPassed = false;
                        }
                        else if (isPhr && string.IsNullOrEmpty(item.ALTERED_USE_QTY) == false 
                                       && (int.Parse(item.ALTERED_USE_QTY) < 0 || int.Parse(item.ALTERED_USE_QTY) % 1 != 0))
                        {
                            item.CHECK_RESULT = "調整消耗需為整數且大於等於0";
                            item.IsPass = false;
                            checkPassed = false;
                        }

                        if (string.IsNullOrEmpty(item.CHECK_RESULT))
                        {
                            item.CHECK_RESULT = "OK";
                        }

                        final_list.Add(item);
                    }

                    //if (checkPassed == false) {
                    //    session.Result.success = false;
                    //}

                    session.Result.etts = final_list;
                    session.Result.msg = checkPassed.ToString();
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }
        //public bool is花蓮且衛星庫房(String chk_no) 
        //{

        //    using (WorkSession session = new WorkSession(this))
        //    {
        //        var DBWork = session.UnitOfWork;
        //        try
        //        {
        //            var repo = new CE0041Repository(DBWork);
        //            IEnumerable<COMBO_MODEL> 花蓮且衛星庫房s = repo.get花蓮且衛星庫房(chk_no);
        //            return 花蓮且衛星庫房s.Any();
        //        }
        //        catch
        //        {
        //            throw;
        //        }
        //    }
        //    return false;
        //}
        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        { // 【匯出】按鈕

            string strName = User.Identity.Name;
            string CHK_NO = form.Get("CHK_NO"); // 盤點單號
            string CHK_YM = form.Get("CHK_YM"); // 
            string CHK_WH_NO = form.Get("CHK_WH_NO"); // 
            string MMCODE = form.Get("MMCODE"); // 院內碼
            string MMNAME_E = form.Get("MMNAME_E"); // 英文品名
            string STORE_LOC = form.Get("STORE_LOC"); // 儲位
            string DISC_CPRICE = form.Get("DISC_CPRICE"); // 價錢(成本價)大於
            string ISIV = form.Get("ISIV"); // 價錢(成本價)大於"
            string drug_kind = form.Get("drug_kind");
            string CB_1 = form.Get("CB_1"); // 價錢(成本價)大於
            string CB_2 = form.Get("CB_2"); // 價錢(成本價)大於
            string excelName = CHK_NO + "_" + (Convert.ToInt32(DateTime.Now.ToString("yyyy")) - 1911).ToString() + DateTime.Now.ToString("MMddhhmmss") + ".xlsx";

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0041Repository repo = new CE0041Repository(DBWork);
                    var hospCode = repo.GetHospCode();
                    bool is花蓮且衛星庫房 = repo.is花蓮且衛星庫房(this, CHK_NO);
                    bool is藥局 = repo.is藥局(CHK_WH_NO);
                    bool is花蓮 = false;
                    if (repo.GetHospCode() == "805"){
                        is花蓮 = true;
                    }

                    JCLib.Excel.Export(excelName, repo.GetExcel(
                            is花蓮且衛星庫房,
                            is藥局,
                            is花蓮,
                            CHK_NO, // 盤點單號
                            CHK_YM,
                            CHK_WH_NO,
                            MMCODE,
                            MMNAME_E, // 英文品名
                            STORE_LOC, // 儲位
                            DISC_CPRICE, // 價錢(成本價)大於
                            ISIV,
                            drug_kind,
                            CB_1, 
                            CB_2
                        )
                    );
                }
                catch(Exception e)
                {
                    throw;
                }
                return session.Result;
            }


        }

        [HttpPost]
        public ApiResponse GetIsIVCombo()
        {
            using (WorkSession session = new WorkSession(this))
            {
                List<COMBO_MODEL> list = new List<COMBO_MODEL>();
                list.Add(new COMBO_MODEL { TEXT = "全部", VALUE = "" });
                list.Add(new COMBO_MODEL { TEXT = "是", VALUE = "Y" });
                list.Add(new COMBO_MODEL { TEXT = "否", VALUE = "N" });

                session.Result.etts = list;
                return session.Result;
            }
        }

        #region 匯入

        // 確認更新
        [HttpPost]
        public ApiResponse ImportUpdate(FormDataCollection formData)
        {
            var chk_no = formData.Get("chk_no");
            var wh_no = formData.Get("wh_no");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();

                IEnumerable<CHK_DETAIL> list = JsonConvert.DeserializeObject<IEnumerable<CHK_DETAIL>>(formData["item"]);

                List<string> checkDuplicate = new List<string>();

                List<CHK_DETAIL> chk_detail_list = new List<CHK_DETAIL>();
                try
                {
                    var repo = new CE0041Repository(DBWork);
                    bool isDuplicate = false;

                    bool flowIdValid = repo.ChceckChkStatus(chk_no);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "盤點單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    foreach (CHK_DETAIL item in list)
                    {
                        item.CHK_NO = chk_no;
                        if (checkDuplicate.Contains(item.MMCODE)) //檢查list有沒有已經insert過的MMCODE
                        {
                            isDuplicate = true;
                            session.Result.msg = isDuplicate.ToString();
                            break;
                        }
                        else
                        {
                            checkDuplicate.Add(item.MMCODE);

                            if (string.IsNullOrEmpty(item.CHK_QTY)) {
                                continue;
                            }

                            try
                            {
                                //item.CREATE_USER = User.Identity.Name;
                                //item.UPDATE_IP = DBWork.ProcIP;
                                //session.Result.afrs = repo.DetailUpdate(item);

                                item.CHK_UID = DBWork.UserInfo.UserId;
                                item.UPDATE_USER = DBWork.UserInfo.UserId;
                                item.UPDATE_IP = DBWork.ProcIP;
                                item.STATUS_INI = "1";

                                string ym = repo.GetChkYmByChkno(item.CHK_NO);
                                string chk_wh_kind = repo.GetChkWhKind(item.CHK_NO);

                                // 不存在於CHK_DETAIL，需新增
                                if (repo.CheckChkExistsMMCODE(item.CHK_NO, item.MMCODE) == false) {
                                    MI_MAST mi_mast = repo.GetMI_MAST(item.MMCODE);
                                    #region create chk_detail
                                    CHK_DETAIL chk_detail = new CHK_DETAIL();
                                    chk_detail.CHK_NO = chk_no;
                                    chk_detail.MMCODE = mi_mast.MMCODE;
                                    chk_detail.MMNAME_C = mi_mast.MMNAME_C;
                                    chk_detail.MMNAME_E = mi_mast.MMNAME_E;
                                    chk_detail.BASE_UNIT = mi_mast.BASE_UNIT;
                                    chk_detail.M_CONTPRICE = mi_mast.M_CONTPRICE;
                                    chk_detail.WH_NO = wh_no;
                                    chk_detail.MAT_CLASS = mi_mast.MAT_CLASS;
                                    chk_detail.M_STOREID = "0";
                                    chk_detail.STORE_QTYC = "0";
                                    chk_detail.STORE_QTYM = "0";
                                    chk_detail.STORE_QTYS = "0";
                                    chk_detail.STATUS_INI = "1";
                                    chk_detail.CREATE_USER = session.UnitOfWork.ProcUser;
                                    chk_detail.UPDATE_USER = session.UnitOfWork.ProcUser;
                                    chk_detail.PYM_INV_QTY = "0";
                                    chk_detail.APL_INQTY = "0";
                                    chk_detail.APL_OUTQTY = "0";
                                    chk_detail.TRN_INQTY = "0";
                                    chk_detail.TRN_OUTQTY = "0";
                                    chk_detail.ADJ_INQTY = "0";
                                    chk_detail.ADJ_OUTQTY = "0";
                                    chk_detail.BAK_INQTY = "0";
                                    chk_detail.BAK_OUTQTY = "0";
                                    chk_detail.REJ_OUTQTY = "0";
                                    chk_detail.DIS_OUTQTY = "0";
                                    chk_detail.EXG_INQTY = "0";
                                    chk_detail.EXG_OUTQTY = "0";
                                    chk_detail.MIL_INQTY = "0";
                                    chk_detail.MIL_OUTQTY = "0";
                                    chk_detail.USE_QTY = "0";
                                    chk_detail.MAT_CLASS_SUB = mi_mast.MAT_CLASS_SUB;
                                    chk_detail.DISC_CPRICE = mi_mast.DISC_CPRICE;
                                    chk_detail.PYM_CONT_PRICE = "0";
                                    chk_detail.PYM_DISC_CPRICE = "0";
                                    chk_detail.WAR_QTY = "0";
                                    chk_detail.UNITRATE = "0";
                                    chk_detail.M_CONTID = "0";
                                    chk_detail.E_SOURCECODE = "0";
                                    chk_detail.E_RESTRICTCODE = "0";
                                    chk_detail.COMMON = mi_mast.COMMON;
                                    chk_detail.FASTDRUG = mi_mast.FASTDRUG;
                                    chk_detail.DRUGKIND = mi_mast.DRUGKIND;
                                    chk_detail.TOUCHCASE = mi_mast.TOUCHCASE;
                                    chk_detail.ORDERKIND = mi_mast.ORDERKIND;
                                    chk_detail.SPDRUG = mi_mast.SPDRUG;
                                    chk_detail.EST_APPQTY = "0";
                                    chk_detail.G34_MAX_APPQTY = "0";
                                    repo.CreateCHK_DETAIL(chk_detail);
                                    #endregion

                                    #region create mi_winvmon
                                    if (repo.CheckMiWinvmonExistsByChkNo(item.CHK_NO, mi_mast.MMCODE) == false)
                                    {
                                        MI_WINVMON mi_winvmon = new MI_WINVMON();

                                        mi_winvmon.DATA_YM = repo.GetChkYmByChkno(item.CHK_NO);
                                        mi_winvmon.WH_NO = wh_no;
                                        mi_winvmon.MMCODE = mi_mast.MMCODE;
                                        mi_winvmon.INV_QTY = "0";
                                        mi_winvmon.APL_INQTY = "0";
                                        mi_winvmon.APL_OUTQTY = "0";
                                        mi_winvmon.TRN_INQTY = "0";
                                        mi_winvmon.TRN_OUTQTY = "0";
                                        mi_winvmon.ADJ_INQTY = "0";
                                        mi_winvmon.ADJ_OUTQTY = "0";
                                        mi_winvmon.BAK_INQTY = "0";
                                        mi_winvmon.BAK_OUTQTY = "0";
                                        mi_winvmon.REJ_OUTQTY = "0";
                                        mi_winvmon.DIS_OUTQTY = "0";
                                        mi_winvmon.EXG_INQTY = "0";
                                        mi_winvmon.EXG_OUTQTY = "0";
                                        mi_winvmon.MIL_INQTY = "0";
                                        mi_winvmon.MIL_OUTQTY = "0";
                                        mi_winvmon.INVENTORYQTY = "0";
                                        mi_winvmon.TUNEAMOUNT = "0";
                                        mi_winvmon.USE_QTY = "0";
                                        mi_winvmon.TURNOVER = "0";
                                        mi_winvmon.SAFE_QTY = "0";
                                        mi_winvmon.OPER_QTY = "0";
                                        mi_winvmon.SHIP_QTY = "0";
                                        mi_winvmon.DAVG_USEQTY = "0";
                                        mi_winvmon.ONWAY_QTY = "0";
                                        mi_winvmon.SAFE_DAY = "15";
                                        mi_winvmon.OPER_DAY = "15";
                                        mi_winvmon.SHIP_DAY = "15";
                                        mi_winvmon.HIGH_QTY = "0";
                                        mi_winvmon.ORI_INV_QTY = "0";
                                        mi_winvmon.ORI_USE_QTY = "0";
                                        repo.CreateMI_WINVMON(mi_winvmon);
                                    }
                                    #endregion

                                    #region create mi_whinv
                                    if (repo.CheckMiWhinvExists(wh_no, mi_mast.MMCODE) == false)
                                    {
                                        MI_WHINV mi_whinv = new MI_WHINV();
                                        mi_whinv.WH_NO = wh_no;
                                        mi_whinv.MMCODE = mi_mast.MMCODE;
                                        mi_whinv.INV_QTY = "0";
                                        mi_whinv.ONWAY_QTY = "0";
                                        mi_whinv.APL_INQTY = "0";
                                        mi_whinv.APL_OUTQTY = "0";
                                        mi_whinv.TRN_INQTY = "0";
                                        mi_whinv.TRN_OUTQTY = "0";
                                        mi_whinv.ADJ_INQTY = "0";
                                        mi_whinv.ADJ_OUTQTY = "0";
                                        mi_whinv.BAK_INQTY = "0";
                                        mi_whinv.BAK_OUTQTY = "0";
                                        mi_whinv.REJ_OUTQTY = "0";
                                        mi_whinv.DIS_OUTQTY = "0";
                                        mi_whinv.EXG_INQTY = "0";
                                        mi_whinv.EXG_OUTQTY = "0";
                                        mi_whinv.MIL_INQTY = "0";
                                        mi_whinv.MIL_OUTQTY = "0";
                                        mi_whinv.INVENTORYQTY = "0";
                                        mi_whinv.TUNEAMOUNT = "0";
                                        mi_whinv.USE_QTY = "0";
                                        mi_whinv.TRANS_ID = "U";
                                        repo.CreateMI_WHINV(mi_whinv);
                                    }
                                    #endregion

                                    #region create mi_winvctl
                                    if (repo.CheckMiWinvctlExists(wh_no, mi_mast.MMCODE) == false)
                                    {
                                        // 特殊轉換 上級庫
                                        var supply_whno = repo.GetWhno_mm1();
                                        if (chk_wh_kind == "0")
                                        {
                                            supply_whno = repo.GetWhno_me1();
                                        }

                                        MI_WINVCTL mi_winvctl = new MI_WINVCTL();
                                        mi_winvctl.WH_NO = wh_no;
                                        mi_winvctl.MMCODE = mi_mast.MMCODE;
                                        mi_winvctl.SAFE_DAY = "15";
                                        mi_winvctl.OPER_DAY = "15";
                                        mi_winvctl.SHIP_DAY = "0";
                                        mi_winvctl.SAFE_QTY = 0;
                                        mi_winvctl.OPER_QTY = 0;
                                        mi_winvctl.SHIP_QTY = 0;
                                        mi_winvctl.DAVG_USEQTY = 0;
                                        mi_winvctl.HIGH_QTY = 1000;
                                        mi_winvctl.LOW_QTY = 0;
                                        mi_winvctl.MIN_ORDQTY = 1;
                                        mi_winvctl.SUPPLY_WHNO = supply_whno;
                                        repo.CreateMI_WINVCTL(mi_winvctl);
                                    }

                                    #endregion
                                }

                                #region create mi_winvmon
                                if (repo.CheckMiWinvmonExists(ym, wh_no, item.MMCODE) == false)
                                {
                                    MI_WINVMON mi_winvmon = new MI_WINVMON();
                                    mi_winvmon.DATA_YM = ym;
                                    mi_winvmon.WH_NO = wh_no;
                                    mi_winvmon.MMCODE = item.MMCODE;
                                    mi_winvmon.INV_QTY = "0";
                                    mi_winvmon.APL_INQTY = "0";
                                    mi_winvmon.APL_OUTQTY = "0";
                                    mi_winvmon.TRN_INQTY = "0";
                                    mi_winvmon.TRN_OUTQTY = "0";
                                    mi_winvmon.ADJ_INQTY = "0";
                                    mi_winvmon.ADJ_OUTQTY = "0";
                                    mi_winvmon.BAK_INQTY = "0";
                                    mi_winvmon.BAK_OUTQTY = "0";
                                    mi_winvmon.REJ_OUTQTY = "0";
                                    mi_winvmon.DIS_OUTQTY = "0";
                                    mi_winvmon.EXG_INQTY = "0";
                                    mi_winvmon.EXG_OUTQTY = "0";
                                    mi_winvmon.MIL_INQTY = "0";
                                    mi_winvmon.MIL_OUTQTY = "0";
                                    mi_winvmon.INVENTORYQTY = "0";
                                    mi_winvmon.TUNEAMOUNT = "0";
                                    mi_winvmon.USE_QTY = "0";
                                    mi_winvmon.TURNOVER = "0";
                                    mi_winvmon.SAFE_QTY = "0";
                                    mi_winvmon.OPER_QTY = "0";
                                    mi_winvmon.SHIP_QTY = "0";
                                    mi_winvmon.DAVG_USEQTY = "0";
                                    mi_winvmon.ONWAY_QTY = "0";
                                    mi_winvmon.SAFE_DAY = "15";
                                    mi_winvmon.OPER_DAY = "15";
                                    mi_winvmon.SHIP_DAY = "15";
                                    mi_winvmon.HIGH_QTY = "0";
                                    mi_winvmon.ORI_INV_QTY = "0";
                                    mi_winvmon.ORI_USE_QTY = "0";
                                    repo.CreateMI_WINVMON(mi_winvmon);
                                }
                                #endregion

                                #region create mi_whinv
                                if (repo.CheckMiWhinvExists(wh_no, item.MMCODE) == false)
                                {
                                    MI_WHINV mi_whinv = new MI_WHINV();
                                    mi_whinv.WH_NO = wh_no;
                                    mi_whinv.MMCODE = item.MMCODE;
                                    mi_whinv.INV_QTY = "0";
                                    mi_whinv.ONWAY_QTY = "0";
                                    mi_whinv.APL_INQTY = "0";
                                    mi_whinv.APL_OUTQTY = "0";
                                    mi_whinv.TRN_INQTY = "0";
                                    mi_whinv.TRN_OUTQTY = "0";
                                    mi_whinv.ADJ_INQTY = "0";
                                    mi_whinv.ADJ_OUTQTY = "0";
                                    mi_whinv.BAK_INQTY = "0";
                                    mi_whinv.BAK_OUTQTY = "0";
                                    mi_whinv.REJ_OUTQTY = "0";
                                    mi_whinv.DIS_OUTQTY = "0";
                                    mi_whinv.EXG_INQTY = "0";
                                    mi_whinv.EXG_OUTQTY = "0";
                                    mi_whinv.MIL_INQTY = "0";
                                    mi_whinv.MIL_OUTQTY = "0";
                                    mi_whinv.INVENTORYQTY = "0";
                                    mi_whinv.TUNEAMOUNT = "0";
                                    mi_whinv.USE_QTY = "0";
                                    mi_whinv.TRANS_ID = "U";
                                    repo.CreateMI_WHINV(mi_whinv);
                                }
                                #endregion

                                #region create mi_winvctl
                                if (repo.CheckMiWinvctlExists(wh_no, item.MMCODE) == false)
                                {
                                    // 特殊轉換 上級庫
                                    var supply_whno = repo.GetWhno_mm1();
                                    if (chk_wh_kind == "0")
                                    {
                                        supply_whno = repo.GetWhno_me1();
                                    }

                                    MI_WINVCTL mi_winvctl = new MI_WINVCTL();
                                    mi_winvctl.WH_NO = wh_no;
                                    mi_winvctl.MMCODE = item.MMCODE;
                                    mi_winvctl.SAFE_DAY = "15";
                                    mi_winvctl.OPER_DAY = "15";
                                    mi_winvctl.SHIP_DAY = "0";
                                    mi_winvctl.SAFE_QTY = 0;
                                    mi_winvctl.OPER_QTY = 0;
                                    mi_winvctl.SHIP_QTY = 0;
                                    mi_winvctl.DAVG_USEQTY = 0;
                                    mi_winvctl.HIGH_QTY = 1000;
                                    mi_winvctl.LOW_QTY = 0;
                                    mi_winvctl.MIN_ORDQTY = 1;
                                    mi_winvctl.SUPPLY_WHNO = supply_whno;
                                    repo.CreateMI_WINVCTL(mi_winvctl);
                                }

                                #endregion
                                
                                // E.針對匯入之EXCEL有填盤點量資料進行CHK_DETAIL更新與計算消耗及差異量(目前為全部都檢查，請改成只檢查有填盤點量的資料)
                                // E.a.更新CHK_DETAIL盤點量、調整消耗
                                session.Result.afrs = repo.UpdateChkDetail(item); // b.更新CHK_DETAIL盤點量、調整消耗 

                                // 20240216: 增加更新CHK_DETAILTOT、MI_WINVMON、MI_WHINV
                                // 檢查是否存在於CHK_DETAILTOT，不存在新增，存在更新
                                bool isNew = false;
                                if (repo.CheckChkTotExistsMMCODE(item.CHK_NO, item.MMCODE) == false)
                                {
                                    isNew = true;
                                    session.Result.afrs = repo.InsertChkDetailtot(item.CHK_NO, item.MMCODE, DBWork.UserInfo.UserId, DBWork.ProcIP);
                                }
                                else {
                                    // 1. 更新CHK_DETAILTOT
                                    session.Result.afrs = repo.UpdateChkDetailtot(item);
                                }

                                // 非新資料，表示已存在於盤點單中，才可修改相關異動量
                                if (isNew == false) {
                                    // 2. 更新MI_WINVMON (先修改回原資料)
                                    session.Result.afrs = repo.UpdateMiwinvmonRollback(item);

                                    // 4.insert MI_WHTRNS (先修改回原資料)
                                    session.Result.afrs = repo.InsertWhtrnsRollback(item);

                                    // 3. 更新MI_WHINV (先修改回原資料)
                                    session.Result.afrs = repo.UpdateMiwhinvRollback(item);

                                    //
                                    item.WH_NAME = repo.GetWhname(item.CHK_WH_NO);
                                    //取得庫存異動量
                                    item.GAP_T = repo.GetTrinvqty(item);
                                    item.USE_QTY_AF_CHK = repo.GetUseQtyAfChk(item);
                                    item.INVENTORY = repo.GetInventory(item);
                                    item.USE_QTY = repo.GetUseQty(item);
                                    item.ALTERED_USE_QTY = repo.GetAlteredUseQty(item);

                                    // 5. 以新資料更新MI_WINVMON
                                    session.Result.afrs = repo.UpdateMiWinvmon(item);

                                    // 7. insert MI_WHTRNS
                                    session.Result.afrs = repo.InsertWhtrns(item);

                                    // 6. 以新資料更新MI_WHINV
                                    session.Result.afrs = repo.UpdateMiwhinv(item);
                                }

                                
                            }
                            catch
                            {
                                throw;
                            }
                            chk_detail_list.Add(item);
                        }
                    }
                    //F.判斷庫房更新CHK_DERAIL計算消耗及差異
                    session.Result.afrs = repo.UpdateChkDetail_c1(chk_no); // --1.一級庫計算差異 = 盤盈虧
                    session.Result.afrs = repo.UpdateChkDetail_c2(chk_no, ""); // --2.供應中心計算差異 = 盤盈虧
                    session.Result.afrs = repo.UpdateChkDetail_c3(chk_no); //-- 3.藥局可調整消耗，需重新計算差異量
                    session.Result.afrs = repo.UpdateChkDetail_c4(chk_no, ""); //-- 4.單位若chk_qty <= store_qtyc，消耗 = store_qtyc - chk_qty；若否，消耗 = 0，差異 = (chk_qty - store_qtyc)

                    session.Result.afrs = repo.UpdateChkTotal(chk_no);
                    session.Result.afrs = repo.UpdateChkNum(chk_no);

                    foreach (CHK_DETAIL item in list) {
                        // 8. 檢查是否有CHK_EXPINV_TRNS，若有則先還原MI_WEXPINV
                        IEnumerable<CHK_EXPLOC> exps = repo.GetChkexpinvtrns(item.CHK_NO, item.MMCODE);
                        foreach (CHK_EXPLOC exp in exps)
                        {
                            // 還原MI_WEXPINV
                            session.Result.afrs = repo.UpdateMiwexpinvRollback(exp);
                        }
                        session.Result.afrs = repo.DeleteChkexpinvtrns(item.CHK_NO, item.MMCODE);

                        // 9. 取得MI_WEXPINV資料
                        // 檢查是否有批號效期資料
                        //取得WH_NO
                        item.CHK_WH_NO = repo.GetChkWhNo(item.CHK_NO);
                        item.CHK_WH_GRADE = repo.GetChkWhGrade(item.CHK_NO);
                        item.CHK_WH_KIND = repo.GetChkWhKind(item.CHK_NO);
                        exps = repo.GetExpinvs(item.CHK_NO, item.CHK_WH_NO, item.MMCODE).ToList<CHK_EXPLOC>();
                        string use_qty = repo.GetNewUseQty(item);

                        // 檢查電腦量與批號效期總量是否相符
                        string exp_diff = repo.GetExpInvDiff(item.CHK_NO, item.MMCODE);
                        if (exp_diff != "0")
                        {
                            // 不相符，修改9991231 TMPLOT
                            repo.MergeExpinv(item.CHK_WH_NO, item.MMCODE, exp_diff);
                        }
                        // 重取
                        exps = repo.GetExpinvs(item.CHK_NO, item.CHK_WH_NO, item.MMCODE).ToList<CHK_EXPLOC>();

                        double use_qty_d = double.Parse(use_qty);

                        if (use_qty_d < 0)
                        {
                            // 若無9991231 TMPLOT則新增
                            CHK_EXPLOC temp = new CHK_EXPLOC()
                            {
                                WH_NO = item.CHK_WH_NO,
                                MMCODE = item.MMCODE,
                                EXP_DATE = "9991231",
                                LOT_NO = "TMPLOT",
                                TRN_QTY = "0"
                            };
                            if (repo.CheckMiwexpinvExists(temp) == false)
                            {
                                repo.InsertMiwexpinv(temp);
                            }
                        }

                        // 重取
                        exps = repo.GetExpinvs(item.CHK_NO, item.CHK_WH_NO, item.MMCODE).ToList<CHK_EXPLOC>();

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

                            if (ori_qty_d >= use_qty_d && use_qty_d != 0)
                            {
                                exp.TRN_QTY = use_qty;
                                use_qty_d = use_qty_d - double.Parse(exp.TRN_QTY);
                            }
                            else if (ori_qty_d < use_qty_d && use_qty_d != 0)
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
                        // 10. 檢查是否有CHK_LOCINV_TRNS，若有則先還原MI_WLOCINV
                        IEnumerable<CHK_EXPLOC> locs = repo.GetChklocinvtrns(item.CHK_NO, item.MMCODE);
                        foreach (CHK_EXPLOC loc in locs)
                        {
                            // 還原MI_WLOCINV
                            session.Result.afrs = repo.UpdateMiwlocinvRollback(loc);
                        }
                        session.Result.afrs = repo.DeleteChklocinvtrns(item.CHK_NO, item.MMCODE);
                        // 11. 取得MI_WLOCINV資料
                        // 檢查是否有儲位資料
                        locs = repo.GetLocinvs(item.CHK_NO, item.CHK_WH_NO, item.MMCODE).ToList<CHK_EXPLOC>();

                        // 檢查電腦量與儲位總量是否相符
                        string loc_diff = repo.GetLocInvDiff(item.CHK_NO, item.MMCODE);
                        if (loc_diff != "0")
                        {
                            // 不相符，修改TMPLOC
                            repo.MergeLocinv(item.CHK_WH_NO, item.MMCODE, loc_diff);
                        }
                        // 重取
                        locs = repo.GetLocinvs(item.CHK_NO, item.CHK_WH_NO, item.MMCODE).ToList<CHK_EXPLOC>();

                        use_qty_d = double.Parse(use_qty);

                        if (use_qty_d < 0)
                        {
                            // 若無9991231 TMPLOT則新增
                            CHK_EXPLOC temp = new CHK_EXPLOC()
                            {
                                WH_NO = item.CHK_WH_NO,
                                MMCODE = item.MMCODE,
                                STORE_LOC = "TMPLOC",
                                TRN_QTY = "0"
                            };
                            if (repo.CheckMiwlocinvExists(temp) == false)
                            {
                                repo.InsertMiwlocinv(temp);
                            }
                        }

                        foreach (CHK_EXPLOC loc in locs)
                        {
                            double ori_qty_d = double.Parse(loc.ORI_INV_QTY);
                            if (ori_qty_d <= 0)
                            {
                                // 無存量，跳過
                                continue;
                            }

                            if (ori_qty_d >= use_qty_d && use_qty_d != 0)
                            {
                                loc.TRN_QTY = use_qty;
                                use_qty_d = use_qty_d - double.Parse(loc.TRN_QTY);
                            }
                            else if (ori_qty_d < use_qty_d && use_qty_d != 0)
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

                    session.Result.etts = chk_detail_list;

                    if (isDuplicate == false)
                    {
                        DBWork.Commit();
                    }
                    else
                    {
                        DBWork.Rollback();
                    }
                }
                catch (Exception ex)
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }
        #endregion
        public bool CheckListDupMMCODE(string pr_no, string mmcode, int chkIdx, DataTable dt)
        {
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i]["盤點單號"].ToString() == pr_no && dt.Rows[i]["院內碼"].ToString() == mmcode && i != chkIdx)
                    return false;
            }
            return true;
        }

        public class HeaderItem
        {
            public string Name { get; set; }
            public string FieldName { get; set; }
            public int Index { get; set; }
            public HeaderItem()
            {
                Name = string.Empty;
                Index = -1;
                FieldName = string.Empty;
            }
            public HeaderItem(string name, int index, string fieldName)
            {
                Name = name;
                Index = index;
                FieldName = fieldName;
            }
            public HeaderItem(string name, string fieldName)
            {
                Name = name;
                Index = -1;
                FieldName = fieldName;
            }
        }

        public List<HeaderItem> SetHeaderIndex(List<HeaderItem> list, IRow headerRow)
        {
            int cellCounts = headerRow.LastCellNum;
            for (int i = 0; i < cellCounts; i++)
            {
                foreach (HeaderItem item in list)
                {
                    if (headerRow.GetCell(i).ToString() == item.Name)
                    {
                        item.Index = i;
                    }
                }
            }

            return list;
        }


        [HttpPost]
        public ApiResponse UpdateGapToUse(FormDataCollection form) {
            string chk_no = form.Get("chk_no");
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    

                    var repo = new CE0041Repository(DBWork);
                    // 1. 消耗量-差異量後更新至調整後消耗
                    session.Result.afrs = repo.UpdateGapToUse(chk_no);

                    // 2. 更新消耗及差異
                    session.Result.afrs = repo.UpdateChkDetail_c3(chk_no);
                    //   session.Result.afrs = repo.UpdateChkDetail_c3(item.CHK_NO); //-- 3.藥局可調整消耗，需重新計算差異量

                    IEnumerable<CHK_DETAIL> list = repo.GetAllDetails(chk_no);
                    foreach (CHK_DETAIL item in list) {
                        // 20240216: 增加更新CHK_DETAILTOT、MI_WINVMON
                        // 總數量不更新，不更改MI_WHTRNS、MI_WHINV、MI_WEXPINV、MI_WLOCINV
                        // 1. 更新CHK_DETAILTOT
                        session.Result.afrs = repo.UpdateChkDetailtot(item);

                        // 2. 更新MI_WINVMON (先修改回原資料)
                        session.Result.afrs = repo.UpdateMiwinvmonRollback(item);

                        //
                        item.WH_NAME = repo.GetWhname(item.CHK_WH_NO);
                        //取得庫存異動量
                        item.GAP_T = repo.GetTrinvqty(item);
                        item.USE_QTY_AF_CHK = repo.GetUseQtyAfChk(item);
                        item.INVENTORY = repo.GetInventory(item);
                        item.USE_QTY = repo.GetUseQty(item);
                        item.ALTERED_USE_QTY = repo.GetAlteredUseQty(item);

                        // 5. 以新資料更新MI_WINVMON
                        session.Result.afrs = repo.UpdateMiWinvmon(item);

                    }

                    DBWork.Commit();
                    session.Result.success = true;
                    return session.Result;
                }
                catch (Exception e)
                {
                    DBWork.Rollback();

                    throw;

                }
            }
      
        }

    }
}