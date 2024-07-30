using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.G;
using WebApp.Models;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

namespace WebApp.Controllers.G
{
    public class GA0002Controller : SiteBase.BaseApiController
    {
        #region master

        [HttpPost]
        public ApiResponse Master(FormDataCollection form)
        {
            var pur_no = form.Get("pur_no");
            var tc_type = form.Get("tc_type");
            var start_date = form.Get("start_date");
            var end_date = form.Get("end_date");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new GA0002Repository(DBWork);
                    session.Result.etts = repo.GetMasters(pur_no, tc_type, start_date, end_date);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse MasterInsert(FormDataCollection form) {
            var pur_date = form.Get("pur_date");
            var tc_type = form.Get("tc_type");
            var pur_note = form.Get("pur_note");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new GA0002Repository(DBWork);

                    TC_PURCH_M master = new TC_PURCH_M();
                    master.PUR_NO = repo.GetTwnSystime();
                    master.PUR_DATE = pur_date;
                    master.TC_TYPE = tc_type;
                    master.PUR_UNM = DBWork.UserInfo.UserId;
                    master.PURCH_ST = "A";
                    master.PUR_NOTE = pur_note;
                    master.CREATE_USER = DBWork.UserInfo.UserId;
                    master.UPDATE_USER = DBWork.UserInfo.UserId;
                    master.UPDATE_IP = DBWork.ProcIP;

                    session.Result.afrs = repo.MasterInsert(master);
                    session.Result.msg = master.PUR_NO;

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
        public ApiResponse MasterUpdate(TC_PURCH_M master) {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new GA0002Repository(DBWork);

                    master.UPDATE_USER = DBWork.UserInfo.UserId;
                    master.UPDATE_IP = DBWork.ProcIP;

                    session.Result.afrs = repo.MasterUpdate(master);

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
        public ApiResponse MasterDelete(FormDataCollection form) {
            var pur_no_string = form.Get("pur_nos");
            string[] pur_nos = pur_no_string.Split(',');
            List<string> list = new List<string>();

            for (int i = 0; i < pur_nos.Length; i++) {
                list.Add(pur_nos[i]);
            }

            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new GA0002Repository(DBWork);


                    foreach (string item in list) {
                        session.Result.afrs = repo.DetailAllDelete(item);
                        session.Result.afrs = repo.MasterDelete(item);
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
        public ApiResponse PlaceOrder(FormDataCollection form)
        {
            var pur_no_string = form.Get("pur_nos");
            string[] pur_nos = pur_no_string.Split(',');
            List<string> list = new List<string>();

            for (int i = 0; i < pur_nos.Length; i++)
            {
                list.Add(pur_nos[i]);
            }

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new GA0002Repository(DBWork);
                    string msg = string.Empty;
                    foreach (string item in list)
                    {
                        msg = string.Empty;
                        IEnumerable<TC_PURCH_DL> details = repo.GetDetails(item);
                        if (details.Any() == false) {
                            session.Result.success = false;
                            session.Result.msg = string.Format("訂單號{0}，無明細項目，無法訂購", item, msg);
                            return session.Result;
                        }
                        foreach (TC_PURCH_DL detail in details) {
                            if (detail.AGEN_NAMEC == string.Empty) {
                                msg += "藥商名稱";
                            }
                            if (detail.PUR_QTY == string.Empty) {
                                if (msg != string.Empty) {
                                    msg += "、";
                                }
                                msg += "訂購數量";
                            }
                            if (detail.PUR_UNIT == string.Empty)
                            {
                                if (msg != string.Empty)
                                {
                                    msg += "、";
                                }
                                msg += "單位劑量";
                            }
                            if (detail.IN_PURPRICE == string.Empty)
                            {
                                if (msg != string.Empty)
                                {
                                    msg += "、";
                                }
                                msg += "進貨單價";
                            }

                            if (msg != string.Empty) {
                                session.Result.success = false;
                                session.Result.msg = string.Format("訂單號{0}，{1}資料不全，無法訂購", item, msg);
                                return session.Result;
                            }
                            
                        }

                        session.Result.afrs = repo.PlaceOrder(item);
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

        #region detail

        [HttpPost]
        public ApiResponse Detail(FormDataCollection form) {
            var pur_no = form.Get("pur_no");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new GA0002Repository(DBWork);
                    session.Result.etts = repo.GetDetails(pur_no);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse DetailDelete(FormDataCollection form) {
            var item_string = form.Get("item_string");
            IEnumerable<TC_PURCH_DL> details = JsonConvert.DeserializeObject<IEnumerable<TC_PURCH_DL>>(item_string);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new GA0002Repository(DBWork);

                    foreach (TC_PURCH_DL detail in details) {
                        session.Result.afrs = repo.DetailDelete(detail);
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
        public ApiResponse Multi(FormDataCollection form)
        {
            var pur_unit = form.Get("pur_unit");
            var base_unit = form.Get("base_unit");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new GA0002Repository(DBWork);
                    session.Result.etts = repo.GetMulti(pur_unit, base_unit);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }


        #endregion

        #region detail insert
        [HttpPost]
        public ApiResponse Invqmtrs(FormDataCollection form)
        {
            var pur_no = form.Get("pur_no");
            var tc_type = form.Get("tc_type");
            var inv_day = form.Get("inv_day");
            var mmcode = form.Get("mmcode");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new GA0002Repository(DBWork);
                    //session.Result.etts = repo.GetInvqmtrs(pur_no, tc_type, inv_day, mmcode);

                    IEnumerable<TC_INVQMTR> items = repo.GetInvqmtrs(pur_no, tc_type, inv_day, mmcode);
                    foreach (TC_INVQMTR item in items)
                    {
                       
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
        [HttpPost]
        public ApiResponse Transfer(FormDataCollection form)
        {
            var item_string = form.Get("item_string");
            var pur_no = form.Get("pur_no");
            IEnumerable<TC_PURCH_DL> invqmtrs = JsonConvert.DeserializeObject<IEnumerable<TC_PURCH_DL>>(item_string);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new GA0002Repository(DBWork);

                    foreach (TC_PURCH_DL item in invqmtrs)
                    {
                        item.CREATE_USER = DBWork.UserInfo.UserId;
                        item.UPDATE_USER = DBWork.UserInfo.UserId;
                        item.UPDATE_IP = DBWork.ProcIP;

                        session.Result.afrs = repo.DetailInsert(item);
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
        public TC_PURCH_DL GetDetailFormat(TC_INVQMTR item, string pur_no)
        {
            TC_PURCH_DL detail = new TC_PURCH_DL();
            detail.PUR_NO = pur_no;
            detail.MMCODE = item.MMCODE;
            detail.AGEN_NAMEC = item.AGEN_NAMEC;    // 藥商名稱
            detail.MMNAME_C = item.MMNAME_C;        // 藥品名稱
            detail.PUR_QTY = item.RCM_PURQTY;       // 建議訂購量
            detail.PUR_UNIT = item.PUR_UNIT;        // 單位劑量
            detail.IN_PURPRICE = item.IN_PURPRICE;  // 進貨單價
            float f;
            if (float.TryParse(detail.PUR_QTY, out f) && float.TryParse(detail.IN_PURPRICE, out f))
            {
                detail.PUR_AMOUNT = Math.Round(float.Parse(detail.PUR_QTY) * float.Parse(detail.IN_PURPRICE), 2).ToString();
            }
            else
            {
                detail.PUR_AMOUNT = "0";
            }




            return detail;
        }

        [HttpPost]
        public ApiResponse Agens(FormDataCollection form) {
            var mmcode = form.Get("mmcode");
            var pur_no = form.Get("pur_no");
            var base_unit = form.Get("base_unit");
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new GA0002Repository(DBWork);
                    //session.Result.etts = repo.GetAgens(mmcode, pur_no, base_unit);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        #endregion

        #region cancel order
        [HttpPost]
        public ApiResponse Orders(FormDataCollection form) {
            var pur_no = form.Get("pur_no");
            var tc_type = form.Get("tc_type");
            var start_date = form.Get("start_date");
            var end_date = form.Get("end_date");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new GA0002Repository(DBWork);
                    session.Result.etts = repo.GetOrders(pur_no, tc_type, start_date, end_date);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse CancelOrder(FormDataCollection form) {
            var pur_no_string = form.Get("pur_nos");
            string[] pur_nos = pur_no_string.Split(',');
            List<string> list = new List<string>();

            for (int i = 0; i < pur_nos.Length; i++)
            {
                list.Add(pur_nos[i]);
            }

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new GA0002Repository(DBWork);                

                    foreach (string item in list)
                    {
                        session.Result.afrs = repo.CancelOrder(item);
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