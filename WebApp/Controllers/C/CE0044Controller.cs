using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.C;
using WebApp.Models;
using System;
using System.Data;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using System.IO;
using System.Web;
using System.Linq;

namespace WebApp.Controllers.C
{
    public class CE0044Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");
            var p6 = form.Get("p6");
            var p7 = form.Get("p7");
            var p8 = form.Get("p8");
            var p9 = form.Get("p9");
            var p10 = form.Get("p10");
            var p11 = form.Get("p11");
            var p12 = form.Get("p12");
            var p13 = form.Get("p13");
            var p14 = form.Get("p14");
            var p15 = form.Get("p15");
            var p16 = form.Get("p16");
            var p17 = form.Get("p17");
            var p18 = form.Get("p18"); // (近6個月進貨)或(近6個月醫令耗用)或(庫量<>0)或(庫存=0且無作廢)
            var p19 = form.Get("p19"); // (期初庫存<>0)或(期初=0但有進出)
            var p20 = form.Get("p20"); // SQL邏輯待補充
            var p21 = form.Get("p21");
            var user = User.Identity.Name;
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0044Repository(DBWork);
                    session.Result.etts = repo.GetAllM(p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18, p19, user, p20, p21, page, limit, sorters);
                }
                catch (Exception e)
                {
                    var a = e.Message.ToString();
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse AllD(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");
            var p6 = form.Get("p6");
            var p7 = form.Get("p7");
            var p8 = form.Get("p8");
            var p9 = form.Get("p9");
            var p10 = form.Get("p10");
            var p11 = form.Get("p11");
            var p12 = form.Get("p12");
            var p13 = form.Get("p13");
            var p14 = form.Get("p14");
            var p15 = form.Get("p15");
            var p16 = form.Get("p16");
            var p17 = form.Get("p17");
            var p18 = form.Get("p18"); // (近6個月進貨)或(近6個月醫令耗用)或(庫量<>0)或(庫存=0且無作廢)
            var p19 = form.Get("p19"); // (期初庫存<>0)或(期初=0但有進出)
            var p20 = form.Get("p20"); // SQL邏輯待補充
            var p21 = form.Get("p21");
            var user = User.Identity.Name;
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0044Repository(DBWork);
                    session.Result.etts = repo.GetAllD(p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18, p19, user, p20, p21,  page, limit, sorters);
                }
                catch (Exception e)
                {
                    var a = e.Message.ToString();
                    throw;
                }
                return session.Result;
            }
        }
        
        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");
            var p6 = form.Get("p6");
            var p7 = form.Get("p7");
            var p8 = form.Get("p8");
            var p9 = form.Get("p9");
            var p10 = form.Get("p10");
            var p11 = form.Get("p11");
            var p12 = form.Get("p12");
            var p13 = form.Get("p13");
            var p14 = form.Get("p14");
            var p15 = form.Get("p15");
            var p16 = form.Get("p16");
            var p17 = form.Get("p17");
            var p18 = form.Get("p18"); // (近6個月進貨)或(近6個月醫令耗用)或(庫量<>0)或(庫存=0且無作廢)
            var p19 = form.Get("p19"); // (期初庫存<>0)或(期初=0但有進出)
            var p20 = form.Get("p20"); // SQL邏輯待補充
            var p21 = form.Get("p21");
            var HospCode = form.Get("hosp_code");
            var user = User.Identity.Name;
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0044Repository repo = new CE0044Repository(DBWork);
                    string dateTime = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MM");
                    DataTable data = repo.GetExcel(p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18, p19, HospCode, user,
                                                    p20, p21);
                    string fileName = "各單位盤點狀況查詢" + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";
                    if (data.Rows.Count > 0)
                    {
                        if (HospCode != "805" && (p1 != "3" && p1 != "4"))
                        {
                            data.Columns.Remove("單位申請基準量");
                            data.Columns.Remove("下月預計申請量");
                        }

                        var workbook = ExoprtToExcel(data);

                        //output
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            workbook.Write(memoryStream);
                            JCLib.Export.OutputFile(memoryStream, fileName);
                            workbook.Close();
                        }
                    }
                    //JCLib.Excel.Export("各單位盤點狀況查詢" + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls", repo.GetExcel(p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, user));
                }
                catch (Exception e)
                {
                    var a = e.Message.ToString();
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse CSV(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");
            var p6 = form.Get("p6");
            var p7 = form.Get("p7");
            var p8 = form.Get("p8");
            var p9 = form.Get("p9");
            var p10 = form.Get("p10");
            var p11 = form.Get("p11");
            var p12 = form.Get("p12");
            var p13 = form.Get("p13");
            var p14 = form.Get("p14");
            var p15 = form.Get("p15");
            var p16 = form.Get("p16");
            var p17 = form.Get("p17");
            var p18 = form.Get("p18"); // (近6個月進貨)或(近6個月醫令耗用)或(庫量<>0)或(庫存=0且無作廢)
            var p19 = form.Get("p19"); // (期初庫存<>0)或(期初=0但有進出)
            var p20 = form.Get("p20"); // SQL邏輯待補充
            var p21 = form.Get("p21");
            var HospCode = form.Get("hosp_code");
            var user = User.Identity.Name;
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0044Repository repo = new CE0044Repository(DBWork);
                    string dateTime = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MM");
                    DataTable data = repo.GetExcel(p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18, p19, HospCode, user,
                                            p20, p21);
                    string fileName = "各單位盤點狀況查詢" + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
                    if (data.Rows.Count > 0)
                    {
                        // fix oracle 舊版撈出中文名過長問題
                        data.Columns["p_c_qty"].ColumnName = "上月寄庫藥品買斷結存";
                        data.Columns["c_c_qty"].ColumnName = "本月寄庫藥品買斷結存";
                        data.Columns["no_w_p_i_dif"].ColumnName = "不含戰備上月結存價差";
                        data.Columns["no_w_p_i"].ColumnName = "不含戰備上月結存";
                        data.Columns["no_w_c_i"].ColumnName = "不含戰備本月應有結存量";
                        data.Columns["no_w_c_c"].ColumnName = "不含戰備本月盤存量";

                        if (HospCode != "805" && (p1 != "3" && p1 != "4"))
                        {
                            data.Columns.Remove("單位申請基準量");
                            data.Columns.Remove("下月預計申請量");
                        }
                    }
                    JCLib.Csv.Export(fileName, data);
                }
                catch (Exception e)
                {
                    var a = e.Message.ToString();
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
                if (data.Columns[i].ToString().ToLower() == "p_c_qty")
                {
                    row.CreateCell(i).SetCellValue("上月寄庫藥品買斷結存");
                }
                else if (data.Columns[i].ToString().ToLower() == "c_c_qty")
                {
                    row.CreateCell(i).SetCellValue("本月寄庫藥品買斷結存");
                }
                else if (data.Columns[i].ToString().ToLower() == "no_w_p_i_dif")
                {
                    row.CreateCell(i).SetCellValue("不含戰備上月結存價差");
                }
                else if (data.Columns[i].ToString().ToLower() == "no_w_p_i")
                {
                    row.CreateCell(i).SetCellValue("不含戰備上月結存");
                }
                else if (data.Columns[i].ToString().ToLower() == "no_w_c_i")
                {
                    row.CreateCell(i).SetCellValue("不含戰備本月應有結存量");
                }
                else if (data.Columns[i].ToString().ToLower() == "no_w_c_c")
                {
                    row.CreateCell(i).SetCellValue("不含戰備本月盤存量");
                }
                else
                {
                    row.CreateCell(i).SetCellValue(data.Columns[i].ToString());
                }
            }

            for (int i = 0; i < data.Rows.Count; i++)
            {
                row = sheet.CreateRow(1 + i);
                for (int j = 0; j < data.Columns.Count; j++)
                {
                    var columnName = data.Columns[j].ColumnName;

                    // 判斷該欄位是整數還是字串 做不同的處理 如果欄位總數有變動 要調整這邊
                    //if ((j > 5 && j < 36) || (j > 39 && j < 56))
                    if (IsNumericColumn(columnName))
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

        // 判斷欄位是否是數字的函數
        private static bool IsNumericColumn(string columnName)
        {
            string[] allColumns =
            {
                "盤存單位",
                "藥材代碼",
                "廠商代碼",
                "藥材名稱",
                "英文品名",
                "藥材單位",
                "單價",
                "優惠後單價",
                "上月單價",
                "上月優惠後單價",
                "包裝量",
                "上月結存",
                "進貨",
                "撥發入庫",
                "撥發出庫",
                "調撥入庫",
                "調撥出庫",
                "調帳入庫",
                "調帳出庫",
                "繳回入庫",
                "繳回出庫",
                "退貨量",
                "報廢量",
                "換貨入庫",
                "換貨出庫",
                "軍方消耗",
                "民眾消耗",
                "本月總消耗",
                "應有結存",
                "盤存量",
                "本月結存",
                "上月寄庫藥品買斷結存",
                "本月寄庫藥品買斷結存",
                "差異量",
                "結存金額",
                "優惠後結存金額",
                "差異金額",
                "優惠後差異金額",
                "藥材類別",
                "是否合約",
                "買斷寄庫",
                "是否戰備",
                "戰備存量",
                "期末比",
                "贈品數量",
                "上月戰備量",
                "上月是否戰備",
                "不含戰備上月結存",
                "單價差額",
                "不含戰備上月結存價差",
                "戰備本月價差",
                "不含戰備本月結存",
                "不含戰備本月應有結存量",
                "不含戰備本月盤存量",
                "不含贈品本月進貨",
                "退料入庫",
                "退料出庫",
                "單價5000元以上總消耗",
                "單價未滿5000元總消耗",
                "合約案號"
            };
            string[] numericColumns = {
                "單價",
                "優惠後單價",
                "上月單價",
                "上月優惠後單價",
                "包裝量",
                "上月結存",
                "進貨",
                "撥發入庫",
                "撥發出庫",
                "調撥入庫",
                "調撥出庫",
                "調帳入庫",
                "調帳出庫",
                "繳回入庫",
                "繳回出庫",
                "退貨量",
                "報廢量",
                "換貨入庫",
                "換貨出庫",
                "軍方消耗",
                "民眾消耗",
                "本月總消耗",
                "應有結存",
                "盤存量",
                "本月結存",
                "上月寄庫藥品買斷結存",
                "本月寄庫藥品買斷結存",
                "差異量",
                "結存金額",
                "優惠後結存金額",
                "差異金額",
                "優惠後差異金額",
                "戰備存量",
                "期末比",
                "贈品數量",
                "上月戰備量",
                "不含戰備上月結存",
                "單價差額",
                "不含戰備上月結存價差",
                "戰備本月價差",
                "不含戰備本月結存",
                "不含戰備本月應有結存量",
                "不含戰備本月盤存量",
                "不含贈品本月進貨",
                "退料入庫",
                "退料出庫",
                "單價5000元以上總消耗",
                "單價未滿5000元總消耗",
                "P_C_QTY",
                "C_C_QTY",
                "NO_W_P_I_DIF",
                "NO_W_P_I",
                "NO_W_C_I",
                "NO_W_C_C",
                "p_c_qty",
                "c_c_qty",
                "no_w_p_i_dif",
                "no_w_p_i",
                "no_w_c_i",
                "no_w_c_c",
                "軍院內退料",
                "民院內退料",
                "本月總院內退料",
                "平行調撥量",
                "折讓金額"
            }; //新增四個803台中醫院的消耗結存明細欄位
            return numericColumns.Contains(columnName);
        }

        [HttpPost]  // [匯出消耗結存表] 805(花蓮)專用
        public ApiResponse Excel_805(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");
            var p6 = form.Get("p6");
            var p7 = form.Get("p7");
            var p8 = form.Get("p8");
            var p9 = form.Get("p9");
            var p10 = form.Get("p10");
            var p11 = form.Get("p11");
            var p12 = form.Get("p12");
            var p13 = form.Get("p13");
            var p14 = form.Get("p14");
            var p15 = form.Get("p15");
            var p16 = form.Get("p16");
            var p17 = form.Get("p17");
            var p18 = form.Get("p18"); // (近6個月進貨)或(近6個月醫令耗用)或(庫量<>0)或(庫存=0且無作廢)
            var p19 = form.Get("p19"); // (期初庫存<>0)或(期初=0但有進出)
            var p20 = form.Get("p20"); // SQL邏輯待補充
            var p21 = form.Get("p21");
            var HospCode = form.Get("hosp_code");
            var user = User.Identity.Name;
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0044Repository repo = new CE0044Repository(DBWork);
                    string dateTime = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MM");
                    DataTable data = repo.GetExcel_805(p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18, p19, HospCode, user, p20, p21);
                    string fileName = "消耗結存表" + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";
                    if (data.Rows.Count > 0)
                    {
                        //if (HospCode != "805")
                        //{
                        //    data.Columns.Remove("單位申請基準量");
                        //    data.Columns.Remove("下月預計申請量");
                        //}

                        var workbook = ExportToExcel_805(data);

                        //output
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            workbook.Write(memoryStream);
                            JCLib.Export.OutputFile(memoryStream, fileName);
                            workbook.Close();
                        }
                    }
                    //JCLib.Excel.Export("各單位盤點狀況查詢" + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls", repo.GetExcel(p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, user));
                }
                catch (Exception e)
                {
                    var a = e.Message.ToString();
                    throw;
                }
                return session.Result;
            }
        }

        //製造EXCEL的內容
        public XSSFWorkbook ExportToExcel_805(DataTable data)
        {
            var wb = new XSSFWorkbook();
            var sheet = (XSSFSheet)wb.CreateSheet("Sheet1");

            IRow row = sheet.CreateRow(0);
            for (int i = 0; i < data.Columns.Count; i++)
            {
                //if (data.Columns[i].ToString().ToLower() == "p_c_qty")
                //{
                //    row.CreateCell(i).SetCellValue("上月寄庫藥品買斷結存");
                //}
                //else if (data.Columns[i].ToString().ToLower() == "c_c_qty")
                //{
                //    row.CreateCell(i).SetCellValue("本月寄庫藥品買斷結存");
                //}
                //else if (data.Columns[i].ToString().ToLower() == "no_w_p_i_dif")
                //{
                //    row.CreateCell(i).SetCellValue("不含戰備上月結存價差");
                //}
                //else if (data.Columns[i].ToString().ToLower() == "no_w_p_i")
                //{
                //    row.CreateCell(i).SetCellValue("不含戰備上月結存");
                //}
                //else if (data.Columns[i].ToString().ToLower() == "no_w_c_i")
                //{
                //    row.CreateCell(i).SetCellValue("不含戰備本月應有結存量");
                //}
                //else if (data.Columns[i].ToString().ToLower() == "no_w_c_c")
                //{
                //    row.CreateCell(i).SetCellValue("不含戰備本月盤存量");
                //}
                //else
                {
                    row.CreateCell(i).SetCellValue(data.Columns[i].ToString());
                }

            }

            for (int i = 0; i < data.Rows.Count; i++)
            {
                row = sheet.CreateRow(1 + i);
                for (int j = 0; j < data.Columns.Count; j++)
                {
                    row.CreateCell(j).SetCellValue(data.Rows[i].ItemArray[j].ToString());
                }
            }
            return wb;
        }

        [HttpPost]  // [匯出消耗結存明細] 803(台中)專用
        public ApiResponse ExcelDetail_803(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");
            var p6 = form.Get("p6");
            var p7 = form.Get("p7");
            var p8 = form.Get("p8");
            var p9 = form.Get("p9");
            var p10 = form.Get("p10");
            var p11 = form.Get("p11");
            var p12 = form.Get("p12");
            var p13 = form.Get("p13");
            var p14 = form.Get("p14");
            var p15 = form.Get("p15");
            var p16 = form.Get("p16");
            var p17 = form.Get("p17");
            var p18 = form.Get("p18"); // (近6個月進貨)或(近6個月醫令耗用)或(庫量<>0)或(庫存=0且無作廢)
            var p19 = form.Get("p19"); // (期初庫存<>0)或(期初=0但有進出)
            var p20 = form.Get("p20"); // SQL邏輯待補充
            var p21 = form.Get("p21");
            var HospCode = form.Get("hosp_code");
            var user = User.Identity.Name;
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0044Repository repo = new CE0044Repository(DBWork);
                    string dateTime = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MM");
                    DataTable data = repo.GetExcelDetail_803(p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18, p19, HospCode, user, p20, p21);
                    string fileName = "消耗結存明細" + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";
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
                    var a = e.Message.ToString();
                    throw;
                }
                return session.Result;
            }
        }

        //月結年月日期區間
        [HttpPost]
        public ApiResponse SetYmDateGet(FormDataCollection form)
        {
            var set_ym = form.Get("set_ym");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0044Repository repo = new CE0044Repository(DBWork);
                    session.Result.etts = repo.SetYmDateGet(set_ym);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //月結年月
        [HttpPost]
        public ApiResponse GetYmCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0044Repository repo = new CE0044Repository(DBWork);
                    session.Result.etts = repo.GetYmCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //盤存單位
        [HttpPost]
        public ApiResponse GetRlnoCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0044Repository repo = new CE0044Repository(DBWork);
                    session.Result.etts = repo.GetRlnoCombo(User.Identity.Name, p0);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //庫房類別
        [HttpPost]
        public ApiResponse GetWhKindCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0044Repository repo = new CE0044Repository(DBWork);
                    session.Result.etts = repo.GetWhKindCombo(User.Identity.Name);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //庫房代碼
        [HttpPost]
        public ApiResponse GetMiWhidCombo(FormDataCollection form)
        {
            var wh_kind = form.Get("wh_kind");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0044Repository repo = new CE0044Repository(DBWork);
                    session.Result.etts = repo.GetMiWhidCombo(User.Identity.Name, wh_kind);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //責任中心代碼
        [HttpPost]
        public ApiResponse GetUrInidCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0044Repository repo = new CE0044Repository(DBWork);
                    session.Result.etts = repo.GetUrInidCombo(User.Identity.Name);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //藥材類別
        [HttpPost]
        public ApiResponse GetMatClassSubParamCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0044Repository repo = new CE0044Repository(DBWork);
                    session.Result.etts = repo.GetMatClassSubParamCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //藥材代碼
        [HttpPost]
        public ApiResponse GetMiMastCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0044Repository repo = new CE0044Repository(DBWork);
                    session.Result.etts = repo.GetMiMastCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //是否合約
        [HttpPost]
        public ApiResponse GetContidParamCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0044Repository repo = new CE0044Repository(DBWork);
                    session.Result.etts = repo.GetContidParamCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //買斷寄庫
        [HttpPost]
        public ApiResponse GetSourcecodeParamCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0044Repository repo = new CE0044Repository(DBWork);
                    session.Result.etts = repo.GetSourcecodeParamCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //是否戰備
        [HttpPost]
        public ApiResponse GetWarbakParamCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0044Repository repo = new CE0044Repository(DBWork);
                    session.Result.etts = repo.GetWarbakParamCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //管制品項
        [HttpPost]
        public ApiResponse GetRestrictcodeParamCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0044Repository repo = new CE0044Repository(DBWork);
                    session.Result.etts = repo.GetRestrictcodeParamCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //是否常用品項
        [HttpPost]
        public ApiResponse GetCommonParamCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0044Repository repo = new CE0044Repository(DBWork);
                    session.Result.etts = repo.GetCommonParamCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //急救品項
        [HttpPost]
        public ApiResponse GetFastdrugParamCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0044Repository repo = new CE0044Repository(DBWork);
                    session.Result.etts = repo.GetFastdrugParamCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //中西藥別
        [HttpPost]
        public ApiResponse GetDrugkindParamCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0044Repository repo = new CE0044Repository(DBWork);
                    session.Result.etts = repo.GetDrugkindParamCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //合約類別
        [HttpPost]
        public ApiResponse GetTouchcaseParamCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0044Repository repo = new CE0044Repository(DBWork);
                    session.Result.etts = repo.GetTouchcaseParamCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //採購類別
        [HttpPost]
        public ApiResponse GetOrderkindParamCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0044Repository repo = new CE0044Repository(DBWork);
                    session.Result.etts = repo.GetOrderkindParamCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //特殊品項
        [HttpPost]
        public ApiResponse GetSpecialorderkindParamCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0044Repository repo = new CE0044Repository(DBWork);
                    session.Result.etts = repo.GetSpecialorderkindParamCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }


        [HttpPost]
        public ApiResponse GetLoginInfo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0044Repository repo = new CE0044Repository(DBWork);
                    var v_userid = User.Identity.Name;
                    var v_ip = DBWork.ProcIP;
                    session.Result.etts = repo.GetLoginInfo(v_userid, v_ip);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

    }
}