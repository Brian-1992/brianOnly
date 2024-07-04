using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using WebApp.Models;
using WebApp.Repository.AA;

namespace WebApp.Controllers.AA
{
    public class AA0188Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0188Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //匯出EXCEL
        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            var p0 = form.Get("MMCODE");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0188Repository repo = new AA0188Repository(DBWork);
                    JCLib.Excel.Export("寄售存放單位維護_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls", repo.GetExcel(p0));
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
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0188Repository repo = new AA0188Repository(DBWork);
                    session.Result.etts = repo.GetMmCodeCombo(p0, page, limit, "");
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetWhnoCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0188Repository(DBWork);
                    session.Result.etts = repo.GetWhnoCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // 新增
        [HttpPost]
        public ApiResponse Create(CN_RECORD cn_record)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0188Repository(DBWork);
                    if (!repo.CheckExists(cn_record.WH_NO, cn_record.MMCODE)) // 新增前檢查主鍵是否已存在
                    {
                        cn_record.CREATE_USER = User.Identity.Name;
                        cn_record.UPDATE_USER = User.Identity.Name;
                        cn_record.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.Create(cn_record);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>寄放單位及藥材代碼</span>重複，請重新輸入。";
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

        // 修改
        [HttpPost]
        public ApiResponse Update(CN_RECORD cn_record)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0188Repository(DBWork);
                    cn_record.UPDATE_USER = User.Identity.Name;
                    cn_record.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.Update(cn_record);
                    session.Result.etts = repo.Get(cn_record.WH_NO, cn_record.MMCODE);

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
        public ApiResponse Delete(CN_RECORD cn_record)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0188Repository(DBWork);
                    if (repo.CheckExists(cn_record.WH_NO, cn_record.MMCODE))
                    {
                        session.Result.afrs = repo.Delete(cn_record.WH_NO, cn_record.MMCODE);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>寄放單位及藥材代碼</span>已不存在，請重新查詢。";
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
        public ApiResponse calcAmtMsg(FormDataCollection form)
        {
            var mmcode = form.Get("MMCODE");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0188Repository(DBWork);
                    DataTable dt = repo.calcAmtMsg(mmcode);
                    string vSUM_QTY = $"{Convert.ToInt32(dt.Rows[0]["SUM_QTY"].ToString()):n0}"; // 處理千位的逗號
                    session.Result.success = true;
                    session.Result.msg = "寄售總量:" + vSUM_QTY;
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