using JCLib.DB;
using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using WebApp.Models;
using WebApp.Repository.G;

namespace WebApp.Controllers.G
{
    public class GA0001Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var inv_qty = form.Get("p0");
            var mmname = form.Get("p2");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new GA0001Repository(DBWork);
                    session.Result.etts = repo.GetAll(inv_qty, mmname, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        #region 計算平均月消耗量
        [HttpPost]
        public ApiResponse Calculate()
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new GA0001Repository(DBWork);
                    //session.Result.etts = repo.GetStatusCombo();
                    string ym = GetYM(DateTime.Now);
                    List<string> ymList = GetYMs(DateTime.Now);

                    IEnumerable<TC_USEQMTR> all = repo.GetUseqmtrs(ymList);

                    IEnumerable<TC_INVCTL> invctls = GetINVCTs(all, DBWork.UserInfo.UserId, DBWork.ProcIP);

                    // 刪除所有資料
                    session.Result.afrs = repo.DeleteInvctl();

                    foreach (TC_INVCTL invctl in invctls)
                    {
                        session.Result.afrs = repo.InsertInvctl(invctl);
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

        public string GetYM(DateTime oriTime)
        {
            int year = oriTime.Year - 1911;
            int month = oriTime.Month;
            string monthString = month > 9 ? month.ToString() : month.ToString().PadLeft(2, '0');
            return string.Format("{0}{1}", year.ToString(), monthString);
        }

        public List<string> GetYMs(DateTime now)
        {
            List<string> list = new List<string>();
            for (int i = 1; i < 7; i++)
            {
                DateTime temp = now.AddMonths(-i);
                list.Add(GetYM(temp));
            }
            return list;
        }

        public IEnumerable<TC_INVCTL> GetINVCTs(IEnumerable<TC_USEQMTR> all, string userid, string ip)
        {
            var o = from a in all
                    orderby a.DATA_YM descending
                    group a by new
                    {
                        MMCODE = a.MMCODE,
                        MMNAME_C = a.MMNAME_C,
                        BASE_UNIT = a.BASE_UNIT
                    } into g
                    select new TC_USEQMTR
                    {
                        MMCODE = g.Key.MMCODE,
                        MMNAME_C = g.Key.MMNAME_C,
                        BASE_UNIT = g.Key.BASE_UNIT,
                        ITEMS = g.ToList()
                    };
            List<TC_USEQMTR> tempList = o.ToList();
            List<TC_INVCTL> list = new List<TC_INVCTL>();
            foreach (TC_USEQMTR temp in tempList)
            {
                TC_INVCTL item = new TC_INVCTL();
                item.MMCODE = temp.MMCODE;
                item.MMNAME_C = temp.MMNAME_C;
                item.BASE_UNIT = temp.BASE_UNIT;
                item.M6AVG_USEQTY = GetAvg(temp.ITEMS);//Math.Round(temp.ITEMS.Average(x => float.Parse(x.MUSE_QTY)), 2).ToString(); //GetAvg(temp.ITEMS);
                item.M3AVG_USEQTY = GetAvg(temp.ITEMS.Take(3));//Math.Round(temp.ITEMS.Take(3).Average(x => float.Parse(x.MUSE_QTY)), 2).ToString();
                item.M6MAX_USEQTY = GetMax(temp.ITEMS);//temp.ITEMS.Max(x => float.Parse(x.MUSE_QTY)).ToString();
                item.M3MAX_USEQTY = GetMax(temp.ITEMS.Take(3));//temp.ITEMS.Take(3).Max(x => float.Parse(x.MUSE_QTY)).ToString();
                item.CREATE_USER = userid;
                item.UPDATE_USER = userid;
                item.UPDATE_IP = ip;

                list.Add(item);
            }
            return list;
        }

        public string GetAvg(IEnumerable<TC_USEQMTR> list)
        {
            float f = 0;
            foreach (TC_USEQMTR item in list)
            {
                f += float.Parse(item.MUSE_QTY);
            }
            return Math.Round((f / list.Count()), 2).ToString();
        }
        public string GetMax(IEnumerable<TC_USEQMTR> list)
        {
            float f = 0;
            foreach (TC_USEQMTR item in list)
            {
                if (f < float.Parse(item.MUSE_QTY))
                {
                    f = float.Parse(item.MUSE_QTY);
                }
            }
            return f.ToString();
        }

        #endregion

        #region 匯入
        [HttpPost]
        public ApiResponse Import()
        {
            using (WorkSession session = new WorkSession(this))
            {

                List<TC_INVQMTR> list = new List<TC_INVQMTR>();
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    GA0001Repository repo = new GA0001Repository(DBWork);

                    List<HeaderItem> headerItems = new List<HeaderItem>() {
                       new HeaderItem("年月", "DATA_YM"),
                        new HeaderItem("電腦編號", "MMCODE"),
                        new HeaderItem("藥品名稱", "MMNAME_C"),
                        new HeaderItem("單位", "BASE_UNIT"),
                        new HeaderItem("進價", "IN_PRICE"),
                       new HeaderItem("上月庫存","PMN_INVQTY"),
                        new HeaderItem("本月進貨","MN_INQTY"),
                        new HeaderItem("本月消耗", "MN_USEQTY"),
                        new HeaderItem("本月庫存", "MN_INVQTY"),
                        new HeaderItem("存放位置", "STORE_LOC")
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

                        TC_INVQMTR invqmtr = new TC_INVQMTR();
                        foreach (HeaderItem item in headerItems) {
                            string value = row.GetCell(item.Index) == null ? string.Empty : row.GetCell(item.Index).ToString();
                            invqmtr.GetType().GetProperty(item.FieldName).SetValue(invqmtr, value, null);
                        }
                        list.Add(invqmtr);
                    }

                    // 刪除TC_INVQMTR
                    session.Result.afrs = repo.DeleteInvqmtr();

                    session.Result.afrs = CalculateRCM(list, DBWork);

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

        public int CalculateRCM(IEnumerable<TC_INVQMTR> list, UnitOfWork DBWork) {
            try
            {
                int afrs = 0;
                GA0001Repository repo = new GA0001Repository(DBWork);

                foreach (TC_INVQMTR item in list)
                {
                    item.DATA_YM = item.DATA_YM.Replace("/", string.Empty);

                    TC_INVCTL invctl = repo.GetInvctl(item.DATA_YM, item.MMCODE);
                    if (invctl != null)
                    {
                        item.M6AVG_USEQTY = invctl.M6AVG_USEQTY;
                        item.M3AVG_USEQTY = invctl.M3AVG_USEQTY;
                        item.M6MAX_USEQTY = invctl.M6MAX_USEQTY;
                        item.M3MAX_USEQTY = invctl.M3MAX_USEQTY;
                    }
                    else
                    {
                        item.M6AVG_USEQTY = "0";
                        item.M3AVG_USEQTY = "0";
                        item.M6MAX_USEQTY = "0";
                        item.M3MAX_USEQTY = "0";
                    }

                    item.AGEN_NAMEC = null;
                    item.PUR_UNIT = null;
                    item.IN_PURPRICE = null;
                    TC_MMAGEN mmagen = repo.GetMmagen(item.MMCODE);
                    if (mmagen != null)
                    {
                        item.AGEN_NAMEC = mmagen.AGEN_NAMEC;
                        item.PUR_UNIT = mmagen.PUR_UNIT;
                        item.IN_PURPRICE = mmagen.IN_PURPRICE.ToString();
                    }

                    // 庫存量天數 = 四捨五入[(本月庫存/前6個月平均消耗量)*30,取整數]
                    item.INV_DAY = item.M6AVG_USEQTY == "0" ? "0" : Math.Round((float.Parse(item.MN_INVQTY) / float.Parse(item.M6AVG_USEQTY)) * 30).ToString();
                    // 應訂購量 = 前6個月平均消耗量-本月庫存
                    item.EXP_PURQTY = Math.Round(float.Parse(item.M6AVG_USEQTY) - float.Parse(item.MN_INVQTY), 2).ToString();

                    if (item.BASE_UNIT != null && item.BASE_UNIT.Trim() != string.Empty)
                    {
                        if (item.BASE_UNIT != item.PUR_UNIT)
                        {
                            TC_PURUNCOV puruncov = repo.GetPuruncov(item.BASE_UNIT, item.PUR_UNIT);
                            if (puruncov != null)
                            {
                                item.BASEUN_MULTI = puruncov.BASEUN_MULTI.ToString();
                                item.PURUN_MULTI = puruncov.PURUN_MULTI.ToString();
                            }
                            else
                            {
                                item.BASEUN_MULTI = "0";
                                item.PURUN_MULTI = "0";
                            }
                        }
                        else
                        {
                            item.BASEUN_MULTI = "1";
                            item.PURUN_MULTI = "1";
                        }
                        // 建議訂購量 = (應訂購量/計量單位乘數)*單位劑量乘數，無條件進位。若 計量單位 無值，則 建議訂購量=1
                        if (item.BASEUN_MULTI != "0") {
                            item.RCM_PURQTY = Math.Ceiling((float.Parse(item.EXP_PURQTY) / float.Parse(item.BASEUN_MULTI)) * float.Parse(item.PURUN_MULTI)).ToString();
                        }
                        else
                        {
                            item.BASEUN_MULTI = "0";
                            item.PURUN_MULTI = "0";
                            item.RCM_PURQTY = "";
                        }
                    }
                    else
                    {
                        item.RCM_PURQTY = "";
                    }
                    // 建議訂購天數 = (建議訂購量/前6個月平均消耗量)*30，取整數
                    if (item.M6AVG_USEQTY == "0" || item.RCM_PURQTY == "")
                    {
                        item.RCM_PURDAY = "";
                    }
                    else
                    {
                        item.RCM_PURDAY = Math.Round(float.Parse(item.RCM_PURQTY) / float.Parse(item.M6AVG_USEQTY) * 30).ToString();
                    }


                    item.CREATE_USER = DBWork.UserInfo.UserId;
                    item.UPDATE_USER = DBWork.UserInfo.UserId;
                    item.UPDATE_IP = DBWork.ProcIP;

                    if (item.BASEUN_MULTI == "0") {
                        item.BASEUN_MULTI = "";
                    }

                    if (item.PURUN_MULTI == "0")
                    {
                        item.PURUN_MULTI = "";
                    }

                    afrs = repo.InsertInvqmtr(item);

                    TC_MAST mast = repo.GetTcMast(item.MMCODE);
                    if (mast == null)
                    {
                        TC_MAST newMast = new TC_MAST(item.MMCODE, item.MMNAME_C, item.CREATE_USER, item.UPDATE_IP);

                        afrs = repo.InsertMast(newMast);
                    }
                    else
                    {
                        if (item.MMNAME_C.Trim() != mast.MMNAME_C.Trim() && item.MMNAME_C.Trim() != string.Empty)
                        {
                            mast.MMNAME_C = item.MMNAME_C.Trim();
                            mast.UPDATE_USER = item.UPDATE_USER;
                            mast.UPDATE_IP = item.UPDATE_IP;
                            afrs = repo.UpdateMast(mast);
                        }
                    }
                }
                return afrs;
            }
            catch
            {
                DBWork.Rollback();
                throw;
            }
        }

        #endregion

        #region 訂購/取消訂購
        [HttpPost]
        public ApiResponse Set(TC_INVQMTR invqmtrItem) {
            IEnumerable<TC_INVQMTR> invqmtrs = JsonConvert.DeserializeObject<IEnumerable<TC_INVQMTR>>(invqmtrItem.ITEM_STRING);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new GA0001Repository(DBWork);

                    foreach (TC_INVQMTR item in invqmtrs)
                    {
                        item.UPDATE_USER = DBWork.UserInfo.UserId;
                        item.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.UpdateInvqmtr(item);
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

        #region combo
        [HttpGet]
        public ApiResponse StatusCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new GA0001Repository(DBWork);
                    session.Result.etts = repo.GetStatusCombo();
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