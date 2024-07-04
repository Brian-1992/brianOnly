using JCLib.DB;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using WebApp.Models;
using WebApp.Models.AA;
using WebApp.Repository.AA;

namespace WebApp.Controllers.AA
{
    public class AA0187Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse GetAll(FormDataCollection form)
        {
            string wh_no = form.Get("WH_NO") != null ? form.Get("WH_NO").Trim() : form.Get("WH_NO");   // 庫房
            string mmcode = form.Get("MMCODE") != null ? form.Get("MMCODE").Trim() : form.Get("MMCODE"); // 院內碼
            string warbak = form.Get("WARBAK") != null ? form.Get("WARBAK").Trim() : form.Get("WARBAK"); //是否戰備(0:非戰備 1:戰備)
            string mat_class = form.Get("MAT_CLASS") != null ? form.Get("MAT_CLASS").Trim() : form.Get("MAT_CLASS");   //藥材類別()
            string m_contid = form.Get("M_CONTID") != null ? form.Get("M_CONTID").Trim() : form.Get("M_CONTID"); //合約碼
            string e_sourcecode = form.Get("E_SOURCECODE") != null ? form.Get("E_SOURCECODE").Trim() : form.Get("E_SOURCECODE"); //買斷寄庫
            string e_restrictcode = form.Get("E_RESTRICTCODE") != null ? form.Get("E_RESTRICTCODE").Trim() : form.Get("E_RESTRICTCODE"); // 管制品項
            string common = form.Get("COMMON") != null ? form.Get("COMMON").Trim() : form.Get("COMMON"); // 是否常用品項(1:非常用品  2:常用品 3:藥品  4:檢驗)
            string spdrug = form.Get("SPDRUG") != null ? form.Get("SPDRUG").Trim() : form.Get("SPDRUG"); // 特殊品項(0非特殊品項;1特殊品項)
            string fastdrug = form.Get("FASTDRUG") != null ? form.Get("FASTDRUG").Trim() : form.Get("FASTDRUG"); // 急救品項(0非急救品項;1急救品項)
            string isiv = form.Get("ISIV") != null ? form.Get("ISIV").Trim() : form.Get("ISIV"); // 是否點滴
            string orderkind = form.Get("ORDERKIND") != null ? form.Get("ORDERKIND").Trim() : form.Get("ORDERKIND"); // 採購類別(0:無, 1:常備品項 2:小額採購)
            string spxfee = form.Get("SPXFEE") != null ? form.Get("SPXFEE").Trim() : form.Get("SPXFEE"); // 是否為特材(0:非特材, 1:特材)
            string drugkind = form.Get("DRUGKIND") != null ? form.Get("DRUGKIND").Trim() : form.Get("DRUGKIND"); // 藥品之中西藥類別(0:非藥品, 1:西藥, 2:中藥)
            string touchcase = form.Get("TOUCHCASE") != null ? form.Get("TOUCHCASE").Trim() : form.Get("TOUCHCASE"); // 藥品之中西藥類別(0:非藥品, 1:西藥, 2:中藥)
            string agen_no = form.Get("AGEN_NO") != null ? form.Get("AGEN_NO").Trim() : form.Get("AGEN_NO"); // 廠商代碼
            string agen_name = form.Get("AGEN_NAME") != null ? form.Get("AGEN_NAME").Trim() : form.Get("AGEN_NAME"); // 廠商名稱
            string qtyiszero = form.Get("QTYISZERO") != null ? form.Get("QTYISZERO").Trim() : form.Get("QTYISZERO");   // 存量<>0
            string qtyisnotzero = form.Get("QTYISNOTZERO") != null ? form.Get("QTYISNOTZERO").Trim() : form.Get("QTYISNOTZERO");   // 存量=0
            string qtyischanged = form.Get("QTYISCHANGED") != null ? form.Get("QTYISCHANGED").Trim() : form.Get("QTYISCHANGED");   // 庫存有異動
            string qtyisnotchanged = form.Get("QTYISNOTCHANGED") != null ? form.Get("QTYISNOTCHANGED").Trim() : form.Get("QTYISNOTCHANGED");   // 庫存無異動
            bool isAA = form.Get("ISAA") == "Y";

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0187Repository(DBWork);

                    session.Result.etts = repo.GetAll(wh_no, mmcode, warbak, mat_class,
                                m_contid, e_sourcecode, e_restrictcode,
                                common, spdrug, fastdrug, isiv,
                                orderkind, spxfee, drugkind, agen_no,
                                agen_name, touchcase,
                                qtyiszero, qtyisnotzero, qtyischanged, qtyisnotchanged,
                                isAA, DBWork.UserInfo.UserId);

                    return session.Result;
                }
                catch (Exception e)
                {
                    throw;
                }
            }
        }

        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            string wh_no = form.Get("WH_NO") != null ? form.Get("WH_NO").Trim() : form.Get("WH_NO");   // 庫房
            string mmcode = form.Get("MMCODE") != null ? form.Get("MMCODE").Trim() : form.Get("MMCODE"); // 院內碼
            string warbak = form.Get("WARBAK") != null ? form.Get("WARBAK").Trim() : form.Get("WARBAK"); //是否戰備(0:非戰備 1:戰備)
            string mat_class = form.Get("MAT_CLASS") != null ? form.Get("MAT_CLASS").Trim() : form.Get("MAT_CLASS");   //藥材類別()
            string m_contid = form.Get("M_CONTID") != null ? form.Get("M_CONTID").Trim() : form.Get("M_CONTID"); //合約碼
            string e_sourcecode = form.Get("E_SOURCECODE") != null ? form.Get("E_SOURCECODE").Trim() : form.Get("E_SOURCECODE"); //買斷寄庫
            string e_restrictcode = form.Get("E_RESTRICTCODE") != null ? form.Get("E_RESTRICTCODE").Trim() : form.Get("E_RESTRICTCODE"); // 管制品項
            string common = form.Get("COMMON") != null ? form.Get("COMMON").Trim() : form.Get("COMMON"); // 是否常用品項(1:非常用品  2:常用品 3:藥品  4:檢驗)
            string spdrug = form.Get("SPDRUG") != null ? form.Get("SPDRUG").Trim() : form.Get("SPDRUG"); // 特殊品項(0非特殊品項;1特殊品項)
            string fastdrug = form.Get("FASTDRUG") != null ? form.Get("FASTDRUG").Trim() : form.Get("FASTDRUG"); // 急救品項(0非急救品項;1急救品項)
            string isiv = form.Get("ISIV") != null ? form.Get("ISIV").Trim() : form.Get("ISIV"); // 是否點滴
            string orderkind = form.Get("ORDERKIND") != null ? form.Get("ORDERKIND").Trim() : form.Get("ORDERKIND"); // 採購類別(0:無, 1:常備品項 2:小額採購)
            string spxfee = form.Get("SPXFEE") != null ? form.Get("SPXFEE").Trim() : form.Get("SPXFEE"); // 是否為特材(0:非特材, 1:特材)
            string drugkind = form.Get("DRUGKIND") != null ? form.Get("DRUGKIND").Trim() : form.Get("DRUGKIND"); // 藥品之中西藥類別(0:非藥品, 1:西藥, 2:中藥)
            string touchcase = form.Get("TOUCHCASE") != null ? form.Get("TOUCHCASE").Trim() : form.Get("TOUCHCASE"); // 藥品之中西藥類別(0:非藥品, 1:西藥, 2:中藥)
            string agen_no = form.Get("AGEN_NO") != null ? form.Get("AGEN_NO").Trim() : form.Get("AGEN_NO"); // 廠商代碼
            string agen_name = form.Get("AGEN_NAME") != null ? form.Get("AGEN_NAME").Trim() : form.Get("AGEN_NAME"); // 廠商名稱
            string qtyiszero = form.Get("QTYISZERO") != null ? form.Get("QTYISZERO").Trim() : form.Get("QTYISZERO");   // 存量<>0
            string qtyisnotzero = form.Get("QTYISNOTZERO") != null ? form.Get("QTYISNOTZERO").Trim() : form.Get("QTYISNOTZERO");   // 存量=0
            string qtyischanged = form.Get("QTYISCHANGED") != null ? form.Get("QTYISCHANGED").Trim() : form.Get("QTYISCHANGED");   // 庫存有異動
            string qtyisnotchanged = form.Get("QTYISNOTCHANGED") != null ? form.Get("QTYISNOTCHANGED").Trim() : form.Get("QTYISNOTCHANGED");   // 庫存無異動
            bool isAA = form.Get("ISAA") == "Y";

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0187Repository(DBWork);

                    DataTable data = repo.GetExcel(wh_no, mmcode, warbak, mat_class,
                                m_contid, e_sourcecode, e_restrictcode,
                                common, spdrug, fastdrug, isiv,
                                orderkind, spxfee, drugkind, agen_no,
                                agen_name, touchcase,
                                qtyiszero, qtyisnotzero, qtyischanged, qtyisnotchanged,
                                isAA, DBWork.UserInfo.UserId);
                    string fileName = "現有存量明細查詢" + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";
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
                    if ((j > 3 && j < 22) || (j == 28))
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

        //庫房代碼
        [HttpPost]
        public ApiResponse GetWhnoCombo(FormDataCollection form)
        {
            bool isAA = form.Get("ISAA") == "Y";
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0187Repository(DBWork);
                    session.Result.etts = repo.GetWhnoCombo(isAA, DBWork.UserInfo.UserId);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //預設庫房代碼
        [HttpPost]
        public ApiResponse GetDefaultWhNo(FormDataCollection form)
        {
            string menuCode = form.Get("menuCode");
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0187Repository(DBWork);
                    session.Result.etts = repo.DefaultWhNo(menuCode);
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
                    var repo = new AA0187Repository(DBWork);
                    session.Result.etts = repo.GetMatClassSubParamCombo();
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
                    AA0187Repository repo = new AA0187Repository(DBWork);
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
                    AA0187Repository repo = new AA0187Repository(DBWork);
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
                    AA0187Repository repo = new AA0187Repository(DBWork);
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
                    AA0187Repository repo = new AA0187Repository(DBWork);
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
                    AA0187Repository repo = new AA0187Repository(DBWork);
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
                    AA0187Repository repo = new AA0187Repository(DBWork);
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
                    AA0187Repository repo = new AA0187Repository(DBWork);
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
                    AA0187Repository repo = new AA0187Repository(DBWork);
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
                    AA0187Repository repo = new AA0187Repository(DBWork);
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
                    AA0187Repository repo = new AA0187Repository(DBWork);
                    session.Result.etts = repo.GetSpecialorderkindParamCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //是否特材
        [HttpPost]
        public ApiResponse GetSpxfeeParamCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0187Repository repo = new AA0187Repository(DBWork);
                    session.Result.etts = repo.GetSpxfeeParamCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //廠商代碼
        [HttpPost]
        public ApiResponse GetAgennoCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0187Repository repo = new AA0187Repository(DBWork);
                    session.Result.etts = repo.GetAgennoCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //是否點滴
        [HttpPost]
        public ApiResponse GetIsivParamCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0187Repository repo = new AA0187Repository(DBWork);
                    session.Result.etts = repo.GetIsivCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetMmCodeCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0187Repository(DBWork);
                    session.Result.etts = repo.GetMmCodeCombo(p0, page, limit, "");
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }
        //列印藥材種類
        [HttpPost]
        public ApiResponse GetMatClassCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0187Repository(DBWork);
                    session.Result.etts = repo.GetMatClassCombo();
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