using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AB;
using WebApp.Models;
using JCLib.DB.Tool;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using Newtonsoft.Json;

namespace WebApp.Controllers.AB
{
    public class AB0053Controller : SiteBase.BaseApiController
    {
        // 查詢[各庫效期表]
        [HttpPost]
        public ApiResponse All_1(FormDataCollection form)
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
                    var repo = new AB0053Repository(DBWork);
                    session.Result.etts = repo.GetAll_1(p0, p1, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse All_2(FormDataCollection form)
        {
            var p2 = form.Get("p2");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0053Repository(DBWork);
                    session.Result.etts = repo.GetAll_2(p2, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse All_3(FormDataCollection form)
        {
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = form.Get("p5").Trim();
            var p6 = form.Get("p6").Trim();
            var p7 = form.Get("p7") == null ? string.Empty : form.Get("p7");
            var p8 = form.Get("p8") == "Y";
            var p9 = form.Get("p9") == "Y";
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            //string closeFlags = GetComboStrings(p5);
            string agennos = GetComboStrings(p7);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0053Repository(DBWork);
                    session.Result.etts = repo.GetAll_3(p3, p4, p5, p6, agennos,p8, p9);
                }
                catch(Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }
        public string GetComboStrings(string ori) {
            if (ori == string.Empty) return ori;
            string result = string.Empty;

            string[] temp = ori.Trim().Split(',');
            for (int i = 0; i < temp.Length; i++) {
                if (result != string.Empty) {
                    result += ",";
                }
                result += string.Format("'{0}'", temp[i]);
            }

            return result;
        }

        // 回報截止
        [HttpPost]
        public ApiResponse Return_Close()
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0053Repository(DBWork);
                    
                    session.Result.afrs = repo.Return_Close(DBWork.UserInfo.UserId, DBWork.ProcIP);
                    // 2020-07-09 新增: 調整廠商，回報截止時修改ME_EXPM廠商資訊
                    // 2020-10-27 刪除: 回報截止時修改ME_EXPM廠商資訊 => 改於AB0052[傳送]時處理
                    //session.Result.afrs = repo.SetAgenInfo(DBWork.UserInfo.UserId, DBWork.ProcIP);
                    

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

        // 新增
        [HttpPost]
        public ApiResponse Create(AB0053_3 AB0053_3)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0053Repository(DBWork);
                    if (!repo.CheckExistsT3(AB0053_3.MMCODE, AB0053_3.EXP_DATE, AB0053_3.LOT_NO, AB0053_3.WARNYM)) // 新增前檢查主鍵是否已存在
                    {
                        AB0053_3 agen_info = repo.GetAgenInfo(AB0053_3.MMCODE);
                        if (agen_info != null) {
                            AB0053_3.AGEN_NO = agen_info.AGEN_NO;
                            AB0053_3.AGEN_NAMEC = agen_info.AGEN_NAMEC;
                        }
                        AB0053_3.CREATE_USER = User.Identity.Name;
                        AB0053_3.UPDATE_USER = User.Identity.Name;
                        AB0053_3.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.Create(AB0053_3);
                        //session.Result.etts = repo.Get(mi_wlocinv.WH_NO, mi_wlocinv.MMCODE, mi_wlocinv.STORE_LOC);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>此院內碼、有效日期及批號</span>重複，請重新輸入。";
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
        public ApiResponse Update(AB0053_3 AB0053_3)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0053Repository(DBWork);
                    AB0053_3.UPDATE_USER = User.Identity.Name;
                    AB0053_3.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.Update(AB0053_3);
                    //session.Result.etts = repo.Get(mi_wlocinv.WH_NO, mi_wlocinv.MMCODE, mi_wlocinv.STORE_LOC);

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
        public ApiResponse Delete(AB0053_3 AB0053_3)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0053Repository(DBWork);
                    if (repo.CheckExistsT3(AB0053_3.MMCODE, AB0053_3.EXP_DATE, AB0053_3.LOT_NO, AB0053_3.WARNYM_KEY))
                    {
                        session.Result.afrs = repo.Delete(AB0053_3.MMCODE, AB0053_3.EXP_DATE, AB0053_3.LOT_NO, AB0053_3.WARNYM_KEY);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>此院內碼、有效日期及批號</span>不存在。";
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

        //匯出EXCEL
        [HttpPost]
        public ApiResponse Excel_T1(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0053Repository repo = new AB0053Repository(DBWork);
                    JCLib.Excel.Export(form.Get("FN"), repo.GetExcel_T1(p0, p1));
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
        public ApiResponse Excel_T2(FormDataCollection form)
        {
            var p2 = form.Get("p2");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0053Repository repo = new AB0053Repository(DBWork);
                    JCLib.Excel.Export(form.Get("FN"), repo.GetExcel_T2(p2));
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
        public ApiResponse Excel_T3(FormDataCollection form)
        {
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = form.Get("p5").Trim();
            var p6 = form.Get("p6").Trim();
            var p7 = form.Get("p7") == null ? string.Empty : form.Get("p7");
            //string closeFlags = GetComboStrings(p5);
            string closeFlags = p5;
            string agennos = GetComboStrings(p7);

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0053Repository repo = new AB0053Repository(DBWork);
                    JCLib.Excel.Export(form.Get("FN"), repo.GetExcel_T3(p3, p4, closeFlags, p6, agennos));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //取得庫房代碼清單
        public ApiResponse GetWhno()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0053Repository(DBWork);
                    session.Result.etts = repo.GetWhno();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //取得院內碼清單
        [HttpPost]
        public ApiResponse GetMMCodeCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            //var wh_no = form.Get("WH_NO");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0053Repository repo = new AB0053Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeCombo(p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //院內碼小視窗
        [HttpPost]
        public ApiResponse GetMmcode(FormDataCollection form)
        {
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0053Repository repo = new AB0053Repository(DBWork);
                    AB0053Repository.MI_MAST_QUERY_PARAMS query = new AB0053Repository.MI_MAST_QUERY_PARAMS();
                    query.MMCODE = form.Get("MMCODE") == null ? "" : form.Get("MMCODE").ToUpper();
                    query.MMNAME_C = form.Get("MMNAME_C") == null ? "" : form.Get("MMNAME_C").ToUpper();
                    query.MMNAME_E = form.Get("MMNAME_E") == null ? "" : form.Get("MMNAME_E").ToUpper();
                    //query.WH_NO = form.Get("WH_NO") == null ? "" : form.Get("WH_NO").ToUpper();
                    session.Result.etts = repo.GetMmcode(query, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //發送EMAIL
        [HttpPost]
        public ApiResponse SendMail()
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0053Repository(DBWork);
                    session.Result.afrs = repo.SendMail_1();
                    session.Result.afrs = repo.SendMail_2(DBWork.ProcIP);
                    session.Result.afrs = repo.SendMail_3(User.Identity.Name, DBWork.ProcIP);

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
        public ApiResponse SendMail_v2(FormDataCollection form) {

            string itemString = form.Get("itemString");
            IEnumerable<AB0053_3> items = JsonConvert.DeserializeObject<IEnumerable<AB0053_3>>(itemString);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0053Repository(DBWork);

                    string no_emails = string.Empty;
                    foreach (AB0053_3 item in items)
                    {
                        string email = repo.GetAgenEmail(item.MMCODE, item.LOT_NO, item.EXP_DATE);
                        if (email.Trim() == string.Empty) {
                            no_emails += "<br>";
                            no_emails += string.Format("院內碼：{0}，批號：{1}", item.MMCODE, item.LOT_NO);
                        }
                    }
                    if (no_emails != string.Empty) {
                        session.Result.msg = string.Format("下列資料無廠商或無EMAIL，請修改後再寄出：<br>{0}", no_emails);
                        session.Result.success = false;
                        return session.Result;
                    }

                    foreach (AB0053_3 item in items) {



                        item.UPDATE_IP = DBWork.ProcIP;
                        item.UPDATE_USER = DBWork.UserInfo.UserId;

                        session.Result.afrs = repo.SendMail_1_v2(item);
                        session.Result.afrs = repo.SendMail_2_v2(item);
                        session.Result.afrs = repo.SendMail_3_v2(item);

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

        //複製資料
        [HttpPost]
        public ApiResponse CopyData()
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0053Repository(DBWork);
                    session.Result.afrs = repo.CopyData(User.Identity.Name, User.Identity.Name, DBWork.ProcIP);

                    // 2022-01-07新增：更新本月qty=0品項為上月qty(若存在)
                    session.Result.afrs = repo.UpdateQtyFromPre(DBWork.UserInfo.UserId, DBWork.ProcIP);

                    DBWork.Commit();
                    return session.Result;
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    session.Result.success = false;
                    session.Result.msg = e.Message;
                    return session.Result;
                    //throw;
                }
            }
        }

        //取得院內碼清單
        [HttpPost]
        public ApiResponse GetAgennoCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            //var wh_no = form.Get("WH_NO");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0053Repository repo = new AB0053Repository(DBWork);
                    session.Result.etts = repo.GetAgenCombo(p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse VenderInfos(FormDataCollection form) {
            var queryType = form.Get("p0");
            var mmcode = form.Get("p1");

            using (WorkSession session = new WorkSession(this)) {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0053Repository repo = new AB0053Repository(DBWork);
                    if (queryType == "H") {     //HIS
                        session.Result.etts = repo.GetHisVenders(mmcode);
                    }
                    if (queryType == "P") {
                        session.Result.etts = repo.GetAllVenders();
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
        public ApiResponse UpdateAgenno(FormDataCollection form) {
            var agen_no = form.Get("agen_no");
            var agen_namec = form.Get("agen_namec");
            var mmcode = form.Get("mmcode");
            var source = form.Get("source");
            var exp_date = form.Get("exp_date");
            var lot_no = form.Get("lot_no");

            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0053Repository(DBWork);
                    session.Result.afrs = repo.UpdateAgenno(mmcode, exp_date, lot_no, agen_no, agen_namec, source, DBWork.UserInfo.UserId, DBWork.ProcIP);

                    DBWork.Commit();
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    session.Result.success = false;
                    session.Result.msg = e.Message;
                    return session.Result;
                    //throw;
                }
                return session.Result;
            }
        }
    }
}