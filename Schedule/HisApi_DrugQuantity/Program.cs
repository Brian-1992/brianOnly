using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using JCLib.DB;
using JCLib.DB.Tool;
using System.Data;
using System.Threading;
using Oracle.ManagedDataAccess.Client;

namespace HisApi_DrugQuantity
{
    class Program : ApiController
    {
        // 呼叫三總提供Web Api, 將藥衛材系統庫存異動回傳HIS
        // 錯誤記錄會寫在TsghmmLog.txt, 資料庫執行錯誤會記錄在ERROR_LOG資料表
        static int isDebug = 0; // 0:正式使用(顯示簡易訊息); 1:測試用(顯示較多訊息)

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("藥衛材系統庫存異動回傳HIS,即時處理中...");
                Console.WriteLine("關閉此視窗將停止即時處理,錯誤訊息會記錄於TsghmmLog_YYYY-MM-DD.txt");
                Console.WriteLine("開始執行時間:" + DateTime.Now.ToString());
                do
                {
                    string msg_oracle = "error";
                    CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                    DataTable dt_oralce = new DataTable();
                    string sql_oracle = " select TR_SNO, TR_MCODE, WH_NO, MMCODE, LOT_NO, TWN_DATE(EXP_DATE) as EXP_DATE, trunc(TR_INV_QTY, 3) as TR_INV_QTY, trunc(AF_TR_INVQTY, 3) as AF_TR_INVQTY from MI_WHTRNS_HIS where PROC_ID = 'N' order by TR_SNO ";
                    if (isDebug == 1) Console.WriteLine("正在擷取待處理資料...");
                    dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, null, "oracle", "T1", ref msg_oracle);
                    if (isDebug == 1) Console.WriteLine("已取得" + dt_oralce.Rows.Count + "筆資料,msg_oracle=" + msg_oracle);
                    if (msg_oracle == "" && dt_oralce.Rows.Count > 0) //資料抓取沒有錯誤 且有資料
                    {
                        if (isDebug == 1) Console.WriteLine("準備傳輸資料至Web API...");
                        int procCnt = 0; // 批次處理幾筆資料
                        int procCntY = 0; // 批次處理幾筆資料(成功)
                        int procCntE = 0; // 批次處理幾筆資料(失敗)
                        int procCntERR = 0; // 批次處理幾筆資料(連線錯誤)
                        for (int i = 0; i < dt_oralce.Rows.Count; i++)
                        {
                            if (isDebug == 1) Console.WriteLine("正在處理第" + (i + 1) + "筆資料...資料序號" + dt_oralce.Rows[i]["TR_SNO"].ToString());
                            string tmpBATCHNO = "";
                            string tmpEXPIREDDATE = "";
                            // 批號與效期非必填,若為null則填入空白
                            if (dt_oralce.Rows[i]["LOT_NO"] != null)
                                tmpBATCHNO = dt_oralce.Rows[i]["LOT_NO"].ToString();
                            if (dt_oralce.Rows[i]["EXP_DATE"] != null)
                                tmpEXPIREDDATE = dt_oralce.Rows[i]["EXP_DATE"].ToString();

                            var postAPIData = new APIData()
                            {
                                TYPE = dt_oralce.Rows[i]["TR_MCODE"].ToString(),
                                STOCKCODE = dt_oralce.Rows[i]["WH_NO"].ToString(),
                                SKORDERCODE = dt_oralce.Rows[i]["MMCODE"].ToString(),
                                BATCHNO = tmpBATCHNO,
                                EXPIREDDATE = tmpEXPIREDDATE,
                                TRANSAMOUNT = dt_oralce.Rows[i]["TR_INV_QTY"].ToString(),
                                AFTERTRANSAMOUNT = dt_oralce.Rows[i]["AF_TR_INVQTY"].ToString()
                            };

                            if (isDebug == 1) Console.WriteLine("開始進行資料傳輸...");
                            var rtnResult = JsonPostAsync(postAPIData).Result; // 將資料post到web api

                            if (rtnResult.Message.ToString() == "true" || rtnResult.Message.ToString() == "True" || rtnResult.Message.ToString() == "TRUE")
                            {
                                if (isDebug == 1) Console.WriteLine("資料傳輸完成,回寫狀態碼Y");
                                // API處理成功
                                try
                                {
                                    // 處理成功則PROC_ID填入Y
                                    int effRows = Update(dt_oralce.Rows[i]["TR_SNO"].ToString(), "Y", "");
                                }
                                catch (Exception ex)
                                {
                                    if (isDebug == 1) Console.WriteLine("回寫狀態碼Y失敗...");
                                    CallDBtools callDBtools = new CallDBtools();
                                    callDBtools.WriteExceptionLog(ex.Message, "HisApi_DrugQuantity", "API處理成功,但資料回寫失敗:" + ex.Message);
                                    throw;
                                }
                                procCntY++;

                            }
                            else if (rtnResult.Message.ToString() == "false" || rtnResult.Message.ToString() == "False" || rtnResult.Message.ToString() == "FALSE")
                            {
                                if (isDebug == 1) Console.WriteLine("資料傳輸完成,回寫狀態碼E");
                                // API回傳false
                                try
                                {
                                    // 處理成功則PROC_ID填入E
                                    Update(dt_oralce.Rows[i]["TR_SNO"].ToString(), "E", "API回傳" + rtnResult.Message + ",處理發生錯誤");
                                }
                                catch (Exception ex)
                                {
                                    if (isDebug == 1) Console.WriteLine("回寫狀態碼E失敗...");
                                    CallDBtools callDBtools = new CallDBtools();
                                    callDBtools.WriteExceptionLog(ex.Message, "HisApi_DrugQuantity", "API處理失敗,資料回寫失敗:" + ex.Message);
                                    throw;
                                }
                                procCntE++;
                            }
                            else
                            {
                                Console.WriteLine("API呼叫時發生問題!");
                                procCntERR++;
                                CallDBtools callDBtools = new CallDBtools();
                                callDBtools.WriteExceptionLog("連線發生問題", "HisApi_DrugQuantity", "問題訊息:" + rtnResult.Message);
                                break; // 如果是連線問題,則中斷資料處理,以免因連線問題沒做的項次,處理順序被往後遞延
                            }
                            procCnt++;
                        }
                        if (procCntERR > 0)
                            Console.WriteLine("[庫存異動]連線錯誤於" + DateTime.Now.ToString());
                        else
                            Console.WriteLine("[庫存異動]批次處理{0}筆資料於" + DateTime.Now.ToString() + "(成功:{1},失敗:{2})", procCnt, procCntY, procCntE);
                    }
                    else if (msg_oracle == "" && dt_oralce.Rows.Count == 0)
                    {
                        if (isDebug == 1) Console.WriteLine("沒有需處理的資料,等候新資料轉入...");
                        procWaiting();
                        if (isDebug == 1) Console.WriteLine("偵測到新資料的轉入,重新開始傳輸流程");
                    }
                    else if (msg_oracle != "")
                    {
                        Console.WriteLine("待處理資料擷取時發生錯誤!");
                        callDbtools_oralce.I_ERROR_LOG("HisApi_DrugQuantity", "資料處理失敗:" + msg_oracle, "AUTO");
                    }

                    Thread.Sleep(1000);
                    if (isDebug == 1) Console.WriteLine("本次迴圈結束,開始新一輪迴圈");
                } while (1 == 1);
            }
            catch (Exception ex)
            {
                CallDBtools callDBtools = new CallDBtools();
                callDBtools.WriteExceptionLog(ex.Message, "HisApi_DrugQuantity", "程式錯誤:" + ex.Message); // 記錄會寫在TsghmmLog
            }

        }

        static public int Update(string tr_sno, string proc_id, string proc_msg)
        {
            CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
            string sql = @" update MI_WHTRNS_HIS 
                            set PROC_TIME = sysdate, PROC_ID = :PROC_ID, PROC_MSG = :PROC_MSG 
                            where TR_SNO = to_number(:TR_SNO) ";
            List<CallDBtools_Oracle.OracleParam> paraList = new List<CallDBtools_Oracle.OracleParam>();
            paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":PROC_ID", "VarChar", proc_id)); // 參數的代入順序需與sql中用到的順序相同
            paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":PROC_MSG", "VarChar", proc_msg));
            paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":TR_SNO", "VarChar", tr_sno.Trim()));

            string msg_oracle = "";
            return callDbtools_oralce.CallExecSQL(sql, paraList, "oracle", ref msg_oracle);
        }

        static public string procWaiting()
        {
            CallDBtools calldbtools = new CallDBtools();
            String s_conn_oracle = calldbtools.SelectDB("oracle");

            OracleConnection conn = new OracleConnection(s_conn_oracle);
            conn.Open();
            OracleCommand cmd = new OracleCommand("MMSTOHIS_WAITING", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.ExecuteNonQuery();

            return "0";
        }

        public class APIResult
        {
            // 執行成功與否
            public bool Success { get; set; }
            // 錯誤訊息
            public string Message { get; set; }

            // 資料時間
            public DateTime DataTime { get; set; }

            public APIResult() { }

        }

        public class APIData
        {
            // 異動類別
            public string TYPE { get; set; }
            // 庫房代碼
            public string STOCKCODE { get; set; }
            // 院內碼
            public string SKORDERCODE { get; set; }
            // 批號
            public string BATCHNO { get; set; }
            // 效期
            public string EXPIREDDATE { get; set; }
            // 交易量
            public string TRANSAMOUNT { get; set; }
            // 異動後存量
            public string AFTERTRANSAMOUNT { get; set; }

            public APIData()
            { }
        }

        private static async Task<APIResult> JsonPostAsync(APIData apiData)
        {
            APIResult fooAPIResult;
            using (HttpClientHandler handler = new HttpClientHandler())
            {
                using (HttpClient client = new HttpClient(handler))
                {
                    try
                    {
                        JCLib.DB.Tool.CallDBtools dbtool = new JCLib.DB.Tool.CallDBtools();
                        string postUrl = "";
                        string serverName = dbtool.GetServerName();
                        if (serverName == "TSGHMM_TEST")
                            postUrl = $"http://f5-hisregweb.ndmctsgh.edu.tw/DrugQuantity/api/DrugQuantity"; // 三總測試環境
                        else if (serverName == "TSGHMM_PRODUCTION")
                        {
                            //postUrl = $"http://f5-hisregweb.ndmctsgh.edu.tw/DrugQuantityOnline/api/DrugQuantity"; // 三總正式環境
                            // 正式環境由兩台SERVER分工處理,其中一台目前無法使用,暫時指定使用可用的另一台
                            postUrl = $"http://10.200.1.168/DrugQuantityOnline/api/DrugQuantity"; // 三總正式環境
                        }
                        else if (serverName == "AIDC_TEST")
                            postUrl = $"http://192.168.3.110:871/api/DrugQuantity"; // 漢翔測試環境

                        HttpResponseMessage response = null;
                        if (isDebug == 1) Console.WriteLine("傳輸目的:" + postUrl);

                        var postFullUrl = $"{postUrl}";

                        // Accept 用於宣告客戶端要求服務端回應的文件型態 (底下兩種方法皆可任選其一來使用)
                        //client.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        // Content-Type 用於宣告遞送給對方的文件型態
                        //client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                        var fooJSON = JsonConvert.SerializeObject(apiData);
                        fooJSON = "'" + fooJSON + "'";
                        using (var fooContent = new StringContent(fooJSON, Encoding.UTF8, "application/json"))
                        {
                            if (isDebug == 1) Console.WriteLine("完成待傳送資料:" + fooJSON);
                            if (isDebug == 1) Console.WriteLine("資料傳送中,正在等候回應...");
                            response = await client.PostAsync(postFullUrl, fooContent);
                        }

                        #region 處理呼叫完成 Web API 之後的回報結果
                        if (response != null)
                        {
                            if (response.IsSuccessStatusCode == true)
                            {
                                // 取得呼叫完成 API 後的回報內容
                                String strResult = await response.Content.ReadAsStringAsync();
                                //fooAPIResult = JsonConvert.DeserializeObject<APIResult>(strResult, new JsonSerializerSettings { MetadataPropertyHandling = MetadataPropertyHandling.Ignore });
                                fooAPIResult = new APIResult
                                {
                                    Success = true,
                                    Message = strResult
                                };
                                if (isDebug == 1) Console.WriteLine("API處理完成,執行結果:true,API回傳" + strResult);
                            }
                            else
                            {
                                fooAPIResult = new APIResult
                                {
                                    Success = false,
                                    Message = string.Format("Error Code:{0}, Error Message:{1}", response.StatusCode, response.RequestMessage)
                                };
                                if (isDebug == 1) Console.WriteLine("API處理失敗,狀態代碼:" + response.StatusCode + ",訊息:" + response.RequestMessage);
                            }
                        }
                        else
                        {
                            if (isDebug == 1) Console.WriteLine("呼叫API發生異常");
                            fooAPIResult = new APIResult
                            {
                                Success = false,
                                Message = "呼叫API發生異常"
                            };
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        fooAPIResult = new APIResult
                        {
                            Success = false,
                            Message = ex.Message
                        };
                        if (isDebug == 1) Console.WriteLine("呼叫API發生異常,錯誤訊息:" + ex.Message);
                    }
                }
            }

            return fooAPIResult;
        }
    }
}
