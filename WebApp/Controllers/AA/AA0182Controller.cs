using JCLib.DB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using WebApp.Models.AA;
using WebApp.Repository.AA;

namespace WebApp.Controllers.AA
{
    public class AA0182Controller : SiteBase.BaseApiController
    {
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");    // 庫房代碼
            var p1 = form.Get("p1");    // 物料分類
            var p2 = form.Get("p2");    // 院內碼
            var d0 = form.Get("d0");    // 效期起
            var d1 = form.Get("d1");    // 效期訖
            var p3 = form.Get("p3");  //效期9991231 是否顯示

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));  //start:0
            var limit = int.Parse(form.Get("limit"));  //pageSize=20
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0182Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1, p2, d0, d1, p3,User.Identity.Name, page, limit, sorters); //撈出object
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetWhCombo(FormDataCollection form)
        {
            string menuLink= form.Get("menuLink") == null? "": form.Get("menuLink");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0182Repository repo = new AA0182Repository(DBWork);
                    session.Result.etts = repo.GetWhCombo(User.Identity.Name, menuLink);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMatclassCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");    // 庫房代碼
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0182Repository repo = new AA0182Repository(DBWork);
                    string hospcode = repo.GetHospCode();
                    session.Result.etts = repo.GetMatClassCombo(User.Identity.Name, hospcode,p0);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMMCodeCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var mat_class = form.Get("MAT_CLASS");
            var wh_no = form.Get("WH_NO");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0182Repository repo = new AA0182Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeCombo(p0, mat_class, wh_no, page, limit, "");
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

        //匯出
        [HttpPost]
        public ApiResponse GetExcel(FormDataCollection form)
        {
            var p0 = form.Get("p0");    // 庫房代碼
            var p1 = form.Get("p1");    // 物料分類
            var p2 = form.Get("p2");    // 院內碼
            var d0 = form.Get("d0");    // 效期起
            var d1 = form.Get("d1");    // 效期訖
            var p3 = form.Get("p3");  //效期9991231 是否顯示

            string fileName = string.Format("批號效期維護_{0}.xlsx", DateTime.Now.ToString("yyyyMMddHHmmss"));

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0182Repository repo = new AA0182Repository(DBWork);
                    JCLib.Excel.Export(fileName, repo.GetExcel(p0, p1, p2, d0, d1,p3, User.Identity.Name));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetExpDate()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0182Repository repo = new AA0182Repository(DBWork);
                    session.Result.etts = repo.GetExpDate();
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse SetQty(FormDataCollection form)
        {

            IEnumerable<AA0182> list = JsonConvert.DeserializeObject<IEnumerable<AA0182>>(form.Get("list"));

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0182Repository(DBWork);

                    string check_flag = "Y";
                    string msg = "";
                    List<string> whmmcode_list = new List<string>();
                    foreach (AA0182 item in list)
                    {
                        // 建立本次更新的WH_NO+MMCODE清單
                        if (!whmmcode_list.Contains(item.WH_NO + "^" + item.MMCODE))
                            whmmcode_list.Add(item.WH_NO + "^" + item.MMCODE);

                        item.UPDATE_USER = DBWork.UserInfo.UserId;
                        item.UPDATE_IP = DBWork.ProcIP;
                        // 修改MI_WEXPINV
                        repo.UpdateWexpinv(item.WH_NO, item.MMCODE, item.LOT_NO, item.EXP_DATE, item.INV_QTY, item.UPDATE_USER, item.UPDATE_IP);
                    }

                    // 檢查有異動的院內碼,目前的批號存量和跟庫房存量是否一致
                    foreach (string whmmcode in whmmcode_list)
                    {
                        string pWhno = whmmcode.Split('^')[0];
                        string pMmcode = whmmcode.Split('^')[1];

                        if (repo.GetWexpinvSum(pWhno, pMmcode) != repo.GetWhinvSum(pWhno, pMmcode))
                        {
                            msg = "庫房:" + pWhno + " 院內碼:" + pMmcode + ", 批號存量總和與庫房存量不一致, 請重新確認!";
                            check_flag = "N";
                        }
                    }

                    if (check_flag == "Y")
                    {
                        DBWork.Commit();
                    }
                    else
                    {
                        session.Result.success = false;
                        session.Result.msg = msg;
                        DBWork.Rollback();
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

        // 新增
        [HttpPost]
        public ApiResponse Create(AA0182 aa0182)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0182Repository(DBWork);

                    if (!repo.CheckWhnoExists(aa0182.WH_NO))
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>庫房代碼</span>不存在，請重新輸入。";

                        return session.Result;
                    }

                    if (!repo.CheckMmcodeExists(aa0182.MMCODE))
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>院內碼</span>不存在，請重新輸入。";

                        return session.Result;
                    }

                    if (!repo.CheckExists(aa0182.WH_NO, aa0182.MMCODE, aa0182.LOT_NO, aa0182.EXP_DATE)) // 新增前檢查主鍵是否已存在
                    {
                        aa0182.EXP_DATE = DateTime.Parse(aa0182.EXP_DATE).ToString("yyyy-MM-dd");
                        aa0182.UPDATE_USER = User.Identity.Name;
                        aa0182.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.Create(aa0182);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "項目已存在，請重新輸入。";
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

        // 刪除
        [HttpPost]
        public ApiResponse Delete(AA0182 aa0182)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0182Repository(DBWork);
                    if (repo.CheckExists(aa0182.WH_NO, aa0182.MMCODE, aa0182.LOT_NO, aa0182.EXP_DATE))
                    {
                        if (repo.GetWexpinv(aa0182.WH_NO, aa0182.MMCODE, aa0182.LOT_NO, aa0182.EXP_DATE) == 0)
                        {
                            session.Result.afrs = repo.Delete(aa0182.WH_NO, aa0182.MMCODE, aa0182.LOT_NO, aa0182.EXP_DATE);
                        }
                        else
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = "批號存量不為0，不可刪除。";
                        }
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>項目</span>不存在。";
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
    }
}