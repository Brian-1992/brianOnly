using JCLib.DB;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using WebApp.Models;
using WebApp.Repository.F;

namespace WebApp.Controllers.F
{
    public class FA0060Controller : SiteBase.BaseApiController
    {
        #region temp class
        public class Quarter
        {
            public int min { get; set; }
            public int max { get; set; }
            public Quarter(int a, int b)
            {
                min = a;
                max = b;
            }
        }
        #endregion

        #region parameters
        //IEnumerable<Quarter> quarters = new List<Quarter>() {
        //    new Quarter(1,3),new Quarter(4,6),new Quarter(7,9),new Quarter(10,12)
        //};
        IEnumerable<string> columnItems = new List<string>() {
            "消耗金額<br/>(C)", "差異金額<br/>D=(C-A)", "成長率<br/>(D/A)", "達標否"
        };
        Dictionary<string, string> properties = new Dictionary<string, string>() {
            {"_AMOUNT", "ABS_AMOUNT" }, {"_AMOUNT_DIFF", "ABS_AMOUNT_DIFF" },
            {"_AMOUNT_RATE", "ABS_AMOUNT_RATE" },{"_REACH", "REACH" }
        };
        Dictionary<string, string> columnPrpoerties = new Dictionary<string, string>() {
            {"消耗金額<br/>(C)", "_AMOUNT"}, {"差異金額<br/>D=(C-A)", "_AMOUNT_DIFF"},
            {"成長率<br/>(D/A)", "_AMOUNT_RATE"},{"達標否", "_REACH"}
        };
        #endregion

        [HttpGet]
        public ApiResponse GetPreym()
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0060Repository(DBWork);
                    session.Result.msg = repo.GetPreym();

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        #region 轉入上月資料
        [HttpPost]
        public ApiResponse Transfer(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new FA0060Repository(DBWork);

                    // 先轉資料再查詢
                    // 刪除上個月MM_ABS_FEE資料，再轉入
                    session.Result.afrs = repo.DeleteAbsFee();
                    session.Result.afrs = repo.insertAbsFee();

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

        #region
        public ApiResponse Columns(FormDataCollection form)
        {
            var data_ym = form.Get("data_ym");
            var grp_no = form.Get("grp_no");
            var inid = form.Get("inid");
            var rate = form.Get("rate");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new FA0060Repository(DBWork);

                    FA0060DataYmItem ymItem = repo.GetDataYmItem();

                    IEnumerable<FA0060> items = repo.GetPastData(ymItem.PreMinDataym, ymItem.PreMaxDataym, grp_no, inid, rate);
                    IEnumerable<MM_ABS_FEE> CurrentFees = repo.GetData(ymItem.MinDataym, ymItem.MaxDataym, grp_no, inid);

                    IEnumerable<string> currentYms = GetCurrentYms(CurrentFees);

                    session.Result.etts = GetColumns(ymItem, currentYms);

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

        #region 分析表內容
        [HttpPost]
        public ApiResponse Data(FormDataCollection form)
        {
            var data_ym = form.Get("data_ym");
            var grp_no = form.Get("grp_no");
            var inid = form.Get("inid");
            var rate = form.Get("rate");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new FA0060Repository(DBWork);

                    FA0060DataYmItem ymItem = repo.GetDataYmItem();

                    IEnumerable<FA0060> items = repo.GetPastData(ymItem.PreMinDataym, ymItem.PreMaxDataym, grp_no, inid, rate);
                    IEnumerable<MM_ABS_FEE> CurrentFees = repo.GetData(ymItem.MinDataym, ymItem.MaxDataym, grp_no, inid);
                    items = GetFa0060(items, CurrentFees, ymItem, data_ym);
                    // Transform
                    session.Result.etts = Transform(items);

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

        public IEnumerable<FA0060> GetFa0060(IEnumerable<FA0060> items, IEnumerable<MM_ABS_FEE> fees, FA0060DataYmItem ymItem, string data_ym)
        {

            IEnumerable<string> currentYms = GetCurrentYms(fees);

            foreach (FA0060 item in items)
            {
                item.ITEMS = GetFees(fees, item.INID, item.TARGET, item.AVG_CONSUME_AMOUNT);
                item.COLUMNS = GetColumns(ymItem, currentYms);
            }


            return items;
        }
        public IEnumerable<string> GetCurrentYms(IEnumerable<MM_ABS_FEE> fees)
        {
            var o = (from a in fees
                     orderby a.DATA_YM ascending
                     select a.DATA_YM).Distinct();
            return o.ToList<string>();
        }
        public IEnumerable<FA0060Item> GetFees(IEnumerable<MM_ABS_FEE> fees, string inid, string target, string avg)
        {
            var o = from a in fees
                    where a.INID == inid
                    orderby a.DATA_YM ascending
                    select new FA0060Item()
                    {
                        ABS_YM = a.DATA_YM,
                        ABS_AMOUNT = Math.Round(double.Parse(a.ABS_AMOUNT)).ToString(),
                        ABS_AMOUNT_DIFF = GetDiff(a.ABS_AMOUNT, avg),
                        ABS_AMOUNT_RATE = GetRate(a.ABS_AMOUNT, target, avg),
                        REACH = GetReach(a.ABS_AMOUNT, target, avg)
                    };
            List<FA0060Item> temps = o.ToList<FA0060Item>();
            return temps;
        }
        public string GetDiff(string amount, string avg)
        {
            return Math.Round((double.Parse(amount) - double.Parse(avg))).ToString();
        }
        public string GetRate(string amount, string target, string avg)
        {
            if (avg == "0")
            {
                return "0%";
            }
            return string.Format("{0}%", Math.Round(((double.Parse(amount) - double.Parse(avg)) / double.Parse(avg)), 4) * 100);
        }
        public string GetReach(string amount, string target, string avg)
        {

            double rate = 0;
            if (avg == "0")
            {
                rate = 0;
            }
            else
            {
                rate = Math.Round((double.Parse(amount) - double.Parse(avg)) / double.Parse(avg), 4) * 100;
            }

            if (rate == 0)
            {
                if (avg == "0") return string.Empty;
                return "達標";
            }
            if (rate > 0)
            {
                return "<span style='color: red'>未達標</span>";
            }
            return "達標";


            //return ((double.Parse(amount) - double.Parse(target)) < 0) ? "達標" : "未達標";
            //return double.Parse(rate.Substring(0, rate.Length - 1)) > 0 ? "達標" : double.Parse(rate.Substring(0,rate.Length-1)) < 0 ? "未達標" : string.Empty;
        }
        public IEnumerable<ColumnItem> GetColumns(FA0060DataYmItem ymItem, IEnumerable<string> currentYms)
        {
            List<ColumnItem> columns = new List<ColumnItem>();

            columns.Add(getColumnItem(string.Format("{0}Q{1}平均消耗金額<br/>(A)", ymItem.Pre_y, ymItem.Q), "AVG_CONSUME_AMOUNT"));
            columns.Add(getColumnItem(string.Format("{0}Q{1}目標值<br/>(B)", ymItem.Y, ymItem.Q), "TARGET"));
            int i = 0;
            foreach (string ym in currentYms)
            {
                i++;
                foreach (string item in columnItems)
                {
                    foreach (KeyValuePair<string, string> property in columnPrpoerties)
                    {
                        if (item == property.Key)
                        {
                            string prop = string.Format("M{0}{1}", i, property.Value);
                            columns.Add(getColumnItem(string.Format("{0}{1}", ym, item), prop));
                        }

                    }
                }
            }
            return columns;
        }
        public ColumnItem getColumnItem(string text, string dataIndex)
        {
            return new ColumnItem()
            {
                TEXT = text,
                DATAINDEX = dataIndex
            };
        }
        public IEnumerable<FA0060> Transform(IEnumerable<FA0060> items)
        {
            int i = 0;
            foreach (FA0060 item in items)
            {
                i = 0;
                foreach (FA0060Item subItem in item.ITEMS)
                {
                    i++;
                    foreach (KeyValuePair<string, string> property in properties)
                    {

                        string prop = string.Format("M{0}{1}", i, property.Key);
                        string value = subItem.GetType().GetProperty(property.Value).GetValue(subItem).ToString();
                        item.GetType().GetProperty(prop).SetValue(item, value, null);
                    }
                }
            }

            return items;
        }
        #endregion

        #region 分析表下載
        [HttpPost]
        public ApiResponse T1Excel(FormDataCollection form)
        {
            var data_ym = form.Get("data_ym");
            var grp_no = form.Get("grp_no");
            var inid = form.Get("inid");
            var rate = form.Get("rate");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    FA0060Repository repo = new FA0060Repository(DBWork);

                    FA0060DataYmItem ymItem = repo.GetDataYmItem();

                    DataTable dtItems = new DataTable();
                    dtItems.Columns.Add("項次", typeof(int));
                    dtItems.Columns["項次"].AutoIncrement = true;
                    dtItems.Columns["項次"].AutoIncrementSeed = 1;
                    dtItems.Columns["項次"].AutoIncrementStep = 1;

                    DataTable result = null;

                    result = repo.GetT1Excel(data_ym, grp_no, inid, rate, ymItem);


                    dtItems.Merge(result);

                    JCLib.Excel.Export(form.Get("FN"), dtItems);

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

        #region 明細內容
        [HttpPost]
        public ApiResponse Details(FormDataCollection form)
        {
            var data_ym_start = form.Get("data_ym_start");
            var data_ym_end = form.Get("data_ym_end");
            var grp_no = form.Get("grp_no");
            var inid = form.Get("inid");
            var mmcode = form.Get("mmcode");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new FA0060Repository(DBWork);

                    session.Result.etts = repo.GetDetails(data_ym_start, data_ym_end, grp_no, inid, mmcode);

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

        #region 明細下載
        [HttpPost]
        public ApiResponse T2Excel(FormDataCollection form)
        {
            var data_ym_start = form.Get("data_ym_start");
            var data_ym_end = form.Get("data_ym_end");
            var grp_no = form.Get("grp_no");
            var inid = form.Get("inid");
            var mmcode = form.Get("mmcode");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    FA0060Repository repo = new FA0060Repository(DBWork);

                    DataTable dtItems = new DataTable();
                    dtItems.Columns.Add("項次", typeof(int));
                    dtItems.Columns["項次"].AutoIncrement = true;
                    dtItems.Columns["項次"].AutoIncrementSeed = 1;
                    dtItems.Columns["項次"].AutoIncrementStep = 1;

                    DataTable result = null;

                    result = repo.GetT2Excel(data_ym_start, data_ym_end, grp_no, inid, mmcode);


                    dtItems.Merge(result);

                    JCLib.Excel.Export(string.Format("{0}-{1}_{2}",
                    data_ym_start, data_ym_end, form.Get("FN")), dtItems);

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

        #region combos

        public ApiResponse Grpnos(FormDataCollection form)
        {

            string queryString = form.Get("queryString");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0060Repository(DBWork);
                    session.Result.etts = repo.GetGrpNos(queryString);

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse Inids(FormDataCollection form)
        {
            string queryString = form.Get("queryString");
            string grp_no = form.Get("p1");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0060Repository(DBWork);
                    session.Result.etts = repo.GetInids(queryString, grp_no);

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMMCodeCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0060Repository repo = new FA0060Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeCombo(p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        #endregion

        #region T3
        [HttpPost]
        public ApiResponse GetT3Columns(FormDataCollection form)
        {
            string year = form.Get("year");
            string q = form.Get("q");
            string grp_no = form.Get("grp_no");
            string inid = form.Get("inid");
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0060Repository(DBWork);

                    FA0060DataYmItem ymItem = repo.GetT3DataYmItem(year, q);


                    //IEnumerable<FA0060> items = repo.GetT3PastData(ymItem.C_MIN_QYM, ymItem.C_MAX_QYM, grp_no, inid, false);
                    //IEnumerable<FA0060> pyItems = repo.GetT3PastData(ymItem.PRE_Y_MIN_QYM, ymItem.PRE_Y_MAX_QYM, grp_no, inid);
                    //IEnumerable<FA0060> psItems = repo.GetT3PastData(ymItem.PRE_S_MIN_QYM, ymItem.PRE_S_MAX_QYM, grp_no, inid);


                    IEnumerable<MM_ABS_FEE> CurrentFees = repo.GetData(ymItem.C_MIN_QYM, ymItem.C_MAX_QYM, grp_no, inid);
                    IEnumerable<MM_ABS_FEE> pyFees = repo.GetData(ymItem.PRE_Y_MIN_QYM, ymItem.PRE_Y_MAX_QYM, grp_no, inid);
                    IEnumerable<MM_ABS_FEE> psFees = repo.GetData(ymItem.PRE_S_MIN_QYM, ymItem.PRE_S_MAX_QYM, grp_no, inid);

                    IEnumerable<string> currentYms = GetCurrentYms(CurrentFees);
                    IEnumerable<string> pyYms = GetCurrentYms(pyFees);
                    IEnumerable<string> psYms = GetCurrentYms(psFees);

                    IEnumerable<ColumnItem> columns = GetT3Columns(ymItem, currentYms, "C");
                    IEnumerable<ColumnItem> pyColumns = GetT3Columns(ymItem, pyYms, "PRE_Y");
                    IEnumerable<ColumnItem> psColumns = GetT3Columns(ymItem, psYms, "PRE_S");

                    List<ColumnItem> list = new List<ColumnItem>();
                    list.AddRange(pyColumns.ToList());
                    list.AddRange(psColumns.ToList());
                    list.AddRange(columns.ToList());

                    pyColumns.ToList().AddRange(psColumns.ToList());
                    pyColumns.ToList().AddRange(columns.ToList());

                    session.Result.etts = list;
                }
                catch (Exception e) {
                    throw;
                }

                return session.Result;
            }

        }

        public IEnumerable<ColumnItem> GetT3Columns(FA0060DataYmItem ymItem, IEnumerable<string> currentYms, string ymNameCompare)
        {
            List<ColumnItem> columns = new List<ColumnItem>();

            var json = JsonConvert.SerializeObject(ymItem);
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            string y = string.Empty;
            string q = string.Empty;

            int i = 0;
            foreach (string ym in currentYms)
            {
                i++;
                columns.Add(getColumnItem(string.Format("{0}消耗金額", ym), string.Format("{0}_AMOUNT_{1}", ymNameCompare, i)));
                
            }

            foreach (KeyValuePair<string, string> property in dictionary)
            {
                if (string.Format("{0}_Y", ymNameCompare) == property.Key)
                {
                    y = property.Value;
                }
                if (string.Format("{0}_Q", ymNameCompare) == property.Key)
                {
                    q = property.Value;
                }
            }

            columns.Add(getColumnItem(string.Format("{0}Q{1}平均金額", y, q), string.Format("{0}_{1}", ymNameCompare, "AVG")));
            return columns;
        }


        [HttpPost]
        public ApiResponse GetT3Data(FormDataCollection form) {
            string year = form.Get("year");
            string q = form.Get("q");
            string grp_no = form.Get("grp_no");
            string inid = form.Get("inid");
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0060Repository(DBWork);

                    FA0060DataYmItem ymItem = repo.GetT3DataYmItem(year, q);
                    List<FA0060> list = new List<FA0060>();
                    // C
                    IEnumerable<FA0060> items = repo.GetT3PastData(ymItem.C_MIN_QYM, ymItem.C_MAX_QYM, grp_no, inid, true);
                    IEnumerable<MM_ABS_FEE> CurrentFees = repo.GetData(ymItem.C_MIN_QYM, ymItem.C_MAX_QYM, grp_no, inid);
                    list.AddRange( GetT3Fa0060(items, CurrentFees, ymItem, "C", list));

                    // PRE_Y
                    items = repo.GetT3PastData(ymItem.PRE_Y_MIN_QYM, ymItem.PRE_Y_MAX_QYM, grp_no, inid, true);
                    CurrentFees = repo.GetData(ymItem.PRE_Y_MIN_QYM, ymItem.PRE_Y_MAX_QYM, grp_no, inid);
                    list = GetT3Fa0060(items, CurrentFees, ymItem, "PRE_Y", list).ToList();
                    //list.AddRange(GetT3Fa0060(list, CurrentFees, ymItem, "PRE_Y"));

                    // PRE_S
                    items = repo.GetT3PastData(ymItem.PRE_S_MIN_QYM, ymItem.PRE_S_MAX_QYM, grp_no, inid, true);
                    CurrentFees = repo.GetData(ymItem.PRE_S_MIN_QYM, ymItem.PRE_S_MAX_QYM, grp_no, inid);
                    list = GetT3Fa0060(items, CurrentFees, ymItem, "PRE_S", list).ToList();
                    //list.AddRange(GetT3Fa0060(list, CurrentFees, ymItem, "PRE_S"));




                    // Transform
                    session.Result.etts = list;
                }
                catch (Exception e)
                {
                    throw;
                }

                return session.Result;
            }
        }
       
        public IEnumerable<FA0060> GetT3Fa0060(IEnumerable<FA0060> items, IEnumerable<MM_ABS_FEE> fees, FA0060DataYmItem ymItem, string columnPrefix, List<FA0060> targetList)
        {

            IEnumerable<string> currentYms = GetCurrentYms(fees);
            List<FA0060> list = new List<FA0060>();
            FA0060 newItem = new FA0060();

            foreach (FA0060 item in items)
            {
                // 平均金額
                var json = JsonConvert.SerializeObject(item);
                var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                List<string> keys = dictionary.Keys.ToList();

                foreach (string key  in keys) {
                    if (key== string.Format("{0}_AVG", columnPrefix)) {
                        dictionary[key] = item.AVG_CONSUME_AMOUNT;
                    }
                }

                // 各欄位
                int i = 0;
                foreach (string ym in currentYms) {
                    i++;
                    var o = from a in fees
                            where a.DATA_YM == ym &&
                                  a.INID == item.INID
                            select a;
                    MM_ABS_FEE temp = o.FirstOrDefault<MM_ABS_FEE>();

                    if (temp == null) {
                        temp = new MM_ABS_FEE();
                    }

                    foreach (string key in keys)
                    {
                        if (key == string.Format("{0}_AMOUNT_{1}", columnPrefix, i))
                        {
                            dictionary[key] = temp.ABS_AMOUNT;
                        }
                    }
                }

                
                newItem = GetObject<FA0060>(dictionary);

                bool isTempNull = false;
                if (columnPrefix != "C") {
                    FA0060 temp = targetList.Where(x => x.INID == newItem.INID).Select(x => x).FirstOrDefault<FA0060>();

                    isTempNull = (temp == null);

                    if (temp == null) {
                        temp = new FA0060();
                        temp.INID = newItem.INID;
                        temp.INID_NAME = newItem.INID_NAME;
                        temp.GRP_NO = newItem.GRP_NO;
                        temp.GRP_NAME = newItem.GRP_NAME;
                    }

                    List<string> prefixKeys = keys.Where(x => x.Contains(columnPrefix)).Select(x => x).ToList<string>();
                    foreach (string prefixKey in prefixKeys) {
                        temp.GetType().GetProperty(prefixKey).SetValue(temp, dictionary[prefixKey], null);
                    }

                    if (isTempNull)
                    {
                        targetList.Add(temp);
                        targetList = targetList.OrderBy(x => x.INID).Select(x => x).ToList();
                    }
                }

                

                list.Add(newItem);
            }

            if (columnPrefix != "C") {
                return targetList;
            }

            return list;
        }

        public T GetObject<T>(Dictionary<string, string> dict)
        {
            Type type = typeof(T);
            var obj = Activator.CreateInstance(type);

            foreach (var kv in dict)
            {
                if (kv.Key.ToString() == "RC")
                {
                    int i = 0;
                    type.GetProperty(kv.Key).SetValue(obj, int.TryParse(kv.Value, out i) ? int.Parse(kv.Value) : 0);
                }
                else {
                    type.GetProperty(kv.Key).SetValue(obj, kv.Value);
                }
                
            }
            return (T)obj;
        }


        [HttpPost]
        public ApiResponse T3Excel(FormDataCollection form)
        {
            string year = form.Get("year");
            string q = form.Get("q");
            string grp_no = form.Get("grp_no");
            string inid = form.Get("inid");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0060Repository repo = new FA0060Repository(DBWork);

                    IEnumerable<ColumnItem> columns = GetT3ExcelColumns(DBWork, year, q, grp_no, inid);

                    IEnumerable<FA0060> datas = GetT3ExcelData(DBWork, year, q, grp_no, inid);

                    DataTable data_dataTable = ConvertToDataTable<FA0060>(datas.ToList());

                    DataTable dtItems = new DataTable();
                    dtItems.Columns.Add("項次", typeof(int));
                    dtItems.Columns["項次"].AutoIncrement = true;
                    dtItems.Columns["項次"].AutoIncrementSeed = 1;
                    dtItems.Columns["項次"].AutoIncrementStep = 1;

                    dtItems.Columns.Add("GRP_NAME", typeof(string));
                    dtItems.Columns.Add("INID", typeof(string));
                    dtItems.Columns.Add("INID_NAME", typeof(string));

                    foreach (ColumnItem column in columns) {
                        dtItems.Columns.Add(column.DATAINDEX, typeof(string));
                    }

                    dtItems.Merge(data_dataTable, true, MissingSchemaAction.Ignore);
                    for (int i = 0; i < dtItems.Columns.Count; i++) {
                        foreach (ColumnItem column in columns) {
                            if (column.DATAINDEX == dtItems.Columns[i].ColumnName) {
                                dtItems.Columns[i].ColumnName = column.TEXT;
                            }
                        }
                        if (dtItems.Columns[i].ColumnName == "GRP_NAME") {
                            dtItems.Columns[i].ColumnName = "歸戶";
                        }
                        if (dtItems.Columns[i].ColumnName == "INID")
                        {
                            dtItems.Columns[i].ColumnName = "單位代碼";
                        }
                        if (dtItems.Columns[i].ColumnName == "INID_NAME")
                        {
                            dtItems.Columns[i].ColumnName = "單位名稱";
                        }
                    }

                    JCLib.Excel.Export(form.Get("FN"), dtItems);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        public IEnumerable<ColumnItem> GetT3ExcelColumns(UnitOfWork DBWork, string year, string q, string grp_no, string inid) {
            try
            {
                var repo = new FA0060Repository(DBWork);

                FA0060DataYmItem ymItem = repo.GetT3DataYmItem(year, q);

                IEnumerable<MM_ABS_FEE> CurrentFees = repo.GetData(ymItem.C_MIN_QYM, ymItem.C_MAX_QYM, grp_no, inid);
                IEnumerable<MM_ABS_FEE> pyFees = repo.GetData(ymItem.PRE_Y_MIN_QYM, ymItem.PRE_Y_MAX_QYM, grp_no, inid);
                IEnumerable<MM_ABS_FEE> psFees = repo.GetData(ymItem.PRE_S_MIN_QYM, ymItem.PRE_S_MAX_QYM, grp_no, inid);

                IEnumerable<string> currentYms = GetCurrentYms(CurrentFees);
                IEnumerable<string> pyYms = GetCurrentYms(pyFees);
                IEnumerable<string> psYms = GetCurrentYms(psFees);

                IEnumerable<ColumnItem> columns = GetT3Columns(ymItem, currentYms, "C");
                IEnumerable<ColumnItem> pyColumns = GetT3Columns(ymItem, pyYms, "PRE_Y");
                IEnumerable<ColumnItem> psColumns = GetT3Columns(ymItem, psYms, "PRE_S");

                List<ColumnItem> list = new List<ColumnItem>();
                list.AddRange(pyColumns.ToList());
                list.AddRange(psColumns.ToList());
                list.AddRange(columns.ToList());

                //pyColumns.ToList().AddRange(psColumns.ToList());
                //pyColumns.ToList().AddRange(columns.ToList());

                return list;
            }
            catch (Exception e) {
                throw;
            }
        }
        public IEnumerable<FA0060> GetT3ExcelData(UnitOfWork DBWork, string year, string q, string grp_no, string inid) {
            try
            {
                var repo = new FA0060Repository(DBWork);

                FA0060DataYmItem ymItem = repo.GetT3DataYmItem(year, q);
                List<FA0060> list = new List<FA0060>();
                // C
                IEnumerable<FA0060> items = repo.GetT3PastData(ymItem.C_MIN_QYM, ymItem.C_MAX_QYM, grp_no, inid, false);
                IEnumerable<MM_ABS_FEE> CurrentFees = repo.GetData(ymItem.C_MIN_QYM, ymItem.C_MAX_QYM, grp_no, inid);
                list.AddRange(GetT3Fa0060(items, CurrentFees, ymItem, "C", list));

                // PRE_Y
                items = repo.GetT3PastData(ymItem.PRE_Y_MIN_QYM, ymItem.PRE_Y_MAX_QYM, grp_no, inid, false);
                CurrentFees = repo.GetData(ymItem.PRE_Y_MIN_QYM, ymItem.PRE_Y_MAX_QYM, grp_no, inid);
                list = GetT3Fa0060(items, CurrentFees, ymItem, "PRE_Y", list).ToList();
                //list.AddRange(GetT3Fa0060(list, CurrentFees, ymItem, "PRE_Y"));

                // PRE_S
                items = repo.GetT3PastData(ymItem.PRE_S_MIN_QYM, ymItem.PRE_S_MAX_QYM, grp_no, inid, false);
                CurrentFees = repo.GetData(ymItem.PRE_S_MIN_QYM, ymItem.PRE_S_MAX_QYM, grp_no, inid);
                list = GetT3Fa0060(items, CurrentFees, ymItem, "PRE_S", list).ToList();
                //list.AddRange(GetT3Fa0060(list, CurrentFees, ymItem, "PRE_S"));

                return list;
            }
            catch (Exception e)
            {
                throw;
            }
        }
        public DataTable ConvertToDataTable<T>(IList<T> data)
        {
            PropertyDescriptorCollection properties =
               TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;

        }

        [HttpPost]
        public ApiResponse TransferByYm(FormDataCollection form) {
            string ym = form.Get("ym");
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new FA0060Repository(DBWork);

                    // 先轉資料再查詢
                    // 刪除上個月MM_ABS_FEE資料，再轉入
                    session.Result.afrs = repo.DeleteAbsFeeByYm(ym);
                    session.Result.afrs = repo.insertAbsFeeByYm(ym);

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
        public ApiResponse GetYearCombo() {
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0060Repository(DBWork);
                    session.Result.etts = repo.GetT3Year();
                }
                catch (Exception e) {
                    throw;
                }
                return session.Result;
            }
        }
        #endregion
    }
}