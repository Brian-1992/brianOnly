using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebApp.Controllers.AA
{
    public class AA0037Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var d0 = form.Get("d0");
            var d1 = form.Get("d1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");
            var p6 = form.Get("p6");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            string[] arr_p2 = { };
            if (!string.IsNullOrEmpty(p2))
            {
                arr_p2 = p2.Trim().Split(','); //用,分割
            }
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0037Repository(DBWork);
                    //var taskid = repo.GetTaskid(User.Identity.Name);
                    var doctype = "";
                    //if (taskid == "2")
                    //{
                    //    doctype = "MR4";
                    //}
                    //else
                    //{
                    //    doctype = "MR3";
                    //}
                    if (p5 != "")
                    {
                        doctype = p5;
                    }
                    session.Result.etts = repo.GetAllM(p0, d0, d1, arr_p2, p3, p4, doctype, p6, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
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
                    var repo = new AA0037Repository(DBWork);
                    session.Result.etts = repo.GetAllD(p0, p1, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // 新增
        [HttpPost]
        public ApiResponse CreateM(ME_DOCM ME_DOCM)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0037Repository(DBWork);
                    if (!repo.CheckExists(ME_DOCM.DOCNO)) // 新增前檢查主鍵是否已存在
                    {
                        var v_inid = ME_DOCM.APPDEPT;
                        var v_twntime = repo.GetTwnsystime();
                        var v_matclass = ME_DOCM.MAT_CLASS;
                        var v_docno = v_inid + v_twntime + v_matclass;
                        var v_whno = repo.GetFrwh();
                        var v_num = repo.CheckApplyKind();
                        var v_applykind = "2"; //只做臨時申請
                        var v_stroeid = ME_DOCM.M_STOREID;
                        
                        var v_doctype = "MR4";
                        if (v_stroeid == "1")
                        {
                            if (v_matclass == "02")
                            {
                                v_doctype = "MR2";
                            }
                            else
                            {
                                v_doctype = "MR1";
                            }
                        }
                        else
                        {
                            if (v_matclass == "02")
                            {
                                v_doctype = "MR4";
                            }
                            else
                            {
                                v_doctype = "MR3";
                            }
                        }

                        ME_DOCM.DOCNO = v_docno;
                        ME_DOCM.DOCTYPE = v_doctype;
                        //ME_DOCM.APPID = User.Identity.Name;
                        ME_DOCM.FRWH = v_whno;
                        //ME_DOCM.APPDEPT = v_inid;
                        ME_DOCM.APPLY_KIND = v_applykind;
                        ME_DOCM.USEID = User.Identity.Name;
                        ME_DOCM.USEDEPT = ME_DOCM.APPDEPT;
                        ME_DOCM.CREATE_USER = User.Identity.Name;
                        ME_DOCM.UPDATE_USER = User.Identity.Name;
                        ME_DOCM.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.CreateM(ME_DOCM);
                        session.Result.etts = repo.GetM(ME_DOCM.DOCNO);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>代碼</span>重複，請重新輸入。";
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
        public ApiResponse CreateD(ME_DOCD ME_DOCD)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0037Repository(DBWork);
                    if (!repo.CheckMmcode(ME_DOCD.MMCODE))
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>此院內碼「" + ME_DOCD.MMCODE + "」不正確</span>，請確認。";
                    }
                    else
                    {
                        if (repo.CheckExistsMM(ME_DOCD.DOCNO, ME_DOCD.MMCODE))
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = "<span style='color:red'>此單明細已有重複的院內碼「" + ME_DOCD.MMCODE + "」</span>，請確認。";
                        }
                        else
                        {
                            if (!repo.CheckExistsD(ME_DOCD.DOCNO)) // 新增前檢查主鍵是否已存在
                            {
                                ME_DOCD.SEQ = "1";
                            }
                            else
                            {
                                ME_DOCD.SEQ = repo.GetDocDSeq(ME_DOCD.DOCNO);
                            }
                            //ME_DOCD.APVID = User.Identity.Name;
                            //ME_DOCD.ACKID = User.Identity.Name;
                            ME_DOCD.CREATE_USER = User.Identity.Name;
                            ME_DOCD.UPDATE_USER = User.Identity.Name;
                            ME_DOCD.UPDATE_IP = DBWork.ProcIP;
                            session.Result.afrs = repo.CreateD(ME_DOCD);
                            session.Result.etts = repo.GetD(ME_DOCD.DOCNO, ME_DOCD.SEQ);
                            DBWork.Commit();
                        }
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
        // 修改
        [HttpPost]
        public ApiResponse UpdateM(ME_DOCM ME_DOCM)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0037Repository(DBWork);

                    bool flowIdValid = repo.ChceckFlowId01(ME_DOCM.DOCNO);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    ME_DOCM.UPDATE_USER = User.Identity.Name;
                    ME_DOCM.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.UpdateM(ME_DOCM);
                    session.Result.etts = repo.GetM(ME_DOCM.DOCNO);

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
        public ApiResponse UpdateD(ME_DOCD ME_DOCD)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0037Repository(DBWork);

                    ME_DOCD.UPDATE_USER = User.Identity.Name;
                    ME_DOCD.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.UpdateD(ME_DOCD);
                    session.Result.etts = repo.GetD(ME_DOCD.DOCNO,ME_DOCD.SEQ);

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

        // 刪除

        // 刪除
        [HttpPost]
        public ApiResponse DeleteM(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0037Repository repo = new AA0037Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            bool flowIdValid = repo.ChceckFlowId01(tmp[i]);
                            if (flowIdValid == false)
                            {
                                session.Result.msg = "申請單狀態已變更，請重新查詢";
                                session.Result.success = false;
                                return session.Result;
                            }

                            //if (!repo.CheckExistsD(tmp[i])) // 傳入DOCNO檢查申請單是否有院內碼項次
                            //{
                            session.Result.afrs = repo.DeleteAllD(tmp[i]);
                            session.Result.afrs = repo.DeleteM(tmp[i]);
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
        public ApiResponse DeleteD(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0037Repository repo = new AA0037Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string seq = form.Get("SEQ").Substring(0, form.Get("SEQ").Length - 1);
                        string[] tmp_docno = docno.Split(',');
                        string[] tmp_seq = seq.Split(',');
                        for (int i = 0; i < tmp_docno.Length; i++) {
                            bool flowIdValid = repo.ChceckFlowId01(tmp_docno[i]);
                            if (flowIdValid == false)
                            {
                                session.Result.msg = "申請單狀態已變更，請重新查詢";
                                session.Result.success = false;
                                return session.Result;
                            }
                            session.Result.afrs = repo.DeleteD(tmp_docno[i], tmp_seq[i]);
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
        // 退回
        [HttpPost]
        public ApiResponse ApplyX(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0037Repository repo = new AA0037Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            if (repo.CheckExistsM(tmp[i]))
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = "<span style='color:red'>此申請單狀態非核撥中</span>不得退回。";
                            }
                            else
                            {
                                if (repo.CheckExistsD(tmp[i])) // 傳入DOCNO檢查申請單是否有院內碼項次
                                {
                                    ME_DOCM me_docm = new ME_DOCM();
                                    me_docm.DOCNO = tmp[i];
                                    me_docm.UPDATE_USER = User.Identity.Name;
                                    me_docm.UPDATE_IP = DBWork.ProcIP;
                                    session.Result.afrs = repo.ApplyX(me_docm);
                                }
                                else
                                {
                                    session.Result.afrs = 0;
                                    session.Result.success = false;
                                    session.Result.msg = "<span style='color:red'>申請單號「" + tmp[i] + "」沒有院內碼項次</span>，請新增院內碼項次。";
                                }
                            }
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
        public ApiResponse Savepk(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0037Repository repo = new AA0037Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO");
                        string note = form.Get("NOTE");
                        var v_matclass = form.Get("MAT_CLASS");
                        var v_inid = repo.GetUridInid(User.Identity.Name);
                        var v_twntime = repo.GetTwnsystime();
                        var newdocno = v_inid + v_twntime + v_matclass;
                        var v_stroeid = form.Get("M_STOREID");

                        var v_doctype = "MR4";
                        if (v_stroeid == "1")
                        {
                            if (v_matclass == "02")
                            {
                                v_doctype = "MR2";
                            }
                            else
                            {
                                v_doctype = "MR1";
                            }
                        }
                        else
                        {
                            if (v_matclass == "02")
                            {
                                v_doctype = "MR4";
                            }
                            else
                            {
                                v_doctype = "MR3";
                            }
                        }
                        session.Result.afrs = repo.SavepkM(docno, note, newdocno,v_doctype);
                        session.Result.afrs = repo.SavepkD(docno, newdocno);
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
        public ApiResponse GetPackD(FormDataCollection form)
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
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0037Repository(DBWork);
                    session.Result.etts = repo.GetPackD(p0, p1, p2, p3, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse InsFromPk(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0037Repository repo = new AA0037Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string mmcode = form.Get("MMCODE").Substring(0, form.Get("MMCODE").Length - 1);
                        string appqty = form.Get("APPQTY").Substring(0, form.Get("APPQTY").Length - 1);
                        string[] tmp_docno = docno.Split(',');
                        string[] tmp_mmcode = mmcode.Split(',');
                        string[] tmp_appqty = appqty.Split(',');
                        string v_seq = "1";
                        ME_DOCD me_docd = new ME_DOCD();
                        for (int i = 0; i < tmp_docno.Length; i++)
                        {
                            if (repo.CheckExistsMM(tmp_docno[i], tmp_mmcode[i]))
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = "<span style='color:red'>此單明細已有重複的院內碼「" + tmp_mmcode[i] + "」</span>，請確認。";
                                break;
                            }
                            else
                            {
                                if (!repo.CheckExistsD(tmp_docno[i])) // 新增前檢查主鍵是否已存在
                                {
                                    v_seq = "1";
                                }
                                else
                                {
                                    v_seq = repo.GetDocDSeq(tmp_docno[i]);
                                }
                                me_docd.DOCNO = tmp_docno[i];
                                me_docd.SEQ = v_seq;
                                me_docd.MMCODE = tmp_mmcode[i];
                                me_docd.APPQTY = tmp_appqty[i];
                                me_docd.CREATE_USER = User.Identity.Name;
                                me_docd.UPDATE_USER = User.Identity.Name;
                                me_docd.UPDATE_IP = DBWork.ProcIP;
                                session.Result.afrs = repo.CreateD(me_docd);
                            }

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
        public ApiResponse GetDocnopkCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0037Repository repo = new AA0037Repository(DBWork);
                    session.Result.etts = repo.GetDocnopkCombo(p0,p1, p2);
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
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0037Repository repo = new AA0037Repository(DBWork);
                    session.Result.etts = repo.GetDocpknoteCombo(p0, p1, p2);
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
                    AA0037Repository repo = new AA0037Repository(DBWork);
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
                    AA0037Repository repo = new AA0037Repository(DBWork);
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
                    AA0037Repository repo = new AA0037Repository(DBWork);
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
        public ApiResponse GetDocmAppDeptCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0037Repository repo = new AA0037Repository(DBWork);
                    session.Result.etts = repo.GetDocmAppDeptCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetAppDeptCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0037Repository repo = new AA0037Repository(DBWork);
                    session.Result.etts = repo.GetAppDeptCombo();
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
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0037Repository repo = new AA0037Repository(DBWork);
                    session.Result.etts = repo.GetMmCodeCombo(p0, p1, p2, p3, p4, p5, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMmCodeQCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            //var p1 = form.Get("p1");
            //var p2 = form.Get("p2");
            //var p3 = form.Get("p3");
            //var p4 = form.Get("p4");
            //var p5 = form.Get("p5");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0037Repository repo = new AA0037Repository(DBWork);
                    session.Result.etts = repo.GetMmCodeQCombo(p0, page, limit, "");
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
                    AA0037Repository repo = new AA0037Repository(DBWork);
                    AA0037Repository.MI_MAST_QUERY_PARAMS query = new AA0037Repository.MI_MAST_QUERY_PARAMS();
                    query.MMCODE = form.Get("MMCODE") == null ? "" : form.Get("MMCODE").ToUpper();
                    query.MMNAME_C = form.Get("MMNAME_C") == null ? "" : form.Get("MMNAME_C").ToUpper();
                    query.MMNAME_E = form.Get("MMNAME_E") == null ? "" : form.Get("MMNAME_E").ToUpper();
                    query.MAT_CLASS = form.Get("MAT_CLASS") == null ? "" : form.Get("MAT_CLASS").ToUpper();
                    query.WH_NO = form.Get("WH_NO") == null ? "" : form.Get("WH_NO").ToUpper();
                    query.M_STOREID = form.Get("M_STOREID") == null ? "" : form.Get("M_STOREID").ToUpper();
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
                    AA0037Repository repo = new AA0037Repository(DBWork);
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
                    AA0037Repository repo = new AA0037Repository(DBWork);
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
        public ApiResponse GetTowhCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0037Repository repo = new AA0037Repository(DBWork);
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
                    AA0037Repository repo = new AA0037Repository(DBWork);
                    //var taskid = repo.GetTaskid(User.Identity.Name);
                    //if (taskid == "2")
                    //{
                    //    session.Result.etts = repo.GetMatclass2Combo();
                    //}
                    //else
                    //{
                    //    session.Result.etts = repo.GetMatclass3Combo();
                    //}
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
                    AA0037Repository repo = new AA0037Repository(DBWork);
                    var v_userid = User.Identity.Name;
                    var v_ip = DBWork.ProcIP;
                    session.Result.etts = repo.GetLoginInfo(v_userid, v_ip);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        
        [HttpPost]
        public ApiResponse GetStoreidCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0037Repository repo = new AA0037Repository(DBWork);
                    session.Result.etts = repo.GetStoreidCombo();
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
                    AA0037Repository repo = new AA0037Repository(DBWork);
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
        public ApiResponse GetDoctypeCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0037Repository repo = new AA0037Repository(DBWork);
                    session.Result.etts = repo.GetDoctypeCombo();
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