using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.H;
using WebApp.Models;
using System.Collections.Generic;
using System.Data;
using WebApp.Models.H;
using System;
using System.Text;
using System.Web;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text.RegularExpressions;

namespace WebApp.Controllers.H
{
    public class HA0001Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                HA0001Repository repo = new HA0001Repository(DBWork);
                session.Result.etts = repo.GetAll(p0, p1, page, limit, sorters);

                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Update(HA0001 ha0001)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new HA0001Repository(DBWork);
                    ha0001.UPDATE_USER = User.Identity.Name;
                    ha0001.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.Update(ha0001);

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
        public ApiResponse ChkBtnGroup(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new HA0001Repository(DBWork);

                    string hospCode = repo.GetHospCode();
                    if (hospCode == "805")
                    {
                        // 805花蓮主計室只開放到[電匯]按鈕，電匯右邊的按鈕由衛保室操作
                        session.Result.msg = repo.GetUserRole(User.Identity.Name);
                    }
                    else
                        session.Result.msg = "ALL";
                }
                catch (Exception ex)
                {
                    session.Result.msg = ex.Message;
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetBankname(FormDataCollection form)
        {
            var agen_bank_14 = form.Get("p0");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new HA0001Repository(DBWork);

                    session.Result.msg = repo.getBANKNAME(agen_bank_14);
                }
                catch (Exception ex)
                {
                    session.Result.msg = ex.Message;
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetHospbank(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new HA0001Repository(DBWork);

                    session.Result.msg = repo.getHospBankAcc() + "^" + repo.getHospName();
                }
                catch (Exception ex)
                {
                    session.Result.msg = ex.Message;
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse LoadAgenBank(FormDataCollection form)
        {
            var remitno = form.Get("p0");
            var agenno = form.Get("p1");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new HA0001Repository(DBWork);

                    // 以PH_VENDER的資料更新銀行代碼及銀行帳號
                    repo.LoadAgenBank(remitno, agenno);
                }
                catch (Exception ex)
                {
                    session.Result.msg = ex.Message;
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse ChkIsremit(FormDataCollection form)
        {
            var data_ym = form.Get("p0");
            var remitnoFrom = form.Get("p2");
            var remitnoTo = form.Get("p3");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new HA0001Repository(DBWork);

                    // 尚無已匯出的資料則回傳P,有則回傳N
                    session.Result.msg = repo.ChkIsremit(data_ym, remitnoFrom, remitnoTo);
                }
                catch (Exception ex)
                {
                    session.Result.msg = ex.Message;
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse ChkBank(FormDataCollection form)
        {
            var data_ym = form.Get("p0");
            var remitnoFrom = form.Get("p2");
            var remitnoTo = form.Get("p3");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new HA0001Repository(DBWork);

                    // 匯款單都有銀行代碼,帳號則回傳P,否則回傳N
                    session.Result.msg = repo.ChkBank(data_ym, remitnoFrom, remitnoTo);
                }
                catch (Exception ex)
                {
                    session.Result.msg = ex.Message;
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetRemitnoCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    HA0001Repository repo = new HA0001Repository(DBWork);
                    session.Result.etts = repo.GetRemitnoCombo(p0);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetTxt(FormDataCollection form)
        {
            var p0 = form.Get("p0");            //月結月份
            var p1 = form.Get("p1").Trim();     //電匯日期
            var p2 = form.Get("p2").Trim();     //電匯編號(起)
            var p3 = form.Get("p3").Trim();     //電匯編號(迄)

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    HA0001Repository repo = new HA0001Repository(DBWork);
                    DataTable result = new DataTable();



                    repo.updateRemitdate(p0, p1, p2, p3);
                    result = repo.GetTxtData(p0, p2, p3);

                    ExportTxt(result);
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
        public ApiResponse GetChkTxt(FormDataCollection form)
        {
            var p0 = form.Get("p0");            //月結月份

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    HA0001Repository repo = new HA0001Repository(DBWork);
                    DataTable result = new DataTable();

                    ExportChkTxt(repo.GetChkResult(p0));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public static void ExportTxt(DataTable dataTable)
        {
            string fileName = DateTime.Now.Year + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Day.ToString().PadLeft(2, '0') + "_edi.txt";

            if (dataTable == null) return;
            if (HttpContext.Current == null) return;

            HttpResponse res = HttpContext.Current.Response;
            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms, Encoding.GetEncoding("big5"));
            string line = "";
            int Index = 1;
            Encoding utf81 = Encoding.GetEncoding("utf-8");
            Encoding utf82 = Encoding.GetEncoding("utf-8");
            Encoding utf83 = Encoding.GetEncoding("utf-8");
            Encoding big51 = Encoding.GetEncoding("big5");
            Encoding big52 = Encoding.GetEncoding("big5");
            Encoding big53 = Encoding.GetEncoding("big5");

            foreach (DataRow dr in dataTable.Rows)
            {
                foreach (DataColumn dc in dataTable.Columns)
                {
                    switch (dc.ColumnName)
                    {
                        case "銀行帳號":
                            //銀行帳號前加流水號，第1位起，共4位 
                            line += Index.ToString("0000");
                            //帳號長度不滿14向右靠，左補0， 第5位起，共14位
                            line += dr[dc].ToString().Trim().PadLeft(14, '0');
                            break;
                        case "匯款金額":
                            //整數11位，小數2位，第19位起，共13位
                            //匯款金額長度不滿11向左補0
                            line += dr[dc].ToString().Trim().PadLeft(11, '0');
                            //匯款金額向右補2個0
                            line += "00";
                            break;
                        case "銀行代碼":
                            //顯示銀行代碼跟分行代碼
                            line += dr[dc].ToString().Trim().PadLeft(7, '0');
                            break;
                        case "廠商名稱":
                            byte[] strUtf81 = utf81.GetBytes(dr[dc].ToString().Trim());
                            byte[] strBig51 = Encoding.Convert(utf81, big51, strUtf81);

                            char[] big5Chars1 = new char[big51.GetCharCount(strBig51, 0, strBig51.Length)];
                            big51.GetChars(strBig51, 0, strBig51.Length, big5Chars1, 0);
                            string tempString1 = new string(big5Chars1);
                            //顯示廠商名稱，且往右補空白直到長度為60，第39位起，共60位
                            int lens = Encoding.Default.GetByteCount(tempString1);
                            if (lens > 60)
                            {
                                string trimStr = TrimStringByByte(tempString1, 60);
                                line += trimStr + "".PadRight(60 - Encoding.Default.GetByteCount(trimStr), ' ');
                            }
                            else
                            {
                                line += tempString1 + "".PadRight(60 - lens, ' ');
                            }
                            break;
                        case "醫院名稱":
                            byte[] strUtf82 = utf82.GetBytes(dr[dc].ToString().Trim());
                            byte[] strBig52 = Encoding.Convert(utf82, big52, strUtf82);

                            char[] big5Chars2 = new char[big52.GetCharCount(strBig52, 0, strBig52.Length)];
                            big52.GetChars(strBig52, 0, strBig52.Length, big5Chars2, 0);
                            string tempString2 = new string(big5Chars2);
                            strUtf82 = utf82.GetBytes(dr[dc].ToString().Trim());
                            strBig52 = Encoding.Convert(utf82, big52, strUtf82);

                            big5Chars2 = new char[big52.GetCharCount(strBig52, 0, strBig52.Length)];
                            big52.GetChars(strBig52, 0, strBig52.Length, big5Chars2, 0);
                            // 醫院名稱長度為40
                            tempString2 = new string(big5Chars2);
                            int lens2 = Encoding.Default.GetByteCount(tempString2);
                            line += tempString2 + "".PadRight(40 - lens2, ' ');
                            break;
                        case "附言":
                            byte[] strUtf83 = utf83.GetBytes(dr[dc].ToString().Trim());
                            byte[] strBig53 = Encoding.Convert(utf83, big53, strUtf83);

                            char[] big5Chars3 = new char[big53.GetCharCount(strBig53, 0, strBig53.Length)];
                            big53.GetChars(strBig53, 0, strBig53.Length, big5Chars3, 0);
                            string tempString3 = new string(big5Chars3);
                            strUtf83 = utf83.GetBytes(dr[dc].ToString().Trim());
                            strBig53 = Encoding.Convert(utf83, big53, strUtf83);

                            big5Chars3 = new char[big52.GetCharCount(strBig53, 0, strBig53.Length)];
                            big53.GetChars(strBig53, 0, strBig53.Length, big5Chars3, 0);
                            tempString3 = new string(big5Chars3);
                            // 附言長度為40
                            int lens3 = Encoding.Default.GetByteCount(tempString3);
                            if (lens3 > 40)
                            {
                                string trimStr = TrimStringByByte(tempString3, 40);
                                line += trimStr + "".PadRight(40 - Encoding.Default.GetByteCount(trimStr), ' ');
                            }
                            else
                            {
                                line += tempString3 + "".PadRight(40 - lens3, ' ');
                            }
                            break;
                        case "統一編號":
                            // 統一編號長度為32
                            line += dr[dc].ToString().Trim().PadRight(32, ' ');
                            break;
                        default:
                            break;
                    }
                }
                Index++;
                if (Index <= dataTable.Rows.Count)
                    line += "\r\n";
            }
            sw.WriteLine(line); //write data
            sw.Close();

            res.BufferOutput = false;

            res.Clear();
            res.ClearHeaders();
            res.ContentEncoding = big51;
            res.Charset = "big5";
            res.ContentType = "application/octet-stream;charset=big5";
            res.AddHeader("Content-Disposition",
                        "attachment; filename=" + HttpUtility.HtmlEncode(HttpUtility.UrlEncode(fileNameFilter(fileName, ".txt"), System.Text.Encoding.UTF8)));
            res.BinaryWrite(ms.ToArray());
            res.Flush();
            res.End();

            ms.Close();
            ms.Dispose();
        }

        public static void ExportChkTxt(IEnumerable<HA0001> inputData)
        {
            string fileName = DateTime.Now.Year + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Day.ToString().PadLeft(2, '0') + "_主計作業檢查結果.txt";

            if (inputData == null) return;
            if (HttpContext.Current == null) return;

            HttpResponse res = HttpContext.Current.Response;
            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms, Encoding.GetEncoding("big5"));
            string line = "";
            Encoding big51 = Encoding.GetEncoding("big5");

            foreach (HA0001 itemRecord in inputData)
            {

                line += "廠商代碼:" + itemRecord.AGEN_NO + " " + itemRecord.CHKMSG + "\r\n";
            }
            sw.WriteLine(line); //write data
            sw.Close();

            res.BufferOutput = false;

            res.Clear();
            res.ClearHeaders();
            res.ContentEncoding = big51;
            res.Charset = "big5";
            res.ContentType = "application/octet-stream;charset=big5";
            res.AddHeader("Content-Disposition",
                        "attachment; filename=" + HttpUtility.HtmlEncode(HttpUtility.UrlEncode(fileNameFilter(fileName, ".txt"), System.Text.Encoding.UTF8)));
            res.BinaryWrite(ms.ToArray());
            res.Flush();
            res.End();

            ms.Close();
            ms.Dispose();
        }

        static string TrimStringByByte(string theString, int lens)
        {
            string ret = "";
            int count = 0;
            for (int i = 0; i < lens; i++)
            {
                ret += theString[i];
                count = Encoding.Default.GetByteCount(ret);
                if (count > lens)
                {
                    ret = ret.Substring(0, ret.Length - 1);
                    break;
                }
            }
            return ret;
        }

        //匯出匯款通知單EXCEL
        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            var data_ym = form.Get("p0");
            var remitno = form.Get("p1");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    HA0001Repository repo = new HA0001Repository(DBWork);
                    IWorkbook workbook = getExcelWorkbook(data_ym, remitno, DBWork);

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        workbook.Write(memoryStream);
                        JCLib.Export.OutputFile(memoryStream, data_ym + "_" + repo.getAgenNoByRemitno(remitno) + "_001.xls");
                        workbook.Close();
                    }
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse sendMail(FormDataCollection form)
        {
            var data_ym = form.Get("p0");
            var remitno = form.Get("p1");

            if (remitno.Length > 0)
            {
                remitno = remitno.Substring(0, remitno.Length - 1);
            }
            string[] remitnos = remitno.Split(',');

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    foreach (string s in remitnos)
                    {
                        DBWork.BeginTransaction();

                        var repo = new HA0001Repository(DBWork);
                        string smtpServer = JCLib.Util.GetEnvSetting("SMTP_SERV");
                        SmtpClient smtp = new SmtpClient(smtpServer);
                        MailMessage mail = new MailMessage();

                        string sysFrom = JCLib.Util.GetEnvSetting("SMTP_SYS_FROM");
                        string sysName = JCLib.Util.GetEnvSetting("SMTP_SYS_NAME");
                        mail.From = new MailAddress(sysFrom, sysName);

                        string strTo = repo.getMailByRemitno(s);
                        mail.To.Add(new MailAddress(strTo));



                        // 寄信給廠商時附件給登入者
                        string strCc = repo.getMailByUser(User.Identity.Name);
                        if (strCc != "" && strCc != null)
                        {
                            mail.CC.Add(new MailAddress(strCc));
                            // 若SMTP_SYS_FROM沒有設定,則信件寄送位址填登入者信箱
                            if (sysFrom == "" || sysFrom == null)
                                sysFrom = strCc;
                        }

                        DataTable remitData = repo.getDataByRemitno(s);
                        string strRemitdate = Convert.ToString(remitData.Rows[0]["REMITDATE"]);
                        mail.Body = "親愛的廠商您好：<br>"
                                + "<br>"
                                + "本單位已於" + strRemitdate.Substring(0, 3) + "年" + Convert.ToInt32(strRemitdate.Substring(3, 2)) + "月" + Convert.ToInt32(strRemitdate.Substring(5, 2)) + "日 匯入貴廠商 " + remitData.Rows[0]["AGEN_NAMEC"] + " 收款帳戶訂單款項，<br>"
                                + "明細詳如附件，請查核。<br>"
                                + "<br>"
                                + "※※※本通知函為系統自動發送，僅回覆給寄件人將無法正確傳遞您的訊息※※※<br>";

                        mail.Subject = repo.getHospName() + "匯款通知單";
                        mail.IsBodyHtml = true;

                        // 處理附件EXCEL檔
                        IWorkbook workbook = getExcelWorkbook(data_ym, s, DBWork);
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            workbook.Write(memoryStream);
                            memoryStream.Position = 0;
                            ContentType ct = new ContentType(MediaTypeNames.Application.Octet);
                            Attachment attach = new Attachment(memoryStream, ct);
                            attach.ContentDisposition.FileName = data_ym + "_" + repo.getAgenNoByRemitno(s) + "_001.xls";
                            mail.Attachments.Add(attach);

                            // 信件送出
                            smtp.Send(mail);

                            workbook.Close();
                        }

                        if (strCc == "" || strCc == null)
                            strCc = sysFrom;

                        repo.MailLog(User.Identity.Name, strCc, strTo, Convert.ToString(remitData.Rows[0]["AGEN_NO"]), s, DBWork.ProcIP);
                        DBWork.Commit();
                    }
                }
                catch (Exception ex)
                {
                    DBWork.Rollback();
                    session.Result.msg = ex.Message;
                    throw;
                }
                return session.Result;
            }
        }

        public IWorkbook getExcelWorkbook(string data_ym, string remitno, UnitOfWork DBWork)
        {
            HA0001Repository repo = new HA0001Repository(DBWork);
            string data_ym_cht = ym_numToCht(data_ym);
            string data_y_cht = data_ym_cht.Split('/')[0];
            string data_m_cht = data_ym_cht.Split('/')[1];
            string hosp_name = repo.getHospName();
            string fileTitle = hosp_name + data_y_cht + "年度" + data_m_cht + "月份匯款通知單";

            DataTable remitData = repo.getDataByRemitno(remitno);

            IWorkbook workbook = new HSSFWorkbook();
            ISheet sheet = workbook.CreateSheet(fileTitle);

            sheet.SetColumnWidth(0, 25 * 256);
            sheet.SetColumnWidth(1, 25 * 256);
            sheet.SetColumnWidth(2, 25 * 256);
            sheet.SetColumnWidth(3, 25 * 256);
            sheet.SetColumnWidth(4, 25 * 256);

            // 文件標題
            ICellStyle titleStyle = workbook.CreateCellStyle();
            IFont titlefont = workbook.CreateFont();
            titlefont.FontName = "標楷體";
            titlefont.FontHeightInPoints = 18;
            titleStyle.SetFont(titlefont);

            // 廠商名稱標題
            ICellStyle agenStyle = workbook.CreateCellStyle();
            IFont agenfont = workbook.CreateFont();
            agenfont.FontName = "新細明體";
            agenfont.FontHeightInPoints = 16;
            agenfont.Color = IndexedColors.Red.Index;
            agenStyle.SetFont(agenfont);

            // 匯款日期
            ICellStyle notice1Style = workbook.CreateCellStyle();
            IFont notice1font = workbook.CreateFont();
            notice1font.FontName = "新細明體";
            notice1font.FontHeightInPoints = 12;
            notice1font.Color = IndexedColors.Red.Index;
            notice1Style.SetFont(notice1font);

            // [發票明細]、承辦人資訊
            ICellStyle notice2Style = workbook.CreateCellStyle();
            IFont notice2font = workbook.CreateFont();
            notice2font.FontName = "新細明體";
            notice2font.FontHeightInPoints = 12;
            notice2font.Color = IndexedColors.Blue.Index;
            notice2Style.SetFont(notice2font);

            // 查核資訊
            ICellStyle notice3Style = workbook.CreateCellStyle();
            IFont notice3font = workbook.CreateFont();
            notice3font.FontName = "新細明體";
            notice3font.FontHeightInPoints = 12;
            notice3font.Color = IndexedColors.Red.Index;
            notice3font.IsBold = true;
            notice3Style.SetFont(notice3font);

            // 內容標題
            ICellStyle contentTitleStyle = workbook.CreateCellStyle();
            IFont contenttitlefont = workbook.CreateFont();
            contenttitlefont.FontName = "新細明體";
            contenttitlefont.FontHeightInPoints = 12;
            contenttitlefont.IsBold = true;
            contentTitleStyle.SetFont(contenttitlefont);

            // 內容
            ICellStyle contentStyle = workbook.CreateCellStyle();
            IFont contentfont = workbook.CreateFont();
            contentfont.FontName = "新細明體";
            contentfont.FontHeightInPoints = 12;
            contentStyle.SetFont(contentfont);

            // 內容(數字)
            ICellStyle contentNumStyle = workbook.CreateCellStyle();
            IFont contentnumfont = workbook.CreateFont();
            contentnumfont.FontName = "新細明體";
            contentnumfont.FontHeightInPoints = 12;
            contentNumStyle.Alignment = HorizontalAlignment.Right;
            contentNumStyle.SetFont(contentnumfont);

            // row 0
            IRow exr = sheet.CreateRow(0);
            ICell exc = exr.CreateCell(1);
            exc.SetCellValue(fileTitle);
            exc.CellStyle = titleStyle;

            // row 2
            IRow exr2 = sheet.CreateRow(2);
            ICell exc2_0 = exr2.CreateCell(0);
            exc2_0.SetCellValue("受匯廠商：" + Convert.ToString(remitData.Rows[0]["AGEN_NAME"]));
            exc2_0.CellStyle = agenStyle;

            // row 3
            IRow exr3 = sheet.CreateRow(3);
            ICell exc3_0 = exr3.CreateCell(0);
            exc3_0.SetCellValue("傳送方式：電子郵件 (" + Convert.ToString(remitData.Rows[0]["EMAIL"]) + ")");
            exc3_0.CellStyle = contentStyle;

            // row 4
            string strRemitdate = Convert.ToString(remitData.Rows[0]["REMITDATE"]);
            IRow exr4 = sheet.CreateRow(4);
            ICell exc4_0 = exr4.CreateCell(0);
            exc4_0.SetCellValue("匯款日期：" + strRemitdate.Substring(0, 3) + "年" + Convert.ToInt32(strRemitdate.Substring(3, 2)) + "月" + Convert.ToInt32(strRemitdate.Substring(5, 2)) + "日");
            exc4_0.CellStyle = notice1Style;

            // row 5
            IRow exr5 = sheet.CreateRow(5);
            ICell exc5_0 = exr5.CreateCell(0);
            exc5_0.SetCellValue("匯款單編號：" + remitno);
            exc5_0.CellStyle = contentStyle;

            // row 6
            IRow exr6 = sheet.CreateRow(6);
            ICell exc6_0 = exr6.CreateCell(0);
            exc6_0.SetCellValue("列表日期：" + (DateTime.Now.Year - 1911) + "年" + DateTime.Now.Month + "月" + DateTime.Now.Day + "日");
            exc6_0.CellStyle = contentStyle;

            // row 8
            IRow exr8 = sheet.CreateRow(8);
            ICell exc8_0 = exr8.CreateCell(0);
            exc8_0.SetCellValue("[發票明細]");
            exc8_0.CellStyle = notice2Style;

            // row 9
            IRow exr9 = sheet.CreateRow(9);
            ICell exc9_0 = exr9.CreateCell(0);
            exc9_0.SetCellValue("------------------------------------------------------------------------------------------------------------------------------------------------------");
            exc9_0.CellStyle = contentStyle;

            // row 10
            IRow exr10 = sheet.CreateRow(10);
            ICell exc10_0 = exr10.CreateCell(0);
            exc10_0.SetCellValue("發票號碼");
            exc10_0.CellStyle = contentTitleStyle;
            ICell exc10_1 = exr10.CreateCell(1);
            exc10_1.SetCellValue("發票金額");
            exc10_1.CellStyle = contentTitleStyle;
            ICell exc10_2 = exr10.CreateCell(2);
            exc10_2.SetCellValue("折讓金額");
            exc10_2.CellStyle = contentTitleStyle;

            // row 11+
            DataTable invoiceData = repo.getInvoiceByRemitno(remitno);
            int ri = 0;
            int ci = 0;
            double amountSum = 0;
            double rebateSum = 0;
            foreach (DataRow dr in invoiceData.Rows)
            {
                IRow exrTbr = sheet.CreateRow(11 + ri);
                foreach (object v in dr.ItemArray)
                {
                    ICell excTbc = exrTbr.CreateCell(ci);
                    excTbc.SetCellValue(v.ToString());
                    if (ci == 0)
                        excTbc.CellStyle = contentStyle; // 發票號碼
                    else
                        excTbc.CellStyle = contentNumStyle; // 發票金額&折讓金額
                    if (ci == 1)
                        amountSum += Convert.ToDouble(v);
                    else if (ci == 2)
                        rebateSum += Convert.ToDouble(v);

                    ci++;
                }
                ri++;
                ci = 0;
            }

            IRow exrSplitLine1 = sheet.CreateRow(11 + ri);
            ICell exrSplitLine1_0 = exrSplitLine1.CreateCell(0);
            exrSplitLine1_0.SetCellValue("------------------------------------------------------------------------------------------------------------------------------------------------------");
            exrSplitLine1_0.CellStyle = contentStyle;
            ri++;

            // row 總計
            IRow exrS = sheet.CreateRow(11 + ri);
            ICell excS_0 = exrS.CreateCell(0);
            excS_0.SetCellValue("發票張數：" + invoiceData.Rows.Count);
            excS_0.CellStyle = notice2Style;
            ICell excS_1 = exrS.CreateCell(1);
            excS_1.SetCellValue("發票總金額：" + amountSum);
            excS_1.CellStyle = notice2Style;
            ICell excS_2 = exrS.CreateCell(2);
            excS_2.SetCellValue("折讓總金額：" + rebateSum);
            excS_2.CellStyle = notice2Style;
            ri++;

            IRow exrSplitLine2 = sheet.CreateRow(11 + ri);
            ICell exrSplitLine2_0 = exrSplitLine2.CreateCell(0);
            exrSplitLine2_0.SetCellValue("------------------------------------------------------------------------------------------------------------------------------------------------------");
            exrSplitLine2_0.CellStyle = contentStyle;
            ri++;

            // row 提示訊息
            IRow exrNotice = sheet.CreateRow(11 + ri);
            ICell exrNotice_0 = exrNotice.CreateCell(0);
            exrNotice_0.SetCellValue("若發票金額為負數表示為退貨");
            exrNotice_0.CellStyle = contentStyle;
            ri++; ri++;

            // row 銀行名稱/帳戶/金額(title)
            IRow exrBNTitle = sheet.CreateRow(11 + ri);
            ICell exrBNTitle_0 = exrBNTitle.CreateCell(0);
            exrBNTitle_0.SetCellValue("銀行名稱");
            exrBNTitle_0.CellStyle = contentTitleStyle;
            ICell exrBNTitle_2 = exrBNTitle.CreateCell(2);
            exrBNTitle_2.SetCellValue("銀行帳戶");
            exrBNTitle_2.CellStyle = contentTitleStyle;
            ICell exrBNTitle_3 = exrBNTitle.CreateCell(3);
            exrBNTitle_3.SetCellValue("匯款金額");
            exrBNTitle_3.CellStyle = contentTitleStyle;
            ri++;

            // row 銀行名稱/帳戶/金額(content)
            IRow exrBNContent = sheet.CreateRow(11 + ri);
            ICell exrBNContent_0 = exrBNContent.CreateCell(0);
            exrBNContent_0.SetCellValue(Convert.ToString(remitData.Rows[0]["BANK"]));
            exrBNContent_0.CellStyle = contentStyle;
            ICell exrBNContent_2 = exrBNContent.CreateCell(2);
            exrBNContent_2.SetCellValue(Convert.ToString(remitData.Rows[0]["AGEN_ACC"]));
            exrBNContent_2.CellStyle = contentStyle;
            ICell exrBNContent_3 = exrBNContent.CreateCell(3);
            exrBNContent_3.SetCellValue(Convert.ToString(remitData.Rows[0]["REMIT"]));
            exrBNContent_3.CellStyle = contentNumStyle;
            ri++; ri++;

            // row 結尾L1
            IRow exrNoticeLine1 = sheet.CreateRow(11 + ri);
            ICell exrNoticeLine1_0 = exrNoticeLine1.CreateCell(0);
            exrNoticeLine1_0.SetCellValue("敬啟者：");
            exrNoticeLine1_0.CellStyle = notice2Style;
            ri++;

            // row 結尾L2
            string strRemitdateMCht = ym_numToCht(strRemitdate.Substring(0, 5));
            string strRemitdateDCht = getChtChar(strRemitdate.Substring(5, 1), 10) + getChtChar(strRemitdate.Substring(6, 1));
            IRow exrNoticeLine2 = sheet.CreateRow(11 + ri);
            ICell exrNoticeLine2_0 = exrNoticeLine2.CreateCell(0);
            exrNoticeLine2_0.SetCellValue("本單位已於 " + strRemitdateMCht.Split('/')[0] + "年" + strRemitdateMCht.Split('/')[1] + "月" + strRemitdateDCht + "日 匯入貴戶帳號 " + Convert.ToString(remitData.Rows[0]["AGEN_ACC"]) + " ,匯款金額 " + Convert.ToString(remitData.Rows[0]["REMIT"]) + " 元, 請查核!!");
            exrNoticeLine2_0.CellStyle = notice3Style;
            ri++;

            // row 結尾L3
            IRow exrNoticeLine3 = sheet.CreateRow(11 + ri);
            ICell exrNoticeLine3_0 = exrNoticeLine3.CreateCell(0);
            exrNoticeLine3_0.SetCellValue("如有疑問請洽本院承辦人 " + Convert.ToString(remitData.Rows[0]["ACC_CONTACT"]) + " 電話 " + Convert.ToString(remitData.Rows[0]["ACC_TEL"]));
            exrNoticeLine3_0.CellStyle = notice2Style;
            ri++;

            // row 結尾L4
            IRow exrNoticeLine4 = sheet.CreateRow(11 + ri);
            ICell exrNoticeLine4_0 = exrNoticeLine4.CreateCell(0);
            exrNoticeLine4_0.SetCellValue("本院地址:" + Convert.ToString(remitData.Rows[0]["HOSP_ADDR"]) + Convert.ToString(remitData.Rows[0]["ACC_ROOM"]) + " (" + hosp_name + ")");
            exrNoticeLine4_0.CellStyle = notice2Style;
            ri++;

            // row 結尾L5
            IRow exrNoticeLine5 = sheet.CreateRow(11 + ri);
            ICell exrNoticeLine5_0 = exrNoticeLine5.CreateCell(0);
            exrNoticeLine5_0.SetCellValue("附言:" + Convert.ToString(remitData.Rows[0]["XFRMEMO"]));
            exrNoticeLine5_0.CellStyle = notice3Style;
            ri++; ri++;

            // row 結尾L6
            IRow exrNoticeLine6 = sheet.CreateRow(11 + ri);
            ICell exrNoticeLine6_0 = exrNoticeLine6.CreateCell(0);
            exrNoticeLine6_0.SetCellValue("備註:");
            exrNoticeLine6_0.CellStyle = notice3Style;

            return workbook;
        }

        [HttpPost]
        public ApiResponse calcAmtMsg(FormDataCollection form)
        {
            var data_ym = form.Get("DATA_YM");
            var agen = form.Get("AGEN");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new HA0001Repository(DBWork);
                    DataTable dt = repo.calcAmtMsg(data_ym, agen);
                    string vSUM_REMIT = $"{Convert.ToInt32(dt.Rows[0]["SUM_REMIT"].ToString()):n0}"; // 處理千位的逗號
                    session.Result.success = true;
                    session.Result.msg = vSUM_REMIT;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse ChkQuery(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    HA0001Repository repo = new HA0001Repository(DBWork);
                    // 轉入該年月資料
                    //if (repo.CheckExistsData(p0) == false)
                    //{
                    
                    // 不論是否檢查通過,資料都要轉入
                    string hospCode = repo.GetHospCode();
                    repo.ImportData(p0, hospCode, User.Identity.Name, DBWork.ProcIP);
                    // 更新匯款金額
                    repo.updateRemit(p0);

                    // 匯入後檢查廠商是否有銀行代碼或銀行帳號,是否有資料應付金額低於匯費+手續費
                    IEnumerable<HA0001> chkResult = repo.GetChkResult(p0);
                    if (chkResult.Count() > 0)
                    {
                        session.Result.success = true;
                        session.Result.msg = "N"; // 有不通過的廠商為N,前端顯示不通過的廠商清單
                        session.Result.etts = chkResult;
                    }
                    else
                    {
                        session.Result.success = true;
                        session.Result.msg = "Y"; // 檢核通過為Y
                    }
                    //else
                    DBWork.Commit();
                    // }
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }

        public string ym_numToCht(string ymNum)
        {
            string chtYYY = getChtChar(ymNum.Substring(0, 1), 100) + getChtChar(ymNum.Substring(1, 1), 10) + getChtChar(ymNum.Substring(2, 1));
            string chtMM = getChtChar(Convert.ToInt32(ymNum.Substring(3, 2)).ToString());

            return chtYYY + "/" + chtMM;
        }

        public string getChtChar(string numChar, int dCnt = 0)
        {
            string rtnStr = "";
            switch (numChar)
            {
                case "0":
                    if (dCnt != 0)
                        rtnStr = "零";
                    break;
                case "1":
                    rtnStr = "一";
                    break;
                case "2":
                    rtnStr = "二";
                    break;
                case "3":
                    rtnStr = "三";
                    break;
                case "4":
                    rtnStr = "四";
                    break;
                case "5":
                    rtnStr = "五";
                    break;
                case "6":
                    rtnStr = "六";
                    break;
                case "7":
                    rtnStr = "七";
                    break;
                case "8":
                    rtnStr = "八";
                    break;
                case "9":
                    rtnStr = "九";
                    break;
                case "10":
                    rtnStr = "十";
                    break;
                case "11":
                    rtnStr = "十一";
                    break;
                case "12":
                    rtnStr = "十二";
                    break;
            }

            if (numChar != "0")
            {
                if (dCnt == 10)
                    rtnStr += "十";
                else if (dCnt == 100)
                    rtnStr += "百";
            }

            return rtnStr;
        }

        static string fileNameFilter(string fileName, string extention)
        {
            string allowlist = @"^[\u4e00-\u9fa5_.\-a-zA-Z0-9]+$";
            fileName = fileName.Trim();
            Regex pattern = new Regex(allowlist);
            if (pattern.IsMatch(fileName))
            {
                return fileName;
            }
            else
            {
                return "downloadFile" + extention;
            }
        }
    }
}