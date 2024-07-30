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
using System.Data;

namespace WebApp.Controllers.BC
{
    public class BC0002Controller : SiteBase.BaseApiController
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

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BC0002Repository(DBWork);
                    session.Result.etts = repo.GetMasterAll(p0, p1, p2, p3, p4, p5, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse MasterCreate(PH_SMALL_M ph_small_m)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BC0002Repository(DBWork);
                    ph_small_m.DN = repo.getNewDn(repo.getUserInid(User.Identity.Name));
                    if (!repo.CheckExists(ph_small_m.DN))
                    {
                        ph_small_m.APP_USER = User.Identity.Name;
                        ph_small_m.APP_INID = DBWork.UserInfo.Inid;
                        ph_small_m.CREATE_USER = User.Identity.Name;
                        ph_small_m.UPDATE_USER = User.Identity.Name;
                        ph_small_m.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.MasterCreate(ph_small_m);
                        session.Result.etts = repo.MasterGet(ph_small_m.DN);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>申請單號</span>重複，請重新嘗試。";
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
        public ApiResponse MasterUpdate(PH_SMALL_M ph_small_m)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BC0002Repository(DBWork);
                    ph_small_m.APP_USER = User.Identity.Name;
                    ph_small_m.APP_INID = DBWork.UserInfo.Inid;
                    ph_small_m.UPDATE_USER = User.Identity.Name;
                    ph_small_m.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.MasterUpdate(ph_small_m);
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

        [HttpPost]
        public ApiResponse MasterDelete(PH_SMALL_M ph_small_m)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BC0002Repository(DBWork);
                    if (repo.CheckExists(ph_small_m.DN))
                    {
                        session.Result.afrs = repo.DetailDelete(ph_small_m.DN);
                        session.Result.afrs = repo.MasterDelete(ph_small_m.DN);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>申請單號</span>不存在。";
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
        public ApiResponse MasterAudit(PH_SMALL_M ph_small_m)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BC0002Repository(DBWork);

                    ph_small_m.UPDATE_USER = User.Identity.Name;
                    ph_small_m.UPDATE_IP = DBWork.ProcIP;

                    // 2023-09-07 檢查最小撥補量
                    IEnumerable<PH_SMALL_D> small_ds = repo.CheckMinOrdqty(ph_small_m.DN);
                    string minordqty_mmcodes = string.Empty;
                    foreach (PH_SMALL_D small_d in small_ds) {
                        int i;
                        int min_ordqty = int.TryParse(small_d.SEQ, out i) ? int.Parse(small_d.SEQ) : 1;
                        if (small_d.QTY % min_ordqty != 0) {
                            if (string.IsNullOrEmpty(minordqty_mmcodes) == false) {
                                minordqty_mmcodes += "、";
                            }
                            minordqty_mmcodes += small_d.MMCODE;
                        }
                    }

                    if (string.IsNullOrEmpty(minordqty_mmcodes) == false) {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = string.Format(@"下列院內碼不為最小撥補量倍數，請重新確認：<br>{0}", minordqty_mmcodes) ;
                        return session.Result;
                    }

                    session.Result.afrs = repo.MasterAudit(ph_small_m);
                    MailController mail = new MailController();
                    string cont = "";
                    string email = "";
                    if (ph_small_m.NRSFLAG.Equals("N"))
                    {
                        cont = "小額採購通知-待單位主管審核<br>";
                        email = repo.getMAIL_ADDRESS(ph_small_m.NEXT_USER);
                    }
                    else
                    {
                        cont = "小額採購通知-待業務督導審核<br>";
                        email = repo.getNRSMAIL_ADDRESS("業務督導");
                    }
                    cont += "單號 : " + ph_small_m.DN + "<br>";
                    cont += "呈核人: " + DBWork.UserInfo.UserName + "<br>";
                    cont += "呈核時間 : " + (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.ToString("MM/dd HH:mm:ss") + "<br>";
                    if (email != null & email != "") mail.Send_Mail("小額採購通知", cont, email);
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
        [HttpPost]
        public ApiResponse DetailAll(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BC0002Repository(DBWork);
                    session.Result.etts = repo.GetDetailAll(p0, p1, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse DetailCreate(PH_SMALL_D ph_small_d)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BC0002Repository(DBWork);

                    ph_small_d.CREATE_USER = User.Identity.Name;
                    ph_small_d.UPDATE_IP = DBWork.ProcIP;
                    ph_small_d.SEQ = repo.getNewSeq(ph_small_d.DN);
                    string chkOK = "N";
                    string chkMSG = "";

                    if (repo.CheckInidExists(ph_small_d.INID))
                    {
                        if (ph_small_d.MMCODE == null || ph_small_d.MMCODE == "")
                        {
                            if (!repo.CheckDetailExist1(ph_small_d.DN, ph_small_d.INID, ph_small_d.NMSPEC))
                                chkOK = "Y";
                            else
                                chkMSG = "品名及規格廠牌:" + ph_small_d.NMSPEC + "； 需求單位: " + ph_small_d.INID + "已存在。";
                        }
                        else
                        {
                            if (!repo.CheckDetailExist2(ph_small_d.DN, ph_small_d.INID, ph_small_d.MMCODE))
                                chkOK = "Y";
                            else
                                chkMSG = "院內碼:" + ph_small_d.MMCODE + "； 需求單位:" + ph_small_d.INID + "已存在。";

                        }

                        //2023-09-07: 判斷最小撥補量
                        string minOrdqty_s = repo.GetMinOrqty(ph_small_d.MMCODE);
                        int i;
                        int minOrdqty_i = int.TryParse(minOrdqty_s, out i) ? int.Parse(minOrdqty_s) : 1;
                        if (ph_small_d.QTY % minOrdqty_i != 0) {
                            chkMSG = "數量需為最小撥補量倍數";
                            chkOK = "";
                        }
                        
                        if (chkOK == "Y")
                        {
                            string FileSEQ = repo.getFileSeq();
                            session.Result.afrs = repo.DetailCreate(ph_small_d);
                            session.Result.afrs = repo.DetailFileCreate(FileSEQ, ph_small_d.DN, ph_small_d.SEQ, ph_small_d.UK);
                            session.Result.etts = repo.DetailGet(ph_small_d.DN, ph_small_d.SEQ);

                            var repo2 = new UR_UploadRepository(DBWork);
                            repo2.Confirm(ph_small_d.UK);
                        }
                        else
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = "<span style='color:red'>" + chkMSG + "</span>，請重新輸入。";
                        }

                        DBWork.Commit();
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>需求單位:" + ph_small_d.INID + "</span> 資料錯誤，請重新輸入。";
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

        [HttpPost]
        public ApiResponse DetailUpdate(PH_SMALL_D ph_small_d)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BC0002Repository(DBWork);
                    ph_small_d.UPDATE_USER = User.Identity.Name;
                    ph_small_d.UPDATE_IP = DBWork.ProcIP;
                    string chkOK = "N";
                    string chkMSG = "";
                    if (ph_small_d.MMCODE != "" && repo.CheckMmcodeExists(ph_small_d.MMCODE))
                    {
                        if (repo.CheckInidExists(ph_small_d.INID))
                        {
                            if (ph_small_d.MMCODE != ph_small_d.OLD_MMCODE || ph_small_d.NMSPEC != ph_small_d.OLD_NMSPEC || ph_small_d.INID != ph_small_d.OLD_INID)
                            {
                                if (ph_small_d.MMCODE == null || ph_small_d.MMCODE == "")
                                {
                                    if (!repo.CheckDetailExist1(ph_small_d.DN, ph_small_d.INID, ph_small_d.NMSPEC))
                                        chkOK = "Y";
                                    else
                                        chkMSG = "品名及規格廠牌:" + ph_small_d.NMSPEC + "； 需求單位: " + ph_small_d.INID + "已存在。";
                                }
                                else
                                {
                                    if (!repo.CheckDetailExist2(ph_small_d.DN, ph_small_d.INID, ph_small_d.MMCODE))
                                        chkOK = "Y";
                                    else
                                        chkMSG = "院內碼:" + ph_small_d.MMCODE + "； 需求單位:" + ph_small_d.INID + "已存在。";
                                }
                            }
                            else
                            {
                                chkOK = "Y";
                            }

                            if (chkOK == "Y")
                            {
                                session.Result.afrs = repo.DetailUpdate(ph_small_d);
                                session.Result.etts = repo.DetailGet(ph_small_d.DN, ph_small_d.SEQ);

                                var repo2 = new UR_UploadRepository(DBWork);
                                repo2.Confirm(ph_small_d.UK);

                                DBWork.Commit();
                            }
                            else
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = "<span style='color:red'>" + chkMSG + "</span>，請重新輸入。";
                            }
                        }
                        else
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = "<span style='color:red'>需求單位:" + ph_small_d.INID + "</span> 資料錯誤，請重新輸入。";
                        }
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>院內碼:" + ph_small_d.MMCODE + "</span> 資料錯誤，請重新輸入。";
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

        [HttpPost]
        public ApiResponse DetailDelete(PH_SMALL_D ph_small_d)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BC0002Repository(DBWork);
                    if (repo.CheckDetailExist1(ph_small_d.DN, ph_small_d.INID, ph_small_d.NMSPEC))
                    {
                        session.Result.afrs = repo.DetailFileDelete(ph_small_d.DN, ph_small_d.UK);
                        session.Result.afrs = repo.DetailDelete(ph_small_d.DN, ph_small_d.SEQ);

                        var repo2 = new UR_UploadRepository(DBWork);
                        repo2.DeleteByUK(ph_small_d.UK);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>項次</span>不存在。";
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
        public ApiResponse GetInidByTuser(FormDataCollection form)
        {
            string tuser = form.Get("TUSER");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BC0002Repository repo = new BC0002Repository(DBWork);
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
                    BC0002Repository repo = new BC0002Repository(DBWork);
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
                    BC0002Repository repo = new BC0002Repository(DBWork);
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
                    BC0002Repository repo = new BC0002Repository(DBWork);
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
                    BC0002Repository repo = new BC0002Repository(DBWork);
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
                    BC0002Repository repo = new BC0002Repository(DBWork);
                    session.Result.etts = repo.GetAgennoCombo(p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetBOSS(FormDataCollection form)
        {
            var bossname = form.Get("BOSS");
            var qry = form.Get("QRY");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BC0002Repository(DBWork);
                    var app_inid = repo.getUserInid(User.Identity.Name);
                    session.Result.etts = repo.GetBOSS(bossname, qry, app_inid, page, limit, sorters);
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
        [HttpPost]
        public ApiResponse CheckExcel()
        {
            using (WorkSession session = new WorkSession(this))
            {

                List<PH_SMALL_D> list = new List<PH_SMALL_D>();
                UnitOfWork DBWork = session.UnitOfWork;


                string[] arr = { "院內碼", "品名及規格廠牌", "需求單位", "數量", "計量單位", "單價", "衛材計價方式", "備考" };

                try
                {
                    BC0002Repository repo = new BC0002Repository(DBWork);

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
                    int cellCount = 0;
                    bool isValid = true;

                    if (headerRow != null)
                    {
                        cellCount = headerRow.LastCellNum;
                    }
                    else
                    {
                        isValid = false;
                        session.Result.msg = "檔案格式不同, 請下載範本更新";
                    }

                    int i, j;
                    bool allConfirm = true;
                    string[] cellstring = new string[cellCount];
                    int[] dataLen = { 13, 250, 6, 38, 10, 38, 100, 300 };
                    #region 檢查檔案格式
                    for (j = 0; j < cellCount; j++)
                    {
                        isValid = headerRow.GetCell(j) == null ? false : true;
                        if (!isValid)
                        {
                            session.Result.msg = "檔案格式不同, 請下載範本更新";
                            break;
                        }
                    }

                    //檢查標頭
                    if (isValid)
                    {
                        for (j = 0; j < cellCount; j++)
                        {
                            isValid = headerRow.GetCell(j).ToString() == arr[j] ? true : false;
                            if (!isValid)
                            { break; }
                        }
                    }

                    if (!isValid)
                    {
                        session.Result.msg = "檔案格式不同, 請下載範本更新";
                    }
                    #endregion

                    if (isValid)
                    {

                        //略過第零列(標題列)，一直處理至最後一列
                        for (i = 1; i <= sheet.LastRowNum; i++)
                        {
                            string[] rowErrStr = new string[cellCount];

                            IRow row = sheet.GetRow(i);
                            if (row is null) { continue; }

                            #region 資料驗證
                            //院內碼	品名及規格廠牌	需求單位	數量	計量單位	單價	衛材計價方式	備考
                            //是否為空白
                            string mmcode = "";
                            for (j = 0; j < cellCount; j++)
                            {
                                cellstring[j] = row.GetCell(j) == null ? "" : row.GetCell(j).ToString();
                                //if (j==0||j==2||j == 7) continue;
                                if (cellstring[0] == "" && (j == 1 || j == 2 || j == 3 || j == 4 || j == 5 || j == 6))
                                {
                                    if (string.IsNullOrWhiteSpace(cellstring[j]))
                                        rowErrStr[j] = "[" + arr[j] + "]不可空白";
                                }
                                else if (cellstring[0] != "" && (j == 2 || j == 3))
                                {
                                    if (string.IsNullOrWhiteSpace(cellstring[j]))
                                        rowErrStr[j] = "[" + arr[j] + "]不可空白";
                                }
                            }
                            if (!(cellstring[0] == "" && cellstring[1] == "" && cellstring[2] == ""))
                            {

                                //檢查資料長度
                                for (j = 0; j < cellCount; j++)
                                {
                                    if (cellstring[j].Length > dataLen[j])
                                    {
                                        rowErrStr[j] = "[" + arr[j] + "]長度超出，最大長度" + dataLen[j].ToString();
                                    }
                                }

                                if (cellstring[0] != "")
                                {
                                    // 檢查院內碼是否已存在
                                    if (!repo.CheckMmcodeExists(cellstring[0]))
                                    {
                                        rowErrStr[0] = "[院內碼]" + cellstring[0] + "院內碼不存在";
                                    }
                                }
                                if (cellstring[2] != "")
                                {
                                    // 檢查需求單位是否已存在
                                    if (!repo.CheckInidExists(cellstring[2]))
                                    {
                                        rowErrStr[2] = "[需求單位]" + cellstring[2] + "不存在";
                                    }
                                }
                                // 檢查數量是否為數字且大於0
                                int a;
                                if (int.TryParse(cellstring[3], out a))
                                {
                                    if (int.Parse(cellstring[3]) <= 0)
                                    {
                                        rowErrStr[3] = "[數量]" + cellstring[3] + "要大於0";
                                    }
                                }
                                else
                                {
                                    rowErrStr[3] = "[數量]" + cellstring[3] + "要大於0";
                                }

                                string CHARGE = cellstring[6];
                                //院內碼有值可以不填
                                if (cellstring[0] == "" && (CHARGE != "不給付" && CHARGE != "給付" && CHARGE != "條件給付" && CHARGE != "醫院自行吸收" && CHARGE != "預設自費"))
                                {
                                    rowErrStr[6] = "[衛材計價方式]" + CHARGE + "填寫錯誤";
                                }
                                #endregion

                                #region 處理輸出資料
                                string resultMsg = "";

                                foreach (string errStr in rowErrStr)
                                {
                                    if (!string.IsNullOrEmpty(errStr))
                                        resultMsg += errStr + "</br>";
                                }
                                if (resultMsg.Length > 0)
                                {
                                    allConfirm = false;  //有一筆資料不合格即無法匯入
                                                         //刪除最後的換行符號
                                    resultMsg = "<span style = 'color:red'>" + resultMsg.Substring(0, resultMsg.Length - 5) + "</span>";
                                }
                                else
                                {
                                    resultMsg = "OK";
                                }
                                #endregion
                                PH_SMALL_D pH_SMALL_D = new PH_SMALL_D();

                                pH_SMALL_D.SEQ = i.ToString();
                                pH_SMALL_D.MMCODE = cellstring[0];
                                pH_SMALL_D.NMSPEC = cellstring[1];
                                pH_SMALL_D.INID = cellstring[2];
                                if (int.TryParse(cellstring[3], out a))
                                {
                                    pH_SMALL_D.QTY = int.Parse(cellstring[3]);
                                }
                                pH_SMALL_D.UNIT = cellstring[4];
                                if (int.TryParse(cellstring[5], out a))
                                {
                                    pH_SMALL_D.PRICE = int.Parse(cellstring[5]);
                                }

                                switch (cellstring[6])
                                {
                                    case "不給付":
                                        cellstring[6] = "0 不給付";
                                        break;
                                    case "給付":
                                        cellstring[6] = "1 給付";
                                        break;
                                    case "條件給付":
                                        cellstring[6] = "2 條件給付";
                                        break;
                                    case "醫院自行吸收":
                                        cellstring[6] = "3 醫院自行吸收";
                                        break;
                                    case "預設自費":
                                        cellstring[6] = "4 預設自費";
                                        break;
                                }

                                pH_SMALL_D.CHARGE = cellstring[6];
                                pH_SMALL_D.MEMO = cellstring[7];

                                pH_SMALL_D.CHECK_RESULT = resultMsg;

                                if (int.TryParse(cellstring[3], out a) && int.TryParse(cellstring[5], out a))
                                {
                                    pH_SMALL_D.TOTAL_PRICE = int.Parse(cellstring[3]) * int.Parse(cellstring[5]);
                                }

                                list.Add(pH_SMALL_D);
                            }
                        }
                    }


                    if (!isValid)
                    {
                        session.Result.success = false;
                    }
                    else
                    {
                        session.Result.etts = list;
                        if (allConfirm)
                        {
                            session.Result.msg = "OK";
                        }
                        else
                        {
                            session.Result.msg = "FALSE";
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
        public ApiResponse Import(FormDataCollection formData)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;

                IEnumerable<PH_SMALL_D> pH_SMALL_D = JsonConvert.DeserializeObject<IEnumerable<PH_SMALL_D>>(formData["data"]);
                List<PH_SMALL_D> list_pH_SMALL_D = new List<PH_SMALL_D>();
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BC0002Repository(DBWork);

                    IEnumerable<PH_SMALL_D> Delete_pH_SMALL_D = repo.GetDetailAll2(formData["DN"]);
                    //刪除原有明細資料

                    DataTable dt = repo.getATTFileUK(formData["DN"]);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var repo2 = new UR_UploadRepository(DBWork);
                        repo2.DeleteByUK(dt.Rows[i]["UK"].ToString().Trim());
                    }
                    session.Result.afrs = repo.DetailFileDelete(formData["DN"]);
                    session.Result.afrs = repo.DetailDelete(formData["DN"]);

                    foreach (PH_SMALL_D data in pH_SMALL_D)
                    {
                        //有院內碼抓基本檔 MI_MAST
                        if (data.MMCODE != "")
                        {
                            DataTable dt_mmcode = repo.GetMmcodeData(data.MMCODE);
                            if (dt_mmcode.Rows.Count > 0)
                            {
                                data.NMSPEC = dt_mmcode.Rows[0]["MMNAME_C"].ToString().Trim();
                                data.UNIT = dt_mmcode.Rows[0]["BASE_UNIT"].ToString().Trim();
                                data.PRICE = float.Parse(dt_mmcode.Rows[0]["UPRICE"].ToString().Trim());
                                data.CHARGE = dt_mmcode.Rows[0]["M_PAYKIND"].ToString().Trim();
                            }
                        }
                        data.CREATE_USER = User.Identity.Name;
                        data.UPDATE_IP = DBWork.ProcIP;
                        data.DN = formData["DN"];
                        switch (data.CHARGE)
                        {
                            case "0 不給付":
                                data.CHARGE = "0";
                                break;
                            case "1 給付":
                                data.CHARGE = "1";
                                break;
                            case "2 條件給付":
                                data.CHARGE = "2";
                                break;
                            case "3 醫院自行吸收":
                                data.CHARGE = "3";
                                break;
                            case "4 預設自費":
                                data.CHARGE = "4";
                                break;
                            //針對有院內碼 MI_MAST["M_PAYKIND"]
                            case "0":
                                data.CHARGE = "0";
                                break;
                            case "1":
                                data.CHARGE = "1";
                                break;
                            case "2":
                                data.CHARGE = "2";
                                break;
                            case "3":
                                data.CHARGE = "3";
                                break;
                            case "4":
                                data.CHARGE = "4";
                                break;
                            case "不給付":
                                data.CHARGE = "0";
                                break;
                            case "給付":
                                data.CHARGE = "1";
                                break;
                            case "條件給付":
                                data.CHARGE = "2";
                                break;
                            case "醫院自行吸收":
                                data.CHARGE = "3";
                                break;
                            case "預設自費":
                                data.CHARGE = "4";
                                break;
                        }
                        string chkOK = "N";
                        if (data.MMCODE == null || data.MMCODE == "")
                        {
                            if (!repo.CheckDetailExist1(data.DN, data.INID, data.NMSPEC))
                                chkOK = "Y";
                        }
                        else
                        {
                            if (!repo.CheckDetailExist2(data.DN, data.INID, data.MMCODE))
                                chkOK = "Y";
                        }
                        if (chkOK == "Y")
                        {
                            try
                            {
                                data.SEQ = repo.getNewSeq(data.DN);
                                string FileSEQ = repo.getFileSeq();
                                session.Result.afrs = repo.DetailCreate(data);
                                session.Result.afrs = repo.DetailFileCreate(FileSEQ, data.DN, data.SEQ, data.UK);
                                //session.Result.etts = repo.DetailGet(data.DN, data.SEQ);

                                //var repo2 = new UR_UploadRepository(DBWork);
                                //repo2.Confirm(data.UK);

                                data.IMPORT_RESULT = "匯入成功";
                            }
                            catch (Exception ex)
                            {
                                data.IMPORT_RESULT = "匯入失敗";
                            }
                        }
                        else
                        {
                            data.IMPORT_RESULT = "<font color=red>資料重複未上傳</font>";
                        }

                        switch (data.CHARGE)
                        {
                            case "0":
                                data.CHARGE = "0 不給付";
                                break;
                            case "1":
                                data.CHARGE = "1 給付";
                                break;
                            case "2":
                                data.CHARGE = "2 條件給付";
                                break;
                            case "3":
                                data.CHARGE = "3 醫院自行吸收";
                                break;
                            case "4":
                                data.CHARGE = "4 預設自費";
                                break;
                        }

                        list_pH_SMALL_D.Add(data);
                    }

                    DBWork.Commit();
                    session.Result.etts = list_pH_SMALL_D;
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
        public ApiResponse DetailCount(FormDataCollection form) {
            string dn = form.Get("dn");
            using (WorkSession session = new WorkSession(this)) {
                UnitOfWork DBWork = session.UnitOfWork;

                try
                {
                    BC0002Repository repo = new BC0002Repository(DBWork);
                    session.Result.afrs = repo.GetDetailCount(dn);
                }
                catch {
                    throw;
                }
                return session.Result;
            }
        }
    }
}