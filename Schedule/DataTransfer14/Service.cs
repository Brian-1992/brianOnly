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

namespace DataTransfer14
{
    public class Service
    {
        protected OracleDB db_MMSMS;
        protected OracleDB db_HISDG;

        LogController log = new LogController();

        public void Run(string hosp_id, string hosp_table_prefix)
        {
            try
            {
                //已手動搬移的UR Table, 0.2MI_MCODE,  0.3PARAM_M,  0.4PARAM_D,  0.5MI_DOCTYPE, 0.8ME_FLOW不動
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

                DataTransfer14Repository repo = new DataTransfer14Repository();

                string jsonString = "";
                PropertyInfo[] properties;
                DataTable dtResult = new DataTable();
                string wh_kind = "";
                string strXfKindID = "";
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
                    { "816", "國軍臺中總醫院中清分院" },
                    { "818", "三軍總醫院北投分院" }
                };

                // 取得醫院名稱
                if (dict_Hosp_Info.ContainsKey(hosp_id))
                {
                    hospName = dict_Hosp_Info[hosp_id];
                    Console.WriteLine("正在進行 " + hospName + " 資料初始化作業程序，請稍後...");
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

                nowSection = "複製MI_WHID至";
                #region COPY  MI_WHID_INIT(MI_WHID暫存表) --用於20 MI_WHID回復資料
                nowInProgress = "MI_WHID_INIT";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);

                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Delete_MI_WHID_INIT(); //清空暫存檔
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                Thread.Sleep(500);

                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Copy_To_MI_WHID_INIT(); //複製資料至暫存檔
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                Console.WriteLine(nowInProgress + " 資料已建立 V");
                #endregion

                nowSection = "刪除";

                #region DELETE ME_DOCI
                nowInProgress = "ME_DOCI";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // DELETE ME_DOCI [要在 ME_DOCM MI_MAST 前刪除]
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Delete_ME_DOCI();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion

                #region DELETE ME_DOCD
                nowInProgress = "ME_DOCD";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // DELETE ME_DOCD [要在 ME_DOCM MI_MAST 前刪除]
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Delete_ME_DOCD();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion

                #region DELETE ME_DOCM
                nowInProgress = "ME_DOCM";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // DELETE ME_DOCM [要在 MI_MATCLASS 前刪除]
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Delete_ME_DOCM();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion

                #region DELETE 11 MI_WEXPINV
                nowInProgress = "MI_WEXPINV";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // DELETE MI_WEXPINV [要在 MI_MAST MI_WHMAST 前刪除]
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Delete_MI_WEXPINV();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion

                #region DELETE MI_WHTRNS
                nowInProgress = "MI_WHTRNS";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // DELETE MI_WHTRNS [要在 MI_MAST MI_WHMAST 前刪除]
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Delete_MI_WHTRNS();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion

                #region DELETE 7 MI_WHINV
                nowInProgress = "MI_WHINV";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // DELETE 7 MI_WHINV [要在 MI_MAST MI_WHMAST 前刪除]
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Delete_MI_WHINV();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion

                #region DELETE 8 MI_WINVCTL
                nowInProgress = "MI_WINVCTL";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // DELETE 8 MI_WINVCTL [要在 MI_MAST MI_WHMAST 前刪除]
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Delete_MI_WINVCTL();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion

                #region DELETE 9 MI_WHID (全刪除，再透過MI_WHID_INIT 回復資料)
                nowInProgress = "MI_WHID";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // DELETE  MI_WHID [要在 MI_WHMAST 前刪除]
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Delete_MI_WHID();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion

                #region DELETE 10 MI_WLOCINV
                nowInProgress = "MI_WLOCINV";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Delete_MI_WLOCINV();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion

                #region DELETE 12 MI_WINVMON
                nowInProgress = "MI_WINVMON";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Delete_MI_WINVMON();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion

                #region DELETE 6 MI_MAST
                nowInProgress = "MI_MAST";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // DELETE 6 MI_MAST [要在 MI_MATCLASS 前刪除]
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Delete_MI_MAST();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion

                #region DELETE 0.1 MI_MATCLASS
                nowInProgress = "MI_MATCLASS";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // DELETE 0.1 MI_MATCLASS
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Delete_MI_MATCLASS();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion

                #region  1130314註解DELETE 0.2MI_MCODE,  0.3PARAM_M,  0.4PARAM_D,  0.5MI_DOCTYPE部分
                /*
                #region DELETE 0.2 MI_MCODE
                nowInProgress = "MI_MCODE";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // DELETE 0.2 MI_MCODE
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Delete_MI_MCODE();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion

                #region DELETE 0.4 PARAM_D
                nowInProgress = "PARAM_D";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // DELETE 0.4 PARAM_D [要在 PARAM_M 前刪除]
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Delete_PARAM_D();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion

                #region DELETE 0.3 PARAM_M
                nowInProgress = "PARAM_M";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // DELETE 0.3 PARAM_M
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Delete_PARAM_M();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion

                #region DELETE 0.5 MI_DOCTYPE
                nowInProgress = "MI_DOCTYPE";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // DELETE 0.5 MI_DOCTYPE
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Delete_MI_DOCTYPE();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion */
                #endregion

                #region DELETE 0.6 BASEUNITCNV
                nowInProgress = "BASEUNITCNV";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // DELETE 0.6 BASEUNITCNV
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Delete_BASEUNITCNV();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion

                #region DELETE 0.7 MI_MNSET
                nowInProgress = "MI_MNSET";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // DELETE 0.7 MI_MNSET
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Delete_MI_MNSET();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion

                #region 1130314註解DELETE 0.8 ME_FLOW部分
                /*
                #region DELETE 0.8 ME_FLOW
                nowInProgress = "ME_FLOW";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // DELETE 0.8 ME_FLOW
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Delete_ME_FLOW();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion */
                #endregion

                #region DELETE 0.9 CHK_MNSET
                nowInProgress = "CHK_MNSET";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // DELETE 0.8 ME_FLOW
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Delete_CHK_MNSET();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion

                #region 1130320註解DELETE 0.10 UR_ID, 0.11 UR_ROLE, 0.12 UR_UIR部分
                /*
                #region DELETE 0.10 UR_ID
                nowInProgress = "UR_ID";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // DELETE 0.8 ME_FLOW
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Delete_UR_ID();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion

                #region DELETE 0.11 UR_ROLE
                nowInProgress = "UR_ROLE";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // DELETE 0.8 ME_FLOW
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Delete_UR_ROLE();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion

                #region DELETE 0.12 UR_UIR
                nowInProgress = "UR_UIR";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // DELETE 0.12 UR_UIR
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Delete_UR_UIR();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion */
                #endregion

                #region DELETE 1 MI_UNITCODE
                nowInProgress = "MI_UNITCODE";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // DELETE 1 MI_UNITCODE
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Delete_MI_UNITCODE();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion

                #region DELETE 2 PH_VENDER
                nowInProgress = "PH_VENDER";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // DELETE 2 PH_VENDER
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Delete_PH_VENDER();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion

                #region DELETE 3 MI_UNITEXCH
                nowInProgress = "MI_UNITEXCH";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // DELETE 3 MI_UNITEXCH
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Delete_MI_UNITEXCH();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion

                #region 1130320註解DELETE 4 UR_INID部分
                /* #region DELETE 4 UR_INID
                nowInProgress = "UR_INID";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // DELETE 4 UR_INID
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Delete_UR_INID();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion */
                #endregion

                #region DELETE 5 MI_WHMAST
                nowInProgress = "MI_WHMAST";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // DELETE 5 MI_WHMAST
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Delete_MI_WHMAST();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion

                #region DELETE 9 BC_BARCODE
                nowInProgress = "BC_BARCODE";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // DELETE 9 BC_BARCODE
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Delete_BC_BARCODE();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion

                #region DELETE 13 MI_WHCOST
                nowInProgress = "MI_WHCOST";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Delete_MI_WHCOST();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion

                #region DELETE 14 MI_MAST_HISTORY
                nowInProgress = "MI_MAST_HISTORY";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Delete_MI_MAST_HISTORY();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion

                #region DELETE 15 SEC_MAST
                nowInProgress = "SEC_MAST";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Delete_SEC_MAST();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion

                #region DELETE 16 SEC_USEMM
                nowInProgress = "SEC_USEMM";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // Check
                ds_HISDG = db_HISDG.GetDataSet(repo.CheckIfTableExists("SEC_USEMM"));
                dt_HISDG = ds_HISDG.Tables[0];
                // 確認有這張 table 才跑刪除
                if (dt_HISDG.Rows.Count == 1)
                {
                    db_MMSMS.BeginTransaction();
                    db_MMSMS.cmd.CommandText = repo.Delete_SEC_USEMM();
                    db_MMSMS.cmd.ExecuteNonQuery();
                    db_MMSMS.Commit();
                }
                #endregion

                #region DELETE MI_BASERO_14
                nowInProgress = "MI_BASERO_14";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Delete_MI_BASERO_14();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion

                #region DELETE MI_WEXPTRNS
                nowInProgress = "MI_WEXPTRNS";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Delete_MI_WEXPTRNS();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion

                #region DELETE 18 RCRATE (818北投不做)
                if (hosp_id != "818") //818北投只有4筆，黃小姐已人工輸入，不必轉檔
                {
                    nowInProgress = "RCRATE";
                    consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                    Console.Write("\n" + consoleLog);
                    db_MMSMS.BeginTransaction();
                    db_MMSMS.cmd.CommandText = repo.Delete_RCRATE();
                    db_MMSMS.cmd.ExecuteNonQuery();
                    db_MMSMS.Commit();
                }
                #endregion

                #region DELETE 20 PH_BANK_AF
                nowInProgress = "PH_BANK_AF";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Delete_PH_BANK_AF();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion

                nowSection = "新增";

                #region INSERT 0.1 MI_MATCLASS(物料分類主檔) --資料寫死
                nowInProgress = "MI_MATCLASS";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // INSERT
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Insert_MI_MATCLASS();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion

                #region 1130314註解INSERT 0.2MI_MCODE,  0.3PARAM_M,  0.4PARAM_D,  0.5MI_DOCTYPE部分
                /*
                #region INSERT 0.2 MI_MCODE
                nowInProgress = "MI_MCODE";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // INSERT
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Insert_MI_MCODE();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion

                #region INSERT 0.3 PARAM_M
                nowInProgress = "PARAM_M";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // INSERT
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Insert_PARAM_M();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion

                #region INSERT 0.4 PARAM_D
                nowInProgress = "PARAM_D";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);

                // 建立 PARAM_D 的 List
                List<PARAM_D> list_803 = new List<PARAM_D>();
                List<PARAM_D> list_804 = new List<PARAM_D>();
                List<PARAM_D> list_805 = new List<PARAM_D>();
                List<PARAM_D> list_811 = new List<PARAM_D>();
                List<PARAM_D> list_812 = new List<PARAM_D>();
                List<PARAM_D> list_813 = new List<PARAM_D>();

                // 塞入 803 PARAM_D 的內容
                list_803.Add(new PARAM_D { DATA_VALUE = "1", DATA_DESC = "藥品" });
                list_803.Add(new PARAM_D { DATA_VALUE = "2", DATA_DESC = "骨材" });
                list_803.Add(new PARAM_D { DATA_VALUE = "3", DATA_DESC = "檢驗試劑" });
                list_803.Add(new PARAM_D { DATA_VALUE = "4", DATA_DESC = "牙科部" });
                list_803.Add(new PARAM_D { DATA_VALUE = "5", DATA_DESC = "氣體" });
                list_803.Add(new PARAM_D { DATA_VALUE = "6", DATA_DESC = "一般衛材" });
                list_803.Add(new PARAM_D { DATA_VALUE = "7", DATA_DESC = "其他" });
                list_803.Add(new PARAM_D { DATA_VALUE = "8", DATA_DESC = "洗腎類" });
                list_803.Add(new PARAM_D { DATA_VALUE = "9", DATA_DESC = "監測試劑" });
                list_803.Add(new PARAM_D { DATA_VALUE = "A", DATA_DESC = "防疫類" });

                // 塞入 804 PARAM_D 的內容
                list_804.Add(new PARAM_D { DATA_VALUE = "1", DATA_DESC = "藥品" });
                list_804.Add(new PARAM_D { DATA_VALUE = "2", DATA_DESC = "衛材" });
                list_804.Add(new PARAM_D { DATA_VALUE = "E", DATA_DESC = " " });

                // 塞入 805 PARAM_D 的內容
                list_805.Add(new PARAM_D { DATA_VALUE = "1", DATA_DESC = "藥品" });
                list_805.Add(new PARAM_D { DATA_VALUE = "2", DATA_DESC = "藥品類衛材" });
                list_805.Add(new PARAM_D { DATA_VALUE = "A", DATA_DESC = "手術室" });
                list_805.Add(new PARAM_D { DATA_VALUE = "B", DATA_DESC = "心臟外科" });
                list_805.Add(new PARAM_D { DATA_VALUE = "C", DATA_DESC = "心臟內科" });
                list_805.Add(new PARAM_D { DATA_VALUE = "D", DATA_DESC = "骨科" });
                list_805.Add(new PARAM_D { DATA_VALUE = "E", DATA_DESC = "供應中心" });
                list_805.Add(new PARAM_D { DATA_VALUE = "F", DATA_DESC = "藥局" });
                list_805.Add(new PARAM_D { DATA_VALUE = "G", DATA_DESC = "洗腎室" });
                list_805.Add(new PARAM_D { DATA_VALUE = "H", DATA_DESC = "麻醉科" });
                list_805.Add(new PARAM_D { DATA_VALUE = "I", DATA_DESC = "眼科" });
                list_805.Add(new PARAM_D { DATA_VALUE = "J", DATA_DESC = "放射科" });
                list_805.Add(new PARAM_D { DATA_VALUE = "K", DATA_DESC = "牙科" });
                list_805.Add(new PARAM_D { DATA_VALUE = "L", DATA_DESC = "內科" });
                list_805.Add(new PARAM_D { DATA_VALUE = "M", DATA_DESC = "產房、嬰兒房" });
                list_805.Add(new PARAM_D { DATA_VALUE = "N", DATA_DESC = "病理" });
                list_805.Add(new PARAM_D { DATA_VALUE = "O", DATA_DESC = "血液腫瘤科" });
                list_805.Add(new PARAM_D { DATA_VALUE = "P", DATA_DESC = "風濕免疫科" });
                list_805.Add(new PARAM_D { DATA_VALUE = "Q", DATA_DESC = "國防預算" });
                list_805.Add(new PARAM_D { DATA_VALUE = "R", DATA_DESC = "檢驗科" });
                list_805.Add(new PARAM_D { DATA_VALUE = "S", DATA_DESC = "ICU" });
                list_805.Add(new PARAM_D { DATA_VALUE = "T", DATA_DESC = "呼吸治療" });
                list_805.Add(new PARAM_D { DATA_VALUE = "U", DATA_DESC = "美容門診" });
                list_805.Add(new PARAM_D { DATA_VALUE = "V", DATA_DESC = "精神科" });
                list_805.Add(new PARAM_D { DATA_VALUE = "W", DATA_DESC = "復健科" });
                list_805.Add(new PARAM_D { DATA_VALUE = "X", DATA_DESC = "家醫科" });
                list_805.Add(new PARAM_D { DATA_VALUE = "Y", DATA_DESC = "耳鼻喉科" });
                list_805.Add(new PARAM_D { DATA_VALUE = "Z", DATA_DESC = "防疫物資" });

                // 塞入 811 PARAM_D 的內容
                list_811.Add(new PARAM_D { DATA_VALUE = "1", DATA_DESC = "藥品" });
                list_811.Add(new PARAM_D { DATA_VALUE = "2", DATA_DESC = "衛材" });
                list_811.Add(new PARAM_D { DATA_VALUE = "3", DATA_DESC = "骨材" });
                list_811.Add(new PARAM_D { DATA_VALUE = "4", DATA_DESC = "檢驗" });
                list_811.Add(new PARAM_D { DATA_VALUE = "5", DATA_DESC = "牙科" });
                list_811.Add(new PARAM_D { DATA_VALUE = "6", DATA_DESC = "儀器零附件" });

                // 塞入 812 PARAM_D 的內容
                list_812.Add(new PARAM_D { DATA_VALUE = "1", DATA_DESC = "藥品" });
                list_812.Add(new PARAM_D { DATA_VALUE = "2", DATA_DESC = "共同衛材" });
                list_812.Add(new PARAM_D { DATA_VALUE = "3", DATA_DESC = "放射科" });
                list_812.Add(new PARAM_D { DATA_VALUE = "4", DATA_DESC = "洗腎室" });
                list_812.Add(new PARAM_D { DATA_VALUE = "5", DATA_DESC = "麻醉科" });
                list_812.Add(new PARAM_D { DATA_VALUE = "6", DATA_DESC = "開刀房" });
                list_812.Add(new PARAM_D { DATA_VALUE = "7", DATA_DESC = "檢驗科" });
                list_812.Add(new PARAM_D { DATA_VALUE = "8", DATA_DESC = "復健科" });
                list_812.Add(new PARAM_D { DATA_VALUE = "9", DATA_DESC = "牙科" });
                list_812.Add(new PARAM_D { DATA_VALUE = "A", DATA_DESC = "急診室" });
                list_812.Add(new PARAM_D { DATA_VALUE = "B", DATA_DESC = "潛醫科" });
                list_812.Add(new PARAM_D { DATA_VALUE = "C", DATA_DESC = "胃鏡室" });
                list_812.Add(new PARAM_D { DATA_VALUE = "D", DATA_DESC = "藥劑科" });
                list_812.Add(new PARAM_D { DATA_VALUE = "E", DATA_DESC = "耳鼻喉科" });
                list_812.Add(new PARAM_D { DATA_VALUE = "F", DATA_DESC = "眼科" });
                list_812.Add(new PARAM_D { DATA_VALUE = "G", DATA_DESC = "正榮診間" });
                list_812.Add(new PARAM_D { DATA_VALUE = "H", DATA_DESC = "孝二診間" });
                list_812.Add(new PARAM_D { DATA_VALUE = "I", DATA_DESC = "供應中心" });
                list_812.Add(new PARAM_D { DATA_VALUE = "J", DATA_DESC = "心超室" });
                list_812.Add(new PARAM_D { DATA_VALUE = "K", DATA_DESC = "藥局衛材" });
                list_812.Add(new PARAM_D { DATA_VALUE = "L", DATA_DESC = "加護病房" });

                // 塞入 813 PARAM_D 的內容
                list_813.Add(new PARAM_D { DATA_VALUE = "1", DATA_DESC = "藥品" });
                list_813.Add(new PARAM_D { DATA_VALUE = "2", DATA_DESC = "骨材" });
                list_813.Add(new PARAM_D { DATA_VALUE = "3", DATA_DESC = "檢驗、監測試劑" });
                list_813.Add(new PARAM_D { DATA_VALUE = "4", DATA_DESC = "牙科部" });
                list_813.Add(new PARAM_D { DATA_VALUE = "5", DATA_DESC = "氣體" });
                list_813.Add(new PARAM_D { DATA_VALUE = "6", DATA_DESC = "一般衛材" });
                list_813.Add(new PARAM_D { DATA_VALUE = "7", DATA_DESC = "其他" });
                list_813.Add(new PARAM_D { DATA_VALUE = "8", DATA_DESC = "眼材" });
                list_813.Add(new PARAM_D { DATA_VALUE = "9", DATA_DESC = "放射衛材" });
                list_813.Add(new PARAM_D { DATA_VALUE = "A", DATA_DESC = "洗腎中心" });
                list_813.Add(new PARAM_D { DATA_VALUE = "B", DATA_DESC = "中藥" });

                List<PARAM_D> list_param_d = new List<PARAM_D>();

                switch (hosp_id)
                {
                    case "803":
                        list_param_d = list_803;
                        break;
                    case "804":
                        list_param_d = list_804;
                        break;
                    case "805":
                        list_param_d = list_805;
                        break;
                    case "811":
                        list_param_d = list_811;
                        break;
                    case "812":
                        list_param_d = list_812;
                        break;
                    case "813":
                        list_param_d = list_813;
                        break;
                    default:
                        break;
                }

                // INSERT [要在 PARAM_M 後新增]
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Insert_PARAM_D(hosp_id, hospName, list_param_d);
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion

                #region INSERT 0.5 MI_DOCTYPE
                nowInProgress = "MI_DOCTYPE";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // INSERT
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Insert_MI_DOCTYPE();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion
                */
                #endregion

                #region INSERT 0.6 BASEUNITCNV(計量單位轉換基本檔) --資料寫死
                nowInProgress = "BASEUNITCNV";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // INSERT
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Insert_BASEUNITCNV();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion

                #region INSERT 0.7 MI_MNSET(月結記錄檔)  --寫入下個月開帳
                nowInProgress = "MI_MNSET";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // INSERT
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Insert_MI_MNSET();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion

                #region 1130314註解INSERT 0.8 ME_FLOW部分
                /*
                #region INSERT 0.8 ME_FLOW
                nowInProgress = "ME_FLOW";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // INSERT
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Insert_ME_FLOW();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion
                */
                #endregion

                #region INSERT 0.9 CHK_MNSET(盤點科室病房自動開單日期設定檔) --寫入目前時間
                nowInProgress = "CHK_MNSET";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // INSERT
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Insert_CHK_MNSET();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion

                #region 1130320註解INSERT 0.10 UR_ID, 0.11 UR_ROLE, 0.12 UR_UIR部分
                /* #region INSERT 0.10 UR_ID(人員基本檔) --固定人員資料寫死
                nowInProgress = "UR_ID";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // INSERT
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Insert_UR_ID();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion

                #region INSERT 0.11 UR_ROLE(角色主檔) --資料寫死
                nowInProgress = "UR_ROLE";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // INSERT
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Insert_UR_ROLE();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion

                #region INERT 0.12 UR_UIR(人員角色關聯檔) --固定人員資料寫死
                nowInProgress = "UR_UIR";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // INSERT
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Insert_UR_UIR();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion */
                #endregion

                #region INSERT 1 MI_UNITCODE(計量單位主檔) --撈HISDG.XFDGUNIT
                nowInProgress = "MI_UNITCODE";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // SELECT
                ds_HISDG = db_HISDG.GetDataSet(repo.Get_MI_UNITCODE(hosp_table_prefix));
                dt_HISDG = ds_HISDG.Tables[0];
                jsonString = JsonConvert.SerializeObject(dt_HISDG, Formatting.Indented);
                IEnumerable<MI_UNITCODE> items1 = JsonConvert.DeserializeObject<IEnumerable<MI_UNITCODE>>(jsonString);

                // INSERT
                int itemIndex1 = 0;
                int items1Count = items1.Count();
                properties = GetPropertyInfo(nowInProgress);
                foreach (MI_UNITCODE item in items1)
                {
                    itemIndex1++;
                    Console.Write("\r{0} [ {1} / {2} ] {3} ", consoleLog, itemIndex1, items1Count, ((double)itemIndex1 / items1Count).ToString("P"));
                    db_MMSMS.cmd.Parameters.Clear();
                    var dictionary = item.AsDictionary();
                    for (var i = 0; i < properties.Length; i++)
                    {
                        db_MMSMS.cmd.Parameters.Add(properties[i].Name, dictionary[properties[i].Name] ?? "");
                    }
                    db_MMSMS.BeginTransaction();
                    db_MMSMS.cmd.CommandText = repo.Insert_MI_UNITCODE();
                    db_MMSMS.cmd.BindByName = true;
                    db_MMSMS.cmd.ExecuteNonQuery();
                    db_MMSMS.Commit();
                }
                #endregion

                #region INSERT 2 PH_VENDER(廠商基本資料檔)  --撈HISDG.XFSUPPLI/XFBANKNO
                nowInProgress = "PH_VENDER";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // SELECT
                ds_HISDG = db_HISDG.GetDataSet(repo.Get_PH_VENDER(hosp_table_prefix));
                dt_HISDG = ds_HISDG.Tables[0];
                jsonString = JsonConvert.SerializeObject(dt_HISDG, Formatting.Indented);
                IEnumerable<PH_VENDER> items2 = JsonConvert.DeserializeObject<IEnumerable<PH_VENDER>>(jsonString);

                // INSERT
                int itemIndex2 = 0;
                int items2Count = items2.Count();
                properties = GetPropertyInfo(nowInProgress);
                foreach (PH_VENDER item in items2)
                {
                    //1130221 805花蓮廠商代碼第1碼=Q不轉入
                    if ((hosp_id != "805") || (item.AGEN_NO.Substring(0, 1) != "Q"))
                    {
                        itemIndex2++;
                        Console.Write("\r{0} [ {1} / {2} ] {3} ", consoleLog, itemIndex2, items2Count, ((double)itemIndex2 / items2Count).ToString("P"));
                        if (!string.IsNullOrWhiteSpace(item.AGEN_ACC))
                        {
                            // 處理 dash 跟 blank (包含半形跟全形)
                            item.AGEN_ACC = item.AGEN_ACC.Replace(" ", "").Replace("-", "").Replace("_", "").Replace("　", "").Replace("－", "").Replace("＿", "");
                        }
                        db_MMSMS.cmd.Parameters.Clear();
                        var dictionary = item.AsDictionary();
                        for (var i = 0; i < properties.Length; i++)
                        {
                            db_MMSMS.cmd.Parameters.Add(properties[i].Name, dictionary[properties[i].Name] ?? "");
                        }
                        db_MMSMS.BeginTransaction();
                        db_MMSMS.cmd.CommandText = repo.Insert_PH_VENDER();
                        db_MMSMS.cmd.BindByName = true;
                        db_MMSMS.cmd.ExecuteNonQuery();
                        db_MMSMS.Commit();
                    }
                }
                #endregion

                #region INSERT 3 MI_UNITEXCH(計量單位轉換率檔)  --撈HISDG.XFUTCOST/XFCHEMIS
                nowInProgress = "MI_UNITEXCH";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // SELECT
                ds_HISDG = db_HISDG.GetDataSet(repo.Get_MI_UNITEXCH(hosp_table_prefix));
                dt_HISDG = ds_HISDG.Tables[0];
                jsonString = JsonConvert.SerializeObject(dt_HISDG, Formatting.Indented);
                IEnumerable<MI_UNITEXCH> items3 = JsonConvert.DeserializeObject<IEnumerable<MI_UNITEXCH>>(jsonString);

                // INSERT
                int itemIndex3 = 0;
                int items3Count = items3.Count();
                properties = GetPropertyInfo(nowInProgress);
                foreach (MI_UNITEXCH item in items3)
                {
                    try
                    {
                        itemIndex3++;
                        Console.Write("\r{0} [ {1} / {2} ] {3} ", consoleLog, itemIndex3, items3Count, ((double)itemIndex3 / items3Count).ToString("P"));
                        db_MMSMS.cmd.Parameters.Clear();
                        var dictionary = item.AsDictionary();
                        for (var i = 0; i < properties.Length; i++)
                        {
                            db_MMSMS.cmd.Parameters.Add(properties[i].Name, dictionary[properties[i].Name] ?? "");
                        }
                        db_MMSMS.BeginTransaction();
                        db_MMSMS.cmd.CommandText = repo.Insert_MI_UNITEXCH();
                        db_MMSMS.cmd.BindByName = true;
                        db_MMSMS.cmd.ExecuteNonQuery();
                        db_MMSMS.Commit();
                    }
                    catch (Exception e)
                    {
                        log.Exception_To_Log(string.Format(@"
                        messsage: MI_UNITEXCH {0}
                        StackTrace: {1}
                        ---------------
                        object: {2}
                        ", e.Message, e.StackTrace, JsonConvert.SerializeObject(item, Formatting.Indented)
                        ), "DataTransfer14", "DataTransfer14");
                        db_MMSMS.Rollback();
                    }

                }
                #endregion

                #region 1130320註解INSERT 4 UR_INID部分
                /* #region INSERT 4 UR_INID(責任中心主檔)  --撈HISDG.XFSTATIO
                nowInProgress = "UR_INID";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // SELECT
                ds_HISDG = db_HISDG.GetDataSet(repo.Get_UR_INID(hosp_table_prefix));
                dt_HISDG = ds_HISDG.Tables[0];
                jsonString = JsonConvert.SerializeObject(dt_HISDG, Formatting.Indented);
                IEnumerable<UR_INID> items4 = JsonConvert.DeserializeObject<IEnumerable<UR_INID>>(jsonString);

                // INSERT
                int itemIndex4 = 0;
                int items4Count = items4.Count();
                properties = GetPropertyInfo(nowInProgress);
                foreach (UR_INID item in items4)
                {
                    itemIndex4++;
                    Console.Write("\r{0} [ {1} / {2} ] {3} ", consoleLog, itemIndex4, items4Count, ((double)itemIndex4 / items4Count).ToString("P"));
                    db_MMSMS.cmd.Parameters.Clear();
                    var dictionary = item.AsDictionary();
                    for (var i = 0; i < properties.Length; i++)
                    {
                        db_MMSMS.cmd.Parameters.Add(properties[i].Name, dictionary[properties[i].Name] ?? "");
                    }
                    db_MMSMS.BeginTransaction();
                    db_MMSMS.cmd.CommandText = repo.Insert_UR_INID();
                    db_MMSMS.cmd.BindByName = true;
                    db_MMSMS.cmd.ExecuteNonQuery();
                    db_MMSMS.Commit();
                }
                #endregion */
                #endregion

                #region INSERT 5 MI_WHMAST(庫房基本檔) 各家醫院以人工判斷 --資料寫死
                nowInProgress = "MI_WHMAST";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // INSERT
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Insert_MI_WHMAST(hosp_id);
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion

                #region INSERT 6 MI_MAST(藥衛材基本檔)  --撈HISDG.XFCHEMIS/XFUTCOST
                nowInProgress = "MI_MAST";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // SELECT
                ds_HISDG = db_HISDG.GetDataSet(repo.Get_MI_MAST(hosp_id, hosp_table_prefix));
                //dt_HISDG = ds_HISDG.Tables[0] ?? null;
                dt_HISDG = ds_HISDG.Tables[0];
                jsonString = JsonConvert.SerializeObject(dt_HISDG, Formatting.Indented);
                IEnumerable<MI_MAST> items6 = JsonConvert.DeserializeObject<IEnumerable<MI_MAST>>(jsonString);
                //IEnumerable<MI_MAST> temppp = items6.Where(x => x.M_STOREID.Length > 1).Select(x => x).ToList();

                // INSERT [要在 MI_MATCLASS 後新增]
                int itemIndex6 = 0;
                int items6Count = items6.Count();
                properties = GetPropertyInfo(nowInProgress);
                foreach (MI_MAST item in items6)
                {
                    try
                    {
                        itemIndex6++;
                        Console.Write("\r{0} [ {1} / {2} ] {3} ", consoleLog, itemIndex6, items6Count, ((double)itemIndex6 / items6Count).ToString("P"));

                        double d = new double();
                        if (string.IsNullOrEmpty(item.NHI_PRICE))
                        {
                            item.NHI_PRICE = "0";
                        }
                        if (double.TryParse(item.NHI_PRICE, out d) == false)
                        {
                            log.Exception_To_Log(string.Format(@"
                        messsage: MI_MAST item.NHI_PRICE not number 
                        {0}
                        ", JsonConvert.SerializeObject(item, Formatting.Indented)
                        //JsonConvert.SerializeObject(rx)
                        ), "DataTransfer14", "DataTransfer14");
                            continue;
                        }

                        db_MMSMS.cmd.Parameters.Clear();
                        var dictionary = item.AsDictionary();
                        for (var i = 0; i < properties.Length; i++)
                        {
                            db_MMSMS.cmd.Parameters.Add(properties[i].Name, dictionary[properties[i].Name] ?? "");
                        }
                        db_MMSMS.BeginTransaction();
                        db_MMSMS.cmd.CommandText = repo.Insert_MI_MAST();
                        db_MMSMS.cmd.BindByName = true;
                        db_MMSMS.cmd.ExecuteNonQuery();
                        db_MMSMS.Commit();
                    }
                    catch (Exception e)
                    {
                        log.Exception_To_Log(string.Format(@"
                        messsage: MI_MAST {0}
                        StackTrace: {1}
                        ---------------
                        object: {2}
                        ", e.Message, e.StackTrace, JsonConvert.SerializeObject(item, Formatting.Indented)
                        ), "DataTransfer14", "DataTransfer14");
                        db_MMSMS.Rollback();
                    }

                }
                Thread.Sleep(1000);
                #endregion

                #region INSERT 7 MI_WHINV(庫房庫存檔) --撈目前HISDG.XFINVENT&MMSADM.MI_WHINV/MI_WHMAST
                nowInProgress = "MI_WHINV";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // SELECT 
                wh_kind = "0"; //取藥品庫資料
                strXfKindID = "1";
                ds_HISDG_SubQuery1 = db_HISDG.GetDataSet(repo.Get_MI_WHINV_HISDG_SubQuery(strXfKindID, hosp_table_prefix));
                dt_HISDG_SubQuery1 = ds_HISDG_SubQuery1.Tables[0];
                List<MI_WHINV> list_whinv_HISDG_SubQuery1 = ConvertDataTable<MI_WHINV>(dt_HISDG_SubQuery1);

                ds_MMSMS_SubQuery1 = db_MMSMS.GetDataSet(repo.Get_MI_WHINV_MMSMS_SubQuery(wh_kind));
                dt_MMSMS_SubQuery1 = ds_MMSMS_SubQuery1.Tables[0];
                List<MI_WHMAST> list_whinv_MMSMS_SubQuery1 = ConvertDataTable<MI_WHMAST>(dt_MMSMS_SubQuery1); //跨表取WH_NO,INID

                wh_kind = "1"; //取衛材庫資料
                strXfKindID = "0"; //不等於1
                ds_HISDG_SubQuery2 = db_HISDG.GetDataSet(repo.Get_MI_WHINV_HISDG_SubQuery(strXfKindID, hosp_table_prefix));
                dt_HISDG_SubQuery2 = ds_HISDG_SubQuery2.Tables[0];
                List<MI_WHINV> list_whinv_HISDG_SubQuery2 = ConvertDataTable<MI_WHINV>(dt_HISDG_SubQuery2);

                ds_MMSMS_SubQuery2 = db_MMSMS.GetDataSet(repo.Get_MI_WHINV_MMSMS_SubQuery(wh_kind));
                dt_MMSMS_SubQuery2 = ds_MMSMS_SubQuery2.Tables[0];
                List<MI_WHMAST> list_whinv_MMSMS_SubQuery2 = ConvertDataTable<MI_WHMAST>(dt_MMSMS_SubQuery2); //跨表取WH_NO,INID

                //A.查詢藥品集合
                List<MI_WHINV> temp_whinv_query1
                    = (from x in list_whinv_HISDG_SubQuery1
                       join y in list_whinv_MMSMS_SubQuery1 on x.STRDEPID equals y.INID

                       select new MI_WHINV
                       {
                           WH_NO = y.WH_NO,
                           MMCODE = x.STRDRUGID,
                           INV_QTY = x.LNGNOWREST
                       }).ToList<MI_WHINV>();

                //B.查詢衛材集合
                List<MI_WHINV> temp_whinv_query2
                    = (from x in list_whinv_HISDG_SubQuery2
                       join y in list_whinv_MMSMS_SubQuery2 on x.STRDEPID equals y.INID

                       select new MI_WHINV
                       {
                           WH_NO = y.WH_NO,
                           MMCODE = x.STRDRUGID,
                           INV_QTY = x.LNGNOWREST
                       }).ToList<MI_WHINV>();

                //C.將兩者進行合併(過濾WH_NO is not null)
                IEnumerable<MI_WHINV> result1 = temp_whinv_query1.Concat(temp_whinv_query2).Where(x => string.IsNullOrEmpty(x.WH_NO) == false).Select(x => x).ToList();

                // INSERT [要在 MI_MAST MI_WHMAST 後新增]
                int itemIndex7 = 0;
                int items7Count = result1.Count();
                properties = GetPropertyInfo(nowInProgress);
                foreach (MI_WHINV item in result1)
                {
                    try
                    {
                        itemIndex7++;
                        Console.Write("\r{0} [ {1} / {2} ] {3} ", consoleLog, itemIndex7, items7Count, ((double)itemIndex7 / items7Count).ToString("P"));
                        db_MMSMS.cmd.Parameters.Clear();
                        var dictionary = item.AsDictionary();
                        //for (var i = 0; i < properties.Length; i++)
                        //{
                        //    db_MMSMS.cmd.Parameters.Add(properties[i].Name, dictionary[properties[i].Name] ?? "");
                        //}
                        db_MMSMS.cmd.Parameters.Add("WH_NO", item.WH_NO ?? "");
                        db_MMSMS.cmd.Parameters.Add("MMCODE", item.MMCODE ?? "");
                        db_MMSMS.cmd.Parameters.Add("INV_QTY", item.INV_QTY ?? "");

                        db_MMSMS.BeginTransaction();
                        db_MMSMS.cmd.CommandText = repo.Insert_MI_WHINV();
                        db_MMSMS.cmd.BindByName = true;
                        db_MMSMS.cmd.ExecuteNonQuery();
                        db_MMSMS.Commit();
                    }
                    catch (Exception e)
                    {
                        CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                        callDbtools_oralce.I_ERROR_LOG("DataTransfer14", e.Message, "AUTO");

                        log.Exception_To_Log(string.Format(@"
                        messsage: MI_WHINV {0} 
                        StackTrace: {1}
                        ---------------
                        object: {2}
                        ", e.Message, e.StackTrace, JsonConvert.SerializeObject(item, Formatting.Indented)
                           ), "DataTransfer14", "DataTransfer14");

                        db_MMSMS.Rollback();
                    }
                }
                Thread.Sleep(1000);
                #endregion

                #region INSERT 8 MI_WINVCTL(庫房存量管制檔) --撈目前HISDG.XFSTKTOL/XFCHEMIS&MMSADM.MI_WHMAST
                nowInProgress = "MI_WINVCTL";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // SELECT 
                ds_HISDG_SubQuery1 = db_HISDG.GetDataSet(repo.Get_MI_WINVCTL_HISDG_SubQuery(hosp_table_prefix));
                dt_HISDG_SubQuery1 = ds_HISDG_SubQuery1.Tables[0];
                List<MI_WINVCTL> list_winvctl_HISDG_SubQuery1 = ConvertDataTable<MI_WINVCTL>(dt_HISDG_SubQuery1);

                ds_MMSMS_SubQuery1 = db_MMSMS.GetDataSet(repo.Get_MI_WINVCTL_MMSMS_SubQuery());
                dt_MMSMS_SubQuery1 = ds_MMSMS_SubQuery1.Tables[0];
                List<MI_WHMAST> list_winvctl_MMSMS_SubQuery1 = ConvertDataTable<MI_WHMAST>(dt_MMSMS_SubQuery1);

                List<MI_WINVCTL> result2 =
                    (from a in list_winvctl_HISDG_SubQuery1
                     from b in list_winvctl_MMSMS_SubQuery1
                     where a.STRDEPID == b.INID
                     group new { WH_NO = b.WH_NO, MMCODE = a.STRDRUGID } by new { b.WH_NO, a.STRDRUGID } into g
                     select new MI_WINVCTL
                     {
                         WH_NO = g.Key.WH_NO,
                         MMCODE = g.Key.STRDRUGID
                     }).ToList<MI_WINVCTL>();

                //jsonString = JsonConvert.SerializeObject(result2, Formatting.Indented);
                //IEnumerable<MI_WINVCTL> items8 = JsonConvert.DeserializeObject<IEnumerable<MI_WINVCTL>>(jsonString);
                //IEnumerable<MI_MAST> temppp = items6.Where(x => x.M_STOREID.Length > 1).Select(x => x).ToList();

                // INSERT [要在 MI_MAST MI_WHMAST 後新增]
                int itemIndex8 = 0;
                int items8Count = result2.Count();
                properties = GetPropertyInfo(nowInProgress);
                foreach (MI_WINVCTL item in result2)
                {
                    try
                    {
                        itemIndex8++;
                        Console.Write("\r{0} [ {1} / {2} ] {3} ", consoleLog, itemIndex8, items8Count, ((double)itemIndex8 / items8Count).ToString("P"));
                        db_MMSMS.cmd.Parameters.Clear();
                        var dictionary = item.AsDictionary();
                        for (var i = 0; i < properties.Length; i++)
                        {
                            db_MMSMS.cmd.Parameters.Add(properties[i].Name, dictionary[properties[i].Name] ?? "");
                        }
                        db_MMSMS.BeginTransaction();
                        db_MMSMS.cmd.CommandText = repo.Insert_MI_WINVCTL();
                        db_MMSMS.cmd.BindByName = true;
                        db_MMSMS.cmd.ExecuteNonQuery();
                        db_MMSMS.Commit();
                    }
                    catch (Exception e)
                    {
                        CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                        callDbtools_oralce.I_ERROR_LOG("DataTransfer14", e.Message, "AUTO");

                        log.Exception_To_Log(string.Format(@"
                        messsage: MI_WINVCTL {0}
                        StackTrace: {1}
                        ---------------
                        object: {2}
                        ", e.Message, e.StackTrace, JsonConvert.SerializeObject(item, Formatting.Indented)
                           ), "DataTransfer14", "DataTransfer14");

                        db_MMSMS.Rollback();
                    }
                }
                Thread.Sleep(1000);
                #endregion

                #region INSERT 9_1 BC_BARCODE barcode(品項條碼資料檔) --撈目前HISDG.XFCHEMIS
                nowInProgress = "BC_BARCODE";
                nowInProgressSub = "[barcode]";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // SELECT BC_BARCODE barcode
                ds_HISDG = db_HISDG.GetDataSet(repo.Get_BC_BARCODE_Barcode(hosp_table_prefix));
                dt_HISDG = ds_HISDG.Tables[0];
                jsonString = JsonConvert.SerializeObject(dt_HISDG, Formatting.Indented);
                IEnumerable<BC_BARCODE> items9_1 = JsonConvert.DeserializeObject<IEnumerable<BC_BARCODE>>(jsonString);

                // INSERT BC_BARCODE barcode
                int itemIndex9_1 = 0;
                int items9_1Count = items9_1.Count();
                properties = GetPropertyInfo(nowInProgress);
                foreach (BC_BARCODE item in items9_1)
                {
                    try
                    {
                        itemIndex9_1++;
                        Console.Write("\r{0} [ {1} / {2} ] {3} ", consoleLog, itemIndex9_1, items9_1Count, ((double)itemIndex9_1 / items9_1Count).ToString("P"));
                        db_MMSMS.cmd.Parameters.Clear();
                        var dictionary = item.AsDictionary();
                        for (var i = 0; i < properties.Length; i++)
                        {
                            db_MMSMS.cmd.Parameters.Add(properties[i].Name, dictionary[properties[i].Name] ?? "");
                        }
                        db_MMSMS.BeginTransaction();
                        db_MMSMS.cmd.CommandText = repo.Insert_BC_BARCODE();
                        db_MMSMS.cmd.BindByName = true;
                        db_MMSMS.cmd.ExecuteNonQuery();
                        db_MMSMS.Commit();
                    }
                    catch (Exception e)
                    {
                        CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                        callDbtools_oralce.I_ERROR_LOG("DataTransfer14", e.Message, "AUTO");

                        log.Exception_To_Log(string.Format(@"
                        messsage: BC_BARCODE barcode {0}
                        StackTrace: {1}
                        ---------------
                        object: {2}
                        ", e.Message, e.StackTrace, JsonConvert.SerializeObject(item, Formatting.Indented)
                           ), "DataTransfer14", "DataTransfer14");

                        db_MMSMS.Rollback();
                    }

                }
                Thread.Sleep(1000);
                #endregion

                #region 1130318註解INSERT 9_2 BC_BARCODE mmcode (MI_MAST已有Trigger)
                /* nowInProgress = "BC_BARCODE";
                nowInProgressSub = "[mmcode]";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // SELECT BC_BARCODE mmcode
                ds_HISDG = db_HISDG.GetDataSet(repo.Get_BC_BARCODE_Mmcode(hosp_table_prefix));
                dt_HISDG = ds_HISDG.Tables[0];
                jsonString = JsonConvert.SerializeObject(dt_HISDG, Formatting.Indented);
                IEnumerable<BC_BARCODE> items9_2 = JsonConvert.DeserializeObject<IEnumerable<BC_BARCODE>>(jsonString);

                // INSERT BC_BARCODE mmcode
                int itemIndex9_2 = 0;
                int items9_2Count = items9_2.Count();
                properties = GetPropertyInfo(nowInProgress);
                foreach (BC_BARCODE item in items9_2)
                {
                    try
                    {
                        itemIndex9_2++;
                        Console.Write("\r{0} [ {1} / {2} ] {3} ", consoleLog, itemIndex9_2, items9_2Count, ((double)itemIndex9_2 / items9_2Count).ToString("P"));
                        db_MMSMS.cmd.Parameters.Clear();
                        var dictionary = item.AsDictionary();
                        for (var i = 0; i < properties.Length; i++)
                        {
                            db_MMSMS.cmd.Parameters.Add(properties[i].Name, dictionary[properties[i].Name] ?? "");
                        }
                        db_MMSMS.BeginTransaction();
                        db_MMSMS.cmd.CommandText = repo.Insert_BC_BARCODE();
                        db_MMSMS.cmd.BindByName = true;
                        db_MMSMS.cmd.ExecuteNonQuery();
                        db_MMSMS.Commit();
                    }
                    catch (Exception e)
                    {
                        CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                        callDbtools_oralce.I_ERROR_LOG("DataTransfer14", e.Message, "AUTO");

                        log.Exception_To_Log(string.Format(@"
                        messsage: BC_BARCODE mmcode {0}
                        StackTrace: {1}
                        ---------------
                        object: {2}
                        ", e.Message, e.StackTrace, JsonConvert.SerializeObject(item, Formatting.Indented)
                           //JsonConvert.SerializeObject(rx)
                           ), "DataTransfer14", "DataTransfer14");

                        db_MMSMS.Rollback();
                        //db_MMSMS.Dispose();

                        //db_HISDG.Rollback();
                        //db_HISDG.Dispose();
                        //Console.ReadLine();
                    }

                }
                Thread.Sleep(1000); */
                #endregion

                #region INSERT MI_WHID(庫房使用人員檔) --固定人員資料寫死
                nowInProgress = "MI_WHID";
                nowInProgressSub = string.Empty;
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // INSERT
                try
                {
                    db_MMSMS.BeginTransaction();
                    db_MMSMS.cmd.CommandText = repo.Insert_MI_WHID();
                    db_MMSMS.cmd.ExecuteNonQuery();
                    db_MMSMS.Commit();
                }
                catch (Exception e)
                {
                    CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                    callDbtools_oralce.I_ERROR_LOG("DataTransfer14", e.Message, "AUTO");

                    log.Exception_To_Log(string.Format(@"
                        messsage: MI_WHID {0}
                        StackTrace: {1}
                        ---------------
                        object: {2}
                        ", e.Message, e.StackTrace, db_MMSMS.cmd.CommandText
                       ), "DataTransfer14", "DataTransfer14");

                    db_MMSMS.Rollback();
                }
                Thread.Sleep(1000);
                #endregion

                #region INSERT 10 MI_WLOCINV(庫房儲位檔) --撈目前HISDG.XFMSBANK/XFCHEMIS&MMSADM.MI_WHMAST
                nowInProgress = "MI_WLOCINV";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // SELECT 
                wh_kind = "0";
                ds_HISDG_SubQuery1 = db_HISDG.GetDataSet(repo.Get_MI_WLOCINV_HISDG_SubQuery(wh_kind, hosp_table_prefix));
                dt_HISDG_SubQuery1 = ds_HISDG_SubQuery1.Tables[0];
                List<MI_WLOCINV> list_wlocinv_HISDG_SubQuery1 = ConvertDataTable<MI_WLOCINV>(dt_HISDG_SubQuery1);

                ds_MMSMS_SubQuery1 = db_MMSMS.GetDataSet(repo.Get_MI_WLOCINV_MMSMS_SubQuery(wh_kind));
                dt_MMSMS_SubQuery1 = ds_MMSMS_SubQuery1.Tables[0];
                List<MI_WHMAST> list_wlocinv_MMSMS_SubQuery1 = ConvertDataTable<MI_WHMAST>(dt_MMSMS_SubQuery1);

                wh_kind = "1";
                ds_HISDG_SubQuery2 = db_HISDG.GetDataSet(repo.Get_MI_WLOCINV_HISDG_SubQuery(wh_kind, hosp_table_prefix));
                dt_HISDG_SubQuery2 = ds_HISDG_SubQuery2.Tables[0];
                List<MI_WLOCINV> list_wlocinv_HISDG_SubQuery2 = ConvertDataTable<MI_WLOCINV>(dt_HISDG_SubQuery2);

                ds_MMSMS_SubQuery2 = db_MMSMS.GetDataSet(repo.Get_MI_WLOCINV_MMSMS_SubQuery(wh_kind));
                dt_MMSMS_SubQuery2 = ds_MMSMS_SubQuery2.Tables[0];
                List<MI_WHMAST> list_wlocinv_MMSMS_SubQuery2 = ConvertDataTable<MI_WHMAST>(dt_MMSMS_SubQuery2);

                List<MI_WLOCINV> temp_wlocinv_query1
                    = (from a in list_wlocinv_HISDG_SubQuery1
                       from b in list_wlocinv_MMSMS_SubQuery1
                       where a.STRDEPID == b.INID
                       group new { WH_NO = b.WH_NO, MMCODE = a.STRDRUGID, STORE_LOC = a.STRSTOREPOS, INV_QTY = a.LNGDRUGAMT } by new { b.WH_NO, a.STRDRUGID, a.STRSTOREPOS } into g

                       select new MI_WLOCINV
                       {
                           WH_NO = g.Key.WH_NO,
                           MMCODE = g.Key.STRDRUGID,
                           STORE_LOC = g.Key.STRSTOREPOS,
                           INV_QTY = g.Sum(x => double.Parse(x.INV_QTY)).ToString()
                       }).ToList<MI_WLOCINV>();

                List<MI_WLOCINV> temp_wlocinv_query2
                    = (from a in list_wlocinv_HISDG_SubQuery2
                       from b in list_wlocinv_MMSMS_SubQuery2
                       where a.STRDEPID == b.INID
                       group new { WH_NO = b.WH_NO, MMCODE = a.STRDRUGID, STORE_LOC = a.STRSTOREPOS, INV_QTY = a.LNGDRUGAMT } by new { b.WH_NO, a.STRDRUGID, a.STRSTOREPOS } into g
                       select new MI_WLOCINV
                       {
                           WH_NO = g.Key.WH_NO,
                           MMCODE = g.Key.STRDRUGID,
                           STORE_LOC = g.Key.STRSTOREPOS,
                           INV_QTY = g.Sum(x => double.Parse(x.INV_QTY)).ToString()
                       }).ToList<MI_WLOCINV>();

                IEnumerable<MI_WLOCINV> result_wlocinv = temp_wlocinv_query1.Concat(temp_wlocinv_query2).Where(x => string.IsNullOrEmpty(x.WH_NO) == false).Select(x => x).ToList();

                // INSERT [要在 MI_MAST MI_WHMAST 後新增]
                int itemIndex10 = 0;
                int items10Count = result_wlocinv.Count();
                properties = GetPropertyInfo(nowInProgress);
                foreach (MI_WLOCINV item in result_wlocinv)
                {
                    try
                    {
                        itemIndex10++;
                        Console.Write("\r{0} [ {1} / {2} ] {3} ", consoleLog, itemIndex10, items10Count, ((double)itemIndex10 / items10Count).ToString("P"));
                        db_MMSMS.cmd.Parameters.Clear();
                        var dictionary = item.AsDictionary();
                        //for (var i = 0; i < properties.Length; i++)
                        //{
                        //    db_MMSMS.cmd.Parameters.Add(properties[i].Name, dictionary[properties[i].Name] ?? "");
                        //}
                        db_MMSMS.cmd.Parameters.Add("WH_NO", item.WH_NO ?? "");
                        db_MMSMS.cmd.Parameters.Add("MMCODE", item.MMCODE ?? "");
                        db_MMSMS.cmd.Parameters.Add("STORE_LOC", item.STORE_LOC ?? "");
                        db_MMSMS.cmd.Parameters.Add("INV_QTY", item.INV_QTY ?? "");

                        db_MMSMS.BeginTransaction();
                        db_MMSMS.cmd.CommandText = repo.Insert_MI_WLOCINV();
                        db_MMSMS.cmd.BindByName = true;
                        db_MMSMS.cmd.ExecuteNonQuery();
                        db_MMSMS.Commit();
                    }
                    catch (Exception e)
                    {
                        CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                        callDbtools_oralce.I_ERROR_LOG("DataTransfer14", e.Message, "AUTO");

                        log.Exception_To_Log(string.Format(@"
                        messsage: MI_WLOCINV {0} 
                        StackTrace: {1}
                        ---------------
                        object: {2}
                        ", e.Message, e.StackTrace, JsonConvert.SerializeObject(item, Formatting.Indented)
                           ), "DataTransfer14", "DataTransfer14");

                        db_MMSMS.Rollback();
                    }
                }
                Thread.Sleep(1000);

                #endregion

                #region INSERT 10校正 MI_WLOCINV(以庫存檔存量來校正儲位檔存量)
                nowInProgress = "MI_WLOCINV";
                nowInProgressSub = string.Empty;
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // SELECT
                ds_MMSMS = db_MMSMS.GetDataSet(repo.Get_MI_WHINV_LOC());
                dt_MMSMS = ds_MMSMS.Tables[0];
                jsonString = JsonConvert.SerializeObject(dt_MMSMS, Formatting.Indented);
                IEnumerable<MI_WLOCINV> items10_2 = JsonConvert.DeserializeObject<IEnumerable<MI_WLOCINV>>(jsonString);

                // INSERT
                int itemIndex10_2 = 0;
                int items10_2Count = items10_2.Count();
                properties = GetPropertyInfo(nowInProgress);
                foreach (MI_WLOCINV item in items10_2)
                {
                    try
                    {
                        itemIndex10_2++;
                        Console.Write("\r{0} [ {1} / {2} ] {3} ", consoleLog, itemIndex10_2, items10_2Count, ((double)itemIndex10_2 / items10_2Count).ToString("P"));
                        db_MMSMS.cmd.Parameters.Clear();
                        var dictionary = item.AsDictionary();
                        for (var i = 0; i < properties.Length; i++)
                        {
                            db_MMSMS.cmd.Parameters.Add(properties[i].Name, dictionary[properties[i].Name] ?? "");
                        }
                        db_MMSMS.BeginTransaction();
                        db_MMSMS.cmd.CommandText = repo.MERGE_MI_WLOCINV();
                        db_MMSMS.cmd.BindByName = true;
                        db_MMSMS.cmd.ExecuteNonQuery();
                        db_MMSMS.Commit();
                    }
                    catch (Exception e)
                    {
                        CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                        callDbtools_oralce.I_ERROR_LOG("DataTransfer14", e.Message, "AUTO");

                        log.Exception_To_Log(string.Format(@"
                        messsage: MI_WLOCINV
                        StackTrace: {1}
                        ---------------
                        object: {2}
                        ", e.Message, e.StackTrace, JsonConvert.SerializeObject(item, Formatting.Indented)
                           ), "DataTransfer14", "DataTransfer14");

                        db_MMSMS.Rollback();
                    }
                }
                Thread.Sleep(1000);
                #endregion

                #region INSERT 11 MI_WEXPINV(庫房批號效期檔) 因部分醫院沒有批號欄位，暫時寫死為TMPLOT --撈目前HISDG.XFMSBANK/XFCHEMIS&MMSADM.MI_WHMAST
                nowInProgress = "MI_WEXPINV";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // SELECT 
                wh_kind = "0";
                ds_HISDG_SubQuery1 = db_HISDG.GetDataSet(repo.Get_MI_WEXPINV_HISDG_SubQuery(wh_kind, hosp_table_prefix));
                dt_HISDG_SubQuery1 = ds_HISDG_SubQuery1.Tables[0];
                List<MI_WEXPINV> list_WEXPINV_HISDG_SubQuery1 = ConvertDataTable<MI_WEXPINV>(dt_HISDG_SubQuery1);

                ds_MMSMS_SubQuery1 = db_MMSMS.GetDataSet(repo.Get_MI_WEXPINV_MMSMS_SubQuery(wh_kind));
                dt_MMSMS_SubQuery1 = ds_MMSMS_SubQuery1.Tables[0];
                List<MI_WHMAST> list_WEXPINV_MMSMS_SubQuery1 = ConvertDataTable<MI_WHMAST>(dt_MMSMS_SubQuery1);

                wh_kind = "1";
                ds_HISDG_SubQuery2 = db_HISDG.GetDataSet(repo.Get_MI_WEXPINV_HISDG_SubQuery(wh_kind, hosp_table_prefix));
                dt_HISDG_SubQuery2 = ds_HISDG_SubQuery2.Tables[0];
                List<MI_WEXPINV> list_WEXPINV_HISDG_SubQuery2 = ConvertDataTable<MI_WEXPINV>(dt_HISDG_SubQuery2);

                ds_MMSMS_SubQuery2 = db_MMSMS.GetDataSet(repo.Get_MI_WEXPINV_MMSMS_SubQuery(wh_kind));
                dt_MMSMS_SubQuery2 = ds_MMSMS_SubQuery2.Tables[0];
                List<MI_WHMAST> list_WEXPINV_MMSMS_SubQuery2 = ConvertDataTable<MI_WHMAST>(dt_MMSMS_SubQuery2);

                List<MI_WEXPINV> temp_WEXPINV_query1
                    = (from a in list_WEXPINV_HISDG_SubQuery1
                       from b in list_WEXPINV_MMSMS_SubQuery1
                       where a.STRDEPID == b.INID
                       group new { WH_NO = b.WH_NO, MMCODE = a.STRDRUGID, EXP_DATE = a.STRDATELIMIT, LOT_NO = a.STRMADEBATCHNO, INV_QTY = a.LNGDRUGAMT } by new { b.WH_NO, a.STRDRUGID, a.STRDATELIMIT, a.STRMADEBATCHNO, a.LNGDRUGAMT } into g
                       select new MI_WEXPINV
                       {
                           WH_NO = g.Key.WH_NO,
                           MMCODE = g.Key.STRDRUGID,
                           EXP_DATE = g.Key.STRDATELIMIT,
                           LOT_NO = g.Key.STRMADEBATCHNO,
                           INV_QTY = g.Sum(x => double.Parse(x.INV_QTY)).ToString()
                       }).ToList<MI_WEXPINV>();

                List<MI_WEXPINV> temp_WEXPINV_query2
                    = (from a in list_WEXPINV_HISDG_SubQuery2
                       from b in list_WEXPINV_MMSMS_SubQuery2
                       where a.STRDEPID == b.INID
                       group new { WH_NO = b.WH_NO, MMCODE = a.STRDRUGID, EXP_DATE = a.STRDATELIMIT, LOT_NO = a.STRMADEBATCHNO, INV_QTY = a.LNGDRUGAMT } by new { b.WH_NO, a.STRDRUGID, a.STRDATELIMIT, a.STRMADEBATCHNO, a.LNGDRUGAMT } into g
                       select new MI_WEXPINV
                       {
                           WH_NO = g.Key.WH_NO,
                           MMCODE = g.Key.STRDRUGID,
                           EXP_DATE = g.Key.STRDATELIMIT,
                           LOT_NO = g.Key.STRMADEBATCHNO,
                           INV_QTY = g.Sum(x => double.Parse(x.INV_QTY)).ToString()
                       }).ToList<MI_WEXPINV>();

                IEnumerable<MI_WEXPINV> result_WEXPINV = temp_WEXPINV_query1.Concat(temp_WEXPINV_query2).Where(x => string.IsNullOrEmpty(x.WH_NO) == false).Select(x => x).ToList();

                // INSERT [要在 MI_MAST MI_WHMAST 後新增]
                int itemIndex11 = 0;
                int items11Count = result_WEXPINV.Count();
                properties = GetPropertyInfo(nowInProgress);
                foreach (MI_WEXPINV item in result_WEXPINV)
                {
                    try
                    {
                        itemIndex11++;
                        Console.Write("\r{0} [ {1} / {2} ] {3} ", consoleLog, itemIndex11, items11Count, ((double)itemIndex11 / items11Count).ToString("P"));
                        db_MMSMS.cmd.Parameters.Clear();
                        var dictionary = item.AsDictionary();
                        //for (var i = 0; i < properties.Length; i++)
                        //{
                        //    db_MMSMS.cmd.Parameters.Add(properties[i].Name, dictionary[properties[i].Name] ?? "");
                        //}
                        if (string.IsNullOrEmpty(item.LOT_NO))
                            item.LOT_NO = "TMPLOT";
                        else if (item.LOT_NO.Trim() == "")
                            item.LOT_NO = "TMPLOT";

                        db_MMSMS.cmd.Parameters.Add("WH_NO", item.WH_NO ?? "");
                        db_MMSMS.cmd.Parameters.Add("MMCODE", item.MMCODE ?? "");
                        db_MMSMS.cmd.Parameters.Add("EXP_DATE", item.EXP_DATE ?? "");
                        db_MMSMS.cmd.Parameters.Add("LOT_NO", item.LOT_NO ?? "");
                        db_MMSMS.cmd.Parameters.Add("INV_QTY", item.INV_QTY ?? "");

                        db_MMSMS.BeginTransaction();
                        db_MMSMS.cmd.CommandText = repo.Insert_MI_WEXPINV();
                        db_MMSMS.cmd.BindByName = true;
                        db_MMSMS.cmd.ExecuteNonQuery();
                        db_MMSMS.Commit();
                    }
                    catch (Exception e)
                    {
                        CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                        callDbtools_oralce.I_ERROR_LOG("DataTransfer14", e.Message, "AUTO");

                        log.Exception_To_Log(string.Format(@"
                        messsage: MI_WEXPINV {0} 
                        StackTrace: {1}
                        ---------------
                        object: {2}
                        ", e.Message, e.StackTrace, JsonConvert.SerializeObject(item, Formatting.Indented)
                           ), "DataTransfer14", "DataTransfer14");

                        db_MMSMS.Rollback();
                    }
                }
                Thread.Sleep(1000);

                #endregion

                #region INSERT 11校正 MI_WEXPINV(以庫存檔存量來校正批號檔存量)
                nowInProgress = "MI_WEXPINV";
                nowInProgressSub = string.Empty;
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // SELECT
                ds_MMSMS = db_MMSMS.GetDataSet(repo.Get_MI_WHINV_EXP());
                dt_MMSMS = ds_MMSMS.Tables[0];
                jsonString = JsonConvert.SerializeObject(dt_MMSMS, Formatting.Indented);
                IEnumerable<MI_WEXPINV> items11_2 = JsonConvert.DeserializeObject<IEnumerable<MI_WEXPINV>>(jsonString);

                // INSERT
                int itemIndex11_2 = 0;
                int items11_2Count = items11_2.Count();
                properties = GetPropertyInfo(nowInProgress);
                foreach (MI_WEXPINV item in items11_2)
                {
                    try
                    {
                        itemIndex11_2++;
                        Console.Write("\r{0} [ {1} / {2} ] {3} ", consoleLog, itemIndex11_2, items11_2Count, ((double)itemIndex11_2 / items11_2Count).ToString("P"));
                        db_MMSMS.cmd.Parameters.Clear();
                        var dictionary = item.AsDictionary();
                        for (var i = 0; i < properties.Length; i++)
                        {
                            db_MMSMS.cmd.Parameters.Add(properties[i].Name, dictionary[properties[i].Name] ?? "");
                        }
                        db_MMSMS.BeginTransaction();
                        db_MMSMS.cmd.CommandText = repo.MERGE_MI_WEXPINV();
                        db_MMSMS.cmd.BindByName = true;
                        db_MMSMS.cmd.ExecuteNonQuery();
                        db_MMSMS.Commit();
                    }
                    catch (Exception e)
                    {
                        CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                        callDbtools_oralce.I_ERROR_LOG("DataTransfer14", e.Message, "AUTO");

                        log.Exception_To_Log(string.Format(@"
                        messsage: MI_WLOCINV
                        StackTrace: {1}
                        ---------------
                        object: {2}
                        ", e.Message, e.StackTrace, JsonConvert.SerializeObject(item, Formatting.Indented)
                           ), "DataTransfer14", "DataTransfer14");

                        db_MMSMS.Rollback();
                    }
                }
                Thread.Sleep(1000);
                #endregion

                #region INSERT 12 MI_WINVMON(庫房庫存月結檔) --撈目前HISDG.XFINVENT/XFCHEMIS &MMSADM.MI_WHMAST
                nowInProgress = "MI_WINVMON";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // SELECT 
                wh_kind = "0";
                ds_HISDG_SubQuery1 = db_HISDG.GetDataSet(repo.Get_MI_WINVMON_HISDG_SubQuery(wh_kind, hosp_table_prefix));
                dt_HISDG_SubQuery1 = ds_HISDG_SubQuery1.Tables[0];
                List<MI_WINVMON> list_WINVMON_HISDG_SubQuery1 = ConvertDataTable<MI_WINVMON>(dt_HISDG_SubQuery1);

                ds_MMSMS_SubQuery1 = db_MMSMS.GetDataSet(repo.Get_MI_WINVMON_MMSMS_SubQuery(wh_kind));
                dt_MMSMS_SubQuery1 = ds_MMSMS_SubQuery1.Tables[0];
                List<MI_WHMAST> list_WINVMON_MMSMS_SubQuery1 = ConvertDataTable<MI_WHMAST>(dt_MMSMS_SubQuery1);

                wh_kind = "1";
                ds_HISDG_SubQuery2 = db_HISDG.GetDataSet(repo.Get_MI_WINVMON_HISDG_SubQuery(wh_kind, hosp_table_prefix));
                dt_HISDG_SubQuery2 = ds_HISDG_SubQuery2.Tables[0];
                List<MI_WINVMON> list_WINVMON_HISDG_SubQuery2 = ConvertDataTable<MI_WINVMON>(dt_HISDG_SubQuery2);

                ds_MMSMS_SubQuery2 = db_MMSMS.GetDataSet(repo.Get_MI_WINVMON_MMSMS_SubQuery(wh_kind));
                dt_MMSMS_SubQuery2 = ds_MMSMS_SubQuery2.Tables[0];
                List<MI_WHMAST> list_WINVMON_MMSMS_SubQuery2 = ConvertDataTable<MI_WHMAST>(dt_MMSMS_SubQuery2);

                List<MI_WINVMON> temp_WINVMON_query1
                    = (from a in list_WINVMON_HISDG_SubQuery1
                       from b in list_WINVMON_MMSMS_SubQuery1
                       where a.STRDEPID == b.INID
                       group new { WH_NO = b.WH_NO, MMCODE = a.STRDRUGID, DATA_YM = a.STRDATE2, INV_QTY = a.LNGNOWREST } by new { b.WH_NO, a.STRDRUGID, a.STRDATE2, a.LNGNOWREST } into g

                       select new MI_WINVMON
                       {
                           WH_NO = g.Key.WH_NO,
                           MMCODE = g.Key.STRDRUGID,
                           DATA_YM = g.Key.STRDATE2,
                           INV_QTY = g.Sum(x => double.Parse(x.INV_QTY)).ToString()
                       }).ToList<MI_WINVMON>();

                List<MI_WINVMON> temp_WINVMON_query2
                    = (from a in list_WINVMON_HISDG_SubQuery2
                       from b in list_WINVMON_MMSMS_SubQuery2
                       where a.STRDEPID == b.INID
                       group new { WH_NO = b.WH_NO, MMCODE = a.STRDRUGID, DATA_YM = a.STRDATE2, INV_QTY = a.LNGNOWREST } by new { b.WH_NO, a.STRDRUGID, a.STRDATE2, a.LNGNOWREST } into g
                       select new MI_WINVMON
                       {
                           WH_NO = g.Key.WH_NO,
                           MMCODE = g.Key.STRDRUGID,
                           DATA_YM = g.Key.STRDATE2,
                           INV_QTY = g.Sum(x => double.Parse(x.INV_QTY)).ToString()
                       }).ToList<MI_WINVMON>();

                IEnumerable<MI_WINVMON> result_WINVMON = temp_WINVMON_query1.Concat(temp_WINVMON_query2).Where(x => string.IsNullOrEmpty(x.WH_NO) == false).Select(x => x).ToList();

                // INSERT [要在 MI_MAST MI_WHMAST 後新增]
                int itemIndex12 = 0;
                int items12Count = result_WINVMON.Count();
                properties = GetPropertyInfo(nowInProgress);
                foreach (MI_WINVMON item in result_WINVMON)
                {
                    try
                    {
                        itemIndex12++;
                        Console.Write("\r{0} [ {1} / {2} ] {3} ", consoleLog, itemIndex12, items12Count, ((double)itemIndex12 / items12Count).ToString("P"));
                        db_MMSMS.cmd.Parameters.Clear();
                        var dictionary = item.AsDictionary();
                        //for (var i = 0; i < properties.Length; i++)
                        //{
                        //    db_MMSMS.cmd.Parameters.Add(properties[i].Name, dictionary[properties[i].Name] ?? "");
                        //}
                        db_MMSMS.cmd.Parameters.Add("WH_NO", item.WH_NO ?? "");
                        db_MMSMS.cmd.Parameters.Add("MMCODE", item.MMCODE ?? "");
                        db_MMSMS.cmd.Parameters.Add("DATA_YM", item.DATA_YM ?? "");
                        db_MMSMS.cmd.Parameters.Add("INV_QTY", item.INV_QTY ?? "");

                        db_MMSMS.BeginTransaction();
                        db_MMSMS.cmd.CommandText = repo.Insert_MI_WINVMON();
                        db_MMSMS.cmd.BindByName = true;
                        db_MMSMS.cmd.ExecuteNonQuery();
                        db_MMSMS.Commit();
                    }
                    catch (Exception e)
                    {
                        CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                        callDbtools_oralce.I_ERROR_LOG("DataTransfer14", e.Message, "AUTO");

                        log.Exception_To_Log(string.Format(@"
                        messsage: MI_WINVMON {0} 
                        StackTrace: {1}
                        ---------------
                        object: {2}
                        ", e.Message, e.StackTrace, JsonConvert.SerializeObject(item, Formatting.Indented)
                           ), "DataTransfer14", "DataTransfer14");

                        db_MMSMS.Rollback();
                    }
                }
                Thread.Sleep(1000);

                #endregion

                #region INSERT 13 MI_WHCOST(庫存成本單價檔) --撈目前HISDG.XFUTCOST/XFINVENT
                nowInProgress = "MI_WHCOST";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // SELECT
                ds_HISDG = db_HISDG.GetDataSet(repo.Get_MI_WHCOST(hosp_table_prefix));
                dt_HISDG = ds_HISDG.Tables[0];
                jsonString = JsonConvert.SerializeObject(dt_HISDG, Formatting.Indented);
                IEnumerable<MI_WHCOST> items13 = JsonConvert.DeserializeObject<IEnumerable<MI_WHCOST>>(jsonString);

                var o = from a in items13
                        group new { MMCODE = a.MMCODE } by new { a.MMCODE } into g
                        select new MI_WHCOST
                        {
                            MMCODE = g.Key.MMCODE,
                            DATA_YM = g.Count().ToString()
                        };
                List<MI_WHCOST> ttt = o.ToList<MI_WHCOST>().ToList<MI_WHCOST>().Where(x => int.Parse(x.DATA_YM) > 1).Select(x => x).ToList();

                // INSERT
                int itemIndex13 = 0;
                int items13Count = items13.Count();
                properties = GetPropertyInfo(nowInProgress);
                foreach (MI_WHCOST item in items13)
                {
                    try
                    {
                        itemIndex13++;
                        Console.Write("\r{0} [ {1} / {2} ] {3} ", consoleLog, itemIndex13, items13Count, ((double)itemIndex13 / items13Count).ToString("P"));
                        db_MMSMS.cmd.Parameters.Clear();
                        var dictionary = item.AsDictionary();
                        for (var i = 0; i < properties.Length; i++)
                        {
                            db_MMSMS.cmd.Parameters.Add(properties[i].Name, dictionary[properties[i].Name] ?? "");
                        }
                        db_MMSMS.BeginTransaction();
                        db_MMSMS.cmd.CommandText = repo.Insert_MI_WHCOST_SubQuery1();
                        db_MMSMS.cmd.BindByName = true;
                        db_MMSMS.cmd.ExecuteNonQuery();
                        db_MMSMS.Commit();
                    }
                    catch (Exception e)
                    {
                        CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                        callDbtools_oralce.I_ERROR_LOG("DataTransfer14", e.Message, "AUTO");

                        log.Exception_To_Log(string.Format(@"
                        messsage: MI_WHCOST {0} 
                        StackTrace: {1}
                        ---------------
                        object: {2}
                        ", e.Message, e.StackTrace, JsonConvert.SerializeObject(item, Formatting.Indented)
                           ), "DataTransfer14", "DataTransfer14");

                        db_MMSMS.Rollback();
                    }
                }
                try
                {
                    db_MMSMS.BeginTransaction();
                    db_MMSMS.cmd.CommandText = repo.Insert_MI_WHCOST_SubQuery2();
                    db_MMSMS.cmd.ExecuteNonQuery();
                    db_MMSMS.Commit();
                }
                catch (Exception e)
                {
                    CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                    callDbtools_oralce.I_ERROR_LOG("DataTransfer14", e.Message, "AUTO");

                    log.Exception_To_Log(string.Format(@"
                        messsage: MI_WHCOST {0} 
                        StackTrace: {1}
                        ---------------
                        object: {2}
                        ", e.Message, e.StackTrace, ""
                       //JsonConvert.SerializeObject(rx)
                       ), "DataTransfer14", "DataTransfer14");

                    db_MMSMS.Rollback();
                    //db_MMSMS.Dispose();

                    //db_HISDG.Rollback();
                    //db_HISDG.Dispose();
                    //Console.ReadLine();
                }
                Thread.Sleep(1000);
                #endregion

                #region INSERT 14 MI_MAST_HISTORY(藥衛材基本檔歷史資料) --撈目前MMSADM.MI_MAST複製一份
                nowInProgress = "MI_MAST_HISTORY";
                nowInProgressSub = string.Empty;
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // INSERT
                try
                {
                    db_MMSMS.BeginTransaction();
                    db_MMSMS.cmd.CommandText = repo.Insert_MI_MAST_HISTORY();
                    db_MMSMS.cmd.ExecuteNonQuery();
                    db_MMSMS.Commit();
                }
                catch (Exception e)
                {
                    CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                    callDbtools_oralce.I_ERROR_LOG("DataTransfer14", e.Message, "AUTO");

                    log.Exception_To_Log(string.Format(@"
                        messsage: MI_MAST_HISTORY {0}
                        StackTrace: {1}
                        ---------------
                        object: {2}
                        ", e.Message, e.StackTrace, db_MMSMS.cmd.CommandText
                       ), "DataTransfer14", "DataTransfer14");

                    db_MMSMS.Rollback();
                }
                Thread.Sleep(1000);
                #endregion

                #region INSERT 15 SEC_MAST(科別代碼主檔) --撈目前HISDG.XFMSDEPT
                nowInProgress = "SEC_MAST";
                nowInProgressSub = string.Empty;
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // SELECT
                ds_HISDG = db_HISDG.GetDataSet(repo.Get_SEC_MAST(hosp_table_prefix));
                dt_HISDG = ds_HISDG.Tables[0];
                jsonString = JsonConvert.SerializeObject(dt_HISDG, Formatting.Indented);
                IEnumerable<SEC_MAST> items15 = JsonConvert.DeserializeObject<IEnumerable<SEC_MAST>>(jsonString);

                // INSERT
                int itemIndex15 = 0;
                int items15Count = items15.Count();
                properties = GetPropertyInfo(nowInProgress);
                foreach (SEC_MAST item in items15)
                {
                    try
                    {
                        itemIndex15++;
                        Console.Write("\r{0} [ {1} / {2} ] {3} ", consoleLog, itemIndex15, items15Count, ((double)itemIndex15 / items15Count).ToString("P"));
                        db_MMSMS.cmd.Parameters.Clear();
                        var dictionary = item.AsDictionary();
                        for (var i = 0; i < properties.Length; i++)
                        {
                            db_MMSMS.cmd.Parameters.Add(properties[i].Name, dictionary[properties[i].Name] ?? "");
                        }
                        db_MMSMS.BeginTransaction();
                        db_MMSMS.cmd.CommandText = repo.Insert_SEC_MAST();
                        db_MMSMS.cmd.BindByName = true;
                        db_MMSMS.cmd.ExecuteNonQuery();
                        db_MMSMS.Commit();
                    }
                    catch (Exception e)
                    {
                        CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                        callDbtools_oralce.I_ERROR_LOG("DataTransfer14", e.Message, "AUTO");

                        log.Exception_To_Log(string.Format(@"
                        messsage: SEC_MAST
                        StackTrace: {1}
                        ---------------
                        object: {2}
                        ", e.Message, e.StackTrace, JsonConvert.SerializeObject(item, Formatting.Indented)
                           ), "DataTransfer14", "DataTransfer14");

                        db_MMSMS.Rollback();
                    }
                }
                Thread.Sleep(1000);
                #endregion

                #region INSERT 16 SEC_USEMM(科別耗用衛材檔) --撈目前HISDG.XFEMOPSET
                nowInProgress = "SEC_USEMM";
                nowInProgressSub = string.Empty;
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // SELECT
                ds_HISDG = db_HISDG.GetDataSet(repo.Get_SEC_USEMM(hosp_table_prefix));
                dt_HISDG = ds_HISDG.Tables[0];
                jsonString = JsonConvert.SerializeObject(dt_HISDG, Formatting.Indented);
                IEnumerable<SEC_USEMM> items16 = JsonConvert.DeserializeObject<IEnumerable<SEC_USEMM>>(jsonString);
                // INSERT
                properties = GetPropertyInfo(nowInProgress);
                switch (hosp_id)
                {
                    //只有803才要轉檔，其它醫院不轉這個檔案
                    case "803":
                        int itemIndex16 = 0;
                        int items16Count = items16.Count();
                        foreach (SEC_USEMM item in items16)
                        {
                            try
                            {
                                itemIndex16++;
                                Console.Write("\r{0} [ {1} / {2} ] {3} ", consoleLog, itemIndex16, items16Count, ((double)itemIndex16 / items16Count).ToString("P"));
                                db_MMSMS.cmd.Parameters.Clear();
                                var dictionary = item.AsDictionary();
                                for (var i = 0; i < properties.Length; i++)
                                {
                                    db_MMSMS.cmd.Parameters.Add(properties[i].Name, dictionary[properties[i].Name] ?? "");
                                }
                                db_MMSMS.BeginTransaction();
                                db_MMSMS.cmd.CommandText = repo.Insert_SEC_USEMM();
                                db_MMSMS.cmd.BindByName = true;
                                db_MMSMS.cmd.ExecuteNonQuery();
                                db_MMSMS.Commit();
                            }
                            catch (Exception e)
                            {
                                CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                                callDbtools_oralce.I_ERROR_LOG("DataTransfer14", e.Message, "AUTO");

                                log.Exception_To_Log(string.Format(@"
                                messsage: SEC_USEMM
                                StackTrace: {1}
                                ---------------
                                object: {2}
                                ", e.Message, e.StackTrace, JsonConvert.SerializeObject(item, Formatting.Indented)
                                   ), "DataTransfer14", "DataTransfer14");

                                db_MMSMS.Rollback();
                            }
                        }
                        break;
                    default:
                        break;
                }
                Thread.Sleep(1000);
                #endregion

                #region INSERT 18 RCRATE(合約優惠比率設定檔) 818北投不做 --撈目前HISDG.XFRCRATE
                if (hosp_id != "818") //818北投只有4筆，黃小姐已人工輸入，不必轉檔
                {
                    nowInProgress = "RCRATE";
                    nowInProgressSub = string.Empty;
                    consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                    Console.Write("\n" + consoleLog);
                    // SELECT
                    ds_HISDG = db_HISDG.GetDataSet(repo.Get_RCRATE(hosp_table_prefix));
                    dt_HISDG = ds_HISDG.Tables[0];
                    jsonString = JsonConvert.SerializeObject(dt_HISDG, Formatting.Indented);
                    IEnumerable<RCRATE> items18 = JsonConvert.DeserializeObject<IEnumerable<RCRATE>>(jsonString);

                    // INSERT
                    int itemIndex18 = 0;
                    int items18Count = items18.Count();
                    properties = GetPropertyInfo(nowInProgress);
                    foreach (RCRATE item in items18)
                    {
                        try
                        {
                            itemIndex18++;
                            Console.Write("\r{0} [ {1} / {2} ] {3} ", consoleLog, itemIndex18, items18Count, ((double)itemIndex18 / items18Count).ToString("P"));
                            db_MMSMS.cmd.Parameters.Clear();
                            var dictionary = item.AsDictionary();
                            for (var i = 0; i < properties.Length; i++)
                            {
                                db_MMSMS.cmd.Parameters.Add(properties[i].Name, dictionary[properties[i].Name] ?? "");
                            }
                            db_MMSMS.BeginTransaction();
                            db_MMSMS.cmd.CommandText = repo.Insert_RCRATE();
                            db_MMSMS.cmd.BindByName = true;
                            db_MMSMS.cmd.ExecuteNonQuery();
                            db_MMSMS.Commit();
                        }
                        catch (Exception e)
                        {
                            CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                            callDbtools_oralce.I_ERROR_LOG("DataTransfer14", e.Message, "AUTO");

                            log.Exception_To_Log(string.Format(@"
                        messsage: RCRATE
                        StackTrace: {1}
                        ---------------
                        object: {2}
                        ", e.Message, e.StackTrace, JsonConvert.SerializeObject(item, Formatting.Indented)
                               ), "DataTransfer14", "DataTransfer14");

                            db_MMSMS.Rollback();
                        }
                    }
                    Thread.Sleep(1000);
                }
                #endregion

                #region INSERT 19 SEC_CALLOC(科室成本分攤比率表) --撈目前HISDG.XFDTRATE
                nowInProgress = "SEC_CALLOC";
                nowInProgressSub = string.Empty;
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // SELECT
                ds_HISDG = db_HISDG.GetDataSet(repo.Get_SEC_CALLOC(hosp_table_prefix));
                dt_HISDG = ds_HISDG.Tables[0];
                jsonString = JsonConvert.SerializeObject(dt_HISDG, Formatting.Indented);
                IEnumerable<SEC_CALLOC> items19 = JsonConvert.DeserializeObject<IEnumerable<SEC_CALLOC>>(jsonString);

                // INSERT
                int itemIndex19 = 0;
                int items19Count = items19.Count();
                properties = GetPropertyInfo(nowInProgress);
                foreach (SEC_CALLOC item in items19)
                {
                    try
                    {
                        itemIndex19++;
                        Console.Write("\r{0} [ {1} / {2} ] {3} ", consoleLog, itemIndex19, items19Count, ((double)itemIndex19 / items19Count).ToString("P"));
                        db_MMSMS.cmd.Parameters.Clear();
                        var dictionary = item.AsDictionary();
                        for (var i = 0; i < properties.Length; i++)
                        {
                            db_MMSMS.cmd.Parameters.Add(properties[i].Name, dictionary[properties[i].Name] ?? "");
                        }
                        db_MMSMS.BeginTransaction();
                        db_MMSMS.cmd.CommandText = repo.Insert_SEC_CALLOC();
                        db_MMSMS.cmd.BindByName = true;
                        db_MMSMS.cmd.ExecuteNonQuery();
                        db_MMSMS.Commit();
                    }
                    catch (Exception e)
                    {
                        CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                        callDbtools_oralce.I_ERROR_LOG("DataTransfer14", e.Message, "AUTO");

                        log.Exception_To_Log(string.Format(@"
                        messsage: SEC_CALLOC
                        StackTrace: {1}
                        ---------------
                        object: {2}
                        ", e.Message, e.StackTrace, JsonConvert.SerializeObject(item, Formatting.Indented)
                           ), "DataTransfer14", "DataTransfer14");

                        db_MMSMS.Rollback();
                    }
                }
                Thread.Sleep(1000);
                #endregion

                # region INSERT 20 MI_WHID(庫房使用人員檔) --撈目前MMSADM.MI_WHID_INIT
                nowInProgress = "MI_WHID";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Restore_From_MI_WHID_INIT();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion

                #region INSERT 21 PH_BANK_AF(國軍銀行代碼檔)  --撈目前HISDG.XFBANKLS
                nowInProgress = "PH_BANK_AF";
                nowInProgressSub = string.Empty;
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // SELECT
                ds_HISDG = db_HISDG.GetDataSet(repo.Get_PH_BANK_AF(hosp_table_prefix));
                dt_HISDG = ds_HISDG.Tables[0];
                jsonString = JsonConvert.SerializeObject(dt_HISDG, Formatting.Indented);
                IEnumerable<PH_BANK_AF> items21 = JsonConvert.DeserializeObject<IEnumerable<PH_BANK_AF>>(jsonString);

                // INSERT
                int itemIndex21 = 0;
                int items21Count = items21.Count();
                properties = GetPropertyInfo(nowInProgress);
                foreach (PH_BANK_AF item in items21)
                {
                    try
                    {
                        itemIndex21++;
                        Console.Write("\r{0} [ {1} / {2} ] {3} ", consoleLog, itemIndex21, items21Count, ((double)itemIndex21 / items21Count).ToString("P"));
                        db_MMSMS.cmd.Parameters.Clear();
                        var dictionary = item.AsDictionary();
                        for (var i = 0; i < properties.Length; i++)
                        {
                            db_MMSMS.cmd.Parameters.Add(properties[i].Name, dictionary[properties[i].Name] ?? "");
                        }
                        db_MMSMS.BeginTransaction();
                        db_MMSMS.cmd.CommandText = repo.Insert_PH_BANK_AF();
                        db_MMSMS.cmd.BindByName = true;
                        db_MMSMS.cmd.ExecuteNonQuery();
                        db_MMSMS.Commit();
                    }
                    catch (Exception e)
                    {
                        CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                        callDbtools_oralce.I_ERROR_LOG("DataTransfer14", e.Message, "AUTO");

                        log.Exception_To_Log(string.Format(@"
                    messsage: PH_BANK_AF
                    StackTrace: {1}
                    ---------------
                    object: {2}
                    ", e.Message, e.StackTrace, JsonConvert.SerializeObject(item, Formatting.Indented)
                            ), "DataTransfer14", "DataTransfer14");

                        db_MMSMS.Rollback();
                    }
                }
                Thread.Sleep(1000);
                #endregion

                nowSection = "更新";

                #region 1130320註解UPDATE UR_ID部分
                /* #region UPDATE UR_ID
                nowInProgress = "UR_ID";
                nowInProgressSub = string.Empty;
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // update
                try
                {
                    db_MMSMS.BeginTransaction();
                    db_MMSMS.cmd.CommandText = repo.Update_UR_ID();
                    db_MMSMS.cmd.ExecuteNonQuery();
                    db_MMSMS.Commit();
                }
                catch (Exception e)
                {
                    CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                    callDbtools_oralce.I_ERROR_LOG("DataTransfer14", e.Message, "AUTO");

                    log.Exception_To_Log(string.Format(@"
                        messsage: UR_ID {0}
                        StackTrace: {1}
                        ---------------
                        object: {2}
                        ", e.Message, e.StackTrace, db_MMSMS.cmd.CommandText
                       ), "DataTransfer14", "DataTransfer14");

                    db_MMSMS.Rollback();
                }
                Thread.Sleep(1000);
                #endregion */
                #endregion

                #region UPDATE MI_MAST
                nowInProgress = "MI_MAST";
                nowInProgressSub = string.Empty;
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                // update
                try
                {
                    db_MMSMS.BeginTransaction();
                    db_MMSMS.cmd.CommandText = repo.Update_MI_MAST();
                    db_MMSMS.cmd.ExecuteNonQuery();
                    db_MMSMS.Commit();
                }
                catch (Exception e)
                {
                    CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                    callDbtools_oralce.I_ERROR_LOG("DataTransfer14", e.Message, "AUTO");

                    log.Exception_To_Log(string.Format(@"
                        messsage: MI_MAST {0}
                        StackTrace: {1}
                        ---------------
                        object: {2}
                        ", e.Message, e.StackTrace, db_MMSMS.cmd.CommandText
                       ), "DataTransfer14", "DataTransfer14");

                    db_MMSMS.Rollback();
                }
                Thread.Sleep(1000);
                #endregion

                db_MMSMS.Dispose();
                db_HISDG.Dispose();
            }
            catch (Exception e)
            {
                CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                callDbtools_oralce.I_ERROR_LOG("DataTransfer14", e.Message, "AUTO");

                log.Exception_To_Log(string.Format(@"
                        messsage: {0}
                        StackTrace: {1}
                        ---------------
                        object: {2}
                        ", e.Message, e.StackTrace, string.Empty
                   //JsonConvert.SerializeObject(rx)
                   ), "DataTransfer14", "DataTransfer14");

                //db_MMSMS.Rollback();
                db_MMSMS.Dispose();

                //db_HISDG.Rollback();
                db_HISDG.Dispose();
            }
        }

        public void RunTransfer_MI_BASERO_14(string hosp_id, string hosp_table_prefix, string START_XF_TIME, string END_XF_TIME)
        {
            try
            {
                #region 宣告與初始化
                db_MMSMS = new OracleDB("MMSMS");
                db_HISDG = new OracleDB("HISDG");

                DataSet ds_HISDG = new DataSet();
                DataTable dt_HISDG = new DataTable();

                DataTransferBasero14Repository repo = new DataTransferBasero14Repository();

                string jsonString = "";
                PropertyInfo[] properties;
                DataTable dtResult = new DataTable();
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
                    consoleLog = "進行 " + hospName + " MI_BASERO_14 轉檔程序";
                    Console.WriteLine("正在進行 " + hospName + " MI_BASERO_14 轉檔程序，請稍後...");
                }
                #endregion


                /*  -------------------將HISDG資料copy到正式機-------------------  */
                //刪除現有XFBASERO、XFCHEMIS、XFDAYRO、XFOPTION資料

                nowSection = "刪除";
                #region DELETE XFBASERO
                nowInProgress = "XFBASERO";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Delete_XFBASERO(START_XF_TIME, END_XF_TIME);
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion
                #region DELETE XFCHEMIS
                nowInProgress = "XFCHEMIS";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Delete_XFCHEMIS();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion
                #region DELETE XFDAYRO
                nowInProgress = "XFDAYRO";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Delete_XFDAYRO(START_XF_TIME, END_XF_TIME);
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion
                #region DELETE XFOPTION
                nowInProgress = "XFOPTION";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Delete_XFOPTION();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion
                #region DELETE MI_BASERO_14
                nowInProgress = "MI_BASERO_14";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Delete_MI_BASERO_14();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion 

                //搬移XFBASERO、XFCHEMIS、XFDAYRO、XFOPTION資料到正式機
                nowSection = "搬移";
                #region INSERT INTO Table XFBASERO
                nowInProgress = "XFBASERO";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);

                // SELECT
                ds_HISDG = db_HISDG.GetDataSet(repo.Get_XFBASERO(hosp_table_prefix, START_XF_TIME, END_XF_TIME));
                dt_HISDG = ds_HISDG.Tables[0];
                jsonString = JsonConvert.SerializeObject(dt_HISDG, Formatting.Indented);
                IEnumerable<XFBASERO> items = JsonConvert.DeserializeObject<IEnumerable<XFBASERO>>(jsonString);
                // INSERT
                properties = GetPropertyInfo(nowInProgress);

                int itemIndex = 0;
                int itemsCount = items.Count();
                foreach (XFBASERO item in items)
                {
                    try
                    {
                        itemIndex++;
                        Console.Write("\r{0} [ {1} / {2} ] {3} ", consoleLog, itemIndex, itemsCount, ((double)itemIndex / itemsCount).ToString("P"));
                        db_MMSMS.cmd.Parameters.Clear();
                        var dictionary = item.AsDictionary();
                        for (var i = 0; i < properties.Length; i++)
                        {
                            db_MMSMS.cmd.Parameters.Add(properties[i].Name, dictionary[properties[i].Name] ?? "");
                        }
                        db_MMSMS.BeginTransaction();
                        db_MMSMS.cmd.CommandText = repo.Insert_XFBASERO();
                        db_MMSMS.cmd.BindByName = true;
                        db_MMSMS.cmd.ExecuteNonQuery();
                        db_MMSMS.Commit();
                    }
                    catch (Exception e)
                    {
                        CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                        callDbtools_oralce.I_ERROR_LOG("DataTransfer14", e.Message, "AUTO");

                        log.Exception_To_Log(string.Format(@"
                                messsage: XFBASERO
                                StackTrace: {1}
                                ---------------
                                object: {2}
                                ", e.Message, e.StackTrace, JsonConvert.SerializeObject(item, Formatting.Indented)
                           //JsonConvert.SerializeObject(rx)
                           ), "DataTransfer14", "DataTransfer14");

                        db_MMSMS.Rollback();
                    }
                }

                Thread.Sleep(1000);
                #endregion
                #region INSERT INTO Table XFCHEMIS
                nowInProgress = "XFCHEMIS";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);

                // SELECT
                ds_HISDG = db_HISDG.GetDataSet(repo.Get_XFCHEMIS(hosp_table_prefix));
                dt_HISDG = ds_HISDG.Tables[0];
                jsonString = JsonConvert.SerializeObject(dt_HISDG, Formatting.Indented);
                IEnumerable<XFCHEMIS> items2 = JsonConvert.DeserializeObject<IEnumerable<XFCHEMIS>>(jsonString);
                // INSERT
                properties = GetPropertyInfo(nowInProgress);

                int itemIndex2 = 0;
                int itemsCount2 = items2.Count();
                foreach (XFCHEMIS item2 in items2)
                {
                    try
                    {
                        itemIndex2++;
                        Console.Write("\r{0} [ {1} / {2} ] {3} ", consoleLog, itemIndex2, itemsCount2, ((double)itemIndex2 / itemsCount2).ToString("P"));
                        db_MMSMS.cmd.Parameters.Clear();
                        var dictionary = item2.AsDictionary();
                        for (var i = 0; i < properties.Length; i++)
                        {
                            db_MMSMS.cmd.Parameters.Add(properties[i].Name, dictionary[properties[i].Name] ?? "");
                        }
                        db_MMSMS.BeginTransaction();
                        db_MMSMS.cmd.CommandText = repo.Insert_XFCHEMIS();
                        db_MMSMS.cmd.BindByName = true;
                        db_MMSMS.cmd.ExecuteNonQuery();
                        db_MMSMS.Commit();
                    }
                    catch (Exception e)
                    {
                        CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                        callDbtools_oralce.I_ERROR_LOG("DataTransfer14", e.Message, "AUTO");

                        log.Exception_To_Log(string.Format(@"
                                messsage: XFCHEMIS
                                StackTrace: {1}
                                ---------------
                                object: {2}
                                ", e.Message, e.StackTrace, JsonConvert.SerializeObject(item2, Formatting.Indented)
                           //JsonConvert.SerializeObject(rx)
                           ), "DataTransfer14", "DataTransfer14");

                        db_MMSMS.Rollback();
                    }
                }

                Thread.Sleep(1000);
                #endregion
                #region INSERT INTO Table XFDAYRO
                nowInProgress = "XFDAYRO";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);

                // SELECT
                ds_HISDG = db_HISDG.GetDataSet(repo.Get_XFDAYRO(hosp_table_prefix, START_XF_TIME, END_XF_TIME));
                dt_HISDG = ds_HISDG.Tables[0];
                jsonString = JsonConvert.SerializeObject(dt_HISDG, Formatting.Indented);
                IEnumerable<XFDAYRO> items3 = JsonConvert.DeserializeObject<IEnumerable<XFDAYRO>>(jsonString);
                // INSERT
                properties = GetPropertyInfo(nowInProgress);

                int itemIndex3 = 0;
                int itemsCount3 = items3.Count();
                foreach (XFDAYRO item3 in items3)
                {
                    try
                    {
                        itemIndex3++;
                        Console.Write("\r{0} [ {1} / {2} ] {3} ", consoleLog, itemIndex3, itemsCount3, ((double)itemIndex3 / itemsCount3).ToString("P"));
                        db_MMSMS.cmd.Parameters.Clear();
                        var dictionary = item3.AsDictionary();
                        for (var i = 0; i < properties.Length; i++)
                        {
                            db_MMSMS.cmd.Parameters.Add(properties[i].Name, dictionary[properties[i].Name] ?? "");
                        }
                        db_MMSMS.BeginTransaction();
                        db_MMSMS.cmd.CommandText = repo.Insert_XFDAYRO();
                        db_MMSMS.cmd.BindByName = true;
                        db_MMSMS.cmd.ExecuteNonQuery();
                        db_MMSMS.Commit();
                    }
                    catch (Exception e)
                    {
                        CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                        callDbtools_oralce.I_ERROR_LOG("DataTransfer14", e.Message, "AUTO");

                        log.Exception_To_Log(string.Format(@"
                                messsage: XFDAYRO
                                StackTrace: {1}
                                ---------------
                                object: {2}
                                ", e.Message, e.StackTrace, JsonConvert.SerializeObject(item3, Formatting.Indented)
                           //JsonConvert.SerializeObject(rx)
                           ), "DataTransfer14", "DataTransfer14");

                        db_MMSMS.Rollback();
                    }
                }

                Thread.Sleep(1000);
                #endregion
                #region INSERT INTO Table XFOPTION
                nowInProgress = "XFOPTION";
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);

                // SELECT
                ds_HISDG = db_HISDG.GetDataSet(repo.Get_XFOPTION(hosp_table_prefix));
                dt_HISDG = ds_HISDG.Tables[0];
                jsonString = JsonConvert.SerializeObject(dt_HISDG, Formatting.Indented);
                IEnumerable<XFOPTION> items4 = JsonConvert.DeserializeObject<IEnumerable<XFOPTION>>(jsonString);
                // INSERT
                properties = GetPropertyInfo(nowInProgress);

                int itemIndex4 = 0;
                int itemsCount4 = items4.Count();
                foreach (XFOPTION item4 in items4)
                {
                    try
                    {
                        itemIndex4++;
                        Console.Write("\r{0} [ {1} / {2} ] {3} ", consoleLog, itemIndex4, itemsCount4, ((double)itemIndex4 / itemsCount4).ToString("P"));
                        db_MMSMS.cmd.Parameters.Clear();
                        var dictionary = item4.AsDictionary();
                        for (var i = 0; i < properties.Length; i++)
                        {
                            db_MMSMS.cmd.Parameters.Add(properties[i].Name, dictionary[properties[i].Name] ?? "");
                        }
                        db_MMSMS.BeginTransaction();
                        db_MMSMS.cmd.CommandText = repo.Insert_XFOPTION();
                        db_MMSMS.cmd.BindByName = true;
                        db_MMSMS.cmd.ExecuteNonQuery();
                        db_MMSMS.Commit();
                    }
                    catch (Exception e)
                    {
                        CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                        callDbtools_oralce.I_ERROR_LOG("DataTransfer14", e.Message, "AUTO");

                        log.Exception_To_Log(string.Format(@"
                                messsage: XFOPTION
                                StackTrace: {1}
                                ---------------
                                object: {2}
                                ", e.Message, e.StackTrace, JsonConvert.SerializeObject(item4, Formatting.Indented)
                           //JsonConvert.SerializeObject(rx)
                           ), "DataTransfer14", "DataTransfer14");

                        db_MMSMS.Rollback();
                    }
                }

                Thread.Sleep(1000);
                #endregion


                /*  -------------------產生MI_BASERO_14的資料-------------------  */
                //寫入資料到MI_BASERO_14 (參考17 GEN_BASERO文件)
                nowSection = "轉檔";
                #region INSERT 17 GEN_BASERO
                nowInProgress = "MI_BASERO_14";
                nowInProgressSub = string.Empty;
                consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                Console.Write("\n" + consoleLog);

                // SELECT 
                DataSet ds_MMSMS_CSR1 = new DataSet();
                DataTable dt_MMSMS_CSR1 = new DataTable();
                DataSet ds_MMSMS_CSR2 = new DataSet();
                DataTable dt_MMSMS_CSR2 = new DataTable();
                DataSet ds_MMSMS_CSR3 = new DataSet();
                DataTable dt_MMSMS_CSR3 = new DataTable();
                DataSet ds_MMSMS_CSR4 = new DataSet();
                DataTable dt_MMSMS_CSR4 = new DataTable();

                //CSR1
                ds_MMSMS_CSR1 = db_MMSMS.GetDataSet(repo.GetMergeTable_CSR1(START_XF_TIME, END_XF_TIME));
                dt_MMSMS_CSR1 = ds_MMSMS_CSR1.Tables[0];
                List<XFBASERO_MIS> listCSR1 = ConvertDataTable<XFBASERO_MIS>(dt_MMSMS_CSR1);
                //CSR4
                ds_MMSMS_CSR4 = db_MMSMS.GetDataSet(repo.GetOPTION_CSR4());
                dt_MMSMS_CSR4 = ds_MMSMS_CSR4.Tables[0];
                List<XFOPTION> listCSR4 = ConvertDataTable<XFOPTION>(dt_MMSMS_CSR4);

                int itemIndex5 = 0;
                int itemsCount5 = listCSR1.Count();
                foreach (XFBASERO_MIS item in listCSR1)
                {
                    try
                    {
                        itemIndex5++;
                        Console.Write("\r{0} [ {1} / {2} ] {3} ", consoleLog, itemIndex5, itemsCount5, ((double)itemIndex5 / itemsCount5).ToString("P"));

                        #region 宣告與初始化
                        var V_INID = item.INID;
                        var V_KIND = item.WKIND;
                        var V_WHTYPE = "";
                        var V_WHNO = "";
                        var drugId = item.MMCODE;
                        decimal V_G34_MAXQTY = 0;
                        decimal V_SUPPLY_MAXQTY = 0;
                        decimal V_PHR_MAXQTY = 0;
                        decimal V_WAST10D = 0;
                        decimal V_WAST14D = 0;
                        decimal V_WAST90D = 0;
                        decimal V_LNGDIFF = 0;
                        decimal V_INTSETRORATE1 = 0;
                        decimal V_INTSETRORATE2 = 0;
                        decimal V_INTSETRORATE3 = 0;
                        decimal V_INTSETRORATE4 = 0;
                        decimal V_INTSETRORATE5 = 0;
                        decimal V_INTSAFERORATE1 = 0;
                        decimal V_INTNORMALRORATE1 = 0;
                        decimal V_INTEARRORATE1 = 0;
                        decimal V_INTSAFERORATE2 = 0;
                        decimal V_INTNORMALRORATE2 = 0;
                        decimal V_INTSAFERORATE3 = 0;
                        decimal V_INTNORMALRORATE3 = 0;
                        #endregion

                        //CSR2
                        ds_MMSMS_CSR2 = db_MMSMS.GetDataSet(repo.GetWH_CSR2(V_INID, V_KIND));
                        dt_MMSMS_CSR2 = ds_MMSMS_CSR2.Tables[0];
                        List<MI_WHMAST> listCSR2 = ConvertDataTable<MI_WHMAST>(dt_MMSMS_CSR2);
                        //依照WHTYPE設定MAXQTY
                        if (listCSR2.Count != 0)
                        {
                            V_WHTYPE = listCSR2[0].RO_WHTYPE;
                            V_WHNO = listCSR2[0].WH_NO;
                            if (V_WHTYPE == "1")
                            {
                                V_G34_MAXQTY = Convert.ToDecimal(item.LNGSETRO1);
                            }
                            else if (V_WHTYPE == "3" && item.STRXFKINDID != "1")
                            {
                                V_G34_MAXQTY = Convert.ToDecimal(item.LNGSETRO2);
                            }
                            else if (V_WHTYPE == "2" && item.STRXFKINDID == "1")
                            {
                                V_G34_MAXQTY = Convert.ToDecimal(item.LNGSETRO3);
                            }
                            else
                            {
                                V_G34_MAXQTY = 0;
                            }

                            if (V_WHTYPE == "1" && item.STRXFKINDID != "1")
                            {
                                V_SUPPLY_MAXQTY = Convert.ToDecimal(item.LNGSETRO4);
                            }
                            else
                            {
                                V_SUPPLY_MAXQTY = 0;
                            }

                            if (V_WHTYPE == "1" && item.STRXFKINDID == "1")
                            {
                                V_PHR_MAXQTY = Convert.ToDecimal(item.LNGSETRO5);
                            }
                            else
                            {
                                V_PHR_MAXQTY = 0;
                            }
                        }

                        //CSR3
                        ds_MMSMS_CSR3 = db_MMSMS.GetDataSet(repo.GetLNGWAST_CSR3(drugId, V_INID));
                        dt_MMSMS_CSR3 = ds_MMSMS_CSR3.Tables[0];
                        List<XFDAYRO> listCSR3 = ConvertDataTable<XFDAYRO>(dt_MMSMS_CSR3);
                        if (listCSR3.Count != 0)
                        {
                            V_WAST10D = Convert.ToDecimal(listCSR3[0].LNGWAST10D);
                            V_WAST14D = Convert.ToDecimal(listCSR3[0].LNGWAST14D);
                            V_WAST90D = Convert.ToDecimal(listCSR3[0].LNGWAST90D);
                            V_LNGDIFF = Convert.ToDecimal(listCSR3[0].LNGDIFF);
                        }
                        if (listCSR4.Count != 0)
                        {
                            V_INTSETRORATE1 = Convert.ToDecimal(listCSR4[0].INTSETRORATE1);
                            V_INTSETRORATE2 = Convert.ToDecimal(listCSR4[0].INTSETRORATE2);
                            V_INTSETRORATE3 = Convert.ToDecimal(listCSR4[0].INTSETRORATE3);
                            V_INTSETRORATE4 = Convert.ToDecimal(listCSR4[0].INTSETRORATE4);
                            V_INTSETRORATE5 = Convert.ToDecimal(listCSR4[0].INTSETRORATE5);
                            V_INTSAFERORATE1 = Convert.ToDecimal(listCSR4[0].INTSAFERORATE1);
                            V_INTNORMALRORATE1 = Convert.ToDecimal(listCSR4[0].INTNORMALRORATE1);
                            V_INTEARRORATE1 = Convert.ToDecimal(listCSR4[0].INTEARRORATE1);
                            V_INTSAFERORATE2 = Convert.ToDecimal(listCSR4[0].INTSAFERORATE2);
                            V_INTNORMALRORATE2 = Convert.ToDecimal(listCSR4[0].INTNORMALRORATE2);
                            V_INTSAFERORATE3 = Convert.ToDecimal(listCSR4[0].INTSAFERORATE3);
                            V_INTNORMALRORATE3 = Convert.ToDecimal(listCSR4[0].INTNORMALRORATE3);
                        }

                        //開始寫入BASERO14
                        MI_BASERO_14 newBASERO14;
                        if (V_WHTYPE == "1")
                        {
                            newBASERO14 = new MI_BASERO_14()
                            {
                                MMCODE = item.MMCODE,
                                RO_WHTYPE = V_WHTYPE,
                                RO_TYPE = item.RO_TYPE,
                                NOW_RO = Convert.ToDecimal(item.NOW_RO),
                                DAY_USE_10 = V_WAST10D,
                                DAY_USE_14 = V_WAST14D,
                                DAY_USE_90 = V_WAST90D,
                                MON_USE_1 = Convert.ToDecimal(item.MON_USE_1),
                                MON_USE_2 = Convert.ToDecimal(item.MON_USE_2),
                                MON_USE_3 = Convert.ToDecimal(item.MON_USE_3),
                                MON_USE_4 = Convert.ToDecimal(item.MON_USE_4),
                                MON_USE_5 = Convert.ToDecimal(item.MON_USE_5),
                                MON_USE_6 = Convert.ToDecimal(item.MON_USE_6),
                                MON_AVG_USE_3 = Convert.ToDecimal(item.MON_AVG_USE_3),
                                MON_AVG_USE_6 = Convert.ToDecimal(item.MON_AVG_USE_6),
                                G34_MAX_APPQTY = V_G34_MAXQTY,
                                SUPPLY_MAX_APPQTY = V_SUPPLY_MAXQTY,
                                PHR_MAX_APPQTY = V_PHR_MAXQTY,
                                WAR_QTY = Convert.ToDecimal(item.WAR_QTY),
                                SAFE_QTY = V_INTSAFERORATE1 * Convert.ToDecimal(item.NOW_RO),
                                NORMAL_QTY = V_INTNORMALRORATE1 * Convert.ToDecimal(item.NOW_RO),
                                DIFF_PERC = V_LNGDIFF,
                                SAFE_PERC = V_INTSAFERORATE1,
                                DAY_RO = item.RO_TYPE == "1" ? Convert.ToDecimal(item.NOW_RO) : 0,
                                MON_RO = item.RO_TYPE == "2" ? Convert.ToDecimal(item.NOW_RO) : 0,
                                G34_PERC = V_INTSETRORATE1,
                                SUPPLY_PERC = V_INTSETRORATE4,
                                PHR_PERC = V_INTSETRORATE5,
                                NORMAL_PERC = V_INTNORMALRORATE1,
                                WAR_PERC = V_INTEARRORATE1,
                                WH_NO = V_WHNO,
                                CREATE_TIME = item.STRLASTUPDATE,
                                CREATE_USER = item.STROPERATERID,
                                UPDATE_TIME = item.STRLASTUPDATE,
                                UPDATE_USER = item.STROPERATERID
                            };
                        }
                        else if (V_WHTYPE == "2")
                        {
                            newBASERO14 = newBASERO14 = new MI_BASERO_14()
                            {
                                MMCODE = item.MMCODE,
                                RO_WHTYPE = V_WHTYPE,
                                RO_TYPE = item.RO_TYPE,
                                NOW_RO = Convert.ToDecimal(item.NOW_RO),
                                DAY_USE_10 = V_WAST10D,
                                DAY_USE_14 = V_WAST14D,
                                DAY_USE_90 = V_WAST90D,
                                MON_USE_1 = Convert.ToDecimal(item.MON_USE_1),
                                MON_USE_2 = Convert.ToDecimal(item.MON_USE_2),
                                MON_USE_3 = Convert.ToDecimal(item.MON_USE_3),
                                MON_USE_4 = Convert.ToDecimal(item.MON_USE_4),
                                MON_USE_5 = Convert.ToDecimal(item.MON_USE_5),
                                MON_USE_6 = Convert.ToDecimal(item.MON_USE_6),
                                MON_AVG_USE_3 = Convert.ToDecimal(item.MON_AVG_USE_3),
                                MON_AVG_USE_6 = Convert.ToDecimal(item.MON_AVG_USE_6),
                                G34_MAX_APPQTY = V_G34_MAXQTY,
                                SUPPLY_MAX_APPQTY = V_SUPPLY_MAXQTY,
                                PHR_MAX_APPQTY = V_PHR_MAXQTY,
                                WAR_QTY = Convert.ToDecimal(item.WAR_QTY),
                                SAFE_QTY = V_INTSAFERORATE3 * Convert.ToDecimal(item.NOW_RO),
                                NORMAL_QTY = V_INTNORMALRORATE3 * Convert.ToDecimal(item.NOW_RO),
                                DIFF_PERC = V_LNGDIFF,
                                SAFE_PERC = V_INTSAFERORATE3,
                                DAY_RO = item.RO_TYPE == "1" ? Convert.ToDecimal(item.NOW_RO) : 0,
                                MON_RO = item.RO_TYPE == "2" ? Convert.ToDecimal(item.NOW_RO) : 0,
                                G34_PERC = V_INTSETRORATE3,
                                SUPPLY_PERC = 0,
                                PHR_PERC = 0,
                                NORMAL_PERC = V_INTNORMALRORATE3,
                                WAR_PERC = 0,
                                WH_NO = V_WHNO,
                                CREATE_TIME = item.STRLASTUPDATE,
                                CREATE_USER = item.STROPERATERID,
                                UPDATE_TIME = item.STRLASTUPDATE,
                                UPDATE_USER = item.STROPERATERID,
                            };
                        }
                        else if (V_WHTYPE == "3")
                        {
                            newBASERO14 = new MI_BASERO_14()
                            {
                                MMCODE = item.MMCODE,
                                RO_WHTYPE = V_WHTYPE,
                                RO_TYPE = item.RO_TYPE,
                                NOW_RO = Convert.ToDecimal(item.NOW_RO),
                                DAY_USE_10 = V_WAST10D,
                                DAY_USE_14 = V_WAST14D,
                                DAY_USE_90 = V_WAST90D,
                                MON_USE_1 = Convert.ToDecimal(item.MON_USE_1),
                                MON_USE_2 = Convert.ToDecimal(item.MON_USE_2),
                                MON_USE_3 = Convert.ToDecimal(item.MON_USE_3),
                                MON_USE_4 = Convert.ToDecimal(item.MON_USE_4),
                                MON_USE_5 = Convert.ToDecimal(item.MON_USE_5),
                                MON_USE_6 = Convert.ToDecimal(item.MON_USE_6),
                                MON_AVG_USE_3 = Convert.ToDecimal(item.MON_AVG_USE_3),
                                MON_AVG_USE_6 = Convert.ToDecimal(item.MON_AVG_USE_6),
                                G34_MAX_APPQTY = V_G34_MAXQTY,
                                SUPPLY_MAX_APPQTY = V_SUPPLY_MAXQTY,
                                PHR_MAX_APPQTY = V_PHR_MAXQTY,
                                WAR_QTY = Convert.ToDecimal(item.WAR_QTY),
                                SAFE_QTY = V_INTSAFERORATE2 * Convert.ToDecimal(item.NOW_RO),
                                NORMAL_QTY = V_INTNORMALRORATE2 * Convert.ToDecimal(item.NOW_RO),
                                DIFF_PERC = V_LNGDIFF,
                                SAFE_PERC = V_INTSAFERORATE2,
                                DAY_RO = item.RO_TYPE == "1" ? Convert.ToDecimal(item.NOW_RO) : 0,
                                MON_RO = item.RO_TYPE == "2" ? Convert.ToDecimal(item.NOW_RO) : 0,
                                G34_PERC = V_INTSETRORATE2,
                                SUPPLY_PERC = 0,
                                PHR_PERC = 0,
                                NORMAL_PERC = V_INTNORMALRORATE2,
                                WAR_PERC = 0,
                                WH_NO = V_WHNO,
                                CREATE_TIME = item.STRLASTUPDATE,
                                CREATE_USER = item.STROPERATERID,
                                UPDATE_TIME = item.STRLASTUPDATE,
                                UPDATE_USER = item.STROPERATERID,
                            };
                        }
                        else
                        {
                            newBASERO14 = new MI_BASERO_14()
                            {
                                MMCODE = item.MMCODE,
                                RO_WHTYPE = V_WHTYPE,
                                RO_TYPE = item.RO_TYPE,
                                NOW_RO = Convert.ToDecimal(item.NOW_RO),
                                DAY_USE_10 = V_WAST10D,
                                DAY_USE_14 = V_WAST14D,
                                DAY_USE_90 = V_WAST90D,
                                MON_USE_1 = Convert.ToDecimal(item.MON_USE_1),
                                MON_USE_2 = Convert.ToDecimal(item.MON_USE_2),
                                MON_USE_3 = Convert.ToDecimal(item.MON_USE_3),
                                MON_USE_4 = Convert.ToDecimal(item.MON_USE_4),
                                MON_USE_5 = Convert.ToDecimal(item.MON_USE_5),
                                MON_USE_6 = Convert.ToDecimal(item.MON_USE_6),
                                MON_AVG_USE_3 = Convert.ToDecimal(item.MON_AVG_USE_3),
                                MON_AVG_USE_6 = Convert.ToDecimal(item.MON_AVG_USE_6),
                                G34_MAX_APPQTY = V_G34_MAXQTY,
                                SUPPLY_MAX_APPQTY = V_SUPPLY_MAXQTY,
                                PHR_MAX_APPQTY = V_PHR_MAXQTY,
                                WAR_QTY = Convert.ToDecimal(item.WAR_QTY),
                                SAFE_QTY = 0,
                                NORMAL_QTY = 0,
                                DIFF_PERC = V_LNGDIFF,
                                SAFE_PERC = 0,
                                DAY_RO = item.RO_TYPE == "1" ? Convert.ToDecimal(item.NOW_RO) : 0,
                                MON_RO = item.RO_TYPE == "2" ? Convert.ToDecimal(item.NOW_RO) : 0,
                                G34_PERC = 0,
                                SUPPLY_PERC = 0,
                                PHR_PERC = 0,
                                NORMAL_PERC = 0,
                                WAR_PERC = 0,
                                WH_NO = V_WHNO,
                                CREATE_TIME = item.STRLASTUPDATE,
                                CREATE_USER = item.STROPERATERID,
                                UPDATE_TIME = item.STRLASTUPDATE,
                                UPDATE_USER = item.STROPERATERID,
                            };
                        }

                        //務必確保要寫入的資料已確實更新到MI_BASERO_14
                        db_MMSMS.cmd.Parameters.Clear();
                        var dictionary = newBASERO14.AsDictionary();
                        properties = GetPropertyInfo(nowInProgress);
                        for (var i = 0; i < properties.Length; i++)
                        {
                            db_MMSMS.cmd.Parameters.Add(properties[i].Name, dictionary[properties[i].Name] ?? "");
                        }
                        db_MMSMS.BeginTransaction();
                        db_MMSMS.cmd.CommandText = repo.Insert_MI_BASERO_14();
                        db_MMSMS.cmd.BindByName = true;
                        db_MMSMS.cmd.ExecuteNonQuery();
                        db_MMSMS.Commit();
                    }
                    catch (Exception e)
                    {
                        CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                        callDbtools_oralce.I_ERROR_LOG("DataTransfer14", e.Message, "AUTO");

                        log.Exception_To_Log(string.Format(@"
                        messsage: MI_BASERO_14 {0}
                        StackTrace: {1}
                        ---------------
                        object: {2}
                        ", e.Message, e.StackTrace, JsonConvert.SerializeObject(item, Formatting.Indented)
                           ), "DataTransfer14", "DataTransfer14");

                        db_MMSMS.Rollback();
                    }
                }
                Thread.Sleep(1000);
                #endregion

                //判斷是否為北投醫院，如果是則預設藥局最大請領RO比值5,  護理病房(檢驗科)為1
                if (hosp_id == "818")
                {
                    nowSection = "更新(客製RO倍數需求)";

                    #region UPDATE MI_BASERO_14
                    nowInProgress = "MI_BASERO_14";
                    nowInProgressSub = string.Empty;
                    consoleLog = "正在 " + nowSection + " " + nowInProgress + " " + nowInProgressSub + " ...";
                    Console.Write("\n" + consoleLog);
                    // update
                    try
                    {
                        db_MMSMS.BeginTransaction();
                        db_MMSMS.cmd.CommandText = repo.Update_MI_BASERO_14_RO_818();
                        db_MMSMS.cmd.ExecuteNonQuery();
                        db_MMSMS.Commit();
                    }
                    catch (Exception e)
                    {
                        CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                        callDbtools_oralce.I_ERROR_LOG("DataTransfer14", e.Message, "AUTO");

                        log.Exception_To_Log(string.Format(@"
                        messsage: MI_BASERO_14 {0}
                        StackTrace: {1}
                        ---------------
                        object: {2}
                        ", e.Message, e.StackTrace, db_MMSMS.cmd.CommandText
                           //JsonConvert.SerializeObject(rx)
                           ), "DataTransfer14", "DataTransfer14");

                        db_MMSMS.Rollback();
                    }
                    Thread.Sleep(1000);
                    #endregion
                }

                //釋放記憶體
                db_MMSMS.Dispose();
                db_HISDG.Dispose();
                Console.WriteLine("\n BASERO14轉檔完成");
            }
            catch (Exception e)
            {
                CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                callDbtools_oralce.I_ERROR_LOG("DataTransfer14", e.Message, "AUTO");

                log.Exception_To_Log(string.Format(@"
                        messsage: {0}
                        StackTrace: {1}
                        ---------------
                        object: {2}
                        ", e.Message, e.StackTrace, string.Empty
                   //JsonConvert.SerializeObject(rx)
                   ), "DataTransfer14", "DataTransfer14");

                //db_MMSMS.Rollback();
                db_MMSMS.Dispose();

                //db_HISDG.Rollback();
                db_HISDG.Dispose();
                Console.WriteLine("\n BASERO14轉檔操作出現異常，請至以下log路徑查看訊息：" + @"
                C:\mmsms-schedule-log\DataTransfer14");
            }

        }

        private PropertyInfo[] GetPropertyInfo(string modelName)
        {
            return Type.GetType(string.Format("DataTransfer14.{0}", modelName)).GetProperties();
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
