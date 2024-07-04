using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using JCLib.Mvc;

namespace WebApp.Repository.AA
{
    public class AA0043Repository : JCLib.Mvc.BaseRepository
    {
        public AA0043Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        //查詢(S)
        public IEnumerable<MI_WLOCINV> GetAll_S(string WH_NO, string MMCODE, string STORE_LOC,string mat_class, int page_index, int page_size, string sorters, string wh_userId)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT MWV.*,
                               TRIM (MWT.WH_NAME) WH_NAME,
                               TRIM (MMT.MMNAME_C) MMNAME_C,
                               TRIM (MMT.MMNAME_E) MMNAME_E,
                               TRIM (MWV.WH_NO || ' ' || MWT.WH_NAME) WH_NAME_DISPLAY,
                               TRIM (MWV.WH_NO || ' ' || MWT.WH_NAME) WH_NAME_TEXT,
                               TRIM (MWV.MMCODE) MMCODE_DISPLAY,
                               TRIM (MWV.MMCODE) MMCODE_TEXT,
                               TRIM (MWV.STORE_LOC) STORE_LOC_DISPLAY,
                               TRIM (MWV.STORE_LOC) STORE_LOC_TEXT,
                               TRIM (MWV.LOC_NOTE) LOC_NOTE_TEXT
                        FROM MI_WLOCINV MWV, MI_WHMAST MWT, MI_MAST MMT
                        WHERE     MWV.WH_NO = MWT.WH_NO
                        AND MWV.MMCODE = MMT.MMCODE
                        AND EXISTS
                                  (SELECT WH_NO
                                   FROM (SELECT WH_NO
                                         FROM MI_WHID
                                         WHERE WH_USERID = :WH_USERID
                                         UNION
                                         SELECT WH_NO
                                         FROM MI_WHMAST
                                         WHERE     INID = USER_INID ( :WH_USERID)
                                         AND WH_KIND = '1') AA
                                   WHERE AA.WH_NO = MWV.WH_NO)
                        AND EXISTS
                                  (SELECT MMCODE
                                   FROM MI_WHINV
                                   WHERE WH_NO = MWV.WH_NO
                                   AND MMCODE = MWV.MMCODE) ";

            if (WH_NO != "")
            {
                sql += " AND MWV.WH_NO = :p0 ";
                p.Add(":p0", string.Format("{0}", WH_NO));
            }
            if (MMCODE != "")
            {
                sql += " AND MWV.MMCODE = :p1 ";
                p.Add(":p1", string.Format("{0}", MMCODE));
            }
            if (STORE_LOC != "")
            {
                sql += " AND MWV.STORE_LOC LIKE :p2 ";
                p.Add(":p2", string.Format("%{0}%", STORE_LOC));
            }
            if (mat_class != "")
            {
                sql += " AND MMT.MAT_CLASS = :p3 ";
                p.Add(":p3", string.Format("{0}", mat_class));
            }
            p.Add(":WH_USERID", string.Format("{0}", wh_userId));
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_WLOCINV>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //查詢(1)
        public IEnumerable<MI_WLOCINV> GetAll_1(string WH_NO, string MMCODE, string STORE_LOC, string mat_class, int page_index, int page_size, string sorters, string wh_userId)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT MWV.*,
                               TRIM (MWT.WH_NAME) WH_NAME,
                               TRIM (MMT.MMNAME_C) MMNAME_C,
                               TRIM (MMT.MMNAME_E) MMNAME_E,
                               TRIM (MWV.WH_NO || ' ' || MWT.WH_NAME) WH_NAME_DISPLAY,
                               TRIM (MWV.WH_NO || ' ' || MWT.WH_NAME) WH_NAME_TEXT,
                               TRIM (MWV.MMCODE) MMCODE_DISPLAY,
                               TRIM (MWV.MMCODE) MMCODE_TEXT,
                               TRIM (MWV.STORE_LOC) STORE_LOC_DISPLAY,
                               TRIM (MWV.STORE_LOC) STORE_LOC_TEXT,
                               TRIM (MWV.LOC_NOTE) LOC_NOTE_TEXT
                        FROM MI_WLOCINV MWV, MI_WHMAST MWT, MI_MAST MMT
                        WHERE     MWV.WH_NO = MWT.WH_NO
                        AND MWV.MMCODE = MMT.MMCODE
                        AND EXISTS
                                  (SELECT WH_NO
                                   FROM MI_WHID
                                   WHERE WH_USERID = :WH_USERID
                                   AND WH_NO = MWV.WH_NO)
                        AND EXISTS
                                  (SELECT MMCODE
                                   FROM MI_WHINV
                                   WHERE WH_NO = MWV.WH_NO
                                   AND MMCODE = MWV.MMCODE) ";

            if (WH_NO != "")
            {
                sql += " AND MWV.WH_NO = :p0 ";
                p.Add(":p0", string.Format("{0}", WH_NO));
            }
            if (MMCODE != "")
            {
                sql += " AND MWV.MMCODE = :p1 ";
                p.Add(":p1", string.Format("{0}", MMCODE));
            }
            if (STORE_LOC != "")
            {
                sql += " AND MWV.STORE_LOC LIKE :p2 ";
                p.Add(":p2", string.Format("%{0}%", STORE_LOC));
            }
            if (mat_class != "")
            {
                sql += " AND MMT.MAT_CLASS = :p3 ";
                p.Add(":p3", string.Format("{0}", mat_class));
            }
            p.Add(":WH_USERID", string.Format("{0}", wh_userId));
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_WLOCINV>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //查詢
        public IEnumerable<MI_WLOCINV> GetAll(string WH_NO, string MMCODE, string STORE_LOC, string mat_class, int page_index, int page_size, string sorters, string wh_userId)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT MWV.*,
                               TRIM (MWT.WH_NAME) WH_NAME,
                               TRIM (MMT.MMNAME_C) MMNAME_C,
                               TRIM (MMT.MMNAME_E) MMNAME_E,
                               TRIM (MWV.WH_NO || ' ' || MWT.WH_NAME) WH_NAME_DISPLAY,
                               TRIM (MWV.WH_NO || ' ' || MWT.WH_NAME) WH_NAME_TEXT,
                               TRIM (MWV.MMCODE) MMCODE_DISPLAY,
                               TRIM (MWV.MMCODE) MMCODE_TEXT,
                               TRIM (MWV.STORE_LOC) STORE_LOC_DISPLAY,
                               TRIM (MWV.STORE_LOC) STORE_LOC_TEXT,
                               TRIM (MWV.LOC_NOTE) LOC_NOTE_TEXT
                        FROM MI_WLOCINV MWV, MI_WHMAST MWT, MI_MAST MMT
                        WHERE     MWV.WH_NO = MWT.WH_NO
                        AND MWV.MMCODE = MMT.MMCODE
                        AND EXISTS
                                  (SELECT WH_NO
                                   FROM MI_WHID
                                   WHERE WH_USERID = :WH_USERID
                                   AND WH_NO = MWV.WH_NO)
                        AND EXISTS
                                  (SELECT MMCODE
                                   FROM MI_WHINV A
                                   WHERE     A.WH_NO = MWV.WH_NO
                                   AND A.MMCODE = MWV.MMCODE
                                   AND EXISTS
                                             (SELECT 1
                                              FROM MI_MAST
                                              WHERE     MMCODE = A.MMCODE
                                              AND MAT_CLASS IN (SELECT MAT_CLASS
                                                                FROM MI_MATCLASS
                                                                WHERE MAT_CLSID in ('2', '3')))) ";

            if (WH_NO != "")
            {
                sql += " AND MWV.WH_NO = :p0 ";
                p.Add(":p0", string.Format("{0}", WH_NO));
            }
            if (MMCODE != "")
            {
                sql += " AND MWV.MMCODE = :p1 ";
                p.Add(":p1", string.Format("{0}", MMCODE));
            }
            if (STORE_LOC != "")
            {
                sql += " AND MWV.STORE_LOC LIKE :p2 ";
                p.Add(":p2", string.Format("%{0}%", STORE_LOC));
            }
            if (mat_class != "")
            {
                sql += " AND MMT.MAT_CLASS = :p3 ";
                p.Add(":p3", string.Format("{0}", mat_class));
            }
            p.Add(":WH_USERID", string.Format("{0}", wh_userId));
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_WLOCINV>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MI_WLOCINV> Get(string wh_no, string mmcode, string store_loc)
        {
            var sql = @"SELECT MWV.*,
                               TRIM(MWT.WH_NAME) WH_NAME,
                               TRIM(MMT.MMNAME_C) MMNAME_C,
                               TRIM(MMT.MMNAME_E) MMNAME_E,
                               TRIM(MWV.WH_NO || ' ' || MWT.WH_NAME) WH_NAME_DISPLAY,
                               TRIM(MWV.WH_NO || ' ' || MWT.WH_NAME) WH_NAME_TEXT,
                               TRIM(MWV.MMCODE || ' ' || MMT.MMNAME_C) MMCODE_DISPLAY,
                               TRIM(MWV.MMCODE || ' ' || MMT.MMNAME_C) MMCODE_TEXT,
                               TRIM(MWV.STORE_LOC) STORE_LOC_DISPLAY,
                               TRIM(MWV.STORE_LOC) STORE_LOC_TEXT,
                               TRIM (MWV.LOC_NOTE) LOC_NOTE_TEXT
                        FROM MI_WHID MWD,
                             MI_WLOCINV MWV,
                             MI_WHMAST MWT,
                             MI_MAST MMT
                        WHERE  MWD.WH_NO = MWV.WH_NO
                        AND MWV.WH_NO = MWT.WH_NO
                        AND MWV.MMCODE = MMT.MMCODE
                        AND MWV.WH_NO = :WH_NO 
                        AND MWV.MMCODE = :MMCODE 
                        AND MWV.STORE_LOC = :STORE_LOC";
            return DBWork.Connection.Query<MI_WLOCINV>(sql, new { WH_NO = wh_no, MMCODE = mmcode, STORE_LOC = store_loc }, DBWork.Transaction);
        }

        //新增
        public int Create(MI_WLOCINV mi_wlocinv)
        {
            var sql = @"INSERT INTO MI_WLOCINV (WH_NO, MMCODE, STORE_LOC, INV_QTY,  CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP,LOC_NOTE)  
                                VALUES (:WH_NO, :MMCODE, :STORE_LOC, :INV_QTY, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP, :LOC_NOTE)";
            return DBWork.Connection.Execute(sql, mi_wlocinv, DBWork.Transaction);
        }

        //修改
        public int Update(MI_WLOCINV mi_wlocinv)
        {
            var sql = @"UPDATE MI_WLOCINV
                        SET INV_QTY = :INV_QTY,
                            STORE_LOC = :STORE_LOC,
                            UPDATE_TIME = SYSDATE,
                            UPDATE_USER = :UPDATE_USER,
                            UPDATE_IP = :UPDATE_IP,
                            LOC_NOTE = :LOC_NOTE 
                        WHERE WH_NO = :WH_NO
                        AND MMCODE = :MMCODE
                        AND STORE_LOC = :STORE_LOC_DISPLAY";
            return DBWork.Connection.Execute(sql, mi_wlocinv, DBWork.Transaction);
        }

        //刪除
        public int Delete(string wh_no, string mmcode, string store_loc)
        {
            var sql = @"DELETE FROM MI_WLOCINV
                        WHERE WH_NO = :WH_NO AND MMCODE = :MMCODE AND STORE_LOC = :STORE_LOC";
            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, MMCODE = mmcode, STORE_LOC = store_loc }, DBWork.Transaction);
        }

        //檢查是否已存在
        public bool CheckExists(string WH_NO, string MMCODE, string STORE_LOC)
        {
            string sql = @"SELECT 1 FROM MI_WLOCINV WHERE WH_NO = :WH_NO AND MMCODE = :MMCODE AND STORE_LOC = :STORE_LOC";
            return !(DBWork.Connection.ExecuteScalar(sql, new { WH_NO = WH_NO, MMCODE = MMCODE, STORE_LOC = STORE_LOC }, DBWork.Transaction) == null);
        }

        //匯出EXCEL(S)
        public DataTable GetExcel_S(string WH_NO, string MMCODE, string WH_USERID)
        {
            DynamicParameters p = new DynamicParameters();

            var sql = @"  SELECT MWV.WH_NO 庫房代碼,
                                 MWT.WH_NAME 庫房名稱,
                                 MWV.MMCODE 院內碼,
                                 MMT.MMNAME_C 中文品名,
                                 MMT.MMNAME_E 英文品名,
                                 MWV.STORE_LOC 儲位代碼,
                                 TO_CHAR (MWV.INV_QTY) 儲位庫存數量,
                                 (  SELECT SUM (MWV2.INV_QTY)
                                    FROM MI_WLOCINV MWV2
                                    WHERE MWV2.WH_NO = MWV.WH_NO
                                    AND MWV2.MMCODE = MWV.MMCODE
                                    GROUP BY MWV2.WH_NO, MWV2.MMCODE) 總庫存數量,
                                 MWV.LOC_NOTE 備註 
                          FROM MI_WLOCINV MWV, MI_WHMAST MWT, MI_MAST MMT
                          WHERE     MWV.WH_NO = MWT.WH_NO
                          AND MWV.MMCODE = MMT.MMCODE
                          AND EXISTS
                                    (SELECT WH_NO
                                     FROM (SELECT WH_NO
                                           FROM MI_WHID
                                           WHERE WH_USERID = :WH_USERID
                                           UNION
                                           SELECT WH_NO
                                           FROM MI_WHMAST
                                           WHERE     INID = USER_INID ( :WH_USERID)
                                           AND WH_KIND = '1') AA
                                     WHERE AA.WH_NO = MWV.WH_NO)
                          AND EXISTS
                                    (SELECT MMCODE
                                     FROM MI_WHINV
                                     WHERE WH_NO = MWV.WH_NO
                                     AND MMCODE = MWV.MMCODE) ";

            p.Add(":WH_USERID", string.Format("{0}", WH_USERID));

            if (WH_NO != "")
            {
                sql += " AND MWV.WH_NO = :WH_NO";
                p.Add(":WH_NO", string.Format("{0}", WH_NO));
            }

            if (MMCODE != "")
            {
                sql += " AND MWV.MMCODE = :MMCODE";
                p.Add(":MMCODE", string.Format("{0}", MMCODE));
            }

            sql += @" ORDER BY MWV.WH_NO, MWV.MMCODE";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        //匯出EXCEL(1)
        public DataTable GetExcel_1(string WH_NO, string MMCODE, string WH_USERID)
        {
            DynamicParameters p = new DynamicParameters();

            var sql = @"  SELECT MWV.WH_NO 庫房代碼,
                                 MWT.WH_NAME 庫房名稱,
                                 MWV.MMCODE 院內碼,
                                 MMT.MMNAME_C 中文品名,
                                 MMT.MMNAME_E 英文品名,
                                 MWV.STORE_LOC 儲位代碼,
                                 TO_CHAR (MWV.INV_QTY) 儲位庫存數量,
                                 (  SELECT SUM (MWV2.INV_QTY)
                                    FROM MI_WLOCINV MWV2
                                    WHERE MWV2.WH_NO = MWV.WH_NO
                                    AND MWV2.MMCODE = MWV.MMCODE
                                    GROUP BY MWV2.WH_NO, MWV2.MMCODE) 總庫存數量,
                                 MWV.LOC_NOTE 備註
                           FROM MI_WLOCINV MWV, MI_WHMAST MWT, MI_MAST MMT
                           WHERE     MWV.WH_NO = MWT.WH_NO
                           AND MWV.MMCODE = MMT.MMCODE
                           AND EXISTS
                                     (SELECT WH_NO
                                      FROM MI_WHID
                                      WHERE WH_USERID = :WH_USERID
                                      AND WH_NO = MWV.WH_NO)
                           AND EXISTS
                                     (SELECT MMCODE
                                      FROM MI_WHINV
                                      WHERE WH_NO = MWV.WH_NO
                                      AND MMCODE = MWV.MMCODE) ";

            p.Add(":WH_USERID", string.Format("{0}", WH_USERID));

            if (WH_NO != "")
            {
                sql += " AND MWV.WH_NO = :WH_NO";
                p.Add(":WH_NO", string.Format("{0}", WH_NO));
            }

            if (MMCODE != "")
            {
                sql += " AND MWV.MMCODE = :MMCODE";
                p.Add(":MMCODE", string.Format("{0}", MMCODE));
            }

            sql += @" ORDER BY MWV.WH_NO, MWV.MMCODE";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        //匯出EXCEL
        public DataTable GetExcel(string WH_NO, string MMCODE, string WH_USERID)
        {
            DynamicParameters p = new DynamicParameters();

            var sql = @"  SELECT MWV.WH_NO 庫房代碼,
                                 MWT.WH_NAME 庫房名稱,
                                 MWV.MMCODE 院內碼,
                                 MMT.MMNAME_C 中文品名,
                                 MMT.MMNAME_E 英文品名,
                                 MWV.STORE_LOC 儲位代碼,
                                 TO_CHAR (MWV.INV_QTY) 儲位庫存數量,
                                 (  SELECT SUM (MWV2.INV_QTY)
                                    FROM MI_WLOCINV MWV2
                                    WHERE MWV2.WH_NO = MWV.WH_NO
                                    AND MWV2.MMCODE = MWV.MMCODE
                                    GROUP BY MWV2.WH_NO, MWV2.MMCODE) 總庫存數量,
                                 MWV.LOC_NOTE 備註
                           FROM MI_WLOCINV MWV, MI_WHMAST MWT, MI_MAST MMT
                           WHERE     MWV.WH_NO = MWT.WH_NO
                           AND MWV.MMCODE = MMT.MMCODE
                           AND EXISTS
                                     (SELECT WH_NO
                                      FROM MI_WHID
                                      WHERE WH_USERID = :WH_USERID
                                      AND WH_NO = MWV.WH_NO)
                           AND EXISTS
                                     (SELECT MMCODE
                                      FROM MI_WHINV A
                                      WHERE     A.WH_NO = MWV.WH_NO
                                      AND A.MMCODE = MWV.MMCODE
                                      AND EXISTS
                                                (SELECT 1
                                                 FROM MI_MAST
                                                 WHERE     MMCODE = A.MMCODE
                                                 AND MAT_CLASS IN (SELECT MAT_CLASS
                                                                   FROM MI_MATCLASS
                                                                   WHERE MAT_CLSID in ('2', '3')))) ";

            p.Add(":WH_USERID", string.Format("{0}", WH_USERID));

            if (WH_NO != "")
            {
                sql += " AND MWV.WH_NO = :WH_NO";
                p.Add(":WH_NO", string.Format("{0}", WH_NO));
            }

            if (MMCODE != "")
            {
                sql += " AND MWV.MMCODE = :MMCODE";
                p.Add(":MMCODE", string.Format("{0}", MMCODE));
            }

            sql += @" ORDER BY MWV.WH_NO, MWV.MMCODE";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        //檢查此庫房代碼是否存在
        public bool CheckExistsWH_NO(string WH_NO)
        {
            string sql = @"    SELECT 1
                               FROM MI_WHMAST MWT
                               WHERE MWT.WH_NO = :WH_NO
                               AND MWT.CANCEL_ID = 'N'";
            return !(DBWork.Connection.ExecuteScalar(sql, new { WH_NO = WH_NO }, DBWork.Transaction) == null);
        }

        //檢查此院內碼是否存在
        public bool CheckExistsMMCODE(string MMCODE)
        {
            string sql = @"     SELECT 1
                                FROM MI_MAST MMT
                                WHERE MMT.MMCODE = :MMCODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = MMCODE }, DBWork.Transaction) == null);
        }

        //檢查此儲位代碼是否存在
        public bool CheckExistsSTORE_LOC(string STORE_LOC)
        {
            string sql = @"SELECT 1
                           FROM BC_STLOC BSC
                           WHERE BSC.STORE_LOC = :STORE_LOC";
            return !(DBWork.Connection.ExecuteScalar(sql, new { STORE_LOC = STORE_LOC }, DBWork.Transaction) == null);
        }

        //檢查此帳號是否存在此庫房代碼(S)
        public bool CheckWH_NO_S(string WH_NO, string USER_NAME)
        {
            string sql = @"SELECT 1
                           FROM (SELECT DISTINCT MWD.WH_NO
                                 FROM MI_WHID MWD
                                 WHERE MWD.WH_USERID = :USER_NAME
                                 UNION
                                 SELECT DISTINCT WH_NO
                                 FROM MI_WHMAST MWT
                                 WHERE MWT.INID = USER_INID ( :USER_NAME) AND MWT.WH_KIND = '1')
                           WHERE WH_NO = :WH_NO";

            return !(DBWork.Connection.ExecuteScalar(sql, new { WH_NO = WH_NO, USER_NAME = USER_NAME }, DBWork.Transaction) == null);
        }

        //檢查此帳號是否存在此庫房代碼(1)
        public bool CheckWH_NO_1(string WH_NO, string USER_NAME)
        {
            string sql = @"SELECT 1
                           FROM MI_WHID MWD
                           WHERE MWD.WH_USERID = :USER_NAME
                           AND MWD.WH_NO = :WH_NO";

            return !(DBWork.Connection.ExecuteScalar(sql, new { WH_NO = WH_NO, USER_NAME = USER_NAME }, DBWork.Transaction) == null);
        }

        //檢查此庫房代碼是否存在此院內碼(S)
        public bool CheckMMCODE_S(string WH_NO, string MMCODE)
        {
            string sql = @"SELECT 1
                           FROM MI_WHINV
                           WHERE WH_NO=:WH_NO
                           AND MMCODE = :MMCODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { WH_NO = WH_NO, MMCODE = MMCODE }, DBWork.Transaction) == null);
        }

        //檢查此庫房代碼是否存在此院內碼
        public bool CheckMMCODE(string WH_NO, string MMCODE, string USER_NAME)
        {
            string sql = @"SELECT 1
                           FROM MI_WHINV MWV
                           WHERE     MWV.WH_NO = :WH_NO
                           AND MWV.MMCODE = :MMCODE
                           AND EXISTS
                                     (SELECT MMCODE
                                      FROM MI_MAST MMT
                                      WHERE     MMT.MMCODE = MWV.MMCODE
                                      AND MMT.MAT_CLASS IN (SELECT MMS.MAT_CLASS
                                                            FROM MI_MATCLASS MMS
                                                            WHERE MMS.MAT_CLSID in ('2', '3')))";

            return !(DBWork.Connection.ExecuteScalar(sql, new { WH_NO = WH_NO, MMCODE = MMCODE, USER_NAME = USER_NAME }, DBWork.Transaction) == null);
        }

        //檢查此庫房代碼是否存在此儲位代碼
        public bool CheckSTORE_LOC(string WH_NO, string STORE_LOC)
        {
            string sql = @"SELECT 1
                           FROM BC_STLOC BSC
                           WHERE BSC.WH_NO = :WH_NO
                           AND BSC.STORE_LOC = :STORE_LOC";
            return !(DBWork.Connection.ExecuteScalar(sql, new { WH_NO = WH_NO, STORE_LOC = STORE_LOC }, DBWork.Transaction) == null);
        }

        //匯入(刪除)
        public int Import_Delete(string wh_no, string mmcode)
        {
            var sql = @"DELETE FROM MI_WLOCINV
                        WHERE WH_NO = :WH_NO AND MMCODE = :MMCODE";
            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, MMCODE = mmcode }, DBWork.Transaction);
        }

        //匯入(新增)
        public int Import_Create(MI_WLOCINV mi_wlocinv)
        {
            var sql = @"INSERT INTO MI_WLOCINV (WH_NO, MMCODE, STORE_LOC, INV_QTY,  CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP,LOC_NOTE)  
                                VALUES (:WH_NO, :MMCODE, :STORE_LOC, :INV_QTY, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP, :LOC_NOTE)";
            return DBWork.Connection.Execute(sql, mi_wlocinv, DBWork.Transaction);
        }

        //取得該組庫房代碼+院內碼的總庫存數量
        public double GetQtySum(string WH_NO, string MMCODE)
        {
            var sql = @"SELECT NVL (AA.TOTAL_QTY, 0) TOTAL_QTY
                        FROM (SELECT SUM (MWV.INV_QTY) TOTAL_QTY
                              FROM MI_WLOCINV MWV
                              WHERE MWV.WH_NO = :WH_NO
                              AND MWV.MMCODE = :MMCODE) AA";

            return DBWork.Connection.QueryFirst<double>(sql, new { WH_NO = WH_NO, MMCODE = MMCODE }, DBWork.Transaction);
        }

        //取得該庫房代碼的庫房名稱
        public string GetWH_NAME(string WH_NO)
        {
            var sql = @"SELECT MWT.WH_NAME
                               FROM MI_WHMAST MWT
                               WHERE MWT.WH_NO = :WH_NO";

            return DBWork.Connection.QueryFirst<string>(sql, new { WH_NO = WH_NO }, DBWork.Transaction);
        }

        //取得該院內碼的中文品名
        public string GetMMNAME_C(string MMCODE)
        {
            var sql = @"SELECT NVL (MMT.MMNAME_C, ' ') MMNAME_C
                        FROM MI_MAST MMT
                        WHERE MMT.MMCODE = :MMCODE";

            return DBWork.Connection.QueryFirst<string>(sql, new { MMCODE = MMCODE }, DBWork.Transaction);
        }

        //取得該院內碼的英文品名
        public string GetMMNAME_E(string MMCODE)
        {
            var sql = @"SELECT NVL (MMT.MMNAME_E, ' ') MMNAME_E
                        FROM MI_MAST MMT
                        WHERE MMT.MMCODE = :MMCODE";

            return DBWork.Connection.QueryFirst<string>(sql, new { MMCODE = MMCODE }, DBWork.Transaction);
        }

        //庫房代碼下拉式選單(S)
        public IEnumerable<ComboItemModel> GetWhnoCombo_S(string USER_NAME)
        {
            string sql = @"SELECT DISTINCT MWD.WH_NO VALUE,
                                           (SELECT MWT.WH_NO || ' ' || MWT.WH_NAME
                                            FROM MI_WHMAST MWT
                                            WHERE MWD.WH_NO = MWT.WH_NO) TEXT,
                                            (SELECT MI_WHMAST.WH_KIND FROM MI_WHMAST WHERE MI_WHMAST.WH_NO=MWD.WH_NO)EXTRA1
                           FROM MI_WHID MWD
                           WHERE MWD.WH_USERID = :USER_NAME
                           UNION
                           SELECT DISTINCT WH_NO VALUE,
                                           (SELECT MWT2.WH_NO || ' ' || MWT2.WH_NAME
                                            FROM MI_WHMAST MWT2
                                            WHERE MWT.WH_NO = MWT2.WH_NO) TEXT,
                                            (SELECT MI_WHMAST.WH_KIND FROM MI_WHMAST WHERE MI_WHMAST.WH_NO=MWT.WH_NO)EXTRA1
                           FROM MI_WHMAST MWT
                           WHERE MWT.INID = USER_INID ( :USER_NAME)
                           AND MWT.WH_KIND = '1'
                           ORDER BY VALUE";
            return DBWork.Connection.Query<ComboItemModel>(sql, new { USER_NAME = USER_NAME }, DBWork.Transaction);
        }

        //庫房代碼下拉式選單(1)
        public IEnumerable<ComboItemModel> GetWhnoCombo_1(string USER_NAME)
        {
            string sql = @"SELECT DISTINCT MWD.WH_NO VALUE,
                                           (SELECT MWT.WH_NO || ' ' || MWT.WH_NAME
                                            FROM MI_WHMAST MWT
                                            WHERE MWD.WH_NO = MWT.WH_NO) TEXT,
                                            (SELECT MI_WHMAST.WH_KIND FROM MI_WHMAST WHERE MI_WHMAST.WH_NO=MWD.WH_NO)EXTRA1
                           FROM MI_WHID MWD
                           WHERE MWD.WH_USERID = :USER_NAME
                           ORDER BY MWD.WH_NO";
            return DBWork.Connection.Query<ComboItemModel>(sql, new { USER_NAME = USER_NAME }, DBWork.Transaction);
        }

        //儲位代碼下拉式選單
        public IEnumerable<ComboItemModel> GetSTORE_LOC(string WH_NO)
        {
            string sql = @"SELECT BSC.STORE_LOC VALUE
                           FROM BC_STLOC BSC
                           WHERE BSC.WH_NO = :WH_NO
                           ORDER BY STORE_LOC";
            return DBWork.Connection.Query<ComboItemModel>(sql, new { WH_NO = WH_NO }, DBWork.Transaction);
        }

        //院內碼下拉式選單(搜尋)
        public IEnumerable<MI_MAST> GetMMCodeCombo_Q(string p0, string p1, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E 
                        FROM MI_MAST A
                        WHERE 1=1 ";
            if (p1 != "")
            {
                sql += " AND A.MAT_CLASS = :MAT_CLASS ";
                p.Add(":MAT_CLASS", string.Format("{0}", p1));
            }
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

        //院內碼下拉式選單(新增)(S)
        public IEnumerable<MI_MAST> GetMMCodeCombo_I_S(string p0, string p1, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E 
                        FROM (SELECT MWV.MMCODE,
                                     MMT.MMNAME_C,
                                     MMT.MMNAME_E
                              FROM MI_WHINV MWV, MI_MAST MMT
                              WHERE MWV.WH_NO = :WH_NO
                              AND MWV.MMCODE = MMT.MMCODE) A
                        WHERE 1=1 ";

            p.Add(":WH_NO", p1);

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

        //院內碼下拉式選單(新增)(ELSE)
        public IEnumerable<MI_MAST> GetMMCodeCombo_I(string p0, string p1, string p2, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E 
                        FROM (SELECT MWV.MMCODE, MMT.MMNAME_C, MMT.MMNAME_E
                              FROM MI_WHINV MWV, MI_MAST MMT
                              WHERE     MWV.WH_NO = :WH_NO
                              AND MWV.MMCODE = MMT.MMCODE
                              AND EXISTS
                                        (SELECT MMT.MMCODE
                                         FROM MI_MAST MMT
                                         WHERE     MMT.MMCODE = MWV.MMCODE
                                         AND MMT.MAT_CLASS IN (SELECT MAT_CLASS
                                                               FROM MI_MATCLASS
                                                               WHERE MAT_CLSID in ('2', '3')))) A
                        WHERE 1=1 ";

            p.Add(":WH_NO", p1);
            p.Add(":USER_NAME", p2);

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
            public string WH_NO;
            public string USER_NAME;
        }

        //院內碼彈出式視窗(搜尋)
        public IEnumerable<MI_MAST> GetMmcode_Q(MI_MAST_QUERY_PARAMS query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            string sql = @"SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E 
                           FROM MI_MAST A
                           WHERE 1=1  ";

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

        //院內碼彈出式視窗(新增)(S)
        public IEnumerable<MI_MAST> GetMmcode_I_S(MI_MAST_QUERY_PARAMS query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            string sql = @"SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E 
                           FROM (SELECT MWV.MMCODE,
                                        MMT.MMNAME_C,
                                        MMT.MMNAME_E
                                 FROM MI_WHINV MWV, MI_MAST MMT
                                 WHERE MWV.WH_NO = :WH_NO
                                 AND MWV.MMCODE = MMT.MMCODE) A
                           WHERE 1=1  ";

            p.Add(":WH_NO", string.Format("{0}", query.WH_NO));

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

        //院內碼彈出式視窗(新增)(ELSE)
        public IEnumerable<MI_MAST> GetMmcode_I(MI_MAST_QUERY_PARAMS query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            string sql = @"SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E 
                           FROM (SELECT MWV.MMCODE, MMT.MMNAME_C, MMT.MMNAME_E
                              FROM MI_WHINV MWV, MI_MAST MMT
                              WHERE     MWV.WH_NO = :WH_NO
                              AND MWV.MMCODE = MMT.MMCODE
                              AND EXISTS
                                        (SELECT MMT.MMCODE
                                         FROM MI_MAST MMT
                                         WHERE     MMT.MMCODE = MWV.MMCODE
                                         AND MMT.MAT_CLASS IN (SELECT MAT_CLASS
                                                               FROM MI_MATCLASS
                                                               WHERE MAT_CLSID in ('2', '3')))) A
                           WHERE 1=1  ";

            p.Add(":WH_NO", string.Format("{0}", query.WH_NO));
            p.Add(":USER_NAME", string.Format("{0}", query.USER_NAME));

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

        //取得USER_KIND
        public string USER_KIND(string USER_NAME)
        {
            string sql = @"SELECT USER_KIND ( :USER_NAME) FROM DUAL";
            return DBWork.Connection.QueryFirst<string>(sql, new { USER_NAME = USER_NAME }, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetMatclass1Combo()
        {
            string sql = @"select MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, 
                        MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM 
                        FROM MI_MATCLASS WHERE MAT_CLSID='1'  
                        ORDER BY MAT_CLASS";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetMatclass23Combo()
        {
            string sql = @"select MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, 
                        MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM 
                        FROM MI_MATCLASS WHERE   MAT_CLSID IN( '2' , '3' )  
                        ORDER BY MAT_CLASS";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public int chkMmcode(string mmcode)
        {
            var p = new DynamicParameters();
            var sql = @" select count(*)
                            from MI_MAST
                            where MMCODE = :p0 ";

            p.Add(":p0", mmcode);

            return DBWork.Connection.QueryFirst<int>(sql, p, DBWork.Transaction);
        }
    }
}