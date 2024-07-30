using Dapper;
using JCLib.DB;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Repository.C
{
    public class CHK_ADD_ITEM {
        public string WH_NO { get; set; }
        public string WH_KIND { get; set; }
        public string CHK_TYPE { get; set; }
        public string CHK_TYPE_NAME { get; set; }
        public string CHK_LEVEL { get; set; }
        public string CHK_CLASS { get; set; }
        public string CHK_YM { get; set; }
        public string CHK_NO { get; set; }
        public string MMCODE { get; set; }
        public string M_STOREID { get; set; }
        public string M_CONTID { get; set; }
        public string MMCODE_COUNT { get; set; }
        public string MMCODE_STRING { get; set; }
        public IEnumerable<CHK_ADD_ITEM> MMCODE_LIST { get; set; }
        public string CHK_STATUS { get; set; }
        public string CHK_STATUS_NAME { get; set; }
        public string RESULT { get; set; }
    }
    public class CE0002Repository : JCLib.Mvc.BaseRepository
    {
        public CE0002Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public class Sorter
        {
            public string property { get; set; }
            public string direction { get; set; }
        }

        public IEnumerable<CHK_MAST> GetMasterAll(string wh_no, string chk_ym, string keeper, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = @"select a.CHK_NO,
                                  a.CHK_YM,
                                  a.CHK_WH_NO, 
                                  a.CHK_WH_GRADE,
                                  a.CHK_WH_KIND,
                                  a.CHK_CLASS,
                                  a.CHK_PERIOD,
                                  a.CHK_TYPE,
                                  a.CHK_LEVEL,
                                  a.CHK_NUM,
                                  a.CHK_TOTAL,
                                  a.CHK_STATUS,
                                  (select g.una from UR_ID g where g.tuser = a.CHK_KEEPER) as CHK_KEEPER,
                                  a.CHK_NO1,
                                  (select b.WH_NO || ' ' ||b.WH_NAME from MI_WHMAST b where b.WH_NO = a.CHK_WH_NO) as WH_NAME,
                                  (select c.DATA_VALUE || ' ' || c.DATA_DESC 
                                     from PARAM_D c
                                    where c.GRP_CODE = 'CHK_MAST' 
                                      and c.DATA_NAME = 'CHK_WH_KIND'
                                      and c.DATA_VALUE = a.CHK_WH_KIND) as WH_KIND_NAME,
                                  (select c.DATA_VALUE || ' ' || c.DATA_DESC 
                                     from PARAM_D c
                                    where c.GRP_CODE = 'CHK_MAST' 
                                      and c.DATA_NAME = 'CHK_CLASS'
                                      and c.DATA_VALUE = a.CHK_CLASS) as CHK_CLASS_NAME,
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
                                  a.create_date, a.create_user,
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
                            where a.chk_level = 1";

            if (wh_no != string.Empty)
            {
                sql += "  and a.CHK_WH_NO = :wh_no";
            }
            if (chk_ym != string.Empty)
            {
                sql += string.Format("  and a.CHK_YM like '{0}%'", chk_ym);
            }

            p.Add(":wh_no", string.Format("{0}", wh_no));
            p.Add(":chk_ym", string.Format("{0}", chk_ym));
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

        #region 新增
        public int InsertChkMast(CHK_MAST mast)
        {
            string sql = @"insert into CHK_MAST
                                  (CHK_NO, CHK_YM, CHK_WH_NO, CHK_WH_GRADE, CHK_WH_KIND, CHK_CLASS,
                                  CHK_PERIOD, CHK_TYPE, CHK_LEVEL, CHK_NUM, CHK_TOTAL, CHK_STATUS, CHK_KEEPER,
                                  CREATE_DATE, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, CHK_NO1)
                           values (:chk_no, :chk_ym, :chk_wh_no, :chk_wh_grade, :chk_wh_kind, : chk_class,
                                   :chk_period, :chk_type, '1', 0, 0, 0, :chk_keeper,
                                   sysdate, :create_user, sysdate, :update_user, :update_ip, :chk_no
                                  )";

            return DBWork.Connection.Execute(sql, mast, DBWork.Transaction);
        }

        public bool CheckExists(string chk_wh_no, string chk_ym, string chk_period, string chk_type, string chk_class)
        {
            string sql = @"select 1 from CHK_MAST
                            WHERE chk_wh_no = :chk_wh_no 
                              AND chk_ym = :chk_ym 
                              AND chk_period = :chk_period 
                              AND chk_type = :chk_type"; // TRUNC(a.EXP_DATE, 'DD') TWN_TODATE(:exp_date)
            if (chk_class != string.Empty)
            {
                sql += "      and chk_class = :chk_class";
            }
            return !(DBWork.Connection.ExecuteScalar(sql,
                                                     new
                                                     {
                                                         chk_wh_no = chk_wh_no,
                                                         chk_ym = chk_ym,
                                                         chk_period = chk_period,
                                                         chk_type = chk_type,
                                                         chk_class = chk_class
                                                     },
                                                     DBWork.Transaction) == null);
        }
        public string CheckPreStatus(string chk_wh_no, string chk_ym, string chk_period, string chk_type, string chk_class) {
            string sql = @"select nvl(a.chk_status, '3') from CHK_MAST a
                            WHERE a.chk_wh_no = :chk_wh_no 
                              AND a.chk_ym = :chk_ym 
                              AND a.chk_period = :chk_period 
                              AND a.chk_type = :chk_type
                              and a.chk_level = (select nvl(max(chk_level), '1') from chk_mast where  chk_no1 = a.chk_no1)"; // TRUNC(a.EXP_DATE, 'DD') TWN_TODATE(:exp_date)
            if (chk_class != string.Empty)
            {
                sql += "      and chk_class = :chk_class";
            }
            string temp = DBWork.Connection.QueryFirstOrDefault<string>(sql, 
                                                         new
                                                         {
                                                             chk_wh_no = chk_wh_no,
                                                             chk_ym = chk_ym,
                                                             chk_period = chk_period,
                                                             chk_type = chk_type,
                                                             chk_class = chk_class
                                                         }, DBWork.Transaction);
            return temp == null ? "3" : temp;
        }
        public string GetCurrentSeq(string wh_no, string ym)
        {
            string sobstringIndex = (wh_no.Length + ym.Length + 3).ToString();
            string queryIndex = (wh_no.Length + ym.Length).ToString();

            string sql = string.Format(@"select NVL(max(to_number(substr(CHK_NO ,{0},3)))+1, 1)
                             from CHK_MAST 
                            where substr(chk_no,1,{1}) = :chk_no
                              and chk_wh_no = :wh_no", sobstringIndex, queryIndex);

            string result = DBWork.Connection.QueryFirst<string>(sql,
                                                         new
                                                         {
                                                             chk_no = string.Format("{0}{1}", wh_no, ym),
                                                             wh_no = wh_no
                                                         }, DBWork.Transaction);
            return result.PadLeft(3, '0');

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

        public bool CheckDetailEntry(string chk_no) {
            string sql = @"select 1 from CHK_DETAIL
                            where chk_no = :chk_no
                              and chk_qty is not null";
            return DBWork.Connection.ExecuteScalar(sql, new { chk_no = chk_no }, DBWork.Transaction) == null;
        }
        public bool CheckG2DetailEntry(string chk_no)
        {
            string sql = @"select 1 from CHK_G2_DETAIL
                            where chk_no = :chk_no
                              and chk_qty is not null";
            return DBWork.Connection.ExecuteScalar(sql, new { chk_no = chk_no }, DBWork.Transaction) == null;
        }
        public int DeleteG2Whinv(string chk_no) {
            string sql = @"delete from CHK_G2_WHINV where chk_no = :chk_no";
            return DBWork.Connection.Execute(sql, new { CHK_NO = chk_no }, DBWork.Transaction);
        }
        public int DeleteG2Detail(string chk_no, string excludes)
        {
            string sql = @"delete from CHK_G2_DETAIL where chk_no = :chk_no";
            if (excludes != string.Empty) {
                sql += string.Format(@" and chk_uid not in ({0})", excludes);
            }

            return DBWork.Connection.Execute(sql, new { CHK_NO = chk_no}, DBWork.Transaction);
        }
        public int DeleteG2Updn(string chk_no, string excludes)
        {
            string sql = @"delete from CHK_GRADE2_UPDN where chk_no = :chk_no";
            if (excludes != string.Empty) {
                sql += string.Format(@" and chk_uid not in ({0})", excludes);
            }
            return DBWork.Connection.Execute(sql, new { CHK_NO = chk_no }, DBWork.Transaction);
        }
        public int DeleteNouid(string chk_no, string excludes)
        {
            string sql = @"delete from CHK_NOUID where chk_no = :chk_no";
            if (excludes != string.Empty)
            {
                sql += string.Format(@" and chk_uid not in ({0})", excludes);
            }
            return DBWork.Connection.Execute(sql, new { CHK_NO = chk_no }, DBWork.Transaction);
        }

        public int UpdatePreMaster(string chk_no1, string preLevel, string update_user, string update_ip) {
            string sql = @"update CHK_MAST 
                              set chk_status = '3', update_time = sysdate, update_user = :update_user, update_ip = :update_ip
                            where chk_no1 = :chk_no1 and chk_level = :preLevel";
            return DBWork.Connection.Execute(sql, new { chk_no1 = chk_no1, preLevel =preLevel, update_user = update_user, update_ip = update_ip }, DBWork.Transaction);
        }
        #endregion

        #region 盤點明細主畫面

        public IEnumerable<CHK_DETAIL> GetIncludeDetails(string chk_no, int page_index, int page_size, string sorters, string currentMmcodes)
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
            //and mmcode not in ({0})", currentMmcodes);
            if (currentMmcodes != string.Empty)
            {
                sql += string.Format("    and mmcode not in ({0})", currentMmcodes);
            }

            p.Add(":chk_no", string.Format("{0}", chk_no));
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CHK_DETAIL>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<CHK_DETAIL> GetExcludeDetails(string wh_no, string chk_wh_grade, string chk_wh_kind, string chk_type, string chk_no, string chk_class, string chk_period, float f_u_price, float f_number, float f_amount, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            #region old sql ~2019-06-21
            //string sql = @"select b.mmcode, b.mmname_c, b.mmname_e,
            //                      b.m_purun, b.m_contprice, a.wh_no,
            //                      a.store_loc, 
            //                        NVL(a.loc_name,' ') as loc_name, b.mat_class,
            //                      b.m_storeid, b.e_takekind, b.e_restrictcode,
            //                      a.inv_qty as STORE_QTYC, b.base_unit,";
            #endregion

            string sql = string.Format(@"select distinct :wh_no as wh_no,
                                  b.mmcode, b.mmname_c, b.mmname_e, b.base_unit,
                                  (select sum(inv_qty) from MI_WHINV
                                    where wh_no = a.wh_no
                                      and mmcode = a.mmcode
                                    group by wh_no, mmcode) as INV_QTY,
                                  0 as CHK_QTY,   
                                  0 as STATUS_INI,
                                  '0 準備' as STATUS_INI_NAME,");
            if (chk_wh_grade == "1")
            {
                sql += @"  
                                (select listagg(store_loc, ',')
                                   within group (order by store_loc)
                                     from mi_wlocinv
                                    where wh_no = a.wh_no
                                      and mmcode = a.mmcode) as STORE_LOC, 
                                  (select listagg(store_loc, '<br>')
                                   within group (order by store_loc)
                                     from mi_wlocinv
                                    where wh_no = a.wh_no
                                      and mmcode = a.mmcode) as STORE_LOC_NAME,";
            }
            else
            {
                sql += " ''  as store_loc, '' as store_loc_name,";
            }

            if (chk_wh_grade == "1" && chk_wh_kind == "1")
            {
                sql += @" (select c.managerid
                             from BC_ITMANAGER c
                            where c.wh_no = a.wh_no 
                              and c.mmcode = b.mmcode) as CHK_UID,
                          (select f.una
                             from BC_ITMANAGER e, UR_ID f
                            where e.wh_no = a.wh_no 
                              and e.mmcode = b.mmcode
                              and f.tuser = e.managerid) as CHK_UID_NAME";
            }
            else
            {
                sql += @" ' ' as MANAGERID, ' ' as MANAGERID_NAME";
            }
            if (chk_wh_grade == "1" && chk_wh_kind == "1") {    //一級庫衛材庫
                sql += @"   from MI_WHINV a, MI_MAST b
                                where a.wh_no = :wh_no
                                  and a.mmcode = b.mmcode
                                  and exists (select 1 from MI_WLOCINV where wh_no = a.wh_no and mmcode = a.mmcode)";
            } else if (chk_wh_grade == "1" && chk_wh_kind == "0") {     //一級庫藥品庫
                sql += @"   from MI_WHINV a, MI_MAST b
                                where a.wh_no = :wh_no
                                  and a.mmcode = b.mmcode
                                  and a.inv_qty > 0";
            }
            // 衛星庫房藥品庫、能設通信
            else if (chk_wh_kind == "0" || chk_wh_kind == "E" || chk_wh_kind == "C")
            {
                sql += @"        from MI_WHINV a, MI_MAST b, MI_WINVCTL c
                                where a.wh_no = :wh_no
                                  and a.mmcode = b.mmcode
                                  and c.wh_no = a.wh_no
                                  and c.mmcode = a.mmcode
                                  and b.e_orderdcflag = 'N'";
            }
            else {  //衛星庫房衛材庫
                sql += @"        from MI_WHINV a, MI_MAST b, MI_WINVMON c
                                where a.wh_no = :wh_no
                                  and a.mmcode = b.mmcode
                                  and c.wh_no = a.wh_no
                                  and c.mmcode = a.mmcode
                                  and c.data_ym = :preym
                                  and not (a.APL_INQTY = 0 and c.INV_QTY = 0 )";
            }

            sql += GetConditionString(chk_wh_grade, chk_wh_kind, chk_type, chk_class, chk_period, wh_no);
            
            sql += @"         and not exists ( select 'x' from CHK_DETAIL_TEMP 
                                                where chk_no = :chk_no 
                                                  and mmcode = a.mmcode)";
            

            p.Add(":wh_no", string.Format("{0}", wh_no));
            p.Add(":chk_wh_grade", string.Format("{0}", chk_wh_grade));
            p.Add(":chk_wh_kind", string.Format("{0}", chk_wh_kind));
            p.Add(":chk_type", string.Format("{0}", chk_type));
            p.Add(":chk_no", string.Format("{0}", chk_no));
            p.Add(":f_u_price", string.Format("{0}", f_u_price));
            p.Add(":f_number", string.Format("{0}", f_number));
            p.Add(":f_amount", string.Format("{0}", f_amount));
            p.Add(":preym", string.Format("{0}", GetPreym()));

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CHK_DETAIL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        private string GetPreym()
        {
            int m;
            string yyy;
            if (DateTime.Now.Month - 1 == 0)
            {
                yyy = (DateTime.Now.Year - 1911 - 1).ToString();
                m = 12;
            }
            else
            {
                yyy = (DateTime.Now.Year - 1911).ToString();
                m = DateTime.Now.Month - 1;
            }

            string mm = m > 9 ? m.ToString() : m.ToString().PadLeft(2, '0');
            
            return string.Format("{0}{1}", yyy, mm);
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
                else {
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
                            condition = @" and b.e_restrictcode not in ('1','2','3', '4') 
                                           and (c.supply_whno != 'PH1S'  or (c.supply_whno is null and nvl(b.e_drugapltype, '0') != '1'))
                                           and c.ctdmdccode = '0'";
                        }

                        break;
                    case "8":           // 大瓶點滴
                        if (wh_no == "OR1" || wh_no == "ORC" || wh_no == "OPOR")    // 暫列手術室藥品庫(內湖4F、汀洲手術室、內湖門診手術室)
                        {
                            condition = @" and b.E_DRUGAPLTYPE = '1' and b.e_restrictcode not in ('1','2','3', '4') and c.ctdmdccode in ('0', '3', '4')";
                        }
                        else
                        {
                            condition = @" and b.e_restrictcode not in ('1','2','3', '4') 
                                           and (c.supply_whno = 'PH1S'  or (c.supply_whno is null and nvl(b.e_drugapltype, '0')='1'))
                                           and c.ctdmdccode = '0'";
                        }
                        break;

                };

                if (chk_period == "S")
                {
                    condition += @"    and b.e_invflag = 'Y'";
                }

                if (wh_no == "PH1S") {
                    condition += @" and substr(b.mmcode,1,3) > '004' 
                                    and substr(b.mmcode,1,3) < '008'";
                }
            }

            return condition;
        }

        public int InsertDetailTemp(CHK_DETAIL_TEMP detailTemp)
        {
            string sql = @"insert into CHK_DETAIL_TEMP
                                  (CHK_NO, WH_NO, MMCODE, MMNAME_C, MMNAME_E, BASE_UNIT,
                                   INV_QTY, CHK_QTY, STORE_LOC, STORE_LOC_NAME, CHK_UID)
                           values (:chk_no, :wh_no, :mmcode, :mmname_c, :mmname_e, :base_unit,
                                   :inv_qty, :chk_qty, :store_loc, :store_loc_name, :chk_uid
                                  )";

            return DBWork.Connection.Execute(sql, detailTemp, DBWork.Transaction);
        }
        public int InsertDetailTempGrade2(CHK_DETAIL_TEMP detailTemp)
        {
            string sql = @"insert into CHK_DETAIL_TEMP
                                  (CHK_NO, WH_NO, MMCODE, MMNAME_C, MMNAME_E, BASE_UNIT,
                                   INV_QTY, CHK_QTY, STORE_LOC, STORE_LOC_NAME, CHK_UID)
                           values (:chk_no, :wh_no, :mmcode, :mmname_c, :mmname_e, :base_unit,
                                   :inv_qty, :chk_qty, :store_loc, :store_loc_name, :chk_uid
                                  )";

            return DBWork.Connection.Execute(sql, detailTemp, DBWork.Transaction);
        }

        public int DeleteDetailTempAll(string chkno)
        {
            string sql = @"delete from CHK_DETAIL_TEMP
                            where chk_no = :CHK_NO";

            return DBWork.Connection.Execute(sql, new { CHK_NO = chkno }, DBWork.Transaction);
        }
        public int DeleteDetailTemp(CHK_DETAIL_TEMP temp)
        {
            string sql = @"delete from CHK_DETAIL_TEMP
                            where chk_no = :CHK_NO
                              and wh_no = :WH_NO
                              and MMCODE = :MMCODE";
            return DBWork.Connection.Execute(sql, temp, DBWork.Transaction);
        }

        public IEnumerable<CHK_DETAIL_TEMP> DetailTempAll(string chkno, string sorter)
        {
            string sql = @"select a.*,
                                  (select una from UR_ID where tuser = a.chk_uid) as chk_uid_name,
                                   0 as STATUS_INI,
                                  '0 準備' as STATUS_INI_NAME
                             from CHK_DETAIL_TEMP a where a.chk_no = :chk_no";
            if (sorter != string.Empty)
            {
                return DBWork.PagingQuery<CHK_DETAIL_TEMP>(sql, new { chk_no = chkno }, DBWork.Transaction);
            }

            return DBWork.Connection.Query<CHK_DETAIL_TEMP>(sql, new { chk_no = chkno }, DBWork.Transaction);
        }
        public IEnumerable<CHK_DETAIL_TEMP> DetailTempOrder(string chkno, Sorter sorter)
        {
            string sql = string.Format(@"select * from CHK_DETAIL_TEMP where chk_no = :chk_no
                                          order by {0} ", sorter.property);

            return DBWork.Connection.Query<CHK_DETAIL_TEMP>(sql, new { chk_no = chkno }, DBWork.Transaction);
        }

        public IEnumerable<CHK_DETAIL> GetExcludeDetailsANE(string wh_no, string chk_type, string chk_no) {
            var p = new DynamicParameters();

            string sql = string.Format(@"select distinct :wh_no as wh_no,
                                  b.mmcode, b.mmname_c, b.mmname_e, b.base_unit,
                                  (select sum(inv_qty) from MI_WHINV
                                    where wh_no = a.wh_no
                                      and mmcode = a.mmcode
                                    group by wh_no, mmcode) as INV_QTY,
                                  0 as CHK_QTY,   
                                  0 as STATUS_INI,
                                  '0 準備' as STATUS_INI_NAME,
                                  ''  as store_loc, '' as store_loc_name,
                                  ' ' as MANAGERID, ' ' as MANAGERID_NAME
                             from MI_WHINV a, MI_MAST b, MI_WINVCTL c
                            where a.wh_no = :wh_no
                              and a.mmcode = b.mmcode 
                              and c.wh_no = a.wh_no
                              and c.mmcode = a.mmcode 
                              and b.e_orderdcflag = 'N'
                              and c.ctdmdccode = '0'");

            switch (chk_type) {
                case "5":
                    sql += @" and substr(b.mmcode, 1,3) <> '006'
                              and b.e_restrictcode not in ('1','2','3', '4')";
                    break;
                case "6":
                    sql += @" and substr(b.mmcode, 1,3) = '006'
                              and b.e_restrictcode not in ('1','2','3', '4')";
                    break;
                case "3":
                    sql += @" and b.e_restrictcode in ('1','2','3')";
                    break;
                case "4":
                    sql += @" and b.e_restrictcode in ('4')";
                    break;
            }

            sql += @"         and not exists ( select 'x' from CHK_DETAIL_TEMP 
                                                where chk_no = :chk_no 
                                                  and mmcode = a.mmcode)";

            p.Add(":wh_no", string.Format("{0}", wh_no));
            p.Add(":chk_type", string.Format("{0}", chk_type));
            p.Add(":chk_no", string.Format("{0}", chk_no));

            return DBWork.PagingQuery<CHK_DETAIL>(sql, p, DBWork.Transaction);
        }

        #endregion

        #region 暫存
        public string GetChkWhGrade(string chk_no)
        {
            string sql = @"select chk_wh_grade from chk_mast where chk_no = :chk_no";
            return DBWork.Connection.QueryFirst<string>(sql, new { chk_no = chk_no }, DBWork.Transaction);
        }
        public string GetChkWhKind(string chk_no)
        {
            string sql = @"select chk_wh_kind from chk_mast where chk_no = :chk_no";
            return DBWork.Connection.QueryFirst<string>(sql, new { chk_no = chk_no }, DBWork.Transaction);
        }
        public int InsertChkdetail(CHK_DETAIL detail, string chk_wh_grade, string chk_wh_kind)
        {
            string sql = string.Empty;

            if (chk_wh_grade == "1")
            {
               
                if (chk_wh_kind == "0")
                {
                    sql = @"insert into CHK_DETAIL
                                  ( CHK_NO, MMCODE, MMNAME_C, MMNAME_E, BASE_UNIT,
                                    M_CONTPRICE, WH_NO, STORE_LOC, LOC_NAME, MAT_CLASS,
                                    M_STOREID, STORE_QTYC, CHK_QTY, CHK_REMARK, CHK_UID,
                                    CHK_TIME, STATUS_INI,  
                                    CREATE_DATE, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)
                           select distinct CHK_NO, MMCODE, MMNAME_C, MMNAME_E, BASE_UNIT,
                                    M_CONTPRICE, WH_NO, STORE_LOC, LOC_NAME, MAT_CLASS,
                                    M_STOREID, STORE_QTYC, CHK_QTY, CHK_REMARK, CHK_UID,
                                    CHK_TIME, STATUS_INI,  
                                    CREATE_DATE, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP
                           from (
                            select :chk_no as CHK_NO,
                                  b.mmcode as mmcode,
                                  b.mmname_c as mmname_c,
                                  b.mmname_e as mmname_e,
                                  b.base_unit as base_unit,
                                  b.m_contprice,
                                  :wh_no as wh_no,
                                  nvl((select store_loc from MI_WLOCINV where wh_no =:wh_no and mmcode = :mmcode and rownum = 1),'暫存區') as STORE_LOC,            
                                  '' as LOC_NAME,
                                  b.mat_class,
                                  b.m_storeid,
                                  a.inv_qty as STORE_QTYC,
                                  '' as CHK_QTY,  
                                  '' as chk_remark,
                                  :chk_uid as CHK_UID,
                                  '' as CHK_TIME,
                                  :status_ini as STATUS_INI,
                                  sysdate as CREATE_DATE,
                                  :create_user as CREATE_USER,
                                  sysdate as UPDATE_TIME,
                                  :update_user as UPDATE_USER,
                                  :update_ip as UPDATE_IP
                             from MI_WHINV a, MI_MAST b
                            where a.wh_no = :wh_no
                              and a.mmcode = :mmcode
                              and b.mmcode = a.mmcode
                              and not (a.inv_qty = 0 and b.m_agenno = '000')
                        union
                           select :chk_no as CHK_NO,
                                  b.mmcode as mmcode,
                                  b.mmname_c as mmname_c,
                                  b.mmname_e as mmname_e,
                                  b.base_unit as base_unit,
                                  b.m_contprice,
                                  :wh_no as wh_no,
                                  nvl((select store_loc from MI_WLOCINV where wh_no =:wh_no and mmcode = :mmcode and rownum = 1),'暫存區') as STORE_LOC,            
                                  '' as LOC_NAME,
                                  b.mat_class,
                                  b.m_storeid,
                                  a.inv_qty as STORE_QTYC,
                                  '' as CHK_QTY,  
                                  '' as chk_remark,
                                  :chk_uid as CHK_UID,
                                  '' as CHK_TIME,
                                  :status_ini as STATUS_INI,
                                  sysdate as CREATE_DATE,
                                  :create_user as CREATE_USER,
                                  sysdate as UPDATE_TIME,
                                  :update_user as UPDATE_USER,
                                  :update_ip as UPDATE_IP
                             from MI_WHINV a, MI_MAST b
                            where a.wh_no = :wh_no
                              and a.mmcode = :mmcode
                              and b.mmcode = a.mmcode
                              and exists (select 1 from MI_MAST where mmcode = '004'||substr(a.mmcode, 4 ,10) and e_orderdcflag = 'N')
                              and b.e_orderdcflag = 'Y'
                              and exists (select 1 from MI_WHINV where wh_no = 'PH1X' and mmcode = '004'||substr(a.mmcode, 4 ,10) and inv_qty > 0 )
                           )";
                }
                else {
                    sql = string.Format(@"insert into CHK_DETAIL
                                  ( CHK_NO, MMCODE, MMNAME_C, MMNAME_E, BASE_UNIT,
                                    M_CONTPRICE, WH_NO, STORE_LOC, MAT_CLASS,
                                    M_STOREID, STORE_QTYC, CHK_QTY, CHK_REMARK, CHK_UID,
                                    CHK_TIME, STATUS_INI,  
                                    CREATE_DATE, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)
                           select :chk_no as CHK_NO,
                                  b.mmcode,
                                  b.mmname_c,
                                  b.mmname_e,
                                  b.base_unit,
                                  b.m_contprice,
                                  a.wh_no,
                                  a.store_loc,         
                                  b.mat_class,
                                  b.m_storeid,
                                  (select sum(inv_qty) from MI_WHINV
                                    where wh_no  in (select wh_no from MI_WHMAST 
                                                      where wh_kind = '1' and inid = '560000' and wh_grade in ('1', '5'))
                                      and mmcode = a.mmcode
                                    group by mmcode) as STORE_QTYC,
                                  '' as CHK_QTY,  
                                  '' as chk_remark,
                                  :chk_uid as CHK_UID,
                                  '' as CHK_TIME,
                                  :status_ini as STATUS_INI,
                                  sysdate as CREATE_DATE,
                                  :create_user as CREATE_USER,
                                  sysdate as UPDATE_TIME,
                                  :update_user as UPDATE_USER,
                                  :update_ip as UPDATE_IP
                             from MI_WLOCINV a, MI_MAST b
                            where a.wh_no = :wh_no
                              and a.mmcode = :mmcode
                              and b.mmcode = a.mmcode");
                }
            }
            else
            {
                sql = @"insert into CHK_DETAIL
                                  ( CHK_NO, MMCODE, MMNAME_C, MMNAME_E, BASE_UNIT,
                                    M_CONTPRICE, WH_NO, STORE_LOC, LOC_NAME, MAT_CLASS,
                                    M_STOREID, STORE_QTYC, CHK_QTY, CHK_REMARK, CHK_UID,
                                    CHK_TIME, STATUS_INI,  
                                    CREATE_DATE, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)
                           select :chk_no as CHK_NO,
                                  b.mmcode as mmcode,
                                  b.mmname_c as mmname_c,
                                  b.mmname_e as mmname_e,
                                  b.base_unit as base_unit,
                                  b.m_contprice,
                                  :wh_no as wh_no,
                                  'TEMP' as STORE_LOC,            
                                  'TEMP' as LOC_NAME,
                                  b.mat_class,
                                  b.m_storeid,
                                  a.inv_qty as STORE_QTYC,
                                  '' as CHK_QTY,  
                                  '' as chk_remark,
                                  :chk_uid as CHK_UID,
                                  '' as CHK_TIME,
                                  :status_ini as STATUS_INI,
                                  sysdate as CREATE_DATE,
                                  :create_user as CREATE_USER,
                                  sysdate as UPDATE_TIME,
                                  :update_user as UPDATE_USER,
                                  :update_ip as UPDATE_IP
                             from MI_WHINV a, MI_MAST b
                            where a.wh_no = :wh_no
                              and a.mmcode = :mmcode
                              and b.mmcode = a.mmcode";
            }


            return DBWork.Connection.Execute(sql, detail, DBWork.Transaction);
        }
        public int InsertChkdetailCE(CHK_DETAIL detail)
        {
            string sql = string.Empty;

            sql = @"insert into CHK_DETAIL
                                  ( CHK_NO, MMCODE, MMNAME_C, MMNAME_E, BASE_UNIT,
                                    M_CONTPRICE, WH_NO, STORE_LOC, LOC_NAME, MAT_CLASS,
                                    M_STOREID, STORE_QTYC, CHK_QTY, CHK_REMARK, CHK_UID,
                                    CHK_TIME, STATUS_INI,  
                                    CREATE_DATE, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)
                           select :chk_no as CHK_NO,
                                  b.mmcode as mmcode,
                                  b.mmname_c as mmname_c,
                                  b.mmname_e as mmname_e,
                                  b.base_unit as base_unit,
                                  b.m_contprice,
                                  :wh_no as wh_no,
                                  'TEMP' as STORE_LOC,            
                                  'TEMP' as LOC_NAME,
                                  b.mat_class,
                                  b.m_storeid,
                                  (select sum(inv_qty) from MI_WHINV
                                    where wh_no  in (select wh_no from MI_WHMAST where wh_kind = :wh_kind)
                                      and mmcode = a.mmcode
                                    group by mmcode) as STORE_QTYC,
                                  '' as CHK_QTY,  
                                  '' as chk_remark,
                                  :chk_uid as CHK_UID,
                                  '' as CHK_TIME,
                                  :status_ini as STATUS_INI,
                                  sysdate as CREATE_DATE,
                                  :create_user as CREATE_USER,
                                  sysdate as UPDATE_TIME,
                                  :update_user as UPDATE_USER,
                                  :update_ip as UPDATE_IP
                             from MI_WHINV a, MI_MAST b
                            where a.wh_no = :wh_no
                              and a.mmcode = :mmcode
                              and b.mmcode = a.mmcode";
            return DBWork.Connection.Execute(sql, detail, DBWork.Transaction);
        }

        public int GetDetailCount(string chkno)
        {
            string sql = @"select count(*) from CHK_DETAIL
                            where chk_no = :CHK_NO";
            return DBWork.Connection.QueryFirst<int>(sql, new { CHK_NO = chkno }, DBWork.Transaction);
        }

        public int InsertChknouid(CHK_NOUID nouid) {
            string sql = @"insert into CHK_NOUID 
                                  (chk_no, chk_uid, 
                                   create_date, create_user, update_time, update_user, update_ip)
                           values (:chk_no, :chk_uid, sysdate, :create_user, sysdate, :update_user, :update_ip)";
            return DBWork.Connection.Execute(sql, nouid, DBWork.Transaction);
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

        #region 全盤
        public IEnumerable<CHK_DETAIL> GetAllinDetails(string wh_no, string chk_wh_grade, string chk_wh_kind, string chk_type, string chk_no, string chk_class, string chk_period)
        {
            var p = new DynamicParameters();

            string storelocString = string.Empty;//"a.store_loc,";
            var preym = GetPreym();

            string sql = string.Format(@"select b.mmcode, b.mmname_c, b.mmname_e,
                                                b.base_unit, b.m_contprice, {0},
                                                b.mat_class,
                                                b.m_storeid, b.e_takekind, b.e_restrictcode,
                                                a.inv_qty as STORE_QTYC, b.base_unit,
                                                0 as STATUS_INI,
                                                '0 準備' as STATUS_INI_NAME,",
                                                chk_wh_kind == "1" && chk_wh_grade == "1" 
                                                ? @"(select wh_no from MI_WHMAST 
                                                      where wh_kind = '1' and wh_grade = '1' and cancel_id = 'N') as wh_no"
                                                :" a.wh_no");
            if (chk_wh_kind == "1" && chk_wh_grade == "1")
            {
                sql += @" c.managerid as CHK_UID,
                          (select una from UR_ID
                            where tuser = c.managerid) as CHK_UID_NAME";
            }
            else
            {
                sql += @" ' ' as MANAGERID, ' ' as MANAGERID_NAME";
            }

            // 中央庫房
            if (chk_wh_grade == "1" && chk_wh_kind == "1")
            {
                sql += string.Format(@"        from MI_WHINV a, MI_MAST b , BC_ITMANAGER c
                                              where a.wh_no in (select wh_no from MI_WHMAST 
                                                                 where wh_kind = '1' and wh_grade in ('1', '5') and cancel_id = 'N')
                                                and a.mmcode = b.mmcode
                                                and c.wh_no = (select wh_no from MI_WHMAST 
                                                                where wh_kind = '1' and wh_grade = '1' and cancel_id = 'N') 
                                                and c.mmcode = a.mmcode
                                                and exists (select 1 from MI_WLOCINV 
                                                             where wh_no = (select wh_no from MI_WHMAST 
                                                                             where wh_kind = '1' and wh_grade = '1' and cancel_id = 'N') 
                                                                and mmcode = a.mmcode)"
                                    );
            }
            // 藥庫
            else if (chk_wh_grade == "1" && chk_wh_kind == "0")
            {
                sql += string.Format(@"        from MI_WHINV a, MI_MAST b 
                                              where a.wh_no = :wh_no
                                                and a.mmcode = b.mmcode"
                                    );
            }
            // 衛星庫房藥品庫、能設通信
            else if (chk_wh_kind == "0" || chk_wh_kind == "E" || chk_wh_kind == "C")
            {
                sql += string.Format(@"        
                                 from MI_WHINV a, MI_MAST b, MI_WINVCTL c
                                where a.wh_no = :wh_no
                                  and a.mmcode = b.mmcode
                                  and c.wh_no = a.wh_no
                                  and c.mmcode = a.mmcode
                                  {0}",
                                  chk_wh_kind == "0" ? "and b.e_orderdcflag = 'N'" : "and b.cancel_id = 'N'");
                // 衛材藥品庫 & 管制藥
                if (chk_wh_kind == "0" && (chk_type == "3" || chk_type == "4")) {
                    sql += "      and c.ctdmdccode = '0'";
                }
                // 衛材藥品庫 & 非管制藥 & 手術室
                if (chk_wh_kind == "0" && 
                    (chk_type != "3" && chk_type != "4") &&
                    (wh_no != "OR1" && wh_no != "ORC" && wh_no != "OPOR")
                    )
                {
                    sql += "      and c.ctdmdccode = '0'";
                }
            }
            else
            {  //衛星庫房衛材庫
                
                sql += @"        from MI_WHINV a, MI_MAST b, MI_WINVMON c
                                where a.wh_no = :wh_no
                                  and a.mmcode = b.mmcode
                                  and c.wh_no = a.wh_no
                                  and c.mmcode = a.mmcode
                                  and c.data_ym = :preym
                                  and not (a.APL_INQTY = 0 and c.INV_QTY = 0 )";
            }

            sql += GetConditionString(chk_wh_grade, chk_wh_kind, chk_type, chk_class, chk_period, wh_no);
            //sql += GetFilter(f_u_price, f_number, f_amount);
            //sql += @"         and not exists ( select 'x' from chk_detail 
            //                                    where chk_no = :chk_no 
            //                                      and mmcode = a.mmcode
            //                                      and store_loc = a.store_loc)";

            sql += @"  order by mmcode";

            p.Add(":wh_no", string.Format("{0}", wh_no));
            p.Add(":chk_wh_grade", string.Format("{0}", chk_wh_grade));
            p.Add(":chk_wh_kind", string.Format("{0}", chk_wh_kind));
            p.Add(":chk_type", string.Format("{0}", chk_type));
            p.Add(":chk_no", string.Format("{0}", chk_no));
            p.Add(":preym", string.Format("{0}", preym));

            return DBWork.Connection.Query<CHK_DETAIL>(sql, p, DBWork.Transaction);
        }
        public IEnumerable<CHK_DETAIL> GetAllinDetailsCE(string wh_no, string chk_type, string chk_no, string chk_class, string wh_kind) {
            var p = new DynamicParameters();

            string sql = string.Format(@"
                           select distinct :wh_no as wh_no,
                                  b.mmcode, b.mmname_c, b.mmname_e, b.base_unit,
                                  (select sum(inv_qty) from MI_WHINV
                                    where wh_no  in (select wh_no from MI_WHMAST where wh_kind = :wh_kind)
                                      and mmcode = a.mmcode
                                    group by mmcode) as INV_QTY,
                                  0 as CHK_QTY,   
                                  0 as STATUS_INI,
                                  '0 準備' as STATUS_INI_NAME,
                                  ''  as store_loc, '' as store_loc_name,
                                  ' ' as MANAGERID, ' ' as MANAGERID_NAME
                             from MI_WHINV a, MI_MAST b
                            where a.wh_no = :wh_no
                              and a.mmcode = b.mmcode 
                              and b.mat_class = '{0}'
                              and b.m_applyid != 'E'
                              and b.m_applyid != 'P'", chk_class);
            sql += @"         and not exists ( select 'x' from CHK_DETAIL_TEMP 
                                                where chk_no = :chk_no 
                                                  and mmcode = a.mmcode)";

            p.Add(":wh_no", string.Format("{0}", wh_no));
            p.Add(":chk_type", string.Format("{0}", chk_type));
            p.Add(":chk_no", string.Format("{0}", chk_no));
            p.Add(":wh_kind", string.Format("{0}", wh_kind));

            return DBWork.Connection.Query<CHK_DETAIL>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<CHK_DETAIL> GetAllinDetailsANE(string wh_no, string chk_type, string chk_no) {
            var p = new DynamicParameters();

            string sql = string.Format(@"select distinct :wh_no as wh_no,
                                  b.mmcode, b.mmname_c, b.mmname_e, b.base_unit,
                                  (select sum(inv_qty) from MI_WHINV
                                    where wh_no = a.wh_no
                                      and mmcode = a.mmcode
                                    group by wh_no, mmcode) as INV_QTY,
                                  0 as CHK_QTY,   
                                  0 as STATUS_INI,
                                  '0 準備' as STATUS_INI_NAME,
                                  ''  as store_loc, '' as store_loc_name,
                                  ' ' as MANAGERID, ' ' as MANAGERID_NAME
                             from MI_WHINV a, MI_MAST b, MI_WINVCTL c
                            where a.wh_no = :wh_no
                              and a.mmcode = b.mmcode 
                              and c.wh_no = a.wh_no
                              and c.mmcode = a.mmcode
                              and c.ctdmdccode  = '0'
                              and b.e_orderdcflag = 'N'");

            switch (chk_type)
            {
                case "5":
                    sql += @" and substr(b.mmcode, 1,3) <> '006'
                              and b.e_restrictcode not in ('1','2','3', '4')";
                    break;
                case "6":
                    sql += @" and substr(b.mmcode, 1,3) = '006'
                              and b.e_restrictcode not in ('1','2','3', '4')";
                    break;
                case "3":
                    sql += @" and b.e_restrictcode in ('1','2','3')";
                    break;
                case "4":
                    sql += @" and b.e_restrictcode in ('4')";
                    break;
            }

            sql += @"         and not exists ( select 'x' from CHK_DETAIL_TEMP 
                                                where chk_no = :chk_no 
                                                  and mmcode = a.mmcode)";

            p.Add(":wh_no", string.Format("{0}", wh_no));
            p.Add(":chk_type", string.Format("{0}", chk_type));
            p.Add(":chk_no", string.Format("{0}", chk_no));

            return DBWork.Connection.Query<CHK_DETAIL>(sql, p, DBWork.Transaction);
        }
        #endregion

        #region 藥局藥品庫
        public bool ChkG2WhinvExist(string chk_no) {
            string sql = @"select 1 from CHK_G2_WHINV where chk_no = :chk_no";

            return !(DBWork.Connection.ExecuteScalar(sql,
                                                     new
                                                     {
                                                         chk_no = chk_no
                                                     },
                                                     DBWork.Transaction) == null);
        }

        public int InsertChkG2Whinv(CHK_G2_WHINV whinv) {
            string sql = @"insert into CHK_G2_WHINV(chk_no, wh_no, mmcode, store_qty, chk_pre_date,
                                                    create_date, create_user, update_time, update_user, update_ip,
                                                    seq) 
                           values (
                                :chk_no, :wh_no, :mmcode, '', '', 
                                sysdate, :create_user, sysdate, :update_user, :update_ip, 
                                :seq
                           )";
            return DBWork.Connection.Execute(sql, whinv, DBWork.Transaction);
        }
        public IEnumerable<CHK_G2_WHINV> GetChkG2Whinvs(string chk_no, string seq1, string seq2, string mmcode1, string mmcode2, bool chkpredateNullonly) {
            string sql = @"select a.chk_no, a.wh_no, a.mmcode, a.store_qty, 
                                  twn_date(a.chk_pre_date) as chk_pre_date,
                                  (select mmname_c from MI_MAST where mmcode = a.mmcode) as mmname_c,
                                  (select mmname_e from MI_MAST where mmcode = a.mmcode) as mmname_e,
                                  (select base_unit from MI_MAST where mmcode = a.mmcode) as base_unit,
                                  a.seq
                             from CHK_G2_WHINV a
                            where a.chk_no = :chk_no";

            if (seq1 != string.Empty) {
                sql += "      and a.seq >= :seq1";
            }
            if (seq2 != string.Empty)
            {
                sql += "      and a.seq <= :seq2";
            }
            if (mmcode1 != string.Empty)
            {
                sql += "      and a.mmcode >= :mmcode1";
            }
            if (mmcode2 != string.Empty)
            {
                sql += "      and a.mmcode <= :mmcode2";
            }
            if (chkpredateNullonly) {
                sql += "      and a.chk_pre_date is null";
            }

            return DBWork.PagingQuery<CHK_G2_WHINV>(sql, new { chk_no = chk_no, seq1 = seq1, seq2 = seq2,
                                                               mmcode1 = mmcode1, mmcode2 = mmcode2 }, DBWork.Transaction);
        }
        public IEnumerable<CHK_G2_WHINV> GetAllChkG2Whinvs(string chk_no)
        {
            string sql = @"select a.chk_no, a.wh_no, a.mmcode, a.store_qty, twn_date(a.chk_pre_date) as chk_pre_date,
                                  b.mmname_c, b.mmname_e, b.base_unit
                             from CHK_G2_WHINV a, MI_MAST b
                            where a.chk_no = :chk_no
                              and b.mmcode = a.mmcode";
            return DBWork.Connection.Query<CHK_G2_WHINV>(sql, new { chk_no = chk_no }, DBWork.Transaction);
        }
        public int UpdateChkG2Whinv(CHK_G2_WHINV whinv) {
            string sql = @"update CHK_G2_WHINV set chk_pre_date= to_date(:chk_pre_date, 'YYYY-MM-DD'), update_user = :update_user, update_ip = :update_ip
                            where chk_no = :chk_no and mmcode = :mmcode";

            return DBWork.Connection.Execute(sql, whinv, DBWork.Transaction);
        }

        public IEnumerable<CHK_DETAIL> GetChkGrade2Ps(string wh_no, string chk_ym)
        {
            string sql = @"select a.wh_no, a.mmcode, c.mmname_c, c.mmname_e,
                                  c.base_unit, a.inv_qty, 0 as chk_qty,
                                  '' as store_loc, '' as chk_uid, '0 開立' as status
                             from MI_WHINV a, CHK_GRADE2_P b, MI_MAST c
                            where a.wh_no = :wh_no
                              and b.mmcode = a.mmcode
                              and b.chk_ym = :chk_ym
                              and c.mmcode = a.mmcode
                              and c.e_orderdcflag = 'N'
                            order by a.mmcode";
            return DBWork.PagingQuery<CHK_DETAIL>(sql, new { wh_no = wh_no, chk_ym = chk_ym }, DBWork.Transaction);
        }
        public IEnumerable<CHK_DETAIL> GetMedItems(string wh_no, string chk_type)
        {
            string sql = @"select a.wh_no, a.mmcode, c.mmname_c, c.mmname_e,
                                  c.base_unit, a.inv_qty, 0 as chk_qty,
                                  '' as store_loc, '' as chk_uid, '0 開立' as status
                             from MI_WHINV a, MI_MAST c
                            where a.wh_no = :wh_no
                              and c.mmcode = a.mmcode
                              and c.e_invflag = 'Y'
                              and c.e_orderdcflag = 'N'
                            order by a.mmcode";

            return DBWork.PagingQuery<CHK_DETAIL>(sql, new { wh_no = wh_no }, DBWork.Transaction);
        }
        public int InsertGrade2UpDn(CHK_GRADE2_UPDN item)
        {
            string sql = @"insert into CHK_GRADE2_UPDN (chk_no, chk_ym, chk_uid,
                                                        up_date, dn_date, updn_status,
                                                        create_date, create_user, update_time, update_user, update_ip)
                           values ( :chk_no, :chk_ym, :chk_uid, 
                                    :up_date, :dn_date, :updn_status,
                                    sysdate, :create_user, sysdate, :update_user, :update_ip)";

            return DBWork.Connection.Execute(sql, item, DBWork.Transaction);
        }

        public int InsertG2Detail(CHK_G2_DETAIL detail) {
            string sql = @"insert into CHK_G2_DETAIL (chk_no, wh_no, mmcode, store_loc,
                                                      chk_qty, chk_uid, chk_time, status_ini, 
                                                      create_date, create_user, update_time, update_user, update_ip)
                           values ( :chk_no, :wh_no, :mmcode, :store_loc,
                                    :chk_qty, :chk_uid, :chk_time, :status_ini,
                                    sysdate, :create_user, sysdate, :update_user, :update_ip)";

            return DBWork.Connection.Execute(sql, detail, DBWork.Transaction);
        }

        public int MedUpdateMaster(string chk_no, int chk_total, string update_user, string update_ip)
        {
            string sql = string.Format(@"update CHK_MAST set chk_status = '1', 
                                                             chk_total = :chk_total,
                                                             update_time = sysdate,
                                                             update_user = :update_user,
                                                             update_ip = :update_ip,
                                                             chk_keeper = :update_user
                                          where chk_no = :chk_no");
            return DBWork.Connection.Execute(sql, new
            {
                chk_no = chk_no,
                chk_total = chk_total,
                update_user = update_user,
                update_ip = update_ip
            }, DBWork.Transaction);
        }

        #region 2020-05-19 新增項次欄位
        public string GetMedSeq(string chk_no) {
            string sql = @"select TO_CHAR(NVL(max(seq)+1, 1))
                             from CHK_G2_WHINV 
                            where chk_no = :chk_no";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new
            {
                chk_no = chk_no
            }, DBWork.Transaction);
        }
        #endregion

        #endregion

        #region combo

        public class WhnoComboItem
        {
            public string WH_NO { get; set; }
            public string WH_NAME { get; set; }
            public string WH_KIND { get; set; }
            public string WH_GRADE { get; set; }
            public string INID { get; set; }
        }

        public IEnumerable<WhnoComboItem> GetWhnoCombo(string wh_userId)
        {
            string sql = @"select b.WH_NO, 
                                  (b.WH_NO || ' ' || b.WH_NAME) as WH_NAME, 
                                  b.WH_KIND, 
                                  b.WH_GRADE, b.INID
                             from MI_WHID a, MI_WHMAST b
                            where a.WH_USERID = :WH_USERID
                              and b.WH_NO = a.WH_NO
                            order by b.WH_NO";
            return DBWork.Connection.Query<WhnoComboItem>(sql, new { WH_USERID = wh_userId }, DBWork.Transaction);
        }

        public IEnumerable<MI_WHID> GetWhidInfo(string wh_userId) {
            string sql = @"select a.* , b.inid, b.wh_grade, b.wh_kind
                         from MI_WHID a, MI_WHMAST b 
                        where a.wh_userid = :wh_userid and a.wh_no = b.wh_no";
            return DBWork.Connection.Query<MI_WHID>(sql, new { WH_USERID = wh_userId }, DBWork.Transaction);
        }
        public WhnoComboItem GetWhnoComboItem(string inid, string wh_kind, string wh_grade) {
            var p = new DynamicParameters();
            string sql = @"select wh_no, 
                                  (wh_no || ' ' || wh_name) as wh_name, 
                                  wh_kind, wh_grade
                             from MI_WHMAST
                            where inid = :inid";

            if (wh_kind != string.Empty) {
                sql += "      and wh_kind = :wh_kind";
            }
            if (wh_grade != string.Empty)
            {
                sql += "      and wh_grade = :wh_grade";
            }

            p.Add(":inid", string.Format("{0}", inid));
            p.Add(":wh_kind", string.Format("{0}", wh_kind));
            p.Add(":wh_grade", string.Format("{0}", wh_grade));
            return DBWork.Connection.QueryFirstOrDefault<WhnoComboItem>(sql, new { inid = inid, wh_kind = wh_kind, wh_grade = wh_grade }, DBWork.Transaction);
        }

        public IEnumerable<WhnoComboItem> GetWhnosBySupplyInid(string inid, string wh_kind) {
            var p = new DynamicParameters();
            string sql = @"select wh_no, 
                                  (wh_no || ' ' || wh_name) as wh_name, 
                                  wh_kind, wh_grade
                             from MI_WHMAST
                            where supply_inid = :inid";

            if (wh_kind != string.Empty)
            {
                sql += "      and wh_kind = :wh_kind";
            }


            p.Add(":inid", string.Format("{0}", inid));
            p.Add(":wh_kind", string.Format("{0}", wh_kind));
            return DBWork.Connection.Query<WhnoComboItem>(sql, new { inid = inid, wh_kind = wh_kind}, DBWork.Transaction);

        }
        #endregion

        #region 列印
        public IEnumerable<CE0002M> Print(string chk_no, string order, string wh_no)
        {
            var p = new DynamicParameters();

            if (wh_no == "PH1S") {
                order = order.Replace("store_loc", "substr(nvl(store_loc, ''), 3, (length(nvl(store_loc, ''))-2))");
            }

            string sql = string.Format(@"
                            select a.chk_no, b.chk_wh_no, b.chk_ym,
                                  a.mmcode, a.mmname_c, a.mmname_e, a.base_unit, a.store_loc, 
                                  a.store_qtyc as inv_qty, b.chk_wh_kind, b.chk_type,
                                  NVL(TO_CHAR((select inv_qty from MI_WINVMON 
                                                where data_ym = TWN_PYM((select substr(chk_ym,1,5) from CHK_MAST where chk_no = a.chk_no)) 
                                                  and wh_no = a.wh_no 
                                                  and mmcode = a.mmcode)),' ') as pre_inv_qty,
                                  NVL((case 
                                       when (TWN_YYYMM(sysdate) = (select SUBSTR(chk_ym, 1,5) from CHK_MAST where chk_no = a.chk_no))
                                           then (select TO_CHAR(apl_inqty) from MI_WHINV where wh_no = a.wh_no and mmcode = a.mmcode)
                                       else 
                                           (select TO_CHAR(apl_inqty) from MI_WINVMON 
                                             where data_ym = TWN_PYM((select substr(chk_ym,1,5) from CHK_MAST where chk_no = a.chk_no)) 
                                               and wh_no = a.wh_no and mmcode = a.mmcode)
                                  end),' ') as apl_inqty, 
                                  NVL((case 
                                       when (TWN_YYYMM(sysdate) = (select SUBSTR(chk_ym, 1,5) from CHK_MAST where chk_no = a.chk_no))
                                           then (select TO_CHAR(apl_outqty) from MI_WHINV where wh_no = a.wh_no and mmcode = a.mmcode)
                                       else 
                                           (select TO_CHAR(apl_outqty) from MI_WINVMON 
                                             where data_ym = TWN_PYM((select substr(chk_ym,1,5) from CHK_MAST where chk_no = a.chk_no)) 
                                               and wh_no = a.wh_no and mmcode = a.mmcode)
                                  end),' ') as apl_outqty, 
                                  (select base_unit from MI_MAST where mmcode = a.mmcode) as base_unit,
                                  (select una from UR_ID where a.chk_uid = tuser) as chk_uid_name,
                                  (select WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO = b.CHK_WH_NO) as WH_NAME,
                                  (select DATA_VALUE || ' ' || DATA_DESC 
                                     from PARAM_D 
                                    where GRP_CODE = 'CHK_MAST' 
                                      and DATA_NAME = 'CHK_CLASS'
                                      and DATA_VALUE = b.CHK_CLASS) as CHK_CLASS_NAME
                             from CHK_DETAIL a, CHK_MAST b
                            where a.chk_no = :chk_no
                            and b.chk_no = a.chk_no
                            order by {0}", order);
            p.Add(":chk_no", string.Format("{0}", chk_no));

            return DBWork.Connection.Query<CE0002M>(sql, p, DBWork.Transaction);
        }

        public string GetWhno(string chk_no) {
            string sql = @"select chk_wh_no from CHK_MAST where chk_no = :chk_No";
            return DBWork.Connection.QueryFirst<string>(sql, new { chk_no = chk_no }, DBWork.Transaction);
        }
        #endregion

        #region 科室病房衛材月盤手動新增項目 2019-12-05新增

        public IEnumerable<CHK_DETAIL> GetAddItemList(string chk_no, string wh_no, string chk_wh_grade, string chk_wh_kind, string chk_type, string chk_class, string chk_period, string mmcode) {

            var p = new DynamicParameters();

            string sql = @"select a.mmcode, b.mmname_c, b.mmname_e, a.inv_qty, b.base_unit
                             from MI_WHINV a, MI_MAST b
                            where a.wh_no = :wh_no
                              and a.mmcode = b.mmcode";
            sql += GetConditionString(chk_wh_grade, chk_wh_kind, chk_type, chk_class, chk_period, wh_no);

            sql += @"         and not exists (select 1 from CHK_DETAIL 
                                               where chk_no = :chk_no and mmcode = a.mmcode)";
            if (mmcode != string.Empty) {
                sql += @"     and a.mmcode like :mmcode";
            }

            p.Add(":wh_no", string.Format("{0}", wh_no));
            p.Add(":chk_no", string.Format("{0}", chk_no));
            p.Add(":mmcode", string.Format("%{0}%", mmcode));

            return DBWork.PagingQuery<CHK_DETAIL>(sql, p, DBWork.Transaction);
        }

        public int AddItem(string chk_no, string wh_no, string mmcode, string userid, string ip) {
            string sql = @"insert into CHK_DETAIL (chk_no, mmcode, mmname_c, mmname_e, base_unit,
                                                   m_contprice, wh_no, store_loc, loc_name, mat_class,
                                                   m_storeid, store_qtyc, store_qtym, store_qtys,
                                                   chk_qty, chk_remark, chk_uid, chk_time, status_ini,
                                                   create_date, create_user, update_time, update_user, update_ip)
                           select :chk_no as chk_no,
                                  :mmcode as mmcode,
                                  a.mmname_c, a.mmname_e, a.base_unit, a.m_contprice,
                                  :wh_no as wh_no,
                                  'TEMP' as store_loc, ' ' as loc_name, 
                                  a.mat_class, a.m_storeid,
                                  (select inv_qty from MI_WHINV where wh_no = :wh_no and mmcode = :mmcode) as store_qtyc,
                                  0 as store_qtym, 
                                  0 as store_qtys,
                                  '' as chk_qty,
                                  '' as chk_remark,
                                  '' as chk_uid,
                                  '' as chk_time,
                                  '1' as status_ini,
                                  sysdate as create_date,
                                  :userid as create_user,
                                  sysdate as update_time,
                                  :userid as update_user,
                                  :ip as update_ip
                             from MI_MAST a 
                            where a.mmcode = :mmcode";
            return DBWork.Connection.Execute(sql, new {chk_no = chk_no, wh_no = wh_no,mmcode = mmcode, userid = userid, ip = ip }, DBWork.Transaction);
        }


        #endregion

        #region 修改已分配人員
        public IEnumerable<BC_WHCHKID> GetUidList(string chk_no, bool isWard) {
            string sql = string.Format(@"select a.wh_no, a.wh_chkuid,
                                  (select una from UR_ID where tuser = a.wh_chkuid) as WH_CHKUID_NAME,
                                  (case
                                        when (select 1 from {0} where chk_no = :chk_no and chk_uid = a.wh_chkuid) = 1
                                            then 'Y'
                                        else 'N'
                                   end) as is_selected,
                                  (case
                                        when exists(select 1 from CHK_{1}DETAIL 
                                               where chk_no = :chk_no and chk_uid = a.wh_chkuid 
                                                 and chk_time > to_date('2000-01-01', 'YYYY-MM-DD'))
                                            then 'Y'
                                        else 'N'
                                   end) as has_entry
                             from BC_WHCHKID a
                            where wh_no = (select chk_wh_no from CHK_MAST where chk_no = :chk_no)"
                          , isWard == true ? "CHK_NOUID" : "CHK_GRADE2_UPDN"
                          , isWard == true ? string.Empty : "G2_");
            return DBWork.PagingQuery<BC_WHCHKID>(sql, new { chk_no = chk_no }, DBWork.Transaction);
        }

        public int MedUpdateChkTotal(string chk_no) {
            string sql = @"update CHK_MAST
                              set chk_total = (select count(*) from CHK_GRADE2_UPDN where chk_no = :chk_no),
                                  chk_status = (case when chk_num = (select count(*) from CHK_GRADE2_UPDN where chk_no = :chk_no)
                                                        then '2'
                                                     else '1'
                                                end)
            
                            where chk_no = :chk_no";
            return DBWork.Connection.Execute(sql, new { chk_no = chk_no }, DBWork.Transaction);
        }
        #endregion

        #region 藥庫-認可庫存為0的品項 2020-02-27新增
        public int Invqty0Confirm(string chk_nos, string userid, string ip) {
            string sql = string.Format(@" update CHK_DETAIL
                               set chk_qty = store_qtyc, chk_time = sysdate, status_ini = '2', 
                                   chk_uid = :userId,
                                   update_user = :userid, update_ip = :ip
                             where chk_no in ( {0} ) 
                               and store_qtyc = 0
                               and chk_time is null
                               and status_ini = '1'", chk_nos);

            return DBWork.Connection.Execute(sql, new { userid = userid, ip = ip }, DBWork.Transaction);
        }
        public int UpdateChkmastNum(string chk_nos, string userId, string userIp)
        {
            var sql = string.Format(@"Update chk_mast a 
                                         set a.CHK_NUM = ( Select count(*)  from chk_detail Where CHK_NO = a.chk_no AND STATUS_INI = '2'), 
                                             a.UPDATE_TIME = SYSDATE, a.UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP 
                                       where a.CHK_NO in ( {0} )", chk_nos);
            return DBWork.Connection.Execute(sql, new { UPDATE_USER = userId, UPDATE_IP = userIp}, DBWork.Transaction);
        }
        #endregion

        #region 中央庫房-有已點收(flowid=5)的單子不可開盤點單 2020-03-10新增
        // 2020-03-24 修改: 改參考月結年月，藥庫也需要判斷
        // 2020-05-07 修改: 加上小額採購條件
        // 2020-05-13 修改: 小額採購條件改為判斷apply_kind = '3'
        public int CheckMeDocm5(string doctypes, string flowids, string matClasses, string apply_kind) {
            // 藥品的flowid: 0104, 0604
            string sql = string.Format(@"select count(docno) 
                                           from ME_DOCM a
                                          where doctype in ( {0} )
                                            and mat_class in ( {2} )
                                            and trunc(apptime) >= trunc(
                                                    (select set_btime from MI_MNSET where set_ym = cur_setym())
                                                )
                                            and flowid in ( {1} )
                                            and exists (select 1 from ME_DOCD
                                                         where docno = a.docno and nvl(onway_qty, 0) <> 0)",
                                         doctypes, flowids, matClasses);
            if (apply_kind != string.Empty)
            {
                sql += string.Format("      and a.apply_kind = '{0}'", apply_kind);
            }
            else {
                sql += string.Format("      and a.apply_kind in ('1', '2')");
            }

            return DBWork.Connection.QueryFirst<int>(sql, DBWork.Transaction);
        }
        #endregion

        #region 以月結方式處理在途量 2020-03-24新增
        public string GetSetYM() {
            string sql = @"select cur_setym() from dual";
            return DBWork.Connection.QueryFirst<string>(sql, DBWork.Transaction);
        }

        public SP_MODEL PostUack(string set_ym, string store_id, string mat_class)
        {
            var p = new OracleDynamicParameters();
            p.Add("I_YM", value: set_ym, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 21);
            p.Add("I_STOREID", value: store_id, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 21);
            p.Add("I_MATCLASS", value: mat_class, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 21);

            p.Add("O_RETID", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 1);
            p.Add("O_RETMSG", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 200);

            DBWork.Connection.Query("INV_SET.POST_UACK_MM", p, commandType: CommandType.StoredProcedure);
            string retid = p.Get<OracleString>("O_RETID").Value;
            string errmsg = p.Get<OracleString>("O_RETMSG").Value;

            SP_MODEL sp = new SP_MODEL
            {
                O_RETID = retid,
                O_ERRMSG = errmsg
            };
            return sp;
        }
        #endregion

        #region 2020-05-22 配合藥局需求 CHK_G2_WHINV新增項次欄位 查詢新增項次院內碼欄位

        public IEnumerable<MI_MAST> GetMmCodeCombo(string p0, string chk_no, int page_index, int page_size, string sorters)
        {

            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT,A.M_CONTPRICE    
                        FROM MI_MAST A 
                        WHERE  1=1  ";
            if (chk_no != "")
            {
                sql += " AND EXISTS (SELECT 1 FROM CHK_G2_WHINV where chk_no = :chk_no and mmcode = a.mmcode) ";
                p.Add(":chk_no", chk_no);
            }
            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}%", p0));

                sql += " OR MMNAME_E LIKE UPPER(:MMNAME_E) ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR MMNAME_C LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY MMCODE ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public bool CheckMedChkPreDatesNull(string chk_no) {
            string sql = @"select 1 from CHK_G2_WHINV
                            where chk_no = :chk_no
                              and chk_pre_date is null"; // TRUNC(a.EXP_DATE, 'DD') TWN_TODATE(:exp_date)

            return !(DBWork.Connection.ExecuteScalar(sql,
                                                     new
                                                     {
                                                         chk_no = chk_no
                                                     },
                                                     DBWork.Transaction) == null);

        }

        #endregion

        #region 2020-07-09 新增: 衛星庫房衛材庫開單時加入本月新申領(上月月結不存在)品項
        public IEnumerable<CHK_DETAIL> GetAllinDetailsNoPre(string wh_no, string chk_wh_grade, string chk_wh_kind, string chk_type, string chk_no, string chk_class, string chk_period) {
            var p = new DynamicParameters();
            var preym = GetPreym();
            string sql = @"select b.mmcode, b.mmname_c, b.mmname_e,
                                  b.base_unit, b.m_contprice, a.wh_no,
                                  b.mat_class,
                                  b.m_storeid, b.e_takekind, b.e_restrictcode,
                                  a.inv_qty as STORE_QTYC, b.base_unit,
                                  0 as STATUS_INI,
                                  '0 準備' as STATUS_INI_NAME,
                                  ' ' as MANAGERID, ' ' as MANAGERID_NAME
                             from MI_WHINV a, MI_MAST b
                            where a.wh_no = :wh_no
                              and a.mmcode = b.mmcode
                              and a.apl_inqty > 0
                              and not exists (select 1 from MI_WINVMON 
                                               where wh_no = a.wh_no and mmcode = a.mmcode and data_ym = :preym)";
            sql += GetConditionString(chk_wh_grade, chk_wh_kind, chk_type, chk_class, chk_period, wh_no);

            sql += @"  order by mmcode";

            p.Add(":wh_no", string.Format("{0}", wh_no));
            p.Add(":chk_wh_grade", string.Format("{0}", chk_wh_grade));
            p.Add(":chk_wh_kind", string.Format("{0}", chk_wh_kind));
            p.Add(":chk_type", string.Format("{0}", chk_type));
            p.Add(":chk_no", string.Format("{0}", chk_no));
            p.Add(":preym", string.Format("{0}", preym));

            return DBWork.Connection.Query<CHK_DETAIL>(sql, p, DBWork.Transaction);
        }
        #endregion

        #region 2020-09-18 新增: 藥庫盤點單需將當下申請量核撥量寫入CHK_PH1S_WHINV
        public int InsertChkPH1SWhinv(string chk_no, string userid, string update_ip) {
            string sql = @"insert into CHK_PH1S_WHINV
                                  (chk_no, mmcode, apl_inqty, apl_outqty, exg_inqty, exg_outqty,
                                   create_time, create_user, update_time, update_user, update_ip)
                            select :chk_no as chk_no,
                                   a.mmcode, 
                                   a.apl_inqty,
                                   a.apl_outqty,
                                   a.exg_inqty,
                                   a.exg_outqty,
                                   sysdate,
                                   :userid,
                                   sysdate,
                                   :userid,
                                   :update_ip
                              from MI_WHINV a
                             where a.wh_no = 'PH1S'
                               and exists (select 1 from CHK_DETAIL where chk_no = :chk_no and mmcode = a.mmcode)
                                   ";
            return DBWork.Connection.Execute(sql, new
            {
                chk_no = chk_no,
                userid = userid,
                update_ip = update_ip
            }, DBWork.Transaction);
        }

        public int DeleteChkPH1SWhinv(string chk_no) {
            string sql = @"delete from CHK_PH1S_WHINV
                            where chk_no = :chk_no";
            return DBWork.Connection.Execute(sql, new { chk_no = chk_no }, DBWork.Transaction);
        }
        #endregion

        #region 2020-12-09 新增: 科室病房檢查是否有在途量，有的話不可開單
        public int CheckOnwayQty(string wh_no)
        {
            string sql = string.Format(@"select count(mmcode)
                                           from MI_WHINV
                                          where wh_no = :wh_no
                                            and onway_qty > 0");

            return DBWork.Connection.QueryFirst<int>(sql,new { wh_no = wh_no}, DBWork.Transaction);
        }
        #endregion

        #region
        public IEnumerable<CHK_ADD_ITEM> GetNeedDetailAddList(string wh_no) {
            string sql = @"
                with set_ym as (
                    select set_ym from MI_MNSET where set_status = 'N'
                 ),chk_settime as (
                    select chk_ym, set_atime 
                      from CHK_MNSET a, set_ym b
                     where a.chk_ym = b.set_ym
                ), wh_trns_data as (
                    select a.wh_no, c.chk_ym,
                           a.mmcode, d.m_storeId, 
                           (case when d.m_contid = '3' then '3' else 'N3' end) as m_contid
                      from MI_WHTRNS a, chk_settime c, MI_MAST d
                     where a.wh_no = :wh_no
                       and exists (select 1 from MI_WHMAST where wh_no = a.wh_no and wh_kind = '1' and wh_grade > '1')
                       and a.tr_date >= c.set_atime
                       and a.mmcode = d.mmcode
                       and d.mat_class = '02'
                       and ( (a.tr_mcode = 'WAYI' and tr_doctype in ('MR1', 'MR2', 'MR3', 'MR4','TR1'))
                             or (a.tr_mcode = 'TRNO')
                             or a.tr_mcode = 'BAKO')
                )
                select a.*,
                       (case when a.m_contid = '3' then m_contid else a.m_storeid end) as chk_type
                  from wh_trns_data a
                 where 1=1
                   and not exists (select 1 from CHK_DETAIL
                                    where chk_no in (select chk_no from CHK_MAST 
                                                      where chk_ym = a.chk_ym
                                                        and chk_wh_no = a.wh_no
                                                        and chk_period = 'M')
                                      and mmcode = a.mmcode)
            ";
            return DBWork.Connection.Query<CHK_ADD_ITEM>(sql, new { wh_no }, DBWork.Transaction);
        }

        public CHK_ADD_ITEM GetChkNoFromAddItem(string wh_no, string chk_type) {
            string sql = @"
                with set_ym as (
                    select set_ym from MI_MNSET where set_status = 'N'
                 ),chk_settime as (
                    select chk_ym, set_atime 
                      from CHK_MNSET a, set_ym b
                     where a.chk_ym = b.set_ym
                ), chk_datas as (
                    select max(a.chk_level) as max_chk_level, a.chk_type, a.chk_wh_no, a.chk_ym
                      from CHK_MAST a, chk_settime b
                     where a.chk_ym = b.chk_ym
                       and a.chk_period = 'M'
                       and a.chk_wh_no = :wh_no
                       and a.chk_type = :chk_type
                     group by a.chk_wh_no, a.chk_ym, a.chk_type
                )
                select a.chk_no, a.chk_wh_no as wh_no, a.chk_type, a.chk_status, a.chk_level
                  from CHK_MAST a, chk_datas b
                 where a.chk_wh_no = b.chk_wh_no
                   and a.chk_ym = b.chk_ym
                   and a.chk_type = b.chk_type
                   and a.chk_level = b.max_chk_level
            ";
            return DBWork.Connection.QueryFirstOrDefault<CHK_ADD_ITEM>(sql, new { wh_no, chk_type }, DBWork.Transaction);
        }

        public int AddDetail(string chk_no, string mmcode, string userId, string ip) {
            string sql = @"
                insert into CHK_DETAIL (chk_no, mmcode, mmname_c, mmname_e, base_unit,
                                                   m_contprice, wh_no, store_loc, loc_name, mat_class,
                                                   m_storeid, store_qtyc, store_qtym, store_qtys,
                                                   chk_qty, chk_remark, chk_uid, chk_time, status_ini,
                                                   create_date, create_user, update_time, update_user, update_ip)
                select :chk_no as chk_no, :mmcode as mmcode,
                       a.mmname_c, a.mmname_e, a.base_unit,
                       a.m_contprice, b.wh_no, 'TEMP' as store_loc, '' as loc_name, a.mat_class,
                       a.m_storeid, b.inv_qty as store_qtyc, 0 as store_qtym, 0 as store_qtys, 
                       '' as chk_qty, '' as chk_remark, '' as chk_uid, '' as chk_time,
                       (select chk_status from CHK_MAST where chk_no = :chk_no) as status_ini,
                       sysdate as create_date, :userId as create_user, sysdate as update_time, :userId as updateUser, :ip as update_ip 
                  from MI_MAST a, MI_WHINV b
                 where b.wh_no = (select chk_wh_no from CHK_MAST where chk_no = :chk_no)
                   and a.mmcode = b.mmcode
                   and a.mmcode = :mmcode
            ";
            return DBWork.Connection.Execute(sql, new { chk_no, mmcode, userId, ip }, DBWork.Transaction);
        }
        public int UpdateChkmastTotal(string chk_no, string userId, string ip) {
            string sql = @"
                update CHK_MAST
                   set chk_total = (select count(*) from CHK_DETAIL where chk_no = :chk_no),
                       update_time = sysdate,
                       update_user = :userId,
                       update_ip = :ip
                 where chk_no = :chk_no
            ";
            return DBWork.Connection.Execute(sql, new { chk_no, userId, ip }, DBWork.Transaction);
        }
        #endregion
    }
}