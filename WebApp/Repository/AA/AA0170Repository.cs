using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models.AA;
using WebApp.Models;
using System.Collections.Generic;


namespace WebApp.Repository.AA
{
    public class AA0170Repository : JCLib.Mvc.BaseRepository
    {
        public AA0170Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        // GET api/<controller>
        public IEnumerable<MI_WHMAST> GetWH_NOComboOne()
        {
            string sql = @"SELECT wh_no, wh_no ||' '|| wh_name as wh_name from MI_WHMAST  ORDER BY wh_no ";
            return DBWork.Connection.Query<MI_WHMAST>(sql, DBWork.Transaction);
        }

        public IEnumerable<ComboItemModel> GetWhnoCombo(string tuser)
        {
            string sql = @"select wh_no as VALUE,wh_no ||' '|| wh_name as text from mi_whmast ";
            return DBWork.Connection.Query<ComboItemModel>(sql, new { TUSER = tuser }, DBWork.Transaction);
        }
        public string MCLSNAME(string mid)
        {
            string rtnmname = "";
            string sql = @"SELECT MAT_CLSNAME  FROM MI_MATCLASS where MAT_CLASS=:mid";


            rtnmname = DBWork.Connection.QueryFirst<string>(sql, new { mid }, DBWork.Transaction);

            return rtnmname;
        }
        public DataTable GetExcel(string mclass, string whno, string chk_ym, string rb1, string rb2)
        {
            var p = new DynamicParameters();

            string sql = string.Format(@"
                select a.chk_wh_no as 庫房代碼,
                       a.wh_name as 庫房名稱,
                       a.max_chk_level_name as 庫房盤點階段,
                       a.final_status_name as 庫房盤點狀態,
                       a.mmcode as 院內碼,
                       a.mmname_c as 中文名稱,
                       a.mmname_e as 英文名稱,
                       a.base_unit as 單位,
                       a.disc_cprice as 優惠合約單價,
                       a.store_qty1 as 初盤電腦量,
                       a.chk_qty1 as 初盤盤點量,
                       a.store_qty2 as 複盤電腦量,
                       a.chk_qty2 as 複盤盤點量,
                       a.store_qty3 as 三盤電腦量,
                       a.chk_qty3 as 三盤盤點量,
                       a.chk_no1 as 初盤單號,
                       a.chk_no2 as 複盤單號,
                       a.chk_no3 as 三盤單號
                  from (
                        {0}
                       ) a
                 order by a.chk_wh_no, a.mmcode
            ", GetMainSql(mclass, whno, chk_ym, rb1, rb2));

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, new { mclass, whno, chk_ym, rb1, rb2 }, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
        public IEnumerable<AA0170> GetAll(string mclass, string whno, string chk_ym, string rb1, string rb2)
        {
            var p = new DynamicParameters();

            string sql = GetMainSql(mclass, whno, chk_ym, rb1, rb2);

            return DBWork.PagingQuery<AA0170>(sql, new { mclass, whno, chk_ym, rb1, rb2 }, DBWork.Transaction);
        }


        public IEnumerable<AA0170> GetReport(string mclass, string whno, string ym, string rb1, string rb2)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT a.wh_no, 
                           a.mmcode,
                           a.MMNAME_C, 
                           a.MMNAME_E,
                           a.wh_name,
                           a.BASE_UNIT, 
                           a.STORE_QTY, 
                           a.chk_qty, 
                           b.use_qty, 
                           (a.STORE_QTY * c.DISC_CPRICE) as store_amount,
                           (a.chk_QTY * c.DISC_CPRICE) as chk_amount 
                      FROM (SELECT A.CHK_NO,
                                   A.CHK_WH_NO wh_no,
                                   A.CHK_TYPE,
                                   B.WH_NAME,
                                   A.CHK_YM,
                                   C.MMCODE,
                                   C.MMNAME_C,
                                   C.MMNAME_E,
                                   C.BASE_UNIT, 
                                   C.STORE_QTY,
                                   C.M_CONTPRICE,
                                   C.M_STOREID,
                                   C. MAT_CLASS,
                                   (CASE WHEN C.STATUS_TOT = '1' THEN C.CHK_QTY1 
                                         WHEN C.STATUS_TOT = '2' THEN C.CHK_QTY2 
                                         WHEN C.STATUS_TOT = '3' THEN C.CHK_QTY3
                                    ELSE 0 END) AS CHK_QTY 
                              FROM CHK_MAST A,  MI_WHMAST B, CHK_DETAILTOT C 
                             WHERE A.CHK_LEVEL ='1' and a.CHK_STATUS = '3'
                               and a.chk_period = 'M'";

            if (mclass != "null" && mclass != "")
            {
                sql += " AND A.CHK_CLASS = :p1 ";
                p.Add(":p1", mclass);
            }


            if (whno != "" && whno != "null")
            {
                sql += " AND A.CHK_WH_NO = :p5 ";
                p.Add(":p5", whno);
            }

            if (ym != "" && ym != "null")
            {
                sql += " AND A.CHK_YM = :p6 ";
                p.Add(":p6", ym);
            }



            sql += @"              AND  A.CHK_WH_NO = B.WH_NO AND   A.CHK_NO = C.CHK_NO ) a, MI_WHINV b, MI_WHCOST c  
                     Where a.wh_no = b.wh_no
                       And a.mmcode = b.mmcode
                       and c.data_ym = cur_setym
                       and c.mmcode = a.mmcode";


            if (mclass == "01")
            {
                if (rb2 == "1")
                {
                    sql += " AND a. CHK_TYPE ='1'";
                }
                if (rb2 == "2")
                {
                    sql += " AND a. CHK_TYPE ='2'";
                }
                if (rb2 == "3")
                {
                    sql += " AND a. CHK_TYPE ='3'";
                }
                if (rb2 == "4")
                {
                    sql += " AND a. CHK_TYPE ='4'";
                }
                if (rb2 == "5")
                {
                    sql += " AND B.USE_qty < 0 ";
                }

            }
            else
            {
                if (rb1 == "1")
                {
                    sql += " AND a. CHK_TYPE ='1'";
                }
                if (rb1 == "0")
                {
                    sql += " AND a. CHK_TYPE ='0'";
                }
                if (rb1 == "5")
                {
                    sql += " AND B.USE_qty < 0 ";
                }
            }

            sql += " order by a.wh_no, a.mmcode ";

            return DBWork.Connection.Query<AA0170>(sql, p, DBWork.Transaction);
        }

        private string GetMainSql(string mclass, string whno, string ym, string rb1, string rb2)
        {
            string condition = GetCondition(mclass, rb1, rb2);

            string sql = string.Format(@"
                -- 非藥局庫房
                with chk_no1_data_134 as (
                  select chk_wh_no, chk_wh_kind, chk_wh_grade, chk_type, chk_period, chk_no1, max(chk_level) as max_chk_level
                    from CHK_MAST a
                   where a.chk_ym = :chk_ym
                     and a.chk_period in ('M', 'P', 'S')
                     and a.chk_wh_kind in ('0', '1')
                     and not(a.chk_wh_kind = '0' and a.chk_wh_grade = '2')
                     and a.chk_status in  ('1','2','3','P', 'C')
                     {0}
                   group by chk_wh_no, chk_wh_kind, chk_wh_grade,chk_type, chk_period, chk_no1
                ), chk_mast_raw_data_134 as (
                  select b.chk_no, b.chk_no1, b.chk_level, b.chk_status
                    from chk_no1_data_134 a, CHK_MAST b
                   where a.chk_no1 = b.chk_no1
                     and b.chk_status in  ('1','2','3','P', 'C')
                )
                , chk_mast_data_134 as (
                  select a.chk_wh_no, a.chk_wh_kind, a.chk_wh_grade, a.chk_type, a.chk_period,  a.max_chk_level, 
                         a.chk_no1,
                         (select chk_status from chk_mast_raw_data_134 where chk_no1 = a.chk_no1 and chk_level = 1) as chk_status1,
                         (select chk_no from chk_mast_raw_data_134 where chk_no1 = a.chk_no1 and chk_level = 2) as chk_no2,
                         (select chk_status from chk_mast_raw_data_134 where chk_no1 = a.chk_no1 and chk_level = 2) as chk_status2,
                         (select chk_no from chk_mast_raw_data_134 where chk_no1 = a.chk_no1 and chk_level = 3) as chk_no3,
                         (select chk_status from chk_mast_raw_data_134 where chk_no1 = a.chk_no1 and chk_level = 3) as chk_status3
                    from chk_no1_data_134 a
                  order by chk_wh_no, chk_type
                )
                , chk_detail_mmcode_134 as (
                  select a.*, b.mmcode
                    from chk_mast_data_134 a, CHK_DETAIL b
                   where a.chk_no1 = b.chk_no
                     and not (a.chk_wh_kind = '0' and a.chk_wh_grade = '2')
                   group by a.chk_wh_no, a.chk_wh_kind, a.chk_wh_grade, a.chk_type, a.chk_period,  a.max_chk_level, --a.chk_nos,
                            a.chk_no1, a.chk_status1, a.chk_no2, a.chk_status2, a.chk_no3, a.chk_status3,
                            a.chk_no1, b.mmcode
                )
                , chk_level1_data_134 as (
                  select a.chk_wh_no, a.chk_wh_kind, a.chk_wh_grade, a.chk_type, a.chk_period,  a.max_chk_level,
                         a.chk_no1, a.chk_status1, a.chk_no2, a.chk_status2, a.chk_no3, a.chk_status3,
                         a.mmcode, sum(b.chk_qty) as chk_qty1,
                         b.store_qtyc as store_qty1 ,
                         '1' as mmcode_level
                    from chk_detail_mmcode_134 a, CHK_DETAIL b
                   where a.chk_no1 = b.chk_no
                     and not (a.chk_wh_kind = '0' and a.chk_wh_grade = '2')
                     and a.mmcode = b.mmcode
                   group by a.chk_wh_no, a.chk_wh_kind, a.chk_wh_grade, a.chk_type, a.chk_period,  a.max_chk_level,
                            a.chk_no1, a.chk_status1, a.chk_no2, a.chk_status2, a.chk_no3, a.chk_status3,
                            a.mmcode, b.store_qtyc   
                )
                , chk_level2_data_134 as (
                  select a.chk_wh_no, a.chk_wh_kind, a.chk_wh_grade, a.chk_type, a.chk_period,  a.max_chk_level,
                         a.chk_no1, a.chk_status1, a.chk_no2, a.chk_status2, a.chk_no3, a.chk_status3,
                         a.chk_qty1, a.store_qty1, 
                         a.mmcode, sum(b.chk_qty) as chk_qty2,
                         b.store_qtyc as store_qty2,
                         (case when b.mmcode is null then a.mmcode_level else '2' end ) as mmcode_level
                    from chk_level1_data_134 a
                    left join CHK_DETAIL b on (a.chk_no2 = b.chk_no and a.mmcode = b.mmcode)
                   where 1 = 1
                   group by a.chk_wh_no, a.chk_wh_kind, a.chk_wh_grade, a.chk_type, a.chk_period,  a.max_chk_level,
                            a.chk_no1, a.chk_status1, a.chk_no2, a.chk_status2, a.chk_no3, a.chk_status3,
                            a.chk_qty1, a.store_qty1,
                            a.mmcode, b.store_qtyc, a.mmcode_level, b.mmcode
                )
                , chk_level3_data_134 as (
                  select a.chk_wh_no, a.chk_wh_kind, a.chk_wh_grade, a.chk_type, a.chk_period,  a.max_chk_level,
                         a.chk_no1, a.chk_status1, a.chk_no2, a.chk_status2, a.chk_no3, a.chk_status3,
                         a.mmcode,
                         a.chk_qty1, a.store_qty1, a.chk_qty2, a.store_qty2, 
                         (case when b.mmcode is null then a.mmcode_level else '3' end ) as mmcode_level,
                          sum(b.chk_qty) as chk_qty3,
                         b.store_qtyc as store_qty3  
                    from chk_level2_data_134 a
                    left join CHK_DETAIL b on (a.chk_no3 = b.chk_no and a.mmcode = b.mmcode) 
                   where 1 = 1
                   group by a.chk_wh_no, a.chk_wh_kind, a.chk_wh_grade, a.chk_type, a.chk_period,  a.max_chk_level,
                            a.chk_no1, a.chk_status1, a.chk_no2, a.chk_status2, a.chk_no3, a.chk_status3,
                            a.chk_qty1, a.store_qty1, a.chk_qty2, a.store_qty2,  a.mmcode_level,
                            a.mmcode, b.store_qtyc, b.mmcode
                ),
                -- 藥局
                chk_no1_data_2 as (
                  select chk_wh_no, chk_wh_kind, chk_wh_grade, chk_type, chk_period, chk_no1, max(chk_level) as max_chk_level
                    from CHK_MAST a
                   where a.chk_ym = :chk_ym
                     and a.chk_period in ('M', 'P', 'S')
                     and a.chk_wh_kind in ('0', '1')
                     and a.chk_wh_kind = '0' and a.chk_wh_grade = '2'
                     and a.chk_status in  ('1','2','3','P', 'C')
                     {0}
                   group by chk_wh_no, chk_wh_kind, chk_wh_grade,chk_type, chk_period, chk_no1
                ), chk_mast_raw_data_2 as (
                  select b.chk_no, b.chk_no1, b.chk_level, b.chk_status
                    from chk_no1_data_2 a, CHK_MAST b
                   where a.chk_no1 = b.chk_no1
                     and b.chk_status in  ('1','2','3','P', 'C')
                )
                , chk_mast_data_2 as (
                  select a.chk_wh_no, a.chk_wh_kind, a.chk_wh_grade, a.chk_type, a.chk_period,  a.max_chk_level, 
                         a.chk_no1,
                         (select chk_status from chk_mast_raw_data_2 where chk_no1 = a.chk_no1 and chk_level = 1) as chk_status1,
                         (select chk_no from chk_mast_raw_data_2 where chk_no1 = a.chk_no1 and chk_level = 2) as chk_no2,
                         (select chk_status from chk_mast_raw_data_2 where chk_no1 = a.chk_no1 and chk_level = 2) as chk_status2,
                         (select chk_no from chk_mast_raw_data_2 where chk_no1 = a.chk_no1 and chk_level = 3) as chk_no3,
                         (select chk_status from chk_mast_raw_data_2 where chk_no1 = a.chk_no1 and chk_level = 3) as chk_status3
                    from chk_no1_data_2 a
                  order by chk_wh_no, chk_type
                )
                , chk_detail_mmcode_2 as (                  
                  select a.*, b.mmcode
                    from chk_mast_data_2 a, CHK_G2_DETAIL b
                   where a.chk_no1 = b.chk_no
                     and a.chk_wh_kind = '0' 
                     and a.chk_wh_grade = '2'
                   group by a.chk_wh_no, a.chk_wh_kind, a.chk_wh_grade, a.chk_type, a.chk_period,  a.max_chk_level, --a.chk_nos,
                            a.chk_no1, a.chk_status1, a.chk_no2, a.chk_status2, a.chk_no3, a.chk_status3,
                            a.chk_no1, b.mmcode

                ), chk_level1_data_2 as (                  
                  select a.chk_wh_no, a.chk_wh_kind, a.chk_wh_grade, a.chk_type, a.chk_period,  a.max_chk_level,
                         a.chk_no1, a.chk_status1, a.chk_no2, a.chk_status2, a.chk_no3, a.chk_status3,
                         a.mmcode, sum(b.chk_qty) as chk_qty1,
                         c.store_qty as store_qty1,
                         '1' as mmcode_level
                    from chk_detail_mmcode_2 a, CHK_G2_DETAIL b, CHK_G2_WHINV c
                   where a.chk_no1 = b.chk_no
                     and a.mmcode = b.mmcode
                     and b.chk_no = c.chk_no
                     and b.mmcode = c.mmcode
                   group by a.chk_wh_no, a.chk_wh_kind, a.chk_wh_grade, a.chk_type, a.chk_period,  a.max_chk_level,
                            a.chk_no1, a.chk_status1, a.chk_no2, a.chk_status2, a.chk_no3, a.chk_status3,
                            a.mmcode, c.store_qty
                ), chk_level2_data_2 as (
                  select a.chk_wh_no, a.chk_wh_kind, a.chk_wh_grade, a.chk_type, a.chk_period,  a.max_chk_level,
                         a.chk_no1, a.chk_status1, a.chk_no2, a.chk_status2, a.chk_no3, a.chk_status3,
                         a.mmcode,
                          a.chk_qty1, a.store_qty1, 
                          sum(b.chk_qty) as chk_qty2,
                         c.store_qty as store_qty2,
                         (case when b.mmcode is null then a.mmcode_level else '2' end ) as mmcode_level
                    from chk_level1_data_2 a
                    left join CHK_G2_DETAIL b on (a.chk_no2 = b.chk_no and a.mmcode = b.mmcode)
                    left join CHK_G2_WHINV c on (b.chk_no = c.chk_no and b.mmcode = c.mmcode)
                   where 1=1
                   group by a.chk_wh_no, a.chk_wh_kind, a.chk_wh_grade, a.chk_type, a.chk_period,  a.max_chk_level,
                            a.chk_no1, a.chk_status1, a.chk_no2, a.chk_status2, a.chk_no3, a.chk_status3,
                            a.chk_qty1, a.store_qty1, 
                            a.mmcode, c.store_qty, a.mmcode_level, b.mmcode
                )
              select a.*,
                     (select data_desc from PARAM_D 
                       where grp_code = 'CHK_MAST' and data_name = 'CHK_STATUS' 
                         and data_value =a.final_status) as final_status_name,
                     (select data_desc from PARAM_D 
                       where grp_code = 'CHK_MAST' and data_name = 'CHK_LEVEL' 
                         and data_value =a.max_chk_level) as max_chk_level_name,
                     b.DISC_CPRICE
                from (
                        select a.*,
                               c.wh_name as wh_name,
                               b.MMNAME_C, b.mmname_e, b.base_unit,
                               (case when chk_status1 = 'P' then chk_status1 
                                     when chk_status3 is not null then chk_status3
                                     when chk_status2 is not null then chk_status2
                                     else chk_status1
                                     end) as final_status
                         from (
                            select a.* from chk_level3_data_134 a
                            union
                            select b.*, null as chk_qty3, null as store_qty3 from chk_level2_data_2 b
                          )   a, MI_MAST b, MI_WHMAST c
                        where a.mmcode = b.mmcode
                          and a.chk_wh_no = c.wh_no
                          {1}
                     ) a, MI_WHCOST b
               where a.mmcode = b.mmcode
                 and b.data_ym = :chk_ym
            ", condition
             , string.IsNullOrEmpty(whno) == false ? " and a.chk_wh_no = :whno " : string.Empty);
            return sql;
        }
        private string GetCondition(string mclass, string rb1, string rb2)
        {
            string sql = string.Empty;
            if (string.IsNullOrEmpty(mclass) == false)
            {
                sql += @" and a.chk_class = :mclass";
            }
            if (mclass == "01" && rb1 != "99")
            {
                sql += string.Format(" AND a. CHK_TYPE ='{0}'", rb1);
            }

            if (mclass != "01" && rb2 != "99")
            {
                sql += string.Format(" AND a. CHK_TYPE ='{0}'", rb2);
            }


            return sql;
        }
    }
}