using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.B;

namespace WebApp.Controllers.B
{
    public class BA0006Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse getBC_CS_ACC_LOG(FormDataCollection form)
        {
            var p0 = form.Get("p0"); // 物料類別
            var p1 = form.Get("p1"); // 進貨日期起
            var p2 = form.Get("p2"); // 進貨日期迄
            //var p4 = form.Get("p4");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BA0006Repository(DBWork);
                    session.Result.etts = repo.getBC_CS_ACC_LOG(p0, p1, p2, page, limit, sorters);
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
                    var repo = new BA0006Repository(DBWork);
                    session.Result.etts = repo.GetWh_noCombo();
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
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BA0006Repository(DBWork);
                    session.Result.etts = repo.GetMmcodeCombo(MAT_CLASS, p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetSelectMmcodeDetail(string MMCODE, string WH_NO)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BA0006Repository(DBWork);
                    session.Result.etts = repo.GetSelectMmcodeDetail(MMCODE, WH_NO);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse CreateBC_CS_ACC_LOG(BA0006M ba0006m)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BA0006Repository repo = new BA0006Repository(DBWork);
                    ba0006m.ACC_USER = User.Identity.Name;
                    ba0006m.INID = DBWork.UserInfo.Inid;
                    ba0006m.UPDATE_IP = DBWork.ProcIP;
                    ba0006m.STATUS = "A"; //暫驗
                    //ba0006m.SEQ = repo.getSeq();
                    ba0006m.PO_NO = repo.getNewPO_NO(ba0006m.M_STOREID);
                    session.Result.afrs = repo.CreateBC_CS_ACC_LOG(ba0006m);
                    //ba0006m.PO_NO = repo.getPO_NO(ba0006m.SEQ);
                    repo.InsertMM_PO_M(ba0006m);
                    repo.InsertMM_PO_D(ba0006m);
                    session.Result.success = true;

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
        public ApiResponse UpdateBC_CS_ACC_LOG(BA0006M ba0006m)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BA0006Repository repo = new BA0006Repository(DBWork);
                    ba0006m.ACC_USER = User.Identity.Name;
                    session.Result.afrs = repo.UpdateBC_CS_ACC_LOG(ba0006m);
                    DBWork.Commit();
                    session.Result.etts = repo.GetD(ba0006m.PO_NO, ba0006m.SEQ);
                    session.Result.success = true;
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
        public ApiResponse DeleteBC_CS_ACC_LOG(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var po_no = form.Get("po_no");
                var seq = form.Get("seq");
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BA0006Repository repo = new BA0006Repository(DBWork);
                    session.Result.afrs = repo.DeleteBC_CS_ACC_LOG(po_no, seq);
                    session.Result.success = true;
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
        public ApiResponse InWHNO(BA0006M ba0006m)
        {
            int ret = 0;
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BA0006Repository repo = new BA0006Repository(DBWork);
                    ba0006m.ACC_USER = User.Identity.Name;
                    ba0006m.STATUS = "C"; //確認驗收
                    if (ba0006m.XACTION == "I")
                    {
                        //ba0006m.SEQ = repo.getSeq();
                        ba0006m.PO_NO = repo.getNewPO_NO(ba0006m.M_STOREID);
                        ret = repo.CreateBC_CS_ACC_LOG(ba0006m);
                        //ba0006m.PO_NO= repo.getPO_NO(ba0006m.SEQ);
                        repo.InsertMM_PO_M(ba0006m);
                        repo.InsertMM_PO_D(ba0006m);
                    }
                    else
                    {
                        ret = repo.UpdateBC_CS_ACC_LOG(ba0006m);
                        repo.DeleteMM_PO_D(ba0006m);
                        repo.DeleteMM_PO_M(ba0006m);
                        repo.InsertMM_PO_M(ba0006m);
                        repo.InsertMM_PO_D(ba0006m);
                    }
                    session.Result.success = false;
                    if (ret ==1)
                    {
                        session.Result.msg = repo.procDocin(ba0006m.PO_NO, User.Identity.Name, DBWork.ProcIP);
                        if (session.Result.msg == "入庫除帳..完成")
                        {
                            DBWork.Commit();
                            session.Result.etts = repo.GetD(ba0006m.PO_NO, null);
                            session.Result.success = true;
                        }
                        else
                            DBWork.Rollback();
                        session.Result.success = true;
                    }
                    else
                        DBWork.Rollback();
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }
        public ApiResponse GetMATCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BA0006Repository(DBWork);
                    session.Result.etts = repo.GetMATCombo();
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
