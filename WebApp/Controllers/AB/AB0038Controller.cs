using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AB;
using WebApp.Models;
using System.Collections.Generic;
using System.Web;
using NPOI.SS.UserModel;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using System.Data;
using Newtonsoft.Json;

namespace WebApp.Controllers.AB
{
    public class AB0038Controller : SiteBase.BaseApiController
    {
        //一進入程式，將TEMP檔清空
        [HttpPost]
        public ApiResponse DeleteTemp()
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0038Repository(DBWork);

                    session.Result.afrs = repo.DeleteTemp(DBWork.ProcIP);

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

        //一進入程式，將預設欄位匯入TEMP檔
        public ApiResponse CreateTemp()
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0038Repository(DBWork);

                    session.Result.afrs = repo.CreateTemp(DBWork.ProcIP);

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

        //欄位設定左視窗
        [HttpPost]
        public ApiResponse GetColList(FormDataCollection form)
        {
            var P0 = form.Get("P0");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0038Repository(DBWork);
                    session.Result.etts = repo.GetColList(DBWork.ProcIP, P0, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //欄位設定右視窗
        [HttpPost]
        public ApiResponse GetDefaultList(FormDataCollection form)
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
                    var repo = new AB0038Repository(DBWork);
                    session.Result.etts = repo.GetDefaultList(DBWork.ProcIP, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //加入欄位
        [HttpPost]
        public ApiResponse JoinCol(FormDataCollection form)
        {
            var SEQ = form.Get("SEQ");
            var ENAME = form.Get("ENAME");
            var NEWSEQ = "";
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0038Repository(DBWork);
                    NEWSEQ = repo.GetNewSeq(DBWork.ProcIP);//取得目前欲新增的SEQ
                    if (SEQ == NEWSEQ)//若該項目是下一號，更改FLAG即可
                    {
                        session.Result.afrs = repo.JoinCol(DBWork.ProcIP, ENAME, SEQ, NEWSEQ);//將要新增的項目改為新SEQ及FLAG
                    }
                    else
                    {
                        session.Result.afrs = repo.SEQ1000(DBWork.ProcIP, NEWSEQ);//先將該序號原本的項目SEQ設為1000
                        session.Result.afrs = repo.JoinCol(DBWork.ProcIP, ENAME, SEQ, NEWSEQ);//將要新增的項目改為新SEQ及FLAG
                        session.Result.afrs = repo.ChangeSeq(DBWork.ProcIP, ENAME, SEQ);//將原本的項目改回被替換的SEQ
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

        //刪除欄位
        [HttpPost]
        public ApiResponse DelCol(FormDataCollection form)
        {
            var SEQ = form.Get("SEQ");
            var ENAME = form.Get("ENAME");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0038Repository(DBWork);
                    session.Result.afrs = repo.SEQ1000_D(DBWork.ProcIP, SEQ); //將該項目的SEQ設為1000
                    session.Result.afrs = repo.SetAllSeq(DBWork.ProcIP, SEQ);//將該序號之後的所有SEQ - 1
                    session.Result.afrs = repo.SetLastSeq(DBWork.ProcIP); //將該項目設為最後一個SEQ

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

        //欄位上移
        [HttpPost]
        public ApiResponse UpCol(FormDataCollection form)
        {
            var SEQ = form.Get("SEQ");
            var ENAME = form.Get("ENAME");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0038Repository(DBWork);
                    session.Result.afrs = repo.SEQ1000_D(DBWork.ProcIP, SEQ); //將該項目的SEQ設為1000
                    session.Result.afrs = repo.SetUpSeq_1(DBWork.ProcIP, SEQ); //將該項目的上一項SEQ + 1
                    session.Result.afrs = repo.SetUpSeq_2(DBWork.ProcIP, SEQ); //將該項目的SEQ - 1

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

        //欄位下移
        [HttpPost]
        public ApiResponse DownCol(FormDataCollection form)
        {
            var SEQ = form.Get("SEQ");
            var ENAME = form.Get("ENAME");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0038Repository(DBWork);
                    session.Result.afrs = repo.SEQ1000_D(DBWork.ProcIP, SEQ); //將該項目的SEQ設為1000
                    session.Result.afrs = repo.SetDownSeq_1(DBWork.ProcIP, SEQ); //將該項目的下一項SEQ - 1
                    session.Result.afrs = repo.SetDownSeq_2(DBWork.ProcIP, SEQ); //將該項目的SEQ + 1

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

        public class AllGrids
        {
            public List<AB0038> Grid_T = new List<AB0038>(); //暫存資料
            public List<AB0038> Grid_1 = new List<AB0038>(); //比對到之資料
            public List<AB0038> Grid_2 = new List<AB0038>(); //比對訊息(資料檢核有誤)
            public List<AB0038> Grid_3 = new List<AB0038>(); //比對訊息(無異動)
            public List<AB0038> Grid_4 = new List<AB0038>(); //上傳檔案資料
            public List<AB0038> Grid_5 = new List<AB0038>(); //載入訊息
            public List<AB0038> Grid_6 = new List<AB0038>(); //比對資訊(查詢無資料)
        }

        //匯入檢核
        [HttpPost]
        public ApiResponse LoadingCheck()
        {
            using (WorkSession session = new WorkSession(this))
            {
                AllGrids AllGrid = new AllGrids();
                AB0038 DATA_T = new AB0038();
                AB0038 DATA_1 = new AB0038();
                AB0038 DATA_2 = new AB0038();
                AB0038 DATA_3 = new AB0038();
                AB0038 DATA_4 = new AB0038();
                AB0038 DATA_5 = new AB0038();
                AB0038 DATA_6 = new AB0038();
                List<AllGrids> lists = new List<AllGrids>();

                #region 各種參數
                bool load_check = true; //載入格式是否成功
                int c = 0;
                string[] ColCname = new string[300];
                string[] ColEname = new string[300];
                #endregion

                UnitOfWork DBWork = session.UnitOfWork;
                var HttpPostedFile = HttpContext.Current.Request.Files["file"];
                IWorkbook workBook;

                try
                {
                    AB0038Repository repo = new AB0038Repository(DBWork);

                    #region 檢查檔案格式，讀取xls或xlsx檔
                    if (Path.GetExtension(HttpPostedFile.FileName).ToLower() == ".xls")
                    {
                        workBook = new HSSFWorkbook(HttpPostedFile.InputStream); //讀取xls檔
                    }
                    else if (Path.GetExtension(HttpPostedFile.FileName).ToLower() == ".xlsx")
                    {
                        workBook = new XSSFWorkbook(HttpPostedFile.InputStream); //讀取xlsx檔
                    }
                    else
                    {
                        workBook = null;
                        DATA_5.LOAD_MSG = "檔案格式不符，請選擇.xls或.xlsx檔";
                        load_check = false;
                        AllGrid.Grid_5.Add(DATA_5);
                        lists.Add(AllGrid);
                    }
                    #endregion

                    //檔案類型正確
                    if (load_check)
                    {
                        var sheet = workBook.GetSheetAt(0); //讀取EXCEL的第一個分頁
                        IRow headerRow = sheet.GetRow(0); //由第一列取標題做為欄位名稱
                        int cellCount = headerRow.LastCellNum; //欄位數目

                        #region 檢查載入格式
                        //取得已設定的欄位
                        var AllCols = repo.GetCol(DBWork.ProcIP);
                        foreach (AB0038VM AllCol in AllCols)
                        {
                            ColCname[c] = AllCol.CNAME;
                            ColEname[c] = AllCol.ENAME;
                            c++;
                        }

                        if (cellCount != c)
                        {
                            DATA_5.LOAD_MSG = "載入的檔案欄位數目與設定欄位不符";
                            load_check = false;
                            AllGrid.Grid_5.Add(DATA_5);
                            lists.Add(AllGrid);
                        }
                        else
                        {
                            for (int i = 0; i < cellCount; i++)
                            {
                                load_check = headerRow.GetCell(i).ToString() == ColCname[i] ? true : false;
                                if (!load_check)
                                {
                                    DATA_5.LOAD_MSG = "載入的檔案欄位順序或名稱與設定欄位不符，須完全一樣";
                                    AllGrid.Grid_5.Add(DATA_5);
                                    lists.Add(AllGrid);
                                    break;
                                }
                            }
                        }
                        #endregion

                        #region 檔案格式正確，開始檢查各欄位
                        if (load_check)
                        {
                            DATA_5.LOAD_MSG = "檔案載入成功";
                            AllGrid.Grid_5.Add(DATA_5);

                            #region 上傳的原始資料 Grid_4
                            for (int i = 1; i <= sheet.LastRowNum; i++)
                            {
                                DATA_4 = new AB0038();
                                for (int j = 0; j < cellCount; j++)
                                {
                                    if (sheet.GetRow(i).GetCell(j) != null)
                                    {
                                        DATA_4.GetType().GetProperty(ColEname[j]).SetValue(DATA_4, sheet.GetRow(i).GetCell(j).ToString(), null);
                                    }
                                    else
                                    {
                                        DATA_4.GetType().GetProperty(ColEname[j]).SetValue(DATA_4, "", null);
                                    }
                                }
                                AllGrid.Grid_4.Add(DATA_4);
                            }
                            #endregion

                            #region 檢查院內代碼是否存在於現有資料中 Grid_1、Grid_6
                            for (int i = 1; i <= sheet.LastRowNum; i++)
                            {
                                DATA_1 = new AB0038();
                                DATA_6 = new AB0038();
                                if (repo.Check_HIS_BASORDD(AllGrid.Grid_4[i - 1].ORDERCODE) &&
                                    repo.Check_HIS_BASORDM(AllGrid.Grid_4[i - 1].ORDERCODE) &&
                                    repo.Check_HIS_STKDMIT(AllGrid.Grid_4[i - 1].ORDERCODE))
                                {
                                    DATA_1 = AllGrid.Grid_4[i - 1];
                                    AllGrid.Grid_1.Add(DATA_1);
                                    AllGrid.Grid_T.Add(DATA_1);
                                }
                                else
                                {
                                    DATA_6 = AllGrid.Grid_4[i - 1];
                                    AllGrid.Grid_6.Add(DATA_6);
                                }
                            }
                            #endregion

                            #region 比對無異動訊息 Grid_3
                            for (int i = 0; i < AllGrid.Grid_1.Count; i++)
                            {
                                DATA_3 = new AB0038();
                                DATA_3 = AllGrid.Grid_1[i];
                                if (repo.Check_NO_Change(DATA_3))
                                {
                                    AllGrid.Grid_3.Add(DATA_3);
                                }
                            }
                            #endregion

                            #region 資料檢核有誤 Grid_2
                            for (int i = 0; i < AllGrid.Grid_T.Count; i++)
                            {
                                DATA_2 = new AB0038();
                                DATA_2 = AllGrid.Grid_T[i];

                                for (int j = 0; j < cellCount; j++)
                                {
                                    //檢核數字欄位及字串長度
                                    if (repo.Check_NUMBER(ColEname[j]))
                                    {
                                        double chk_n;
                                        if (!(double.TryParse(DATA_2.GetType().GetProperty(ColEname[j]).GetValue(DATA_2).ToString(), out chk_n)))
                                        {
                                            DATA_2.CHECK_FLAG = "N";
                                            AllGrid.Grid_T[i].CHECK_FLAG = "N";
                                            DATA_2.CHECK_MSG = DATA_2.CHECK_MSG + "「" + ColCname[j] + "」需為數字;";
                                        }
                                    }
                                    else
                                    {
                                        if (DATA_2.GetType().GetProperty(ColEname[j]).GetValue(DATA_2).ToString().Length > int.Parse(repo.GetVARCHAR_LENGH(ColEname[j])))
                                        {
                                            DATA_2.CHECK_FLAG = "N";
                                            AllGrid.Grid_T[i].CHECK_FLAG = "N";
                                            DATA_2.CHECK_MSG = DATA_2.CHECK_MSG + "「" + ColCname[j] + "」字數過長;";
                                        }
                                    }

                                    //檢核空值
                                    if (repo.Check_NULL(ColEname[j]))
                                    {
                                        if (DATA_2.GetType().GetProperty(ColEname[j]).GetValue(DATA_2).ToString() == null ||
                                            DATA_2.GetType().GetProperty(ColEname[j]).GetValue(DATA_2).ToString().Trim() == "")
                                        {
                                            DATA_2.CHECK_FLAG = "N";
                                            AllGrid.Grid_T[i].CHECK_FLAG = "N";
                                            DATA_2.CHECK_MSG = DATA_2.CHECK_MSG + "「" + ColCname[j] + "」不可空白;";
                                        }
                                    }

                                    //檢核代碼
                                    if (repo.Check_CODE(ColEname[j]) &&
                                        DATA_2.GetType().GetProperty(ColEname[j]).GetValue(DATA_2).ToString() != "")
                                    {
                                        if (!(repo.Check_CODE_2(ColEname[j], DATA_2.GetType().GetProperty(ColEname[j]).GetValue(DATA_2).ToString())))
                                        {
                                            DATA_2.CHECK_FLAG = "N";
                                            AllGrid.Grid_T[i].CHECK_FLAG = "N";
                                            DATA_2.CHECK_MSG = DATA_2.CHECK_MSG + "「" + ColCname[j] + "」代碼不存在;";
                                        }
                                    }
                                }

                                if (DATA_2.CHECK_FLAG == "N")
                                {
                                    AllGrid.Grid_2.Add(DATA_2);
                                }
                            }
                            #endregion
                        }
                        #endregion
                        lists.Add(AllGrid);
                    }

                    session.Result.etts = lists;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //儲存
        [HttpPost]
        public ApiResponse Commit(FormDataCollection formData)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                IEnumerable<AB0038> AB0038 = JsonConvert.DeserializeObject<IEnumerable<AB0038>>(formData["data"]);
                List<AB0038> list_AB0038 = new List<AB0038>();

                DBWork.BeginTransaction();

                try
                {
                    var repo = new AB0038Repository(DBWork);

                    foreach (AB0038 AB0038_1 in AB0038)
                    {
                        if (AB0038_1.CHECK_FLAG != "N")
                        {
                            repo.SetCommit_HIS_BASORDD(AB0038_1);
                            repo.SetCommit_HIS_BASORDM(AB0038_1);
                            repo.SetCommit_HIS_STKDMIT(AB0038_1);
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
    }
}