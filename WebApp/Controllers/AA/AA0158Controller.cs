using JCLib.DB;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Data;
using System.IO;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web;
using WebApp.Models;
using WebApp.Repository.AA;
using NPOI.SS.Util;
using System.Collections.Generic;
using NPOI.HSSF.UserModel;
using System.Text;
using Newtonsoft.Json;
using JCLib.Mvc;
using System.Globalization;
using System.Text.RegularExpressions;

namespace WebApp.Controllers.AA
{
    public class AA0158Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse GetAll(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");
            var p6 = form.Get("p6");
            var p7 = form.Get("p7");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0158Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1, p2, p3, p4, p5, p6, p7, page, limit, sorters);
                }
                catch (Exception e)
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
                    var repo = new AA0158Repository(DBWork);
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
        public ApiResponse AllD(FormDataCollection form)
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
                    var repo = new AA0158Repository(DBWork);
                    session.Result.etts = repo.GetAllD(p0, page, limit, sorters);
                }
                catch (Exception e)
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
            var p5 = form.Get("p5");
            var p6 = form.Get("p6");
            var p7 = form.Get("p7");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0158Repository repo = new AA0158Repository(DBWork);
                    JCLib.Excel.Export(form.Get("FN"), repo.GetExcel(p0, p1, p2, p3, p4, p5, p6, p7));
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
                    var repo = new AA0158Repository(DBWork);

                    if (!repo.CheckExists(mi_mast.MMCODE.Trim()))
                    {
                        mi_mast.MMCODE = mi_mast.MMCODE.Trim();
                        mi_mast.MIMASTHIS_SEQ = repo.GetHisSeq();
                        mi_mast.CREATE_USER = User.Identity.Name;
                        mi_mast.UPDATE_USER = User.Identity.Name;
                        mi_mast.UPDATE_IP = DBWork.ProcIP;
                        mi_mast.MONEYCHANGE = "Y";
                        if (mi_mast.MAT_CLASS_SUB == "1")
                        {
                            mi_mast.MAT_CLASS = "01";
                        }
                        else
                        {
                            mi_mast.MAT_CLASS = "02";
                        }
                        repo.CreateHis(mi_mast);
                        session.Result.afrs = repo.Create(mi_mast);
                        var E_CODATE = mi_mast.E_CODATE_T;
                        if (E_CODATE != null)
                        {
                            mi_mast.E_CODATE = repo.Getdatetime(E_CODATE);
                            repo.UpdateMastECodate(mi_mast);
                            repo.UpdateHisECodate(mi_mast);
                        }
                        var BEGINDATE_14 = mi_mast.BEGINDATE_14_T;
                        if (BEGINDATE_14 != null)
                        {
                            mi_mast.BEGINDATE_14 = repo.Getdatetime(BEGINDATE_14);
                            repo.UpdateMastBegindate(mi_mast);
                            repo.UpdateHisBegindate(mi_mast);
                        }
                        session.Result.etts = repo.Get(mi_mast.MMCODE);

                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>院內碼</span>已存在於藥衛材基本檔，請重新輸入。";
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

        // 複製轉新增
        [HttpPost]
        public ApiResponse Copy(FormDataCollection form)
        {
            var oldmmcode = form.Get("OLDMMCODE");
            var newmmcode = form.Get("NEWMMCODE");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0158Repository(DBWork);

                    if (!repo.CheckExists(newmmcode.Trim()))
                    {
                        IEnumerable<MI_MAST> mi_masts = repo.Get(oldmmcode);

                        foreach (MI_MAST mi_mast in mi_masts)
                        {
                            mi_mast.MMCODE = newmmcode.Trim();
                            mi_mast.MIMASTHIS_SEQ = repo.GetHisSeq();
                            mi_mast.CREATE_USER = User.Identity.Name;
                            mi_mast.UPDATE_USER = User.Identity.Name;
                            mi_mast.UPDATE_IP = DBWork.ProcIP;
                            mi_mast.MONEYCHANGE = "Y";
                            //repo.CreateHis(mi_mast);  //1130126複製轉新增不須copy歷程
                            session.Result.afrs = repo.Create(mi_mast);
                            session.Result.etts = repo.Get(mi_mast.MMCODE);

                        }

                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>院內碼</span>已存在於藥衛材基本檔，請重新輸入。";
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
                    var repo = new AA0158Repository(DBWork);
                    mi_mast.MIMASTHIS_SEQ_NEW = repo.GetHisSeq();
                    mi_mast.CREATE_USER = User.Identity.Name;
                    mi_mast.UPDATE_USER = User.Identity.Name;
                    mi_mast.UPDATE_IP = DBWork.ProcIP;


                    repo.UpdateHisEffEndDate(mi_mast.MIMASTHIS_SEQ);
                    repo.InsertHis(mi_mast);
                    mi_mast.MIMASTHIS_SEQ = mi_mast.MIMASTHIS_SEQ_NEW;
                    session.Result.afrs = repo.Update(mi_mast);
                    //相關日期欄位需另外處理
                    //(1)合約到期日
                    var E_CODATE = mi_mast.E_CODATE_T;
                    if (E_CODATE != null)
                    {
                        mi_mast.E_CODATE = repo.Getdatetime(E_CODATE);
                        repo.UpdateMastECodate(mi_mast);
                        repo.UpdateHisECodate(mi_mast);
                    }
                    //將E_CODATE改成NULL
                    if ((mi_mast.E_CODATE != null) && (E_CODATE == null))
                    {
                        repo.UpdateMastECodate(mi_mast);
                        repo.UpdateHisECodate(mi_mast);
                    }
                    //(2)建立日期
                    var BEGINDATE_14 = mi_mast.BEGINDATE_14_T;
                    if (BEGINDATE_14 != null)
                    {
                        mi_mast.BEGINDATE_14 = repo.Getdatetime(BEGINDATE_14);
                        repo.UpdateMastBegindate(mi_mast);
                        repo.UpdateHisBegindate(mi_mast);
                    }
                    //將BEGINDATE_14改成NULL
                    if ((mi_mast.BEGINDATE_14 != null) && (BEGINDATE_14 == null))
                    {
                        repo.UpdateMastBegindate(mi_mast);
                        repo.UpdateHisBegindate(mi_mast);
                    }

                    session.Result.etts = repo.Get(mi_mast.MMCODE);

                    DBWork.Commit();
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse SetPrice(FormDataCollection form)
        {
            var MIMASTHIS_SEQ = form.Get("MIMASTHIS_SEQ");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0158Repository(DBWork);
                    repo.UpdateHisEffEndDate(MIMASTHIS_SEQ);
                    IEnumerable<MI_MAST> mi_masts = repo.GetD(MIMASTHIS_SEQ);

                    foreach (MI_MAST mi_mast in mi_masts)
                    {
                        mi_mast.MIMASTHIS_SEQ = MIMASTHIS_SEQ;
                        mi_mast.MIMASTHIS_SEQ_NEW = repo.GetHisSeq();
                        mi_mast.CREATE_USER = User.Identity.Name;
                        mi_mast.UPDATE_USER = User.Identity.Name;
                        mi_mast.UPDATE_USER = User.Identity.Name;
                        mi_mast.UPDATE_IP = DBWork.ProcIP;
                        mi_mast.MONEYCHANGE = "Y";
                        repo.InsertHisPrice(mi_mast);
                        session.Result.afrs = repo.Update(mi_mast);

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
        public ApiResponse GetHisdata(FormDataCollection form)
        {
            var mmcode = form.Get("MMCODE");
            WorkSession session;

            using (session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0158Repository(DBWork);

                    if (!repo.CheckExists(mmcode))
                    {
                        if (repo.CheckExists803())
                        {
                            DBWork.BeginTransaction();
                            IEnumerable<MI_MAST> mi_masts = GetMI_MASTFromHis(mmcode);

                            foreach (MI_MAST mi_mast in mi_masts)
                            {
                                mi_mast.UPDATE_USER = User.Identity.Name;
                                mi_mast.UPDATE_IP = DBWork.ProcIP;
                                if (!repo.CheckExistsUnit(mi_mast.BASE_UNIT))
                                {
                                    repo.InsertUnit(mi_mast);
                                }
                            }
                            DBWork.Commit();
                            session.Result.etts = mi_masts;
                        }
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>院內碼</span>已存在於藥衛材基本檔，請重新輸入。";
                    }
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }

        public IEnumerable<MI_MAST> GetMI_MASTFromHis(string strMmcode)
        {
            using (WorkSession session = new WorkSession(this, "HIS"))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0158Repository(DBWork);
                    return repo.GetHisdata(strMmcode);
                }
                catch
                {
                    throw;
                }
            }
        }


        [HttpPost]
        public ApiResponse GetMMCodeCombo(FormDataCollection form)
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
                    AA0158Repository repo = new AA0158Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeCombo(p0, page, limit, "");
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMatclassCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0158Repository(DBWork);
                    session.Result.etts = repo.GetMatclassCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMatclassSubCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0158Repository(DBWork);
                    session.Result.etts = repo.GetMatclassSubCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMatclassSubFCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0158Repository(DBWork);
                    session.Result.etts = repo.GetMatclassSubFCombo();
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
                    AA0158Repository repo = new AA0158Repository(DBWork);
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
                    AA0158Repository repo = new AA0158Repository(DBWork);
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
                    AA0158Repository repo = new AA0158Repository(DBWork);
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
                    AA0158Repository repo = new AA0158Repository(DBWork);
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
        public ApiResponse GetERestrictCodeCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0158Repository repo = new AA0158Repository(DBWork);
                    session.Result.etts = repo.GetERestrictCodeCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetWarbakCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0158Repository repo = new AA0158Repository(DBWork);
                    session.Result.etts = repo.GetWarbakCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetOnecostCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0158Repository repo = new AA0158Repository(DBWork);
                    session.Result.etts = repo.GetOnecostCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetHealthPayCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0158Repository repo = new AA0158Repository(DBWork);
                    session.Result.etts = repo.GetHealthPayCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetCostKindCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0158Repository repo = new AA0158Repository(DBWork);
                    session.Result.etts = repo.GetCostKindCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetWastKindCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0158Repository repo = new AA0158Repository(DBWork);
                    session.Result.etts = repo.GetWastKindCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetSpXfeeCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0158Repository repo = new AA0158Repository(DBWork);
                    session.Result.etts = repo.GetSpXfeeCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetOrderKindCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0158Repository repo = new AA0158Repository(DBWork);
                    session.Result.etts = repo.GetOrderKindCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetDrugKindCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0158Repository repo = new AA0158Repository(DBWork);
                    session.Result.etts = repo.GetDrugKindCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetSpDrugCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0158Repository repo = new AA0158Repository(DBWork);
                    session.Result.etts = repo.GetSpDrugCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetFastDrugCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0158Repository repo = new AA0158Repository(DBWork);
                    session.Result.etts = repo.GetFastDrugCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetMContidCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0158Repository repo = new AA0158Repository(DBWork);
                    session.Result.etts = repo.GetMContidCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetTouchCaseCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0158Repository repo = new AA0158Repository(DBWork);
                    session.Result.etts = repo.GetTouchCaseCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetCommonCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0158Repository repo = new AA0158Repository(DBWork);
                    session.Result.etts = repo.GetCommonCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetCaseNoCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0158Repository repo = new AA0158Repository(DBWork);
                    session.Result.etts = repo.GetCaseNoCombo(p0);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetCurrCaseNoCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0158Repository repo = new AA0158Repository(DBWork);
                    session.Result.etts = repo.GetCurrCaseNoCombo(p0);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse MStoreidComboGet()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0158Repository repo = new AA0158Repository(DBWork);
                    session.Result.etts = repo.MStoreidComboGet();
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
                    AA0158Repository repo = new AA0158Repository(DBWork);
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
        public ApiResponse GetAgenNamecCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0158Repository repo = new AA0158Repository(DBWork);
                    session.Result.etts = repo.GetAgenNamecCombo(p0);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetUniNoCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0158Repository repo = new AA0158Repository(DBWork);
                    session.Result.etts = repo.GetUniNoCombo(p0);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetMPhctncoCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0158Repository repo = new AA0158Repository(DBWork);
                    session.Result.etts = repo.GetMPhctncoCombo(p0);
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
                    var repo = new AA0158Repository(DBWork);
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
                    AA0158Repository repo = new AA0158Repository(DBWork);
                    session.Result.etts = repo.GetFormEditable(p0);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse CheckMmcodeExists(FormDataCollection form)
        {
            var mmcode = form.Get("MMCODE");
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0158Repository repo = new AA0158Repository(DBWork);
                    bool result = repo.CheckExists(mmcode.Trim());
                    if (result)
                    { //有查到資料表示有存在院內碼
                        session.Result.success = false;
                        session.Result.msg = "院內碼已存在，請重新輸入";
                        return session.Result;
                    }

                    session.Result.success = true;
                    return session.Result;
                }
                catch (Exception e)
                {
                    throw;
                }
            }
        }

        //匯入檢核
        [HttpPost]
        public ApiResponse SendExcel()
        {
            /**
             * 
             * 以下為 匯入彈出視窗用到的 Controllers
             * 
             */
            List<HeaderItem> headerItems = new List<HeaderItem>() {
                new HeaderItem("院內碼", "MMCODE"),
                //new HeaderItem("生效起始時間", "EFFSTARTDATE"),
                new HeaderItem("是否作廢", "CANCEL_ID"),
                new HeaderItem("健保代碼", "M_NHIKEY"),
                new HeaderItem("健保自費碼", "HEALTHOWNEXP"),
                new HeaderItem("學名", "DRUGSNAME"),
                new HeaderItem("英文品名", "MMNAME_E"),
                new HeaderItem("中文品名", "MMNAME_C"),
                new HeaderItem("許可證號", "M_PHCTNCO"),
                new HeaderItem("許可證效期", "M_ENVDT"),
                new HeaderItem("申請廠商", "ISSUESUPPLY"),
                new HeaderItem("製造商", "E_MANUFACT"),
                new HeaderItem("藥材單位", "BASE_UNIT"),
                new HeaderItem("出貨包裝單位", "M_PURUN"),
                new HeaderItem("每包裝出貨量", "UNITRATE"),
                new HeaderItem("與HIS單位換算比值", "TRUTRATE"),
                //物料分類
                //new HeaderItem("物料分類", "MAT_CLASS"),
                new HeaderItem("物料子類別", "MAT_CLASS_SUB"),
                new HeaderItem("管制級數", "E_RESTRICTCODE"),
                new HeaderItem("是否戰備", "WARBAK"),
                new HeaderItem("是否可單一計價", "ONECOST"),
                new HeaderItem("是否健保給付", "HEALTHPAY"),
                new HeaderItem("費用分類", "COSTKIND"),
                new HeaderItem("是否正向消耗", "WASTKIND"),
                new HeaderItem("是否為特材", "SPXFEE"),
                new HeaderItem("採購類別", "ORDERKIND"),
                new HeaderItem("小採需求醫師", "CASEDOCT"),
                new HeaderItem("中西藥類別", "DRUGKIND"),
                new HeaderItem("廠商代碼", "M_AGENNO"),
                new HeaderItem("廠牌", "M_AGENLAB"),
                new HeaderItem("合約案號", "CASENO"),
                new HeaderItem("付款方式", "E_SOURCECODE"),
                new HeaderItem("合約方式", "M_CONTID"),
                new HeaderItem("聯標項次", "E_ITEMARMYNO"),
                new HeaderItem("健保價", "NHI_PRICE"),
                new HeaderItem("成本價", "DISC_CPRICE"),
                new HeaderItem("決標價", "M_CONTPRICE"),
                new HeaderItem("合約到期日", "E_CODATE"),
                new HeaderItem("聯標契約總數量", "CONTRACTAMT"),
                new HeaderItem("聯標項次契約總價", "CONTRACTSUM"),
                new HeaderItem("合約類別", "TOUCHCASE"),
                new HeaderItem("特殊品項", "SPDRUG"),
                new HeaderItem("急救品項", "FASTDRUG"),
                new HeaderItem("是否常用品項", "COMMON"),
                new HeaderItem("特材號碼", "SPMMCODE"),
                new HeaderItem("二次折讓數量", "DISCOUNT_QTY"),
                new HeaderItem("申請倍數", "APPQTY_TIMES"),
                new HeaderItem("二次優惠單價", "DISC_COST_UPRICE"),
                new HeaderItem("是否點滴", "ISIV"),
                new HeaderItem("庫備識別碼", "M_STOREID"),
            };

            using (WorkSession session = new WorkSession(this))
            {
                List<MI_MAST> list = new List<MI_MAST>();
                List<MI_MAST> final_list = new List<MI_MAST>();
                UnitOfWork DBWork = session.UnitOfWork;
                var HttpPostedFile = HttpContext.Current.Request.Files["file"];
                IWorkbook workBook;
                try
                {
                    AA0158Repository repo = new AA0158Repository(DBWork);
                    bool checkPassed = true; //檢核有沒有通過 有通過就true 沒通過就false

                    #region 檢查檔案格式
                    if (Path.GetExtension(HttpPostedFile.FileName).ToLower() == ".xls")
                    {
                        workBook = new HSSFWorkbook(HttpPostedFile.InputStream); //讀取xls檔
                    }
                    else
                    {
                        workBook = new XSSFWorkbook(HttpPostedFile.InputStream); //讀取xlsx檔
                    }

                    var sheet = workBook.GetSheetAt(0); //讀取EXCEL的第一個分頁
                    IRow headerRow = sheet.GetRow(0);   //由第一列取標題做為欄位名稱
                    int cellCount = headerRow.LastCellNum; //欄位數目
                    //int i, j;

                    #endregion

                    // 確定該有的欄位都有，沒有的回傳錯誤訊息
                    #region check excel header
                    headerItems = SetHeaderIndex(headerItems, headerRow);
                    string errMsg = string.Empty;
                    foreach (HeaderItem item in headerItems)
                    {
                        if (item.Index == -1)
                        {
                            if (errMsg == string.Empty)
                            {
                                errMsg += item.Name;
                            }
                            else
                            {
                                errMsg += string.Format("、{0}", item.Name);
                            }
                        }
                    }

                    if (errMsg != string.Empty)
                    {
                        session.Result.success = false;
                        session.Result.msg = string.Format("欄位錯誤，缺：{0}", errMsg);
                        return session.Result;
                    }
                    # endregion

                    for (int i = 1; i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row is null) { continue; }

                        MI_MAST temp = new MI_MAST();
                        foreach (HeaderItem item in headerItems)
                        {

                            string value = row.GetCell(item.Index) == null ? string.Empty : row.GetCell(item.Index).ToString();

                            if (temp.GetType().GetProperty(item.FieldName).PropertyType == typeof(string))
                            {
                                temp.GetType().GetProperty(item.FieldName).SetValue(temp, value, null);
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(value))
                                {
                                    DateTime d;
                                    // 如果 Excel 欄位為日期格式，傳到後面格式會為 月/日/年 10/10/24
                                    if (value.Length == 8)
                                    {
                                        var splitValue = value.Split(new[] { "/" }, StringSplitOptions.None);
                                        value = "20" + splitValue[2] + "/" + splitValue[0] + "/" + splitValue[1];
                                    }
                                    DateTime.TryParse(value.ToString(), out d);
                                    temp.GetType().GetProperty(item.FieldName).SetValue(temp, d, null);

                                }
                            }
                        }
                        list.Add(temp);
                    }

                    // 儲存 院內碼 的字串，如果 匯入時，此字串重複，直接過濾
                    List<string> checkList = new List<string>();

                    int j;
                    foreach (MI_MAST item in list)
                    {
                        item.UPDATE_USER = DBWork.UserInfo.UserId;
                        bool breakloop = false;

                        // 檢查院內碼
                        #region 檢查 key 值
                        if (string.IsNullOrEmpty(item.MMCODE))
                        {
                            item.CHECK_RESULT = "院內碼為必填";
                            checkPassed = false;
                            final_list.Add(item);
                            continue;
                        }
                        else if (checkList.Contains(item.MMCODE))
                        {
                            item.CHECK_RESULT = "院內碼重複";
                            checkPassed = false;
                            final_list.Add(item);
                            continue;
                        }
                        checkList.Add(item.MMCODE);
                        #endregion

                        #region 檢查必填欄位
                        Dictionary<string, string> requiredCols;
                        // 如果不存在，代表資料為新增，必填欄位要有值
                        breakloop = false;
                        if (!repo.CheckExistsMast(item.MMCODE)) // Create
                        {
                            requiredCols = new Dictionary<string, string>{
                                { "MMCODE", "院內碼" }, { "E_RESTRICTCODE", "管制級數" }, { "WARBAK", "是否戰備" },
                                { "ONECOST", "是否可單一計價" }, { "HEALTHPAY", "是否健保給付" }, { "COSTKIND", "費用分類" },
                                { "WASTKIND", "是否正向消耗" }, { "SPXFEE", "是否為特材" }, { "ORDERKIND", "採購類別" },
                                { "DRUGKIND", "中西藥類別" }, { "E_SOURCECODE", "付款方式" }, { "M_CONTID", "合約方式" },
                                { "TOUCHCASE", "合約類別" }, { "SPDRUG", "特殊品項" }, { "FASTDRUG", "急救品項" },
                                { "COMMON", "是否常用品項" }
                            };
                        }
                        else // Update
                        {
                            requiredCols = new Dictionary<string, string>{
                                { "MMCODE", "院內碼" }
                            };
                        }

                        foreach (KeyValuePair<string, string> mapItem in requiredCols)
                        {
                            var value = item.GetType().GetProperty(mapItem.Key).GetValue(item, null).ToString();

                            if (string.IsNullOrEmpty(value))
                            {
                                item.CHECK_RESULT = mapItem.Value + " 為必填欄位";
                                checkPassed = false;
                                breakloop = true;
                                final_list.Add(item);
                                break;
                            }
                        }
                        if (breakloop) continue;
                        #endregion

                        #region 檢查 枚舉值
                        if (!string.IsNullOrEmpty(item.CANCEL_ID) && item.CANCEL_ID != "Y" && item.CANCEL_ID != "N")
                        {
                            item.CHECK_RESULT = "是否作廢:  必須等於Y, N";
                            checkPassed = false;
                            final_list.Add(item);
                            continue;
                        }

                        if (!string.IsNullOrEmpty(item.M_STOREID) && item.M_STOREID != "0" && item.M_STOREID != "1")
                        {
                            item.CHECK_RESULT = "庫備識別碼: 必須等於0, 1";
                            checkPassed = false;
                            final_list.Add(item);
                            continue;
                        }
                        #endregion

                        #region 檢查 數值欄位
                        Dictionary<string, string> numberCols = new Dictionary<string, string>{
                            { "DISCOUNT_QTY", "二次折讓數量" }, { "APPQTY_TIMES", "申請倍數" }, { "DISC_COST_UPRICE", "二次優惠單價" },
                            { "DISC_CPRICE", "成本價" }, { "M_CONTPRICE", "決標價" }, { "NHI_PRICE", "健保價" },
                            { "CONTRACTAMT", "聯標契約總數量" }, { "UNITRATE", "出貨單位" },
                        };
                        breakloop = false;
                        double dNumber;
                        foreach (KeyValuePair<string, string> mapItem in numberCols)
                        {
                            if (!string.IsNullOrEmpty(item.GetType().GetProperty(mapItem.Key).GetValue(item, null).ToString()))
                            {
                                var value = item.GetType().GetProperty(mapItem.Key).GetValue(item, null).ToString();

                                if (double.TryParse(value, out dNumber))
                                {
                                    if (dNumber < 0)
                                    {
                                        item.CHECK_RESULT = mapItem.Value + ":  必須大於等於1";
                                        checkPassed = false;
                                        final_list.Add(item);
                                        breakloop = true;
                                        break;
                                    }

                                    item.GetType().GetProperty(mapItem.Key).SetValue(item, value.Replace(",", ""));
                                }
                                else
                                {
                                    item.CHECK_RESULT = mapItem.Value + ":  必須為數值";
                                    checkPassed = false;
                                    final_list.Add(item);
                                    breakloop = true;
                                    break;
                                }
                            }
                        }
                        if (breakloop) continue;
                        #endregion

                        #region 檢查 參數值
                        // check paramd
                        var paramds = new Dictionary<string, string>{
                                { "E_RESTRICTCODE", "管制級數" }, { "WARBAK", "是否戰備" }, { "ONECOST", "是否可單一計價" },
                                { "HEALTHPAY", "是否健保給付" }, { "COSTKIND", "費用分類" }, { "WASTKIND", "是否正向消耗" },
                                { "SPXFEE", "是否為特材" }, { "ORDERKIND", "採購類別" }, { "DRUGKIND", "中西藥類別" },
                                { "E_SOURCECODE", "付款方式" }, { "M_CONTID", "合約方式" }, { "TOUCHCASE", "合約類別" },
                                { "SPDRUG", "特殊品項" }, { "FASTDRUG", "急救品項" }, { "COMMON", "是否常用品項" }
                            };

                        breakloop = false;
                        foreach (KeyValuePair<string, string> mapItem in paramds)
                        {
                            if (!string.IsNullOrEmpty(item.GetType().GetProperty(mapItem.Key).GetValue(item, null).ToString()))
                            {
                                var value = item.GetType().GetProperty(mapItem.Key).GetValue(item, null).ToString();

                                if (!repo.CheckExistsPARAM_D(mapItem.Key, value))
                                {
                                    item.CHECK_RESULT = mapItem.Value + " 不存在或是有誤";
                                    checkPassed = false;
                                    breakloop = true;
                                    final_list.Add(item);
                                    break;
                                }
                            }
                        }
                        if (breakloop) continue;
                        #endregion

                        // 若無問題補上 OK
                        if (string.IsNullOrEmpty(item.CHECK_RESULT))
                        {
                            item.CHECK_RESULT = "OK";
                        }

                        final_list.Add(item);
                    }

                    session.Result.etts = final_list;
                    session.Result.msg = checkPassed.ToString();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //確認更新
        [HttpPost]
        public ApiResponse InsertFromXls(FormDataCollection formData)
        {
            /**
             * 
             * 以下為 匯入彈出視窗用到的 Controllers
             * 
             */
            List<HeaderItem> headerItems = new List<HeaderItem>() {
                new HeaderItem("院內碼", "MMCODE"),
                //new HeaderItem("生效起始時間", "EFFSTARTDATE"),
                new HeaderItem("是否作廢", "CANCEL_ID"),
                new HeaderItem("健保代碼", "M_NHIKEY"),
                new HeaderItem("健保自費碼", "HEALTHOWNEXP"),
                new HeaderItem("學名", "DRUGSNAME"),
                new HeaderItem("英文品名", "MMNAME_E"),
                new HeaderItem("中文品名", "MMNAME_C"),
                new HeaderItem("許可證號", "M_PHCTNCO"),
                new HeaderItem("許可證效期", "M_ENVDT"),
                new HeaderItem("申請廠商", "ISSUESUPPLY"),
                new HeaderItem("製造商", "E_MANUFACT"),
                new HeaderItem("藥材單位", "BASE_UNIT"),
                new HeaderItem("出貨包裝單位", "M_PURUN"),
                new HeaderItem("每包裝出貨量", "UNITRATE"),
                new HeaderItem("與HIS單位換算比值", "TRUTRATE"),
                //物料分類
                //new HeaderItem("物料分類", "MAT_CLASS"),
                new HeaderItem("物料子類別", "MAT_CLASS_SUB"),
                new HeaderItem("管制級數", "E_RESTRICTCODE"),
                new HeaderItem("是否戰備", "WARBAK"),
                new HeaderItem("是否可單一計價", "ONECOST"),
                new HeaderItem("是否健保給付", "HEALTHPAY"),
                new HeaderItem("費用分類", "COSTKIND"),
                new HeaderItem("是否正向消耗", "WASTKIND"),
                new HeaderItem("是否為特材", "SPXFEE"),
                new HeaderItem("採購類別", "ORDERKIND"),
                new HeaderItem("小採需求醫師", "CASEDOCT"),
                new HeaderItem("中西藥類別", "DRUGKIND"),
                new HeaderItem("廠商代碼", "M_AGENNO"),
                new HeaderItem("廠牌", "M_AGENLAB"),
                new HeaderItem("合約案號", "CASENO"),
                new HeaderItem("付款方式", "E_SOURCECODE"),
                new HeaderItem("合約方式", "M_CONTID"),
                new HeaderItem("聯標項次", "E_ITEMARMYNO"),
                new HeaderItem("健保價", "NHI_PRICE"),
                new HeaderItem("成本價", "DISC_CPRICE"),
                new HeaderItem("決標價", "M_CONTPRICE"),
                new HeaderItem("合約到期日", "E_CODATE"),
                new HeaderItem("聯標契約總數量", "CONTRACTAMT"),
                new HeaderItem("聯標項次契約總價", "CONTRACTSUM"),
                new HeaderItem("合約類別", "TOUCHCASE"),
                new HeaderItem("特殊品項", "SPDRUG"),
                new HeaderItem("急救品項", "FASTDRUG"),
                new HeaderItem("是否常用品項", "COMMON"),
                new HeaderItem("特材號碼", "SPMMCODE"),
                new HeaderItem("二次折讓數量", "DISCOUNT_QTY"),
                new HeaderItem("申請倍數", "APPQTY_TIMES"),
                new HeaderItem("二次優惠單價", "DISC_COST_UPRICE"),
                new HeaderItem("是否點滴", "ISIV"),
                new HeaderItem("庫備識別碼", "M_STOREID"),
            };

            //這些欄位 跟 前端的 欄位 要同步檢查
            /// <summary>
            ///    "健保價", "NHI_PRICE"
            ///    "成本價", "DISC_CPRICE"
            ///    "決標價", "M_CONTPRICE"
            ///   "聯標項次契約總價", "CONTRACTSUM"
            /// </summary>
            List<string> CheckMoneyChangeItems = new List<string>() {
                "NHI_PRICE",
                "DISC_CPRICE",
                "M_CONTPRICE",
                "CONTRACTSUM"
            };

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();

                IEnumerable<MI_MAST> mi_masts = JsonConvert.DeserializeObject<IEnumerable<MI_MAST>>(formData["data"]);

                try
                {
                    var repo = new AA0158Repository(DBWork);

                    foreach (MI_MAST dataMI_MAST in mi_masts)
                    {
                        dataMI_MAST.CREATE_USER = User.Identity.Name;
                        dataMI_MAST.UPDATE_USER = User.Identity.Name;
                        dataMI_MAST.UPDATE_IP = DBWork.ProcIP;

                        if (repo.CheckExistsMast(dataMI_MAST.MMCODE))
                        {
                            MI_MAST mi_mast = repo.GetMI_MAST(dataMI_MAST.MMCODE);
                            mi_mast.MONEYCHANGE = "N";

                            foreach (HeaderItem headerItem in headerItems)
                            {
                                if (headerItem.FieldName == "MMCODE") continue;

                                string strFieldName = headerItem.FieldName;
                                var property = dataMI_MAST.GetType().GetProperty(strFieldName);

                                var newValue = property.GetValue(dataMI_MAST, null);

                                if (newValue != null && !string.IsNullOrEmpty(newValue.ToString()))
                                {
                                    if (property.PropertyType == typeof(string) || property.PropertyType == typeof(int))
                                    {
                                        if ("MAT_CLASS_SUB" == strFieldName)
                                        {
                                            mi_mast.MAT_CLASS = mi_mast.MAT_CLASS_SUB == "1" ? "01" : "02";
                                        }

                                        // 如果某些跟金額相關的欄位要秀在下方歷史紀錄的話，moneychange 要為 Y
                                        if (!string.IsNullOrEmpty(newValue.ToString()) && CheckMoneyChangeItems.Contains(strFieldName))
                                        {
                                            var oldValue = mi_mast.GetType().GetProperty(strFieldName).GetValue(mi_mast, null).ToString();

                                            if (oldValue != newValue.ToString())
                                            {
                                                mi_mast.MONEYCHANGE = "Y";
                                            }
                                        }

                                        mi_mast.GetType().GetProperty(strFieldName).SetValue(mi_mast, newValue.ToString());
                                    }
                                    else
                                    {
                                        DateTime d;
                                        DateTime.TryParse(newValue.ToString(), out d);
                                        mi_mast.GetType().GetProperty(strFieldName).SetValue(mi_mast, d, null);
                                    }
                                }
                            }

                            int afrs = repo.Update(mi_mast);
                            if (afrs > 0)
                            {
                                repo.UpdateHisEffEndDate(mi_mast.MIMASTHIS_SEQ);
                                mi_mast.MIMASTHIS_SEQ_NEW = repo.GetHisSeq();
                                repo.InsertHis(mi_mast);
                            }
                        }
                        else
                        {
                            dataMI_MAST.MAT_CLASS = dataMI_MAST.MAT_CLASS_SUB == "1" ? "01" : "02";
                            repo.Create(dataMI_MAST);

                            // his
                            dataMI_MAST.MIMASTHIS_SEQ_NEW = repo.GetHisSeq();
                            dataMI_MAST.MONEYCHANGE = "Y";
                            repo.InsertHis(dataMI_MAST);
                        }
                    }
                    //session.Result.etts = mi_hist_list;
                    DBWork.Commit();
                }
                catch(Exception e)
                {
                    DBWork.Rollback();
                    throw e;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetExcelExample(FormDataCollection form)
        {

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0158Repository repo = new AA0158Repository(DBWork);
                    JCLib.Excel.Export("藥衛材基本檔維護作業.xls", repo.GetExcelExample());
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetTxtExample()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    ExportTxt();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }

        }

        public void ExportTxt()
        {
            string fileName = "填寫說明.txt";

            if (HttpContext.Current == null) return;

            HttpResponse res = HttpContext.Current.Response;
            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms, Encoding.GetEncoding("big5"));
            string line = Content();
            Encoding utf8 = Encoding.GetEncoding("utf-8");
            Encoding big51 = Encoding.GetEncoding("big5");
            sw.WriteLine(line); //write data
            sw.Close();
            res.BufferOutput = false;
            res.Clear();
            res.ClearHeaders();
            res.ContentEncoding = utf8;
            res.Charset = "utf-8";
            res.ContentType = "application/octet-stream;charset=utf-8";
            res.AddHeader("Content-Disposition",
                        "attachment; filename=" + HttpUtility.HtmlEncode(HttpUtility.UrlEncode(fileNameFilter(fileName, ".txt"), System.Text.Encoding.UTF8)));
            res.BinaryWrite(ms.ToArray());
            res.Flush();
            res.End();

            ms.Close();
            ms.Dispose();
        }

        static string fileNameFilter(string fileName, string extention)
        {
            string allowlist = @"^[\u4e00-\u9fa5_.\-a-zA-Z0-9]+$";
            fileName = fileName.Trim();
            Regex pattern = new Regex(allowlist);
            if (pattern.IsMatch(fileName))
            {
                return fileName;
            }
            else
            {
                return "downloadFile" + extention;
            }
        }

        public string Content()
        {
            // 以下文字可以用 純文字編輯 細明體 (或中英等寬字型)
            return @"
欄位  欄位名稱              是否必填    代碼說明                                         資料類型
A     院內碼                必填                                                         文數字13碼
B     學名                  可以不填                                                     文數字300碼
C     英文品名              可以不填    英文品名、中文品名 二個欄位至少填一個            文數字300碼
D     中文品名              可以不填    英文品名、中文品名 二個欄位至少填一個            文數字250碼
E     健保代碼              可以不填                                                     文數字20碼
F     健保自費碼            可以不填                                                     文數字16碼
G     許可證號    	    可以不填                                                     文數字120碼
H     許可證效期            可以不填                                                     文數字8碼
I     申請廠商              可以不填                                                     文數字128碼
J     製造商                可以不填                                                     文數字200碼
K     藥材單位              必填        相當於 舊藥衛材的「藥材單位」；
                                        必須先在「計量單位維護」功能建立資料             文數字6碼
L     出貨包裝單位          必填        出貨包裝單位 
                                        必須先在「計量單位維護」功能建立資料             文數字6碼
M     每包裝出貨量          必填        相當於 每包裝出貨量(單位/包裝)，
                                        每一包裝 等於 多少單位量                         數值
N     物料子類別       	    必填        相當於舊藥衛材的「類別代碼」                     文數字2碼
O     是否常用品項          必填        1：非常用品, 2：常用品, 3：藥品, 4：檢驗         文數字1碼
P     與HIS單位換算比值     必填        預設值為1                                        數值
Q     是否可單一計價        必填        0：不具計價性, 1：可單一計價, 2：不可單一計價    文數字1碼
R     是否健保給付          可以不填    0：不具計價性, 1：健保給付, 2：自費              文數字1碼
S     費用分類              必填    0：不具計價性, 1：材料費, 2：處置費,
                                        3：兩者皆有                                      文數字1碼
T     管制級數              必填        N：非管制用藥, 0：其它列管藥品, 
                                        1：第一級管制用藥, 2：第二級管制用藥,
                                        3：第三級管制用藥, 4：第四級管制用藥             文數字1碼
U     是否戰備              必填        0：非戰備, 1：戰備                               文數字1碼
V     是否正向消耗          必填        0：否, 1：是                                     文數字1碼
W     是否為特材            必填        0：非特材, 1：特材                               文數字1碼
X     採購類別              必填        0：無, 1：常備品項, 2：小額採購                  文數字1碼
Y     小採需求醫師          可以不填                                                     文數字30碼
Z     是否作廢              必填        N：使用, Y：作廢                                 文數字1碼
AA    庫備識別碼            必填        0：非庫備, 1：庫備； 
                                        除了804之外，其他醫院都填0；                     文數字1碼
AB    中西藥類別            必填        0：非藥品, 1：西藥, 2：中藥                      文數字1碼
AC    是否點滴              必填        N：非點滴, Y：點滴                               文數字1碼
AD    特殊品項              必填        0：非特殊品項, 1：特殊品項                       文數字1碼
AE    特材號碼              可以不填                                                     文數字12碼
AF    急救品項              必填        0：非急救品項, 1：急救品項                       文數字1碼
AG    申請倍數              可以不填    預設值為1                                        數值
AH    廠商代碼              可以不填    必須先在「廠商資料維護」功能建立資料             文數字8碼
AI    合約案號              可以不填                                                     文數字30碼
AJ    廠牌                  可以不填                                                     文數字64碼
AK    合約類別              必填        0：無合約, 1：院內選項, 2：非院內選項,
                                        3：院內自辦合約；                                文數字1碼
AL    合約到期日            可以不填                                                     日期
AM    付款方式              必填    P：買斷, C：寄售, R：核醫, N：其它               文數字1碼
AN    合約方式              必填    0：合約品項, 2：非合約, 3：不能申請(零購)        文數字1碼
AO    成本價                可以不填    優惠合約單價                                     數值
AP    決標價                可以不填    合約單價                                         數值
AQ    二次折讓數量          可以不填                                                     數值
AR    二次優惠單價          可以不填                                                     數值，小數2位
AS    聯標項次              可以不填    軍聯項次號                                       文數字20碼
AT    健保價                可以不填    健保給付價                                       數值
AU    聯標契約總數量        可以不填                                                     數值
AV    聯標項次契約總價      可以不填                                                     文數字30碼
";
        }

        public class HeaderItem
        {
            public string Name { get; set; }
            public string FieldName { get; set; }
            public int Index { get; set; }
            public HeaderItem()
            {
                Name = string.Empty;
                Index = -1;
                FieldName = string.Empty;
            }
            public HeaderItem(string name, int index, string fieldName)
            {
                Name = name;
                Index = index;
                FieldName = fieldName;
            }
            public HeaderItem(string name, string fieldName)
            {
                Name = name;
                Index = -1;
                FieldName = fieldName;
            }
        }

        public List<HeaderItem> SetHeaderIndex(List<HeaderItem> list, IRow headerRow)
        {
            int cellCounts = headerRow.LastCellNum;
            for (int i = 0; i < cellCounts; i++)
            {
                foreach (HeaderItem item in list)
                {
                    if (headerRow.GetCell(i).ToString() == item.Name)
                    {
                        item.Index = i;
                    }
                }
            }

            return list;
        }
    }
}