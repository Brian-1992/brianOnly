using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.C;
using WebApp.Models;
using System.Data;
using System.Collections.Generic;

namespace WebApp.Controllers.C
{
    public class CE0028Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0028Repository(DBWork);
                    var CE0028_FA = repo.FindAll(p1, p2, p3);
                    var CE0028_F3 = repo.FindLevel3(p1, p2, p3);
                    List<CE0028> CE0028_A_list = new List<CE0028>();


                    foreach (CE0028 CE0028_A in CE0028_FA)
                    {
                        foreach (CE0028 CE0028_3 in CE0028_F3)
                        {
                            if (CE0028_3.MMCODE_3 == CE0028_A.MMCODE_A)
                            {
                                CE0028_A.CHK_QTY3_A = CE0028_3.CHK_QTY_3;
                                if (CE0028_A.CHK_QTY3_A != string.Empty && CE0028_A.CHK_QTY3_A != null) {
                                    CE0028_A.GAP_T_A = (int.Parse(CE0028_A.CHK_QTY3_A) - int.Parse(CE0028_A.STORE_QTY_A)).ToString();
                                }
                                else{
                                    CE0028_A.GAP_T_A = string.Empty;
                                }
                                
                                CE0028_A.STATUS_TOT_A = "3";
                                CE0028_A.CHK_NO_A = CE0028_3.CHK_NO_3;
                            }
                        }
                        if (CE0028_A.STATUS_TOT_A == "3")
                        {
                            CE0028_A.CHK_QTY_A = CE0028_A.CHK_QTY3_A;
                        }
                        else if (CE0028_A.STATUS_TOT_A == "2")
                        {
                            CE0028_A.CHK_QTY_A = CE0028_A.CHK_QTY2_A;
                        }
                        else if (CE0028_A.STATUS_TOT_A == "1")
                        {
                            CE0028_A.CHK_QTY_A = CE0028_A.CHK_QTY1_A;
                        }

                        CE0028_A_list.Add(CE0028_A);
                    }

                    session.Result.etts = CE0028_A_list;
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
        public string GetCHK_STATUS(FormDataCollection form)
        {
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");

            var CHK_STATUS = "";

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0028Repository repo = new CE0028Repository(DBWork);
                    CHK_STATUS = repo.GetCHK_STATUS(p1, p2, p3);
                }
                catch
                {
                    throw;
                }
                return CHK_STATUS;
            }
        }

        //駁回
        [HttpPost]
        public ApiResponse GoBack(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0028Repository(DBWork);
                    string CHK_NO_A = form.Get("CHK_NO_A").Substring(0, form.Get("CHK_NO_A").Length - 1); // 去除前端傳進來最後一個逗號
                    string MMCODE_A = form.Get("MMCODE_A").Substring(0, form.Get("MMCODE_A").Length - 1); // 去除前端傳進來最後一個逗號

                    string[] TMP_CHK_NO_A = CHK_NO_A.Split(',');
                    string[] TMP_MMCODE_A = MMCODE_A.Split(',');

                    for (int i = 0; i < TMP_CHK_NO_A.Length; i++)
                    {
                        CHK_MAST mast = repo.GetChkMast(TMP_CHK_NO_A[i]);
                        if (mast.CHK_STATUS != "2")
                        {
                            session.Result.success = false;
                            session.Result.msg = "盤點單狀態已變更，請重新查詢";
                            return session.Result;
                        }

                        session.Result.afrs = repo.GoBack_1(TMP_CHK_NO_A[i], TMP_MMCODE_A[i]);
                        session.Result.afrs = repo.GoBack_2(TMP_CHK_NO_A[i]);
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