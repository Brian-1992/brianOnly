using JCLib.DB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using WebApp.Models;
using WebApp.Repository.C;

namespace WebApp.Controllers.C
{
    public class CE0029Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse CheckUserInChkuid(FormDataCollection form)
        {
            var chk_level = form.Get("chk_level"); 
            var chk_type = form.Get("chk_type");   
            var chk_class = form.Get("chk_class"); 
            
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0029Repository repo = new CE0029Repository(DBWork);

                    if (repo.CheckUserInChkuid(chk_level, chk_type, chk_class, DBWork.UserInfo.UserId) == false)
                    {
                        session.Result.msg = "您不須盤點此盤點單，請重新選擇";
                        
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
        public ApiResponse CheckPDADone(FormDataCollection form)
        {
            var chk_level = form.Get("chk_level");
            var chk_type = form.Get("chk_type");
            var chk_class = form.Get("chk_class");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0029Repository repo = new CE0029Repository(DBWork);

                    if (repo.CheckPDADone(chk_level, chk_type, chk_class, DBWork.UserInfo.UserId) == false)
                    {
                        session.Result.msg = "PDA尚未輸入完成，請先完成PDA輸入再使用此功能確認";

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
        public ApiResponse All(FormDataCollection form) {
            var p0 = form.Get("p0");    //chk_level
            var p1 = form.Get("p1");    //chk_type
            var p2 = form.Get("p2");    //chk_class
            var p3 = User.Identity.Name;
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession(this)) {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0029Repository repo = new CE0029Repository(DBWork);

                    session.Result.etts = repo.GetAll(p0, p1, p2, p3, page, limit, sorters);
                    
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse UpdateChkd(CHK_DETAIL chk_detail)
        {
            IEnumerable<CHK_DETAIL> chk_details = JsonConvert.DeserializeObject<IEnumerable<CHK_DETAIL>>(chk_detail.ITEM_STRING);
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    CE0029Repository repo = new CE0029Repository(DBWork);

                    foreach (CHK_DETAIL chkd in chk_details)
                    {
                        CHK_MAST mast = repo.GetChkMast(chkd.CHK_NO);
                        if (mast.CHK_STATUS != "1")
                        {
                            session.Result.success = false;
                            session.Result.msg = "盤點單狀態已變更，請重新查詢";
                            return session.Result;
                        }

                        chkd.UPDATE_USER = User.Identity.Name;
                        chkd.UPDATE_IP = DBWork.ProcIP;

                        session.Result.afrs = repo.UpdateChkd(chkd);
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

        public ApiResponse FinishChkd(FormDataCollection form)
        {
            var chk_no = form.Get("chk_no");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    CE0029Repository repo = new CE0029Repository(DBWork);

                    CHK_MAST mast = repo.GetChkMast(chk_no);
                    if (mast.CHK_STATUS != "1")
                    {
                        session.Result.success = false;
                        session.Result.msg = "盤點單狀態已變更，請重新查詢";
                        return session.Result;
                    }

                    if (repo.CheckHasNullChkqty(chk_no, DBWork.UserInfo.UserId)) {
                        session.Result.msg = "尚有未輸入盤點量的品項，請檢查";
                        session.Result.success = false;
                        return session.Result;
                    }

                    session.Result.afrs = repo.FinishChkDetail(chk_no, DBWork.UserInfo.UserId, DBWork.ProcIP);
                    session.Result.afrs = repo.FinishChkd2(chk_no, DBWork.UserInfo.UserId, DBWork.ProcIP);
                    var v_chk_num = repo.GetChkNum(chk_no);
                    var v_chk_tot = repo.GetChkTot(chk_no);
                    if (v_chk_num == v_chk_tot)
                    {
                        session.Result.afrs = repo.FinishChkd3(chk_no, DBWork.UserInfo.UserId, DBWork.ProcIP);
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


    }
}