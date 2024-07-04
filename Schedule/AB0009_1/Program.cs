using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JCLib.DB.Tool;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace AB0009_1
{
    class Program
    {
        static void Main(string[] args)
        {
            procCreate();
        }

        static L l = new L("AB0009_1.Program");

        static public string procCreate()
        {
            CallDBtools calldbtools = new CallDBtools();
            String s_conn_oracle = calldbtools.SelectDB("oracle");

            OracleConnection conn = new OracleConnection(s_conn_oracle);
            conn.Open();
            OracleCommand cmd = new OracleCommand("CREATE_MR_DOC", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.ExecuteNonQuery();

            l.clg("排程執行完畢");
            return "Finish";
        }
    }
}
