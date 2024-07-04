using System;
using System.Collections.Generic;
using System.Web;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;

namespace WebApp.Repository.B
{
    public class BG0010Repository : JCLib.Mvc.BaseRepository
    {
        public BG0010Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<BG0010MasterMODEL> GetAll(string p0, string[] arr_p1, string p2, string p3, int page_index, int page_size, string sorters)
        {
            DynamicParameters sqlParam = new DynamicParameters();

            string sqlStr = GetSqlstr(p0, arr_p1, p2, p3, out sqlParam);

            sqlParam.Add("OFFSET", (page_index - 1) * page_size);
            sqlParam.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<BG0010MasterMODEL>(GetPagingStatement(sqlStr, sorters), sqlParam, DBWork.Transaction);
        }
        public DataTable GetExcel(string p0, string[] arr_p1, string p2, string p3)
        {
            DynamicParameters sqlParam = new DynamicParameters();

            string sqlStr = GetSqlstr2(p0, arr_p1, p2, p3, out sqlParam);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sqlStr, sqlParam, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public DataTable GetExcel2(string p0, string[] arr_p1, string p2, string p3)
        {
            var p = new DynamicParameters();
            string mat_class = "''";
            string mat_class_sub = "''";
            for (int i = 0; i < arr_p1.Length; i++)
            {
                if (arr_p1[i].Contains("SUB_"))
                {
                    if (i == 0)
                        mat_class_sub = @" '" + arr_p1[i].Replace("SUB_", "") + "'";
                    else
                        mat_class_sub += @",'" + arr_p1[i].Replace("SUB_", "") + "'";
                }
                else
                {
                    if (i == 0)
                        mat_class = @" '" + arr_p1[i] + "'";
                    else
                        mat_class += @",'" + arr_p1[i] + "'";
                }
            }
            string sql = @"select  A.AGEN_NO 廠商代碼,  
                                   A.AGEN_NAMEC 廠商名稱, 
                                   (select UNI_NO from PH_VENDER where AGEN_NO=A.AGEN_NO) as 統一編號,
                                   sum(round(A.PAYMASS) ) 應付總價,  
                                   sum(round(A.DISAMOUNT)) 折讓金額,  sum(round(A.MGTFEE)) 聯標契約優惠,  
                                   sum(round(A.PAYMASS-A.DISAMOUNT-A.EXTRA_DISC_AMOUNT)) as 實付總價
                                   from INVCHK A where 1=1";

            if (p0.Trim() != "")
            {
                sql += " and  A.CHK_YM = :p0";
                p.Add(":p0", string.Format("{0}", p0));
            }

            if (arr_p1.Length > 0)
            {
                sql += " and (A.MAT_CLASS in (" + mat_class + ") or A.MAT_CLASS in (" + mat_class_sub + "))";
            }

            // 廠商代碼起迄
            if (!string.IsNullOrEmpty(p2) || !string.IsNullOrEmpty(p3))
            {
                if (!string.IsNullOrEmpty(p2) && !string.IsNullOrEmpty(p3))
                {

                    sql += " and A.AGEN_NO between :p2 and :p3 ";
                    p.Add(":p2", string.Format("{0}", p2));
                    p.Add(":p3", string.Format("{0}", p3));
                }
                else if (!string.IsNullOrEmpty(p2) && !string.IsNullOrEmpty(p3))
                {
                    sql += " and A.AGEN_NO >= :p2";
                    p.Add(":p2", string.Format("{0}", p2));
                }
                else if (!string.IsNullOrEmpty(p2) && !string.IsNullOrEmpty(p3))
                {
                    sql += " and A.AGEN_NO <= :p3";
                    p.Add(":p3", string.Format("{0}", p3));
                }
            }

            sql += " GROUP BY A.AGEN_NO, A.AGEN_NAMEC ORDER BY A.AGEN_NO";

            //return DBWork.Connection.Query<BG0010>(sql, p, DBWork.Transaction);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }


        public String GetSqlstr(string p0, string[] arr_p1, string p2, string p3, out DynamicParameters pSqlParam)
        {
            pSqlParam = new DynamicParameters();
            string mat_class = "''";
            for (int i = 0; i < arr_p1.Length; i++)
            {
                if (i == 0)
                    mat_class = @" '" + arr_p1[i] + "'";
                else
                    mat_class += @",'" + arr_p1[i] + "'";
            }

            String sqlStr = @"select A.CHK_YM,A.AGEN_NO, A.AGEN_NAMEC, 
                                                         round(A.PAYARMY) as PAYARMY, round(A.BUYARMY) as BUYARMY, round( A.TMPARMY) as TMPARMY,
                                                         (case when A.RCMOD<0 then round(A.PAYMASS+A.RCMOD*(-1))  
                                                                    else round(A.PAYMASS-A.RCMOD) end) as PAYMASS, 
                                                         round(A.BUYMASS) as BUYMASS, round(A.TMPMASS) as TMPMASS,
                                                         round(A.MGTFEE) as MGTFEE, round(A.DISAMOUNT) as DISAMOUNT,
                                                         A.MASSMOD , A.RCMOD, A.MAT_CLASS,
                                                        (select UNI_NO from PH_VENDER where AGEN_NO=A.AGEN_NO) as UNI_NO,
                                                        round(A.MORE_DISC_AMOUNT) as MORE_DISC_AMOUNT
                                               from INVCHK A where 1 = 1";
            if (p0.Trim() != "")
            {
                sqlStr += " and A.CHK_YM=:p0";
                pSqlParam.Add(":p0", string.Format("{0}", p0));
            }
            if (arr_p1.Length > 0)
            {
                sqlStr += " and A.MAT_CLASS in (" + mat_class + ") ";
            }
            // 廠商代碼起迄
            if (p2.Trim() != "" || p3.Trim() != "")
            {
                if (p2 != "" && p3 != "")
                {
                    sqlStr += " and A.AGEN_NO between :p2 and :p3 ";
                    pSqlParam.Add(":p2", string.Format("{0}", p2));
                    pSqlParam.Add(":p3", string.Format("{0}", p3));
                }
                else if (p2 != "" && p3 == "")
                {
                    sqlStr += " and A.AGEN_NO >= :p2";
                    pSqlParam.Add(":p2", string.Format("{0}", p2));
                }
                else if (p2 == "" && p3 != "")
                {
                    sqlStr += " and A.AGEN_NO <= :p3";
                    pSqlParam.Add(":p3", string.Format("{0}", p3));
                }
            }
            sqlStr += " order by A.CHK_YM,A.AGEN_NO";
            return sqlStr.ToString();
        }
        public String GetSqlstr2(string p0, string[] arr_p1, string p2, string p3, out DynamicParameters pSqlParam)
        {
            pSqlParam = new DynamicParameters();
            string mat_class = "''";
            for (int i = 0; i < arr_p1.Length; i++)
            {
                if (i == 0)
                    mat_class = @" '" + arr_p1[i] + "'";
                else
                    mat_class += @",'" + arr_p1[i] + "'";
            }

            String sqlStr = @"select A.CHK_YM as 年月,A.AGEN_NO as 廠商代碼, A.AGEN_NAMEC as 廠商名稱, 
                                                         round(A.PAYARMY) as 軍應付總金額, round(A.BUYARMY) as 軍買斷總金額, round( A.TMPARMY) as 軍寄庫金額,
                                                         (case when A.RCMOD<0 then round(A.PAYMASS+A.RCMOD*(-1))  
                                                                    else round(A.PAYMASS-A.RCMOD) end) as 民應付總金額, 
                                                         round(A.BUYMASS) as 民買斷總金額, round(A.TMPMASS) as 民寄庫金額,
                                                         round(A.MGTFEE) as 聯標契約優惠, round(A.DISAMOUNT) as 折讓金額,
                                                         A.MASSMOD as 民應付調整金額 , A.RCMOD as 聯標契約優惠調整金額, A.MAT_CLASS as 類別,
                                                        (select UNI_NO from PH_VENDER where AGEN_NO=A.AGEN_NO) as 統一編號,
                                                        round(A.MORE_DISC_AMOUNT) as 優惠金額
                                               from INVCHK A where 1 = 1";
            if (p0.Trim() != "")
            {
                sqlStr += " and A.CHK_YM=:p0";
                pSqlParam.Add(":p0", string.Format("{0}", p0));
            }
            if (arr_p1.Length > 0)
            {
                sqlStr += " and A.MAT_CLASS in (" + mat_class + ") ";
            }
            // 廠商代碼起迄
            if (p2.Trim() != "" || p3.Trim() != "")
            {
                if (p2 != "" && p3 != "")
                {
                    sqlStr += " and A.AGEN_NO between :p2 and :p3 ";
                    pSqlParam.Add(":p2", string.Format("{0}", p2));
                    pSqlParam.Add(":p3", string.Format("{0}", p3));
                }
                else if (p2 != "" && p3 == "")
                {
                    sqlStr += " and A.AGEN_NO >= :p2";
                    pSqlParam.Add(":p2", string.Format("{0}", p2));
                }
                else if (p2 == "" && p3 != "")
                {
                    sqlStr += " and A.AGEN_NO <= :p3";
                    pSqlParam.Add(":p3", string.Format("{0}", p3));
                }
            }
            sqlStr += " order by A.CHK_YM,A.AGEN_NO";
            return sqlStr.ToString();
        }

        public IEnumerable<BG0010MasterMODEL> GetSumAll(string p0, string[] arr_p1, string p2, string p3)
        {
            DynamicParameters p = new DynamicParameters();
            string sql = GetSqlstr(p0, arr_p1, p2, p3, out p);
            sql = string.Format(@" select sum(PAYARMY) PAYARMY, sum(BUYARMY) BUYARMY, sum(TMPARMY) TMPARMY, 
                                                                   sum(PAYMASS) PAYMASS, sum(BUYMASS) BUYMASS, sum(TMPMASS) TMPMASS, 
                                                                   sum(MGTFEE) MGTFEE, sum(DISAMOUNT) DISAMOUNT 
                                                       from ({0})", sql);

            return DBWork.Connection.Query<BG0010MasterMODEL>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<BG0010> Report(string p0, string[] arr_p1, string p2, string p3)
        {
            var p = new DynamicParameters();
            string mat_class = "''";
            for (int i = 0; i < arr_p1.Length; i++)
            {
                if (i == 0)
                    mat_class = @" '" + arr_p1[i] + "'";
                else
                    mat_class += @",'" + arr_p1[i] + "'";
            }

            string sql = @"select  A.AGEN_NO F1,  A.AGEN_NAMEC F2, 
                                                     sum(round(A.PAYMASS) ) F3,  
                                                     sum(round(A.DISAMOUNT)) F4,  sum(round(A.MGTFEE)) F5,  
                                                     sum(round(A.PAYMASS-A.DISAMOUNT-A.MGTFEE)) as F6
                                         from INVCHK A where 1=1";

            if (p0.Trim() != "")
            {
                sql += " and  A.CHK_YM = :p0";
                p.Add(":p0", string.Format("{0}", p0));
            }

            if (arr_p1.Length > 0)
            {
                sql += " and A.MAT_CLASS in (" + mat_class + ") ";
            }

            // 廠商代碼起迄
            if (!string.IsNullOrEmpty(p2) || !string.IsNullOrEmpty(p3))
            {
                if (!string.IsNullOrEmpty(p2) && !string.IsNullOrEmpty(p3))
                {

                    sql += " and A.AGEN_NO between :p2 and :p3 ";
                    p.Add(":p2", string.Format("{0}", p2));
                    p.Add(":p3", string.Format("{0}", p3));
                }
                else if (!string.IsNullOrEmpty(p2) && !string.IsNullOrEmpty(p3))
                {
                    sql += " and A.AGEN_NO >= :p2";
                    p.Add(":p2", string.Format("{0}", p2));
                }
                else if (!string.IsNullOrEmpty(p2) && !string.IsNullOrEmpty(p3))
                {
                    sql += " and A.AGEN_NO <= :p3";
                    p.Add(":p3", string.Format("{0}", p3));
                }
            }

            sql += " GROUP BY A.AGEN_NO, A.AGEN_NAMEC ORDER BY A.AGEN_NO";

            return DBWork.Connection.Query<BG0010>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<BG0010> Report_2(int mode, string p0, string[] arr_p1, string p2, string p3)
        {
            var p = new DynamicParameters();
            string mat_class = "''";
            for (int i = 0; i < arr_p1.Length; i++)
            {
                if (i == 0)
                    mat_class = @" '" + arr_p1[i] + "'";
                else
                    mat_class += @",'" + arr_p1[i] + "'";
            }

            string sql = "";
            if (mode == 2)  //實付列印
                sql = @"select A.AGEN_NO F1,  A.AGEN_NAMEC  F2,
                                           (case when sum(A.RCMOD)<0 then sum(round(A.PAYMASS-A.DISAMOUNT-A.MGTFEE+(A.RCMOD*-1)))
                                                      else sum(round(A.PAYMASS-A.DISAMOUNT-A.MGTFEE-A.RCMOD)) end)  F3
                        from INVCHK A where 1=1";
            else if (mode == 3) // 聯標契約優惠列印
                sql = @"select A.AGEN_NO F1,  A.AGEN_NAMEC  F2,
                                           SUM(round(A.MGTFEE)) F3
                        from INVCHK A where 1=1";
            else if (mode == 4) // 應付列印
                sql = @"select A.AGEN_NO F1,  A.AGEN_NAMEC  F2,
                                           SUM(round(A.PAYMASS)) F3
                        from INVCHK A where 1=1";

            if (p0.Trim() != "")
            {
                sql += " and A.CHK_YM = :p0";
                p.Add(":p0", string.Format("{0}", p0));
            }

            if (arr_p1.Length > 0)
            {
                sql += " and A.MAT_CLASS in (" + mat_class + ") ";
            }

            // 廠商代碼起迄
            if (p2.Trim() != "" || p3.Trim() != "")
            {
                if (p2 != "" && p3 != "")
                {

                    sql += " and A.AGEN_NO between :p2 and :p3 ";
                    p.Add(":p2", string.Format("{0}", p2));
                    p.Add(":p3", string.Format("{0}", p3));
                }
                else if (p2 != "" && p3 == "")
                {
                    sql += " and A.AGEN_NO >= :p2";
                    p.Add(":p2", string.Format("{0}", p2));
                }
                else if (p2 == "" && p3 != "")
                {
                    sql += " and A.AGEN_NO <= :p3";
                    p.Add(":p3", string.Format("{0}", p3));
                }
            }
            sql += " group by A.AGEN_NO, A.AGEN_NAMEC";

            if (mode == 3)  // 聯標契約優惠列印
            {
                sql += " having sum(round(A.MGTFEE))>0";
            }
            sql += " order by A.AGEN_NO";
            return DBWork.Connection.Query<BG0010>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetMatClassCombo()
        {
            string sql = @" SELECT MAT_CLASS AS VALUE, '全部' || MAT_CLSNAME AS TEXT, 
                                   MAT_CLASS || ' ' || '全部' || MAT_CLSNAME AS COMBITEM
                            FROM MI_MATCLASS WHERE  mat_clsid in ('1','2')
                            ORDER BY VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<PH_VENDER> GetAgenNoCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.AGEN_NO, A.AGEN_NAMEC, A.EASYNAME
                          FROM PH_VENDER A 
                         WHERE 1=1 ";

            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.AGEN_NO, :AGEN_NO_I), 1000) + NVL(INSTR(A.EASYNAME, :EASYNAME_I), 100) * 10) IDX,"); // 設定權重, 值越小權重最大                             
                p.Add(":AGEN_NO_I", p0);
                p.Add(":EASYNAME_I", p0);

                sql += " AND (A.AGEN_NO LIKE :AGEN_NO";
                p.Add(":AGEN_NO", string.Format("%{0}%", p0));

                sql += " OR A.EASYNAME LIKE :EASYNAME) ";
                p.Add(":EASYNAME", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX, AGEN_NO", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY A.AGEN_NO ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<PH_VENDER>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);

        }

        public int GetYm()
        {
            string sql = @" SELECT MAX(SET_YM) FROM MI_MNSET WHERE SET_STATUS = 'C' ";

            return DBWork.Connection.ExecuteScalar<int>(sql, DBWork.Transaction);
        }
        public string GetHospCode()
        {
            var sql = @"select DATA_VALUE from PARAM_D where GRP_CODE = 'HOSP_INFO' and DATA_NAME = 'HospCode'";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }
        public int deleteData(string chk_ym)
        {
            var sql = @"delete from INVCHK where CHK_YM=:CHK_YM";
            return DBWork.Connection.Execute(sql, new { CHK_YM = chk_ym }, DBWork.Transaction);
        }

        public int ImportData(string chk_ym, string hospCode, string tuser, string userIp)
        {
            string sql_procfee = @"(case when (substr(T.AGEN_BANK_14, 1, 3) = 
                                                                      (select DATA_VALUE from PARAM_D where GRP_CODE = 'HOSP_INFO' and DATA_NAME = 'HospBankCode'))
                                                                    then 0
                                                                     else  (select FEE from PH_BANK_FEE where (PO_AMT + ADDORSUB_AMT - DISC_AMT) >= CASHFROM 
                                                                                   and (PO_AMT + ADDORSUB_AMT - DISC_AMT) <= CASHTO and rownum = 1) 
                        end) as PROCFEE, ";
            // 部分醫院匯費預設為0
            if (hospCode == "805")
                sql_procfee = " 0 as PROCFEE, ";

            string source_sql = "";
            // 台中,花蓮以外不進行[發票彙整->發票轉主計]流程,INVOICE不會有資料,改用可略過這些步驟的做法
            if (hospCode == "803" || hospCode == "805")
            {
                source_sql = @" select D.ACT_YM as CHK_YM,C.AGEN_NO, C.EASYNAME,C.POSTFEE, 
                                            nvl(C.AGEN_BANK_14, C.AGEN_BANK || C.AGEN_SUB) as AGEN_BANK_14,
                                            nvl(sum(B.MORE_DISC_AMOUNT),0) + nvl(sum(B.EXTRA_DISC_AMOUNT), 0) as DISC_AMT,
                                            nvl(sum(B.EXTRA_DISC_AMOUNT), 0) as EXTRA_DISC_AMOUNT,
                                            0 as ADDORSUB_AMT, 
                                            round(sum(B.DELI_QTY * B.PO_PRICE)) as PO_AMT,
                                            (case when E.E_SOURCECODE='P' then round(sum(B.DELI_QTY * B.PO_PRICE)) else 0 end) as BUYMASS,
                                            (case when E.E_SOURCECODE='C' then round(sum(B.DELI_QTY * B.PO_PRICE)) else 0 end) as TMPMASS,
                                            E.MAT_CLASS, nvl(sum(B.MORE_DISC_AMOUNT),0) as MORE_DISC_AMOUNT,
                                            nvl(round(sum(B.MORE_DISC_AMOUNT)),0) as MGTFEE
                                from MM_PO_M A, PH_INVOICE B, PH_VENDER C, INVOICE D, MI_MAST E
                                where A.PO_NO = B.PO_NO and A.AGEN_NO = C.AGEN_NO and B.MMCODE = D.MMCODE and B.INVOICE = D.INVOICE 
                                    and B.INVOICE_DT = D.INVOICE_DT and B.MMCODE=E.MMCODE and D.INVMARK = '1'
                                    and D.ACT_YM = :CHK_YM
                                    and B.DELI_STATUS = 'C'
                                    and B.STATUS <> 'D'
                                group by D.ACT_YM,C.AGEN_NO, C.EASYNAME, C.POSTFEE,C.AGEN_BANK_14,C.AGEN_BANK,C.AGEN_SUB,E.E_SOURCECODE,E.MAT_CLASS ";
            }
            else
            {
                source_sql = @" select :CHK_YM as CHK_YM,C.AGEN_NO, C.EASYNAME,C.POSTFEE, 
                                            nvl(C.AGEN_BANK_14, C.AGEN_BANK || C.AGEN_SUB) as AGEN_BANK_14,
                                            nvl(sum(B.MORE_DISC_AMOUNT),0) + nvl(sum(B.EXTRA_DISC_AMOUNT), 0) as DISC_AMT,
                                            nvl(sum(B.EXTRA_DISC_AMOUNT), 0) as EXTRA_DISC_AMOUNT,
                                            0 as ADDORSUB_AMT, 
                                            round(sum(B.DELI_QTY * B.PO_PRICE)) as PO_AMT,
                                            (case when E.E_SOURCECODE='P' then round(sum(B.DELI_QTY * B.PO_PRICE)) else 0 end) as BUYMASS,
                                            (case when E.E_SOURCECODE='C' then round(sum(B.DELI_QTY * B.PO_PRICE)) else 0 end) as TMPMASS,
                                            E.MAT_CLASS, nvl(sum(B.MORE_DISC_AMOUNT),0) as MORE_DISC_AMOUNT,
                                            nvl(round(sum(B.MORE_DISC_AMOUNT)),0) as MGTFEE
                                from MM_PO_M A, PH_INVOICE B, PH_VENDER C, MI_MAST E
                                where A.PO_NO = B.PO_NO and A.AGEN_NO = C.AGEN_NO and B.MMCODE=E.MMCODE
                                    and B.INVOICE_DT >= nvl((select SET_BTIME from MI_MNSET where SET_YM=:CHK_YM), B.INVOICE_DT)
                                    and B.INVOICE_DT <= nvl((select SET_ETIME from MI_MNSET where SET_YM=:CHK_YM), B.INVOICE_DT)
                                    and (select count(*) from MI_MNSET where SET_YM=:CHK_YM) > 0
                                    and B.DELI_STATUS = 'C'
                                    and B.STATUS <> 'D'
                                group by C.AGEN_NO, C.EASYNAME, C.POSTFEE,C.AGEN_BANK_14,C.AGEN_BANK,C.AGEN_SUB,E.E_SOURCECODE,E.MAT_CLASS ";
            }
            
            string sql = @"insert into INVCHK(CHK_YM,AGEN_NO,CHKSTARTDATE,CHKENDDATE,AGEN_NAMEC,PROCFEE,POSTFEE,
                                                            PAYARMY,BUYARMY,TMPARMY,DISC_AMT,EXTRA_DISC_AMOUNT,
                                                            PAYMASS,BUYMASS,TMPMASS,MAT_CLASS,MORE_DISC_AMOUNT,
                                                            MGTFEE, DISAMOUNT, 
                                                            CREATE_TIME,CREATE_USER,UPDATE_TIME,UPDATE_USER,UPDATE_IP)
                            select T.CHK_YM, T.AGEN_NO,
                                    (select SET_BTIME from MI_MNSET where SET_YM=T.CHK_YM) as CHKSTARTDATE,
                                    (select SET_CTIME from MI_MNSET where SET_YM=T.CHK_YM) as CHKENDDATE,
                                        T.EASYNAME," + sql_procfee + @"T.POSTFEE,0 as PAYARMY,0 as BUYARMY, 0 as TMPARMY,
                                        T.DISC_AMT,T.EXTRA_DISC_AMOUNT,T.PO_AMT as PAYMASS,T.BUYMASS,T.TMPMASS,T.MAT_CLASS,T.MORE_DISC_AMOUNT,
                                        T.MGTFEE, T.EXTRA_DISC_AMOUNT as DISAMOUNT,
                                    sysdate as CREATE_TIME, :TUSER as CREATE_USER, sysdate as UPDATE_TIME, :TUSER as UPDATE_USER, :USERIP as UPDATE_IP
                        from (" + source_sql + @" ) T ";

            return DBWork.Connection.Execute(sql, new { CHK_YM = chk_ym, TUSER = tuser, USERIP = userIp }, DBWork.Transaction);
        }
        public int Update(BG0010 bg0010)
        {
            var sql = @"update INVCHK set 
                                         RCMOD = nvl(trim(:RCMOD), '0'), 
                                         UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                  where CHK_YM=:CHK_YM and AGEN_NO=:AGEN_NO and MAT_CLASS=:MAT_CLASS ";
            return DBWork.Connection.Execute(sql, bg0010, DBWork.Transaction);
        }
        public string GetHospFullName()
        {
            string sql = @" SELECT data_value FROM PARAM_D WHERE grp_code = 'HOSP_INFO' AND data_name = 'HospFullName' ";
            return DBWork.Connection.ExecuteScalar<string>(sql, DBWork.Transaction);
        }

        public IEnumerable<string> GetMiMnset(string ym)
        {
            var p = new DynamicParameters();

            string sql = @" select ('自'||TWN_DATE(SET_BTIME)||'至'||TWN_DATE(SET_ETIME)||'止') as SET_TIME
                                            from MI_MNSET where SET_YM=:SET_YM";

            p.Add(":SET_YM", ym);
            return DBWork.Connection.Query<string>(sql, p, DBWork.Transaction);
        }
        public bool ChkSetYM(string set_ym)
        {
            string sql = @" select 1 from MI_MNSET where SET_YM=:set_ym and SET_STATUS='C'";

            return DBWork.Connection.ExecuteScalar(sql, new { set_ym }, DBWork.Transaction) == null;
        }

        public class BG0010MasterMODEL : JCLib.Mvc.BaseModel
        {
            public string rnm { get; set; }
            public string CHK_YM { get; set; }
            public string AGEN_NO { get; set; }
            public string AGEN_NAMEC { get; set; }
            public string PAYARMY { get; set; }
            public string BUYARMY { get; set; }
            public string TMPARMY { get; set; }
            public string PAYMASS { get; set; }
            public string BUYMASS { get; set; }
            public string TMPMASS { get; set; }
            public string MGTFEE { get; set; }
            public string DISAMOUNT { get; set; }
            public string MASSMOD { get; set; }
            public string RCMOD { get; set; }
            public string MAT_CLASS { get; set; }
            public string UNI_NO { get; set; }
            public string MORE_DISC_AMOUNT { get; set; }
        }
    }
}