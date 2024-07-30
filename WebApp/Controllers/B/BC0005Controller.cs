using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.BC;
using WebApp.Models;


namespace WebApp.Controllers.BC
{
    public class BC0005Controller : SiteBase.BaseApiController
    {
        // 查詢Master
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BC0005Repository(DBWork);
                    session.Result.etts = repo.GetAllM(p0, p1, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // 查詢Detail
        [HttpPost]
        public ApiResponse AllD(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BC0005Repository(DBWork);
                    session.Result.etts = repo.GetAllD(p0, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //修改Master
        [HttpPost]
        public ApiResponse UpdateM(BC0005M BC0005M)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BC0005Repository(DBWork);
                    BC0005M.UPDATE_USER = User.Identity.Name;
                    BC0005M.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.UpdateM(BC0005M);

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

        //取消訂單
        [HttpPost]
        public ApiResponse CANCEL_ORDER(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BC0005Repository(DBWork);
                    string po_no = form.Get("PO_NO").Substring(0, form.Get("PO_NO").Length - 1); // 去除前端傳進來最後一個逗號
                    string sdn = form.Get("SDN").Substring(0, form.Get("SDN").Length - 1); // 去除前端傳進來最後一個逗號
                    string[] TMP_po_no = po_no.Split(',');
                    string[] TMP_sdn = sdn.Split(',');
                    for (int i = 0; i < TMP_po_no.Length; i++)
                    {
                        session.Result.afrs = repo.CANCEL_ORDER_1(TMP_sdn[i], User.Identity.Name, DBWork.ProcIP);
                        session.Result.afrs = repo.CANCEL_ORDER_2(TMP_po_no[i]);
                        session.Result.afrs = repo.CANCEL_ORDER_3(TMP_sdn[i]);
                        session.Result.afrs = repo.CANCEL_ORDER_4(TMP_sdn[i]);
                        session.Result.afrs = repo.CANCEL_ORDER_5(TMP_sdn[i]);
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

        //寄送MAIL
        [HttpPost]
        public ApiResponse SEND_MAIL(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BC0005Repository(DBWork);
                    string po_no = form.Get("PO_NO").Substring(0, form.Get("PO_NO").Length - 1); // 去除前端傳進來最後一個逗號
                    string[] TMP_po_no = po_no.Split(',');
                    for (int i = 0; i < TMP_po_no.Length; i++)
                    {
                        session.Result.afrs = repo.SEND_MAIL(TMP_po_no[i], User.Identity.Name, DBWork.ProcIP);
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
    }
}