﻿using System;
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
    public class AA0027Repository : JCLib.Mvc.BaseRepository
    {
        public AA0027Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        //查詢Master
        public IEnumerable<AA0027M> GetAllM(string START_DATE, string END_DATE, string FRWH, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT MDM.DOCNO,
                               MDM.FLOWID,
                               (CASE 
                                    WHEN MDM.FLOWID = '1001' THEN '換貨申請中'
                                    WHEN MDM.FLOWID = '1099' THEN '換貨完成'
                                    WHEN MDM.FLOWID = 'X' THEN '已撤銷'
                               END) FLOWID_NAME,
                               TO_CHAR (MDM.APPTIME, 'YYYYMMDD') - 19110000 APPTIME,
                               TO_CHAR (MDM.APPTIME, 'YYYYMMDD') - 19110000 APPTIME_TEXT,
                               MDM.FRWH,
                               (SELECT MWT.WH_NAME
                                FROM MI_WHMAST MWT
                                WHERE MWT.WH_NO = MDM.FRWH) FRWH_NAME,
                               (SELECT MWT.WH_NO || ' ' || MWT.WH_NAME
                                FROM MI_WHMAST MWT
                                WHERE MWT.WH_NO = MDM.FRWH) FRWH_CODE,
                               MDM.APPLY_NOTE,
                               MDM.APPID,
                               (SELECT UNA
                                FROM UR_ID
                                WHERE TUSER = MDM.APPID) APP_ID_NAME,
                               TO_CHAR (MDM.UPDATE_TIME, 'YYYYMMDD') - 19110000 UPDATE_TIME
                        FROM ME_DOCM MDM
                        WHERE DOCTYPE = 'EM'
                        AND MAT_CLASS = '01'
                        AND MDM.FRWH = :FRWH ";

            p.Add(":FRWH", string.Format("{0}", FRWH));

            if (START_DATE != "")
            {
                sql += @" AND TRUNC (MDM.APPTIME) >= TO_DATE ( :START_DATE, 'YYYY/MM/DD') ";
                p.Add(":START_DATE", string.Format("{0}", START_DATE));
            }

            if (END_DATE != "")
            {
                sql += @" AND TRUNC (MDM.APPTIME) <= TO_DATE ( :END_DATE, 'YYYY/MM/DD') ";
                p.Add(":END_DATE", string.Format("{0}", END_DATE));
            }

            sql += @" ORDER BY MDM.APPTIME DESC";
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0027M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //查詢Detail
        public IEnumerable<AA0027D> GetAllD(string FRWH, string DOCNO, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"    SELECT MDP.DOCNO,
                                   MDP.MMCODE,
                                   MDP.MMCODE MMCODE_TEXT,
                                   (SELECT MMT.MMNAME_C
                                    FROM MI_MAST MMT
                                    WHERE MMT.MMCODE = MDP.MMCODE) MMNAME_C,
                                   (SELECT MMT.MMNAME_E
                                    FROM MI_MAST MMT
                                    WHERE MMT.MMCODE = MDP.MMCODE) MMNAME_E,
                                   (SELECT PVR.AGEN_NAMEC
                                    FROM MI_MAST MMT, PH_VENDER PVR
                                    WHERE MMT.MMCODE = MDP.MMCODE
                                    AND MMT.M_AGENNO = PVR.AGEN_NO) AGEN_NAME,
                                   MDP.LOT_NO,
                                   MDP.LOT_NO LOT_NO_TEXT,
                                   (TO_CHAR (MDP.EXP_DATE, 'YYYYMMDD') - 19110000) EXP_DATE,
                                   (SELECT TO_CHAR (ROUND (MWV2.INV_QTY, 2), 'FM999999990.00')
                                    FROM MI_WEXPINV MWV2
                                    WHERE     MWV2.WH_NO = :FRWH
                                    AND MWV2.MMCODE = MDP.MMCODE
                                    AND MWV2.LOT_NO = MDP.LOT_NO
                                    AND MWV2.EXP_DATE = MDP.EXP_DATE) INV_QTY,
                                   -MDP.APVQTY APVQTY,
                                   -MDP.APVQTY APVQTY_TEXT,
                                   (SELECT MMT.BASE_UNIT
                                    FROM MI_MAST MMT
                                    WHERE MMT.MMCODE = MDP.MMCODE) BASE_UNIT,
                                   MDP.C_TYPE,
                                   (CASE
                                        WHEN MDP.C_TYPE = 1 THEN '1 進貨'
                                        WHEN MDP.C_TYPE = 2 THEN '2 合約'
                                        ELSE TO_CHAR (MDP.C_TYPE)
                                    END) C_TYPE_NAME,
                                   (CASE
                                        WHEN MDP.C_TYPE = 1 THEN '1 進貨'
                                        WHEN MDP.C_TYPE = 2 THEN '2 合約'
                                        ELSE TO_CHAR (MDP.C_TYPE)
                                    END) C_TYPE_NAME_TEXT,
                                   DECODE (MDP.C_TYPE, 2, '', MDP.C_UP) IN_PRICE,
                                   DECODE (MDP.C_TYPE, 1, '', MDP.C_UP) CONTPRICE,
                                   MDP.C_AMT,
                                   MDP.ITEM_NOTE,
                                   MDP.SEQ,
                                   (SELECT UNA
                                    FROM UR_ID
                                    WHERE TUSER = MDP.UPDATE_USER) UPDATE_USER
                            FROM ME_DOCEXP MDP
                            WHERE MDP.DOCNO = :DOCNO
                            ORDER BY MDP.MMCODE, MDP.SEQ";

            p.Add(":FRWH", string.Format("{0}", FRWH));
            p.Add(":DOCNO", string.Format("{0}", DOCNO));
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0027D>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //檢查Master是否存在
        public bool CheckExistsM(string DOCNO)
        {
            string sql = @"SELECT 1
                           FROM ME_DOCM MDM
                           WHERE MDM.DOCNO = :DOCNO";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = DOCNO }, DBWork.Transaction) == null);
        }

        //新增Master
        public int CreateM(AA0027M AA0027M)
        {
            var sql = @"INSERT INTO ME_DOCM (DOCNO,
                                             FLOWID,
                                             APPTIME,
                                             FRWH,
                                             APPLY_NOTE,
                                             APPID,
                                             DOCTYPE,
                                             MAT_CLASS,
                                             TOWH,
                                             CREATE_TIME,
                                             CREATE_USER,
                                             UPDATE_TIME,
                                             UPDATE_USER,
                                             UPDATE_IP)
                        VALUES (:DOCNO,
                                '1001',
                                TO_DATE( :APPTIME, 'YYYY/MM/DD'),
                                :FRWH,
                                :APPLY_NOTE,
                                :CREATE_USER,
                                'EM',
                                '01',
                                :FRWH,
                                SYSDATE,
                                :CREATE_USER,
                                SYSDATE,
                                :UPDATE_USER,
                                :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, AA0027M, DBWork.Transaction);
        }

        //修改Master
        public int UpdateM(AA0027M AA0027M)
        {
            var sql = @"UPDATE ME_DOCM
                        SET APPTIME = TO_DATE( :APPTIME, 'YYYY/MM/DD'),
                            FLOWID = '1001',
                            FRWH = :FRWH,
                            TOWH = :FRWH,
                            APPLY_NOTE = :APPLY_NOTE,
                            UPDATE_TIME = SYSDATE,
                            UPDATE_USER = :UPDATE_USER,
                            UPDATE_IP = :UPDATE_IP
                            WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, AA0027M, DBWork.Transaction);
        }

        //撤銷Master
        public int BackM(AA0027M AA0027M)
        {
            var sql = @"UPDATE ME_DOCM
                        SET FLOWID = 'X',
                            UPDATE_TIME = SYSDATE,
                            UPDATE_USER = :UPDATE_USER,
                            UPDATE_IP = :UPDATE_IP
                            WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, AA0027M, DBWork.Transaction);
        }


        //新增Detail
        public int CreateD(AA0027D AA0027D)
        {
            var sql = @"INSERT INTO ME_DOCEXP (DOCNO,
                                               SEQ,
                                               MMCODE,
                                               APVQTY,
                                               LOT_NO,
                                               EXP_DATE,
                                               ITEM_NOTE,
                                               C_TYPE,
                                               C_AMT,
                                               C_UP,
                                               UPDATE_TIME,
                                               UPDATE_USER,
                                               UPDATE_IP)
                        VALUES ( :DOCNO,
                                 (SELECT NVL (MAX (AA.SEQ), 0) + 1 SEQ
                                  FROM (  SELECT MAX (SEQ) SEQ
                                          FROM ME_DOCEXP
                                          WHERE DOCNO = :DOCNO
                                          GROUP BY DOCNO) AA),
                                 :MMCODE,
                                 - :APVQTY,
                                 :LOT_NO,
                                 TO_DATE ( :EXP_DATE + 19110000 , 'YYYY/MM/DD'),
                                 :ITEM_NOTE,
                                 :C_TYPE,
                                 :C_AMT,
                                 :C_UP,
                                 SYSDATE,
                                 :UPDATE_USER,
                                 :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, AA0027D, DBWork.Transaction);
        }

        //修改Detail
        public int UpdateD(AA0027D AA0027D)
        {
            var sql = @"UPDATE ME_DOCEXP
                        SET MMCODE = :MMCODE,
                            APVQTY = - :APVQTY,
                            LOT_NO = :LOT_NO,
                            EXP_DATE = TO_DATE ( :EXP_DATE + 19110000 , 'YYYY/MM/DD'),
                            ITEM_NOTE = :ITEM_NOTE,
                            C_TYPE = :C_TYPE,
                            C_AMT = :C_AMT,
                            C_UP = :C_UP,
                            UPDATE_TIME = SYSDATE,
                            UPDATE_USER = :UPDATE_USER,
                            UPDATE_IP = :UPDATE_IP
                        WHERE DOCNO = :DOCNO AND SEQ = :SEQ";
            return DBWork.Connection.Execute(sql, AA0027D, DBWork.Transaction);
        }

        //刪除Detail
        public int DeleteD(string DOCNO, string SEQ)
        {
            var sql = @"DELETE ME_DOCEXP
                        WHERE DOCNO = :DOCNO
                        AND SEQ = :SEQ";
            return DBWork.Connection.Execute(sql, new { DOCNO = DOCNO, SEQ = SEQ }, DBWork.Transaction);
        }

        //送審核(狀態改為1099)
        public int RunM_1(AA0027M AA0027M)
        {
            var sql = @"UPDATE ME_DOCM
                        SET UPDATE_TIME = SYSDATE,
                            UPDATE_USER = :UPDATE_USER,
                            UPDATE_IP = :UPDATE_IP
                        WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, AA0027M, DBWork.Transaction);
        }

        //送審核(找出ME_DOCEXP資料)
        public IEnumerable<AA0027D> RunM_2(string DOCNO)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT ROWNUM SEQ,
                               AA.DOCNO,
                               AA.MMCODE,
                               AA.APVQTY,
                               AA.ITEM_NOTE
                        FROM (  SELECT MDP.DOCNO,
                                       MDP.MMCODE MMCODE,
                                       SUM (MDP.APVQTY) APVQTY,
                                       (SELECT LISTAGG (MDP2.ITEM_NOTE, ',') WITHIN GROUP (ORDER BY MDP2.ITEM_NOTE)
                                FROM ME_DOCEXP MDP2
                                WHERE MDP.DOCNO = MDP2.DOCNO
                                AND MDP.MMCODE = MDP2.MMCODE) ITEM_NOTE
                        FROM ME_DOCEXP MDP
                        WHERE MDP.DOCNO = :DOCNO
                        GROUP BY MDP.DOCNO, MDP.MMCODE
                        ORDER BY MMCODE) AA";

            p.Add(":DOCNO", string.Format("{0}", DOCNO));

            return DBWork.Connection.Query<AA0027D>(sql, p, DBWork.Transaction);
        }

        //送審核(將ME_DOCEXP資料新增至ME_DOCD)
        public int RunM_3(AA0027D AA0027D)
        {
            var sql = @"INSERT INTO ME_DOCD ( DOCNO,
                                              MMCODE,
                                              SEQ,
                                              APPQTY,
                                              STAT,
                                              APLYITEM_NOTE,
                                              CREATE_USER,
                                              CREATE_TIME,
                                              UPDATE_TIME,
                                              UPDATE_USER,
                                              UPDATE_IP)
                        VALUES ( :DOCNO,
                                 :MMCODE,
                                 :SEQ,
                                 :APVQTY,
                                 (CASE
                                       WHEN :APVQTY > 0 THEN '1'
                                       WHEN :APVQTY < 0 THEN '2'
                                       ELSE NULL
                                  END),
                                 :ITEM_NOTE,
                                 :UPDATE_USER,
                                 SYSDATE,
                                 SYSDATE,
                                 :UPDATE_USER,
                                 :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, AA0027D, DBWork.Transaction);
        }

        //送審核(呼叫STORE PROCEDURE: POST_DOC)
        public string POST_DOC(string I_DOCNO, string I_UPDUSR, string I_UPDIP)
        {
            var p = new OracleDynamicParameters();
            p.Add("I_DOCNO", value: I_DOCNO, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 21);
            p.Add("I_UPDUSR", value: I_UPDUSR, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 8);
            p.Add("I_UPDIP", value: I_UPDIP, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 20);

            p.Add("O_RETID", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 1);
            p.Add("O_ERRMSG", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 255);

            DBWork.Connection.Query("POST_DOC", p, commandType: CommandType.StoredProcedure);
            string retid = p.Get<OracleString>("O_RETID").Value;
            string errmsg = p.Get<OracleString>("O_ERRMSG").Value;
            return retid + "," + errmsg;
        }

        //調帳庫別combox
        public IEnumerable<ComboItemModel> GetWH_NO()
        {
            string sql = @"  SELECT MWT.WH_NO VALUE,
                                    MWT.WH_NO || ' ' || MWT.WH_NAME TEXT
                             FROM MI_WHMAST MWT
                             WHERE MWT.WH_KIND = '0' 
                             AND MWT.WH_GRADE IN ('1', '2')
                             ORDER BY MWT.WH_NO";

            return DBWork.Connection.Query<ComboItemModel>(sql, DBWork.Transaction);
        }

        //取得調帳單號
        public string GetDocno()
        {
            var p = new OracleDynamicParameters();
            p.Add("O_DOCNO", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 12);

            DBWork.Connection.Query("GET_DOCNO", p, commandType: CommandType.StoredProcedure);
            return p.Get<OracleString>("O_DOCNO").Value;
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

        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, string p1, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.AGEN_NAMEC, A.BASE_UNIT, A.M_DISCPERC, A.M_CONTPRICE
                        FROM (SELECT MMT.MMCODE,
                                     MMT.MMNAME_C,
                                     MMT.MMNAME_E,
                                     PVR.AGEN_NO || ' ' || PVR.AGEN_NAMEC AGEN_NAMEC,
                                     MMT.BASE_UNIT,
                                     MMT.DISC_CPRICE as  M_DISCPERC,
                                     MMT.M_CONTPRICE
                              FROM MI_WHINV MWV, MI_MAST MMT, PH_VENDER PVR
                              WHERE     MWV.WH_NO = :FRWH
                              AND MWV.MMCODE = MMT.MMCODE
                              AND MMT.M_AGENNO = PVR.AGEN_NO
                              AND EXISTS
                                        (SELECT 1
                                         FROM MI_WEXPINV MWV2
                                         WHERE MWV2.WH_NO = MWV.WH_NO
                                         AND MWV2.MMCODE = MWV.MMCODE)) A
                        WHERE 1=1 ";
            p.Add(":FRWH", string.Format("{0}", p1));


            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(A.MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(A.MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("%{0}%", p0));

                sql += " OR A.MMNAME_E LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR A.MMNAME_C LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX, MMCODE ", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY A.MMCODE ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public class MI_MAST_QUERY_PARAMS
        {
            public string MMCODE;
            public string MMNAME_C;
            public string MMNAME_E;
            public string FRWH;
            public string AGEN_NAME;
        }

        public IEnumerable<MI_MAST> GetMmcode(MI_MAST_QUERY_PARAMS query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            string sql = @"SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.AGEN_NAMEC, A.BASE_UNIT, A.M_DISCPERC, A.M_CONTPRICE
                           FROM (SELECT MMT.MMCODE,
                                        MMT.MMNAME_C,
                                        MMT.MMNAME_E,
                                        PVR.AGEN_NO || ' ' || PVR.AGEN_NAMEC AGEN_NAMEC,
                                        MMT.BASE_UNIT,
                                        MMT.DISC_CPRICE as M_DISCPERC,
                                        MMT.M_CONTPRICE
                                 FROM MI_WHINV MWV, MI_MAST MMT, PH_VENDER PVR
                                 WHERE     MWV.WH_NO = :FRWH
                                 AND MWV.MMCODE = MMT.MMCODE
                                 AND MMT.M_AGENNO = PVR.AGEN_NO
                                 AND EXISTS
                                           (SELECT 1
                                            FROM MI_WEXPINV MWV2
                                            WHERE MWV2.WH_NO = MWV.WH_NO
                                            AND MWV2.MMCODE = MWV.MMCODE)) A 
                              WHERE 1=1  ";

            p.Add(":FRWH", string.Format("{0}", query.FRWH));

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
                sql += " AND A.MMNAME_E LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", query.MMNAME_E));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //帶出進貨單價
        public string GetM_DISCPERC(string WH_NO, string MMCODE)
        {
            var p = new DynamicParameters();
            string sql = @"SELECT MMT.DISC_CPRICE M_DISCPERC
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

        //取得建立人員
        public string GetAPPID_NAME(string TUSER)
        {
            var p = new DynamicParameters();
            string sql = @"SELECT UNA
                           FROM UR_ID
                           WHERE TUSER = :TUSER";

            return DBWork.Connection.QueryFirst<string>(sql, new { TUSER = TUSER }, DBWork.Transaction);
        }

        //新增前檢查院內碼、批號是否有對應到
        public bool CheckMmcode(string FRWH, string MMCODE, string LOT_NO)
        {
            string sql = @"SELECT 1
                           FROM MI_WEXPINV MWV
                           WHERE MWV.WH_NO = :FRWH
                           AND MWV.MMCODE = :MMCODE
                           AND MWV.LOT_NO = :LOT_NO";
            return !(DBWork.Connection.ExecuteScalar(sql, new { FRWH = FRWH, MMCODE = MMCODE, LOT_NO = LOT_NO }, DBWork.Transaction) == null);
        }


        #region 2020-08-18 新增: 列印
        public class AA0027ReportMODEL : JCLib.Mvc.BaseModel
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
            public string free { get; set; }
        }

        public IEnumerable<AA0025ReportMODEL> GetPrintData(string apptime1, string apptime2)
        {
            var p = new DynamicParameters();

            string sql = @"select TWN_DATE(b.APPTIME) F1,
                                  (select WH_NAME from MI_WHMAST 
                                    where WH_NO=b.FRWH) F2,
                                  a.MMCODE F3,c.MMNAME_E F4,
                                  SUBSTR((select p.AGEN_NO || '_' || p.AGEN_NAMEC
                                            from MI_MAST m, PH_VENDER p
                                           where m.MMCODE=a.MMCODE
                                             and m.M_AGENNO=p.AGEN_NO
                                         ),0,6) F5,
                                  -a.APVQTY F6,
                                  c.DISC_CPRICE F7,
                                  c.M_CONTPRICE F8,
                                  DECODE(a.C_TYPE, '1', '進貨單價','合約單價') F9,
                                  a.C_AMT F10,
                                  a.ITEM_NOTE F11,
                                  (case when a.C_AMT=0 then 2
                                        else 1
                                   end) free 
                             from ME_DOCEXP a, ME_DOCM b, MI_MAST c
                            where a.DOCNO=b.DOCNO and a.MMCODE=c.MMCODE 
                              and b.DOCTYPE='EM' and b.MAT_CLASS='01'  
                              and b.FRWH='PH1S'  --藥庫業務
                              and FLOWID='1099'
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

            sql += "        order by free, b.apptime, a.mmcode";
            return DBWork.Connection.Query<AA0025ReportMODEL>(sql, p, DBWork.Transaction);
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
    }
}
