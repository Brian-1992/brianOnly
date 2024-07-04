using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Models;
using WebApp.Repository.B;

namespace WebApp.Controllers.B
{
    public class BA0007Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse getALL(FormDataCollection form)
        {
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BA0007Repository(DBWork);
                    session.Result.etts = repo.getAll(page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse CreateDTBAS(MM_PR_DTBAS mm_pr_dtbas)
        {
            using (WorkSession session = new WorkSession(this))
            {
                string rtnStr = chkRule(mm_pr_dtbas);
                if (rtnStr == "")
                {
                    var DBWork = session.UnitOfWork;
                    DBWork.BeginTransaction();
                    try
                    {
                        BA0007Repository repo = new BA0007Repository(DBWork);
                        mm_pr_dtbas.MMPRDTBAS_SEQ = repo.getSeq();
                        mm_pr_dtbas.CREATE_USER = User.Identity.Name;
                        mm_pr_dtbas.UPDATE_IP = DBWork.ProcIP;

                        // 若沒有值則代預設值
                        if (mm_pr_dtbas.DATEBAS == null)
                            mm_pr_dtbas.DATEBAS = "0";
                        if (mm_pr_dtbas.MTHBAS == null)
                            mm_pr_dtbas.MTHBAS = "0";
                        if (mm_pr_dtbas.LASTDELI_MTH == null)
                            mm_pr_dtbas.LASTDELI_MTH = "A";
                        if (mm_pr_dtbas.BEGINDATE == null)
                            mm_pr_dtbas.BEGINDATE = "0";
                        if (mm_pr_dtbas.ENDDATE == null)
                            mm_pr_dtbas.ENDDATE = "0";
                        if (mm_pr_dtbas.SUMDATE == null)
                            mm_pr_dtbas.SUMDATE = "0";

                        session.Result.afrs = repo.CreateDTBAS(mm_pr_dtbas);

                        DBWork.Commit();
                    }
                    catch
                    {
                        DBWork.Rollback();
                        throw;
                    }
                }
                else
                {
                    session.Result.afrs = 0;
                    session.Result.success = false;
                    session.Result.msg = rtnStr;
                }
                
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse UpdateDTBAS(MM_PR_DTBAS mm_pr_dtbas)
        {
            using (WorkSession session = new WorkSession(this))
            {
                string rtnStr = chkRule(mm_pr_dtbas);
                if (rtnStr == "")
                {
                    var DBWork = session.UnitOfWork;
                    DBWork.BeginTransaction();
                    try
                    {
                        BA0007Repository repo = new BA0007Repository(DBWork);
                        mm_pr_dtbas.UPDATE_USER = User.Identity.Name;
                        mm_pr_dtbas.UPDATE_IP = DBWork.ProcIP;

                        // 若沒有值則代預設值
                        if (mm_pr_dtbas.DATEBAS == null)
                            mm_pr_dtbas.DATEBAS = "0";
                        if (mm_pr_dtbas.MTHBAS == null)
                            mm_pr_dtbas.MTHBAS = "0";
                        if (mm_pr_dtbas.LASTDELI_MTH == null)
                            mm_pr_dtbas.LASTDELI_MTH = "A";
                        if (mm_pr_dtbas.BEGINDATE == null)
                            mm_pr_dtbas.BEGINDATE = "0";
                        if (mm_pr_dtbas.ENDDATE == null)
                            mm_pr_dtbas.ENDDATE = "0";
                        if (mm_pr_dtbas.SUMDATE == null)
                            mm_pr_dtbas.SUMDATE = "0";

                        session.Result.afrs = repo.UpdateDTBAS(mm_pr_dtbas);
                        DBWork.Commit();
                        session.Result.success = true;
                    }
                    catch
                    {
                        DBWork.Rollback();
                        throw;
                    }
                }
                else
                {
                    session.Result.afrs = 0;
                    session.Result.success = false;
                    session.Result.msg = rtnStr;
                }
                
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse DeleteDTBAS(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var seq = form.Get("seq");
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BA0007Repository repo = new BA0007Repository(DBWork);
                    session.Result.afrs = repo.DeleteDTBAS(seq);
                    session.Result.success = true;
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

        public string chkRule(MM_PR_DTBAS mm_pr_dtbas)
        {
            string rtnStr = "";
            if (mm_pr_dtbas.MMPRDTBAS_SEQ == "系統自編")
                mm_pr_dtbas.MMPRDTBAS_SEQ = "";
            if (mm_pr_dtbas.M_STOREID == "0")
            {
                // 申購生效起日：>= 1 and <= 28
                if (!(Convert.ToInt32(mm_pr_dtbas.BEGINDATE) >= 1 && Convert.ToInt32(mm_pr_dtbas.BEGINDATE) <= 28))
                    rtnStr = "<span style='color:red'>申購生效起日</span>大於等於1且小於等於28。";
                // 申購生效迄日：>=1 and <=28
                else if (!(Convert.ToInt32(mm_pr_dtbas.ENDDATE) >= 1 && Convert.ToInt32(mm_pr_dtbas.ENDDATE) <= 28))
                    rtnStr = "<span style='color:red'>申購生效迄日</span>大於等於1且小於等於28。";
                // 彙總日：>=1 and <=28
                else if (!(Convert.ToInt32(mm_pr_dtbas.SUMDATE) >= 1 && Convert.ToInt32(mm_pr_dtbas.SUMDATE) <= 28))
                    rtnStr = "<span style='color:red'>彙總日</span>大於等於1且小於等於28。";
                // 最後進貨日：>=1 and <=28
                else if (!(Convert.ToInt32(mm_pr_dtbas.LASTDELI_DT) >= 1 && Convert.ToInt32(mm_pr_dtbas.LASTDELI_DT) <= 28))
                    rtnStr = "<span style='color:red'>最後進貨日</span>大於等於1且小於等於28。";
                // 申購生效迄日 >= 申購生效起日
                else if (!(Convert.ToInt32(mm_pr_dtbas.ENDDATE) >= Convert.ToInt32(mm_pr_dtbas.BEGINDATE)))
                    rtnStr = "<span style='color:red'>申購生效迄日</span>需大於等於<span style='color:red'>申購生效起日</span>。";
                // 彙總日 > 申購生效迄日
                else if (!(Convert.ToInt32(mm_pr_dtbas.SUMDATE) > Convert.ToInt32(mm_pr_dtbas.ENDDATE)))
                    rtnStr = "<span style='color:red'>彙總日</span>需大於<span style='color:red'>申購生效迄日</span>。";
                else
                {
                    // 最後進貨日
                    if (mm_pr_dtbas.LASTDELI_MTH == "A")
                    {
                        // 若最後進貨日為當月,則最後進貨日 > 彙總日
                        if (!(Convert.ToInt32(mm_pr_dtbas.LASTDELI_DT) > Convert.ToInt32(mm_pr_dtbas.SUMDATE)))
                            rtnStr = "<span style='color:red'>最後進貨日</span>需大於<span style='color:red'>彙總日</span>。";
                    }

                    // [申購生效起日~申購生效迄日~彙總日]是一個區段，每一個區段不可與其它區段重疊
                    using (WorkSession session = new WorkSession(this))
                    {
                        UnitOfWork DBWork = session.UnitOfWork;
                        var repo = new BA0007Repository(DBWork);
                        
                        bool[] bArray = new bool[28];
                        for (int i = 0; i < 28; i++)
                            bArray[i] = false; // 建立一個0~27皆為false的array

                        // 將現有資料的申購生效起日~彙總日設為true
                        foreach (MM_PR_DTBAS dtbas in repo.GetDTBASList(mm_pr_dtbas.MMPRDTBAS_SEQ))
                        {
                            // 若抓到的值超過28則限縮到28
                            if (Convert.ToInt32(dtbas.SUMDATE) > 28)
                                dtbas.SUMDATE = "28";
                            for (int i = Convert.ToInt32(dtbas.BEGINDATE) - 1; i <= Convert.ToInt32(dtbas.SUMDATE) - 1; i++)
                                bArray[i] = true;
                        }

                        // 檢查本次儲存的申購生效起日~彙總日在bool array中是否有任一日為true
                        for (int i = Convert.ToInt32(mm_pr_dtbas.BEGINDATE) - 1; i <= Convert.ToInt32(mm_pr_dtbas.SUMDATE) - 1; i++)
                        {
                            if (bArray[i] == true)
                            {
                                rtnStr = "<span style='color:red'>申購生效起日~彙總日</span>區段不可與其它區段重疊。";
                                break;
                            }
                        } 
                    }
                }
            }
            else if (mm_pr_dtbas.M_STOREID == "1")
            {
                using (WorkSession session = new WorkSession(this))
                {
                    UnitOfWork DBWork = session.UnitOfWork;
                    var repo = new BA0007Repository(DBWork);

                    // 庫備識別碼=庫備時，不同物料分類各只能有1筆
                    if (repo.ChceckDataExist("1", mm_pr_dtbas.MAT_CLSID, mm_pr_dtbas.MMPRDTBAS_SEQ))
                        rtnStr = "<span style='color:red'>庫備識別碼=庫備</span>時，不同物料分類各只能有1筆。";
                }  
            }
            return rtnStr;
        }
    }
}