using JCLib.DB;
using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using WebApp.Models;
using WebApp.Repository.B;

namespace WebApp.Controllers.B
{
    public class BE0007Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0"); //廠商代碼
            var p1 = form.Get("p1"); //發票號碼
            var p2 = form.Get("p2"); //進貨日期 起
            var p3 = form.Get("p3"); //進貨日期 迄
            var p4 = form.Get("p4"); //月結年月
            var p5 = form.Get("p5"); //物料類別

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BE0007Repository(DBWork);
                    //string mat_sub_class = string.Empty; 
                    //if (p5.IndexOf("SUB_") > -1) {
                    //    mat_sub_class = p5.Replace("SUB_","");
                    //}
                    //if (string.IsNullOrEmpty(mat_sub_class) == false) {
                    //    p5 = string.Empty;
                    //}
                    session.Result.etts = repo.GetAll(p0, p1, p2, p3, p4, p5);
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse Merge(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BE0007Repository repo = new BE0007Repository(DBWork);
                    string user = User.Identity.Name;
                    string ip = DBWork.ProcIP;
                    string p0 = form.Get("p0").ToString();
                    string p1 = form.Get("p1").ToString();
                    string p2 = form.Get("p2").ToString();

                    DataTable chkDt = repo.GetDupInvoice(p1, p2);
                    if (chkDt.Rows.Count > 0)
                    {
                        session.Result.success = false;
                        session.Result.msg = "發票號碼：" + chkDt.Rows[0]["INVOICE"] + " 院內碼：" + chkDt.Rows[0]["MMCODE"] + " 重複，請檢查！";
                        return session.Result;
                    }

                    // 取得要刪除的invoice
                    IEnumerable<INVOICE_INFO> invoices = repo.GetDeleteInvoiceInfo(p1, p2);

                    // 刪除invoice
                    foreach (INVOICE_INFO invoice in invoices)
                    {
                        session.Result.afrs = repo.DeleteInvoice(invoice.INVOICE, invoice.MMCODE);
                    }
                    // insert into INVOICE
                    repo.Merge(p0, p1, p2, user, ip);
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
        public ApiResponse UpdateInvoicedt(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                string chkmsg = "";
                try
                {
                    BE0007Repository repo = new BE0007Repository(DBWork);
                    string invoicedt = form.Get("INVOICE_DT").Substring(0, 10);
                    string invoice = form.Get("INVOICE");
                    string chk1 = repo.ChkInvoice(invoicedt, invoice); //同一發票不可以 不同發票日期
                    string chk2 = repo.ChkInvoicePONO(invoicedt, invoice); //同一發票不可以出現在1家以上廠商
                    if (chk1.Equals("N") && chk2.Equals("N"))
                    {
                        if (form.Get("PO_NO") != "")
                        {
                            string user = User.Identity.Name;
                            string ip = DBWork.ProcIP;
                            string po_no = form.Get("PO_NO").Substring(0, form.Get("PO_NO").Length - 1); // 去除前端傳進來最後一個逗號
                            string mmcode = form.Get("MMCODE").Substring(0, form.Get("MMCODE").Length - 1); // 去除前端傳進來最後一個逗號
                            string transno = form.Get("TRANSNO").Substring(0, form.Get("TRANSNO").Length - 1); // 去除前端傳進來最後一個逗號
                            string[] tmp = po_no.Split(',');
                            string[] tmp1 = mmcode.Split(',');
                            string[] tmp2 = transno.Split(',');
                            for (int i = 0; i < tmp.Length; i++)
                            {
                                string chk3 = repo.ChkInvoiceDB(po_no, mmcode, invoice);  //同一PO_NO+MMCODE不可有 1張以上發票
                                if (chk3.Equals("N"))
                                {
                                    session.Result.afrs = repo.UpdateInvoicedt(tmp[i], tmp1[i], tmp2[i], invoicedt, invoice, user, ip);
                                }
                                else
                                {
                                    chkmsg += mmcode + ",";
                                }
                            }
                            DBWork.Commit();
                        }
                    }
                    else
                    {
                        if (chk1.Equals("Y"))
                            session.Result.msg = "1.發票編號已存在且日期不同,請檢查!";
                        if (chk2.Equals("Y") && session.Result.msg == "")
                            session.Result.msg += "1.同一發票不可以出現在1家以上廠商,請檢查!";
                        else if (chk2.Equals("Y"))
                            session.Result.msg += "2.同一發票不可以出現在1家以上廠商,請檢查!";
                    }
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                if (chkmsg != "")
                    session.Result.msg = "下列相同院內碼已存在相同發票,請檢查!" + chkmsg;
                return session.Result;
            }
        }
        // 取消驗證
        [HttpPost]
        public ApiResponse Reject(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BE0007Repository repo = new BE0007Repository(DBWork);
                    if (form.Get("PO_NO") != "")
                    {
                        string user = User.Identity.Name;
                        string ip = DBWork.ProcIP;
                        string po_no = form.Get("PO_NO").Substring(0, form.Get("PO_NO").Length - 1); // 去除前端傳進來最後一個逗號
                        string mmcode = form.Get("MMCODE").Substring(0, form.Get("MMCODE").Length - 1); // 去除前端傳進來最後一個逗號
                        string transno = form.Get("TRANSNO").Substring(0, form.Get("TRANSNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = po_no.Split(',');
                        string[] tmp1 = mmcode.Split(',');
                        string[] tmp2 = transno.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            session.Result.afrs = repo.Reject(tmp[i], tmp1[i], tmp2[i], user, ip);
                        }
                        DBWork.Commit();
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
        // 刪除
        [HttpPost]
        public ApiResponse UpdateDEL(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BE0007Repository repo = new BE0007Repository(DBWork);
                    if (form.Get("PO_NO") != "")
                    {
                        string user = User.Identity.Name;
                        string ip = DBWork.ProcIP;
                        string po_no = form.Get("PO_NO").Substring(0, form.Get("PO_NO").Length - 1); // 去除前端傳進來最後一個逗號
                        string mmcode = form.Get("MMCODE").Substring(0, form.Get("MMCODE").Length - 1); // 去除前端傳進來最後一個逗號
                        string transno = form.Get("TRANSNO").Substring(0, form.Get("TRANSNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = po_no.Split(',');
                        string[] tmp1 = mmcode.Split(',');
                        string[] tmp2 = transno.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            session.Result.afrs = repo.UpdateDEL(tmp[i], tmp1[i], tmp2[i], user, ip);
                        }
                        DBWork.Commit();
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
        // 修改
        [HttpPost]
        public ApiResponse UpdateM(BE0007 BE0007)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BE0007Repository(DBWork);

                    BE0007.UPDATE_USER = User.Identity.Name;
                    BE0007.UPDATE_IP = DBWork.ProcIP;
                    if (BE0007.INVOICE_TYPE == "銷貨") BE0007.INVOICE_TYPE = "1";
                    else if (BE0007.INVOICE_TYPE == "退貨") BE0007.INVOICE_TYPE = "2";
                    else if (BE0007.INVOICE_TYPE == "非發票") BE0007.INVOICE_TYPE = "3";
                    string chk1 = repo.ChkInvoice(BE0007.INVOICE, BE0007.INVOICE_DT);
                    string chk2 = repo.ChkInvoicePONO(BE0007.INVOICE, BE0007.PO_NO);
                    if (chk1.Equals("N") && chk2.Equals("N"))
                    {
                        session.Result.afrs = repo.UpdateM(BE0007);
                        session.Result.etts = repo.Get(BE0007.PO_NO, BE0007.MMCODE, BE0007.TRANSNO);
                        DBWork.Commit();
                    }
                    else
                    {
                        int n = 1;
                        if (chk1.Equals("Y"))
                        {
                            session.Result.msg = n.ToString() + ".同一發票不可不同日期，請重新確認!";
                            n++;
                        }
                        if (chk2.Equals("Y"))
                        {
                            session.Result.msg += n.ToString() + ".同一發票需為相同廠商，請重新確認!";
                            n++;
                        }
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
        //4
        [HttpPost]
        public ApiResponse UpdateB(BE0007 BE0007)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BE0007Repository(DBWork);

                    BE0007.UPDATE_USER = User.Identity.Name;
                    BE0007.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.UpdateB(BE0007);
                    session.Result.etts = repo.Get(BE0007.PO_NO, BE0007.MMCODE, BE0007.TRANSNO);

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
        //5

        // 新增
        [HttpPost]
        public ApiResponse CreateM(BE0007 BE0007)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BE0007Repository(DBWork);
                    BE0007.CREATE_USER = User.Identity.Name;
                    BE0007.UPDATE_IP = DBWork.ProcIP;
                    if (BE0007.INVOICE_TYPE == "銷貨") BE0007.INVOICE_TYPE = "1";
                    else if (BE0007.INVOICE_TYPE == "退貨") BE0007.INVOICE_TYPE = "2";
                    else if (BE0007.INVOICE_TYPE == "非發票") BE0007.INVOICE_TYPE = "3";
                    string chk1 = repo.ChkInvoice(BE0007.INVOICE, BE0007.INVOICE_DT);
                    //string chk2 = repo.ChkMmPoMPONO(BE0007.PO_NO);
                    string chk3 = repo.CheckSamePhrInsert(BE0007.MMCODE, BE0007.INVOICE);
                    //string chk4 = repo.ChkMmPoDdata(BE0007.PO_NO, BE0007.MMCODE);
                    if (chk1.Equals("N") && chk3.Equals("N"))
                    {
                        session.Result.afrs = repo.CreateM(BE0007);
                        //session.Result.etts = repo.Get(BE0007.PO_NO, BE0007.MMCODE, BE0007.TRANSNO);
                        DBWork.Commit();
                    }
                    else
                    {
                        int n = 1;
                        if (chk1.Equals("Y"))
                        {
                            session.Result.msg = n.ToString() + ".同一發票不可不同日期，請重新確認!";
                            n++;
                        }
                        //if (chk2.Equals("Y"))
                        //{
                        //    session.Result.msg += n.ToString() + ".訂單號碼不存在，請重新輸入!";
                        //    n++;
                        //}
                        if (chk3.Equals("Y"))
                        {
                            session.Result.msg += n.ToString() + ".同一發票需為相同廠商，請重新確認!";
                            n++;
                        }
                        //if (chk4.Equals("Y"))
                        //{
                        //    session.Result.msg += n.ToString() + ".藥材代碼" + BE0007.MMCODE + "不存在於訂單號碼" + BE0007.PO_NO + "中，請重新確認!";
                        //    n++;
                        //}
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
        public ApiResponse UpdateA(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BE0007Repository repo = new BE0007Repository(DBWork);
                    if (form.Get("PO_NO") != "")
                    {
                        string user = User.Identity.Name;
                        string ip = DBWork.ProcIP;
                        string po_no = form.Get("PO_NO").Substring(0, form.Get("PO_NO").Length - 1); // 去除前端傳進來最後一個逗號
                        string mmcode = form.Get("MMCODE").Substring(0, form.Get("MMCODE").Length - 1); // 去除前端傳進來最後一個逗號
                        string transno = form.Get("TRANSNO").Substring(0, form.Get("TRANSNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = po_no.Split(',');
                        string[] tmp1 = mmcode.Split(',');
                        string[] tmp2 = transno.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            session.Result.afrs = repo.UpdateA(tmp[i], tmp1[i], tmp2[i], user, ip);
                        }
                        DBWork.Commit();
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
        public ApiResponse GetInsertData(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BE0007Repository repo = new BE0007Repository(DBWork);
                    if (form.Get("PO_NO") != "")
                    {

                        string po_no = form.Get("PO_NO").ToString();
                        session.Result.msg = repo.GetInsertData(po_no);
                        DBWork.Commit();
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

        [HttpGet]
        public ApiResponse GetMatclassCombo()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BE0007Repository repo = new BE0007Repository(DBWork);
                    session.Result.etts = repo.GetMatclassCombo(repo.DBWork.UserInfo.UserId);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetInvoiceExistCombo()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BE0007Repository repo = new BE0007Repository(DBWork);
                    session.Result.etts = repo.GetInvoiceExistCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetContIdCombo()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BE0007Repository repo = new BE0007Repository(DBWork);
                    session.Result.etts = repo.GetContIdCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetCkStatusCombo()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BE0007Repository repo = new BE0007Repository(DBWork);
                    session.Result.etts = repo.GetCkStatusCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetMemoCombo()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BE0007Repository repo = new BE0007Repository(DBWork);
                    session.Result.etts = repo.GetMemoCombo();
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
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BE0007Repository repo = new BE0007Repository(DBWork);
                    session.Result.etts = repo.GetAgennoCombo(p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        #region 刪除
        //[HttpPost]
        //public ApiResponse Delete(PH_VENDER ph_vender)
        //{
        //    using (WorkSession session = new WorkSession(this))
        //    {
        //        var DBWork = session.UnitOfWork;
        //        DBWork.BeginTransaction();
        //        try
        //        {
        //            var repo = new BE0007Repository(DBWork);
        //            if (repo.CheckExists(ph_vender.AGEN_NO))
        //            {
        //                session.Result.afrs = repo.Delete(ph_vender.AGEN_NO);
        //            }
        //            else
        //            {
        //                session.Result.afrs = 0;
        //                session.Result.success = false;
        //                session.Result.msg = "<span style='color:red'>廠商碼</span>不存在。";
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
        //public ApiResponse chkINVOICE(BE0007 BE0007)
        //{
        //    using (WorkSession session = new WorkSession(this))
        //    {
        //        var DBWork = session.UnitOfWork;
        //        DBWork.BeginTransaction();
        //        try
        //        {
        //            var repo = new BE0007Repository(DBWork);

        //            session.Result.afrs = repo.UpdateM(BE0007);
        //            session.Result.etts = repo.Get(BE0007.PO_NO, BE0007.MMCODE, BE0007.TRANSNO);

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
        #endregion

        #region 2022-09-26
        [HttpPost]
        public ApiResponse GetUploadExample()
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BE0007Repository(DBWork);

                    JCLib.Excel.Export("發票維護上傳範本.xls", repo.GetUploadExample());

                    return session.Result;
                }
                catch (Exception e)
                {
                    throw;
                }
            }
        }

        [HttpPost]
        public ApiResponse GetExcel(FormDataCollection form)
        {
            var p0 = form.Get("p0"); //廠商代碼
            var p1 = form.Get("p1"); //發票號碼
            var p2 = form.Get("p2"); //進貨日期 起
            var p3 = form.Get("p3"); //進貨日期 迄
            var p4 = form.Get("p4"); //月結年月
            var p5 = form.Get("p5"); //物料類別

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BE0007Repository(DBWork);
                    string fileName = string.Format("發票管理維護_{0}.xlsx", DateTime.Now.ToString("yyyyMMddHHmmss"));

                    JCLib.Excel.Export(fileName, repo.GetExcel(p0, p1, p2, p3, p4, p5));
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }

        }

        [HttpPost]
        public ApiResponse UploadCheck()
        {
            List<BE0007> list = new List<BE0007>();
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;

                var repo = new BE0007Repository(DBWork);

                try
                {
                    #region data upload
                    List<HeaderItem> headerItems = new List<HeaderItem>() {
                       new HeaderItem("訂單號碼", "PO_NO"),
                        new HeaderItem("院內碼", "MMCODE"),
                        new HeaderItem("資料流水號", "TRANSNO"),
                        new HeaderItem("發票號碼", "INVOICE"),
                        new HeaderItem("發票日期", "INVOICE_DT"),
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

                        BE0007 temp = new BE0007();
                        foreach (HeaderItem item in headerItems)
                        {
                            string value = row.GetCell(item.Index) == null ? string.Empty : row.GetCell(item.Index).ToString();
                            temp.GetType().GetProperty(item.FieldName).SetValue(temp, value, null);
                        }
                        list.Add(temp);
                    }
                    #endregion

                    list = list.Where(x => x.PO_NO != null && x.MMCODE != null).Select(x => x).ToList();

                    foreach (BE0007 item in list)
                    {
                        item.ITEM_STRING = string.Empty;
                        item.INVOICE = item.INVOICE.Substring(0, 10);

                        // 檢查資料是否存在
                        if (repo.CheckPhInvoiceExists(item.PO_NO, item.MMCODE, item.TRANSNO) == false)
                        {
                            item.ITEM_STRING = "資料不存在，請重新確認";
                            continue;
                        }

                        // 檢查資料是否已驗證
                        if (repo.CheckPhInvoiceCksStatusNotY(item.PO_NO, item.MMCODE, item.TRANSNO) == false)
                        {
                            item.ITEM_STRING = "資料已驗證完成";
                            continue;
                        }

                        // 發票日期是否為民國年月日
                        if (repo.CheckInvoiceDtValid(item.INVOICE_DT) == false)
                        {
                            item.ITEM_STRING = "發票日期請填入民國年月日";
                            continue;
                        }
                    }

                    session.Result.etts = list;
                }
                catch (Exception e)
                {
                    throw;
                }

                return session.Result;
            }

        }

        [HttpPost]
        public ApiResponse UploadConfirm(FormDataCollection form)
        {
            string data_string = form.Get("data");
            List<BE0007> list = JsonConvert.DeserializeObject<IEnumerable<BE0007>>(data_string).ToList();
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BE0007Repository(DBWork);
                    foreach (BE0007 item in list)
                    {
                        session.Result.afrs = repo.UpdatePhInvoiceFromUpload(item.PO_NO, item.MMCODE, item.TRANSNO, item.INVOICE, item.INVOICE_DT,
                                                                             DBWork.UserInfo.UserId, DBWork.ProcIP);
                    }

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
        public ApiResponse GetDateRanges()
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BE0007Repository(DBWork);

                    session.Result.etts = repo.GetDateRanges();

                    return session.Result;
                }
                catch (Exception e)
                {
                    throw;
                }
            }
        }

        [HttpPost]
        public ApiResponse CheckHasEmptyInvoice(FormDataCollection form)
        {
            string start_date = form.Get("start_date");
            string end_date = form.Get("end_date");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BE0007Repository(DBWork);
                    session.Result.success = repo.CheckHasEmptyInvoice(start_date, end_date);
                    return session.Result;
                }
                catch (Exception e)
                {
                    throw;
                }
            }
        }

        [HttpPost]
        public ApiResponse GetMmCodeCombo(FormDataCollection form)
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
                    var repo = new BE0007Repository(DBWork);
                    session.Result.etts = repo.GetMmCodeCombo(p0, page, limit, "");
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetInvoiceCombo(FormDataCollection form) {
            var p2 = form.Get("p2"); //進貨日期 起
            var p3 = form.Get("p3"); //進貨日期 迄
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BE0007Repository(DBWork);
                    session.Result.etts = repo.GetInvoiceCombo(p2, p3);
                    return session.Result;
                }
                catch (Exception e) {
                    throw;
                }
            }
        }
    }
}