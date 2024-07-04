using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.C;
using WebApp.Models;
using System.Net.Http;
using Newtonsoft.Json;
using System;
using Newtonsoft.Json.Linq;
using WebApp.Repository.AA;

namespace WebApp.Controllers.C
{
    public class CC0002Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0").Trim();
            var p1 = form.Get("p1").Trim();
            var p2 = form.Get("p2").Trim();
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CC0002Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1, p2, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse DetailAll(FormDataCollection form)
        {
            var p0 = form.Get("p0").Trim();
            var p1 = form.Get("p1").Trim();
            var p2 = form.Get("p2").Trim();
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CC0002Repository(DBWork);
                    session.Result.etts = repo.GetDetailAll(p0, p1, p2, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse DetailAllExp(FormDataCollection form)
        {
            var p0 = form.Get("p0").Trim();
            var p1 = form.Get("p1").Trim();
            var p2 = form.Get("p2").Trim();
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CC0002Repository(DBWork);
                    session.Result.etts = repo.GetDetailAllExp(p0, p1, p2, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse DistAll(FormDataCollection form)
        {
            var p0 = form.Get("p0").Trim();
            var p1 = form.Get("p1").Trim();
            var p2 = form.Get("p2").Trim();
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CC0002Repository(DBWork);
                    session.Result.etts = repo.GetDistAll(p0, p1, p2, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse DistAll_SCAN(FormDataCollection form)
        {
            var p0 = form.Get("p0").Trim();
            var p1 = form.Get("p1").Trim();
            var p2 = form.Get("p2").Trim();
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CC0002Repository(DBWork);
                    session.Result.etts = repo.GetDistAll_SCAN(p0, p1, p2, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse DetailInid(FormDataCollection form)
        {
            var p0 = form.Get("p0").Trim();
            var p1 = form.Get("p1").Trim();

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CC0002Repository(DBWork);
                    session.Result.etts = repo.GetDetailInid(p0, p1);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetLotNoCombo(FormDataCollection form)
        {
            var po_no = form.Get("po_no");
            var mmcode = form.Get("mmcode");
            var agen_no = form.Get("agen_no");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CC0002Repository repo = new CC0002Repository(DBWork);
                    session.Result.etts = repo.GetLotNoData(po_no, mmcode, agen_no);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetLotNoData(FormDataCollection form)
        {
            var po_no = form.Get("po_no");
            var mmcode = form.Get("mmcode");
            var agen_no = form.Get("agen_no");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CC0002Repository repo = new CC0002Repository(DBWork);
                    session.Result.etts = repo.GetLotNoDataForm(po_no, mmcode, agen_no);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Create(CC0002D cc0002d)
        {   //輸入[廠商代碼],有批號效期
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CC0002Repository(DBWork);
                    cc0002d.SEQ = repo.getSeq();
                    cc0002d.BW_SQTY = "0";
                    cc0002d.ACC_USER = User.Identity.Name;
                    /* 避免重複進貨的問題 
                     * 1.檢查 table MM_PO_D[DELI_QTY] +ACC_QTY 是否會超過
                     * 2.檢查 BC_CS_ACC_LOG 是否已新增 ChkAccLogExist()
                    */
                    var chkMSG = repo.DoubleIN(cc0002d);
                    if (chkMSG == "N")
                    {
                        session.Result.afrs = repo.Create(cc0002d);
                        session.Result.afrs += repo.UpdatePhReply(cc0002d);
                        if (cc0002d.M_STOREID == "0" || cc0002d.PO_NO.Substring(1, 3) == "SML") // 非庫備,小額採購預先寫入
                        {
                            session.Result.msg = cc0002d.SEQ;
                            if ((cc0002d.PO_QTY == cc0002d.ACC_QTY) && cc0002d.UNIT_SWAP == "1") //轉換率=1 && [訂單量]=[進貨量]自動分配
                                                                                                 //前端 url:accLogSet 傳 PO_QTY2 [訂單量]=[進貨量]
                            {
                                session.Result.afrs += repo.CreateDistC(cc0002d);
                            }
                            else
                                session.Result.afrs += repo.CreateDistL(cc0002d);
                        }

                        session.Result.msg = repo.procDocin(cc0002d.PO_NO, User.Identity.Name, DBWork.ProcIP);
                        if (session.Result.msg == "接收數量存檔..成功")
                        {
                            DBWork.Commit();
                            session.Result.msg = cc0002d.SEQ; //qty2
                            if ((cc0002d.PO_QTY == cc0002d.ACC_QTY) && cc0002d.UNIT_SWAP == "1") //轉換率=1 && [訂單量]=[進貨量]自動分配
                            {
                                repo.procDistin(cc0002d.PO_NO, cc0002d.MMCODE, User.Identity.Name, DBWork.ProcIP);
                            }
                            repo.procCC0002(cc0002d.PO_NO, User.Identity.Name, DBWork.ProcIP);
                        }
                        else
                            DBWork.Rollback();
                    }
                    else
                    {
                        session.Result.msg = chkMSG;
                    }
                }
                catch
                {
                    DBWork.Rollback();
                    session.Result.msg = "接收數量存檔..失敗";
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse AccAllCreate(FormDataCollection form)
        {   //一鍵接收確認
            string po_no = form.Get("po_no");
            string agen_no = form.Get("agen_no");
            string[] tempData = form.Get("gridData").Split('ˋ');
            string[] distMMCODE = new string[300];
            int i = 0;
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CC0002Repository(DBWork);
                    foreach (string strData in tempData)
                    {
                        CC0002D cc0002d = new CC0002D();
                        cc0002d.PO_NO = po_no;
                        cc0002d.AGEN_NO = agen_no;
                        cc0002d.MMCODE = strData.Split('^')[0];
                        cc0002d.WEXP_ID = strData.Split('^')[1];
                        if (cc0002d.WEXP_ID.Equals("") || cc0002d.WEXP_ID.Equals("null"))
                            cc0002d.WEXP_ID = "";
                        cc0002d.M_STOREID = strData.Split('^')[2];
                        cc0002d.MAT_CLASS = strData.Split('^')[3];
                        cc0002d.PO_QTY = strData.Split('^')[4];
                        cc0002d.M_PURUN = strData.Split('^')[5];
                        cc0002d.UNIT_SWAP = strData.Split('^')[6];
                        cc0002d.EXP_DATE = DateTime.Parse(strData.Split('^')[7]);
                        cc0002d.BW_SQTY = strData.Split('^')[8];
                        if (cc0002d.BW_SQTY == "" || cc0002d.BW_SQTY == null || cc0002d.BW_SQTY == "null")
                            cc0002d.BW_SQTY = "0";
                        cc0002d.INQTY = strData.Split('^')[9];
                        cc0002d.BASE_UNIT = strData.Split('^')[10];
                        cc0002d.ACC_QTY = strData.Split('^')[11];
                        cc0002d.PH_REPLY_SEQ = strData.Split('^')[12];
                        cc0002d.LOT_NO = strData.Split('^')[13];
                        cc0002d.INVOICE = strData.Split('^')[14];
                        cc0002d.SEQ = repo.getSeq();
                        cc0002d.ACC_USER = User.Identity.Name;

                        var retain_qty = Int32.Parse(repo.getRetainQty(po_no, cc0002d.MMCODE));
                        if (Int32.Parse(cc0002d.ACC_QTY) > 0)
                        {
                            /* 避免重複進貨的問題 
                             * 1.檢查 table MM_PO_D[DELI_QTY] +ACC_QTY 是否會超過
                             * 2.檢查 BC_CS_ACC_LOG 是否已新增 ChkAccLogExist()
                            */
                            var chkMSG = repo.DoubleIN(cc0002d);
                            if (chkMSG == "N")
                            {
                                session.Result.afrs += repo.AccAllCreate(cc0002d);
                                session.Result.afrs += repo.UpdatePhReplyNoSeq(cc0002d);
                                if (cc0002d.M_STOREID == "0" || cc0002d.PO_NO.Substring(1, 3) == "SML") // 非庫備,小額採購預先寫入
                                {
                                    // session.Result.msg = cc0002d.SEQ;
                                    //string bw_sqty = cc0002d.BW_SQTY;
                                    //if (cc0002d.BW_SQTY == "" || cc0002d.BW_SQTY == null)
                                    //    bw_sqty = "0";

                                    // ~2020-10-05: 轉換率=1 && [訂單量]=[進貨量] 自動分配
                                    // ~2020-10-06: 轉換率=1 && [訂單量]=[進貨量] && [進貨量]=[申請量] 自動分配
                                    //  2023-04-06: 轉換率=1 && [訂單量]=[進貨量] && [進貨量-中央庫房自留數量]=[申請量] 自動分配
                                    var acc_qty = (Int32.Parse(cc0002d.ACC_QTY) - retain_qty).ToString();
                                    var appqty = repo.GetAppqty(cc0002d.PO_NO, cc0002d.MMCODE);           //申請量
                                    if ((cc0002d.PO_QTY == cc0002d.ACC_QTY) && cc0002d.UNIT_SWAP == "1" && (acc_qty == appqty)) 
                                    {
                                        session.Result.afrs += repo.CreateDistC(cc0002d);
                                        //--庫房縮減:計算中央庫房自留數量，若有自留需insert BC_CS_DIST_LOG(PR_DEPT=56000)
                                        if (retain_qty > 0)
                                        {
                                            cc0002d.DIST_QTY = retain_qty.ToString();  //中央庫房之分配量=自留數量
                                            session.Result.afrs += repo.InsertDIST_LOG_WhnoMm1(cc0002d);
                                        }
                                        distMMCODE[i] = cc0002d.MMCODE;
                                        i++;
                                    }
                                    else
                                    {
                                        cc0002d.ACC_QTY = acc_qty; //一般庫房之分配量=申請量
                                        session.Result.afrs += repo.CreateDistL(cc0002d);
                                        //--庫房縮減:計算中央庫房自留數量，若有自留需insert BC_CS_DIST_LOG(PR_DEPT=56000)
                                        if (retain_qty > 0)
                                        {
                                            cc0002d.DIST_QTY = (retain_qty* int.Parse(cc0002d.UNIT_SWAP)).ToString();
                                            session.Result.afrs += repo.InsertDIST_LOG_WhnoMm1(cc0002d);
                                        }
                                    }
                                        
                                }
                            }
                            else
                                session.Result.msg = chkMSG;
                        }
                    }

                    session.Result.msg = repo.procDocin(po_no, User.Identity.Name, DBWork.ProcIP);
                    if (session.Result.msg == "接收數量存檔..成功")
                    {
                        DBWork.Commit();
                        for (int j = 0; j < distMMCODE.Length; j++)
                        {
                            if (distMMCODE[j] != null)
                                repo.procDistin(po_no, distMMCODE[j], User.Identity.Name, DBWork.ProcIP);                        
                        }
                        repo.procCC0002(po_no, User.Identity.Name, DBWork.ProcIP);
                    }
                    else
                        DBWork.Rollback();
                }
                catch
                {
                    DBWork.Rollback();
                    session.Result.msg = "接收數量存檔..失敗";
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse UpdateDist(CC0002D cc0002d)
        {   //有批號效期
            string[] tmpDistQty = cc0002d.DIST_QTY.Split('^');
            string[] tmpSeq = cc0002d.SEQ.Split('^');
            string[] tmpInid = cc0002d.INID.Split('^');

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CC0002Repository(DBWork);
                    cc0002d.ACC_USER = User.Identity.Name;
                    for (int i = 0; i < tmpDistQty.Length; i++)
                    {
                        if (tmpDistQty[i] == null || tmpDistQty[i] == "null" || tmpDistQty[i] == "")
                            cc0002d.DIST_QTY = "0";
                        else
                            cc0002d.DIST_QTY = tmpDistQty[i];
                        if (i > tmpSeq.Length - 1)
                            cc0002d.SEQ = tmpSeq[0]; // 透過掃描功能接收時SEQ只會有一筆
                        else
                            cc0002d.SEQ = tmpSeq[i];
                        cc0002d.INID = tmpInid[i];
                        if (cc0002d.DIST_QTY != "0")
                            session.Result.afrs += repo.UpdateDist(cc0002d);
                        else  //刪除之前create() 預先 insert BC_CS_DIST_LOG[DIST_STATUS]='L' record
                            session.Result.afrs += repo.DeleteDist(cc0002d);
                    }
                    DBWork.Commit();
                    repo.procDistin(cc0002d.PO_NO, cc0002d.MMCODE, User.Identity.Name, DBWork.ProcIP);
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
        public ApiResponse UpdateScanDist(CC0002D cc0002d)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    string errMsg = string.Empty;
                    var repo = new CC0002Repository(DBWork);
                    cc0002d.SEQ = repo.getSeq();
                    cc0002d.ACC_USER = User.Identity.Name;
                    /* 避免重複進貨的問題 
                     * 1.檢查 table MM_PO_D[DELI_QTY] +ACC_QTY 是否會超過
                     * 2.檢查 BC_CS_ACC_LOG 是否已新增 ChkAccLogExist()
                    */
                    var chkMSG = repo.DoubleIN(cc0002d);
                    if (chkMSG == "N")
                    {
                        //檢查待分配量
                        if (cc0002d.DIST_QTY != "" && cc0002d.DIST_QTY != null)
                        {
                            string[] tmpDistQty = cc0002d.DIST_QTY.Split('^');
                            string[] tmpInid = cc0002d.INID.Split('^');
                            string po_no = cc0002d.PO_NO;
                            string mmcode = cc0002d.MMCODE;
                            string agen_no = cc0002d.AGEN_NO;
                            string inid = null,inidName=null;
                            int curdistqty = 0;
                            

                            for (int i = 0; i < tmpDistQty.Length; i++)
                            {
                                inid = tmpInid[i];
                                if (tmpDistQty[i] == null || tmpDistQty[i] == "null" || tmpDistQty[i] == "")
                                    curdistqty = 0;
                                else
                                    curdistqty = int.Parse(tmpDistQty[i]);

                                if (curdistqty > 0)
                                {
                                    //計算 可分配量=申請量-已分配量(含待分配量)，分配量 不可大於 可分配量
                                    int getCanDistQty = int.Parse(repo.getCanDistQty(po_no, mmcode, agen_no, inid));
                                    if (curdistqty > getCanDistQty)
                                    {
                                        inidName = repo.getInidName(inid);
                                        if (errMsg != "")
                                        {
                                            errMsg +=  "、" ;
                                        }
                                        errMsg += inidName;
                                    }
                                }
                            }
                            if (errMsg != "")
                            {
                                errMsg = "<br/><span style='color:red'>● 分配量 大於 申請量-已分配量(含待分配量)之申請單位</span>：<br/>"+ errMsg;
                            }
                        }

                        if (errMsg == "")
                        {
                            session.Result.afrs += repo.ScanCreate(cc0002d);
                            session.Result.afrs += repo.UpdatePhReply(cc0002d);

                            session.Result.msg = repo.procDocin(cc0002d.PO_NO, User.Identity.Name, DBWork.ProcIP);
                            if (session.Result.msg == "接收數量存檔..成功")
                            {
                                //全部視為已分配 , 分配申請單位
                                if (cc0002d.DIST_QTY != "" && cc0002d.DIST_QTY != null)
                                {
                                    string[] tmpDistQty = cc0002d.DIST_QTY.Split('^');
                                    string[] tmpSeq = cc0002d.SEQ.Split('^');
                                    string[] tmpInid = cc0002d.INID.Split('^');
                                    int sumDistQty = 0; //計算分配量總和

                                    for (int i = 0; i < tmpDistQty.Length; i++)
                                    {
                                        if (tmpDistQty[i] == null || tmpDistQty[i] == "null" || tmpDistQty[i] == "")
                                            cc0002d.DIST_QTY = "0";
                                        else
                                            cc0002d.DIST_QTY = tmpDistQty[i];
                                        if (i > tmpSeq.Length - 1)
                                            cc0002d.SEQ = tmpSeq[0]; // 透過掃描功能接收時SEQ只會有一筆
                                        else
                                            cc0002d.SEQ = tmpSeq[i];
                                        cc0002d.INID = tmpInid[i];
                                        //UNIT_SWAP 轉換率== "1" 才可以分配
                                        if (cc0002d.DIST_QTY != "0" && cc0002d.UNIT_SWAP == "1")
                                            session.Result.afrs += repo.InsertDIST_LOG(cc0002d, "C");
                                        else
                                        {
                                            if (cc0002d.DIST_QTY != "0")
                                                session.Result.afrs += repo.InsertDIST_LOG(cc0002d, "L");
                                        }
                                        //庫房縮減:計算總分配量
                                        sumDistQty = sumDistQty + int.Parse(cc0002d.DIST_QTY);
                                    }

                                    //--庫房縮減:計算中央庫房自留數量(進貨數量-總分配量>0)，若此次進貨有自留需insert BC_CS_DIST_LOG(PR_DEPT=56000)
                                    int nonDistqty = int.Parse(cc0002d.ACC_QTY) - sumDistQty;
                                    if (cc0002d.UNIT_SWAP != "1")
                                    {
                                        nonDistqty = nonDistqty * int.Parse(cc0002d.UNIT_SWAP);
                                    }
                                    if (nonDistqty > 0)
                                    {
                                        cc0002d.DIST_QTY = nonDistqty.ToString();
                                        session.Result.afrs += repo.InsertDIST_LOG_WhnoMm1(cc0002d);
                                    }
                                    //--END
                                    DBWork.Commit();
                                    repo.procDistin(cc0002d.PO_NO, cc0002d.MMCODE, User.Identity.Name, DBWork.ProcIP);
                                }
                                repo.procCC0002(cc0002d.PO_NO, User.Identity.Name, DBWork.ProcIP);
                            }
                            else
                                DBWork.Rollback();
                        }
                        else {
                            session.Result.msg = errMsg;
                        }
                    }
                    else
                    {
                        session.Result.msg = "接收數量存檔..成功";
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
        public ApiResponse ChkAgenno(FormDataCollection form)
        {
            var p0 = form.Get("p0");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CC0002Repository(DBWork);
                    string rtnMsg = "";
                    foreach (string val in repo.ChkAgenno(p0))
                        rtnMsg = val;
                    session.Result.msg = rtnMsg;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse ChkBarcode(FormDataCollection form)
        {
            var p0 = form.Get("p0"); // 條碼
            var p1 = form.Get("p1"); // 訂單日期(起)
            var p11 = form.Get("p11"); // 訂單日期(迄)

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    string rtnMsg = "";
                    var repo = new CC0002Repository(DBWork);

                    //int p1Cnt = 0;
                    //foreach (CC0002D val in repo.GetBarCodeDetailP1(p0))
                    //    p1Cnt++;
                    session.Result.etts = repo.GetBarCodeDetailP1(p0, p1, p11);
                    if (((System.Collections.Generic.List<WebApp.Models.CC0002D>)session.Result.etts).Count == 0)
                    {
                        string barcodeChk = repo.ChkMmcodeBarcode(p0);
                        if (barcodeChk == "notfound")
                            rtnMsg = "bcnotfound";
                        else
                        {
                            //int p3Cnt = 0;
                            //foreach (CC0002D val in repo.GetBarCodeDetailP3(barcodeChk))
                            //    p3Cnt++;
                            //if (p3Cnt == 0)
                            //    rtnMsg = "ponotfound";
                            //else
                            session.Result.etts = repo.GetBarCodeDetailP3(barcodeChk, p1, p11);
                            if (((System.Collections.Generic.List<WebApp.Models.CC0002D>)session.Result.etts).Count == 0)
                            {
                                session.Result.etts = repo.GetTheLastPO(barcodeChk, p1, p11);
                                if (((System.Collections.Generic.List<WebApp.Models.CC0002D>)session.Result.etts).Count == 0)
                                    rtnMsg = "ponotfound";
                            }
                        }
                    }

                    session.Result.msg = rtnMsg;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // 檢查MMCODE:
        // 在BC_BARCODE無對應資料->回傳bcnotfound
        // 在BC_BARCODE有對應資料,在MM_PO_D無對應資料->回傳ponotfound
        // 在BC_BARCODE, MM_PO_D都有對應資料->回傳MMCODE
        [HttpPost]
        public ApiResponse ChkMmcode(FormDataCollection form)
        {
            var p0 = form.Get("p0"); // 院內碼
            var p1 = form.Get("p1"); // 訂購單號

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    string rtnMsg = "";
                    var repo = new CC0002Repository(DBWork);
                    string barcodeChk = repo.ChkMmcodeBarcode(p0, p1); // 若查詢有資料則回傳MMCODE, 否則回傳notfound
                    if (barcodeChk == "notfound")
                        rtnMsg = "bcnotfound";
                    else
                    {
                        int polistChk = repo.ChkMmcodePo(barcodeChk, p1);
                        if (polistChk > 0)
                            rtnMsg = barcodeChk;
                        else
                            rtnMsg = "ponotfound";
                    }
                    session.Result.msg = rtnMsg;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetHadDistQty(FormDataCollection form)
        {
            var po_no = form.Get("po_no");
            var mmcode = form.Get("mmcode");
            var seq = form.Get("seq");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CC0002Repository(DBWork);
                    session.Result.msg = repo.GetHadDistQty(po_no, mmcode, seq);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse CC0006_ChkWHIINV(FormDataCollection form)
        {
            var po_no = form.Get("po_no");
            var mmcode = form.Get("mmcode");
            var lot_no = form.Get("lot_no");
            var exp_date = form.Get("exp_date");
            int inqty = Convert.ToInt32(form.Get("inqty"));
            var storeid = form.Get("storeid");
            int inv_qty = 0;

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    CC0002D cc0002d = new CC0002D();
                    var repo = new CC0002Repository(DBWork);
                    cc0002d.PO_NO = po_no;
                    cc0002d.MMCODE = mmcode;
                    cc0002d.LOT_NO = lot_no;
                    if (storeid == "0") // 庫備檢查 mi_whinv存量
                        inv_qty = repo.CC0006_ChkONWAY_QTY(cc0002d);
                    else
                        inv_qty = repo.CC0006_ChkINV_QTY(cc0002d);

                    if (inv_qty < (inqty * -1))
                        session.Result.msg = "N"; //存量不足
                    else
                        session.Result.msg = "Y";
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse CC0006_UpdateScanDist(CC0002D cc0002d)
        {
            // for CC0006 調整處理順序
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CC0002Repository(DBWork);
                    cc0002d.SEQ = repo.getSeq();
                    cc0002d.ACC_USER = User.Identity.Name;
                    /* 避免重複進貨的問題 
                     * 1.檢查 table MM_PO_D[DELI_QTY] +ACC_QTY 是否會超過
                     * 2.檢查 BC_CS_ACC_LOG 是否已新增 ChkAccLogExist()
                    */
                    var chkMSG = repo.DoubleIN(cc0002d);
                    int onway_qty = 0;
                    if (chkMSG == "N")
                    {
                        session.Result.afrs += repo.ScanCreate(cc0002d);
                        session.Result.afrs += repo.UpdatePhReply(cc0002d);

                        if (cc0002d.DIST_QTY != "" && cc0002d.DIST_QTY != null)
                        {
                            string[] tmpDistQty = cc0002d.DIST_QTY.Split('^');
                            string[] tmpSeq = cc0002d.SEQ.Split('^');
                            string[] tmpInid = cc0002d.INID.Split('^');

                            for (int i = 0; i < tmpDistQty.Length; i++)
                            {
                                if (tmpDistQty[i] == null || tmpDistQty[i] == "null" || tmpDistQty[i] == "")
                                    cc0002d.DIST_QTY = "0";
                                else
                                    cc0002d.DIST_QTY = tmpDistQty[i];
                                if (i > tmpSeq.Length - 1)
                                    cc0002d.SEQ = tmpSeq[0]; // 透過掃描功能接收時SEQ只會有一筆
                                else
                                    cc0002d.SEQ = tmpSeq[i];
                                cc0002d.INID = tmpInid[i];
                                if (Convert.ToInt32(cc0002d.DIST_QTY) < 0)  // 分配量負值,檢查未點收在途量
                                {
                                    onway_qty = repo.CC0006_ChkONWAY_QTY_INID(cc0002d);
                                    if (onway_qty < (Convert.ToInt32(cc0002d.DIST_QTY) * -1))
                                        chkMSG = "此品項申請單位未點收量不夠扣減"; //未點收在途量不足
                                    else
                                    {
                                        //UNIT_SWAP 轉換率== "1" 才可以分配
                                        if (cc0002d.DIST_QTY != "0" && cc0002d.UNIT_SWAP == "1")
                                            session.Result.afrs += repo.InsertDIST_LOG(cc0002d, "C");
                                        else
                                            session.Result.afrs += repo.InsertDIST_LOG(cc0002d, "L");
                                    }
                                }
                                else
                                {
                                    //UNIT_SWAP 轉換率== "1" 才可以分配
                                    if (cc0002d.DIST_QTY != "0" && cc0002d.UNIT_SWAP == "1")
                                        session.Result.afrs += repo.InsertDIST_LOG(cc0002d, "C");
                                    else
                                        session.Result.afrs += repo.InsertDIST_LOG(cc0002d, "L");
                                }
                            }
                            if (chkMSG == "N")
                            {
                                session.Result.msg = repo.procDistin(cc0002d.PO_NO, cc0002d.MMCODE, User.Identity.Name, DBWork.ProcIP);
                                if (session.Result.msg == "分配數量..成功")
                                {
                                    DBWork.Commit();
                                    session.Result.msg = repo.procDocin(cc0002d.PO_NO, User.Identity.Name, DBWork.ProcIP);
                                    if (session.Result.msg == "接收數量存檔..成功")
                                    {
                                        repo.procCC0002(cc0002d.PO_NO, User.Identity.Name, DBWork.ProcIP);
                                    }
                                    else
                                    {
                                        DBWork.Rollback();
                                    }
                                }
                                else
                                {
                                    DBWork.Rollback();
                                }
                            }
                            else
                            {
                                session.Result.msg = chkMSG;
                            }
                        }
                        else
                        {
                            DBWork.Commit();
                            session.Result.msg = repo.procDocin(cc0002d.PO_NO, User.Identity.Name, DBWork.ProcIP);
                            if (session.Result.msg == "接收數量存檔..成功")
                            {
                                repo.procCC0002(cc0002d.PO_NO, User.Identity.Name, DBWork.ProcIP);
                            }
                            else
                            {
                                DBWork.Rollback();
                            }
                        }
                    }
                    else
                    {
                        session.Result.msg = chkMSG;
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
        public ApiResponse GetUdiData(FormDataCollection form)
        {
            var p0 = form.Get("p0");

            using (WorkSession session = new WorkSession())
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        DecodeRequest request = new DecodeRequest();//請先建立POST 物件Class 見本原始碼最底處
                        request.WmCmpy = "04125805A"; //三總統編
                        //request.CrVmpy = "";
                        request.WmOrg = "010802";
                        request.WmWhs = "010802W";
                        //request.WmOrg = "";
                        //request.WmWhs = "";
                        //request.WmSku = "08080722-088";
                        request.WmSku = "";
                        request.WmPak = "";
                        request.WmQy = "";
                        request.WmLot = "";
                        request.WmEffcDate = "";
                        request.WmSeno = "";
                        request.WmBox = "";
                        request.WmLoc = "";
                        request.WmSrv = "";
                        //request.CrItm = "";
                        //request.ThisBarcode = "01047119081201531730123110LOT123"; //UDI 條碼
                        request.ThisBarcode = p0; //UDI 條碼
                        request.UdiBarcodes = "";
                        request.NhiBarcodes = "";
                        request.GtinString = "";
                        //request.Check();

                        client.BaseAddress = new Uri("https://tsghudi.ndmctsgh.edu.tw/api/DecodeInfo"); //請於ServerIP 填入院內伺服器IP及PORT
                        string stringData = JsonConvert.SerializeObject(request);
                        var data = new StringContent(stringData, System.Text.Encoding.UTF8, "application/json");
                        HttpResponseMessage response = client.PostAsync(client.BaseAddress, data).Result;
                        string newMsg = response.Content.ReadAsStringAsync().Result;
                        // string newMsg = "[{\"WmBox\":\"\",\"WmLoc\":\"\",\"WmSrv\":\"\",\"IsChgItm\":\"N\",\"WmCmpy\":null,\"WmWhs\":null,\"WmOrg\":\"010802\",\"CrVmpy\":\"\",\"CrItm\":\"\",\"WmRefCode\":\"\",\"WmSku\":\"08080722-088\",\"WmMid\":\"08080722\",\"WmMidName\":\"LAUNCHER GUIDE CATHETERS\",\"WmMidNameH\":\"馬克導引導管\",\"WmSkuSpec\":\"MODEL-6F MACH 1 JL4\",\"WmBrand\":\"\",\"WmMdl\":\"\",\"WmMidCtg\":\"SKU\",\"WmEffcDate\":\"\",\"WmLot\":\"\",\"WmSeno\":\"\",\"WmPak\":\"個\",\"WmQy\":\"1\",\"ThisBarcode\":\"8714729351863\",\"UdiBarcodes\":\"8714729351863\",\"GtinString\":\"8714729351863<;>0871\",\"NhiBarcode\":\"(01)8714729351863\",\"NhiBarcodes\":\"(01)8714729351863\",\"BarcodeType\":\"UDI\",\"GtinInString\":\"'8714729351863' '08714729351863'\",\"Result\":\"SUCCESS\",\"ErrMsg\":\"\"}]";
                        newMsg = newMsg.Replace(",\"", "^");
                        newMsg = newMsg.Replace("\"", "");
                        newMsg = newMsg.Replace("[{", "");
                        newMsg = newMsg.Replace("}]", "");
                        newMsg = newMsg.Replace(@"""", "");

                        string[] strArray = newMsg.Split(new Char[] { '^' }, StringSplitOptions.RemoveEmptyEntries);
                        DecodeResponse deResponse = new DecodeResponse();

                        foreach (string innerStr in strArray)
                        {
                            string dataString = innerStr.Substring(innerStr.IndexOf(":") + 1);
                            if (dataString == "null")
                                dataString = "";

                            if (innerStr.IndexOf("WmBox:") >= 0)
                                deResponse.WmBox = dataString;
                            else if (innerStr.IndexOf("WmLoc:") >= 0)
                                deResponse.WmLoc = dataString;
                            else if (innerStr.IndexOf("WmSrv:") >= 0)
                                deResponse.WmSrv = dataString;
                            else if (innerStr.IndexOf("IsChgItm:") >= 0)
                                deResponse.IsChgItm = dataString;
                            else if (innerStr.IndexOf("WmCmpy:") >= 0)
                                deResponse.WmCmpy = dataString;
                            else if (innerStr.IndexOf("WmWhs:") >= 0)
                                deResponse.WmWhs = dataString;
                            else if (innerStr.IndexOf("WmOrg:") >= 0)
                                deResponse.WmOrg = dataString;
                            else if (innerStr.IndexOf("CrVmpy:") >= 0)
                                deResponse.CrVmpy = dataString;
                            else if (innerStr.IndexOf("CrItm:") >= 0)
                                deResponse.CrItm = dataString;
                            else if (innerStr.IndexOf("WmRefCode:") >= 0)
                                deResponse.WmRefCode = dataString;
                            else if (innerStr.IndexOf("WmSku:") >= 0)
                                deResponse.WmSku = dataString;
                            else if (innerStr.IndexOf("WmMid:") >= 0)
                                deResponse.WmMid = dataString;
                            else if (innerStr.IndexOf("WmMidName:") >= 0)
                                deResponse.WmMidName = dataString;
                            else if (innerStr.IndexOf("WmMidNameH:") >= 0)
                                deResponse.WmMidNameH = dataString;
                            else if (innerStr.IndexOf("WmSkuSpec:") >= 0)
                                deResponse.WmSkuSpec = dataString;
                            else if (innerStr.IndexOf("WmBrand:") >= 0)
                                deResponse.WmBrand = dataString;
                            else if (innerStr.IndexOf("WmMdl:") >= 0)
                                deResponse.WmMdl = dataString;
                            else if (innerStr.IndexOf("WmMidCtg:") >= 0)
                                deResponse.WmMidCtg = dataString;
                            else if (innerStr.IndexOf("WmEffcDate:") >= 0)
                                deResponse.WmEffcDate = dataString;
                            else if (innerStr.IndexOf("WmLot:") >= 0)
                                deResponse.WmLot = dataString;
                            else if (innerStr.IndexOf("WmSeno:") >= 0)
                                deResponse.WmSeno = dataString;
                            else if (innerStr.IndexOf("WmPak:") >= 0)
                                deResponse.WmPak = dataString;
                            else if (innerStr.IndexOf("WmQy:") >= 0)
                                deResponse.WmQy = dataString;
                            else if (innerStr.IndexOf("ThisBarcode:") >= 0)
                                deResponse.ThisBarcode = dataString;
                            else if (innerStr.IndexOf("UdiBarcodes:") >= 0)
                                deResponse.UdiBarcodes = dataString;
                            else if (innerStr.IndexOf("GtinString:") >= 0)
                                deResponse.GtinString = dataString;
                            else if (innerStr.IndexOf("NhiBarcode:") >= 0)
                                deResponse.NhiBarcode = dataString;
                            else if (innerStr.IndexOf("NhiBarcodes:") >= 0)
                                deResponse.NhiBarcodes = dataString;
                            else if (innerStr.IndexOf("BarcodeType:") >= 0)
                                deResponse.BarcodeType = dataString;
                            else if (innerStr.IndexOf("GtinInString:") >= 0)
                                deResponse.GtinInString = dataString;
                            else if (innerStr.IndexOf("Result:") >= 0)
                                deResponse.Result = dataString;
                            else if (innerStr.IndexOf("ErrMsg:") >= 0)
                                deResponse.ErrMsg = dataString;

                        }

                        if (deResponse.Result == "SUCCESS" && string.IsNullOrEmpty(deResponse.WmMid) == false)
                        {
                            var DBWork = session.UnitOfWork;
                            try
                            {
                                var repo = new CC0002Repository(DBWork);
                                if (repo.getUdiMmcodeCnt(deResponse.WmMid, deResponse.WmLot) > 0)
                                    session.Result.afrs = repo.UpdateUdiLog(deResponse);
                                else
                                    session.Result.afrs = repo.CreateUdiLog(deResponse);

                                session.Result.etts = repo.GetUdiLog(deResponse.WmMid, deResponse.WmLot);
                                session.Result.msg = "UDI資料接收完成";
                            }
                            catch
                            {
                                throw;
                            }
                        }


                    }
                }
                catch
                {

                }
                return session.Result;
            }
        }
    }
}