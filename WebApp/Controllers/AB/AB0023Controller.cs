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
    public class AB0023Controller : SiteBase.BaseApiController
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
                    var repo = new AB0023Repository(DBWork);
                    AB0023Repository.ME_DOCM_QUERY_PARAMS query = new AB0023Repository.ME_DOCM_QUERY_PARAMS();
                    query.DOCNO = form.Get("p1") == null ? "" : form.Get("p1");
                    query.TOWH = form.Get("p5") == null ? "" : form.Get("p5");
                    query.FRWH = form.Get("p6") == null ? "" : form.Get("p6");
                    query.APPTIME_B = form.Get("p7") == null ? "" : form.Get("p7");
                    query.APPTIME_E = form.Get("p8") == null ? "" : form.Get("p8");

                    // 只顯示核撥庫房 = Login帳號可管理的庫房
                    List<MI_WHID> whno = repo.GetWhnoById(DBWork.UserInfo.UserId);
                    if (string.IsNullOrEmpty(query.TOWH))
                    {
                        if (whno.Count > 0)
                        {
                            string tmp = "";
                            for (int i = 0; i < whno.Count; i++)
                            {
                                if (i == 0)
                                    tmp = "'" + whno[i].WH_NO + "'";
                                else
                                    tmp += ",'" + whno[i].WH_NO + "'";
                            }
                            query.TOWH = tmp;
                        }
                    }
                    else
                    {
                        query.TOWH = "'" + query.TOWH + "'";
                    }

                    query.DOCTYPE = "RS";
                    query.FLOWID = form.Get("FLOWID");
                    session.Result.etts = repo.GetAll(query, page, limit, sorters);
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
                    var repo = new AB0023Repository(DBWork);
                    AB0023Repository.ME_DOCD_QUERY_PARAMS query = new AB0023Repository.ME_DOCD_QUERY_PARAMS();
                    query.DOCNO = form.Get("p0");
                    query.STAT = form.Get("p1");
                    session.Result.etts = repo.GetAllMeDocd(query, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse UpdateM(ME_DOCM me_docm)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0023Repository(DBWork);
                    //me_mdfm.APP_USER = User.Identity.Name;
                    //me_mdfm.APP_INID = DBWork.UserInfo.Inid;
                    me_docm.UPDATE_TIME = DBWork.ProcIP;
                    me_docm.UPDATE_USER = User.Identity.Name;
                    me_docm.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.UpdateM(me_docm);

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
        public ApiResponse UpdateD(ME_DOCD me_docd)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0023Repository(DBWork);
                    //me_mdfm.APP_USER = User.Identity.Name;
                    //me_mdfm.APP_INID = DBWork.UserInfo.Inid;
                    me_docd.UPDATE_TIME = DBWork.ProcIP;
                    me_docd.UPDATE_USER = User.Identity.Name;
                    me_docd.UPDATE_IP = DBWork.ProcIP;

                    session.Result.afrs = repo.UpdateD(me_docd);
                    session.Result.etts = repo.DetailGet(me_docd);

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
        public ApiResponse AcceptD(ME_DOCD me_docd)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0023Repository(DBWork);
                    //me_mdfm.APP_USER = User.Identity.Name;
                    //me_mdfm.APP_INID = DBWork.UserInfo.Inid;
                    me_docd.UPDATE_TIME = DBWork.ProcIP;
                    me_docd.UPDATE_USER = User.Identity.Name;
                    me_docd.UPDATE_IP = DBWork.ProcIP;

                    string docno = me_docd.DOCNO.Split(',')[0];
                    string mmcode = me_docd.MMCODE.Substring(0, me_docd.MMCODE.Length - 1); // 去除前端傳進來最後一個逗號
                    string[] tmp = mmcode.Split(',');
                    for (int i = 0; i < tmp.Length; i++)
                    {
                        me_docd.DOCNO = docno;
                        me_docd.MMCODE = tmp[i];
                        session.Result.afrs = repo.AcceptD(me_docd);
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
        public ApiResponse ReturnD(ME_DOCD me_docd)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0023Repository(DBWork);
                    //me_mdfm.APP_USER = User.Identity.Name;
                    //me_mdfm.APP_INID = DBWork.UserInfo.Inid;
                    me_docd.UPDATE_TIME = DBWork.ProcIP;
                    me_docd.UPDATE_USER = User.Identity.Name;
                    me_docd.UPDATE_IP = DBWork.ProcIP;

                    string docno = me_docd.DOCNO.Split(',')[0];
                    string mmcode = me_docd.MMCODE.Substring(0, me_docd.MMCODE.Length - 1); // 去除前端傳進來最後一個逗號
                    string[] tmp = mmcode.Split(',');
                    for (int i = 0; i < tmp.Length; i++)
                    {
                        me_docd.DOCNO = docno;
                        me_docd.MMCODE = tmp[i];
                        session.Result.afrs = repo.ReturnD(me_docd);
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
        public ApiResponse UpdateMeDocd()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0023Repository repo = new AB0023Repository(DBWork);
                    HttpContent requestContent = Request.Content;
                    string jsonContent = requestContent.ReadAsStringAsync().Result;
                    //NM_CONTACT contact = JsonConvert.DeserializeObject<NM_CONTACT>(jsonContent);
                    JObject obj = JsonConvert.DeserializeObject<JObject>(jsonContent);          // 先解第一層 {"item":[{"id":24,"part_no":"12223"},{...}]}
                    JArray ja = JsonConvert.DeserializeObject<JArray>(obj["item"].ToString());  // 解第二層
                    ME_DOCD me_docd = JsonConvert.DeserializeObject<ME_DOCD>(ja[0].ToString());
                    me_docd.UPDATE_USER = User.Identity.Name;
                    me_docd.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.UpdateMeDocd(me_docd);

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
                    AB0023Repository repo = new AB0023Repository(DBWork);
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
                            me_docm.FLOWID = "1299";
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
                    AB0023Repository repo = new AB0023Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');

                        string statB_docnos = string.Empty;
                        for (int i = 0; i < tmp.Length; i++) {
                            int statB = repo.CheckDocdStatB(tmp[i]);
                            if (statB > 0) {
                                if (statB_docnos != string.Empty) {
                                    statB_docnos += "、";
                                }
                                statB_docnos += tmp[i];
                            }
                        }
                        if (statB_docnos != string.Empty) {
                            session.Result.success = false;
                            session.Result.msg = string.Format(@"以下申請單號尚有已退回明細資料，單據無法結案<br/>{0}", statB_docnos);
                            return session.Result;
                        }

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
        public ApiResponse AllMeBack(FormDataCollection form)
        {
            var docno = form.Get("p0");
            var rseq = form.Get("p1");
            var ordersort = form.Get("p2"); // for AB0101

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0023Repository(DBWork);
                    session.Result.etts = repo.GetMeBack(docno, rseq, ordersort);
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
                    AB0023Repository repo = new AB0023Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');

                        string statB_docnos = string.Empty;
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            int statB = repo.CheckDocdStatB(tmp[i]);
                            if (statB == 0)
                            {
                                if (statB_docnos != string.Empty)
                                {
                                    statB_docnos += "、";
                                }
                                statB_docnos += tmp[i];
                            }
                        }
                        if (statB_docnos != string.Empty)
                        {
                            session.Result.success = false;
                            session.Result.msg = string.Format(@"以下申請單號無已退回明細資料，單據無法退回<br/>{0}", statB_docnos);
                            return session.Result;
                        }


                        for (int i = 0; i < tmp.Length; i++)
                        {
                            ME_DOCM me_docm = new ME_DOCM();
                            me_docm.DOCNO = tmp[i];
                            me_docm.FLOWID = "1201";
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
        public ApiResponse GetTowh(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0023Repository(DBWork);
                    session.Result.etts = repo.GetTowh(User.Identity.Name)
                        .Select(w => new { WH_NO = w.WH_NO, WH_NAME = w.WH_NAME });
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetFrwh(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0023Repository(DBWork);
                    session.Result.etts = repo.GetFrwh()
                        .Select(w => new { WH_NO = w.WH_NO, WH_NAME = w.WH_NAME });
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