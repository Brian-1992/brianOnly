using JCLib.DB.Tool;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;


namespace HIS14SUPDETtransfer
{
    public class Service
    {
        const String sBr = "\r\n";
        public bool isInFlylon()
        {
            IPAddress[] aryIPAddress = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress ipAddress in aryIPAddress)
            {

                String sEachIp = ipAddress.ToString();
                if (IPAddress.Parse(sEachIp).AddressFamily == AddressFamily.InterNetwork)
                {
                    if (
                            (sEachIp.IndexOf("192.20.2") > -1) ||
                        false
                    )
                    {
                        return true;
                    }
                }
            }
            return false;
        } //

        public String getIp()
        {
            IPAddress[] aryIPAddress = Dns.GetHostAddresses(Dns.GetHostName());
            String ip = "";
            foreach (IPAddress ipAddress in aryIPAddress)
            {

                String sEachIp = ipAddress.ToString();
                if (IPAddress.Parse(sEachIp).AddressFamily == AddressFamily.InterNetwork)
                {
                    if (sEachIp.IndexOf("192.20.2") > -1)
                    {
                        ip = sEachIp;
                        break;
                    }
                    ip = sEachIp;
                }
            }
            return ip;
        } //


        protected OracleDB db_MMSMS;
        protected OracleDB db_HISDG;

        LogController log = new LogController();

        public void Run(string hosp_id, string hosp_table_prefix)
        {
            try
            {
                #region 宣告與初始化
                db_MMSMS = new OracleDB("MMSMS");
                db_HISDG = new OracleDB("HISDG");

                DataSet ds_MMSMS = new DataSet();
                DataTable dt_MMSMS = new DataTable();

                DataSet ds_HISDG = new DataSet();
                DataTable dt_HISDG = new DataTable();

                DataSet ds_HISDG_SubQuery1 = new DataSet();
                DataTable dt_HISDG_SubQuery1 = new DataTable();

                DataSet ds_HISDG_SubQuery2 = new DataSet();
                DataTable dt_HISDG_SubQuery2 = new DataTable();

                DataSet ds_MMSMS_SubQuery1 = new DataSet();
                DataTable dt_MMSMS_SubQuery1 = new DataTable();

                DataSet ds_MMSMS_SubQuery2 = new DataSet();
                DataTable dt_MMSMS_SubQuery2 = new DataTable();

                HIS14SUPDETtransferRepository repo = new HIS14SUPDETtransferRepository();

                string jsonString = "";
                PropertyInfo[] properties;
                DataTable dtResult = new DataTable();
                string wh_kind = "";
                string nowSection = "";
                string nowInProgress = "";
                string nowInProgressSub = "";
                string hospName = "";
                string consoleLog = "";

                // 建立醫院資料
                Dictionary<string, string> dict_Hosp_Info = new Dictionary<string, string>
                {
                    { "803", "國軍臺中總醫院" },
                    { "804", "國軍桃園總醫院" },
                    { "805", "國軍花蓮總醫院" },
                    { "807", "三軍總醫院松山分院" },
                    { "811", "三軍總醫院澎湖分院" },
                    { "812", "三軍總醫院基隆分院" },
                    { "813", "國軍桃園總醫院新竹分院" },
                    { "816", "國軍台中總醫院中清分院" },
                    { "818", "三軍總醫院北投分院" }
                };

                // 取得醫院名稱
                if (dict_Hosp_Info.ContainsKey(hosp_id))
                {
                    hospName = dict_Hosp_Info[hosp_id];
                }
                #endregion

                #region 相依性順序整理
                /* DELETE 跟 INSERT 有相依性 需依序處理 */

                /* DELETE */
                // MI_WHINV [要在 MI_MAST MI_WHMAST 前刪除]
                // MI_WINVCTL [要在 MI_MAST MI_WHMAST 前刪除]
                // MI_MAST[要在 MI_MATCLASS 前刪除]
                // PARAM_D[要在 PARAM_M 前刪除]
                // ME_DOCM [要在 MI_MATCLASS 前刪除]
                // ME_DOCD [要在 ME_DOCM MI_MAST 前刪除]
                // ME_DOCI [要在 ME_DOCM MI_MAST 前刪除]
                // MI_WEXPINV [要在 MI_MAST MI_WHMAST 前刪除]
                // MI_WHTRNS [要在 MI_MAST MI_WHMAST 前刪除]

                /* INSERT */
                // MI_WHINV [要在 MI_MAST MI_WHMAST 後新增]
                // MI_WINVCTL [要在 MI_MAST MI_WHMAST 後新增]
                // MI_MAST [要在 MI_MATCLASS 後新增]
                // PARAM_D [要在 PARAM_M 後新增]
                // ME_DOCM [要在 MI_MATCLASS 後新增]
                // ME_DOCD [要在 ME_DOCM MI_MAST 後新增]
                // ME_DOCI [要在 ME_DOCM MI_MAST 後新增]
                // MI_WEXPINV [要在 MI_MAST MI_WHMAST 後新增]
                // MI_WHTRNS [要在 MI_MAST MI_WHMAST 後新增]
                #endregion

                nowSection = "刪除";

                #region DELETE ME_DOCI
                //nowInProgress = "ME_DOCI";
                //consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                //Console.Write("\n" + consoleLog);
                //// DELETE ME_DOCI [要在 ME_DOCM MI_MAST 前刪除]
                //db_MMSMS.BeginTransaction();
                //db_MMSMS.cmd.CommandText = repo.Delete_ME_DOCI();
                //db_MMSMS.cmd.ExecuteNonQuery();
                //db_MMSMS.Commit();
                #endregion


                nowSection = "新增";

                //#region INSERT 0.1 MI_MATCLASS
                //nowInProgress = "MI_MATCLASS";
                //consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                //Console.Write("\n" + consoleLog);
                //// INSERT
                //db_MMSMS.BeginTransaction();
                //db_MMSMS.cmd.CommandText = repo.Insert_MI_MATCLASS();
                //db_MMSMS.cmd.ExecuteNonQuery();
                //db_MMSMS.Commit();
                //#endregion



                //#region INSERT 1 MI_UNITCODE
                //nowInProgress = "MI_UNITCODE";
                //consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                //Console.Write("\n" + consoleLog);
                //// SELECT
                //ds_HISDG = db_HISDG.GetDataSet(repo.Get01讀取HIS骨科手術衛材資料(hosp_table_prefix));
                //dt_HISDG = ds_HISDG.Tables[0];
                //jsonString = JsonConvert.SerializeObject(dt_HISDG, Formatting.Indented);
                ////IEnumerable<MI_UNITCODE> items1 = JsonConvert.DeserializeObject<IEnumerable<MI_UNITCODE>>(jsonString);
                //IEnumerable<MI_UNITCODE> items1 = JsonConvert.DeserializeObject<IEnumerable<MI_UNITCODE>>(jsonString);

                //// INSERT
                //properties = GetPropertyInfo(nowInProgress);
                //foreach (MI_UNITCODE item in items1)
                //{
                //    int itemIndex = items1.ToList().IndexOf(item) + 1;
                //    Console.Write("\r{0} [ {1} / {2} ] {3} ", consoleLog, itemIndex, items1.Count(), ((double)itemIndex / items1.Count()).ToString("P"));
                //    db_MMSMS.cmd.Parameters.Clear();
                //    var dictionary = item.AsDictionary();
                //    for (var i = 0; i < properties.Length; i++)
                //    {
                //        db_MMSMS.cmd.Parameters.Add(properties[i].Name, dictionary[properties[i].Name] ?? "");
                //    }
                //    db_MMSMS.BeginTransaction();
                //    db_MMSMS.cmd.CommandText = repo.Insert_MI_UNITCODE();
                //    db_MMSMS.cmd.BindByName = true;
                //    db_MMSMS.cmd.ExecuteNonQuery();
                //    db_MMSMS.Commit();
                //}
                //#endregion
                // MI_UNITCODE
                #region INSERT 1 HISDG_HIS14_SUPDET
                //nowInProgress = "MI_UNITCODE";
                nowInProgress = "HISDG_HIS14_SUPDET"; // HIS14SUPDETtransfer.HISDG_HIS14_SUPDET
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // SELECT
                ds_HISDG = db_HISDG.GetDataSet(repo.Get01讀取HIS骨科手術衛材資料(hosp_table_prefix));
                dt_HISDG = ds_HISDG.Tables[0];
                jsonString = JsonConvert.SerializeObject(dt_HISDG, Formatting.Indented);
                IEnumerable<HISDG_HIS14_SUPDET> items1 = JsonConvert.DeserializeObject<IEnumerable<HISDG_HIS14_SUPDET>>(jsonString);

                // INSERT
                properties = GetPropertyInfo(nowInProgress);
                int iSumEffect = 0; int iEffect = 0;
                foreach (HISDG_HIS14_SUPDET item in items1)
                {
                    int itemIndex = items1.ToList().IndexOf(item) + 1;
                    Console.Write("\r{0} [ {1} / {2} ] {3} ", consoleLog, itemIndex, items1.Count(), ((double)itemIndex / items1.Count()).ToString("P"));
                    db_MMSMS.cmd.Parameters.Clear();
                    var dictionary = item.AsDictionary();
                    for (var i = 0; i < properties.Length; i++)
                    {
                        if (!properties[i].Name.Equals("ENDL"))
                        {
                            db_MMSMS.cmd.Parameters.Add(properties[i].Name, dictionary[properties[i].Name] ?? "");
                            //Console.WriteLine(properties[i].Name + "=" + dictionary[properties[i].Name] ?? "");
                        }
                    }
                    db_MMSMS.cmd.Parameters.Add("UPDATE_USER", "admin");
                    db_MMSMS.cmd.Parameters.Add("UPDATE_IP", getIp());
                    db_MMSMS.BeginTransaction();
                    db_MMSMS.cmd.CommandText = repo.Insert_MMSMS_HIS14_SUPDET(); // Insert_MI_UNITCODE
                    String debug_sql = db_MMSMS.cmd.CommandText;
                    foreach (Oracle.ManagedDataAccess.Client.OracleParameter name in db_MMSMS.cmd.Parameters)
                    {   
                        var val = name.Value;
                        debug_sql = debug_sql.Replace(":" + name, "'" + val + "'");
                    }
                    //debug_sql = debug_sql.Replace(":whno", "'" + whno + "'");
                    if (isInFlylon() && debug_sql.Length > 0)
                        System.IO.File.AppendAllText(
                                        @"D:\wwwroot\InadArea\" + System.DateTime.Now.ToString("yyyyMMdd") + "_tsghmm_HIS14SUPDETtransferRepository.txt",
                                        sBr + System.DateTime.Now.ToString("yyyyMMddHHmmss") + sBr + "-- Run" + sBr + debug_sql);
                    db_MMSMS.cmd.BindByName = true;
                    iEffect = db_MMSMS.cmd.ExecuteNonQuery();
                    iSumEffect += iEffect;
                    db_MMSMS.Commit();
                }
                Console.WriteLine("共新增 " + iSumEffect + " 筆");


                // SELECT
                ds_MMSMS = db_MMSMS.GetDataSet("select * from HIS14_SUPDET where 1=1 and update_ip is not null order by 1 desc");
                dt_MMSMS = ds_MMSMS.Tables[0];
                JCLib.DB.Tool.L l = new L("HIS14SUPDETtransfer");
                String sHtml = l.getHtmlTable("tableTitle", dt_MMSMS, dt_MMSMS, "SUPDET_SEQ");
                l.lg("Run", sHtml);
                FL fl = new FL("HIS14SUPDETtransfer");
                fl.sendMailToAdmin("三總排程:HIS14SUPDETtransfer", sHtml);

                #endregion


                nowSection = "更新";

                //#region UPDATE UR_ID
                //nowInProgress = "UR_ID";
                //nowInProgressSub = string.Empty;
                //consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                //Console.Write("\n" + consoleLog);
                //// update
                //try
                //{
                //    db_MMSMS.BeginTransaction();
                //    db_MMSMS.cmd.CommandText = repo.Update_UR_ID();
                //    db_MMSMS.cmd.ExecuteNonQuery();
                //    db_MMSMS.Commit();
                //}
                //catch (Exception e)
                //{
                //    CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                //    callDbtools_oralce.I_ERROR_LOG("HIS14SUPDETtransfer", e.Message, "AUTO");

                //    log.Exception_To_Log(string.Format(@"
                //        messsage: UR_ID {0}
                //        StackTrace: {1}
                //        ---------------
                //        object: {2}
                //        ", e.Message, e.StackTrace, db_MMSMS.cmd.CommandText
                //       //JsonConvert.SerializeObject(rx)
                //       ), "HIS14SUPDETtransfer", "HIS14SUPDETtransfer");

                //    db_MMSMS.Rollback();
                //    //db_MMSMS.Dispose();

                //    //db_HISDG.Rollback();
                //    //db_HISDG.Dispose();
                //}
                //Thread.Sleep(1000);
                //#endregion

                //    #region UPDATE MI_MAST
                //    nowInProgress = "MI_MAST";
                //    nowInProgressSub = string.Empty;
                //    consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                //    Console.Write("\n" + consoleLog);
                //    // update
                //    try
                //    {
                //        db_MMSMS.BeginTransaction();
                //        db_MMSMS.cmd.CommandText = repo.Update_MI_MAST();
                //        db_MMSMS.cmd.ExecuteNonQuery();
                //        db_MMSMS.Commit();
                //    }
                //    catch (Exception e)
                //    {
                //        CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                //        callDbtools_oralce.I_ERROR_LOG("HIS14SUPDETtransfer", e.Message, "AUTO");

                //        log.Exception_To_Log(string.Format(@"
                //            messsage: MI_MAST {0}
                //            StackTrace: {1}
                //            ---------------
                //            object: {2}
                //            ", e.Message, e.StackTrace, db_MMSMS.cmd.CommandText
                //           //JsonConvert.SerializeObject(rx)
                //           ), "HIS14SUPDETtransfer", "HIS14SUPDETtransfer");

                //        db_MMSMS.Rollback();
                //        //db_MMSMS.Dispose();

                //        //db_HISDG.Rollback();
                //        //db_HISDG.Dispose();
                //    }
                //    Thread.Sleep(1000);
                //    #endregion

                db_MMSMS.Dispose();
                db_HISDG.Dispose();
            }
            catch (Exception e)
            {
                //CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                //callDbtools_oralce.I_ERROR_LOG("HIS14SUPDETtransfer", e.Message, "AUTO");

                log.Exception_To_Log(string.Format(@"
                        messsage: {0}
                        StackTrace: {1}
                        ---------------
                        object: {2}
                        ", e.Message, e.StackTrace, string.Empty
                   //JsonConvert.SerializeObject(rx)
                   ), "HIS14SUPDETtransfer", "HIS14SUPDETtransfer");

                Console.WriteLine(e.Message);

                //db_MMSMS.Rollback();
                db_MMSMS.Dispose();

                //db_HISDG.Rollback();
                db_HISDG.Dispose();
            }
        }

        private PropertyInfo[] GetPropertyInfo(string modelName)
        {
            return Type.GetType(string.Format("HIS14SUPDETtransfer.{0}", modelName)).GetProperties();
        }

        private static List<T> ConvertDataTable<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }
        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                        pro.SetValue(obj, dr[column.ColumnName].ToString(), null);
                    else
                        continue;
                }
            }
            return obj;
        }
    }
}
