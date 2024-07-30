using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.F;
using WebApp.Models;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using System.Data;
using NPOI.SS.UserModel;
using System.IO;
using System.Web;
using NPOI.XSSF.UserModel;
using System;

namespace WebApp.Controllers.F
{
    public class FA0023Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0") == null ? string.Empty : form.Get("p0");    //物料分類  MAT_CLASS
            var p1 = form.Get("p1") == null ? string.Empty : form.Get("p1");    //成立年月  SET_YM
            var p2 = form.Get("p2") == null ? string.Empty : form.Get("p2");    //單位類別  INID_FLAG
            var p3 = form.Get("p3") == null ? string.Empty : form.Get("p3");    //是否庫備  M_STOREID
            var p4 = bool.Parse(form.Get("p4"));    //物料分類是否全選 clsALL
            var p5 = form.Get("p5") == null ? string.Empty : form.Get("p5");    //單位科別庫房代碼 
            //var p6 = form.Get("p6") == null ? false : bool.Parse(form.Get("p6"));    //for FA0058 -- true
            var p6 = form.Get("p6") == null ? string.Empty : form.Get("p6");    //庫存量<0
            var p7 = form.Get("p7") == null ? string.Empty : form.Get("p7");    //MMCODE
            var p8 = form.Get("p8") == null ? string.Empty : form.Get("p8");    //消耗量<0
            var p9 = form.Get("p9") == null ? string.Empty : form.Get("p9");    //期初<0
            var p10 = form.Get("p10") == null ? string.Empty : form.Get("p10");    //院內碼是否作廢
            var p11 = form.Get("p11") == null ? string.Empty : form.Get("p11");    //是否寄售
            var p12 = form.Get("p12") == null ? string.Empty : form.Get("p12");    //庫房是否作廢

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    if (p2 != "4")  //只有是否庫備Radio選擇單位科別才會抓取庫房代碼
                    {
                        p5 = string.Empty;
                    }
                    var repo = new FA0023Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12);
                }
                catch
                {
                    throw;
                }

                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse Print(FormDataCollection form)
        {
            var p0 = form.Get("p0") == null ? string.Empty : form.Get("p0");    //物料分類  MAT_CLASS
            var p1 = form.Get("p1") == null ? string.Empty : form.Get("p1");    //成立年月  SET_YM
            var p2 = form.Get("p2") == null ? string.Empty : form.Get("p2");    //單位種類  INID_FLAG
            var p3 = form.Get("p3") == null ? string.Empty : form.Get("p3");    //是否庫備  M_STOREID
            var p4 = bool.Parse(form.Get("p4"));    //物料分類是否全選 clsALL
            var p5 = form.Get("p5") == null ? string.Empty : form.Get("p5");    //單位科別庫房代碼 
            var p6 = form.Get("p6") == null ? string.Empty : form.Get("p6");    //IS_INV_MINUS
            var p7 = form.Get("p7") == null ? string.Empty : form.Get("p7");    //MMCODE
            var p8 = form.Get("p8") == null ? string.Empty : form.Get("p8");    //IS_OUT_MINUS
            var p9 = form.Get("p9") == null ? string.Empty : form.Get("p9");    //is_pmnqty_minus
            var p10 = form.Get("p10") == null ? string.Empty : form.Get("p10");    //是否作廢
            var p11 = form.Get("p11") == null ? string.Empty : form.Get("p11");    //是否寄售
            var p12 = form.Get("p12") == null ? string.Empty : form.Get("p12");    //是否寄售

            //var page = int.Parse(form.Get("page"));
            //var start = int.Parse(form.Get("start"));
            //var limit = int.Parse(form.Get("limit"));
            //var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    if (p2 != "4")  //只有是否庫備Radio選擇單位科別才會抓取庫房代碼
                    {
                        p5 = string.Empty;
                    }

                    var repo = new FA0023Repository(DBWork);
                    session.Result.etts = repo.Print(p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12);
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
            var p0 = form.Get("p0") == null ? string.Empty : form.Get("p0");    //物料分類  MAT_CLASS
            var p1 = form.Get("p1") == null ? string.Empty : form.Get("p1");    //成立年月  SET_YM
            var p2 = form.Get("p2") == null ? string.Empty : form.Get("p2");    //單位種類  INID_FLAG
            var p3 = form.Get("p3") == null ? string.Empty : form.Get("p3");    //是否庫備  M_STOREID
            var p4 = bool.Parse(form.Get("p4"));    //物料分類是否全選 clsALL
            var p5 = form.Get("p5") == null ? string.Empty : form.Get("p5");    //單位科別庫房代碼 
            var p6 = form.Get("p6") == null ? string.Empty : form.Get("p6");
            var p7 = form.Get("p7") == null ? string.Empty : form.Get("p7");
            var p8 = form.Get("p8") == null ? string.Empty : form.Get("p8");
            var p9 = form.Get("p9") == null ? string.Empty : form.Get("p9");
            var p10 = form.Get("p10") == null ? string.Empty : form.Get("p10");    //院內碼是否作廢
            var p11 = form.Get("p11") == null ? string.Empty : form.Get("p11");    //是否寄售
            var p12 = form.Get("p12") == null ? string.Empty : form.Get("p12");    //庫房是否作廢

            var T3P1 = form.Get("T3P1") == null ? string.Empty : form.Get("T3P1");
            var T3P2 = form.Get("T3P2") == null ? string.Empty : form.Get("T3P2");
            var T3P3 = form.Get("T3P3") == null ? string.Empty : form.Get("T3P3");
            var T3P4 = form.Get("T3P4") == null ? string.Empty : form.Get("T3P4");
            var T3P5 = form.Get("T3P5") == null ? string.Empty : form.Get("T3P5");
            var T3P6 = form.Get("T3P6") == null ? string.Empty : form.Get("T3P6");
            var T3P7 = form.Get("T3P7") == null ? string.Empty : form.Get("T3P7");
            var T3P8 = form.Get("T3P8") == null ? string.Empty : form.Get("T3P8");
            var T3P9 = form.Get("T3P9") == null ? string.Empty : form.Get("T3P9");
            var T3P10 = form.Get("T3P10") == null ? string.Empty : form.Get("T3P10");
            var T3P11 = form.Get("T3P11") == null ? string.Empty : form.Get("T3P11");
            var T3P12 = form.Get("T3P12") == null ? string.Empty : form.Get("T3P12");
            var T3P13 = form.Get("T3P13") == null ? string.Empty : form.Get("T3P13");
            var T3P14 = form.Get("T3P14") == null ? string.Empty : form.Get("T3P14");
            var T3P15 = form.Get("T3P15") == null ? string.Empty : form.Get("T3P15");
            var T3P16 = form.Get("T3P16") == null ? string.Empty : form.Get("T3P16");
            var T3P17 = form.Get("T3P17") == null ? string.Empty : form.Get("T3P17");
            var T3P18 = form.Get("T3P18") == null ? string.Empty : form.Get("T3P18");
            var T3P19 = form.Get("T3P19") == null ? string.Empty : form.Get("T3P19");
            var T3P20 = form.Get("T3P20") == null ? string.Empty : form.Get("T3P20");
            var T3P21 = form.Get("T3P21") == null ? string.Empty : form.Get("T3P21");
            var T3P22 = form.Get("T3P22") == null ? string.Empty : form.Get("T3P22");
            var T3P23 = form.Get("T3P23") == null ? string.Empty : form.Get("T3P23");
            var T3P24 = form.Get("T3P24") == null ? string.Empty : form.Get("T3P24");
            var T3P25 = form.Get("T3P25") == null ? string.Empty : form.Get("T3P25");
            var T3P26 = form.Get("T3P26") == null ? string.Empty : form.Get("T3P26");
            var T3P27 = form.Get("T3P27") == null ? string.Empty : form.Get("T3P27");
            var T3P28 = form.Get("T3P28") == null ? string.Empty : form.Get("T3P28");
            var T3P29 = form.Get("T3P29") == null ? string.Empty : form.Get("T3P29");
            var T3P30 = form.Get("T3P30") == null ? string.Empty : form.Get("T3P30");
            var T3P31 = form.Get("T3P31") == null ? string.Empty : form.Get("T3P31");
            var T3P32 = form.Get("T3P32") == null ? string.Empty : form.Get("T3P32");
            var T3P33 = form.Get("T3P33") == null ? string.Empty : form.Get("T3P33");
            //var page = int.Parse(form.Get("page"));
            //var start = int.Parse(form.Get("start"));
            //var limit = int.Parse(form.Get("limit"));
            //var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    if (p2 != "4")  //只有是否庫備Radio選擇單位科別才會抓取庫房代碼
                    {
                        p5 = string.Empty;
                    }

                    var repo = new FA0023Repository(DBWork);
                    JCLib.Excel.Export(form.Get("FN"), repo.GetExcel(p0, p1, p2, p3, p4, p5, p6, p7, p8, p9,p10, p11, p12,
                                                                     T3P1, T3P2, T3P3, T3P4, T3P5, T3P6, T3P7, T3P8, T3P9, T3P10,
                                                                     T3P11, T3P12, T3P13, T3P14, T3P15, T3P16, T3P17, T3P18, T3P19, T3P20,
                                                                     T3P21, T3P22, T3P23, T3P24, T3P25, T3P26, T3P27, T3P28, T3P29, T3P30, T3P31,
                                                                     T3P32, T3P33));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //匯出EXCEL
        [HttpPost]
        public void ExportExcel(FormDataCollection form)
        {
            var p0 = form.Get("p0") == null ? string.Empty : form.Get("p0");    //物料分類  MAT_CLASS
            var p1 = form.Get("p1") == null ? string.Empty : form.Get("p1");    //成立年月  SET_YM
            var p2 = form.Get("p2") == null ? string.Empty : form.Get("p2");    //單位種類  INID_FLAG
            var p3 = form.Get("p3") == null ? string.Empty : form.Get("p3");    //是否庫備  M_STOREID
            var p4 = bool.Parse(form.Get("p4"));    //物料分類是否全選 clsALL
            var p5 = form.Get("p5") == null ? string.Empty : form.Get("p5");    //單位科別庫房代碼 
            var p6 = form.Get("p6") == null ? string.Empty : form.Get("p6");
            var p7 = form.Get("p7") == null ? string.Empty : form.Get("p7");
            var p8 = form.Get("p8") == null ? string.Empty : form.Get("p8");
            var p9 = form.Get("p9") == null ? string.Empty : form.Get("p9");
            var p10 = form.Get("p10") == null ? string.Empty : form.Get("p10");    //是否作廢
            var p11 = form.Get("p11") == null ? string.Empty : form.Get("p11");    //是否寄售
            var p12 = form.Get("p12") == null ? string.Empty : form.Get("p12");    //是否寄售

            var T3P1 = form.Get("T3P1") == null ? string.Empty : form.Get("T3P1");
            var T3P2 = form.Get("T3P2") == null ? string.Empty : form.Get("T3P2");
            var T3P3 = form.Get("T3P3") == null ? string.Empty : form.Get("T3P3");
            var T3P4 = form.Get("T3P4") == null ? string.Empty : form.Get("T3P4");
            var T3P5 = form.Get("T3P5") == null ? string.Empty : form.Get("T3P5");
            var T3P6 = form.Get("T3P6") == null ? string.Empty : form.Get("T3P6");
            var T3P7 = form.Get("T3P7") == null ? string.Empty : form.Get("T3P7");
            var T3P8 = form.Get("T3P8") == null ? string.Empty : form.Get("T3P8");
            var T3P9 = form.Get("T3P9") == null ? string.Empty : form.Get("T3P9");
            var T3P10 = form.Get("T3P10") == null ? string.Empty : form.Get("T3P10");
            var T3P11 = form.Get("T3P11") == null ? string.Empty : form.Get("T3P11");
            var T3P12 = form.Get("T3P12") == null ? string.Empty : form.Get("T3P12");
            var T3P13 = form.Get("T3P13") == null ? string.Empty : form.Get("T3P13");
            var T3P14 = form.Get("T3P14") == null ? string.Empty : form.Get("T3P14");
            var T3P15 = form.Get("T3P15") == null ? string.Empty : form.Get("T3P15");
            var T3P16 = form.Get("T3P16") == null ? string.Empty : form.Get("T3P16");
            var T3P17 = form.Get("T3P17") == null ? string.Empty : form.Get("T3P17");
            var T3P18 = form.Get("T3P18") == null ? string.Empty : form.Get("T3P18");
            var T3P19 = form.Get("T3P19") == null ? string.Empty : form.Get("T3P19");
            var T3P20 = form.Get("T3P20") == null ? string.Empty : form.Get("T3P20");
            var T3P21 = form.Get("T3P21") == null ? string.Empty : form.Get("T3P21");
            var T3P22 = form.Get("T3P22") == null ? string.Empty : form.Get("T3P22");
            var T3P23 = form.Get("T3P23") == null ? string.Empty : form.Get("T3P23");
            var T3P24 = form.Get("T3P24") == null ? string.Empty : form.Get("T3P24");
            var T3P25 = form.Get("T3P25") == null ? string.Empty : form.Get("T3P25");
            var T3P26 = form.Get("T3P26") == null ? string.Empty : form.Get("T3P26");
            var T3P27 = form.Get("T3P27") == null ? string.Empty : form.Get("T3P27");
            var T3P28 = form.Get("T3P28") == null ? string.Empty : form.Get("T3P28");
            var T3P29 = form.Get("T3P29") == null ? string.Empty : form.Get("T3P29");
            var T3P30 = form.Get("T3P30") == null ? string.Empty : form.Get("T3P30");
            var T3P31 = form.Get("T3P31") == null ? string.Empty : form.Get("T3P31");
            var T3P32 = form.Get("T3P32") == null ? string.Empty : form.Get("T3P32");
            var T3P33 = form.Get("T3P33") == null ? string.Empty : form.Get("T3P33");
            DataTable data ;
            var t1 = "";var t2 = "";var t3 = "";
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0023Repository repo = new FA0023Repository(DBWork);
                    var fileName = form.Get("FN");
                    switch (p2)
                    {
                        case "0":  
                            t1 = "應盤點單位";
                            break;
                        case "1":   
                            t1 = "行政科室";
                            break;
                        case "2":   
                            t1 = "財務獨立單位";
                            break;
                        case "3":   
                            t1 = "全院";
                            break;
                        case "4":   
                            t1 = "單位科別";
                            break;
                        default:
                            break;
                    }
                    t2 = repo.getMatclassName(p0);

                    switch (p3)
                    {
                        case "0":
                            t3 = "不區分";
                            break;
                        case "1":
                            t3 = "庫備";
                            break;
                        case "2":
                            t3 = "非庫備";
                            break;
                        case "3":   
                            t3 = "單價 > 5000 且週轉率 < 1 (僅限全院和單科)";
                            break;
                        default:
                            break;
                    }

                    if (p2 != "4")  //只有是否庫備Radio選擇單位科別才會抓取庫房代碼
                    {
                        p5 = null;
                    }

                    data = repo.GetExcel(p0, p1, p2, p3, p4, p5, p6, p7, p8, p9,p10, p11, p12,
                                             T3P1, T3P2, T3P3, T3P4, T3P5, T3P6, T3P7, T3P8, T3P9, T3P10,
                                             T3P11, T3P12, T3P13, T3P14, T3P15, T3P16, T3P17, T3P18, T3P19, T3P20,
                                             T3P21, T3P22, T3P23, T3P24, T3P25, T3P26, T3P27, T3P28, T3P29,T3P30, T3P31,
                                             T3P32, T3P33);
                    if (data.Rows.Count > 0)
                    {
                        var workbook = ExoprtToExcel(data, t1, t2, t3);

                        //output
                        var ms = new MemoryStream();
                        workbook.Write(ms);
                        var res = HttpContext.Current.Response;
                        res.BufferOutput = false;

                        res.Clear();
                        res.ClearHeaders();
                        res.HeaderEncoding = System.Text.Encoding.Default;
                        res.ContentType = "application/octet-stream";
                        res.AddHeader("Content-Disposition",
                                    "attachment; filename=" + fileName);
                        res.BinaryWrite(ms.ToArray());

                        ms.Close();
                        ms.Dispose();
                    }
                }
                catch
                {
                    throw;
                }
            }
        }

        //製造EXCEL的內容
        public XSSFWorkbook ExoprtToExcel(DataTable data, string tt1,string tt2, string tt3)
        {
            var wb = new XSSFWorkbook();
            var sheet = (XSSFSheet)wb.CreateSheet("Sheet1");

            IRow row = sheet.CreateRow(0);
            row.CreateCell(0).SetCellValue("衛星庫房(" + tt1 + ")" + tt2 + "類庫存成本單位明細報表 (FA0023) (" + tt3 + ")");

            row = sheet.CreateRow(1);
            for (int i = 0; i < data.Columns.Count; i++)
            {
                row.CreateCell(i).SetCellValue(data.Columns[i].ToString());
            }

            for (int i = 0; i < data.Rows.Count; i++)
            {
                row = sheet.CreateRow(2 + i);
                for (int j = 0; j < data.Columns.Count; j++)
                {
                    row.CreateCell(j).SetCellValue(data.Rows[i].ItemArray[j].ToString());
                }
            }
            return wb;
        }

        [HttpGet]
        public ApiResponse GetMatCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0023Repository(DBWork);
                    session.Result.etts = repo.GetMatCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpGet]
        public ApiResponse GetYMCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0023Repository(DBWork);
                    session.Result.etts = repo.GetYMCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //public ApiResponse GetWh_no()
        //{
        //    using (WorkSession session = new WorkSession(this))
        //    {
        //        var DBWork = session.UnitOfWork;
        //        try
        //        {
        //            var repo = new FA0023Repository(DBWork);
        //            session.Result.etts = repo.GetWh_no();
        //        }
        //        catch
        //        {
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //}

        [HttpPost]
        public ApiResponse GetMMCodeCombo(FormDataCollection form)
        {
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    var p0 = form.Get("MAT_CLASS");
                    var p1 = form.Get("MMCODE");
                    var clsALL = bool.Parse(form.Get("clsALL"));
                    //var wh_no = form.Get("WH_NO");

                    if (clsALL == true)
                    {
                        var repo = new FA0023Repository(DBWork);
                        string _p0 = p0.Substring(0, p0.Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = _p0.Split(',');
                        p0 = "";
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            p0 += "'" + tmp[i] + "',";
                        }
                        p0 = p0.Substring(0, p0.Length - 1);
                        session.Result.etts = repo.GetMMCodeCombo(p0, p1, clsALL, page, limit, sorters);
                    }
                    else
                    {
                        var repo = new FA0023Repository(DBWork);
                        session.Result.etts = repo.GetMMCodeCombo(p0, p1, clsALL, page, limit, sorters);
                    }

                    //FA0023Repository repo = new FA0023Repository(DBWork);
                    //session.Result.etts = repo.GetMMCodeCombo(p0, p1, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetWH_NoCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            //var wh_no = form.Get("WH_NO");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0023Repository repo = new FA0023Repository(DBWork);
                    session.Result.etts = repo.GetWH_NoCombo(p0, page, limit, "")
                        .Select(w => new { WH_NO = w.WH_NO, WH_NAME = w.WH_NAME, WH_KIND = w.WH_KIND, WH_GRADE = w.WH_GRADE });
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMMCodeComboQ(FormDataCollection form)
        {
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    var p0 = form.Get("MAT_CLASS");
                    var p1 = form.Get("MMCODE");
                    var clsALL = bool.Parse(form.Get("clsALL"));
                    //var wh_no = form.Get("WH_NO");

                    if (clsALL == true)
                    {
                        var repo = new FA0023Repository(DBWork);
                        string _p0 = p0.Substring(0, p0.Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = _p0.Split(',');
                        p0 = "";
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            p0 += "'" + tmp[i] + "',";
                        }
                        p0 = p0.Substring(0, p0.Length - 1);
                        session.Result.etts = repo.GetMMCodeComboQ(p0, p1, clsALL, page, limit, sorters);
                    }
                    else
                    {
                        var repo = new FA0023Repository(DBWork);
                        session.Result.etts = repo.GetMMCodeComboQ(p0, p1, clsALL, page, limit, sorters);
                    }

                    //AA0074Repository repo = new AA0074Repository(DBWork);
                    //session.Result.etts = repo.GetMMCodeCombo(p0, p1, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpGet]
        public ApiResponse GetYNCombo() {
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0023Repository(DBWork);
                    session.Result.etts = repo.GetYNCombo();
                }
                catch (Exception e) {
                    throw;
                }
                return session.Result;
            }
        }
    }
}
