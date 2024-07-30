using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using TSGH.Models;
using System.Collections.Generic;

namespace WebApp.Repository.AA
{
    public class AA0185Repository : JCLib.Mvc.BaseRepository
    {
        public AA0185Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AA0185_MODEL> GetAll(string p0, string p1, string p2, string p3, string p4, string p5, string p6, string p7, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT
                            A.DOCNO AS F1,  
                            A.SEQ AS F2,  
                            TWN_DATE(A.APPTIME) AS F3,  
                            A.APP_INID || ' '||INID_NAME(A.APP_INID) AS F4,  
                            USER_NAME (A. CREATE_USER) AS F5,  
                            CASE WHEN A.IS_DEL = 'S' AND A.APVQTY > 0 THEN USER_NAME (A. UPDATE_USER) ELSE ' ' END AS F6,  
                            A.MMCODE AS F7,  
                            B.MMNAME_C AS F8,  
                            B.BASE_UNIT AS F9,  
                            A.APPQTY AS F10,  
                            A.APVQTY AS F11, 
                            CASE WHEN IS_DEL = 'Y' THEN '取消'
                                 WHEN IS_DEL = 'S' AND APVTIME IS NOT NULL THEN '核可確認'
                                 WHEN IS_DEL = 'S' AND APVTIME IS NULL THEN '核可中'
                                 WHEN IS_DEL = 'N' THEN '未送出' END AS F12  
                            FROM DGMISS A, MI_MAST B
                            WHERE A.MMCODE = B.MMCODE
                             ";

            if (!string.IsNullOrWhiteSpace(p0) & !string.IsNullOrWhiteSpace(p1))
            {
                sql += " AND A.DOCNO BETWEEN :p0 AND :p1 ";
                p.Add(":p0", string.Format("{0}", p0));
                p.Add(":p1", string.Format("{0}", p1));
            }
            if (!string.IsNullOrWhiteSpace(p0) & string.IsNullOrWhiteSpace(p1))
            {
                sql += " AND A.DOCNO = :p0 ";
                p.Add(":p0", string.Format("{0}", p0));
            }
            if (string.IsNullOrWhiteSpace(p0) & !string.IsNullOrWhiteSpace(p1))
            {
                sql += " AND A.DOCNO = :p1 ";
                p.Add(":p1", string.Format("{0}", p1));
            }

            if (!string.IsNullOrWhiteSpace(p2) & !string.IsNullOrWhiteSpace(p3))
            {
                sql += " AND TWN_DATE(A.APPTIME) BETWEEN :p2 AND :p3 ";
                p.Add(":p2", string.Format("{0}", p2));
                p.Add(":p3", string.Format("{0}", p3));
            }
            if (!string.IsNullOrWhiteSpace(p2) & string.IsNullOrWhiteSpace(p3))
            {
                sql += " AND TWN_DATE(A.APPTIME) >= :p2 ";
                p.Add(":p2", string.Format("{0}", p2));
            }
            if (string.IsNullOrWhiteSpace(p2) & !string.IsNullOrWhiteSpace(p3))
            {
                sql += " AND TWN_DATE(A.APPTIME) = :p3 ";
                p.Add(":p3", string.Format("{0}", p3));
            }

            if (!string.IsNullOrWhiteSpace(p4) & !string.IsNullOrWhiteSpace(p5))
            {
                sql += " AND A.APP_INID BETWEEN :p4 AND :p5 ";
                p.Add(":p4", string.Format("{0}", p4));
                p.Add(":p5", string.Format("{0}", p5));
            }
            if (!string.IsNullOrWhiteSpace(p4) & string.IsNullOrWhiteSpace(p5))
            {
                sql += " AND A.APP_INID = :p4 ";
                p.Add(":p4", string.Format("{0}", p4));
            }
            if (string.IsNullOrWhiteSpace(p4) & !string.IsNullOrWhiteSpace(p5))
            {
                sql += " AND A.APP_INID = :p5 ";
                p.Add(":p5", string.Format("{0}", p5));
            }

            if (!string.IsNullOrWhiteSpace(p6))
            {
                sql += " AND A.MMCODE = :p6 ";
                p.Add(":p6", p6);
            }

            if (!string.IsNullOrWhiteSpace(p7) && p7=="false")
            {
                sql += "  AND IS_DEL<> 'Y' ";
            }


            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0185_MODEL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<AA0185_MODEL> GetAllM(string p0, string p1, string p2, string p3, string p4, string p5, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT DISTINCT
                            A.DOCNO AS F1,  
                            TWN_DATE(A.APPTIME) AS F3,  
                            A.APP_INID || ' '||INID_NAME(A.APP_INID) AS F4,  
                            USER_NAME (A. CREATE_USER) AS F5
                            FROM DGMISS A, MI_MAST B
                            WHERE A.MMCODE = B.MMCODE
                            AND IS_DEL <> 'Y' 
                             ";

            if (!string.IsNullOrWhiteSpace(p0) & !string.IsNullOrWhiteSpace(p1))
            {
                sql += " AND A.DOCNO BETWEEN :p0 AND :p1 ";
                p.Add(":p0", string.Format("{0}", p0));
                p.Add(":p1", string.Format("{0}", p1));
            }
            if (!string.IsNullOrWhiteSpace(p0) & string.IsNullOrWhiteSpace(p1))
            {
                sql += " AND A.DOCNO = :p0 ";
                p.Add(":p0", string.Format("{0}", p0));
            }
            if (string.IsNullOrWhiteSpace(p0) & !string.IsNullOrWhiteSpace(p1))
            {
                sql += " AND A.DOCNO = :p1 ";
                p.Add(":p1", string.Format("{0}", p1));
            }

            if (!string.IsNullOrWhiteSpace(p2) & !string.IsNullOrWhiteSpace(p3))
            {
                sql += " AND TWN_DATE(A.APPTIME) BETWEEN :p2 AND :p3 ";
                p.Add(":p2", string.Format("{0}", p2));
                p.Add(":p3", string.Format("{0}", p3));
            }
            if (!string.IsNullOrWhiteSpace(p2) & string.IsNullOrWhiteSpace(p3))
            {
                sql += " AND TWN_DATE(A.APPTIME) >= :p2 ";
                p.Add(":p2", string.Format("{0}", p2));
            }
            if (string.IsNullOrWhiteSpace(p2) & !string.IsNullOrWhiteSpace(p3))
            {
                sql += " AND TWN_DATE(A.APPTIME) = :p3 ";
                p.Add(":p3", string.Format("{0}", p3));
            }

            if (!string.IsNullOrWhiteSpace(p4) & !string.IsNullOrWhiteSpace(p5))
            {
                sql += " AND A.APP_INID BETWEEN :p4 AND :p5 ";
                p.Add(":p4", string.Format("{0}", p4));
                p.Add(":p5", string.Format("{0}", p5));
            }
            if (!string.IsNullOrWhiteSpace(p4) & string.IsNullOrWhiteSpace(p5))
            {
                sql += " AND A.APP_INID = :p4 ";
                p.Add(":p4", string.Format("{0}", p4));
            }
            if (string.IsNullOrWhiteSpace(p4) & !string.IsNullOrWhiteSpace(p5))
            {
                sql += " AND A.APP_INID = :p5 ";
                p.Add(":p5", string.Format("{0}", p5));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0185_MODEL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }


        public IEnumerable<AA0185_MODEL> GetAllD(string p0, string p1, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT
                            A.SEQ AS F2,
                            CASE WHEN A.IS_DEL = 'S' AND A.APVQTY > 0 THEN USER_NAME (A. UPDATE_USER) ELSE ' ' END AS F6,
                            A.MMCODE AS F7,  
                            B.MMNAME_C AS F8,  
                            B.BASE_UNIT AS F9,  
                            A.APPQTY AS F10,  
                            A.APVQTY AS F11, 
                            CASE WHEN IS_DEL = 'Y' THEN '取消'
                                 WHEN IS_DEL = 'S' AND APVTIME IS NOT NULL THEN '核可確認'
                                 WHEN IS_DEL = 'S' AND APVTIME IS NULL THEN '核可中'
                                 WHEN IS_DEL = 'N' THEN '未送出' END AS F12  
                            FROM DGMISS A, MI_MAST B
                            WHERE A.MMCODE = B.MMCODE
                            AND IS_DEL <> 'Y' 
                             ";

            if (!string.IsNullOrWhiteSpace(p0))
            {
                sql += " AND A.DOCNO = :p0 ";
                p.Add(":p0", string.Format("{0}", p0));
            }

            if (!string.IsNullOrWhiteSpace(p1))
            {
                sql += " AND A.MMCODE = :p1 ";
                p.Add(":p1", p1);
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0185_MODEL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"
                                    SELECT {0} 
                                           a.mmcode,
                                           a.mmname_c,
                                           a.mmname_e
                                     FROM  MI_MAST a
                                    WHERE  1 = 1
                                      AND  nvl(cancel_id, 'N') = 'N'
                                ";

            if (!string.IsNullOrWhiteSpace(p0))
            {
                sql = string.Format(sql, "(NVL(INSTR(UPPER(A.MMCODE), UPPER(:MMCODE_I)), 1000) + NVL(INSTR(UPPER(A.MMNAME_E), UPPER(:MMNAME_E_I)), 100) * 10 + NVL(INSTR(UPPER(A.MMNAME_C), UPPER(:MMNAME_C_I)), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql = string.Format("SELECT * FROM ({0}) TMP WHERE 1=1 ", sql);

                sql += " AND (UPPER(MMCODE) LIKE UPPER(:MMCODE) ";
                p.Add(":MMCODE", string.Format("%{0}%", p0));

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

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetDocnoCombo()
        {
            string sql = @" SELECT DISTINCT A.DOCNO as VALUE, A.DOCNO as TEXT, A.DOCNO as COMBITEM
                            FROM DGMISS A WHERE 1 = 1 ORDER BY A.DOCNO ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<COMBO_MODEL> GetInidCombo()
        {
            string sql = @" SELECT INID as VALUE, INID || ' ' || INID_NAME as TEXT, INID || ' ' || INID_NAME as COMBITEM
                            FROM UR_INID WHERE 1 = 1 ORDER BY INID ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

    }

    public class AA0185_MODEL : JCLib.Mvc.BaseModel
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
    }

}