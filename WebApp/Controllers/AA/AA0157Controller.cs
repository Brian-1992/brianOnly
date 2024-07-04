using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using Newtonsoft.Json;
using WebApp.Models;

namespace WebApp.Controllers.AA
{
    public class AA0157Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");
            var p6 = form.Get("p6");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            string[] arr_p0 = { };
            if (!string.IsNullOrEmpty(p0))
            {
                arr_p0 = p0.Trim().Split(','); //用,分割
            }
            string[] arr_p4 = { };
            if (!string.IsNullOrEmpty(p4))
            {
                arr_p4 = p4.Trim().Split(','); //用,分割
            }
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0157Repository(DBWork);
                    session.Result.etts = repo.GetAllM(arr_p0, p1, p2, p3, arr_p4, p5, p6, User.Identity.Name, page, limit, sorters);
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
                    var repo = new AA0157Repository(DBWork);
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
        public ApiResponse GetMatclassCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0157Repository repo = new AA0157Repository(DBWork);
                    session.Result.etts = repo.GetMatclassCombo(User.Identity.Name);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetApplyKindCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0157Repository repo = new AA0157Repository(DBWork);
                    session.Result.etts = repo.GetApplyKindCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetFlowidCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0157Repository repo = new AA0157Repository(DBWork);
                    session.Result.etts = repo.GetFlowidCombo(User.Identity.Name);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetTowhCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0157Repository repo = new AA0157Repository(DBWork);
                    session.Result.etts = repo.GetTowhCombo(User.Identity.Name);
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
                    AA0157Repository repo = new AA0157Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeDocd(p0, p1, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // 檢查所選申請單內是否有註記轉申購單品項
        [HttpPost]
        public ApiResponse ChkIsTransPr(ME_DOCM me_docm)
        {
            IEnumerable<ME_DOCM> me_docms = JsonConvert.DeserializeObject<IEnumerable<ME_DOCM>>(me_docm.ITEM_STRING);
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    AA0157Repository repo = new AA0157Repository(DBWork);

                    string chkItemFound = "N";
                    foreach (ME_DOCM docm in me_docms)
                    {
                        if (repo.ChkIsTransPr(docm.DOCNO))
                            chkItemFound = "Y";
                    }
                    session.Result.msg = chkItemFound;

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // 檢查所選申請單內是否有(預計撥發量+調撥量)小於申請量且未註記轉申購單品項
        [HttpPost]
        public ApiResponse chkAppqty(ME_DOCM me_docm)
        {
            IEnumerable<ME_DOCM> me_docms = JsonConvert.DeserializeObject<IEnumerable<ME_DOCM>>(me_docm.ITEM_STRING);
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    AA0157Repository repo = new AA0157Repository(DBWork);

                    string chkItemFound = "";
                    foreach (ME_DOCM docm in me_docms)
                    {
                        string chkMmcode = repo.ChkAppqty(docm.DOCNO);
                        if (chkMmcode != "")
                            chkItemFound = docm.DOCNO + "^" + chkMmcode;
                    }
                    session.Result.msg = chkItemFound;

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Apply(ME_DOCM me_docm)
        {
            IEnumerable<ME_DOCM> me_docms = JsonConvert.DeserializeObject<IEnumerable<ME_DOCM>>(me_docm.ITEM_STRING);
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0157Repository repo = new AA0157Repository(DBWork);

                    List<string> v_docno_list = new List<string>();
                    // 先做完檢查的部分,都通過再繼續往下做
                    foreach (ME_DOCM docm in me_docms)
                    {
                        string errMsg = "";
                        // 檢查資料庫目前單據狀態是否為核撥中
                        if (repo.ChkFlowid(docm.DOCNO) != "2")
                            errMsg = docm.DOCNO + "申請單狀態不為核撥中，請重新確認";
                        // 申請單需有院內碼明細資料
                        else if (repo.ChkDocd(docm.DOCNO) == false)
                            errMsg = docm.DOCNO + "申請單需有院內碼明細資料";
                        else if (repo.ChkAppqty2(docm.DOCNO))
                            errMsg = docm.DOCNO + "存在品項 (預計撥發量+調撥量)大於申請量，請重新確認";
                        else if (repo.ChkBwMqty(docm.DOCNO))
                            errMsg = docm.DOCNO + "存在品項 調撥量大於戰備存量，請重新確認";
                        //else if (ChkExptDistqty(docm.DOCNO))
                        //    errMsg = docm.DOCNO + "存在品項 預計撥發量大於建議核撥量，請重新確認";
                        else
                        {
                            // 各申請單明細(預計撥發量+調撥量)是否大於(民庫存量+戰備存量)
                            // 1120524依使用者建議 民庫存量 改用 囤儲量
                            string chkMmcode = repo.CheckExptBwQtyMmcode(docm.DOCNO);
                            if (chkMmcode != "")
                                errMsg = "申請單號:" + docm.DOCNO + " <span style='color:red'>院內碼:" + chkMmcode +"</span><br/>" +" 預計撥發量加調撥量大於囤儲量+戰備存量";
                            else
                            {
                                // 各申請單明細預計撥發量是否大於民庫存量
                                // 1120524依使用者建議 民庫存量 改用 囤儲量
                                chkMmcode = repo.CheckExptQtyMmcode(docm.DOCNO);
                                if (chkMmcode != "")
                                    errMsg = "申請單號:" + docm.DOCNO + "<span style='color:red'> 院內碼:" + chkMmcode + "</span><br/>" + " 預計撥發量大於囤儲量";
                            }
                        }

                        if (errMsg != "")
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = errMsg;
                            DBWork.Rollback();
                            return session.Result;
                        }
                    }

                    foreach (ME_DOCM docm in me_docms)
                    {
                        docm.UPDATE_USER = User.Identity.Name;
                        docm.UPDATE_IP = DBWork.ProcIP;

                        // 更新ME_DOCD
                        // 未設定轉申購依據核撥量設定修改核撥與點收數量
                        repo.Apply_UpdateMeDocd1(docm);
                        // 有設定轉申購核撥量與點收量為0
                        repo.Apply_UpdateMeDocd2(docm);

                        // 執行SP：POST_DOC (為進行核撥)
                        SP_MODEL sp = repo.Apply_PostDoc(docm.DOCNO, User.Identity.Name, DBWork.ProcIP);
                        if (sp.O_RETID == "N")
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = sp.O_ERRMSG;
                            DBWork.Rollback();
                            return session.Result;
                        }

                        // 轉新申請單
                        // 有註記轉申購若有找到則取得v_docno
                        if (repo.ChkIsTransPr(docm.DOCNO))
                        {
                            docm.ITEM_STRING = repo.GetVDocno(docm.DOCNO);
                            v_docno_list.Add(docm.ITEM_STRING);
                            // 新增ME_DOCM
                            repo.Apply_InsertMeDocm(docm);
                            // 新增ME_DOCD
                            repo.Apply_InsertMeDocd(docm);
                            // 更新原始ME_DOCD欠撥原因
                            repo.Apply_UpdateMeDocdReason(docm);
                        }
                    }

                    // 申購單會由SP產生,故以下動作不做
                    // 有註記轉申購建立的新申請單
                    //string[] v_docno = v_docno_list.ToArray();

                    //// 產生申購單
                    //// 有任何註記轉申購的資料才繼續做
                    //if (v_docno_list.Any())
                    //{
                    //    // 將本次勾選的DOCNO組成string array
                    //    string tempString = string.Empty;
                    //    foreach (ME_DOCM temp in me_docms)
                    //    {
                    //        tempString += temp.DOCNO + ",";
                    //    }
                    //    tempString = tempString.Substring(0, tempString.Length - 1);

                    //    // 依本次勾選的DOCNO取得所有MAT_CLASS,再依MAT_CLASS建立申購單
                    //    string[] tmpdocno = tempString.Split(',');
                    //    IEnumerable<string> mm_pr_m_matclass = repo.GetMatClassList(tmpdocno);
                    //    foreach (string v_matclass in mm_pr_m_matclass)
                    //    {
                    //        string v_prno = repo.GetVPrno(v_matclass);
                    //        // 檢查申購單號是否已存在於MM_PR_M，若存在則等一秒後再執行一次，直到v_prno不存在於MM_PR_M
                    //        while (repo.ChkPrNo(v_prno) == true)
                    //        {
                    //            System.Threading.Thread.Sleep(1000);
                    //            v_prno = repo.GetVPrno(v_matclass);
                    //        }

                    //        // 新增MM_PR_M
                    //        repo.Apply_InsertPrM(v_prno, v_matclass, User.Identity.Name, DBWork.ProcIP);
                    //        // 新增MM_PR_D
                    //        repo.Apply_InsertPrD(v_docno, v_prno, v_matclass, User.Identity.Name, DBWork.ProcIP);
                    //    }
                    //}

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
        public ApiResponse ApplyX(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0157Repository repo = new AA0157Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            if (repo.ChkFlowid(tmp[i]) != "2")
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = tmp[i] + "申請單狀態不為核撥中，請重新確認";
                                DBWork.Rollback();
                                return session.Result;
                            }
                            else if (repo.ChkSrcdoc(tmp[i]))
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = tmp[i] + "轉申購單據不可退回";
                                DBWork.Rollback();
                                return session.Result;
                            }
                            else if (repo.ChkApptime(tmp[i]) == false)
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = tmp[i] + "超出期限不可退回";
                                DBWork.Rollback();
                                return session.Result;
                            }

                            ME_DOCM me_docm = new ME_DOCM();
                            me_docm.DOCNO = tmp[i];
                            me_docm.UPDATE_USER = User.Identity.Name;
                            me_docm.UPDATE_IP = DBWork.ProcIP;
                            session.Result.afrs = repo.ApplyX(me_docm);
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
        public ApiResponse UpdateMeDocd(ME_DOCD me_docd)
        {
            IEnumerable<ME_DOCD> me_docds = JsonConvert.DeserializeObject<IEnumerable<ME_DOCD>>(me_docd.ITEM_STRING);
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0157Repository repo = new AA0157Repository(DBWork);

                    foreach (ME_DOCD docd in me_docds)
                    {
                        docd.UPDATE_USER = User.Identity.Name;
                        docd.UPDATE_IP = DBWork.ProcIP;

                        session.Result.afrs = repo.UpdateMeDocd(docd);
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
        public ApiResponse TransConfirmMeDocd(ME_DOCD me_docd)
        {
            IEnumerable<ME_DOCD> me_docds = JsonConvert.DeserializeObject<IEnumerable<ME_DOCD>>(me_docd.ITEM_STRING);
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0157Repository repo = new AA0157Repository(DBWork);

                    foreach (ME_DOCD docd in me_docds)
                    {
                        docd.UPDATE_USER = User.Identity.Name;
                        docd.UPDATE_IP = DBWork.ProcIP;

                        // 是否全院停用
                        if (repo.CheckOrderDcFlag(docd) == "Y")
                        {
                            session.Result.msg = docd.MMCODE + "已全院停用，不可申購";
                            session.Result.success = false;
                            DBWork.Rollback();
                            return session.Result;
                        }
                        // 是否為子藥
                        else if (repo.CheckParcode(docd) == "Y")
                        {
                            session.Result.msg = docd.MMCODE + "為子藥，不可申購";
                            session.Result.success = false;
                            DBWork.Rollback();
                            return session.Result;
                        }

                        session.Result.afrs = repo.TransConfirmMeDocd(docd);
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
        public ApiResponse TransCancelMeDocd(ME_DOCD me_docd)
        {
            IEnumerable<ME_DOCD> me_docds = JsonConvert.DeserializeObject<IEnumerable<ME_DOCD>>(me_docd.ITEM_STRING);
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0157Repository repo = new AA0157Repository(DBWork);

                    foreach (ME_DOCD docd in me_docds)
                    {
                        docd.UPDATE_USER = User.Identity.Name;
                        docd.UPDATE_IP = DBWork.ProcIP;

                        session.Result.afrs = repo.TransCancelMeDocd(docd);
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

        // 各申請單明細預計撥發量是否大於建議核撥量
        //public bool ChkExptDistqty(string docno)
        //{
        //    bool rtnBool = false;
        //    using (WorkSession session = new WorkSession())
        //    {
        //        var DBWork = session.UnitOfWork;
        //        AA0157Repository repo = new AA0157Repository(DBWork);
        //        IEnumerable<ME_DOCD> me_docds = repo.GetAllD(docno, "", 1, 10000, "[{\"property\":\"SEQ\",\"direction\":\"DESC\"}]");
        //        foreach (ME_DOCD docd in me_docds)
        //        {
        //            if (Convert.ToInt32(docd.EXPT_DISTQTY) > Convert.ToInt32(docd.B_INV_QTY))
        //                rtnBool = true;
        //        }
        //    }
        //    return rtnBool;
        //}
    }
}