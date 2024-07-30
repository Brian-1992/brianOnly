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
using WebApp.Repository.AB;

namespace WebApp.Controllers.AB
{
    public class AB0024Controller : SiteBase.BaseApiController
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
                    var repo = new AB0024Repository(DBWork);
                    AB0024Repository.ME_DOCM_QUERY_PARAMS query = new AB0024Repository.ME_DOCM_QUERY_PARAMS();
                    query.DOCNO = form.Get("p1") == null ? "" : form.Get("p1");
                    query.APPID = form.Get("p2") == null ? "" : form.Get("p2");
                    query.APPDEPT = form.Get("p3").ToUpper();
                    query.FROMDATE = form.Get("p7") == null || form.Get("p7").Trim() == string.Empty ? "" : DateTime.Parse(form.Get("p7")).ToString("yyyy-MM-dd");
                    query.TODATE = form.Get("p8") == null || form.Get("p8").Trim() == string.Empty ? "" : DateTime.Parse(form.Get("p8")).ToString("yyyy-MM-dd");
                    //query.USEDEPT = form.Get("p4").ToUpper();

                    //// 只顯示核撥庫房 = Login帳號可管理的庫房
                    //List<MI_WHID> whno = repo.GetWhnoById(DBWork.UserInfo.UserId);
                    //if (whno.Count > 0)
                    //{
                    //    string tmp = "";
                    //    for (int i = 0; i < whno.Count; i++)
                    //    {
                    //        if (i == 0)
                    //            tmp = "'" + whno[i].WH_NO + "'";
                    //        else
                    //            tmp += ",'" + whno[i].WH_NO + "'";
                    //    }
                    //    query.TOWH = tmp;
                    //}
                    query.FRWH = form.Get("p6").ToUpper();
                    query.TOWH = form.Get("p5").ToUpper();

                    query.DOCTYPE = "RN";
                    query.FLOWID = form.Get("FLOWID");
                    session.Result.etts = repo.GetAll(query, DBWork.UserInfo.UserId);
                }
                catch
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
                    var repo = new AB0024Repository(DBWork);
                    AB0024Repository.ME_DOCD_QUERY_PARAMS query = new AB0024Repository.ME_DOCD_QUERY_PARAMS();
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
                    AB0024Repository repo = new AB0024Repository(DBWork);

                    foreach (ME_DOCD docd in docds) {
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
                    AB0024Repository repo = new AB0024Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            ME_DOCD me_docd = new ME_DOCD();
                            me_docd.DOCNO = tmp[i];
                            me_docd.UPDATE_USER = User.Identity.Name;
                            me_docd.UPDATE_IP = DBWork.ProcIP;
                            repo.UpdateApvqty(me_docd); // 如果核撥量=0,則預設將核撥量=申請量

                            ME_DOCM me_docm = new ME_DOCM();
                            me_docm.DOCNO = tmp[i];
                            me_docm.FLOWID = "0499";
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
        public ApiResponse UpdateStatusBySP(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0024Repository repo = new AB0024Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            ME_DOCD me_docd = new ME_DOCD();
                            me_docd.DOCNO = tmp[i];
                            me_docd.UPDATE_USER = User.Identity.Name;
                            me_docd.UPDATE_IP = DBWork.ProcIP;
                            repo.UpdateApvqty(me_docd); // 如果核撥量=0,則預設將核撥量=申請量

                            SP_MODEL sp = repo.PostDoc(tmp[i], User.Identity.Name, DBWork.ProcIP);
                            if (sp.O_RETID == "N")
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = sp.O_ERRMSG;
                                return session.Result;
                            }

                            //ME_DOCM me_docm = new ME_DOCM();
                            //me_docm.DOCNO = tmp[i];
                            //me_docm.FLOWID = "0499";
                            //me_docm.UPDATE_USER = User.Identity.Name;
                            //me_docm.UPDATE_IP = DBWork.ProcIP;
                            //session.Result.afrs = repo.UpdateStatus(me_docm);

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
        public ApiResponse Return(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0024Repository repo = new AB0024Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            ME_DOCM me_docm = new ME_DOCM();
                            me_docm.DOCNO = tmp[i];
                            me_docm.FLOWID = "0401";
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
                    AB0024Repository repo = new AB0024Repository(DBWork);
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
                    AB0024Repository repo = new AB0024Repository(DBWork);
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
                    AB0024Repository repo = new AB0024Repository(DBWork);
                    session.Result.etts = repo.GetTowhComboQ(p0);
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