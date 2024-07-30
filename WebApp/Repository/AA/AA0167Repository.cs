using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using TSGH.Models;
using System.Collections.Generic;

namespace WebApp.Repository.AA
{

    public class AA0167_MODEL : JCLib.Mvc.BaseModel
    {
        public string DOCNO { get; set; }
        public string SEQ { get; set; }
        public string FLOW_ID { get; set; }
        public string MAT_CLSNAME { get; set; }
        public string APPDEPT { get; set; }
        public string APPTIME { get; set; }
        public string APPLY_NOTE { get; set; }
        public string STAT { get; set; }
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public int APVQTY { get; set; }
        public string BASE_UNIT { get; set; }
        public string DISC_CPRICE { get; set; }

    }
    public class AA0167Repository : JCLib.Mvc.BaseRepository
    {

        public AA0167Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AA0167_MODEL> GetAll(string APPTIME1, string APPTIME2, string MAT_CLASS, string DOCTYPE, string MMCODE, string USRID, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT MDM.DOCNO,
                               MDP.SEQ,
                               (SELECT DATA_DESC
                                FROM PARAM_D PD
                                WHERE     PD.DATA_VALUE = MDM.FLOWID
                                AND GRP_CODE = 'ME_DOCM'
                                AND DATA_NAME = 'FLOWID_EX1') FLOW_ID,
                               (SELECT MAT_CLSNAME
                                FROM MI_MATCLASS MMS
                                WHERE MMS.MAT_CLASS = MDM.MAT_CLASS) MAT_CLSNAME,
                               MDM.APPDEPT || ' ' || INID_NAME(MDM.APPDEPT) APPDEPT,
                               SUBSTR(TWN_DATE(MDM.APPTIME),0,5) APPTIME,
                               MDM.APPLY_NOTE,
                               (CASE
                                   WHEN MDP.APVQTY >= 0 THEN '入'
                                   WHEN MDP.APVQTY < 0 THEN '出'
                                END) STAT,
                               MDP.MMCODE,
                               (SELECT MMT.MMNAME_C
                                FROM MI_MAST MMT
                                WHERE MMT.MMCODE = MDP.MMCODE) MMNAME_C,
                               (SELECT MMT.MMNAME_E
                                FROM MI_MAST MMT
                                WHERE MMT.MMCODE = MDP.MMCODE) MMNAME_E,
                               ABS (MDP.APVQTY) APVQTY,
                               (SELECT MMT.BASE_UNIT
                                FROM MI_MAST MMT
                                WHERE MMT.MMCODE = MDP.MMCODE) BASE_UNIT,
                               (SELECT MWT.DISC_CPRICE
                                FROM MI_WHCOST MWT
                                WHERE MWT.MMCODE = MDP.MMCODE
                                AND MWT.DATA_YM = SUBSTR(TWN_DATE(MDM.APPTIME),0,5)) DISC_CPRICE
                        FROM ME_DOCM MDM, ME_DOCEXP MDP
                        WHERE     MDM.DOCNO = MDP.DOCNO
                        AND MDM.FRWH = WHNO_MM1";

            if (DOCTYPE == "")
            {
                sql += @" AND (MDM.DOCTYPE = 'EX1' OR MDM.DOCTYPE = 'RJ1') ";
            }
            else
            {
                sql += @" AND MDM.DOCTYPE = :DOCTYPE ";
                p.Add(":DOCTYPE", DOCTYPE);
            }

            if (APPTIME1 != "") //日期
            {
                sql += " AND TRUNC(MDM.APPTIME) >= TO_DATE(SUBSTR(:P0, 1, 10),'YYYY/MM/DD')  ";
                p.Add(":P0", APPTIME1);
            }
            if (APPTIME2 != "")
            {
                sql += " AND TRUNC(MDM.APPTIME) <= TO_DATE(SUBSTR(:P1, 1, 10),'YYYY/MM/DD')  ";
                p.Add(":P1", APPTIME2);
            }

            if (MAT_CLASS != "")  //物料分類
            {
                sql += " AND MDM.MAT_CLASS = :MAT_CLASS ";
                p.Add(":MAT_CLASS", MAT_CLASS);
            }
            else
            {
                sql += @"AND EXISTS(SELECT 1
                                    FROM MI_MATCLASS MMS
                                    WHERE MMS.MAT_CLSID = WHM1_TASK( :USRID)
                                    AND  MDM.MAT_CLASS = MMS.MAT_CLASS)";
                p.Add(":USRID", USRID);
            }

            if (MMCODE != "")    //院內碼
            {
                sql += "AND MDP.MMCODE = :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}", MMCODE));
            }

            sql += " ORDER BY MDM.APPTIME, MDM.DOCNO, MDP.SEQ";


            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0167_MODEL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public DataTable GetExcel(string APPTIME1, string APPTIME2, string MAT_CLASS, string DOCTYPE, string MMCODE, string USRID)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT MDM.DOCNO 單據號碼,
                               (SELECT DATA_DESC
                                FROM PARAM_D PD
                                WHERE     PD.DATA_VALUE = MDM.FLOWID
                                AND GRP_CODE = 'ME_DOCM'
                                AND DATA_NAME = 'FLOWID_EX1') 申請單狀態,
                               (SELECT MAT_CLSNAME
                                FROM MI_MATCLASS MMS
                                WHERE MMS.MAT_CLASS = MDM.MAT_CLASS) 物料類別,
                               MDM.APPDEPT || ' ' || INID_NAME(MDM.APPDEPT) 申請部門,
                               SUBSTR(TWN_DATE(MDM.APPTIME),0,5) 申請日期,
                               MDM.APPLY_NOTE 申請單備註,
                               (CASE
                                   WHEN MDP.APVQTY >= 0 THEN '入'
                                   WHEN MDP.APVQTY < 0 THEN '出'
                                END) ""出/入"",
                               MDP.MMCODE 院內碼,
                               (SELECT MMT.MMNAME_C
                                FROM MI_MAST MMT
                                WHERE MMT.MMCODE = MDP.MMCODE) 中文品名,
                               (SELECT MMT.MMNAME_E
                                FROM MI_MAST MMT
                                WHERE MMT.MMCODE = MDP.MMCODE) 英文品名,
                               ABS (MDP.APVQTY) 數量,
                               (SELECT MMT.BASE_UNIT
                                FROM MI_MAST MMT
                                WHERE MMT.MMCODE = MDP.MMCODE) 單位,
                               (SELECT MWT.DISC_CPRICE
                                FROM MI_WHCOST MWT
                                WHERE     MWT.MMCODE = MDP.MMCODE
                                AND MWT.DATA_YM = SUBSTR(TWN_DATE(MDM.APPTIME),0,5)) 優惠合約單價
                        FROM ME_DOCM MDM, ME_DOCEXP MDP
                        WHERE     MDM.DOCNO = MDP.DOCNO
                        AND MDM.FRWH = WHNO_MM1";

            if (DOCTYPE == "")
            {
                sql += @" AND (MDM.DOCTYPE = 'EX1' OR MDM.DOCTYPE = 'RJ1') ";
            }
            else
            {
                sql += @" AND MDM.DOCTYPE = :DOCTYPE ";
                p.Add(":DOCTYPE", DOCTYPE);
            }

            if (APPTIME1 != "") //日期
            {
                sql += " AND TRUNC (MDM.APPTIME) >= TO_DATE ( :P0 + 19110000, 'YYYY/MM/DD')  ";
                p.Add(":P0", APPTIME1);
            }
            if (APPTIME2 != "")
            {
                sql += " AND TRUNC (MDM.APPTIME) <= TO_DATE ( :P1 + 19110000, 'YYYY/MM/DD')  ";
                p.Add(":P1", APPTIME2);
            }

            if (MAT_CLASS != "")  //物料分類
            {
                sql += " AND MDM.MAT_CLASS = :MAT_CLASS ";
                p.Add(":MAT_CLASS", MAT_CLASS);
            }
            else
            {
                sql += @"AND EXISTS(SELECT 1
                                    FROM MI_MATCLASS MMS
                                    WHERE MMS.MAT_CLSID = WHM1_TASK( :USRID)
                                    AND  MDM.MAT_CLASS = MMS.MAT_CLASS)";
                p.Add(":USRID", USRID);
            }

            if (MMCODE != "")    //院內碼
            {
                sql += "AND MDP.MMCODE = :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}", MMCODE));
            }

            sql += " ORDER BY MDM.APPTIME, MDM.DOCNO, MDP.SEQ";



            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<ComboItemModel> GetMatClass()
        {
            string sql = @"SELECT MAT_CLASS VALUE,
                                  MAT_CLASS || ' ' || MAT_CLSNAME TEXT
                           FROM MI_MATCLASS
                           WHERE MAT_CLSID  IN ('2','3')";
            return DBWork.Connection.Query<ComboItemModel>(sql, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMMCODECombo(string mmcode, string matclass, int page_index, int page_size, string sorters)       //AA0061 查詢
        {
            var p = new DynamicParameters();

            string sql = @"SELECT DISTINCT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E " +
                "from MI_MAST A JOIN ME_DOCD B ON (A.MMCODE = B.MMCODE), ME_DOCM C, MI_WHINV D " +
                "WHERE C.FRWH = WHNO_MM1 AND C.FRWH = D.WH_NO AND B.MMCODE = D.MMCODE AND D.INV_QTY <> 0 ";

            if (matclass != "" && matclass != null)  //物料分類
            {
                string[] tmp = matclass.Split(',');
                sql += "AND A.MAT_CLASS IN :mat_class ";
                p.Add(":mat_class", tmp);
            }

            if (mmcode != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(UPPER(A.MMCODE), UPPER(:MMCODE_I)), 1000) + NVL(INSTR(UPPER(MMNAME_E), UPPER(:MMNAME_E_I)), 100) * 10 + NVL(INSTR(UPPER(MMNAME_C), UPPER(:MMNAME_C_I)), 100) * 10) IDX,");
                p.Add(":MMCODE_I", mmcode);
                p.Add(":MMNAME_E_I", mmcode);
                p.Add(":MMNAME_C_I", mmcode);

                sql += " AND (UPPER(A.MMCODE) LIKE UPPER(:MMCODE) ";
                p.Add(":MMCODE", string.Format("{0}%", mmcode));

                sql += " OR UPPER(MMNAME_E) LIKE UPPER(:MMNAME_E) ";
                p.Add(":MMNAME_E", string.Format("%{0}%", mmcode));

                sql += " OR UPPER(MMNAME_C) LIKE UPPER(:MMNAME_C)) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", mmcode));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY MMCODE ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<AA0167_MODEL> GetPrintData(string APPTIME1, string APPTIME2, string MAT_CLASS, string DOCTYPE, string MMCODE, string USRID)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT MDM.DOCNO,
                               (SELECT DATA_DESC
                                FROM PARAM_D PD
                                WHERE     PD.DATA_VALUE = MDM.FLOWID
                                AND GRP_CODE = 'ME_DOCM'
                                AND DATA_NAME = 'FLOWID_EX1') FLOW_ID,
                               (SELECT MAT_CLSNAME
                                FROM MI_MATCLASS MMS
                                WHERE MMS.MAT_CLASS = MDM.MAT_CLASS) MAT_CLSNAME,
                               MDM.APPDEPT || ' ' || INID_NAME(MDM.APPDEPT) APPDEPT,
                               SUBSTR(TWN_DATE(MDM.APPTIME),0,5) APPTIME,
                               MDM.APPLY_NOTE,
                               (CASE
                                   WHEN MDP.APVQTY >= 0 THEN '入'
                                   WHEN MDP.APVQTY < 0 THEN '出'
                                END) STAT,
                               MDP.MMCODE,
                               (SELECT CASE 
                                           WHEN MMT.MMNAME_C = '' AND MMT.MMNAME_E = '' THEN ''
                                           WHEN MMT.MMNAME_C = '' AND MMT.MMNAME_E <> '' THEN MMT.MMNAME_E
                                           WHEN MMT.MMNAME_C <> '' AND MMT.MMNAME_E = '' THEN MMT.MMNAME_C
                                           ELSE MMT.MMNAME_C || CHR(10) || MMT.MMNAME_E
                                       END
                                FROM MI_MAST MMT
                                WHERE MMT.MMCODE = MDP.MMCODE) MMNAME_E,
                               ABS (MDP.APVQTY) APVQTY,
                               (SELECT MMT.BASE_UNIT
                                FROM MI_MAST MMT
                                WHERE MMT.MMCODE = MDP.MMCODE) BASE_UNIT,
                               (SELECT MWT.DISC_CPRICE
                                FROM MI_WHCOST MWT
                                WHERE MWT.MMCODE = MDP.MMCODE
                                AND MWT.DATA_YM =SUBSTR(TWN_DATE(MDM.APPTIME),0,5)) DISC_CPRICE
                        FROM ME_DOCM MDM, ME_DOCEXP MDP
                        WHERE     MDM.DOCNO = MDP.DOCNO
                        AND MDM.FRWH = WHNO_MM1";

            if (DOCTYPE == "")
            {
                sql += @" AND (MDM.DOCTYPE = 'EX1' OR MDM.DOCTYPE = 'RJ1') ";
            }
            else
            {
                sql += @" AND MDM.DOCTYPE = :DOCTYPE ";
                p.Add(":DOCTYPE", DOCTYPE);
            }

            if (APPTIME1 != "") //日期
            {
                sql += " AND TRUNC(MDM.APPTIME) >= TO_DATE ( :P0 + 19110000, 'YYYY/MM/DD')  ";
                p.Add(":P0", APPTIME1);
            }
            if (APPTIME2 != "")
            {
                sql += " AND TRUNC(MDM.APPTIME) <= TO_DATE ( :P1 + 19110000, 'YYYY/MM/DD')  ";
                p.Add(":P1", APPTIME2);
            }

            if (MAT_CLASS != "")  //物料分類
            {
                sql += " AND MDM.MAT_CLASS = :MAT_CLASS ";
                p.Add(":MAT_CLASS", MAT_CLASS);
            }
            else
            {
                sql += @"AND EXISTS(SELECT 1
                                    FROM MI_MATCLASS MMS
                                    WHERE MMS.MAT_CLSID = WHM1_TASK( :USRID)
                                    AND  MDM.MAT_CLASS = MMS.MAT_CLASS)";
                p.Add(":USRID", USRID);
            }

            if (MMCODE != "")    //院內碼
            {
                sql += "AND MDP.MMCODE = :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}", MMCODE));
            }

            sql += " ORDER BY MDM.APPTIME, MDM.DOCNO, MDP.SEQ";

            return DBWork.Connection.Query<AA0167_MODEL>(sql, p, DBWork.Transaction);
        }

        public string GetINIDNAME(string USRID)
        {
            var sql = @"SELECT INID_NAME (USER_INID ( :USRID)) INID_NAME_USER FROM DUAL";

            return DBWork.Connection.QueryFirst<string>(sql, new { USRID = USRID }, DBWork.Transaction);
        }
    }
}