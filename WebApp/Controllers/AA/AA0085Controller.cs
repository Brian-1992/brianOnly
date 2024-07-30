using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using System;
using System.Web;
using System.Data;
using System.IO;
using System.Text;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace WebApp.Controllers.AA
{
    public class AA0085Controller : SiteBase.BaseApiController
    {

        [HttpPost]
        public ApiResponse GetAgenCombo()
        {

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0085Repository repo = new AA0085Repository(DBWork);
                    session.Result.etts = repo.GetAgenCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }


        [HttpPost]
        public ApiResponse Wh_NoComboGet()
        {

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0085Repository repo = new AA0085Repository(DBWork);
                    session.Result.etts = repo.Wh_NoComboGet();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse All(FormDataCollection form)
        {
            var WH_NO = form.Get("P0");
            var ExportType = form.Get("P1");
            var YYYMM = form.Get("P2");
            var AGEN_NO = form.Get("P3");
            var IsTCB = form.Get("IsTCB");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = "[" + form.Get("sort").ToString().Replace("[", "").Replace("]", "") + ", {\"property\":\"MEMO\",\"direction\":\"ASC\"}]";

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0085Repository(DBWork);
                    // 每次先更新價格資料
                    //repo.UpdatePrice(WH_NO, YYYMM, User.Identity.Name, DBWork.ProcIP);
                    session.Result.etts = repo.GetAll(WH_NO, ExportType, YYYMM, AGEN_NO, IsTCB, page, limit, sorters);
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
            var WH_NO = form.Get("P0");
            var ExportType = form.Get("P1");
            var YYYMM = form.Get("P2");
            var AGEN_NO = form.Get("P3");
            var IsTCB = form.Get("IsTCB");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0085Repository repo = new AA0085Repository(DBWork);
                    string dateTime = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MM");
                    JCLib.Excel.Export("藥品結報月報表" + "_" + DateTime.Now.ToString("yyyyMMddhhmm") + ".xls", repo.GetExcel(WH_NO, ExportType, YYYMM, AGEN_NO, IsTCB));
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
            var WH_NO = form.Get("P0");
            var ExportType = form.Get("P1");
            var YYYMM = form.Get("P2");
            var AGEN_NO = form.Get("P3");
            var IsTCB = form.Get("IsTCB");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0085Repository repo = new AA0085Repository(DBWork);
                    DataTable result = new DataTable();

                    result = repo.GetTxtData(WH_NO, ExportType, YYYMM, AGEN_NO, IsTCB);
                    foreach (DataColumn col in result.Columns)
                    {
                        col.ReadOnly = false;
                    }

                    List<PH_BANK_FEE> bank_fees = repo.GetPhBankFee().ToList();

                    // 非合庫，扣匯費，超過50000000須拆成多筆資料
                    if (IsTCB == "1")
                    {
                        result = SplitData(result, bank_fees, IsTCB);
                    }

                    ExportTxt(YYYMM, ExportType, IsTCB, result);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        public DataTable SplitData(DataTable dataTable, List<PH_BANK_FEE> bank_fees, string IsTCB)
        {

            DataTable temp = new DataTable();
            temp.Columns.Add("agen_no", typeof(string));
            temp.Columns.Add("銀行帳號", typeof(string));
            temp.Columns.Add("匯款金額", typeof(string));
            temp.Columns.Add("銀行代碼", typeof(string));
            temp.Columns.Add("分行代碼", typeof(string));
            temp.Columns.Add("統一編號", typeof(string));
            temp.Columns.Add("廠商名稱", typeof(string));

            foreach (DataRow ori_row in dataTable.Rows)
            {
                int tot = int.Parse(ori_row["匯款金額"].ToString());

                // 匯款金額小於 50000000
                if (tot <= 50000000)
                {
                    // 取得匯費
                    int fee = bank_fees.Where(x => x.CASHTO >= tot).Where(x => x.CASHFROM <= tot).Select(x => x.FEE).FirstOrDefault();
                    // 以扣除匯費後的金額修改原資料
                    ori_row["匯款金額"] = (tot - fee).ToString();
                    // 將資料新增至新的DataTable
                    temp.Rows.Add(ori_row.ItemArray);
                    continue;
                }
                // 計算需要幾筆資料
                int counts = (tot / 50000000) + 1;
                int ori_tot = tot;
                // 第一筆金額預設為50000000
                int temp_amount = 50000000;
                for (int j = 0; j < counts; j++)
                {
                    DataRow row = temp.NewRow();
                    for (int k = 0; k < dataTable.Columns.Count; k++)
                    {
                        // 2：匯款金額，非匯款金額欄位不須變動，直接指定
                        if (k != 2)
                        {
                            row[k] = ori_row[k];
                        }
                        else
                        {
                            // 取得匯費
                            int fee = bank_fees.Where(x => x.CASHTO >= temp_amount).Where(x => x.CASHFROM <= temp_amount).Select(x => x.FEE).FirstOrDefault();
                            // 匯款金額 = 預設金額 - 扣除匯費後的金額
                            row[k] = (temp_amount - fee).ToString();
                            // 剩餘金額 = 總匯款金額 - 預設金額
                            tot = tot - temp_amount;
                            // 剩餘金額 > 50000000，以50000000作為預設金額；剩餘金額 <= 50000000，以自己作為預設金額
                            if (tot > 50000000)
                            {
                                temp_amount = 50000000;
                            }
                            else
                            {
                                temp_amount = tot;
                            }
                        }
                    }
                    // 將資料新增至新的DataTable
                    temp.Rows.Add(row);
                }
            }

            return temp;
        }
        public static void ExportTxt(string YYYMM, string ExportType, string IsTCB, DataTable dataTable, string[] columnName = null)
        {
            string fileName = "";
            int parYYY = 0;
            int parMM = 0;
            string parYYYMM = "";

            // 避免直接將傳入參數YYYMM直接加入檔名以應對Header Manipulation問題
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0085Repository repo = new AA0085Repository(DBWork);
                    parYYYMM = repo.ChkChtDateStr(fileNameFilter(YYYMM, "").Substring(0, 5));
                }
                catch
                {
                    throw;
                }
            }

            if (Convert.ToInt32(parYYYMM.Substring(0, 3)) > 0)
            {
                parYYY = Convert.ToInt32(parYYYMM.Substring(0, 3));
            }
            if (Convert.ToInt32(parYYYMM.Substring(3, 2)) >= 1 && Convert.ToInt32(parYYYMM.Substring(3, 2)) <= 12)
            {
                parMM = Convert.ToInt32(parYYYMM.Substring(3, 2));
            }

            //IsTCB == "0" 合庫, IsTCB == "1" 非合庫
            if (IsTCB == "0")
                fileName = "合庫-" + parYYY.ToString().PadLeft(3, '0') + "年" + parMM.ToString().PadLeft(2, '0') + "月份藥品合約品項結報轉檔";
            else if (IsTCB == "1")
                fileName = "非合庫-" + parYYY.ToString().PadLeft(3, '0') + "年" + parMM.ToString().PadLeft(2, '0') + "月份藥品合約品項結報轉檔";

            // Y '零購' , N '合約' 
            if (ExportType == "Y")
                fileName += "-零購.txt";
            else
                fileName += "-合約.txt";

            //IsTCB = 0 合庫  
            //IsTCB = 1 非合庫
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
                            if (IsTCB == "0")
                            {
                                //顯示合庫帳號
                                line += "Z2" + dr[dc].ToString().PadLeft(13, '0');
                            }
                            else if (IsTCB == "1")
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
                            if (IsTCB == "1")
                            {
                                //非合庫顯示銀行別跟分行別
                                line += dr[dc].ToString();
                            }
                            break;
                        case "統一編號":
                            if (IsTCB == "0")
                            {
                                //合庫顯示統一編號(統編8碼，不足往左補空白，再往右補25空白[33-8=25])
                                // line += dr[dc].ToString().PadLeft(8, ' ').PadRight(33, ' '); // 藥品不使用統編
                                line += "".PadLeft(8, ' ').PadRight(33, ' ');
                                //加"883"再往右補8個空白
                                // line += "883".PadRight(11, ' '); // 藥品不使用883
                                line += "   ".PadRight(11, ' ');
                                //加上"QB565137"
                                line += "QB565137";
                            }
                            break;
                        case "廠商名稱":
                            if (IsTCB == "1")
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

        #region 暫存
        //[HttpPost]
        //public ApiResponse Excel(FormDataCollection form)
        //{
        //    var ExportType = form.Get("ExportType");
        //    var YYYMM = form.Get("YYYMM");
        //    var AGEN_NO = form.Get("AGEN_NO");
        //    //var path = form.Get("path");


        //    using (WorkSession session = new WorkSession())
        //    {
        //        UnitOfWork DBWork = session.UnitOfWork;
        //        try
        //        {
        //            AA0085Repository repo = new AA0085Repository(DBWork);

        //            DataSet ds = repo.GetExcel(ExportType, YYYMM, AGEN_NO);

        //            ExportZip(ds);
        //        }
        //        catch
        //        {
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //}

        //private static void OutputFile(MemoryStream memoryStream, string fileName)
        //{
        //    HttpResponse res = HttpContext.Current.Response;

        //    res.BufferOutput = false;
        //    res.Clear();
        //    res.ClearHeaders();
        //    res.HeaderEncoding = System.Text.Encoding.Default;
        //    res.ContentType = "application/octet-stream";

        //    res.AddHeader("Content-Disposition",
        //                "attachment; filename=" + HttpUtility.UrlEncode(fileName, System.Text.Encoding.UTF8));
        //    res.BinaryWrite(memoryStream.ToArray());
        //    res.End();
        //}

        //public  string ToCSV(DataTable table)
        //{
        //    var result = new StringBuilder();

        //    foreach (DataRow row in table.Rows)
        //    {
        //        for (int i = 0; i < table.Columns.Count; i++)
        //        {
        //            result.Append(row[i].ToString());
        //            result.Append(i == table.Columns.Count - 1 ? "\n" : ",");
        //        }
        //    }

        //    return result.ToString();
        //}


        //private  void ExportZip(DataSet dataSet)
        //{
        //    if (dataSet == null) return;
        //    DataTable aa = new DataTable();
        //    if (HttpContext.Current == null) return;

        //    using (var memoryStream = new MemoryStream())
        //    {
        //        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        //        {
        //            int i = 0;
        //            foreach (DataTable dataTable in dataSet.Tables)
        //            {
        //                var zipArchiveEntry = archive.CreateEntry(dataTable.TableName, CompressionLevel.Fastest);

        //                using (var zipStream = zipArchiveEntry.Open())
        //                {
        //                    var bytes = Encoding.Default.GetBytes(ToCSV(dataTable));
        //                    zipStream.Write(bytes, 0, bytes.Length);
        //                }

        //                i++;
        //            }
        //        }

        //        OutputFile(memoryStream, "藥品衛材結報月報表.zip");
        //    }
        //}
        #endregion
    }
}