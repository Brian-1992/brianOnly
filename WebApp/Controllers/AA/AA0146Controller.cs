using System;
using System.Collections.Generic;
using System.Web;
using System.Data;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System.IO;
using NPOI.XSSF.UserModel;
using WebApp.Models.AA;
using Newtonsoft.Json;

namespace WebApp.Controllers.AA
{
    public class AA0146Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse GetFL_NAMECombo()
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0146Repository(DBWork);
                    session.Result.etts = repo.GetFL_NAMECombo();
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }
        //查詢
        [HttpPost]
        public ApiResponse GetAll(FormDataCollection form)
        {
            var p0 = form.Get("p0"); //fl_name
            var p1 = form.Get("p1"); //Control_Id
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0146Repository(DBWork);
                    string hospCode = repo.GetHospCode();
                    session.Result.etts = repo.GetAll(p0, p1,hospCode=="0", page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetControl(FormDataCollection form)
        {
            var p2 = form.Get("p2"); //fl_name            
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0146Repository(DBWork);
                    session.Result.etts = repo.GetControl(p2, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // 匯出
        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            var fl_name = form.Get("fl_name");
            string FN = form.Get("FN");
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    DataTable data = new DataTable();

                    var repo = new AA0146Repository(DBWork);
                    data = repo.GetExcel(fl_name);
                    if (data.Rows.Count > 0)
                    {
                        var workbook = ExoprtToExcel(data);

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
                                    "attachment; filename=" + FN);
                        res.BinaryWrite(ms.ToArray());

                        ms.Close();
                        ms.Dispose();
                    }
                    //JCLib.Excel.Export(form.Get("FN"), dtItems);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public XSSFWorkbook ExoprtToExcel(DataTable data)
        {
            var wb = new XSSFWorkbook();
            var sheet = (XSSFSheet)wb.CreateSheet("Sheet1");

            Dictionary<string, string> dictHeader = new Dictionary<string, string>{
                { "fl_name", "管制檔名稱"},
                { "seq_no", "項次"},
                { "nob_no", "動員序號"},
                { "mat_name", "類型"},
                { "mmname", "品項"},
                { "e_specnunit", "規格"},
                { "e_drugform", "劑型_類別"},
                { "wresqty", "三總須採購囤儲數量"},
                { "transqty", "依包裝規格換算須採購量"},
                { "pur_mmcode", "規劃採購品項院內碼"},
                { "wres_mmcode", "專用戰略物資院內碼"},
                { "mmname_e", "院內品名"},
                { "mmname_c", "中文品名"},
                { "agen_name", "合約商"},
                { "e_itemarmyno", "軍聯標項次"},
                { "m_contprice", "發票單價"},
                { "disc_cprice", "實售價"},
                { "pur_qty", "採購數量"},
                { "pur_amt", "採購金額"},
                { "m_storeid", "採購方式}" }
            };
            string strVal = "";

            // Oracel 12 以下，如果 select 超過 column length 限制 會噴錯，通常發生在給予中文命名的問題
            IRow row = sheet.CreateRow(0);
            for (int i = 0; i < data.Columns.Count; i++)
            {
                if (dictHeader.TryGetValue(data.Columns[i].ToString().ToLower(), out strVal))
                {
                    row.CreateCell(i).SetCellValue(strVal);
                }
                else
                {
                    row.CreateCell(i).SetCellValue(data.Columns[i].ToString());
                }
            }

            for (int i = 1; i < data.Rows.Count; i++)
            {
                row = sheet.CreateRow(i);
                for (int j = 0; j < data.Columns.Count; j++)
                {
                    row.CreateCell(j).SetCellValue(data.Rows[i].ItemArray[j].ToString());
                }
            }
            return wb;
        }

        #region upload
        [HttpPost]
        public ApiResponse Upload() //[匯入]呼叫
        {
            using (WorkSession session = new WorkSession(this))
            {

                List<AA0146> list = new List<AA0146>();
                UnitOfWork DBWork = session.UnitOfWork;

                try
                {
                    AA0146Repository repo = new AA0146Repository(DBWork);

                    #region data upload
                    List<HeaderItem> headerItems = new List<HeaderItem>() {
                        new HeaderItem("管制檔名稱", "FL_NAME"),
                        new HeaderItem("項次", "SEQ_NO"),
                        new HeaderItem("動員序號", "NOB_NO"),
                        new HeaderItem("類型", "MAT_NAME"),
                        new HeaderItem("品項", "MMNAME"),
                        new HeaderItem("規格", "E_SPECNUNIT"),
                        new HeaderItem("劑型_類別", "E_DRUGFORM"),
                        new HeaderItem("三總須採購囤儲數量", "WRESQTY"),
                        new HeaderItem("依包裝規格換算須採購量", "TRANSQTY"),
                        new HeaderItem("規劃採購品項院內碼", "PUR_MMCODE"),
                        new HeaderItem("專用戰略物資院內碼", "WRES_MMCODE"),
                        new HeaderItem("院內品名", "MMNAME_E"),
                        new HeaderItem("中文品名", "MMNAME_C"),
                        new HeaderItem("合約商", "AGEN_NAME"),
                        new HeaderItem("軍聯標項次", "M_CONTPRICE"),
                        new HeaderItem("發票單價", "E_ITEMARMYNO"),
                        new HeaderItem("實售價", "DISC_CPRICE"),
                        new HeaderItem("採購數量", "PUR_QTY"),
                        new HeaderItem("採購金額", "PUR_AMT"),
                        new HeaderItem("採購方式", "M_STOREID"),
                    };

                    IWorkbook workBook;
                    var HttpPostedFile = HttpContext.Current.Request.Files["file"];

                    if (Path.GetExtension(HttpPostedFile.FileName).ToLower() == ".xls")
                    {
                        workBook = new HSSFWorkbook(HttpPostedFile.InputStream);
                    }
                    else
                    {
                        workBook = new XSSFWorkbook(HttpPostedFile.InputStream);
                    }
                    var sheet = workBook.GetSheetAt(0);
                    IRow headerRow = sheet.GetRow(0);//由第一列取標題做為欄位名稱
                    int cellCount = headerRow.LastCellNum;

                    headerItems = SetHeaderIndex(headerItems, headerRow);
                    // 確定該有的欄位都有，沒有的回傳錯誤訊息
                    string errMsg = string.Empty;
                    foreach (HeaderItem item in headerItems)
                    {
                        if (item.Index == -1)
                        {
                            if (errMsg == string.Empty)
                            {
                                errMsg += item.Name;
                            }
                            else
                            {
                                errMsg += string.Format("、{0}", item.Name);
                            }
                        }
                    }

                    if (errMsg != string.Empty)
                    {
                        session.Result.success = false;
                        session.Result.msg = string.Format("欄位錯誤，缺：{0}", errMsg);
                        return session.Result;
                    }

                    for (int i = 1; i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row is null) { continue; }

                        AA0146 temp = new AA0146();
                        foreach (HeaderItem item in headerItems)
                        {
                            string value = row.GetCell(item.Index) == null ? string.Empty : row.GetCell(item.Index).ToString();
                            if (item.FieldName == "SEQ_NO")
                            {
                                if (value.Length == 1)
                                { value = "00" + value.ToString(); }

                                if (value.Length == 2)
                                { value = "0" + value.ToString(); }

                            }
                            if ((item.FieldName == "WRESQTY") || (item.FieldName == "TRANSQTY ") ||
                            (item.FieldName == "M_CONTPRICE") || (item.FieldName == "DISC_CPRICE") ||
                            (item.FieldName == "PUR_QTY") || (item.FieldName == "M_STOREID"))
                            {
                                value = value.Replace(",", "");
                            }
                            temp.GetType().GetProperty(item.FieldName).SetValue(temp, value, null);
                        }
                        temp.Seq = i - 1;
                        list.Add(temp);
                    }
                    #endregion

                    foreach (AA0146 item in list)
                    {
                        string msg = string.Empty;
                        string temp_msg = string.Empty;

                        item.SaveStatus = "Y";  // N:Error Y:Insert

                        //確認專用戰略物資院內碼是否存在於藥品基本檔
                        if (repo.CheckExists_Mimast(item.WRES_MMCODE))
                        {
                            temp_msg = "專用戰略物資院內碼不存在於藥品基本檔";
                            msg = msg == string.Empty ? temp_msg : string.Format("{0}、{1}", msg, temp_msg);
                            item.SaveStatus = "N";
                        }

                        ////確認專用戰略物資院內碼是否存在於藥品基本檔
                        //if (repo.CheckExists_Warres_Ctl(item.WRES_MMCODE))
                        //{
                        //    temp_msg = "專用戰略物資院內碼存在於戰備庫管制檔";
                        //    msg = msg == string.Empty ? temp_msg : string.Format("{0}、{1}", msg, temp_msg);
                        //    item.SaveStatus = "N";
                        //}

                        item.UploadMsg = msg;
                    }

                    session.Result.etts = list;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public class HeaderItem
        {
            public string Name { get; set; }
            public string FieldName { get; set; }
            public int Index { get; set; }
            public HeaderItem()
            {
                Name = string.Empty;
                Index = -1;
                FieldName = string.Empty;
            }
            public HeaderItem(string name, int index, string fieldName)
            {
                Name = name;
                Index = index;
                FieldName = fieldName;
            }
            public HeaderItem(string name, string fieldName)
            {
                Name = name;
                Index = -1;
                FieldName = fieldName;
            }
        }

        public List<HeaderItem> SetHeaderIndex(List<HeaderItem> list, IRow headerRow)
        {
            int cellCounts = headerRow.LastCellNum;
            for (int i = 0; i < cellCounts; i++)
            {
                foreach (HeaderItem item in list)
                {
                    if (headerRow.GetCell(i).ToString() == item.Name)
                    {
                        item.Index = i;
                    }
                }
            }

            return list;
        }
        #endregion

        [HttpPost]
        public ApiResponse UploadConfirm(FormDataCollection form)  //[確定上傳]呼叫
        {
            string itemString = form.Get("data");
            IEnumerable<AA0146> list = JsonConvert.DeserializeObject<IEnumerable<AA0146>>(itemString);

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0146Repository repo = new AA0146Repository(DBWork);

                    foreach (AA0146 item in list)
                    {
                        item.CreateUser = User.Identity.Name;
                        item.UpdateIp = DBWork.ProcIP;

                        session.Result.afrs = repo.Delete_import(item.FL_NAME, item.SEQ_NO);
                        session.Result.afrs = repo.Create_import(item);

                    }

                    DBWork.Commit();
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }

        }
    }
}