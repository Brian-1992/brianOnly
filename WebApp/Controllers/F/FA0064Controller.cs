using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.F;
using NPOI.SS.UserModel;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using WebApp.Models;
using System.Data;
using Newtonsoft.Json;
using System.Text;

namespace WebApp.Controllers.F
{
    public class FA0064Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse GetData(FormDataCollection form)
        {
            string start_date = form.Get("p0");
            string end_date = form.Get("p1");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0064Repository(DBWork);
                    if (repo.CheckIsSameYear(start_date, end_date) == false)
                    {
                        session.Result.success = false;
                        session.Result.msg = "起迄日期年份需相同";
                        return session.Result;
                    }

                    string hospCode = repo.GetHospCode();

                    session.Result.etts = repo.GetData(start_date, end_date, (hospCode == "0"));
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
        public ApiResponse Excel(FormDataCollection form)
        {
            string start_date = form.Get("start_date");
            string end_date = form.Get("end_date");


            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0064Repository repo = new FA0064Repository(DBWork);

                    string hospCode = repo.GetHospCode();

                    JCLib.Excel.Export(form.Get("FN"), repo.Excel(start_date, end_date, (hospCode == "0")));
                }
                catch
                {
                    throw;
                }

                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetDcluicnvs(FormDataCollection form)
        {
            string dclyr = form.Get("dclyr");
            string mmcode = form.Get("mmcode");
            string med_license = form.Get("med_license");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0064Repository(DBWork);
                    session.Result.etts = repo.GetDcluicnvs(dclyr, mmcode, med_license);
                }
                catch (Exception e)
                {

                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMmCodeCombo(FormDataCollection form)
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
                    FA0064Repository repo = new FA0064Repository(DBWork);
                    string hospCode = repo.GetHospCode();
                    session.Result.etts = repo.GetMmCodeCombo(p0, page, limit, "", (hospCode == "0"));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpGet]
        public ApiResponse GetYrDefault()
        {
            using (WorkSession session = new WorkSession())
            {
                int pYear = DateTime.Now.Year - 1911 - 1;

                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0064Repository(DBWork);
                    var hospCode = repo.GetHospCode();

                    COMBO_MODEL temp = new COMBO_MODEL()
                    {
                        TEXT = pYear.ToString(),
                        VALUE = (pYear - 1).ToString(),
                        HOSP_CODE = hospCode //寫入醫院CODE
                    };

                    session.Result.etts = new List<COMBO_MODEL>() { temp };
                }
                catch (Exception e)
                {
                    throw;
                }

                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Copy(FormDataCollection form)
        {
            string from = form.Get("from");
            string to = form.Get("to");
            using (WorkSession session = new WorkSession(this))
            {
                var Dbwork = session.UnitOfWork;
                Dbwork.BeginTransaction();
                try
                {
                    int i = 0;
                    int fromYear = -1;
                    int toYear = -1;
                    if (int.TryParse(from, out i))
                    {
                        fromYear = int.Parse(from);
                    }
                    if (int.TryParse(to, out i))
                    {
                        toYear = int.Parse(to);
                    }
                    if (fromYear >= toYear)
                    {
                        session.Result.success = false;
                        session.Result.msg = "請檢查年份正確性";
                        return session.Result;
                    }

                    var repo = new FA0064Repository(Dbwork);
                    session.Result.afrs = repo.DeleteCdDcluicnv(string.Format("'{0}'", to));
                    session.Result.afrs = repo.Copy(from, to, Dbwork.UserInfo.UserId, Dbwork.ProcIP);
                    session.Result.success = true;

                    Dbwork.Commit();
                    return session.Result;
                }
                catch (Exception e)
                {
                    Dbwork.Rollback();
                    throw;
                }
            }
        }

        [HttpPost]
        public ApiResponse ExcelDcluicnv(FormDataCollection form)
        {
            string dclyr = form.Get("dclyr");
            string mmcode = form.Get("mmcode");
            string med_license = form.Get("med_license");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0064Repository repo = new FA0064Repository(DBWork);

                    JCLib.Excel.Export(form.Get("FN"), repo.GetDcluicnvsExcel(dclyr, mmcode, med_license));
                }
                catch
                {
                    throw;
                }

                return session.Result;
            }

        }

        [HttpPost]
        public ApiResponse Upload()
        {
            using (WorkSession session = new WorkSession(this))
            {
                List<CD_DCLUICNV> list = new List<CD_DCLUICNV>();
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new FA0064Repository(DBWork);

                    #region data upload
                    List<HeaderItem> headerItems = new List<HeaderItem>() {
                       new HeaderItem("申報年度", "DCLYR"),
                        new HeaderItem("院內碼", "MMCODE"),
                        new HeaderItem("許可證字號", "MED_LICENSE"),
                        new HeaderItem("申報計量單位", "DECLARE_UI"),
                        new HeaderItem("換算率", "CNV_RATE"),
                    };

                    IWorkbook workBook;
                    var HttpPostedFile = HttpContext.Current.Request.Files["file"];

                    if (Path.GetExtension(HttpPostedFile.FileName).ToLower() == ".xls")
                    {
                        workBook = new HSSFWorkbook(HttpPostedFile.InputStream);
                    }
                    else
                    {
                        workBook = new XSSFWorkbook(HttpPostedFile.InputStream);
                    }
                    var sheet = workBook.GetSheetAt(0);
                    IRow headerRow = sheet.GetRow(0);//由第一列取標題做為欄位名稱
                    int cellCount = headerRow.LastCellNum;

                    headerItems = SetHeaderIndex(headerItems, headerRow);
                    // 確定該有的欄位都有，沒有的回傳錯誤訊息
                    string errMsg = string.Empty;
                    foreach (HeaderItem item in headerItems)
                    {
                        if (item.Index == -1)
                        {
                            if (errMsg == string.Empty)
                            {
                                errMsg += item.Name;
                            }
                            else
                            {
                                errMsg += string.Format("、{0}", item.Name);
                            }
                        }
                    }

                    if (errMsg != string.Empty)
                    {
                        session.Result.success = false;
                        session.Result.msg = string.Format("欄位錯誤，缺：{0}", errMsg);
                        return session.Result;
                    }

                    for (int i = 1; i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row is null) { continue; }

                        CD_DCLUICNV temp = new CD_DCLUICNV();
                        foreach (HeaderItem item in headerItems)
                        {
                            string value = row.GetCell(item.Index) == null ? string.Empty : row.GetCell(item.Index).ToString();
                            temp.GetType().GetProperty(item.FieldName).SetValue(temp, value, null);
                        }
                        list.Add(temp);
                    }
                    #endregion

                    // 只可修改今年及去年資料
                    string year = (DateTime.Now.Year - 1911).ToString();
                    string pYear = (DateTime.Now.Year - 1911 - 1).ToString();

                    List<CD_DCLUICNV> invalidList = list.Where(x => x.DCLYR != year && x.DCLYR != pYear).Select(x => x).ToList();
                    if (invalidList.Any())
                    {
                        session.Result.success = false;
                        session.Result.msg = "申報年度只能為今年或去年";
                        return session.Result;
                    }

                    IEnumerable<string> dclYreas = list.Select(x => x.DCLYR).Distinct();

                    string yearString = string.Empty;
                    //// 刪除年度資料
                    //foreach (string dclYear in dclYreas) {
                    //    if (yearString != string.Empty) {
                    //        yearString += ",";
                    //    }
                    //    yearString += string.Format("'{0}'", dclYear);
                    //}
                    //session.Result.afrs = repo.DeleteCdDcluicnv(yearString);

                    foreach (CD_DCLUICNV item in list)
                    {
                        item.CREATE_USER = DBWork.UserInfo.UserId;
                        item.UPDATE_IP = DBWork.ProcIP;

                        if (repo.CheckExists(item.DCLYR, item.MMCODE))
                        {
                            session.Result.afrs += repo.UpdateCdDcluicnv(item);
                        }
                        else
                        {
                            session.Result.afrs += repo.InsertCdDcluicnv(item);
                        }
                    }

                    DBWork.Commit();

                    session.Result.success = true;
                    return session.Result;
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    throw;
                }
            }
        }

        public class HeaderItem
        {
            public string Name { get; set; }
            public string FieldName { get; set; }
            public int Index { get; set; }
            public HeaderItem()
            {
                Name = string.Empty;
                Index = -1;
                FieldName = string.Empty;
            }
            public HeaderItem(string name, int index, string fieldName)
            {
                Name = name;
                Index = index;
                FieldName = fieldName;
            }
            public HeaderItem(string name, string fieldName)
            {
                Name = name;
                Index = -1;
                FieldName = fieldName;
            }
        }

        public List<HeaderItem> SetHeaderIndex(List<HeaderItem> list, IRow headerRow)
        {
            int cellCounts = headerRow.LastCellNum;
            for (int i = 0; i < cellCounts; i++)
            {
                foreach (HeaderItem item in list)
                {
                    if (headerRow.GetCell(i).ToString() == item.Name)
                    {
                        item.Index = i;
                    }
                }
            }

            return list;
        }
    }
}