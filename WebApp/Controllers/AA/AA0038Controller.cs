using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using WebApp.Repository.UR;
using System;

namespace WebApp.Controllers.AA
{
    public class AA0038Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse GetAll(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0038Repository(DBWork);
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
        public ApiResponse Get(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0038Repository(DBWork);
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
            var p4 = form.Get("p4");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0038Repository repo = new AA0038Repository(DBWork);
                    JCLib.Excel.Export(form.Get("FN"), repo.GetExcel(p0, p1, p2, p3, p4));
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
                    var repo = new AA0038Repository(DBWork);
                    if (!repo.CheckExists(mi_mast.MMCODE))
                    {
                        mi_mast.CREATE_USER = User.Identity.Name;
                        mi_mast.UPDATE_USER = User.Identity.Name;
                        mi_mast.UPDATE_IP = DBWork.ProcIP;

                        string mat_clsid = repo.GetMatClsid(mi_mast.MAT_CLASS);

                        // 新增及修改時,若為一般物品,能設,通信則計算最小單價,優惠合約單價,優惠最小單價
                        // 衛材不可在此新增,故對衛材的計算只在修改才有
                        if (mat_clsid == "3" || mat_clsid == "4" || mat_clsid == "5")
                        {
                            if (mi_mast.M_CONTPRICE == "" || mi_mast.M_CONTPRICE == null)
                                mi_mast.M_CONTPRICE = "0";
                            if (mi_mast.M_DISCPERC == "" || mi_mast.M_DISCPERC == null)
                                mi_mast.M_DISCPERC = "0";
                            else if (Convert.ToDouble(mi_mast.M_DISCPERC) > 100)
                                mi_mast.M_DISCPERC = "100";
                            mi_mast.UPRICE = Math.Round(Convert.ToDouble(mi_mast.M_CONTPRICE) / Convert.ToDouble(mi_mast.EXCH_RATIO), 4).ToString(); //最小單價=合約單價/廠商包裝轉換率 (4捨5入到小數4位)
                            mi_mast.DISC_CPRICE = Math.Round(Convert.ToDouble(mi_mast.M_CONTPRICE) * (1 - (Convert.ToDouble(mi_mast.M_DISCPERC) / 100)), 4).ToString(); // 優惠合約單價=合約單價*(1-(折讓比/100)) (4捨5入到小數4位)
                            mi_mast.DISC_UPRICE = Math.Round(Convert.ToDouble(mi_mast.UPRICE) * (1 - (Convert.ToDouble(mi_mast.M_DISCPERC) / 100)), 4).ToString(); // 優惠最小單價=最小單價*(1-(折讓比/100)) (4捨5入到小數4位)
                        }
                        session.Result.afrs = repo.Create(mi_mast);

                        // 最小撥補量
                        if (mat_clsid == "2" || mat_clsid == "3")
                        {
                            repo.UpdateMinOrdQty(mi_mast);
                            repo.InsertMinOrdQty(mi_mast);
                        }

                        // 廠商包裝轉換率
                        if (repo.ChkExchRatio(mi_mast.MMCODE, mi_mast.M_PURUN, mi_mast.M_AGENNO) > 0)
                            repo.UpdateExchRatio(mi_mast);
                        else
                            repo.InsertExchRatio(mi_mast);

                        var repo2 = new UR_UploadRepository(DBWork);
                        repo2.Confirm(mi_mast.PFILE_ID);
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
                    var repo = new AA0038Repository(DBWork);
                    if (mi_mast.CANCEL_ID == "Y" && repo.CheckMmcodeRef(mi_mast.MMCODE))
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "此院內碼已被參考，不可修改為作廢。";
                    }
                    else
                    {
                        mi_mast.UPDATE_USER = User.Identity.Name;
                        mi_mast.UPDATE_IP = DBWork.ProcIP;

                        // 新增及修改時,若為一般物品,能設,通信則計算最小單價,優惠合約單價,優惠最小單價
                        // 2020.05.19增加衛材也套用此規則
                        string mat_clsid = repo.GetMatClsid(mi_mast.MAT_CLASS);
                        if (mat_clsid == "2" || mat_clsid == "3" || mat_clsid == "4" || mat_clsid == "5")
                        {
                            if (mi_mast.M_CONTPRICE == "" || mi_mast.M_CONTPRICE == null)
                                mi_mast.M_CONTPRICE = "0";
                            if (mi_mast.M_DISCPERC == "" || mi_mast.M_DISCPERC == null)
                                mi_mast.M_DISCPERC = "0";
                            else if (Convert.ToDouble(mi_mast.M_DISCPERC) > 100)
                                mi_mast.M_DISCPERC = "100";
                            // 若合約單價為0且是衛材則不做計算
                            if (Convert.ToDouble(mi_mast.M_CONTPRICE) != 0 || mat_clsid != "2")
                            {
                                mi_mast.UPRICE = Math.Round(Convert.ToDouble(mi_mast.M_CONTPRICE) / Convert.ToDouble(mi_mast.EXCH_RATIO), 4).ToString(); //最小單價=合約單價/廠商包裝轉換率 (4捨5入到小數4位)
                                mi_mast.DISC_CPRICE = Math.Round(Convert.ToDouble(mi_mast.M_CONTPRICE) * (1 - (Convert.ToDouble(mi_mast.M_DISCPERC) / 100)), 4).ToString(); // 優惠合約單價=合約單價*(1-(折讓比/100)) (4捨5入到小數4位)
                                mi_mast.DISC_UPRICE = Math.Round(Convert.ToDouble(mi_mast.UPRICE) * (1 - (Convert.ToDouble(mi_mast.M_DISCPERC) / 100)), 4).ToString(); // 優惠最小單價=最小單價*(1-(折讓比/100)) (4捨5入到小數4位)
                            }
                        }
                        session.Result.afrs = repo.Update(mi_mast);

                        if (mi_mast.MIN_ORDQTY == "" || mi_mast.MIN_ORDQTY == null)
                            mi_mast.MIN_ORDQTY = "0";
                        if (mi_mast.EXCH_RATIO == "" || mi_mast.EXCH_RATIO == null)
                            mi_mast.EXCH_RATIO = "0";
                        // 最小撥補量
                        repo.UpdateMinOrdQty(mi_mast);
                        repo.InsertMinOrdQty(mi_mast);

                        // 廠商包裝轉換率
                        if (repo.ChkExchRatio(mi_mast.MMCODE, mi_mast.M_PURUN, mi_mast.M_AGENNO) > 0)
                            repo.UpdateExchRatio(mi_mast);
                        else
                            repo.InsertExchRatio(mi_mast);

                        var repo2 = new UR_UploadRepository(DBWork);
                        repo2.Confirm(mi_mast.PFILE_ID);
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
        public ApiResponse GetMmcodeCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0038Repository repo = new AA0038Repository(DBWork);
                    session.Result.etts = repo.GetMmcodeCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMatclassCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0"); // 衛材(M),能設(ED),通信(CN),氣體(AR),衛材+氣體(M+AR)
            var p1 = form.Get("p1"); // 新增(I),修改(U),檢視(V)

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0038Repository repo = new AA0038Repository(DBWork);
                    session.Result.etts = repo.GetMatclassCombo(p0, p1);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetBaseunitCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0038Repository repo = new AA0038Repository(DBWork);
                    session.Result.etts = repo.GetBaseunitCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetYnCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0038Repository repo = new AA0038Repository(DBWork);
                    session.Result.etts = repo.GetYnCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetRestriCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0038Repository repo = new AA0038Repository(DBWork);
                    session.Result.etts = repo.GetRestriCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetESourceCodeCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0038Repository repo = new AA0038Repository(DBWork);
                    session.Result.etts = repo.GetESourceCodeCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetAgennoCombo(FormDataCollection form)
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
                    AA0038Repository repo = new AA0038Repository(DBWork);
                    session.Result.etts = repo.GetAgennoCombo(p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse ChkIsAdmg(FormDataCollection form)
        {

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0038Repository(DBWork);
                    if (repo.ChkIsAdmg(DBWork.ProcUser) > 0)
                        session.Result.msg = "Y";
                    else
                        session.Result.msg = "N";
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetFormEditable(FormDataCollection form)
        {
            var p0 = form.Get("p0"); // 院內碼
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0038Repository repo = new AA0038Repository(DBWork);
                    session.Result.etts = repo.GetFormEditable(p0);
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