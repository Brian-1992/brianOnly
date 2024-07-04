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

    public class AA0081ReportMODEL : JCLib.Mvc.BaseModel
    {
        public string F0 { get; set; }
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
        public double F14 { get; set; }
        public double F15 { get; set; }
        public string F16 { get; set; }
        public string CHK_NO { get; set; }
    }
    public class AA0081Repository : JCLib.Mvc.BaseRepository
    {
        public AA0081Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }


        public IEnumerable<AA0081> GetAllM(string chk_ym, string type, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT a.MMCODE, a.MMNAME_E, 
                               PMN_INVQTY(SUBSTR(a.CHK_YM,1,5), a.wh_no, a.mmcode) PMN_INVQTY, 
                               nvl(b.apl_inqty,0)apl_inqty, 
                               nvl(b.apl_outqty,0)apl_outqty,
                               nvl(a.STORE_QTYC,0) STORE_QTYC, 
                               nvl(c.AVG_PRICE,0) AVG_PRICE,
                               nvl((c.AVG_PRICE * a.STORE_QTYC),0) as store_amount, 
                               (case
                                   when (a.mmcode like '005%')
                                       then  nvl(a.STORE_QTYM,0)
                                   else 0
                                end) STORE_QTYM,
                               (case
                                   when (a.mmcode like '005%')
                                       then nvl((select CONT_PRICE from MI_WHCOST 
                                                  where DATA_YM = SUBSTR(a.CHK_YM,1,5)
                                                    and mmcode = ('004'||substr(a.mmcode, 4, 5))),0)
                                   else 0
                                end) MIL_PRICE,
                               nvl(b.EXG_INQTY,0)EXG_INQTY ,
                               nvl(b.EXG_OUTQTY,0)EXG_OUTQTY,
                               nvl((a.STORE_QTYC + a.STORE_QTYM),0) as CaddM,
                               nvl(a.chk_qty,0)chk_qty,
                               nvl(UNIT_EXCHRATIO(a.mmcode, a.BASE_UNIT ,a.M_AGENNO),0) pack_qty 
                         FROM (
                               SELECT A.CHK_NO,
                                      A.CHK_WH_NO wh_no,
                                      A.CHK_TYPE,
                                      A.CHK_YM,
                                      C.MMCODE,
                                      C.MMNAME_E,
                                      C.BASE_UNIT,
                                      (select  M_AGENNO from MI_MAST where MMCODE= c.MMCODE)  as  M_AGENNO,                   
                                      C.STORE_QTY,
                                      C.STORE_QTYC,
                                      C. STORE_QTYM,
                                      C.M_CONTPRICE,
                                      C. M_STOREID,
                                      C. MAT_CLASS,
                                      (CASE
                                        WHEN C.STATUS_TOT = '1' THEN C.CHK_QTY1
                                        WHEN C.STATUS_TOT = '2' THEN C.CHK_QTY2
                                        WHEN C.STATUS_TOT = '3' THEN C.CHK_QTY3
                                        ELSE 0
                                      END)    AS CHK_QTY
                                 FROM CHK_MAST A,  CHK_DETAILTOT C
                                WHERE A.CHK_LEVEL ='1' and A.CHK_STATUS in ('3', 'C', 'P')
                                  AND A.CHK_YM     = :CHK_YM
                                  AND A.CHK_CLASS  = '01'             
                                  AND A.CHK_WH_NO  = 'PH1S'    
                                  AND A.CHK_NO = C.CHK_NO 
                                  AND A.CHK_TYPE = :p1
                              ) a
                         left join CHK_PH1S_WHINV b on (b.chk_no = a.chk_no and b.mmcode = a.mmcode)
                         left join MI_WHCOST c  on (c.DATA_YM = to_char(add_months(to_date(SUBSTR(a.CHK_YM,1,5),'yyymm'),-1),'yyymm') and c.mmcode = b.mmcode)
                        Where 1=1
                          and substr(a.mmcode,1,3) > '004' 
                          and substr(a.mmcode,1,3) < '008'
                        order by a.mmcode
             ";

            p.Add("CHK_YM", chk_ym);
            p.Add(":p1", string.Format("{0}", type));

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<AA0081>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<AA0081ReportMODEL> GetPrintData(string chk_ym, string type)
        {
            var p = new DynamicParameters();
            var sql = @" select rownum as F0, TB.* from (SELECT  a.CHK_NO as CHK_NO, a.MMCODE AS F1, a.MMNAME_E AS F2, 
                        PMN_INVQTY(SUBSTR(a.CHK_YM,1,5), a.wh_no, a.mmcode) AS F3, 
                        nvl(b.apl_inqty,0) AS F4, 
                        nvl(b.apl_outqty,0) AS F5,
                        nvl(a.STORE_QTYC,0)  AS F6, 
                        nvl(c.AVG_PRICE,0) AS F7,
                        nvl((c.AVG_PRICE * a.STORE_QTYC),0) AS F8, 
                        (case
                                    when (a.mmcode like '005%')
                                        then  nvl(a.STORE_QTYM,0)
                                    else 0
                                 end) AS F9,
                        (case
                                    when (a.mmcode like '005%')
                                        then nvl((select CONT_PRICE from MI_WHCOST 
                                                   where DATA_YM = SUBSTR(a.CHK_YM,1,5)
                                                     and mmcode = ('004'||substr(a.mmcode, 4, 5))),0)
                                    else 0
                                 end) AS F10,
                        nvl(b.EXG_INQTY,0) AS F11 ,
                        nvl(b.EXG_OUTQTY,0) AS F12,
                        nvl((a.STORE_QTYC + a.STORE_QTYM),0)  AS F13,
                        nvl(a.chk_qty,0) AS F14,
                        nvl(UNIT_EXCHRATIO(a.mmcode, a.BASE_UNIT ,a.M_AGENNO),0)  AS F15,
                        ''  AS F16 
                    FROM (
                               SELECT A.CHK_NO,
                                      A.CHK_WH_NO wh_no,
                                      A.CHK_TYPE,
                                      A.CHK_YM,
                                      C.MMCODE,
                                      C.MMNAME_E,
                                      C.BASE_UNIT,
                                      (select  M_AGENNO from MI_MAST where MMCODE= c.MMCODE)  as  M_AGENNO,                   
                                      C.STORE_QTY,
                                      C.STORE_QTYC,
                                      C. STORE_QTYM,
                                      C.M_CONTPRICE,
                                      C. M_STOREID,
                                      C. MAT_CLASS,
                                      (CASE
                                        WHEN C.STATUS_TOT = '1' THEN C.CHK_QTY1
                                        WHEN C.STATUS_TOT = '2' THEN C.CHK_QTY2
                                        WHEN C.STATUS_TOT = '3' THEN C.CHK_QTY3
                                        ELSE 0
                                      END)    AS CHK_QTY
                                 FROM CHK_MAST A,  CHK_DETAILTOT C
                                WHERE A.CHK_LEVEL ='1' and A.CHK_STATUS in ('3', 'C', 'P')
                                  AND A.CHK_YM     = :CHK_YM
                                  AND A.CHK_CLASS  = '01'             
                                  AND A.CHK_WH_NO  = 'PH1S'    
                                  AND A.CHK_NO = C.CHK_NO 
                                  AND A.CHK_TYPE = :p1
                              ) a
                         left join CHK_PH1S_WHINV b on (b.chk_no = a.chk_no and b.mmcode = a.mmcode)
                         left join MI_WHCOST c  on (c.DATA_YM = to_char(add_months(to_date(SUBSTR(a.CHK_YM,1,5),'yyymm'),-1),'yyymm') and c.mmcode = b.mmcode)
                        Where 1=1
                          and substr(a.mmcode,1,3) > '004' 
                          and substr(a.mmcode,1,3) < '008'
                        order by a.mmcode ) TB
             ";

            p.Add("CHK_YM", chk_ym);
            p.Add(":p1", string.Format("{0}", type));

            return DBWork.Connection.Query<AA0081ReportMODEL>(sql, p, DBWork.Transaction);
        }

        //匯出
        public DataTable GetExcel(string chk_ym, string type)
        {
            DynamicParameters p = new DynamicParameters();
            var sql = @" select rownum as 項次, TB.* from (SELECT  a.MMCODE 院內碼, a.MMNAME_E 英文品名, 
                        PMN_INVQTY(SUBSTR(a.CHK_YM,1,5), a.wh_no, a.mmcode) 上月結存, 
                        nvl(b.apl_inqty,0) 民品本月進貨, 
                        nvl(b.apl_outqty,0) 民品本月撥發,
                        nvl(a.STORE_QTYC,0) 民品本月結存, 
                        nvl(c.AVG_PRICE,0) 民品單價,
                        nvl((c.AVG_PRICE * a.STORE_QTYC),0) 民品結存金額, 
                        (case
                                    when (a.mmcode like '005%')
                                        then  nvl(a.STORE_QTYM,0)
                                    else 0
                                 end) 軍品數量,
                        (case
                                    when (a.mmcode like '005%')
                                        then nvl((select CONT_PRICE from MI_WHCOST 
                                                   where DATA_YM = SUBSTR(a.CHK_YM,1,5)
                                                     and mmcode = ('004'||substr(a.mmcode, 4, 5))),0)
                                    else 0
                                 end) 軍品單價,
                        nvl(b.EXG_INQTY,0) 調撥入庫 ,nvl(b.EXG_OUTQTY,0) 調撥出庫,
                        nvl((a.STORE_QTYC + a.STORE_QTYM),0) 軍加民,nvl(a.chk_qty,0) 盤點量,nvl(UNIT_EXCHRATIO(a.mmcode, a.BASE_UNIT ,a.M_AGENNO),0) 包裝量,
                        ''  備註 
                    FROM (
                        
                               SELECT A.CHK_NO,
                                      A.CHK_WH_NO wh_no,
                                      A.CHK_TYPE,
                                      A.CHK_YM,
                                      C.MMCODE,
                                      C.MMNAME_E,
                                      C.BASE_UNIT,
                                      (select  M_AGENNO from MI_MAST where MMCODE= c.MMCODE)  as  M_AGENNO,                   
                                      C.STORE_QTY,
                                      C.STORE_QTYC,
                                      C. STORE_QTYM,
                                      C.M_CONTPRICE,
                                      C. M_STOREID,
                                      C. MAT_CLASS,
                                      (CASE
                                        WHEN C.STATUS_TOT = '1' THEN C.CHK_QTY1
                                        WHEN C.STATUS_TOT = '2' THEN C.CHK_QTY2
                                        WHEN C.STATUS_TOT = '3' THEN C.CHK_QTY3
                                        ELSE 0
                                      END)    AS CHK_QTY
                                 FROM CHK_MAST A,  CHK_DETAILTOT C
                                WHERE A.CHK_LEVEL ='1' and A.CHK_STATUS in ('3', 'C', 'P')
                                  AND A.CHK_YM     = :CHK_YM
                                  AND A.CHK_CLASS  = '01'             
                                  AND A.CHK_WH_NO  = 'PH1S'    
                                  AND A.CHK_NO = C.CHK_NO 
                                  AND A.CHK_TYPE = :p1
                              ) a
                         left join CHK_PH1S_WHINV b on (b.chk_no = a.chk_no and b.mmcode = a.mmcode)
                         left join MI_WHCOST c  on (c.DATA_YM = to_char(add_months(to_date(SUBSTR(a.CHK_YM,1,5),'yyymm'),-1),'yyymm') and c.mmcode = b.mmcode)
                        Where 1=1
                          and substr(a.mmcode,1,3) > '004' 
                          and substr(a.mmcode,1,3) < '008'
                        order by a.mmcode) TB
             ";

            p.Add("CHK_YM", chk_ym);
            p.Add(":p1", string.Format("{0}", type));

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
