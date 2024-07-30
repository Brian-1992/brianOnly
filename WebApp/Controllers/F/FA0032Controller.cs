using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.F;
using WebApp.Models;
using System;

namespace WebApp.Controllers.F
{
    public class FA0032Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var MAT_CLASS = form.Get("P0");
            var APPTIME_B = form.Get("P1");
            var APPTIME_E = form.Get("P2");
            var P3 = form.Get("P3");
            var P4 = form.Get("P4");
            var P5 = form.Get("P5");
            var P6 = form.Get("P6");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");


            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0032Repository(DBWork);
                    if (P3 == "1")
                    {
                        session.Result.etts = repo.GetAll_A(MAT_CLASS, APPTIME_B, APPTIME_E, P4, P5, P6, page, limit, sorters);
                    }
                    else
                    {
                        session.Result.etts = repo.GetAll_M(MAT_CLASS, APPTIME_B, APPTIME_E, P4, P5, P6, page, limit, sorters);

                    }

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetMATCombo()
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0032Repository(DBWork);
                    session.Result.etts = repo.GetMATCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //[HttpPost]
        //public ApiResponse GetMMCodeCombo(FormDataCollection form)
        //{
        //    var p0 = form.Get("p0");
        //    //var wh_no = form.Get("WH_NO");
        //    var page = int.Parse(form.Get("page"));
        //    var start = int.Parse(form.Get("start"));
        //    var limit = int.Parse(form.Get("limit"));
        //    var sorters = form.Get("sort");

        //    using (WorkSession session = new WorkSession())
        //    {
        //        UnitOfWork DBWork = session.UnitOfWork;
        //        try
        //        {
        //           FA0016Repository repo = new FA0016Repository(DBWork);
        //            session.Result.etts = repo.GetMMCodeCombo(p0, page, limit, "");
        //        }
        //        catch
        //        {
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //}

        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            var MAT_CLASS = form.Get("P0");
            var APPTIME_B = form.Get("P1");
            var APPTIME_E = form.Get("P2");
            var P3 = form.Get("P3");
            var P4 = form.Get("P4");  //庫備 非庫備
            var P5 = form.Get("P5");  //是否核撥
            var P6 = form.Get("P6");  //申請狀態
            var P7 = form.Get("P7");



            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0032Repository repo = new FA0032Repository(DBWork);
                    string dateTime = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MM");

                    if (P3 == "1")
                    {

                        JCLib.Excel.Export("單位申請品項統計" + "_" + DateTime.Now.ToString("yyyyMMddhhmm") + ".xls", repo.GetExcel_A(MAT_CLASS, APPTIME_B, APPTIME_E, P4, P5, P6),
                             (dt) =>
                             {
                                 if (P4 == "1") { P4 = "庫備品"; }
                                 else if (P4 == "0") { P4 = "非庫備"; }
                                 else { P4 = ""; }

                                 if (P5 == "1") { P5 = "已核撥"; }
                                 else { P5 = "未核撥"; }

                                 if (P6 == "1") { P6 = "正常申請"; }
                                 else if (P6 == "2") { P6 = "臨時申請"; }
                                 else { P6 = ""; }
                                 return string.Format("{0}{1}({2}{3}{4}){5}至{6}單位申請品項統計", DBWork.UserInfo.InidName, P7.Length > 2 ? P7.Substring(3) : "", P4, P6, P5, APPTIME_B, APPTIME_E);
                             });
                    }
                    else
                    {
                        JCLib.Excel.Export("品項申請量統計" + "_" + DateTime.Now.ToString("yyyyMMddhhmm") + ".xls", repo.GetExcel_M(MAT_CLASS, APPTIME_B, APPTIME_E, P4, P5, P6),
                             (dt) =>
                             {
                                 if (P4 == "1") { P4 = "庫備品"; }
                                 else if (P4 == "0") { P4 = "非庫備"; }
                                 else { P4 = ""; }
                        
                                 if (P5 == "1") { P5 = "已核撥"; }
                                 else { P5 = "未核撥"; }
                        
                                 if (P6 == "1") { P6 = "正常申請"; }
                                 else if (P6 == "2") { P6 = "臨時申請"; }
                                 else { P6 = ""; }
                                 return string.Format("{0}{1}({2}{3}{4}){5}至{6}品項申請量統計", DBWork.UserInfo.InidName, P7.Length > 2 ? P7.Substring(3) : "", P4, P6, P5, APPTIME_B, APPTIME_E);
                             });
                    }
                }//--{使用者單位v_user_deptname}+{物料分類名稱}+(庫備品/非庫備+正常申請/臨時申請+已核撥/已鎖單/未鎖單)+{開始申請日期}+-+{結束申請日期}+單位申請品項統計(依成本碼統計)
                 //{使用者單位v_user_deptname}+{物料分類名稱}+(庫備品/非庫備+正常申請/臨時申請+已核撥/已鎖單/未鎖單)+{開始申請日期}+-+{結束申請日期}+品項申請量統計(依院內碼統計)
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
    }
}