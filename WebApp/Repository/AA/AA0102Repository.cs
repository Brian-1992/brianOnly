using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using TSGH.Models;
using System.Collections.Generic;

namespace WebApp.Repository.AA
{

    public class AA0102_MODEL : JCLib.Mvc.BaseModel
    {
        public string INID { get; set; }
        public string INID_NAME { get; set; }
        public string WH_NO { get; set; }
        public string WH_NAME { get; set; }
        public string WH_KIND { get; set; }
        public string CHK_NO { get; set; }
        public string CHK_TYPE { get; set; }
        public string CHK_LEVEL { get; set; }
        public string CHK_CLASS { get; set; }
        public string CHK_TYPE_NAME { get; set; }
        public string CHK_CLASS_NAME { get; set; }
        public string WH_KIND_NAME { get; set; }
        public string CHK_LEVEL_NAME { get; set; }
        public string REPORT_TYPE { get; set; }
        public string CHK_YM { get; set; }

        public string MMCODE { get; set; }
        public string M_STOREID { get; set;}
        public string M_CONTID { get; set; }
        public string MMCODE_COUNT { get; set; }
        public string MMCODE_STRING { get; set; }
        public IEnumerable<AA0102_MODEL> MMCODE_LIST { get; set; }

    }
    public class AA0102Repository : JCLib.Mvc.BaseRepository
    {

        public AA0102Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        #region sql
        // 完全未盤點
        private string GetAA0102Sql(string trshowdata, bool isExcel) {
            // 非藥局未開單 + 非藥局初盤未輸入 + 藥局未開單 + 藥局初盤未輸入
            // 2020-12-11: 2020-12-07開會稽核組提出僅抓衛材庫 wh_kind = '1'
            #region sql
            string sql = string.Format(@"
                            
                                select a1.inid, a1.inid_name, b1.wh_no, b1.wh_name , b1.wh_kind, 
                                       c.chk_type, c.chk_level, c.chk_no1,c.chk_no,
                                       (select DATA_DESC from PARAM_D
                                         where GRP_CODE = 'CHK_MAST' 
                                           and DATA_NAME = 'CHK_CLASS'
                                           and DATA_VALUE = c.CHK_CLASS) as chk_class_name,
                                       (select DATA_DESC from PARAM_D
                                         where GRP_CODE = 'CHK_MAST' 
                                           and DATA_NAME = 'CHK_WH_KIND'
                                           and DATA_VALUE = c.CHK_WH_KIND) as wh_kind_name,
                                       (select DATA_DESC from PARAM_D 
                                         where GRP_CODE = 'CHK_MAST' 
                                           and DATA_NAME = 'CHK_LEVEL'
                                           and DATA_VALUE = c.CHK_LEVEL) as chk_level_name
                                  from UR_INID a1, MI_WHMAST b1,  chk_mast c 
                                 where a1.inid_flag {0} :TRSHOWDATA
                                   and b1.inid = a1.inid
                                   and b1.cancel_id = 'N' 
                                   and b1.wh_kind = '1'
                                   and c.chk_wh_no = b1.wh_no
                                   and c.chk_ym = :APPTIME1
                                   and c.chk_period = 'M'
                                   and c.chk_level = '1'
                                   and c.chk_total > 0
                                   and not exists (select 1 from CHK_DETAIL 
                                                    where chk_no = c.chk_no 
                                                      and chk_time > TO_DATE ('2000-01-01','YYYY-MM-DD'))
                            
                              ", trshowdata == "D" ? "<>" : " =");

            if (isExcel) {
                sql = string.Format(@" select a.inid as 單位代碼, a.inid_name as 單位名稱,
                                              a.wh_no as 庫房代碼,
                                              a.wh_name as 庫房名稱,
                                              (
                                                case when wh_kind = '0'
                                                        then (select data_desc from param_d 
                                                               where grp_code = 'CHK_MAST'
                                                                 and data_name = 'CHK_WH_KIND_0'
                                                                 and data_value = a.chk_type)
                                                     when wh_kind = '1'
                                                        then (select data_desc from param_d 
                                                               where grp_code = 'CHK_MAST'
                                                                 and data_name = 'CHK_WH_KIND_1'
                                                                 and data_value = a.chk_type)
                                                      when wh_kind = 'E'
                                                        then (select data_desc from param_d 
                                                               where grp_code = 'CHK_MAST'
                                                                 and data_name = 'CHK_WH_KIND_E'
                                                                 and data_value = a.chk_type)
                                                     when wh_kind = 'C'
                                                        then (select data_desc from param_d 
                                                               where grp_code = 'CHK_MAST'
                                                                 and data_name = 'CHK_WH_KIND_C'
                                                                 and data_value = a.chk_type)
                                                     else ''
                                                  end) as 盤點類別,
                                              a.chk_class_name as 物料類別,
                                              a.chk_level_name as 盤點階段,
                                              a.wh_kind_name as 庫房類別
                                         from ( {0} ) a
                                        order by a.inid, a.wh_no, a.chk_type", sql);
            }
            #endregion

            return sql;
        }
        // 未完全盤點
        private string GetAA0103Sql(string trshowdata, bool isExcel) {
            // 非藥局複盤三盤 + 非藥局初盤 + 藥局複盤 + 藥局初盤
            // 2020-12-11: 2020-12-07開會稽核組提出僅抓衛材庫 wh_kind = '1'
            string sql =  string.Format(@"(select a1.inid, a1.inid_name, b1.wh_no, b1.wh_name , b1.wh_kind, 
                                           c.chk_type, c.chk_level, c.chk_no1,c.chk_no,
                                           (select DATA_DESC from PARAM_D
                                             where GRP_CODE = 'CHK_MAST' 
                                               and DATA_NAME = 'CHK_CLASS'
                                               and DATA_VALUE = c.CHK_CLASS) as chk_class_name,
                                           (select DATA_DESC from PARAM_D
                                             where GRP_CODE = 'CHK_MAST' 
                                               and DATA_NAME = 'CHK_WH_KIND'
                                               and DATA_VALUE = c.CHK_WH_KIND) as wh_kind_name,
                                           (select DATA_DESC from PARAM_D 
                                             where GRP_CODE = 'CHK_MAST' 
                                               and DATA_NAME = 'CHK_LEVEL'
                                               and DATA_VALUE = c.CHK_LEVEL) as chk_level_name
                                      from UR_INID a1, MI_WHMAST b1,  chk_mast c 
                                     where a1.inid_flag {0} :TRSHOWDATA
                                       and b1.inid = a1.inid
                                       and b1.cancel_id = 'N' 
                                       and b1.wh_kind = '1'
                                       and c.chk_wh_no = b1.wh_no
                                       and c.chk_ym = :APPTIME1
                                       and c.chk_level > '1'
                                       and c.chk_period = 'M'
                                       and c.chk_total > 0
                                       and c.chk_no = (select max(chk_no) from CHK_MAST 
                                                        where chk_ym = c.chk_ym 
                                                          and chk_wh_no = c.chk_wh_no 
                                                          and chk_type = c.chk_type
                                                          and chk_period = c.chk_period)
                                       and exists (
                                             select 1 from CHK_DETAIL d
                                              where 1=1 
                                                and d.chk_no = c.chk_no
                                                and d.chk_time is null
                                       )
                                    )
                                    union all
                                    (select a1.inid, a1.inid_name, b1.wh_no, b1.wh_name , b1.wh_kind,
                                            c.chk_type, c.chk_level, c.chk_no1,c.chk_no,
                                            (select DATA_DESC from PARAM_D
                                              where GRP_CODE = 'CHK_MAST' 
                                                and DATA_NAME = 'CHK_CLASS'
                                                and DATA_VALUE = c.CHK_CLASS) as chk_class_name,
                                            (select DATA_DESC from PARAM_D
                                              where GRP_CODE = 'CHK_MAST' 
                                                and DATA_NAME = 'CHK_WH_KIND'
                                                and DATA_VALUE = c.CHK_WH_KIND) as wh_kind_name,
                                            (select DATA_DESC from PARAM_D 
                                              where GRP_CODE = 'CHK_MAST' 
                                                and DATA_NAME = 'CHK_LEVEL'
                                                and DATA_VALUE = c.CHK_LEVEL) as chk_level_name
                                       from UR_INID a1, MI_WHMAST b1,  chk_mast c 
                                      where a1.inid_flag {0} :TRSHOWDATA
                                        and b1.inid = a1.inid
                                        and b1.cancel_id = 'N' 
                                        and b1.wh_kind = '1'
                                        and c.chk_wh_no = b1.wh_no
                                        and c.chk_ym = :APPTIME1
                                        and c.chk_level = '1'
                                        and c.chk_total > 0
                                        and not exists (
                                                    select 1 from CHK_mast 
                                                     where 1=1 
                                                        and chk_no1 = c.chk_no1
                                                        and chk_level > '1'
                                                   )
                                        and exists (select 1 from CHK_DETAIl where chk_no = c.chk_no and chk_time is null)
                                        and exists (select 1 from CHK_DETAIl where chk_no = c.chk_no and chk_time > TO_DATE ('2000-01-01','YYYY-MM-DD'))
                                    )
                              ", trshowdata == "D" ? "<>" : " =");
            if (isExcel)
            {
                sql = string.Format(@" select a.inid as 單位代碼, a.inid_name as 單位名稱,
                                              a.wh_no as 庫房代碼,
                                              a.wh_name as 庫房名稱,
                                              (
                                                case when wh_kind = '0'
                                                        then (select data_desc from param_d 
                                                               where grp_code = 'CHK_MAST'
                                                                 and data_name = 'CHK_WH_KIND_0'
                                                                 and data_value = a.chk_type)
                                                     when wh_kind = '1'
                                                        then (select data_desc from param_d 
                                                               where grp_code = 'CHK_MAST'
                                                                 and data_name = 'CHK_WH_KIND_1'
                                                                 and data_value = a.chk_type)
                                                      when wh_kind = 'E'
                                                        then (select data_desc from param_d 
                                                               where grp_code = 'CHK_MAST'
                                                                 and data_name = 'CHK_WH_KIND_E'
                                                                 and data_value = a.chk_type)
                                                     when wh_kind = 'C'
                                                        then (select data_desc from param_d 
                                                               where grp_code = 'CHK_MAST'
                                                                 and data_name = 'CHK_WH_KIND_C'
                                                                 and data_value = a.chk_type)
                                                     else ''
                                                  end) as 盤點類別,
                                              a.chk_class_name as 物料類別,
                                              a.chk_level_name as 盤點階段,
                                              a.wh_kind_name as 庫房類別
                                         from ( {0} ) a
                                        order by a.inid, a.wh_no, a.chk_type", sql);
            }

            return sql;
        }
        // 有盤點
        private string GetFA0043Sql(string trshowdata, bool isExcel) {
            string sql =  string.Format(@"(
                             select a1.inid, a1.inid_name, b1.wh_no, b1.wh_name, b1.wh_kind,  
                                    c.chk_type, c.chk_level, c.chk_no1, c.chk_no, c.chk_class,
                                    (select DATA_DESC from PARAM_D
                                      where GRP_CODE = 'CHK_MAST' 
                                        and DATA_NAME = 'CHK_CLASS'
                                        and DATA_VALUE = c.CHK_CLASS) as chk_class_name,
                                    (select DATA_DESC from PARAM_D
                                      where GRP_CODE = 'CHK_MAST' 
                                        and DATA_NAME = 'CHK_WH_KIND'
                                        and DATA_VALUE = c.CHK_WH_KIND) as wh_kind_name,
                                    (select DATA_DESC from PARAM_D 
                                      where GRP_CODE = 'CHK_MAST' 
                                        and DATA_NAME = 'CHK_LEVEL'
                                        and DATA_VALUE = c.CHK_LEVEL) as chk_level_name
                               from UR_INID a1, MI_WHMAST b1,  chk_mast c 
                              where a1.inid_flag {0} :TRSHOWDATA
                                and b1.inid = a1.inid
                                and b1.cancel_id = 'N' 
                                and b1.wh_kind = '1'
                                and c.chk_wh_no = b1.wh_no
                                and c.chk_ym = :APPTIME1
                                and c.chk_no = (select max(chk_no) from CHK_MAST where chk_ym = c.chk_ym and chk_wh_no = c.chk_wh_no and chk_type = c.chk_type)
                                and c.chk_total > 0
                                and not exists (
                                        select 1 from CHK_DETAIL d
                                         where 1=1 
                                           and d.chk_no = c.chk_no
                                           and d.chk_time is null
                                       )
                            )
                            
                              ", trshowdata == "D" ? "<>" : " =");
            if (isExcel)
            {
                sql = string.Format(@" select a.inid as 單位代碼, a.inid_name as 單位名稱,
                                              a.wh_no as 庫房代碼,
                                              a.wh_name as 庫房名稱,
                                              (
                                                case when wh_kind = '0'
                                                        then (select data_desc from param_d 
                                                               where grp_code = 'CHK_MAST'
                                                                 and data_name = 'CHK_WH_KIND_0'
                                                                 and data_value = a.chk_type)
                                                     when wh_kind = '1'
                                                        then (select data_desc from param_d 
                                                               where grp_code = 'CHK_MAST'
                                                                 and data_name = 'CHK_WH_KIND_1'
                                                                 and data_value = a.chk_type)
                                                      when wh_kind = 'E'
                                                        then (select data_desc from param_d 
                                                               where grp_code = 'CHK_MAST'
                                                                 and data_name = 'CHK_WH_KIND_E'
                                                                 and data_value = a.chk_type)
                                                     when wh_kind = 'C'
                                                        then (select data_desc from param_d 
                                                               where grp_code = 'CHK_MAST'
                                                                 and data_name = 'CHK_WH_KIND_C'
                                                                 and data_value = a.chk_type)
                                                     else ''
                                                  end) as 盤點類別,
                                              a.chk_class_name as 物料類別,
                                              a.chk_level_name as 盤點階段,
                                              a.wh_kind_name as 庫房類別
                                         from ( {0} ) a
                                        order by a.inid, a.wh_no, a.chk_type", sql);
            }
            return sql;
        }
        #endregion

        #region common

        public string GetChkWhkindName(string kind, string value)
        {

            string sql = string.Format(@"select DATA_DESC 
                                           from PARAM_D
                                          where GRP_CODE = 'CHK_MAST' 
                                            and DATA_NAME = 'CHK_WH_KIND_{0}'
                                            and DATA_VALUE = '{1}'", kind, value);

            return DBWork.Connection.QueryFirst<string>(sql, DBWork.Transaction);
        }
        #endregion

        #region grid
        //AA0102
        public IEnumerable<AA0102_MODEL> GetAll(string APPTIME1, string trshowdata, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = GetAA0102Sql(trshowdata, false);

            p.Add(":TRSHOWDATA", trshowdata);
            p.Add(":APPTIME1", APPTIME1);


            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0102_MODEL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //AA0103
        public IEnumerable<AA0102_MODEL> GetAll_AA0103(string APPTIME1, string trshowdata, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            string sql = GetAA0103Sql(trshowdata, false);

            p.Add(":TRSHOWDATA", trshowdata);
            p.Add(":APPTIME1", APPTIME1);


            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0102_MODEL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //FA0043
        public IEnumerable<AA0102_MODEL> GetAll_FA0043(string APPTIME1, string trshowdata, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = GetFA0043Sql(trshowdata, false);

            p.Add(":TRSHOWDATA", trshowdata);
            p.Add(":APPTIME1", APPTIME1);


            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0102_MODEL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        #endregion

        #region excel
        //AA0102
        public DataTable GetExcel(string APPTIME1, string trshowdata)
        {

            DynamicParameters p = new DynamicParameters();

            string sql = GetAA0102Sql(trshowdata, true);

            p.Add(":TRSHOWDATA", trshowdata);

            if (APPTIME1 != "" && APPTIME1 != null)
            {
                p.Add(":APPTIME1", APPTIME1);
            }
            else
            {
                p.Add(":APPTIME1", "");
            }

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        //AA0103
        public DataTable GetExcel_AA0103(string APPTIME1, string trshowdata)
        {

            DynamicParameters p = new DynamicParameters();

            string sql = GetAA0103Sql(trshowdata, true);

            p.Add(":TRSHOWDATA", trshowdata);

            if (APPTIME1 != "" && APPTIME1 != null)
            {
                p.Add(":APPTIME1", APPTIME1);
            }
            else
            {
                p.Add(":APPTIME1", "");
            }

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        //FA0043
        public DataTable GetExcel_FA0043(string APPTIME1, string trshowdata)
        {

            DynamicParameters p = new DynamicParameters();

            string sql = GetFA0043Sql(trshowdata, true);


            p.Add(":TRSHOWDATA", trshowdata);

            if (APPTIME1 != "" && APPTIME1 != null)
            {
                p.Add(":APPTIME1", APPTIME1);
            }
            else
            {
                p.Add(":APPTIME1", "");
            }

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
        #endregion

        #region print
        //AA0102
        public IEnumerable<AA0102_MODEL> GetPrintData(string APPTIME1, string trshowdata)
        {

            DynamicParameters p = new DynamicParameters();

            string sql = GetAA0102Sql(trshowdata, false);

            p.Add(":TRSHOWDATA", trshowdata);

            if (APPTIME1 != "" && APPTIME1 != null)
            {
                p.Add(":APPTIME1", APPTIME1);
            }
            else
            {
                p.Add(":APPTIME1", "");
            }


            return DBWork.Connection.Query<AA0102_MODEL>(sql, p, DBWork.Transaction);
        }
        //AA0103
        public IEnumerable<AA0102_MODEL> AA0103_GetPrintData(string APPTIME1, string trshowdata)
        {
            DynamicParameters p = new DynamicParameters();

            string sql = GetAA0103Sql(trshowdata, false);

            p.Add(":TRSHOWDATA", trshowdata);

            if (APPTIME1 != "" && APPTIME1 != null)
            {
                p.Add(":APPTIME1", APPTIME1);
            }
            else
            {
                p.Add(":APPTIME1", "");
            }


            return DBWork.Connection.Query<AA0102_MODEL>(sql, p, DBWork.Transaction);
        }
        //FA0043
        public IEnumerable<AA0102_MODEL> FA0043_GetPrintData(string APPTIME1, string trshowdata)
        {
            DynamicParameters p = new DynamicParameters();

            string sql = GetFA0043Sql(trshowdata, false);

            p.Add(":TRSHOWDATA", trshowdata);

            if (APPTIME1 != "" && APPTIME1 != null)
            {
                p.Add(":APPTIME1", APPTIME1);
            }
            else
            {
                p.Add(":APPTIME1", "");
            }


            return DBWork.Connection.Query<AA0102_MODEL>(sql, p, DBWork.Transaction);
        }
        #endregion

        #region radio number
        //AA0102,AA0103 Raddio選項後塞值
        public string GetRadio1()
        {
            var p = new DynamicParameters();

            string sql = @"Select count(*) as VALUE from ur_inid where INID_FLAG = 'A'";

            return DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction).ToString();

        }
        //AA0102,AA0103 Raddio選項後塞值
        public string GetRadio2()
        {
            var p = new DynamicParameters();

            string sql = @"Select count(*) as VALUE from ur_inid where INID_FLAG = 'B'";

            return DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction).ToString();

        }
        //AA0102,AA0103 Raddio選項後塞值
        public string GetRadio3()
        {
            var p = new DynamicParameters();

            string sql = @"Select count(*) as VALUE from ur_inid where INID_FLAG = 'C'";

            return DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction).ToString();

        }
        //AA0102,AA0103 Raddio選項後塞值
        public string GetRadio4()
        {
            var p = new DynamicParameters();

            string sql = @"Select count(*) as VALUE from ur_inid where INID_FLAG <> 'D'";

            return DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction).ToString();

        }
        #endregion

        #region 2021-10-20 檢查開單後是否有點收品項或有在途量但不在盤點單內
        public IEnumerable<AA0102_MODEL> GetChkNotExists() {
            string sql = string.Format(@"
                with set_ym as (
                    select set_ym from MI_MNSET where set_status = 'N'
                 ),chk_settime as (
                    select chk_ym, set_atime 
                      from CHK_MNSET a, set_ym b
                     where a.chk_ym = b.set_ym
                ), inids as (
                    select a.inid, a.inid_name, b.wh_no, b.wh_name , b.wh_kind
                      from UR_INID a, MI_WHMAST b
                     where b.inid = a.inid
                       and b.cancel_id = 'N' 
                       and b.wh_kind = '1'
                ), wh_trns_data as (
                    select b.inid, b.inid_name, b.wh_no, b.wh_name , b.wh_kind, 
                           a.mmcode, d.m_storeId, 
                           (case when d.m_contid = '3' then '3' else 'N3' end) as m_contid
                      from MI_WHTRNS a, inids b, chk_settime c, MI_MAST d
                     where a.wh_no = b.wh_no
                       and a.tr_date >= c.set_atime
                       and a.mmcode = d.mmcode
                       and d.mat_class = '02'
                       and ( (a.tr_mcode = 'WAYI' and tr_doctype in ('MR1', 'MR2', 'MR3', 'MR4','TR1'))
                             or (a.tr_mcode = 'TRNO')
                             or a.tr_mcode = 'BAKO')
                )
                select * 
                  from wh_trns_data a
                 where 1=1
                   and not exists (select 1 from CHK_DETAIL
                                    where chk_no in (select chk_no from CHK_MAST 
                                                      where chk_ym = twn_yyymm(sysdate)
                                                        and chk_wh_no = a.wh_no
                                                        and chk_period = 'M')
                                      and mmcode = a.mmcode)
            ");
            return DBWork.Connection.Query<AA0102_MODEL>(sql, DBWork.Transaction);
        }

        #endregion

    }
}