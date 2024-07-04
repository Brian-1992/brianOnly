using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AB;
using WebApp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace WebApp.Controllers.AB
{
    public class AA0012Controller : SiteBase.BaseApiController
    {
        // 查詢
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
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0012Repository(DBWork);
                    var doctype = "MR2";
                    if (p6 is null || p6 == "")
                    {
                        doctype = "MR1234";
                    }
                    else
                    {
                        if (p6 == "1")
                        {
                            doctype = "MR12";
                        }
                        else
                        {
                            doctype = "MR34";
                        }
                    }
                    session.Result.etts = repo.GetAllM(p0, p1, p2, arr_p3, p4, arr_p5, doctype, page, limit, sorters);
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
            var p2 = form.Get("p2");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0012Repository(DBWork);

                    if (p0 == "") {
                        return session.Result;
                    }

                    string doctype = repo.GetDocmdoctype(p0);
                    if (doctype == "MR1" || doctype == "MR2") {
                        session.Result.etts = repo.GetAllD(p0, p1, p2, page, limit, sorters);
                    }
                    if (doctype == "MR3" || doctype == "MR4")
                    {
                        session.Result.etts = repo.GetAllDMr34(p0, p1, p2, page, limit, sorters);
                    }
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse UpdateMeDocd(ME_DOCD me_docd)
        {
            IEnumerable<ME_DOCD> me_docds = JsonConvert.DeserializeObject<IEnumerable<ME_DOCD>>(me_docd.ITEM_STRING);
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0012Repository repo = new AA0012Repository(DBWork);

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
        public ApiResponse Apply(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0012Repository repo = new AA0012Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            if (repo.CheckExistsM(tmp[i]))
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = "<span style='color:red'>此申請單狀態非已點收</span>不得核撥。";
                                return session.Result;
                            }
                            if (repo.CheckExistsD(tmp[i]) == false) // 傳入DOCNO檢查申請單是否有院內碼項次
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = "<span style='color:red'>申請單號「" + tmp[i] + "」沒有院內碼項次</span>，請新增院內碼項次。";
                                return session.Result;
                            }
                            
                            ME_DOCM me_docm = new ME_DOCM();
                            me_docm.DOCNO = tmp[i];
                            me_docm.UPDATE_USER = User.Identity.Name;
                            me_docm.UPDATE_IP = DBWork.ProcIP;
                            var rtn = repo.CallProc(tmp[i], User.Identity.Name, DBWork.ProcIP);
                            if (rtn == "Y")
                            {
                                string doctype = repo.GetDocmdoctype(tmp[i]);
                                if (doctype == "MR1" || doctype == "MR2")
                                {
                                    session.Result.afrs = repo.ApplyD(me_docm);
                                }
                                if (doctype == "MR3" || doctype == "MR4")
                                {
                                    session.Result.afrs = repo.ApplyDMr34(me_docm);
                                }
                            }
                            else
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = "<span style='color:red'>申請單號「" + tmp[i] + "」</span>，發生執行錯誤，" + rtn + "。";
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
        public ApiResponse GetTask()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0012Repository repo = new AA0012Repository(DBWork);
                    session.Result.msg = repo.GetTaskid(User.Identity.Name);
                    session.Result.success = true;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetDocnoCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0012Repository repo = new AA0012Repository(DBWork);
                    var taskid = repo.GetTaskid(User.Identity.Name);
                    session.Result.etts = repo.GetDocnoCombo(taskid);
                }
                catch
                {
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
                    AA0012Repository repo = new AA0012Repository(DBWork);
                    session.Result.etts = repo.GetFlowidCombo();
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
                    AA0012Repository repo = new AA0012Repository(DBWork);
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
        public ApiResponse GetAppDeptCombo(FormDataCollection form)
        {
            var matclass = form.Get("matclass");
            var dates = form.Get("dates");
            var datee = form.Get("datee");
            var applykind = form.Get("applykind");
            var flowid = form.Get("flowid");
            string[] arr_p3 = { };
            if (!string.IsNullOrEmpty(flowid))
            {
                arr_p3 = flowid.Trim().Split(','); //用,分割
            }
            string[] arr_p5 = { };
            if (!string.IsNullOrEmpty(matclass))
            {
                arr_p5 = matclass.Trim().Split(','); //用,分割
            }
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0012Repository repo = new AA0012Repository(DBWork);
                    session.Result.etts = repo.GetAppDeptCombo(arr_p5, dates, datee, applykind, arr_p3);
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
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0012Repository repo = new AA0012Repository(DBWork);
                    session.Result.etts = repo.GetMmCodeCombo(p0, p1, p2, page, limit, "");
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
                    AA0012Repository repo = new AA0012Repository(DBWork);
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
        public ApiResponse GetFrwhCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0012Repository repo = new AA0012Repository(DBWork);
                    session.Result.etts = repo.GetFrwhCombo();
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
                    AA0012Repository repo = new AA0012Repository(DBWork);
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
                    AA0012Repository repo = new AA0012Repository(DBWork);
                    //var taskid = repo.GetTaskid(User.Identity.Name);
                    //if (taskid == "2")
                    //{
                    //    session.Result.etts = repo.GetMatclass2Combo();
                    //}
                    //else
                    //{
                    //    session.Result.etts = repo.GetMatclass3Combo();
                    //}
                    session.Result.etts = repo.GetMatclassCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetStoreidCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0012Repository repo = new AA0012Repository(DBWork);
                    session.Result.etts = repo.GetStoreidCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        #region 2020-06-04 非庫備detail修改
        public ApiResponse UpdateMeDocdMr34(ME_DOCD me_docd)
        {
            IEnumerable<ME_DOCD> me_docds = JsonConvert.DeserializeObject<IEnumerable<ME_DOCD>>(me_docd.ITEM_STRING);
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0012Repository repo = new AA0012Repository(DBWork);

                    foreach (ME_DOCD docd in me_docds)
                    {
                        docd.UPDATE_USER = User.Identity.Name;
                        docd.UPDATE_IP = DBWork.ProcIP;

                        session.Result.afrs = repo.UpdateMeDocdMr34(docd);
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


        #endregion
    }
}