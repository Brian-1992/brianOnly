using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using WebApp.Models;
using WebApp.Models.C;

namespace WebApp.Repository.C
{
    public class CE0016Repository : JCLib.Mvc.BaseRepository
    {
        public CE0016Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        #region master
        public IEnumerable<CE0003> GetMasters(string wh_no, string date, string userId, string chk_level)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT CHK_WH_NO, 
                               (SELECT WH_NAME FROM MI_WHMAST WHERE WH_NO = a.CHK_WH_NO) as WH_NAME, 
                               a.CHK_YM, 
                               a.CHK_WH_GRADE as CHK_WH_GRADE_CODE,
                               a.CHK_WH_KIND as CHK_WH_KIND_CODE,
                               a.CHK_PERIOD as CHK_PERIOD_CODE,
                               a.CHK_TYPE as CHK_TYPE_CODE,
                               a.CHK_STATUS as CHK_STATUS_CODE,
                               a.CREATE_USER as CREATE_USER,
                               (SELECT DISTINCT  DATA_DESC as COMBITEM FROM PARAM_D 
                               WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_WH_GRADE' AND DATA_VALUE =A.CHK_WH_GRADE) CHK_WH_GRADE, 
                               (SELECT DISTINCT  DATA_DESC as COMBITEM FROM PARAM_D 
                               WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_WH_KIND' AND DATA_VALUE =A.CHK_WH_KIND) CHK_WH_KIND, 
                               (SELECT DISTINCT  DATA_DESC as COMBITEM FROM PARAM_D 
                               WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_PERIOD' AND DATA_VALUE =A.CHK_PERIOD) CHK_PERIOD, 
                               a.CHK_TYPE,a.CHK_NUM || '/' || a.CHK_TOTAL as MERGE_NUM_TOTAL, a.CHK_NO, 
                               (SELECT UNA FROM UR_ID WHERE TUSER = a.CHK_KEEPER) CHK_KEEPER, 
                               (SELECT DISTINCT  DATA_DESC as COMBITEM FROM PARAM_D 
                               WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_STATUS' AND DATA_VALUE =A.CHK_STATUS) CHK_STATUS,
                               (select UPDN_STATUS from CHK_GRADE2_UPDN where CHK_NO = a.chk_no and chk_uid = :userid) as updn_status,
                               a.chk_no1
                          from CHK_MAST a 
                         where 1=1 
                           and a.chk_level = :chk_level ";

            if (wh_no != "" && wh_no != null)
            {
                sql += " AND a.chk_wh_no LIKE :wh_no ";
                p.Add(":wh_no", string.Format("%{0}%", wh_no));
            }
            if (date != "" && date != null)
            {
                sql += " AND a.chk_ym like :chk_ym ";
                p.Add(":chk_ym", string.Format("%{0}%", date));
                //p.Add(":p1", date);
            }
            sql += @"and  chk_no  in  (select DISTINCT chk_no from CHK_DETAIL where CHK_UID =  :userid 
                                    union 
                                       select distinct chk_no from CHK_NOUID where chk_uid = :userid
                                    union 
                                       select chk_no  from  chk_mast where CHK_KEEPER  in ( select  distinct inid  from ur_id  where tuser  =  :userid)
                                      )";
            //藥庫 union這個人的責任中心 MI_WHMAST INID; 人的UR_ID TUSER INID欄位庫房代碼與此盤點單的庫房代碼的類別 (master部份)

            p.Add(":userid", userId);
            p.Add(":chk_level", chk_level);

            return DBWork.PagingQuery<CE0003>(sql, p, DBWork.Transaction);
        }
        #endregion

        public IEnumerable<CE0003> GetDetails(string chk_no, string mmcodeorstore, string chk_time_status)
        {
            var p = new DynamicParameters();

            var sql = @"
                        with set_times as (
                            select set_ym as set_ym, 
                                   twn_date(set_btime) as set_btime, 
                                   twn_date(set_ctime) as set_ctime
                              from MI_MNSET
                             where (select create_date 
                                      from CHK_MAST where chk_no = :p0) 
                                   between set_btime and nvl(set_ctime, sysdate)
                               and rownum = 1
                             order by set_ym desc
                        )
                        select a.chk_no, 
                               a.wh_no, 
                               a.mmcode, a.mmname_c, a.mmname_e, a.STORE_LOC,
                               a.LOC_NAME, a.BASE_UNIT, 
                               (select m_trnid from MI_MAST where mmcode = a.mmcode) as m_trnid, 
                               (case
                                    when (b.set_ym = '10908' and 
                                          (select chk_wh_kind from CHK_MAST where chk_no = :p0) = '0')
                                        then
                                        NVL(TO_CHAR((select inv_qty from MI_WHINV_INIT 
                                             where wh_no = a.wh_no 
                                               and mmcode = a.mmcode)),' ')
                                    else 
                                        NVL(TO_CHAR((select inv_qty from MI_WINVMON 
                                             where data_ym = TWN_PYM((select substr(chk_ym,1,5) from CHK_MAST where chk_no = a.chk_no))
                                               and wh_no = a.wh_no 
                                               and mmcode = a.mmcode)),' ')
                                end) as pre_inv_qty,
                               NVL((case 
                                    when (TWN_YYYMM(sysdate) = (select SUBSTR(chk_ym, 1,5) from CHK_MAST where chk_no = a.chk_no))
                                        then (select TO_CHAR(apl_inqty) from MI_WHINV where wh_no = a.wh_no and mmcode = a.mmcode)
                                    else 
                                        (select TO_CHAR(apl_inqty) from MI_WINVMON 
                                          where data_ym = (select substr(chk_ym,1,5) from CHK_MAST where chk_no = a.chk_no)
                                            and wh_no = a.wh_no 
                                            and mmcode = a.mmcode)
                               end),' ') as apl_inqty, 
                               NVL((case 
                                    when (TWN_YYYMM(sysdate) = (select SUBSTR(chk_ym, 1,5) from CHK_MAST where chk_no = a.chk_no))
                                        then (select TO_CHAR(apl_outqty) from MI_WHINV where wh_no = a.wh_no and mmcode = a.mmcode)
                                    else 
                                        (select TO_CHAR(apl_outqty) from MI_WINVMON 
                                          where data_ym = (select substr(chk_ym,1,5) from CHK_MAST where chk_no = a.chk_no)
                                            and wh_no = a.wh_no 
                                            and mmcode = a.mmcode)
                               end),' ') as apl_outqty, 
                               NVL((case 
                                    when (TWN_YYYMM(sysdate) = (select SUBSTR(chk_ym, 1,5) from CHK_MAST where chk_no = a.chk_no))
                                        then (select TO_CHAR(bak_outqty) from MI_WHINV where wh_no = a.wh_no and mmcode = a.mmcode)
                                    else 
                                        (select TO_CHAR(bak_outqty) from MI_WINVMON 
                                          where data_ym = (select substr(chk_ym,1,5) from CHK_MAST where chk_no = a.chk_no)
                                            and wh_no = a.wh_no 
                                            and mmcode = a.mmcode)
                               end),' ') as bak_outqty, 
                               NVL((case 
                                    when (TWN_YYYMM(sysdate) = (select SUBSTR(chk_ym, 1,5) from CHK_MAST where chk_no = a.chk_no))
                                        then (select TO_CHAR(TRN_INQTY) from MI_WHINV where wh_no = a.wh_no and mmcode = a.mmcode)
                                    else 
                                        (select TO_CHAR(TRN_INQTY) from MI_WINVMON 
                                          where data_ym = (select substr(chk_ym,1,5) from CHK_MAST where chk_no = a.chk_no)
                                            and wh_no = a.wh_no 
                                            and mmcode = a.mmcode)
                               end),' ') as TRN_INQTY, 
                               NVL((case 
                                    when (TWN_YYYMM(sysdate) = (select SUBSTR(chk_ym, 1,5) from CHK_MAST where chk_no = a.chk_no))
                                        then (select TO_CHAR(TRN_OUTQTY) from MI_WHINV where wh_no = a.wh_no and mmcode = a.mmcode)
                                    else 
                                        (select TO_CHAR(TRN_OUTQTY) from MI_WINVMON 
                                          where data_ym = (select substr(chk_ym,1,5) from CHK_MAST where chk_no = a.chk_no)
                                            and wh_no = a.wh_no 
                                            and mmcode = a.mmcode)
                               end),' ') as TRN_OUTQTY, 
                               NVL((case 
                                    when (TWN_YYYMM(sysdate) = (select SUBSTR(chk_ym, 1,5) from CHK_MAST where chk_no = a.chk_no))
                                        then (select TO_CHAR(ADJ_INQTY) from MI_WHINV where wh_no = a.wh_no and mmcode = a.mmcode)
                                    else 
                                        (select TO_CHAR(ADJ_INQTY) from MI_WINVMON 
                                          where data_ym = (select substr(chk_ym,1,5) from CHK_MAST where chk_no = a.chk_no)
                                            and wh_no = a.wh_no 
                                            and mmcode = a.mmcode)
                               end),' ') as ADJ_INQTY, 
                               NVL((case 
                                    when (TWN_YYYMM(sysdate) = (select SUBSTR(chk_ym, 1,5) from CHK_MAST where chk_no = a.chk_no))
                                        then (select TO_CHAR(ADJ_OUTQTY) from MI_WHINV where wh_no = a.wh_no and mmcode = a.mmcode)
                                    else 
                                        (select TO_CHAR(ADJ_OUTQTY) from MI_WINVMON 
                                          where data_ym = (select substr(chk_ym,1,5) from CHK_MAST where chk_no = a.chk_no)
                                            and wh_no = a.wh_no 
                                            and mmcode = a.mmcode)
                               end),' ') as ADJ_OUTQTY, 
                               (select nvl(sum(c.consume_qty), 0)
                                  from MI_CONSUME_DATE c
                                 where c.data_date between b.set_btime and b.set_ctime
                                   and c.wh_no = a.wh_no 
                                   and c.mmcode = a.mmcode) as use_qty,
                                (case
                                     when (a.chk_time is null or a.store_qty_update_time is null)
                                         then a.store_qtyc
                                     else a.store_qty_n
                                 end) as store_qtyc,
                                (case
                                     when (a.wh_no = 'PH1S' and a.chk_time is null)
                                         then store_qtyc
                                     else CHK_QTY
                                 end) as CHK_QTY,
                                 (case
                                    when (a.chk_time is null)
                                         then ' '
                                    when (a.store_qty_update_time is null)
                                         then TO_CHAR(a.chk_qty - a.store_qtyc)
                                    else TO_CHAR(a.chk_qty - a.store_qty_n)
                                  end) as DIFF_QTY,
                                 a.chk_uid, 
                                 (case
                                    when (a.chk_uid is null )
                                        then ' '
                                    else (select una from UR_ID where tuser = a.chk_uid)
                                  end) as chk_uid_name,
                                 a.chk_remark, a.chk_time, a.status_ini ,
                                 (case
                                     when (a.chk_time is null or a.store_qty_update_time is null)
                                         then a.store_qtyc
                                     else a.store_qty_n
                                 end) as ori_store_qtyc,
                               (select NVL(to_char(sum(consume_qty)),'0') 
                                  from MI_CONSUME_DATE
                                 where data_date between b.set_btime and b.set_ctime
                                   and wh_no = a.wh_no 
                                   and mmcode = a.mmcode) as ori_use_qty,
                               (select nvl(sum(tr_inv_qty), 0)
                                  from V_MN_BACK
                                 where data_ym = b.set_ym
                                   and wh_no = a.wh_no
                                   and mmcode = a.mmcode) as back_qty
                          from CHK_DETAIL a, set_times b
                         where 1=1 ";

            if (chk_no != "" && chk_no != null)
            {
                sql += " AND a.chk_no = :p0 ";
                p.Add(":p0", chk_no);
            }

            if (mmcodeorstore != "" && mmcodeorstore != null)
            {
                sql += " AND ( a.MMCODE LIKE :p2 OR a.STORE_LOC LIKE :p2 ) ";
                p.Add(":p2", string.Format("%{0}%", mmcodeorstore));
            }
            if (chk_time_status != string.Empty)
            {
                if (chk_time_status == "is")
                {
                    sql += string.Format(" and (a.chk_time {0} null or a.chk_qty {0} null)", chk_time_status);
                }
                else if (chk_time_status == "is not")
                {
                    sql += string.Format(" and (a.chk_time {0} null and a.chk_qty {0} null)", chk_time_status);
                }

            }

            return DBWork.PagingQuery<CE0003>(sql, p, DBWork.Transaction);
        }

        public int UpdateChkDetail(CHK_DETAIL detail)
        {
            string sql = @"update CHK_DETAIL a
                              set a.chk_qty = :chk_qty, a.chk_uid = :chk_uid, a.chk_time = sysdate, a.status_ini = :status_ini,
                                  a.update_time = sysdate, a.update_user = :update_user, a.update_ip = :update_ip,
                                  a.store_qty_n = (select inv_qty from MI_WHINV where wh_no = a.wh_no and mmcode = a.mmcode),
                                  a.store_qty_update_time = sysdate
                            where a.chk_no = :chk_no
                              and a.mmcode = :mmcode";
            return DBWork.Connection.Execute(sql, detail, DBWork.Transaction);
        }
        public int UpdateChkDetailCE(CHK_DETAIL detail, string wh_kind)
        {
            string sql = string.Format(@"
                           update CHK_DETAIL a
                              set a.chk_qty = :chk_qty, a.chk_uid = :chk_uid, a.chk_time = sysdate, a.status_ini = :status_ini,
                                  a.update_time = sysdate, a.update_user = :update_user, a.update_ip = :update_ip,
                                  a.store_qty_n = (select sum(inv_qty) from MI_WHINV
                                                  where wh_no  in (select wh_no from MI_WHMAST where wh_kind = '{0}')
                                                    and mmcode = a.mmcode
                                                  group by mmcode),
                                  a.store_qty_update_time = sysdate
                            where a.chk_no = :chk_no
                              and a.mmcode = :mmcode", wh_kind);
            return DBWork.Connection.Execute(sql, detail, DBWork.Transaction);
        }

        public string GetUndoneDetailCount(string chk_no)
        {
            string sql = @"select count(*) from CHK_DETAIL
                            where chk_no = :chk_no
                              and (chk_time is null 
                                    or chk_qty is null
                                  )";
            return DBWork.Connection.QueryFirst<string>(sql, new { chk_no = chk_no }, DBWork.Transaction);
        }
        public int UpdateChkDetailAll(string chk_no, string update_user, string update_ip)
        {
            string sql = @"update CHK_DETAIL 
                              set status_ini = '2', update_time = sysdate, 
                                  update_user= :update_user, update_ip = :update_ip
                            where chk_no = :chk_no";

            return DBWork.Connection.Execute(sql, new { chk_no = chk_no, update_user = update_user, update_ip = update_ip }, DBWork.Transaction);
        }
        public int UpdateChkmast(string chk_no, string update_user, string update_ip)
        {
            string sql = @"update CHK_MAST
                              set chk_status = '2', chk_num = chk_total, update_time = sysdate, 
                                  update_user= :update_user, update_ip = :update_ip
                            where chk_no = :chk_no";

            return DBWork.Connection.Execute(sql, new { chk_no = chk_no, update_user = update_user, update_ip = update_ip }, DBWork.Transaction);
        }

        public string GetChkym(string chk_no)
        {
            string sql = @"select chk_ym from CHK_MAST where chk_no = :chk_no";
            return DBWork.Connection.QueryFirst<string>(sql, new { chk_no = chk_no }, DBWork.Transaction);
        }


        #region 多筆盤點
        public IEnumerable<CE0003> GetChkDetailsMulti(string chk_nos, string userid, string mmcode, string chk_time_status)
        {
            string sql = string.Format(@"
                                        with set_times as (
                                              select set_ym as set_ym,
                                                     twn_date(set_btime) as set_btime, 
                                                     twn_date(set_ctime) as set_ctime
                                                from MI_MNSET a, CHK_MAST b
                                               where b.chk_no in ( {0} )
                                                 and b.create_date
                                                     between a.set_btime and a.set_ctime
                                                 and rownum = 1 
                                               order by set_ym desc
                                          )
                                         select a.CHK_NO, 
                                                a.WH_NO as CHK_WH_NO, 
                                                a.mmcode,nvl(a.MMNAME_C, ' ') as MMNAME_C,nvl(a.MMNAME_E, ' ') as MMNAME_E,a.STORE_LOC,a.LOC_NAME,a.BASE_UNIT,a.STORE_QTYC,
                                                (select m_trnid from MI_MAST where mmcode = a.mmcode) as m_trnid, 
                                                (select chk_type from CHK_MAST where chk_no = a.chk_no) as chk_type,
                                                (select chk_wh_kind from CHK_MAST where chk_no = a.chk_no) as chk_wh_kind_code,
                                                (case
                                                     when (b.set_ym = '10908' and 
                                                           (select chk_wh_kind from CHK_MAST where chk_no = a.chk_no) = '0')
                                                         then 
                                                            NVL(TO_CHAR((select inv_qty from MI_WHINV_INIT 
                                                                          where wh_no = a.wh_no 
                                                                            and mmcode = a.mmcode)),' ')
                                                     else 
                                                         NVL(TO_CHAR((select inv_qty from MI_WINVMON 
                                                              where data_ym = TWN_PYM((select substr(chk_ym,1,5) from CHK_MAST where chk_no = a.chk_no)) 
                                                                and wh_no = a.wh_no 
                                                                and mmcode = a.mmcode)),' ')
                                                 end) as pre_inv_qty,
                                                NVL((case 
                                                     when (TWN_YYYMM(sysdate) = (select SUBSTR(chk_ym, 1,5) from CHK_MAST where chk_no = a.chk_no))
                                                         then (select TO_CHAR(apl_inqty) from MI_WHINV where wh_no = a.wh_no and mmcode = a.mmcode)
                                                     else 
                                                         (select TO_CHAR(apl_inqty) from MI_WINVMON 
                                                           where data_ym = (select substr(chk_ym,1,5) from CHK_MAST where chk_no = a.chk_no) 
                                                             and wh_no = a.wh_no and mmcode = a.mmcode)
                                                end),' ') as apl_inqty, 
                                                NVL((case 
                                                    when (TWN_YYYMM(sysdate) = (select SUBSTR(chk_ym, 1,5) from CHK_MAST where chk_no = a.chk_no))
                                                        then (select TO_CHAR(apl_outqty) from MI_WHINV where wh_no = a.wh_no and mmcode = a.mmcode)
                                                    else 
                                                        (select TO_CHAR(apl_outqty) from MI_WINVMON 
                                                          where data_ym = (select substr(chk_ym,1,5) from CHK_MAST where chk_no = a.chk_no) 
                                                            and wh_no = a.wh_no 
                                                            and mmcode = a.mmcode)
                                                end),' ') as apl_outqty, 
                                                NVL((case 
                                                     when (TWN_YYYMM(sysdate) = (select SUBSTR(chk_ym, 1,5) from CHK_MAST where chk_no = a.chk_no))
                                                         then (select TO_CHAR(bak_outqty) from MI_WHINV where wh_no = a.wh_no and mmcode = a.mmcode)
                                                     else 
                                                         (select TO_CHAR(bak_outqty) from MI_WINVMON 
                                                           where data_ym = (select substr(chk_ym,1,5) from CHK_MAST where chk_no = a.chk_no) 
                                                             and wh_no = a.wh_no 
                                                             and mmcode = a.mmcode)
                                                end),' ') as bak_outqty, 
                                                NVL((case 
                                                     when (TWN_YYYMM(sysdate) = (select SUBSTR(chk_ym, 1,5) from CHK_MAST where chk_no = a.chk_no))
                                                         then (select TO_CHAR(TRN_INQTY) from MI_WHINV where wh_no = a.wh_no and mmcode = a.mmcode)
                                                     else 
                                                         (select TO_CHAR(TRN_INQTY) from MI_WINVMON 
                                                           where data_ym = (select substr(chk_ym,1,5) from CHK_MAST where chk_no = a.chk_no)
                                                             and wh_no = a.wh_no 
                                                             and mmcode = a.mmcode)
                                                end),' ') as TRN_INQTY, 
                                                NVL((case 
                                                     when (TWN_YYYMM(sysdate) = (select SUBSTR(chk_ym, 1,5) from CHK_MAST where chk_no = a.chk_no))
                                                         then (select TO_CHAR(TRN_OUTQTY) from MI_WHINV where wh_no = a.wh_no and mmcode = a.mmcode)
                                                     else 
                                                         (select TO_CHAR(TRN_OUTQTY) from MI_WINVMON 
                                                           where data_ym = (select substr(chk_ym,1,5) from CHK_MAST where chk_no = a.chk_no)
                                                             and wh_no = a.wh_no 
                                                             and mmcode = a.mmcode)
                                                end),' ') as TRN_OUTQTY, 
                                                NVL((case 
                                                     when (TWN_YYYMM(sysdate) = (select SUBSTR(chk_ym, 1,5) from CHK_MAST where chk_no = a.chk_no))
                                                         then (select TO_CHAR(ADJ_INQTY) from MI_WHINV where wh_no = a.wh_no and mmcode = a.mmcode)
                                                     else 
                                                         (select TO_CHAR(ADJ_INQTY) from MI_WINVMON 
                                                           where data_ym = (select substr(chk_ym,1,5) from CHK_MAST where chk_no = a.chk_no)
                                                             and wh_no = a.wh_no 
                                                             and mmcode = a.mmcode)
                                                end),' ') as ADJ_INQTY, 
                                                NVL((case 
                                                     when (TWN_YYYMM(sysdate) = (select SUBSTR(chk_ym, 1,5) from CHK_MAST where chk_no = a.chk_no))
                                                         then (select TO_CHAR(ADJ_OUTQTY) from MI_WHINV where wh_no = a.wh_no and mmcode = a.mmcode)
                                                     else 
                                                         (select TO_CHAR(ADJ_OUTQTY) from MI_WINVMON 
                                                           where data_ym = (select substr(chk_ym,1,5) from CHK_MAST where chk_no = a.chk_no)
                                                             and wh_no = a.wh_no 
                                                             and mmcode = a.mmcode)
                                                end),' ') as ADJ_OUTQTY, 
                                                (select sum(c.consume_qty) 
                                                   from MI_CONSUME_DATE c
                                                  where c.data_date between b.set_btime and b.set_ctime
                                                    and c.wh_no = a.wh_no 
                                                    and c.mmcode = a.mmcode) as use_qty,
                                                (case 
                                                    when (wh_no = 'PH1S' and chk_time is null)
                                                        then a.store_qtyc
                                                    else a.CHK_QTY
                                                  end) as CHK_QTY,
                                                 a.CHK_REMARK, a.CHK_TIME,a.STATUS_INI,
                                                 a.chk_uid, 
                                                (case
                                                    when (a.chk_uid is null)
                                                        then ' '
                                                    else 
                                                        (select una from UR_ID where tuser = a.chk_uid)
                                                end) as chk_uid_name,
                                               (case
                                                      when (a.chk_time is null or a.store_qty_update_time is null)
                                                          then a.store_qtyc
                                                      else a.store_qty_n
                                                  end) as ori_store_qtyc,
                                                (select NVL(to_char(sum(consume_qty)),'0') 
                                                   from MI_CONSUME_DATE
                                                  where data_date between b.set_btime and b.set_ctime
                                                    and wh_no = a.wh_no 
                                                    and mmcode = a.mmcode) as ori_use_qty,
                                                (select nvl(sum(tr_inv_qty), 0)
                                                   from V_MN_BACK
                                                  where data_ym = b.set_ym
                                                    and wh_no = a.wh_no
                                                    and mmcode = a.mmcode) as back_qty
                                           FROM CHK_DETAIL a, set_times b
                                          WHERE 1=1 
                                            and chk_no in ( {0} )", chk_nos);
            if (mmcode != string.Empty)
            {
                sql += string.Format("     and a.mmcode like '%{0}%'", mmcode);
            }
            if (chk_time_status != string.Empty)
            {
                sql += string.Format("     and a.chk_time {0} null", chk_time_status);
            }

            return DBWork.PagingQuery<CE0003>(sql, DBWork.Transaction);
        }
        #endregion

        #region  全部符合
        public string GetSetym(string chk_no) {
            string sql = @"select set_ym as set_ym
                              from MI_MNSET
                             where (select create_date 
                                      from CHK_MAST where chk_no = :chk_no) 
                                    between set_btime and nvl(set_ctime, sysdate) 
                               and rownum = 1 
                             order by set_ym desc";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql,new { chk_no = chk_no}, DBWork.Transaction);
        }

        public int UpdateChkQtyMatch(CHK_DETAIL detail, string set_ym)
        {// 盤點量 = 上月結存 + 本月入庫 - 本月出庫 - 繳回 - 醫令扣庫(=扣庫-退藥) - 待更新醫令扣庫數量
            string sql = string.Format(@"
                           update CHK_DETAIL a
                              set a.chk_uid = :chk_uid,
                                  a.chk_qty = (
                                                (case
                                                    when ({0} = '10908' and 
                                                          (select chk_wh_kind from CHK_MAST where chk_no = :chk_no) = '0')
                                                        then 
                                                            to_number(NVL(TO_CHAR((select inv_qty from MI_WHINV_INIT 
                                                                          where wh_no = a.wh_no 
                                                                            and mmcode = a.mmcode)),'0'))
                                                    else
                                                        to_number(NVL(TO_CHAR((select inv_qty from MI_WINVMON 
                                                              where data_ym = TWN_PYM((select substr(chk_ym,1,5) from CHK_MAST where chk_no = a.chk_no)) 
                                                                and wh_no = a.wh_no 
                                                                and mmcode = a.mmcode)),'0'))
                                                end)  + 
                                               to_number(NVL((case 
                                                    when (TWN_YYYMM(sysdate) = (select SUBSTR(chk_ym, 1,5) from CHK_MAST where chk_no = a.chk_no))
                                                        then (select TO_CHAR(apl_inqty) from MI_WHINV where wh_no = a.wh_no and mmcode = a.mmcode)
                                                    else 
                                                        (select TO_CHAR(apl_inqty) from MI_WINVMON 
                                                          where data_ym = (select substr(chk_ym,1,5) from CHK_MAST where chk_no = a.chk_no) 
                                                            and wh_no = a.wh_no 
                                                            and mmcode = a.mmcode)
                                               end),'0')) - 
                                               to_number(NVL((case 
                                                    when (TWN_YYYMM(sysdate) = (select SUBSTR(chk_ym, 1,5) from CHK_MAST where chk_no = a.chk_no))
                                                        then (select TO_CHAR(apl_outqty) from MI_WHINV where wh_no = a.wh_no and mmcode = a.mmcode)
                                                    else 
                                                        (select TO_CHAR(apl_outqty) from MI_WINVMON 
                                                          where data_ym = (select substr(chk_ym,1,5) from CHK_MAST where chk_no = a.chk_no) 
                                                            and wh_no = a.wh_no 
                                                            and mmcode = a.mmcode)
                                               end),'0')) - 
                                               to_number(NVL((case 
                                                    when (TWN_YYYMM(sysdate) = (select SUBSTR(chk_ym, 1,5) from CHK_MAST where chk_no = a.chk_no))
                                                        then (select TO_CHAR(bak_outqty) from MI_WHINV where wh_no = a.wh_no and mmcode = a.mmcode)
                                                    else 
                                                        (select TO_CHAR(bak_outqty) from MI_WINVMON 
                                                          where data_ym = (select substr(chk_ym,1,5) from CHK_MAST where chk_no = a.chk_no) 
                                                            and wh_no = a.wh_no 
                                                            and mmcode = a.mmcode)
                                               end),'0')) - 
                                               (
                                                  to_number(
                                                           (select nvl(sum(b.consume_qty), 0)
                                                              from MI_CONSUME_DATE b, MI_MNSET c
                                                             where c.set_ym = {0}
                                                               and b.data_date between twn_date(c.set_btime) and twn_date(c.set_ctime)
                                                               and b.wh_no = a.wh_no 
                                                               and b.mmcode = a.mmcode
                                                            )
                                                  ) - 
                                                  to_number(
                                                           (select nvl(sum(b.tr_inv_qty), 0)
                                                              from V_MN_BACK b
                                                             where b.data_ym = {0}
                                                               and b.wh_no = a.wh_no 
                                                               and b.mmcode = a.mmcode
                                                            )
                                                  ) 
                                                )- 
                                                nvl(a.his_consume_qty_t, 0)
                                            ),  
                                  a.chk_time = sysdate, 
                                  a.update_time = sysdate, 
                                  a.update_user = :update_user,
                                  a.update_ip = :update_ip,
                                  a.store_qty_n = (select inv_qty from MI_WHINV where wh_no = a.wh_no and mmcode = a.mmcode),
                                  a.store_qty_update_time = sysdate
                            where a.chk_no = :chk_no and a.mmcode = :mmcode", set_ym);

            return DBWork.Connection.Execute(sql, detail, DBWork.Transaction);
        }

        public int UpdateChkQtyMatchCE(string chk_nos, string wh_kind, string update_user, string update_ip)
        {
            // 盤點量 = 所有 能設/通信 庫的量的總和
            string sql = string.Format(@"update CHK_DETAIL a
                                            set chk_qty = (case
                                                              when store_qty_update_time is null
                                                                  then store_qtyc
                                                              else store_qty_n
                                                           end), 
                                                chk_time = sysdate, 
                                                update_time = sysdate, 
                                                update_user = :update_user,
                                                update_ip = :update_ip,
                                                store_qty_n = (select sum(inv_qty) from MI_WHINV
                                                                where wh_no  in (select wh_no from MI_WHMAST where wh_kind = :wh_kind)
                                                                  and mmcode = a.mmcode
                                                                group by mmcode),
                                                store_qty_update_time = sysdate
                                          where chk_no in ( {0} )", chk_nos);

            return DBWork.Connection.Execute(sql, new { wh_kind = wh_kind, update_user = update_user, update_ip = update_ip }, DBWork.Transaction);
        }
        #endregion

        #region 列印
        public IEnumerable<CHK_DETAIL> GetPrintData(string chk_nos, string userId)
        {
            var p = new DynamicParameters();

            var sql = @"select a.CHK_NO, a.mmcode,a.MMNAME_C,a.MMNAME_E,a.STORE_LOC,a.LOC_NAME,a.BASE_UNIT,a.STORE_QTYC, a.WH_NO,
                               (select chk_type from CHK_MAST where chk_no = a.chk_no) as chk_type,
                               (select chk_wh_kind from CHK_MAST where chk_no = a.chk_no) as chk_wh_kind_code,
                               NVL(TO_CHAR((select inv_qty from MI_WINVMON 
                                             where data_ym = TWN_PYM((select substr(chk_ym,1,5) from CHK_MAST where chk_no = a.chk_no)) 
                                               and wh_no = a.wh_no 
                                               and mmcode = a.mmcode)),' ') as pre_inv_qty,
                               NVL((case 
                                    when (TWN_YYYMM(sysdate) = (select SUBSTR(chk_ym, 1,5) from CHK_MAST where chk_no = a.chk_no))
                                        then (select TO_CHAR(apl_inqty) from MI_WHINV where wh_no = a.wh_no and mmcode = a.mmcode)
                                    else 
                                        (select TO_CHAR(apl_inqty) from MI_WINVMON 
                                          where data_ym = (select substr(chk_ym,1,5) from CHK_MAST where chk_no = a.chk_no) 
                                            and wh_no = a.wh_no and mmcode = a.mmcode)
                               end),' ') as apl_inqty, 
                               NVL((case 
                                    when (TWN_YYYMM(sysdate) = (select SUBSTR(chk_ym, 1,5) from CHK_MAST where chk_no = a.chk_no))
                                        then (select TO_CHAR(apl_outqty) from MI_WHINV where wh_no = a.wh_no and mmcode = a.mmcode)
                                    else 
                                        (select TO_CHAR(apl_outqty) from MI_WINVMON 
                                          where data_ym = (select substr(chk_ym,1,5) from CHK_MAST where chk_no = a.chk_no) 
                                            and wh_no = a.wh_no and mmcode = a.mmcode)
                               end),' ') as apl_outqty, 
                               NVL((case 
                                    when (TWN_YYYMM(sysdate) = (select SUBSTR(chk_ym, 1,5) from CHK_MAST where chk_no = a.chk_no))
                                        then (select TO_CHAR(use_qty) from MI_WHINV where wh_no = a.wh_no and mmcode = a.mmcode)
                                    else 
                                        (select TO_CHAR(use_qty) from MI_WINVMON 
                                          where data_ym = (select substr(chk_ym,1,5) from CHK_MAST where chk_no = a.chk_no) 
                                            and wh_no = a.wh_no and mmcode = a.mmcode)
                               end),' ') as use_qty, 
                              (case when (wh_no = 'PH1S' and chk_time is null)
                               then store_qtyc
                               else CHK_QTY end) as CHK_QTY, b.CHK_TYPE, b.CHK_WH_KIND,
                               a.CHK_REMARK, TWN_TIME_FORMAT(a.CHK_TIME) as CHK_TIME, a.STATUS_INI,b.CHK_WH_NO, b.chk_ym,
                              (select c.DATA_VALUE || ' ' || c.DATA_DESC 
                                     from PARAM_D c
                                    where c.GRP_CODE = 'CHK_MAST' 
                                      and c.DATA_NAME = 'CHK_CLASS'
                                      and c.DATA_VALUE = b.CHK_CLASS) as CHK_CLASS_NAME 
                                      FROM CHK_DETAIL a, CHK_MAST b 
                                      WHERE 1=1 AND b.chk_no = a.chk_no ";

            if (chk_nos != "" && chk_nos != null)
            {
                string[] tmp = chk_nos.Split(',');
                sql += "AND a.chk_no IN :CHK_NO ";
                p.Add(":CHK_NO", tmp);
            }

            sql += "Order by  a.MMCODE asc";

            return DBWork.Connection.Query<CHK_DETAIL>(sql, p, DBWork.Transaction);
        }
        #endregion

        #region call webapi
        public bool IsCurrentSetYm(string chk_no) {
            string sql = @"select 1 from CHK_MAST a
                            where chk_no = :chk_no
                              and exists (select 1 from MI_MNSET 
                                           where a.create_date between set_btime and set_ctime
                                             and set_status = 'N')";
            return !(DBWork.Connection.ExecuteScalar(sql,
                                                     new
                                                     {
                                                         chk_no = chk_no
                                                     },
                                                     DBWork.Transaction) == null);
        }

        public bool IsCurrentSetYmMulti(string chk_nos)
        {
            string sql = string.Format(@"select 1 from CHK_MAST a
                            where chk_no in ( {0} )
                              and exists (select 1 from MI_MNSET 
                                           where a.create_date between set_btime and set_ctime
                                             and set_status = 'N')", chk_nos);
            return !(DBWork.Connection.ExecuteScalar(sql,
                                                     DBWork.Transaction) == null);
        }

        public COMBO_MODEL GetMaxConsumeDate()
        {
            string sql = @"select max(data_date||data_etime)  as value,
                                  twn_systime as text
                             from MI_CONSUME_DATE
                            where proc_id = 'Y'";
            return DBWork.Connection.QueryFirstOrDefault<COMBO_MODEL>(sql, DBWork.Transaction);
        }

        public CHK_MAST GetChkmast(string chk_no) {
            string sql = @"select * from CHK_MAST where chk_no = :chk_no";
            return DBWork.Connection.QueryFirstOrDefault<CHK_MAST>(sql, new { chk_no = chk_no }, DBWork.Transaction);
        }
        public IEnumerable<CHK_MAST> GetChkmasts(string chk_nos) {
            string sql = string.Format(@"select * from CHK_MAST where chk_no in ( {0} )", chk_nos);
            return DBWork.Connection.Query<CHK_MAST>(sql, DBWork.Transaction);
        }

        public int UpdateHisConsumeQtyT(string chk_no, string mmcode, int order_count, string maxtime)
        {
            string sql = @"update CHK_DETAIL
                              set his_consume_qty_t = :order_count,
                                  his_consume_datatime = :maxtime,
                                  update_time = sysdate
                            where chk_no = :chk_no
                              and mmcode = :mmcode";
            return DBWork.Connection.Execute(sql, new { chk_no = chk_no, mmcode = mmcode, order_count = order_count, maxtime = maxtime }, DBWork.Transaction);
        }

        public IEnumerable<CHK_DETAIL> GetAllDetails(string chk_no) {
            string sql = string.Format(@"select * from CHK_DETAIL where chk_no  = :chk_no");
            return DBWork.Connection.Query<CHK_DETAIL>(sql, new { chk_no = chk_no}, DBWork.Transaction);
        }
        #endregion

        public string GetCreateUser(string chk_no) {
            string sql = @"
                select create_user from CHK_MAST
                 where chk_no = :chk_no
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { chk_no }, DBWork.Transaction);
        }
    }
}