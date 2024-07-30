using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.C;
using WebApp.Models;
using System;

namespace WebApp.Controllers.C
{
    public class CC0003Controller : SiteBase.BaseApiController
    {

        // 查詢T1
        [HttpPost]
        public ApiResponse All_1(FormDataCollection form)
        {

            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p2_1 = form.Get("p2_1");
            var p3 = form.Get("p3");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CC0003Repository(DBWork);
                    //var ret_code = repo.MM_PO_INREC_CHK(p0, p2.ToString(), User.Identity.Name, DBWork.ProcIP);
                    session.Result.etts = repo.GetAll_1(p0, p1, p2, p2_1, p3, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // 查詢T2
        [HttpPost]
        public ApiResponse All_2(FormDataCollection form)
        {

            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = int.Parse(form.Get("p2").Substring(0, 10).Replace("-", "")) - 19110000;
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CC0003Repository(DBWork);
                    //var ret_code = repo.MM_PO_INREC_CHK(p0, p2.ToString(), User.Identity.Name, DBWork.ProcIP);
                    session.Result.etts = repo.GetAll_2(p0, p1, p2.ToString(),page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //物料分類combox
        public ApiResponse GetWH_NO()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CC0003Repository(DBWork);
                    session.Result.etts = repo.GetWH_NO();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //廠商代碼combox
        public ApiResponse GetAGEN_NO()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CC0003Repository(DBWork);
                    session.Result.etts = repo.GetAGEN_NO();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //儲存
        [HttpPost]
        public ApiResponse Commit(FormDataCollection form)
        {
            bool isTempSave = form.Get("isTempSave") == "Y";
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CC0003Repository(DBWork);
                    string accountdate = form.Get("ACCOUNTDATE").Substring(0, form.Get("ACCOUNTDATE").Length - 1); // 去除前端傳進來最後一個逗號
                    string deli_qty = form.Get("DELI_QTY").Substring(0, form.Get("DELI_QTY").Length - 1); // 去除前端傳進來最後一個逗號
                    string memo = form.Get("MEMO").Substring(0, form.Get("MEMO").Length - 1); // 去除前端傳進來最後一個逗號
                    string lot_no = form.Get("LOT_NO").Substring(0, form.Get("LOT_NO").Length - 1); // 去除前端傳進來最後一個逗號
                    string exp_date = form.Get("EXP_DATE").Substring(0, form.Get("EXP_DATE").Length - 1); // 去除前端傳進來最後一個逗號
                    string po_amt = form.Get("PO_AMT").Substring(0, form.Get("PO_AMT").Length - 1); // 去除前端傳進來最後一個逗號
                    string po_qty = form.Get("PO_QTY").Substring(0, form.Get("PO_QTY").Length - 1); // 去除前端傳進來最後一個逗號
                    string po_no = form.Get("PO_NO").Substring(0, form.Get("PO_NO").Length - 1); // 去除前端傳進來最後一個逗號
                    string mmcode = form.Get("MMCODE").Substring(0, form.Get("MMCODE").Length - 1); // 去除前端傳進來最後一個逗號
                    string seq = form.Get("SEQ").Substring(0, form.Get("SEQ").Length - 1); // 去除前端傳進來最後一個逗號
                    string[] TMP_accountdate = accountdate.Split(',');
                    string[] TMP_deli_qty = deli_qty.Split(',');
                    string[] TMP_memo = memo.Split(',');
                    string[] TMP_lot_no = lot_no.Split(',');
                    string[] TMP_exp_date = exp_date.Split(',');
                    string[] TMP_po_amt = po_amt.Split(',');
                    string[] TMP_po_qty = po_qty.Split(',');
                    string[] TMP_po_no = po_no.Split(',');
                    string[] TMP_mmcode = mmcode.Split(',');
                    string[] TMP_seq = seq.Split(',');

                    for (int i = 0; i < TMP_accountdate.Length; i++)
                    {
                        session.Result.afrs = repo.Commit(TMP_accountdate[i], TMP_deli_qty[i], TMP_memo[i], TMP_lot_no[i], TMP_exp_date[i],
                                                          TMP_po_amt[i], DBWork.ProcIP, User.Identity.Name, TMP_po_qty[i], TMP_po_no[i], TMP_mmcode[i], TMP_seq[i], isTempSave);
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

        //新增
        [HttpPost]
        public ApiResponse Insert(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CC0003Repository(DBWork);
                    string PURDATE = form.Get("PURDATE");
                    string PO_NO = form.Get("PO_NO");
                    string MMCODE = form.Get("MMCODE");
                    string E_PURTYPE = form.Get("E_PURTYPE");
                    string CONTRACNO = form.Get("CONTRACNO");
                    string AGEN_NO = form.Get("AGEN_NO");
                    string PO_QTY = form.Get("PO_QTY");
                    string PO_PRICE = form.Get("PO_PRICE");
                    string STATUS = form.Get("STATUS");
                    string M_PURUN = form.Get("M_PURUN");
                    string PO_AMT = form.Get("PO_AMT");
                    string WH_NO = form.Get("WH_NO");
                    string M_DISCPERC = form.Get("M_DISCPERC");
                    string UNIT_SWAP = form.Get("UNIT_SWAP");
                    string UPRICE = form.Get("UPRICE");
                    string DISC_CPRICE = form.Get("DISC_CPRICE");
                    string DISC_UPRICE = form.Get("DISC_UPRICE");
                    string TRANSKIND = form.Get("TRANSKIND");
                    string IFLAG = form.Get("IFLAG");
                    string CREATE_USER = User.Identity.Name;
                    string UPDATE_USER = User.Identity.Name;
                    string UPDATE_IP = DBWork.ProcIP;

                    if (repo.CheckStatusNExists(PO_NO, MMCODE)) {
                        session.Result.success = false;
                        session.Result.msg = "已存在未進貨資料，無法新增";
                        return session.Result;
                    }

                    string ISWILLING = string.Empty;
                    string DISCOUNT_QTY = string.Empty;
                    string DISC_COST_UPRICE = string.Empty;
                    MILMED_JBID_LIST jbid = repo.GetMILMED_JBID_LIST(MMCODE);
                    if (jbid != null) {
                        ISWILLING = jbid.ISWILLING;
                        DISCOUNT_QTY = jbid.DISCOUNT_QTY;
                        DISC_COST_UPRICE = jbid.DISC_COST_UPRICE;
                    }

                    string SEQ = repo.GetSEQ();
                    if (E_PURTYPE.Equals('甲')) 
                        E_PURTYPE = "1";
                    else
                        E_PURTYPE = "2";
                    session.Result.afrs = repo.Insert(SEQ, PURDATE, PO_NO, MMCODE, E_PURTYPE, CONTRACNO, AGEN_NO, PO_QTY, PO_PRICE, STATUS, M_PURUN,
                                                          PO_AMT, WH_NO, M_DISCPERC, UNIT_SWAP, UPRICE, DISC_CPRICE,
                                                          DISC_UPRICE, CREATE_USER, UPDATE_USER, UPDATE_IP, TRANSKIND, IFLAG,
                                                          ISWILLING, DISCOUNT_QTY, DISC_COST_UPRICE);

                    DBWork.Commit();
                    session.Result.msg = SEQ;
                    //return int.Parse(SEQ);
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }

        //退貨
        [HttpPost]
        public ApiResponse Back(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CC0003Repository(DBWork);
                    string po_no = form.Get("PO_NO").Substring(0, form.Get("PO_NO").Length - 1); // 去除前端傳進來最後一個逗號
                    string mmcode = form.Get("MMCODE").Substring(0, form.Get("MMCODE").Length - 1); // 去除前端傳進來最後一個逗號
                    string seq = form.Get("SEQ").Substring(0, form.Get("SEQ").Length - 1); // 去除前端傳進來最後一個逗號
                    string[] TMP_po_no = po_no.Split(',');
                    string[] TMP_mmcode = mmcode.Split(',');
                    string[] TMP_seq = seq.Split(',');
                    string check_flag = "Y";
                    var msg = "";
                    for (int i = 0; i < TMP_po_no.Length; i++)
                    {
                        session.Result.afrs = repo.Back_1(TMP_po_no[i], TMP_mmcode[i], TMP_seq[i], User.Identity.Name, DBWork.ProcIP);
                        if (repo.CheckStatusNExists(TMP_po_no[i], TMP_mmcode[i]) == false) {
                            session.Result.afrs = repo.Back_2(TMP_po_no[i], TMP_mmcode[i], TMP_seq[i], User.Identity.Name, DBWork.ProcIP);
                        }
                        session.Result.afrs = repo.Back_3(TMP_seq[i], User.Identity.Name);
                        session.Result.afrs = repo.Back_4(TMP_po_no[i], TMP_mmcode[i], TMP_seq[i], User.Identity.Name, DBWork.ProcIP);
                        msg = repo.INV_SET_PO_DOCIN(TMP_po_no[i], User.Identity.Name, DBWork.ProcIP);
                        if (msg.Split('-')[0] == "N")
                        {
                            check_flag = "N";
                            break;
                        }
                    }
                    if (check_flag == "Y")
                    {
                        DBWork.Commit();
                    }
                    else
                    {
                        session.Result.success = false;
                        session.Result.msg = msg;
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

        //T1進貨
        [HttpPost]
        public ApiResponse T1in_pur(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CC0003Repository(DBWork);
                    string PO_NO = form.Get("PO_NO").Substring(0, form.Get("PO_NO").Length - 1); // 去除前端傳進來最後一個逗號
                    string SEQ = form.Get("SEQ").Substring(0, form.Get("SEQ").Length - 1); // 去除前端傳進來最後一個逗號
                    string MMCODE = form.Get("MMCODE").Substring(0, form.Get("MMCODE").Length - 1); // 去除前端傳進來最後一個逗號
                    string[] TMP_PO_NO = PO_NO.Split(',');
                    string[] TMP_SEQ = SEQ.Split(',');
                    string[] TMP_MMCODE = MMCODE.Split(',');
                    string check_flag = "Y";
                    var msg = "";
                    var ret_code = "";
                    for (var i = 0; i < TMP_PO_NO.Length; i++)
                    {
                        if (repo.ChkDELI_QTY(TMP_SEQ[i]) =="Y")
                        {
                            session.Result.afrs = repo.T1in_pur_1(TMP_SEQ[i], User.Identity.Name);
                            session.Result.afrs = repo.T1in_pur_2(TMP_PO_NO[i], TMP_MMCODE[i], TMP_SEQ[i]);
                            ret_code = repo.MM_PO_INREC_CHK_SUBMIT(TMP_PO_NO[i], TMP_MMCODE[i], TMP_SEQ[i], User.Identity.Name, DBWork.ProcIP);
                            msg = repo.INV_SET_PO_DOCIN(TMP_PO_NO[i], User.Identity.Name, DBWork.ProcIP);
                            if (msg.Split('-')[0] == "N")
                            {
                                check_flag = "N";
                                break;
                            }
                        }
                        else
                        {
                            session.Result.msg = TMP_MMCODE[i]+"-->[進貨量]累計超過[訂單量],請檢查!";
                            break;
                        }
                    }
                    if (check_flag == "Y")
                    {
                        DBWork.Commit();
                    }
                    else
                    {
                        session.Result.success = false;
                        session.Result.msg = msg;
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
        
        //T2進貨
        [HttpPost]
        public ApiResponse T2in_pur(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CC0003Repository(DBWork);
                    string PO_NO = form.Get("PO_NO").Substring(0, form.Get("PO_NO").Length - 1); // 去除前端傳進來最後一個逗號
                    string SEQ = form.Get("SEQ").Substring(0, form.Get("SEQ").Length - 1); // 去除前端傳進來最後一個逗號
                    string MMCODE = form.Get("MMCODE").Substring(0, form.Get("MMCODE").Length - 1); // 去除前端傳進來最後一個逗號
                    string[] TMP_PO_NO = PO_NO.Split(',');
                    string[] TMP_SEQ = SEQ.Split(',');
                    string[] TMP_MMCODE = MMCODE.Split(',');
                    string check_flag = "Y";
                    var msg = "";
                    var ret_code = "";
                    for (var i = 0; i < TMP_PO_NO.Length; i++)
                    {
                        session.Result.afrs = repo.T2in_pur_1(TMP_PO_NO[i], TMP_MMCODE[i], TMP_SEQ[i]);
                        session.Result.afrs = repo.T2in_pur_2(TMP_SEQ[i], User.Identity.Name);
                        ret_code = repo.MM_PO_INREC_CHK_SUBMIT(TMP_PO_NO[i], TMP_MMCODE[i], TMP_SEQ[i], User.Identity.Name, DBWork.ProcIP);
                        msg = repo.INV_SET_PO_DOCIN(TMP_PO_NO[i], User.Identity.Name, DBWork.ProcIP);
                        if (msg.Split('-')[0] == "N")
                        {
                            check_flag = "N";
                            break;
                        }
                    }
                    if (check_flag == "Y")
                    {
                        DBWork.Commit();
                    }
                    else
                    {
                        session.Result.success = false;
                        session.Result.msg = msg;
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
        public ApiResponse Excel(FormDataCollection form)
        {
            var WH_NO = form.Get("p0");
            var AGEN_NO = form.Get("p1");
            var PURDATE = form.Get("p2"); //PURDATE=yyymmdd
            var PURDATE_1 = form.Get("p2_1"); //PURDATE=yyymmdd
            var KIND = form.Get("p3");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CC0003Repository repo = new CC0003Repository(DBWork);
                    JCLib.Excel.Export("CC0003_藥品進貨日報表" + "_" + DateTime.Now.ToString("yyyyMMddhhmm") + ".xls", repo.GetExcel(WH_NO, AGEN_NO, PURDATE, PURDATE_1, KIND));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        #region 2022-01-22 新增 訂單進貨資料 頁面：查詢MM_PO_INREC訂單進貨資料檔 所有資料
        public ApiResponse All_3(FormDataCollection form) {
            var p0 = form.Get("p0");    // wh_no
            var p1 = form.Get("p1");    // agen_no
            var p2 = form.Get("p2");    //purdate
            var p2_1 = form.Get("p2_1");    //purdate_1
            var p3 = form.Get("p3");    // kind
            var p4 = form.Get("p4");    // mmcode

            using (WorkSession session = new WorkSession(this)) {

                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CC0003Repository(DBWork);
                    session.Result.etts = repo.GetAll_3(p0, p1, p2, p2_1, p3, p4);
                }
                catch (Exception e) {
                    throw;
                }

                return session.Result;
            }
        }
        #endregion
    }
}