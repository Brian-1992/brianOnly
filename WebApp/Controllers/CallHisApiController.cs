using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using System.Text;
using System.Net.Http.Headers;
using System.Net.Http;
using Newtonsoft.Json;
using JCLib.DB;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using WebApp.Models;
using WebApp.Repository;

namespace WebApp.Controllers
{
    public class CallHisApiController : SiteBase.BaseApiController
    {

        public class CallWebApi : ApiController
        {
            public static async Task<CallHisApiResult> JsonPostAsync(CallHisApiData apiData, string postUrl)
            {
                CallHisApiResult fooAPIResult;
                using (HttpClient client = new HttpClient())
                {
                    string log_SEND = "";
                    string log_RECEIVE = "";
                    try
                    {
                        var postFullUrl = $"{postUrl}";

                        // Accept 用於宣告客戶端要求服務端回應的文件型態 (底下兩種方法皆可任選其一來使用)
                        //client.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        // Content-Type 用於宣告遞送給對方的文件型態
                        //client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                        var fooJSON = JsonConvert.SerializeObject(apiData);
                        fooJSON = "'" + fooJSON + "'";
                        log_SEND = fooJSON;

                        if (postFullUrl.StartsWith($"http://f5-hisregweb.ndmctsgh.edu.tw/DrugQuantity") || postFullUrl.StartsWith($"http://10.200.1.168/DrugQuantityOnline"))
                        {
                            using (var fooContent = new StringContent(fooJSON, Encoding.UTF8, "application/json"))
                            {
                                client.Timeout = TimeSpan.FromSeconds(30);
                                var responseGet = Task.Run(() => client.PostAsync(postFullUrl, fooContent));
                                responseGet.Wait();
                                if (responseGet.Result.IsSuccessStatusCode)
                                {
                                    // APIResultData strResult = await responseGet.Result.Content.ReadAsAsync<APIResultData>();

                                    bool isNoData = false;
                                    // JValue joNoData = null;
                                    JArray jo = null;
                                    try
                                    {
                                        jo = await responseGet.Result.Content.ReadAsAsync<JArray>();
                                        log_RECEIVE = jo.ToString();
                                        isNoData = false;

                                    }
                                    catch
                                    {
                                        isNoData = true;
                                        log_RECEIVE = "資料查詢失敗";
                                        //if (isNoData == true)
                                        //{
                                        //    try
                                        //    {
                                        //        joNoData = await responseGet.Result.Content.ReadAsAsync<JValue>();
                                        //        isNoData = true;
                                        //        log_RECEIVE = joNoData.ToString();
                                        //    }
                                        //    catch { }
                                        //}
                                    }

                                    //if (log_RECEIVE == "API查詢結果無資料" || log_RECEIVE.Contains("無資料") || log_RECEIVE.Contains("資料查詢失敗"))
                                    //{
                                    //    using (WorkSession session = new WorkSession())
                                    //    {
                                    //        UnitOfWork DBWork = session.UnitOfWork;
                                    //        try
                                    //        {
                                    //            CallHisApiRepository repo = new CallHisApiRepository(DBWork);
                                    //            repo.SetErrLog(postUrl, log_SEND, log_RECEIVE, "", "callAPI"); // 記錄傳送和收到的資料
                                    //        }
                                    //        catch { }
                                    //    }
                                    //}

                                    fooAPIResult = new CallHisApiResult
                                    {
                                        Success = true,
                                        Message = "OK"
                                    };
                                    if (log_RECEIVE != "資料查詢失敗")
                                    {
                                        fooAPIResult.APIResultData = JsonConvert.DeserializeObject<IEnumerable<APIResultData>>(log_RECEIVE, new JsonSerializerSettings { MetadataPropertyHandling = MetadataPropertyHandling.Ignore });
                                        if (fooAPIResult.APIResultData.Count() == 0)
                                        {
                                            using (WorkSession session = new WorkSession())
                                            {
                                                UnitOfWork DBWork = session.UnitOfWork;
                                                try
                                                {
                                                    // API查詢無資料
                                                    CallHisApiRepository repo = new CallHisApiRepository(DBWork);
                                                    repo.SetErrLog(postUrl, log_SEND, log_RECEIVE, "API查詢結果無資料", "callAPI"); // 記錄本次傳送的品項
                                                }
                                                catch (Exception ex)
                                                {
                                                    throw;
                                                }
                                            }

                                        }
                                    }

                                }
                                else
                                {
                                    fooAPIResult = new CallHisApiResult
                                    {
                                        Success = false,
                                        Message = "呼叫API發生異常"
                                    };
                                    using (WorkSession session = new WorkSession())
                                    {
                                        log_RECEIVE = responseGet.Result.StatusCode.ToString();
                                        UnitOfWork DBWork = session.UnitOfWork;
                                        try
                                        {
                                            CallHisApiRepository repo = new CallHisApiRepository(DBWork);
                                            repo.SetErrLog(postUrl, log_SEND, log_RECEIVE, "呼叫API發生異常", "callAPI"); // 記錄傳送和收到的資料
                                        }
                                        catch { }
                                    }
                                }
                                responseGet.Dispose();


                            }
                        }
                        else
                        {
                            fooAPIResult = new CallHisApiResult
                            {
                                Success = false,
                                Message = "API位址錯誤"
                            };
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        fooAPIResult = new CallHisApiResult
                        {
                            Success = false,
                            Message = ex.Message
                        };
                        using (WorkSession session = new WorkSession())
                        {
                            UnitOfWork DBWork = session.UnitOfWork;
                            try
                            {
                                CallHisApiRepository repo = new CallHisApiRepository(DBWork);
                                repo.SetErrLog(postUrl, log_SEND, log_RECEIVE, ex.Message, "callAPI"); // 記錄傳送和收到的資料
                            }
                            catch { }
                        }
                    }
                }

                return fooAPIResult;
            }
        }
    }
}