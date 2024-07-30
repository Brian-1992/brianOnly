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
    public class AA0124Repository : JCLib.Mvc.BaseRepository
    {
        public AA0124Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ME_DOCM> GetAllM(string apptime1, string apptime2, string appdept, string applykind, string doctype, string mmcode)
        {
            var p = new DynamicParameters();

            var sql = string.Format(@"SELECT A.* ,
                                            (CASE WHEN A.DOCTYPE IN ('MR1', 'MR2') THEN '庫備' WHEN A.DOCTYPE IN ('MR3', 'MR4') THEN '非庫備' ELSE '' END) DOCTYPE_N,
                                            (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='FLOWID_MR1' AND DATA_VALUE=A.FLOWID) FLOWID_N,
                                            (SELECT MAT_CLSNAME FROM MI_MATCLASS WHERE MAT_CLASS=A.MAT_CLASS) MAT_CLASS_N,
                                            (SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.FRWH) FRWH_N,
                                            (SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.TOWH) TOWH_N,
                                            (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='APPLY_KIND' AND DATA_VALUE=A.APPLY_KIND) APPLY_KIND_N ,
                                            (SELECT UNA FROM UR_ID WHERE TUSER=A.APPID) APP_NAME,
                                            (SELECT INID_NAME FROM UR_INID WHERE INID=A.APPDEPT) APPDEPT_NAME,
                                            TWN_DATE(A.APPTIME) APPTIME_T 
                                            FROM ME_DOCM A WHERE 1=1  AND A.FLOWID in ('6', '51')
                                            AND A.MAT_CLASS = '02'
                                            AND EXISTS (SELECT 1 FROM ME_DOCD WHERE DOCNO=A.DOCNO AND BW_MQTY - RV_MQTY <> 0 {0} )",
                                            mmcode == string.Empty ? string.Empty : "and mmcode = :mmcode ");
            p.Add(":mmcode", string.Format("{0}", mmcode));
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
            if (appdept != "")
            {
                sql += " AND A.APPDEPT = :p2 ";
                p.Add(":p2", string.Format("{0}", appdept));
            }
            if (applykind != "")
            {
                sql += " AND A.APPLY_KIND = :p4 ";
                p.Add(":p4", string.Format("{0}", applykind));
            }
            if (doctype != "")
            {
                switch (doctype)
                {
                    case "0":
                        sql += " AND A.DOCTYPE IN ('MR3', 'MR4') ";
                        break;
                    case "1":
                        sql += " AND A.DOCTYPE IN ('MR1', 'MR2') ";
                        break;
                    default:
                        break;
                }
            }

            return DBWork.PagingQuery<ME_DOCM>(sql, p, DBWork.Transaction);
        }
        public IEnumerable<ME_DOCD> GetAllD(string DOCNO, string MMCODE, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.DOCNO, A.SEQ, A.MMCODE , A.APPQTY , A.APLYITEM_NOTE, 
                        A.GTAPL_RESON,A.STAT,A.EXPT_DISTQTY,A.APVQTY,A.ACKQTY,
                        A.BW_MQTY,A.BW_SQTY,A.APVTIME,A.ONWAY_QTY,A.RV_MQTY,
                        B.MMNAME_C, B.MMNAME_E, B.BASE_UNIT,B.M_CONTPRICE,
                        ( SELECT AVG_PRICE FROM MI_WHCOST WHERE MMCODE = A.MMCODE AND ROWNUM=1 ) AS AVG_PRICE,
                        ( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO=C.FRWH ) AS INV_QTY,
                        ( SELECT AVG_APLQTY FROM V_MM_AVGAPL WHERE MMCODE = A.MMCODE AND WH_NO=C.FRWH ) AS AVG_APLQTY,
                        ( SELECT NVL(TOT_APVQTY,0) FROM V_MM_TOTAPL WHERE DATE_YM=SUBSTR(TWN_DATE(C.APPTIME),0,5) and MMCODE=A.MMCODE ) AS TOT_APVQTY,
                        ( SELECT SUM(TOT_BWQTY) FROM V_MM_TOTAPL WHERE DATE_YM=SUBSTR(TWN_DATE(C.APPTIME),0,5) and MMCODE=A.MMCODE ) AS TOT_BWQTY,
                        ( SELECT MIN_ORDQTY FROM MI_WINVCTL WHERE MMCODE = A.MMCODE AND WH_NO ='560000' AND ROWNUM =1) AS TOT_DISTUN,
                        C.FLOWID,
                        ( SELECT SAFE_QTY FROM V_MM_WHINVCTL WHERE MMCODE = A.MMCODE AND WH_NO=C.TOWH ) AS SAFE_QTY 
                        FROM ME_DOCD A,MI_MAST B, ME_DOCM C WHERE C.DOCNO=A.DOCNO AND A.MMCODE = B.MMCODE(+) 
                        AND A.BW_MQTY - A.RV_MQTY <> 0";

            if (DOCNO != "")
            {
                sql += " AND A.DOCNO LIKE :p0 ";
                p.Add(":p0", string.Format("%{0}%", DOCNO));
            }
            else
            {
                sql += " AND 1=2 ";
            }
            if (MMCODE != "")
            {
                sql += " AND A.MMCODE LIKE :p1 ";
                p.Add(":p1", string.Format("%{0}%", MMCODE));
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCD>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<ME_DOCM> GetM(string id)
        {
            var sql = @"SELECT A.*,
                        (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='FLOWID' AND DATA_VALUE=A.FLOWID) FLOWID_N,
                        (SELECT MAT_CLSNAME FROM MI_MATCLASS WHERE MAT_CLASS=A.MAT_CLASS) MAT_CLASS_N,
                        (SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.FRWH) FRWH_N,
                        (SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.TOWH) TOWH_N,
                        (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='APPLY_KIND' AND DATA_VALUE=A.APPLY_KIND) APPLY_KIND_N
                        FROM ME_DOCM A 
                        WHERE A.DOCNO = :DOCNO
                        AND EXISTS (SELECT 1 FROM ME_DOCD WHERE DOCNO=A.DOCNO AND BW_MQTY - RV_MQTY <> 0 ) 
                        ";
            return DBWork.Connection.Query<ME_DOCM>(sql, new { DOCNO = id }, DBWork.Transaction);
        }
        public IEnumerable<ME_DOCD> GetD(string id)
        {
            var sql = @"SELECT A.DOCNO, A.SEQ, A.MMCODE , A.APPQTY , A.APLYITEM_NOTE, 
                        A.GTAPL_RESON,A.STAT,A.EXPT_DISTQTY,A.APVQTY,A.ACKQTY,
                        A.BW_MQTY,A.BW_SQTY,A.APVTIME,A.ONWAY_QTY,A.RV_MQTY,
                        B.MMNAME_C, B.MMNAME_E, B.BASE_UNIT,B.M_CONTPRICE,
                        ( SELECT AVG_PRICE FROM MI_WHCOST WHERE MMCODE = A.MMCODE AND ROWNUM=1 ) AS AVG_PRICE,
                        ( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO=C.FRWH ) AS INV_QTY,
                        ( SELECT AVG_APLQTY FROM V_MM_AVGAPL WHERE MMCODE = A.MMCODE AND WH_NO=C.FRWH ) AS AVG_APLQTY,
                        ( SELECT NVL(TOT_APVQTY,0) FROM V_MM_TOTAPL WHERE DATE_YM=SUBSTR(TWN_DATE(C.APPTIME),0,5) and MMCODE=A.MMCODE ) AS TOT_APVQTY,
                        ( SELECT SUM(TOT_BWQTY) FROM V_MM_TOTAPL WHERE DATE_YM=SUBSTR(TWN_DATE(C.APPTIME),0,5) and MMCODE=A.MMCODE ) AS TOT_BWQTY,
                        ( SELECT MIN_ORDQTY FROM MI_WINVCTL WHERE MMCODE = A.MMCODE AND WH_NO ='560000' AND ROWNUM =1) AS TOT_DISTUN,
                        C.FLOWID,
                        ( SELECT SAFE_QTY FROM MI_WINVCTL WHERE MMCODE = A.MMCODE AND WH_NO=C.TOWH ) AS SAFE_QTY 
                        FROM ME_DOCD A,MI_MAST B, ME_DOCM C WHERE C.DOCNO=A.DOCNO AND A.MMCODE = B.MMCODE(+)
                        AND A.BW_MQTY - A.RV_MQTY <> 0 
                        AND A.DOCNO = :DOCNO";
            return DBWork.Connection.Query<ME_DOCD>(sql, new { DOCNO = id }, DBWork.Transaction);
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
                        SYSDATE , :USEID , :USEDEPT , :FRWH , :TOWH , 
                        :APPLY_KIND , :APPLY_NOTE ,:MAT_CLASS,
                        SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);
        }

        public int CreateD(ME_DOCD ME_DOCD)
        {
            var sql = @"INSERT INTO ME_DOCD (
                        DOCNO, SEQ, MMCODE , APPQTY , APLYITEM_NOTE,
                        CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                      VALUES (
                        :DOCNO, :SEQ, :MMCODE , :APPQTY ,  :APLYITEM_NOTE, 
                        :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, ME_DOCD, DBWork.Transaction);
        }
        public int UpdateM(ME_DOCM ME_DOCM)
        {
            var sql = @"UPDATE ME_DOCM SET 
                        DOCTYPE = :DOCTYPE, FLOWID = :FLOWID, APPID = :APPID, APPDEPT = :APPDEPT, 
                        APPTIME = SYSDATE, USEID = :USEID, USEDEPT = :USEDEPT, FRWH = :FRWH, TOWH = :TOWH,
                        APPLY_KIND = :APPLY_KIND ,APPLY_NOTE = :APPLY_NOTE,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);
        }
        public int UpdateD(ME_DOCD ME_DOCD)
        {
            var sql = @"UPDATE ME_DOCD SET 
                        RV_MQTY = :RV_MQTY,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO AND SEQ = :SEQ ";
            return DBWork.Connection.Execute(sql, ME_DOCD, DBWork.Transaction);
        }

        public int UpdateMeDocd(ME_DOCD ME_DOCD)
        {
            var sql = @"UPDATE ME_DOCD SET RV_MQTY = :RV_MQTY, 
                                UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE DOCNO = :DOCNO AND SEQ = :SEQ";
            return DBWork.Connection.Execute(sql, ME_DOCD, DBWork.Transaction);
        }
        public int ApplyM(ME_DOCM ME_DOCM)
        {
            var sql = @"UPDATE ME_DOCM SET FLOWID = '3',
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);

        }
        public int ApplyD(ME_DOCM ME_DOCM)
        {
            var sql = @"UPDATE ME_DOCD SET PICK_QTY = EXPT_DISTQTY, ACKQTY = EXPT_DISTQTY,
                        DIS_USER = :UPDATE_USER,DIS_TIME = SYSDATE,
                        APL_CONTIME = SYSDATE,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);

        }
        public int DeleteM(ME_DOCM ME_DOCM)
        {
            var sql = @"DELETE ME_DOCM WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);
        }
        public int DeleteD(ME_DOCD ME_DOCD)
        {
            var sql = @"DELETE ME_DOCD WHERE DOCNO = :DOCNO AND SEQ = :SEQ";
            return DBWork.Connection.Execute(sql, ME_DOCD, DBWork.Transaction);
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
        public bool CheckExistsDD(string id, string sid)
        {
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO AND SEQ=:SEQ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id, SEQ = sid }, DBWork.Transaction) == null);
        }
        public bool CheckExistsDN(string id)
        {
            //string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO AND ( EXPT_DISTQTY = 0 OR BW_MQTY = 0 ) ";
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO AND EXPT_DISTQTY = 0  ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        public int CheckApplyKind()
        {
            string sql = @"SELECT COUNT(*) AS CNT FROM ME_DOCM 
                            WHERE DOCTYPE='MR2' AND APPLY_KIND='1' 
                            AND SUBSTR(APPTIME,0,7) BETWEEN TWN_DATE(NEXT_DAY(SYSDATE-7,1)) AND TWN_DATE(NEXT_DAY(SYSDATE-7,1)+6)";
            int rtn = Convert.ToInt32(DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction).ToString());
            return rtn;
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
        public string CallProc2(string id, int seq, string upuser, string upip)
        {
            var p = new OracleDynamicParameters();
            p.Add("I_DOCNO", value: id, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 21);
            p.Add("I_SEQ", value: seq, dbType: OracleDbType.Int32, direction: ParameterDirection.Input, size: 13);
            p.Add("I_UPDUSR", value: upuser, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 8);
            p.Add("I_UPDIP", value: upip, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 20);

            p.Add("O_RETID", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 1);
            p.Add("O_ERRMSG", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 255);

            DBWork.Connection.Query("POST_DOC_BWMIL", p, commandType: CommandType.StoredProcedure);
            string retid = p.Get<OracleString>("O_RETID").Value;
            string errmsg = p.Get<OracleString>("O_ERRMSG").Value;
            if (retid == "N")
            {
                retid = errmsg;
            }
            return retid;
        }
        public IEnumerable<string> GetTaskid(string id)
        {
            string sql = @"SELECT TASK_ID FROM MI_WHID WHERE WH_USERID=:WH_USERID 
                            AND WH_NO IN (SELECT WH_NO FROM MI_WHMAST WHERE WH_KIND='1' AND WH_GRADE='1')";
            return DBWork.Connection.Query<string>(sql, new { WH_USERID = id }, DBWork.Transaction);
        }
        public IEnumerable<COMBO_MODEL> GetDocnoCombo()
        {
            string sql = @"SELECT DISTINCT DOCNO as VALUE, DOCNO as TEXT,
                        DOCNO as COMBITEM
                        FROM ME_DOCM 
                        WHERE 1=1 AND DOCTYPE = 'MR2' 
                        ORDER BY DOCNO";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetFlowidCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='FLOWID' 
                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetApplyKindCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='APPLY_KIND' 
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
        public IEnumerable<MI_MAST> GetMmCodeCombo(string p0, string taskid, int page_index, int page_size, string sorters)
        {

            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT,A.M_CONTPRICE
                        FROM MI_MAST A WHERE 1=1 
                         AND A.MAT_CLASS in (select mat_class from MI_MATCLASS where mat_clsid = : taskid) 
                         AND A.M_CONTID <> '3'
                         AND A.M_APPLYID <> 'E' ";
            p.Add(":taskid", taskid);

            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (A.MMCODE LIKE :MMCODE ";
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
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (A.MMCODE LIKE :MMCODE ";
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
        public IEnumerable<COMBO_MODEL> GetTowhCombo(string id)
        {
            string sql = @"SELECT DISTINCT A.WH_NO as VALUE, A.WH_NAME as TEXT,
                        A.WH_NO || ' ' || A.WH_NAME as COMBITEM 
                        FROM MI_WHMAST A,UR_ID B
                        WHERE A.INID=B.INID 
                        AND A.WH_KIND = '1' 
                        AND TUSER=:TUSER 
                        ORDER BY WH_NO";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { TUSER = id });
        }
        public IEnumerable<COMBO_MODEL> GetMatclassCombo()
        {
            string sql = @"select MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, 
                        MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM 
                        FROM MI_MATCLASS WHERE MAT_CLSID IN( '2' , '3' )   
                        ORDER BY MAT_CLASS";

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

        #region
        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, string p1, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E  
                        FROM MI_MAST A, ME_DOCD B WHERE A.MMCODE=B.MMCODE AND B.DOCNO = :DOCNO  ";
            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (A.MMCODE LIKE :MMCODE ";
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
        #endregion

        public IEnumerable<ME_DOCD> GetCurrentDocds(string docno, string mmcodes)
        {
            string sql = string.Format(@"select * from ME_DOCD 
                                          where docno = :docno
                                            and mmcode in ( {0} )", mmcodes);
            return DBWork.Connection.Query<ME_DOCD>(sql, new { docno = docno }, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetStoreidCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='MI_MAST' AND DATA_NAME='M_STOREID'
                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
    }
}
