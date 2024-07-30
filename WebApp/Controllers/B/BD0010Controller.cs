using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using WebApp.Models;
using WebApp.Repository.B;

namespace WebApp.Controllers.B
{
    public class BD0010Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse MasterAll(FormDataCollection form)
        {
            var WH_NO = form.Get("WH_NO");
            var YYYYMMDD = form.Get("YYYYMMDD");
            var YYYYMMDD_E = form.Get("YYYYMMDD_E");
            var PO_STATUS = form.Get("PO_STATUS");
            var Agen_No = form.Get("Agen_No");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0010Repository(DBWork);
                    session.Result.etts = repo.GetMasterAll(WH_NO, YYYYMMDD, YYYYMMDD_E, PO_STATUS, Agen_No, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse MasterUpdate(MM_PO_M mmpom) {

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();

                try
                {
                    mmpom.UPDATE_USER = DBWork.UserInfo.UserId;
                    mmpom.UPDATE_IP = DBWork.ProcIP;

                    var repo = new BD0010Repository(DBWork);
                    session.Result.afrs = repo.MasterUpdate(mmpom);
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
        public ApiResponse MasterObsolete(FormDataCollection form)
        {

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();

                try
                {

                    BD0010Repository repo = new BD0010Repository(DBWork);
                    if (form.Get("PO_NO") != "")
                    {
                        string po_nos = form.Get("PO_NO").Substring(0, form.Get("PO_NO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] po_noTmp = po_nos.Split(',');
                        for (int i = 0; i < po_noTmp.Length; i++)
                        {
                            MM_PO_M mmpom = new MM_PO_M();
                            mmpom.PO_NO = po_noTmp[i];
                            mmpom.UPDATE_USER = DBWork.UserInfo.UserId;
                            mmpom.UPDATE_IP = DBWork.ProcIP;
                            repo.MasterObsolete(mmpom);

                            MM_PO_D mmpod = new MM_PO_D();
                            mmpod.PO_NO = po_noTmp[i];
                            mmpod.UPDATE_USER = DBWork.UserInfo.UserId;
                            mmpod.UPDATE_IP = DBWork.ProcIP;
                            repo.DetailAllObsolete(mmpod);
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
        public ApiResponse DetailAll(FormDataCollection form)
        {
            string po_no = form.Get("p0");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();

                try
                {
                    var repo = new BD0010Repository(DBWork);
                    session.Result.etts = repo.GetDetailAll(po_no);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse DetailUpdate(MM_PO_D mmpod)
        {

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    mmpod.UPDATE_USER = DBWork.UserInfo.UserId;
                    mmpod.UPDATE_IP = DBWork.ProcIP;

                    var repo = new BD0010Repository(DBWork);
                    session.Result.afrs = repo.DetailUpdate(mmpod);
                    repo.UpdateMM_PO_INREC(mmpod);
                    repo.UpdatePH_INVOICE(mmpod);
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
        public ApiResponse DetailObsolete(FormDataCollection form)
        {

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();

                try
                {

                    BD0010Repository repo = new BD0010Repository(DBWork);
                    if (form.Get("PO_NO") != "" && form.Get("MMCODE")!="")
                    {
                        string po_nos = form.Get("PO_NO");
                        string mmcodes = form.Get("MMCODE").Substring(0, form.Get("MMCODE").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] mmcodeTmp = mmcodes.Split(',');
                        for (int i = 0; i < mmcodeTmp.Length; i++)
                        {
                            MM_PO_D mmpod = new MM_PO_D();
                            mmpod.PO_NO = po_nos;
                            mmpod.MMCODE = mmcodeTmp[i];
                            mmpod.UPDATE_USER = DBWork.UserInfo.UserId;
                            mmpod.UPDATE_IP = DBWork.ProcIP;

                            repo.DetailObsolete(mmpod);
                        }

                        if (repo.IsAllStatusD(po_nos)) {
                            // 全部作廢，狀態改為87
                            MM_PO_M mmpom = new MM_PO_M();
                            mmpom.PO_NO = po_nos;
                            mmpom.UPDATE_USER = DBWork.UserInfo.UserId;
                            mmpom.UPDATE_IP = DBWork.ProcIP;
                            repo.MasterObsolete(mmpom);

                            session.Result.msg = "單據狀態已變更，請重新查詢";
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

        #region 2021-03-23
        [HttpPost]
        public ApiResponse CheckPoAmt(FormDataCollection form) {
            string po_nos = form.Get("PO_NO").Substring(0, form.Get("PO_NO").Length - 1); // 去除前端傳進來最後一個逗號
            string[] tmp = po_nos.Split(',');
            using (WorkSession session = new WorkSession()) {
                var DBWork = session.UnitOfWork;
                var repo = new BD0010Repository(DBWork);

                List<MM_PO_D> details = new List<MM_PO_D>();
                for (int i = 0; i < tmp.Length; i++)
                {
                    IEnumerable<MM_PO_D> temp = repo.GetOver100K(tmp[i]);
                    details.AddRange(temp);
                }
                if (details.Any()) {
                    session.Result.msg = "有超過10萬元品項";
                    session.Result.success = false;
                    session.Result.etts = details;
                    return session.Result;
                }

                session.Result.success = true;
                return session.Result;
            }
        }

        public ApiResponse Over100KExcel(FormDataCollection form) {
            string po_nos = form.Get("PO_NO").Substring(0, form.Get("PO_NO").Length - 1); // 去除前端傳進來最後一個逗號
            string[] tmp = po_nos.Split(',');
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                var repo = new BD0010Repository(DBWork);

                List<MM_PO_D> details = new List<MM_PO_D>();

                DataTable dtItems = new DataTable();
                dtItems.Columns.Add("項次", typeof(int));
                dtItems.Columns["項次"].AutoIncrement = true;
                dtItems.Columns["項次"].AutoIncrementSeed = 1;
                dtItems.Columns["項次"].AutoIncrementStep = 1;

                DataTable result = null;

                //dtItems.Merge(result);

                for (int i = 0; i < tmp.Length; i++)
                {
                    result = repo.GetOver100KExcel(tmp[i]);
                    dtItems.Merge(result);
                }

                JCLib.Excel.Export(string.Format("零購超過十萬元清單_{0}.xls", DateTime.Now.ToString("yyyyMMddHHmmss")), dtItems);

                session.Result.success = true;
                return session.Result;
            }
        }
        #endregion


        [HttpPost]
        public ApiResponse SendEmail(FormDataCollection form) {


            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();

                try
                {
                    BD0010Repository repo = new BD0010Repository(DBWork);
                    if (form.Get("PO_NO") != "")
                    {
                        string po_nos = form.Get("PO_NO").Substring(0, form.Get("PO_NO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = po_nos.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            MM_PO_M mmpom = new MM_PO_M();
                            mmpom.PO_NO = tmp[i];
                            mmpom.PO_STATUS = "84";
                            mmpom.UPDATE_USER = DBWork.UserInfo.UserId;
                            mmpom.UPDATE_IP = DBWork.ProcIP;

                            session.Result.afrs = repo.SendEmail(mmpom);
                        }
                        DBWork.Commit();
                    }
                }
                catch {
                    DBWork.Rollback();
                    throw;
                }

                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse ReSendEmail(FormDataCollection form)
        {


            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();

                try
                {
                    BD0010Repository repo = new BD0010Repository(DBWork);
                    if (form.Get("PO_NO") != "")
                    {
                        string po_nos = form.Get("PO_NO").Substring(0, form.Get("PO_NO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = po_nos.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            MM_PO_M mmpom = new MM_PO_M();
                            mmpom.PO_NO = tmp[i];
                            mmpom.UPDATE_USER = DBWork.UserInfo.UserId;
                            mmpom.UPDATE_IP = DBWork.ProcIP;

                            session.Result.afrs = repo.ReSendEmail(mmpom);
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

        

        //[HttpPost]
        //public ApiResponse GetWH_NoCombo(FormDataCollection form)
        //{
        //    var p0 = form.Get("p0");
        //    //var wh_no = form.Get("WH_NO");
        //    var page = int.Parse(form.Get("page"));
        //    var start = int.Parse(form.Get("start"));
        //    var limit = int.Parse(form.Get("limit"));
        //    var sorters = form.Get("sort");

        //    using (WorkSession session = new WorkSession(this))
        //    {
        //        UnitOfWork DBWork = session.UnitOfWork;
        //        try
        //        {
        //            BD0010Repository repo = new BD0010Repository(DBWork);
        //            session.Result.etts = repo.GetWH_NoCombo(p0,DBWork.ProcUser, page, limit, "");
        //        }
        //        catch
        //        {
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //}
        public ApiResponse GetWH_NO()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BD0010Repository repo = new BD0010Repository(DBWork);
                    session.Result.etts = repo.GetWH_NO();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetAgenCombo(FormDataCollection form)
        {


            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BD0010Repository repo = new BD0010Repository(DBWork);
                    session.Result.etts = repo.GetWH_NoCombo();
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