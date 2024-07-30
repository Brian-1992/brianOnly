using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using TSGH.Models;
using System.Collections.Generic;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using System.Data.OracleClient;

namespace WebApp.Repository.AA
{
    public class AA0192Repository : JCLib.Mvc.BaseRepository
    {
        public AA0192Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AA0192_MODEL> GetAll(string p0, string p1, string p2, string p3, string p4, string p5, string p6, string p7, string p8, string p9, string p10, string p11, string p12, string p13, string p14, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT 
                            A.MMCODE AS F1, 	
                            C.MMNAME_C AS F2, 	
                            B.STORE_LOC AS F3, 	
                            TWN_DATE(A.EXP_DATE) AS F4, 	
                            A.INV_QTY AS F5, 
                            B.INV_QTY AS F6, 
                            NVL(D.WAR_QTY,0)  AS F7, 
                            C.BASE_UNIT AS F8, 
                        ";

            if (p13 == "Y")
            {
                sql += @"   C.DISC_UPRICE AS F9, 
                            A.INV_QTY * C.DISC_UPRICE AS F10, 
                        ";
            }
            else
            {
                sql += @"   C.UPRICE AS F9, 
                            A.INV_QTY * C.UPRICE AS F10, 
                        ";
            }
            sql += @"	    DECODE(C.E_SOURCECODE,'P','買斷','C','寄售','') AS F11, 	
                            (SELECT M_AGENNO || ' ' || EASYNAME FROM PH_VENDER WHERE M_AGENNO= C.M_AGENNO AND ROWNUM=1) AS F12, 	
                            A.LOT_NO AS F13
                        FROM MI_WEXPINV A, MI_WLOCINV B, MI_MAST C, ( SELECT DISTINCT MMCODE,WAR_QTY FROM MI_BASERO_14 ) D
                        WHERE A.WH_NO  = B.WH_NO
                        AND   A.MMCODE = B.MMCODE
                        AND   A.MMCODE = C.MMCODE
                        AND   A.MMCODE = D.MMCODE(+)
                          ";

            
            if (!string.IsNullOrWhiteSpace(p0))
            {
                sql += " AND A.MMCODE = :p0 ";
                p.Add(":p0", p0);
            }
            if (!string.IsNullOrWhiteSpace(p1))
            {
                sql += " AND B.STORE_LOC = :p1 ";
                p.Add(":p1", p1);
            }
            if (!string.IsNullOrWhiteSpace(p2))
            {
                sql += " AND TWN_DATE(A.EXP_DATE) = :p2 ";
                p.Add(":p2", p2);
            }
            if (!string.IsNullOrWhiteSpace(p3))
            {
                sql += " AND C.M_AGENNO = :p3 ";
                p.Add(":p3", p3);
            }

            if (!string.IsNullOrWhiteSpace(p4))
            {
                if (p4 == "01" || p4 == "02")
                {
                    sql += " AND C.MAT_CLASS = :p4 ";
                    p.Add(":p4", p4);
                }
                else
                {
                    sql += " AND C.COMMON = :p4 ";
                    p.Add(":p4", p4);
                }
            }
            if (!string.IsNullOrWhiteSpace(p5))
            {
                sql += " AND C.E_SOURCECODE = :p5 ";
                p.Add(":p5", p5);
            }
            if (!string.IsNullOrWhiteSpace(p6))
            {
                sql += " AND C.M_CONTID = :p6 ";
                p.Add(":p6", p6);
            }
            if (!string.IsNullOrWhiteSpace(p7))
            {
                sql += " AND C.MAT_CLASS_SUB = :p7 ";
                p.Add(":p7", p7);
            }
            if (!string.IsNullOrWhiteSpace(p8))
            {
                sql += " AND C.ORDERKIND = :p8 ";
                p.Add(":p8", p8);
            }
            if (p9 == "Y")
            {
                sql += " AND C.CANCEL_ID = 'Y' ";
            }
            if (p10 == "Y")
            {
                sql += " AND C.SPDRUG = '1' ";
            }
            if (p11 == "Y")
            {
                sql += " AND C.FASTDRUG = '1' ";
            }
            if (p12 == "Y")
            {
                sql += " AND C.WARBAK = '1' ";
            }
            if (p13 == "Y")
            {
                sql += " AND ( A.INV_QTY < 0 OR B.INV_QTY < 0 ) ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0192_MODEL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public DataTable GetExcel(string p0, string p1, string p2, string p3, string p4, string p5, string p6, string p7, string p8, string p9, string p10, string p11, string p12, string p13, string p14)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT 
                            A.MMCODE AS 藥材代碼, 	
                            C.MMNAME_C AS 藥材名稱, 	
                            B.STORE_LOC AS 倉儲儲位, 	
                            TWN_DATE(A.EXP_DATE) AS 末效期, 	
                            A.INV_QTY AS 現量, 
                            B.INV_QTY AS 總量, 
                            NVL(D.WAR_QTY,0)  AS 戰備量, 
                            C.BASE_UNIT AS 單位, 
                        ";

            if (p13 == "Y")
            {
                sql += @"   C.DISC_UPRICE AS 單價, 
                            A.INV_QTY * C.DISC_UPRICE AS 小計, 
                        ";
            }
            else
            {
                sql += @"   C.UPRICE AS 單價, 
                            A.INV_QTY * C.UPRICE AS 小計, 
                        ";
            }
            sql += @"	    DECODE(C.E_SOURCECODE,'P','買斷','C','寄售','') AS 買斷寄庫, 	
                            (SELECT M_AGENNO || ' ' || EASYNAME FROM PH_VENDER WHERE M_AGENNO= C.M_AGENNO AND ROWNUM=1) AS 廠商, 	
                            A.LOT_NO AS 批號
                        FROM MI_WEXPINV A, MI_WLOCINV B, MI_MAST C, ( SELECT DISTINCT MMCODE,WAR_QTY FROM MI_BASERO_14 ) D
                        WHERE A.WH_NO  = B.WH_NO
                        AND   A.MMCODE = B.MMCODE
                        AND   A.MMCODE = C.MMCODE
                        AND   A.MMCODE = D.MMCODE(+)
                          ";


            if (!string.IsNullOrWhiteSpace(p0))
            {
                sql += " AND A.MMCODE = :p0 ";
                p.Add(":p0", p0);
            }
            if (!string.IsNullOrWhiteSpace(p1))
            {
                sql += " AND B.STORE_LOC = :p1 ";
                p.Add(":p1", p1);
            }
            if (!string.IsNullOrWhiteSpace(p2))
            {
                sql += " AND TWN_DATE(A.EXP_DATE) = :p2 ";
                p.Add(":p2", p2);
            }
            if (!string.IsNullOrWhiteSpace(p3))
            {
                sql += " AND C.M_AGENNO = :p3 ";
                p.Add(":p3", p3);
            }

            if (!string.IsNullOrWhiteSpace(p4))
            {
                if (p4 == "01" || p4 == "02")
                {
                    sql += " AND C.MAT_CLASS = :p4 ";
                    p.Add(":p4", p4);
                }
                else
                {
                    sql += " AND C.COMMON = :p4 ";
                    p.Add(":p4", p4);
                }
            }
            if (!string.IsNullOrWhiteSpace(p5))
            {
                sql += " AND C.E_SOURCECODE = :p5 ";
                p.Add(":p5", p5);
            }
            if (!string.IsNullOrWhiteSpace(p6))
            {
                sql += " AND C.M_CONTID = :p6 ";
                p.Add(":p6", p6);
            }
            if (!string.IsNullOrWhiteSpace(p7))
            {
                sql += " AND C.MAT_CLASS_SUB = :p7 ";
                p.Add(":p7", p7);
            }
            if (!string.IsNullOrWhiteSpace(p8))
            {
                sql += " AND C.ORDERKIND = :p8 ";
                p.Add(":p8", p8);
            }
            if (p9 == "Y")
            {
                sql += " AND C.CANCEL_ID = 'Y' ";
            }
            if (p10 == "Y")
            {
                sql += " AND C.SPDRUG = '1' ";
            }
            if (p11 == "Y")
            {
                sql += " AND C.FASTDRUG = '1' ";
            }
            if (p12 == "Y")
            {
                sql += " AND C.WARBAK = '1' ";
            }
            if (p13 == "Y")
            {
                sql += " AND ( A.INV_QTY < 0 OR B.INV_QTY < 0 ) ";
            }


            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
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

        public IEnumerable<PH_VENDER> GetPHVenderCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"
                                    SELECT {0} 
                                           a.AGEN_NO,
                                           a.EASYNAME,
                                           a.AGEN_NAMEC
                                     FROM  PH_VENDER a
                                    WHERE  1 = 1 
                                ";

            if (!string.IsNullOrWhiteSpace(p0))
            {
                sql = string.Format(sql, "(NVL(INSTR(A.AGEN_NO, :AGEN_NO_I), 1000) + NVL(INSTR(A.AGEN_NAMEC, :AGEN_NAMEC_I), 100) * 10 + NVL(INSTR(A.EASYNAME, :EASYNAME_I), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
                p.Add(":AGEN_NO_I", p0);
                p.Add(":AGEN_NAMEC_I", p0);
                p.Add(":EASYNAME_I", p0);

                sql = string.Format("SELECT * FROM ({0}) TMP WHERE 1=1 ", sql);

                sql += " AND (AGEN_NO LIKE :AGEN_NO ";
                p.Add(":AGEN_NO", string.Format("%{0}%", p0));

                sql += " OR AGEN_NAMEC LIKE :AGEN_NAMEC ";
                p.Add(":AGEN_NAMEC", string.Format("%{0}%", p0));

                sql += " OR EASYNAME LIKE :EASYNAME) ";
                p.Add(":EASYNAME", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY AGEN_NO ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<PH_VENDER>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetStoreLocCombo()
        {
            string sql = @"
                         SELECT DISTINCT STORE_LOC AS VALUE, STORE_LOC AS TEXT
                         FROM MI_WLOCINV 
                         WHERE STORE_LOC IS NOT NULL 
                         ORDER BY STORE_LOC ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetExpDateCombo()
        {
            string sql = @"
                        SELECT TWN_DATE(T.EXP_DATE) AS VALUE,TWN_DATE(T.EXP_DATE) AS TEXT 
                        FROM(
                        SELECT DISTINCT EXP_DATE FROM MI_WEXPINV 
                        WHERE EXP_DATE IS NOT NULL 
                        ORDER BY EXP_DATE
                        ) T
                        ORDER BY 1 ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetMatClassCombo()
        {
            string sql = @"
                     SELECT '01' AS value, '藥品' AS text FROM dual
                        UNION
                     SELECT '02' AS value, '衛材' AS text FROM dual
                        UNION
                     SELECT '1' AS value, '非常用衛材' AS text FROM dual
                        UNION
                     SELECT '2' AS value, '常用衛材' AS text FROM dual ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetMatClassSubCombo()
        {
            string sql = @"SELECT 
                                data_value AS value, data_desc AS text
                            FROM
                                param_d
                            WHERE
                                grp_code = 'MI_MAST'
                                AND   data_name = 'MAT_CLASS_SUB'
                                AND   data_value BETWEEN '1' AND '7' 
                                AND   TRIM(data_desc) IS NOT NULL 
                            ORDER BY VALUE
                ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<COMBO_MODEL> GetESourceCodeCombo()
        {
            string sql = @" SELECT VALUE, TEXT FROM (
                            SELECT 'P' VALUE, '買斷' TEXT FROM DUAL
                            UNION
                            SELECT 'C' VALUE, '寄售' TEXT FROM DUAL
                            ) ORDER BY VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<COMBO_MODEL> GetMContidCombo()
        {
            string sql = @" SELECT VALUE, TEXT FROM (
                            SELECT '0' VALUE, '合約' TEXT FROM DUAL
                            UNION
                            SELECT '2' VALUE, '非合約' TEXT FROM DUAL
                            ) ORDER BY VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        

        public IEnumerable<COMBO_MODEL> GetOrderkindCombo()
        {
            string sql = @" SELECT VALUE, TEXT FROM (
                            SELECT '0' VALUE, '無' TEXT FROM DUAL
                            UNION
                            SELECT '1' VALUE, '常備品項' TEXT FROM DUAL
                            UNION
                            SELECT '2' VALUE, '小額採購' TEXT FROM DUAL
                            ) ORDER BY VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }


    }



    public class AA0192_MODEL : JCLib.Mvc.BaseModel
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
    }
    
}