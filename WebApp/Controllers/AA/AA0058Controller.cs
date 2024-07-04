using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using WebApp.Repository.UR;
using System;

namespace WebApp.Controllers.AA
{
    public class AA0058Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse GetAll(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4"); //p4~p8 AB0092 used 
            var p5 = form.Get("p5");
            var p6 = form.Get("p6");
            var p7 = form.Get("p7");
            var p8 = form.Get("p8");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0058Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1, p2, p3, p4, p5, p6, p7, p8, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Get(FormDataCollection form)
        {
            var p0 = form.Get("p0");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0058Repository(DBWork);
                    session.Result.etts = repo.Get(p0);
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
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0058Repository repo = new AA0058Repository(DBWork);
                    JCLib.Excel.Export(form.Get("FN"), repo.GetExcel(p0, p1, p2, p3));
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
        public ApiResponse Create(MI_MAST mi_mast)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0058Repository(DBWork);
                    if (!repo.CheckExists(mi_mast.MMCODE))
                    {
                        mi_mast.CREATE_USER = User.Identity.Name;
                        mi_mast.UPDATE_USER = User.Identity.Name;
                        mi_mast.UPDATE_IP = DBWork.ProcIP;

                        //// 計算最小單價,優惠合約單價,優惠最小單價
                        //if (mi_mast.M_CONTPRICE == "" || mi_mast.M_CONTPRICE == null)
                        //    mi_mast.M_CONTPRICE = "0";
                        //if (mi_mast.M_DISCPERC == "" || mi_mast.M_DISCPERC == null)
                        //    mi_mast.M_DISCPERC = "0";
                        //else if (Convert.ToDouble(mi_mast.M_DISCPERC) > 100)
                        //    mi_mast.M_DISCPERC = "100";
                        //mi_mast.UPRICE = Math.Round(Convert.ToDouble(mi_mast.M_CONTPRICE) / Convert.ToDouble(mi_mast.EXCH_RATIO), 4).ToString(); //最小單價=合約單價/廠商包裝轉換率 (4捨5入到小數4位)
                        //mi_mast.DISC_CPRICE = Math.Round(Convert.ToDouble(mi_mast.M_CONTPRICE) * (1 - (Convert.ToDouble(mi_mast.M_DISCPERC) / 100)), 4).ToString(); // 優惠合約單價=合約單價*(1-(折讓比/100)) (4捨5入到小數4位)
                        //mi_mast.DISC_UPRICE = Math.Round(Convert.ToDouble(mi_mast.UPRICE) * (1 - (Convert.ToDouble(mi_mast.M_DISCPERC) / 100)), 4).ToString(); // 優惠最小單價=最小單價*(1-(折讓比/100)) (4捨5入到小數4位)
                        session.Result.afrs = repo.Create(mi_mast);

                        var repo2 = new AA0038Repository(DBWork);
                        // 廠商包裝轉換率
                        if (repo2.ChkExchRatio(mi_mast.MMCODE, mi_mast.M_PURUN, mi_mast.M_AGENNO) > 0)
                            repo2.UpdateExchRatio(mi_mast);
                        else
                            repo2.InsertExchRatio(mi_mast);

                        var repo3 = new UR_UploadRepository(DBWork);
                        repo3.Confirm(mi_mast.PFILE_ID);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>院內碼</span>重複，請重新輸入。";
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
        public ApiResponse Update(MI_MAST mi_mast)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo1 = new AA0038Repository(DBWork);
                    if (mi_mast.CANCEL_ID == "Y" && repo1.CheckMmcodeRef(mi_mast.MMCODE))
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "此院內碼已被參考，不可修改為作廢。";
                    }
                    else
                    {
                        var repo2 = new AA0058Repository(DBWork);
                        mi_mast.UPDATE_USER = User.Identity.Name;
                        mi_mast.UPDATE_IP = DBWork.ProcIP;

                        //// 計算最小單價,優惠合約單價,優惠最小單價
                        //if (mi_mast.M_CONTPRICE == "" || mi_mast.M_CONTPRICE == null)
                        //    mi_mast.M_CONTPRICE = "0";
                        //if (mi_mast.M_DISCPERC == "" || mi_mast.M_DISCPERC == null)
                        //    mi_mast.M_DISCPERC = "0";
                        //else if (Convert.ToDouble(mi_mast.M_DISCPERC) > 100)
                        //    mi_mast.M_DISCPERC = "100";
                        //// 若合約單價為0則不做計算
                        //if (Convert.ToDouble(mi_mast.M_CONTPRICE) != 0)
                        //{
                        //    mi_mast.UPRICE = Math.Round(Convert.ToDouble(mi_mast.M_CONTPRICE) / Convert.ToDouble(mi_mast.EXCH_RATIO), 4).ToString(); //最小單價=合約單價/廠商包裝轉換率 (4捨5入到小數4位)
                        //    mi_mast.DISC_CPRICE = Math.Round(Convert.ToDouble(mi_mast.M_CONTPRICE) * (1 - (Convert.ToDouble(mi_mast.M_DISCPERC) / 100)), 4).ToString(); // 優惠合約單價=合約單價*(1-(折讓比/100)) (4捨5入到小數4位)
                        //    mi_mast.DISC_UPRICE = Math.Round(Convert.ToDouble(mi_mast.UPRICE) * (1 - (Convert.ToDouble(mi_mast.M_DISCPERC) / 100)), 4).ToString(); // 優惠最小單價=最小單價*(1-(折讓比/100)) (4捨5入到小數4位)
                        //}
                        session.Result.afrs = repo2.Update(mi_mast);

                        if (mi_mast.EXCH_RATIO == "" || mi_mast.EXCH_RATIO == null)
                            mi_mast.EXCH_RATIO = "0";

                        var repo3 = new AA0038Repository(DBWork);
                        // 廠商包裝轉換率
                        if (repo3.ChkExchRatio(mi_mast.MMCODE, mi_mast.M_PURUN, mi_mast.M_AGENNO) > 0)
                            repo3.UpdateExchRatio(mi_mast);
                        else
                            repo3.InsertExchRatio(mi_mast);

                        var repo4 = new UR_UploadRepository(DBWork);
                        repo4.Confirm(mi_mast.PFILE_ID);
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