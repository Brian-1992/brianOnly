using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http.Formatting;
using System.Web.Http;
using WebApp.Repository.B;
using WebApp.Models;
using JCLib.DB;

namespace WebApp.Controllers.B
{
    public class BE0006Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse GetMatClassCombo()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BE0006Repository repo = new BE0006Repository(DBWork);
                    session.Result.etts = repo.GetMatClassCombo(DBWork.UserInfo.UserId);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }



        // 刷新/查詢
        [HttpPost]
        public ApiResponse GetAll(FormDataCollection form)
        {
            string p0 = form.Get("P0");
            string p1 = form.Get("P1");
            string p2 = form.Get("P2");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BE0006Repository(DBWork);

                    //將已有發票號碼 or 發票日期不為空白，存在INVOICE_EMAIL的資料刪除
                    session.Result.afrs = repo.DelInvoiceEmail(p0);

                    //將已交貨,發票號碼 or 發票日期為空白,未新增INVOICE_EMAIL的資料撈出 新增至INVOICE_EMAIL
                    IEnumerable<BE0006> list = repo.GetAll(p0, p1, p2, true);
                    foreach (BE0006 item in list)
                    {
                        session.Result.afrs = repo.InsINVOICE_EMAIL(item.PO_NO, item.MMCODE, item.TRANSNO, item.AGEN_NO, item.MMNAME_C,
                                            item.MMNAME_E, item.PO_QTY, item.DELI_QTY, item.DELI_DT, item.agen_email,
                                            item.EMAIL_DT, item.REPLY_DT, User.Identity.Name, DBWork.ProcIP, item.MAT_CLASS,
                                            item.M_CONTID, item.PO_TIME, item.PO_PRICE, item.AMOUNT_1, item.INVOICE,
                                            item.INVOICE_DT
                                       );
                    }                    
                    session.Result.etts = repo.GetInvoiceEmail(p0, p1, p2, false);
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

        //匯出
        [HttpPost]
        public ApiResponse GetAllExcel(FormDataCollection form)
        {
            string p0 = form.Get("P0");
            string p1 = form.Get("P1");
            string p2 = form.Get("P2");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BE0006Repository repo = new BE0006Repository(DBWork);

                    JCLib.Excel.Export(form.Get("FN"), repo.GetAllExcel(p0, p1, p2)); //
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // 寄送MAIL
        [HttpPost]
        public ApiResponse UpdateInvoiceEmail(FormDataCollection form)
        {
            string p0 = form.Get("P0");
            string p1 = form.Get("P1");
            string p2 = form.Get("P2");

            using (WorkSession session = new WorkSession(this))
            {
                //UnitOfWork DBWork = session.UnitOfWork;
                bool success = true;
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BE0006Repository repo = new BE0006Repository(DBWork);

                    session.Result.afrs = repo.UpInvoiceEmail_Status(p0, p1, p2); //將待寄信的清單set STATUS=a

                    DBWork.Commit();
                }
                catch
                {
                    success = false;
                    DBWork.Rollback();
                    throw;
                }

                if (success)
                {
                    var DBWork2 = session.UnitOfWork;
                    DBWork2.BeginTransaction();
                    try
                    {
                        BE0006Repository repo = new BE0006Repository(DBWork2);

                        IEnumerable<BE0006> list = repo.GetAGEN_NO_forMail(); //撈出待寄信的廠商名冊

                        foreach (BE0006 item in list)
                        {
                            IEnumerable<BE0006> list2 = repo.GetBatno();  //相同廠商取得唯一序號值
                            var Batno = list2.First().Batno;

                            session.Result.afrs = repo.UpInvoiceEmail_Batno(item.AGEN_NO, Batno);//更新廠商取得唯一序號值
                        }

                        session.Result.afrs = repo.UpdateInvoiceEmail(DBWork2.UserInfo.UserId, DBWork2.ProcIP); //將待寄信的清單set STATUS=B                        

                        DBWork2.Commit();
                    }
                    catch
                    {
                        DBWork.Rollback();
                        throw;
                    }
                }
                return session.Result;
            }
        }
    }
}