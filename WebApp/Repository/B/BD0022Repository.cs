using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using TSGH.Models;
using System.Collections.Generic;

namespace WebApp.Repository.B
{
    public class BD0022Repository : JCLib.Mvc.BaseRepository
    {
        public BD0022Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<BD0022> GetAll(string p0, string p0_1, string p1, string p2, string p3, string p3_1, string p4, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"select A.PO_NO, TWN_DATE(A.PO_TIME) as PO_TIME, A.AGEN_NO,
                                               (SELECT nvl(easyname,agen_namec) FROM ph_vender WHERE agen_no = a.agen_no ) AS agen_namec,
                                              (select sum(PO_AMT) from MM_PO_D where PO_NO=A.PO_NO) as PO_AMT, A.MEMO
                                    from MM_PO_M A
                                 where 1=1 AND A.PO_STATUS<>'37'
                                     and (select count(*) from MM_PO_D where PO_NO=A.PO_NO and STATUS<>'D' and (PO_QTY-DELI_QTY)>0 and DELI_STATUS <> 'Y')>0";

            if (!string.IsNullOrWhiteSpace(p0))  //訂單號碼
            {
                if (!string.IsNullOrWhiteSpace(p0_1))
                {
                    sql += " and A.PO_NO between :p0 and :p0_1";
                    p.Add(":p0", string.Format("{0}", p0));
                    p.Add(":p0_1", string.Format("{0}", p0_1));
                }
                else
                {
                    sql += " and A.PO_NO=:p0";
                    p.Add(":p0", string.Format("{0}", p0));
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(p0_1))
                {
                    sql += " and A.PO_NO=:p0_1";
                    p.Add(":p0_1", string.Format("{0}", p0_1));
                }
            }
            if (!string.IsNullOrWhiteSpace(p1))
            {
                if (p1.Contains("SUB_"))
                {
                    sql += " and (select count(*) from MM_PO_D C left join MI_MAST D on C.MMCODE = D.MMCODE where A.PO_NO = C.PO_NO and D.MAT_CLASS_SUB = :p1) > 0";
                    p.Add(":p1", string.Format("{0}", p1.Replace("SUB_", "")));
                }
                else
                {
                    sql += " and A.MAT_CLASS = :p1 ";
                    p.Add(":p1", string.Format("{0}", p1));
                }
            }
            if (!string.IsNullOrWhiteSpace(p2))  //合約識別碼
            {
                sql += " and A.M_CONTID=:p2";
                p.Add(":p2", string.Format("{0}", p2));
            }
            if (!string.IsNullOrWhiteSpace(p3)) // 訂單日期起
            {
                sql += " AND TWN_DATE(A.PO_TIME) >= :p3 ";
                p.Add(":p3", string.Format("{0}", p3));
            }
            if (!string.IsNullOrWhiteSpace(p3_1)) // 訂單日期訖
            {
                sql += " AND TWN_DATE(A.PO_TIME) <= :p3_1 ";
                p.Add(":p3_1", string.Format("{0}", p3_1));
            }
            if (!string.IsNullOrWhiteSpace(p4))  //廠商代碼
            {
                sql += " and A.AGEN_NO=:p4";
                p.Add(":p4", string.Format("{0}", p4));
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<BD0022>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<BD0022> GetAllD(string po_no, string chk_deli_y, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"select A.PO_NO, A.MMCODE,(A.MMNAME_E||' '||A.MMNAME_C) as MMNAME,
                                               A.M_PURUN,A.BASE_UNIT,A.PO_QTY,A.PO_PRICE,A.PO_AMT,A.MEMO, A.DELI_STATUS
                                    from MM_PO_D A
                                 where A.STATUS <> 'D' ";

            if (!string.IsNullOrWhiteSpace(po_no))  //訂單號碼
            {
                sql += " and A.PO_NO=:po_no";
                p.Add(":po_no", string.Format("{0}", po_no));
            }

            if (chk_deli_y != "Y")
            {
                sql += " and A.DELI_STATUS <> 'Y' ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<BD0022>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<COMBO_MODEL> GetMatclassCombo()
        {
            string sql = @"select MAT_CLASS VALUE, MAT_CLASS||' '||'全部'||MAT_CLSNAME COMBITEM from MI_MATCLASS
                                        where MAT_CLSID in ('1','2')
                                         union
                                        select 'SUB_' || data_value as value, data_value || ' ' || data_desc as COMBITEM
                                          from PARAM_D
	                                   where grp_code ='MI_MAST' 
	                                       and data_name = 'MAT_CLASS_SUB'
	                                       and trim(data_desc) is not null
                                     ORDER BY VALUE";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, null);
        }
        public IEnumerable<COMBO_MODEL> GetMcontidCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                                  DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                             FROM PARAM_D
                            WHERE GRP_CODE='MM_PO_M' AND DATA_NAME='M_CONTID' and DATA_VALUE in ('0','2')
                            ORDER BY DATA_VALUE";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, null);
        }

        public IEnumerable<COMBO_MODEL> GetPoTime()
        {
            string sql = @" 
                                        SELECT TWN_YYYMM(add_months(SYSDATE, -1)) || '01' AS VALUE,
                                               TWN_DATE(SYSDATE) AS TEXT
                                        FROM   DUAL  ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, DBWork.Transaction);
        }
        public DataTable GetExcel(string p0, string p0_1, string p1, string p2, string p3, string p3_1,string p4)
        {
            var p = new DynamicParameters();
            var sql = @"select A.PO_NO as 訂單號碼, TWN_DATE(A.PO_TIME) as  訂單日期, A.AGEN_NO as 廠商代碼,
                                              (select NVL(EASYNAME,AGEN_NAMEC) from PH_VENDER where AGEN_NO=A.AGEN_NO) as 廠商名稱,
                                              B.MMCODE as 院內碼, (B.MMNAME_E||' '||B.MMNAME_C) as 藥材名稱, B.M_PURUN as 包裝,B.BASE_UNIT as 單位,
                                              B.PO_QTY as 訂單數量, B.PO_PRICE as 訂單單價, B.PO_AMT as 小計,B.MEMO as 備註
                                    from MM_PO_M A, MM_PO_D B
                                 where A.PO_NO=B.PO_NO and A.PO_STATUS<>'37'
                                     and B.STATUS<>'D' and (B.PO_QTY - B.DELI_QTY)> 0
                                     and B.DELI_STATUS <> 'Y' ";

            if (!string.IsNullOrWhiteSpace(p0))  //訂單號碼
            {
                if (!string.IsNullOrWhiteSpace(p0_1))
                {
                    sql += " and A.PO_NO between :p0 and :p0_1";
                    p.Add(":p0", string.Format("{0}", p0));
                    p.Add(":p0_1", string.Format("{0}", p0_1));
                }
                else
                {
                    sql += " and A.PO_NO=:p0";
                    p.Add(":p0", string.Format("{0}", p0));
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(p0_1))
                {
                    sql += " and A.PO_NO=:p0_1";
                    p.Add(":p0_1", string.Format("{0}", p0_1));
                }
            }
            if (!string.IsNullOrWhiteSpace(p1))
            {
                if (p1.Contains("SUB_"))
                {
                    sql += " and (select count(*) from MM_PR_D C left join MI_MAST D on C.MMCODE = D.MMCODE where A.PO_NO = C.PO_NO and D.MAT_CLASS_SUB = :p1) > 0";
                    p.Add(":p1", string.Format("{0}", p1.Replace("SUB_", "")));
                }
                else
                {
                    sql += " and A.MAT_CLASS = :p1 ";
                    p.Add(":p1", string.Format("{0}", p1));
                }
            }
            if (!string.IsNullOrWhiteSpace(p2))  //合約識別碼
            {
                sql += " and A.M_CONTID=:p2";
                p.Add(":p2", string.Format("{0}", p2));
            }
            if (!string.IsNullOrWhiteSpace(p3)) // 訂單日期起
            {
                sql += " AND TWN_DATE(A.PO_TIME) >= :p3 ";
                p.Add(":p3", string.Format("{0}", p3));
            }
            if (!string.IsNullOrWhiteSpace(p3_1)) // 訂單日期訖
            {
                sql += " AND TWN_DATE(A.PO_TIME) <= :p3_1 ";
                p.Add(":p3_1", string.Format("{0}", p3_1));
            }
            if (!string.IsNullOrWhiteSpace(p4))  //廠商代碼
            {
                sql += " and A.AGEN_NO=:p4";
                p.Add(":p4", string.Format("{0}", p4));
            }
            sql += " order by A.PO_NO desc";
            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
        public string GetHospName()
        {
            string sql = @" SELECT data_value FROM PARAM_D WHERE grp_code = 'HOSP_INFO' AND data_name = 'HospName' ";
            return DBWork.Connection.ExecuteScalar<string>(sql, DBWork.Transaction);
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
                where a.PO_NO=b.PO_NO and a.AGEN_NO=c.AGEN_NO and a.PO_NO=:PO_NO and b.STATUS<>'D' and b.DELI_STATUS <> 'Y'
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

            var sql = @"select rownum as ITEM_NO, a.MMCODE, a.MMNAME_C, a.M_PURUN, 
                               a.PO_QTY,  a.PO_PRICE, a.BASE_UNIT as E_ORDERUNIT, 
                               round(a.PO_QTY * a.PO_PRICE) as SUM_PO_PRICE,  
                               (case when a.E_SOURCECODE = 'P' then '買斷' when a.E_SOURCECODE = 'C' then '寄庫' else '' end) as E_SOURCECODE,  
                               --(select SELF_CONT_EDATE from MED_SELFPUR_DEF where MMCODE = a.MMCODE and twn_date(sysdate) >= SELF_CONT_BDATE and twn_date(sysdate) <= SELF_CONT_EDATE and rownum = 1) as SELF_CONT_EDATE, 
                               twn_date(a.e_codate) as SELF_CONT_EDATE,
                               --(select SELF_CONTRACT_NO from MED_SELFPUR_DEF where MMCODE = a.MMCODE and twn_date(sysdate) >= SELF_CONT_BDATE and twn_date(sysdate) <= SELF_CONT_EDATE and rownum = 1) as SELF_CONTRACT_NO, 
                               a.caseno as SELF_CONTRACT_NO,
                               '' as BATCH_DELI_DATE, 
                               a.MEMO
                          from MM_PO_D a,MI_MAST b 
                         where a.MMCODE=b.MMCODE and a.PO_NO=:po_no and a.STATUS<>'D' and a.DELI_STATUS <> 'Y'
                         order by a.mmcode  ";

            p.Add(":PO_NO", PO_NO);

            return DBWork.Connection.Query<MM_PO_D>(sql, p, DBWork.Transaction);
        }
        //廠商代碼
        public IEnumerable<PH_VENDER> GetAgennoCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = @"select {0} AGEN_NO, AGEN_NAMEC, AGEN_NAMEE, AGEN_NO||' '||AGEN_NAMEC as EASYNAME
                            from PH_VENDER where (REC_STATUS <> 'X' or REC_STATUS is null) ";

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
        public IEnumerable<BD0022> GetListReport(string p0, string p0_1, string p1, string p2, string p3, string p3_1, string p4)
        {
            var p = new DynamicParameters();
            var sql = @"select A.PO_NO, A.AGEN_NO,
                                              (select NVL(EASYNAME,AGEN_NAMEC) from PH_VENDER where AGEN_NO=A.AGEN_NO) as AGEN_NAMEC,
                                              B.MMCODE, (B.MMNAME_E||' '||B.MMNAME_C) as MMNAME,B.PO_QTY,  B.BASE_UNIT,
                                              B.PO_PRICE, B.PO_AMT
                                    from MM_PO_M A, MM_PO_D B
                                 where A.PO_NO=B.PO_NO and A.PO_STATUS<>'37'
                                     and B.STATUS <> 'D' and (B.PO_QTY - B.DELI_QTY)>0
                                     and B.DELI_STATUS <> 'Y' ";

            if (!string.IsNullOrWhiteSpace(p0))  //訂單號碼
            {
                if (!string.IsNullOrWhiteSpace(p0_1))
                {
                    sql += " and A.PO_NO between :p0 and :p0_1";
                    p.Add(":p0", string.Format("{0}", p0));
                    p.Add(":p0_1", string.Format("{0}", p0_1));
                }
                else
                {
                    sql += " and A.PO_NO=:p0";
                    p.Add(":p0", string.Format("{0}", p0));
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(p0_1))
                {
                    sql += " and A.PO_NO=:p0_1";
                    p.Add(":p0_1", string.Format("{0}", p0_1));
                }
            }
            if (!string.IsNullOrWhiteSpace(p1))
            {
                if (p1.Contains("SUB_"))
                {
                    sql += " and (select count(*) from MM_PR_D C left join MI_MAST D on C.MMCODE = D.MMCODE where A.PO_NO = C.PO_NO and D.MAT_CLASS_SUB = :p1) > 0";
                    p.Add(":p1", string.Format("{0}", p1.Replace("SUB_", "")));
                }
                else
                {
                    sql += " and A.MAT_CLASS = :p1 ";
                    p.Add(":p1", string.Format("{0}", p1));
                }
            }
            if (!string.IsNullOrWhiteSpace(p2))  //合約識別碼
            {
                sql += " and A.M_CONTID=:p2";
                p.Add(":p2", string.Format("{0}", p2));
            }
            if (!string.IsNullOrWhiteSpace(p3)) // 訂單日期起
            {
                sql += " and TWN_DATE(A.PO_TIME) >= :p3 ";
                p.Add(":p3", string.Format("{0}", p3));
            }
            if (!string.IsNullOrWhiteSpace(p3_1)) // 訂單日期訖
            {
                sql += " and TWN_DATE(A.PO_TIME) <= :p3_1 ";
                p.Add(":p3_1", string.Format("{0}", p3_1));
            }
            if (!string.IsNullOrWhiteSpace(p4))  //廠商代碼
            {
                sql += " and A.AGEN_NO=:p4";
                p.Add(":p4", string.Format("{0}", p4));
            }
            sql += " order by po_no asc";

            return DBWork.Connection.Query<BD0022>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<BD0022> GetListReport(string p0, string p0_1, string p1, string p2, string p3, string p3_1, string p4, string[] str_pono)
        {
            var p = new DynamicParameters();
            var sql = @"select A.PO_NO, A.AGEN_NO,
                                              (select NVL(EASYNAME,AGEN_NAMEC) from PH_VENDER where AGEN_NO=A.AGEN_NO) as AGEN_NAMEC,
                                              B.MMCODE, (B.MMNAME_E||' '||B.MMNAME_C) as MMNAME,B.PO_QTY,  B.BASE_UNIT,
                                              B.PO_PRICE, B.PO_AMT
                                    from MM_PO_M A, MM_PO_D B
                                 where A.PO_NO=B.PO_NO and A.PO_STATUS<>'37'
                                     and B.STATUS <> 'D' and (B.PO_QTY - B.DELI_QTY)>0
                                     and B.DELI_STATUS <> 'Y' ";

            if (!string.IsNullOrWhiteSpace(p0))  //訂單號碼
            {
                if (!string.IsNullOrWhiteSpace(p0_1))
                {
                    sql += " and A.PO_NO between :p0 and :p0_1";
                    p.Add(":p0", string.Format("{0}", p0));
                    p.Add(":p0_1", string.Format("{0}", p0_1));
                }
                else
                {
                    sql += " and A.PO_NO=:p0";
                    p.Add(":p0", string.Format("{0}", p0));
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(p0_1))
                {
                    sql += " and A.PO_NO=:p0_1";
                    p.Add(":p0_1", string.Format("{0}", p0_1));
                }
            }
            if (!string.IsNullOrWhiteSpace(p1))
            {
                if (p1.Contains("SUB_"))
                {
                    sql += " and (select count(*) from MM_PR_D C left join MI_MAST D on C.MMCODE = D.MMCODE where A.PO_NO = C.PO_NO and D.MAT_CLASS_SUB = :p1) > 0";
                    p.Add(":p1", string.Format("{0}", p1.Replace("SUB_", "")));
                }
                else
                {
                    sql += " and A.MAT_CLASS = :p1 ";
                    p.Add(":p1", string.Format("{0}", p1));
                }
            }
            if (!string.IsNullOrWhiteSpace(p2))  //合約識別碼
            {
                sql += " and A.M_CONTID=:p2";
                p.Add(":p2", string.Format("{0}", p2));
            }
            if (!string.IsNullOrWhiteSpace(p3)) // 訂單日期起
            {
                sql += " and TWN_DATE(A.PO_TIME) >= :p3 ";
                p.Add(":p3", string.Format("{0}", p3));
            }
            if (!string.IsNullOrWhiteSpace(p3_1)) // 訂單日期訖
            {
                sql += " and TWN_DATE(A.PO_TIME) <= :p3_1 ";
                p.Add(":p3_1", string.Format("{0}", p3_1));
            }
            if (!string.IsNullOrWhiteSpace(p4))  //廠商代碼
            {
                sql += " and A.AGEN_NO=:p4";
                p.Add(":p4", string.Format("{0}", p4));
            }

            if (str_pono.Length > 0)
            {
                string sql_pono = "";
                sql += @" AND (";
                foreach (string tmp_pono in str_pono)
                {
                    if (string.IsNullOrEmpty(sql_pono))
                    {
                        sql_pono = @"A.PO_NO = '" + tmp_pono + "'";
                    }
                    else
                    {
                        sql_pono += @" OR A.PO_NO = '" + tmp_pono + "'";
                    }
                }
                sql += sql_pono + ") ";
            }


            sql += " order by po_no asc";


            return DBWork.Connection.Query<BD0022>(sql, p, DBWork.Transaction);
        }
    }
}