﻿using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Models;
using JCLib.DB;
using WebApp.Repository.F;
using WebApp.Repository.AA;

namespace WebApp.Report.F
{
    public partial class FA0069 : SiteBase.BasePage
    {
        string parMAT_CLASS;
        string parSET_YM;
        string parINIDFLAG;
        string parM_STOREID;
        bool parclsALL;
        string parWH_NO;
        string parGetMatName;
        string parGetInidFlagName;
        string parGetM_StoreIDName;
        string parIS_INV_MINUS;
        string parMMCODE;
        string parIS_OUT_MINUS;
        string is_pmnqty_minus;
        string cancel_id;
        string is_source_c;
        string whno_cancel;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                parMAT_CLASS = Request.QueryString["MAT_CLASS"].ToString().Replace("null", "");
                parSET_YM = Request.QueryString["SET_YM"].ToString().Replace("null", "");
                parINIDFLAG = Request.QueryString["INIDFLAG"].ToString().Replace("null", "");
                parM_STOREID = Request.QueryString["M_STOREID"].ToString().Replace("null", "");
                parclsALL = bool.Parse(Request.QueryString["clsALL"].ToString().Replace("null", ""));
                parWH_NO = Request.QueryString["WH_NO"].ToString().Replace("null", "");
                parGetMatName = Request.QueryString["getMatName"].ToString().Replace("null", "");
                parGetInidFlagName = Request.QueryString["getInidFlagName"].ToString().Replace("null", "");
                parGetM_StoreIDName = Request.QueryString["getM_StoreIDName"].ToString().Replace("null", "");
                parIS_INV_MINUS = Request.QueryString["IS_INV_MINUS"].ToString().Replace("null", "");
                parMMCODE = Request.QueryString["MMCODE"].ToString().Replace("null", "");
                parIS_OUT_MINUS = Request.QueryString["IS_OUT_MINUS"].ToString().Replace("null", "");
                is_pmnqty_minus = Request.QueryString["IS_PMNQTY_MINUS"].ToString().Replace("null", "");
                cancel_id = Request.QueryString["cancel_id"].ToString().Replace("null", "");
                is_source_c = Request.QueryString["is_source_c"].ToString().Replace("null", "");
                whno_cancel = Request.QueryString["whno_cancel"].ToString().Replace("null", "");
                report1Bind();
            }
        }
        protected void report1Bind()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0069Repository repo = new FA0069Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    ReportViewer1.EnableTelemetry = false;

                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MMdd");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("FA0069", repo.GetFa0069Report(parSET_YM, parINIDFLAG, parM_STOREID, parclsALL, parWH_NO, parIS_INV_MINUS, parMMCODE, parIS_OUT_MINUS, is_pmnqty_minus, cancel_id, is_source_c, whno_cancel)));
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