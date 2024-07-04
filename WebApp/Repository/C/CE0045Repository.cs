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
    public class CE0045Repository : JCLib.Mvc.BaseRepository
    {
        public CE0045Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<CE0045M> GetAllM(string p0, string p1, string user, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"
WITH temp AS (
    SELECT
        set_ym,
        set_btime,
        nvl(set_etime,set_ctime) AS set_etime
    FROM
        mi_mnset
    WHERE
        set_ym =:data_ym
) SELECT
    a.mmcode as mmcode, -- 院內碼
    b.mmname_c as mmname_c, -- 中文品名
    b.mmname_e as mmname_e,-- 英文品名
    b.base_unit as base_unit, -- 計量單位
    a.inv_qty as inv_qty, -- 電腦量(庫存量)
    a.chk_qty as chk_qty, -- 盤點量
    a.memo as memo, -- 備註
    (
        SELECT
            inv_qty
        FROM
            dgmiss_inv
        WHERE
            data_ym = twn_pym(a.data_ym)
            AND   mmcode = a.mmcode
            AND   inid = a.inid
    ) AS OLD_INV_QTY, --  上期結存
    (
        SELECT
            SUM(ackqty)
        FROM
            dgmiss b,
            temp c
        WHERE
            acktime BETWEEN c.set_btime AND c.set_etime
            AND   b.app_inid = a.inid
            AND   b.mmcode = a.mmcode
            AND   is_del = 'N'
    ) AS CURRENT_APPLY_AMOUNT, -- 本月申請總量
    (
        CASE
            WHEN ( a.chk_uid IS NULL ) THEN ' '
            ELSE (
                SELECT
                    una
                FROM
                    ur_id
                WHERE
                    tuser = a.chk_uid
            )
        END
    ) AS CHK_UID, -- 盤點人員
    twn_time(a.chk_time) AS CHK_TIME, -- 盤點時間
    a.chk_status AS CHK_STATUS -- 狀態_畫面不顯示
  FROM
    dgmiss_chk a,
    mi_mast b
  WHERE
    1 = 1
    AND   a.data_ym =:data_ym
    AND   a.inid =:inid
    AND   a.mmcode = b.mmcode
";
            p.Add(":USER_NAME", user);
            p.Add(":data_ym", p0); // 盤點年月
            p.Add(":inid", p1); //責任中心

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CE0045M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public DGMISS_CHK GetDGMISS_CHK(DGMISS_CHK obj)
        {
            string strSql = @"
SELECT
    *
FROM
    dgmiss_chk
WHERE
    data_ym =:data_ym
    AND   inid =:inid
    AND   mmcode =:mmcode
";
            return DBWork.Connection.QuerySingle<DGMISS_CHK>(strSql, obj);
        }
        //匯出
        public DataTable GetExcel(string p0, string p1)
        {
            var p = new DynamicParameters();
            #region
            var sql = @"
WITH temp AS (
    SELECT
        set_ym,
        set_btime,
        nvl(set_etime,set_ctime) AS set_etime
    FROM
        mi_mnset
    WHERE
        set_ym =:set_ym
) SELECT
    a.data_ym 資料年月,
    a.inid AS 責任中心,
    a.mmcode 院內碼,
    b.mmname_c 中文品名,
    b.mmname_e 英文品名,
    b.base_unit 計量單位,
    a.inv_qty 電腦量,
    a.chk_qty 盤點量,
    a.memo 備註,
    (
        SELECT
            inv_qty
        FROM
            dgmiss_inv
        WHERE
            data_ym = twn_pym(a.data_ym)
            AND   mmcode = a.mmcode
            AND   inid = a.inid
    ) AS 上期結存,
    (
        SELECT
            SUM(ackqty)
        FROM
            dgmiss b,
            temp c
        WHERE
            acktime BETWEEN c.set_btime AND c.set_etime
            AND   b.app_inid = a.inid
            AND   b.mmcode = a.mmcode
            AND   is_del = 'N'
    ) AS 本月申請總量,
    (
        CASE
            WHEN ( a.chk_uid IS NULL ) THEN ' '
            ELSE (
                SELECT
                    una
                FROM
                    ur_id
                WHERE
                    tuser = a.chk_uid
            )
        END
    ) AS 盤點人員,
    twn_time(a.chk_time) 盤點時間
  FROM
    dgmiss_chk a,
    mi_mast b
  WHERE
    1 = 1
    AND   a.data_ym =:set_ym
    AND   a.inid =:ur_inid
    AND   a.mmcode = b.mmcode
";
            #endregion

            p.Add(":set_ym", p0); //月結年月
            p.Add(":ur_inid", p1); //責任中心

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

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

        //盤存單位
        public IEnumerable<COMBO_MODEL> GetRlnoCombo(string user_name)
        {
            var p = new DynamicParameters();
            string sql = @"WITH TEMP AS ( SELECT RLNO FROM UR_UIR WHERE TUSER = :USER_NAME)
                           SELECT '1' AS VALUE, '各請領單位' AS TEXT FROM TEMP WHERE RLNO IN ('MAT_14','MED_14', 'NRS_14', 'ADMG', 'ADMG_14', 'PHR_14','MMSpl_14' ) 
                           UNION 
                           SELECT '2' AS VALUE, '藥材庫' AS TEXT FROM TEMP WHERE RLNO IN ('ADMG', 'ADMG_14', 'MAT_14','MED_14','MMSpl_14' ) 
                           UNION 
                           SELECT '3' AS VALUE, '全部藥局' AS TEXT FROM TEMP WHERE RLNO IN ('ADMG', 'ADMG_14', 'MAT_14','MED_14', 'PHR_14','MMSpl_14' ) 
                           UNION 
                           SELECT '4' AS VALUE, '全院合計' AS TEXT FROM TEMP WHERE RLNO IN ('ADMG', 'ADMG_14', 'MAT_14','MED_14','MMSpl_14' ) 
                           UNION 
                           SELECT '5' AS VALUE, '各單位消耗結存明細' AS TEXT FROM TEMP WHERE RLNO IN ('ADMG', 'ADMG_14', 'MAT_14','MED_14','MMSpl_14' ) 
                           ";
            p.Add(":USER_NAME", user_name);

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }
        //庫房類別
        public IEnumerable<COMBO_MODEL> GetWhKindCombo(string user_name)
        {
            var p = new DynamicParameters();
            string sql = @"WITH MAT AS ( SELECT DISTINCT '1' AS VALUE, '衛材' AS TEXT 
                                         FROM MI_WHID A, MI_WHMAST B
                                         WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO AND B.WH_KIND = '1')
                                         OR EXISTS (SELECT 1 FROM UR_UIR WHERE TUSER = :USER_NAME AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14','MMSpl_14'))
                                        ),
                                MED AS ( SELECT DISTINCT '0' AS VALUE, '藥品' AS TEXT 
                                         FROM MI_WHID A, MI_WHMAST B
                                         WHERE (A.WH_USERID= :USER_NAME AND A.WH_NO = B.WH_NO AND B.WH_KIND = '1')
                                         OR EXISTS (SELECT 1 FROM UR_UIR WHERE TUSER = :USER_NAME AND RLNO IN ('ADMG', 'ADMG_14', 'MED_14','MMSpl_14'))
                                        ),
                                TEMP AS ( SELECT * FROM MAT UNION SELECT * FROM MED )
                           SELECT * FROM TEMP
                           UNION
                           SELECT ' ' AS VALUE, '全部' AS TEXT FROM TEMP WHERE (SELECT COUNT(*) FROM TEMP) = 2
                          ";
            p.Add(":USER_NAME", user_name);

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }

        public IEnumerable<CHK_ST> GetChkStatus(string strYm, string strUrInid)
        {
            string sql = @"
SELECT
    (
        SELECT DISTINCT
            data_desc
        FROM
            param_d
        WHERE
            grp_code = 'CHK_MAST'
            AND   data_name = 'CHK_STATUS'
            AND   data_value = a.chk_status
    ) AS CHK_STATUS, -- 狀態
    (
        SELECT
            twn_date(set_ctime)
        FROM
            mi_mnset
        WHERE
            set_ym =:set_ym
    ) AS CHK_CLOSE_DATE -- 月結日期
FROM
    dgmiss_chk a
WHERE
    a.data_ym =:set_ym
    AND   a.inid =:ur_inid
    AND   ROWNUM = 1
";
            return DBWork.Connection.Query<CHK_ST>(sql, new { set_ym = strYm, ur_inid = strUrInid });
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

        public int UpdateDgmissChkDetail(DGMISS_CHK rec)
        {
            string sql = @"
UPDATE dgmiss_chk a
    SET
        a.chk_qty = :chk_qty,
        a.memo = :memo,
        a.chk_uid = :chk_uid,
        a.chk_time = sysdate,
        a.chk_status = :chk_status,
        a.update_time = sysdate,
        a.update_user = :update_user,
        a.update_ip = :update_ip
WHERE
    a.data_ym = :data_ym
    AND   a.inid = :inid
    AND   a.mmcode = :mmcode
";
            return DBWork.Connection.Execute(sql, rec, DBWork.Transaction);
        }
        
        /**
         * Enums list start from here
         */
        public class CHK_ST
        {
            public string CHK_STATUS { get; set; }
            public string CHK_CLOSE_DATE { get; set; }
        }

    }
}