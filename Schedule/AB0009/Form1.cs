using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using JCLib.DB.Tool;


namespace AB0009
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CallDBtools calldbtools = new CallDBtools();
            String s_conn_oracle = calldbtools.SelectDB("oracle");

            OracleConnection conn = new OracleConnection(s_conn_oracle);
            conn.Open();
            OracleCommand cmd = new OracleCommand("CREATE_MR_DOC", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            OracleParameter i_appid = new OracleParameter("I_APPID", OracleDbType.Varchar2, 8);
            i_appid.Direction = ParameterDirection.Input;
            i_appid.Value = "";

            OracleParameter o_retid = new OracleParameter("O_RETID", OracleDbType.Varchar2, 1);
            o_retid.Direction = ParameterDirection.Output;

            OracleParameter o_errmsg = new OracleParameter("O_ERRMSG", OracleDbType.Varchar2, 200);
            o_errmsg.Direction = ParameterDirection.Output;

            cmd.Parameters.Add(i_appid);
            cmd.Parameters.Add(o_retid);
            cmd.Parameters.Add(o_errmsg);

            cmd.ExecuteNonQuery();

            //MessageBox.Show(o_retid.Value.ToString() + "\n" + o_errmsg.Value.ToString());
            conn.Close();
        }
    }
}
