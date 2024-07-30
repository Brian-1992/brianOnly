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
using static WebApp.Repository.C.CE0020Repository;

namespace WebApp.Controllers.C
{
    public class CE0023Controller : SiteBase.BaseApiController
    {
        public ApiResponse MasterAll(FormDataCollection form)
        {

            var wh_no = form.Get("p0");
            var chk_ym = form.Get("p1");
            var keeper = form.Get("p2");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0023Repository(DBWork);
                    IEnumerable<CHK_MAST> masters = repo.GetMasterAll(wh_no, chk_ym, keeper);
                    session.Result.etts = masters;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse ModifiedSheet(FormDataCollection form)
        {
            var CHK_NO = form.Get("CHK_NO");
            var CHK_NO1 = form.Get("CHK_NO1");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0023Repository(DBWork);
                    var repoCE0024 = new CE0024Repository(DBWork);
                    var repoCE0004 = new CE0004Repository(DBWork);

                    CHK_MAST mast = repoCE0004.GetChkMast(CHK_NO);
                    string start_date = repoCE0024.GetStartDate(mast.CHK_YM);
                    string end_date = repoCE0024.GetEndDate(mast.CHK_YM);

                    var UserName = DBWork.UserInfo.UserId;
                    var UserIP = DBWork.ProcIP;
                    session.Result.afrs = repo.Update_CHK_G2_DETAIL_Status(CHK_NO);

                    IEnumerable<CHK_G2_WHINV> whinvs = repo.GetWhinvs(CHK_NO);
                    foreach(CHK_G2_WHINV whinv in whinvs) {
                        string store_qty = repo.GetStoreQty(whinv.CHK_NO, whinv.MMCODE);
                        string chk_qty2 = repo.GetChkqtySum(whinv.CHK_NO, whinv.MMCODE);
                        string apl_outqty = repo.GetAploutqty(start_date, end_date, whinv.WH_NO, whinv.MMCODE);

                        session.Result.afrs = repo.Update_CHK_G2_DETAILTOT(store_qty, chk_qty2, CHK_NO1, whinv.MMCODE, UserName, UserIP, CHK_NO, apl_outqty);
                    }

                    session.Result.afrs = repo.Update_CHK_MAST_Status(CHK_NO);

                    DBWork.Commit();
                }
                catch
                {
                    DBWork.Rollback();
                    session.Result.success = false;
                    throw;
                }
                return session.Result;
            }
        }



        #region 盤點明細
        [HttpPost]
        public ApiResponse GetDetailAll(FormDataCollection form)
        {
            var chk_no = form.Get("chk_no");
            FilterItem filter = new FilterItem()
            {
                gap_t = form.Get("gap_t") == null ? 0 : float.Parse(form.Get("gap_t")),                         // 誤差輛
                gap_price = form.Get("gap_price") == null ? 0 : float.Parse(form.Get("gap_price")),             // 誤差金額
                gap_p = form.Get("gap_p") == null ? 0 : float.Parse(form.Get("gap_p")),                         // 誤差百分比
                m_contprice = form.Get("m_contprice") == null ? 0 : float.Parse(form.Get("m_contprice")),       // 進價
                e_orderdcflag = form.Get("e_orderdcflag") == null ? string.Empty : form.Get("e_orderdcflag"),   // 是否停用
                drug_type = form.Get("drug_type") == null ? string.Empty : form.Get("drug_type"),               // 篩選條件
                seq1 = form.Get("seq1") == null ? string.Empty : form.Get("seq1"),
                seq2 = form.Get("seq2") == null ? string.Empty : form.Get("seq2"),
                mmcode1 = form.Get("mmcode1") == null ? string.Empty : form.Get("mmcode1"),
                mmcode2 = form.Get("mmcode2") == null ? string.Empty : form.Get("mmcode2"),
                mmname = form.Get("mmname") == null ? string.Empty : form.Get("mmname"),
            };

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0023Repository(DBWork);

                    IEnumerable<CHK_DETAIL> includes = repo.GetDetailAll(chk_no, filter);

                    session.Result.etts = includes;

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }




        [HttpPost]
        public ApiResponse GetUserDetails(FormDataCollection form)
        {
            // GetExcludeDetails
            var mmcode = form.Get("mmcode");
            var chk_no = form.Get("chk_no");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0023Repository(DBWork);
                    IEnumerable<CHK_DETAIL> excludes = repo.GetUserDetails(chk_no, mmcode);

                    session.Result.etts = excludes;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse SaveEdit(FormDataCollection form)
        {
            string item_string = form.Get("item_string");

            IEnumerable<CHK_DETAIL> list = JsonConvert.DeserializeObject<IEnumerable<CHK_DETAIL>>(item_string);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0023Repository(DBWork);
                    var UpdateUser = DBWork.UserInfo.UserId;
                    var UpdateIP = DBWork.ProcIP;

                    foreach (CHK_DETAIL item in list)
                    {
                        session.Result.afrs = repo.SaveEdit(item.CHK_QTY, item.CHK_NO, item.MMCODE, item.CHK_UID, DBWork.UserInfo.UserId, DBWork.ProcIP);
                    }
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        #endregion


        #region combo
        public ApiResponse GetWhnoCombo()
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0023Repository(DBWork);
                    session.Result.etts = repo.GetWhnoCombo(User.Identity.Name);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        #endregion

        #region 2020-07-15 新增: 備註欄位(CHK_G2_WHINV.MEMO)
        public ApiResponse UpdateMemo(FormDataCollection form)
        {
            string item_string = form.Get("item_string");
            IEnumerable<CHK_G2_WHINV> list = JsonConvert.DeserializeObject<IEnumerable<CHK_G2_WHINV>>(item_string);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0023Repository(DBWork);

                    foreach (CHK_G2_WHINV item in list)
                    {
                        item.UPDATE_USER = DBWork.UserInfo.UserId;
                        item.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.UpdateMemo(item);
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
        #endregion

    }
}