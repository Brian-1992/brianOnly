using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Repository.AA
{
    public class AA0135Repository : JCLib.Mvc.BaseRepository
    {
        public AA0135Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<CHK_MNSET> GetDone() {
            string sql = @"select chk_ym as chk_ym, 
                                  twn_date(set_ctime) as set_ctime,
                                  twn_date(set_atime) as set_atime
                             from CHK_MNSET
                            where set_atime is not null";
            return DBWork.PagingQuery<CHK_MNSET>(sql, DBWork.Transaction);
        }

        public IEnumerable<CHK_MNSET> GetCurrent() {
            string sql = @"select chk_ym as chk_ym, 
                                  twn_date(set_ctime) as set_ctime
                             from CHK_MNSET
                            where set_atime is null";
            return DBWork.PagingQuery<CHK_MNSET>(sql, DBWork.Transaction);
        }

        public int UpdateChkMnset(CHK_MNSET mnset) {
            string sql = @"update CHK_MNSET
                              set set_ctime = twn_todate(:set_ctime),
                                  update_user = :update_user,
                                  update_time = sysdate,
                                  update_ip = :update_ip
                            where chk_ym = :chk_ym";
            return DBWork.Connection.Execute(sql, mnset, DBWork.Transaction);
        }

        #region 2020-05-27 新增: 補開科室病房月盤單

        

        public bool CheckMiWhmast(string wh_no) {
            string sql = @"select 1 from MI_WHMAST 
                            where wh_no = :wh_no
                              and wh_kind = '1'
                              and wh_grade = '2'
                              and cancel_id  = 'N'";
            return !(DBWork.Connection.ExecuteScalar(sql,
                                                     new
                                                     {
                                                         wh_no = wh_no
                                                     },
                                                     DBWork.Transaction) == null);
        }

        public CHK_MNSET GetChkmnset()
        {
            string sql = @"select a.chk_ym, a.set_status,
                                  to_char(a.set_ctime, 'YYYY-MM-DD') as set_ctime, 
                                  twn_pym(a.chk_ym) as pre_ym,
                                  twn_yyymm(add_months(twn_todate(a.chk_ym||'01'), 1)) as post_ym
                             from CHK_MNSET a
                            where a.set_atime < sysdate
                              and set_status = 'Y'
                            order by chk_ym desc";
            return DBWork.Connection.QueryFirst<CHK_MNSET>(sql, DBWork.Transaction);
        }

        public IEnumerable<ChkWh> GetWhnos(string m_storeid, string preym, string wh_no)
        {
            string sql = @"select a.wh_no as wh_no,
                                  b.wh_grade as wh_grade,
                                  count(*) as chk_total,
                                  b.inid as inid
                             from MI_WHINV a, MI_WHMAST b, MI_MAST c, MI_WINVMON d
                            where a.wh_no = :wh_no
                              and b.wh_no = a.wh_no
                              and c.mmcode = a.mmcode
                              and b.wh_kind = '1'
                              and c.mat_class = '02'
                              and b.wh_grade in ('2', '3', '4')
                              and c.m_storeid = :STOREID
                              and c.m_contid <> '3'
                              and b.cancel_id = 'N'
                              and d.wh_no = a.wh_no
                              and d.mmcode = a.mmcode
                              and d.data_ym = :preym
                              and not (a.apl_inqty = 0 and d.inv_qty = 0)
                           having count(*) > 0
                            group by a.wh_no, b.wh_grade, b.inid
                            order by a.wh_no, b.wh_grade, b.inid";

            return DBWork.Connection.Query<ChkWh>(sql, new { STOREID = m_storeid, preym = preym, wh_no = wh_no }, DBWork.Transaction);
        }

        public bool CheckExists(string wh_no, string chk_ym, string store_id, string chk_class, string chk_period)
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
                                                         chk_wh_no = wh_no,
                                                         chk_ym = chk_ym,
                                                         chk_period = chk_period,
                                                         chk_type = store_id,
                                                         chk_class = chk_class
                                                     },
                                                     DBWork.Transaction) == null);
        }
        public string GetCurrentSeq(string wh_no, string ym)
        {
            string sobstringIndex = (wh_no.Length + ym.Length + 3).ToString();
            string queryIndex = (wh_no.Length + ym.Length).ToString();

            string sql = string.Format(@"select NVL(max(substr(CHK_NO ,{0},2))+1, 1)
                                           from CHK_MAST 
                                          where substr(chk_no,1,{1}) = :chk_no", sobstringIndex, queryIndex);

            string result = DBWork.Connection.QueryFirst<string>(sql,
                                                         new
                                                         {
                                                             chk_no = string.Format("{0}{1}", wh_no, ym)
                                                         }, DBWork.Transaction);
            return result.PadLeft(2, '0');

        }
        public int InsertMaster(CHK_MAST mast)
        {
            string sql = @"insert into CHK_MAST 
                                  (CHK_NO, CHK_YM, CHK_WH_NO, CHK_WH_GRADE, CHK_WH_KIND, CHK_CLASS,
                                   CHK_PERIOD, CHK_TYPE, CHK_LEVEL, CHK_NUM, CHK_TOTAL, CHK_STATUS,
                                   CHK_KEEPER, CHK_NO1,
                                   CREATE_DATE, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)
                           values ( :chk_no, :chk_ym, :chk_wh_no, :chk_wh_grade, :chk_wh_kind, :chk_class,
                                    :chk_period, :chk_type, :chk_level, :chk_num, :chk_total, :chk_status,
                                    :chk_keeper, :chk_no1,
                                    sysdate, :create_user, sysdate, :update_user, :update_ip
                                  )";
            return DBWork.Connection.Execute(sql, mast, DBWork.Transaction);
        }
        public int InsertDetail(CHK_MAST mast, string preym)
        {
            string sql = string.Format(@"insert into CHK_DETAIL(chk_no, mmcode, mmname_c, mmname_e, base_unit,
                                                   m_contprice, wh_no, store_loc, loc_name, mat_class,
                                                   m_storeid, store_qtyc, store_qtym, store_qtys,
                                                   chk_qty, chk_remark, chk_uid, chk_time, status_ini,
                                                   create_date, create_user, update_time, update_user, update_ip)
                            select :chk_no as chk_no,
                                   b.mmcode as mmcode,
                                   b.mmname_c as mmname_c,
                                   b.mmname_e as mmname_e,
                                   b.base_unit as base_unit,
                                   b.m_contprice as m_contprice,
                                   a.wh_no as wh_no,
                                   'TEMP' as store_loc,
                                   '' as loc_name,
                                   b.mat_class as mat_class,
                                   b.m_storeid as m_storeid,
                                   a.inv_qty as store_qtyc, 
                                   0 as store_qtym, 
                                   0 as store_qtys,
                                   '' as chk_qty,
                                   '' as chk_remark,
                                   '' as chk_uid,
                                   '' as chk_time,
                                   '1' as status_ini,
                                   sysdate as create_date,
                                   'BATCH' as create_user,
                                   sysdate as update_time,
                                   '' as update_user,
                                   '' as update_ip
                              from MI_WHINV a, MI_MAST b, MI_WINVMON c
                             where a.wh_no = :wh_no
                               and b.m_storeid = :storeid
                               and a.mmcode = b.mmcode
                               and b.mat_class = '02'
                               and c.wh_no = a.wh_no
                               and c.mmcode = a.mmcode
                               and c.data_ym = :preym
                               and not (a.APL_INQTY = 0 and c.INV_QTY = 0 )
                               and b.m_contid <> '3'");

            return DBWork.Connection.Execute(sql,
                new
                {
                    wh_no = mast.CHK_WH_NO,
                    storeid = mast.CHK_TYPE,
                    chk_no = mast.CHK_NO,
                    preym = preym,
                },
                DBWork.Transaction);
        }
        public int UpdateMaster(CHK_MAST mast)
        {
            string sql = "update chk_mast set CHK_TOTAL = (select count(*) from CHK_DETAIL where chk_no = :chk_no) where chk_no = :chk_no";
            return DBWork.Connection.Execute(sql, new { chk_no = mast.CHK_NO }, DBWork.Transaction);
        }
        public int InsertNouid(CHK_MAST mast)
        {
            string sql = @"insert into CHK_NOUID(chk_no, chk_uid, create_date, create_user, update_time, update_user, update_ip)
                           select :chk_no as chk_no,
                                  a.wh_chkuid as chk_uid,
                                  sysdate as create_date,
                                   'BATCH' as create_user,
                                   sysdate as update_time,
                                   '' as update_user,
                                   '' as update_ip
                             from BC_WHCHKID a
                            where a.wh_no = :wh_no";
            return DBWork.Connection.Execute(sql,
                new
                {
                    wh_no = mast.CHK_WH_NO,
                    chk_no = mast.CHK_NO
                },
                DBWork.Transaction);
        }

        #region 小額採購

        public IEnumerable<ChkWh> GetWhno3s(string preym, string wh_no)
        {
            string sql = @"select a.wh_no as wh_no,
                                  b.wh_grade as wh_grade,
                                  count(*) as chk_total,
                                  b.inid as inid
                             from MI_WHINV a, MI_WHMAST b, MI_MAST c, MI_WINVMON d
                            where a.wh_no = :wh_no
                              and b.wh_no = a.wh_no
                              and c.mmcode = a.mmcode
                              and not (a.apl_inqty = 0 and a.inv_qty = 0)
                              and c.m_contid = '3'
                              and c.mat_class = '02'
                              and b.wh_kind = '1'
                              and c.mat_class = '02'
                              and b.wh_grade in ('2', '3', '4')
                              and b.cancel_id = 'N'
                              and d.wh_no = a.wh_no
                              and d.mmcode = a.mmcode
                              and d.data_ym = :preym
                              and not (a.apl_inqty = 0 and d.inv_qty = 0)
                           having count(*) > 0
                            group by a.wh_no, b.wh_grade, b.inid
                            order by a.wh_no, b.wh_grade, b.inid";

            return DBWork.Connection.Query<ChkWh>(sql, new { preym = preym, wh_no = wh_no }, DBWork.Transaction);
        }

        public int InsertDetailType3(CHK_MAST mast, string preym)
        {
            string sql = @"insert into CHK_DETAIL(chk_no, mmcode, mmname_c, mmname_e, base_unit,
                                                   m_contprice, wh_no, store_loc, loc_name, mat_class,
                                                   m_storeid, store_qtyc, store_qtym, store_qtys,
                                                   chk_qty, chk_remark, chk_uid, chk_time, status_ini,
                                                   create_date, create_user, update_time, update_user, update_ip)
                            select :chk_no as chk_no,
                                   b.mmcode as mmcode,
                                   b.mmname_c as mmname_c,
                                   b.mmname_e as mmname_e,
                                   b.base_unit as base_unit,
                                   b.m_contprice as m_contprice,
                                   a.wh_no as wh_no,
                                   'TEMP' as store_loc,
                                   '' as loc_name,
                                   b.mat_class as mat_class,
                                   b.m_storeid as m_storeid,
                                   a.inv_qty as store_qtyc, 
                                   0 as store_qtym, 
                                   0 as store_qtys,
                                   '' as chk_qty,
                                   '' as chk_remark,
                                   '' as chk_uid,
                                   '' as chk_time,
                                   '1' as status_ini,
                                   sysdate as create_date,
                                   'BATCH' as create_user,
                                   sysdate as update_time,
                                   '' as update_user,
                                   '' as update_ip
                              from MI_WHINV a, MI_MAST b, MI_WINVMON c
                             where a.wh_no = :wh_no
                               and a.mmcode = b.mmcode
                               and b.mat_class = '02'
                               and b.m_contid = '3'
                               and c.wh_no = a.wh_no
                               and c.mmcode = a.mmcode
                               and not (a.APL_INQTY = 0 and c.INV_QTY = 0 )
                               and c.data_ym = :preym";



            return DBWork.Connection.Execute(sql,
                new
                {
                    wh_no = mast.CHK_WH_NO,
                    storeid = mast.CHK_TYPE,
                    chk_no = mast.CHK_NO,
                    preym = preym
                },
                DBWork.Transaction);
        }

        #endregion

        #endregion
    }
}