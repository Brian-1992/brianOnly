using System;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace JCLib.DB.Tool
{
    public class DBtools_Oracle
    {
        //單一SQL回傳單一值
        public string ExecScalar(OracleCommand cmd, string connNM, ref string dbmsg)
        {
            OracleConnection conn = new OracleConnection(connNM);
            string resultStr="";
            DataSet ds = new DataSet();
            try
            {
                if (conn.State == ConnectionState.Open) { conn.Close(); }
                conn.Open();
                cmd.Connection = conn;
                var tmpResult = cmd.ExecuteScalar();
                if (tmpResult != null) { resultStr = tmpResult.ToString(); }
                dbmsg = "";
            }
            catch (Exception ex)
            {
                string parStr = "";
                for (int i = 0; i < cmd.Parameters.Count; i++)
                {
                    parStr += cmd.Parameters[i].ParameterName + " = " + cmd.Parameters[i].Value;
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
            return resultStr;
        }

        //單一SQL回傳單一dataset.table
        public DataSet OpenSQL(OracleCommand cmd, string connNM, ref string dbmsg)
        {
            OracleConnection conn = new OracleConnection(connNM);
            OracleDataAdapter adp = new OracleDataAdapter();
            DataSet ds = new DataSet();
            try
            {
                if (conn.State == ConnectionState.Open) { conn.Close(); }
                conn.Open();
                cmd.Connection = conn;
                adp.SelectCommand = cmd;
                adp.SelectCommand.CommandTimeout = 900;
                adp.Fill(ds);
                dbmsg = "";
            }
            catch (Exception ex)
            {
                string parStr = "";
                for (int i = 0; i < cmd.Parameters.Count; i++)
                {
                    parStr += cmd.Parameters[i].ParameterName + " = " + cmd.Parameters[i].Value;
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
            return ds;
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
                string parStr = "";
                for (int i = 0; i < cmd.Parameters.Count; i++)
                {
                    parStr += cmd.Parameters[i].ParameterName + " = " + cmd.Parameters[i].Value;
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
            return dt;
        }

        //多個SQL回傳單一dataset多個table
        public DataSet OpenSQL(List<OracleCommand> cmd, string connNM, ref string dbmsg)
        {
            OracleDataAdapter adp = new OracleDataAdapter();
            DataSet ds = new DataSet();
            OracleCommand exeCmd = new OracleCommand();
            using (OracleConnection conn = new OracleConnection(connNM))
                {
                    try
                    {
                        if (conn.State == ConnectionState.Open) { conn.Close(); }
                        conn.Open();
                        foreach (OracleCommand dbcommand in cmd)
                        {
                            exeCmd = dbcommand;
                            dbcommand.Connection = conn;
                            adp.SelectCommand = dbcommand;
                            adp.Fill(ds.Tables.Add());
                            dbmsg = "";
                        }
                    }
                    catch (Exception ex)
                    {
                        string parStr = "";
                        for (int i = 0; i < exeCmd.Parameters.Count; i++)
                        {
                            parStr += exeCmd.Parameters[i].ParameterName + " = " + exeCmd.Parameters[i].Value;
                        }
                        CallDBtools callDBtools = new CallDBtools();
                        callDBtools.WriteExceptionLog(ex.Message, exeCmd.CommandText.ToString(), parStr);
                        dbmsg = ex.Message;
                    }
                    finally
                    {
                        exeCmd.Dispose();
                        conn.Close();
                        conn.Dispose();
                    }
                }
            return ds;
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
                    string parStr = "";
                    for (int i = 0; i < cmd.Parameters.Count; i++)
                    {
                        parStr += cmd.Parameters[i].ParameterName + " = " + cmd.Parameters[i].Value;
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
            return rowsAffected;
        }

        //回傳多個SQL資料庫更動的筆數
        public int ExecSQL(List<OracleCommand> cmd, string connNM, ref string dbmsg)
        {
            OracleTransaction transaction;
            int rowsAffected = 0;
            OracleCommand exeCmd = new OracleCommand();
            try
            {
                using (OracleConnection conn = new OracleConnection(connNM))
                {
                    if (conn.State == ConnectionState.Open) { conn.Close(); }
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    try
                    {
                        foreach (OracleCommand dbcommand in cmd)
                        {
                            exeCmd = dbcommand;
                            dbcommand.Connection = conn;
                            dbcommand.Transaction = transaction;
                            rowsAffected += dbcommand.ExecuteNonQuery();
                        }
                        transaction.Commit();
                        dbmsg = "";
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        rowsAffected = -1;
                        string parStr = "";
                        for (int i = 0; i < exeCmd.Parameters.Count; i++)
                        {
                            parStr += exeCmd.Parameters[i].ParameterName + " = " + exeCmd.Parameters[i].Value;
                        }
                        CallDBtools callDBtools = new CallDBtools();
                        callDBtools.WriteExceptionLog(ex.Message, exeCmd.CommandText.ToString(), parStr);
                        dbmsg = ex.Message;
                    }
                    finally 
                    {
                        exeCmd.Dispose();
                        conn.Close();
                        conn.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                rowsAffected = -1;
                string parStr = "";
                for (int i = 0; i < exeCmd.Parameters.Count; i++)
                {
                    parStr += exeCmd.Parameters[i].ParameterName + " = " + exeCmd.Parameters[i].Value;
                }
                CallDBtools callDBtools = new CallDBtools();
                callDBtools.WriteExceptionLog(ex.Message, exeCmd.CommandText.ToString(), parStr);
            }
            return rowsAffected;
        }

        //呼叫stored procedure回傳單一dataset.table
        public DataSet ExecSP(OracleCommand cmd, List<CallDBtools_Oracle.OracleParam> paraList, string connNM, ref string dbmsg)
        {
            OracleConnection conn = new OracleConnection(connNM);
            DataSet ds = new DataSet();
            DataTable dt = ds.Tables.Add();
            try
            {
                if (conn.State == ConnectionState.Open) { conn.Close(); }
                conn.Open();
                cmd.Connection = conn;
                if (paraList == null) {
                }
                else {
                    OracleCommandBuilder.DeriveParameters(cmd);
                    for (int i = 0; i < paraList.Count; i++) {
                        cmd.Parameters[i].Value = paraList[i].ParaVL;
                    }
                }
                OracleDataReader dr = cmd.ExecuteReader();
                if(dr != null)
                    dt.Load(dr);
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
            return ds;
        }
        
        //呼叫stored procedure回傳 output parameter 轉成 DataSet
        public DataSet ExecSP_output(OracleCommand cmd, List<CallDBtools_Oracle.OracleParam> paraList, string connNM, ref string dbmsg)
        {
            OracleConnection conn = new OracleConnection(connNM);
            DataSet ds = new DataSet();
            ds.Tables.Add("T1");
            DataRow dr = ds.Tables["T1"].NewRow();
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
                        if (paraList[i].ParaVL.Contains("Output"))   ds.Tables["T1"].Columns.Add(paraList[i].ParaNM);
                    }
                }
                cmd.ExecuteNonQuery();
                for (int i = 0; i < paraList.Count; i++)
                {
                    if (paraList[i].ParaVL.Contains("Output"))
                    {
                        for (int j = 0; j < cmd.Parameters.Count; j++)  //只撈 Output欄位
                        {
                            if (paraList[i].ParaNM == cmd.Parameters[j].ToString())
                                dr[paraList[i].ParaNM] = cmd.Parameters[j].Value.ToString();
                        }
                    }
                }
                ds.Tables["T1"].Rows.Add(dr);  
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
            return ds;
        }
        
        //包Transaction 單一SQL回傳單一值
        public string ExecScalarByTransaction(OracleCommand cmd)
        {
            string resultStr = "";
            if (cmd.ExecuteScalar() != null) { resultStr = cmd.ExecuteScalar().ToString(); }
            return resultStr;
        }

        //包Transaction 單一SQL回傳單一dataset.table
        public DataSet OpenSQLByTransaction(OracleCommand cmd) 
        {
            OracleDataAdapter adp = new OracleDataAdapter();
            DataSet ds1 = new DataSet();
            adp.SelectCommand = cmd;
            adp.Fill(ds1);
            return ds1;
        }

        //包Transaction 回傳單一SQL資料庫更動的筆數
        public int ExecSQLByTransaction(OracleCommand cmd, OracleTransaction transaction)
        {
            int rowsAffected = -1;
            cmd.Transaction = transaction;
            rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected;
        }

        L l = new L("JCLib.DB.Tool.DBtools_Oracle");
        String getDebugSql(OracleCommand cmd)
        {
            String sql = cmd.CommandText;
            foreach (OracleParameter p in cmd.Parameters)
            {
                sql = sql.Replace(p.ParameterName, "'" + p.Value + "'");
            }
            return sql;
        }

        //包Transaction 回傳多個SQL資料庫更動的筆數
        public int ExecSQLByTransaction(List<OracleCommand> cmd, OracleTransaction transaction)
        {
            int rowsAffected = 0;
            int idx = 0;
            //l.lg("ExecSQLByTransaction()", idx.ToString() + "\r\n--start -------------------------------------");
            foreach (OracleCommand dbcommand in cmd)
            {
                dbcommand.Transaction = transaction;
                //l.lg("ExecSQLByTransaction()", idx++.ToString() + "\r\n" + getDebugSql(dbcommand));
                rowsAffected += dbcommand.ExecuteNonQuery();
            }
            //l.lg("ExecSQLByTransaction()", idx.ToString() + "\r\n--end -------------------------------------");
            return rowsAffected;
        }

    }
}