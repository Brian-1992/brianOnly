using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AA;
using WebApp.Models;
using JCLib.DB;
using JCLib.DB.Tool;
using System.Collections.Generic;
using WebApp.Repository.AA;

namespace WebApp.Report.A
{
    public partial class AB0081 : SiteBase.BasePage
    {
        FL fl = new FL("WebApp.Report.A.AB0081");
        string P0;
        // string P1;
        string YYYMM;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {   
                P0 = fl.reqQueryString(Request, "P0", "10806");
                //P1 = Request.QueryString["p1"].ToString().Replace("null", "");
                YYYMM = P0;
                report1Bind();
            }
        } // 

        protected void report1Bind()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    // AA0123Repository repo = new AA0123Repository(DBWork);
                    AB0081Repository repo = new AB0081Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    //var taskid = repo.GetTaskid(User.Identity.Name);
                    //if (P1 == "2")
                    //{
                    //    P1 = "2";
                    //}
                    //else
                    //{
                    //    P1 = "3";
                    //}

                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.ToString("MM") + DateTime.Now.ToString("dd") + " " + DateTime.Now.ToString("HH:mm:ss");
                    ReportViewer1.EnableTelemetry = false; // 加快報表載入速度
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });                    
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("InidName", DBWork.UserInfo.UserName) });
                    // -- 月報、季報、年報，需配合真實「關帳日」呈現區段時間
                    IEnumerable<AB0081ReportMODEL> lstTime = repo.GetMiMnsetData(P0);  // 從資料庫讀取開始結束日期(SET_BTIME, SET_ETIME)
                    foreach (AB0081ReportMODEL t in lstTime)
                    {
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("SET_BTIME", t.SET_BTIME) });
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("SET_ETIME", t.SET_ETIME) });
                        break;
                    }

                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("YYYMM", YYYMM) });
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("ReportType", "1") });
                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DisplayName = YYYMM;


                    IEnumerable<AB0081ReportMODEL> lst;
                    // 從資料庫讀取資料
                    lst = repo.GetPrintData(P0);
                    if (
                        ((List<AB0081ReportMODEL>)lst).Count == 0
                    )
                    {
                        // 模擬給值
                        List<AB0081ReportMODEL> lstAll = new List<AB0081ReportMODEL>();
                        AB0081ReportMODEL v = new AB0081ReportMODEL();
                        v.MMCODE = " "; // 00.院內碼
                        v.MMNAME_E = "目前無資料"; // 01.商品名
                        v.BASE_UNIT = " "; // 02.單位
                        v.CHEMO_A = "0"; // 03.內湖化療調配室_消耗
                        v.CHEMO_I = "0"; // 04.內湖化療調配室_結存 
                        v.CHEMOT_A = "0"; // 05.汀州化療調配室_消耗
                        v.CHEMOT_I = "0"; // 06.汀州化療調配室_結存 
                        v.PH1A_A = "0"; // 07.內湖住院藥局_消耗
                        v.PH1A_I = "0"; // 08.內湖住院藥局_結存 
                        v.PH1C_A = "0"; // 09.內湖門診藥局_消耗
                        v.PH1C_I = "0"; // 10.內湖門診藥局_結存 
                        v.AVG_PRICE = "0"; // 11.移動平圴加權量
                        v.PH1R_A = "0"; // 12.內湖急診藥局_消耗
                        v.PH1R_I = "0"; // 13.內湖急診藥局_結存 
                        v.PHMC_A = "0"; // 14.汀州藥局_消耗
                        v.PHMC_I = "0"; // 15.汀州藥局_結存 
                        v.TPN_A = "0"; // 16.製劑室_消耗
                        v.TPN_I = "0"; // 17.製劑室_結存 
                        lstAll.Add(v);
                        lst = (IEnumerable<AB0081ReportMODEL>)lstAll;
                    } // 

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AB0081DataSet", lst));

                    ReportViewer1.LocalReport.Refresh();
                }
                catch (Exception ex)
                {
                    fl.le("report1Bind()", ex.Message);
                    throw;
                }
            }
        } // 


    } // ec
} // en