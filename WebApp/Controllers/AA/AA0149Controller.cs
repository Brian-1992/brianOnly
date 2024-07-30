using JCLib.DB;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using WebApp.Models;
using WebApp.Repository.AA;

namespace WebApp.Controllers.AA
{
    public class AA0149Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0149Repository(DBWork);
                    AA0149Repository.ME_DOCM_QUERY_PARAMS query = new AA0149Repository.ME_DOCM_QUERY_PARAMS();
                    query.MAT_CLASS = form.Get("p3");

                    string p2 = form.Get("p2");
                    string[] arr_p2 = { };
                    if (!string.IsNullOrEmpty(p2))
                    {
                        arr_p2 = p2.Trim().Split(','); //用,分割
                    }
                    string p3 = form.Get("p3");
                    string[] arr_p3 = { };
                    if (!string.IsNullOrEmpty(p3))
                    {
                        arr_p3 = p3.Trim().Split(','); //用,分割
                    }

                    query.APPTIME_S = "";
                    query.APPTIME_E = "";

                    if (form.Get("d0") != null && form.Get("d0") != "")
                        query.APPTIME_S = form.Get("d0").Split('T')[0];  // yyyy-mm-ddT00:00:00
                    if (form.Get("d1") != null && form.Get("d1") != "")
                        query.APPTIME_E = form.Get("d1").Split('T')[0];

                    query.USERID = DBWork.UserInfo.UserId;

                    session.Result.etts = repo.GetAll(query, arr_p2, arr_p3, page, limit, sorters);
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

        // 查詢
        [HttpPost]
        public ApiResponse AllMeDocd(FormDataCollection form)
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
                    var repo = new AA0149Repository(DBWork);
                    AA0149Repository.ME_DOCD_QUERY_PARAMS query = new AA0149Repository.ME_DOCD_QUERY_PARAMS();
                    query.DOCNO = form.Get("p0");
                    session.Result.etts = repo.GetAllMeDocd(query, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // 修改
        [HttpPost]
        public ApiResponse UpdateMeDocd(FormDataCollection form)
        {
            string items = form.Get("item");
            IEnumerable<ME_DOCD> docds = JsonConvert.DeserializeObject<IEnumerable<ME_DOCD>>(items);

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0149Repository repo = new AA0149Repository(DBWork);

                    foreach (ME_DOCD docd in docds)
                    {
                        docd.UPDATE_USER = User.Identity.Name;
                        docd.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.UpdateMeDocd(docd);
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
        public ApiResponse UpdateStatus(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0149Repository repo = new AA0149Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                      //  IEnumerable<ME_DOCD> data_list = JsonConvert.DeserializeObject<IEnumerable<ME_DOCD>>(form.Get("DATA_LIST"));
                        string[] tmp = docno.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            ME_DOCD me_docd = new ME_DOCD();
                            me_docd.DOCNO = tmp[i];
                            me_docd.UPDATE_USER = User.Identity.Name;
                            me_docd.UPDATE_IP = DBWork.ProcIP;
                            repo.UpdateApvqty(me_docd);

                            SP_MODEL sp = repo.PostDoc(tmp[i], User.Identity.Name, DBWork.ProcIP);
                            if (sp.O_RETID == "N")
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = sp.O_ERRMSG;
                            }
                            if (session.Result.success == false)
                            {
                                DBWork.Rollback();
                                return session.Result;
                            }
                            // 更新批號效期及儲位的庫存量
                            AA0147Controller cont = new AA0147Controller();
                            List<ME_DOCD> data_list = repo.GetMeDocExpWexpidNs(tmp[i]).ToList<ME_DOCD>(); ;

                            cont.updateExpLocInv(data_list, DBWork);

                        }
                        DBWork.Commit();
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

        [HttpPost]
        public ApiResponse TransferDoc(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0149Repository repo = new AA0149Repository(DBWork);

                    //Check

                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] strDocNoList = docno.Split(',');

                        foreach (string strDocNo in strDocNoList)
                        {
                            ME_DOCM mME_DOCM = repo.GetME_DOCM(strDocNo);
                            string v_docno = repo.GetDailyDocno(); // 取得系統自動生成的 編號

                            if (!repo.CheckExists(v_docno)) // 新增前檢查主鍵是否已存在
                            {
                                // 建立 退貨作業 主表
                                ME_DOCM newME_DOCUM = new ME_DOCM();
                                #region 建立主表
                                var v_twntime = repo.GetTwnsystime();
                                var v_matclass = mME_DOCM.MAT_CLASS;
                                var v_doctype = "RJ1";
                                var v_flowid = "1";
                                var v_whno = repo.GetWhno_mm1();
                                if (v_matclass == "01")
                                {
                                    v_doctype = "RJ";
                                    v_flowid = "0901";
                                    v_whno = repo.GetWhno_me1();
                                }

                                newME_DOCUM.DOCNO = v_docno;
                                newME_DOCUM.DOCTYPE = v_doctype;
                                newME_DOCUM.FLOWID = v_flowid;
                                newME_DOCUM.FLOWID_N = repo.GetFlowIdN(v_flowid);
                                newME_DOCUM.FRWH = v_whno;
                                newME_DOCUM.TOWH = v_whno;
                                newME_DOCUM.MAT_CLASS = mME_DOCM.MAT_CLASS;
                                newME_DOCUM.MAT_CLASS_N = repo.GetMAT_CLASS_N(mME_DOCM.MAT_CLASS);
                                newME_DOCUM.STKTRANSKIND = "1";
                                newME_DOCUM.APPID = User.Identity.Name;
                                newME_DOCUM.USEID = User.Identity.Name;
                                newME_DOCUM.CREATE_USER = User.Identity.Name;
                                newME_DOCUM.UPDATE_USER = User.Identity.Name;
                                mME_DOCM.UPDATE_IP = DBWork.ProcIP;
                                if (repo.CreateM(newME_DOCUM) == 0)
                                {
                                    session.Result.afrs = 0;
                                    session.Result.success = false;
                                    session.Result.msg = "新增失敗";
                                    return session.Result;
                                }
                                #endregion

                                // 建立 退貨作業 子表
                                #region  建立單據項目
                                ME_DOCEXP mME_DOCEXP = new ME_DOCEXP();
                                mME_DOCEXP.DOCNO = v_docno;
                                mME_DOCEXP.UPDATE_USER = User.Identity.Name;
                                mME_DOCEXP.UPDATE_IP = DBWork.ProcIP;
                                mME_DOCEXP.DOCNO_E = strDocNo;
                                session.Result.afrs = repo.CreateE(mME_DOCEXP);
                                #endregion
                            }
                            else
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = "<span style='color:red'>退貨單號</span>重複，請重新輸入。";
                            }
                        }
                    }
                    DBWork.Commit();
                }
                catch(Exception ex)
                {
                    DBWork.Rollback();
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
                    AA0149Repository repo = new AA0149Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            SP_MODEL sp = repo.PostDoc(tmp[i], User.Identity.Name, DBWork.ProcIP);
                            if (sp.O_RETID == "N")
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = sp.O_ERRMSG;
                                return session.Result;
                            }
                        }
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
        public ApiResponse MasterReject(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0149Repository repo = new AA0149Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            ME_DOCM me_docm = new ME_DOCM();
                            me_docm.DOCNO = tmp[i];
                            me_docm.UPDATE_USER = User.Identity.Name;
                            me_docm.UPDATE_IP = DBWork.ProcIP;
                            session.Result.afrs = repo.UpdateStatus(me_docm);  
                        }
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
        public ApiResponse GetInidComboQ()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0149Repository repo = new AA0149Repository(DBWork);
                    session.Result.etts = repo.GetInidComboQ();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetFrwhComboQ()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0149Repository repo = new AA0149Repository(DBWork);
                    session.Result.etts = repo.GetFrwhComboQ();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetTowhComboQ()
        {
            var p0 = User.Identity.Name;
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0149Repository repo = new AA0149Repository(DBWork);
                    session.Result.etts = repo.GetTowhComboQ(p0);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetFlowidCombo()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0149Repository repo = new AA0149Repository(DBWork);
                    session.Result.etts = repo.GetFlowidCombo(DBWork.UserInfo.UserId);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMatClassQCombo()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0149Repository repo = new AA0149Repository(DBWork);
                    session.Result.etts = repo.GetMatClassQCombo(DBWork.UserInfo.UserId);
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
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0149Repository repo = new AA0149Repository(DBWork);
                    session.Result.etts = repo.GetMatClassCombo(DBWork.UserInfo.UserId);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse CheckExp(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0149Repository(DBWork);
                    session.Result.success = repo.CheckExp(form.Get("DOCNO"));
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }
    }
}