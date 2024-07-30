using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApp.Controllers.AA
{
    public class AA0102Controller : SiteBase.BaseApiController
    {
        // AA0102 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var d0 = form.Get("d0");
            var showdata = form.Get("showdata");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));  //start:0
            var limit = int.Parse(form.Get("limit"));  //pageSize=20
            var sorters = form.Get("sort");
            var Trshowdata = "";
            var newd0 = Convert.ToString(Int64.Parse(d0.Substring(0, 4)) - 1911) + d0.Substring(5, 2);

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0102Repository(DBWork);
                    if(showdata == "1")
                    {
                        Trshowdata = "A";
                    }
                    else if(showdata == "2")
                    {
                        Trshowdata = "B";
                    }
                    else if (showdata == "3")
                    {
                        Trshowdata = "C";
                    }
                    else if (showdata == "4")
                    {
                        Trshowdata = "D";
                    }
                    IEnumerable<AA0102_MODEL> list = repo.GetAll(newd0, Trshowdata, page, limit, sorters); //撈出object
                    foreach (AA0102_MODEL item in list) {
                        if (item.CHK_LEVEL == "0") {
                            continue;
                        }
                        item.CHK_TYPE_NAME = repo.GetChkWhkindName(item.WH_KIND, item.CHK_TYPE);
                    }

                    session.Result.etts = list;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // AA0103 查詢
        [HttpPost]
        public ApiResponse All_AA0103(FormDataCollection form)
        {
            var d0 = form.Get("d0");
            var showdata = form.Get("showdata");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));  //start:0
            var limit = int.Parse(form.Get("limit"));  //pageSize=20
            var sorters = form.Get("sort");
            var Trshowdata = "";
            var newd0 = Convert.ToString(Int64.Parse(d0.Substring(0, 4)) - 1911) + d0.Substring(5, 2);

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0102Repository(DBWork);
                    if (showdata == "1")
                    {
                        Trshowdata = "A";
                    }
                    else if (showdata == "2")
                    {
                        Trshowdata = "B";
                    }
                    else if (showdata == "3")
                    {
                        Trshowdata = "C";
                    }
                    else if (showdata == "4")
                    {
                        Trshowdata = "D";
                    }
                    //session.Result.etts = repo.GetAll_AA0103(newd0, Trshowdata, page, limit, sorters); //撈出object
                    IEnumerable<AA0102_MODEL> list = repo.GetAll_AA0103(newd0, Trshowdata, page, limit, sorters); //撈出object
                    foreach (AA0102_MODEL item in list)
                    {
                        if (item.CHK_LEVEL == "0")
                        {
                            continue;
                        }
                        item.CHK_TYPE_NAME = repo.GetChkWhkindName(item.WH_KIND, item.CHK_TYPE);
                    }
                    session.Result.etts = list;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // FA0043 查詢
        [HttpPost]
        public ApiResponse All_FA0043(FormDataCollection form)
        {
            var d0 = form.Get("d0");
            var showdata = form.Get("showdata");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));  //start:0
            var limit = int.Parse(form.Get("limit"));  //pageSize=20
            var sorters = form.Get("sort");
            var Trshowdata = "";

            var newd0 =  Convert.ToString(Int64.Parse(d0.Substring(0, 4)) - 1911) + d0.Substring(5, 2);

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0102Repository(DBWork);
                    if (showdata == "1")
                    {
                        Trshowdata = "A";
                    }
                    else if (showdata == "2")
                    {
                        Trshowdata = "B";
                    }
                    else if (showdata == "3")
                    {
                        Trshowdata = "C";
                    }
                    else if (showdata == "4")
                    {
                        Trshowdata = "D";
                    }
                    //session.Result.etts = repo.GetAll_FA0043(newd0, Trshowdata, page, limit, sorters); //撈出object
                    IEnumerable<AA0102_MODEL> list = repo.GetAll_FA0043(newd0, Trshowdata, page, limit, sorters); //撈出object
                    foreach (AA0102_MODEL item in list)
                    {
                        if (item.CHK_LEVEL == "0")
                        {
                            continue;
                        }
                        item.CHK_TYPE_NAME = repo.GetChkWhkindName(item.WH_KIND, item.CHK_TYPE);
                    }
                    session.Result.etts = list;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            var d0 = form.Get("d0");
            var showdata = form.Get("showdata");
            var Trshowdata = "";

            if (showdata == "1")
            {
                Trshowdata = "A";
            }
            else if (showdata == "2")
            {
                Trshowdata = "B";
            }
            else if (showdata == "3")
            {
                Trshowdata = "C";
            }
            else if (showdata == "4")
            {
                Trshowdata = "D";
            }

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0102Repository repo = new AA0102Repository(DBWork);
                    JCLib.Excel.Export(form.Get("FN")+"_"+DateTime.Now.ToString("yyyyMMddhhmm")+".xls", repo.GetExcel(d0, Trshowdata));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // AA0103 Excel
        [HttpPost]
        public ApiResponse Excel_AA0103(FormDataCollection form)
        {
            var d0 = form.Get("d0");
            var showdata = form.Get("showdata");
            var Trshowdata = "";

            if (showdata == "1")
            {
                Trshowdata = "A";
            }
            else if (showdata == "2")
            {
                Trshowdata = "B";
            }
            else if (showdata == "3")
            {
                Trshowdata = "C";
            }
            else if (showdata == "4")
            {
                Trshowdata = "D";
            }

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0102Repository repo = new AA0102Repository(DBWork);
                    JCLib.Excel.Export(form.Get("FN") + "_" + DateTime.Now.ToString("yyyyMMddhhmm") + ".xls", repo.GetExcel_AA0103(d0, Trshowdata));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // FA0043 Excel
        [HttpPost]
        public ApiResponse Excel_FA0043(FormDataCollection form)
        {
            var d0 = form.Get("d0");
            var showdata = form.Get("showdata");
            var Trshowdata = "";

            if (showdata == "1")
            {
                Trshowdata = "A";
            }
            else if (showdata == "2")
            {
                Trshowdata = "B";
            }
            else if (showdata == "3")
            {
                Trshowdata = "C";
            }
            else if (showdata == "4")
            {
                Trshowdata = "D";
            }

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0102Repository repo = new AA0102Repository(DBWork);
                    JCLib.Excel.Export(form.Get("FN") + "_" + DateTime.Now.ToString("yyyyMMddhhmm") + ".xls", repo.GetExcel_FA0043(d0, Trshowdata));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }


        [HttpPost]
        public ApiResponse GetRadio1(FormDataCollection form)
        {

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0102Repository repo = new AA0102Repository(DBWork);
                    session.Result.msg = repo.GetRadio1();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetRadio2(FormDataCollection form)
        {

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0102Repository repo = new AA0102Repository(DBWork);
                    session.Result.msg = repo.GetRadio2();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetRadio3(FormDataCollection form)
        {

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0102Repository repo = new AA0102Repository(DBWork);
                    session.Result.msg = repo.GetRadio3();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetRadio4(FormDataCollection form)
        {

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0102Repository repo = new AA0102Repository(DBWork);
                    session.Result.msg = repo.
GetRadio4();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }



        #region 2021-10-20 檢查開單後是否有點收品項或有在途量但不再盤點單內
        [HttpPost]
        public ApiResponse GetChkNotExists(FormatException form) {
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0102Repository(DBWork);
                    List<AA0102_MODEL> list = repo.GetChkNotExists().ToList<AA0102_MODEL>();
                    list = TranNotExistList(list);
                    foreach (AA0102_MODEL item in list) {
                        string mmcodes = string.Empty;
                        if (item.CHK_TYPE == "3")
                        {
                            item.CHK_TYPE_NAME = "小採";
                        }
                        if (item.CHK_TYPE == "0")
                        {
                            item.CHK_TYPE_NAME = "非庫備";
                        }
                        if (item.CHK_TYPE == "1")
                        {
                            item.CHK_TYPE_NAME = "庫備";
                        }
                        foreach (AA0102_MODEL temp in item.MMCODE_LIST) {
                            if (mmcodes != string.Empty) {
                                mmcodes += "、";
                            }
                            mmcodes += temp.MMCODE;
                        }
                        item.MMCODE_STRING = mmcodes;
                    }

                    session.Result.etts = list;
                    return session.Result;
                }
                catch (Exception e) {
                    throw;
                }
            }
        }

        public List<AA0102_MODEL> TranNotExistList(IEnumerable<AA0102_MODEL> list) {
            var o = from a in list
                    group a by new {
                        INID = a.INID,
                        INID_NAME = a.INID_NAME,
                        WH_NO = a.WH_NO,
                        WH_NAME = a.WH_NAME,
                        CHK_TYPE = (a.M_CONTID == "3" ? "3" : (a.M_STOREID))
                    } into g
                    select new AA0102_MODEL
                    {
                        INID = g.Key.INID,
                        INID_NAME = g.Key.INID_NAME,
                        WH_NO = g.Key.WH_NO,
                        WH_NAME = g.Key.WH_NAME,
                        CHK_TYPE = g.Key.CHK_TYPE,
                        MMCODE_COUNT = g.Count().ToString(),
                        MMCODE_LIST = g.ToList()
                    };
            return o.ToList();

        }

        #endregion
    }
}