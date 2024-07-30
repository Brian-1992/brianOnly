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
    public class AA0020Repository : JCLib.Mvc.BaseRepository
    {
        public AA0020Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ME_EXPM> GetAllM(string flowid, string mmcode, string agenno, string expdate, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.*,
                        B.MMNAME_C,B.MMNAME_E,B.BASE_UNIT,B.M_CONTPRICE,
                        E.AGEN_NAMEC M_AGENNO,
                        TWN_DATE(A.EXP_DATE) EXP_DATE_T, TO_CHAR(A.EXP_DATE,'YYYYMMDD') EXP_DATE_N,
                        (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_EXPM' AND DATA_NAME='CLOSEFLAG' AND DATA_VALUE=A.CLOSEFLAG) CLOSEFLAG_N,
                        NVL((SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='FLOWID_XE' AND DATA_VALUE=F.FLOWID),'未處理') FLOWID_N,
                        F.DOCNO AS DOCNO_M, B.M_DISCPERC, F.FLOWID   
                        FROM ME_EXPM A 
                        INNER JOIN MI_MAST B ON B.MMCODE=A.MMCODE
                        LEFT OUTER JOIN PH_VENDER E ON E.AGEN_NO=B.M_AGENNO
                        LEFT OUTER JOIN ME_DOCM F ON F.DOCNO=A.RDOCNO 
                        WHERE 1=1  ";
            
            if (flowid != "")
            {
                if (flowid == "1400")
                {
                    sql += " AND F.FLOWID IS NULL ";
                }
                else
                {
                    sql += " AND F.FLOWID = :p0 ";
                    p.Add(":p0", string.Format("{0}", flowid));
                }

            }

            if (mmcode != "")
            {
                sql += " AND A.MMCODE = :p1 ";
                p.Add(":p1", string.Format("{0}", mmcode));
            }
            if (agenno != "")
            {
                sql += " AND B.M_AGENNO = :p2 ";
                p.Add(":p2", string.Format("{0}", agenno));
            }
            if (expdate != "")
            {
                sql += " AND SUBSTR(TO_CHAR(A.EXP_DATE,'YYYYMMDD'),1,6) = :p3 ";
                p.Add(":p3", string.Format("{0}", expdate));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_EXPM>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<ME_DOCEXP> GetAllD(string docno, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.*,B.MMNAME_C,B.MMNAME_E,B.BASE_UNIT,B.M_CONTPRICE,
                        B.M_AGENNO,C.AGEN_NAMEC,C.AGEN_NAMEE,
                        TWN_DATE(A.EXP_DATE) EXP_DATET 
                        FROM ME_DOCEXP A 
                        INNER JOIN MI_MAST B ON B.MMCODE=A.MMCODE
                        LEFT OUTER JOIN PH_VENDER C ON C.AGEN_NO=B.M_AGENNO
                        WHERE 1=1 ";

            if (docno != "")
            {
                sql += " AND A.DOCNO = :p0 ";
                p.Add(":p0", string.Format("{0}", docno));
            }
            else
            {
                sql += "AND 1=2 ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCEXP>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public int CreateMedoce(ME_DOCEXP ME_DOCEXP)
        {
            var sql = @"INSERT INTO ME_DOCEXP (
                        DOCNO, SEQ, EXP_DATE, MMCODE , APVQTY , 
                        LOT_NO, MEMO, PROC_ID, 
                        CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                      VALUES (
                        :DOCNO, :SEQ, :EXP_DATE, :MMCODE , :APVQTY ,  
                        :LOT_NO, :MEMO, :PROC_ID, 
                        :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, ME_DOCEXP, DBWork.Transaction);
        }
        public int CreateMedocm(ME_DOCM me_dcom)
        {
            var sql = @"INSERT INTO ME_DOCM (
                        DOCNO, DOCTYPE, FLOWID , FRWH , TOWH , 
                        APPLY_NOTE ,MAT_CLASS,
                        CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                      VALUES (
                        :DOCNO, :DOCTYPE, :FLOWID , :FRWH , :TOWH , 
                        :APPLY_NOTE ,:MAT_CLASS,
                        SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, me_dcom, DBWork.Transaction);
        }
        public int CreateMedocd(ME_DOCD me_docd)
        {
            var sql = @"INSERT INTO ME_DOCD (
                        DOCNO, SEQ, MMCODE , APPQTY , STAT, APLYITEM_NOTE,
                        CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                      VALUES (
                        :DOCNO, :SEQ, :MMCODE , :APPQTY ,  :STAT, :APLYITEM_NOTE,
                        SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, me_docd, DBWork.Transaction);
        }
        public int CreateMedocExp(ME_DOCEXP me_docexp)
        {
            var sql = @"INSERT INTO ME_DOCEXP (
                        DOCNO, SEQ, MMCODE , APVQTY , LOT_NO,
                        EXP_DATE,ITEM_NOTE,C_TYPE,
                        UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                      VALUES (
                        :DOCNO, :SEQ, :MMCODE , :APVQTY ,  :LOT_NO,
                        TO_DATE(:EXP_DATE,'YYYY/MM/DD'),:ITEM_NOTE, :C_TYPE,
                        SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, me_docexp, DBWork.Transaction);
        }
        public int UpdateMedoce(ME_DOCEXP ME_DOCEXP)
        {
            var sql = @"UPDATE ME_DOCEXP SET 
                        MMCODE = :MMCODE, APVQTY = :APVQTY, LOT_NO = :LOT_NO, ITEM_NOTE = :ITEM_NOTE, 
                        C_TYPE = :C_TYPE, EXP_DATE = TO_DATE(:EXP_DATE,'YYYY/MM/DD'), 
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO AND SEQ = :SEQ ";
            return DBWork.Connection.Execute(sql, ME_DOCEXP, DBWork.Transaction);
        }
        public int UpdateMedocm(ME_DOCM me_dcom)
        {
            var sql = @"UPDATE ME_DOCM SET 
                        FLOWID = :FLOWID, APPLY_NOTE = :APPLY_NOTE,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, me_dcom, DBWork.Transaction);
        }
        public int UpdateMedocd(ME_DOCD me_docd)
        {
            var sql = @"UPDATE ME_DOCD SET 
                        MMCODE = :MMCODE, APPQTY = :APPQTY, AMT = :AMT,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO AND SEQ = :SEQ ";
            return DBWork.Connection.Execute(sql, me_docd, DBWork.Transaction);
        }
        public int UpdateMeexpm(ME_EXPM me_expm)
        {
            var sql = @"UPDATE ME_EXPM SET CLOSEFLAG = :CLOSEFLAG, RDOCNO = :RDOCNO, WARNYM = :WARNYM,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE MMCODE = :MMCODE AND TWN_DATE(EXP_DATE) = :EXP_DATE  
                        AND LOT_NO = :LOT_NO ";
            return DBWork.Connection.Execute(sql, me_expm, DBWork.Transaction);
        }
        public int ApplyMeexpm(ME_EXPM me_expm)
        {
            var sql = @"UPDATE ME_EXPM SET CLOSEFLAG = 'Y',
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE MMCODE = :MMCODE AND TWN_DATE(EXP_DATE) = :EXP_DATE 
                        AND LOT_NO = :LOT_NO ";
            return DBWork.Connection.Execute(sql, me_expm, DBWork.Transaction);
        }
        public int ApplyM(ME_DOCEXP ME_DOCEXP)
        {
            var sql = @"UPDATE ME_DOCEXP SET FLOWID = '3',
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, ME_DOCEXP, DBWork.Transaction);

        }
        public int ApplyD(ME_DOCEXP ME_DOCEXP)
        {
            var sql = @"UPDATE ME_DOCD SET PICK_QTY = EXPT_DISTQTY, ACKQTY = EXPT_DISTQTY,
                        DIS_USER = :UPDATE_USER,DIS_TIME = SYSDATE,
                        APL_CONTIME = SYSDATE,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, ME_DOCEXP, DBWork.Transaction);

        }

        public string GetTwndate(string dt)
        {
            string sql = @"SELECT TWN_DATE(TO_DATE(:pDate,'YYYY/MM/DD')) TWNDATE FROM DUAL ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { pDate = dt }, DBWork.Transaction).ToString();
            return rtn;
        }
        public DateTime Getdatetime(string dt)
        {
            string sql = @"SELECT TO_DATE(:pDate || '00:00:00','YYYY/MM/DD HH24:MI:SS') TWNDATE FROM DUAL ";
            DateTime rtn = Convert.ToDateTime(DBWork.Connection.ExecuteScalar(sql, new { pDate = dt }, DBWork.Transaction));
            return rtn;
        }
        public bool CheckExistsMM(string id, string mm)
        {
            string sql = @"SELECT 1 FROM ME_DOCEXP WHERE DOCNO=:DOCNO AND MMCODE=:MMCODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id, MMCODE = mm }, DBWork.Transaction) == null);
        }
        public bool CheckExists(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCEXP WHERE DOCNO=:DOCNO";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        public bool CheckExistsD(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        public bool CheckExistsExp(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCEXP WHERE DOCNO=:DOCNO ";
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
        public string GetDocno()
        {
            var p = new OracleDynamicParameters();
            p.Add("O_DOCNO", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 12);

            DBWork.Connection.Query("GET_DOCNO", p, commandType: CommandType.StoredProcedure);
            return p.Get<OracleString>("O_DOCNO").Value;
        }
        public string GetDocDSeq(string id)
        {
            string sql = @"SELECT MAX(SEQ)+1 as SEQ FROM ME_DOCD WHERE DOCNO=:DOCNO ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction).ToString();
            return rtn;
        }
        public string GetDocExpApvqty(string doc, string mmcode)
        {
            string sql = @"SELECT SUM(APVQTY) as APVQTY FROM ME_DOCEXP WHERE DOCNO=:DOCNO AND MMCODE=:MMCODE  ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { DOCNO = doc ,MMCODE = mmcode }, DBWork.Transaction).ToString();
            return rtn;
        }
        public string GetDocExpNote(string doc, string mmcode)
        {
            string sql = @"SELECT LISTAGG(ITEM_NOTE, ',') WITHIN GROUP (ORDER BY SEQ)  AS NOTE FROM ME_DOCEXP WHERE DOCNO=:DOCNO AND MMCODE=:MMCODE  ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { DOCNO = doc, MMCODE = mmcode }, DBWork.Transaction).ToString();
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
        public string GetDocMFlowid(string doc)
        {
            string sql = @"SELECT FLOWID FROM ME_DOCM WHERE DOCNO=:DOCNO  ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { DOCNO = doc }, DBWork.Transaction).ToString();
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
        public string GetTaskid(string id)
        {
            string sql = @"SELECT TASK_ID FROM MI_WHID WHERE WH_USERID=:WH_USERID 
                            AND WH_NO IN (SELECT WH_NO FROM MI_WHMAST WHERE WH_KIND='1' AND WH_GRADE='1')";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { WH_USERID = id }, DBWork.Transaction).ToString();
            return rtn;
        }
        public IEnumerable<COMBO_MODEL> GetAgenCombo()
        {
            string sql = @"SELECT DISTINCT AGEN_NO as VALUE, AGEN_NAMEC as TEXT,
                        AGEN_NO || ' ' || AGEN_NAMEC as COMBITEM
                        FROM PH_VENDER WHERE 1=1  
                        ORDER BY AGEN_NO";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetYyymmCombo()
        {
            string sql = @"SELECT tt.YYYMM as VALUE,tt.YYYMM as TEXT, substr(tt.YYYMM,1,4)||'年'||substr(tt.YYYMM,5,6)||'月' as COMBITEM 
                        FROM( select substr(to_char(add_months(trunc(sysdate, 'MM'), level - 1),'YYYYMMDD'),1,6) YYYMM 
                                from dual connect by level <= 8 ) tt 
                        ORDER BY YYYMM ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetProcidCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='ME_DOCE' AND DATA_NAME='PROC_ID' 
                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetCloseflagCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='ME_EXPM' AND DATA_NAME='CLOSEFLAG' 
                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetFlowidCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='FLOWID_XE' 
                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<MI_MAST> GetMmCodeCombo(string p0, int page_index, int page_size, string sorters)
        {

            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT,
                        A.M_CONTPRICE, A.M_DISCPERC   
                        FROM MI_MAST A WHERE 1=1  ";

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

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<MI_MAST> GetMmcode(MI_MAST_QUERY_PARAMS query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT,
                        A.M_CONTPRICE, A.M_DISCPERC   
                        FROM MI_MAST A WHERE 1=1 ";

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
    }
}
