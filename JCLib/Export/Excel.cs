using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace JCLib
{
    public class Excel
    {
        /// <summary>
        /// 匯出單一Excel檔，內含一個工作表(Sheet1)
        /// </summary>
        /// <param name="fileName">匯出Excel檔名</param>
        /// <param name="dataTable">要匯出的資料表</param>
        /// <param name="headerHandler">根據資料表設定表首顯示的資料</param>
        /// <param name="footerHandler">根據資料表設定表尾顯示的資料</param>
        public static void Export(string fileName, DataTable dataTable,
            Func<DataTable, string> headerHandler = null, Func<DataTable, string> footerHandler = null, bool colAutoFit = true)
        {
            if (dataTable == null) return;

            if (HttpContext.Current == null) return;

            string[] fileNameSplit = fileName.Split('.');
            IWorkbook workbook = CreateWorkBook(fileName);

            CreateDataSheet(workbook, dataTable, "Sheet1", headerHandler, footerHandler, colAutoFit);

            OutputWorkbook(workbook, fileName);
        }

        /// <summary>
        /// 匯出單一Excel檔，內含多個工作表
        /// <para>工作表名稱為dataSet內的DataTable.TableName</para>
        /// </summary>
        /// <param name="fileName">匯出Excel檔名</param>
        /// <param name="dataSet">要匯出的資料集</param>
        /// <param name="headerHandler">根據資料表設定表首顯示的資料</param>
        /// <param name="footerHandler">根據資料表設定表尾顯示的資料</param>
        public static void Export(string fileName, DataSet dataSet,
            Func<DataTable, string> headerHandler = null, Func<DataTable, string> footerHandler = null)
        {
            if (dataSet == null) return;

            if (HttpContext.Current == null) return;

            IWorkbook workbook = CreateWorkBook(fileName);
            int i = 0;
            foreach (DataTable dataTable in dataSet.Tables)
            {
                string xlsSheetName = dataTable.TableName;
                if (xlsSheetName == "") xlsSheetName = string.Format("Sheet{0}", i + 1);
                CreateDataSheet(workbook, dataTable, xlsSheetName, headerHandler, footerHandler);
                i++;
            }

            OutputWorkbook(workbook, fileName);
        }

        /// <summary>
        /// 匯出單一壓縮檔(zip)，內含多個Excel檔，每個Excel檔內含一個工作表(Sheet1)
        /// <para>壓縮檔內的Excel檔名為dataSet內的DataTable.TableName</para>
        /// </summary>
        /// <param name="fileName">匯出壓縮檔名(zip)</param>
        /// <param name="dataSet">要匯出的資料集</param>
        /// <param name="headerHandler">根據資料表設定表首顯示的資料</param>
        /// <param name="footerHandler">根據資料表設定表尾顯示的資料</param>
        public static void ExportZip(string fileName, DataSet dataSet,
            Func<DataTable, string> headerHandler = null, Func<DataTable, string> footerHandler = null)
        {
            if (dataSet == null) return;

            if (HttpContext.Current == null) return;

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    int i = 0;
                    foreach (DataTable dataTable in dataSet.Tables)
                    {
                        string xlsFileName = dataTable.TableName;
                        if (xlsFileName == "") xlsFileName = string.Format("Excel{0}", i + 1);
                        var xlsFileNameUpper = xlsFileName.ToUpper();
                        if (!(xlsFileNameUpper.EndsWith(".XLS") || xlsFileNameUpper.EndsWith(".XLSX")))
                            xlsFileName += ".xls";

                        var xlsFile = archive.CreateEntry(xlsFileName);
                        using (var entryStream = xlsFile.Open())
                        {
                            IWorkbook workbook = new HSSFWorkbook();
                            CreateDataSheet(workbook, dataTable, "Sheet1", headerHandler, footerHandler);
                            workbook.Write(entryStream);
                        }

                        i++;
                    }
                }

                JCLib.Export.OutputFile(memoryStream, fileName);
            }
        }

        /// <summary>
        /// 匯出單一壓縮檔(zip)，內含多個Excel檔，每個Excel檔內含一個工作表(Sheet1)
        /// <para>壓縮檔內的Excel檔名為dataSet內的DataTable.TableName</para>
        /// </summary>
        /// <param name="zipName">匯出壓縮檔名(zip)</param>
        /// <param name="dataSet">要匯出的資料集</param>
        /// <param name="headerHandler">根據資料表設定表首顯示的資料</param>
        /// <param name="footerHandler">根據資料表設定表尾顯示的資料</param>
        public static void ExportZipSpecFormat(
            string zipName, 
            //DataSet dataSet,
            List<String> lstFilePaths, 
            Func<DataTable, string> headerHandler = null, 
            Func<DataTable, string> footerHandler = null
        )
        {
            if (lstFilePaths.Count == 0) return;
            if (HttpContext.Current == null) return;

            using (var memoryStream = new MemoryStream())
            {
                using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (String filePath in lstFilePaths)
                    {
                        String fileName = Path.GetFileName(filePath);
                        ZipArchiveEntry aEntry = zipArchive.CreateEntry(fileName);
                        using (FileStream aFileStream = new FileStream(filePath, FileMode.Open))
                        {
                            using (Stream aZipStream = aEntry.Open())
                            {
                                aFileStream.CopyTo(aZipStream);
                            }
                        }
                    }
                }
                memoryStream.Position = 0;

                //JCLib.Export.OutputFile(memoryStream, fileName);
                // 下載 ZIP 檔案
                HttpResponse res = HttpContext.Current.Response;
                res.BufferOutput = false;
                res.Clear();
                res.ClearHeaders();
                res.ContentType = "application/zip"; // 設置 Content-Type 為 ZIP 檔案
                res.AddHeader("Content-Disposition", 
                    "attachment; filename=\"" + HttpUtility.UrlEncode(zipName, System.Text.Encoding.UTF8) + "\"");
                memoryStream.CopyTo(res.OutputStream);
                // res.BinaryWrite(memoryStream.ToArray());
                res.Flush();
                res.End();
            }
        }

        /// <summary>
        /// 匯出單一壓縮檔(zip)，內含多個Excel檔，每個Excel檔內含一個工作表(Sheet1)
        /// <para>壓縮檔內的Excel檔名為groupByField名稱[空白]groupByField值</para>
        /// </summary>
        /// <param name="fileName">匯出壓縮檔名(zip)</param>
        /// <param name="dataTable">要分群的資料表</param>
        /// <param name="gropuByField">要用來分群的欄位</param>
        /// <param name="removeGroupByField">分群後的資料表是否移除分群欄位，預設為false</param>
        /// <param name="headerHandler">根據資料表設定表首顯示的資料</param>
        /// <param name="footerHandler">根據資料表設定表尾顯示的資料</param>
        public static void ExportZip(string fileName, DataTable dataTable, string groupByField, bool removeGroupByField = false,
            Func<DataTable, string> headerHandler = null, Func<DataTable, string> footerHandler = null)
        {
            if (dataTable == null) return;

            if (groupByField == null) return;

            if (HttpContext.Current == null) return;

            DataSet dataSet = GroupDataRows(dataTable, groupByField, removeGroupByField);

            ExportZip(fileName, dataSet, headerHandler, footerHandler);
        }

        private static DataSet GroupDataRows(DataTable dataTable, string groupByField, bool removeGroupByField)
        {
            IEnumerable<DataRow> source = dataTable.Rows.Cast<DataRow>();
            DataSet dataSet = new DataSet();

            var result = source.GroupBy(o => o[groupByField]);
            foreach (var rows in result)
            {
                DataTable dt = dataTable.Clone();
                foreach (DataRow row in rows)
                {
                    DataRow dr = dt.NewRow();
                    dr.ItemArray = row.ItemArray;
                    dt.Rows.Add(dr);
                }
                dt.TableName = groupByField + " " + dt.Rows[0][groupByField];
                if (removeGroupByField) dt.Columns.Remove(groupByField);

                dataSet.Tables.Add(dt);
            }

            return dataSet;
        }
        
        /// <summary>
        /// 根據 fileName 建立 WorkBook
        /// </summary>
        /// <param name="strFileName"></param>
        /// <returns></returns>
        private static IWorkbook CreateWorkBook(string strFileName)
        {
            string[] fileNameSplit = strFileName.Split('.');

            if (fileNameSplit[fileNameSplit.Length - 1].ToUpper() == "XLSX")
            {
                return new XSSFWorkbook();
            }

            return new HSSFWorkbook();
        }

        private static void CreateDataSheet(
            IWorkbook workbook, DataTable dataTable, string sheetName,
            Func<DataTable, string> headerHandler = null,
            Func<DataTable, string> footerHandler = null,
            bool colAutoFit = true
            )
        {
            ISheet sheet = workbook.CreateSheet(sheetName);

            int ri = 0;
            short ci = 0;

            //寫入頁首資料
            if (headerHandler != null)
            {
                CreateSpecialRow(workbook, sheet, ri, dataTable.Columns.Count - 1, headerHandler(dataTable));
                ri++;
            }

            //寫入欄位名稱 ColumnName
            IRow excnr = sheet.CreateRow(ri);
            foreach (DataColumn dc in dataTable.Columns)
            {
                ICell exc = excnr.CreateCell(ci);
                exc.SetCellValue(dc.ColumnName);
                ci++;
            }
            ri++;
            ci = 0;

            //寫入欄位資料
            foreach (DataRow dr in dataTable.Rows)
            {
                IRow exr = sheet.CreateRow(ri);
                foreach (object v in dr.ItemArray)
                {
                    ICell exc = exr.CreateCell(ci);
                    exc.SetCellValue(v.ToString());
                    ci++;
                }
                ri++;
                ci = 0;
            }

            //寫入頁尾資料
            if (footerHandler != null)
            {
                CreateSpecialRow(workbook, sheet, ri, dataTable.Columns.Count - 1, footerHandler(dataTable));
            }

            // 自動調整欄位寬度(預設true)
            if (colAutoFit)
            {
                foreach (DataColumn dc in dataTable.Columns)
                {
                    sheet.AutoSizeColumn(ci);
                    ci++;
                }
                ci = 0;
            }
        }

        private static IRow CreateSpecialRow(IWorkbook workbook, ISheet sheet, int rowIndex, int colSpan, string cellValue)
        {
            IRow row = sheet.CreateRow(rowIndex);
            ICell cell = row.CreateCell(0);
            var cra = new CellRangeAddress(rowIndex, rowIndex, 0, colSpan);
            sheet.AddMergedRegion(cra);
            cell.SetCellValue(cellValue);

            var lineCount = cellValue.Count((c) => { return c == '\n'; });
            row.Height = (short)((lineCount + 2) * sheet.DefaultRowHeight);

            ICellStyle cs = workbook.CreateCellStyle();
            IFont cf = workbook.CreateFont();
            cf.Boldweight = (short)FontBoldWeight.Bold;
            cs.Alignment = HorizontalAlignment.Center;
            cs.VerticalAlignment = VerticalAlignment.Center;
            cs.WrapText = true;
            cs.SetFont(cf);

            cell.CellStyle = cs;

            return row;
        }

        private static void OutputWorkbook(IWorkbook workbook, string fileName)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                workbook.Write(memoryStream);
                JCLib.Export.OutputFile(memoryStream, fileName);
                workbook.Close();
            }
        }
    }
}
