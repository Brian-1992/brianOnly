using System;
using System.Collections.Generic;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using WebApp.Repository.AA;
using WebApp.Models.AA;
using JCLib.DB;

namespace WebApp.Controllers.AA
{
    public class AA0181Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var user = User.Identity.Name;
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0181Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse MasterCreate(AA0181 input)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0181Repository repo = new AA0181Repository(DBWork);
                    input.CREATE_USER = User.Identity.Name;
                    input.UPDATE_USER = User.Identity.Name;
                    input.UPDATE_IP = DBWork.ProcIP;

                    input.DATA_YM = repo.GetCurrentDataYm();

                    if (repo.CheckDateYm(input.DATA_YM))
                    {
                        if (repo.CheckRcrateExists(input.DATA_YM, input.CASENO) == false)
                        {
                            float x;
                            if (float.TryParse(input.JBID_RCRATE, out x))
                                session.Result.msg = repo.MasterCreate(input);
                            else
                                session.Result.msg = "管理費只限於輸入數字或小數點";
                        }
                        else
                            session.Result.msg = "合約案號已存在,請檢核!";
                    }
                    else
                        session.Result.msg = "查詢月份格式有誤";

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
        public ApiResponse MasterUpdate(AA0181 input)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0181Repository repo = new AA0181Repository(DBWork);
                    input.UPDATE_USER = User.Identity.Name;
                    input.UPDATE_IP = DBWork.ProcIP;

                    input.DATA_YM = repo.GetCurrentDataYm();

                    if (repo.CheckDateYm(input.DATA_YM))
                    {
                        float x;
                        if (float.TryParse(input.JBID_RCRATE, out x))
                            session.Result.afrs = repo.MasterUpdate(input);
                        else
                            session.Result.msg = "管理費只限於輸入數字或小數點";
                    }
                    else
                        session.Result.msg = "查詢月份格式有誤";

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
        public ApiResponse MasterDelete(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0181Repository repo = new AA0181Repository(DBWork);
                    string data_ym = form.Get("DATA_YM");
                    string user = DBWork.UserInfo.UserId;

                    if (form.Get("CASENO") != "")
                    {
                        string caseno = form.Get("CASENO").Substring(0, form.Get("CASENO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = caseno.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            int tmp_afrs = repo.InsertRCRATE_DEL(tmp[i], data_ym, user);
                            session.Result.afrs = repo.MasterDelete(tmp[i], data_ym);
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

        //匯出EXCEL
        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0181Repository repo = new AA0181Repository(DBWork);
                    JCLib.Excel.Export("AA0181合約優惠設定作業" + p0 + ".xls", repo.GetExcel(p0, p1));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse getDataYm(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0181Repository(DBWork);
                    session.Result.success = true;
                    session.Result.msg = repo.GetCurrentDataYm();
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