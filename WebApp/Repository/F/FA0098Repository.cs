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

namespace WebApp.Repository.F
{

    public class FA0098Repository : JCLib.Mvc.BaseRepository
    {
        public FA0098Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<FA0098MODEL_FORM> GetAllForm(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = "";

            #region
            sql = @" WITH SQL_1 
                       AS ( SELECT CHK_WH_KIND WH_KIND,
                                   DECODE (CHK_WH_GRADE, '1', '1', '2') WH_GRADE,
                                   ROUND (SUM (PYM_INV_QTY * PYM_DISC_CPRICE), 0) S1,
                                   ROUND (SUM (STORE_QTYC * DISC_CPRICE), 0) S2,
                                   ROUND (SUM (CHK_QTY * DISC_CPRICE), 0) S3,
                                   ROUND (SUM (INVENTORY * DISC_CPRICE), 0) S4,
                                   ROUND (SUM (WAR_QTY * DISC_CPRICE), 0) S5,
                                   0 S6,
                                   0 S7
                            FROM CHK_MAST A, CHK_DETAIL B
                            WHERE A.CHK_NO = B.CHK_NO
                            AND CHK_YM = :CHK_YM
                            AND CHK_WH_KIND IN ('0', '1')
                            GROUP BY CHK_WH_KIND, DECODE (CHK_WH_GRADE, '1', '1', '2')
                            ORDER BY CHK_WH_KIND, DECODE (CHK_WH_GRADE, '1', '1', '2')),
                 SQL_2 AS ( SELECT B.WH_KIND,
                                   'SQL_2' WH_GRADE,
                                   0 S1,
                                   0 S2,
                                   0 S3,
                                   0 S4,
                                   0 S5,
                                   ROUND (SUM (A.APL_INQTY * C.DISC_CPRICE - A.REJ_OUTQTY * C.DISC_CPRICE), 0) S6,
                                   0 S7
                            FROM MI_WINVMON A, MI_WHMAST B, MI_WHCOST C
                            WHERE A.WH_NO = B.WH_NO
                            AND A.DATA_YM = C.DATA_YM
                            AND A.MMCODE = C.MMCODE
                            AND A.DATA_YM = :CHK_YM
                            AND C.DATA_YM = :CHK_YM
                            AND B.WH_GRADE = '1'
                            AND B.WH_KIND IN ('0', '1')
                            GROUP BY B.WH_KIND),
                 SQL_3 AS ( SELECT B.WH_KIND,
                                   'SQL_3' WH_GRADE,
                                   0 S1,
                                   0 S2,
                                   0 S3,
                                   0 S4,
                                   0 S5,
                                   0 S6,
                                   ROUND (SUM (A.USE_QTY * C.DISC_CPRICE), 0) S7
                            FROM MI_WINVMON A, MI_WHMAST B, MI_WHCOST C
                            WHERE A.WH_NO = B.WH_NO 
                            AND A.DATA_YM = C.DATA_YM 
                            AND A.MMCODE = C.MMCODE
                            AND A.DATA_YM = :CHK_YM
                            AND C.DATA_YM = :CHK_YM
                            AND B.WH_KIND IN ('0','1')
                            GROUP BY B.WH_KIND),
               SQL_ALL AS ( SELECT *
                            FROM SQL_1 
                            UNION SELECT *
                            FROM SQL_2
                            UNION SELECT *
                            FROM SQL_3),
               SQL_CAL AS ( SELECT 0 F2,
                                   SUM ((CASE WHEN WH_KIND = '0' AND WH_GRADE = '2' THEN S1 ELSE 0 END)) F3,
                                   SUM ((CASE WHEN WH_KIND = '0' AND WH_GRADE = '1' THEN S1 ELSE 0 END)) F4,
                                   0 F6,
                                   SUM ((CASE WHEN WH_KIND = '0' AND WH_GRADE = '2' THEN S2 ELSE 0 END)) F7,
                                   SUM ((CASE WHEN WH_KIND = '0' AND WH_GRADE = '1' THEN S2 ELSE 0 END)) F8,
                                   0 F10,
                                   SUM ((CASE WHEN WH_KIND = '0' AND WH_GRADE = '2' THEN S3 ELSE 0 END)) F11,
                                   SUM ((CASE WHEN WH_KIND = '0' AND WH_GRADE = '1' THEN S3 ELSE 0 END)) F12,
                                   0 F14,
                                   SUM ((CASE WHEN WH_KIND = '0' AND WH_GRADE = '2' THEN S4 ELSE 0 END)) F15,
                                   SUM ((CASE WHEN WH_KIND = '0' AND WH_GRADE = '1' THEN S4 ELSE 0 END)) F16,
                                   SUM ((CASE WHEN WH_KIND = '1' AND WH_GRADE = '2' THEN S1 ELSE 0 END)) F18,
                                   0 F19,
                                   SUM ((CASE WHEN WH_KIND = '1' AND WH_GRADE = '1' THEN S1 ELSE 0 END)) F20,
                                   SUM ((CASE WHEN WH_KIND = '1' AND WH_GRADE = '2' THEN S2 ELSE 0 END)) F22,
                                   0 F23,
                                   SUM ((CASE WHEN WH_KIND = '1' AND WH_GRADE = '1' THEN S2 ELSE 0 END)) F24,
                                   SUM ((CASE WHEN WH_KIND = '1' AND WH_GRADE = '2' THEN S3 ELSE 0 END)) F26,
                                   0 F27,
                                   SUM ((CASE WHEN WH_KIND = '1' AND WH_GRADE = '1' THEN S3 ELSE 0 END)) F28,
                                   SUM ((CASE WHEN WH_KIND = '1' AND WH_GRADE = '2' THEN S4 ELSE 0 END)) F30,
                                   0 F31,
                                   SUM ((CASE WHEN WH_KIND = '1' AND WH_GRADE = '1' THEN S4 ELSE 0 END)) F32,
                                   SUM ((CASE WHEN WH_KIND = '0' AND WH_GRADE = 'SQL_2' THEN S6 ELSE 0 END)) F33,
                                   SUM ((CASE WHEN WH_KIND = '0' AND WH_GRADE = 'SQL_3' THEN S7 ELSE 0 END)) F34,
                                   SUM ((CASE WHEN WH_KIND = '1' AND WH_GRADE = 'SQL_2' THEN S6 ELSE 0 END)) F35,
                                   SUM ((CASE WHEN WH_KIND = '1' AND WH_GRADE = 'SQL_3' THEN S7 ELSE 0 END)) F36,
                                   SUM ((CASE WHEN WH_KIND = '0' AND (WH_GRADE = '1' OR  WH_GRADE = '1') THEN S5 ELSE 0 END)) F37,
                                   SUM ((CASE WHEN WH_KIND = '1' AND (WH_GRADE = '1' OR  WH_GRADE = '1') THEN S5 ELSE 0 END)) F38
                            FROM SQL_ALL)
                       SELECT F2 + F3 + F4 F1,
                              F2 F2,
                              F3 F3,
                              F4 F4,
                              F6 + F7 + F8 F5,
                              F6 F6,
                              F7 F7,
                              F8 F8,
                              F10 + F11 + F12 F9,
                              F10 F10,
                              F11 F11,
                              F12 F12,
                              F14 + F15 + F16 F13,
                              F14 F14,
                              F15 F15,
                              F16 F16,
                              F18 + F19 + F20 F17,
                              F18 F18,
                              F19 F19,
                              F20 F20,
                              F22 + F23 + F24 F21,
                              F22 F22,
                              F23 F23,
                              F24 F24,
                              F26 + F27 + F28 F25,
                              F26 F26,
                              F27 F27,
                              F28 F28,
                              F30 + F31 + F32 F29,
                              F30 F30,
                              F31 F31,
                              F32 F32,
                              F33 F33,
                              F34 F34,
                              F35 F35,
                              F36 F36,
                              F37 F37,
                              F38 F38,
                              F33 + F35 F_A,
                              F33 F_B,
                              F35 F_C,
                              F33 + (F2 + F3 + F4) - (F6 + F7 + F8) + F35 + (F18 + F19 + F20) - (F22 + F23 + F24) F_D,
                              F33 + (F2 + F3 + F4) - (F6 + F7 + F8) F_E,
                              F35 + (F18 + F19 + F20) - (F22 + F23 + F24) F_F
                       FROM SQL_CAL";
            #endregion

            p.Add("CHK_YM", p0);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<FA0098MODEL_FORM>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<FA0098MODEL_GRID> GetAllGrid(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = "";

            #region
            sql = @" SELECT E.SECTIONNO,
                            F.SECTIONNAME,
                            ROUND( SUM(USEAMT * ( E.SEC_DISRATIO / 100)), 0) S8
                     FROM ( SELECT B.INID,
                                   ROUND( SUM(A.USE_QTY * C.DISC_CPRICE), 0) USEAMT
                            FROM MI_WINVMON A, MI_WHMAST B, MI_WHCOST C
                            WHERE A.WH_NO = B.WH_NO
                            AND A.DATA_YM = C.DATA_YM
                            AND A.MMCODE = C.MMCODE
                            AND A.DATA_YM = :DATA_YM 
                            AND C.DATA_YM = :DATA_YM
                            AND B.WH_KIND IN ('0','1')
                            GROUP BY B.INID) D,
                            SEC_CALLOC E, SEC_MAST F
                     WHERE E.DATA_YM = :DATA_YM 
                     AND D.INID = E.INID
                     AND E.SECTIONNO = F.SECTIONNO
                     AND E.SEC_DISRATIO <> 0
                     GROUP BY E.SECTIONNO, F.SECTIONNAME
                     ORDER BY S8 DESC ";
            #endregion

            p.Add(":DATA_YM", p0);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<FA0098MODEL_GRID>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<FA0098MODEL_AMOUNT_FORM> GetAmountForm(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = "";

            #region
            sql = @" SELECT SUM(S8) GridAmount,
                            COUNT(*) GridCount
                     FROM (SELECT E.SECTIONNO,
                                  F.SECTIONNAME,
                                  ROUND( SUM(USEAMT * ( E.SEC_DISRATIO / 100)), 0) S8
                           FROM ( SELECT B.INID,
                                         ROUND( SUM(A.USE_QTY * C.DISC_CPRICE), 0) USEAMT
                                  FROM MI_WINVMON A, MI_WHMAST B, MI_WHCOST C
                                  WHERE A.WH_NO = B.WH_NO
                                  AND A.DATA_YM = C.DATA_YM
                                  AND A.MMCODE = C.MMCODE
                                  AND A.DATA_YM = :DATA_YM 
                                  AND C.DATA_YM = :DATA_YM
                                  AND B.WH_KIND IN ('0','1')
                                  GROUP BY B.INID) D,
                                  SEC_CALLOC E, SEC_MAST F
                           WHERE E.DATA_YM = :DATA_YM 
                           AND D.INID = E.INID
                           AND E.SECTIONNO = F.SECTIONNO
                           AND E.SEC_DISRATIO <> 0
                           GROUP BY E.SECTIONNO, F.SECTIONNAME) ";
            #endregion

            p.Add(":DATA_YM", p0);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<FA0098MODEL_AMOUNT_FORM>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public class FA0098MODEL_FORM : JCLib.Mvc.BaseModel
        {
            public string F1 { get; set; }
            public string F2 { get; set; }
            public string F3 { get; set; }
            public string F4 { get; set; }
            public string F5 { get; set; }
            public string F6 { get; set; }
            public string F7 { get; set; }
            public string F8 { get; set; }
            public string F9 { get; set; }
            public string F10 { get; set; }
            public string F11 { get; set; }
            public string F12 { get; set; }
            public string F13 { get; set; }
            public string F14 { get; set; }
            public string F15 { get; set; }
            public string F16 { get; set; }
            public string F17 { get; set; }
            public string F18 { get; set; }
            public string F19 { get; set; }
            public string F20 { get; set; }
            public string F21 { get; set; }
            public string F22 { get; set; }
            public string F23 { get; set; }
            public string F24 { get; set; }
            public string F25 { get; set; }
            public string F26 { get; set; }
            public string F27 { get; set; }
            public string F28 { get; set; }
            public string F29 { get; set; }
            public string F30 { get; set; }
            public string F31 { get; set; }
            public string F32 { get; set; }
            public string F33 { get; set; }
            public string F34 { get; set; }
            public string F35 { get; set; }
            public string F36 { get; set; }
            public string F37 { get; set; }
            public string F38 { get; set; }
            public string F_A { get; set; }
            public string F_B { get; set; }
            public string F_C { get; set; }
            public string F_D { get; set; }
            public string F_E { get; set; }
            public string F_F { get; set; }
        }
        public class FA0098MODEL_GRID : JCLib.Mvc.BaseModel
        {
            public string SECTIONNO { get; set; }
            public string SECTIONNAME { get; set; }
            public string S8 { get; set; }
        }
        public class FA0098MODEL_AMOUNT_FORM : JCLib.Mvc.BaseModel
        {
            public string GridAmount { get; set; }
            public string GridCount { get; set; }
        }
    }
}
