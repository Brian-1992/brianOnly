using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Web;

namespace JCLib
{
    public class Csv
    {
        /// <summary>
        /// 匯出CSV檔
        /// </summary>
        /// <param name="fileName">匯出CSV檔名</param>
        /// <param name="dataTable">要匯出的資料表</param>
        /// <param name="title">首列資料</param>
        public static void Export(string fileName, DataTable dataTable, string title = "")
        {
            if (dataTable == null) return;

            if (HttpContext.Current == null) return;

            using (MemoryStream ms = new MemoryStream())
            {
                using (StreamWriter sw = new StreamWriter(ms, System.Text.Encoding.UTF8))
                {
                    // create title
                    if (title != "")
                    {
                        sw.WriteLine(title);
                    }
                    else
                    {
                        int idxCol = 0;
                        foreach (DataColumn dc in dataTable.Columns)
                        {
                            if (0 != idxCol)
                            {
                                sw.Write(",");
                            }

                            sw.Write("\"" + dc.ColumnName + "\"");
                            idxCol++;
                        }
                        sw.WriteLine();
                    }

                    // create content
                    foreach (DataRow row in dataTable.Rows)
                    {
                        int idxCol = 0;
                        foreach (object v in row.ItemArray)
                        {
                            if (0 != idxCol)
                            {
                                sw.Write(",");
                            }

                            sw.Write("\"" + v.ToString() + "\"");
                            idxCol++;
                        }
                        sw.WriteLine();
                    }
                    sw.Flush();

                    JCLib.Export.OutputFile(ms, fileName);
                }
            }
        }
    }
}
