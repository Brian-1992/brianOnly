
using JCLib.DB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using WebApp.Models;
using WebApp.Repository.C;

namespace WebApp.Controllers.C
{
    public class CE0024Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var wh_no = form.Get("wh_no");
            var chk_ym = form.Get("chk_ym");
            var f_u_price = form.Get("F_U_PRICE") == null ? 0 : float.Parse(form.Get("F_U_PRICE"));
            var f_number = form.Get("F_NUMBER") == null ? 0 : float.Parse(form.Get("F_NUMBER"));
            var f_amount = form.Get("F_AMOUNT") == null ? 0 : float.Parse(form.Get("F_AMOUNT"));
            var miss_per = form.Get("MISS_PER") == null ? 0 : float.Parse(form.Get("MISS_PER"));
            var recheck_only = (form.Get("RECHECK_ONLY") == "true");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0024Repository(DBWork);
                    string start_date = repo.GetStartDate(chk_ym);
                    string end_date = repo.GetEndDate(chk_ym);

                    IEnumerable<CHK_G2_DETAILTOT> items = repo.GetAll(wh_no, chk_ym, f_u_price, f_number, f_amount, miss_per, recheck_only, start_date, end_date);
                    foreach (CHK_G2_DETAILTOT item in items) {
                        switch (item.STATUS_TOT) {
                            case "1":
                                item.CHK_QTY = item.CHK_QTY1;
                                break;
                            case "2":
                                item.CHK_QTY = item.CHK_QTY2;
                                break;
                            case "3":
                                item.CHK_QTY = item.CHK_QTY3;
                                break;
                        }
                    }
                    session.Result.etts = items;
                    //session.Result.etts = repo.GetAll(wh_no, chk_ym);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse ChknoExists(FormDataCollection form) {
            var wh_no = form.Get("wh_no");
            var chk_ym = form.Get("chk_ym");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0024Repository(DBWork);
                    session.Result.etts = repo.GetDetailtotChkno(wh_no, chk_ym);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetG2ChkStatus(FormDataCollection form)
        {
            var chk_ym = form.Get("chk_ym");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0024Repository(DBWork);
                    var repoCE0015 = new CE0015Repository(DBWork);

                    IEnumerable<CHK_MAST> masts = repo.GetG2ChkStatus(chk_ym);
                    IEnumerable<ComboItemModel> combos = repoCE0015.GetWhnos();
                    masts = GetGroupMasts(masts);

                    masts = GetChkMasts(masts, combos);

                    session.Result.etts = masts;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public IEnumerable<CHK_MAST> GetChkMasts(IEnumerable<CHK_MAST> masts, IEnumerable<ComboItemModel> combos) {
            List<CHK_MAST> mastList = new List<CHK_MAST>();
            foreach (ComboItemModel item in combos) {
                CHK_MAST nMast = new CHK_MAST();
                nMast.CHK_WH_NO = item.VALUE;
                foreach (CHK_MAST mast in masts) {
                    if (mast.CHK_WH_NO == nMast.CHK_WH_NO) {
                        nMast.CHK_NO = mast.CHK_NO;
                        nMast.CHK_STATUS = mast.CHK_STATUS;
                        nMast.CHK_LEVEL = mast.CHK_LEVEL;
                        
                    }
                }
                mastList.Add(nMast);

            }

            return mastList;
        }

        public IEnumerable<CHK_MAST> GetGroupMasts(IEnumerable<CHK_MAST> masts) {
            // chk_no, chk_wh_no, chk_status, chk_ym, chk_level
            var o = from a in masts
                    group a by new { CHK_WH_NO = a.CHK_WH_NO, CHK_YM = a.CHK_YM } into g
                    select new CHK_MAST
                    {
                        CHK_NO = g.Select(x=>x.CHK_NO).FirstOrDefault(),
                        CHK_WH_NO = g.Key.CHK_WH_NO,
                        CHK_YM = g.Key.CHK_YM,
                        CHK_STATUS = g.Select(x=>x.CHK_STATUS).FirstOrDefault(),
                        CHK_LEVEL = g.Select(x=>x.CHK_LEVEL).FirstOrDefault()
                    };

            if (o.Any())
            {
                return o.ToList<CHK_MAST>();
            }
            return new List<CHK_MAST>();
        }

        public ApiResponse Mark(FormDataCollection form) {
            var item_string = form.Get("item_string");
            var chk_no1 = form.Get("chk_no1");
            IEnumerable<CHK_G2_DETAILTOT> tots = JsonConvert.DeserializeObject<IEnumerable<CHK_G2_DETAILTOT>>(item_string);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0024Repository(DBWork);

                    // 更新重盤項目status
                    foreach (CHK_G2_DETAILTOT tot in tots)
                    {
                        tot.CHK_NO = chk_no1;
                        session.Result.afrs = repo.UpdateG2DetailTotR(tot);

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
        public ApiResponse Recheck(FormDataCollection form) {
            
            var chk_no1 = form.Get("chk_no1");
            //var chk_no = form.Get("chk_no");

            string chk_no = string.Empty;

            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0024Repository(DBWork);

                    if (repo.CheckRecheckItemExists(chk_no1) == false) {
                        session.Result.success = false;
                        session.Result.msg = "尚未設定重盤項目";
                        return session.Result;
                    }

                    IEnumerable<string> tempChkno = repo.GetChkno(chk_no1);
                    if (tempChkno.Any())
                    {
                        chk_no = tempChkno.Take(1).FirstOrDefault();
                    }
                    else {

                        CHK_MAST mast = repo.GetChkMast(chk_no1);
                        mast.CHK_LEVEL = "2";
                        mast.CHK_NUM = "0";
                        mast.CHK_TOTAL = "0";
                        mast.CHK_STATUS = "4";
                        mast.CHK_NO = GetChkNo(mast);  
                        mast.CHK_NO1 = chk_no1;
                        mast.UPDATE_USER = mast.CREATE_USER;
                        mast.UPDATE_IP = DBWork.ProcIP;

                        session.Result.afrs = repo.InsertMaster(mast);

                        chk_no = mast.CHK_NO;
                    }

                    session.Result.afrs = repo.DeleteG2Detail(chk_no);
                    session.Result.afrs = repo.DeleteG2Whinv(chk_no);
                    session.Result.afrs = repo.DeleteG2Updn(chk_no);

                    session.Result.afrs = repo.ClearRecheckMark(chk_no1);

                    session.Result.afrs = repo.UpdateMast(chk_no);

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
            string period = master.CHK_PERIOD == "D" ? "D" : "B";

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
        public ApiResponse Finish(FormDataCollection form) {
            var chk_ym = form.Get("chk_ym");

            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();

                try
                {
                    var repo = new CE0024Repository(DBWork);
                    var repoCE0004 = new CE0004Repository(DBWork);

                    IEnumerable<CHK_G2_DETAILTOT> g2tots = repo.GetAllG2Details(chk_ym);

                    foreach (CHK_G2_DETAILTOT g2tot in g2tots) {
                        MI_MAST mimast = repo.GetMimast(g2tot.MMCODE);

                        CHK_MAST chk_mast = repo.GetChkMast(g2tot.CHK_NO);

                        CHK_DETAILTOT tot = GetDetilTot(g2tot, mimast);
                        string start_date = repo.GetStartDate(chk_mast.CHK_YM);
                        string end_date = repo.GetEndDate(chk_mast.CHK_YM);


                        tot.LAST_QTY = repoCE0004.GetLAST_QTYC(chk_ym, g2tot.MMCODE, g2tot.WH_NO);
                        tot.LAST_QTYC = tot.LAST_QTY;
                        tot.APL_OUTQTY = repo.GetAploutqty(start_date, end_date, g2tot.WH_NO, g2tot.MMCODE);

                        tot.MISS_PER = GetMissPer(tot);
                        tot.MISS_PERC = tot.MISS_PER;

                        tot.CREATE_USER = DBWork.UserInfo.UserId;
                        tot.UPDATE_USER = DBWork.UserInfo.UserId;
                        tot.UPDATE_IP = DBWork.ProcIP;

                        session.Result.afrs = repo.InsertChkDetailTot(tot);
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

        public CHK_DETAILTOT GetDetilTot(CHK_G2_DETAILTOT g2tot, MI_MAST mimast) {
            return new CHK_DETAILTOT()
            {
                CHK_NO = g2tot.CHK_NO,
                MMCODE = g2tot.MMCODE,
                MMNAME_C = mimast.MMNAME_C,
                MMNAME_E = mimast.MMNAME_E,
                BASE_UNIT = mimast.BASE_UNIT,
                M_CONTPRICE = mimast.M_CONTPRICE,
                WH_NO = g2tot.WH_NO,
                MAT_CLASS = mimast.MAT_CLASS,
                M_STOREID = mimast.M_STOREID,
                STORE_QTY = g2tot.STORE_QTY,
                STORE_QTYC = g2tot.STORE_QTY,
                STORE_QTYM = "0",
                STORE_QTYS = "0",
                LAST_QTYM = "0",
                LAST_QTYS = "0",
                GAP_T = g2tot.GAP_T,
                GAP_C = g2tot.GAP_T,
                GAP_M = "0",
                GAP_S = "0",
                PRO_LOS_QTY = g2tot.GAP_T,
                PRO_LOS_AMOUNT = mimast.M_CONTPRICE == null ? null : (double.Parse(g2tot.GAP_T) * double.Parse(mimast.M_CONTPRICE)).ToString(),
                MISS_PERM = "0",
                MISS_PERS = "0",
                CHK_QTY1 = g2tot.CHK_QTY1,
                CHK_QTY2 = g2tot.CHK_QTY2 == null ? "0" : g2tot.CHK_QTY2,
                CHK_QTY3 = g2tot.CHK_QTY3 == null ? "0" : g2tot.CHK_QTY3,
                STATUS_TOT = g2tot.STATUS_TOT,
                CHK_REMARK = string.Empty
            };
        }
        public string GetMissPer(CHK_DETAILTOT tot) {
            if (string.IsNullOrEmpty(tot.APL_OUTQTY) || tot.APL_OUTQTY == "0") {
                return Math.Round(double.Parse(tot.GAP_T) * 100, 5).ToString();
            }
            return Math.Round(double.Parse(tot.GAP_T) / double.Parse(tot.APL_OUTQTY), 5).ToString();
        }

        [HttpPost]
        public ApiResponse GetIfFinish(FormDataCollection form) {
            string chk_ym = form.Get("chk_ym");
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0024Repository(DBWork);

                    session.Result.msg = repo.CheckDetailtotExists(chk_ym) ? "Y" : "N";
                }
                catch {
                    throw;   
                }

                return session.Result;
            }
        }



        #region 2020-07-15 新增: 備註欄位(CHK_G2_DETAILTOT.MEMO)
        public ApiResponse UpdateMemo(FormDataCollection form)
        {
            string item_string = form.Get("item_string");
            IEnumerable<CHK_G2_DETAILTOT> list = JsonConvert.DeserializeObject<IEnumerable<CHK_G2_DETAILTOT>>(item_string);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0024Repository(DBWork);

                    foreach (CHK_G2_DETAILTOT item in list)
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

        #region 2020-09-08 新增: 目前藥局盤盈虧結果查詢、消耗量日期起迄
        [HttpPost]
        public ApiResponse GetChkResult(FormDataCollection form) {
            string chk_ym = form.Get("chk_ym");
            string wh_no = form.Get("wh_no")== null ? string.Empty : form.Get("wh_no");
            string content_type = form.Get("content_type");

            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0024Repository(DBWork);

                    string start_date = repo.GetStartDate(chk_ym);
                    string end_date = repo.GetEndDate(chk_ym);

                    session.Result.etts = repo.GetChkResult(chk_ym, wh_no, content_type, start_date, end_date);
                }
                catch
                {
                    throw;
                }

                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetChkCounts(FormDataCollection form)
        {
            string chk_ym = form.Get("chk_ym");
            string wh_no = form.Get("wh_no") == null ? string.Empty : form.Get("wh_no");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0024Repository(DBWork);

                    string start_date = repo.GetStartDate(chk_ym);
                    string end_date = repo.GetEndDate(chk_ym);

                    session.Result.etts = repo.GetChkCounts(chk_ym, wh_no, start_date, end_date);
                }
                catch
                {
                    throw;
                }

                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse AploutqtyDateRange(FormDataCollection form) {
            string chk_ym = form.Get("chk_ym");
            
            using (WorkSession session = new WorkSession(this)) {

                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0024Repository(DBWork);

                    string start_date = repo.GetStartDate(chk_ym);
                    string end_date = repo.GetEndDate(chk_ym);

                    session.Result.msg = string.Format("消耗量計算區間：{0} 至 {1}", start_date, end_date);
                }
                catch
                {
                    throw;
                }

                return session.Result;

            }
        }

        #endregion

        #region 2020-10-20 新增: 匯出盤盈虧結果
        [HttpPost]
        public ApiResponse Excel(FormDataCollection form) {
            string chk_ym = form.Get("chk_ym");
            string wh_no = form.Get("wh_no") == null ? string.Empty : form.Get("wh_no");
            string content_type = form.Get("content_type");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0024Repository repo = new CE0024Repository(DBWork);

                    string start_date = repo.GetStartDate(chk_ym);
                    string end_date = repo.GetEndDate(chk_ym);

                    using (var dataSet1 = new System.Data.DataSet())
                    {
                        
                        var dataTable1 = repo.GetChkResultExcel(chk_ym, wh_no, content_type, start_date, end_date);
                        dataTable1.TableName = "盤點品項資料";
                        dataSet1.Tables.Add(dataTable1);

                        DataTable dataTable2 = new DataTable();
                        dataTable2.Columns.Add("盤總項目", typeof(string));
                        dataTable2.Columns.Add("盤總數值", typeof(string));
                        dataTable2.Columns.Add("盤盈項目", typeof(string));
                        dataTable2.Columns.Add("盤盈數值", typeof(string));
                        dataTable2.Columns.Add("盤虧項目", typeof(string));
                        dataTable2.Columns.Add("盤虧數值", typeof(string));

                        IEnumerable<CE0027Count> counts = repo.GetChkCounts(chk_ym, wh_no, start_date, end_date);
                        CE0027Count item = counts.First();
                        dataTable2 = GetChkCountsDataTable(item, dataTable2);

                        dataTable2.TableName = "盤盈虧金額";
                        dataSet1.Tables.Add(dataTable2);

                        JCLib.Excel.Export(string.Format("{0}_藥局盤盈虧報表現況_{1}.xls", chk_ym, wh_no == string.Empty ? "全部" : wh_no)
                                           , dataSet1);
                    }

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public DataTable GetChkCountsDataTable(CE0027Count item, DataTable table) {

            DataRow dt = table.NewRow();
            dt["盤總項目"] = "盤總品項現品量總金額";
            dt["盤總數值"] = item.TOT1;
            dt["盤盈項目"] = "盤盈品項現品量總金額";
            dt["盤盈數值"] = item.P_TOT1;
            dt["盤虧項目"] = "盤虧品項現品量總金額";
            dt["盤虧數值"] = item.N_TOT1;
            table.Rows.Add(dt);

            dt = table.NewRow();
            dt["盤總項目"] = "盤總品項電腦量總金額";
            dt["盤總數值"] = item.TOT2;
            dt["盤盈項目"] = "盤盈品項電腦量總金額";
            dt["盤盈數值"] = item.P_TOT2;
            dt["盤虧項目"] = "盤虧品項電腦量總金額";
            dt["盤虧數值"] = item.N_TOT2;
            table.Rows.Add(dt);

            dt = table.NewRow();
            dt["盤總項目"] = "盤總品項誤差百分比";
            dt["盤總數值"] = item.TOT3;
            dt["盤盈項目"] = "盤盈品項誤差百分比";
            dt["盤盈數值"] = item.P_TOT3;
            dt["盤虧項目"] = "盤虧品項誤差百分比";
            dt["盤虧數值"] = item.N_TOT3;
            table.Rows.Add(dt);

            dt = table.NewRow();
            dt["盤總項目"] = "盤總品項誤差總金額";
            dt["盤總數值"] = item.TOT4;
            dt["盤盈項目"] = "盤盈品項誤差總金額";
            dt["盤盈數值"] = item.P_TOT4;
            dt["盤虧項目"] = "盤虧品項誤差總金額";
            dt["盤虧數值"] = item.N_TOT4;
            table.Rows.Add(dt);

            dt = table.NewRow();
            dt["盤總項目"] = "盤總品項當季耗總金額";
            dt["盤總數值"] = item.TOT5;
            dt["盤盈項目"] = "盤盈品項當季耗總金額";
            dt["盤盈數值"] = item.P_TOT5;
            dt["盤虧項目"] = "盤虧品項當季耗總金額";
            dt["盤虧數值"] = item.N_TOT5;
            table.Rows.Add(dt);


            return table;
        }
        #endregion
    }
}