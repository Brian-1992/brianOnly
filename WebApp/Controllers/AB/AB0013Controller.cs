using System;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Net.Http;
using JCLib.DB;
using WebApp.Repository.AB;
using WebApp.Models;
using System.Diagnostics;
using System.Collections.Generic;
using MMSMSREPORT.MMReport;
using MMSMSREPORT.Models;
namespace WebApp.Controllers.AB
{
    public class AB0013Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse QueryMEDOCD(FormDataCollection form)
        {
            var p0 = form.Get("p0"); // 申請單號
            var p1 = form.Get("p1"); // 申請單號

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");


            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0013Repository(DBWork);
                    session.Result.etts = repo.QueryMEDOCD(p0, p1, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        public ApiResponse UpdateEnd(FormDataCollection form)
        {
            string dno = form.Get("docno");
            using (WorkSession session = new WorkSession(this))
            {
                ;
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    string UPUSER = User.Identity.Name;
                    string UIP = DBWork.ProcIP;
                    AB0013Repository repo = new AB0013Repository(DBWork);

                    // session.Result.afrs = repo.UpdateEnd(medocm);
                    session.Result.afrs = repo.UpdateEnd(dno, UPUSER, UIP);
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
        [HttpPost]
        public ApiResponse MasterCreate(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0013Repository repo = new AB0013Repository(DBWork);
                    AB0010Repository repo1 = new AB0010Repository(DBWork);
                    ME_DOCM me_docm = new ME_DOCM();
                    me_docm.DOCNO = repo1.GetDocno();

                    // string frwh = repo.GetDrugWhnoByINID(DBWork.UserInfo.Inid);

                    if (!repo1.CheckExists(me_docm.DOCNO))
                    {
                        me_docm.CREATE_USER = User.Identity.Name;
                        me_docm.UPDATE_USER = User.Identity.Name;
                        me_docm.UPDATE_IP = DBWork.ProcIP;
                        me_docm.APPID = User.Identity.Name;
                        me_docm.APPDEPT = form.Get("INID_NAME").Split(' ')[0];
                        me_docm.USEID = User.Identity.Name;
                        me_docm.TOWH = form.Get("TOWH");        // 申請庫房
                        me_docm.FRWH = form.Get("FRWH"); // frwh;//form.Get("FRWH") == null ? "" : form.Get("FRWH");        // 核撥庫房
                        me_docm.STKTRANSKIND = form.Get("STKTRANSKIND");
                        me_docm.DOCTYPE = "RR";
                        me_docm.FLOWID = "1301";

                        session.Result.afrs = repo.MasterCreate(me_docm);
                        session.Result.etts = repo.MasterGet(me_docm.DOCNO);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>單據號碼</span>重複，請重新嘗試。";
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
        public ApiResponse MasterUpdate(ME_DOCM me_docm)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0013Repository repo = new AB0013Repository(DBWork);
                    AB0010Repository repo1 = new AB0010Repository(DBWork);
                    me_docm.UPDATE_USER = User.Identity.Name;
                    me_docm.UPDATE_IP = DBWork.ProcIP;

                    //string frwh = repo1.GetFrwh(me_docm.DOCNO);
                    //if (frwh == me_docm.FRWH.Trim()) // 如果核撥庫房一樣,則可以直接更新
                    //session.Result.afrs = repo.MasterUpdate(me_docm);
                    //else
                    //{
                    //if (!repo1.CheckMeDocdExists(me_docm.DOCNO)) // 傳入DOCNO檢查申請單是否有院內碼項次

                    // 更新時不允許更新異動類別,故這邊update不處理該欄位(要注意前端異動類別沒有透過選單選取時,會直接傳入類別名稱)
                    session.Result.afrs = repo.MasterUpdate(me_docm);
                    //    else
                    //    {
                    //        session.Result.afrs = 0;
                    //        session.Result.success = false;
                    //        session.Result.msg = "<span style='color:red'>申請單號「" + me_docm.DOCNO + "」已存在" + frwh + "庫房院內碼項次，所以無法修改核撥庫房</span><br>如欲修改核撥庫房，請先刪除所有項次。";
                    //        return session.Result;
                    //    }
                    //}


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
        public ApiResponse CreateD(ME_DOCM medocm)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0013Repository(DBWork);

                    if (!repo.CheckExistsD(medocm.DOCNO)) // 新增前檢查主鍵是否已存在
                    {
                        medocm.SEQ = "1";
                    }
                    else
                    {
                        medocm.SEQ = repo.GetDocDSeq(medocm.DOCNO);
                    }

                    try
                    {
                        MMRep_AB0013 ab0013 = new MMRep_AB0013();
                        if (medocm.MEDNO != null && medocm.MEDNO != "")
                        {
                            IEnumerable<ME_AB0013Modles> result = ab0013.GetDrugReissue<ME_AB0013Modles>(medocm.MMCODE, medocm.MEDNO);
                            result.GetEnumerator();
                            foreach (var item in result)
                            {
                                medocm.MEDNO = item.MEDNO;
                                medocm.BEDNO = item.BEDNO;
                                medocm.CHINNAME = item.CHINNAME;
                                medocm.ORDERDATE = item.CREATEDATETIME;
                            }
                        }
                    }
                    catch { }
                    
                    //ME_DOCD.APVID = User.Identity.Name;
                    //ME_DOCD.ACKID = User.Identity.Name;
                    medocm.CREATE_USER = User.Identity.Name;
                    medocm.UPDATE_USER = User.Identity.Name;
                    medocm.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.CreateD(medocm);
                    session.Result.etts = repo.GetD(medocm.DOCNO, medocm.STKTRANSKIND);
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
        public ApiResponse MasterDelete(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0012Repository repo = new AB0012Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            //if (!repo.CheckMeDocdExists(tmp[i])) // 傳入DOCNO檢查申請單是否有院內碼項次
                            //{
                            repo.DetailAllDelete(tmp[i]);
                            session.Result.afrs = repo.MasterDelete(tmp[i]);
                            //}
                            //else
                            //{
                            //    session.Result.afrs = 0;
                            //    session.Result.success = false;
                            //    session.Result.msg = "<span style='color:red'>申請單號「" + tmp[i] + "」有院內碼項次，請先刪除所有項次。</span>";
                            //    return session.Result;
                            //}
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
        public ApiResponse UpdateD(ME_DOCM medocm)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    MMRep_AB0013 ab0013 = new MMRep_AB0013();
                    //IEnumerable<ME_AB0013Modles> result = ab0013.GetDrugReissue_TEST<ME_AB0013Modles>();
                    var repo = new AB0013Repository(DBWork);
                    //讀取病歷資料需到三總測試
                    try
                    {
                        if (medocm.MEDNO != null && medocm.MEDNO != "")
                        {
                            IEnumerable<ME_AB0013Modles> result = ab0013.GetDrugReissue<ME_AB0013Modles>(medocm.MMCODE, medocm.MEDNO);
                            result.GetEnumerator();
                            foreach (var item in result)
                            {
                                medocm.MEDNO = item.MEDNO;
                                medocm.BEDNO = item.BEDNO;
                                medocm.CHINNAME = item.CHINNAME;
                                medocm.ORDERDATE = item.CREATEDATETIME;
                            }
                        }
                    }
                    catch
                    {

                    }
                    //讀取病歷資料
                    // medocm.GTAPL_RESON = medocm.GTAPL_RESON.Substring(0, 2);
                    medocm.GTAPL_RESON = medocm.GTAPL_RESON;
                    medocm.UPDATE_USER = User.Identity.Name;
                    medocm.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.UpdateD(medocm);
                    if (medocm.STKTRANSKIND == null)
                    {
                        medocm.STKTRANSKIND = "一般藥";
                    }
                    session.Result.etts = repo.GetD(medocm.DOCNO, medocm.STKTRANSKIND);

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
        public ApiResponse DeleteD(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0013Repository repo = new AB0013Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO"); // 去除前端傳進來最後一個逗號
                        string seq = form.Get("SEQ");

                        session.Result.afrs = repo.DeleteD(docno, seq);
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

        public ApiResponse GetWhnoCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0013Repository(DBWork);
                    session.Result.etts = repo.GetWhnoCombo(User.Identity.Name);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        public ApiResponse GetFrwhCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0013Repository(DBWork);
                    session.Result.etts = repo.GetFrwhCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetMMCodeCombo(FormDataCollection form)
        {
            var p0 = form.Get("p1");
            var p1 = form.Get("p2");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0013Repository repo = new AB0013Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeCombo(p0, p1, page, limit, "");
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