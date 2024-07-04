using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace WebApp.Repository.AA
{
    public class AA0026Repository : JCLib.Mvc.BaseRepository
    {
        public AA0026Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ME_DOCM> GetAllM(string apptime1, string apptime2, string mmcode, string frwh, string agenno, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.* ,
                        (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='FLOWID_RJ' AND DATA_VALUE=A.FLOWID) FLOWID_N,
                        (SELECT MAT_CLSNAME FROM MI_MATCLASS WHERE MAT_CLASS=A.MAT_CLASS) MAT_CLASS_N,
                        (SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.FRWH) FRWH_N,
                        (SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.TOWH) TOWH_N,
                        (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='APPLY_KIND' AND DATA_VALUE=A.APPLY_KIND) APPLY_KIND_N ,
                        (SELECT UNA FROM UR_ID WHERE TUSER=A.APPID) APP_NAME,
                        (SELECT INID_NAME FROM UR_INID WHERE INID=A.APPDEPT) APPDEPT_NAME,
                        (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='STKTRANSKIND' AND DATA_VALUE=A.STKTRANSKIND) STKTRANSKIND_N,
                        (SELECT AGEN_NAMEC FROM PH_VENDER WHERE AGEN_NO = A.AGEN_NO)AGEN_NO_N,
                        TWN_DATE(A.APPTIME) APPTIME_T,
                        (SELECT SUM(X.C_AMT * -1) FROM ME_DOCEXP X, MI_MAST Y WHERE X.DOCNO=A.DOCNO AND X.MMCODE=Y.MMCODE AND X.APVQTY < 0) SUM_EX,
                        (SELECT SUM(X.C_AMT) FROM ME_DOCEXP X, MI_MAST Y WHERE X.DOCNO=A.DOCNO AND X.MMCODE=Y.MMCODE AND X.APVQTY > 0) SUM_IN  
                        FROM ME_DOCM A 
                        WHERE 1=1 AND A.DOCTYPE = 'RJ' AND FRWH = 'PH1S' AND MAT_CLASS = '01' ";

            if (apptime1 != "" & apptime2 != "")
            {
                sql += " AND TWN_DATE(A.APPTIME) BETWEEN :d0 AND :d1 ";
                p.Add(":d0", string.Format("{0}", apptime1));
                p.Add(":d1", string.Format("{0}", apptime2));
            }
            if (apptime1 != "" & apptime2 == "")
            {
                sql += " AND TWN_DATE(A.APPTIME) >= :d0 ";
                p.Add(":d0", string.Format("{0}", apptime1));
            }
            if (apptime1 == "" & apptime2 != "")
            {
                sql += " AND TWN_DATE(A.APPTIME) <= :d1 ";
                p.Add(":d1", string.Format("{0}", apptime2));
            }
            if (mmcode != "")
            {
                sql += " AND EXISTS (SELECT 1 FROM ME_DOCEXP WHERE DOCNO=A.DOCNO AND MMCODE =:p2) ";
                p.Add(":p2", string.Format("{0}", mmcode));
            }
            if (frwh != "")
            {
                sql += " AND A.FRWH = :p3 ";
                p.Add(":p3", string.Format("{0}", frwh));
            }
            if (agenno != "")
            {
                sql += "  AND EXISTS (SELECT 1 FROM ME_DOCEXP B,MI_MAST C " +
                    " WHERE B.MMCODE=C.MMCODE AND B.DOCNO=A.DOCNO AND C.M_AGENNO =:p4) ";
                p.Add(":p4", string.Format("{0}", agenno));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCM>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<ME_DOCEXP> GetAllD(string DOCNO, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.DOCNO,A.SEQ,A.EXP_DATE,A.LOT_NO,A.LOT_NO LOT_NO_N,TWN_DATE(A.EXP_DATE) EXP_DATE_T,
                        A.MMCODE,A.C_TYPE,A.C_STATUS,A.C_RNO,A.C_UP,
                        (CASE WHEN A.APVQTY < 0 THEN -1 ELSE 1 END) * A.APVQTY APVQTY, 
                        (CASE WHEN A.APVQTY < 0 THEN -1 ELSE 1 END) * A.C_AMT C_AMT,
                        ( SELECT INV_QTY FROM MI_WEXPINV WHERE MMCODE = A.MMCODE AND WH_NO ='PH1S'  AND LOT_NO=A.LOT_NO) AS INV_QTY,
                        B.MMNAME_C, B.MMNAME_E, B.BASE_UNIT,B.M_CONTPRICE,
                        (SELECT DATA_DESC FROM PARAM_D 
                        WHERE GRP_CODE='ME_DOCD' AND DATA_NAME='STAT' 
                        AND DATA_VALUE=(CASE WHEN A.APVQTY < 0 THEN '2' ELSE '1' END)) INOUT_N,
                        (CASE WHEN A.APVQTY < 0 THEN '2' ELSE '1' END) INOUT,
                        ( SELECT SUM( X.APVQTY * Y.M_CONTPRICE ) FROM ME_DOCEXP X,MI_MAST Y WHERE X.MMCODE = Y.MMCODE AND X.DOCNO = A.DOCNO ) BALANCE,
                        ITEM_NOTE,
                        (SELECT PVR.AGEN_NO || ' ' || PVR.AGEN_NAMEC
                        FROM MI_MAST MMT, PH_VENDER PVR
                        WHERE MMT.MMCODE = A.MMCODE
                        AND MMT.M_AGENNO = PVR.AGEN_NO) AGEN_NAMEC ,
                        NVL((SELECT SUM(X.APVQTY * Y.M_CONTPRICE * -1) FROM ME_DOCEXP X, MI_MAST Y WHERE X.DOCNO=A.DOCNO AND X.MMCODE=Y.MMCODE AND X.APVQTY < 0),0) SUM_EX,
                        NVL((SELECT SUM(X.APVQTY * Y.M_CONTPRICE) FROM ME_DOCEXP X, MI_MAST Y WHERE X.DOCNO=A.DOCNO AND X.MMCODE=Y.MMCODE AND X.APVQTY > 0),0) SUM_IN,
                        NVL(DECODE (A.C_TYPE, 2, '', A.C_UP),0) IN_PRICE,
                        NVL(DECODE (A.C_TYPE, 1, '', A.C_UP),0) CONTPRICE  
                        FROM ME_DOCEXP A
                        INNER JOIN MI_MAST B ON B.MMCODE=A.MMCODE
                        INNER JOIN ME_DOCM C ON C.DOCNO=A.DOCNO
                        WHERE 1=1 ";

            if (DOCNO != "")
            {
                sql += " AND A.DOCNO = :p0 ";
                p.Add(":p0", string.Format("{0}", DOCNO));
            }
            else
            {
                sql += " AND 1=2 ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCEXP>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<ME_DOCM> GetM(string id)
        {
            var sql = @"SELECT A.* ,
                        (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='FLOWID_RJ' AND DATA_VALUE=A.FLOWID) FLOWID_N,
                        (SELECT MAT_CLSNAME FROM MI_MATCLASS WHERE MAT_CLASS=A.MAT_CLASS) MAT_CLASS_N,
                        (SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.FRWH) FRWH_N,
                        (SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.TOWH) TOWH_N,
                        (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='APPLY_KIND' AND DATA_VALUE=A.APPLY_KIND) APPLY_KIND_N ,
                        (SELECT UNA FROM UR_ID WHERE TUSER=A.APPID) APP_NAME,
                        (SELECT INID_NAME FROM UR_INID WHERE INID=A.APPDEPT) APPDEPT_NAME,
                        (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='STKTRANSKIND' AND DATA_VALUE=A.STKTRANSKIND) STKTRANSKIND_N,
                        (SELECT AGEN_NAMEC FROM PH_VENDER WHERE AGEN_NO = A.AGEN_NO)AGEN_NO_N,
                        TWN_DATE(A.APPTIME) APPTIME_T,
                        (SELECT SUM(X.APPQTY*Y.M_CONTPRICE) FROM ME_DOCD X, MI_MAST Y WHERE X.DOCNO=A.DOCNO AND X.MMCODE=Y.MMCODE AND STAT='2') SUM_EX,
                        (SELECT SUM(X.APPQTY*Y.M_CONTPRICE) FROM ME_DOCD X, MI_MAST Y WHERE X.DOCNO=A.DOCNO AND X.MMCODE=Y.MMCODE AND STAT='1') SUM_IN  
                        FROM ME_DOCM A WHERE 1=1 AND A.DOCTYPE = 'RJ' AND FRWH = 'PH1S' AND MAT_CLASS = '01'
                        AND A.DOCNO = :DOCNO";
            return DBWork.Connection.Query<ME_DOCM>(sql, new { DOCNO = id }, DBWork.Transaction);
        }
        public IEnumerable<ME_DOCEXP> GetD(string id)
        {
            var sql = @"SELECT A.DOCNO,A.SEQ,A.EXP_DATE,A.LOT_NO,A.LOT_NO LOT_NO_N,TWN_DATE(A.EXP_DATE) EXP_DATE_T,
                        A.MMCODE,A.C_TYPE,A.C_STATUS,A.C_RNO,A.C_UP,
                        (CASE WHEN A.APVQTY < 0 THEN -1 ELSE 1 END) * A.APVQTY APVQTY, 
                        (CASE WHEN A.APVQTY < 0 THEN -1 ELSE 1 END) * A.C_AMT C_AMT,
                        ( SELECT INV_QTY FROM MI_WEXPINV WHERE MMCODE = A.MMCODE AND WH_NO ='PH1S'  AND LOT_NO=A.LOT_NO) AS INV_QTY,
                        B.MMNAME_C, B.MMNAME_E, B.BASE_UNIT,B.M_CONTPRICE,
                        (SELECT DATA_DESC FROM PARAM_D 
                        WHERE GRP_CODE='ME_DOCD' AND DATA_NAME='STAT' 
                        AND DATA_VALUE=(CASE WHEN A.APVQTY < 0 THEN '2' ELSE '1' END)) INOUT_N,
                        (CASE WHEN A.APVQTY < 0 THEN '2' ELSE '1' END) INOUT,
                        ( SELECT SUM( X.APVQTY * Y.M_CONTPRICE ) FROM ME_DOCEXP X,MI_MAST Y WHERE X.MMCODE = Y.MMCODE AND X.DOCNO = A.DOCNO ) BALANCE,
                        ITEM_NOTE,
                        (SELECT PVR.AGEN_NO || ' ' || PVR.AGEN_NAMEC
                        FROM MI_MAST MMT, PH_VENDER PVR
                        WHERE MMT.MMCODE = A.MMCODE
                        AND MMT.M_AGENNO = PVR.AGEN_NO) AGEN_NAMEC ,
                        NVL((SELECT SUM(X.APVQTY * Y.M_CONTPRICE * -1) FROM ME_DOCEXP X, MI_MAST Y WHERE X.DOCNO=A.DOCNO AND X.MMCODE=Y.MMCODE AND X.APVQTY < 0),0) SUM_EX,
                        NVL((SELECT SUM(X.APVQTY * Y.M_CONTPRICE) FROM ME_DOCEXP X, MI_MAST Y WHERE X.DOCNO=A.DOCNO AND X.MMCODE=Y.MMCODE AND X.APVQTY > 0),0) SUM_IN,
                        NVL(DECODE (A.C_TYPE, 2, '', A.C_UP),0) IN_PRICE,
                        NVL(DECODE (A.C_TYPE, 1, '', A.C_UP),0) CONTPRICE  
                        FROM ME_DOCEXP A
                        INNER JOIN MI_MAST B ON B.MMCODE=A.MMCODE
                        INNER JOIN ME_DOCM C ON C.DOCNO=A.DOCNO
                        WHERE 1=1 
                        AND A.DOCNO = :DOCNO";
            return DBWork.Connection.Query<ME_DOCEXP>(sql, new { DOCNO = id }, DBWork.Transaction);
        }

        public int CreateM(ME_DOCM ME_DOCM)
        {
            var sql = @"INSERT INTO ME_DOCM (
                        DOCNO, DOCTYPE, FLOWID , APPID , APPDEPT , 
                        APPTIME , USEID , USEDEPT , FRWH , TOWH , 
                        APPLY_KIND ,APPLY_NOTE ,MAT_CLASS,STKTRANSKIND,AGEN_NO,
                        CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                      VALUES (
                        :DOCNO, :DOCTYPE, :FLOWID , :APPID , :APPDEPT , 
                        TO_DATE(:APPTIME,'YYYY/MM/DD') , :USEID , :USEDEPT , :FRWH , :TOWH , 
                        :APPLY_KIND , :APPLY_NOTE ,:MAT_CLASS, :STKTRANSKIND, :AGEN_NO,
                        SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);
        }

        public int CreateE(ME_DOCEXP docexp)
        {
            var sql = @"INSERT INTO ME_DOCEXP (
                        DOCNO, SEQ, MMCODE , APVQTY , ITEM_NOTE, 
                        C_UP, C_AMT, EXP_DATE, LOT_NO,
                        UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                      VALUES (
                        :DOCNO, :SEQ, :MMCODE , :APVQTY , :ITEM_NOTE, 
                        :C_UP, :C_AMT, TO_DATE(:EXP_DATE,'YYYY/MM/DD'), :LOT_NO,
                        SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, docexp, DBWork.Transaction);
        }
        public int CreateD(ME_DOCD ME_DOCD)
        {
            var sql = @"INSERT INTO ME_DOCD (
                        DOCNO, SEQ, MMCODE , APPQTY , APLYITEM_NOTE, STAT, 
                        CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                      VALUES (
                        :DOCNO, :SEQ, :MMCODE , :APPQTY , :APLYITEM_NOTE, :STAT,
                        SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, ME_DOCD, DBWork.Transaction);
        }
        public int CreateDE(ME_DOCD ME_DOCD)
        {
            var sql = @"INSERT INTO ME_DOCE (
                        DOCNO, SEQ, MMCODE , EXPDATE , LOT_NO, APVQTY,
                         CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                      VALUES (
                        :DOCNO, :SEQ, :MMCODE , TO_DATE(:EXPDATE,'YYYY/MM/DD') , :LOT_NO,  :APPQTY,
                        SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, ME_DOCD, DBWork.Transaction);
        }
        public int UpdateM(ME_DOCM ME_DOCM)
        {
            var sql = @"UPDATE ME_DOCM SET 
                        APPTIME = TO_DATE(:APPTIME,'YYYY/MM/DD'),
                        APPLY_NOTE = :APPLY_NOTE,STKTRANSKIND = :STKTRANSKIND,AGEN_NO=:AGEN_NO,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);
        }
        public int UpdateE(ME_DOCEXP docexp)
        {
            var sql = @"UPDATE ME_DOCEXP SET MMCODE = :MMCODE, 
                        APVQTY = :APVQTY, ITEM_NOTE = :ITEM_NOTE, C_UP = :C_UP, C_AMT = :C_AMT,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO AND SEQ = :SEQ ";
            return DBWork.Connection.Execute(sql, docexp, DBWork.Transaction);
        }
        public int UpdateD(ME_DOCD ME_DOCD)
        {
            var sql = @"UPDATE ME_DOCD SET MMCODE = :MMCODE, 
                        APPQTY = :APPQTY, APLYITEM_NOTE = :APLYITEM_NOTE,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO AND SEQ = :SEQ ";
            return DBWork.Connection.Execute(sql, ME_DOCD, DBWork.Transaction);
        }
        public int UpdateDE(ME_DOCD ME_DOCD)
        {
            var sql = @"UPDATE ME_DOCE SET MMCODE = :MMCODE, LOT_NO = :LOT_NO,
                        APVQTY = :APPQTY,  EXPDATE = TO_DATE(:EXPDATE,'YYYY/MM/DD'),
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO AND SEQ = :SEQ ";
            return DBWork.Connection.Execute(sql, ME_DOCD, DBWork.Transaction);
        }

        public int UpdateMeDocd(ME_DOCD ME_DOCD)
        {
            var sql = @"UPDATE ME_DOCD SET APPQTY = :APPQTY,  APLYITEM_NOTE = :APLYITEM_NOTE, 
                                UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE DOCNO = :DOCNO AND SEQ = :SEQ";
            return DBWork.Connection.Execute(sql, ME_DOCD, DBWork.Transaction);
        }
        public int ApplyM(ME_DOCM me_docm)
        {
            var sql = @"UPDATE ME_DOCM SET FLOWID = '0999',
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);
        }
        public int ApplyD(ME_DOCM ME_DOCM)
        {
            var sql = @"UPDATE ME_DOCD SET APL_CONTIME = SYSDATE,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);
        }
        public int DeleteM(ME_DOCM me_docm)
        {
            var sql = @"UPDATE ME_DOCM SET FLOWID = 'X',
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);
        }
        public int DeleteAllD(string docno)
        {
            var sql = @" DELETE from ME_DOCD WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public int DeleteD(string docno, string seq)
        {
            var sql = @" DELETE from ME_DOCD WHERE DOCNO=:DOCNO AND SEQ=:SEQ";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno, SEQ = seq }, DBWork.Transaction);
        }
        public int DeleteD_DOCEXP(string docno, string seq)
        {
            var sql = @" DELETE from ME_DOCEXP WHERE DOCNO=:DOCNO AND SEQ=:SEQ";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno, SEQ = seq }, DBWork.Transaction);
        }
        public IEnumerable<AA0123ReportMODEL> GetPrintData(string p0)
        {
            var p = new DynamicParameters();

            string sql = @"SELECT A.DOCNO,ROWNUM AS ITEMNO,A.MMCODE,C.MMNAME_C,C.MMNAME_E,C.BASE_UNIT,C.M_STOREID, 
                    NVL((SELECT MIL_PRICE FROM MI_WHCOST WHERE MMCODE=A.MMCODE AND DATA_YM=SUBSTR(TWN_DATE(ADD_MONTHS(B.APPTIME,-1)),1,5)),0) AS LAST_PRICE,
                    NVL((SELECT INV_QTY FROM MI_WINVMON WHERE MMCODE=A.MMCODE AND DATA_YM=SUBSTR(TWN_DATE(ADD_MONTHS(B.APPTIME,-1)),1,5) AND WH_NO IN (
                                                SELECT WH_NO  FROM MI_WHMAST  WHERE 1=1 AND WH_KIND = '1' AND WH_GRADE = '5'  AND ROWNUM=1 )),0) AS LAST_QTY,
                    0 AS APP_PRICE1,
                    CASE WHEN A.STAT='1' THEN A.APPQTY ELSE 0 END AS APP_QTY1,
                    NVL((SELECT MIL_PRICE FROM MI_WHCOST WHERE MMCODE=A.MMCODE AND DATA_YM=SUBSTR(TWN_DATE(B.APPTIME),1,5)),0) AS APP_PRICE2,
                    A.APPQTY AS APP_QTY2,
                    NVL((SELECT MIL_PRICE FROM MI_WHCOST WHERE MMCODE=A.MMCODE AND DATA_YM=SUBSTR(TWN_DATE(B.APPTIME),1,5)),0) AS NEW_PRICE,
                    NVL(
                    (SELECT INV_QTY FROM MI_WINVMON WHERE MMCODE=A.MMCODE AND DATA_YM=SUBSTR(TWN_DATE(ADD_MONTHS(B.APPTIME,-1)),1,5) AND WH_NO IN (
                                                SELECT WH_NO  FROM MI_WHMAST  WHERE 1=1 AND WH_KIND = '1' AND WH_GRADE = '5'  AND ROWNUM=1 )) - A.APPQTY ,0) AS NEW_QTY,
                    0 AS SUB_PRICE,
                    B.MAT_CLASS,
                    D.MAT_CLSID AS MAT_CLASS_T,
                    D.MAT_CLSNAME AS MAT_CLASS_N,
                    A.STAT 
                    FROM ME_DOCD A, ME_DOCM B, MI_MAST C, MI_MATCLASS D   
                    WHERE A.MMCODE = C.MMCODE AND A.DOCNO=B.DOCNO AND B.DOCTYPE='EX1'
                    AND C.MAT_CLASS = D.MAT_CLASS  
                    AND A.DOCNO = :DOCNO 
                    ORDER BY MMCODE ";

            return DBWork.Connection.Query<AA0123ReportMODEL>(sql, new { DOCNO = p0 }, DBWork.Transaction);
        }
        public bool CheckExists(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCM WHERE DOCNO=:DOCNO";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        public bool CheckExistsM(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCM WHERE DOCNO=:DOCNO AND FLOWID <> '0901'";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        public bool CheckExistsE(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCEXP WHERE DOCNO=:DOCNO ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        public bool CheckExistsD(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        public bool CheckExistsDD(string id, string sid)
        {
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO AND SEQ=:SEQ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id, SEQ = sid }, DBWork.Transaction) == null);
        }
        public bool CheckExistsDN(string id)
        {
            //string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO AND ( EXPT_DISTQTY = 0 OR BW_MQTY = 0 ) ";
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO AND APPQTY = 0  ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        public bool CheckExistsMM(string id, string mm, string st)
        {
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO AND MMCODE=:MMCODE AND STAT=:STAT";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id, MMCODE = mm, STAT = st }, DBWork.Transaction) == null);
        }
        public string GetDocno()
        {
            var p = new OracleDynamicParameters();
            p.Add("O_DOCNO", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 12);

            DBWork.Connection.Query("GET_DOCNO", p, commandType: CommandType.StoredProcedure);
            return p.Get<OracleString>("O_DOCNO").Value;
        }
        public string GetUridInid(string id)
        {
            string sql = @"SELECT INID FROM UR_ID WHERE TUSER=:TUSER ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { TUSER = id }, DBWork.Transaction).ToString();
            return rtn;
        }
        public string GetTwnsystime()
        {
            string sql = @"SELECT TWN_SYSTIME FROM DUAL ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction).ToString();
            return rtn;
        }

        public string GetDocESeq(string id)
        {
            string sql = @"SELECT MAX(SEQ)+1 as SEQ FROM ME_DOCEXP WHERE DOCNO=:DOCNO ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction).ToString();
            return rtn;
        }
        public string Getdate(string dt)
        {
            string sql = @"SELECT substr(:pDate,1,3) + 1911 ||'/'||substr(:pDate,4,2)||'/'||substr(:pDate,6,2) GETDATE FROM DUAL ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { pDate = dt }, DBWork.Transaction).ToString();
            return rtn;
        }
        public string GetFrwh()
        {
            string sql = @"SELECT WH_NO 
                        FROM MI_WHMAST 
                        WHERE 1=1 AND WH_KIND = '1' AND WH_GRADE = '1' AND ROWNUM=1";
            string rtn = DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction).ToString();
            return rtn;
        }

        public DateTime Getdatetime(string dt)
        {
            string sql = @"SELECT TO_DATE(:pDate || '00:00:00','YYYY/MM/DD HH24:MI:SS') TWNDATE FROM DUAL ";
            DateTime rtn = Convert.ToDateTime(DBWork.Connection.ExecuteScalar(sql, new { pDate = dt }, DBWork.Transaction));
            return rtn;
        }
        public string GetDocDSeq(string id)
        {
            string sql = @"SELECT MAX(SEQ)+1 as SEQ FROM ME_DOCD WHERE DOCNO=:DOCNO ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction).ToString();
            return rtn;
        }

        public string GetDocENum(string id)
        {
            string sql = @"SELECT COUNT(*)NUM FROM ME_DOCEXP WHERE DOCNO=:DOCNO ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction).ToString();
            return rtn;
        }
        public string GetDocEMmcode(string doc, string seq)
        {
            string sql = @"SELECT MMCODE FROM ME_DOCEXP WHERE DOCNO=:DOCNO AND SEQ = :SEQ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { DOCNO = doc, SEQ = seq }, DBWork.Transaction).ToString();
            return rtn;
        }
        public IEnumerable<ME_DOCM> GetDocEMmcode_All(string doc)
        {
            string sql = @"SELECT MMCODE FROM ME_DOCEXP WHERE DOCNO=:DOCNO ";
            return DBWork.Connection.Query<ME_DOCM>(sql, new { DOCNO = doc }, DBWork.Transaction);
        }

        public string GetDocEMmcodeApvqty(string doc, string mmcode)
        {
            string sql = @"SELECT SUM(APVQTY) FROM ME_DOCEXP WHERE DOCNO=:DOCNO AND MMCODE = :MMCODE";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { DOCNO = doc, MMCODE = mmcode }, DBWork.Transaction).ToString();
            return rtn;
        }
        public string GetDocEMmcodeNote(string doc, string mmcode)
        {
            string sql = @" SELECT LISTAGG(NVL(ITEM_NOTE,' '), ' ') WITHIN GROUP (ORDER BY mmcode) AS note FROM ME_DOCEXP WHERE DOCNO=:DOCNO AND MMCODE = :MMCODE GROUP BY mmcode";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { DOCNO = doc, MMCODE = mmcode }, DBWork.Transaction).ToString();
            return rtn;
        }
        public bool CheckExistsDMmcode(string doc, string mmcode)
        {
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO AND MMCODE=:MMCODE ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = doc, MMCODE = mmcode }, DBWork.Transaction) == null);
        }
        public string CallProc(string id, string upuser, string upip)
        {
            var p = new OracleDynamicParameters();
            p.Add("I_DOCNO", value: id, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 21);
            p.Add("I_UPDUSR", value: upuser, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 8);
            p.Add("I_UPDIP", value: upip, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 20);

            p.Add("O_RETID", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 1);
            p.Add("O_ERRMSG", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 255);

            DBWork.Connection.Query("POST_DOC", p, commandType: CommandType.StoredProcedure);
            string retid = p.Get<OracleString>("O_RETID").Value;
            string errmsg = p.Get<OracleString>("O_ERRMSG").Value;
            if (retid == "N")
            {
                retid = errmsg;
            }
            return retid;
        }

        public string GetTaskid(string id)
        {
            string sql = @"SELECT TASK_ID FROM MI_WHID WHERE WH_USERID=:WH_USERID 
                            AND WH_NO IN (SELECT WH_NO FROM MI_WHMAST WHERE WH_KIND='1' AND WH_GRADE='1')";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { WH_USERID = id }, DBWork.Transaction).ToString();
            return rtn;
        }
        public IEnumerable<COMBO_MODEL> GetAgenCombo()
        {
            string sql = @"SELECT DISTINCT AGEN_NO as VALUE, AGEN_NAMEC as TEXT ,
                        AGEN_NO || ' ' || AGEN_NAMEC as COMBITEM 
                        FROM PH_VENDER
                        WHERE 1=1
                        ORDER BY AGEN_NO";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetSTKTRANSKINDCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='STKTRANSKIND' 
                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<COMBO_MODEL> GetLotNoCombo(string id)
        {
            string sql = @"SELECT DISTINCT A.LOT_NO as VALUE, A.LOT_NO as TEXT ,
                        '批號:'||A.LOT_NO||' 效期:'||TWN_DATE(A.EXP_DATE)||' 效期數量:'||A.INV_QTY as COMBITEM,
                        TWN_DATE(A.EXP_DATE) EXTRA1, A.INV_QTY EXTRA2 
                        FROM MI_WEXPINV  A 
                        WHERE A.WH_NO='PH1S'
                        AND A.MMCODE=:MMCODE
                        ORDER BY A.LOT_NO";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { MMCODE = id });
        }
        public IEnumerable<MI_MAST> GetMmCodeCombo(string p0, string p1, int page_index, int page_size, string sorters)
        {

            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT, A.M_CONTPRICE, 
                        ( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO ='PH1S' AND ROWNUM=1) AS INV_QTY,
                        B.AGEN_NO || ' ' || B.AGEN_NAMEC AGEN_NAMEC,
                        A.M_CONTPRICE * A.M_DISCPERC M_DISCPERC 
                        FROM MI_MAST A, PH_VENDER B WHERE 1=1 AND A.M_AGENNO = B.AGEN_NO AND A.MAT_CLASS = :MAT_CLASS 
                        AND EXISTS ( SELECT 1 FROM MI_WHINV WHERE MMCODE = A.MMCODE AND INV_QTY > 0  AND WH_NO ='PH1S' ) 
                        AND EXISTS ( SELECT 1 FROM MI_WEXPINV WHERE MMCODE = A.MMCODE AND INV_QTY > 0  AND WH_NO ='PH1S' ) 
                            ";

            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}%", p0));

                sql += " OR MMNAME_E LIKE UPPER(:MMNAME_E) ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR MMNAME_C LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY MMCODE ";
            }
            p.Add(":MAT_CLASS", p1);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMmcode(MI_MAST_QUERY_PARAMS query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT,A.M_CONTPRICE,
                        ( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO ='PH1S' AND ROWNUM=1) AS INV_QTY 
                        FROM MI_MAST A WHERE 1=1 AND A.MAT_CLASS = :MAT_CLASS 
                        AND EXISTS ( SELECT 1 FROM MI_WHINV WHERE MMCODE = A.MMCODE AND INV_QTY > 0  AND WH_NO ='PH1S' )
                        AND EXISTS ( SELECT 1 FROM MI_WEXPINV WHERE MMCODE = A.MMCODE AND INV_QTY > 0  AND WH_NO ='PH1S' )
                            ";

            if (query.MMCODE != "")
            {
                sql += " AND A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("%{0}%", query.MMCODE));
            }

            if (query.MMNAME_C != "")
            {
                sql += " AND A.MMNAME_C LIKE :MMNAME_C ";
                p.Add(":MMNAME_C", string.Format("%{0}%", query.MMNAME_C));
            }

            if (query.MMNAME_E != "")
            {
                sql += " AND A.MMNAME_E LIKE UPPER(:MMNAME_E) ";
                p.Add(":MMNAME_E", string.Format("%{0}%", query.MMNAME_E));
            }

            p.Add(":MAT_CLASS", query.MAT_CLASS);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<MI_MAST> GetMMCodeDocd(string p0, string p1, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E  
                        FROM MI_MAST A, ME_DOCD B WHERE A.MMCODE=B.MMCODE AND B.DOCNO = :DOCNO  ";
            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}%", p0));

                sql += " OR MMNAME_E LIKE UPPER(:MMNAME_E) ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR MMNAME_C LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY MMCODE ";
            }
            p.Add(":DOCNO", p1);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<COMBO_MODEL> GetFrwhCombo()
        {
            string sql = @"SELECT DISTINCT WH_NO as VALUE, WH_NAME as TEXT,
                        WH_NO || ' ' || WH_NAME as COMBITEM 
                        FROM MI_WHMAST 
                        WHERE 1=1 AND WH_KIND = '1' AND WH_GRADE = '1'  
                        ORDER BY WH_NO";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetMatclassCombo(string id)
        {
            string sql = @"SELECT MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, 
                        MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM 
                        FROM MI_MATCLASS WHERE MAT_CLSID=:MAT_CLSID    
                        ORDER BY MAT_CLASS";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { MAT_CLSID = id });
        }

        public IEnumerable<COMBO_MODEL> GetMatclass23Combo()
        {
            string sql = @"SELECT MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, 
                        MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM 
                        FROM MI_MATCLASS WHERE MAT_CLSID IN ('2','3')     
                        ORDER BY MAT_CLASS";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetStatCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='ME_DOCD' AND DATA_NAME='STAT' 
                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetWhGrade()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='WH_GRADE' 
                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetWhKind()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='WH_KIND' 
                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<AB0003Model> GetLoginInfo(string id, string ip)
        {
            string sql = @"SELECT TUSER AS USERID, UNA AS USERNAME, INID, INID_NAME(INID) AS INIDNAME,
                        WHNO_MM1 CENTER_WHNO,INID_NAME(WHNO_MM1) AS CENTER_WHNAME, TO_CHAR(SYSDATE,'YYYYMMDD') AS TODAY,
                        TWN_SYSDATE,
                        :UPDATE_IP,
                        (SELECT COUNT(*) AS CNT FROM ME_DOCM 
                            WHERE DOCTYPE='MR2' AND APPLY_KIND='1' 
                            AND APPTIME BETWEEN NEXT_DAY(SYSDATE-7,1) AND NEXT_DAY(SYSDATE-7,1)+6 ) MR2,
                        NVL((SELECT 'Y' FROM DUAL WHERE (SELECT EXTRACT(DAY FROM SYSDATE) FROM DUAL) 
                            BETWEEN (SELECT DATA_VALUE FROM PARAM_D WHERE DATA_NAME = 'MR3_BDAY')
                            AND (SELECT DATA_VALUE FROM PARAM_D WHERE DATA_NAME = 'MR3_EDAY')),'N') MR3,
                        NVL((SELECT 'Y' FROM DUAL WHERE (SELECT EXTRACT(DAY FROM SYSDATE) FROM DUAL) 
                            BETWEEN (SELECT DATA_VALUE FROM PARAM_D WHERE DATA_NAME = 'MR4_BDAY')
                            AND (SELECT DATA_VALUE FROM PARAM_D WHERE DATA_NAME = 'MR4_EDAY')),'N') MR4 
                        FROM UR_ID
                        WHERE UR_ID.TUSER=:TUSER";

            return DBWork.Connection.Query<AB0003Model>(sql, new { TUSER = id, UPDATE_IP = ip });
        }

        public class MI_MAST_QUERY_PARAMS
        {
            public string MMCODE;
            public string MMNAME_C;
            public string MMNAME_E;
            public string MAT_CLASS;

            public string WH_NO;
            public string IS_INV;  // 需判斷庫存量>0
            public string E_IFPUBLIC;  // 是否公藥
        }

        #region 20201-05-06新增: 修改、刪除時檢查flowId
        public bool ChceckFlowId01(string docno)
        {
            string sql = @"
                select 1 from ME_DOCM
                 where docno = :docno
                   and (substr(flowId, length(flowId)-1 , 2) = '01'
                       or substr(flowId, length(flowId)-1 , 2) = '00' 
                       or substr(flowId, length(flowId)-1 , 2) = '1')
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { docno }, DBWork.Transaction) != null;
        }
        #endregion
    }
}
