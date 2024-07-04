using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using WebApp.Models;
using WebApp.Repository.B;
using System.IO.Compression;
using System.IO;

using NPOI.XSSF.Model;
using NPOI.XSSF.UserModel;

using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using WebApp.Models.BD;
using System.Net;
using System.Net.Sockets;
using WebApp.Repository.UR;

namespace WebApp.Controllers.B
{
    public class BD0014Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse MasterAll(FormDataCollection form)
        {
            var wh_no = form.Get("wh_no");
            var start_date = form.Get("start_date");
            var end_date = form.Get("end_date");
            var mmcode = form.Get("mmcode");
            var agen_no = form.Get("agen_no");
            var po_status = form.Get("po_status");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0014Repository(DBWork);
                    session.Result.etts = repo.GetMasterAll(wh_no, start_date, end_date, po_status, agen_no, mmcode, page, limit, sorters);
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse MasterUpdate(MM_PO_M mmpom)
        {

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();

                try
                {
                    mmpom.UPDATE_USER = DBWork.UserInfo.UserId;
                    mmpom.UPDATE_IP = DBWork.ProcIP;

                    var repo = new BD0014Repository(DBWork);
                    session.Result.afrs = repo.MasterUpdate(mmpom);
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
        public ApiResponse MasterObsolete(FormDataCollection form)
        {

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();

                try
                {

                    BD0014Repository repo = new BD0014Repository(DBWork);
                    if (form.Get("PO_NO") != "")
                    {
                        string po_nos = form.Get("PO_NO").Substring(0, form.Get("PO_NO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] po_noTmp = po_nos.Split(',');
                        for (int i = 0; i < po_noTmp.Length; i++)
                        {
                            MM_PO_M mmpom = new MM_PO_M();
                            mmpom.PO_NO = po_noTmp[i];
                            mmpom.UPDATE_USER = DBWork.UserInfo.UserId;
                            mmpom.UPDATE_IP = DBWork.ProcIP;
                            repo.MasterObsolete(mmpom);

                            MM_PO_D mmpod = new MM_PO_D();
                            mmpod.PO_NO = po_noTmp[i];
                            mmpod.UPDATE_USER = DBWork.UserInfo.UserId;
                            mmpod.UPDATE_IP = DBWork.ProcIP;
                            repo.DetailAllObsolete(mmpod);

                            // 整份作廢前可能曾做過寄送EMAIL,若有則IS_SEND設為Y
                            repo.ObsoleteSendMailLog(po_noTmp[i], DBWork.UserInfo.UserId, DBWork.ProcIP);
                        }
                        DBWork.Commit();
                    }
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
        public ApiResponse DetailAll(FormDataCollection form)
        {
            string po_no = form.Get("p0");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();

                try
                {
                    var repo = new BD0014Repository(DBWork);
                    session.Result.etts = repo.GetDetailAll(po_no);
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse DetailUpdate(MM_PO_D mmpod)
        {

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    mmpod.UPDATE_USER = DBWork.UserInfo.UserId;
                    mmpod.UPDATE_IP = DBWork.ProcIP;

                    var repo = new BD0014Repository(DBWork);

                    // 檢查PO_QTY
                    int po_qty;
                    if (int.TryParse(mmpod.PO_QTY, out po_qty) == false)
                    {
                        session.Result.success = false;
                        session.Result.msg = "數量需為數字，請重新確認";
                        return session.Result;
                    }

                    session.Result.afrs = repo.DetailUpdate(mmpod);
                    repo.UpdateMM_PO_INREC(mmpod);
                    repo.UpdatePH_INVOICE(mmpod);
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
        public ApiResponse DetailObsolete(FormDataCollection form)
        {

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();

                try
                {

                    BD0014Repository repo = new BD0014Repository(DBWork);
                    if (form.Get("PO_NO") != "" && form.Get("MMCODE") != "")
                    {
                        string po_nos = form.Get("PO_NO");
                        string mmcodes = form.Get("MMCODE").Substring(0, form.Get("MMCODE").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] mmcodeTmp = mmcodes.Split(',');
                        for (int i = 0; i < mmcodeTmp.Length; i++)
                        {
                            MM_PO_D mmpod = new MM_PO_D();
                            mmpod.PO_NO = po_nos;
                            mmpod.MMCODE = mmcodeTmp[i];
                            mmpod.UPDATE_USER = DBWork.UserInfo.UserId;
                            mmpod.UPDATE_IP = DBWork.ProcIP;

                            repo.DetailObsolete(mmpod);
                        }
                        DBWork.Commit();
                    }
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }

        #region 2021-03-23
        [HttpPost]
        public ApiResponse CheckPoAmt(FormDataCollection form)
        {
            string po_nos = form.Get("PO_NO").Substring(0, form.Get("PO_NO").Length - 1); // 去除前端傳進來最後一個逗號
            string[] tmp = po_nos.Split(',');
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                var repo = new BD0014Repository(DBWork);

                List<MM_PO_D> details = new List<MM_PO_D>();
                for (int i = 0; i < tmp.Length; i++)
                {
                    IEnumerable<MM_PO_D> temp = repo.GetOver100K(tmp[i]);
                    details.AddRange(temp);
                }
                if (details.Any())
                {
                    session.Result.msg = "有超過15萬元品項";
                    session.Result.success = false;
                    session.Result.etts = details;
                    return session.Result;
                }

                session.Result.success = true;
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse CheckAll_PH_VENDER_had_mail(FormDataCollection form)
        {
            string agen_nos = form.Get("AGEN_NOS"); // 去除前端傳進來最後一個逗號
            string[] sa_agen_no = agen_nos.Split(',');
            String agen_nos_comma = "";
            if (sa_agen_no.Length > 0)
            {
                foreach (String agen_no in sa_agen_no)
                {
                    if (!String.IsNullOrEmpty(agen_no))
                        agen_nos_comma += "'" + agen_no + "',";
                }
                agen_nos_comma = agen_nos_comma.Substring(0, agen_nos_comma.Length - 1);
            }

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                var repo = new BD0014Repository(DBWork);
                IEnumerable<ComboItemModel> comboItemModels = repo.GetCheckAll_PH_VENDER_had_mail(agen_nos_comma);
                if (comboItemModels.Any()) // 有廠商沒mail
                {
                    String s = "";
                    s = "{";
                    foreach (ComboItemModel c in comboItemModels)
                    {
                        s += c.TEXT + ",";
                    }
                    s += "} 無EMAIL資訊 ，請維護後再寄信";
                    session.Result.msg = s;
                    session.Result.success = false;
                    session.Result.etts = comboItemModels;
                    return session.Result;
                }

                session.Result.success = true;
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse ChkSendMailAll(FormDataCollection form)
        {
            var wh_no = form.Get("wh_no");
            var start_date = form.Get("start_date");
            var end_date = form.Get("end_date");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                var repo = new BD0014Repository(DBWork);

                bool chkResult = true;
                string rtnMsg = "";

                // 列出年度累計金額超出定額者
                int chk1Cnt = 0;
                string limitAmt = repo.GetLimitAmt();
                rtnMsg += "====金額超過" + limitAmt + "院內碼====" + Environment.NewLine;
                rtnMsg += "訂單編號 | 院內碼 | 數量 | 單價 | 金額 | 年度累積金額" + Environment.NewLine;
                foreach (MM_PO_M po in repo.GetSendAll(wh_no, start_date, end_date))
                {
                    List<MM_PO_D> details = new List<MM_PO_D>();
                    IEnumerable<MM_PO_D> temp = repo.GetOver100K(po.PO_NO);
                    details.AddRange(temp);

                    if (details.Any())
                    {
                        foreach (MM_PO_D item in details)
                        {
                            rtnMsg += item.PO_NO + " | " + item.MMCODE + " | " + item.PO_QTY + " | " + item.PO_PRICE + " | " + item.PO_AMT + " | " + item.SUM_PO_AMT + Environment.NewLine;
                            chk1Cnt++;
                        }
                        chkResult = false;
                    }
                }
                if (chk1Cnt == 0)
                    rtnMsg += "(檢查無符合資料)" + Environment.NewLine;

                rtnMsg += Environment.NewLine;

                // 列出訂單廠商沒有EMAIL者
                int chk2Cnt = 0;
                rtnMsg += "====訂單廠商沒有EMAIL====" + Environment.NewLine;
                rtnMsg += "訂單編號 | 廠商代碼 | 廠商名稱" + Environment.NewLine;
                foreach (MM_PO_M po in repo.GetAgenAboutEmail(wh_no, start_date, end_date, "80", false))
                {
                    rtnMsg += po.PO_NO + " | " + po.AGEN_NO + " | " + po.AGEN_NAMEC + Environment.NewLine;
                    chk2Cnt++;
                    chkResult = false;
                }
                if (chk2Cnt == 0)
                    rtnMsg += "(檢查無符合資料)" + Environment.NewLine;

                if (chkResult == false)
                {
                    session.Result.afrs = chk2Cnt;
                    session.Result.msg = rtnMsg;
                    session.Result.success = false;
                    return session.Result;
                }

                session.Result.success = true;
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse ExportNoEmailSupplier(FormDataCollection form)
        {
            var wh_no = form.Get("wh_no");
            var start_date = form.Get("start_date");
            var end_date = form.Get("end_date");

            using (WorkSession session = new WorkSession(this))
            {
                try
                {
                    var DBWork = session.UnitOfWork;
                    var repo = new BD0014Repository(DBWork);

                    DataTable dt = new DataTable();
                    //dt.Columns.Add("訂單編號", typeof(string));
                    //dt.Columns.Add("廠商代碼", typeof(string));
                    //dt.Columns.Add("廠商名稱", typeof(string));
                    dt.Columns.Add("訂單編號");
                    dt.Columns.Add("廠商代碼");
                    dt.Columns.Add("廠商名稱");

                    foreach (MM_PO_M po in repo.GetAgenAboutEmail(wh_no, start_date, end_date, "80", false))
                    {
                        var NewRow = dt.NewRow();
                        NewRow[0] = string.IsNullOrEmpty(po.PO_NO) ? string.Empty : po.PO_NO.ToString();
                        NewRow[1] = string.IsNullOrEmpty(po.AGEN_NO) ? string.Empty : po.AGEN_NO.ToString();
                        NewRow[2] = string.IsNullOrEmpty(po.AGEN_NAMEC) ? string.Empty : po.AGEN_NAMEC.ToString();
                        dt.Rows.Add(NewRow);
                    }
                    JCLib.Excel.Export("BD0014_無Email訂單廠商"+DateTime.Now.ToShortDateString()+".xls", dt);

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }

        }



        public ApiResponse ExportDetailByExcel(FormDataCollection form)
        {
            string start_date = form.Get("start_date");
            string end_date = form.Get("end_date");
            string wh_no = form.Get("wh_no"); // wh_no 庫房代碼
            string po_nos = form.Get("po_no").Substring(0, form.Get("po_no").Length - 1); // , 訂單編號去除前端傳進來最後一個逗號
            string agen_no = form.Get("agen_no");
            string mmcode = form.Get("mmcode"); // mmcode 院內碼 08021274
            string po_status = form.Get("po_status"); // 訂單狀態 [無], PO_STATUS = T1Query.getForm().findField('P2').getValue(); // 訂單狀態', [''全部|'0'未取消訂單|'D'取消訂單]
            string easyname = form.Get("easyname");
            string[] tmp = po_nos.Split(',');
            string filename = "";
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                var repo = new BD0014Repository(DBWork);

                List<MM_PO_D> details = new List<MM_PO_D>();

                DataTable dtItems = new DataTable();
                dtItems.Columns.Add("項次", typeof(int));
                dtItems.Columns["項次"].AutoIncrement = true;
                dtItems.Columns["項次"].AutoIncrementSeed = 1;
                dtItems.Columns["項次"].AutoIncrementStep = 1;

                DataTable result = null;

                //dtItems.Merge(result);

                for (int i = 0; i < tmp.Length; i++)
                {
                    result = repo.GetExportDetailByExcel(
                        start_date,
                        end_date,
                        wh_no,
                        tmp[i], // po_no,
                        agen_no,
                        mmcode,
                        po_status
                    ); //  GetOver100KExcel
                    dtItems.Merge(result);
                }

                if (agen_no != "")
                {
                    filename = agen_no + easyname + DateTime.Now.ToString("yyyyMMddHHmmss");
                }
                else {
                    filename = agen_no  + DateTime.Now.ToString("yyyyMMddHHmmss");
                }

                JCLib.Excel.Export(string.Format("訂單明細_{0}.xls", filename), dtItems);

                session.Result.success = true;
                return session.Result;
            }
        }


        [HttpPost]
        public ApiResponse ExportDetailByZip(FormDataCollection form)
        {
            string wh_no = form.Get("wh_no"); // wh_no 庫房代碼
            string start_ym = form.Get("start_date");
            string end_ym = form.Get("end_date");
            string po_no = form.Get("po_no"); // 訂單編號集
            string mmcode = form.Get("mmcode"); // mmcode 院內碼 08021274
            string agen_no = form.Get("agen_no"); // agen_no 廠商代碼
            string po_status = form.Get("po_status"); // 訂單狀態 [無], PO_STATUS = T1Query.getForm().findField('P2').getValue(); // 訂單狀態', [''全部|'0'未取消訂單|'D'取消訂單]
            string agenno = "";
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    BD0014Repository repo = new BD0014Repository(DBWork);
                    List<String> lstFilePaths = new List<String>();

                    String filePath = "";
                    IEnumerable<BD0014M> orders;
                    String[] po_nos = po_no.Split(',');
                    foreach (String s_po_no in po_nos)
                    {
                        if (!String.IsNullOrEmpty(s_po_no))
                        {
                            agenno = repo.GetAgenNo(s_po_no);
                            //if (HttpContext.Current != null)
                            //string tempFilePath = Path.Combine(HttpContext.Current.Server.MapPath("~/App_Data"), s_po_no + ".xlsx");
                            //File.WriteAllText(tempFilePath, "Hello, this is a temporary file content.");
                            String App_Data_Dir_path = HttpContext.Current.Server.MapPath("~/App_Data");
                            if (!Directory.Exists(App_Data_Dir_path))
                                Directory.CreateDirectory(App_Data_Dir_path);
                            filePath = Path.Combine(HttpContext.Current.Server.MapPath("~/App_Data"), agenno + "_" + System.DateTime.Now.ToString("yyyyMMdd") + ".xlsx"); // Path.GetFullPath(s_po_no + ".xlsx"); // @"D:\wwwroot\InadArea\" + System.DateTime.Now.ToString("yyyyMMdd") + "_tsghmm_BD0014_a_" + po_no + ".xls";
                            orders = repo.Get壓縮檔的單筆訂單(s_po_no);
                            if (orders.Any()) // 廢單不顯示
                            {
                                產生Excel報表(orders, filePath);
                                lstFilePaths.Add(filePath);
                            }
                        } // end of 
                    }
                    JCLib.Excel.ExportZipSpecFormat("訂單明細_" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".zip", lstFilePaths);
                    foreach (String fp in lstFilePaths)
                    {
                        if (File.Exists(fp))
                            File.Delete(fp);
                    }
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse Over150KExcel(FormDataCollection form)
        {
            string po_nos = form.Get("PO_NO").Substring(0, form.Get("PO_NO").Length - 1); // 去除前端傳進來最後一個逗號
            string[] tmp = po_nos.Split(',');
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BD0014Repository(DBWork);

                    List<MM_PO_D> details = new List<MM_PO_D>();

                    DataTable dtItems = new DataTable();
                    dtItems.Columns.Add("項次", typeof(int));
                    dtItems.Columns["項次"].AutoIncrement = true;
                    dtItems.Columns["項次"].AutoIncrementSeed = 1;
                    dtItems.Columns["項次"].AutoIncrementStep = 1;

                    DataTable result = null;
                    for (int i = 0; i < tmp.Length; i++)
                    {
                        result = repo.GetOver150KExcel(tmp[i]);
                        dtItems.Merge(result);

                        MM_PO_M m = new MM_PO_M();
                        m.PO_NO = tmp[i];
                        m.PO_STATUS = "82"; // 訂單狀態; 80-開單,82-已傳MAIL,83-已傳真(待轉檔) ,84-待傳MAIL, 85-已傳真 , 87-作廢 , 88-補寄MAIL-for 藥品
                        m.UPDATE_USER = DBWork.UserInfo.UserId;
                        m.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.UPD_MM_PO_M_by_GetOver150KExcel(m);
                    }

                    JCLib.Excel.Export(string.Format("零購超過十五萬元清單_{0}.xls", DateTime.Now.ToString("yyyyMMddHHmmss")), dtItems);
                    DBWork.Commit();
                }
                catch (Exception ex)
                {
                    DBWork.Rollback();
                    throw;
                }
                session.Result.success = true;
                return session.Result;
            }
        }
        #endregion


        [HttpPost]
        public ApiResponse SendEmail(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();

                try
                {
                    BD0014Repository repo = new BD0014Repository(DBWork);

                    if (repo.CheckEmailExists(DBWork.UserInfo.UserId) == "N")
                    {
                        session.Result.success = false;
                        session.Result.msg = "登入人員無EMAIL，請先維護後再寄送";
                        return session.Result;
                    }

                    if (form.Get("PO_NO") != "")
                    {
                        string po_nos = form.Get("PO_NO").Substring(0, form.Get("PO_NO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = po_nos.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            MM_PO_M mmpom = new MM_PO_M();
                            mmpom.PO_NO = tmp[i];
                            mmpom.PO_STATUS = "84";
                            mmpom.UPDATE_USER = DBWork.UserInfo.UserId;
                            mmpom.UPDATE_IP = DBWork.ProcIP;

                            session.Result.afrs = repo.SendEmail(mmpom);

                            // 2023-08-24 新增至SEND_MAIL_LOG
                            session.Result.afrs = repo.InsertSendMailLog(tmp[i], DBWork.UserInfo.UserId, DBWork.ProcIP);
                        }
                        DBWork.Commit();
                    }
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
        public ApiResponse SendMailAll(FormDataCollection form)
        {
            var wh_no = form.Get("wh_no");
            var start_date = form.Get("start_date");
            var end_date = form.Get("end_date");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();

                try
                {
                    BD0014Repository repo = new BD0014Repository(DBWork);
                    if (repo.CheckEmailExists(DBWork.UserInfo.UserId) == "N")
                    {
                        session.Result.success = false;
                        session.Result.msg = "登入人員無EMAIL，請先維護後再寄送";
                        return session.Result;
                    }
                    int procCnt = 0;
                    // 排除廠商沒有EMAIL的訂單進行寄信動作
                    foreach (MM_PO_M po in repo.GetAgenAboutEmail(wh_no, start_date, end_date, "80", true))
                    {
                        MM_PO_M mmpom = new MM_PO_M();
                        mmpom.PO_NO = po.PO_NO;
                        mmpom.PO_STATUS = "84";
                        mmpom.UPDATE_USER = DBWork.UserInfo.UserId;
                        mmpom.UPDATE_IP = DBWork.ProcIP;

                        session.Result.afrs = repo.SendEmail(mmpom);

                        // 新增至SEND_MAIL_LOG
                        session.Result.afrs = repo.InsertSendMailLog(po.PO_NO, DBWork.UserInfo.UserId, DBWork.ProcIP);

                        procCnt++;
                    }
                    DBWork.Commit();

                    session.Result.msg = procCnt.ToString();
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
        public ApiResponse ReSendEmail(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();

                try
                {
                    BD0014Repository repo = new BD0014Repository(DBWork);
                    if (form.Get("PO_NO") != "")
                    {
                        string po_nos = form.Get("PO_NO").Substring(0, form.Get("PO_NO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = po_nos.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            MM_PO_M mmpom = new MM_PO_M();
                            mmpom.PO_NO = tmp[i];
                            mmpom.UPDATE_USER = DBWork.UserInfo.UserId;
                            mmpom.UPDATE_IP = DBWork.ProcIP;

                            session.Result.afrs = repo.ReSendEmail(mmpom);

                            // 新增至SEND_MAIL_LOG
                            session.Result.afrs = repo.InsertSendMailLog(mmpom.PO_NO, DBWork.UserInfo.UserId, DBWork.ProcIP);
                        }
                        DBWork.Commit();
                    }
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
        public ApiResponse SetFax(FormDataCollection form)
        {


            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();

                try
                {
                    BD0014Repository repo = new BD0014Repository(DBWork);
                    if (form.Get("PO_NO") != "")
                    {
                        string po_nos = form.Get("PO_NO").Substring(0, form.Get("PO_NO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = po_nos.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            MM_PO_M mmpom = new MM_PO_M();
                            mmpom.PO_NO = tmp[i];
                            mmpom.UPDATE_USER = DBWork.UserInfo.UserId;
                            mmpom.UPDATE_IP = DBWork.ProcIP;

                            session.Result.afrs = repo.SetFax(mmpom);
                        }
                        DBWork.Commit();
                    }
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    throw;
                }

                return session.Result;
            }
        }

        public ApiResponse GetWH_NO()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BD0014Repository repo = new BD0014Repository(DBWork);
                    session.Result.etts = repo.GetWH_NO(DBWork.UserInfo.UserId);
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetAgenCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BD0014Repository repo = new BD0014Repository(DBWork);
                    session.Result.etts = repo.GetAgenCombo();
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetMmcodeCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BD0014Repository repo = new BD0014Repository(DBWork);
                    session.Result.etts = repo.GetMmcodeCombo();
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetMatClassCombo(FormDataCollection form)
        {

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BD0014Repository repo = new BD0014Repository(DBWork);
                    session.Result.etts = repo.GetMatClassCombo(DBWork.UserInfo.UserId);
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetMatClassTextArea(FormDataCollection form)
        {
            var mat_class = form.Get("mat_class"); // data_name='MAIL_CONTENT_' + mat_class // MI_MAST.MAT_CLASS 物料分類代碼(01-藥品,02-衛材(含檢材),03-文具,04-清潔用品,05-表格,06-防護用具,07-被服,08-資訊耗材,09-氣體, 13-中藥)

            // 物料分類()
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BD0014Repository repo = new BD0014Repository(DBWork);
                    session.Result.etts = repo.GetMatClassTextArea(
                        mat_class,
                        DBWork.UserInfo.UserId
                    );
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetStatusCombo(FormDataCollection form) {
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0014Repository(DBWork);
                    string hospCode = repo.getHospCode();
                    session.Result.etts = repo.GetStatusCombo(hospCode);
                    return session.Result;
                }
                catch (Exception e) {
                    throw;
                }
            }
        }

        [HttpPost]
        public ApiResponse GetMemoCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0014Repository(DBWork);
                    session.Result.etts = repo.GetMemoCombo();
                    return session.Result;
                }
                catch (Exception e)
                {
                    throw;
                }
            }
        }

        [HttpPost]
        public ApiResponse Get壓縮檔的單筆訂單(FormDataCollection form)
        {
            var po_no = form.Get("po_no");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {

                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        } // 




        public DataTable GetMailBasicData(string po_no)
        {
            string msg_oracle = "error";
            //CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
            DataTable dt_oralce = new DataTable();
            //string sql_oracle = " select a.PO_NO, a.M_CONTID, a.AGEN_NO, c.AGEN_NAMEC, c.EMAIL, a.MAT_CLASS, ";
            //sql_oracle += "              a.MEMO, a.SMEMO, a.UPDATE_USER, a.ISCOPY, sum(floor(b.PO_PRICE*b.PO_QTY))  as AMOUNT ";
            //sql_oracle += "         from MM_PO_M a, MM_PO_D b, PH_VENDER c ";
            //sql_oracle += "        where a.PO_NO=b.PO_NO and a.AGEN_NO=c.AGEN_NO and a.PO_NO=:po_no and b.STATUS<>'D' ";
            //sql_oracle += "        group by a.PO_NO,a.M_CONTID,a.AGEN_NO,c.AGEN_NAMEC,c.EMAIL, a.MAT_CLASS, a.MEMO,a.SMEMO, a.UPDATE_USER,a.ISCOPY ";
            //List<CallDBtools_Oracle.OracleParam> paraListA = new List<CallDBtools_Oracle.OracleParam>();
            //paraListA.Add(new CallDBtools_Oracle.OracleParam(1, ":po_no", "VarChar", po_no));
            //dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, paraListA, "oracle", "T1", ref msg_oracle);
            //if (msg_oracle != "")
            //    callDbtools_oralce.I_ERROR_LOG("BDS008", "(衛材)(藥材)STEP2-取得信件基本資料失敗:" + msg_oracle, "AUTO");

            dt_oralce.Columns.Add(new DataColumn("PO_NO"));
            dt_oralce.Columns.Add(new DataColumn("M_CONTID"));
            dt_oralce.Columns.Add(new DataColumn("AGEN_NO"));
            dt_oralce.Columns.Add(new DataColumn("AGEN_NAMEC"));
            dt_oralce.Columns.Add(new DataColumn("EMAIL"));
            dt_oralce.Columns.Add(new DataColumn("MAT_CLASS"));
            dt_oralce.Columns.Add(new DataColumn("MEMO"));
            dt_oralce.Columns.Add(new DataColumn("SMEMO"));
            dt_oralce.Columns.Add(new DataColumn("UPDATE_USER"));
            dt_oralce.Columns.Add(new DataColumn("ISCOPY"));
            dt_oralce.Columns.Add(new DataColumn("AMOUNT"));
            DataRow dr = dt_oralce.NewRow();
            dr["PO_NO"] = "PO_NO";
            dr["M_CONTID"] = "M_CONTID";
            dr["AGEN_NO"] = "AGEN_NO";
            dr["AGEN_NAMEC"] = "AGEN_NAMEC";
            dr["EMAIL"] = "EMAIL";
            dr["MAT_CLASS"] = "MAT_CLASS";
            dr["MEMO"] = "MEMO";
            dr["SMEMO"] = "SMEMO";
            dr["UPDATE_USER"] = "UPDATE_USER";
            dr["ISCOPY"] = "ISCOPY";
            dr["AMOUNT"] = "AMOUNT";
            dt_oralce.Rows.Add(dr);




            return dt_oralce;
        } // 
        //撈出信件訂單內容(藥材)
        public DataTable GetMailDt1_MatClassEqual_01(string po_no)
        {
            string msg_oracle = "error";
            //CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
            DataTable dt_oralce = new DataTable();
            //string sql_oracle = " select rownum 項次, b.MMCODE as 藥材代碼,b.MMNAME_C as 藥材名稱, a.M_PURUN as 出貨單位, ";
            //sql_oracle += "               PO_QTY as 數量,  PO_PRICE as 單價, b.E_ORDERUNIT as 單位, ";
            //sql_oracle += "               round(a.PO_QTY * a.PO_PRICE) as 小計,  ";
            //sql_oracle += "               (case when b.E_SOURCECODE = 'P' then '買斷' when b.E_SOURCECODE = 'C' then '寄庫' else '' end) as 買斷寄庫,  ";
            //sql_oracle += "               (select SELF_CONT_EDATE from MED_SELFPUR_DEF where MMCODE = a.MMCODE and twn_date(sysdate) >= SELF_CONT_BDATE and twn_date(sysdate) <= SELF_CONT_EDATE and rownum = 1) as 合約到期日,  ";
            //sql_oracle += "               (select SELF_CONTRACT_NO from MED_SELFPUR_DEF where MMCODE = a.MMCODE and twn_date(sysdate) >= SELF_CONT_BDATE and twn_date(sysdate) <= SELF_CONT_EDATE and rownum = 1) as 合約案號, ";
            //sql_oracle += "               '' as 分批交貨日期, a.PO_NO as 進貨單號, ";
            //sql_oracle += "               a.MEMO as 備註 ";
            //sql_oracle += "         from MM_PO_D a,MI_MAST b ";
            //sql_oracle += "        where a.MMCODE=b.MMCODE and a.PO_NO=:po_no and a.STATUS<>'D'";
            //sql_oracle += "        order by b.MMNAME_E ";
            //List<CallDBtools_Oracle.OracleParam> paraListA = new List<CallDBtools_Oracle.OracleParam>();
            //paraListA.Add(new CallDBtools_Oracle.OracleParam(1, ":po_no", "VarChar", po_no));
            //dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, paraListA, "oracle", "T1", ref msg_oracle);
            //if (msg_oracle != "")
            //    callDbtools_oralce.I_ERROR_LOG("BDS008", "(藥材)STEP3-取得(藥材)信件訂單內容失敗:" + msg_oracle, "AUTO");

            dt_oralce.Columns.Add(new DataColumn("項次"));
            dt_oralce.Columns.Add(new DataColumn("藥材代碼"));
            dt_oralce.Columns.Add(new DataColumn("藥材名稱"));
            dt_oralce.Columns.Add(new DataColumn("出貨單位"));
            dt_oralce.Columns.Add(new DataColumn("數量"));
            dt_oralce.Columns.Add(new DataColumn("單價"));
            dt_oralce.Columns.Add(new DataColumn("單位"));
            dt_oralce.Columns.Add(new DataColumn("小計"));
            dt_oralce.Columns.Add(new DataColumn("買斷寄庫"));
            dt_oralce.Columns.Add(new DataColumn("合約到期日"));
            dt_oralce.Columns.Add(new DataColumn("合約案號"));
            dt_oralce.Columns.Add(new DataColumn("分批交貨日期"));
            dt_oralce.Columns.Add(new DataColumn("進貨單號"));
            dt_oralce.Columns.Add(new DataColumn("備註"));
            DataRow dr = dt_oralce.NewRow();
            dr["項次"] = "項次";
            dr["藥材代碼"] = "藥材代碼";
            dr["藥材名稱"] = "藥材名稱";
            dr["出貨單位"] = "出貨單位";
            dr["數量"] = "數量";
            dr["單價"] = "單價";
            dr["單位"] = "單位";
            dr["小計"] = "小計";
            dr["買斷寄庫"] = "買斷寄庫";
            dr["合約到期日"] = "合約到期日";
            dr["合約案號"] = "合約案號";
            dr["分批交貨日期"] = "分批交貨日期";
            dr["進貨單號"] = "進貨單號";
            dr["備註"] = "備註";
            dt_oralce.Rows.Add(dr);
            return dt_oralce;
        } // 
        // 取得醫院名稱
        public string GetHospName()
        {
            string msg_oracle = "error";
            //CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
            string str_oracle = "";
            //string sql_oracle = @" SELECT data_value FROM PARAM_D WHERE grp_code = 'HOSP_INFO' AND data_name = 'HospName' ";
            //str_oracle = callDbtools_oralce.CallExecScalar(sql_oracle, null, "oracle", ref msg_oracle);
            //if (msg_oracle != "")
            //{
            //    callDbtools_oralce.I_ERROR_LOG("BDS008", "取得醫院名稱資料失敗:" + msg_oracle, "AUTO");
            //}
            str_oracle = System.DateTime.Now.ToString("yyyyMMddHHmmss");
            return str_oracle;
        } // 

        public void 產生Excel報表(
            IEnumerable<BD0014M> orders,
            String fileName
        )
        {
            string fName = "";
            using (WorkSession session = new WorkSession())
            {
                // 因應Path Manipulation 問題
                var DBWork = session.UnitOfWork;
                var repo = new UR_IDRepository(DBWork);
                fName = repo.CheckValidString(fileName);
            }

            //IEnumerable<BD0014M> orders = repo.Get壓縮檔的單筆訂單(po_no); ;
            BD0014M m = orders.FirstOrDefault();
            DataTable dtM = new DataTable();
            dtM = GetMailBasicData(""); // dt_oralce.Rows[i]["PO_NO"].ToString().Trim()

            DataTable dtD = new DataTable();
            dtD = GetMailDt1_MatClassEqual_01(""); // dt_oralce.Rows[i]["PO_NO"].ToString().Trim()

            //if (dtM == null || dtD == null) return;

            IWorkbook workbook = new XSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("Sheet1");
            sheet.SetColumnWidth(0, 10 * 256); // 項次
            sheet.SetColumnWidth(1, 13 * 256); // 藥材代碼
            sheet.SetColumnWidth(2, 30 * 256); // 藥材名稱
            sheet.SetColumnWidth(3, 13 * 256); // 出貨單位
            sheet.SetColumnWidth(4, 10 * 256); // 數量
            sheet.SetColumnWidth(5, 10 * 256); // 單價
            sheet.SetColumnWidth(6, 10 * 256); // 單位
            sheet.SetColumnWidth(7, 10 * 256); // 小計
            sheet.SetColumnWidth(8, 13 * 256); // 買斷寄庫
            sheet.SetColumnWidth(9, 15 * 256); // 合約到期日
            sheet.SetColumnWidth(10, 13 * 256); // 合約案號
            sheet.SetColumnWidth(11, 19 * 256); // 分批交貨日期
            sheet.SetColumnWidth(12, 20 * 256); // 進貨單號
            sheet.SetColumnWidth(13, 10 * 256); // 備註

            // 文件標題
            ICellStyle titleStyle = workbook.CreateCellStyle();
            IFont titlefont = workbook.CreateFont();
            titlefont.IsBold = true;
            titlefont.FontHeightInPoints = 22;
            titleStyle.SetFont(titlefont);

            // 表格標題
            ICellStyle tableStyle = workbook.CreateCellStyle();
            IFont tablefont = workbook.CreateFont();
            tablefont.IsBold = true;
            tablefont.FontHeightInPoints = 14;
            tableStyle.SetFont(tablefont);

            // 內容
            ICellStyle contentStyle = workbook.CreateCellStyle();
            IFont contentfont = workbook.CreateFont();
            contentfont.FontHeightInPoints = 12;
            contentStyle.SetFont(contentfont);

            // row 0
            IRow exr = sheet.CreateRow(0);
            ICell exc = exr.CreateCell(2);
            exc.SetCellValue(m.送貨地點+" 訂購單(編號：" + m.訂單編號 + ")");
            exc.CellStyle = titleStyle;

            // row 2
            IRow exr2 = sheet.CreateRow(2);
            ICell exc2_0 = exr2.CreateCell(0);
            exc2_0.SetCellValue("訂購日期：" + m.訂購日期.Substring(0, 3) + "年" + m.訂購日期.Substring(3, 2) + "月" + m.訂購日期.Substring(5, 2) + "日");
            exc2_0.CellStyle = contentStyle;

            ICell exc2_7 = exr2.CreateCell(7);
            exc2_7.SetCellValue("廠商：" + m.廠商);
            exc2_7.CellStyle = contentStyle;

            // row 3
            IRow exr3 = sheet.CreateRow(3);
            ICell exc3_0 = exr3.CreateCell(0);
            exc3_0.SetCellValue("訂購方式：電子郵件(" + m.EMAIL + ")");
            exc3_0.CellStyle = contentStyle;

            ICell exc3_7 = exr3.CreateCell(7);
            exc3_7.SetCellValue("電話：" + m.電話);
            exc3_7.CellStyle = contentStyle;

            ICell exc3_10 = exr3.CreateCell(10);
            exc3_10.SetCellValue("傳真：" + m.傳真);
            exc3_10.CellStyle = contentStyle;

            // row 4
            IRow exr4 = sheet.CreateRow(4);
            ICell exc4_0 = exr4.CreateCell(0);
            exc4_0.SetCellValue("備註：" + m.備註);
            exc4_0.CellStyle = contentStyle;

            // row 5
            IRow exr5 = sheet.CreateRow(5);
            ICell exc5_0 = exr5.CreateCell(0);
            exc5_0.SetCellValue("送貨地點："+m.送貨地點);

            // row 6
            IRow exr6 = sheet.CreateRow(6);
            ICell exc6_0 = exr6.CreateCell(0);
            exc6_0.SetCellValue("連絡電話");

            // row 7
            IRow exr7 = sheet.CreateRow(7);
            ICell exc7_0 = exr7.CreateCell(0);
            exc7_0.SetCellValue("列印日期：" + (int.Parse(System.DateTime.Now.ToString("yyyy")) - 1911) + "年" + int.Parse(DateTime.Now.ToString("MM")) + "月" + int.Parse(DateTime.Now.ToString("dd")) + "日");
            exc7_0.CellStyle = contentStyle;

            // row 8
            IRow exr8 = sheet.CreateRow(8);
            ICell exc8_0 = exr8.CreateCell(0);
            exc8_0.SetCellValue("");
            exc8_0.CellStyle = contentStyle;

            // row 10
            IRow exr10 = sheet.CreateRow(10);
            ICell exc10_0 = exr10.CreateCell(0);
            exc10_0.SetCellValue("");
            exc10_0.CellStyle = contentStyle;

            // row 12
            IRow exr12 = sheet.CreateRow(12);
            int ci = 0;
            ICell excTT;
            excTT = exr12.CreateCell(ci++); excTT.CellStyle = contentStyle; excTT.SetCellValue("訂單編號");
            excTT = exr12.CreateCell(ci++); excTT.CellStyle = contentStyle; excTT.SetCellValue("訂購日期");
            //excTT = exr12.CreateCell(ci++); excTT.CellStyle = contentStyle; excTT.SetCellValue("備註");
            excTT = exr12.CreateCell(ci++); excTT.CellStyle = contentStyle; excTT.SetCellValue("廠商");
            excTT = exr12.CreateCell(ci++); excTT.CellStyle = contentStyle; excTT.SetCellValue("電話");
            excTT = exr12.CreateCell(ci++); excTT.CellStyle = contentStyle; excTT.SetCellValue("傳真");
            excTT = exr12.CreateCell(ci++); excTT.CellStyle = contentStyle; excTT.SetCellValue("送貨地點");
            excTT = exr12.CreateCell(ci++); excTT.CellStyle = contentStyle; excTT.SetCellValue("列印日期");
            excTT = exr12.CreateCell(ci++); excTT.CellStyle = contentStyle; excTT.SetCellValue("藥材代碼");
            excTT = exr12.CreateCell(ci++); excTT.CellStyle = contentStyle; excTT.SetCellValue("藥材名稱");
            excTT = exr12.CreateCell(ci++); excTT.CellStyle = contentStyle; excTT.SetCellValue("出貨單位");
            excTT = exr12.CreateCell(ci++); excTT.CellStyle = contentStyle; excTT.SetCellValue("單位");
            excTT = exr12.CreateCell(ci++); excTT.CellStyle = contentStyle; excTT.SetCellValue("單價");
            excTT = exr12.CreateCell(ci++); excTT.CellStyle = contentStyle; excTT.SetCellValue("小計");
            excTT = exr12.CreateCell(ci++); excTT.CellStyle = contentStyle; excTT.SetCellValue("買斷寄庫");
            excTT = exr12.CreateCell(ci++); excTT.CellStyle = contentStyle; excTT.SetCellValue("合約到期日");
            excTT = exr12.CreateCell(ci++); excTT.CellStyle = contentStyle; excTT.SetCellValue("合約案號");
            excTT = exr12.CreateCell(ci++); excTT.CellStyle = contentStyle; excTT.SetCellValue("病人姓名");
            excTT = exr12.CreateCell(ci++); excTT.CellStyle = contentStyle; excTT.SetCellValue("病人病歷號");
            excTT = exr12.CreateCell(ci++); excTT.CellStyle = contentStyle; excTT.SetCellValue("備註");
            excTT = exr12.CreateCell(ci++); excTT.CellStyle = contentStyle; excTT.SetCellValue("分批交貨日期");

            //int ci = 0;
            //foreach (DataColumn dc in dtD.Columns)
            //{
            //    ICell excTT = exr12.CreateCell(ci);
            //    excTT.SetCellValue(dc.ColumnName);
            //    excTT.CellStyle = tableStyle;
            //    ci++;
            //}
            //ci = 0;

            // row 13+
            int ri_base_idx = 13;
            int ri = 0;
            Decimal 總價 = 0;
            foreach (BD0014M d in orders)
            {
                IRow exr13 = sheet.CreateRow(ri_base_idx + ri); // 13
                ci = 0;
                excTT = exr13.CreateCell(ci++); excTT.CellStyle = contentStyle; excTT.SetCellValue(d.訂單編號);
                excTT = exr13.CreateCell(ci++); excTT.CellStyle = contentStyle; excTT.SetCellValue(d.訂購日期);
                //excTT = exr13.CreateCell(ci++); excTT.CellStyle = contentStyle; excTT.SetCellValue(d.備註);
                excTT = exr13.CreateCell(ci++); excTT.CellStyle = contentStyle; excTT.SetCellValue(d.廠商);
                excTT = exr13.CreateCell(ci++); excTT.CellStyle = contentStyle; excTT.SetCellValue(d.電話);
                excTT = exr13.CreateCell(ci++); excTT.CellStyle = contentStyle; excTT.SetCellValue(d.傳真);
                excTT = exr13.CreateCell(ci++); excTT.CellStyle = contentStyle; excTT.SetCellValue(d.送貨地點);
                excTT = exr13.CreateCell(ci++); excTT.CellStyle = contentStyle; excTT.SetCellValue(d.列印日期);
                excTT = exr13.CreateCell(ci++); excTT.CellStyle = contentStyle; excTT.SetCellValue(d.藥材代碼);
                excTT = exr13.CreateCell(ci++); excTT.CellStyle = contentStyle; excTT.SetCellValue(d.藥材名稱);
                excTT = exr13.CreateCell(ci++); excTT.CellStyle = contentStyle; excTT.SetCellValue(d.出貨單位);
                excTT = exr13.CreateCell(ci++); excTT.CellStyle = contentStyle; excTT.SetCellValue(d.單位);
                excTT = exr13.CreateCell(ci++); excTT.CellStyle = contentStyle; excTT.SetCellValue(d.單價);
                excTT = exr13.CreateCell(ci++); excTT.CellStyle = contentStyle; excTT.SetCellValue(d.小計);
                excTT = exr13.CreateCell(ci++); excTT.CellStyle = contentStyle; excTT.SetCellValue(d.買斷寄庫);
                excTT = exr13.CreateCell(ci++); excTT.CellStyle = contentStyle; excTT.SetCellValue(d.合約到期日);
                excTT = exr13.CreateCell(ci++); excTT.CellStyle = contentStyle; excTT.SetCellValue(d.合約案號);
                excTT = exr13.CreateCell(ci++); excTT.CellStyle = contentStyle; excTT.SetCellValue(d.病人姓名);
                excTT = exr13.CreateCell(ci++); excTT.CellStyle = contentStyle; excTT.SetCellValue(d.病人病歷號);
                excTT = exr13.CreateCell(ci++); excTT.CellStyle = contentStyle; excTT.SetCellValue(d.明細備註);
                excTT = exr13.CreateCell(ci++); excTT.CellStyle = contentStyle; excTT.SetCellValue(d.分批交貨日期);
                Decimal 小計 = 0;
                if (decimal.TryParse(d.小計, out 小計))
                    總價 += 小計;
                ri++;
            }


            //int ri = 0;
            //foreach (DataRow dr in dtD.Rows)
            //{
            //    IRow exrTbr = sheet.CreateRow(13 + ri);
            //    foreach (object v in dr.ItemArray)
            //    {
            //        ICell excTbc = exrTbr.CreateCell(ci);
            //        excTbc.SetCellValue(v.ToString());
            //        excTbc.CellStyle = contentStyle;
            //        ci++;
            //    }
            //    ri++;
            //    ci = 0;
            //}

            // row 總價
            IRow exrS = sheet.CreateRow(13 + ri);
            //ICell excS_0 = exrS.CreateCell(0);
            //excS_0.SetCellValue("總價：");
            //excS_0.CellStyle = contentStyle;
            ICell excS_7 = exrS.CreateCell(7);
            excS_7.SetCellValue("總價：" + 總價);
            excS_7.CellStyle = contentStyle;

            // row last
            IRow exrL = sheet.CreateRow(15 + ri);
            ICell excL_0 = exrL.CreateCell(0);
            excL_0.SetCellValue("請注意：當有指定[分批交貨日期]時，請準時於指定日期交貨");
            excL_0.CellStyle = contentStyle;


            //String fileName = @"D:\wwwroot\InadArea\" + System.DateTime.Now.ToString("yyyyMMdd") + "_tsghmm_BD0014_Controller.xls";
            FileStream fs = new FileStream(fName, FileMode.Create, FileAccess.Write);
            workbook.Write(fs);
            fs.Dispose();
            workbook.Close();
        } // 

        [HttpPost]
        public ApiResponse GetCreateEmailAttFile(FormDataCollection form) // 參Schedule\BDS008
        {
            var po_no = form.Get("po_no");
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BD0014Repository repo = new BD0014Repository(DBWork);
                    String App_Data_Dir_path = HttpContext.Current.Server.MapPath("~/App_Data");
                    if (!Directory.Exists(App_Data_Dir_path))
                        Directory.CreateDirectory(App_Data_Dir_path);
                    String filePath = Path.Combine(HttpContext.Current.Server.MapPath("~/App_Data"), "明細_" + System.DateTime.Now.ToString("yyyyMMdd") + ".xlsx"); // Path.GetFullPath(s_po_no + ".xlsx"); // @"D:\wwwroot\InadArea\" + System.DateTime.Now.ToString("yyyyMMdd") + "_tsghmm_BD0014_a_" + po_no + ".xls";
                    產生Excel報表(repo.Get壓縮檔的單筆訂單(po_no), filePath);
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        } // 



        public ApiResponse CheckNotD(FormDataCollection form)
        {
            List<string> po_noList = new List<string>();

            if (form.Count() > 0)
            {
                foreach (var item in form)
                {
                    po_noList.Add(item.Value);
                }
            }

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0014Repository(DBWork);

                    if (po_noList.Count == 1)
                    {
                        int notD = repo.GetNotD(po_noList[0]);

                        if (notD == 0)
                        {
                            session.Result.success = false;
                            session.Result.msg = po_noList[0];
                            return session.Result;
                        }
                    }
                    else if(po_noList.Count>1)
                    {
                        List<string> noPassList = new List<string>();

                        foreach (string po_no in po_noList)
                        {
                            int notD = repo.GetNotD(po_no);
                            if (notD == 0)
                            {
                                noPassList.Add(po_no);
                            }
                        }

                        if (noPassList.Count>0)
                        {
                            string noPassStr = "";
                            bool isFirst = true;
                            foreach (string p in noPassList)
                            {
                                if (isFirst)
                                {
                                    noPassStr = p;
                                    isFirst = false;
                                }
                                else
                                {
                                    noPassStr += ","+p;
                                }
                            }

                            session.Result.success = false;
                            session.Result.msg = noPassStr;
                            return session.Result;
                        }

                    }
                    session.Result.success = true;
                    return session.Result;


                }
                catch (Exception e)
                {
                    throw;
                }
            }
        } // 

        [HttpPost]
        public ApiResponse Set信件內容維護(FormDataCollection form) // 原參考public ApiResponse MasterUpdate(MM_PO_M mmpom)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BD0014M o = new BD0014M();
                    o.DATA_NAME = "MAIL_CONTENT_" + form.Get("MAT_CLASS"); // data_name = (case when 所選物料類別 = '01' then 'MAIL_CONTENT_01' else 'MAIL_CONTENT_02' end)
                    o.DATA_VALUE = form.Get("DATA_VALUE");
                    o.DATA_REMARK = DBWork.UserInfo.UserId; // 登入人員帳號
                    var repo = new BD0014Repository(DBWork);
                    session.Result.afrs = repo.set信件內容維護(o);
                    DBWork.Commit();
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        } // 

        [HttpPost]
        public ApiResponse ChkHospCode(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                var repo = new BD0014Repository(DBWork);
                session.Result.msg = repo.getHospCode();
                session.Result.success = true;
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse ChkAgenCnt(FormDataCollection form)
        {
            string po_no = form.Get("po_no").Substring(0, form.Get("PO_NO").Length - 1);
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                var repo = new BD0014Repository(DBWork);
                session.Result.msg = repo.ChkAgenCnt(po_no);
                session.Result.success = true;
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse UpdateStatusOnFax(FormDataCollection form)
        {
            string po_no = form.Get("po_no").Substring(0, form.Get("PO_NO").Length - 1);
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                var repo = new BD0014Repository(DBWork);
                try
                {
                    repo.UpdateStatusOnFax(po_no, DBWork.UserInfo.UserId, DBWork.ProcIP);
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