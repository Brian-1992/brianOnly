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
                //初始化物件
                var DBWork = session.UnitOfWork;
                WebApiADCRepository repo = new WebApiADCRepository(DBWork);
                LogTool logTool = LogTool.getInstance(DBWork);
                //存log資料
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
                        LogId = "",
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

    }
}
