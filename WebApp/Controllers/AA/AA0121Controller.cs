using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebApp.Controllers.AA
{
    public class AA0121Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse QueryM(FormDataCollection form)  // SELECT * FROM MI_WHID 
        {
            var p0 = "";
            var p2 = "";

            p2 = form.Get("p2");

            if (p2 == "normalByUser")
            {
                p0 = form.Get("p0");
            }
            else
            {
                p0 = form.Get("p0");
            }

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0121Repository(DBWork);
                    if (p2 == "normalByUser")
                        session.Result.etts = repo.GetMasterAllByUser(p0, page, limit, sorters);
                    else if (p2 == "normalByWH")
                        // session.Result.etts = repo.GetMasterAllByWH(p0, page, limit, sorters);
                        session.Result.etts = repo.GetMasterAllByWH(p0, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse QueryD(FormDataCollection form)   //SELECT * FROM UR_ID where INID tab1  
        {
            var p0 = "";
            var p1 = "";
            var p2 = "";
            var p3 = "";
            var p4 = "";

            p2 = form.Get("p2");

            if (p2 == "userpopwindow")
            {
                p0 = form.Get("p0");
                p3 = form.Get("p3");
                p4 = form.Get("p4");
            }
            else
            {
                p0 = form.Get("p0");
                p1 = form.Get("p1");
                p4 = form.Get("p4");
            }

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0121Repository(DBWork);
                    if (p2 == "userpopwindow")
                        session.Result.etts = repo.GetUserPopWindow(p0, p3, page, limit, sorters);
                    else
                        session.Result.etts = repo.GetDetailAll(p0, p1, p4, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse QueryUserCode(FormDataCollection form)  //SELECT * FROM UR_ID where TUSER for tab2 openwindow 
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
                    var repo = new AA0121Repository(DBWork);
                    session.Result.etts = repo.GetUserCode(p0, p1, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse QWHMASTAll(FormDataCollection form)   // SELECT * FROM MI_WHMAST
        {
            var p0 = "";
            var p1 = "";
            var p2 = "";
            var p3 = "";
            var p4 = "";

            p2 = form.Get("p2");    // flag

            if (p2 == "normalByWH")
            {
                p0 = form.Get("p0");    // 庫房代碼
            }
            else if (p2 == "normalByUser")
            {
                p0 = form.Get("p0");    // 庫房代碼
                p1 = form.Get("p1");    // 人員代碼
                /////p3 = form.Get("p3");    // 責任中心
                p4 = form.Get("p4");    // 人員姓名
            }
            else if (p2 == "WHpopwindow")
            {
                p0 = form.Get("p0");    // 庫房代碼
                p1 = form.Get("p1");    // 人員代碼
            }


            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0121Repository(DBWork);
                    if (p2 == "normalByWH")   //for tab2 detail Grid2 by conditon ipt fetch data 
                        session.Result.etts = repo.GetWHMASTByWH(p0, page, limit, sorters);
                    else if (p2 == "normalByUser")
                        session.Result.etts = repo.GetWHMASTByUser(p0, p1, p3, p4, page, limit, sorters);
                    else if (p2 == "WHpopwindow")   // for tab1 WH windowpopup 
                        session.Result.etts = repo.GetWHMASTByPopupWindow(p0, p1, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse DeleteM(FormDataCollection form)    // DELETE MI_WHID
        {
            var p0 = form.Get("p0"); 
            IEnumerable<MI_WHID> whids = JsonConvert.DeserializeObject<IEnumerable<MI_WHID>>(p0);

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0121Repository(DBWork);

                    foreach (MI_WHID whid in whids) {
                        if (whid.TASK_ID == null) {
                            whid.TASK_ID = string.Empty;
                        }
                        session.Result.afrs = repo.DeleteM(whid.WH_NO, whid.WH_USERID, whid.TASK_ID);
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
        public ApiResponse InsertM(FormDataCollection form)      // 上傳指定人員到 MI_WHID
        {
            var p0 = form.Get("p0");

            IEnumerable<MI_WHID> whids = JsonConvert.DeserializeObject<IEnumerable<MI_WHID>>(p0);

            using (WorkSession session = new WorkSession(this))            // 一定要加 this , 否則login user data 會抓不到
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0121Repository(DBWork);

                    foreach (MI_WHID whid in whids) {
                        if (repo.CheckUserTaskNullExists(whid.WH_NO, whid.WH_USERID))
                        {
                            session.Result.success = false;
                            session.Result.msg = string.Format("庫房代碼：{0} 人員代碼：{1} 尚有未設定作業類別之資料，請先設定", whid.WH_NO, whid.WH_USERID);
                            return session.Result;
                        }

                        session.Result.afrs = repo.InsertM(whid.WH_NO, whid.WH_USERID, DBWork.UserInfo.UserId, DBWork.ProcIP);
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
        public ApiResponse GetUsrCodeCombo(FormDataCollection form)
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
                    AA0121Repository repo = new AA0121Repository(DBWork);
                    session.Result.etts = repo.GetUsrCodeCombo(p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetUsrNameCombo(FormDataCollection form)
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
                    AA0121Repository repo = new AA0121Repository(DBWork);
                    session.Result.etts = repo.GetUsrNameCombo(p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }


        public ApiResponse GetTaskIdCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0121Repository(DBWork);
                    session.Result.etts = repo.GetTaskIdCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse ChkTuser(FormDataCollection form)
        {
            var p0 = form.Get("p0");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0121Repository(DBWork);
                    if (repo.getChkTuser(p0))
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

        [HttpPost]
        public ApiResponse ChkWhNo(FormDataCollection form)
        {
            var p0 = form.Get("p0");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0121Repository(DBWork);
                    if (repo.getChkWhNo(p0))
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

        //修改
        [HttpPost]
        public ApiResponse UpdateTaskId()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0121Repository repo = new AA0121Repository(DBWork);
                    HttpContent requestContent = Request.Content;
                    string jsonContent = requestContent.ReadAsStringAsync().Result;
                    //NM_CONTACT contact = JsonConvert.DeserializeObject<NM_CONTACT>(jsonContent);
                    JObject obj = JsonConvert.DeserializeObject<JObject>(jsonContent);          // 先解第一層 {"item":[{"id":24,"part_no":"12223"},{...}]}
                    JArray ja = JsonConvert.DeserializeObject<JArray>(obj["item"].ToString());  // 解第二層
                    MI_WHID mi_whid = JsonConvert.DeserializeObject<MI_WHID>(ja[0].ToString());
                    //me_docd.UPDATE_USER = User.Identity.Name;
                    //me_docd.UPDATE_IP = DBWork.ProcIP;
                    //if (repo.CheckTaskIdExist(mi_whid.TASK_ID) || mi_whid.TASK_ID == "")

                    if (repo.CheckUserTaskExists(mi_whid.WH_NO, mi_whid.WH_USERID, mi_whid.TASK_ID)) {
                        session.Result.success = false;
                        session.Result.msg = "作業類別已存在，請重新輸入";
                        return session.Result;
                    }

                    session.Result.afrs = repo.UpdateTaskId(mi_whid);
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
    }
}
