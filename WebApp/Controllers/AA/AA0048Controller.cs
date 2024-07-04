using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using System;
using System.Collections.Generic;
using NPOI.SS.UserModel;
using System.Web;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using System.Data;
using Newtonsoft.Json;
using System.Linq;

namespace WebApp.Controllers.AA
{
    public class AA0048Controller : SiteBase.BaseApiController
    {
        #region models- HeaderItem
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
        #endregion



        //Check excel
        [HttpPost]
        public ApiResponse CheckExcelMat() {
            using (WorkSession session = new WorkSession(this)) {

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

                for (int i = 0; i < cellCount; i++)
                {
                    if (headerRow.GetCell(i) == null || headerRow.GetCell(i).ToString() == string.Empty)
                    {
                        session.Result.msg = string.Format("欄位標題包含空白，請重新處理");
                        session.Result.success = false;
                        return session.Result;
                    }
                }

                List<HeaderItem> headerItems = new List<HeaderItem>() {
                       new HeaderItem("庫房代碼", "WH_NO"),
                        new HeaderItem("院內碼", "MMCODE"),
                        new HeaderItem("安全日", "SAFE_DAY"),
                        new HeaderItem("作業日", "OPER_DAY"),
                        new HeaderItem("運補日", "SHIP_DAY"),
                       new HeaderItem("基準量","HIGH_QTY"),
                        new HeaderItem("最低庫存量","LOW_QTY"),
                        new HeaderItem("最小撥補量", "MIN_ORDQTY"),
                        new HeaderItem("是否自動撥補(Y/N)", "IS_AUTO")
                    };

                headerItems = SetHeaderIndex(headerItems, headerRow);
                string msg = string.Empty;
                foreach (HeaderItem header in headerItems) {
                    if (header.Index < 0) {
                        if (msg != string.Empty) {
                            msg += "、";
                        }
                        msg += header.Name;
                    }
                }
                if (msg != string.Empty) {
                    session.Result.msg = string.Format("上傳格式錯誤，缺少欄位：{0}", msg);
                    session.Result.success = false;
                    return session.Result;
                }

                headerItems.Add(new HeaderItem("檢核結果", headerItems.Count, "STATUS_DISPLAY"));
                List<AA0048> listWithmsg = new List<AA0048>();
                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    IRow row = sheet.GetRow(i);
                    if (row is null) { continue; }

                    AA0048 aa0048 = new AA0048();
                    foreach (HeaderItem item in headerItems)
                    {
                        string value = row.GetCell(item.Index) == null ? string.Empty : row.GetCell(item.Index).ToString();
                        aa0048.GetType().GetProperty(item.FieldName).SetValue(aa0048, value, null);
                    }
                    listWithmsg.Add(aa0048);
                }
                return CheckExcel(listWithmsg, true);
            }
                
        }
        [HttpPost]
        public ApiResponse CheckExcelMed() {
            using (WorkSession session = new WorkSession(this))
            {

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


                for (int i = 0; i < cellCount; i++)
                {
                    if (headerRow.GetCell(i) == null || headerRow.GetCell(i).ToString() == string.Empty)
                    {
                        session.Result.msg = string.Format("欄位標題包含空白，請重新處理");
                        session.Result.success = false;
                        return session.Result;
                    }
                }

                List<HeaderItem> headerItems = new List<HeaderItem>() {
                       new HeaderItem("庫房代碼", "WH_NO"),
                        new HeaderItem("院內碼", "MMCODE"),
                        new HeaderItem("是否自動撥補(Y/N)", "IS_AUTO"),
                        new HeaderItem("醫令扣庫歸整(1/2/3)", "USEADJ_CLASS"),
                        new HeaderItem("是否拆單(Y/N)", "ISSPLIT"),
                    };

                headerItems = SetHeaderIndex(headerItems, headerRow);
                string msg = string.Empty;
                foreach (HeaderItem header in headerItems)
                {
                    if (header.Index < 0)
                    {
                        if (msg != string.Empty)
                        {
                            msg += "、";
                        }
                        msg += header.Name;
                    }
                }
                if (msg != string.Empty)
                {
                    session.Result.msg = string.Format("上傳格式錯誤，缺少欄位：{0}", msg);
                    session.Result.success = false;
                    return session.Result;
                }
                headerItems.Add(new HeaderItem("檢核結果", headerRow.LastCellNum+1, "STATUS_DISPLAY"));

                List<AA0048> listWithmsg = new List<AA0048>();
                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    IRow row = sheet.GetRow(i);
                    if (row is null) { continue; }

                    AA0048 aa0048 = new AA0048();
                    foreach (HeaderItem item in headerItems)
                    {
                        string value = row.GetCell(item.Index) == null ? string.Empty : row.GetCell(item.Index).ToString();
                        aa0048.GetType().GetProperty(item.FieldName).SetValue(aa0048, value, null);
                    }
                    listWithmsg.Add(aa0048);
                }
                return CheckExcel(listWithmsg, false);
            }
        }

        public ApiResponse CheckExcel(IEnumerable<AA0048> itemList, bool isMat)
        {
            bool pass = true; //若上傳筆數全數可以,則須更新,並寫出更新成功
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0048Repository repo = new AA0048Repository(DBWork);

                    #region 逐筆檢查格式
                    foreach (AA0048 item in itemList)
                    {
                        // 庫房代碼
                        if (item.WH_NO == string.Empty)
                        {
                            if (item.STATUS_DISPLAY != string.Empty)
                            {
                                item.STATUS_DISPLAY += "<br/>";
                            }
                            item.STATUS_DISPLAY += string.Format("未輸入庫房代碼");
                            pass = false;
                        }
                        else
                        if (repo.SelectMI_WHMAST(item.WH_NO) == "0")
                        {
                            if (item.STATUS_DISPLAY != string.Empty)
                            {
                                item.STATUS_DISPLAY += "<br/>";
                            }
                            pass = false;
                            item.STATUS_DISPLAY += "庫房代碼不存在基本檔";

                        }
                        else if (repo.SelectMI_WHMAST_check_kind(item.WH_NO, isMat) == "0")
                        {
                            if (item.STATUS_DISPLAY != string.Empty)
                            {
                                item.STATUS_DISPLAY += "<br/>";
                            }
                            pass = false;
                            item.STATUS_DISPLAY += string.Format("不是{0}庫房，不處理", isMat ? "衛材" : "藥品");
                        }
                        else if (repo.SelectMI_WHMAST_check_grade(item.WH_NO, isMat) == "0") {
                            if (item.STATUS_DISPLAY != string.Empty)
                            {
                                item.STATUS_DISPLAY += "<br/>";
                            }
                            pass = false;
                            item.STATUS_DISPLAY += string.Format("非可修改庫房，不處理");
                        }
                        // 院內碼
                        if (item.MMCODE == string.Empty)
                        {
                            if (item.STATUS_DISPLAY != string.Empty)
                            {
                                item.STATUS_DISPLAY += "<br/>";
                            }
                            item.STATUS_DISPLAY += string.Format("未輸入院內碼");
                            pass = false;
                        } else
                        if (repo.SelectMI_MAST(item.MMCODE) == "0")
                        {
                            if (item.STATUS_DISPLAY != string.Empty)
                            {
                                item.STATUS_DISPLAY += "<br/>";
                            }
                            pass = false;
                            item.STATUS_DISPLAY += "院內碼不存在基本檔";

                        }
                        // 衛材多確認 安全日、作業日、運補日、基準量、最低庫存量、最小撥補量
                        if (isMat)
                        {
                            double d;
                            if (item.SAFE_DAY.Trim() != string.Empty && (double.TryParse(item.SAFE_DAY, out d) == false || double.Parse(item.SAFE_DAY) < 0))
                            {
                                if (item.STATUS_DISPLAY != string.Empty)
                                {
                                    item.STATUS_DISPLAY += "<br/>";
                                }
                                pass = false;
                                item.STATUS_DISPLAY += "安全日不為數字或小於0";
                            }
                            if (item.OPER_DAY.Trim() != string.Empty && (double.TryParse(item.OPER_DAY, out d) == false || double.Parse(item.OPER_DAY) < 0))
                            {
                                if (item.STATUS_DISPLAY != string.Empty)
                                {
                                    item.STATUS_DISPLAY += "<br/>";
                                }
                                pass = false;
                                item.STATUS_DISPLAY += "作業日不為數字或小於0";
                            }
                            if (item.SHIP_DAY.Trim() != string.Empty && (double.TryParse(item.SHIP_DAY, out d) == false || double.Parse(item.SHIP_DAY) < 0))
                            {
                                if (item.STATUS_DISPLAY != string.Empty)
                                {
                                    item.STATUS_DISPLAY += "<br/>";
                                }
                                pass = false;
                                item.STATUS_DISPLAY += "運補日不為數字或小於0";
                            }
                            if (item.HIGH_QTY.ToString() != string.Empty && (double.TryParse(item.HIGH_QTY, out d) == false || double.Parse(item.HIGH_QTY) < 0))
                            {
                                if (item.STATUS_DISPLAY != string.Empty)
                                {
                                    item.STATUS_DISPLAY += "<br/>";
                                }
                                pass = false;
                                item.STATUS_DISPLAY += "基準量不為數字或小於0";
                            }
                            if (item.LOW_QTY.ToString() != string.Empty && (double.TryParse(item.LOW_QTY, out d) == false || double.Parse(item.LOW_QTY) < 0))
                            {
                                if (item.STATUS_DISPLAY != string.Empty)
                                {
                                    item.STATUS_DISPLAY += "<br/>";
                                }
                                pass = false;
                                item.STATUS_DISPLAY += "最低庫存量不為數字或小於0";
                            }
                            if (item.MIN_ORDQTY.Trim() != string.Empty && (double.TryParse(item.MIN_ORDQTY, out d) == false || double.Parse(item.MIN_ORDQTY) < 0))
                            {
                                if (item.STATUS_DISPLAY != string.Empty)
                                {
                                    item.STATUS_DISPLAY += "<br/>";
                                }
                                pass = false;
                                item.STATUS_DISPLAY += "最小撥補量不為數字或小於0";
                            }
                        }
                        if (item.IS_AUTO.Trim() != string.Empty && item.IS_AUTO.Trim() != "Y" && item.IS_AUTO != "N")
                        {
                            if (item.STATUS_DISPLAY != string.Empty)
                            {
                                item.STATUS_DISPLAY += "<br/>";
                            }
                            pass = false;
                            item.STATUS_DISPLAY += "是否自動撥補格式錯誤：僅能輸入Y或N";
                        }
                        // 藥品多確認醫令扣庫規整、是否拆單
                        if (isMat == false) {
                            if (item.USEADJ_CLASS != string.Empty && item.USEADJ_CLASS.Trim() != "1" && item.USEADJ_CLASS != "2" && item.USEADJ_CLASS != "3")
                            {
                                if (item.STATUS_DISPLAY != string.Empty)
                                {
                                    item.STATUS_DISPLAY += "<br/>";
                                }
                                pass = false;
                                item.STATUS_DISPLAY += "醫令扣庫歸整格式錯誤：僅能輸入1、2或3";
                            }

                            if (item.ISSPLIT.Trim() != string.Empty && item.ISSPLIT.Trim() != "Y" && item.ISSPLIT != "N")
                            {
                                if (item.STATUS_DISPLAY != string.Empty)
                                {
                                    item.STATUS_DISPLAY += "<br/>";
                                }
                                pass = false;
                                item.STATUS_DISPLAY += "是否拆單格式錯誤：僅能輸入Y或N";
                            }
                        }
                    }
                    #endregion

                    #region 有過 儲存
                    if (pass) {
                        foreach (AA0048 item in itemList) {
                            
                            if (repo.SelectMI_WINVCTL(item.WH_NO, item.MMCODE) == "0")  //不存在，新增
                            {
                                //在新增
                                session.Result.afrs = repo.InsertMI_WINVCTL(isMat, item.WH_NO, item.MMCODE
                                    , item.SAFE_DAY, item.OPER_DAY, item.SHIP_DAY, item.HIGH_QTY, item.LOW_QTY, item.MIN_ORDQTY, item.IS_AUTO, "0",
                                    DBWork.UserInfo.UserId, DBWork.ProcIP,
                                    item.USEADJ_CLASS, item.ISSPLIT);
                                item.STATUS_DISPLAY = "已新增資料";
                            }
                            else
                            {
                                //更新
                                session.Result.afrs = repo.UpdateMI_WINVCTL(isMat, item.WH_NO, item.MMCODE
                                    , item.SAFE_DAY, item.OPER_DAY, item.SHIP_DAY, item.HIGH_QTY, item.LOW_QTY, item.MIN_ORDQTY, item.IS_AUTO, item.USEADJ_CLASS, item.ISSPLIT, 
                                    DBWork.UserInfo.UserId, DBWork.ProcIP);
                                item.STATUS_DISPLAY = "已更新資料";
                            }
                        }
                    }
                    #endregion

                    #region 輸出到前端
                    foreach (AA0048 item in itemList) {
                        if (item.MMCODE != string.Empty && repo.SelectDANGERDRUGFLAG_N(item.MMCODE) != "-1") {
                            item.DANGERDRUGFLAG_N = repo.SelectDANGERDRUGFLAG_N(item.MMCODE);  //高警訊
                        }
                        if (item.MMCODE != string.Empty && repo.SelectE_RESTRICTCODE_N(item.MMCODE) != "-1")
                        {
                            item.E_RESTRICTCODE_N = repo.SelectE_RESTRICTCODE_N(item.MMCODE);    // 管制藥
                        }
                        if (item.MMCODE != string.Empty && repo.SelectMMNAME(item.MMCODE) != "-1")
                        {
                            item.MMNAME = repo.SelectMMNAME(item.MMCODE);
                        }
                        if (item.WH_NO != string.Empty && item.MMCODE != string.Empty && repo.SelectAVG_USEQTYL(item.WH_NO, item.MMCODE) != "")
                        {
                            item.DAVG_USEQTY = repo.SelectAVG_USEQTYL(item.WH_NO, item.MMCODE);
                        }
                        if (item.WH_NO != string.Empty && item.MMCODE != string.Empty && repo.SelectSAFE_QTY(item.WH_NO, item.MMCODE) != "")
                        {
                            item.SAFE_QTY = repo.SelectSAFE_QTY(item.WH_NO, item.MMCODE); //安全量
                        }
                        if (item.WH_NO != string.Empty && item.MMCODE != string.Empty && repo.SelectOPER_QTY(item.WH_NO, item.MMCODE) != "")
                        {
                            item.OPER_QTY = repo.SelectOPER_QTY(item.WH_NO, item.MMCODE); //作業量
                        }
                        if (item.WH_NO != string.Empty && item.MMCODE != string.Empty) {
                            item.SUPPLY_WHNO = repo.SelectSUPPLY_WHNO(item.WH_NO, item.MMCODE);
                            item.CTDMDCCODE = repo.SelectCTDMDCCODE(item.WH_NO, item.MMCODE);
                            item.CTDMDCCODE_N = repo.SelectCTDMDCCODE_N(item.CTDMDCCODE);

                            item.SAFE_DAY = repo.SelectSAFE_DAY(item.WH_NO, item.MMCODE);
                            item.OPER_DAY = repo.SelectOPER_DAY(item.WH_NO, item.MMCODE);
                            item.SHIP_DAY = repo.SelectSHIP_DAY(item.WH_NO, item.MMCODE);
                            item.MIN_ORDQTY = repo.SelectMIN_ORDQTY(item.WH_NO, item.MMCODE);
                            item.IS_AUTO = repo.SelectIS_AUTO(item.WH_NO, item.MMCODE);
                            item.HIGH_QTY = repo.SelectHIGH_QTY(item.WH_NO, item.MMCODE);
                            item.LOW_QTY = repo.SelectLOW_QTY(item.WH_NO, item.MMCODE);
                            item.NOWCONSUMEFLAG = repo.SelectNOWCONSUMEFLAG(item.WH_NO, item.MMCODE);
                            item.USEADJ_CLASS_NAME = repo.SelectUSEADJ_CLASS(item.WH_NO, item.MMCODE);
                            item.ISSPLIT = repo.SelectISSPLIT(item.WH_NO, item.MMCODE);
                        }
                        
                        
                    }
                    #endregion

                    session.Result.etts = itemList;

                    DBWork.Commit();
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
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

        //確認更新
        [HttpPost]
        public ApiResponse Insert(FormDataCollection formData)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();

                IEnumerable<AA0048> aa0048 = JsonConvert.DeserializeObject<IEnumerable<AA0048>>(formData["data"]);

                List<string> checkDuplicate = new List<string>();

                List<AA0048> aa0048_list = new List<AA0048>();
                try
                {
                    var repo = new AA0048Repository(DBWork);
                    bool isDuplicate = false;

                    foreach (AA0048 data in aa0048)
                    {

                        if (checkDuplicate.Contains(data.MMCODE)) //檢查list有沒有已經insert過的MMCODE
                        {
                            isDuplicate = true;
                            session.Result.msg = isDuplicate.ToString();
                            break;
                        }
                        else
                        {
                            data.UPDATE_USER = User.Identity.Name;
                            data.UPDATE_IP = DBWork.ProcIP;
                            checkDuplicate.Add(data.MMCODE); //每次insert都把MMCODE寫入這個list

                            try
                            {

                                if (!repo.CheckExists(data.WH_NO, data.MMCODE)) // 新增前檢查主鍵是否已存在
                                {
                                    data.CREATE_USER = User.Identity.Name;
                                    data.UPDATE_USER = User.Identity.Name;
                                    data.UPDATE_IP = DBWork.ProcIP;
                                    repo.Create(data);         
                                }
                                else
                                {
                                    data.CREATE_USER = User.Identity.Name;
                                    data.UPDATE_USER = User.Identity.Name;
                                    data.UPDATE_IP = DBWork.ProcIP;
                                    repo.Update(data);
                                }
                            }
                            catch
                            {
                                throw;
                            }
                            aa0048_list.Add(data);
                        }
                    }

                    session.Result.etts = aa0048_list;

                    if (isDuplicate == false)
                    {
                        DBWork.Commit();
                    }
                    else
                    {
                        DBWork.Rollback();
                    }
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }
        // 新增
        [HttpPost]
        public ApiResponse Create(AA0048 mi_winvctl)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0048Repository(DBWork);
                    if (!repo.CheckExists(mi_winvctl.WH_NO, mi_winvctl.MMCODE)) // 新增前檢查主鍵是否已存在
                    {
                        mi_winvctl.CREATE_USER = User.Identity.Name;
                        mi_winvctl.UPDATE_USER = User.Identity.Name;
                        mi_winvctl.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.Create(mi_winvctl);         //問 session.Result.afrs
                        session.Result.etts = repo.Get(mi_winvctl.WH_NO, mi_winvctl.MMCODE);      //搜尋一筆，且在ext上loadStore
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>庫房代碼與院內碼</span>重複，請重新輸入。";
                    }

                    DBWork.Commit();
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }

        // 修改
        [HttpPost]
        public ApiResponse Update(AA0048 mi_winvctl)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0048Repository(DBWork);
                    mi_winvctl.UPDATE_USER = User.Identity.Name;
                    mi_winvctl.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.Update(mi_winvctl);
                    session.Result.etts = repo.Get(mi_winvctl.WH_NO, mi_winvctl.MMCODE); //可讓前端收到資料後關閉mask

                    DBWork.Commit();
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse Delete(MI_WINVCTL mi_winvctl)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0048Repository(DBWork);
                    if (repo.CheckExists(mi_winvctl.WH_NO, mi_winvctl.MMCODE))
                    {
                        session.Result.afrs = repo.Delete(mi_winvctl.WH_NO, mi_winvctl.MMCODE);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>庫房代碼與院內碼</span>不存在。";
                    }

                    DBWork.Commit();
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            //var p4 = form.Get("p4");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            string[] arr_p3 = { };
            if (!string.IsNullOrEmpty(p3))
            {
                arr_p3 = p3.Trim().Split(','); //用,分割
            }
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0048Repository(DBWork);

                    session.Result.etts = repo.GetAll(p0, p1, p2, arr_p3, page, limit, sorters);
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
        public ApiResponse Excel(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            string[] arr_p3 = { };
            if (!string.IsNullOrEmpty(p3))
            {
                arr_p3 = p3.Trim().Split(','); //用,分割
            }
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0048Repository repo = new AA0048Repository(DBWork);
                    JCLib.Excel.Export(string.Format("AA0048_{0}.xls", DateTime.Now.ToString("yyyyMMddHHmm")), repo.GetExcel(p0, p1, p2, arr_p3));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetWH_NOComboOne(FormDataCollection form) //AA0048
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0076Repository repo = new AA0076Repository(DBWork); //AA0076與AA0048 WH_NO SQL 下法一樣
                    if (repo.GetUserKind(User.Identity.Name).Contains("S"))
                    {
                        session.Result.etts = repo.GetWhnoCombo_S(User.Identity.Name);
                    }
                    else if (repo.GetUserKind(User.Identity.Name).Contains("1"))
                    {
                        session.Result.etts = repo.GetWhnoCombo_1OrElse(User.Identity.Name);
                    }
                    else
                    {
                        session.Result.etts = repo.GetWhnoCombo_1OrElse(User.Identity.Name);
                    }
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetWH_NOComboNotOne(FormDataCollection form) ////AB0036
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0048Repository repo = new AA0048Repository(DBWork);
                    session.Result.etts = repo.GetWH_NOComboNotOne(User.Identity.Name)
                        .Select(w => new { WH_NO = w.WH_NO, WH_NAME = w.WH_NAME });
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMMCODECombo(FormDataCollection form) //AA0076與AA0048 MMCODE SQL 下法一樣
        {
            var p0 = form.Get("p0"); //動態mmcode
            var p1 = form.Get("p1"); //wh_no
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0076Repository repo = new AA0076Repository(DBWork);
                    if (repo.GetUserKind(User.Identity.Name).Contains("S") ||
                        repo.GetUserKind(User.Identity.Name).Contains("1"))
                    {
                        session.Result.etts = repo.GetMMCODECombo(p0, p1, page, limit, "");
                    }
                    else
                    {
                        session.Result.etts = repo.GetMMCODECombo_else(p0, p1, User.Identity.Name, page, limit, "");
                    }

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse EditGetMMCODECombo(FormDataCollection form)
        {
            var p0 = form.Get("wh_no");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0048Repository repo = new AA0048Repository(DBWork);
                    session.Result.msg = repo.CheckWhKIND(p0);
                    if (session.Result.msg == "0")      //藥品庫
                    {
                        session.Result.etts = repo.GetMMCODEComboWhMM(p0);
                    }
                    else if (session.Result.msg == "1") //衛材庫
                    {
                        session.Result.msg = repo.CheckMiWhid(User.Identity.Name);
                        if (session.Result.msg != "" || session.Result.msg != null)
                        {
                            session.Result.etts = repo.GetMMCODEComboMastGeneral(session.Result.msg); //這邊是 TASK_ID: session.Result.msg
                        }
                        else
                        {
                            session.Result.etts = repo.GetMMCODEComboMast();

                        }
                    }
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetSUPPLY_WHNOCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0048Repository repo = new AA0048Repository(DBWork);
                    session.Result.etts = repo.GetSUPPLY_WHNOCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMatClassCombo(FormDataCollection form)
        {
            var wh_no = form.Get("wh_no"); //wh_no


            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0048Repository repo = new AA0048Repository(DBWork);
                    AA0076Repository repo2 = new AA0076Repository(DBWork);

                    if (repo2.GetUserKind(User.Identity.Name).Contains("S"))
                    {
                        string rtnWhKind = repo.GetWH_KIND(wh_no);
                        if (rtnWhKind == "0")
                        {
                            // 選擇的WH_NO只有WH_KIND=0
                            session.Result.etts = repo.GetMatClassCombo_1();
                        }
                        else if (rtnWhKind.Contains("0") && rtnWhKind.Contains("1"))
                        {
                            // 選擇的WH_NO的WH_KIND有0且有1
                            session.Result.etts = repo.GetMatClassCombo_A();
                        }
                        else if (rtnWhKind == "1")
                        {
                            // 選擇的WH_NO的WH_KIND只有1
                            session.Result.etts = repo.GetMatClassCombo_else();
                        }
                    }
                    else if (repo2.GetUserKind(User.Identity.Name).Contains("1"))
                    {
                        session.Result.etts = repo.GetMatClassCombo_1();
                    }
                    else
                    {
                        session.Result.etts = repo.GetMatClassCombo_else();
                    }
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }


        [HttpPost]
        public ApiResponse GetCtdmdCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0048Repository repo = new AA0048Repository(DBWork);
                    session.Result.etts = repo.GetCtdmdCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetUseadjClassCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0048Repository repo = new AA0048Repository(DBWork);
                    session.Result.etts = repo.GetUseadjClassCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse CheckWhid(FormDataCollection form) {
            using (WorkSession session = new WorkSession(this)) {
                if (form != null)
                {
                    var wh_no = form.Get("wh_no");

                    UnitOfWork DBWork = session.UnitOfWork;
                    try
                    {
                        AA0048Repository repo = new AA0048Repository(DBWork);
                        if (repo.CheckInWhid(DBWork.UserInfo.UserId, wh_no) == false)
                        {
                            session.Result.success = false;
                            session.Result.msg = "無選取庫房存取權限，請重新輸入";
                            return session.Result;
                        }

                        session.Result.success = true;
                        session.Result.msg = string.Empty;
                    }
                    catch
                    {
                        throw;
                    }
                    return session.Result;
                }
                else
                {
                    session.Result.success = true;
                    session.Result.msg = string.Empty;
                    return session.Result;
                }
            }
        }

    }
}