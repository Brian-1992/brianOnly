using JCLib.DB.Tool;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace His2ConsumeTransferByDatetimeRange
{
    public class Service
    {
        protected OracleDB db_MMSMS;
        protected OracleDB db_HIS2;

        LogController log = new LogController();

        public void Run(string startDate, string endDate) {
            try
            {
                db_MMSMS = new OracleDB("MMSMS");
                db_HIS2 = new OracleDB("HIS2");

                DataSet ds_MMSMS = new DataSet();
                DataTable dt_MMSMS = new DataTable();

                DataSet ds_HIS2 = new DataSet();
                DataTable dt_HIS2 = new DataTable();

                His2ConsumeTransferRepository repo = new His2ConsumeTransferRepository();

                // 1. 檢查時間區間內資料
                Console.WriteLine("檢查時間區間內資料");
                ds_HIS2 = db_HIS2.GetDataSet(repo.GetDateRange(startDate, endDate));
                dt_HIS2 = ds_HIS2.Tables[0];

                Console.WriteLine(string.Format("資料數量: {0}", dt_HIS2.Rows.Count));
                if (dt_HIS2.Rows.Count == 0)
                {
                    // 無資料，不處理
                    Console.WriteLine("無資料，不處理");
                    return;
                }

                // 2. datatable轉型為HIS_CONSUME_D_HIS2
                Console.WriteLine("datatable轉型為HIS_CONSUME_D_HIS2");
                string jsonString = JsonConvert.SerializeObject(dt_HIS2, Newtonsoft.Json.Formatting.Indented);
                IEnumerable<HIS_CONSUME_D_HIS2> items = JsonConvert.DeserializeObject<IEnumerable<HIS_CONSUME_D_HIS2>>(jsonString);

                PropertyInfo[] properties = GetPropertyInfo("HIS_CONSUME_D_HIS2");

                Console.WriteLine("檢查是否存在於MMSMS，不存在新增，存在跳過");
                foreach (HIS_CONSUME_D_HIS2 item in items)
                {

                    db_HIS2.cmd.Parameters.Clear();
                    db_MMSMS.cmd.Parameters.Clear();

                    var dictionary = item.AsDictionary();
                    for (var i = 0; i < properties.Length; i++)
                    {
                        db_HIS2.cmd.Parameters.Add(properties[i].Name,
                           dictionary[properties[i].Name] == null ? "" : dictionary[properties[i].Name]);
                        db_MMSMS.cmd.Parameters.Add(properties[i].Name,
                            dictionary[properties[i].Name] == null ? "" : dictionary[properties[i].Name]);
                    }

                    // 2.1. 檢查是否存在於MMSMS，不存在新增，存在跳過
                   
                    ds_MMSMS = db_MMSMS.GetDataSet(repo.CheckIdExists(item.ID));
                    dt_MMSMS = ds_MMSMS.Tables[0];

                    if (dt_MMSMS.Rows.Count > 0)
                    {
                        // 已存在，不處理
                        continue;
                    }

                    // 2.2. 新增至MMSMS
                    Console.Write("不存在資料 id = {0}", item.ID);
                    db_MMSMS.BeginTransaction();
                    db_MMSMS.cmd.CommandText = repo.InsertMMSMS();
                    db_MMSMS.cmd.BindByName = true;
                    db_MMSMS.cmd.ExecuteNonQuery();

                    // Commit
                    db_MMSMS.Commit();
                    Console.WriteLine("     新增成功");
                }

                db_MMSMS.Dispose();
                db_HIS2.Dispose();

                Console.WriteLine("轉檔完成");
                Console.ReadLine();
            }
            catch (Exception e)
            {

                CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                callDbtools_oralce.I_ERROR_LOG("His2Consume補轉", e.Message, "AUTO");

                log.Exception_To_Log(string.Format(@"
                        messsage: {0}
                        StackTrace: {1}
                        ---------------
                        object: {2}
                        ", e.Message, e.StackTrace, string.Empty
                   //JsonConvert.SerializeObject(rx)
                   ), "His2Consume補轉", "His2Consume補轉");

                //db_MMSMS.Rollback();
                db_MMSMS.Dispose();

                //db_HIS2.Rollback();
                db_HIS2.Dispose();
            }
        }

        private PropertyInfo[] GetPropertyInfo(string modelName)
        {
            return Type.GetType(string.Format("His2ConsumeTransferByDatetimeRange.{0}", modelName)).GetProperties();
        }
    }
}
