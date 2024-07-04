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
    public class AA0022Repository : JCLib.Mvc.BaseRepository
    {
        public AA0022Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ME_DOCM> GetAllM(string apptime1, string apptime2, string matclass, string frwh,string docno, string tuser, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.* ,
                        (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='FLOWID_RJ1' AND DATA_VALUE=A.FLOWID) FLOWID_N,
                        (SELECT MAT_CLSNAME FROM MI_MATCLASS WHERE MAT_CLASS=A.MAT_CLASS) MAT_CLASS_N,
                        (SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.FRWH) FRWH_N,
                        (SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.TOWH) TOWH_N,
                        (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='APPLY_KIND' AND DATA_VALUE=A.APPLY_KIND) APPLY_KIND_N ,
                        (SELECT UNA FROM UR_ID WHERE TUSER=A.APPID) APP_NAME,
                        (SELECT INID_NAME FROM UR_INID WHERE INID=A.APPDEPT) APPDEPT_NAME,
                        TWN_DATE(A.APPTIME) APPTIME_T ,
                        (SELECT SUM(X.C_AMT * -1) FROM ME_DOCEXP X, MI_MAST Y WHERE X.DOCNO=A.DOCNO AND X.MMCODE=Y.MMCODE AND X.APVQTY < 0) SUM_EX,
                        (SELECT SUM(X.C_AMT) FROM ME_DOCEXP X, MI_MAST Y WHERE X.DOCNO=A.DOCNO AND X.MMCODE=Y.MMCODE AND X.APVQTY > 0) SUM_IN  
                        FROM ME_DOCM A ,MI_MATCLASS B WHERE A.MAT_CLASS=B.MAT_CLASS AND A.DOCTYPE ='RJ1' ";
            //if (task != "")
            //{
            //    sql += " AND B.MAT_CLSID = :task ";
            //    p.Add(":task", string.Format("{0}", task));
            //}
            if (apptime1 != "" & apptime2 != "")
            {
                sql += " AND TWN_DATE(A.APPTIME) BETWEEN :p0 AND :p1 ";
                p.Add(":p0", string.Format("{0}", apptime1));
                p.Add(":p1", string.Format("{0}", apptime2));
            }
            if (apptime1 != "" & apptime2 == "")
            {
                sql += " AND TWN_DATE(A.APPTIME) >= :p0 ";
                p.Add(":p0", string.Format("{0}", apptime1));
            }
            if (apptime1 == "" & apptime2 != "")
            {
                sql += " AND TWN_DATE(A.APPTIME) <= :p1 ";
                p.Add(":p1", string.Format("{0}", apptime2));
            }
            if (matclass != "")
            {
                sql += " AND A.MAT_CLASS = :p2 ";
                p.Add(":p2", string.Format("{0}", matclass));
            }
            if (frwh != "")
            {
                sql += " AND A.FRWH = :p3 ";
                p.Add(":p3", string.Format("{0}", frwh));
            }
            if (docno != "")
            {
                sql += " AND A.DOCNO = :pp ";
                p.Add(":pp", string.Format("{0}", docno));
            }
            if (tuser != "")
            {
                sql += " AND A.APPID = :tuser ";
                p.Add(":tuser", string.Format("{0}", tuser));
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCM>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
            //return DBWork.Connection.Query<ME_DOCM>(sql, p, DBWork.Transaction);
        }
        public IEnumerable<ME_DOCEXP> GetAllD(string DOCNO, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.DOCNO,A.SEQ,A.EXP_DATE,A.LOT_NO,A.LOT_NO LOT_NO_N,TWN_DATE(A.EXP_DATE) EXP_DATE_T,
                        A.MMCODE,A.C_TYPE,A.C_STATUS,A.C_RNO,A.C_UP,
                        (CASE WHEN A.APVQTY < 0 THEN -1 ELSE 1 END) * A.APVQTY APVQTY, 
                        (CASE WHEN A.APVQTY < 0 THEN -1 ELSE 1 END) * A.C_AMT C_AMT,
                        NVL(CASE WHEN B.WEXP_ID='Y' THEN 
                        ( SELECT INV_QTY FROM MI_WEXPINV WHERE MMCODE = A.MMCODE AND WH_NO ='560000' AND LOT_NO=A.LOT_NO AND ROWNUM=1 )
                        ELSE  
                        ( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO ='560000' AND ROWNUM=1 ) 
                        END,0)  AS INV_QTY,
                        B.MMNAME_C, B.MMNAME_E, B.BASE_UNIT,B.M_CONTPRICE,
                        (SELECT DATA_DESC FROM PARAM_D 
                        WHERE GRP_CODE='ME_DOCD' AND DATA_NAME='STAT' 
                        AND DATA_VALUE=(CASE WHEN A.APVQTY < 0 THEN '2' ELSE '1' END)) INOUT_N,
                        (CASE WHEN A.APVQTY < 0 THEN '2' ELSE '1' END) INOUT,
                        ( SELECT SUM( X.APVQTY * Y.M_CONTPRICE ) FROM ME_DOCEXP X,MI_MAST Y WHERE X.MMCODE = Y.MMCODE AND X.DOCNO = A.DOCNO ) BALANCE,
                        ITEM_NOTE,
                        B.M_AGENNO || '_' || D.AGEN_NAMEC AGEN_NAMEC
                        FROM ME_DOCEXP A
                        INNER JOIN MI_MAST B ON B.MMCODE=A.MMCODE
                        INNER JOIN ME_DOCM C ON C.DOCNO=A.DOCNO 
                        INNER JOIN PH_VENDER D ON D.AGEN_NO=B.M_AGENNO 
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
                        (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='FLOWID_RJ1' AND DATA_VALUE=A.FLOWID) FLOWID_N,
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
                        FROM ME_DOCM A WHERE 1=1 AND A.DOCTYPE = 'RJ1'  
                        AND A.DOCNO = :DOCNO";
            return DBWork.Connection.Query<ME_DOCM>(sql, new { DOCNO = id }, DBWork.Transaction);
        }
        public IEnumerable<ME_DOCEXP> GetD(string id)
        {
            var sql = @"SELECT A.DOCNO,A.SEQ,A.EXP_DATE,A.LOT_NO,A.LOT_NO LOT_NO_N,TWN_DATE(A.EXP_DATE) EXP_DATE_T,
                        A.MMCODE,A.C_TYPE,A.C_STATUS,A.C_RNO,A.C_UP,
                        (CASE WHEN A.APVQTY < 0 THEN -1 ELSE 1 END) * A.APVQTY APVQTY, 
                        (CASE WHEN A.APVQTY < 0 THEN -1 ELSE 1 END) * A.C_AMT C_AMT,
                        NVL(CASE WHEN B.WEXP_ID='Y' THEN 
                        ( SELECT INV_QTY FROM MI_WEXPINV WHERE MMCODE = A.MMCODE AND WH_NO ='560000' AND LOT_NO=A.LOT_NO AND ROWNUM=1 )
                        ELSE  
                        ( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO ='560000' AND ROWNUM=1 ) 
                        END,0)  AS INV_QTY,
                        B.MMNAME_C, B.MMNAME_E, B.BASE_UNIT,B.M_CONTPRICE,
                        (SELECT DATA_DESC FROM PARAM_D 
                        WHERE GRP_CODE='ME_DOCD' AND DATA_NAME='STAT' 
                        AND DATA_VALUE=(CASE WHEN A.APVQTY < 0 THEN '2' ELSE '1' END)) INOUT_N,
                        (CASE WHEN A.APVQTY < 0 THEN '2' ELSE '1' END) INOUT,
                        ( SELECT SUM( X.APVQTY * Y.M_CONTPRICE ) FROM ME_DOCEXP X,MI_MAST Y WHERE X.MMCODE = Y.MMCODE AND X.DOCNO = A.DOCNO ) BALANCE,
                        A.ITEM_NOTE,
                        B.M_AGENNO || '_' || D.AGEN_NAMEC AGEN_NAMEC 
                        FROM ME_DOCEXP A
                        INNER JOIN MI_MAST B ON B.MMCODE=A.MMCODE
                        INNER JOIN ME_DOCM C ON C.DOCNO=A.DOCNO 
                        INNER JOIN PH_VENDER D ON D.AGEN_NO=B.M_AGENNO 
                        WHERE 1=1 
                        AND A.DOCNO = :DOCNO";
            return DBWork.Connection.Query<ME_DOCEXP>(sql, new { DOCNO = id }, DBWork.Transaction);
        }
        public int CreateM(ME_DOCM ME_DOCM)
        {
            var sql = @"INSERT INTO ME_DOCM (
                        DOCNO, DOCTYPE, FLOWID , APPID , APPDEPT , 
                        APPTIME , USEID , USEDEPT , FRWH , TOWH , 
                        APPLY_KIND ,APPLY_NOTE ,MAT_CLASS,
                        CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                      VALUES (
                        :DOCNO, :DOCTYPE, :FLOWID , :APPID , :APPDEPT , 
                        TO_DATE(:APPTIME,'YYYY/MM/DD') , :USEID , :USEDEPT , :FRWH , :TOWH , 
                        :APPLY_KIND , :APPLY_NOTE ,:MAT_CLASS,
                        SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);
        }

        public int CreateD(ME_DOCD ME_DOCD)
        {
            var sql = @"INSERT INTO ME_DOCD (
                        DOCNO, SEQ, MMCODE , APPQTY , APLYITEM_NOTE, STAT, 
                        CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                      VALUES (
                        :DOCNO, :SEQ, :MMCODE , :APPQTY , :APLYITEM_NOTE, '2',
                        :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, ME_DOCD, DBWork.Transaction);
        }
        public int CreateExp(ME_DOCEXP me_docexp)
        {
            var sql = @"INSERT INTO ME_DOCEXP (
                        DOCNO, SEQ, MMCODE , APVQTY , ITEM_NOTE, 
                        C_UP, C_AMT, EXP_DATE, LOT_NO,
                        UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                      VALUES (
                        :DOCNO, :SEQ, :MMCODE , :APVQTY , :ITEM_NOTE, 
                        :C_UP, :C_AMT, TO_DATE(:EXP_DATE,'YYYY/MM/DD'), :LOT_NO,
                        SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, me_docexp, DBWork.Transaction);
        }
        public int UpdateM(ME_DOCM ME_DOCM)
        {
            var sql = @"UPDATE ME_DOCM SET 
                        MAT_CLASS = :MAT_CLASS ,APPLY_NOTE = :APPLY_NOTE, APPTIME = TO_DATE(:APPTIME,'YYYY/MM/DD') ,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);
        }
        public int UpdateD(ME_DOCD ME_DOCD)
        {
            var sql = @"UPDATE ME_DOCD SET 
                        EXPT_DISTQTY = :EXPT_DISTQTY, BW_MQTY = :BW_MQTY, APLYITEM_NOTE =:APLYITEM_NOTE,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO AND SEQ = :SEQ ";
            return DBWork.Connection.Execute(sql, ME_DOCD, DBWork.Transaction);
        }

        public int UpdateMeDocd(ME_DOCD ME_DOCD)
        {
            var sql = @"UPDATE ME_DOCD SET EXPT_DISTQTY = :EXPT_DISTQTY, BW_MQTY = :BW_MQTY, 
                                UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE DOCNO = :DOCNO AND SEQ = :SEQ";
            return DBWork.Connection.Execute(sql, ME_DOCD, DBWork.Transaction);
        }
        public int ApplyM(ME_DOCM ME_DOCM)
        {
            var sql = @"UPDATE ME_DOCM SET FLOWID = :FLOWID,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);
        }
        public int ApplyD(ME_DOCM ME_DOCM)
        {
            var sql = @"UPDATE ME_DOCD SET APL_CONTIME = SYSDATE,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);
        }
        public int DeleteM(string docno)
        {
            var sql = @" DELETE from ME_DOCM WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno }, DBWork.Transaction);
        }
        public int DeleteAllD(string docno)
        {
            var sql = @" DELETE from ME_DOCD WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public int DeleteD(string docno, string seq)
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
                    WHERE A.MMCODE = C.MMCODE AND A.DOCNO=B.DOCNO AND B.DOCTYPE='RJ1'
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
        public bool CheckExistsD(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        public bool CheckExistsDN(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO  AND APPQTY = 0";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        public bool CheckExistsExp(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCEXP WHERE DOCNO=:DOCNO ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        public bool CheckExistsExpN(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCEXP WHERE DOCNO=:DOCNO AND APVQTY = 0  ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        public bool CheckExistsMM(string id, string mm)
        {
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO AND MMCODE=:MMCODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id, MMCODE = mm }, DBWork.Transaction) == null);
        }
        public bool CheckExistsE(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCEXP WHERE DOCNO=:DOCNO ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        public string GetDocESeq(string id)
        {
            string sql = @"SELECT MAX(SEQ)+1 as SEQ FROM ME_DOCEXP WHERE DOCNO=:DOCNO ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction).ToString();
            return rtn;
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
        public string GetFrwh()
        {
            string sql = @"SELECT WH_NO 
                        FROM MI_WHMAST 
                        WHERE 1=1 AND WH_KIND = '1' AND WH_GRADE = '1' AND ROWNUM=1";
            string rtn = DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction).ToString();
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
        public string GetMmcodeWexp(string mmcode)
        {
            string sql = @"SELECT NVL(WEXP_ID,'N') WEXP_ID FROM MI_MAST WHERE MMCODE= :MMCODE";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { MMCODE = mmcode }, DBWork.Transaction).ToString();
            return rtn;
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
        public IEnumerable<COMBO_MODEL> GetDocnoCombo()
        {
            string sql = @"SELECT DISTINCT A.DOCNO as VALUE, A.DOCNO as TEXT,
                        A.DOCNO as COMBITEM
                        FROM ME_DOCM A ,MI_MATCLASS B 
                        WHERE A.MAT_CLASS=B.MAT_CLASS AND DOCTYPE = 'RJ1'
                        ORDER BY A.DOCNO";
            
            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetFlowidCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='FLOWID_RJ1' 
                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<COMBO_MODEL> GetAppDeptCombo()
        {
            string sql = @"SELECT DISTINCT A.APPDEPT as VALUE, B.INID_NAME as TEXT ,
                        A.APPDEPT || ' ' || B.INID_NAME as COMBITEM 
                        FROM ME_DOCM A, UR_INID B 
                        WHERE A.APPDEPT=B.INID 
                        ORDER BY A.APPDEPT";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<MI_MAST> GetMmCodeCombo(string p0, string p1, int page_index, int page_size, string sorters)
        {

            var p = new DynamicParameters();

            var sql = @"
                        SELECT {0} P.* FROM (
                        (SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT,
                        A.WEXP_ID,A.M_CONTPRICE,
                        ( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO ='560000' AND ROWNUM=1 ) AS INV_QTY,
                        A.M_AGENNO || '_' || B.AGEN_NAMEC AGEN_NAMEC
                        FROM MI_MAST A 
                        INNER JOIN PH_VENDER B ON B.AGEN_NO=A.M_AGENNO 
                        WHERE 1=1 AND A.MAT_CLASS = :MAT_CLASS 
                          AND A.M_CONTID <> '3'
                          AND A.M_APPLYID <> 'E' 
                          AND ( A.WEXP_ID='N' OR A.WEXP_ID IS NULL )
                          AND EXISTS (SELECT 1 FROM MI_WHINV WHERE MMCODE = A.MMCODE AND INV_QTY > 0 AND WH_NO ='560000' )
                          )
                          UNION ALL 
                          (SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT,
                        A.WEXP_ID,A.M_CONTPRICE,
                        ( SELECT INV_QTY FROM MI_WEXPINV WHERE MMCODE = A.MMCODE AND WH_NO ='560000' AND ROWNUM=1 ) AS INV_QTY,
                        A.M_AGENNO || '_' || B.AGEN_NAMEC AGEN_NAMEC
                        FROM MI_MAST A 
                        INNER JOIN PH_VENDER B ON B.AGEN_NO=A.M_AGENNO 
                        WHERE 1=1 AND A.MAT_CLASS = :MAT_CLASS 
                          AND A.M_CONTID <> '3'
                          AND A.M_APPLYID <> 'E' 
                          AND  A.WEXP_ID='Y' 
                          AND EXISTS (SELECT 1 FROM MI_WEXPINV WHERE MMCODE = A.MMCODE AND INV_QTY > 0 AND WH_NO ='560000' )
                          )
                          )P WHERE 1=1
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
            var sql = @"SELECT P.* FROM (
                        (SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT,
                        A.WEXP_ID,A.M_CONTPRICE,
                        ( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO ='560000' AND ROWNUM=1 ) AS INV_QTY,
                        A.M_AGENNO || '_' || B.AGEN_NAMEC AGEN_NAMEC
                        FROM MI_MAST A 
                        INNER JOIN PH_VENDER B ON B.AGEN_NO=A.M_AGENNO 
                        WHERE 1=1 AND A.MAT_CLASS = :MAT_CLASS 
                          AND A.M_CONTID <> '3'
                          AND A.M_APPLYID <> 'E' 
                          AND ( A.WEXP_ID='N' OR A.WEXP_ID IS NULL )
                          AND EXISTS (SELECT 1 FROM MI_WHINV WHERE MMCODE = A.MMCODE AND INV_QTY > 0 AND WH_NO ='560000' )
                          )
                          UNION ALL 
                          (SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT,
                        A.WEXP_ID,A.M_CONTPRICE,
                        ( SELECT INV_QTY FROM MI_WEXPINV WHERE MMCODE = A.MMCODE AND WH_NO ='560000' AND ROWNUM=1 ) AS INV_QTY,
                        A.M_AGENNO || '_' || B.AGEN_NAMEC AGEN_NAMEC
                        FROM MI_MAST A 
                        INNER JOIN PH_VENDER B ON B.AGEN_NO=A.M_AGENNO 
                        WHERE 1=1 AND A.MAT_CLASS = :MAT_CLASS 
                          AND A.M_CONTID <> '3'
                          AND A.M_APPLYID <> 'E' 
                          AND A.WEXP_ID='Y'
                          AND EXISTS (SELECT 1 FROM MI_WEXPINV WHERE MMCODE = A.MMCODE AND INV_QTY > 0 AND WH_NO ='560000' )
                          )
                          )P WHERE 1=1
                             ";

            if (query.MMCODE != "")
            {
                sql += " AND P.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("%{0}%", query.MMCODE));
            }

            if (query.MMNAME_C != "")
            {
                sql += " AND P.MMNAME_C LIKE :MMNAME_C ";
                p.Add(":MMNAME_C", string.Format("%{0}%", query.MMNAME_C));
            }

            if (query.MMNAME_E != "")
            {
                sql += " AND P.MMNAME_E LIKE UPPER(:MMNAME_E) ";
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

        public IEnumerable<COMBO_MODEL> GetLotNoCombo(string id)
        {
            string sql = @"SELECT DISTINCT A.LOT_NO as VALUE, A.LOT_NO as TEXT ,
                        '批號:'||A.LOT_NO||' 效期:'||TWN_DATE(A.EXP_DATE)||' 效期數量:'||A.INV_QTY as COMBITEM,
                        TWN_DATE(A.EXP_DATE) EXTRA1, A.INV_QTY EXTRA2 
                        FROM MI_WEXPINV  A 
                        WHERE A.WH_NO='560000'
                        AND A.MMCODE=:MMCODE
                        ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { MMCODE = id });
        }
        public IEnumerable<COMBO_MODEL> GetLotNo2Combo(string id)
        {
            string sql = @"
                        SELECT '--' VALUE,'--' TEXT,'--'COMBITEM,'1880101'EXTRA1,
                        (SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = :MMCODE AND WH_NO ='560000' AND ROWNUM=1) EXTRA2 FROM DUAL";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { MMCODE = id });
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
        public IEnumerable<COMBO_MODEL> GetMatclassCombo()
        {
            string sql = @"SELECT MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, 
                        MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM 
                        FROM MI_MATCLASS WHERE MAT_CLSID IN ('2','3') 
                        ORDER BY MAT_CLASS";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<COMBO_MODEL> GetYN()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='Y_OR_N' AND DATA_NAME='YN' 
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
