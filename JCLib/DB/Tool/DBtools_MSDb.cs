using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Data.SqlClient;

namespace JCLib.DB.Tool
{
    public class DBtools_MSDb
    {
        //單一SQL回傳單一值
        public string ExecScalar(SqlCommand cmd, string connNM, ref string dbmsg)
        {
            SqlConnection conn = new SqlConnection(connNM);
            string resultStr = "";
            DataSet ds = new DataSet();
            try
            {
                if (conn.State == ConnectionState.Open) { conn.Close(); }
                conn.Open();
                cmd.Connection = conn;
                //if (cmd.ExecuteScalar() != null) { resultStr = cmd.ExecuteScalar().ToString(); }  // <== 執行兩次 cmd.ExecuteScalar()
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
        public DataSet OpenSQL(SqlCommand cmd, string connNM, ref string dbmsg)
        {
            SqlConnection conn = new SqlConnection(connNM);
            SqlDataAdapter adp = new SqlDataAdapter();
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
        public DataTable OpenSQLReturnDT(SqlCommand cmd, string connNM, ref string dbmsg)
        {
            SqlConnection conn = new SqlConnection(connNM);
            SqlDataAdapter adp = new SqlDataAdapter();
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
        public DataSet OpenSQL(List<SqlCommand> cmd, string connNM, ref string dbmsg)
        {
            SqlDataAdapter adp = new SqlDataAdapter();
            DataSet ds = new DataSet();
            SqlCommand exeCmd = new SqlCommand();
            using (SqlConnection conn = new SqlConnection(connNM))
            {
                try
                {
                    if (conn.State == ConnectionState.Open) { conn.Close(); }
                    conn.Open();
                    foreach (SqlCommand dbcommand in cmd)
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
        public int ExecSQL(SqlCommand cmd, string connNM, ref string dbmsg)
        {
            int rowsAffected = -1;
            SqlConnection conn = new SqlConnection(connNM);
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
        public int ExecSQL(List<SqlCommand> cmd, string connNM, ref string dbmsg)
        {
            SqlTransaction transaction;
            int rowsAffected = 0;
            SqlCommand exeCmd = new SqlCommand();
            try
            {
                using (SqlConnection conn = new SqlConnection(connNM))
                {
                    if (conn.State == ConnectionState.Open) { conn.Close(); }
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    try
                    {
                        foreach (SqlCommand dbcommand in cmd)
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
        public DataSet ExecSP(SqlCommand cmd, List<CallDBtools_MSDb.MSDbParam> paraList, string connNM, ref string dbmsg)
        {
            SqlConnection conn = new SqlConnection(connNM);
            DataSet ds = new DataSet();
            DataTable dt = ds.Tables.Add();
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
                    SqlCommandBuilder.DeriveParameters(cmd);
                    for (int i = 0; i < paraList.Count; i++)
                    {
                        cmd.Parameters[i].Value = paraList[i].ParaVL;
                    }
                }
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr != null)
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
        public DataSet ExecSP_output(SqlCommand cmd, List<CallDBtools_MSDb.MSDbParam> paraList, string connNM, ref string dbmsg)
        {
            SqlConnection conn = new SqlConnection(connNM);
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
                    SqlCommandBuilder.DeriveParameters(cmd);
                    for (int i = 0; i < paraList.Count; i++)
                    {
                        cmd.Parameters[i].Value = paraList[i].ParaVL;
                        if (paraList[i].ParaVL.ToString().Contains("Output")) ds.Tables["T1"].Columns.Add(paraList[i].ParaNM);
                    }
                }
                cmd.ExecuteNonQuery();
                for (int i = 0; i < paraList.Count; i++)
                {
                    if (paraList[i].ParaVL.ToString().Contains("Output"))
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
        public string ExecScalarByTransaction(SqlCommand cmd)
        {
            string resultStr = "";
            if (cmd.ExecuteScalar() != null) { resultStr = cmd.ExecuteScalar().ToString(); }
            return resultStr;
        }

        //包Transaction 單一SQL回傳單一dataset.table
        public DataSet OpenSQLByTransaction(SqlCommand cmd)
        {
            SqlDataAdapter adp = new SqlDataAdapter();
            DataSet ds1 = new DataSet();
            adp.SelectCommand = cmd;
            adp.Fill(ds1);
            return ds1;
        }

        //包Transaction 回傳單一SQL資料庫更動的筆數
        public int ExecSQLByTransaction(SqlCommand cmd, SqlTransaction transaction)
        {
            int rowsAffected = -1;
            cmd.Transaction = transaction;
            rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected;
        }

        L l = new L("JCLib.DB.Tool.DBtools_MSDB");
        String getDebugSql(SqlCommand cmd)
        {
            String sql = cmd.CommandText;
            foreach (SqlParameter p in cmd.Parameters)
            {
                sql = sql.Replace(p.ParameterName, "'" + p.Value + "'");
            }
            return sql;
        }
        //包Transaction 回傳多個SQL資料庫更動的筆數
        public int ExecSQLByTransaction(List<SqlCommand> cmd, SqlTransaction transaction)
        {
            int rowsAffected = 0;
            int idx = 0;
            //l.lg("ExecSQLByTransaction()", idx.ToString() + "\r\n--start -------------------------------------");
            foreach (SqlCommand dbcommand in cmd)
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