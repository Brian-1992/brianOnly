using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Models;
using WebApp.Models.MI;

namespace AA0112
{
    public class DailyControllMedRepository : JCLib.Mvc.BaseRepository
    {
        public DailyControllMedRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ChkWh> GetWhnos(string code_start, string code_end)
        {
            string sql = @"select a.wh_no as wh_no,
                                  b.wh_grade as wh_grade,
                                  count(*) as chk_total,
                                  b.inid as inid
                             from MI_WHINV a, MI_WHMAST b, MI_MAST c, MI_WINVCTL d
                            where b.wh_no = a.wh_no
                              and c.mmcode = a.mmcode
                              and b.wh_kind = '0'
                              and c.mat_class = '01'
                              and b.wh_grade in ('3', '4')
                              and c.e_restrictcode between :code_start and :code_end
                              and d.mmcode = c.mmcode
                              and d.wh_no = b.wh_no
                              and d.ctdmdccode = '0'
                           having count(*) > 0
                            group by a.wh_no, b.wh_grade, b.inid
                            order by a.wh_no, b.wh_grade, b.inid";

            return DBWork.Connection.Query<ChkWh>(sql, new { code_start = code_start, code_end = code_end }, DBWork.Transaction);
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

        public int InsertDetail(CHK_MAST mast)
        {
            string sql = string.Format(@"insert into CHK_DETAIL(chk_no, mmcode, mmname_c, mmname_e, base_unit,
                                                                m_contprice, wh_no, store_loc, loc_name, mat_class,
                                                                m_storeid, store_qtyc, store_qtym, store_qtys,
                                                                chk_qty, chk_remark, chk_uid, chk_time, status_ini, 
                                                                create_date, create_user, update_time, update_user, update_ip)
                            (
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
                              from MI_WHINV a, MI_MAST b, MI_WINVCTL c
                             where a.wh_no = :wh_no
                               and a.mmcode = b.mmcode
                               and b.mat_class = '01'
                               and c.wh_no = a.wh_no
                               and c.mmcode = a.mmcode
                               and c.ctdmdccode = '0'");
            if (mast.CHK_TYPE == "4")
            {
                sql += "       and b.E_RESTRICTCODE IN ('4')";
            }
            else
            {
                sql += "       and b.E_RESTRICTCODE IN ('1','2','3')";
            }

            sql += ")";

            return DBWork.Connection.Execute(sql,
                new
                {
                    wh_no = mast.CHK_WH_NO,
                    chk_no = mast.CHK_NO,
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

        public MI_MNSET GetCurrentSetym() {
            string sql = @"select set_ym,
                                  to_char(set_btime, 'YYYY-MM-DD') as set_btime,
                                  to_char(set_etime, 'YYYY-MM-DD') as set_etime,
                                  set_status,
                                  set_msg,
                                  to_char(set_ctime, 'YYYY-MM-DD') as set_ctime
                             from MI_MNSET
                            where set_status = 'N'";
            return DBWork.Connection.QueryFirstOrDefault<MI_MNSET>(sql, DBWork.Transaction);
        }

        public bool IsOpened(string set_ym) {
            string sql = @"select 1 
                             from CHK_MAST a
                            where 1=1
                              and substr(a.chk_ym, 1,5) = :set_ym
                              and create_user = 'BATCH'
                              and chk_wh_kind = '0'
                              and chk_period = 'D'
                              and chk_type in ('3', '4')
                              and chk_wh_grade in ('3', '4')";
            return (DBWork.Connection.QueryFirstOrDefault<int>(sql, new { set_ym = set_ym }, DBWork.Transaction)) > 0;
        }
    }
}
