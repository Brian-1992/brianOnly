using JCLib.DB;
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
    public class CE0033Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse GetUserChkList(FormDataCollection form) {
            string chk_no = form.Get("chk_no");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0033Repository(DBWork);
                    session.Result.etts = repo.GetUserChkList(chk_no, DBWork.UserInfo.UserId);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetItemDetail(FormDataCollection form) {
            string chk_no = form.Get("chk_no");
            string store_loc = form.Get("store_loc");
            string mmcode = form.Get("mmcode");
            string barcode = form.Get("barcode");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0033Repository(DBWork);
                    IEnumerable<CHK_DETAIL> items = repo.GetItemDetail(chk_no, store_loc, barcode, mmcode, DBWork.UserInfo.UserId);

                    if (items.Any() == false) {
                        session.Result.msg = "無符合之資料，請檢查儲位與物料號碼是否正確";
                    }
                    session.Result.etts = items;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }

        }

        public ApiResponse SetChkqty(FormDataCollection form) {
            string chk_no = form.Get("chk_no");
            string store_loc = form.Get("store_loc");
            string mmcode = form.Get("mmcode");
            string chk_qty = form.Get("chk_qty");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0033Repository(DBWork);
                    CHK_MAST mast = repo.GetChkMast(chk_no);
                    if (mast.CHK_STATUS != "1") {
                        session.Result.success = false;
                        session.Result.msg = "盤點單狀態已變更，請重新查詢";
                        return session.Result;
                    }

                    session.Result.afrs = repo.updateChkqty(chk_no, store_loc, mmcode, DBWork.UserInfo.UserId, chk_qty, DBWork.ProcIP);

                    DBWork.Commit();
                }
                catch
                {
                    session.Result.success = false;
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }

        #region combos
        [HttpPost]
        public ApiResponse GetChknoCombo(FormDataCollection form) {
            string chk_level = form.Get("chk_level");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0033Repository(DBWork);
                    session.Result.etts = repo.GetChknoCombo(chk_level, DBWork.UserInfo.UserId); 
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        #endregion
    }
}