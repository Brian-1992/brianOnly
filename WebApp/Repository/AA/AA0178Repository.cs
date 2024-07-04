using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using WebApp.Models;
using WebApp.Models.MI;

namespace WebApp.Repository.AA
{
    public class AA0178Repository : JCLib.Mvc.BaseRepository
    {
        public AA0178Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        //查詢
        public IEnumerable<AA0178> GetAll(string MatClass, string MMCode, string p2, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            #region
            var sql = @" SELECT A.MMCODE MMCODE, 
                                B.MMNAME_C MMNAME_C,
                                (B.UNITRATE || ' ' || B.BASE_UNIT || '/' || B.M_PURUN) UNITRATE,
                                B.TRUTRATE TRUTRATE,
                                NVL((SELECT DATA_DESC
                                     FROM PARAM_D 
                                     WHERE GRP_CODE = 'MI_MAST' 
                                     AND DATA_NAME = 'MAT_CLASS_SUB' 
                                     AND DATA_VALUE = B.MAT_CLASS_SUB), C.MAT_CLSNAME) MAT_CLSNAME,
                                GET_PARAM('MI_BASERO_14','RO_TYPE', A.RO_TYPE) RO_TYPE, 
                                A.NOW_RO NOW_RO, 
                                A.DAY_USE_10 DAY_USE_10,  
                                A.DAY_USE_14 DAY_USE_14,  
                                A.DAY_USE_90 DAY_USE_90,   
                                A.MON_USE_1 MON_USE_1,   
                                A.MON_USE_2 MON_USE_2,    
                                A.MON_USE_3 MON_USE_3,     
                                A.MON_USE_4 MON_USE_4,     
                                A.MON_USE_5 MON_USE_5,     
                                A.MON_USE_6 MON_USE_6,     
                                A.MON_AVG_USE_3 MON_AVG_USE_3,      
                                A.MON_AVG_USE_6 MON_AVG_USE_6,      
                                A.G34_MAX_APPQTY G34_MAX_APPQTY,      
                                A.SUPPLY_MAX_APPQTY SUPPLY_MAX_APPQTY,      
                                A.PHR_MAX_APPQTY PHR_MAX_APPQTY,      
                                A.WAR_QTY WAR_QTY,      
                                A.SAFE_QTY SAFE_QTY,      
                                A.NORMAL_QTY NORMAL_QTY,      
                                A.DIFF_PERC DIFF_PERC,       
                                A.SAFE_PERC SAFE_PERC,       
                                A.DAY_RO DAY_RO,       
                                A.MON_RO MON_RO,     
                                A.G34_PERC,
                                A.SUPPLY_PERC,
                                A.PHR_PERC,
                                A.NORMAL_PERC,
                                A.WAR_PERC,
                                GET_PARAM('MI_MAST','WARBAK', B.WARBAK) WARBAK,      
                                B.BASE_UNIT BASE_UNIT,       
                                (SELECT SUM(INV_QTY)  
                                 FROM MI_WHINV   
                                 WHERE WH_NO IN (WHNO_MM1, WHNO_ME1)   
                                 AND MMCODE = A.MMCODE) INV_QTY_1,        
                                (SELECT SUM(INV_QTY)    
                                 FROM MI_WHINV    
                                 WHERE WH_NO IN (SELECT WH_NO    
                                                 FROM MI_WHMAST     
                                                 WHERE WH_KIND = '0'     
                                                 AND WH_GRADE = '2')     
                                 AND MMCODE = A.MMCODE
                                 AND WH_NO=A.WH_NO) INV_QTY_2,         
                                (SELECT SUM(INV_QTY)      
                                 FROM MI_WHINV      
                                 WHERE WH_NO IN (SELECT WH_NO     
                                                 FROM MI_WHMAST     
                                                 WHERE WH_NAME LIKE '%供應中心%')       
                                 AND MMCODE = A.MMCODE) INV_QTY_3,    
                                 a.wh_no WH_NO, 
                                 user_name(a.update_user) as UPDATE_USER, 
                                 twn_time(a.update_time) as UPDATE_TIME,
                                A.RO_WHTYPE
                         FROM MI_BASERO_14 A, MI_MAST B, MI_MATCLASS C
                         WHERE A.MMCODE = B.MMCODE AND B.MAT_CLASS = C.MAT_CLASS 
                         AND A.RO_WHTYPE = '2' ";

            if (MatClass.Trim() != "")
            {
                sql += " AND B.MAT_CLASS_SUB = :MAT_CLASS_SUB_TEXT ";
            }
            if (MMCode.Trim() != "")
            {
                sql += " AND A.MMCODE = :MMCODE_TEXT ";
            }
            if (!String.IsNullOrEmpty(p2.Trim()))
            {
                if (p2.Trim() != "全院藥局")
                {
                    sql += " AND A.WH_NO = :p2";
                    p.Add(":p2", p2);
                }
                else if (p2.Trim() == "全院藥局")
                {
                    sql = @"SELECT MMCODE, 
                                   MMNAME_C, 
                                   UNITRATE, 
                                   TRUTRATE, 
                                   MAT_CLSNAME, 
                                   RO_TYPE, 
                                   WARBAK, 
                                   BASE_UNIT, 
                                   RO_WHTYPE, 
                                   SUM(NOW_RO) NOW_RO, 
                                   SUM(DAY_USE_10) DAY_USE_10, 
                                   SUM(DAY_USE_14) DAY_USE_14, 
                                   SUM(DAY_USE_90) DAY_USE_90,  
                                   SUM(MON_USE_1) MON_USE_1, 
                                   SUM(MON_USE_2) MON_USE_2, 
                                   SUM(MON_USE_3) MON_USE_3, 
                                   SUM(MON_USE_4) MON_USE_4, 
                                   SUM(MON_USE_5) MON_USE_5, 
                                   SUM(MON_USE_6) MON_USE_6, 
                                   SUM(MON_AVG_USE_3) MON_AVG_USE_3, 
                                   SUM(MON_AVG_USE_6) MON_AVG_USE_6, 
                                   SUM(G34_MAX_APPQTY) G34_MAX_APPQTY, 
                                   SUM(SUPPLY_MAX_APPQTY) SUPPLY_MAX_APPQTY, 
                                   SUM(PHR_MAX_APPQTY) PHR_MAX_APPQTY, 
                                   SUM(WAR_QTY) WAR_QTY, 
                                   SUM(SAFE_QTY) SAFE_QTY, 
                                   SUM(NORMAL_QTY) NORMAL_QTY, 
                                   SUM(DIFF_PERC) DIFF_PERC, 
                                   SUM(SAFE_PERC) SAFE_PERC, 
                                   SUM(DAY_RO) DAY_RO, 
                                   SUM(MON_RO) MON_RO, 
                                   SUM(G34_PERC) G34_PERC, 
                                   SUM(SUPPLY_PERC) SUPPLY_PERC, 
                                   SUM(PHR_PERC) PHR_PERC, 
                                   SUM(NORMAL_PERC) NORMAL_PERC, 
                                   SUM(WAR_PERC) WAR_PERC, 
                                   SUM(INV_QTY_1) INV_QTY_1, 
                                   SUM(INV_QTY_2) INV_QTY_2, 
                                   SUM(INV_QTY_3) INV_QTY_3
                            FROM ( " + sql + @" ) data
                            GROUP BY MMCODE, MMNAME_C, UNITRATE, TRUTRATE, MAT_CLSNAME, RO_TYPE, WARBAK, BASE_UNIT, RO_WHTYPE
                            ORDER BY MMCODE";
                }
            }
            #endregion

            p.Add(":MAT_CLASS_SUB_TEXT", MatClass);
            p.Add(":MMCODE_TEXT", MMCode);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0178>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public MI_BASERO_14 GetMI_BASERO_14(string strMmcode, string strWhNo)
        {
            string sql = @"select * from mi_basero_14 where mmcode = :mmcode and wh_no =: wh_no";

            return DBWork.Connection.QueryFirstOrDefault<MI_BASERO_14>(sql, new { mmcode = strMmcode, wh_no = strWhNo });
        }
        //匯出
        public DataTable GetExcel(string MatClass, string MMCode, string p2)
        {
            DynamicParameters p = new DynamicParameters();

            #region
            var sql = @" SELECT A.MMCODE 院內碼, 
                                B.MMNAME_C 品名,
                                (B.UNITRATE || ' ' || B.BASE_UNIT || '/' || B.M_PURUN) 出貨單位,
                                B.TRUTRATE 轉換量,
                                NVL((SELECT DATA_DESC
                                     FROM PARAM_D 
                                     WHERE GRP_CODE = 'MI_MAST' 
                                     AND DATA_NAME = 'MAT_CLASS_SUB' 
                                     AND DATA_VALUE = B.MAT_CLASS_SUB), C.MAT_CLSNAME) 類別,
                                GET_PARAM('MI_BASERO_14','RO_TYPE', A.RO_TYPE) 基準量模式, 
                                A.NOW_RO 現用基準量, 
                                A.DAY_USE_10 日平均消耗10天,  
                                A.DAY_USE_14 日平均消耗14天,  
                                A.DAY_USE_90 日平均消耗90天,   
                                A.MON_USE_1 前第一個月消耗,   
                                A.MON_USE_2 前第二個月消耗,    
                                A.MON_USE_3 前第三個月消耗,     
                                A.MON_USE_4 前第四個月消耗,     
                                A.MON_USE_5 前第五個月消耗,     
                                A.MON_USE_6 前第六個月消耗,     
                                A.MON_AVG_USE_3 三個月平均消耗量,      
                                A.MON_AVG_USE_6 六個月平均消耗量,      
                                A.G34_MAX_APPQTY 護理病房最大請領量,      
                                A.SAFE_QTY 安全庫存量,      
                                A.NORMAL_QTY 正常庫存量,      
                                A.DIFF_PERC 誤差百分比
                         FROM MI_BASERO_14 A, MI_MAST B, MI_MATCLASS C
                         WHERE A.MMCODE = B.MMCODE AND B.MAT_CLASS = C.MAT_CLASS 
                         AND A.RO_WHTYPE = '2' ";

            if (MatClass.Trim() != "")
            {
                sql += " AND B.MAT_CLASS_SUB = :MAT_CLASS_SUB_TEXT ";
            }
            if (MMCode.Trim() != "")
            {
                sql += " AND A.MMCODE = :MMCODE_TEXT ";
            }
            if (!String.IsNullOrEmpty(p2.Trim()))
            {
                if (p2.Trim() != "全院藥局")
                {
                    sql += " AND A.WH_NO = :p2";
                    p.Add(":p2", p2);
                }
                else if (p2.Trim() == "全院藥局")
                {
                    sql = @"SELECT 院內碼, 
                                   品名, 
                                   出貨單位, 
                                   轉換量, 
                                   類別, 
                                   基準量模式, 
                                   SUM(現用基準量) 現用基準量, 
                                   SUM(日平均消耗10天) 日平均消耗10天, 
                                   SUM(日平均消耗14天) 日平均消耗14天, 
                                   SUM(日平均消耗90天) 日平均消耗90天,  
                                   SUM(前第一個月消耗) 前第一個月消耗, 
                                   SUM(前第二個月消耗) 前第二個月消耗, 
                                   SUM(前第三個月消耗) 前第三個月消耗, 
                                   SUM(前第四個月消耗) 前第四個月消耗, 
                                   SUM(前第五個月消耗) 前第五個月消耗, 
                                   SUM(前第六個月消耗) 前第六個月消耗, 
                                   SUM(三個月平均消耗量) 三個月平均消耗量, 
                                   SUM(六個月平均消耗量) 六個月平均消耗量, 
                                   SUM(護理病房最大請領量) 護理病房最大請領量, 
                                   SUM(安全庫存量) 安全庫存量, 
                                   SUM(正常庫存量) 正常庫存量, 
                                   SUM(誤差百分比) 誤差百分比
                            FROM ( " + sql + @" ) data
                            GROUP BY 院內碼, 品名, 出貨單位, 轉換量, 類別, 基準量模式
                            ORDER BY 院內碼";
                }
            }
            #endregion

            p.Add(":MAT_CLASS_SUB_TEXT", MatClass);
            p.Add(":MMCODE_TEXT", MMCode);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
        //物料類別combo
        public IEnumerable<COMBO_MODEL> GetMatClassSubCombo()
        {
            string sql = @"  SELECT DISTINCT
                                data_value AS VALUE,
                                data_desc AS TEXT,
                                data_desc
                                || ' '
                                || data_value AS combitem
                            FROM
                                param_d
                            WHERE
                                data_name = upper('mat_class_sub')
                                AND  data_value = '1' ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        //院內碼combo
        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, string p1, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = @"  SELECT {0}
                                    MMCODE, 
                                    MMNAME_C,  
                                    MMNAME_E 
                             FROM MI_MAST
                             WHERE 1 = 1 ";
            if (p1.Trim() != "")
            {
                sql += "     AND MAT_CLASS_SUB = TRIM(:MAT_CLASS_TEXT) ";
                p.Add(":MAT_CLASS_TEXT", p1);
            }
            if (p0.Trim() != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += @"     AND ( MMCODE LIKE :MMCODE
                                    OR MMNAME_E LIKE UPPER(:MMNAME_E) 
                                    OR MMNAME_C LIKE UPPER(:MMNAME_C) )   ";
                p.Add(":MMCODE", string.Format("{0}%", p0));
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));
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

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p);
        }
        public IEnumerable<MI_WHID> GetWH_NoCombo(string userId)
        {
            string sql = @"SELECT '全院藥局' WH_NO,
                                  '全院藥局' WH_NAME,
                                  0 SORT
                           FROM DUAL
                           UNION ALL
                           SELECT A.WH_NO WH_NO,
                                  A.WH_NAME WH_NAME,
                                  1 SORT
                           FROM MI_WHMAST A, MI_WHID B
                           WHERE A.WH_NO = B.WH_NO
                           AND   A.WH_KIND = '0'
                           AND   WH_GRADE = '2'
                           AND   CANCEL_ID = 'N'
                           AND   B.WH_USERID = :USER_ID
                           ORDER BY SORT, WH_NO";
            return DBWork.Connection.Query<MI_WHID>(sql, new { user_id = userId });
        }

        //修改
        public int Update(AA0178 AA0178)
        {
            var sql = @"UPDATE MI_BASERO_14
                           SET RO_TYPE = :RO_TYPE,
                               DAY_RO = :DAY_RO,
                               NOW_RO = :NOW_RO,
                               SAFE_QTY = :SAFE_QTY,
                               NORMAL_QTY = :NORMAL_QTY,
                               G34_MAX_APPQTY = :G34_MAX_APPQTY,
                               DIFF_PERC = :DIFF_PERC, 
                               update_time = sysdate, 
                               update_user=:update_user, 
                               update_ip=:update_ip
                         WHERE MMCODE = :MMCODE AND WH_NO = :WH_NO AND RO_WHTYPE = '2' ";
            return DBWork.Connection.Execute(sql, AA0178, DBWork.Transaction);
        }


        public int UpdatePerc(AA0178 AA0178)
        {
            var sql = @"update MI_BASERO_14 
                        set safe_perc = :safe_perc, 
                        normal_perc=:normal_perc, 
                        g34_perc=:g34_perc, 
                        safe_qty = :safe_qty,
                        normal_qty = :normal_qty,
                        g34_max_appqty = :g34_max_appqty,
                        update_time = sysdate, 
                        update_user=:update_user, 
                        update_ip=:update_ip
                        where wh_no = :WH_NO 
                        and mmcode = :MMCODE 
                        and ro_whtype = '2'";
            return DBWork.Connection.Execute(sql, AA0178, DBWork.Transaction);
        }

        /**
         * ToolBar 匯入功能
         */
        public DataTable GetExcelExample()
        {
            DynamicParameters p = new DynamicParameters();

            string sql = @" SELECT '' 院內碼, 
                                '' 品名,
                                '' 出貨單位,
                                0 轉換量,
                                '' 類別,
                                '' 基準量模式, 
                                0 現用基準量,     
                                0 護理病房最大請領量,      
                                0 安全庫存量,      
                                0 正常庫存量,      
                                0 誤差百分比
                    FROM DUAL";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        //檢查此院內碼是否存在
        public bool CheckExistsMMCODE(string MMCODE)
        {
            string sql = @"SELECT 1
                        FROM mi_mast
                        WHERE
                            mmcode =:mmcode
                            AND   mat_class_sub IN (
                                SELECT DISTINCT
                                    data_value
                                FROM
                                    param_d
                                WHERE
                                    data_name = upper('mat_class_sub')
                                    AND   data_value = '1'
                            )";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = MMCODE }, DBWork.Transaction) == null);
        }

        public bool CheckExistsWH_NO(string WH_NO)
        {
            string sql = @"SELECT 1
                        FROM mi_whmast a, mi_whid b
                        WHERE a.wh_no =:wh_no
                            AND a.wh_no = b.wh_no
                            AND   a.wh_kind = '0'
                            AND   wh_grade = '2'
                            AND   cancel_id = 'N'
                            AND   b.wh_userid =:user_id
                        ORDER BY a.wh_no";
            return !(DBWork.Connection.ExecuteScalar(sql, new { WH_NO = WH_NO }, DBWork.Transaction) == null);
        }

        public bool CheckExistsPARAM_D(string strDataValue, string strDataName)
        {
            string sql = @"SELECT 1
                           FROM param_d
                           WHERE data_name =:data_name
                            AND   data_value =:data_value";
            return !(DBWork.Connection.ExecuteScalar(sql, new { data_name = strDataName, data_value = strDataValue }, DBWork.Transaction) == null);
        }

        public bool CheckExistsCompundKey(string strMMcode, string strWhNo)
        {
            string sql = @"SELECT 1
                    FROM mi_basero_14
                    WHERE
                        RO_WHTYPE = '2'
                        AND mmcode =:mmcode
                        AND   wh_no =:wh_no";
            return !(DBWork.Connection.ExecuteScalar(sql, new { mmcode = strMMcode, wh_no = strWhNo }, DBWork.Transaction) == null);
        }

        public int UpdateMI_BASERO_14(MI_BASERO_14 mi_basero_14)
        {
            var sql = @"UPDATE MI_BASERO_14
                           SET RO_TYPE = :RO_TYPE,
                               DAY_RO = :DAY_RO,
                               NOW_RO = :NOW_RO,
                               SAFE_QTY = :SAFE_QTY,
                               NORMAL_QTY = :NORMAL_QTY,
                               G34_MAX_APPQTY = :G34_MAX_APPQTY,
                               SUPPLY_MAX_APPQTY = :SUPPLY_MAX_APPQTY,
                               PHR_MAX_APPQTY = :PHR_MAX_APPQTY,
                               WAR_QTY = :WAR_QTY,
                               DIFF_PERC = :DIFF_PERC, 
                               UPDATE_TIME = sysdate, 
                               UPDATE_USER= :UPDATE_USER, 
                               UPDATE_IP= :UPDATE_IP
                         WHERE MMCODE = :MMCODE AND WH_NO = :WH_NO AND  RO_WHTYPE = '2' ";
            return DBWork.Connection.Execute(sql, mi_basero_14, DBWork.Transaction);
        }

        public int UpdateFromXLS(MI_BASERO_14 mi_basero_14)
        {
            var sql = @"UPDATE MI_BASERO_14
                        SET RO_WHTYPE =:RO_WHTYPE, RO_TYPE =:RO_TYPE, NOW_RO =:NOW_RO, G34_MAX_APPQTY =:G34_MAX_APPQTY,
                            SUPPLY_MAX_APPQTY =:SUPPLY_MAX_APPQTY, PHR_MAX_APPQTY =:PHR_MAX_APPQTY, WAR_QTY =:WAR_QTY, SAFE_QTY =:SAFE_QTY, NORMAL_QTY =:NORMAL_QTY,
                            DIFF_PERC =:DIFF_PERC, SAFE_PERC =:SAFE_PERC, DAY_RO =:DAY_RO, MON_RO =:MON_RO, G34_PERC =:G34_PERC,
                            SUPPLY_PERC =:SUPPLY_PERC, PHR_PERC =:PHR_PERC, NORMAL_PERC =:NORMAL_PERC, WAR_PERC =:WAR_PERC,
                            UPDATE_TIME =sysdate, UPDATE_USER=:UPDATE_USER, UPDATE_IP=:UPDATE_IP
                        WHERE MMCODE =:MMCODE AND WH_NO =:WH_NO  AND  RO_WHTYPE = '2'";
            return DBWork.Connection.Execute(sql, mi_basero_14, DBWork.Transaction);
        }
        public int InsertFromXLS(MI_BASERO_14 mi_basero_14)
        {
            var sql = @"INSERT INTO MI_BASERO_14 (
                            MMCODE, RO_WHTYPE, RO_TYPE, NOW_RO,
                            G34_MAX_APPQTY, SUPPLY_MAX_APPQTY, PHR_MAX_APPQTY, WAR_QTY, SAFE_QTY,
                            NORMAL_QTY, DIFF_PERC, SAFE_PERC, DAY_RO, MON_RO,
                            G34_PERC, SUPPLY_PERC, PHR_PERC, NORMAL_PERC, WAR_PERC,
                            WH_NO,
                            CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP
                        )  
                        VALUES (
                            :MMCODE, :RO_WHTYPE, :RO_TYPE, :NOW_RO,
                            :G34_MAX_APPQTY, :SUPPLY_MAX_APPQTY, :PHR_MAX_APPQTY, :WAR_QTY, :SAFE_QTY,
                            :NORMAL_QTY, :DIFF_PERC, :SAFE_PERC, :DAY_RO, :MON_RO,
                            :G34_PERC, :SUPPLY_PERC, :PHR_PERC, :NORMAL_PERC, :WAR_PERC,
                            :WH_NO,
                            sysdate, :CREATE_USER, sysdate, :UPDATE_USER, :UPDATE_IP
                        )";
            return DBWork.Connection.Execute(sql, mi_basero_14, DBWork.Transaction);
        }
    }
}