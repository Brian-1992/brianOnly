using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.C;
using WebApp.Repository.AB;
using WebApp.Models.AB;
using Newtonsoft.Json;

using System.Data;

namespace WebApp.Controllers.C
{
    public class AB0046Controller : SiteBase.BaseApiController
    {
        // 新增
        [HttpPost]
        public ApiResponse Create(AB0046 ab0046)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0046Repository(DBWork);

                    var maxFreqNo = "0";
                    

                    if (repo.getMaxFreqNo() != null)
                    {
                        maxFreqNo = repo.getMaxFreqNo().ToString();
                        maxFreqNo = (int.Parse(maxFreqNo) + 1).ToString(); 
                    }

                    if (!repo.CheckExists(ab0046.VISITKIND, ab0046.LOCATION, maxFreqNo)) // 新增前檢查主鍵是否已存在 
                    {
                        ab0046.CREATEOPID = User.Identity.Name;
                        ab0046.PROCOPID = User.Identity.Name;
                        ab0046.FREQNO = maxFreqNo;
                        session.Result.afrs = repo.Create(ab0046);          //問 session.Result.afrs 
                        session.Result.etts = repo.Get(ab0046.VISITKIND, ab0046.LOCATION, ab0046.FREQNO);      //搜尋一筆，且在ext上loadStore
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>門住診別/動向碼/院內瀕率</span>重複，請重新輸入。";
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

        public ApiResponse Update(AB0046 ab0046)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0046Repository(DBWork);
                    ab0046.CREATEOPID = User.Identity.Name;
                    ab0046.PROCOPID = User.Identity.Name;
                    session.Result.afrs = repo.Update(ab0046);
                    session.Result.etts = repo.Get(ab0046.VISITKIND, ab0046.LOCATION, ab0046.FREQNO); //可讓前端收到資料後關閉mask

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
        public ApiResponse Delete(AB0046 ab0046)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0046Repository(DBWork);
                    //if (repo.CheckExists(ab0046.VISITKIND, ab0046.LOCATION))
                    //{
                        // session.Result.afrs = repo.Delete(ab0046.VISITKIND, ab0046.LOCATION, ab0046.FREQNO);
                        session.Result.afrs = repo.Delete(ab0046);
                    //}
                    //else
                    //{
                    //    session.Result.afrs = 0;
                    //    session.Result.success = false;
                    //    session.Result.msg = "<span style='color:red'>庫房代碼與院內碼</span>不存在。";
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

        public ApiResponse All(FormDataCollection form)   //SELECT * FROM UR_ID where INID tab1   
        {
            var p0 = form.Get("p0");   // 門住別
            var p1 = form.Get("p1");   // 診間/病房代碼

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0046Repository repo = new AB0046Repository(DBWork);
                    session.Result.etts = repo.GetAB0046Detail(p0, p1, page, limit, sorters); 
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        } 

        [HttpPost]
        public ApiResponse VisitKindCombo(FormDataCollection form)  // 門住別
        {
            var p0 = form.Get("p0");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0046Repository repo = new AB0046Repository(DBWork);
                    session.Result.etts = repo.GetVisitKindCombo(p0, page, limit, ""); 
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse LocationCombo(FormDataCollection form)    // 診間/病房代碼
        {
            //var visitkind = form.Get("visitkind");
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
                    AB0046Repository repo = new AB0046Repository(DBWork);
                    session.Result.etts = repo.GetLocationCombo(p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse CommonCombo(FormDataCollection form)    // 診間/病房代碼
        {
            //var visitkind = form.Get("visitkind");
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
                    AB0046Repository repo = new AB0046Repository(DBWork);
                    session.Result.etts = repo.GetCommonCombo(p0, page, limit, ""); 
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
