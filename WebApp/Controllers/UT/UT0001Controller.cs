using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.UT;
using WebApp.Models.UT;
using System;
using WebApp.Models;

namespace WebApp.Controllers.UT
{
    public class UT0001Controller : SiteBase.BaseApiController
    {
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
                    var repo = new UT0001Repository(DBWork);
                    string list  = repo.GetDoctype(p0);
                    
                    session.Result.etts = repo.GetAllM(p0,p1, list.Split(';')[0], list.Split(';')[1], User.Identity.Name, page, limit, sorters);
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
            var doctype = form.Get("doctype");
            var edit_type = form.Get("edit_type");
            var docno = form.Get("docno");
            var flowid = form.Get("flowid");
            var wh_no = form.Get("p2");
            var seq = form.Get("p3");
            var mmcode = form.Get("p4");
            var suggest = form.Get("p5");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new UT0001Repository(DBWork);
                    if (suggest == "Y")
                    {
                        var qty = form.Get("p6");
                        session.Result.etts = repo.GetAllD3(doctype,wh_no, mmcode, qty, page, limit, sorters);
                    }
                    else
                    {
                        if (edit_type == "1") session.Result.etts = repo.GetAllD1(doctype,docno, wh_no, seq, mmcode, page, limit, sorters);
                        else session.Result.etts = repo.GetAllD2(flowid,doctype, docno, seq, page, limit, sorters);
                    }
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse CreateD(UT_DOCD d) 
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();

                try
                {
                    var repo = new UT0001Repository(DBWork);
                    d.UPDATE_USER = User.Identity.Name;
                    d.UPDATE_IP = DBWork.ProcIP;
                    string[] expArray = d.EXP_DATE_LIST.Split(',');
                    string[] lotArray = d.LOT_NO_LIST.Split(',');
                    string[] qtyArray = d.APVQTY_LIST.Split(',');
                    bool isOK = true;
                    //分批交貨
                    if (d.DOCTYPE == "MR3" | d.DOCTYPE == "MR4")
                    {
                        session.Result.afrs = repo.DeleteD2(d);
                        for (var i = 0; i < expArray.Length; i++)
                        {
                            d.EXP_DATE = DateTime.ParseExact(expArray[i].ToString(), "yyyy/MM/dd", System.Globalization.CultureInfo.InvariantCulture);
                            d.LOT_NO = lotArray[i].ToString();
                            d.APVQTY = qtyArray[i].ToString();
                            if (repo.CheckExistsD2Key(d)) {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = "<span style='color:red'>效期與批號</span>重複，請重新輸入。";
                                isOK = false;
                                break;
                            } else {
                                session.Result.afrs = repo.CreateD2(d);
                                if (session.Result.afrs == 0)
                                {
                                    isOK = false;
                                    break;
                                }
                            }

                        }
                    }
                    else {
                        session.Result.afrs = repo.DeleteD(d);
                        for (var i = 0; i < expArray.Length; i++)
                        {
                            d.EXP_DATE = DateTime.ParseExact(expArray[i].ToString(), "yyyy/MM/dd", System.Globalization.CultureInfo.InvariantCulture);
                            d.LOT_NO = lotArray[i].ToString();
                            d.APVQTY = qtyArray[i].ToString();
                            if (repo.CheckExistsDKey(d))
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = "<span style='color:red'>效期與批號</span>重複，請重新輸入。";
                                isOK = false;
                                break;
                            }
                            else
                            {
                                session.Result.afrs = repo.CreateD(d);
                                if (session.Result.afrs == 0)
                                {
                                    isOK = false;
                                    break;
                                }
                            }
                        }
                    }


                    if (isOK)
                        DBWork.Commit();
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
        [HttpPost]
        public ApiResponse Move(FormDataCollection form)
        {
            var docno = form.Get("DOCNO");
            var seq_list = form.Get("SEQ_LIST");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                bool isOK = true;
                try
                {
                    var repo = new UT0002Repository(DBWork);

                    string[] seqArray = seq_list.Split(',');

                    for (var i = 0; i < seqArray.Length; i++)
                    {
                        SP_MODEL sp = repo.WEXPINV_R(docno, Convert.ToInt16(seqArray[i]), User.Identity.Name, DBWork.ProcIP);
                        if (sp.O_RETID == "N")
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = sp.O_ERRMSG;
                            isOK = false;
                            break;
                        }
                    }
                    if (isOK)
                    {
                        DBWork.Commit();
                    }
                    else
                    {
                        DBWork.Rollback();
                    }
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
            //var docno = form.Get("p0");
            //var mmcode = form.Get("p1");
            //using (WorkSession session = new WorkSession(this))
            //{
            //    var DBWork = session.UnitOfWork;
            //    DBWork.BeginTransaction();
            //    bool isOK = true;
            //    try
            //    {
            //        var repo = new UT0002Repository(DBWork);

            //        SP_MODEL sp = repo.WEXPINV_R(docno, mmcode, User.Identity.Name, DBWork.ProcIP);
            //        if (sp.O_RETID == "N")
            //        {
            //            session.Result.afrs = 0;
            //            session.Result.success = false;
            //            session.Result.msg = sp.O_ERRMSG;
            //            isOK = false;
            //        }
            //        if (isOK)
            //        {
            //            DBWork.Commit();
            //            session.Result.afrs = 1;
            //        }
            //        else
            //        {
            //            DBWork.Rollback();
            //        }
            //    }
            //    catch
            //    {
            //        DBWork.Rollback();
            //        throw;
            //    }
            //    return session.Result;
            //}
        }
    }
}