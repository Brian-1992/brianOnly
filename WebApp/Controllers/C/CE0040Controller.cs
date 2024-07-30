using JCLib.DB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http.Formatting;
using System.Web.Http;
using WebApp.Models;
using WebApp.Models.C;
using WebApp.Repository.C;

namespace WebApp.Controllers.C
{
    public class CE0040Controller : SiteBase.BaseApiController
    {

        // 查詢
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            bool p2 = form.Get("p2") == "true";
            bool p3 = form.Get("p3") == "true";
            bool p4 = form.Get("p4") == "true";
            string menuLink = form.Get("menuLink") == null ? "" : form.Get("menuLink");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0040Repository(DBWork);
                    session.Result.etts = repo.GetAllM(p0, p1, p2,p3,p4, menuLink, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse AllD(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0040Repository(DBWork);

                    if (p0 == "")
                    {
                        return session.Result;
                    }

                    session.Result.etts = repo.GetAllD(p0, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //匯出
        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            string CHK_NO = form.Get("CHK_NO");
            string excelName = CHK_NO + "_" + (Convert.ToInt32(DateTime.Now.ToString("yyyy")) - 1911).ToString() + DateTime.Now.ToString("MMddhhmmss") + ".xlsx";

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0040Repository repo = new CE0040Repository(DBWork);
                    JCLib.Excel.Export(excelName, repo.GetExcel(CHK_NO));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }


        }
        //盤點年月(combo)
        [HttpPost]
        public ApiResponse GetSetymCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0040Repository repo = new CE0040Repository(DBWork);
                    session.Result.etts = repo.GetSetymCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //庫房代碼(combo)
        [HttpPost]
        public ApiResponse GetWhNoCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0040Repository repo = new CE0040Repository(DBWork);
                    session.Result.etts = repo.GetWhNoCombo(User.Identity.Name);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //盤點單開立完成時間
        [HttpPost]
        public ApiResponse GetSetAtime(FormDataCollection form)
        {
            var p0 = form.Get("set_ym");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0040Repository repo = new CE0040Repository(DBWork);
                    session.Result.etts = repo.GetSetAtime(p0);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //盤點單結束日期
        [HttpPost]
        public ApiResponse GetChkEndtime(FormDataCollection form)
        {
            var p0 = form.Get("set_ym");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0040Repository repo = new CE0040Repository(DBWork);
                    session.Result.etts = repo.GetChkEndtime(p0);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //全院盤點結束時間
        [HttpPost]
        public ApiResponse GetChkClosetime(FormDataCollection form)
        {
            var p0 = form.Get("set_ym");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0040Repository repo = new CE0040Repository(DBWork);
                    session.Result.etts = repo.GetChkClosetime(p0);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //檢查是否有庫房未盤點
        [HttpPost]
        public ApiResponse CheckChkClosetime(FormDataCollection form)
        {
            var p0 = form.Get("set_ym");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0040Repository repo = new CE0040Repository(DBWork);
                    session.Result.etts = repo.CheckChkClosetime(p0);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //尚未完成盤點的項目
        [HttpPost]
        public ApiResponse ShowChkData(FormDataCollection form)
        {
            var p0 = form.Get("set_ym");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0040Repository repo = new CE0040Repository(DBWork);
                    session.Result.etts = repo.ShowChkData(p0);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //設定盤點結束時間
        [HttpPost]
        public ApiResponse SetChkEndTime(CE0040 CE0040)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    CE0040.UPDATE_USER = DBWork.UserInfo.UserId;
                    CE0040.UPDATE_IP = DBWork.ProcIP;

                    var repo = new CE0040Repository(DBWork);

                    session.Result.afrs = repo.SetChkEndTime(CE0040);
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
        //結束全院盤點
        [HttpPost]
        public ApiResponse SetChkClosetime(FormDataCollection form)
        {
            var set_ym = form.Get("p0");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0040Repository(DBWork);

                    //取得全部盤點單(結束全院盤點)
                    IEnumerable<CE0040> datas_1 = repo.GetChkMast(set_ym);

                    foreach (CE0040 item_1 in datas_1)
                    {
                        item_1.CREATE_USER = DBWork.UserInfo.UserId;
                        item_1.UPDATE_USER = DBWork.UserInfo.UserId;
                        item_1.UPDATE_IP = DBWork.ProcIP;
                        item_1.WH_NO = item_1.CHK_WH_NO;

                        //更新CHK_DETAIL狀態(結束全院盤點)
                        var UpdateChkDetailNum = repo.UpdateChkDetail(item_1);

                        //新增至CHK_DETAILTOT(結束全院盤點)
                        var AddChkDetailtotNum = repo.AddChkDetailtot(item_1);

                        //取得明細與現存量(結束全院盤點)
                        IEnumerable<CE0040> datas_2 = repo.GetChkDetail(item_1.CHK_NO);

                        foreach (CE0040 item_2 in datas_2)
                        {
                            item_1.ALTERED_USE_QTY = item_2.ALTERED_USE_QTY;
                            item_1.STORE_QTYC = item_2.STORE_QTYC;
                            item_1.USE_QTY = item_2.USE_QTY;
                            item_1.USE_QTY_AF_CHK = item_2.USE_QTY_AF_CHK;
                            item_1.INVENTORY = item_2.INVENTORY;
                            item_1.CHK_QTY = item_2.CHK_QTY;
                            item_1.MMCODE = item_2.MMCODE;
                            item_1.INV_QTY = item_2.INV_QTY;
                            item_1.SET_YM = set_ym;



                            //修改MI_WINVMON(結束全院盤點)
                            var UpdateMiWinvmonNum = repo.UpdateMiWinvmon(item_1);
                            //修改MI_WHINV(結束全院盤點)
                            var UpdateMiWhinvNum = repo.UpdateMiWhinv(item_1);
                            //新增MI_WHTRNS(結束全院盤點)
                            var AddMiWhtrnsNum = repo.AddMiWhtrns(item_1);
                        }
                        //新增至新增MI_WHTRNS(結束全院盤點)
                        var UpdateChkMastNum = repo.UpdateChkMast(item_1);
                    }

                    //更新CHK_MNSET closetime(結束全院盤點)
                    var UpdateChkMnsetNum = repo.UpdateChkMnset(set_ym, DBWork.UserInfo.UserId, DBWork.ProcIP);

                    DBWork.Commit();
                }
                catch (Exception e)
                {
                    var aaa = e.Message.ToString();
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }

    }
}