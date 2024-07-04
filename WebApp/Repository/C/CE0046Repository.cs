using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using TSGH.Models;
using WebApp.Models.D;
using System.Linq;

namespace WebApp.Repository.C
{
    public class CE0046Repository : JCLib.Mvc.BaseRepository
    {
        public CE0046Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<CE0045M> GetAllM(string p0, string p1, string user, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            string strSql = @"with temp AS (
                    select set_ym, set_btime, nvl(set_etime,set_ctime) as set_etime
                    from mi_mnset
                    where set_ym =:data_ym
                ) 
                select a.mmcode as mmcode, b.mmname_c as mmname_c, b.mmname_e as mmname_e, b.base_unit as base_unit, a.inv_qty as inv_qty, a.chk_qty as chk_qty, a.memo as memo,
                    ( select inv_qty from dgmiss_inv where data_ym = twn_pym(a.data_ym) and   mmcode = a.mmcode and   inid = a.inid ) as old_inv_qty, 
                    ( select sum(ackqty) from dgmiss b, temp c where acktime between c.set_btime and c.set_etime and   b.app_inid = a.inid and   b.mmcode = a.mmcode and   is_del = 'n' ) as current_apply_amount, 
                    ( case when ( a.chk_uid is null ) then ' ' else ( select una from ur_id where tuser = a.chk_uid )end ) as chk_uid, 
                    twn_time(a.chk_time) as chk_time, a.chk_status as chk_status 
                from dgmiss_chk a, mi_mast b
                where 1 = 1 and   a.data_ym =:data_ym and   a.mmcode = b.mmcode
            ";

            p.Add(":USER_NAME", user);
            p.Add(":data_ym", p0); // 盤點年月

            if (!String.IsNullOrEmpty(p1))
            {
                strSql += " AND   a.inid =:inid";
                p.Add(":inid", p1); //責任中心
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CE0045M>(GetPagingStatement(strSql, sorters), p, DBWork.Transaction);
        }

        public DataTable GetExcelListSheet(string p0, string p1)
        {
            var p = new DynamicParameters();
            string strSql = @"with temp as (
                    select set_ym, set_btime, nvl(set_etime, set_ctime) as set_etime 
                    from MI_MNSET
                    where set_ym = :set_ym
                )
                select a.data_ym 資料年月, a.inid as 責任中心, a.mmcode 院內碼, b.mmname_c 中文品名, b.mmname_e 英文品名, b.BASE_UNIT 計量單位, a.inv_qty 電腦量, a.CHK_QTY 盤點量, a.memo 備註, (select inv_qty from DGMISS_INV where data_ym=twn_pym(a.data_ym) and mmcode=a.mmcode and inid=a.inid) as 上期結存, 
                    (select sum(ackqty) from DGMISS b, temp c where acktime between c.set_btime and c.set_etime and b.app_inid=a.inid and b.mmcode = a.mmcode and is_del='N') as 本月申請總量, (case when (a.chk_uid is null ) then ' ' else (select una from UR_ID where tuser = a.chk_uid) end) as 盤點人員, twn_time(a.chk_time) 盤點時間 
                from DGMISS_CHK a, MI_MAST b
                where 1=1 and a.data_ym = :set_ym  and a.mmcode = b.mmcode
";
            p.Add(":set_ym", p0);

            if (!String.IsNullOrEmpty(p1))
            {
                strSql += " and a.inid =:ur_inid ";
                p.Add(":ur_inid", p1); //責任中心
            }

            strSql += @" order by a.data_ym, a.inid, a.mmcode";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(strSql, p, DBWork.Transaction))
            {
                dt.Load(rdr);
            }

            return dt;
        }

        public DataTable GetExcelSummarySheet(string p0, string p1)
        {
            var p = new DynamicParameters();

            #region sql
            var sql = @"with temp as (
                    select set_ym, set_btime, nvl(set_etime, set_ctime) as set_etime 
                    from MI_MNSET
                    where set_ym = :set_ym
                )
                select a.data_ym 資料年月, a.mmcode 院內碼, b.mmname_c 中文品名, b.mmname_e 英文品名, b.BASE_UNIT 計量單位, sum(a.inv_qty) 總電腦量, sum(a.CHK_QTY) 總盤點量, sum((select inv_qty from DGMISS_INV where data_ym=twn_pym(a.data_ym) and mmcode=a.mmcode and inid=a.inid)) as 上期總結存, 
                (select sum(ackqty) from DGMISS b, temp c where acktime between c.set_btime and c.set_etime and b.mmcode = a.mmcode and is_del='N') as 本月申請總量 
                from DGMISS_CHK a, MI_MAST b
                where 1=1 and a.data_ym = :set_ym and a.mmcode = b.mmcode and a.chk_time is not null
";
            #endregion

            p.Add(":set_ym", p0); //月結年月

            if (!String.IsNullOrEmpty(p1))
            {
                sql += " AND   a.inid =:ur_inid ";
                p.Add(":ur_inid", p1); //責任中心
            }

            sql += @"  group by a.data_ym, a.mmcode, b.mmname_c, b.mmname_e, b.base_unit
                order  by a.mmcode";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
            {
                dt.Load(rdr);
            }

            return dt;
        }

        /// <summary>
        /// 盤點年月(combo)
        /// </summary>
        /// <returns></returns>
        public IEnumerable<COMBO_MODEL> GetYmCombo()
        {
            string strSql = @"
SELECT
    set_ym AS VALUE,
    set_ym AS TEXT
FROM
    mi_mnset
WHERE
    set_status = 'C'
ORDER BY
    set_ym DESC";
            return DBWork.Connection.Query<COMBO_MODEL>(strSql);
        }

        /// <summary>
        /// 責任中心代碼(combo)
        /// </summary>
        /// <returns></returns>
        public IEnumerable<COMBO_MODEL> GetUrInidCombo(string strUserName)
        {
            string strSql = @"
WITH inids AS (
    SELECT
        c.inid AS value,
        c.inid_name AS text
    FROM
        mi_whid a,
        mi_whmast b,
        ur_inid c
    WHERE
        (
            a.wh_userid = :user_name
            AND   a.wh_no = b.wh_no
        )
        AND   b.inid = c.inid
),inid_all AS (
    SELECT
        b.inid AS value,
        b.inid_name AS text
    FROM
        mi_whmast a,
        ur_inid b
    WHERE
        EXISTS (
            SELECT
                1
            FROM
                ur_uir
            WHERE
                tuser = :user_name
                AND   rlno IN (
                    'ADMG',
                    'ADMG_14',
                    'MAT_14',
                    'MED_14',
                    'MMSpl_14'
                )
        )
) SELECT
    *
  FROM
    (
        SELECT
            *
        FROM
            inids
        UNION
        SELECT
            *
        FROM
            inid_all
    ) datas
ORDER BY
    value
";
            return DBWork.Connection.Query<COMBO_MODEL>(strSql, new { user_name = strUserName });
        }

        public IEnumerable<CHK_ST> GetChkStatus(string strYm)
        {
            string sql = @"
SELECT
    twn_date(set_ctime) as CHK_CLOSE_DATE
FROM
    mi_mnset
WHERE
    set_ym = :set_ym
    AND   ROWNUM = 1
";
            return DBWork.Connection.Query<CHK_ST>(sql, new { set_ym = strYm });
        }

        public string GetUserInid(string strUserId)
        {
            var sql = @"
SELECT
    user_inid(:user_id)
FROM
    dual
";

            return DBWork.Connection.QuerySingle<string>(sql, new { user_id = strUserId });
        }

        /**
         * Enums list start from here
         */
        public class CHK_ST
        {
            public string CHK_CLOSE_DATE { get; set; }
        }

    }
}