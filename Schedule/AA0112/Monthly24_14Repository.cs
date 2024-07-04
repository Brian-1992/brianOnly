using Dapper;
using JCLib.DB;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Models;

namespace AA0112
{
    public class Monthly24_14Repository : JCLib.Mvc.BaseRepository
    {
        public Monthly24_14Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public CHK_MNSET GetChkmnset()
        {
            string sql = @"select chk_ym, set_status,
                                  to_char(set_ctime, 'YYYY-MM-DD') as set_ctime, 
                                  twn_pym(chk_ym) as pre_ym,
                                  twn_yyymm(add_months(twn_todate(chk_ym||'01'), 1)) as post_ym
                             from CHK_MNSET
                            where set_status = 'N'";
            return DBWork.Connection.QueryFirst<CHK_MNSET>(sql, DBWork.Transaction);
        }

        public IEnumerable<ChkWh> GetWhnos()
        {
            string sql = @"select a.set_ym as chk_ym, b.wh_no as wh_no, wh_name(b.wh_no) as wh_name, b.wh_grade as wh_grade, 
                                  b.wh_kind , b.inid as inid 
                             from ((select set_ym 
                                      from (select set_ym, set_ctime 
                                              from (select set_ym, twn_date(set_ctime+1) as set_ctime from MI_MNSET where set_status = 'C'
                                             order by set_ym desc)
                                     where rownum=1)) ) a, MI_WHMAST b where b.cancel_id = 'N'";
            return DBWork.Connection.Query<ChkWh>(sql, DBWork.Transaction);
        }
        public bool CheckExists(string wh_no, string chk_ym, string chk_period)
        {
            string sql = @"select 1 from CHK_MAST
                            WHERE chk_wh_no = :chk_wh_no 
                              AND chk_ym = :chk_ym 
                              AND chk_period = :chk_period 
                              AND chk_type = 'X' ";
            return !(DBWork.Connection.ExecuteScalar(sql,
                                                     new
                                                     {
                                                         chk_wh_no = wh_no,
                                                         chk_ym = chk_ym,
                                                         chk_period = chk_period
                                                     },
                                                     DBWork.Transaction) == null);
        }
        public string GetCurrentSeq(string wh_no, string ym)
        {
            string sobstringIndex = (wh_no.Length + ym.Length + 3).ToString();
            string queryIndex = (wh_no.Length + ym.Length).ToString();

            string sql = string.Format(@"select NVL(max(to_number(substr(CHK_NO ,{0},3)))+1, 1)
                             from CHK_MAST 
                            where substr(chk_no,1,{1}) = :chk_no", sobstringIndex, queryIndex);

            string result = DBWork.Connection.QueryFirst<string>(sql,
                                                         new
                                                         {
                                                             chk_no = string.Format("{0}{1}", wh_no, ym)
                                                         }, DBWork.Transaction);
            return result.PadLeft(3, '0');

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

        public int InsertDetail_A(CHK_MAST mast)
        {
            string sql = string.Format(@"insert into CHK_DETAIL(
                                                                chk_no, mmcode, mmname_c, mmname_e, base_unit, m_contprice, wh_no, 
                                                                store_loc, 
                                                                mat_class, m_storeid, store_qtyc, store_qtym, store_qtys, 
                                                                chk_qty, chk_remark, chk_uid, chk_time, status_ini, 
                                                                create_date, create_user, update_time, update_user, update_ip,
                                                                pym_inv_qty, apl_inqty, apl_outqty, trn_inqty, trn_outqty, adj_inqty, adj_outqty,
                                                                bak_inqty, bak_outqty, rej_outqty, dis_outqty, exg_inqty, exg_outqty, mil_inqty, mil_outqty, use_qty,
                                                                mat_class_sub, disc_cprice, pym_cont_price, pym_disc_cprice, 
                                                                war_qty, 
                                                                pym_war_qty,
                                                                unitrate, m_contid, e_sourcecode, e_restrictcode, common,
                                                                fastdrug, drugkind, touchcase, orderkind, spdrug)
                                            select :chk_no as chk_no, b.mmcode as mmcode, b.mmname_c as mmname_c, b.mmname_e as mmname_e, b.base_unit as base_unit, b.m_contprice as m_contprice, a.wh_no as wh_no, 
                                            (select listagg(store_loc, ', ') within group (order by store_loc) from MI_WLOCINV where wh_no = a.wh_no and mmcode = a.mmcode) as store_loc, 
                                            b.mat_class as mat_class, b.m_storeid as m_storeid, a.inv_qty as store_qtyc, 0 as store_qtym,  0 as store_qtys, 
                                            a.inv_qty as chk_qty, '' as chk_remark, (case when :wh_grade='1' then 'system' else '' end) as chk_uid,
                                            (case when :wh_grade='1' then sysdate else null end) as chk_time, '1' as status_ini,
                                             sysdate as create_date, 'BATCH' as create_user, sysdate as update_time, '' as update_user, '' as update_ip,
                                            nvl(d.inv_qty, 0), a.apl_inqty, a.apl_outqty, a.trn_inqty, a.trn_outqty, a.adj_inqty, a.adj_outqty,
                                            a.bak_inqty, a.bak_outqty, a.rej_outqty, a.dis_outqty, a.exg_inqty, a.exg_outqty, a.mil_inqty, a.mil_outqty, a.use_qty,
                                            b.mat_class_sub, b.disc_cprice, nvl(c.cont_price, 0), nvl(c.disc_cprice, 0), 
                                            (select war_qty from MI_WHCOST where data_ym = a.data_ym and mmcode = a.mmcode), 
                                            c.war_qty,
                                             b.unitrate, 
                                             b.m_contid, b.e_sourcecode, b.e_restrictcode, b.common,
                                            b.fastdrug, b.drugkind, b.touchcase, b.orderkind, b.spdrug
                                            from  MI_MAST_MON b, MI_WINVMON a
                                            left join MI_WINVMON d on (d.data_ym = twn_pym(a.data_ym) and d.mmcode = a.mmcode and d.wh_no = a.wh_No)
                                            left join MI_WHCOST c on (c.data_ym = twn_pym(a.data_ym) and c.mmcode = a.mmcode)
                                            where 1=1
                                            and b.data_ym = :chk_ym and b.data_ym = a.data_ym
                                            and (nvl(b.cancel_id,'N')='N' or nvl(e_orderdcflag,'N')='N')
                                            and a.wh_no = :wh_no and a.mmcode = b.mmcode");

            return DBWork.Connection.Execute(sql,
                new
                {
                    chk_no = mast.CHK_NO,
                    chk_ym = mast.CHK_YM,
                    wh_no = mast.CHK_WH_NO,
                    wh_grade=mast.CHK_WH_GRADE
                },
                DBWork.Transaction);
        }

        public int InsertDetail_B(CHK_MAST mast)
        {
            string sql = string.Format(@"insert into CHK_DETAIL(
                                                                chk_no, mmcode, mmname_c, mmname_e, base_unit, m_contprice, wh_no, 
                                                                store_loc, 
                                                                mat_class, m_storeid, store_qtyc, store_qtym, store_qtys, 
                                                                chk_qty, chk_remark, chk_uid, chk_time, status_ini, 
                                                                create_date, create_user, update_time, update_user, update_ip,
                                                                pym_inv_qty, apl_inqty, apl_outqty, trn_inqty, trn_outqty, adj_inqty, adj_outqty,
                                                                bak_inqty, bak_outqty, rej_outqty, dis_outqty, exg_inqty, exg_outqty, mil_inqty, mil_outqty, use_qty,
                                                                mat_class_sub, disc_cprice, pym_cont_price, pym_disc_cprice, 
                                                                war_qty, 
                                                                pym_war_qty,
                                                                unitrate, m_contid, e_sourcecode, e_restrictcode, common,
                                                                fastdrug, drugkind, touchcase, orderkind, spdrug, g34_max_appqty)
                                            select :chk_no as chk_no, b.mmcode as mmcode, b.mmname_c as mmname_c, b.mmname_e as mmname_e, b.base_unit as base_unit, b.m_contprice as m_contprice, a.wh_no as wh_no, 
                                                    (select listagg(store_loc, ', ') within group (order by store_loc) from MI_WLOCINV where wh_no = a.wh_no and mmcode = a.mmcode) as store_loc, 
                                                    b.mat_class as mat_class, b.m_storeid as m_storeid, a.inv_qty as store_qtyc, 0 as store_qtym,  0 as store_qtys, 
                                                    a.inv_qty as chk_qty, '' as chk_remark, '' as chk_uid, '' as chk_time, '1' as status_ini,
                                                     sysdate as create_date, 'BATCH' as create_user, sysdate as update_time, '' as update_user, '' as update_ip,
                                                    nvl(d.inv_qty, 0), a.apl_inqty, a.apl_outqty, a.trn_inqty, a.trn_outqty, a.adj_inqty, a.adj_outqty,
                                                    a.bak_inqty, a.bak_outqty, a.rej_outqty, a.dis_outqty, a.exg_inqty, a.exg_outqty, a.mil_inqty, a.mil_outqty, a.use_qty,
                                                    b.mat_class_sub, b.disc_cprice, nvl(c.cont_price, 0), nvl(c.disc_cprice, 0), 
                                                    (select war_qty from MI_WHCOST where data_ym = a.data_ym and mmcode = a.mmcode), 
                                                    c.war_qty,
                                                     b.unitrate, 
                                                     b.m_contid, b.e_sourcecode, b.e_restrictcode, b.common,
                                                    b.fastdrug, b.drugkind, b.touchcase, b.orderkind, b.spdrug,
                                                    (select g34_max_appqty from MI_BASERO_14 where mmcode = a.mmcode and ro_whtype = '1')
                                            from  MI_MAST_MON b, MI_WINVMON a
                                            left join MI_WINVMON d on (d.data_ym = twn_pym(a.data_ym) and d.mmcode = a.mmcode and d.wh_no = a.wh_No)
                                            left join MI_WHCOST c on (c.data_ym = twn_pym(a.data_ym) and c.mmcode = a.mmcode)
                                            where 1=1
                                            and b.data_ym = :chk_ym and b.data_ym = a.data_ym
                                            and (nvl(b.cancel_id,'N')='N' or nvl(e_orderdcflag,'N')='N')
                                            and a.wh_no = :wh_no and a.mmcode = b.mmcode
                                            and not (a.apl_inqty = 0              
                                                   and nvl(d.inv_qty, 0) = 0   
                                                   and a.inv_qty = 0           
                                                   and a.trn_inqty = 0        
                                                   and a.trn_outqty = 0        
                                                   and a.adj_inqty = 0        
                                                   and a.adj_outqty = 0        
                                                   and a.bak_outqty = 0)");

            return DBWork.Connection.Execute(sql,
                new
                {
                    chk_no = mast.CHK_NO,
                    chk_ym = mast.CHK_YM,
                    wh_no = mast.CHK_WH_NO,
                },
                DBWork.Transaction);
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
        public int UpdateMaster(CHK_MAST mast)
        {
            string sql = "update chk_mast set CHK_TOTAL = (select count(*) from CHK_DETAIL where chk_no = :chk_no) where chk_no = :chk_no";
            return DBWork.Connection.Execute(sql, new { chk_no = mast.CHK_NO }, DBWork.Transaction);
        }

        // 判斷是否為月結日隔天，是則繼續執行，否則結束
        public string IsOpenDay()
        {
            string sql = string.Format(@" select NVL((select 1
                                            from (select set_ctime 
                                                    from (select set_ctime 
                                                            from (select set_ym, twn_date(set_ctime+1) as set_ctime from MI_MNSET where set_status = 'C' order by set_ym desc)
                                                   where rownum=1)
                                                  )
                                            where set_ctime = twn_date(sysdate)),0) from dual");
            return DBWork.Connection.QueryFirst<string>(sql);
        }

        #region 小額採購

        public IEnumerable<ChkWh> GetWhno3s(string preym)
        {
            string sql = @"select a.wh_no as wh_no,
                                  b.wh_grade as wh_grade,
                                  count(*) as chk_total,
                                  b.inid as inid
                             from MI_WHMAST b, MI_MAST c, MI_WHINV a
                             left join MI_WINVMON d on (a.wh_no = d.wh_no and a.mmcode = d.mmcode and d.data_ym = :preym)
                            where b.wh_no = a.wh_no
                              and c.mmcode = a.mmcode
                              and b.wh_kind = '1'
                              and c.mat_class = '02'
                              and b.wh_grade in ('2', '3', '4')
                              and b.cancel_id = 'N'
                              and c.m_contid = '3'
                              and not (a.apl_inqty = 0              -- 申請入庫 = 0
                                        and nvl(d.inv_qty, 0) = 0   -- 上期結存 = 0
                                        and a.inv_qty = 0           -- 庫存量 = 0
                                        and a.trn_inqty = 0         -- 調撥入庫 = 0
                                        and a.trn_outqty = 0        -- 調撥出庫 = 0
                                        and a.adj_inqty = 0         -- 調帳入庫 = 0
                                        and a.adj_outqty = 0        -- 調帳出庫 = 0
                                        and a.bak_outqty = 0        -- 繳回出庫 = 0
                                      )
                           having count(*) > 0
                            group by a.wh_no, b.wh_grade, b.inid
                            order by a.wh_no, b.wh_grade, b.inid";

            return DBWork.Connection.Query<ChkWh>(sql, new { preym = preym }, DBWork.Transaction);
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
                              from  MI_MAST b, MI_WHINV a
                              left join MI_WINVMON c on (a.wh_no = c.wh_no and a.mmcode = c.mmcode and c.data_ym = :preym)
                             where a.wh_no = :wh_no
                               and a.mmcode = b.mmcode
                               and b.mat_class = '02'
                               and not (a.apl_inqty = 0              -- 申請入庫 = 0
                                        and nvl(c.inv_qty, 0) = 0   -- 上期結存 = 0
                                        and a.inv_qty = 0           -- 庫存量 = 0
                                        and a.trn_inqty = 0         -- 調撥入庫 = 0
                                        and a.trn_outqty = 0        -- 調撥出庫 = 0
                                        and a.adj_inqty = 0         -- 調帳入庫 = 0
                                        and a.adj_outqty = 0        -- 調帳出庫 = 0
                                        and a.bak_outqty = 0        -- 繳回出庫 = 0
                                      )
                               and b.m_contid = '3'";



            return DBWork.Connection.Execute(sql,
                new
                {
                    wh_no = mast.CHK_WH_NO,
                    storeid = mast.CHK_TYPE,
                    chk_no = mast.CHK_NO
                },
                DBWork.Transaction);
        }

        #endregion

        #region 更新CHK_MNSET
        public int UpdateChkmnset(string chk_no)
        {
            string sql = @"update CHK_MNSET set set_ctime = (select set_ctime+1 from MI_MNSET where set_ym = :p_chk_no), 
                                            set_atime = sysdate, set_status = 'Y', update_time = sysdate, 
                                            update_user = 'BATCH' 
                            where set_status = 'N' and chk_ym = :p_chk_no";
            return DBWork.Connection.Execute(sql, new { p_chk_no = chk_no }, DBWork.Transaction);
        }

        public int InsertChkmnset(string chk_ym)
        {
            string sql = @"insert into CHK_MNSET(chk_ym, set_ctime, set_status, set_atime, create_date, create_user, update_time, update_user)
                           select set_ym, set_ctime+1, 'Y', ,sysdate, sysdate, 'BATCH', sysdate, 'BATCH'
                             from MI_MNSET where set_ym=:chk_ym ";
            return DBWork.Connection.Execute(sql, DBWork.Transaction);
        }
        #endregion

        #region 開單前呼叫stored procedure

        public SP_MODEL PostUack(string set_ym, string wh_no)
        {
            var p = new OracleDynamicParameters();
            p.Add("I_YM", value: set_ym, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 21);
            p.Add("I_WHNO", value: wh_no, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 21);

            p.Add("O_RETID", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 1);
            p.Add("O_RETMSG", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 200);

            DBWork.Connection.Query("INV_SET.POST_UACK_MM2", p, commandType: CommandType.StoredProcedure);
            string retid = p.Get<OracleString>("O_RETID").Value;
            string errmsg = p.Get<OracleString>("O_RETMSG").Value;

            SP_MODEL sp = new SP_MODEL
            {
                O_RETID = retid,
                O_ERRMSG = errmsg
            };
            return sp;
        }

        public IEnumerable<string> GetWhNosGrade2()
        {
            string sql = @"
                select wh_no from MI_WHMAST
                 where wh_kind in ('0','1')
                   and wh_grade in ('2', '3', '4')
                   and cancel_id = 'N'
            ";
            return DBWork.Connection.Query<string>(sql, DBWork.Transaction);
        }

        #endregion

        #region 2020-07-09 新增: 本月新品項加入盤點單
        public int InsertDetailNoPre(CHK_MAST mast, string preym)
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
                              from MI_WHINV a, MI_MAST b
                             where a.wh_no = :wh_no
                               and b.m_storeid = :storeid
                               and a.mmcode = b.mmcode
                               and b.mat_class = '02'
                               and b.m_contid <> '3'
                               and a.APL_INQTY > 0
                               and not exists (select 1 from MI_WINVMON where wh_no = a.wh_no and mmcode = a.mmcode and data_ym = :preym)");

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

        public int InsertDetailNoPreType3(CHK_MAST mast, string preym)
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
                              from MI_WHINV a, MI_MAST b
                             where a.wh_no = :wh_no
                               and a.mmcode = b.mmcode
                               and b.mat_class = '02'
                               and b.m_contid = '3'
                               and a.APL_INQTY > 0
                               and not exists (select 1 from MI_WINVMON where wh_no = a.wh_no and mmcode = a.mmcode and data_ym = :preym)");

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
        #endregion

        public COMBO_MODEL GetMnset() {
            string sql = @"
                select * from (
                    select set_ym as value, twn_date(set_btime) as extra1, twn_date(set_ctime) as extra2
                      from MI_MNSET 
                     where set_status ='C'
                     order by set_ym desc
                )
                where rownum=1
            ";
            return DBWork.Connection.QueryFirstOrDefault<COMBO_MODEL>(sql, DBWork.Transaction);
        }

        public IEnumerable<DGMISS_ITEM> GetDgmissItem(string start_date, string end_date, string set_ym) {
            string sql = @"
                 select a.app_inid, a.mmcode, (nvl(b.chk_qty, 0) + nvl(a.apvqty, 0)) as inv_qty
                   from (
                  select APP_INID, MMCODE, sum(apvqty) as apvqty
                   from DGMISS 
                   where twn_date(apvtime) >= :start_date and twn_date(apvtime) <= :end_date
                   group by app_inid, mmcode
                   ) a
                    left join DGMISS_CHK b on (a.app_inid = b.inid and a.mmcode= b.mmcode and b.data_ym=twn_pym(:set_ym))
                union
                select a.inid as app_inid, a.mmcode, a.chk_qty as inv_qty
                  from DGMISS_CHK a
                 where not exists (select 1 from DGMISS 
                                   where app_inid = a.inid and mmcode = a.mmcode 
                                     and twn_date(apvtime) >= :start_date and twn_date(apvtime) <= :end_date )
                   and a.chk_qty <> 0
                   and a.data_ym=twn_pym(:set_ym)  
            ";
            return DBWork.Connection.Query<DGMISS_ITEM>(sql, new { start_date, end_date, set_ym }, DBWork.Transaction);
        }

        public int InsertDgmissChk(DGMISS_ITEM item) {
            string sql = @"
                insert into DGMISS_CHK(data_ym, inid, mmcode, inv_qty,
                                       create_time, create_user, update_time, update_user, update_ip)
                values (:DATA_YM, :APP_INID, :MMCODE, to_number(nvl(:INV_QTY, '0')), 
                    sysdate, 'SYSTEM', sysdate, 'SYSTEM', :IP
                )
            ";
            return DBWork.Connection.Execute(sql, item, DBWork.Transaction);
        }

        #region CHK_DETAILTOT

        public IEnumerable<CE0040> GetChkMast(string set_ym)
        {
            var p = new DynamicParameters();

            string sql = @" SELECT CHK_NO CHK_NO,
                                   CHK_WH_NO CHK_WH_NO, 
                                   CHK_WH_KIND CHK_WH_KIND, 
                                   CHK_WH_GRADE CHK_WH_GRADE, 
                                   WH_NAME(CHK_WH_NO) WH_NAME 
                            FROM CHK_MAST a
                            WHERE CHK_YM = :SET_YM
                              and not exists (select 1 from CHK_DETAILTOT where chk_no = a.chk_no)";

            p.Add(":SET_YM", set_ym);
            return DBWork.Connection.Query<CE0040>(sql, p, DBWork.Transaction);
        }

        //新增至CHK_DETAILTOT
        public int AddChkDetailtot(CE0040 item)
        {
            string sql = @"    INSERT INTO CHK_DETAILTOT(CHK_NO, MMCODE, MMNAME_C, MMNAME_E, BASE_UNIT, M_CONTPRICE, WH_NO, STORE_LOC,
                                                         MAT_CLASS, M_STOREID, STORE_QTY, STORE_QTYC, GAP_T, 
                                                         GAP_C, 
                                                         PRO_LOS_QTY, 
                                                         MISS_PER, APL_OUTQTY, CHK_QTY1, STATUS_TOT, CREATE_DATE, CREATE_USER, 
                                                         UPDATE_TIME, UPDATE_USER, UPDATE_IP, STORE_QTY1, CONSUME_QTY, CONSUME_AMOUNT)
                               SELECT CD.CHK_NO, CD.MMCODE, CD.MMNAME_C, CD.MMNAME_E, CD.BASE_UNIT, CD.M_CONTPRICE, CD.WH_NO, CD.STORE_LOC, 
                                      CD.MAT_CLASS, CD.M_STOREID, CD.STORE_QTYC, CD.STORE_QTYC, (CD.CHK_QTY - CD.STORE_QTYC),
                                      (CD.CHK_QTY - CD.STORE_QTYC), 
                                      (CASE WHEN :CHK_WH_GRADE = 1 THEN (CD.CHK_QTY - CD.STORE_QTYC)
                                            WHEN :CHK_WH_GRADE = 2 AND :CHK_WH_KIND = 0 THEN (CD.CHK_QTY - CD.STORE_QTYC) 
                                            WHEN :WH_NAME ='供應中心' THEN (CD.CHK_QTY - CD.STORE_QTYC) ELSE 0 END),
                                      0, CD.APL_OUTQTY, CD.CHK_QTY, '1', SYSDATE, :CREATE_USER, 
                                      SYSDATE, :UPDATE_USER, :UPDATE_IP, CD.STORE_QTYC, 0, 0
                               FROM CHK_DETAIL CD 
                               WHERE CD.CHK_NO = :CHK_NO 
                                 and not exists(select 1 from CHK_DETAILTOT where chk_no = cd.chk_no)";
            return DBWork.Connection.Execute(sql, item, DBWork.Transaction);
        }

        #endregion

        #region
        public bool CheckChkmnsetExists(string chk_ym) {
            string sql = @"
                select 1 from CHK_MNSET where chk_ym=:chk_ym
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { chk_ym }, DBWork.Transaction) != null;
        }

        public string GetHospCode() {
            string sql = @"
                select data_value from PARAM_D where grp_code='HOSP_INFO' and data_name='HospCode'
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }

        public int UpdateChkDetailUse(string chk_no) {
            string sql = @"
                update CHK_DETAIL
                   set chk_qty = 0, use_qty = store_qtyc
                 where chk_no = :chk_no
            ";
            return DBWork.Connection.Execute(sql, new { chk_no }, DBWork.Transaction);
        }

        public int UpdateMiwinvmonUse(string set_ym, string wh_no, string chk_no) {
            string sql = @"
                update MI_WINVMON a
                   set inv_qty = 0, use_qty = inv_qty
                 where data_ym = :set_ym and wh_no = :wh_no
                   and exists (select 1 from CHK_DETAIL where chk_no = :chk_no and mmcode = a.mmcode)
            ";
            return DBWork.Connection.Execute(sql, new { set_ym, wh_no, chk_no }, DBWork.Transaction);
        }

        public int InsertMiwhtrns(string wh_no, string chk_no) {
            string sql = @"
                INSERT INTO MI_WHTRNS (WH_NO, TR_DATE, TR_SNO, MMCODE, TR_INV_QTY, TR_ONWAY_QTY, TR_DOCNO, TR_IO, TR_MCODE, 
                                                    BF_TR_INVQTY, AF_TR_INVQTY)
                             SELECT A.WH_NO, SYSDATE, WHTRNS_SEQ.NEXTVAL, A.MMCODE,  (-1) * nvl(a.use_qty, 0), 0, A.CHK_NO, 'I', 'CHIO', 
                                    b.INV_QTY, b.INV_QTY - nvl(a.use_qty, 0)
                             FROM CHK_DETAIL A , MI_WHINV b
                             WHERE A.CHK_NO = :chk_no  
                             and b.wh_no = :wh_no
                             and b.mmcode = a.mmcode
            ";
            return DBWork.Connection.Execute(sql, new { wh_no, chk_no }, DBWork.Transaction);
        }

        public int UpdateMiwhinv(string wh_no, string chk_no) {
            string sql = @"
                UPDATE MI_WHINV a
                   SET INV_QTY = 0
                 WHERE WH_NO = :wh_no 
                   and exists (select 1 from CHK_DETAIL where chk_no = :chk_no and mmcode = a.mmcode)
            ";
            return DBWork.Connection.Execute(sql, new { wh_no, chk_no }, DBWork.Transaction);
        }
        #endregion

        #region 批號效期
        public IEnumerable<string> GetChkDetailMmcodes(string chk_no) {
            string sql = @"
                select mmcode from CHK_DETAIL where chk_no = :chk_no
            ";
            return DBWork.Connection.Query<string>(sql, new { chk_no }, DBWork.Transaction);
        }
        public IEnumerable<CHK_EXPLOC> GetExpinvs(string chk_no, string wh_no, string mmcode) {
            string sql = @"
                select * from (
                    select b.chk_no, b.wh_no, b.mmcode, twn_date(a.exp_date) as exp_date, a.lot_no, a.inv_qty as ori_inv_qty,
                           b.use_qty,
                           (case when twn_date(a.exp_date) < twn_sysdate then 'Y' else 'N' end) as isexpired
                      from CHK_DETAIL b, MI_WEXPINV a 
                     where b.chk_no = :chk_no
                       and b.mmcode = :mmcode
                       and b.wh_no = a.wh_no and a.mmcode = b.mmcode
                )
                order by mmcode, exp_date, lot_no
            ";
            return DBWork.Connection.Query<CHK_EXPLOC>(sql, new { chk_no, wh_no, mmcode }, DBWork.Transaction);
        }
        public string GetUseQty(string chk_no, string mmcode) {
            string sql = @"
                select use_qty from CHK_DETAIL where chk_no=:chk_no and mmcode = :mmcode
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { chk_no, mmcode }, DBWork.Transaction);
        }
        public string GetExpInvDiff(string chk_no, string mmcode) {
            string sql = @"
                select a.store_qtyc - nvl((select sum(nvl(inv_qty, 0)) from MI_WEXPINV where wh_no = a.wh_no and mmcode = a.mmcode),0)
                  from CHK_DETAIL a
                  where chk_no=:chk_no and mmcode = :mmcode
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { chk_no, mmcode }, DBWork.Transaction);
        }
        public int MergeExpinv(string wh_no, string mmcode, string diff) {
            string sql = @"
                merge into MI_WEXPINV a
                using ( 
                  select :wh_no as wh_no, :mmcode as mmcode, '9991231' as exp_date, 'TMPLOT' as lot_no, 0 as inv_qty
                    from dual
                ) b 
                on (a.wh_no = b.wh_no and a.mmcode = b.mmcode and twn_date(a.exp_date) = b.exp_date and a.lot_no = b.lot_no)
                when matched then
                  update set a.inv_qty = a.inv_qty + :diff
                when not matched then
                  insert (wh_no, mmcode, exp_date, lot_no, inv_qty) 
                  values (:wh_no, :mmcode, twn_todate('9991231'), 'TMPLOT', :diff)
            ";
            return DBWork.Connection.Execute(sql, new { wh_no, mmcode, diff }, DBWork.Transaction);
        }

        public int InsertChkExpinvTrns(CHK_EXPLOC exp) {
            string sql = @"
                insert into CHK_EXPINV_TRNS (chk_no, wh_no, mmcode, exp_date, lot_no, trn_qty)
                values (:CHK_NO, :WH_NO, :MMCODE, twn_todate(:EXP_DATE), :LOT_NO, :TRN_QTY)
            ";
            return DBWork.Connection.Execute(sql, exp, DBWork.Transaction);
        }


        public bool CheckMiwexpinvExists(CHK_EXPLOC exp) {
            string sql = @"
                select 1 from MI_WEXPINV
                 where wh_no = :WH_NO and mmcode = :MMCODE
                   and exp_date = twn_todate(:EXP_DATE)
                   and lot_no = :LOT_NO
            ";
            return DBWork.Connection.ExecuteScalar(sql, exp, DBWork.Transaction) != null;
        }
        public int UpdateMiwexpinv(CHK_EXPLOC exp) {
            string sql = @"
                update MI_WEXPINV
                   set inv_qty = inv_qty - to_number(:TRN_QTY)
                 where wh_no = :WH_NO and mmcode = :MMCODE
                   and exp_date = twn_todate(:EXP_DATE)
                   and lot_no = :LOT_NO
            ";

            return DBWork.Connection.Execute(sql, exp, DBWork.Transaction);
        }
        public int InsertMiwexpinv(CHK_EXPLOC exp) {
            string sql = @"
                insert into MI_WEXPINV (wh_no, mmcode, exp_date, lot_no, inv_Qty)
                values (:WH_NO, :MMCODE, twn_todate(:EXP_DATE), :LOT_NO, (-1) * :TRN_QTY )
            ";
            return DBWork.Connection.Execute(sql, exp, DBWork.Transaction);
        }
        #endregion

        #region 儲位

        public IEnumerable<CHK_EXPLOC> GetLocinvs(string chk_no, string wh_no, string mmcode)
        {
            string sql = @"
                select b.chk_no, b.wh_no, b.mmcode, a.store_loc, a.inv_qty as ori_inv_qty,
                       b.use_qty
                  from CHK_DETAIL b, MI_WLOCINV a
                 where b.chk_no = :chk_no
                   and b.mmcode = :mmcode
                   and b.wh_no = a.wh_no and a.mmcode = b.mmcode
                 order by b.mmcode, a.store_loc
            ";
            return DBWork.Connection.Query<CHK_EXPLOC>(sql, new { chk_no, wh_no, mmcode }, DBWork.Transaction);
        }
        public string GetLocInvDiff(string chk_no, string mmcode)
        {
            string sql = @"
                select a.store_qtyc - nvl((select sum(nvl(inv_qty, 0)) from MI_WLOCINV where wh_no = a.wh_no and mmcode = a.mmcode),0)
                  from CHK_DETAIL a
                  where chk_no=:chk_no and mmcode = :mmcode
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { chk_no, mmcode }, DBWork.Transaction);
        }
        public int MergeLocinv(string wh_no, string mmcode, string diff)
        {
            string sql = @"
                merge into MI_WLOCINV a
                using ( 
                  select :wh_no as wh_no, :mmcode as mmcode, 'TMPLOC' as store_loc, 0 as inv_qty
                    from dual
                ) b 
                on (a.wh_no = b.wh_no and a.mmcode = b.mmcode and a.store_loc = b.store_loc)
                when matched then
                  update set a.inv_qty = a.inv_qty + :diff
                when not matched then
                  insert (wh_no, mmcode, store_loc, inv_qty) 
                  values (:wh_no, :mmcode, 'TMPLOC', :diff)
            ";
            return DBWork.Connection.Execute(sql, new { wh_no, mmcode, diff }, DBWork.Transaction);
        }

        public int InsertChkLocinvTrns(CHK_EXPLOC exp)
        {
            string sql = @"
                insert into CHK_LOCINV_TRNS (chk_no, wh_no, mmcode, store_loc, trn_qty)
                values (:CHK_NO, :WH_NO, :MMCODE, :STORE_LOC, :TRN_QTY)
            ";
            return DBWork.Connection.Execute(sql, exp, DBWork.Transaction);
        }
        public bool CheckMiwlocinvExists(CHK_EXPLOC exp)
        {
            string sql = @"
                select 1 from MI_WLOCINV
                 where wh_no = :WH_NO and mmcode = :MMCODE
                   and store_loc = :STORE_LOC
            ";
            return DBWork.Connection.ExecuteScalar(sql, exp, DBWork.Transaction) != null;
        }
        public int UpdateMiwlocinv(CHK_EXPLOC exp)
        {
            string sql = @"
                update MI_WLOCINV
                   set inv_qty = inv_qty - to_number(:TRN_QTY)
                 where wh_no = :WH_NO and mmcode = :MMCODE
                   and store_loc = :STORE_LOC
            ";

            return DBWork.Connection.Execute(sql, exp, DBWork.Transaction);
        }
        public int InsertMiwlocinv(CHK_EXPLOC exp)
        {
            string sql = @"
                insert into MI_WLOCINV (wh_no, mmcode, store_loc, inv_qty)
                values (:WH_NO, :MMCODE, :STORE_LOC, (-1) * :TRN_QTY)
            ";
            return DBWork.Connection.Execute(sql, exp, DBWork.Transaction);
        }
        #endregion
    }
}
