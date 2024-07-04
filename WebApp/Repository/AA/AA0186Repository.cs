using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using TSGH.Models;
using System.Collections.Generic;

namespace WebApp.Repository.AA
{
    public class AA0186Repository : JCLib.Mvc.BaseRepository
    {
        public AA0186Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AA0186_MODEL> GetAll(string p0, string p1, string p2, string p3, string p4, string p5, string p6, string p7, string p8, string p9, string p10, string p11, string p12, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT
                            A.TOWH AS F1, 	
                            WH_NAME(A.TOWH) AS F2,	
                            A.DOCNO AS F3, 	
                            TWN_DATE(A.APPTIME) AS F4,	
                            B.MMCODE AS F5, 	
                            B.APPQTY AS F6, 	
                            C.BASE_UNIT AS F7, 	
                            C.M_CONTPRICE AS F8, 	
                            B.APPQTY * C.M_CONTPRICE AS F9, 	
                            DECODE(A.ISARMY,'1','軍','2','民','') AS F10,	
                            DECODE(C.E_SOURCECODE,'P','買斷','C','寄售','') AS F11,	
                            DECODE(C.M_CONTID,'0','合約品項','2','非合約','') AS F12,	
                            C.CASENO AS F13, 	
                            TWN_DATE(C.E_CODATE) AS F14,	
                            DECODE(C.TOUCHCASE,'1','院內選項','2','非院內選項','3','院內自辦合約','') AS F15,	
                            A.APPLY_NOTE AS F16, 	
                            DECODE(C.ORDERKIND,'0','無',' 1','常備品項',' 2','小額採購','') AS F17,	
                            C.MMNAME_C AS F18, 	
                            CONVERT_MR_STATUS(A.FLOWID,A.DOCTYPE,B.APVQTY) AS F19,	
                            B.APVQTY AS F20, 	
                            B.ACKQTY AS F21, 	
                            '' AS F22, 	
                            B.CHINNAME AS F23, 	
                            B.APLYITEM_NOTE AS F24 
                        FROM ME_DOCM A, ME_DOCD B, MI_MAST C
                        WHERE A.DOCNO = B.DOCNO
                          AND B.MMCODE = C.MMCODE
                          AND A.DOCTYPE LIKE 'MR%'  ";

            if (!string.IsNullOrWhiteSpace(p0) & !string.IsNullOrWhiteSpace(p1))
            {
                sql += " AND TWN_DATE(A.APPTIME) BETWEEN :p0 AND :p1 ";
                p.Add(":p0", string.Format("{0}", p0));
                p.Add(":p1", string.Format("{0}", p1));
            }
            if (!string.IsNullOrWhiteSpace(p0) & string.IsNullOrWhiteSpace(p1))
            {
                sql += " AND TWN_DATE(A.APPTIME) >= :p0 ";
                p.Add(":p0", string.Format("{0}", p0));
            }
            if (string.IsNullOrWhiteSpace(p0) & !string.IsNullOrWhiteSpace(p1))
            {
                sql += " AND TWN_DATE(A.APPTIME) = :p1 ";
                p.Add(":p1", string.Format("{0}", p1));
            }

            if (!string.IsNullOrWhiteSpace(p2))
            {
                sql += " AND A.DOCNO like :p2 ";
                p.Add(":p2", string.Format("%{0}%", p2));
            }
            if (!string.IsNullOrWhiteSpace(p3))  //請領單位
            {
                sql += " AND A.TOWH = :p3 ";
                p.Add(":p3", p3);
            }

            if (!string.IsNullOrWhiteSpace(p4))
            {
                if (p4 == "all01" || p4 == "all02")
                {
                    sql += " AND C.MAT_CLASS = :p4 ";
                    p.Add(":p4", p4.Replace("all", ""));
                }
                else
                {
                    sql += " AND C.MAT_CLASS_SUB = :p4 ";
                    p.Add(":p4", p4);
                }
            }
            if (!string.IsNullOrWhiteSpace(p5))
            {
                sql += " AND B.MMCODE = :p5 ";
                p.Add(":p5", p5);
            }
            if (!string.IsNullOrWhiteSpace(p6))
            {
                sql += " AND CONVERT_MR_STATUS(A.FLOWID,A.DOCTYPE,B.APVQTY)  = :p6 ";
                p.Add(":p6", p6);
            }
            if (!string.IsNullOrWhiteSpace(p7))
            {
                sql += " AND C.E_SOURCECODE = :p7 ";
                p.Add(":p7", p7);
            }
            if (!string.IsNullOrWhiteSpace(p8))
            {
                sql += " AND C.M_CONTID = :p8 ";
                p.Add(":p8", p8);
            }
            if (!string.IsNullOrWhiteSpace(p9))
            {
                sql += " AND C.TOUCHCASE = :p9 ";
                p.Add(":p9", p9);
            }
            if (!string.IsNullOrWhiteSpace(p10))
            {
                sql += " AND C.ORDERKIND = :p10 ";
                p.Add(":p10", p10);
            }
            if (!string.IsNullOrWhiteSpace(p11))
            {
                sql += " AND A.APPLY_NOTE like :p11 ";
                p.Add(":p11", string.Format("%{0}%", p11));
            }

            if (!string.IsNullOrWhiteSpace(p12))
            {
                sql += " AND C.M_AGENNO like :p12 ";
                p.Add(":p12", string.Format("%{0}%", p12));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0186_MODEL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public DataTable GetExcel(string p0, string p1, string p2, string p3, string p4, string p5, string p6, string p7, string p8, string p9, string p10, string p11, string p12)
        {
            var p = new DynamicParameters();


            var sql = @"SELECT
                            A.TOWH 單位代碼, 	
                            WH_NAME(A.TOWH) 請領單位,	
                            A.DOCNO 請領單, 	
                            TWN_DATE(A.APPTIME) 請領日期,	
                            B.MMCODE 藥材代碼, 	
                            B.APPQTY 請領數量, 	
                            C.BASE_UNIT 單位, 	
                            C.M_CONTPRICE 單價, 	
                            B.APPQTY * C.M_CONTPRICE 小計, 	
                            DECODE(A.ISARMY,'1','軍','2','民','') 軍民別,	
                            DECODE(C.E_SOURCECODE,'P','買斷','C','寄售','') 買斷寄庫,	
                            DECODE(C.M_CONTID,'0','合約品項','2','非合約','') 是否合約,	
                            C.CASENO 合約案號, 	
                            TWN_DATE(C.E_CODATE) 合約到期日,	
                            DECODE(C.TOUCHCASE,'1','院內選項','2','非院內選項','3','院內自辦合約','') 合約類別,	
                            A.APPLY_NOTE 備註, 	
                            DECODE(C.ORDERKIND,'0','無',' 1','常備品項',' 2','小額採購','') 採購類別,	
                            C.MMNAME_C 藥材名稱, 	
                            CONVERT_MR_STATUS(A.FLOWID,A.DOCTYPE,B.APVQTY) 請領單狀態,	
                            B.APVQTY 核可量, 	
                            B.ACKQTY 已撥發量, 	
                            '' 病患身份證, 	
                            B.CHINNAME 病患姓名, 	
                            B.APLYITEM_NOTE 明細備註
                        FROM ME_DOCM A, ME_DOCD B, MI_MAST C
                        WHERE A.DOCNO = B.DOCNO
                          AND B.MMCODE = C.MMCODE
                          AND A.DOCTYPE LIKE 'MR%'  ";

            if (!string.IsNullOrWhiteSpace(p0) & !string.IsNullOrWhiteSpace(p1))
            {
                sql += " AND TWN_DATE(A.APPTIME) BETWEEN :p0 AND :p1 ";
                p.Add(":p0", string.Format("{0}", p0));
                p.Add(":p1", string.Format("{0}", p1));
            }
            if (!string.IsNullOrWhiteSpace(p0) & string.IsNullOrWhiteSpace(p1))
            {
                sql += " AND TWN_DATE(A.APPTIME) >= :p0 ";
                p.Add(":p0", string.Format("{0}", p0));
            }
            if (string.IsNullOrWhiteSpace(p0) & !string.IsNullOrWhiteSpace(p1))
            {
                sql += " AND TWN_DATE(A.APPTIME) = :p1 ";
                p.Add(":p1", string.Format("{0}", p1));
            }

            if (!string.IsNullOrWhiteSpace(p2))
            {
                sql += " AND A.DOCNO like :p2 ";
                p.Add(":p2", string.Format("%{0}%", p2));
            }
            if (!string.IsNullOrWhiteSpace(p3))
            {
                sql += " AND A.TOWH = :p3 ";
                p.Add(":p3", p3);
            }

            if (!string.IsNullOrWhiteSpace(p4))
            {
                if (p4 == "all01" || p4 == "all02")
                {
                    sql += " AND C.MAT_CLASS = :p4 ";
                    p.Add(":p4", p4.Replace("all", ""));
                }
                else
                {
                    sql += " AND C.MAT_CLASS_SUB = :p4 ";
                    p.Add(":p4", p4);
                }
            }
            if (!string.IsNullOrWhiteSpace(p5))
            {
                sql += " AND B.MMCODE = :p5 ";
                p.Add(":p5", p5);
            }
            if (!string.IsNullOrWhiteSpace(p6))
            {
                sql += " AND CONVERT_MR_STATUS(A.FLOWID,A.DOCTYPE,B.APVQTY)  = :p6 ";
                p.Add(":p6", p6);
            }
            if (!string.IsNullOrWhiteSpace(p7))
            {
                sql += " AND C.E_SOURCECODE = :p7 ";
                p.Add(":p7", p7);
            }
            if (!string.IsNullOrWhiteSpace(p8))
            {
                sql += " AND C.M_CONTID = :p8 ";
                p.Add(":p8", p8);
            }
            if (!string.IsNullOrWhiteSpace(p9))
            {
                sql += " AND C.TOUCHCASE = :p9 ";
                p.Add(":p9", p9);
            }
            if (!string.IsNullOrWhiteSpace(p10))
            {
                sql += " AND C.ORDERKIND = :p10 ";
                p.Add(":p10", p10);
            }
            if (!string.IsNullOrWhiteSpace(p11))
            {
                sql += " AND A.APPLY_NOTE like :p11 ";
                p.Add(":p11", string.Format("%{0}%", p11));
            }

            if (!string.IsNullOrWhiteSpace(p12))
            {
                sql += " AND C.M_AGENNO like :p12 ";
                p.Add(":p12", string.Format("%{0}%", p12));
            }

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
        public IEnumerable<UR_INID> GetUrInidCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"
                                    SELECT {0} 
                                           a.INID,
                                           a.INID_NAME
                                     FROM  UR_INID a
                                    WHERE  1 = 1 
                                ";

            if (!string.IsNullOrWhiteSpace(p0))
            {
                sql = string.Format(sql, "(NVL(INSTR(UPPER(A.INID), UPPER(:INID_I)), 1000) + NVL(INSTR(UPPER(A.INID_NAME), UPPER(:INID_NAME_I)), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
                p.Add(":INID_I", p0);
                p.Add(":INID_NAME_I", p0);

                sql = string.Format("SELECT * FROM ({0}) TMP WHERE 1=1 ", sql);

                sql += " AND (UPPER(INID) LIKE UPPER(:INID) ";
                p.Add(":INID", string.Format("%{0}%", p0));

                sql += " OR UPPER(INID_NAME) LIKE UPPER(:INID_NAME)) ";
                p.Add(":INID_NAME", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY INID ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<UR_INID>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
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

        public IEnumerable<COMBO_MODEL> GetMatClassSubCombo()
        {
            string sql = @"SELECT 
                    data_value AS value, data_desc AS text, 99 as sort_num 
                FROM
                    param_d
                WHERE
                    grp_code = 'MI_MAST'
                    AND   data_name = 'MAT_CLASS_SUB'
                    AND   TRIM(data_desc) IS NOT NULL
                UNION
                     SELECT ' ' AS value, '全部' AS text, 1 as sort_num FROM dual
                UNION 
                     SELECT 'all01' AS value, '全部藥品' AS text, 2 as sort_num FROM dual
                UNION
                     SELECT 'all02' AS value, '全部衛材' AS text, 3 as sort_num FROM dual
                    order by sort_num";

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
                            SELECT '0' VALUE, '合約品項' TEXT FROM DUAL
                            UNION
                            SELECT '2' VALUE, '非合約' TEXT FROM DUAL
                            ) ORDER BY VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<COMBO_MODEL> GetTouchcaseCombo()
        {
            string sql = @" SELECT VALUE, TEXT FROM (
                            SELECT '1' VALUE, '院內選項' TEXT FROM DUAL
                            UNION
                            SELECT '2' VALUE, '非院內選項' TEXT FROM DUAL
                            UNION
                            SELECT '3' VALUE, '院內自辦合約' TEXT FROM DUAL
                            ) ORDER BY VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }



        public IEnumerable<COMBO_MODEL> GetAgen_noCombo()
        {
            string sql = string.Empty;
            sql = @"
                    select AGEN_NO as value,AGEN_NAMEC as text
                      from PH_VENDER 
                     order by AGEN_NO
                ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, DBWork.Transaction);
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

        public IEnumerable<COMBO_MODEL> GetWhnoCombo()
        {
            string sql = string.Empty;
            sql = @"
                    select wh_no as value,wh_name as text 
                      from MI_WHMAST 
                     where nvl(cancel_id, 'N')='N'
                     order by wh_grade, wh_no
                ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, DBWork.Transaction);
        }



    }



    public class AA0186_MODEL : JCLib.Mvc.BaseModel
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
    }

}