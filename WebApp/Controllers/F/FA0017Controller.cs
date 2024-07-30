using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.F;
using WebApp.Models;
using System;
using System.Data;
using System.Web;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace WebApp.Controllers.F
{
    public class FA0017Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0017Repository(DBWork);
                    //計算今天民國年月
                    var TWN_YearMonth = (DateTime.Now.Year - 1911).ToString() + (DateTime.Now.Month).ToString("00");
                    //月結年月==當月須跑SP，否則直接查詢
                    if (TWN_YearMonth == p1)
                    {
                        var str_UserID = DBWork.UserInfo.UserId;
                        var str_UserIP = DBWork.ProcIP;
                        //查詢前呼叫SP檢核，回傳碼為000才繼續
                        if (repo.SearchCheckProcedure(p0, p1, str_UserID, str_UserIP) == "000")
                        {
                            session.Result.etts = repo.GetAll(p0, p1, p2, p3);
                        }
                    }
                    else
                    {
                        session.Result.etts = repo.GetAll(p0, p1, p2, p3);
                    }
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        public ApiResponse All_2(FormDataCollection form)
        {
            var p0 = form.Get("p0"); //物料分類
            var p1 = form.Get("p1"); //進貨日期起
            var p2 = form.Get("p2"); //進貨日期迄
            var p3 = form.Get("p3"); //中信或院內

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0017Repository(DBWork);
                    session.Result.etts = repo.GetAll_2(p0, p1, p2, p3);
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
            var p0 = form.Get("p0");            //物料代碼
            var p0_Name = form.Get("p0_Name");  //物料名稱
            var p1 = form.Get("p1").Trim();     //月結年月
            var p2 = form.Get("p2").Trim();     //合約種類
            var p3 = form.Get("p3").Trim();     //是否為合庫

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0017Repository repo = new FA0017Repository(DBWork);
                    DataTable result = new DataTable();

                    //計算今天民國年月
                    var TWN_YearMonth = (DateTime.Now.Year - 1911).ToString() + (DateTime.Now.Month).ToString("00");
                    //月結年月==當月須跑SP，否則直接匯出
                    if (TWN_YearMonth == p1)
                    {
                        var str_UserID = DBWork.UserInfo.UserId;
                        var str_UserIP = DBWork.ProcIP;
                        //匯出前呼叫SP檢核，回傳碼為000才繼續
                        if (repo.SearchCheckProcedure(p0, p1, str_UserID, str_UserIP) == "000")
                        {
                            result = repo.GetTxt(p0, p1, p2, p3);
                        }
                    }
                    else
                    {
                        result = repo.GetTxt(p0, p1, p2, p3);
                    }

                    ExportTxt(p1, p2, p3, result);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1").Trim();
            var p2 = form.Get("p2").Trim();
            var p3 = form.Get("p3").Trim();

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0017Repository repo = new FA0017Repository(DBWork);
                    DataTable result = new DataTable();
                    DataTable dtItems = new DataTable();
                    //計算今天民國年月
                    var TWN_YearMonth = (DateTime.Now.Year - 1911).ToString() + (DateTime.Now.Month).ToString("00");
                    //月結年月==當月須跑SP，否則直接匯出
                    if (TWN_YearMonth == p1)
                    {
                        var str_UserID = DBWork.UserInfo.UserId;
                        var str_UserIP = DBWork.ProcIP;
                        //匯出前呼叫SP檢核，回傳碼為000才繼續
                        if (repo.SearchCheckProcedure(p0, p1, str_UserID, str_UserIP) == "000")
                        {
                            result = repo.GetExcel(p0, p1, p2, p3);
                            dtItems.Merge(result);
                        }
                    }
                    else
                    {
                        result = repo.GetExcel(p0, p1, p2, p3);
                        dtItems.Merge(result);
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
        [HttpPost]
        public ApiResponse Excel2(FormDataCollection form)
        {
            var p0 = form.Get("p0"); //物料分類
            var p1 = form.Get("p1"); //進貨日期起
            var p2 = form.Get("p2"); //進貨日期迄
            var p3 = form.Get("p3"); //中信或院內
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0017Repository repo = new FA0017Repository(DBWork);
                    DataTable result = new DataTable();
                    DataTable dtItems = new DataTable();
                    result = repo.GetExcel2(p0, p1, p2, p3);
                    dtItems.Merge(result);
                    JCLib.Excel.Export(form.Get("FN"), dtItems);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //非合庫
        //*照順序每欄位字元數應該是 4+14+11+2(小數二位)+7(銀行別+分行別)+60(廠商名)+”三軍總醫院”(固定)
        // //v1流水號 4碼
        // Position := i;
        // if length(inttostr(i)) < 4 then
        // v1:=StringOfChar('0', (4-length(inttostr(i))))+inttostr(i);
        // //v2銀行帳號 14碼
        // v2:=DM1.QR1.FieldByName('agen_acc').asstring;
        // if length(trim(v2)) < 14 then
        // v2:=StringOfChar('0', (14-length(trim(v2))))+v2;
        // //v3匯款金額 11碼
        //  v3:=inttostr(DM1.QR1.FieldByName('totalsum').asinteger-DM1.QR1.FieldByName('txfee').asinteger);;
        // if length(trim(v3)) < 11 then
        //  v3:=StringOfChar('0', (11-length(trim(v3))))+v3;
        // //v4匯款金額小數二位
        // v4:='00';
        // //v5單位代號=銀行別+局號
        // v5:=DM1.QR1.FieldByName('agen_bank').asstring+DM1.QR1.FieldByName('agen_sub').asstring;
        // //v6廠商名稱
        // v6:=DM1.QR1.FieldByName('agen_namec').asstring;
        //// showmessage(inttostr(length(trimright(v6))));
        //// showmessage(StringOfChar('0',(60-length(trimright(v6)))));
        // if length(trimright(v6)) < 60 then
        // v6:=v6+StringOfChar(' ', (60-length(trimright(v6))));
        // v7:='三軍總醫院'+StringOfChar(' ',30);
        // v8:=StringOfChar(' ',72); //x(40)+x(15)+x(17) 空白碼
        // Writeln(F1, v1, v2, v3, v4, v5, v6, v7, v8);
        public static void ExportTxt(string YYYMM, string ContractType, string IsTBC, DataTable dataTable, string[] columnName = null)
        {
            string fileName = "";
            string parYYYMM = "";

            // 避免直接將傳入參數YYYMM直接加入檔名以應對Header Manipulation問題
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0017Repository repo = new FA0017Repository(DBWork);
                    parYYYMM = repo.ChkChtDateStr(fileNameFilter(YYYMM, "").Substring(0, 5));
                }
                catch
                {
                    throw;
                }
            }

            //合約
            if (ContractType == "0" || ContractType == "")
            {
                //合庫
                if (IsTBC == "0")
                {
                    fileName = "合約合庫-中央庫房衛材類" + parYYYMM + "採購月結報表.txt";
                }
                //非合庫
                else if (IsTBC == "1")
                {
                    fileName = "合約非合庫-中央庫房衛材類" + parYYYMM + "採購月結報表.txt";
                }
            }
            //非合約
            else if (ContractType == "2")
            {
                //合庫
                if (IsTBC == "0")
                {
                    fileName = "非合約合庫-中央庫房衛材類" + YYYMM.Substring(0, 5) + "採購月結報表.txt";
                }
                //非合庫
                else if (IsTBC == "1")
                {
                    fileName = "非合約非合庫-中央庫房衛材類" + YYYMM.Substring(0, 5) + "採購月結報表.txt";
                }
            }

            //IsTBC = 0 合庫  
            //IsTBC = 1 非合庫
            if (dataTable == null) return;

            if (HttpContext.Current == null) return;

            HttpResponse res = HttpContext.Current.Response;
            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms, Encoding.GetEncoding("big5"));
            string line = "";
            int Index = 1;
            Encoding utf81 = Encoding.GetEncoding("utf-8");
            Encoding big51 = Encoding.GetEncoding("big5");
            foreach (DataRow dr in dataTable.Rows)
            {
                foreach (DataColumn dc in dataTable.Columns)
                {
                    switch (dc.ColumnName)
                    {
                        case "銀行帳號":
                            if (IsTBC == "0")
                            {
                                //顯示合庫帳號
                                line += "Z2" + dr[dc].ToString().PadLeft(13, '0');
                            }
                            else if (IsTBC == "1")
                            {
                                //非合庫銀行帳號前加流水號，第1位起，共4位 
                                line += Index.ToString("0000");
                                //非合庫帳號長度不滿14向右靠，左補0， 第5位起，共14位
                                line += dr[dc].ToString().PadLeft(14, '0');
                            }

                            break;
                        case "匯款金額":
                            //整數11位，小數2位，第19位起，共13位
                            //匯款金額長度不滿11向左補0
                            line += dr[dc].ToString().PadLeft(11, '0');
                            //匯款金額向右補2個0
                            line += "00";
                            break;
                        case "銀行代碼":
                        case "分行代碼":
                            if (IsTBC == "1")
                            {
                                //非合庫顯示銀行別跟分行別
                                line += dr[dc].ToString();
                            }
                            break;
                        case "統一編號":
                            if (IsTBC == "0")
                            {
                                //合庫顯示統一編號(統編8碼，不足往左補空白，再往右補25空白[33-8=25])
                                line += dr[dc].ToString().PadLeft(8, ' ').PadRight(33, ' ');
                                //加"883"再往右補8個空白
                                line += "883".PadRight(11, ' ');
                                //加上"QB565137"
                                line += "QB565137";
                            }
                            break;
                        case "廠商名稱":
                            if (IsTBC == "1")
                            {
                                byte[] strUtf81 = utf81.GetBytes(dr[dc].ToString().Trim());
                                byte[] strBig51 = Encoding.Convert(utf81, big51, strUtf81);

                                char[] big5Chars1 = new char[big51.GetCharCount(strBig51, 0, strBig51.Length)];
                                big51.GetChars(strBig51, 0, strBig51.Length, big5Chars1, 0);
                                string tempString1 = new string(big5Chars1);
                                //非合庫顯示廠商名稱，且往右補空白直到長度為60，第39位起，共60位
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
                                strUtf81 = utf81.GetBytes("三軍總醫院");
                                strBig51 = Encoding.Convert(utf81, big51, strUtf81);

                                big5Chars1 = new char[big51.GetCharCount(strBig51, 0, strBig51.Length)];
                                big51.GetChars(strBig51, 0, strBig51.Length, big5Chars1, 0);
                                tempString1 = new string(big5Chars1);
                                //非合庫顯示"三軍總醫院"
                                line += tempString1 + "".PadRight(82, ' ');
                            }
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

        public static string fileNameFilter(string fileName, string extention)
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

        public static string TrimStringByByte(string theString, int lens)
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
    }
}