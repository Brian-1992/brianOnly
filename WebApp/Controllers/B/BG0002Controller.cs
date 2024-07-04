using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using JCLib.DB.Tool;
using WebApp.Repository.BG;
using WebApp.Models;
using System;
using System.Collections.Generic;


namespace WebApp.Controllers.BG
{
    public class BG0002Controller : SiteBase.BaseApiController
    {
        //FL fl = new FL("WebApp.Controllers.BG.BG0002Controller");

        [HttpPost]
        public ApiResponse GetAll(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BG0002Repository(DBWork);
                    BG0002 v = new BG0002();
                    v.WH_NO = form.Get("WH_NO");    // 庫房代碼
                    
                    DateTime data_ym_start; // 查詢年月 開始 10807
                    if (DateTime.TryParse(form.Get("DATA_YM_START"), out data_ym_start))
                    {
                        v.DATA_YM_START = data_ym_start.ToString("yyyy/MM/01");
                    }
                    DateTime data_ym_end; // 查詢年月 開始 10807
                    if (DateTime.TryParse(form.Get("DATA_YM_END"), out data_ym_end))
                    {
                        v.DATA_YM_END = data_ym_end.ToString("yyyy/MM/" + DateTime.DaysInMonth(data_ym_end.Year, data_ym_end.Month));
                    }
                    //v.M_CONTID = form.Get("M_CONTID");    // 合約識別碼
                    v.MAT_CLASS = form.Get("MAT_CLASS");    // 物料分類
                    v.RADIO_BUTTON = form.Get("RADIO_BUTTON");    // 管控項目 0-庫備品、1-非庫備品(排除鎖E品項) 、2-庫備品(管控項目)
                    var page = int.Parse(form.Get("page"));
                    var start = int.Parse(form.Get("start"));
                    var limit = int.Parse(form.Get("limit"));
                    var sorters = form.Get("sort");

                    IEnumerable<WebApp.Models.BG0002> lst = (IEnumerable<WebApp.Models.BG0002>)repo.GetAll(v, page, limit, sorters);
                    
                    int i = 1;
                    foreach (WebApp.Models.BG0002 eV in lst)
                    {
                        eV.ROWNUMBERER = i.ToString();
                        i++;
                    }
                    session.Result.etts = lst;
                }
                catch (Exception ex)
                {
                    //fl.le("getAll()", ex.Message);
                    throw;
                }
                return session.Result;
            }
        }

        // 查庫房別
        public ApiResponse GetWhnoCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BG0002Repository(DBWork);
                    session.Result.etts = repo.GetWhnoCombo(User.Identity.Name);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        } // 

        // 查物料分類
        public ApiResponse GetMatClassCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BG0002Repository(DBWork);
                    session.Result.etts = repo.GetMatClassCombo(User.Identity.Name);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        } // 

        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BG0002Repository(DBWork);
                    BG0002 v = new BG0002();
                    v.WH_NO = form.Get("WH_NO");    // 庫房代碼
                    DateTime data_ym_start; // 查詢年月 開始 10807
                    if (DateTime.TryParse(form.Get("DATA_YM_START"), out data_ym_start))
                    {
                        v.DATA_YM_START = data_ym_start.ToString("yyyy/MM/01");
                    }
                    DateTime data_ym_end; // 查詢年月 開始 10807
                    if (DateTime.TryParse(form.Get("DATA_YM_END"), out data_ym_end))
                    {
                        v.DATA_YM_END = data_ym_end.ToString("yyyy/MM/" + DateTime.DaysInMonth(data_ym_end.Year, data_ym_end.Month));
                    }
                    v.MAT_CLASS = form.Get("MAT_CLASS");    // 物料分類
                    v.RADIO_BUTTON = form.Get("RADIO_BUTTON");    // 管控項目 0-庫備品、1-非庫備品(排除鎖E品項) 、2-庫備品(管控項目)
                    JCLib.Excel.Export(
                        "衛材非合約累計進貨金額查詢_" + DateTime.Now.ToString("yyyyMMddhhmm") + ".xls",
                        repo.GetExcel(v));
                }
                catch (Exception ex)
                {
                    //fl.le("Excel()", ex.Message);
                    throw;
                }
                return session.Result;
            }
        }

        //[HttpPost]
        //public ApiResponse Get(FormDataCollection form)
        //{
        //    var p0 = form.Get("p0");

        //    using (WorkSession session = new WorkSession())
        //    {
        //        var DBWork = session.UnitOfWork;
        //        try
        //        {
        //            var repo = new BG0002Repository(DBWork);
        //            session.Result.etts = repo.Get(p0);
        //        }
        //        catch
        //        {
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //}

        //// 新增
        //[HttpPost]
        //public ApiResponse Create(MI_MAST mi_mast)
        //{
        //    using (WorkSession session = new WorkSession(this))
        //    {
        //        var DBWork = session.UnitOfWork;
        //        DBWork.BeginTransaction();
        //        try
        //        {
        //            var repo = new BG0002Repository(DBWork);
        //            if (!repo.CheckExists(mi_mast.MMCODE))
        //            {
        //                mi_mast.CREATE_USER = User.Identity.Name;
        //                mi_mast.UPDATE_USER = User.Identity.Name;
        //                mi_mast.UPDATE_IP = DBWork.ProcIP;
        //                session.Result.afrs = repo.Create(mi_mast);
        //            }
        //            else
        //            {
        //                session.Result.afrs = 0;
        //                session.Result.success = false;
        //                session.Result.msg = "<span style='color:red'>院內碼</span>重複，請重新輸入。";
        //            }

        //            DBWork.Commit();
        //        }
        //        catch
        //        {
        //            DBWork.Rollback();
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //}

        //// 修改
        //[HttpPost]
        //public ApiResponse Update(MI_MAST mi_mast)
        //{
        //    using (WorkSession session = new WorkSession(this))
        //    {
        //        var DBWork = session.UnitOfWork;
        //        DBWork.BeginTransaction();
        //        try
        //        {
        //            var repo1 = new AA0038Repository(DBWork);
        //            if (mi_mast.CANCEL_ID == "Y" && repo1.CheckMmcodeRef(mi_mast.MMCODE))
        //            {
        //                session.Result.afrs = 0;
        //                session.Result.success = false;
        //                session.Result.msg = "此院內碼已被參考，不可修改為作廢。";
        //            }
        //            else
        //            {
        //                var repo2 = new BG0002Repository(DBWork);
        //                mi_mast.UPDATE_USER = User.Identity.Name;
        //                mi_mast.UPDATE_IP = DBWork.ProcIP;
        //                session.Result.afrs = repo2.Update(mi_mast);
        //            }

        //            DBWork.Commit();
        //        }
        //        catch
        //        {
        //            DBWork.Rollback();
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //}

        //[HttpPost]
        //public ApiResponse GetMmcode(FormDataCollection form)
        //{
        //    var page = int.Parse(form.Get("page"));
        //    var start = int.Parse(form.Get("start"));
        //    var limit = int.Parse(form.Get("limit"));
        //    var sorters = form.Get("sort");

        //    using (WorkSession session = new WorkSession())
        //    {
        //        UnitOfWork DBWork = session.UnitOfWork;
        //        try
        //        {
        //            BG0002Repository repo = new BG0002Repository(DBWork);
        //            BG0002Repository.MI_MAST_QUERY_PARAMS query = new BG0002Repository.MI_MAST_QUERY_PARAMS();
        //            query.MMCODE = form.Get("MMCODE") == null ? "" : form.Get("MMCODE").ToUpper();
        //            query.MMNAME_C = form.Get("MMNAME_C") == null ? "" : form.Get("MMNAME_C").ToUpper();
        //            query.MMNAME_E = form.Get("MMNAME_E") == null ? "" : form.Get("MMNAME_E").ToUpper();
        //            //query.WH_NO = form.Get("WH_NO") == null ? "" : form.Get("WH_NO").ToUpper();
        //            session.Result.etts = repo.GetMmcode(query, page, limit, sorters);
        //        }
        //        catch
        //        {
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //}




    } // ec
} // en