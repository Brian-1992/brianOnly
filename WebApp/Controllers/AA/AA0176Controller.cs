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
using System.Web;
using NPOI.SS.UserModel;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using System.Text.RegularExpressions;

namespace WebApp.Controllers.AA
{
    public class AA0176Controller : SiteBase.BaseApiController
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
            var p7 = form.Get("p7");
            var p8 = form.Get("p8");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0176Repository(DBWork);
                    AA0176 query = new AA0176();

                    query.MAT_CLASS = p0 == null ? "" : p0.Trim();
                    query.PO_NO = p1 == null ? "" : p1.Trim();
                    query.PO_TIME_S = p3;
                    query.PO_TIME_E = p4;
                    query.AGEN_NO = p5 == null ? "" : p5.Trim();
                    query.MMCODE = p6 == null ? "" : p6.Trim();
                    query.EASYNAME = p7 == null ? "" : p7.Trim();
                    query.isEdit = Convert.ToBoolean(p8) == true ? "Y" : "N";
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
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0176Repository(DBWork);
                    AA0176 query = new AA0176();

                    query.PO_NO = p1 == null ? "" : p1.Trim();
                    query.MMCODE = p2 == null ? "" : p2.Trim();

                    if (repo.CheckPoAccExists(query.PO_NO) == false)
                    {
                        // 若MM_PO_ACC尚無資料則匯入
                        repo.ImportPoAcc(query.PO_NO, DBWork.ProcUser, DBWork.ProcIP);
                    }
                    session.Result.etts = repo.GetAllD(query, page, limit, sorters);
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
                    var repo = new AA0176Repository(DBWork);
                    AA0176 query = new AA0176();

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
                    var repo = new AA0176Repository(DBWork);
                    AA0176 query = new AA0176();

                    query.PO_NO = p1 == null ? "" : p1.Trim();
                    query.MMCODE = p2 == null ? "" : p2.Trim();
                    query.ACC_TIME_S = p3;
                    query.ACC_TIME_E = p4;
                    query.LOT_NO = p7 == null ? "" : p7.Trim();
                    query.EXP_DATE_S = p8;
                    query.EXP_DATE_E = p9;
                    query.STATUS = Convert.ToBoolean(p6) == true ? "Y" : "N"; // 顯示退貨記錄
                    query.STATUS2 = Convert.ToBoolean(p12) == true ? "Y" : "N"; // 顯示已退貨項目
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
        public ApiResponse DetailUpdate(AA0176 aa0176)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0176Repository repo = new AA0176Repository(DBWork);
                    aa0176.UPDATE_USER = User.Identity.Name;
                    aa0176.UPDATE_IP = DBWork.ProcIP;

                    aa0176.EXP_DATE = aa0176.EXP_DATE_RAW;
                    aa0176.INVOICE_DT = aa0176.INVOICE_DT_RAW;

                    if (!string.IsNullOrEmpty(aa0176.INVOICE))
                        aa0176.INVOICE = aa0176.INVOICE.ToUpper();

                    session.Result.afrs = repo.DetailUpdate(aa0176);

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
        public ApiResponse DetailDelete(FormDataCollection form)
        {
            var po_no = form.Get("PO_NO");
            var poacc_seq = form.Get("POACC_SEQ");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0176Repository(DBWork);

                    if (repo.CheckAccCnt(po_no, poacc_seq) == 1)
                    {
                        session.Result.success = false;
                        session.Result.msg = "此院內碼僅剩餘一項,不可刪除";
                        return session.Result;
                    }
                    else if (repo.CheckBcCsAccExists(po_no, poacc_seq) == true)
                    {
                        session.Result.success = false;
                        session.Result.msg = "此項目有已進貨記錄,不可刪除";
                        return session.Result;
                    }

                    repo.DetailDelete(po_no, poacc_seq);

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
        public ApiResponse DetailComplete(FormDataCollection form)
        {
            var po_no = form.Get("PO_NO");
            var poacc_seq = form.Get("POACC_SEQ");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0176Repository(DBWork);
                    AA0176 item = repo.GetPoAcc(po_no, poacc_seq);
                    item.UPDATE_USER = DBWork.UserInfo.UserId;
                    item.UPDATE_IP = DBWork.ProcIP;
                    // 備註標註不再進貨
                    repo.UpdateCompleteMemo(po_no, poacc_seq, item.UPDATE_USER, item.UPDATE_IP);
                    // 不再進貨的品項,DELI_STATUS設為Y
                    repo.DetailComplete(item);

                    DBWork.Commit();

                    if (repo.getPO_INVITEM(po_no) == 0)
                        session.Result.msg = "DONE";
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
        public ApiResponse SetQty(FormDataCollection form)
        {
            string poacc_seq = form.Get("POACC_SEQ").Substring(0, form.Get("POACC_SEQ").Length - 1); // 去除前端傳進來最後一個逗號
            string po_no = form.Get("PO_NO");
            string[] tmp = poacc_seq.Split('^');

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0176Repository(DBWork);

                    string check_flag = "Y";
                    string msg = "";
                    for (int i = 0; i < tmp.Length; i++)
                    {
                        AA0176 item = repo.GetPoAcc(po_no, tmp[i]);
                        item.UPDATE_USER = DBWork.UserInfo.UserId;
                        item.UPDATE_IP = DBWork.ProcIP;

                        // 儲位未填則代預設值
                        if (item.STORE_LOC == "" || item.STORE_LOC == null)
                            item.STORE_LOC = "TMPLOC";
                        // 批號效期未填則代預設值
                        if (item.LOT_NO == "" || item.LOT_NO == null)
                            item.LOT_NO = "TMPLOT";
                        if (item.EXP_DATE == "" || item.EXP_DATE == null)
                            item.EXP_DATE = "9991231";
                        if (item.EXTRA_DISC_AMOUNT == "" || item.EXTRA_DISC_AMOUNT == null)
                            item.EXTRA_DISC_AMOUNT = "0";

                        if (repo.ChkInvoiceDup(item.INVOICE, item.INVOICE_DT) == "Y")
                        {
                            session.Result.success = false;
                            session.Result.msg = item.INVOICE + "發票號碼已存在且日期(" + item.INVOICE_DT + ")不同,請檢查!";
                            DBWork.Rollback();
                            return session.Result;
                        }

                        // 新增BC_CS_ACC_LOG
                        string accSeq = repo.GetAccSeq();
                        item.SEQ = accSeq;
                        repo.InsertAccLog(item);
                        // 修改PH_INVOICE
                        repo.UpdateInvoice(item);
                        // 檢查是否已進貨完全，若未完全則新增一筆PH_INVOICE
                        AA0176 cInvoice = repo.ChkInvoice(item.PO_NO, item.MMCODE);
                        if (cInvoice.PO_QTY != cInvoice.DELI_QTY_SUM)
                            repo.InsertInvoice(item);

                        // 更新MM_PO_D
                        repo.UpdateDeliQty(item);
                        // 更新MM_PO_ACC
                        repo.UpdateAccDeliQty(item);
                        repo.UpdateAccAccQty(item);

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
                    }
                    // PO_DOCIN已包含異動MI_WEXPINV庫存量的部分
                    msg = repo.INV_SET_PO_DOCIN(po_no, User.Identity.Name, DBWork.ProcIP);
                    if (msg.Split('-')[0] == "N")
                    {
                        check_flag = "N";
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
                    var repo = new AA0176Repository(DBWork);

                    string userName = User.Identity.Name;
                    string userIp = DBWork.ProcIP;
                    // 寫入LOG檔
                    session.Result.afrs += repo.InsertPriceLog(po_no, mmcode, userName, userIp);
                    // 更新MM_PO_D
                    session.Result.afrs += repo.UpdatePoPrice(po_no, mmcode, userName, userIp);
                    // 更新MM_PO_ACC
                    string new_disc_cprice = repo.getDiscCprice(po_no, mmcode);
                    session.Result.afrs += repo.UpdateAccPoPrice(po_no, mmcode, new_disc_cprice, userName, userIp);
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
        public ApiResponse SetTrans(FormDataCollection form)
        {

            IEnumerable<AA0176> list = JsonConvert.DeserializeObject<IEnumerable<AA0176>>(form.Get("list"));

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0176Repository(DBWork);

                    foreach (AA0176 item in list)
                    {
                        item.UPDATE_USER = DBWork.UserInfo.UserId;
                        item.UPDATE_IP = DBWork.ProcIP;

                        // 要加重複判斷還需要考慮同時勾選多筆...
                        //if (repo.ChkInvoiceDup(item.INVOICE, item.INVOICE_DT, item.SEQ) == "Y")
                        //{
                        //    session.Result.success = false;
                        //    session.Result.msg = item.INVOICE + "發票號碼已存在且日期(" + item.INVOICE_DT + ")不同,請檢查!";
                        //    DBWork.Rollback();
                        //    return session.Result;
                        //}

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
                    var repo = new AA0176Repository(DBWork);

                    AA0176 porcData = repo.GetAccLog(po_no, mmcode, seq);

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
                    // 更新MM_PO_D
                    repo.UpdateDeliQty(porcData);
                    // 更新MM_PO_ACC
                    repo.UpdateAccDeliQty(porcData);
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
        public ApiResponse SplitMmcode(FormDataCollection form)
        {
            // 依傳入的拆分數, 將指定的院內碼項目拆項
            var po_no = form.Get("p0");
            var poacc_seq = form.Get("p1");
            var split_qty = form.Get("p2");
            var memo = form.Get("p3");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0176Repository(DBWork);

                    string userName = User.Identity.Name;
                    string userIp = DBWork.ProcIP;
                    for(int i = 1; i < Convert.ToInt32(split_qty); i++)
                    {
                        // 寫入MM_PO_ACC
                        session.Result.afrs += repo.InsertPoAcc(po_no, poacc_seq, split_qty, memo, userName, userIp);
                    }
                    // 更新MM_PO_ACC原本那筆
                    session.Result.afrs += repo.UpdatePoAcc(po_no, poacc_seq, split_qty, userName, userIp);
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
        public ApiResponse ImportInvoice(FormDataCollection form)
        {
            string po_no = form.Get("P0");
            string poacc_seq = form.Get("P1").Substring(0, form.Get("P1").Length - 1); // 去除前端傳進來最後一個逗號
            string[] tmp = poacc_seq.Split('^');
            string lot_no = form.Get("P2");
            string exp_date = form.Get("P3");
            string invoice = form.Get("P4");
            string invoice_dt = form.Get("P5");
            string extra_disc_amount = form.Get("P6");
            string memo = form.Get("P7");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0176Repository(DBWork);
                    for (int i = 0; i < tmp.Length; i++)
                    {
                        // 更新指定項目有填寫的欄位
                        repo.UpdateAccInvoice(po_no, tmp[i], lot_no, exp_date, invoice, invoice_dt, extra_disc_amount, memo, User.Identity.Name, DBWork.ProcIP);
                        // 重算小計
                        repo.RecaltAccAmt(po_no, tmp[i]);
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
        public ApiResponse GetMatClassCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0176Repository repo = new AA0176Repository(DBWork);
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

        //匯出EXCEL
        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0176Repository repo = new AA0176Repository(DBWork);
                    JCLib.Excel.Export("AA0176.xls", repo.GetExcel());
                }
                catch
                {
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

                List<AA0176> list = new List<AA0176>();
                UnitOfWork DBWork = session.UnitOfWork;
                var HttpPostedFile = HttpContext.Current.Request.Files["file"];
                IWorkbook workBook;

                try
                {
                    AA0176Repository repo = new AA0176Repository(DBWork);
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
                    int i, j;


                    bool isValid = true;
                    string[] arr = { "訂單號碼", "院內碼", "儲位", "本次進貨量", "批號", "效期", "發票號碼", "發票日期", "備註" };

                    for (j = 0; j < cellCount; j++)
                    {
                        isValid = headerRow.GetCell(j) == null ? false : true;
                        if (!isValid)
                        {
                            session.Result.msg = "檔案格式不同，請下載範本來更新。";
                            break;
                        }
                    }

                    //檢查檔案中欄位名稱是否符合
                    if (isValid)
                    {
                        for (i = 0; i < cellCount; i++)
                        {
                            isValid = headerRow.GetCell(i).ToString() == arr[i] ? true : false;
                            if (!isValid)
                            {
                                break;
                            }
                        }
                    }

                    if (!isValid)
                    {
                        session.Result.msg = "檔案格式不同，請下載範本來更新。";
                    }
                    #endregion


                    DataTable dtTable = new DataTable();
                    DataRow datarow = dtTable.NewRow();
                    string arrCheckResult = "";
                    int nullnum = 0; //判斷是否整列為空

                    if (isValid)
                    {
                        #region 建立DataTable
                        for (i = 0; i < cellCount; i++)
                        //以欄位文字為名新增欄位，此處全視為字串型別以求簡化
                        {
                            dtTable.Columns.Add(
                                  new DataColumn(headerRow.GetCell(i).StringCellValue));
                        }
                        dtTable.Columns.Add("檢核結果");

                        //略過第0列(標題列)，一直處理至最後一列
                        //for (i = 1; i <= sheet.LastRowNum; i++)
                        //略過第0列(說明列)和第1列(標題列)，一直處理至最後一列
                        for (i = 1; i <= sheet.LastRowNum; i++)
                        {
                            IRow row = sheet.GetRow(i);
                            datarow = dtTable.NewRow();
                            arrCheckResult = "OK";
                            nullnum = 0;
                            //依先前取得的欄位數逐一設定欄位內容
                            for (j = 0; j < cellCount; j++)
                            {
                                if (row == null)
                                {
                                    nullnum = cellCount;
                                    break;
                                }
                                datarow[j] = row.GetCell(j) == null ? "" : row.GetCell(j).ToString();
                            }
                            if (nullnum != cellCount)
                            {
                                datarow[cellCount] = arrCheckResult;
                                dtTable.Rows.Add(datarow);
                            }
                        }

                        dtTable.DefaultView.Sort = "訂單號碼";
                        dtTable = dtTable.DefaultView.ToTable();
                        #endregion

                        DataTable newTable = dtTable.Clone();
                        newTable = dtTable;

                        //加入至MM_PO_ACC中
                        #region 加入至MM_PO_ACC中
                        for (i = 0; i < newTable.Rows.Count; i++)
                        {
                            AA0176 aa0176 = new AA0176();
                            aa0176.PO_NO = newTable.Rows[i]["訂單號碼"].ToString().Trim();
                            aa0176.MMCODE = newTable.Rows[i]["院內碼"].ToString().Trim();
                            aa0176.STORE_LOC = newTable.Rows[i]["儲位"].ToString().Trim();
                            aa0176.INQTY = newTable.Rows[i]["本次進貨量"].ToString().Trim();
                            aa0176.LOT_NO = newTable.Rows[i]["批號"].ToString().Trim();
                            aa0176.EXP_DATE = newTable.Rows[i]["效期"].ToString().Trim();
                            aa0176.INVOICE = newTable.Rows[i]["發票號碼"].ToString().Trim();
                            aa0176.INVOICE_DT = newTable.Rows[i]["發票日期"].ToString().Trim();
                            aa0176.MEMO = newTable.Rows[i]["備註"].ToString().Trim();
                            aa0176.CHECK_RESULT = "OK";

                            if (newTable.Rows[i]["訂單號碼"].ToString().Trim() != "" 
                                && newTable.Rows[i]["院內碼"].ToString().Trim() != "")
                            {
                                if (repo.CheckExistsMMCODE(aa0176.MMCODE) != true)
                                {
                                    aa0176.CHECK_RESULT = "此院內碼不存在";
                                }
                                else if (repo.CheckExistsPo(aa0176.PO_NO) != true)
                                {
                                    aa0176.CHECK_RESULT = "此訂單號碼不存在";
                                }
                                else if (repo.CheckPoExistsMMCODE(aa0176.PO_NO, aa0176.MMCODE) != true)
                                {
                                    aa0176.CHECK_RESULT = "訂單不存在此院內碼";
                                }
                                else if (aa0176.EXP_DATE.Trim() != "" && aa0176.EXP_DATE.Length != 7 && IsChtDate(aa0176.EXP_DATE) == false)
                                {
                                    aa0176.CHECK_RESULT = "效期格式不正確";
                                }
                                else if (aa0176.INVOICE_DT.Trim() != "" && aa0176.INVOICE_DT.Length != 7 && IsChtDate(aa0176.INVOICE_DT) == false)
                                {
                                    aa0176.CHECK_RESULT = "發票日期格式不正確";
                                }
                                else
                                {

                                }
                                if (aa0176.CHECK_RESULT != "OK")
                                {
                                    checkPassed = false;
                                }
                                //產生一筆資料
                                list.Add(aa0176);
                            }
                        }
                        #endregion
                    }

                    if (list.Count() == 0)
                    {
                        isValid = false;
                        session.Result.msg = "檔案內沒有可匯入的資料。";
                    }
                        
                    if (!isValid)
                    {
                        session.Result.success = false;
                    }
                    else
                    {
                        session.Result.etts = list;
                        session.Result.msg = checkPassed.ToString();
                    }
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //確認更新
        [HttpPost]
        public ApiResponse Insert(FormDataCollection formData)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();

                IEnumerable<AA0176> aa0176 = JsonConvert.DeserializeObject<IEnumerable<AA0176>>(formData["data"]);

                List<AA0176> aa0176_list = new List<AA0176>();
                try
                {
                    var repo = new AA0176Repository(DBWork);
                    foreach (AA0176 data in aa0176)
                    {
                        try
                        {
                            if (!repo.CheckExistsD(data.PO_NO)) // 新增前檢查主鍵是否已存在
                            {
                                data.SEQ = "1";
                            }
                            else
                            {
                                data.SEQ = repo.GetPoAccSeq(data.PO_NO);
                            }
                            data.UPDATE_USER = User.Identity.Name;
                            data.UPDATE_IP = DBWork.ProcIP;
                            session.Result.afrs = repo.ImportPoAccByExcel(data);
                        }
                        catch
                        {
                            throw;
                        }
                        aa0176_list.Add(data);
                    }

                    session.Result.etts = aa0176_list;

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
        public ApiResponse calcAmtMsg(FormDataCollection form)
        {
            var po_no = form.Get("PO_NO");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0176Repository(DBWork);
                    DataTable dt = repo.calcAmtMsg(po_no);
                    string vSUM_PO_PRICE = $"{Convert.ToInt32(dt.Rows[0]["SUM_PO_PRICE"].ToString()):n0}"; // 處理千位的逗號
                    string vSUM_IN_DISC_CPRICE = $"{Convert.ToInt32(dt.Rows[0]["SUM_IN_DISC_CPRICE"].ToString()):n0}";
                    string vSUM_INV_PO_PRICE = $"{Convert.ToInt32(dt.Rows[0]["SUM_INV_PO_PRICE"].ToString()):n0}";
                    string vSUM_INV_DISC_CPRICE = $"{Convert.ToInt32(dt.Rows[0]["SUM_INV_DISC_CPRICE_2"].ToString()):n0}";
                    string vSUM_DISC = $"{Convert.ToInt32(dt.Rows[0]["SUM_DISC"].ToString()):n0}";
                    session.Result.success = true;
                    session.Result.msg = "訂單合約合計:" + vSUM_PO_PRICE
                        + " 　本次進貨小計總和:" + vSUM_IN_DISC_CPRICE
                        + " 　已進合約價合計:" + vSUM_INV_PO_PRICE
                        + " 　已進優惠價合計:" + vSUM_INV_DISC_CPRICE
                        + " 　已進折讓合計:" + vSUM_DISC;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public bool IsChtDate(String strNumber)
        {
            Regex NumberPattern = new Regex("^[1-9][0-9][0-9](([0][1-9])|([1][0-2]))(([0-2][0-9])|([3][0-1]))$");
            return NumberPattern.IsMatch(strNumber);
        }

        [HttpPost]
        public ApiResponse GetLoginInfo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0176Repository repo = new AA0176Repository(DBWork);
                    var v_userid = User.Identity.Name;
                    var v_ip = DBWork.ProcIP;
                    session.Result.etts = repo.GetLoginInfo(v_userid, v_ip);
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