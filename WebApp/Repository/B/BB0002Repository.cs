using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.B
{
    public class BB0002Repository : JCLib.Mvc.BaseRepository
    {
        public BB0002Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        //查詢Master
        public IEnumerable<PH_PUT_M> GetAllM(string AGEN_NO, string MMCODE, string MMNAME_C, string MMNAME_E, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT PPM.AGEN_NO,
                               (SELECT PVR.AGEN_NO || ' ' || TRIM (PVR.AGEN_NAMEC)
                                FROM PH_VENDER PVR
                                WHERE PVR.AGEN_NO = PPM.AGEN_NO) AGEN_NAMEC,
                               (SELECT PVR.AGEN_NO || ' ' || TRIM (PVR.AGEN_NAMEC)
                                FROM PH_VENDER PVR
                                WHERE PVR.AGEN_NO = PPM.AGEN_NO) AGEN_NAMEC_DISPLAY,
                               (SELECT PVR.AGEN_NO || ' ' || TRIM (PVR.AGEN_NAMEC)
                                FROM PH_VENDER PVR
                                WHERE PVR.AGEN_NO = PPM.AGEN_NO) AGEN_NAMEC_TEXT,
                               PPM.MMCODE,
                               PPM.MMCODE MMCODE_DISPLAY,
                               PPM.MMCODE MMCODE_TEXT,
                               PPM.MEMO,
                               PPM.QTY,
                               PPM.QTY QTY_DISPLAY,
                               TRIM (PPM.MMNAME_C) MMNAME_C,
                               TRIM (PPM.MMNAME_E) MMNAME_E,
                               (CASE
                                    WHEN PPM.STATUS = 'A' THEN '寄放中'
                                    WHEN PPM.STATUS = 'D' THEN '刪除'
                               END) STATUS_NAME,
                               (CASE
                                    WHEN PPM.STATUS = 'A' THEN 'A 寄放中'
                                    WHEN PPM.STATUS = 'D' THEN 'D 刪除'
                               END) STATUS_TEXT,
                               (SELECT PPM.DEPT || ' ' || TRIM (URID.INID_NAME)
                                FROM UR_INID URID
                                WHERE URID.INID = PPM.DEPT) DEPT_NAME,
                               (SELECT PPM.DEPT || ' ' || TRIM (URID.INID_NAME)
                                FROM UR_INID URID
                                WHERE URID.INID = PPM.DEPT) DEPT_NAME_DISPLAY,
                               (SELECT PPM.DEPT || ' ' || TRIM (URID.INID_NAME)
                                FROM UR_INID URID
                                WHERE URID.INID = PPM.DEPT) DEPT_NAME_TEXT,
                               TRIM (PPM.DEPTNAME) DEPTNAME,
                               PPM.DEPT,
                               PPM.STATUS
                        FROM PH_PUT_M PPM
                        WHERE 1 = 1";

            if (AGEN_NO != "")
            {
                sql += @" AND PPM.AGEN_NO = :p0 ";
                p.Add(":p0", string.Format("{0}", AGEN_NO));
            }

            if (MMCODE != "")
            {
                sql += @" AND PPM.MMCODE LIKE :p1 ";
                p.Add(":p1", string.Format("{0}%", MMCODE));
            }

            if (MMNAME_C != "")
            {
                sql += @" AND PPM.MMNAME_C LIKE :p2 ";
                p.Add(":p2", string.Format("{0}%", MMNAME_C));
            }

            if (MMNAME_E != "")
            {
                sql += @" AND PPM.MMNAME_E LIKE :p3 ";
                p.Add(":p3", string.Format("{0}%", MMNAME_E));
            }

            sql += @"ORDER BY PPM.AGEN_NO, PPM.MMCODE";
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<PH_PUT_M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //查詢Detail
        public IEnumerable<PH_PUT_D> GetAllD(string AGEN_NO, string MMCODE, string DEPT, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT PPD.*,
                               (CASE
                                    WHEN PPD.EXTYPE = '10' THEN '取回'
                                    WHEN PPD.EXTYPE = '20' THEN '耗用'
                                    WHEN PPD.EXTYPE = '31' THEN '寄放'
                                END) EXTYPE_NAME,
                               TO_CHAR ( TO_NUMBER (TO_CHAR (PPD.TXTDAY, 'YYYYMMDDHH24MI')) - 191100000000) TXTDAY_TEXT,
                               TO_CHAR ( TO_NUMBER (TO_CHAR (PPD.TXTDAY, 'YYYYMMDDHH24MI')) - 191100000000) TXTDAY_DISPLAY,
                               (CASE
                                    WHEN PPD.EXTYPE = '10' THEN '10 取回'
                                    WHEN PPD.EXTYPE = '20' THEN '20 耗用'
                                    WHEN PPD.EXTYPE = '31' THEN '31 寄放'
                                END) EXTYPE_TEXT,
                                PPD.QTY QTY_DISPLAY,
                               (CASE
                                    WHEN PPD.STATUS = 'A' THEN '未處理'
                                    WHEN PPD.STATUS = 'B' THEN '已轉檔'
                                END) STATUS_NAME,
                               (CASE
                                    WHEN PPD.STATUS = 'A' THEN 'A 未處理'
                                    WHEN PPD.STATUS = 'B' THEN 'B 已轉檔'
                                END) STATUS_TEXT
                        FROM PH_PUT_D PPD
                        WHERE PPD.AGEN_NO = :AGEN_NO 
                        AND PPD.MMCODE = :MMCODE
                        AND PPD.DEPT = :DEPT
                        ORDER BY PPD.TXTDAY DESC";

            p.Add(":AGEN_NO", string.Format("{0}", AGEN_NO));
            p.Add(":MMCODE", string.Format("{0}", MMCODE));
            p.Add(":DEPT", string.Format("{0}", DEPT));
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<PH_PUT_D>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<PH_PUT_M> GetM(string agen_no, string mmcode, string dept)
        {
            var sql = @"SELECT PPM.AGEN_NO,
                               (SELECT PVR.AGEN_NO || ' ' || TRIM (PVR.AGEN_NAMEC)
                                FROM PH_VENDER PVR
                                WHERE PVR.AGEN_NO = PPM.AGEN_NO) AGEN_NAMEC,
                               (SELECT PVR.AGEN_NO || ' ' || TRIM (PVR.AGEN_NAMEC)
                                FROM PH_VENDER PVR
                                WHERE PVR.AGEN_NO = PPM.AGEN_NO) AGEN_NAMEC_DISPLAY,
                               (SELECT PVR.AGEN_NO || ' ' || TRIM (PVR.AGEN_NAMEC)
                                FROM PH_VENDER PVR
                                WHERE PVR.AGEN_NO = PPM.AGEN_NO) AGEN_NAMEC_TEXT,
                               PPM.MMCODE,
                               PPM.MMCODE || ' ' || TRIM (PPM.MMNAME_C) MMCODE_DISPLAY,
                               PPM.MMCODE || ' ' || TRIM (PPM.MMNAME_C) MMCODE_TEXT,
                               PPM.MEMO,
                               PPM.QTY,
                               PPM.QTY QTY_DISPLAY,
                               TRIM (PPM.MMNAME_C) MMNAME_C,
                               TRIM (PPM.MMNAME_E) MMNAME_E,
                               (CASE
                                    WHEN PPM.STATUS = 'A' THEN '寄放中'
                                    WHEN PPM.STATUS = 'D' THEN '刪除'
                               END) STATUS_NAME,
                               (CASE
                                    WHEN PPM.STATUS = 'A' THEN '寄放中'
                                    WHEN PPM.STATUS = 'D' THEN '刪除'
                               END) STATUS_TEXT,
                               (SELECT PPM.DEPT || ' ' || TRIM (URID.INID_NAME)
                                FROM UR_INID URID
                                WHERE URID.INID = PPM.DEPT) DEPT_NAME,
                               (SELECT PPM.DEPT || ' ' || TRIM (URID.INID_NAME)
                                FROM UR_INID URID
                                WHERE URID.INID = PPM.DEPT) DEPT_NAME_DISPLAY,
                               (SELECT PPM.DEPT || ' ' || TRIM (URID.INID_NAME)
                                FROM UR_INID URID
                                WHERE URID.INID = PPM.DEPT) DEPT_NAME_TEXT,
                               TRIM (PPM.DEPTNAME) DEPTNAME,
                               PPM.DEPT,
                               PPM.STATUS
                        FROM PH_PUT_M PPM
                        WHERE PPM.AGEN_NO = :AGEN_NO
                        AND PPM.MMCODE = :MMCODE
                        AND PPM.DEPT = :DEPT";
            return DBWork.Connection.Query<PH_PUT_M>(sql, new { AGEN_NO = agen_no, MMCODE = mmcode, DEPT = dept }, DBWork.Transaction);
        }

        //(TXTDAY格式為民國)
        public IEnumerable<PH_PUT_D> GetD_1(string agen_no, string mmcode, string txtday, string seq, string dept)
        {
            var sql = @"SELECT PPD.*,
                               (CASE
                                    WHEN PPD.EXTYPE = '10' THEN '取回'
                                    WHEN PPD.EXTYPE = '20' THEN '耗用'
                                    WHEN PPD.EXTYPE = '31' THEN '寄放'
                                END) EXTYPE_NAME,
                               TO_CHAR ( TO_NUMBER (TO_CHAR (PPD.TXTDAY, 'YYYYMMDDHH24MI')) - 191100000000) TXTDAY_TEXT,
                               TO_CHAR ( TO_NUMBER (TO_CHAR (PPD.TXTDAY, 'YYYYMMDDHH24MI')) - 191100000000) TXTDAY_DISPLAY,
                               (CASE
                                    WHEN PPD.EXTYPE = '10' THEN '10 取回'
                                    WHEN PPD.EXTYPE = '20' THEN '20 耗用'
                                    WHEN PPD.EXTYPE = '31' THEN '31 寄放'
                                END) EXTYPE_TEXT,
                                PPD.QTY QTY_DISPLAY,
                               (CASE
                                    WHEN PPD.STATUS = 'A' THEN '未處理'
                                    WHEN PPD.STATUS = 'B' THEN '已轉檔'
                                END) STATUS_NAME,
                               (CASE
                                    WHEN PPD.STATUS = 'A' THEN 'A 未處理'
                                    WHEN PPD.STATUS = 'B' THEN 'B 已轉檔'
                                END) STATUS_TEXT
                        FROM PH_PUT_D PPD
                        WHERE PPD.AGEN_NO = :AGEN_NO 
                        AND PPD.MMCODE = :MMCODE
                        AND PPD.TXTDAY = TO_DATE((:TXTDAY+191100000000),'YYYY/MM/DD HH24:MI:SS')
                        AND PPD.SEQ = :SEQ
                        AND PPD.DEPT = :DEPT";
            return DBWork.Connection.Query<PH_PUT_D>(sql, new { AGEN_NO = agen_no, MMCODE = mmcode, TXTDAY = txtday, SEQ = seq, DEPT = dept }, DBWork.Transaction);
        }

        //(TXTDAY格式為西元)
        public IEnumerable<PH_PUT_D> GetD_2(string agen_no, string mmcode, string txtday, string seq, string dept)
        {
            var sql = @"SELECT PPD.*,
                               (CASE
                                    WHEN PPD.EXTYPE = '10' THEN '取回'
                                    WHEN PPD.EXTYPE = '20' THEN '耗用'
                                    WHEN PPD.EXTYPE = '31' THEN '寄放'
                                END) EXTYPE_NAME,
                               TO_CHAR ( TO_NUMBER (TO_CHAR (PPD.TXTDAY, 'YYYYMMDDHH24MI')) - 191100000000) TXTDAY_TEXT,
                               TO_CHAR ( TO_NUMBER (TO_CHAR (PPD.TXTDAY, 'YYYYMMDDHH24MI')) - 191100000000) TXTDAY_DISPLAY,
                               (CASE
                                    WHEN PPD.EXTYPE = '10' THEN '10 取回'
                                    WHEN PPD.EXTYPE = '20' THEN '20 耗用'
                                    WHEN PPD.EXTYPE = '31' THEN '31 寄放'
                                END) EXTYPE_TEXT,
                                PPD.QTY QTY_DISPLAY,
                               (CASE
                                    WHEN PPD.STATUS = 'A' THEN '未處理'
                                    WHEN PPD.STATUS = 'B' THEN '已轉檔'
                                END) STATUS_NAME,
                               (CASE
                                    WHEN PPD.STATUS = 'A' THEN 'A 未處理'
                                    WHEN PPD.STATUS = 'B' THEN 'B 已轉檔'
                                END) STATUS_TEXT
                        FROM PH_PUT_D PPD
                        WHERE PPD.AGEN_NO = :AGEN_NO 
                        AND PPD.MMCODE = :MMCODE
                        AND PPD.TXTDAY = TO_DATE((:TXTDAY || to_char(SYSDATE,'HH24MI')),'YYYY/MM/DDHH24MI')
                        AND PPD.SEQ = :SEQ
                        AND PPD.DEPT = :DEPT";
            return DBWork.Connection.Query<PH_PUT_D>(sql, new { AGEN_NO = agen_no, MMCODE = mmcode, TXTDAY = txtday, SEQ = seq, DEPT = dept }, DBWork.Transaction);
        }

        //新增Master
        public int CreateM(PH_PUT_M PH_PUT_M)
        {
            var sql = @"INSERT INTO PH_PUT_M (
                              AGEN_NO, MMCODE, DEPT, DEPTNAME, MEMO, MMNAME_C, MMNAME_E, QTY, STATUS, CREATE_TIME,
                              CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, FLAG)  
                        VALUES (
                              :AGEN_NO, :MMCODE, :DEPT, :DEPTNAME, :MEMO, :MMNAME_C, :MMNAME_E, :QTY, :STATUS, SYSDATE,
                              :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP, 'A')";
            return DBWork.Connection.Execute(sql, PH_PUT_M, DBWork.Transaction);
        }

        //修改Master
        public int UpdateM(PH_PUT_M PH_PUT_M)
        {
            var sql = @"UPDATE PH_PUT_M SET 
                               DEPTNAME = :DEPTNAME,
                               MEMO = :MEMO,
                               STATUS = :STATUS,
                               FLAG = 'A',
                               UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                        WHERE AGEN_NO = :AGEN_NO
                        AND MMCODE = :MMCODE
                        AND DEPT = :DEPT";
            return DBWork.Connection.Execute(sql, PH_PUT_M, DBWork.Transaction);
        }

        //刪除Master
        public int DeleteM(PH_PUT_M PH_PUT_M)
        {
            var sql = @"DELETE PH_PUT_M PPM
                        WHERE PPM.AGEN_NO = :AGEN_NO
                        AND PPM.MMCODE = :MMCODE
                        AND PPM.DEPT = :DEPT ";
            return DBWork.Connection.Execute(sql, PH_PUT_M, DBWork.Transaction);
        }

        //新增Detail
        public int CreateD(PH_PUT_D PH_PUT_D)
        {
            var sql = @"INSERT INTO PH_PUT_D (AGEN_NO,
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
                        VALUES ( :AGEN_NO,
                                 :MMCODE,
                                 TO_DATE ( ( :TXTDAY || TO_CHAR (SYSDATE, 'HH24MI')),  'YYYY/MM/DDHH24MI'),
                                 PH_PUT_D_SEQ.NEXTVAL,
                                 :DEPT,
                                 :EXTYPE,
                                 :MEMO,
                                 :QTY,
                                 :STATUS,
                                 SYSDATE,
                                 :CREATE_USER,
                                 SYSDATE,
                                 :UPDATE_USER,
                                 :UPDATE_IP,
                                 'A')";
            return DBWork.Connection.Execute(sql, PH_PUT_D, DBWork.Transaction);
        }
        public int UpdateM_Qty(PH_PUT_D PH_PUT_D)
        {
            var sql = @"UPDATE PH_PUT_M SET
                               QTY = QTY - :QTY,
                               UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                        WHERE AGEN_NO = :AGEN_NO
                        AND MMCODE = :MMCODE
                        AND DEPT = :DEPT";

            return DBWork.Connection.Execute(sql, PH_PUT_D, DBWork.Transaction);
        }

        //修改Detail
        public int UpdateD(PH_PUT_D PH_PUT_D)
        {
            var sql = @"UPDATE PH_PUT_D SET 
                                  EXTYPE = :EXTYPE,
                                  MEMO = :MEMO,
                                  STATUS = :STATUS,
                                  FLAG = 'A',
                                  UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                        WHERE AGEN_NO = :AGEN_NO
                        AND MMCODE = :MMCODE
                        AND TXTDAY = TO_DATE((:TXTDAY_TEXT+191100000000),'YYYY/MM/DD HH24:MI:SS')
                        AND SEQ = :SEQ
                        AND DEPT = :DEPT";
            return DBWork.Connection.Execute(sql, PH_PUT_D, DBWork.Transaction);
        }

        //刪除Detail
        public int DeleteD(PH_PUT_D PH_PUT_D)
        {
            var sql = @"DELETE PH_PUT_D
                        WHERE AGEN_NO = :AGEN_NO
                        AND MMCODE = :MMCODE
                        AND TXTDAY = TO_DATE((:TXTDAY+191100000000),'YYYY/MM/DD HH24:MI:SS') 
                        AND DEPT = :DEPT";
            return DBWork.Connection.Execute(sql, PH_PUT_D, DBWork.Transaction);
        }

        //檢查Master是否存在
        public bool CheckExistsM(string agen_no, string mmcode, string dept)
        {
            string sql = @"SELECT 1
                           FROM PH_PUT_M PPM
                           WHERE PPM.AGEN_NO = :AGEN_NO
                           AND PPM.MMCODE = :MMCODE
                           AND PPM.DEPT = :DEPT";
            return !(DBWork.Connection.ExecuteScalar(sql, new { AGEN_NO = agen_no, MMCODE = mmcode, DEPT = dept }, DBWork.Transaction) == null);
        }

        //檢查Detail是否存在(TXTDAY格式為西元)
        public bool CheckExistsD_1(string agen_no, string mmcode, string txtday, string seq, string dept)
        {
            string sql = @"SELECT 1
                           FROM PH_PUT_D PPD
                           WHERE PPD.AGEN_NO = :AGEN_NO
                           AND PPD.MMCODE = :MMCODE
                           AND PPD.TXTDAY = TO_DATE((:TXTDAY || to_char(SYSDATE,'HH24MI')),'YYYY/MM/DDHH24MI')
                           AND PPD.SEQ = :SEQ
                           AND PPD.DEPT = :DEPT";
            return !(DBWork.Connection.ExecuteScalar(sql, new { AGEN_NO = agen_no, MMCODE = mmcode, TXTDAY = txtday, SEQ = seq, DEPT = dept }, DBWork.Transaction) == null);
        }

        //檢查Detail是否存在(TXTDAY格式為民國)
        public bool CheckExistsD_2(string agen_no, string mmcode, string txtday, string seq, string dept)
        {
            string sql = @"SELECT 1
                           FROM PH_PUT_D PPD
                           WHERE PPD.AGEN_NO = :AGEN_NO
                           AND PPD.MMCODE = :MMCODE
                           AND PPD.TXTDAY = TO_DATE((:TXTDAY_TEXT+191100000000),'YYYY/MM/DD HH24:MI:SS')
                           AND PPD.SEQ = :SEQ
                           AND PPD.DEPT = :DEPT";
            return !(DBWork.Connection.ExecuteScalar(sql, new { AGEN_NO = agen_no, MMCODE = mmcode, TXTDAY = txtday, SEQ = seq, DEPT = dept }, DBWork.Transaction) == null);
        }

        //廠商碼combox
        public IEnumerable<ComboItemModel> GetAGEN_NO()
        {
            string sql = @"SELECT AGEN_NO VALUE,
                                  AGEN_NO || ' ' || TRIM(AGEN_NAMEC) TEXT
                           FROM PH_VENDER
                           ORDER BY AGEN_NO";
            return DBWork.Connection.Query<ComboItemModel>(sql, DBWork.Transaction);
        }

        //寄放責任中心combox
        public IEnumerable<INIDNAMECombo> GetINIDNAME()
        {
            string sql = @"SELECT URID.INID VALUE,
                                  URID.INID || ' ' || TRIM(URID.INID_NAME) TEXT,
                                  TRIM(URID.INID_NAME) INIDNAME_TEXT
                           FROM UR_INID URID
                           ORDER BY URID.INID";
            return DBWork.Connection.Query<INIDNAMECombo>(sql, DBWork.Transaction);
        }

        //匯出
        public DataTable GetExcel(string AGEN_NO, string MMCODE, string MMNAME_C, string MMNAME_E)
        {
            DynamicParameters p = new DynamicParameters();

            var sql = @"SELECT ROWNUM 項次,AA.*
                        FROM (SELECT (SELECT PVR.AGEN_NO || ' ' || TRIM (PVR.AGEN_NAMEC)
                                FROM PH_VENDER PVR
                                WHERE PVR.AGEN_NO = PPM.AGEN_NO) 廠商碼,
                               PPM.MMCODE 院內碼,
                               TRIM (PPM.MMNAME_C) 中文品名,
                               TRIM (PPM.MMNAME_E) 英文品名,
                               (SELECT PPM.DEPT || ' ' || TRIM (URID.INID_NAME)
                                FROM UR_INID URID
                                WHERE URID.INID = PPM.DEPT) 寄放地點,
                               PPM.QTY 現有寄放量,
                               PPM.MEMO 備註,
                               (CASE
                                    WHEN PPM.STATUS = 'A' THEN '寄放中'
                                    WHEN PPM.STATUS = 'D' THEN '刪除'
                               END) 狀態
                        FROM PH_PUT_M PPM
                        WHERE 1 = 1";

            if (AGEN_NO != "")
            {
                sql += @" AND PPM.AGEN_NO = :p0 ";
                p.Add(":p0", string.Format("{0}", AGEN_NO));
            }

            if (MMCODE != "")
            {
                sql += @" AND PPM.MMCODE LIKE :p1 ";
                p.Add(":p1", string.Format("{0}%", MMCODE));
            }

            if (MMNAME_C != "")
            {
                sql += @" AND PPM.MMNAME_C LIKE :p2 ";
                p.Add(":p2", string.Format("{0}%", MMNAME_C));
            }

            if (MMNAME_E != "")
            {
                sql += @" AND PPM.MMNAME_E LIKE :p3 ";
                p.Add(":p3", string.Format("{0}%", MMNAME_E));
            }

            sql += @"  ORDER BY PPM.AGEN_NO, PPM.MMCODE) AA";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT FROM MI_MAST A WHERE 1=1 AND A.E_SOURCECODE = 'C' ";


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
            public string MAT_CLASS;

            public string WH_NO;
            public string IS_INV;  // 需判斷庫存量>0
            public string E_IFPUBLIC;  // 是否公藥
        }

        public IEnumerable<MI_MAST> GetMmcode(MI_MAST_QUERY_PARAMS query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            string sql = @"SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.M_CONTPRICE, A.BASE_UNIT FROM MI_MAST A WHERE 1=1 AND A.E_SOURCECODE = 'C' ";


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

        public string GetAgenno(string MMCODE)
        {
            var sql = @"SELECT PVR.AGEN_NO || ',' || PVR.AGEN_NO || ' ' || PVR.AGEN_NAMEC
                        FROM MI_MAST MMT, PH_VENDER PVR
                        WHERE MMT.M_AGENNO = PVR.AGEN_NO
                        AND MMT.MMCODE = :MMCODE";

            return DBWork.Connection.QueryFirst<string>(sql, new { MMCODE = MMCODE }, DBWork.Transaction);
        }

        //帳務明細匯出
        public DataTable GetExcel_D(string START_DATE, string END_DATE)
        {
            DynamicParameters p = new DynamicParameters();

            var sql = @"  SELECT PPM.AGEN_NO 廠商代碼,
                                 PVR.AGEN_NAMEC 廠商名稱,
                                 PPM.MMCODE 院內碼,
                                 PPM.MMNAME_C 中文品名,
                                 PPM.MMNAME_E 英文品名,
                                 (SELECT URID.INID || '_' || URID.INID_NAME
                                  FROM UR_INID URID
                                  WHERE URID.INID = PPM.DEPT) 寄放地點,
                                 PPD.TXTDAY 交易日期,
                                 (CASE
                                      WHEN PPD.EXTYPE = '10' THEN '取回'
                                      WHEN PPD.EXTYPE = '20' THEN '耗用'
                                      WHEN PPD.EXTYPE = '31' THEN '寄放'
                                  END) 交易類別,
                                 PPD.QTY 數量
                          FROM PH_PUT_M PPM, PH_PUT_D PPD, PH_VENDER PVR
                          WHERE     PPM.AGEN_NO = PPD.AGEN_NO
                          AND PPM.MMCODE = PPD.MMCODE
                          AND PPM.DEPT = PPD.DEPT
                          AND PPM.AGEN_NO = PVR.AGEN_NO";

            if (START_DATE != "")
            {
                sql += @" AND TRUNC (PPD.TXTDAY) >= TO_DATE ( :START_DATE + 19110000, 'YYYYMMDD')";
                p.Add(":START_DATE", string.Format("{0}", START_DATE));
            }

            if (END_DATE != "")
            {
                sql += @" AND TRUNC (PPD.TXTDAY) <= TO_DATE ( :END_DATE + 19110000, 'YYYYMMDD')";
                p.Add(":END_DATE", string.Format("{0}", END_DATE));
            }

            sql += @" ORDER BY PPM.AGEN_NO, PPM.DEPT, PPD.TXTDAY";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
    }
}
