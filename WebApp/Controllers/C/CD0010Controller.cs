using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.C;
using WebApp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace WebApp.Controllers.C
{
    public class CD0010Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
        {
            CD0010 v = new CD0010();
            v.WH_NO = form.Get("WH_NO");
            v.PICK_USERID = form.Get("PICK_USERID");
            v.PICK_DATE_START = form.Get("PICK_DATE_START");
            v.PICK_DATE_END = form.Get("PICK_DATE_END");
            v.SHOPOUT_DATE_START = form.Get("SHOPOUT_DATE_START");
            v.SHOPOUT_DATE_END = form.Get("SHOPOUT_DATE_END");
            v.DOCNO = form.Get("DOCNO");
            v.APPDEPT = form.Get("APPDEPT");
            v.MMCODE = form.Get("MMCODE");
            v.ACT_PICK_USERID = form.Get("ACT_PICK_USERID");
            v.HAS_APPQTY = form.Get("HAS_APPQTY");
            v.HAS_CONFIRMED = form.Get("HAS_CONFIRMED");
            v.HAS_SHOPOUT = form.Get("HAS_SHOPOUT");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    
                    var repo = new CD0010Repository(DBWork);
                    var v_inid = repo.GetUridInid(User.Identity.Name);
                    session.Result.etts = repo.GetAllM(v, v_inid, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        } // end of AllM


        [HttpPost]
        public ApiResponse AllD(FormDataCollection form)
        {
            CD0010 v = new CD0010();
            v.WH_NO = form.Get("WH_NO");
            v.PICK_DATE_START = form.Get("PICK_DATE_START");
            v.PICK_DATE_END = form.Get("PICK_DATE_END");
            v.SHOPOUT_DATE_START = form.Get("SHOPOUT_DATE_START");
            v.SHOPOUT_DATE_END = form.Get("SHOPOUT_DATE_END");
            v.DOCNO = form.Get("DOCNO");
            v.MMCODE = form.Get("MMCODE");
            v.ACT_PICK_USERID = form.Get("ACT_PICK_USERID");
            v.HAS_APPQTY = form.Get("HAS_APPQTY");
            v.HAS_CONFIRMED = form.Get("HAS_CONFIRMED");
            v.HAS_SHOPOUT = form.Get("HAS_SHOPOUT");


            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CD0010Repository(DBWork);
                    session.Result.etts = repo.GetAllD(v, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        } // 


        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CD0010Repository repo = new CD0010Repository(DBWork);

                    CD0010 v = new CD0010();
                    v.WH_NO = form.Get("WH_NO");
                    v.PICK_DATE_START = form.Get("PICK_DATE_START");
                    v.PICK_DATE_END = form.Get("PICK_DATE_END");
                    v.SHOPOUT_DATE_START = form.Get("SHOPOUT_DATE_START");
                    v.SHOPOUT_DATE_END = form.Get("SHOPOUT_DATE_END");
                    v.DOCNO = form.Get("DOCNO");
                    v.MMCODE = form.Get("MMCODE");
                    v.ACT_PICK_USERID = form.Get("ACT_PICK_USERID");
                    v.HAS_APPQTY = form.Get("HAS_APPQTY");
                    v.HAS_CONFIRMED = form.Get("HAS_CONFIRMED");
                    v.HAS_SHOPOUT = form.Get("HAS_SHOPOUT");
                    JCLib.Excel.Export(
                        "揀貨資料查詢作業明細表_" + System.DateTime.Now.ToString("yyyyMMddhhmm") + ".xls",
                        repo.GetExcel(v));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetAppdeptCombo(FormDataCollection form)
        {
            var wh_no = form.Get("WH_NO");
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CD0010Repository repo = new CD0010Repository(DBWork);
                    session.Result.etts = repo.GetAppdeptCombo(wh_no);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetActPickUseridCombo(FormDataCollection form)
        {
            var wh_no = form.Get("WH_NO");
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CD0010Repository repo = new CD0010Repository(DBWork);
                    session.Result.etts = repo.GetActPickUseridCombo(wh_no);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }


        [HttpPost]
        public ApiResponse GetDocnopkCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CD0010Repository repo = new CD0010Repository(DBWork);
                    var v_inid = repo.GetUridInid(User.Identity.Name);
                    session.Result.etts = repo.GetDocnopkCombo(p0,v_inid);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetDocpknoteCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CD0010Repository repo = new CD0010Repository(DBWork);
                    var v_inid = repo.GetUridInid(User.Identity.Name);
                    session.Result.etts = repo.GetDocpknoteCombo(p0, v_inid);
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
                    CD0010Repository repo = new CD0010Repository(DBWork);
                    session.Result.etts = repo.GetDocnoCombo();
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
                    CD0010Repository repo = new CD0010Repository(DBWork);
                    session.Result.etts = repo.GetFlowidCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetApplyKindCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CD0010Repository repo = new CD0010Repository(DBWork);
                    session.Result.etts = repo.GetApplyKindCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetMmCodeCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CD0010Repository repo = new CD0010Repository(DBWork);
                    session.Result.etts = repo.GetMmCodeCombo(p0,p1,p2, p3,page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMmcode(FormDataCollection form)
        {
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CD0010Repository repo = new CD0010Repository(DBWork);
                    CD0010Repository.MI_MAST_QUERY_PARAMS query = new CD0010Repository.MI_MAST_QUERY_PARAMS();
                    query.MMCODE = form.Get("MMCODE") == null ? "" : form.Get("MMCODE").ToUpper();
                    query.MMNAME_C = form.Get("MMNAME_C") == null ? "" : form.Get("MMNAME_C").ToUpper();
                    query.MMNAME_E = form.Get("MMNAME_E") == null ? "" : form.Get("MMNAME_E").ToUpper();
                    query.WH_NO = form.Get("WH_NO") == null ? "" : form.Get("WH_NO").ToUpper();
                    query.MAT_CLASS = form.Get("MAT_CLASS") == null ? "" : form.Get("MAT_CLASS").ToUpper();
                    session.Result.etts = repo.GetMmcode(query, page, limit, sorters);
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
                    CD0010Repository repo = new CD0010Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeDocd(p0, p1, page, limit, "");
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
                    CD0010Repository repo = new CD0010Repository(DBWork);
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
        public ApiResponse GetTowhCombo()
        {
            var p0 = User.Identity.Name;
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CD0010Repository repo = new CD0010Repository(DBWork);
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
        public ApiResponse GetMatclassCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CD0010Repository repo = new CD0010Repository(DBWork);
                    session.Result.etts = repo.GetMatclassCombo();
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
                    CD0010Repository repo = new CD0010Repository(DBWork);
                    var v_userid = User.Identity.Name;
                    var v_ip = DBWork.ProcIP;
                    session.Result.etts = repo.GetLoginInfo(v_userid,v_ip);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        
        [HttpPost]
        public ApiResponse GetReasonCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CD0010Repository repo = new CD0010Repository(DBWork);
                    session.Result.etts = repo.GetReasonCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetUserPh1s()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CD0010Repository repo = new CD0010Repository(DBWork);
                    var v_msg = "";
                    if (repo.CheckUserPh1s(User.Identity.Name))
                    {
                        v_msg = "Y";
                    }
                    else
                    {
                        v_msg = "N";
                    }
                    session.Result.msg = v_msg;
                    session.Result.success = true;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse UpdateBoxqty(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    CD0010Repository repo = new CD0010Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        
                        string docno = form.Get("DOCNO").ToString();
                        string whno = form.Get("WH_NO").ToString();
                        var v_qty1 = int.Parse(form.Get("QTY").ToString());
                        var v_qty2 = int.Parse(repo.GetUse_box_qty(docno, whno));
                        var v_qty = v_qty1 + v_qty2;

                        if (repo.CheckExistsM(docno)) // 新增前檢查主鍵是否已存在
                        {
                            session.Result.afrs = repo.UpdateBoxqty(docno, v_qty);
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
        public ApiResponse GetUse_box_qty(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CD0010Repository repo = new CD0010Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").ToString();
                        string whno = form.Get("WH_NO").ToString();
                        var v_msg = repo.GetUse_box_qty(docno, whno);
                        session.Result.msg = v_msg;
                        session.Result.success = true;
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