using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Repository.F
{
    public class FA0041Repository : JCLib.Mvc.BaseRepository
    {
        public FA0041Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        #region 主檔
        private string GetSql(string chkym, string unitClass, string isChk, string chk_type, string chk_status) {
            string sql = string.Format(@"
                       -- 取得所有藥品盤點類別  
                       with chk_types as (      
                            select 'PH1S' as wh_no, '1' as chk_type from dual
                            union
                            select 'PH1S' as wh_no, '2' as chk_type from dual
                            union 
                            select 'ANE1' as wh_no, '5' as chk_type from dual
                            union
                            select 'ANE1' as wh_no, '6' as chk_type from dual
                            union 
                            select wh_no, '3' as chk_type from MI_WHMAST
                             where wh_kind = '0'
                               and cancel_id = 'N'
                            union 
                            select wh_no, 'X' as chk_type from MI_WHMAST
                             where wh_kind = '0' and wh_grade = '2'
                               and cancel_id = 'N'
                            union
                            select wh_no, '4' as chk_type from MI_WHMAST
                             where wh_kind = '0'
                               and cancel_id = 'N'
                            union 
                            select wh_no, '7' as chk_type from MI_WHMAST
                             where wh_kind = '0'
                               and wh_grade > '2'
                               and wh_no not in ('ANE1')
                               and cancel_id = 'N'
                            union 
                            select wh_no, '8' as chk_type from MI_WHMAST
                             where wh_kind = '0'
                               and wh_grade > '2'
                               and wh_no not in ('ANE1')
                               and cancel_id = 'N'
                      ),
                      -- 取得盤點資料
                      chk_datas as (
                            select a.wh_no as chk_wh_no, a.chk_no1 as chk_no, a.chk_level, a.chk_type, a.chk_period, a.chk_ym,
                                   (case when((select chk_status from CHK_MAST where chk_no1 = a.chk_no1 and chk_level = '1' and rownum=1) = 'P')
                                            then 'P'
                                            else (select chk_status from CHK_MAST where chk_no1 = a.chk_no1 and chk_level = a.chk_level and rownum=1)
                                    end) as chk_status
                              from (
                                    select a.wh_no, a.chk_type, b.chk_no1, b.chk_period, max(b.chk_level) as chk_level, b.chk_ym
                                      from chk_types a
                                      left join chk_mast b on (a.wh_no = b.chk_wh_no and 
                                                               a.chk_type = b.chk_type and 
                                                               b.chk_wh_kind = '0' and 
                                                               b.chk_ym like '{0}%')
                                     where 1=1
                                     group by a.wh_no, a.chk_type, b.chk_no1, b.chk_period, b.chk_ym
                                   ) a
                     )
                     select a.inid, a.inid_name, c.chk_wh_no as wh_no, b.wh_name, c.chk_ym, c.chk_no, 
                            (select data_value || ' ' || data_desc from PARAM_D
                              where grp_code = 'CHK_MAST' and data_name = 'CHK_LEVEL' and data_value = c.chk_level) as chk_level,
                            (select data_value || ' ' || data_desc from PARAM_D
                              where grp_code = 'CHK_MAST' and data_name = 'CHK_WH_KIND_0' and data_value = c.chk_type) as chk_type,
                            (select data_value || ' ' || data_desc from PARAM_D
                              where grp_code = 'CHK_MAST' and data_name = 'CHK_STATUS' and data_value = c.chk_status) as chk_status,
                            (select data_value || ' ' || data_desc from PARAM_D
                              where grp_code = 'CHK_MAST' and data_name = 'CHK_PERIOD' and data_value = c.chk_period) as chk_period
                       from UR_INID a, MI_WHMAST b, chk_datas c
                      where a.inid = b.inid
                        and a.inid_flag {1}
                        and b.wh_no = c.chk_wh_no
                        and c.chk_no is {2} null
                        {3}
                        {4}
                      order by c.chk_wh_no, c.chk_type, c.chk_no
            ", chkym
             , unitClass
             , isChk == "Y" ? "not" : string.Empty
             , chk_type != string.Empty ? " and c.chk_type = :chk_type" : string.Empty
             , (isChk == "Y" && chk_status != string.Empty) ? " and c.chk_status = :chk_status" : string.Empty);

            return sql;
        }

        public IEnumerable<FA0041M> GetAll(string chkym, string unitClass, string isChk, string chk_type, string chk_status) {
            string sql = GetSql(chkym, unitClass, isChk, chk_type, chk_status);

            return DBWork.PagingQuery<FA0041M>(sql, new { chk_type = chk_type, chk_status = chk_status }, DBWork.Transaction);
        }

        public DataTable GetExcel(string chkym, string unitClass, string isChk, string chk_type, string chk_status)
        {
            string sql = GetSql(chkym, unitClass, isChk, chk_type, chk_status);

            sql = string.Format(@"select inid as 單位代碼,
                                         inid_name as 單位名稱,
                                         wh_no as 庫房代碼,
                                         wh_name as 庫房名稱,
                                         chk_type as 盤點類別,
                                         chk_period as 盤點期,
                                         chk_no as 盤點單號,
                                         chk_ym as 盤點年月,
                                         chk_level as 最終盤點階段,
                                         chk_status as 盤點單狀態
                                   from (
                                        {0}
                                   )
                                  order by inid, wh_no, chk_type", sql);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, new { chk_type = chk_type, chk_status = chk_status }, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<FA0041M> Print(string chkym, string unitClass, string isChk, string chk_type, string chk_status) {
            string sql = GetSql(chkym, unitClass, isChk, chk_type, chk_status);

            sql = string.Format(@"
                        select * 
                          from (
                                {0}
                               )
                         order by inid, wh_no, chk_type
                  ", sql);

            return DBWork.Connection.Query<FA0041M>(sql, new { chk_type = chk_type, chk_status = chk_status }, DBWork.Transaction);
        }
        #endregion

        #region 已完成盤點明細

        private string GetDetailtotSql(string chkym, string unitClass, string chk_type, string chk_status) {
            string sql = string.Format(@"
                    -- 取得已完成盤點資料
                     with chk_datas as (
                           select a.chk_wh_no, a.chk_no1 as chk_no, a.chk_level, a.chk_type, a.chk_period, a.chk_ym,
                                  (case when((select chk_status from CHK_MAST where chk_no1 = a.chk_no1 and chk_level = '1' and rownum=1) = 'P')
                                           then 'P'
                                           else (select chk_status from CHK_MAST where chk_no1 = a.chk_no1 and chk_level = a.chk_level and rownum=1)
                                   end) as chk_status
                             from (
                                   select b.chk_wh_no, b.chk_type, b.chk_no1, b.chk_period, max(b.chk_level) as chk_level, b.chk_ym
                                     from chk_mast b
                                     where b.chk_wh_kind = '0' 
                                       and b.chk_ym like '{0}%'
                                       {2}
                                       and exists (select 1 from CHK_DETAILTOT where chk_no = b.chk_no1)                                    
                                    group by b.chk_wh_no, b.chk_type, b.chk_no1, b.chk_period, b.chk_ym
                                  ) a
                     )
                     select a.inid, a.inid_name, c.chk_wh_no as wh_no, b.wh_name, c.chk_ym, c.chk_no, 
                            (select data_value || ' ' || data_desc from PARAM_D
                              where grp_code = 'CHK_MAST' and data_name = 'CHK_LEVEL' and data_value = c.chk_level) as chk_level,
                            (select data_value || ' ' || data_desc from PARAM_D
                              where grp_code = 'CHK_MAST' and data_name = 'CHK_WH_KIND_0' and data_value = c.chk_type) as chk_type,
                            (select data_value || ' ' || data_desc from PARAM_D
                              where grp_code = 'CHK_MAST' and data_name = 'CHK_STATUS' and data_value = c.chk_status) as chk_status,
                            (select data_value || ' ' || data_desc from PARAM_D
                              where grp_code = 'CHK_MAST' and data_name = 'CHK_PERIOD' and data_value = c.chk_period) as chk_period,
                              d.mmcode, d.mmname_c, d.mmname_e, d.base_unit, d.store_qty, 
                            (case
                                when d.status_tot = '1' then d.chk_qty1
                                when d.status_tot = '2' then d.chk_qty2
                                when d.status_tot = '3' then d.chk_qty3
                            end) chk_qty,
                            d.gap_t
                       from UR_INID a, MI_WHMAST b, chk_datas c, CHK_DETAILTOT d
                      where a.inid = b.inid
                        and a.inid_flag {1}
                        and b.wh_no = c.chk_wh_no
                        and c.chk_no = d.chk_no       
                        and c.chk_status in ('3', 'P')
                        {3}
                      order by c.chk_wh_no, c.chk_type, c.chk_no
            "
            , chkym
            , unitClass
            , chk_type == string.Empty ? string.Empty : " and b.chk_type = :chk_type "
            , chk_status == string.Empty ? string.Empty : " and c.chk_status = :chk_status ");

            return sql;
        }

        public IEnumerable<FA0041D> GetDoneDatas(string chkym, string unitClass, string chk_type, string chk_status, string wh_no, string mmcode, string chk_no) {
            string sql = GetDetailtotSql(chkym, unitClass, chk_type, chk_status);

            sql = string.Format(@"
                        select * 
                          from (
                                {0}
                               ) a
                         where 1=1
                  ", sql);
            if (wh_no != string.Empty)
            {
                sql += " and a.wh_no = :wh_no";
            }
            if (mmcode != string.Empty) {
                sql += " and a.mmcode = :mmcode";
            }
            if (chk_no != string.Empty)
            {
                sql += " and a.chk_no = :chk_no";
            }

            return DBWork.PagingQuery<FA0041D>(sql, new { chk_type = chk_type, chk_status = chk_status, mmcode = mmcode, wh_no = wh_no, chk_no = chk_no }, DBWork.Transaction);
        }

        public DataTable GetDoneExcel(string chkym, string unitClass, string chk_type, string chk_status, string wh_no, string mmcode, string chk_no) {
            string sql = GetDetailtotSql(chkym, unitClass, chk_type, chk_status);
            sql = string.Format(@"
                        select inid as 單位代碼,
                               inid_name as 單位名稱,
                               wh_no as 庫房代碼,
                               wh_name as 庫房名稱,
                               chk_type as 盤點類別,
                               chk_period as 盤點期,
                               chk_no as 盤點單號,
                               chk_ym as 盤點年月,
                               chk_level as 最終盤點階段,
                               chk_status as 盤點單狀態,
                               mmcode as 院內碼,
                               mmname_c as 中文品名,
                               mmname_e as 英文品名,
                               base_unit as 計量單位,
                               store_qty as 電腦量,
                               chk_qty as 盤點量,
                               gap_t as 差異量
                         from (
                                {0}
                              ) a
                        where 1=1
                  ", sql);
            if (wh_no != string.Empty)
            {
                sql += " and a.wh_no = :wh_no";
            }
            if (mmcode != string.Empty)
            {
                sql += " and a.mmcode = :mmcode";
            }
            if (chk_no != string.Empty)
            {
                sql += " and a.chk_no = :chk_no";
            }
            sql += "   order by inid, wh_no, chk_type, mmcode";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, new { chk_type = chk_type, chk_status = chk_status, mmcode = mmcode, wh_no = wh_no, chk_no = chk_no }, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
        #endregion

        #region combo
        public string GetUnitCount(string flag)
        {
            string sql = @"select count(*) from UR_INID where INID_FLAG = :FLAG";
            return DBWork.Connection.QueryFirst<string>(sql, new { FLAG = flag }, DBWork.Transaction);

        }

        public IEnumerable<COMBO_MODEL> GetChkType() {
            string sql = @"select data_value as value,
                                  data_value || ' ' || data_desc as text
                             from PARAM_D
                            where grp_code = 'CHK_MAST'
                              and data_name = 'CHK_WH_KIND_0'
                            order by data_value";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, DBWork.Transaction);
        }

        public IEnumerable<ChkStatus> GetChkStatus()
        {
            string sql = @"select data_value as value,
                                  data_value || ' ' || data_desc as text,
                                  (case when (data_value = '3' or data_value = 'P')
                                            then 'Y'
                                        else 'N'
                                   end) as is_done
                             from PARAM_D
                            where grp_code = 'CHK_MAST'
                              and data_name = 'CHK_STATUS'
                              and data_value not in ('C')
                            order by data_value";
            return DBWork.Connection.Query<ChkStatus>(sql, DBWork.Transaction);
        }

        public IEnumerable<MI_WHMAST> GetWhnoCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.WH_NO, A.WH_NAME, A.WH_KIND, A.WH_GRADE 
                          FROM MI_WHMAST A WHERE 1=1 AND WH_KIND = '0'  ";


            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.WH_NO, :WH_NO_I), 1000) + NVL(INSTR(A.WH_NAME, :WH_NAME_I), 100) * 10) IDX,"); // 設定權重, 值越小權重最大                
                p.Add(":WH_NO_I", p0);
                p.Add(":WH_NAME_I", p0);

                sql += " AND (A.WH_NO LIKE :WH_NO ";
                p.Add(":WH_NO", string.Format("%{0}%", p0));

                sql += " OR A.WH_NAME LIKE :WH_NAME) ";
                p.Add(":WH_NAME", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX, WH_NO", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY A.WH_NO ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_WHMAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);

        }

        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E
                        FROM MI_MAST A 
                        WHERE 1=1 
                          and a.mat_class in ('01')";

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

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
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
        #endregion
    }

    public class ChkStatus {
        public string VALUE { get; set; }
        public string TEXT { get; set; }
        public string IS_DONE { get; set; }
    }
}