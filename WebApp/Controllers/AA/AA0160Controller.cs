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
    public class AA0160Controller : SiteBase.BaseApiController
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
                    var repo = new AA0160Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetColumns(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0160Repository(DBWork);

                    List<ColumnItem> columns = new List<ColumnItem>();
                    foreach (ColumnItem item in repo.GetColumnItems())
                    {
                        columns.Add(new ColumnItem { TEXT= item.TEXT, DATAINDEX = item.DATAINDEX });
                    }

                    session.Result.etts = columns;

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
        public ApiResponse GetYmCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    // 年月選項包含:
                    // 1.依MI_MNSET內SET_STATUS為N的月份及其上個月份
                    // 2.SEC_CALLOC已有資料的DATA_YM
                    AA0160Repository repo = new AA0160Repository(DBWork);
                    session.Result.etts = repo.GetYmCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetEditable(FormDataCollection form)
        {
            var data_ym = form.Get("DATA_YM");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0160Repository repo = new AA0160Repository(DBWork);
                    session.Result.msg = repo.GetEditable(data_ym);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse UpdateDisratio(AA0160 aa0160)
        {
            Dictionary<string, string>[] aa0160s = JsonConvert.DeserializeObject<Dictionary<string, string>[]>(aa0160.EXTRA_DATA);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0160Repository repo = new AA0160Repository(DBWork);

                    foreach (Dictionary<string, string> rowdata in aa0160s)
                    {
                        List<string> columnNames = new List<string>();

                        if (repo.GetEditable(rowdata["DATA_YM"]) == "N")
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = rowdata["DATA_YM"] + "不是可編輯資料的月份";
                            DBWork.Rollback();
                            return session.Result;
                        }

                        foreach (string key in rowdata.Keys)
                        {
                            // 排除科別代碼以外的欄位
                            if (key != "DATA_YM" && key != "INID" && key != "INID_NAME" 
                                && key != "undefined" && key != "EXTRA_DATA" && key != "id")
                                columnNames.Add(key);
                        }

                        float ratioSum = 0;
                        foreach (string key in columnNames)
                        {
                            if (rowdata[key] == "" || rowdata[key] == null)
                                rowdata[key] = "0";
                            ratioSum += Convert.ToSingle(rowdata[key]);
                            // 依每個欄位對應的值更新資料
                            session.Result.afrs += repo.MergeDisratio(rowdata["DATA_YM"], rowdata["INID"], key, rowdata[key], User.Identity.Name, DBWork.ProcIP);
                        }

                        // 單一責任中心各比率總和需為100
                        if (ratioSum != 100 && ratioSum != 0)
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = rowdata["INID_NAME"] + " 科室各分攤比率總和需為100";
                            DBWork.Rollback();
                            return session.Result;
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
    }
}