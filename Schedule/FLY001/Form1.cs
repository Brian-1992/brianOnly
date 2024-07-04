using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using GenTools;
using System.Text.RegularExpressions;
using JCLib.DB.Tool;
using System.Configuration;
using System.IO;

namespace GenTools
{

    public partial class Form1 : Form
    {

        #region " 共用函式區 "

            void bindCol(DataTable dt, ref DataGridView dgv)
            {
                DataTable dtN = new DataTable();
                DataRow dr;
                dtN.Columns.Add(new DataColumn("Seq"));
                dtN.Columns.Add(new DataColumn("Name"));
                dtN.Columns.Add(new DataColumn("TwName"));  // 中文別名
                dtN.Columns.Add(new DataColumn("Type"));    // 型態
                dtN.Columns.Add(new DataColumn("Size"));    // 大小
                dtN.Columns.Add(new DataColumn("Precision"));
                dtN.Columns.Add(new DataColumn("Scale"));
                dtN.Columns.Add(new DataColumn("IsNotNull"));
                dtN.Columns.Add(new DataColumn("IsKey"));
                dtN.Columns.Add(new DataColumn("IsIndex"));
                dtN.Columns.Add(new DataColumn("DefaultValue"));    // 預設值 
                dtN.Columns.Add(new DataColumn("Domain"));          // 值域，例(A:未處理, B:已處理)
                dtN.Columns.Add(new DataColumn("Memo"));            // 備註
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    DataColumn c = (DataColumn)dt.Columns[i];
                    dr = dtN.NewRow();
                    dr["Seq"] = i;
                    dr["Name"] = c.ColumnName;
                    dr["TwName"] = ""; // 中文別名
                    dr["Type"] = "Text"; // 中文別名
                    dtN.Rows.Add(dr);
                }
                dgv.DataSource = dtN;
            } // 

            String getControllerFolder(String code)
            {
                if (new Regex(@"^AA\w+").Match(code.ToUpper()).Success)
                    return "AA";
                else if (new Regex(@"^AB\w+").Match(code.ToUpper()).Success)
                    return "AA";
                else if (new Regex(@"^UR\w+").Match(code.ToUpper()).Success)
                    return "UR";
                else if (new Regex(@"^(\w{1})\w+").Match(code.ToUpper()).Success)
                {
                    Match m = new Regex(@"^(\w{1})\w+").Match(code.ToUpper());
                    if (m.Groups.Count >= 2)
                        return m.Groups[1].ToString();
                }
                return "ERR";
            } // 

            String getRepositoryFolder(String code)
            {
                if (new Regex(@"^AA\w+").Match(code.ToUpper()).Success)
                    return "AA";
                else if (new Regex(@"^AB\w+").Match(code.ToUpper()).Success)
                    return "AA";
                else if (new Regex(@"^UR\w+").Match(code.ToUpper()).Success)
                    return "UR";
                else if (new Regex(@"^(\w{1})\w+").Match(code.ToUpper()).Success)
                {
                    Match m = new Regex(@"^(\w{1})\w+").Match(code.ToUpper());
                    if (m.Groups.Count >= 2)
                        return m.Groups[1].ToString();
                }
                return "ERR";
            } // 

            String getModelsFolder(String code)
            {
                if (new Regex(@"^C\w+").Match(code.ToUpper()).Success)
                    return "C";
                else if (new Regex(@"^(\w{2})\w+").Match(code.ToUpper()).Success)
                {
                    Match m = new Regex(@"^(\w{2})\w+").Match(code.ToUpper());
                    if (m.Groups.Count >= 2)
                        return m.Groups[1].ToString();
                }
                return "ERR";
            } // 



        String getScriptsFolder(String code)
            {
                if (new Regex(@"^AA\w+").Match(code.ToUpper()).Success)
                    return "AA";
                else if (new Regex(@"^AB\w+").Match(code.ToUpper()).Success)
                    return "AA";
                else if (new Regex(@"^UR\w+").Match(code.ToUpper()).Success)
                    return "UR";
                else if (new Regex(@"^SP\w+").Match(code.ToUpper()).Success)
                    return "SP";
                else if (new Regex(@"^(\w{1})\w+").Match(code.ToUpper()).Success)
                {
                    Match m = new Regex(@"^(\w{1})\w+").Match(code.ToUpper());
                    if (m.Groups.Count >= 2)
                        return m.Groups[1].ToString();
                }
                return "ERR";
            } // 

        

        void load_listbox1()
        {
            //listBox1.MultiColumn = true;
            listBox1.SelectionMode = SelectionMode.MultiExtended;


            // Loop through and add 50 items to the ListBox.
            listBox1.BeginUpdate();
            //for (int x = 1; x <= 50; x++)
            //{
            //    listBox1.Items.Add("Item " + x.ToString());
            //}
            listBox1.Items.Add("BC_BARCODE");
            listBox1.Items.Add("BC_BOX");
            listBox1.Items.Add("BC_CATEGORY");
            listBox1.Items.Add("BC_CS_ACC_LOG");
            listBox1.Items.Add("BC_ITMANAGER");
            listBox1.Items.Add("BC_MANAGER");
            listBox1.Items.Add("BC_STLOC");
            listBox1.Items.Add("BC_WHCHKID");
            listBox1.Items.Add("BC_WHID");
            listBox1.Items.Add("BC_WHPICK");
            listBox1.Items.Add("BC_WHPICK_SHIPOUT");
            listBox1.Items.Add("BC_WHPICK_TEMP_LOTDOC");
            listBox1.Items.Add("BC_WHPICK_TEMP_LOTDOCSEQ");
            listBox1.Items.Add("BC_WHPICK_TEMP_LOTSUM");
            listBox1.Items.Add("BC_WHPICK_VALID");
            listBox1.Items.Add("BC_WHPICKDOC");
            listBox1.Items.Add("BC_WHPICKLOT");
            listBox1.Items.Add("CHK_DETAIL");
            listBox1.Items.Add("CHK_DETAILTOT");
            listBox1.Items.Add("CHK_MAST");
            listBox1.Items.Add("ERROR_LOG");
            listBox1.Items.Add("HIS_BACK");
            listBox1.Items.Add("HIS_BASORDD");
            listBox1.Items.Add("HIS_BASORDM");
            listBox1.Items.Add("HIS_HISBACK");
            listBox1.Items.Add("HIS_STKDMIT");
            listBox1.Items.Add("LOG_LOGIN");
            listBox1.Items.Add("ME_AB0071");
            listBox1.Items.Add("ME_AB0075A");
            listBox1.Items.Add("ME_AB0075B");
            listBox1.Items.Add("ME_AB0075C");
            listBox1.Items.Add("ME_AB0075D");
            listBox1.Items.Add("ME_AB0075E");
            listBox1.Items.Add("ME_AB0079");
            listBox1.Items.Add("ME_AB0079D");
            listBox1.Items.Add("ME_AB0079M");
            listBox1.Items.Add("ME_AB0079S");
            listBox1.Items.Add("ME_AB0084");
            listBox1.Items.Add("ME_BACK");
            listBox1.Items.Add("ME_CSTM");
            listBox1.Items.Add("ME_DOCA");
            listBox1.Items.Add("ME_DOCC");
            listBox1.Items.Add("ME_DOCD");
            listBox1.Items.Add("ME_DOCD_EC");
            listBox1.Items.Add("ME_DOCE");
            listBox1.Items.Add("ME_DOCEXP");
            listBox1.Items.Add("ME_DOCI");
            listBox1.Items.Add("ME_DOCM");
            listBox1.Items.Add("ME_EXPD");
            listBox1.Items.Add("ME_EXPM");
            listBox1.Items.Add("ME_FLOW");
            listBox1.Items.Add("ME_MDFD");
            listBox1.Items.Add("ME_MDFM");
            listBox1.Items.Add("ME_PCAD");
            listBox1.Items.Add("ME_PCAM");
            listBox1.Items.Add("MENULIST");
            listBox1.Items.Add("MI_DOCNO");
            listBox1.Items.Add("MI_DOCTYPE");
            listBox1.Items.Add("MI_MAST");
            listBox1.Items.Add("MI_MAST_N");
            listBox1.Items.Add("MI_MAST_N_LOG");
            listBox1.Items.Add("MI_MATCLASS");
            listBox1.Items.Add("MI_MCODE");
            listBox1.Items.Add("MI_MNSET");
            listBox1.Items.Add("MI_UNITCODE");
            listBox1.Items.Add("MI_UNITEXCH");
            listBox1.Items.Add("MI_WEXP_TRNS");
            listBox1.Items.Add("MI_WEXPINV");
            listBox1.Items.Add("MI_WHCOST");
            listBox1.Items.Add("MI_WHID");
            listBox1.Items.Add("MI_WHINV");
            listBox1.Items.Add("MI_WHMAST");
            listBox1.Items.Add("MI_WHMM");
            listBox1.Items.Add("MI_WHTRNS");
            listBox1.Items.Add("MI_WINVCTL");
            listBox1.Items.Add("MI_WINVMON");
            listBox1.Items.Add("MI_WLOCINV");
            listBox1.Items.Add("MM_DELI");
            listBox1.Items.Add("MM_ECUSE");
            listBox1.Items.Add("MM_PACK_D");
            listBox1.Items.Add("MM_PACK_M");
            listBox1.Items.Add("MM_PO_D");
            listBox1.Items.Add("MM_PO_M");
            listBox1.Items.Add("MM_PR_D");
            listBox1.Items.Add("MM_PR_M");
            listBox1.Items.Add("MM_WHAPLDT");
            listBox1.Items.Add("MMSMS_TBL");
            listBox1.Items.Add("PARAM_D");
            listBox1.Items.Add("PARAM_M");
            listBox1.Items.Add("PH_AIRHIS");
            listBox1.Items.Add("PH_AIRST");
            listBox1.Items.Add("PH_AIRTIME");
            listBox1.Items.Add("PH_ATTFILE");
            listBox1.Items.Add("PH_BANK");
            listBox1.Items.Add("PH_EQPD");
            listBox1.Items.Add("PH_MAILBACK");
            listBox1.Items.Add("PH_MAILLOG");
            listBox1.Items.Add("PH_MAILSP");
            listBox1.Items.Add("PH_MAILSP_D");
            listBox1.Items.Add("PH_MAILSP_M");
            listBox1.Items.Add("PH_PO_N");
            listBox1.Items.Add("PH_PUT_D");
            listBox1.Items.Add("PH_PUT_M");
            listBox1.Items.Add("PH_PUTTIME");
            listBox1.Items.Add("PH_REPLY");
            listBox1.Items.Add("PH_REPLY_LOG");
            listBox1.Items.Add("PH_SMALL_D");
            listBox1.Items.Add("PH_SMALL_M");
            listBox1.Items.Add("PH_VENDER");
            listBox1.Items.Add("PHRSDPT");
            listBox1.Items.Add("ROLE");
            listBox1.Items.Add("ROLE_DT");
            listBox1.Items.Add("TOAD_PLAN_TABLE");
            listBox1.Items.Add("UR_DOC");
            listBox1.Items.Add("UR_ERR_D");
            listBox1.Items.Add("UR_ERR_M");
            listBox1.Items.Add("UR_ID");
            listBox1.Items.Add("UR_INID");
            listBox1.Items.Add("UR_LOGIN");
            listBox1.Items.Add("UR_MENU");
            listBox1.Items.Add("UR_ROLE");
            listBox1.Items.Add("UR_TACL");
            listBox1.Items.Add("UR_UIR");
            listBox1.Items.Add("UR_UPLOAD");
            listBox1.Items.Add("USER_ADDPRIV");
            listBox1.Items.Add("USER_ROLE");
            listBox1.Items.Add("WB_AIRHIS");
            listBox1.Items.Add("WB_AIRST");
            listBox1.Items.Add("WB_AIRTIME");
            listBox1.Items.Add("WB_MAILBACK");
            listBox1.Items.Add("WB_PUT_D");
            listBox1.Items.Add("WB_PUT_M");
            listBox1.Items.Add("WB_PUTTIME");
            listBox1.Items.Add("WB_REPLY");


            listBox1.EndUpdate();
            // Allow the ListBox to repaint and display the new items.


            // Select three items from the ListBox.
            //listBox1.SetSelected(1, true);
            //listBox1.SetSelected(3, true);
            //listBox1.SetSelected(5, true);
        }

        void loadTableScript()
        {
            textBoxM.Text =
@"
select 
docno,
(select appdept from me_docm where docno=b.docno) as appdept,
sum(item_cnt) as item_sum,
sum(appqty) as appqty_sum,
(select lot_no from BC_WHPICKDOC where wh_no='560000' and docno=b.docno) as lot_no,
sum(act_pick_qty) as act_pick_qty_sum,sum(diffqty) as diffqty_sum
from (
    select wh_no, docno,1 as item_cnt,appqty,act_pick_qty,(act_pick_qty-appqty) as diffqty
    from BC_WHPICK a
    where wh_no='560000' "; // --{目前庫房號碼}
                            //    -- 這邊放條件
            textBoxM.Text +=
@"
)b group by docno order by docno
";
            // ---------------------------------------------
            tbTableScript.Text =
@"
ALTER TABLE MMSADM.ME_DOCD
 DROP PRIMARY KEY CASCADE;

DROP TABLE MMSADM.ME_DOCD CASCADE CONSTRAINTS;

CREATE TABLE MMSADM.ME_DOCD
(
  DOCNO          VARCHAR2(21 CHAR)              NOT NULL,
  SEQ            NUMBER                         NOT NULL,
  MMCODE         VARCHAR2(13 CHAR)              NOT NULL,
  APPQTY         NUMBER(11,3)                   DEFAULT 0                     NOT NULL,
  APVQTY         NUMBER(11,3)                   DEFAULT 0                     NOT NULL,
  APVTIME        DATE,
  APVID          VARCHAR2(8 CHAR),
  ACKQTY         NUMBER(11,3)                   DEFAULT 0                     NOT NULL,
  ACKID          VARCHAR2(8 CHAR),
  ACKTIME        DATE,
  STAT           VARCHAR2(1 CHAR),
  RDOCNO         VARCHAR2(21 CHAR),
  RSEQ           NUMBER,
  EXPT_DISTQTY   NUMBER(11,3)                   DEFAULT 0,
  DIS_USER       VARCHAR2(8 CHAR),
  DIS_TIME       DATE,
  BW_MQTY        NUMBER(11,3)                   DEFAULT 0,
  BW_SQTY        NUMBER(11,3)                   DEFAULT 0,
  PICK_QTY       NUMBER(11,3)                   DEFAULT 0,
  PICK_USER      VARCHAR2(8 CHAR),
  PICK_TIME      DATE,
  ONWAY_QTY      NUMBER(11,3),
  APL_CONTIME    DATE,
  AMT            NUMBER(14,3)                   DEFAULT 0,
  UP             NUMBER(11,3)                   DEFAULT 0,
  RV_MQTY        NUMBER(11,3)                   DEFAULT 0,
  GTAPL_RESON    VARCHAR2(2 CHAR)               DEFAULT NULL,
  APLYITEM_NOTE  VARCHAR2(50 CHAR),
  CREATE_TIME    DATE,
  CREATE_USER    VARCHAR2(8 CHAR),
  UPDATE_TIME    DATE,
  UPDATE_USER    VARCHAR2(8 CHAR),
  UPDATE_IP      VARCHAR2(20 CHAR),
  EXP_STATUS     VARCHAR2(2 CHAR)
)
TABLESPACE MMSMSTB01
PCTUSED    0
PCTFREE    10
INITRANS   1
MAXTRANS   255
STORAGE    (
            INITIAL          64K
            NEXT             1M
            MINEXTENTS       1
            MAXEXTENTS       UNLIMITED
            PCTINCREASE      0
            BUFFER_POOL      DEFAULT
           )
LOGGING 
NOCOMPRESS 
NOCACHE
NOPARALLEL
MONITORING;

COMMENT ON TABLE MMSADM.ME_DOCD IS '單據明細';

COMMENT ON COLUMN MMSADM.ME_DOCD.EXP_STATUS IS '效期管理狀態(R 退回)';

COMMENT ON COLUMN MMSADM.ME_DOCD.DOCNO IS '單據號碼(申請單號)';

COMMENT ON COLUMN MMSADM.ME_DOCD.SEQ IS '單據項次';

COMMENT ON COLUMN MMSADM.ME_DOCD.MMCODE IS '院內碼';

COMMENT ON COLUMN MMSADM.ME_DOCD.APPQTY IS '申請數量';

COMMENT ON COLUMN MMSADM.ME_DOCD.APVQTY IS '核撥數量(實際核撥數量)';

COMMENT ON COLUMN MMSADM.ME_DOCD.APVTIME IS '核撥時間';

COMMENT ON COLUMN MMSADM.ME_DOCD.APVID IS '核撥人員代碼';

COMMENT ON COLUMN MMSADM.ME_DOCD.ACKQTY IS '點收數量';

COMMENT ON COLUMN MMSADM.ME_DOCD.ACKID IS '點收人員代碼';

COMMENT ON COLUMN MMSADM.ME_DOCD.ACKTIME IS '點收時間';

COMMENT ON COLUMN MMSADM.ME_DOCD.STAT IS '執行狀況(衛材:軍品1換入2換出。藥材:A已接受、B已退回、C處理中)';

COMMENT ON COLUMN MMSADM.ME_DOCD.RDOCNO IS '關聯單據號碼(申購單號)';

COMMENT ON COLUMN MMSADM.ME_DOCD.RSEQ IS '關聯項次(申購單項次)';

COMMENT ON COLUMN MMSADM.ME_DOCD.EXPT_DISTQTY IS '預計核撥量';

COMMENT ON COLUMN MMSADM.ME_DOCD.DIS_USER IS '預計核撥人員';

COMMENT ON COLUMN MMSADM.ME_DOCD.DIS_TIME IS '預計核撥時間';

COMMENT ON COLUMN MMSADM.ME_DOCD.BW_MQTY IS '戰備調撥量';

COMMENT ON COLUMN MMSADM.ME_DOCD.BW_SQTY IS '借貨量';

COMMENT ON COLUMN MMSADM.ME_DOCD.PICK_QTY IS '揀料量';

COMMENT ON COLUMN MMSADM.ME_DOCD.PICK_USER IS '揀料人員';

COMMENT ON COLUMN MMSADM.ME_DOCD.PICK_TIME IS '揀料時間';

COMMENT ON COLUMN MMSADM.ME_DOCD.ONWAY_QTY IS '在途量';

COMMENT ON COLUMN MMSADM.ME_DOCD.APL_CONTIME IS '申請確認時間';

COMMENT ON COLUMN MMSADM.ME_DOCD.AMT IS '金額';

COMMENT ON COLUMN MMSADM.ME_DOCD.UP IS '單價';

COMMENT ON COLUMN MMSADM.ME_DOCD.RV_MQTY IS '戰備調撥歸墊';

COMMENT ON COLUMN MMSADM.ME_DOCD.GTAPL_RESON IS '申請超量理由';

COMMENT ON COLUMN MMSADM.ME_DOCD.APLYITEM_NOTE IS '申請單項次備註';

COMMENT ON COLUMN MMSADM.ME_DOCD.CREATE_TIME IS '建立時間';

COMMENT ON COLUMN MMSADM.ME_DOCD.CREATE_USER IS '建立人員代碼';

COMMENT ON COLUMN MMSADM.ME_DOCD.UPDATE_TIME IS '異動時間';

COMMENT ON COLUMN MMSADM.ME_DOCD.UPDATE_USER IS '異動人員代碼';

COMMENT ON COLUMN MMSADM.ME_DOCD.UPDATE_IP IS '異動IP';


CREATE UNIQUE INDEX MMSADM.DOCD_IDX1 ON MMSADM.ME_DOCD
(DOCNO, MMCODE)
LOGGING
TABLESPACE MMSMSTB01
PCTFREE    10
INITRANS   2
MAXTRANS   255
STORAGE    (
            INITIAL          64K
            NEXT             1M
            MINEXTENTS       1
            MAXEXTENTS       UNLIMITED
            PCTINCREASE      0
            BUFFER_POOL      DEFAULT
           )
NOPARALLEL;


DROP PUBLIC SYNONYM ME_DOCD;

CREATE PUBLIC SYNONYM ME_DOCD FOR MMSADM.ME_DOCD;


ALTER TABLE MMSADM.ME_DOCD ADD (
  PRIMARY KEY
 (DOCNO, SEQ)
    USING INDEX 
    TABLESPACE MMSMSTB01
    PCTFREE    10
    INITRANS   2
    MAXTRANS   255
    STORAGE    (
                INITIAL          64K
                NEXT             1M
                MINEXTENTS       1
                MAXEXTENTS       UNLIMITED
                PCTINCREASE      0
               ));

ALTER TABLE MMSADM.ME_DOCD ADD (
  FOREIGN KEY (DOCNO) 
 REFERENCES MMSADM.ME_DOCM (DOCNO),
  FOREIGN KEY (MMCODE) 
 REFERENCES MMSADM.MI_MAST (MMCODE));

GRANT DELETE, INSERT, SELECT, UPDATE ON MMSADM.ME_DOCD TO MMS_DEVAPP;

            ";
        }

        #endregion

        L l = new L("GenTools.Form1");

        public Form1()
        {
            InitializeComponent();
            load_listbox1();
            loadTableScript();
            gen();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Clipboard.SetData(DataFormats.Text, GenTools.OracleSchemaHelper.readDataTableSchema(listBox1.SelectedItem.ToString()));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Clipboard.SetData(DataFormats.Text, GenTools.OracleSchemaHelper.readOraTableScript(tbTableScript.Text));
        }

        private void buttonQry_Click(object sender, EventArgs e)
        {
            DataTable dtM = GenTools.OracleSchemaHelper.query(textBoxM.Text);
            dataGridViewM.DataSource = dtM;
            bindCol(dtM, ref dataGridViewFormM);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            gen();
        } // 


        // ------------------------------
        // -- gencode的部份 開始 --------
        // ------------------------------
        public Configuration config;
        public String sBaseOutputFolder;
        public String sModelsFilePath;
        public String sRepositoryFilePath;
        String sControllerFilePath;
        String sScriptsFilePath;
        void gen()
        {
            config = ConfigurationManager.OpenExeConfiguration(System.Windows.Forms.Application.StartupPath + "\\Schedule.config");
            sBaseOutputFolder = config.AppSettings.Settings["output_folder"].Value.ToString();

            sModelsFilePath = sBaseOutputFolder + "\\Models\\" + getModelsFolder(textBoxCodeName.Text) + "\\" + textBoxCodeName.Text + "Repository.cs";
            sRepositoryFilePath = sBaseOutputFolder + "\\Repository\\" + getRepositoryFolder(textBoxCodeName.Text) + "\\" + textBoxCodeName.Text + "Repository.cs";
            sControllerFilePath = sBaseOutputFolder + "\\Controllers\\" + getControllerFolder(textBoxCodeName.Text) + "\\" + textBoxCodeName.Text + "Controller.cs";
            sScriptsFilePath = sBaseOutputFolder + "\\Scripts\\" + getScriptsFolder(textBoxCodeName.Text) + "\\" + textBoxCodeName.Text + ".js";

            if (File.Exists(sModelsFilePath)) File.Delete(sModelsFilePath);
            if (File.Exists(sRepositoryFilePath)) File.Delete(sRepositoryFilePath);
            if (File.Exists(sControllerFilePath)) File.Delete(sControllerFilePath);
            if (File.Exists(sScriptsFilePath)) File.Delete(sScriptsFilePath);


            String s = "";
            l.writeFile(Path.GetDirectoryName(sModelsFilePath), sModelsFilePath, s);
            l.writeFile(Path.GetDirectoryName(sRepositoryFilePath), sRepositoryFilePath, s);
            l.writeFile(Path.GetDirectoryName(sControllerFilePath), sControllerFilePath, s);
            l.writeFile(Path.GetDirectoryName(sScriptsFilePath), sScriptsFilePath, new sample.single_page_01.Js().getCode(this));
        }
        // ------------------------------
        // -- gencode的部份 結束 --------
        // ------------------------------


    } // ec
} // en
