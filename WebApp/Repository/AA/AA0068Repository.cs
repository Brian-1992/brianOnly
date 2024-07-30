using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.AA
{
    // 資料自1090801起
    public class AA0068Repository : JCLib.Mvc.BaseRepository
    {
        public AA0068Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AA0068> GetAll(string WH_NO, string YYYMMDD_B, string YYYMMDD_E, string CONTRACNO, string AGEN_NO, int page_index, int page_size, string sorters, bool isHospCode0)
        {
            var p = new DynamicParameters();
            var sql = "";
            sql += @" select a.agen_no||'  '||agen_namec as agen_no, a.mmcode, MMNAME_E, twn_date(accountdate) accountdate,  
                        a.M_PURUN , a.PO_PRICE,  DELI_QTY, a.DISC_CPRICE, a.CONTRACNO,
                        (round(a.PO_PRICE * DELI_QTY) - round(a.DISC_CPRICE * DELI_QTY)) DISC_AMT, 
                        (case when a.ISWILLING='是' and a.po_qty >= a.discount_qty
                               then (round(a.DISC_CPRICE * DELI_QTY) - round(a.DISC_COST_UPRICE * DELI_QTY))
                               else 0
                               end) WILL_DISC_AMT,  --單次訂購達優惠數量折讓金額
                        round(a.PO_PRICE * DELI_QTY) PO_AMT,   
                        round(a.DISC_CPRICE * DELI_QTY) PAY_AMT,
                        (case when a.TRANSKIND='111' then 'A進貨'  when a.TRANSKIND='120' then 'D退貨' else '' end) as flag,
                        (case when a.CONTRACNO='0Y' then '*零購*' when a.CONTRACNO='0N' then '*零購*' when a.CONTRACNO in ('1','2','3','01','02','03') then '*合約*' else '' end)  memo
                      from  MM_PO_INREC a, PH_VENDER p, MI_MAST y   
                         Where wh_no=:wh_no 
                               and a.mmcode=y.mmcode  
                               and a.agen_no=p.agen_no
                               and DELI_QTY <> 0
                               and twn_date(accountdate) >= :YYYMMDD_B
                               and twn_date(accountdate) <= :YYYMMDD_E ";

            if (AGEN_NO != "" && AGEN_NO != null)  //排除廠商
            {
                string[] tmpAgen_no = AGEN_NO.Split(',');
                sql += " and a.AGEN_NO not in :AGEN_NO ";
                p.Add(":AGEN_NO", tmpAgen_no);
            }

            if (isHospCode0)
            {
                switch (CONTRACNO)
                {
                    case "零購":
                        sql += "and a.CONTRACNO in ('0Y', '0N', 'X')";
                        break;
                    case "合約":
                        sql += "and a.CONTRACNO in ('1','2','3','01','02','03')";
                        break;
                }
            }
            else {
                switch (CONTRACNO) //M_CONTID
                {
                    case "非合約":
                        sql += "and (select m_contid from MM_PO_M where po_no=a.po_no) ='2'";
                        break;
                    case "合約":
                        sql += "and (select m_contid from MM_PO_M where po_no=a.po_no) ='0'";
                        break;
                }
            }
            sql += " order by a.agen_no, a.accountdate, MMNAME_E, a.mmcode";
            p.Add(":wh_no", WH_NO);
            p.Add(":YYYMMDD_B", YYYMMDD_B);
            p.Add(":YYYMMDD_E", YYYMMDD_E);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0068>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }



        public IEnumerable<ComboItemModel> GetMATCombo()
        {
            string sql = @"Select MAT_CLASS||' '||MAT_CLSNAME TEXT, MAT_CLASS VALUE from MI_MATCLASS
                            ORDER BY VALUE";


            return DBWork.Connection.Query<ComboItemModel>(sql, new { userID = DBWork.ProcUser }, DBWork.Transaction);
        }
        public DataTable GetExcel(string WH_NO, string YYYMMDD_B, string YYYMMDD_E, string CONTRACNO, string AGEN_NO, bool isHospCode0)
        {
            var p = new DynamicParameters();
            var sql = "";
            sql += @"select twn_date(accountdate) as 進貨日期, a.agen_no||'  '||agen_namec as 廠商代碼, a.mmcode as 院內碼,
                            (case when y.e_restrictcode='0' then '它管' when y.e_restrictcode='1' then '管一' when y.e_restrictcode='2' then '管二' 
                                  when y.e_restrictcode='3' then '管三' when y.e_restrictcode='4' then '管四' end ) as 管制藥, MMNAME_E as 藥品名稱, 
                        a.M_PURUN as 單位, a.PO_PRICE as 發票單價,  
                        (case when a.TRANSKIND='111' then 'A進貨'  when a.TRANSKIND='120' then 'D退貨' else '' end) as 類別,
                        DELI_QTY as 進貨量, 
                        round(a.PO_PRICE * DELI_QTY) 發票金額,    
                        (case when a.CONTRACNO='0Y' then '*零購*' when a.CONTRACNO='0N' then '*零購*' when a.CONTRACNO in ('1','2','3','01','02','03') then '*合約*' else '' end) as 說明,  
                        (round(a.PO_PRICE * DELI_QTY) - round(a.disc_cprice * DELI_QTY)) as 折讓金額, 
                        (case when a.ISWILLING='是' and a.po_qty >= a.discount_qty
                            then (round(a.DISC_CPRICE * DELI_QTY) - round(a.DISC_COST_UPRICE * DELI_QTY))
                            else 0
                            end) as 單次訂購達優惠數量折讓金額,
                        a.DISC_CPRICE as 優惠價,
                        round(a.DISC_CPRICE * DELI_QTY) 優惠金額
                        
                      from  MM_PO_INREC a, PH_VENDER p, MI_MAST y   
                         Where wh_no=:wh_no 
                               and a.mmcode=y.mmcode  
                               and a.agen_no=p.agen_no
                               and DELI_QTY <> 0
                               and twn_date(accountdate) >= :YYYMMDD_B
                               and twn_date(accountdate) <= :YYYMMDD_E ";

            if (AGEN_NO != "" && AGEN_NO != null)  //排除廠商
            {
                string[] tmpAgen_no = AGEN_NO.Split(',');
                sql += " and a.AGEN_NO not in :AGEN_NO ";
                p.Add(":AGEN_NO", tmpAgen_no);
            }

            if (isHospCode0)
            {
                switch (CONTRACNO)
                {
                    case "零購":
                        sql += "and a.CONTRACNO in ('0Y', '0N', 'X')";
                        break;
                    case "合約":
                        sql += "and a.CONTRACNO in ('1','2','3','01','02','03')";
                        break;
                }
            }
            else
            {
                switch (CONTRACNO) //M_CONTID
                {
                    case "非合約":
                        sql += "and (select m_contid from MM_PO_M where po_no=a.po_no) ='2'";
                        break;
                    case "合約":
                        sql += "and (select m_contid from MM_PO_M where po_no=a.po_no) ='0'";
                        break;
                }
            }

            sql += " order by a.agen_no, a.accountdate, MMNAME_E, a.mmcode";
            p.Add(":wh_no", WH_NO);
            p.Add(":YYYMMDD_B", YYYMMDD_B);
            p.Add(":YYYMMDD_E", YYYMMDD_E);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }


        public DataTable Report(string WH_NO, string YYYMMDD_B, string YYYMMDD_E, string CONTRACNO, string AGEN_NO, bool isHospCode0)
        {
            var p = new DynamicParameters();

            var sql = @"select a.agen_no, a.mmcode||' | '||substr(MMNAME_E,1,30) MMNAME_E,   
                        a.M_PURUN , a.PO_PRICE, DELI_QTY as DELI_QTY, 
                        (round(a.PO_PRICE * DELI_QTY) - round(a.disc_cprice * DELI_QTY)) DISC_AMT, 
                        (case when a.ISWILLING='是' and a.po_qty >= a.discount_qty
                             then (round(a.DISC_CPRICE * DELI_QTY) - round(a.DISC_COST_UPRICE * DELI_QTY))
                             else 0
                             end) WILL_DISC_AMT,  --單次訂購達優惠數量折讓金額
                        round(a.PO_PRICE * DELI_QTY) PO_AMT,    
                        (case when a.TRANSKIND='111' then 'A進貨'  when a.TRANSKIND='120' then 'D退貨' else '' end) as flag,
                        (case when a.CONTRACNO='0Y' then '*零購*' when a.CONTRACNO='0N' then '*零購*' when a.CONTRACNO in ('1','2','3','01','02','03') then '*合約*' else '' end)  memo,
                        (case when a.TransKind='111' then round(a.PO_PRICE * DELI_QTY)  else  round(a.PO_PRICE * DELI_QTY)*(-1) end) RAW_PRICE,
                        (case when a.TransKind='111' then round(a.PO_PRICE * DELI_QTY)-round(a.disc_cprice * DELI_QTY)  else  (round(a.PO_PRICE * DELI_QTY)-round(a.disc_cprice * DELI_QTY))*(-1) end) RAW_DISC
                      from  MM_PO_INREC a, PH_VENDER p, MI_MAST y  
                         Where wh_no=:wh_no 
                               and a.agen_no=p.agen_no
							   and a.mmcode=y.mmcode  
                               and DELI_QTY <> 0
                               and twn_date(accountdate) >= :YYYMMDD_B
                               and twn_date(accountdate) <= :YYYMMDD_E ";

            if (AGEN_NO != "" && AGEN_NO != null)  //排除廠商
            {
                string[] tmpAgen_no = AGEN_NO.Split(',');
                sql += " and a.AGEN_NO not in :AGEN_NO ";
                p.Add(":AGEN_NO", tmpAgen_no);
            }

            if (isHospCode0)
            {
                switch (CONTRACNO)
                {
                    case "零購":
                        sql += "and a.CONTRACNO in ('0Y', '0N', 'X')";
                        break;
                    case "合約":
                        sql += "and a.CONTRACNO in ('1','2','3','01','02','03')";
                        break;
                }
            }
            else
            {
                switch (CONTRACNO) //M_CONTID
                {
                    case "非合約":
                        sql += "and (select m_contid from MM_PO_M where po_no=a.po_no) ='2'";
                        break;
                    case "合約":
                        sql += "and (select m_contid from MM_PO_M where po_no=a.po_no) ='0'";
                        break;
                }
            }
            sql += " order by a.agen_no, MMNAME_E, a.mmcode, flag";
            p.Add(":wh_no", WH_NO);
            p.Add(":YYYMMDD_B", YYYMMDD_B);
            p.Add(":YYYMMDD_E", YYYMMDD_E);
            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT FROM MI_MAST A WHERE 1=1 ";


            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(A.MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(A.MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("%{0}%", p0));

                sql += " OR A.MMNAME_E LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR A.MMNAME_C LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX, MMCODE", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY A.MMCODE ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<ComboItemModel> GetWH_NoCombo()
        {
            var p = new DynamicParameters();

            var sql = @"SELECT WH_NO VALUE, WH_NO ||' '|| WH_NAME TEXT FROM MI_WHMAST  WHERE  WH_GRADE in ('1','5') AND WH_KIND ='0' 
                        ORDER BY WH_NO";


            return DBWork.Connection.Query<ComboItemModel>(sql, p, DBWork.Transaction);

        }

        public IEnumerable<ComboItemModel> GetAgen_NoCombo()
        {
            var p = new DynamicParameters();

            var sql = @" SELECT AGEN_NO VALUE, AGEN_NO ||'_'|| TRIM(REPLACE(AGEN_NAMEC, '　', ' ')) TEXT FROM PH_VENDER WHERE REC_STATUS <>'X' order by AGEN_NO ";


            return DBWork.Connection.Query<ComboItemModel>(sql, p, DBWork.Transaction);

        }
        public int UpdatePrice(string WH_NO, string YYYMMDD_B, string YYYMMDD_E, string UPDATE_USER, string UPDATE_IP)
        {
            var p = new DynamicParameters();
            var sql = @" update MM_PO_INREC a
                           set PO_PRICE =(select CONT_PRICE from V_MMCODE_PRICE where mmcode=a.mmcode and  begindate <= twn_date(accountdate) and twn_date(accountdate) <= enddate and rownum=1 ), 
                              DISC_CPRICE=(select DISC_CPRICE from V_MMCODE_PRICE where mmcode=a.mmcode and  begindate <= twn_date(accountdate) and twn_date(accountdate) <= enddate and rownum=1 ),  
                              UPDATE_TIME =SYSDATE, UPDATE_USER=:UPDATE_USER, UPDATE_IP=:UPDATE_IP,
                              PO_PRICE_OLD=(case when PO_PRICE_OLD is null then PO_PRICE else PO_PRICE_OLD end),
                              DISC_CPRICE_OLD=(case when DISC_CPRICE_OLD is null then DISC_CPRICE else DISC_CPRICE_OLD end) 
                        where wh_no=:WH_NO 
                              and twn_date(accountdate) >= :YYYMMDD_B
                              and twn_date(accountdate) <= :YYYMMDD_E 
                       ";
            p.Add(":WH_NO", WH_NO);
            p.Add(":YYYMMDD_B", YYYMMDD_B);
            p.Add(":YYYMMDD_E", YYYMMDD_E);
            p.Add(":UPDATE_USER", UPDATE_USER);
            p.Add(":UPDATE_IP", UPDATE_IP);
            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }

        public string GetHospCode()
        {
            string sql = @"
                select data_value from PARAM_D where grp_code='HOSP_INFO' and data_name='HospCode'
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }
    }
}