using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Repository.C
{
    public class CE0008Repository : JCLib.Mvc.BaseRepository
    {
        public CE0008Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<CHK_MAST> GetChkMasts(string wh_no, string chk_ym, string keeper, string chk_status, string chk_level, int page_index, int page_size, string sorters)
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
                                  (select c.DATA_VALUE || ' ' || c.DATA_DESC 
                                     from PARAM_D c
                                    where c.GRP_CODE = 'CHK_MAST' 
                                      and c.DATA_NAME = 'CHK_CLASS'
                                      and c.DATA_VALUE = a.CHK_CLASS) as CHK_CLASS_NAME,
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
                                  (case
                                    when a.chk_status = '0' then ''
                                    else (select listagg(una, '、') within group (order by una)
                                            from ( 
                                                  (select una 
                                                    from CHK_NOUID c, UR_ID d
                                                   where c.chk_no = a.chk_no
                                                     and c.chk_uid = d.tuser)
                                                  union
                                                  (
                                                  select una from CHK_DETAIL c, UR_ID d
                                                   where c.chk_no = a.chk_no 
                                                     and c.chk_uid = d.tuser
                                                  )
                                                  union
                                                  (
                                                  select una from CHK_GRADE2_UPDN c, UR_ID d
                                                   where c.chk_no = a.chk_no 
                                                     and c.chk_uid = d.tuser
                                                  )
                                             order by una))
                                  end) as chk_uid_names
                             from CHK_MAST a
                            where a.chk_level = :chk_level";
            if (wh_no != string.Empty)
            {
                sql += "  and a.CHK_WH_NO = :wh_no";
            }
            if (chk_ym != string.Empty)
            {
                sql += string.Format("  and a.CHK_YM like '{0}%'", chk_ym);
            }
            if (chk_status != string.Empty)
            {
                sql += @"     and a.CHK_STATUS = :chk_status";
            }

            p.Add(":wh_no", string.Format("{0}", wh_no));
            p.Add(":chk_ym", string.Format("{0}", chk_ym));
            p.Add(":keeper", string.Format("{0}", keeper));
            p.Add(":chk_level", string.Format("{0}", chk_level));
            p.Add(":chk_status", string.Format("{0}", chk_status));
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

        #region 新增

        // 取得新增來源清單
        public IEnumerable<CHK_MAST> GetMasterInsertList(string wh_no, string chk_ym, string keeper, int page_index, int page_size, string sorters)
        {
            string chk_ym_datetime = GetDateTime(chk_ym);
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
                                  a.CHK_CLASS,
                                  a.CHK_STATUS,
                                  a.CHK_KEEPER,
                                  (select  g.una from UR_ID g where g.tuser = a.CHK_KEEPER) as CHK_KEEPER_NAME,
                                  a.CHK_NO1,
                                  (select b.WH_NO || ' ' ||b.WH_NAME from MI_WHMAST b where b.WH_NO = a.CHK_WH_NO) as WH_NAME,
                                  (select c.DATA_VALUE || ' ' || c.DATA_DESC 
                                     from PARAM_D c
                                    where c.GRP_CODE = 'CHK_MAST' 
                                      and c.DATA_NAME = 'CHK_CLASS'
                                      and c.DATA_VALUE = a.CHK_CLASS) as CHK_CLASS_NAME,
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
                                  a.create_user,
                                  (case
                                    when a.create_user = 'BATCH' then 'Y' else 'N'
                                  end) as is_batch,
                                  (case
                                    when a.chk_status = '0' then ''
                                    else (select listagg(una, '、') within group (order by una)
                                            from ( 
                                                  (select una 
                                                    from CHK_NOUID c, UR_ID d
                                                   where c.chk_no = a.chk_no
                                                     and c.chk_uid = d.tuser)
                                                  union
                                                  (
                                                  select una from CHK_DETAIL c, UR_ID d
                                                   where c.chk_no = a.chk_no 
                                                     and c.chk_uid = d.tuser
                                                  )
                                                  union
                                                  (
                                                  select una from CHK_GRADE2_UPDN c, UR_ID d
                                                   where c.chk_no = a.chk_no 
                                                     and c.chk_uid = d.tuser
                                                  )
                                             order by una))
                                  end) as chk_uid_names
                             from CHK_MAST a
                            where SUBSTR(a.CHK_YM, 0, 5) = :chk_ym
                              and a.CHK_WH_NO = :wh_no
                              and a.chk_level = '2'
                              and a.chk_status = '3'
                              and a.chk_no1 not in (
                                    select NVL(chk_no1, ' ')
                                      from CHK_MAST
                                     where CHK_WH_NO = :wh_no
                                       and chk_level = '3'
                                       and SUBSTR(a.CHK_YM, 0, 5) = :chk_ym
                                    )
                              and exists (select 1 from MI_MNSET
                                           where set_status = 'N'
                                             and twn_date(a.create_date) between twn_date(set_btime) and twn_date(set_ctime) 
                                  )";

            p.Add(":wh_no", string.Format("{0}", wh_no));
            p.Add(":chk_ym", string.Format("{0}", chk_ym));
            p.Add(":keeper", string.Format("{0}", keeper));
            p.Add(":chk_ym_datetime", chk_ym_datetime);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CHK_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);

        }
        private string GetDateTime(string yyymm)
        {
            int yyyy = int.Parse(yyymm.Substring(0, 3)) + 1911;
            string mm = yyymm.Substring(3, 2);
            return string.Format("{0}-{1}-01", yyyy, mm);
        }

        public int InsertChkMast(CHK_MAST mast)
        {
            string sql = @"insert into CHK_MAST
                                  (CHK_NO, CHK_YM, CHK_WH_NO, CHK_WH_GRADE, CHK_WH_KIND,CHK_CLASS,
                                  CHK_PERIOD, CHK_TYPE, CHK_LEVEL, CHK_NUM, CHK_TOTAL, CHK_STATUS, CHK_KEEPER, CHK_NO1,
                                  CREATE_DATE, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)
                           values (:chk_no, :chk_ym, :chk_wh_no, :chk_wh_grade, :chk_wh_kind, :chk_class,
                                   :chk_period, :chk_type, '3', 0, 0, 0, :chk_keeper, : chk_no1,
                                   sysdate, :create_user, sysdate, :update_user, :update_ip
                                  )";

            return DBWork.Connection.Execute(sql, mast, DBWork.Transaction);
        }

        public bool CheckExists(string chk_no)
        {
            string sql = @"select 1 from CHK_MAST
                            WHERE chk_no1 = :chk_no 
                              and chk_level = '3'"; // TRUNC(a.EXP_DATE, 'DD') TWN_TODATE(:exp_date)
            return !(DBWork.Connection.ExecuteScalar(sql,
                                                     new { chk_no = chk_no, },
                                                     DBWork.Transaction) == null);
        }
        public string GetCurrentSeq(string wh_no, string ym)
        {
            string sql = @"select NVL(max(substr(CHK_NO ,15,2))+1, 1)
                             from CHK_MAST 
                            where substr(chk_no,1,11) = :chk_no";

            string result = DBWork.Connection.QueryFirst<string>(sql,
                                                         new
                                                         {
                                                             chk_no = string.Format("{0}{1}", wh_no, ym)
                                                         }, DBWork.Transaction);
            return result.PadLeft(2, '0');

        }
        #endregion

        #region 刪除
        public int DeleteChkDetail(string chk_no)
        {
            string sql = @"delete from CHK_DETAIL
                            where chk_no = :chk_no";

            return DBWork.Connection.Execute(sql, new { CHK_NO = chk_no }, DBWork.Transaction);
        }

        public int DeleteChkMast(string chk_no)
        {
            string sql = @"delete from CHK_MAST
                            where chk_no = :chk_no";

            return DBWork.Connection.Execute(sql, new { CHK_NO = chk_no }, DBWork.Transaction);
        }
        #endregion

        #region 盤點明細主畫面

        public IEnumerable<CHK_DETAIL> GetIncludeDetails(string chk_no, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = @"select a.*,
                             (select base_unit from MI_MAST where mmcode = a.mmcode) as base_unit,
                              b.una as chk_uid_name,
                             (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D 
                               where GRP_CODE = 'CHK_DETAIL'
                                 and DATA_NAME = 'STATUS_INI'
                                 and DATA_VALUE = a.STATUS_INI) as STATUS_INI_NAME
                             from CHK_DETAIL a
                             left outer join UR_ID b on b.tuser = a.chk_uid
                            where chk_no = :chk_no";

            p.Add(":chk_no", string.Format("{0}", chk_no));
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CHK_DETAIL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<CHK_DETAIL> GetExcludeDetails(string chk_no, string chk_no1, string wh_no, float f_u_price, float f_number, float f_amount, string f_mmcode, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            #region old sql ~190623
            //string sql = @"select a.mmcode, a.mmname_c, a.mmname_e, a.m_purun, a.m_contprice,
            //                      a.wh_no, a.store_loc, 
            //                      NVL(a.loc_name,' ') as loc_name, 
            //                      a.mat_class, a.m_storeid,
            //                      c.e_takekind, c.e_restrictcode, a.store_qtyc, a.chk_qty,
            //                      (a.store_qtyc - a.chk_qty) as QTY_DIFF,
            //                      a.chk_uid,
            //                      (a.chk_uid || ' '|| b.una) as CHK_UID_NAME,
            //                      c.base_unit
            //                 from CHK_DETAIL a, UR_ID b, MI_MAST c
            //                where a.chk_no = :chk_no
            //                  and b.tuser = a.chk_uid
            //                  and c.mmcode = a.mmcode";
            #endregion

            string sql = @"select a.mmcode, a.mmname_c, a.mmname_e, a.base_unit,a.m_contprice,	
                                  a.wh_no, a.mat_class,	a.m_storeid, b.e_takekind, b.e_restrictcode,
                                  a.store_qty as STORE_QTYC, ";

            if (wh_no == "PH1S")
            {
                sql += @"          (select inv_qty
                                      from mi_whinv
                                     where wh_no = a.wh_no
                                       and mmcode = a.mmcode) as NEW_STORE_QTYC,";
            }

            sql += string.Format(@"             a.chk_qty2 as CHK_QTY,
                                  ( a.chk_qty2 - a.store_qty{0}) as QTY_DIFF,
                                  (case
                                                    when (a.store_qty{0} = 0 or a.store_qty{0} is null)
                                                        then (a.chk_qty2 - a.store_qty{0})
                                                    else TRUNC(((a.chk_qty2 - a.store_qty{0}) / a.store_qty{0} * 100),5)
                                                  end) as diff_p,
                                  (select listagg(chk_uid, ',') within group (order by chk_uid)
                                     from (
                                            select distinct (chk_uid) as chk_uid
                                              from chk_detail c, chk_mast d
                                             where d.chk_no1 = a.chk_no
                                               and d.chk_level = '2'
                                               and c.chk_no = d.chk_no
                                               and c.mmcode = a.mmcode
                                             order by chk_uid) ) as CHK_UID, 
                                  (select listagg(una, '<br>') within group (order by una)
                                     from (
                                            select distinct ( una) as UNA
                                              from chk_detail c, UR_ID d, chk_mast e
                                             where e.chk_no1 = a.chk_no
                                               and e.chk_level = '2' 
                                               and c.chk_no = e.chk_no
                                               and c.chk_uid = d.tuser 
                                               and c.mmcode = a.mmcode
                                             order by una) ) as CHK_UID_NAME, 
                                  (select listagg(store_loc, ',') within group (order by store_loc) as chk_details                  
                                     from chk_detail c, chk_mast d
                                    where d.chk_no1 = a.chk_no
                                      and d.chk_level = '2'
                                      and c.chk_no = d.chk_no
                                      and c.mmcode = a.mmcode) as STORE_LOC,
                                  (select listagg(store_loc, '<br>') within group (order by store_loc) as chk_details                  
                                     from chk_detail c, chk_mast d
                                    where d.chk_no1 = a.chk_no
                                      and d.chk_level = '2'
                                      and c.chk_no = d.chk_no
                                      and c.mmcode = a.mmcode) as STORE_LOC_NAME,
                                  0 as STATUS_INI,
                                  '0 準備' as STATUS_INI_NAME
                             from CHK_DETAILTOT a , MI_MAST b
                            where a.chk_no = :chk_no1       
                              and a.status_tot = '2'
                              and b.mmcode = a.mmcode", wh_no == "PH1S" ? "c" : string.Empty);

            // [藥庫] 只要現庫存量與初盤庫存量不等的品項  
            if (wh_no == "PH1S")
            {
                sql += @"     and a.store_qtyc - (
                                    select inv_qty
                                      from mi_whinv
                                     where wh_no = a.wh_no
                                       and mmcode = a.mmcode) <> 0";
            }

            sql += GetFilter(f_u_price, f_number, f_amount, f_mmcode);
            sql += @"         and not exists ( select 'x' from CHK_DETAIL_TEMP 
                                                where chk_no = :chk_no 
                                                  and mmcode = a.mmcode)";

            p.Add(":chk_no", string.Format("{0}", chk_no));
            p.Add(":chk_no1", string.Format("{0}", chk_no1));
            p.Add(":f_u_price", string.Format("{0}", f_u_price));
            p.Add(":f_number", string.Format("{0}", f_number));
            p.Add(":f_amount", string.Format("{0}", f_amount));
            p.Add(":f_mmcode", string.Format("%{0}%", f_mmcode));

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CHK_DETAIL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        private string GetFilter(float f_u_price, float f_number, float f_amount, string f_mmcode)
        {
            string filter = string.Empty;

            if (f_u_price > 0)
            {
                filter += @"   and a.m_contprice >= :f_u_price";
            }
            if (f_number > 0)
            {
                filter += @"   and ABS(a.store_qtyc + a.store_qtym + a.store_qtys - a.chk_qty2 ) >= ABS(:f_number)";
            }
            if (f_amount > 0)
            {
                filter += @"   and ABS((a.store_qtyc + a.store_qtym + a.store_qtys - a.chk_qty2) * a.M_CONTPRICE) >= ABS(:f_amount)";
            }
            if (f_mmcode != "")
            {
                filter += @"   and a.mmcode like :f_mmcode";
            }

            return filter;
        }

        #endregion

        #region 暫存
        public CHK_MAST GetChkMast(string chk_no)
        {
            string sql = "select * from CHK_MAST where chk_no = :chk_no";
            return DBWork.Connection.QueryFirst<CHK_MAST>(sql, new { chk_no = chk_no }, DBWork.Transaction);
        }
        public int InsertChkdetail(CHK_DETAIL detail, string chk_wh_grade)
        {
            //string sql = @"insert into CHK_DETAIL
            //                      ( CHK_NO, MMCODE, MMNAME_C, MMNAME_E, M_PURUN,
            //                        M_CONTPRICE, WH_NO, STORE_LOC, LOC_NAME, MAT_CLASS,
            //                        M_STOREID, STORE_QTYC, CHK_QTY, CHK_REMARK, CHK_UID,
            //                        CHK_TIME, STATUS_INI,  
            //                        CREATE_DATE, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)
            //               values ( :chk_no, :mmcode, :mmname_c, :mmname_e, :m_purun,
            //                        :m_contprice, :wh_no, :store_loc, :loc_name, :mat_class,
            //                        :m_storeid, :store_qtyc, :chk_qty, :chk_remark, :chk_uid,
            //                        :chk_time, :status_ini, 
            //                        sysdate, :create_user, sysdate, :update_user, :update_ip)";
            string sql = @"insert into CHK_DETAIL
                                  ( CHK_NO, MMCODE, MMNAME_C, MMNAME_E, BASE_UNIT,
                                    M_CONTPRICE, WH_NO, STORE_LOC, MAT_CLASS,
                                    M_STOREID, STORE_QTYC, CHK_QTY, CHK_REMARK, CHK_UID,
                                    CHK_TIME, STATUS_INI,  
                                    CREATE_DATE, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)
                           select :chk_no as CHK_NO,
                                  a.mmcode,
                                  a.mmname_c,
                                  a.mmname_e,
                                  a.base_unit,
                                  a.m_contprice,
                                  a.wh_no,
                                  a.store_loc,
                                  a.mat_class,
                                  a.m_storeid,
                                  (case
                                    when store_qty_n is null
                                        then a.STORE_QTYC
                                    else a.store_qtyc
                                  end) as store_qtyc,
                                  '' as CHK_QTY,  
                                  '' as chk_remark,";
            if (chk_wh_grade == "1")
            {
                sql += "          a.chk_uid as CHK_UID,";
            }
            else
            {
                sql += "          '' as CHK_UID,";
            }
            sql += @"          '' as CHK_TIME,
                                  :status_ini as STATUS_INI,
                                  sysdate as CREATE_DATE,
                                  :create_user as CREATE_USER,
                                  sysdate as UPDATE_TIME,
                                  :update_user as CREATE_USER,
                                  :update_ip as UPDATE_IP
                             from CHK_DETAIL a, CHK_MAST b
                            where b.chk_no1 = :chk_no1
                              and b.chk_level = '2'
                              and a.chk_no = b.chk_no
                              and a.mmcode = :mmcode";
            return DBWork.Connection.Execute(sql, detail, DBWork.Transaction);
        }

        public int InsertChkdetailPH1S(CHK_DETAIL detail, bool is_distri)
        {
            string chkuidString = is_distri == true ? ":chk_uid" : "a.chk_uid";
            string sql = string.Format(@"insert into CHK_DETAIL
                                  ( CHK_NO, MMCODE, MMNAME_C, MMNAME_E, BASE_UNIT,
                                    M_CONTPRICE, WH_NO, STORE_LOC, MAT_CLASS,
                                    M_STOREID, STORE_QTYC, CHK_QTY, CHK_REMARK, CHK_UID,
                                    CHK_TIME, STATUS_INI,  
                                    CREATE_DATE, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)
                           select :chk_no as CHK_NO,
                                  a.mmcode,
                                  a.mmname_c,
                                  a.mmname_e,
                                  a.base_unit,
                                  a.m_contprice,
                                  a.wh_no,
                                  a.store_loc,
                                  a.mat_class,
                                  a.m_storeid,
                                  (select inv_qty
                                     from mi_wlocinv
                                    where wh_no = a.wh_no
                                      and mmcode = a.mmcode)  as store_qtyc,
                                  '' as CHK_QTY,  
                                  '' as chk_remark,
                                  {0} as CHK_UID,
                                  '' as CHK_TIME,
                                  :status_ini as STATUS_INI,
                                  sysdate as CREATE_DATE,
                                  :create_user as CREATE_USER,
                                  sysdate as UPDATE_TIME,
                                  :update_user as CREATE_USER,
                                  :update_ip as UPDATE_IP
                             from CHK_DETAIL a, CHK_MAST b
                            where b.chk_no1 = :chk_no1
                              and b.chk_level = '2'
                              and a.CHK_NO = b.chk_no
                              and a.mmcode = :mmcode",
                              chkuidString);
            return DBWork.Connection.Execute(sql, detail, DBWork.Transaction);
        }
        public int InsertChkdetailCE(CHK_DETAIL detail)
        {
            string sql = @"insert into CHK_DETAIL
                                  ( CHK_NO, MMCODE, MMNAME_C, MMNAME_E, BASE_UNIT,
                                    M_CONTPRICE, WH_NO, STORE_LOC, MAT_CLASS,
                                    M_STOREID, STORE_QTYC, CHK_QTY, CHK_REMARK, CHK_UID,
                                    CHK_TIME, STATUS_INI,  
                                    CREATE_DATE, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)
                           select :chk_no as CHK_NO,
                                  a.mmcode,
                                  a.mmname_c,
                                  a.mmname_e,
                                  a.base_unit,
                                  a.m_contprice,
                                  a.wh_no,
                                  a.store_loc,
                                  a.mat_class,
                                  a.m_storeid,
                                  (case
                                    when store_qty_n is null
                                        then a.STORE_QTYC
                                    else a.store_qty_n
                                  end) as store_qtyc,
                                  '' as CHK_QTY,  
                                  '' as chk_remark,
                                  '' as CHK_UID,
                                  '' as CHK_TIME,
                                  :status_ini as STATUS_INI,
                                  sysdate as CREATE_DATE,
                                  :create_user as CREATE_USER,
                                  sysdate as UPDATE_TIME,
                                  :update_user as CREATE_USER,
                                  :update_ip as UPDATE_IP
                             from CHK_DETAIL a, CHK_MAST b
                            where b.chk_no1 = :chk_no1
                              and b.chk_level = '2'
                              and a.CHK_NO = b.chk_no
                              and a.mmcode = :mmcode";
            return DBWork.Connection.Execute(sql, detail, DBWork.Transaction);
        }

        public int InsertChkdetailWard(CHK_DETAIL detail)
        {
            string sql = @"insert into CHK_DETAIL
                                  ( CHK_NO, MMCODE, MMNAME_C, MMNAME_E, BASE_UNIT,
                                    M_CONTPRICE, WH_NO, STORE_LOC, MAT_CLASS,
                                    M_STOREID, STORE_QTYC, CHK_QTY, CHK_REMARK, CHK_UID,
                                    CHK_TIME, STATUS_INI,  
                                    CREATE_DATE, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)
                           select :chk_no as CHK_NO,
                                  a.mmcode,
                                  a.mmname_c,
                                  a.mmname_e,
                                  a.base_unit,
                                  a.m_contprice,
                                  a.wh_no,
                                  a.store_loc,
                                  a.mat_class,
                                  a.m_storeid,
                                  (case
                                    when store_qty_n is null
                                        then a.STORE_QTYC
                                    else a.store_qty_n
                                  end) as store_qtyc,
                                  '' as CHK_QTY,  
                                  '' as chk_remark,
                                  '' as CHK_UID,
                                  '' as CHK_TIME,
                                  :status_ini as STATUS_INI,
                                  sysdate as CREATE_DATE,
                                  :create_user as CREATE_USER,
                                  sysdate as UPDATE_TIME,
                                  :update_user as CREATE_USER,
                                  :update_ip as UPDATE_IP
                             from CHK_DETAIL a, CHK_MAST b
                            where b.chk_no1 = :chk_no1
                              and b.chk_level = '2'
                              and a.CHK_NO = b.chk_no
                              and a.mmcode = :mmcode";
            return DBWork.Connection.Execute(sql, detail, DBWork.Transaction);
        }

        public int InsertChkDetailFromTemp(CHK_DETAIL detail)
        {
            string sql = @"insert into CHK_DETAIL
                                  ( CHK_NO, MMCODE, MMNAME_C, MMNAME_E, BASE_UNIT,
                                    M_CONTPRICE, WH_NO, STORE_LOC, MAT_CLASS,
                                    M_STOREID, STORE_QTYC, CHK_QTY, CHK_REMARK, CHK_UID,
                                    CHK_TIME, STATUS_INI,  
                                    CREATE_DATE, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)
                           select a.chk_no as chk_no, a.mmcode, b.mmname_c, b.mmname_e, b.base_unit,
                                  b.m_contprice, :wh_no as wh_no, a.store_loc, b.mat_class,
                                  b.m_storeid, a.inv_qty, '' as chk_qty, '' as chk_remark, '' as chk_uid, '' as chk_time, :status_ini as status_ini,
                                  sysdate as CREATE_DATE,
                                  :create_user as CREATE_USER,
                                  sysdate as UPDATE_TIME,
                                  :update_user as CREATE_USER,
                                  :update_ip as UPDATE_IP
                             from CHK_DETAIL_TEMP a, MI_MAST b
                            where a.chk_no = :chk_no
                              and a.mmcode = :mmcode
                              and b.mmcode = a.mmcode
                              and b.mmcode = a.mmcode";
            return DBWork.Connection.Execute(sql, detail, DBWork.Transaction);
        }

        public bool CheckDetailExists(string chk_no1, string mmcode)
        {
            string sql = @"select 1 from CHK_DETAILTOT
                            where chk_no = :chk_no1
                              and mmcode = :mmcode";
            return !(DBWork.Connection.ExecuteScalar(sql,
                                                     new { chk_no1 = chk_no1, mmcode = mmcode },
                                                     DBWork.Transaction) == null);
        }
        #endregion

        #region 分派盤點人元
        public IEnumerable<BC_WHCHKID> GetPickUsers(string wh_no)
        {
            string sql = @"select a.WH_CHKUID, 
                                  b.una as WH_CHKUID_NAME
                             from BC_WHCHKID a, UR_ID b
                            where a.WH_NO = :wh_no
                              and b.tuser = a.WH_CHKUID";
            return DBWork.Connection.Query<BC_WHCHKID>(sql, new { WH_NO = wh_no }, DBWork.Transaction);
        }

        public int UpdateMaster(string chk_no)
        {
            string sql = @"update CHK_MAST set chk_status = '1',
                                               chk_total = (select count(*) from chk_detail where chk_no = :chk_no)
                            where chk_no = :chk_no";
            return DBWork.Connection.Execute(sql, new { CHK_NO = chk_no }, DBWork.Transaction);
        }
        #endregion

        #region combo

        public class WhnoComboItem
        {
            public string WH_NO { get; set; }
            public string WH_NAME { get; set; }
            public string WH_KIND { get; set; }
            public string WH_GRADE { get; set; }
        }

        public IEnumerable<WhnoComboItem> GetWhnoCombo(string wh_userId)
        {
            string sql = @"select b.WH_NO, 
                                  (b.WH_NO || ' ' || b.WH_NAME) as WH_NAME, 
                                  b.WH_KIND, 
                                  b.WH_GRADE
                             from MI_WHID a, MI_WHMAST b
                            where a.WH_USERID = :WH_USERID
                              and b.WH_NO = a.WH_NO
                            order by b.WH_NO";
            return DBWork.Connection.Query<WhnoComboItem>(sql, new { WH_USERID = wh_userId }, DBWork.Transaction);
        }

        #endregion

        #region 科室病房衛材月盤手動新增項目 2019-12-13新增

        public IEnumerable<CHK_DETAIL> GetAddItemList(string chk_no, string wh_no, string chk_wh_grade, string chk_wh_kind, string chk_type, string chk_class, string chk_period, string mmcode, string chk_no1)
        {

            var p = new DynamicParameters();

            string sql = @"select a.mmcode, b.mmname_c, b.mmname_e, a.inv_qty, b.base_unit
                             from MI_WHINV a, MI_MAST b
                            where a.wh_no = :wh_no
                              and a.mmcode = b.mmcode";
            sql += GetConditionString(chk_wh_grade, chk_wh_kind, chk_type, chk_class, chk_period, wh_no);

            sql += @"         and not exists (select 1 from CHK_DETAILTOT 
                                               where chk_no = :chk_no1 and mmcode = a.mmcode)
                              and not exists (select 1 from CHK_DETAIL_TEMP
                                               where chk_no = :chk_no and mmcode = a.mmcode)
                              and not exists (select 1 from CHK_DETAIL
                                               where chk_no = :chk_no and mmcode = a.mmcode)";
            if (mmcode != string.Empty)
            {
                sql += @"     and a.mmcode like :mmcode";
            }

            p.Add(":wh_no", string.Format("{0}", wh_no));
            p.Add(":chk_no", string.Format("{0}", chk_no));
            p.Add(":mmcode", string.Format("%{0}%", mmcode));
            p.Add(":chk_no1", string.Format("{0}", chk_no1));

            return DBWork.PagingQuery<CHK_DETAIL>(sql, p, DBWork.Transaction);
        }
        private string GetConditionString(string chk_wh_grade, string chk_wh_kind, string chk_type, string chk_class, string chk_period, string wh_no)
        {
            string condition = string.Empty;

            if (chk_wh_kind == "1" || chk_wh_kind == "E" || chk_wh_kind == "C")     // 衛材
            {
                if (chk_type == "1")    // 庫備品
                {
                    condition = @"   and b.m_storeid = '1' 
                                     and b.m_contid <> '3'";
                }
                else if (chk_type == "0")                   // 非庫備品
                {
                    condition = string.Format(@"   and b.m_storeid = '0'
                                                   {0}
                                                   and b.m_contid <> '3'",
                                      chk_wh_grade == "1" ? "and a.inv_qty > 0" : string.Empty);
                }
                else if (chk_type == "3")
                {         // 小額採購
                    condition = string.Format(@"   and b.m_contid = '3'
                                                   {0}",
                                                   chk_wh_grade == "1" ? "and a.inv_qty > 0" : string.Empty);
                }

                if (chk_wh_grade == "1")    // 一級庫
                {
                    if (chk_class == "0X")
                    {
                        condition += @"  and b.mat_class in ('03', '04', '05', '06')";
                    }
                    else
                    {
                        condition += string.Format(@"  and b.mat_class = '{0}'", chk_class);
                    }
                }
                else
                {
                    condition += @"  and b.mat_class = '02'";
                }
            }
            else if (chk_wh_kind == "0")
            {    // 藥品
                switch (chk_type)
                {
                    case "1":           // 口服(月盤)
                        condition = @" and b.E_TAKEKIND IN ('11','12','13') and b.e_restrictcode not in ('1','2','3', '4')";
                        break;
                    case "2":           // 非口服(月盤)
                        condition = @" and b.E_TAKEKIND IN ('00','21','31','41','51') and b.e_restrictcode not in ('1','2','3', '4')";
                        break;
                    case "3":           // 1~3管制用藥(日盤)
                        condition = @" and b. E_RESTRICTCODE IN ('1','2','3')";
                        break;
                    case "4":           // 4級管制用藥(日盤)
                        condition = @" and b.E_RESTRICTCODE IN ('4')";
                        break;
                    case "7":           // 一般藥品
                        if (wh_no == "OR1" || wh_no == "ORC" || wh_no == "OPOR")    // 暫列手術室藥品庫(內湖4F、汀洲手術室、內湖門診手術室)
                        {
                            condition = @" and b.E_DRUGAPLTYPE != '1' and b.e_restrictcode not in ('1','2','3', '4') and c.ctdmdccode in ('0', '3', '4')";
                        }
                        else
                        {
                            condition = @" and b.e_restrictcode not in ('1','2','3', '4') and c.supply_whno != 'PH1S' and c.ctdmdccode = '0'";
                        }

                        break;
                    case "8":           // 大瓶點滴
                        if (wh_no == "OR1" || wh_no == "ORC" || wh_no == "OPOR")    // 暫列手術室藥品庫(內湖4F、汀洲手術室、內湖門診手術室)
                        {
                            condition = @" and b.E_DRUGAPLTYPE = '1' and b.e_restrictcode not in ('1','2','3', '4') and c.ctdmdccode in ('0', '3', '4')";
                        }
                        else
                        {
                            condition = @" and b.e_restrictcode not in ('1','2','3', '4') and c.supply_whno = 'PH1S' and c.ctdmdccode = '0'";
                        }
                        break;

                };

                if (chk_period == "S")
                {
                    condition += @"    and b.e_invflag = 'Y'";
                }
            }

            return condition;
        }

        public int AddItem(string chk_no, string wh_no, string mmcode, string userid, string ip)
        {
            string sql = @"insert into CHK_DETAIL_TEMP 
                                        (chk_no, wh_no, mmcode, mmname_c, mmname_e, 
                                         base_unit, inv_qty, store_loc, chk_uid)
                           select :chk_no as chk_no,
                                  :wh_no as wh_no,
                                  a.mmcode, a.mmname_c, a.mmname_e, a.base_unit,
                                  b.inv_qty, 
                                  'TEMP' as store_loc,
                                  '' as chk_uid
                             from MI_MAST a, MI_WHINV b
                            where a.mmcode = :mmcode
                              and b.mmcode = a.mmcode
                              and b.wh_no = :wh_no
";
            return DBWork.Connection.Execute(sql, new { chk_no = chk_no, wh_no = wh_no, mmcode = mmcode, userid = userid, ip = ip }, DBWork.Transaction);
        }


        #endregion

        #region 中央庫房產生複盤單自動將數量不符之品項加入盤點 2020-01-30新增
        public IEnumerable<CHK_DETAILTOT> GetNotMatchDetails(string chk_no)
        {
            string sql = @"select * from CHK_DETAILTOT where chk_no = :chk_no and status_tot = '2' and ABS(gap_t) >= 1";
            return DBWork.Connection.Query<CHK_DETAILTOT>(sql, new { chk_no = chk_no }, DBWork.Transaction);
        }
        #endregion
    }
}