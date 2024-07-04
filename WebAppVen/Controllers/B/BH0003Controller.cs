using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebAppVen.Repository.B;
using WebAppVen.Models;


namespace WebAppVen.Controllers.B
{
    public class BH0003Controller : SiteBase.BaseApiController
    {
        // 查詢Master
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
        {
            //var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BH0003Repository(DBWork);
                    session.Result.etts = repo.GetAllM(User.Identity.Name, p1, p2, p3, page, limit, sorters);
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
                    var repo = new BH0003Repository(DBWork);
                    session.Result.etts = repo.GetAllD(p0, p1, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //新增Detail
        [HttpPost]
        public ApiResponse CreateD(WB_PUT_D WB_PUT_D)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BH0003Repository(DBWork);
                    if (!repo.CheckExistsD_1(WB_PUT_D.AGEN_NO, WB_PUT_D.MMCODE, WB_PUT_D.TXTDAY, WB_PUT_D.SEQ, WB_PUT_D.DEPT)) // 新增前檢查主鍵是否已存在
                    {
                        WB_PUT_D.CREATE_USER = User.Identity.Name;
                        WB_PUT_D.UPDATE_USER = User.Identity.Name;
                        WB_PUT_D.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.CreateD(WB_PUT_D);
                        session.Result.etts = repo.GetD_2(WB_PUT_D.AGEN_NO, WB_PUT_D.MMCODE, WB_PUT_D.TXTDAY, WB_PUT_D.SEQ, WB_PUT_D.DEPT);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>廠商碼、院內碼及交易日期</span>重複，請重新輸入。";
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

        //修改Detail
        [HttpPost]
        public ApiResponse UpdateD(WB_PUT_D WB_PUT_D)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BH0003Repository(DBWork);
                    WB_PUT_D.UPDATE_USER = User.Identity.Name;
                    WB_PUT_D.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.UpdateD(WB_PUT_D);
                    session.Result.etts = repo.GetD_1(WB_PUT_D.AGEN_NO, WB_PUT_D.MMCODE, WB_PUT_D.TXTDAY_TEXT, WB_PUT_D.SEQ, WB_PUT_D.DEPT);

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

        //刪除Detail
        [HttpPost]
        public ApiResponse DeleteD(WB_PUT_D WB_PUT_D)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BH0003Repository(DBWork);
                    if (repo.CheckExistsD_2(WB_PUT_D.AGEN_NO, WB_PUT_D.MMCODE, WB_PUT_D.TXTDAY_TEXT, WB_PUT_D.SEQ, WB_PUT_D.DEPT))
                    {
                        session.Result.afrs = repo.DeleteD(WB_PUT_D);
                        session.Result.etts = repo.GetD_1(WB_PUT_D.AGEN_NO, WB_PUT_D.MMCODE, WB_PUT_D.TXTDAY_TEXT, WB_PUT_D.SEQ, WB_PUT_D.DEPT);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>廠商碼:" + WB_PUT_D.AGEN_NO + " 院內碼:" + WB_PUT_D.MMCODE + " 交易日期:" + WB_PUT_D.TXTDAY + " </span> 不存在。";
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

        //確認回傳
        [HttpPost]
        public ApiResponse CONFIRM_RETURN(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BH0003Repository(DBWork);
                    string agen_no = form.Get("AGEN_NO").Substring(0, form.Get("AGEN_NO").Length - 1); // 去除前端傳進來最後一個逗號
                    string mmcode = form.Get("MMCODE").Substring(0, form.Get("MMCODE").Length - 1); // 去除前端傳進來最後一個逗號
                    string txtday = form.Get("TXTDAY").Substring(0, form.Get("TXTDAY").Length - 1); // 去除前端傳進來最後一個逗號
                    string seq = form.Get("SEQ").Substring(0, form.Get("SEQ").Length - 1); // 去除前端傳進來最後一個逗號
                    string extype = form.Get("EXTYPE").Substring(0, form.Get("EXTYPE").Length - 1); // 去除前端傳進來最後一個逗號
                    string qty = form.Get("QTY").Substring(0, form.Get("QTY").Length - 1); // 去除前端傳進來最後一個逗號
                    string dept = form.Get("DEPT").Substring(0, form.Get("DEPT").Length - 1); // 去除前端傳進來最後一個逗號
                    string[] TMP_agen_no = agen_no.Split(',');
                    string[] TMP_mmcode = mmcode.Split(',');
                    string[] TMP_txtday = txtday.Split(',');
                    string[] TMP_seq = seq.Split(',');
                    string[] TMP_extype = extype.Split(',');
                    string[] TMP_qty = qty.Split(',');
                    string[] TMP_dept = dept.Split(',');

                    for (int i = 0; i < TMP_agen_no.Length; i++)
                    {
                        session.Result.afrs = repo.CONFIRM_RETURN(TMP_agen_no[i], TMP_mmcode[i], TMP_txtday[i], TMP_seq[i], TMP_dept[i], User.Identity.Name, DBWork.ProcIP);
                        if (TMP_extype[i] == "10")
                        {
                            session.Result.afrs = repo.UpdateM_Qty10(TMP_agen_no[i], TMP_mmcode[i], TMP_dept[i], TMP_qty[i], User.Identity.Name, DBWork.ProcIP);
                        }
                        else if (TMP_extype[i] == "31")
                        {
                            session.Result.afrs = repo.UpdateM_Qty31(TMP_agen_no[i], TMP_mmcode[i], TMP_dept[i], TMP_qty[i], User.Identity.Name, DBWork.ProcIP);
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

        //廠商碼
        public string GetAGEN_NO()
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BH0003Repository(DBWork);
                    var UserId = User.Identity.Name;
                    var Agen_Name = UserId +" "+ DBWork.UserInfo.UserName;
                    return Agen_Name;
                }
                catch
                {
                    throw;
                }
            }
        }

        //院內碼combox
        public ApiResponse GetMMCODE()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BH0003Repository(DBWork);
                    session.Result.etts = repo.GetMMCODE();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //匯出EXCEL
        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            //var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BH0003Repository repo = new BH0003Repository(DBWork);
                    JCLib.Excel.Export(form.Get("FN"), repo.GetExcel(User.Identity.Name, p1, p2, p3));
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
            var p0 = form.Get("p0");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BH0003Repository repo = new BH0003Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeCombo(p0, page, limit, "");
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
                    BH0003Repository repo = new BH0003Repository(DBWork);
                    BH0003Repository.MI_MAST_QUERY_PARAMS query = new BH0003Repository.MI_MAST_QUERY_PARAMS();
                    query.MMCODE = form.Get("MMCODE") == null ? "" : form.Get("MMCODE").ToUpper();
                    query.MMNAME_C = form.Get("MMNAME_C") == null ? "" : form.Get("MMNAME_C").ToUpper();
                    query.MMNAME_E = form.Get("MMNAME_E") == null ? "" : form.Get("MMNAME_E").ToUpper();
                    session.Result.etts = repo.GetMmcode(query, page, limit, sorters);
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