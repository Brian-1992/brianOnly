using System;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AB;
using WebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Data;
using NPOI.SS.UserModel;
using System.Web;
using System.IO;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;

namespace WebApp.Controllers.AB
{
    public class AB0052Controller : SiteBase.BaseApiController
    {

        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0052Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1, p2);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetWexpinvs(FormDataCollection form)
        {
            string wh_no = form.Get("wh_no");
            string transMonth = form.Get("transMonth");

            DateTime maxDate = DateTime.Now.AddMonths(int.Parse(transMonth));

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0052Repository(DBWork);
                    IEnumerable<MI_WEXPINV> miwexpinvs = repo.GetMiWexpinvs(wh_no, maxDate);
                    IEnumerable<MI_WEXPINV_TRAN> trans = GetMiWexpinvTran(miwexpinvs);

                    foreach (MI_WEXPINV_TRAN tran in trans)
                    {
                        ME_EXPD item = new ME_EXPD();
                        item.WH_NO = tran.WH_NO;
                        item.MMCODE = tran.MMCODE;
                        item.EXP_DATE = DateTime.Now.AddDays((-DateTime.Now.Day) + 1).ToString("yyyy-MM-dd");
                        item.EXP_SEQ = repo.GetMaxExpSeq(tran.WH_NO, tran.MMCODE, DateTime.Now).ToString();
                        item.EXP_DATE1 = tran.EXPDATE_LOTNO.Count > 0 ?
                                          DateTransfer(tran.EXPDATE_LOTNO.Skip(0).Take(1).FirstOrDefault().EXP_DATE) :
                                          null;
                        item.LOT_NO1 = tran.EXPDATE_LOTNO.Count > 0 ?
                                        tran.EXPDATE_LOTNO.Skip(0).Take(1).FirstOrDefault().LOT_NO : null;
                        item.EXP_DATE2 = tran.EXPDATE_LOTNO.Count > 1 ?
                                          DateTransfer(tran.EXPDATE_LOTNO.Skip(1).Take(1).FirstOrDefault().EXP_DATE) :
                                          null;
                        item.LOT_NO2 = tran.EXPDATE_LOTNO.Count > 1 ? tran.EXPDATE_LOTNO.Skip(1).Take(1).FirstOrDefault().LOT_NO : null;
                        item.EXP_DATE3 = tran.EXPDATE_LOTNO.Count > 2 ?
                                          DateTransfer(tran.EXPDATE_LOTNO.Skip(2).Take(1).FirstOrDefault().EXP_DATE) :
                                          null; ;
                        item.LOT_NO3 = tran.EXPDATE_LOTNO.Count > 2 ? tran.EXPDATE_LOTNO.Skip(2).Take(1).FirstOrDefault().LOT_NO : null;
                        item.EXP_STAT = "1";
                        item.CREATE_ID = User.Identity.Name;
                        item.UPDATE_ID = User.Identity.Name;
                        item.IP = String.IsNullOrEmpty(DBWork.ProcIP) ? "::1" : DBWork.ProcIP;

                        int setResult = repo.Create(item);
                        session.Result.afrs = setResult;
                    }

                    DBWork.Commit();

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        public IEnumerable<MI_WEXPINV_TRAN> GetMiWexpinvTran(IEnumerable<MI_WEXPINV> miwexpinvs)
        {
            var o = from a in miwexpinvs
                    group a by new
                    {
                        WH_NO = a.WH_NO,
                        MMCODE = a.MMCODE,
                        WH_NAME = a.WH_NAME,
                        MMNAME_C = a.MMNAME_C,
                        MMNAME_E = a.MMNAME_E
                    } into g
                    select new MI_WEXPINV_TRAN
                    {
                        WH_NO = g.Key.WH_NO,
                        MMCODE = g.Key.MMCODE,
                        WH_NAME = g.Key.WH_NAME,
                        MMNAME_C = g.Key.MMNAME_C,
                        MMNAME_E = g.Key.MMNAME_E,
                        EXPDATE_LOTNO = g.ToList()
                    };
            if (o.Any())
            {
                return o.ToList<MI_WEXPINV_TRAN>();
            }
            return new List<MI_WEXPINV_TRAN>();
        }
        public class MI_WEXPINV_TRAN
        {
            public string WH_NO { get; set; }
            public string MMCODE { get; set; }
            public string MMNAME_C { get; set; }        //中文品名
            public string MMNAME_E { get; set; }
            public string WH_NAME { get; set; }
            public List<MI_WEXPINV> EXPDATE_LOTNO { get; set; }
        }
        public bool SetMeExpds(IEnumerable<MI_WEXPINV_TRAN> trans, string ip)
        {
            int i = 0;

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                var repo = new AB0052Repository(DBWork);
                try
                {
                    foreach (MI_WEXPINV_TRAN tran in trans)
                    {
                        ME_EXPD item = new ME_EXPD();
                        item.WH_NO = tran.WH_NO;
                        item.MMCODE = tran.MMCODE;
                        item.EXP_DATE = DateTime.Now.AddDays((-DateTime.Now.Day) + 1).ToString("yyyy-MM-dd");
                        item.EXP_SEQ = repo.GetMaxExpSeq(tran.WH_NO, tran.MMCODE, DateTime.Now).ToString();
                        item.EXP_DATE1 = tran.EXPDATE_LOTNO.Count > 0 ?
                                          DateTransfer(tran.EXPDATE_LOTNO.Skip(0).Take(1).FirstOrDefault().EXP_DATE) :
                                          null;
                        item.LOT_NO1 = tran.EXPDATE_LOTNO.Count > 0 ?
                                        tran.EXPDATE_LOTNO.Skip(0).Take(1).FirstOrDefault().LOT_NO : null;
                        item.EXP_DATE2 = tran.EXPDATE_LOTNO.Count > 1 ?
                                          DateTransfer(tran.EXPDATE_LOTNO.Skip(1).Take(1).FirstOrDefault().EXP_DATE) :
                                          null;
                        item.LOT_NO2 = tran.EXPDATE_LOTNO.Count > 1 ? tran.EXPDATE_LOTNO.Skip(1).Take(1).FirstOrDefault().LOT_NO : null;
                        item.EXP_DATE3 = tran.EXPDATE_LOTNO.Count > 2 ?
                                          DateTransfer(tran.EXPDATE_LOTNO.Skip(2).Take(1).FirstOrDefault().EXP_DATE) :
                                          null; ;
                        item.LOT_NO3 = tran.EXPDATE_LOTNO.Count > 2 ? tran.EXPDATE_LOTNO.Skip(2).Take(1).FirstOrDefault().LOT_NO : null;
                        item.EXP_STAT = "1";
                        item.CREATE_ID = User.Identity.Name;
                        item.UPDATE_ID = User.Identity.Name;
                        item.IP = String.IsNullOrEmpty(ip) ? "::1" : ip;

                        int setResult = repo.Create(item);
                        i++;
                    }

                    DBWork.Commit();
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }

                //return repo.GetAll();
                return i == trans.Count();
            }
        }
        public string DateTransfer(string date)
        {
            return DateTime.Parse(date).ToString("yyyy-MM-dd");
        }

        [HttpPost]
        public ApiResponse Add(ME_EXPD me_expd)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0052Repository(DBWork);

                    me_expd.EXP_DATE = DateTime.Now.AddDays((-DateTime.Now.Day) + 1).ToString("yyyy-MM-dd");

                    if (repo.CheckExistsAdd(me_expd.WH_NO, me_expd.MMCODE, me_expd.LOT_NO, me_expd.EXP_DATE) == false)
                    {
                        session.Result.msg = "該月已存在此回報資料，請重新輸入";
                        session.Result.success = false;
                        return session.Result;
                    }


                    me_expd.EXP_SEQ = repo.GetMaxExpSeq(me_expd.WH_NO, me_expd.MMCODE, DateTime.Now).ToString();
                    me_expd.EXP_DATE = DateTime.Now.AddDays((-DateTime.Now.Day) + 1).ToString("yyyy-MM-dd");
                    me_expd.EXP_STAT = "1";
                    //me_expd.REPLY_DATE = DateTime.Parse(me_expd.REPLY_DATE).ToString("yyyy-MM-dd");
                    me_expd.REPLY_ID = User.Identity.Name;
                    me_expd.CREATE_ID = User.Identity.Name;
                    me_expd.UPDATE_ID = User.Identity.Name;
                    me_expd.IP = DBWork.ProcIP;
                    session.Result.afrs = repo.CreateManual(me_expd);
                    //session.Result.etts = repo.Get(mi_wexpinv.WH_NO, mi_wexpinv.MMCODE, mi_wexpinv.LOT_NO, mi_wexpinv.EXP_DATE);

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
        public ApiResponse Update(ME_EXPD me_expd)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0052Repository(DBWork);

                    //if (repo.CheckExistsUpdate(me_expd.WH_NO, me_expd.MMCODE, me_expd.LOT_NO, me_expd.EXP_DATE, me_expd.REPLY_DATE))
                    //{
                    //    session.Result.msg = "該月已存在此回報資料，請重新輸入";
                    //    session.Result.success = false;
                    //    return session.Result;
                    //}
                    ME_EXPD existing = repo.GetExistingExpd(me_expd.WH_NO, me_expd.MMCODE, me_expd.LOT_NO, me_expd.EXP_DATE, me_expd.REPLY_DATE);
                    if (existing != null && existing.EXP_SEQ != me_expd.EXP_SEQ) {
                        session.Result.msg = "該月已存在此回報資料，請重新輸入";
                        session.Result.success = false;
                        return session.Result;
                    }

                    //me_expd.EXP_DATE = DateTime.Parse(me_expd.EXP_DATE).ToString("yyyy-MM-dd");
                    //me_expd.REPLY_DATE = DateTime.Parse(me_expd.REPLY_DATE).ToString("yyyy-MM-dd");
                    me_expd.REPLY_ID = User.Identity.Name;
                    me_expd.UPDATE_ID = User.Identity.Name;
                    me_expd.IP = DBWork.ProcIP;
                    session.Result.afrs = repo.Update(me_expd);
                    //session.Result.etts = repo.Get(mi_wexpinv.WH_NO, mi_wexpinv.MMCODE, mi_wexpinv.LOT_NO, mi_wexpinv.EXP_DATE);

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
        //public ApiResponse Delete(ME_EXPD me_expd)
        public ApiResponse Delete(FormDataCollection form)
        {
            var item_string = form.Get("list");
            IEnumerable<ME_EXPD> expds = JsonConvert.DeserializeObject<IEnumerable<ME_EXPD>>(item_string);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0052Repository(DBWork);

                    //me_expd.EXP_DATE = DateTime.Parse(me_expd.EXP_DATE).ToString("yyyy-MM-dd");
                    //me_expd.REPLY_DATE = DateTime.Parse(me_expd.REPLY_DATE).ToString("yyyy-MM-dd");
                    //me_expd.REPLY_ID = User.Identity.Name;
                    //me_expd.UPDATE_ID = User.Identity.Name;
                    //me_expd.IP = DBWork.ProcIP;
                    foreach (ME_EXPD expd in expds) {
                        session.Result.afrs = repo.Delete(expd);
                    }
                    //session.Result.etts = repo.Get(mi_wexpinv.WH_NO, mi_wexpinv.MMCODE, mi_wexpinv.LOT_NO, mi_wexpinv.EXP_DATE);

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
        public ApiResponse Transfer(ME_EXPD me_expd)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0052Repository(DBWork);

                    //IEnumerable<ME_EXPD> expds = repo.GetAllExpds(me_expd.WH_NO, me_expd.EXP_DATE);

                    // 存在於ME_EXPM者更新
                    session.Result.afrs = repo.UpdateExpm(me_expd.WH_NO, me_expd.EXP_DATE, DBWork.UserInfo.UserId, DBWork.ProcIP);
                    // 不存在於ME_EXPM者新增
                    session.Result.afrs = repo.InsertExpm(me_expd.WH_NO, me_expd.EXP_DATE, DBWork.UserInfo.UserId, DBWork.ProcIP);

                    // 若回報藥量為0, 則刪除該筆 1110.10.07mark
                    //session.Result.afrs = repo.DeleteExpd0(me_expd.WH_NO, me_expd.EXP_DATE);

                    me_expd.UPDATE_ID = User.Identity.Name;
                    me_expd.IP = DBWork.ProcIP;
                    session.Result.afrs = repo.Transfer(me_expd);
                    //session.Result.etts = repo.Get(mi_wexpinv.WH_NO, mi_wexpinv.MMCODE, mi_wexpinv.LOT_NO, mi_wexpinv.EXP_DATE);

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
        public ApiResponse GetWexpinvsN(FormDataCollection form)
        {
            var wh_no = form.Get("wh_no");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0052Repository(DBWork);
                    // 2022-03-02：新增 載入前刪除該庫房過往未傳送資料(exp_stat = 1)
                    session.Result.afrs = repo.DeleteExpStat1(wh_no);

                    session.Result.afrs = repo.GetMiWexpinvsN(wh_no, DBWork.UserInfo.UserId, DBWork.ProcIP);

                    DBWork.Commit();

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        #region combox
        public ApiResponse GetWhnoCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0052Repository(DBWork);
                    session.Result.etts = repo.GetWhnoCombo(User.Identity.Name);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }


        [HttpPost]
        public ApiResponse GetExpDates(FormDataCollection form)
        {
            var mmcode = form.Get("mmcode");
            var wh_no = form.Get("wh_no");
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0052Repository(DBWork);
                    session.Result.etts = repo.GetExpDates(wh_no, mmcode);

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetLotNos(FormDataCollection form)
        {
            var mmcode = form.Get("mmcode");
            var wh_no = form.Get("wh_no");
            var exp_date = form.Get("exp_date");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0052Repository(DBWork);
                    session.Result.etts = repo.GetLotNos(wh_no, mmcode, exp_date);

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }



        [HttpPost]
        public ApiResponse GetExpQty(FormDataCollection form)
        {
            var mmcode = form.Get("mmcode");
            var wh_no = form.Get("wh_no");
            var lot_no = form.Get("lot_no");
            var exp_date = form.Get("exp_date");
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0052Repository(DBWork);
                    session.Result.etts = repo.GetExpQty(wh_no, mmcode, lot_no, exp_date);

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetExpdItems(FormDataCollection form)
        {
            var mmcode = form.Get("mmcode");
            var wh_no = form.Get("wh_no");
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0052Repository(DBWork);

                    IEnumerable<AB0052Repository.ExpdItem> items = repo.GetExpdCombos(wh_no, mmcode);
                    foreach (AB0052Repository.ExpdItem item in items)
                    {
                        item.ExpDateItems = repo.GetExpDateItems(wh_no, mmcode, item.LOT_NO);
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
        public ApiResponse GetMMCODECombo(FormDataCollection form)
        {
            var p0 = form.Get("p0"); //動態mmcode
            var wh_no = form.Get("wh_no");
            var page = int.Parse(form.Get("page"));

            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0052Repository repo = new AB0052Repository(DBWork);
                    session.Result.etts = repo.GetMMCODECombo(p0, wh_no, page, limit, "");
                }
                catch(Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

        #endregion

        #region 匯入匯出 2019-12-03新增
        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            var wh_no = form.Get("wh_no");
            var exp_date = form.Get("exp_date");
            var status = form.Get("status");
            var fileName = form.Get("fileName");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0052Repository repo = new AB0052Repository(DBWork);

                    DataTable dtItems = new DataTable();
                    dtItems.Columns.Add("項次", typeof(int));
                    dtItems.Columns["項次"].AutoIncrement = true;
                    dtItems.Columns["項次"].AutoIncrementSeed = 1;
                    dtItems.Columns["項次"].AutoIncrementStep = 1;

                    DataTable result = null;

                    result = repo.GetExcel(wh_no, exp_date, status);


                    dtItems.Merge(result);

                    JCLib.Excel.Export(fileName, dtItems);

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
        public ApiResponse ExcelFail(FormDataCollection form)
        {
            var wh_no = form.Get("wh_no");
            var exp_date = form.Get("exp_date");
            exp_date = (Convert.ToInt32(exp_date.Substring(0, 3)) + 1911) + "-" + exp_date.Substring(3, 2) + "-" + exp_date.Substring(5, 2);
            var status = form.Get("status");
            string[] mmcodeData = form.Get("mmcode").ToString().Trim().Split('^');
            string[] reply_dateData = form.Get("reply_date").ToString().Trim().Split('^');
            string[] lot_noData = form.Get("lot_no").ToString().Trim().Split('^');
            string[] exp_qtyData = form.Get("exp_qty").ToString().Trim().Split('^');
            string[] reply_timeData = form.Get("reply_time").ToString().Trim().Split('^');
            string[] ufrData = form.Get("ufr").ToString().Trim().Split('^');
            var fileName = form.Get("fileName");
            
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0052Repository repo = new AB0052Repository(DBWork);

                    DataTable dtItems = new DataTable();
                    dtItems.Columns.Add("項次", typeof(int));
                    dtItems.Columns["項次"].AutoIncrement = true;
                    dtItems.Columns["項次"].AutoIncrementSeed = 1;
                    dtItems.Columns["項次"].AutoIncrementStep = 1;

                    DataTable result = null;

                    result = repo.GetExcel(wh_no, exp_date, status);


                    dtItems.Merge(result);

                    // 填入錯誤說明和原回覆藥量
                    dtItems.Columns.Add("錯誤說明", typeof(string));
                    string dateNow = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.Month.ToString().PadLeft(2, '0') + "01";
                    for (int i = 0; i < mmcodeData.Length; i++)
                    {
                            
                        if (dtItems.Select("院內碼='" + mmcodeData[i] + "' " +
                            "and 回覆效期 = '" + reply_dateData[i] + "' " +
                            "and 藥品批號 = '" + lot_noData[i] + "'").Length > 0)
                        {
                            dtItems.Select("院內碼='" + mmcodeData[i] + "' " +
                            "and 回覆效期 = '" + reply_dateData[i] + "' " +
                            "and 藥品批號 = '" + lot_noData[i] + "'")[0]["錯誤說明"] = ufrData[i];
                        }
                        else
                        {
                            // 在ME_EXPD找不到對應的品項
                            string extraMsg = "";
                            DataRow newRow = dtItems.NewRow();
                            // 輸入資料長度有可能比欄位長度大,像是院內碼或藥品批號,有可能被EXCEL轉為科學符號
                            if (mmcodeData[i].Length > dtItems.Columns["院內碼"].MaxLength)
                            {
                                extraMsg = ";院內碼:" + mmcodeData[i] + "資料長度大於欄位長度" + dtItems.Columns["院內碼"].MaxLength;
                                mmcodeData[i] = mmcodeData[i].Substring(0, dtItems.Columns["院內碼"].MaxLength);
                            }
                            if (lot_noData[i].Length > dtItems.Columns["藥品批號"].MaxLength)
                            {
                                extraMsg = ";藥品批號:" + lot_noData[i] + "資料長度大於欄位長度" + dtItems.Columns["藥品批號"].MaxLength;
                                lot_noData[i] = lot_noData[i].Substring(0, dtItems.Columns["藥品批號"].MaxLength);
                            }
                                
                            newRow["庫房代碼"] = wh_no;
                            newRow["院內碼"] = mmcodeData[i];
                            newRow["英文品名"] = "";
                            newRow["回覆效期"] = reply_dateData[i];
                            newRow["藥品批號"] = lot_noData[i];
                            newRow["回覆藥量"] = "0";
                            newRow["庫存量"] = "0";
                            newRow["藥庫儲位"] = "";
                            newRow["回報狀態"] = "未回報";
                            newRow["回覆日期"] = reply_timeData[i];
                            newRow["回覆月份"] = dateNow;
                            newRow["錯誤說明"] = ufrData[i] + extraMsg;
                            dtItems.Rows.Add(newRow);
                        }
                            
                    }
                    
                    JCLib.Excel.Export(fileName, dtItems);

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
        public ApiResponse Import()
        {
            using (WorkSession session = new WorkSession(this))
            {

                List<ME_EXPD> list = new List<ME_EXPD>();
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0052Repository repo = new AB0052Repository(DBWork);

                    List<HeaderItem> headerItems = new List<HeaderItem>() {
                        new HeaderItem("項次", "UPLOAD_ROW_NUMBER"),
                        new HeaderItem("庫房代碼", "WH_NO"),
                        new HeaderItem("院內碼", "MMCODE"),
                        new HeaderItem("英文品名", "MMNAME_E"),
                        new HeaderItem("回覆效期", "REPLY_DATE"),
                        new HeaderItem("藥品批號", "LOT_NO"),
                        new HeaderItem("回覆藥量","EXP_QTY"),
                        new HeaderItem("庫存量","INV_QTY"),
                        new HeaderItem("藥庫儲位", "STORE_LOC"),
                        new HeaderItem("回報狀態", "EXP_STAT"),
                        new HeaderItem("備註", "MEMO"),
                        new HeaderItem("回覆日期", "REPLY_TIME"),
                        new HeaderItem("回覆月份", "EXP_DATE")
                    };

                    IWorkbook workBook;
                    var HttpPostedFile = HttpContext.Current.Request.Files["file"];

                    if (Path.GetExtension(HttpPostedFile.FileName).ToLower() == ".xls")
                    {
                        workBook = new HSSFWorkbook(HttpPostedFile.InputStream);
                    }
                    else
                    {
                        workBook = new XSSFWorkbook(HttpPostedFile.InputStream);
                    }
                    var sheet = workBook.GetSheetAt(0);
                    IRow headerRow = sheet.GetRow(0);//由第一列取標題做為欄位名稱
                    int cellCount = headerRow.LastCellNum;

                    headerItems = SetHeaderIndex(headerItems, headerRow);

                    for (int i = 1; i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row is null) { continue; }

                        ME_EXPD expd = new ME_EXPD();
                        foreach (HeaderItem item in headerItems)
                        {
                            string value = row.GetCell(item.Index) == null ? string.Empty : row.GetCell(item.Index).ToString();
                            expd.GetType().GetProperty(item.FieldName).SetValue(expd, value, null);
                        }
                        list.Add(expd);
                    }

                    string msg = string.Empty;
                    foreach (HeaderItem header in headerItems)
                    {
                        if (header.Index < 0)
                        {
                            if (msg != string.Empty)
                            {
                                msg += "、";
                            }
                            msg += header.Name;
                        }
                    }
                    if (msg != string.Empty)
                    {
                        session.Result.msg = string.Format("上傳格式錯誤，缺少欄位：{0}", msg);
                        session.Result.success = false;
                        return session.Result;
                    }


                    int updateResult = -1;
                    List<ME_EXPD> failList = new List<ME_EXPD>();
                    foreach (ME_EXPD item in list) {

                        ME_EXPD existing = repo.GetExistingExpd(item.WH_NO, item.MMCODE, item.LOT_NO, item.EXP_DATE, item.REPLY_DATE);
                        if (existing == null) {
                            item.UPLOAD_FAIL_REASON = "項目不存在";
                            failList.Add(item);
                            continue;
                        }
                        
                        if (item.EXP_QTY.Trim() == string.Empty )
                        {
                            item.UPLOAD_FAIL_REASON = "回覆藥量無數值";
                            failList.Add(item);
                            continue;
                        }
                        float f = 0;
                        if (float.TryParse(item.EXP_QTY.Trim(), out f) == false)
                        {
                            item.UPLOAD_FAIL_REASON = "回覆藥量非數值";
                            failList.Add(item);
                            continue;
                        }
                        if (float.Parse(item.EXP_QTY) < 0) {
                            item.UPLOAD_FAIL_REASON = "回覆藥量為負值";
                            failList.Add(item);
                            continue;
                        }
                        
                        if (item.EXP_STAT.Trim() == "已回報")
                        {
                            item.UPLOAD_FAIL_REASON = "項目已回報";
                            failList.Add(item);
                            continue;
                        }

                        updateResult = repo.UpdateExpdExpqty(item.WH_NO, item.MMCODE, item.EXP_DATE, item.LOT_NO, item.EXP_QTY,
                                                             item.MEMO, item.REPLY_DATE, DBWork.UserInfo.UserId, DBWork.ProcIP);
                        if (updateResult == 0) {
                            item.UPLOAD_FAIL_REASON = "上傳失敗";
                            failList.Add(item);
                            continue;
                        }
                        session.Result.afrs += 1;
                    }

                    session.Result.etts = failList;

                    if (failList.Count() == 0)
                        DBWork.Commit(); // 沒有錯誤才commit
                    else
                        DBWork.Rollback(); // 有任何錯誤則本次上傳不做資料異動
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }
        public class HeaderItem
        {
            public string Name { get; set; }
            public string FieldName { get; set; }
            public int Index { get; set; }
            public HeaderItem()
            {
                Name = string.Empty;
                Index = -1;
                FieldName = string.Empty;
            }
            public HeaderItem(string name, int index, string fieldName)
            {
                Name = name;
                Index = index;
                FieldName = fieldName;
            }
            public HeaderItem(string name, string fieldName)
            {
                Name = name;
                Index = -1;
                FieldName = fieldName;
            }
        }
        public List<HeaderItem> SetHeaderIndex(List<HeaderItem> list, IRow headerRow)
        {
            int cellCounts = headerRow.LastCellNum;
            for (int i = 0; i < cellCounts; i++)
            {
                foreach (HeaderItem item in list)
                {
                    if (headerRow.GetCell(i).ToString() == item.Name)
                    {
                        item.Index = i;
                    }
                }
            }

            return list;
        }

        #endregion

        #region 2020-07-08 新增 匯出最近一筆效期
        [HttpPost]
        public ApiResponse ExcelSingle(FormDataCollection form)
        {
            var wh_no = form.Get("wh_no");
            var exp_date = form.Get("exp_date");
            var status = form.Get("status");
            var fileName = form.Get("fileName");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0052Repository repo = new AB0052Repository(DBWork);

                    DataTable dtItems = new DataTable();
                    dtItems.Columns.Add("項次", typeof(int));
                    dtItems.Columns["項次"].AutoIncrement = true;
                    dtItems.Columns["項次"].AutoIncrementSeed = 1;
                    dtItems.Columns["項次"].AutoIncrementStep = 1;

                    DataTable result = null;

                    result = repo.GetExcelSingle(wh_no, exp_date, status);


                    dtItems.Merge(result);

                    JCLib.Excel.Export(fileName, dtItems);

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