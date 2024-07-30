using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AB;
using WebApp.Models;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System;
using NPOI.HSSF.UserModel;
using System.IO;
using System.Web;
using NPOI.SS.UserModel;

namespace WebApp.Controllers.AB
{
    public class AB0090Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2") == null ? string.Empty : form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = form.Get("p5") == null ? string.Empty : form.Get("p5");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0090Repository(DBWork);
                    if (p2 == "all_phd")
                    {
                        session.Result.etts = repo.GetMs_phd(p0, p1, p3, p4, true, p5);
                    }
                    else {
                        session.Result.etts = repo.GetMs(p0, p1, p2, p3, p4, true, p5);
                    }
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetExcel(FormDataCollection form)
        {
            string start_date = form.Get("start_date");
            string end_date = form.Get("end_date");
            string wh_no = form.Get("wh_no") == null ? string.Empty : form.Get("wh_no");
            string mmcode1 = form.Get("mmcode1");
            string mmcode2 = form.Get("mmcode2");
            string orderdcflag = form.Get("orderdcflag") == null ? string.Empty : form.Get("orderdcflag");
            var fileName = form.Get("FN");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0090Repository repo = new AB0090Repository(DBWork);
                    var datay = repo.GetYYYMM(start_date, end_date);
                    IEnumerable<AB0090Repository.M> data; //= repo.GetMs(start_date, end_date, wh_no, mmcode1, mmcode2, false);
                    if (wh_no == "all_phd") {
                        data = repo.GetMs_phd(start_date, end_date, mmcode1, mmcode2, false, orderdcflag);
                    } else {
                        data = repo.GetMs(start_date, end_date, wh_no, mmcode1, mmcode2, false, orderdcflag);
                    }

                    var workbook = ExoprtToExcel(datay, data, wh_no);

                    //output
                    var ms = new MemoryStream();
                    workbook.Write(ms);
                    var res = HttpContext.Current.Response;
                    res.BufferOutput = false;

                    res.Clear();
                    res.ClearHeaders();
                    res.HeaderEncoding = System.Text.Encoding.Default;
                    res.ContentType = "application/octet-stream";
                    res.AddHeader("Content-Disposition",
                                "attachment; filename=" + fileName);
                    res.BinaryWrite(ms.ToArray());

                    ms.Close();
                    ms.Dispose();

                    session.Result.success = true;

                }
                catch
                {
                    throw;
                }

                return session.Result;
            }
        }

        public HSSFWorkbook ExoprtToExcel(IEnumerable<AB0090Repository.Y> datay, IEnumerable<AB0090Repository.M> data, string wh_no)
        {
            //var ss = s.Split(',');
            //var apptime_bg = ss[0];
            //var apptime_ed = ss[1];
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0090Repository repo = new AB0090Repository(DBWork);
                    var wb = new HSSFWorkbook();
                    var sheet = (HSSFSheet)wb.CreateSheet("Sheet1");
                    var kind3 = "住";
                    var kind2 = "急";
                    var kind1 = "門";
                    var kind_other = "其他";
                    var kindAll = "全";
                    var yyymm = "";
                    var mmcode = "";
                    int ii = 0;

                    IRow row = sheet.CreateRow(0);
                    row.CreateCell(0).SetCellValue("消/進");
                    row.CreateCell(1).SetCellValue("合約碼");
                    row.CreateCell(2).SetCellValue("主藥理");
                    row.CreateCell(3).SetCellValue("次藥理");
                    row.CreateCell(4).SetCellValue("健保");
                    row.CreateCell(5).SetCellValue("院內代碼");
                    row.CreateCell(6).SetCellValue("藥品名稱");
                    row.CreateCell(7).SetCellValue("主成份");
                    row.CreateCell(8).SetCellValue("計價單位");
                    row.CreateCell(9).SetCellValue("進價");
                    row.CreateCell(10).SetCellValue("合約價");
                    row.CreateCell(11).SetCellValue("優惠百分比");
                    row.CreateCell(12).SetCellValue("移動加權平均價");
                    row.CreateCell(13).SetCellValue("健保價");
                    row.CreateCell(14).SetCellValue("健保碼");
                    row.CreateCell(15).SetCellValue("廠商名稱");
                    row.CreateCell(16).SetCellValue("廠牌註記");
                    row.CreateCell(17).SetCellValue("廠牌");
                    row.CreateCell(18).SetCellValue("申請商註記");
                    row.CreateCell(19).SetCellValue("申請商");
                    ii = 0;
                    for (int i = 0; i < datay.Count(); i++)
                    {
                        row.CreateCell(ii + 20).SetCellValue(datay.ElementAt(i).YYYMM + kind1); //門
                        row.CreateCell(ii + 21).SetCellValue(datay.ElementAt(i).YYYMM + kind2); //急
                        row.CreateCell(ii + 22).SetCellValue(datay.ElementAt(i).YYYMM + kind3); //住
                        row.CreateCell(ii + 23).SetCellValue(datay.ElementAt(i).YYYMM + kind_other); //其他
                        row.CreateCell(ii + 24).SetCellValue(datay.ElementAt(i).YYYMM + kindAll); //全
                        ii = ii + 5;
                    }

                    for (int k = 0; k < data.Count(); k++)
                    {
                        row = sheet.CreateRow(k + 1);
                        row.CreateCell(0).SetCellValue(data.ElementAt(k).TR_MCODE);
                        row.CreateCell(1).SetCellValue(data.ElementAt(k).CONTRACNO);
                        row.CreateCell(2).SetCellValue(data.ElementAt(k).DRUGPTYCODE1);
                        row.CreateCell(3).SetCellValue(data.ElementAt(k).DRUGPTYCODE2);
                        row.CreateCell(4).SetCellValue(data.ElementAt(k).INSUSIGNI);
                        row.CreateCell(5).SetCellValue(data.ElementAt(k).MMCODE);
                        row.CreateCell(6).SetCellValue(data.ElementAt(k).MMNAME_E);
                        row.CreateCell(7).SetCellValue(data.ElementAt(k).E_SCIENTIFICNAME);
                        row.CreateCell(8).SetCellValue(data.ElementAt(k).E_ORDERUNIT);
                        row.CreateCell(9).SetCellValue(data.ElementAt(k).DISC_UPRICE);
                        row.CreateCell(10).SetCellValue(data.ElementAt(k).M_CONTPRICE);
                        row.CreateCell(11).SetCellValue(data.ElementAt(k).MAMAGERATE);
                        row.CreateCell(12).SetCellValue(data.ElementAt(k).AVG_PRICE);
                        row.CreateCell(13).SetCellValue(data.ElementAt(k).NHI_PRICE);
                        row.CreateCell(14).SetCellValue(data.ElementAt(k).M_NHIKEY);
                        row.CreateCell(15).SetCellValue(data.ElementAt(k).AGEN_NAME);
                        row.CreateCell(16).SetCellValue(data.ElementAt(k).M_AGENLAB_RMK);
                        row.CreateCell(17).SetCellValue(data.ElementAt(k).M_AGENLAB);
                        row.CreateCell(18).SetCellValue(data.ElementAt(k).AGENTNAME1);
                        row.CreateCell(19).SetCellValue(data.ElementAt(k).AGENTNAME2);
                        ii = 0;
                        for (int i = 0; i < datay.Count(); i++)
                        {
                            yyymm = datay.ElementAt(i).YYYMM;
                            mmcode = data.ElementAt(k).MMCODE;

                            if (data.ElementAt(k).TR_MCODE == "進貨")
                            {
                                MI_WINVMON invmon = repo.GetInvmon(yyymm, wh_no, mmcode);
                                if (wh_no == "all_phd")
                                {
                                    invmon = repo.GetInvmon_phd(yyymm, mmcode);
                                }
                                else
                                {
                                    invmon = repo.GetInvmon(yyymm, wh_no, mmcode);
                                }
                                if (invmon == null)
                                {
                                    invmon = new MI_WINVMON();
                                }

                                row.CreateCell(ii + 20).SetCellValue("0");
                                row.CreateCell(ii + 21).SetCellValue("0");
                                row.CreateCell(ii + 22).SetCellValue("0");
                                row.CreateCell(ii + 23).SetCellValue(invmon.APL_INQTY);
                            }
                            else//消秏
                            {
                                MI_CONSUME_MN consume_mn;
                                if (wh_no == "all_phd") {
                                    consume_mn = repo.GetConsumeMn_phd(yyymm, mmcode);
                                } else {
                                    consume_mn = repo.GetConsumeMn(yyymm, wh_no, mmcode);
                                }
                                if (consume_mn == null)
                                {
                                    consume_mn = new MI_CONSUME_MN();
                                }

                                
                                row.CreateCell(ii + 20).SetCellValue(consume_mn.ALLQTY1);       // 門
                                row.CreateCell(ii + 21).SetCellValue(consume_mn.ALLQTY2);       // 急
                                row.CreateCell(ii + 22).SetCellValue(consume_mn.ALLQTY3);       // 住
                                row.CreateCell(ii + 23).SetCellValue(consume_mn.OTH_CONSUME);   // 其他
                                row.CreateCell(ii + 24).SetCellValue(consume_mn.ALLQTY);        // 總
                            }
                            ii = ii + 5;
                        }
                    }

                    return wb;
                }
                catch(Exception e)
                {
                    throw;
                }
            }
        }

        [HttpPost]
        public ApiResponse GetWhnoCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0090Repository repo = new AB0090Repository(DBWork);

                    List<COMBO_MODEL> list = repo.GetWhnoCombo().ToList();
                    list.Add(new COMBO_MODEL { VALUE="all_phd", COMBITEM = "全部藥局"});

                    session.Result.etts = list;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetMmCodeCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0"); //動態mmcode
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0090Repository repo = new AB0090Repository(DBWork);
                    session.Result.etts = repo.GetMmCodeCombo(p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse UpdateConsumeMnWh(FormDataCollection form) {
            bool isInit = form.Get("isInit") == "Y";

            using (WorkSession session = new WorkSession(this)) {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0090Repository repo = new AB0090Repository(DBWork);

                    string pym = repo.GetPreym();
                    string rtn = string.Empty;

                    // 第一次進入功能，檢查是否有上月資料，沒有的話呼叫SP轉入資料
                    if (isInit)
                    {
                        if (repo.CheckConsumeMnWh(pym) == false)
                        {
                            rtn = repo.CallProc(pym);
                            if (rtn != "Y")
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = "發生執行錯誤，" + rtn + "。";
                            }
                            return session.Result;
                        }
                        session.Result.msg = "已存在資料";
                        session.Result.success = true;
                        return session.Result;
                    }

                    // 手動更新資料
                    rtn = repo.CallProc(pym);
                    if (rtn != "Y")
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "發生執行錯誤，" + rtn + "。";
                    }
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpGet]
        public ApiResponse GetPym() {
            using (WorkSession session = new WorkSession(this)) {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0090Repository repo = new AB0090Repository(DBWork);
                    session.Result.msg = repo.GetPreym();
                    session.Result.success = true;
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