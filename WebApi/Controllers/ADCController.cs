using JCLib.DB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApi.Models;
using WebApi.Models.ADC;
using WebApi.Models.ADC.GetMedicalCheckAccept;
using WebApi.Models.ADC.GetMedicalReturn;
using WebApi.Models.ADC.UpdateMedicalAllocate;
using WebApi.Models.ADC.UpdateMedicalCheckAccept;
using WebApi.Models.ADC.UpdateMedicalReturn;
using WebApi.Repository;
using WebApi.Utilities;
using WebApi.Utilities.models;

namespace WebApi.Controllers
{
    [RoutePrefix("api/MMSMS")]
    public class ADCController : ApiController
    {
        [HttpPost]
        [AllowAnonymous]
        [Route("GetMedicalOrder")]
        public ApiResult GetMedicalOrder([FromBody] getAdcMedOrdRqBody input)
        {
            using (WorkSession session = new WorkSession())
            {
                
                var DBWork = session.UnitOfWork;
                WebApiADCRepository repo = new WebApiADCRepository(DBWork);
                LogTool logTool = LogTool.getInstance(DBWork);
                
                logModel logContent = new logModel();
                
                try
                {
                    logContent.REQUESTDATETIME = DateTime.Now;
                    logContent.REQUESTDATA = JsonConvert.SerializeObject(input);
                    logContent.ADCNO = input.ADCNO;
                    logContent.FUNCTIONNAME = "GetMedicalOrder";

                    if (!ModelState.IsValid)
                    {
                        logContent.RESPONSEDATETIME = DateTime.UtcNow;
                        logContent.RESPONSESTATUS = 0;
                        ApiResult errorResult = new ApiResult()
                        {
                            Msg = "Required properties are missing.",
                            Success = false
                        };
                        logContent.RESPONSECONTENT = JsonConvert.SerializeObject(errorResult);
                        logTool.WriteLog(logContent);
                        return errorResult;
                    }
                    //執行撈取
                    IEnumerable<getAdcMedOrdResult> orderList = repo.getMedicalOrder(input);
                    ApiResult result = new ApiResult()
                    {
                        TotalCount = orderList.Count(),
                        Data = orderList,
                        Success= true
                    };

                    logContent.RESPONSEDATETIME = DateTime.Now;
                    logContent.RESPONSESTATUS = 1;
                    logContent.RESPONSECONTENT = JsonConvert.SerializeObject(result);
                    //寫log
                    logTool.WriteLog(logContent);
                    //回傳結果
                    return result;
                }
                catch (Exception e)
                {
                    logContent.RESPONSEDATETIME = DateTime.Now;
                    logContent.RESPONSESTATUS = 0;
                    ApiResult errorResult = new ApiResult()
                    {
                        Msg = e.Message,
                        Success = false
                    };
                    logContent.RESPONSECONTENT = JsonConvert.SerializeObject(errorResult);
                    logTool.WriteLog(logContent);
                    return errorResult;
                }
            }

        }

        [HttpPost]
        [AllowAnonymous]
        [Route("UpdateMedicalOrder")]
        public ApiResult UpdateMedicalOrder([FromBody] updateMedOrdRqBody input)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                WebApiADCRepository repo = new WebApiADCRepository(DBWork);
                LogTool logTool = LogTool.getInstance(DBWork);
                logModel logContent = new logModel();

                try
                {
                    logContent.ID = logTool.getHexString();
                    logContent.REQUESTDATETIME = DateTime.Now;
                    logContent.REQUESTDATA = JsonConvert.SerializeObject(input);
                    logContent.ADCNO = input.ADCNO;
                    logContent.FUNCTIONNAME = "UpdateMedicalOrder";

                    if (!ModelState.IsValid)
                    {
                        logContent.RESPONSEDATETIME = DateTime.UtcNow;
                        logContent.RESPONSESTATUS = 0;
                        ApiResult errorResult = new ApiResult()
                        {
                            Msg = "Required properties are missing.",
                            Success = false
                        };
                        logContent.RESPONSECONTENT = JsonConvert.SerializeObject(errorResult);
                        logTool.WriteLog(logContent);
                        return errorResult;
                    }

                    int repoResult = repo.updateMedicalOrder(input.DOCNO,input.SEQ);
                    logContent.RESPONSESTATUS = 1;
                    logContent.RESPONSEDATETIME = DateTime.Now;
                    updateMedOrdResult resultData = new updateMedOrdResult()
                    {
                        LogId = logContent.ID,
                        ResponseTime = logContent.RESPONSEDATETIME.ToString()
                    };
                    ApiResult result = new ApiResult()
                    {
                        Data = resultData,
                        Success = true,
                        Msg = $"寫入{repoResult}筆資料"
                    };
                    logContent.RESPONSECONTENT = JsonConvert.SerializeObject(result);
                    logTool.WriteLog(logContent);
                    return result;

                }
                catch (Exception e)
                {
                    logContent.ID = logTool.getHexString();
                    logContent.RESPONSEDATETIME = DateTime.Now;
                    logContent.RESPONSESTATUS = 0;
                    updateMedOrdResult errorResultData = new updateMedOrdResult()
                    {
                        LogId = logContent.ID,
                        ResponseTime = logContent.RESPONSEDATETIME.ToString()
                    };
                    ApiResult errorResult = new ApiResult()
                    {
                        Data= errorResultData,
                        Msg = e.Message,
                        Success = false
                    };
                    logContent.RESPONSECONTENT = JsonConvert.SerializeObject(errorResult);
                    logTool.WriteLog(logContent);
                    return errorResult;
                }
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("GetMedicalAllocate")]
        public ApiResult GetMedicalAllocate([FromBody] getMedicalAllocateRqBody input)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                WebApiADCRepository repo = new WebApiADCRepository(DBWork);
                LogTool logTool = LogTool.getInstance(DBWork);

                logModel logContent = new logModel();

                try
                {
                    bool vaildFlag = false;
                    string coustomMsg = "";
                    if (ModelState.IsValid)
                    {
                        vaildFlag = true;
                    }

                    logContent.REQUESTDATETIME = DateTime.Now;
                    logContent.REQUESTDATA = JsonConvert.SerializeObject(input);
                    logContent.ADCNO = input.ADCNO;
                    logContent.FUNCTIONNAME = "GetMedicalAllocate";

                    if (input.ExecuteType==1)
                    {
                        if (string.IsNullOrEmpty(input.FRWH) || string.IsNullOrEmpty(input.TOWH))
                        {
                            vaildFlag = false;
                            coustomMsg = "FRWH,TOWH must have value.";
                        }
                    }
                    switch (input.ExecuteType)
                    {
                        case 1:
                            if (string.IsNullOrEmpty(input.FRWH) || string.IsNullOrEmpty(input.TOWH))
                            {
                                vaildFlag = false;
                                coustomMsg = "FRWH,TOWH must have value.";
                            }
                            break;
                        case 2:
                            if (string.IsNullOrEmpty(input.FRWH))
                            {
                                vaildFlag = false;
                                coustomMsg = "FRWH must have value.";
                            }
                            break;
                        case 3:
                            if (string.IsNullOrEmpty(input.TOWH))
                            {
                                vaildFlag = false;
                                coustomMsg = "TOWH must have value.";
                            }
                            break;
                        default:
                            vaildFlag = false;
                            coustomMsg = "請確認傳入調撥狀態ExecuteType";
                            break;
                    }

                    if (!vaildFlag)
                    {
                        logContent.RESPONSEDATETIME = DateTime.UtcNow;
                        logContent.RESPONSESTATUS = 0;
                        ApiResult errorResult = new ApiResult()
                        {
                            Msg = $"Required properties are missing.{coustomMsg}",
                            Success = false
                        };
                        logContent.RESPONSECONTENT = JsonConvert.SerializeObject(errorResult);
                        logTool.WriteLog(logContent);
                        return errorResult;
                    }

                    //執行撈取
                    IEnumerable<getMedicalAllocateResult> resultBody = repo.getMedicalAllocate(input);
                    ApiResult result = new ApiResult()
                    {
                        TotalCount = resultBody.Count(),
                        Data = resultBody,
                        Success = true
                    };

                    logContent.RESPONSEDATETIME = DateTime.Now;
                    logContent.RESPONSESTATUS = 1;
                    logContent.RESPONSECONTENT = JsonConvert.SerializeObject(result);
                    //寫log
                    logTool.WriteLog(logContent);
                    //回傳結果
                    return result;

                }
                catch (Exception e)
                {
                    logContent.RESPONSEDATETIME = DateTime.Now;
                    logContent.RESPONSESTATUS = 0;
                    ApiResult errorResult = new ApiResult()
                    {
                        Msg = e.Message,
                        Success = false
                    };
                    logContent.RESPONSECONTENT = JsonConvert.SerializeObject(errorResult);
                    logTool.WriteLog(logContent);
                    return errorResult;
                }
            }
        }
        [HttpPost]
        [AllowAnonymous]
        [Route("UpdateMedicalAllocate")]
        public ApiResult UpdateMedicalAllocate([FromBody] UpdateMedicalAllocateRqBody input) {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                WebApiADCRepository repo = new WebApiADCRepository(DBWork);
                LogTool logTool = LogTool.getInstance(DBWork);
                logModel logContent = new logModel();

                try
                {
                    bool vaildFlag = false;
                    string coustomMsg = "";
                    if (ModelState.IsValid)
                    {
                        coustomMsg += "Required properties are missing.";
                        vaildFlag = true;
                    }

                    logContent.ID = logTool.getHexString();
                    logContent.REQUESTDATETIME = DateTime.Now;
                    logContent.REQUESTDATA = JsonConvert.SerializeObject(input);
                    logContent.ADCNO = input.ADCNO;
                    logContent.FUNCTIONNAME = "UpdateMedicalAllocate";
                    int qtyNum = 0;
                    switch (input.ExecuteType)
                    {
                        case 1:
                            if (input.APPQTY<0)
                            {
                                vaildFlag = false;
                                coustomMsg = "APPQTY must have value.";
                            }
                            else
                            {
                                qtyNum = input.APPQTY;
                            }
                            break;
                        case 2:
                            if (input.APVQTY<0)
                            {
                                vaildFlag = false;
                                coustomMsg = "APVQTY must have value.";
                            }
                            else
                            {
                                qtyNum = input.APVQTY;
                            }
                            break;
                        case 3:
                            if (input.ACKQTY<0)
                            {
                                vaildFlag = false;
                                coustomMsg = "ACKQTY must have value.";
                            }
                            else
                            {
                                qtyNum = input.ACKQTY;
                            }
                            break;
                        default:
                            vaildFlag = false;
                            coustomMsg = "請確認傳入調撥狀態ExecuteType";
                            break;
                    }

                    //此處增加撈取此品項庫存量，若小於零，流程中止
                    List<dynamic> itemList = repo.getINV_QTY(input.DOCNO, input.SEQ).ToList();
                    if (itemList.Count<0 || itemList[0].INV_QTY<=0)
                    {
                        vaildFlag = false;
                        coustomMsg += "無庫存";
                    }



                    if (!vaildFlag)
                    {
                        logContent.RESPONSEDATETIME = DateTime.UtcNow;
                        logContent.RESPONSESTATUS = 0;
                        ApiResult errorResult = new ApiResult()
                        {
                            Msg = $"{coustomMsg}",
                            Success = false
                        };
                        logContent.RESPONSECONTENT = JsonConvert.SerializeObject(errorResult);
                        logTool.WriteLog(logContent);
                        return errorResult;
                    }

                    int repoResult = repo.updateMedicalQty(input.ExecuteType,input.DOCNO, input.SEQ, qtyNum);
                    logContent.RESPONSESTATUS = 1;
                    logContent.RESPONSEDATETIME = DateTime.Now;
                    UpdateMedicalAllocateResult resultData = new UpdateMedicalAllocateResult()
                    {
                        LogId = logContent.ID,
                        ResponseTime = logContent.RESPONSEDATETIME.ToString()
                    };
                    ApiResult result = new ApiResult()
                    {
                        Data = resultData,
                        Success = true,
                        Msg = $"寫入{repoResult}筆資料"
                    };
                    logContent.RESPONSECONTENT = JsonConvert.SerializeObject(result);
                    logTool.WriteLog(logContent);
                    return result;

                }
                catch (Exception e)
                {
                    logContent.ID = logTool.getHexString();
                    logContent.RESPONSEDATETIME = DateTime.Now;
                    logContent.RESPONSESTATUS = 0;
                    UpdateMedicalAllocateResult errorResultData = new UpdateMedicalAllocateResult()
                    {
                        LogId = logContent.ID,
                        ResponseTime = logContent.RESPONSEDATETIME.ToString()
                    };
                    ApiResult errorResult = new ApiResult()
                    {
                        Data = errorResultData,
                        Msg = e.Message,
                        Success = false
                    };
                    logContent.RESPONSECONTENT = JsonConvert.SerializeObject(errorResult);
                    logTool.WriteLog(logContent);
                    return errorResult;
                }
            }

        }

        [HttpPost]
        [AllowAnonymous]
        [Route("GetMedicalReturn")]
        public ApiResult GetMedicalReturn([FromBody] GetMedicalReturnRqBody input)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                WebApiADCRepository repo = new WebApiADCRepository(DBWork);
                LogTool logTool = LogTool.getInstance(DBWork);

                logModel logContent = new logModel();

                try
                {
                    bool vaildFlag = false;
                    string coustomMsg = "";
                    if (ModelState.IsValid)
                    {
                        vaildFlag = true;
                    }

                    logContent.REQUESTDATETIME = DateTime.Now;
                    logContent.REQUESTDATA = JsonConvert.SerializeObject(input);
                    logContent.ADCNO = input.ADCNO;
                    logContent.FUNCTIONNAME = "GetMedicalReturn";

                    if (!vaildFlag)
                    {
                        logContent.RESPONSEDATETIME = DateTime.UtcNow;
                        logContent.RESPONSESTATUS = 0;
                        ApiResult errorResult = new ApiResult()
                        {
                            Msg = $"Required properties are missing.{coustomMsg}",
                            Success = false
                        };
                        logContent.RESPONSECONTENT = JsonConvert.SerializeObject(errorResult);
                        logTool.WriteLog(logContent);
                        return errorResult;
                    }

                    //執行撈取
                    IEnumerable<GetMedicalReturnResult> resultBody = repo.getMedicalReturn(input);
                    ApiResult result = new ApiResult()
                    {
                        TotalCount = resultBody.Count(),
                        Data = resultBody,
                        Success = true
                    };

                    logContent.RESPONSEDATETIME = DateTime.Now;
                    logContent.RESPONSESTATUS = 1;
                    logContent.RESPONSECONTENT = JsonConvert.SerializeObject(result);
                    //寫log
                    logTool.WriteLog(logContent);
                    //回傳結果
                    return result;

                }
                catch (Exception e)
                {
                    logContent.RESPONSEDATETIME = DateTime.Now;
                    logContent.RESPONSESTATUS = 0;
                    ApiResult errorResult = new ApiResult()
                    {
                        Msg = e.Message,
                        Success = false
                    };
                    logContent.RESPONSECONTENT = JsonConvert.SerializeObject(errorResult);
                    logTool.WriteLog(logContent);
                    return errorResult;
                }
            }
        }
        [HttpPost]
        [AllowAnonymous]
        [Route("UpdateMedicalReturn")]
        public ApiResult UpdateMedicalReturn([FromBody] UpdateMedicalReturnRqbody input )
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                WebApiADCRepository repo = new WebApiADCRepository(DBWork);
                LogTool logTool = LogTool.getInstance(DBWork);
                logModel logContent = new logModel();

                try
                {
                    bool vaildFlag = false;
                    string coustomMsg = "";
                    if (ModelState.IsValid)
                    {
                        coustomMsg += "Required properties are missing.";
                        vaildFlag = true;
                    }

                    logContent.ID = logTool.getHexString();
                    logContent.REQUESTDATETIME = DateTime.Now;
                    logContent.REQUESTDATA = JsonConvert.SerializeObject(input);
                    logContent.ADCNO = input.ADCNO;
                    logContent.FUNCTIONNAME = "UpdateMedicalAllocate";
                    int qtyNum = 0;
                    switch (input.ExecuteType)
                    {
                        case 1:
                            if (input.APPQTY < 0)
                            {
                                vaildFlag = false;
                                coustomMsg = "APPQTY must have value.";
                            }
                            else
                            {
                                qtyNum = input.APPQTY;
                            }
                            break;
                        case 2:
                            if (input.APPQTY < 0)
                            {
                                vaildFlag = false;
                                coustomMsg = "APPQTY must have value.";
                            }
                            else
                            {
                                qtyNum = input.APPQTY;
                            }
                            break;
                        default:
                            vaildFlag = false;
                            coustomMsg = "請確認傳入調撥狀態ExecuteType";
                            break;
                    }

                    //此處增加撈取此品項庫存量，若小於零，流程中止
                    List<dynamic> itemList = repo.getINV_QTY(input.DOCNO, input.SEQ).ToList();
                    if (itemList.Count < 0 || itemList[0].INV_QTY <= 0)
                    {
                        vaildFlag = false;
                        coustomMsg += "無庫存";
                    }



                    if (!vaildFlag)
                    {
                        logContent.RESPONSEDATETIME = DateTime.UtcNow;
                        logContent.RESPONSESTATUS = 0;
                        ApiResult errorResult = new ApiResult()
                        {
                            Msg = $"{coustomMsg}",
                            Success = false
                        };
                        logContent.RESPONSECONTENT = JsonConvert.SerializeObject(errorResult);
                        logTool.WriteLog(logContent);
                        return errorResult;
                    }

                    int repoResult = repo.updateMedicalAPPQTY(input.DOCNO, input.SEQ, qtyNum);
                    logContent.RESPONSESTATUS = 1;
                    logContent.RESPONSEDATETIME = DateTime.Now;
                    UpdateMedicalReturnResult resultData = new UpdateMedicalReturnResult()
                    {
                        LogId = logContent.ID,
                        ResponseTime = logContent.RESPONSEDATETIME.ToString()
                    };
                    ApiResult result = new ApiResult()
                    {
                        Data = resultData,
                        Success = true,
                        Msg = $"寫入{repoResult}筆資料"
                    };
                    logContent.RESPONSECONTENT = JsonConvert.SerializeObject(result);
                    logTool.WriteLog(logContent);
                    return result;

                }
                catch (Exception e)
                {
                    logContent.ID = logTool.getHexString();
                    logContent.RESPONSEDATETIME = DateTime.Now;
                    logContent.RESPONSESTATUS = 0;
                    UpdateMedicalReturnResult errorResultData = new UpdateMedicalReturnResult()
                    {
                        LogId = logContent.ID,
                        ResponseTime = logContent.RESPONSEDATETIME.ToString()
                    };
                    ApiResult errorResult = new ApiResult()
                    {
                        Data = errorResultData,
                        Msg = e.Message,
                        Success = false
                    };
                    logContent.RESPONSECONTENT = JsonConvert.SerializeObject(errorResult);
                    logTool.WriteLog(logContent);
                    return errorResult;
                }
            }

        }

        [HttpPost]
        [AllowAnonymous]
        [Route("GetMedicalCheckAccept")]
        public ApiResult GetMedicalCheckAccept([FromBody] GetMedicalCheckAcceptRqBody input)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                WebApiADCRepository repo = new WebApiADCRepository(DBWork);
                LogTool logTool = LogTool.getInstance(DBWork);

                logModel logContent = new logModel();

                try
                {
                    bool vaildFlag = false;
                    string coustomMsg = "";
                    if (ModelState.IsValid)
                    {
                        vaildFlag = true;
                    }

                    logContent.REQUESTDATETIME = DateTime.Now;
                    logContent.REQUESTDATA = JsonConvert.SerializeObject(input);
                    logContent.ADCNO = input.ADCNO;
                    logContent.FUNCTIONNAME = "GetMedicalCheckAccept";

                    if (!vaildFlag)
                    {
                        logContent.RESPONSEDATETIME = DateTime.UtcNow;
                        logContent.RESPONSESTATUS = 0;
                        ApiResult errorResult = new ApiResult()
                        {
                            Msg = $"Required properties are missing.{coustomMsg}",
                            Success = false
                        };
                        logContent.RESPONSECONTENT = JsonConvert.SerializeObject(errorResult);
                        logTool.WriteLog(logContent);
                        return errorResult;
                    }

                    //執行撈取
                    IEnumerable<GetMedicalCheckAcceptResult> resultBody = repo.getMedicalCheckAccept(input);
                    ApiResult result = new ApiResult()
                    {
                        TotalCount = resultBody.Count(),
                        Data = resultBody,
                        Success = true
                    };

                    logContent.RESPONSEDATETIME = DateTime.Now;
                    logContent.RESPONSESTATUS = 1;
                    logContent.RESPONSECONTENT = JsonConvert.SerializeObject(result);
                    //寫log
                    logTool.WriteLog(logContent);
                    //回傳結果
                    return result;

                }
                catch (Exception e)
                {
                    logContent.RESPONSEDATETIME = DateTime.Now;
                    logContent.RESPONSESTATUS = 0;
                    ApiResult errorResult = new ApiResult()
                    {
                        Msg = e.Message,
                        Success = false
                    };
                    logContent.RESPONSECONTENT = JsonConvert.SerializeObject(errorResult);
                    logTool.WriteLog(logContent);
                    return errorResult;
                }
            }

        }
        [HttpPost]
        [AllowAnonymous]
        [Route("UpdateMedicalCheckAccept")]
        public ApiResult UpdateMedicalCheckAccept([FromBody] UpdateMedicalCheckAcceptRqBody input) {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                WebApiADCRepository repo = new WebApiADCRepository(DBWork);
                LogTool logTool = LogTool.getInstance(DBWork);
                logModel logContent = new logModel();

                try
                {
                    bool vaildFlag = false;
                    string coustomMsg = "";
                    if (ModelState.IsValid)
                    {
                        coustomMsg += "Required properties are missing.";
                        vaildFlag = true;
                    }

                    logContent.ID = logTool.getHexString();
                    logContent.REQUESTDATETIME = DateTime.Now;
                    logContent.REQUESTDATA = JsonConvert.SerializeObject(input);
                    logContent.ADCNO = input.ADCNO;
                    logContent.FUNCTIONNAME = "UpdateMedicalAllocate";
                    int qtyNum = 0;

                    //此處增加撈取此品項庫存量，若小於零，流程中止
                    List<dynamic> itemList = repo.getEXPT_DISTQTY_AND_ACKQTY(input.DOCNO, input.SEQ).ToList();
                    if (itemList.Count < 0)
                    {
                        vaildFlag = false;
                        coustomMsg += "查無資料";
                    }
                    

                    if (!vaildFlag)
                    {
                        logContent.RESPONSEDATETIME = DateTime.UtcNow;
                        logContent.RESPONSESTATUS = 0;
                        ApiResult errorResult = new ApiResult()
                        {
                            Msg = $"{coustomMsg}",
                            Success = false
                        };
                        logContent.RESPONSECONTENT = JsonConvert.SerializeObject(errorResult);
                        logTool.WriteLog(logContent);
                        return errorResult;
                    }
                    string qtyMsg = "";
                    //核撥量< 點收量
                    if (itemList[0].EXPT_DISTQTY < input.ACKQTY)
                    {
                        qtyMsg += "注意:核撥量小於點收量。";
                    }
                    //核撥量> 點收量
                    if (itemList[0].EXPT_DISTQTY > input.ACKQTY)
                    {
                        qtyMsg += "注意:核撥量大於點收量。";
                    }

                    int repoResult = repo.UpdateMedicalCheckAccept(input.DOCNO, input.SEQ, input.ACKQTY,input.ACKID);
                    logContent.RESPONSESTATUS = 1;
                    logContent.RESPONSEDATETIME = DateTime.Now;
                    UpdateMedicalCheckAcceptResult resultData = new UpdateMedicalCheckAcceptResult()
                    {
                        LogId = logContent.ID,
                        ResponseTime = logContent.RESPONSEDATETIME.ToString()
                    };
                    ApiResult result = new ApiResult()
                    {
                        Data = resultData,
                        Success = true,
                        Msg = $"{qtyMsg}寫入{repoResult}筆資料"
                    };
                    logContent.RESPONSECONTENT = JsonConvert.SerializeObject(result);
                    logTool.WriteLog(logContent);
                    return result;

                }
                catch (Exception e)
                {
                    logContent.ID = logTool.getHexString();
                    logContent.RESPONSEDATETIME = DateTime.Now;
                    logContent.RESPONSESTATUS = 0;
                    UpdateMedicalCheckAcceptResult errorResultData = new UpdateMedicalCheckAcceptResult()
                    {
                        LogId = logContent.ID,
                        ResponseTime = logContent.RESPONSEDATETIME.ToString()
                    };
                    ApiResult errorResult = new ApiResult()
                    {
                        Data = errorResultData,
                        Msg = e.Message,
                        Success = false
                    };
                    logContent.RESPONSECONTENT = JsonConvert.SerializeObject(errorResult);
                    logTool.WriteLog(logContent);
                    return errorResult;
                }
            }





        }

    }
}
