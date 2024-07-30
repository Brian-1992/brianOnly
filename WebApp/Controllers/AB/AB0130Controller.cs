using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AB;
using WebApp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System;

namespace WebApp.Controllers.AB
{
    public class AB0130Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            string tuser = User.Identity.Name;
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0130Repository(DBWork);
                    session.Result.etts = repo.GetAllM(tuser, p0, p1, p2, p3, page, limit, sorters);
                }
                catch (Exception e)
                {
                    string ex = e.ToString();
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse AllD(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0130Repository(DBWork);
                    session.Result.etts = repo.GetAllD(p0, p1, p2, User.Identity.Name, page, limit, sorters);
                }
                catch (Exception e)
                {
                    string ex = e.ToString();
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMmCodeCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var appInid = form.Get("appdept");
            string tuser = User.Identity.Name;
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0130Repository repo = new AB0130Repository(DBWork);

                    string supply_whno = repo.GetSupplyWhno(tuser);
                    var dgMastInid = repo.GetDgmissMast(appInid) == "1" ? appInid : ""; //如果DGMISS_MAST有值就傳入主檔的補藥單位
                    session.Result.etts = repo.GetMmCodeCombo(p0, tuser, supply_whno, dgMastInid, page, limit, "");
                }
                catch (Exception e)
                {
                    string ex = e.ToString();
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse InsertD(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0130Repository(DBWork);
                    var docno = form.Get("DOCNO");
                    var mmcode = form.Get("MMCODE");
                    var appqty = form.Get("APPQTY");
                    var inv_qty = form.Get("TO_INV_QTY");
                    //var appdept = repo.GetDocnoAppdept("DOCNO");
                    string update_user = User.Identity.Name;
                    string update_ip = DBWork.ProcIP;
                    string supply_whno = repo.GetSupplyWhno(update_user);

                    // 若DGMISS的第一筆MMCODE是null, 則更新那筆資料, 否則新增一筆資料
                    if (repo.CheckIsFirst(docno))
                    {
                        repo.UpdateFirstD(docno, appqty, mmcode, inv_qty, supply_whno, update_user, update_ip);
                    }
                    else
                        repo.InsertD(docno, appqty, mmcode, inv_qty, supply_whno, update_user, update_ip);

                    // 更新DGMISS_INV的INV_QTY,若尚無資料則新增一筆
                    if (repo.CheckDgmissInv(docno, update_user, mmcode))
                        repo.UpdateDgmissInv(docno, mmcode, inv_qty, supply_whno, update_user, update_ip);
                    else
                        repo.InsertDgmissInv(docno, mmcode, inv_qty, supply_whno, update_user, update_ip);

                    // 嘗試更新可能重複MMCODE的其他項
                    repo.UpdateDinv(docno, mmcode, inv_qty);

                    DBWork.Commit();
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    string ex = e.ToString();
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse UpdateD(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0130Repository(DBWork);
                    string docno = form.Get("DOCNO");
                    string seq = form.Get("SEQ");
                    string mmcode = form.Get("MMCODE");
                    string appqty = form.Get("APPQTY");
                    string inv_qty = form.Get("TO_INV_QTY");
                    string update_user = User.Identity.Name;
                    string update_ip = DBWork.ProcIP;
                    string supply_whno = repo.GetSupplyWhno(update_user);

                    session.Result.afrs = repo.UpdateD(docno, seq, appqty, inv_qty, update_user, update_ip);

                    // 更新DGMISS_INV的INV_QTY,若尚無資料則新增一筆
                    if (repo.CheckDgmissInv(docno, update_user, mmcode))
                        repo.UpdateDgmissInv(docno, mmcode, inv_qty, supply_whno, update_user, update_ip);
                    else
                        repo.InsertDgmissInv(docno, mmcode, inv_qty, supply_whno, update_user, update_ip);

                    // 嘗試更新可能重複MMCODE的其他項
                    repo.UpdateDinv(docno, mmcode, inv_qty);

                    DBWork.Commit();
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    string ex = e.ToString();
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse DeleteD(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0130Repository repo = new AB0130Repository(DBWork);
                    string docno = form.Get("DOCNO");
                    string seq = form.Get("SEQ");

                    // 若為第一項則復原為沒有MMCODE的狀態;否則刪除該項
                    if (seq == "1")
                        repo.ClearD(docno);
                    else
                        repo.DeleteD(docno, seq);

                    DBWork.Commit();
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    string ex = e.ToString();
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse InsertM(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0130Repository repo = new AB0130Repository(DBWork);
                    string docnoEXE = repo.GetDocno();
                    string update_user = User.Identity.Name;
                    string update_ip = DBWork.ProcIP;
                    //新增DGMISS
                    repo.InsertDGMISS(docnoEXE, update_user, update_ip);
                    DBWork.Commit();
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    string ex = e.ToString();
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse UpdateM(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0130Repository repo = new AB0130Repository(DBWork);
                    string docno = form.Get("docno");
                    string appdept = form.Get("appdept");
                    string update_user = User.Identity.Name;
                    string update_ip = DBWork.ProcIP;

                    session.Result.afrs = repo.UpdateM(docno, appdept, update_user, update_ip);
                    DBWork.Commit();
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    string ex = e.ToString();
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse Cancel(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0130Repository repo = new AB0130Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        string msg = string.Empty;
                        string UPDATE_USER = User.Identity.Name;
                        string UPDATE_IP = DBWork.ProcIP;
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            session.Result.afrs = repo.Cancel(tmp[i].ToString(), UPDATE_USER, UPDATE_IP);
                        }
                    }
                    DBWork.Commit();
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    string ex = e.ToString();
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Apply(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0130Repository repo = new AB0130Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        string msg = string.Empty;
                        string UPDATE_USER = User.Identity.Name;
                        string UPDATE_IP = DBWork.ProcIP;
                        string supply_whno = repo.GetSupplyWhno_SupplyInid(docno);
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            if (repo.ChkDetailCnt(tmp[i].ToString()) == 0)
                            {
                                DBWork.Rollback();
                                session.Result.success = false;
                                session.Result.msg = "補藥單" + tmp[i].ToString() + "無明細資料,請確認!";
                                return session.Result;
                            }
                            else
                                session.Result.afrs = repo.Apply(tmp[i].ToString(), supply_whno, UPDATE_USER, UPDATE_IP);
                        }
                    }
                    DBWork.Commit();
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    string ex = e.ToString();
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetInidCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0130Repository repo = new AB0130Repository(DBWork);
                    session.Result.etts = repo.GetInidCombo();
                }
                catch (Exception e)
                {
                    string ex = e.ToString();
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetMMCodeDocd(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0130Repository repo = new AB0130Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeDocd(p0, p1, page, limit, "");
                }
                catch (Exception e)
                {
                    string ex = e.ToString();
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMmcode(FormDataCollection form)
        {
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0130Repository repo = new AB0130Repository(DBWork);
                    AB0130Repository.MI_MAST_QUERY_PARAMS query = new AB0130Repository.MI_MAST_QUERY_PARAMS();
                    query.MMCODE = form.Get("MMCODE") == null ? "" : form.Get("MMCODE").ToUpper();
                    query.MMNAME_C = form.Get("MMNAME_C") == null ? "" : form.Get("MMNAME_C").ToUpper();
                    query.MMNAME_E = form.Get("MMNAME_E") == null ? "" : form.Get("MMNAME_E").ToUpper();
                    query.WH_NO = form.Get("WH_NO") == null ? "" : form.Get("WH_NO").ToUpper();
                    query.MAT_CLASS = form.Get("MAT_CLASS") == null ? "" : form.Get("MAT_CLASS").ToUpper();
                    query.ISCONTID3 = form.Get("ISCONTID3") == null ? "" : form.Get("ISCONTID3").ToUpper();
                    query.M_AGENNO = form.Get("M_AGENNO") == null ? "" : form.Get("M_AGENNO").ToUpper();
                    query.AGEN_NAME = form.Get("AGEN_NAME") == null ? "" : form.Get("AGEN_NAME").ToUpper();

                    var appInid = form.Get("APP_INID") == null ? "" : form.Get("APP_INID");
                    var dgMastInid = repo.GetDgmissMast(appInid) == "1" ? appInid : ""; //如果DGMISS_MAST有值就傳入主檔的補藥單位
                    session.Result.etts = repo.GetMmcode(query, dgMastInid, page, limit, sorters);
                }
                catch (Exception e)
                {
                    string ex = e.ToString();
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse ChkDetailCnt(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0130Repository repo = new AB0130Repository(DBWork);
                    if (repo.ChkDetailCnt(form.Get("DOCNO")) == 0)
                        session.Result.msg = "N";
                    else
                        session.Result.msg = "Y";
                }
                catch (Exception e)
                {
                    string ex = e.ToString();
                    throw;
                }
                return session.Result;
            }
        }

    }
}