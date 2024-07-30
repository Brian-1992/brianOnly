using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.F;
using WebApp.Models;
using System;
using System.Net.Http;
using WebApp.Repository.AB;
using System.Diagnostics;

namespace WebApp.Controllers.F
{
    public class FA0035Controller : SiteBase.BaseApiController
    {
        public ApiResponse GetMATCombo()
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0035Repository(DBWork);
                    session.Result.etts = repo.GetMATCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetMMcCombo(FormDataCollection form)
        {
            //var p0 = form.Get("p0"); //動態mmcode
            var mat_class = form.Get("mat_class");
            var store_id = form.Get("store_id");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0035Repository repo = new FA0035Repository(DBWork);
                    session.Result.etts = repo.GetMMCODECombo("",mat_class, store_id, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetAll(FormDataCollection form)
        {
            var p0 = form.Get("P0");
            var p1 = form.Get("P1");
            var p2 = form.Get("P2");
            var p3 = form.Get("P3");
            var p4 = form.Get("P4");
            var p5 = form.Get("P5");
    
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            //int v_from_ym;
            //int v_to_ym;
            //string v_ym=p1;
            ////v_ym =”'”+{開始申請年月}+”'”
            //v_from_ym = Convert.ToInt32(p1) + 1;
            //v_to_ym = Convert.ToInt32(p2);
            //for (var i = v_from_ym; i < v_to_ym; i++)
            //{
            //    if (p1.Substring(4,2) >'00' && p1.Substring(4, 2) < '13')
                
            //    {
            //        v_ym = v_ym + i;
            

            //}
            // for i = v_from_ym to v_to_vm
          



            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0035Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1, p2, p3, p4, page, limit, sorters);
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
            var p0 = form.Get("P0");
            var p1 = form.Get("P1");
            var p2 = form.Get("P2");
            var p3 = form.Get("P3");
            var p4 = form.Get("P4");
            var p5 = form.Get("P5");
            var p4n = "";
            string mclsname;
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0035Repository repo = new FA0035Repository(DBWork);
                    string str_UserDept = DBWork.UserInfo.InidName;
                    //title = str_UserDept + YM + "申領品項總材積明細.xls";
                    if (p4=="0")
                    {
                        p4n = "非庫備品";
                    }
                    if (p4 == "1")
                    {
                        p4n = "庫備品";
                    }
                    if (p4 == "2")
                    {
                        p4n = "所有品項";
                    }
                    if (p0!="")
                    {
                        mclsname = repo.MCLSNAME(p0);
                    }
                   else
                    {
                        mclsname = "";
                    }
                    //using (var dataTable1 = repo.GetExcel(YM))
                    //{
                    //    JCLib.Excel.Export(title, dataTable1);
                    //}
                    JCLib.Excel.Export(str_UserDept + mclsname + p5 + p2 + p4n + "總庫存量報表.xls", repo.GetExcel(p0, p1, p2, p3, p4),
                         (dt) =>
                         {
                             return string.Format(str_UserDept+ mclsname + p5 + "~" + p2 +p4n + "總庫存量報表", DBWork.UserInfo.InidName, "");
                         });
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