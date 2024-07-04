using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.BC;
using WebApp.Models;
using System;
using WebApp.Repository.UR;
using System.Collections.Generic;
using NPOI.SS.UserModel;
using System.Web;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using Newtonsoft.Json;

namespace WebApp.Controllers.BC
{
    public class BC0009Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse MasterAll(FormDataCollection form)
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
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BC0009Repository(DBWork);
                    session.Result.etts = repo.GetMasterAll(p0, p1, p2, p3, p4, p5, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //[HttpPost]
        //public ApiResponse MasterCreate(PH_SMALL_M ph_small_m)
        //{
        //    using (WorkSession session = new WorkSession(this))
        //    {
        //        var DBWork = session.UnitOfWork;
        //        DBWork.BeginTransaction();
        //        try
        //        {
        //            var repo = new BC0009Repository(DBWork);
        //            ph_small_m.DN = repo.getNewDn(repo.getUserInid(User.Identity.Name));
        //            if (!repo.CheckExists(ph_small_m.DN))
        //            {
        //                ph_small_m.APP_USER = User.Identity.Name;
        //                ph_small_m.APP_INID = DBWork.UserInfo.Inid;
        //                ph_small_m.CREATE_USER = User.Identity.Name;
        //                ph_small_m.UPDATE_USER = User.Identity.Name;
        //                ph_small_m.UPDATE_IP = DBWork.ProcIP;
        //                session.Result.afrs = repo.MasterCreate(ph_small_m);
        //                session.Result.etts = repo.MasterGet(ph_small_m.DN);
        //            }
        //            else
        //            {
        //                session.Result.afrs = 0;
        //                session.Result.success = false;
        //                session.Result.msg = "<span style='color:red'>申請單號</span>重複，請重新嘗試。";
        //            }

        //            DBWork.Commit();
        //        }
        //        catch
        //        {
        //            DBWork.Rollback();
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //}

        //[HttpPost]
        //public ApiResponse MasterUpdate(PH_SMALL_M ph_small_m)
        //{
        //    using (WorkSession session = new WorkSession(this))
        //    {
        //        var DBWork = session.UnitOfWork;
        //        DBWork.BeginTransaction();
        //        try
        //        {
        //            var repo = new BC0009Repository(DBWork);
        //            ph_small_m.APP_USER = User.Identity.Name;
        //            ph_small_m.APP_INID = DBWork.UserInfo.Inid;
        //            ph_small_m.UPDATE_USER = User.Identity.Name;
        //            ph_small_m.UPDATE_IP = DBWork.ProcIP;
        //            session.Result.afrs = repo.MasterUpdate(ph_small_m);
        //            session.Result.etts = repo.MasterGet(ph_small_m.DN);

        //            DBWork.Commit();
        //        }
        //        catch
        //        {
        //            DBWork.Rollback();
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //}

        //[HttpPost]
        //public ApiResponse MasterDelete(PH_SMALL_M ph_small_m)
        //{
        //    using (WorkSession session = new WorkSession())
        //    {
        //        var DBWork = session.UnitOfWork;
        //        DBWork.BeginTransaction();
        //        try
        //        {
        //            var repo = new BC0009Repository(DBWork);
        //            if (repo.CheckExists(ph_small_m.DN))
        //            {
        //                session.Result.afrs = repo.DetailDelete(ph_small_m.DN);
        //                session.Result.afrs = repo.MasterDelete(ph_small_m.DN);
        //            }
        //            else
        //            {
        //                session.Result.afrs = 0;
        //                session.Result.success = false;
        //                session.Result.msg = "<span style='color:red'>申請單號</span>不存在。";
        //            }

        //            DBWork.Commit();
        //        }
        //        catch
        //        {
        //            DBWork.Rollback();
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //}

        [HttpPost]
        public ApiResponse MasterAudit(PH_SMALL_M ph_small_m)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BC0009Repository(DBWork);
                    ph_small_m.UPDATE_USER = User.Identity.Name;
                    ph_small_m.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.MasterAudit(ph_small_m);
                    MailController mail = new MailController();
                    var getMAIL = new BC0002Repository(DBWork);
                    string cont = "小額採購簽審通知-待業務副主任審核<br>";
                    cont += "單號 : " + ph_small_m.DN + "<br>";
                    cont += "呈核人: " + DBWork.UserInfo.UserName + "<br>";
                    cont += "呈核時間 : " + (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.ToString("MM/dd HH:mm:ss") + "<br>";
                    mail.Send_Mail("小額採購簽審通知", cont, getMAIL.getNRSMAIL_ADDRESS("業務副主任"));
                    session.Result.etts = repo.MasterGet(ph_small_m.DN);

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

        // ===============================================================================================
        //[HttpPost]
        //public ApiResponse DetailAll(FormDataCollection form)
        //{
        //    var p0 = form.Get("p0");
        //    var p1 = form.Get("p1");
        //    var page = int.Parse(form.Get("page"));
        //    var start = int.Parse(form.Get("start"));
        //    var limit = int.Parse(form.Get("limit"));
        //    var sorters = form.Get("sort");

        //    using (WorkSession session = new WorkSession())
        //    {
        //        var DBWork = session.UnitOfWork;
        //        try
        //        {
        //            var repo = new BC0009Repository(DBWork);
        //            session.Result.etts = repo.GetDetailAll(p0,p1, page, limit, sorters);
        //        }
        //        catch
        //        {
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //}

        //[HttpPost]
        //public ApiResponse DetailCreate(PH_SMALL_D ph_small_d)
        //{
        //    using (WorkSession session = new WorkSession(this))
        //    {
        //        var DBWork = session.UnitOfWork;
        //        DBWork.BeginTransaction();
        //        try
        //        {
        //            var repo = new BC0009Repository(DBWork);

        //            ph_small_d.CREATE_USER = User.Identity.Name;
        //            ph_small_d.UPDATE_IP = DBWork.ProcIP;
        //            ph_small_d.SEQ = repo.getNewSeq(ph_small_d.DN);
        //            if (!repo.CheckExists(ph_small_d.DN, ph_small_d.INID, ph_small_d.NMSPEC))
        //            {
        //                string FileSEQ = repo.getFileSeq();
        //                session.Result.afrs = repo.DetailCreate(ph_small_d);
        //                session.Result.afrs = repo.DetailFileCreate(FileSEQ, ph_small_d.DN, ph_small_d.SEQ, ph_small_d.UK);
        //                session.Result.etts = repo.DetailGet(ph_small_d.DN, ph_small_d.SEQ);

        //                var repo2 = new UR_UploadRepository(DBWork);
        //                repo2.Confirm(ph_small_d.UK);
        //            }
        //            else
        //            {
        //                session.Result.afrs = 0;
        //                session.Result.success = false;
        //                session.Result.msg = "<span style='color:red'>資料重複</span>，請重新輸入。";
        //            }

        //            DBWork.Commit();
        //        }
        //        catch
        //        {
        //            DBWork.Rollback();
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //}

        //[HttpPost]
        //public ApiResponse DetailUpdate(PH_SMALL_D ph_small_d)
        //{
        //    using (WorkSession session = new WorkSession(this))
        //    {
        //        var DBWork = session.UnitOfWork;
        //        DBWork.BeginTransaction();
        //        try
        //        {
        //            ph_small_d.UPDATE_USER = User.Identity.Name;
        //            ph_small_d.UPDATE_IP = DBWork.ProcIP;

        //            var repo = new BC0009Repository(DBWork);
        //            session.Result.afrs = repo.DetailUpdate(ph_small_d);
        //            session.Result.etts = repo.DetailGet(ph_small_d.DN, ph_small_d.SEQ);

        //            var repo2 = new UR_UploadRepository(DBWork);
        //            repo2.Confirm(ph_small_d.UK);

        //            DBWork.Commit();
        //        }
        //        catch
        //        {
        //            DBWork.Rollback();
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //}

        //[HttpPost]
        //public ApiResponse DetailDelete(PH_SMALL_D ph_small_d)
        //{
        //    using (WorkSession session = new WorkSession())
        //    {
        //        var DBWork = session.UnitOfWork;
        //        DBWork.BeginTransaction();
        //        try
        //        {
        //            var repo = new BC0009Repository(DBWork);
        //            if (repo.CheckExists(ph_small_d.DN, ph_small_d.INID, ph_small_d.NMSPEC))
        //            {
        //                session.Result.afrs = repo.DetailFileDelete(ph_small_d.DN, ph_small_d.UK);
        //                session.Result.afrs = repo.DetailDelete(ph_small_d.DN, ph_small_d.SEQ);

        //                var repo2 = new UR_UploadRepository(DBWork);
        //                repo2.DeleteByUK(ph_small_d.UK);
        //            }
        //            else
        //            {
        //                session.Result.afrs = 0;
        //                session.Result.success = false;
        //                session.Result.msg = "<span style='color:red'>項次</span>不存在。";
        //            }

        //            DBWork.Commit();
        //        }
        //        catch
        //        {
        //            DBWork.Rollback();
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //}

        [HttpPost]
        public ApiResponse GetInidByTuser(FormDataCollection form)
        {
            string tuser = form.Get("TUSER");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BC0009Repository repo = new BC0009Repository(DBWork);
                    session.Result.etts = repo.GetInidByTuser(tuser);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetAgennmByAgenno(FormDataCollection form)
        {
            string agen_no = form.Get("AGEN_NO");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BC0009Repository repo = new BC0009Repository(DBWork);
                    session.Result.etts = repo.GetAgennmByAgenno(agen_no);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMmdataByMmcode(FormDataCollection form)
        {
            string mmcode = form.Get("MMCODE");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BC0009Repository repo = new BC0009Repository(DBWork);
                    session.Result.etts = repo.GetMmdataByMmcode(mmcode);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetInidCombo(FormDataCollection form)
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
                    BC0009Repository repo = new BC0009Repository(DBWork);
                    session.Result.etts = repo.GetInidCombo(p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMmcodeCombo(FormDataCollection form)
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
                    BC0009Repository repo = new BC0009Repository(DBWork);
                    session.Result.etts = repo.GetMmcodeCombo(p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetAgennoCombo(FormDataCollection form)
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
                    BC0009Repository repo = new BC0009Repository(DBWork);
                    session.Result.etts = repo.GetAgennoCombo(p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //匯入 
        //==========================================================
        //[HttpPost]
        //public ApiResponse CheckExcel()  call BC0002
        //{
        //    using (WorkSession session = new WorkSession(this))
        //    {

        //        List<PH_SMALL_D> list = new List<PH_SMALL_D>();
        //        UnitOfWork DBWork = session.UnitOfWork;


        //        string[] arr = { "院內碼", "品名及規格廠牌", "需求單位", "數量", "計量單位", "單價", "衛材計價方式", "備考" };

        //        try
        //        {
        //            BC0009Repository repo = new BC0009Repository(DBWork);

        //            IWorkbook workBook;
        //            var HttpPostedFile = HttpContext.Current.Request.Files["file"];

        //            if (Path.GetExtension(HttpPostedFile.FileName).ToLower() == ".xls")
        //            {
        //                workBook = new HSSFWorkbook(HttpPostedFile.InputStream);
        //            }
        //            else
        //            {
        //                workBook = new XSSFWorkbook(HttpPostedFile.InputStream);
        //            }
        //            var sheet = workBook.GetSheetAt(0);
        //            IRow headerRow = sheet.GetRow(0);//由第一列取標題做為欄位名稱
        //            int cellCount = 0;
        //            bool isValid = true;

        //            if (headerRow != null)
        //            {
        //                cellCount = headerRow.LastCellNum;
        //            }
        //            else
        //            {
        //                isValid = false;
        //                session.Result.msg = "檔案格式不同, 請下載範本更新";
        //            }

        //            int i, j;
        //            bool allConfirm =true;
        //            string[] cellstring = new string[cellCount];
        //            int[] dataLen = { 13, 100, 6, 38, 10, 38, 100,300 };
        //            #region 檢查檔案格式
        //            for (j = 0; j < cellCount; j++)
        //            {
        //                isValid = headerRow.GetCell(j) == null ? false : true;
        //                if (!isValid)
        //                {
        //                    session.Result.msg = "檔案格式不同, 請下載範本更新";
        //                    break;
        //                }
        //            }

        //            //檢查標頭
        //            if (isValid)
        //            {
        //                for (j = 0; j < cellCount; j++)
        //                {
        //                    isValid = headerRow.GetCell(j).ToString() == arr[j] ? true : false;
        //                    if (!isValid)
        //                    { break; }
        //                }
        //            }

        //            if (!isValid)
        //            {
        //                session.Result.msg = "檔案格式不同, 請下載範本更新";
        //            }
        //            #endregion

        //            if (isValid)
        //            {

        //                //略過第零列(標題列)，一直處理至最後一列
        //                for (i = 1; i <= sheet.LastRowNum; i++)
        //                {
        //                    string[] rowErrStr = new string[cellCount];

        //                    IRow row = sheet.GetRow(i);
        //                    if (row is null) { continue; }
        //                    #region 資料驗證
        //                    //是否為空白
        //                    for (j = 0; j < cellCount; j++)
        //                    {
        //                        cellstring[j] = row.GetCell(j) == null ? "" : row.GetCell(j).ToString();

        //                        if (j==0||j==2||j == 7) continue;

        //                        if (string.IsNullOrWhiteSpace(cellstring[j]))
        //                        {
        //                            rowErrStr[j] = "[" + arr[j] + "]不可空白";
        //                        }
        //                    }


        //                    //檢查資料長度
        //                    for (j = 0; j < cellCount; j++)
        //                    {
        //                        if (cellstring[j].Length > dataLen[j])
        //                        {
        //                            rowErrStr[j] = "[" + arr[j] + "]長度超出，最大長度" + dataLen[j].ToString();
        //                        }
        //                    }

        //                    if (cellstring[0] != "")
        //                    {
        //                        // 檢查院內碼是否已存在
        //                        if (!repo.CheckMmcodeExists(cellstring[0]))
        //                        {
        //                            rowErrStr[0] = "[院內碼]" + cellstring[0] + "院內碼不存在";
        //                        }
        //                    }
        //                    if (cellstring[2] != "")
        //                    {
        //                        // 檢查需求單位是否已存在
        //                        if (!repo.CheckInidExists(cellstring[2]))
        //                        {
        //                            rowErrStr[2] = "[需求單位]" + cellstring[2] + "不存在";
        //                        }
        //                    }
        //                    // 檢查數量是否為數字且大於0
        //                    int a;
        //                    if (int.TryParse(cellstring[3], out a))
        //                    {
        //                        if (int.Parse(cellstring[3]) <= 0)
        //                        {
        //                            rowErrStr[3] = "[數量]" + cellstring[3] + "要大於0";
        //                        }
        //                    }
        //                    else
        //                    {
        //                        rowErrStr[3] = "[數量]" + cellstring[3] + "要大於0";
        //                    }

        //                    string CHARGE = cellstring[6];

        //                    if (CHARGE != "健保給付" && CHARGE != "健保處置內含" && CHARGE != "自費" && CHARGE != "自費處置內含")
        //                    {
        //                        rowErrStr[6] = "[衛材計價方式]" + CHARGE + "填寫錯誤";
        //                    }
        //                    #endregion

        //                    #region 處理輸出資料
        //                    string resultMsg = "";

        //                    foreach (string errStr in rowErrStr)
        //                    {
        //                        if (!string.IsNullOrEmpty(errStr))
        //                            resultMsg += errStr + "</br>";
        //                    }
        //                    if (resultMsg.Length > 0)
        //                    {
        //                        allConfirm = false;  //有一筆資料不合格即無法匯入
        //                        //刪除最後的換行符號
        //                        resultMsg ="<span style = 'color:red'>"+ resultMsg.Substring(0, resultMsg.Length - 5)+ "</span>";
        //                    }
        //                    else
        //                    {
        //                        resultMsg = "OK";
        //                    }
        //                    #endregion
        //                    PH_SMALL_D pH_SMALL_D = new PH_SMALL_D();

        //                    pH_SMALL_D.SEQ = i.ToString();
        //                    pH_SMALL_D.MMCODE = cellstring[0];
        //                    pH_SMALL_D.NMSPEC = cellstring[1];
        //                    pH_SMALL_D.INID = cellstring[2];
        //                    if (int.TryParse(cellstring[3], out a))
        //                    {
        //                        pH_SMALL_D.QTY = int.Parse(cellstring[3]);
        //                    }
        //                    pH_SMALL_D.UNIT = cellstring[4];
        //                    if (int.TryParse(cellstring[5], out a))
        //                    {
        //                        pH_SMALL_D.PRICE = int.Parse(cellstring[5]);
        //                    }

        //                    switch (cellstring[6])
        //                    {
        //                        case "健保給付":
        //                            cellstring[6] = "A 健保給付";
        //                            break;
        //                        case "健保處置內含":
        //                            cellstring[6] = "B 健保處置內含";
        //                            break;
        //                        case "自費":
        //                            cellstring[6] = "C 自費";
        //                            break;
        //                        case "自費處置內含":
        //                            cellstring[6] = "D 自費處置內含";
        //                            break;
        //                        case "不計價":
        //                            cellstring[6] = "E 不計價";
        //                            break;
        //                    }

        //                    pH_SMALL_D.CHARGE = cellstring[6];
        //                    pH_SMALL_D.MEMO = cellstring[7];

        //                    pH_SMALL_D.CHECK_RESULT = resultMsg;

        //                    if (int.TryParse(cellstring[3], out a) && int.TryParse(cellstring[5], out a))
        //                    {
        //                        pH_SMALL_D.TOTAL_PRICE = int.Parse(cellstring[3])* int.Parse(cellstring[5]);
        //                    }



        //                    list.Add(pH_SMALL_D);
        //                }
        //            }


        //            if (!isValid)
        //            {
        //                session.Result.success = false;
        //            }
        //            else
        //            {
        //                session.Result.etts = list;
        //                if (allConfirm)
        //                {
        //                    session.Result.msg = "OK";
        //                }
        //                else
        //                {
        //                    session.Result.msg = "FALSE";
        //                }
        //            }
        //        }
        //        catch
        //        {
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //}

        //[HttpPost]
        //public ApiResponse Import(FormDataCollection formData)
        //{
        //    using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
        //    {
        //        var DBWork = session.UnitOfWork;
        //        IEnumerable<PH_SMALL_D> pH_SMALL_D = JsonConvert.DeserializeObject<IEnumerable<PH_SMALL_D>>(formData["data"]);
        //        List<PH_SMALL_D> list_pH_SMALL_D = new List<PH_SMALL_D>();
        //        DBWork.BeginTransaction();
        //        try
        //        {
        //            var repo = new BC0009Repository(DBWork);

        //            IEnumerable<PH_SMALL_D> Delete_pH_SMALL_D = repo.GetDetailAll2(formData["DN"]);

        //            //刪除原有明細資料
        //            foreach (PH_SMALL_D data in Delete_pH_SMALL_D)
        //            {
        //                session.Result.afrs = repo.DetailFileDelete(data.DN, data.UK);
        //                session.Result.afrs = repo.DetailDelete(data.DN, data.SEQ);

        //                var repo2 = new UR_UploadRepository(DBWork);
        //                repo2.DeleteByUK(data.UK);
        //            }


        //            foreach (PH_SMALL_D data in pH_SMALL_D)
        //            {
        //                data.CREATE_USER = User.Identity.Name;
        //                data.UPDATE_IP = DBWork.ProcIP;
        //                data.DN = formData["DN"];
        //                switch (data.CHARGE)
        //                {
        //                    case "A 健保給付":
        //                        data.CHARGE = "A";
        //                        break;
        //                    case "B 健保處置內含":
        //                        data.CHARGE = "B";
        //                        break;
        //                    case "C 自費":
        //                        data.CHARGE = "C";
        //                        break;
        //                    case "D 自費處置內含":
        //                        data.CHARGE = "D";
        //                        break;
        //                }

        //                if (!repo.CheckExists(data.DN, data.INID, data.NMSPEC))
        //                {
        //                    data.SEQ = repo.getNewSeq(data.DN);
        //                    try
        //                    {
        //                        string FileSEQ = repo.getFileSeq();
        //                        session.Result.afrs = repo.DetailCreate(data);
        //                        session.Result.afrs = repo.DetailFileCreate(FileSEQ, data.DN, data.SEQ, data.UK);
        //                        session.Result.etts = repo.DetailGet(data.DN, data.SEQ);

        //                        var repo2 = new UR_UploadRepository(DBWork);
        //                        repo2.Confirm(data.UK);

        //                        data.IMPORT_RESULT = "匯入成功";
        //                    }
        //                    catch(Exception ex)
        //                    {
        //                        data.IMPORT_RESULT = "匯入失敗";
        //                    }
        //                }
        //                else
        //                {
        //                    data.IMPORT_RESULT = "資料未上傳";
        //                }


        //                switch (data.CHARGE)
        //                {
        //                    case "A":
        //                        data.CHARGE = "A 健保給付";
        //                        break;
        //                    case "B":
        //                        data.CHARGE = "B 健保處置內含";
        //                        break;
        //                    case "C":
        //                        data.CHARGE = "C 自費";
        //                        break;
        //                    case "D":
        //                        data.CHARGE = "D 自費處置內含";
        //                        break;
        //                    case "不計價":
        //                        data.CHARGE = "E 不計價";
        //                        break;
        //                }


        //                list_pH_SMALL_D.Add(data);
        //            }

        //            DBWork.Commit();
        //            session.Result.etts = list_pH_SMALL_D;
        //        }
        //        catch
        //        {
        //            DBWork.Rollback();
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //}
        [HttpPost]
        public ApiResponse MasterReject(PH_SMALL_M ph_small_m)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    ph_small_m.APP_USER1 = User.Identity.Name;
                    ph_small_m.UPDATE_USER = User.Identity.Name;
                    ph_small_m.UPDATE_IP = DBWork.ProcIP;
                    var repo1 = new BC0009Repository(DBWork);
                    session.Result.afrs = repo1.MasterReject(ph_small_m);
                    var repo2 = new BC0002Repository(DBWork);
                    session.Result.etts = repo2.MasterGet(ph_small_m.DN);
                    string email = repo2.getUser_MAIL_ADDRESS(ph_small_m.DN);
                    if (email != null && email != "")
                    {
                        BC0004Controller BC4 = new BC0004Controller();
                        BC4.sendRejectMail(ph_small_m, DBWork.UserInfo.UserName, email);
                    }
                    //MailController mail = new MailController();
                    //string cont = "小額採購通知-審查剔退<br>";
                    //cont += "單號 : " + ph_small_m.DN + "<br>";
                    //cont += "審核人: " + DBWork.UserInfo.UserName + "<br>";
                    //cont += "剔退時間 : " + (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.ToString("MM/dd HH:mm:ss") + "<br>";
                    //mail.Send_Mail("小額採購通知", cont, repo2.getUser_MAIL_ADDRESS(ph_small_m.APP_USER));
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
    }
}