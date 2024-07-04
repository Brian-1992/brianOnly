using System;
using System.Data;
using System.Configuration;

namespace JCLib.DB
{
    public abstract class AccessLoggedDB : IDBAccess
    {
        private IDbConnection _conn;
        private IDbTransaction _tran;

        public IDbConnection Connection { get { return _conn; } }

        public IDbConnection GetConnection()
        {
            return _conn;
        }

        public AccessLoggedDB(IDbConnection conn)
        {
            _conn = conn;
            _conn.Open();
        }

        public AccessLoggedDB(string conn_name)
        {
            string connstr = ConfigurationManager.ConnectionStrings[conn_name].ConnectionString;
            _conn = CreateConnection(connstr);
            _conn.Open();
        }

        protected abstract IDbConnection CreateConnection(string connstr);

        public void BeginTransaction()
        {
            _tran = _conn.BeginTransaction();
        }

        public void Commit()
        {
            _tran.Commit();
        }

        public void Rollback()
        {
            _tran.Rollback();
        }

        public Object ExecScalar(CommandParam param)
        {
            object result = null;
            using (IDbCommand cmd = PrepareCommand(param))
            {
                result = LogAccess(cmd.CommandText, () => { return cmd.ExecuteScalar(); });
            }

            return result;
        }

        public void ExecReader(CommandParam param, Action<IDataReader> action)
        {
            using (IDbCommand cmd = PrepareCommand(param))
            {
                LogAccess(cmd.CommandText, () =>
                {
                    using (IDataReader rdr = cmd.ExecuteReader())
                    {
                        action(rdr);
                    }

                    return 0;
                });
            }
        }

        public abstract DataTable ExecQuery(CommandParam param);

        public ApiResponse TryExecQuery(CommandParam param, ApiResponse ar = null)
        {
            if (ar == null)
                ar = new ApiResponse();
            try
            {
                ar.ds.Tables.Add(this.ExecQuery(param));
            }
            catch (Exception ex)
            {
                ar.success = false;
                ar.msg += string.Format("TryExecQuery Error:{0}{1}", ex.Message, Environment.NewLine);
            }

            return ar;
        }

        public int ExecNonQuery(CommandParam param)
        {
            int result = 0;
            using (IDbCommand cmd = PrepareCommand(param))
            {
                result = LogAccess(cmd.CommandText, () => { return cmd.ExecuteNonQuery(); });
            }

            return result;
        }

        public ApiResponse TryExecNonQuery(CommandParam param, ApiResponse ar = null)
        {
            if (ar == null)
                ar = new ApiResponse();
            try
            {
                ar.afrs = this.ExecNonQuery(param);
            }
            catch (Exception ex)
            {
                ar.success = false;
                ar.msg += string.Format("TryExecNonQuery Error:{0}{1}", ex.Message, Environment.NewLine);
            }

            return ar;
        }

        protected abstract IDbCommand PrepareCommand(CommandParam param);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="page"></param>
        /// <param name="page_size"></param>
        /// <param name="sort">ID DESC、NAME ASC</param>
        /// <returns></returns>
        public ApiResponse GetPagedRows(CommandParam param, int page, int page_size, string sort = "", ApiResponse ar = null)
        {
            //若效能不夠，再改用CTE來實作
            param.CommandText = @"SELECT COUNT(1) OVER() RC, V.* FROM ( " + param.CommandText + ") V " +
                    "{0} OFFSET @OFFSET ROWS FETCH NEXT @PAGE_SIZE ROWS ONLY ";
            if (sort == "")
                param.CommandText = string.Format(param.CommandText, "");
            else
            {
                sort = string.Format(" ORDER BY {0} ", sort);
                param.CommandText = string.Format(param.CommandText, sort);
            }
            param.AddParam("@OFFSET", (page - 1) * page_size);
            param.AddParam("@PAGE_SIZE", page_size);

            return TryExecQuery(param, ar);
        }

        protected dynamic LogAccess(string command_text, Func<dynamic> func)
        {
            try
            {
                //紀錄CommandText，還可紀錄Parameters
                Console.WriteLine(command_text);
                return func();
            }
            catch (Exception ex)
            {
                //紀錄SQL Error
                Console.WriteLine(ex.Message);
                throw ex;
            }
        }

        public void Dispose()
        {
            _conn.Close();
        }
    }
}
