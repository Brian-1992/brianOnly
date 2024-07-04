using System;
using System.Collections.Generic;
using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace JCLib.DB.Impl
{
    public class OraDB : AccessLoggedDB
    {
        public OraDB(IDbConnection conn) : base(conn) { }
        public OraDB(string conn_name) : base(conn_name) { }

        public override DataTable ExecQuery(CommandParam command_param)
        {
            DataTable result = null;
            using (OracleCommand _cmd = (OracleCommand)PrepareCommand(command_param))
            {
                using (OracleDataAdapter _adapter = new OracleDataAdapter(_cmd))
                {
                    result = LogAccess(_adapter.SelectCommand.CommandText, () =>
                    {
                        DataTable _result = new DataTable();
                        _adapter.Fill(_result);
                        return _result;
                    });
                }
            }

            return result;
        }

        protected override IDbCommand PrepareCommand(CommandParam command_param)
        {
            OracleCommand cmd = new OracleCommand();
            cmd.Connection = (OracleConnection)this.Connection;
            cmd.CommandText = command_param.CommandText;
            foreach (KeyValuePair<string, object> _kvp in command_param.Parameters)
            {
                cmd.Parameters.Add(_kvp.Key, _kvp.Value);
            }

            return cmd;
        }

        protected override IDbConnection CreateConnection(string connstr)
        {
            return new OracleConnection(connstr);
        }
    }
}
