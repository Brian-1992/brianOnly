using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using JCLib.DB;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using WebApp.Repository.AA;
using WebApp.Repository.AB;
using WebApp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace WebApp.Controllers.AA
{
    public class AA0122Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0122Repository(DBWork);
                    AA0122Repository.MI_MAST_QUERY_PARAMS query = new AA0122Repository.MI_MAST_QUERY_PARAMS();
                    //if (form.Get("p1").Trim() != "")
                    //{
                    //    string tmp = form.Get("p1").Split('T')[0];  // yyyy-mm-ddT00:00:00
                    //    string[] tmp2 = tmp.Split('-');
                    //    string ym = Convert.ToString(Convert.ToInt32(tmp2[0]) - 1911) + tmp2[1];
                    //    query.DATA_YM = ym;
                    //}
                    //else
                    //    query.DATA_YM = "";

                    query.DATA_YM = form.Get("p1");
                    query.NotEqualOnly = (form.Get("p3") == "Y");
                    if (form.Get("p2").Trim() != "")
                        query.MAT_CLASS = form.Get("p2");
                    else
                    {
                        IEnumerable<COMBO_MODEL> myEnum = repo.GetMatClassCombo(User.Identity.Name);
                        myEnum.GetEnumerator();
                        int i = 0;
                        foreach (var item in myEnum)
                        {
                            if (i == 0)
                                query.MAT_CLASS += item.VALUE;
                            else
                                query.MAT_CLASS += "," + item.VALUE;
                            i++;
                        }
                    }

                    session.Result.etts = repo.GetAll(query, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse UpdateStatusBySP(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0122Repository repo = new AA0122Repository(DBWork);

                    string set_ym = repo.GetSetYm();
                    if (form.Get("YM") != set_ym) {
                        session.Result.success = false;
                        session.Result.msg = "非目前開帳年月，不可異動單價";
                        return session.Result;
                    }

                    SP_MODEL sp = repo.UpdMcost(form.Get("YM"), form.Get("MATCLASS"), User.Identity.Name, DBWork.ProcIP);
                    if (sp.O_RETID == "N")
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = sp.O_ERRMSG;
                        return session.Result;
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
        public ApiResponse GetMatClassCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0122Repository repo = new AA0122Repository(DBWork);
                    session.Result.etts = repo.GetMatClassCombo(User.Identity.Name);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public HttpResponseMessage GetYmCombo()
        {
            string rtn = "";
            string thismonth = DateTime.Now.Month.ToString();

            string ym1 = GetYm(true);
            string ym2 = GetYm(false);

            List<ComboItem> aa = new List<ComboItem>()
            {
                new ComboItem() {TEXT = ym1, VALUE = ym1},
                new ComboItem() {TEXT = ym2, VALUE = ym2}
            };

            rtn = JsonConvert.SerializeObject(aa);

            // 去掉反斜線, json就只剩下雙引號
            HttpResponseMessage result = new HttpResponseMessage { Content = new StringContent(rtn, System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
            return result;
        }

        protected string GetYm(bool isLast)
        {
            string rtn = "", year = "";
            year = Convert.ToString(Convert.ToInt32(DateTime.Now.Year) - 1911);
            int i = 0;
            if (isLast)
            {
                i = DateTime.Now.AddMonths(-1).Month;
                if (i == 12)
                    year = Convert.ToString(Convert.ToInt32(DateTime.Now.AddYears(-1).Year) - 1911);
            }
            else
                i = DateTime.Now.Month;

            if (i < 10)
                rtn = year + "0" + i.ToString();
            else
                rtn = year + i.ToString();

            return rtn;
        }

        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            //var p0 = form.Get("P0");
            //var p1 = form.Get("P1").Trim();
            //var p1_name = form.Get("P1_Name").Trim();
            //var p2 = form.Get("P2").Trim();

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0122Repository repo = new AA0122Repository(DBWork);
                    AA0122Repository.MI_MAST_QUERY_PARAMS query = new AA0122Repository.MI_MAST_QUERY_PARAMS();
                    DataTable result = new DataTable();
                    DataTable dtItems = new DataTable();

                    query.DATA_YM = form.Get("p1");
                    query.NotEqualOnly = (form.Get("p3") == "Y");
                    if (form.Get("p2").Trim() != "")
                        query.MAT_CLASS = form.Get("p2");
                    else
                    {
                        IEnumerable<COMBO_MODEL> myEnum = repo.GetMatClassCombo(User.Identity.Name);
                        myEnum.GetEnumerator();
                        int i = 0;
                        foreach (var item in myEnum)
                        {
                            if (i == 0)
                                query.MAT_CLASS += item.VALUE;
                            else
                                query.MAT_CLASS += "," + item.VALUE;
                            i++;
                        }
                    }

                    result = repo.GetExcel(query);
                    dtItems.Merge(result);

                    //string str_UserDept = DBWork.UserInfo.InidName;
                    //string export_FileName = str_UserDept + p1_name + "儲位基本資料表";
                    string export_FileName = "戰備調帳單價資料表(" + query.DATA_YM + ")";
                    JCLib.Excel.Export(export_FileName + ".xls", dtItems, (tmp_dt) => { return export_FileName; });
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        protected class ComboItem
        {
            public string TEXT { get; set; }
            public string VALUE { get; set; }
        }
    }
}
