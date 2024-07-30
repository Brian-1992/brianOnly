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
    public class FA0006Repository : JCLib.Mvc.BaseRepository
    {
        public FA0006Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<FA0006M> GetAll(string MAT_CLASS, string DATA_YM, string M_STOREID, string MIL, bool clsALL, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = @" SELECT 
                                DISTINCT A.MMCODE, --院內碼,
                                (select (SELECT MAT_CLASS || ' ' || MAT_CLSNAME FROM MI_MATCLASS WHERE MAT_CLASS = MI_MAST.MAT_CLASS) from MI_MAST where MMCODE = C.MMCODE) AS MAT_CLASS, --物料分類,
                                C.MMNAME_C, --中文品名,
                                C.MMNAME_E, --英文品名,
                                C.BASE_UNIT, --計量單位,
                                TRIM(TO_CHAR(CAST(ROUND(A.PMN_AVGPRICEA, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PMN_AVGPRICEA, --上期結存單價(軍),
                                TRIM(TO_CHAR(CAST(ROUND(A.PMN_AVGPRICEB, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PMN_AVGPRICEB, --上期結存單價(民),
                                TRIM(TO_CHAR(CAST(ROUND(A.PINV_QTYA, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PINV_QTYA, --期初戰備存量,
                                TRIM(TO_CHAR(CAST(ROUND(A.PINV_QTYB, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PINV_QTYB, --期初民品存量,
                                TRIM(TO_CHAR(CAST(ROUND((A.PINV_QTYA + A.PINV_QTYB), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PINVQTY, --期初合計存量,
                                TRIM(TO_CHAR(CAST(ROUND((A.PINV_QTYA * A.PMN_AVGPRICEA), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS SUM1A, --期初戰備成本,
                                TRIM(TO_CHAR(CAST(ROUND((A.PINV_QTYB * A.PMN_AVGPRICEB), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS SUM1B, --期初民品成本,
                                TRIM(TO_CHAR(CAST(ROUND(((A.PINV_QTYA * A.PMN_AVGPRICEA) + (A.PINV_QTYB * A.PMN_AVGPRICEB)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS SUM1, --期初成本,
                                TRIM(TO_CHAR(CAST(ROUND(B.IN_PRICE, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IN_PRICE, --進貨單價,
                                TRIM(TO_CHAR(CAST(ROUND(A.IN_QTYA, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IN_QTYA, --戰備進貨量,
                                TRIM(TO_CHAR(CAST(ROUND(A.IN_QTYB, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IN_QTYB, --民品進貨量,
                                TRIM(TO_CHAR(CAST(ROUND((A.IN_QTYA + A.IN_QTYB), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS INQTY, --合計進貨量,
                                TRIM(TO_CHAR(CAST(ROUND((A.IN_QTYA * B.IN_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS SUM2A, --戰備進貨成本,
                                TRIM(TO_CHAR(CAST(ROUND((A.IN_QTYB * B.IN_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS SUM2B, --民品進貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(((A.IN_QTYA * B.IN_PRICE) + (A.IN_QTYB * B.IN_PRICE)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS SUM2, --進貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(A.AVG_PRICEA, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS AVG_PRICEA, --消耗單價(軍),期末單價(軍),
                                TRIM(TO_CHAR(CAST(ROUND(A.AVG_PRICEB, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS AVG_PRICEB, --消耗單價(民),期末單價(民),
                                TRIM(TO_CHAR(CAST(ROUND(A.OUT_QTYA, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OUT_QTYA, --戰備消耗量,
                                TRIM(TO_CHAR(CAST(ROUND(A.OUT_QTYB, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OUT_QTYB, --民品消耗量,
                                TRIM(TO_CHAR(CAST(ROUND((A.OUT_QTYA + A.OUT_QTYB), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OUTQTY, --合計消耗量,
                                TRIM(TO_CHAR(CAST(ROUND((A.OUT_QTYA * A.AVG_PRICEA), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS SUM3A, --戰備消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND((A.OUT_QTYB * A.AVG_PRICEB), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS SUM3B, --民品消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(((A.OUT_QTYA * A.AVG_PRICEA) + (A.OUT_QTYB * A.AVG_PRICEB)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS SUM3, --消耗成本,
                                --TRIM(TO_CHAR(CAST(ROUND(A.AVG_PRICEA, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS AVG_PRICEA, --期末單價(軍),
                                --TRIM(TO_CHAR(CAST(ROUND(A.AVG_PRICEB, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS AVG_PRICEB, --期末單價(民),
                                TRIM(TO_CHAR(CAST(ROUND(A.INV_QTYA, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS INV_QTYA, --期末戰備存量,
                                TRIM(TO_CHAR(CAST(ROUND(A.INV_QTYB, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS INV_QTYB, --期末民品存量,
                                TRIM(TO_CHAR(CAST(ROUND((A.INV_QTYA + A.INV_QTYB), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS INVQTY, --期末合計存量,
                                TRIM(TO_CHAR(CAST(ROUND((A.INV_QTYA * A.AVG_PRICEA), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS SUM4A, --期末戰備成本,
                                TRIM(TO_CHAR(CAST(ROUND((A.INV_QTYB * A.AVG_PRICEB), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS SUM4B, --期末民品成本,
                                TRIM(TO_CHAR(CAST(ROUND(((A.INV_QTYA * A.AVG_PRICEA) + (A.INV_QTYB * A.AVG_PRICEB)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS SUM4 --期末成本
                            FROM
                                (
                                    select 
                                        MMCODE,                                        
                                        SUM((CASE WH_NO WHEN WHNO_MM5 THEN P_MIL_PRICE ELSE 0 END)) AS PMN_AVGPRICEA, --上期結存單價(軍),
                                        SUM((CASE WH_NO WHEN WHNO_MM5 THEN 0 ELSE PMN_AVGPRICE END)) AS PMN_AVGPRICEB, --上期結存單價(民),
                                        SUM((CASE WH_NO WHEN WHNO_MM5 THEN P_INV_QTY ELSE 0 END)) AS PINV_QTYA, --期初戰備存量,
                                        SUM((CASE WH_NO WHEN WHNO_MM5 THEN 0 ELSE P_INV_QTY END)) AS PINV_QTYB, --期初民品存量,
                                        SUM((CASE WH_NO WHEN WHNO_MM5 THEN IN_QTY ELSE 0 END)) AS IN_QTYA, --戰備進貨量,
                                        SUM((CASE WH_NO WHEN WHNO_MM5 THEN 0 ELSE IN_QTY END)) AS IN_QTYB, --民品進貨量,
                                        SUM((CASE WH_NO WHEN WHNO_MM5 THEN MIL_PRICE ELSE 0 END)) AS AVG_PRICEA, --期末單價(軍),
                                        SUM((CASE WH_NO WHEN WHNO_MM5 THEN 0 ELSE AVG_PRICE END)) AS AVG_PRICEB, --期末單價(民),
                                        SUM((CASE WH_NO WHEN WHNO_MM5 THEN OUT_QTY ELSE 0 END)) AS OUT_QTYA, --戰備消耗量,
                                        SUM((CASE WH_NO WHEN WHNO_MM5 THEN 0 ELSE OUT_QTY END)) AS OUT_QTYB, --民品消耗量,
                                        SUM((CASE WH_NO WHEN WHNO_MM5 THEN INV_QTY ELSE 0 END)) AS INV_QTYA, --期末戰備存量,
                                        SUM((CASE WH_NO WHEN WHNO_MM5 THEN 0 ELSE INV_QTY END)) AS INV_QTYB --期末民品存量,
                                    from
                                        V_COST_CURM15V
                                    where
                                        (WH_NO = WHNO_MM5 or WH_NO = WHNO_MM1)
                                    group by
                                        MMCODE
                                ) A,
                                V_COST_CURM15V B,
                                MI_MAST C
                            WHERE
                                1 = 1 
                                AND A.MMCODE = B.MMCODE
                                AND A.MMCODE = C.MMCODE
                            ";

            if (!string.IsNullOrWhiteSpace(MAT_CLASS))
            {
                if (clsALL == true)
                {
                    sql += @" AND A.MMCODE IN (select MMCODE from MI_MAST where MAT_CLASS in (" + MAT_CLASS + ")) ";
                }
                else
                {
                    sql += @" AND A.MMCODE IN (select MMCODE from MI_MAST where MAT_CLASS = :MAT_CLASS ) ";
                    p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS));
                }
            }

            if (!string.IsNullOrWhiteSpace(DATA_YM))
            {
                sql += " AND TO_DATE(B.DATA_YM + 191100, 'yyyy/MM') = TO_DATE(:DATA_YM + 191100, 'yyyy/MM') ";
                p.Add(":DATA_YM", string.Format("{0}", DATA_YM));
            }

            if (!string.IsNullOrWhiteSpace(M_STOREID))
            {
                switch (M_STOREID)
                {
                    case "0":   //radio選庫備
                        sql += @" AND A.MMCODE IN (select MMCODE from MI_MAST where M_STOREID = '1' and M_APPLYID not in ('P', 'E')) ";
                        break;
                    case "1":   //radio選非庫備
                        sql += @" AND A.MMCODE IN (select MMCODE from MI_MAST where M_STOREID = '0' )";
                        break;
                    case "2":   //radio選庫備品(管控項目)
                        sql += @" AND A.MMCODE IN (select MMCODE from MI_MAST where M_STOREID = '1' and M_APPLYID in ('P', 'E')) ";
                        break;
                    default:
                        break;
                }
                p.Add(":M_STOREID", string.Format("{0}", M_STOREID));
            }

            if (!string.IsNullOrWhiteSpace(MIL))
            {
                switch (MIL)
                {
                    case "0":   //全部
                        sql += @" AND (B.WH_NO = WHNO_MM1 OR B.WH_NO = WHNO_MM5) ";
                        break;
                    case "1":   //軍用
                        sql += @" AND B.WH_NO = WHNO_MM5";
                        break;
                    case "2":   //民用
                        sql += @" AND B.WH_NO = WHNO_MM1 ";
                        break;
                    default:
                        break;
                }
                p.Add(":MIL", string.Format("{0}", MIL));
            }

            sql += "ORDER BY A.MMCODE ";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<FA0006M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<FA0006M> Print(string MAT_CLASS, string DATA_YM, string M_STOREID, string MIL, bool clsALL)
        {
            var p = new DynamicParameters();

            string sql = @" SELECT
                                DISTINCT A.MMCODE, --院內碼,
                                (select (SELECT MAT_CLASS || ' ' || MAT_CLSNAME FROM MI_MATCLASS WHERE MAT_CLASS = MI_MAST.MAT_CLASS) from MI_MAST where MMCODE = C.MMCODE) AS MAT_CLASS, --物料分類,
                                C.MMNAME_C, --中文品名,
                                C.MMNAME_E, --英文品名,
                                C.BASE_UNIT, --計量單位,
                                TRIM(TO_CHAR(CAST(ROUND(A.PMN_AVGPRICEA, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PMN_AVGPRICEA, --上期結存單價(軍),
                                TRIM(TO_CHAR(CAST(ROUND(A.PMN_AVGPRICEB, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PMN_AVGPRICEB, --上期結存單價(民),
                                TRIM(TO_CHAR(CAST(ROUND(A.PINV_QTYA, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PINV_QTYA, --期初戰備存量,
                                TRIM(TO_CHAR(CAST(ROUND(A.PINV_QTYB, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PINV_QTYB, --期初民品存量,
                                TRIM(TO_CHAR(CAST(ROUND((A.PINV_QTYA + A.PINV_QTYB), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS PINVQTY, --期初合計存量,
                                TRIM(TO_CHAR(CAST(ROUND((A.PINV_QTYA * A.PMN_AVGPRICEA), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS SUM1A, --期初戰備成本,
                                TRIM(TO_CHAR(CAST(ROUND((A.PINV_QTYB * A.PMN_AVGPRICEB), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS SUM1B, --期初民品成本,
                                TRIM(TO_CHAR(CAST(ROUND(((A.PINV_QTYA * A.PMN_AVGPRICEA) + (A.PINV_QTYB * A.PMN_AVGPRICEB)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS SUM1, --期初成本,
                                TRIM(TO_CHAR(CAST(ROUND(B.IN_PRICE, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IN_PRICE, --進貨單價,
                                TRIM(TO_CHAR(CAST(ROUND(A.IN_QTYA, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IN_QTYA, --戰備進貨量,
                                TRIM(TO_CHAR(CAST(ROUND(A.IN_QTYB, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS IN_QTYB, --民品進貨量,
                                TRIM(TO_CHAR(CAST(ROUND((A.IN_QTYA + A.IN_QTYB), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS INQTY, --合計進貨量,
                                TRIM(TO_CHAR(CAST(ROUND((A.IN_QTYA * B.IN_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS SUM2A, --戰備進貨成本,
                                TRIM(TO_CHAR(CAST(ROUND((A.IN_QTYB * B.IN_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS SUM2B, --民品進貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(((A.IN_QTYA * B.IN_PRICE) + (A.IN_QTYB * B.IN_PRICE)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS SUM2, --進貨成本,
                                TRIM(TO_CHAR(CAST(ROUND(A.AVG_PRICEA, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS AVG_PRICEA, --消耗單價(軍),期末單價(軍),
                                TRIM(TO_CHAR(CAST(ROUND(A.AVG_PRICEB, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS AVG_PRICEB, --消耗單價(民),期末單價(民),
                                TRIM(TO_CHAR(CAST(ROUND(A.OUT_QTYA, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OUT_QTYA, --戰備消耗量,
                                TRIM(TO_CHAR(CAST(ROUND(A.OUT_QTYB, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OUT_QTYB, --民品消耗量,
                                TRIM(TO_CHAR(CAST(ROUND((A.OUT_QTYA + A.OUT_QTYB), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS OUTQTY, --合計消耗量,
                                TRIM(TO_CHAR(CAST(ROUND((A.OUT_QTYA * A.AVG_PRICEA), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS SUM3A, --戰備消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND((A.OUT_QTYB * A.AVG_PRICEB), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS SUM3B, --民品消耗成本,
                                TRIM(TO_CHAR(CAST(ROUND(((A.OUT_QTYA * A.AVG_PRICEA) + (A.OUT_QTYB * A.AVG_PRICEB)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS SUM3, --消耗成本,
                                --TRIM(TO_CHAR(CAST(ROUND(A.AVG_PRICEA, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS AVG_PRICEA, --期末單價(軍),
                                --TRIM(TO_CHAR(CAST(ROUND(A.AVG_PRICEB, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS AVG_PRICEB, --期末單價(民),
                                TRIM(TO_CHAR(CAST(ROUND(A.INV_QTYA, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS INV_QTYA, --期末戰備存量,
                                TRIM(TO_CHAR(CAST(ROUND(A.INV_QTYB, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS INV_QTYB, --期末民品存量,
                                TRIM(TO_CHAR(CAST(ROUND((A.INV_QTYA + A.INV_QTYB), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS INVQTY, --期末合計存量,
                                TRIM(TO_CHAR(CAST(ROUND((A.INV_QTYA * A.AVG_PRICEA), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS SUM4A, --期末戰備成本,
                                TRIM(TO_CHAR(CAST(ROUND((A.INV_QTYB * A.AVG_PRICEB), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS SUM4B, --期末民品成本,
                                TRIM(TO_CHAR(CAST(ROUND(((A.INV_QTYA * A.AVG_PRICEA) + (A.INV_QTYB * A.AVG_PRICEB)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS SUM4 --期末成本
                            FROM
                                (
                                    select 
                                        MMCODE,                                        
                                        SUM((CASE WH_NO WHEN 'MM1X' THEN P_MIL_PRICE ELSE 0 END)) AS PMN_AVGPRICEA, --上期結存單價(軍),
                                        SUM((CASE WH_NO WHEN 'MM1X' THEN 0 ELSE PMN_AVGPRICE END)) AS PMN_AVGPRICEB, --上期結存單價(民),
                                        SUM((CASE WH_NO WHEN 'MM1X' THEN P_INV_QTY ELSE 0 END)) AS PINV_QTYA, --期初戰備存量,
                                        SUM((CASE WH_NO WHEN 'MM1X' THEN 0 ELSE P_INV_QTY END)) AS PINV_QTYB, --期初民品存量,
                                        SUM((CASE WH_NO WHEN 'MM1X' THEN IN_QTY ELSE 0 END)) AS IN_QTYA, --戰備進貨量,
                                        SUM((CASE WH_NO WHEN 'MM1X' THEN 0 ELSE IN_QTY END)) AS IN_QTYB, --民品進貨量,
                                        SUM((CASE WH_NO WHEN 'MM1X' THEN MIL_PRICE ELSE 0 END)) AS AVG_PRICEA, --期末單價(軍),
                                        SUM((CASE WH_NO WHEN 'MM1X' THEN 0 ELSE AVG_PRICE END)) AS AVG_PRICEB, --期末單價(民),
                                        SUM((CASE WH_NO WHEN 'MM1X' THEN OUT_QTY ELSE 0 END)) AS OUT_QTYA, --戰備消耗量,
                                        SUM((CASE WH_NO WHEN 'MM1X' THEN 0 ELSE OUT_QTY END)) AS OUT_QTYB, --民品消耗量,
                                        SUM((CASE WH_NO WHEN 'MM1X' THEN INV_QTY ELSE 0 END)) AS INV_QTYA, --期末戰備存量,
                                        SUM((CASE WH_NO WHEN 'MM1X' THEN 0 ELSE INV_QTY END)) AS INV_QTYB --期末民品存量,
                                    from
                                        V_COST_CURM15V
                                    where
                                        (WH_NO = WHNO_MM5 or WH_NO = WHNO_MM1)
                                    group by
                                        MMCODE
                                ) A,
                                V_COST_CURM15V B,
                                MI_MAST C
                            WHERE
                                1 = 1 
                                AND A.MMCODE = B.MMCODE
                                AND A.MMCODE = C.MMCODE
                            ";

            if (!string.IsNullOrWhiteSpace(MAT_CLASS))
            {
                if (clsALL == true)
                {
                    sql += @" AND A.MMCODE IN (select MMCODE from MI_MAST where MAT_CLASS in (" + MAT_CLASS + ")) ";
                }
                else
                {
                    sql += @" AND A.MMCODE IN (select MMCODE from MI_MAST where MAT_CLASS = :MAT_CLASS ) ";
                    p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS));
                }
            }

            if (!string.IsNullOrWhiteSpace(DATA_YM))
            {
                sql += " AND TO_DATE(B.DATA_YM + 191100, 'yyyy/MM') = TO_DATE(:DATA_YM + 191100, 'yyyy/MM') ";
                p.Add(":DATA_YM", string.Format("{0}", DATA_YM));
            }

            if (!string.IsNullOrWhiteSpace(M_STOREID))
            {
                switch (M_STOREID)
                {
                    case "0":   //radio選庫備
                        sql += @" AND A.MMCODE IN (select MMCODE from MI_MAST where M_STOREID = '1' and M_APPLYID not in ('P', 'E')) ";
                        break;
                    case "1":   //radio選非庫備
                        sql += @" AND A.MMCODE IN (select MMCODE from MI_MAST where M_STOREID = '0' )";
                        break;
                    case "2":   //radio選庫備品(管控項目)
                        sql += @" AND A.MMCODE IN (select MMCODE from MI_MAST where M_STOREID = '1' and M_APPLYID in ('P', 'E')) ";
                        break;
                    default:
                        break;
                }
                p.Add(":M_STOREID", string.Format("{0}", M_STOREID));
            }

            if (!string.IsNullOrWhiteSpace(MIL))
            {
                switch (MIL)
                {
                    case "0":   //全部
                        sql += @" AND (B.WH_NO = WHNO_MM1 OR B.WH_NO = WHNO_MM5) ";
                        break;
                    case "1":   //軍用
                        sql += @" AND B.WH_NO = WHNO_MM5 ";
                        break;
                    case "2":   //民用
                        sql += @" AND B.WH_NO = WHNO_MM1 ";
                        break;
                    default:
                        break;
                }
                p.Add(":MIL", string.Format("{0}", MIL));
            }

            sql += "ORDER BY A.MMCODE ";

            return DBWork.Connection.Query<FA0006M>(sql, p, DBWork.Transaction);
        }

        public DataTable GetExcel(string MAT_CLASS, string DATA_YM, string M_STOREID, string MIL, bool clsALL)
        {
            var p = new DynamicParameters();

            string sql = @" SELECT 
                                DISTINCT A.MMCODE AS ""院內碼"",
                                (select (SELECT MAT_CLASS || ' ' || MAT_CLSNAME FROM MI_MATCLASS WHERE MAT_CLASS = MI_MAST.MAT_CLASS) from MI_MAST where MMCODE = C.MMCODE) AS ""物料分類"",
                                C.MMNAME_C AS ""中文品名"",
                                C.MMNAME_E AS ""英文品名"",
                                C.BASE_UNIT AS ""計量單位"",
                                TRIM(TO_CHAR(CAST(ROUND(A.PMN_AVGPRICEA, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS ""上期結存單價(軍)"",
                                TRIM(TO_CHAR(CAST(ROUND(A.PMN_AVGPRICEB, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS ""上期結存單價(民)"",
                                TRIM(TO_CHAR(CAST(ROUND(A.PINV_QTYA, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS ""期初戰備存量"",
                                TRIM(TO_CHAR(CAST(ROUND(A.PINV_QTYB, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS ""期初民品存量"",
                                TRIM(TO_CHAR(CAST(ROUND((A.PINV_QTYA + A.PINV_QTYB), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS ""期初合計存量"",
                                TRIM(TO_CHAR(CAST(ROUND((A.PINV_QTYA * A.PMN_AVGPRICEA), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS ""期初戰備成本"",
                                TRIM(TO_CHAR(CAST(ROUND((A.PINV_QTYB * A.PMN_AVGPRICEB), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS ""期初民品成本"",
                                TRIM(TO_CHAR(CAST(ROUND(((A.PINV_QTYA * A.PMN_AVGPRICEA) + (A.PINV_QTYB * A.PMN_AVGPRICEB)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS ""期初成本"",
                                TRIM(TO_CHAR(CAST(ROUND(B.IN_PRICE, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS ""進貨單價"",
                                TRIM(TO_CHAR(CAST(ROUND(A.IN_QTYA, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS ""戰備進貨量"",
                                TRIM(TO_CHAR(CAST(ROUND(A.IN_QTYB, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS ""民品進貨量"",
                                TRIM(TO_CHAR(CAST(ROUND((A.IN_QTYA + A.IN_QTYB), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS ""合計進貨量"",
                                TRIM(TO_CHAR(CAST(ROUND((A.IN_QTYA * B.IN_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS ""戰備進貨成本"",
                                TRIM(TO_CHAR(CAST(ROUND((A.IN_QTYB * B.IN_PRICE), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS ""民品進貨成本"",
                                TRIM(TO_CHAR(CAST(ROUND(((A.IN_QTYA * B.IN_PRICE) + (A.IN_QTYB * B.IN_PRICE)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS ""進貨成本"",
                                TRIM(TO_CHAR(CAST(ROUND(A.AVG_PRICEA, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS ""消耗單價(軍)"",
                                TRIM(TO_CHAR(CAST(ROUND(A.AVG_PRICEB, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS ""消耗單價(民)"",
                                TRIM(TO_CHAR(CAST(ROUND(A.OUT_QTYA, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS ""戰備消耗量"",
                                TRIM(TO_CHAR(CAST(ROUND(A.OUT_QTYB, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS ""民品消耗量"",
                                TRIM(TO_CHAR(CAST(ROUND((A.OUT_QTYA + A.OUT_QTYB), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS ""合計消耗量"",
                                TRIM(TO_CHAR(CAST(ROUND((A.OUT_QTYA * A.AVG_PRICEA), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS ""戰備消耗成本"",
                                TRIM(TO_CHAR(CAST(ROUND((A.OUT_QTYB * A.AVG_PRICEB), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS ""民品消耗成本"",
                                TRIM(TO_CHAR(CAST(ROUND(((A.OUT_QTYA * A.AVG_PRICEA) + (A.OUT_QTYB * A.AVG_PRICEB)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS ""消耗成本"",
                                TRIM(TO_CHAR(CAST(ROUND(A.AVG_PRICEA, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS ""期末單價(軍)"",
                                TRIM(TO_CHAR(CAST(ROUND(A.AVG_PRICEB, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS ""期末單價(民)"",
                                TRIM(TO_CHAR(CAST(ROUND(A.INV_QTYA, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS ""期末戰備存量"",
                                TRIM(TO_CHAR(CAST(ROUND(A.INV_QTYB, 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS ""期末民品存量"",
                                TRIM(TO_CHAR(CAST(ROUND((A.INV_QTYA + A.INV_QTYB), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS ""期末合計存量"",
                                TRIM(TO_CHAR(CAST(ROUND((A.INV_QTYA * A.AVG_PRICEA), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS ""期末戰備成本"",
                                TRIM(TO_CHAR(CAST(ROUND((A.INV_QTYB * A.AVG_PRICEB), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS ""期末民品成本"",
                                TRIM(TO_CHAR(CAST(ROUND(((A.INV_QTYA * A.AVG_PRICEA) + (A.INV_QTYB * A.AVG_PRICEB)), 2) AS NUMERIC(38, 2)),'99999999999999999990.99')) AS ""期末成本""
                            FROM
                                (
                                    select 
                                        MMCODE,                                        
                                        SUM((CASE WH_NO WHEN 'MM1X' THEN P_MIL_PRICE ELSE 0 END)) AS PMN_AVGPRICEA, --上期結存單價(軍),
                                        SUM((CASE WH_NO WHEN 'MM1X' THEN 0 ELSE PMN_AVGPRICE END)) AS PMN_AVGPRICEB, --上期結存單價(民),
                                        SUM((CASE WH_NO WHEN 'MM1X' THEN P_INV_QTY ELSE 0 END)) AS PINV_QTYA, --期初戰備存量,
                                        SUM((CASE WH_NO WHEN 'MM1X' THEN 0 ELSE P_INV_QTY END)) AS PINV_QTYB, --期初民品存量,
                                        SUM((CASE WH_NO WHEN 'MM1X' THEN IN_QTY ELSE 0 END)) AS IN_QTYA, --戰備進貨量,
                                        SUM((CASE WH_NO WHEN 'MM1X' THEN 0 ELSE IN_QTY END)) AS IN_QTYB, --民品進貨量,
                                        SUM((CASE WH_NO WHEN 'MM1X' THEN MIL_PRICE ELSE 0 END)) AS AVG_PRICEA, --期末單價(軍),
                                        SUM((CASE WH_NO WHEN 'MM1X' THEN 0 ELSE AVG_PRICE END)) AS AVG_PRICEB, --期末單價(民),
                                        SUM((CASE WH_NO WHEN 'MM1X' THEN OUT_QTY ELSE 0 END)) AS OUT_QTYA, --戰備消耗量,
                                        SUM((CASE WH_NO WHEN 'MM1X' THEN 0 ELSE OUT_QTY END)) AS OUT_QTYB, --民品消耗量,
                                        SUM((CASE WH_NO WHEN 'MM1X' THEN INV_QTY ELSE 0 END)) AS INV_QTYA, --期末戰備存量,
                                        SUM((CASE WH_NO WHEN 'MM1X' THEN 0 ELSE INV_QTY END)) AS INV_QTYB --期末民品存量,
                                    from
                                        V_COST_CURM15V
                                    where
                                        (WH_NO = WHNO_MM5 or WH_NO = WHNO_MM1)
                                    group by
                                        MMCODE
                                ) A,
                                V_COST_CURM15V B,
                                MI_MAST C
                            WHERE
                                1 = 1 
                                AND A.MMCODE = B.MMCODE
                                AND A.MMCODE = C.MMCODE
                            ";

            if (!string.IsNullOrWhiteSpace(MAT_CLASS))
            {
                if (clsALL == true)
                {
                    sql += @" AND A.MMCODE IN (select MMCODE from MI_MAST where MAT_CLASS in (" + MAT_CLASS + ")) ";
                }
                else
                {
                    sql += @" AND A.MMCODE IN (select MMCODE from MI_MAST where MAT_CLASS = :MAT_CLASS ) ";
                    p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS));
                }
            }

            if (!string.IsNullOrWhiteSpace(DATA_YM))
            {
                sql += " AND TO_DATE(B.DATA_YM + 191100, 'yyyy/MM') = TO_DATE(:DATA_YM + 191100, 'yyyy/MM') ";
                p.Add(":DATA_YM", string.Format("{0}", DATA_YM));
            }

            if (!string.IsNullOrWhiteSpace(M_STOREID))
            {
                switch (M_STOREID)
                {
                    case "0":   //radio選庫備
                        sql += @" AND A.MMCODE IN (select MMCODE from MI_MAST where M_STOREID = '1' and M_APPLYID not in ('P', 'E')) ";
                        break;
                    case "1":   //radio選非庫備
                        sql += @" AND A.MMCODE IN (select MMCODE from MI_MAST where M_STOREID = '0' )";
                        break;
                    case "2":   //radio選庫備品(管控項目)
                        sql += @" AND A.MMCODE IN (select MMCODE from MI_MAST where M_STOREID = '1' and M_APPLYID in ('P', 'E')) ";
                        break;
                    default:
                        break;
                }
                p.Add(":M_STOREID", string.Format("{0}", M_STOREID));
            }

            if (!string.IsNullOrWhiteSpace(MIL))
            {
                switch (MIL)
                {
                    case "0":   //全部
                        sql += @" AND (B.WH_NO = WHNO_MM1 OR B.WH_NO = WHNO_MM5) ";
                        break;
                    case "1":   //軍用
                        sql += @" AND B.WH_NO = WHNO_MM5 ";
                        break;
                    case "2":   //民用
                        sql += @" AND B.WH_NO = WHNO_MM1 ";
                        break;
                    default:
                        break;
                }
                p.Add(":MIL", string.Format("{0}", MIL));
            }

            sql += "ORDER BY A.MMCODE ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<ComboItemModel> GetMatCombo(string userid)
        {
            var p = new DynamicParameters();

            string sql = @"  SELECT MAT_CLASS VALUE,
                                    MAT_CLASS || ' ' || MAT_CLSNAME TEXT
                             FROM MI_MATCLASS
                             WHERE MAT_CLSID in ('2', '3')
                             ORDER BY MAT_CLASS";

            p.Add(":p0", string.Format("{0}", userid));

            return DBWork.Connection.Query<ComboItemModel>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<ComboItemModel> GetWh_no()
        {
            var p = new DynamicParameters();

            string sql = @" SELECT 
                                WH_NO VALUE, WH_NO || ' ' || WH_NAME TEXT
                            FROM 
                                MI_WHMAST
                            WHERE 
                                WH_NO = WHNO_MM1";

            return DBWork.Connection.Query<ComboItemModel>(sql, new { inid = DBWork.UserInfo.Inid }, DBWork.Transaction);
        }

        //public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, string p1, bool clsALL, int page_index, int page_size, string sorters)
        //{
        //    var p = new DynamicParameters();

        //    var sql = @"SELECT {0} 
        //                    A.MMCODE, 
        //                    A.MMNAME_C, 
        //                    A.MMNAME_E, 
        //                    A.MAT_CLASS, 
        //                    A.BASE_UNIT 
        //                FROM 
        //                    MI_MAST A 
        //                WHERE 1=1 ";
        //    //AND (SELECT COUNT(*) FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO = '560000' ) > 0 ";

        //    if (clsALL != true)
        //    {
        //        sql += " AND A.MAT_CLASS = :MAT_CLASS ";
        //        p.Add(":MAT_CLASS", string.Format("{0}", p0));
        //    }
        //    else
        //    {
        //        sql += @" AND A.MAT_CLASS IN (" + p0 + ") ";
        //    }

        //    if (p1 != "")
        //    {
        //        sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(A.MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(A.MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
        //        p.Add(":MMCODE_I", p1);
        //        p.Add(":MMNAME_E_I", p1);
        //        p.Add(":MMNAME_C_I", p1);

        //        sql += " AND (A.MMCODE LIKE :MMCODE ";
        //        p.Add(":MMCODE", string.Format("%{0}%", p1));

        //        sql += " OR A.MMNAME_E LIKE :MMNAME_E ";
        //        p.Add(":MMNAME_E", string.Format("%{0}%", p1));

        //        sql += " OR A.MMNAME_C LIKE :MMNAME_C) ";
        //        p.Add(":MMNAME_C", string.Format("%{0}%", p1));

        //        sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX, MMCODE", sql);
        //    }
        //    else
        //    {
        //        sql = string.Format(sql, "");
        //        sql += " ORDER BY A.MMCODE ";
        //    }

        //    p.Add("OFFSET", (page_index - 1) * page_size);
        //    p.Add("PAGE_SIZE", page_size);

        //    return DBWork.Connection.Query<MI_MAST>(sql, p, DBWork.Transaction);
        //}

        public string getDeptName()
        {
            string sql = @" SELECT  INID_NAME AS USER_DEPTNAME
                            FROM    UR_INID
                            WHERE   INID = (select INID from UR_ID where TUSER = (:userID)) ";

            var str = DBWork.Connection.ExecuteScalar(sql, new { userID = DBWork.ProcUser }, DBWork.Transaction);
            return str == null ? "" : str.ToString();
        }

    }
}