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
using System.Linq;

namespace WebApp.Controllers.C
{
    public class CE0003Controller : SiteBase.BaseApiController
    {

        //修改
        [HttpPost]
        public ApiResponse UpdateCE0003_INI(CE0003 ce0003)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0003Repository(DBWork);

                    CHK_MAST mast = repo.GetMast(ce0003.CHK_NO);
                    if (mast.CHK_STATUS != "1")
                    {
                        session.Result.msg = "盤點單狀態已變更，請重新查詢";
                        session.Result.success = false;
                    }

                    ce0003.UPDATE_USER = User.Identity.Name;
                    ce0003.UPDATE_IP = DBWork.ProcIP;



                    session.Result.afrs = repo.UpdateCE0003_INI(ce0003);
                    session.Result.etts = repo.Get(ce0003.CHK_NO, ce0003.MMCODE, ce0003.STORE_LOC); //可讓前端收到資料後關閉mask

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
                    var repo = new CE0003Repository(DBWork);
                    var repoCE0002 = new CE0002Repository(DBWork);

                    IEnumerable<CE0003> items = repo.GetAll(wh_no, d0, User.Identity.Name, page, limit, sorters);

                    foreach (CE0003 item in items)
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
        public ApiResponse GetChk(FormDataCollection form)
        {
            var wh_no = form.Get("wh_no");
            var d0 = form.Get("d0");
            var chk_no = form.Get("chk_no");
            var barcode = form.Get("barcode");
            //var grid_sort = form.Get("grid_sort"); // 1 : 院內碼; 2 : 儲位
            var ischk = form.Get("ischk"); // Y : 已盤; N : 未盤
            var chk_uid = form.Get("chk_uid");

            string sort_by = form.Get("sort_by");
            string sort_order = form.Get("sort_order");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0003Repository(DBWork);

                    if (chk_uid == "" || chk_uid == null)
                        chk_uid = User.Identity.Name;

                    session.Result.etts = repo.GetChkData(wh_no, d0, chk_no, barcode, sort_by, sort_order, chk_uid, ischk);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetChkBarcode(FormDataCollection form)
        {
            var barcode = form.Get("p0");
            var chk_no = form.Get("p1");
            var chk_uid = form.Get("p2");
            var grid_sort = form.Get("p3");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0003Repository(DBWork);

                    session.Result.etts = repo.GetChkBarcodeData(barcode, chk_no, chk_uid, grid_sort);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse AllINIPDA(FormDataCollection form)
        {
            var chk_no = form.Get("chk_no");
            var mmcodeorstore = form.Get("mmcodeorstore");
            var chk_uid = form.Get("chk_uid");
            var searchRule = form.Get("searchRule");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0003Repository(DBWork);
                    IEnumerable<CE0003> items =null;
                    if (searchRule == "All")
                    {
                        //if(chk_uid == User.Identity.Name)
                        //{
                        //    items = repo.GetAllINIPDA(chk_no, mmcodeorstore, chk_uid);
                        //}//幫別人盤點時，不應該顯示對方已完成的項目
                        //else
                        //{
                        //    items = repo.GetAllINIotherPDA(chk_no, mmcodeorstore, chk_uid);
                        //}
                        //預設顯示自己未盤點項目
                        items = repo.GetAllINIotherPDA(chk_no, mmcodeorstore, chk_uid, page, limit, sorters);
                    }
                    else if(searchRule == "OKorder") //已盤點
                    {
                         items = repo.GetSign(chk_no, chk_uid, page, limit, sorters);
                    }
                    else if (searchRule == "NOorder") //還沒盤點
                    {
                        items = repo.GetNoSign(chk_no, chk_uid, page, limit, sorters);
                    }
                    else
                    {
                        //CE0016使用
                        items = repo.GetAllINIPDA(chk_no, mmcodeorstore, chk_uid, page, limit, sorters);
                    }


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
        public ApiResponse UPDN_STATUSGet(FormDataCollection form)
        {
            var chk_no = form.Get("chk_no");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0003Repository(DBWork);
                    session.Result.etts = repo.UPDN_STATUSGet(chk_no);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }



        [HttpPost]
        public ApiResponse AllINIAutoLoadPDA(FormDataCollection form)
        {
            var chk_no = form.Get("chk_no");
            var mmcodeorstore = form.Get("mmcodeorstore");
            var chk_uid = form.Get("chk_uid");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0003Repository(DBWork);
                    //預設顯示自己未盤點項目
                    session.Result.etts = repo.GetAllINIAutoLoadPDA(chk_no, mmcodeorstore, chk_uid);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost] //已盤
        public ApiResponse SelectCountSign(FormDataCollection form) //CE0003
        {
            var chk_no = form.Get("chk_no");
            var chk_uid = form.Get("chk_uid");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0003Repository repo = new CE0003Repository(DBWork);
                    session.Result.etts = repo.SelectCountSign(chk_no);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]//未盤
        public ApiResponse SelectCountNoSign(FormDataCollection form) //CE0003
        {
            var chk_no = form.Get("chk_no");
            var chk_uid = form.Get("chk_uid");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0003Repository repo = new CE0003Repository(DBWork);

                    session.Result.etts = repo.SelectCountNoSign(chk_no);

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMsgCount(FormDataCollection form)
        {
            var chk_no = form.Get("chk_no");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0003Repository repo = new CE0003Repository(DBWork);

                    session.Result.etts = repo.GetMsgCount(chk_no);

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse FinalPro(FormDataCollection form)
        {
            var chk_no = form.Get("chk_no");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0003Repository(DBWork);
                    CHK_MAST mast = repo.GetMast(chk_no);
                    if (mast.CHK_STATUS != "1") {
                        session.Result.msg = "盤點單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    string count = "0";

                    if (mast.CHK_WH_GRADE == "1")
                        count = repo.SelectCountFinalPro(chk_no, User.Identity.Name);
                    else
                        count = repo.SelectCountFinalPro(chk_no);

                    if (count != "0") //尚有負責的品項未盤點!
                    {
                        session.Result.etts = repo.SelectFinalPro(chk_no, User.Identity.Name);
                        session.Result.msg = "尚有負責的品項未盤點!";
                    }
                    else if (count == "0") //做update，  是否要提示自己完成盤點或是 CHK_TOTAL = CHK_NUM 提示訊息
                    {
                        //先Update Chk_detail(自己的CHK_NO)
                        session.Result.afrs = repo.UpdateChk_detail(chk_no, User.Identity.Name, DBWork.ProcIP);                        

                        //Update CHK_NUM
                        if (mast.CHK_WH_GRADE == "2" && mast.CHK_WH_KIND == "0" && mast.CHK_TYPE == "X")    // 藥局
                        {
                            session.Result.afrs = repo.UpdateChkmastMed(chk_no, DBWork.UserInfo.UserId, DBWork.ProcIP);
                        }
                        else
                        {
                            //更新chk_no為0
                            repo.UpdateChk_mastCHK_NUM(chk_no, User.Identity.Name, DBWork.ProcIP);
                            //根據Update chk_detail的STATUS_INI等於2且不限制userID
                            repo.UpdateChk_mast(chk_no, User.Identity.Name, DBWork.ProcIP);
                        }


                        string CHKNum = repo.SelectChk_mast(chk_no);  //查詢 格式: CHK_TOTAL,CHK_NUM

                        if (CHKNum.Substring(0, CHKNum.IndexOf(",")).Equals(CHKNum.Substring(CHKNum.IndexOf(",") + 1)))
                        {
                            repo.UpdateChk_mast_STATUS(chk_no, User.Identity.Name, DBWork.ProcIP);
                        }

                        if (mast.CHK_WH_GRADE != "1")
                        {
                            repo.UpdateDetailStat(chk_no, "2", User.Identity.Name, DBWork.ProcIP);
                            repo.UpdateMastStat(chk_no, "2", User.Identity.Name, DBWork.ProcIP);
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

        //[HttpPost]
        //public ApiResponse FinalProAll(FormDataCollection form)  //CE0016，有先暫存功能
        //{
        //    var CHK_NO = form.Get("CHK_NO");
        //    var ITEM_STRING = form.Get("ITEM_STRING");

        //    IEnumerable<CE0003> cE0003 = JsonConvert.DeserializeObject<IEnumerable<CE0003>>(ITEM_STRING);

        //    using (WorkSession session = new WorkSession(this))
        //    {
        //        var DBWork = session.UnitOfWork;
        //        try
        //        {
        //            var repo = new CE0003Repository(DBWork);
        //            foreach (var onecE0003 in cE0003)          //目前的CHK_NO 暫存
        //            {
        //                if (onecE0003.CHK_QTY == "")
        //                {
        //                    session.Result.afrs = repo.UpdateCE0003_INI_NOTIME_PC(onecE0003.CHK_QTY, onecE0003.CHK_REMARK, User.Identity.Name, DBWork.ProcIP, CHK_NO, onecE0003.MMCODE, onecE0003.STORE_LOC);
        //                }
        //                else
        //                {
        //                    session.Result.afrs = repo.UpdateCE0003_INIPC(onecE0003.CHK_QTY, onecE0003.CHK_REMARK, User.Identity.Name, DBWork.ProcIP, CHK_NO, onecE0003.MMCODE, onecE0003.STORE_LOC);
        //                }
        //            }

        //            var count = repo.SelectCountFinalPro(CHK_NO, User.Identity.Name);
        //            if (count != "0") //尚有負責的品項未盤點!
        //            {
        //                session.Result.etts = repo.SelectFinalPro(CHK_NO, User.Identity.Name);
        //                session.Result.msg = "尚有負責的品項未盤點!";
        //            }
        //            else if (count == "0") //做update，  是否要提示自己完成盤點或是 CHK_TOTAL = CHK_NUM 提示訊息
        //            {

        //                CHK_MAST mast = repo.GetMast(CHK_NO);


        //                //Update CHK_NUM
        //                if (mast.CHK_WH_GRADE == "2" && mast.CHK_WH_KIND == "0" && mast.CHK_TYPE == "X")    // 藥局
        //                {
        //                    session.Result.afrs = repo.UpdateChkmastMed(CHK_NO, DBWork.UserInfo.UserId, DBWork.ProcIP);
        //                }
        //                else
        //                {
        //                    repo.UpdateChk_mast(CHK_NO, User.Identity.Name, DBWork.ProcIP);
        //                }

        //                session.Result.afrs = repo.UpdateChk_detail(CHK_NO, User.Identity.Name, DBWork.ProcIP);

        //                string CHKNum = repo.SelectChk_mast(CHK_NO);  //查詢 格式: CHK_TOTAL,CHK_NUM

        //                if (CHKNum.Substring(0, CHKNum.IndexOf(",")).Equals(CHKNum.Substring(CHKNum.IndexOf(",") + 1)))
        //                {
        //                    repo.UpdateChk_mast_STATUS(CHK_NO, User.Identity.Name, DBWork.ProcIP);
        //                }

        //                session.Result.msg = "您已經完成此單號的盤點!";


        //            }
        //        }
        //        catch
        //        {
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //}

        [HttpPost]
        public ApiResponse UpdateAll(FormDataCollection form) //CE0016
        {
            var CHK_NO = form.Get("CHK_NO");
            var ITEM_STRING = form.Get("ITEM_STRING");

            IEnumerable<CE0003> cE0003 = JsonConvert.DeserializeObject<IEnumerable<CE0003>>(ITEM_STRING);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0003Repository(DBWork);
                    foreach (var onecE0003 in cE0003)
                    {
                        if (onecE0003.CHK_QTY == "")
                        {
                            session.Result.afrs = repo.UpdateCE0003_INI_NOTIME_PC(onecE0003.CHK_QTY, onecE0003.CHK_REMARK, User.Identity.Name, DBWork.ProcIP, onecE0003.CHK_NO, onecE0003.MMCODE, onecE0003.STORE_LOC);
                        }
                        else if(onecE0003.CHK_QTY != "") //CHK_TIME不為0，則造舊
                        {
                            session.Result.afrs = repo.UpdateCE0003_INIPC(onecE0003.CHK_QTY, onecE0003.CHK_REMARK, User.Identity.Name, DBWork.ProcIP, onecE0003.CHK_NO, onecE0003.MMCODE, onecE0003.STORE_LOC);
                        }
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
        public ApiResponse UpdateWhoDo(FormDataCollection form)
        {
            var CHK_NO = form.Get("CHK_NO");


            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0003Repository(DBWork);

                    session.Result.afrs = repo.UpdateMastWhoDo(CHK_NO, User.Identity.Name, "BATCH-X");
                    session.Result.afrs = repo.UpdateDetailWhoDo(CHK_NO, User.Identity.Name);


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
        public ApiResponse UpdateWhoDoMulti(FormDataCollection form) {
            var list = form.Get("list");

            IEnumerable<CHK_MAST> masts = JsonConvert.DeserializeObject<IEnumerable<CHK_MAST>>(list);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0003Repository(DBWork);

                    foreach (CHK_MAST mast in masts) {
                        session.Result.afrs = repo.UpdateMastWhoDo(mast.CHK_NO, User.Identity.Name, "BATCH-X");
                        session.Result.afrs = repo.UpdateDetailWhoDo(mast.CHK_NO, User.Identity.Name);
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



        public ApiResponse FinishAll(FormDataCollection form)
        {
            var chk_nos_string = form.Get("chk_nos");

            IEnumerable<string> chk_nos = GetChknos(chk_nos_string);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0003Repository(DBWork);
                    foreach (string chk_no in chk_nos)
                    {
                        var count = repo.SelectCountFinalPro(chk_no, User.Identity.Name);
                        if (count != "0") //尚有負責的品項未盤點!
                        {
                            session.Result.etts = repo.SelectFinalPro(chk_no, User.Identity.Name);
                            session.Result.msg = "尚有負責的品項未盤點!";
                            return session.Result;
                        }

                    }
                    foreach (string chk_no in chk_nos)
                    {
                        repo.UpdateChk_detail(chk_no, User.Identity.Name, DBWork.ProcIP);
                        repo.UpdateChk_mast(chk_no, User.Identity.Name, DBWork.ProcIP);  //Update CHK_NUM
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

        public IEnumerable<string> GetChknos(string tempChknos)
        {
            string[] temp = tempChknos.Split(',');
            List<string> list = new List<string>();
            for (int i = 0; i < temp.Length; i++)
            {
                list.Add(temp[i].Replace("'", string.Empty));
            }

            return list;
        }

        [HttpPost]
        public ApiResponse GetWhnoCombo(FormDataCollection form)
        {
            var chk_ym = form.Get("chk_ym");
            var chk_period = form.Get("chk_period");
            var chk_level = form.Get("chk_level");
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0003Repository repo = new CE0003Repository(DBWork);
                    session.Result.etts = repo.GetWhnoCombo(chk_ym, chk_level, chk_period, DBWork.UserInfo.UserId)
                        .Select(w => new { WH_NO = w.WH_NO, WH_NAME = w.WH_NAME, WH_KIND = w.WH_KIND, WH_GRADE = w.WH_GRADE });
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetChknoCombo(FormDataCollection form)
        {
            var chk_level = form.Get("chk_level");
            var chk_period = form.Get("chk_period"); // WH_NO
            var chk_ym = form.Get("chk_ym"); // DATE
            var wh_no = form.Get("wh_no"); // DATE
            var chk_type = form.Get("chk_type"); // DATE

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0003Repository repo = new CE0003Repository(DBWork);
                    //CE0002Repository repoCE0002 = new CE0002Repository(DBWork);
                    //IEnumerable<CE0003> items = repo.GetChknoCombo(p0, p1, p2, DBWork.UserInfo.UserId, page, limit, "");

                    //foreach (CE0003 item in items)
                    //{
                    //    item.CHK_TYPE = repoCE0002.GetChkWhkindName(item.CHK_WH_KIND_CODE, item.CHK_TYPE_CODE).Split(' ')[1];
                    //}
                    session.Result.etts = repo.GetChknoCombo(wh_no, chk_ym, chk_level, chk_period, DBWork.UserInfo.UserId, chk_type);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //此盤點單的盤點人員
        [HttpPost]
        public ApiResponse GetPeopleCombo(FormDataCollection form)
        {
            var chk_no = form.Get("chk_no");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0003Repository repo = new CE0003Repository(DBWork);
                    session.Result.etts = repo.PeopleCombo(chk_no);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetChkTypeCombo(FormDataCollection form)
        {
            var chk_ym = form.Get("chk_ym");
            var chk_period = form.Get("chk_period");
            var wh_no = form.Get("wh_no");
            var wh_kind = form.Get("wh_kind");
            var chk_level = form.Get("chk_level");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0003Repository repo = new CE0003Repository(DBWork);
                    session.Result.etts = repo.ChkTypeCombo(chk_ym, chk_period, wh_no, wh_kind, DBWork.UserInfo.UserId);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetFirstItem(FormDataCollection form) {
            string chk_no = form.Get("chk_no");
            string mmcode = form.Get("mmcode");
            string store_loc = form.Get("store_loc");
            string sort_by = form.Get("sort_by");
            string sort_order = form.Get("sort_order");
            string chk_uid = form.Get("chk_uid");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0003Repository repo = new CE0003Repository(DBWork);

                    CE0003 item = repo.GetSingleItem(chk_no, chk_uid, sort_order, sort_by, mmcode, store_loc);
                    List<CE0003> items = new List<CE0003>();
                    if (item != null) {
                        items.Add(item);
                    }
                    
                    session.Result.etts = items;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
            //string sort_direc = form.Get("sort_direc");
            //string sort_direc = form.Get("sort_direc");
            return null;
        }
    }
}