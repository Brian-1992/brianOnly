using System;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Collections.Generic;
using System.IO;
using JCLib.DB.Tool;

namespace CDS010
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //資料庫連線
        private string getDBConnStr()
        {
            string connStr;
            OracleConnectionStringBuilder oracleConnstr = new OracleConnectionStringBuilder();
            //AIDC連線
            oracleConnstr.ConnectionString = "Persist Security Info=True;Data Source=192.168.99.52/mmsms;User ID=MMSDEV;Password=Mms#Dev1357;";
            //三總連線
            //oracleConnstr.ConnectionString = "Persist Security Info=True;Data Source=10.200.6.200:1524/MATE2DB;User ID=MMSDEV;Password=Mms#Dev1357;";
            connStr = oracleConnstr.ConnectionString;
            return connStr;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            ExcutePassto();
            this.Close();
        }
        private void ExcutePassto()
        {
            
            try
            {
                string v_whno = "560000";
                string v_max_lotno = "";
                int i_max_lotno = 0;
                string v_need_pick_cnt = "";
                int i_need_pick_cnt = 0;
                int rowsAffected_oracle = -1;
                string msg_oracle = "error";
                DataTable dt_oralce = new DataTable();
                string sql_oracle = " ";
                //查詢當日是否已有分派臨時申請單大於1000的揀貨批次資料
                sql_oracle += " select max(lot_no) as max_lotno ";
                sql_oracle += " from BC_WHPICKDOC ";
                sql_oracle += " where wh_no=:whno and pick_date=trunc(sysdate)+1 ";

                List<OracleParam> paraList1 = new List<OracleParam>();
                paraList1.Add(new OracleParam(1, ":whno", "VarChar", v_whno));
                dt_oralce = CallOpenSQLReturnDT(sql_oracle, null, paraList1, "oracle", "T1", ref msg_oracle);
                if (msg_oracle == "" && dt_oralce.Rows.Count > 0)
                {
                    v_max_lotno = dt_oralce.Rows[0]["max_lotno"].ToString().Trim();
                    i_max_lotno = int.Parse(v_max_lotno);
                }
                //若已有分派臨時申請單資料，則進行轉入新臨時申請單分派給值日生
                if(v_max_lotno != "" && i_max_lotno > 1000)
                {
                    //刪除已轉入揀貨作業但未列入揀貨批次過期臨時申請單明細資料
                    sql_oracle = " delete from BC_WHPICK a ";
                    sql_oracle += " where wh_no=:whno  ";
                    sql_oracle += " and pick_date>sysdate-5 and pick_date<sysdate ";
                    sql_oracle += " and docno in (select docno from BC_WHPICKDOC ";
                    sql_oracle += "  where wh_no=:whno ";
                    sql_oracle += "  and pick_date>sysdate-5 and pick_date<sysdate ";
                    sql_oracle += "  and apply_kind='2' and lot_no=0) ";
                    List<OracleParam> paraList2 = new List<OracleParam>();
                    paraList2.Add(new OracleParam(1, ":whno", "VarChar", v_whno));
                    rowsAffected_oracle = CallExecSQL(sql_oracle, paraList2, "oracle", ref msg_oracle);
                    if (msg_oracle != "")
                    {
                        I_ERROR_LOG("CDS010", "(分配臨時申請單)STEP2-刪除BC_WHPICK失敗,庫房:" + v_whno, "AUTO");
                    }
                    //刪除已轉入揀貨作業但未列入揀貨批次過期臨時申請單資料
                    sql_oracle = " delete from BC_WHPICKDOC a ";
                    sql_oracle += " where wh_no=:whno  ";
                    sql_oracle += " and pick_date>sysdate-5 and pick_date<sysdate ";
                    sql_oracle += " and apply_kind='2' and lot_no=0 ";
                    List<OracleParam> paraList3 = new List<OracleParam>();
                    paraList3.Add(new OracleParam(1, ":whno", "VarChar", v_whno));
                    rowsAffected_oracle = CallExecSQL(sql_oracle, paraList3, "oracle", ref msg_oracle);
                    if (msg_oracle != "")
                    {
                        I_ERROR_LOG("CDS010", "(分配臨時申請單)STEP3-刪除BC_WHPICK失敗,庫房:" + v_whno, "AUTO");
                    }
                    //轉入新臨時申請單明細資料
                    sql_oracle = " insert into BC_WHPICK ( ";
                    sql_oracle += " wh_no,pick_date,docno, "; //3
                    sql_oracle += " seq,mmcode,appqty, "; //6
                    sql_oracle += " base_unit,aplyitem_note,mat_class,  "; //9
                    sql_oracle += " mmname_c,mmname_e,wexp_id,  "; //12
                    sql_oracle += " store_loc)  "; //13
                    sql_oracle += " select :whno as wh_no,trunc(sysdate)+1 as pick_date,a.docno, "; //3
                    sql_oracle += " b.seq,b.mmcode,b.pick_qty as appqty, "; //6
                    sql_oracle += " (select base_unit from MI_MAST where mmcode=b.mmcode) as base_unit, ";
                    sql_oracle += " b.aplyitem_note, ";
                    sql_oracle += " (select mat_class from MI_MAST where mmcode=b.mmcode) as mat_class, "; //9
                    sql_oracle += " (select mmname_c from MI_MAST where mmcode=b.mmcode) as mmname_c, ";
                    sql_oracle += " (select mmname_e from MI_MAST where mmcode=b.mmcode) as mmname_e, ";
                    sql_oracle += " (select wexp_id from MI_MAST where mmcode=b.mmcode) as wexp_id, "; //12
                    sql_oracle += " (select store_loc from MI_WLOCINV where wh_no=:whno and mmcode=b.mmcode and rownum=1) as store_loc "; //13
                    sql_oracle += " from ME_DOCM a,ME_DOCD b "; 
                    sql_oracle += " where a.docno=b.docno "; 
                    sql_oracle += " and a.apptime>sysdate-5 and a.frwh=:whno "; 
                    sql_oracle += " and a.doctype in ('MR','MS','MR1','MR2','MR3','MR4') "; 
                    sql_oracle += " and a.flowid in ('0102','0602','3') "; 
                    sql_oracle += " and a.apply_kind='2' "; 
                    sql_oracle += " and not exists (select 'x' from BC_WHPICKDOC where wh_no=a.frwh and docno=a.docno) "; 
                    sql_oracle += " order by docno "; 
                    List<OracleParam> paraList4 = new List<OracleParam>();
                    paraList4.Add(new OracleParam(1, ":whno", "VarChar", v_whno));
                    rowsAffected_oracle = CallExecSQL(sql_oracle, paraList4, "oracle", ref msg_oracle);
                    if (msg_oracle != "")
                    {
                        I_ERROR_LOG("CDS010", "(分配臨時申請單)STEP4-新增臨時申請單明細失敗,庫房:" + v_whno, "AUTO");
                    }
                    //轉入新臨時申請單單號資料
                    sql_oracle = " insert into BC_WHPICKDOC ( ";
                    sql_oracle += " wh_no,pick_date,docno, "; //3
                    sql_oracle += " apply_kind,complexity,lot_no) "; //6
                    sql_oracle += " select :whno as wh_no,trunc(sysdate)+1 as pick_date,docno,  ";  //3
                    sql_oracle += " apply_kind,1,0 "; //6
                    sql_oracle += " from ME_DOCM a "; 
                    sql_oracle += " where apptime>sysdate-5 and frwh = :whno "; 
                    sql_oracle += " and doctype in ('MR','MS','MR1','MR2','MR3','MR4') "; 
                    sql_oracle += " and a.flowid in ('0102','0602','3') "; 
                    sql_oracle += " and apply_kind='2' "; 
                    sql_oracle += " and not exists (select 'x' from BC_WHPICKDOC where wh_no=a.frwh and docno=a.docno)  "; 
                    sql_oracle += " order by docno  "; 

                    List<OracleParam> paraList5 = new List<OracleParam>();
                    paraList5.Add(new OracleParam(1, ":whno", "VarChar", v_whno));
                    rowsAffected_oracle = CallExecSQL(sql_oracle, paraList5, "oracle", ref msg_oracle);
                    if (msg_oracle != "")
                    {
                        I_ERROR_LOG("CDS010", "(分配臨時申請單)STEP5-新增臨時申請單單號失敗,庫房:" + v_whno, "AUTO");
                    }
                    //查詢是否有未分派臨時申請單資料，如有則進行分派給值日生
                    sql_oracle = " select count(*) as need_pick_cnt from BC_WHPICKDOC ";
                    sql_oracle += " where wh_no= :whno and pick_date=trunc(sysdate)+1 ";
                    sql_oracle += " and apply_kind='2' and lot_no=0 ";

                    List<OracleParam> paraList6 = new List<OracleParam>();
                    paraList6.Add(new OracleParam(1, ":whno", "VarChar", v_whno));
                    dt_oralce = CallOpenSQLReturnDT(sql_oracle, null, paraList6, "oracle", "T1", ref msg_oracle);
                    if (msg_oracle == "" && dt_oralce.Rows.Count > 0)
                    {
                        v_need_pick_cnt = dt_oralce.Rows[0]["need_pick_cnt"].ToString().Trim();
                        i_need_pick_cnt = int.Parse(v_need_pick_cnt);
                    }
                    //如有未分派臨時申請單資料則進行分派給值日生
                    if (i_need_pick_cnt > 0)
                    {
                        i_max_lotno = i_max_lotno + 1;
                        v_max_lotno = i_max_lotno.ToString();
                        sql_oracle = " insert into BC_WHPICKLOT ( ";
                        sql_oracle += " wh_no,pick_date,lot_no, "; //3
                        sql_oracle += " pick_userid,pick_status) "; //5
                        sql_oracle += " values ( "; 
                        sql_oracle += " '560000' ,trunc(sysdate)+1, :max_lotno,  ";  //3
                        sql_oracle += " (select wh_userid from BC_WHID where wh_no= '560000' and is_duty='Y'),'A' "; //5
                        sql_oracle += " ) ";
                        List<OracleParam> paraList7 = new List<OracleParam>();
                        //paraList7.Add(new OracleParam(1, ":whno", "VarChar", v_whno));
                        paraList7.Add(new OracleParam(1, ":max_lotno", "VarChar", v_max_lotno));
                        rowsAffected_oracle = CallExecSQL(sql_oracle, paraList7, "oracle", ref msg_oracle);
                        if (msg_oracle != "")
                        {
                            I_ERROR_LOG("CDS010", "(分配臨時申請單)STEP6-新增臨時申請單批號失敗,庫房:" + v_whno + ",批號:" + v_max_lotno, "AUTO");
                        }

                        sql_oracle = " update BC_WHPICKDOC set lot_no = :max_lotno ";
                        sql_oracle += " where wh_no = '560000' and pick_date=trunc(sysdate)+1 "; 
                        sql_oracle += " and apply_kind='2' and lot_no=0 "; 
                        List<OracleParam> paraList8 = new List<OracleParam>();
                        //paraList8.Add(new OracleParam(1, ":whno", "VarChar", v_whno));
                        paraList8.Add(new OracleParam(1, ":max_lotno", "VarChar", v_max_lotno));
                        rowsAffected_oracle = CallExecSQL(sql_oracle, paraList8, "oracle", ref msg_oracle);
                        if (msg_oracle != "")
                        {
                            I_ERROR_LOG("CDS010", "(分配臨時申請單)STEP7-更新臨時申請單失敗,庫房:" + v_whno + ",批號:" + v_max_lotno, "AUTO");
                        }

                        sql_oracle = " update BC_WHPICK  ";
                        sql_oracle += " set pick_userid=(select wh_userid from BC_WHID where wh_no= '560000' and is_duty='Y') ";
                        sql_oracle += " where wh_no= '560000' and pick_date=trunc(sysdate)+1 ";
                        sql_oracle += " and docno in (select docno from BC_WHPICKDOC where wh_no = '560000' ";
                        sql_oracle += " and pick_date = trunc(sysdate)+1 ";
                        sql_oracle += " and lot_no = :max_lotno ) ";
                        List<OracleParam> paraList9 = new List<OracleParam>();
                        //paraList9.Add(new OracleParam(1, ":whno", "VarChar", v_whno));
                        paraList9.Add(new OracleParam(1, ":max_lotno", "VarChar", v_max_lotno));
                        rowsAffected_oracle = CallExecSQL(sql_oracle, paraList9, "oracle", ref msg_oracle);
                        if (msg_oracle != "")
                        {
                            I_ERROR_LOG("CDS010", "(分配臨時申請單)STEP8-更新臨時申請單失敗,庫房:" + v_whno + ",批號:" + v_max_lotno, "AUTO");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CallDBtools callDBtools = new CallDBtools();
                callDBtools.WriteExceptionLog(ex.Message, "BDS001", "(衛材)程式錯誤:");
            }
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
                //string parStr = "";
                //for (int i = 0; i < cmd.Parameters.Count; i++)
                //{
                //    parStr += cmd.Parameters[i].ParameterName + " = " + cmd.Parameters[i].Value;
                //}
                //CallDBtools callDBtools = new CallDBtools();
                //callDBtools.WriteExceptionLog(ex.Message, cmd.CommandText.ToString(), parStr);
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

        //回傳單一SQL資料庫更動的筆數
        //cmdstr:單一SQL語法、paraList:參數NO=1、參數NAME、參數TYPE、參數VALUE、dbNM:資料庫名稱
        public int CallExecSQL(string cmdstr, List<OracleParam> paraList, string dbNM, ref string dbmsg)
        {
            OracleCommand cmd = new OracleCommand(cmdstr);
            int rowsAffected = -1;
            string dbConn = "";

            //dbConn = getDBConnStr(); // 連線字串改放在Schedule.config
            CallDBtools calldbtools = new CallDBtools();
            dbConn = calldbtools.SelectDB("oracle");

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
            rowsAffected = ExecSQL(cmd, dbConn, ref dbmsg);
            return rowsAffected;
        }

        //單一SQL回傳單一datatable
        //cmdstr:單一SQL語法、sortstr:排序欄位、paraList:參數NO=1、參數NAME、參數TYPE、參數VALUE、dbNM:資料庫名稱、dsname:data table name
        public DataTable CallOpenSQLReturnDT(string cmdstr, string sortstr, List<OracleParam> paraList, string dbNM, string dsname, ref string dbmsg)
        {
            OracleCommand cmd = new OracleCommand();
            DataTable dt = new DataTable();
            string dbConn = "";

            //dbConn = getDBConnStr(); // 連線字串改放在Schedule.config
            CallDBtools calldbtools = new CallDBtools();
            dbConn = calldbtools.SelectDB("oracle");

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

            dt = OpenSQLReturnDT(cmd, dbConn, ref dbmsg);
            dt.TableName = dsname;
            return dt;
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
                //string parStr = "";
                //for (int i = 0; i < cmd.Parameters.Count; i++)
                //{
                //    parStr += cmd.Parameters[i].ParameterName + " = " + cmd.Parameters[i].Value;
                //}
                //CallDBtools callDBtools = new CallDBtools();
                //callDBtools.WriteExceptionLog(ex.Message, cmd.CommandText.ToString(), parStr);
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

        // 寫 Oracle ERROR_LOG
        public int I_ERROR_LOG(string pg, string msg, string userid)
        {
            int rowsAffected = -1;
            string dbmsg = "error";
            List<OracleParam> paraList = new List<OracleParam>();
            string cmdstr = "insert into ERROR_LOG(LOGTIME,PG,MSG,USERID) ";
            cmdstr += "                    values (sysdate, :pg, :msg, :userid) ";
            paraList.Add(new OracleParam(1, ":pg", "VarChar", pg));
            paraList.Add(new OracleParam(1, ":msg", "VarChar", msg));
            paraList.Add(new OracleParam(1, ":userid", "VarChar", userid));
            rowsAffected = CallExecSQL(cmdstr, paraList, "oracle", ref dbmsg);
            return rowsAffected;
        }

    }
}
