using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using TSGH.Models;
using System.Collections.Generic;

namespace WebApp.Repository.B
{
    public class BD0021_MODEL : JCLib.Mvc.BaseModel
    {
        public string PO_NO { get; set; }   // 訂單號碼
        public string AGEN_NAME { get; set; } // 廠商代碼+名稱
        public string PO_TIME { get; set; }  // 訂單日期(民國年月日)
        public string M_CONTID { get; set; }    // 合約識別碼
        public string PO_STATUS { get; set; }    // 狀態
        public string MAT_CLASS { get; set; }  // 物料分類
        public string WH_NO { get; set; }    // 庫房別
        public string MMCODE { get; set; } // 院內碼
        public string MMNAME { get; set; }   // 品名
        public string M_PURUN { get; set; } // 申購計量單位
        public string PO_PRICE { get; set; }  // 訂單單價
        public string PO_QTY { get; set; }    // 訂單數量
        public string PO_AMT { get; set; }    // 總金額
        public string UPRICE { get; set; }  // 院內最小計量單位
        public string UNIT_SWAP { get; set; }    // 採購計量單位轉換率
        public string M_DISCPERC { get; set; } // 折讓比
        public string ISWILLING { get; set; }   // 單次訂購達優惠數量折讓意願
        public string DISCOUNT_QTY { get; set; } // 單次採購優惠數量
        public string DISC_COST_UPRICE { get; set; }  // 單次訂購達優惠數量成本價
        public string DELI_QTY { get; set; }    // 已交數量
        public string PR_NO { get; set; }    // 申購單號
    }

    public class BD0021Repository : JCLib.Mvc.BaseRepository
    {
        public BD0021Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<BD0021_MODEL> GetAllM(string wh_no, string mat_class, string mmcode, string m_contid, string strPoNo, string po_time_from, string po_time_to,string AGEN_NO, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT DISTINCT a.PO_NO,
                                a.AGEN_NO || ' ' || (SELECT AGEN_NAMEC FROM PH_VENDER p WHERE p.AGEN_NO = a.AGEN_NO) AS AGEN_NAME,
                                TWN_DATE(a.PO_TIME) AS PO_TIME,
                                a.M_CONTID || ' ' || (SELECT DATA_DESC FROM PARAM_D d WHERE d.GRP_CODE = 'MM_PO_M' AND d.DATA_NAME = 'M_CONTID' AND DATA_VALUE = a.M_CONTID) AS M_CONTID,
                                a.PO_STATUS || ' ' || (SELECT DATA_DESC FROM PARAM_D d WHERE d.GRP_CODE = 'MM_PO_M' AND d.DATA_NAME = 'PO_STATUS' AND DATA_VALUE = a.PO_STATUS) AS PO_STATUS,
                                a.MAT_CLASS || ' ' || (SELECT MAT_CLSNAME FROM MI_MATCLASS m WHERE m.MAT_CLASS = a.MAT_CLASS) AS MAT_CLASS,
                                a.WH_NO
                        FROM   MM_PO_M a
                                join MM_PO_D b
                                    ON a.PO_NO = b.PO_NO
                                join MI_MAST c
                                    ON b.MMCODE = c.MMCODE 
                        WHERE 1 = 1
                                ";

            if (!string.IsNullOrWhiteSpace(wh_no))  // 庫房代碼
            {
                sql += " AND a.WH_NO = :WH_NO ";
                p.Add(":WH_NO", wh_no);
            }

            if (!string.IsNullOrWhiteSpace(mat_class)) // 物料分類
            {
                if (mat_class.Contains("SUB_"))
                {
                    sql += " and c.MAT_CLASS_SUB = :MAT_CLASS ";
                    p.Add(":MAT_CLASS", mat_class.Replace("SUB_", ""));
                }
                else
                {
                    sql += " AND a.MAT_CLASS = :MAT_CLASS ";
                    p.Add(":MAT_CLASS", mat_class);
                }
            }

            if (!string.IsNullOrWhiteSpace(mmcode)) // 院內碼
            {
                sql += " AND b.MMCODE = :MMCODE ";
                p.Add(":MMCODE", mmcode);
            }

            if (!string.IsNullOrWhiteSpace(m_contid)) // 是否合約
            {
                sql += " AND a.M_CONTID = :M_CONTID ";
                p.Add(":M_CONTID", m_contid);
            }

            if (!string.IsNullOrEmpty(strPoNo)) // 訂單編號
            {
                sql += " AND a.PO_NO = :PO_NO ";
                p.Add(":PO_NO", strPoNo);
            }

            if (!string.IsNullOrWhiteSpace(po_time_from)) // 訂單日期起
            {
                sql += " AND TWN_DATE(a.PO_TIME) >= :PO_TIME_FROM ";
                p.Add(":PO_TIME_FROM", po_time_from);
            }

            if (!string.IsNullOrWhiteSpace(po_time_to)) // 訂單日期訖
            {
                sql += " AND TWN_DATE(a.PO_TIME) <= :PO_TIME_TO ";
                p.Add(":PO_TIME_TO", po_time_to);
            }

            if (!string.IsNullOrWhiteSpace(AGEN_NO)) // 廠商代碼
            {
                sql += " AND AGEN_NO = :AGEN_NO ";
                p.Add(":AGEN_NO", AGEN_NO);
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<BD0021_MODEL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<BD0021_MODEL> GetAllD(string strPoNo, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"
                                    SELECT a.PO_NO, --訂單號碼
                                           b.MMCODE, --院內碼
                                           c.MMNAME_C || ' ' || c.MMNAME_E AS MMNAME, --品名
                                           b.M_PURUN, --申購計量單位
                                           b.PO_PRICE, --訂單單價
                                           b.PO_QTY, --訂單數量
                                           b.PO_AMT, --總金額
                                           b.base_unit, --院內最小計量單位
                                           b.disc_cprice, --成本價
                                           b.UNIT_SWAP, --採購計量單位轉換率
                                           b.M_DISCPERC, --折讓比
                                           b.ISWILLING, --單次訂購達優惠數量折讓意願
                                           b.DISCOUNT_QTY, --單次採購優惠數量
                                           b.DISC_COST_UPRICE, --單次訂購達優惠數量成本價
                                           b.DELI_QTY, --已交數量
                                           b.PR_NO --申購單號
                                    FROM   MM_PO_M a
                                           join MM_PO_D b
                                             ON a.PO_NO = b.PO_NO
                                           join MI_MAST c
                                             ON b.MMCODE = c.MMCODE 
                                    WHERE 1 = 1
                                ";
            
            if (!string.IsNullOrEmpty(strPoNo)) // 訂單編號
            {
                sql += " AND a.PO_NO = :PO_NO ";
                p.Add(":PO_NO", strPoNo);
            }
            
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<BD0021_MODEL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetWhCombo()
        {
            string sql = @" 
                                        SELECT wh_no AS VALUE,
                                               wh_no || ' ' || wh_name AS COMBITEM,
                                               wh_kind
                                        FROM   MI_WHMAST
                                        WHERE  wh_no IN( WHNO_MM1, WHNO_ME1 )
                                        ORDER  BY wh_no 
                                    ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, DBWork.Transaction);
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
                                                 WHERE  wh_no = WHNO_MM1)
                                        SELECT b.mat_class AS VALUE,
                                               b.mat_class || ' ' || b.mat_clsname AS COMBITEM
                                        FROM   temp_whnos a,
                                               MI_MATCLASS b
                                        WHERE  a.taskId = b.mat_clsid ";

            if (!string.IsNullOrWhiteSpace(wh_no))
            {
                sql += " AND a.wh_no = :wh_no ";
            }

            sql += @"                   union
                                        select 'SUB_' || b.data_value as value, 
                                        b.data_value || ' ' || b.data_desc as COMBITEM 
                                        from temp_whnos a, PARAM_D b
	                                     where b.grp_code ='MI_MAST' 
	                                       and b.data_name = 'MAT_CLASS_SUB'
	                                       and b.data_value = '1'
	                                       and trim(b.data_desc) is not null
                                           and (a.taskId = '1')
                                    ";

            if (!string.IsNullOrWhiteSpace(wh_no))
            {
                sql += " AND a.wh_no = :wh_no ";
                p.Add(":wh_no", wh_no);
            }

            sql += @"                   union
                                        select 'SUB_' || b.data_value as value, 
                                        b.data_value || ' ' || b.data_desc as COMBITEM 
                                        from temp_whnos a, PARAM_D b
	                                     where b.grp_code ='MI_MAST' 
	                                       and b.data_name = 'MAT_CLASS_SUB'
	                                       and b.data_value <> '1'
	                                       and trim(b.data_desc) is not null
                                           and (a.taskId = '2')
                                    ";

            if (!string.IsNullOrWhiteSpace(wh_no))
            {
                sql += " AND a.wh_no = :wh_no ";
                p.Add(":wh_no", wh_no);
            }

            sql += " ORDER BY value ";

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
                                           --AND nvl(cancel_id, 'N') = 'N'
                                ";

            if (!string.IsNullOrWhiteSpace(mat_class))
            {
                sql += " AND a.mat_class = :mat_class ";
                p.Add(":mat_class", mat_class);
            }

            if (!string.IsNullOrWhiteSpace(p0))
            {
                sql = string.Format(sql, "(NVL(INSTR(UPPER(A.MMCODE), UPPER(:MMCODE_I)), 1000) + NVL(INSTR(UPPER(A.MMNAME_E), :MMNAME_E_I), 100) * 10 + NVL(INSTR(UPPER(A.MMNAME_C), UPPER(:MMNAME_C_I)), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
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

        public IEnumerable<COMBO_MODEL> GetPoTime()
        {
            string sql = @" 
                                        SELECT TWN_YYYMM(add_months(SYSDATE, -1)) || '01' AS VALUE,
                                               TWN_DATE(SYSDATE) AS TEXT
                                        FROM   DUAL 
                                    ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, DBWork.Transaction);
        }

        public DataTable GetExcel(string wh_no, string mat_class, string mmcode, string m_contid, string strPoNo, string po_time_from, string po_time_to)
        {
            var p = new DynamicParameters();

            var sql = @"
                                    SELECT a.PO_NO AS 訂單號碼,
                                           a.AGEN_NO || ' ' || (SELECT AGEN_NAMEC FROM PH_VENDER p WHERE p.AGEN_NO = a.AGEN_NO) AS 廠商代碼,
                                           TWN_DATE(a.PO_TIME) AS 訂單日期,
                                           a.M_CONTID || ' ' || (SELECT DATA_DESC FROM PARAM_D d WHERE d.GRP_CODE = 'MM_PO_M' AND d.DATA_NAME = 'M_CONTID' AND DATA_VALUE = a.M_CONTID) AS 合約識別碼,
                                           a.PO_STATUS || ' ' || (SELECT DATA_DESC FROM PARAM_D d WHERE d.GRP_CODE = 'MM_PO_M' AND d.DATA_NAME = 'PO_STATUS' AND DATA_VALUE = a.PO_STATUS) AS 狀態,
                                           a.MAT_CLASS || ' ' || (SELECT MAT_CLSNAME FROM MI_MATCLASS m WHERE m.MAT_CLASS = a.MAT_CLASS) AS 物料分類,
                                           a.WH_NO AS 庫房別,
                                           b.MMCODE AS 院內碼,
                                           c.MMNAME_C || ' ' || c.MMNAME_E AS 品名,
                                           b.M_PURUN AS 申購計量單位,
                                           b.PO_PRICE AS 訂單單價,
                                           b.PO_QTY AS 訂單數量,
                                           b.PO_AMT AS 總金額,
                                           b.base_unit , --院內最小計量單位
                                          --b.disc_cprice as 成本價, 
                                          -- b.UNIT_SWAP, --採購計量單位轉換率
                                           b.M_DISCPERC AS 折讓比,
                                          -- b.ISWILLING AS 二次折讓意願,
                                          -- b.DISCOUNT_QTY AS 二次折讓數量,
                                          -- b.DISC_COST_UPRICE AS 二次折讓成本價,
                                           b.DELI_QTY AS 已交數量,
                                           b.PR_NO AS 申購單號
                                    FROM   MM_PO_M a
                                           join MM_PO_D b
                                             ON a.PO_NO = b.PO_NO
                                           join MI_MAST c
                                             ON b.MMCODE = c.MMCODE 
                                    WHERE 1 = 1
                                ";

            if (!string.IsNullOrWhiteSpace(wh_no))  // 庫房代碼
            {
                sql += " AND a.WH_NO = :WH_NO ";
                p.Add(":WH_NO", wh_no);
            }

            if (!string.IsNullOrWhiteSpace(mat_class)) // 物料分類
            {
                if (mat_class.Contains("SUB_"))
                {
                    sql += " AND c.MAT_CLASS_SUB = :MAT_CLASS ";
                    p.Add(":MAT_CLASS", mat_class.Replace("SUB_", ""));
                }
                else
                {
                    sql += " AND a.MAT_CLASS = :MAT_CLASS ";
                    p.Add(":MAT_CLASS", mat_class);
                }  
            }

            if (!string.IsNullOrWhiteSpace(mmcode)) // 院內碼
            {
                sql += " AND b.MMCODE = :MMCODE ";
                p.Add(":MMCODE", mmcode);
            }


            if (!string.IsNullOrWhiteSpace(m_contid)) // 是否合約
            {
                sql += " AND a.M_CONTID = :M_CONTID ";
                p.Add(":M_CONTID", m_contid);
            }

            if (!string.IsNullOrEmpty(strPoNo)) // 訂單編號
            {
                sql += " AND a.PO_NO = :PO_NO ";
                p.Add(":PO_NO", strPoNo);
            }

            if (!string.IsNullOrWhiteSpace(po_time_from)) // 訂單日期起
            {
                sql += " AND TWN_DATE(a.PO_TIME) >= :PO_TIME_FROM ";
                p.Add(":PO_TIME_FROM", po_time_from);
            }

            if (!string.IsNullOrWhiteSpace(po_time_to)) // 訂單日期訖
            {
                sql += " AND TWN_DATE(a.PO_TIME) <= :PO_TIME_TO ";
                p.Add(":PO_TIME_TO", po_time_to);
            }
            sql += "order by a.po_no, b.mmcode";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        // 訂購單
        public DataTable GetReportMain(string PO_NO)
        {
            var p = new DynamicParameters();

            var sql = @"select a.PO_NO, a.M_CONTID, a.AGEN_NO, c.AGEN_NAMEC, c.AGEN_TEL, c.AGEN_FAX, c.EMAIL, a.MAT_CLASS,
                a.MEMO, a.SMEMO, a.UPDATE_USER, a.ISCOPY, to_char(a.PO_TIME, 'yyyy/mm/dd') as PO_TIME, sum(round(b.PO_PRICE*b.PO_QTY))  as AMOUNT,
                (select DATA_VALUE from PARAM_D where GRP_CODE = 'HOSP_INFO' and DATA_NAME = 'HospRecAddr') as REC_ADDR,
                (select DATA_VALUE from PARAM_D where GRP_CODE = 'HOSP_INFO' and DATA_NAME = 'HospContact') as CONTACT
                from MM_PO_M a, MM_PO_D b, PH_VENDER c
                where a.PO_NO=b.PO_NO and a.AGEN_NO=c.AGEN_NO and a.PO_NO=:PO_NO and b.STATUS<>'D'
                group by a.PO_NO,a.M_CONTID,a.AGEN_NO,c.AGEN_NAMEC,c.AGEN_TEL,c.AGEN_FAX,c.EMAIL, a.MAT_CLASS, a.MEMO,a.SMEMO, a.UPDATE_USER,a.ISCOPY, a.PO_TIME
                                        ";
            
            p.Add(":PO_NO", PO_NO);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<MM_PO_D> GetReport(string PO_NO)
        {
            var p = new DynamicParameters();

            var sql = @"select rownum as ITEM_NO, a.MMCODE, (a.MMNAME_E|| ' ' || a.MMNAME_C) as mmname_c, a.M_PURUN, 
                               a.PO_QTY,  a.PO_PRICE, a.BASE_UNIT as E_ORDERUNIT, 
                               round(a.PO_QTY * a.PO_PRICE) as SUM_PO_PRICE,  
                               (case when a.E_SOURCECODE = 'P' then '買斷' when a.E_SOURCECODE = 'C' then '寄庫' else '' end) as E_SOURCECODE,  
                               --(select SELF_CONT_EDATE from MED_SELFPUR_DEF where MMCODE = a.MMCODE and twn_date(sysdate) >= SELF_CONT_BDATE and twn_date(sysdate) <= SELF_CONT_EDATE and rownum = 1) as SELF_CONT_EDATE, 
                               twn_date(a.e_codate) as SELF_CONT_EDATE,
                               --(select SELF_CONTRACT_NO from MED_SELFPUR_DEF where MMCODE = a.MMCODE and twn_date(sysdate) >= SELF_CONT_BDATE and twn_date(sysdate) <= SELF_CONT_EDATE and rownum = 1) as SELF_CONTRACT_NO, 
                               a.caseno as SELF_CONTRACT_NO,
                               '' as BATCH_DELI_DATE, 
                               a.MEMO,a.PARTIALDL_DT
                          from MM_PO_D a
                         where a.PO_NO=:po_no and a.STATUS<>'D'
                         order by a.mmcode  ";

            p.Add(":PO_NO", PO_NO);

            return DBWork.Connection.Query<MM_PO_D>(sql, p, DBWork.Transaction);
        }

        public DataTable GetReport_BD0014_2(string PO_NO)
        {
            var p = new DynamicParameters();

            var sql = @"select a.PO_NO, a.M_CONTID, a.AGEN_NO, c.AGEN_NAMEC, c.AGEN_TEL, c.AGEN_FAX, c.EMAIL, a.MAT_CLASS,
                a.MEMO  AS A_MEMO, a.SMEMO, a.UPDATE_USER, a.ISCOPY, to_char(a.PO_TIME, 'yyyy/mm/dd') as PO_TIME
               -- , sum(round(D.PO_PRICE*D.PO_QTY))  as AMOUNT
                ,
                (select DATA_VALUE from PARAM_D where GRP_CODE = 'HOSP_INFO' and DATA_NAME = 'HospRecAddr') as REC_ADDR,
                (select DATA_VALUE from PARAM_D where GRP_CODE = 'HOSP_INFO' and DATA_NAME = 'HospContact') as CONTACT
                , D.MMCODE, D.MMNAME_C, D.M_PURUN, 
                 D.PO_QTY,  D.PO_PRICE, D.BASE_UNIT as E_ORDERUNIT, 
                 round(D.PO_QTY * D.PO_PRICE) as SUM_PO_PRICE,  
                 (case when D.E_SOURCECODE = 'P' then '買斷' when D.E_SOURCECODE = 'C' then '寄庫' else '' end) as E_SOURCECODE,  
                 
                 twn_date(D.e_codate) as SELF_CONT_EDATE,
             
                 D.caseno as SELF_CONTRACT_NO,
                 '' as BATCH_DELI_DATE, 
                 D.MEMO AS D_MEMO,
                 D.PARTIALDL_DT AS D_PARTIALDL_DT

                from MM_PO_M a, MM_PO_D D, PH_VENDER c
                where a.PO_NO=D.PO_NO and a.AGEN_NO=c.AGEN_NO
                  and D.STATUS<>'D'
                  and a.PO_NO in :PO_NO                  
                order by a.PO_NO,D.MMCODE
                ";

            var PO_NOs = PO_NO.Split(',');
            p.Add(":PO_NO", PO_NOs);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }


        // 未到貨明細表
        public DataTable GetReportMain_1()
        {
            var p = new DynamicParameters();

            var sql = @"select 
                (select DATA_VALUE from PARAM_D where GRP_CODE = 'HOSP_INFO' and DATA_NAME = 'HospRecAddr') as REC_ADDR,
                (select DATA_VALUE from PARAM_D where GRP_CODE = 'HOSP_INFO' and DATA_NAME = 'HospContact') as CONTACT,
                (select DATA_VALUE from PARAM_D where GRP_CODE = 'HOSP_INFO' and DATA_NAME = 'HospFax') as HOSP_FAX
                from dual
                ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<MM_PO_D> GetReport_1(string WH_NO, string MAT_CLASS, string MMCODE, string M_CONTID, string PO_NO, string PO_TIME_FROM, string PO_TIME_TO)
        {
            var p = new DynamicParameters();
            //1121120 桃園需顯示部分到貨(不含超收)
            var sql = @"select 
                A.PO_NO, A.AGEN_NO || ' ' || D.AGEN_NAMEC as AGEN_NO, D.AGEN_TEL, D.AGEN_FAX, D.EMAIL, A.MAT_CLASS,
                A.MEMO, A.SMEMO, 
                to_number(to_char(A.PO_TIME, 'yyyy') - 1911) || '年' || to_char(A.PO_TIME, 'mm') || '月' || to_char(A.PO_TIME, 'dd') || '日' as PO_TIME, 
                C.MMCODE,C.MMNAME_C, B.M_PURUN, 
                B.PO_QTY,  B.PO_PRICE, C.E_ORDERUNIT, 
                round(B.PO_QTY * B.PO_PRICE) as SUM_PO_PRICE,  
                B.DELI_QTY,
                B.MEMO as MEMO_D,
                (select listagg(STORE_LOC, ',') within group (order by MMCODE) from MI_WLOCINV where WH_NO = A.WH_NO and MMCODE = B.MMCODE) as STORE_LOC
                from MM_PO_M A, MM_PO_D B, MI_MAST C, PH_VENDER D
                where A.PO_NO=B.PO_NO and B.MMCODE=C.MMCODE and A.AGEN_NO = D.AGEN_NO and B.STATUS<>'D' and (B.PO_QTY - B.DELI_QTY) > 0
                ";

            if (!string.IsNullOrWhiteSpace(WH_NO))  // 庫房代碼
            {
                sql += " AND A.WH_NO = :WH_NO ";
                p.Add(":WH_NO", WH_NO);
            }

            if (!string.IsNullOrWhiteSpace(MAT_CLASS)) // 物料分類
            {
                if (MAT_CLASS.Contains("SUB_"))
                {
                    sql += " AND C.MAT_CLASS_SUB = :MAT_CLASS ";
                    p.Add(":MAT_CLASS", MAT_CLASS.Replace("SUB_", ""));
                }
                else
                {
                    sql += " AND A.MAT_CLASS = :MAT_CLASS ";
                    p.Add(":MAT_CLASS", MAT_CLASS);
                }  
            }

            if (!string.IsNullOrWhiteSpace(MMCODE)) // 院內碼
            {
                sql += " AND B.MMCODE = :MMCODE ";
                p.Add(":MMCODE", MMCODE);
            }

            if (!string.IsNullOrWhiteSpace(M_CONTID)) // 是否合約
            {
                sql += " AND A.M_CONTID = :M_CONTID ";
                p.Add(":M_CONTID", M_CONTID);
            }


            if (!string.IsNullOrWhiteSpace(PO_NO)) // 是否合約
            {
                sql += " AND A.PO_NO = :PO_NO ";
                p.Add(":PO_NO", PO_NO);
            }

            if (!string.IsNullOrWhiteSpace(PO_TIME_FROM)) // 訂單日期起
            {
                sql += " AND TWN_DATE(A.PO_TIME) >= :PO_TIME_FROM ";
                p.Add(":PO_TIME_FROM", PO_TIME_FROM);
            }

            if (!string.IsNullOrWhiteSpace(PO_TIME_TO)) // 訂單日期訖
            {
                sql += " AND TWN_DATE(A.PO_TIME) <= :PO_TIME_TO ";
                p.Add(":PO_TIME_TO", PO_TIME_TO);
            }
            sql += " order by B.MMCODE  ";

            return DBWork.Connection.Query<MM_PO_D>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<PH_VENDER> GetAgennoCombo(string p0, int page_index, int page_size, string sorters)
        {

            var p = new DynamicParameters();

            var sql = @"SELECT {0} AGEN_NO, AGEN_NAMEC, AGEN_NAMEE
                        FROM PH_VENDER WHERE (REC_STATUS<>'X' OR REC_STATUS is null)";
            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(AGEN_NO, :AGEN_NO_I), 1000) + NVL(INSTR(AGEN_NAMEC, :AGEN_NAMEC_I), 100) * 10 + NVL(INSTR(AGEN_NAMEE, :AGEN_NAMEE_I), 100) * 10) IDX,");
                p.Add(":AGEN_NO_I", p0);
                p.Add(":AGEN_NAMEC_I", p0);
                p.Add(":AGEN_NAMEE_I", p0);

                sql += " AND (AGEN_NO LIKE :AGEN_NO ";
                p.Add(":AGEN_NO", string.Format("{0}%", p0));

                sql += " OR AGEN_NAMEC LIKE :AGEN_NAMEC ";
                p.Add(":AGEN_NAMEC", string.Format("%{0}%", p0));

                sql += " OR AGEN_NAMEE LIKE :AGEN_NAMEE) ";
                p.Add(":AGEN_NAMEE", string.Format("%{0}%", p0));

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

    }
}