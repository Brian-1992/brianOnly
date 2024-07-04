using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.F;
using NPOI.SS.UserModel;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using WebApp.Models;
using System.Data;
using Newtonsoft.Json;
using System.Text;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace WebApp.Controllers.F
{


    public class FA0063Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            string start = form.Get("P0");
            string end = form.Get("P1");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0063Repository(DBWork);
                    session.Result.etts = repo.GetAll(start, end, true);

                    return session.Result;
                }
                catch (Exception e)
                {
                    throw;
                }
            }
        }

        [HttpGet]
        public ApiResponse GetHospId()
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0063Repository(DBWork);
                    session.Result.msg = repo.GetHospId();
                    return session.Result;
                }
                catch (Exception e)
                {
                    throw;
                }

            }
        }

        [HttpPost]
        public ApiResponse UpdateHospId(FormDataCollection form)
        {
            string hospId = form.Get("hospId");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new FA0063Repository(DBWork);
                    session.Result.afrs = repo.UpdateHospId1(hospId);
                    session.Result.afrs = repo.UpdateHospId2(hospId);
                    session.Result.success = true;
                    DBWork.Commit();
                    return session.Result;
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    throw;
                }

            }
        }

        [HttpGet]
        public ApiResponse CheckSetRatio()
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0063Repository(DBWork);
                    session.Result.msg = repo.CheckSetRatioEmpty() ? "Y" : "N";
                    return session.Result;
                }
                catch (Exception e)
                {
                    throw;
                }
            }
        }

        #region upload
        [HttpPost]
        public ApiResponse Upload()
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new FA0063Repository(DBWork);
                    // 刪除現有資料
                    session.Result.afrs = repo.DeleteNhiPsurvList();

                    List<NHI_PSURV_LIST> list = new List<NHI_PSURV_LIST>();

                    #region data upload
                    List<HeaderItem> headerItems = new List<HeaderItem>() {
                       new HeaderItem("序號", "PS_SEQ"),
                        new HeaderItem("健保代碼", "M_NHIKEY"),
                        new HeaderItem("中文名稱", "MMNAME_C"),
                        new HeaderItem("藥品名稱", "MMNAME_E"),
                        new HeaderItem("規格量", "E_SPEC"),
                        new HeaderItem("規格單位", "E_UNIT"),
                       new HeaderItem("劑型","E_DRUGFORM"),
                        new HeaderItem("藥商名稱","AGEN_NAME"),
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

                        NHI_PSURV_LIST temp = new NHI_PSURV_LIST();
                        foreach (HeaderItem item in headerItems)
                        {
                            string value = row.GetCell(item.Index) == null ? string.Empty : row.GetCell(item.Index).ToString();
                            temp.GetType().GetProperty(item.FieldName).SetValue(temp, value, null);
                        }
                        temp.Seq = i - 1;

                        temp.CREATE_USER = DBWork.UserInfo.UserId;
                        temp.UPDATE_IP = DBWork.ProcIP;
                        list.Add(temp);
                    }
                    #endregion

                    // insert NHI_PSURV_LIST
                    foreach (NHI_PSURV_LIST item in list)
                    {
                        session.Result.afrs += repo.InsertNhiPsurvList(item);
                    }

                    DBWork.Commit();
                    session.Result.success = true;
                    return session.Result;
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    throw;
                }
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

        [HttpGet]
        public ApiResponse GetLastUploadTime()
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0063Repository(DBWork);
                    session.Result.msg = repo.GetLastUploadTime();
                    return session.Result;
                }
                catch (Exception e)
                {
                    throw;
                }
            }
        }

        #region 設定倍率
        [HttpPost]
        public ApiResponse GetNhipsurvlistInit(FormDataCollection form)
        {

            bool reCal = (form != null && form.Get("reCal") == "Y");
            List<string> timeOneBaseUnits = new List<string>() {
                "SYRING","SYRIN","BOT","TAB","CAP","PIE","PIECE","TUB","TUBE","BAG","PKG"
            };

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();

                try
                {
                    var repo = new FA0063Repository(DBWork);

                    if (reCal)
                    {
                        session.Result.afrs = repo.ReCal();
                    }

                    IEnumerable<NHI_PSURV_LIST> list = repo.GetNhipsurvlist(false, false);
                    IEnumerable<NHI_PSURV_LIST> temp = list.Where(x => x.HIS_BASE_UNIT == null).Select(x => x).ToList();
                    // 檢查是否有建議倍率
                    // 有 => 查詢，無 => 重新計算
                    bool emptyRcmRatio = list.Where(x => x.RCM_RATIO == null).Any();

                    if (emptyRcmRatio)
                    {

                        #region 設定倍率
                        // 檢查換算
                        foreach (NHI_PSURV_LIST item in list)
                        {
                            // 1. 若 (HIS_藥品劑型='點眼液劑') and (HIS_計量單位='BOX') => 異常訊息
                            if (item.HIS_E_DRUGFORM == "點眼液劑" && item.HIS_BASE_UNIT == "BOX")
                            {
                                item.RCM_RATIO = "*";
                                continue;
                            }
                            // 2. 若 HIS_計量單位(I欄) in (SYRING,SYRIN,BOT,TAB,CAP,PIE,PIECE,TUB,TUBE,BAG,PKG) => 倍數1
                            if (timeOneBaseUnits.Contains(item.HIS_BASE_UNIT))
                            {
                                item.RCM_RATIO = "1";
                                item.SET_RATIO = item.RCM_RATIO;
                                continue;
                            }
                            // 3. 轉換NHI_規格量 E_SPEC 為小數
                            double d = 0;
                            if (item.PS_SEQ == "390")
                            {
                                string t = string.Empty;
                            }
                            if (double.TryParse(item.E_SPEC, out d))
                            {
                                item.E_SPEC = double.Parse(item.E_SPEC).ToString();
                            }
                            /*取得劑量單位列表*/

                            /* 4.
                             若 NHI_規格單位(N欄)='IU' and NHI_規格量(M欄)=1
                                 and HIS_劑量(規格量及單位) (J欄)='1 IU'
                                 and HIS_成份(K欄) like '%IU/'||HIS_計量單位
                              則 (發票購買藥品數量A) * (HIS_成份，取IU前的數值)
                             */
                            if (item.E_SPEC == "1" && item.E_UNIT == "IU" &&
                                (item.HIS_E_SPECNUNIT == "1 IU" || item.HIS_E_SPECNUNIT == "1IU") &&
                                item.HIS_E_COMPUNIT.Contains(string.Format("IU/{0}", item.HIS_BASE_UNIT))
                                )
                            {
                                string[] splitValues = item.HIS_E_COMPUNIT.Split(new string[] { string.Format("IU/{0}", item.HIS_BASE_UNIT) }, StringSplitOptions.None);
                                bool parsable = double.TryParse(splitValues[0], out d);
                                if (parsable == true)
                                {
                                    item.RCM_RATIO = Math.Round(double.Parse(splitValues[0]), 0).ToString();
                                    item.SET_RATIO = item.RCM_RATIO;
                                }
                                else
                                {
                                    item.RCM_RATIO = "*";
                                }
                                continue;
                            }
                            /* 5. 
                              若 NHI_規格單位(N欄)='IU' and NHI_規格量(M欄)=1 and HIS_劑量(規格量及單位)<>'1 IU'
                                 and HIS_成份 like '%IU/'||HIS_計量單位
                              則 發票購買藥品數量A*(HIS_成份，取IU前的數值)
                            */
                            if (item.E_SPEC == "1" && item.E_UNIT == "IU" &&
                                (item.HIS_E_SPECNUNIT != "1 IU" && item.HIS_E_SPECNUNIT != "1IU") &&
                                item.HIS_E_COMPUNIT.Contains(string.Format("IU/{0}", item.HIS_BASE_UNIT))
                                )
                            {
                                string[] splitValues = item.HIS_E_COMPUNIT.Split(new string[] { string.Format("IU/{0}", item.HIS_BASE_UNIT) }, StringSplitOptions.None);
                                bool parsable = double.TryParse(splitValues[0], out d);
                                if (parsable == true)
                                {
                                    item.RCM_RATIO = Math.Round(double.Parse(splitValues[0]), 0).ToString();
                                    item.SET_RATIO = item.RCM_RATIO;
                                }
                                else
                                {
                                    item.RCM_RATIO = "*";
                                }
                                continue;
                            }
                            /* 6.
                             若 NHI_規格單位(N欄)='IU' and NHI_規格量(M欄)<>1
                                and (NHI_規格量||' '||NHI_規格單位)=HIS_劑量(規格量及單位)
                                則 發票購買藥品數量A
                            */
                            if (item.E_UNIT.Trim() == "IU" && item.E_SPEC != "1" &&
                                item.HIS_E_SPECNUNIT == string.Format("{0} {1}", item.E_SPEC, item.E_UNIT))
                            {
                                item.RCM_RATIO = "1";
                                item.SET_RATIO = item.RCM_RATIO;
                                continue;
                            }
                            /* 7. 
                             若 NHI_規格單位(N欄)='IU' and NHI_規格量(M欄)<>1
                                and (NHI_規格量||' '||NHI_規格單位)<>HIS_劑量(規格量及單位)
                                and HIS_成份=NHI_規格量||' '||'IU/'||HIS_計量單位
                             則 發票購買藥品數量A
                            */
                            if (item.E_UNIT.Trim() == "IU" && item.E_SPEC != "1" &&
                                item.HIS_E_SPECNUNIT != string.Format("{0} {1}", item.E_SPEC, item.E_UNIT) &&
                                item.HIS_E_COMPUNIT == string.Format("{0} IU/{1}", item.E_SPEC, item.HIS_BASE_UNIT))
                            {
                                item.RCM_RATIO = "1";
                                item.SET_RATIO = item.RCM_RATIO;
                                continue;
                            }
                            /* 8. 
                                若 NHI_規格單位<>'IU'
                                   and 轉換[NHI_規格量,NHI_規格單位]=HIS_劑量(規格量及單位)
                                則 發票購買藥品數量A
                           */
                            if (string.IsNullOrEmpty(item.E_SPEC.Trim()))
                            {
                                item.RCM_RATIO = "*";
                                continue;
                            }

                            // 轉換完成單位
                            IEnumerable<string> units = repo.GetUnitCnvs(item.E_UNIT, item.E_SPEC, item.HIS_BASE_UNIT);

                            if (item.E_UNIT.Trim() != "IU" &&
                                units.Contains(item.HIS_E_SPECNUNIT))
                            {
                                item.RCM_RATIO = "1";
                                item.SET_RATIO = item.RCM_RATIO;
                                continue;
                            }
                            /*9. 若 NHI_規格單位<>'IU'
                                    and 轉換[NHI_規格量,NHI_規格單位]<>HIS_劑量(規格量及單位)
                                    and 轉換[NHI_規格量,NHI_規格單位,HIS_計量單位]=HIS_成份
                                 則 發票購買藥品數量A
                            */
                            if (item.E_UNIT.Trim() != "IU" &&
                               units.Contains(item.HIS_E_SPECNUNIT) == false &&
                               units.Contains(item.HIS_E_COMPUNIT))
                            {
                                item.RCM_RATIO = "1";
                                item.SET_RATIO = item.RCM_RATIO;
                                continue;
                            }
                            if (string.IsNullOrEmpty(item.RCM_RATIO))
                            {
                                item.RCM_RATIO = "*";
                            }
                        }
                        #endregion

                        // 更新倍率
                        foreach (NHI_PSURV_LIST item in list)
                        {
                            session.Result.afrs = repo.UpdateNhipsurvlistRatios(item.PS_SEQ, item.RCM_RATIO, item.SET_RATIO);
                        }
                        DBWork.Commit();
                    }

                    session.Result.success = true;

                    return session.Result;
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    throw;
                }
            }
        }
        [HttpPost]
        public ApiResponse GetNhipsurvlist(FormDataCollection form)
        {
            bool starOnly = form.Get("starOnly") == "Y";

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0063Repository(DBWork);
                    session.Result.etts = repo.GetNhipsurvlist(starOnly, true);

                    return session.Result;
                }
                catch (Exception e)
                {
                    throw;
                }
            }
        }

        [HttpPost]
        public ApiResponse UpdateNhupsurvlistSetRatio(FormDataCollection form)
        {
            string itemString = form.Get("itemString");
            IEnumerable<NHI_PSURV_LIST> items = JsonConvert.DeserializeObject<IEnumerable<NHI_PSURV_LIST>>(itemString);
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new FA0063Repository(DBWork);
                    foreach (NHI_PSURV_LIST item in items)
                    {
                        session.Result.afrs = repo.UpdateNhipsurvlistRatios(item.PS_SEQ, item.RCM_RATIO, item.SET_RATIO);
                    }
                    DBWork.Commit();
                    session.Result.success = true;
                    return session.Result;
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    throw;

                }
            }
        }

        #endregion


        #region Excel
        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            string start_ym = form.Get("P0");
            string end_ym = form.Get("P1");
            using (WorkSession session = new WorkSession(this))
            {

                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0063Repository(DBWork);

                    DataTable data = repo.GetExcel(start_ym, end_ym);

                    var workbook = ExoprtToExcel(data);

                    //output
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        workbook.Write(memoryStream);
                        JCLib.Export.OutputFile(memoryStream, string.Format("價量調查表_{0}-{1}.xlsx", start_ym, end_ym));
                        workbook.Close();
                    }
                }
                catch (Exception e)
                {
                    throw;
                }

                return session.Result;
            }
        }
        public XSSFWorkbook ExoprtToExcel(DataTable data)
        {
            IEnumerable<string> yyymms = data.AsEnumerable().Select(x => x.Field<string>("F03")).OrderBy(x => x).Distinct();

            var wb = new XSSFWorkbook();

            if (yyymms.Any() == false)
            {
                var sheet = (XSSFSheet)wb.CreateSheet("Sheet1");
                IRow row = sheet.CreateRow(0);
                row.CreateCell(0).SetCellValue("無資料");

                return wb;
            }

            Dictionary<string, string> dictHeader = new Dictionary<string, string>{
                { "f01", "格式"},
                { "f02", "醫事服務機構代號"},
                { "f03", "申報資料年月"},
                { "f04", "藥品代碼"},
                { "f05", "藥商統一編號"},
                { "f06", "發票號碼（或收據號碼）"},
                { "f07", "發票日期"},
                { "f08", "發票購買藥品數量(A)"},
                { "f16", "贈品數量-附贈之藥品數量(B)"},
                { "f17", "贈品數量-藥品耗損數量(C)"},
                { "f18", "退貨數量(D)"},
                { "f19", "實際購買數量(E)"},
                { "f20", "發票金額(F,元)"},
                { "f21", "退貨金額(G,元)"},
                { "f22", "折讓-折讓單金額(H,元)"},
                { "f23", "折讓-指定捐贈(I,元)"},
                { "f24", "折讓-藥商提撥管理費(J,元)"},
                { "f25", "折讓-藥商提撥研究費(K,元)"},
                { "f26", "折讓-藥商提出國會議(L,元)"},
                { "f27", "折讓-其他附帶利益(M,元)"},
                { "f28", "購藥總金額(N,元)"},
                { "f29", "發票註記"}
            };
            string strVal = "";
            foreach (string yyymm in yyymms)
            {
                var sheet = (XSSFSheet)wb.CreateSheet(yyymm);

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

                DataTable temp = data.AsEnumerable().Where(x => x.Field<string>("F03") == yyymm)
                                    .OrderBy(x => x.Field<string>("F07"))
                                    .CopyToDataTable();

                for (int i = 0; i < temp.Rows.Count; i++)
                {
                    row = sheet.CreateRow(1 + i);
                    for (int j = 0; j < temp.Columns.Count; j++)
                    {
                        row.CreateCell(j).SetCellValue(temp.Rows[i].ItemArray[j].ToString());
                    }
                }
            }


            return wb;
        }

        #endregion

        #region Txt

        [HttpPost]
        public ApiResponse Txt(FormDataCollection form)
        {
            string start_ym = form.Get("P0");
            string end_ym = form.Get("P1");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0063Repository(DBWork);
                    IEnumerable<FA0063> list = repo.GetAll(start_ym, end_ym, false);

                    IEnumerable<string> ym_list = repo.GetTxtYmList(start_ym, end_ym);

                    List<COMBO_MODEL> pro_list = new List<COMBO_MODEL>();

                    foreach (string ym in ym_list)
                    {
                        List<string> string_list = new List<string>();

                        List<FA0063> temp_list = list.Where(x => x.F03 == ym).Select(x => x).ToList();



                        COMBO_MODEL pro = new COMBO_MODEL();
                        pro.VALUE = ym;
                        foreach (FA0063 item in temp_list)
                        {
                            string temp = string.Empty;
                            temp += (item.F01 ?? "").PadLeft(2, '0');   //  資料格式
                            temp += (item.F02 ?? "").PadLeft(10, '0');   // 醫事服務機構代號
                            temp += (item.F03 ?? "").PadLeft(5, '0');   // 申報資料年月
                            temp += (item.F04 ?? "").PadLeft(10, '0');   // 藥品代碼
                            temp += (item.F05 ?? "").PadLeft(8, '0');   // 藥商統一編號
                            temp += (item.F06 ?? "").PadLeft(10, '0');   // 發票號碼（或收據號碼）
                            temp += (item.F07 ?? "").PadLeft(7, '0');   // 發票日期
                            temp += (item.F08 ?? "").PadLeft(9, '0');   // 發票購買藥品數量(A)
                            temp += (item.F16 ?? "").PadLeft(9, '0');   // 贈品數量-附贈之藥品數量(B)
                            temp += (item.F17 ?? "").PadLeft(9, '0');   // 贈品數量-藥品耗損數量(C)
                            temp += (item.F18 ?? "").PadLeft(9, '0');   // 退貨數量(D)
                            temp += (item.F19 ?? "").PadLeft(9, '0');   // 實際購買數量(E)
                            temp += (item.F20 ?? "").PadLeft(9, '0');   // 發票金額(F,元)
                            temp += (item.F21 ?? "").PadLeft(9, '0');   // 退貨金額(G,元)
                            temp += (item.F22 ?? "").PadLeft(9, '0');   // 折讓金額-折讓單金額(H,元)
                            temp += (item.F23 ?? "").PadLeft(9, '0');   // 折讓金額指定捐贈I元
                            temp += (item.F24 ?? "").PadLeft(9, '0');   // 折讓金額藥商提撥管理費J元
                            temp += (item.F25 ?? "").PadLeft(9, '0');   // 折讓金額藥商提撥研究費K元
                            temp += (item.F26 ?? "").PadLeft(9, '0');   // 折讓金額藥商提撥補助醫師出國會議L元
                            temp += (item.F27 ?? "").PadLeft(9, '0');   // 折讓金額其他與本交易相關之附帶利益M元
                            temp += (item.F28 ?? "").PadLeft(9, '0');   // 購藥總金額(N,元)
                            temp += (item.F29 ?? "").PadLeft(1, '0');   // 發票註記

                            string_list.Add(temp);

                            if (string.IsNullOrEmpty(pro.TEXT) == false)
                            {
                                pro.TEXT += "\r\n";
                            }
                            pro.TEXT += temp;
                        }

                        pro_list.Add(pro);


                        // ExportTxt(string.Format("{0}_{1}.zip", "價量調查表", DateTime.Now.ToString("yyyyMMddHHmmss")), string_list);  
                    }
                    ExportTxtZip(pro_list);


                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

        //public static void ExportTxt(string fileName, List<string> list) {
        //    if (HttpContext.Current == null) return;

        //    HttpResponse res = HttpContext.Current.Response;
        //    MemoryStream ms = new MemoryStream();
        //    StreamWriter sw = new StreamWriter(ms, Encoding.GetEncoding("big5"));
        //    string line = "";
        //    int Index = 1;
        //    Encoding utf81 = Encoding.GetEncoding("utf-8");
        //    Encoding big51 = Encoding.GetEncoding("big5");

        //    int i = 0;
        //    foreach (string item in list) {
        //        string temp = string.Empty;
        //        if (i < list.Count) {
        //            temp = string.Format("{0}\r\n", item);
        //        }
        //        sw.WriteLine(item);
        //    }

        //    sw.Close();


        //    res.BufferOutput = true;
        //    //res.Buffer = false;

        //    res.Clear();
        //    res.ClearHeaders();
        //    res.ContentEncoding = big51;
        //    res.Charset = "big5";
        //    res.ContentType = "application/octet-stream;charset=big5";
        //    res.AddHeader("Content-Disposition",
        //                "attachment; filename=" + HttpUtility.HtmlEncode(HttpUtility.UrlEncode(fileNameFilter(fileName, ".txt"), System.Text.Encoding.UTF8)));
        //    res.BinaryWrite(ms.ToArray());
        //    res.Flush();
        //    res.End();

        //    ms.Close();
        //    ms.Dispose();
        //}

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

        public static void ExportTxtZip(List<COMBO_MODEL> list)
        {
            string fileName = string.Format("{0}_{1}.zip", "價量調查表", DateTime.Now.ToString("yyyyMMddHHmmss"));

            if (HttpContext.Current == null) return;

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (COMBO_MODEL item in list)
                    {
                        var temp = archive.CreateEntry(string.Format("{0}.txt", item.VALUE));
                        using (StreamWriter writer = new StreamWriter(temp.Open()))
                        {
                            writer.WriteLine(item.TEXT);
                        }
                    }
                }

                HttpResponse res = HttpContext.Current.Response;

                res.BufferOutput = false;
                res.Clear();
                res.ClearHeaders();
                res.HeaderEncoding = System.Text.Encoding.Default;
                res.ContentType = "application/octet-stream";
                res.AddHeader("Content-Disposition",
                            "attachment; filename=\"" + HttpUtility.HtmlEncode(HttpUtility.UrlEncode(fileNameFilter(fileName, ".zip"), System.Text.Encoding.UTF8)) + "\"");
                res.BinaryWrite(memoryStream.ToArray());
                res.Flush();
                res.End();

            }
        }

        #endregion


        #region combo
        [HttpGet]
        public ApiResponse GetYMCombo()
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0063Repository(DBWork);
                    session.Result.etts = repo.GetYMCombo();
                    return session.Result;
                }
                catch (Exception e)
                {
                    throw;
                }
            }
        }
        #endregion

        #region 單位設定
        [HttpPost]
        public ApiResponse GetBaseUnitCnvs(FormDataCollection form)
        {
            string ui_from = form.Get("ui_from");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;

                try
                {
                    var repo = new FA0063Repository(DBWork);
                    session.Result.etts = repo.GetBaseUnitCnvs(ui_from);

                    return session.Result;
                }
                catch (Exception e)
                {
                    throw;
                }
            }
        }
        [HttpPost]
        public ApiResponse CreateBaseunitcnv(BASEUNITCNV item)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new FA0063Repository(DBWork);
                    if (repo.CheckBaseunitcnvExists(item.UI_FROM, item.UI_TO))
                    {
                        session.Result.success = false;
                        session.Result.msg = "資料已存在，請重新確認";
                        return session.Result;
                    }

                    item.CREATE_USER = DBWork.UserInfo.UserId;
                    item.UPDATE_USER = DBWork.UserInfo.UserId;
                    item.UPDATE_IP = DBWork.ProcIP;

                    session.Result.afrs = repo.CreateBaseunitcnv(item);

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
        [HttpPost]
        public ApiResponse UpdateBaseunitcnv(BASEUNITCNV item)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new FA0063Repository(DBWork);

                    item.UPDATE_USER = DBWork.UserInfo.UserId;
                    item.UPDATE_IP = DBWork.ProcIP;

                    session.Result.afrs = repo.UpdateBaseunitcnv(item);

                    DBWork.Commit();
                    return session.Result;
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    throw;
                }
            }
        }
        [HttpPost]
        public ApiResponse DeleteBaseunitcnv(BASEUNITCNV item)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new FA0063Repository(DBWork);


                    session.Result.afrs = repo.DeleteBaseunitcnv(item);

                    DBWork.Commit();

                    return session.Result;
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    throw;
                }
            }
        }
        #endregion
    }
}