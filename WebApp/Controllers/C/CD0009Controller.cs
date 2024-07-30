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
    public class CD0009Controller : SiteBase.BaseApiController
    {

        public ApiResponse MasterAll(FormDataCollection form)
        {
            var wh_no = form.Get("p0");
            var pick_date = form.Get("p1");
            var apply_date = form.Get("p4");
            var isDistributed = form.Get("p3");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CD0009Repository(DBWork);
                    string user = DBWork.UserInfo.UserId;
                    string ip = DBWork.ProcIP;

                    if (isDistributed == "=") {
                        session.Result.afrs = repo.DeleteBcWhpick(wh_no);
                        session.Result.afrs = repo.DeleteBcWhpickdoc(wh_no);

                        session.Result.afrs = repo.InsertBcwhpick(wh_no, pick_date, string.Empty, user, ip, apply_date);

                        session.Result.afrs = repo.InsertBcwhpickdoc(wh_no, pick_date, string.Empty, user, ip, apply_date);
                    }

                    session.Result.etts = repo.GetMasterAll(wh_no, pick_date, isDistributed, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse DetailAll(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CD0009Repository(DBWork);
                    session.Result.etts = repo.GetDetailAll(p0, p1, p2);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        #region 查詢新申請單資料

        [HttpPost]
        public ApiResponse GetMeDocms(FormDataCollection form)
        {
            var wh_no = form.Get("p0");
            string runDelete = form.Get("runDelete");
            // var p1 = form.Get("p1");
            // var p2 = form.Get("p2");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CD0009Repository(DBWork);

                    if (runDelete == "true")
                    {
                        session.Result.afrs = repo.DeleteBcWhpick(wh_no);
                        session.Result.afrs = repo.DeleteBcWhpickdoc(wh_no);
                    }

                    session.Result.etts = repo.GetMeDocms(wh_no, page, limit, sorters);

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
        public ApiResponse GetMedocds(FormDataCollection form)
        {
            var docno = form.Get("p0");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CD0009Repository(DBWork);
                    session.Result.etts = repo.GetMeDocds(docno);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }

        }

        [HttpPost]
        public ApiResponse TransferToBcwhpick(FormDataCollection form)
        {
            var docno = form.Get("DOCNO");
            var wh_no = form.Get("WH_NO");
            var pick_date = form.Get("PICK_DATE");
            var apply_date = form.Get("APPLY_DATE");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    string user = DBWork.UserInfo.UserId;
                    string ip = DBWork.ProcIP;

                    var repo = new CD0009Repository(DBWork);

                    session.Result.afrs = repo.InsertBcwhpick(wh_no, pick_date, docno, user, ip, apply_date);

                    session.Result.afrs = repo.InsertBcwhpickdoc(wh_no, pick_date, docno, user, ip, apply_date);

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

        #region 分配揀貨員
        [HttpPost]
        public ApiResponse DistributeRegular(FormDataCollection form)
        {
            var wh_no = form.Get("WH_NO");
            var pick_date = form.Get("PICK_DATE");
            var userCount = form.Get("USER_COUNT");
            var docnos = form.Get("DOCNOS");
            var sorter = form.Get("SORTER");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                string userId = DBWork.UserInfo.UserId;
                string ip = DBWork.ProcIP;
                DBWork.BeginTransaction();
                try
                {
                    var repo02 = new CD0002Repository(DBWork);
                    var repo09 = new CD0009Repository(DBWork);
                    // 記錄現在時間
                    string calc_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

                    // 刪除批次分配申請單明細暫存歷史資料
                    int deleteLotsum = repo09.DeleteTemplotdocseq(wh_no);

                    // 查詢最大分配揀貨批次號碼
                    int max_lot_no = repo02.GetMaxLotNo(wh_no, pick_date);
                    int next_lotno = max_lot_no + 1;

                    // 查詢要分配的申請單明細項數資料
                    int pickCount = repo09.GetWhpickCount(wh_no, pick_date, docnos);

                    // 查詢要分配的申請單明細
                    IEnumerable<BC_WHPICK> picks = repo09.GetWhpicks(wh_no, pick_date, docnos, sorter, calc_time, next_lotno);
                    int pick_seq = 0;

                    // 每人應分配項數
                    int base_num = pickCount / int.Parse(userCount);
                    // 零頭數
                    int mod_num = pickCount % int.Parse(userCount);

                    int group_num = 1;
                    int group_count = 0;

                    List<BC_WHPICK_TEMP_LOTDOCSEQ> tempList = new List<BC_WHPICK_TEMP_LOTDOCSEQ>();

                    // 生成站存檔資料
                    foreach (BC_WHPICK pick in picks)
                    {
                        pick_seq++;

                        BC_WHPICK_TEMP_LOTDOCSEQ temp = new BC_WHPICK_TEMP_LOTDOCSEQ()
                        {
                            WH_NO = pick.WH_NO,
                            CALC_TIME = pick.CALC_TIME,
                            LOT_NO = pick.LOT_NO,
                            DOCNO = pick.DOCNO,
                            SEQ = pick.SEQ,
                            MMCODE = pick.MMCODE,
                            APPQTY = pick.APPQTY,
                            BASE_UNIT = pick.BASE_UNIT,
                            MMNAME_C = pick.MMNAME_C,
                            MMNAME_E = pick.MMNAME_E,
                            STORE_LOC = pick.STORE_LOC,
                            PICK_SEQ = pick_seq.ToString(),
                            CREATE_USER = userId,
                            UPDATE_USER = userId,
                            UPDATE_IP = ip
                        };

                        if (group_count < base_num)
                        {
                            group_count++;
                            temp.GROUP_NO = group_num.ToString();
                        }
                        else if (group_count == base_num && mod_num > 0)
                        {
                            mod_num--;
                            temp.GROUP_NO = group_num.ToString();
                            group_num++;
                            group_count = 0;
                        }
                        else if (group_count == base_num && mod_num == 0)
                        {
                            group_count = 1;
                            group_num++;
                            temp.GROUP_NO = group_num.ToString();
                            
                        }

                        tempList.Add(temp);
                        //int seqResult = repo09.InsertTemplotdocseq(temp);
                    }

                    foreach (BC_WHPICK_TEMP_LOTDOCSEQ temp in tempList)
                    {
                        int seqResult = repo09.InsertTemplotdocseq(temp);
                    }

                    IEnumerable<CD0009Repository.DistriMaster> distriMasters = repo09.GetTemplotdocseqMasterItems(wh_no, calc_time, next_lotno.ToString());
                    foreach (CD0009Repository.DistriMaster master in distriMasters)
                    {
                        master.Details = repo09.GetTemplotdocseqDetailItems(master.WH_NO, master.CALC_TIME, master.LOT_NO, master.GROUP_NO);
                    }


                    session.Result.etts = distriMasters;//repo09.GetTemplotdocseqMaster(wh_no, calc_time, next_lotno.ToString());


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
        public ApiResponse GetTemplotdocseqDetail(FormDataCollection form)
        {
            var wh_no = form.Get("WH_NO");
            var calc_time = form.Get("CALC_TIME");
            var group_no = form.Get("GROUP_NO");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CD0009Repository(DBWork);
                    session.Result.etts = repo.GetTemplotdocseqDetail(wh_no, calc_time, group_no);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetDistributeDetail(FormDataCollection form)
        {
            var wh_no = form.Get("WH_NO");
            var calc_time = form.Get("CALC_TIME");
            var group_no = form.Get("GROUP_NO");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CD0009Repository(DBWork);
                    session.Result.etts = repo.GetTemplotdocseqDetail(wh_no, calc_time, group_no);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse SetPickUser(BC_WHPICK_TEMP_LOTDOCSEQ bc_whpick)
        {

            IEnumerable<BC_WHPICK_TEMP_LOTDOCSEQ> picks = JsonConvert.DeserializeObject<IEnumerable<BC_WHPICK_TEMP_LOTDOCSEQ>>(bc_whpick.ITEM_STRING);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CD0009Repository(DBWork);
                    string userId = DBWork.UserInfo.UserId;
                    string ip = DBWork.ProcIP;

                    BC_WHPICK_TEMP_LOTDOCSEQ tempItem = picks.Take(1).FirstOrDefault<BC_WHPICK_TEMP_LOTDOCSEQ>();

                    session.Result.afrs = repo.InsertBcwhpicklot(tempItem.WH_NO, tempItem.PICK_DATE, int.Parse(tempItem.LOT_NO), tempItem.PICK_USERID, userId, ip);
                    session.Result.afrs = repo.UpdateBcwhpickdocReg(tempItem.WH_NO, tempItem.PICK_DATE, tempItem.CALC_TIME, int.Parse(tempItem.LOT_NO), userId, ip);

                    foreach (BC_WHPICK_TEMP_LOTDOCSEQ pick in picks)
                    {
                        pick.UPDATE_USER = userId;
                        pick.UPDATE_IP = ip;
                        session.Result.afrs = repo.UpdateBcwhpickReg(pick);
                    }


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

        #endregion

        #region 已排揀貨批次
        [HttpPost]
        public ApiResponse DistributedMaster(FormDataCollection form)
        {

            var wh_no = form.Get("WH_NO");
            var pick_date = form.Get("PICK_DATE");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CD0009Repository(DBWork);
                    session.Result.etts = repo.GetBcwhpicklots(wh_no, pick_date, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse DistributedDetail(FormDataCollection form)
        {

            var wh_no = form.Get("WH_NO");
            var pick_date = form.Get("PICK_DATE");
            var lot_no = int.Parse(form.Get("LOT_NO"));
            var sorters = form.Get("sorter");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CD0009Repository(DBWork);
                    session.Result.etts = repo.GetPicksByLotno(wh_no, pick_date, lot_no, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }



        [HttpPost]
        public ApiResponse CancelDistributed(FormDataCollection form)
        {
            var wh_no = form.Get("WH_NO");
            var pick_date = form.Get("PICK_DATE");
            var lot_no = form.Get("LOT_NO");


            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    string user = DBWork.UserInfo.UserId;
                    string ip = DBWork.ProcIP;

                    var repo = new CD0002Repository(DBWork);

                    session.Result.afrs = repo.UpdateBcwhpickPickuser(wh_no, pick_date, lot_no, user, ip);
                    session.Result.afrs = repo.UpdateBcwhpickdocLotno(wh_no, pick_date, lot_no, user, ip);
                    session.Result.afrs = repo.DeleteBcwhpicklot(wh_no, pick_date, lot_no);

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

        #region 全部重新分配

        public ApiResponse ClearDistribution(FormDataCollection form)
        {
            var wh_no = form.Get("WH_NO");
            var pick_date = form.Get("PICK_DATE");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    string user = DBWork.UserInfo.UserId;
                    string ip = DBWork.ProcIP;

                    var repo = new CD0009Repository(DBWork);

                    session.Result.afrs = repo.UpdateBcwhpickForClearAll(wh_no, pick_date, user, ip);

                    session.Result.afrs = repo.DeleteBcwhpicklotForClearAll(wh_no, pick_date);

                    session.Result.afrs = repo.UpdateBcwhpickdocForClearAll(wh_no, pick_date, user, ip);

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
