using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Data.SqlClient;

namespace JCLib.DB.Tool
{
    public class CallDBtools_MSDb
    {
        //單一SQL回傳單一值
        //cmdstr:單一SQL語法、paraList:參數NO=1、參數NAME、參數TYPE、參數VALUE、dbNM:資料庫名稱
        public string CallExecScalar(string cmdstr, List<MSDbParam> paraList, string dbNM, ref string dbmsg)
        {
            SqlCommand cmd = new SqlCommand();
            string resultStr = "";
            string dbConn = "";

            CallDBtools calldbtools = new CallDBtools();
            dbConn = calldbtools.SelectDB(dbNM);

            cmd.CommandText = cmdstr;

            if (paraList != null)
            {
                SqlParameter[] oraParAry = new SqlParameter[paraList.Count];
                for (int i = 0; i < paraList.Count; i++)
                {
                    oraParAry[i] = new SqlParameter(paraList[i].ParaNM, paraList[i].ParaTP);
                    oraParAry[i].Value = paraList[i].ParaVL;
                    cmd.Parameters.Add(oraParAry[i]);
                }
            }

            DBtools_MSDb dbtools = new DBtools_MSDb();
            resultStr = dbtools.ExecScalar(cmd, dbConn, ref dbmsg);
            return resultStr;
        }

        //單一SQL回傳單一dataset.table
        //cmdstr:單一SQL語法、sortstr:排序欄位、paraList:參數NO=1、參數NAME、參數TYPE、參數VALUE、dbNM:資料庫名稱、dsname:data table name
        public DataSet CallOpenSQL(string cmdstr, string sortstr, List<MSDbParam> paraList, string dbNM, string dsname, ref string dbmsg)
        {
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            string dbConn = "";

            CallDBtools calldbtools = new CallDBtools();
            dbConn = calldbtools.SelectDB(dbNM);

            if (sortstr != null && sortstr != "")
            {
                cmdstr += " order by " + sortstr;
            }
            cmd.CommandText = cmdstr;

            if (paraList != null)
            {
                SqlParameter[] oraParAry = new SqlParameter[paraList.Count];
                for (int i = 0; i < paraList.Count; i++)
                {
                    oraParAry[i] = new SqlParameter(paraList[i].ParaNM, paraList[i].ParaTP);
                    oraParAry[i].Value = paraList[i].ParaVL;
                    cmd.Parameters.Add(oraParAry[i]);
                }
            }

            DBtools_MSDb dbtools = new DBtools_MSDb();
            ds = dbtools.OpenSQL(cmd, dbConn, ref dbmsg);
            if (ds.Tables.Count > 0) ds.Tables[0].TableName = dsname;
            return ds;
        }

        //單一SQL回傳單一datatable
        //cmdstr:單一SQL語法、sortstr:排序欄位、paraList:參數NO=1、參數NAME、參數TYPE、參數VALUE、dbNM:資料庫名稱、dsname:data table name
        public DataTable CallOpenSQLReturnDT(string cmdstr, string sortstr, List<MSDbParam> paraList, string dbNM, string dsname, ref string dbmsg)
        {
            SqlCommand cmd = new SqlCommand();
            DataTable dt = new DataTable();
            string dbConn = "";

            CallDBtools calldbtools = new CallDBtools();
            dbConn = calldbtools.SelectDB(dbNM);

            if (sortstr != null && sortstr != "")
            {
                cmdstr += " order by " + sortstr;
            }
            cmd.CommandText = checkSQLcmdstr(cmdstr);

            if (paraList != null)
            {
                SqlParameter[] oraParAry = new SqlParameter[paraList.Count];
                for (int i = 0; i < paraList.Count; i++)
                {
                    oraParAry[i] = new SqlParameter(paraList[i].ParaNM, paraList[i].ParaTP);
                    oraParAry[i].Value = paraList[i].ParaVL;
                    cmd.Parameters.Add(oraParAry[i]);
                }
            }

            DBtools_MSDb dbtools = new DBtools_MSDb();
            dt = dbtools.OpenSQLReturnDT(cmd, dbConn, ref dbmsg);
            dt.TableName = dsname;
            return dt;
        }

        //1個SQL回傳筆數及分頁查詢結果
        //cmdstr:SQL語法、sortstr:排序欄位、iStart:起始筆數、iEnd:結束筆數、paraList:參數NO、參數NAME、參數TYPE、參數VALUE、dbNM:資料庫名稱、dsname:data table name
        public DataSet CallOpenSQL(string cmdstr, string sortstr, int iStart, int iEnd, List<MSDbParam> paraList, string dbNM, List<string> dsname, ref string dbmsg)
        {
            List<SqlCommand> cmdlist = new List<SqlCommand>();
            DataSet ds = new DataSet();
            string dbConn = "";

            CallDBtools calldbtools = new CallDBtools();
            dbConn = calldbtools.SelectDB(dbNM);
            SqlCommand cmd1 = new SqlCommand();
            SqlCommand cmd2 = new SqlCommand();

            cmd1.CommandText = "select count(*) RC from (" + cmdstr + ")";
            cmdlist.Add(cmd1);
            cmd2.CommandText = "select * from (select T.*,ROW_NUMBER() over (order by " + sortstr + ") as RN from (" + cmdstr + ") T ) where RN between " + iStart + " and " + iEnd + " order by RN ";
            cmdlist.Add(cmd2);

            if (paraList != null && paraList.Count > 0)
            {
                int paraListFlag = 0;
                SqlParameter[] oraParAry = new SqlParameter[paraList.Count];
                for (int k = 0; k < paraList.Count; k++)
                {
                    int j = k;
                    oraParAry[k] = new SqlParameter(paraList[paraListFlag].ParaNM.ToString(), paraList[paraListFlag].ParaTP.ToString());
                    oraParAry[k].Value = paraList[paraListFlag].ParaVL.ToString();
                    cmd1.Parameters.Add(oraParAry[k]);
                    oraParAry[j] = new SqlParameter(paraList[paraListFlag].ParaNM.ToString(), paraList[paraListFlag].ParaTP.ToString());
                    oraParAry[j].Value = paraList[paraListFlag].ParaVL.ToString();
                    cmd2.Parameters.Add(oraParAry[j]);
                    paraListFlag++;
                }
            }

            DBtools_MSDb dbtools = new DBtools_MSDb();
            ds = dbtools.OpenSQL(cmdlist, dbConn, ref dbmsg);
            if (ds.Tables.Count > 0)
            {
                for (int i = 0; i < dsname.Count; i++)
                {
                    ds.Tables[i].TableName = dsname[i].ToString();
                }
            }
            return ds;
        }

        //多個SQL回傳單一dataset多個table
        //cmdstr:多個SQL語法、paraList:參數NO、參數NAME、參數TYPE、參數VALUE、dbNM:資料庫名稱、dsname:data table name
        public DataSet CallOpenSQL(List<string> cmdstr, List<MSDbParam> paraList, string dbNM, List<string> dsname, ref string dbmsg)
        {
            List<SqlCommand> cmdlist = new List<SqlCommand>();
            DataSet ds = new DataSet();
            string dbConn = "";

            if (paraList == null && dbNM != "")
            {
                CallDBtools calldbtools = new CallDBtools();
                dbConn = calldbtools.SelectDB(dbNM);
                foreach (string cmdstrS in cmdstr)
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = cmdstrS;
                    cmdlist.Add(cmd);
                }
                DBtools_MSDb dbtools = new DBtools_MSDb();
                ds = dbtools.OpenSQL(cmdlist, dbConn, ref dbmsg);
                if (ds.Tables.Count > 0)
                {
                    for (int i = 0; i < dsname.Count; i++)
                    {
                        ds.Tables[i].TableName = dsname[i].ToString();
                    }
                }
            }
            else if (paraList != null && dbNM != "")
            {
                CallDBtools calldbtools = new CallDBtools();
                dbConn = calldbtools.SelectDB(dbNM);
                int paraListFlag = 0;
                for (int i = 0; i < cmdstr.Count; i++)
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = cmdstr[i];
                    cmdlist.Add(cmd);
                    int j = paraList.FindAll(delegate(MSDbParam op) { return op.ParaNo == i + 1; }).Count;
                    SqlParameter[] oraParAry = new SqlParameter[j];
                    for (int k = 0; k < j; k++)
                    {
                        oraParAry[k] = new SqlParameter(paraList[paraListFlag].ParaNM.ToString(), paraList[paraListFlag].ParaTP.ToString());
                        oraParAry[k].Value = paraList[paraListFlag].ParaVL.ToString();
                        cmd.Parameters.Add(oraParAry[k]);
                        paraListFlag++;
                    }
                }
                DBtools_MSDb dbtools = new DBtools_MSDb();
                ds = dbtools.OpenSQL(cmdlist, dbConn, ref dbmsg);
                if (ds.Tables.Count > 0)
                {
                    for (int i = 0; i < dsname.Count; i++)
                    {
                        ds.Tables[i].TableName = dsname[i].ToString();
                    }
                }
            }
            return ds;
        }

        //回傳單一SQL資料庫更動的筆數
        //cmdstr:單一SQL語法、paraList:參數NO=1、參數NAME、參數TYPE、參數VALUE、dbNM:資料庫名稱
        public int CallExecSQL(string cmdstr, List<MSDbParam> paraList, string dbNM, ref string dbmsg)
        {
            SqlCommand cmd = new SqlCommand(checkSQLcmdstr(cmdstr));
            int rowsAffected = -1;
            string dbConn = "";

            CallDBtools calldbtools = new CallDBtools();
            dbConn = calldbtools.SelectDB(dbNM);

            if (paraList != null)
            {
                SqlParameter[] oraParAry = new SqlParameter[paraList.Count];
                for (int i = 0; i < paraList.Count; i++)
                {
                    oraParAry[i] = new SqlParameter(paraList[i].ParaNM, paraList[i].ParaTP);
                    oraParAry[i].Value = paraList[i].ParaVL;
                    cmd.Parameters.Add(oraParAry[i]);
                }
            }
            DBtools_MSDb dbtools = new DBtools_MSDb();
            rowsAffected = dbtools.ExecSQL(cmd, dbConn, ref dbmsg);
            return rowsAffected;
        }

        //回傳多個SQL資料庫更動的筆數
        //cmdstr:多個SQL語法、paraList:參數NO、參數NAME、參數TYPE、參數VALUE、dbNM:資料庫名稱
        public int CallExecSQL(List<string> cmdstr, List<MSDbParam> paraList, string dbNM, ref string dbmsg)
        {
            List<SqlCommand> cmdlist = new List<SqlCommand>();
            int rowsAffected = -1;
            string dbConn = "";

            if (paraList == null && dbNM != "")
            {
                CallDBtools calldbtools = new CallDBtools();
                dbConn = calldbtools.SelectDB(dbNM);
                foreach (string cmdstrS in cmdstr)
                {
                    SqlCommand cmd = new SqlCommand(checkSQLcmdstr(cmdstrS));
                    cmdlist.Add(cmd);
                }
                DBtools_MSDb dbtools = new DBtools_MSDb();
                rowsAffected = dbtools.ExecSQL(cmdlist, dbConn, ref dbmsg);
            }
            else if (paraList != null && dbNM != "")
            {
                CallDBtools calldbtools = new CallDBtools();
                dbConn = calldbtools.SelectDB(dbNM);
                int paraListFlag = 0;
                for (int i = 0; i < cmdstr.Count; i++)
                {
                    SqlCommand cmd = new SqlCommand(checkSQLcmdstr(cmdstr[i]));
                    cmdlist.Add(cmd);
                    int j = paraList.FindAll(delegate(MSDbParam op) { return op.ParaNo == i + 1; }).Count;
                    SqlParameter[] oraParAry = new SqlParameter[j];
                    for (int k = 0; k < j; k++)
                    {
                        oraParAry[k] = new SqlParameter(paraList[paraListFlag].ParaNM.ToString(), paraList[paraListFlag].ParaTP.ToString());
                        oraParAry[k].Value = paraList[paraListFlag].ParaVL.ToString();
                        cmd.Parameters.Add(oraParAry[k]);
                        paraListFlag++;
                    }
                }
                DBtools_MSDb dbtools = new DBtools_MSDb();
                rowsAffected = dbtools.ExecSQL(cmdlist, dbConn, ref dbmsg);
            }
            return rowsAffected;
        }

        //呼叫stored procedure回傳單一dataset.table
        //spname:stored procedure name、paraList:參數NO=1、參數NAME、參數TYPE、參數VALUE、dbNM:資料庫名稱、dsname:data table name
        public DataSet CallExecSP(string spname, List<MSDbParam> paraList, string dbNM, string dsname, ref string dbmsg)
        {
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            string dbConn = "";

            CallDBtools calldbtools = new CallDBtools();
            dbConn = calldbtools.SelectDB(dbNM);
            DBtools_MSDb dbtools = new DBtools_MSDb();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = spname;

            if (paraList == null)
            {
                ds = dbtools.ExecSP(cmd, null, dbConn, ref dbmsg);
            }
            else if (paraList != null)
            {
                ds = dbtools.ExecSP(cmd, paraList, dbConn, ref dbmsg);
            }
            if (ds.Tables.Count > 0) ds.Tables[0].TableName = dsname;
            return ds;
        }
        //呼叫stored procedure回傳 output parameter 轉成 DataSet
        public DataSet CallExecSP_output(string spname, List<MSDbParam> paraList, string dbNM, string dsname, ref string dbmsg)
        {
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            string dbConn = "";

            CallDBtools calldbtools = new CallDBtools();
            dbConn = calldbtools.SelectDB(dbNM);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = spname;
            DBtools_MSDb dbtools = new DBtools_MSDb();
            ds = dbtools.ExecSP_output(cmd, paraList, dbConn, ref dbmsg);
            if (ds.Tables.Count > 0) ds.Tables[0].TableName = dsname;
            return ds;
        }
        //包Transaction 單一SQL回傳單一值
        public string CallExecScalarByTransaction(string cmdstr, List<MSDbParam> paraList)
        {
            SqlCommand cmd = new SqlCommand();
            string resultStr = "";

            DBtools_MSDb dbtools = new DBtools_MSDb();
            cmd.CommandText = cmdstr;

            if (paraList != null)
            {
                SqlParameter[] oraParAry = new SqlParameter[paraList.Count];
                for (int i = 0; i < paraList.Count; i++)
                {
                    oraParAry[i] = new SqlParameter(paraList[i].ParaNM, paraList[i].ParaTP);
                    oraParAry[i].Value = paraList[i].ParaVL;
                    cmd.Parameters.Add(oraParAry[i]);
                }
            }

            resultStr = dbtools.ExecScalarByTransaction(cmd);
            return resultStr;
        }

        //包Transaction 單一SQL回傳單一dataset.table
        public DataSet CallOpenSQLByTransaction(string cmdstr, string sortstr, List<MSDbParam> paraList)
        {
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();

            DBtools_MSDb dbtools = new DBtools_MSDb();

            if (sortstr != null && sortstr != "")
            {
                cmdstr += " order by " + sortstr;
            }
            cmd.CommandText = cmdstr;

            if (paraList != null)
            {
                SqlParameter[] oraParAry = new SqlParameter[paraList.Count];
                for (int i = 0; i < paraList.Count; i++)
                {
                    oraParAry[i] = new SqlParameter(paraList[i].ParaNM, paraList[i].ParaTP);
                    oraParAry[i].Value = paraList[i].ParaVL;
                    cmd.Parameters.Add(oraParAry[i]);
                }
            }

            ds = dbtools.OpenSQLByTransaction(cmd);
            return ds;
        }

        //包Transaction 回傳單一SQL資料庫更動的筆數
        public int CallExecSQLByTransaction(string cmdstr, List<MSDbParam> paraList, SqlTransaction transaction)
        {
            SqlCommand cmd = new SqlCommand();
            int rowsAffected = -1;

            DBtools_MSDb dbtools = new DBtools_MSDb();
            cmd.CommandText = cmdstr;

            if (paraList != null)
            {
                SqlParameter[] oraParAry = new SqlParameter[paraList.Count];
                for (int i = 0; i < paraList.Count; i++)
                {
                    oraParAry[i] = new SqlParameter(paraList[i].ParaNM, paraList[i].ParaTP);
                    oraParAry[i].Value = paraList[i].ParaVL;
                    cmd.Parameters.Add(oraParAry[i]);
                }
            }
            rowsAffected = dbtools.ExecSQLByTransaction(cmd, transaction);
            return rowsAffected;
        }

        //包Transaction 回傳多個SQL資料庫更動的筆數
        public int CallExecSQLByTransaction(List<string> cmdstr, List<MSDbParam> paraList, SqlTransaction transaction)
        {
            List<SqlCommand> cmdlist = new List<SqlCommand>();
            int rowsAffected = -1;
            DBtools_MSDb dbtools = new DBtools_MSDb();
            if (paraList == null)
            {
                foreach (string cmdstrS in cmdstr)
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = cmdstrS;
                    cmdlist.Add(cmd);
                }
                rowsAffected = dbtools.ExecSQLByTransaction(cmdlist, transaction);
            }
            else if (paraList != null)
            {
                int paraListFlag = 0;
                for (int i = 0; i < cmdstr.Count; i++)
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = cmdstr[i];
                    cmdlist.Add(cmd);
                    int j = paraList.FindAll(delegate(MSDbParam op) { return op.ParaNo == i + 1; }).Count;
                    SqlParameter[] oraParAry = new SqlParameter[j];
                    for (int k = 0; k < j; k++)
                    {
                        oraParAry[k] = new SqlParameter(paraList[paraListFlag].ParaNM.ToString(), paraList[paraListFlag].ParaTP.ToString());
                        oraParAry[k].Value = paraList[paraListFlag].ParaVL.ToString();
                        cmd.Parameters.Add(oraParAry[k]);
                        paraListFlag++;
                    }
                }
                rowsAffected = dbtools.ExecSQLByTransaction(cmdlist, transaction);
            }
            return rowsAffected;
        }

        //包Transaction 回傳多個SQL資料庫更動的筆數
        public int CallExecSQLByTransaction(List<string> cmdstr, List<MSDbParam> paraList, SqlTransaction transaction, SqlConnection conn_msdb)
        {
            List<SqlCommand> cmdlist = new List<SqlCommand>();
            int rowsAffected = -1;
            DBtools_MSDb dbtools = new DBtools_MSDb();
            if (paraList == null)
            {
                foreach (string cmdstrS in cmdstr)
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn_msdb;
                    cmd.CommandText = cmdstrS;
                    cmdlist.Add(cmd);
                }
                rowsAffected = dbtools.ExecSQLByTransaction(cmdlist, transaction);
            }
            else if (paraList != null)
            {
                int paraListFlag = 0;
                for (int i = 0; i < cmdstr.Count; i++)
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn_msdb;
                    cmd.CommandText = cmdstr[i];
                    cmdlist.Add(cmd);
                    int j = paraList.FindAll(delegate (MSDbParam op) { return op.ParaNo == i + 1; }).Count; // 找出符合TransSeq的有多少個參數
                    SqlParameter[] oraParAry = new SqlParameter[j];
                    for (int k = 0; k < j; k++) // 把TransSeq下的參數一個一個對上去
                    {
                        String name = paraList[paraListFlag].ParaNM.ToString();
                        String type = paraList[paraListFlag].ParaTP.ToString();
                        String val = paraList[paraListFlag].ParaVL.ToString();
                        oraParAry[k] = new SqlParameter(
                            name, // 參數名稱
                            type  // 參數型態
                        );
                        oraParAry[k].Value = val;
                        if (name.Length > 0 && type.Length > 0)
                        {
                            cmd.Parameters.Add(oraParAry[k]);
                            paraListFlag++;
                        }
                    }
                }
                rowsAffected = dbtools.ExecSQLByTransaction(cmdlist, transaction);
            }
            return rowsAffected;
        }
        public string checkSQLcmdstr(string cmdstr)
        {
            if (cmdstr.Trim().Contains("--"))
                cmdstr = cmdstr.Replace("--", "");
            if (cmdstr.Trim().Contains("//"))
                cmdstr = cmdstr.Replace("//", "");
            return cmdstr;
        }

        public class MSDbParam
        {
            public int ParaNo { set; get; }
            public string ParaNM { set; get; }
            public string ParaTP { set; get; }
            public object ParaVL { set; get; }

            public MSDbParam(int paraNo, string paraNM, string paraTP, object paraVL)
            {
                this.ParaNo = paraNo;
                this.ParaNM = paraNM;
                this.ParaTP = paraTP;
                this.ParaVL = paraVL;
            }

        }

    }
}