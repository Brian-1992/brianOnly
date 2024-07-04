using JCLib.DB;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Data;
using System.IO;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web;
using WebApp.Models;
using WebApp.Repository.AA;
using NPOI.SS.Util;

namespace WebApp.Controllers.AA
{
    public class AA0174Controller : SiteBase.BaseApiController
    {
        public ApiResponse All(FormDataCollection form)
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
            var p10 = form.Get("p10");   
            var p11 = form.Get("p11");   
            var p12 = form.Get("p12");   
            var p13 = form.Get("p13");
            var p14 = form.Get("p14");
            var p15 = form.Get("p15");
            var p16 = form.Get("p16");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));  
            var limit = int.Parse(form.Get("limit")); 
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0174Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1, p2, p3, p4, p5, p6, p7, p8, p10, p11, p12, p13, p14, p15, p16, page, limit, sorters); 
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetMatClassSubCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0174Repository(DBWork);
                    session.Result.etts = repo.GetMatClassSubCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetWarBakCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0174Repository(DBWork);
                    session.Result.etts = repo.GetWarBakCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetESourceCodeCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0174Repository(DBWork);
                    session.Result.etts = repo.GetESourceCodeCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetERestrictcodeCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0174Repository(DBWork);
                    session.Result.etts = repo.GetERestrictcodeCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetMContidCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0174Repository(DBWork);
                    session.Result.etts = repo.GetMContidCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetDrugKindCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0174Repository(DBWork);
                    session.Result.etts = repo.GetDrugKindCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetMMCodeCombo(FormDataCollection form)
        {
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
                    AA0174Repository repo = new AA0174Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeCombo(p0, page, limit, "");
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetPHVenderCombo(FormDataCollection form)
        {
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
                    AA0174Repository repo = new AA0174Repository(DBWork);
                    session.Result.etts = repo.GetPHVenderCombo(p0, page, limit, "");
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Excel1(FormDataCollection form)
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
            var p10 = form.Get("p10");
            var p11 = form.Get("p11");
            var p12 = form.Get("p12");
            var p13 = form.Get("p13");
            var p14 = form.Get("p14");
            var p15 = form.Get("p15");
            var p16 = form.Get("p16");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0174Repository repo = new AA0174Repository(DBWork);
                    string dateTime = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MM");
                    //JCLib.Excel.Export("申報健保廠商" + "_" + DateTime.Now.ToString("yyyyMMddhhmm") + ".xls", repo.GetExcel1(p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13));
                    JCLib.Excel.Export("申報健保廠商.xls", repo.GetExcel1(p0, p1, p2, p3, p4, p5, p6, p7, p8, p10, p11, p12, p13, p14, p15, p16));
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
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");
            var p6 = form.Get("p6");
            var p7 = form.Get("p7");
            var p8 = form.Get("p8");
            var p10 = form.Get("p10");
            var p11 = form.Get("p11");
            var p12 = form.Get("p12");
            var p13 = form.Get("p13");
            var p14 = form.Get("p14");
            var p15 = form.Get("p15");
            var p16 = form.Get("p16");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0174Repository repo = new AA0174Repository(DBWork);

                    //DataTable dtItems = new DataTable();
                    //dtItems.Columns.Add("類別", typeof(int));
                    //dtItems.Columns["類別"].AutoIncrement = true;
                    //dtItems.Columns["類別"].AutoIncrementSeed = 1;
                    //dtItems.Columns["類別"].AutoIncrementStep = 1;

                    //DataTable result = null;
                    //JCLib.Excel.Export( string.Format("廠商進退貨明細表_{0}.xlsx", DateTime.Now.ToString("yyyyMMddHHmmss")), 
                    //    repo.GetExcel2(p0, p1, p2, p3, p4, p5, p6, p7, p8, p10, p11, p12, p13, p14, p15, p16));

                    var workbook = CreatWorkbook(repo.GetExcel2(p0, p1, p2, p3, p4, p5, p6, p7, p8, p10, p11, p12, p13, p14, p15, p16));

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        workbook.Write(memoryStream);
                        JCLib.Export.OutputFile(memoryStream, string.Format("廠商進退貨明細表_{0}.xlsx", DateTime.Now.ToString("yyyyMMddHHmmss")));
                        workbook.Close();
                    }

                    // 註解的部分，先不要處理，先簡單 output 結果
                    //var workbook = ExoprtToExcel(result);


                    //var wb = new XSSFWorkbook();
                    //var sheet = (XSSFSheet)wb.CreateSheet("Sheet1");

                    //int rownum = 0;
                    //int rownumend = 0;
                    //double dInAmt = 0.00;
                    //double dInContAmt = 0.00;
                    //double dOutAmt = 0.00;
                    //double dOutContAmt = 0.00;
                    //double dSAmt = 0.00;
                    //double dSContAmt = 0.00;
                    //double dTotAmt = 0.00;
                    //double dTotContAmt = 0.00;
                    //double dDiscAmt = 0.00;
                    //double dContAmt = 0.00;

                    //CellRangeAddress cra;
                    //IDataFormat currDataFormat = wb.CreateDataFormat();
                    //IFont str_font1 = wb.CreateFont();
                    //str_font1.FontName = "新細明體";
                    //str_font1.FontHeightInPoints = 12;
                    //str_font1.Boldweight = 1;
                    //str_font1.Color = NPOI.HSSF.Util.HSSFColor.Blue.Index;
                    //IFont str_font2 = wb.CreateFont();
                    //str_font2.FontName = "新細明體";
                    //str_font2.FontHeightInPoints = 12;
                    //str_font2.Boldweight = 1;
                    //str_font2.Color = NPOI.HSSF.Util.HSSFColor.White.Index;
                    //IFont str_font3 = wb.CreateFont();
                    //str_font3.FontName = "新細明體";
                    //str_font3.FontHeightInPoints = 12;
                    //str_font3.Boldweight = 1;
                    //str_font3.Color = NPOI.HSSF.Util.HSSFColor.Red.Index;
                    //ICellStyle str_style1 = wb.CreateCellStyle();
                    //str_style1.BorderTop = NPOI.SS.UserModel.BorderStyle.None;
                    //str_style1.BorderBottom = NPOI.SS.UserModel.BorderStyle.None;
                    //str_style1.BorderLeft = NPOI.SS.UserModel.BorderStyle.None;
                    //str_style1.BorderRight = NPOI.SS.UserModel.BorderStyle.None;
                    //str_style1.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Left;
                    //str_style1.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.White.Index;
                    //str_style1.FillPattern = FillPattern.SolidForeground;
                    //str_style1.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
                    //str_style1.SetFont(str_font1);
                    //ICellStyle str_stylen1 = wb.CreateCellStyle();
                    //str_stylen1.BorderTop = NPOI.SS.UserModel.BorderStyle.None;
                    //str_stylen1.BorderBottom = NPOI.SS.UserModel.BorderStyle.None;
                    //str_stylen1.BorderLeft = NPOI.SS.UserModel.BorderStyle.None;
                    //str_stylen1.BorderRight = NPOI.SS.UserModel.BorderStyle.None;
                    //str_stylen1.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Right;
                    //str_stylen1.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.White.Index;
                    //str_stylen1.FillPattern = FillPattern.SolidForeground;
                    //str_stylen1.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
                    //str_stylen1.SetFont(str_font1);
                    //str_stylen1.DataFormat = currDataFormat.GetFormat("#,##0.00;[Red]-#,##0.00");
                    //ICellStyle str_style2 = wb.CreateCellStyle();
                    //str_style2.BorderTop = NPOI.SS.UserModel.BorderStyle.Hair;
                    //str_style2.BorderLeft = NPOI.SS.UserModel.BorderStyle.Hair;
                    //str_style2.BorderRight = NPOI.SS.UserModel.BorderStyle.Hair;
                    //str_style2.BorderBottom = NPOI.SS.UserModel.BorderStyle.Hair;
                    //str_style2.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Left;
                    //str_style2.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.SeaGreen.Index;
                    //str_style2.FillPattern = FillPattern.SolidForeground;
                    //str_style2.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
                    //str_style2.SetFont(str_font2);
                    //ICellStyle str_stylen2 = wb.CreateCellStyle();
                    //str_stylen2.BorderTop = NPOI.SS.UserModel.BorderStyle.Hair;
                    //str_stylen2.BorderLeft = NPOI.SS.UserModel.BorderStyle.Hair;
                    //str_stylen2.BorderRight = NPOI.SS.UserModel.BorderStyle.Hair;
                    //str_stylen2.BorderBottom = NPOI.SS.UserModel.BorderStyle.Hair;
                    //str_stylen2.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Right;
                    //str_stylen2.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.SeaGreen.Index;
                    //str_stylen2.FillPattern = FillPattern.SolidForeground;
                    //str_stylen2.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
                    //str_stylen2.SetFont(str_font2);
                    //str_stylen2.DataFormat = currDataFormat.GetFormat("#,##0.00;[Red]-#,##0.00");
                    //ICellStyle str_style3 = wb.CreateCellStyle();
                    //str_style3.BorderTop = NPOI.SS.UserModel.BorderStyle.Hair;
                    //str_style3.BorderLeft = NPOI.SS.UserModel.BorderStyle.Hair;
                    //str_style3.BorderRight = NPOI.SS.UserModel.BorderStyle.Hair;
                    //str_style3.BorderBottom = NPOI.SS.UserModel.BorderStyle.Hair;
                    //str_style3.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Left;
                    //str_style3.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Yellow.Index;
                    //str_style3.FillPattern = FillPattern.SolidForeground;
                    //str_style3.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
                    //str_style3.SetFont(str_font3);

                    //IRow row = sheet.CreateRow(0);

                    //cra = new NPOI.SS.Util.CellRangeAddress(0, 0, 0, 9);
                    //sheet.AddMergedRegion(cra);
                    //for (int t = 0; t < 12; t++)
                    //{
                    //    ICell newcell = row.CreateCell(t);
                    //}
                    //row.GetCell(0).SetCellValue("廠商進退貨明細表");

                    //row = sheet.CreateRow(1);
                    //for (int i = 0; i < result.Columns.Count; i++)
                    //{
                    //    row.CreateCell(i).SetCellValue(result.Columns[i].ToString());
                    //}

                    //for (int i = 0; i < result.Rows.Count; i++)
                    //{
                    //    row = sheet.CreateRow(2 + i);
                    //    for (int j = 0; j < result.Columns.Count; j++)
                    //    {
                    //        if (j == 10 || j == 12 || j == 13 || j == 14 || j == 15 || j == 21 || j == 30 || j == 31 || j == 32 || j == 41 || j == 42 || j == 43 || j == 44 || j == 45 || j == 49 || j == 50)
                    //        {
                    //            row.CreateCell(j).SetCellValue(double.Parse(result.Rows[i].ItemArray[j].ToString()));
                    //        }
                    //        else
                    //        {
                    //            row.CreateCell(j).SetCellValue(result.Rows[i].ItemArray[j].ToString());
                    //        }
                    //    }
                    //    if (result.Rows[i].ItemArray[0].ToString() == "進貨")
                    //    {
                    //        dInAmt += double.Parse(result.Rows[i].ItemArray[31].ToString());
                    //        if(result.Rows[i].ItemArray[39].ToString() == "合約")
                    //        {
                    //            dInContAmt += double.Parse(result.Rows[i].ItemArray[31].ToString());
                    //        }
                    //    }
                    //    if (result.Rows[i].ItemArray[0].ToString() == "退貨")
                    //    {
                    //        dOutAmt += double.Parse(result.Rows[i].ItemArray[31].ToString());
                    //        if (result.Rows[i].ItemArray[39].ToString() == "合約")
                    //        {
                    //            dOutContAmt += double.Parse(result.Rows[i].ItemArray[31].ToString());
                    //        }
                    //    }
                    //    dSAmt += double.Parse(result.Rows[i].ItemArray[21].ToString());
                    //    if (result.Rows[i].ItemArray[39].ToString() == "合約")
                    //    {
                    //        dSContAmt += double.Parse(result.Rows[i].ItemArray[21].ToString());
                    //    }
                    //    dDiscAmt += double.Parse(result.Rows[i].ItemArray[14].ToString());
                    //    dContAmt += double.Parse(result.Rows[i].ItemArray[43].ToString());
                    //}
                    //dTotAmt = dInAmt + dOutAmt;
                    //dTotContAmt = dInContAmt + dOutContAmt;

                    //rownum = result.Rows.Count + 3;
                    //rownumend = result.Rows.Count;
                    //row = sheet.CreateRow(rownum);
                    //for (int t = 0; t < 12; t++)
                    //{
                    //    ICell newcell = row.CreateCell(t);
                    //}
                    //row.GetCell(2).CellStyle = str_style1;
                    //row.GetCell(2).SetCellValue("進貨合計:");
                    //row.GetCell(3).CellStyle = str_stylen1;
                    //row.GetCell(3).SetCellValue(dInAmt);
                    //row.GetCell(4).CellStyle = str_style1;
                    //row.GetCell(4).SetCellValue("退貨合計:");
                    //row.GetCell(5).CellStyle = str_stylen1;
                    //row.GetCell(5).SetCellValue(dOutAmt);
                    //row.GetCell(6).CellStyle = str_style1;
                    //row.GetCell(6).SetCellValue("贈品合計:");
                    //row.GetCell(7).CellStyle = str_stylen1;
                    //row.GetCell(7).SetCellValue(dSAmt);
                    //row.GetCell(8).CellStyle = str_style2;
                    //row.GetCell(8).SetCellValue("總計金額:");
                    //row.GetCell(9).CellStyle = str_stylen2;
                    //row.GetCell(9).SetCellValue(dTotAmt);

                    //rownum++;
                    //row = sheet.CreateRow(rownum);
                    //for (int t = 0; t < 12; t++)
                    //{
                    //    ICell newcell = row.CreateCell(t);
                    //}
                    //row.GetCell(2).CellStyle = str_style1;
                    //row.GetCell(2).SetCellValue("合約進貨合計:");
                    //row.GetCell(3).CellStyle = str_stylen1;
                    //row.GetCell(3).SetCellValue(dInContAmt);
                    //row.GetCell(4).CellStyle = str_style1;
                    //row.GetCell(4).SetCellValue("合約退貨合計:");
                    //row.GetCell(5).CellStyle = str_stylen1;
                    //row.GetCell(5).SetCellValue(dOutContAmt);
                    //row.GetCell(6).CellStyle = str_style1;
                    //row.GetCell(6).SetCellValue("合約贈品合計:");
                    //row.GetCell(7).CellStyle = str_stylen1;
                    //row.GetCell(7).SetCellValue(dSContAmt);
                    //row.GetCell(8).CellStyle = str_style2;
                    //row.GetCell(8).SetCellValue("合約總計金額:");
                    //row.GetCell(9).CellStyle = str_stylen2;
                    //row.GetCell(9).SetCellValue(dTotContAmt);

                    //rownum++;
                    //row = sheet.CreateRow(rownum);
                    //for (int t = 0; t < 12; t++)
                    //{
                    //    ICell newcell = row.CreateCell(t);
                    //}
                    //row.GetCell(2).CellStyle = str_style1;
                    //row.GetCell(2).SetCellValue("折讓合計:");
                    //row.GetCell(3).CellStyle = str_stylen1;
                    //row.GetCell(3).SetCellValue(dDiscAmt);
                    //row.GetCell(8).CellStyle = str_style2;
                    //row.GetCell(8).SetCellValue("總計合約成本差:");
                    //row.GetCell(9).CellStyle = str_stylen2;
                    //row.GetCell(9).SetCellValue(dContAmt);

                    //rownum++;
                    //row = sheet.CreateRow(rownum);
                    //cra = new NPOI.SS.Util.CellRangeAddress(rownum, rownum, 2, 9);
                    //sheet.AddMergedRegion(cra);
                    //for (int t = 0; t < 12; t++)
                    //{
                    //    ICell newcell = row.CreateCell(t);
                    //}
                    //row.GetCell(2).CellStyle = str_style3;
                    //row.GetCell(2).SetCellValue("請注意:各品項成本單價比合約價高時，會產生負數合約成本差，<總計合約成本差>可能變少，另非合約不計入差額");

                    //rownum++;
                    //row = sheet.CreateRow(rownum);
                    //cra = new NPOI.SS.Util.CellRangeAddress(rownum, rownum, 2, 9);
                    //sheet.AddMergedRegion(cra);
                    //for (int t = 0; t < 12; t++)
                    //{
                    //    ICell newcell = row.CreateCell(t);
                    //}
                    //row.GetCell(2).CellStyle = str_style3;
                    //row.GetCell(2).SetCellValue("　　　<總計金額>及<合約總計金額>皆已扣除[折讓金額](折讓金額是指進貨單額外輸入的折讓金額)");

                    //rownum++;
                    //row = sheet.CreateRow(rownum);
                    //cra = new NPOI.SS.Util.CellRangeAddress(rownum, rownum, 2, 9);
                    //sheet.AddMergedRegion(cra);
                    //for (int t = 0; t < 12; t++)
                    //{
                    //    ICell newcell = row.CreateCell(t);
                    //}
                    //row.GetCell(2).CellStyle = str_style3;
                    //row.GetCell(2).SetCellValue("　　　另外，資料中若有[退貨項目]，<總計金額>及<合約總計金額>也會扣除退貨金額");
                    //for (int i = 0; i < 20; i++)
                    //{
                    //    sheet.AutoSizeColumn(i);
                    //}
                    ////output
                    //var ms = new MemoryStream();
                    //wb.Write(ms);
                    //var res = HttpContext.Current.Response;
                    //res.BufferOutput = false;

                    //res.Clear();
                    //res.ClearHeaders();
                    //res.HeaderEncoding = System.Text.Encoding.Default;
                    //res.ContentType = "application/octet-stream";
                    //res.AddHeader("Content-Disposition",
                    //            "attachment; filename=" + string.Format("{0}.xlsx", form.Get("FN")));
                    //res.BinaryWrite(ms.ToArray());

                    //ms.Close();
                    //ms.Dispose();
                }
                catch (Exception e)
                {
                    //DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetT2F1(FormDataCollection form)
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
            var p10 = form.Get("p10");
            var p11 = form.Get("p11");
            var p12 = form.Get("p12");
            var p13 = form.Get("p13");
            var p14 = form.Get("p14");
            var p15 = form.Get("p15");
            var p16 = form.Get("p16");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0174Repository repo = new AA0174Repository(DBWork);
                    session.Result.msg = repo.GetT2F1(p0, p1, p2, p3, p4, p5, p6, p7, p8, p10, p11, p12, p13, p14, p15, p16);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetT2F2(FormDataCollection form)
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
            var p10 = form.Get("p10");
            var p11 = form.Get("p11");
            var p12 = form.Get("p12");
            var p13 = form.Get("p13");
            var p14 = form.Get("p14");
            var p15 = form.Get("p15");
            var p16 = form.Get("p16");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0174Repository repo = new AA0174Repository(DBWork);
                    session.Result.msg = repo.GetT2F2(p0, p1, p2, p3, p4, p5, p6, p7, p8, p10, p11, p12, p13, p14, p15, p16);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetT2F3(FormDataCollection form)
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
            var p10 = form.Get("p10");
            var p11 = form.Get("p11");
            var p12 = form.Get("p12");
            var p13 = form.Get("p13");
            var p14 = form.Get("p14");
            var p15 = form.Get("p15");
            var p16 = form.Get("p16");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0174Repository repo = new AA0174Repository(DBWork);
                    session.Result.msg = repo.GetT2F3(p0, p1, p2, p3, p4, p5, p6, p7, p8, p10, p11, p12, p13, p14, p15, p16);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetT2F4(FormDataCollection form)
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
            var p10 = form.Get("p10");
            var p11 = form.Get("p11");
            var p12 = form.Get("p12");
            var p13 = form.Get("p13");
            var p14 = form.Get("p14");
            var p15 = form.Get("p15");
            var p16 = form.Get("p16"); 

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0174Repository repo = new AA0174Repository(DBWork);
                    session.Result.msg = repo.GetT2F4(p0, p1, p2, p3, p4, p5, p6, p7, p8, p10, p11, p12, p13, p14, p15, p16);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetT2F5(FormDataCollection form)
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
            var p10 = form.Get("p10");
            var p11 = form.Get("p11");
            var p12 = form.Get("p12");
            var p13 = form.Get("p13");
            var p14 = form.Get("p14");
            var p15 = form.Get("p15");
            var p16 = form.Get("p16");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0174Repository repo = new AA0174Repository(DBWork);
                    session.Result.msg = repo.GetT2F5(p0, p1, p2, p3, p4, p5, p6, p7, p8, p10, p11, p12, p13, p14, p15, p16);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetT2F6(FormDataCollection form)
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
            var p10 = form.Get("p10");
            var p11 = form.Get("p11");
            var p12 = form.Get("p12");
            var p13 = form.Get("p13");
            var p14 = form.Get("p14");
            var p15 = form.Get("p15");
            var p16 = form.Get("p16");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0174Repository repo = new AA0174Repository(DBWork);
                    session.Result.msg = repo.GetT2F6(p0, p1, p2, p3, p4, p5, p6, p7, p8, p10, p11, p12, p13, p14, p15, p16);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetT2F7(FormDataCollection form)
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
            var p10 = form.Get("p10");
            var p11 = form.Get("p11");
            var p12 = form.Get("p12");
            var p13 = form.Get("p13");
            var p14 = form.Get("p14");
            var p15 = form.Get("p15");
            var p16 = form.Get("p16");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0174Repository repo = new AA0174Repository(DBWork);
                    session.Result.msg = repo.GetT2F7(p0, p1, p2, p3, p4, p5, p6, p7, p8, p10, p11, p12, p13, p14, p15, p16);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetT2F8(FormDataCollection form)
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
            var p10 = form.Get("p10");
            var p11 = form.Get("p11");
            var p12 = form.Get("p12");
            var p13 = form.Get("p13");
            var p14 = form.Get("p14");
            var p15 = form.Get("p15");
            var p16 = form.Get("p16");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0174Repository repo = new AA0174Repository(DBWork);
                    session.Result.msg = repo.GetT2F8(p0, p1, p2, p3, p4, p5, p6, p7, p8, p10, p11, p12, p13, p14, p15, p16);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetT2F9(FormDataCollection form)
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
            var p10 = form.Get("p10");
            var p11 = form.Get("p11");
            var p12 = form.Get("p12");
            var p13 = form.Get("p13");
            var p14 = form.Get("p14");
            var p15 = form.Get("p15");
            var p16 = form.Get("p16");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0174Repository repo = new AA0174Repository(DBWork);
                    session.Result.msg = repo.GetT2F9(p0, p1, p2, p3, p4, p5, p6, p7, p8, p10, p11, p12, p13, p14, p15, p16);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetT2F10(FormDataCollection form)
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
            var p10 = form.Get("p10");
            var p11 = form.Get("p11");
            var p12 = form.Get("p12");
            var p13 = form.Get("p13");
            var p14 = form.Get("p14");
            var p15 = form.Get("p15");
            var p16 = form.Get("p16");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0174Repository repo = new AA0174Repository(DBWork);
                    session.Result.msg = repo.GetT2F10(p0, p1, p2, p3, p4, p5, p6, p7, p8, p10, p11, p12, p13, p14, p15, p16);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //製造EXCEL的內容
        public XSSFWorkbook CreatWorkbook(DataTable data)
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
                    if ((j==10) || (j >11&&j<16)|| (j ==19) || (j > 25 && j < 29) || (j > 35 && j < 41) || (j > 43 && j < 46))
                    {
                        var cell = row.CreateCell(j);
                        cell.SetCellType(CellType.Numeric);
                        double a = 0;
                        double.TryParse(data.Rows[i].ItemArray[j].ToString(),out a);
                        cell.SetCellValue(a);
                    }
                    else
                    {
                        row.CreateCell(j).SetCellValue(data.Rows[i].ItemArray[j].ToString());
                    }

                }
            }
            return wb;
        }

    }
}