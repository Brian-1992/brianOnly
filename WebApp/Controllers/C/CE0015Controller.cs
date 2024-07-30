using JCLib.DB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using WebApp.Models;
using WebApp.Repository.C;

namespace WebApp.Controllers.C
{
    public class CE0015Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse Include(FormDataCollection form)
        {
            var chk_ym = form.Get("P0");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0015Repository(DBWork);

                    IEnumerable<CHK_GRADE2_P> result = repo.GetInclude(chk_ym);

                    foreach (CHK_GRADE2_P item in result)
                    {
                        item.E_TAKEKIND_NAME = GetTakeKindName(item.E_TAKEKIND);
                    }

                    session.Result.etts = result;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var sorters = form.Get("sorters");
            var gap_t = form.Get("gap_t") == null ? 0 : float.Parse(form.Get("gap_t"));                  // 誤差輛
            var gap_price = form.Get("gap_price") == null ? 0 : float.Parse(form.Get("gap_price"));          // 誤差金額
            var gap_p = form.Get("gap_p") == null ? 0 : float.Parse(form.Get("gap_p"));                 // 誤差百分比
            var m_contprice = form.Get("m_contprice") == null ? 0 : float.Parse(form.Get("m_contprice"));     // 進價
            var e_orderdcflag = form.Get("e_orderdcflag") == null ? string.Empty : form.Get("e_orderdcflag");  // 是否停用
            var drug_type = form.Get("drug_type") == null ? string.Empty : form.Get("drug_type");          // 篩選條件
            //var 


            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0015Repository(DBWork);

                    IEnumerable<CHK_GRADE2_P> result = repo.GetAll(m_contprice, e_orderdcflag, drug_type);

                    foreach (CHK_GRADE2_P item in result)
                    {
                        item.E_TAKEKIND_NAME = GetTakeKindName(item.E_TAKEKIND);
                    }

                    session.Result.etts = result;

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse PreviousP(FormDataCollection form)
        {
            string chk_ym_now = form.Get("P0");
            var gap_t = form.Get("gap_t") == null ? 0 : float.Parse(form.Get("gap_t"));                  // 誤差輛
            var gap_price = form.Get("gap_price") == null ? 0 : float.Parse(form.Get("gap_price"));          // 誤差金額
            var gap_p = form.Get("gap_p") == null ? 0 : float.Parse(form.Get("gap_p"));                 // 誤差百分比
            var m_contprice = form.Get("m_contprice") == null ? 0 : float.Parse(form.Get("m_contprice"));     // 進價
            var e_orderdcflag = form.Get("e_orderdcflag") == null ? string.Empty : form.Get("e_orderdcflag");  // 是否停用
            var drug_type = form.Get("drug_type") == null ? string.Empty : form.Get("drug_type");          // 篩選條件
            var wh_no = form.Get("wh_no") == null ? string.Empty : form.Get("wh_no");          // 庫房代碼

            //var chk_ym = GetChkymP(chk_ym_now);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0015Repository(DBWork);

                    IEnumerable<CHK_GRADE2_P> result = repo.GetPreviousP(chk_ym_now, gap_t, gap_price, gap_p, m_contprice, e_orderdcflag, drug_type, wh_no);

                    foreach (CHK_GRADE2_P item in result)
                    {
                        if (wh_no != string.Empty) {
                            item.CHK_QTY = GetChkqty(item);
                        }
                        
                        item.DIFF_P = GetDiffP(item);
                    }

                    session.Result.etts = result;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse ChknoExists(FormDataCollection form)
        {
            var chk_ym = form.Get("P0");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0015Repository(DBWork);

                    session.Result.etts = repo.GetExistChknos(chk_ym);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public string GetChkymP(string now)
        {
            int yyy = int.Parse(now.Substring(0, 3));
            int m = int.Parse(now.Substring(3, 2));
            if (m == 1)
            {
                return string.Format("{0}{1}", (yyy - 1).ToString(), "12");
            }

            string mm = (m - 1) > 9 ? (m - 1).ToString() : (m - 1).ToString().PadLeft(2, '0');

            return string.Format("{0}{1}", yyy.ToString(), mm);
        }
        public string GetDiffP(CHK_GRADE2_P item)
        {
            float chk_qty = float.Parse(item.CHK_QTY);
            float inv_qty = float.Parse(item.STORE_QTYC);
            if (inv_qty == 0)
            {
                return (chk_qty * 100).ToString();
            }
            return Math.Round(((chk_qty - inv_qty) / inv_qty * 100), 5).ToString();
        }


        public string GetTakeKindName(string takekind)
        {
            switch (takekind)
            {
                case "11":
                case "12":
                case "13":
                    return "口服";
                case "00":
                case "21":
                case "31":
                case "41":
                case "51":
                    return "非口服";
                default:
                    return string.Empty;
            }
        }
        public string GetChkqty(CHK_GRADE2_P item)
        {
            switch (item.STATUS_TOT)
            {
                case "1":
                    return item.CHK_QTY1;
                case "2":
                    return item.CHK_QTY2;
                case "3":
                    return item.CHK_QTY3;
                default:
                    return string.Empty;
            }
        }

        [HttpPost]
        public ApiResponse PreviousS(FormDataCollection form)
        {
            string chk_ym_now = form.Get("P0");

            var gap_t = form.Get("gap_t") == null ? 0 : float.Parse(form.Get("gap_t"));                  // 誤差輛
            var gap_price = form.Get("gap_price") == null ? 0 : float.Parse(form.Get("gap_price"));          // 誤差金額
            var gap_p = form.Get("gap_p") == null ? 0 : float.Parse(form.Get("gap_p"));                 // 誤差百分比
            var m_contprice = form.Get("m_contprice") == null ? 0 : float.Parse(form.Get("m_contprice"));     // 進價
            var e_orderdcflag = form.Get("e_orderdcflag") == null ? string.Empty : form.Get("e_orderdcflag");  // 是否停用
            var drug_type = form.Get("drug_type") == null ? string.Empty : form.Get("drug_type");          // 篩選條件
            var wh_no = form.Get("wh_no") == null ? string.Empty : form.Get("wh_no");          // 庫房代碼

            //var chk_ym = GetChkymS(chk_ym_now);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0015Repository(DBWork);

                    IEnumerable<CHK_GRADE2_P> result = repo.GetPreviousS(chk_ym_now, gap_t, gap_price, gap_p, m_contprice, e_orderdcflag, drug_type, wh_no);

                    foreach (CHK_GRADE2_P item in result)
                    {
                        item.CHK_QTY = GetChkqty(item);
                    }

                    session.Result.etts = result;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public string GetChkymS(string now)
        {
            int yyy = int.Parse(now.Substring(0, 3));
            int m = int.Parse(now.Substring(3, 2));

            if (m >= 1 && m <= 3)
            {
                return string.Format("{0}{1}", (yyy - 1).ToString(), "12");
            }
            else if (m >= 4 && m <= 6)
            {
                return string.Format("{0}{1}", yyy.ToString(), "03");
            }
            else if (m >= 7 && m <= 9)
            {
                return string.Format("{0}{1}", yyy.ToString(), "06");
            }
            else
            {            //  if (m >= 10 && m <= 12)
                return string.Format("{0}{1}", yyy.ToString(), "09");
            }

        }

        [HttpPost]
        public ApiResponse Insert(FormDataCollection form)
        {
            var itemString = form.Get("ITEM_STRING");

            IEnumerable<CHK_GRADE2_P> grade2ps = JsonConvert.DeserializeObject<IEnumerable<CHK_GRADE2_P>>(itemString);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0015Repository(DBWork);

                    foreach (CHK_GRADE2_P item in grade2ps)
                    {

                        if (repo.Exists(item))
                        {
                            continue;
                        }

                        item.CREATE_USER = DBWork.UserInfo.UserId;
                        item.UPDATE_USER = DBWork.UserInfo.UserId;
                        item.UPDATE_IP = DBWork.ProcIP;

                        session.Result.afrs += repo.Insert(item);
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
        public ApiResponse Delete(FormDataCollection form)
        {
            var itemString = form.Get("ITEM_STRING");

            IEnumerable<CHK_GRADE2_P> grade2ps = JsonConvert.DeserializeObject<IEnumerable<CHK_GRADE2_P>>(itemString);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0015Repository(DBWork);

                    foreach (CHK_GRADE2_P item in grade2ps)
                    {
                        session.Result.afrs += repo.Delete(item);
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
        public ApiResponse CreateSMats(FormDataCollection form) {
            string chk_ym = form.Get("chk_ym");
            string chk_pre_date = form.Get("chk_pre_date");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0015Repository(DBWork);
                    var repoCE0002 = new CE0002Repository(DBWork);

                    IEnumerable<CHK_MAST> masts = repo.GetChkMasts(chk_ym, "S");
                    foreach (CHK_MAST mast in masts) {
                        mast.CHK_NO = GetChkNo(mast);
                        mast.CHK_NO1 = mast.CHK_NO;
                        mast.UPDATE_USER = DBWork.UserInfo.UserId;
                        mast.UPDATE_IP = DBWork.ProcIP;

                        session.Result.afrs = repoCE0002.InsertChkMast(mast);

                        IEnumerable<CHK_DETAIL> items = null;
                        items = repo.GetMedItems(mast.CHK_WH_NO);
                        int i = 0;
                        foreach (CHK_DETAIL item in items)
                        {
                            i++;
                            CHK_G2_WHINV whinv = new CHK_G2_WHINV()
                            {
                                CHK_NO = mast.CHK_NO,
                                WH_NO = mast.CHK_WH_NO,
                                MMCODE = item.MMCODE,
                                CREATE_USER = DBWork.UserInfo.UserId,
                                UPDATE_USER = DBWork.UserInfo.UserId,
                                UPDATE_IP = DBWork.ProcIP,
                                SEQ = i.ToString(),
                                CHK_PRE_DATE = chk_pre_date
                            };
                            repo.InsertChkG2Whinv(whinv);
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
        public string GetChkNo(CHK_MAST master)
        {
            string period = master.CHK_PERIOD == "D" ? "D" : "A";

            string chk_ym = period != "D" ? master.CHK_YM : master.CHK_YMD.Substring(0, 5);

            string currentSeq = GetCurrentSeq(master.CHK_WH_NO, chk_ym);
            return string.Format("{0}{1}{2}{3}{4}", master.CHK_WH_NO, master.CHK_YM, period, master.CHK_TYPE, currentSeq);
        }
        public string GetCurrentSeq(string wh_no, string ym)
        {
            string maxNo = string.Empty;
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0002Repository(DBWork);
                    maxNo = repo.GetCurrentSeq(wh_no, ym);
                }
                catch
                {
                    throw;
                }
                return maxNo;
            }
        }
        [HttpPost]
        public ApiResponse ChknoSExists(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0015Repository(DBWork);

                    session.Result.etts = repo.GetExistChknosS();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse CreatePMats(FormDataCollection form) {
            string chk_ym = form.Get("chk_ym");
            string chk_pre_date = form.Get("chk_pre_date");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0015Repository(DBWork);
                    var repoCE0002 = new CE0002Repository(DBWork);

                    IEnumerable<CHK_MAST> masts = repo.GetChkMasts(chk_ym, "P");
                    foreach (CHK_MAST mast in masts)
                    {
                        mast.CHK_NO = GetChkNo(mast);
                        mast.CHK_NO1 = mast.CHK_NO;
                        mast.UPDATE_USER = DBWork.UserInfo.UserId;
                        mast.UPDATE_IP = DBWork.ProcIP;

                        session.Result.afrs = repoCE0002.InsertChkMast(mast);

                        IEnumerable<CHK_DETAIL> items = null;
                        items = repo.GetChkGrade2Ps(mast.CHK_WH_NO, chk_ym);
                        int i = 0;
                        foreach (CHK_DETAIL item in items)
                        {
                            i++;
                            CHK_G2_WHINV whinv = new CHK_G2_WHINV()
                            {
                                CHK_NO = mast.CHK_NO,
                                WH_NO = mast.CHK_WH_NO,
                                MMCODE = item.MMCODE,
                                CREATE_USER = DBWork.UserInfo.UserId,
                                UPDATE_USER = DBWork.UserInfo.UserId,
                                UPDATE_IP = DBWork.ProcIP,
                                SEQ = i.ToString(),
                                CHK_PRE_DATE = chk_pre_date
                            };
                            repo.InsertChkG2Whinv(whinv);
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

        #region combo
        public ApiResponse GetWhnos() {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0015Repository(DBWork);
                    session.Result.etts = repo.GetWhnos();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        #endregion

        #region 2021-12-12
        [HttpGet]
        public ApiResponse GetSetYm() {
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0015Repository(DBWork);
                    session.Result.etts = repo.GetMnSet();
                }
                catch (Exception e) {
                    throw;
                }
                return session.Result;
            }
        }
        #endregion
    }
}