using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace JCLib.DB.Impl
{
    public class SqlDB : AccessLoggedDB
    {
        public SqlDB(IDbConnection conn) : base(conn) { }
        public SqlDB(string conn_name) : base(conn_name) { }

        public override DataTable ExecQuery(CommandParam param)
        {
            DataTable result = null;
            using (SqlCommand _cmd = (SqlCommand)PrepareCommand(param))
            {
                using (SqlDataAdapter _adapter = new SqlDataAdapter(_cmd))
                {
                    string _table_name = param.TableName;
                    result = LogAccess(_adapter.SelectCommand.CommandText, () =>
                    {
                        DataTable _result = new DataTable(_table_name);
                        _adapter.Fill(_result);
                        return _result;
                    });
                }
            }

            return result;
        }

        protected override IDbCommand PrepareCommand(CommandParam param)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = (SqlConnection)this.Connection;
            cmd.CommandText = param.CommandText;
            foreach (KeyValuePair<string, object> _kvp in param.Parameters)
            {
                cmd.Parameters.AddWithValue(_kvp.Key, _kvp.Value);
            }

            return cmd;
        }

        protected override IDbConnection CreateConnection(string connstr)
        {
            return new SqlConnection(connstr);
        }
    }
}
