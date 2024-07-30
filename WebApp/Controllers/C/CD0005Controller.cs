using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.C;
using WebApp.Models;


namespace WebApp.Controllers.C
{
    public class CD0005Controller : SiteBase.BaseApiController
    {
        // 查詢Master
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
        {

            var p0 = form.Get("p0");
            var p1 = "";
            if ((form.Get("p1") != null) && (form.Get("p1") != ""))
            {
                p1 = form.Get("p1").Substring(0, 10).Replace("-", "/");
            }
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CD0005Repository(DBWork);
                    session.Result.etts = repo.GetAllM(p0, p1, p2, p3, p4, page, limit, sorters);
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
            var p1 = "";
            if ((form.Get("p1") != null) && (form.Get("p1") != ""))
            {
                p1 = form.Get("p1").Substring(0, 10).Replace("-", "/");
            }

            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");
            var p6 = form.Get("p6");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CD0005Repository(DBWork);
                    session.Result.etts = repo.GetAllD(p0, p1, p2, p3, p4, p5, p6, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }





        //確認完成
        [HttpPost]
        public ApiResponse UpdateOK(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CD0005Repository(DBWork);
                    string wh_no = form.Get("WH_NO").Substring(0, form.Get("WH_NO").Length - 1); // 去除前端傳進來最後一個逗號
                    string pick_date = form.Get("PICK_DATE").Substring(0, form.Get("PICK_DATE").Length - 1); // 去除前端傳進來最後一個逗號
                    string lot_no = form.Get("LOT_NO").Substring(0, form.Get("LOT_NO").Length - 1); // 去除前端傳進來最後一個逗號
                    string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                    string pick_userid = form.Get("PICK_USERID").Substring(0, form.Get("PICK_USERID").Length - 1); // 去除前端傳進來最後一個逗號
                    string act_pick_qty_code = form.Get("ACT_PICK_QTY_CODE").Substring(0, form.Get("ACT_PICK_QTY_CODE").Length - 1); // 去除前端傳進來最後一個逗號
                    string[] TMP_wh_no = wh_no.Split(',');
                    string[] TMP_pick_date = pick_date.Split(',');
                    string[] TMP_lot_no = lot_no.Split(',');
                    string[] TMP_docno = docno.Split(',');
                    string[] TMP_pick_userid = pick_userid.Split(',');
                    string[] TMP_act_pick_qty_code = act_pick_qty_code.Split(',');
                    for (int i = 0; i < TMP_wh_no.Length; i++)
                    {
                        session.Result.afrs = repo.UpdateOK(TMP_wh_no[i], TMP_pick_date[i], TMP_lot_no[i], TMP_docno[i], TMP_pick_userid[i], TMP_act_pick_qty_code[i]);
                        var Details = repo.Find_Detail_OK(TMP_wh_no[i], TMP_pick_date[i], TMP_lot_no[i], TMP_docno[i], TMP_pick_userid[i], TMP_act_pick_qty_code[i]);
                        foreach (CD0005MD Detail in Details)
                        {
                            session.Result.afrs = repo.UpdateOK_Deail(Detail.ACT_PICK_QTY, Detail.MAT_CLASS, Detail.ACT_PICK_USERID, Detail.ACT_PICK_TIME, Detail.DOCNO, Detail.SEQ);
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

        //確認取消
        [HttpPost]
        public ApiResponse UpdateCanael(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CD0005Repository(DBWork);
                    string wh_no = form.Get("WH_NO").Substring(0, form.Get("WH_NO").Length - 1); // 去除前端傳進來最後一個逗號
                    string pick_date = form.Get("PICK_DATE").Substring(0, form.Get("PICK_DATE").Length - 1); // 去除前端傳進來最後一個逗號
                    string lot_no = form.Get("LOT_NO").Substring(0, form.Get("LOT_NO").Length - 1); // 去除前端傳進來最後一個逗號
                    string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                    string pick_userid = form.Get("PICK_USERID").Substring(0, form.Get("PICK_USERID").Length - 1); // 去除前端傳進來最後一個逗號
                    string act_pick_qty_code = form.Get("ACT_PICK_QTY_CODE").Substring(0, form.Get("ACT_PICK_QTY_CODE").Length - 1); // 去除前端傳進來最後一個逗號
                    string[] TMP_wh_no = wh_no.Split(',');
                    string[] TMP_pick_date = pick_date.Split(',');
                    string[] TMP_lot_no = lot_no.Split(',');
                    string[] TMP_docno = docno.Split(',');
                    string[] TMP_pick_userid = pick_userid.Split(',');
                    string[] TMP_act_pick_qty_code = act_pick_qty_code.Split(',');
                    for (int i = 0; i < TMP_wh_no.Length; i++)
                    {
                        session.Result.afrs = repo.UpdateCanael(TMP_wh_no[i], TMP_pick_date[i], TMP_lot_no[i], TMP_docno[i], TMP_pick_userid[i], TMP_act_pick_qty_code[i]);
                        var Details = repo.Find_Detail_Canael(TMP_wh_no[i], TMP_pick_date[i], TMP_lot_no[i], TMP_docno[i], TMP_pick_userid[i], TMP_act_pick_qty_code[i]);
                        foreach (CD0005DD Detail in Details)
                        {
                            session.Result.afrs = repo.UpdateCanael_Deail(Detail.APPQTY, Detail.MAT_CLASS, Detail.DOCNO, Detail.SEQ);
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

        //庫房代碼combox
        public ApiResponse GetWH_NO()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CD0005Repository(DBWork);
                    session.Result.etts = repo.GetWH_NO(User.Identity.Name);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        //揀貨人員combox
        public ApiResponse GetPICK_USERID(FormDataCollection form)
        {
            var WH_NO = form.Get("WH_NO");
            var PICK_DATE = form.Get("PICK_DATE").Substring(0, 10).Replace("-", "/");
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CD0005Repository(DBWork);
                    session.Result.etts = repo.GetPICK_USERID(WH_NO, PICK_DATE);
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