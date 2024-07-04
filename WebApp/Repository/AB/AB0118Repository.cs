using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using BarcodeLib;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace WebApp.Repository.AB
{

    public class AB0118Report_MODEL : JCLib.Mvc.BaseModel
    {
        public string DOCNO { get; set; }
        public string APPID { get; set; }
        public string APPDEPT { get; set; }
        public string MMCODE { get; set; }
        public Int32 APPQTY { get; set; }
        public Int32 APVQTY { get; set; }
        public string APLYITEM_NOTE { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string BASE_UNIT { get; set; }
        public string APPDEPT_N { get; set; }
        public string APPID_N { get; set; }
        public string FRWH_N { get; set; }
        public string APPLY_NOTE { get; set; }
        public string FRWHINV_QTY { get; set; }
        public string TOWHINV_QTY { get; set; }
        public string ISARMY_N { get; set; }
        public string APPTIME_T { get; set; }
        public string DISC_CPRICE { get; set; }
        public string M_AGENNO { get; set; }
        public string M_CONTID_T { get; set; }
        public string E_ITEMARMYNO { get; set; }
        public string E_CODATE { get; set; }
        public string CASENO { get; set; }
        public string MAX_QTY { get; set; }
    }
    public class HIS14_SUPDET : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string USEQTY { get; set; } // 耗用量
        public string SUP_USEQTY { get; set; }
        public string SUP_UNIT { get; set; } // 耗用單位
        public string IDNO { get; set; } // 病患證號
        public string PATIENTNAME { get; set; } // 病患姓名
        public string SUP_PATNAME { get; set; }// 病患姓名
        public string SUP_MEDNO { get; set; }// 病歷號
        public string SECTIONNO { get; set; } // 科別
        public string SECTIONNAME { get; set; } // 科別名稱
        public string DRID { get; set; } // 醫師代碼
        public string DRNAME { get; set; } // 醫師姓名
        public string USEDATE { get; set; } // 消耗日期
        public string PROCDATE { get; set; } // 處理日期
        public string DOCNO { get; set; } // 申請單號
        public string SUPDET_SEQ { get; set; } // 流水號
        public string SUP_SKDIACODE { get; set; } // 院內碼
        public string CREATE_USER { get; set; }
        // public string UPDATE_TIME { get; set; }
        public string UPDATE_IP { get; set; }
        public string UPDATE_USER { get; set; }
        public string SUP_USEDATE_S { get; set; } // 消耗日期開始
        public string SUP_USEDATE_E { get; set; } // 消耗日期結束
        public string MMCODE_OR_MMNAME_C { get; set; } // 院內碼或中文品名
        public string SUP_PATIDNO_OR_SUP_PATNAME { get; set; } // 病患證號或姓名
        public string SUP_FEATOPID_OR_SUP_EMPNAME { get; set; } // 醫師代碼或姓名 
        public Int32 APPQTY { get; set; }
    }

    public class AB0118Repository : JCLib.Mvc.BaseRepository
    {
        #region " flylon "
        const String sBr = "\r\n";
        public AB0118Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public int Delete_SEC_USEMM(HIS14_SUPDET v)
        {
            var sql = @""; //  DELETE from ME_DOCM WHERE DOCNO = :DOCNO";
            sql += sBr + "DELETE from SEC_USEMM ";
            sql += sBr + "where 1=1 ";
            sql += sBr + "and SECTIONNO=:sectionno ";
            sql += sBr + "and MMCODE=:mmcode ";

            return DBWork.Connection.Execute(sql, v, DBWork.Transaction);
        }
        #endregion // end of flylon
        public IEnumerable<ME_DOCM> GetAllM(string DOCNO, string APPTIME1, string APPTIME2, string[] str_FLOWIDs, string APPLY_KIND, string MAT_CLASS, string tuser, string hospCode, int page_index, int page_size, string sorters)

        {
            var p = new DynamicParameters();

            string addCol = "";
            // 若為桃園則增加顯示庫備/非庫備
            if (hospCode == "804")
                addCol = @", (
                            select DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'M_STOREID' and DATA_VALUE = A.M_STOREID
                        ) as M_STOREID_T ";

            var sql = @"
                            SELECT A.* , 
                            (
                                SELECT DATA_DESC FROM PARAM_D
                                WHERE GRP_CODE='ME_DOCM'
                                AND DATA_NAME='FLOWID_MR1'
                                AND DATA_VALUE=A.FLOWID 
                            ) FLOWID_N,
                            ( 
                                SELECT 
                                MAT_CLSNAME FROM MI_MATCLASS
                                WHERE MAT_CLASS=A.MAT_CLASS
                            ) MAT_CLASS_N,
                            (
                                SELECT WH_NO || ' ' || WH_NAME 
                                from MI_WHMAST
                                where WH_NO=A.FRWH
                            ) FRWH_N,
                            (	
                                SELECT WH_NO || ' ' || WH_NAME
                                from MI_WHMAST
                                where WH_NO=A.TOWH 
                            ) TOWH_N,
                            (
                                SELECT DATA_DESC FROM PARAM_D
                                WHERE 1=1
                                and GRP_CODE='ME_DOCM'
                                AND DATA_NAME='APPLY_KIND'
                                AND DATA_VALUE=A.APPLY_KIND 
                            ) APPLY_KIND_N,
                            ( 
                                SELECT UNA FROM UR_ID WHERE 1=1
                                and TUSER=A.APPID
                            ) APP_NAME,
                            (
                                SELECT INID_NAME FROM UR_INID 
                                WHERE 1=1
                                and INID=A.APPDEPT 
                            ) APPDEPT_NAME,
                            TWN_DATE(A.APPTIME) APPTIME_T ,
                            (
                                SELECT EXT FROM UR_ID
                                WHERE TUSER=A.APPID
                            ) EXT,
                            (
                                SELECT DATA_DESC
                                FROM PARAM_D
                                WHERE GRP_CODE='ME_DOCM'
                                AND DATA_NAME='ISARMY' 
                                AND DATA_VALUE=A.ISARMY 
                            ) ISARMY_N,
                            (
                                SELECT DISTINCT rtrim(DATA_DESC) FROM PARAM_D 
                                WHERE GRP_CODE = 'MI_MAST'
                                AND DATA_NAME = 'M_CONTID'
                                AND DATA_VALUE = A.M_CONTID 
                                ) M_CONTID_T " + addCol
                        + @" FROM ME_DOCM A
                            WHERE 1=1
                            AND A.DOCNO = A.SRCDOCNO 
                            AND A.DOCTYPE IN ('MR5','MR6') ";

            if (tuser != "")
            {
                sql += sBr + @" AND (A.TOWH IN ( SELECT WH_NO FROM MI_WHID B WHERE B.WH_USERID=:TUSER 
                                          UNION
                                          SELECT WH_NO FROM MI_WHMAST C WHERE C.INID = USER_INID(:TUSER)
                                        ) 
                                    OR (select count(*) from UR_UIR where RLNO in ('MAT_14') and TUSER = :TUSER) > 0
                                )"; // 若有衛材庫群組則可以看所有單子,否則只能看TOWH有權限的
                p.Add(":TUSER", string.Format("{0}", tuser));
            }

            if (DOCNO != "")
            {
                sql += sBr + " AND A.DOCNO LIKE :p0 ";
                p.Add(":p0", string.Format("%{0}%", DOCNO));
            }
            if (APPTIME1 != "" & APPTIME2 != "")
            {
                sql += sBr + " AND TWN_DATE(A.APPTIME) BETWEEN :d0 AND :d1 ";
                p.Add(":d0", string.Format("{0}", APPTIME1));
                p.Add(":d1", string.Format("{0}", APPTIME2));
            }
            if (APPTIME1 != "" & APPTIME2 == "")
            {
                sql += sBr + " AND TWN_DATE(A.APPTIME) >= :d0 ";
                p.Add(":d0", string.Format("{0}", APPTIME1));
            }
            if (APPTIME1 == "" & APPTIME2 != "")
            {
                sql += sBr + " AND TWN_DATE(A.APPTIME) <= :d1 ";
                p.Add(":d1", string.Format("{0}", APPTIME2));
            }
            //判斷FLOWID查詢條件是否有值，有的話用字串相加的方式串接條件(IN的方法會有問題)
            if (str_FLOWIDs.Length > 0)
            {
                sql += "  AND a.flowid in :p2 ";
                p.Add(":p2", str_FLOWIDs);
            }
            if (APPLY_KIND != "")
            {
                sql += sBr + " AND A.APPLY_KIND = :p3 ";
                p.Add(":p3", string.Format("{0}", APPLY_KIND));
            }
            if (MAT_CLASS != "")
            {
                sql += sBr + " AND A.MAT_CLASS = :p4 ";
                p.Add(":p4", string.Format("{0}", MAT_CLASS));
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCM>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }


        public IEnumerable<ME_DOCD> GetAllD(string DOCNO, string MMCODE, string hospCode, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string addCol1 = "";
            string addCol2 = "";
            // 若為桃園則增加顯示庫備/非庫備
            if (hospCode == "804")
            {
                addCol1 = @", (
                            select DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'M_STOREID' and DATA_VALUE = P.M_STOREID
                        ) as M_STOREID_T ";
                addCol2 = @", (
                            select M_STOREID from MI_MAST where MMCODE = A.MMCODE
                        ) as M_STOREID ";
            }

            var sql = @"SELECT P.*,
                        NVL(P.APPQTY * P.DISC_UPRICE,0) AS APP_AMT
                        " + addCol1 + @"
                        FROM (
                        SELECT A.SRCDOCNO AS DOCNO, A.SEQ, A.MMCODE , A.APPQTY , A.APLYITEM_NOTE, 
                        A.GTAPL_RESON, SUM(A.ACKQTY) AS ACKQTY, C.SENDAPVTIME, MAX(A.APVTIME) AS APVTIME, B.DISC_UPRICE,
                        B.MMNAME_C, B.MMNAME_E, B.BASE_UNIT, B.M_CONTPRICE, SUM(NVL(A.APVQTY,0)) APVQTY, A.FRWH_D, A.M_AGENNO,
                        twn_date(A.APL_CONTIME) as APL_CONTIME, A.CREATE_USER, USER_NAME(A.CREATE_USER) AS CREATE_USER_NAME, 
                        A.S_INV_QTY, A.INV_QTY, A.OPER_QTY as HIGH_QTY, A.CHINNAME, A.CHARTNO, A.M_CONTID,
                        (SELECT DISTINCT RTrim(DATA_DESC) FROM PARAM_D WHERE GRP_CODE='MI_MAST' and DATA_NAME='M_CONTID' AND DATA_VALUE = A.M_CONTID) as M_CONTID_T,
                        B.APPQTY_TIMES, B.UNITRATE,A.CHINNAME as CHINNAME_OLD, A.CHARTNO as CHARTNO_OLD
                        " + addCol2 + @"
                        FROM ME_DOCD A 
                        INNER JOIN MI_MAST B ON B.MMCODE=A.MMCODE 
                        INNER JOIN ME_DOCM C ON C.DOCNO=A.SRCDOCNO 
                        WHERE 1=1 ";

            if (DOCNO != "")
            {
                sql += sBr + " AND A.SRCDOCNO LIKE :p0 ";
                p.Add(":p0", string.Format("%{0}%", DOCNO));
            }
            else
            {
                sql += sBr + " AND 1=2 ";
            }
            if (MMCODE != "")
            {
                sql += sBr + " AND A.MMCODE LIKE :p1 ";
                p.Add(":p1", string.Format("%{0}%", MMCODE));
            }
            sql += sBr + @" group by  a.SRCDOCNO, a.seq, a.mmcode, a.appqty, A.APLYITEM_NOTE,
                                A.GTAPL_RESON, C.SENDAPVTIME, B.DISC_UPRICE, B.MMNAME_C, B.MMNAME_E, B.BASE_UNIT,
                                B.M_CONTPRICE, A.FRWH_D, A.M_AGENNO, A.APL_CONTIME, A.CREATE_USER , A.S_INV_QTY,
                                A.INV_QTY, A.OPER_QTY, A.CHINNAME, A.CHARTNO, A.M_CONTID, B.APPQTY_TIMES, B.UNITRATE
                 ) P ";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCD>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<ME_DOCM> GetM(string id, string hospCode = "")
        {
            string addCol = "";
            // 若為桃園則增加顯示庫備/非庫備
            if (hospCode == "804")
                addCol = @", (
                            select DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'M_STOREID' and DATA_VALUE = A.M_STOREID
                        ) as M_STOREID ";

            var sql = @"SELECT A.*,
                        (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='FLOWID_MR1' AND DATA_VALUE=A.FLOWID) FLOWID_N,
                        (SELECT MAT_CLSNAME FROM MI_MATCLASS WHERE MAT_CLASS=A.MAT_CLASS) MAT_CLASS_N,
                        (SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.FRWH) FRWH_N,
                        (SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.TOWH) TOWH_N,
                        (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='APPLY_KIND' AND DATA_VALUE=A.APPLY_KIND) APPLY_KIND_N,
                        (SELECT UNA FROM UR_ID WHERE TUSER=A.APPID) APP_NAME,
                        (SELECT INID_NAME FROM UR_INID WHERE INID=A.APPDEPT) APPDEPT_NAME,
                        TWN_DATE(A.APPTIME) APPTIME_T,
                        (SELECT EXT FROM UR_ID WHERE TUSER=A.APPID) EXT,
                        (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='ISARMY' AND DATA_VALUE=A.ISARMY) ISARMY_N,
                        A.M_CONTID, (SELECT DISTINCT RTrim(DATA_DESC) FROM PARAM_D WHERE GRP_CODE='MI_MAST' and DATA_NAME='M_CONTID' AND DATA_VALUE = A.M_CONTID) as M_CONTID_T
                        " + addCol + @"
                        FROM ME_DOCM A
                        WHERE A.DOCNO = :DOCNO";

            return DBWork.Connection.Query<ME_DOCM>(sql, new { DOCNO = id }, DBWork.Transaction);
        }
        public IEnumerable<ME_DOCD> GetD(string id, string seq, string hospCode)
        {
            string addCol1 = "";
            string addCol2 = "";
            // 若為桃園則增加顯示庫備/非庫備
            if (hospCode == "804")
            {
                addCol1 = @", (
                            select DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'M_STOREID' and DATA_VALUE = P.M_STOREID
                        ) as M_STOREID_T ";
                addCol2 = @", (
                            select M_STOREID from MI_MAST where MMCODE = A.MMCODE
                        ) as M_STOREID ";
            }

            var sql = @"SELECT P.*,
                        NVL(P.APPQTY * P.DISC_UPRICE,0) AS APP_AMT
                        " + addCol1 + @"
                        FROM (
                        SELECT A.SRCDOCNO AS DOCNO, A.SEQ, A.MMCODE , A.APPQTY , A.APLYITEM_NOTE, 
                        A.GTAPL_RESON, SUM(A.ACKQTY) AS ACKQTY, C.SENDAPVTIME, MAX(A.APVTIME) AS APVTIME,B.DISC_UPRICE,
                        B.MMNAME_C, B.MMNAME_E, B.BASE_UNIT, B.M_CONTPRICE, SUM(NVL(A.APVQTY,0)) APVQTY, A.FRWH_D,
                        twn_date(A.APL_CONTIME) as APL_CONTIME, A.M_AGENNO,
                        A.CREATE_USER, USER_NAME(A.CREATE_USER) AS CREATE_USER_NAME, A.S_INV_QTY,A.INV_QTY, A.OPER_QTY as HIGH_QTY, A.CHINNAME, A.CHARTNO, A.M_CONTID,
                        (SELECT DISTINCT RTrim(DATA_DESC) FROM PARAM_D WHERE GRP_CODE='MI_MAST' and DATA_NAME='M_CONTID' AND DATA_VALUE = A.M_CONTID) as M_CONTID_T,
                        B.APPQTY_TIMES, B.UNITRATE,A.CHINNAME as CHINNAME_OLD, A.CHARTNO as CHARTNO_OLD
                        " + addCol2 + @"
                        FROM ME_DOCD A 
                        INNER JOIN MI_MAST B ON B.MMCODE=A.MMCODE 
                        INNER JOIN ME_DOCM C ON C.DOCNO=A.SRCDOCNO 
                        WHERE 1=1 
                        AND A.DOCNO = :DOCNO AND A.SEQ= :SEQ 
                        group by  a.SRCDOCNO, a.seq, a.mmcode, a.appqty, A.APLYITEM_NOTE,
                                A.GTAPL_RESON, C.SENDAPVTIME,B.DISC_UPRICE,B.MMNAME_C, B.MMNAME_E, B.BASE_UNIT, 
                                A.FRWH_D, A.APL_CONTIME, A.M_AGENNO, B.M_CONTPRICE, A.CREATE_USER, S_INV_QTY,INV_QTY, OPER_QTY, 
                                A.CHINNAME, A.CHARTNO, A.M_CONTID, B.APPQTY_TIMES, B.UNITRATE
                        ) P";

            return DBWork.Connection.Query<ME_DOCD>(sql, new { DOCNO = id, SEQ = seq }, DBWork.Transaction);
        }

        public int CreateM(ME_DOCM ME_DOCM, bool procMstoreid = false)
        {
            string sql = "";

            if (procMstoreid == true) // 增加M_STOREID的處理
            {
                sql = @"INSERT INTO ME_DOCM (
                        DOCNO, DOCTYPE, FLOWID , APPID , APPDEPT , 
                        APPTIME , USEID , USEDEPT , FRWH , TOWH , 
                        APPLY_KIND ,APPLY_NOTE ,MAT_CLASS, SRCDOCNO, ISARMY, APPUNA, M_CONTID, M_STOREID,
                        CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                      VALUES (
                        :DOCNO, :DOCTYPE, :FLOWID , :APPID , :APPDEPT , 
                        SYSDATE , :USEID , :USEDEPT , :FRWH , :TOWH , 
                        :APPLY_KIND , :APPLY_NOTE ,:MAT_CLASS, :SRCDOCNO, :ISARMY, :APPUNA, :M_CONTID, :M_STOREID,
                        SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            }
            else
            {
                sql = @"INSERT INTO ME_DOCM (
                        DOCNO, DOCTYPE, FLOWID , APPID , APPDEPT , 
                        APPTIME , USEID , USEDEPT , FRWH , TOWH , 
                        APPLY_KIND ,APPLY_NOTE ,MAT_CLASS, SRCDOCNO, ISARMY, APPUNA, M_CONTID,
                        CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                      VALUES (
                        :DOCNO, :DOCTYPE, :FLOWID , :APPID , :APPDEPT , 
                        SYSDATE , :USEID , :USEDEPT , :FRWH , :TOWH , 
                        :APPLY_KIND , :APPLY_NOTE ,:MAT_CLASS, :SRCDOCNO, :ISARMY, :APPUNA, :M_CONTID,
                        SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            }

            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);
        }

        public int CreateD(ME_DOCD ME_DOCD)
        {
            var sql = @"INSERT INTO ME_DOCD (
                        DOCNO, SEQ, MMCODE , APPQTY, FRWH_D, AVG_PRICE, APLYITEM_NOTE, GTAPL_RESON,
                        M_CONTPRICE, UPRICE, DISC_CPRICE, DISC_UPRICE, M_NHIKEY, M_AGENNO,
                        SRCDOCNO, S_INV_QTY, INV_QTY, OPER_QTY, CHINNAME, CHARTNO, M_CONTID,
                        CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                        SELECT  :DOCNO, :SEQ, :MMCODE, :APPQTY, :FRWH_D, NVL((SELECT NVL(AVG_PRICE,0) FROM MI_WHCOST WHERE MMCODE = :MMCODE AND DATA_YM=CUR_SETYM AND ROWNUM=1),0) AS AVG_PRICE,
                                :APLYITEM_NOTE, :GTAPL_RESON, M_CONTPRICE, UPRICE, DISC_CPRICE, 
                                DISC_UPRICE, M_NHIKEY, M_AGENNO, :SRCDOCNO, :S_INV_QTY, :INV_QTY, :HIGH_QTY,:CHINNAME,:CHARTNO, M_CONTID,
                                SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP
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
                        S_INV_QTY=:S_INV_QTY, OPER_QTY=:HIGH_QTY, CHINNAME=:CHINNAME, CHARTNO=:CHARTNO, 
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
                        :NEWDOC, DOCTYPE, FLOWID , APPID , TOWH , 
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
        public IEnumerable<ME_DOCD> GetPackD(string DOCNO, string DOCNO2, string MAT_CLASS, string APPDEPT, string ISCONTID3, int page_index, int page_size, string sorters)
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
                           and b.m_applyid <> 'E'
                           and b.cancel_id = 'N' ";
            if (DOCNO == "" && DOCNO2 == "")
            {
                sql += sBr + " AND 1=2 ";
            }
            if (DOCNO != "")
            {
                sql += sBr + " AND A.DOCNO LIKE :p0 ";
            }
            if (DOCNO2 != "")
            {
                sql += sBr + " AND A.DOCNO LIKE :p1 ";
            }
            if (MAT_CLASS != "")
            {
                sql += sBr + " AND C.MAT_CLASS = :p2 ";
            }
            if (APPDEPT != "")
            {
                sql += sBr + " AND C.APPDEPT = :p3 ";
            }
            if (ISCONTID3 == "Y")
            {
                sql += sBr + " AND B.M_CONTID = '3' ";
            }
            else
            {
                sql += sBr + " AND B.M_CONTID <> '3' ";
            }

            p.Add(":p0", string.Format("%{0}%", DOCNO));
            p.Add(":p1", string.Format("%{0}%", DOCNO2));
            p.Add(":p2", string.Format("{0}", MAT_CLASS));
            p.Add(":p3", string.Format("{0}", APPDEPT));
            p.Add(":p4", string.Format("{0}", ISCONTID3));
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
                        CEIL((D.SAFE_QTY-A.INV_QTY)/NVL(C.APPQTY_TIMES,1))*NVL(C.APPQTY_TIMES,1) APPQTY,
                        C.MMNAME_C, C.MMNAME_E, C.BASE_UNIT,C.M_CONTPRICE,C.DISC_CPRICE as AVG_PRICE,
                        (case when NVL((select 'Y' from MI_WHMAST where WH_NO=A.WH_NO and WH_NAME like '%供應中心%'),'N')='Y'
                              then D.SUPPLY_MAX_APPQTY else D.G34_MAX_APPQTY end) as MAX_APPQTY,
                        B.HIGH_QTY,A.INV_QTY,
                        (case
                                    when ((select 1 from dual where not exists (select 1 from MI_WHMM where mmcode = a.mmcode)
                                              union
                                           select 1 from MI_WHMM where wh_no = a.wh_no and mmcode = a.mmcode
                                          )) = '1' 
                                    then 'Y' else 'N'
                                 end) as whmm_valid
                        FROM MI_WHINV A,MI_WINVCTL B, MI_MAST C, MI_BASERO_14 D
                        WHERE A.WH_NO=B.WH_NO AND A.MMCODE=B.MMCODE AND A.MMCODE=C.MMCODE AND C.MMCODE=D.MMCODE 
                        AND D.RO_WHTYPE=(case when NVL((select 'Y' from MI_WHMAST where WH_NO=A.WH_NO and WH_NAME like '%供應中心%'),'N')='Y' then '3' else '1' end)
                        AND A.INV_QTY<D.SAFE_QTY 
                       -- AND C.M_STOREID = '1'
                        AND C.M_CONTID <> '3'
                        AND C.M_APPLYID <> 'E' 
                        AND C.CANCEL_ID <> 'Y' ";

            if (MAT_CLASS != "")
            {
                sql += sBr + " AND C.MAT_CLASS = :p2 ";

            }
            if (APPDEPT != "")
            {
                sql += sBr + " AND A.WH_NO = :p3 ";
            }

            sql += @" ) P WHERE 1=1 and (P.APPQTY+P.INV_QTY)<P.MAX_APPQTY ";  //請領量+現有庫存不可超過庫房設定的單位請領量

            p.Add(":p2", string.Format("{0}", MAT_CLASS));
            p.Add(":p3", string.Format("{0}", APPDEPT));
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCD>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public bool CheckMmcode(string id, string doc)
        {
            string sql = @"SELECT 1 FROM MI_MAST WHERE MMCODE=:MMCODE
                            -- AND M_STOREID = '1'
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
        } // 
        public bool CheckExists_SEC_USEMM(HIS14_SUPDET v)
        {
            string sql = @"";
            sql += sBr + "select 1 from SEC_USEMM ";
            sql += sBr + "where 1=1 ";
            sql += sBr + "and SECTIONNO=:sectionno -- 畫面輸入科別代碼 ";
            sql += sBr + "and MMCODE=:mmcode -- 畫面輸入院內碼 ";

            return !(DBWork.Connection.ExecuteScalar(sql, v, DBWork.Transaction) == null);
        } // 
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

        public bool CheckExistsM(string id, string flowid)
        {
            string sql = @"SELECT 1 FROM ME_DOCM WHERE DOCNO=:DOCNO AND FLOWID <> :FLOWID";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id, FLOWID = flowid }, DBWork.Transaction) == null);
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
        public bool CheckExistsMM(string id, string mm, string chinname, string chartno)
        {
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO AND MMCODE=:MMCODE";

            if (chinname == "" || chinname == null)
            {
                sql += " and trim(CHINNAME) is null";
            }
            else
            {
                sql += " and CHINNAME=:CHINNAME";
            }
            if (chartno == "" || chartno == null)
            {
                sql += " and trim(CHARTNO) is null";
            }
            else
            {
                sql += " and CHARTNO=:CHARTNO";
            }
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id, MMCODE = mm, CHINNAME = chinname, CHARTNO = chartno }, DBWork.Transaction) == null);
        }
        public bool CheckWhapldt(string id)
        {
            string sql = @"SELECT 1 FROM MM_WHAPLDT WHERE WH_NO=:WH_NO AND TO_CHAR(APPLY_DATE,'YYYYMM') = TO_CHAR(SYSDATE,'YYYYMM') ";
            return (DBWork.Connection.ExecuteScalar(sql, new { WH_NO = id }, DBWork.Transaction) == null);
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
            // SELECT WH_NO 
            // FROM MI_WHMAST  
            // WHERE 1=1 
            // AND WH_KIND = '1' // 庫別分類(0藥品庫 1衛材庫 E能設 C通信)
            // AND WH_GRADE = '1' //庫別級別(1庫 2局(衛星庫) 3病房 4科室 5戰備庫 M醫院軍 S學院軍)
            // and cancel_id = 'N' // 是否作廢
            // AND ROWNUM=1
            string sql = @"SELECT WH_NO 
                        FROM MI_WHMAST 
                        WHERE 1=1 AND WH_KIND = '1' AND WH_GRADE = '1'  
                          and cancel_id = 'N' AND ROWNUM=1";
            string rtn = DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction).ToString();
            return rtn;
        }
        public string GetDocDSeq(string id)
        {
            string sql = @"SELECT MAX(SEQ)+1 as SEQ FROM ME_DOCD WHERE DOCNO=:DOCNO ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction).ToString();
            return rtn;
        }

        public string GetDocMApplyKind(string id)
        {
            string sql = @"SELECT APPLY_KIND FROM ME_DOCM WHERE DOCNO=:DOCNO ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction).ToString();
            return rtn;
        }
        public string GetDocMTowh(string id)
        {
            string sql = @"SELECT TOWH FROM ME_DOCM WHERE DOCNO=:DOCNO ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction).ToString();
            return rtn;
        }
        public string GetApplyDateS(string id)
        {
            string sql = @"SELECT TWN_DATE(APPLY_DATE-7)APPLY_DATE FROM MM_WHAPLDT WHERE WH_NO=:WH_NO AND TO_CHAR(APPLY_DATE,'YYYYMM') = TO_CHAR(SYSDATE,'YYYYMM')";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { WH_NO = id }, DBWork.Transaction).ToString();
            return rtn;
        }
        public string GetApplyDateE(string id)
        {
            string sql = @"SELECT TWN_DATE(APPLY_DATE-1)APPLY_DATE FROM MM_WHAPLDT WHERE WH_NO=:WH_NO AND TO_CHAR(APPLY_DATE,'YYYYMM') = TO_CHAR(SYSDATE,'YYYYMM')";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { WH_NO = id }, DBWork.Transaction).ToString();
            return rtn;
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
        public IEnumerable<COMBO_MODEL> GetDocpknoteCombo(string p0, string p1)
        {
            string sql = @"SELECT DISTINCT DOCNO as VALUE, APPLY_NOTE as TEXT,
                        APPLY_NOTE as COMBITEM,CREATE_TIME EXTRA1
                        FROM MM_PACK_M 
                        WHERE 1=1 AND DOCTYPE = 'MR5' 
                        AND MAT_CLASS=:MAT_CLASS
                        AND APPDEPT=:APPDEPT
                        ORDER BY EXTRA1 DESC ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { MAT_CLASS = p0, APPDEPT = p1 });
        }
        public IEnumerable<COMBO_MODEL> GetFlowidCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='FLOWID_MR1' 
                          AND DATA_VALUE IN ('1','11','2','6') 
                        ORDER BY DATA_VALUE ";

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
        public IEnumerable<MI_MAST> GetMmCodeCombo(string p0, string p1, string p2, string docno, string p4, string p5, int page_index, int page_size, string sorters)
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
                                NVL((select (case when ((select wh_grade from temp) = '2' and a.mat_class='01') then phr_max_appqty else g34_max_appqty end) from MI_BASERO_14 
                                      where wh_no = ( SELECT supply_whno FROM MI_WINVCTL WHERE MMCODE = A.MMCODE AND WH_NO=C.TOWH ) 
                                        and mmcode = a.mmcode),0) AS HIGH_QTY,
                                NVL(INV_QTY(( SELECT supply_whno FROM MI_WINVCTL WHERE MMCODE = A.MMCODE AND WH_NO=C.TOWH), A.MMCODE),0) as S_INV_QTY,
                                NVL(INV_QTY(c.TOWH, a.mmcode),0) as INV_QTY,
                                (SELECT EASYNAME from PH_VENDER where AGEN_NO=A.M_AGENNO) AS M_AGENNO, A.APPQTY_TIMES, A.UNITRATE
                          FROM MI_MAST A 
                         INNER JOIN ME_DOCM C ON C.DOCNO=:DOCNO
                          LEFT OUTER JOIN MI_WINVCTL D ON D.MMCODE = A.MMCODE AND D.WH_NO IN (SELECT WH_NO FROM MI_WHMAST WHERE WH_KIND='1' AND WH_GRADE='1')
                         WHERE 1=1 
                           AND A.MAT_CLASS = :MAT_CLASS 
                           AND A.M_APPLYID <> 'E'
                           AND nvl(A.CANCEL_ID, 'N') <> 'Y' ";
            if (p5 == "Y")
            {
                sql += sBr + " AND A.M_CONTID = '3' ";
            }
            else
            {
                sql += sBr + " AND A.M_CONTID <> '3' ";
            }
            sql += sBr + " {1} ";

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
                sql += sBr + " ORDER BY MMCODE ";
            }
            p.Add(":MAT_CLASS", p1);
            p.Add(":TOWH", p2);
            p.Add(":DOCNO", docno);
            p.Add(":FRWH", p4);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMmCodeCombo_2(string p0, int page_index, int page_size, string sorters)
        {

            var p = new DynamicParameters();

            var sql = @"
                        SELECT DISTINCT {0} 
                            A.MMCODE, 
                            A.MMNAME_C, 
                            A.MMNAME_E
                        FROM MI_MAST A 
                        INNER JOIN SEC_USEMM B ON A.MMCODE = B.MMCODE
                        WHERE 1=1 
                    ";
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

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX, mmcode", sql);
            }
            else
            {
                sql = string.Format(sql, "", "");
                sql += " ORDER BY MMCODE ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMmCodeCombo_3(string p0, int page_index, int page_size, string sorters)
        {

            var p = new DynamicParameters();

            var sql = @"
                        SELECT DISTINCT {0} 
                            A.MMCODE, 
                            A.MMNAME_C, 
                            A.MMNAME_E
                        FROM MI_MAST A 
                        WHERE 1=1 
                            AND A.MAT_CLASS = '02' 
                    ";
            sql += " {1} ";
            if (p0 != "")
            {
                sql = string.Format(sql,
                     "(NVL(INSTR(UPPER(A.MMCODE), UPPER(:MMCODE_I)), 1000) + NVL(INSTR(UPPER(MMNAME_E), UPPER(:MMNAME_E_I)), 100) * 10 + NVL(INSTR(UPPER(MMNAME_C), UPPER(:MMNAME_C_I)), 100) * 10) IDX,",
                     @" AND (UPPER(A.MMCODE) LIKE UPPER(:MMCODE) OR UPPER(MMNAME_E) LIKE UPPER(:MMNAME_E) OR UPPER(MMNAME_C) LIKE UPPER(:MMNAME_C))");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);
                p.Add(":MMCODE", string.Format("{0}%", p0));
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX, mmcode", sql);
            }
            else
            {
                sql = string.Format(sql, "", "");
                sql += " ORDER BY MMCODE ";
            }

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
                                (select AGEN_NAMEE from PH_VENDER where AGEN_NO = A.M_AGENNO) as AGEN_NAMEE
                          FROM MI_MAST A 
                         WHERE 1=1 
                           AND A.MAT_CLASS = :MAT_CLASS  
                        --   AND A.M_STOREID = '1'
                           AND A.M_APPLYID <> 'E'
                           AND nvl(A.CANCEL_ID, 'N') <> 'Y' ";

            if (query.MMCODE != "")
            {
                sql += sBr + " AND UPPER(A.MMCODE) LIKE :MMCODE ";
            }
            if (query.MMNAME_C != "")
            {
                sql += sBr + " AND A.MMNAME_C LIKE :MMNAME_C ";
            }
            if (query.MMNAME_E != "")
            {
                sql += sBr + " AND UPPER(A.MMNAME_E) LIKE UPPER(:MMNAME_E) ";
            }
            if (query.ISCONTID3 == "Y")
            {
                sql += sBr + " AND A.M_CONTID = '3' ";
            }
            else
            {
                sql += sBr + " AND A.M_CONTID <> '3' ";
            }
            if (query.M_AGENNO != "")
            {
                sql += sBr + " AND A.M_AGENNO LIKE :M_AGENNO ";
            }
            if (query.AGEN_NAME != "")
            {
                sql += sBr + " AND ((select AGEN_NAMEC from PH_VENDER where AGEN_NO = A.M_AGENNO) LIKE :AGEN_NAME ";
                sql += sBr + "  OR (select AGEN_NAMEE from PH_VENDER where AGEN_NO = A.M_AGENNO) LIKE :AGEN_NAME) ";
            }

            p.Add(":WH_NO", query.WH_NO);
            p.Add(":MAT_CLASS", query.MAT_CLASS);
            p.Add(":MMCODE", string.Format("%{0}%", query.MMCODE));
            p.Add(":MMNAME_C", string.Format("%{0}%", query.MMNAME_C));
            p.Add(":MMNAME_E", string.Format("%{0}%", query.MMNAME_E));
            p.Add(":M_AGENNO", string.Format("%{0}%", query.M_AGENNO));
            p.Add(":AGEN_NAME", string.Format("%{0}%", query.AGEN_NAME));

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
                sql = string.Format(sql, "(NVL(INSTR(UPPER(A.MMCODE), UPPER(:MMCODE_I)), 1000) + NVL(INSTR(MMNAME_E, UPPER(:MMNAME_E_I)), 100) * 10 + NVL(INSTR(UPPER(MMNAME_C), UPPER(:MMNAME_C_I)), 100) * 10) IDX,");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += sBr + " AND (UPPER(A.MMCODE) LIKE UPPER(:MMCODE) ";
                p.Add(":MMCODE", string.Format("{0}%", p0));

                sql += sBr + " OR UPPER(MMNAME_E) LIKE UPPER(:MMNAME_E) ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += sBr + " OR UPPER(MMNAME_C) LIKE UPPER(:MMNAME_C)) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += sBr + " ORDER BY MMCODE ";
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
            string sql = @"select A.WH_NO VALUE , A.WH_NO||' '||B.WH_NAME TEXT ,A.WH_NO || ' ' || B.WH_NAME COMBITEM 
                            from MI_WHID A,MI_WHMAST B
                            WHERE A.WH_NO = B.WH_NO 
                              AND A.WH_USERID=:TUSER 
                              AND B.WH_KIND = '1' 
                              and b.wh_grade <> '1'
                            UNION  
                            SELECT A.WH_NO ,A.WH_NO||' '||WH_NAME,A.WH_NO || ' ' || A.WH_NAME COMBITEM 
                            FROM MI_WHMAST A
                            WHERE A.INID= USER_INID(:TUSER) 
                              AND A.WH_KIND = '1' 
                              and a.wh_grade <> '1'
                            UNION  
                            SELECT A.WH_NO ,A.WH_NO||' '||WH_NAME,A.WH_NO || ' ' || A.WH_NAME COMBITEM 
                            FROM MI_WHMAST A
                            WHERE A.WH_KIND = '1' and A.WH_GRADE = '2'
                              AND (select count(*) from UR_UIR where RLNO = 'MAT_14' and TUSER = :TUSER) > 0
                            ORDER BY 1";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { TUSER = id });
        }
        public IEnumerable<COMBO_MODEL> GetMatclassCombo()
        {
            string sql = @"SELECT MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, 
                        MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM 
                        FROM MI_MATCLASS WHERE MAT_CLASS IN ('02','03','04','05','06','07','08')
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
                        (select DATA_VALUE from PARAM_D where GRP_CODE = 'HOSP_INFO' and DATA_NAME = 'HospCode') as HOSP_CODE,
                        (case when (select count(*) from UR_UIR where RLNO in ('MAT_14') and TUSER = :TUSER) > 0 then 'Y' else 'N' end) as IS_GRADE1
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
            public string ISCONTID3;

            public string M_AGENNO;
            public string AGEN_NAME;
        }

        //匯出
        public DataTable GetExcel()
        {
            var p = new DynamicParameters();

            var sql = @" SELECT '' as 院內碼,'' as 申請數量, '' as 備註, '' as 病患姓名, '' as 病歷號  FROM DUAL ";


            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
        // 依據 DOCNO 匯出 DETAIL EXCEL
        public DataTable GetDetailExcel(string docno)
        {
            var p = new DynamicParameters();

            var sql = @"
                                    SELECT
                                        p.DOCNO AS 申請單號,
                                        p.MMCODE AS 院內碼,
                                        p.MMNAME_C AS 中文品名,
                                        p.MMNAME_E AS 英文品名,
                                        p.BASE_UNIT AS 計量單位,
                                        p.APPQTY AS 申請數量
                                    FROM
                                        (
                                            SELECT
                                                a.srcdocno               AS docno,
                                                a.seq,
                                                a.mmcode,
                                                a.appqty,
                                                a.aplyitem_note,
                                                a.gtapl_reson,
                                                SUM(a.ackqty)            AS ackqty,
                                                c.sendapvtime,
                                                MAX(a.apvtime)           AS apvtime,
                                                b.disc_uprice,
                                                b.mmname_c,
                                                b.mmname_e,
                                                b.base_unit,
                                                b.m_contprice,
                                                SUM(nvl(a.apvqty, 0))    apvqty,
                                                a.create_user,
                                                user_name(a.create_user) AS create_user_name
                                            FROM
                                                     me_docd a
                                                INNER JOIN mi_mast b ON b.mmcode = a.mmcode
                                                INNER JOIN me_docm c ON c.docno = a.srcdocno
                                            WHERE
                                                1 = 1
                                ";

            if (docno != "")
            {
                sql += sBr + " AND A.SRCDOCNO LIKE :p0 ";
                p.Add(":p0", string.Format("%{0}%", docno));
            }
            else
            {
                sql += sBr + " AND 1=2 ";
            }
            sql += sBr + @" group by  a.SRCDOCNO, a.seq, a.mmcode, a.appqty, A.APLYITEM_NOTE,
                                A.GTAPL_RESON, C.SENDAPVTIME, B.DISC_UPRICE,
                                B.MMNAME_C, B.MMNAME_E, B.BASE_UNIT,B.M_CONTPRICE, A.CREATE_USER 
                 ) P ";

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
                        --  AND M_STOREID = '1'
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

        public bool Checkappqty(string id, string qty)
        {
            string sql = @" SELECT 1 FROM MI_MAST A 
                            WHERE 1=1 AND A.MMCODE=:MMCODE 
                            AND mod(:APPQTY , NVL(A.APPQTY_TIMES ,1)) > 0";

            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = id, APPQTY = qty }, DBWork.Transaction) == null);
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

        public string GetDocAppAmout(string docno)
        {
            var sql = @" select nvl(sum((case when A.AVG_PRICE is null then NVL(( SELECT AVG_PRICE FROM MI_WHCOST WHERE MMCODE = A.MMCODE AND DATA_YM=CUR_SETYM AND ROWNUM=1 ),0) else A.AVG_PRICE end) * A.APPQTY), 0) as APP_AMOUT 
                            from ME_DOCD A where A.DOCNO = :DOCNO ";

            return DBWork.Connection.ExecuteScalar(sql, new { DOCNO = docno }, DBWork.Transaction).ToString();
        }

        #region 2020-05-20 新增: 送核撥時檢核是否ME_DOCD有院內碼重複
        public bool CheckDuplicateMmcode(string docno)
        {
            string sql = @"select 1 from ME_DOCD
                            where docno = :docno
                            group by mmcode, chinname,chartno
                           having count(*) > 1";

            return !(DBWork.Connection.ExecuteScalar(sql, new { docno = docno }, DBWork.Transaction) == null);
        }
        #endregion 

        #region 2020-08-15 新增: 送核撥時檢核是否ME_DOCD品項的M_APPLYID ='E'
        public IEnumerable<ME_DOCD> CheckMApplyidE(string docno, bool isView)
        {
            string sql = string.Format(@"
                           select * from ME_DOCD a
                            where docno = :docno
                              and exists (select 1 from {0}MI_MAST b,ME_DOCM c 
                                            where b.mmcode = a.mmcode and c.docno = a.docno
                                                and (b.m_applyid = 'E' or b.cancel_id = 'Y') 
                                                and ( ( c.ISCONTID3 = 'Y' AND b.M_CONTID='3' ) or ( c.ISCONTID3 = 'N' AND b.M_CONTID <> '3' ) ) )"
                        , isView ? "V_" : string.Empty);

            return DBWork.Connection.Query<ME_DOCD>(sql, new { docno = docno }, DBWork.Transaction);
        }
        #endregion

        #region 2021-02-20新增: 送核撥時檢核是否符合MI_WHMM規則
        public IEnumerable<ME_DOCD> CheckApplyWHMM(string docno)
        {
            string sql = string.Format(@"
                            select * from ME_DOCD a
                             where docno = :docno
                               and mmcode not in (
                                   select a.mmcode from dual 
                                    where not exists (select 1 from MI_WHMM where mmcode = a.mmcode)
                                   union  
                                   select mmcode 
                                     from MI_WHMM 
                                    where wh_no = (select towh from ME_DOCM where docno = a.docno)
                                      and mmcode = a.mmcode
                               )
        ");

            return DBWork.Connection.Query<ME_DOCD>(sql, new { docno = docno }, DBWork.Transaction);
        }
        #endregion

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
            // 依庫房代碼(WH_NO)查庫房基本檔 是否申請庫房已作廢 select 1 from MI_WHMAST where cancel_id = 'N' and wh_no = :towh 
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

        public IEnumerable<AB0118Report_MODEL> GetPrintData(string[] str_DOCNO, string UserName, string order)
        {
            var p = new DynamicParameters();

            string sql = @" SELECT A.DOCNO, A.APPID, A.APPDEPT, B.MMCODE, B.APPQTY, B.APVQTY, 
                        ((case when trim( B.M_NHIKEY) is not null then '(健保碼:'||B.M_NHIKEY||');' else null end)||  B.APLYITEM_NOTE) as APLYITEM_NOTE,
                         C.MMNAME_C, C.MMNAME_E, C.BASE_UNIT
                        ,(SELECT RTrim(WH_NO || ' ' || WH_NAME) FROM MI_WHMAST WHERE WH_NO = A.TOWH) AS APPDEPT_N                                              
                        ,(SELECT UNA FROM UR_ID WHERE A.APPID = TUSER) AS APPID_N
                        ,(SELECT WH_NAME FROM MI_WHMAST WHERE WH_NO = A.FRWH) AS FRWH_N 
                        ,A.APPLY_NOTE
                        ,NVL(( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = B.MMCODE AND WH_NO=A.FRWH ),0) AS FRWHINV_QTY
                        ,NVL(( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = B.MMCODE AND WH_NO=A.TOWH ),0) AS TOWHINV_QTY
                        ,(SELECT DISTINCT RTrim(DATA_DESC) FROM PARAM_D WHERE GRP_CODE='ME_DOCM' and DATA_NAME='ISARMY' AND DATA_VALUE = A.ISARMY) as ISARMY_N
                        ,TWN_TIME(A.APPTIME) AS APPTIME_T, NVL(B.DISC_CPRICE,0) AS DISC_CPRICE
                        ,(SELECT EASYNAME from PH_VENDER where AGEN_NO=B.M_AGENNO) AS M_AGENNO
                        ,(SELECT DISTINCT RTrim(DATA_DESC) FROM PARAM_D WHERE GRP_CODE='MI_MAST' and DATA_NAME='M_CONTID' AND DATA_VALUE = C.M_CONTID) as M_CONTID_T
                        ,C.E_ITEMARMYNO,TWN_DATE(C.E_CODATE) AS E_CODATE, C.CASENO
                        ,(case when (a.doctype in ('MR1','MR2','MR5','MR6') and (select wh_grade from MI_WHMAST where wh_no = a.frwh)='1' )
                                then (select g34_max_appqty from MI_BASERO_14 where ro_whtype='1' and mmcode = b.mmcode)
                               when (a.doctype in ('MR') and (select wh_grade from MI_WHMAST where wh_no = a.frwh)='1')
                                then (select phr_max_appqty from MI_BASERO_14 where ro_whtype='1' and mmcode = b.mmcode)
                               when (a.doctype in ('MR1','MR2','MR5','MR6') and (select wh_grade from MI_WHMAST where wh_no = a.frwh) <> '1')
                                then (select g34_max_appqty from MI_BASERO_14 where ro_whtype='3' and mmcode=b.mmcode)
                          end) as max_qty
                        FROM ME_DOCM A, ME_DOCD B, MI_MAST C
                        WHERE A.DOCNO = B.DOCNO AND B.MMCODE = C.MMCODE ";

            p.Add(":PRINT_USER", UserName);

            //判斷DOCNO查詢條件是否有值
            if (str_DOCNO.Length > 0)
            {
                sql += sBr + @" AND A.DOCNO IN :DOCNO ";
                p.Add("DOCNO", str_DOCNO);
            }
            sql += sBr + " ORDER BY  A.DOCNO ";

            //先將查詢結果暫存在tmp_AB0118Report_MODEL，接著產生BarCode的資料
            IEnumerable<AB0118Report_MODEL> tmp_AB0118Report_MODEL = DBWork.Connection.Query<AB0118Report_MODEL>(sql, p);

            //return DBWork.Connection.Query<AB0118Report_MODEL>(sql, p);
            return tmp_AB0118Report_MODEL;
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
                        select (case when  ((select wh_grade from temp) = '2' and (select 1 from MI_MAST where mmcode = :MMCODE and mat_class='01')='1')
                                     then phr_max_appqty else g34_max_appqty 
                                end) 
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
        // -- flylon
        public IEnumerable<COMBO_MODEL> GetCombo科別(string id)
        {
            //string sql = @"";
            //sql += sBr + "select distinct  ";
            //sql += sBr + "SECTIONNO as VALUE, -- 科別代碼 ";
            //sql += sBr + "( ";
            //sql += sBr + "    select SECTIONNAME from SEC_MAST -- 科別代碼主檔 ";
            //sql += sBr + "    where SECTIONNO=a.SECTIONNO ";
            //sql += sBr + ") as COMBITEM, -- SECTIONNAME ";
            //sql += sBr + "'' endl ";
            //sql += sBr + "from SEC_USEMM a -- 科別耗用品項設定檔 ";
            //sql += sBr + "order by SECTIONNO ";

            string sql = @"
                SELECT DISTINCT
                    A.SECTIONNO AS VALUE, -- 科別代碼
                    A.SECTIONNO || ' ' || A.SECTIONNAME AS COMBITEM -- 科別名稱
                FROM 
                    SEC_MAST A
                INNER JOIN SEC_USEMM B ON A.SECTIONNO = B.SECTIONNO
                WHERE 1 = 1 
                    AND A.SEC_ENABLE='Y'
                ORDER BY 
                    A.SECTIONNO
            ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { TUSER = id });
        } // 

        public IEnumerable<COMBO_MODEL> GetCombo科別_2(string id)
        {
            //string sql = @"";
            //sql += sBr + "select distinct  ";
            //sql += sBr + "SECTIONNO as VALUE, -- 科別代碼 ";
            //sql += sBr + "( ";
            //sql += sBr + "    select SECTIONNAME from SEC_MAST -- 科別代碼主檔 ";
            //sql += sBr + "    where SECTIONNO=a.SECTIONNO ";
            //sql += sBr + ") as COMBITEM, -- SECTIONNAME ";
            //sql += sBr + "'' endl ";
            //sql += sBr + "from SEC_USEMM a -- 科別耗用品項設定檔 ";
            //sql += sBr + "order by SECTIONNO ";

            string sql = @"
                SELECT DISTINCT
                    SECTIONNO AS VALUE, -- 科別代碼
                    SECTIONNO || ' ' || SECTIONNAME AS COMBITEM -- 科別名稱
                FROM 
                    SEC_MAST
                WHERE 1 = 1 
                    AND SEC_ENABLE='Y'
                ORDER BY 
                    SECTIONNO
            ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { TUSER = id });
        } // 

        public IEnumerable<COMBO_MODEL> GetCombo院內碼(string id)
        {
            string sql = @"";
            sql += sBr + "select  ";
            sql += sBr + "distinct mmcode VALUE, ";
            sql += sBr + "case ";
            sql += sBr + "    when length(mmname_c)>20 then ";
            sql += sBr + "        substr(mmname_c,1,20) || '(' || mmcode || ')'   ";
            sql += sBr + "    else ";
            sql += sBr + "        mmname_c || '(' || mmcode || ')' ";
            sql += sBr + "end COMBITEM ";
            sql += sBr + "from MI_MAST ";
            sql += sBr + "where 1=1  ";
            sql += sBr + "and mat_class='02' -- 物料分類代碼[01藥品|02衛材(含檢材)|03文具|04清潔用品|05表格|06防護用具|07被服|08資訊耗材|09氣體|13中藥] ";
            sql += sBr + "and rownum<4000 -- 測試後要拿掉此行 ";
            sql += sBr + "order by COMBITEM ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { TUSER = id });
        } // 

        public IEnumerable<HIS14_SUPDET> AllT7Grid(
            HIS14_SUPDET q,
            string INID,
            int page_index,
            int page_size,
            string sorters
        )
        {
            var p = new DynamicParameters();

            //var sql = @"";
            //sql += sBr + "select * from  ";
            //sql += sBr + "( ";
            //sql += sBr + "    select  "; 
            //sql += sBr + "    SUP_SKDIACODE as mmcode, ";
            //sql += sBr + "    ( ";
            //sql += sBr + "        select MMNAME_C  ";
            //sql += sBr + "        from mi_mast where 1=1  -- 藥衛材基本檔 ";
            //sql += sBr + "        and mmcode=a.SUP_SKDIACODE ";
            //sql += sBr + "    ) as mmname_c, ";
            //sql += sBr + "    SUP_USEQTY as useqty,  ";
            //sql += sBr + "    SUP_USEQTY,  ";
            //sql += sBr + "    SUP_UNIT, ";
            //sql += sBr + "    SUP_PATIDNO as IdNo,  ";
            //sql += sBr + "    SUP_PATNAME as PatientName, ";
            //sql += sBr + "    SUP_PATNAME, ";
            //sql += sBr + "    SUP_MEDNO, ";
            //sql += sBr + "    SUP_SECTNO as SectionNo, ";
            //sql += sBr + "    ( ";
            //sql += sBr + "        select SECTIONNAME  ";
            //sql += sBr + "        from SEC_MAST -- 科別代碼主檔 ";
            //sql += sBr + "        where 1=1 ";
            //sql += sBr + "        and SECTIONNO=a.SUP_SECTNO  ";
            //sql += sBr + "        and sec_enable='Y' ";
            //sql += sBr + "    ) as SectionName, ";
            //sql += sBr + "    SUP_FEATOPID as DrId,  ";
            //sql += sBr + "    SUP_EMPNAME as DrName, ";
            //sql += sBr + "    SUP_USEDATE as UseDate,  ";
            //sql += sBr + "    SUP_PROCDATE as ProcDate,  ";
            //sql += sBr + "    DOCNO, ";
            //sql += sBr + "    SUPDET_SEQ, ";
            //sql += sBr + "    SUP_SKDIACODE, -- 院內碼MMCODE ";
            //sql += sBr + "    '' ENDL ";
            //sql += sBr + "    from HIS14_SUPDET a, -- HIS骨科手術衛材資料 ";
            //sql += sBr + "         SEC_USEMM b -- 科別耗用品項設定檔 ";
            //sql += sBr + "    where 1=1 ";
            //sql += sBr + "    and rownum<10 ";
            //sql += sBr + "    and a.SUP_SECTNO=b.SECTIONNO ";
            //sql += sBr + "    and a.SUP_SKDIACODE=b.MMCODE ";

            string sql = @"
                select * from 
                (
                select  
                SUP_SKDIACODE as mmcode,
                (
                    select MMNAME_C 
                    from mi_mast where 1=1  -- 藥衛材基本檔
                    and mmcode=a.SUP_SKDIACODE
                ) as mmname_c,
                SUP_USEQTY as useqty, 
                SUP_USEQTY, 
                SUP_UNIT,
                SUP_PATIDNO as IdNo, 
                SUP_PATNAME as PatientName,
                SUP_PATNAME,
                SUP_MEDNO,
                SUP_SECTNO as SectionNo,
                (
                    select SECTIONNAME 
                    from SEC_MAST -- 科別代碼主檔
                    where 1=1
                    and SECTIONNO=a.SUP_SECTNO 
                    and sec_enable='Y'
                ) as SectionName,
                SUP_FEATOPID as DrId, 
                SUP_EMPNAME as DrName,
                SUP_USEDATE as UseDate, 
                SUP_PROCDATE as ProcDate, 
                DOCNO,
                SUPDET_SEQ,
                SUP_SKDIACODE -- 院內碼MMCODE
                from HIS14_SUPDET a, -- HIS骨科手術衛材資料
                     SEC_USEMM b -- 科別耗用品項設定檔
                where 1=1
                and rownum<10
                and a.SUP_SECTNO=b.SECTIONNO
                and a.SUP_SKDIACODE=b.MMCODE
";
            if (!String.IsNullOrEmpty(q.DOCNO))
            {
                sql += sBr + "    and a.DOCNO=:docno ";
                p.Add(":docno", string.Format("{0}", q.DOCNO));
            }
            if (!String.IsNullOrEmpty(q.SUP_USEDATE_S))
            {
                sql += sBr + "       and SUP_USEDATE>=:sup_usedate_s -- twn_date(查詢條件消耗日期區間起值) ";
                p.Add(":sup_usedate_s", string.Format("{0}", q.SUP_USEDATE_S));
            }
            if (!String.IsNullOrEmpty(q.SUP_USEDATE_E))
            {
                sql += sBr + "       and SUP_USEDATE<=:sup_usedate_e -- 查詢條件消耗日期區間起值 ";
                p.Add(":sup_usedate_e", string.Format("{0}", q.SUP_USEDATE_E));
            }
            if (!String.IsNullOrEmpty(q.SUP_PATIDNO_OR_SUP_PATNAME))
            {
                sql += sBr + "       and SUP_PATIDNO || SUP_PATNAME like UPPER(:sup_patidno_or_sup_patname) -- '%查詢條件病患證號或姓名%' ";
                p.Add(":sup_patidno_or_sup_patname", string.Format("%{0}%", q.SUP_PATIDNO_OR_SUP_PATNAME.ToUpper()));
            }
            if (!String.IsNullOrEmpty(q.SUP_FEATOPID_OR_SUP_EMPNAME))
            {
                sql += sBr + "       and SUP_FEATOPID || SUP_EMPNAME like UPPER(:sup_featopid_or_sup_empname) -- '%查詢條件醫師代碼或姓名%' ";
                p.Add(":sup_featopid_or_sup_empname", string.Format("%{0}%", q.SUP_FEATOPID_OR_SUP_EMPNAME.ToUpper()));
            }
            sql += sBr + ") A ";
            sql += sBr + "where 1=1 ";
            if (!String.IsNullOrEmpty(q.MMCODE_OR_MMNAME_C))
            {
                sql += sBr + "       and MMCODE || MMNAME_C like :mmcode_or_mmname_c -- '%查詢條件院內碼或中文品名%' ";
                p.Add(":mmcode_or_mmname_c", string.Format("%{0}%", q.MMCODE_OR_MMNAME_C));
            }
            sql += sBr + " ";

            sql += sBr + "order by SectionNo, mmcode ";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<HIS14_SUPDET>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        } // end of AllT7Grid

        public IEnumerable<HIS14_SUPDET> AllT8Grid(
            string sectionno,
            string mmcode,
            int page_index,
            int page_size,
            string sorters
        )
        {
            var p = new DynamicParameters();

            var sql = @"";
            sql += sBr + "select  ";
            sql += sBr + "SECTIONNO, ";
            sql += sBr + "( ";
            sql += sBr + "	select SECTIONNAME  ";
            sql += sBr + "	from SEC_MAST -- 科別代碼主檔 ";
            sql += sBr + "	where SECTIONNO=a.SECTIONNO ";
            sql += sBr + ") as SECTIONNAME, ";
            sql += sBr + "mmcode, ";
            sql += sBr + "( ";
            sql += sBr + "	select mmname_c  ";
            sql += sBr + "	from MI_MAST -- 藥衛材基本檔 ";
            sql += sBr + "	where mmcode=a.mmcode ";
            sql += sBr + ") as mmname_c, ";
            sql += sBr + "( ";
            sql += sBr + "	select mmname_e  ";
            sql += sBr + "	from MI_MAST -- 藥衛材基本檔 ";
            sql += sBr + "	where mmcode=a.mmcode ";
            sql += sBr + ") as MMNAME_E ";
            sql += sBr + "from SEC_USEMM a -- 科別耗用品項設定檔 ";
            sql += sBr + "where 1=1 ";
            //if (!String.IsNullOrEmpty(chk_no))
            //{
            //    sql += sBr + "and a.chk_no = :chk_no  ";
            //    p.Add(":chk_no", string.Format("{0}", chk_no));
            //}
            if (!String.IsNullOrEmpty(sectionno))
            {
                sql += sBr + "and a.SECTIONNO = :sectionno  ";
                p.Add(":sectionno", string.Format("{0}", sectionno));
            }
            if (!String.IsNullOrEmpty(mmcode))
            {
                sql += sBr + "and a.MMCODE = :mmcode  ";
                p.Add(":mmcode", string.Format("{0}", mmcode));
            }
            //sql += sBr + "order by SECTIONNO, MMCODE -- 科別代碼, 院內碼 ";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<HIS14_SUPDET>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        } // end of AllT8Grid

        public int Create_SEC_USEMM(HIS14_SUPDET v)
        {
            var sql = @"";
            sql += sBr + "";

            sql += sBr + "insert into SEC_USEMM ( ";
            sql += sBr + "    SECTIONNO, MMCODE, CREATE_TIME, CREATE_USER, UPDATE_IP ";
            sql += sBr + ") values ( ";
            sql += sBr + "    '" + v.SECTIONNO + "', -- 畫面輸入科別代碼 ";
            sql += sBr + "    '" + v.MMCODE + "', -- 畫面輸入院內碼 ";
            sql += sBr + "    sysdate, -- CREATE_TIME";
            sql += sBr + "    '" + v.CREATE_USER + "', -- CREATE_USER ";
            sql += sBr + "    '" + v.UPDATE_IP + "' -- UPDATE_IP ";
            sql += sBr + ") ";

            return DBWork.Connection.Execute(sql, v, DBWork.Transaction);
        }
        public int Update_HIS14_SUPDET_DOCNO(string supdet_seq, string docno, string update_user, string update_ip)
        {
            var sql = @"update HIS14_SUPDET set 
                               DOCNO=:DOCNO, UPDATE_TIME=sysdate, UPDATE_USER=:UPDATE_USER, UPDATE_IP=:UPDATE_IP
                         where SUPDET_SEQ = :SUPDET_SEQ";

            return DBWork.Connection.Execute(sql, new { SUPDET_SEQ = supdet_seq, DOCNO = docno, UPDATE_USER = update_user, UPDATE_IP = update_ip }, DBWork.Transaction);
        }

        public IEnumerable<HIS14_SUPDET> Select_HIS14_SUPDET_DOCNO(string update_user)
        {
            var p = new DynamicParameters();

            var sql = @"select SUPDET_SEQ,SUP_SKDIACODE, SUP_USEQTY, SUP_PATNAME, SUP_MEDNO
                          from HIS14_SUPDET where DOCNO = :DOCNO";

            p.Add(":docno", string.Format("{0}", update_user));

            return DBWork.Connection.Query<HIS14_SUPDET>(sql, p, DBWork.Transaction);
        } // 
        // -- flylon end 

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

        public IEnumerable<ME_DOCD> GetSplitValue(string docno, bool procMstoreid)
        {
            string sql = "";
            if (procMstoreid == true) // 增加M_STOREID的處理
                sql = @"select distinct FRWH_D, M_CONTID, 
                        (select M_STOREID from MI_MAST where MMCODE = A.MMCODE) as M_STOREID from me_docd A where DOCNO=:DOCNO";
            else
                sql = @"select distinct FRWH_D, M_CONTID from me_docd where DOCNO=:DOCNO";
            return DBWork.Connection.Query<ME_DOCD>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public int DetailUpdateDocno(string old_docno, string frwh, string mcondit, string new_docno, string update_user, string update_ip)
        {
            var sql = @"UPDATE ME_DOCD SET DOCNO=:NEW_DOCNO, SRCDOCNO=:NEW_DOCNO, 
                           UPDATE_TIME=SYSDATE, UPDATE_USER=:UPDATE_USER, UPDATE_IP=:UPDATE_IP 
                         WHERE DOCNO=:DOCNO AND FRWH_D=:FRWH_D and M_CONTID=:M_CONTID";
            return DBWork.Connection.Execute(sql, new { DOCNO = old_docno, FRWH_D = frwh, M_CONTID = mcondit, NEW_DOCNO = new_docno, UPDATE_USER = update_user, UPDATE_IP = update_ip }, DBWork.Transaction);
        }

        public int DetailUpdateDocno(string old_docno, string frwh, string mcondit, string mstoreid, string new_docno, string update_user, string update_ip)
        {
            var sql = @"UPDATE ME_DOCD A SET DOCNO=:NEW_DOCNO, SRCDOCNO=:NEW_DOCNO, 
                           UPDATE_TIME=SYSDATE, UPDATE_USER=:UPDATE_USER, UPDATE_IP=:UPDATE_IP 
                         WHERE DOCNO=:DOCNO AND FRWH_D=:FRWH_D and M_CONTID=:M_CONTID 
                            and (select M_STOREID from MI_MAST where MMCODE = A.MMCODE) = :M_STOREID ";
            return DBWork.Connection.Execute(sql, new { DOCNO = old_docno, FRWH_D = frwh, M_CONTID = mcondit, M_STOREID = mstoreid, NEW_DOCNO = new_docno, UPDATE_USER = update_user, UPDATE_IP = update_ip }, DBWork.Transaction);
        }

        public int DetailUpdateDocnoBySeq(string old_docno, string seq, string new_docno, string appqty, string update_user, string update_ip)
        {
            var sql = @"UPDATE ME_DOCD SET DOCNO=:NEW_DOCNO, SRCDOCNO=:NEW_DOCNO, APPQTY = :APPQTY,
                           UPDATE_TIME=SYSDATE, UPDATE_USER=:UPDATE_USER, UPDATE_IP=:UPDATE_IP 
                         WHERE DOCNO=:DOCNO AND SEQ=:SEQ ";
            return DBWork.Connection.Execute(sql, new { DOCNO = old_docno, SEQ = seq, NEW_DOCNO = new_docno, APPQTY = appqty, UPDATE_USER = update_user, UPDATE_IP = update_ip }, DBWork.Transaction);
        }

        public int MasterUpdateFrwhMcontid(ME_DOCM me_docm, bool procMstoreid = false)
        {
            string sql = "";
            if (procMstoreid == true) // 增加M_STOREID的處理
            {
                sql = @"UPDATE ME_DOCM SET FRWH=:FRWH, M_CONTID=:M_CONTID, M_STOREID=:M_STOREID, FLOWID=:FLOWID,
                            UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP 
                         WHERE DOCNO = :DOCNO";
            }
            else
            {
                sql = @"UPDATE ME_DOCM SET FRWH=:FRWH, M_CONTID=:M_CONTID, FLOWID=:FLOWID,
                            UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP 
                         WHERE DOCNO = :DOCNO";
            }

            return DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);
        }

        public string getDocTowh(string docno)
        {
            string sql = @"SELECT TOWH from ME_DOCM where DOCNO = :DOCNO ";
            string rtn = Convert.ToString(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = docno }, DBWork.Transaction));
            return rtn;
        }

        public string getDocMatclass(string docno)
        {
            string sql = @"SELECT MAT_CLASS from ME_DOCM where DOCNO = :DOCNO ";
            string rtn = Convert.ToString(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = docno }, DBWork.Transaction));
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
        public bool GetFrwhIsSupply(string wh_no)
        {
            string sql = "select 1 from MI_WHMAST where WH_NO=:WH_NO and WH_NAME like '%供應中心%'";
            return !(DBWork.Connection.ExecuteScalar(sql, new { WH_NO = wh_no }, DBWork.Transaction) == null);
        }
        public string CheckMaxAppqtyFlag(string mmcode, string towh, string appqty, bool frwhIsSupply)
        {
            string max_appqty_field = "G34_MAX_APPQTY";
            if (frwhIsSupply == true)
            {
                max_appqty_field = "SUPPLY_MAX_APPQTY";
            }

            string sql = @"select (case when :APPQTY > " + max_appqty_field;
            sql += @" then 'Y' else 'N' end ) flag
                             from MI_BASERO_14 A
                            where A.MMCODE=:MMCODE
                              and a.wh_no = (select supply_whno from MI_WINVCTL where wh_no = :TOWH and mmcode = a.mmcode)";

            if (DBWork.Connection.ExecuteScalar(sql, new { MMCODE = mmcode, TOWH = towh, APPQTY = appqty }, DBWork.Transaction) == null)
            {
                return "Y";
            }
            else
            {
                return DBWork.Connection.ExecuteScalar(sql, new { MMCODE = mmcode, TOWH = towh, APPQTY = appqty }, DBWork.Transaction).ToString();
            }
        }
        public IEnumerable<ME_DOCD> CheckMmcodeMaxAppqtyFlag(string docno)
        {
            string sql = @"
              with temp as(
                select A.MMCODE,A.APPQTY,A.FRWH_D,
                       NVL((select INV_QTY from MI_WHINV where MMCODE=A.MMCODE and WH_NO=B.TOWH),0) as INV_QTY,
                       (select supply_whno FROM MI_WINVCTL WHERE MMCODE = A.MMCODE AND WH_NO=B.TOWH) as SUPPLY_WHNO,
                       (select WH_NAME from MI_WHMAST where WH_NO=(select supply_whno FROM MI_WINVCTL WHERE MMCODE = A.MMCODE AND WH_NO=B.TOWH)) as WH_NAME
                  from ME_DOCD A, ME_DOCM B where A.DOCNO=B.DOCNO and A.DOCNO=:docno
               )
               select C.MMCODE from temp C
                 left join MI_BASERO_14 D on (C.MMCODE=D.MMCODE and D.WH_NO =C.SUPPLY_WHNO)
                where (C.APPQTY) > nvl((case when C.WH_NAME like '%供應中心%' then D.SUPPLY_MAX_APPQTY else D.G34_MAX_APPQTY end),0)
            ";
            return DBWork.Connection.Query<ME_DOCD>(sql, new { docno }, DBWork.Transaction);
        }
        public string GetLastHIS14SUPDET()
        {
            var sql = @"select TWN_TIME(max(CREATE_TIME)) from HIS14_SUPDET";
            string rtn = DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
            return rtn;
        }

        public int Clear_Temp_HIS14_SUPDET(string docno)
        {
            var sql = @"update HIS14_SUPDET set DOCNO=null where DOCNO=:DOCNO";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno }, DBWork.Transaction);
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

        public string GetHospCode()
        {
            var sql = @"select DATA_VALUE from PARAM_D where GRP_CODE = 'HOSP_INFO' and DATA_NAME = 'HospCode'";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCD> GetDocItems(string docno, string m_contid, string m_storeid)
        {
            string addSql = "";
            if (m_storeid != "" && m_storeid != null)
            {
                addSql = " and (select M_STOREID from MI_MAST where MMCODE = A.MMCODE) = :M_STOREID ";
            }

            // DISC_UPRICE>=15萬的排在前面,小於15萬的以APP_AMT降冪排序
            string sql = @"
                select '1' as GRP, MMCODE, SEQ, 
                    floor(nvl(nvl(DISC_UPRICE, DISC_CPRICE), 0)) as DISC_UPRICE, APPQTY,
                    (floor(nvl(nvl(DISC_UPRICE, DISC_CPRICE), 0)) * APPQTY) as APP_AMT 
                    from ME_DOCD A
                    where DOCNO = :DOCNO and M_CONTID = :M_CONTID " + addSql + @"
                    and floor(nvl(nvl(DISC_UPRICE, DISC_CPRICE), 0)) >= 150000
                union
                select '2' as GRP, MMCODE, SEQ, 
                    floor(nvl(nvl(DISC_UPRICE, DISC_CPRICE), 0)) as DISC_UPRICE, APPQTY,
                    (floor(nvl(nvl(DISC_UPRICE, DISC_CPRICE), 0)) * APPQTY) as APP_AMT 
                    from ME_DOCD A
                    where DOCNO = :DOCNO and M_CONTID = :M_CONTID " + addSql + @"
                    and floor(nvl(nvl(DISC_UPRICE, DISC_CPRICE), 0)) < 150000
                order by GRP, APP_AMT desc
            ";

            return DBWork.Connection.Query<ME_DOCD>(sql, new { DOCNO = docno, M_CONTID = m_contid, M_STOREID = m_storeid }, DBWork.Transaction);
        }

        public int CopyD(string old_docno, string seq, string new_docno, string appqty, string update_user, string update_ip)
        {
            var sql = @"INSERT INTO ME_DOCD (
                        DOCNO, SEQ, MMCODE , APPQTY, FRWH_D, AVG_PRICE, APLYITEM_NOTE, GTAPL_RESON,
                        M_CONTPRICE, UPRICE, DISC_CPRICE, DISC_UPRICE, M_NHIKEY, M_AGENNO,
                        SRCDOCNO, S_INV_QTY, INV_QTY, OPER_QTY, CHINNAME, CHARTNO, M_CONTID,
                        CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                        SELECT :NEW_DOCNO as DOCNO, 
                            (select (max(SEQ) + 1) as SEQ from ME_DOCD where DOCNO = :DOCNO) as SEQ, 
                            MMCODE, :APPQTY as APPQTY, FRWH_D, AVG_PRICE, APLYITEM_NOTE, GTAPL_RESON,
                            M_CONTPRICE, UPRICE, DISC_CPRICE, DISC_UPRICE, M_NHIKEY, M_AGENNO,
                            :NEW_DOCNO as SRCDOCNO, S_INV_QTY, INV_QTY, OPER_QTY, CHINNAME, CHARTNO, M_CONTID,
                            CREATE_TIME, :UPDATE_USER as CREATE_USER, UPDATE_TIME, :UPDATE_USER as UPDATE_USER, :UPDATE_IP as UPDATE_IP
                        FROM ME_DOCD 
                        WHERE DOCNO = :DOCNO and SEQ = :SEQ ";

            return DBWork.Connection.Execute(sql, new { DOCNO = old_docno, SEQ = seq, NEW_DOCNO = new_docno, APPQTY = appqty, UPDATE_USER = update_user, UPDATE_IP = update_ip }, DBWork.Transaction);
        }

        public int GetDocdCnt(string docno)
        {
            string sql = @"select count(*) from ME_DOCD
                            where DOCNO = :DOCNO ";
            return DBWork.Connection.QueryFirst<int>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }
    }
}
