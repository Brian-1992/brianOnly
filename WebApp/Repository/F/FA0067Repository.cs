using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.F
{
    public class FA0067Repository : JCLib.Mvc.BaseRepository
    {
        public FA0067Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        private string GetConditionSql(string MAT_CLASS, string YYYMM_B, string YYYMM_E, string P3, string P4, string MMCODE, string ISCHK)
        {
            string sql = string.Empty;


            if (P3 != "1")
            {
                if (P3 == "2") // 軍
                {
                    sql += @"    and A.wh_no in (select whno_1x('0') as wh_no from dual 
                                    union select whno_1x('1') as wh_no from dual) ";
                }
                else if (P3 == "3") // 民
                {
                    sql += @"   and A.wh_no in (select whno_mm1 as wh_no from dual 
                                   union select whno_me1 as wh_no from dual)  ";
                }
                else if (P3 == "4") // 醫院軍
                {
                    sql += @" and A.WH_NO in (select WH_NO from MI_WHMAST where WH_GRADE='M') ";
                }
                else if (P3 == "5") // 醫院民
                {
                    sql += @"  and A.WH_NO in (select WH_NO from MI_WHMAST 
                                where WH_KIND in('E','C') and WH_GRADE='1') ";
                }
                else if (P3 == "6") // 學院軍
                {
                    sql += @" and A.WH_NO in (select WH_NO from MI_WHMAST where WH_GRADE='S') ";
                }
            }
            if (P4 == "1" || P4 == "0")
            {
                sql += @"  and b.m_storeid = :P4 ";

            }
            if (MMCODE != "")
            {
                sql += " AND a.mmcode  = :MMCODE ";
            }
            return sql;
        }

        private string GetQuerySql(string MAT_CLASS, string YYYMM_B, string YYYMM_E, string P3, string P4, string MMCODE, string ISCHK)
        {

            string mat_class_sql = string.Empty;
            if (MAT_CLASS != "" && MAT_CLASS != null)
            {
                string strInPar = "";
                string[] matclassSplit = MAT_CLASS.Split(',');
                for (int i = 0; i < matclassSplit.Length; i++)
                {
                    if (!strInPar.Equals(""))
                        strInPar += ",";

                    strInPar += ":p0_" + i;

                    if (strInPar.Equals(""))
                        strInPar = "''";
                }

                mat_class_sql += " AND a.MAT_CLASS in (" + strInPar + ")";
            }

            string sql = string.Format(@"
                (   select mat_class,
                           a.mmcode,
                           case (select WH_KIND||WH_GRADE from MI_WHMAST
                                  where WH_NO=a.WH_NO)
                                when '01' then '2 民'
                                when '05' then '1 軍'
                                when '11' then '2 民'
                                when '15' then '1 軍'
                                when 'E1' then '2 醫院民'
                                when 'EM' then '1 醫院軍'
                                when 'ES' then '3 學院軍'
                                when 'C1' then '2 醫院民'
                                when 'CM' then '1 醫院軍'
                                when 'CS' then '3 學院軍' 
                            end as miltype,
                           (select mmname_c from MI_MAST where mmcode=a.mmcode) as mmname_c,
                           (select mmname_e from MI_MAST where mmcode=a.mmcode) as mmname_e,
                           (select base_unit from MI_MAST where mmcode=a.mmcode) as base_unit,
                           a.data_ym,
                           (case (select M_STOREID from MI_MAST where mmcode=a.mmcode)
                                when '1' then '1 庫備'
                                when '0' then '0 非庫備'
                                else ''
                             end) as m_storeid,
                           (case (select m_contid from MI_MAST where mmcode=a.mmcode)
                                when '0' then '0 合約品項'
                                when '2' then '2 非合約'
                                when '3' then '3 不能申請(零購)'
                                else ''
                            end) as m_contid,
                           p_inv_qty, 
                           round((case 
                              when wh_grade in ('5', 'M')
                                then p_mil_price
                              else pmn_avgprice
                             end), 4) as pmn_avgprice, 
                           round((case 
                              when wh_grade in ('5', 'M')
                                then (p_inv_qty * p_mil_price)
                              else (p_inv_qty * pmn_avgprice)
                             end), 4) as pmn_amt,
                           in_qty, 
                           round(uprice, 4) as uprice, 
                           round(in_price, 4) as in_price, 
                           round((in_qty * in_price), 4) as in_amt, 
                           out_qty, 
                           round((case 
                              when wh_grade in ('5', 'M')
                                then mil_price
                              else DISC_CPRICE
                             end), 4) as DISC_CPRICE,
                           round((case 
                              when wh_grade in ('5', 'M')
                                then (out_qty * mil_price)
                              else (out_qty*DISC_CPRICE)
                             end), 4) as out_amt, 
                           inv_qty, 
                           round((case 
                              when wh_grade in ('5', 'M')
                                then mil_price
                              else DISC_CPRICE
                             end), 4) as inv_price,
                           round((case 
                              when wh_grade in ('5', 'M')
                                then (inv_qty * mil_price)
                              else (inv_qty * DISC_CPRICE)
                             end), 4) as inv_amt,
                           round((case 
                              when wh_grade in ('5', 'M')
                                then (p_inv_qty * (mil_price-PMN_AVGPRICE))
                              else (p_inv_qty * (DISC_CPRICE-PMN_AVGPRICE))
                             end), 4) as d_amt,
                           a.WH_NO, 
                           a.INVENTORYQTY, 
                           round((case 
                              when wh_grade in ('5', 'M')
                                then (inventoryqty * mil_price)
                              else (inventoryqty * DISC_CPRICE)
                             end), 4) as INVENTORYQTY_amt
                        from 
                          (  (SELECT A.DATA_YM, 
                                     A.WH_NO, 
                                     A.MMCODE,  
                                     NVL((SELECT INV_QTY FROM MI_WINVMON 
                                           WHERE DATA_YM=TWN_PYM(A.DATA_YM) 
                                             AND WH_NO=A.WH_NO AND MMCODE=A.MMCODE),0)
                                      as P_INV_QTY,
                                     (CASE WHEN C.WH_GRADE = '5'
                                            then 0
                                            else 
                                            (CASE WHEN C.WH_KIND='0' THEN
                                                   NVL((SELECT SUMQTY FROM V_INCOSTE_UN_MN WHERE DATA_YM=A.DATA_YM AND MMCODE=A.MMCODE),0)
                                                ELSE
                                                NVL((SELECT SUMQTY FROM V_INCOST_UN_MN WHERE DATA_YM=A.DATA_YM AND MMCODE=A.MMCODE),0)
                                             END)
                                      END) IN_QTY,
                                     APL_OUTQTY OUT_QTY,
                                     A.INV_QTY,
                                     B.PMN_AVGPRICE, 
                                     (CASE WHEN C.WH_KIND='0' THEN
                                       NVL((SELECT in_price FROM V_INCOSTE_UN_MN WHERE DATA_YM=A.DATA_YM AND MMCODE=A.MMCODE),0)
                                      ELSE
                                       NVL((SELECT in_price FROM V_INCOST_UN_MN WHERE DATA_YM=A.DATA_YM AND MMCODE=A.MMCODE),0)
                                      END) IN_PRICE,
                                     B.DISC_CPRICE,
                                     B.UPRICE, 
                                     B.MIL_PRICE, 
                                     (select mat_class from MI_MAST 
                                       where mmcode= A.MMCODE) as mat_class, INVENTORYQTY,
                                     (select wh_grade from MI_WHMAST where wh_no = a.wh_no) as wh_grade,
                                     (select wh_kind from MI_WHMAST where wh_no = a.wh_no) as wh_kind,
                                     NVL((SELECT mil_price FROM MI_WHCOST 
                                           WHERE DATA_YM=TWN_PYM(A.DATA_YM) 
                                             AND MMCODE=A.MMCODE),0)
                                      as p_mil_price
                                FROM MI_WINVMON A, MI_WHCOST B, MI_WHMAST c
                               WHERE A.DATA_YM=B.DATA_YM(+) 
                                 AND A.MMCODE=B.MMCODE(+)
                                 AND A.WH_NO in(select WH_NO from MI_WHMAST
                                               where (WH_KIND='1' and
                                                      WH_GRADE in('1','5'))  
                                                  or (WH_KIND='0' and
                                                      WH_GRADE in('1','5'))
                                               )
                                 and A.DATA_YM >=:YYYMM_B
                                 and A.DATA_YM <=:YYYMM_E
                                 {0}
                                 and c.wh_no = a.wh_no
                             )
                             union
                             (SELECT A.DATA_YM, 
                                     A.WH_NO, 
                                     A.MMCODE,  
                                     NVL((SELECT INV_QTY FROM MI_WINVMON 
                                           WHERE DATA_YM=TWN_PYM(A.DATA_YM) 
                                             AND WH_NO=A.WH_NO 
                                             AND MMCODE=A.MMCODE),0)
                                     as P_INV_QTY,
                                     A.APL_INQTY IN_QTY,
                                     APL_OUTQTY OUT_QTY,
                                     A.INV_QTY,
                                     B.PMN_AVGPRICE,
                                     B.DISC_UPRICE as IN_PRICE,
                                     B.DISC_CPRICE,
                                     B.UPRICE, 
                                     B.DISC_CPRICE as MIL_PRICE,
                                     (select mat_class from MI_MAST 
                                       where mmcode= A.MMCODE) as mat_class, INVENTORYQTY,
                                     (select wh_grade from MI_WHMAST where wh_no = a.wh_no) as wh_grade,
                                     (select wh_kind from MI_WHMAST where wh_no = a.wh_no) as wh_kind,
                                     NVL((select DISC_CPRICE from MI_WHCOST_EC 
                                           where data_ym=twn_pym(a.data_ym) 
                                             and mmcode=A.mmcode
                                             and mc_id = a.wh_grade
                                             ),0) as p_mil_price
                                FROM (select x.*,
                                             case (select WH_GRADE from MI_WHMAST 
                                                    where WH_NO=x.WH_NO)
                                                  when '1' then '2'
                                                  when 'M' then '1'
                                                  when 'S' then '3'
                                             end as WH_GRADE
                                        from MI_WINVMON x
                                       where WH_NO in(select WH_NO from MI_WHMAST
                                                       where (WH_KIND='E' and
                                                              WH_GRADE in('1','M','S')) 
                                                          or (WH_KIND='C' and
                                                              WH_GRADE in('1','M','S')) 
                                                     )
                                         and DATA_YM >=:YYYMM_B
                                         and DATA_YM <=:YYYMM_E
                                     ) A,
                                     MI_WHCOST_EC B
                               WHERE A.DATA_YM=B.DATA_YM(+) 
                                 AND A.MMCODE=B.MMCODE(+)
                                 AND A.WH_GRADE=B.MC_ID(+) 
                                 {0}
                             )
                          ) a
                      where 1=1
                        {1}
                    ) a1  
                    left join
                      (  select b1.data_ym,b1.MMCODE,
                            b2.WH_NO,
                            'chk' as isChk            
                           from
                               (select substr(CHK_NO,length(WH_NO)+1,5) as data_ym,
                                      MMCODE,
                                      (select WH_KIND from MI_WHMAST 
                                        where WH_NO=a.WH_NO) as WH_KIND
                                 from CHK_DETAILTOT a
                                where 1=1
                                  and substr(CHK_NO,length(WH_NO)+1,5) >=:YYYMM_B
                                  and substr(CHK_NO,length(WH_NO)+1,5) <=:YYYMM_E
                                  and WH_NO in(select WH_NO from MI_WHMAST
                                                where (WH_KIND='1' and 
                                                       WH_GRADE in('1','5'))
                                                   or (WH_KIND='E' and 
                                                       WH_GRADE in('1','M','S'))
                                                   or (WH_KIND='C' and 
                                                       WH_GRADE in('1','M','S'))
                                                   or (WH_KIND='0' and 
                                                       WH_GRADE in('1','5'))
                                              )
                               ) b1,
                               (select WH_NO,WH_KIND
                                 from MI_WHMAST
                                where CANCEL_ID='N'
                                  and (   (WH_KIND='1' and WH_GRADE in('1','5'))
                                       or (WH_KIND='E' and WH_GRADE in('1','M','S'))
                                       or (WH_KIND='C' and WH_GRADE in('1','M','S'))
                                       or (WH_KIND='0' and WH_GRADE in('1','5'))
                                      )
                               ) b2
                          where b1.WH_KIND=b2.WH_KIND
                      ) a2
                    on a1.data_ym=a2.data_ym and a1.WH_NO=a2.WH_NO and 
                       a1.mmcode=a2.MMCODE
                    where 1=1
            ", GetConditionSql(MAT_CLASS, YYYMM_B, YYYMM_E, P3, P4, MMCODE, ISCHK)
             , mat_class_sql);
            if (ISCHK == "1")
            {
                sql += @"  and a2.isChk='chk' ";
            }
            else if (ISCHK == "2")
            {
                sql += @"  and a2.isChk is null ";
            }
            return sql;
        }

        public IEnumerable<FA0067> GetAll(string MAT_CLASS, string YYYMM_B, string YYYMM_E, string P3, string P4, string MMCODE, string ISCHK, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = string.Format(@" select a1.*,
                       case a2.isChk
                            when 'chk' then '實盤'
                            else '無盤點'            
                        end as isChk       
                from       
                 {0} ", GetQuerySql(MAT_CLASS, YYYMM_B, YYYMM_E, P3, P4, MMCODE, ISCHK));
            p.Add(":P3", string.Format("{0}", P3));
            p.Add(":YYYMM_B", string.Format("{0}", YYYMM_B));
            p.Add(":YYYMM_E", string.Format("{0}", YYYMM_E));


            if (MAT_CLASS != "" && MAT_CLASS != null)
            {
                string[] matclassSplit = MAT_CLASS.Split(',');
                for (int i = 0; i < matclassSplit.Length; i++)
                {
                    p.Add(":p0_" + i, matclassSplit[i]);
                }
            }
            p.Add(":P4", string.Format("{0}", P4));
            p.Add(":MMCODE", string.Format("{0}", MMCODE));


            return DBWork.PagingQuery<FA0067>(sql, p, DBWork.Transaction);
        }



        public IEnumerable<ComboItemModel> GetMATCombo()
        {
            string sql = @"Select MAT_CLASS||' '||MAT_CLSNAME TEXT, MAT_CLASS VALUE from MI_MATCLASS
                            ORDER BY VALUE";


            return DBWork.Connection.Query<ComboItemModel>(sql, new { userID = DBWork.ProcUser }, DBWork.Transaction);
        }
        public DataTable GetExcel(string MAT_CLASS, string YYYMM_B, string YYYMM_E, string P3, string P4, string MMCODE, string ISCHK)
        {
            var p = new DynamicParameters();

            var sql = string.Format(@" 
                  select a1.MAT_CLASS as 物料類別, a1.mmcode as 院內碼, 
                         a1.miltype as 軍民別, a1.mmname_c as 中文品名,
                         a1.mmname_e as 英文品名, a1.base_unit as 計量單位,
                         a1.data_ym  年月, a1.m_storeid as 是否庫備,
                         a1.m_contid as 是否合約, a1.p_inv_qty 期初數量,
                         a1.pmn_avgprice 期初單價, a1.pmn_amt as 期初金額,
                         a1.in_qty 進貨數量, a1.in_price 進貨單價,
                         a1.in_amt as 進貨金額, a1.out_qty  撥發數量,
                         a1.DISC_CPRICE  撥發單價, a1.out_amt as 撥發金額, 
                         a1.INVENTORYQTY as 盤盈虧數量, a1.DISC_CPRICE as 庫存單價, 
                         a1.INVENTORYQTY_amt as 盤盈虧金額, a1.inv_qty 結存數量, 
                         a1.inv_amt as 結存金額, a1.D_AMT as 差異金額, a1.uprice as 原進貨單價,
                         case a2.isChk
                              when 'chk' then '實盤'
                              else '無盤點'            
                          end as 是否盤點       
                   from       
                        {0} 
                  order by a1.mat_class, a1.mmcode, a1.data_ym,軍民別"
            , GetQuerySql(MAT_CLASS, YYYMM_B, YYYMM_E, P3, P4, MMCODE, ISCHK));

            p.Add(":P3", string.Format("{0}", P3));
            p.Add(":YYYMM_B", string.Format("{0}", YYYMM_B));
            p.Add(":YYYMM_E", string.Format("{0}", YYYMM_E));

            if (MAT_CLASS != "" && MAT_CLASS != null)
            {
                string[] matclassSplit = MAT_CLASS.Split(',');
                for (int i = 0; i < matclassSplit.Length; i++)
                {
                    p.Add(":p0_" + i, matclassSplit[i]);
                }
            }
            p.Add(":P4", string.Format("{0}", P4));
            p.Add(":MMCODE", string.Format("{0}", MMCODE));

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }


        public DataTable Report(string MAT_CLASS, string YYYMM_B, string YYYMM_E, string P3, string P4, string MMCODE, string ISCHK)
        {
            var p = new DynamicParameters();

            var sql = string.Format(@" 
                select a1.*,
                       case a2.isChk
                            when 'chk' then '實盤'
                            else '無盤點'            
                        end as isChk       
                  from       
                   {0}
                 order by a1.mat_class,a1.mmcode,a1.data_ym,miltype ", GetQuerySql(MAT_CLASS, YYYMM_B, YYYMM_E, P3, P4, MMCODE, ISCHK));
            p.Add(":P3", string.Format("{0}", P3));
            p.Add(":YYYMM_B", string.Format("{0}", YYYMM_B));
            p.Add(":YYYMM_E", string.Format("{0}", YYYMM_E));


            if (MAT_CLASS != "" && MAT_CLASS != null)
            {
                string[] matclassSplit = MAT_CLASS.Split(',');
                for (int i = 0; i < matclassSplit.Length; i++)
                {
                    p.Add(":p0_" + i, matclassSplit[i]);
                }
            }
            p.Add(":P4", string.Format("{0}", P4));
            p.Add(":MMCODE", string.Format("{0}", MMCODE));

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
                sql = string.Format(sql, "(NVL(INSTR(UPPER(A.MMCODE), UPPER(:MMCODE_I)), 1000) + NVL(INSTR(UPPER(A.MMNAME_E), UPPER(:MMNAME_E_I)), 100) * 10 + NVL(INSTR(UPPER(A.MMNAME_C), UPPER(:MMNAME_C_I)), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (UPPER(A.MMCODE) LIKE UPPER(:MMCODE) ";
                p.Add(":MMCODE", string.Format("%{0}%", p0));

                sql += " OR UPPER(A.MMNAME_E) LIKE UPPER(:MMNAME_E) ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR UPPER(A.MMNAME_C) LIKE UPPER(:MMNAME_C)) ";
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

        public string GetHospCode()
        {
            string sql = @"
                select data_value from PARAM_D where grp_code='HOSP_INFO' and data_name='HospCode'
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }
    }
}