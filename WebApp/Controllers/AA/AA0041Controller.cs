using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;

namespace WebApp.Controllers.AA
{
    public class AA0041Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse T1All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0041Repository(DBWork);
                    session.Result.etts = repo.GetT1All(p0, p1);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse T2All(FormDataCollection form)
        {
            var p0 = form.Get("p0");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0041Repository(DBWork);
                    session.Result.etts = repo.GetT2All(p0);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse T3All(FormDataCollection form)
        {
            var p0 = form.Get("p0");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0041Repository(DBWork);
                    session.Result.etts = repo.GetT3All(p0);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse T31All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0041Repository(DBWork);
                    session.Result.etts = repo.GetT31All(p0, p1, p2);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse T1Create(UR_INID ur_inid)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0041Repository(DBWork);
                    if (!repo.T1CheckExists(ur_inid.INID))
                    {
                        ur_inid.INID_OLD = ur_inid.INID;
                        ur_inid.CREATE_USER = User.Identity.Name;
                        ur_inid.UPDATE_USER = User.Identity.Name;
                        ur_inid.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.T1Create(ur_inid);
                        session.Result.etts = repo.T1Get(ur_inid.INID);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>責任中心代碼</span> 重複，請重新輸入。";
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
        public ApiResponse T2Create(UR_INID_GRP ur_inid_grp)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0041Repository(DBWork);
                    if (!repo.T2CheckExists(ur_inid_grp.GRP_NO))
                    {
                        ur_inid_grp.CREATE_USER = User.Identity.Name;
                        ur_inid_grp.UPDATE_USER = User.Identity.Name;
                        ur_inid_grp.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.T2Create(ur_inid_grp);
                        session.Result.etts = repo.T2Get(ur_inid_grp.GRP_NO);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>歸戶代碼</span> 重複，請重新輸入。";
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
        public ApiResponse T1Update(UR_INID ur_inid)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0041Repository(DBWork);
                    
                    if (ur_inid.INID == ur_inid.INID_O) //如果INID沒改
                    {
                        if (repo.T1CheckExists(ur_inid.INID_O))
                        {
                            ur_inid.UPDATE_USER = User.Identity.Name;
                            ur_inid.UPDATE_IP = DBWork.ProcIP;
                            session.Result.afrs = repo.T1Update(ur_inid);
                            session.Result.etts = repo.T1Get(ur_inid.INID);
                        }
                        else
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = "<span style='color:red'>責任中心代碼</span> 不存在。";
                        }
                    }
                    else //如果INID有改
                    {
                        if (!repo.T1CheckExists(ur_inid.INID)) //檢查新的INID有無重複
                        {
                            if (repo.T1CheckExists(ur_inid.INID_O))
                            {
                                ur_inid.UPDATE_USER = User.Identity.Name;
                                ur_inid.UPDATE_IP = DBWork.ProcIP;
                                session.Result.afrs = repo.T1Update(ur_inid);
                                session.Result.etts = repo.T1Get(ur_inid.INID);
                            }
                            else
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = "<span style='color:red'>責任中心代碼</span> 不存在。";
                            }
                        }
                        else
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = "<span style='color:red'>責任中心代碼</span> 重複，請重新輸入。";
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
        public ApiResponse T2Update(UR_INID_GRP ur_inid_grp)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0041Repository(DBWork);

                    if (repo.T2CheckExists(ur_inid_grp.GRP_NO))
                    {
                        ur_inid_grp.UPDATE_USER = User.Identity.Name;
                        ur_inid_grp.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.T2Update(ur_inid_grp);
                        session.Result.etts = repo.T2Get(ur_inid_grp.GRP_NO);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>歸戶代碼</span> 不存在。";
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
        public ApiResponse T1Delete(UR_INID ur_inid)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0041Repository(DBWork);
                    if (repo.T1CheckExists(ur_inid.INID))
                    {
                        ur_inid.UPDATE_USER = User.Identity.Name;
                        ur_inid.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.T1Delete(ur_inid);
                        session.Result.etts = repo.T1Get(ur_inid.INID);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>責任中心代碼</span> 不存在。";
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
        public ApiResponse T2Delete(UR_INID_GRP ur_inid_grp)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0041Repository(DBWork);
                    if (repo.T2CheckExists(ur_inid_grp.GRP_NO))
                    {
                        if (!repo.T2CheckRelExists(ur_inid_grp.GRP_NO))
                        {
                            ur_inid_grp.UPDATE_USER = User.Identity.Name;
                            ur_inid_grp.UPDATE_IP = DBWork.ProcIP;
                            session.Result.afrs = repo.T2Delete(ur_inid_grp);
                            session.Result.etts = repo.T2Get(ur_inid_grp.GRP_NO);
                        }
                        else
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = "歸戶代碼已被歸戶責任中心對照檔參用，不可刪除。";
                        }
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>歸戶代碼</span> 不存在。";
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
        public ApiResponse T3Delete(FormDataCollection form)
        {
            var p0 = form.Get("p0"); // selectArray;

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0041Repository(DBWork);
                    session.Result.afrs = repo.T3Delete(p0);

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
        public ApiResponse T31Add(FormDataCollection form)
        {
            var p0 = form.Get("p0"); // selectArray;

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0041Repository(DBWork);
                    session.Result.afrs = repo.T31Add(p0);

                    if (session.Result.afrs > 0)
                        DBWork.Commit();
                    else
                    {
                        session.Result.success = false;
                        DBWork.Rollback();
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
        public void T1Excel(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0041Repository(DBWork);

                    string fileName = form.Get("FN");

                    /* 匯出單一檔案(.xls)，內含一個工作表(sheet) */
                    using (var dataTable1 = repo.T1GetExcel(form.Get("p0"), form.Get("p1")))
                    {
                        JCLib.Excel.Export(fileName, dataTable1);
                    }


                    /* 匯出單一檔案(.xls)，內含多個工作表(sheet) 
                    using (var dataSet1 = new System.Data.DataSet())
                    {
                        var dataTable1 = repo.GetExcel(form.Get("p0"), form.Get("p1"));
                        dataTable1.TableName = "SheetName1";
                        dataSet1.Tables.Add(dataTable1);

                        var dataTable2 = repo.GetExcel(form.Get("p0"), form.Get("p1"));
                        dataTable2.TableName = "SheetName2"; //名稱不能重複
                        dataSet1.Tables.Add(dataTable2);

                        JCLib.Excel.Export("多Sheet匯出.xls", dataSet1);
                    }
                    */

                    /* 匯出多個xls檔案，包含在單一壓縮檔(.zip)
                    using (var dataSet2 = new System.Data.DataSet())
                    {
                        var dataTable1 = repo.GetExcel(form.Get("p0"), form.Get("p1"));
                        dataTable1.TableName = "FileName1";
                        dataSet2.Tables.Add(dataTable1);

                        var dataTable2 = repo.GetExcel(form.Get("p0"), form.Get("p1"));
                        dataTable2.TableName = "FileName2"; //名稱不能重複
                        dataSet2.Tables.Add(dataTable2);

                        JCLib.Excel.ExportZip("匯出壓縮檔.zip", dataSet2);
                    }
                    */

                    /* 匯出多個xls檔案，包含在單一壓縮檔(.zip) 
                     * 傳入DataTable與分群欄位，自動分群並打包下載 
                     * 壓縮檔內的Excel檔名為groupByField名稱[空白]groupByField值
                     */
                    /*
                    using (var dataTable1 = repo.GetExcel(form.Get("p0"), form.Get("p1")))
                    {
                        JCLib.Excel.ExportZip("匯出壓縮檔.zip", dataTable1, "紀錄更新人員");
                    }
                    */

                    /* 匯出多個xls檔案，包含在單一壓縮檔(.zip) 
                     * 傳入DataTable與分群欄位，自動分群並打包下載 
                     * 壓縮檔內的Excel檔名為groupByField名稱[空白]groupByField值
                     * 設定表頭以及表尾顯示
                     *
                    using (var dataTable1 = repo.GetExcel(form.Get("p0"), form.Get("p1")))
                    {
                        JCLib.Excel.ExportZip("匯出壓縮檔.zip", dataTable1, "紀錄更新人員", false, 
                            (dt) => {
                                //headerHandler: 返回Excel表首要顯示的文字
                                var rocYear = System.DateTime.Now.Year - 1911;
                                return string.Format("=這是表頭=\n民國{0}年統計", rocYear);
                            },
                            (dt) => {
                                //footerHandler: 返回Excel表尾要顯示的文字
                                var rowCount = dt.Rows.Count; //dt是分群後的資料(DataTable)
                                return string.Format("=這是表尾=\n總共{0}筆", rowCount); });
                    }
                    */

                }
                catch
                {
                    throw;
                }
            }
        }

        [HttpPost]
        public ApiResponse GetCombo(FormDataCollection form)
        {
            /*
            var grp_code = form.Get("GRP_CODE");
            var data_name = form.Get("DATA_NAME");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            */

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0041Repository repo = new AA0041Repository(DBWork);
                    session.Result.etts = repo.GetCombo();
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
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0041Repository repo = new AA0041Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeCombo(p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetGrpCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0041Repository repo = new AA0041Repository(DBWork);
                    session.Result.etts = repo.GetGrpCombo();
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