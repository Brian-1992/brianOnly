using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using WebApp.Models;
using WebApp.Models.AA;

namespace WebApp.Repository.AA
{
    public class AA0182Repository : JCLib.Mvc.BaseRepository
    {
        public AA0182Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AA0182> GetAll(string wh_no, string mat_class, string mmcode, string exp_date_from, string exp_date_to, string is9991231, string tuser, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"
                                    SELECT A.wh_no, wh_name(A.wh_no) AS wh_name,
                                           A.mmcode, B.mmname_c, B.mmname_e,
                                           A.lot_no, twn_date(A.exp_date) AS exp_date,
                                           A.inv_qty,
                                           CASE WHEN ROW_NUMBER()OVER(PARTITION BY WH_NO,A.MMCODE ORDER BY exp_date) =
                                           COUNT(1)OVER(PARTITION BY WH_NO,A.MMCODE) THEN 
                                           nvl((select inv_qty from MI_WHINV where wh_no = A.wh_no and mmcode = A.mmcode), 0) END wh_inv_qty ,
                                           (case when A.EXP_DATE - sysdate <= 180 then 'Y' else 'N' end) as EXP_CHK
                                    FROM   MI_WEXPINV A,
                                           MI_MAST B
                                    WHERE  1 = 1
                                           AND A.MMCODE = B.MMCODE 
                                           AND A.WH_NO in 
                                           (select A.WH_NO
                                            from MI_WHID A,MI_WHMAST B
                                            WHERE A.WH_NO = B.WH_NO 
                                              AND A.WH_USERID=:TUSER 
                                            UNION  
                                            SELECT A.WH_NO
                                            FROM MI_WHMAST A
                                            WHERE A.INID= USER_INID(:TUSER) 
                                            or (select count(*) from UR_UIR where RLNO in ('MAT_14', 'MED_14', 'MMSpl_14') and TUSER = :TUSER) > 0 )
            ";

            p.Add(":TUSER", tuser);

            if (!string.IsNullOrWhiteSpace(wh_no))  // 庫房代碼
            {
                sql += " AND A.wh_no = :wh_no ";
                p.Add(":wh_no", wh_no);
            }

            if (!string.IsNullOrWhiteSpace(mat_class)) // 物料分類
            {
                if (mat_class.Contains("SUB_"))
                {
                    sql += " AND B.mat_class_sub = :mat_class ";
                    p.Add(":mat_class", mat_class.Replace("SUB_", ""));
                }
                else
                {
                    sql += " AND B.mat_class = :mat_class ";
                    p.Add(":mat_class", mat_class);
                }
            }

            if (!string.IsNullOrWhiteSpace(mmcode)) // 院內碼
            {
                sql += " AND B.mmcode = :mmcode ";
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

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0182>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetWhCombo(string tuser, string menuLink)
        {
            string sql = @" 
                                select A.WH_NO VALUE ,A.WH_NO || ' ' || B.WH_NAME COMBITEM 
                                from MI_WHID A,MI_WHMAST B
                                WHERE A.WH_NO = B.WH_NO 
                                  AND A.WH_USERID=:TUSER {0}
                                UNION  
                                SELECT A.WH_NO, A.WH_NO || ' ' || A.WH_NAME COMBITEM 
                                FROM MI_WHMAST A
                                WHERE A.INID= USER_INID(:TUSER)  {1}
                                or (select count(*) from UR_UIR where RLNO in ('MAT_14', 'MED_14', 'MMSpl_14') and TUSER = :TUSER) > 0
                                ORDER BY VALUE
                            ";
            if (menuLink == "AB0141")
            {
                sql = string.Format(sql, " AND B.WH_KIND='0'", " AND A.WH_KIND='0'");
            }
            else
            {
                sql = string.Format(sql, "", "");

            }


            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { TUSER = tuser }, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetMatClassCombo(string tuser, string hospcode, string wh_no)
        {
            var p = new DynamicParameters();
            var sql_whno = "";
            if (!string.IsNullOrWhiteSpace(wh_no))  //庫房
            {
                sql_whno = " AND (a.wh_no = :wh_no or a.wh_no=(select PWH_NO from MI_WHMAST where WH_NO=:wh_no))";
            }

            string sql = @"
                            with temp_whkinds as (
                            select b.wh_no, b.wh_kind, 
                                             nvl((case when b.wh_kind = '0' then '1' else '2' end), '2') as task_id
                            from MI_WHID a, MI_WHMAST b
                            where wh_userid = :TUSER
                                       and a.wh_no = b.wh_no
                                       and ((b.wh_grade in ('1', '2') and b.wh_kind = '0')
                                            or (b.wh_grade = '1' and b.wh_kind = '1')
                                           )
                            )
                            select distinct b.mat_class as value,
                                         '全部'||b.mat_clsname as text,
                                         b.mat_class || ' ' ||  '全部'||b.mat_clsname as COMBITEM 
                                  from temp_whkinds a, MI_MATCLASS b
                                 where (a.task_id = b.mat_clsid) " + sql_whno;
            if (hospcode != "803")
            {
                sql += @"
                                union
                                select 'SUB_' || b.data_value as value, b.data_desc as text,
                                b.data_value || ' ' || b.data_desc as COMBITEM 
                                from temp_whkinds a, PARAM_D b
	                             where b.grp_code ='MI_MAST' 
	                               and b.data_name = 'MAT_CLASS_SUB'
	                               and b.data_value = '1'
	                               and trim(b.data_desc) is not null
                                   and (a.task_id = '1')" + sql_whno;
            }
            sql += @"
                                union
                                select 'SUB_' || b.data_value as value, b.data_desc as text,
                                b.data_value || ' ' || b.data_desc as COMBITEM 
                                from temp_whkinds a, PARAM_D b
	                             where b.grp_code ='MI_MAST' 
	                               and b.data_name = 'MAT_CLASS_SUB'
	                               and b.data_value <> '1'
	                               and trim(b.data_desc) is not null
                                   and (a.task_id = '2')" + sql_whno;
            sql += " ORDER  BY value ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { TUSER = tuser, wh_no = wh_no }, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, string mat_class, string wh_no, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"
                                    SELECT {0} 
                                           a.mmcode,
                                           a.mmname_c,
                                           a.mmname_e
                                    FROM   MI_MAST a
                                    WHERE  1 = 1
                                           AND a.mat_class IN ( '01', '02' )
                                ";

            if (!string.IsNullOrWhiteSpace(mat_class))
            {
                if (mat_class.Contains("SUB_"))
                {
                    sql += " AND A.mat_class_sub = :mat_class ";
                    p.Add(":mat_class", mat_class.Replace("SUB_", ""));
                }
                else
                {
                    sql += " AND A.mat_class = :mat_class ";
                    p.Add(":mat_class", mat_class);
                }
            }

            if (!string.IsNullOrWhiteSpace(wh_no))
            {
                sql += " AND (select count(*) from MI_WHINV where WH_NO = :wh_no and MMCODE = A.MMCODE) > 0 ";
                p.Add(":wh_no", wh_no);
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

        //匯出
        public DataTable GetExcel(string wh_no, string mat_class, string mmcode, string exp_date_from, string exp_date_to, string is9991231, string tuser)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT 
                    A.wh_no as 庫房代碼, 
                    wh_name(A.wh_no) AS 庫房名稱,
                    A.mmcode as 院內碼, 
                    B.mmname_c as 中文品名, 
                    B.mmname_e as 英文品名,
                    A.lot_no as 批號, 
                    twn_date(A.exp_date) as 效期,
                    A.inv_qty as 批號存量,
                    nvl((select inv_qty from MI_WHINV where wh_no = A.wh_no and mmcode = A.mmcode), 0) as 庫房存量
            FROM   MI_WEXPINV A,
                    MI_MAST B
            WHERE  1 = 1
                    AND A.MMCODE = B.MMCODE 
                    AND A.WH_NO in 
                    (select A.WH_NO
                    from MI_WHID A,MI_WHMAST B
                    WHERE A.WH_NO = B.WH_NO 
                        AND A.WH_USERID=:TUSER 
                    UNION  
                    SELECT A.WH_NO
                    FROM MI_WHMAST A
                    WHERE A.INID= USER_INID(:TUSER) 
                    or (select count(*) from UR_UIR where RLNO in ('MAT_14', 'MED_14', 'MMSpl_14') and TUSER = :TUSER) > 0 )
            ";

            p.Add(":TUSER", tuser);

            if (!string.IsNullOrWhiteSpace(wh_no))  // 庫房代碼
            {
                sql += " AND A.wh_no = :wh_no ";
                p.Add(":wh_no", wh_no);
            }

            if (!string.IsNullOrWhiteSpace(mat_class)) // 物料分類
            {
                if (mat_class.Contains("SUB_"))
                {
                    sql += " AND B.mat_class_sub = :mat_class ";
                    p.Add(":mat_class", mat_class.Replace("SUB_", ""));
                }
                else
                {
                    sql += " AND B.mat_class = :mat_class ";
                    p.Add(":mat_class", mat_class);
                }
            }

            if (!string.IsNullOrWhiteSpace(mmcode)) // 院內碼
            {
                sql += " AND B.mmcode = :mmcode ";
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
            sql += " order by A.WH_NO, A.MMCODE ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
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

        public int UpdateWexpinv(string wh_no, string mmcode, string lot_no, string exp_date, string invqty, string tuser, string userIp)
        {
            string lot_no_cond = " and LOT_NO = :LOT_NO ";
            if (string.IsNullOrEmpty(lot_no))
                lot_no_cond = " and trim(LOT_NO) is null ";
            else if (lot_no.Trim() == "")
                lot_no_cond = " and trim(LOT_NO) is null ";

            exp_date = exp_date.PadLeft(7, '0');

            string sql = @" update MI_WEXPINV
                set INV_QTY = :INV_QTY, UPDATE_USER = :TUSER, UPDATE_TIME = sysdate, UPDATE_IP = :USERIP
                where WH_NO = :WH_NO and MMCODE = :MMCODE " + lot_no_cond + @" and EXP_DATE = twn_todate(:EXP_DATE)
                ";
            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, MMCODE = mmcode, LOT_NO = lot_no, EXP_DATE = exp_date, INV_QTY = invqty, TUSER = tuser, USERIP = userIp }, DBWork.Transaction);
        }

        public int GetWexpinvSum(string wh_no, string mmcode)
        {
            var sql = @"select sum(INV_QTY) from MI_WEXPINV where WH_NO = :WH_NO and MMCODE = :MMCODE ";
            return DBWork.Connection.QueryFirstOrDefault<int>(sql, new { WH_NO = wh_no, MMCODE = mmcode }, DBWork.Transaction);
        }

        public int GetWhinvSum(string wh_no, string mmcode)
        {
            var sql = @"select INV_QTY from MI_WHINV where WH_NO = :WH_NO and MMCODE = :MMCODE ";
            return DBWork.Connection.QueryFirstOrDefault<int>(sql, new { WH_NO = wh_no, MMCODE = mmcode }, DBWork.Transaction);
        }

        public int GetWexpinv(string wh_no, string mmcode, string lot_no, string exp_date)
        {
            string lot_no_cond = " and LOT_NO = :LOT_NO ";
            if (string.IsNullOrEmpty(lot_no))
                lot_no_cond = " and trim(LOT_NO) is null ";
            else if (lot_no.Trim() == "")
                lot_no_cond = " and trim(LOT_NO) is null ";

            exp_date = exp_date.PadLeft(7, '0');

            var sql = @"select INV_QTY from MI_WEXPINV WHERE 
                           WH_NO = :wh_no 
                           AND MMCODE = :mmcode 
                           " + lot_no_cond + @"
                           AND TRUNC(EXP_DATE, 'DD') = TO_DATE(:exp_date,'YYYY-MM-DD')";
            return DBWork.Connection.QueryFirstOrDefault<int>(sql, new { WH_NO = wh_no, MMCODE = mmcode, LOT_NO = lot_no, EXP_DATE = exp_date }, DBWork.Transaction);
        }

        public bool CheckWhnoExists(string wh_no)
        {
            string sql = @"SELECT 1 FROM MI_WHMAST 
                            WHERE WH_NO = :wh_no ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { WH_NO = wh_no }, DBWork.Transaction) == null);
        }

        public bool CheckMmcodeExists(string mmcode)
        {
            string sql = @"SELECT 1 FROM MI_MAST 
                            WHERE MMCODE = :mmcode ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = mmcode }, DBWork.Transaction) == null);
        }

        public bool CheckExists(string wh_no, string mmcode, string lot_no, string exp_date)
        {
            string lot_no_cond = " and LOT_NO = :LOT_NO ";
            if (string.IsNullOrEmpty(lot_no))
                lot_no_cond = " and trim(LOT_NO) is null ";
            else if (lot_no.Trim() == "")
                lot_no_cond = " and trim(LOT_NO) is null ";

            exp_date = exp_date.PadLeft(7, '0');

            string sql = @"SELECT 1 FROM MI_WEXPINV 
                            WHERE WH_NO = :wh_no 
                              AND MMCODE = :mmcode 
                              " + lot_no_cond + @"
                              AND TRUNC(EXP_DATE, 'DD') = TO_DATE(:exp_date,'YYYY-MM-DD')"; // TRUNC(a.EXP_DATE, 'DD') TWN_TODATE(:exp_date)
            return !(DBWork.Connection.ExecuteScalar(sql,
                                                     new
                                                     {
                                                         WH_NO = wh_no,
                                                         MMCODE = mmcode,
                                                         LOT_NO = lot_no,
                                                         EXP_DATE = string.Format("{0}", DateTime.Parse(exp_date).ToString("yyyy-MM-dd"))
                                                     },
                                                     DBWork.Transaction) == null);
        }

        public int Create(AA0182 aa0182)
        {
            var sql = @"INSERT INTO MI_WEXPINV (WH_NO, MMCODE, EXP_DATE, LOT_NO, INV_QTY, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                        VALUES (:WH_NO, :MMCODE, TO_DATE(:EXP_DATE,'YYYY-MM-DD'), :LOT_NO, :INV_QTY,  :UPDATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, aa0182, DBWork.Transaction);
        }

        public int Delete(string wh_no, string mmcode, string lot_no, string exp_date)
        {
            string lot_no_cond = " and LOT_NO = :LOT_NO ";
            if (string.IsNullOrEmpty(lot_no))
                lot_no_cond = " and trim(LOT_NO) is null ";
            else if (lot_no.Trim() == "")
                lot_no_cond = " and trim(LOT_NO) is null ";

            exp_date = exp_date.PadLeft(7, '0');

            var sql = @"DELETE FROM MI_WEXPINV
                         WHERE WH_NO = :wh_no 
                           AND MMCODE = :mmcode 
                           " + lot_no_cond + @"
                           AND TRUNC(EXP_DATE, 'DD') = TO_DATE(:exp_date,'YYYY-MM-DD')";
            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, MMCODE = mmcode, LOT_NO = lot_no, EXP_DATE = exp_date }, DBWork.Transaction);
        }

        public string GetHospCode()
        {
            string sql = @" SELECT data_value FROM PARAM_D WHERE grp_code = 'HOSP_INFO' AND data_name = 'HospCode' ";
            return DBWork.Connection.ExecuteScalar<string>(sql, DBWork.Transaction);
        }
    }
}