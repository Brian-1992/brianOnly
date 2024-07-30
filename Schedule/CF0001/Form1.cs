using System;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Collections.Generic;
using System.IO;
using JCLib.DB.Tool;

namespace CF0001
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //資料庫連線
        private string getDBConnStr()
        {
            string connStr;
            OracleConnectionStringBuilder oracleConnstr = new OracleConnectionStringBuilder();
            //AIDC連線
            //oracleConnstr.ConnectionString = "Persist Security Info=True;Data Source=192.168.99.52/mmsms;User ID=MMSDEV;Password=Mms#Dev1357;";
            //三總連線
            oracleConnstr.ConnectionString = "Persist Security Info=True;Data Source=10.200.6.200:1524/MATE2DB;User ID=MMSDEV;Password=Mms#Dev1357;";
            connStr = oracleConnstr.ConnectionString;
            return connStr;
        }

        //程式起始 CF0001-電子儲位標籤資料產出排程 排程每天 7:20、11:50
        private void Form1_Load(object sender, EventArgs e)
        {
            //1_呼叫 procedure GEN_STLOC_ETAG
            string callSpRes = CallSp();
            if(callSpRes == "sucess")
            {
                //2_export data to csv
                exportCSV();
            }
            this.Close();
        }

        //1_呼叫 procedure GEN_STLOC_ETAG
        public string CallSp()
        {
            string ConnStr = getDBConnStr(); // 排程會放在庫房,連線字串寫在程式內以避免被看到
            //CallDBtools calldbtools = new CallDBtools();
            //string ConnStr = calldbtools.SelectDB("oracle");
            OracleConnection Conn = new OracleConnection(ConnStr);
            string spname = "GEN_STLOC_ETAG";
            OracleCommand cmd = new OracleCommand();
            string codemsg = "error";
            try
            {
                cmd.CommandText = spname;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                Conn.Open();
                cmd.Connection = Conn;
                cmd.Parameters.Add("ret_code", OracleDbType.Varchar2,500).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();

                var ret_code = cmd.Parameters["ret_code"].Value;
                string retCodeStr = ((Oracle.ManagedDataAccess.Types.OracleString)ret_code).Value;
                if (retCodeStr == "000")
                    codemsg = "sucess";
                else
                    codemsg = "error";
            }
            catch (Exception ex)
            {
                codemsg = "error";
                I_ERROR_LOG("CF0001", "STEP1-產生電子儲位標籤-執行SP失敗：" + ex.Message, "AUTO");
            }
            finally
            {
                cmd.Dispose();
                Conn.Close();
                Conn.Dispose();
            }
            return codemsg;
        }

        //2_export data to csv
        public void exportCSV()
        {
            string msg_oracle = "error";
            DataTable dt_oralce = new DataTable();
            string sql_oracle = " select mmcode as 藥材代碼, mmcode as Barcode條碼, MMNAME_C as 產品名稱, ";
            sql_oracle += "              STORE_LOC as 儲位,	BASE_UNIT as 單位, QTY as 實存量, ";
            sql_oracle += "              SAFE_QTY as 最低安全量, to_char(EXP_DATE,'yyyymmdd') as 效期,	ALARM_MON as 近效期, ";
            sql_oracle += "              AGEN_NAMEC as 廠商  ";
            sql_oracle += "         from BC_ETAG ";
            sql_oracle += "        where flag = 'A' ";
            dt_oralce = CallOpenSQLReturnDT(sql_oracle, "藥材代碼", null, "oracle", "T1", ref msg_oracle);
            if (msg_oracle == "" && dt_oralce.Rows.Count > 0) //資料抓取沒有錯誤 且有資料
            {
                string codeStr = SaveToCSV(dt_oralce, "D:\\", "ESL_GetInfo", "電子儲位標籤資料.csv");
                if(codeStr == "sucess") //csv產生成功
                {
                    int rowsAffected = -1;
                    string dbmsg = "error";
                    List<OracleParam> paraList = new List<OracleParam>();
                    string cmdstr = "update BC_ETAG set flag='B', out_dt=sysdate where flag='A' ";
                    rowsAffected = CallExecSQL(cmdstr, null, "oracle", ref dbmsg);
                    if(dbmsg != "")
                        I_ERROR_LOG("CF0001", "STEP4-產生電子儲位標籤-更新BC_ETAG失敗：" + dbmsg, "AUTO");

                }
            }else if(msg_oracle != "")
                I_ERROR_LOG("CF0001", "STEP2-產生電子儲位標籤-取得BC_ETAG失敗：" + msg_oracle, "AUTO");
        }

        public string SaveToCSV(DataTable oTable, string partition, string filePath, string fileName)
        {
            string codemsg = "error";
            try
            {
                string AllFilePathName = partition + "\\" + filePath + "\\" + fileName;
                string PartitionFilePath = partition + "\\" + filePath;
                if (!File.Exists(AllFilePathName)) //檔案不存在
                {
                    if (Directory.Exists(partition)) //D槽存在
                    {
                        Directory.CreateDirectory(PartitionFilePath); //產生 ESL_GetInfo資料夾
                    }
                    else //D槽不存在
                    {
                        AllFilePathName = "C:\\" + filePath + "\\" + fileName;
                        PartitionFilePath = "C:\\" + filePath;
                        Directory.CreateDirectory(PartitionFilePath);
                    }
                }
                string data = "";
                //StreamWriter wr = new StreamWriter(AllFilePathName, false, System.Text.Encoding.Default);
                StreamWriter wr = new StreamWriter(AllFilePathName, false, System.Text.Encoding.UTF8);
                foreach (DataColumn column in oTable.Columns)
                {
                    data += column.ColumnName + ",";
                }
                data = data.TrimEnd(',');
                data += "\r\n";
                wr.Write(data);
                data = "";

                foreach (DataRow row in oTable.Rows)
                {
                    foreach (DataColumn column in oTable.Columns)
                    {
                        data += row[column].ToString().Trim() + ",";
                    }
                    data = data.TrimEnd(',');
                    data += "\r\n";
                    wr.Write(data);
                    data = "";
                }
                data += "\r\n";

                wr.Dispose();
                wr.Close();
                codemsg = "sucess";
            }
            catch (Exception ex)
            {
                codemsg = "error";
                I_ERROR_LOG("CF0001", "STEP3-產生電子儲位標籤-產生csv失敗：" + ex.Message, "AUTO");
            }
            return codemsg;
        }

        public class OracleParam
        {
            public int ParaNo { set; get; }
            public string ParaNM { set; get; }
            public string ParaTP { set; get; }
            public string ParaVL { set; get; }

            public OracleParam(int paraNo, string paraNM, string paraTP, string paraVL)
            {
                this.ParaNo = paraNo;
                this.ParaNM = paraNM;
                this.ParaTP = paraTP;
                this.ParaVL = paraVL;
            }

        }

        //回傳單一SQL資料庫更動的筆數
        public int ExecSQL(OracleCommand cmd, string connNM, ref string dbmsg)
        {
            int rowsAffected = -1;
            OracleConnection conn = new OracleConnection(connNM);
            conn.Open();
            cmd.Connection = conn;
            try
            {
                rowsAffected = cmd.ExecuteNonQuery();
                dbmsg = "";
            }
            catch (Exception ex)
            {
                //string parStr = "";
                //for (int i = 0; i < cmd.Parameters.Count; i++)
                //{
                //    parStr += cmd.Parameters[i].ParameterName + " = " + cmd.Parameters[i].Value;
                //}
                //CallDBtools callDBtools = new CallDBtools();
                //callDBtools.WriteExceptionLog(ex.Message, cmd.CommandText.ToString(), parStr);
                dbmsg = ex.Message;
            }
            finally
            {
                cmd.Dispose();
                conn.Close();
                conn.Dispose();
            }
            return rowsAffected;
        }

        //回傳單一SQL資料庫更動的筆數
        //cmdstr:單一SQL語法、paraList:參數NO=1、參數NAME、參數TYPE、參數VALUE、dbNM:資料庫名稱
        public int CallExecSQL(string cmdstr, List<OracleParam> paraList, string dbNM, ref string dbmsg)
        {
            OracleCommand cmd = new OracleCommand(cmdstr);
            int rowsAffected = -1;
            string dbConn = "";

            dbConn = getDBConnStr(); // 排程會放在庫房,連線字串寫在程式內以避免被看到
            //CallDBtools calldbtools = new CallDBtools();
            //dbConn = calldbtools.SelectDB("oracle");

            if (paraList != null)
            {
                OracleParameter[] oraParAry = new OracleParameter[paraList.Count];
                for (int i = 0; i < paraList.Count; i++)
                {
                    oraParAry[i] = new OracleParameter(paraList[i].ParaNM, paraList[i].ParaTP);
                    oraParAry[i].Value = paraList[i].ParaVL;
                    cmd.Parameters.Add(oraParAry[i]);
                }
            }
            rowsAffected = ExecSQL(cmd, dbConn, ref dbmsg);
            return rowsAffected;
        }

        //單一SQL回傳單一datatable
        //cmdstr:單一SQL語法、sortstr:排序欄位、paraList:參數NO=1、參數NAME、參數TYPE、參數VALUE、dbNM:資料庫名稱、dsname:data table name
        public DataTable CallOpenSQLReturnDT(string cmdstr, string sortstr, List<OracleParam> paraList, string dbNM, string dsname, ref string dbmsg)
        {
            OracleCommand cmd = new OracleCommand();
            DataTable dt = new DataTable();
            string dbConn = "";

            dbConn = getDBConnStr(); // 排程會放在庫房,連線字串寫在程式內以避免被看到
            //CallDBtools calldbtools = new CallDBtools();
            //dbConn = calldbtools.SelectDB("oracle");

            if (sortstr != null && sortstr != "")
            {
                cmdstr += " order by " + sortstr;
            }
            cmd.CommandText = cmdstr;

            if (paraList != null)
            {
                OracleParameter[] oraParAry = new OracleParameter[paraList.Count];
                for (int i = 0; i < paraList.Count; i++)
                {
                    oraParAry[i] = new OracleParameter(paraList[i].ParaNM, paraList[i].ParaTP);
                    oraParAry[i].Value = paraList[i].ParaVL;
                    cmd.Parameters.Add(oraParAry[i]);
                }
            }

            dt = OpenSQLReturnDT(cmd, dbConn, ref dbmsg);
            dt.TableName = dsname;
            return dt;
        }

        //單一SQL回傳單一datatable
        public DataTable OpenSQLReturnDT(OracleCommand cmd, string connNM, ref string dbmsg)
        {
            OracleConnection conn = new OracleConnection(connNM);
            OracleDataAdapter adp = new OracleDataAdapter();
            DataTable dt = new DataTable();
            try
            {
                if (conn.State == ConnectionState.Open) { conn.Close(); }
                conn.Open();
                cmd.Connection = conn;
                adp.SelectCommand = cmd;
                adp.SelectCommand.CommandTimeout = 900;
                adp.Fill(dt);
                dbmsg = "";
            }
            catch (Exception ex)
            {
                //string parStr = "";
                //for (int i = 0; i < cmd.Parameters.Count; i++)
                //{
                //    parStr += cmd.Parameters[i].ParameterName + " = " + cmd.Parameters[i].Value;
                //}
                //CallDBtools callDBtools = new CallDBtools();
                //callDBtools.WriteExceptionLog(ex.Message, cmd.CommandText.ToString(), parStr);
                dbmsg = ex.Message;
            }
            finally
            {
                cmd.Dispose();
                conn.Close();
                conn.Dispose();
            }
            return dt;
        }

        // 寫 Oracle ERROR_LOG
        public int I_ERROR_LOG(string pg, string msg, string userid)
        {
            int rowsAffected = -1;
            string dbmsg = "error";
            List<OracleParam> paraList = new List<OracleParam>();
            string cmdstr = "insert into ERROR_LOG(LOGTIME,PG,MSG,USERID) ";
            cmdstr += "                    values (sysdate, :pg, :msg, :userid) ";
            paraList.Add(new OracleParam(1, ":pg", "VarChar", pg));
            paraList.Add(new OracleParam(1, ":msg", "VarChar", msg));
            paraList.Add(new OracleParam(1, ":userid", "VarChar", userid));
            rowsAffected = CallExecSQL(cmdstr, paraList, "oracle", ref dbmsg);
            return rowsAffected;
        }
        
    }
}
