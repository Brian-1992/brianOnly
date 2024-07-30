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
    public class AA0031Repository : JCLib.Mvc.BaseRepository
    {
        public AA0031Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ME_DOCM> GetAllM(string docno, string APPTIME1, string APPTIME2, string MAT_CLASS, string FLOWID, string INID, string task,int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.* ,
                        (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='FLOWID_XR1' AND DATA_VALUE=A.FLOWID) FLOWID_N,
                        (SELECT MAT_CLSNAME FROM MI_MATCLASS WHERE MAT_CLASS=A.MAT_CLASS) MAT_CLASS_N,
                        (SELECT UNA FROM UR_ID WHERE TUSER=A.APPID) APP_NAME,
                        (SELECT INID_NAME FROM UR_INID WHERE INID=A.APPDEPT) APPDEPT_NAME,
                        TWN_DATE(A.APPTIME) APPTIME_T 
                        FROM ME_DOCM A ,MI_MATCLASS B WHERE A.MAT_CLASS=B.MAT_CLASS AND A.DOCTYPE = 'XR1' ";
            if (docno != "")
            {
                sql += " AND A.DOCNO = :p0 ";
                p.Add(":p0", string.Format("{0}", docno));
            }
            if (INID != "")
            {
                sql += " AND A.APPDEPT = :inid ";
                p.Add(":inid", string.Format("{0}", INID));
            }
            //if (task != "")
            //{
            //    sql += " AND B.MAT_CLSID = :task ";
            //    p.Add(":task", string.Format("{0}", task));
            //}

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
            if (MAT_CLASS != "")
            {
                sql += " AND A.MAT_CLASS = :p1 ";
                p.Add(":p1", string.Format("{0}", MAT_CLASS));
            }
            if (FLOWID != "")
            {
                sql += " AND A.FLOWID = :p2 ";
                p.Add(":p2", string.Format("{0}", FLOWID));
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCM>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<ME_DOCD> GetAllD(string DOCNO, string MMCODE, string stat,int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.DOCNO,A.SEQ,A.MMCODE ,A.APPQTY,A.APVQTY,A.APVTIME       
                              ,A.APVID,A.ACKQTY,A.ACKID,A.ACKTIME ,A.STAT ,A.RDOCNO        
                              ,A.RSEQ ,A.EXPT_DISTQTY ,A.DIS_USER,A.DIS_TIME ,A.BW_MQTY       
                              ,A.BW_SQTY ,A.PICK_QTY,A.PICK_USER ,A.PICK_TIME,A.ONWAY_QTY     
                              ,A.APL_CONTIME ,A.AMT,A.UP,A.RV_MQTY,A.GTAPL_RESON,A.APLYITEM_NOTE 
                              ,A.CREATE_TIME,A.CREATE_USER ,A.UPDATE_TIME ,A.UPDATE_USER ,A.UPDATE_IP     
                              ,A.EXP_STATUS,A.MEDNO,A.NRCODE ,A.BEDNO,A.FRWH_D        
                              ,A.ORDERDATE,A.CHINNAME,A.VISITSEQ,A.E_DRUGCLASSIFY,A.S_INV_QTY     
                              ,A.INV_QTY,A.SAFE_QTY ,A.OPER_QTY,A.PACK_QTY ,A.PACK_UNIT     
                              ,A.E_ORDERDCFLAG,A.CONFIRMSWITCH,A.TRANSKIND,A.POSTID        
                              ,A.AVG_PRICE,A.ACKQTYT ,A.PMMCODE ,A.PQTY ,A.TRNAB_RESON   
                              ,A.TRNAB_QTY,A.PACK_TIMES ,A.ISSPLIT,A.M_AGENNO  ,A.ACC_ACKQTY    
                              ,A.SRCDOCNO,A.M_CONTPRICE,
                              B.MMNAME_C, B.MMNAME_E, B.BASE_UNIT,
                              (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCD' AND DATA_NAME='STAT' AND DATA_VALUE=A.STAT) STAT_N,
                              NVL(( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO IN (
                                  SELECT WH_NO  FROM MI_WHMAST  WHERE 1=1 AND WH_KIND = '1' AND WH_GRADE = '1'  AND ROWNUM=1 ) ),0) AS A_INV_QTY ,
                              NVL(( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO IN (
                                  SELECT WH_NO  FROM MI_WHMAST  WHERE 1=1 AND WH_KIND = '1' AND WH_GRADE = '5'  AND ROWNUM=1 ) ),0) AS B_INV_QTY ,
                               nvl( (select trunc(sum(x.appqty * Y.MIL_PRICE), 2 )
                                       FROM ME_DOCD X, MI_WHCOST Y 
                                      WHERE X.DOCNO=A.DOCNO AND X.MMCODE=Y.MMCODE AND Y.DATA_YM=CUR_SETYM AND X.STAT ='2'), 0) as sum_ex,
                              nvl( (select trunc(sum(x.appqty * Y.MIL_PRICE), 2 )
                                      FROM ME_DOCD X, MI_WHCOST Y 
                                     WHERE X.DOCNO=A.DOCNO AND X.MMCODE=Y.MMCODE AND Y.DATA_YM=CUR_SETYM AND X.STAT ='1'), 0) as sum_in,
                              D.MIL_PRICE as MIL_PRICE,
                              NVL(TRUNC(D.MIL_PRICE,2),0) * A.APPQTY as TOT_PRICE,
                              d.uprice as uprice, d.mil_price as mil_price_temp
                        FROM ME_DOCD A,MI_MAST B, ME_DOCM C, MI_WHCOST D
                       WHERE C.DOCNO=A.DOCNO AND A.MMCODE = B.MMCODE 
                         AND D.MMCODE=A.MMCODE  AND D.DATA_YM=CUR_SETYM ";

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
            if (stat != "")
            {
                sql += " AND A.STAT = :p2 ";
                p.Add(":p2", string.Format("{0}", stat));
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
                        (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='APPLY_KIND' AND DATA_VALUE=A.APPLY_KIND) APPLY_KIND_N,
                        (SELECT UNA FROM UR_ID WHERE TUSER=A.APPID) APP_NAME,
                        (SELECT INID_NAME FROM UR_INID WHERE INID=A.APPDEPT) APPDEPT_NAME,
                        TWN_DATE(A.APPTIME) APPTIME_T 
                        FROM ME_DOCM A
                        WHERE A.DOCNO = :DOCNO";
            return DBWork.Connection.Query<ME_DOCM>(sql, new { DOCNO = id }, DBWork.Transaction);
        }
        public IEnumerable<ME_DOCD> GetD(string id)
        {
            var sql = @"SELECT A.* ,
                        B.MMNAME_C, B.MMNAME_E, B.BASE_UNIT,
                        (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCD' AND DATA_NAME='STAT' AND DATA_VALUE=A.STAT) STAT_N,
                        NVL(( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO IN (
                            SELECT WH_NO  FROM MI_WHMAST  WHERE 1=1 AND WH_KIND = '1' AND WH_GRADE = '1'  AND ROWNUM=1 ) ),0) AS A_INV_QTY ,
                        NVL(( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO IN (
                            SELECT WH_NO  FROM MI_WHMAST  WHERE 1=1 AND WH_KIND = '1' AND WH_GRADE = '5'  AND ROWNUM=1 ) ),0) AS B_INV_QTY ,
                         nvl( (select trunc(sum(x.appqty * Y.MIL_PRICE) )
                                 FROM ME_DOCD X, MI_WHCOST Y 
                                WHERE X.DOCNO=A.DOCNO AND X.MMCODE=Y.MMCODE AND Y.DATA_YM=CUR_SETYM AND X.STAT ='2'), 0) as sum_ex,
                        nvl( (select trunc(sum(x.appqty * Y.MIL_PRICE) )
                                FROM ME_DOCD X, MI_WHCOST Y 
                               WHERE X.DOCNO=A.DOCNO AND X.MMCODE=Y.MMCODE AND Y.DATA_YM=CUR_SETYM AND X.STAT ='1'), 0) as sum_in,
                        D.MIL_PRICE as MIL_PRICE,
                        D.MIL_PRICE as TOT_PRICE,
                            d.uprice as uprice, d.mil_price as mil_price_temp
                        FROM ME_DOCD A,MI_MAST B, ME_DOCM C, MI_WHCOST D
                        WHERE C.DOCNO=A.DOCNO AND A.MMCODE = B.MMCODE 
                          AND D.MMCODE=A.MMCODE  AND D.DATA_YM=CUR_SETYM 
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
                        DOCNO, SEQ, MMCODE , APPQTY , STAT,
                        CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                      VALUES (
                        :DOCNO, :SEQ, :MMCODE , :APPQTY ,  :STAT, 
                        SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, ME_DOCD, DBWork.Transaction);
        }

        public int UpdateD(ME_DOCD ME_DOCD)
        {
            var sql = @"UPDATE ME_DOCD SET 
                        STAT = :STAT, MMCODE = :MMCODE, APPQTY = :APPQTY,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO AND SEQ = :SEQ ";
            return DBWork.Connection.Execute(sql, ME_DOCD, DBWork.Transaction);
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
            var sql = @" DELETE from ME_DOCD WHERE DOCNO=:DOCNO AND SEQ=:SEQ";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno, SEQ = seq }, DBWork.Transaction);
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
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO AND APPQTY = 0 ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        public bool CheckExistsMM(string id, string mm)
        {
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO AND MMCODE=:MMCODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id, MMCODE = mm }, DBWork.Transaction) == null);
        }
        public bool CheckBalance(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCD A WHERE  
                            NVL((SELECT TRUNC(SUM(X.APVQTY * Y.MIL_PRICE),2) FROM ME_DOCD X, MI_WHCOST Y WHERE X.DOCNO=A.DOCNO AND X.MMCODE=Y.MMCODE AND Y.DATA_YM=CUR_SETYM AND X.STAT ='2'),0) <> 
                            NVL((SELECT TRUNC(SUM(X.APVQTY * Y.MIL_PRICE),2) FROM ME_DOCD X, MI_WHCOST Y WHERE X.DOCNO=A.DOCNO AND X.MMCODE=Y.MMCODE AND Y.DATA_YM=CUR_SETYM AND X.STAT ='1'),0) 
                             AND A.DOCNO=:DOCNO  ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
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
                        WHERE 1=1 AND WH_KIND = '1' AND WH_GRADE = '5' AND ROWNUM=1";
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
        public string GetTaskid(string id)
        {
            string sql = @"SELECT TASK_ID FROM MI_WHID WHERE WH_USERID=:WH_USERID 
                            AND WH_NO IN (SELECT WH_NO FROM MI_WHMAST WHERE WH_KIND='1' AND WH_GRADE='1')";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { WH_USERID = id }, DBWork.Transaction).ToString();
            return rtn;
        }
        public string GetDocno()
        {
            var p = new OracleDynamicParameters();
            p.Add("O_DOCNO", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 12);

            DBWork.Connection.Query("GET_DOCNO", p, commandType: CommandType.StoredProcedure);
            return p.Get<OracleString>("O_DOCNO").Value;
        }
        public IEnumerable<COMBO_MODEL> GetFlowidCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='FLOWID_XR1' 
                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetDocnoCombo(string id, string task)
        {
            string sql = @"SELECT DISTINCT A.DOCNO as VALUE, A.DOCNO as TEXT,
                        A.DOCNO as COMBITEM
                        FROM ME_DOCM A ,MI_MATCLASS B 
                        WHERE A.MAT_CLASS=B.MAT_CLASS
                        AND A.DOCTYPE = 'XR1' 
                        AND A.APPDEPT=:APPDEPT 
                        AND B.MAT_CLSID = :TASK
                        ORDER BY A.DOCNO";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { APPDEPT = id, TASK = task });
        }
        public IEnumerable<MI_MAST> GetMmCodeCombo(string p0, string p1,int page_index, int page_size, string sorters)
        {

            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT,
                        NVL(( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO IN (
                            SELECT WH_NO  FROM MI_WHMAST  WHERE 1=1 AND WH_KIND = '1' AND WH_GRADE = '1'  AND ROWNUM=1 ) ),0) AS A_INV_QTY ,
                        NVL(( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO IN (
                            SELECT WH_NO  FROM MI_WHMAST  WHERE 1=1 AND WH_KIND = '1' AND WH_GRADE = '5'  AND ROWNUM=1 ) ),0) AS B_INV_QTY,
                        NVL(B.MIL_PRICE,0) MIL_PRICE,
                        NVL(B.UPRICE,0) UPRICE
                        FROM MI_MAST A ,MI_WHCOST B 
                        WHERE A.MMCODE=B.MMCODE AND B.DATA_YM=CUR_SETYM 
                          AND A.MAT_CLASS = :MAT_CLASS
                          ";

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
            p.Add(":MAT_CLASS", p1);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<MI_MAST> GetMmcode(MI_MAST_QUERY_PARAMS query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT,
                        NVL(( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO IN (
                            SELECT WH_NO  FROM MI_WHMAST  WHERE 1=1 AND WH_KIND = '1' AND WH_GRADE = '1'  AND ROWNUM=1 ) ),0) AS A_INV_QTY ,
                        NVL(( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO IN (
                            SELECT WH_NO  FROM MI_WHMAST  WHERE 1=1 AND WH_KIND = '1' AND WH_GRADE = '5'  AND ROWNUM=1 ) ),0) AS B_INV_QTY 
                        FROM MI_MAST A WHERE 1=1 AND A.MAT_CLASS = :MAT_CLASS  ";

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

        public IEnumerable<COMBO_MODEL> GetMatclassCombo()
        {
            string sql = @"SELECT MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, 
                        MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM 
                        FROM MI_MATCLASS WHERE MAT_CLSID='3'   
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
        public IEnumerable<COMBO_MODEL> GetStatCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='ME_DOCD' AND DATA_NAME='STAT' 
                          and data_value in ('1', '2')
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

        public bool CheckMmcodeValid(string mmcode, string mat_class) {
            string sql = @"select 1 
                             from MI_MAST
                            where mmcode = :mmcode
                              and mat_class = :mat_class ";
            return (DBWork.Connection.ExecuteScalar(sql, new { mat_class = mat_class, mmcode = mmcode }, DBWork.Transaction) == null);
        }
        public bool CheckMmcodeExists(string mmcode)
        {
            string sql = @"select 1 
                             from MI_MAST
                            where mmcode = :mmcode";
            return (DBWork.Connection.ExecuteScalar(sql, new {mmcode = mmcode }, DBWork.Transaction) == null);
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
