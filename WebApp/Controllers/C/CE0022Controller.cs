using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.C;
using WebApp.Models.C;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Web;
using Newtonsoft.Json.Converters;
using WebApp.Models;
using System.Data;
using NPOI.SS.UserModel;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using System.Linq;

namespace ePMS.Controllers.C
{
    public class CE0022Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var wh_no = form.Get("wh_no");
            var d0 = form.Get("d0");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0022Repository(DBWork);
                    var repoCE0002 = new CE0002Repository(DBWork);

                    IEnumerable<CE0022> items = repo.GetAll(wh_no, d0, User.Identity.Name, page, limit, sorters);

                    foreach (CE0022 item in items)
                    {
                        //item.UPDN_STATUS = repo.GetUpdnStatus(item.CHK_NO);

                        item.CHK_TYPE = repoCE0002.GetChkWhkindName(item.CHK_WH_KIND_CODE, item.CHK_TYPE_CODE);
                    }

                    //session.Result.etts = repo.GetAll(wh_no, d0,User.Identity.Name, page, limit, sorters);
                    session.Result.etts = items;
                    //  ,
                    // (select UPDN_STATUS from CHK_GRADE2_UPDN where CHK_NO = a.chk_no) as updn_status
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }



        [HttpPost]
        public ApiResponse AllINI(FormDataCollection form)
        {
            var chk_no = form.Get("chk_no");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");


            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0022Repository(DBWork);
                    IEnumerable<CHK_CE0022VM> items = repo.GetAllINI(chk_no, User.Identity.Name, page, limit, sorters);

                    session.Result.etts = items;
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
            var chk_no = form.Get("chk_no");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    CE0022Repository repo = new CE0022Repository(DBWork);

                    session.Result.afrs = repo.UpdateGrade2UpDn(chk_no, DBWork.UserInfo.UserId, "2", DBWork.ProcIP);

                    DataTable dtItems = new DataTable();
                    // 2022-03-07: 項次改在sql中直接抓CHK_G2_WHINV的SEQ
                    //dtItems.Columns.Add("項次", typeof(int));
                    //dtItems.Columns["項次"].AutoIncrement = true;
                    //dtItems.Columns["項次"].AutoIncrementSeed = 1;
                    //dtItems.Columns["項次"].AutoIncrementStep = 1;

                    DataTable result = null;

                    result = repo.GetExcel(chk_no, User.Identity.Name);


                    dtItems.Merge(result);

                    JCLib.Excel.Export(form.Get("FN"), dtItems);

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
        public ApiResponse UPDN_STATUSGet(FormDataCollection form)
        {
            var chk_no = form.Get("chk_no");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0022Repository(DBWork);
                    session.Result.etts = repo.UPDN_STATUSGet(chk_no);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        #region upload
        [HttpPost]
        public ApiResponse Upload()
        {
            using (WorkSession session = new WorkSession(this))
            {

                List<CHK_DETAIL_TEMP> list = new List<CHK_DETAIL_TEMP>();
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    CE0022Repository repo = new CE0022Repository(DBWork);

                    #region data upload
                    List<HeaderItem> headerItems = new List<HeaderItem>() {
                       new HeaderItem("盤點單號", "CHK_NO"),
                        new HeaderItem("院內碼", "MMCODE"),
                        new HeaderItem("中文品名", "MMNAME_C"),
                        new HeaderItem("英文品名", "MMNAME_E"),
                        new HeaderItem("計量單位", "BASE_UNIT"),
                        new HeaderItem("藥槽號", "STORE_LOC"),
                       new HeaderItem("盤點數量","CHK_QTY"),
                        new HeaderItem("盤點人員","CHK_UID"),
                        new HeaderItem("盤點時間","CHK_TIME"),
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

                        CHK_DETAIL_TEMP temp = new CHK_DETAIL_TEMP();
                        foreach (HeaderItem item in headerItems)
                        {
                            string value = row.GetCell(item.Index) == null ? string.Empty : row.GetCell(item.Index).ToString();
                            temp.GetType().GetProperty(item.FieldName).SetValue(temp, value, null);
                        }
                        temp.Seq = i - 1;
                        list.Add(temp);
                    }
                    #endregion

                    string chk_no = list.FirstOrDefault().CHK_NO;

                    CHK_MAST mast = repo.GetMast(chk_no);

                    foreach (CHK_DETAIL_TEMP item in list)
                    {
                        item.WH_NO = mast.CHK_WH_NO;
                        item.CHK_UID = DBWork.UserInfo.UserId;
                        item.saveStatus = "1";  // 0: skip 1:update

                        // 確認院內碼是否存在於此次盤點清單
                        if (!repo.CheckExistsS(item.WH_NO, item.MMCODE))
                        {
                            item.saveStatus = "0";
                        }

                        if (item.CHK_QTY.Trim() == string.Empty)
                        {
                            item.saveStatus = "0";
                        }
                        // 確認是否有數值欄位放文字或負值，有的話改0
                        float b = 0;
                        if (float.TryParse(item.CHK_QTY, out b) == false)
                        {
                            item.CHK_QTY = "0";
                        }
                        else if (float.Parse(item.CHK_QTY) < 0)
                        {
                            item.CHK_QTY = "0";
                        }

                        // 盤點量相同不更新
                        if (repo.CheckDetailSameValue(item.CHK_NO, item.MMCODE, item.CHK_QTY, item.CHK_UID) && (item.saveStatus != "0"))
                        {
                            item.saveStatus = "0";
                        }

                        // 需跳過者不處理
                        if (item.saveStatus == "0")
                        {
                            continue;
                        }

                        item.CHK_UID = DBWork.UserInfo.UserId;
                        item.UPDATE_USER = DBWork.UserInfo.UserId;
                        item.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs += repo.UpdateCHK_G2_DETAIL(item);

                    }
                    session.Result.afrs = repo.UpdateGrade2UpDn(mast.CHK_NO, DBWork.UserInfo.UserId, "3", DBWork.ProcIP);

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

        //修改
        [HttpPost]
        public ApiResponse Save(FormDataCollection form) //CE0022
        {
            var CHK_NO = form.Get("CHK_NO");
            var ITEM_STRING = form.Get("ITEM_STRING");

            IEnumerable<CHK_CE0022VM> cE0022 = JsonConvert.DeserializeObject<IEnumerable<CHK_CE0022VM>>(ITEM_STRING);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0022Repository(DBWork);
                    foreach (var onecE0022 in cE0022)
                    {
                        session.Result.afrs = repo.UpdateCHK_G2_DETAIL_Save(onecE0022.CHK_QTY, CHK_NO , onecE0022.MMCODE, User.Identity.Name, DBWork.ProcIP);
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
        public ApiResponse FinalPro(FormDataCollection form)
        {
            var chk_no = form.Get("CHK_NO");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0022Repository(DBWork);
                    var count = repo.SelectCountFinalPro(chk_no, User.Identity.Name);
                    if (count != "0") //尚有負責的品項未盤點!
                    {
                        session.Result.etts = repo.SelectFinalPro(chk_no, User.Identity.Name);
                        session.Result.msg = "尚有負責的品項未盤點!";
                    }
                    else if (count == "0") //做update，  是否要提示自己完成盤點或是 CHK_TOTAL = CHK_NUM 提示訊息
                    {

                        repo.UpdateChk_g2_Status(chk_no, DBWork.UserInfo.UserId, DBWork.ProcIP);
                        repo.UpdateGrade2UpDn(chk_no, DBWork.UserInfo.UserId, "4", DBWork.ProcIP);
                        repo.UpdateChk_mast(chk_no, User.Identity.Name, DBWork.ProcIP);

                        session.Result.afrs = repo.UpdateChk_detail(chk_no, User.Identity.Name, DBWork.ProcIP); //Update STATUS_INI

                        string CHKNum = repo.SelectChk_mast(chk_no);  //查詢 格式: CHK_TOTAL,CHK_NUM

                        if (CHKNum.Substring(0, CHKNum.IndexOf(",")).Equals(CHKNum.Substring(CHKNum.IndexOf(",") + 1)))
                        {
                            repo.UpdateChk_mast_STATUS(chk_no, User.Identity.Name, DBWork.ProcIP);
                        }

                        session.Result.msg = "您已經完成此單號的盤點!";


                    }
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