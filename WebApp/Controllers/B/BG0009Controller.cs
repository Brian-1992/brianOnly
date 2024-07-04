using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.B;
using WebApp.Models;
using Microsoft.Reporting.WebForms;
using System;
using System.Data;
using System.IO;
using System.Web;
using System.Net;
using System.Net.Sockets;
using System.Net.Mail;
using System.Collections.Generic;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace WebApp.Controllers.BG
{
    public class BG0009Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var pDATA_YM = form.Get("data_ym");
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p2_1 = form.Get("p2_1");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");
            var p6 = form.Get("p6");
            var p7 = form.Get("p7");//廠商統編

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort") == null ? "" : form.Get("sort");

            string[] arr_p0 = { };
            if (!string.IsNullOrEmpty(p0))
            {
                arr_p0 = p0.Trim().Split(','); //用,分割
            }
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BG0009Repository(DBWork);
                    session.Result.etts = repo.GetAll(pDATA_YM, arr_p0, p1, p2, p2_1, p3, p4, p5, p6, p7, page, limit, sorters);

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMatclassCombo()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BG0009Repository repo = new BG0009Repository(DBWork);
                    session.Result.etts = repo.GetMatclassCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetSourcecodeCombo()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BG0009Repository repo = new BG0009Repository(DBWork);
                    session.Result.etts = repo.GetSourcecodeCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetMcontidCombo()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BG0009Repository repo = new BG0009Repository(DBWork);
                    session.Result.etts = repo.GetMcontidCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetAgennoCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BG0009Repository repo = new BG0009Repository(DBWork);
                    session.Result.etts = repo.GetAgennoCombo(p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse chkDATA_YM(FormDataCollection form)
        {
            string data_ym = form.Get("DATA_YM");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BG0009Repository repo = new BG0009Repository(DBWork);
                    if (repo.ChkDATA_YM(data_ym) > 0)
                        session.Result.msg = "Y";
                    else
                        session.Result.msg = "N";
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetPH_VENDER_MEMO(FormDataCollection form)
        {
            string agenno = form.Get("AGENNO");
            string agenno1 = form.Get("AGENNO_1");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BG0009Repository repo = new BG0009Repository(DBWork);
                    session.Result.msg = repo.GetPH_VENDER_MEMO(agenno, agenno1);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetPhVenderUniNoCombo(FormDataCollection form)
        {
            //IEnumerator<KeyValuePair<string, string>> values = form.GetEnumerator();

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
                    BG0009Repository repo = new BG0009Repository(DBWork);
                    session.Result.etts = repo.GetPhVenderUniNoCombo(p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse SavePH_VENDER_MEMO(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BG0009Repository repo = new BG0009Repository(DBWork);
                    if (form.Get("NOTE") != "")
                    {
                        string agenno = form.Get("AGENNO");
                        string agenno1 = form.Get("AGENNO_1");
                        string note = form.Get("NOTE");

                        session.Result.afrs = repo.SavePH_VENDER_MEMO(agenno, agenno1, note);
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
        public ApiResponse SendMail(FormDataCollection form)
        {
            string pDATA_YM = form.Get("pDATA_YM");
            string p2 = form.Get("p2"); ;
            string p2_1 = form.Get("p2_1"); ;
            string YYY = pDATA_YM.Substring(0, 3);
            string MM = pDATA_YM.Substring(3, 2);
            if (MM.Substring(0, 1) == "0")
            {
                MM = pDATA_YM.Substring(4, 1);
            }
            int CNT = 0;
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BG0009Repository repo = new BG0009Repository(DBWork);
                    DataTable dtM = new DataTable();
                    dtM = repo.GetMailPhvenderData(p2, p2_1);
                    string agenno = "", email = "", memo = "";
                    foreach (DataRow row in dtM.Rows)
                    {
                        agenno = row["AGEN_NO"].ToString();
                        email = row["EMAIL"].ToString();
                        memo = row["MEMO"].ToString();
                        //檢查廠商於統計年月是否有資料
                        CNT = repo.ChkPrintData(pDATA_YM, agenno);
                        if (CNT > 0)
                        {
                            ReportViewer reportViewer = new ReportViewer();
                            reportViewer.ProcessingMode = ProcessingMode.Local;
                            reportViewer.LocalReport.ReportPath = Path.Combine(HttpContext.Current.Server.MapPath("~/Report/B"), "BG0009M.rdlc");

                            string hospName = repo.GetHospName();
                            string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "年" + DateTime.Now.Month + "月" + DateTime.Now.Day + "日 ";
                            reportViewer.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                            reportViewer.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });

                            //停用收集使用方式資料，加快報表顯示
                            reportViewer.EnableTelemetry = false;

                            DataTable dt = repo.GetReportMain(agenno);

                            reportViewer.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("YYY", YYY) });
                            reportViewer.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("MM", MM) });
                            reportViewer.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("M_AGENNO", dt.Rows[0]["M_AGENNO"].ToString().Trim()) });
                            reportViewer.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("AGEN_NAME", dt.Rows[0]["AGEN_NAME"].ToString().Trim()) });
                            reportViewer.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("AGEN_TEL", dt.Rows[0]["AGEN_TEL"].ToString().Trim()) });
                            reportViewer.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("AGEN_FAX", dt.Rows[0]["AGEN_FAX"].ToString().Trim()) });

                            //先清空報表的DataSet，再將讀到的BG0009M DataTable放到DataSources(對應到BG0009M.xsd)
                            reportViewer.LocalReport.DataSources.Clear();
                            reportViewer.LocalReport.DataSources.Add(new ReportDataSource("BG0009M", repo.GetPrintData(pDATA_YM, agenno)));
                            reportViewer.LocalReport.DataSources.Add(new ReportDataSource("BG0009Count", repo.GetPrintDetails(pDATA_YM, agenno)));

                            // 因應Path Traversal問題,不將檔名當作sendMail的參數,故在sendMail內外各宣告一次檔名
                            string sServerFile = @"~/App_Data/廠商貨款對帳單_" + DateTime.Now.ToString("yyyyMMdd") + ".pdf";
                            CreateFile("PDF", sServerFile, reportViewer);

                            sendMail(email, "廠商貨款對帳單(" + agenno + ")", memo);
                            //寄信後刪除檔案
                            if (File.Exists(HttpContext.Current.Server.MapPath(sServerFile)))
                                File.Delete(HttpContext.Current.Server.MapPath(sServerFile));
                        }
                    }
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }
        public string CreateFile(string szFileType, string szFileName, ReportViewer reportViewer)
        {
            string sMSG = "";
            Warning[] warnings;
            string[] streamIds;
            string mimeType = string.Empty;
            string encoding = "utf-8";
            string extension = string.Empty;
            try
            {
                byte[] bytes = reportViewer.LocalReport.Render(szFileType, null, out mimeType, out encoding, out extension, out streamIds, out warnings);
                FileStream fs = new FileStream(HttpContext.Current.Server.MapPath(szFileName), FileMode.Create);
                fs.Write(bytes, 0, bytes.Length);
                fs.Close();
                fs.Dispose();
            }
            catch (DirectoryNotFoundException ex)
            {
                sMSG = ex.Message.ToString();
            }
            return sMSG;
        }

        public void sendMail(string toMailPar, string toSubjectPar, string toContentPar)
        {
            // 因應Path Traversal問題,不將檔名當作sendMail的參數,故在sendMail內外各宣告一次檔名
            string sAttchFilePath = @"~/App_Data/廠商貨款對帳單_" + DateTime.Now.ToString("yyyyMMdd") + ".pdf";

            // 因應Resource Injection問題,在sendMail內再次檢查Mail, Subject, Content
            string toMail = "";
            string toSubject = "";
            string toContent = "";
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                BG0009Repository repo = new BG0009Repository(DBWork);
                toMail = repo.chkMail(toMailPar);
                toSubject = repo.chkSubject(toSubjectPar);
                toContent = repo.chkContent(toContentPar);
            }

            string fromMail = JCLib.Util.GetEnvSetting("SMTP_SYS_FROM");
            string fromName = JCLib.Util.GetEnvSetting("SMTP_SYS_NAME");
            if (isInAidc())
            {
                fromMail = "sandyhuang@ms.aidc.com.tw";
                toMail = "sandyhuang@ms.aidc.com.tw";
                toSubject = toSubject + "(測試)";
            }
            string smtpServer = JCLib.Util.GetEnvSetting("SMTP_SERV");
            SmtpClient smtp = new SmtpClient(smtpServer);

            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(fromMail, fromName);
                mail.To.Add(toMail);
                mail.Subject = toSubject;
                mail.Body = toContent;
                mail.IsBodyHtml = true; // Can set to false, if you are sending pure text.

                sAttchFilePath = HttpContext.Current.Server.MapPath(sAttchFilePath);
                if (!String.IsNullOrEmpty(sAttchFilePath) && File.Exists(sAttchFilePath))
                {
                    mail.Attachments.Add(new Attachment(sAttchFilePath));
                }

                try
                {
                    smtp.UseDefaultCredentials = true;
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
                catch (Exception ex)
                {
                    throw;
                    // rtnMessage = ex.Message;
                }
            }
        }

        public bool isInAidc()
        {
            // 使用GetHostName及GetHostAddresses在Fortify掃描會有問題,改為取DB位址判斷
            string dbConnStr = "";
            if (JCLib.Util.GetEnvSetting("DB_CONN_TYPE") == "TEST")
                dbConnStr = JCLib.DB.WorkSession.ConnectionStringTEST; // 測試DB
            else
                dbConnStr = JCLib.DB.WorkSession.ConnectionStringOFFICIAL; // 正式DB

            if (dbConnStr.Contains("192.168.99"))
            {
                return true;
            }
            return false;
        }

        // 匯出 廠商支付明細表
        [HttpPost]
        public ApiResponse AgenExcel(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    var pDATA_YM = (form.Get("pDATA_YM") is null) ? "" : form.Get("pDATA_YM").ToString().Replace("null", "");
                    var p0 = (form.Get("p0") is null) ? "" : form.Get("p0").ToString().Replace("null", "");
                    var p1 = (form.Get("p1") is null) ? "" : form.Get("p1").ToString().Replace("null", "");
                    var p2 = (form.Get("p2") is null) ? "" : form.Get("p2").ToString().Replace("null", "");
                    var p2_1 = (form.Get("p2_1") is null) ? "" : form.Get("p2_1").ToString().Replace("null", "");
                    var p3 = (form.Get("p3") is null) ? "" : form.Get("p3").ToString().Replace("null", "");
                    var p4 = (form.Get("p4") is null) ? "" : form.Get("p4").ToString().Replace("null", "");
                    var p5 = (form.Get("p5") is null) ? "" : form.Get("p5").ToString().Replace("null", "");
                    var p6 = (form.Get("p6") is null) ? "" : form.Get("p6").ToString().Replace("null", "");
                    var YYY = pDATA_YM.Substring(0, 3);
                    var MM = pDATA_YM.Substring(3, 2);
                    if (MM.Substring(0, 1) == "0")
                    {
                        MM = pDATA_YM.Substring(4, 1);
                    }
                    string[] arr_p0 = { };
                    if (!string.IsNullOrEmpty(p0))
                    {
                        arr_p0 = p0.Trim().Split(','); //用,分割
                    }

                    BG0009Repository repo = new BG0009Repository(DBWork);
                    //JCLib.Excel.Export("對帳單-廠商支付明細表_" + DateTime.Now.ToString("yyyyMMdd") + ".xls", repo.GetAgenExcel(pDATA_YM, arr_p0, p1, p2, p2_1, p3, p4, p5, p6));

                    //採取客製EXCEL格式進行匯出
                    DataTable data = repo.GetAgenExcel(pDATA_YM, arr_p0, p1, p2, p2_1, p3, p4, p5, p6);
                    string fileName = "對帳單-廠商支付明細表" + "_" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx";
                    if (data.Rows.Count > 0)
                    {
                        var workbook = ExoprtToExcel(data);

                        //output
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            workbook.Write(memoryStream);
                            JCLib.Export.OutputFile(memoryStream, fileName);
                            workbook.Close();
                        }
                    }

                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

        //製造EXCEL的內容
        public XSSFWorkbook ExoprtToExcel(DataTable data)
        {
            var wb = new XSSFWorkbook();
            var sheet = (XSSFSheet)wb.CreateSheet("Sheet1");

            IRow row = sheet.CreateRow(0);
            for (int i = 0; i < data.Columns.Count; i++)
            {
                row.CreateCell(i).SetCellValue(data.Columns[i].ToString());
            }

            for (int i = 0; i < data.Rows.Count; i++)
            {
                row = sheet.CreateRow(1 + i);
                for (int j = 0; j < data.Columns.Count; j++)
                {
                    if (j > 3)
                    {
                        var cell = row.CreateCell(j);
                        cell.SetCellType(CellType.Numeric);
                        var a = data.Rows[i].ItemArray[j];
                        if (a != null && a.ToString() != "null" && a != DBNull.Value)
                        {
                            double d;
                            if (double.TryParse(a.ToString(), out d))
                            {
                                cell.SetCellValue(Convert.ToDouble(a));
                            }
                        }
                    }
                    else
                    {
                        row.CreateCell(j).SetCellValue(data.Rows[i].ItemArray[j].ToString());
                    }
                }
            }
            return wb;
        }

        [HttpGet]
        public ApiResponse GetParamDMemo()
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BG0009Repository(DBWork);

                    if (repo.CheckParamDMemoExists() == false)
                    {
                        if (repo.CheckParamMBG0009Exists() == false)
                        {
                            session.Result.afrs = repo.InsertParamMBG0009();
                        }

                        session.Result.afrs = repo.InsertParamDMEMO();
                    }

                    session.Result.msg = repo.GetParamDMemo();

                    return session.Result;
                }
                catch (Exception e)
                {
                    throw;
                }
            }
        }

        [HttpPost]
        public ApiResponse SetParamDMemo(FormDataCollection form)
        {
            string memo = form.Get("MEMO");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BG0009Repository(DBWork);

                    session.Result.afrs = repo.UpdateParamDMemo(memo);

                    DBWork.Commit();

                    return session.Result;
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    throw;
                }
            }
        }
    }
}