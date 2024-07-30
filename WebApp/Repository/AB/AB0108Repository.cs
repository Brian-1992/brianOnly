using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Repository.AB
{
    public class AB0108 : JCLib.Mvc.BaseModel
    {
        public string WH_NO { get; set; }
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string USE_TYPE { get; set; }
        public string USE_QTY { get; set; }
        public string BASE_UNIT { get; set; }
        public string WEXP_ID { get; set; }
        public string LOT_NO { get; set; }
        public string EXP_DATE { get; set; }
        public string M_TRNID { get; set; }
        public string E_SOURCECODE { get; set; }
        public string ISUSE { get; set; }
        public string USE_DATE { get; set; }
        public string SUSE_NOTE { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string SUSE_SEQ { get; set; }
        public string BF_INVQTY { get; set; }  //扣庫前庫存量   
        public string TRATIO { get; set; }     //轉換後數量   
        public string ACKTIMES { get; set; }   //扣庫倍率   
        public string ADJQTY { get; set; }     //散裝量   
        public string SCAN_BARCODE { get; set; }  //前端掃入的條碼           
        
    }
    public class AB0108Repository : JCLib.Mvc.BaseRepository
    {
        public AB0108Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        private string GetSql(string status, string wh_no, string mmcode, bool view_all, string userId) {
            string sql = @"
                select twn_time(CREATE_TIME) as create_time,a.wh_no, a.mmcode,
                       (select mmname_c from mi_mast where mmcode=a.mmcode) as mmname_c,
                       (select mmname_e from mi_mast where mmcode=a.mmcode) as mmname_e,
                       BF_INVQTY,  --扣庫前庫存量
                       (case when a.use_type='A' then 'A扣庫'
                             when a.use_type='B' then 'B繳回'
                        end) as use_type,
                       a.use_qty, a.base_unit,
                       (case when a.wexp_id='Y' then 'Y批號效期管理'
                             when a.wexp_id='N' then 'N不需批號效期管理'
                        end) as wexp_id,
                       a.lot_no, twn_date(a.exp_date) as exp_date,
                       (case when a.m_trnid='1' then '1盤盈虧(正推)'
                             when a.m_trnid='2' then '2消耗(逆推)'
                        end) as m_trnid,
                       (case when a.e_sourcecode='P' then 'P買斷'
                             when a.e_sourcecode='C' then 'C寄售'
                             when a.e_sourcecode='R' then 'R核醫'
                             when a.e_sourcecode='N' then  'N其它'
                             else a.e_sourcecode
                        end) as e_sourcecode,
                       (case when a.m_trnid='1' then 'Z其它系統扣庫'
                             when a.m_trnid='2' then
                                  (case when a.isuse='Y' then 'Y已扣庫'
                                        when a.isuse='N' then 'N不扣庫(逆推非買斷)'
                                        else 'Z其它系統扣庫'
                                   end)
                        end) as isuse,
                       a.suse_note,                       
                       user_name(a.create_user) as create_user,  --刷條碼人員
                       TRATIO,  --轉換後數量
                       ACKTIMES,  --扣庫倍率
                       ADJQTY,  --散裝量
                       SCAN_BARCODE,  --前端掃入的條碼
                       a.suse_seq
                  from SCAN_USE_LOG a
                 where 1=1
                   and twn_date(a.create_time)>= :start_date
                   and twn_date(a.create_time)<= :end_date
            ";

            if (string.IsNullOrEmpty(status) == false) {
                sql += "  and a.isuse = :status";
            }

            if (view_all == false) {
                sql += @"
                          and a.wh_no in (
                                            select A.WH_NO 
                                              from MI_WHMAST A
                                             WHERE WH_KIND = '1' and wh_grade > '1'
                                               AND EXISTS
                                                     (SELECT 'X' FROM UR_ID B
                                                       WHERE (A.SUPPLY_INID=B.INID OR A.INID=B.INID) AND TUSER=:userId)
                                               AND NOT EXISTS
                                                     (SELECT 'X' FROM MI_WHID B
                                                       WHERE TASK_ID IN ('2','3') AND WH_USERID=:userId)
                                               and a.cancel_id = 'N'
                                            UNION ALL 
                                            SELECT A.WH_NO
                                              FROM MI_WHMAST A, MI_WHID B
                                             WHERE A.WH_NO=B.WH_NO AND TASK_ID IN ('2','3') AND WH_USERID=:userId
                                               and a.cancel_id = 'N'
                                               and a.wh_grade > '1'
                                       )
                ";
            }

            if (string.IsNullOrEmpty(wh_no) == false) {
                sql += "  and a.wh_no = :wh_no";
            }

            if (string.IsNullOrEmpty(mmcode) == false) {
                sql += string.Format("  and a.mmcode like '%{0}%'", mmcode);
            }

            return sql;
        }

        public IEnumerable<AB0108> GetData(string start_date, string end_date, string status, string wh_no, string mmcode, bool view_all, string userId) {
            string sql = GetSql(status, wh_no, mmcode, view_all, userId);
            return DBWork.PagingQuery<AB0108>(sql, new { start_date, end_date, status, wh_no, mmcode, userId });
        }

        public DataTable GetExcel(string start_date, string end_date, string status, string wh_no, string mmcode, bool view_all, string userId) {
            var p = new DynamicParameters();

            p.Add("start_date", start_date);
            p.Add("end_date", end_date);
            p.Add("status", status);
            p.Add("wh_no", wh_no);
            p.Add("mmcode", mmcode);
            p.Add("userId", userId);

            string sql = string.Format(@"
                select create_time as 刷條碼時間,wh_no as 庫房代碼,  mmcode as 院內碼, mmname_c as 中文品名, mmname_e as 英文品名,
                       BF_INVQTY as 扣庫前庫存量,use_type as 異動類別, use_qty as 扣庫量_繳回量, base_unit as 計量單位, wexp_id as 批號效期註記,
                       lot_no as 批號, exp_date as 效期, m_trnid as 盤差種類, e_sourcecode as 來源代碼,
                       isuse as 是否已扣庫, suse_note as 備註, create_user as 刷條碼人員,
                       TRATIO as 轉換後數量,ACKTIMES as 扣庫倍率,ADJQTY as 散裝量,SCAN_BARCODE as 前端掃入的條碼,suse_seq as 序號
                  from (
                        {0}
                       )
                order by 1
            ", GetSql(status, wh_no, mmcode, view_all, userId));

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public bool CheckWhnoValid(string wh_no, string userId) {
            string sql = @"
                select A.WH_NO VALUE, WH_NO||' '||WH_NAME TEXT, A.WH_NO || ' ' || A.WH_NAME COMBITEM
                  from MI_WHMAST A
                 WHERE WH_KIND = '1' and wh_grade > '1'
                   AND EXISTS
                         (SELECT 'X' FROM UR_ID B
                           WHERE (A.SUPPLY_INID=B.INID OR A.INID=B.INID) AND TUSER=:userId)
                   AND NOT EXISTS
                         (SELECT 'X' FROM MI_WHID B
                           WHERE TASK_ID IN ('2','3') AND WH_USERID=:userId)
                   and a.cancel_id = 'N'
                   and a.wh_no = :wh_no
                UNION ALL 
                SELECT A.WH_NO, A.WH_NO||' '||WH_NAME, A.WH_NO || ' ' || A.WH_NAME COMBITEM
                  FROM MI_WHMAST A, MI_WHID B
                 WHERE A.WH_NO=B.WH_NO AND TASK_ID IN ('2','3') AND WH_USERID=:userId
                   and a.cancel_id = 'N'
                   and a.wh_grade > '1'
                   and a.wh_no = :wh_no
            ";
            return (DBWork.Connection.ExecuteScalar(sql, new { wh_no, userId }, DBWork.Transaction)) != null;
        }

        #region combo

        public IEnumerable<COMBO_MODEL> GetViewAllWhnos() {
            string sql = @"
                select wh_no as value, wh_no||' '||wh_name as text
                  from MI_WHMAST
                 where wh_kind = '1' 
                   and wh_grade > '1'
                   and cancel_id = 'N'
                 order by wh_no
            ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetWhnoCombo(string userId)
        {
            string sql = @"
                select A.WH_NO VALUE, WH_NO||' '||WH_NAME TEXT, A.WH_NO || ' ' || A.WH_NAME COMBITEM
                  from MI_WHMAST A
                 WHERE WH_KIND = '1' and wh_grade > '1'
                   AND EXISTS
                         (SELECT 'X' FROM UR_ID B
                           WHERE (A.SUPPLY_INID=B.INID OR A.INID=B.INID) AND TUSER=:userId)
                   AND NOT EXISTS
                         (SELECT 'X' FROM MI_WHID B
                           WHERE TASK_ID IN ('2','3') AND WH_USERID=:userId)
                   and a.cancel_id = 'N'
                UNION ALL 
                SELECT A.WH_NO, A.WH_NO||' '||WH_NAME, A.WH_NO || ' ' || A.WH_NAME COMBITEM
                  FROM MI_WHMAST A, MI_WHID B
                 WHERE A.WH_NO=B.WH_NO AND TASK_ID IN ('2','3') AND WH_USERID=:userId
                   and a.cancel_id = 'N'
                   and a.wh_grade > '1'
                 ORDER BY 1
            ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { userId }, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetStatusCombo() {
            string sql = @"
                select data_value as value, data_value||' '||data_desc as text
                  from PARAM_D
                 where grp_code = 'SCAN_USE_LOG'
                   and data_name = 'ISUSE'
                 order by data_seq
            ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, DBWork.Transaction);
        }
        #endregion
    }
}