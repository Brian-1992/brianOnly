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
    public class CE0021Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse MasterAll(FormDataCollection form)
        {

            var wh_no = form.Get("p0");
            var chk_ym = form.Get("p1");
            var keeper = form.Get("p2");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0021Repository(DBWork);
                    string chk_status = "";
                    string chk_level = "2";
                    IEnumerable<CHK_MAST> masters = repo.GetChkMasts(wh_no, chk_ym, keeper, chk_status, chk_level, page, limit, sorters);
                    foreach (CHK_MAST master in masters)
                    {
                        master.CHK_TYPE_NAME = repo.GetChkWhkindName(master.CHK_WH_KIND, master.CHK_TYPE);
                    }
                    session.Result.etts = masters;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        #region 新增 master

        [HttpPost]
        public ApiResponse GetMasterInsertList(FormDataCollection form)
        {
            var wh_no = form.Get("wh_no");
            var chk_ym = form.Get("chk_ym");
            var keeper = form.Get("keeper");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;

                try
                {
                    var repo = new CE0021Repository(DBWork);
                    IEnumerable<CHK_MAST> masters = repo.GetMasterInsertList(wh_no, chk_ym, keeper, page, limit, sorters);
                    foreach (CHK_MAST master in masters)
                    {
                        master.CHK_TYPE_NAME = repo.GetChkWhkindName(master.CHK_WH_KIND, master.CHK_TYPE);
                    }
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
        public ApiResponse InsertMaster(CHK_MAST masterInput)
        {
            IEnumerable<CHK_MAST> masters = JsonConvert.DeserializeObject<IEnumerable<CHK_MAST>>(masterInput.ITEM_STRING);

            //CHK_MAST master = new CHK_MAST();
            //master.CHK_WH_NO = form.Get("CHK_CH_NO");
            //master.CHK_YM = form.Get("CHK_YM");
            //master.CHK_WH_GRADE = form.Get("CHK_WH_GRADE");
            //master.CHK_WH_KIND = form.Get("CHK_WH_KIND");
            //master.CHK_PERIOD = form.Get("CHK_PERIOD");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0005Repository(DBWork);
                    foreach (CHK_MAST master in masters)
                    {
                        if (master.CHK_PERIOD == "D")
                        {
                            master.CHK_YMD = master.CHK_YM;
                            master.CHK_YM = master.CHK_YM.Substring(0, 5);
                        }

                        master.CHK_NO1 = master.CHK_NO;

                        string period = master.CHK_PERIOD == "D" ? "D" : "B";
                        string currentSeq = repo.GetCurrentSeq(master.CHK_WH_NO, master.CHK_YM);
                        master.CHK_NO = string.Format("{0}{1}{2}{3}{4}", master.CHK_WH_NO, master.CHK_YM, period, master.CHK_TYPE, currentSeq);

                        if (master.CHK_PERIOD == "D")
                        {
                            master.CHK_YM = master.CHK_YMD;
                        }
                        
                        //master.CHK_KEEPER = DBWork.UserInfo.UserId;
                        master.CREATE_USER = DBWork.UserInfo.UserId;
                        master.UPDATE_USER = DBWork.UserInfo.UserId;
                        master.UPDATE_IP = DBWork.ProcIP;

                        //session.Result.afrs = repo.InsertChkMast(master);

                        if (!repo.CheckExists(master.CHK_NO1)) // 新增前檢查是否已存在複盤
                        {
                            session.Result.afrs = repo.InsertChkMast(master);
                        }
                        else
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = string.Format("已存在複盤資料，請重新選取。");
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

        #endregion

        #region 刪除 maaster
        [HttpPost]
        public ApiResponse DeleteMaster(CHK_MAST masterInput)
        {
            IEnumerable<CHK_MAST> masters = JsonConvert.DeserializeObject<IEnumerable<CHK_MAST>>(masterInput.ITEM_STRING);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0002Repository(DBWork);

                    foreach (CHK_MAST master in masters)
                    {
                        // 先刪detail
                        session.Result.afrs = repo.DeleteChkDetail(master.CHK_NO);
                        // 後刪master
                        session.Result.afrs = repo.DeleteChkMast(master.CHK_NO);
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

        #region 盤點明細主畫面
        [HttpPost]
        public ApiResponse GetDetailInclude(FormDataCollection form) {
            var chk_no = form.Get("chk_no");
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0021Repository(DBWork);

                    session.Result.etts = repo.GetIncludeDetails(chk_no);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetDetailExclude(FormDataCollection form) {
            var chk_no = form.Get("chk_no");
            var chk_no1 = form.Get("chk_no1");
            var list = form.Get("list");
            var f_u_price = form.Get("F_U_PRICE") == null ? 0 : float.Parse(form.Get("F_U_PRICE"));
            var f_number = form.Get("F_NUMBER") == null ? 0 : float.Parse(form.Get("F_NUMBER"));
            var f_amount = form.Get("F_AMOUNT") == null ? 0 : float.Parse(form.Get("F_AMOUNT"));
            var miss_per = form.Get("MISS_PER") == null ? 0 : float.Parse(form.Get("MISS_PER"));

            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0021Repository(DBWork);

                    session.Result.etts = repo.GetExcludeDetails(chk_no, chk_no1, f_u_price, f_number, f_amount, miss_per);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse AddG2Whinv(FormDataCollection form)
        {
            var item_string = form.Get("list");

            IEnumerable<CHK_G2_DETAILTOT> tots = JsonConvert.DeserializeObject<IEnumerable<CHK_G2_DETAILTOT>>(item_string);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0021Repository(DBWork);

                    foreach (CHK_G2_DETAILTOT tot in tots)
                    {
                        CHK_G2_WHINV whinv = new CHK_G2_WHINV()
                        {
                            CHK_NO = tot.CHK_NO,
                            WH_NO = tot.WH_NO,
                            MMCODE = tot.MMCODE,
                            UPDATE_USER = DBWork.UserInfo.UserId,
                            CREATE_USER = DBWork.UserInfo.UserId,
                            UPDATE_IP = DBWork.ProcIP,
                            SEQ = repo.GetMedSeq(tot.CHK_NO, tot.MMCODE)
                        };

                        session.Result.afrs = repo.InsertG2Whinv(whinv);
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
        public ApiResponse DeleteG2Whinv(FormDataCollection form)
        {
            var item_string = form.Get("item_string");

            IEnumerable<CHK_G2_WHINV> whinvs = JsonConvert.DeserializeObject<IEnumerable<CHK_G2_WHINV>>(item_string);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0021Repository(DBWork);

                    foreach (CHK_G2_WHINV whinv in whinvs)
                    {
                        session.Result.afrs = repo.DeleteG2Whinv(whinv.CHK_NO, whinv.MMCODE);
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
        public ApiResponse CreateSheet(FormDataCollection form)
        {
            var chk_no = form.Get("chk_no");
            var chk_ym = form.Get("chk_ym");
            var usersString = form.Get("users");

            IEnumerable<BC_WHCHKID> users = JsonConvert.DeserializeObject<IEnumerable<BC_WHCHKID>>(usersString);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0002Repository(DBWork);

                    if (repo.CheckMedChkPreDatesNull(chk_no))
                    {
                        session.Result.success = false;
                        session.Result.msg = "尚有未設定預計盤點日之品項，無法開立盤點單";
                        return session.Result;
                    }

                    foreach (BC_WHCHKID user in users)
                    {
                        CHK_GRADE2_UPDN item = new CHK_GRADE2_UPDN()
                        {
                            CHK_NO = chk_no,
                            CHK_YM = chk_ym,
                            CHK_UID = user.WH_CHKUID,
                            UP_DATE = string.Empty,
                            DN_DATE = string.Empty,
                            UPDN_STATUS = "1",
                            CREATE_USER = DBWork.UserInfo.UserId,
                            UPDATE_USER = DBWork.UserInfo.UserId,
                            UPDATE_IP = DBWork.ProcIP
                        };
                        session.Result.afrs = repo.InsertGrade2UpDn(item);
                    }

                    IEnumerable<CHK_G2_WHINV> whinvs = repo.GetAllChkG2Whinvs(chk_no);
                    IEnumerable<CHK_G2_DETAIL> details = GetG2Details(whinvs, users);

                    foreach (CHK_G2_DETAIL detail in details)
                    {
                        detail.STATUS_INI = "1";
                        detail.CREATE_USER = DBWork.UserInfo.UserId;
                        detail.UPDATE_USER = DBWork.UserInfo.UserId;
                        detail.UPDATE_IP = DBWork.ProcIP;

                        session.Result.afrs = repo.InsertG2Detail(detail);
                    }


                    session.Result.afrs = repo.MedUpdateMaster(chk_no, users.Count(), DBWork.UserInfo.UserId, DBWork.ProcIP);

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
        public IEnumerable<CHK_G2_DETAIL> GetG2Details(IEnumerable<CHK_G2_WHINV> whinvs, IEnumerable<BC_WHCHKID> users)
        {
            List<CHK_G2_DETAIL> details = new List<CHK_G2_DETAIL>();
            foreach (CHK_G2_WHINV whinv in whinvs)
            {
                foreach (BC_WHCHKID user in users)
                {
                    CHK_G2_DETAIL detail = new CHK_G2_DETAIL()
                    {
                        CHK_NO = whinv.CHK_NO,
                        WH_NO = whinv.WH_NO,
                        MMCODE = whinv.MMCODE,
                        CHK_UID = user.WH_CHKUID,
                        STATUS_INI = "1"
                    };
                    details.Add(detail);
                }
            }
            return details;
        }

        #endregion

        #region 重盤
        [HttpPost]
        public ApiResponse GetDetailsR(FormDataCollection form) {
            var chk_no1 = form.Get("chk_no1");
            var chk_no = form.Get("chk_no");
            string seq1 = form.Get("seq1");
            string seq2 = form.Get("seq2");
            string mmcode1 = form.Get("mmcode1");
            string mmcode2 = form.Get("mmcode2");
            bool chkpredateNullonly = form.Get("chkpredateNullonly") == "Y";

            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                IEnumerable<CHK_DETAILTOT> items = null;

                try
                {
                    var repo = new CE0021Repository(DBWork);

                    CHK_MAST mast = repo.GetMast(chk_no);
                   
                    items = repo.GetDetialTotsR(chk_no1);

                    // 不存在於CHK_G2_WHINV 新增
                    if (repo.ChkG2WhinvExist(chk_no) == false)
                    {
                        foreach (CHK_DETAILTOT item in items)
                        {
                            CHK_G2_WHINV whinv = new CHK_G2_WHINV()
                            {
                                CHK_NO = chk_no,
                                WH_NO = mast.CHK_WH_NO,
                                MMCODE = item.MMCODE,
                                CREATE_USER = DBWork.UserInfo.UserId,
                                UPDATE_USER = DBWork.UserInfo.UserId,
                                UPDATE_IP = DBWork.ProcIP,
                                SEQ = repo.GetMedSeq(chk_no, item.MMCODE)
                            };
                            repo.InsertG2Whinv(whinv);
                        }
                    }

                    session.Result.etts = repo.GetChkG2Whinvs(chk_no, seq1, seq2, mmcode1, mmcode2, chkpredateNullonly);

                    DBWork.Commit();

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
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0021Repository(DBWork);
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
    }
}