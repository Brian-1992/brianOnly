using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Reporting.WebForms;
using System.Threading.Tasks;
using System.Text;
using System.Net.Http.Headers;
using System.Net.Http;
using Newtonsoft.Json;
using JCLib.DB;
using System.Web.Http;
using WebApp.Repository.AB;
using Newtonsoft.Json.Linq;
using WebApp.Controllers;
using WebApp.Models;
using WebApp.Repository.AA;

namespace WebApp.Report.A
{
    public partial class AB0012a : SiteBase.BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                report1Bind();
            }
        }

        protected void report1Bind()
        {
            using (WorkSession session = new WorkSession(this))
            {
                string logMmcode = "";
                string logStep = "";
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    ReportViewer1.EnableTelemetry = false;
                    string DOCNO = Request.QueryString["docno"].ToString().Replace("null", "");

                    AB0012Repository repo = new AB0012Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    DateTime timeNow = DateTime.Now;
                    string parStartDateTime = "";
                    string parEndDateTime = repo.twnDateNow();
                    string parStockCode = repo.getTowhFromDocno(DOCNO); // 取TOWH

                    logStep = "傳遞報表參數";
                    //產生當下列印時間(民國格式)，將時間寫到報表中顯示(固定)
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.ToString("HH:mm:ss");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("UserId", repo.getUserName(User.Identity.Name)) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Towh", parStockCode) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("EndTime", parEndDateTime) });
                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DisplayName = "麻醉及管制藥品登記表";

                    CallHisApiResult rtnResult = new CallHisApiResult();

                    try
                    {
                        foreach (string mmcode in repo.getMmcodeFromDocno(DOCNO))
                        {
                            logMmcode = mmcode;

                            logStep = "取得上一筆申請單";
                            string lastActDocno = repo.getLastAckDocno(mmcode, parStockCode, DOCNO); // 取得MI_WHTRNS中上一筆交易的DOCNO

                            logStep = "取得上次點收時間";
                            //parStartDateTime = repo.getLastAcktimeFromMmcode(mmcode, parStockCode, DOCNO);
                            parStartDateTime = repo.getLastApliTrDate(lastActDocno, mmcode);
                            string parStartDateTimeDayBegin = parStartDateTime.Substring(0, 7) + "000000";

                            //2020-09-23: 新增繳回資料
                            logStep = "取得繳回資料";
                            IEnumerable<APIResultData> returnDatas = repo.GetReturnDatas(parStartDateTimeDayBegin, parStockCode, mmcode);

                            logStep = "取得盤點資料";
                            IEnumerable<APIResultData> chkDatas = repo.GetChkDatas(parStartDateTimeDayBegin, parStockCode, mmcode);

                            logStep = "取得調帳資料";
                            IEnumerable<APIResultData> adjDatas = repo.GetAdjDatas(parStartDateTimeDayBegin, parStockCode, mmcode);

                            //2020-09-30: 取得點收資料
                            logStep = "取得點收資料";
                            List<APIResultData> applyInDatas = new List<APIResultData>();
                            // 前次點收時間為1090731235900，表示上線後從未點收過，第一筆資料為原始結存
                            if (parStartDateTime == "1090731235900")
                            {
                                applyInDatas.Add(new APIResultData() {
                                    USEDATETIME = parStartDateTime,
                                    CHINNAME = "原始結存",
                                    INV_QTY = repo.getLastApliTrInv(lastActDocno, mmcode),
                                    AF_INVQTY = repo.getLastApliAF(lastActDocno, mmcode, parStockCode),
                                    ORDERCODE = mmcode,
                                    ORDERENGNAME = repo.getMMNAME_E(mmcode), 
                                    USEQTY = "",
                                    SPECNUNIT = repo.GetSpecnunit(mmcode),
                                    FLOORQTY = "",
                                    BASE_UNIT = repo.GetBaseunit(mmcode),
                                CREATEDATETIME = parStartDateTime
                                });
                            }
                            else {
                                applyInDatas = repo.GetApplyInDatas(parStartDateTimeDayBegin, parStockCode, mmcode).ToList();
                            }

                            // API傳送參數,會採用json格式post,
                            // 範例 '{StartDateTime:1090709140100,EndDateTime:1090713090000,OrderCode:005MOR02,StockCode:51}'
                            var postAPIData = new CallHisApiData()
                            {
                                //StartDateTime = parStartDateTime,
                                StartDateTime = parStartDateTimeDayBegin,
                                EndDateTime = parEndDateTime,
                                OrderCode = mmcode,
                                StockCode = parStockCode
                                // ,ParTest = "NODATA" //測試用參數
                            };

                            logStep = "傳送至API";
                            string postUrl = "";
                            if (JCLib.Util.GetEnvSetting("DB_CONN_TYPE") == "TEST")
                                postUrl = $"http://f5-hisregweb.ndmctsgh.edu.tw/DrugQuantity/api/DrugApply";
                            else
                            {
                                // postUrl = $"http://f5-hisregweb.ndmctsgh.edu.tw/DrugQuantityOnline/api/DrugApply";
                                // 正式環境由兩台SERVER分工處理,其中一台目前無法使用,暫時指定使用可用的另一台
                                postUrl = $"http://10.200.1.168/DrugQuantityOnline/api/DrugApply"; // 三總正式環境
                            }
                            // string postUrl = $"http://192.168.3.110:871/api/DrugApply"; // 測試環境

                            CallHisApiResult getResult = CallHisApiController.CallWebApi.JsonPostAsync(postAPIData, postUrl).Result; // 將資料post到web api
                            
                            // 每個品項加入一筆標頭
                            logStep = "準備標頭資訊";
                            if (getResult.APIResultData != null)
                            {
                                foreach (APIResultData item in getResult.APIResultData)
                                {
                                    if (item.TYPE == "3")
                                    {
                                        item.CHINNAME = "破損補發";
                                    }
                                }

                                // API資料
                                var tmpAPIResultData = getResult.APIResultData;
                                // 加上繳回資料
                                tmpAPIResultData = tmpAPIResultData.Concat(returnDatas);
                                // 加上點收資料
                                tmpAPIResultData = tmpAPIResultData.Concat(applyInDatas);
                                // 加上盤點資料
                                tmpAPIResultData = tmpAPIResultData.Concat(chkDatas);
                                // 加上調帳資料
                                tmpAPIResultData = tmpAPIResultData.Concat(adjDatas);
                                if (parStartDateTime != "1090731235900")
                                {
                                    string parChinname_P1 = "每日結存";

                                    string parORDERENGNAME_P1 = "";
                                    string parSPECNUNIT_P1 = "";
                                    string parFLOORQTY_P1 = "";
                                    string parBASEUNIT_P1 = "";
                                    if (getResult.APIResultData.Count() > 0)
                                    {
                                        parORDERENGNAME_P1 = getResult.APIResultData.First().ORDERENGNAME;
                                        parSPECNUNIT_P1 = repo.GetSpecnunit(mmcode);
                                        parBASEUNIT_P1 = repo.GetBaseunit(mmcode);
                                        parFLOORQTY_P1 = getResult.APIResultData.First().FLOORQTY;
                                    }
                                    else
                                    {
                                        // API查詢無資料
                                        // repo.AB0012aErrLog("MMCODE=" + logMmcode, "錯誤階段=" + logStep, "API查詢結果無資料", User.Identity.Name); // 記錄本次傳送的品項
                                        parORDERENGNAME_P1 = repo.getMMNAME_E(mmcode);
                                        parSPECNUNIT_P1 = repo.GetSpecnunit(mmcode);
                                        parFLOORQTY_P1 = repo.GetFloorQty(parStockCode, mmcode);
                                        parBASEUNIT_P1 = repo.GetBaseunit(mmcode);
                                    }

                                    // 每日結存的標頭
                                    APIResultData[] resultHeader_P1 = new APIResultData[1];
                                    resultHeader_P1[0] = new APIResultData()
                                    {
                                        USEDATETIME = parStartDateTimeDayBegin, // 上次撥發入庫當日00:00
                                        CHINNAME = parChinname_P1,
                                        INV_QTY = "", // 沒有領入數量
                                        AF_INVQTY = repo.getDayInvqty(parStartDateTime.Substring(0, 7), parStockCode, mmcode),
                                        ORDERCODE = mmcode,
                                        ORDERENGNAME = parORDERENGNAME_P1,
                                        USEQTY = "",
                                        SPECNUNIT = parSPECNUNIT_P1,
                                        BASE_UNIT = parBASEUNIT_P1,
                                        FLOORQTY = parFLOORQTY_P1,
                                        CREATEDATETIME = parStartDateTimeDayBegin
                                    };
                                    logStep = "加入標頭資訊P1";
                                    getResult.APIResultData = tmpAPIResultData.Concat<APIResultData>(resultHeader_P1);
                                    //getResult.APIResultData = resultHeader_P1.Concat<APIResultData>(tmpAPIResultData.Where(APIResultData => APIResultData.CREATEDATETIME.CompareTo(parStartDateTime) < 0).ToList());

                                    // 用CREATEDATETIME排序
                                    getResult.APIResultData = getResult.APIResultData.OrderBy(x => x.CREATEDATETIME).ToList();
                                }
                                else
                                {
                                    // API無資料須加上繳庫回資料
                                    getResult.APIResultData = getResult.APIResultData.Concat<APIResultData>(returnDatas).ToList();
                                    // API無資料須加上盤點資料
                                    getResult.APIResultData = getResult.APIResultData.Concat<APIResultData>(chkDatas).ToList();
                                    // API無資料須加上調帳資料
                                    getResult.APIResultData = getResult.APIResultData.Concat<APIResultData>(adjDatas).ToList();
                                    getResult.APIResultData = getResult.APIResultData.Concat<APIResultData>(applyInDatas).ToList();
                                    // 用CREATEDATETIME排序
                                    getResult.APIResultData = getResult.APIResultData.OrderBy(x => x.CREATEDATETIME).ToList();
                                }
                                
                            }
                            else // API錯誤
                            {
                                getResult.APIResultData = new List<APIResultData>();
                                if (parStartDateTime != "1090731235900") {
                                    string parChinname_P1 = "每日結存";
                                    string parORDERENGNAME_P1 = "";
                                    string parSPECNUNIT_P1 = "";
                                    string parFLOORQTY_P1 = "";
                                    string parBASEUNIT_P1 = "";

                                    parORDERENGNAME_P1 = repo.getMMNAME_E(mmcode);
                                    parSPECNUNIT_P1 = repo.GetSpecnunit(mmcode);
                                    parFLOORQTY_P1 = repo.GetFloorQty(parStockCode, mmcode);
                                    parBASEUNIT_P1 = repo.GetBaseunit(mmcode);

                                    // 每日結存的標頭
                                    APIResultData[] resultHeader_P1 = new APIResultData[1];
                                    resultHeader_P1[0] = new APIResultData()
                                    {
                                        USEDATETIME = parStartDateTimeDayBegin, // 上次撥發入庫當日00:00
                                        CHINNAME = parChinname_P1,
                                        INV_QTY = "", // 沒有領入數量
                                        AF_INVQTY = repo.getDayInvqty(parStartDateTime.Substring(0, 7), parStockCode, mmcode),
                                        ORDERCODE = mmcode,
                                        ORDERENGNAME = parORDERENGNAME_P1,
                                        USEQTY = "",
                                        SPECNUNIT = parSPECNUNIT_P1,
                                        BASE_UNIT = parBASEUNIT_P1,
                                        FLOORQTY = parFLOORQTY_P1,
                                        CREATEDATETIME = parStartDateTimeDayBegin
                                    };
                                    logStep = "加入標頭資訊P1";
                                    getResult.APIResultData = getResult.APIResultData.Concat<APIResultData>(resultHeader_P1);
                                }
                                    
                                // 加上繳庫回資料
                                getResult.APIResultData = getResult.APIResultData.Concat<APIResultData>(returnDatas).ToList();
                                // 加上盤點資料
                                getResult.APIResultData = getResult.APIResultData.Concat<APIResultData>(chkDatas).ToList();
                                // 加上調帳資料
                                getResult.APIResultData = getResult.APIResultData.Concat<APIResultData>(adjDatas).ToList();
                                getResult.APIResultData = getResult.APIResultData.Concat<APIResultData>(applyInDatas).ToList();
                                // 用CREATEDATETIME排序
                                getResult.APIResultData = getResult.APIResultData.OrderBy(x => x.CREATEDATETIME).ToList();
                            }

                            logStep = "計算結存數";
                            // 計算每項的結存數(本項的結存數=前一項的結存數-本項的消耗數)
                            for (int i = 0; i < getResult.APIResultData.Count(); i++)
                            {
                                if (i > 0 && getResult.APIResultData.ElementAt(i).CHINNAME != "撥發入庫") // 第0列是入庫領入或原始結存數量,代表結存數的初始值,其餘各筆逐一扣除消耗數以獲得結存量
                                {
                                    string pAF_INVQTY = getResult.APIResultData.ElementAt(i - 1).AF_INVQTY;
                                    if (pAF_INVQTY == "")
                                        pAF_INVQTY = "0";
                                    string pUSEQTY = getResult.APIResultData.ElementAt(i).USEQTY;
                                    if (pUSEQTY == "")
                                        pUSEQTY = "0";
                                    getResult.APIResultData.ElementAt(i).AF_INVQTY = (Convert.ToDouble(pAF_INVQTY) - Convert.ToDouble(pUSEQTY)).ToString();
                                }
                                else if (i > 0 && getResult.APIResultData.ElementAt(i).CHINNAME == "撥發入庫") // 若為撥發入庫,則將結存量加上領入數量
                                {
                                    // 每日結存到撥發入庫間的消耗記錄不一定已與HIS同步,結存量仍以每日結存為準計算較好
                                    string pAF_INVQTY = getResult.APIResultData.ElementAt(i - 1).AF_INVQTY;
                                    if (pAF_INVQTY == "")
                                        pAF_INVQTY = "0";
                                    string pINV_QTY = getResult.APIResultData.ElementAt(i).INV_QTY;
                                    if (pINV_QTY == "")
                                        pINV_QTY = "0";
                                    getResult.APIResultData.ElementAt(i).AF_INVQTY = (Convert.ToDouble(pAF_INVQTY) + Convert.ToDouble(pINV_QTY)).ToString();
                                }
                            }

                            logStep = "串接品項回傳資訊";
                            // 將每筆品項的資料接到主體
                            if (rtnResult.APIResultData == null)
                                rtnResult = getResult; // 第一筆
                            else
                                rtnResult.APIResultData = rtnResult.APIResultData.Concat<APIResultData>(getResult.APIResultData); // 把每次API取到的資料串接上去

                        }


                    }
                    catch
                    {
                        throw;
                    }

                    if (rtnResult.APIResultData == null)
                    {
                        APIResultData[] resultNull = new APIResultData[1];
                        resultNull[0] = new APIResultData() { };
                        rtnResult.APIResultData = resultNull;
                    }

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", rtnResult.APIResultData));

                    ReportViewer1.LocalReport.Refresh();
                }
                catch (Exception ex)
                {
                    AB0012Repository repo = new AB0012Repository(DBWork);
                    repo.AB0012aErrLog("MMCODE=" + logMmcode, "錯誤階段=" + logStep, ex.Message, User.Identity.Name); // 記錄本次傳送的品項
                    throw;
                }
                //return session.Result;
            }


        }

    }
}