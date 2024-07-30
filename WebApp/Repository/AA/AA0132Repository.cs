using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace WebApp.Repository.AA
{

    public class AA0132ReportMODEL : JCLib.Mvc.BaseModel
    {
        public string F1 { get; set; }
        public string F2 { get; set; }
        public double F3 { get; set; }
        public double F4 { get; set; }
        public double F5 { get; set; }
        public double F6 { get; set; }
        public double F7 { get; set; }
        public double F8 { get; set; }
        public double F9 { get; set; }
        public double F10 { get; set; }
        public double F11 { get; set; }
        public double F12 { get; set; }
        public double F13 { get; set; }
        public string F14 { get; set; }
        public double F15 { get; set; }
        public string F16 { get; set; }

    }
    public class AA0132Repository : JCLib.Mvc.BaseRepository
    {
        public AA0132Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }


        public IEnumerable<AA0132> GetAllM(string type, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.MMCODE,
                               A.MMNAME_E,
                               nvl((select inv_qty from MI_WINVMON
                                 where data_ym = TWN_YYYMM(ADD_MONTHS(SYSDATE, -1))
                                   and wh_no = a.wh_no 
                                   and mmcode = a.mmcode), 0) as PMN_INVQTY,
                               A.APL_INQTY,
                               A.APL_OUTQTY,
                               A.STORE_QTYC,
                               nvl(B.AVG_PRICE, 0) as avg_price,
                               NVL ( (nvl(B.AVG_PRICE, 0) * A.STORE_QTYC), 0) AS STORE_AMOUNT,
                               A.STORE_QTYM,
                               nvl(B.MIL_PRICE, 0) as mil_price,
                               A.EXG_INQTY,
                               A.EXG_OUTQTY,
                               (A.STORE_QTYC + A.STORE_QTYM) AS CADDM,
                               ' ' AS CHK_QTY,
                               UNIT_EXCHRATIO (A.MMCODE, A.BASE_UNIT, A.M_AGENNO) AS PACK_QTY
                        FROM (SELECT A.WH_NO,
                                     '1' AS CHK_TYPE,
                                     ' ' AS CHK_YM,
                                     C.MMCODE,
                                     C.MMNAME_E,
                                     C.BASE_UNIT,
                                     C.M_AGENNO,
                                     B.INV_QTY AS STORE_QTY,
                                     B.INV_QTY AS STORE_QTYC,
                                     NVL (
                                          (SELECT INV_QTY
                                           FROM MI_WHINV
                                           WHERE     WH_NO = 'PH1X'
                                           AND MMCODE = ('004' || SUBSTR (B.MMCODE, 4, 10))), 0) AS STORE_QTYM,
                                     B.EXG_INQTY,
                                     B.EXG_OUTQTY,
                                     B.APL_INQTY,
                                     B.APL_OUTQTY,
                                     C.M_CONTPRICE,
                                     C.M_STOREID,
                                     0 AS CHK_QTY
                              FROM MI_WHMAST A, MI_WHINV B, MI_MAST C
                              WHERE     A.WH_GRADE = '1'
                              AND A.WH_KIND = '0'
                              AND A.WH_NO = 'PH1S'
                              AND A.WH_NO = B.WH_NO
                              AND B.MMCODE = C.MMCODE
                              and substr(b.mmcode,1,3) > '004' 
                              and substr(b.mmcode,1,3) < '008'";

            if (type == "1")
            {
                sql += " AND C.E_TAKEKIND IN('11','12','13') and C.E_RESTRICTCODE not IN('1','2','3', '4')";
            }
            else if (type == "2")
            {
                sql += " AND C.E_TAKEKIND IN('00','21','31','41','51') and C.E_RESTRICTCODE not IN('1','2','3', '4')";
            }
            else if (type == "3")
            {
                sql += " AND C.E_RESTRICTCODE IN('1','2','3') ";
            }
            else if (type == "4")
            {
                sql += " AND C.E_RESTRICTCODE IN('4') ";
            }
            sql += @" ) A
                      left join MI_WHCOST b on (a.mmcode = b.mmcode and b.data_ym = twn_yyymm(sysdate))
                      ORDER BY A.MMCODE ";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<AA0132>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<AA0132ReportMODEL> GetPrintData(string type)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT A.MMCODE F1,
                               A.MMNAME_E F2,
                               nvl((select inv_qty from MI_WINVMON
                                     where data_ym = TWN_YYYMM(ADD_MONTHS(SYSDATE, -1))
                                       and wh_no = a.wh_no 
                                       and mmcode = a.mmcode), 0) F3,
                               A.APL_INQTY F4,
                               A.APL_OUTQTY F5,
                               A.STORE_QTYC F6,
                               nvl(B.AVG_PRICE, 0) F7,
                               NVL ( (nvl(B.AVG_PRICE, 0) * A.STORE_QTYC), 0) F8,
                               A.STORE_QTYM F9,
                               nvl(B.MIL_PRICE, 0) F10,
                               A.EXG_INQTY F11,
                               A.EXG_OUTQTY F12,
                               (A.STORE_QTYC + A.STORE_QTYM) F13,
                               ' ' F14,
                               UNIT_EXCHRATIO (A.MMCODE, A.BASE_UNIT, A.M_AGENNO) AS F15,
                               '' F16
                        FROM (SELECT A.WH_NO,
                                     '1' AS CHK_TYPE,
                                     ' ' AS CHK_YM,
                                     C.MMCODE,
                                     C.MMNAME_E,
                                     C.BASE_UNIT,
                                     C.M_AGENNO,
                                     B.INV_QTY AS STORE_QTY,
                                     B.INV_QTY AS STORE_QTYC,
                                     NVL (
                                          (SELECT INV_QTY
                                           FROM MI_WHINV
                                           WHERE     WH_NO = 'PH1X'
                                           AND MMCODE = ('004' || SUBSTR (B.MMCODE, 4, 10))), 0) STORE_QTYM,
                                     B.EXG_INQTY,
                                     B.EXG_OUTQTY,
                                     B.APL_INQTY,
                                     B.APL_OUTQTY,
                                     C.M_CONTPRICE,
                                     C.M_STOREID,
                                     0 AS CHK_QTY
                              FROM MI_WHMAST A, MI_WHINV B, MI_MAST C
                              WHERE     A.WH_GRADE = '1'
                              AND A.WH_KIND = '0'
                              AND A.WH_NO = 'PH1S'
                              AND A.WH_NO = B.WH_NO
                              AND B.MMCODE = C.MMCODE";

            if (type == "1")
            {
                sql += " AND C.E_TAKEKIND IN('11','12','13') and C.E_RESTRICTCODE not IN('1','2','3', '4')";
            }
            else if (type == "2")
            {
                sql += " AND C.E_TAKEKIND IN('00','21','31','41','51') and C.E_RESTRICTCODE not IN('1','2','3', '4')";
            }
            else if (type == "3")
            {
                sql += " AND C.E_RESTRICTCODE IN('1','2','3') ";
            }
            else if (type == "4")
            {
                sql += " AND C.E_RESTRICTCODE IN('4') ";
            }
            sql += @" ) A
                      left join MI_WHCOST b on (a.mmcode = b.mmcode and b.data_ym = twn_yyymm(sysdate))
                      ORDER BY A.MMCODE";

            return DBWork.Connection.Query<AA0132ReportMODEL>(sql, p, DBWork.Transaction);
        }

        //匯出
        public DataTable GetExcel(string type)
        {
            DynamicParameters p = new DynamicParameters();
            var sql = @" SELECT A.MMCODE 院內碼,
                                A.MMNAME_E 英文品名,
                                nvl((select inv_qty from MI_WINVMON
                                      where data_ym = TWN_YYYMM(ADD_MONTHS(SYSDATE, -1))
                                        and wh_no = a.wh_no 
                                        and mmcode = a.mmcode), 0)  上月結存,
                                A.APL_INQTY 民品本月進貨,
                                A.APL_OUTQTY 民品本月撥發,
                                A.STORE_QTYC 民品本月結存,
                                nvl(B.AVG_PRICE, 0) 民品單價,
                                NVL ( (nvl(B.AVG_PRICE, 0) * A.STORE_QTY), 0) 民品結存金額,
                                A.STORE_QTYM 軍品數量,
                                nvl(B.MIL_PRICE, 0) 軍品單價,
                                A.EXG_INQTY 調撥入庫,
                                A.EXG_OUTQTY 調撥出庫,
                                (A.STORE_QTYC + A.STORE_QTYM) 軍加民,
                                ' ' 盤點量,
                                UNIT_EXCHRATIO (A.MMCODE, A.BASE_UNIT, A.M_AGENNO) AS 包裝量,
                                '' 備註
                         FROM (SELECT A.WH_NO,
                                      '1' AS CHK_TYPE,
                                      ' ' AS CHK_YM,
                                      C.MMCODE,
                                      C.MMNAME_E,
                                      C.BASE_UNIT,
                                      C.M_AGENNO,
                                      B.INV_QTY AS STORE_QTY,
                                      B.INV_QTY as STORE_QTYC,
                                      NVL (
                                           (SELECT INV_QTY
                                            FROM MI_WHINV
                                            WHERE     WH_NO = 'PH1X'
                                            AND MMCODE = ('004' || SUBSTR (B.MMCODE, 4, 10))), 0) STORE_QTYM,
                                      B.EXG_INQTY,
                                      B.EXG_OUTQTY,
                                      B.APL_INQTY,
                                      B.APL_OUTQTY,
                                      C.M_CONTPRICE,
                                      C.M_STOREID,
                                      0 AS CHK_QTY
                               FROM MI_WHMAST A, MI_WHINV B, MI_MAST C
                               WHERE     A.WH_GRADE = '1'
                               AND A.WH_KIND = '0'
                               AND A.WH_NO = 'PH1S'
                               AND A.WH_NO = B.WH_NO
                               AND B.MMCODE = C.MMCODE";

            if (type == "1")
            {
                sql += " AND C.E_TAKEKIND IN('11','12','13') and C.E_RESTRICTCODE not IN('1','2','3', '4') ";
            }
            else if (type == "2")
            {
                sql += " AND C.E_TAKEKIND IN('00','21','31','41','51') and C.E_RESTRICTCODE not IN('1','2','3', '4') ";
            }
            else if (type == "3")
            {
                sql += " AND C.E_RESTRICTCODE IN('1','2','3') ";
            }
            else if (type == "4")
            {
                sql += " AND C.E_RESTRICTCODE IN('4') ";
            }
            sql += @" ) A
                      left join MI_WHCOST b on (a.mmcode = b.mmcode and b.data_ym = twn_yyymm(sysdate))
                      ORDER BY A.MMCODE ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public string GetUserName(string id)
        {
            string sql = @"SELECT TUSER || ' ' || UNA FROM UR_ID WHERE UR_ID.TUSER=:TUSER";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { TUSER = id }, DBWork.Transaction).ToString();
            return rtn;
        }
        public string GetUserInid(string id)
        {
            string sql = @"SELECT INID_NAME(INID) || '-' || INID FROM UR_ID WHERE UR_ID.TUSER=:TUSER";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { TUSER = id }, DBWork.Transaction).ToString();
            return rtn;
        }
    }
}
