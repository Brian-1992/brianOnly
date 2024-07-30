using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System;

namespace WebApp.Controllers.AA
{
    public class AA0147Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p5 = form.Get("p5");
            //  var p6 = form.Get("p6");
            var p7 = form.Get("p7"); // 院內碼
            var p8 = form.Get("p8"); // 單據號碼
            var p9 = form.Get("p9");//可提撥
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            string[] arr_p3 = { };
            if (!string.IsNullOrEmpty(p3))
            {
                arr_p3 = p3.Trim().Split(','); //用,分割
            }
            string[] arr_p5 = { };
            if (!string.IsNullOrEmpty(p5))
            {
                arr_p5 = p5.Trim().Split(','); //用,分割
            }
            string tuser = User.Identity.Name;
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0147Repository(DBWork);
                    session.Result.etts = repo.GetAllM(p0, p1, p2, arr_p3, arr_p5, p7, p8, p9, tuser, page, limit, sorters);
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
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0147Repository(DBWork);
                    session.Result.etts = repo.GetAllD(p0, p1, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Apply(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0147Repository repo = new AA0147Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        IEnumerable<ME_DOCD> data_list = JsonConvert.DeserializeObject<IEnumerable<ME_DOCD>>(form.Get("DATA_LIST"));
                        string[] tmp = docno.Split(',');
                        string msg = string.Empty;
                        string rtn = string.Empty;
                        string v_docno = string.Empty;
                        string v_doctype = string.Empty;
                        string v_matclass = string.Empty;
                        string v_doclist = string.Empty;
                        string v_prno = string.Empty;
                        List<ME_DOCD> ExpqtyLargerC = new List<ME_DOCD>();  // 預計核撥量大於民庫存量
                        List<string> DocmStatusError = new List<string>();    // 此申請單狀態非核撥中
                        List<string> EmptyDocd = new List<string>();  // 沒有院內碼項次
                        List<ME_DOCD> ApvqtyError = new List<ME_DOCD>(); //核撥量<=0

                        for (int i = 0; i < tmp.Length; i++)
                        {
                            if (repo.CheckExistsM2M4(tmp[i]))
                            {
                                DocmStatusError.Add(tmp[i]);
                                continue;
                            }

                            IEnumerable<ME_DOCD> docds = repo.GetAllDocds(tmp[i]);

                            // 沒有院內碼項次
                            if (docds.Any() == false)
                            {
                                EmptyDocd.Add(tmp[i]);
                                continue;
                            }

                            foreach (ME_DOCD docd in docds)
                            {

                                // 調撥量 = 0 & 預計撥發量 = 0 => 不須判斷
                                if (docd.BW_MQTY == "0" && docd.EXPT_DISTQTY == "0")
                                {
                                    continue;
                                }
                                if (float.Parse(docd.APVQTY) <= 0)
                                {
                                    ApvqtyError.Add(docd);
                                }
                                // 預計撥發量 >0 => 比對民庫量
                                if (float.Parse(docd.EXPT_DISTQTY) > 0)
                                {
                                    if (repo.CheckExptQtyMmcode(docd.DOCNO, docd.MMCODE))
                                    {
                                        ExpqtyLargerC.Add(docd);
                                    }
                                }
                            }
                        }
                        // 若list中有東西，串成文字
                        if (EmptyDocd.Any())
                        {
                            string tempString = string.Empty;
                            if (msg == string.Empty)
                            {
                                msg += "請檢查下列項目：";
                            }
                            msg += "<br/><span style='color:red'>● 沒有院內碼項次之申請單</span>：<br/>";
                            foreach (string temp in EmptyDocd)
                            {
                                if (tempString.Length > 0)
                                {
                                    tempString += "、";
                                }
                                tempString += temp;
                            }
                            msg += tempString;
                        }
                        if (DocmStatusError.Any())
                        {
                            string tempString = string.Empty;
                            if (msg == string.Empty)
                            {
                                msg += "請檢查下列項目：";
                            }
                            msg += "<br/><span style='color:red'>● 申請單狀態非 核撥中 或 揀料中</span>：<br/>";
                            foreach (string temp in DocmStatusError)
                            {
                                if (tempString.Length > 0)
                                {
                                    tempString += "、";
                                }
                                tempString += temp;
                            }
                            msg += tempString;
                        }

                        if (ExpqtyLargerC.Any())
                        {
                            string tempString = string.Empty;
                            if (msg == string.Empty)
                            {
                                msg += "請檢查下列項目：";
                            }
                            msg += "<br/><span style='color:red'>● 核撥量大於庫房庫存量</span>：<br/>";
                            int i = 1;
                            foreach (ME_DOCD temp in ExpqtyLargerC)
                            {
                                if (tempString.Length > 0)
                                {
                                    tempString += "<br>";
                                }
                                tempString += string.Format("{0}. 申請單號:{1} 院內碼:{2}", i, temp.DOCNO, temp.MMCODE);
                                i++;
                            }
                            msg += tempString;
                        }

                        if (ApvqtyError.Any())
                        {
                            string tempString = string.Empty;
                            if (msg == string.Empty)
                            {
                                msg += "請檢查下列項目：";
                            }
                            msg += "<br/><span style='color:red'>● 核撥量不得小於等於0<</span>：<br/>";
                            int i = 1;
                            foreach (ME_DOCD temp in ApvqtyError)
                            {
                                if (tempString.Length > 0)
                                {
                                    tempString += "<br>";
                                }
                                tempString += string.Format("{0}. 申請單號:{1} 院內碼:{2}", i, temp.DOCNO, temp.MMCODE);
                                i++;
                            }
                            msg += tempString;
                        }

                        // 若msg不為空，表示有錯誤
                        if (msg != string.Empty)
                        {
                            session.Result.success = false;
                            session.Result.msg = msg;
                            return session.Result;
                        }
                        msg = "";
                        // 所有申請單通過檢核，進行更新
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            ME_DOCM me_docm = new ME_DOCM();
                            me_docm.DOCNO = tmp[i];
                            me_docm.UPDATE_USER = User.Identity.Name;
                            me_docm.UPDATE_IP = DBWork.ProcIP;
                            me_docm.DOCTYPE = repo.GetDoctype(tmp[i]);

                            //C.更新ME_DOCD 未設定
                            repo.ApplyDN(me_docm);
                            //若全部品項皆轉申購就不需執行SP會error，直接更新ME_DOCM狀態
                            if (repo.ChkNotIsTransPr(me_docm.DOCNO) == 0)
                            {
                                //更新ME_DOCM狀態
                                repo.ApplyM(me_docm);
                            }
                            else
                            {
                                //D.執行SP：POST_DOC (為進行核撥)
                                rtn = repo.CallProc(tmp[i], User.Identity.Name, DBWork.ProcIP);
                                if (rtn != "Y")
                                {
                                    DBWork.Rollback();
                                    session.Result.success = false;
                                    session.Result.msg = "<br/><span style='color:red'>申請單號「" + tmp[i] + "」</span>,發生執行錯誤。<br/>" + rtn;
                                    return session.Result;
                                }
                                // 點收量預帶核撥量
                                repo.defAckqty(tmp[i]);
                                //C -> 4 待點收
                                repo.ApplyDMRMS(me_docm);

                                //F.再次執行SP：POST_DOC (為進行點收)
                                rtn = repo.CallProc(tmp[i], User.Identity.Name, DBWork.ProcIP);
                                if (rtn != "Y")
                                {
                                    DBWork.Rollback();
                                    session.Result.success = false;
                                    session.Result.msg = "<br/><span style='color:red'>申請單號「" + tmp[i] + "」</span>，發生執行錯誤。<br/>" + rtn;
                                    return session.Result;
                                }
                            }
                        }
                        // 更新批號效期及儲位的庫存量
                        updateExpLocInv(data_list, DBWork);
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
        public ApiResponse GetFlowidCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0147Repository repo = new AA0147Repository(DBWork);
                    var id = User.Identity.Name;
                    session.Result.etts = repo.GetFlowidCombo(id);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetApplyKindCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0147Repository repo = new AA0147Repository(DBWork);
                    var id = User.Identity.Name;
                    session.Result.etts = repo.GetApplyKindCombo(id);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetAppDeptCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0147Repository repo = new AA0147Repository(DBWork);
                    var id = User.Identity.Name;
                    session.Result.etts = repo.GetAppDeptCombo(id);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetMMCodeDocd(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0147Repository repo = new AA0147Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeDocd(p0, p1, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetTowhCombo()
        {
            var p0 = User.Identity.Name;
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0147Repository repo = new AA0147Repository(DBWork);
                    session.Result.etts = repo.GetTowhCombo(p0);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMatclassCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0147Repository repo = new AA0147Repository(DBWork);
                    var id = User.Identity.Name;
                    session.Result.etts = repo.GetMatclassCombo(id);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // 共用功能:檢查單號是否有批號效期數量(有:回傳Y,無:回傳N)
        [HttpPost]
        public ApiResponse ChkExp(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0147Repository repo = new AA0147Repository(DBWork);

                    string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                    string[] tmpDoc = docno.Split(',');
                    if (repo.ChkHasExp(tmpDoc) > 0)
                        session.Result.msg = "Y";
                    else
                        session.Result.msg = "N";
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // 共用功能:依傳入單號查詢批號效期數量,並依先進先出規則預先填入數量
        [HttpPost]
        public ApiResponse GetExpList(FormDataCollection form)
        {
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0147Repository repo = new AA0147Repository(DBWork);

                    string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                    string chkColType = Convert.ToString(form.Get("CHKCOLTYPE")); // 依傳入的值指定用哪個欄位當作申請量
                    string chkCol = "";
                    if (chkColType == "1")
                        chkCol = "APVQTY"; // 核撥(撥發), 報廢, 調撥
                    else if (chkColType == "2")
                        chkCol = "APPQTY"; // 繳回

                    string[] tmpDoc = docno.Split(',');
                    // 依先進先出規則填入數量: 查詢資料時依效期排序, 然後依序填入數量
                    IEnumerable<ME_DOCD> rtnData = repo.GetExpList(tmpDoc, chkCol, page, limit, sorters);

                    session.Result.etts = getFIFOData(rtnData);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public IEnumerable<ME_DOCD> getFIFOData(IEnumerable<ME_DOCD> rtnData)
        {
            string tmpDocno = "";
            string tmpMmcode = "";
            int remainApvqty = 0;
            foreach (ME_DOCD item in rtnData)
            {
                // 處理到另一張申請單了, 或第一張申請單
                if (item.DOCNO != tmpDocno)
                {
                    tmpDocno = item.DOCNO;
                    tmpMmcode = "";
                    remainApvqty = 0;
                }

                // 院內碼不一樣了, 或申請單的第一筆
                if (item.MMCODE != tmpMmcode)
                {
                    tmpMmcode = item.MMCODE;
                    remainApvqty = Convert.ToInt32(item.APVQTY);
                }

                // 若處理的這筆數量大於庫存量,則最多只能使用等於庫存量的部分, 餘量留給下一筆處理
                if (remainApvqty > Convert.ToInt32(item.INV_QTY))
                {
                    item.APPQTY = item.INV_QTY;
                    remainApvqty = remainApvqty - Convert.ToInt32(item.INV_QTY);
                }
                else
                {
                    // 若要處理的量小於庫存量, 則可以直接使用處理量
                    item.APPQTY = remainApvqty.ToString();
                    remainApvqty = 0;
                }
            }

            return rtnData;
        }

        // 共用功能:依傳入資料逐筆調整出入庫房的批號效期及儲位庫存量
        public void updateExpLocInv(IEnumerable<ME_DOCD> data_list, UnitOfWork DBWork)
        {
            if (data_list.Count() > 0)
            {
                AA0147Repository repo = new AA0147Repository(DBWork);
                foreach (ME_DOCD item in data_list)
                {
                    item.UPDATE_USER = DBWork.UserInfo.UserId;
                    item.UPDATE_IP = DBWork.ProcIP;
                    // 出庫庫房-扣除
                    repo.UpdateWloc(item.FRWH, item.MMCODE, item.STORE_LOC, item.APPQTY, "-", item.UPDATE_USER, item.UPDATE_IP);
                    repo.UpdateWexp(item.FRWH, item.MMCODE, item.LOT_NO, item.EXPDATE, item.APPQTY, "-", item.UPDATE_USER, item.UPDATE_IP);

                    // 報廢等流程可能不會有入庫庫房
                    if (item.TOWH != null && item.TOWH != "" && item.FRWH != item.TOWH)
                    {
                        // 入庫庫房-增加
                        // 若MI_WLOCINV無對應資料則新增, 有則更新
                        if (repo.CheckWlocExists(item.TOWH, item.MMCODE, item.STORE_LOC) == false)
                            repo.InsertWloc(item.TOWH, item.MMCODE, item.STORE_LOC, item.APPQTY, item.UPDATE_USER, item.UPDATE_IP);
                        else
                            repo.UpdateWloc(item.TOWH, item.MMCODE, item.STORE_LOC, item.APPQTY, "+", item.UPDATE_USER, item.UPDATE_IP);

                        // 若MI_WEXPINV無對應資料則新增, 有則更新
                        if (repo.CheckWexpExists(item.TOWH, item.MMCODE, item.LOT_NO, item.EXPDATE) == false)
                            repo.InsertWexp(item.TOWH, item.MMCODE, item.LOT_NO, item.EXPDATE, item.APPQTY, item.UPDATE_USER, item.UPDATE_IP);
                        else
                            repo.UpdateWexp(item.TOWH, item.MMCODE, item.LOT_NO, item.EXPDATE, item.APPQTY, "+", item.UPDATE_USER, item.UPDATE_IP);
                    }
                }
            }
        }

        [HttpPost]
        public ApiResponse GetMMCodeCombo(FormDataCollection form)
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
                    AA0147Repository repo = new AA0147Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeCombo(p0, page, limit, "");
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse UpdatePostidBySP(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0147Repository repo = new AA0147Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string seq = form.Get("SEQ").Substring(0, form.Get("SEQ").Length - 1);
                        IEnumerable<ME_DOCD> data_list = JsonConvert.DeserializeObject<IEnumerable<ME_DOCD>>(form.Get("DATA_LIST"));
                        string[] tmp = docno.Split(',');
                        string[] tmp_seq = seq.Split(',');
                        string msg = string.Empty;
                        string rtn = string.Empty;
                        List<string> DocdPostIdError = new List<string>(); //postid已核可
                        List<ME_DOCD> ApvqtyError = new List<ME_DOCD>(); //核撥量<=0
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            IEnumerable<ME_DOCD> docds = repo.GetDocds(tmp[i], tmp_seq[i]);
                            foreach (ME_DOCD docd in docds)
                            {
                                if (repo.CheckWhidValid(tmp[i], DBWork.UserInfo.UserId) == false)
                                {
                                    string tempString = string.Empty;
                                    if (msg == string.Empty)
                                    {
                                        msg += "請檢查下列項目：院內碼" + docd.MMCODE;
                                    }
                                    msg += "<br/><span style='color:red'>● 無此庫房權限，請重新確認</span><br/>";
                                }
                                //檢查申請單狀態
                                if (repo.CheckExistsM2M4(docd.DOCNO))
                                {
                                    string tempString = string.Empty;
                                    if (msg == string.Empty)
                                    {
                                        msg += "請檢查下列項目：院內碼" + docd.MMCODE;
                                    }
                                    msg += "<br/><span style='color:red'>● 申請單狀態非 核撥中 或 揀料中</span><br/>";
                                }
                                //檢查明細apvqty
                                if (float.Parse(docd.APVQTY) <= 0)
                                {
                                    string tempString = string.Empty;
                                    if (msg == string.Empty)
                                    {
                                        msg += "請檢查下列項目：院內碼" + docd.MMCODE;
                                    }
                                    msg += "<br/><span style='color:red'>● 核撥量不得小於等於0</span><br/>";
                                }
                                //檢查明細postid
                                if (repo.ChkPostIdNotA(tmp[i], tmp_seq[i]) == true)
                                {
                                    string tempString = string.Empty;
                                    if (msg == string.Empty)
                                    {
                                        msg += "請檢查下列項目：院內碼" + docd.MMCODE;
                                    }
                                    msg += "<br/><span style='color:red'>●申請明細院內碼過帳狀態非已核可</span><br/>";
                                }

                                // 若msg不為空，表示有錯誤
                                if (msg != string.Empty)
                                {
                                    session.Result.success = false;
                                    session.Result.msg = msg;
                                    return session.Result;
                                }
                                ME_DOCD me_docd = new ME_DOCD();
                                me_docd.DOCNO = docd.DOCNO;
                                me_docd.SEQ = docd.SEQ;
                                me_docd.UPDATE_USER = User.Identity.Name;
                                me_docd.UPDATE_IP = DBWork.ProcIP;
                                //批次撥發時如只有一個品項扣帳SP判斷無未撥發品項會將flowid=0199故需先則拆單
                                if (int.Parse(docd.APPQTY) - int.Parse(docd.APVQTY) > 0)
                                {
                                    repo.InsertMEDOCD(me_docd);
                                    me_docd.APPQTY = docd.APVQTY;
                                    repo.UpdateAppqty(me_docd);
                                }
                                //A->3
                                me_docd.POSTID = "3";   // 待核撥
                                repo.UpdatePostid(me_docd);

                                // 3 -> C 已核撥
                                rtn = repo.CallProc(docd.DOCNO, User.Identity.Name, DBWork.ProcIP);
                                if (rtn != "Y")
                                {
                                    session.Result.afrs = 0;
                                    session.Result.success = false;
                                    session.Result.msg = "<br/><span style='color:red'>申請單號「" + docd.DOCNO + "」</span>,發生執行錯誤。<br/>" + rtn;
                                    return session.Result;
                                }

                                // 點收量預帶核撥量
                                repo.defAckqty(docd.DOCNO, docd.SEQ);

                                //C -> 4 待點收
                                me_docd.POSTID = "4";   // 待點收
                                repo.UpdatePostid(me_docd);

                                // 4-> D 已點收(已核撥)
                                rtn = repo.CallProc(docd.DOCNO, User.Identity.Name, DBWork.ProcIP);
                                if (rtn != "Y")
                                {
                                    session.Result.afrs = 0;
                                    session.Result.success = false;
                                    session.Result.msg = "<br/><span style='color:red'>申請單號「" + docd.DOCNO + "」</span>,發生執行錯誤。<br/>" + rtn;
                                    return session.Result;
                                }
                            }
                        }
                        // 更新批號效期及儲位的庫存量
                        if (data_list.Count() > 0)
                        {
                            updateExpLocInv(data_list, DBWork);
                        }
                    }
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        public ApiResponse ChkExpDetail(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0147Repository repo = new AA0147Repository(DBWork);
                    if (form.Get("DOCNO") != "" && form.Get("SEQ") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string seq = form.Get("SEQ").Substring(0, form.Get("SEQ").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmpDocno = docno.Split(',');
                        string[] tmpSeq = seq.Split(',');
                        string rtnMsg = "";
                        for (int i = 0; i < tmpSeq.Length; i++)
                        {
                            //檢查明細有一筆批號即停止
                            if (repo.ChkHasExpDetail(tmpDocno[i], tmpSeq[i]) > 0)
                            {
                                rtnMsg = "Y";
                                break;
                            }
                            else
                                rtnMsg = "N";
                        }
                        session.Result.afrs = 0;
                        session.Result.success = true;
                        session.Result.msg = rtnMsg;
                        return session.Result;
                    }
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetExpListDetail(FormDataCollection form)
        {
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0147Repository repo = new AA0147Repository(DBWork);

                    string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                    string seq = form.Get("SEQ").Substring(0, form.Get("SEQ").Length - 1); // 去除前端傳進來最後一個逗號
                    string chkColType = Convert.ToString(form.Get("CHKCOLTYPE")); // 依傳入的值指定用哪個欄位當作申請量
                    string chkCol = "";
                    if (chkColType == "1")
                        chkCol = "APVQTY"; // 核撥(撥發), 報廢, 調撥
                    else if (chkColType == "2")
                        chkCol = "APPQTY"; // 繳回

                    string[] tmpDoc = docno.Split(',');
                    string[] tmpSeq = seq.Split(',');
                    // 依先進先出規則填入數量: 查詢資料時依效期排序, 然後依序填入數量
                    IEnumerable<ME_DOCD> rtnData = repo.GetExpListDetail(tmpDoc, tmpSeq, chkCol, page, limit, sorters);

                    session.Result.etts = getFIFOData(rtnData);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse UpdateMeDocd(ME_DOCD me_docd)
        {
            IEnumerable<ME_DOCD> me_docds = JsonConvert.DeserializeObject<IEnumerable<ME_DOCD>>(me_docd.ITEM_STRING);
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0147Repository repo = new AA0147Repository(DBWork);
                    string msg = string.Empty;
                    foreach (ME_DOCD docd in me_docds)
                    {
                        if (repo.CheckExistsM2M4(docd.DOCNO))
                        {
                            if (msg == string.Empty)
                            {
                                msg += "請檢查下列項目：";
                            }
                            msg += "<br/><span style='color:red'>申請單狀態非 核撥中 或 揀料中</span>不得更新。";
                        }
                        if (float.Parse(docd.APVQTY) <= 0)
                        {
                            if (msg == string.Empty)
                            {
                                msg += "請檢查下列項目：";
                            }
                            msg += "<br/><span style='color:red'>核撥量不得小於等於0</span>";
                        }
                        if (float.Parse(docd.APVQTY) > float.Parse(docd.APPQTY))
                        {
                            if (msg == string.Empty)
                            {
                                msg += "請檢查下列項目：";
                            }
                            msg += "<br/><span style='color:red'>核撥量不得超過申請量</span>";
                        }

                        if (float.Parse(docd.APVQTY) > float.Parse(docd.S_INV_QTY))
                        {
                            if (msg == string.Empty)
                            {
                                msg += "請檢查下列項目：";
                            }
                            msg += "<br/><span style='color:red'>核撥量不得超過上級庫庫房存量</span>";
                        }
                        if (repo.ChkPostIdNotA(docd.DOCNO, docd.SEQ) == true)
                        {
                            if (msg == string.Empty)
                            {
                                msg += "請檢查下列項目：";
                            }
                            msg += "<br/><span style='color:red'>過帳狀態非已核可</span>";
                        }
                        // 若msg不為空，表示有錯誤
                        if (msg != string.Empty)
                        {
                            session.Result.success = false;
                            session.Result.msg = msg;
                            return session.Result;
                        }
                        else
                        {
                            docd.UPDATE_USER = User.Identity.Name;
                            docd.UPDATE_IP = DBWork.ProcIP;
                            session.Result.afrs = repo.UpdateMeDocd(docd);
                        }
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
        public ApiResponse ChkApvQtyDetail(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0147Repository repo = new AA0147Repository(DBWork);
                    if (form.Get("DOCNO") != "" && form.Get("SEQ") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string seq = form.Get("SEQ").Substring(0, form.Get("SEQ").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmpDocno = docno.Split(',');
                        string[] tmpSeq = seq.Split(',');
                        string rtnMsg = "";
                        for (int i = 0; i < tmpSeq.Length; i++)
                        {
                            //檢查明細是否有核撥量<=0
                            if (repo.ChkApvQtyDetail(tmpDocno[i], tmpSeq[i]) > 0)
                            {
                                rtnMsg = "Y";
                                break;
                            }
                            else
                                rtnMsg = "N";
                        }
                        session.Result.afrs = 0;
                        session.Result.success = true;
                        session.Result.msg = rtnMsg;
                        return session.Result;
                    }
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetLoginInfo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0147Repository repo = new AA0147Repository(DBWork);
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
        [HttpPost]
        public ApiResponse ApplyInv(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0147Repository repo = new AA0147Repository(DBWork);

                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        IEnumerable<ME_DOCD> data_list = JsonConvert.DeserializeObject<IEnumerable<ME_DOCD>>(form.Get("DATA_LIST"));
                        string[] tmp = docno.Split(',');
                        string msg = string.Empty;
                        string rtn = string.Empty;
                        List<string> EmptyDocd = new List<string>();  // 沒有院內碼項次
                        List<string> DocmStatusError = new List<string>();    // 此申請單狀態非核撥中

                        for (int i = 0; i < tmp.Length; i++)
                        {
                            if (repo.CheckExistsM2M4(tmp[i]))
                            {
                                DocmStatusError.Add(tmp[i]);
                                continue;
                            }

                            //取得庫存足夠可核撥品項
                            IEnumerable<ME_DOCD> docds = repo.GetDocdsInv(tmp[i]);
                            // 沒有院內碼項次
                            if (docds.Any() == false)
                            {
                                EmptyDocd.Add(tmp[i]);
                                continue;
                            }
                        }
                        // 若list中有東西，串成文字
                        if (DocmStatusError.Any())
                        {
                            string tempString = string.Empty;
                            if (msg == string.Empty)
                            {
                                msg += "請檢查下列項目：";
                            }
                            msg += "<br/><span style='color:red'>● 申請單狀態非 核撥中 或 揀料中</span>：<br/>";
                            foreach (string temp in DocmStatusError)
                            {
                                if (tempString.Length > 0)
                                {
                                    tempString += "、";
                                }
                                tempString += temp;
                            }
                            msg += tempString;
                        }

                        if (EmptyDocd.Any())
                        {
                            string tempString = string.Empty;
                            if (msg == string.Empty)
                            {
                                msg += "請檢查下列項目：";
                            }
                            msg += "<br/><span style='color:red'>● 核撥量=0 或 沒有庫存足夠可核撥品項之申請單</span>：<br/>";
                            foreach (string temp in EmptyDocd)
                            {
                                if (tempString.Length > 0)
                                {
                                    tempString += "、";
                                }
                                tempString += temp;
                            }
                            msg += tempString;
                        }
                        // 若msg不為空，表示有錯誤
                        if (msg != string.Empty)
                        {
                            session.Result.success = false;
                            session.Result.msg = msg;
                            return session.Result;
                        }
                        msg = "";
                        // 所有申請單通過檢核，進行更新
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            //取得庫存足夠可核撥品項
                            IEnumerable<ME_DOCD> docds = repo.GetDocdsInv(tmp[i]);
                            foreach (ME_DOCD docd in docds)
                            {
                                ME_DOCD me_docd = new ME_DOCD();
                                me_docd.DOCNO = docd.DOCNO;
                                me_docd.SEQ = docd.SEQ;
                                me_docd.UPDATE_USER = User.Identity.Name;
                                me_docd.UPDATE_IP = DBWork.ProcIP;

                                //A->3
                                me_docd.POSTID = "3";   // 待核撥
                                repo.UpdatePostid(me_docd);

                                // 3 -> C 已核撥
                                rtn = repo.CallProc(docd.DOCNO, User.Identity.Name, DBWork.ProcIP);
                                if (rtn != "Y")
                                {
                                    session.Result.afrs = 0;
                                    session.Result.success = false;
                                    session.Result.msg = "<br/><span style='color:red'>申請單號「" + docd.DOCNO + "」</span>,發生執行錯誤。<br/>" + rtn;
                                    return session.Result;
                                }

                                // 點收量預帶核撥量
                                repo.defAckqty(docd.DOCNO, docd.SEQ);

                                //C -> 4 待點收
                                me_docd.POSTID = "4";   // 待點收
                                repo.UpdatePostid(me_docd);

                                // 4-> D 已點收(已核撥)
                                rtn = repo.CallProc(docd.DOCNO, User.Identity.Name, DBWork.ProcIP);
                                if (rtn != "Y")
                                {
                                    session.Result.afrs = 0;
                                    session.Result.success = false;
                                    session.Result.msg = "<br/><span style='color:red'>申請單號「" + docd.DOCNO + "」</span>,發生執行錯誤。<br/>" + rtn;
                                    return session.Result;
                                }
                            }
                        }
                        // 更新批號效期及儲位的庫存量
                        if (data_list.Count() > 0)
                        {
                            updateExpLocInv(data_list, DBWork);
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
    }
}