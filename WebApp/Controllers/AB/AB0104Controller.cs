using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using WebApp.Models;
using WebApp.Repository.AB;

namespace WebApp.Controllers.AB
{
    public class AB0104Controller : SiteBase.BaseApiController
    {

        public ApiResponse All(FormDataCollection form) {
            string towh = form.Get("towh");
            string frwh = form.Get("frwh");
            string start_date = form.Get("start_date");
            string end_date = form.Get("end_date");
            string docno = form.Get("docno");
            string flowid = form.Get("flowid") == null ? string.Empty : form.Get("flowid");
            string mmcode = form.Get("mmcode");
            bool is_ab = form.Get("is_ab") == "Y";

            using (WorkSession session = new WorkSession(this)) {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0104Repository repo = new AB0104Repository(DBWork);
                    session.Result.etts = repo.GetAll(towh, frwh, start_date, end_date, docno, flowid, mmcode, is_ab);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        #region combo
        [HttpGet]
        public ApiResponse GetTowhCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0104Repository repo = new AB0104Repository(DBWork);
                    session.Result.etts = repo.GetTowhCombo(DBWork.UserInfo.UserId);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpGet]
        public ApiResponse GetFrwhCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0104Repository repo = new AB0104Repository(DBWork);
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
        public ApiResponse GetMMCodeCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var wh_no = form.Get("wh_no");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0104Repository repo = new AB0104Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeCombo(p0, wh_no, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpGet]
        public ApiResponse GetFlowidCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0104Repository repo = new AB0104Repository(DBWork);
                    session.Result.etts = repo.GetFlowidCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        #endregion


        //匯出EXCEL
        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            string towh = form.Get("towh");
            string frwh = form.Get("frwh");
            string start_date = form.Get("start_date");
            string end_date = form.Get("end_date");
            string docno = form.Get("docno");
            string flowid = form.Get("flowid") == null ? string.Empty : form.Get("flowid");
            string mmcode = form.Get("mmcode");
            bool is_ab = form.Get("is_ab") == "Y";
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0104Repository repo = new AB0104Repository(DBWork);
                    JCLib.Excel.Export(form.Get("FN"), repo.GetExcel(towh, frwh, start_date, end_date, docno, flowid, mmcode, is_ab));
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