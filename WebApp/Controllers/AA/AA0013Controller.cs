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
    public class AA0013Controller : SiteBase.BaseApiController
    {
        /// <summary>
        /// 取得請單列印資料
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        /// // 查詢
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
        {
            var p0 = form.Get("p0");    //申請單位
            var p1 = form.Get("p1");    //物料分類
            var p2 = form.Get("p2");    //申請日期(起)
            var p3 = form.Get("p3");    //申請日期(迄)
            var p4 = form.Get("p4");    //申請單狀態
            var p5 = form.Get("p5");    //申請單類別
            var p6 = form.Get("p6");    //核撥日期(起)
            var p7 = form.Get("p7");    //核撥日期(迄)
            bool isGas = form.Get("isGas") == "Y"; //是否為氣體
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            
            string[] arr_p0 = { };
            string[] arr_p1 = { };
            string[] arr_p4 = { };
            string[] arr_p5 = { };
            if (!string.IsNullOrEmpty(p0))
            {
                arr_p0 = p0.Trim().Split(','); //用,分割
            }
            if (!string.IsNullOrEmpty(p1))
            {
                arr_p1 = p1.Trim().Split(','); //用,分割
            }
            if (!string.IsNullOrEmpty(p4))
            {
                arr_p4 = p4.Trim().Split(','); //用,分割
            }
            if (!string.IsNullOrEmpty(p5))
            {
                arr_p5 = p5.Trim().Split(','); //用,分割
            }

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0013Repository repo = new AA0013Repository(DBWork);

                    session.Result.etts = repo.GetAllM(isGas, arr_p0, arr_p1, p2, p3, arr_p4, arr_p5, p6, p7, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse SearchPrintData(FormDataCollection form)
        {
            var p0 = form.Get("p0");    //申請單位
            var p1 = form.Get("p1");    //物料分類
            var p2 = form.Get("p2");    //申請日期(起)
            var p3 = form.Get("p3");    //申請日期(迄)
            var p4 = form.Get("p4");    //申請單狀態
            var p5 = form.Get("p5");    //申請單類別
            var p6 = form.Get("p6");    //核撥日期(起)
            var p7 = form.Get("p7");    //核撥日期(迄)
            bool isGas = form.Get("isGas") == "Y"; //是否為氣體
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            string[] arr_p0 = { };
            string[] arr_p1 = { };
            string[] arr_p4 = { };
            string[] arr_p5 = { };
            if (!string.IsNullOrEmpty(p0))
            {
                arr_p0 = p0.Trim().Split(','); //用,分割
            }
            if (!string.IsNullOrEmpty(p1))
            {
                arr_p1 = p1.Trim().Split(','); //用,分割
            }
            if (!string.IsNullOrEmpty(p4))
            {
                arr_p4 = p4.Trim().Split(','); //用,分割
            }
            if (!string.IsNullOrEmpty(p5))
            {
                arr_p5 = p5.Trim().Split(','); //用,分割
            }

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0013Repository repo = new AA0013Repository(DBWork);

                    session.Result.etts = repo.SearchPrintData(isGas, arr_p0, arr_p1, p2, p3, arr_p4, arr_p5, p6, p7, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        /// <summary>
        /// 讀取申請單號
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ApiResponse GetDocnoCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p4 = form.Get("p4");
            bool isGas = form.Get("isGas") == "Y"; //是否為氣體
            string[] arr_p0 = { };
            string[] arr_p1 = { };
            string[] arr_p4 = { };
            if (!string.IsNullOrEmpty(p0))
            {
                arr_p0 = p0.Trim().Split(','); //用,分割
            }
            if (!string.IsNullOrEmpty(p1))
            {
                arr_p1 = p1.Trim().Split(','); //用,分割
            }
            if (!string.IsNullOrEmpty(p4))
            {
                arr_p4 = p4.Trim().Split(','); //用,分割
            }

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0013Repository repo = new AA0013Repository(DBWork);

                    session.Result.etts = repo.GetDocnoCombo(isGas, arr_p0, arr_p1, arr_p4);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        /// <summary>
        /// 讀取申請單狀態
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ApiResponse GetFlowidCombo(FormDataCollection form)
        {
            bool isGas = form.Get("isGas") == "Y"; //是否為氣體

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0013Repository repo = new AA0013Repository(DBWork);
                    session.Result.etts = repo.GetFlowidCombo(isGas);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        /// <summary>
        /// 讀取物料分類
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ApiResponse GetMatclassCombo(FormDataCollection form)
        {
            bool isGas = form.Get("isGas") == "Y"; //是否為氣體

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0013Repository repo = new AA0013Repository(DBWork);
                    string TASK_ID = repo.GetTASK_ID(User.Identity.Name);
                    List<string> taskids = repo.GetTaskids(DBWork.UserInfo.UserId).ToList<string>();

                    // 不是衛材 一般物品 氣體 不給用
                    if (taskids.Any() == false)
                    {
                        session.Result.success = false;
                        return session.Result;
                    }
                    taskids = taskids.Where(x => x != null).ToList();

                    bool IsUserWHNO_Equal_WHNOMM1 = repo.GetIsUserWHNO_Equal_WHNOMM1(User.Identity.Name);
                    session.Result.etts = repo.GetMatclassCombo(isGas);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        /// <summary>
        /// 讀取申請單位
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ApiResponse GetAppDeptCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0013Repository repo = new AA0013Repository(DBWork);
                    var v_USER_WHNO = repo.GetUSER_WHNO(User.Identity.Name);
                    session.Result.etts = repo.GetAppDeptCombo(v_USER_WHNO);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        /// <summary>
        /// 讀取申請單類別
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ApiResponse GetDocType(FormDataCollection form)
        {
            bool isGas = form.Get("isGas") == "Y"; //是否為氣體
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0013Repository repo = new AA0013Repository(DBWork);
                    session.Result.etts = repo.GetDocType(isGas);
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