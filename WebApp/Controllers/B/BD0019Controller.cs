using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.B;
using WebApp.Models;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System;
using System.Web;
using NPOI.SS.UserModel;
using System.Data;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;

namespace WebApp.Controllers.B
{
    public class BD0019Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse GetMST(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0019Repository(DBWork);
                    session.Result.etts = repo.GetMST(p0, p1, p2, p3, DBWork.UserInfo.Inid, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetD(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var wh_no = form.Get("wh_no");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    BD0019Repository repo = new BD0019Repository(DBWork);
                    session.Result.etts = repo.GetD(p0, p1, wh_no, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse MasterCreate(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BD0019Repository repo = new BD0019Repository(DBWork);
                    MM_PR_M mm_pr_m = new MM_PR_M();

                    mm_pr_m.PR_USER = User.Identity.Name;
                    mm_pr_m.CREATE_USER = User.Identity.Name;
                    mm_pr_m.UPDATE_IP = DBWork.ProcIP;
                    mm_pr_m.PR_DEPT = DBWork.UserInfo.Inid;
                    mm_pr_m.MAT_CLASS = form.Get("MAT_CLASS");
                    mm_pr_m.MEMO = form.Get("MEMO");

                    if (mm_pr_m.M_STOREID == "0")
                    {
                        session.Result.success = false;
                        session.Result.msg = "不可新增非庫備申購單";
                        return session.Result;
                    }


                    session.Result.msg = repo.MasterCreate(mm_pr_m);
                    //session.Result.etts = repo.Get(me_docm.DOCNO);

                    //session.Result.afrs = 0;
                    //session.Result.success = false;

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
        public ApiResponse MasterDelete(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BD0019Repository repo = new BD0019Repository(DBWork);
                    if (form.Get("PR_NO") != "")
                    {
                        string[] strSeparating = { "<br>" };
                        string[] strProNoList = form.Get("PR_NO").Split(strSeparating, System.StringSplitOptions.RemoveEmptyEntries);

                        foreach (string strProNo in strProNoList)
                        {
                            string chkMsg = chkPrStatus(strProNo);

                            if (chkMsg == "")
                            {
                                repo.DetailDeleteAll(strProNo);
                                session.Result.afrs = repo.MasterDelete(strProNo);
                            }
                            else
                            {
                                DBWork.Rollback();
                                session.Result.msg = chkMsg;
                                session.Result.success = false;
                                return session.Result;
                            }
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
        public ApiResponse MasterUpdate(MM_PR_M mm_pr_m)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BD0019Repository repo = new BD0019Repository(DBWork);
                    mm_pr_m.UPDATE_USER = User.Identity.Name;
                    mm_pr_m.UPDATE_IP = DBWork.ProcIP;

                    session.Result.afrs = repo.MasterUpdate(mm_pr_m);
                    //session.Result.etts = repo.Get(me_docm.DOCNO);

                    DBWork.Commit();
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse MasterTrans(FormDataCollection form)
        {
            string itemString = form.Get("ITEM_STRING");
            IEnumerable<MM_PR_D> items = JsonConvert.DeserializeObject<IEnumerable<MM_PR_D>>(itemString);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BD0019Repository(DBWork);
                    string pr_no_string = string.Empty;
                    foreach (MM_PR_D item in items)
                    {
                        if (pr_no_string != string.Empty)
                        {
                            pr_no_string += ",";
                        }
                        pr_no_string += string.Format("'{0}'", item.PR_NO);
                    }

                    // 檢查單據狀態是否為35
                    IEnumerable<string> pr_no_not35_list = repo.CheckPrStatus(pr_no_string);
                    if (pr_no_not35_list.Any())
                    {
                        string pr_no_not35 = string.Empty;

                        foreach (string item in pr_no_not35_list)
                        {
                            if (pr_no_not35 != string.Empty)
                            {
                                pr_no_string += "、";
                            }
                            pr_no_not35 += item;
                        }

                        session.Result.success = false;
                        session.Result.msg = string.Format("單據狀態不為開立中，請重新確認：<br>{0}", pr_no_not35);

                        return session.Result;
                    }

                    // 1. 取得 mat_class, agen_no, m_contid
                    IEnumerable<MM_PR_D> temps = repo.GetAgaennoMContids(pr_no_string);

                    // 2. 逐筆處理
                    foreach (MM_PR_D temp in temps)
                    {
                        //// 2.1 取得單號，衛材開頭INV，其他GEN
                        //string v_pono_pre = temp.MAT_CLASS == "02" ? "INV" : "GEN";
                        //string today = repo.GetTodayDate();
                        //string seq = repo.GetPoMSeq(v_pono_pre, temp.M_CONTID, today);
                        //string v_pono = string.Format("{0}{1}{2}{3}", v_pono_pre, temp.M_CONTID, today, seq.PadLeft(4,'0'));
                        // 2.1 取得單號，改以呼叫SP處理 2023-06-15
                        string v_pono = repo.GetPono();

                        string memo = repo.GetMmPrMMemos(pr_no_string, temp.AGEN_NO, temp.M_CONTID, temp.MAT_CLASS);
                        // 2.2 insert into MM_PO_M
                        session.Result.afrs = repo.InsertMmPoM(v_pono, temp.MAT_CLASS, temp.AGEN_NO, temp.M_CONTID,
                                memo, string.Empty, string.Empty,
                                DBWork.UserInfo.UserId, DBWork.ProcIP);
                        // 2.3 insert into MM_PO_D
                        session.Result.afrs = repo.InsertMmPoD(v_pono, pr_no_string, temp.MAT_CLASS, temp.AGEN_NO, temp.M_CONTID, DBWork.UserInfo.UserId, DBWork.ProcIP);

                        // 2.4 insert into PH_INVOICE
                        session.Result.afrs = repo.InsertPhInvoice(v_pono, pr_no_string, temp.MAT_CLASS, temp.AGEN_NO, temp.M_CONTID, DBWork.UserInfo.UserId, DBWork.ProcIP);

                        // 2.5.更新pr_po_no
                        session.Result.afrs = repo.UpdateMmPrD(v_pono, pr_no_string, temp.MAT_CLASS, temp.AGEN_NO, temp.M_CONTID, DBWork.UserInfo.UserId, DBWork.ProcIP);
                    }

                    // 3 更新MM_PO_M status
                    session.Result.afrs = repo.UpdateMmPrM(pr_no_string, DBWork.UserInfo.UserId, DBWork.ProcIP);

                    DBWork.Commit();
                    session.Result.success = true;
                    return session.Result;

                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    throw;
                }
            }
        }


        [HttpPost]
        public ApiResponse MasterTotal(FormDataCollection form)
        {
            string itemString = form.Get("ITEM_STRING");
            IEnumerable<MM_PR_D> items = JsonConvert.DeserializeObject<IEnumerable<MM_PR_D>>(itemString);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BD0019Repository(DBWork);
                    string pr_no_string = string.Empty;
                    string prno = string.Empty;

                    foreach (MM_PR_D item in items)
                    {
                        if (pr_no_string != string.Empty)
                        {
                            pr_no_string += ",";
                        }
                        pr_no_string += string.Format("'{0}'", item.PR_NO);
                    }

                    // 檢查單據狀態是否為35
                    IEnumerable<string> pr_no_not35_list = repo.CheckPrStatus(pr_no_string);
                    if (pr_no_not35_list.Any())
                    {
                        string pr_no_not35 = string.Empty;

                        foreach (string item in pr_no_not35_list)
                        {
                            if (pr_no_not35 != string.Empty)
                            {
                                pr_no_string += "、";
                            }
                            pr_no_not35 += item;
                        }

                        session.Result.success = false;
                        session.Result.msg = string.Format("單據狀態不為開立中，請重新確認：<br>{0}", pr_no_not35);

                        return session.Result;
                    }

                    // 2. 逐筆處理
                    //新建一筆MM_PR_M
                    string v_prno = repo.Getsumno(DBWork.UserInfo.Inid, form.Get("MAT_CLASS"));
                    //檢查prno是否已存在於MM_PR_M，若存在，等一秒後再執行a，直到v_prno不存在於MM_PR_M
                    while (repo.CheckExistsM(v_prno) == true)
                    {
                        System.Threading.Thread.Sleep(1000);
                        v_prno = repo.Getsumno(DBWork.UserInfo.Inid, form.Get("MAT_CLASS"));
                    }
                    BD0019Repository repo1 = new BD0019Repository(DBWork);
                    MM_PR_M mm_pr_m = new MM_PR_M();

                    mm_pr_m.PR_USER = User.Identity.Name;
                    mm_pr_m.CREATE_USER = User.Identity.Name;
                    mm_pr_m.UPDATE_IP = DBWork.ProcIP;
                    mm_pr_m.PR_DEPT = DBWork.UserInfo.Inid;
                    mm_pr_m.MAT_CLASS = form.Get("MAT_CLASS");
                    mm_pr_m.PR_NO = v_prno;
                    session.Result.afrs = repo.totalCreate(mm_pr_m);
                    //將勾選單據明細匯總至新PR_NO
                    session.Result.afrs = repo.InsertMmPrD(v_prno, pr_no_string, DBWork.UserInfo.UserId, DBWork.ProcIP);
                    //勾選單據明細寫入LOG後刪除
                    session.Result.afrs = repo.InsertMmPrDLog(pr_no_string);
                   
                    session.Result.afrs = repo.DeleteMmPrD(pr_no_string);
                    //勾選單據寫入LOG後刪除
                    session.Result.afrs = repo.InsertMmPrMLog(v_prno, pr_no_string, DBWork.UserInfo.UserId);

                    session.Result.afrs = repo.DeleteMmPrM(pr_no_string);

                    // 3 更新MM_PO_M status
                    DBWork.Commit();
                    session.Result.success = true;
                    return session.Result;

                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    throw;
                }
            }
        }

        //[HttpPost]
        //public ApiResponse AllMeDocd(FormDataCollection form)
        //{
        //    var page = int.Parse(form.Get("page"));
        //    var start = int.Parse(form.Get("start"));
        //    var limit = int.Parse(form.Get("limit"));
        //    var sorters = form.Get("sort");

        //    using (WorkSession session = new WorkSession())
        //    {
        //        var DBWork = session.UnitOfWork;
        //        try
        //        {
        //            var repo = new AA0035Repository(DBWork);
        //            string docno = form.Get("p0");
        //            string wh_no = form.Get("p1");
        //            //session.Result.etts = repo.GetMeDocds(docno, wh_no, page, limit, sorters);
        //            var docds = repo.GetMeDocds(docno, wh_no, page, limit, sorters);

        //            foreach (ME_DOCD docd in docds)
        //            {
        //                docd.AMOUNT = (double.Parse(docd.APVQTY) * double.Parse(docd.M_CONTPRICE)).ToString();
        //            }

        //            session.Result.etts = docds;

        //        }
        //        catch
        //        {
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //}

        //[HttpPost]
        //public ApiResponse AllMeDoce(FormDataCollection form)
        //{
        //    var docno = form.Get("p0");
        //    var seq = form.Get("p1");

        //    using (WorkSession session = new WorkSession())
        //    {
        //        var DBWork = session.UnitOfWork;
        //        try
        //        {
        //            var repo = new AA0035Repository(DBWork);
        //            session.Result.etts = repo.GetMeDoces(docno, seq);
        //        }
        //        catch
        //        {
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //}

        //[HttpPost]
        //public ApiResponse GetMmdataByMmcode(FormDataCollection form)
        //{
        //    string mmcode = form.Get("mmcode");
        //    string wh_no = form.Get("wh_no");

        //    using (WorkSession session = new WorkSession())
        //    {
        //        UnitOfWork DBWork = session.UnitOfWork;
        //        try
        //        {
        //            AA0076Repository repo = new AA0076Repository(DBWork);
        //            if (!repo.CheckWhmmExist(mmcode, wh_no))
        //            {
        //                session.Result.afrs = 0;
        //                session.Result.success = false;
        //                session.Result.msg = "<span style='color:red'>此院內碼不存在於此庫房中</span>，請重新輸入。";

        //                return session.Result;
        //            }

        //            session.Result.etts = repo.GetMmdataByMmcode(mmcode);
        //        }
        //        catch
        //        {
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //}

        //[HttpPost]
        //public ApiResponse GetMeDocdMaxSeq(ME_DOCD me_docd)
        //{
        //    string docno = me_docd.DOCNO;
        //    using (WorkSession session = new WorkSession())
        //    {
        //        UnitOfWork DBWork = session.UnitOfWork;
        //        try
        //        {
        //            AA0035Repository repo = new AA0035Repository(DBWork);
        //            List<string> result = new List<string>();
        //            session.Result.etts = repo.GetMeDocdMaxSeq(docno);
        //        }
        //        catch
        //        {
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //}

        [HttpPost]
        public ApiResponse DetailCreate(MM_PR_D mm_pr_d)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BD0019Repository repo = new BD0019Repository(DBWork);
                    if (repo.CheckMmPrDExists(mm_pr_d.PR_NO, mm_pr_d.MMCODE,
                                                                       (string.IsNullOrEmpty(mm_pr_d.CHINNAME) ? null : mm_pr_d.CHINNAME.Trim()),
                                                                       (string.IsNullOrEmpty(mm_pr_d.CHARTNO) ? null : mm_pr_d.CHARTNO.Trim()),
                                                                       (string.IsNullOrEmpty(mm_pr_d.MEMO) ? null : mm_pr_d.MEMO.Trim())))
                    {
                        session.Result.success = false;
                        session.Result.msg = "院內碼、病患姓名、病歷號、備註 已存在於申購單，請重新確認";
                        return session.Result;
                    }
                    mm_pr_d.CREATE_USER = User.Identity.Name;
                    mm_pr_d.UPDATE_IP = DBWork.ProcIP;
                    int unitRate = repo.GetUnitRate(mm_pr_d.MMCODE);
                    //1121009新竹 檢查出貨單位 不卡關改提示(前端處理)
                    /* if (mm_pr_d.PR_QTY % unitRate != 0) {
                         session.Result.success = false;
                         session.Result.msg = "申購量不為出貨單位倍數，請重新確認";
                         return session.Result;
                     }*/

                    //if (repo.CheckDetailMmcodedExists(mm_pr_d.PR_NO, mm_pr_d.MMCODE))
                    //{

                    MI_MAST mi_mast = repo.GetMiMast(mm_pr_d.MMCODE);
                    mm_pr_d.DISC_CPRICE = mi_mast.DISC_CPRICE;
                    mm_pr_d.DISC_UPRICE = mi_mast.DISC_UPRICE;
                    mm_pr_d.UPRICE = mi_mast.UPRICE;
                    mm_pr_d.M_DISCPERC = mi_mast.M_DISCPERC;

                    session.Result.afrs = repo.DetailCreate(mm_pr_d);
                    DBWork.Commit();
                    //}
                    //else
                    //{
                    //    session.Result.success = false;
                    //    session.Result.msg = "院內碼重複申請請確認!!";
                    //}

                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }
        //public ApiResponse CreateMeDoec(ME_DOCD me_docd)
        //{
        //    string docno = me_docd.DOCNO;
        //    string seq = me_docd.SEQ;
        //    string wh_no = me_docd.WH_NO;

        //    using (WorkSession session = new WorkSession())
        //    {
        //        UnitOfWork DBWork = session.UnitOfWork;
        //        try
        //        {
        //            AA0035Repository repo = new AA0035Repository(DBWork);
        //            var result = repo.CreateMeDoce(docno, seq, wh_no);



        //            session.Result.msg = result;
        //        }
        //        catch
        //        {
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //}

        [HttpPost]
        public ApiResponse DetailDelete(MM_PR_D mm_pr_d)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BD0019Repository repo = new BD0019Repository(DBWork);


                    session.Result.afrs = repo.DetailDelete(mm_pr_d.PR_NO, mm_pr_d.MMCODE, mm_pr_d.Seq);

                    //2022-06-16: 修改申請單註記
                    session.Result.afrs = repo.UpdateMedocd(mm_pr_d.PR_NO, mm_pr_d.MMCODE);

                    //if (me_doces.Any())
                    //{
                    //    int medoceResult = repo.MeDoceDelete(tempDocnos[0], tempSeqs[i]);

                    //    if (medoceResult < 1)
                    //    {
                    //        DBWork.Rollback();
                    //        session.Result.success = false;
                    //        session.Result.msg = "細項刪除失敗";
                    //        return session.Result;
                    //    }
                    //}



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
        public ApiResponse DetailUpdate(MM_PR_D mm_pr_d)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BD0019Repository repo = new BD0019Repository(DBWork);
                    mm_pr_d.UPDATE_USER = User.Identity.Name;
                    mm_pr_d.UPDATE_IP = DBWork.ProcIP;

                    MM_PR_M prData = repo.GetChkPrStatus(mm_pr_d.PR_NO);

                    int unitRate = repo.GetUnitRate(mm_pr_d.MMCODE);
                    int i;
                    if (prData.ISFROMDOCM == "Y" && (mm_pr_d.PR_QTY < mm_pr_d.SRC_PR_QTY))
                    {
                        session.Result.success = false;
                        session.Result.msg = "轉申購之單據數量不可小於原需求量";
                        return session.Result;
                    }
                    //1121009新竹 檢查出貨單位 不卡關改提示(前端處理)
                    /* if (mm_pr_d.PR_QTY % unitRate != 0)
                     {
                         session.Result.success = false;
                         session.Result.msg = "申購量不為出貨單位倍數，請重新確認";
                         return session.Result;
                     }*/

                    session.Result.afrs = repo.DetailUpdate(mm_pr_d);


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

        //public ApiResponse MeDoceUpdate(ME_DOCE me_doce)
        //{

        //    IEnumerable<ME_DOCE> me_doces = JsonConvert.DeserializeObject<IEnumerable<ME_DOCE>>(me_doce.ITEM_STRING);

        //    using (WorkSession session = new WorkSession(this))
        //    {
        //        var DBWork = session.UnitOfWork;
        //        DBWork.BeginTransaction();
        //        try
        //        {
        //            AA0035Repository repo = new AA0035Repository(DBWork);

        //            foreach (ME_DOCE doce in me_doces)
        //            {
        //                doce.UPDATE_USER = User.Identity.Name;
        //                doce.UPDATE_IP = DBWork.ProcIP;

        //                session.Result.afrs = repo.MeDoceUpdate(doce);
        //            }

        //            DBWork.Commit();
        //        }
        //        catch
        //        {
        //            DBWork.Rollback();
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //}

        [HttpPost]
        public ApiResponse GetMasterExceedAmtMMCodes(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0019Repository(DBWork);

                    string[] strSeparating = { "<br>" };
                    string[] strProNoList = form.Get("PR_NO").Split(strSeparating, System.StringSplitOptions.RemoveEmptyEntries);

                    foreach (string strPrNo in strProNoList)
                    {
                        IEnumerable<MM_PR_D> prnoList = repo.GetExceedAmtMMCodes(strPrNo);
                        if (prnoList.Count() > 0)
                        {
                            string strMMcodes = string.Join(", ", prnoList.Select(obj => obj.MMCODE));
                            string chkMsg = $"{strPrNo} = {strMMcodes} = 非合約累計採購金額預計超過(含)十五萬元，是否仍要產生訂單 ?";

                            session.Result.msg = chkMsg;
                            session.Result.etts = prnoList;
                            session.Result.success = true;
                            return session.Result;
                        }
                    }
                }
                catch (Exception e)
                {
                    throw;
                }

                session.Result.success = true;
                return session.Result;
            }
        }
        [HttpPost]
        //檢查轉訂單是否有明細資料
        public ApiResponse GetDetailDataForT1TransOrders(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var wh_no = form.Get("wh_no");
            var page = 1;
            var limit = 100;
            var sorters = "";

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    BD0019Repository repo = new BD0019Repository(DBWork);
                    session.Result.etts = repo.GetD(p0, p1, wh_no, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetMATQCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0019Repository(DBWork);
                    session.Result.etts = repo.GetMATQCombo();
                }
                catch
                {
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
                    var repo = new BD0019Repository(DBWork);
                    session.Result.etts = repo.GetMATCombo();
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
                    var repo = new BD0019Repository(DBWork);
                    session.Result.etts = repo.GetMmcodeCombo(MAT_CLASS, p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetSelectMmcodeDetail(string MMCODE, string MAT_CLASS, string WH_NO)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0019Repository(DBWork);
                    session.Result.etts = repo.GetSelectMmcodeDetail(MMCODE, MAT_CLASS, WH_NO);
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
                    var repo = new BD0019Repository(DBWork);
                    session.Result.etts = repo.GetWh_noCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetTot(string wh_no, string mat_class, string mmcode, string totprice)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BD0019Repository repo = new BD0019Repository(DBWork);
                    session.Result.msg = repo.GetTot(mmcode, totprice);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //匯入檢核
        [HttpPost]
        public ApiResponse SendExcel()
        {
            using (WorkSession session = new WorkSession(this))
            {

                List<MM_PR01_D> list = new List<MM_PR01_D>();
                List<MM_PR01_D> ori_list = new List<MM_PR01_D>();
                UnitOfWork DBWork = session.UnitOfWork;
                var HttpPostedFile = HttpContext.Current.Request.Files["file"];
                var v_prno = HttpContext.Current.Request.Form["pr_no"];
                IWorkbook workBook;

                try
                {
                    BD0019Repository repo = new BD0019Repository(DBWork);
                    bool checkPassed = true; //檢核有沒有通過 有通過就true 沒通過就false

                    #region 檢查檔案格式
                    if (Path.GetExtension(HttpPostedFile.FileName).ToLower() == ".xls")
                    {
                        workBook = new HSSFWorkbook(HttpPostedFile.InputStream); //讀取xls檔
                    }
                    else
                    {
                        workBook = new XSSFWorkbook(HttpPostedFile.InputStream); //讀取xlsx檔
                    }

                    var sheet = workBook.GetSheetAt(0); //讀取EXCEL的第一個分頁
                    //IRow headerRow = sheet.GetRow(0); //由第一列取標題做為欄位名稱
                    IRow headerRow = sheet.GetRow(0); //由第二列取標題做為欄位名稱
                    int cellCount = headerRow.LastCellNum; //欄位數目
                    //int i, j;

                    #region excel欄位檢查
                    List<HeaderItem> headerItems = new List<HeaderItem>() {
                        new HeaderItem("院內碼", "MMCODE"),
                        new HeaderItem("申購數量", "PR_QTY"),
                        new HeaderItem("備註", "MEMO"),
                        new HeaderItem("病患姓名","CHINNAME"),
                        new HeaderItem("病歷號","CHARTNO")
                    };

                    List<HeaderItem> headerItems2 = new List<HeaderItem>() {
                        new HeaderItem("院內碼", "MMCODE"),
                        new HeaderItem("申購數量", "PR_QTY"),
                        new HeaderItem("備註", "MEMO")
                    };

                    headerItems = SetHeaderIndex(headerItems, headerRow);
                    headerItems2 = SetHeaderIndex(headerItems2, headerRow);
                    // 確定該有的欄位都有，沒有的回傳錯誤訊息
                    string errMsg = string.Empty;
                    string errMsg2 = string.Empty;
                    foreach (HeaderItem item in headerItems)
                    {
                        if (item.Index == -1)
                        {
                            if (item.Name != "病歷號") // Excel檔允許沒有病歷號
                            {
                                if (errMsg == string.Empty)
                                {
                                    errMsg += item.Name;
                                }
                                else
                                {
                                    errMsg += string.Format("、{0}", item.Name);
                                }
                            }
                        }
                    }

                    //如果範本不符合 使用廠商提供格式
                    if (errMsg != string.Empty)
                    {
                        foreach (HeaderItem item in headerItems2)
                        {
                            if (item.Index == -1)
                            {
                                if (errMsg2 == string.Empty)
                                {
                                    errMsg2 += item.Name;
                                }
                                else
                                {
                                    errMsg2 += string.Format("、{0}", item.Name);
                                }
                            }
                        }
                        if (errMsg2 != string.Empty)
                        {
                            session.Result.success = false;
                            session.Result.msg = string.Format("欄位錯誤，缺：{0}", errMsg2);
                            return session.Result;
                        }

                    }
                    #endregion
                    #region 資料轉成list
                    for (int i = 1; i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row is null) { continue; }

                        MM_PR01_D temp = new MM_PR01_D();

                        if (errMsg == string.Empty)
                        {
                            foreach (HeaderItem item in headerItems)
                            {
                                string value = "";

                                if (row.GetCell(item.Index) == null)
                                {
                                    continue;
                                }

                                if (item.Name == "申購數量")
                                {
                                    if (row.GetCell(item.Index).CellType == CellType.Error)
                                    {
                                        value = null;
                                    }
                                    else if (row.GetCell(item.Index).CellType == CellType.Formula)
                                    {
                                        if (row.GetCell(item.Index).CachedFormulaResultType == CellType.Error)
                                        {
                                            value = null;
                                        }
                                        else
                                        {
                                            value = row.GetCell(item.Index) == null ? string.Empty : row.GetCell(item.Index).NumericCellValue.ToString();
                                        }
                                    }
                                    else
                                    {
                                        value = row.GetCell(item.Index) == null ? string.Empty : row.GetCell(item.Index).NumericCellValue.ToString();
                                    }
                                }
                                else
                                {
                                    value = row.GetCell(item.Index) == null ? string.Empty : row.GetCell(item.Index).ToString();

                                }
                                temp.GetType().GetProperty(item.FieldName).SetValue(temp, value, null);
                            }
                        }
                        else
                        {
                            foreach (HeaderItem item in headerItems2)
                            {

                                string value = "";

                                if (row.GetCell(item.Index) == null)
                                {
                                    continue;
                                }

                                if (item.Name == "申購數量")
                                {
                                    if (row.GetCell(item.Index).CellType == CellType.Error)
                                    {
                                        value = null;
                                    }
                                    else if (row.GetCell(item.Index).CellType == CellType.Formula)
                                    {
                                        if (row.GetCell(item.Index).CachedFormulaResultType == CellType.Error)
                                        {
                                            value = null;
                                        }
                                        else
                                        {
                                            value = row.GetCell(item.Index) == null ? string.Empty : row.GetCell(item.Index).NumericCellValue.ToString();
                                        }
                                    }
                                    else
                                    {
                                        value = row.GetCell(item.Index) == null ? string.Empty : row.GetCell(item.Index).NumericCellValue.ToString();
                                    }
                                }
                                else
                                {
                                    value = row.GetCell(item.Index) == null ? string.Empty : row.GetCell(item.Index).ToString();

                                }
                                temp.GetType().GetProperty(item.FieldName).SetValue(temp, value, null);
                            }
                        }
                        temp.Seq = i - 1;
                        ori_list.Add(temp);
                    }
                    #endregion

                    #endregion





                    DataTable dtTable = new DataTable();
                    DataRow datarow = dtTable.NewRow();
                    string arrCheckResult = "";
                    int nullnum = 0; //判斷是否整列為空

                    bool flowIdValid = repo.ChceckPrStatus(v_prno);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    foreach (MM_PR01_D mm_pr01_d in ori_list)
                    {
                        mm_pr01_d.PR_NO = v_prno;
                        mm_pr01_d.CHECK_RESULT = "OK";

                        //資料是否被使用者填入更新值
                        bool dataUpdated = false;

                        //如果有任何一格不是空的
                        if (string.IsNullOrEmpty(mm_pr01_d.MMCODE) == false || string.IsNullOrEmpty(mm_pr01_d.PR_QTY) == false)
                        {
                            //表示有效資料
                            dataUpdated = true;
                        }
                        else {
                            continue;
                        }

                        //整理資料
                        if (string.IsNullOrEmpty(mm_pr01_d.PR_QTY))
                        {
                            mm_pr01_d.PR_QTY = "0";
                        }
                        if (string.IsNullOrEmpty(mm_pr01_d.MEMO))
                        {
                            mm_pr01_d.MEMO = "";
                        }
                        if (string.IsNullOrEmpty(mm_pr01_d.CHINNAME))
                        {
                            mm_pr01_d.CHINNAME = "";
                        }
                        if (string.IsNullOrEmpty(mm_pr01_d.CHARTNO))
                        {
                            mm_pr01_d.CHARTNO = "";
                        }
                        if (string.IsNullOrEmpty(mm_pr01_d.MEMO))
                        {
                            mm_pr01_d.MEMO = "";
                        }

                        //若有待匯入資料
                        if (mm_pr01_d.MMCODE.ToString() != "" && dataUpdated == true)
                        {
                            if (mm_pr01_d.PR_NO != v_prno)
                            {
                                mm_pr01_d.CHECK_RESULT = "此申購單號與匯入檢核申購單號不同";
                            }
                            else if (repo.CheckExistsPR_NO(mm_pr01_d.PR_NO) != true)
                            {
                                mm_pr01_d.CHECK_RESULT = "申購單號錯誤";
                            }
                            else if (repo.CheckExistsMMCODE(mm_pr01_d.MMCODE, v_prno) != true)
                            {
                                mm_pr01_d.CHECK_RESULT = "院內碼不存在";
                            }
                            else if (repo.CheckFlagMMCODE(mm_pr01_d.MMCODE) != true)
                            {
                                mm_pr01_d.CHECK_RESULT = "院內碼已全院停用";
                            }
                            else if (ori_list.Where(x => x.MMCODE == mm_pr01_d.MMCODE &&
                                                    x.CHINNAME == mm_pr01_d.CHINNAME &&
                                                    x.CHARTNO == mm_pr01_d.CHARTNO &&
                                                    x.MEMO == mm_pr01_d.MEMO).
                                             Select(x => x).Count() > 1)
                            {
                                mm_pr01_d.CHECK_RESULT = "院內碼、病患姓名、病歷號、備註 重複";
                            }
                            else if (repo.CheckMmPrDExists(v_prno, mm_pr01_d.MMCODE,
                                                            (string.IsNullOrEmpty(mm_pr01_d.CHINNAME) ? null : mm_pr01_d.CHINNAME.Trim()),
                                                            (string.IsNullOrEmpty(mm_pr01_d.CHARTNO) ? null : mm_pr01_d.CHARTNO.Trim()),
                                                            (string.IsNullOrEmpty(mm_pr01_d.MEMO) ? null : mm_pr01_d.MEMO.Trim())))
                            {
                                mm_pr01_d.CHECK_RESULT = "院內碼、病患姓名、病歷號、備註 已存在於申購單";
                            }
                            // else if (repo.CheckPrExistsMMCODE(v_prno, mm_pr01_d.MMCODE) == true)
                            // {
                            //     mm_pr01_d.CHECK_RESULT = "此單已有重複院內碼";
                            // }
                            //else if (CheckListDupMMCODE(mm_pr01_d.PR_NO, mm_pr01_d.MMCODE, mm_pr01_d.Seq, ori_list) != true)
                            //{
                            //     mm_pr01_d.CHECK_RESULT = "匯入院內碼重複";
                            // }
                            else if (repo.CheckExistsAGENNO(mm_pr01_d.MMCODE) != true)
                            {
                                mm_pr01_d.CHECK_RESULT = "未設定廠商代碼";
                            }
                            else if (mm_pr01_d.MEMO.Length > 1000)
                            {
                                mm_pr01_d.CHECK_RESULT = "備註不可超過1000字";
                            }
                            else if (mm_pr01_d.CHINNAME.Length > 20)
                            {
                                mm_pr01_d.CHECK_RESULT = "病患姓名不可超過20字";
                            }
                            else if (mm_pr01_d.CHARTNO.Length > 10)
                            {
                                mm_pr01_d.CHECK_RESULT = "病歷號不可超過10字";
                            }
                            else
                            {
                                int unitRate = repo.GetUnitRate(mm_pr01_d.MMCODE);
                                int t;
                                if (int.TryParse(mm_pr01_d.PR_QTY, out t) == false)
                                {
                                    mm_pr01_d.CHECK_RESULT = string.Format("申購量 {0} 需為數字", mm_pr01_d.PR_QTY);
                                }
                                else
                                {
                                    t = int.Parse(mm_pr01_d.PR_QTY);
                                    if (t <= 0)  // 申購單匯入時，數量小於等於0跳過不處理
                                    {
                                        // mm_pr01_d.CHECK_RESULT = string.Format("申購量 {0} 需為整數且大於0", mm_pr01_d.PR_QTY);
                                        continue;
                                    }
                                    //1121009新竹 檢查出貨單位 不卡關改提示(匯入無法)
                                    /*
                                    if (t % unitRate != 0)
                                    {
                                        mm_pr01_d.CHECK_RESULT = string.Format("申購量 {0} 不為出貨單位 {1} 倍數", mm_pr01_d.PR_QTY, unitRate);
                                    }*/
                                }


                                if (mm_pr01_d.CHECK_RESULT == "OK")
                                {
                                    mm_pr01_d.CHECK_RESULT = "通過";
                                };
                            }

                            if (mm_pr01_d.CHECK_RESULT == "OK")
                            {
                                mm_pr01_d.CHECK_RESULT = "通過";
                            };

                            if (mm_pr01_d.CHECK_RESULT != "通過")
                            {
                                checkPassed = false;
                            }
                            //產生一筆資料
                            list.Add(mm_pr01_d);
                        }
                    }

                    bool IsTotalPriceReached = false;
                    if (repo.GetTotalPrice(ori_list) > 150000)
                    {
                        IsTotalPriceReached = true;
                    }

                    session.Result.etts = list;
                    session.Result.msg = checkPassed.ToString() + "," + IsTotalPriceReached.ToString();

                }
                catch(Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }
        public bool CheckListDupMMCODE(string pr_no, string mmcode, int? chkIdx, IEnumerable<MM_PR01_D> dt)
        {
            MM_PR01_D temp = null;
            return dt.Where(x => x.MMCODE == mmcode).Where(x => x.Seq != chkIdx).Select(x => x).Any() == false;
        }

        //匯出EXCEL
        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            string PR_NO = form.Get("p0");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BD0019Repository repo = new BD0019Repository(DBWork);
                    JCLib.Excel.Export("BD0019.xls", repo.GetExcel(PR_NO));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // 確認更新
        [HttpPost]
        public ApiResponse Insert(FormDataCollection formData)
        {
            var v_prno = formData.Get("pr_no");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();

                IEnumerable<MM_PR_D> mm_pr_d = JsonConvert.DeserializeObject<IEnumerable<MM_PR_D>>(formData["data"]);

                List<string> checkDuplicate = new List<string>();

                List<MM_PR_D> mm_pr_d_list = new List<MM_PR_D>();
                try
                {
                    var repo = new BD0019Repository(DBWork);
                    bool isDuplicate = false;

                    foreach (MM_PR_D data in mm_pr_d)
                    {
                        bool flowIdValid = repo.ChceckPrStatus(v_prno);
                        if (flowIdValid == false)
                        {
                            session.Result.msg = "申請單狀態已變更，請重新查詢";
                            session.Result.success = false;
                            return session.Result;
                        }

                        data.PR_NO = v_prno;
                        //if (checkDuplicate.Contains(data.MMCODE)) //檢查list有沒有已經insert過的MMCODE
                        //{
                        //    isDuplicate = true;
                        //    session.Result.msg = isDuplicate.ToString();
                        //    break;
                        //}
                        //else
                        //{
                        checkDuplicate.Add(data.MMCODE);

                        try
                        {
                            data.CREATE_USER = User.Identity.Name;
                            data.UPDATE_IP = DBWork.ProcIP;
                            session.Result.afrs = repo.DetailCreate(data);
                        }
                        catch (Exception e)
                        {
                            throw;
                        }
                        mm_pr_d_list.Add(data);
                        //}
                    }

                    session.Result.etts = mm_pr_d_list;

                    if (isDuplicate == false)
                    {
                        DBWork.Commit();
                    }
                    else
                    {
                        DBWork.Rollback();
                    }
                }
                catch (Exception ex)
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }

        #region 匯入比對欄位設定
        public class HeaderItem
        {
            public string Name { get; set; }
            public string FieldName { get; set; }
            public int Index { get; set; }
            public HeaderItem()
            {
                Name = string.Empty;
                Index = -1;
                FieldName = string.Empty;
            }
            public HeaderItem(string name, int index, string fieldName)
            {
                Name = name;
                Index = index;
                FieldName = fieldName;
            }
            public HeaderItem(string name, string fieldName)
            {
                Name = name;
                Index = -1;
                FieldName = fieldName;
            }
        }

        public List<HeaderItem> SetHeaderIndex(List<HeaderItem> list, IRow headerRow)
        {
            int cellCounts = headerRow.LastCellNum;
            for (int i = 0; i < cellCounts; i++)
            {
                if (headerRow.GetCell(i) == null)
                {
                    continue;
                }

                foreach (HeaderItem item in list)
                {

                    if (headerRow.GetCell(i).ToString() == item.Name)
                    {
                        item.Index = i;
                    }
                }
            }

            return list;
        }
        #endregion

        public string chkPrStatus(string pr_no)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                BD0019Repository repo = new BD0019Repository(DBWork);
                MM_PR_M prData = repo.GetChkPrStatus(pr_no);

                if (prData.PR_STATUS != "35")
                    return pr_no + "狀態已變更，請重新確認";
                else if (prData.ISFROMDOCM == "Y")
                    return pr_no + "為申請轉申購，不可刪除";
                else
                    return "";
            }

        }

        [HttpPost]
        public ApiResponse ChkUnitrate(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0019Repository(DBWork);

                    string[] strSeparating = { "<br>" };
                    string[] strProNoList = form.Get("PR_NO").Split(strSeparating, System.StringSplitOptions.RemoveEmptyEntries);

                    foreach (string strPrNo in strProNoList)
                    {
                        IEnumerable<MM_PR_D> prnoList = repo.GetUnitrateMMCodes(strPrNo);
                        if (prnoList.Count() > 0)
                        {
                            string strMMcodes = string.Join(", ", prnoList.Select(obj => obj.MMCODE));
                            string chkMsg = $"{strPrNo} = {strMMcodes} = 數量不能被出貨單位整除，是否仍要產生訂單 ?";

                            session.Result.msg = chkMsg;
                            session.Result.etts = prnoList;
                            session.Result.success = true;
                            return session.Result;
                        }
                    }
                }
                catch (Exception e)
                {
                    throw;
                }

                session.Result.success = true;
                return session.Result;
            }
        }

    }


}
