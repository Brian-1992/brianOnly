using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using WebApp.Models;
using WebApp.Repository.AA;

namespace WebApp.Controllers.AA
{
    public class AA0135Controller : SiteBase.BaseApiController
    {
        [HttpGet]
        public ApiResponse Done() {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0135Repository(DBWork);
                    session.Result.etts = repo.GetDone();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpGet]
        public ApiResponse Current() {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0135Repository(DBWork);
                    IEnumerable<CHK_MNSET> current = repo.GetCurrent();
                    session.Result.etts = current;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Update(FormDataCollection form) {
            string chk_ym = form.Get("chk_ym");
            string set_ctime = form.Get("set_ctime");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0135Repository(DBWork);
                    CHK_MNSET mnset = new CHK_MNSET();
                    mnset.CHK_YM = chk_ym;
                    mnset.SET_CTIME = set_ctime;
                    mnset.UPDATE_USER = DBWork.UserInfo.UserId;
                    mnset.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.UpdateChkMnset(mnset);

                    DBWork.Commit();

                    session.Result.etts = repo.GetCurrent();
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }

        #region 2020-05-27 新增: 補開科室病房月盤單
        [HttpPost]
        public ApiResponse CreateChkMasts(FormDataCollection form) {
            string wh_no = form.Get("wh_no");
            string chkym = string.Empty;
            string preym = string.Empty;
            string postym = string.Empty;
            List<CHK_MAST> masts = new List<CHK_MAST>();

            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0135Repository(DBWork);

                    CHK_MNSET chkmnset = repo.GetChkmnset();
                    chkym = chkmnset.CHK_YM;
                    preym = chkmnset.PRE_YM;
                    postym = chkmnset.POST_YM;

                    if (repo.CheckMiWhmast(wh_no) == false) {
                        session.Result.success = false;
                        session.Result.msg = string.Format("{0}不為衛星庫房衛材庫，請重新輸入", wh_no);
                        return session.Result;
                    }

                    DBWork.BeginTransaction();

                    // 0:庫備 1:非庫備
                    IEnumerable<string> storeids = new List<string>() { "0", "1" };
                    foreach (string storeid in storeids) {
                        CHK_MAST temp = new CHK_MAST();
                        IEnumerable<ChkWh> whnos = repo.GetWhnos(storeid, preym, wh_no);
                        if (whnos.Any() == false) {
                            temp.CHK_TYPE = storeid;
                            temp.ITEM_STRING = "無符合品項，不開單";
                            masts.Add(temp);
                            continue;
                        }
                        foreach (ChkWh wh in whnos) {
                            if (repo.CheckExists(wh.WH_NO, chkym, storeid, "02", "M"))
                            {
                                temp.CHK_TYPE = storeid;
                                temp.ITEM_STRING = "已由排程開單完成";
                                masts.Add(temp);
                                continue;
                            }
                            CHK_MAST mast = GetChkMast(wh, storeid, chkym);

                            string currentSeq = repo.GetCurrentSeq(wh_no, chkym);
                            mast.CHK_NO = string.Format("{0}{1}{2}{3}{4}", wh_no, chkym, "A", storeid, currentSeq);
                            mast.CHK_NO1 = mast.CHK_NO;

                            // 新增master
                            session.Result.afrs = repo.InsertMaster(mast);
                            // 新增detail
                            session.Result.afrs = repo.InsertDetail(mast, preym);
                            // 更新master總數量
                            session.Result.afrs = repo.UpdateMaster(mast);
                            // 新增可盤點人員檔
                            session.Result.afrs = repo.InsertNouid(mast);

                            temp.CHK_TYPE = storeid;
                            temp.ITEM_STRING = string.Format("開單完成，盤點單號：{0}",mast.CHK_NO);
                            masts.Add(temp);
                        }
                    }

                    // 3:小額採購
                    IEnumerable<ChkWh> whs = repo.GetWhno3s(preym, wh_no);
                    if (whs.Any() == false)
                    {
                        CHK_MAST temp = new CHK_MAST();
                        temp.CHK_TYPE = "3";
                        temp.ITEM_STRING = "無符合品項，不開單";
                        masts.Add(temp);
                    }
                    else
                    {
                        foreach (ChkWh wh in whs)
                        {
                            if (repo.CheckExists(wh.WH_NO, chkym, "3", "02", "M"))
                            {
                                CHK_MAST temp = new CHK_MAST();
                                temp.CHK_TYPE = "3";
                                temp.ITEM_STRING = "已由排程開單完成";
                                masts.Add(temp);
                                continue;
                            }
                            CHK_MAST mast = GetChkMast(wh, "3", chkym);
                            string currentSeq = repo.GetCurrentSeq(wh_no, chkym);
                            mast.CHK_NO = string.Format("{0}{1}{2}{3}{4}", wh_no, chkym, "A", "3", currentSeq);
                            mast.CHK_NO1 = mast.CHK_NO;

                            // 新增master
                            session.Result.afrs = repo.InsertMaster(mast);
                            // 新增detail
                            session.Result.afrs = repo.InsertDetail(mast, preym);
                            // 更新master總數量
                            session.Result.afrs = repo.UpdateMaster(mast);
                            // 新增可盤點人員檔
                            session.Result.afrs = repo.InsertNouid(mast);
                        }
                    }
                    DBWork.Commit();
                    session.Result.etts = masts;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public CHK_MAST GetChkMast(ChkWh chkwh, string storeid, string chkym)
        {
            return new CHK_MAST()
            {
                CHK_WH_NO = chkwh.WH_NO,
                CHK_YM = chkym,
                CHK_WH_GRADE = chkwh.WH_GRADE,
                CHK_WH_KIND = "1",
                CHK_CLASS = "02",
                CHK_PERIOD = "M",
                CHK_TYPE = storeid,
                CHK_LEVEL = "1",
                CHK_NUM = "0",
                CHK_TOTAL = chkwh.CHK_TOTAL.ToString(),
                CHK_STATUS = "1",
                CHK_KEEPER = chkwh.INID,
                CHK_NO1 = string.Empty,
                CREATE_USER = "BATCH",
                UPDATE_USER = "",
                UPDATE_IP = string.Empty
            };
        }
        #endregion
    }
}