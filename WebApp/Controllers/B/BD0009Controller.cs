using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.B;
using WebApp.Models;
using System.Data;
using System;

namespace WebApp.Controllers.B
{
    public class BD0009Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0"); // 庫別代碼
            var p1 = form.Get("p1"); // 採購日期
            var p1_1 = form.Get("p1_1"); // 採購日期(yyymmdd)
            var p2 = form.Get("p2"); // 載入類別
            var p4 = form.Get("p4"); // 廠商代碼
            // var p5 = form.Get("p5"); // 本日出帳
            var p6 = form.Get("p6"); // 已轉出訂單
            var p7 = form.Get("p7"); // 申購中
            var p8 = form.Get("p8"); // 院內碼
            var reCalc = form.Get("recalc"); // 是否為重算
            var reCalcDisRatio = form.Get("reCalcDisRatio");// 是否為重算
            var icalc = form.Get("icalc"); // 實際採購量重算倍率
            var disRatio = form.Get("disRatio"); //單次採購優惠數量百分比
            var getLowFlag = form.Get("getLowFlag"); // 是否抓[低於安全存量]資料
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = "[" + form.Get("sort").ToString().Replace("[", "").Replace("]", "") + ", {\"property\":\"MMCODE\",\"direction\":\"ASC\"}, {\"property\":\"MMNAME_E\",\"direction\":\"ASC\"}]";
            var today = System.DateTime.Now.ToString("yyyyMMdd");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BD0009Repository(DBWork);
                    // 改為call procedure
                    //if (repo.ChkMmPoT(p1) == 0)
                    //    repo.Create(p0, p1, User.Identity.Name, DBWork.ProcIP);
                    if (icalc == "") icalc = repo.GetCalc(p1_1);
                    string maxcnt = repo.GetMaxcnt(p1_1).ToString();
                    if (p1.Substring(0, 10).Replace("-", "").Replace("/", "").Equals(today))
                    {
                        if (reCalc == "Y")
                        {
                            repo.recalc_1(p1_1, User.Identity.Name, DBWork.ProcIP, icalc);
                            session.Result.afrs = repo.recalc(p1_1, User.Identity.Name, DBWork.ProcIP, icalc);
                        }
                        else if (reCalcDisRatio == "Y")
                        {
                            repo.recalc_1(p1_1, User.Identity.Name, DBWork.ProcIP, icalc);
                            session.Result.afrs = repo.recalc(p1_1, User.Identity.Name, DBWork.ProcIP, icalc);
                            session.Result.afrs = repo.RecalcDisRatio(p1_1);
                            session.Result.afrs = repo.CalcDisPoQty(disRatio, p1_1, DBWork.UserInfo.UserId, DBWork.ProcIP);
                        }
                        else if (getLowFlag == "Y")
                        { // 當天是否抓[低於安全存量]資料, 先刪除舊資料
                            repo.ClearData(p1_1);
                            repo.CallSP_BD0009_INSERT(reCalc, p1_1, p0, User.Identity.Name, DBWork.ProcIP, icalc);
                        }
                        else
                        {
                            repo.UpdateADVISEQTY(p1_1, User.Identity.Name, DBWork.ProcIP);
                            
                        }
                        DBWork.Commit();
                        //GetAll()與GetAll_1() SQL 目前一樣
                        //if (getLowFlag == "Y") // 當天是否抓[低於安全存量]資料
                        session.Result.etts = repo.GetAll(p0, p1_1, p2, p4, false, Convert.ToBoolean(p6), Convert.ToBoolean(p7), p8, maxcnt, reCalc, icalc, page, limit, sorters);
                        //else
                        //session.Result.etts = repo.GetAll_1(p0, p1_1, p2, p4, false, Convert.ToBoolean(p6), Convert.ToBoolean(p7), p8, maxcnt, reCalc, icalc, page, limit, sorters);
                    }
                    else
                    {
                        //session.Result.etts = repo.GetAll_1(p0, p1_1, p2, p4, false, Convert.ToBoolean(p6), Convert.ToBoolean(p7), p8, maxcnt, reCalc, icalc, page, limit, sorters);
                        session.Result.etts = repo.GetAll(p0, p1_1, p2, p4, false, Convert.ToBoolean(p6), Convert.ToBoolean(p7), p8, maxcnt, reCalc, icalc, page, limit, sorters);
                    }
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetImportItems(FormDataCollection form)
        {
            var p0 = form.Get("p0"); // 庫別代碼
            var p1 = form.Get("p1"); // 採購日期
            var p2 = form.Get("p2"); // 載入類別
            var pm = form.Get("pm"); // 載入院內碼
            var agen_no = form.Get("agen_no"); // 載入廠商
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = "[" + form.Get("sort").ToString().Replace("[", "").Replace("]", "") + ", {\"property\":\"MMNAME_E\",\"direction\":\"ASC\"}]";
            string[] arr_pm = { };
            if (!string.IsNullOrEmpty(pm))
            {
                arr_pm = pm.Trim().Split(','); //用,分割
            }
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BD0009Repository(DBWork);
                    session.Result.etts = repo.GetImportItems(p0, p1, p2, arr_pm, agen_no, page, limit, sorters);
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse ClearData(FormDataCollection form)
        {
            var p1 = form.Get("purdate"); // 採購日期

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BD0009Repository(DBWork);
                    repo.ClearData(p1);
                    DBWork.Commit();
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse ChkMmPoT(FormDataCollection form)
        {
            var purdate = form.Get("purdate"); // 採購日期

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                var repo = new BD0009Repository(DBWork);
                if (repo.ChkMmPoT(purdate) == 0)
                    session.Result.msg = "";
                else
                    session.Result.msg = purdate;
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse DeleteRec(FormDataCollection form)
        {
            var purdate = form.Get("purdate");
            var submitt = form.Get("submitt");
            string[] MmmcodeData = submitt.ToString().Trim().Split('ˋ');
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BD0009Repository(DBWork);
                    for (int i = 0; i < MmmcodeData.Length; i++)
                    {
                        repo.DeleteRec(purdate, MmmcodeData[i]);
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
        }
        [HttpPost]
        public ApiResponse ImportSubmit(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BD0009Repository repo = new BD0009Repository(DBWork);
                    if (form.Get("MMCODE") != "")
                    {
                        string wh_no = form.Get("WH_NO");
                        string purdate = form.Get("PURDATE");
                        string mmcode = form.Get("MMCODE").Substring(0, form.Get("MMCODE").Length - 1); // 去除前端傳進來最後一個逗號
                        string adviseqty = form.Get("ADVISEQTY").Substring(0, form.Get("ADVISEQTY").Length - 1);
                        string inv_qty = form.Get("INV_QTY").Substring(0, form.Get("INV_QTY").Length - 1);
                        string apl_outqty = form.Get("APL_OUTQTY").Substring(0, form.Get("APL_OUTQTY").Length - 1);
                        string apl_inqty = form.Get("APL_INQTY").Substring(0, form.Get("APL_INQTY").Length - 1);
                        string safe_qty = form.Get("SAFE_QTY").Substring(0, form.Get("SAFE_QTY").Length - 1);
                        string oper_qty = form.Get("OPER_QTY").Substring(0, form.Get("OPER_QTY").Length - 1);
                        string ship_qty = form.Get("SHIP_QTY").Substring(0, form.Get("SHIP_QTY").Length - 1);
                        string high_qty = form.Get("HIGH_QTY").Substring(0, form.Get("HIGH_QTY").Length - 1);
                        string min_ordqty = form.Get("MIN_ORDQTY").Substring(0, form.Get("MIN_ORDQTY").Length - 1);
                        string low_qty = form.Get("LOW_QTY").Substring(0, form.Get("LOW_QTY").Length - 1);
                        string allqty = form.Get("ALLQTY").Substring(0, form.Get("ALLQTY").Length - 1);
                        string e_purtype = form.Get("E_PURTYPE").Substring(0, form.Get("E_PURTYPE").Length - 1);
                        string contracno = form.Get("CONTRACNO").Substring(0, form.Get("CONTRACNO").Length - 1);
                        string agen_no = form.Get("AGEN_NO").Substring(0, form.Get("AGEN_NO").Length - 1);
                        string po_price = form.Get("PO_PRICE").Substring(0, form.Get("PO_PRICE").Length - 1);
                        string disc_cprice = form.Get("DISC_CPRICE").Substring(0, form.Get("DISC_CPRICE").Length - 1);
                        string m_discperc = form.Get("M_DISCPERC").Substring(0, form.Get("M_DISCPERC").Length - 1);
                        string unit_swap = form.Get("UNIT_SWAP").Substring(0, form.Get("UNIT_SWAP").Length - 1);
                        string pack_qty0 = form.Get("PACK_QTY0").Substring(0, form.Get("PACK_QTY0").Length - 1);
                        string m_purun = form.Get("M_PURUN").Substring(0, form.Get("M_PURUN").Length - 1);
                        string[] tmpMmcode = mmcode.Split('^');
                        string[] tmpAdviseqty = adviseqty.Split('^');
                        string[] tmpInv_qty = inv_qty.Split('^');
                        string[] tmpApl_outqty = apl_outqty.Split('^');
                        string[] tmpApl_inqty = apl_inqty.Split('^');
                        string[] tmpSafe_qty = safe_qty.Split('^');
                        string[] tmpOper_qty = oper_qty.Split('^');
                        string[] tmpShip_qty = ship_qty.Split('^');
                        string[] tmpHigh_qty = high_qty.Split('^');
                        string[] tmpMin_ordqty = min_ordqty.Split('^');
                        string[] tmpLow_qty = low_qty.Split('^');
                        string[] tmpAllqty = allqty.Split('^');
                        string[] tmpE_purtype = e_purtype.Split('^');
                        string[] tmpContracno = contracno.Split('^');
                        string[] tmpAgen_no = agen_no.Split('^');
                        string[] tmpPo_price = po_price.Split('^');
                        string[] tmpDisc_cprice = disc_cprice.Split('^');
                        string[] tmpM_discperc = m_discperc.Split('^');
                        string[] tmpUnit_swap = unit_swap.Split('^');
                        string[] tmpPack_qty0 = pack_qty0.Split('^');
                        string[] tmpM_purun = m_purun.Split('^');

                        for (int i = 0; i < tmpMmcode.Length; i++)
                        {
                            BD0009 bd0009 = new BD0009();
                            bd0009.WH_NO = wh_no;
                            bd0009.PURDATE = purdate;
                            bd0009.MMCODE = tmpMmcode[i];
                            bd0009.ADVISEQTY = tmpAdviseqty[i];
                            bd0009.INV_QTY = tmpInv_qty[i];
                            bd0009.APL_OUTQTY = tmpApl_outqty[i];
                            bd0009.APL_INQTY = tmpApl_inqty[i];
                            bd0009.SAFE_QTY = tmpSafe_qty[i];
                            bd0009.OPER_QTY = tmpOper_qty[i];
                            bd0009.SHIP_QTY = tmpShip_qty[i];
                            bd0009.HIGH_QTY = tmpHigh_qty[i];
                            bd0009.MIN_ORDQTY = tmpMin_ordqty[i];
                            bd0009.LOW_QTY = tmpLow_qty[i];
                            bd0009.ALLQTY = tmpAllqty[i];
                            bd0009.E_PURTYPE = tmpE_purtype[i];
                            bd0009.CONTRACNO = tmpContracno[i];
                            bd0009.AGEN_NO = tmpAgen_no[i];
                            bd0009.PO_PRICE = tmpPo_price[i];
                            bd0009.DISC_CPRICE = tmpDisc_cprice[i];
                            bd0009.M_DISCPERC = tmpM_discperc[i];
                            bd0009.UNIT_SWAP = tmpUnit_swap[i];
                            bd0009.PACK_QTY0 = tmpPack_qty0[i];
                            bd0009.M_PURUN = tmpM_purun[i];
                            bd0009.CREATE_USER = DBWork.ProcUser;
                            bd0009.UPDATE_IP = DBWork.ProcIP;
                            bd0009.CALC = repo.GetCalc(purdate);
                            bd0009.PO_QTY = tmpAdviseqty[i];
                            if (repo.ChkMmPoTMmcode(purdate, tmpMmcode[i]) > 0)
                            {
                                // 存在MM_PO_T則更新
                                repo.UpdateMmPot2(bd0009);
                            }
                            else
                            {
                                bd0009.ISTRAN = "N";
                                bd0009.ADVQTY_OLD = "0";
                                // 不存在MM_PO_T則新增
                                repo.InsertMmPot(bd0009);
                            }
                        }
                    }
                    DBWork.Commit();
                }
                catch(Exception e)
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse SetSingleMmPot(FormDataCollection form)
        {
            var purdate = form.Get("purdate");
            purdate = purdate.Substring(0, 10);
            purdate = (Convert.ToInt32(purdate.Split('-')[0]) - 1911) + purdate.Split('-')[1] + purdate.Split('-')[2];

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                double aaa = Convert.ToDouble(form.Get("po_qty"));
                try
                {
                    BD0009 bd0009 = new BD0009();
                    bd0009.PURDATE = purdate;
                    bd0009.MMCODE = form.Get("mmcode");
                    bd0009.WH_NO = form.Get("wh_no");
                    bd0009.CONTRACNO = form.Get("contracno");
                    bd0009.AGEN_NO = form.Get("agen_no");
                    bd0009.ISTRAN = form.Get("istran");
                    bd0009.PO_QTY = form.Get("po_qty");
                    bd0009.PO_PRICE = form.Get("po_price");
                    bd0009.DISC_CPRICE = form.Get("disc_cprice");
                    bd0009.M_PURUN = form.Get("m_purun");
                    bd0009.PO_AMT = form.Get("po_amt");
                    bd0009.M_DISCPERC = form.Get("m_discperc");
                    bd0009.MEMO = form.Get("memo");
                    bd0009.UNIT_SWAP = form.Get("unit_swap");
                    bd0009.ADVISEQTY = form.Get("adviseqty");
                    bd0009.E_PURTYPE = form.Get("e_purtype");
                    bd0009.FLAG = form.Get("flag");
                    bd0009.SAFE_QTY = form.Get("safe_qty");
                    bd0009.OPER_QTY = form.Get("oper_qty");
                    bd0009.LOW_QTY = form.Get("low_qty");
                    bd0009.MIN_ORDQTY = form.Get("min_ordqty");
                    bd0009.INV_QTY = form.Get("inv_qty");
                    bd0009.ALLQTY = form.Get("allqty");
                    bd0009.CREATE_USER = DBWork.ProcUser;
                    bd0009.UPDATE_IP = DBWork.ProcIP;
                    bd0009.CALC = form.Get("calc");
                    var repo = new BD0009Repository(DBWork);
                    if (repo.ChkMmPoTMmcode(bd0009.PURDATE, bd0009.MMCODE) > 0)
                        session.Result.afrs += repo.UpdateMmPot(bd0009);
                    else
                        session.Result.afrs += repo.InsertMmPot(bd0009);

                    //if (bd0009.FLAG == "1")
                    //    session.Result.afrs += repo.UpdateMmPot(bd0009);
                    //else if (bd0009.FLAG == "0")
                    //    session.Result.afrs += repo.InsertMmPot(bd0009);

                    DBWork.Commit();
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse SetMultiMmPot(FormDataCollection form)
        {
            var purdate = form.Get("purdate");
            var submitt = form.Get("submitt");
            purdate = purdate.Substring(0, 10);
            purdate = (Convert.ToInt32(purdate.Split('-')[0]) - 1911) + purdate.Split('-')[1] + purdate.Split('-')[2];
            string[] allData = submitt.ToString().Trim().Split('ˋ');

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    for (int i = 0; i < allData.Length; i++)
                    {
                        BD0009 bd0009 = new BD0009();
                        string[] gridRowData = allData[i].Split('^');
                        bd0009.PURDATE = purdate;
                        bd0009.MMCODE = gridRowData[0].ToString();
                        bd0009.WH_NO = gridRowData[1].ToString();
                        bd0009.CONTRACNO = gridRowData[2].ToString();
                        bd0009.AGEN_NO = gridRowData[3].ToString();
                        bd0009.ISTRAN = gridRowData[4].ToString();
                        bd0009.PO_QTY = gridRowData[5].ToString();
                        bd0009.PO_PRICE = gridRowData[6].ToString();
                        bd0009.M_PURUN = gridRowData[7].ToString();
                        bd0009.PO_AMT = gridRowData[8].ToString();
                        bd0009.M_DISCPERC = gridRowData[9].ToString();
                        bd0009.MEMO = gridRowData[10].ToString();
                        bd0009.UNIT_SWAP = gridRowData[11].ToString();
                        bd0009.ADVISEQTY = gridRowData[12].ToString();
                        bd0009.E_PURTYPE = gridRowData[13].ToString();
                        bd0009.FLAG = gridRowData[14].ToString();
                        bd0009.CREATE_USER = DBWork.ProcUser;
                        bd0009.UPDATE_IP = DBWork.ProcIP;

                        var repo = new BD0009Repository(DBWork);
                        if (bd0009.FLAG == "1")
                            session.Result.afrs += repo.UpdateMmPot(bd0009);
                        else if (bd0009.FLAG == "0")
                            session.Result.afrs += repo.InsertMmPot(bd0009);
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
        }

        [HttpPost]
        public ApiResponse updateUpdateUser(FormDataCollection form)
        {
            var purdate = form.Get("purdate");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BD0009Repository(DBWork);
                    session.Result.afrs += repo.updateUpdateUser(purdate, DBWork.ProcUser);

                    DBWork.Commit();
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse SetTran(FormDataCollection form)
        {
            var purdate = form.Get("purdate");
            var submitt = form.Get("submitt");
            var trantype = form.Get("trantype");
            purdate = purdate.Substring(0, 10);
            purdate = (Convert.ToInt32(purdate.Split('-')[0]) - 1911) + purdate.Split('-')[1] + purdate.Split('-')[2];

            string procName = "";
            if (trantype == "T")
                procName = "BD0009";
            else if (trantype == "N")
                procName = "BD0009_1";

            string[] allData = submitt.ToString().Trim().Split('ˋ');

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    string firstPURDATE = "";
                    string firstWH_NO = "";
                    string poqtyDecimalMmcodes = string.Empty;
                    var repo = new BD0009Repository(DBWork);
                    for (int i = 0; i < allData.Length; i++) {
                        string[] gridRowData = allData[i].Split('^');
                        decimal poqty = repo.GetPoQty(purdate, gridRowData[0].ToString(), gridRowData[1].ToString());
                        if ((poqty % 1) > 0 && (poqty % 1) < 1) {
                            if (poqtyDecimalMmcodes != string.Empty) {
                                poqtyDecimalMmcodes += "、";
                            }
                            poqtyDecimalMmcodes += gridRowData[0].ToString();
                        }
                    }
                    if (poqtyDecimalMmcodes != string.Empty) {
                        session.Result.msg = string.Format("<span style='color:red'>實際採購量不可有小數</span>，請檢察下列院內碼：<br>{0}", poqtyDecimalMmcodes);
                        session.Result.success = false;
                        return session.Result;
                    }
                    repo.RecalcDisRatio(purdate);

                    for (int i = 0; i < allData.Length; i++)
                    {
                        BD0009 bd0009 = new BD0009();
                        string[] gridRowData = allData[i].Split('^');
                        bd0009.PURDATE = purdate;
                        firstPURDATE = bd0009.PURDATE;
                        bd0009.MMCODE = gridRowData[0].ToString();
                        bd0009.WH_NO = gridRowData[1].ToString();
                        firstWH_NO = bd0009.WH_NO;
                        bd0009.CREATE_USER = DBWork.ProcUser;
                        bd0009.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs += repo.UpdateTran(bd0009);
                    }
                    session.Result.msg = repo.CallParamaterOutSP(firstPURDATE, firstWH_NO, DBWork.UserInfo.Inid, DBWork.ProcUser, DBWork.ProcIP, procName);


                    DBWork.Commit();
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }

        // 庫房號碼
        [HttpPost]
        public ApiResponse GetWH_NoCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BD0009Repository repo = new BD0009Repository(DBWork);
                    session.Result.etts = repo.GetWH_NoCombo(DBWork.ProcUser);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // 採購總項次
        [HttpPost]
        public ApiResponse GetTotalCntPrice(FormDataCollection form)
        {
            var purdate = form.Get("purdate");
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BD0009Repository repo = new BD0009Repository(DBWork);
                    int totalCnt = repo.GetGridTotalCnt(purdate);
                    int totalPrice = 0;
                    if (totalCnt > 0)
                        totalPrice = repo.GetGridTotalPrice(purdate);
                    session.Result.msg = totalCnt + "_" + totalPrice;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //取得 calc
        [HttpPost]
        public ApiResponse GetCalc(FormDataCollection form)
        {
            var purdate = form.Get("purdate");
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BD0009Repository repo = new BD0009Repository(DBWork);
                    session.Result.msg = repo.GetCalc(purdate);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
    }
}