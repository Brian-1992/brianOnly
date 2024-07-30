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
using NPOI.XSSF.UserModel;
using System.Data;
using NPOI.SS.UserModel;
using System.IO;
using System.Web;

namespace WebApp.Controllers.AA
{
    public class AA0172Controller : SiteBase.BaseApiController
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
            var p7 = form.Get("p7"); // 院內碼
            var p8 = form.Get("p8"); // 單據號碼
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
                    var repo = new AA0172Repository(DBWork);
                    session.Result.etts = repo.GetAllM(p0, p1, p2, arr_p3, arr_p5, p7, p8, tuser, page, limit, sorters);
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
                    var repo = new AA0172Repository(DBWork);
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
        public ApiResponse UpdateMeDocd(ME_DOCD me_docd)
        {
            IEnumerable<ME_DOCD> me_docds = JsonConvert.DeserializeObject<IEnumerable<ME_DOCD>>(me_docd.ITEM_STRING);
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0172Repository repo = new AA0172Repository(DBWork);
                    var v_userid = User.Identity.Name;
                    var v_ip = DBWork.ProcIP;
                    IEnumerable<AB0003Model> LoginInfo = repo.GetLoginInfo(v_userid, v_ip);
                    var hosp_code = LoginInfo.FirstOrDefault().HOSP_CODE;

                    string msg = string.Empty;
                    foreach (ME_DOCD docd in me_docds)
                    {
                        if (repo.CheckExistsM11M2(docd.DOCNO))
                        {
                            if (msg == string.Empty)
                            {
                                msg += "請檢查下列項目：";
                            }
                            msg += "<br/><span style='color:red'>申請單狀態非 未核可 或 核撥中</span>不得更新。";
                        }
                        /*if (float.Parse(docd.APVQTY) <=0)
                        {
                            if (msg == string.Empty)
                            {
                                msg += "請檢查下列項目：";
                            }
                            msg += "<br/><span style='color:red'>核撥量不得小於等於0</span>";
                        }*/
                        if (string.IsNullOrEmpty(docd.APPQTY))
                        {
                            docd.APPQTY = "0";
                        }
                        if (string.IsNullOrEmpty(docd.PR_QTY))
                        {
                            docd.PR_QTY = "0";
                        }
                        if (string.IsNullOrEmpty(docd.APVQTY))
                        {
                            docd.APVQTY = "0";
                        }
                        if (float.Parse(docd.APVQTY) > float.Parse(docd.APPQTY))
                        {
                            if (msg == string.Empty)
                            {
                                msg += "請檢查下列項目：";
                            }
                            msg += "<br/><span style='color:red'>核撥量不得超過申請量</span>";
                        }
                        if (hosp_code != "807")
                        {
                            if (float.Parse(docd.PR_QTY) > float.Parse(docd.APPQTY))
                            {
                                if (msg == string.Empty)
                                {
                                    msg += "請檢查下列項目：";
                                }
                                msg += "<br/><span style='color:red'>申購量不得超過申請量</span>";
                            }
                            if ((float.Parse(docd.APVQTY) + float.Parse(docd.PR_QTY)) > float.Parse(docd.APPQTY))
                            {
                                if (msg == string.Empty)
                                {
                                    msg += "請檢查下列項目：";
                                }
                                msg += "<br/><span style='color:red'>核撥量+申購量不得超過申請量</span>";
                            }
                        }
                        if ((docd.POSTID != "待核可") && ((docd.APVQTY!=docd.APVQTY_O) || (docd.PR_QTY!=docd.PRQTY_O)))
                        {
                            if (msg == string.Empty)
                            {
                                msg += "請檢查下列項目：";
                            }
                            msg += "<br/><span style='color:red'>過帳狀態非待核可，不可修改核撥量、申購量</span>";
                        }
                        //1121013國軍現行藥衛材系統核撥量超過庫房存量也能核可，比照辦理不卡關
                        /* if (float.Parse(docd.APVQTY) > float.Parse(docd.INV_QTY))
                         {
                             if (msg == string.Empty)
                             {
                                 msg += "請檢查下列項目：";
                             }
                             msg += "<br/><span style='color:red'>核撥量不得超過庫房存量</span>";
                         }*/
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

        [HttpPost]
        public ApiResponse UpdateMeDocdPrY(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0172Repository repo = new AA0172Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string seq = form.Get("SEQ").Substring(0, form.Get("SEQ").Length - 1);
                        string[] tmp = docno.Split(',');
                        string[] tmp_seq = seq.Split(',');
                        string msg = string.Empty;
                        List<string> DocmStatusError = new List<string>();    // 此申請單狀態非核撥中
                        List<string> DocmWhgradeError = new List<string>();   // 非一級庫
                        List<ME_DOCD> DocdOrderdcError = new List<ME_DOCD>(); // 全院停用
                        List<ME_DOCD> DocdParcdeError = new List<ME_DOCD>();  // 子藥
                        List<ME_DOCD> DocdIstransprError = new List<ME_DOCD>();  // 轉申購但沒有對應供應商
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            IEnumerable<ME_DOCD> docds = repo.GetDocds(tmp[i], tmp_seq[i]);

                            foreach (ME_DOCD docd in docds)
                            {
                                if (repo.CheckExistsM11(docd.DOCNO))
                                {
                                    DocmStatusError.Add(docd.DOCNO);
                                    continue;
                                }
                                if (repo.CheckExistsWhgrade1(docd.DOCNO))
                                {
                                    DocmWhgradeError.Add(docd.DOCNO);
                                    continue;
                                }
                                if (repo.CheckExistsOrderdc(docd.DOCNO, docd.SEQ))
                                {
                                    DocdOrderdcError.Add(docd);
                                    continue;
                                }
                                if (repo.CheckExistsParcde(docd.DOCNO, docd.SEQ))
                                {
                                    DocdParcdeError.Add(docd);
                                    continue;
                                }
                                if (repo.ChkIstransprExistsVender(docd.DOCNO, docd.SEQ) == false)
                                {
                                    DocdIstransprError.Add(docd);
                                    continue;
                                }
                            }

                            if (DocmStatusError.Any())
                            {
                                string tempString = string.Empty;
                                if (msg == string.Empty)
                                {
                                    msg += "請檢查下列項目：";
                                }
                                msg += "<br/><span style='color:red'>● 申請單狀態非未核可</span>：<br/>";
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
                            if (DocmWhgradeError.Any())
                            {
                                string tempString = string.Empty;
                                if (msg == string.Empty)
                                {
                                    msg += "請檢查下列項目：";
                                }
                                msg += "<br/><span style='color:red'>● 核撥庫房非為一級庫</span>：<br/>";
                                foreach (string temp in DocmWhgradeError)
                                {
                                    if (tempString.Length > 0)
                                    {
                                        tempString += "、";
                                    }
                                    tempString += temp;
                                }
                                msg += tempString;
                            }
                            if (DocdOrderdcError.Any())
                            {
                                string tempString = string.Empty;
                                if (msg == string.Empty)
                                {
                                    msg += "請檢查下列項目：";
                                }
                                msg += "<br/><span style='color:red'>● 院內碼全院停用</span>：<br/>";
                                int j = 1;
                                foreach (ME_DOCD temp in DocdOrderdcError)
                                {
                                    if (tempString.Length > 0)
                                    {
                                        tempString += "<br>";
                                    }
                                    tempString += string.Format("{0}. 申請單號:{1} 院內碼:{2}", j, temp.DOCNO, temp.MMCODE);
                                    j++;
                                }
                                msg += tempString;
                            }
                            if (DocdParcdeError.Any())
                            {
                                string tempString = string.Empty;
                                if (msg == string.Empty)
                                {
                                    msg += "請檢查下列項目：";
                                }
                                msg += "<br/><span style='color:red'>● 院內碼為子藥</span>：<br/>";
                                int k = 1;
                                foreach (ME_DOCD temp in DocdParcdeError)
                                {
                                    if (tempString.Length > 0)
                                    {
                                        tempString += "<br>";
                                    }
                                    tempString += string.Format("{0}. 申請單號:{1} 院內碼:{2}", k, temp.DOCNO, temp.MMCODE);
                                    k++;
                                }
                                msg += tempString;
                            }
                            if (DocdIstransprError.Any())
                            {
                                string tempString = string.Empty;
                                if (msg == string.Empty)
                                {
                                    msg += "請檢查下列項目：";
                                }
                                msg += "<br/><span style='color:red'>● 院內碼設為轉申購但無對應可用廠商</span>：<br/>";
                                int k = 1;
                                foreach (ME_DOCD temp in DocdIstransprError)
                                {
                                    if (tempString.Length > 0)
                                    {
                                        tempString += "<br>";
                                    }
                                    tempString += string.Format("{0}. 申請單號:{1} 院內碼:{2}", k, temp.DOCNO, temp.MMCODE);
                                    k++;
                                }
                                msg += tempString;
                            }
                            // 若msg不為空，表示有錯誤
                            if (msg != string.Empty)
                            {
                                DBWork.Rollback();
                                session.Result.success = false;
                                session.Result.msg = msg;
                                return session.Result;
                            }
                            // 所有申請單通過檢核，進行更新
                            foreach (ME_DOCD docd in docds)
                            {
                                session.Result.afrs = repo.UpdateDTransPr(docd.DOCNO, docd.SEQ, "Y");
                            }
                        }
                        DBWork.Commit();
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
        public ApiResponse UpdateMeDocdPrN(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0172Repository repo = new AA0172Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string seq = form.Get("SEQ").Substring(0, form.Get("SEQ").Length - 1);
                        string[] tmp = docno.Split(',');
                        string[] tmp_seq = seq.Split(',');

                        for (int i = 0; i < tmp.Length; i++)
                        {
                            IEnumerable<ME_DOCD> docds = repo.GetDocds(tmp[i], tmp_seq[i]);

                            foreach (ME_DOCD docd in docds)
                            {
                                if (repo.CheckExistsM11(docd.DOCNO))
                                {
                                    session.Result.afrs = 0;
                                    session.Result.success = false;
                                    session.Result.msg = "<span style='color:red'>此申請單狀態非未核可</span>不得取消設定轉申購。";
                                }
                                else
                                {
                                    session.Result.afrs = repo.UpdateDTransPr(docd.DOCNO, docd.SEQ, "N");
                                }
                            }
                        }
                        DBWork.Commit();
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
        public ApiResponse ChkIsTransPr(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0172Repository repo = new AA0172Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        string rtnMsg = "";
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            if (repo.ChkIsTransPr(tmp[i]) > 0)
                            {
                                rtnMsg = tmp[i];
                                break;
                            }
                            else
                                rtnMsg = "Y";
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
        public ApiResponse Apply(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0172Repository repo = new AA0172Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string autotranspr = form.Get("autoTransPr"); //系統自動產生訂單及新單據
                        string[] tmp = docno.Split(',');
                        string msg = string.Empty;
                        string rtn = string.Empty;
                        string v_docno = string.Empty;
                        string v_doctype = string.Empty;
                        string v_matclass = string.Empty;
                        string v_doclist = string.Empty;
                        string v_prno = string.Empty;
                        List<ME_DOCD> ExpqtyLargerC = new List<ME_DOCD>();  // 預計核撥量大於民庫存量
                        List<ME_DOCD> DocdIstransprError = new List<ME_DOCD>();  // 轉申購但沒有對應供應商
                        List<string> DocmStatusError = new List<string>();    // 檢查申請單狀態
                        List<string> EmptyDocd = new List<string>();  // 沒有院內碼項次
                        List<string> v_docno_list = new List<string>(); //轉申購流程
                        List<string> v_matclass_list = new List<string>(); //轉申購流程

                        for (int i = 0; i < tmp.Length; i++)
                        {
                            //檢查申請單狀態
                            if (repo.CheckExistsM11M2(tmp[i]))
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
                                if (autotranspr == "Y")
                                {
                                    if (repo.ChkIstransprExistsVender(docd.DOCNO, docd.SEQ) == false)
                                    {
                                        DocdIstransprError.Add(docd);
                                    }
                                    continue;
                                }

                                // 調撥量 = 0 & 預計撥發量 = 0 => 不須判斷
                                if (docd.BW_MQTY == "0" && docd.EXPT_DISTQTY == "0")
                                {
                                    continue;
                                }
                                // 預計核撥量 > 0 => 比對民庫量
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
                            msg += "<br/><span style='color:red'>● 申請單狀態非 未核可 或 核撥中</span>：<br/>";
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
                        if (DocdIstransprError.Any())
                        {
                            string tempString = string.Empty;
                            if (msg == string.Empty)
                            {
                                msg += "請檢查下列項目：";
                            }
                            msg += "<br/><span style='color:red'>● 院內碼設為轉申購但無對應可用廠商</span>：<br/>";
                            int k = 1;
                            foreach (ME_DOCD temp in DocdIstransprError)
                            {
                                if (tempString.Length > 0)
                                {
                                    tempString += "<br>";
                                }
                                tempString += string.Format("{0}. 申請單號:{1} 院內碼:{2}", k, temp.DOCNO, temp.MMCODE);
                                k++;
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
                            me_docm.TOWH = repo.GetTowh(tmp[i]);
                            //C.更新ME_DOCD
                            repo.ApplyD(me_docm);

                            if (autotranspr == "Y")
                            {
                                //D.轉新申請單
                                if (repo.CheckExistsPr(tmp[i])) //a.
                                {
                                    //a.取得申請單號
                                    v_docno = repo.GetDailyDocno();
                                    //v_docno = (me_docm.DOCTYPE == "MR" || me_docm.DOCTYPE == "MS")
                                    //            ? repo.GetMRMSDocno()
                                    //            : repo.GetIstransprDocno(tmp[i]);
                                    //b.檢查申請單號是否已存在於ME_DOCM，若存在，等一秒後再執行a，直到v_docno不存在於ME_DOCM
                                    while (repo.CheckExistsM(v_docno) == true)
                                    {
                                        System.Threading.Thread.Sleep(1000);
                                        v_docno = repo.GetDailyDocno();
                                        //v_docno = (me_docm.DOCTYPE == "MR" || me_docm.DOCTYPE == "MS")
                                        //            ? repo.GetMRMSDocno()
                                        //            : repo.GetIstransprDocno(tmp[i]);
                                    }
                                    v_docno_list.Add(v_docno);
                                    me_docm.ITEM_STRING = v_docno;
                                    //c.新增ME_DOCM
                                    repo.InsertMEDOCM(me_docm);
                                    //d.新增ME_DOCD
                                    repo.InsertMEDOCD(me_docm);
                                    //e.更新原始ME_DOCD欠撥原因
                                    repo.UpdateDReason(tmp[i], v_docno);
                                }
                            }
                            // E.更新ME_DOCM狀態
                            repo.ApplyM(me_docm);
                        }
                        if (autotranspr == "Y")
                        {
                            //H.取得v_docno_list之物料類別清單 v_matclass
                            if (v_docno_list.Any())
                            {
                                string tempString = string.Empty;
                                foreach (string temp in v_docno_list)
                                {
                                    if (tempString.Length > 0)
                                    {
                                        tempString += ",";
                                    }
                                    tempString += string.Format("{0}", temp);
                                }
                                v_doclist += tempString;
                            }
                            string[] tmpdocno = v_doclist.Split(',');
                            IEnumerable<MM_PR_M> mm_pr_m_matclass = repo.GetMatClassList(tmpdocno);
                            foreach (MM_PR_M mmprm in mm_pr_m_matclass)
                            {
                                mmprm.UPDATE_USER = User.Identity.Name;
                                mmprm.UPDATE_IP = DBWork.ProcIP;
                                //I.以H取得結果(即v_matclass)進行判斷處理
                                if (mmprm.MAT_CLASS != "01")
                                {
                                    //a.取得申購單號
                                    v_prno = repo.GetPrno(mmprm.MAT_CLASS);
                                    //b.檢查申購單號是否已存在於MM_PR_M，若存在，等一秒後再執行a，直到v_prno不存在於MM_PR_M
                                    while (repo.CheckExistsMMPRM(v_prno) == true)
                                    {
                                        System.Threading.Thread.Sleep(1000);
                                        v_prno = repo.GetPrno(mmprm.MAT_CLASS);
                                    }
                                    mmprm.PR_NO = v_prno;
                                    //c.新增MM_PR_M
                                    repo.InsertMMPRM(mmprm, tmpdocno);
                                    //d.新增MM_PR_D
                                    repo.InsertMMPRD(mmprm, tmpdocno);
                                    //e.將備註帶至請購單
                                    //repo.UpdateMMPRM_MEMO()
                                }
                                else
                                {
                                    v_prno = repo.GetPrno01();
                                    mmprm.PR_NO = v_prno;
                                    //a. 取得需轉訂單院內碼與細項資料，若有取得資料，繼續執行後續步驟
                                    IEnumerable<MM_PR_D> mm_pr_d = repo.GetMmprd(tmpdocno);
                                    //b.新增至MM_PR01_M
                                    repo.InsertMMPRM01(mmprm, tmpdocno);
                                    //c.新增MM_PR01_D，以a所得資料迴圈執行
                                    foreach (MM_PR_D mmprd in mm_pr_d)
                                    {
                                        mmprd.PR_NO = v_prno;
                                        mmprd.UPDATE_USER = User.Identity.Name;
                                        mmprd.UPDATE_IP = DBWork.ProcIP;
                                        repo.InsertMMPRD01(mmprd);

                                    }
                                }
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
        public ApiResponse ApplyX(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0172Repository repo = new AA0172Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        string msg = string.Empty;
                        List<string> DocmStatusError = new List<string>();    // 此申請單狀態非核撥中
                        List<string> DocmSrcdocError = new List<string>();   // 轉申購單
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            if (repo.CheckExistsM11(tmp[i]))
                            {
                                DocmStatusError.Add(tmp[i]);
                                continue;
                            }
                            if (repo.CheckExistsMSrc(tmp[i]))
                            {
                                DocmSrcdocError.Add(tmp[i]);
                                continue;
                            }
                        }

                        if (DocmStatusError.Any())
                        {
                            string tempString = string.Empty;
                            if (msg == string.Empty)
                            {
                                msg += "請檢查下列項目：";
                            }
                            msg += "<br/><span style='color:red'>● 申請單狀態非未核可</span>：<br/>";
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

                        if (DocmSrcdocError.Any())
                        {
                            string tempString = string.Empty;
                            if (msg == string.Empty)
                            {
                                msg += "請檢查下列項目：";
                            }
                            msg += "<br/><span style='color:red'>● 轉申購單據</span>：<br/>";
                            foreach (string temp in DocmSrcdocError)
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
                        // 所有申請單通過檢核，進行更新
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            if (repo.CheckExistsD(tmp[i])) // 傳入DOCNO檢查申請單是否有院內碼項次
                            {
                                ME_DOCM me_docm = new ME_DOCM();
                                me_docm.DOCNO = tmp[i];
                                me_docm.UPDATE_USER = User.Identity.Name;
                                me_docm.UPDATE_IP = DBWork.ProcIP;
                                session.Result.afrs = repo.ApplyX(me_docm);
                            }
                            else
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = "<span style='color:red'>申請單號「" + tmp[i] + "」沒有院內碼項次</span>，請新增院內碼項次。";
                            }
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
        [HttpPost]
        public ApiResponse ApplyC(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0172Repository repo = new AA0172Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        string msg = string.Empty;
                        List<string> DocmStatusError = new List<string>();    // 此申請單狀態非核撥中
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            if (repo.CheckExistsM2(tmp[i]))
                            {
                                DocmStatusError.Add(tmp[i]);
                                continue;
                            }
                        }

                        if (DocmStatusError.Any())
                        {
                            string tempString = string.Empty;
                            if (msg == string.Empty)
                            {
                                msg += "請檢查下列項目：";
                            }
                            msg += "<br/><span style='color:red'>● 申請單狀態非核撥中</span>：<br/>";
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
                        //檢查明細是否已核可
                        List<string> DocdPostIdError = new List<string>();
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            if (repo.CheckAllPostIdNotA(tmp[i]))
                            {
                                DocdPostIdError.Add(tmp[i]);
                                continue;
                            }
                        }
                        if (DocdPostIdError.Any())
                        {
                            string tempString = string.Empty;
                            if (msg == string.Empty)
                            {
                                msg += "請檢查下列項目：";
                            }
                            msg += "<br/><span style='color:red'>● 申請單已有明細過帳狀態非未核可 或 已核可</span>：<br/>";
                            foreach (string temp in DocdPostIdError)
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
                        // 所有申請單通過檢核，進行更新
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            if (repo.CheckExistsD(tmp[i])) // 傳入DOCNO檢查申請單是否有院內碼項次
                            {
                                ME_DOCD me_docd = new ME_DOCD();
                                me_docd.DOCNO = tmp[i];
                                me_docd.POSTID = " ";   // 未核可
                                me_docd.UPDATE_USER = User.Identity.Name;
                                me_docd.UPDATE_IP = DBWork.ProcIP;
                                repo.UpdateAllPostid(me_docd);

                                ME_DOCM me_docm = new ME_DOCM();
                                me_docm.DOCNO = tmp[i];
                                me_docm.UPDATE_USER = User.Identity.Name;
                                me_docm.UPDATE_IP = DBWork.ProcIP;
                                session.Result.afrs = repo.ApplyC(me_docm);
                            }
                            else
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = "<span style='color:red'>申請單號「" + tmp[i] + "」沒有院內碼項次</span>，請新增院內碼項次。";
                            }
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

        [HttpPost]
        public ApiResponse GetFlowidCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0172Repository repo = new AA0172Repository(DBWork);
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
                    AA0172Repository repo = new AA0172Repository(DBWork);
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
                    AA0172Repository repo = new AA0172Repository(DBWork);
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
                    AA0172Repository repo = new AA0172Repository(DBWork);
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
                    AA0172Repository repo = new AA0172Repository(DBWork);
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
                    AA0172Repository repo = new AA0172Repository(DBWork);
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
                    AA0172Repository repo = new AA0172Repository(DBWork);
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
        public ApiResponse Excel(FormDataCollection form)
        {
            string fn = form.Get("FN");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0172Repository repo = new AA0172Repository(DBWork);

                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmpDOCNO = docno.Split(',');

                        JCLib.Excel.Export(form.Get("FN"), repo.GetExcel(tmpDOCNO));
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
        public ApiResponse ChkIsTransPrDetail(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0172Repository repo = new AA0172Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string seq = form.Get("SEQ").Substring(0, form.Get("SEQ").Length - 1);
                        string[] tmp = docno.Split(',');
                        string[] tmp_seq = seq.Split(',');
                        string rtnMsg = "";
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            if (repo.ChkIsTransPrDetail(tmp[i], tmp_seq[i]) > 0)
                            {
                                rtnMsg = tmp[i];
                                break;
                            }
                            else
                                rtnMsg = "Y";
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
        //單筆核可
        [HttpPost]
        public ApiResponse SignApply(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0172Repository repo = new AA0172Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string seq = form.Get("SEQ").Substring(0, form.Get("SEQ").Length - 1);

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
        public ApiResponse UpdatePostid(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0172Repository repo = new AA0172Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string seq = form.Get("SEQ").Substring(0, form.Get("SEQ").Length - 1);
                        string autotranspr = form.Get("autoTransPr"); //系統自動產生訂單及新單據
                        string[] tmp = docno.Split(',');
                        string[] tmp_seq = seq.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            if (repo.CheckWhidValid(tmp[i], DBWork.UserInfo.UserId) == false)
                            {
                                session.Result.success = false;
                                session.Result.msg = "無此庫房權限，請重新確認";
                                return session.Result;
                            }
                            if (autotranspr == "N")
                            {
                                string flowid = repo.GetFlowid(tmp[i]);
                                if (flowid == "11" || flowid == "0111" || flowid == "0611" || flowid == "2" || flowid == "0102" || flowid == "0602") //11:未核可,2:核撥中
                                {
                                    ME_DOCD me_docd = new ME_DOCD();
                                    me_docd.DOCNO = tmp[i];
                                    me_docd.SEQ = tmp_seq[i];
                                    me_docd.POSTID = "A";   // 已核可
                                    me_docd.UPDATE_USER = User.Identity.Name;
                                    me_docd.UPDATE_IP = DBWork.ProcIP;
                                    repo.UpdatePostid(me_docd);
                                }
                                //11:未核可 -> 2:核撥中
                                if (flowid == "11" || flowid == "0111" || flowid == "0611")
                                {
                                    ME_DOCM me_docm = new ME_DOCM();
                                    me_docm.DOCNO = tmp[i];
                                    me_docm.UPDATE_USER = User.Identity.Name;
                                    me_docm.UPDATE_IP = DBWork.ProcIP;
                                    repo.ApplyM(me_docm);
                                }
                            }
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
        [HttpPost]
        public ApiResponse UpdatePostidNull(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0172Repository repo = new AA0172Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string seq = form.Get("SEQ").Substring(0, form.Get("SEQ").Length - 1);
                        string[] tmp = docno.Split(',');
                        string[] tmp_seq = seq.Split(',');
                        string msg = string.Empty;
                        string strDocno = "", flowid = "";
                        List<string> DocdPostIdError = new List<string>(); //postid已核可
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            IEnumerable<ME_DOCD> docds = repo.GetDocds(tmp[i], tmp_seq[i]);
                            foreach (ME_DOCD docd in docds)
                            {
                                //檢查申請單狀態
                                if (repo.CheckExistsM11M2(docd.DOCNO))
                                {
                                    string tempString = string.Empty;
                                    if (msg == string.Empty)
                                    {
                                        msg += "請檢查下列項目：";
                                    }
                                    msg += "<br/><span style='color:red'>● 申請單狀態非 未核可 或 核撥中</span><br/>";
                                    continue;
                                }
                                //檢查明細postid
                                if (repo.ChkPostIdNotA(tmp[i], tmp_seq[i]) == true)
                                {
                                    DocdPostIdError.Add(docd.MMCODE);
                                    continue;
                                }
                            }
                            if (DocdPostIdError.Any())
                            {
                                string tempString = string.Empty;
                                if (msg == string.Empty)
                                {
                                    msg += "請檢查下列項目：";
                                }
                                msg += "<br/><span style='color:red'>● 申請單明細院內碼過帳狀態非已核可 或 已核可但已產生申購單</span>：<br/>";
                                foreach (string temp in DocdPostIdError)
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
                            if (strDocno != tmp[i])
                            {
                                flowid = repo.GetFlowid(tmp[i]);
                            }

                            if (flowid == "2" || flowid == "0102" || flowid == "0602")
                            {
                                ME_DOCD me_docd = new ME_DOCD();
                                me_docd.DOCNO = tmp[i];
                                me_docd.SEQ = tmp_seq[i];
                                me_docd.POSTID = "";   // 未核可
                                me_docd.UPDATE_USER = User.Identity.Name;
                                me_docd.UPDATE_IP = DBWork.ProcIP;
                                repo.UpdatePostid(me_docd);

                                //檢查明細是否有postid有值,若都沒有將flowid改為11
                                if (repo.CheckAllPostidNotNull(tmp[i]) == false)
                                {
                                    ME_DOCM me_docm = new ME_DOCM();
                                    me_docm.DOCNO = tmp[i];
                                    me_docm.UPDATE_USER = User.Identity.Name;
                                    me_docm.UPDATE_IP = DBWork.ProcIP;
                                    session.Result.afrs = repo.ApplyC(me_docm);
                                }
                            }
                            strDocno = tmp[i];
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
        [HttpPost]
        public ApiResponse DetailApply(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0172Repository repo = new AA0172Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string seq = form.Get("SEQ").Substring(0, form.Get("SEQ").Length - 1);
                        string autotranspr = form.Get("autoTransPr"); //系統自動產生訂單及新單據
                        string[] tmp = docno.Split(',');
                        string[] tmp_seq = seq.Split(',');
                        string msg = string.Empty;
                        string rtn = string.Empty;
                        string v_docno = string.Empty;
                        string v_matclass = string.Empty;
                        string v_aplyitem_note = string.Empty;
                        string v_prno = string.Empty;
                        List<string> DocdIstransprError = new List<string>();  // 轉申購但沒有對應供應商
                        List<string> DocdPostIdError = new List<string>(); //postid已核可
                        List<string> v_tranpr_list = new List<string>();  //轉申購流程
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            IEnumerable<ME_DOCD> docds = repo.GetDocds(tmp[i], tmp_seq[i]);
                            foreach (ME_DOCD docd in docds)
                            {
                                //檢查庫房權限
                                if (repo.CheckWhidValid(docd.DOCNO, DBWork.UserInfo.UserId) == false)
                                {
                                    string tempString = string.Empty;
                                    if (msg == string.Empty)
                                    {
                                        msg += "請檢查下列項目：";
                                    }
                                    msg += "<br/><span style='color:red'>● 無此庫房權限，請重新確認</span><br/>";
                                    continue;
                                }
                                //檢查申請單狀態
                                if (repo.CheckExistsM11M2(docd.DOCNO))
                                {
                                    string tempString = string.Empty;
                                    if (msg == string.Empty)
                                    {
                                        msg += "請檢查下列項目：";
                                    }
                                    msg += "<br/><span style='color:red'>● 申請單狀態非 未核可 或 核撥中</span><br/>";
                                    continue;
                                }
                                //檢查明細postid
                                if (repo.ChkPostidNotNull(tmp[i], tmp_seq[i]) == true)
                                {
                                    DocdPostIdError.Add(docd.MMCODE);
                                    continue;
                                }
                                //要產生申購單且申購量>0檢查廠商代碼
                                if (autotranspr == "Y")
                                {
                                    if (string.IsNullOrEmpty(docd.PR_QTY))
                                    {
                                        docd.PR_QTY = "0";
                                    }
                                    if (int.Parse(docd.PR_QTY) > 0)
                                    {
                                        if (repo.ChkprExistsVender(tmp[i], tmp_seq[i]) == false)
                                        {
                                            DocdIstransprError.Add(docd.MMCODE);
                                        }
                                    }
                                    continue;
                                }
                            }
                            // 若list中有東西，串成文字
                            if (DocdIstransprError.Any())
                            {
                                string tempString = string.Empty;
                                if (msg == string.Empty)
                                {
                                    msg += "請檢查下列項目：";
                                }
                                msg += "<br/><span style='color:red'>● 院內碼有輸入申購量但無對應可用廠商</span>：<br/>";
                                foreach (string temp in DocdIstransprError)
                                {
                                    if (tempString.Length > 0)
                                    {
                                        tempString += "、";
                                    }
                                    tempString += temp;
                                }
                                msg += tempString;
                            }
                            if (DocdPostIdError.Any())
                            {
                                string tempString = string.Empty;
                                if (msg == string.Empty)
                                {
                                    msg += "請檢查下列項目：";
                                }
                                msg += "<br/><span style='color:red'>●申請明細院內碼過帳狀態非未核可</span>：<br/>";
                                foreach (string temp in DocdPostIdError)
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
                            foreach (ME_DOCD docd in docds)
                            {
                                v_docno = docd.DOCNO;
                                v_matclass = repo.GetMatClass(v_docno);
                                v_aplyitem_note = docd.APLYITEM_NOTE;
                                if (string.IsNullOrEmpty(docd.APVQTY))
                                {
                                    docd.APVQTY = "0";
                                }
                                if (string.IsNullOrEmpty(docd.PR_QTY))
                                {
                                    docd.PR_QTY = "0";
                                }
                                if (int.Parse(docd.APVQTY) > 0)  //核撥量>0
                                {
                                    string flowid = repo.GetFlowid(v_docno);
                                    //(1)ME_DOCD
                                    if (flowid == "11" || flowid == "0111" || flowid == "0611" || flowid == "2" || flowid == "0102" || flowid == "0602") //11:未核可,2:核撥中
                                    {
                                        ME_DOCD me_docd = new ME_DOCD();
                                        me_docd.DOCNO = docd.DOCNO;
                                        me_docd.SEQ = docd.SEQ;
                                        me_docd.POSTID = "A";   // 已核可
                                        me_docd.UPDATE_USER = User.Identity.Name;
                                        me_docd.UPDATE_IP = DBWork.ProcIP;
                                        repo.UpdatePostid(me_docd);
                                    }
                                    //(2)ME_DOCM
                                    //11:未核可 -> 2:核撥中
                                    if (flowid == "11" || flowid == "0111" || flowid == "0611")
                                    {
                                        ME_DOCM me_docm = new ME_DOCM();
                                        me_docm.DOCNO = docd.DOCNO;
                                        me_docm.UPDATE_USER = User.Identity.Name;
                                        me_docm.UPDATE_IP = DBWork.ProcIP;
                                        repo.ApplyM(me_docm);
                                    }
                                }
                                if (int.Parse(docd.PR_QTY) > 0) //申購量>0
                                {
                                    v_tranpr_list.Add(docd.SEQ);
                                }
                                if ((int.Parse(docd.APVQTY) == 0) && (int.Parse(docd.PR_QTY) == 0))   //核撥量>0
                                {
                                    string flowid = repo.GetFlowid(v_docno);
                                    //(1)ME_DOCD
                                    if (flowid == "11" || flowid == "0111" || flowid == "0611" || flowid == "2" || flowid == "0102" || flowid == "0602") //11:未核可,2:核撥中
                                    {
                                        ME_DOCD me_docd = new ME_DOCD();
                                        me_docd.DOCNO = docd.DOCNO;
                                        me_docd.SEQ = docd.SEQ;
                                        me_docd.POSTID = "D";   // 已點收
                                        me_docd.UPDATE_USER = User.Identity.Name;
                                        me_docd.UPDATE_IP = DBWork.ProcIP;
                                        repo.UpdatePostid(me_docd);
                                    }
                                    //(2)ME_DOCM
                                    //11:未核可 -> 2:核撥中
                                    if (flowid == "11" || flowid == "0111" || flowid == "0611")
                                    {
                                        ME_DOCM me_docm = new ME_DOCM();
                                        me_docm.DOCNO = docd.DOCNO;
                                        me_docm.UPDATE_USER = User.Identity.Name;
                                        me_docm.UPDATE_IP = DBWork.ProcIP;
                                        repo.ApplyM(me_docm);
                                    }
                                }
                            }
                        }
                        //若有申購量需產生PR
                        if (v_tranpr_list.Any())
                        {
                            if (v_matclass != "01")
                            {
                                //a.取得申購單號
                                v_prno = repo.GetPrno(v_matclass);
                                //b.檢查申購單號是否已存在於MM_PR_M，若存在，等一秒後再執行a，直到v_prno不存在於MM_PR_M
                                while (repo.CheckExistsMMPRM(v_prno) == true)
                                {
                                    System.Threading.Thread.Sleep(1000);
                                    v_prno = repo.GetPrno(v_matclass);
                                }
                                MM_PR_M mmprm = new MM_PR_M();
                                mmprm.PR_NO = v_prno;
                                mmprm.MAT_CLASS = v_matclass;
                                mmprm.MEMO = v_aplyitem_note;
                                mmprm.UPDATE_USER = User.Identity.Name;
                                mmprm.UPDATE_IP = DBWork.ProcIP;
                                //c.新增MM_PR_M
                                repo.InsertMM_PR_M(mmprm);

                                foreach (string v_seq in v_tranpr_list)
                                {
                                    //d.新增MM_PR_D
                                    repo.InsertMM_PR_D(v_docno, v_seq, v_prno, User.Identity.Name, DBWork.ProcIP);
                                    //e. 修改ME_DOCD                                 
                                    ME_DOCD me_docd = new ME_DOCD();
                                    me_docd.DOCNO = v_docno;
                                    me_docd.SEQ = v_seq;
                                    me_docd.POSTID = "A";   // 已核可
                                    me_docd.RDOCNO = v_prno;
                                    me_docd.UPDATE_USER = User.Identity.Name;
                                    me_docd.UPDATE_IP = DBWork.ProcIP;
                                    repo.UpdatePostid(me_docd);
                                }
                                //f.修改ME_DOCM
                                ME_DOCM me_docm = new ME_DOCM();
                                me_docm.DOCNO = v_docno;
                                me_docm.UPDATE_USER = User.Identity.Name;
                                me_docm.UPDATE_IP = DBWork.ProcIP;
                                repo.ApplyM(me_docm);
                            }
                            else
                            {
                                //a.取得申購單號
                                v_prno = repo.GetPrno01();
                                //b.檢查申購單號是否已存在於MM_PR01_M，若存在，等一秒後再執行a，直到v_prno不存在於MM_PR01_M
                                while (repo.CheckExistsMMPR01M(v_prno) == true)
                                {
                                    System.Threading.Thread.Sleep(1000);
                                    v_prno = repo.GetPrno01();
                                }
                                MM_PR_M mmprm = new MM_PR_M();
                                mmprm.PR_NO = v_prno;
                                mmprm.MAT_CLASS = v_matclass;
                                mmprm.MEMO = v_aplyitem_note;
                                mmprm.UPDATE_USER = User.Identity.Name;
                                mmprm.UPDATE_IP = DBWork.ProcIP;
                                //c.新增MM_PR01_M
                                repo.InsertMM_PR01_M(mmprm);
                                //d.新增MM_PR01_D
                                foreach (string v_seq in v_tranpr_list)
                                {
                                    repo.InsertMM_PR01_D(v_docno, v_seq, v_prno, User.Identity.Name, DBWork.ProcIP);
                                    //e. 修改ME_DOCD                                 
                                    ME_DOCD me_docd = new ME_DOCD();
                                    me_docd.DOCNO = v_docno;
                                    me_docd.SEQ = v_seq;
                                    me_docd.POSTID = "A";   // 已核可
                                    me_docd.RDOCNO = v_prno;
                                    me_docd.UPDATE_USER = User.Identity.Name;
                                    me_docd.UPDATE_IP = DBWork.ProcIP;
                                    repo.UpdatePostid(me_docd);
                                }
                                //f.修改ME_DOCM
                                ME_DOCM me_docm = new ME_DOCM();
                                me_docm.DOCNO = v_docno;
                                me_docm.UPDATE_USER = User.Identity.Name;
                                me_docm.UPDATE_IP = DBWork.ProcIP;
                                repo.ApplyM(me_docm);
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
        public ApiResponse GetLoginInfo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0172Repository repo = new AA0172Repository(DBWork);
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