using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AB;
using WebApp.Models;
using JCLib.DB;
using System.Drawing.Printing;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using System.Linq;
using WebApp.Controllers;
using WebApp.Repository.AA;

namespace WebApp.Report.A
{
    public partial class AB0068 : SiteBase.BasePage
    {
        string P0;  //日報or月報
        string P1;  //日期：日報(年月日) 月報(年月)
        string P2;  //高價藥品IsCheck
        string P3;  //CDC用藥IsCheck
        string P4;  //藥品分類
        string P4_Name;  //藥品分類名稱
        string P5;  //庫別
        string P5_Name;  //庫別名稱
        string P6;  // 全院停用碼
        string P7;  // 各庫停用碼
        bool isWard = false;


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //RDLC本身的列印功能鍵
                //ReportViewer1.ShowPrintButton = false;
                //ReportViewer1.ShowExportControls = false;
                P0 = (Request.QueryString["p0"] is null) ? "" : Request.QueryString["p0"].ToString().Replace("null", "");
                P1 = (Request.QueryString["p1"] is null) ? "" : Request.QueryString["p1"].ToString().Replace("null", "");
                P2 = (Request.QueryString["p2"] is null) ? "" : Request.QueryString["p2"].ToString().Replace("null", "");
                P3 = (Request.QueryString["p3"] is null) ? "" : Request.QueryString["p3"].ToString().Replace("null", "");
                P4 = (Request.QueryString["p4"] is null) ? "" : Request.QueryString["p4"].ToString().Replace("null", "");
                P4_Name = (Request.QueryString["p4_Name"] is null) ? "" : Request.QueryString["p4_Name"].ToString().Replace("null", "");
                P5 = (Request.QueryString["p5"] is null) ? "" : Request.QueryString["p5"].ToString().Replace("null", "");
                P5_Name = (Request.QueryString["p5_Name"] is null) ? "" : Request.QueryString["p5_Name"].ToString().Replace("null", "");
                P6 = (Request.QueryString["p6"] is null) ? "" : Request.QueryString["p6"].ToString().Replace("null", "");
                P7 = (Request.QueryString["p7"] is null) ? "" : Request.QueryString["p7"].ToString().Replace("null", "");
                isWard = Request.QueryString["isWard"] == "Y";

                report1Bind();
            }
        }
        protected void report1Bind()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0068Repository repo = new AB0068Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();


                    //停用收集使用方式資料，加快報表顯示
                    ReportViewer1.EnableTelemetry = false;

                    //日報與月報報表路徑設定
                    ReportViewer1.LocalReport.ReportPath = P0 == "1" ? @"Report\A\AB0068_Date.rdlc" : @"Report\A\AB0068_Month.rdlc";

                    //產生當下列印時間(民國格式)，將時間寫到報表中顯示(固定)
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.ToString("HH:mm:ss");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });

                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    string str_SET_BTIME = "";
                    string str_SET_ETIME = "";
                    AB0068_TIME li_Time = new AB0068_TIME();

                    string today = repo.GetTodayDate();

                    if (P0 == "1") {
                        li_Time = repo.GetDateTimeRange(P1);
                    }
                    else {
                        li_Time = repo.GetMonthTimeRange(P1.Substring(0, 3) + P1.Substring(3, 2));
                    }

                    str_SET_BTIME = li_Time.SET_BTIME;
                    str_SET_ETIME = li_Time.SET_CTIME;
                    if (!string.IsNullOrEmpty(str_SET_BTIME))
                    {
                        str_SET_BTIME = str_SET_BTIME.Substring(0, 7) + " " + str_SET_BTIME.Substring(7, 2) + ":" + str_SET_BTIME.Substring(9, 2) + ":" + str_SET_BTIME.Substring(11, 2);
                    }
                    else
                    {
                        str_SET_BTIME = "---";
                    }
                    if (!string.IsNullOrEmpty(str_SET_ETIME))
                    {
                        str_SET_ETIME = str_SET_ETIME.Substring(0, 7) + " " + str_SET_ETIME.Substring(7, 2) + ":" + str_SET_ETIME.Substring(9, 2) + ":" + str_SET_ETIME.Substring(11, 2);
                    }
                    else
                    {
                        str_SET_ETIME = "---";
                    }

                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("SET_BTIME", str_SET_BTIME) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("SET_ETIME", str_SET_ETIME) });

                    string str_Date = "";
                    if (P0 == "1")
                    {
                        str_Date = P1.Substring(0, 3) + "年" + P1.Substring(3, 2) + "月" + P1.Substring(5, 2) + "日";
                    }
                    else
                    {
                        str_Date = P1.Substring(0, 3) + "年" + P1.Substring(3, 2) + "月";
                    }
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Date", str_Date) });

                    string str_WHNO_Name = P5_Name;
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("WHNO_Name", str_WHNO_Name) });

                    string str_MedicineClass = P4_Name;
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("MedicineClass", str_MedicineClass) });

                    string str_PrintUser = DBWork.UserInfo.UserName;
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintUser", str_PrintUser) });


                    ReportViewer1.PageCountMode = PageCountMode.Actual;

                    //先清空報表的DataSet，再將讀到的AB0068 DataTable放到DataSources(對應到AB0068.xsd)
                    ReportViewer1.LocalReport.DataSources.Clear();
                    if (P0 == "1")
                    {
                        if (P5 != "all" && P5 != "isPhd")
                        {
                            IEnumerable<Models.AB0068> list = repo.SearchReportData_Date(P5, P4, P1, P2, P3, false, P6, P7);

                            if (today == P1 && isWard)
                            {
                                list = GetNewUSEO_QTY(DBWork, list, P5);
                            }

                            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AB0068", list));
                        }
                        else {
                            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AB0068", repo.SearchReportData_Date_notCombo(P5, P4, P1, P2, P3, false, P6, P7)));
                        }
                        
                    }
                    else
                    {
                        if (P1 == "10908")
                        {
                            ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("SET_BTIME", "1090731 23:59:59") });

                            if (P5 != "all" && P5 != "isPhd")
                            {
                                ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AB0068", repo.SearchReportData_Month_10908(P5, P4, P1, P2, P3, false,  P6, P7)));
                            }
                            else
                            {
                                ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AB0068", repo.SearchReportData_Month_notCombo_10908(P5, P4, P1, P2, P3, false,  P6, P7)));
                            }
                        }
                        else {
                            if (P5 != "all" && P5 != "isPhd")
                            {
                                ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AB0068", repo.SearchReportData_Month(P5, P4, P1, P2, P3, false, li_Time, P6, P7)));
                            }
                            else
                            {
                                ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AB0068", repo.SearchReportData_Month_notCombo(P5, P4, P1, P2, P3, false, li_Time, P6, P7)));
                            }
                        }
                    }

                    ReportViewer1.LocalReport.Refresh();
                }
                catch(Exception e)
                {
                    throw;
                }
                //return session.Result;
            }
        }

        public CallHisApiResult GetCallHisApiData(UnitOfWork DBWork, IEnumerable<Models.AB0068> list, string wh_no)
        {
            AB0068Repository repo = new AB0068Repository(DBWork);

            COMBO_MODEL datetimeItem = repo.GetMaxComsumeDate();

            CallHisApiData data = new CallHisApiData();
            data.StartDateTime = datetimeItem.VALUE;
            data.EndDateTime = datetimeItem.TEXT;
            data.OrderCode =  GetMmcodes(list);
            data.StockCode = wh_no;

            string url = $"http://f5-hisregweb.ndmctsgh.edu.tw/DrugQuantity/api/DrugApplyMultiple";
            //string url = $"http://192.168.3.110:871/api/DrugApply";

            CallHisApiResult result = CallHisApiController.CallWebApi.JsonPostAsync(data, url).Result;

            return result;
        }
        public IEnumerable<Models.AB0068> GetNewUSEO_QTY(UnitOfWork DBWork, IEnumerable<Models.AB0068> list, string wh_no)
        {
            CallHisApiResult hisResult = GetCallHisApiData(DBWork, list, wh_no);

            if (hisResult.APIResultData == null)
            {
                return list;
            }

            IEnumerable<APIResultData> orderList = hisResult.APIResultData;
            int count = 0;
            int useo_qty = 0;
            int af_qty = 0;
            foreach (Models.AB0068 item in list)
            {
                item.ORI_USEO_QTY = item.USEO_QTY;
                item.ORI_AF_QTY = item.AF_QTY;

                count = 0;
                List<APIResultData> temps = orderList.Where(x => x.ORDERCODE == item.MMCODE).Select(x => x).ToList<APIResultData>();
                foreach (APIResultData temp in temps)
                {
                    bool isNumber = int.TryParse(temp.USEQTY, out int i);
                    if (isNumber)
                    {
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

        public string GetMmcodes(IEnumerable<Models.AB0068> list)
        {
            string mmcodes = string.Empty;
            foreach (Models.AB0068 item in list)
            {
                if (mmcodes != string.Empty)
                {
                    mmcodes += ",";
                }
                mmcodes += item.MMCODE;
            }
            return mmcodes;
        }
    }
}