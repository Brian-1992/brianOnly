using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using TSGH.Models;
using WebApp.Models.F;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace WebApp.Repository.F
{
    public class FA0017Repository : JCLib.Mvc.BaseRepository
    {
        public FA0017Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<FA0017> GetAll(string MATCLS, string YearMonth, string ContractType, string IsTBC)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT yyymm, agen_no, agen_namec, agen_bank, agen_sub, agen_acc, uni_no,
                          mat_class, m_contid, paysum , fullsum,  discsum
                        FROM PH_PO_MONPAY 
                        WHERE yyymm = :YearMonth 
                        AND mat_class = :MATCLS ";

            p.Add(":YearMonth", string.Format("{0}", YearMonth));
            p.Add(":MATCLS", string.Format("{0}", MATCLS));

            if (!string.IsNullOrEmpty(ContractType))
            {
                sql += @" AND m_contid = :ContractType ";
                p.Add(":ContractType", string.Format("{0}", ContractType));
            }
            //不為空且不等於不分區(2)
            if (!string.IsNullOrEmpty(IsTBC) && IsTBC != "2")
            {
                if (IsTBC == "0")
                {
                    sql += @" AND agen_bank = '006'";
                }
                else if (IsTBC == "1")
                {
                    sql += @" AND agen_bank <> '006'";
                }
            }

            sql += @" ORDER BY agen_no ";

            return DBWork.PagingQuery<FA0017>(sql, p, DBWork.Transaction);
        }
        public IEnumerable<FA0017> GetAll_2(string MATCLS, string yyymmdds, string yyymmdde, string IsSupply)
        {
            var p = new DynamicParameters();

            var sql = @"select a.agen_no, c.agen_namec, c.agen_acc,
                          sum(floor(d.po_price * a.acc_qty/a.unit_swap )) as FULLSUM
                        from bc_cs_acc_log a, mi_mast b, ph_vender c, mm_po_d d
                        where a.mmcode=b.mmcode and a.agen_no=c.agen_no and a.po_no=d.po_no and a.mmcode=d.mmcode 
                        and substr(a.po_no,1,3) not in ('TXT') ";
            if (MATCLS == "03")
                sql += " and a.mat_class in ('03','04','05','06','08') ";
            else
                sql += " and a.mat_class ='07' ";

            sql += @" and twn_date(a.acc_time) >= :yyymmdds
                          and twn_date(a.acc_time) <= :yyymmdde
                          and a.status='P' ";
            if (IsSupply == "Y")
                sql += " and b.m_supplyid='Y' ";
            else
                sql += " and b.m_supplyid='N' and m_contid='0' ";
            sql += @" group by a.agen_no, c.agen_namec, c.agen_acc
                          order by a.agen_no";

            p.Add(":yyymmdds", yyymmdds);
            p.Add(":yyymmdde", yyymmdde);
            return DBWork.PagingQuery<FA0017>(sql, p, DBWork.Transaction);
        }
        public DataTable GetTxt(string MATCLS, string YearMonth, string ContractType, string IsTBC)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT AGEN_ACC AS 銀行帳號,
                        (PAYSUM - TXFEE) AS 匯款金額,
                        AGEN_BANK AS 銀行代碼,
                        AGEN_SUB AS 分行代碼,
                        UNI_NO AS 統一編號,
                        AGEN_NAMEC AS 廠商名稱
                        FROM PH_PO_MONPAY 
                        WHERE yyymm = :YearMonth 
                         AND mat_class = :MATCLS ";


            p.Add(":YearMonth", string.Format("{0}", YearMonth));
            p.Add(":MATCLS", string.Format("{0}", MATCLS));

            if (!string.IsNullOrEmpty(ContractType))
            {
                sql += @" AND m_contid = :ContractType ";
                p.Add(":ContractType", string.Format("{0}", ContractType));
            }
            //不為空且不等於不分區(2)
            if (!string.IsNullOrEmpty(IsTBC) && IsTBC != "2")
            {
                if (IsTBC == "0")
                {
                    sql += @" AND agen_bank = '006'";
                }
                else if (IsTBC == "1")
                {
                    sql += @" AND agen_bank <> '006'";
                }
            }

            sql += @" ORDER BY agen_no, m_contid";

            sql = string.Format(@"select * 
                                    from ( {0} )
                                   where 匯款金額 <> 0", sql);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public DataTable GetExcel(string MATCLS, string YearMonth, string ContractType, string IsTBC)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT AGEN_NO AS 廠商碼,
                        AGEN_NAMEC AS 廠商名稱,
                        AGEN_BANK AS 銀行代碼,
                        AGEN_SUB AS 分行代碼,
                        AGEN_ACC AS 銀行帳號,
                        UNI_NO AS 統一編號,
                        PAYSUM AS 應收貨款,
                        FULLSUM AS 總貨款,
                        DISCSUM AS 管理費,
                        TXFEE AS 匯費,
                        MAT_CLASS AS 物料類別,
                        (case when m_contid='0' then '合約' when m_contid='2' then '非合約' 
                         when m_contid='3' then '小採' end)   AS 合約類別 
                        FROM PH_PO_MONPAY 
                        WHERE yyymm = :YearMonth 
                         AND mat_class = :MATCLS ";

            p.Add(":YearMonth", string.Format("{0}", YearMonth));
            p.Add(":MATCLS", string.Format("{0}", MATCLS));

            if (!string.IsNullOrEmpty(ContractType))
            {
                sql += @" AND m_contid = :ContractType ";
                p.Add(":ContractType", string.Format("{0}", ContractType));
            }
            //不為空且不等於不分區(2)
            if (!string.IsNullOrEmpty(IsTBC) && IsTBC != "2")
            {
                if (IsTBC == "0")
                {
                    sql += @" AND agen_bank = '006'";
                }
                else if (IsTBC == "1")
                {
                    sql += @" AND agen_bank <> '006'";
                }
            }

            sql += @" ORDER BY agen_no, m_contid";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
        public DataTable GetExcel2(string MATCLS, string yyymmdds, string yyymmdde, string IsSupply)
        {
            var p = new DynamicParameters();
            var sql = @"select a.agen_no AS 廠商碼, 
                               c.agen_namec AS 廠商名稱, 
                               c.agen_acc AS 銀行帳號,
                               sum(floor(d.po_price * a.acc_qty/a.unit_swap )) AS 申購總額  
                        from bc_cs_acc_log a, mi_mast b, ph_vender c, mm_po_d d
                        where a.mmcode=b.mmcode and a.agen_no=c.agen_no and a.po_no=d.po_no and a.mmcode=d.mmcode 
                        and substr(a.po_no,1,3) not in ('TXT') ";
            if (MATCLS == "03")
                sql += " and a.mat_class in ('03','04','05','06','08') ";
            else
                sql += " and a.mat_class ='07' ";

            sql += @" and twn_date(a.acc_time) >= :yyymmdds
                          and twn_date(a.acc_time) <= :yyymmdde
                          and a.status='P' ";
            if (IsSupply == "Y")
                sql += " and b.m_supplyid='Y' ";
            else
                sql += " and b.m_supplyid='N' and m_contid='0' ";
            sql += @" group by a.agen_no, c.agen_namec, c.agen_acc
                          order by a.agen_no";

            p.Add(":yyymmdds", yyymmdds);
            p.Add(":yyymmdde", yyymmdde);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
        public IEnumerable<FA0017> GetPrintData_All(string YearMonth, string ContractType, string MATCLS, string IsTBC)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT yyymm, AGEN_NO, AGEN_NAMEC, sum(fullsum) FULLSUM,
                        sum(DISCSUM) DISCSUM, sum(PAYSUM) PAYSUM, 
                        sum(DISCSUM) DISCSUM, sum(PAYSUM) PAYSUM, 
                        (select txfee from PH_PO_MONPAY where yyymm=a.yyymm and mat_class=a.mat_class and agen_no=a.agen_no and m_contid=a.m_contid) TXFEE,
                        (sum(PAYSUM)-sum(TXFEE)) PAY,
                        agen_bank, AGEN_SUB, AGEN_ACC
                        FROM PH_PO_MON a
                        WHERE yyymm = :YearMonth 
                        AND mat_class = :MATCLS
                        AND m_contid = :ContractType
                        ";
            //不為空且不等於不分區(2)
            if (!string.IsNullOrEmpty(IsTBC) && IsTBC != "2")
            {
                if (IsTBC == "0")
                {
                    sql += @" AND agen_bank = '006'";
                }
                else if (IsTBC == "1")
                {
                    sql += @" AND agen_bank <> '006'";
                }
            }
            sql += @" GROUP BY yyymm, mat_class, m_contid , agen_no, agen_namec, agen_bank, AGEN_SUB, AGEN_ACC
                        ORDER BY agen_no";
            p.Add(":YearMonth", string.Format("{0}", YearMonth));
            p.Add(":MATCLS", string.Format("{0}", MATCLS));
            p.Add(":ContractType", string.Format("{0}", ContractType));

            return DBWork.Connection.Query<FA0017>(sql, p, DBWork.Transaction);
        }
        public IEnumerable<FA0017> GetPrintData_All_2(string MATCLS, string yyymmdds, string yyymmdde, string IsSupply)
        {
            var p = new DynamicParameters();
            var sql = @"select a.agen_no, c.agen_namec, c.agen_acc,
                          sum(floor(d.po_price * a.acc_qty/a.unit_swap )) as FULLSUM
                        from bc_cs_acc_log a, mi_mast b, ph_vender c, mm_po_d d
                        where a.mmcode=b.mmcode and a.agen_no=c.agen_no and a.po_no=d.po_no and a.mmcode=d.mmcode
                        and substr(a.po_no,1,3) not in ('TXT') ";
            if (MATCLS == "03")
                sql += " and a.mat_class in ('03','04','05','06','08') ";
            else
                sql += " and a.mat_class ='07' ";

            sql += @" and twn_date(a.acc_time) >= :yyymmdds
                          and twn_date(a.acc_time) <= :yyymmdde
                          and a.status='P' ";
            if (IsSupply == "Y")
                sql += " and b.m_supplyid='Y' ";
            else
                sql += " and b.m_supplyid='N' and m_contid='0' ";
            sql += @" group by a.agen_no, c.agen_namec, c.agen_acc
                          order by a.agen_no";

            p.Add(":yyymmdds", yyymmdds);
            p.Add(":yyymmdde", yyymmdde);
            return DBWork.Connection.Query<FA0017>(sql, p, DBWork.Transaction);
        }
        public IEnumerable<FA0017> GetPrintData_Detail(string YearMonth, string ContractType, string MATCLS, string IsTBC)
        {
            var p = new DynamicParameters();

            var sql = @"select AGEN_NO, m_discperc || '%' M_DISCPERC, FULLSUM, 
                        DISCSUM,  PAYSUM,  TXFEE,  PAYSUM-TXFEE as PAY 
                        from PH_PO_MON
                        WHERE yyymm = :YearMonth 
                        AND mat_class = :MATCLS
                        AND m_contid = :ContractType
                        AND agen_no in 
                        (
                            select AGEN_NO 
                            from PH_PO_MON
                            WHERE yyymm = :YearMonth 
                            AND mat_class = :MATCLS
                            AND m_contid = :ContractType ";
            if (!string.IsNullOrEmpty(IsTBC) && IsTBC != "2")
            {
                if (IsTBC == "0")
                {
                    sql += @" AND agen_bank = '006'";
                }
                else if (IsTBC == "1")
                {
                    sql += @" AND agen_bank <> '006'";
                }
            }
            sql += " group by yyymm, mat_class, m_contid , agen_no, agen_namec ) ";

            //不為空且不等於不分區(2)
            if (!string.IsNullOrEmpty(IsTBC) && IsTBC != "2")
            {
                if (IsTBC == "0")
                {
                    sql += @" AND agen_bank = '006'";
                }
                else if (IsTBC == "1")
                {
                    sql += @" AND agen_bank <> '006'";
                }
            }
            sql += "  ORDER BY agen_no, m_discperc";
            p.Add(":YearMonth", string.Format("{0}", YearMonth));
            p.Add(":MATCLS", string.Format("{0}", MATCLS));
            p.Add(":ContractType", string.Format("{0}", ContractType));

            return DBWork.Connection.Query<FA0017>(sql, p, DBWork.Transaction);
        }

        /// <summary>
        /// 查詢時先呼叫SP檢核
        /// </summary>
        /// <param name="MATCLS">物料代碼</param>
        /// <param name="YearMonth">月結年月</param>
        /// <param name="USER_ID">使用者ID</param>
        /// <param name="USER_IP">使用者IP</param>
        /// <returns></returns>
        public string SearchCheckProcedure(string MATCLS, string YearMonth, string USER_ID, string USER_IP)
        {
            var p = new OracleDynamicParameters();
            p.Add("i_yyymm", value: YearMonth, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);
            p.Add("i_mat_class", value: MATCLS, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);
            p.Add("i_userid", value: USER_ID, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);
            p.Add("i_ip", value: USER_IP, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);

            p.Add("ret_code", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 200);

            DBWork.Connection.Query("PO_MONCLOSE", p, commandType: CommandType.StoredProcedure);
            string RetCode = p.Get<OracleString>("ret_code").Value;

            return RetCode;
        }

        public string ChkChtDateStr(string yyymm)
        {
            var sql = @"select twn_yyymm(twn_todate(:YYYMM || '01')) from dual ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { YYYMM = yyymm }, DBWork.Transaction);
        }

        //public IEnumerable<COMBO_MODEL> GetMatClassCombo()
        //{
        //    string sql = @"SELECT MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, 
        //                MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM
        //                FROM MI_MATCLASS 
        //                WHERE MAT_CLSID IN ('3','6')";
        //    return DBWork.Connection.Query<COMBO_MODEL>(sql);
        //}
        //public IEnumerable<COMBO_MODEL> GetMatClassCombo02()
        //{
        //    string sql = @"SELECT MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, 
        //                MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM
        //                FROM MI_MATCLASS 
        //                WHERE MAT_CLSID IN ('2')";
        //    return DBWork.Connection.Query<COMBO_MODEL>(sql);
        //}
    }
}