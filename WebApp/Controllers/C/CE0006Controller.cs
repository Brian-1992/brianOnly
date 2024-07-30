using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.C;
using WebApp.Models.C;
using System.Collections.Generic;
using Newtonsoft.Json;
using WebApp.Models;

namespace WebApp.Controllers.C
{
    public class CE0006Controller : SiteBase.BaseApiController
    {

        //修改
        [HttpPost]
        public ApiResponse UpdateCE0006_INI(CE0006 ce0006)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0006Repository(DBWork);
                    ce0006.UPDATE_USER = User.Identity.Name;
                    ce0006.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.UpdateCE0006_INI(ce0006);
                    session.Result.etts = repo.Get(ce0006.CHK_NO, ce0006.MMCODE, ce0006.STORE_LOC); //可讓前端收到資料後關閉mask

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
                    var repo = new CE0006Repository(DBWork);
                    session.Result.etts = repo.GetAll(wh_no, d0, User.Identity.Name, page, limit, sorters);
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
                    var repo = new CE0006Repository(DBWork);
                    IEnumerable<CE0006> items = null;
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
                    else if (searchRule == "OKorder") //已盤點
                    {
                        items = repo.GetSign(chk_no, chk_uid, page, limit, sorters);
                    }
                    else if (searchRule == "NOorder") //還沒盤點
                    {
                        items = repo.GetNoSign(chk_no, chk_uid, page, limit, sorters);
                    }
                    else
                    {
                        //CE0017使用
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
                    var repo = new CE0006Repository(DBWork);
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
        [HttpPost]
        public ApiResponse GetCE0006_INI(FormDataCollection form) //CE0006
        {
            var chk_no = form.Get("chk_no");
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0006Repository repo = new CE0006Repository(DBWork);
                    session.Result.etts = repo.GetCE0006_INI(chk_no);
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
                    var repo = new CE0006Repository(DBWork);
                    var count = repo.SelectCountFinalPro(chk_no, User.Identity.Name);
                    if (count != "0") //尚有負責的品項未盤點!
                    {
                        session.Result.etts = repo.SelectFinalPro(chk_no, User.Identity.Name);
                        session.Result.msg = "尚有負責的品項未盤點!";
                    }
                    else if (count == "0") //做update，  是否要提示自己完成盤點或是 CHK_TOTAL = CHK_NUM 提示訊息
                    {
                        //先Update Chk_detail(自己的CHK_NO)
                        session.Result.afrs = repo.UpdateChk_detail(chk_no, User.Identity.Name, DBWork.ProcIP);
                        //更新chk_no為0
                        repo.UpdateChk_mastCHK_NUM(chk_no, User.Identity.Name, DBWork.ProcIP);
                        //根據Update chk_detail的STATUS_INI等於2且不限制userID
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

        [HttpPost] //已盤
        public ApiResponse SelectCountSign(FormDataCollection form) //CE0006
        {
            var chk_no = form.Get("chk_no");
            var chk_uid = form.Get("chk_uid");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0006Repository repo = new CE0006Repository(DBWork);
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
        public ApiResponse SelectCountNoSign(FormDataCollection form) //CE0006
        {
            var chk_no = form.Get("chk_no");
            var chk_uid = form.Get("chk_uid");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0006Repository repo = new CE0006Repository(DBWork);

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
        public ApiResponse UpdateAll(FormDataCollection form)
        {
            var CHK_NO = form.Get("CHK_NO");
            var ITEM_STRING = form.Get("ITEM_STRING");

            IEnumerable<CE0006> cE0006 = JsonConvert.DeserializeObject<IEnumerable<CE0006>>(ITEM_STRING);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0006Repository(DBWork);
                    foreach (var onecE0006 in cE0006)
                    {
                        if (onecE0006.CHK_QTY == "")
                        {
                            session.Result.afrs = repo.UpdateCE0006_INI_NOTIME_PC(onecE0006.CHK_QTY, onecE0006.CHK_REMARK, User.Identity.Name, DBWork.ProcIP, onecE0006.CHK_NO, onecE0006.MMCODE, onecE0006.STORE_LOC);
                        }
                        else
                        {
                            session.Result.afrs = repo.UpdateCE0006_INIPC(onecE0006.CHK_QTY, onecE0006.CHK_REMARK, User.Identity.Name, DBWork.ProcIP, onecE0006.CHK_NO, onecE0006.MMCODE, onecE0006.STORE_LOC);
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
                    CE0006Repository repo = new CE0006Repository(DBWork);
                    session.Result.etts = repo.PeopleCombo(chk_no);
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