using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Repository.C
{
    public class CE0011Repository : JCLib.Mvc.BaseRepository
    {
        public CE0011Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<CHK_MAST> GetMasterAll(string wh_no, string chk_ym, string keeper, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = @"select a.CHK_NO,
                                  a.CHK_YM,
                                  a.CHK_WH_NO, 
                                  a.CHK_WH_GRADE,
                                  a.CHK_WH_KIND,
                                  a.CHK_PERIOD,
                                  a.CHK_TYPE,
                                  a.CHK_LEVEL,
                                  a.CHK_NUM,
                                  a.CHK_TOTAL,
                                  a.CHK_STATUS,
                                  a.CHK_KEEPER,
                                  a.CHK_CLASS,
                                  (select g.una from UR_ID g where g.tuser = a.CHK_KEEPER) as CHK_KEEPER_NAME,
                                  a.CHK_NO1,
                                  (select b.WH_NO || ' ' ||b.WH_NAME from MI_WHMAST b where b.WH_NO = a.CHK_WH_NO) as WH_NAME,
                                  (select c.DATA_VALUE || ' ' || c.DATA_DESC 
                                     from PARAM_D c
                                    where c.GRP_CODE = 'CHK_MAST' 
                                      and c.DATA_NAME = 'CHK_WH_KIND'
                                      and c.DATA_VALUE = a.CHK_WH_KIND) as WH_KIND_NAME,
                                  (select d.DATA_VALUE || ' ' || d.DATA_DESC 
                                     from PARAM_D d
                                    where d.GRP_CODE = 'CHK_MAST' 
                                      and d.DATA_NAME = 'CHK_PERIOD'
                                      and d.DATA_VALUE = a.CHK_PERIOD) as CHK_PERIOD_NAME,
                                  (select e.DATA_VALUE || ' ' || e.DATA_DESC 
                                     from PARAM_D e
                                    where e.GRP_CODE = 'CHK_MAST' 
                                      and e.DATA_NAME = 'CHK_LEVEL'
                                      and e.DATA_VALUE = a.CHK_LEVEL) as CHK_LEVEL_NAME,
                                  (select f.DATA_VALUE || ' ' || f.DATA_DESC 
                                     from PARAM_D f
                                    where f.GRP_CODE = 'CHK_MAST' 
                                      and f.DATA_NAME = 'CHK_STATUS'
                                      and f.DATA_VALUE = a.CHK_STATUS) as CHK_STATUS_NAME,
                                  (select f.DATA_VALUE || ' ' || f.DATA_DESC 
                                     from PARAM_D f
                                    where f.GRP_CODE = 'CHK_MAST' 
                                      and f.DATA_NAME = 'CHK_CLASS'
                                      and f.DATA_VALUE = a.CHK_CLASS) as CHK_CLASS_NAME,
                                  (select h.chk_level from CHK_MAST h 
                                    where h.chk_no1 = a.chk_no 
                                      and h.chk_level = (select max(chk_level) from  CHK_MAST h2 
                                                          where h2.chk_no1 = a.chk_no 
                                                            and h2.chk_status in ('3', 'P', 'C'))  
                                  ) as FINAL_LEVEL,
                                  (select h.chk_level from CHK_MAST h 
                                    where h.chk_no1 = a.chk_no 
                                      and h.chk_level = (select max(chk_level) from  CHK_MAST h2 
                                                          where h2.chk_no1 = a.chk_no 
                                                            and h2.chk_status not in ('3', 'P', 'C'))  
                                  ) as ING_LEVEL
                             from CHK_MAST a
                            where a.chk_level = '1'
                              and chk_status in ('3', 'P', 'C')";

            if (wh_no != string.Empty) {
                sql += "      and a.CHK_WH_NO = :wh_no";
            }

            if (chk_ym != string.Empty)
            {
                sql += "      and a.CHK_YM like :chk_ym";
            }

            p.Add(":wh_no", string.Format("{0}", wh_no));
            p.Add(":chk_ym", string.Format("{0}%", chk_ym));
            p.Add(":keeper", string.Format("{0}", keeper));
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CHK_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);

        }

        public string GetChkWhkindName(string kind, string value)
        {

            string sql = string.Format(@"select c.DATA_VALUE || ' ' || c.DATA_DESC 
                                           from PARAM_D c
                                          where c.GRP_CODE = 'CHK_MAST' 
                                            and c.DATA_NAME = 'CHK_WH_KIND_{0}'
                                            and c.DATA_VALUE = '{1}'", kind, value);

            return DBWork.Connection.QueryFirst<string>(sql, DBWork.Transaction);
        }

        public IEnumerable<CHK_DETAIL> GetIncludeDetails(string chk_no, string chk_level, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();


            string sql = string.Format(
                    @"select a.mmcode, a.mmname_c, a.mmname_e,
                             (select base_unit from MI_MAST where mmcode = a.mmcode) as base_unit,
                             (a.store_qty) as store_qtyc,
                             (case
                                  WHEN a.STATUS_TOT = '1' THEN a.CHK_QTY1
                                     WHEN a.STATUS_TOT = '2' THEN a.CHK_QTY2
                                     WHEN a.STATUS_TOT = '3' THEN a.CHK_QTY3
                                     ELSE 0
                               end)    as chk_qty,
                             a.pro_los_qty as qty_diff,
                             a.consume_qty as consume_qty,
                             (select listagg(una, '<br>') within group (order by una)
                                     from (
                                            select distinct una 
                                              from chk_detail c, UR_ID d
                                             where c.chk_uid = d.tuser 
                                               and c.chk_no = a.chk_no
                                               and c.mmcode = a.mmcode
                                             order by una) 
                             ) as CHK_UID_NAME, 
                             (select listagg(store_loc||'-'||to_char(chk_qty), '<br>') within group (order by store_loc) as chk_details                  
                                     from chk_detail
                                    where chk_no = a. chk_no
                                      and mmcode = a. mmcode) as STORE_LOC_NAMES 
                        from CHK_DETAILTOT a
                       where chk_no = :chk_no", chk_level) ;

            p.Add(":chk_no", string.Format("{0}", chk_no));
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CHK_DETAIL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<CE0011M> Print(string chk_no, string chk_level)
        {
            var p = new DynamicParameters();


            string sql = string.Format(
                    @"select a.mmcode, a.mmname_c, a.mmname_e,
                             (select base_unit from MI_MAST where mmcode = a.mmcode) as base_unit,
                             (a.store_qty) as store_qtyc,
                             (case
                                  WHEN a.STATUS_TOT = '1' THEN a.CHK_QTY1
                                     WHEN a.STATUS_TOT = '2' THEN a.CHK_QTY2
                                     WHEN a.STATUS_TOT = '3' THEN a.CHK_QTY3
                                     ELSE 0
                               end)    as chk_qty,
                             a.pro_los_qty as qty_diff,
                             a.consume_qty as consume_qty,
                             (select listagg(una, '<br>') within group (order by una)
                                     from (
                                            select distinct una 
                                              from chk_detail c, UR_ID d
                                             where c.chk_uid = d.tuser 
                                               and c.chk_no = a.chk_no
                                               and c.mmcode = a.mmcode
                                             order by una) ) as CHK_UID_NAME, 
                             (select listagg(store_loc||'-'||to_char(chk_qty), '<br>') within group (order by store_loc) as chk_details                  
                                     from chk_detail
                                    where chk_no = a. chk_no
                                      and mmcode = a. mmcode) as STORE_LOC_NAMES,
                             e.CHK_NO,
                             e.CHK_YM,
                             e.CHK_WH_NO, 
                             e.CHK_WH_GRADE,
                             e.CHK_WH_KIND,
                             e.CHK_PERIOD,
                             e.CHK_TYPE,
                             e.CHK_LEVEL,
                             e.CHK_NUM,
                             e.CHK_TOTAL,
                             e.CHK_STATUS,
                             e.CHK_KEEPER,
                             e.CHK_CLASS,
                             (select una from UR_ID where tuser = e.CHK_KEEPER) as CHK_KEEPER_NAME,
                             (select WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO = e.CHK_WH_NO) as WH_NAME,
                             (select DATA_VALUE || ' ' || DATA_DESC 
                                from PARAM_D
                               where GRP_CODE = 'CHK_MAST' 
                                 and DATA_NAME = 'CHK_WH_KIND'
                                 and DATA_VALUE = e.CHK_WH_KIND) as WH_KIND_NAME,
                             (select DATA_VALUE || ' ' || DATA_DESC 
                                from PARAM_D
                               where GRP_CODE = 'CHK_MAST' 
                                 and DATA_NAME = 'CHK_PERIOD'
                                 and DATA_VALUE = e.CHK_PERIOD) as CHK_PERIOD_NAME,
                             (select DATA_VALUE || ' ' || DATA_DESC 
                                from PARAM_D
                               where GRP_CODE = 'CHK_MAST' 
                                 and DATA_NAME = 'CHK_LEVEL'
                                 and DATA_VALUE = e.CHK_LEVEL) as CHK_LEVEL_NAME,
                             (select DATA_VALUE || ' ' || DATA_DESC 
                                from PARAM_D f
                               where GRP_CODE = 'CHK_MAST' 
                                 and DATA_NAME = 'CHK_STATUS'
                                 and DATA_VALUE = e.CHK_STATUS) as CHK_STATUS_NAME,
                             (select DATA_VALUE || ' ' || DATA_DESC 
                                from PARAM_D f
                               where GRP_CODE = 'CHK_MAST' 
                                 and DATA_NAME = 'CHK_CLASS'
                                 and DATA_VALUE = e.CHK_CLASS) as CHK_CLASS_NAME
                        from CHK_DETAILTOT a, CHK_MAST e
                       where a.chk_no = :chk_no
                         and e.chk_no = a.chk_no
                       order by mmcode", chk_level);

            p.Add(":chk_no", string.Format("{0}", chk_no));

            return DBWork.Connection.Query<CE0011M>(sql, p, DBWork.Transaction);
        }
    }
}