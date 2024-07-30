using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Data;
using System;
using System.Data.SqlClient;

namespace WebApp.Repository.F
{

    public class FA0008_MODEL : JCLib.Mvc.BaseModel
    {
        public string ROWORDER { get; set; }       // 排序用
        public string MAT { get; set; }            // 類別
        public Double PIQ_PA_G1 { get; set; }      // 期初存貨成本 - 中央庫房
        public Double PIQ_PA_G2 { get; set; }      // 期初存貨成本 - 衛星庫房
        public Double IQ_IP { get; set; }          // 進貨成本
        public Double OQ_A_AP_G1 { get; set; }     // 內湖消耗成本 - 中央庫房
        public Double OQ_A_AP_G2 { get; set; }     // 內湖消耗成本 - 衛星庫房
        public Double I_AP_G1 { get; set; }      // 盤盈虧 - 中央庫房
        public Double I_AP_G2 { get; set; }      // 盤盈虧 - 衛星庫房
        public Double A_PA { get; set; }           // 台北門診應收帳款,
        public Double IQ_PA_G1 { get; set; }       // 期末庫存成本 - 中央庫房
        public Double IQ_PA_G2 { get; set; }       // 期末庫存成本 - 中央庫房
        public Double A_PC { get; set; }           // 寄售衛材消耗成本

        public Double I_AP_G1_P { get; set; }       // 盤盈-中央庫房
        public Double I_AP_G1_N { get; set; }       // 盤虧-中央庫房
        public Double I_AP_G2_P { get; set; }       // 盤盈-衛星庫房
        public Double I_AP_G2_N { get; set; }       // 盤虧-衛星庫房
    }
    public class FA0008Repository : JCLib.Mvc.BaseRepository
    {
        public FA0008Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<FA0008M> GetAll(string DATA_YM, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = @" SELECT 1 AS ROWORDER,
                                   '　戰備-庫備品' AS MAT, --""類別"",
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',A.P_INV_QTY,0)* A.PMN_MIL_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G1 , --中央庫房期初存貨成本,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',0,A.P_INV_QTY)* A.PMN_MIL_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G2, --衛星庫房期初存貨成本,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(A.INCOST), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_IP, --進貨成本,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',A.OUT_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G1, --中央庫房內湖消耗成本,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',0,(A.OUT_QTY - NVL(A.OUT_QTYA, 0)) * A.AVG_PRICE)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G2, --衛星庫房內湖消耗成本,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',A.INVENTORYQTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1, --中央庫房盤盈虧,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_P, --中央庫房盤盈,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_N, --中央庫房盤虧,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',0,A.INVENTORYQTY) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2, --衛星庫房盤盈虧,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',0,(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_P, --衛星庫房盤盈,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',0,(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_N, --衛星庫房盤虧,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(NVL(A.OUT_QTYA, 0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PA, --台北門診應收帳款,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',A.INV_QTY,0) * A.MIL_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G1, --中央庫房期末庫存成本
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',0,A.INV_QTY) * A.MIL_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G2, --衛星庫房期末庫存成本
                                   TRIM(TO_CHAR(CAST('0' AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PC --寄售衛材消耗成本
                               FROM
                                   V_COST_FA0008 A
                               WHERE
                                   1 = 1                            
                                   AND A.DATA_YM=:DATA_YM AND A.M_STOREID='1' AND A.MAT_CLASS='02'
                                   AND A.WH_NO = 'MM1X'

                            union 

                            SELECT 2 AS ROWORDER,
                                '　戰備-非庫備品' AS MAT, --""類別"",
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',A.P_INV_QTY,0)* A.PMN_MIL_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G1 , --中央庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',0,A.P_INV_QTY)* A.PMN_MIL_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G2, --衛星庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(A.INCOST), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_IP, --進貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',A.OUT_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G1, --中央庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',0,(A.OUT_QTY - NVL(A.OUT_QTYA, 0)) * A.AVG_PRICE)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G2, --衛星庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',A.INVENTORYQTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1, --中央庫房盤盈虧,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_P, --中央庫房盤盈,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_N, --中央庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',0,A.INVENTORYQTY) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2, --衛星庫房盤盈虧,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',0,(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_P, --衛星庫房盤盈,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',0,(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_N, --衛星庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(NVL(A.OUT_QTYA, 0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PA, --台北門診應收帳款,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',A.INV_QTY,0) * A.MIL_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G1, --中央庫房期末庫存成本
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',0,A.INV_QTY) * A.MIL_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G2, --衛星庫房期末庫存成本
                                TRIM(TO_CHAR(CAST('0' AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PC --寄售衛材消耗成本
                            FROM
                                V_COST_FA0008 A
                            WHERE
                                1 = 1                            
                                AND A.DATA_YM=:DATA_YM AND A.M_STOREID='0' AND A.MAT_CLASS='02'
                                AND A.WH_NO = 'MM1X'
                            
                            	union
                            
                            SELECT 3 AS ROWORDER,
                                '　民品-庫備品' AS MAT, --""類別"",
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.P_INV_QTY,0)* A.PMN_AVGPRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G1, --中央庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.P_INV_QTY)* A.PMN_AVGPRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G2, --衛星庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(A.INCOST), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_IP, --進貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.OUT_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G1, --中央庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(A.OUT_QTY - NVL(A.OUT_QTYA, 0)) * A.AVG_PRICE)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G2, --衛星庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.INVENTORYQTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1, --中央庫房盤盈虧,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_P, --中央庫房盤盈,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_N, --中央庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.INVENTORYQTY) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2, --衛星庫房盤盈虧,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_P, --衛星庫房盤盈,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_N, --衛星庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(NVL(A.OUT_QTYA, 0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PA, --台北門診應收帳款,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.INV_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G1, --中央庫房期末庫存成本
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.INV_QTY) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G2, --衛星庫房期末庫存成本
                                TRIM(TO_CHAR(CAST(ROUND(MMCODE_COST_C(:DATA_YM, '1'), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PC --寄售衛材消耗成本
                            FROM
                                V_COST_FA0008 A
                            WHERE
                                1 = 1                            
                                AND A.DATA_YM=:DATA_YM AND A.M_STOREID='1' AND A.MAT_CLASS='02'
                                AND A.WH_NO <> 'MM1X'
                            
                            	UNION
                            
                            SELECT 4 AS ROWORDER,
                                '　民品-非庫備品' AS MAT, --""類別"",
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.P_INV_QTY,0)* A.PMN_AVGPRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G1, --中央庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.P_INV_QTY)* A.PMN_AVGPRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G2, --衛星庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(A.INCOST), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_IP, --進貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.OUT_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G1, --中央庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(A.OUT_QTY - NVL(A.OUT_QTYA, 0)) * A.AVG_PRICE)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G2, --衛星庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.INVENTORYQTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1, --中央庫房盤盈虧,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_P, --中央庫房盤盈,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_N, --中央庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.INVENTORYQTY) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2, --衛星庫房盤盈虧,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_P, --衛星庫房盤盈,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_N, --衛星庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(NVL(A.OUT_QTYA, 0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PA, --台北門診應收帳款,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.INV_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G1, --中央庫房期末庫存成本
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.INV_QTY) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G2, --衛星庫房期末庫存成本
                                TRIM(TO_CHAR(CAST(ROUND(MMCODE_COST_C(:DATA_YM, '0'), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PC --寄售衛材消耗成本
                            FROM
                                V_COST_FA0008 A
                            WHERE
                                1 = 1                            
                                AND A.DATA_YM=:DATA_YM AND A.M_STOREID='0' AND A.MAT_CLASS='02'
                                AND A.WH_NO <> 'MM1X'
                            
                            	union
                            
                            SELECT 5 AS ROWORDER,
                                   '小計' AS MAT, --""類別"",
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'2',0,A.P_INV_QTY)* DECODE(A.WH_GRADE,'5',A.PMN_MIL_PRICE,A.PMN_AVGPRICE)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G1, --中央庫房期初存貨成本,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'2',A.P_INV_QTY,0)* DECODE(A.WH_GRADE,'5',A.PMN_MIL_PRICE,A.PMN_AVGPRICE)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G2, --衛星庫房期初存貨成本,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(A.INCOST), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_IP, --進貨成本,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'2',0,A.OUT_QTY) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G1, --中央庫房內湖消耗成本,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'2',(A.OUT_QTY - NVL(A.OUT_QTYA, 0)),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G2, --衛星庫房內湖消耗成本,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'2',0,A.INVENTORYQTY) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1, --中央庫房盤盈虧,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'2',0,(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_P, --中央庫房盤盈,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'2',0,(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_N, --中央庫房盤虧,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'2',A.INVENTORYQTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2, --衛星庫房盤盈虧,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'2',(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_P, --衛星庫房盤盈,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'2',(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_N, --衛星庫房盤虧,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(NVL(A.OUT_QTYA, 0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PA, --台北門診應收帳款,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'2',0,A.INV_QTY) * DECODE(A.WH_GRADE,'5',A.MIL_PRICE,A.AVG_PRICE)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G1, --中央庫房期末庫存成本
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'2',A.INV_QTY,0) * DECODE(A.WH_GRADE,'5',A.MIL_PRICE,A.AVG_PRICE)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G2, --衛星庫房期末庫存成本
                                   TRIM(TO_CHAR(CAST(ROUND(MMCODE_COST_C(:DATA_YM, ''), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PC --寄售衛材消耗成本
                               FROM
                                   V_COST_FA0008 A
                               WHERE
                                   1 = 1                            
                                   AND A.DATA_YM=:DATA_YM AND A.MAT_CLASS='02'
                            
                            union
                            
                            SELECT
                                6 AS ROWORDER,
                                '氣體' AS MAT, --""類別"",
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.P_INV_QTY,0)* A.PMN_AVGPRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G1, --中央庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.P_INV_QTY)* A.PMN_AVGPRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G2, --衛星庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(A.INCOST), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_IP, --進貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.OUT_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G1, --中央庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(A.OUT_QTY - NVL(A.OUT_QTYA, 0) ) * A.AVG_PRICE)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G2, --衛星庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.INVENTORYQTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1, --中央庫房盤盈虧,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_P, --中央庫房盤盈,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_N, --中央庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.INVENTORYQTY) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2, --衛星庫房盤盈虧,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_P, --衛星庫房盤盈,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_N, --衛星庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(NVL(A.OUT_QTYA, 0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PA, --台北門診應收帳款,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.INV_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G1, --中央庫房期末庫存成本
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.INV_QTY) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G2, --衛星庫房期末庫存成本
                                TRIM(TO_CHAR(CAST('0' AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PC --寄售衛材消耗成本
                            FROM
                                V_COST_FA0008 A
                            WHERE
                                1 = 1                       
                                AND A.DATA_YM = :DATA_YM AND A.MAT_CLASS = '09'
                                AND A.WH_NO <> 'MM1X'
                            
                            union
                            
                            SELECT
                                7 AS ROWORDER,
                                '文具' AS MAT, --""類別"",
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.P_INV_QTY,0)* A.PMN_AVGPRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G1, --中央庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.P_INV_QTY)* A.PMN_AVGPRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G2, --衛星庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(A.INCOST), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_IP, --進貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.OUT_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G1, --中央庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(A.OUT_QTY - NVL(A.OUT_QTYA, 0)) * A.AVG_PRICE)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G2, --衛星庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.INVENTORYQTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1, --中央庫房盤盈虧,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_P, --中央庫房盤盈,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_N, --中央庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.INVENTORYQTY) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2, --衛星庫房盤盈虧,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_P, --衛星庫房盤盈,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_N, --衛星庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(NVL(A.OUT_QTYA, 0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PA, --台北門診應收帳款,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.INV_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G1, --中央庫房期末庫存成本
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.INV_QTY) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G2, --衛星庫房期末庫存成本
                                TRIM(TO_CHAR(CAST('0' AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PC --寄售衛材消耗成本
                            FROM
                                V_COST_FA0008 A
                            WHERE
                                1 = 1                       
                                AND A.DATA_YM = :DATA_YM AND A.MAT_CLASS = '03'
                                AND A.WH_NO <> 'MM1X'
                            
                            union
                            
                            SELECT
                                8 AS ROWORDER,
                            	'清潔用品' AS MAT, --""類別"",
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.P_INV_QTY,0)* A.PMN_AVGPRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G1, --中央庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.P_INV_QTY)* A.PMN_AVGPRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G2, --衛星庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(A.INCOST), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_IP, --進貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.OUT_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G1, --中央庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(A.OUT_QTY - NVL(A.OUT_QTYA, 0)) * A.AVG_PRICE)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G2, --衛星庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.INVENTORYQTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1, --中央庫房盤盈虧,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_P, --中央庫房盤盈,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_N, --中央庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.INVENTORYQTY) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2, --衛星庫房盤盈虧,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_P, --衛星庫房盤盈,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_N, --衛星庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(NVL(A.OUT_QTYA, 0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PA, --台北門診應收帳款,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.INV_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G1, --中央庫房期末庫存成本
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.INV_QTY) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G2, --衛星庫房期末庫存成本
                                TRIM(TO_CHAR(CAST('0' AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PC --寄售衛材消耗成本
                            FROM
                                V_COST_FA0008 A
                            WHERE
                                1 = 1                       
                                AND A.DATA_YM = :DATA_YM AND A.MAT_CLASS = '04'
                                AND A.WH_NO <> 'MM1X'
                            
                            union
                            
                            SELECT
                                9 AS ROWORDER,
                            	'醫療表格' AS MAT, --""類別"",,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.P_INV_QTY,0)* A.PMN_AVGPRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G1, --中央庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.P_INV_QTY)* A.PMN_AVGPRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G2, --衛星庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(A.INCOST), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_IP, --進貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.OUT_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G1, --中央庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(A.OUT_QTY - NVL(A.OUT_QTYA, 0)) * A.AVG_PRICE)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G2, --衛星庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.INVENTORYQTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1, --中央庫房盤盈虧,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_P, --中央庫房盤盈,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_N, --中央庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.INVENTORYQTY) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2, --衛星庫房盤盈虧,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_P, --衛星庫房盤盈,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_N, --衛星庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(NVL(A.OUT_QTYA, 0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PA, --台北門診應收帳款,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.INV_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G1, --中央庫房期末庫存成本
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.INV_QTY) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G2, --衛星庫房期末庫存成本
                                TRIM(TO_CHAR(CAST('0' AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PC --寄售衛材消耗成本
                            FROM
                                V_COST_FA0008 A
                            WHERE
                                1 = 1                       
                                AND A.DATA_YM = :DATA_YM AND A.MAT_CLASS = '05'
                                AND A.WH_NO <> 'MM1X'
                            
                            union
                            
                            SELECT
                                10 AS ROWORDER,
                            	'防護用具' AS MAT, --""類別"",,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.P_INV_QTY,0)* A.PMN_AVGPRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G1, --中央庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.P_INV_QTY)* A.PMN_AVGPRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G2, --衛星庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(A.INCOST), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_IP, --進貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.OUT_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G1, --中央庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(A.OUT_QTY - NVL(A.OUT_QTYA, 0)) * A.AVG_PRICE)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G2, --衛星庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.INVENTORYQTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1, --中央庫房盤盈虧,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_P, --中央庫房盤盈,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_N, --中央庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.INVENTORYQTY) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2, --衛星庫房盤盈虧,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_P, --衛星庫房盤盈,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_N, --衛星庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(NVL(A.OUT_QTYA, 0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PA, --台北門診應收帳款,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.INV_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G1, --中央庫房期末庫存成本
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.INV_QTY) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G2, --衛星庫房期末庫存成本
                                TRIM(TO_CHAR(CAST('0' AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PC --寄售衛材消耗成本
                            FROM
                                V_COST_FA0008 A
                            WHERE
                                1 = 1                       
                                AND A.DATA_YM = :DATA_YM AND A.MAT_CLASS = '06'
                                AND A.WH_NO <> 'MM1X'
                            
                            union
                            
                            SELECT
                                11 AS ROWORDER,
                            	'資訊耗材' AS MAT, --""類別"",,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.P_INV_QTY,0)* A.PMN_AVGPRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G1, --中央庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.P_INV_QTY)* A.PMN_AVGPRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G2, --衛星庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(A.INCOST), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_IP, --進貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.OUT_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G1, --中央庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(A.OUT_QTY - NVL(A.OUT_QTYA, 0)) * A.AVG_PRICE)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G2, --衛星庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.INVENTORYQTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1, --中央庫房盤盈虧,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_P, --中央庫房盤盈,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_N, --中央庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.INVENTORYQTY) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2, --衛星庫房盤盈虧,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_P, --衛星庫房盤盈,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_N, --衛星庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(NVL(A.OUT_QTYA, 0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PA, --台北門診應收帳款,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.INV_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G1, --中央庫房期末庫存成本
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.INV_QTY) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G2, --衛星庫房期末庫存成本
                                TRIM(TO_CHAR(CAST('0' AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PC --寄售衛材消耗成本
                            FROM
                                V_COST_FA0008 A
                            WHERE
                                1 = 1                       
                                AND A.DATA_YM = :DATA_YM AND A.MAT_CLASS = '08'
                                AND A.WH_NO <> 'MM1X'
                            
                            union
                            
                            SELECT
                                12 AS ROWORDER,
                            	'一般物品小計' AS MAT, --""類別"",
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.P_INV_QTY,0)* A.PMN_AVGPRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G1, --中央庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.P_INV_QTY)* A.PMN_AVGPRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G2, --衛星庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(A.INCOST), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_IP, --進貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.OUT_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G1, --中央庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(A.OUT_QTY - NVL(A.OUT_QTYA, 0) ) * A.AVG_PRICE)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G2, --衛星庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.INVENTORYQTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1, --中央庫房盤盈虧,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_P, --中央庫房盤盈,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_N, --中央庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.INVENTORYQTY) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2, --衛星庫房盤盈虧,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_P, --衛星庫房盤盈,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_N, --衛星庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(NVL(A.OUT_QTYA, 0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PA, --台北門診應收帳款,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.INV_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G1, --中央庫房期末庫存成本
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.INV_QTY) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G2, --衛星庫房期末庫存成本
                                TRIM(TO_CHAR(CAST('0' AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PC --寄售衛材消耗成本
                            FROM
                                V_COST_FA0008 A
                            WHERE
                                1 = 1                       
                                AND A.DATA_YM = :DATA_YM AND A.MAT_CLASS IN ('03', '04', '05', '06', '08')
                                AND A.WH_NO <> 'MM1X'
                            
                            union
                            
                            SELECT
                                13 AS ROWORDER,
                            	'被服用品' AS MAT, --""類別"",
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.P_INV_QTY,0)* A.PMN_AVGPRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G1, --中央庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.P_INV_QTY)* A.PMN_AVGPRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G2, --衛星庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(A.INCOST), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_IP, --進貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.OUT_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G1, --中央庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(A.OUT_QTY - NVL(A.OUT_QTYA, 0)) * A.AVG_PRICE)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G2, --衛星庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.INVENTORYQTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1, --中央庫房盤盈虧,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_P, --中央庫房盤盈,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_N, --中央庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.INVENTORYQTY) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2, --衛星庫房盤盈虧,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_P, --衛星庫房盤盈,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_N, --衛星庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(NVL(A.OUT_QTYA, 0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PA, --台北門診應收帳款,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.INV_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G1, --中央庫房期末庫存成本
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.INV_QTY) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G2, --衛星庫房期末庫存成本
                                TRIM(TO_CHAR(CAST('0' AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PC --寄售衛材消耗成本
                            FROM
                                V_COST_FA0008 A
                            WHERE
                                1 = 1                       
                                AND A.DATA_YM = :DATA_YM AND A.MAT_CLASS = '07'
                                AND A.WH_NO <> 'MM1X'
                            ";

            p.Add(":DATA_YM", string.Format("{0}", DATA_YM));

            sql += @" ORDER BY ROWORDER ";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            //return DBWork.Connection.Query<FA0008M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
            List<FA0008M> list = new List<FA0008M>();
            FA0008M bc = new FA0008M();
            bc.MAT = "衛材";
            list.Add(bc);
            foreach (FA0008M _FA0008M in DBWork.Connection.Query<FA0008M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction))
            {
                list.Add(_FA0008M);
            }
            return list;
        }

        public IEnumerable<FA0008_MODEL> Print(string DATA_YM)
        {
            var p = new DynamicParameters();

            string sql = @" SELECT 1 AS ROWORDER,
                                   '　戰備-庫備品' AS MAT, --""類別"",
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',A.P_INV_QTY,0)* A.PMN_MIL_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G1 , --中央庫房期初存貨成本,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',0,A.P_INV_QTY)* A.PMN_MIL_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G2, --衛星庫房期初存貨成本,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(A.INCOST), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_IP, --進貨成本,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',A.OUT_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G1, --中央庫房內湖消耗成本,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',0,(A.OUT_QTY - NVL(A.OUT_QTYA, 0) ) * A.AVG_PRICE)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G2, --衛星庫房內湖消耗成本,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_P, --中央庫房盤盈,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_N, --中央庫房盤虧,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',0,(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_P, --衛星庫房盤盈,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',0,(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_N, --衛星庫房盤虧,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(NVL(A.OUT_QTYA, 0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PA, --台北門診應收帳款,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',A.INV_QTY,0) * A.MIL_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G1, --中央庫房期末庫存成本
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',0,A.INV_QTY) * A.MIL_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G2, --衛星庫房期末庫存成本
                                   TRIM(TO_CHAR(CAST('0' AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PC --寄售衛材消耗成本
                               FROM
                                   V_COST_FA0008 A
                               WHERE
                                   1 = 1                            
                                   AND A.DATA_YM=:DATA_YM AND A.M_STOREID='1' AND A.MAT_CLASS='02'
                                   AND A.WH_NO = 'MM1X'

                            union 

                            SELECT 2 AS ROWORDER,
                                '　戰備-非庫備品' AS MAT, --""類別"",
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',A.P_INV_QTY,0)* A.PMN_MIL_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G1 , --中央庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',0,A.P_INV_QTY)* A.PMN_MIL_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G2, --衛星庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(A.INCOST), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_IP, --進貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',A.OUT_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G1, --中央庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',0,(A.OUT_QTY - NVL(A.OUT_QTYA, 0) ) * A.AVG_PRICE)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G2, --衛星庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_P, --中央庫房盤盈,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_N, --中央庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',0,(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_P, --衛星庫房盤盈,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',0,(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_N, --衛星庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(NVL(A.OUT_QTYA, 0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PA, --台北門診應收帳款,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',A.INV_QTY,0) * A.MIL_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G1, --中央庫房期末庫存成本
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'5',0,A.INV_QTY) * A.MIL_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G2, --衛星庫房期末庫存成本
                                TRIM(TO_CHAR(CAST('0' AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PC --寄售衛材消耗成本
                            FROM
                                V_COST_FA0008 A
                            WHERE
                                1 = 1                            
                                AND A.DATA_YM=:DATA_YM AND A.M_STOREID='0' AND A.MAT_CLASS='02'
                                AND A.WH_NO = 'MM1X'
                            
                            	union
                            
                            SELECT 3 AS ROWORDER,
                                '　民品-庫備品' AS MAT, --""類別"",
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.P_INV_QTY,0)* A.PMN_AVGPRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G1, --中央庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.P_INV_QTY)* A.PMN_AVGPRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G2, --衛星庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(A.INCOST), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_IP, --進貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.OUT_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G1, --中央庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(A.OUT_QTY - NVL(A.OUT_QTYA, 0)) * A.AVG_PRICE)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G2, --衛星庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_P, --中央庫房盤盈,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_N, --中央庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_P, --衛星庫房盤盈,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_N, --衛星庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(NVL(A.OUT_QTYA, 0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PA, --台北門診應收帳款,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.INV_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G1, --中央庫房期末庫存成本
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.INV_QTY) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G2, --衛星庫房期末庫存成本
                                TRIM(TO_CHAR(CAST(ROUND(MMCODE_COST_C(:DATA_YM, '1'), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PC --寄售衛材消耗成本
                            FROM
                                V_COST_FA0008 A
                            WHERE
                                1 = 1                            
                                AND A.DATA_YM=:DATA_YM AND A.M_STOREID='1' AND A.MAT_CLASS='02'
                                AND A.WH_NO <> 'MM1X'
                            
                            	UNION
                            
                            SELECT 4 AS ROWORDER,
                                '　民品-非庫備品' AS MAT, --""類別"",
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.P_INV_QTY,0)* A.PMN_AVGPRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G1, --中央庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.P_INV_QTY)* A.PMN_AVGPRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G2, --衛星庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(A.INCOST), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_IP, --進貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.OUT_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G1, --中央庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(A.OUT_QTY - NVL(A.OUT_QTYA, 0) ) * A.AVG_PRICE)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G2, --衛星庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_P, --中央庫房盤盈,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_N, --中央庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_P, --衛星庫房盤盈,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_N, --衛星庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(NVL(A.OUT_QTYA, 0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PA, --台北門診應收帳款,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.INV_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G1, --中央庫房期末庫存成本
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.INV_QTY) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G2, --衛星庫房期末庫存成本
                                TRIM(TO_CHAR(CAST(ROUND(MMCODE_COST_C(:DATA_YM, '0'), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PC --寄售衛材消耗成本
                            FROM
                                V_COST_FA0008 A
                            WHERE
                                1 = 1                            
                                AND A.DATA_YM=:DATA_YM AND A.M_STOREID='0' AND A.MAT_CLASS='02'
                                AND A.WH_NO <> 'MM1X'
                            
                            	union
                            
                            SELECT 5 AS ROWORDER,
                                   '小計' AS MAT, --""類別"",
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'2',0,A.P_INV_QTY)* DECODE(A.WH_GRADE,'5',A.PMN_MIL_PRICE,A.PMN_AVGPRICE)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G1, --中央庫房期初存貨成本,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'2',A.P_INV_QTY,0)* DECODE(A.WH_GRADE,'5',A.PMN_MIL_PRICE,A.PMN_AVGPRICE)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G2, --衛星庫房期初存貨成本,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(A.INCOST), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_IP, --進貨成本,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'2',0,A.OUT_QTY) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G1, --中央庫房內湖消耗成本,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'2',(A.OUT_QTY - NVL(A.OUT_QTYA, 0) ),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G2, --衛星庫房內湖消耗成本,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'2',0,(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_P, --中央庫房盤盈,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'2',0,(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_N, --中央庫房盤虧,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'2',(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_P, --衛星庫房盤盈,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'2',(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_N, --衛星庫房盤虧,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(NVL(A.OUT_QTYA, 0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PA, --台北門診應收帳款,
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'2',0,A.INV_QTY) * DECODE(A.WH_GRADE,'5',A.MIL_PRICE,A.AVG_PRICE)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G1, --中央庫房期末庫存成本
                                   TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'2',A.INV_QTY,0) * DECODE(A.WH_GRADE,'5',A.MIL_PRICE,A.AVG_PRICE)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G2, --衛星庫房期末庫存成本
                                   TRIM(TO_CHAR(CAST(ROUND(MMCODE_COST_C(:DATA_YM, ''), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PC --寄售衛材消耗成本
                               FROM
                                   V_COST_FA0008 A
                               WHERE
                                   1 = 1                            
                                   AND A.DATA_YM=:DATA_YM AND A.MAT_CLASS='02'
                            
                            union
                            
                            SELECT
                                6 AS ROWORDER,
                                '氣體' AS MAT, --""類別"",
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.P_INV_QTY,0)* A.PMN_AVGPRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G1, --中央庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.P_INV_QTY)* A.PMN_AVGPRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G2, --衛星庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(A.INCOST), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_IP, --進貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.OUT_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G1, --中央庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(A.OUT_QTY - NVL(A.OUT_QTYA, 0)) * A.AVG_PRICE)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G2, --衛星庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_P, --中央庫房盤盈,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_N, --中央庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_P, --衛星庫房盤盈,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_N, --衛星庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(NVL(A.OUT_QTYA, 0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PA, --台北門診應收帳款,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.INV_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G1, --中央庫房期末庫存成本
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.INV_QTY) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G2, --衛星庫房期末庫存成本
                                TRIM(TO_CHAR(CAST('0' AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PC --寄售衛材消耗成本
                            FROM
                                V_COST_FA0008 A
                            WHERE
                                1 = 1                       
                                AND A.DATA_YM = :DATA_YM AND A.MAT_CLASS = '09'
                                AND A.WH_NO <> 'MM1X'
                            
                            union
                            
                            SELECT
                                7 AS ROWORDER,
                                '文具' AS MAT, --""類別"",
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.P_INV_QTY,0)* A.PMN_AVGPRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G1, --中央庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.P_INV_QTY)* A.PMN_AVGPRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G2, --衛星庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(A.INCOST), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_IP, --進貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.OUT_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G1, --中央庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(A.OUT_QTY - NVL(A.OUT_QTYA, 0)) * A.AVG_PRICE)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G2, --衛星庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_P, --中央庫房盤盈,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_N, --中央庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_P, --衛星庫房盤盈,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_N, --衛星庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(NVL(A.OUT_QTYA, 0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PA, --台北門診應收帳款,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.INV_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G1, --中央庫房期末庫存成本
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.INV_QTY) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G2, --衛星庫房期末庫存成本
                                TRIM(TO_CHAR(CAST('0' AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PC --寄售衛材消耗成本
                            FROM
                                V_COST_FA0008 A
                            WHERE
                                1 = 1                       
                                AND A.DATA_YM = :DATA_YM AND A.MAT_CLASS = '03'
                                AND A.WH_NO <> 'MM1X'
                            
                            union
                            
                            SELECT
                                8 AS ROWORDER,
                            	'清潔用品' AS MAT, --""類別"",
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.P_INV_QTY,0)* A.PMN_AVGPRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G1, --中央庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.P_INV_QTY)* A.PMN_AVGPRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G2, --衛星庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(A.INCOST), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_IP, --進貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.OUT_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G1, --中央庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(A.OUT_QTY - NVL(A.OUT_QTYA, 0) ) * A.AVG_PRICE)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G2, --衛星庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_P, --中央庫房盤盈,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_N, --中央庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_P, --衛星庫房盤盈,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_N, --衛星庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(NVL(A.OUT_QTYA, 0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PA, --台北門診應收帳款,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.INV_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G1, --中央庫房期末庫存成本
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.INV_QTY) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G2, --衛星庫房期末庫存成本
                                TRIM(TO_CHAR(CAST('0' AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PC --寄售衛材消耗成本
                            FROM
                                V_COST_FA0008 A
                            WHERE
                                1 = 1                       
                                AND A.DATA_YM = :DATA_YM AND A.MAT_CLASS = '04'
                                AND A.WH_NO <> 'MM1X'
                            
                            union
                            
                            SELECT
                                9 AS ROWORDER,
                            	'醫療表格' AS MAT, --""類別"",,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.P_INV_QTY,0)* A.PMN_AVGPRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G1, --中央庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.P_INV_QTY)* A.PMN_AVGPRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G2, --衛星庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(A.INCOST), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_IP, --進貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.OUT_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G1, --中央庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(A.OUT_QTY - NVL(A.OUT_QTYA, 0)) * A.AVG_PRICE)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G2, --衛星庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_P, --中央庫房盤盈,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_N, --中央庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_P, --衛星庫房盤盈,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_N, --衛星庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(NVL(A.OUT_QTYA, 0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PA, --台北門診應收帳款,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.INV_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G1, --中央庫房期末庫存成本
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.INV_QTY) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G2, --衛星庫房期末庫存成本
                                TRIM(TO_CHAR(CAST('0' AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PC --寄售衛材消耗成本
                            FROM
                                V_COST_FA0008 A
                            WHERE
                                1 = 1                       
                                AND A.DATA_YM = :DATA_YM AND A.MAT_CLASS = '05'
                                AND A.WH_NO <> 'MM1X'
                            
                            union
                            
                            SELECT
                                10 AS ROWORDER,
                            	'防護用具' AS MAT, --""類別"",,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.P_INV_QTY,0)* A.PMN_AVGPRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G1, --中央庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.P_INV_QTY)* A.PMN_AVGPRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G2, --衛星庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(A.INCOST), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_IP, --進貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.OUT_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G1, --中央庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(A.OUT_QTY - NVL(A.OUT_QTYA, 0) ) * A.AVG_PRICE)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G2, --衛星庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_P, --中央庫房盤盈,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_N, --中央庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_P, --衛星庫房盤盈,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_N, --衛星庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(NVL(A.OUT_QTYA, 0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PA, --台北門診應收帳款,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.INV_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G1, --中央庫房期末庫存成本
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.INV_QTY) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G2, --衛星庫房期末庫存成本
                                TRIM(TO_CHAR(CAST('0' AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PC --寄售衛材消耗成本
                            FROM
                                V_COST_FA0008 A
                            WHERE
                                1 = 1                       
                                AND A.DATA_YM = :DATA_YM AND A.MAT_CLASS = '06'
                                AND A.WH_NO <> 'MM1X'
                            
                            union
                            
                            SELECT
                                11 AS ROWORDER,
                            	'資訊耗材' AS MAT, --""類別"",,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.P_INV_QTY,0)* A.PMN_AVGPRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G1, --中央庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.P_INV_QTY)* A.PMN_AVGPRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G2, --衛星庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(A.INCOST), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_IP, --進貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.OUT_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G1, --中央庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(A.OUT_QTY - NVL(A.OUT_QTYA, 0) ) * A.AVG_PRICE)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G2, --衛星庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_P, --中央庫房盤盈,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_N, --中央庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_P, --衛星庫房盤盈,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_N, --衛星庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(NVL(A.OUT_QTYA, 0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PA, --台北門診應收帳款,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.INV_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G1, --中央庫房期末庫存成本
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.INV_QTY) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G2, --衛星庫房期末庫存成本
                                TRIM(TO_CHAR(CAST('0' AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PC --寄售衛材消耗成本
                            FROM
                                V_COST_FA0008 A
                            WHERE
                                1 = 1                       
                                AND A.DATA_YM = :DATA_YM AND A.MAT_CLASS = '08'
                                AND A.WH_NO <> 'MM1X'
                            
                            union
                            
                            SELECT
                                12 AS ROWORDER,
                            	'一般物品小計' AS MAT, --""類別"",
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.P_INV_QTY,0)* A.PMN_AVGPRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G1, --中央庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.P_INV_QTY)* A.PMN_AVGPRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G2, --衛星庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(A.INCOST), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_IP, --進貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.OUT_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G1, --中央庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(A.OUT_QTY - NVL(A.OUT_QTYA, 0) ) * A.AVG_PRICE)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G2, --衛星庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_P, --中央庫房盤盈,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_N, --中央庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_P, --衛星庫房盤盈,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_N, --衛星庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(NVL(A.OUT_QTYA, 0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PA, --台北門診應收帳款,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.INV_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G1, --中央庫房期末庫存成本
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.INV_QTY) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G2, --衛星庫房期末庫存成本
                                TRIM(TO_CHAR(CAST('0' AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PC --寄售衛材消耗成本
                            FROM
                                V_COST_FA0008 A
                            WHERE
                                1 = 1                       
                                AND A.DATA_YM = :DATA_YM AND A.MAT_CLASS IN ('03', '04', '05', '06', '08')
                                AND A.WH_NO <> 'MM1X'
                            
                            union
                            
                            SELECT
                                13 AS ROWORDER,
                            	'被服用品' AS MAT, --""類別"",
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.P_INV_QTY,0)* A.PMN_AVGPRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G1, --中央庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.P_INV_QTY)* A.PMN_AVGPRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PIQ_PA_G2, --衛星庫房期初存貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(A.INCOST), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_IP, --進貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.OUT_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G1, --中央庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(A.OUT_QTY - NVL(A.OUT_QTYA, 0) ) * A.AVG_PRICE)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OQ_A_AP_G2, --衛星庫房內湖消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_P, --中央庫房盤盈,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END),0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G1_N, --中央庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(CASE WHEN (A.INVENTORYQTY > 0) THEN A.INVENTORYQTY ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_P, --衛星庫房盤盈,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,(CASE WHEN (A.INVENTORYQTY < 0) THEN (A.INVENTORYQTY * -1) ELSE 0 END)) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS I_AP_G2_N, --衛星庫房盤虧,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(NVL(A.OUT_QTYA, 0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PA, --台北門診應收帳款,
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',A.INV_QTY,0) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G1, --中央庫房期末庫存成本
                                TRIM(TO_CHAR(CAST(ROUND(SUM(DECODE(A.WH_GRADE,'1',0,A.INV_QTY) * A.AVG_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IQ_PA_G2, --衛星庫房期末庫存成本
                                TRIM(TO_CHAR(CAST('0' AS NUMERIC(38, 2)),'99999999999999999990.99')) AS A_PC --寄售衛材消耗成本
                            FROM
                                V_COST_FA0008 A
                            WHERE
                                1 = 1                       
                                AND A.DATA_YM = :DATA_YM AND A.MAT_CLASS = '07'
                                AND A.WH_NO <> 'MM1X'
                            ";

            p.Add(":DATA_YM", string.Format("{0}", DATA_YM));

            sql += @" ORDER BY ROWORDER ";

            return DBWork.Connection.Query<FA0008_MODEL>(sql, p, DBWork.Transaction);
        }

        public string getDeptName()
        {
            string sql = @" SELECT  INID_NAME AS USER_DEPTNAME
                            FROM    UR_INID
                            WHERE   INID = (select INID from UR_ID where TUSER = (:userID)) ";

            var str = DBWork.Connection.ExecuteScalar(sql, new { userID = DBWork.ProcUser }, DBWork.Transaction);
            return str == null ? "" : str.ToString();
        }
        public string getUserName()
        {
            string sql = @" SELECT UNA FROM UR_ID WHERE  TUSER = (:userID) ";

            var str = DBWork.Connection.ExecuteScalar(sql, new { userID = DBWork.ProcUser }, DBWork.Transaction);
            return str == null ? "" : str.ToString();
        }

        public IEnumerable<ComboItemModel> GetYMCombo()
        {
            var p = new DynamicParameters();

            string sql = @" SELECT  SET_YM VALUE, SET_YM TEXT
                            FROM    MI_MNSET
                            WHERE SET_STATUS = 'C'
                            ORDER BY SET_YM DESC ";

            return DBWork.Connection.Query<ComboItemModel>(sql, p, DBWork.Transaction);
        }

    }
}