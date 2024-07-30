using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AB;
using WebApp.Models;
using Newtonsoft.Json.Linq;
using WebApp.Models.AB;
using System;

namespace WebApp.Controllers.AB
{
    public class AB0109Controller : SiteBase.BaseApiController
    {
        #region GetComboBox
        [HttpPost]
        public ApiResponse GetMmCodeCombo(FormDataCollection form)
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
                    AB0109Repository repo = new AB0109Repository(DBWork);
                    session.Result.etts = repo.GetMmCodeCombo(p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetTOWHcombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0109Repository repo = new AB0109Repository(DBWork);
                    session.Result.etts = repo.GetTOWHcombo(User.Identity.Name);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetStatusCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0109Repository repo = new AB0109Repository(DBWork);
                    session.Result.etts = repo.GetStatusCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        #endregion 

        //查詢
        [HttpPost]
        public ApiResponse GetAll(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = User.Identity.Name;
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0109Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1, p2,p3, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse Create(AB0109 def)
        {
            using (WorkSession session = new WorkSession(this))  // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0109Repository(DBWork);
                    string msg = string.Empty;

                    bool mmcodeValid = repo.ChkExists_WHMM01(def.MMCODE); //WHMM 存在該筆MMCODE，僅存在的WHNO可申請
                    if (mmcodeValid)
                    {
                        bool whnoValid = repo.ChkExists_WHMM02(def.MMCODE, def.TOWH);
                        if (whnoValid == false) //WHMM 不存在該筆MMCODE+TOWH
                        {
                            msg += "<span style='color:red'>院內碼</span>" + def.MMCODE + "<span style='color:red'>,庫房</span>" + def.TOWH + "<br/>" +
                                   "<span style='color:red'>與 藥衛材可存放庫房檔 設定不符，請洽衛保室</span><br/>";
                        }
                    }
                    //MMCODE不存WHMM表全院可申請

                    if (repo.CheckMmcodeValid(def.MMCODE) == false) {
                        msg = "院內碼不可申請，請重新確認";
                    }

                    // 若msg不為空，表示有錯誤
                    if (msg != string.Empty)
                    {
                        session.Result.success = false;
                        session.Result.msg = msg;
                        return session.Result;
                    }

                    def.CREATE_USER = User.Identity.Name;
                    def.UPDATE_USER = User.Identity.Name;
                    def.UPDATE_IP = DBWork.ProcIP;
                    def.CRDOCNO = repo.GetCrdocno();  //取得緊急醫療出貨單編號 
                    if ((def.M_APPLYID == "E") && (def.M_CONTID == "3"))
                    {
                        def.ISSMALL = "Y";
                        session.Result.afrs = repo.InsCR_DOC_SMALL(def);
                    }
                    else
                    {
                        def.ISSMALL = "N";
                    }
                    session.Result.afrs = repo.Create(def);

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
        //修改
        [HttpPost]
        public ApiResponse Update(AB0109 def)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0109Repository(DBWork);
                    string msg = string.Empty;
                    bool mmcodeValid = repo.ChkExists_WHMM01(def.MMCODE); //WHMM 存在該筆MMCODE，僅存在的WHNO可申請
                    if (mmcodeValid)
                    {
                        bool whnoValid = repo.ChkExists_WHMM02(def.MMCODE, def.TOWH);
                        if (whnoValid == false) //WHMM 不存在該筆MMCODE+TOWH
                        {
                            msg += "<span style='color:red'>院內碼</span>" + def.MMCODE + "<span style='color:red'>,庫房</span>" + def.TOWH + "<br/>" +
                                   "<span style='color:red'>與 藥衛材可存放庫房檔 設定不符，請洽衛保室</span><br/>";
                        }
                    }
                    //MMCODE不存WHMM表全院可申請

                    // 若msg不為空，表示有錯誤
                    if (msg != string.Empty)
                    {
                        session.Result.success = false;
                        session.Result.msg = msg;
                        return session.Result;
                    }

                    def.UPDATE_USER = User.Identity.Name;
                    def.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.Update(def);
                    if ((def.M_APPLYID == "E") && (def.M_CONTID == "3"))
                    {
                        session.Result.afrs = repo.UpCR_DOC_SMALL(def);
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
        // 刪除
        [HttpPost]
        public ApiResponse Delete(AB0109 def)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0109Repository(DBWork);
                    def.UPDATE_USER = User.Identity.Name;
                    def.UPDATE_IP = DBWork.ProcIP;
                    if ((def.M_APPLYID == "E") && (def.M_CONTID == "3"))
                    {
                        session.Result.afrs = repo.DelCR_DOC_SMALL(def);
                    }
                    session.Result.afrs = repo.Delete(def);
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
        // 申請
        [HttpPost]
        public ApiResponse Apply(AB0109 def)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0109Repository(DBWork);
                    def.APPID = User.Identity.Name;
                    def.UPDATE_USER = User.Identity.Name;
                    def.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.Apply(def);
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

        // 撤銷
        [HttpPost]
        public ApiResponse Reject(AB0109 def)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0109Repository(DBWork);
                    def.UPDATE_USER = User.Identity.Name;
                    def.UPDATE_IP = DBWork.ProcIP;
                    if ((def.M_APPLYID == "E") && (def.M_CONTID == "3"))
                    {
                        session.Result.afrs = repo.RejCR_DOC_SMALL(def);
                    }
                    session.Result.afrs = repo.Reject(def);
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
        //放大鏡使用的連結
        [HttpPost]
        public ApiResponse GetMmcode(FormDataCollection form)
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
                    AB0109Repository repo = new AB0109Repository(DBWork);
                    AB0109Repository.MI_MAST_QUERY_PARAMS query = new AB0109Repository.MI_MAST_QUERY_PARAMS();
                    query.MMCODE = form.Get("MMCODE") == null ? "" : form.Get("MMCODE").ToUpper();
                    query.MMNAME_C = form.Get("MMNAME_C") == null ? "" : form.Get("MMNAME_C").ToUpper();
                    query.MMNAME_E = form.Get("MMNAME_E") == null ? "" : form.Get("MMNAME_E").ToUpper();
                    session.Result.etts = repo.GetMmcode(query, page, limit, sorters);
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