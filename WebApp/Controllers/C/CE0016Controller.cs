using JCLib.DB;
using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using WebApp.Models;
using WebApp.Models.C;
using WebApp.Repository.C;

namespace WebApp.Controllers.C
{
    public class CE0016Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse Masters(FormDataCollection form) {
            string wh_no = form.Get("wh_no");
            string chk_ym = form.Get("chk_ym");
            string chk_level = form.Get("chk_level");
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0016Repository repo = new CE0016Repository(DBWork);
                    CE0002Repository repoCE0002 = new CE0002Repository(DBWork);

                    IEnumerable<CE0003> masts = repo.GetMasters(wh_no, chk_ym, DBWork.UserInfo.UserId, chk_level);
                    foreach (CE0003 mast in masts) {
                        mast.CHK_TYPE = repoCE0002.GetChkWhkindName(mast.CHK_WH_KIND_CODE, mast.CHK_TYPE_CODE);
                        mast.CHK_NO1_CREATE_USER = repo.GetCreateUser(mast.CHK_NO1);
                    }

                    session.Result.etts = masts;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Details(FormDataCollection form) {
            string chk_no = form.Get("chk_no");
            string mmcode = form.Get("mmcode");
            string chk_time_status = form.Get("chk_time_status");
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0016Repository repo = new CE0016Repository(DBWork);

                    IEnumerable<CE0003> list = repo.GetDetails(chk_no, mmcode, chk_time_status);

                    foreach (CE0003 item in list) {
                        double use_qty = item.USE_QTY == null ? 0 : double.Parse(item.USE_QTY);
                        double back_qty = item.BACK_QTY == null ? 0 : double.Parse(item.BACK_QTY);

                        item.USE_QTY = (use_qty - back_qty).ToString();
                    }

                    CHK_MAST mast = repo.GetChkmast(chk_no);
                    // 非能設通信庫房、盤點單狀態為盤中、開單日期屬於目前月結期間才去call webapi
                    if (mast.CHK_WH_KIND != "E" && mast.CHK_WH_KIND != "C" && 
                        mast.CHK_STATUS == "1" && 
                        repo.IsCurrentSetYm(chk_no)) {
                        list = GetNewUSEO_QTY(DBWork, list, mast.CHK_WH_NO);
                    }

                    session.Result.etts = list;

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //public string GetPreym(string chkym)
        //{
        //    string mm = chkym.Substring(3, 2);
        //    string yyy = chkym.Substring(0, 3);

        //    int m = int.Parse(mm);
        //    int y = int.Parse(yyy);
        //    if (m == 1) {
        //        y = y - 1;
        //        m = 12;
        //    }
        //    else {
        //        m = m - 1;
        //    }
        //    mm = m > 9 ? m.ToString() : string.Format("0{0}", m);

        //    return string.Format("{0}{1}", y, mm);
        //}

        [HttpPost]
        public ApiResponse UpdateDetails(FormDataCollection form) {
            var itemString = form.Get("list");
            string wh_kind = form.Get("wh_kind");

            IEnumerable<CHK_DETAIL> items = JsonConvert.DeserializeObject<IEnumerable<CHK_DETAIL>>(itemString);

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    CE0016Repository repo = new CE0016Repository(DBWork);

                    foreach (CHK_DETAIL item in items) {

                        CHK_MAST mast = repo.GetChkmast(item.CHK_NO);
                        if (mast.CHK_STATUS != "1") {
                            session.Result.success = false;
                            session.Result.msg = "盤點單狀態已變更，請重新查詢";
                            return session.Result;
                        }

                        item.CHK_UID = DBWork.UserInfo.UserId;
                        item.UPDATE_USER = DBWork.UserInfo.UserId;
                        item.UPDATE_IP = DBWork.ProcIP;
                        item.STATUS_INI = "1";

                        session.Result.afrs = repo.UpdateChkDetail(item);
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
        public ApiResponse UpdateDetailsCE(FormDataCollection form)
        {
            var itemString = form.Get("list");
            string wh_kind = form.Get("wh_kind");

            IEnumerable<CHK_DETAIL> items = JsonConvert.DeserializeObject<IEnumerable<CHK_DETAIL>>(itemString);

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    CE0016Repository repo = new CE0016Repository(DBWork);
                    foreach (CHK_DETAIL item in items)
                    {
                        item.CHK_UID = DBWork.UserInfo.UserId;
                        item.UPDATE_USER = DBWork.UserInfo.UserId;
                        item.UPDATE_IP = DBWork.ProcIP;
                        item.STATUS_INI = "1";

                        session.Result.afrs = repo.UpdateChkDetailCE(item, wh_kind);
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
        public ApiResponse FinishAll(FormDataCollection form) {
            var chk_nos_string = form.Get("chk_nos");

            IEnumerable<string> chk_nos = GetChknos(chk_nos_string);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0016Repository(DBWork);
                    var repoCE0003 = new CE0003Repository(DBWork);
                    foreach (string chk_no in chk_nos)
                    {
                        CHK_MAST mast = repo.GetChkmast(chk_no);
                        if (mast.CHK_STATUS != "1")
                        {
                            session.Result.success = false;
                            session.Result.msg = "盤點單狀態已變更，請重新查詢";
                            return session.Result;
                        }

                        var count = repo.GetUndoneDetailCount(chk_no);
                        if (count != "0") //沒盤完
                        {
                            session.Result.success = false;
                            session.Result.msg = string.Format("盤點單號：{0} 尚有{1}品項未盤點", chk_no, count);
                            return session.Result;
                        }

                    }
                    foreach (string chk_no in chk_nos)
                    {
                        repo.UpdateChkDetailAll(chk_no, User.Identity.Name, DBWork.ProcIP);
                        repo.UpdateChkmast(chk_no, User.Identity.Name, DBWork.ProcIP);  //Update CHK_NUM
                        session.Result.msg = "您已經完成此單號的盤點!";
                    }


                }
                catch
                {
                    throw;
                }
                return session.Result;
            }

        }
        public IEnumerable<string> GetChknos(string tempChknos)
        {
            string[] temp = tempChknos.Split(',');
            List<string> list = new List<string>();
            for (int i = 0; i < temp.Length; i++)
            {
                list.Add(temp[i].Replace("'", string.Empty));
            }

            return list;
        }

        #region 多筆盤點
        [HttpPost]
        public ApiResponse DetailsMulti(FormDataCollection form)
        {

            var chk_nos = form.Get("chk_nos");
            string mmcode = form.Get("mmcode");
            string chk_time_status = form.Get("chk_time_status");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0016Repository repo = new CE0016Repository(DBWork);

                    IEnumerable<CHK_MAST> masts = repo.GetChkmasts(chk_nos);
                    IEnumerable<string> wh_nos = masts.Select(x => x.CHK_WH_NO).Distinct<string>();
                    IEnumerable<string> chk_yms = masts.Select(x => x.CHK_YM.Substring(0,5)).Distinct<string>();

                    IEnumerable<CE0003> list = repo.GetChkDetailsMulti(chk_nos, DBWork.UserInfo.UserId, mmcode, chk_time_status);

                    foreach (CE0003 item in list)
                    {
                        double use_qty = item.USE_QTY == null ? 0 : double.Parse(item.USE_QTY);
                        double back_qty = item.BACK_QTY == null ? 0 : double.Parse(item.BACK_QTY);

                        item.USE_QTY = (use_qty - back_qty).ToString();
                    }

                    // 所選庫房相同，盤點年月相同，不為能設通信，盤點單狀態為盤中，屬於本次開帳中之年月再去更新待扣醫令數量
                    if (wh_nos.Count()  == 1 && 
                        chk_yms.Count() == 1 &&
                        masts.First().CHK_WH_KIND != "E" && masts.First().CHK_WH_KIND != "C" &&
                        masts.First().CHK_STATUS == "1" &&
                        repo.IsCurrentSetYmMulti(chk_nos)
                        )
                    {
                        list = GetNewUSEO_QTY(DBWork, list, wh_nos.First()) ;    
                    }

                    session.Result.etts = list;

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        #endregion

        #region  全部符合
        [HttpPost]
        public ApiResponse Match(FormDataCollection form)
        {
            var itemString = form.Get("list");

            IEnumerable<CHK_DETAIL> items = JsonConvert.DeserializeObject<IEnumerable<CHK_DETAIL>>(itemString);
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0016Repository(DBWork);

                    foreach (CHK_DETAIL item in items)
                    {
                        CHK_MAST mast = repo.GetChkmast(item.CHK_NO);
                        if (mast.CHK_STATUS != "1")
                        {
                            session.Result.success = false;
                            session.Result.msg = "盤點單狀態已變更，請重新查詢";
                            return session.Result;
                        }

                        item.CHK_UID = DBWork.UserInfo.UserId;
                        item.UPDATE_USER = DBWork.UserInfo.UserId;
                        item.UPDATE_IP = DBWork.ProcIP;

                        string set_ym = repo.GetSetym(item.CHK_NO);

                        session.Result.afrs = repo.UpdateChkQtyMatch(item, set_ym);
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
        public ApiResponse MatchCE(FormDataCollection form) {
            var chk_nos = form.Get("chk_no");
            var wh_kind = form.Get("wh_kind");
            using (WorkSession session = new WorkSession(this)) {

                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0016Repository(DBWork);
                    session.Result.afrs = repo.UpdateChkQtyMatchCE(chk_nos, wh_kind, DBWork.UserInfo.UserId, DBWork.ProcIP);

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

        #region call api
        public IEnumerable<CE0003> GetNewUSEO_QTY(UnitOfWork DBWork, IEnumerable<CE0003> list, string wh_no)
        {
            CallHisApiResult hisResult = GetCallHisApiData(DBWork, list, wh_no);

            CE0016Repository repo = new CE0016Repository(DBWork);

            // 無回傳資料: 呼叫API錯誤 回傳原陣列 不更新醫令扣庫時間 與待扣數量 
            if (hisResult.APIResultData == null)
            {
                return list;
            }

            // 更新醫令扣庫時間 與待扣數量 
            COMBO_MODEL maxdatatime = repo.GetMaxConsumeDate();
            int afrs = -1;

            IEnumerable<APIResultData> orderList = hisResult.APIResultData;
            int count = 0;
            foreach (CE0003 item in list)
            {
                count = 0;
                // 取得清單內與此院內碼相同的資料
                List<APIResultData> temps = orderList.Where(x => x.ORDERCODE == item.MMCODE).Select(x => x).ToList<APIResultData>();
                // 若找不到，更新待扣數量為0，繼續下一筆
                if (temps.Any() == false)
                {
                    afrs = repo.UpdateHisConsumeQtyT(item.CHK_NO, item.MMCODE, 0, maxdatatime.VALUE);
                    continue;
                }
                // 有找到資料，計算耗用量，更新待扣數量
                foreach (APIResultData temp in temps)
                {
                    bool isNumber = int.TryParse(temp.USEQTY, out int i);
                    if (isNumber)
                    {
                        count += int.Parse(temp.USEQTY);
                    }
                }
                afrs = repo.UpdateHisConsumeQtyT(item.CHK_NO, item.MMCODE, count, maxdatatime.VALUE);

                item.HIS_CONSUME_QTY_T = count.ToString();
                item.HIS_CONSUME_DATATIME = maxdatatime.VALUE;
                // 更新電腦量(畫面顯示用)
                item.STORE_QTYC = (double.Parse(item.STORE_QTYC) - count).ToString();
                // 更新醫令扣庫量(畫面顯示用)
                item.USE_QTY = (double.Parse(item.USE_QTY) + count).ToString();
                // 若有輸入盤點量，更新盤差(畫面顯示用)
                if (item.CHK_QTY != null && item.CHK_QTY != string.Empty)
                {
                    item.DIFF_QTY = (double.Parse(item.CHK_QTY) - double.Parse(item.STORE_QTYC)).ToString();
                }
            }

            return list;
        }

        public CallHisApiResult GetCallHisApiData(UnitOfWork DBWork, IEnumerable<CE0003> list, string wh_no)
        {
            CE0016Repository repo = new CE0016Repository(DBWork);

            COMBO_MODEL datetimeItem = repo.GetMaxConsumeDate();

            CallHisApiData data = new CallHisApiData();
            data.StartDateTime = datetimeItem.VALUE;
            data.EndDateTime = datetimeItem.TEXT;
            data.OrderCode = GetMmcodes(list);
            data.StockCode = wh_no;

            string url = $"http://f5-hisregweb.ndmctsgh.edu.tw/DrugQuantity/api/DrugApplyMultiple";
            //string url = $"http://192.168.3.110:871/api/DrugApply";

            CallHisApiResult result = CallHisApiController.CallWebApi.JsonPostAsync(data, url).Result;

            return result;
        }

        

        public string GetMmcodes(IEnumerable<CE0003> list)
        {
            string mmcodes = string.Empty;
            foreach (CE0003 item in list)
            {
                if (mmcodes != string.Empty)
                {
                    mmcodes += ",";
                }
                mmcodes += item.MMCODE;
            }
            return mmcodes;
        }
        #endregion

    }
}