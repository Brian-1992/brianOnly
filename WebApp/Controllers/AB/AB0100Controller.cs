using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using WebApp.Models;
using WebApp.Repository.AB;

namespace WebApp.Controllers.AB
{
    public class AB0100Controller : SiteBase.BaseApiController
    {
        public class AB0100 {
            public string isSchedule { get; set; }
        }

        [HttpPost]
        public ApiResponse Transfer(AB0100 form) {

            bool isSchedule = ((form != null) && (form.isSchedule == "Y"));

            using (WorkSession session = new WorkSession(this)) {

                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();

                try
                {
                    var repo = new AB0100Repository(DBWork);
                    var repoAB04 = new AB0004Repository(DBWork);

                    // 更新現存未讀取之資料的DOCNO = process
                    session.Result.afrs = repo.UpdateDocnoProcess();

                    // 檢查資料合理性
                    // 1. 檢查：院內碼已於[藥衛材基本檔]建擋
                    session.Result.afrs = repo.UpdateMmcodeNotExists();
                    // 2. 檢查：院內碼符合申請條件
                    session.Result.afrs = repo.UpdateNotMR4Condition();
                    // 3. 檢查：計量單位代碼與藥衛材基本檔相符
                    session.Result.afrs = repo.UpdateBaseunitNotMatch();

                    // 新增ME_DOCM

                    #region 1. 取得DOCNO
                    // 取得系統時間
                    string sysdate_string = repoAB04.GetTwnsystime();
                    // 取得入庫庫房代碼
                    string towh = repo.GetTowh();
                    string docno = string.Format("{0}{1}{2}", towh, sysdate_string, "02");
                    #endregion

                    #region 2. 取得申請單類別(1:常態 2:臨時)
                    string apply_kind = string.Empty;
                    // 每天最多一張常態申請單
                    int apply_kind1_num_daily = repo.CheckApplyKindDaily("02", towh);
                    if (apply_kind1_num_daily >= 1) {
                        apply_kind = "2";
                    }
                    if (apply_kind == string.Empty) {
                        // 衛材庫備每周申請兩次
                        int apply_kind1_num = repo.CheckApplyKindNum("02", towh);
                        if (apply_kind1_num > 1)
                        {
                            apply_kind = "2";
                        }
                    }
                    if (apply_kind == string.Empty) {
                        apply_kind = "1";
                    }
                    #endregion

                    #region 3. 取得申請人帳號
                    string appid = string.Empty;
                    if (isSchedule) {
                        appid = "LIS";
                    }
                    else{
                        appid = DBWork.UserInfo.UserId;

                    }
                    #endregion

                    #region 4. insert me_docd
                    session.Result.afrs = repo.InsertMedocd(docno, appid);
                    #endregion

                    // 有detail再新增master
                    #region 5. insert ME_DOCM

                    if (session.Result.afrs > 0) {
                        // 5. insert ME_DOCM
                        session.Result.afrs = repo.InsertMedocm(docno, apply_kind, appid, towh);
                        // 6. 更新LIS_APP
                        session.Result.afrs = repo.UpdateLisapp(docno);
                    }
                    #endregion

                    DBWork.Commit();
                }
                catch(Exception e)
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
                
            }
        }

        public ApiResponse All(FormDataCollection form)
        {
            string apptime_s = form.Get("apptime_s");
            string apptime_e = form.Get("apptime_e");
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try {
                    var repo = new AB0100Repository(DBWork);

                    session.Result.etts = repo.All(apptime_s, apptime_e);
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