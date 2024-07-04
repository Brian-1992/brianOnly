using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using TSGH.Models;
using System.Collections.Generic;

namespace WebApp.Repository.AA
{
    public class AA0152_MODEL : JCLib.Mvc.BaseModel
    {
        public string WH_NO { get; set; }   // 庫房代碼
        public string WH_NAME { get; set; } // 庫房名稱
        public string MMCODE { get; set; }  // 院內碼
        public string MMNAME_C { get; set; }    // 中文品名
        public string MMNAME_E { get; set; }    // 英文品名
        public string LOT_NO { get; set; }  // 批號
        public string EXP_DATE { get; set; }    // 效期
        public string INV_QTY { get; set; } // 存量
        public string EXP_CHK { get; set; } // 是否為近效期
    }

    public class AA0152Repository : JCLib.Mvc.BaseRepository
    {
        public AA0152Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AA0152_MODEL> GetAll(string wh_no, string mat_class, string mmcode, string exp_date_from, string exp_date_to, string isab,string is9991231, string tuser, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"
                                    SELECT a.wh_no, --庫房代碼,
                                           wh_name(a.wh_no) AS wh_name, --庫房名稱,
                                           a.mmcode, --院內碼,
                                           b.mmname_c, --中文品名,
                                           b.mmname_e, --英文品名,
                                           a.lot_no, --批號,
                                           twn_date(exp_date) AS exp_date, --效期,
                                           inv_qty, --存量
                                           (case when A.EXP_DATE - sysdate <= 180 then 'Y' else 'N' end) as EXP_CHK
                                    FROM   MI_WEXPINV a,
                                           MI_MAST b
                                    WHERE  1 = 1
                                           AND a.mmcode = b.mmcode 
                                ";

            if (!string.IsNullOrWhiteSpace(wh_no))  // 庫房代碼
            {
                sql += " AND a.wh_no = :wh_no ";
                p.Add(":wh_no", wh_no);
            }

            if (!string.IsNullOrWhiteSpace(mat_class)) // 物料分類
            {
                sql += " AND b.mat_class = :mat_class ";
                p.Add(":mat_class", mat_class);
            }

            if (!string.IsNullOrWhiteSpace(mmcode)) // 院內碼
            {
                sql += " AND b.mmcode = :mmcode ";
                p.Add(":mmcode", mmcode);
            }

            if (is9991231 == "true")
            {
                string tempStr = " 1=1 ";

                if (!string.IsNullOrWhiteSpace(exp_date_from)) // 效期起
                {
                    tempStr += " AND twn_yyymm(exp_date) >= lpad(:exp_date_from, 5,'0') ";
                    p.Add(":exp_date_from", exp_date_from);
                }

                if (!string.IsNullOrWhiteSpace(exp_date_to)) // 效期訖
                {
                    tempStr += " AND twn_yyymm(exp_date) <= lpad(:exp_date_to, 5, '0') ";
                    p.Add(":exp_date_to", exp_date_to);
                }

                sql += string.Format(" AND ( ({0}) or  twn_yyymm(exp_date) = lpad('9991231', 5, '0'))", tempStr);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(exp_date_from)) // 效期起
                {
                    sql += " AND twn_yyymm(exp_date) >= lpad(:exp_date_from, 5,'0') ";
                    p.Add(":exp_date_from", exp_date_from);
                }

                if (!string.IsNullOrWhiteSpace(exp_date_to)) // 效期訖
                {
                    sql += " AND twn_yyymm(exp_date) <= lpad(:exp_date_to, 5, '0') ";
                    p.Add(":exp_date_to", exp_date_to);
                }
            }

            if (isab == "Y")
            {
                sql += @" AND exists 
                            ( 
                            SELECT 1 
                            FROM MI_WHID X,MI_WHMAST Y
                            WHERE X.WH_NO = Y.WH_NO 
                              AND X.WH_USERID=:TUSER 
                              AND X.WH_NO = A.WH_NO
                            UNION  
                            SELECT 1 
                            FROM MI_WHMAST X
                            WHERE X.WH_NO = A.WH_NO 
                              AND ( X.INID= USER_INID(:TUSER) 
                              or (select count(*) from UR_UIR where RLNO in ('MAT_14', 'MED_14', 'MMSpl_14') and TUSER = :TUSER) > 0 
                            ) ) ";
                p.Add(":TUSER", tuser);

            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0152_MODEL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetWhCombo(string tuser)
        {
            string sql = @"   select A.WH_NO VALUE ,A.WH_NO || ' ' || B.WH_NAME COMBITEM 
                                from MI_WHID A,MI_WHMAST B
                                WHERE A.WH_NO = B.WH_NO 
                                  AND A.WH_USERID=:TUSER 
                                UNION  
                                SELECT A.WH_NO, A.WH_NO || ' ' || A.WH_NAME COMBITEM 
                                FROM MI_WHMAST A
                                WHERE A.INID= USER_INID(:TUSER) 
                                or (select count(*) from UR_UIR where RLNO in ('MAT_14', 'MED_14', 'MMSpl_14') and TUSER = :TUSER) > 0
                                ORDER BY VALUE
                                    ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { TUSER = tuser }, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetMatClassCombo(string wh_no)
        {
            var p = new DynamicParameters();

            string sql = @"
                                        WITH temp_whnos
                                             AS (SELECT wh_no,
                                                        '1' AS taskId,
                                                        wh_kind
                                                 FROM   MI_WHMAST
                                                 WHERE  wh_no = WHNO_ME1
                                                 UNION
                                                 SELECT wh_no,
                                                        '2' AS taskId,
                                                        wh_kind
                                                 FROM   MI_WHMAST
                                                 WHERE  wh_no = WHNO_MM1
                                                 UNION
                                                 SELECT wh_no,
                                                        '3' AS taskId,
                                                        wh_kind
                                                 FROM   MI_WHMAST
                                                 WHERE  wh_no = WHNO_MM1)
                                        SELECT b.mat_class AS VALUE,
                                               b.mat_class || ' ' || b.mat_clsname AS COMBITEM
                                        FROM   temp_whnos a,
                                               MI_MATCLASS b
                                        WHERE  a.taskId = b.mat_clsid
                                    ";

            if (!string.IsNullOrWhiteSpace(wh_no))  //庫房
            {
                sql += " AND (a.wh_no = :wh_no or a.wh_no=(select PWH_NO from MI_WHMAST where WH_NO=:wh_no))";
                p.Add(":wh_no", wh_no);
            }

            sql += " ORDER  BY b.mat_class ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, string mat_class, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"
                                    SELECT {0} 
                                           a.mmcode,
                                           a.mmname_c,
                                           a.mmname_e
                                    FROM   MI_MAST a
                                    WHERE  1 = 1
                                           AND a.mat_class IN ( '01', '02', '03', '04',
                                                                '05', '06', '07', '08' )
                                           AND nvl(cancel_id, 'N') = 'N'
                                ";

            if (!string.IsNullOrWhiteSpace(mat_class))
            {
                sql += " AND a.mat_class = :mat_class ";
                p.Add(":mat_class", mat_class);
            }

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

        public IEnumerable<COMBO_MODEL> GetExpDate()
        {
            string sql = @" 
                                        SELECT twn_yyymm(SYSDATE) AS VALUE,
                                               twn_yyymm(add_months(SYSDATE, 6)) AS TEXT
                                        FROM   DUAL 
                                    ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, DBWork.Transaction);
        }

        public DataTable GetExcel(string wh_no, string mat_class, string mmcode, string exp_date_from, string exp_date_to, string is9991231)
        {
            var p = new DynamicParameters();

            var sql = @"
                                    SELECT a.wh_no AS 庫房代碼,
                                           wh_name(a.wh_no) AS 庫房名稱,
                                           a.mmcode AS 院內碼,
                                           b.mmname_c AS 中文品名,
                                           b.mmname_e AS 英文品名,
                                           a.lot_no AS 批號,
                                           twn_date(exp_date) AS 效期,
                                           inv_qty AS 存量
                                    FROM   MI_WEXPINV a,
                                           MI_MAST b
                                    WHERE  1 = 1
                                           AND a.mmcode = b.mmcode 
                                ";

            if (!string.IsNullOrWhiteSpace(wh_no))  // 庫房代碼
            {
                sql += " AND a.wh_no = :wh_no ";
                p.Add(":wh_no", wh_no);
            }

            if (!string.IsNullOrWhiteSpace(mat_class)) // 物料分類
            {
                sql += " AND b.mat_class = :mat_class ";
                p.Add(":mat_class", mat_class);
            }

            if (!string.IsNullOrWhiteSpace(mmcode)) // 院內碼
            {
                sql += " AND b.mmcode = :mmcode ";
                p.Add(":mmcode", mmcode);
            }

            if (is9991231 == "true")
            {
                string tempStr = " 1=1 ";

                if (!string.IsNullOrWhiteSpace(exp_date_from)) // 效期起
                {
                    tempStr += " AND twn_yyymm(exp_date) >= lpad(:exp_date_from, 5,'0') ";
                    p.Add(":exp_date_from", exp_date_from);
                }

                if (!string.IsNullOrWhiteSpace(exp_date_to)) // 效期訖
                {
                    tempStr += " AND twn_yyymm(exp_date) <= lpad(:exp_date_to, 5, '0') ";
                    p.Add(":exp_date_to", exp_date_to);
                }

                sql += string.Format(" AND ( ({0}) or  twn_yyymm(exp_date) = lpad('9991231', 5, '0'))", tempStr);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(exp_date_from)) // 效期起
                {
                    sql += " AND twn_yyymm(exp_date) >= lpad(:exp_date_from, 5,'0') ";
                    p.Add(":exp_date_from", exp_date_from);
                }

                if (!string.IsNullOrWhiteSpace(exp_date_to)) // 效期訖
                {
                    sql += " AND twn_yyymm(exp_date) <= lpad(:exp_date_to, 5, '0') ";
                    p.Add(":exp_date_to", exp_date_to);
                }
            }
            sql += " order by a.WH_NO, a.MMCODE ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

    }
}