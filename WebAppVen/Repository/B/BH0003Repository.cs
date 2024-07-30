using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebAppVen.Models;
using System.Collections.Generic;

namespace WebAppVen.Repository.B
{
    public class BH0003Repository : JCLib.Mvc.BaseRepository
    {
        public BH0003Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        //查詢Master
        public IEnumerable<WB_PUT_M> GetAllM(string AGEN_NO, string MMCODE, string MMNAME_C, string MMNAME_E, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT WPM.AGEN_NO,
                               WPM.MMCODE,
                               WPM.MMCODE MMCODE_DISPLAY,
                               WPM.MMCODE MMCODE_TEXT,
                               WPM.MEMO,
                               WPM.QTY,
                               WPM.QTY QTY_DISPLAY,
                               RTRIM (WPM.MMNAME_C) MMNAME_C,
                               RTRIM (WPM.MMNAME_E) MMNAME_E,
                               (CASE
                                    WHEN WPM.STATUS = 'A' THEN '寄放中'
                                    WHEN WPM.STATUS = 'D' THEN '刪除'
                               END) STATUS_NAME,
                               (CASE
                                    WHEN WPM.STATUS = 'A' THEN '寄放中'
                                    WHEN WPM.STATUS = 'D' THEN '刪除'
                               END) STATUS_TEXT,
                               WPM.DEPT + ' ' + WPM.DEPTNAME DEPT_NAME,
                               WPM.DEPT + ' ' + WPM.DEPTNAME DEPT_NAME_DISPLAY,
                               WPM.DEPT + ' ' + WPM.DEPTNAME DEPT_NAME_TEXT,
                               RTRIM (WPM.DEPTNAME) DEPTNAME,
                               WPM.DEPT,
                               WPM.STATUS
                        FROM WB_PUT_M WPM
                        WHERE 1 = 1";

            sql += @" AND WPM.AGEN_NO = @p0 ";
            p.Add("@p0", string.Format("{0}", AGEN_NO));

            if (MMCODE != "")
            {
                sql += @" AND WPM.MMCODE LIKE @p1 ";
                p.Add("@p1", string.Format("{0}%", MMCODE));
            }

            if (MMNAME_C != "")
            {
                sql += @" AND WPM.MMNAME_C LIKE @p2 ";
                p.Add("@p2", string.Format("{0}%", MMNAME_C));
            }

            if (MMNAME_E != "")
            {
                sql += @" AND WPM.MMNAME_E LIKE @p3 ";
                p.Add("@p3", string.Format("{0}%", MMNAME_E));
            }

            //sql += @"ORDER BY WPM.AGEN_NO, WPM.MMCODE";
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<WB_PUT_M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //查詢Detail
        public IEnumerable<WB_PUT_D> GetAllD(string AGEN_NO, string MMCODE, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT WPD.*,
                               (CASE
                                    WHEN WPD.EXTYPE = '10' THEN '取回'
                                    WHEN WPD.EXTYPE = '20' THEN '耗用'
                                    WHEN WPD.EXTYPE = '31' THEN '寄放'
                                END) EXTYPE_NAME,
                               REPLACE(REPLACE(REPLACE(SUBSTRING(CONVERT(VARCHAR, WPD.TXTDAY, 120),0,17),'-',''),' ',''),':','') - 191100000000 TXTDAY_TEXT,
                               REPLACE(REPLACE(REPLACE(SUBSTRING(CONVERT(VARCHAR, WPD.TXTDAY, 120),0,17),'-',''),' ',''),':','') - 191100000000 TXTDAY_DISPLAY,
                               (CASE
                                    WHEN WPD.EXTYPE = '10' THEN '10 取回'
                                    WHEN WPD.EXTYPE = '20' THEN '20 耗用'
                                    WHEN WPD.EXTYPE = '31' THEN '31 寄放'
                                END) EXTYPE_TEXT,
                               (CASE
                                    WHEN WPD.EXTYPE = '10' THEN '10 取回'
                                    WHEN WPD.EXTYPE = '20' THEN '20 耗用'
                                    WHEN WPD.EXTYPE = '31' THEN '31 寄放'
                                END) EXTYPE_DISPLAY,
                                WPD.QTY QTY_DISPLAY,
                               (CASE
                                    WHEN (WPD.STATUS = 'B' AND EXTYPE = '20') THEN '三總回傳'
                                    WHEN WPD.STATUS = 'A' THEN '處理中'
                                    WHEN WPD.STATUS = 'B' THEN '確認回傳'
                                END) STATUS_NAME,
                               (CASE
                                    WHEN (WPD.STATUS = 'B' AND EXTYPE = '20') THEN '三總回傳'
                                    WHEN WPD.STATUS = 'A' THEN 'A 處理中'
                                    WHEN WPD.STATUS = 'B' THEN 'B 確認回傳'
                                END) STATUS_TEXT
                        FROM WB_PUT_D WPD
                        WHERE WPD.AGEN_NO = @AGEN_NO 
                        AND WPD.MMCODE = @MMCODE";

            //sql += @" ORDER BY WPD.TXTDAY DESC";
            p.Add("@AGEN_NO", string.Format("{0}", AGEN_NO));
            p.Add("@MMCODE", string.Format("{0}", MMCODE));
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<WB_PUT_D>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //(TXTDAY格式為民國)
        public IEnumerable<WB_PUT_D> GetD_1(string agen_no, string mmcode, string txtday, string seq, string dept)
        {
            var sql = @"SELECT WPD.*,
                               (CASE
                                    WHEN WPD.EXTYPE = '10' THEN '取回'
                                    WHEN WPD.EXTYPE = '20' THEN '耗用'
                                    WHEN WPD.EXTYPE = '31' THEN '寄放'
                                END) EXTYPE_NAME,
                               REPLACE(REPLACE(REPLACE(SUBSTRING(CONVERT(VARCHAR, WPD.TXTDAY, 120),0,17),'-',''),' ',''),':','') - 191100000000 TXTDAY_TEXT,
                               REPLACE(REPLACE(REPLACE(SUBSTRING(CONVERT(VARCHAR, WPD.TXTDAY, 120),0,17),'-',''),' ',''),':','') - 191100000000 TXTDAY_DISPLAY,
                               (CASE
                                    WHEN WPD.EXTYPE = '10' THEN '10 取回'
                                    WHEN WPD.EXTYPE = '20' THEN '20 耗用'
                                    WHEN WPD.EXTYPE = '31' THEN '31 寄放'
                                END) EXTYPE_TEXT,
                                WPD.QTY QTY_DISPLAY,
                               (CASE
                                    WHEN WPD.STATUS = 'A' THEN '未處理'
                                    WHEN WPD.STATUS = 'B' THEN '已轉檔'
                                END) STATUS_NAME,
                               (CASE
                                    WHEN WPD.STATUS = 'A' THEN 'A 未處理'
                                    WHEN WPD.STATUS = 'B' THEN 'B 已轉檔'
                                END) STATUS_TEXT
                        FROM WB_PUT_D WPD
                        WHERE WPD.AGEN_NO = @AGEN_NO 
                        AND WPD.MMCODE = @MMCODE
                        AND WPD.TXTDAY = CONVERT(DATETIME, STUFF(STUFF(STUFF((CONVERT(VARCHAR,@TXTDAY_TEXT+191100000000)+'00'), 9, 0, ' '), 12, 0, ':'), 15, 0, ':'))
                        AND WPD.SEQ = @SEQ
                        AND WPD.DEPT = @DEPT";
            return DBWork.Connection.Query<WB_PUT_D>(sql, new { AGEN_NO = agen_no, MMCODE = mmcode, TXTDAY_TEXT = txtday, SEQ = seq, DEPT = dept }, DBWork.Transaction);
        }

        //(TXTDAY格式為西元)
        public IEnumerable<WB_PUT_D> GetD_2(string agen_no, string mmcode, string txtday, string seq, string dept)
        {
            var sql = @"SELECT WPD.*,
                               (CASE
                                    WHEN WPD.EXTYPE = '10' THEN '取回'
                                    WHEN WPD.EXTYPE = '20' THEN '耗用'
                                    WHEN WPD.EXTYPE = '31' THEN '寄放'
                                END) EXTYPE_NAME,
                               REPLACE(REPLACE(REPLACE(SUBSTRING(CONVERT(VARCHAR, WPD.TXTDAY, 120),0,17),'-',''),' ',''),':','') - 191100000000 TXTDAY_TEXT,
                               REPLACE(REPLACE(REPLACE(SUBSTRING(CONVERT(VARCHAR, WPD.TXTDAY, 120),0,17),'-',''),' ',''),':','') - 191100000000 TXTDAY_DISPLAY,
                               (CASE
                                    WHEN WPD.EXTYPE = '10' THEN '10 取回'
                                    WHEN WPD.EXTYPE = '20' THEN '20 耗用'
                                    WHEN WPD.EXTYPE = '31' THEN '31 寄放'
                                END) EXTYPE_TEXT,
                                WPD.QTY QTY_DISPLAY,
                               (CASE
                                    WHEN WPD.STATUS = 'A' THEN '未處理'
                                    WHEN WPD.STATUS = 'B' THEN '已轉檔'
                                END) STATUS_NAME,
                               (CASE
                                    WHEN WPD.STATUS = 'A' THEN 'A 未處理'
                                    WHEN WPD.STATUS = 'B' THEN 'B 已轉檔'
                                END) STATUS_TEXT
                        FROM WB_PUT_D WPD
                        WHERE WPD.AGEN_NO = @AGEN_NO 
                        AND WPD.MMCODE = @MMCODE
                        AND WPD.TXTDAY = CONVERT ( DATETIME , @TXTDAY + ' ' + SUBSTRING ( CONVERT ( CHAR , SYSDATETIME() , 120 ) , 12, 5) , 120)
                        AND WPD.SEQ = @SEQ
                        AND WPD.DEPT = @DEPT ";
            return DBWork.Connection.Query<WB_PUT_D>(sql, new { AGEN_NO = agen_no, MMCODE = mmcode, TXTDAY = txtday, SEQ = seq, DEPT = dept }, DBWork.Transaction);
        }

        //新增Detail
        public int CreateD(WB_PUT_D WB_PUT_D)
        {
            var sql = @"INSERT INTO WB_PUT_D (AGEN_NO,
                                              MMCODE,
                                              TXTDAY,
                                              SEQ,
                                              DEPT,
                                              EXTYPE,
                                              MEMO,
                                              QTY,
                                              STATUS,
                                              CREATE_TIME,
                                              CREATE_USER,
                                              UPDATE_TIME,
                                              UPDATE_USER,
                                              UPDATE_IP,
                                              FLAG)
                        VALUES ( @AGEN_NO,
                                 @MMCODE,
                                 CONVERT ( DATETIME , @TXTDAY + ' ' + SUBSTRING ( CONVERT ( CHAR , SYSDATETIME() , 120 ) , 12, 5) , 120),
                                 (SELECT  ISNULL( AA.SEQ , '0' ) + 1 SEQ
                                  FROM( SELECT MAX (WPD.SEQ) SEQ
                                        FROM WB_PUT_D WPD
                                        WHERE     WPD.AGEN_NO = @AGEN_NO
                                        AND WPD.MMCODE = @MMCODE
                                        AND WPD.TXTDAY = CONVERT ( DATETIME , @TXTDAY + ' ' + SUBSTRING ( CONVERT ( CHAR , SYSDATETIME() , 120 ) , 12, 5) , 120)) AA),
                                 @DEPT,
                                 @EXTYPE,
                                 @MEMO,
                                 @QTY,
                                 'A',
                                 SYSDATETIME(),
                                 @CREATE_USER,
                                 SYSDATETIME(),
                                 @UPDATE_USER,
                                 @UPDATE_IP,
                                 'A')";
            return DBWork.Connection.Execute(sql, WB_PUT_D, DBWork.Transaction);
        }

        //更新master的現有寄放量(減帳)
        public int UpdateM_Qty10(string AGEN_NO, string MMCODE, string DEPT, string QTY, string UPDATE_USER, string UPDATE_IP)
        {
            var sql = @"UPDATE WB_PUT_M SET
                               QTY = QTY - @QTY,
                               UPDATE_TIME = SYSDATETIME(), UPDATE_USER = @UPDATE_USER, UPDATE_IP = @UPDATE_IP
                        WHERE AGEN_NO = @AGEN_NO
                        AND MMCODE = @MMCODE
                        AND DEPT = @DEPT";

            return DBWork.Connection.Execute(sql, new { AGEN_NO = AGEN_NO, MMCODE = MMCODE, DEPT = DEPT, QTY = QTY, UPDATE_USER = UPDATE_USER, UPDATE_IP = UPDATE_IP }, DBWork.Transaction);
        }

        //更新master的現有寄放量(加帳)
        public int UpdateM_Qty31(string AGEN_NO, string MMCODE, string DEPT, string QTY, string UPDATE_USER, string UPDATE_IP)
        {
            var sql = @"UPDATE WB_PUT_M SET
                               QTY = QTY + @QTY,
                               UPDATE_TIME = SYSDATETIME(), UPDATE_USER = @UPDATE_USER, UPDATE_IP = @UPDATE_IP
                        WHERE AGEN_NO = @AGEN_NO
                        AND MMCODE = @MMCODE
                        AND DEPT = @DEPT";

            return DBWork.Connection.Execute(sql, new { AGEN_NO = AGEN_NO, MMCODE = MMCODE, DEPT = DEPT, QTY = QTY, UPDATE_USER = UPDATE_USER, UPDATE_IP = UPDATE_IP }, DBWork.Transaction);
        }

        //修改Detail
        public int UpdateD(WB_PUT_D WB_PUT_D)
        {
            var sql = @"UPDATE WB_PUT_D SET 
                                  EXTYPE = @EXTYPE,
                                  MEMO = @MEMO,
                                  STATUS = 'A',
                                  FLAG = 'A',
                                  UPDATE_TIME = SYSDATETIME(), UPDATE_USER = @UPDATE_USER, UPDATE_IP = @UPDATE_IP
                        WHERE AGEN_NO = @AGEN_NO
                        AND MMCODE = @MMCODE
                        AND TXTDAY = CONVERT(DATETIME, STUFF(STUFF(STUFF((CONVERT(VARCHAR,@TXTDAY_TEXT+191100000000)+'00'), 9, 0, ' '), 12, 0, ':'), 15, 0, ':'))
                        AND SEQ = @SEQ
                        AND DEPT = @DEPT";
            return DBWork.Connection.Execute(sql, WB_PUT_D, DBWork.Transaction);
        }

        //刪除Detail
        public int DeleteD(WB_PUT_D WB_PUT_D)
        {
            var sql = @"DELETE WB_PUT_D
                        WHERE AGEN_NO = :AGEN_NO
                        AND MMCODE = :MMCODE
                        AND TXTDAY = CONVERT(DATETIME, STUFF(STUFF(STUFF((CONVERT(VARCHAR,@TXTDAY_TEXT+191100000000)+'00'), 9, 0, ' '), 12, 0, ':'), 15, 0, ':'))
                        AND SEQ = @SEQ
                        AND DEPT = @DEPT";
            return DBWork.Connection.Execute(sql, WB_PUT_D, DBWork.Transaction);
        }

        //檢查Detail是否存在(TXTDAY格式為西元)
        public bool CheckExistsD_1(string agen_no, string mmcode, string txtday, string seq, string dept)
        {
            string sql = @"SELECT 1
                           FROM WB_PUT_D WPD
                           WHERE WPD.AGEN_NO = @AGEN_NO
                           AND WPD.MMCODE = @MMCODE
                           AND WPD.TXTDAY = CONVERT ( DATETIME , @TXTDAY + ' ' + SUBSTRING ( CONVERT ( CHAR , SYSDATETIME() , 120 ) , 12, 5) , 120)
                           AND WPD.SEQ = @SEQ
                           AND WPD.DEPT = @DEPT";
            return !(DBWork.Connection.ExecuteScalar(sql, new { AGEN_NO = agen_no, MMCODE = mmcode, TXTDAY = txtday, SEQ = seq, DEPT = dept }, DBWork.Transaction) == null);
        }

        //檢查Detail是否存在(TXTDAY格式為民國)
        public bool CheckExistsD_2(string agen_no, string mmcode, string txtday, string seq, string dept)
        {
            string sql = @"SELECT 1
                           FROM WB_PUT_D WPD
                           WHERE WPD.AGEN_NO = @AGEN_NO
                           AND WPD.MMCODE = @MMCODE
                           AND WPD.TXTDAY = CONVERT(DATETIME, STUFF(STUFF(STUFF((CONVERT(VARCHAR,@TXTDAY_TEXT+191100000000)+'00'), 9, 0, ' '), 12, 0, ':'), 15, 0, ':'))
                           AND WPD.SEQ = @SEQ
                           AND WPD.DEPT = @DEPT";
            return !(DBWork.Connection.ExecuteScalar(sql, new { AGEN_NO = agen_no, MMCODE = mmcode, TXTDAY = txtday, SEQ = seq, DEPT = dept }, DBWork.Transaction) == null);
        }

        //確認回傳
        public int CONFIRM_RETURN(string AGEN_NO, string MMCODE, string TXTDAY, string SEQ, string DEPT, string UPDATE_USER, string UPDATE_IP)
        {
            var sql = @"UPDATE WB_PUT_D SET 
                                  STATUS = 'B',
                                  FLAG = 'A',
                                  UPDATE_TIME = SYSDATETIME(), UPDATE_USER = @UPDATE_USER, UPDATE_IP = @UPDATE_IP
                        WHERE AGEN_NO = @AGEN_NO
                        AND MMCODE = @MMCODE
                        AND TXTDAY = CONVERT(DATETIME, STUFF(STUFF(STUFF((CONVERT(VARCHAR,@TXTDAY+191100000000)+'00'), 9, 0, ' '), 12, 0, ':'), 15, 0, ':'))
                        AND SEQ = @SEQ
                        AND DEPT = @DEPT";
            return DBWork.Connection.Execute(sql, new { AGEN_NO = AGEN_NO, MMCODE = MMCODE, TXTDAY = TXTDAY, SEQ = SEQ, DEPT = DEPT, UPDATE_USER = UPDATE_USER, UPDATE_IP = UPDATE_IP }, DBWork.Transaction);
        }

        //廠商碼combox
        public string GetAGEN_NO(string userid)
        {
            string sql = @"SELECT AGEN_NO + ' ' + RTRIM(AGEN_NAMEC) TEXT
                           FROM PH_VENDER
						   WHERE AGEN_NO = @USERID";
            return DBWork.Connection.ExecuteScalar(sql, new { USERID = userid }, DBWork.Transaction).ToString();
        }

        //院內碼combox
        public IEnumerable<MMCODECombo> GetMMCODE()
        {
            string sql = @"SELECT MMT.MMCODE VALUE,
                                  MMT.MMCODE || ' ' || TRIM(MMT.MMNAME_C) TEXT,
                                  TRIM(MMT.MMNAME_C) MMNAME_C_TEXT,
                                  TRIM(MMT.MMNAME_E) MMNAME_E_TEXT
                           FROM MI_MAST MMT
                           WHERE E_SOURCECODE = 'C'
                           ORDER BY MMT.MMCODE";
            return DBWork.Connection.Query<MMCODECombo>(sql, DBWork.Transaction);
        }

        public DataTable GetExcel(string AGEN_NO, string MMCODE, string MMNAME_C, string MMNAME_E)
        {
            DynamicParameters p = new DynamicParameters();

            var sql = @"SELECT ROW_NUMBER() OVER(ORDER BY AA.院內碼) 項次,AA.*
                        FROM (SELECT (SELECT PVR.AGEN_NO + ' ' + RTRIM (PVR.AGEN_NAMEC)
                                      FROM PH_VENDER PVR
                                      WHERE PVR.AGEN_NO = WPM.AGEN_NO) 廠商碼,
                                     WPM.MMCODE 院內碼,
                                     RTRIM (WPM.MMNAME_C) 中文品名,
                                     RTRIM (WPM.MMNAME_E) 英文品名,
                                     WPM.DEPT + ' ' + WPM.DEPTNAME 寄放地點,
                                     WPM.QTY 現有寄放量,
                                     WPM.MEMO 備註,
                                     (CASE
                                      WHEN WPM.STATUS = 'A' THEN '寄放中'
                                      WHEN WPM.STATUS = 'D' THEN '刪除'
                                     END) 狀態
                              FROM WB_PUT_M WPM
                              WHERE 1 = 1";

            sql += @" AND WPM.AGEN_NO = @p0 ";
            p.Add("@p0", string.Format("{0}", AGEN_NO));

            if (MMCODE != "")
            {
                sql += @" AND WPM.MMCODE LIKE @p1 ";
                p.Add("@p1", string.Format("{0}%", MMCODE));
            }

            if (MMNAME_C != "")
            {
                sql += @" AND WPM.MMNAME_C LIKE @p2 ";
                p.Add("@p2", string.Format("{0}%", MMNAME_C));
            }

            if (MMNAME_E != "")
            {
                sql += @" AND WPM.MMNAME_E LIKE @p3 ";
                p.Add("@p3", string.Format("{0}%", MMNAME_E));
            }

            sql += @"  ) AA 
						 ORDER BY AA.院內碼";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT FROM MI_MAST A WHERE 1=1 ";


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

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
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
            public string MAT_CLASS;

            public string WH_NO;
            public string IS_INV;  // 需判斷庫存量>0
            public string E_IFPUBLIC;  // 是否公藥
        }

        public IEnumerable<MI_MAST> GetMmcode(MI_MAST_QUERY_PARAMS query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            string sql = @"SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.M_CONTPRICE, A.BASE_UNIT FROM MI_MAST A WHERE 1=1 ";


            if (query.MMCODE != "")
            {
                sql += " AND A.MMCODE LIKE @MMCODE ";
                p.Add(":MMCODE", string.Format("%{0}%", query.MMCODE));
            }

            if (query.MMNAME_C != "")
            {
                sql += " AND A.MMNAME_C LIKE @MMNAME_C ";
                p.Add(":MMNAME_C", string.Format("%{0}%", query.MMNAME_C));
            }

            if (query.MMNAME_E != "")
            {
                sql += " AND A.MMNAME_E LIKE @MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", query.MMNAME_E));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
    }
}
