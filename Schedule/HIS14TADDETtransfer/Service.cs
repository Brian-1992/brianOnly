using JCLib.DB.Tool;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HIS14TADDETtransfer
{
    class Service
    {
        protected OracleDB db_MMSMS;
        protected OracleDB db_HISDG;
        protected OracleDB db_HISDG_2;

        LogController log = new LogController();

        public void Run(string hosp_id, string strStartDate = null, string strEndDate = null)
        {
            DateTime StartDate = DateTime.Today;
            DateTime EndDate = DateTime.Today;
            DateTime dateValue;

            #region parse date_range
            if (!string.IsNullOrEmpty(strStartDate) && !string.IsNullOrEmpty(strEndDate))
            {
                Console.Write("\n指定 開始結束期間 " + strStartDate + " ~ " + strEndDate);
                if (DateTime.TryParse(strStartDate, out dateValue))
                {
                    StartDate = dateValue;
                }
                else
                {
                    Console.WriteLine("  Unable to parse EndDate '{0}'.", strStartDate);
                }

                if (DateTime.TryParse(strEndDate, out dateValue))
                {
                    EndDate = dateValue;
                }
                else
                {
                    Console.WriteLine("  Unable to parse EndDate '{0}'.", strEndDate);
                }
            }
            #endregion

            try
            {
                db_MMSMS = new OracleDB("MMSMS");
                db_HISDG = new OracleDB("HISDG");
                
                HIS14TADDETtransferRepository repo = new HIS14TADDETtransferRepository();

                // 0.系統建置前，param_m、param_d要insert各醫院藥局扣庫點(STOCKCODE)基本資料，insert 指令參閱「四、各院藥局扣庫點」
                #region
                Console.Write("\n正在更新各院藥局扣庫點(PARMA_M, PARAM_D)");
                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Delete_PARAM_D();
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();

                DataTable chkPM = db_MMSMS.GetDataSet(repo.Get_PARAM_M_Query()).Tables[0];
                if (chkPM.Rows.Count == 0)
                {
                    // insert PARAM_M
                    db_MMSMS.BeginTransaction();
                    db_MMSMS.cmd.CommandText = repo.Insert_PARAM_M();
                    db_MMSMS.cmd.ExecuteNonQuery();
                    db_MMSMS.Commit();
                }

                #region PARAM_D HIS14_TADDET List
                List<PARAM_D> list_param_d = new List<PARAM_D>();
                switch (hosp_id)
                {
                    case "803":
                        // 803臺中 E40010 總院藥局
                        list_param_d.Add(new PARAM_D { GRP_CODE = "HIS14_TADDET", DATA_SEQ = "1", DATA_NAME = "1", DATA_VALUE = "E40010", DATA_DESC = "門診藥局扣庫點", DATA_REMARK = "8031" });
                        list_param_d.Add(new PARAM_D { GRP_CODE = "HIS14_TADDET", DATA_SEQ = "2", DATA_NAME = "2", DATA_VALUE = "E40010", DATA_DESC = "急診藥局扣庫點", DATA_REMARK = "8031" });
                        list_param_d.Add(new PARAM_D { GRP_CODE = "HIS14_TADDET", DATA_SEQ = "3", DATA_NAME = "3", DATA_VALUE = "E40010", DATA_DESC = "住院藥局扣庫點", DATA_REMARK = "8031" });
                        // 816中清 E80000 中清藥局(跟803共用藥衛材系統)
                        list_param_d.Add(new PARAM_D { GRP_CODE = "HIS14_TADDET", DATA_SEQ = "4", DATA_NAME = "1", DATA_VALUE = "E80000", DATA_DESC = "門診藥局扣庫點", DATA_REMARK = "8161" });
                        list_param_d.Add(new PARAM_D { GRP_CODE = "HIS14_TADDET", DATA_SEQ = "5", DATA_NAME = "2", DATA_VALUE = "E80000", DATA_DESC = "急診藥局扣庫點", DATA_REMARK = "8161" });
                        list_param_d.Add(new PARAM_D { GRP_CODE = "HIS14_TADDET", DATA_SEQ = "6", DATA_NAME = "3", DATA_VALUE = "E80000", DATA_DESC = "住院藥局扣庫點", DATA_REMARK = "8161" });
                        break;
                    //case "816":
                    //    // 816中清 E80000 中清藥局
                    //    list_param_d.Add(new PARAM_D { GRP_CODE = "HIS14_TADDET", DATA_SEQ = "4", DATA_NAME = "1", DATA_VALUE = "E80000", DATA_DESC = "門診藥局扣庫點", DATA_REMARK = "8161" });
                    //    list_param_d.Add(new PARAM_D { GRP_CODE = "HIS14_TADDET", DATA_SEQ = "5", DATA_NAME = "2", DATA_VALUE = "E80000", DATA_DESC = "急診藥局扣庫點", DATA_REMARK = "8161" });
                    //    list_param_d.Add(new PARAM_D { GRP_CODE = "HIS14_TADDET", DATA_SEQ = "6", DATA_NAME = "3", DATA_VALUE = "E80000", DATA_DESC = "住院藥局扣庫點", DATA_REMARK = "8161" });
                    //    break;
                    case "804":
                        // 804桃園 EA8 藥劑科(藥局)
                        list_param_d.Add(new PARAM_D { GRP_CODE = "HIS14_TADDET", DATA_SEQ = "7", DATA_NAME = "1", DATA_VALUE = "EA8", DATA_DESC = "門診藥局扣庫點", DATA_REMARK = "8041" });
                        list_param_d.Add(new PARAM_D { GRP_CODE = "HIS14_TADDET", DATA_SEQ = "8", DATA_NAME = "2", DATA_VALUE = "EA8", DATA_DESC = "急診藥局扣庫點", DATA_REMARK = "8041" });
                        list_param_d.Add(new PARAM_D { GRP_CODE = "HIS14_TADDET", DATA_SEQ = "9", DATA_NAME = "3", DATA_VALUE = "EA8", DATA_DESC = "住院藥局扣庫點", DATA_REMARK = "8041" });
                        break;
                    case "805":
                        // 805花蓮
                        // E40010 藥劑科(北埔藥局) (本院藥局)
                        // E40020 太魯閣藥局
                        // E40030 進豐藥局
                        list_param_d.Add(new PARAM_D { GRP_CODE = "HIS14_TADDET", DATA_SEQ = "10", DATA_NAME = "1", DATA_VALUE = "E40010", DATA_DESC = "北埔門診藥局扣庫點", DATA_REMARK = "8051" });
                        list_param_d.Add(new PARAM_D { GRP_CODE = "HIS14_TADDET", DATA_SEQ = "11", DATA_NAME = "2", DATA_VALUE = "E40010", DATA_DESC = "北埔急診藥局扣庫點", DATA_REMARK = "8051" });
                        list_param_d.Add(new PARAM_D { GRP_CODE = "HIS14_TADDET", DATA_SEQ = "12", DATA_NAME = "3", DATA_VALUE = "E40010", DATA_DESC = "北埔住院藥局扣庫點", DATA_REMARK = "8051" });
                        list_param_d.Add(new PARAM_D { GRP_CODE = "HIS14_TADDET", DATA_SEQ = "13", DATA_NAME = "1", DATA_VALUE = "E40020", DATA_DESC = "太魯閣門診藥局扣庫點", DATA_REMARK = "8052" });
                        list_param_d.Add(new PARAM_D { GRP_CODE = "HIS14_TADDET", DATA_SEQ = "14", DATA_NAME = "2", DATA_VALUE = "E40020", DATA_DESC = "太魯閣急診藥局扣庫點", DATA_REMARK = "8052" });
                        list_param_d.Add(new PARAM_D { GRP_CODE = "HIS14_TADDET", DATA_SEQ = "15", DATA_NAME = "3", DATA_VALUE = "E40020", DATA_DESC = "太魯閣住院藥局扣庫點", DATA_REMARK = "8052" });
                        list_param_d.Add(new PARAM_D { GRP_CODE = "HIS14_TADDET", DATA_SEQ = "16", DATA_NAME = "1", DATA_VALUE = "E40030", DATA_DESC = "進豐門診藥局扣庫點", DATA_REMARK = "8053" });
                        list_param_d.Add(new PARAM_D { GRP_CODE = "HIS14_TADDET", DATA_SEQ = "17", DATA_NAME = "2", DATA_VALUE = "E40030", DATA_DESC = "進豐急診藥局扣庫點", DATA_REMARK = "8053" });
                        list_param_d.Add(new PARAM_D { GRP_CODE = "HIS14_TADDET", DATA_SEQ = "18", DATA_NAME = "3", DATA_VALUE = "E40030", DATA_DESC = "進豐住院藥局扣庫點", DATA_REMARK = "8053" });
                        break;
                    case "807":
                        // 807松山 E40000 藥局
                        list_param_d.Add(new PARAM_D { GRP_CODE = "HIS14_TADDET", DATA_SEQ = "19", DATA_NAME = "1", DATA_VALUE = "E40000", DATA_DESC = "門診藥局扣庫點", DATA_REMARK = "8071" });
                        list_param_d.Add(new PARAM_D { GRP_CODE = "HIS14_TADDET", DATA_SEQ = "20", DATA_NAME = "2", DATA_VALUE = "E40000", DATA_DESC = "急診藥局扣庫點", DATA_REMARK = "8071" });
                        list_param_d.Add(new PARAM_D { GRP_CODE = "HIS14_TADDET", DATA_SEQ = "21", DATA_NAME = "3", DATA_VALUE = "E40000", DATA_DESC = "住院藥局扣庫點", DATA_REMARK = "8071" });
                        break;
                    case "811":
                        // 811澎湖 E30 藥局
                        list_param_d.Add(new PARAM_D { GRP_CODE = "HIS14_TADDET", DATA_SEQ = "22", DATA_NAME = "1", DATA_VALUE = "E30", DATA_DESC = "門診藥局扣庫點", DATA_REMARK = "8111" });
                        list_param_d.Add(new PARAM_D { GRP_CODE = "HIS14_TADDET", DATA_SEQ = "23", DATA_NAME = "2", DATA_VALUE = "E30", DATA_DESC = "急診藥局扣庫點", DATA_REMARK = "8111" });
                        list_param_d.Add(new PARAM_D { GRP_CODE = "HIS14_TADDET", DATA_SEQ = "24", DATA_NAME = "3", DATA_VALUE = "E30", DATA_DESC = "住院藥局扣庫點", DATA_REMARK = "8111" });
                        break;
                    case "812":
                        // 812基隆
                        // E40010 正榮藥局(本院藥局)
                        // E70010 孝二藥局
                        list_param_d.Add(new PARAM_D { GRP_CODE = "HIS14_TADDET", DATA_SEQ = "25", DATA_NAME = "1", DATA_VALUE = "E40010", DATA_DESC = "正榮門診藥局扣庫點", DATA_REMARK = "8121" });
                        list_param_d.Add(new PARAM_D { GRP_CODE = "HIS14_TADDET", DATA_SEQ = "26", DATA_NAME = "2", DATA_VALUE = "E40010", DATA_DESC = "正榮急診藥局扣庫點", DATA_REMARK = "8121" });
                        list_param_d.Add(new PARAM_D { GRP_CODE = "HIS14_TADDET", DATA_SEQ = "27", DATA_NAME = "3", DATA_VALUE = "E40010", DATA_DESC = "正榮住院藥局扣庫點", DATA_REMARK = "8121" });
                        list_param_d.Add(new PARAM_D { GRP_CODE = "HIS14_TADDET", DATA_SEQ = "28", DATA_NAME = "1", DATA_VALUE = "E70010", DATA_DESC = "孝二門診藥局扣庫點", DATA_REMARK = "8122" });
                        list_param_d.Add(new PARAM_D { GRP_CODE = "HIS14_TADDET", DATA_SEQ = "29", DATA_NAME = "2", DATA_VALUE = "E70010", DATA_DESC = "孝二急診藥局扣庫點", DATA_REMARK = "8122" });
                        list_param_d.Add(new PARAM_D { GRP_CODE = "HIS14_TADDET", DATA_SEQ = "30", DATA_NAME = "3", DATA_VALUE = "E70010", DATA_DESC = "孝二住院藥局扣庫點", DATA_REMARK = "8122" });
                        break;
                    case "813":
                        // 813新竹 E60801 藥事科1(藥局)
                        list_param_d.Add(new PARAM_D { GRP_CODE = "HIS14_TADDET", DATA_SEQ = "31", DATA_NAME = "1", DATA_VALUE = "E60801", DATA_DESC = "門診藥局扣庫點", DATA_REMARK = "8131" });
                        list_param_d.Add(new PARAM_D { GRP_CODE = "HIS14_TADDET", DATA_SEQ = "32", DATA_NAME = "2", DATA_VALUE = "E60801", DATA_DESC = "急診藥局扣庫點", DATA_REMARK = "8131" });
                        list_param_d.Add(new PARAM_D { GRP_CODE = "HIS14_TADDET", DATA_SEQ = "33", DATA_NAME = "3", DATA_VALUE = "E60801", DATA_DESC = "住院藥局扣庫點", DATA_REMARK = "8131" });
                        break;
                    case "818":
                        // 818北投
                        // E66020 山上藥局(本院藥局)
                        // E66030 山下藥局(門診藥局)
                        list_param_d.Add(new PARAM_D { GRP_CODE = "HIS14_TADDET", DATA_SEQ = "34", DATA_NAME = "1", DATA_VALUE = "E66020", DATA_DESC = "山上門診藥局扣庫點", DATA_REMARK = "8181" });
                        list_param_d.Add(new PARAM_D { GRP_CODE = "HIS14_TADDET", DATA_SEQ = "35", DATA_NAME = "2", DATA_VALUE = "E66020", DATA_DESC = "山上急診藥局扣庫點", DATA_REMARK = "8181" });
                        list_param_d.Add(new PARAM_D { GRP_CODE = "HIS14_TADDET", DATA_SEQ = "36", DATA_NAME = "3", DATA_VALUE = "E66020", DATA_DESC = "山上住院藥局扣庫點", DATA_REMARK = "8181" });
                        list_param_d.Add(new PARAM_D { GRP_CODE = "HIS14_TADDET", DATA_SEQ = "37", DATA_NAME = "1", DATA_VALUE = "E66030", DATA_DESC = "山下門診藥局扣庫點", DATA_REMARK = "8182" });
                        list_param_d.Add(new PARAM_D { GRP_CODE = "HIS14_TADDET", DATA_SEQ = "38", DATA_NAME = "2", DATA_VALUE = "E66030", DATA_DESC = "山下急診藥局扣庫點", DATA_REMARK = "8182" });
                        list_param_d.Add(new PARAM_D { GRP_CODE = "HIS14_TADDET", DATA_SEQ = "39", DATA_NAME = "3", DATA_VALUE = "E66030", DATA_DESC = "山下住院藥局扣庫點", DATA_REMARK = "8182" });
                        break;
                    default:
                        break;
                }
                #endregion PARAM_D List

                db_MMSMS.BeginTransaction();
                db_MMSMS.cmd.CommandText = repo.Insert_PARAM_D(list_param_d);
                db_MMSMS.cmd.ExecuteNonQuery();
                db_MMSMS.Commit();
                #endregion

                // 1.判斷現在幾點，決定介接哪一天的資料
                #region
                DataTable getHRBT = db_MMSMS.GetDataSet(repo.Get_HRBT_Query()).Tables[0];
                string V_HOUR = Convert.ToString(getHRBT.Rows[0]["V_HOUR"]);
                string V_DATE = Convert.ToString(getHRBT.Rows[0]["V_DATE"]); // 若V_HOUR=00則為twn_date(sysdate-1), 否則為twn_date(sysdate)
                string V_DATE_TODAY = Convert.ToString(getHRBT.Rows[0]["V_DATE_TODAY"]); 
                string V_ETIME = Convert.ToString(getHRBT.Rows[0]["V_ETIME"]);

                // 若V_HOUR=00，介接昨天整天的資料
                foreach (DateTime day in EachDay(StartDate, EndDate))
                {
                    if (StartDate != EndDate)
                        V_DATE = day.AddYears(-1911).ToString("yyyMMdd");
                    Console.Write("\n processing " + V_DATE);
                    if (V_HOUR == "00")
                    {
                        Console.Write("\nV_HOUR=00,介接前一天資料");
                        db_MMSMS.BeginTransaction();
                        db_MMSMS.cmd.CommandText = repo.Delete_HIS14_TADDET(V_DATE);
                        db_MMSMS.cmd.ExecuteNonQuery();
                        db_MMSMS.Commit();

                        DataTable getTADDET = db_HISDG.GetDataSet(repo.Get_TADDET_Query(V_DATE, hosp_id)).Tables[0];
                        db_MMSMS.BeginTransaction();
                        for (int i = 0; i < getTADDET.Rows.Count; i++)
                        {
                            int itemIndex = i + 1;
                            Console.Write("\r{0} [ {1} / {2} ] {3} ", "正在轉入HIS14_TADDET ...", itemIndex, getTADDET.Rows.Count, ((double)itemIndex / getTADDET.Rows.Count).ToString("P"));
                            db_MMSMS.cmd.Parameters.Clear();
                            for (int j = 0; j < getTADDET.Columns.Count; j++)
                            {
                                db_MMSMS.cmd.Parameters.Add(Convert.ToString(getTADDET.Columns[j]), getTADDET.Rows[i][getTADDET.Columns[j]]);
                            }

                            // 先找醫院代碼sHospCode
                            DataTable getHospCode = db_MMSMS.GetDataSet(repo.Get_HOSPCODE_Query()).Tables[0];
                            string sHospCode = Convert.ToString(getHospCode.Rows[0]["sHospCode"]);

                            // 再找扣庫點sSTOCKCODE
                            DataTable getSTOCKCODE = db_MMSMS.GetDataSet(repo.Get_STOCKCODE_Query(sHospCode, Convert.ToString(getTADDET.Rows[0]["DET_STKKIND"]), Convert.ToString(getTADDET.Rows[0]["DET_KIND"]))).Tables[0];
                            string sSTOCKCODE = Convert.ToString(getSTOCKCODE.Rows[0]["sSTOCKCODE"]);

                            db_MMSMS.cmd.Parameters.Add("STOCKCODE", sSTOCKCODE);

                            db_MMSMS.cmd.CommandText = repo.Insert_HIS14_TADDET();
                            db_MMSMS.cmd.BindByName = true;
                            db_MMSMS.cmd.ExecuteNonQuery();

                        }
                        db_MMSMS.Commit();

                        Thread.Sleep(1000);

                        // 若為803臺中總院的資料庫，增加轉中清分院的扣庫資料
                        if (hosp_id == "803")
                        {
                            db_HISDG_2 = new OracleDB("HISDG_2");
                            Console.Write("\n803臺中總院轉中清分院扣庫資料");
                            db_MMSMS.BeginTransaction();
                            db_MMSMS.cmd.CommandText = repo.Delete_HIS14_TADDET1(V_DATE);
                            db_MMSMS.cmd.ExecuteNonQuery();
                            db_MMSMS.Commit();

                            DataTable getTADDET1 = db_HISDG_2.GetDataSet(repo.Get_TADDET1_Query(V_DATE)).Tables[0];
                            db_MMSMS.BeginTransaction();
                            for (int i = 0; i < getTADDET1.Rows.Count; i++)
                            {
                                int itemIndex = i + 1;
                                Console.Write("\r{0} [ {1} / {2} ] {3} ", "正在轉入HIS14_TADDET ...", itemIndex, getTADDET1.Rows.Count, ((double)itemIndex / getTADDET1.Rows.Count).ToString("P"));
                                db_MMSMS.cmd.Parameters.Clear();
                                for (int j = 0; j < getTADDET1.Columns.Count; j++)
                                {
                                    db_MMSMS.cmd.Parameters.Add(Convert.ToString(getTADDET1.Columns[j]), getTADDET1.Rows[i][getTADDET1.Columns[j]]);
                                }

                                // 醫院代碼sHospCode
                                string sHospCode = "816";

                                // 再找扣庫點sSTOCKCODE
                                DataTable getSTOCKCODE = db_MMSMS.GetDataSet(repo.Get_STOCKCODE_Query(sHospCode, Convert.ToString(getTADDET1.Rows[0]["DET_STKKIND"]), Convert.ToString(getTADDET1.Rows[0]["DET_KIND"]))).Tables[0];
                                string sSTOCKCODE = Convert.ToString(getSTOCKCODE.Rows[0]["sSTOCKCODE"]);

                                db_MMSMS.cmd.Parameters.Add("STOCKCODE", sSTOCKCODE);

                                db_MMSMS.cmd.CommandText = repo.Insert_HIS14_TADDET1();
                                db_MMSMS.cmd.BindByName = true;
                                db_MMSMS.cmd.ExecuteNonQuery();

                            }
                            db_MMSMS.Commit();

                            Thread.Sleep(1000);

                            db_HISDG_2.Dispose();
                        }
                    }
                    else
                    {
                        // V_HOUR非00時，介接今天000000到當下前一整點的59分59秒
                        Console.Write("\nV_HOUR非00,介接今日到目前為止的資料");
                        db_MMSMS.BeginTransaction();
                        db_MMSMS.cmd.CommandText = repo.Delete_HIS14_TADDET(V_DATE);
                        db_MMSMS.cmd.ExecuteNonQuery();
                        db_MMSMS.Commit();

                        DataTable getTADDET = db_HISDG.GetDataSet(repo.Get_TADDET_Query(V_DATE, hosp_id)).Tables[0];
                        db_MMSMS.BeginTransaction();
                        for (int i = 0; i < getTADDET.Rows.Count; i++)
                        {
                            int itemIndex = i + 1;
                            Console.Write("\r{0} [ {1} / {2} ] {3} ", "正在轉入HIS14_TADDET ...", itemIndex, getTADDET.Rows.Count, ((double)itemIndex / getTADDET.Rows.Count).ToString("P"));
                            db_MMSMS.cmd.Parameters.Clear();
                            for (int j = 0; j < getTADDET.Columns.Count; j++)
                            {
                                db_MMSMS.cmd.Parameters.Add(Convert.ToString(getTADDET.Columns[j]), getTADDET.Rows[i][getTADDET.Columns[j]]);
                            }

                            // 先找醫院代碼sHospCode
                            DataTable getHospCode = db_MMSMS.GetDataSet(repo.Get_HOSPCODE_Query()).Tables[0];
                            string sHospCode = Convert.ToString(getHospCode.Rows[0]["sHospCode"]);

                            // 再找扣庫點sSTOCKCODE
                            DataTable getSTOCKCODE = db_MMSMS.GetDataSet(repo.Get_STOCKCODE_Query(sHospCode, Convert.ToString(getTADDET.Rows[0]["DET_STKKIND"]), Convert.ToString(getTADDET.Rows[0]["DET_KIND"]))).Tables[0];
                            string sSTOCKCODE = Convert.ToString(getSTOCKCODE.Rows[0]["sSTOCKCODE"]);

                            db_MMSMS.cmd.Parameters.Add("STOCKCODE", sSTOCKCODE);

                            db_MMSMS.cmd.CommandText = repo.Insert_HIS14_TADDET();
                            db_MMSMS.cmd.BindByName = true;
                            db_MMSMS.cmd.ExecuteNonQuery();

                        }
                        db_MMSMS.Commit();

                        Thread.Sleep(1000);

                        // 若為803臺中總院的資料庫，增加轉中清分院的扣庫資料
                        if (hosp_id == "803")
                        {
                            db_HISDG_2 = new OracleDB("HISDG_2");
                            Console.Write("\n803臺中總院轉中清分院扣庫資料");
                            db_MMSMS.BeginTransaction();
                            db_MMSMS.cmd.CommandText = repo.Delete_HIS14_TADDET1(V_DATE);
                            db_MMSMS.cmd.ExecuteNonQuery();
                            db_MMSMS.Commit();

                            DataTable getTADDET1 = db_HISDG_2.GetDataSet(repo.Get_TADDET1_Query(V_DATE)).Tables[0];
                            db_MMSMS.BeginTransaction();
                            for (int i = 0; i < getTADDET1.Rows.Count; i++)
                            {
                                int itemIndex = i + 1;
                                Console.Write("\r{0} [ {1} / {2} ] {3} ", "正在轉入HIS14_TADDET ...", itemIndex, getTADDET1.Rows.Count, ((double)itemIndex / getTADDET1.Rows.Count).ToString("P"));
                                db_MMSMS.cmd.Parameters.Clear();
                                for (int j = 0; j < getTADDET1.Columns.Count; j++)
                                {
                                    db_MMSMS.cmd.Parameters.Add(Convert.ToString(getTADDET1.Columns[j]), getTADDET1.Rows[i][getTADDET1.Columns[j]]);
                                }

                                // 醫院代碼sHospCode
                                string sHospCode = "816";

                                // 再找扣庫點sSTOCKCODE
                                DataTable getSTOCKCODE = db_MMSMS.GetDataSet(repo.Get_STOCKCODE_Query(sHospCode, Convert.ToString(getTADDET1.Rows[0]["DET_STKKIND"]), Convert.ToString(getTADDET1.Rows[0]["DET_KIND"]))).Tables[0];
                                string sSTOCKCODE = Convert.ToString(getSTOCKCODE.Rows[0]["sSTOCKCODE"]);

                                db_MMSMS.cmd.Parameters.Add("STOCKCODE", sSTOCKCODE);

                                db_MMSMS.cmd.CommandText = repo.Insert_HIS14_TADDET1();
                                db_MMSMS.cmd.BindByName = true;
                                db_MMSMS.cmd.ExecuteNonQuery();

                            }
                            db_MMSMS.Commit();

                            Thread.Sleep(1000);

                            db_HISDG_2.Dispose();
                        }
                    }
                }
                #endregion

                // 完成轉檔，呼叫store procedure GEN_HIS14_CONSUME
                Console.Write("\n正在執行store procedure GEN_HIS14_CONSUME_BYDATE...");
                db_MMSMS.cmd.Parameters.Clear();
                db_MMSMS.cmd.CommandText = "MMSADM.GEN_HIS14_CONSUME_BYDATE";
                db_MMSMS.cmd.CommandType = CommandType.StoredProcedure;

                OracleParameter o_retid = new OracleParameter("O_RETID", OracleDbType.Varchar2, 1);
                o_retid.Direction = ParameterDirection.Output;
                db_MMSMS.cmd.Parameters.Add(o_retid);

                OracleParameter i_v_date = new OracleParameter("I_V_DATE", OracleDbType.Varchar2, 7);
                i_v_date.Direction = ParameterDirection.Input;
                i_v_date.Value = V_DATE_TODAY; // 日期一律傳入今日,SP內會再依據現在是否為00決定處理的是今天或昨天

                OracleParameter i_type = new OracleParameter("I_TYPE", OracleDbType.Varchar2, 1);
                i_type.Direction = ParameterDirection.Input;
                if (V_DATE == "00")
                    i_type.Value = "D";
                else
                    i_type.Value = "T";
                db_MMSMS.cmd.Parameters.Add(i_v_date);
                db_MMSMS.cmd.Parameters.Add(i_type);
                db_MMSMS.cmd.ExecuteNonQuery();

                // 完成轉檔，呼叫store procedure GEN_HIS14_CONSUME
                Console.Write("\n正在執行store procedure JOB_USE_INVMON...");
                db_MMSMS.cmd.Parameters.Clear();
                db_MMSMS.cmd.CommandText = "MMSADM.JOB_USE_INVMON";
                db_MMSMS.cmd.CommandType = CommandType.StoredProcedure;
                OracleParameter o_retid2 = new OracleParameter("O_RETID", OracleDbType.Varchar2, 1);
                o_retid2.Direction = ParameterDirection.Output;
                db_MMSMS.cmd.Parameters.Add(o_retid2);
                db_MMSMS.cmd.ExecuteNonQuery();

                db_MMSMS.Dispose();
                db_HISDG.Dispose();
            }
            catch (Exception e)
            {
                Console.Write("\r發生錯誤：" + e.Message);
                // Console.ReadLine();

                log.Exception_To_Log(string.Format(@"
                        messsage: {0}
                        StackTrace: {1}
                        ---------------
                        object: {2}
                        ", e.Message, e.StackTrace, string.Empty
                   //JsonConvert.SerializeObject(rx)
                   ), "HIS14TADDETtransfer", "HIS14TADDETtransfer");

                //db_MMSMS.Rollback();
                db_MMSMS.Dispose();

                //db_HISDG.Rollback();
                db_HISDG.Dispose();

                if (hosp_id == "803")
                    db_HISDG_2.Dispose();
            }
        }

        public IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                yield return day;
        }
    }
}
