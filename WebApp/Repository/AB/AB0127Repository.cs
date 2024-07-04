using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System;
using System.Data.SqlClient;
using System.Text;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace WebApp.Repository.AB
{
    public class AB0127Repository : JCLib.Mvc.BaseRepository
    {
        public AB0127Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ME_DOCM> GetAllM(string DOCNO, string APPTIME1, string APPTIME2, string[] str_FLOWID, string fromDGMISS, string tuser, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.DOCNO, A.DOCTYPE, A.FLOWID, A.APPID, A.TOWH, A.APPDEPT, A.APPLY_NOTE,
                        (SELECT FLOWNAME FROM ME_FLOW WHERE DOCTYPE in ('MS', 'MR') and FLOWID=A.FLOWID and DOCTYPE=A.DOCTYPE) FLOWID_N,                       
                        (SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.TOWH) TOWH_N,
                        (SELECT UNA FROM UR_ID WHERE TUSER=A.APPID) APP_NAME,
                        (SELECT INID_NAME FROM UR_INID WHERE INID=A.APPDEPT) APPDEPT_NAME,
                        TWN_DATE(A.APPTIME) APPTIME_T ,
                        (SELECT EXT FROM UR_ID WHERE TUSER=A.APPID) EXT, A.ISARMY, A.APPUNA,
                        (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='ISARMY' AND DATA_VALUE=A.ISARMY) ISARMY_N,A.M_CONTID,
                        (SELECT DISTINCT RTrim(DATA_DESC) FROM PARAM_D WHERE GRP_CODE='MI_MAST' and DATA_NAME='M_CONTID' AND DATA_VALUE = A.M_CONTID) as M_CONTID_T
                        FROM ME_DOCM A 
                        WHERE 1=1 
                         AND A.DOCTYPE IN ('MS','MR') 
                         AND A.FLOWID in (
                            select FLOWID from ME_FLOW where DOCTYPE = 'MR' 
                            and USER_INID(:TUSER) in (select INID from MI_WHMAST where WH_KIND = '0' and WH_GRADE = '2' and CANCEL_ID = 'N')
                            union
                            select FLOWID from ME_FLOW where DOCTYPE = 'MR'
                            and (select count(*) from MI_WHID A,MI_WHMAST B WHERE A.WH_NO = B.WH_NO AND A.WH_USERID=:TUSER AND B.WH_KIND = '0' and WH_GRADE = '2' and CANCEL_ID = 'N') > 0
                            union
                            select FLOWID from ME_FLOW where DOCTYPE = 'MS' 
                            and USER_INID(:TUSER) not in (select INID from MI_WHMAST where WH_KIND = '0' and WH_GRADE = '2' and CANCEL_ID = 'N')
                            and (select count(*) from MI_WHID A,MI_WHMAST B WHERE A.WH_NO = B.WH_NO AND A.WH_USERID=:TUSER AND B.WH_KIND = '0' and WH_GRADE <> '2' and CANCEL_ID = 'N') > 0
                        ) ";
            if (tuser != "")
            {
                sql += @" AND A.TOWH IN ( SELECT WH_NO FROM MI_WHID B WHERE B.WH_USERID=:TUSER 
                                          UNION
                                          SELECT WH_NO FROM MI_WHMAST C WHERE C.INID = USER_INID(:TUSER)
                                        ) ";
                p.Add(":TUSER", string.Format("{0}", tuser));
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

            if (str_FLOWID.Length > 0)
            {
                sql += @" and A.FLOWID in :p2 ";
                p.Add(":p2", str_FLOWID);
            }
            if (fromDGMISS == "true")
            {
                sql += " AND (select count(*) from DGMISS where RDOCNO=A.DOCNO)>0 ";
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCM>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCD> GetAllD(string DOCNO, string MMCODE, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT P.*,
                        NVL(P.APPQTY * P.DISC_UPRICE,0) AS APP_AMT
                        FROM (
                        SELECT A.DOCNO, A.SEQ, A.MMCODE , A.APPQTY , A.APLYITEM_NOTE, 
                        A.GTAPL_RESON, SUM(A.ACKQTY) AS ACKQTY, C.SENDAPVTIME, MAX(A.APVTIME) AS APVTIME, B.DISC_UPRICE,
                        B.MMNAME_C, B.MMNAME_E, B.BASE_UNIT, B.M_CONTPRICE, SUM(NVL(A.APVQTY,0)) APVQTY, A.FRWH_D,
                        twn_date(A.APL_CONTIME) as APL_CONTIME, A.CREATE_USER, USER_NAME(A.CREATE_USER) AS CREATE_USER_NAME, 
                        A.S_INV_QTY,A.INV_QTY, A.OPER_QTY as HIGH_QTY, A.SAFE_QTY, A.M_CONTID,
                        (SELECT DISTINCT RTrim(DATA_DESC) FROM PARAM_D WHERE GRP_CODE='MI_MAST' and DATA_NAME='M_CONTID' AND DATA_VALUE = A.M_CONTID) as M_CONTID_T,
                        B.APPQTY_TIMES, B.UNITRATE,
                        (case when A.POSTID IS NULL then '待核可' 
                        else (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D 
                                 where GRP_CODE = 'ME_DOCD' and DATA_NAME = 'POSTID' and DATA_VALUE = A.POSTID) end) as POSTID
                        FROM ME_DOCD A 
                        INNER JOIN MI_MAST B ON B.MMCODE=A.MMCODE 
                        INNER JOIN ME_DOCM C ON C.DOCNO=A.SRCDOCNO 
                        WHERE 1=1 ";

            if (DOCNO != "")
            {
                // sql += " AND A.SRCDOCNO LIKE :p0 ";
                sql += " AND A.DOCNO = :p0 ";
                p.Add(":p0", string.Format("{0}", DOCNO));
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
            sql += @" group by  A.DOCNO, A.SEQ, A.MMCODE, A.APPQTY, A.APLYITEM_NOTE,
                                A.GTAPL_RESON, C.SENDAPVTIME, B.DISC_UPRICE, A.FRWH_D, 
                                B.MMNAME_C, B.MMNAME_E, B.BASE_UNIT,B.M_CONTPRICE, A.APL_CONTIME, 
                                A.CREATE_USER, A.S_INV_QTY, A.INV_QTY, A.OPER_QTY, A.SAFE_QTY, A.M_CONTID,
                                B.APPQTY_TIMES, B.UNITRATE,POSTID
                 ) P ";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCD>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCM> GetM(string id)
        {
            var sql = @"SELECT A.DOCNO, A.DOCTYPE, A.FLOWID, A.APPID, A.TOWH, A.APPDEPT, A.APPLY_NOTE,
                        (SELECT FLOWNAME FROM ME_FLOW WHERE DOCTYPE in ('MS', 'MR') and FLOWID=A.FLOWID and DOCTYPE=A.DOCTYPE) FLOWID_N,
                        (SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.TOWH) TOWH_N,
                        (SELECT UNA FROM UR_ID WHERE TUSER=A.APPID) APP_NAME,
                        (SELECT INID_NAME FROM UR_INID WHERE INID=A.APPDEPT) APPDEPT_NAME,
                        TWN_DATE(A.APPTIME) APPTIME_T ,
                        (SELECT EXT FROM UR_ID WHERE TUSER=A.APPID) EXT, A.ISARMY, A.APPUNA,
                        (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='ISARMY' AND DATA_VALUE=A.ISARMY) ISARMY_N,
                        A.M_CONTID,(SELECT DISTINCT RTrim(DATA_DESC) FROM PARAM_D WHERE GRP_CODE='MI_MAST' and DATA_NAME='M_CONTID' AND DATA_VALUE = A.M_CONTID) as M_CONTID_T
                        FROM ME_DOCM A 
                        WHERE A.DOCNO = :DOCNO";
            return DBWork.Connection.Query<ME_DOCM>(sql, new { DOCNO = id }, DBWork.Transaction);
        }
        public IEnumerable<ME_DOCD> GetD(string id, string seq)
        {
            var sql = @"SELECT P.*,
                        NVL(P.APPQTY * P.DISC_UPRICE,0) AS APP_AMT
                        FROM (
                        SELECT A.DOCNO, A.SEQ, A.MMCODE , A.APPQTY , A.APLYITEM_NOTE, 
                        A.GTAPL_RESON, SUM(A.ACKQTY) AS ACKQTY, C.SENDAPVTIME, MAX(A.APVTIME) AS APVTIME, B.DISC_UPRICE,
                        B.MMNAME_C, B.MMNAME_E, B.BASE_UNIT, B.M_CONTPRICE, SUM(NVL(A.APVQTY,0)) APVQTY, A.FRWH_D,
                        twn_date(A.APL_CONTIME) as APL_CONTIME, A.CREATE_USER, USER_NAME(A.CREATE_USER) AS CREATE_USER_NAME,
                        A.S_INV_QTY, A.INV_QTY, A.OPER_QTY as HIGH_QTY, A.SAFE_QTY, A.M_CONTID,
                        (SELECT DISTINCT RTrim(DATA_DESC) FROM PARAM_D WHERE GRP_CODE='MI_MAST' and DATA_NAME='M_CONTID' AND DATA_VALUE = A.M_CONTID) as M_CONTID_T,
                        B.APPQTY_TIMES, B.UNITRATE
                        FROM ME_DOCD A 
                        INNER JOIN MI_MAST B ON B.MMCODE=A.MMCODE 
                        INNER JOIN ME_DOCM C ON C.DOCNO=A.SRCDOCNO 
                        WHERE 1=1 
                        AND A.DOCNO = :DOCNO AND A.SEQ= :SEQ 
                        group by  A.DOCNO, A.SEQ, A.MMCODE, A.APPQTY, A.APLYITEM_NOTE,
                                A.GTAPL_RESON, C.SENDAPVTIME,B.DISC_UPRICE,
                                B.MMNAME_C, B.MMNAME_E, B.BASE_UNIT,B.M_CONTPRICE, A.FRWH_D, 
                                A.APL_CONTIME, A.CREATE_USER, A.S_INV_QTY, A.INV_QTY, A.OPER_QTY, A.SAFE_QTY, A.M_CONTID,
                                B.APPQTY_TIMES, B.UNITRATE
                        ) P";
            return DBWork.Connection.Query<ME_DOCD>(sql, new { DOCNO = id, SEQ = seq }, DBWork.Transaction);
        }

        public int CreateM(ME_DOCM ME_DOCM)
        {
            var sql = @"INSERT INTO ME_DOCM (
                        DOCNO, DOCTYPE, FLOWID , APPID , APPDEPT , 
                        APPTIME , USEID , USEDEPT , FRWH , TOWH , 
                        APPLY_KIND ,APPLY_NOTE ,MAT_CLASS, SRCDOCNO, 
                        ISARMY, APPUNA, M_CONTID,
                        CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                      VALUES (
                        :DOCNO, :DOCTYPE, :FLOWID , :APPID , :APPDEPT , 
                        SYSDATE , :USEID , :USEDEPT , :FRWH , :TOWH , 
                        :APPLY_KIND , :APPLY_NOTE ,:MAT_CLASS, :SRCDOCNO,
                        :ISARMY, :APPUNA,:M_CONTID,
                        SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);
        }

        public int CreateD(ME_DOCD ME_DOCD)
        {
            var sql = @"INSERT INTO ME_DOCD (
                        DOCNO, SEQ, MMCODE , APPQTY, FRWH_D, AVG_PRICE, APLYITEM_NOTE, GTAPL_RESON,
                        M_CONTPRICE, UPRICE, DISC_CPRICE, DISC_UPRICE, M_NHIKEY,M_AGENNO,
                        SRCDOCNO, S_INV_QTY, INV_QTY, OPER_QTY, M_CONTID,
                        CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, SAFE_QTY)  
                        SELECT  :DOCNO, :SEQ, :MMCODE, :APPQTY, :FRWH_D, 
                                NVL((SELECT NVL(AVG_PRICE,0) FROM MI_WHCOST WHERE MMCODE = :MMCODE AND DATA_YM=CUR_SETYM AND ROWNUM=1),0) AS AVG_PRICE,
                                :APLYITEM_NOTE, :GTAPL_RESON, M_CONTPRICE, UPRICE, DISC_CPRICE, 
                                DISC_UPRICE, M_NHIKEY, M_AGENNO,:SRCDOCNO, :S_INV_QTY, :INV_QTY, :HIGH_QTY, M_CONTID,
                                SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP, :SAFE_QTY
                        FROM MI_MAST 
                        WHERE MMCODE = :MMCODE ";
            return DBWork.Connection.Execute(sql, ME_DOCD, DBWork.Transaction);
        }
        public int UpdateM(ME_DOCM ME_DOCM)
        {
            var sql = @"UPDATE ME_DOCM SET 
                        ISARMY=:ISARMY, APPUNA=:APPUNA, APPLY_NOTE = :APPLY_NOTE, 
                        APPID = :APPID, APPDEPT = USER_INID(:APPID), APPTIME = SYSDATE, USEID = :USEID, USEDEPT = :USEDEPT, 
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);
        }
        public int UpdateD(ME_DOCD ME_DOCD)
        {
            var sql = @"UPDATE ME_DOCD SET 
                        APPQTY = :APPQTY, AVG_PRICE = :AVG_PRICE, APLYITEM_NOTE = :APLYITEM_NOTE,GTAPL_RESON = :GTAPL_RESON,
                        S_INV_QTY=:S_INV_QTY, OPER_QTY=:HIGH_QTY, 
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO AND SEQ = :SEQ ";
            return DBWork.Connection.Execute(sql, ME_DOCD, DBWork.Transaction);
        }

        public int ApplyM(ME_DOCM ME_DOCM)
        {
            var sql = @"UPDATE ME_DOCM SET FLOWID = :FLOWID, APPTIME = sysdate,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP,
                        sendapvid = :UPDATE_USER, sendapvtime = sysdate,
                        sendapvdept = (SELECT INID FROM UR_ID WHERE TUSER=:UPDATE_USER)
                      WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);

        }
        public int ApplyD(ME_DOCM ME_DOCM)
        {
            var sql = @"UPDATE ME_DOCD SET EXPT_DISTQTY = APPQTY, APVQTY = APPQTY, PR_QTY=0,
                        APL_CONTIME = SYSDATE, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
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

        public int SavepkM(string docno, string doctype, string note, string newdocno)
        {
            var sql = @"INSERT INTO MM_PACK_M (
                        DOCNO, DOCTYPE, FLOWID , APPID , APPDEPT , 
                        APPLY_NOTE ,MAT_CLASS,
                        CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                       SELECT 
                        :NEWDOC, :doctype, FLOWID , APPID , TOWH , 
                        :NOTE ,MAT_CLASS,
                        SYSDATE, CREATE_USER, SYSDATE, UPDATE_USER, UPDATE_IP 
                        FROM ME_DOCM WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno, NOTE = note, NEWDOC = newdocno, DOCTYPE = doctype }, DBWork.Transaction);
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
                               B.MMNAME_C, B.MMNAME_E, B.BASE_UNIT,B.M_CONTPRICE, B.APPQTY_TIMES, B.UNITRATE,
                               (case
                                   when ((select 1 from dual where not exists (select 1 from MI_WHMM where mmcode = a.mmcode)
                                             union
                                          select 1 from MI_WHMM where wh_no = c.appdept and mmcode = a.mmcode
                                         )) = '1' 
                                   then 'Y' else 'N'
                                end) as whmm_valid
                          FROM MM_PACK_D A,MI_MAST B, MM_PACK_M C 
                         WHERE C.DOCNO=A.DOCNO AND A.MMCODE = B.MMCODE 
                           and b.cancel_id = 'N' ";
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

        public IEnumerable<ME_DOCD> GetSaveD(string MAT_CLASS, string APPDEPT, int page_index, int page_size, string sorters, string hospCode)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT distinct P.*,
                         (case when appqty_t  <= PHR_MAX_APPQTY then appqty_t else PHR_MAX_APPQTY end ) as appqty
                         FROM (
                           select A.MMCODE, 
                                  CEIL((B.NORMAL_QTY-A.INV_QTY)/NVL(C.APPQTY_TIMES ,1))* NVL(C.APPQTY_TIMES,1) APPQTY_t,
                                  C.MMNAME_C, C.MMNAME_E, C.BASE_UNIT,C.M_CONTPRICE,C.DISC_CPRICE as AVG_PRICE, 
                                  (select y.phr_max_appqty from MI_WINVCTL z, MI_BASERO_14 y 
                                    where z.wh_no = a.wh_no and z.mmcode = a.mmcode 
                                    and z.supply_whno = y.wh_no and z.mmcode = y.mmcode
                                    ) PHR_MAX_APPQTY, 
                                  A.INV_QTY,
                                  (case
                                    when ((select 1 from dual where not exists (select 1 from MI_WHMM where mmcode = a.mmcode)
                                              union
                                           select 1 from MI_WHMM where wh_no = a.wh_no and mmcode = a.mmcode
                                          )) = '1' 
                                    then 'Y' else 'N'
                                 end) as whmm_valid
                             from MI_WHINV A, MI_BASERO_14 B, MI_MAST C
                            where A.MMCODE=B.MMCODE AND A.MMCODE=C.MMCODE and B.RO_WHTYPE='2'
                              and (select count(*) from MI_WHMAST where A.WH_NO=A.WH_NO and WH_KIND='0' and WH_GRADE='2')>0
                              and A.INV_QTY<B.NORMAL_QTY  and C.CANCEL_ID <> 'Y'  
                              and a.wh_no = b.wh_no
            ";

            if (MAT_CLASS != "")
            {
                sql += " AND C.MAT_CLASS = :p2 ";

            }
            if (APPDEPT != "")
            {
                sql += " AND A.WH_NO = :p3 ";
            }

            sql += string.Format(@" 
                    ) P WHERE 1=1 
                    {0} "
            , hospCode == "805" ? " and(P.APPQTY + P.INV_QTY) < P.PHR_MAX_APPQTY" : " and p.appqty_t > 0 ");  // 805: 請領量+現有庫存不可超過庫房設定的單位請領量

            p.Add(":p2", string.Format("{0}", MAT_CLASS));
            p.Add(":p3", string.Format("{0}", APPDEPT));
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCD>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetDocnopkCombo(string p0, string p1, string p2)
        {
            string sql = @"SELECT DISTINCT DOCNO as VALUE, DOCNO as TEXT,
                        DOCNO as COMBITEM,CREATE_TIME EXTRA1
                        FROM MM_PACK_M 
                        WHERE 1=1 
                        AND MAT_CLASS = :MAT_CLASS
                        AND APPDEPT = :APPDEPT
                        AND DOCTYPE = :DOCTYPE
                        ORDER BY EXTRA1 DESC ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { MAT_CLASS = p0, APPDEPT = p1, DOCTYPE = p2 });
        }
        public IEnumerable<COMBO_MODEL> GetDocpknoteCombo(string p0)
        {
            string sql = @"SELECT DISTINCT DOCNO as VALUE, APPLY_NOTE as TEXT,
                        APPLY_NOTE as COMBITEM,CREATE_TIME EXTRA1
                        FROM MM_PACK_M 
                        WHERE 1=1 AND DOCTYPE in ('MS', 'MR')
                        AND MAT_CLASS='01'
                        AND APPDEPT=:APPDEPT
                        ORDER BY EXTRA1 DESC ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { APPDEPT = p0 });
        }

        public IEnumerable<COMBO_MODEL> GetFlowidCombo(string tuser)
        {
            string sql = @"
                        select A.FLOWID as VALUE, A.FLOWNAME as TEXT,
                        A.FLOWID || ' ' || A.FLOWNAME as COMBITEM 
                        from ME_FLOW A 
                        where A.DOCTYPE = 'MR'  and substr(flowid, length(flowid),1) in ('1','2','3','9')
                        and USER_INID(:TUSER) in (select INID from MI_WHMAST where WH_KIND = '0' and WH_GRADE = '2' and CANCEL_ID = 'N')
                        union
                        select A.FLOWID as VALUE, A.FLOWNAME as TEXT,
                        A.FLOWID || ' ' || A.FLOWNAME as COMBITEM 
                        from ME_FLOW A 
                        where A.DOCTYPE = 'MR'  and substr(flowid, length(flowid),1) in ('1','2','3','9')
                        and (select count(*) from MI_WHID A,MI_WHMAST B WHERE A.WH_NO = B.WH_NO AND A.WH_USERID=:TUSER AND B.WH_KIND = '0' and WH_GRADE = '2' and CANCEL_ID = 'N') > 0
                        union
                        select A.FLOWID as VALUE, A.FLOWNAME as TEXT,
                        A.FLOWID || ' ' || A.FLOWNAME as COMBITEM 
                        from ME_FLOW A 
                        where A.DOCTYPE = 'MS'  and substr(flowid, length(flowid),1) in ('1','2','3','9')
                        and USER_INID(:TUSER) not in (select INID from MI_WHMAST where WH_KIND = '0' and WH_GRADE = '2' and CANCEL_ID = 'N')
                        and (select count(*) from MI_WHID A,MI_WHMAST B WHERE A.WH_NO = B.WH_NO AND A.WH_USERID=:TUSER AND B.WH_KIND = '0' and WH_GRADE <> '2' and CANCEL_ID = 'N') > 0
                        order by VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { TUSER = tuser });
        }

        public IEnumerable<MI_MAST> GetMmCodeCombo(string p0, string p1, string docno, int page_index, int page_size, string sorters)
        {

            var p = new DynamicParameters();

            var sql = @"
                       with temp as (
                            select wh_grade from MI_WHMAST where wh_no = (select towh from ME_DOCM where docno = :DOCNO)
                       )
                       SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT, A.M_CONTPRICE, A.DISC_UPRICE, 
                               NVL(( SELECT AVG_PRICE FROM MI_WHCOST WHERE MMCODE = A.MMCODE AND DATA_YM=CUR_SETYM AND ROWNUM=1 ),0) AS AVG_PRICE,
                               A.PFILE_ID,
                               (case
                                    when ((select 1 from dual where not exists (select 1 from MI_WHMM where mmcode = a.mmcode)
                                              union
                                           select 1 from MI_WHMM where wh_no = c.towh and mmcode = a.mmcode
                                          )) = '1' 
                                    then 'Y' else 'N'
                                 end) as whmm_valid,
                                NVL((select (case when (select wh_grade from temp) = '2' and a.mat_class='01' then phr_max_appqty else g34_max_appqty end) from MI_BASERO_14 
                                      where wh_no = ( SELECT supply_whno FROM MI_WINVCTL WHERE MMCODE = A.MMCODE AND WH_NO=C.TOWH ) 
                                        and mmcode = a.mmcode),0) AS HIGH_QTY,
                                INV_QTY(( SELECT supply_whno FROM MI_WINVCTL WHERE MMCODE = A.MMCODE AND WH_NO=c.TOWH), a.MMCODE) as S_INV_QTY,
                                INV_QTY(c.TOWH, a.MMCODE) as INV_QTY, A.APPQTY_TIMES, A.UNITRATE,
                                 (SELECT DATA_DESC SPE FROM PARAM_D 
                                    WHERE GRP_CODE='MI_MAST'  AND DATA_NAME='M_CONTID' AND DATA_VALUE=A.M_CONTID) M_CONTID,
                                 (SELECT EASYNAME from PH_VENDER where AGEN_NO=A.M_AGENNO) AS M_AGENNO, 
                                 (select SAFE_QTY from MI_BASERO_14 
                                  where MMCODE=A.MMCODE and WH_NO=( ( SELECT supply_whno FROM MI_WINVCTL WHERE MMCODE = A.MMCODE AND WH_NO=C.TOWH ) )) as SAFE_QTY
                          FROM MI_MAST A 
                         INNER JOIN ME_DOCM C ON C.DOCNO=:DOCNO
                          LEFT OUTER JOIN MI_WINVCTL D ON D.MMCODE = A.MMCODE AND D.WH_NO IN (SELECT WH_NO FROM MI_WHMAST WHERE WH_KIND='1' AND WH_GRADE='1')
                         WHERE 1=1 
                           AND A.MAT_CLASS = :MAT_CLASS 
                           AND nvl(A.CANCEL_ID, 'N') <> 'Y' AND NVL(A.E_ORDERDCFLAG,'N')='N' ";
            sql += " {1} ";

            if (p0 != "")
            {
                sql = string.Format(sql,
                     "(NVL(INSTR(UPPER(A.MMCODE), UPPER(:MMCODE_I)), 1000) + NVL(INSTR(UPPER(MMNAME_E), UPPER(:MMNAME_E_I)), 100) * 10 + NVL(INSTR(UPPER(MMNAME_C), UPPER(:MMNAME_C_I)), 100) * 10) IDX,",
                     @"   AND (UPPER(A.MMCODE) LIKE UPPER(:MMCODE) OR UPPER(MMNAME_E) LIKE UPPER(:MMNAME_E) OR UPPER(MMNAME_C) LIKE UPPER(:MMNAME_C))");
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
            p.Add(":DOCNO", docno);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMmcode(MI_MAST_QUERY_PARAMS query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT,A.M_CONTPRICE,A.DISC_UPRICE, 
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
                               AND DATA_NAME='M_APPLYID' AND DATA_VALUE=A.M_APPLYID) M_APPLYID,
                               (case
                                   when ((select 1 from dual where not exists (select 1 from MI_WHMM where mmcode = a.mmcode)
                                             union
                                          select 1 from MI_WHMM where wh_no = :WH_NO and mmcode = a.mmcode
                                         )) = '1' 
                                   then 'Y' else 'N'
                                end) as whmm_valid,
                                INV_QTY(( SELECT supply_whno FROM MI_WINVCTL WHERE MMCODE = A.MMCODE AND WH_NO=:WH_NO), A.MMCODE) as S_INV_QTY,
                                A.M_AGENNO,
                                (select AGEN_NAMEC from PH_VENDER where AGEN_NO = A.M_AGENNO) as AGEN_NAMEC,
                                (select AGEN_NAMEE from PH_VENDER where AGEN_NO = A.M_AGENNO) as AGEN_NAMEE,
                                A.DRUGSNAME
                          FROM MI_MAST A 
                         WHERE 1=1 
                           AND A.MAT_CLASS = :MAT_CLASS  
                           AND nvl(A.CANCEL_ID, 'N') <> 'Y'  AND NVL(A.E_ORDERDCFLAG,'N')='N' ";

            if (query.MMCODE != "")
            {
                sql += " AND UPPER(A.MMCODE) LIKE :MMCODE ";
            }
            if (query.MMNAME_C != "")
            {
                sql += " AND A.MMNAME_C LIKE :MMNAME_C ";
            }
            if (query.MMNAME_E != "")
            {
                sql += " AND UPPER(A.MMNAME_E) LIKE UPPER(:MMNAME_E) ";
            }
            if (query.M_AGENNO != "")
            {
                sql += " AND A.M_AGENNO LIKE :M_AGENNO ";
            }
            if (query.AGEN_NAME != "")
            {
                sql += " AND ((select AGEN_NAMEC from PH_VENDER where AGEN_NO = A.M_AGENNO) LIKE :AGEN_NAME ";
                sql += "  OR (select AGEN_NAMEE from PH_VENDER where AGEN_NO = A.M_AGENNO) LIKE :AGEN_NAME) ";
            }
            if (query.DRUGSNAME != "")
            {
                sql += " AND A.DRUGSNAME LIKE :DRUGSNAME ";
            }

            p.Add(":WH_NO", query.WH_NO);
            p.Add(":MAT_CLASS", query.MAT_CLASS);
            // 決定MMCODE查詢的規則
            string mmcode_format = "{0}";
            if (query.MMCODE_Q1)
                mmcode_format = "%" + mmcode_format;
            if (query.MMCODE_Q2)
                mmcode_format = mmcode_format + "%";
            p.Add(":MMCODE", string.Format(mmcode_format, query.MMCODE));
            p.Add(":MMNAME_C", string.Format("%{0}%", query.MMNAME_C));
            p.Add(":MMNAME_E", string.Format("%{0}%", query.MMNAME_E));
            p.Add(":M_AGENNO", string.Format("%{0}%", query.M_AGENNO));
            p.Add(":AGEN_NAME", string.Format("%{0}%", query.AGEN_NAME));
            p.Add(":DRUGSNAME", string.Format("%{0}%", query.DRUGSNAME));

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
                sql = string.Format(sql, "(NVL(INSTR(UPPER(A.MMCODE), UPPER(:MMCODE_I)), 1000) + NVL(INSTR(UPPER(MMNAME_E), UPPER(:MMNAME_E_I)), 100) * 10 + NVL(INSTR(UPPER(MMNAME_C), UPPER(:MMNAME_C_I)), 100) * 10) IDX,");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (UPPER(A.MMCODE) LIKE UPPER(:MMCODE) ";
                p.Add(":MMCODE", string.Format("{0}%", p0));

                sql += " OR UPPER(MMNAME_E) LIKE UPPER(:MMNAME_E) ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR UPPER(MMNAME_C) LIKE UPPER(:MMNAME_C)) ";
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

        public IEnumerable<COMBO_MODEL> GetTowhCombo(string id)
        {
            string sql = @"select A.WH_NO VALUE , A.WH_NO||' '||B.WH_NAME TEXT ,A.WH_NO || ' ' || B.WH_NAME COMBITEM 
                            from MI_WHID A,MI_WHMAST B
                            WHERE A.WH_NO = B.WH_NO 
                              AND A.WH_USERID=:TUSER 
                              AND B.WH_KIND = '0' and B.CANCEL_ID = 'N'
                              and b.wh_grade <> '1'
                            UNION  
                            SELECT A.WH_NO ,A.WH_NO||' '||WH_NAME,A.WH_NO || ' ' || A.WH_NAME COMBITEM 
                            FROM MI_WHMAST A
                            WHERE A.INID= USER_INID(:TUSER) 
                              AND A.WH_KIND = '0' and A.CANCEL_ID = 'N'
                              and a.wh_grade <> '1'
                            ORDER BY 1";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { TUSER = id });
        }

        public IEnumerable<AB0003Model> GetLoginInfo(string id, string ip)
        {
            string sql = @"SELECT TUSER AS USERID, UNA AS USERNAME, INID, INID_NAME(INID) AS INIDNAME,
                        WHNO_ME1 CENTER_WHNO,INID_NAME(WHNO_ME1) AS CENTER_WHNAME, TO_CHAR(SYSDATE,'YYYYMMDD') AS TODAY,
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
                        (case when (select count(*) from UR_UIR where RLNO in ('MED_14','MMSpl_14','PHR_14') and TUSER = UR_ID.TUSER) > 0 then 'Y' else 'N' end) as IS_GRADE1  
                        FROM UR_ID
                        WHERE UR_ID.TUSER=:TUSER";

            return DBWork.Connection.Query<AB0003Model>(sql, new { TUSER = id, UPDATE_IP = ip });
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
        public bool CheckExists(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCM WHERE DOCNO=:DOCNO";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }

        public string getDoctype(string wh_no)
        {
            string sql = @"SELECT (case when :WH_NO in (select WH_NO from MI_WHMAST where WH_KIND = '0' and WH_GRADE = '2') 
                then 'MR'
                else 'MS' end) as DOCTYPE
                FROM dual ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { WH_NO = wh_no }, DBWork.Transaction).ToString();
            return rtn;
        }

        public bool CheckFlowId01(string docno)
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

        public bool CheckMmcode(string id)
        {
            string sql = @"SELECT 1 FROM MI_MAST WHERE MMCODE=:MMCODE
                            AND MAT_CLASS = '01'
                        ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = id }, DBWork.Transaction) == null);
        }

        public bool CheckExistsMM(string id, string mm)
        {
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO AND MMCODE=:MMCODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id, MMCODE = mm }, DBWork.Transaction) == null);
        }

        public bool CheckExistsD(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }

        public string GetDocDSeq(string id)
        {
            string sql = @"SELECT MAX(SEQ)+1 as SEQ FROM ME_DOCD WHERE DOCNO=:DOCNO ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction).ToString();
            return rtn;
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

        public bool CheckDuplicateMmcode(string docno)
        {
            string sql = @"select 1 from ME_DOCD
                            where docno = :docno
                            group by mmcode
                           having count(*) > 1";

            return !(DBWork.Connection.ExecuteScalar(sql, new { docno = docno }, DBWork.Transaction) == null);
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

        public bool CheckExistsM(string id, string flowid)
        {
            string sql = @"SELECT 1 FROM ME_DOCM WHERE DOCNO=:DOCNO ";
            if (flowid == "01")
            {
                sql += " AND FLOWID <> '0601' AND FLOWID <> '0101'";
            }
            if (flowid == "11")
            {
                sql += " AND FLOWID <> '0611' AND FLOWID <> '0111'";
            }
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }

        public bool CheckExistsDN(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO AND APPQTY = 0 ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }

        public bool CheckExistsMMCODE(string id)
        {
            string sql = @" SELECT 1 FROM MI_MAST WHERE 1=1 
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

        public DataTable GetExcel()
        {
            var p = new DynamicParameters();

            var sql = @" SELECT '' 院內碼,'' 申請數量, '' 備註 FROM DUAL ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        // 依據 DOCNO 匯出 DETAIL EXCEL
        public DataTable GetDetailExcel(string docno, string mmcode)
        {
            var p = new DynamicParameters();

            var sql = @"
                                 SELECT P.DOCNO as 申請單號,P.MMCODE as 院內碼,P.MMNAME_C aS 中文品名,P.MMNAME_E as 英文品名,
                                                P.BASE_UNIT as 單位,S_INV_QTY as 上級庫庫房存量,P.POSTID as 撥發狀態,P.HIGH_QTY as 基準量, P.SAFE_QTY as 庫房安全量, P.APPQTY as 申請數量,
                                                P.APVQTY as 核撥量,P.APL_CONTIME as 送核撥時間,P.FRWH_D as 出庫庫房, P.M_CONTID_T as 合約識別碼,
                                                P.APLYITEM_NOTE as 備註
                                    FROM
                                        (
                                           SELECT A.DOCNO, A.SEQ, A.MMCODE , A.APPQTY , A.APLYITEM_NOTE, 
                                                         A.GTAPL_RESON, SUM(A.ACKQTY) AS ACKQTY, C.SENDAPVTIME, MAX(A.APVTIME) AS APVTIME, B.DISC_UPRICE,
                                                         B.MMNAME_C, B.MMNAME_E, B.BASE_UNIT, B.M_CONTPRICE, SUM(NVL(A.APVQTY,0)) APVQTY, A.FRWH_D,
                                                         twn_date(A.APL_CONTIME) as APL_CONTIME, A.CREATE_USER, USER_NAME(A.CREATE_USER) AS CREATE_USER_NAME, 
                                                         A.S_INV_QTY,A.INV_QTY, A.OPER_QTY as HIGH_QTY, A.M_CONTID, A.SAFE_QTY, 
                                                         (SELECT DISTINCT RTrim(DATA_DESC) FROM PARAM_D WHERE GRP_CODE='MI_MAST' and DATA_NAME='M_CONTID' AND DATA_VALUE = A.M_CONTID) as M_CONTID_T,
                                                        B.APPQTY_TIMES, B.UNITRATE,
                                                        (case when A.POSTID IS NULL then '待核可' 
                                                         else (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D 
                                                        where GRP_CODE = 'ME_DOCD' and DATA_NAME = 'POSTID' and DATA_VALUE = A.POSTID) end) as POSTID
                                            FROM ME_DOCD A 
                                           INNER JOIN MI_MAST B ON B.MMCODE=A.MMCODE 
                                           INNER JOIN ME_DOCM C ON C.DOCNO=A.SRCDOCNO 
                                          WHERE 1=1
                                ";

            if (docno != "")
            {
                sql += " AND A.DOCNO = :p0 ";
                p.Add(":p0", string.Format("{0}", docno));
            }
            else
            {
                sql += " AND 1=2 ";
            }
            if (mmcode != "")
            {
                sql += " AND A.MMCODE LIKE :p1 ";
                p.Add(":p1", string.Format("%{0}%", mmcode));
            }
            sql += @" group by  A.DOCNO, A.SEQ, A.MMCODE, A.APPQTY, A.APLYITEM_NOTE,
                                A.GTAPL_RESON, C.SENDAPVTIME, B.DISC_UPRICE, A.FRWH_D, 
                                B.MMNAME_C, B.MMNAME_E, B.BASE_UNIT,B.M_CONTPRICE, A.APL_CONTIME, 
                                A.CREATE_USER, A.S_INV_QTY, A.INV_QTY, A.OPER_QTY, A.M_CONTID, A.SAFE_QTY,
                                B.APPQTY_TIMES, B.UNITRATE,POSTID
                 ) P ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public string GetDocAppAmout(string docno)
        {
            var sql = @" select nvl(sum((case when A.AVG_PRICE is null then NVL(( SELECT AVG_PRICE FROM MI_WHCOST WHERE MMCODE = A.MMCODE AND DATA_YM=CUR_SETYM AND ROWNUM=1 ),0) else A.AVG_PRICE end) * A.APPQTY), 0) as APP_AMOUT 
                            from ME_DOCD A where A.DOCNO = :DOCNO ";
            return DBWork.Connection.ExecuteScalar(sql, new { DOCNO = docno }, DBWork.Transaction).ToString();
        }

        public string GetThisTowh(string docno)
        {
            var sql = @"select TOWH from me_docm where DOCNO=:DOCNO";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public string GetFrwhWithMmcode(string towh, string mmcode)
        {
            var sql = @"SELECT SUPPLY_WHNO FROM MI_WINVCTL WHERE WH_NO = :TOWH AND MMCODE = :MMCODE";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { TOWH = towh, MMCODE = mmcode }, DBWork.Transaction);
        }
        public string getDocTowh(string docno)
        {
            string sql = @"SELECT TOWH from ME_DOCM where DOCNO = :DOCNO ";
            string rtn = Convert.ToString(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = docno }, DBWork.Transaction));
            return rtn;
        }
        public int DetailUpdateDocno(string old_docno, string frwh, string mcondit, string new_docno, string update_user, string update_ip)
        {
            var sql = @"UPDATE ME_DOCD SET DOCNO=:NEW_DOCNO, SRCDOCNO=:NEW_DOCNO, 
                           UPDATE_TIME=SYSDATE, UPDATE_USER=:UPDATE_USER, UPDATE_IP=:UPDATE_IP 
                         WHERE DOCNO=:DOCNO AND FRWH_D=:FRWH_D and M_CONTID=:M_CONTID";
            return DBWork.Connection.Execute(sql, new { DOCNO = old_docno, FRWH_D = frwh, M_CONTID = mcondit, NEW_DOCNO = new_docno, UPDATE_USER = update_user, UPDATE_IP = update_ip }, DBWork.Transaction);
        }
        public IEnumerable<COMBO_MODEL> GetIsArmyCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='ISARMY' 
                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public string GetOperqtyWithMmcode(string towh, string mmcode)
        {
            var sql = @"with temp as (
                            select wh_grade from MI_WHMAST where wh_no = :TOWH
                        )
                        select (case when (select wh_grade from temp) = '2' and (select 1 from MI_MAST where mmcode = :MMCODE and mat_class='01')='1'
                                     then phr_max_appqty else g34_max_appqty end) 
                          from MI_BASERO_14 
                         where wh_no = (SELECT supply_whno FROM MI_WINVCTL WHERE MMCODE = :MMCODE AND WH_NO=:TOWH) 
                           and mmcode = :MMCODE";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { TOWH = towh, MMCODE = mmcode }, DBWork.Transaction);
        }
        public string GetSinvqtyWithMmcode(string towh, string mmcode)
        {
            var sql = @"select INV_QTY((SELECT supply_whno FROM MI_WINVCTL WHERE MMCODE = :MMCODE AND WH_NO=:TOWH ), :MMCODE) from dual";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { TOWH = towh, MMCODE = mmcode }, DBWork.Transaction);
        }
        //國軍DOCNO單號統一用GET_DAILY_DOCNO
        public string GetDailyDocno()
        {
            string sql = @"select GET_DAILY_DOCNO from DUAL";
            string rtn = DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction).ToString();
            return rtn;
        }
        public string getDocIsarmy(string docno)
        {
            string sql = @"SELECT ISARMY from ME_DOCM where DOCNO = :DOCNO ";
            string rtn = Convert.ToString(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = docno }, DBWork.Transaction));
            return rtn;
        }
        public string getDocAppuna(string docno)
        {
            string sql = @"SELECT APPUNA from ME_DOCM where DOCNO = :DOCNO ";
            string rtn = Convert.ToString(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = docno }, DBWork.Transaction));
            return rtn;
        }
        public IEnumerable<ME_DOCD> GetSplitValue(string docno)
        {
            var sql = @"select distinct FRWH_D, M_CONTID from me_docd where DOCNO=:DOCNO";
            return DBWork.Connection.Query<ME_DOCD>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }
        public int MasterUpdateFrwhMcontid(ME_DOCM me_docm)
        {
            var sql = @"UPDATE ME_DOCM SET FRWH=:FRWH,M_CONTID=:M_CONTID, FLOWID=:FLOWID,
                            UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP 
                         WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);
        }
        public string CheckMaxAppqtyFlag(string mmcode, string towh, string frwh_d, string appqty)
        {
            string sql = @"select (case when 
                                     ((nvl((select INV_QTY from MI_WHINV 
                                             where MMCODE=A.MMCODE and WH_NO=:TOWH),0)+:APPQTY) > PHR_MAX_APPQTY) then 'Y' else 'N' end ) flag
                             from MI_BASERO_14 A
                            where A.MMCODE=:MMCODE
                              and a.wh_no = (select supply_whno from MI_WINVCTL where wh_no =:TOWH and mmcode = a.mmcode)";
            if (DBWork.Connection.ExecuteScalar(sql, new { MMCODE = mmcode, TOWH = towh, FRWH_D = frwh_d, APPQTY = appqty }, DBWork.Transaction) == null)
            {
                return "Y";
            }
            else
            {
                return DBWork.Connection.ExecuteScalar(sql, new { MMCODE = mmcode, TOWH = towh, FRWH_D = frwh_d, APPQTY = appqty }, DBWork.Transaction).ToString();
            }
        }

        public IEnumerable<ME_DOCD> CheckMmcodeMaxAppqtyFlag(string docno)
        {
            string sql = @"
              with temp as(
                select A.MMCODE,A.APPQTY,A.FRWH_D,
                       NVL((select INV_QTY from MI_WHINV where MMCODE=A.MMCODE and WH_NO=B.TOWH),0) as INV_QTY,
                       (select supply_whno FROM MI_WINVCTL WHERE MMCODE = A.MMCODE AND WH_NO=B.TOWH) as SUPPLY_WHNO
                  from ME_DOCD A, ME_DOCM B where A.DOCNO=B.DOCNO and A.DOCNO=:docno
               )
               select C.MMCODE from temp C
                 left join MI_BASERO_14 D on (C.MMCODE=D.MMCODE and D.WH_NO=C.SUPPLY_WHNO)
                where (C.APPQTY+C.INV_QTY) > nvl(D.PHR_MAX_APPQTY,0)
            ";
            return DBWork.Connection.Query<ME_DOCD>(sql, new { docno }, DBWork.Transaction);
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
            public string ISCONTID3;

            public string M_AGENNO;
            public string AGEN_NAME;

            public bool MMCODE_Q1;
            public bool MMCODE_Q2;

            public string DRUGSNAME;
        }

        public DataTable GetMasterExcel(string docnos)
        {
            string sql = string.Format(@"
            SELECT a.docno 單據號碼, twn_date(apptime) 申請日期, 
                   (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='ISARMY' AND DATA_VALUE=A.ISARMY) 軍民別,
                   user_name(a.appid) as 申請人員,
                   a.appuna 申請人姓名,a.apply_note 單據備註,
                   d.mmcode 院內碼, B.MMNAME_C 中文品名, B.MMNAME_E 英文品名, b.base_unit 單位,
                   ( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = d.MMCODE AND WH_NO=a.FRWH ) 庫房存量,
                   d.appqty 申請量, d.OPER_QTY 基準量,
                   d.APLYITEM_NOTE 明細備註,
                   (case when d.POSTID IS NULL then '待核可' 
                    else (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D 
                   where GRP_CODE = 'ME_DOCD' and DATA_NAME = 'POSTID' and DATA_VALUE = d.POSTID) end) as 撥發狀態
              from ME_DOCM a
             left join ME_DOCD d on (a.docno = d.docno)
             left JOIN MI_MAST B ON B.MMCODE = d.MMCODE 
             where 1=1
             and a.docno in ( {0} )
            ", docnos);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public bool CheckMmcodeCancelSingle(string mmcode)
        {
            string sql = @"
                select 1 from MI_MAST where mmcode = :mmcode and nvl(cancel_id, 'N')='Y'
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { mmcode }, DBWork.Transaction) != null;
        }
        public bool Checkappqty(string id, string qty)
        {
            string sql = @" SELECT 1 FROM MI_MAST A 
                            WHERE 1=1 AND A.MMCODE=:MMCODE 
                            AND mod(:APPQTY , NVL(A.APPQTY_TIMES ,1)) > 0";

            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = id, APPQTY = qty }, DBWork.Transaction) == null);
        }
        public IEnumerable<ME_DOCD> CheckMinOrderQty(string docno)
        {
            string sql = @"
                with appqty_times as (
                    select a.mmcode, 
                           nvl( ( select appqty_times from MI_MAST where mmcode = a.mmcode) ,1) as appqty_times
                     from ME_DOCD a
                    where a.docno = :docno
                )
                select a.*,
                       (case 
                          when mod(nvl(a.appqty, 0), b.appqty_times) = 0
                          then 'Y' else 'N'
                         end) as is_appqty_valid 
                  from ME_DOCD a, appqty_times b
                 where a.docno = :docno
                   and a.mmcode = b.mmcode
            ";

            return DBWork.Connection.Query<ME_DOCD>(sql, new { docno }, DBWork.Transaction);
        }
        public int CheckUnitrateFlg(string docno)
        {
            // 檢查所選申請單內申請量是否為出貨單位的倍數
            string sql = @"select count(*) from ME_DOCD A, MI_MAST B
                            where A.MMCODE=B.MMCODE and A.DOCNO = :DOCNO
                              and mod(A.APPQTY,B.UNITRATE)<>0 ";
            return DBWork.Connection.QueryFirst<int>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }
        public string GetInvqtyWithMmcode(string towh, string mmcode)
        {
            var sql = @"select INV_QTY(:TOWH, :MMCODE) from dual";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { TOWH = towh, MMCODE = mmcode }, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCD> GetInvqty(string mmcode, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.MMCODE ,
                               A.WH_NO,
                               WH_NAME(A.WH_NO) WH_NAME,
                               A.INV_QTY,
                               B.SAFE_QTY,
                               B.NORMAL_QTY
                          FROM MI_WHINV A 
                          LEFT JOIN MI_BASERO_14 B ON (A.WH_NO=B.WH_NO AND A.MMCODE=B.MMCODE)
                         WHERE A.MMCODE = :p0 ";

            p.Add(":p0", string.Format("{0}", mmcode));
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCD>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public string GetHospCode()
        {
            string sql = @"
                select data_value from PARAM_D where grp_code='HOSP_INFO' and data_name='HospCode'
            ";

            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }


    }
}
