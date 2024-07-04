using System.Collections.Generic;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Net.Http;
using JCLib.DB;
using WebApp.Repository.AB;
using WebApp.Models;
using System.Diagnostics;

namespace WebApp.Controllers.AB
{
    public class AB0018Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse QueryME(FormDataCollection form)
        {
            var p0 = form.Get("p0"); // 申請單號
            var p1 = form.Get("p1"); // 申請日(起)
            var p2 = form.Get("p2"); // 申請日(迄)
            var p3 = form.Get("p3"); // 核撥庫房
            //var p4 = form.Get("p4"); // 申請人員
            //var p5 = form.Get("p5");// 申請部門
            //var p6 = form.Get("p6");// 使用部門
            var p7 = form.Get("p7");// 領用庫房
            var p8 = form.Get("p8");// 狀態
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            var v_user = User.Identity.Name;
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0018Repository(DBWork);

                    //p7 = "";
                    //IEnumerable<MI_WHID> myEnum = repo.GetWhid(User.Identity.Name);
                    //myEnum.GetEnumerator();
                    //int i = 0;
                    //foreach (var item in myEnum)
                    //{
                    //    if (i == 0)
                    //        p7 += item.WH_NO;
                    //    else
                    //        p7 += "," + item.WH_NO;
                    //    i++;
                    //}

                    string[] arr_p8 = { };
                    if (!string.IsNullOrEmpty(p8))
                    {
                        arr_p8 = p8.Trim().Split(','); //用,分割
                    }

                    var tflow = repo.GetTflow(User.Identity.Name);

                    session.Result.etts = repo.QueryME(p0, p1, p2, p3, p7, arr_p8, v_user, tflow, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse QueryMEDOCD(FormDataCollection form)
        {
            var p0 = form.Get("p0"); // 申請單號

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");


            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0018Repository(DBWork);
                    session.Result.etts = repo.QueryMEDOCD(p0, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public IHttpActionResult CheckMEDOCC(FormDataCollection form)//(string p0, string p1, string p2, string p3, string p4, string sort = null, int? page = 1, int? start = 1, int? limit = 99999)
        {
            var p0 = form.Get("p0"); // 報價單號
            var p1 = form.Get("p1");
            p1 = p1.ToString().PadLeft(5, '0');

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                AB0018Repository repo = new AB0018Repository(DBWork);
                //int rfq = repo.CHECKRFQ(p0, p1);
                var result = repo.CheckMEDOCC(p0, p1);
                if (result == null)
                {
                    return Ok(result);
                }
                return Ok(result);
            }
        }
        public IHttpActionResult CheckMEDOCM(FormDataCollection form)//(string p0, string p1, string p2, string p3, string p4, string sort = null, int? page = 1, int? start = 1, int? limit = 99999)
        {
            var p0 = form.Get("p0"); 

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                AB0018Repository repo = new AB0018Repository(DBWork);
                var result = repo.CheckMEDOCM(p0);
                if (result == null)
                {
                    return Ok(result);
                }
                return Ok(result);
            }
        }

        public ApiResponse Update(ME_DOCM medocm)
        {
            using (WorkSession session = new WorkSession(this))
            {
                //List<QUOTN> list = new List<QUOTN>();
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0018Repository repo = new AB0018Repository(DBWork);
                    medocm.CREATE_USER = User.Identity.Name;
                    medocm.UPDATE_USER = User.Identity.Name;
                    medocm.UPDATE_IP = DBWork.ProcIP;
                    medocm.CHECKSEQ = 0;
                    medocm.GENWAY = "1";
                    session.Result.afrs += repo.Update(medocm); //update ME_DOCD,ME_DOCC
                    session.Result.afrs += repo.UpdateMI_WINVCTL(medocm.DOCNO, medocm.TOWH_NO, medocm.MMCODE);                    
                    session.Result.afrs += repo.Create(medocm);
                    DBWork.Commit();
                }
                catch when (!Debugger.IsAttached)
                {
                    DBWork.Rollback();
                    //throw;
                }
                return session.Result;
            }
        }

        public ApiResponse UpdateC(ME_DOCM medocm) //單筆點收時呼叫
        {
            using (WorkSession session = new WorkSession(this))
            {
                //List<QUOTN> list = new List<QUOTN>();
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0018Repository repo = new AB0018Repository(DBWork);
                    medocm.CREATE_USER = User.Identity.Name;
                    medocm.UPDATE_USER = User.Identity.Name;
                    medocm.UPDATE_IP = DBWork.ProcIP;
                    if (medocm.UPDATE_IP == null)
                    {
                        medocm.UPDATE_IP = "0.0.0.0";
                    }
                    medocm.CHECKSEQ = 0;
                    medocm.GENWAY = "1";
                    if (medocm.ACKQTY == medocm.APVQTY)
                    {
                        session.Result.afrs += repo.UpdateE(medocm); //update ME_DOCD,ME_DOCC
                        session.Result.afrs += repo.UpdateMI_WINVCTL(medocm.DOCNO, medocm.TOWH_NO, medocm.MMCODE);
                    }
                    else
                    {
                        session.Result.afrs += repo.Update(medocm); //update ME_DOCD,ME_DOCC
                        session.Result.afrs += repo.UpdateMI_WINVCTL(medocm.DOCNO, medocm.TOWH_NO, medocm.MMCODE);
                    }
                    //session.Result.afrs = repo.Create(medocm);

                    DBWork.Commit();
                }
                catch when (!Debugger.IsAttached)
                {
                    DBWork.Rollback();
                    //throw;
                }
                return session.Result;
            }
        }

        public ApiResponse ApplyD(FormDataCollection form)//單筆接收
        {
            string dno = form.Get("p0");
            string submitSeq = form.Get("p1");
            string[] allData = submitSeq.Trim().Split('ˋ');

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;

                DBWork.BeginTransaction();
                try
                {
                    string UPUSER = User.Identity.Name;
                    string UIP = DBWork.ProcIP;
                    AB0018Repository repo = new AB0018Repository(DBWork);

                    for (int i = 0; i < allData.Length; i++)
                    {
                        ME_DOCD medocd = new ME_DOCD();
                        medocd.DOCNO = dno;
                        medocd.SEQ = allData[i].Split('^')[0];
                        medocd.ACKQTY = allData[i].Split('^')[1];
                        medocd.TOWH_NO = allData[i].Split('^')[2];
                        medocd.MMCODE = allData[i].Split('^')[3];
                        medocd.CREATE_USER = User.Identity.Name;
                        medocd.UPDATE_USER = User.Identity.Name;
                        medocd.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs += repo.Updated(medocd); //POSTID=4
                        session.Result.afrs += repo.UpdateMI_WINVCTL(dno, medocd.TOWH_NO, medocd.MMCODE); 
                    }
                    
                    DBWork.Commit();
                    SP_MODEL sp = repo.POST_DOC(dno, UPUSER, UIP);
                    if (sp.O_RETID == "N")
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = sp.O_ERRMSG;
                        return session.Result;
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
        public ApiResponse UpdateEnd(FormDataCollection form) //結案
        {
            string dno = form.Get("dno");
            string towh_no = form.Get("towh_no");
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;

                DBWork.BeginTransaction();
                try
                {
                    string UPUSER = User.Identity.Name;
                    string UIP = DBWork.ProcIP;
                    AB0018Repository repo = new AB0018Repository(DBWork);

                    //session.Result.afrs = repo.UpdateEnd(dno, UPUSER, UIP);
                    session.Result.afrs += repo.UpdatePostid4(dno, UPUSER, UIP); //update ME_DOCD
                    session.Result.afrs += repo.UpdateMI_WINVCTL(dno, towh_no, "");
                    SP_MODEL sp = repo.POST_DOC(dno, UPUSER, UIP);

                    if (sp.O_RETID == "N")
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = sp.O_ERRMSG;
                        DBWork.Rollback();
                        return session.Result;
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
        public ApiResponse QueryMEDOCC(FormDataCollection form)
        {
            var p0 = form.Get("p0"); // 申請單號

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");


            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0018Repository(DBWork);
                    session.Result.etts = repo.QueryMEDOCC(p0, page, limit, sorters);
                }
                catch when (!Debugger.IsAttached)
                {

                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetStatusCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0018Repository repo = new AB0018Repository(DBWork);
                    session.Result.etts = repo.GetStatusCombo();
                }
                catch when (!Debugger.IsAttached)
                {

                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetTowhCombo()
        {
            var p0 = User.Identity.Name;
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0018Repository repo = new AB0018Repository(DBWork);
                    session.Result.etts = repo.GetTowhCombo(p0);
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
            var p0 = User.Identity.Name;
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0018Repository repo = new AB0018Repository(DBWork);
                    session.Result.etts = repo.GetFrwhCombo(p0);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetLoginInfo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0018Repository repo = new AB0018Repository(DBWork);
                    var v_userid = User.Identity.Name;
                    session.Result.etts = repo.GetLoginInfo(v_userid);
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
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0018Repository repo = new AB0018Repository(DBWork);

                    var tflow = repo.GetTflow(User.Identity.Name);
                    if (tflow == "0")
                    {
                        session.Result.etts = repo.GetFlowidCombo();
                    }
                    else if (tflow == "1")
                    {
                        session.Result.etts = repo.GetFlowid1Combo();
                    }
                    else if (tflow == "6")
                    {
                        session.Result.etts = repo.GetFlowid6Combo();
                    }
                    else
                    {

                        session.Result.etts = repo.GetFlowidCombo();
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
        public ApiResponse UpdateMedocD(FormDataCollection form)
        {
            var DOCNO = form.Get("DOCNO");
            var SEQ = form.Get("SEQ");
            var ACKQTY = form.Get("ACKQTY");
            var MMCODE = form.Get("MMCODE");
            var TOWH_NO = form.Get("TOWH_NO");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0018Repository(DBWork);
                    ME_DOCD me_docd = new ME_DOCD();
                    me_docd.DOCNO = DOCNO;
                    me_docd.SEQ = SEQ;
                    if (repo.CheckExistsDKeyByUpd(me_docd.DOCNO, me_docd.SEQ))
                    {
                        me_docd.SEQ = SEQ;
                        me_docd.ACKQTY = ACKQTY;
                        me_docd.UPDATE_USER = User.Identity.Name;
                        me_docd.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs += repo.UpdateMedocD(me_docd);
                        session.Result.afrs += repo.UpdateMI_WINVCTL(DOCNO, TOWH_NO, MMCODE);
                        //session.Result.etts = repo.GetD(me_docd.DOCNO, me_docd.SEQ);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>無此項次</span>，請重新輸入。";

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
        public string GetUsertask()
        {
            string user_task = "";
            using (WorkSession session = new WorkSession())
            {

                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0018Repository repo = new AB0018Repository(DBWork);
                    user_task = repo.GetTaskid(User.Identity.Name);
                    return user_task;
                }
                catch
                {

                }

            }
            return user_task;
        }


        [HttpPost]
        public string GetUsertflow()
        {
            string user_task = "";
            using (WorkSession session = new WorkSession())
            {

                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0018Repository repo = new AB0018Repository(DBWork);
                    user_task = repo.GetTflow(User.Identity.Name);
                    return user_task;
                }
                catch
                {

                }

            }
            return user_task;
        }

        [HttpPost]
        public ApiResponse GetExcel(FormDataCollection form)
        {
            var p9 = form.Get("p9");   //申請單號
            var p10 = form.Get("p10"); //核撥庫房
            var p11 = form.Get("p11"); //申請日期 起
            var p12 = form.Get("p12"); //申請日期 迄
            var p13 = form.Get("p13"); //領用庫房
            var p14 = form.Get("p14"); //院內碼

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0018Repository(DBWork);
                    JCLib.Excel.Export(form.Get("FN"), repo.GetExcel(p9, p10, p11, p12, p13,p14)); //查詢點收差異資料匯出
                }
                catch
                {
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
                    AB0018Repository repo = new AB0018Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeDocd(p0,p1, page, limit, "");
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