using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Repository.C;
using WebApp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebApp.Controllers.AA
{
    public class AA0015Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {

            var p8 = form.Get("p8") == null ? "" : form.Get("p8").Replace(",", "");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0015Repository(DBWork);
                    AA0015Repository.ME_DOCM_QUERY_PARAMS query = new AA0015Repository.ME_DOCM_QUERY_PARAMS();
                    query.DOCNO = form.Get("p1") == null ? "" : form.Get("p1");
                    query.APPID = form.Get("p2") == null ? "" : form.Get("p2").ToUpper();
                    query.APPDEPT = form.Get("p3") == null ? "" : form.Get("p3").ToUpper();
                    //query.USEDEPT = form.Get("p4").ToUpper();
                    query.FRWH = form.Get("p5") == null ? "" : form.Get("p5").ToUpper();
                    query.TOWH = form.Get("p6") == null ? "" : form.Get("p6").ToUpper();

                    string wh_no = "";
                    IEnumerable<string> myEnum = repo.GetWhid(User.Identity.Name);
                    myEnum.GetEnumerator();
                    int i = 0;
                    foreach (var item in myEnum)
                    {
                        if (i == 0)
                            wh_no += item.ToString();
                        else
                            wh_no += "," + item.ToString();
                        i++;
                    }
                    query.WH_NO = wh_no;

                    query.APPTIME_S = "";
                    query.APPTIME_E = "";

                    if (form.Get("d0") != null && form.Get("d0") != "")
                        query.APPTIME_S = form.Get("d0").Split('T')[0];  // yyyy-mm-ddT00:00:00
                    if (form.Get("d1") != null && form.Get("d1") != "")
                        query.APPTIME_E = form.Get("d1").Split('T')[0];

                    query.DOCTYPE = "MR,MS";
                    //query.FLOWID = "0102,0602,0103,0603,0104,0604";
                    query.FLOWID = form.Get("FLOWID") == null ? "0102,0602,0103,0603,0104,0604,0199,0699" : form.Get("FLOWID");

                    session.Result.etts = repo.GetAll(query, p8, DBWork.UserInfo.UserId, page, limit, sorters);
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
            var p8 = form.Get("p8") == null ? "" : form.Get("p8").Replace(",", "");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0015Repository(DBWork);
                    AA0015Repository.ME_DOCD_QUERY_PARAMS query = new AA0015Repository.ME_DOCD_QUERY_PARAMS();
                    query.DOCNO = form.Get("p0");
                    query.WH_NO = form.Get("p1");

                    session.Result.etts = repo.GetAllMeDocd(query, p8);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // 修改
        //[HttpPost]
        //public ApiResponse UpdateMeDocd()
        //{
        //    using (WorkSession session = new WorkSession(this))
        //    {
        //        UnitOfWork DBWork = session.UnitOfWork;
        //        DBWork.BeginTransaction();
        //        try
        //        {
        //            AA0015Repository repo = new AA0015Repository(DBWork);
        //            HttpContent requestContent = Request.Content;
        //            string jsonContent = requestContent.ReadAsStringAsync().Result;
        //            //NM_CONTACT contact = JsonConvert.DeserializeObject<NM_CONTACT>(jsonContent);
        //            JObject obj = JsonConvert.DeserializeObject<JObject>(jsonContent);          // 先解第一層 {"item":[{"id":24,"part_no":"12223"},{...}]}
        //            JArray ja = JsonConvert.DeserializeObject<JArray>(obj["item"].ToString());  // 解第二層
        //            ME_DOCD me_docd = JsonConvert.DeserializeObject<ME_DOCD>(ja[0].ToString());
        //            me_docd.UPDATE_USER = User.Identity.Name;
        //            me_docd.UPDATE_IP = DBWork.ProcIP;
        //            session.Result.afrs = repo.UpdateMeDocd(me_docd);

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
        [HttpPost]
        public ApiResponse UpdateMeDocd(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0015Repository repo = new AA0015Repository(DBWork);

                    if (repo.CheckWhidValid(form.Get("DOCNO"), DBWork.UserInfo.UserId) == false)
                    {
                        session.Result.success = false;
                        session.Result.msg = "無此庫房權限，請重新確認";
                        return session.Result;
                    }

                    ME_DOCD me_docd = new ME_DOCD();
                    me_docd.DOCNO = form.Get("DOCNO");
                    me_docd.SEQ = form.Get("SEQ");
                    me_docd.APVQTY = form.Get("APVQTY");
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

        // 修改
        [HttpPost]
        public ApiResponse UpdateMeDocmStatus(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0015Repository repo = new AA0015Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            if (repo.CheckWhidValid(tmp[i], DBWork.UserInfo.UserId) == false)
                            {
                                session.Result.success = false;
                                session.Result.msg = "無此庫房權限，請重新確認";
                                return session.Result;
                            }

                            ME_DOCM me_docm = new ME_DOCM();
                            me_docm.DOCNO = tmp[i];
                            me_docm.FLOWID = "0103";    // 0103-藥局點收中
                            me_docm.UPDATE_USER = User.Identity.Name;
                            me_docm.UPDATE_IP = DBWork.ProcIP;
                            session.Result.afrs = repo.UpdateMeDocmStatus(me_docm);
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
                    AA0015Repository repo = new AA0015Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            if (repo.CheckWhidValid(tmp[i], DBWork.UserInfo.UserId) == false) {
                                session.Result.success = false;
                                session.Result.msg = "無此庫房權限，請重新確認";
                                return session.Result;
                            }

                            string flowid = repo.GetFlowid(tmp[i]);
                            if (flowid == "0102" || flowid == "0602")
                            {
                                ME_DOCD me_docd = new ME_DOCD();
                                me_docd.DOCNO = tmp[i];
                                me_docd.POSTID = "3";   // 待核撥
                                me_docd.UPDATE_USER = User.Identity.Name;
                                me_docd.UPDATE_IP = DBWork.ProcIP;
                                repo.UpdateAllPostid(me_docd);
                            }

                            SP_MODEL sp = repo.PostDoc(tmp[i], User.Identity.Name, DBWork.ProcIP);
                            if (sp.O_RETID == "N")
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = sp.O_ERRMSG;
                                return session.Result;
                            }

                            // 點收量預帶核撥量
                            repo.defAckqty(tmp[i]);

                            //// 同步批號效期資料到PDA揀貨資料
                            //CD0004Repository repo2 = new CD0004Repository(DBWork);
                            //repo2.CreateWhpickValidByPc(tmp[i], User.Identity.Name, DBWork.ProcIP);
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
        public ApiResponse UpdateStatusBySP2(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0015Repository repo = new AA0015Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            if (repo.CheckWhidValid(tmp[i], DBWork.UserInfo.UserId) == false)
                            {
                                session.Result.success = false;
                                session.Result.msg = "無此庫房權限，請重新確認";
                                return session.Result;
                            }

                            string flowid = repo.GetFlowid(tmp[i]);

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
        public ApiResponse UpdatePostidBySP(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0015Repository repo = new AA0015Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string seq = form.Get("SEQ").Substring(0, form.Get("SEQ").Length - 1);
                        string[] tmp = docno.Split(',');
                        string[] tmp_seq = seq.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            if (repo.CheckWhidValid(tmp[i], DBWork.UserInfo.UserId) == false)
                            {
                                session.Result.success = false;
                                session.Result.msg = "無此庫房權限，請重新確認";
                                return session.Result;
                            }

                            string flowid = repo.GetFlowid(tmp[i]);
                            if (flowid == "0102" || flowid == "0602")
                            {
                                ME_DOCD me_docd = new ME_DOCD();
                                me_docd.DOCNO = tmp[i];
                                me_docd.SEQ = tmp_seq[i];
                                me_docd.POSTID = "3";   // 待核撥
                                me_docd.UPDATE_USER = User.Identity.Name;
                                me_docd.UPDATE_IP = DBWork.ProcIP;
                                repo.UpdatePostid(me_docd);

                                // 同步資料到PDA揀貨單
                                CD0004Repository repo2 = new CD0004Repository(DBWork);
                                repo2.insertNewWhpickDocByPc(me_docd.DOCNO);

                                // POST_DOC一律會建立BC_WHPICK,
                                // 若曾用PDA載入,BC_WHPICK會有資料,這裡先做刪除
                                repo2.DeleteWhpick(me_docd.DOCNO, me_docd.SEQ);
                            }

                            // 3 -> C 已核撥
                            SP_MODEL sp = repo.PostDoc(tmp[i], User.Identity.Name, DBWork.ProcIP);
                            if (sp.O_RETID == "N")
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = sp.O_ERRMSG;
                                return session.Result;
                            }

                            // 點收量預帶核撥量
                            repo.defAckqty(tmp[i], tmp_seq[i]);
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
        public ApiResponse CancelPostidBySP(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0015Repository repo = new AA0015Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string seq = form.Get("SEQ").Substring(0, form.Get("SEQ").Length - 1);
                        string[] tmp = docno.Split(',');
                        string[] tmp_seq = seq.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            if (repo.CheckWhidValid(tmp[i], DBWork.UserInfo.UserId) == false)
                            {
                                session.Result.success = false;
                                session.Result.msg = "無此庫房權限，請重新確認";
                                return session.Result;
                            }

                            string flowid = repo.GetFlowid(tmp[i]);
                            // 2020-04-27 依據少如大哥來信增加 FLOWID(流程代碼)= 0103、0603時，按【取消揀料】，更新posid(藥品過帳註記) = '2'
                            if (flowid == "0102" || flowid == "0602" || flowid == "0103" || flowid == "0603")
                            {
                                ME_DOCD me_docd = new ME_DOCD();
                                me_docd.DOCNO = tmp[i];
                                me_docd.SEQ = tmp_seq[i];
                                me_docd.POSTID = "2";
                                me_docd.UPDATE_USER = User.Identity.Name;
                                me_docd.UPDATE_IP = DBWork.ProcIP;
                                repo.CancelPostid(me_docd);
                            }

                            SP_MODEL sp = repo.PostDoc(tmp[i], User.Identity.Name, DBWork.ProcIP);
                            if (sp.O_RETID == "N")
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = sp.O_ERRMSG;
                                return session.Result;
                            }

                            // 刪除ME_DOCEXP的資料
                            repo.DeleteDocexp(tmp[i], tmp_seq[i]);

                            CD0004Repository repo2 = new CD0004Repository(DBWork);
                            // POSTDOC在完成揀料(取消揀料時也會...)時會新增項目到BC_WHPICK和BC_WHPICK_VALID,取消時需刪除
                            repo2.DeleteWhpickValid(tmp[i], tmp_seq[i]);
                            repo2.DeleteWhpick(tmp[i], tmp_seq[i]);
                            // PDA揀貨後會做出庫,這邊也要刪除相關資料
                            repo2.deleteWhpickShipout(tmp[i], tmp_seq[i]);

                            // 核撥量預帶申請量,清空揀料時間,點收數量
                            repo.clearApvqty(tmp[i], tmp_seq[i]);
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
        public ApiResponse Excel(FormDataCollection form)
        {
            var p0 = form.Get("P0");
            var p1 = form.Get("P1").Trim();
            //var p1_name = form.Get("P1_Name").Trim();
            //var p2 = form.Get("P2").Trim();

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0015Repository repo = new AA0015Repository(DBWork);
                    DataTable result = new DataTable();
                    DataTable dtItems = new DataTable();

                    result = repo.GetExcel(p0, p1, DBWork.UserInfo.UserId);
                    dtItems.Merge(result);

                    //string str_UserDept = DBWork.UserInfo.InidName;
                    //string export_FileName = str_UserDept + p1_name + "藥品核撥報表";
                    string export_FileName = p0 + "藥品核撥報表";
                    string title = "申請單號 " + p0 + " 藥品核撥報表";
                    JCLib.Excel.Export(export_FileName + ".xls", dtItems, (tmp_dt) => { return title; });
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetPostidCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0015Repository repo = new AA0015Repository(DBWork);
                    session.Result.etts = repo.GetPostidCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetAppdeptCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0015Repository repo = new AA0015Repository(DBWork);
                    session.Result.etts = repo.GetAppdeptCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetUsedeptCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0015Repository repo = new AA0015Repository(DBWork);
                    session.Result.etts = repo.GetUsedeptCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetFrwhCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0015Repository repo = new AA0015Repository(DBWork);
                    session.Result.etts = repo.GetFrwhCombo(User.Identity.Name);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetTowhCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0015Repository repo = new AA0015Repository(DBWork);
                    session.Result.etts = repo.GetTowhCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetDocnoCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0015Repository repo = new AA0015Repository(DBWork);
                    session.Result.etts = repo.GetDocnoCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse ChkWexpid(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0015Repository repo = new AA0015Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        string rtnMsg = "";
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            if (repo.ChkWexpid(tmp[i]) > 0)
                            {
                                rtnMsg = tmp[i];
                                break;
                            }
                            else
                                rtnMsg = "Y";
                        }

                        session.Result.afrs = 0;
                        session.Result.success = true;
                        session.Result.msg = rtnMsg;
                        return session.Result;
                    }

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse ChkWexpidDetail(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0015Repository repo = new AA0015Repository(DBWork);
                    if (form.Get("DOCNO") != "" && form.Get("SEQ") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string seq = form.Get("SEQ").Substring(0, form.Get("SEQ").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmpDocno = docno.Split(',');
                        string[] tmpSeq = seq.Split(',');
                        string rtnMsg = "";
                        for (int i = 0; i < tmpSeq.Length; i++)
                        {
                            if (repo.ChkWexpid(tmpDocno[i], tmpSeq[i]) > 0)
                            {
                                rtnMsg = tmpSeq[i];
                                break;
                            }
                            else
                                rtnMsg = "Y";
                        }

                        session.Result.afrs = 0;
                        session.Result.success = true;
                        session.Result.msg = rtnMsg;
                        return session.Result;
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
        public ApiResponse ChkDefSort(FormDataCollection form)
        {
            var wh_no = form.Get("wh_no");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0015Repository(DBWork);
                    session.Result.msg = repo.chkDefSort(wh_no);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }


        #region 2020-07-20: 新增退回功能

        [HttpPost]
        public ApiResponse Return(FormDataCollection form)
        {
            string docno = form.Get("docno");
            string return_note = form.Get("return_note");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();

                try
                {
                    AA0015Repository repo = new AA0015Repository(DBWork);

                    if (repo.CheckWhidValid(docno, DBWork.UserInfo.UserId) == false)
                    {
                        session.Result.success = false;
                        session.Result.msg = "無此庫房權限，請重新確認";
                        return session.Result;
                    }

                    session.Result.afrs = repo.Return(docno, return_note, DBWork.UserInfo.UserName, DBWork.UserInfo.UserId, DBWork.ProcIP);

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

        #endregion
    }
}