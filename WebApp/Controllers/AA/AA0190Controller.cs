using JCLib.DB;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Data;
using System.IO;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web;
using WebApp.Models;
using WebApp.Repository.AA;
using NPOI.SS.Util;
using System.Collections.Generic;

namespace WebApp.Controllers.AA
{
    public class AA0190Controller : SiteBase.BaseApiController
    {
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");   
            var p1 = form.Get("p1");  
            var p2 = form.Get("p2");   
            var p3 = form.Get("p3");  
            var p4 = form.Get("p4");   
            var p5 = form.Get("p5");   
            var p6 = form.Get("p6");
            var p7 = form.Get("p7");
            var p8 = form.Get("p8");
            var p9 = form.Get("p9");
            var p10 = form.Get("p10");
            var p11 = form.Get("p11");
            var p12 = form.Get("p12");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));  
            var limit = int.Parse(form.Get("limit")); 
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0190Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, page, limit, sorters); 
                }
                catch (Exception e)
                {
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
                    AA0190Repository repo = new AA0190Repository(DBWork);
                    if (form.Get("MMCODE") != "")
                    {
                        string mmcode = form.Get("MMCODE").Substring(0, form.Get("MMCODE").Length - 1); // 去除前端傳進來最後一個逗號
                        string prqty = form.Get("PRQTY").Substring(0, form.Get("PRQTY").Length - 1);
                        string[] tmp_mmcode = mmcode.Split(',');
                        string[] tmp_prqty = prqty.Split(',');
                        string v_mat_class = "01";
                        string v_prno = string.Empty;
                        for (int i = 0; i < tmp_mmcode.Length; i++)
                        {
                            v_mat_class = repo.GetMatClass(tmp_mmcode[i]);
                            if (v_mat_class != "01")
                            {
                                MM_PR_M mmprm = new MM_PR_M();
                                mmprm.MAT_CLASS = v_mat_class;
                                v_prno = repo.GetPrno(mmprm.MAT_CLASS);
                                while (repo.CheckExistsMMPRM(v_prno) == true)
                                {
                                    System.Threading.Thread.Sleep(1000);
                                    v_prno = repo.GetPrno(mmprm.MAT_CLASS);
                                }
                                mmprm.PR_NO = v_prno;
                                mmprm.UPDATE_USER = User.Identity.Name;
                                mmprm.UPDATE_IP = DBWork.ProcIP;
                                repo.InsertMMPRM(mmprm);
                                MM_PR_D mmprd = new MM_PR_D();
                                mmprd.MAT_CLASS = v_mat_class;
                                mmprd.PR_NO = v_prno;
                                mmprd.MMCODE = tmp_mmcode[i];
                                mmprd.PR_QTY = int.Parse(tmp_prqty[i]);
                                mmprd.UPDATE_USER = User.Identity.Name;
                                mmprd.UPDATE_IP = DBWork.ProcIP;
                                repo.InsertMMPRD(mmprd);
                            }
                            else
                            {
                                MM_PR_M mmprm = new MM_PR_M();
                                v_prno = repo.GetPrno01();
                                mmprm.PR_NO = v_prno;
                                mmprm.UPDATE_USER = User.Identity.Name;
                                mmprm.UPDATE_IP = DBWork.ProcIP;
                                repo.InsertMMPRM01(mmprm);
                                MM_PR_D mmprd = new MM_PR_D();
                                mmprd.PR_NO = v_prno;
                                mmprd.MMCODE = tmp_mmcode[i];
                                mmprd.PR_QTY = int.Parse(tmp_prqty[i]);
                                mmprd.UPDATE_USER = User.Identity.Name;
                                mmprd.UPDATE_IP = DBWork.ProcIP;
                                repo.InsertMMPRD01(mmprd);
                            }
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
        public ApiResponse GetMMCodeCombo(FormDataCollection form)
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
                    AA0190Repository repo = new AA0190Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeCombo(p0, page, limit, "");
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetMatClassSubCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0190Repository(DBWork);
                    session.Result.etts = repo.GetMatClassSubCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetESourceCodeCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0190Repository(DBWork);
                    session.Result.etts = repo.GetESourceCodeCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetMContidCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0190Repository(DBWork);
                    session.Result.etts = repo.GetMContidCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        

        public ApiResponse GetOrderkindCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0190Repository(DBWork);
                    session.Result.etts = repo.GetOrderkindCombo();
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