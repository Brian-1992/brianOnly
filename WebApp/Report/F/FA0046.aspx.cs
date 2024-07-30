using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Models;
using JCLib.DB;
using WebApp.Repository.F;
using WebApp.Repository.AA;

namespace WebApp.Report.F
{
    public partial class FA0046 : SiteBase.BasePage
    {
        string parP0;
        string parP1;
        string parP2;
        string parP3;
        string parP4;
        string Class_N;
        string RG_N;


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                parP0 = Request.QueryString["P0"].ToString().Replace("null", "");
                parP1 = Request.QueryString["P1"].ToString().Replace("null", "");
                parP2 = Request.QueryString["P2"].ToString().Replace("null", "");
                parP3 = Request.QueryString["P3"].ToString().Replace("null", "");
                parP4 = Request.QueryString["P4"].ToString().Replace("null", "");

                switch (parP2)
                {
                    case "01":
                        Class_N = "藥品";
                        break;
                    case "02":
                        Class_N = "衛材(含檢材)";
                        break;
                    case "07":
                        Class_N = "被服";
                        break;
                    case "08":
                        Class_N = "資訊耗材";
                        break;
                    case "0X":
                        Class_N = "一般行政類";
                        break;
                }


                if (parP2 == "01")
                {
                    switch (parP4)
                    {
                        case "1":
                            RG_N = "口服";
                            break;
                        case "2":
                            RG_N = "非口服";
                            break;
                        case "3":
                            RG_N = "1~3級管制";
                            break;
                        case "4":
                            RG_N = "4級管制";
                            break;
                    }
                }
                else
                {
                    switch (parP3)
                    {
                        case "1":
                            RG_N = "庫備品";
                            break;
                        case "0":
                            RG_N = "非庫備品";
                            break;
                    }
                }


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
                    FA0046Repository repo = new FA0046Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("WH_NO", parP0) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Class", Class_N) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("RG_N", RG_N) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("YYYMM", parP1) });
                    ReportViewer1.EnableTelemetry = false;
                    ReportViewer1.LocalReport.DataSources.Clear();

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("FA0046", repo.Report(parP0, parP1, parP2, parP2 == "01" ? parP4 : parP3)));

                    ReportViewer1.LocalReport.Refresh();
                }
                catch
                {
                    throw;
                }
            }


        }
    }
}