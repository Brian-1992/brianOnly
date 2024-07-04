using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AB;
using WebApp.Models;
using System;
using System.Data;
using System.Collections.Generic;
using WebApp.Controllers;
using System.Linq;

namespace WebApp.Controllers.AB
{
    public class AB0068Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse All(FormDataCollection form) {
            string P0 = (form.Get("p0") is null) ? "" : form.Get("p0");
            string P1 = (form.Get("p1") is null) ? "" : form.Get("p1");
            string P2 = form.Get("p2");
            string P3 = form.Get("p3");
            string P4 = (form.Get("p4") is null) ? "" : form.Get("p4");
            string P4_Name = (form.Get("p4_Name") is null) ? "" : form.Get("p4_Name").ToString().Replace("null", "");
            string P5 = (form.Get("p5") is null) ? "" : form.Get("p5").ToString().Replace("null", "");
            string P5_Name = (form.Get("p5_Name") is null) ? "" : form.Get("p5_Name").ToString().Replace("null", "");
            string P6 = form.Get("p6");
            string P7 = form.Get("p7");
            bool isWard = form.Get("isWard") == "Y";

            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0068Repository(DBWork);
                    if (P0 == "1")
                    {
                        IEnumerable<AB0068> list = new List<AB0068>();

                        if (int.Parse(P1) < 1090801) {
                            session.Result.msg = "查詢日期不可早於1090801";
                            session.Result.success = false;
                            return session.Result;
                        }

                        string today = repo.GetTodayDate();

                        if (P5 != "all" && P5 != "isPhd")
                        {
                            list =  repo.SearchReportData_Date(P5, P4, P1, P2, P3, true, P6, P7);
                            // 查詢日期為今天且進入點為病房(AB0103)
                            if (today == P1 && isWard) {
                                list = GetNewUSEO_QTY(DBWork, list, P5);
                            }
                            
                            session.Result.etts = list;
                        }
                        else
                        {
                            list = repo.SearchReportData_Date_notCombo(P5, P4, P1, P2, P3, true, P6, P7);

                            session.Result.etts = list;
                        }

                    }
                    else
                    {
                        if (int.Parse(P1) < 10908)
                        {
                            session.Result.msg = "查詢月份不可早於10908";
                            session.Result.success = false;
                            return session.Result;
                        }

                        AB0068_TIME time_range = repo.GetMonthTimeRange(P1);
                        // 2020-09-04: 月結年月為10908 使用10908功能
                        if (P1 == "10908")
                        {
                            if (P5 != "all" && P5 != "isPhd")
                            {
                                session.Result.etts = repo.SearchReportData_Month_10908(P5, P4, P1, P2, P3, true, P6, P7);
                            }
                            else
                            {
                                session.Result.etts = repo.SearchReportData_Month_notCombo_10908(P5, P4, P1, P2, P3, true, P6, P7);
                            }
                        }
                        else {
                            if (P5 != "all" && P5 != "isPhd")
                            {
                                session.Result.etts = repo.SearchReportData_Month(P5, P4, P1, P2, P3, true, time_range, P6, P7);
                            }
                            else
                            {
                                session.Result.etts = repo.SearchReportData_Month_notCombo(P5, P4, P1, P2, P3, true, time_range, P6, P7);
                            }
                        }
                    }

                }
                catch (Exception ex) {
                    throw;
                }

                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Excel(FormDataCollection form) {
            string P0 = (form.Get("p0") is null) ? "" : form.Get("p0");
            string P1 = (form.Get("p1") is null) ? "" : form.Get("p1");
            string P2 = form.Get("p2");
            string P3 = form.Get("p3");
            string P4 = (form.Get("p4") is null) ? "" : form.Get("p4");
            string P4_Name = (form.Get("p4_Name") is null) ? "" : form.Get("p4_Name").ToString().Replace("null", "");
            string P5 = (form.Get("p5") is null) ? "" : form.Get("p5").ToString().Replace("null", "");
            string P5_Name = (form.Get("p5_Name") is null) ? "" : form.Get("p5_Name").ToString().Replace("null", "");
            string P6 = form.Get("p6");
            string P7 = form.Get("p7");
            bool isWard = form.Get("isWard") == "Y";

            DataTable dtItems = new DataTable();
            dtItems.Columns.Add("項次", typeof(int));
            dtItems.Columns["項次"].AutoIncrement = true;
            dtItems.Columns["項次"].AutoIncrementSeed = 1;
            dtItems.Columns["項次"].AutoIncrementStep = 1;

            DataTable result = null;

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0068Repository(DBWork);
                    if (P0 == "1")
                    {
                        string today = repo.GetTodayDate();

                        if (P5 != "all" && P5 != "isPhd")
                        {

                            result = repo.SearchReportData_Date_excel(P5, P4, P1, P2, P3, P6, P7);
                            foreach (DataColumn col in result.Columns) {
                                col.ReadOnly = false;
                            }
                            // 查詢日期為今天且進入點為病房(AB0103)
                            if (today == P1 && isWard) {
                                result = GetNewUSEO_QTY_EXCEL(DBWork, result, P5);
                            }

                            //result = GetNewUSEO_QTY_EXCEL(DBWork, result, P5);

                        }
                        else
                        {
                            result = repo.SearchReportData_Date_notCombo_excel(P5, P4, P1, P2, P3, P6, P7);
                        }

                    }
                    else
                    {
                        AB0068_TIME time_range = repo.GetMonthTimeRange(P1);

                        if (P1 == "10908")
                        {
                            if (P5 != "all" && P5 != "isPhd")
                            {
                                result = repo.SearchReportData_Month_excel_10908(P5, P4, P1, P2, P3,  P6, P7);
                            }
                            else
                            {
                                result = repo.SearchReportData_Month_notCombo_excel_10908(P5, P4, P1, P2, P3,  P6, P7);
                            }
                        }
                        else {
                            if (P5 != "all" && P5 != "isPhd")
                            {
                                result = repo.SearchReportData_Month_excel(P5, P4, P1, P2, P3, time_range, P6, P7);
                            }
                            else
                            {
                                result = repo.SearchReportData_Month_notCombo_excel(P5, P4, P1, P2, P3, time_range, P6, P7);
                            }
                        }
                    }

                    dtItems.Merge(result);

                    JCLib.Excel.Export(form.Get("FN"), dtItems);

                }
                catch (Exception ex)
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse SearchCheck(FormDataCollection form)
        {
            string P0 = (form.Get("p0") is null) ? "" : form.Get("p0");
            string P1 = (form.Get("p1") is null) ? "" : form.Get("p1");
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    AB0068Repository repo = new AB0068Repository(DBWork);

                    if (P0 == "1")
                    {
                        if (int.Parse(P1) < 1090801)
                        {
                            session.Result.msg = "查詢日期不可早於1090801";
                            session.Result.success = false;
                            return session.Result;
                        }
                    }
                    else {
                        if (int.Parse(P1) < 10908)
                        {
                            session.Result.msg = "查詢月份不可早於10908";
                            session.Result.success = false;
                            return session.Result;
                        }

                        AB0068_TIME time_range = repo.GetMonthTimeRange(P1);
                    }

                    session.Result.success = true;
                }
                catch (Exception e) {
                    session.Result.success = false;
                    return session.Result;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetWH_NOComboOne(FormDataCollection form)
        {

            bool isWard = form.Get("isWard") == "Y";

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0068Repository repo = new AB0068Repository(DBWork);
                    if (isWard)
                    {
                        session.Result.etts = repo.GetWH_NOComboWard(DBWork.UserInfo.UserId, DBWork.UserInfo.Inid);
                    }
                    else {
                        session.Result.etts = repo.GetWH_NOComboOne();
                    }
                    
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public CallHisApiResult GetCallHisApiData(UnitOfWork DBWork, IEnumerable<AB0068> list, string wh_no) {
            AB0068Repository repo = new AB0068Repository(DBWork);
            
            COMBO_MODEL datetimeItem = repo.GetMaxComsumeDate();

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
        public CallHisApiResult GetCallHisApiData(UnitOfWork DBWork, DataTable dataTable, string wh_no)
        {
            AB0068Repository repo = new AB0068Repository(DBWork);

            COMBO_MODEL datetimeItem = repo.GetMaxComsumeDate();

            CallHisApiData data = new CallHisApiData();
            data.StartDateTime = datetimeItem.VALUE;
            data.EndDateTime = datetimeItem.TEXT;
            data.OrderCode = GetMmcodes_excel(dataTable);
            data.StockCode = wh_no;

            string url = $"http://f5-hisregweb.ndmctsgh.edu.tw/DrugQuantity/api/DrugApplyMultiple";
            //string url = $"http://192.168.3.110:871/api/DrugApply";

            CallHisApiResult result = CallHisApiController.CallWebApi.JsonPostAsync(data, url).Result;

            return result;
        }

        public IEnumerable<AB0068> GetNewUSEO_QTY(UnitOfWork DBWork, IEnumerable<AB0068> list, string wh_no)
        {
            CallHisApiResult hisResult = GetCallHisApiData(DBWork, list, wh_no);

            if (hisResult.APIResultData == null) {
                return list;
            }

            IEnumerable<APIResultData> orderList = hisResult.APIResultData;
            int count = 0;
            int useo_qty = 0;
            int af_qty = 0;
            foreach (AB0068 item in list)
            {
                item.ORI_USEO_QTY = item.USEO_QTY;
                item.ORI_AF_QTY = item.AF_QTY;

                count = 0;
                List<APIResultData> temps = orderList.Where(x => x.ORDERCODE == item.MMCODE).Select(x => x).ToList<APIResultData>();
                foreach (APIResultData temp in temps) {
                    bool isNumber = int.TryParse(temp.USEQTY, out int i);
                    if (isNumber){
                        count += int.Parse(temp.USEQTY);
                    }
                }

                useo_qty = int.Parse(item.USEO_QTY);
                af_qty = int.Parse(item.AF_QTY);

                item.NEW_USEO_QTY = count.ToString();
                useo_qty += count;
                af_qty = af_qty - count;

                item.USEO_QTY = useo_qty.ToString();
                item.AF_QTY = af_qty.ToString();
            }

            return list;
        }

        public string GetMmcodes(IEnumerable<AB0068> list) {
            string mmcodes = string.Empty;
            foreach (AB0068 item in list) {
                if (mmcodes != string.Empty) {
                    mmcodes += ",";
                }
                mmcodes += item.MMCODE;
            }
            return mmcodes;
        }

        public DataTable GetNewUSEO_QTY_EXCEL(UnitOfWork DBWork, DataTable dataTable, string wh_no)
        {
            CallHisApiResult hisResult = GetCallHisApiData(DBWork, dataTable, wh_no);

            if (hisResult.APIResultData == null)
            {
                return dataTable;
            }

            IEnumerable<APIResultData> orderList = hisResult.APIResultData;
            int count = 0;
            int useo_qty = 0;
            int af_qty = 0;
            foreach (DataRow row in dataTable.Rows) {
                count = 0;
                List<APIResultData> temps = orderList.Where(x => x.ORDERCODE == row["院內碼"].ToString()).Select(x => x).ToList<APIResultData>();
                foreach (APIResultData temp in temps)
                {
                    bool isNumber = int.TryParse(temp.USEQTY, out int i);
                    if (isNumber)
                    {
                        count += int.Parse(temp.USEQTY);
                    }
                }
                useo_qty = int.Parse(row["醫令消耗"].ToString());
                af_qty = int.Parse(row["結存"].ToString());

                dataTable.Select("院內碼='" + row["院內碼"] + "'")[0]["醫令消耗"] = (useo_qty + count).ToString();
                dataTable.Select("院內碼='" + row["院內碼"] + "'")[0]["結存"] = (af_qty - count).ToString();
            }

            return dataTable;
        }

        public string GetMmcodes_excel(DataTable dataTable) {
            string mmcodes = string.Empty;
            foreach (DataRow row in dataTable.Rows) {
                if (mmcodes != string.Empty) {
                    mmcodes += ",";
                }
                mmcodes += row["院內碼"];
            }

            return mmcodes;
        }
    }
}