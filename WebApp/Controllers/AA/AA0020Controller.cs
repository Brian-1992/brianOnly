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
    public class AA0020Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
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
                    var repo = new AA0020Repository(DBWork);

                    session.Result.etts = repo.GetAllM(p0, p1, p2, p3, page, limit, sorters);
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
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0020Repository(DBWork);
                    session.Result.etts = repo.GetAllD(p0, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse CreateD(ME_DOCEXP me_docexp)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0020Repository(DBWork);

                    var v_docno = repo.GetDocno();
                    var v_mmcode = me_docexp.MMCODE;
                    var v_mmcode1 = me_docexp.MMCODE1;
                    var v_expdate = me_docexp.EXP_DATET;
                    var v_expdatet = repo.GetTwndate(me_docexp.EXP_DATET);
                    var v_lot_no = me_docexp.LOT_NO;
                    var v_procid = me_docexp.C_TYPE;
                    var v_expqty = me_docexp.EXP_QTY;
                    var v_apvqty = double.Parse(me_docexp.APVQTY);
                    var v_contprice = double.Parse(me_docexp.M_CONTPRICE);
                    var v_discperc = me_docexp.M_DISCPERC;
                    var v_mmcode_o = me_docexp.MMCODE_O;
                    var v_expdate_o = me_docexp.EXP_DATE_O;
                    var v_expdatet_o = me_docexp.EXP_DATE;
                    var v_expdatet_n = me_docexp.EXP_DATE;
                    var v_exp_date_o = v_expdate_o.Substring(0, 4) + '/' + v_expdate_o.Substring(4, 2) + '/' + v_expdate_o.Substring(6, 2);
                    var v_lot_no_o = me_docexp.LOT_NO_O;
                    var v_expqty_o = me_docexp.EXP_QTY_O;
                    var v_memo = me_docexp.MEMO_O;
                    // FOR RDOCNO
                    ME_EXPM me_expm = new ME_EXPM();
                    me_expm.MMCODE = v_mmcode_o;
                    me_expm.EXP_DATE = v_expdatet_o;
                    me_expm.LOT_NO = v_lot_no_o;
                    me_expm.RDOCNO = v_docno;
                    me_expm.CLOSEFLAG = "N";
                    me_expm.UPDATE_USER = User.Identity.Name;
                    me_expm.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.UpdateMeexpm(me_expm);
                    //(1)
                    ME_DOCM me_docm = new ME_DOCM();
                    me_docm.CREATE_USER = User.Identity.Name;
                    me_docm.UPDATE_USER = User.Identity.Name;
                    me_docm.UPDATE_IP = DBWork.ProcIP;
                    me_docm.DOCNO = v_docno;
                    me_docm.DOCTYPE = "XE";
                    me_docm.FLOWID = "140" + v_procid;
                    me_docm.FRWH = "PH1S";
                    me_docm.TOWH = "PH1S";
                    me_docm.MAT_CLASS = "01";
                    me_docm.APPLY_NOTE = v_mmcode + v_expdate + v_lot_no ;
                    session.Result.afrs = repo.CreateMedocm(me_docm);
                    //(2)
                    me_docexp.UPDATE_USER = User.Identity.Name;
                    me_docexp.UPDATE_IP = DBWork.ProcIP;
                    if (!repo.CheckExistsExp(v_docno)) //若找無
                    {
                        me_docexp.DOCNO = v_docno;
                        me_docexp.SEQ = "2";
                        me_docexp.MMCODE = v_mmcode;
                        me_docexp.APVQTY = v_apvqty.ToString();
                        me_docexp.EXP_DATE = v_expdate;
                        me_docexp.LOT_NO = me_docexp.LOT_NO;
                        me_docexp.ITEM_NOTE = me_docexp.ITEM_NOTE;
                        session.Result.afrs = repo.CreateMedocExp(me_docexp);

                        me_docexp.DOCNO = v_docno;
                        me_docexp.SEQ = "1";
                        me_docexp.MMCODE = v_mmcode_o;
                        me_docexp.APVQTY = "-" + v_expqty_o.ToString();
                        me_docexp.EXP_DATE = v_exp_date_o;
                        me_docexp.LOT_NO = me_docexp.LOT_NO_O;
                        me_docexp.ITEM_NOTE = me_docexp.MEMO_O;
                        session.Result.afrs = repo.CreateMedocExp(me_docexp);

                    }
                    else
                    {
                        me_docexp.DOCNO = v_docno;
                        me_docexp.SEQ = "2";
                        me_docexp.MMCODE = v_mmcode;
                        me_docexp.APVQTY = v_apvqty.ToString();
                        me_docexp.EXP_DATE_O = me_docexp.EXP_DATE;
                        me_docexp.LOT_NO = me_docexp.LOT_NO;
                        me_docexp.ITEM_NOTE = me_docexp.ITEM_NOTE;
                        session.Result.afrs = repo.UpdateMedoce(me_docexp);
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
        public ApiResponse UpdateD(ME_DOCEXP me_docexp)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0020Repository(DBWork);

                    var v_docno = me_docexp.DOCNO;
                    var v_seq = me_docexp.SEQ;
                    var v_mmcode = me_docexp.MMCODE;
                    var v_mmcode1 = me_docexp.MMCODE1;
                    var v_expdate = me_docexp.EXP_DATET;
                    var v_expdatet = repo.GetTwndate(me_docexp.EXP_DATET);
                    var v_lot_no = me_docexp.LOT_NO;
                    var v_procid = me_docexp.C_TYPE;
                    var v_expqty = me_docexp.EXP_QTY;
                    var v_apvqty = double.Parse(me_docexp.APVQTY);
                    var v_contprice = double.Parse(me_docexp.M_CONTPRICE);
                    var v_discperc = me_docexp.M_DISCPERC;
                    var v_mmcode_o = me_docexp.MMCODE_O;
                    var v_expdate_o = me_docexp.EXP_DATE_O;
                    var v_expdatet_o = me_docexp.EXP_DATE;
                    var v_lot_no_o = me_docexp.LOT_NO_O;
                    var v_expqty_o = me_docexp.EXP_QTY_O;
                    var v_memo = me_docexp.MEMO_O;
                    //(1)
                    //ME_DOCM me_docm = new ME_DOCM();
                    //me_docm.CREATE_USER = User.Identity.Name;
                    //me_docm.UPDATE_USER = User.Identity.Name;
                    //me_docm.UPDATE_IP = DBWork.ProcIP;
                    //me_docm.FLOWID = "140" + v_procid;
                    //me_docm.APPLY_NOTE = v_mmcode + v_expdate + v_lot_no;
                    //session.Result.afrs = repo.UpdateMedocm(me_docm);
                    //(2)
                    me_docexp.DOCNO = v_docno;
                    me_docexp.SEQ = "2";
                    me_docexp.MMCODE = v_mmcode;
                    me_docexp.APVQTY = v_apvqty.ToString();
                    me_docexp.EXP_DATE_O = me_docexp.EXP_DATE;
                    me_docexp.LOT_NO = me_docexp.LOT_NO;
                    me_docexp.ITEM_NOTE = me_docexp.ITEM_NOTE;
                    session.Result.afrs = repo.UpdateMedoce(me_docexp);
                    

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
        public ApiResponse Apply(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0020Repository repo = new AA0020Repository(DBWork);
                    string docno = form.Get("DOCNO");
                    string mmcode = form.Get("MMCODE");
                    string exp_date = form.Get("EXP_DATE");
                    string lot_no = form.Get("LOT_NO");
                    //(1)
                    var v_length = 0;
                    var v_mmcode = "";
                    var v_note = "";
                    var v_appqty = 0.0000;
                    v_length = int.Parse(repo.GetDocENum(docno));
                    for (int k = 1; k < (v_length + 1); k++)
                    {
                        v_mmcode = repo.GetDocEMmcode(docno, k.ToString());
                        if (!repo.CheckExistsDMmcode(docno, v_mmcode))
                        {
                            v_appqty = double.Parse(repo.GetDocEMmcodeApvqty(docno, v_mmcode));
                            v_note = repo.GetDocEMmcodeNote(docno, v_mmcode);
                            ME_DOCD me_docd = new ME_DOCD();
                            me_docd.DOCNO = docno;
                            if (!repo.CheckExistsD(docno))
                            {
                                me_docd.SEQ = "1";
                            }
                            else
                            {
                                me_docd.SEQ = repo.GetDocDSeq(docno);
                            }
                            if (v_appqty != 0)
                            {
                                if (v_appqty > 0)
                                {
                                    me_docd.STAT = "1";
                                }
                                else
                                {
                                    me_docd.STAT = "2";
                                }
                            }
                            me_docd.MMCODE = v_mmcode;
                            me_docd.APPQTY = v_appqty.ToString();
                            me_docd.APLYITEM_NOTE = v_note;
                            me_docd.UPDATE_USER = User.Identity.Name;
                            me_docd.UPDATE_IP = DBWork.ProcIP;
                            session.Result.afrs = repo.CreateMedocd(me_docd);
                        }
                    }

                    //(2)
                    var rtn = repo.CallProc(docno, User.Identity.Name, DBWork.ProcIP);
                    if ( rtn == "Y")
                    {
                        //(3)
                        ME_EXPM me_expm = new ME_EXPM();
                        me_expm.MMCODE = mmcode;
                        me_expm.EXP_DATE = exp_date;
                        me_expm.LOT_NO = lot_no;
                        me_expm.CLOSEFLAG = "Y";
                        me_expm.UPDATE_USER = User.Identity.Name;
                        me_expm.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.ApplyMeexpm(me_expm);
                        //(4) POST_DOC 有處理
                        //var flowid = repo.GetDocMFlowid(docno);
                        //var newflowid = "";
                        //if (flowid == "1401")
                        //{
                        //    newflowid = "1499";
                        //}
                        //else
                        //{
                        //    if (flowid == "1403")
                        //    {
                        //        newflowid = "1498";
                        //    }
                        //    else
                        //    {
                        //        newflowid = "1497";
                        //    }
                        //}
                        //ME_DOCM me_docm = new ME_DOCM();
                        //me_docm.UPDATE_USER = User.Identity.Name;
                        //me_docm.UPDATE_IP = DBWork.ProcIP;
                        //me_docm.DOCNO = docno;
                        //me_docm.FLOWID = newflowid;
                        //session.Result.afrs = repo.UpdateMedocm(me_docm);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>申請單號「" + docno + "」</span>，發生執行錯誤，" + rtn + "。";
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
        public ApiResponse GetAgenCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0020Repository repo = new AA0020Repository(DBWork);
                    session.Result.etts = repo.GetAgenCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetYyymmCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0020Repository repo = new AA0020Repository(DBWork);
                    session.Result.etts = repo.GetYyymmCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetProcidCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0020Repository repo = new AA0020Repository(DBWork);
                    session.Result.etts = repo.GetProcidCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetCloseflagCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0020Repository repo = new AA0020Repository(DBWork);
                    session.Result.etts = repo.GetCloseflagCombo();
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
                    AA0020Repository repo = new AA0020Repository(DBWork);
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
        public ApiResponse GetMmCodeCombo(FormDataCollection form)
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
                    AA0020Repository repo = new AA0020Repository(DBWork);
                    session.Result.etts = repo.GetMmCodeCombo(p0, page, limit, "");
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
                    AA0020Repository repo = new AA0020Repository(DBWork);
                    AA0020Repository.MI_MAST_QUERY_PARAMS query = new AA0020Repository.MI_MAST_QUERY_PARAMS();
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
                    AA0020Repository repo = new AA0020Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeDocd(p0, p1, page, limit, "");
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