using System;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using System.Text;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;

namespace WebApp.Repository.AA
{
    public class AA0123ReportMODEL : JCLib.Mvc.BaseModel
    {
        public string YYYMM { get; set; }
        public string DOCNO { get; set; }
        public string ITEMNO { get; set; }
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string BASE_UNIT { get; set; }
        public string M_STOREID { get; set; }
        public float LAST_PRICE { get; set; }
        public int LAST_QTY { get; set; }
        public float APP_PRICE1 { get; set; }
        public int APP_QTY1 { get; set; }
        public float APP_PRICE2 { get; set; }
        public int APP_QTY2 { get; set; }
        public float NEW_PRICE { get; set; }
        public int NEW_QTY { get; set; }
        public float SUB_PRICE { get; set; }
        public string MAT_CLASS { get; set; }
        public string MAT_CLASS_T { get; set; }
        public string MAT_CLASS_N { get; set; }
        public string STAT { get; set; }
    }
    public class AA0123Repository : JCLib.Mvc.BaseRepository
    {
        public AA0123Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AA0123ReportMODEL> GetPrintData(string p0, string p1)
        {
            var p = new DynamicParameters();

            string sql = @"SELECT A.DOCNO,ROWNUM AS ITEMNO,A.MMCODE,C.MMNAME_C,C.MMNAME_E,C.BASE_UNIT,C.M_STOREID, 
                                  NVL((SELECT MIL_PRICE FROM MI_WHCOST WHERE MMCODE=A.MMCODE AND DATA_YM=TWN_PYM(SUBSTR(TWN_DATE(B.APPTIME),1,5))),0) AS LAST_PRICE,
                                  NVL(PMN_INVQTY(TWN_PYM(SUBSTR(TWN_DATE(B.APPTIME),1,5)),WHNO_MM5,A.MMCODE ),0) AS LAST_QTY,
                                  NVL((SELECT MIL_PRICE FROM MI_WHCOST WHERE MMCODE=A.MMCODE AND DATA_YM=SUBSTR(TWN_DATE(B.APPTIME),1,5)),0) AS APP_PRICE1,
                                  CASE WHEN A.STAT='1' THEN A.APPQTY ELSE 0 END AS APP_QTY1,
                                  NVL((SELECT MIL_PRICE FROM MI_WHCOST WHERE MMCODE=A.MMCODE AND DATA_YM=SUBSTR(TWN_DATE(B.APPTIME),1,5)),0) AS APP_PRICE2,
                                  CASE WHEN A.STAT='2' THEN A.APPQTY ELSE 0 END AS APP_QTY2,
                                  NVL((SELECT MIL_PRICE FROM MI_WHCOST WHERE MMCODE=A.MMCODE AND DATA_YM=SUBSTR(TWN_DATE(B.APPTIME),1,5)),0) AS NEW_PRICE,
                                  NVL(CASE WHEN A.STAT='1' THEN 
                                              NVL(PMN_INVQTY(TWN_PYM(SUBSTR(TWN_DATE(B.APPTIME),1,5)),WHNO_MM5,A.MMCODE ),0) + A.APPQTY 
                                           WHEN A.STAT='2' THEN 
                                              NVL(PMN_INVQTY(TWN_PYM(SUBSTR(TWN_DATE(B.APPTIME),1,5)),WHNO_MM5,A.MMCODE ),0) - A.APPQTY END,0) 
                                        AS NEW_QTY,
                                  0 AS SUB_PRICE,
                                  B.MAT_CLASS,
                                  D.MAT_CLSID AS MAT_CLASS_T,
                                  D.MAT_CLSNAME AS MAT_CLASS_N,
                                  A.STAT 
                             FROM ME_DOCD A, ME_DOCM B, MI_MAST C, MI_MATCLASS D   
                            WHERE A.MMCODE = C.MMCODE AND A.DOCNO=B.DOCNO AND B.DOCTYPE='XR1'
                              AND C.MAT_CLASS = D.MAT_CLASS 
                              AND SUBSTR(TWN_DATE(B.APPTIME),1,5) = :YYYMM 
                              AND D.MAT_CLSID = :CLSID 
                            ORDER BY MMCODE ";
            
            return DBWork.Connection.Query<AA0123ReportMODEL>(sql, new { YYYMM = p0, CLSID = p1 }, DBWork.Transaction);
        }
        public IEnumerable<AA0123ReportMODEL> GetPrintDataAll(string p0, string p1)
        {
            var p = new DynamicParameters();

            string sql = @"SELECT D.DOCNO,ROWNUM AS ITEMNO,D.MMCODE,E.MMNAME_C,E.MMNAME_E,E.BASE_UNIT,E.M_STOREID,
                                  D.LAST_PRICE,D.LAST_QTY,D.APP_PRICE1,D.APP_QTY1,D.APP_PRICE2,D.APP_QTY2,D.NEW_PRICE,D.NEW_QTY,
                                  D.SUB_PRICE,E.MAT_CLASS,F.MAT_CLSID AS MAT_CLASS_T,
                                  F.MAT_CLSNAME AS MAT_CLASS_N,D.STAT
                             FROM (
                                    SELECT A.DOCNO,A.MMCODE,
                                           NVL((SELECT MIL_PRICE FROM MI_WHCOST WHERE MMCODE=A.MMCODE AND DATA_YM=TWN_PYM(:YYYMM)),0) AS LAST_PRICE,
                                           NVL(PMN_INVQTY(:YYYMM,WHNO_MM5,A.MMCODE),0) AS LAST_QTY,
                                           NVL((SELECT MIL_PRICE FROM MI_WHCOST WHERE MMCODE=A.MMCODE AND DATA_YM=:YYYMM),0) AS APP_PRICE1,
                                           CASE WHEN A.STAT='1' THEN A.APPQTY ELSE 0 END AS APP_QTY1,
                                           NVL((SELECT MIL_PRICE FROM MI_WHCOST WHERE MMCODE=A.MMCODE AND DATA_YM=:YYYMM),0) AS APP_PRICE2,
                                           CASE WHEN A.STAT='2' THEN A.APPQTY ELSE 0 END AS APP_QTY2,
                                           NVL((SELECT MIL_PRICE FROM MI_WHCOST WHERE MMCODE=A.MMCODE AND DATA_YM=:YYYMM),0) AS NEW_PRICE,
                                           NVL(CASE WHEN A.STAT='1' THEN 
                                                       NVL(PMN_INVQTY(:YYYMM,WHNO_MM5,A.MMCODE ),0) + A.APPQTY 
                                                    WHEN A.STAT='2' THEN 
                                                       NVL(PMN_INVQTY(:YYYMM,WHNO_MM5,A.MMCODE ),0) - A.APPQTY END,0) 
                                                 AS NEW_QTY,
                                           0 AS SUB_PRICE,A.STAT
                                      FROM ME_DOCD A, ME_DOCM B
                                     WHERE A.DOCNO=B.DOCNO AND B.DOCTYPE='XR1'
                                           AND SUBSTR(TWN_DATE(B.APPTIME),1,5) = :YYYMM
                                           AND GET_MAT_CLSID(A.MMCODE)=:CLSID
                                    UNION ALL
                                    SELECT NULL DOCNO,MMCODE,
                                           NVL((SELECT MIL_PRICE FROM MI_WHCOST WHERE MMCODE=C.MMCODE AND DATA_YM=TWN_PYM(:YYYMM)),0) LAST_PRICE,
                                           NVL(PMN_INVQTY(:YYYMM,C.WH_NO,C.MMCODE),0) LAST_QTY,
                                           NVL((SELECT MIL_PRICE FROM MI_WHCOST WHERE MMCODE=C.MMCODE AND DATA_YM=:YYYMM),0) APP_PRICE1,0 APP_QTY1,
                                           NVL((SELECT MIL_PRICE FROM MI_WHCOST WHERE MMCODE=C.MMCODE AND DATA_YM=:YYYMM),0) APP_PRICE2,0 APP_QTY2,
                                           NVL((SELECT MIL_PRICE FROM MI_WHCOST WHERE MMCODE=C.MMCODE AND DATA_YM=:YYYMM),0) NEW_PRICE,
                                           NVL(PMN_INVQTY(:YYYMM,C.WH_NO,C.MMCODE),0) NEW_QTY,
                                           0 SUB_PRICE,NULL STAT
                                      FROM MI_WHINV C
                                     WHERE WH_NO=WHNO_MM5 AND INV_QTY>0 AND GET_MAT_CLSID(C.MMCODE)=:CLSID
                                           AND NOT EXISTS
                                           (SELECT 1 FROM ME_DOCD C1, ME_DOCM C2
                                            WHERE C1.DOCNO=C2.DOCNO AND C1.MMCODE=C.MMCODE AND C2.DOCTYPE='XR1'
                                                  AND SUBSTR(TWN_DATE(C2.APPTIME),1,5)=:YYYMM)
                                      ) D,MI_MAST E,MI_MATCLASS F
                            WHERE D.MMCODE=E.MMCODE AND E.MAT_CLASS=F.MAT_CLASS
                            ORDER BY D.MMCODE";

            return DBWork.Connection.Query<AA0123ReportMODEL>(sql, new { YYYMM = p0, CLSID = p1 }, DBWork.Transaction);
        }


        public IEnumerable<COMBO_MODEL> GetYmCombo()
        {
            string sql = @"SELECT DISTINCT SUBSTR(TWN_DATE(APPTIME),1,5) AS VALUE 
                        ,SUBSTR(TWN_DATE(APPTIME),1,5) AS TEXT
                        ,SUBSTR(TWN_DATE(APPTIME),1,5) AS COMBITEM
                        FROM ME_DOCM WHERE DOCTYPE='XR1' 
                        ORDER BY 1 DESC ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetMatclass2Combo()
        {
            string sql = @"select MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, 
                        MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM 
                        FROM MI_MATCLASS WHERE MAT_CLSID='2'  
                        ORDER BY MAT_CLASS";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetMatclass3Combo()
        {
            string sql = @"select MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, 
                        MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM 
                        FROM MI_MATCLASS WHERE MAT_CLSID='3'   
                        ORDER BY MAT_CLASS";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public string GetTaskid(string id)
        {
            string sql = @"SELECT TASK_ID FROM MI_WHID WHERE WH_USERID=:WH_USERID 
                            AND WH_NO IN (SELECT WH_NO FROM MI_WHMAST WHERE WH_KIND='1' AND WH_GRADE='1')";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { WH_USERID = id }, DBWork.Transaction).ToString();
            return rtn;
        }
        public IEnumerable<COMBO_MODEL> GetTaskCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='TASK_ID' 
                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<AB0003Model> GetLoginInfo(string id, string ip)
        {
            string sql = @"SELECT TUSER AS USERID, UNA AS USERNAME, INID, INID_NAME(INID) AS INIDNAME,
                        WHNO_MM1 CENTER_WHNO,INID_NAME(WHNO_MM1) AS CENTER_WHNAME, TO_CHAR(SYSDATE,'YYYYMMDD') AS TODAY,
                        :UPDATE_IP,
                        (SELECT COUNT(*) AS CNT FROM ME_DOCM 
                            WHERE DOCTYPE='MR2' AND APPLY_KIND='1' 
                            AND APPTIME BETWEEN NEXT_DAY(SYSDATE-7,1) AND NEXT_DAY(SYSDATE-7,1)+6 ) MR2,
                        NVL((SELECT 'Y' FROM DUAL WHERE (SELECT EXTRACT(DAY FROM SYSDATE) FROM DUAL) 
                            BETWEEN (SELECT DATA_VALUE FROM PARAM_D WHERE DATA_NAME = 'MR3_BDAY')
                            AND (SELECT DATA_VALUE FROM PARAM_D WHERE DATA_NAME = 'MR3_EDAY')),'N') MR3,
                        NVL((SELECT 'Y' FROM DUAL WHERE (SELECT EXTRACT(DAY FROM SYSDATE) FROM DUAL) 
                            BETWEEN (SELECT DATA_VALUE FROM PARAM_D WHERE DATA_NAME = 'MR4_BDAY')
                            AND (SELECT DATA_VALUE FROM PARAM_D WHERE DATA_NAME = 'MR4_EDAY')),'N') MR4,
                        NVL((SELECT TASK_ID FROM MI_WHID WHERE WH_USERID=UR_ID.TUSER 
                            AND WH_NO IN (SELECT WH_NO FROM MI_WHMAST WHERE WH_KIND='1' AND WH_GRADE='1')),'2')TASK_ID
                        FROM UR_ID
                        WHERE UR_ID.TUSER=:TUSER";

            return DBWork.Connection.Query<AB0003Model>(sql, new { TUSER = id, UPDATE_IP = ip });
        }
    }
}
