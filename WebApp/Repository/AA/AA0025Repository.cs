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
    public class AA0025ReportMODEL : JCLib.Mvc.BaseModel
    {
        public string F1 { get; set; }
        public string F2 { get; set; }
        public string F3 { get; set; }
        public string F4 { get; set; }
        public string F5 { get; set; }
        public float F6 { get; set; }
        public float F7 { get; set; }
        public float F8 { get; set; }
        public string F9 { get; set; }
        public float F10 { get; set; }
        public string F11 { get; set; }
        public string F12 { get; set; }
        public string F13 { get; set; }
    }
    public class AA0025ReportBMODEL : JCLib.Mvc.BaseModel
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
        public float F10 { get; set; }
        public float F11 { get; set; }
        public string F12 { get; set; }
        public string F13 { get; set; }
        public float F14 { get; set; }
        public float F15 { get; set; }
        public float F16 { get; set; }
    }
    public class AA0025Repository : JCLib.Mvc.BaseRepository
    {
        public AA0025Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ME_DOCM> GetAllM(string apptime1, string apptime2, string[] str_purno, string[] str_agenno,  string docno, string[] str_FLOWID, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.* ,
                        (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='FLOWID_EX' AND DATA_VALUE=A.FLOWID and rownum=1) FLOWID_N,
                        (SELECT MAT_CLSNAME FROM MI_MATCLASS WHERE MAT_CLASS=A.MAT_CLASS and rownum=1) MAT_CLASS_N,
                        (SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.FRWH and rownum=1) FRWH_N,
                        (SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.TOWH and rownum=1) TOWH_N,
                        (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='APPLY_KIND' AND DATA_VALUE=A.APPLY_KIND and rownum=1) APPLY_KIND_N ,
                        (SELECT UNA FROM UR_ID WHERE TUSER=A.APPID and rownum=1) APP_NAME,
                        (SELECT INID_NAME FROM UR_INID WHERE INID=A.APPDEPT and rownum=1) APPDEPT_NAME,
                        (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='STKTRANSKIND2' AND DATA_VALUE=A.STKTRANSKIND and rownum=1) STKTRANSKIND_N,
                        (SELECT AGEN_NAMEC FROM PH_VENDER WHERE AGEN_NO = A.AGEN_NO and rownum=1)AGEN_NO_N,
                        TWN_DATE(A.APPTIME) APPTIME_T,
                        (SELECT SUM(X.APVQTY * Y.M_CONTPRICE * -1) FROM ME_DOCEXP X, MI_MAST Y WHERE X.DOCNO=A.DOCNO AND X.MMCODE=Y.MMCODE AND X.APVQTY < 0) SUM_EX,
                        (SELECT SUM(X.APVQTY * Y.M_CONTPRICE) FROM ME_DOCEXP X, MI_MAST Y WHERE X.DOCNO=A.DOCNO AND X.MMCODE=Y.MMCODE AND X.APVQTY > 0) SUM_IN  
                        FROM ME_DOCM A WHERE 1=1 AND A.DOCTYPE = 'EX' 
                        AND A.FRWH = 'PH1S' AND A.MAT_CLASS = '01' ";

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
            if (str_purno.Length > 0)
            {
                string sql_purno = "";
                sql += @"AND (";
                foreach (string tmp_purno in str_purno)
                {
                    if (string.IsNullOrEmpty(sql_purno))
                    {
                        sql_purno = @"A.STKTRANSKIND = '" + tmp_purno + "'";
                    }
                    else
                    {
                        sql_purno += @" OR A.STKTRANSKIND = '" + tmp_purno + "'";
                    }
                }
                sql += sql_purno + ") ";
            }
            if (str_agenno.Length > 0)
            {
                string sql_agenno = "";
                sql += @"AND (";
                foreach (string tmp_agenno in str_agenno)
                {
                    if (string.IsNullOrEmpty(sql_agenno))
                    {
                        sql_agenno = @"A.AGEN_NO = '" + tmp_agenno + "'";
                    }
                    else
                    {
                        sql_agenno += @" OR A.AGEN_NO = '" + tmp_agenno + "'";
                    }
                }
                sql += sql_agenno + ") ";
            }

            if (docno != "")
            {
                sql += " AND A.DOCNO = :pp ";
                p.Add(":pp", string.Format("{0}", docno));
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
            sql += @" ORDER BY A.DOCNO DESC";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<ME_DOCM>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<ME_DOCEXP> GetAllD(string DOCNO, string mmcode, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.DOCNO,A.SEQ,A.EXP_DATE,A.LOT_NO,A.LOT_NO LOT_NO_N,TWN_DATE(A.EXP_DATE) EXP_DATE_T,
                        A.MMCODE,A.C_TYPE,A.C_STATUS,A.C_RNO,A.C_UP,
                        (CASE WHEN A.APVQTY < 0 THEN -1 ELSE 1 END) * A.APVQTY APVQTY, 
                        (CASE WHEN A.APVQTY < 0 THEN -1 ELSE 1 END) * A.C_AMT C_AMT,
                        --( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO ='PH1S' AND ROWNUM=1) AS INV_QTY,
                        ( SELECT INV_QTY FROM MI_WEXPINV 
                           WHERE MMCODE = A.MMCODE AND WH_NO ='PH1S'  
                             AND LOT_NO=A.LOT_NO
                             and twn_date(exp_date) = twn_date(a.exp_date)) AS INV_QTY,
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

            if (mmcode != "")
            {
                sql += " AND A.MMCODE LIKE :p1 ";
                p.Add(":p1", string.Format("%{0}%", mmcode));
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCEXP>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<ME_DOCM> GetM(string id)
        {
            var sql = @"SELECT A.* ,
                        (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='FLOWID_EX' AND DATA_VALUE=A.FLOWID and rownum=1) FLOWID_N,
                        (SELECT MAT_CLSNAME FROM MI_MATCLASS WHERE MAT_CLASS=A.MAT_CLASS and rownum=1) MAT_CLASS_N,
                        (SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.FRWH and rownum=1) FRWH_N,
                        (SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.TOWH and rownum=1) TOWH_N,
                        (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='APPLY_KIND' AND DATA_VALUE=A.APPLY_KIND and rownum=1) APPLY_KIND_N ,
                        (SELECT UNA FROM UR_ID WHERE TUSER=A.APPID and rownum=1) APP_NAME,
                        (SELECT INID_NAME FROM UR_INID WHERE INID=A.APPDEPT and rownum=1) APPDEPT_NAME,
                        (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='STKTRANSKIND2' AND DATA_VALUE=A.STKTRANSKIND and rownum=1) STKTRANSKIND_N,
                        (SELECT AGEN_NAMEC FROM PH_VENDER WHERE AGEN_NO = A.AGEN_NO and rownum=1)AGEN_NO_N,
                        TWN_DATE(A.APPTIME) APPTIME_T,
                        (SELECT SUM(X.APVQTY * Y.M_CONTPRICE * -1) FROM ME_DOCEXP X, MI_MAST Y WHERE X.DOCNO=A.DOCNO AND X.MMCODE=Y.MMCODE AND X.APVQTY < 0) SUM_EX,
                        (SELECT SUM(X.APVQTY * Y.M_CONTPRICE) FROM ME_DOCEXP X, MI_MAST Y WHERE X.DOCNO=A.DOCNO AND X.MMCODE=Y.MMCODE AND X.APVQTY > 0) SUM_IN  
                        FROM ME_DOCM A WHERE 1=1 AND A.DOCTYPE = 'EX' AND A.FRWH = 'PH1S' AND A.MAT_CLASS = '01'
                        AND A.DOCNO = :DOCNO";
            return DBWork.Connection.Query<ME_DOCM>(sql, new { DOCNO = id }, DBWork.Transaction);
        }
        public IEnumerable<ME_DOCEXP> GetD(string id)
        {
            var sql = @"SELECT A.DOCNO,A.SEQ,A.EXP_DATE,A.LOT_NO,A.LOT_NO LOT_NO_N,TWN_DATE(A.EXP_DATE) EXP_DATE_T,
                        A.MMCODE,A.C_TYPE,A.C_STATUS,A.C_RNO,A.C_UP,
                        (CASE WHEN A.APVQTY < 0 THEN -1 ELSE 1 END) * A.APVQTY APVQTY,
                        (CASE WHEN A.APVQTY < 0 THEN -1 ELSE 1 END) * A.C_AMT C_AMT,
                        --( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO ='PH1S' AND ROWNUM=1) AS INV_QTY,
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
                        C_UP, C_AMT, EXP_DATE, LOT_NO,C_TYPE,
                        UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                      VALUES (
                        :DOCNO, :SEQ, :MMCODE , :APVQTY , :ITEM_NOTE, 
                        :C_UP, :C_AMT, TO_DATE(:EXP_DATE,'YYYY/MM/DD'), :LOT_NO, :C_TYPE,
                        SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, docexp, DBWork.Transaction);
        }
        public int CreateD(ME_DOCD docd)
        {
            var sql = @"INSERT INTO ME_DOCD (
                        DOCNO, SEQ, MMCODE ,APPQTY, APLYITEM_NOTE,
                         CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                      VALUES (
                        :DOCNO, :SEQ, :MMCODE , :APPQTY, :APLYITEM_NOTE,
                        SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, docd, DBWork.Transaction);
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
        public int UpdateD(ME_DOCD docd)
        {
            var sql = @"UPDATE ME_DOCD SET MMCODE = :MMCODE, 
                        APPQTY = :APPQTY, APLYITEM_NOTE = :APLYITEM_NOTE,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO AND SEQ = :SEQ ";
            return DBWork.Connection.Execute(sql, docd, DBWork.Transaction);
        }
        public int UpdateE(ME_DOCEXP docexp)
        {
            var sql = @"UPDATE ME_DOCEXP SET MMCODE = :MMCODE, 
                        APVQTY = :APVQTY, ITEM_NOTE = :ITEM_NOTE, C_UP = :C_UP, C_AMT = :C_AMT,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO AND SEQ = :SEQ ";
            return DBWork.Connection.Execute(sql, docexp, DBWork.Transaction);
        }

        public int ApplyM(ME_DOCM me_docm)
        {
            var sql = @"UPDATE ME_DOCM SET FLOWID = '0899',
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);
        }
        public int ApplyD(ME_DOCM ME_DOCM)
        {
            var sql = @"UPDATE ME_DOCEXP SET APL_CONTIME = SYSDATE,
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
        public int DeleteAllM(string docno)
        {
            var sql = @" DELETE from ME_DOCM WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno }, DBWork.Transaction);
        }
        public int DeleteAllD(string docno)
        {
            var sql = @" DELETE from ME_DOCEXP WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public int DeleteD(string docno, string seq)
        {
            var sql = @" DELETE from ME_DOCEXP WHERE DOCNO=:DOCNO AND SEQ=:SEQ";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno, SEQ = seq }, DBWork.Transaction);
        }
        public IEnumerable<AA0025ReportMODEL> GetPrintData(string apptime1, string apptime2)
        {
            var p = new DynamicParameters();

            string sql = @"SELECT TWN_DATE(B.APPTIME) F1,
                            (SELECT WH_NAME from MI_WHMAST where WH_NO=B.FRWH and rownum=1) F2,
                            A.MMCODE F3,C.MMNAME_E F4,
                            SUBSTR((SELECT PVR.AGEN_NO || '_' || PVR.AGEN_NAMEC
                              FROM MI_MAST MMT, PH_VENDER PVR
                              WHERE MMT.MMCODE = A.MMCODE
                              AND MMT.M_AGENNO = PVR.AGEN_NO),0,6) F5,
                            A.APVQTY F6,
                            C.M_CONTPRICE * C.M_DISCPERC  F7,
                            C.M_CONTPRICE F8,
                            DECODE (A.C_TYPE, '1', '進貨單價','合約單價') F9,
                            A.C_AMT F10,
                            A.ITEM_NOTE F11 ,
                            twn_date(a.exp_date) as F12,
                            a.lot_no as F13
                            FROM ME_DOCEXP A, ME_DOCM B,MI_MAST C
                            WHERE A.DOCNO=B.DOCNO AND A.MMCODE=C.MMCODE 
                            AND B.DOCTYPE = 'EX'  AND B.FRWH = 'PH1S' AND B.MAT_CLASS = '01'
                            and b.flowId = '0899'        
            ";

            if (apptime1 != "" & apptime2 != "")
            {
                sql += " AND TWN_DATE(B.APPTIME) BETWEEN :d0 AND :d1 ";
                p.Add(":d0", string.Format("{0}", apptime1));
                p.Add(":d1", string.Format("{0}", apptime2));
            }
            if (apptime1 != "" & apptime2 == "")
            {
                sql += " AND TWN_DATE(B.APPTIME) >= :d0 ";
                p.Add(":d0", string.Format("{0}", apptime1));
            }
            if (apptime1 == "" & apptime2 != "")
            {
                sql += " AND TWN_DATE(B.APPTIME) <= :d1 ";
                p.Add(":d1", string.Format("{0}", apptime2));
            }
            return DBWork.Connection.Query<AA0025ReportMODEL>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<AA0025ReportBMODEL> GetPrintDataB(string apptime1, string apptime2)
        {
            var p = new DynamicParameters();

            string sql = @"SELECT A.DOCNO F1,(SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='STKTRANSKIND2' AND DATA_VALUE=A.STKTRANSKIND and rownum=1) F2,
                            TWN_DATE(A.APPTIME) F3, B.MMCODE F4,C.MMNAME_E F5,C.BASE_UNIT F6,
                            (SELECT WH_NAME from MI_WHMAST where WH_NO=A.FRWH and rownum=1) F7,
                            (SELECT DATA_DESC FROM PARAM_D 
                              WHERE GRP_CODE='ME_DOCD' AND DATA_NAME='STAT' 
                              AND DATA_VALUE=(CASE WHEN B.APVQTY < 0 THEN '2' ELSE '1' END)) F8,
                            (CASE WHEN B.APVQTY < 0 THEN -1 ELSE 1 END) * B.APVQTY F9,
                            B.C_UP F10,
                            (CASE WHEN B.APVQTY < 0 THEN -1 ELSE 1 END) * B.C_AMT F11,
                            TWN_DATE(B.EXP_DATE) F12,
                            ITEM_NOTE F13,
                            NVL((SELECT SUM(X.APVQTY * X.C_UP * -1) FROM ME_DOCEXP X WHERE X.DOCNO=A.DOCNO AND X.APVQTY < 0),0) F14,
                            NVL((SELECT SUM(X.APVQTY * X.C_UP) FROM ME_DOCEXP X WHERE X.DOCNO=A.DOCNO AND X.APVQTY > 0),0) F15,
                            ( SELECT SUM( X.APVQTY * X.C_UP ) FROM ME_DOCEXP X WHERE X.DOCNO = A.DOCNO ) F16 
                            FROM ME_DOCM A,ME_DOCEXP B,MI_MAST C 
                            WHERE A.DOCNO = B.DOCNO AND B.MMCODE=C.MMCODE 
                            AND A.DOCTYPE = 'EX' 
                            AND A.FRWH = 'PH1S' AND A.MAT_CLASS = '01' AND A.FLOWID = '0899'
                             ";

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
            sql += " ORDER BY A.DOCNO ";
            return DBWork.Connection.Query<AA0025ReportBMODEL>(sql, p, DBWork.Transaction);
        }
        public bool CheckExists(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCM WHERE DOCNO=:DOCNO";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        public bool CheckExistsM(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCM WHERE DOCNO=:DOCNO AND FLOWID <> '0801'";
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
            string sql = @"SELECT 1 FROM ME_DOCEXP WHERE DOCNO=:DOCNO AND SEQ=:SEQ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id, SEQ = sid }, DBWork.Transaction) == null);
        }
        public bool CheckExistsDN(string id)
        {
            //string sql = @"SELECT 1 FROM ME_DOCEXP WHERE DOCNO=:DOCNO AND ( EXPT_DISTQTY = 0 OR BW_MQTY = 0 ) ";
            string sql = @"SELECT 1 FROM ME_DOCEXP WHERE DOCNO=:DOCNO AND APPQTY = 0  ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        public bool CheckExistsMM(string id, string mm)
        {
            string sql = @"SELECT 1 FROM ME_DOCEXP WHERE DOCNO=:DOCNO AND MMCODE=:MMCODE ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id, MMCODE = mm}, DBWork.Transaction) == null);
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
        public string GetUsername(string id)
        {
            string sql = @" SELECT TUSER || '.' || UNA AS TUSER FROM UR_ID WHERE TUSER=:TUSER ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { TUSER = id }, DBWork.Transaction).ToString();
            return rtn;
        }
        public string GetTwnsystime()
        {
            string sql = @"SELECT TWN_SYSTIME FROM DUAL ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction).ToString();
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
        public string GetDocESeq(string id)
        {
            string sql = @"SELECT MAX(SEQ)+1 as SEQ FROM ME_DOCEXP WHERE DOCNO=:DOCNO ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction).ToString();
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
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { DOCNO = doc , SEQ = seq}, DBWork.Transaction).ToString();
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
        public bool CheckExistsDMmcode(string doc,string mmcode)
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

        public IEnumerable<COMBO_MODEL> GetFlowidCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='FLOWID_EX' 
                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
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
                        WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='STKTRANSKIND2' 
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
        public IEnumerable<MI_MAST> GetMmCodeCombo(string p0, string p1, string docno, int page_index, int page_size, string sorters)
        {

            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT, A.M_CONTPRICE, 
                        ( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO ='PH1S' AND ROWNUM=1) AS INV_QTY,
                        B.AGEN_NO || ' ' || B.AGEN_NAMEC AGEN_NAMEC,
                        A.UPRICE  as M_DISCPERC 
                        FROM MI_MAST A, PH_VENDER B WHERE 1=1 AND A.M_AGENNO = B.AGEN_NO AND A.MAT_CLASS = :MAT_CLASS 
                        AND EXISTS ( SELECT 1 FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO ='PH1S' )
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
            //p.Add(":DOCNO", docno);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMmcode(MI_MAST_QUERY_PARAMS query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT,A.M_CONTPRICE,
                        ( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO IN ( SELECT WH_NO FROM MI_WHMAST WHERE WH_KIND='1' AND WH_GRADE='1')  AND ROWNUM=1) AS INV_QTY,
                        B.AGEN_NO || ' ' || B.AGEN_NAMEC AGEN_NAMEC,
                        A.UPRICE  as M_DISCPERC  
                        FROM MI_MAST A, PH_VENDER B WHERE 1=1 AND A.M_AGENNO = B.AGEN_NO 
                        AND A.MAT_CLASS = :MAT_CLASS  
                        AND EXISTS ( SELECT 1 FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO ='PH1S' )
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
                        FROM MI_MAST A, ME_DOCEXP B WHERE A.MMCODE=B.MMCODE AND B.DOCNO = :DOCNO  ";
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

        public IEnumerable<MI_WEXPINV> GetLotno(string mmcode, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT DISTINCT A.MMCODE, A.LOT_NO , TWN_DATE(A.EXP_DATE) as EXP_DATE , A.INV_QTY  
                        FROM MI_WEXPINV  A 
                        WHERE A.WH_NO='PH1S'
                        AND A.MMCODE=:MMCODE
                        ORDER BY A.LOT_NO   ";

            p.Add(":MMCODE", mmcode);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_WEXPINV>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
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
                          and data_value in ('1','2')
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
            public string AGEN_NO;

            public string WH_NO;
            public string IS_INV;  // 需判斷庫存量>0
            public string E_IFPUBLIC;  // 是否公藥
        }

        //批號+效期+效期數量combox
        public IEnumerable<MI_WEXPINV> GetLOT_NO(string FRWH, string MMCODE)
        {
            string sql = @"  SELECT MWV.LOT_NO LOT_NO,
                                    (TO_CHAR (MWV.EXP_DATE, 'YYYYMMDD') - 19110000) EXP_DATE,
                                    MWV.INV_QTY
                             FROM MI_WEXPINV MWV
                             WHERE MWV.WH_NO = :FRWH
                             AND MWV.MMCODE = :MMCODE";

            return DBWork.Connection.Query<MI_WEXPINV>(sql, new { FRWH = FRWH, MMCODE = MMCODE }, DBWork.Transaction);
        }

        //帶出效期
        public string GetEXP_DATE(string FRWH, string MMCODE, string LOT_NO)
        {
            string sql = @"  SELECT (TO_CHAR (MWV.EXP_DATE, 'YYYYMMDD') - 19110000) EXP_DATE
                             FROM MI_WEXPINV MWV
                             WHERE MWV.WH_NO = :FRWH
                             AND MWV.MMCODE = :MMCODE
                             AND MWV.LOT_NO = :LOT_NO";

            return DBWork.Connection.Query<string>(sql, new { FRWH = FRWH, MMCODE = MMCODE }, DBWork.Transaction).ToString();
        }

        //帶出效期
        public string GetINV_QTY(string FRWH, string MMCODE, string LOT_NO)
        {
            string sql = @"  SELECT MWV.INV_QTY INV_QTY
                             FROM MI_WEXPINV MWV
                             WHERE MWV.WH_NO = :FRWH
                             AND MWV.MMCODE = :MMCODE
                             AND MWV.LOT_NO = :LOT_NO";

            return DBWork.Connection.Query<string>(sql, new { FRWH = FRWH, MMCODE = MMCODE }, DBWork.Transaction).ToString();
        }

        //帶出進貨單價
        public string GetM_DISCPERC(string WH_NO, string MMCODE)
        {
            var p = new DynamicParameters();
            string sql = @"SELECT MMT.UPRICE M_DISCPERC
                           FROM MI_WHINV MWV, MI_MAST MMT, PH_VENDER PVR
                           WHERE MWV.WH_NO = :WH_NO
                           AND   MWV.MMCODE = :MMCODE
                           AND   MWV.MMCODE = MMT.MMCODE
                           AND   MMT.M_AGENNO = PVR.AGEN_NO";

            return DBWork.Connection.QueryFirst<string>(sql, new { WH_NO = WH_NO, MMCODE = MMCODE }, DBWork.Transaction);
        }

        //帶出合約單價
        public string GetM_CONTPRICE(string WH_NO, string MMCODE)
        {
            var p = new DynamicParameters();
            string sql = @"SELECT MMT.M_CONTPRICE
                           FROM MI_WHINV MWV, MI_MAST MMT, PH_VENDER PVR
                           WHERE MWV.WH_NO = :WH_NO
                           AND   MWV.MMCODE = :MMCODE
                           AND   MWV.MMCODE = MMT.MMCODE
                           AND   MMT.M_AGENNO = PVR.AGEN_NO";

            return DBWork.Connection.QueryFirst<string>(sql, new { WH_NO = WH_NO, MMCODE = MMCODE }, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetDocnoCombo()
        {
            string sql = @"SELECT DISTINCT A.DOCNO as VALUE, A.DOCNO as TEXT,
                        A.DOCNO as COMBITEM
                        FROM ME_DOCM A ,MI_MATCLASS B 
                        WHERE A.MAT_CLASS=B.MAT_CLASS 
                        AND A.DOCTYPE = 'EX' 
                        AND A.FRWH = 'PH1S' AND A.MAT_CLASS = '01'
                        ORDER BY A.DOCNO DESC";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
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

        #region 2022-06-07 過帳針對非批號效期管制品項新增至MI_WEXPINV
        public ME_DOCM GetMeDocm(string docno) {
            string sql = @" select * from ME_DOCM where docno = :docno";
            return DBWork.Connection.QueryFirstOrDefault<ME_DOCM>(sql, new { docno}, DBWork.Transaction);
        }
        public IEnumerable<ME_DOCEXP> GetMeDocExpWexpidNs(string docno) {
            string sql = @"
                select a.docno, a.mmcode, a.lot_no, to_char(a.exp_date, 'yyyy/mm/dd') as exp_date, a.apvqty
                  from ME_DOCEXP a
                 where a.docno = :docno
                   and exists(select 1 from MI_MAST where mmcode = a.mmcode and nvl(trim(wexp_id), 'N') = 'N')
            ";
            return DBWork.Connection.Query<ME_DOCEXP>(sql, new { docno }, DBWork.Transaction);
        }
        public bool CheckExistsWexpinv(string wh_no, string mmcode, string lot_no, string exp_date) {
            string sql = @"
                select 1 from MI_WEXPINV
                 where wh_no = :wh_no
                   and mmcode = :mmcode
                   and lot_no = :lot_no
                   and exp_date = to_date(:exp_date,'yyyy/mm/dd')
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { wh_no, mmcode, lot_no, exp_date }, DBWork.Transaction) != null;
        }

        public int UpdateWexpinv(string wh_no, string mmcode, string lot_no, string exp_date, string qty, string userId, string updateIp) {
            string sql = @"
                update MI_WEXPINV
                   set inv_qty = inv_qty + :qty ,
                       update_time = sysdate,
                       update_user = :userId,
                       update_ip = :updateIp
                 where wh_no = :wh_no
                   and mmcode = :mmcode
                   and lot_no = :lot_no
                   and exp_date = to_date(:exp_date, 'yyyy/mm/dd')
            ";
            return DBWork.Connection.Execute(sql, new { wh_no, mmcode, lot_no, exp_date, qty, userId, updateIp }, DBWork.Transaction);
        }

        public int InsertWexpinv(string wh_no, string mmcode, string lot_no, string exp_date, string qty, string userId, string updateIp) {
            string sql = @"
                insert into MI_WEXPINV (wh_no, mmcode, exp_date, lot_no, inv_qty,
                                        create_user, update_time, update_user, update_ip, create_time)
                values (:wh_no, :mmcode, to_date(:exp_date, 'yyyy/mm/dd'), :lot_no, :qty,
                        :userId, sysdate, :userId, :updateIp, sysdate)
            ";
            return DBWork.Connection.Execute(sql, new { wh_no, mmcode, lot_no, exp_date, qty, userId, updateIp }, DBWork.Transaction);
        }
        #endregion
    }
}
