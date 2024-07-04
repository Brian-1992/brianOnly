using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace WebApp.Repository.AB
{
    public class AB0007ReportMODEL : JCLib.Mvc.BaseModel
    {
        public string F1 { get; set; }
        public string F2 { get; set; }
        public string F3 { get; set; }
        public string F4 { get; set; }
        public string F5 { get; set; }
        public string F6 { get; set; }
        public string F7 { get; set; }
        public string F8 { get; set; }
        public float F9 { get; set; }
        public string F10 { get; set; }
        public float F11 { get; set; }
        public string F12 { get; set; }
        public float F13 { get; set; }
    }
    public class AB0007Repository : JCLib.Mvc.BaseRepository
    {
        public AB0007Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ME_DOCM> GetAllM(string DOCNO, string APPTIME1, string APPTIME2, string[] str_FLOWID, string MAT_CLASS, string jcn, string towh, string INID, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.* ,
                        (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='FLOWID_EF1' AND DATA_VALUE=A.FLOWID) FLOWID_N,
                        (SELECT MAT_CLSNAME FROM MI_MATCLASS WHERE MAT_CLASS=A.MAT_CLASS) MAT_CLASS_N,
                        (SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.FRWH) FRWH_N,
                        (SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.TOWH) TOWH_N,
                        (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='APPLY_KIND' AND DATA_VALUE=A.APPLY_KIND) APPLY_KIND_N,
                        (SELECT UNA FROM UR_ID WHERE TUSER=A.APPID) APP_NAME,
                        (SELECT INID_NAME FROM UR_INID WHERE INID=A.APPDEPT) APPDEPT_NAME,
                        TWN_DATE(A.APPTIME) APPTIME_T ,
                        (SELECT COUNT(*) AS CNT FROM ME_DOCM 
                            WHERE DOCTYPE='MR2' AND APPLY_KIND='1' 
                            AND APPTIME BETWEEN NEXT_DAY(SYSDATE-7,1) AND NEXT_DAY(SYSDATE-7,1)+6 ) MR2,
                        NVL((SELECT 'Y' FROM DUAL WHERE (SELECT EXTRACT(DAY FROM SYSDATE) FROM DUAL) 
                            BETWEEN (SELECT DATA_VALUE FROM PARAM_D WHERE DATA_NAME = 'MR3_BDAY')
                            AND (SELECT DATA_VALUE FROM PARAM_D WHERE DATA_NAME = 'MR3_EDAY')),'N') MR3,
                        NVL((SELECT 'Y' FROM DUAL WHERE (SELECT EXTRACT(DAY FROM SYSDATE) FROM DUAL) 
                            BETWEEN (SELECT DATA_VALUE FROM PARAM_D WHERE DATA_NAME = 'MR4_BDAY')
                            AND (SELECT DATA_VALUE FROM PARAM_D WHERE DATA_NAME = 'MR4_EDAY')),'N') MR4
                        FROM ME_DOCM A WHERE 1=1 AND A.DOCTYPE = 'EF1' ";

            if (INID != "")
            {
                sql += " AND A.APPDEPT = :inid ";
                p.Add(":inid", string.Format("{0}", INID));
            }
            if (DOCNO != "")
            {
                sql += " AND A.DOCNO LIKE :p0 ";
                p.Add(":p0", string.Format("%{0}%", DOCNO));
            }
            if (APPTIME1 != "" & APPTIME2 != "")
            {
                sql += " AND TWN_DATE(A.APPTIME) BETWEEN :d0 AND :d1 ";
                p.Add(":d0", string.Format("{0}", APPTIME1));
                p.Add(":d1", string.Format("{0}", APPTIME2));
            }
            if (APPTIME1 != "" & APPTIME2 == "")
            {
                sql += " AND TWN_DATE(A.APPTIME) >= :d0 ";
                p.Add(":d0", string.Format("{0}", APPTIME1));
            }
            if (APPTIME1 == "" & APPTIME2 != "")
            {
                sql += " AND TWN_DATE(A.APPTIME) <= :d1 ";
                p.Add(":d1", string.Format("{0}", APPTIME2));
            }
            //判斷FLOWID查詢條件是否有值，有的話用字串相加的方式串接條件(IN的方法會有問題)
            if (str_FLOWID.Length > 0)
            {
                string sql_FLOWID = "";
                sql += @"AND (";
                foreach (string tmp_FLOWID in str_FLOWID)
                {
                    if (string.IsNullOrEmpty(sql_FLOWID))
                    {
                        sql_FLOWID = @"A.FLOWID = '" + tmp_FLOWID + "'";
                    }
                    else
                    {
                        sql_FLOWID += @" OR A.FLOWID = '" + tmp_FLOWID + "'";
                    }
                }
                sql += sql_FLOWID + ") ";
            }
            if (MAT_CLASS != "")
            {
                sql += " AND A.MAT_CLASS = :p3 ";
                p.Add(":p3", string.Format("{0}", MAT_CLASS));
            }
            if (jcn != "")
            {
                sql += " AND A.JCN = :p4 ";
                p.Add(":p4", string.Format("{0}", jcn));
            }
            if (towh != "")
            {
                sql += " AND A.TOWH = :p5 ";
                p.Add(":p5", string.Format("{0}", towh));
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCM>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<ME_DOCD_EC> GetAllD(string DOCNO, string MMCODE, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.* ,
                        B.MMNAME_C, B.MMNAME_E, B.BASE_UNIT, B.DISC_UPRICE M_CONTPRICE, B.M_AGENNO,
                        C.AGEN_NAMEC,B.M_PURUN,
                        (A.APPQTY+A.M_APPQTY+A.S_APPQTY)T_APPQTY,D.EXCH_RATIO,
                        (A.APPQTY/D.EXCH_RATIO) A_PACK,
                        (A.M_APPQTY/D.EXCH_RATIO) M_PACK,
                        (A.S_APPQTY/D.EXCH_RATIO) S_PACK,
                        (A.APPQTY/D.EXCH_RATIO)+(A.M_APPQTY/D.EXCH_RATIO)+(A.S_APPQTY/D.EXCH_RATIO) T_PACK,
                        (B.DISC_UPRICE/D.EXCH_RATIO)MIN_PRICE,
                        ((A.APPQTY/D.EXCH_RATIO)+(A.M_APPQTY/D.EXCH_RATIO)+(A.S_APPQTY/D.EXCH_RATIO))*B.DISC_UPRICE T_PRICE,
                        ( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO IN (
                          SELECT WH_NO FROM MI_WHMAST WHERE WH_KIND='E' AND WH_GRADE='1') ) A_INV_QTY,
                        ( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO IN (
                          SELECT WH_NO FROM MI_WHMAST WHERE WH_KIND='E' AND WH_GRADE='M') ) M_INV_QTY,
                        ( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO IN (
                          SELECT WH_NO FROM MI_WHMAST WHERE WH_KIND='E' AND WH_GRADE='S') ) S_INV_QTY,
                        F.APVQTY A_APV_QTY,
                        F.M_APVQTY M_APV_QTY,
                        F.S_APVQTY S_APV_QTY,
                        A.APPQTY + A.M_APPQTY + A.S_APPQTY TOT_APPQTY,
                        F.APVQTY + F.M_APVQTY + F.S_APVQTY TOT_APVQTY
                        FROM ME_DOCD_EC A
                        INNER JOIN MI_MAST B ON B.MMCODE=A.MMCODE
                        LEFT OUTER JOIN PH_VENDER C ON C.AGEN_NO=B.M_AGENNO 
                        LEFT OUTER JOIN MI_UNITEXCH D ON D.MMCODE=A.MMCODE AND D.UNIT_CODE=B.M_PURUN AND D.AGEN_NO=B.M_AGENNO 
                        INNER JOIN ME_DOCM E ON E.DOCNO=A.DOCNO 
                        LEFT OUTER JOIN MM_ECUSE F ON F.MMCODE=A.MMCODE AND F.USE_DEPT=E.TOWH AND EC_ID='E' AND USE_YM=SUBSTR(TWN_DATE(E.APPTIME),0,5)
                        WHERE 1 = 1 ";

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

            return DBWork.Connection.Query<ME_DOCD_EC>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<ME_DOCM> GetM(string id)
        {
            var sql = @"SELECT A.*,
                        (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='FLOWID_EF1' AND DATA_VALUE=A.FLOWID) FLOWID_N,
                        (SELECT MAT_CLSNAME FROM MI_MATCLASS WHERE MAT_CLASS=A.MAT_CLASS) MAT_CLASS_N,
                        (SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.FRWH) FRWH_N,
                        (SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.TOWH) TOWH_N,
                        (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='APPLY_KIND' AND DATA_VALUE=A.APPLY_KIND) APPLY_KIND_N,
                        (SELECT UNA FROM UR_ID WHERE TUSER=A.APPID) APP_NAME,
                        (SELECT INID_NAME FROM UR_INID WHERE INID=A.APPDEPT) APPDEPT_NAME,
                        TWN_DATE(A.APPTIME) APPTIME_T,
                        (SELECT COUNT(*) AS CNT FROM ME_DOCM 
                            WHERE DOCTYPE='MR2' AND APPLY_KIND='1' 
                            AND APPTIME BETWEEN NEXT_DAY(SYSDATE-7,1) AND NEXT_DAY(SYSDATE-7,1)+6 ) MR2,
                        NVL((SELECT 'Y' FROM DUAL WHERE (SELECT EXTRACT(DAY FROM SYSDATE) FROM DUAL) 
                            BETWEEN (SELECT DATA_VALUE FROM PARAM_D WHERE DATA_NAME = 'MR3_BDAY')
                            AND (SELECT DATA_VALUE FROM PARAM_D WHERE DATA_NAME = 'MR3_EDAY')),'N') MR3,
                        NVL((SELECT 'Y' FROM DUAL WHERE (SELECT EXTRACT(DAY FROM SYSDATE) FROM DUAL) 
                            BETWEEN (SELECT DATA_VALUE FROM PARAM_D WHERE DATA_NAME = 'MR4_BDAY')
                            AND (SELECT DATA_VALUE FROM PARAM_D WHERE DATA_NAME = 'MR4_EDAY')),'N') MR4 
                        FROM ME_DOCM A
                        WHERE A.DOCNO = :DOCNO";
            return DBWork.Connection.Query<ME_DOCM>(sql, new { DOCNO = id }, DBWork.Transaction);
        }
        public IEnumerable<ME_DOCD_EC> GetD(string id,string seq)
        {
            var sql = @"SELECT A.* ,
                        B.MMNAME_C, B.MMNAME_E, B.BASE_UNIT, B.DISC_UPRICE M_CONTPRICE, B.M_AGENNO,
                        C.AGEN_NAMEC,B.M_PURUN,
                        (A.APPQTY+A.M_APPQTY+A.S_APPQTY)T_APPQTY,D.EXCH_RATIO,
                        (A.APPQTY/D.EXCH_RATIO) A_PACK,
                        (A.M_APPQTY/D.EXCH_RATIO) M_PACK,
                        (A.S_APPQTY/D.EXCH_RATIO) S_PACK,
                        (A.APPQTY/D.EXCH_RATIO)+(A.M_APPQTY/D.EXCH_RATIO)+(A.S_APPQTY/D.EXCH_RATIO) T_PACK,
                        (B.DISC_UPRICE/D.EXCH_RATIO)MIN_PRICE,
                        ((A.APPQTY/D.EXCH_RATIO)+(A.M_APPQTY/D.EXCH_RATIO)+(A.S_APPQTY/D.EXCH_RATIO))*B.DISC_UPRICE T_PRICE,
                        ( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO IN (
                          SELECT WH_NO FROM MI_WHMAST WHERE WH_KIND='E' AND WH_GRADE='1') ) A_INV_QTY,
                        ( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO IN (
                          SELECT WH_NO FROM MI_WHMAST WHERE WH_KIND='E' AND WH_GRADE='M') ) M_INV_QTY,
                        ( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO IN (
                          SELECT WH_NO FROM MI_WHMAST WHERE WH_KIND='E' AND WH_GRADE='S') ) S_INV_QTY,
                        F.APVQTY A_APV_QTY,
                        F.M_APVQTY M_APV_QTY,
                        F.S_APVQTY S_APV_QTY,
                        A.APPQTY + A.M_APPQTY + A.S_APPQTY TOT_APPQTY,
                        F.APVQTY + F.M_APVQTY + F.S_APVQTY TOT_APVQTY 
                        FROM ME_DOCD_EC A
                        INNER JOIN MI_MAST B ON B.MMCODE=A.MMCODE
                        LEFT OUTER JOIN PH_VENDER C ON C.AGEN_NO=B.M_AGENNO 
                        LEFT OUTER JOIN MI_UNITEXCH D ON D.MMCODE=A.MMCODE AND D.UNIT_CODE=B.M_PURUN AND D.AGEN_NO=B.M_AGENNO 
                        INNER JOIN ME_DOCM E ON E.DOCNO=A.DOCNO 
                        LEFT OUTER JOIN MM_ECUSE F ON F.MMCODE=A.MMCODE AND F.USE_DEPT=E.TOWH AND EC_ID='E' AND USE_YM=SUBSTR(TWN_DATE(E.APPTIME),0,5)
                        WHERE 1 = 1 
                        AND A.DOCNO = :DOCNO AND A.SEQ= :SEQ";
            return DBWork.Connection.Query<ME_DOCD_EC>(sql, new { DOCNO = id, SEQ = seq }, DBWork.Transaction);
        }

        public int CreateM(ME_DOCM ME_DOCM)
        {
            var sql = @"INSERT INTO ME_DOCM (
                        DOCNO, DOCTYPE, FLOWID , APPID , APPDEPT , 
                        APPTIME , USEID , USEDEPT , FRWH , TOWH , 
                        APPLY_KIND ,APPLY_NOTE ,MAT_CLASS, JCN,
                        CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                      VALUES (
                        :DOCNO, :DOCTYPE, :FLOWID , :APPID , :APPDEPT , 
                        SYSDATE , :USEID , :USEDEPT , :FRWH , :TOWH , 
                        :APPLY_KIND , :APPLY_NOTE ,:MAT_CLASS, :JCN,
                        SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);
        }

        public int CreateD(ME_DOCD_EC ME_DOCD_EC)
        {
            var sql = @"INSERT INTO ME_DOCD_EC (
                        DOCNO, SEQ, MMCODE , APPQTY , APLYITEM_NOTE,
                        M_APPQTY, S_APPQTY,
                        CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                      VALUES (
                        :DOCNO, :SEQ, :MMCODE , :APPQTY ,  :APLYITEM_NOTE, 
                        :M_APPQTY, :S_APPQTY,
                        :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, ME_DOCD_EC, DBWork.Transaction);
        }
        public int UpdateM(ME_DOCM ME_DOCM)
        {
            var sql = @"UPDATE ME_DOCM SET 
                        DOCTYPE = :DOCTYPE, FLOWID = :FLOWID, APPID = :APPID, APPDEPT = :APPDEPT, 
                        APPTIME = SYSDATE, USEID = :USEID, USEDEPT = :USEDEPT, FRWH = :FRWH, TOWH = :TOWH,
                        APPLY_KIND = :APPLY_KIND ,APPLY_NOTE = :APPLY_NOTE, JCN = :JCN,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);
        }
        public int UpdateD(ME_DOCD_EC ME_DOCD_EC)
        {
            var sql = @"UPDATE ME_DOCD_EC SET 
                        MMCODE = :MMCODE, APPQTY = :APPQTY, APLYITEM_NOTE = :APLYITEM_NOTE,
                        M_APPQTY = :M_APPQTY, S_APPQTY = :S_APPQTY,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO AND SEQ = :SEQ ";
            return DBWork.Connection.Execute(sql, ME_DOCD_EC, DBWork.Transaction);
        }
        public int ApplyM(ME_DOCM ME_DOCM)
        {
            var sql = @"UPDATE ME_DOCM SET FLOWID = '2',
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);

        }
        public int ApplyD(ME_DOCM ME_DOCM)
        {
            var sql = @"UPDATE ME_DOCD_EC SET APVID = :UPDATE_USER, APVTIME = SYSDATE, 
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);

        }
        public string CallProc(string id, string code, string upuser, string upip)
        {
            var p = new OracleDynamicParameters();
            p.Add("I_DOCNO", value: id, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 21);
            p.Add("I_MCODE", value: code, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 1);
            p.Add("I_UPDUSR", value: upuser, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 8);
            p.Add("I_UPDIP", value: upip, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 20);

            p.Add("O_RETID", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 1);
            p.Add("O_ERRMSG", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 255);

            DBWork.Connection.Query("POST_DOC_EC", p, commandType: CommandType.StoredProcedure);
            string retid = p.Get<OracleString>("O_RETID").Value;
            string errmsg = p.Get<OracleString>("O_ERRMSG").Value;
            if (retid == "N")
            {
                retid = errmsg;
            }
            return retid;
        }
        public int DeleteM(string docno)
        {
            var sql = @" DELETE from ME_DOCM WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno }, DBWork.Transaction);
        }
        public int DeleteAllD(string docno)
        {
            var sql = @" DELETE from ME_DOCD_EC WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public int DeleteD(string docno, string seq)
        {
            var sql = @" DELETE from ME_DOCD_EC WHERE DOCNO=:DOCNO AND SEQ=:SEQ";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno, SEQ = seq }, DBWork.Transaction);
        }
        public bool CheckMmcode(string id, string doc)
        {
            string sql = @"SELECT 1 FROM MI_MAST WHERE MMCODE=:MMCODE
                            AND M_STOREID = '1'
                            AND M_CONTID <> '3'
                            AND M_APPLYID <> 'E'
                            AND EXISTS ( SELECT 1 FROM ME_DOCM WHERE MAT_CLASS=MI_MAST.MAT_CLASS AND DOCNO=:DOCNO)
                        ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = id, DOCNO = doc }, DBWork.Transaction) == null);
        }
        public bool CheckExistsM(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCM WHERE DOCNO=:DOCNO AND FLOWID <> '1'";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        public bool CheckExistsM2(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCM WHERE DOCNO=:DOCNO AND FLOWID <> '2'";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        public bool CheckExists(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCM WHERE DOCNO=:DOCNO";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        public bool CheckExistsD(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCD_EC WHERE DOCNO=:DOCNO ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        public bool CheckExistsDD(string id, string sid)
        {
            string sql = @"SELECT 1 FROM ME_DOCD_EC WHERE DOCNO=:DOCNO AND SEQ=:SEQ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id, SEQ = sid }, DBWork.Transaction) == null);
        }
        public bool CheckExistsDN(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCD_EC WHERE DOCNO=:DOCNO AND (APPQTY+M_APPQTY+S_APPQTY) = 0 ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        public bool CheckExistsMM(string id, string mm)
        {
            string sql = @"SELECT 1 FROM ME_DOCD_EC WHERE DOCNO=:DOCNO AND MMCODE=:MMCODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id, MMCODE = mm }, DBWork.Transaction) == null);
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
                        WHERE 1=1 AND WH_KIND = '1' AND WH_GRADE = '1'  AND ROWNUM=1";
            string rtn = DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction).ToString();
            return rtn;
        }
        public string GetDocDSeq(string id)
        {
            string sql = @"SELECT MAX(SEQ)+1 as SEQ FROM ME_DOCD_EC WHERE DOCNO=:DOCNO ";
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
        public IEnumerable<COMBO_MODEL> GetDocnoCombo(string id)
        {
            string sql = @"SELECT DISTINCT DOCNO as VALUE, DOCNO as TEXT,
                        DOCNO as COMBITEM
                        FROM ME_DOCM 
                        WHERE 1=1 AND DOCTYPE = 'EF1' 
                        AND APPDEPT=:APPDEPT
                        ORDER BY DOCNO";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { APPDEPT = id });
        }
        public IEnumerable<COMBO_MODEL> GetJcnCombo(string id)
        {
            string sql = @"SELECT DISTINCT JCN as VALUE, JCN as TEXT,
                        JCN as COMBITEM
                        FROM ME_DOCM 
                        WHERE 1=1 AND DOCTYPE = 'EF1' 
                        AND APPDEPT=:APPDEPT
                        ORDER BY JCN";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { APPDEPT = id });
        }
        public IEnumerable<COMBO_MODEL> GetFlowidCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='FLOWID_EF1' 
                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<MI_MAST> GetMmCodeCombo(string p0, string p1, string p2, int page_index, int page_size, string sorters)
        {

            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT,A.M_CONTPRICE,
                       (select exch_ratio from MI_UNITEXCH where mmcode = a.mmcode and a.m_agenno = agen_no and unit_code = a.m_purun) as exch_ratio,
                        ( SELECT AVG_PRICE FROM MI_WHCOST WHERE MMCODE = A.MMCODE AND DATA_YM=CUR_SETYM AND ROWNUM=1 ) AS AVG_PRICE,
                        ( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO=:TOWH ) AS INV_QTY,
                        ( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO IN (
                          SELECT WH_NO FROM MI_WHMAST WHERE WH_KIND='E' AND WH_GRADE='1') ) A_INV_QTY,
                        ( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO IN (
                          SELECT WH_NO FROM MI_WHMAST WHERE WH_KIND='E' AND WH_GRADE='M') ) M_INV_QTY,
                        ( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO IN (
                          SELECT WH_NO FROM MI_WHMAST WHERE WH_KIND='E' AND WH_GRADE='S') ) S_INV_QTY,
                        ( SELECT AVG_APLQTY FROM V_MM_AVGAPL WHERE MMCODE = A.MMCODE AND WH_NO=:TOWH ) AS AVG_APLQTY 
                        FROM MI_MAST A WHERE 1=1 AND A.MAT_CLASS = :MAT_CLASS 
                          AND A.M_STOREID = '1'
                          AND A.M_CONTID <> '3'
                          AND A.M_APPLYID <> 'E' ";

            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("%{0}%", p0));

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
            p.Add(":TOWH", p2);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<MI_MAST> GetMmcode(MI_MAST_QUERY_PARAMS query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT,A.M_CONTPRICE,
                           (select exch_ratio from MI_UNITEXCH where mmcode = a.mmcode and a.m_agenno = agen_no and unit_code = a.m_purun) as exch_ratio,
                        ( SELECT AVG_PRICE FROM MI_WHCOST WHERE MMCODE = A.MMCODE AND DATA_YM=CUR_SETYM AND ROWNUM=1 ) AS AVG_PRICE,
                        ( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO=:WH_NO ) AS INV_QTY,
                        ( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO IN (
                          SELECT WH_NO FROM MI_WHMAST WHERE WH_KIND='E' AND WH_GRADE='1') ) A_INV_QTY,
                        ( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO IN (
                          SELECT WH_NO FROM MI_WHMAST WHERE WH_KIND='E' AND WH_GRADE='M') ) M_INV_QTY,
                        ( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO IN (
                          SELECT WH_NO FROM MI_WHMAST WHERE WH_KIND='E' AND WH_GRADE='S') ) S_INV_QTY,
                        ( SELECT AVG_APLQTY FROM V_MM_AVGAPL WHERE MMCODE = A.MMCODE AND WH_NO=:WH_NO ) AS AVG_APLQTY 
                        FROM MI_MAST A WHERE 1=1 AND A.MAT_CLASS = :MAT_CLASS 
                          AND A.M_STOREID = '1'
                          AND A.M_CONTID <> '3'
                          AND A.M_APPLYID <> 'E' ";

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
            p.Add(":WH_NO", query.WH_NO);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<MI_MAST> GetMMCodeDocd(string p0, string p1, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E , C.EXCH_RATIO 
                        FROM MI_MAST A, ME_DOCD_EC B, MI_UNITEXCH C
                       WHERE A.MMCODE=B.MMCODE AND B.DOCNO = :DOCNO 
                        AND A.M_AGENNO = C.AGEN_NO and A.MMCODE = C.MMCODE and c.unit_code = a.m_purun";
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
                        FROM MI_WHMAST A
                        WHERE A.WH_KIND = '1' 
                          and a.cancel_id = 'N'
                        ORDER BY WH_NO";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetMatclassCombo()
        {
            string sql = @"SELECT MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, 
                        MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM 
                        FROM MI_MATCLASS WHERE MAT_CLSID='4'   
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
        //匯出
        public DataTable GetExcel3(string apptime1, string apptime2)
        {
            var p = new DynamicParameters();

            var sql = @"
                    SELECT A.TOWH 責任中心,WH_NAME(A.TOWH) 單位名稱,C.MAT_CLASS 類別,B.MMCODE 院內碼,C.MMNAME_C 中文品名,C.BASE_UNIT 計量單位,
                    '醫院軍' 軍民別,M_APPQTY 數量,
                    NVL((SELECT AVG_PRICE FROM MI_WHCOST_EC WHERE DATA_YM=SUBSTR(TWN_DATE(A.APPTIME),1,5) AND MMCODE=B.MMCODE AND MC_ID='1'),C.DISC_UPRICE) 消耗單價,
                    NVL((SELECT AVG_PRICE FROM MI_WHCOST_EC WHERE DATA_YM=SUBSTR(TWN_DATE(A.APPTIME),1,5) AND MMCODE=B.MMCODE AND MC_ID='1'),C.DISC_UPRICE)
                    *(M_APPQTY) 消耗總價,
                    A.DOCNO 申請單號,A.JCN 工單編號,NVL(B.APVTIME,A.APPTIME) 交易日期
                    FROM ME_DOCM A, ME_DOCD_EC B, MI_MAST C
                    WHERE A.DOCNO=B.DOCNO AND B.MMCODE=C.MMCODE
                          AND DOCTYPE='EF1' AND FLOWID='2'
                    ";
            if (apptime1 != "" & apptime2 != "")
            {
                sql += " AND  TWN_DATE(NVL(B.APVTIME,A.APPTIME)) BETWEEN :p0 AND :p1";
                p.Add(":p0", string.Format("{0}", apptime1));
                p.Add(":p1", string.Format("{0}", apptime2));
            }
            if (apptime1 != "" & apptime2 == "")
            {
                sql += " AND  TWN_DATE(NVL(B.APVTIME,A.APPTIME)) >= :p0 ";
                p.Add(":p0", string.Format("{0}", apptime1));
            }
            if (apptime1 == "" & apptime2 != "")
            {
                sql += " AND  TWN_DATE(NVL(B.APVTIME,A.APPTIME)) <= :p1 ";
                p.Add(":p1", string.Format("{0}", apptime2));
            }
            sql += @"
                    UNION ALL 
                    SELECT A.TOWH 責任中心,WH_NAME(A.TOWH) 單位名稱,C.MAT_CLASS 類別,B.MMCODE 院內碼,C.MMNAME_C 中文品名,C.BASE_UNIT 計量單位,
                    '醫院民' 軍民別,
                    APPQTY 數量,
                    NVL((SELECT AVG_PRICE FROM MI_WHCOST_EC WHERE DATA_YM=SUBSTR(TWN_DATE(A.APPTIME),1,5) AND MMCODE=B.MMCODE AND MC_ID='2'),C.DISC_UPRICE) 消耗單價,
                    NVL((SELECT AVG_PRICE FROM MI_WHCOST_EC WHERE DATA_YM=SUBSTR(TWN_DATE(A.APPTIME),1,5) AND MMCODE=B.MMCODE AND MC_ID='2'),C.DISC_UPRICE)
                    *(APPQTY) 消耗總價,
                    A.DOCNO 申請單號,A.JCN 工單編號,NVL(B.APVTIME,A.APPTIME) 交易日期
                    FROM ME_DOCM A, ME_DOCD_EC B, MI_MAST C
                    WHERE A.DOCNO=B.DOCNO AND B.MMCODE=C.MMCODE
                          AND DOCTYPE='EF1' AND FLOWID='2'
                    ";
            if (apptime1 != "" & apptime2 != "")
            {
                sql += " AND  TWN_DATE(NVL(B.APVTIME,A.APPTIME)) BETWEEN :p0 AND :p1";
                p.Add(":p0", string.Format("{0}", apptime1));
                p.Add(":p1", string.Format("{0}", apptime2));
            }
            if (apptime1 != "" & apptime2 == "")
            {
                sql += " AND  TWN_DATE(NVL(B.APVTIME,A.APPTIME)) >= :p0 ";
                p.Add(":p0", string.Format("{0}", apptime1));
            }
            if (apptime1 == "" & apptime2 != "")
            {
                sql += " AND  TWN_DATE(NVL(B.APVTIME,A.APPTIME)) <= :p1 ";
                p.Add(":p1", string.Format("{0}", apptime2));
            }
            sql += @"
                    UNION ALL 
                    SELECT A.TOWH 責任中心,WH_NAME(A.TOWH) 單位名稱,C.MAT_CLASS 類別,B.MMCODE 院內碼,C.MMNAME_C 中文品名,C.BASE_UNIT 計量單位,
                    '學院軍' 軍民別,
                    S_APPQTY 數量,
                    NVL((SELECT AVG_PRICE FROM MI_WHCOST_EC WHERE DATA_YM=SUBSTR(TWN_DATE(A.APPTIME),1,5) AND MMCODE=B.MMCODE AND MC_ID='3'),C.DISC_UPRICE) 消耗單價,
                    NVL((SELECT AVG_PRICE FROM MI_WHCOST_EC WHERE DATA_YM=SUBSTR(TWN_DATE(A.APPTIME),1,5) AND MMCODE=B.MMCODE AND MC_ID='3'),C.DISC_UPRICE)
                    *(S_APPQTY) 消耗總價,
                    A.DOCNO 申請單號,A.JCN 工單編號,NVL(B.APVTIME,A.APPTIME) 交易日期
                    FROM ME_DOCM A, ME_DOCD_EC B, MI_MAST C
                    WHERE A.DOCNO=B.DOCNO AND B.MMCODE=C.MMCODE
                          AND DOCTYPE='EF1' AND FLOWID='2'
                     ";
            if (apptime1 != "" & apptime2 != "")
            {
                sql += " AND  TWN_DATE(NVL(B.APVTIME,A.APPTIME)) BETWEEN :p0 AND :p1";
                p.Add(":p0", string.Format("{0}", apptime1));
                p.Add(":p1", string.Format("{0}", apptime2));
            }
            if (apptime1 != "" & apptime2 == "")
            {
                sql += " AND  TWN_DATE(NVL(B.APVTIME,A.APPTIME)) >= :p0 ";
                p.Add(":p0", string.Format("{0}", apptime1));
            }
            if (apptime1 == "" & apptime2 != "")
            {
                sql += " AND  TWN_DATE(NVL(B.APVTIME,A.APPTIME)) <= :p1 ";
                p.Add(":p1", string.Format("{0}", apptime2));
            }
            sql += " ORDER BY 1,11,7 DESC ";
            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
        //匯出
        public DataTable GetExcel4(string apptime1, string apptime2)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.MMCODE 院內碼,B.MAT_CLASS 類別,B.MMNAME_C 中文品名,B.BASE_UNIT 計量單位, SUM(APVQTY+M_APVQTY+S_APVQTY) 消耗量
                        FROM MM_ECUSE A,MI_MAST B
                        WHERE A.MMCODE=B.MMCODE AND EC_ID='E' ";
            if (apptime1 != "" & apptime2 != "")
            {
                sql += " AND  A.USE_YM BETWEEN :p0 AND :p1";
                p.Add(":p0", string.Format("{0}", apptime1));
                p.Add(":p1", string.Format("{0}", apptime2));
            }
            if (apptime1 != "" & apptime2 == "")
            {
                sql += " AND  A.USE_YM >= :p0 ";
                p.Add(":p0", string.Format("{0}", apptime1));
            }
            if (apptime1 == "" & apptime2 != "")
            {
                sql += " AND  A.USE_YM <= :p1 ";
                p.Add(":p1", string.Format("{0}", apptime2));
            }
            sql += " GROUP BY A.MMCODE,B.MAT_CLASS,B.MMNAME_C,B.BASE_UNIT ";
            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
        public IEnumerable<AB0062ReportMODEL> GetPrintData(string docno)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.DOCNO AS F01,
                        (SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.FRWH) F02,
                        (SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.TOWH) F03,
                        (SELECT C.MAT_CLASS|| ' ' || MAT_CLSNAME FROM MI_MATCLASS WHERE MAT_CLASS=C.MAT_CLASS) F04,
                        B.MMCODE AS F05,(C.MMNAME_C||' '||C.MMNAME_E) AS F06,
                        C.BASE_UNIT AS F07,TWN_DATE(A.APPTIME) F08,
                        B.APPQTY F09,TWN_DATE(B.ACKTIME) F10,
                        B.ACKQTY AS F11,TWN_DATE(B.APVTIME) F12,B.APVQTY AS F13
                        FROM ME_DOCM A,ME_DOCD B,MI_MAST C
                        WHERE A.DOCNO=B.DOCNO AND B.MMCODE=C.MMCODE
                        AND A.DOCTYPE IN ('TR1') AND A.FLOWID='0299'  ";
            
            if (docno != "")
            {
                sql += " AND A.DOCNO = :p0 ";
                p.Add(":p0", string.Format("{0}", docno));
            }
            

            return DBWork.Connection.Query<AB0062ReportMODEL>(sql, p, DBWork.Transaction);
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

        #region 2021-07-30 新增: 新增主檔、送核撥 檢查申請庫房是否作廢
        public bool CheckIsTowhCancelByWhno(string towh)
        {
            string sql = @"
               select 1 from MI_WHMAST 
                where wh_no = :towh
                  and cancel_id = 'N'
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { towh }, DBWork.Transaction) == null;
        }

        public bool CheckIsTowhCancelByDocno(string docno)
        {
            string sql = @"
               select 1 from ME_DOCM  a
                where a.docno = :docno
                  and exists (select 1 from MI_WHMAST 
                               where wh_no = a.towh
                                 and cancel_id = 'N')
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { docno }, DBWork.Transaction) == null;
        }
        #endregion
    }
}
