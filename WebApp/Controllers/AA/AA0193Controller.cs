using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System;

namespace WebApp.Controllers.AA
{
    public class AA0193Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
        {
            var p1 = form.Get("p1");
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
                    var repo = new AA0193Repository(DBWork);
                    session.Result.etts = repo.GetAllM(p1, p2, User.Identity.Name, page, limit, sorters);
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
                    var repo = new AA0193Repository(DBWork);
                    session.Result.etts = repo.GetAllD(p0, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse UpdateM(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0193Repository(DBWork);
                    var docno = form.Get("DOCNO");
                    var mmcode = form.Get("MMCODE");
                    var iswas = form.Get("ISWAS");
                    var ackqty = form.Get("ACKQTY");
                    string update_user = User.Identity.Name;
                    string update_ip = DBWork.ProcIP;
                    session.Result.afrs = repo.UpdateM(docno, mmcode, iswas, ackqty, update_user, update_ip);
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
        public ApiResponse Apply(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0193Repository repo = new AA0193Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        string update_user = User.Identity.Name;
                        string update_ip = DBWork.ProcIP;

                        session.Result.afrs = repo.Apply(tmp, update_user, update_ip);

                        //if (repo.CheckExistsDN(tmp[i]))
                        //{
                        //    session.Result.afrs = 0;
                        //    session.Result.success = false;
                        //    session.Result.msg = "<span style='color:red'>補藥通知單編號(" + tmp[i] + ")明細尚有核可補藥量為0</span>不得核可。" + "<br/>";
                        //    DBWork.Rollback();
                        //    return session.Result;
                        //}


                        //if (!repo.CheckExists(v_docno)) // 新增前檢查主鍵是否已存在
                        //{
                        //    //C.更新DGMISS
                        //    repo.ApplyD(tmp[i], v_docno, update_user, update_ip);
                        //    //c.新增ME_DOCM
                        //    repo.InsertMEDOCM(tmp[i], v_docno, update_user, update_ip);
                        //    //d.新增ME_DOCD
                        //    repo.InsertMEDOCD(tmp[i], v_docno, update_user, update_ip);
                        //}
                        //else
                        //{
                        //    session.Result.afrs = 0;
                        //    session.Result.success = false;
                        //    session.Result.msg = "<span style='color:red'>申請單號</span>重複，請重新嘗試。";
                        //}
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

        [HttpPost]
        public ApiResponse GetInidCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0193Repository repo = new AA0193Repository(DBWork);
                    session.Result.etts = repo.GetInidCombo();
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