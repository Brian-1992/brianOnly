using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace ROReset
{
    public class ROResetRepository : JCLib.Mvc.BaseRepository
    {
        public ROResetRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        /// <summary>
        /// 取得所有 mmcode 院內碼, ro_whtype 庫房級別 的欄位資料
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SelectType> GetSelectTypeResult()
        {
            string strSql = @"
SELECT
    mmcode,
    ro_whtype
FROM
    mi_basero_14";
            return DBWork.Connection.Query<SelectType>(strSql, DBWork.Transaction);
        }

        /// <summary>
        /// 使用條件 ro_type 基準日類型，取得所有 mmcode 院內碼, ro_whtype 庫房級別 的欄位資料
        /// 
        /// 基準量模式(1:日基準 2:月基準 3:自訂基準量)
        /// </summary>
        /// <param name="strRoType"></param>
        /// <returns></returns>
        public IEnumerable<SelectType> GetSelectTypeResultByRoType(string strRoType)
        {
            string strSql = @"
SELECT
    mmcode,
    ro_whtype
FROM
    mi_basero_14
WHERE
    ro_type = :ro_type";
            return DBWork.Connection.Query<SelectType>(strSql, new { ro_type = strRoType }, DBWork.Transaction);
        }

        /// <summary>
        /// 更新 MI_CONSUME_DATE x日平均消耗
        /// ex: 更新 10日平均消耗, 更新 14 日平均消耗...
        /// </summary>
        /// <param name="iConsumption"></param>
        /// <param name="strMmcode"></param>
        /// <param name="strDays"></param>
        /// <returns></returns>
        public int UpdateDayUse(int iConsumption, string strMmcode, string strDayUse)
        {
            string strSql = @"
UPDATE mi_basero_14
    SET
        "+ strDayUse + @" = :consumption / 10
WHERE
    mmcode = :mmcode";

            return DBWork.Connection.Execute(
                strSql,
                new
                {
                    consumption = iConsumption,
                    mmcode = strMmcode
                }
            );
        }

        /// <summary>
        /// 根據 院內碼 取得 x 天前 至 今天前 總消耗量
        /// </summary>
        public int GetConsumptionOfLastDays(string strMmcode, int iDays)
        {
            string strSql = @"
SELECT
    ceil(nvl(SUM(consume_qty),0) )
FROM
    mi_consume_date
WHERE
    data_date BETWEEN twn_date(SYSDATE - :days) AND twn_date(SYSDATE - 1)
    AND   mmcode = :mmcode
";
            return DBWork.Connection.Execute(
                strSql,
                new { mmcode = strMmcode, days = iDays + 1 }
            );
        }

        public int UpdateNowRo(string strMmcode, string strRoWhtype)
        {
            string strSql = @"
UPDATE mi_basero_14
    SET
        now_ro = (
            CASE
                WHEN abs(day_use_10 - mon_avg_use_3) > mon_avg_use_3 * diff_perc / 100 THEN day_use_10
                ELSE mon_avg_use_3
            END
        )
WHERE
    mmcode = :mmcode
    AND   ro_whtype = :ro_whtype
";
            return DBWork.Connection.Execute(strSql, new { mmcode = strMmcode, ro_whtype = strRoWhtype});
        }

        /// <summary>
        /// 取得是否為
        /// </summary>
        /// <returns></returns>
        public string GetEndOfMonth()
        {
            string strSql = @"
SELECT
    nvl( (
        SELECT
            1
        FROM
            (
                SELECT
                    set_ctime
                FROM
                    (
                        SELECT
                            set_ctime
                        FROM
                            (
                                SELECT
                                    set_ym,
                                    twn_date(set_ctime + 1) AS set_ctime
                                FROM
                                    mi_mnset
                                WHERE
                                    set_status = 'C'
                                ORDER BY
                                    set_ym DESC
                            )
                        WHERE
                            ROWNUM = 1
                    )
            )
        WHERE
            set_ctime = twn_date(SYSDATE)
    ),0)
FROM
    dual";
            return DBWork.Connection.QueryFirst<string>(strSql);
        }


        public int UpdateMonthlyConsumption(string strMmcode, string strRoWhtype)
        {
            string strSql = @"
UPDATE mi_basero_14 a
    SET
        mon_use_1 = (
            SELECT
                ceil(nvl(SUM(use_qty),0) )
            FROM
                mi_winvmon
            WHERE
                data_ym = twn_pym(cur_setym)
                AND   mmcode = a.mmcode
        ),
        mon_use_2 = (
            SELECT
                ceil(nvl(SUM(use_qty),0) )
            FROM
                mi_winvmon
            WHERE
                data_ym = twn_pym(twn_pym(cur_setym) )
                AND   mmcode = a.mmcode
        ),
        mon_use_3 = (
            SELECT
                ceil(nvl(SUM(use_qty),0) )
            FROM
                mi_winvmon
            WHERE
                data_ym = twn_pym(twn_pym(twn_pym(cur_setym) ) )
                AND   mmcode = a.mmcode
        ),
        mon_use_4 = (
            SELECT
                ceil(nvl(SUM(use_qty),0) )
            FROM
                mi_winvmon
            WHERE
                data_ym = twn_pym(twn_pym(twn_pym(twn_pym(cur_setym) ) ) )
                AND   mmcode = a.mmcode
        ),
        mon_use_5 = (
            SELECT
                ceil(nvl(SUM(use_qty),0) )
            FROM
                mi_winvmon
            WHERE
                data_ym = twn_pym(twn_pym(twn_pym(twn_pym(twn_pym(cur_setym) ) ) ) )
                AND   mmcode = a.mmcode
        ),
        mon_use_6 = (
            SELECT
                ceil(nvl(SUM(use_qty),0) )
            FROM
                mi_winvmon
            WHERE
                data_ym = twn_pym(twn_pym(twn_pym(twn_pym(twn_pym(twn_pym(cur_setym) ) ) ) ) )
                AND   mmcode = a.mmcode
        ),
        mon_avg_use_3 = (
            CASE
                WHEN (
                    SELECT
                        mat_class
                    FROM
                        mi_mast
                    WHERE
                        mmcode = a.mmcode
                ) = '02' THEN (
                    SELECT
                        ceil(nvl(SUM(use_qty),0) / 3)
                    FROM
                        mi_winvmon
                    WHERE
                        data_ym <= twn_pym(cur_setym)
                        AND   data_ym >= twn_pym(twn_pym(twn_pym(cur_setym) ) )
                        AND   mmcode = a.mmcode
                )
                ELSE 0
            END
        ),
        mon_avg_use_6 = (
            SELECT
                ceil(nvl(SUM(use_qty),0) / 6)
            FROM
                mi_winvmon
            WHERE
                data_ym <= twn_pym(cur_setym)
                AND   data_ym >= twn_pym(twn_pym(twn_pym(twn_pym(twn_pym(twn_pym(cur_setym) ) ) ) ) )
                AND   mmcode = a.mmcode
        )
WHERE
    a.mmcode =:mmcode
    AND   ro_whtype =:ro_whtype
";
            return DBWork.Connection.Execute(strSql, new { mmcode = strMmcode, ro_whtype = strRoWhtype });
        }

        /// <summary>
        /// 處理藥品資料(藥品未計算本次盤點資料，以醫令扣庫為主)
        /// </summary>
        /// <param name="strMmcode"></param>
        /// <param name="strRowWhType"></param>
        /// <returns></returns>
        public int UpdateBaseRoAvgUse3(string strMmcode, string strRowWhType)
        {
            string strSql = @"
UPDATE mi_basero_14 a
    SET
        now_ro = mon_avg_use_3
WHERE
    mmcode = 院內碼
    AND   ro_whtype = 庫房類別
    AND   EXISTS (
        SELECT
            1
        FROM
            mi_mast
        WHERE
            mmcode = a.mmcode
            AND   mat_class = '01'
    )";
            return DBWork.Connection.Execute(strSql, new { mmcode = strMmcode, ro_whtype = strRowWhType});
        }

        /// <summary>
        /// 處理衛材資料(衛材未計算本次月結資料，以前2~4月為主)
        /// </summary>
        /// <param name="strMmcode"></param>
        /// <param name="strRowWhType"></param>
        /// <returns></returns>
        public int UpdateBaseRo2To4Months(string strMmcode, string strRoWhType)
        {
            string strSql = @"
UPDATE mi_basero_14 a
    SET
        now_ro = (
            SELECT
                ceil(nvl(SUM(use_qty),0) / 3)
            FROM
                mi_winvmon
            WHERE
                data_ym <= twn_pym(twn_pym(cur_setym) )
                AND   data_ym >= twn_pym(twn_pym(twn_pym(twn_pym(cur_setym) ) ) )
                AND   mmcode = a.mmcode
        )
WHERE
    mmcode = :mmcode
    AND   ro_whtype = :ro_whtype
    AND   EXISTS (
        SELECT
            1
        FROM
            mi_mast
        WHERE
            mmcode = a.mmcode
            AND   mat_class = '02'
    )
";
            return DBWork.Connection.Execute(strSql, new { mmcode = strMmcode, ro_whtype = strRoWhType });
        }

        /// <summary>
        /// 更新一級庫藥品     
        /// 
        /// 護理病房最大請領量, 供應中心最大請領量, 藥局請領最大量, 安全庫存量, 正常庫存量
        /// </summary>
        public int UpdateMedicineQtys()
        {
            string strSql = @"
UPDATE mi_basero_14 a
    SET
        g34_max_appqty = now_ro * g34_perc,
        supply_max_appqty = 0,
        phr_max_appqty = now_ro * phr_perc,
        safe_qty = now_ro * safe_perc,
        normal_qty = now_ro * normal_perc
WHERE
    EXISTS (
        SELECT
            1
        FROM
            mi_mast
        WHERE
            mmcode = a.mmcode
            AND   mat_class = '01'
    )
    AND   a.ro_whtype = '1'
    AND   a.ro_type IN (
        '1',
        '2'
    )
";
            return DBWork.Connection.Execute(strSql);
        }

        /// <summary>
        /// 更新一級庫衛材     
        /// 
        /// 護理病房最大請領量, 供應中心最大請領量, 藥局請領最大量, 安全庫存量, 正常庫存量
        /// </summary>
        /// <returns></returns>
        public int UpdateHealthcareMaterialQtys()
        {
            string strSql = @"
UPDATE mi_basero_14 a
    SET
        g34_max_appqty = now_ro * g34_perc,
        supply_max_appqty = now_ro * supply_perc,
        phr_max_appqty = 0,
        safe_qty = now_ro * safe_perc,
        normal_qty = now_ro * normal_perc
WHERE
    EXISTS (
        SELECT
            1
        FROM
            mi_mast
        WHERE
            mmcode = a.mmcode
            AND   mat_class = '02'
    )
    AND   ro_whtype = '1'
    AND   a.ro_type IN (
        '1',
        '2'
    )
";
            return DBWork.Connection.Execute(strSql);
        }

        /// <summary>
        /// 更新藥局藥品
        /// 
        /// 護理病房最大請領量, 供應中心最大請領量, 藥局請領最大量, 安全庫存量, 正常庫存量
        /// </summary>
        public int UpdatePharmacyQtys()
        {
            string strSql = @"
UPDATE mi_basero_14 a
    SET
        g34_max_appqty = now_ro * g34_perc,
        supply_max_appqty = 0,
        phr_max_appqty = 0,
        safe_qty = now_ro * safe_perc,
        normal_qty = now_ro * normal_perc
WHERE
    EXISTS (
        SELECT
            1
        FROM
            mi_mast
        WHERE
            mmcode = a.mmcode
            AND   mat_class = '01'
    )
    AND   ro_whtype = '2'
    AND   a.ro_type IN (
        '1',
        '2'
    )
";
            return DBWork.Connection.Execute(strSql);
        }

        /// <summary>
        /// 更新 供應中心
        /// 
        /// 護理病房最大請領量, 供應中心最大請領量, 藥局請領最大量, 安全庫存量, 正常庫存量
        /// </summary>
        public int UpdateSupplyCenterQtys()
        {
            string strSql = @"
UPDATE mi_basero_14 a
    SET
        g34_max_appqty = now_ro * g34_perc,
        supply_max_appqty = now_ro * supply_perc,
        phr_max_appqty = 0,
        safe_qty = now_ro * safe_perc,
        normal_qty = now_ro * normal_perc
WHERE
    EXISTS (
        SELECT
            1
        FROM
            mi_mast
        WHERE
            mmcode = a.mmcode
            AND   mat_class = '02'
    )
    AND   ro_whtype = '3'
    AND   a.ro_type IN (
        '1',
        '2'
    )
";
            return DBWork.Connection.Execute(strSql);
        }

    }
}
