using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIS14SUPDETtransfer
{
    public class OracleDB : IDisposable
    {
        // Pointer to an external unmanaged resource.
        private IntPtr handle;
        // Track whether Dispose has been called.
        private bool disposed = false;

        private OracleConnection dbcon = new OracleConnection();
        private object _p1;
        private OracleCommand cmd_ = new OracleCommand();
        private OracleTransaction dbtransaction;

        private string dbalias = "";
        private string dbConnectionString;

        public OracleDB(string ORACLE_DB)
        {
            string constr = getConnectionString(ORACLE_DB);
            //if (constr.Trim() > " ")  Modi y Fisher
            if (constr.Trim() != " ")
            {
                try
                {
                    dbcon.ConnectionString = constr;
                    dbcon.Open();
                    cmd_.CommandType = CommandType.Text;
                    cmd_.Connection = dbcon;
                }
                catch (Exception ex)
                {
                    string t = ex.Message;
                }
            }
        }

        public string getConnectionString(string ORACLE_DB)
        {
            System.Configuration.AppSettingsReader config = new System.Configuration.AppSettingsReader();
            string value = string.Empty;

            try
            {
                dbConnectionString = ConfigurationManager.ConnectionStrings[ORACLE_DB].ConnectionString; ;
                Console.WriteLine(dbConnectionString);
            }
            catch (Exception ex)
            {
                return "";
            }
            return dbConnectionString;
        }

        public void dbClose()
        {
            dbcon.Close();
        }

        public System.Data.DataSet GetDataSet(string SQL)
        {
            DataSet ds = new DataSet();
            using (OracleCommand cmd = new OracleCommand())
            {
                cmd.Connection = dbcon;
                // If dbtransaction IsNot Nothing Then
                // cmd.Transaction = dbtransaction
                // End If
                cmd.CommandTimeout = 60 * 120; // 120 minutes
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.CommandText = SQL;

                try
                {
                    using (OracleDataAdapter da = new OracleDataAdapter(cmd))
                    {
                        da.Fill(ds);
                    }
                }
                catch (Exception e)
                {
                    ds = new DataSet();
                    DataTable dt = new DataTable();
                    ds.Tables.Add(dt);
                }

            }
            return ds;
        }

        public System.Data.DataSet GetDataSet(string SQL, int startpage, int pagesize)
        {
            DataSet ds = new DataSet();
            using (OracleCommand cmd = new OracleCommand())
            {
                cmd.Connection = dbcon;
                // If dbtransaction IsNot Nothing Then
                // cmd.Transaction = dbtransaction
                // End If
                cmd.CommandTimeout = 60 * 120; // 120 minutes
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.CommandText = SQL;
                DataTable dt = new DataTable();
                using (OracleDataAdapter da = new OracleDataAdapter(cmd))
                {
                    int start = startpage * pagesize;
                    da.Fill(ds, start, pagesize, "TMPTABLE");
                    ds.Tables.Add(dt);
                }
            }
            return ds;
        }

        public OracleDataReader GetDataReader(string SQL)
        {
            OracleDataReader reader;
            using (OracleCommand cmd = new OracleCommand())
            {
                cmd.Connection = dbcon;
                // If dbtransaction IsNot Nothing Then
                // cmd.Transaction = dbtransaction
                // End If
                cmd.CommandTimeout = 60 * 120; // 120 minutes
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.CommandText = SQL;
                reader = cmd.ExecuteReader();
            }
            return reader;
        }

        public int ExecSql(string SQL)
        {
            DataSet ds = new DataSet();
            using (OracleCommand cmd = new OracleCommand())
            {
                cmd.Connection = dbcon;
                // If dbtransaction IsNot Nothing Then
                // cmd.Transaction = dbtransaction
                // End If
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.CommandText = SQL;
                return cmd.ExecuteNonQuery();
            }
        }

        public int ExecSP(string SQL)
        {
            DataSet ds = new DataSet();
            if ((int)dbcon.State == (int)ConnectionState.Closed)
                dbcon.Open();
            using (OracleCommand OraCmd = new OracleCommand())
            {
                OraCmd.Connection = dbcon;
                // If dbtransaction IsNot Nothing Then
                // OraCmd.Transaction = dbtransaction
                // End If
                OraCmd.CommandType = System.Data.CommandType.StoredProcedure;
                OraCmd.CommandText = SQL;
                return OraCmd.ExecuteNonQuery();
            }
        }




        public DateTime GetDate()
        {
            DateTime dt;
            using (OracleCommand cmd = new OracleCommand())
            {
                cmd.Connection = dbcon;
                // If dbtransaction IsNot Nothing Then
                // cmd.Transaction = dbtransaction
                // End If
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.CommandText = "SELECT SYSDATE AS DT FROM DUAL";
                OracleDataReader reader = default(OracleDataReader);
                reader = cmd.ExecuteReader();
                reader.Read();
                dt = Convert.ToDateTime(reader["DT"]);
                reader.Close();
            }
            return dt;
        }

        public System.Data.DataSet cmdDataSet()
        {
            DataSet ds = new DataSet();
            using (OracleDataAdapter da = new OracleDataAdapter(cmd_))
            {
                da.Fill(ds);
            }
            return ds;
        }

        public void BeginTransaction()
        {
            dbtransaction = dbcon.BeginTransaction();
        }
        public void Commit()
        {
            dbtransaction.Commit();
            dbtransaction.Dispose();
            dbtransaction = null;
        }
        public void Rollback()
        {
            dbtransaction.Rollback();
            dbtransaction.Dispose();
            dbtransaction = null;
        }

        public OracleCommand cmd
        {
            get
            {
                OracleCommand cmdRet = default(OracleCommand);
                // If dbtransaction IsNot Nothing Then
                // cmd_.Transaction = dbtransaction
                // End If
                cmdRet = cmd_;
                return cmdRet;
            }
        }

        public OracleConnection connection
        {
            get
            {
                OracleConnection connectionRet = default(OracleConnection);
                connectionRet = dbcon;
                return connectionRet;
            }
        }

        public bool Connected
        {
            get
            {
                bool ConnectedRet = default(bool);
                ConnectedRet = ((int)dbcon.State == (int)ConnectionState.Executing) || ((int)dbcon.State == (int)ConnectionState.Fetching) || ((int)dbcon.State == (int)ConnectionState.Open);
                return ConnectedRet;
            }
        }

        private bool disposedValue; // 偵測多餘的呼叫

        // IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                }
            }
            this.disposedValue = true;
        }

        // TODO: 只有當上面的 Dispose(ByVal disposing As Boolean) 有可釋放 Unmanaged 資源的程式碼時，才覆寫 Finalize()。
        // Protected Overrides Sub Finalize()
        // ' 請勿變更此程式碼。在上面的 Dispose(ByVal disposing As Boolean) 中輸入清除程式碼。
        // Dispose(False)
        // MyBase.Finalize()
        // End Sub

        // 由 Visual Basic 新增此程式碼以正確實作可處置的模式。
        public void Dispose()
        {
            // 請勿變更此程式碼。在以上的 Dispose 置入清除程式碼 (ByVal 視為布林值處置)。
            dbcon.Close();
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
