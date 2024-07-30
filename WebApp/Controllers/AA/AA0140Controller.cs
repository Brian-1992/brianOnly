using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using System.Data;
using NPOI.SS.UserModel;
using System.IO;
using System.Web;
using NPOI.XSSF.UserModel;
using WebApp.Repository.AA;
using WebApp.Models.AA;
using System;

namespace WebApp.Controllers.AA
{
    public class AA0140Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            string mmcode = form.Get("mmcode");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0140Repository(DBWork);
                    session.Result.etts = repo.All(mmcode);
                }
                catch
                {
                    throw;
                }

                return session.Result;
            }
        }

        // 新增
        [HttpPost]
        public ApiResponse Create(AA0140 def)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork; 
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0140Repository(DBWork);
                    string msg = string.Empty;

                    if (repo.CheckExists_Mimast(def.MMCODE))
                    {
                        msg += "<span style='color:red'>● 院內碼必須存在於藥品基本檔</span>";
                    }

                    if (DateTime.Parse(def.SELF_CONT_BDATE) >= DateTime.Parse(def.SELF_CONT_EDATE)) 
                    {
                        msg += "<br/><span style='color:red'>● 藥品契約生效迄日 應>= 藥品契約生效起日</span><br/>";
                    }

                    if (def.SELF_PUR_UPPER_LIMIT.Trim() == string.Empty)
                    {
                        msg += "<br/><span style='color:red'>● 採購上限金額為空值</span><br/>";
                    }

                    // 確認是否有數值欄位放文字或負值，有的話改0
                    float b = 0;
                    if (float.TryParse(def.SELF_PUR_UPPER_LIMIT, out b) == false)
                    {
                        msg += "<br/><span style='color:red'>● 採購上限金額無法轉換為數值</span><br/>";
                    }
                    else if (float.Parse(def.SELF_PUR_UPPER_LIMIT) < 0)
                    {
                        msg += "<br/><span style='color:red'>● 採購上限金額小於零</span><br/>";
                    }

                    // 若msg不為空，表示有錯誤
                    if (msg != string.Empty)
                    {
                        session.Result.success = false;
                        session.Result.msg = msg;
                        return session.Result;
                    }

                    if (repo.ChkExists_MED_SELFPUR_DEF(def.MMCODE)) //院內碼不存在MED_SELFPUR_DEF
                    {
                        def.CreateUser = User.Identity.Name;
                        def.UpdateUser = User.Identity.Name;
                        def.UpdateIp = DBWork.ProcIP;
                        session.Result.afrs = repo.Create(def);
                    }
                    else //院內碼存在MED_SELFPUR_DEF
                    {
                        var MaxSELF_CONT_EDATE = repo.ChkMaxVids_SELF_CONT_EDATE(def.MMCODE, "");

                        if ((MaxSELF_CONT_EDATE != "") && (MaxSELF_CONT_EDATE != null))
                        {
                            if (DateTime.Parse(def.SELF_CONT_BDATE) > DateTime.Parse(MaxSELF_CONT_EDATE))
                            {
                                def.CreateUser = User.Identity.Name;
                                def.UpdateUser = User.Identity.Name;
                                def.UpdateIp = DBWork.ProcIP;
                                session.Result.afrs = repo.Create(def);
                            }
                            else
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                //session.Result.msg = "<span style='color:red'>藥品契約生效起日 應大於 相同院內碼藥品契約生效迄日最大值</span>，請重新輸入。";
                                session.Result.msg = "<span style='color:red'>藥品契約生效起日</span>" + def.SELF_CONT_BDATE +
                                                     "<span style='color:red'>應大於</span><br/>" +
                                                     "<span style='color:red'>相同院內碼藥品契約生效迄日最大值</span>" + MaxSELF_CONT_EDATE + "<br/>" +
                                                     "請重新輸入。";
                                return session.Result;
                            }
                        }
                    }

                    session.Result.etts = repo.Get(def.MMCODE); //重新抓取資料

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
        public ApiResponse Update(AA0140 def)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0140Repository(DBWork);
                    string msg = string.Empty;

                    if (def.SELF_PUR_UPPER_LIMIT.Trim() == string.Empty)
                    {
                        msg += "<br/><span style='color:red'>● 採購上限金額為空值</span><br/>";
                    }

                    // 確認是否有數值欄位放文字或負值，有的話改0
                    float b = 0;
                    if (float.TryParse(def.SELF_PUR_UPPER_LIMIT, out b) == false)
                    {
                        msg += "<br/><span style='color:red'>● 採購上限金額無法轉換為數值</span><br/>";
                    }
                    else if (float.Parse(def.SELF_PUR_UPPER_LIMIT) < 0)
                    {
                        msg += "<br/><span style='color:red'>● 採購上限金額小於零</span><br/>";
                    }

                    if (DateTime.Parse(def.SELF_CONT_BDATE) > DateTime.Parse(def.SELF_CONT_EDATE))
                    {
                        msg += "<br/><span style='color:red'>● 藥品契約生效迄日 應> 藥品契約生效起日</span><br/>";
                    }

                    // 若msg不為空，表示有錯誤
                    if (msg != string.Empty)
                    {
                        session.Result.success = false;
                        session.Result.msg = msg;
                        return session.Result;
                    }

                    var Count_MED_SELFPUR_DEF = repo.Count_MED_SELFPUR_DEF(def.MMCODE);
                    if (Count_MED_SELFPUR_DEF > 1)
                    {
                        //取最大契約生效迄日
                        var MaxSELF_CONT_EDATE = repo.ChkMaxVids_SELF_CONT_EDATE(def.MMCODE, "");

                        if (DateTime.Parse(def.SELF_CONT_EDATE) != DateTime.Parse(def.SELF_CONT_EDATE_virtual)) //現有迄日<>原有迄日,表示迄日有異動                         {
                        {
                            if (DateTime.Parse(def.SELF_CONT_EDATE) > DateTime.Parse(def.SELF_CONT_EDATE_virtual)) //修改後迄日>修改前迄日
                            {
                                if (DateTime.Parse(def.SELF_CONT_EDATE) > DateTime.Parse(MaxSELF_CONT_EDATE)) //修改後迄日>取得最大契約生效迄日
                                {
                                    MaxSELF_CONT_EDATE = def.SELF_CONT_EDATE;
                                }
                            }
                            else
                            {
                                if (DateTime.Parse(def.SELF_CONT_EDATE_virtual) > DateTime.Parse(MaxSELF_CONT_EDATE)) //修改前迄日>取得最大契約生效迄日
                                {
                                    MaxSELF_CONT_EDATE = def.SELF_CONT_EDATE_virtual;
                                }
                            }
                        }

                        if (DateTime.Parse(def.SELF_CONT_BDATE) >= DateTime.Parse(MaxSELF_CONT_EDATE)) //藥品契約生效起日 > 相同院內碼藥品契約生效迄日最大值
                        {
                            def.UpdateUser = User.Identity.Name;
                            def.UpdateIp = DBWork.ProcIP;
                            session.Result.afrs = repo.Update(def);
                        }
                        else
                        {   //藥品契約生效起日 < 相同院內碼藥品契約生效迄日最大值
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = "<span style='color:red'>藥品契約生效起日</span>" + def.SELF_CONT_BDATE +
                                                 "<span style='color:red'>應大於</span><br/>" +
                                                 "<span style='color:red'>相同院內碼藥品契約生效迄日最大值</span>" + MaxSELF_CONT_EDATE + "<br/>" +
                                                 "請重新輸入。";
                            return session.Result;
                        }

                        //if (DateTime.Parse(MaxSELF_CONT_EDATE) == DateTime.Parse(def.SELF_CONT_EDATE_virtual)) //取最大契約生效迄日=原有迄日==>即最大值那筆
                        //{
                        //    def.UpdateUser = User.Identity.Name;
                        //    def.UpdateIp = DBWork.ProcIP;
                        //    session.Result.afrs = repo.Update(def);
                        //}
                        //else
                        //{
                        //    if (DateTime.Parse(def.SELF_CONT_EDATE) != DateTime.Parse(def.SELF_CONT_EDATE_virtual)) //現有迄日<>原有迄日,表示迄日有異動                         {
                        //    {
                        //        if (DateTime.Parse(def.SELF_CONT_EDATE) > DateTime.Parse(def.SELF_CONT_EDATE_virtual)) //修改後迄日>修改前迄日
                        //        {
                        //            if (DateTime.Parse(def.SELF_CONT_EDATE) > DateTime.Parse(MaxSELF_CONT_EDATE)) //修改後迄日>取得最大契約生效迄日
                        //            {
                        //                MaxSELF_CONT_EDATE = def.SELF_CONT_EDATE;
                        //            }
                        //        }
                        //        else
                        //        {
                        //            if (DateTime.Parse(def.SELF_CONT_EDATE_virtual) > DateTime.Parse(MaxSELF_CONT_EDATE)) //修改前迄日>取得最大契約生效迄日
                        //            {
                        //                MaxSELF_CONT_EDATE = def.SELF_CONT_EDATE_virtual;
                        //            }
                        //        }
                        //    }

                        //    if (DateTime.Parse(def.SELF_CONT_BDATE) >= DateTime.Parse(MaxSELF_CONT_EDATE)) //藥品契約生效起日 > 相同院內碼藥品契約生效迄日最大值
                        //    {
                        //        def.UpdateUser = User.Identity.Name;
                        //        def.UpdateIp = DBWork.ProcIP;
                        //        session.Result.afrs = repo.Update(def);
                        //    }
                        //    else
                        //    {   //藥品契約生效起日 < 相同院內碼藥品契約生效迄日最大值
                        //        session.Result.afrs = 0;
                        //        session.Result.success = false;
                        //        session.Result.msg = "<span style='color:red'>藥品契約生效起日</span>" + def.SELF_CONT_BDATE +
                        //                             "<span style='color:red'>應大於</span><br/>" +
                        //                             "<span style='color:red'>相同院內碼藥品契約生效迄日最大值</span>" + MaxSELF_CONT_EDATE + "<br/>" +
                        //                             "請重新輸入。";
                        //        return session.Result;
                        //    }
                        //}
                    }
                    else
                    {
                        def.UpdateUser = User.Identity.Name;
                        def.UpdateIp = DBWork.ProcIP;
                        session.Result.afrs = repo.Update(def);
                    }
                    session.Result.etts = repo.Get(def.MMCODE); //重新抓取資料  
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

        // 刪除
        [HttpPost]
        public ApiResponse Delete(FormDataCollection form)
        {
            var mmcode = form.Get("mmcode");
            var self_cont_bdate = form.Get("self_cont_bdate");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0140Repository(DBWork);
                    session.Result.afrs = repo.Delete(mmcode, self_cont_bdate);
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
                    AA0140Repository repo = new AA0140Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeCombo(p0, page, limit, "");
                }
                catch (Exception e)
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
            var mmocde = form.Get("mmcode");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    DataTable result = new DataTable();
                    DataTable dtItems = new DataTable();

                    var repo = new AA0140Repository(DBWork);
                    result = repo.GetExcel(mmocde);
                    dtItems.Merge(result);

                    JCLib.Excel.Export(form.Get("FN"), dtItems);
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
        public ApiResponse Upload() //[匯入]呼叫
        {
            using (WorkSession session = new WorkSession(this))
            {

                List<AA0140> list = new List<AA0140>();
                UnitOfWork DBWork = session.UnitOfWork;

                try
                {
                    AA0140Repository repo = new AA0140Repository(DBWork);

                    #region data upload
                    List<HeaderItem> headerItems = new List<HeaderItem>() {
                        new HeaderItem("院內碼", "MMCODE"),
                        new HeaderItem("藥品契約生效起日", "SELF_CONT_BDATE"),
                        new HeaderItem("藥品契約生效迄日", "SELF_CONT_EDATE"),
                        new HeaderItem("合約案號", "SELF_CONTRACT_NO"),
                        new HeaderItem("採購上限金額", "SELF_PUR_UPPER_LIMIT"),
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

                        AA0140 temp = new AA0140();
                        foreach (HeaderItem item in headerItems)
                        {
                            string value = row.GetCell(item.Index) == null ? string.Empty : row.GetCell(item.Index).ToString();
                            temp.GetType().GetProperty(item.FieldName).SetValue(temp, value, null);
                        }
                        temp.Seq = i - 1;
                        list.Add(temp);
                    }
                    #endregion

                    foreach (AA0140 item in list)
                    {
                        string msg = string.Empty;
                        string temp_msg = string.Empty;

                        item.SaveStatus = "Y";  // N:Error Y:Insert

                        // 確認院內碼是否存在於藥品基本檔
                        if (repo.CheckExists_Mimast(item.MMCODE))
                        {
                            temp_msg = "院內碼不存在於藥品基本檔";
                            item.SaveStatus = "N";
                        }

                        int x = 0;
                        if (int.TryParse(item.SELF_CONT_BDATE, out x) == true)
                        {
                            int y = 0;
                            if (int.TryParse(item.SELF_CONT_EDATE, out y) == true)
                            {
                                if (int.Parse(item.SELF_CONT_BDATE) > int.Parse(item.SELF_CONT_EDATE))
                                {
                                    temp_msg = "藥品契約生效起日 大於 藥品契約生效迄日";
                                    msg = msg == string.Empty ? temp_msg : string.Format("{0}、{1}", msg, temp_msg);
                                    item.SaveStatus = "N";
                                }
                                var MaxSELF_CONT_EDATE = repo.ChkMaxTwn_SELF_CONT_EDATE(item.MMCODE, "");

                                if ((MaxSELF_CONT_EDATE != "") && (MaxSELF_CONT_EDATE != null))
                                {
                                    if (int.Parse(item.SELF_CONT_BDATE) <= int.Parse(MaxSELF_CONT_EDATE))
                                    {
                                        temp_msg = "藥品契約生效起日 應大於 相同院內碼藥品契約生效迄日最大值";
                                        msg = msg == string.Empty ? temp_msg : string.Format("{0}、{1}", msg, temp_msg);
                                        item.SaveStatus = "N";
                                    }
                                }
                            }
                            else
                            {
                                temp_msg = "藥品契約生效迄日格式有誤，無法轉換";
                                msg = msg == string.Empty ? temp_msg : string.Format("{0}、{1}", msg, temp_msg);
                                item.SaveStatus = "N";
                            }
                        }
                        else
                        {
                            temp_msg = "藥品契約生效起日格式有誤，無法轉換";
                            msg = msg == string.Empty ? temp_msg : string.Format("{0}、{1}", msg, temp_msg);
                            item.SaveStatus = "N";
                        }


                        if (item.SELF_CONTRACT_NO.Trim() == string.Empty)
                        {
                            temp_msg = "合約案號為空值";
                            msg = msg == string.Empty ? temp_msg : string.Format("{0}、{1}", msg, temp_msg);
                            item.SaveStatus = "N";
                        }

                        if (item.SELF_PUR_UPPER_LIMIT.Trim() == string.Empty)
                        {
                            temp_msg = "採購上限金額為空值";
                            msg = msg == string.Empty ? temp_msg : string.Format("{0}、{1}", msg, temp_msg);
                            item.SaveStatus = "N";
                        }

                        // 確認是否有數值欄位放文字或負值，有的話改0
                        float b = 0;
                        if (float.TryParse(item.SELF_PUR_UPPER_LIMIT, out b) == false)
                        {
                            temp_msg = "採購上限金額無法轉換為數值";
                            msg = msg == string.Empty ? temp_msg : string.Format("{0}、{1}", msg, temp_msg);
                            item.SaveStatus = "N";
                            item.SELF_PUR_UPPER_LIMIT = "0";
                        }
                        else if (float.Parse(item.SELF_PUR_UPPER_LIMIT) < 0)
                        {
                            temp_msg = "採購上限金額小於零";
                            msg = msg == string.Empty ? temp_msg : string.Format("{0}、{1}", msg, temp_msg);
                            item.SaveStatus = "N";
                            item.SELF_PUR_UPPER_LIMIT = "0";
                        }

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
            IEnumerable<AA0140> list = JsonConvert.DeserializeObject<IEnumerable<AA0140>>(itemString);

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0140Repository repo = new AA0140Repository(DBWork);

                    foreach (AA0140 item in list)
                    {
                        item.CreateUser = User.Identity.Name;
                        item.UpdateUser = User.Identity.Name;
                        item.UpdateIp = DBWork.ProcIP;

                        session.Result.afrs = repo.Delete_import(item.MMCODE, item.SELF_CONT_BDATE);
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