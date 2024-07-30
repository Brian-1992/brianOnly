using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;

namespace JCLib.DB.Impl
{
    public class OleDB : AccessLoggedDB
    {
        public OleDB(IDbConnection conn) : base(conn) { }
        public OleDB(string conn_name) : base(conn_name) { }

        public override DataTable ExecQuery(CommandParam command_param)
        {
            DataTable result = null;
            using (OleDbCommand _cmd = (OleDbCommand)PrepareCommand(command_param))
            {
                using (OleDbDataAdapter _adapter = new OleDbDataAdapter(_cmd))
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
            OleDbCommand cmd = new OleDbCommand();
            cmd.Connection = (OleDbConnection)this.Connection;
            cmd.CommandText = command_param.CommandText;
            foreach (KeyValuePair<string, object> _kvp in command_param.Parameters)
            {
                cmd.Parameters.AddWithValue(_kvp.Key, _kvp.Value);
            }

            return cmd;
        }

        protected override IDbConnection CreateConnection(string connstr)
        {
            return new OleDbConnection(connstr);
        }
    }
}
