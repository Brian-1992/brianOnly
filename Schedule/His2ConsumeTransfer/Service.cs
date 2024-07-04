using JCLib.DB.Tool;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace His2ConsumeTransfer
{
    public class Service
    {
        protected OracleDB db_MMSMS;
        protected OracleDB db_HIS2;

        LogController log = new LogController();

        public void Run() {
            try
            {
                db_MMSMS = new OracleDB("MMSMS");
                db_HIS2 = new OracleDB("HIS2");

                DataSet ds_MMSMS = new DataSet();
                DataTable dt_MMSMS = new DataTable();

                DataSet ds_HIS2 = new DataSet();
                DataTable dt_HIS2 = new DataTable();

                His2ConsumeTransferRepository repo = new His2ConsumeTransferRepository();

                // 1. 檢查是否有未讀取資料

                ds_HIS2 = db_HIS2.GetDataSet(repo.GetReadflagN());
                dt_HIS2 = ds_HIS2.Tables[0];

                if (dt_HIS2.Rows.Count == 0)
                {
                    // 無資料，不處理
                    return;
                }

                db_HIS2.BeginTransaction();
                // 2. 將未讀取資料狀態更新為P
                db_HIS2.cmd.CommandText = repo.UpdateReadflagP();
                db_HIS2.cmd.Parameters.Clear();
                db_HIS2.cmd.BindByName = true;
                db_HIS2.cmd.ExecuteNonQuery();
                db_HIS2.Commit();

                // 3. 取得狀態P的資料
                ds_HIS2 = db_HIS2.GetDataSet(repo.GetReadflagP());
                dt_HIS2 = ds_HIS2.Tables[0];

                string jsonString = JsonConvert.SerializeObject(dt_HIS2, Newtonsoft.Json.Formatting.Indented);
                IEnumerable<HIS_CONSUME_D_HIS2> items = JsonConvert.DeserializeObject<IEnumerable<HIS_CONSUME_D_HIS2>>(jsonString);

                PropertyInfo[] properties = GetPropertyInfo("HIS_CONSUME_D_HIS2");

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

                    // 4. 檢查是否存在於MMSMS，存在直接更新readflag
                    ds_MMSMS = db_MMSMS.GetDataSet(repo.CheckIdExists(item.ID));
                    dt_MMSMS = ds_MMSMS.Tables[0];

                    if (dt_MMSMS.Rows.Count > 0)
                    {
                        // 已存在，不處理
                        // 更新readflag=Y, readdatetime = sysdate

                        db_HIS2.BeginTransaction();
                        db_HIS2.cmd.CommandText = repo.UpdateReadflag();
                        db_HIS2.cmd.BindByName = true;
                        db_HIS2.cmd.ExecuteNonQuery();

                        db_HIS2.Commit();
                        continue;
                    }

                    // 5. 新增至MMSMS
                    db_MMSMS.BeginTransaction();
                    db_MMSMS.cmd.CommandText = repo.InsertMMSMS();
                    db_MMSMS.cmd.BindByName = true;
                    db_MMSMS.cmd.ExecuteNonQuery();

                    // Commit
                    db_MMSMS.Commit();

                    // 6. 更新HIS2 readflag, readdatetime
                    db_HIS2.BeginTransaction();
                    db_HIS2.cmd.CommandText = repo.UpdateReadflag();
                    db_HIS2.cmd.BindByName = true;
                    db_HIS2.cmd.ExecuteNonQuery();

                    db_HIS2.Commit();
                }

                db_MMSMS.Dispose();
                db_HIS2.Dispose();
            }
            catch (Exception e) {

                CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                callDbtools_oralce.I_ERROR_LOG("His2ConsumeTransfer",e.Message, "AUTO");

                log.Exception_To_Log(string.Format(@"
                        messsage: {0}
                        StackTrace: {1}
                        ---------------
                        object: {2}
                        ", e.Message, e.StackTrace, string.Empty
                   //JsonConvert.SerializeObject(rx)
                   ), "His2ConsumeTransfer", "His2ConsumeTransfer");

                //db_MMSMS.Rollback();
                db_MMSMS.Dispose();

                //db_HIS2.Rollback();
                db_HIS2.Dispose();
            }
        }

        private PropertyInfo[] GetPropertyInfo(string modelName)
        {
            return Type.GetType(string.Format("His2ConsumeTransfer.{0}", modelName)).GetProperties();
        }
    }
}
