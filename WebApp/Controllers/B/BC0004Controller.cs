using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.BC;
using WebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApp.Controllers.BC
{
    public class BC0004Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            var p0 = form.Get("p0");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BC0004Repository repo = new BC0004Repository(DBWork);
                    JCLib.Excel.Export(form.Get("FN"), repo.GetExcel(p0));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse MasterUpdate(PH_SMALL_M ph_small_m)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BC0004Repository(DBWork);
                    ph_small_m.DO_USER = User.Identity.Name;
                    ph_small_m.DEPT = DBWork.UserInfo.Inid;
                    ph_small_m.UPDATE_USER = User.Identity.Name;
                    ph_small_m.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.MasterUpdate(ph_small_m);
                    var repo1 = new BC0002Repository(DBWork);
                    session.Result.etts = repo1.MasterGet(ph_small_m.DN);

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
        public ApiResponse MasterReject(PH_SMALL_M ph_small_m)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    ph_small_m.DO_USER = User.Identity.Name;
                    ph_small_m.DEPT = DBWork.UserInfo.Inid;
                    ph_small_m.UPDATE_USER = User.Identity.Name;
                    ph_small_m.UPDATE_IP = DBWork.ProcIP;
                    var repo1 = new BC0004Repository(DBWork);
                    session.Result.afrs = repo1.MasterReject(ph_small_m);
                    var repo2 = new BC0002Repository(DBWork);
                    session.Result.etts = repo2.MasterGet(ph_small_m.DN);
                    string email = repo2.getUser_MAIL_ADDRESS(ph_small_m.DN);
                    if (email != null && email != "")
                    {
                        BC0004Controller BC4 = new BC0004Controller();
                        BC4.sendRejectMail(ph_small_m, "", email);
                    }
                    //MailController mail = new MailController();
                    //string cont = "小額採購通知-消審會剔退<br>";
                    //cont += "單號 : " + ph_small_m.DN + "<br>";
                    //cont += "剔退時間 : " + (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.ToString("MM/dd HH:mm:ss") + "<br>";
                    //mail.Send_Mail("小額採購通知", cont, repo2.getUser_MAIL_ADDRESS(ph_small_m.APP_USER));
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
        public void sendRejectMail(PH_SMALL_M ph_small_m, string doman, string email)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                var repo = new BC0002Repository(DBWork);
                MailController mail = new MailController();
                string cont = "";
                if (doman == "") cont = "小額採購通知-消審會剔退<br>";
                else
                    cont = "小額採購通知-審查剔退<br>";
                cont += "單號 : " + ph_small_m.DN + "<br>";
                cont += "剔退時間 : " + (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.ToString("MM/dd HH:mm:ss") + "<br>";
                if (doman != "") cont += "審核人: " + doman + "<br>";
                mail.Send_Mail("小額採購通知", cont, email);
            }
        }
        [HttpPost]
        public ApiResponse MasterApprove(PH_SMALL_M ph_small_m)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BC0004Repository(DBWork);
                    ////檢核條件1 :基本檔是否完整[m_contprice],[m_purun], MI_UNITEXCH[exch_ratio]n
                    //session.Result.etts = repo.CheckAndCreatOrder_Step1(ph_small_m.DN);
                    //if (session.Result.rc > 1)
                    //{
                    //    session.Result.msg = "申購品項超過1家廠商,請分別開單<br>";
                    //}
                    //else
                    //{
                        //檢核條件2
                        if (!repo.CheckAndCreatOrder_Step2(ph_small_m.DN))
                        {
                            session.Result.msg += "申購品項未建立院內碼或品項重覆,請先建立院內碼或調整品項<br>";
                        }
                        else
                        {
                        //檢核條件3
                        //if (!repo.CheckAndCreatOrder_Step3(ph_small_m.DN))
                        //{
                        //    session.Result.msg += "廠商資料不存在,請先建立廠商資料<br>";
                        //}
                        //else
                        //{

                        // 2023-09-07 檢查最小撥補量
                        IEnumerable<PH_SMALL_D> small_ds = repo.CheckMinOrdqty(ph_small_m.DN);
                        List<PH_SMALL_D> minordqty_mmcodes = new List<PH_SMALL_D>();
                        foreach (PH_SMALL_D small_d in small_ds)
                        {
                            int i;
                            int min_ordqty = int.TryParse(small_d.SEQ, out i) ? int.Parse(small_d.SEQ) : 1;
                            if (small_d.QTY % min_ordqty != 0)
                            {
                                minordqty_mmcodes.Add((small_d));
                            }
                        }
                        if (minordqty_mmcodes.Any())
                        {
                            string temp = "下列院內碼不為最小撥補量倍數，請重新確認：<br>";
                            foreach (PH_SMALL_D mmcode in minordqty_mmcodes)
                            {
                                temp += string.Format(@"院內碼：{0} 申請數量：{1} 最小撥補量：{2}<br>", mmcode.MMCODE, mmcode.QTY, mmcode.SEQ);
                            }

                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = temp;
                            return session.Result;
                        }

                        //2022-03-18 檢查detail是否為非庫備
                        List<string> storeid1_mmcodes = repo.CheckDetailAll0(ph_small_m.DN).ToList();
                                if (storeid1_mmcodes.Any()) {
                                    string temp = "下列院內碼為庫備品項，請重新確認：<br>";
                                    foreach (string mmcode in storeid1_mmcodes) {
                                        temp += string.Format("{0}<br>", mmcode);
                                    }
                              
                                    session.Result.msg = temp;
                                    session.Result.success = false;
                                    return session.Result;
                                }
                                //檢核條件4
                                if (string.IsNullOrEmpty(session.Result.msg))
                                {
                                    ph_small_m.DO_USER = User.Identity.Name;
                                    ph_small_m.DEPT = DBWork.UserInfo.Inid;
                                    ph_small_m.UPDATE_USER = User.Identity.Name;
                                    ph_small_m.UPDATE_IP = DBWork.ProcIP;

                                    session.Result.afrs = repo.MasterApprove(ph_small_m);
                                    var repo2 = new BC0002Repository(DBWork);
                                    session.Result.etts = repo2.MasterGet(ph_small_m.DN);
                                }
                            //}
                        }
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

        /// <summary>
        /// 讀取庫房別ComboList
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ApiResponse GetAgennoCombo()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BC0004Repository repo = new BC0004Repository(DBWork);
                    session.Result.etts = repo.GetAgennoCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        /// <summary>
        /// 產生訂單
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ApiResponse CheckAndCreatOrder(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p4 = form.Get("p4");
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BC0004Repository repo = new BC0004Repository(DBWork);

                    //2022-03-18 檢查detail是否為非庫備
                    List<string> storeid1_mmcodes = repo.CheckDetailAll0(p0).ToList();
                    if (storeid1_mmcodes.Any())
                    {
                        string temp = "下列院內碼為庫備品項，請重新確認：<br>";
                        foreach (string mmcode in storeid1_mmcodes)
                        {
                            temp += string.Format("{0}<br>", mmcode);
                        }

                        session.Result.msg = temp;
                        session.Result.success = false;
                        return session.Result;
                    }


                    var INID = DBWork.UserInfo.Inid;
                    var WHNO = p4;
                    var USER_ID = DBWork.UserInfo.UserId;
                    var USER_IP = DBWork.ProcIP;

                    var RetCode = repo.CheckAndCreatOrder_Step4(p0, INID, WHNO, USER_ID, USER_IP);
                    var repo2 = new BC0002Repository(DBWork);
                    session.Result.etts = repo2.MasterGet(p0);
                    if (RetCode == "ERR")
                    {
                        session.Result.msg += "產生訂單失敗<br>";
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