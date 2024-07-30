using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models.AA;
using Newtonsoft.Json;

namespace WebApp.Controllers.AA
{
    public class AA0153Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");
            var p6 = form.Get("p6");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0153Repository(DBWork);
                    AA0153 query = new AA0153();

                    query.MAT_CLASS = p0 == null ? "" : p0.Trim();
                    query.PO_NO = p1 == null ? "" : p1.Trim();
                    query.PO_TIME_S = p3;
                    query.PO_TIME_E = p4;
                    query.AGEN_NO = p5 == null ? "" : p5.Trim();
                    query.MMCODE = p6 == null ? "" : p6.Trim();
                    session.Result.etts = repo.GetAllM(query, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse AllD(FormDataCollection form)
        {
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0153Repository(DBWork);
                    AA0153 query = new AA0153();

                    query.PO_NO = p1 == null ? "" : p1.Trim();
                    query.MMCODE = p2 == null ? "" : p2.Trim();
                    session.Result.etts = repo.GetAllD(query, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse AllAccLogM(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p5 = form.Get("p5");
            var p10 = form.Get("p10");
            var p11 = form.Get("p11");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0153Repository(DBWork);
                    AA0153 query = new AA0153();

                    query.MAT_CLASS = p0 == null ? "" : p0.Trim();
                    query.PO_NO = p1 == null ? "" : p1.Trim();
                    query.AGEN_NO = p5 == null ? "" : p5.Trim();
                    query.PO_TIME_S = p10;
                    query.PO_TIME_E = p11;
                    session.Result.etts = repo.GetAllAccLogM(query, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse AllAccLogD(FormDataCollection form)
        {
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p6 = form.Get("p6");
            var p12 = form.Get("p12");
            var p7 = form.Get("p7");
            var p8 = form.Get("p8");
            var p9 = form.Get("p9");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0153Repository(DBWork);
                    AA0153 query = new AA0153();

                    query.PO_NO = p1 == null ? "" : p1.Trim();
                    query.MMCODE = p2 == null ? "" : p2.Trim();
                    query.ACC_TIME_S = p3;
                    query.ACC_TIME_E = p4;
                    query.STATUS = Convert.ToBoolean(p6) == true ? "Y" : "N"; // 顯示退貨記錄
                    query.STATUS2 = Convert.ToBoolean(p12) == true ? "Y" : "N"; // 顯示已退貨項目
                    query.LOT_NO = p7 == null ? "" : p7.Trim();
                    query.EXP_DATE_S = p8;
                    query.EXP_DATE_E = p9;
                    session.Result.etts = repo.GetAllAccLogD(query, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse SetQty(FormDataCollection form)
        {

            IEnumerable<AA0153> list = JsonConvert.DeserializeObject<IEnumerable<AA0153>>(form.Get("list"));

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0153Repository(DBWork);

                    string check_flag = "Y";
                    string msg = "";
                    string po_no = "";
                    foreach (AA0153 item in list)
                    {
                        po_no = item.PO_NO;

                        item.UPDATE_USER = DBWork.UserInfo.UserId;
                        item.UPDATE_IP = DBWork.ProcIP;
                        // 新增BC_CS_ACC_LOG
                        string accSeq = repo.GetAccSeq();
                        item.SEQ = accSeq;
                        repo.InsertAccLog(item);

                        // 取BC_CS_ACC_LOG的ACC_PO_PRICE, ACC_DISC_CPRICE, ACC_DISC_UPRICE
                        AA0153 cPrice = repo.ChkPrice(item.PO_NO, item.MMCODE);
                        item.PO_PRICE = cPrice.ACC_PO_PRICE;
                        item.DISC_CPRICE = cPrice.ACC_DISC_CPRICE;
                        item.DISC_UPRICE = cPrice.ACC_DISC_UPRICE;
                        // 修改PH_INVOICE
                        repo.UpdateInvoice(item);
                        // 檢查是否已進貨完全，若未完全則新增一筆PH_INVOICE
                        AA0153 cInvoice = repo.ChkInvoice(item.PO_NO, item.MMCODE);
                        if (cInvoice.PO_QTY != cInvoice.DELI_QTY_SUM)
                            repo.InsertInvoice(item);
                        // 更新MM_PO_D
                        repo.UpdateDeliQty(item);
                        // 檢查是否已進貨完全，若已進滿更新MM_PO_D狀態
                        repo.UpdateDeliStatus(item);
                        // 新增PH_LOTNO
                        repo.InsertPhLotno(item);

                        if (item.MAT_CLASS == "01")
                        {
                            // 藥品增加修改MM_PO_INREC
                            item.SEQ = repo.GetInrecSeq(item);
                            repo.UpdateInrec(item);
                            // 若藥品未進貨完全則新增一筆MM_PO_INREC
                            repo.InsertInrec(item);
                        }

                        string poWH_NO = repo.getPO_WHNO(item.PO_NO);
                        if (repo.CheckStlocExists(poWH_NO, item.STORE_LOC) == false)
                        {
                            // 若BC_STLOC未建立儲位標籤資料則預建一筆
                            repo.InsertStloc(poWH_NO, item.STORE_LOC, item.UPDATE_USER, item.UPDATE_IP);
                        }

                        // 若MI_WLOCINV無對應資料則新增, 有則更新
                        if (repo.CheckWlocExists(poWH_NO, item.MMCODE, item.STORE_LOC) == false)
                            repo.InsertWloc(poWH_NO, item.MMCODE, item.STORE_LOC, item.INQTY, item.UPDATE_USER, item.UPDATE_IP);
                        else
                            repo.UpdateWloc(poWH_NO, item.MMCODE, item.STORE_LOC, item.INQTY, item.UPDATE_USER, item.UPDATE_IP);

                        // PO_DOCIN已包含異動MI_WEXPINV庫存量的部分
                        msg = repo.INV_SET_PO_DOCIN(item.PO_NO, User.Identity.Name, DBWork.ProcIP);
                        if (msg.Split('-')[0] == "N")
                        {
                            check_flag = "N";
                            break;
                        }
                    }

                    if (check_flag == "Y")
                    {
                        DBWork.Commit();
                        if (repo.getPO_INVITEM(po_no) == 0)
                            session.Result.msg = "DONE";
                    }
                    else
                    {
                        session.Result.success = false;
                        session.Result.msg = msg;
                        DBWork.Rollback();
                    }
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
        public ApiResponse SetPrice(FormDataCollection form)
        {
            var po_no = form.Get("po_no");
            var mmcode = form.Get("mmcode");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0153Repository(DBWork);

                    string userName = User.Identity.Name;
                    string userIp = DBWork.ProcIP;
                    // 寫入LOG檔
                    session.Result.afrs += repo.InsertPriceLog(po_no, mmcode, userName, userIp);
                    // 更新MM_PO_D
                    session.Result.afrs += repo.UpdatePoPrice(po_no, mmcode, userName, userIp);
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
        public ApiResponse SetTrans(FormDataCollection form)
        {

            IEnumerable<AA0153> list = JsonConvert.DeserializeObject<IEnumerable<AA0153>>(form.Get("list"));

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0153Repository(DBWork);

                    foreach (AA0153 item in list)
                    {
                        item.UPDATE_USER = DBWork.UserInfo.UserId;
                        item.UPDATE_IP = DBWork.ProcIP;
                        // 更新BC_CS_ACC_LOG
                        repo.UpdateAccLog(item);
                        // 修改PH_INVOICE
                        repo.UpdateInvoiceByAccLog(item);
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
        public ApiResponse SetTransBack(FormDataCollection form)
        {
            var po_no = form.Get("PO_NO");
            var mmcode = form.Get("MMCODE");
            var seq = form.Get("SEQ");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0153Repository(DBWork);

                    AA0153 porcData = repo.GetAccLog(po_no, mmcode, seq);

                    porcData.INQTY = porcData.TX_QTY_T;
                    porcData.UPDATE_USER = User.Identity.Name;
                    porcData.UPDATE_IP = DBWork.ProcIP;

                    string check_flag = "Y";
                    string msg = "";

                    if (porcData.MAT_CLASS == "01")
                    {
                        // 藥品增加修改MM_PO_INREC
                        string InrecSEQ = repo.GetInrecSeqForTransBack(porcData);
                        porcData.FROMSEQ = InrecSEQ;
                        repo.InsertInrecByTransBack(porcData);
                        if (repo.CheckStatusNExists(porcData.PO_NO, porcData.MMCODE) == false)
                        {
                            repo.InsertInrec(porcData);
                        }

                        repo.UpdateInrecStatus(porcData);
                    }

                    porcData.INQTY = (Convert.ToInt32(porcData.INQTY) * -1).ToString();
                    // 刪除PH_INVOICE DELI_STATUS = N
                    repo.DelInvoice(porcData.PO_NO, porcData.MMCODE, porcData.SEQ);
                    // 修改PH_INVOICE DELI_STATUS = C
                    repo.UpdateInvoiceByTransBack(porcData);
                    // 更新MM_PO_D狀態為未進滿狀態
                    repo.UpdateDeliStatusByTransBack(porcData);
                    // 更新MM_PO_D(若用AA0176進貨會有POACC_SEQ)
                    repo.UpdateDeliQty(porcData);
                    if (porcData.POACC_SEQ != "" && porcData.POACC_SEQ != null)
                    {
                        // 更新MM_PO_ACC
                        repo.UpdateAccDeliQty(porcData);
                    }
                    // 刪除PH_LOTNO
                    repo.DelPhLotno(porcData);

                    // 更新MI_WLOCINV
                    string pWH_NO = repo.getPO_WHNO(porcData.PO_NO);
                    string pSTORE_LOC = repo.GetStoreLoc(pWH_NO, porcData.MMCODE);
                    repo.UpdateWloc(pWH_NO, porcData.MMCODE, pSTORE_LOC, porcData.INQTY, porcData.UPDATE_USER, porcData.UPDATE_IP);

                    // 新增BC_CS_ACC_LOG
                    string accSeq = repo.GetAccSeq();
                    // 退貨時把額外折讓金額扣回去
                    porcData.EXTRA_DISC_AMOUNT = (Convert.ToInt32(porcData.EXTRA_DISC_AMOUNT) * -1).ToString();
                    porcData.SRC_SEQ = porcData.SEQ;
                    porcData.SEQ = accSeq;
                    repo.InsertAccLog(porcData, true);

                    msg = repo.INV_SET_PO_DOCIN(po_no, User.Identity.Name, DBWork.ProcIP);
                    if (msg.Split('-')[0] == "N")
                    {
                        check_flag = "N";
                    }

                    if (check_flag == "Y")
                    {
                        DBWork.Commit();
                    }
                    else
                    {
                        session.Result.success = false;
                        session.Result.msg = msg;
                        DBWork.Rollback();
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
        public ApiResponse GetMatClassCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0153Repository repo = new AA0153Repository(DBWork);
                    session.Result.etts = repo.GetMatClassCombo(User.Identity.Name);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetAgennoCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0038Repository repo = new AA0038Repository(DBWork);
                    session.Result.etts = repo.GetAgennoCombo(p0, page, limit, "");
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