using System;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace JCLib.DB.Tool
{
    public class CallDBtools_Oracle
    {
        //單一SQL回傳單一值
        //cmdstr:單一SQL語法、paraList:參數NO=1、參數NAME、參數TYPE、參數VALUE、dbNM:資料庫名稱
        public string CallExecScalar(string cmdstr, List<OracleParam> paraList, string dbNM, ref string dbmsg)
        {
            OracleCommand cmd = new OracleCommand();
            string resultStr = "";
            string dbConn = "";

            CallDBtools calldbtools = new CallDBtools();
            dbConn = calldbtools.SelectDB(dbNM);

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

            DBtools_Oracle dbtools = new DBtools_Oracle();
            resultStr = dbtools.ExecScalar(cmd, dbConn, ref dbmsg);
            return resultStr;
        }

        //單一SQL回傳單一dataset.table
        //cmdstr:單一SQL語法、sortstr:排序欄位、paraList:參數NO=1、參數NAME、參數TYPE、參數VALUE、dbNM:資料庫名稱、dsname:data table name
        public DataSet CallOpenSQL(string cmdstr, string sortstr, List<OracleParam> paraList, string dbNM, string dsname, ref string dbmsg)
        {
            OracleCommand cmd = new OracleCommand();
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
                OracleParameter[] oraParAry = new OracleParameter[paraList.Count];
                for (int i = 0; i < paraList.Count; i++)
                {
                    oraParAry[i] = new OracleParameter(paraList[i].ParaNM, paraList[i].ParaTP);
                    oraParAry[i].Value = paraList[i].ParaVL;
                    cmd.Parameters.Add(oraParAry[i]);
                }
            }

            DBtools_Oracle dbtools = new DBtools_Oracle();
            ds = dbtools.OpenSQL(cmd, dbConn, ref dbmsg);
            if (ds.Tables.Count > 0) ds.Tables[0].TableName = dsname;
            return ds;
        }

        //單一SQL回傳單一datatable
        //cmdstr:單一SQL語法、sortstr:排序欄位、paraList:參數NO=1、參數NAME、參數TYPE、參數VALUE、dbNM:資料庫名稱、dsname:data table name
        public DataTable CallOpenSQLReturnDT(string cmdstr, string sortstr, List<OracleParam> paraList, string dbNM, string dsname, ref string dbmsg)
        {
            OracleCommand cmd = new OracleCommand();
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
                OracleParameter[] oraParAry = new OracleParameter[paraList.Count];
                for (int i = 0; i < paraList.Count; i++)
                {
                    oraParAry[i] = new OracleParameter(paraList[i].ParaNM, paraList[i].ParaTP);
                    oraParAry[i].Value = paraList[i].ParaVL;
                    cmd.Parameters.Add(oraParAry[i]);
                }
            }

            DBtools_Oracle dbtools = new DBtools_Oracle();
            dt = dbtools.OpenSQLReturnDT(cmd, dbConn, ref dbmsg);
            dt.TableName = dsname;
            return dt;
        }

        //1個SQL回傳筆數及分頁查詢結果
        //cmdstr:SQL語法、sortstr:排序欄位、iStart:起始筆數、iEnd:結束筆數、paraList:參數NO、參數NAME、參數TYPE、參數VALUE、dbNM:資料庫名稱、dsname:data table name
        public DataSet CallOpenSQL(string cmdstr, string sortstr, int iStart, int iEnd, List<OracleParam> paraList, string dbNM, List<string> dsname, ref string dbmsg)
        {
            List<OracleCommand> cmdlist = new List<OracleCommand>();
            DataSet ds = new DataSet();
            string dbConn = "";

            CallDBtools calldbtools = new CallDBtools();
            dbConn = calldbtools.SelectDB(dbNM);
            OracleCommand cmd1 = new OracleCommand();
            OracleCommand cmd2 = new OracleCommand();

            cmd1.CommandText = "select count(*) RC from (" + cmdstr + ")";
            cmdlist.Add(cmd1);
            cmd2.CommandText = "select * from (select T.*,ROW_NUMBER() over (order by " + sortstr + ") as RN from (" + cmdstr + ") T ) where RN between " + iStart + " and " + iEnd + " order by RN ";
            cmdlist.Add(cmd2);

            if (paraList != null && paraList.Count > 0)
            {
                int paraListFlag = 0;
                OracleParameter[] oraParAry = new OracleParameter[paraList.Count];
                for (int k = 0; k < paraList.Count; k++)
                {
                    int j = k;
                    oraParAry[k] = new OracleParameter(paraList[paraListFlag].ParaNM.ToString(), paraList[paraListFlag].ParaTP.ToString());
                    oraParAry[k].Value = paraList[paraListFlag].ParaVL.ToString();
                    cmd1.Parameters.Add(oraParAry[k]);
                    oraParAry[j] = new OracleParameter(paraList[paraListFlag].ParaNM.ToString(), paraList[paraListFlag].ParaTP.ToString());
                    oraParAry[j].Value = paraList[paraListFlag].ParaVL.ToString();
                    cmd2.Parameters.Add(oraParAry[j]);
                    paraListFlag++;
                }
            }

            DBtools_Oracle dbtools = new DBtools_Oracle();
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
        public DataSet CallOpenSQL(List<string> cmdstr, List<OracleParam> paraList, string dbNM, List<string> dsname, ref string dbmsg)
        {
            List<OracleCommand> cmdlist = new List<OracleCommand>();
            DataSet ds = new DataSet();
            string dbConn = "";

            if (paraList == null && dbNM != "")
            {
                CallDBtools calldbtools = new CallDBtools();
                dbConn = calldbtools.SelectDB(dbNM);
                foreach (string cmdstrS in cmdstr)
                {
                    OracleCommand cmd = new OracleCommand();
                    cmd.CommandText = cmdstrS;
                    cmdlist.Add(cmd);
                }
                DBtools_Oracle dbtools = new DBtools_Oracle();
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
                    OracleCommand cmd = new OracleCommand();
                    cmd.CommandText = cmdstr[i];
                    cmdlist.Add(cmd);
                    int j = paraList.FindAll(delegate(OracleParam op) { return op.ParaNo == i + 1; }).Count;
                    OracleParameter[] oraParAry = new OracleParameter[j];
                    for (int k = 0; k < j; k++)
                    {
                        oraParAry[k] = new OracleParameter(paraList[paraListFlag].ParaNM.ToString(), paraList[paraListFlag].ParaTP.ToString());
                        oraParAry[k].Value = paraList[paraListFlag].ParaVL.ToString();
                        cmd.Parameters.Add(oraParAry[k]);
                        paraListFlag++;
                    }
                }
                DBtools_Oracle dbtools = new DBtools_Oracle();
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
        public int CallExecSQL(string cmdstr, List<OracleParam> paraList, string dbNM, ref string dbmsg)
        {
            OracleCommand cmd = new OracleCommand(checkSQLcmdstr(cmdstr));
            int rowsAffected = -1;
            string dbConn = "";

            CallDBtools calldbtools = new CallDBtools();
            dbConn = calldbtools.SelectDB(dbNM);

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
            DBtools_Oracle dbtools = new DBtools_Oracle();
            rowsAffected = dbtools.ExecSQL(cmd, dbConn, ref dbmsg);
            return rowsAffected;
        }

        //回傳多個SQL資料庫更動的筆數
        //cmdstr:多個SQL語法、paraList:參數NO、參數NAME、參數TYPE、參數VALUE、dbNM:資料庫名稱
        public int CallExecSQL(List<string> cmdstr, List<OracleParam> paraList, string dbNM, ref string dbmsg)
        {
            List<OracleCommand> cmdlist = new List<OracleCommand>();
            int rowsAffected = -1;
            string dbConn = "";

            if (paraList == null && dbNM != "")
            {
                CallDBtools calldbtools = new CallDBtools();
                dbConn = calldbtools.SelectDB(dbNM);
                foreach (string cmdstrS in cmdstr)
                {
                    OracleCommand cmd = new OracleCommand(checkSQLcmdstr(cmdstrS));
                    cmdlist.Add(cmd);
                }
                DBtools_Oracle dbtools = new DBtools_Oracle();
                rowsAffected = dbtools.ExecSQL(cmdlist, dbConn, ref dbmsg);
            }
            else if (paraList != null && dbNM != "")
            {
                CallDBtools calldbtools = new CallDBtools();
                dbConn = calldbtools.SelectDB(dbNM);
                int paraListFlag = 0;
                for (int i = 0; i < cmdstr.Count; i++)
                {
                    OracleCommand cmd = new OracleCommand(checkSQLcmdstr(cmdstr[i]));
                    cmdlist.Add(cmd);
                    int j = paraList.FindAll(delegate(OracleParam op) { return op.ParaNo == i + 1; }).Count;
                    OracleParameter[] oraParAry = new OracleParameter[j];
                    for (int k = 0; k < j; k++)
                    {
                        oraParAry[k] = new OracleParameter(paraList[paraListFlag].ParaNM.ToString(), paraList[paraListFlag].ParaTP.ToString());
                        oraParAry[k].Value = paraList[paraListFlag].ParaVL.ToString();
                        cmd.Parameters.Add(oraParAry[k]);
                        paraListFlag++;
                    }
                }
                DBtools_Oracle dbtools = new DBtools_Oracle();
                rowsAffected = dbtools.ExecSQL(cmdlist, dbConn, ref dbmsg);
            }
            return rowsAffected;
        }

        //呼叫stored procedure回傳單一dataset.table
        //spname:stored procedure name、paraList:參數NO=1、參數NAME、參數TYPE、參數VALUE、dbNM:資料庫名稱、dsname:data table name
        public DataSet CallExecSP(string spname, List<OracleParam> paraList, string dbNM, string dsname, ref string dbmsg)
        {
            OracleCommand cmd = new OracleCommand();
            DataSet ds = new DataSet();
            string dbConn = "";

            CallDBtools calldbtools = new CallDBtools();
            dbConn = calldbtools.SelectDB(dbNM);
            DBtools_Oracle dbtools = new DBtools_Oracle();
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

        //呼叫stored procedure不回傳值
        //spname:stored procedure name、paraList:參數NO=1、參數NAME、參數TYPE、參數VALUE、dbNM:資料庫名稱、dsname:data table name
        public void CallExecSPWithoutRtn(string spname, List<OracleParam> paraList, string dbNM, ref string dbmsg)
        {
            CallDBtools calldbtools = new CallDBtools();
            string dbConn = calldbtools.SelectDB(dbNM);
            OracleConnection conn = new OracleConnection(dbConn);
            OracleCommand cmd = new OracleCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = spname;
            try
            {
                if (conn.State == ConnectionState.Open) { conn.Close(); }
                conn.Open();
                cmd.Connection = conn;
                if (paraList == null)
                {
                }
                else
                {
                    OracleCommandBuilder.DeriveParameters(cmd);
                    for (int i = 0; i < paraList.Count; i++)
                    {
                        cmd.Parameters[i].Value = paraList[i].ParaVL;
                    }
                }
                cmd.ExecuteNonQuery();
                dbmsg = "";
            }
            catch (Exception ex)
            {
                string parStr = "";
                if (paraList != null)
                {
                    for (int i = 0; i < cmd.Parameters.Count; i++)
                    {
                        parStr += cmd.Parameters[i].ParameterName + " = " + cmd.Parameters[i].Value;
                    }
                }
                CallDBtools callDBtools = new CallDBtools();
                callDBtools.WriteExceptionLog(ex.Message, cmd.CommandText.ToString(), parStr);
                dbmsg = ex.Message;
            }
            finally
            {
                cmd.Dispose();
                conn.Close();
                conn.Dispose();
            }
        } // 

        public String CallExecSpReturnOneVarchar2(
            string spname, 
            string parameterName, 
            List<OracleParam> paraList, 
            string dbNM, 
            ref string dbmsg
        )
        {
            CallDBtools calldbtools = new CallDBtools();
            string dbConn = calldbtools.SelectDB(dbNM);
            OracleConnection conn = new OracleConnection(dbConn);
            OracleCommand cmd = new OracleCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = spname;

            String sRtn = "";
            OracleParameter p = new OracleParameter(parameterName, OracleDbType.Varchar2);
            p.Direction = ParameterDirection.Output;
            p.Size = 4000;
            cmd.Parameters.Add(p);
            try
            {
                if (conn.State == ConnectionState.Open) { conn.Close(); }
                conn.Open();
                cmd.Connection = conn;
                if (paraList == null)
                {
                }
                else
                {
                    OracleCommandBuilder.DeriveParameters(cmd);
                    for (int i = 0; i < paraList.Count; i++)
                    {
                        cmd.Parameters[i].Value = paraList[i].ParaVL;
                    }
                }
                cmd.ExecuteNonQuery();

                sRtn = cmd.Parameters[parameterName].Value.ToString();
                dbmsg = "";
            }
            catch (Exception ex)
            {
                string parStr = "";
                if (paraList != null)
                {
                    for (int i = 0; i < cmd.Parameters.Count; i++)
                    {
                        parStr += cmd.Parameters[i].ParameterName + " = " + cmd.Parameters[i].Value;
                    }
                }
                CallDBtools callDBtools = new CallDBtools();
                callDBtools.WriteExceptionLog(ex.Message, cmd.CommandText.ToString(), parStr);
                dbmsg = ex.Message;
            }
            finally
            {
                cmd.Dispose();
                conn.Close();
                conn.Dispose();
            }
            return sRtn;
        } // 

        public SP CallExecSpInOutParameter(
            string spname,
            SP sp,
            string dbNM,
            ref string dbmsg
        )
        {
            CallDBtools calldbtools = new CallDBtools();
            string dbConn = calldbtools.SelectDB(dbNM);
            OracleConnection conn = new OracleConnection(dbConn);
            OracleCommand cmd = new OracleCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = spname;
            OracleParameter p = new OracleParameter();
            try
            {
                if (conn.State == ConnectionState.Open) { conn.Close(); }
                conn.Open();
                cmd.Connection = conn;

                if (sp != null)
                {
                    foreach (SpItem i in sp.items)
                    {
                        if (i.type.ToLower().Equals("varchar2"))
                        {
                            p = new OracleParameter(i.name, OracleDbType.Varchar2);
                            p.Size = 4000;
                        }
                        else if (i.type.ToLower().Equals("number"))
                        {
                            p = new OracleParameter(i.name, OracleDbType.Int32);
                        }
                        if (!String.IsNullOrEmpty(i.value))
                            p.Value = i.value;
                        if (i.direction.ToLower().Equals("in"))
                        {
                        }
                        else if (i.direction.ToLower().Equals("out"))
                        {
                            p.Direction = ParameterDirection.Output;
                        }
                        cmd.Parameters.Add(p);
                    }
                }
                cmd.ExecuteNonQuery();

                foreach (SpItem i in sp.items)
                {
                    i.value = "";
                    if (i.direction.ToLower().Equals("out"))
                    {
                        i.value = cmd.Parameters[i.name].Value.ToString();
                    }
                }
                dbmsg = "";
            }
            catch (Exception ex)
            {
                string parStr = "";
                foreach (SpItem i in sp.items)
                {
                    parStr += i.name + "=" + i.value + "\r\n";
                    cmd.Parameters.Add(p);
                }
                CallDBtools callDBtools = new CallDBtools();
                callDBtools.WriteExceptionLog(ex.Message, cmd.CommandText.ToString(), parStr);
                dbmsg = ex.Message;
            }
            finally
            {
                cmd.Dispose();
                conn.Close();
                conn.Dispose();
            }
            return sp;
        } // 

        //呼叫stored procedure回傳 output parameter 轉成 DataSet
        public DataSet CallExecSP_output(string spname, List<OracleParam> paraList, string dbNM, string dsname, ref string dbmsg)
        {
            OracleCommand cmd = new OracleCommand();
            DataSet ds = new DataSet();
            string dbConn = "";

            CallDBtools calldbtools = new CallDBtools();
            dbConn = calldbtools.SelectDB(dbNM);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = spname;
            DBtools_Oracle dbtools = new DBtools_Oracle();
            ds = dbtools.ExecSP_output(cmd, paraList, dbConn, ref dbmsg);
            if (ds.Tables.Count > 0) ds.Tables[0].TableName = dsname;
            return ds;
        }
        
        //包Transaction 單一SQL回傳單一值
        public string CallExecScalarByTransaction(string cmdstr, List<OracleParam> paraList)
        {
            OracleCommand cmd = new OracleCommand();
            string resultStr = "";

            DBtools_Oracle dbtools = new DBtools_Oracle();
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

            resultStr = dbtools.ExecScalarByTransaction(cmd);
            return resultStr;
        }

        //包Transaction 單一SQL回傳單一dataset.table
        public DataSet CallOpenSQLByTransaction(string cmdstr, string sortstr, List<OracleParam> paraList)
        {
            OracleCommand cmd = new OracleCommand();
            DataSet ds = new DataSet();

            DBtools_Oracle dbtools = new DBtools_Oracle();

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

            ds = dbtools.OpenSQLByTransaction(cmd);
            return ds;
        }

        //包Transaction 回傳單一SQL資料庫更動的筆數
        public int CallExecSQLByTransaction(string cmdstr, List<OracleParam> paraList, OracleTransaction transaction)
        {
            OracleCommand cmd = new OracleCommand();
            int rowsAffected = -1;

            DBtools_Oracle dbtools = new DBtools_Oracle();
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
            rowsAffected = dbtools.ExecSQLByTransaction(cmd, transaction);
            return rowsAffected;
        }

        //包Transaction 回傳多個SQL資料庫更動的筆數
        public int CallExecSQLByTransaction(List<string> cmdstr, List<OracleParam> paraList, OracleTransaction transaction)
        {
            List<OracleCommand> cmdlist = new List<OracleCommand>();
            int rowsAffected = -1;
            DBtools_Oracle dbtools = new DBtools_Oracle();
            if (paraList == null)
            {
                foreach (string cmdstrS in cmdstr)
                {
                    OracleCommand cmd = new OracleCommand();
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
                    OracleCommand cmd = new OracleCommand();
                    cmd.CommandText = cmdstr[i];
                    cmdlist.Add(cmd);
                    int j = paraList.FindAll(delegate(OracleParam op) { return op.ParaNo == i + 1; }).Count;
                    OracleParameter[] oraParAry = new OracleParameter[j];
                    for (int k = 0; k < j; k++)
                    {
                        oraParAry[k] = new OracleParameter(paraList[paraListFlag].ParaNM.ToString(), paraList[paraListFlag].ParaTP.ToString());
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
        public int CallExecSQLByTransaction(List<string> cmdstr, List<OracleParam> paraList, OracleTransaction transaction, OracleConnection conn_oracle)
        {
            List<OracleCommand> cmdlist = new List<OracleCommand>();
            int rowsAffected = -1;
            DBtools_Oracle dbtools = new DBtools_Oracle();
            if (paraList == null)
            {
                foreach (string cmdstrS in cmdstr)
                {
                    OracleCommand cmd = new OracleCommand();
                    cmd.Connection = conn_oracle;
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
                    OracleCommand cmd = new OracleCommand();
                    cmd.Connection = conn_oracle;
                    cmd.CommandText = cmdstr[i];
                    cmdlist.Add(cmd);
                    int j = paraList.FindAll(delegate (OracleParam op) { return op.ParaNo == i + 1; }).Count;
                    OracleParameter[] oraParAry = new OracleParameter[j];
                    for (int k = 0; k < j; k++)
                    {
                        oraParAry[k] = new OracleParameter(paraList[paraListFlag].ParaNM.ToString(), paraList[paraListFlag].ParaTP.ToString());
                        oraParAry[k].Value = paraList[paraListFlag].ParaVL.ToString();
                        cmd.Parameters.Add(oraParAry[k]);
                        paraListFlag++;
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

        // 寫 Oracle ERROR_LOG
        public int I_ERROR_LOG(string pg, string msg, string userid)
        {
            int rowsAffected = -1;
            CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
            string dbmsg = "error";
            List<CallDBtools_Oracle.OracleParam> paraList = new List<CallDBtools_Oracle.OracleParam>();
            string cmdstr = "insert into ERROR_LOG(LOGTIME,PG,MSG,USERID) ";
            cmdstr += "                    values (sysdate, :pg, :msg, :userid) ";
            paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":pg", "VarChar", pg));
            paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":msg", "VarChar", msg));
            paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":userid", "VarChar", userid));
            rowsAffected = callDbtools_oralce.CallExecSQL(cmdstr, paraList, "oracle", ref dbmsg);
            return rowsAffected;
        }

    } // ec
    public class SP
    {
        public List<SpItem> items = new List<SpItem>();
    } // ec
    public class SpItem
    {
        public String name;
        public String direction;
        public String type;
        public String value;
    } // ec
} // en