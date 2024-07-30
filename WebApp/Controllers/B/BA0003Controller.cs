using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.B;
using WebApp.Models;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace WebApp.Controllers.B
{
    public class BA0003Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse GetMST(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BA0003Repository(DBWork);
                    session.Result.etts = repo.GetMST(p0, p1, p2, p3, DBWork.UserInfo.Inid, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetD(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var wh_no = form.Get("wh_no");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    BA0003Repository repo = new BA0003Repository(DBWork);
                    session.Result.etts = repo.GetD(p0, p1, wh_no, page, limit, sorters);
                }
                catch
                {
                    throw;
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
                    BA0003Repository repo = new BA0003Repository(DBWork);
                    MM_PR_M mm_pr_m = new MM_PR_M();

                    mm_pr_m.PR_USER = User.Identity.Name;
                    mm_pr_m.CREATE_USER = User.Identity.Name;
                    mm_pr_m.UPDATE_IP = DBWork.ProcIP;
                    mm_pr_m.PR_DEPT = DBWork.UserInfo.Inid;
                    mm_pr_m.MAT_CLASS = form.Get("MAT_CLASS");
                    mm_pr_m.M_STOREID = form.Get("M_STOREID");

                    if (mm_pr_m.M_STOREID == "0")
                    {
                        session.Result.success = false;
                        session.Result.msg = "不可新增非庫備申購單";
                        return session.Result;
                    }


                    session.Result.msg = repo.MasterCreate(mm_pr_m);
                    //session.Result.etts = repo.Get(me_docm.DOCNO);

                    //session.Result.afrs = 0;
                    //session.Result.success = false;

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
                    BA0003Repository repo = new BA0003Repository(DBWork);
                    if (form.Get("PR_NO") != "")
                    {
                        repo.DetailDeleteAll(form.Get("PR_NO"));
                        session.Result.afrs = repo.MasterDelete(form.Get("PR_NO"));
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
        public ApiResponse MasterUpdate(MM_PR_M mm_pr_m)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BA0003Repository repo = new BA0003Repository(DBWork);
                    mm_pr_m.UPDATE_USER = User.Identity.Name;
                    mm_pr_m.UPDATE_IP = DBWork.ProcIP;

                    session.Result.afrs = repo.MasterUpdate(mm_pr_m);
                    //session.Result.etts = repo.Get(me_docm.DOCNO);

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


        //[HttpPost]
        //public ApiResponse AllMeDocd(FormDataCollection form)
        //{
        //    var page = int.Parse(form.Get("page"));
        //    var start = int.Parse(form.Get("start"));
        //    var limit = int.Parse(form.Get("limit"));
        //    var sorters = form.Get("sort");

        //    using (WorkSession session = new WorkSession())
        //    {
        //        var DBWork = session.UnitOfWork;
        //        try
        //        {
        //            var repo = new AA0035Repository(DBWork);
        //            string docno = form.Get("p0");
        //            string wh_no = form.Get("p1");
        //            //session.Result.etts = repo.GetMeDocds(docno, wh_no, page, limit, sorters);
        //            var docds = repo.GetMeDocds(docno, wh_no, page, limit, sorters);

        //            foreach (ME_DOCD docd in docds)
        //            {
        //                docd.AMOUNT = (double.Parse(docd.APVQTY) * double.Parse(docd.M_CONTPRICE)).ToString();
        //            }

        //            session.Result.etts = docds;

        //        }
        //        catch
        //        {
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //}

        //[HttpPost]
        //public ApiResponse AllMeDoce(FormDataCollection form)
        //{
        //    var docno = form.Get("p0");
        //    var seq = form.Get("p1");

        //    using (WorkSession session = new WorkSession())
        //    {
        //        var DBWork = session.UnitOfWork;
        //        try
        //        {
        //            var repo = new AA0035Repository(DBWork);
        //            session.Result.etts = repo.GetMeDoces(docno, seq);
        //        }
        //        catch
        //        {
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //}

        //[HttpPost]
        //public ApiResponse GetMmdataByMmcode(FormDataCollection form)
        //{
        //    string mmcode = form.Get("mmcode");
        //    string wh_no = form.Get("wh_no");

        //    using (WorkSession session = new WorkSession())
        //    {
        //        UnitOfWork DBWork = session.UnitOfWork;
        //        try
        //        {
        //            AA0076Repository repo = new AA0076Repository(DBWork);
        //            if (!repo.CheckWhmmExist(mmcode, wh_no))
        //            {
        //                session.Result.afrs = 0;
        //                session.Result.success = false;
        //                session.Result.msg = "<span style='color:red'>此院內碼不存在於此庫房中</span>，請重新輸入。";

        //                return session.Result;
        //            }

        //            session.Result.etts = repo.GetMmdataByMmcode(mmcode);
        //        }
        //        catch
        //        {
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //}

        //[HttpPost]
        //public ApiResponse GetMeDocdMaxSeq(ME_DOCD me_docd)
        //{
        //    string docno = me_docd.DOCNO;
        //    using (WorkSession session = new WorkSession())
        //    {
        //        UnitOfWork DBWork = session.UnitOfWork;
        //        try
        //        {
        //            AA0035Repository repo = new AA0035Repository(DBWork);
        //            List<string> result = new List<string>();
        //            session.Result.etts = repo.GetMeDocdMaxSeq(docno);
        //        }
        //        catch
        //        {
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //}

        [HttpPost]
        public ApiResponse DetailCreate(MM_PR_D mm_pr_d)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BA0003Repository repo = new BA0003Repository(DBWork);

                    mm_pr_d.CREATE_USER = User.Identity.Name;
                    mm_pr_d.UPDATE_IP = DBWork.ProcIP;
                    if (repo.CheckDetailMmcodedExists(mm_pr_d.PR_NO, mm_pr_d.MMCODE))
                    {
                        session.Result.afrs = repo.DetailCreate(mm_pr_d);
                        DBWork.Commit();
                    }
                    else
                    {
                        session.Result.success = false;
                        session.Result.msg = "院內碼重複申請請確認!!";
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
        //public ApiResponse CreateMeDoec(ME_DOCD me_docd)
        //{
        //    string docno = me_docd.DOCNO;
        //    string seq = me_docd.SEQ;
        //    string wh_no = me_docd.WH_NO;

        //    using (WorkSession session = new WorkSession())
        //    {
        //        UnitOfWork DBWork = session.UnitOfWork;
        //        try
        //        {
        //            AA0035Repository repo = new AA0035Repository(DBWork);
        //            var result = repo.CreateMeDoce(docno, seq, wh_no);



        //            session.Result.msg = result;
        //        }
        //        catch
        //        {
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //}

        [HttpPost]
        public ApiResponse DetailDelete(MM_PR_D mm_pr_d)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BA0003Repository repo = new BA0003Repository(DBWork);


                    session.Result.afrs = repo.DetailDelete(mm_pr_d.PR_NO, mm_pr_d.MMCODE);

                    //2022-06-16: 修改申請單註記
                    session.Result.afrs = repo.UpdateMedocd(mm_pr_d.PR_NO, mm_pr_d.MMCODE);

                    //if (me_doces.Any())
                    //{
                    //    int medoceResult = repo.MeDoceDelete(tempDocnos[0], tempSeqs[i]);

                    //    if (medoceResult < 1)
                    //    {
                    //        DBWork.Rollback();
                    //        session.Result.success = false;
                    //        session.Result.msg = "細項刪除失敗";
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
        public ApiResponse DetailUpdate(MM_PR_D mm_pr_d)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BA0003Repository repo = new BA0003Repository(DBWork);
                    mm_pr_d.UPDATE_USER = User.Identity.Name;
                    mm_pr_d.UPDATE_IP = DBWork.ProcIP;

                    session.Result.afrs = repo.DetailUpdate(mm_pr_d);


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

        //public ApiResponse MeDoceUpdate(ME_DOCE me_doce)
        //{

        //    IEnumerable<ME_DOCE> me_doces = JsonConvert.DeserializeObject<IEnumerable<ME_DOCE>>(me_doce.ITEM_STRING);

        //    using (WorkSession session = new WorkSession(this))
        //    {
        //        var DBWork = session.UnitOfWork;
        //        DBWork.BeginTransaction();
        //        try
        //        {
        //            AA0035Repository repo = new AA0035Repository(DBWork);

        //            foreach (ME_DOCE doce in me_doces)
        //            {
        //                doce.UPDATE_USER = User.Identity.Name;
        //                doce.UPDATE_IP = DBWork.ProcIP;

        //                session.Result.afrs = repo.MeDoceUpdate(doce);
        //            }

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

        public ApiResponse GetMATCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BA0003Repository(DBWork);
                    session.Result.etts = repo.GetMATCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetMmcodeCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var MAT_CLASS = form.Get("MAT_CLASS");
            var M_STOREID = form.Get("M_STOREID");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BA0003Repository(DBWork);
                    session.Result.etts = repo.GetMmcodeCombo(MAT_CLASS, M_STOREID, p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetSelectMmcodeDetail(string MMCODE, string MAT_CLASS, string WH_NO, string M_STOREID)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BA0003Repository(DBWork);
                    session.Result.etts = repo.GetSelectMmcodeDetail(MMCODE, MAT_CLASS, WH_NO, M_STOREID);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetWh_noCombo()
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BA0003Repository(DBWork);
                    session.Result.etts = repo.GetWh_noCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetTot(string wh_no, string mat_class, string mmcode, string totprice)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BA0003Repository repo = new BA0003Repository(DBWork);

                    session.Result.msg = repo.GetTot(wh_no, mat_class, mmcode, totprice);
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
