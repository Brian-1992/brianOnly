using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.AB
{
    public class AB0004Repository : JCLib.Mvc.BaseRepository
    {
        public AB0004Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ME_DOCM> GetAllM(string DOCNO, string APPTIME1, string APPTIME2, string[] str_FLOWID, string APPLY_KIND, string INID, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.* ,
                        (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='FLOWID_MR1' AND DATA_VALUE=A.FLOWID) FLOWID_N,
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
                            AND (SELECT DATA_VALUE FROM PARAM_D WHERE DATA_NAME = 'MR4_EDAY')),'N') MR4,
                        (SELECT EXT FROM UR_ID WHERE TUSER=A.APPID) EXT
                        FROM ME_DOCM A WHERE 1=1 AND DOCTYPE = 'MR2' ";
            if (INID != "")
            {
                //20200401 MARK AND MODIFY INID傳入LOGIN USER
                //sql += " AND A.APPDEPT = :inid ";
                //p.Add(":inid", string.Format("{0}", INID));
                sql += @"AND EXISTS (select 1 from MI_WHMAST C
                            WHERE WH_KIND='1'
                            AND EXISTS(SELECT 'X' FROM UR_ID B WHERE ( C.SUPPLY_INID=B.INID OR C.INID=B.INID ) AND B.TUSER=:TUSER)
                            AND NOT EXISTS(SELECT 'X' FROM MI_WHID B WHERE TASK_ID IN ('2','3') AND B.WH_USERID=:TUSER)
                            AND C.WH_NO = A.TOWH
                            UNION ALL 
                            SELECT 1 FROM MI_WHMAST C,MI_WHID B
                            WHERE C.WH_NO=B.WH_NO AND B.TASK_ID IN ('2','3') AND B.WH_USERID=:TUSER
                            AND  C.WH_NO = A.TOWH )";
                p.Add(":TUSER", string.Format("{0}", INID));
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
            if (APPLY_KIND != "")
            {
                sql += " AND A.APPLY_KIND = :p3 ";
                p.Add(":p3", string.Format("{0}", APPLY_KIND));
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCM>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<ME_DOCD> GetAllD(string DOCNO, string MMCODE, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.DOCNO, A.SEQ, A.MMCODE , A.APPQTY , A.APLYITEM_NOTE, 
                        A.GTAPL_RESON,A.STAT,A.EXPT_DISTQTY,A.APVQTY,A.ACKQTY, TWN_TIME(a.apl_contime) as apl_contime,
                        A.BW_MQTY,A.BW_SQTY,A.APVTIME,A.ONWAY_QTY,
                        B.MMNAME_C, B.MMNAME_E, B.BASE_UNIT,B.DISC_UPRICE M_CONTPRICE,
                        (case when A.AVG_PRICE is null then NVL(( SELECT AVG_PRICE FROM MI_WHCOST WHERE MMCODE = A.MMCODE AND DATA_YM=CUR_SETYM AND ROWNUM=1 ),0) else A.AVG_PRICE end) AS AVG_PRICE,
                        NVL(( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO=C.FRWH ),0) AS INV_QTY,
                        NVL(( SELECT SAFE_QTY FROM MI_WINVCTL WHERE MMCODE = A.MMCODE AND WH_NO=C.TOWH ),0) AS SAFE_QTY,
                        NVL(( SELECT HIGH_QTY FROM MI_WINVCTL WHERE MMCODE = A.MMCODE AND WH_NO=C.TOWH ),0) AS HIGH_QTY,
                        NVL(E.TOT_APVQTY,0) TOT_APVQTY,
                        NVL( ( SELECT MIN_ORDQTY FROM MI_WINVCTL WHERE MMCODE = A.MMCODE AND WH_NO ='560000' AND ROWNUM=1 ) ,1) AS TOT_DISTUN,
                        B.PFILE_ID 
                        , A.CREATE_USER, USER_NAME(A.CREATE_USER) as create_user_name
                        FROM ME_DOCD A 
                        INNER JOIN MI_MAST B ON B.MMCODE=A.MMCODE 
                        INNER JOIN ME_DOCM C ON C.DOCNO=A.DOCNO
                        LEFT OUTER JOIN V_MM_TOTAPL2 E ON E.DATE_YM=SUBSTR(TWN_DATE(C.APPTIME),0,5) and E.MMCODE=A.MMCODE and E.TOWH=C.TOWH 
                        WHERE 1=1 ";

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
                        (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='FLOWID_MR1' AND DATA_VALUE=A.FLOWID) FLOWID_N,
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
                            AND (SELECT DATA_VALUE FROM PARAM_D WHERE DATA_NAME = 'MR4_EDAY')),'N') MR4,
                        (SELECT EXT FROM UR_ID WHERE TUSER=A.APPID) EXT
                        FROM ME_DOCM A
                        WHERE A.DOCNO = :DOCNO";
            return DBWork.Connection.Query<ME_DOCM>(sql, new { DOCNO = id }, DBWork.Transaction);
        }
        public IEnumerable<ME_DOCD> GetD(string id,string seq)
        {
            var sql = @"SELECT A.DOCNO, A.SEQ, A.MMCODE , A.APPQTY , A.APLYITEM_NOTE, 
                        A.GTAPL_RESON,A.STAT,A.EXPT_DISTQTY,A.APVQTY,A.ACKQTY,
                        A.BW_MQTY,A.BW_SQTY,A.APVTIME,A.ONWAY_QTY,
                        B.MMNAME_C, B.MMNAME_E, B.BASE_UNIT,B.DISC_UPRICE M_CONTPRICE,
                        (case when A.AVG_PRICE is null then NVL(( SELECT AVG_PRICE FROM MI_WHCOST WHERE MMCODE = A.MMCODE AND DATA_YM=CUR_SETYM AND ROWNUM=1 ),0) else A.AVG_PRICE end) AS AVG_PRICE,
                        NVL(( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO=C.FRWH ),0) AS INV_QTY,
                        NVL(( SELECT SAFE_QTY FROM MI_WINVCTL WHERE MMCODE = A.MMCODE AND WH_NO=C.TOWH ),0) AS SAFE_QTY,
                        NVL(( SELECT HIGH_QTY FROM MI_WINVCTL WHERE MMCODE = A.MMCODE AND WH_NO=C.TOWH ),0) AS HIGH_QTY,
                        NVL(E.TOT_APVQTY,0) TOT_APVQTY,
                        NVL( ( SELECT MIN_ORDQTY FROM MI_WINVCTL WHERE MMCODE = A.MMCODE AND WH_NO ='560000' AND ROWNUM=1 ) ,1) AS TOT_DISTUN,
                        B.PFILE_ID   
                        FROM ME_DOCD A 
                        INNER JOIN MI_MAST B ON B.MMCODE=A.MMCODE 
                        INNER JOIN ME_DOCM C ON C.DOCNO=A.DOCNO
                        LEFT OUTER JOIN V_MM_TOTAPL2 E ON E.DATE_YM=SUBSTR(TWN_DATE(C.APPTIME),0,5) and E.MMCODE=A.MMCODE and E.TOWH=C.TOWH 
                        WHERE 1=1
                        AND A.DOCNO = :DOCNO AND A.SEQ= :SEQ";
            return DBWork.Connection.Query<ME_DOCD>(sql, new { DOCNO = id, SEQ = seq }, DBWork.Transaction);
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
                        DOCNO, SEQ, MMCODE, APPQTY, AVG_PRICE, APLYITEM_NOTE, GTAPL_RESON,
                        CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                      VALUES (
                        :DOCNO, :SEQ, :MMCODE, :APPQTY, :AVG_PRICE, :APLYITEM_NOTE, :GTAPL_RESON,
                        SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
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
                        MMCODE = :MMCODE, APPQTY = :APPQTY, AVG_PRICE = :AVG_PRICE, APLYITEM_NOTE = :APLYITEM_NOTE, GTAPL_RESON = :GTAPL_RESON,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO AND SEQ = :SEQ ";
            return DBWork.Connection.Execute(sql, ME_DOCD, DBWork.Transaction);
        }
        public int ApplyM(ME_DOCM ME_DOCM)
        {
            var sql = @"UPDATE ME_DOCM SET FLOWID = '2',
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP,
                        sendapvid = :UPDATE_USER, sendapvtime = sysdate,
                        sendapvdept = (SELECT INID FROM UR_ID WHERE TUSER=:UPDATE_USER)
                      WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);

        }
        public int ApplyD(ME_DOCM ME_DOCM)
        {
            var sql = @"UPDATE ME_DOCD SET EXPT_DISTQTY = GET_DISTQTY(:TOWH,MMCODE,APPQTY),
                        APL_CONTIME = SYSDATE,UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
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
        public int SavepkM(string docno, string note, string newdocno)
        {
            var sql = @"INSERT INTO MM_PACK_M (
                        DOCNO, DOCTYPE, FLOWID , APPID , APPDEPT , 
                        APPLY_NOTE ,MAT_CLASS,
                        CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                       SELECT 
                        :NEWDOC, 'MR2', FLOWID , APPID , TOWH , 
                        :NOTE ,MAT_CLASS,
                        SYSDATE, CREATE_USER, SYSDATE, UPDATE_USER, UPDATE_IP 
                        FROM ME_DOCM WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno, NOTE = note, NEWDOC = newdocno }, DBWork.Transaction);
        }

        public int SavepkD(string docno, string newdocno)
        {
            var sql = @"INSERT INTO MM_PACK_D (
                        DOCNO, SEQ, MMCODE , APPQTY , APLYITEM_NOTE,
                        CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                      SELECT 
                        :NEWDOC, SEQ, MMCODE , APPQTY , APLYITEM_NOTE, 
                        CREATE_USER, SYSDATE, UPDATE_USER, UPDATE_IP 
                        FROM ME_DOCD WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno, NEWDOC = newdocno }, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCD> GetPackD(string DOCNO, string DOCNO2, string MAT_CLASS, string APPDEPT, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.MMCODE ,A.APPQTY,
                               B.MMNAME_C, B.MMNAME_E, B.BASE_UNIT,B.DISC_UPRICE M_CONTPRICE,
                               ( SELECT AVG_PRICE FROM MI_WHCOST WHERE MMCODE = A.MMCODE AND DATA_YM=CUR_SETYM AND ROWNUM=1 ) AS AVG_PRICE ,
                               NVL( ( SELECT MIN_ORDQTY FROM MI_WINVCTL WHERE MMCODE = A.MMCODE AND WH_NO ='560000' AND ROWNUM=1 ) ,1) AS TOT_DISTUN,
                               (case
                                   when ((select 1 from dual where not exists (select 1 from MI_WHMM where mmcode = a.mmcode)
                                             union
                                          select 1 from MI_WHMM where wh_no = c.appdept and mmcode = a.mmcode
                                         )) = '1' 
                                   then 'Y' else 'N'
                                end) as whmm_valid
                          FROM MM_PACK_D A,MI_MAST B, MM_PACK_M C 
                         WHERE C.DOCNO=A.DOCNO AND A.MMCODE = B.MMCODE 
                           and b.m_applyid <> 'E'
                           and b.m_storeid = '1'";

            if (DOCNO == "" && DOCNO2 == "")
            {
                sql += " AND 1=2 ";
            }
            if (DOCNO != "")
            {
                sql += " AND A.DOCNO LIKE :p0 ";
            }
            if (DOCNO2 != "")
            {
                sql += " AND A.DOCNO LIKE :p1 ";
            }
            if (MAT_CLASS != "")
            {
                sql += " AND C.MAT_CLASS = :p2 ";
            }
            if (APPDEPT != "")
            {
                sql += " AND C.APPDEPT = :p3 ";
            }

            p.Add(":p0", string.Format("%{0}%", DOCNO));
            p.Add(":p1", string.Format("%{0}%", DOCNO2));
            p.Add(":p2", string.Format("{0}", MAT_CLASS));
            p.Add(":p3", string.Format("{0}", APPDEPT));
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCD>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCD> GetSaveD(string MAT_CLASS, string APPDEPT, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT P.*,
                        (CASE WHEN P.APPQTY > P.HIGH_QTY THEN '1' ELSE '' END) GTAPL_RESON,
                        (CASE WHEN P.APPQTY > P.HIGH_QTY THEN (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCD' AND DATA_NAME='GTAPL_REASON' AND DATA_VALUE='1') ELSE '' END) GTAPL_RESON_N 
                        FROM (
                        SELECT A.MMCODE, 
                        CEIL((B.SAFE_QTY+B.OPER_QTY+B.SHIP_QTY-A.INV_QTY)/NVL( ( SELECT MIN_ORDQTY FROM MI_WINVCTL WHERE MMCODE = A.MMCODE AND WH_NO ='560000' AND ROWNUM=1 ) ,1))*NVL( ( SELECT MIN_ORDQTY FROM MI_WINVCTL WHERE MMCODE = A.MMCODE AND WH_NO ='560000' AND ROWNUM=1 ) ,1) APPQTY,
                        C.MMNAME_C, C.MMNAME_E, C.BASE_UNIT,C.M_CONTPRICE,
                        ( SELECT AVG_PRICE FROM MI_WHCOST WHERE MMCODE = A.MMCODE AND DATA_YM=CUR_SETYM AND ROWNUM=1 ) AS AVG_PRICE,
                        B.HIGH_QTY ,
                        (case
                                    when ((select 1 from dual where not exists (select 1 from MI_WHMM where mmcode = a.mmcode)
                                              union
                                           select 1 from MI_WHMM where wh_no = a.wh_no and mmcode = a.mmcode
                                          )) = '1' 
                                    then 'Y' else 'N'
                                 end) as whmm_valid
                        FROM MI_WHINV A,MI_WINVCTL B, MI_MAST C
                        WHERE A.WH_NO=B.WH_NO AND A.MMCODE=B.MMCODE AND A.MMCODE=C.MMCODE
                        AND A.INV_QTY<(B.SAFE_QTY+B.OPER_QTY+B.SHIP_QTY) 
                        AND C.M_STOREID = '1'
                        AND C.M_CONTID <> '3'
                        AND C.M_APPLYID <> 'E'";

            if (MAT_CLASS != "")
            {
                sql += " AND C.MAT_CLASS = :p2 ";

            }
            if (APPDEPT != "")
            {
                sql += " AND A.WH_NO = :p3 ";
            }

            sql += @" ) P WHERE 1=1";

            p.Add(":p2", string.Format("{0}", MAT_CLASS));
            p.Add(":p3", string.Format("{0}", APPDEPT));
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCD>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
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
        public bool CheckExists(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCM WHERE DOCNO=:DOCNO";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        public bool CheckExistsM(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCM WHERE DOCNO=:DOCNO AND FLOWID <> '1'";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        public bool CheckExistsM3(string matclass, string appdept, string doctype)
        {
            string sql = @"SELECT 1 FROM ME_DOCM WHERE MAT_CLASS=:MAT_CLASS AND TOWH=:APPDEPT AND FLOWID IN ('3','4') AND DOCTYPE=:DOCTYPE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MAT_CLASS = matclass, APPDEPT = appdept, DOCTYPE = doctype }, DBWork.Transaction) == null);
        }
        public bool CheckExistsM1(string matclass, string appdept, string doctype)
        {
            string sql = @"SELECT 1 FROM ME_DOCM WHERE MAT_CLASS=:MAT_CLASS AND TOWH=:APPDEPT AND FLOWID IN ('1','2') AND DOCTYPE=:DOCTYPE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MAT_CLASS = matclass, APPDEPT = appdept, DOCTYPE = doctype }, DBWork.Transaction) == null);
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

        public bool CheckAppid(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCM WHERE DOCNO=:DOCNO AND APPID IS NULL ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }

        public int UpdateAppid(string id, string ip, string no)
        {
            var sql = @"UPDATE ME_DOCM SET 
                        APPID = :APPID, 
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :APPID, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, new { APPID = id, UPDATE_IP = ip, DOCNO = no }, DBWork.Transaction);
        }
        public int CheckApplyKind(string id, string inid)
        {
            string sql = @"SELECT COUNT(*) AS CNT FROM ME_DOCM 
                            WHERE DOCTYPE='MR2' AND APPLY_KIND='1' 
                            AND APPTIME BETWEEN NEXT_DAY(SYSDATE-7,1) AND NEXT_DAY(SYSDATE-7,1)+6 
                            AND MAT_CLASS = :MAT_CLASS AND TOWH = :INID ";
            int rtn = Convert.ToInt32(DBWork.Connection.ExecuteScalar(sql, new { MAT_CLASS = id, INID = inid }, DBWork.Transaction).ToString());
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

        public IEnumerable<COMBO_MODEL> GetDocnopkCombo(string p0, string p1)
        {
            string sql = @"SELECT DISTINCT DOCNO as VALUE, DOCNO as TEXT,
                        DOCNO as COMBITEM,CREATE_TIME EXTRA1
                        FROM MM_PACK_M 
                        WHERE 1=1 AND DOCTYPE = 'MR2' 
                        AND MAT_CLASS=:MAT_CLASS
                        AND APPDEPT=:APPDEPT
                        ORDER BY EXTRA1 DESC ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { MAT_CLASS = p0, APPDEPT = p1 });
        }
        public IEnumerable<COMBO_MODEL> GetDocpknoteCombo(string p0, string p1)
        {
            string sql = @"SELECT DISTINCT DOCNO as VALUE, APPLY_NOTE as TEXT,
                        APPLY_NOTE as COMBITEM,CREATE_TIME EXTRA1
                        FROM MM_PACK_M 
                        WHERE 1=1 AND DOCTYPE = 'MR2' 
                        AND MAT_CLASS=:MAT_CLASS
                        AND APPDEPT=:APPDEPT
                        ORDER BY EXTRA1 DESC ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { MAT_CLASS = p0, APPDEPT = p1 });
        }
        public IEnumerable<COMBO_MODEL> GetDocnoCombo(string id)
        {
            string sql = @"SELECT DISTINCT DOCNO as VALUE, DOCNO as TEXT,
                        DOCNO as COMBITEM,APPTIME EXTRA1
                        FROM ME_DOCM 
                        WHERE 1=1 AND DOCTYPE = 'MR2' 
                        AND APPDEPT=:APPDEPT
                        ORDER BY EXTRA1 DESC";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { APPDEPT = id });
        }
        public IEnumerable<COMBO_MODEL> GetFlowidCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='FLOWID_MR1' 
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
        public IEnumerable<MI_MAST> GetMmCodeCombo(string p0, string p1, string p2, string docno,  string p4,int page_index, int page_size, string sorters)
        {

            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT, A.DISC_UPRICE as M_CONTPRICE,
                               NVL(( SELECT AVG_PRICE FROM MI_WHCOST WHERE MMCODE = A.MMCODE AND DATA_YM=CUR_SETYM AND ROWNUM=1 ),0) AS AVG_PRICE,
                               NVL(( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO=:FRWH ),0) AS INV_QTY,
                               NVL(( SELECT AVG_APLQTY FROM V_MM_AVGAPL WHERE MMCODE = A.MMCODE AND WH_NO=:TOWH ),0) AS AVG_APLQTY ,
                               NVL(( SELECT HIGH_QTY FROM MI_WINVCTL WHERE MMCODE = A.MMCODE AND WH_NO=:TOWH ),0) AS HIGH_QTY,
                               NVL(E.TOT_APVQTY,0) TOT_APVQTY ,
                               NVL( ( SELECT MIN_ORDQTY FROM MI_WINVCTL WHERE MMCODE = A.MMCODE AND WH_NO ='560000' AND ROWNUM=1 ) ,1) AS TOT_DISTUN,
                               A.PFILE_ID,
                               (case
                                    when ((select 1 from dual where not exists (select 1 from MI_WHMM where mmcode = a.mmcode)
                                              union
                                           select 1 from MI_WHMM where wh_no = c.towh and mmcode = a.mmcode
                                          )) = '1' 
                                    then 'Y' else 'N'
                                 end) as whmm_valid
                          FROM MI_MAST A 
                         INNER JOIN ME_DOCM C ON C.DOCNO=:DOCNO
                          LEFT OUTER JOIN MI_WINVCTL D ON D.MMCODE = A.MMCODE AND D.WH_NO IN (SELECT WH_NO FROM MI_WHMAST WHERE WH_KIND='1' AND WH_GRADE='1')
                          LEFT OUTER JOIN V_MM_TOTAPL2 E ON E.DATE_YM=SUBSTR(TWN_DATE(C.APPTIME),0,5) and E.MMCODE=A.MMCODE  and E.TOWH=C.TOWH 
                         WHERE 1=1 
                           AND A.MAT_CLASS = :MAT_CLASS 
                           AND A.M_STOREID = '1'
                           AND A.M_CONTID <> '3'
                           AND A.M_APPLYID <> 'E'
                           {1}";

            if (p0 != "")
            {
                sql = string.Format(sql,
                     "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,",
                     @"   AND (A.MMCODE LIKE :MMCODE OR MMNAME_E LIKE UPPER(:MMNAME_E) OR MMNAME_C LIKE :MMNAME_C)");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);
                p.Add(":MMCODE", string.Format("{0}%", p0));
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "", "");
                sql += " ORDER BY MMCODE ";
            }
            p.Add(":MAT_CLASS", p1);
            p.Add(":TOWH", p2);
            p.Add(":DOCNO", docno);
            p.Add(":FRWH", p4);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<MI_MAST> GetMmcode(MI_MAST_QUERY_PARAMS query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT,A.M_CONTPRICE,
                               ( SELECT AVG_PRICE FROM MI_WHCOST WHERE MMCODE = A.MMCODE AND DATA_YM=CUR_SETYM AND ROWNUM=1 ) AS AVG_PRICE,
                               ( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO=:WH_NO ) AS INV_QTY,
                               ( SELECT AVG_APLQTY FROM V_MM_AVGAPL WHERE MMCODE = A.MMCODE AND WH_NO=:WH_NO ) AS AVG_APLQTY,
                               ( CASE WHEN A.M_STOREID = '1'
                                 AND A.M_CONTID <> '3'
                                 AND A.M_APPLYID <> 'E' THEN 'Y' ELSE 'X' END) AS M_PAYID,
                               (SELECT DATA_VALUE||' '||DATA_DESC SPE 
                               FROM PARAM_D WHERE GRP_CODE='MI_MAST' 
                               AND DATA_NAME='M_STOREID' AND DATA_VALUE=A.M_STOREID) M_STOREID,
                               (SELECT DATA_VALUE||' '||DATA_DESC SPE 
                               FROM PARAM_D WHERE GRP_CODE='MI_MAST' 
                               AND DATA_NAME='M_CONTID' AND DATA_VALUE=A.M_CONTID) M_CONTID,
                               (SELECT DATA_VALUE||' '||DATA_DESC SPE 
                               FROM PARAM_D WHERE GRP_CODE='MI_MAST' 
                               AND DATA_NAME='M_APPLYID' AND DATA_VALUE=A.M_APPLYID) M_APPLYID  ,
                               (case
                                   when ((select 1 from dual where not exists (select 1 from MI_WHMM where mmcode = a.mmcode)
                                             union
                                          select 1 from MI_WHMM where wh_no = :WH_NO and mmcode = a.mmcode
                                         )) = '1' 
                                   then 'Y' else 'N'
                                end) as whmm_valid
                          FROM MI_MAST A 
                         WHERE 1=1 
                           AND A.MAT_CLASS = :MAT_CLASS  ";

            if (query.MMCODE != "")
            {
                sql += " AND A.MMCODE LIKE :MMCODE ";
            }
            if (query.MMNAME_C != "")
            {
                sql += " AND A.MMNAME_C LIKE :MMNAME_C ";
            }
            if (query.MMNAME_E != "")
            {
                sql += " AND A.MMNAME_E LIKE UPPER(:MMNAME_E) ";
            }

            p.Add(":MMNAME_E", string.Format("%{0}%", query.MMNAME_E));
            p.Add(":MMNAME_C", string.Format("%{0}%", query.MMNAME_C));
            p.Add(":MMCODE", string.Format("%{0}%", query.MMCODE));
            p.Add(":MAT_CLASS", query.MAT_CLASS);
            p.Add(":WH_NO", query.WH_NO);

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
            string sql = @"select A.WH_NO VALUE , WH_NO||' '||WH_NAME TEXT ,A.WH_NO || ' ' || A.WH_NAME COMBITEM from MI_WHMAST A
                        WHERE WH_KIND='1' and wh_grade > '1'
                        AND EXISTS(SELECT 'X' FROM UR_ID B WHERE ( A.SUPPLY_INID=B.INID OR A.INID=B.INID ) AND TUSER=:TUSER)
                        AND NOT EXISTS(SELECT 'X' FROM MI_WHID B WHERE TASK_ID IN ('2','3') AND WH_USERID=:TUSER)
                        and a.cancel_id = 'N'
                        UNION ALL 
                        SELECT A.WH_NO ,A.WH_NO||' '||WH_NAME,A.WH_NO || ' ' || A.WH_NAME COMBITEM FROM MI_WHMAST A,MI_WHID B
                        WHERE A.WH_NO=B.WH_NO AND TASK_ID IN ('2','3') AND WH_USERID=:TUSER
                          and a.cancel_id = 'N'
                          and a.wh_grade > '1'
                        ORDER BY 1";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { TUSER = id });
        }
        public IEnumerable<COMBO_MODEL> GetMatclassCombo()
        {
            string sql = @"SELECT MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, 
                        MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM 
                        FROM MI_MATCLASS WHERE MAT_CLSID='2'  
                        ORDER BY MAT_CLASS";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetReasonCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='ME_DOCD' AND DATA_NAME='GTAPL_REASON' 
                        ORDER BY DATA_VALUE";

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
        public DataTable GetExcel()
        {
            var p = new DynamicParameters();

            var sql = @" SELECT '' 院內碼,'' 申請數量 FROM DUAL ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
        public bool CheckExistsMMCODE(string id)
        {
            string sql = @" SELECT 1 FROM MI_MAST WHERE 1=1 
                          AND MMCODE = :MMCODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = id }, DBWork.Transaction) == null);
        }

        public bool CheckExistsMMCODE2(string id)
        {
            string sql = @" SELECT 1 FROM MI_MAST WHERE 1=1 
                          AND M_STOREID = '1'
                          AND M_CONTID <> '3'
                          AND M_APPLYID <> 'E' 
                          AND MMCODE = :MMCODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = id }, DBWork.Transaction) == null);
        }
        public bool CheckMatClassMMCODE(string id1, string id2)
        {
            string sql = @" SELECT 1 FROM MI_MAST WHERE 1=1 
                          AND MAT_CLASS = :MAT_CLASS 
                          AND MMCODE = :MMCODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MAT_CLASS = id1, MMCODE = id2 }, DBWork.Transaction) == null);
        }

        public bool Checkappqty(string id,string qty)
        {
            string sql = @" SELECT 1 FROM MI_MAST A 
                            WHERE 1=1 AND A.MMCODE=:MMCODE 
                            AND mod(:APPQTY , NVL( ( SELECT MIN_ORDQTY FROM MI_WINVCTL WHERE MMCODE = A.MMCODE AND WH_NO ='560000' AND ROWNUM=1 ) ,1)) > 0";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = id,APPQTY = qty}, DBWork.Transaction) == null);
        }
        public bool CheckWHMM(string mmcode, string wh_no)
        {
            string sql = @"select 1 from dual
                            where not exists (select 1 from MI_WHMM where mmcode = :mmcode)
                           union 
                           select 1 from MI_WHMM
                            where mmcode = :mmcode and wh_no = :wh_no";
            return !(DBWork.Connection.ExecuteScalar(sql, new { mmcode = mmcode, wh_no = wh_no }, DBWork.Transaction) == null);
        }
        public string GetTowh(string docno)
        {
            string sql = @"select towh from ME_DOCM where docno = :docno";
            return DBWork.Connection.QueryFirst<string>(sql, new { docno = docno }, DBWork.Transaction);
        }

        #region 衛材庫備常態申請一天只能一次 2020-05-06
        public int CheckApplyKindDaily(string id, string inid)
        {
            string sql = @"SELECT COUNT(*) AS CNT FROM ME_DOCM 
                            WHERE DOCTYPE='MR2' AND APPLY_KIND='1' 
                            AND TWN_DATE(APPTIME) = TWN_DATE(sysdate)
                            AND MAT_CLASS = :MAT_CLASS AND TOWH = :INID ";
            int rtn = Convert.ToInt32(DBWork.Connection.ExecuteScalar(sql, new { MAT_CLASS = id, INID = inid }, DBWork.Transaction).ToString());
            return rtn;
        }
        #endregion


        #region 20201-05-06新增: 刪除時檢查flowId
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


        public IEnumerable<ME_DOCD> CheckMstoreid(string docno, string m_storeid)
        {
            string sql = @"
                select * from ME_DOCD a
                 where docno = :docno
                   and exists (select 1 from MI_MAST 
                                where mmcode = a.mmcode
                                  and (m_storeid <> :m_storeid or m_storeid is null))
            ";
            return DBWork.Connection.Query<ME_DOCD>(sql, new { docno, m_storeid }, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCD> CheckMinOrderQty(string docno)
        {
            string sql = @"
                with min_ordqtys as (
                    select a.mmcode, 
                           nvl( ( select min_ordqty from MI_WINVCTL where mmcode = a.mmcode and wh_no =WHNO_MM1 and rownum=1 ) ,1) as min_ordqty
                     from ME_DOCD a
                    where a.docno = :docno
                )
                select a.*,
                       (case 
                          when mod(nvl(a.appqty, 0), b.min_ordqty) = 0
                          then 'Y' else 'N'
                         end) as is_appqty_valid 
                  from ME_DOCD a, min_ordqtys b
                 where a.docno = :docno
                   and a.mmcode = b.mmcode
            ";

            return DBWork.Connection.Query<ME_DOCD>(sql, new { docno }, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCD> CheckMmcodeCancel(string docno)
        {
            string sql = @"
                select * from ME_DOCD a
                 where docno = :docno
                   and exists (select 1 from MI_MAST
                                where mmcode = a.mmcode
                                  and nvl(cancel_id, 'N') = 'Y')
            ";
            return DBWork.Connection.Query<ME_DOCD>(sql, new { docno }, DBWork.Transaction);
        }
    }
}
