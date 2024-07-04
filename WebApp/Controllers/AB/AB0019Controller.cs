using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AB;
using WebApp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebApp.Controllers.AB
{
    public class AB0019Controller : SiteBase.BaseApiController
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
                    var repo = new AB0019Repository(DBWork);
                    AB0019Repository.ME_DOCM_QUERY_PARAMS query = new AB0019Repository.ME_DOCM_QUERY_PARAMS();
                    query.DOCNO = form.Get("p1") == null ? "" : form.Get("p1");
                    query.APPID = form.Get("p2") == null ? "" : form.Get("p2");
                    query.APPDEPT = form.Get("p3").ToUpper();
                    //query.USEDEPT = form.Get("p4").ToUpper();

                    // 只顯示核撥庫房 = Login帳號可管理的庫房
                    bool flag = false;
                    List<MI_WHID> whno = repo.GetWhnoById(DBWork.UserInfo.UserId);
                    if (whno.Count > 0)
                    {
                        string tmp = "";
                        for (int i = 0; i < whno.Count; i++)
                        {
                            if (whno[i].WH_NO == form.Get("p5").ToUpper() && form.Get("p5") != "")
                                flag = true;

                            if (i == 0)
                                tmp = "'" + whno[i].WH_NO + "'";
                            else
                                tmp += ",'" + whno[i].WH_NO + "'";
                        }
                        query.FRWH = tmp;
                    }
                    if (flag)
                        query.FRWH = "'" + form.Get("p5").ToUpper() + "'";
                    else
                    {
                        if (form.Get("p5") != "")
                            query.FRWH = "'XX'";
                    }
                    query.TOWH = form.Get("p6").ToUpper();

                    query.DOCTYPE = "MS";
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
                    var repo = new AB0019Repository(DBWork);
                    AB0019Repository.ME_DOCD_QUERY_PARAMS query = new AB0019Repository.ME_DOCD_QUERY_PARAMS();
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
        public ApiResponse UpdateMeDocd()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0019Repository repo = new AB0019Repository(DBWork);
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
                    AB0019Repository repo = new AB0019Repository(DBWork);
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
                            me_docm.FLOWID = "0699";    
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
                    AB0019Repository repo = new AB0019Repository(DBWork);
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
    }
}
