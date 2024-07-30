using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using WebApp.Models.MI;
using System.Collections.Generic;
using TSGH.Models;

namespace WebApp.Repository.C
{
    public class CE0044Repository : JCLib.Mvc.BaseRepository
    {
        public CE0044Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public class Print2_class
        {
            public string F0 { get; set; }
            public string F1 { get; set; }
            public string S1 { get; set; }
            public string S2 { get; set; }
            public string S3 { get; set; }
            public string S4 { get; set; }
            public string S5 { get; set; }
            public string S6 { get; set; }
            public string S7 { get; set; }
            public string S8 { get; set; }
            public string S9 { get; set; }
            public string S10 { get; set; }
            public string S11 { get; set; }
            public string S12 { get; set; }
            public string S13 { get; set; }
            public string S14 { get; set; }
            public string S15 { get; set; }
            public string S16 { get; set; }
            public string S17 { get; set; }
            public string S18 { get; set; }
        }

        public IEnumerable<CE0044M> GetAllM(string p0, string p1, string p2, string p3, string p4, string p5, string p6, string p7, string p8, string p9, string p10, 
                                            string p11, string p12, string p13, string p14, string p15, string p16, string p17, string p18, string p19, string user, 
                                            string p20, string p21,
                                            int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = "";

            //應有結存
            string temp = @"(CASE WHEN (E.CHK_WH_GRADE = '1') THEN E.STORE_QTYC
                                               WHEN (E.WH_NAME LIKE '%供應中心%') THEN E.STORE_QTYC
                                               WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN (
                                                    (e.pym_inv_qty -- 上月庫存數量 
                                                                +e.apl_inqty   -- 月結入庫總量 
                                                                -e.apl_outqty  -- 月結撥發總量 
                                                                +e.trn_inqty   -- 月結調撥入總量 
                                                                -e.trn_outqty  -- 月結調撥入總量 
                                                                +e.adj_inqty   -- 月結調帳入總量 
                                                                -e.adj_outqty  -- 月結調帳出總量 
                                                                +e.bak_inqty   -- 月結繳回入庫總量 
                                                                -e.bak_outqty  -- 月結繳回出庫總量 
                                                                -e.rej_outqty  -- 月結退貨總量 
                                                                -e.dis_outqty  -- 月結報廢總量
                                                                +e.exg_inqty   -- 月結換貨入庫總量 
                                                                -e.exg_outqty  -- 月結換貨出庫總量 
                                                                -nvl( 
                                                                    nvl(e.altered_use_qty, e.ori_use_qty), -- 調整消耗(只有藥局可以用), 月結耗用量=醫令扣庫 
                                                                    0 
                                                                ) )
                                                    )
                                               ELSE E.STORE_QTYC - (CASE WHEN E.CHK_TIME IS NOT NULL THEN E.USE_QTY_AF_CHK
                                                                         ELSE E.ORI_USE_QTY
                                                                    END)
                                          END) ";
            if (p20 == "true") {
                temp = string.Format(@"
                    (case when E.WAR_QTY > e.chk_qty then 0 
                          else
                            ( {0} - nvl(E.war_qty, 0) )
                    end )
                ", temp);
            }

            #region sql
            //盤存單位 選 各請領單位、各單位消耗結存明細、藥材庫
            if (p1 == "1" || p1 == "2" || p1 == "5"||p1 =="6")
            {
                sql = @"      
                              WITH WH_NOS AS ( SELECT A.WH_NO, WH_KIND 
                                               FROM MI_WHID A, MI_WHMAST B 
                                               WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO)),
                            MAT_WH_NO_ALL AS ( SELECT WH_NO, WH_KIND 
                                               FROM MI_WHMAST 
                                               WHERE WH_KIND = '1' 
                                               AND EXISTS (SELECT 1 
                                                           FROM UR_UIR  
                                                           WHERE TUSER = :USER_NAME
                                               AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14', 'MMSpl_14'))),
                            MED_WH_NO_ALL AS ( SELECT WH_NO, WH_KIND  
                                               FROM MI_WHMAST  
                                               WHERE WH_KIND = '0' 
                                               AND EXISTS (SELECT 1 FROM UR_UIR  
                                                           WHERE TUSER = :USER_NAME 
                                                           AND RLNO IN ('ADMG', 'ADMG_14', 'MED_14', 'MMSpl_14', 'PHR_14'))),
                                    INIDS AS ( SELECT C.INID  
                                               FROM MI_WHID A, MI_WHMAST B, UR_INID C  
                                               WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO)  
                                               AND B.INID = C.INID), 
                                 INID_ALL AS ( SELECT B.INID  
                                               FROM MI_WHMAST A, UR_INID B 
                                               WHERE EXISTS (SELECT 1  
                                                             FROM UR_UIR 
                                                             WHERE TUSER = :USER_NAME  
                                                             AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14', 'MED_14', 'MMSpl_14'))),
                              MAT_WH_KIND AS ( SELECT DISTINCT 1 AS WH_KIND, '衛材' AS TEXT  
                                               FROM MI_WHID A, MI_WHMAST B 
                                               WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO AND B.WH_KIND = '1') 
                                               OR EXISTS (SELECT 1 
                                                          FROM UR_UIR  
                                                          WHERE TUSER = :USER_NAME 
                                                          AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14', 'MMSpl_14'))),
                              MED_WH_KIND AS ( SELECT DISTINCT 0 AS WH_KIND, '藥品' AS TEXT 
                                               FROM MI_WHID A, MI_WHMAST B 
                                               WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO AND B.WH_KIND = '1') 
                                               OR EXISTS (SELECT 1  
                                                          FROM UR_UIR 
                                                          WHERE TUSER = :USER_NAME 
                                                          AND RLNO IN ('ADMG', 'ADMG_14', 'MED_14', 'MMSpl_14', 'PHR_14'))),
                                 CHK_DATA AS ( SELECT B.CHK_WH_NO, B.CHK_WH_KIND, B.CHK_WH_GRADE, B.CHK_YM, A.MMCODE, A.CHK_QTY, A.STORE_QTYC,
                                                      NVL((A.CHK_QTY - A.STORE_QTYC), 0) GAP, NVL(A.WAR_QTY, 0) AS WAR_QTY, NVL(A.PYM_WAR_QTY, 0) AS PYM_WAR_QTY,
                                                      LAST_INVMON(:SET_YM, B.CHK_WH_NO, A.MMCODE) AS PYM_INV_QTY, C.M_CONTPRICE AS CONT_PRICE, C.DISC_CPRICE,
                                                      NVL(D.M_CONTPRICE, 0) AS PYM_CONT_PRICE, NVL(D.DISC_CPRICE, 0) AS PYM_DISC_CPRICE, C.E_SOURCECODE AS SOURCECODE,
                                                      NVL(D.E_SOURCECODE, 'P') AS PYM_SOURCECODE, C.UNITRATE, C.M_CONTID, WH_NAME(B.CHK_WH_NO) AS WH_NAME,
                                                      GET_PARAM('MI_MAST','E_SOURCECODE', C.E_SOURCECODE) AS SOURCECODE_NAME, GET_PARAM('MI_MAST','M_CONTID',
                                                      C.M_CONTID) AS CONTID_NAME, NVL(C.BASE_UNIT, A.BASE_UNIT) BASE_UNIT,
                                                      NVL(A.APL_INQTY, 0) APL_INQTY , NVL(A.APL_OUTQTY, 0) APL_OUTQTY, NVL(A.TRN_INQTY, 0) TRN_INQTY, 
                                                      NVL(A.TRN_OUTQTY, 0) TRN_OUTQTY, NVL(A.ADJ_INQTY, 0) ADJ_INQTY, NVL(A.ADJ_OUTQTY, 0) ADJ_OUTQTY,
                                                      NVL(A.BAK_INQTY, 0) BAK_INQTY, NVL(A.BAK_OUTQTY, 0) BAK_OUTQTY, NVL(A.REJ_OUTQTY, 0) REJ_OUTQTY, 
                                                      NVL(A.DIS_OUTQTY, 0) DIS_OUTQTY, NVL(A.EXG_INQTY, 0) EXG_INQTY, NVL(A.EXG_OUTQTY, 0) EXG_OUTQTY,
                                                      NVL(A.USE_QTY, 0) USE_QTY, NVL(A.USE_QTY_AF_CHK, 0) USE_QTY_AF_CHK, NVL(INVENTORY, 0) INVENTORY, 
                                                      NVL(A.MIL_USE_QTY, 0) MIL_USE_QTY, NVL(A.CIVIL_USE_QTY, 0) CIVIL_USE_QTY, NVL(A.ALTERED_USE_QTY, 0) ALTERED_USE_QTY, 
                                                      A.CHK_TIME, C.CASENO,
                                                      C.WARBAK, NVL(D.WARBAK, ' ') AS PYM_WARBAK, C.M_AGENNO, C.MMNAME_C, C.MMNAME_E, C.MAT_CLASS_SUB,A.G34_MAX_APPQTY ,A.EST_APPQTY 
                                               FROM CHK_MAST B, CHK_DETAIL A, MI_MAST_MON C
                                                    LEFT JOIN MI_MAST_MON D ON (D.MMCODE = C.MMCODE AND D.DATA_YM = TWN_PYM(C.DATA_YM))
                                               WHERE B.CHK_YM = :SET_YM AND B.CHK_NO = A.CHK_NO
                                               AND A.MMCODE = C.MMCODE
                                               AND B.CHK_YM = C.DATA_YM
                                               AND B.CHK_LEVEL = '1'
                                               AND B.CHK_WH_NO IN ( SELECT WH_NO FROM WH_NOS 
                                                                    UNION 
                                                                    SELECT WH_NO FROM MAT_WH_NO_ALL 
                                                                    UNION 
                                                                    SELECT WH_NO FROM MED_WH_NO_ALL)
                                               AND B.CHK_WH_KIND IN ( SELECT WH_KIND FROM MAT_WH_KIND 
                                                                      UNION 
                                                                      SELECT WH_KIND FROM MED_WH_KIND)
                                               AND (EXISTS ( SELECT 1  
                                                             FROM MI_WHMAST A, INIDS B  
                                                             WHERE A.WH_NO = B.CHK_WH_NO  
                                                             AND A.INID = B.INID)  
                                                    OR EXISTS (SELECT 1  
                                                               FROM MI_WHMAST A, INID_ALL B  
                                                               WHERE A.WH_NO = B.CHK_WH_NO  
                                                               AND A.INID = B.INID) ) ";

                if (p17 == "true")
                {
                    sql += @"                  AND A.INVENTORY <> 0";
                }
                if (p21 == "true")
                {
                    sql += @"                  AND (A.WAR_QTY <> 0 and a.chk_qty < A.WAR_QTY)";
                }

                if (p18 == "true")
                {
                    sql += @"and (
exists (select 1 from mi_winvmon
  where mmcode=a.mmcode
    and DATA_YM>=
        substr(twn_date(ADD_MONTHS(twn_todate((select max(set_ym) from mi_mnset where set_status='C')||'01'),-5)),1,5)
    and (APL_INQTY>0 or TRN_INQTY>0 or USE_QTY>0)
    and wh_no=a.wh_no)
or 
exists (select 1 from mi_winvmon
  where mmcode=a.mmcode
    and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
    and INV_QTY<>0
    and wh_no=a.wh_no)
or
exists (select 1 from mi_winvmon b
  where mmcode=a.mmcode
    and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
    and INV_QTY=0
    and wh_no=a.wh_no
    and exists(select 1 from mi_mast_mon where mmcode=b.mmcode
          and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
          and CANCEL_ID='N')
  )
)";
                }

                if (p19 == "true")
                {
                    sql += @"
                    and (
                        NVL(a.PYM_INV_QTY,0) > 0
                        or
                        (NVL(a.PYM_INV_QTY,0) = 0 and not (a.apl_inqty = 0                       
                                                   and a.trn_inqty = 0        
                                                   and a.trn_outqty = 0        
                                                   and a.adj_inqty = 0        
                                                   and a.adj_outqty = 0        
                                                   and a.bak_outqty = 0
                                                    and a.bak_inqty=0
                                                    and nvl(nvl(a.altered_use_qty, a.use_qty),0)=0
                                                )
                        )
                    )
                ";
                }

                if (p1 == "1")//若盤存單位 選 各請領單位
                {
                    sql += @"                  AND B.CHK_WH_GRADE <> '1' 
                                               AND B.CHK_WH_KIND = NVL(TRIM(:CHK_WH_KIND_TEXT), B.CHK_WH_KIND)
                                               AND B.CHK_WH_NO = NVL(TRIM(:CHK_WH_NO_TEXT), B.CHK_WH_NO)
                                               AND B.CHK_WH_NO IN (SELECT WH_NO 
                                                                   FROM MI_WHMAST 
                                                                   WHERE INID = NVL(TRIM(:INID_TEXT), INID)) ";
                }
                else if (p1 == "2")//若盤存單位 選 藥材庫
                {
                    sql += @"                  AND B.CHK_WH_NO IN (WHNO_MM1, WHNO_ME1) ";
                }
                else if (p1 == "5")//若盤存單位 選 各單位消耗結存明細
                {
                    sql += @"                  AND B.CHK_WH_GRADE <> '1'  ";
                }

                sql += @"                      AND A.MMCODE = NVL(TRIM(:MMCODE_TEXT), A.MMCODE) 
                                               AND C.M_CONTID = NVL(TRIM(:M_CONTID_TEXT), C.M_CONTID) 
                                               AND C.E_SOURCECODE = NVL(TRIM(:SOURCECODE_TEXT), C.E_SOURCECODE)
                        ";
                if (p5 == "A")//若藥材類別選A
                {
                    sql += @"                   AND C.MAT_CLASS = '01' ";
                }
                else if (p5 == "B")//若藥材類別選B
                {
                    sql += @"                   AND C.MAT_CLASS = '02' ";
                }
                else if (p5 != "A" && p5 != "B" && p5.Trim() != "")//若藥材類別選其他
                {
                    sql += @"                   AND C.MAT_CLASS_SUB = NVL(TRIM(:MAT_CLASS_SUB_TEXT), C.MAT_CLASS_SUB) ";
                }

                sql += string.Format(@" 
                                                AND C.WARBAK = NVL(TRIM(:WARBAK_TEXT), C.WARBAK)
                                                AND C.E_RESTRICTCODE = NVL(TRIM(:E_RESTRICTCODE_TEXT), C.E_RESTRICTCODE)
                                                AND C.COMMON = NVL(TRIM(:COMMON_TEXT), C.COMMON)
                                                AND C.FASTDRUG = NVL(TRIM(:FASTDRUG_TEXT), C.FASTDRUG)
                                                AND C.DRUGKIND = NVL(TRIM(:DRUGKIND_TEXT), C.DRUGKIND)
                                                AND C.TOUCHCASE = NVL(TRIM(:TOUCHCASE_TEXT), C.TOUCHCASE)
                                                AND C.ORDERKIND = NVL(TRIM(:ORDERKIND_TEXT), C.ORDERKIND)
                                                AND C.SPDRUG = NVL(TRIM(:SPDRUG_TEXT), C.SPDRUG)), 
                              INVMON_DATA AS ( SELECT A.CHK_WH_NO AS WH_NO, A.WH_NAME, A.MMCODE, SUM(NVL(A.STORE_QTYC, 0)) AS ORI_INV_QTY, 
                                                      SUM(A.APL_INQTY) AS APL_INQTY,
                                                      (CASE WHEN A.CHK_WH_GRADE = '1' THEN SUM(A.APL_INQTY) else 0 END) AS APL_INQTY_1,
                                                      (CASE WHEN A.CHK_WH_GRADE != '1' THEN SUM(A.APL_INQTY) else 0 END) AS APL_INQTY_2,
                                                      SUM(A.APL_OUTQTY) AS APL_OUTQTY, SUM(A.TRN_INQTY) AS TRN_INQTY, 
                                                      SUM(A.TRN_OUTQTY) AS TRN_OUTQTY, SUM(A.ADJ_INQTY) AS ADJ_INQTY, SUM(A.ADJ_OUTQTY) AS ADJ_OUTQTY,
                                                      SUM(A.BAK_INQTY) AS BAK_INQTY, SUM(A.BAK_OUTQTY) AS BAK_OUTQTY, SUM(A.REJ_OUTQTY) AS REJ_OUTQTY, 
                                                      SUM(A.DIS_OUTQTY) AS DIS_OUTQTY, SUM(A.EXG_INQTY) AS EXG_INQTY, SUM(A.EXG_OUTQTY) AS EXG_OUTQTY,
                                                      SUM(NVL(A.USE_QTY, 0)) AS ORI_USE_QTY, SUM(NVL(A.USE_QTY_AF_CHK, 0)) AS USE_QTY_AF_CHK, 
                                                      SUM(NVL(A.INVENTORY, 0)) AS INVENTORY, SUM(NVL(A.MIL_USE_QTY, 0)) AS MIL_USE_QTY, 
                                                      SUM(NVL(A.CIVIL_USE_QTY, 0)) AS CIVIL_USE_QTY,
                                                      SUM(NVL(A.ALTERED_USE_QTY, 0)) AS ALTERED_USE_QTY, A.CHK_TIME,
                                                      A.CHK_WH_KIND, A.CHK_WH_GRADE,A.CHK_YM, 
                                                      SUM(A.CHK_QTY) AS CHK_QTY, SUM(A.STORE_QTYC) AS STORE_QTYC, SUM(A.GAP) AS GAP, 
                                                      SUM(A.WAR_QTY) AS WAR_QTY, SUM(A.PYM_WAR_QTY) AS PYM_WAR_QTY, SUM(A.PYM_INV_QTY) AS PYM_INV_QTY, 
                                                      A.CONT_PRICE, A.DISC_CPRICE, A.PYM_CONT_PRICE, A.PYM_DISC_CPRICE, A.SOURCECODE, A.PYM_SOURCECODE,
                                                      A.UNITRATE, A.M_CONTID, A.SOURCECODE_NAME, A.CONTID_NAME, A.BASE_UNIT, A.WARBAK, A.PYM_WARBAK, 
                                                      A.M_AGENNO, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS_SUB, A.CASENO,
                                                      SUM(A.G34_MAX_APPQTY) AS G34_MAX_APPQTY ,SUM(A.EST_APPQTY ) AS EST_APPQTY 
                                               FROM CHK_DATA A
                                               GROUP BY A.CHK_WH_NO, A.WH_NAME, A.MMCODE, A.CHK_WH_KIND, A.CHK_WH_GRADE, A.CHK_YM,A.CONT_PRICE, A.DISC_CPRICE,
                                                        A.PYM_CONT_PRICE, A.PYM_DISC_CPRICE, A.SOURCECODE, A.PYM_SOURCECODE, A.UNITRATE, A.M_CONTID, A.CASENO,
                                                        A.SOURCECODE_NAME, A.CONTID_NAME, A.BASE_UNIT, A.WARBAK, A.PYM_WARBAK, A.M_AGENNO, A.MMNAME_C, 
                                                        A.MMNAME_E, A.MAT_CLASS_SUB, A.CHK_TIME )

                              SELECT E.WH_NO || ' '|| WH_NAME(E.WH_NO) F1 ,--盤存單位
                                     E.MMCODE F2 ,--藥材代碼
                                     E.M_AGENNO F3 ,--廠商代碼
                                     E.MMNAME_C F4 ,--藥材名稱
                                     E.MMNAME_E F61 ,--英文品名
                                     E.BASE_UNIT F5 ,--藥材單位
                                     E.CONT_PRICE F6 ,--單價
                                     E.DISC_CPRICE F7 ,--優惠後單價
                                     E.PYM_CONT_PRICE F8 ,--上月單價
                                     E.PYM_DISC_CPRICE F9 ,--上月優惠後單價
                                     E.UNITRATE F10 ,--包裝量
                                     {0} F11 ,--上月結存
                                     NVL(E.APL_INQTY_1, 0) F12_1 ,--進貨入庫
                                     NVL(E.APL_INQTY_2, 0) F12_2 ,--撥發入庫
                                     E.APL_OUTQTY F13 ,--撥發出庫
                                     E.TRN_INQTY F14 ,--調撥入庫
                                     E.TRN_OUTQTY F15 ,--調撥出庫
                                     E.ADJ_INQTY F16 ,--調帳入庫
                                     E.ADJ_OUTQTY F17 ,--調帳出庫
                                     E.BAK_INQTY F18 ,--繳回入庫
                                     E.BAK_OUTQTY F19 ,--繳回出庫
                                     E.REJ_OUTQTY F20 ,--退貨量
                                     E.DIS_OUTQTY F21 ,--報廢量
                                     E.EXG_INQTY F22 ,--換貨入庫
                                     E.EXG_OUTQTY F23 ,--換貨出庫
                                     0 AS F24 ,--軍方消耗
                                     (CASE WHEN (E.CHK_WH_GRADE = '1') THEN 0 
                                           WHEN (E.WH_NAME LIKE '%供應中心%') THEN 0
                                           WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN E.ALTERED_USE_QTY
                                           ELSE E.USE_QTY_AF_CHK
                                      END) F25 ,--民眾消耗
                                     --(E.ORI_USE_QTY + (CASE WHEN(E.GAP <= 0) THEN (E.STORE_QTYC - E.CHK_QTY) ELSE 0 END)) F25,
                                     (CASE WHEN (E.CHK_WH_GRADE = '1') THEN 0 
                                           WHEN (E.WH_NAME LIKE '%供應中心%') THEN 0
                                           WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN E.ALTERED_USE_QTY
                                           ELSE E.USE_QTY_AF_CHK
                                      END) F26 ,--本月總消耗
                                     --(E.ORI_USE_QTY + (CASE WHEN(E.GAP <= 0) THEN (E.STORE_QTYC - E.CHK_QTY) ELSE 0 END)) F26, 
                                     ( {3} ) F27 ,--應有結存
                                     {2} F28 ,--盤存量
                                     {2} F29 ,--本月結存
                                     (CASE WHEN E.PYM_SOURCECODE = 'C' THEN {0}
                                           ELSE 0
                                      END) F30 ,--上月寄庫藥品買斷結存
                                     (CASE WHEN E.SOURCECODE = 'C' THEN {2}
                                           ELSE 0
                                      END) F31 ,--本月寄庫藥品買斷結存
                                     E.INVENTORY F32 ,--差異量
                                     E.CONT_PRICE * ({2}) F33 ,--結存金額
                                     E.DISC_CPRICE * ({2}) F34 ,--優惠後結存金額
                                     E.INVENTORY * E.CONT_PRICE F35 ,--差異金額
                                     E.INVENTORY * E.DISC_CPRICE F36 ,--優惠後差異金額
                                     get_param('MI_MAST', 'MAT_CLASS_SUB', E.MAT_CLASS_SUB) F37 ,--藥材類別
                                     E.CONTID_NAME F38 ,--是否合約
                                     E.SOURCECODE_NAME F39 ,--買斷寄庫
                                     E.WARBAK F40 ,--是否戰備
                                     E.WAR_QTY F41 ,--戰備存量
                                     ROUND((CASE WHEN (E.ORI_USE_QTY + (CASE WHEN (E.GAP <= 0) THEN (E.STORE_QTYC - E.CHK_QTY) 
                                                                             ELSE 0
                                                                        END)) = 0  
                                                 THEN 0
                                                 ELSE ((E.CHK_QTY {1} ) / (E.ORI_USE_QTY + (CASE WHEN (E.GAP <= 0) THEN (E.STORE_QTYC - E.CHK_QTY) 
                                                                                          ELSE 0
                                                                                     END)))
                                            END), 2) F42 ,--期末比
                                     0 F43 ,--贈品數量
                                     E.PYM_WAR_QTY F44 ,--上月戰備量
                                     E.PYM_WARBAK F45 ,--上月是否戰備
                                     (CASE WHEN (E.PYM_INV_QTY - E.PYM_WAR_QTY) < 0 THEN 0 
                                           ELSE E.PYM_INV_QTY - E.PYM_WAR_QTY
                                      END) F46 ,--不含戰備上月結存
                                     E.DISC_CPRICE - E.PYM_DISC_CPRICE F47 ,--單價差額
                                     (CASE WHEN (E.PYM_INV_QTY - E.PYM_WAR_QTY) < 0 THEN 0 
                                           ELSE E.PYM_INV_QTY - E.PYM_WAR_QTY
                                      END) * (E.DISC_CPRICE - E.PYM_DISC_CPRICE) F48 ,--不含戰備上月結存價差
                                     (CASE WHEN E.CHK_QTY <= E.WAR_QTY THEN E.CHK_QTY * (E.DISC_CPRICE - E.PYM_DISC_CPRICE)
                                           ELSE (E.CHK_QTY - E.WAR_QTY) * (E.DISC_CPRICE - E.PYM_DISC_CPRICE)
                                      END) F49 ,--戰備本月價差
                                     (CASE WHEN E.WAR_QTY = 0 then E.CHK_QTY 
                                           WHEN E.CHK_QTY <= E.WAR_QTY THEN 0
                                           ELSE E.CHK_QTY - E.WAR_QTY
                                      END) F50 ,--不含戰備本月結存
                                     (case when E.WAR_QTY > e.chk_qty then 0 
                                       else
                                        (
                                              (CASE WHEN (E.CHK_WH_GRADE = '1') THEN E.STORE_QTYC
                                                 WHEN (E.WH_NAME LIKE '%供應中心%') THEN E.STORE_QTYC
                                                 WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN (
                                                      (e.pym_inv_qty -- 上月庫存數量 
                                                                  +e.apl_inqty   -- 月結入庫總量 
                                                                  -e.apl_outqty  -- 月結撥發總量 
                                                                  +e.trn_inqty   -- 月結調撥入總量 
                                                                  -e.trn_outqty  -- 月結調撥入總量 
                                                                  +e.adj_inqty   -- 月結調帳入總量 
                                                                  -e.adj_outqty  -- 月結調帳出總量 
                                                                  +e.bak_inqty   -- 月結繳回入庫總量 
                                                                  -e.bak_outqty  -- 月結繳回出庫總量 
                                                                  -e.rej_outqty  -- 月結退貨總量 
                                                                  -e.dis_outqty  -- 月結報廢總量
                                                                  +e.exg_inqty   -- 月結換貨入庫總量 
                                                                  -e.exg_outqty  -- 月結換貨出庫總量 
                                                                  -nvl( 
                                                                      nvl(e.altered_use_qty, e.ori_use_qty), -- 調整消耗(只有藥局可以用), 月結耗用量=醫令扣庫 
                                                                      0 
                                                                  ) )
                                                      )
                                                 ELSE E.STORE_QTYC - (CASE WHEN E.CHK_TIME IS NOT NULL THEN E.USE_QTY_AF_CHK
                                                                           ELSE E.ORI_USE_QTY
                                                                      END)
                                            END) - nvl(e.war_qty, 0)
                                       ) 
                                     end) F51 ,--不含戰備本月應有結存量
                                     (CASE WHEN E.WAR_QTY = 0 then E.CHK_QTY 
                                           WHEN E.CHK_QTY <= E.WAR_QTY THEN 0
                                           ELSE E.CHK_QTY - E.WAR_QTY
                                      END) F52 ,--不含戰備本月盤存量
                                     (E.APL_INQTY - 0) F53 ,--不含贈品本月進貨
                                     E.G34_MAX_APPQTY F54 ,--單位申請基準量
                                     E.EST_APPQTY F55 ,--下月預計申請量
                                       E.BAK_INQTY F56 ,--退料入庫
                                       E.BAK_OUTQTY F57 ,--退料出庫
                                       (CASE WHEN E.DISC_CPRICE >= 5000 THEN (CASE WHEN (E.CHK_WH_GRADE = '1') THEN 0 
                                                                                   WHEN (E.WH_NAME LIKE '%供應中心%') THEN 0
                                                                                   WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN E.ALTERED_USE_QTY
                                                                                   ELSE E.USE_QTY_AF_CHK
                                                                              END)
                                             ELSE 0 
                                        END) F58, --本月單價5000元以上總消耗
                                       (CASE WHEN E.DISC_CPRICE < 5000 THEN (CASE WHEN (E.CHK_WH_GRADE = '1') THEN 0 
                                                                                  WHEN (E.WH_NAME LIKE '%供應中心%') THEN 0
                                                                                  WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN E.ALTERED_USE_QTY
                                                                                  ELSE E.USE_QTY_AF_CHK
                                                                             END)
                                             ELSE 0 
                                        END) F59  ,--本月單價未滿5000元總消耗
                                       E.CASENO F60, --合約案號
                                       {0} * E.PYM_DISC_CPRICE T01 ,--上月結存總金額=上月結存*上月優惠後單價
                                       E.APL_INQTY * E.DISC_CPRICE T02 ,--本月進貨總金額=進貨*優惠後單價
                                       E.REJ_OUTQTY * E.DISC_CPRICE T03 ,--本月退貨總金額=退貨量*優惠後單價
                                       0 T04 ,--本月贈品總金額=贈品數量*優惠後單價
                                       (CASE WHEN (E.CHK_WH_GRADE = '1') THEN 0 
                                             WHEN (E.WH_NAME LIKE '%供應中心%') THEN 0
                                             WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN E.ALTERED_USE_QTY
                                             ELSE E.USE_QTY_AF_CHK
                                        END) * E.DISC_CPRICE T05 ,--消耗金額總金額=本月總消耗*優惠後單價
                                       0 T06 ,--軍方消耗總金額=軍方消耗*優惠後單價
                                      (CASE WHEN (E.CHK_WH_GRADE = '1') THEN 0 
                                            WHEN (E.WH_NAME LIKE '%供應中心%') THEN 0
                                            WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN E.ALTERED_USE_QTY
                                            ELSE E.USE_QTY_AF_CHK
                                       END) * E.DISC_CPRICE T07 ,--民眾消耗總金額=民眾消耗*優惠後單價
                                      (E.BAK_INQTY - E.BAK_OUTQTY) * E.DISC_CPRICE T08 ,--退料總金額=(退料入庫-退料出庫)*優惠後單價
                                      (E.TRN_INQTY - E.TRN_OUTQTY) * E.DISC_CPRICE T09 ,--調撥總金額=(調撥入庫-調撥出庫)*優惠後單價
                                      (E.EXG_INQTY - E.EXG_OUTQTY) * E.DISC_CPRICE T10 ,--換貨總金額=(換貨入庫-換貨出庫)*優惠後單價
                                      E.DIS_OUTQTY * E.DISC_CPRICE T11 ,--報廢總金額=報廢量*優惠後單價
                                      ( {3} ) * E.DISC_CPRICE T12 ,--應有結存總金額=應有結存*優惠後單價
                                      E.CHK_QTY * E.DISC_CPRICE T13 ,--盤點結存總金額=盤存量*優惠後單價
                                      E.CHK_QTY * E.DISC_CPRICE T14 ,--本月結存總金額=本月結存*優惠後單價
                                      (CASE WHEN E.SOURCECODE_NAME = '買斷' THEN E.CHK_QTY 
                                            ELSE 0 
                                       END) * E.DISC_CPRICE T15 ,--買斷結存總金額=(CASE WHEN A.買斷寄庫 = '買斷' THEN 本月結存 ELSE 0 END) * 優惠後單價
                                      (CASE WHEN E.PYM_SOURCECODE = 'C' THEN E.PYM_INV_QTY
                                            ELSE 0
                                       END) * E.DISC_CPRICE  T16,--上月寄庫藥品買斷結存總金額=上月寄庫藥品買斷結存*優惠後單價
                                      (CASE WHEN E.SOURCECODE = 'C' THEN E.CHK_QTY
                                            ELSE 0
                                       END) * E.DISC_CPRICE T17 ,--本月寄庫藥品買斷結存總金額=本月寄庫藥品買斷結存*優惠後單價
                                      ( CASE
                WHEN E.war_qty > 0 then (case when e.chk_qty > e.war_qty then e.war_qty else e.chk_qty end)
                ELSE 0
                END ) * E.DISC_CPRICE T18 ,--戰備金額總金額
                                      (CASE WHEN E.SOURCECODE_NAME = '寄庫' THEN E.CHK_QTY 
                                            ELSE 0
                                       END) * E.DISC_CPRICE T19 ,--寄庫結存總金額
                                      (CASE WHEN (E.PYM_INV_QTY - E.PYM_WAR_QTY) < 0 THEN 0 
                                            ELSE E.PYM_INV_QTY - E.PYM_WAR_QTY
                                       END) * (E.DISC_CPRICE - E.PYM_DISC_CPRICE) T20 ,--不含戰備上月結存價差金額
                                      (CASE WHEN E.CHK_QTY <= E.WAR_QTY THEN E.CHK_QTY * (E.DISC_CPRICE - E.PYM_DISC_CPRICE)
                                            ELSE (E.CHK_QTY - E.WAR_QTY) * (E.DISC_CPRICE - E.PYM_DISC_CPRICE)
                                       END) T21 ,--戰備本月價差金額
                                      0 T22 ,--折讓總金額
                                      (CASE WHEN :RLNO_TEXT = '2' THEN TO_CHAR((E.APL_INQTY * E.DISC_CPRICE)-(E.REJ_OUTQTY * E.DISC_CPRICE))
                                            ELSE ''
                                       END) T23 ,--進貨總金額
                                      (CASE WHEN((E.CHK_WH_KIND ='1' AND (E.CHK_WH_GRADE <> '1' OR E.WH_NAME NOT LIKE '%供應中心%') ) OR (E.CHK_WH_KIND='0' AND E.CHK_WH_GRADE NOT IN ('1','2')) ) THEN 0
                                            WHEN E.INVENTORY * E.CONT_PRICE > 0 THEN E.INVENTORY * E.CONT_PRICE
                                            ELSE 0
                                       END) T24 ,--盤盈總金額
                                      (CASE WHEN((E.CHK_WH_KIND ='1' AND (E.CHK_WH_GRADE <> '1' OR E.WH_NAME NOT LIKE '%供應中心%') ) OR (E.CHK_WH_KIND='0' AND E.CHK_WH_GRADE NOT IN ('1','2')) ) THEN 0
                                            WHEN E.INVENTORY * E.CONT_PRICE < 0 THEN E.INVENTORY * E.CONT_PRICE
                                            ELSE 0
                                       END)  T25 ,--盤虧總金額  
                                      (CASE WHEN((E.CHK_WH_KIND ='1' AND (E.CHK_WH_GRADE <> '1' OR E.WH_NAME NOT LIKE '%供應中心%') ) OR (E.CHK_WH_KIND='0' AND E.CHK_WH_GRADE NOT IN ('1','2')) ) THEN 0
                                            ELSE E.INVENTORY * E.CONT_PRICE
                                       END) T26 ,-- 合計盤盈虧總金額
                                      (CASE WHEN E.DISC_CPRICE >= 5000 THEN (CASE WHEN (E.CHK_WH_GRADE = '1') THEN 0 
                                                                                  WHEN (E.WH_NAME LIKE '%供應中心%') THEN 0
                                                                                  WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN E.ALTERED_USE_QTY
                                                                                  ELSE E.USE_QTY_AF_CHK
                                                                             END)
                                            ELSE 0 
                                       END) * E.DISC_CPRICE T27, --本月單價5000元以上消耗總金額
                                      (CASE WHEN E.DISC_CPRICE < 5000 THEN (CASE WHEN (E.CHK_WH_GRADE = '1') THEN 0 
                                                                                 WHEN (E.WH_NAME LIKE '%供應中心%') THEN 0
                                                                                 WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN E.ALTERED_USE_QTY
                                                                                 ELSE E.USE_QTY_AF_CHK
                                                                            END)
                                            ELSE 0 
                                       END) * E.DISC_CPRICE T28  --本月單價未滿5000元消耗總金額    
                              FROM INVMON_DATA E WHERE 1 = 1 "
                 , p20 == "false" ? "E.PYM_INV_QTY": @"(case when E.PYM_WAR_QTY = 0 then E.PYM_INV_QTY when E.PYM_INV_QTY <= E.PYM_WAR_QTY then 0 else E.PYM_INV_QTY - E.PYM_WAR_QTY end)"
                 , p20 == "false" ? string.Empty : " - nvl(E.WAR_QTY, 0)"
                 , p20 == "false" ? "E.CHK_QTY" : @"(case when E.WAR_QTY = 0 then E.CHK_QTY when E.CHK_QTY <= E.WAR_QTY then 0 else E.CHK_QTY - E.WAR_QTY end)"
                 , temp
                );
            }
            else if (p1 == "3" || p1 == "4")//盤存單位 選 全部藥局/全院
            {
                sql = @"        WITH WH_NOS AS ( SELECT A.WH_NO, WH_KIND 
                                                 FROM MI_WHID A, MI_WHMAST B 
                                                 WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO)),
                              MAT_WH_NO_ALL AS ( SELECT WH_NO, WH_KIND
                                                 FROM MI_WHMAST 
                                                 WHERE WH_KIND = '1'
                                                 AND EXISTS (SELECT 1
                                                             FROM UR_UIR 
                                                             WHERE TUSER = :USER_NAME 
                                                             AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14', 'MMSpl_14'))), 
                              MED_WH_NO_ALL AS ( SELECT WH_NO, WH_KIND 
                                                 FROM MI_WHMAST 
                                                 WHERE WH_KIND = '0'
                                                 AND EXISTS (SELECT 1 
                                                             FROM UR_UIR
                                                             WHERE TUSER = :USER_NAME 
                                                             AND RLNO IN ('ADMG', 'ADMG_14', 'MED_14', 'PHR_14', 'MMSpl_14'))),
                                      INIDS AS ( SELECT C.INID  
                                                 FROM MI_WHID A, MI_WHMAST B, UR_INID C 
                                                 WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO) 
                                                 AND B.INID = C.INID), 
                                   INID_ALL AS ( SELECT B.INID  
                                                 FROM MI_WHMAST A, UR_INID B  
                                                 WHERE EXISTS (SELECT 1  
                                                               FROM UR_UIR  
                                                               WHERE TUSER = :USER_NAME  
                                                               AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14', 'MED_14', 'MMSpl_14'))),
                                MAT_WH_KIND AS ( SELECT DISTINCT 1 AS WH_KIND, '衛材' AS TEXT  
                                                 FROM MI_WHID A, MI_WHMAST B  
                                                 WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO AND B.WH_KIND = '1')  
                                                 OR EXISTS (SELECT 1
                                                            FROM UR_UIR
                                                            WHERE TUSER = :USER_NAME
                                                            AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14', 'MMSpl_14'))), 
                                MED_WH_KIND AS ( SELECT DISTINCT 0 AS WH_KIND, '藥品' AS TEXT  
                                                 FROM MI_WHID A, MI_WHMAST B  
                                                 WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO AND B.WH_KIND = '1')  
                                                 OR EXISTS (SELECT 1   
                                                            FROM UR_UIR   
                                                            WHERE TUSER = :USER_NAME  
                                                            AND RLNO IN ('ADMG', 'ADMG_14', 'MED_14', 'PHR_14', 'MMSpl_14'))), 
                                   CHK_DATA AS ( SELECT B.CHK_WH_NO, B.CHK_WH_KIND, B.CHK_WH_GRADE, B.CHK_YM, A.MMCODE, A.CHK_QTY, A.STORE_QTYC,
                                                        NVL((A.CHK_QTY - A.STORE_QTYC), 0) GAP, NVL(A.WAR_QTY, 0) AS WAR_QTY, NVL(A.PYM_WAR_QTY, 0) AS PYM_WAR_QTY,
                                                        LAST_INVMON(:SET_YM, B.CHK_WH_NO, A.MMCODE) AS PYM_INV_QTY, C.M_CONTPRICE AS CONT_PRICE, C.DISC_CPRICE,
                                                        NVL(D.M_CONTPRICE, 0) AS PYM_CONT_PRICE, NVL(D.DISC_CPRICE, 0) AS PYM_DISC_CPRICE, C.E_SOURCECODE AS SOURCECODE,
                                                        NVL(D.E_SOURCECODE, 'P') AS PYM_SOURCECODE, C.UNITRATE, C.M_CONTID, WH_NAME(B.CHK_WH_NO) AS WH_NAME,
                                                        GET_PARAM('MI_MAST','E_SOURCECODE', C.E_SOURCECODE) AS SOURCECODE_NAME, GET_PARAM('MI_MAST','M_CONTID',
                                                        C.M_CONTID) AS CONTID_NAME, NVL(C.BASE_UNIT, A.BASE_UNIT) BASE_UNIT,
                                                        NVL(A.APL_INQTY, 0) APL_INQTY , NVL(A.APL_OUTQTY, 0) APL_OUTQTY, NVL(A.TRN_INQTY, 0) TRN_INQTY, 
                                                        NVL(A.TRN_OUTQTY, 0) TRN_OUTQTY, NVL(A.ADJ_INQTY, 0) ADJ_INQTY, NVL(A.ADJ_OUTQTY, 0) ADJ_OUTQTY,
                                                        NVL(A.BAK_INQTY, 0) BAK_INQTY, NVL(A.BAK_OUTQTY, 0) BAK_OUTQTY, NVL(A.REJ_OUTQTY, 0) REJ_OUTQTY, 
                                                        NVL(A.DIS_OUTQTY, 0) DIS_OUTQTY, NVL(A.EXG_INQTY, 0) EXG_INQTY, NVL(A.EXG_OUTQTY, 0) EXG_OUTQTY,
                                                        NVL(A.USE_QTY, 0) USE_QTY, NVL(A.USE_QTY_AF_CHK, 0) USE_QTY_AF_CHK, NVL(INVENTORY, 0) INVENTORY, 
                                                        NVL(A.MIL_USE_QTY, 0) MIL_USE_QTY, NVL(A.CIVIL_USE_QTY, 0) CIVIL_USE_QTY, NVL(A.ALTERED_USE_QTY, 0) ALTERED_USE_QTY, 
                                                        A.CHK_TIME, C.CASENO,
                                                        C.WARBAK, NVL(D.WARBAK, ' ') AS PYM_WARBAK, C.M_AGENNO, C.MMNAME_C, C.MMNAME_E, C.MAT_CLASS_SUB,A.G34_MAX_APPQTY ,A.EST_APPQTY 
                                                 FROM CHK_MAST B, CHK_DETAIL A, MI_MAST_MON C
                                                      LEFT JOIN MI_MAST_MON D ON (D.MMCODE = C.MMCODE AND D.DATA_YM = TWN_PYM(C.DATA_YM))
                                                 WHERE B.CHK_YM = :SET_YM
                                                 AND B.CHK_NO = A.CHK_NO
                                                 AND A.MMCODE = C.MMCODE
                                                 AND B.CHK_YM = C.DATA_YM
                                                 AND B.CHK_LEVEL = '1' ";

                if (p17 == "true")
                {
                    sql += @"                    AND A.INVENTORY <> 0";
                }
                if (p21 == "true")
                {
                    sql += @"                  AND (A.WAR_QTY <> 0 and a.chk_qty < A.WAR_QTY)";
                }

                if (p18 == "true")
                {
                    sql += @"and (
exists (select 1 from mi_winvmon
  where mmcode=a.mmcode
    and DATA_YM>=
        substr(twn_date(ADD_MONTHS(twn_todate((select max(set_ym) from mi_mnset where set_status='C')||'01'),-5)),1,5)
    and (APL_INQTY>0 or TRN_INQTY>0 or USE_QTY>0)
    and wh_no=a.wh_no)
or 
exists (select 1 from mi_winvmon
  where mmcode=a.mmcode
    and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
    and INV_QTY<>0
    and wh_no=a.wh_no)
or
exists (select 1 from mi_winvmon b
  where mmcode=a.mmcode
    and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
    and INV_QTY=0
    and wh_no=a.wh_no
    and exists(select 1 from mi_mast_mon where mmcode=b.mmcode
          and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
          and CANCEL_ID='N')
  )
)";
                }

                if (p19 == "true")
                {
                    sql += @"
                    and (
                        NVL(a.PYM_INV_QTY,0) > 0
                        or
                        (NVL(a.PYM_INV_QTY,0) = 0 and not (a.apl_inqty = 0                       
                                                   and a.trn_inqty = 0        
                                                   and a.trn_outqty = 0        
                                                   and a.adj_inqty = 0        
                                                   and a.adj_outqty = 0        
                                                   and a.bak_outqty = 0
                                                    and a.bak_inqty=0
                                                    and nvl(nvl(a.altered_use_qty, a.use_qty),0)=0
                                                )
                        )
                    )
                ";
                }

                if (p1 == "3") //若盤存單位 選 全部藥局
                {
                    sql += @"                    AND B.CHK_WH_KIND = '0' 
                                                 AND B.CHK_WH_GRADE = '2' ";
                }

                sql += @"                        AND A.MMCODE = NVL(TRIM(:MMCODE_TEXT), A.MMCODE) 
                                                 AND C.M_CONTID = NVL(TRIM(:M_CONTID_TEXT), C.M_CONTID) 
                                                 AND C.e_SOURCECODE = NVL(TRIM(:SOURCECODE_TEXT), C.e_SOURCECODE) 
                ";
                if (p5 == "A")//若藥材類別選A
                {
                    sql += @"                    AND C.MAT_CLASS = '01' ";
                }
                else if (p5 == "B")//若藥材類別選B
                {
                    sql += @"                    AND C.MAT_CLASS = '02' ";
                }
                else if (p5 != "A" && p5 != "B" && p5.Trim() != "")//若藥材類別選其他
                {
                    sql += @"                   AND C.MAT_CLASS_SUB = NVL(TRIM(:MAT_CLASS_SUB_TEXT), C.MAT_CLASS_SUB) ";
                }

                sql += string.Format(@"
                                                AND C.WARBAK = NVL(TRIM(:WARBAK_TEXT), C.WARBAK)
                                                AND C.E_RESTRICTCODE = NVL(TRIM(:E_RESTRICTCODE_TEXT), C.E_RESTRICTCODE)
                                                AND C.COMMON = NVL(TRIM(:COMMON_TEXT), C.COMMON)
                                                AND C.FASTDRUG = NVL(TRIM(:FASTDRUG_TEXT), C.FASTDRUG)
                                                AND C.DRUGKIND = NVL(TRIM(:DRUGKIND_TEXT), C.DRUGKIND)
                                                AND C.TOUCHCASE = NVL(TRIM(:TOUCHCASE_TEXT), C.TOUCHCASE)
                                                AND C.ORDERKIND = NVL(TRIM(:ORDERKIND_TEXT), C.ORDERKIND)
                                                AND C.SPDRUG = NVL(TRIM(:SPDRUG_TEXT), C.SPDRUG)),
                              INVMON_DATA AS ( SELECT '{0}' AS WH_NO, A.WH_NAME, A.MMCODE, SUM(NVL(A.STORE_QTYC, 0)) AS ORI_INV_QTY, 
                                                      SUM(A.APL_INQTY) AS APL_INQTY,
                                                      (CASE WHEN A.CHK_WH_GRADE = '1' THEN SUM(A.APL_INQTY) else 0 END) AS APL_INQTY_1,
                                                      (CASE WHEN A.CHK_WH_GRADE != '1' THEN SUM(A.APL_INQTY) else 0 END) AS APL_INQTY_2,
                                                      SUM(A.APL_OUTQTY) AS APL_OUTQTY, SUM(A.TRN_INQTY) AS TRN_INQTY, 
                                                      SUM(A.TRN_OUTQTY) AS TRN_OUTQTY, SUM(A.ADJ_INQTY) AS ADJ_INQTY, SUM(A.ADJ_OUTQTY) AS ADJ_OUTQTY,
                                                      SUM(A.BAK_INQTY) AS BAK_INQTY, SUM(A.BAK_OUTQTY) AS BAK_OUTQTY, SUM(A.REJ_OUTQTY) AS REJ_OUTQTY, 
                                                      SUM(A.DIS_OUTQTY) AS DIS_OUTQTY, SUM(A.EXG_INQTY) AS EXG_INQTY, SUM(A.EXG_OUTQTY) AS EXG_OUTQTY,
                                                      SUM(NVL(A.USE_QTY, 0)) AS ORI_USE_QTY, SUM(NVL(A.USE_QTY_AF_CHK, 0)) AS USE_QTY_AF_CHK, 
                                                      SUM(NVL(A.INVENTORY, 0)) AS INVENTORY, SUM(NVL(A.MIL_USE_QTY, 0)) AS MIL_USE_QTY, 
                                                      SUM(NVL(A.CIVIL_USE_QTY, 0)) AS CIVIL_USE_QTY,
                                                      SUM(NVL(A.ALTERED_USE_QTY, 0)) AS ALTERED_USE_QTY, A.CHK_TIME,
                                                      A.CHK_WH_KIND, A.CHK_WH_GRADE,A.CHK_YM, 
                                                      SUM(A.CHK_QTY) AS CHK_QTY, SUM(A.STORE_QTYC) AS STORE_QTYC, SUM(A.GAP) AS GAP, 
                                                      SUM(A.WAR_QTY) AS WAR_QTY, SUM(A.PYM_WAR_QTY) AS PYM_WAR_QTY, SUM(A.PYM_INV_QTY) AS PYM_INV_QTY, 
                                                      A.CONT_PRICE, A.DISC_CPRICE, A.PYM_CONT_PRICE, A.PYM_DISC_CPRICE, A.SOURCECODE, A.PYM_SOURCECODE,
                                                      A.UNITRATE, A.M_CONTID, A.SOURCECODE_NAME, A.CONTID_NAME, A.BASE_UNIT, A.WARBAK, A.PYM_WARBAK, 
                                                      A.M_AGENNO, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS_SUB, A.CASENO,
                                                       SUM(A.G34_MAX_APPQTY) AS G34_MAX_APPQTY ,SUM(A.EST_APPQTY ) AS EST_APPQTY 
                                               FROM CHK_DATA A
                                               GROUP BY A.WH_NAME, A.MMCODE, A.CHK_WH_KIND, A.CHK_WH_GRADE, A.CHK_YM,A.CONT_PRICE, A.DISC_CPRICE,
                                                        A.PYM_CONT_PRICE, A.PYM_DISC_CPRICE, A.SOURCECODE, A.PYM_SOURCECODE, A.UNITRATE, A.M_CONTID, A.CASENO,
                                                        A.SOURCECODE_NAME, A.CONTID_NAME, A.BASE_UNIT, A.WARBAK, A.PYM_WARBAK, A.M_AGENNO, A.MMNAME_C, 
                                                        A.MMNAME_E, A.MAT_CLASS_SUB, A.CHK_TIME )

                                SELECT E.WH_NO || ' ' || WH_NAME(E.WH_NO) F1 ,--盤存單位
                                       E.MMCODE F2 ,--藥材代碼
                                       E.M_AGENNO F3 ,--廠商代碼
                                       E.MMNAME_C F4 ,--藥材名稱
                                       E.MMNAME_E F61 ,--英文品名
                                       E.BASE_UNIT F5 ,--藥材單位
                                       E.CONT_PRICE F6 ,--單價
                                       E.DISC_CPRICE F7 ,--優惠後單價
                                       E.PYM_CONT_PRICE F8 ,--上月單價
                                       E.PYM_DISC_CPRICE F9 ,--上月優惠後單價
                                       E.UNITRATE F10 ,--包裝量
                                       SUM({1}) F11 ,--上月結存
                                       SUM(NVL(E.APL_INQTY_1, 0)) F12_1 ,--進貨入庫
                                       SUM(NVL(E.APL_INQTY_2, 0)) F12_2 ,--撥發入庫
                                       SUM(E.APL_OUTQTY) F13 ,--撥發出庫
                                       SUM(E.TRN_INQTY) F14 ,--調撥入庫
                                       SUM(E.TRN_OUTQTY) F15 ,--調撥出庫
                                       SUM(E.ADJ_INQTY) F16 ,--調帳入庫
                                       SUM(E.ADJ_OUTQTY) F17 ,--調帳出庫
                                       SUM(E.BAK_INQTY) F18 ,--繳回入庫
                                       SUM(E.BAK_OUTQTY) F19 ,--繳回出庫
                                       SUM(E.REJ_OUTQTY) F20 ,--退貨量
                                       SUM(E.DIS_OUTQTY) F21 ,--報廢量
                                       SUM(E.EXG_INQTY) F22 ,--換貨入庫
                                       SUM(E.EXG_OUTQTY) F23 ,--換貨出庫
                                       0 AS F24 ,--軍方消耗
                                       SUM((CASE WHEN (E.CHK_WH_GRADE = '1') THEN 0 
                                             WHEN (E.WH_NAME LIKE '%供應中心%') THEN 0
                                             WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN E.ALTERED_USE_QTY
                                             ELSE E.USE_QTY_AF_CHK
                                        END)) F25 ,--民眾消耗
                                       SUM((CASE WHEN (E.CHK_WH_GRADE = '1') THEN 0 
                                             WHEN (E.WH_NAME LIKE '%供應中心%') THEN 0
                                             WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN E.ALTERED_USE_QTY
                                             ELSE E.USE_QTY_AF_CHK
                                        END) )F26 ,--本月總消耗
                                       SUM( ( {4} ) ) F27 ,--應有結存
                                       SUM( {3} ) F28 ,--盤存量
                                       SUM( {3} ) F29 ,--本月結存
                                       SUM((CASE WHEN E.PYM_SOURCECODE = 'C'
                                             THEN {1}
                                             ELSE 0
                                        END)) F30 ,--上月寄庫藥品買斷結存
                                       SUM((CASE WHEN E.SOURCECODE = 'C'
                                             THEN  {3} 
                                             ELSE 0
                                        END)) F31 ,--本月寄庫藥品買斷結存
                                       SUM(E.INVENTORY) F32 ,--差異量
                                       E.CONT_PRICE * SUM( {3} ) F33 ,--結存金額
                                       E.DISC_CPRICE * SUM( {3} ) F34 ,--優惠後結存金額
                                       SUM(E.INVENTORY) * E.CONT_PRICE F35 ,--差異金額
                                       SUM(E.INVENTORY) * E.DISC_CPRICE F36 ,--優惠後差異金額
                                       get_param('MI_MAST', 'MAT_CLASS_SUB', E.MAT_CLASS_SUB) F37 ,--藥材類別
                                       E.CONTID_NAME F38 ,--是否合約
                                       E.SOURCECODE_NAME F39 ,--買斷寄庫
                                       E.WARBAK F40 ,--是否戰備
                                       SUM(E.WAR_QTY) F41 ,--戰備存量
                                       ROUND((CASE WHEN SUM((E.ORI_USE_QTY + (CASE WHEN (E.GAP <= 0) THEN (E.STORE_QTYC - E.CHK_QTY) 
                                                                               ELSE 0 
                                                                          END))) = 0  
                                                   THEN 0
                                                   ELSE (SUM(E.CHK_QTY {2} ) / SUM((E.ORI_USE_QTY + (CASE WHEN (E.GAP <= 0) THEN (E.STORE_QTYC - E.CHK_QTY) 
                                                                                            ELSE 0 
                                                                                       END)))) 
                                              END), 2) F42 ,--期末比
                                       0 F43 ,--贈品數量
                                       SUM(E.PYM_WAR_QTY) F44 ,--上月戰備量
                                       E.PYM_WARBAK F45 ,--上月是否戰備
                                       (CASE WHEN SUM((E.PYM_INV_QTY - E.PYM_WAR_QTY)) < 0 THEN 0 
                                             ELSE SUM(E.PYM_INV_QTY - E.PYM_WAR_QTY)
                                        END) F46 ,--不含戰備上月結存
                                       E.DISC_CPRICE - E.PYM_DISC_CPRICE F47 ,--單價差額
                                       (CASE WHEN SUM((E.PYM_INV_QTY - E.PYM_WAR_QTY)) < 0 THEN 0 
                                             ELSE SUM(E.PYM_INV_QTY - E.PYM_WAR_QTY)
                                        END) * (E.DISC_CPRICE - E.PYM_DISC_CPRICE) F48 ,--不含戰備上月結存價差
                                       (CASE WHEN SUM(E.WAR_QTY) = 0 THEN SUM(E.CHK_QTY)
                                             WHEN SUM(E.CHK_QTY) <= SUM(E.WAR_QTY) THEN SUM(E.CHK_QTY) * (E.DISC_CPRICE - E.PYM_DISC_CPRICE)
                                             ELSE SUM((E.CHK_QTY - E.WAR_QTY)) * (E.DISC_CPRICE - E.PYM_DISC_CPRICE)
                                        END) F49 ,--戰備本月價差
                                       SUM((CASE WHEN E.CHK_QTY <= E.WAR_QTY THEN 0
                                             ELSE E.CHK_QTY - E.WAR_QTY
                                        END)) F50 ,--不含戰備本月結存
                                       SUM((case when E.WAR_QTY > e.chk_qty then 0 
                                           else
                                            (
                                                  (CASE WHEN (E.CHK_WH_GRADE = '1') THEN E.STORE_QTYC
                                                     WHEN (E.WH_NAME LIKE '%供應中心%') THEN E.STORE_QTYC
                                                     WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN (
                                                          (e.pym_inv_qty -- 上月庫存數量 
                                                                      +e.apl_inqty   -- 月結入庫總量 
                                                                      -e.apl_outqty  -- 月結撥發總量 
                                                                      +e.trn_inqty   -- 月結調撥入總量 
                                                                      -e.trn_outqty  -- 月結調撥入總量 
                                                                      +e.adj_inqty   -- 月結調帳入總量 
                                                                      -e.adj_outqty  -- 月結調帳出總量 
                                                                      +e.bak_inqty   -- 月結繳回入庫總量 
                                                                      -e.bak_outqty  -- 月結繳回出庫總量 
                                                                      -e.rej_outqty  -- 月結退貨總量 
                                                                      -e.dis_outqty  -- 月結報廢總量
                                                                      +e.exg_inqty   -- 月結換貨入庫總量 
                                                                      -e.exg_outqty  -- 月結換貨出庫總量 
                                                                      -nvl( 
                                                                          nvl(e.altered_use_qty, e.ori_use_qty), -- 調整消耗(只有藥局可以用), 月結耗用量=醫令扣庫 
                                                                          0 
                                                                      ) )
                                                          )
                                                     ELSE E.STORE_QTYC - (CASE WHEN E.CHK_TIME IS NOT NULL THEN E.USE_QTY_AF_CHK
                                                                               ELSE E.ORI_USE_QTY
                                                                          END)
                                                END) - nvl(e.war_qty, 0)
                                           ) 
                                         end)) F51 ,--不含戰備本月應有結存量
                                       SUM((CASE WHEN E.WAR_QTY = 0 then E.CHK_QTY
                                                 WHEN E.CHK_QTY <= E.WAR_QTY THEN 0
                                             ELSE E.CHK_QTY - E.WAR_QTY
                                        END)) F52 ,--不含戰備本月盤存量
                                       SUM((case when E.CHK_WH_GRADE='1' then (E.APL_INQTY - 0) else 0 end)) F53 ,--不含贈品本月進貨
                                       --E.G34_MAX_APPQTY F54 ,--單位申請基準量
                                       --E.EST_APPQTY F55 ,--下月預計申請量
                                       SUM(E.BAK_INQTY )F56 ,--退料入庫
                                       SUM(E.BAK_OUTQTY) F57 ,--退料出庫
                                       SUM((CASE WHEN E.DISC_CPRICE >= 5000 THEN (CASE WHEN (E.CHK_WH_GRADE = '1') THEN 0 
                                                                                   WHEN (E.WH_NAME LIKE '%供應中心%') THEN 0
                                                                                   WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN E.ALTERED_USE_QTY
                                                                                   ELSE E.USE_QTY_AF_CHK
                                                                              END)
                                             ELSE 0 
                                        END)) F58, --本月單價5000元以上總消耗
                                       SUM((CASE WHEN E.DISC_CPRICE < 5000 THEN (CASE WHEN (E.CHK_WH_GRADE = '1') THEN 0 
                                                                                  WHEN (E.WH_NAME LIKE '%供應中心%') THEN 0
                                                                                  WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN E.ALTERED_USE_QTY
                                                                                  ELSE E.USE_QTY_AF_CHK
                                                                             END)
                                             ELSE 0 
                                        END)) F59  ,--本月單價未滿5000元總消耗
                                       E.CASENO F60 --合約案號
                              FROM INVMON_DATA E WHERE 1 = 1
                              group by E.WH_NO, E.MMCODE, E.M_AGENNO, E.MMNAME_C, E.MMNAME_E,
                                       E.BASE_UNIT, E.CONT_PRICE , E.DISC_CPRICE, E.PYM_CONT_PRICE, 
                                       E.PYM_DISC_CPRICE, E.UNITRATE,
                                       --E.CHK_WH_GRADE, E.WH_NAME, E.CHK_WH_KIND,
                                        E.PYM_SOURCECODE, 
                                       E.MAT_CLASS_SUB, E.CONTID_NAME, E.SOURCECODE_NAME,
                                       E.WARBAK,E.PYM_WARBAK,
                                       --E.G34_MAX_APPQTY, 
                                       --E.EST_APPQTY, 
                                       E.CASENO
                              order by E.mmcode
            "
                 , p1 == "3" ? "全院藥局" : "全院"
                 , p20 == "false" ? "E.PYM_INV_QTY" : @"(case when E.PYM_WAR_QTY = 0 then E.PYM_INV_QTY when E.PYM_INV_QTY <= E.PYM_WAR_QTY then 0 else E.PYM_INV_QTY - E.PYM_WAR_QTY end)"
                 , p20 == "false" ? string.Empty : " - nvl(E.WAR_QTY, 0)"
                 , p20 == "false" ? "E.CHK_QTY" : @"(case when E.WAR_QTY = 0 then E.CHK_QTY when E.CHK_QTY <= E.WAR_QTY then 0 else E.CHK_QTY - E.WAR_QTY end)"
                 , temp
                 );
            }
            #endregion

            p.Add(":USER_NAME", user);
            p.Add(":SET_YM", p0); //月結年月
            p.Add(":RLNO_TEXT", p1); //盤存單位
            p.Add(":CHK_WH_KIND_TEXT", p2); //庫房類別
            p.Add(":CHK_WH_NO_TEXT", p3); //庫房代碼
            p.Add(":INID_TEXT", p4); //責任中心
            p.Add(":MAT_CLASS_SUB_TEXT", p5); //藥材類別
            p.Add(":MMCODE_TEXT", p6); //藥材代碼
            p.Add(":M_CONTID_TEXT", p7); //是否合約
            p.Add(":SOURCECODE_TEXT", p8); //買斷寄庫
            p.Add(":WARBAK_TEXT", p9); //是否戰備
            p.Add(":E_RESTRICTCODE_TEXT", p10); //管制品項
            p.Add(":COMMON_TEXT", p11); //是否常用品項
            p.Add(":FASTDRUG_TEXT", p12); //急救品項
            p.Add(":DRUGKIND_TEXT", p13); //中西藥別
            p.Add(":TOUCHCASE_TEXT", p14); //合約類別
            p.Add(":ORDERKIND_TEXT", p15); //採購類別
            p.Add(":SPDRUG_TEXT", p16); //特殊品項

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CE0044M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<CE0044D> GetAllD(string p0, string p1, string p2, string p3, string p4, string p5, string p6, string p7, string p8, string p9, string p10, 
                                            string p11, string p12, string p13, string p14, string p15, string p16, string p17, string p18, string p19, string user,
                                            string p20, string p21,
                                            int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = "";
            //應有結存
            string temp = @"(CASE WHEN (E.CHK_WH_GRADE = '1') THEN E.STORE_QTYC
                                               WHEN (E.WH_NAME LIKE '%供應中心%') THEN E.STORE_QTYC
                                               WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN (
                                                    (e.pym_inv_qty -- 上月庫存數量 
                                                                +e.apl_inqty   -- 月結入庫總量 
                                                                -e.apl_outqty  -- 月結撥發總量 
                                                                +e.trn_inqty   -- 月結調撥入總量 
                                                                -e.trn_outqty  -- 月結調撥入總量 
                                                                +e.adj_inqty   -- 月結調帳入總量 
                                                                -e.adj_outqty  -- 月結調帳出總量 
                                                                +e.bak_inqty   -- 月結繳回入庫總量 
                                                                -e.bak_outqty  -- 月結繳回出庫總量 
                                                                -e.rej_outqty  -- 月結退貨總量 
                                                                -e.dis_outqty  -- 月結報廢總量
                                                                +e.exg_inqty   -- 月結換貨入庫總量 
                                                                -e.exg_outqty  -- 月結換貨出庫總量 
                                                                -nvl( 
                                                                    nvl(e.altered_use_qty, e.ori_use_qty), -- 調整消耗(只有藥局可以用), 月結耗用量=醫令扣庫 
                                                                    0 
                                                                ) )
                                                    )
                                               ELSE E.STORE_QTYC - (CASE WHEN E.CHK_TIME IS NOT NULL THEN E.USE_QTY_AF_CHK
                                                                         ELSE E.ORI_USE_QTY
                                                                    END)
                                          END) ";
            if (p20 == "true")
            {
                temp = string.Format(@"
                    (case when E.WAR_QTY > e.chk_qty then 0 
                          else
                            ( {0} - nvl(E.war_qty, 0) )
                    end )
                ", temp);
            }

            #region
            sql = @"         WITH WH_NOS AS ( SELECT A.WH_NO, WH_KIND
                                              FROM MI_WHID A, MI_WHMAST B
                                              WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO)),
                           MAT_WH_NO_ALL AS ( SELECT WH_NO, WH_KIND
                                              FROM MI_WHMAST
                                              WHERE WH_KIND = '1'
                                              AND EXISTS (SELECT 1
                                                          FROM UR_UIR
                                                          WHERE TUSER = :USER_NAME
                                                          AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14', 'MMSpl_14'))),
                           MED_WH_NO_ALL AS ( SELECT WH_NO, WH_KIND
                                              FROM MI_WHMAST
                                              WHERE WH_KIND = '0'
                                              AND EXISTS (SELECT 1
                                                          FROM UR_UIR
                                                          WHERE TUSER = :USER_NAME
                                                          AND RLNO IN ('ADMG', 'ADMG_14', 'MED_14', 'PHR_14'))),
                                   INIDS AS ( SELECT C.INID
                                              FROM MI_WHID A, MI_WHMAST B, UR_INID C
                                              WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO)
                                              AND B.INID = C.INID), 
                                INID_ALL AS ( SELECT B.INID
                                              FROM MI_WHMAST A, UR_INID B
                                              WHERE EXISTS (SELECT 1
                                                            FROM UR_UIR
                                                            WHERE TUSER = :USER_NAME
                                                            AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14', 'MED_14', 'MMSpl_14'))), 
                             MAT_WH_KIND AS ( SELECT DISTINCT 1 AS WH_KIND, '衛材' AS TEXT
                                              FROM MI_WHID A, MI_WHMAST B
                                              WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO AND B.WH_KIND = '1')
                                              OR EXISTS (SELECT 1
                                                         FROM UR_UIR WHERE TUSER = :USER_NAME
                                                         AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14', 'MMSpl_14'))),
                             MED_WH_KIND AS ( SELECT DISTINCT 0 AS WH_KIND, '藥品' AS TEXT
                                              FROM MI_WHID A, MI_WHMAST B
                                              WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO AND B.WH_KIND = '0')
                                              OR EXISTS (SELECT 1
                                                         FROM UR_UIR
                                                         WHERE TUSER = :USER_NAME
                                                         AND RLNO IN ('ADMG', 'ADMG_14', 'MED_14', 'PHR_14'))),
                                CHK_DATA AS ( SELECT B.CHK_WH_NO, B.CHK_WH_KIND, B.CHK_WH_GRADE, B.CHK_YM, A.MMCODE, A.CHK_QTY, A.STORE_QTYC,
                                                     NVL((A.CHK_QTY - A.STORE_QTYC), 0) GAP, NVL(A.WAR_QTY, 0) AS WAR_QTY, A.PYM_WAR_QTY AS PYM_WAR_QTY,
                                                     LAST_INVMON(:SET_YM, B.CHK_WH_NO, A.MMCODE) AS PYM_INV_QTY, C.M_CONTPRICE AS CONT_PRICE, C.DISC_CPRICE, 
                                                     NVL(D.M_CONTPRICE, 0) AS PYM_CONT_PRICE, NVL(D.DISC_CPRICE, 0) AS PYM_DISC_CPRICE, C.E_SOURCECODE AS SOURCECODE, 
                                                     NVL(D.E_SOURCECODE, 'P') AS PYM_SOURCECODE, C.UNITRATE, C.M_CONTID, WH_NAME(B.CHK_WH_NO) AS WH_NAME,  
                                                     GET_PARAM('MI_MAST','E_SOURCECODE', C.E_SOURCECODE) AS SOURCECODE_NAME,  
                                                     GET_PARAM('MI_MAST','M_CONTID', C.M_CONTID) AS CONTID_NAME, C.BASE_UNIT,
                                                     NVL(A.APL_INQTY, 0) APL_INQTY , NVL(A.APL_OUTQTY, 0) APL_OUTQTY, NVL(A.TRN_INQTY, 0) TRN_INQTY, 
                                                     NVL(A.TRN_OUTQTY, 0) TRN_OUTQTY, NVL(A.ADJ_INQTY, 0) ADJ_INQTY, NVL(A.ADJ_OUTQTY, 0) ADJ_OUTQTY,
                                                     NVL(A.BAK_INQTY, 0) BAK_INQTY, NVL(A.BAK_OUTQTY, 0) BAK_OUTQTY, NVL(A.REJ_OUTQTY, 0) REJ_OUTQTY, 
                                                     NVL(A.DIS_OUTQTY, 0) DIS_OUTQTY, NVL(A.EXG_INQTY, 0) EXG_INQTY, NVL(A.EXG_OUTQTY, 0) EXG_OUTQTY,
                                                     NVL(A.USE_QTY, 0) USE_QTY, NVL(A.USE_QTY_AF_CHK, 0) USE_QTY_AF_CHK, NVL(INVENTORY, 0) INVENTORY, 
                                                     NVL(A.MIL_USE_QTY, 0) MIL_USE_QTY, NVL(A.CIVIL_USE_QTY, 0) CIVIL_USE_QTY, NVL(A.ALTERED_USE_QTY, 0) ALTERED_USE_QTY, 
                                                     A.CHK_TIME,
                                                     C.WARBAK, NVL(D.WARBAK, ' ') AS PYM_WARBAK, C.M_AGENNO, C.MMNAME_C, C.MMNAME_E, C.MAT_CLASS_SUB
                                              FROM CHK_MAST B, CHK_DETAIL A, MI_MAST_MON C
                                                   LEFT JOIN MI_MAST_MON D ON (D.MMCODE = C.MMCODE AND D.DATA_YM = TWN_PYM(C.DATA_YM))
                                              WHERE B.CHK_YM = :SET_YM
                                              AND B.CHK_NO = A.CHK_NO
                                              AND A.MMCODE= C.MMCODE
                                              AND B.CHK_YM = C.DATA_YM
                                              AND B.CHK_LEVEL = '1'
                                              AND B.CHK_WH_NO IN (SELECT WH_NO FROM WH_NOS 
                                                                  UNION  
                                                                  SELECT WH_NO FROM MAT_WH_NO_ALL  
                                                                  UNION  
                                                                  SELECT WH_NO FROM MED_WH_NO_ALL)
                                              AND B.CHK_WH_KIND IN (SELECT WH_KIND FROM MAT_WH_KIND  
                                                                    UNION  
                                                                    SELECT WH_KIND FROM MED_WH_KIND)
                                              AND (EXISTS (SELECT 1  
                                                           FROM MI_WHMAST A, INIDS B  
                                                           WHERE A.WH_NO = B.CHK_WH_NO  
                                                           AND A.INID = B.INID)  
                                                    OR EXISTS (SELECT 1   
                                                               FROM MI_WHMAST A, INID_ALL B   
                                                               WHERE A.WH_NO = B.CHK_WH_NO   
                                                               AND A.INID = B.INID) ) ";
            if (p17 == "true")
            {
                sql += @"                     AND A.INVENTORY <> 0";
            }
            if (p21 == "true")
            {
                sql += @"                  AND (A.WAR_QTY <> 0 and a.chk_qty < A.WAR_QTY)";
            }

            if (p18 == "true")
            {
                sql += @"and (
exists (select 1 from mi_winvmon
  where mmcode=a.mmcode
    and DATA_YM>=
        substr(twn_date(ADD_MONTHS(twn_todate((select max(set_ym) from mi_mnset where set_status='C')||'01'),-5)),1,5)
    and (APL_INQTY>0 or TRN_INQTY>0 or USE_QTY>0)
    and wh_no=a.wh_no)
or 
exists (select 1 from mi_winvmon
  where mmcode=a.mmcode
    and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
    and INV_QTY<>0
    and wh_no=a.wh_no)
or
exists (select 1 from mi_winvmon b
  where mmcode=a.mmcode
    and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
    and INV_QTY=0
    and wh_no=a.wh_no
    and exists(select 1 from mi_mast_mon where mmcode=b.mmcode
          and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
          and CANCEL_ID='N')
  )
)";
            }

            if (p19 == "true")
            {
                sql += @"
                    and (
                        NVL(a.PYM_INV_QTY,0) > 0
                        or
                        (NVL(a.PYM_INV_QTY,0) = 0 and not (a.apl_inqty = 0                       
                                                   and a.trn_inqty = 0        
                                                   and a.trn_outqty = 0        
                                                   and a.adj_inqty = 0        
                                                   and a.adj_outqty = 0        
                                                   and a.bak_outqty = 0
                                                    and a.bak_inqty=0
                                                    and nvl(nvl(a.altered_use_qty, a.use_qty),0)=0
                                                )
                        )
                    )
                ";
            }

            if (p1 == "1")//若盤存單位 選 各請領單位
            {
                sql += @"                     AND B.CHK_WH_GRADE <> '1' 
							                  AND B.CHK_WH_KIND = NVL(TRIM(:CHK_WH_KIND_TEXT), B.CHK_WH_KIND)
							                  AND B.CHK_WH_NO = NVL(TRIM(:CHK_WH_NO_TEXT), B.CHK_WH_NO)
							                  AND B.CHK_WH_NO IN (SELECT WH_NO 
												                   FROM MI_WHMAST 
												                   WHERE INID = NVL(TRIM(:INID_TEXT), INID)) ";
            }
            else if (p1 == "2")//若盤存單位 選 藥材庫
            {
                sql += @"                     AND B.CHK_WH_NO IN (WHNO_MM1, WHNO_ME1)";
            }
            else if (p1 == "5")//若盤存單位 選 各單位消耗結存明細
            {
                sql += @"                     AND B.CHK_WH_GRADE <> '1'  ";
            }
            else if (p1 == "3")//若盤存單位 選 全部藥局
            {
                sql += @"                     AND B.CHK_WH_GRADE ='2' and B.CHK_WH_KIND='0'  ";
            }

            sql += @"                         AND A.MMCODE = NVL(TRIM(:MMCODE_TEXT), A.MMCODE) 
                                              AND C.M_CONTID = NVL(TRIM(:M_CONTID_TEXT), C.M_CONTID) 
                                              AND C.E_SOURCECODE = NVL(TRIM(:SOURCECODE_TEXT), C.E_SOURCECODE)
            ";
            if (p5 == "A")//若藥材類別選A
            {
                sql += @"                     AND C.MAT_CLASS = '01' ";
            }
            else if (p5 == "B")//若藥材類別選B
            {
                sql += @"                     AND C.MAT_CLASS = '02' ";
            }
            else if (p5 != "A" && p5 != "B" && p5.Trim() != "")//若藥材類別選其他
            {
                sql += @"                     AND C.MAT_CLASS_SUB = NVL(TRIM(:MAT_CLASS_SUB_TEXT), C.MAT_CLASS_SUB) ";
            }

            sql += string.Format(@"    
                                              AND C.WARBAK = NVL(TRIM(:WARBAK_TEXT), C.WARBAK)
                                              AND C.E_RESTRICTCODE = NVL(TRIM(:E_RESTRICTCODE_TEXT), C.E_RESTRICTCODE)
                                              AND C.COMMON = NVL(TRIM(:COMMON_TEXT), C.COMMON)
                                              AND C.FASTDRUG = NVL(TRIM(:FASTDRUG_TEXT), C.FASTDRUG)
                                              AND C.DRUGKIND = NVL(TRIM(:DRUGKIND_TEXT), C.DRUGKIND)
                                              AND C.TOUCHCASE = NVL(TRIM(:TOUCHCASE_TEXT), C.TOUCHCASE)
                                              AND C.ORDERKIND = NVL(TRIM(:ORDERKIND_TEXT), C.ORDERKIND)
                                              AND C.SPDRUG = NVL(TRIM(:SPDRUG_TEXT), C.SPDRUG)), 
                             INVMON_DATA AS ( SELECT '金額' AS WH_NO, A.WH_NAME, A.MMCODE, SUM(NVL(A.STORE_QTYC, 0)) AS ORI_INV_QTY, 
                                                      SUM(A.APL_INQTY) as APL_INQTY,
                                                      (CASE WHEN A.CHK_WH_GRADE = '1' THEN SUM(A.APL_INQTY) else 0 END) AS APL_INQTY_1,
                                                      (CASE WHEN A.CHK_WH_GRADE != '1' THEN SUM(A.APL_INQTY) else 0 END) AS APL_INQTY_2,
                                                      SUM(A.APL_OUTQTY) AS APL_OUTQTY, SUM(A.TRN_INQTY) AS TRN_INQTY, 
                                                      SUM(A.TRN_OUTQTY) AS TRN_OUTQTY, SUM(A.ADJ_INQTY) AS ADJ_INQTY, SUM(A.ADJ_OUTQTY) AS ADJ_OUTQTY,
                                                      SUM(A.BAK_INQTY) AS BAK_INQTY, SUM(A.BAK_OUTQTY) AS BAK_OUTQTY, SUM(A.REJ_OUTQTY) AS REJ_OUTQTY, 
                                                      SUM(A.DIS_OUTQTY) AS DIS_OUTQTY, SUM(A.EXG_INQTY) AS EXG_INQTY, SUM(A.EXG_OUTQTY) AS EXG_OUTQTY,
                                                      SUM(NVL(A.USE_QTY, 0)) AS ORI_USE_QTY, SUM(NVL(A.USE_QTY_AF_CHK, 0)) AS USE_QTY_AF_CHK, 
                                                      SUM(NVL(A.INVENTORY, 0)) AS INVENTORY, SUM(NVL(A.MIL_USE_QTY, 0)) AS MIL_USE_QTY, 
                                                      SUM(NVL(A.CIVIL_USE_QTY, 0)) AS CIVIL_USE_QTY,
                                                      SUM(NVL(A.ALTERED_USE_QTY, 0)) AS ALTERED_USE_QTY, A.CHK_TIME,
                                                      A.CHK_WH_KIND, A.CHK_WH_GRADE,A.CHK_YM, 
                                                      SUM(A.CHK_QTY) AS CHK_QTY, SUM(A.STORE_QTYC) AS STORE_QTYC, SUM(A.GAP) AS GAP, 
                                                      SUM(A.WAR_QTY) AS WAR_QTY, SUM(A.PYM_WAR_QTY) AS PYM_WAR_QTY, SUM(A.PYM_INV_QTY) AS PYM_INV_QTY, 
                                                      A.CONT_PRICE, A.DISC_CPRICE, A.PYM_CONT_PRICE, A.PYM_DISC_CPRICE, A.SOURCECODE, A.PYM_SOURCECODE,
                                                      A.UNITRATE, A.M_CONTID, A.SOURCECODE_NAME, A.CONTID_NAME, A.BASE_UNIT, A.WARBAK, A.PYM_WARBAK, 
                                                      A.M_AGENNO, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS_SUB
                                               FROM CHK_DATA A
                                               GROUP BY A.WH_NAME, A.MMCODE, A.CHK_WH_KIND, A.CHK_WH_GRADE, A.CHK_YM,A.CONT_PRICE, A.DISC_CPRICE,
                                                        A.PYM_CONT_PRICE, A.PYM_DISC_CPRICE, A.SOURCECODE, A.PYM_SOURCECODE, A.UNITRATE, A.M_CONTID,
                                                        A.SOURCECODE_NAME, A.CONTID_NAME, A.BASE_UNIT, A.WARBAK, A.PYM_WARBAK, A.M_AGENNO, A.MMNAME_C, 
                                                        A.MMNAME_E, A.MAT_CLASS_SUB, A.CHK_TIME ),
                             DETAIL_DATA AS ( SELECT E.WH_NO 盤存單位, E.MMCODE 藥材代碼, E.M_AGENNO 廠商代碼, E.MMNAME_C 藥材名稱, E.MMNAME_E 英文品名, E.BASE_UNIT 藥材單位,
                                                     E.CONT_PRICE 單價, E.DISC_CPRICE 優惠後單價, E.PYM_CONT_PRICE 上月單價, E.PYM_DISC_CPRICE 上月優惠後單價,
                                                     E.UNITRATE 包裝量, 
                                                     {0} 上月結存,
                                                     E.APL_INQTY_1 進貨, E.APL_INQTY_2 撥發入庫, E.APL_OUTQTY 撥發出庫,
                                                     E.TRN_INQTY 調撥入庫, E.TRN_OUTQTY 調撥出庫, E.ADJ_INQTY 調帳入庫, E.ADJ_OUTQTY 調帳出庫, E.BAK_INQTY 退料入庫,
                                                     E.BAK_OUTQTY 退料出庫, E.REJ_OUTQTY 退貨量, E.DIS_OUTQTY 報廢量, E.EXG_INQTY 換貨入庫, E.EXG_OUTQTY 換貨出庫, 
                                                     0 AS 軍方消耗, e.CHK_WH_KIND, e.CHK_WH_GRADE, e.WH_NAME,
                                                     (CASE WHEN (E.CHK_WH_GRADE = '1') THEN 0 
                                                           WHEN (E.WH_NAME LIKE '%供應中心%') THEN 0
                                                           WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN E.ALTERED_USE_QTY
                                                           ELSE E.USE_QTY_AF_CHK
                                                      END) 民眾消耗,
                                                     (CASE WHEN (E.CHK_WH_GRADE = '1') THEN 0 
                                                           WHEN (E.WH_NAME LIKE '%供應中心%') THEN 0
                                                           WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN E.ALTERED_USE_QTY
                                                           ELSE E.USE_QTY_AF_CHK
                                                      END) 本月總消耗,
                                                     ( {3} ) 應有結存,
                                                     {2} 盤存量,
                                                     {2} 本月結存, 
                                                     (CASE WHEN E.PYM_SOURCECODE = 'C' THEN {0}
                                                           ELSE 0
                                                      END) P_C_QTY, -- 上月寄庫藥品買斷結存,
                                                     (CASE WHEN E.SOURCECODE = 'C' THEN {2}
                                                           ELSE 0
                                                      END) C_C_QTY, --本月寄庫藥品買斷結存, 
                                                     E.INVENTORY 差異量,
                                                     E.CONT_PRICE * {2} 結存金額,
                                                     E.DISC_CPRICE * {2} 優惠後結存金額,
                                                     E.INVENTORY * E.CONT_PRICE 差異金額,
                                                     E.INVENTORY * E.DISC_CPRICE 優惠後差異金額,
                                                     get_param('MI_MAST', 'MAT_CLASS_SUB', E.MAT_CLASS_SUB) 藥材類別,
                                                     E.CONTID_NAME 是否合約,
                                                     E.SOURCECODE_NAME 買斷寄庫,
                                                     E.WARBAK 是否戰備, 
                                                     E.WAR_QTY 戰備存量, 
                                                     ROUND((CASE WHEN (E.ORI_USE_QTY + (CASE WHEN (E.GAP <= 0) THEN (E.STORE_QTYC - E.CHK_QTY) 
                                                                                             ELSE 0 
                                                                                        END)) = 0 
                                                                 THEN 0 
                                                                 ELSE( {2} / (E.ORI_USE_QTY + (CASE WHEN (E.GAP <= 0) THEN (E.STORE_QTYC - E.CHK_QTY) 
                                                                                                         ELSE 0 
                                                                                                    END))) 
                                                            END), 2) 期末比, 
                                                     0 贈品數量, 
                                                     E.PYM_WAR_QTY 上月戰備量, 
                                                     E.PYM_WARBAK 上月是否戰備, 
                                                     (CASE WHEN (E.PYM_INV_QTY - E.PYM_WAR_QTY) < 0 THEN 0 
                                                           ELSE E.PYM_INV_QTY - E.PYM_WAR_QTY 
                                                      END) NO_W_P_I, --不含戰備上月結存, 
                                                     E.DISC_CPRICE - E.PYM_DISC_CPRICE 單價差額, 
                                                     (CASE WHEN (E.PYM_INV_QTY - E.PYM_WAR_QTY) < 0 THEN 0 
                                                           ELSE E.PYM_INV_QTY - E.PYM_WAR_QTY 
                                                      END) * (E.DISC_CPRICE - E.PYM_DISC_CPRICE) NO_W_P_I_DIF, -- 不含戰備上月結存價差,
                                                     (CASE WHEN E.WAR_QTY=0 THEN 0
                                                           WHEN E.CHK_QTY <= E.WAR_QTY THEN E.CHK_QTY * (E.DISC_CPRICE - E.PYM_DISC_CPRICE) 
                                                           ELSE (E.CHK_QTY - E.WAR_QTY) * (E.DISC_CPRICE - E.PYM_DISC_CPRICE) 
                                                      END) 戰備本月價差,
                                                     (CASE when E.WAR_QTY = 0 then E.CHK_QTY
                                                           WHEN E.CHK_QTY <= E.WAR_QTY THEN 0 
                                                           ELSE E.CHK_QTY - E.WAR_QTY 
                                                      END) 不含戰備本月結存,
                                                     (case when E.WAR_QTY > e.chk_qty then 0 
                                                           else
                                                            (
                                                                  (CASE WHEN (E.CHK_WH_GRADE = '1') THEN E.STORE_QTYC
                                                                     WHEN (E.WH_NAME LIKE '%供應中心%') THEN E.STORE_QTYC
                                                                     WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN (
                                                                          (e.pym_inv_qty -- 上月庫存數量 
                                                                                      +e.apl_inqty   -- 月結入庫總量 
                                                                                      -e.apl_outqty  -- 月結撥發總量 
                                                                                      +e.trn_inqty   -- 月結調撥入總量 
                                                                                      -e.trn_outqty  -- 月結調撥入總量 
                                                                                      +e.adj_inqty   -- 月結調帳入總量 
                                                                                      -e.adj_outqty  -- 月結調帳出總量 
                                                                                      +e.bak_inqty   -- 月結繳回入庫總量 
                                                                                      -e.bak_outqty  -- 月結繳回出庫總量 
                                                                                      -e.rej_outqty  -- 月結退貨總量 
                                                                                      -e.dis_outqty  -- 月結報廢總量
                                                                                      +e.exg_inqty   -- 月結換貨入庫總量 
                                                                                      -e.exg_outqty  -- 月結換貨出庫總量 
                                                                                      -nvl( 
                                                                                          nvl(e.altered_use_qty, e.ori_use_qty), -- 調整消耗(只有藥局可以用), 月結耗用量=醫令扣庫 
                                                                                          0 
                                                                                      ) )
                                                                          )
                                                                     ELSE E.STORE_QTYC - (CASE WHEN E.CHK_TIME IS NOT NULL THEN E.USE_QTY_AF_CHK
                                                                                               ELSE E.ORI_USE_QTY
                                                                                          END)
                                                                END) - nvl(e.war_qty, 0)
                                                           ) 
                                                         end) NO_W_C_I, -- 不含戰備本月應有結存量,
                                                     (CASE when E.WAR_QTY = 0 then E.CHK_QTY
                                                           WHEN E.CHK_QTY <= E.WAR_QTY THEN 0 
                                                           ELSE E.CHK_QTY - E.WAR_QTY 
                                                      END) NO_W_C_C, --不含戰備本月盤存量,
                                                     (E.APL_INQTY - 0) 不含贈品本月進貨,
                                                     (CASE WHEN E.DISC_CPRICE >= 5000 THEN (CASE WHEN (E.CHK_WH_GRADE = '1') THEN 0 
                                                                                                 WHEN (E.WH_NAME LIKE '%供應中心%') THEN 0
                                                                                                 WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN E.ALTERED_USE_QTY
                                                                                                 ELSE E.USE_QTY_AF_CHK
                                                                                            END)
                                                           ELSE 0 
                                                      END) PRICE_L_5000_Y, --本月單價5000元以上總消耗
                                                     (CASE WHEN E.DISC_CPRICE < 5000 THEN (CASE WHEN (E.CHK_WH_GRADE = '1') THEN 0 
                                                                                                WHEN (E.WH_NAME LIKE '%供應中心%') THEN 0
                                                                                                WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN E.ALTERED_USE_QTY
                                                                                                ELSE E.USE_QTY_AF_CHK
                                                                                           END)
                                                      ELSE 0 END) PRICE_L_5000_N  --本月單價未滿5000元總消耗
                                              FROM INVMON_DATA E
                                              WHERE 1 = 1) 
                             SELECT SUM(A.上月結存 * A.上月優惠後單價) F1, -- 上月結存
                                    SUM(A.進貨 * A.優惠後單價) AS F2,  --本月進貨
                                    SUM(A.撥發入庫 * A.優惠後單價) AS F29, -- 本月撥發入庫
                                    SUM(A.撥發出庫 * A.優惠後單價) AS F30, -- 本月撥發出庫
                                    SUM(A.退貨量 * A.優惠後單價) AS F3, --本月退貨
                                    SUM(A.贈品數量 * A.優惠後單價) AS F4, --本月贈品
                                    SUM(A.本月總消耗 * A.優惠後單價) AS F5, --消耗金額
                                    SUM(A.軍方消耗 * A.優惠後單價) AS F6, --軍方消耗
                                    SUM(A.民眾消耗 * A.優惠後單價) AS F7, --民眾消耗
                                    SUM((A.退料入庫 - A.退料出庫) * A.優惠後單價) AS F8, --退料金額
                                    SUM((A.調撥入庫 - A.調撥出庫) * A.優惠後單價) AS F9, --調撥金額
                                    SUM((A.換貨入庫 - A.換貨出庫) * A.優惠後單價) AS F10, --換貨金額
                                    SUM(A.報廢量 * A.優惠後單價) AS F11,  --報廢金額
                                    SUM(A.應有結存 * A.優惠後單價) AS F12,  --應有結存
                                    SUM(A.盤存量 * A.優惠後單價) AS F13,  --盤點結存
                                    SUM(A.本月結存 * A.優惠後單價) AS F14, --本月結存
                                    SUM((CASE WHEN A.買斷寄庫 = '買斷' THEN A.本月結存 ELSE 0 END) * A.優惠後單價) AS F15,  --買斷結存
                                    SUM(A.P_C_QTY * A.優惠後單價) AS F16,  --上月寄庫藥品買斷結存(P_C_QTY=上月寄庫藥品買斷結存)
                                    SUM(A.C_C_QTY * A.優惠後單價) AS F17,  --本月寄庫藥品買斷結存(C_C_QTY=本月寄庫藥品買斷結存)
                                    SUM((case when A.戰備存量=0 then 0 else (CASE WHEN A.戰備存量 > 0 AND A.本月結存 > A.戰備存量 THEN A.戰備存量 ELSE A.本月結存 END) end) * A.優惠後單價) AS F18, --戰備金額
                                    SUM((CASE WHEN A.買斷寄庫 = '寄庫' THEN A.本月結存 ELSE 0 END) * A.優惠後單價) AS F19,  --寄庫結存
                                    SUM(A.NO_W_P_I_DIF) AS F20,  --不含戰備上月結存價差
                                    SUM(A.戰備本月價差) AS F21,  --戰備本月價差
                                    0 AS F22,  --折讓金額
                                    (CASE WHEN :RLNO_TEXT = '2' THEN TO_CHAR(SUM(A.進貨 * A.優惠後單價)-SUM(A.退貨量 * A.優惠後單價)) ELSE '' END) F23,  --進貨總金額
                                    SUM((CASE WHEN (   (A.CHK_WH_KIND ='1' AND (A.CHK_WH_GRADE <> '1' OR A.WH_NAME NOT LIKE '%供應中心%') )
                                                            OR (A.CHK_WH_KIND='0' AND A.CHK_WH_GRADE NOT IN ('1','2')) ) THEN 0
                                                        WHEN A.差異金額 > 0 THEN A.差異金額 ELSE 0 END)) F24,  --盤盈金額
                                    SUM((CASE WHEN (   (A.CHK_WH_KIND ='1' AND (A.CHK_WH_GRADE <> '1' OR A.WH_NAME NOT LIKE '%供應中心%') )
                                                            OR (A.CHK_WH_KIND='0' AND A.CHK_WH_GRADE NOT IN ('1','2')) ) THEN 0
                                                      WHEN A.差異金額 < 0 THEN A.差異金額 ELSE 0 END)) F25,  --盤虧金額
                                    SUM((CASE WHEN (   (A.CHK_WH_KIND ='1' AND (A.CHK_WH_GRADE <> '1' OR A.WH_NAME NOT LIKE '%供應中心%') )
                                                            OR (A.CHK_WH_KIND='0' AND A.CHK_WH_GRADE NOT IN ('1','2')) ) THEN 0
                                                      ELSE A.差異金額 END)) F26,  --合計盤盈虧總金額
                                    SUM(A.PRICE_L_5000_Y * A.優惠後單價) AS F27, --本月單價5000元以上消耗金額
                                    SUM(A.PRICE_L_5000_N * A.優惠後單價) AS F28 --本月單價未滿5000元消耗金額
                             FROM DETAIL_DATA A"

                , p20 == "false" ? "E.PYM_INV_QTY" : @"(case when E.PYM_WAR_QTY = 0 then E.PYM_INV_QTY when E.PYM_INV_QTY <= E.PYM_WAR_QTY then 0 else E.PYM_INV_QTY - E.PYM_WAR_QTY end)"
                , p20 == "false" ? string.Empty : " - nvl(E.WAR_QTY, 0)"
                , p20 == "false" ? "E.CHK_QTY" : @"(case when E.WAR_QTY = 0 then E.CHK_QTY when E.CHK_QTY <= E.WAR_QTY then 0 else E.CHK_QTY - E.WAR_QTY end)"
                , temp
            );
            #endregion

            p.Add(":USER_NAME", user);
            p.Add(":SET_YM", p0); //月結年月
            p.Add(":RLNO_TEXT", p1); //盤存單位
            p.Add(":CHK_WH_KIND_TEXT", p2); //庫房類別
            p.Add(":CHK_WH_NO_TEXT", p3); //庫房代碼
            p.Add(":INID_TEXT", p4); //責任中心
            p.Add(":MAT_CLASS_SUB_TEXT", p5); //藥材類別
            p.Add(":MMCODE_TEXT", p6); //藥材代碼
            p.Add(":M_CONTID_TEXT", p7); //是否合約
            p.Add(":SOURCECODE_TEXT", p8); //買斷寄庫
            p.Add(":WARBAK_TEXT", p9); //是否戰備
            p.Add(":E_RESTRICTCODE_TEXT", p10); //管制品項
            p.Add(":COMMON_TEXT", p11); //是否常用品項
            p.Add(":FASTDRUG_TEXT", p12); //急救品項
            p.Add(":DRUGKIND_TEXT", p13); //中西藥別
            p.Add(":TOUCHCASE_TEXT", p14); //合約類別
            p.Add(":ORDERKIND_TEXT", p15); //採購類別
            p.Add(":SPDRUG_TEXT", p16); //特殊品項

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CE0044D>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        //匯出
        public DataTable GetExcel(string p0, string p1, string p2, string p3, string p4, string p5, string p6, string p7, string p8, string p9, string p10, 
                                  string p11, string p12, string p13, string p14, string p15, string p16, string p17, string p18, string p19, 
                                  string HospCode, string user
                                  ,string p20, string p21)
        {
            var p = new DynamicParameters();
            var sql = "";
            //應有結存
            string temp = @"(CASE WHEN (E.CHK_WH_GRADE = '1') THEN E.STORE_QTYC
                                               WHEN (E.WH_NAME LIKE '%供應中心%') THEN E.STORE_QTYC
                                               WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN (
                                                    (e.pym_inv_qty -- 上月庫存數量 
                                                                +e.apl_inqty   -- 月結入庫總量 
                                                                -e.apl_outqty  -- 月結撥發總量 
                                                                +e.trn_inqty   -- 月結調撥入總量 
                                                                -e.trn_outqty  -- 月結調撥入總量 
                                                                +e.adj_inqty   -- 月結調帳入總量 
                                                                -e.adj_outqty  -- 月結調帳出總量 
                                                                +e.bak_inqty   -- 月結繳回入庫總量 
                                                                -e.bak_outqty  -- 月結繳回出庫總量 
                                                                -e.rej_outqty  -- 月結退貨總量 
                                                                -e.dis_outqty  -- 月結報廢總量
                                                                +e.exg_inqty   -- 月結換貨入庫總量 
                                                                -e.exg_outqty  -- 月結換貨出庫總量 
                                                                -nvl( 
                                                                    nvl(e.altered_use_qty, e.ori_use_qty), -- 調整消耗(只有藥局可以用), 月結耗用量=醫令扣庫 
                                                                    0 
                                                                ) )
                                                    )
                                               ELSE E.STORE_QTYC - (CASE WHEN E.CHK_TIME IS NOT NULL THEN E.USE_QTY_AF_CHK
                                                                         ELSE E.ORI_USE_QTY
                                                                    END)
                                          END) ";
            if (p20 == "true")
            {
                temp = string.Format(@"
                    (case when E.WAR_QTY > e.chk_qty then 0 
                          else
                            ( {0} - nvl(E.war_qty, 0) )
                    end )
                ", temp);
            }

            #region
            //盤存單位 選 各請領單位、各單位消耗結存明細、藥材庫
            if (p1 == "1" || p1 == "2" || p1 == "5")
            {
                sql = @"      WITH WH_NOS AS ( SELECT A.WH_NO, WH_KIND 
                                               FROM MI_WHID A, MI_WHMAST B 
                                               WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO)),
                            MAT_WH_NO_ALL AS ( SELECT WH_NO, WH_KIND 
                                               FROM MI_WHMAST 
                                               WHERE WH_KIND = '1' 
                                               AND EXISTS (SELECT 1 
                                                           FROM UR_UIR  
                                                           WHERE TUSER = :USER_NAME
                                               AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14','MMSpl_14'))),
                            MED_WH_NO_ALL AS ( SELECT WH_NO, WH_KIND  
                                               FROM MI_WHMAST  
                                               WHERE WH_KIND = '0' 
                                               AND EXISTS (SELECT 1 FROM UR_UIR  
                                                           WHERE TUSER = :USER_NAME 
                                                           AND RLNO IN ('ADMG', 'ADMG_14', 'MED_14','PHR_14'))),
                                    INIDS AS ( SELECT C.INID  
                                               FROM MI_WHID A, MI_WHMAST B, UR_INID C  
                                               WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO)  
                                               AND B.INID = C.INID), 
                                 INID_ALL AS ( SELECT B.INID  
                                               FROM MI_WHMAST A, UR_INID B 
                                               WHERE EXISTS (SELECT 1  
                                                             FROM UR_UIR 
                                                             WHERE TUSER = :USER_NAME  
                                                             AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14','MED_14','MMSpl_14'))),
                              MAT_WH_KIND AS ( SELECT DISTINCT 1 AS WH_KIND, '衛材' AS TEXT  
                                               FROM MI_WHID A, MI_WHMAST B 
                                               WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO AND B.WH_KIND = '1') 
                                               OR EXISTS (SELECT 1 
                                                          FROM UR_UIR  
                                                          WHERE TUSER = :USER_NAME 
                                                          AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14','MMSpl_14'))),
                              MED_WH_KIND AS ( SELECT DISTINCT 0 AS WH_KIND, '藥品' AS TEXT 
                                               FROM MI_WHID A, MI_WHMAST B 
                                               WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO AND B.WH_KIND = '0') 
                                               OR EXISTS (SELECT 1  
                                                          FROM UR_UIR 
                                                          WHERE TUSER = :USER_NAME 
                                                          AND RLNO IN ('ADMG', 'ADMG_14', 'MED_14','PHR_14'))),
                                 CHK_DATA AS ( SELECT B.CHK_WH_NO, B.CHK_WH_KIND, B.CHK_WH_GRADE, B.CHK_YM, A.MMCODE, A.CHK_QTY, A.STORE_QTYC,
                                                      NVL((A.CHK_QTY - A.STORE_QTYC), 0) GAP, nvl(a.WAR_QTY, 0) AS WAR_QTY, a.PYM_WAR_QTY AS PYM_WAR_QTY,
                                                      LAST_INVMON(:SET_YM, B.CHK_WH_NO, A.MMCODE) AS PYM_INV_QTY, C.m_contprice as cont_price, C.DISC_CPRICE,
                                                      NVL(D.m_contprice, 0) AS PYM_CONT_PRICE, NVL(D.DISC_CPRICE, 0) AS PYM_DISC_CPRICE, C.e_SOURCECODE as sourcecode,
                                                      NVL(D.e_SOURCECODE, 'P') AS PYM_SOURCECODE, C.UNITRATE, C.M_CONTID, WH_NAME(B.CHK_WH_NO) AS WH_NAME,
                                                      GET_PARAM('MI_MAST','E_SOURCECODE', C.e_SOURCECODE) AS SOURCECODE_NAME, GET_PARAM('MI_MAST','M_CONTID',
                                                      C.M_CONTID) AS CONTID_NAME, NVL(C.BASE_UNIT, A.BASE_UNIT) BASE_UNIT,
nvl(a.apl_inqty, 0) apl_inqty , nvl(a.apl_outqty, 0) apl_outqty, nvl(a.TRN_INQTY, 0) TRN_INQTY, 
                                                      nvl(a.TRN_OUTQTY, 0) TRN_OUTQTY, nvl(a.ADJ_INQTY, 0) ADJ_INQTY, nvl(a.ADJ_OUTQTY, 0) ADJ_OUTQTY,
                                                      nvl(a.BAK_INQTY, 0) BAK_INQTY, nvl(a.BAK_OUTQTy, 0) BAK_OUTQTy, nvl(a.REJ_OUTQTY, 0) REJ_OUTQTY, 
                                                      nvl(a.DIS_OUTQTY, 0) DIS_OUTQTY, nvl(a.EXG_INQTy, 0) EXG_INQTy, nvl(a.EXG_OUTQTY, 0) EXG_OUTQTY,
                                                      nvl(a.use_qty, 0) use_qty, nvl(a.use_qty_af_chk, 0) use_qty_af_chk, nvl(inventory, 0) inventory, 
                                                      nvl(a.mil_use_qty, 0) mil_use_qty, nvl(a.civil_use_qty, 0) civil_use_qty, nvl(a.ALTERED_USE_QTY, 0) ALTERED_USE_QTY, 
                                                     a.chk_time, C.CASENO,
                                                      c.WARBAK, nvl(d.WARBAK, ' ') as pym_warbak, c.m_agenno, c.mmname_c, c.mmname_e, c.mat_class_sub,A.g34_max_appqty ,A.est_appqty 
                                               FROM CHK_MAST B, CHK_DETAIL A, MI_MAST_MON c
                                                    left join MI_MAST_MON d on (d.mmcode = c.mmcode and d.data_ym = twn_pym(c.data_ym))
                                               WHERE B.CHK_YM = :SET_YM AND B.CHK_NO = A.CHK_NO
                                               and a.mmcode= c.mmcode and b.chk_ym=c.data_ym
                                               AND B.CHK_LEVEL = '1'
                                               AND B.CHK_WH_NO IN ( SELECT WH_NO FROM WH_NOS 
                                                                    UNION 
                                                                    SELECT WH_NO FROM MAT_WH_NO_ALL 
                                                                    UNION 
                                                                    SELECT WH_NO FROM MED_WH_NO_ALL)
                                               AND B.CHK_WH_KIND IN ( SELECT WH_KIND FROM MAT_WH_KIND 
                                                                      UNION 
                                                                      SELECT WH_KIND FROM MED_WH_KIND)
                                               AND (EXISTS ( SELECT 1  
                                                             FROM MI_WHMAST A, INIDS B  
                                                             WHERE A.WH_NO = B.CHK_WH_NO  
                                                             AND A.INID = B.INID)  
                                                    OR EXISTS (SELECT 1  
                                                               FROM MI_WHMAST A, INID_ALL B  
                                                               WHERE A.WH_NO = B.CHK_WH_NO  
                                                               AND A.INID = B.INID) ) ";

                if (p17 == "true")
                {
                    sql += @" AND A.inventory <> 0";
                }
                if (p21 == "true")
                {
                    sql += @"                  AND (A.WAR_QTY <> 0 and a.chk_qty < A.WAR_QTY)";
                }

                if (p18 == "true")
                {
                    sql += @"and (
exists (select 1 from mi_winvmon
  where mmcode=a.mmcode
    and DATA_YM>=
        substr(twn_date(ADD_MONTHS(twn_todate((select max(set_ym) from mi_mnset where set_status='C')||'01'),-5)),1,5)
    and (APL_INQTY>0 or TRN_INQTY>0 or USE_QTY>0)
    and wh_no=a.wh_no)
or 
exists (select 1 from mi_winvmon
  where mmcode=a.mmcode
    and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
    and INV_QTY<>0
    and wh_no=a.wh_no)
or
exists (select 1 from mi_winvmon b
  where mmcode=a.mmcode
    and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
    and INV_QTY=0
    and wh_no=a.wh_no
    and exists(select 1 from mi_mast_mon where mmcode=b.mmcode
          and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
          and CANCEL_ID='N')
  )
)";
                }

                if (p19 == "true")
                {
                    sql += @"
                    and (
                        NVL(a.PYM_INV_QTY,0) > 0
                        or
                        (NVL(a.PYM_INV_QTY,0) = 0 and not (a.apl_inqty = 0                       
                                                   and a.trn_inqty = 0        
                                                   and a.trn_outqty = 0        
                                                   and a.adj_inqty = 0        
                                                   and a.adj_outqty = 0        
                                                   and a.bak_outqty = 0
                                                    and a.bak_inqty=0
                                                    and nvl(nvl(a.altered_use_qty, a.use_qty),0)=0
                                                )
                        )
                    )
                ";
                }

                if (p1 == "1")//若盤存單位 選 各請領單位
                {
                    sql += @"                  AND B.CHK_WH_GRADE <> '1' 
                                               AND B.CHK_WH_KIND = NVL(TRIM(:CHK_WH_KIND_TEXT),B.CHK_WH_KIND)
                                               AND B.CHK_WH_NO = NVL(TRIM(:CHK_WH_NO_TEXT),B.CHK_WH_NO)
                                               AND B.CHK_WH_NO IN (SELECT WH_NO 
                                                                   FROM MI_WHMAST 
                                                                   WHERE INID = NVL(TRIM(:INID_TEXT),INID)) ";
                }
                else if (p1 == "2")//若盤存單位 選 藥材庫
                {
                    sql += @"                  AND B.CHK_WH_NO IN (WHNO_MM1, WHNO_ME1) ";
                }
                else if (p1 == "5")//若盤存單位 選 各單位消耗結存明細
                {
                    sql += @"                  AND B.CHK_WH_GRADE <> '1'  ";
                }

                sql += @"                      AND A.MMCODE = NVL(TRIM(:MMCODE_TEXT),A.MMCODE) 
                                               AND C.M_CONTID = NVL(TRIM(:M_CONTID_TEXT),C.M_CONTID) 
                                               AND C.e_SOURCECODE = NVL(TRIM(:SOURCECODE_TEXT),C.e_SOURCECODE)
                ";
                if (p5 == "A")//若藥材類別選A
                {
                    sql += @"                   AND C.MAT_CLASS = '01' ";
                }
                else if (p5 == "B")//若藥材類別選B
                {
                    sql += @"                   AND C.MAT_CLASS = '02' ";
                }
                else if (p5 != "A" && p5 != "B" && p5.Trim() != "")//若藥材類別選其他
                {
                    sql += @"                   AND C.MAT_CLASS_SUB = NVL(TRIM(:MAT_CLASS_SUB_TEXT),C.MAT_CLASS_SUB) ";
                }

                sql += string.Format(@"
                                                AND C.WARBAK = NVL(TRIM(:WARBAK_TEXT),C.WARBAK)
                                                AND C.E_RESTRICTCODE = NVL(TRIM(:E_RESTRICTCODE_TEXT),C.E_RESTRICTCODE)
                                                AND C.COMMON = NVL(TRIM(:COMMON_TEXT),C.COMMON)
                                                AND C.FASTDRUG = NVL(TRIM(:FASTDRUG_TEXT),C.FASTDRUG)
                                                AND C.DRUGKIND = NVL(TRIM(:DRUGKIND_TEXT),C.DRUGKIND)
                                                AND C.TOUCHCASE = NVL(TRIM(:TOUCHCASE_TEXT),C.TOUCHCASE)
                                                AND C.ORDERKIND = NVL(TRIM(:ORDERKIND_TEXT),C.ORDERKIND)
                                                AND C.SPDRUG = NVL(TRIM(:SPDRUG_TEXT),C.SPDRUG)), 
                              INVMON_DATA AS ( SELECT A.CHK_WH_NO as WH_NO, A.WH_NAME, A.MMCODE, SUM(NVL(a.store_qtyc, 0)) AS ORI_INV_QTY, 
                                                      SUM(A.APL_INQTY) AS APL_INQTY,
                                                      (CASE WHEN A.CHK_WH_GRADE = '1' THEN SUM(A.APL_INQTY) else 0 END) AS APL_INQTY_1,
                                                      (CASE WHEN A.CHK_WH_GRADE != '1' THEN SUM(A.APL_INQTY) else 0 END) AS APL_INQTY_2,
                                                      SUM(A.APL_OUTQTY) AS APL_OUTQTY, SUM(A.TRN_INQTY) AS TRN_INQTY, 
                                                      SUM(A.TRN_OUTQTY) AS TRN_OUTQTY, SUM(A.ADJ_INQTY) AS ADJ_INQTY, SUM(A.ADJ_OUTQTY) AS ADJ_OUTQTY,
                                                      SUM(A.BAK_INQTY) AS BAK_INQTY, SUM(A.BAK_OUTQTY) AS BAK_OUTQTY, SUM(A.REJ_OUTQTY) AS REJ_OUTQTY, 
                                                      SUM(A.DIS_OUTQTY) AS DIS_OUTQTY, SUM(A.EXG_INQTY) AS EXG_INQTY, SUM(A.EXG_OUTQTY) AS EXG_OUTQTY,
                                                      SUM(NVL(a.use_qty, 0)) AS ORI_USE_QTY, sum(nvl(a.use_qty_af_chk, 0)) as use_qty_af_chk, 
                                                      sum(nvl(a.inventory, 0)) as inventory, sum(nvl(a.mil_use_qty, 0)) as mil_use_qty, 
                                                      sum(nvl(a.civil_use_qty, 0)) as civil_use_qty,
                                                      sum(nvl(a.ALTERED_USE_QTY, 0)) as ALTERED_USE_QTY, a.chk_time,
                                                      A.CHK_WH_KIND, A.CHK_WH_GRADE,A.CHK_YM, 
                                                      SUM(A.CHK_QTY) AS CHK_QTY, SUM(A.STORE_QTYC) AS STORE_QTYC, SUM(A.GAP) AS GAP, 
                                                      SUM(A.WAR_QTY) AS WAR_QTY, SUM(A.PYM_WAR_QTY) AS PYM_WAR_QTY, SUM(A.PYM_INV_QTY) AS PYM_INV_QTY, 
                                                      A.CONT_PRICE, A.DISC_CPRICE, A.PYM_CONT_PRICE, A.PYM_DISC_CPRICE, A.SOURCECODE, A.PYM_SOURCECODE,
                                                      A.UNITRATE, A.M_CONTID, A.SOURCECODE_NAME, A.CONTID_NAME, A.BASE_UNIT, A.WARBAK, A.PYM_WARBAK, 
                                                      A.M_AGENNO, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS_SUB, A.CASENO,
                                                       SUM(A.g34_max_appqty) AS G34_MAX_APPQTY ,SUM(A.est_appqty ) AS EST_APPQTY 
                                               FROM CHK_DATA A
                                               GROUP BY A.chk_WH_NO, A.WH_NAME, A.MMCODE, A.CHK_WH_KIND, A.CHK_WH_GRADE, A.CHK_YM,A.CONT_PRICE, A.DISC_CPRICE,
                                                        A.PYM_CONT_PRICE, A.PYM_DISC_CPRICE, A.SOURCECODE, A.PYM_SOURCECODE, A.UNITRATE, A.M_CONTID, A.CASENO,
                                                        A.SOURCECODE_NAME, A.CONTID_NAME, A.BASE_UNIT, A.WARBAK, A.PYM_WARBAK, A.M_AGENNO, A.MMNAME_C, 
                                                        A.MMNAME_E, A.MAT_CLASS_SUB, a.chk_time )

                              SELECT E.WH_NO|| ' '||WH_NAME(E.WH_NO) 盤存單位, E.MMCODE 藥材代碼, E.M_AGENNO 廠商代碼, E.MMNAME_C 藥材名稱, E.MMNAME_E 英文品名, E.BASE_UNIT 藥材單位, E.CONT_PRICE 單價, 
                                     E.DISC_CPRICE 優惠後單價, E.PYM_CONT_PRICE 上月單價, E.PYM_DISC_CPRICE 上月優惠後單價, E.UNITRATE 包裝量, 
                                     {0} 上月結存, 
                                     E.APL_INQTY_1 進貨, E.APL_INQTY_2 撥發入庫,  E.APL_OUTQTY 撥發出庫, E.TRN_INQTY 調撥入庫, E.TRN_OUTQTY 調撥出庫,
                                     E.ADJ_INQTY 調帳入庫, E.ADJ_OUTQTY 調帳出庫, E.BAK_INQTY 退料入庫, E.BAK_OUTQTY 退料出庫, E.REJ_OUTQTY 退貨量, 
                                     E.DIS_OUTQTY 報廢量, E.EXG_INQTY 換貨入庫, E.EXG_OUTQTY 換貨出庫, 0 AS 軍方消耗,
                                     (case when (e.chk_wh_grade = '1') then 0 
                                           when (e.wh_name like '%供應中心%') then 0
                                           when (e.chk_wh_grade = '2' and e.chk_wh_kind = '0') then e.ALTERED_USE_QTY
                                           else e.USE_QTY_AF_CHK
                                     end) 民眾消耗,
                                     (case when (e.chk_wh_grade = '1') then 0 
                                           when (e.wh_name like '%供應中心%') then 0
                                           when (e.chk_wh_grade = '2' and e.chk_wh_kind = '0') then e.ALTERED_USE_QTY
                                           else e.USE_QTY_AF_CHK
                                     end) 本月總消耗,
                                     ( {3} ) 應有結存,
                                     {2} 盤存量, {2} 本月結存,
                                     (CASE WHEN E.PYM_SOURCECODE = 'C' THEN {0} ELSE 0 END) p_c_qty, -- 上月寄庫藥品買斷結存
                                     (CASE WHEN E.SOURCECODE = 'C' THEN {2} ELSE 0 END) c_c_qty, --本月寄庫藥品買斷結存,
                                     e.inventory 差異量,
                                     E.CONT_PRICE* {2} 結存金額, E.DISC_CPRICE* {2} 優惠後結存金額,
                                     e.inventory * e.cont_price 差異金額,
                                     e.inventory * e.DISC_CPRICE 優惠後差異金額,
                                     get_param('MI_MAST', 'MAT_CLASS_SUB', E.MAT_CLASS_SUB) 藥材類別,
                                     E.CONTID_NAME 是否合約, E.SOURCECODE_NAME 買斷寄庫, E.WARBAK 是否戰備, E.WAR_QTY 戰備存量,
                                     ROUND((CASE WHEN (E.ORI_USE_QTY + (CASE WHEN (E.GAP <= 0) THEN (E.STORE_QTYC - E.CHK_QTY) 
                                                                             ELSE 0 END)) = 0 
                                                 THEN 0
                                                 ELSE ({2} / (E.ORI_USE_QTY + (CASE WHEN (E.GAP <= 0) THEN (E.STORE_QTYC - E.CHK_QTY) 
                                                                                          ELSE 0 END))) END), 2) 期末比,
                                     0 贈品數量, E.PYM_WAR_QTY 上月戰備量, E.PYM_WARBAK 上月是否戰備, 
                                     (CASE WHEN (E.PYM_INV_QTY - E.PYM_WAR_QTY) < 0 THEN 0 
                                           ELSE E.PYM_INV_QTY - E.PYM_WAR_QTY END) no_w_p_i, --不含戰備上月結存, 
                                     E.DISC_CPRICE - E.PYM_DISC_CPRICE 單價差額, 
                                     (CASE WHEN (E.PYM_INV_QTY - E.PYM_WAR_QTY) < 0 THEN 0
                                           ELSE E.PYM_INV_QTY - E.PYM_WAR_QTY END) *(E.DISC_CPRICE - E.PYM_DISC_CPRICE) no_w_p_i_dif, -- 不含戰備上月結存價差,
                                     (CASE WHEN E.WAR_QTY=0 THEN 0
                                           WHEN E.CHK_QTY <= E.WAR_QTY THEN E.CHK_QTY * (E.DISC_CPRICE - E.PYM_DISC_CPRICE)
                                           ELSE (E.CHK_QTY - E.WAR_QTY) * (E.DISC_CPRICE - E.PYM_DISC_CPRICE) END) 戰備本月價差,
                                     (CASE WHEN E.WAR_QTY=0 THEN 0 WHEN E.CHK_QTY <= E.WAR_QTY THEN 0 ELSE E.CHK_QTY - E.WAR_QTY END) 不含戰備本月結存,
                                     (CASE WHEN (E.CHK_WH_GRADE = '1') THEN E.STORE_QTYC
                                           WHEN (E.WH_NAME LIKE '%供應中心%') THEN E.STORE_QTYC
                                           WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN (
                                                (e.pym_inv_qty -- 上月庫存數量 
                                                            +e.apl_inqty   -- 月結入庫總量 
                                                            -e.apl_outqty  -- 月結撥發總量 
                                                            +e.trn_inqty   -- 月結調撥入總量 
                                                            -e.trn_outqty  -- 月結調撥入總量 
                                                            +e.adj_inqty   -- 月結調帳入總量 
                                                            -e.adj_outqty  -- 月結調帳出總量 
                                                            +e.bak_inqty   -- 月結繳回入庫總量 
                                                            -e.bak_outqty  -- 月結繳回出庫總量 
                                                            -e.rej_outqty  -- 月結退貨總量 
                                                            -e.dis_outqty  -- 月結報廢總量
                                                            +e.exg_inqty   -- 月結換貨入庫總量 
                                                            -e.exg_outqty  -- 月結換貨出庫總量 
                                                            -nvl( 
                                                                nvl(e.altered_use_qty, e.ori_use_qty), -- 調整消耗(只有藥局可以用), 月結耗用量=醫令扣庫 
                                                                0 
                                                            ) )
                                                )
                                           ELSE E.STORE_QTYC - (CASE WHEN E.CHK_TIME IS NOT NULL THEN E.USE_QTY_AF_CHK
                                                                     ELSE E.ORI_USE_QTY
                                                                END)
                                      END) - nvl(e.war_qty, 0) no_w_c_i, -- 不含戰備本月應有結存量,
                                          (CASE WHEN E.WAR_QTY=0 THEN E.CHK_QTY WHEN E.CHK_QTY <= E.WAR_QTY THEN 0 ELSE E.CHK_QTY - E.WAR_QTY END) no_w_c_c, --不含戰備本月盤存量,
                                          (E.APL_INQTY - 0) 不含贈品本月進貨,
                                         E.G34_MAX_APPQTY 單位申請基準量 ,E.EST_APPQTY  下月預計申請量,
                                         E.BAK_INQTY 退料入庫, BAK_OUTQTY 退料出庫, 
                                                                           (CASE WHEN E.DISC_CPRICE >= 5000 THEN (CASE WHEN (E.CHK_WH_GRADE = '1') THEN 0 
                                                                                   WHEN (E.WH_NAME LIKE '%供應中心%') THEN 0
                                                                                   WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN E.ALTERED_USE_QTY
                                                                                   ELSE E.USE_QTY_AF_CHK
                                                                              END)
                                             ELSE 0 
                                        END) 單價5000元以上總消耗, 
                                      (CASE WHEN E.DISC_CPRICE < 5000 THEN (CASE WHEN (E.CHK_WH_GRADE = '1') THEN 0 
                                                                                  WHEN (E.WH_NAME LIKE '%供應中心%') THEN 0
                                                                                  WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN E.ALTERED_USE_QTY
                                                                                  ELSE E.USE_QTY_AF_CHK
                                                                             END)
                                             ELSE 0 
                                        END) 單價未滿5000元總消耗,
                                        E.CASENO 合約案號
                              FROM INVMON_DATA E WHERE 1 = 1 
                              order by E.mmcode"
                 , p20 == "false" ? "E.PYM_INV_QTY" : @"(case when E.PYM_WAR_QTY = 0 then E.PYM_INV_QTY when E.PYM_INV_QTY <= E.PYM_WAR_QTY then 0 else E.PYM_INV_QTY - E.PYM_WAR_QTY end)"
                 , p20 == "false" ? string.Empty : " - nvl(E.WAR_QTY, 0)"
                 , p20 == "false" ? "E.CHK_QTY" : @"(case when E.WAR_QTY = 0 then E.CHK_QTY when E.CHK_QTY <= E.WAR_QTY then 0 else E.CHK_QTY - E.WAR_QTY end)"
                 , temp
                );
            }
            else if (p1 == "3" || p1 == "4")//盤存單位 選 全部藥局/全院
            {
                sql = @"        WITH WH_NOS AS ( SELECT A.WH_NO, WH_KIND 
                                                 FROM MI_WHID A, MI_WHMAST B 
                                                 WHERE (A.WH_USERID= :USER_NAME AND A.WH_NO = B.WH_NO)),
                              MAT_WH_NO_ALL AS ( SELECT WH_NO, WH_KIND
                                                 FROM MI_WHMAST 
                                                 WHERE WH_KIND = '1'
                                                 AND EXISTS (SELECT 1
                                                             FROM UR_UIR 
                                                             WHERE TUSER = :USER_NAME 
                                                             AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14','MMSpl_14'))), 
                              MED_WH_NO_ALL AS ( SELECT WH_NO, WH_KIND 
                                                 FROM MI_WHMAST 
                                                 WHERE WH_KIND = '0'
                                                 AND EXISTS (SELECT 1 
                                                             FROM UR_UIR
                                                             WHERE TUSER = :USER_NAME 
                                                             AND RLNO IN ('ADMG', 'ADMG_14', 'MED_14','PHR_14'))),
                                      INIDS AS ( SELECT C.INID  
                                                 FROM MI_WHID A, MI_WHMAST B, UR_INID C 
                                                 WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO) 
                                                 AND B.INID = C.INID), 
                                   INID_ALL AS ( SELECT B.INID  
                                                 FROM MI_WHMAST A, UR_INID B  
                                                 WHERE EXISTS (SELECT 1  
                                                               FROM UR_UIR  
                                                               WHERE TUSER = :USER_NAME  
                                                               AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14','MED_14','MMSpl_14'))),
                                MAT_WH_KIND AS ( SELECT DISTINCT 1 AS WH_KIND, '衛材' AS TEXT  
                                                 FROM MI_WHID A, MI_WHMAST B  
                                                 WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO AND B.WH_KIND = '1')  
                                                 OR EXISTS (SELECT 1
                                                            FROM UR_UIR
                                                            WHERE TUSER = :USER_NAME
                                                            AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14','MMSpl_14'))), 
                                MED_WH_KIND AS ( SELECT DISTINCT 0 AS WH_KIND, '藥品' AS TEXT  
                                                 FROM MI_WHID A, MI_WHMAST B  
                                                 WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO AND B.WH_KIND = '0')  
                                                 OR EXISTS (SELECT 1   
                                                            FROM UR_UIR   
                                                            WHERE TUSER = :USER_NAME  
                                                            AND RLNO IN ('ADMG', 'ADMG_14', 'MED_14','PHR_14'))), 
                                   CHK_DATA AS ( SELECT B.CHK_WH_NO, B.CHK_WH_KIND, B.CHK_WH_GRADE, B.CHK_YM, A.MMCODE, A.CHK_QTY, A.STORE_QTYC,
                                                        NVL((A.CHK_QTY - A.STORE_QTYC), 0) GAP, nvl(a.WAR_QTY, 0) AS WAR_QTY, nvl(a.pym_war_qty, 0) AS PYM_WAR_QTY,
                                                        LAST_INVMON(:SET_YM, B.CHK_WH_NO, A.MMCODE) AS PYM_INV_QTY, C.m_contprice as cont_price, C.DISC_CPRICE,
                                                        NVL(D.m_contprice, 0) AS PYM_CONT_PRICE, NVL(D.DISC_CPRICE, 0) AS PYM_DISC_CPRICE, C.e_SOURCECODE as sourcecode,
                                                        NVL(D.e_SOURCECODE, 'P') AS PYM_SOURCECODE, C.UNITRATE, C.M_CONTID, WH_NAME(B.CHK_WH_NO) AS WH_NAME,
                                                        GET_PARAM('MI_MAST','E_SOURCECODE', C.e_SOURCECODE) AS SOURCECODE_NAME, GET_PARAM('MI_MAST','M_CONTID',
                                                        C.M_CONTID) AS CONTID_NAME, NVL(C.BASE_UNIT, A.BASE_UNIT) BASE_UNIT,
                                                        nvl(a.apl_inqty, 0) apl_inqty , nvl(a.apl_outqty, 0) apl_outqty, nvl(a.TRN_INQTY, 0) TRN_INQTY, 
                                                        nvl(a.TRN_OUTQTY, 0) TRN_OUTQTY, nvl(a.ADJ_INQTY, 0) ADJ_INQTY, nvl(a.ADJ_OUTQTY, 0) ADJ_OUTQTY,
                                                        nvl(a.BAK_INQTY, 0) BAK_INQTY, nvl(a.BAK_OUTQTy, 0) BAK_OUTQTy, nvl(a.REJ_OUTQTY, 0) REJ_OUTQTY, 
                                                        nvl(a.DIS_OUTQTY, 0) DIS_OUTQTY, nvl(a.EXG_INQTy, 0) EXG_INQTy, nvl(a.EXG_OUTQTY, 0) EXG_OUTQTY,
                                                        nvl(a.use_qty, 0) use_qty, nvl(a.use_qty_af_chk, 0) use_qty_af_chk, nvl(inventory, 0) inventory, 
                                                        nvl(a.mil_use_qty, 0) mil_use_qty, nvl(a.civil_use_qty, 0) civil_use_qty, nvl(a.ALTERED_USE_QTY, 0) ALTERED_USE_QTY, 
                                                        a.chk_time, C.CASENO,
                                                        c.WARBAK, nvl(d.WARBAK, ' ') as pym_warbak, c.m_agenno, c.mmname_c, c.mmname_e, c.mat_class_sub,A.g34_max_appqty ,A.est_appqty 
                                                 FROM CHK_MAST B, CHK_DETAIL A, MI_MAST_MON c
                                                      left join MI_MAST_MON d on (d.mmcode = c.mmcode and d.data_ym = twn_pym(c.data_ym))
                                                 WHERE B.CHK_YM = :SET_YM AND B.CHK_NO = A.CHK_NO
                                                 and a.mmcode= c.mmcode and b.chk_ym=c.data_ym
                                                 AND B.CHK_LEVEL = '1' ";
                if (p17 == "true")
                {
                    sql += @" AND A.inventory <> 0";
                }
                if (p21 == "true")
                {
                    sql += @"                  AND (A.WAR_QTY <> 0 and a.chk_qty < A.WAR_QTY)";
                }

                if (p18 == "true")
                {
                    sql += @"and (
exists (select 1 from mi_winvmon
  where mmcode=a.mmcode
    and DATA_YM>=
        substr(twn_date(ADD_MONTHS(twn_todate((select max(set_ym) from mi_mnset where set_status='C')||'01'),-5)),1,5)
    and (APL_INQTY>0 or TRN_INQTY>0 or USE_QTY>0)
    and wh_no=a.wh_no)
or 
exists (select 1 from mi_winvmon
  where mmcode=a.mmcode
    and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
    and INV_QTY<>0
    and wh_no=a.wh_no)
or
exists (select 1 from mi_winvmon b
  where mmcode=a.mmcode
    and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
    and INV_QTY=0
    and wh_no=a.wh_no
    and exists(select 1 from mi_mast_mon where mmcode=b.mmcode
          and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
          and CANCEL_ID='N')
  )
)";
                }

                if (p19 == "true")
                {
                    sql += @"
                    and (
                        NVL(a.PYM_INV_QTY,0) > 0
                        or
                        (NVL(a.PYM_INV_QTY,0) = 0 and not (a.apl_inqty = 0                       
                                                   and a.trn_inqty = 0        
                                                   and a.trn_outqty = 0        
                                                   and a.adj_inqty = 0        
                                                   and a.adj_outqty = 0        
                                                   and a.bak_outqty = 0
                                                    and a.bak_inqty=0
                                                    and nvl(nvl(a.altered_use_qty, a.use_qty),0)=0
                                                )
                        )
                    )
                ";
                }

                if (p1 == "3") //若盤存單位 選 全部藥局
                {
                    sql += @"                    AND B.CHK_WH_KIND = '0' 
                                                 AND B.CHK_WH_GRADE = '2' ";
                }

                sql += @"                        AND A.MMCODE = NVL(TRIM(:MMCODE_TEXT),A.MMCODE) 
                                                 AND C.M_CONTID = NVL(TRIM(:M_CONTID_TEXT),C.M_CONTID) 
                                                 AND C.e_SOURCECODE = NVL(TRIM(:SOURCECODE_TEXT),C.e_SOURCECODE) ";
                if (p5 == "A")//若藥材類別選A
                {
                    sql += @"                   AND C.MAT_CLASS = '01' ";
                }
                else if (p5 == "B")//若藥材類別選B
                {
                    sql += @"                   AND C.MAT_CLASS = '02' ";
                }
                else if (p5 != "A" && p5 != "B" && p5.Trim() != "")//若藥材類別選其他
                {
                    sql += @"                   AND C.MAT_CLASS_SUB = NVL(TRIM(:MAT_CLASS_SUB_TEXT),C.MAT_CLASS_SUB) ";
                }

                sql += string.Format(@"         AND C.WARBAK = NVL(TRIM(:WARBAK_TEXT),C.WARBAK)
                                                AND C.E_RESTRICTCODE = NVL(TRIM(:E_RESTRICTCODE_TEXT),C.E_RESTRICTCODE)
                                                AND C.COMMON = NVL(TRIM(:COMMON_TEXT),C.COMMON)
                                                AND C.FASTDRUG = NVL(TRIM(:FASTDRUG_TEXT),C.FASTDRUG)
                                                AND C.DRUGKIND = NVL(TRIM(:DRUGKIND_TEXT),C.DRUGKIND)
                                                AND C.TOUCHCASE = NVL(TRIM(:TOUCHCASE_TEXT),C.TOUCHCASE)
                                                AND C.ORDERKIND = NVL(TRIM(:ORDERKIND_TEXT),C.ORDERKIND)
                                                AND C.SPDRUG = NVL(TRIM(:SPDRUG_TEXT),C.SPDRUG)),
                                INVMON_DATA AS ( SELECT '{0}' as WH_NO, A.WH_NAME, A.MMCODE, SUM(NVL(a.store_qtyc, 0)) AS ORI_INV_QTY, 
                                                      SUM(A.APL_INQTY) AS APL_INQTY,
                                                      (CASE WHEN A.CHK_WH_GRADE = '1' THEN SUM(A.APL_INQTY) else 0 END) AS APL_INQTY_1,
                                                      (CASE WHEN A.CHK_WH_GRADE != '1' THEN SUM(A.APL_INQTY) else 0 END) AS APL_INQTY_2,
                                                      SUM(A.APL_OUTQTY) AS APL_OUTQTY, SUM(A.TRN_INQTY) AS TRN_INQTY, 
                                                      SUM(A.TRN_OUTQTY) AS TRN_OUTQTY, SUM(A.ADJ_INQTY) AS ADJ_INQTY, SUM(A.ADJ_OUTQTY) AS ADJ_OUTQTY,
                                                      SUM(A.BAK_INQTY) AS BAK_INQTY, SUM(A.BAK_OUTQTY) AS BAK_OUTQTY, SUM(A.REJ_OUTQTY) AS REJ_OUTQTY, 
                                                      SUM(A.DIS_OUTQTY) AS DIS_OUTQTY, SUM(A.EXG_INQTY) AS EXG_INQTY, SUM(A.EXG_OUTQTY) AS EXG_OUTQTY,
                                                      SUM(NVL(a.use_qty, 0)) AS ORI_USE_QTY, sum(nvl(a.use_qty_af_chk, 0)) as use_qty_af_chk, 
                                                      sum(nvl(a.inventory, 0)) as inventory, sum(nvl(a.mil_use_qty, 0)) as mil_use_qty, 
                                                      sum(nvl(a.civil_use_qty, 0)) as civil_use_qty,
                                                      sum(nvl(a.ALTERED_USE_QTY, 0)) as ALTERED_USE_QTY, a.chk_time,
                                                      A.CHK_WH_KIND, A.CHK_WH_GRADE,A.CHK_YM, 
                                                      SUM(A.CHK_QTY) AS CHK_QTY, SUM(A.STORE_QTYC) AS STORE_QTYC, SUM(A.GAP) AS GAP, 
                                                      SUM(A.WAR_QTY) AS WAR_QTY, SUM(A.PYM_WAR_QTY) AS PYM_WAR_QTY, SUM(A.PYM_INV_QTY) AS PYM_INV_QTY, 
                                                      A.CONT_PRICE, A.DISC_CPRICE, A.PYM_CONT_PRICE, A.PYM_DISC_CPRICE, A.SOURCECODE, A.PYM_SOURCECODE,
                                                      A.UNITRATE, A.M_CONTID, A.SOURCECODE_NAME, A.CONTID_NAME, A.BASE_UNIT, A.WARBAK, A.PYM_WARBAK, 
                                                      A.M_AGENNO, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS_SUB, A.CASENO,
                                                      SUM(A.g34_max_appqty) AS G34_MAX_APPQTY ,SUM(A.est_appqty ) AS EST_APPQTY 
                                               FROM CHK_DATA A
                                               GROUP BY A.WH_NAME, A.MMCODE, A.CHK_WH_KIND, A.CHK_WH_GRADE, A.CHK_YM,A.CONT_PRICE, A.DISC_CPRICE,
                                                        A.PYM_CONT_PRICE, A.PYM_DISC_CPRICE, A.SOURCECODE, A.PYM_SOURCECODE, A.UNITRATE, A.M_CONTID, A.CASENO,
                                                        A.SOURCECODE_NAME, A.CONTID_NAME, A.BASE_UNIT, A.WARBAK, A.PYM_WARBAK, A.M_AGENNO, A.MMNAME_C, 
                                                        A.MMNAME_E, A.MAT_CLASS_SUB, a.chk_time)

                                SELECT E.WH_NO|| ' '||WH_NAME(E.WH_NO) 盤存單位, E.MMCODE 藥材代碼, E.M_AGENNO 廠商代碼, E.MMNAME_C 藥材名稱, E.MMNAME_E 英文品名, 
                                       E.BASE_UNIT 藥材單位, E.CONT_PRICE 單價, 
                                     E.DISC_CPRICE 優惠後單價, E.PYM_CONT_PRICE 上月單價, E.PYM_DISC_CPRICE 上月優惠後單價, E.UNITRATE 包裝量, 
                                     SUM( {1} ) 上月結存, SUM(NVL(E.APL_INQTY_1, 0)) 進貨, SUM(NVL(E.APL_INQTY_2, 0)) 撥發入庫, SUM(E.APL_OUTQTY) 撥發出庫, 
                                     SUM(E.TRN_INQTY) 調撥入庫, SUM(E.TRN_OUTQTY) 調撥出庫,
                                     SUM(E.ADJ_INQTY) 調帳入庫, SUM(E.ADJ_OUTQTY) 調帳出庫, SUM(E.BAK_INQTY) 退料入庫, SUM(E.BAK_OUTQTY) 退料出庫, SUM(E.REJ_OUTQTY) 退貨量, 
                                     SUM(E.DIS_OUTQTY) 報廢量, SUM(E.EXG_INQTY) 換貨入庫, SUM(E.EXG_OUTQTY) 換貨出庫, 0 AS 軍方消耗,
                                     SUM((case when (e.chk_wh_grade = '1') then 0 
                                           when (e.wh_name like '%供應中心%') then 0
                                           when (e.chk_wh_grade = '2' and e.chk_wh_kind = '0') then e.ALTERED_USE_QTY
                                           else e.USE_QTY_AF_CHK
                                     end)) 民眾消耗,
                                     SUM((case when (e.chk_wh_grade = '1') then 0 
                                           when (e.wh_name like '%供應中心%') then 0
                                           when (e.chk_wh_grade = '2' and e.chk_wh_kind = '0') then e.ALTERED_USE_QTY
                                           else e.USE_QTY_AF_CHK
                                     end)) 本月總消耗,
                                     SUM(( {4} ))  應有結存,
                                     SUM( {3} ) 盤存量, SUM( {3} ) 本月結存,
                                     SUM((CASE WHEN E.PYM_SOURCECODE = 'C' THEN E.PYM_INV_QTY ELSE 0 END) {2} ) p_c_qty, -- 上月寄庫藥品買斷結存
                                     SUM((CASE WHEN E.SOURCECODE = 'C' THEN E.CHK_QTY ELSE 0 END) {2}) c_c_qty, --本月寄庫藥品買斷結存,
                                     SUM(e.inventory) 差異量,
                                     SUM(E.CONT_PRICE*  {3} ) 結存金額, SUM(E.DISC_CPRICE*  {3} ) 優惠後結存金額,
                                     SUM(e.inventory * e.cont_price) 差異金額,
                                     SUM(e.inventory * e.DISC_CPRICE) 優惠後差異金額,
                                     get_param('MI_MAST', 'MAT_CLASS_SUB', E.MAT_CLASS_SUB) 藥材類別, 
                                     E.CONTID_NAME 是否合約, E.SOURCECODE_NAME 買斷寄庫, E.WARBAK 是否戰備, SUM(E.WAR_QTY) 戰備存量,
                                     ROUND((CASE WHEN SUM((E.ORI_USE_QTY + (CASE WHEN (E.GAP <= 0) THEN (E.STORE_QTYC - E.CHK_QTY) 
                                                                             ELSE 0 END))) = 0 
                                                 THEN 0
                                                 ELSE (SUM( {3} ) / SUM((E.ORI_USE_QTY + (CASE WHEN (E.GAP <= 0) THEN (E.STORE_QTYC - E.CHK_QTY) 
                                                                                          ELSE 0 END))))
                                          END), 2) 期末比,
                                     0 贈品數量, SUM(E.PYM_WAR_QTY) 上月戰備量, E.PYM_WARBAK 上月是否戰備, 
                                     (CASE WHEN SUM(E.PYM_WAR_QTY)=0 THEN SUM(E.PYM_INV_QTY)
                                           WHEN SUM((E.PYM_INV_QTY - E.PYM_WAR_QTY)) < 0 THEN 0 
                                           ELSE SUM(E.PYM_INV_QTY - E.PYM_WAR_QTY) END) no_w_p_i, --不含戰備上月結存, 
                                     E.DISC_CPRICE - E.PYM_DISC_CPRICE 單價差額, 
                                     (CASE WHEN SUM((E.PYM_INV_QTY - E.PYM_WAR_QTY)) < 0 THEN 0
                                           ELSE SUM(E.PYM_INV_QTY - E.PYM_WAR_QTY) END) *(E.DISC_CPRICE - E.PYM_DISC_CPRICE) no_w_p_i_dif, -- 不含戰備上月結存價差,
                                     SUM((CASE  WHEN E.WAR_QTY=0 THEN 0
                                                WHEN E.CHK_QTY <= E.WAR_QTY THEN E.CHK_QTY * (E.DISC_CPRICE - E.PYM_DISC_CPRICE)
                                           ELSE (E.CHK_QTY - E.WAR_QTY) * (E.DISC_CPRICE - E.PYM_DISC_CPRICE) END)) 戰備本月價差,
                                     SUM((CASE WHEN E.CHK_QTY <= E.WAR_QTY THEN 0 ELSE E.CHK_QTY - E.WAR_QTY END)) 不含戰備本月結存,
                                    SUM( (case when E.WAR_QTY > e.chk_qty then 0 
                                       else
                                        (
                                              (CASE WHEN (E.CHK_WH_GRADE = '1') THEN E.STORE_QTYC
                                                 WHEN (E.WH_NAME LIKE '%供應中心%') THEN E.STORE_QTYC
                                                 WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN (
                                                      (e.pym_inv_qty -- 上月庫存數量 
                                                                  +e.apl_inqty   -- 月結入庫總量 
                                                                  -e.apl_outqty  -- 月結撥發總量 
                                                                  +e.trn_inqty   -- 月結調撥入總量 
                                                                  -e.trn_outqty  -- 月結調撥入總量 
                                                                  +e.adj_inqty   -- 月結調帳入總量 
                                                                  -e.adj_outqty  -- 月結調帳出總量 
                                                                  +e.bak_inqty   -- 月結繳回入庫總量 
                                                                  -e.bak_outqty  -- 月結繳回出庫總量 
                                                                  -e.rej_outqty  -- 月結退貨總量 
                                                                  -e.dis_outqty  -- 月結報廢總量
                                                                  +e.exg_inqty   -- 月結換貨入庫總量 
                                                                  -e.exg_outqty  -- 月結換貨出庫總量 
                                                                  -nvl( 
                                                                      nvl(e.altered_use_qty, e.ori_use_qty), -- 調整消耗(只有藥局可以用), 月結耗用量=醫令扣庫 
                                                                      0 
                                                                  ) )
                                                      )
                                                 ELSE E.STORE_QTYC - (CASE WHEN E.CHK_TIME IS NOT NULL THEN E.USE_QTY_AF_CHK
                                                                           ELSE E.ORI_USE_QTY
                                                                      END)
                                            END) - nvl(e.war_qty, 0)
                                       ) 
                                     end)) no_w_c_i, -- 不含戰備本月應有結存量,
                                     SUM((CASE WHEN E.WAR_QTY=0 THEN E.CHK_QTY WHEN E.CHK_QTY <= E.WAR_QTY THEN 0 ELSE E.CHK_QTY - E.WAR_QTY END)) no_w_c_c, --不含戰備本月盤存量,
                                     SUM((E.APL_INQTY - 0)) 不含贈品本月進貨,
                                    -- E.G34_MAX_APPQTY 單位申請基準量 ,E.EST_APPQTY  下月預計申請量,
                                   --SUM( E.BAK_INQTY) 退料入庫,SUM( BAK_OUTQTY) 退料出庫, 
                                                                          SUM( (CASE WHEN E.DISC_CPRICE >= 5000 THEN (CASE WHEN (E.CHK_WH_GRADE = '1') THEN 0 
                                                                                   WHEN (E.WH_NAME LIKE '%供應中心%') THEN 0
                                                                                   WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN E.ALTERED_USE_QTY
                                                                                   ELSE E.USE_QTY_AF_CHK
                                                                              END)
                                             ELSE 0 
                                        END)) 單價5000元以上總消耗, 
                                     SUM( (CASE WHEN E.DISC_CPRICE < 5000 THEN (CASE WHEN (E.CHK_WH_GRADE = '1') THEN 0 
                                                                                  WHEN (E.WH_NAME LIKE '%供應中心%') THEN 0
                                                                                  WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN E.ALTERED_USE_QTY
                                                                                  ELSE E.USE_QTY_AF_CHK
                                                                             END)
                                             ELSE 0 
                                        END)) 單價未滿5000元總消耗,
                                        E.CASENO 合約案號
                              FROM INVMON_DATA E WHERE 1 = 1
                                group by E.WH_NO, E.MMCODE, E.M_AGENNO, E.MMNAME_C, E.MMNAME_E,
                                E.BASE_UNIT, E.CONT_PRICE , E.DISC_CPRICE, E.PYM_CONT_PRICE, 
                                E.PYM_DISC_CPRICE, E.UNITRATE,
                                --E.CHK_WH_GRADE, E.WH_NAME, E.CHK_WH_KIND,
                                 E.PYM_SOURCECODE, 
                                E.MAT_CLASS_SUB, E.CONTID_NAME, E.SOURCECODE_NAME,
                                E.WARBAK,E.PYM_WARBAK,
                                --E.G34_MAX_APPQTY, 
                                --E.EST_APPQTY, 
                                E.CASENO
                                order by E.mmcode
                "
                                , p1 == "3" ? "全院藥局" : "全院"
                                , p20 == "false" ? "E.PYM_INV_QTY" : @"(case when E.PYM_WAR_QTY = 0 then E.PYM_INV_QTY when E.PYM_INV_QTY <= E.PYM_WAR_QTY then 0 else E.PYM_INV_QTY - E.PYM_WAR_QTY end)"
                 , p20 == "false" ? string.Empty : " - nvl(E.WAR_QTY, 0)"
                 , p20 == "false" ? "E.CHK_QTY" : @"(case when E.WAR_QTY = 0 then E.CHK_QTY when E.CHK_QTY <= E.WAR_QTY then 0 else E.CHK_QTY - E.WAR_QTY end)"
                 , temp
                );
            }
            #endregion

            p.Add(":USER_NAME", user);
            p.Add(":SET_YM", p0); //月結年月
            p.Add(":RLNO_TEXT", p1); //盤存單位
            p.Add(":CHK_WH_KIND_TEXT", p2); //庫房類別
            p.Add(":CHK_WH_NO_TEXT", p3); //庫房代碼
            p.Add(":INID_TEXT", p4); //責任中心
            p.Add(":MAT_CLASS_SUB_TEXT", p5); //藥材類別
            p.Add(":MMCODE_TEXT", p6); //藥材代碼
            p.Add(":M_CONTID_TEXT", p7); //是否合約
            p.Add(":SOURCECODE_TEXT", p8); //買斷寄庫
            p.Add(":WARBAK_TEXT", p9); //是否戰備
            p.Add(":E_RESTRICTCODE_TEXT", p10); //管制品項
            p.Add(":COMMON_TEXT", p11); //是否常用品項
            p.Add(":FASTDRUG_TEXT", p12); //急救品項
            p.Add(":DRUGKIND_TEXT", p13); //中西藥別
            p.Add(":TOUCHCASE_TEXT", p14); //合約類別
            p.Add(":ORDERKIND_TEXT", p15); //採購類別
            p.Add(":SPDRUG_TEXT", p16); //特殊品項


            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        //[匯出消耗結存表] 805(花蓮)專用
        public DataTable GetExcel_805(string p0, string p1, string p2, string p3, string p4, string p5, string p6, string p7, string p8, string p9, string p10, 
                                      string p11, string p12, string p13, string p14, string p15, string p16, string p17, string p18, string p19, string HospCode, string user,
                                      string p20, string p21)
        {
            var p = new DynamicParameters();
            var sql = "";

            //應有結存
            string temp = @"(CASE WHEN (E.CHK_WH_GRADE = '1') THEN E.STORE_QTYC
                                               WHEN (E.WH_NAME LIKE '%供應中心%') THEN E.STORE_QTYC
                                               WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN (
                                                    (e.pym_inv_qty -- 上月庫存數量 
                                                                +e.apl_inqty   -- 月結入庫總量 
                                                                -e.apl_outqty  -- 月結撥發總量 
                                                                +e.trn_inqty   -- 月結調撥入總量 
                                                                -e.trn_outqty  -- 月結調撥入總量 
                                                                +e.adj_inqty   -- 月結調帳入總量 
                                                                -e.adj_outqty  -- 月結調帳出總量 
                                                                +e.bak_inqty   -- 月結繳回入庫總量 
                                                                -e.bak_outqty  -- 月結繳回出庫總量 
                                                                -e.rej_outqty  -- 月結退貨總量 
                                                                -e.dis_outqty  -- 月結報廢總量
                                                                +e.exg_inqty   -- 月結換貨入庫總量 
                                                                -e.exg_outqty  -- 月結換貨出庫總量 
                                                                -nvl( 
                                                                    nvl(e.altered_use_qty, e.ori_use_qty), -- 調整消耗(只有藥局可以用), 月結耗用量=醫令扣庫 
                                                                    0 
                                                                ) )
                                                    )
                                               ELSE E.STORE_QTYC - (CASE WHEN E.CHK_TIME IS NOT NULL THEN E.USE_QTY_AF_CHK
                                                                         ELSE E.ORI_USE_QTY
                                                                    END)
                                          END) ";
            if (p20 == "true")
            {
                temp = string.Format(@"
                    (case when E.WAR_QTY > e.chk_qty then 0 
                          else
                            ( {0} - nvl(E.war_qty, 0) )
                    end )
                ", temp);
            }
            #region
            //盤存單位 選 各請領單位、各單位消耗結存明細、藥材庫
            if (p1 == "1" || p1 == "2" || p1 == "5")
            {
                sql = @"      WITH WH_NOS AS ( SELECT A.WH_NO, WH_KIND 
                                               FROM MI_WHID A, MI_WHMAST B 
                                               WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO)),
                            MAT_WH_NO_ALL AS ( SELECT WH_NO, WH_KIND 
                                               FROM MI_WHMAST 
                                               WHERE WH_KIND = '1' 
                                               AND EXISTS (SELECT 1 
                                                           FROM UR_UIR  
                                                           WHERE TUSER = :USER_NAME
                                               AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14','MMSpl_14'))),
                            MED_WH_NO_ALL AS ( SELECT WH_NO, WH_KIND  
                                               FROM MI_WHMAST  
                                               WHERE WH_KIND = '0' 
                                               AND EXISTS (SELECT 1 FROM UR_UIR  
                                                           WHERE TUSER = :USER_NAME 
                                                           AND RLNO IN ('ADMG', 'ADMG_14', 'MED_14','PHR_14'))),
                                    INIDS AS ( SELECT C.INID  
                                               FROM MI_WHID A, MI_WHMAST B, UR_INID C  
                                               WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO)  
                                               AND B.INID = C.INID), 
                                 INID_ALL AS ( SELECT B.INID  
                                               FROM MI_WHMAST A, UR_INID B 
                                               WHERE EXISTS (SELECT 1  
                                                             FROM UR_UIR 
                                                             WHERE TUSER = :USER_NAME  
                                                             AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14','MED_14','MMSpl_14'))),
                              MAT_WH_KIND AS ( SELECT DISTINCT 1 AS WH_KIND, '衛材' AS TEXT  
                                               FROM MI_WHID A, MI_WHMAST B 
                                               WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO AND B.WH_KIND = '1') 
                                               OR EXISTS (SELECT 1 
                                                          FROM UR_UIR  
                                                          WHERE TUSER = :USER_NAME 
                                                          AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14','MMSpl_14'))),
                              MED_WH_KIND AS ( SELECT DISTINCT 0 AS WH_KIND, '藥品' AS TEXT 
                                               FROM MI_WHID A, MI_WHMAST B 
                                               WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO AND B.WH_KIND = '0') 
                                               OR EXISTS (SELECT 1  
                                                          FROM UR_UIR 
                                                          WHERE TUSER = :USER_NAME 
                                                          AND RLNO IN ('ADMG', 'ADMG_14', 'MED_14','PHR_14'))),
                                 CHK_DATA AS ( SELECT B.CHK_WH_NO, B.CHK_WH_KIND, B.CHK_WH_GRADE, B.CHK_YM, A.MMCODE, A.CHK_QTY, A.STORE_QTYC,
                                                      NVL((A.CHK_QTY - A.STORE_QTYC), 0) GAP, nvl(a.WAR_QTY, 0) AS WAR_QTY, a.PYM_WAR_QTY AS PYM_WAR_QTY,
                                                      LAST_INVMON(:SET_YM, B.CHK_WH_NO, A.MMCODE) AS PYM_INV_QTY, C.m_contprice as cont_price, C.DISC_CPRICE,
                                                      NVL(D.m_contprice, 0) AS PYM_CONT_PRICE, NVL(D.DISC_CPRICE, 0) AS PYM_DISC_CPRICE, C.e_SOURCECODE as sourcecode,
                                                      NVL(D.e_SOURCECODE, 'P') AS PYM_SOURCECODE, C.UNITRATE, C.M_CONTID, WH_NAME(B.CHK_WH_NO) AS WH_NAME,
                                                      GET_PARAM('MI_MAST','E_SOURCECODE', C.e_SOURCECODE) AS SOURCECODE_NAME, GET_PARAM('MI_MAST','M_CONTID',
                                                      C.M_CONTID) AS CONTID_NAME, NVL(C.BASE_UNIT, A.BASE_UNIT) BASE_UNIT,
nvl(a.apl_inqty, 0) apl_inqty , nvl(a.apl_outqty, 0) apl_outqty, nvl(a.TRN_INQTY, 0) TRN_INQTY, 
                                                      nvl(a.TRN_OUTQTY, 0) TRN_OUTQTY, nvl(a.ADJ_INQTY, 0) ADJ_INQTY, nvl(a.ADJ_OUTQTY, 0) ADJ_OUTQTY,
                                                      nvl(a.BAK_INQTY, 0) BAK_INQTY, nvl(a.BAK_OUTQTy, 0) BAK_OUTQTy, nvl(a.REJ_OUTQTY, 0) REJ_OUTQTY, 
                                                      nvl(a.DIS_OUTQTY, 0) DIS_OUTQTY, nvl(a.EXG_INQTy, 0) EXG_INQTy, nvl(a.EXG_OUTQTY, 0) EXG_OUTQTY,
                                                      nvl(a.use_qty, 0) use_qty, nvl(a.use_qty_af_chk, 0) use_qty_af_chk, nvl(inventory, 0) inventory, 
                                                      nvl(a.mil_use_qty, 0) mil_use_qty, nvl(a.civil_use_qty, 0) civil_use_qty, nvl(a.ALTERED_USE_QTY, 0) ALTERED_USE_QTY, 
                                                     a.chk_time, C.CASENO,
                                                      c.WARBAK, nvl(d.WARBAK, ' ') as pym_warbak, c.m_agenno, c.mmname_c, c.mmname_e, c.mat_class_sub,A.g34_max_appqty ,A.est_appqty 
                                               FROM CHK_MAST B, CHK_DETAIL A, MI_MAST_MON c
                                                    left join MI_MAST_MON d on (d.mmcode = c.mmcode and d.data_ym = twn_pym(c.data_ym))
                                               WHERE B.CHK_YM = :SET_YM AND B.CHK_NO = A.CHK_NO
                                               and a.mmcode= c.mmcode and b.chk_ym=c.data_ym
                                               AND B.CHK_LEVEL = '1'
                                               AND B.CHK_WH_NO IN ( SELECT WH_NO FROM WH_NOS 
                                                                    UNION 
                                                                    SELECT WH_NO FROM MAT_WH_NO_ALL 
                                                                    UNION 
                                                                    SELECT WH_NO FROM MED_WH_NO_ALL)
                                               AND B.CHK_WH_KIND IN ( SELECT WH_KIND FROM MAT_WH_KIND 
                                                                      UNION 
                                                                      SELECT WH_KIND FROM MED_WH_KIND)
                                               AND (EXISTS ( SELECT 1  
                                                             FROM MI_WHMAST A, INIDS B  
                                                             WHERE A.WH_NO = B.CHK_WH_NO  
                                                             AND A.INID = B.INID)  
                                                    OR EXISTS (SELECT 1  
                                                               FROM MI_WHMAST A, INID_ALL B  
                                                               WHERE A.WH_NO = B.CHK_WH_NO  
                                                               AND A.INID = B.INID) ) ";

                if (p17 == "true")
                {
                    sql += @" AND A.inventory <> 0";
                }
                if (p21 == "true")
                {
                    sql += @"                  AND (A.WAR_QTY <> 0 and a.chk_qty < A.WAR_QTY)";
                }

                if (p18 == "true")
                {
                    sql += @"and (
exists (select 1 from mi_winvmon
  where mmcode=a.mmcode
    and DATA_YM>=
        substr(twn_date(ADD_MONTHS(twn_todate((select max(set_ym) from mi_mnset where set_status='C')||'01'),-5)),1,5)
    and (APL_INQTY>0 or TRN_INQTY>0 or USE_QTY>0)
    and wh_no=a.wh_no)
or 
exists (select 1 from mi_winvmon
  where mmcode=a.mmcode
    and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
    and INV_QTY<>0
    and wh_no=a.wh_no)
or
exists (select 1 from mi_winvmon b
  where mmcode=a.mmcode
    and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
    and INV_QTY=0
    and wh_no=a.wh_no
    and exists(select 1 from mi_mast_mon where mmcode=b.mmcode
          and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
          and CANCEL_ID='N')
  )
)";
                }

                if (p19 == "true")
                {
                    sql += @"
                    and (
                        NVL(a.PYM_INV_QTY,0) > 0
                        or
                        (NVL(a.PYM_INV_QTY,0) = 0 and not (a.apl_inqty = 0                       
                                                   and a.trn_inqty = 0        
                                                   and a.trn_outqty = 0        
                                                   and a.adj_inqty = 0        
                                                   and a.adj_outqty = 0        
                                                   and a.bak_outqty = 0
                                                    and a.bak_inqty=0
                                                    and nvl(nvl(a.altered_use_qty, a.use_qty),0)=0
                                                )
                        )
                    )
                ";
                }

                if (p1 == "1")//若盤存單位 選 各請領單位
                {
                    sql += @"                  AND B.CHK_WH_GRADE <> '1' 
                                               AND B.CHK_WH_KIND = NVL(TRIM(:CHK_WH_KIND_TEXT),B.CHK_WH_KIND)
                                               AND B.CHK_WH_NO = NVL(TRIM(:CHK_WH_NO_TEXT),B.CHK_WH_NO)
                                               AND B.CHK_WH_NO IN (SELECT WH_NO 
                                                                   FROM MI_WHMAST 
                                                                   WHERE INID = NVL(TRIM(:INID_TEXT),INID)) ";
                }
                else if (p1 == "2")//若盤存單位 選 藥材庫
                {
                    sql += @"                  AND B.CHK_WH_NO IN (WHNO_MM1, WHNO_ME1) ";
                }
                else if (p1 == "5")//若盤存單位 選 各單位消耗結存明細
                {
                    sql += @"                  AND B.CHK_WH_GRADE <> '1'  ";
                }

                sql += @"                      AND A.MMCODE = NVL(TRIM(:MMCODE_TEXT),A.MMCODE) 
                                               AND C.M_CONTID = NVL(TRIM(:M_CONTID_TEXT),C.M_CONTID) 
                                               AND C.e_SOURCECODE = NVL(TRIM(:SOURCECODE_TEXT),C.e_SOURCECODE)
                ";
                if (p5 == "A")//若藥材類別選A
                {
                    sql += @"                   AND C.MAT_CLASS = '01' ";
                }
                else if (p5 == "B")//若藥材類別選B
                {
                    sql += @"                   AND C.MAT_CLASS = '02' ";
                }
                else if (p5 != "A" && p5 != "B" && p5.Trim() != "")//若藥材類別選其他
                {
                    sql += @"                   AND C.MAT_CLASS_SUB = NVL(TRIM(:MAT_CLASS_SUB_TEXT),C.MAT_CLASS_SUB) ";
                }

                sql += @"                       AND C.WARBAK = NVL(TRIM(:WARBAK_TEXT),C.WARBAK)
                                                AND C.E_RESTRICTCODE = NVL(TRIM(:E_RESTRICTCODE_TEXT),C.E_RESTRICTCODE)
                                                AND C.COMMON = NVL(TRIM(:COMMON_TEXT),C.COMMON)
                                                AND C.FASTDRUG = NVL(TRIM(:FASTDRUG_TEXT),C.FASTDRUG)
                                                AND C.DRUGKIND = NVL(TRIM(:DRUGKIND_TEXT),C.DRUGKIND)
                                                AND C.TOUCHCASE = NVL(TRIM(:TOUCHCASE_TEXT),C.TOUCHCASE)
                                                AND C.ORDERKIND = NVL(TRIM(:ORDERKIND_TEXT),C.ORDERKIND)
                                                AND C.SPDRUG = NVL(TRIM(:SPDRUG_TEXT),C.SPDRUG)), 
                              INVMON_DATA AS ( SELECT A.CHK_WH_NO as WH_NO, A.WH_NAME, A.MMCODE, SUM(NVL(a.store_qtyc, 0)) AS ORI_INV_QTY, 
                                                      SUM(A.APL_INQTY) AS APL_INQTY, 
                                                      (CASE WHEN A.CHK_WH_GRADE = '1' THEN SUM(A.APL_INQTY) else 0 END) AS APL_INQTY_1,
                                                      (CASE WHEN A.CHK_WH_GRADE != '1' THEN SUM(A.APL_INQTY) else 0 END) AS APL_INQTY_2,
                                                      SUM(A.APL_OUTQTY) AS APL_OUTQTY, SUM(A.TRN_INQTY) AS TRN_INQTY, 
                                                      SUM(A.TRN_OUTQTY) AS TRN_OUTQTY, SUM(A.ADJ_INQTY) AS ADJ_INQTY, SUM(A.ADJ_OUTQTY) AS ADJ_OUTQTY,
                                                      SUM(A.BAK_INQTY) AS BAK_INQTY, SUM(A.BAK_OUTQTY) AS BAK_OUTQTY, SUM(A.REJ_OUTQTY) AS REJ_OUTQTY, 
                                                      SUM(A.DIS_OUTQTY) AS DIS_OUTQTY, SUM(A.EXG_INQTY) AS EXG_INQTY, SUM(A.EXG_OUTQTY) AS EXG_OUTQTY,
                                                      SUM(NVL(a.use_qty, 0)) AS ORI_USE_QTY, sum(nvl(a.use_qty_af_chk, 0)) as use_qty_af_chk, 
                                                      sum(nvl(a.inventory, 0)) as inventory, sum(nvl(a.mil_use_qty, 0)) as mil_use_qty, 
                                                      sum(nvl(a.civil_use_qty, 0)) as civil_use_qty,
                                                      sum(nvl(a.ALTERED_USE_QTY, 0)) as ALTERED_USE_QTY, a.chk_time,
                                                      A.CHK_WH_KIND, A.CHK_WH_GRADE,A.CHK_YM, 
                                                      SUM(A.CHK_QTY) AS CHK_QTY, SUM(A.STORE_QTYC) AS STORE_QTYC, SUM(A.GAP) AS GAP, 
                                                      SUM(A.WAR_QTY) AS WAR_QTY, SUM(A.PYM_WAR_QTY) AS PYM_WAR_QTY, SUM(A.PYM_INV_QTY) AS PYM_INV_QTY, 
                                                      A.CONT_PRICE, A.DISC_CPRICE, A.PYM_CONT_PRICE, A.PYM_DISC_CPRICE, A.SOURCECODE, A.PYM_SOURCECODE,
                                                      A.UNITRATE, A.M_CONTID, A.SOURCECODE_NAME, A.CONTID_NAME, A.BASE_UNIT, A.WARBAK, A.PYM_WARBAK, 
                                                      A.M_AGENNO, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS_SUB, A.CASENO,
                                                       SUM(A.g34_max_appqty) AS G34_MAX_APPQTY ,SUM(A.est_appqty ) AS EST_APPQTY 
                                               FROM CHK_DATA A
                                               GROUP BY A.chk_WH_NO, A.WH_NAME, A.MMCODE, A.CHK_WH_KIND, A.CHK_WH_GRADE, A.CHK_YM,A.CONT_PRICE, A.DISC_CPRICE,
                                                        A.PYM_CONT_PRICE, A.PYM_DISC_CPRICE, A.SOURCECODE, A.PYM_SOURCECODE, A.UNITRATE, A.M_CONTID, A.CASENO,
                                                        A.SOURCECODE_NAME, A.CONTID_NAME, A.BASE_UNIT, A.WARBAK, A.PYM_WARBAK, A.M_AGENNO, A.MMNAME_C, 
                                                        A.MMNAME_E, A.MAT_CLASS_SUB, a.chk_time )

                              SELECT 
                                    E.WH_NO|| ' '||WH_NAME(E.WH_NO) 盤存單位, 
                                    E.MMCODE 藥材代碼, 
                                    E.M_AGENNO 廠商代碼, 
                                    E.MMNAME_C 藥材名稱, 
                                    E.BASE_UNIT 藥材單位,
                                    E.CONT_PRICE 單價, 
                                    E.DISC_CPRICE 優惠後單價, 
                                    E.PYM_DISC_CPRICE 上月優惠後單價, 
                                    E.PYM_INV_QTY 上月結存, 
                                    E.PYM_DISC_CPRICE * E.PYM_INV_QTY 上月結存金額, 
                                    (E.APL_INQTY + E.TRN_INQTY + E.ADJ_INQTY + E.BAK_OUTQTY + E.REJ_OUTQTY + E.DIS_OUTQTY) 本月進貨,
                                    (E.APL_INQTY + E.TRN_INQTY + E.ADJ_INQTY + E.BAK_OUTQTY + E.REJ_OUTQTY + E.DIS_OUTQTY) * E.PYM_DISC_CPRICE 本月進貨金額,
                                    (E.TRN_OUTQTY + E.ADJ_OUTQTY + E.BAK_INQTY) 本月退貨,
                                    (E.TRN_OUTQTY + E.ADJ_OUTQTY + E.BAK_INQTY) * E.PYM_DISC_CPRICE 本月退貨金額,
                                    (case when (e.chk_wh_grade = '1') then 0 
                                           when (e.wh_name like '%供應中心%') then 0
                                           when (e.chk_wh_grade = '2' and e.chk_wh_kind = '0') then e.ALTERED_USE_QTY
                                           else e.USE_QTY_AF_CHK
                                     end) 本月總消耗,
                                    (case when (e.chk_wh_grade = '1') then 0 
                                           when (e.wh_name like '%供應中心%') then 0
                                           when (e.chk_wh_grade = '2' and e.chk_wh_kind = '0') then e.ALTERED_USE_QTY
                                           else e.USE_QTY_AF_CHK
                                     end) *  E.PYM_DISC_CPRICE 本月總消耗金額,
                                    E.CHK_QTY 本月結存,
                                    E.DISC_CPRICE * E.CHK_QTY 優惠後結存金額,
                                    E.CONTID_NAME 是否合約,
                                    E.G34_MAX_APPQTY 基準量45天,
                                    E.EST_APPQTY  下月申請量
                              FROM INVMON_DATA E WHERE 1 = 1 ";
            }
            else if (p1 == "3" || p1 == "4")//盤存單位 選 全部藥局/全院
            {
                sql = @"        WITH WH_NOS AS ( SELECT A.WH_NO, WH_KIND 
                                                 FROM MI_WHID A, MI_WHMAST B 
                                                 WHERE (A.WH_USERID= :USER_NAME AND A.WH_NO = B.WH_NO)),
                              MAT_WH_NO_ALL AS ( SELECT WH_NO, WH_KIND
                                                 FROM MI_WHMAST 
                                                 WHERE WH_KIND = '1'
                                                 AND EXISTS (SELECT 1
                                                             FROM UR_UIR 
                                                             WHERE TUSER = :USER_NAME 
                                                             AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14','MMSpl_14'))), 
                              MED_WH_NO_ALL AS ( SELECT WH_NO, WH_KIND 
                                                 FROM MI_WHMAST 
                                                 WHERE WH_KIND = '0'
                                                 AND EXISTS (SELECT 1 
                                                             FROM UR_UIR
                                                             WHERE TUSER = :USER_NAME 
                                                             AND RLNO IN ('ADMG', 'ADMG_14', 'MED_14','PHR_14'))),
                                      INIDS AS ( SELECT C.INID  
                                                 FROM MI_WHID A, MI_WHMAST B, UR_INID C 
                                                 WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO) 
                                                 AND B.INID = C.INID), 
                                   INID_ALL AS ( SELECT B.INID  
                                                 FROM MI_WHMAST A, UR_INID B  
                                                 WHERE EXISTS (SELECT 1  
                                                               FROM UR_UIR  
                                                               WHERE TUSER = :USER_NAME  
                                                               AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14','MED_14','MMSpl_14'))),
                                MAT_WH_KIND AS ( SELECT DISTINCT 1 AS WH_KIND, '衛材' AS TEXT  
                                                 FROM MI_WHID A, MI_WHMAST B  
                                                 WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO AND B.WH_KIND = '1')  
                                                 OR EXISTS (SELECT 1
                                                            FROM UR_UIR
                                                            WHERE TUSER = :USER_NAME
                                                            AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14','MMSpl_14'))), 
                                MED_WH_KIND AS ( SELECT DISTINCT 0 AS WH_KIND, '藥品' AS TEXT  
                                                 FROM MI_WHID A, MI_WHMAST B  
                                                 WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO AND B.WH_KIND = '0')  
                                                 OR EXISTS (SELECT 1   
                                                            FROM UR_UIR   
                                                            WHERE TUSER = :USER_NAME  
                                                            AND RLNO IN ('ADMG', 'ADMG_14', 'MED_14','PHR_14'))), 
                                   CHK_DATA AS ( SELECT B.CHK_WH_NO, B.CHK_WH_KIND, B.CHK_WH_GRADE, B.CHK_YM, A.MMCODE, A.CHK_QTY, A.STORE_QTYC,
                                                        NVL((A.CHK_QTY - A.STORE_QTYC), 0) GAP, nvl(a.WAR_QTY, 0) AS WAR_QTY, nvl(a.pym_war_qty, 0) AS PYM_WAR_QTY,
                                                        LAST_INVMON(:SET_YM, B.CHK_WH_NO, A.MMCODE) AS PYM_INV_QTY, C.m_contprice as cont_price, C.DISC_CPRICE,
                                                        NVL(D.m_contprice, 0) AS PYM_CONT_PRICE, NVL(D.DISC_CPRICE, 0) AS PYM_DISC_CPRICE, C.e_SOURCECODE as sourcecode,
                                                        NVL(D.e_SOURCECODE, 'P') AS PYM_SOURCECODE, C.UNITRATE, C.M_CONTID, WH_NAME(B.CHK_WH_NO) AS WH_NAME,
                                                        GET_PARAM('MI_MAST','E_SOURCECODE', C.e_SOURCECODE) AS SOURCECODE_NAME, GET_PARAM('MI_MAST','M_CONTID',
                                                        C.M_CONTID) AS CONTID_NAME, NVL(C.BASE_UNIT, A.BASE_UNIT) BASE_UNIT,
                                                        nvl(a.apl_inqty, 0) apl_inqty , nvl(a.apl_outqty, 0) apl_outqty, nvl(a.TRN_INQTY, 0) TRN_INQTY, 
                                                        nvl(a.TRN_OUTQTY, 0) TRN_OUTQTY, nvl(a.ADJ_INQTY, 0) ADJ_INQTY, nvl(a.ADJ_OUTQTY, 0) ADJ_OUTQTY,
                                                        nvl(a.BAK_INQTY, 0) BAK_INQTY, nvl(a.BAK_OUTQTy, 0) BAK_OUTQTy, nvl(a.REJ_OUTQTY, 0) REJ_OUTQTY, 
                                                        nvl(a.DIS_OUTQTY, 0) DIS_OUTQTY, nvl(a.EXG_INQTy, 0) EXG_INQTy, nvl(a.EXG_OUTQTY, 0) EXG_OUTQTY,
                                                        nvl(a.use_qty, 0) use_qty, nvl(a.use_qty_af_chk, 0) use_qty_af_chk, nvl(inventory, 0) inventory, 
                                                        nvl(a.mil_use_qty, 0) mil_use_qty, nvl(a.civil_use_qty, 0) civil_use_qty, nvl(a.ALTERED_USE_QTY, 0) ALTERED_USE_QTY, 
                                                        a.chk_time, C.CASENO,
                                                        c.WARBAK, nvl(d.WARBAK, ' ') as pym_warbak, c.m_agenno, c.mmname_c, c.mmname_e, c.mat_class_sub,A.g34_max_appqty ,A.est_appqty 
                                                 FROM CHK_MAST B, CHK_DETAIL A, MI_MAST_MON c
                                                      left join MI_MAST_MON d on (d.mmcode = c.mmcode and d.data_ym = twn_pym(c.data_ym))
                                                 WHERE B.CHK_YM = :SET_YM AND B.CHK_NO = A.CHK_NO
                                                 and a.mmcode= c.mmcode and b.chk_ym=c.data_ym
                                                 AND B.CHK_LEVEL = '1' ";
                if (p17 == "true")
                {
                    sql += @" AND A.inventory <> 0";
                }
                if (p21 == "true")
                {
                    sql += @"                  AND (A.WAR_QTY <> 0 and a.chk_qty < A.WAR_QTY)";
                }

                if (p18 == "true")
                {
                    sql += @"and (
exists (select 1 from mi_winvmon
  where mmcode=a.mmcode
    and DATA_YM>=
        substr(twn_date(ADD_MONTHS(twn_todate((select max(set_ym) from mi_mnset where set_status='C')||'01'),-5)),1,5)
    and (APL_INQTY>0 or TRN_INQTY>0 or USE_QTY>0)
    and wh_no=a.wh_no)
or 
exists (select 1 from mi_winvmon
  where mmcode=a.mmcode
    and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
    and INV_QTY<>0
    and wh_no=a.wh_no)
or
exists (select 1 from mi_winvmon b
  where mmcode=a.mmcode
    and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
    and INV_QTY=0
    and wh_no=a.wh_no
    and exists(select 1 from mi_mast_mon where mmcode=b.mmcode
          and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
          and CANCEL_ID='N')
  )
)";
                }

                if (p19 == "true")
                {
                    sql += @"
                    and (
                        NVL(a.PYM_INV_QTY,0) > 0
                        or
                        (NVL(a.PYM_INV_QTY,0) = 0 and not (a.apl_inqty = 0                       
                                                   and a.trn_inqty = 0        
                                                   and a.trn_outqty = 0        
                                                   and a.adj_inqty = 0        
                                                   and a.adj_outqty = 0        
                                                   and a.bak_outqty = 0
                                                    and a.bak_inqty=0
                                                    and nvl(nvl(a.altered_use_qty, a.use_qty),0)=0
                                                )
                        )
                    )
                ";
                }

                if (p1 == "3") //若盤存單位 選 全部藥局
                {
                    sql += @"                    AND B.CHK_WH_KIND = '0' 
                                                 AND B.CHK_WH_GRADE = '2' ";
                }

                sql += @"                        AND A.MMCODE = NVL(TRIM(:MMCODE_TEXT),A.MMCODE) 
                                                 AND C.M_CONTID = NVL(TRIM(:M_CONTID_TEXT),C.M_CONTID) 
                                                 AND C.e_SOURCECODE = NVL(TRIM(:SOURCECODE_TEXT),C.e_SOURCECODE) ";
                if (p5 == "A")//若藥材類別選A
                {
                    sql += @"                   AND C.MAT_CLASS = '01' ";
                }
                else if (p5 == "B")//若藥材類別選B
                {
                    sql += @"                   AND C.MAT_CLASS = '02' ";
                }
                else if (p5 != "A" && p5 != "B" && p5.Trim() != "")//若藥材類別選其他
                {
                    sql += @"                   AND C.MAT_CLASS_SUB = NVL(TRIM(:MAT_CLASS_SUB_TEXT),C.MAT_CLASS_SUB) ";
                }

                sql += string.Format(@"         AND C.WARBAK = NVL(TRIM(:WARBAK_TEXT),C.WARBAK)
                                                AND C.E_RESTRICTCODE = NVL(TRIM(:E_RESTRICTCODE_TEXT),C.E_RESTRICTCODE)
                                                AND C.COMMON = NVL(TRIM(:COMMON_TEXT),C.COMMON)
                                                AND C.FASTDRUG = NVL(TRIM(:FASTDRUG_TEXT),C.FASTDRUG)
                                                AND C.DRUGKIND = NVL(TRIM(:DRUGKIND_TEXT),C.DRUGKIND)
                                                AND C.TOUCHCASE = NVL(TRIM(:TOUCHCASE_TEXT),C.TOUCHCASE)
                                                AND C.ORDERKIND = NVL(TRIM(:ORDERKIND_TEXT),C.ORDERKIND)
                                                AND C.SPDRUG = NVL(TRIM(:SPDRUG_TEXT),C.SPDRUG)),
                                INVMON_DATA AS ( SELECT '{0}' as WH_NO, A.WH_NAME, A.MMCODE, SUM(NVL(a.store_qtyc, 0)) AS ORI_INV_QTY, 
                                                      SUM(A.APL_INQTY) AS APL_INQTY, 
                                                      (CASE WHEN A.CHK_WH_GRADE = '1' THEN SUM(A.APL_INQTY) else 0 END) AS APL_INQTY_1,
                                                      (CASE WHEN A.CHK_WH_GRADE != '1' THEN SUM(A.APL_INQTY) else 0 END) AS APL_INQTY_2,
                                                      SUM(A.APL_OUTQTY) AS APL_OUTQTY, SUM(A.TRN_INQTY) AS TRN_INQTY, 
                                                      SUM(A.TRN_OUTQTY) AS TRN_OUTQTY, SUM(A.ADJ_INQTY) AS ADJ_INQTY, SUM(A.ADJ_OUTQTY) AS ADJ_OUTQTY,
                                                      SUM(A.BAK_INQTY) AS BAK_INQTY, SUM(A.BAK_OUTQTY) AS BAK_OUTQTY, SUM(A.REJ_OUTQTY) AS REJ_OUTQTY, 
                                                      SUM(A.DIS_OUTQTY) AS DIS_OUTQTY, SUM(A.EXG_INQTY) AS EXG_INQTY, SUM(A.EXG_OUTQTY) AS EXG_OUTQTY,
                                                      SUM(NVL(a.use_qty, 0)) AS ORI_USE_QTY, sum(nvl(a.use_qty_af_chk, 0)) as use_qty_af_chk, 
                                                      sum(nvl(a.inventory, 0)) as inventory, sum(nvl(a.mil_use_qty, 0)) as mil_use_qty, 
                                                      sum(nvl(a.civil_use_qty, 0)) as civil_use_qty,
                                                      sum(nvl(a.ALTERED_USE_QTY, 0)) as ALTERED_USE_QTY, a.chk_time,
                                                      A.CHK_WH_KIND, A.CHK_WH_GRADE,A.CHK_YM, 
                                                      SUM(A.CHK_QTY) AS CHK_QTY, SUM(A.STORE_QTYC) AS STORE_QTYC, SUM(A.GAP) AS GAP, 
                                                      SUM(A.WAR_QTY) AS WAR_QTY, SUM(A.PYM_WAR_QTY) AS PYM_WAR_QTY, SUM(A.PYM_INV_QTY) AS PYM_INV_QTY, 
                                                      A.CONT_PRICE, A.DISC_CPRICE, A.PYM_CONT_PRICE, A.PYM_DISC_CPRICE, A.SOURCECODE, A.PYM_SOURCECODE,
                                                      A.UNITRATE, A.M_CONTID, A.SOURCECODE_NAME, A.CONTID_NAME, A.BASE_UNIT, A.WARBAK, A.PYM_WARBAK, 
                                                      A.M_AGENNO, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS_SUB, A.CASENO,
                                                      SUM(A.g34_max_appqty) AS G34_MAX_APPQTY ,SUM(A.est_appqty ) AS EST_APPQTY 
                                               FROM CHK_DATA A
                                               GROUP BY A.WH_NAME, A.MMCODE, A.CHK_WH_KIND, A.CHK_WH_GRADE, A.CHK_YM,A.CONT_PRICE, A.DISC_CPRICE,
                                                        A.PYM_CONT_PRICE, A.PYM_DISC_CPRICE, A.SOURCECODE, A.PYM_SOURCECODE, A.UNITRATE, A.M_CONTID, A.CASENO,
                                                        A.SOURCECODE_NAME, A.CONTID_NAME, A.BASE_UNIT, A.WARBAK, A.PYM_WARBAK, A.M_AGENNO, A.MMNAME_C, 
                                                        A.MMNAME_E, A.MAT_CLASS_SUB, a.chk_time)

                                SELECT 
                                    E.WH_NO|| ' '||WH_NAME(E.WH_NO) 盤存單位, 
                                    E.MMCODE 藥材代碼, 
                                    E.M_AGENNO 廠商代碼, 
                                    E.MMNAME_C 藥材名稱, 
                                    E.BASE_UNIT 藥材單位,
                                    E.CONT_PRICE 單價, 
                                    E.DISC_CPRICE 優惠後單價, 
                                    E.PYM_DISC_CPRICE 上月優惠後單價, 
                                    E.PYM_INV_QTY 上月結存, 
                                    E.PYM_DISC_CPRICE * E.PYM_INV_QTY 上月結存金額, 
                                    (E.APL_INQTY + E.TRN_INQTY + E.ADJ_INQTY + E.BAK_OUTQTY + E.REJ_OUTQTY + E.DIS_OUTQTY) 本月進貨,
                                    (E.APL_INQTY + E.TRN_INQTY + E.ADJ_INQTY + E.BAK_OUTQTY + E.REJ_OUTQTY + E.DIS_OUTQTY) * E.PYM_DISC_CPRICE 本月進貨金額,
                                    (E.TRN_OUTQTY + E.ADJ_OUTQTY + E.BAK_INQTY) 本月退貨,
                                    (E.TRN_OUTQTY + E.ADJ_OUTQTY + E.BAK_INQTY) * E.PYM_DISC_CPRICE 本月退貨金額,
                                    (case when (e.chk_wh_grade = '1') then 0 
                                           when (e.wh_name like '%供應中心%') then 0
                                           when (e.chk_wh_grade = '2' and e.chk_wh_kind = '0') then e.ALTERED_USE_QTY
                                           else e.USE_QTY_AF_CHK
                                     end) 本月總消耗,
                                    (case when (e.chk_wh_grade = '1') then 0 
                                           when (e.wh_name like '%供應中心%') then 0
                                           when (e.chk_wh_grade = '2' and e.chk_wh_kind = '0') then e.ALTERED_USE_QTY
                                           else e.USE_QTY_AF_CHK
                                     end) *  E.PYM_DISC_CPRICE 本月總消耗金額,
                                    E.CHK_QTY 本月結存,
                                    E.DISC_CPRICE * E.CHK_QTY 優惠後結存金額,
                                    E.CONTID_NAME 是否合約,
                                    E.G34_MAX_APPQTY 基準量45天,
                                    E.EST_APPQTY  下月申請量
                              FROM INVMON_DATA E WHERE 1 = 1 "
                                , p1 == "3" ? "全院藥局" : "全院");
            }
            #endregion

            p.Add(":USER_NAME", user);
            p.Add(":SET_YM", p0); //月結年月
            p.Add(":RLNO_TEXT", p1); //盤存單位
            p.Add(":CHK_WH_KIND_TEXT", p2); //庫房類別
            p.Add(":CHK_WH_NO_TEXT", p3); //庫房代碼
            p.Add(":INID_TEXT", p4); //責任中心
            p.Add(":MAT_CLASS_SUB_TEXT", p5); //藥材類別
            p.Add(":MMCODE_TEXT", p6); //藥材代碼
            p.Add(":M_CONTID_TEXT", p7); //是否合約
            p.Add(":SOURCECODE_TEXT", p8); //買斷寄庫
            p.Add(":WARBAK_TEXT", p9); //是否戰備
            p.Add(":E_RESTRICTCODE_TEXT", p10); //管制品項
            p.Add(":COMMON_TEXT", p11); //是否常用品項
            p.Add(":FASTDRUG_TEXT", p12); //急救品項
            p.Add(":DRUGKIND_TEXT", p13); //中西藥別
            p.Add(":TOUCHCASE_TEXT", p14); //合約類別
            p.Add(":ORDERKIND_TEXT", p15); //採購類別
            p.Add(":SPDRUG_TEXT", p16); //特殊品項


            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        //[匯出消耗結存明細] 803(台中)專用
        public DataTable GetExcelDetail_803(string p0, string p1, string p2, string p3, string p4, string p5, string p6, string p7, string p8, string p9, string p10, 
                                            string p11, string p12, string p13, string p14, string p15, string p16, string p17, string p18, string p19, string HospCode, string user,
                                            string p20, string p21)
        {
            var p = new DynamicParameters();
            var sql = "";

            //應有結存
            string temp = @"(CASE WHEN (E.CHK_WH_GRADE = '1') THEN E.STORE_QTYC
                                               WHEN (E.WH_NAME LIKE '%供應中心%') THEN E.STORE_QTYC
                                               WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN (
                                                    (e.pym_inv_qty -- 上月庫存數量 
                                                                +e.apl_inqty   -- 月結入庫總量 
                                                                -e.apl_outqty  -- 月結撥發總量 
                                                                +e.trn_inqty   -- 月結調撥入總量 
                                                                -e.trn_outqty  -- 月結調撥入總量 
                                                                +e.adj_inqty   -- 月結調帳入總量 
                                                                -e.adj_outqty  -- 月結調帳出總量 
                                                                +e.bak_inqty   -- 月結繳回入庫總量 
                                                                -e.bak_outqty  -- 月結繳回出庫總量 
                                                                -e.rej_outqty  -- 月結退貨總量 
                                                                -e.dis_outqty  -- 月結報廢總量
                                                                +e.exg_inqty   -- 月結換貨入庫總量 
                                                                -e.exg_outqty  -- 月結換貨出庫總量 
                                                                -nvl( 
                                                                    nvl(e.altered_use_qty, e.ori_use_qty), -- 調整消耗(只有藥局可以用), 月結耗用量=醫令扣庫 
                                                                    0 
                                                                ) )
                                                    )
                                               ELSE E.STORE_QTYC - (CASE WHEN E.CHK_TIME IS NOT NULL THEN E.USE_QTY_AF_CHK
                                                                         ELSE E.ORI_USE_QTY
                                                                    END)
                                          END) ";
            if (p20 == "true")
            {
                temp = string.Format(@"
                    (case when E.WAR_QTY > e.chk_qty then 0 
                          else
                            ( {0} - nvl(E.war_qty, 0) )
                    end )
                ", temp);
            }
            #region
            //盤存單位 選 各請領單位、各單位消耗結存明細、藥材庫
            if (p1 == "1" || p1 == "2" || p1 == "5")
            {
                sql = @"      WITH WH_NOS AS ( SELECT A.WH_NO, WH_KIND 
                                               FROM MI_WHID A, MI_WHMAST B 
                                               WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO)),
                            MAT_WH_NO_ALL AS ( SELECT WH_NO, WH_KIND 
                                               FROM MI_WHMAST 
                                               WHERE WH_KIND = '1' 
                                               AND EXISTS (SELECT 1 
                                                           FROM UR_UIR  
                                                           WHERE TUSER = :USER_NAME
                                               AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14','MMSpl_14'))),
                            MED_WH_NO_ALL AS ( SELECT WH_NO, WH_KIND  
                                               FROM MI_WHMAST  
                                               WHERE WH_KIND = '0' 
                                               AND EXISTS (SELECT 1 FROM UR_UIR  
                                                           WHERE TUSER = :USER_NAME 
                                                           AND RLNO IN ('ADMG', 'ADMG_14', 'MED_14','PHR_14'))),
                                    INIDS AS ( SELECT C.INID  
                                               FROM MI_WHID A, MI_WHMAST B, UR_INID C  
                                               WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO)  
                                               AND B.INID = C.INID), 
                                 INID_ALL AS ( SELECT B.INID  
                                               FROM MI_WHMAST A, UR_INID B 
                                               WHERE EXISTS (SELECT 1  
                                                             FROM UR_UIR 
                                                             WHERE TUSER = :USER_NAME  
                                                             AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14','MED_14','MMSpl_14'))),
                              MAT_WH_KIND AS ( SELECT DISTINCT 1 AS WH_KIND, '衛材' AS TEXT  
                                               FROM MI_WHID A, MI_WHMAST B 
                                               WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO AND B.WH_KIND = '1') 
                                               OR EXISTS (SELECT 1 
                                                          FROM UR_UIR  
                                                          WHERE TUSER = :USER_NAME 
                                                          AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14','MMSpl_14'))),
                              MED_WH_KIND AS ( SELECT DISTINCT 0 AS WH_KIND, '藥品' AS TEXT 
                                               FROM MI_WHID A, MI_WHMAST B 
                                               WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO AND B.WH_KIND = '0') 
                                               OR EXISTS (SELECT 1  
                                                          FROM UR_UIR 
                                                          WHERE TUSER = :USER_NAME 
                                                          AND RLNO IN ('ADMG', 'ADMG_14', 'MED_14','PHR_14'))),
                                 CHK_DATA AS ( SELECT B.CHK_WH_NO, B.CHK_WH_KIND, B.CHK_WH_GRADE, B.CHK_YM, A.MMCODE, A.CHK_QTY, A.STORE_QTYC,
                                                      NVL((A.CHK_QTY - A.STORE_QTYC), 0) GAP, nvl(a.WAR_QTY, 0) AS WAR_QTY, a.PYM_WAR_QTY AS PYM_WAR_QTY,
                                                      LAST_INVMON(:SET_YM, B.CHK_WH_NO, A.MMCODE) AS PYM_INV_QTY, C.m_contprice as cont_price, C.DISC_CPRICE,
                                                      NVL(D.m_contprice, 0) AS PYM_CONT_PRICE, NVL(D.DISC_CPRICE, 0) AS PYM_DISC_CPRICE, C.e_SOURCECODE as sourcecode,
                                                      NVL(D.e_SOURCECODE, 'P') AS PYM_SOURCECODE, C.UNITRATE, C.M_CONTID, WH_NAME(B.CHK_WH_NO) AS WH_NAME,
                                                      GET_PARAM('MI_MAST','E_SOURCECODE', C.e_SOURCECODE) AS SOURCECODE_NAME, GET_PARAM('MI_MAST','M_CONTID',
                                                      C.M_CONTID) AS CONTID_NAME, NVL(C.BASE_UNIT, A.BASE_UNIT) BASE_UNIT,
nvl(a.apl_inqty, 0) apl_inqty , nvl(a.apl_outqty, 0) apl_outqty, nvl(a.TRN_INQTY, 0) TRN_INQTY, 
                                                      nvl(a.TRN_OUTQTY, 0) TRN_OUTQTY, nvl(a.ADJ_INQTY, 0) ADJ_INQTY, nvl(a.ADJ_OUTQTY, 0) ADJ_OUTQTY,
                                                      nvl(a.BAK_INQTY, 0) BAK_INQTY, nvl(a.BAK_OUTQTy, 0) BAK_OUTQTy, nvl(a.REJ_OUTQTY, 0) REJ_OUTQTY, 
                                                      nvl(a.DIS_OUTQTY, 0) DIS_OUTQTY, nvl(a.EXG_INQTy, 0) EXG_INQTy, nvl(a.EXG_OUTQTY, 0) EXG_OUTQTY,
                                                      nvl(a.use_qty, 0) use_qty, nvl(a.use_qty_af_chk, 0) use_qty_af_chk, nvl(inventory, 0) inventory, 
                                                      nvl(a.mil_use_qty, 0) mil_use_qty, nvl(a.civil_use_qty, 0) civil_use_qty, nvl(a.ALTERED_USE_QTY, 0) ALTERED_USE_QTY, 
                                                     a.chk_time, C.CASENO,
                                                      c.WARBAK, nvl(d.WARBAK, ' ') as pym_warbak, c.m_agenno, c.mmname_c, c.mmname_e, c.mat_class_sub,A.g34_max_appqty ,A.est_appqty 
                                               FROM CHK_MAST B, CHK_DETAIL A, MI_MAST_MON c
                                                    left join MI_MAST_MON d on (d.mmcode = c.mmcode and d.data_ym = twn_pym(c.data_ym))
                                               WHERE B.CHK_YM = :SET_YM AND B.CHK_NO = A.CHK_NO
                                               and a.mmcode= c.mmcode and b.chk_ym=c.data_ym
                                               AND B.CHK_LEVEL = '1'
                                               AND B.CHK_WH_NO IN ( SELECT WH_NO FROM WH_NOS 
                                                                    UNION 
                                                                    SELECT WH_NO FROM MAT_WH_NO_ALL 
                                                                    UNION 
                                                                    SELECT WH_NO FROM MED_WH_NO_ALL)
                                               AND B.CHK_WH_KIND IN ( SELECT WH_KIND FROM MAT_WH_KIND 
                                                                      UNION 
                                                                      SELECT WH_KIND FROM MED_WH_KIND)
                                               AND (EXISTS ( SELECT 1  
                                                             FROM MI_WHMAST A, INIDS B  
                                                             WHERE A.WH_NO = B.CHK_WH_NO  
                                                             AND A.INID = B.INID)  
                                                    OR EXISTS (SELECT 1  
                                                               FROM MI_WHMAST A, INID_ALL B  
                                                               WHERE A.WH_NO = B.CHK_WH_NO  
                                                               AND A.INID = B.INID) ) ";

                if (p17 == "true")
                {
                    sql += @" AND A.inventory <> 0";
                }
                if (p21 == "true")
                {
                    sql += @"                  AND (A.WAR_QTY <> 0 and a.chk_qty < A.WAR_QTY)";
                }

                if (p18 == "true")
                {
                    sql += @"and (
exists (select 1 from mi_winvmon
  where mmcode=a.mmcode
    and DATA_YM>=
        substr(twn_date(ADD_MONTHS(twn_todate((select max(set_ym) from mi_mnset where set_status='C')||'01'),-5)),1,5)
    and (APL_INQTY>0 or TRN_INQTY>0 or USE_QTY>0)
    and wh_no=a.wh_no)
or 
exists (select 1 from mi_winvmon
  where mmcode=a.mmcode
    and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
    and INV_QTY<>0
    and wh_no=a.wh_no)
or
exists (select 1 from mi_winvmon b
  where mmcode=a.mmcode
    and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
    and INV_QTY=0
    and wh_no=a.wh_no
    and exists(select 1 from mi_mast_mon where mmcode=b.mmcode
          and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
          and CANCEL_ID='N')
  )
)";
                }

                if (p19 == "true")
                {
                    sql += @"
                    and (
                        NVL(a.PYM_INV_QTY,0) > 0
                        or
                        (NVL(a.PYM_INV_QTY,0) = 0 and not (a.apl_inqty = 0                       
                                                   and a.trn_inqty = 0        
                                                   and a.trn_outqty = 0        
                                                   and a.adj_inqty = 0        
                                                   and a.adj_outqty = 0        
                                                   and a.bak_outqty = 0
                                                    and a.bak_inqty=0
                                                    and nvl(nvl(a.altered_use_qty, a.use_qty),0)=0
                                                )
                        )
                    )
                ";
                }

                if (p1 == "1")//若盤存單位 選 各請領單位
                {
                    sql += @"                  AND B.CHK_WH_GRADE <> '1' 
                                               AND B.CHK_WH_KIND = NVL(TRIM(:CHK_WH_KIND_TEXT),B.CHK_WH_KIND)
                                               AND B.CHK_WH_NO = NVL(TRIM(:CHK_WH_NO_TEXT),B.CHK_WH_NO)
                                               AND B.CHK_WH_NO IN (SELECT WH_NO 
                                                                   FROM MI_WHMAST 
                                                                   WHERE INID = NVL(TRIM(:INID_TEXT),INID)) ";
                }
                else if (p1 == "2")//若盤存單位 選 藥材庫
                {
                    sql += @"                  AND B.CHK_WH_NO IN (WHNO_MM1, WHNO_ME1) ";
                }
                else if (p1 == "5")//若盤存單位 選 各單位消耗結存明細
                {
                    sql += @"                  AND B.CHK_WH_GRADE <> '1'  ";
                }

                sql += @"                      AND A.MMCODE = NVL(TRIM(:MMCODE_TEXT),A.MMCODE) 
                                               AND C.M_CONTID = NVL(TRIM(:M_CONTID_TEXT),C.M_CONTID) 
                                               AND C.e_SOURCECODE = NVL(TRIM(:SOURCECODE_TEXT),C.e_SOURCECODE)
                ";
                if (p5 == "A")//若藥材類別選A
                {
                    sql += @"                   AND C.MAT_CLASS = '01' ";
                }
                else if (p5 == "B")//若藥材類別選B
                {
                    sql += @"                   AND C.MAT_CLASS = '02' ";
                }
                else if (p5 != "A" && p5 != "B" && p5.Trim() != "")//若藥材類別選其他
                {
                    sql += @"                   AND C.MAT_CLASS_SUB = NVL(TRIM(:MAT_CLASS_SUB_TEXT),C.MAT_CLASS_SUB) ";
                }

                sql += string.Format(@" 
                                                AND C.WARBAK = NVL(TRIM(:WARBAK_TEXT),C.WARBAK)
                                                AND C.E_RESTRICTCODE = NVL(TRIM(:E_RESTRICTCODE_TEXT),C.E_RESTRICTCODE)
                                                AND C.COMMON = NVL(TRIM(:COMMON_TEXT),C.COMMON)
                                                AND C.FASTDRUG = NVL(TRIM(:FASTDRUG_TEXT),C.FASTDRUG)
                                                AND C.DRUGKIND = NVL(TRIM(:DRUGKIND_TEXT),C.DRUGKIND)
                                                AND C.TOUCHCASE = NVL(TRIM(:TOUCHCASE_TEXT),C.TOUCHCASE)
                                                AND C.ORDERKIND = NVL(TRIM(:ORDERKIND_TEXT),C.ORDERKIND)
                                                AND C.SPDRUG = NVL(TRIM(:SPDRUG_TEXT),C.SPDRUG)), 
                              INVMON_DATA AS ( SELECT A.CHK_WH_NO as WH_NO, A.WH_NAME, A.MMCODE, SUM(NVL(a.store_qtyc, 0)) AS ORI_INV_QTY, 
                                                      SUM(A.APL_INQTY) AS APL_INQTY, 
                                                      (CASE WHEN A.CHK_WH_GRADE = '1' THEN SUM(A.APL_INQTY) else 0 END) AS APL_INQTY_1,
                                                      (CASE WHEN A.CHK_WH_GRADE != '1' THEN SUM(A.APL_INQTY) else 0 END) AS APL_INQTY_2,
                                                      SUM(A.APL_OUTQTY) AS APL_OUTQTY, SUM(A.TRN_INQTY) AS TRN_INQTY, 
                                                      SUM(A.TRN_OUTQTY) AS TRN_OUTQTY, SUM(A.ADJ_INQTY) AS ADJ_INQTY, SUM(A.ADJ_OUTQTY) AS ADJ_OUTQTY,
                                                      SUM(A.BAK_INQTY) AS BAK_INQTY, SUM(A.BAK_OUTQTY) AS BAK_OUTQTY, SUM(A.REJ_OUTQTY) AS REJ_OUTQTY, 
                                                      SUM(A.DIS_OUTQTY) AS DIS_OUTQTY, SUM(A.EXG_INQTY) AS EXG_INQTY, SUM(A.EXG_OUTQTY) AS EXG_OUTQTY,
                                                      SUM(NVL(a.use_qty, 0)) AS ORI_USE_QTY, sum(nvl(a.use_qty_af_chk, 0)) as use_qty_af_chk, 
                                                      sum(nvl(a.inventory, 0)) as inventory, sum(nvl(a.mil_use_qty, 0)) as mil_use_qty, 
                                                      sum(nvl(a.civil_use_qty, 0)) as civil_use_qty,
                                                      sum(nvl(a.ALTERED_USE_QTY, 0)) as ALTERED_USE_QTY, a.chk_time,
                                                      A.CHK_WH_KIND, A.CHK_WH_GRADE,A.CHK_YM, 
                                                      SUM(A.CHK_QTY) AS CHK_QTY, SUM(A.STORE_QTYC) AS STORE_QTYC, SUM(A.GAP) AS GAP, 
                                                      SUM(A.WAR_QTY) AS WAR_QTY, SUM(A.PYM_WAR_QTY) AS PYM_WAR_QTY, SUM(A.PYM_INV_QTY) AS PYM_INV_QTY, 
                                                      A.CONT_PRICE, A.DISC_CPRICE, A.PYM_CONT_PRICE, A.PYM_DISC_CPRICE, A.SOURCECODE, A.PYM_SOURCECODE,
                                                      A.UNITRATE, A.M_CONTID, A.SOURCECODE_NAME, A.CONTID_NAME, A.BASE_UNIT, A.WARBAK, A.PYM_WARBAK, 
                                                      A.M_AGENNO, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS_SUB, A.CASENO,
                                                       SUM(A.g34_max_appqty) AS G34_MAX_APPQTY ,SUM(A.est_appqty ) AS EST_APPQTY 
                                               FROM CHK_DATA A
                                               GROUP BY A.chk_WH_NO, A.WH_NAME, A.MMCODE, A.CHK_WH_KIND, A.CHK_WH_GRADE, A.CHK_YM,A.CONT_PRICE, A.DISC_CPRICE,
                                                        A.PYM_CONT_PRICE, A.PYM_DISC_CPRICE, A.SOURCECODE, A.PYM_SOURCECODE, A.UNITRATE, A.M_CONTID, A.CASENO,
                                                        A.SOURCECODE_NAME, A.CONTID_NAME, A.BASE_UNIT, A.WARBAK, A.PYM_WARBAK, A.M_AGENNO, A.MMNAME_C, 
                                                        A.MMNAME_E, A.MAT_CLASS_SUB, a.chk_time )

                              SELECT E.WH_NO|| ' '||WH_NAME(E.WH_NO) 盤存單位, E.MMCODE 藥材代碼, E.M_AGENNO 廠商代碼, E.MMNAME_C 藥材名稱, E.MMNAME_E 英文品名, E.BASE_UNIT 藥材單位, E.CONT_PRICE 單價, 
                                     E.DISC_CPRICE 優惠後單價, E.PYM_CONT_PRICE 上月單價, E.PYM_DISC_CPRICE 上月優惠後單價, E.UNITRATE 包裝量, 
                                     E.PYM_INV_QTY 上月結存, NVL(E.APL_INQTY_1,0) 進貨入庫, NVL(E.APL_INQTY_2,0) 撥發入庫, E.REJ_OUTQTY 退貨量, 0 AS 軍方消耗,
(case when (e.chk_wh_grade = '1') then 0 
                                           when (e.wh_name like '%供應中心%') then 0
                                           when (e.chk_wh_grade = '2' and e.chk_wh_kind = '0') then e.ALTERED_USE_QTY
                                           else e.USE_QTY_AF_CHK
                                     end) 民眾消耗,
                                     --(E.ORI_USE_QTY + (CASE WHEN(E.GAP <= 0) THEN (E.STORE_QTYC - E.CHK_QTY) ELSE 0 END)) 民眾消耗,
(case when (e.chk_wh_grade = '1') then 0 
                                           when (e.wh_name like '%供應中心%') then 0
                                           when (e.chk_wh_grade = '2' and e.chk_wh_kind = '0') then e.ALTERED_USE_QTY
                                           else e.USE_QTY_AF_CHK
                                     end) 本月總消耗,
                                     0 AS 軍院內退料, E.BAK_INQTY-BAK_OUTQTY AS 民院內退料, E.BAK_INQTY-BAK_OUTQTY AS 本月總院內退料,
                                     --(E.ORI_USE_QTY + (CASE WHEN(E.GAP <= 0) THEN (E.STORE_QTYC - E.CHK_QTY) ELSE 0 END)) 本月總消耗, 
                                    ( {0} ) 應有結存,
                                     --(E.ORI_INV_QTY - (CASE WHEN(E.GAP <= 0) THEN (E.STORE_QTYC - E.CHK_QTY) ELSE 0 END)) 應有結存, 
                                     E.CHK_QTY 盤存量, E.CHK_QTY 本月結存,
                                     (CASE WHEN E.PYM_SOURCECODE = 'C' THEN E.PYM_INV_QTY ELSE 0 END) p_c_qty, -- 上月寄庫藥品買斷結存
                                     (CASE WHEN E.SOURCECODE = 'C' THEN E.CHK_QTY ELSE 0 END) c_c_qty, --本月寄庫藥品買斷結存,
e.inventory 差異量,
                                     E.CONT_PRICE* E.CHK_QTY 結存金額, E.DISC_CPRICE* E.CHK_QTY 優惠後結存金額,
 e.inventory * e.cont_price 差異金額,
e.inventory * e.DISC_CPRICE 優惠後差異金額,
                                     E.ADJ_INQTY- E.ADJ_OUTQTY AS 平行調撥量, 0 AS 折讓金額,
                                     get_param('MI_MAST', 'MAT_CLASS_SUB', E.MAT_CLASS_SUB) 藥材類別, 
                                     E.CONTID_NAME 是否合約, E.SOURCECODE_NAME 買斷寄庫, E.WARBAK 是否戰備, E.WAR_QTY 戰備存量,
                                     ROUND((CASE WHEN (E.ORI_USE_QTY + (CASE WHEN (E.GAP <= 0) THEN (E.STORE_QTYC - E.CHK_QTY) 
                                                                             ELSE 0 END)) = 0 
                                                 THEN 0
                                                 ELSE (E.CHK_QTY / (E.ORI_USE_QTY + (CASE WHEN (E.GAP <= 0) THEN (E.STORE_QTYC - E.CHK_QTY) 
                                                                                          ELSE 0 END))) END), 2) 期末比,
                                     0 贈品數量, E.PYM_WAR_QTY 上月戰備量, E.PYM_WARBAK 上月是否戰備, 
                                     (CASE WHEN (E.PYM_INV_QTY - E.PYM_WAR_QTY) < 0 THEN 0 
                                           ELSE E.PYM_INV_QTY - E.PYM_WAR_QTY END) no_w_p_i, --不含戰備上月結存, 
                                     E.DISC_CPRICE - E.PYM_DISC_CPRICE 單價差額, 
                                     (CASE WHEN (E.PYM_INV_QTY - E.PYM_WAR_QTY) < 0 THEN 0
                                           ELSE E.PYM_INV_QTY - E.PYM_WAR_QTY END) *(E.DISC_CPRICE - E.PYM_DISC_CPRICE) no_w_p_i_dif, -- 不含戰備上月結存價差,
                                     (CASE WHEN E.WAR_QTY = 0 THEN 0
                                           WHEN E.CHK_QTY <= E.WAR_QTY THEN E.CHK_QTY * (E.DISC_CPRICE - E.PYM_DISC_CPRICE)
                                           ELSE (E.CHK_QTY - E.WAR_QTY) * (E.DISC_CPRICE - E.PYM_DISC_CPRICE) END) 戰備本月價差,
                                     (CASE WHEN E.CHK_QTY <= E.WAR_QTY THEN 0 ELSE E.CHK_QTY - E.WAR_QTY END) 不含戰備本月結存,
                                     (case when E.WAR_QTY > e.chk_qty then 0 
                                       else
                                        (
                                              (CASE WHEN (E.CHK_WH_GRADE = '1') THEN E.STORE_QTYC
                                                 WHEN (E.WH_NAME LIKE '%供應中心%') THEN E.STORE_QTYC
                                                 WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN (
                                                      (e.pym_inv_qty -- 上月庫存數量 
                                                                  +e.apl_inqty   -- 月結入庫總量 
                                                                  -e.apl_outqty  -- 月結撥發總量 
                                                                  +e.trn_inqty   -- 月結調撥入總量 
                                                                  -e.trn_outqty  -- 月結調撥入總量 
                                                                  +e.adj_inqty   -- 月結調帳入總量 
                                                                  -e.adj_outqty  -- 月結調帳出總量 
                                                                  +e.bak_inqty   -- 月結繳回入庫總量 
                                                                  -e.bak_outqty  -- 月結繳回出庫總量 
                                                                  -e.rej_outqty  -- 月結退貨總量 
                                                                  -e.dis_outqty  -- 月結報廢總量
                                                                  +e.exg_inqty   -- 月結換貨入庫總量 
                                                                  -e.exg_outqty  -- 月結換貨出庫總量 
                                                                  -nvl( 
                                                                      nvl(e.altered_use_qty, e.ori_use_qty), -- 調整消耗(只有藥局可以用), 月結耗用量=醫令扣庫 
                                                                      0 
                                                                  ) )
                                                      )
                                                 ELSE E.STORE_QTYC - (CASE WHEN E.CHK_TIME IS NOT NULL THEN E.USE_QTY_AF_CHK
                                                                           ELSE E.ORI_USE_QTY
                                                                      END)
                                            END) - nvl(e.war_qty, 0)
                                       ) 
                                     end) no_w_c_i, -- 不含戰備本月應有結存量,
                                     (CASE WHEN E.CHK_QTY <= E.WAR_QTY THEN 0 ELSE E.CHK_QTY - E.WAR_QTY END) no_w_c_c, --不含戰備本月盤存量,
                                     (E.APL_INQTY - 0) 不含贈品本月進貨
                              FROM INVMON_DATA E WHERE 1 = 1 "
                , temp);
            }
            else if (p1 == "3" || p1 == "4")//盤存單位 選 全部藥局/全院
            {
                sql = @"        WITH WH_NOS AS ( SELECT A.WH_NO, WH_KIND 
                                                 FROM MI_WHID A, MI_WHMAST B 
                                                 WHERE (A.WH_USERID= :USER_NAME AND A.WH_NO = B.WH_NO)),
                              MAT_WH_NO_ALL AS ( SELECT WH_NO, WH_KIND
                                                 FROM MI_WHMAST 
                                                 WHERE WH_KIND = '1'
                                                 AND EXISTS (SELECT 1
                                                             FROM UR_UIR 
                                                             WHERE TUSER = :USER_NAME 
                                                             AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14','MMSpl_14'))), 
                              MED_WH_NO_ALL AS ( SELECT WH_NO, WH_KIND 
                                                 FROM MI_WHMAST 
                                                 WHERE WH_KIND = '0'
                                                 AND EXISTS (SELECT 1 
                                                             FROM UR_UIR
                                                             WHERE TUSER = :USER_NAME 
                                                             AND RLNO IN ('ADMG', 'ADMG_14', 'MED_14','PHR_14'))),
                                      INIDS AS ( SELECT C.INID  
                                                 FROM MI_WHID A, MI_WHMAST B, UR_INID C 
                                                 WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO) 
                                                 AND B.INID = C.INID), 
                                   INID_ALL AS ( SELECT B.INID  
                                                 FROM MI_WHMAST A, UR_INID B  
                                                 WHERE EXISTS (SELECT 1  
                                                               FROM UR_UIR  
                                                               WHERE TUSER = :USER_NAME  
                                                               AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14','MED_14','MMSpl_14'))),
                                MAT_WH_KIND AS ( SELECT DISTINCT 1 AS WH_KIND, '衛材' AS TEXT  
                                                 FROM MI_WHID A, MI_WHMAST B  
                                                 WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO AND B.WH_KIND = '1')  
                                                 OR EXISTS (SELECT 1
                                                            FROM UR_UIR
                                                            WHERE TUSER = :USER_NAME
                                                            AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14','MMSpl_14'))), 
                                MED_WH_KIND AS ( SELECT DISTINCT 0 AS WH_KIND, '藥品' AS TEXT  
                                                 FROM MI_WHID A, MI_WHMAST B  
                                                 WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO AND B.WH_KIND = '0')  
                                                 OR EXISTS (SELECT 1   
                                                            FROM UR_UIR   
                                                            WHERE TUSER = :USER_NAME  
                                                            AND RLNO IN ('ADMG', 'ADMG_14', 'MED_14','PHR_14'))), 
                                   CHK_DATA AS ( SELECT B.CHK_WH_NO, B.CHK_WH_KIND, B.CHK_WH_GRADE, B.CHK_YM, A.MMCODE, A.CHK_QTY, A.STORE_QTYC,
                                                        NVL((A.CHK_QTY - A.STORE_QTYC), 0) GAP, nvl(a.WAR_QTY, 0) AS WAR_QTY, nvl(a.pym_war_qty, 0) AS PYM_WAR_QTY,
                                                        LAST_INVMON(:SET_YM, B.CHK_WH_NO, A.MMCODE) AS PYM_INV_QTY, C.m_contprice as cont_price, C.DISC_CPRICE,
                                                        NVL(D.m_contprice, 0) AS PYM_CONT_PRICE, NVL(D.DISC_CPRICE, 0) AS PYM_DISC_CPRICE, C.e_SOURCECODE as sourcecode,
                                                        NVL(D.e_SOURCECODE, 'P') AS PYM_SOURCECODE, C.UNITRATE, C.M_CONTID, WH_NAME(B.CHK_WH_NO) AS WH_NAME,
                                                        GET_PARAM('MI_MAST','E_SOURCECODE', C.e_SOURCECODE) AS SOURCECODE_NAME, GET_PARAM('MI_MAST','M_CONTID',
                                                        C.M_CONTID) AS CONTID_NAME, NVL(C.BASE_UNIT, A.BASE_UNIT) BASE_UNIT,
                                                        nvl(a.apl_inqty, 0) apl_inqty , nvl(a.apl_outqty, 0) apl_outqty, nvl(a.TRN_INQTY, 0) TRN_INQTY, 
                                                        nvl(a.TRN_OUTQTY, 0) TRN_OUTQTY, nvl(a.ADJ_INQTY, 0) ADJ_INQTY, nvl(a.ADJ_OUTQTY, 0) ADJ_OUTQTY,
                                                        nvl(a.BAK_INQTY, 0) BAK_INQTY, nvl(a.BAK_OUTQTy, 0) BAK_OUTQTy, nvl(a.REJ_OUTQTY, 0) REJ_OUTQTY, 
                                                        nvl(a.DIS_OUTQTY, 0) DIS_OUTQTY, nvl(a.EXG_INQTy, 0) EXG_INQTy, nvl(a.EXG_OUTQTY, 0) EXG_OUTQTY,
                                                        nvl(a.use_qty, 0) use_qty, nvl(a.use_qty_af_chk, 0) use_qty_af_chk, nvl(inventory, 0) inventory, 
                                                        nvl(a.mil_use_qty, 0) mil_use_qty, nvl(a.civil_use_qty, 0) civil_use_qty, nvl(a.ALTERED_USE_QTY, 0) ALTERED_USE_QTY, 
                                                        a.chk_time, C.CASENO,
                                                        c.WARBAK, nvl(d.WARBAK, ' ') as pym_warbak, c.m_agenno, c.mmname_c, c.mmname_e, c.mat_class_sub,A.g34_max_appqty ,A.est_appqty 
                                                 FROM CHK_MAST B, CHK_DETAIL A, MI_MAST_MON c
                                                      left join MI_MAST_MON d on (d.mmcode = c.mmcode and d.data_ym = twn_pym(c.data_ym))
                                                 WHERE B.CHK_YM = :SET_YM AND B.CHK_NO = A.CHK_NO
                                                 and a.mmcode= c.mmcode and b.chk_ym=c.data_ym
                                                 AND B.CHK_LEVEL = '1' ";
                if (p17 == "true")
                {
                    sql += @" AND A.inventory <> 0";
                }
                if (p21 == "true")
                {
                    sql += @"                  AND (A.WAR_QTY <> 0 and a.chk_qty < A.WAR_QTY)";
                }

                if (p18 == "true")
                {
                    sql += @"and (
exists (select 1 from mi_winvmon
  where mmcode=a.mmcode
    and DATA_YM>=
        substr(twn_date(ADD_MONTHS(twn_todate((select max(set_ym) from mi_mnset where set_status='C')||'01'),-5)),1,5)
    and (APL_INQTY>0 or TRN_INQTY>0 or USE_QTY>0)
    and wh_no=a.wh_no)
or 
exists (select 1 from mi_winvmon
  where mmcode=a.mmcode
    and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
    and INV_QTY<>0
    and wh_no=a.wh_no)
or
exists (select 1 from mi_winvmon b
  where mmcode=a.mmcode
    and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
    and INV_QTY=0
    and wh_no=a.wh_no
    and exists(select 1 from mi_mast_mon where mmcode=b.mmcode
          and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
          and CANCEL_ID='N')
  )
)";
                }

                if (p19 == "true")
                {
                    sql += @"
                    and (
                        NVL(a.PYM_INV_QTY,0) > 0
                        or
                        (NVL(a.PYM_INV_QTY,0) = 0 and not (a.apl_inqty = 0                       
                                                   and a.trn_inqty = 0        
                                                   and a.trn_outqty = 0        
                                                   and a.adj_inqty = 0        
                                                   and a.adj_outqty = 0        
                                                   and a.bak_outqty = 0
                                                    and a.bak_inqty=0
                                                    and nvl(nvl(a.altered_use_qty, a.use_qty),0)=0
                                                )
                        )
                    )
                ";
                }

                if (p1 == "3") //若盤存單位 選 全部藥局
                {
                    sql += @"                    AND B.CHK_WH_KIND = '0' 
                                                 AND B.CHK_WH_GRADE = '2' ";
                }

                sql += @"                        AND A.MMCODE = NVL(TRIM(:MMCODE_TEXT),A.MMCODE) 
                                                 AND C.M_CONTID = NVL(TRIM(:M_CONTID_TEXT),C.M_CONTID) 
                                                 AND C.e_SOURCECODE = NVL(TRIM(:SOURCECODE_TEXT),C.e_SOURCECODE) ";
                if (p5 == "A")//若藥材類別選A
                {
                    sql += @"                   AND C.MAT_CLASS = '01' ";
                }
                else if (p5 == "B")//若藥材類別選B
                {
                    sql += @"                   AND C.MAT_CLASS = '02' ";
                }
                else if (p5 != "A" && p5 != "B" && p5.Trim() != "")//若藥材類別選其他
                {
                    sql += @"                   AND C.MAT_CLASS_SUB = NVL(TRIM(:MAT_CLASS_SUB_TEXT),C.MAT_CLASS_SUB) ";
                }

                sql += string.Format(@"         AND C.WARBAK = NVL(TRIM(:WARBAK_TEXT),C.WARBAK)
                                                AND C.E_RESTRICTCODE = NVL(TRIM(:E_RESTRICTCODE_TEXT),C.E_RESTRICTCODE)
                                                AND C.COMMON = NVL(TRIM(:COMMON_TEXT),C.COMMON)
                                                AND C.FASTDRUG = NVL(TRIM(:FASTDRUG_TEXT),C.FASTDRUG)
                                                AND C.DRUGKIND = NVL(TRIM(:DRUGKIND_TEXT),C.DRUGKIND)
                                                AND C.TOUCHCASE = NVL(TRIM(:TOUCHCASE_TEXT),C.TOUCHCASE)
                                                AND C.ORDERKIND = NVL(TRIM(:ORDERKIND_TEXT),C.ORDERKIND)
                                                AND C.SPDRUG = NVL(TRIM(:SPDRUG_TEXT),C.SPDRUG)),
                                INVMON_DATA AS ( SELECT '{0}' as WH_NO, A.WH_NAME, A.MMCODE, SUM(NVL(a.store_qtyc, 0)) AS ORI_INV_QTY, 
                                                      SUM(A.APL_INQTY) AS APL_INQTY,
                                                      (CASE WHEN A.CHK_WH_GRADE = '1' THEN SUM(A.APL_INQTY) else 0 END) AS APL_INQTY_1,
                                                      (CASE WHEN A.CHK_WH_GRADE != '1' THEN SUM(A.APL_INQTY) else 0 END) AS APL_INQTY_2,
                                                      SUM(A.APL_OUTQTY) AS APL_OUTQTY, SUM(A.TRN_INQTY) AS TRN_INQTY, 
                                                      SUM(A.TRN_OUTQTY) AS TRN_OUTQTY, SUM(A.ADJ_INQTY) AS ADJ_INQTY, SUM(A.ADJ_OUTQTY) AS ADJ_OUTQTY,
                                                      SUM(A.BAK_INQTY) AS BAK_INQTY, SUM(A.BAK_OUTQTY) AS BAK_OUTQTY, SUM(A.REJ_OUTQTY) AS REJ_OUTQTY, 
                                                      SUM(A.DIS_OUTQTY) AS DIS_OUTQTY, SUM(A.EXG_INQTY) AS EXG_INQTY, SUM(A.EXG_OUTQTY) AS EXG_OUTQTY,
                                                      SUM(NVL(a.use_qty, 0)) AS ORI_USE_QTY, sum(nvl(a.use_qty_af_chk, 0)) as use_qty_af_chk, 
                                                      sum(nvl(a.inventory, 0)) as inventory, sum(nvl(a.mil_use_qty, 0)) as mil_use_qty, 
                                                      sum(nvl(a.civil_use_qty, 0)) as civil_use_qty,
                                                      sum(nvl(a.ALTERED_USE_QTY, 0)) as ALTERED_USE_QTY, a.chk_time,
                                                      A.CHK_WH_KIND, A.CHK_WH_GRADE,A.CHK_YM, 
                                                      SUM(A.CHK_QTY) AS CHK_QTY, SUM(A.STORE_QTYC) AS STORE_QTYC, SUM(A.GAP) AS GAP, 
                                                      SUM(A.WAR_QTY) AS WAR_QTY, SUM(A.PYM_WAR_QTY) AS PYM_WAR_QTY, SUM(A.PYM_INV_QTY) AS PYM_INV_QTY, 
                                                      A.CONT_PRICE, A.DISC_CPRICE, A.PYM_CONT_PRICE, A.PYM_DISC_CPRICE, A.SOURCECODE, A.PYM_SOURCECODE,
                                                      A.UNITRATE, A.M_CONTID, A.SOURCECODE_NAME, A.CONTID_NAME, A.BASE_UNIT, A.WARBAK, A.PYM_WARBAK, 
                                                      A.M_AGENNO, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS_SUB, A.CASENO,
                                                      SUM(A.g34_max_appqty) AS G34_MAX_APPQTY ,SUM(A.est_appqty ) AS EST_APPQTY 
                                               FROM CHK_DATA A
                                               GROUP BY A.WH_NAME, A.MMCODE, A.CHK_WH_KIND, A.CHK_WH_GRADE, A.CHK_YM,A.CONT_PRICE, A.DISC_CPRICE,
                                                        A.PYM_CONT_PRICE, A.PYM_DISC_CPRICE, A.SOURCECODE, A.PYM_SOURCECODE, A.UNITRATE, A.M_CONTID, A.CASENO,
                                                        A.SOURCECODE_NAME, A.CONTID_NAME, A.BASE_UNIT, A.WARBAK, A.PYM_WARBAK, A.M_AGENNO, A.MMNAME_C, 
                                                        A.MMNAME_E, A.MAT_CLASS_SUB, a.chk_time)

                                SELECT E.WH_NO|| ' '||WH_NAME(E.WH_NO) 盤存單位, E.MMCODE 藥材代碼, E.M_AGENNO 廠商代碼, E.MMNAME_C 藥材名稱, E.MMNAME_E 英文品名, E.BASE_UNIT 藥材單位, E.CONT_PRICE 單價, 
                                     E.DISC_CPRICE 優惠後單價, E.PYM_CONT_PRICE 上月單價, E.PYM_DISC_CPRICE 上月優惠後單價, E.UNITRATE 包裝量, 
                                     E.PYM_INV_QTY 上月結存, NVL(E.APL_INQTY_1,0) 進貨入庫, NVL(E.APL_INQTY_2,0) 撥發入庫, E.APL_OUTQTY 撥發出庫, E.TRN_INQTY 調撥入庫, E.TRN_OUTQTY 調撥出庫,
                                     E.ADJ_INQTY 調帳入庫, E.ADJ_OUTQTY 調帳出庫, E.BAK_INQTY 退料入庫, E.BAK_OUTQTY 退料出庫, E.REJ_OUTQTY 退貨量, 
                                     E.DIS_OUTQTY 報廢量, E.EXG_INQTY 換貨入庫, E.EXG_OUTQTY 換貨出庫, 0 AS 軍方消耗,
(case when (e.chk_wh_grade = '1') then 0 
                                           when (e.wh_name like '%供應中心%') then 0
                                           when (e.chk_wh_grade = '2' and e.chk_wh_kind = '0') then e.ALTERED_USE_QTY
                                           else e.USE_QTY_AF_CHK
                                     end) 民眾消耗,
(case when (e.chk_wh_grade = '1') then 0 
                                           when (e.wh_name like '%供應中心%') then 0
                                           when (e.chk_wh_grade = '2' and e.chk_wh_kind = '0') then e.ALTERED_USE_QTY
                                           else e.USE_QTY_AF_CHK
                                     end) 本月總消耗,
 
                                    ( {1} ) 應有結存,
                                     E.CHK_QTY 盤存量, E.CHK_QTY 本月結存,
                                     (CASE WHEN E.PYM_SOURCECODE = 'C' THEN E.PYM_INV_QTY ELSE 0 END) p_c_qty, -- 上月寄庫藥品買斷結存
                                     (CASE WHEN E.SOURCECODE = 'C' THEN E.CHK_QTY ELSE 0 END) c_c_qty, --本月寄庫藥品買斷結存,
e.inventory 差異量,
                                     E.CONT_PRICE* E.CHK_QTY 結存金額, E.DISC_CPRICE* E.CHK_QTY 優惠後結存金額,
 e.inventory * e.cont_price 差異金額,
e.inventory * e.DISC_CPRICE 優惠後差異金額,
                                     get_param('MI_MAST', 'MAT_CLASS_SUB', E.MAT_CLASS_SUB) 藥材類別, 
                                     E.CONTID_NAME 是否合約, E.SOURCECODE_NAME 買斷寄庫, E.WARBAK 是否戰備, E.WAR_QTY 戰備存量,
                                     ROUND((CASE WHEN (E.ORI_USE_QTY + (CASE WHEN (E.GAP <= 0) THEN (E.STORE_QTYC - E.CHK_QTY) 
                                                                             ELSE 0 END)) = 0 
                                                 THEN 0
                                                 ELSE (E.CHK_QTY / (E.ORI_USE_QTY + (CASE WHEN (E.GAP <= 0) THEN (E.STORE_QTYC - E.CHK_QTY) 
                                                                                          ELSE 0 END))) END), 2) 期末比,
                                     0 贈品數量, E.PYM_WAR_QTY 上月戰備量, E.PYM_WARBAK 上月是否戰備, 
                                     (CASE WHEN (E.PYM_INV_QTY - E.PYM_WAR_QTY) < 0 THEN 0 
                                           ELSE E.PYM_INV_QTY - E.PYM_WAR_QTY END) no_w_p_i, --不含戰備上月結存, 
                                     E.DISC_CPRICE - E.PYM_DISC_CPRICE 單價差額, 
                                     (CASE WHEN (E.PYM_INV_QTY - E.PYM_WAR_QTY) < 0 THEN 0
                                           ELSE E.PYM_INV_QTY - E.PYM_WAR_QTY END) *(E.DISC_CPRICE - E.PYM_DISC_CPRICE) no_w_p_i_dif, -- 不含戰備上月結存價差,
                                     (CASE WHEN E.WAR_QTY=0 THEN 0
                                           WHEN E.CHK_QTY <= E.WAR_QTY THEN E.CHK_QTY * (E.DISC_CPRICE - E.PYM_DISC_CPRICE)
                                           ELSE (E.CHK_QTY - E.WAR_QTY) * (E.DISC_CPRICE - E.PYM_DISC_CPRICE) END) 戰備本月價差,
                                     (CASE WHEN E.CHK_QTY <= E.WAR_QTY THEN 0 ELSE E.CHK_QTY - E.WAR_QTY END) 不含戰備本月結存,
                                     (case when E.WAR_QTY > e.chk_qty then 0 
                                       else
                                        (
                                              (CASE WHEN (E.CHK_WH_GRADE = '1') THEN E.STORE_QTYC
                                                 WHEN (E.WH_NAME LIKE '%供應中心%') THEN E.STORE_QTYC
                                                 WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN (
                                                      (e.pym_inv_qty -- 上月庫存數量 
                                                                  +e.apl_inqty   -- 月結入庫總量 
                                                                  -e.apl_outqty  -- 月結撥發總量 
                                                                  +e.trn_inqty   -- 月結調撥入總量 
                                                                  -e.trn_outqty  -- 月結調撥入總量 
                                                                  +e.adj_inqty   -- 月結調帳入總量 
                                                                  -e.adj_outqty  -- 月結調帳出總量 
                                                                  +e.bak_inqty   -- 月結繳回入庫總量 
                                                                  -e.bak_outqty  -- 月結繳回出庫總量 
                                                                  -e.rej_outqty  -- 月結退貨總量 
                                                                  -e.dis_outqty  -- 月結報廢總量
                                                                  +e.exg_inqty   -- 月結換貨入庫總量 
                                                                  -e.exg_outqty  -- 月結換貨出庫總量 
                                                                  -nvl( 
                                                                      nvl(e.altered_use_qty, e.ori_use_qty), -- 調整消耗(只有藥局可以用), 月結耗用量=醫令扣庫 
                                                                      0 
                                                                  ) )
                                                      )
                                                 ELSE E.STORE_QTYC - (CASE WHEN E.CHK_TIME IS NOT NULL THEN E.USE_QTY_AF_CHK
                                                                           ELSE E.ORI_USE_QTY
                                                                      END)
                                            END) - nvl(e.war_qty, 0)
                                       ) 
                                     end) no_w_c_i, -- 不含戰備本月應有結存量,
                                     (CASE WHEN E.CHK_QTY <= E.WAR_QTY THEN 0 ELSE E.CHK_QTY - E.WAR_QTY END) no_w_c_c, --不含戰備本月盤存量,
                                     (E.APL_INQTY - 0) 不含贈品本月進貨,
                                     E.G34_MAX_APPQTY 單位申請基準量 ,E.EST_APPQTY  下月預計申請量,
                                    E.BAK_INQTY 退料入庫, BAK_OUTQTY 退料出庫, 
                                                                           (CASE WHEN E.DISC_CPRICE >= 5000 THEN (CASE WHEN (E.CHK_WH_GRADE = '1') THEN 0 
                                                                                   WHEN (E.WH_NAME LIKE '%供應中心%') THEN 0
                                                                                   WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN E.ALTERED_USE_QTY
                                                                                   ELSE E.USE_QTY_AF_CHK
                                                                              END)
                                             ELSE 0 
                                        END) 單價5000元以上總消耗, 
                                      (CASE WHEN E.DISC_CPRICE < 5000 THEN (CASE WHEN (E.CHK_WH_GRADE = '1') THEN 0 
                                                                                  WHEN (E.WH_NAME LIKE '%供應中心%') THEN 0
                                                                                  WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN E.ALTERED_USE_QTY
                                                                                  ELSE E.USE_QTY_AF_CHK
                                                                             END)
                                             ELSE 0 
                                        END) 單價未滿5000元總消耗,
                                        E.CASENO 合約案號
                              FROM INVMON_DATA E WHERE 1 = 1"
                                , p1 == "3" ? "全院藥局" : "全院"
                                , temp);
            }
            #endregion

            p.Add(":USER_NAME", user);
            p.Add(":SET_YM", p0); //月結年月
            p.Add(":RLNO_TEXT", p1); //盤存單位
            p.Add(":CHK_WH_KIND_TEXT", p2); //庫房類別
            p.Add(":CHK_WH_NO_TEXT", p3); //庫房代碼
            p.Add(":INID_TEXT", p4); //責任中心
            p.Add(":MAT_CLASS_SUB_TEXT", p5); //藥材類別
            p.Add(":MMCODE_TEXT", p6); //藥材代碼
            p.Add(":M_CONTID_TEXT", p7); //是否合約
            p.Add(":SOURCECODE_TEXT", p8); //買斷寄庫
            p.Add(":WARBAK_TEXT", p9); //是否戰備
            p.Add(":E_RESTRICTCODE_TEXT", p10); //管制品項
            p.Add(":COMMON_TEXT", p11); //是否常用品項
            p.Add(":FASTDRUG_TEXT", p12); //急救品項
            p.Add(":DRUGKIND_TEXT", p13); //中西藥別
            p.Add(":TOUCHCASE_TEXT", p14); //合約類別
            p.Add(":ORDERKIND_TEXT", p15); //採購類別
            p.Add(":SPDRUG_TEXT", p16); //特殊品項

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<CE0044M> Print(string p0, string p1, string p2, string p3, string p4, string p5, string p6, string p7, string p8, string p9, string p10, 
                                          string p11, string p12, string p13, string p14, string p15, string p16, string p17, string p18, string p19, string user,
                                          string p20, string p21)
        {
            var p = new DynamicParameters();
            var sql = "";

            //應有結存
            string temp = @"(CASE WHEN (E.CHK_WH_GRADE = '1') THEN E.STORE_QTYC
                                               WHEN (E.WH_NAME LIKE '%供應中心%') THEN E.STORE_QTYC
                                               WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN (
                                                    (e.pym_inv_qty -- 上月庫存數量 
                                                                +e.apl_inqty   -- 月結入庫總量 
                                                                -e.apl_outqty  -- 月結撥發總量 
                                                                +e.trn_inqty   -- 月結調撥入總量 
                                                                -e.trn_outqty  -- 月結調撥入總量 
                                                                +e.adj_inqty   -- 月結調帳入總量 
                                                                -e.adj_outqty  -- 月結調帳出總量 
                                                                +e.bak_inqty   -- 月結繳回入庫總量 
                                                                -e.bak_outqty  -- 月結繳回出庫總量 
                                                                -e.rej_outqty  -- 月結退貨總量 
                                                                -e.dis_outqty  -- 月結報廢總量
                                                                +e.exg_inqty   -- 月結換貨入庫總量 
                                                                -e.exg_outqty  -- 月結換貨出庫總量 
                                                                -nvl( 
                                                                    nvl(e.altered_use_qty, e.ori_use_qty), -- 調整消耗(只有藥局可以用), 月結耗用量=醫令扣庫 
                                                                    0 
                                                                ) )
                                                    )
                                               ELSE E.STORE_QTYC - (CASE WHEN E.CHK_TIME IS NOT NULL THEN E.USE_QTY_AF_CHK
                                                                         ELSE E.ORI_USE_QTY
                                                                    END)
                                          END) ";
            if (p20 == "true")
            {
                temp = string.Format(@"
                    (case when E.WAR_QTY > e.chk_qty then 0 
                          else
                            ( {0} - nvl(E.war_qty, 0) )
                    end )
                ", temp);
            }

            #region sql
            //盤存單位 選 各請領單位、各單位消耗結存明細、藥材庫
            if (p1 == "1" || p1 == "2" || p1 == "5")
            {
                sql = @"      WITH WH_NOS AS ( SELECT A.WH_NO, WH_KIND 
                                               FROM MI_WHID A, MI_WHMAST B 
                                               WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO)),
                            MAT_WH_NO_ALL AS ( SELECT WH_NO, WH_KIND 
                                               FROM MI_WHMAST 
                                               WHERE WH_KIND = '1' 
                                               AND EXISTS (SELECT 1 
                                                           FROM UR_UIR  
                                                           WHERE TUSER = :USER_NAME
                                               AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14','MMSpl_14'))),
                            MED_WH_NO_ALL AS ( SELECT WH_NO, WH_KIND  
                                               FROM MI_WHMAST  
                                               WHERE WH_KIND = '0' 
                                               AND EXISTS (SELECT 1 FROM UR_UIR  
                                                           WHERE TUSER = :USER_NAME 
                                                           AND RLNO IN ('ADMG', 'ADMG_14', 'MED_14','MMSpl_14'))),
                                    INIDS AS ( SELECT C.INID  
                                               FROM MI_WHID A, MI_WHMAST B, UR_INID C  
                                               WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO)  
                                               AND B.INID = C.INID), 
                                 INID_ALL AS ( SELECT B.INID  
                                               FROM MI_WHMAST A, UR_INID B 
                                               WHERE EXISTS (SELECT 1  
                                                             FROM UR_UIR 
                                                             WHERE TUSER = :USER_NAME  
                                                             AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14','MED_14','MMSpl_14'))),
                              MAT_WH_KIND AS ( SELECT DISTINCT 1 AS WH_KIND, '衛材' AS TEXT  
                                               FROM MI_WHID A, MI_WHMAST B 
                                               WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO AND B.WH_KIND = '1') 
                                               OR EXISTS (SELECT 1 
                                                          FROM UR_UIR  
                                                          WHERE TUSER = :USER_NAME 
                                                          AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14','MMSpl_14'))),
                              MED_WH_KIND AS ( SELECT DISTINCT 0 AS WH_KIND, '藥品' AS TEXT 
                                               FROM MI_WHID A, MI_WHMAST B 
                                               WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO AND B.WH_KIND = '1') 
                                               OR EXISTS (SELECT 1  
                                                          FROM UR_UIR 
                                                          WHERE TUSER = :USER_NAME 
                                                          AND RLNO IN ('ADMG', 'ADMG_14', 'MED_14','MMSpl_14'))),
                                 CHK_DATA AS ( SELECT B.CHK_WH_NO, B.CHK_WH_KIND, B.CHK_WH_GRADE, B.CHK_YM, A.MMCODE, A.CHK_QTY, A.STORE_QTYC,
                                                      NVL((A.CHK_QTY - A.STORE_QTYC), 0) GAP, nvl(a.WAR_QTY, 0) AS WAR_QTY, nvl(a.pym_war_qty, 0) AS PYM_WAR_QTY,
                                                      LAST_INVMON(:SET_YM, B.CHK_WH_NO, A.MMCODE) AS PYM_INV_QTY, C.m_contprice as cont_price, C.DISC_CPRICE,
                                                      NVL(D.m_CONTPRICE, 0) AS PYM_CONT_PRICE, NVL(D.DISC_CPRICE, 0) AS PYM_DISC_CPRICE, C.e_SOURCECODE as sourcecode,
                                                      NVL(D.e_SOURCECODE, 'P') AS PYM_SOURCECODE, C.UNITRATE, C.M_CONTID, WH_NAME(B.CHK_WH_NO) AS WH_NAME,
                                                      GET_PARAM('MI_MAST','E_SOURCECODE', C.e_SOURCECODE) AS SOURCECODE_NAME, GET_PARAM('MI_MAST','M_CONTID',
                                                      C.M_CONTID) AS CONTID_NAME, NVL(C.BASE_UNIT, A.BASE_UNIT) BASE_UNIT,
                                                      nvl(a.apl_inqty, 0) apl_inqty , nvl(a.apl_outqty, 0) apl_outqty, nvl(a.TRN_INQTY, 0) TRN_INQTY, 
                                                      nvl(a.TRN_OUTQTY, 0) TRN_OUTQTY, nvl(a.ADJ_INQTY, 0) ADJ_INQTY, nvl(a.ADJ_OUTQTY, 0) ADJ_OUTQTY,
                                                      nvl(a.BAK_INQTY, 0) BAK_INQTY, nvl(a.BAK_OUTQTy, 0) BAK_OUTQTy, nvl(a.REJ_OUTQTY, 0) REJ_OUTQTY, 
                                                      nvl(a.DIS_OUTQTY, 0) DIS_OUTQTY, nvl(a.EXG_INQTy, 0) EXG_INQTy, nvl(a.EXG_OUTQTY, 0) EXG_OUTQTY,
                                                      nvl(a.use_qty, 0) use_qty, nvl(a.use_qty_af_chk, 0) use_qty_af_chk, nvl(inventory, 0) inventory, 
                                                      nvl(a.mil_use_qty, 0) mil_use_qty, nvl(a.civil_use_qty, 0) civil_use_qty, nvl(a.ALTERED_USE_QTY, 0) ALTERED_USE_QTY, 
                                                     a.chk_time, C.CASENO,
                                                      c.WARBAK, nvl(d.WARBAK, ' ') as pym_warbak, c.m_agenno, c.mmname_c, c.mmname_e, c.mat_class_sub,A.g34_max_appqty ,A.est_appqty 
                                               FROM CHK_MAST B, CHK_DETAIL A, MI_MAST_MON c
                                                    left join MI_MAST_MON d on (d.mmcode = c.mmcode and d.data_ym = twn_pym(c.data_ym))
                                               WHERE B.CHK_YM = :SET_YM AND B.CHK_NO = A.CHK_NO
                                               and a.mmcode= c.mmcode and b.chk_ym=c.data_ym
                                               AND B.CHK_LEVEL = '1'
                                               AND B.CHK_WH_NO IN ( SELECT WH_NO FROM WH_NOS 
                                                                    UNION 
                                                                    SELECT WH_NO FROM MAT_WH_NO_ALL 
                                                                    UNION 
                                                                    SELECT WH_NO FROM MED_WH_NO_ALL)
                                               AND B.CHK_WH_KIND IN ( SELECT WH_KIND FROM MAT_WH_KIND 
                                                                      UNION 
                                                                      SELECT WH_KIND FROM MED_WH_KIND)
                                               AND (EXISTS ( SELECT 1  
                                                             FROM MI_WHMAST A, INIDS B  
                                                             WHERE A.WH_NO = B.CHK_WH_NO  
                                                             AND A.INID = B.INID)  
                                                    OR EXISTS (SELECT 1  
                                                               FROM MI_WHMAST A, INID_ALL B  
                                                               WHERE A.WH_NO = B.CHK_WH_NO  
                                                               AND A.INID = B.INID) ) ";

                if (p17 == "true")
                {
                    sql += @" AND A.inventory <> 0";
                }
                if (p21 == "true")
                {
                    sql += @"                  AND (A.WAR_QTY <> 0 and a.chk_qty < A.WAR_QTY)";
                }

                if (p18 == "true")
                {
                    sql += @"and (
exists (select 1 from mi_winvmon
  where mmcode=a.mmcode
    and DATA_YM>=
        substr(twn_date(ADD_MONTHS(twn_todate((select max(set_ym) from mi_mnset where set_status='C')||'01'),-5)),1,5)
    and (APL_INQTY>0 or TRN_INQTY>0 or USE_QTY>0)
    and wh_no=a.wh_no)
or 
exists (select 1 from mi_winvmon
  where mmcode=a.mmcode
    and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
    and INV_QTY<>0
    and wh_no=a.wh_no)
or
exists (select 1 from mi_winvmon b
  where mmcode=a.mmcode
    and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
    and INV_QTY=0
    and wh_no=a.wh_no
    and exists(select 1 from mi_mast_mon where mmcode=b.mmcode
          and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
          and CANCEL_ID='N')
  )
)";
                }

                if (p19 == "true")
                {
                    sql += @"
                    and (
                        NVL(a.PYM_INV_QTY,0) > 0
                        or
                        (NVL(a.PYM_INV_QTY,0) = 0 and not (a.apl_inqty = 0                       
                                                   and a.trn_inqty = 0        
                                                   and a.trn_outqty = 0        
                                                   and a.adj_inqty = 0        
                                                   and a.adj_outqty = 0        
                                                   and a.bak_outqty = 0
                                                    and a.bak_inqty=0
                                                    and nvl(nvl(a.altered_use_qty, a.use_qty),0)=0
                                                )
                        )
                    )
                ";
                }

                if (p1 == "1")//若盤存單位 選 各請領單位
                {
                    sql += @"                  AND B.CHK_WH_GRADE <> '1' 
                                               AND B.CHK_WH_KIND = NVL(TRIM(:CHK_WH_KIND_TEXT),B.CHK_WH_KIND)
                                               AND B.CHK_WH_NO = NVL(TRIM(:CHK_WH_NO_TEXT),B.CHK_WH_NO)
                                               AND B.CHK_WH_NO IN (SELECT WH_NO 
                                                                   FROM MI_WHMAST 
                                                                   WHERE INID = NVL(TRIM(:INID_TEXT),INID)) ";
                }
                else if (p1 == "2")//若盤存單位 選 藥材庫
                {
                    sql += @"                  AND B.CHK_WH_NO IN (WHNO_MM1, WHNO_ME1) ";
                }
                else if (p1 == "5")//若盤存單位 選 各單位消耗結存明細
                {
                    sql += @"                  AND B.CHK_WH_GRADE <> '1'  ";
                }

                sql += @"                      AND A.MMCODE = NVL(TRIM(:MMCODE_TEXT),A.MMCODE) 
                                               AND C.M_CONTID = NVL(TRIM(:M_CONTID_TEXT),C.M_CONTID) 
                                               AND C.e_SOURCECODE = NVL(TRIM(:SOURCECODE_TEXT),C.e_SOURCECODE)
                        ";
                if (p5 == "A")//若藥材類別選A
                {
                    sql += @"                   AND C.MAT_CLASS = '01' ";
                }
                else if (p5 == "B")//若藥材類別選B
                {
                    sql += @"                   AND C.MAT_CLASS = '02' ";
                }
                else if (p5 != "A" && p5 != "B" && p5.Trim() != "")//若藥材類別選其他
                {
                    sql += @"                   AND C.MAT_CLASS_SUB = NVL(TRIM(:MAT_CLASS_SUB_TEXT),C.MAT_CLASS_SUB) ";
                }

                sql += string.Format(@" 
                                                AND C.WARBAK = NVL(TRIM(:WARBAK_TEXT),C.WARBAK)
                                                AND C.E_RESTRICTCODE = NVL(TRIM(:E_RESTRICTCODE_TEXT),C.E_RESTRICTCODE)
                                                AND C.COMMON = NVL(TRIM(:COMMON_TEXT),C.COMMON)
                                                AND C.FASTDRUG = NVL(TRIM(:FASTDRUG_TEXT),C.FASTDRUG)
                                                AND C.DRUGKIND = NVL(TRIM(:DRUGKIND_TEXT),C.DRUGKIND)
                                                AND C.TOUCHCASE = NVL(TRIM(:TOUCHCASE_TEXT),C.TOUCHCASE)
                                                AND C.ORDERKIND = NVL(TRIM(:ORDERKIND_TEXT),C.ORDERKIND)
                                                AND C.SPDRUG = NVL(TRIM(:SPDRUG_TEXT),C.SPDRUG)), 
                              INVMON_DATA AS ( SELECT A.CHK_WH_NO as WH_NO, A.WH_NAME, A.MMCODE, SUM(NVL(a.store_qtyc, 0)) AS ORI_INV_QTY, 
                                                      SUM(A.APL_INQTY) AS APL_INQTY, SUM(A.APL_OUTQTY) AS APL_OUTQTY, SUM(A.TRN_INQTY) AS TRN_INQTY, 
                                                      SUM(A.TRN_OUTQTY) AS TRN_OUTQTY, SUM(A.ADJ_INQTY) AS ADJ_INQTY, SUM(A.ADJ_OUTQTY) AS ADJ_OUTQTY,
                                                      SUM(A.BAK_INQTY) AS BAK_INQTY, SUM(A.BAK_OUTQTY) AS BAK_OUTQTY, SUM(A.REJ_OUTQTY) AS REJ_OUTQTY, 
                                                      SUM(A.DIS_OUTQTY) AS DIS_OUTQTY, SUM(A.EXG_INQTY) AS EXG_INQTY, SUM(A.EXG_OUTQTY) AS EXG_OUTQTY,
                                                      SUM(NVL(a.use_qty, 0)) AS ORI_USE_QTY, sum(nvl(a.use_qty_af_chk, 0)) as use_qty_af_chk, 
                                                      sum(nvl(a.inventory, 0)) as inventory, sum(nvl(a.mil_use_qty, 0)) as mil_use_qty, 
                                                      sum(nvl(a.civil_use_qty, 0)) as civil_use_qty,
                                                      sum(nvl(a.ALTERED_USE_QTY, 0)) as ALTERED_USE_QTY, a.chk_time,
                                                      A.CHK_WH_KIND, A.CHK_WH_GRADE,A.CHK_YM, 
                                                      SUM(A.CHK_QTY) AS CHK_QTY, SUM(A.STORE_QTYC) AS STORE_QTYC, SUM(A.GAP) AS GAP, 
                                                      SUM(A.WAR_QTY) AS WAR_QTY, SUM(A.PYM_WAR_QTY) AS PYM_WAR_QTY, SUM(A.PYM_INV_QTY) AS PYM_INV_QTY, 
                                                      A.CONT_PRICE, A.DISC_CPRICE, A.PYM_CONT_PRICE, A.PYM_DISC_CPRICE, A.SOURCECODE, A.PYM_SOURCECODE,
                                                      A.UNITRATE, A.M_CONTID, A.SOURCECODE_NAME, A.CONTID_NAME, A.BASE_UNIT, A.WARBAK, A.PYM_WARBAK, 
                                                      A.M_AGENNO, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS_SUB, A.CASENO,
                                                      SUM(A.g34_max_appqty) AS G34_MAX_APPQTY ,SUM(A.est_appqty ) AS EST_APPQTY 
                                               FROM CHK_DATA A
                                               GROUP BY A.chk_WH_NO, A.WH_NAME, A.MMCODE, A.CHK_WH_KIND, A.CHK_WH_GRADE, A.CHK_YM,A.CONT_PRICE, A.DISC_CPRICE,
                                                        A.PYM_CONT_PRICE, A.PYM_DISC_CPRICE, A.SOURCECODE, A.PYM_SOURCECODE, A.UNITRATE, A.M_CONTID, A.CASENO,
                                                        A.SOURCECODE_NAME, A.CONTID_NAME, A.BASE_UNIT, A.WARBAK, A.PYM_WARBAK, A.M_AGENNO, A.MMNAME_C, 
                                                        A.MMNAME_E, A.MAT_CLASS_SUB, a.chk_time )

                              SELECT E.WH_NO|| ' '||WH_NAME(E.WH_NO) F1, E.MMCODE F2, E.MMNAME_C || E.MMNAME_E F4, E.BASE_UNIT F5, E.CONT_PRICE F6, E.DISC_CPRICE F7, 
                                     E.PYM_INV_QTY F11, E.APL_INQTY F12,
                                     E.REJ_OUTQTY F20, 
                                     (case when (e.chk_wh_grade = '1') then 0 
                                           when (e.wh_name like '%供應中心%') then 0
                                           when (e.chk_wh_grade = '2' and e.chk_wh_kind = '0') then e.ALTERED_USE_QTY
                                           else e.USE_QTY_AF_CHK
                                     end) F25,
                                     ( {0} ) F27, 
                                     E.CHK_QTY F29,
                                     e.inventory F32,
                                     E.CONT_PRICE* E.CHK_QTY F33, E.DISC_CPRICE* E.CHK_QTY F34,
                                     e.inventory * e.cont_price F35
                              FROM INVMON_DATA E WHERE 1 = 1 order by E.WH_NO, E.MMCODE "
                , temp);
            }
            else if (p1 == "3" || p1 == "4")//盤存單位 選 全部藥局/全院
            {
                sql = @"        WITH WH_NOS AS ( SELECT A.WH_NO, WH_KIND 
                                                 FROM MI_WHID A, MI_WHMAST B 
                                                 WHERE (A.WH_USERID= :USER_NAME AND A.WH_NO = B.WH_NO)),
                              MAT_WH_NO_ALL AS ( SELECT WH_NO, WH_KIND
                                                 FROM MI_WHMAST 
                                                 WHERE WH_KIND = '1'
                                                 AND EXISTS (SELECT 1
                                                             FROM UR_UIR 
                                                             WHERE TUSER = :USER_NAME 
                                                             AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14','MMSpl_14'))), 
                              MED_WH_NO_ALL AS ( SELECT WH_NO, WH_KIND 
                                                 FROM MI_WHMAST 
                                                 WHERE WH_KIND = '0'
                                                 AND EXISTS (SELECT 1 
                                                             FROM UR_UIR
                                                             WHERE TUSER = :USER_NAME 
                                                             AND RLNO IN ('ADMG', 'ADMG_14', 'MED_14'))),
                                      INIDS AS ( SELECT C.INID  
                                                 FROM MI_WHID A, MI_WHMAST B, UR_INID C 
                                                 WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO) 
                                                 AND B.INID = C.INID), 
                                   INID_ALL AS ( SELECT B.INID  
                                                 FROM MI_WHMAST A, UR_INID B  
                                                 WHERE EXISTS (SELECT 1  
                                                               FROM UR_UIR  
                                                               WHERE TUSER = :USER_NAME  
                                                               AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14','MED_14','MMSpl_14'))),
                                MAT_WH_KIND AS ( SELECT DISTINCT 1 AS WH_KIND, '衛材' AS TEXT  
                                                 FROM MI_WHID A, MI_WHMAST B  
                                                 WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO AND B.WH_KIND = '1')  
                                                 OR EXISTS (SELECT 1
                                                            FROM UR_UIR
                                                            WHERE TUSER = :USER_NAME
                                                            AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14','MMSpl_14'))), 
                                MED_WH_KIND AS ( SELECT DISTINCT 0 AS WH_KIND, '藥品' AS TEXT  
                                                 FROM MI_WHID A, MI_WHMAST B  
                                                 WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO AND B.WH_KIND = '1')  
                                                 OR EXISTS (SELECT 1   
                                                            FROM UR_UIR   
                                                            WHERE TUSER = :USER_NAME  
                                                            AND RLNO IN ('ADMG', 'ADMG_14', 'MED_14','PHR_14'))), 
                                   CHK_DATA AS ( SELECT B.CHK_WH_NO, B.CHK_WH_KIND, B.CHK_WH_GRADE, B.CHK_YM, A.MMCODE, A.CHK_QTY, A.STORE_QTYC,
                                                        NVL((A.CHK_QTY - A.STORE_QTYC), 0) GAP, nvl(a.WAR_QTY, 0) AS WAR_QTY, nvl(a.pym_war_qty, 0) AS PYM_WAR_QTY,
                                                        LAST_INVMON(:SET_YM, B.CHK_WH_NO, A.MMCODE) AS PYM_INV_QTY, C.m_contprice as cont_price, C.DISC_CPRICE,
                                                        NVL(D.m_contprice, 0) AS PYM_CONT_PRICE, NVL(D.DISC_CPRICE, 0) AS PYM_DISC_CPRICE, C.e_SOURCECODE as sourcecode,
                                                        NVL(D.e_SOURCECODE, 'P') AS PYM_SOURCECODE, C.UNITRATE, C.M_CONTID, WH_NAME(B.CHK_WH_NO) AS WH_NAME,
                                                        GET_PARAM('MI_MAST','E_SOURCECODE', C.e_SOURCECODE) AS SOURCECODE_NAME, GET_PARAM('MI_MAST','M_CONTID',
                                                        C.M_CONTID) AS CONTID_NAME, NVL(C.BASE_UNIT, A.BASE_UNIT) BASE_UNIT,
                                                        nvl(a.apl_inqty, 0) apl_inqty , nvl(a.apl_outqty, 0) apl_outqty, nvl(a.TRN_INQTY, 0) TRN_INQTY, 
                                                        nvl(a.TRN_OUTQTY, 0) TRN_OUTQTY, nvl(a.ADJ_INQTY, 0) ADJ_INQTY, nvl(a.ADJ_OUTQTY, 0) ADJ_OUTQTY,
                                                        nvl(a.BAK_INQTY, 0) BAK_INQTY, nvl(a.BAK_OUTQTy, 0) BAK_OUTQTy, nvl(a.REJ_OUTQTY, 0) REJ_OUTQTY, 
                                                        nvl(a.DIS_OUTQTY, 0) DIS_OUTQTY, nvl(a.EXG_INQTy, 0) EXG_INQTy, nvl(a.EXG_OUTQTY, 0) EXG_OUTQTY,
                                                        nvl(a.use_qty, 0) use_qty, nvl(a.use_qty_af_chk, 0) use_qty_af_chk, nvl(inventory, 0) inventory, 
                                                        nvl(a.mil_use_qty, 0) mil_use_qty, nvl(a.civil_use_qty, 0) civil_use_qty, nvl(a.ALTERED_USE_QTY, 0) ALTERED_USE_QTY, 
                                                        a.chk_time, C.CASENO,
                                                        c.WARBAK, nvl(d.WARBAK, ' ') as pym_warbak, c.m_agenno, c.mmname_c, c.mmname_e, c.mat_class_sub,A.g34_max_appqty ,A.est_appqty 
                                                 FROM CHK_MAST B, CHK_DETAIL A, MI_MAST_MON c
                                                      left join MI_MAST_MON d on (d.mmcode = c.mmcode and d.data_ym = twn_pym(c.data_ym))
                                                 WHERE B.CHK_YM = :SET_YM AND B.CHK_NO = A.CHK_NO
                                                 and a.mmcode= c.mmcode and b.chk_ym=c.data_ym
                                                 AND B.CHK_LEVEL = '1' ";

                if (p17 == "true")
                {
                    sql += @" AND A.inventory <> 0";
                }
                if (p21 == "true")
                {
                    sql += @"                  AND (A.WAR_QTY <> 0 and a.chk_qty < A.WAR_QTY)";
                }

                if (p18 == "true")
                {
                    sql += @"and (
exists (select 1 from mi_winvmon
  where mmcode=a.mmcode
    and DATA_YM>=
        substr(twn_date(ADD_MONTHS(twn_todate((select max(set_ym) from mi_mnset where set_status='C')||'01'),-5)),1,5)
    and (APL_INQTY>0 or TRN_INQTY>0 or USE_QTY>0)
    and wh_no=a.wh_no)
or 
exists (select 1 from mi_winvmon
  where mmcode=a.mmcode
    and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
    and INV_QTY<>0
    and wh_no=a.wh_no)
or
exists (select 1 from mi_winvmon b
  where mmcode=a.mmcode
    and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
    and INV_QTY=0
    and wh_no=a.wh_no
    and exists(select 1 from mi_mast_mon where mmcode=b.mmcode
          and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
          and CANCEL_ID='N')
  )
)";
                }

                if (p19 == "true")
                {
                    sql += @"
                    and (
                        NVL(a.PYM_INV_QTY,0) > 0
                        or
                        (NVL(a.PYM_INV_QTY,0) = 0 and not (a.apl_inqty = 0                       
                                                   and a.trn_inqty = 0        
                                                   and a.trn_outqty = 0        
                                                   and a.adj_inqty = 0        
                                                   and a.adj_outqty = 0        
                                                   and a.bak_outqty = 0
                                                    and a.bak_inqty=0
                                                    and nvl(nvl(a.altered_use_qty, a.use_qty),0)=0
                                                )
                        )
                    )
                ";
                }

                if (p1 == "3") //若盤存單位 選 全部藥局
                {
                    sql += @"                    AND B.CHK_WH_KIND = '0' 
                                                 AND B.CHK_WH_GRADE = '2' ";
                }

                sql += @"                        AND A.MMCODE = NVL(TRIM(:MMCODE_TEXT),A.MMCODE) 
                                                 AND C.M_CONTID = NVL(TRIM(:M_CONTID_TEXT),C.M_CONTID) 
                                                 AND C.e_SOURCECODE = NVL(TRIM(:SOURCECODE_TEXT),C.e_SOURCECODE)
                ";
                if (p5 == "A")//若藥材類別選A
                {
                    sql += @"                   AND C.MAT_CLASS = '01' ";
                }
                else if (p5 == "B")//若藥材類別選B
                {
                    sql += @"                   AND C.MAT_CLASS = '02' ";
                }
                else if (p5 != "A" && p5 != "B" && p5.Trim() != "")//若藥材類別選其他
                {
                    sql += @"                   AND C.MAT_CLASS_SUB = NVL(TRIM(:MAT_CLASS_SUB_TEXT),C.MAT_CLASS_SUB) ";
                }

                sql += string.Format(@"
                                                AND C.WARBAK = NVL(TRIM(:WARBAK_TEXT),C.WARBAK)
                                                AND C.E_RESTRICTCODE = NVL(TRIM(:E_RESTRICTCODE_TEXT),C.E_RESTRICTCODE)
                                                AND C.COMMON = NVL(TRIM(:COMMON_TEXT),C.COMMON)
                                                AND C.FASTDRUG = NVL(TRIM(:FASTDRUG_TEXT),C.FASTDRUG)
                                                AND C.DRUGKIND = NVL(TRIM(:DRUGKIND_TEXT),C.DRUGKIND)
                                                AND C.TOUCHCASE = NVL(TRIM(:TOUCHCASE_TEXT),C.TOUCHCASE)
                                                AND C.ORDERKIND = NVL(TRIM(:ORDERKIND_TEXT),C.ORDERKIND)
                                                AND C.SPDRUG = NVL(TRIM(:SPDRUG_TEXT),C.SPDRUG)),
                              INVMON_DATA AS ( SELECT '{0}' as WH_NO, A.WH_NAME, A.MMCODE, SUM(NVL(a.store_qtyc, 0)) AS ORI_INV_QTY, 
                                                      SUM(A.APL_INQTY) AS APL_INQTY, SUM(A.APL_OUTQTY) AS APL_OUTQTY, SUM(A.TRN_INQTY) AS TRN_INQTY, 
                                                      SUM(A.TRN_OUTQTY) AS TRN_OUTQTY, SUM(A.ADJ_INQTY) AS ADJ_INQTY, SUM(A.ADJ_OUTQTY) AS ADJ_OUTQTY,
                                                      SUM(A.BAK_INQTY) AS BAK_INQTY, SUM(A.BAK_OUTQTY) AS BAK_OUTQTY, SUM(A.REJ_OUTQTY) AS REJ_OUTQTY, 
                                                      SUM(A.DIS_OUTQTY) AS DIS_OUTQTY, SUM(A.EXG_INQTY) AS EXG_INQTY, SUM(A.EXG_OUTQTY) AS EXG_OUTQTY,
                                                      SUM(NVL(a.use_qty, 0)) AS ORI_USE_QTY, sum(nvl(a.use_qty_af_chk, 0)) as use_qty_af_chk, 
                                                      sum(nvl(a.inventory, 0)) as inventory, sum(nvl(a.mil_use_qty, 0)) as mil_use_qty, 
                                                      sum(nvl(a.civil_use_qty, 0)) as civil_use_qty,
                                                      sum(nvl(a.ALTERED_USE_QTY, 0)) as ALTERED_USE_QTY, a.chk_time,
                                                      A.CHK_WH_KIND, A.CHK_WH_GRADE,A.CHK_YM, 
                                                      SUM(A.CHK_QTY) AS CHK_QTY, SUM(A.STORE_QTYC) AS STORE_QTYC, SUM(A.GAP) AS GAP, 
                                                      SUM(A.WAR_QTY) AS WAR_QTY, SUM(A.PYM_WAR_QTY) AS PYM_WAR_QTY, SUM(A.PYM_INV_QTY) AS PYM_INV_QTY, 
                                                      A.CONT_PRICE, A.DISC_CPRICE, A.PYM_CONT_PRICE, A.PYM_DISC_CPRICE, A.SOURCECODE, A.PYM_SOURCECODE,
                                                      A.UNITRATE, A.M_CONTID, A.SOURCECODE_NAME, A.CONTID_NAME, A.BASE_UNIT, A.WARBAK, A.PYM_WARBAK, 
                                                      A.M_AGENNO, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS_SUB, A.CASENO,
                                                       SUM(A.g34_max_appqty) AS G34_MAX_APPQTY ,SUM(A.est_appqty ) AS EST_APPQTY 
                                               FROM CHK_DATA A
                                               GROUP BY A.WH_NAME, A.MMCODE, A.CHK_WH_KIND, A.CHK_WH_GRADE, A.CHK_YM,A.CONT_PRICE, A.DISC_CPRICE,
                                                        A.PYM_CONT_PRICE, A.PYM_DISC_CPRICE, A.SOURCECODE, A.PYM_SOURCECODE, A.UNITRATE, A.M_CONTID, A.CASENO,
                                                        A.SOURCECODE_NAME, A.CONTID_NAME, A.BASE_UNIT, A.WARBAK, A.PYM_WARBAK, A.M_AGENNO, A.MMNAME_C, 
                                                        A.MMNAME_E, A.MAT_CLASS_SUB, a.chk_time )

                                SELECT E.WH_NO|| ' '||WH_NAME(E.WH_NO) F1, E.MMCODE F2, E.MMNAME_C || E.MMNAME_E F4, E.BASE_UNIT F5, E.CONT_PRICE F6, E.DISC_CPRICE F7, 
                                     E.PYM_INV_QTY F11, E.APL_INQTY F12, 
                                     E.REJ_OUTQTY F20, 
                                     (case when (e.chk_wh_grade = '1') then 0 
                                           when (e.wh_name like '%供應中心%') then 0
                                           when (e.chk_wh_grade = '2' and e.chk_wh_kind = '0') then e.ALTERED_USE_QTY
                                           else e.USE_QTY_AF_CHK
                                     end) F25,
                                     ( {1} ) F27, 
                                     E.CHK_QTY F29,
                                     e.inventory F32,
                                     E.CONT_PRICE* E.CHK_QTY F33, E.DISC_CPRICE* E.CHK_QTY F34,
                                     e.inventory * e.cont_price F35
                              FROM INVMON_DATA E WHERE 1 = 1 order by E.WH_NO, E.MMCODE"
            , p1 == "3" ? "全院藥局" : "全院"
            , temp);
            }
            #endregion

            p.Add(":USER_NAME", user);
            p.Add(":SET_YM", p0); //月結年月
            p.Add(":RLNO_TEXT", p1); //盤存單位
            p.Add(":CHK_WH_KIND_TEXT", p2); //庫房類別
            p.Add(":CHK_WH_NO_TEXT", p3); //庫房代碼
            p.Add(":INID_TEXT", p4); //責任中心
            p.Add(":MAT_CLASS_SUB_TEXT", p5); //藥材類別
            p.Add(":MMCODE_TEXT", p6); //藥材代碼
            p.Add(":M_CONTID_TEXT", p7); //是否合約
            p.Add(":SOURCECODE_TEXT", p8); //買斷寄庫
            p.Add(":WARBAK_TEXT", p9); //是否戰備
            p.Add(":E_RESTRICTCODE_TEXT", p10); //管制品項
            p.Add(":COMMON_TEXT", p11); //是否常用品項
            p.Add(":FASTDRUG_TEXT", p12); //急救品項
            p.Add(":DRUGKIND_TEXT", p13); //中西藥別
            p.Add(":TOUCHCASE_TEXT", p14); //合約類別
            p.Add(":ORDERKIND_TEXT", p15); //採購類別
            p.Add(":SPDRUG_TEXT", p16); //特殊品項

            return DBWork.Connection.Query<CE0044M>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<Print2_class> Print2(string p0,  string user, string p20)
        {
            var p = new DynamicParameters();
            var sql = "";

            //應有結存
            string temp = @"(CASE WHEN (E.CHK_WH_GRADE = '1') THEN E.STORE_QTYC
                                               WHEN (E.WH_NAME LIKE '%供應中心%') THEN E.STORE_QTYC
                                               WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN (
                                                    (e.pym_inv_qty -- 上月庫存數量 
                                                                +e.apl_inqty   -- 月結入庫總量 
                                                                -e.apl_outqty  -- 月結撥發總量 
                                                                +e.trn_inqty   -- 月結調撥入總量 
                                                                -e.trn_outqty  -- 月結調撥入總量 
                                                                +e.adj_inqty   -- 月結調帳入總量 
                                                                -e.adj_outqty  -- 月結調帳出總量 
                                                                +e.bak_inqty   -- 月結繳回入庫總量 
                                                                -e.bak_outqty  -- 月結繳回出庫總量 
                                                                -e.rej_outqty  -- 月結退貨總量 
                                                                -e.dis_outqty  -- 月結報廢總量
                                                                +e.exg_inqty   -- 月結換貨入庫總量 
                                                                -e.exg_outqty  -- 月結換貨出庫總量 
                                                                -nvl( 
                                                                    nvl(e.altered_use_qty, e.ori_use_qty), -- 調整消耗(只有藥局可以用), 月結耗用量=醫令扣庫 
                                                                    0 
                                                                ) )
                                                    )
                                               ELSE E.STORE_QTYC - (CASE WHEN E.CHK_TIME IS NOT NULL THEN E.USE_QTY_AF_CHK
                                                                         ELSE E.ORI_USE_QTY
                                                                    END)
                                          END) ";
            if (p20 == "true")
            {
                temp = string.Format(@"
                    (case when E.WAR_QTY > e.chk_qty then 0 
                          else
                            ( {0} - nvl(E.war_qty, 0) )
                    end )
                ", temp);
            }

            #region sql
            sql = string.Format(@"
                          SELECT  F0,F1,
                            SUM(F11*F7) AS S1 ,
                            SUM(F62*F7) AS S2 ,
                            SUM(F63*F7) AS S3 ,
                            SUM(F12_2*F7) AS S4 ,
                            SUM(F20*F7) AS S5 ,
                            SUM(F24*F7) AS S6 ,
                            SUM(F25*F7) AS S7 ,
                            SUM(0) AS S8,
                            SUM( f27 * f7 ) S9,
                            SUM( CASE
                                    WHEN f39 = '買斷' THEN ( f27 * f7 )
                                    ELSE 0
                                  END )      S10,
                            SUM( CASE
                                WHEN f39 = '寄庫' THEN ( f27 * f7 )
                                ELSE 0
                              END )      S11,
                               
                            SUM(F34) AS S12 ,
                            SUM(F64) AS S13 ,
                            SUM(F65) AS S14 ,
                            SUM(F36) AS S15 ,
                            SUM(T18) AS S16 ,
                            SUM(T22) AS S17 ,
                            SUM(T05) AS S18
                FROM   ( 
            ");
            sql += @"      
                              WITH WH_NOS AS ( SELECT A.WH_NO, WH_KIND 
                                               FROM MI_WHID A, MI_WHMAST B 
                                               WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO)),
                            MAT_WH_NO_ALL AS ( SELECT WH_NO, WH_KIND 
                                               FROM MI_WHMAST 
                                               WHERE WH_KIND = '1' 
                                               AND EXISTS (SELECT 1 
                                                           FROM UR_UIR  
                                                           WHERE TUSER = :USER_NAME
                                               AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14', 'MMSpl_14'))),
                            MED_WH_NO_ALL AS ( SELECT WH_NO, WH_KIND  
                                               FROM MI_WHMAST  
                                               WHERE WH_KIND = '0' 
                                               AND EXISTS (SELECT 1 FROM UR_UIR  
                                                           WHERE TUSER = :USER_NAME 
                                                           AND RLNO IN ('ADMG', 'ADMG_14', 'MED_14', 'MMSpl_14', 'PHR_14'))),
                                    INIDS AS ( SELECT C.INID  
                                               FROM MI_WHID A, MI_WHMAST B, UR_INID C  
                                               WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO)  
                                               AND B.INID = C.INID), 
                                 INID_ALL AS ( SELECT B.INID  
                                               FROM MI_WHMAST A, UR_INID B 
                                               WHERE EXISTS (SELECT 1  
                                                             FROM UR_UIR 
                                                             WHERE TUSER = :USER_NAME  
                                                             AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14', 'MED_14', 'MMSpl_14'))),
                              MAT_WH_KIND AS ( SELECT DISTINCT 1 AS WH_KIND, '衛材' AS TEXT  
                                               FROM MI_WHID A, MI_WHMAST B 
                                               WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO AND B.WH_KIND = '1') 
                                               OR EXISTS (SELECT 1 
                                                          FROM UR_UIR  
                                                          WHERE TUSER = :USER_NAME 
                                                          AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14', 'MMSpl_14'))),
                              MED_WH_KIND AS ( SELECT DISTINCT 0 AS WH_KIND, '藥品' AS TEXT 
                                               FROM MI_WHID A, MI_WHMAST B 
                                               WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO AND B.WH_KIND = '1') 
                                               OR EXISTS (SELECT 1  
                                                          FROM UR_UIR 
                                                          WHERE TUSER = :USER_NAME 
                                                          AND RLNO IN ('ADMG', 'ADMG_14', 'MED_14', 'MMSpl_14', 'PHR_14'))),
                                 CHK_DATA AS ( SELECT B.CHK_WH_NO, B.CHK_WH_KIND, B.CHK_WH_GRADE, B.CHK_YM, A.MMCODE, A.CHK_QTY, A.STORE_QTYC,
                                                      NVL((A.CHK_QTY - A.STORE_QTYC), 0) GAP, NVL(A.WAR_QTY, 0) AS WAR_QTY, NVL(A.PYM_WAR_QTY, 0) AS PYM_WAR_QTY,
                                                      LAST_INVMON(:SET_YM, B.CHK_WH_NO, A.MMCODE) AS PYM_INV_QTY, C.M_CONTPRICE AS CONT_PRICE, C.DISC_CPRICE,
                                                      NVL(D.M_CONTPRICE, 0) AS PYM_CONT_PRICE, NVL(D.DISC_CPRICE, 0) AS PYM_DISC_CPRICE, C.E_SOURCECODE AS SOURCECODE,
                                                      NVL(D.E_SOURCECODE, 'P') AS PYM_SOURCECODE, C.UNITRATE, C.M_CONTID, WH_NAME(B.CHK_WH_NO) AS WH_NAME,
                                                      GET_PARAM('MI_MAST','E_SOURCECODE', C.E_SOURCECODE) AS SOURCECODE_NAME, GET_PARAM('MI_MAST','M_CONTID',
                                                      C.M_CONTID) AS CONTID_NAME, NVL(C.BASE_UNIT, A.BASE_UNIT) BASE_UNIT,
                                                      NVL(A.APL_INQTY, 0) APL_INQTY , NVL(A.APL_OUTQTY, 0) APL_OUTQTY, NVL(A.TRN_INQTY, 0) TRN_INQTY, 
                                                      NVL(A.TRN_OUTQTY, 0) TRN_OUTQTY, NVL(A.ADJ_INQTY, 0) ADJ_INQTY, NVL(A.ADJ_OUTQTY, 0) ADJ_OUTQTY,
                                                      NVL(A.BAK_INQTY, 0) BAK_INQTY, NVL(A.BAK_OUTQTY, 0) BAK_OUTQTY, NVL(A.REJ_OUTQTY, 0) REJ_OUTQTY, 
                                                      NVL(A.DIS_OUTQTY, 0) DIS_OUTQTY, NVL(A.EXG_INQTY, 0) EXG_INQTY, NVL(A.EXG_OUTQTY, 0) EXG_OUTQTY,
                                                      NVL(A.USE_QTY, 0) USE_QTY, NVL(A.USE_QTY_AF_CHK, 0) USE_QTY_AF_CHK, NVL(INVENTORY, 0) INVENTORY, 
                                                      NVL(A.MIL_USE_QTY, 0) MIL_USE_QTY, NVL(A.CIVIL_USE_QTY, 0) CIVIL_USE_QTY, NVL(A.ALTERED_USE_QTY, 0) ALTERED_USE_QTY, 
                                                      A.CHK_TIME, C.CASENO,
                                                      C.WARBAK, NVL(D.WARBAK, ' ') AS PYM_WARBAK, C.M_AGENNO, C.MMNAME_C, C.MMNAME_E, C.MAT_CLASS_SUB,A.G34_MAX_APPQTY ,A.EST_APPQTY 
                                               FROM CHK_MAST B, CHK_DETAIL A, MI_MAST_MON C
                                                    LEFT JOIN MI_MAST_MON D ON (D.MMCODE = C.MMCODE AND D.DATA_YM = TWN_PYM(C.DATA_YM))
                                               WHERE B.CHK_YM = :SET_YM AND B.CHK_NO = A.CHK_NO
                                               AND A.MMCODE = C.MMCODE
                                               AND B.CHK_YM = C.DATA_YM
                                               AND B.CHK_LEVEL = '1'
                                               AND B.CHK_WH_NO IN ( SELECT WH_NO FROM WH_NOS 
                                                                    UNION 
                                                                    SELECT WH_NO FROM MAT_WH_NO_ALL 
                                                                    UNION 
                                                                    SELECT WH_NO FROM MED_WH_NO_ALL)
                                               AND B.CHK_WH_KIND IN ( SELECT WH_KIND FROM MAT_WH_KIND 
                                                                      UNION 
                                                                      SELECT WH_KIND FROM MED_WH_KIND)
                                               AND (EXISTS ( SELECT 1  
                                                             FROM MI_WHMAST A, INIDS B  
                                                             WHERE A.WH_NO = B.CHK_WH_NO  
                                                             AND A.INID = B.INID)  
                                                    OR EXISTS (SELECT 1  
                                                               FROM MI_WHMAST A, INID_ALL B  
                                                               WHERE A.WH_NO = B.CHK_WH_NO  
                                                               AND A.INID = B.INID) ) ";

            sql += @"                  AND B.CHK_WH_GRADE <> '1' 
                                               AND B.CHK_WH_KIND = NVL(TRIM(:CHK_WH_KIND_TEXT), B.CHK_WH_KIND)
                                               AND B.CHK_WH_NO = NVL(TRIM(:CHK_WH_NO_TEXT), B.CHK_WH_NO)
                                               AND B.CHK_WH_NO IN (SELECT WH_NO 
                                                                   FROM MI_WHMAST 
                                                                   WHERE INID = NVL(TRIM(:INID_TEXT), INID)) ";
            sql += @"                          AND A.MMCODE = NVL(TRIM(:MMCODE_TEXT), A.MMCODE) 
                                               AND C.M_CONTID = NVL(TRIM(:M_CONTID_TEXT), C.M_CONTID) 
                                               AND C.E_SOURCECODE = NVL(TRIM(:SOURCECODE_TEXT), C.E_SOURCECODE)
                        ";

            sql += string.Format(@" 
                                                AND C.WARBAK = NVL(TRIM(:WARBAK_TEXT), C.WARBAK)
                                                AND C.E_RESTRICTCODE = NVL(TRIM(:E_RESTRICTCODE_TEXT), C.E_RESTRICTCODE)
                                                AND C.COMMON = NVL(TRIM(:COMMON_TEXT), C.COMMON)
                                                AND C.FASTDRUG = NVL(TRIM(:FASTDRUG_TEXT), C.FASTDRUG)
                                                AND C.DRUGKIND = NVL(TRIM(:DRUGKIND_TEXT), C.DRUGKIND)
                                                AND C.TOUCHCASE = NVL(TRIM(:TOUCHCASE_TEXT), C.TOUCHCASE)
                                                AND C.ORDERKIND = NVL(TRIM(:ORDERKIND_TEXT), C.ORDERKIND)
                                                AND C.SPDRUG = NVL(TRIM(:SPDRUG_TEXT), C.SPDRUG)), 
                              INVMON_DATA AS ( SELECT A.CHK_WH_NO AS WH_NO, A.WH_NAME, A.MMCODE, SUM(NVL(A.STORE_QTYC, 0)) AS ORI_INV_QTY, 
                                                      SUM(A.APL_INQTY) AS APL_INQTY,
                                                      (CASE WHEN A.CHK_WH_GRADE = '1' THEN SUM(A.APL_INQTY) else 0 END) AS APL_INQTY_1,
                                                      (CASE WHEN A.CHK_WH_GRADE != '1' THEN SUM(A.APL_INQTY) else 0 END) AS APL_INQTY_2,
                                                      SUM(A.APL_OUTQTY) AS APL_OUTQTY, SUM(A.TRN_INQTY) AS TRN_INQTY, 
                                                      SUM(A.TRN_OUTQTY) AS TRN_OUTQTY, SUM(A.ADJ_INQTY) AS ADJ_INQTY, SUM(A.ADJ_OUTQTY) AS ADJ_OUTQTY,
                                                      SUM(A.BAK_INQTY) AS BAK_INQTY, SUM(A.BAK_OUTQTY) AS BAK_OUTQTY, SUM(A.REJ_OUTQTY) AS REJ_OUTQTY, 
                                                      SUM(A.DIS_OUTQTY) AS DIS_OUTQTY, SUM(A.EXG_INQTY) AS EXG_INQTY, SUM(A.EXG_OUTQTY) AS EXG_OUTQTY,
                                                      SUM(NVL(A.USE_QTY, 0)) AS ORI_USE_QTY, SUM(NVL(A.USE_QTY_AF_CHK, 0)) AS USE_QTY_AF_CHK, 
                                                      SUM(NVL(A.INVENTORY, 0)) AS INVENTORY, SUM(NVL(A.MIL_USE_QTY, 0)) AS MIL_USE_QTY, 
                                                      SUM(NVL(A.CIVIL_USE_QTY, 0)) AS CIVIL_USE_QTY,
                                                      SUM(NVL(A.ALTERED_USE_QTY, 0)) AS ALTERED_USE_QTY, A.CHK_TIME,
                                                      A.CHK_WH_KIND, A.CHK_WH_GRADE,A.CHK_YM, 
                                                      SUM(A.CHK_QTY) AS CHK_QTY, SUM(A.STORE_QTYC) AS STORE_QTYC, SUM(A.GAP) AS GAP, 
                                                      SUM(A.WAR_QTY) AS WAR_QTY, SUM(A.PYM_WAR_QTY) AS PYM_WAR_QTY, SUM(A.PYM_INV_QTY) AS PYM_INV_QTY, 
                                                      A.CONT_PRICE, A.DISC_CPRICE, A.PYM_CONT_PRICE, A.PYM_DISC_CPRICE, A.SOURCECODE, A.PYM_SOURCECODE,
                                                      A.UNITRATE, A.M_CONTID, A.SOURCECODE_NAME, A.CONTID_NAME, A.BASE_UNIT, A.WARBAK, A.PYM_WARBAK, 
                                                      A.M_AGENNO, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS_SUB, A.CASENO,
                                                      SUM(A.G34_MAX_APPQTY) AS G34_MAX_APPQTY ,SUM(A.EST_APPQTY ) AS EST_APPQTY 
                                               FROM CHK_DATA A
                                               GROUP BY A.CHK_WH_NO, A.WH_NAME, A.MMCODE, A.CHK_WH_KIND, A.CHK_WH_GRADE, A.CHK_YM,A.CONT_PRICE, A.DISC_CPRICE,
                                                        A.PYM_CONT_PRICE, A.PYM_DISC_CPRICE, A.SOURCECODE, A.PYM_SOURCECODE, A.UNITRATE, A.M_CONTID, A.CASENO,
                                                        A.SOURCECODE_NAME, A.CONTID_NAME, A.BASE_UNIT, A.WARBAK, A.PYM_WARBAK, A.M_AGENNO, A.MMNAME_C, 
                                                        A.MMNAME_E, A.MAT_CLASS_SUB, A.CHK_TIME )

                              SELECT E.wh_no F0,
                                     Wh_name(E.wh_no) F1,--盤存單位
                                     E.MMCODE F2 ,--藥材代碼
                                     E.M_AGENNO F3 ,--廠商代碼
                                     E.MMNAME_C F4 ,--藥材名稱
                                     E.MMNAME_E F61 ,--英文品名
                                     E.BASE_UNIT F5 ,--藥材單位
                                     E.CONT_PRICE F6 ,--單價
                                     E.DISC_CPRICE F7 ,--優惠後單價
                                     E.PYM_CONT_PRICE F8 ,--上月單價
                                     E.PYM_DISC_CPRICE F9 ,--上月優惠後單價
                                     E.UNITRATE F10 ,--包裝量
                                     {0} F11 ,--上月結存
                                     NVL(E.APL_INQTY_1, 0) F12_1 ,--進貨入庫
                                     NVL(E.APL_INQTY_2, 0) F12_2 ,--撥發入庫
                                     E.APL_OUTQTY F13 ,--撥發出庫
                                     E.TRN_INQTY F14 ,--調撥入庫
                                     E.TRN_OUTQTY F15 ,--調撥出庫
                                     E.ADJ_INQTY F16 ,--調帳入庫
                                     E.ADJ_OUTQTY F17 ,--調帳出庫
                                     E.BAK_INQTY F18 ,--繳回入庫
                                     E.BAK_OUTQTY F19 ,--繳回出庫
                                     E.REJ_OUTQTY F20 ,--退貨量
                                     E.DIS_OUTQTY F21 ,--報廢量
                                     E.EXG_INQTY F22 ,--換貨入庫
                                     E.EXG_OUTQTY F23 ,--換貨出庫
                                     0 AS F24 ,--軍方消耗
                                     (CASE WHEN (E.CHK_WH_GRADE = '1') THEN 0 
                                           WHEN (E.WH_NAME LIKE '%供應中心%') THEN 0
                                           WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN E.ALTERED_USE_QTY
                                           ELSE E.USE_QTY_AF_CHK
                                      END) F25 ,--民眾消耗
                                     --(E.ORI_USE_QTY + (CASE WHEN(E.GAP <= 0) THEN (E.STORE_QTYC - E.CHK_QTY) ELSE 0 END)) F25,
                                     (CASE WHEN (E.CHK_WH_GRADE = '1') THEN 0 
                                           WHEN (E.WH_NAME LIKE '%供應中心%') THEN 0
                                           WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN E.ALTERED_USE_QTY
                                           ELSE E.USE_QTY_AF_CHK
                                      END) F26 ,--本月總消耗
                                     --(E.ORI_USE_QTY + (CASE WHEN(E.GAP <= 0) THEN (E.STORE_QTYC - E.CHK_QTY) ELSE 0 END)) F26, 
                                     ( {3} ) F27 ,--應有結存
                                     {2} F28 ,--盤存量
                                     {2} F29 ,--本月結存
                                     (CASE WHEN E.PYM_SOURCECODE = 'C' THEN {0}
                                           ELSE 0
                                      END) F30 ,--上月寄庫藥品買斷結存
                                     (CASE WHEN E.SOURCECODE = 'C' THEN {2}
                                           ELSE 0
                                      END) F31 ,--本月寄庫藥品買斷結存
                                     E.INVENTORY F32 ,--差異量
                                     E.CONT_PRICE * ({2}) F33 ,--結存金額
                                     E.DISC_CPRICE * ({2}) F34 ,--優惠後結存金額
                                     E.INVENTORY * E.CONT_PRICE F35 ,--差異金額
                                     E.INVENTORY * E.DISC_CPRICE F36 ,--優惠後差異金額
                                     get_param('MI_MAST', 'MAT_CLASS_SUB', E.MAT_CLASS_SUB) F37 ,--藥材類別
                                     E.CONTID_NAME F38 ,--是否合約
                                     E.SOURCECODE_NAME F39 ,--買斷寄庫
                                     E.WARBAK F40 ,--是否戰備
                                     E.WAR_QTY F41 ,--戰備存量
                                     ROUND((CASE WHEN (E.ORI_USE_QTY + (CASE WHEN (E.GAP <= 0) THEN (E.STORE_QTYC - E.CHK_QTY) 
                                                                             ELSE 0
                                                                        END)) = 0  
                                                 THEN 0
                                                 ELSE ((E.CHK_QTY {1} ) / (E.ORI_USE_QTY + (CASE WHEN (E.GAP <= 0) THEN (E.STORE_QTYC - E.CHK_QTY) 
                                                                                          ELSE 0
                                                                                     END)))
                                            END), 2) F42 ,--期末比
                                     0 F43 ,--贈品數量
                                     E.PYM_WAR_QTY F44 ,--上月戰備量
                                     E.PYM_WARBAK F45 ,--上月是否戰備
                                     (CASE WHEN (E.PYM_INV_QTY - E.PYM_WAR_QTY) < 0 THEN 0 
                                           ELSE E.PYM_INV_QTY - E.PYM_WAR_QTY
                                      END) F46 ,--不含戰備上月結存
                                     E.DISC_CPRICE - E.PYM_DISC_CPRICE F47 ,--單價差額
                                     (CASE WHEN (E.PYM_INV_QTY - E.PYM_WAR_QTY) < 0 THEN 0 
                                           ELSE E.PYM_INV_QTY - E.PYM_WAR_QTY
                                      END) * (E.DISC_CPRICE - E.PYM_DISC_CPRICE) F48 ,--不含戰備上月結存價差
                                     (CASE WHEN E.CHK_QTY <= E.WAR_QTY THEN E.CHK_QTY * (E.DISC_CPRICE - E.PYM_DISC_CPRICE)
                                           ELSE (E.CHK_QTY - E.WAR_QTY) * (E.DISC_CPRICE - E.PYM_DISC_CPRICE)
                                      END) F49 ,--戰備本月價差
                                     (CASE WHEN E.WAR_QTY = 0 then E.CHK_QTY 
                                           WHEN E.CHK_QTY <= E.WAR_QTY THEN 0
                                           ELSE E.CHK_QTY - E.WAR_QTY
                                      END) F50 ,--不含戰備本月結存
                                     (case when E.WAR_QTY > e.chk_qty then 0 
                                       else
                                        (
                                              (CASE WHEN (E.CHK_WH_GRADE = '1') THEN E.STORE_QTYC
                                                 WHEN (E.WH_NAME LIKE '%供應中心%') THEN E.STORE_QTYC
                                                 WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN (
                                                      (e.pym_inv_qty -- 上月庫存數量 
                                                                  +e.apl_inqty   -- 月結入庫總量 
                                                                  -e.apl_outqty  -- 月結撥發總量 
                                                                  +e.trn_inqty   -- 月結調撥入總量 
                                                                  -e.trn_outqty  -- 月結調撥入總量 
                                                                  +e.adj_inqty   -- 月結調帳入總量 
                                                                  -e.adj_outqty  -- 月結調帳出總量 
                                                                  +e.bak_inqty   -- 月結繳回入庫總量 
                                                                  -e.bak_outqty  -- 月結繳回出庫總量 
                                                                  -e.rej_outqty  -- 月結退貨總量 
                                                                  -e.dis_outqty  -- 月結報廢總量
                                                                  +e.exg_inqty   -- 月結換貨入庫總量 
                                                                  -e.exg_outqty  -- 月結換貨出庫總量 
                                                                  -nvl( 
                                                                      nvl(e.altered_use_qty, e.ori_use_qty), -- 調整消耗(只有藥局可以用), 月結耗用量=醫令扣庫 
                                                                      0 
                                                                  ) )
                                                      )
                                                 ELSE E.STORE_QTYC - (CASE WHEN E.CHK_TIME IS NOT NULL THEN E.USE_QTY_AF_CHK
                                                                           ELSE E.ORI_USE_QTY
                                                                      END)
                                            END) - nvl(e.war_qty, 0)
                                       ) 
                                     end) F51 ,--不含戰備本月應有結存量
                                     (CASE WHEN E.WAR_QTY = 0 then E.CHK_QTY 
                                           WHEN E.CHK_QTY <= E.WAR_QTY THEN 0
                                           ELSE E.CHK_QTY - E.WAR_QTY
                                      END) F52 ,--不含戰備本月盤存量
                                     (E.APL_INQTY - 0) F53 ,--不含贈品本月進貨
                                     E.G34_MAX_APPQTY F54 ,--單位申請基準量
                                     E.EST_APPQTY F55 ,--下月預計申請量
                                       E.BAK_INQTY F56 ,--退料入庫
                                       E.BAK_OUTQTY F57 ,--退料出庫
                                       (CASE WHEN E.DISC_CPRICE >= 5000 THEN (CASE WHEN (E.CHK_WH_GRADE = '1') THEN 0 
                                                                                   WHEN (E.WH_NAME LIKE '%供應中心%') THEN 0
                                                                                   WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN E.ALTERED_USE_QTY
                                                                                   ELSE E.USE_QTY_AF_CHK
                                                                              END)
                                             ELSE 0 
                                        END) F58, --本月單價5000元以上總消耗
                                       (CASE WHEN E.DISC_CPRICE < 5000 THEN (CASE WHEN (E.CHK_WH_GRADE = '1') THEN 0 
                                                                                  WHEN (E.WH_NAME LIKE '%供應中心%') THEN 0
                                                                                  WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN E.ALTERED_USE_QTY
                                                                                  ELSE E.USE_QTY_AF_CHK
                                                                             END)
                                             ELSE 0 
                                        END) F59  ,--本月單價未滿5000元總消耗
                                       E.CASENO F60, --合約案號
                                      ( CASE
                                                WHEN E.sourcecode_name = '買斷' THEN 
                                                     (( CASE
                                                        WHEN E.pym_sourcecode = 'C' THEN {0}
                                                        ELSE 0
                                                      END ))
                                                ELSE 0
                                              END ) F62 , --上月寄庫藥品買斷結存 (買斷)
                                      ( CASE
                                                WHEN E.sourcecode_name = '寄庫' THEN 
                                                     (( CASE
                                                        WHEN E.pym_sourcecode = 'C' THEN {0}
                                                        ELSE 0
                                                      END ))
                                                ELSE 0
                                              END ) F63, --上月寄庫藥品買斷結存 (寄庫)
                                      ( CASE
                                                WHEN E.sourcecode_name = '買斷' THEN (E.disc_cprice * {2} )
                                                ELSE 0
                                              END ) F64,
                                      ( CASE
                                         WHEN E.sourcecode_name = '寄庫' THEN (E.disc_cprice * {2} )
                                         ELSE 0
                                       END ) F65,
                                       {0} * E.PYM_DISC_CPRICE T01 ,--上月結存總金額=上月結存*上月優惠後單價
                                       E.APL_INQTY * E.DISC_CPRICE T02 ,--本月進貨總金額=進貨*優惠後單價
                                       E.REJ_OUTQTY * E.DISC_CPRICE T03 ,--本月退貨總金額=退貨量*優惠後單價
                                       0 T04 ,--本月贈品總金額=贈品數量*優惠後單價
                                       (CASE WHEN (E.CHK_WH_GRADE = '1') THEN 0 
                                             WHEN (E.WH_NAME LIKE '%供應中心%') THEN 0
                                             WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN E.ALTERED_USE_QTY
                                             ELSE E.USE_QTY_AF_CHK
                                        END) * E.DISC_CPRICE T05 ,--消耗金額總金額=本月總消耗*優惠後單價
                                       0 T06 ,--軍方消耗總金額=軍方消耗*優惠後單價
                                      (CASE WHEN (E.CHK_WH_GRADE = '1') THEN 0 
                                            WHEN (E.WH_NAME LIKE '%供應中心%') THEN 0
                                            WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN E.ALTERED_USE_QTY
                                            ELSE E.USE_QTY_AF_CHK
                                       END) * E.DISC_CPRICE T07 ,--民眾消耗總金額=民眾消耗*優惠後單價
                                      (E.BAK_INQTY - E.BAK_OUTQTY) * E.DISC_CPRICE T08 ,--退料總金額=(退料入庫-退料出庫)*優惠後單價
                                      (E.TRN_INQTY - E.TRN_OUTQTY) * E.DISC_CPRICE T09 ,--調撥總金額=(調撥入庫-調撥出庫)*優惠後單價
                                      (E.EXG_INQTY - E.EXG_OUTQTY) * E.DISC_CPRICE T10 ,--換貨總金額=(換貨入庫-換貨出庫)*優惠後單價
                                      E.DIS_OUTQTY * E.DISC_CPRICE T11 ,--報廢總金額=報廢量*優惠後單價
                                      ( {3} ) * E.DISC_CPRICE T12 ,--應有結存總金額=應有結存*優惠後單價
                                      E.CHK_QTY * E.DISC_CPRICE T13 ,--盤點結存總金額=盤存量*優惠後單價
                                      E.CHK_QTY * E.DISC_CPRICE T14 ,--本月結存總金額=本月結存*優惠後單價
                                      (CASE WHEN E.SOURCECODE_NAME = '買斷' THEN E.CHK_QTY 
                                            ELSE 0 
                                       END) * E.DISC_CPRICE T15 ,--買斷結存總金額=(CASE WHEN A.買斷寄庫 = '買斷' THEN 本月結存 ELSE 0 END) * 優惠後單價
                                      (CASE WHEN E.PYM_SOURCECODE = 'C' THEN E.PYM_INV_QTY
                                            ELSE 0
                                       END) * E.DISC_CPRICE  T16,--上月寄庫藥品買斷結存總金額=上月寄庫藥品買斷結存*優惠後單價
                                      (CASE WHEN E.SOURCECODE = 'C' THEN E.CHK_QTY
                                            ELSE 0
                                       END) * E.DISC_CPRICE T17 ,--本月寄庫藥品買斷結存總金額=本月寄庫藥品買斷結存*優惠後單價
                                      ( CASE
                WHEN E.war_qty > 0 then (case when e.chk_qty > e.war_qty then e.war_qty else e.chk_qty end)
                ELSE 0
                END ) * E.DISC_CPRICE T18 ,--戰備金額總金額
                                      (CASE WHEN E.SOURCECODE_NAME = '寄庫' THEN E.CHK_QTY 
                                            ELSE 0
                                       END) * E.DISC_CPRICE T19 ,--寄庫結存總金額
                                      (CASE WHEN (E.PYM_INV_QTY - E.PYM_WAR_QTY) < 0 THEN 0 
                                            ELSE E.PYM_INV_QTY - E.PYM_WAR_QTY
                                       END) * (E.DISC_CPRICE - E.PYM_DISC_CPRICE) T20 ,--不含戰備上月結存價差金額
                                      (CASE WHEN E.CHK_QTY <= E.WAR_QTY THEN E.CHK_QTY * (E.DISC_CPRICE - E.PYM_DISC_CPRICE)
                                            ELSE (E.CHK_QTY - E.WAR_QTY) * (E.DISC_CPRICE - E.PYM_DISC_CPRICE)
                                       END) T21 ,--戰備本月價差金額
                                      0 T22 ,--折讓總金額
                                      (CASE WHEN :RLNO_TEXT = '2' THEN TO_CHAR((E.APL_INQTY * E.DISC_CPRICE)-(E.REJ_OUTQTY * E.DISC_CPRICE))
                                            ELSE ''
                                       END) T23 ,--進貨總金額
                                      (CASE WHEN((E.CHK_WH_KIND ='1' AND (E.CHK_WH_GRADE <> '1' OR E.WH_NAME NOT LIKE '%供應中心%') ) OR (E.CHK_WH_KIND='0' AND E.CHK_WH_GRADE NOT IN ('1','2')) ) THEN 0
                                            WHEN E.INVENTORY * E.CONT_PRICE > 0 THEN E.INVENTORY * E.CONT_PRICE
                                            ELSE 0
                                       END) T24 ,--盤盈總金額
                                      (CASE WHEN((E.CHK_WH_KIND ='1' AND (E.CHK_WH_GRADE <> '1' OR E.WH_NAME NOT LIKE '%供應中心%') ) OR (E.CHK_WH_KIND='0' AND E.CHK_WH_GRADE NOT IN ('1','2')) ) THEN 0
                                            WHEN E.INVENTORY * E.CONT_PRICE < 0 THEN E.INVENTORY * E.CONT_PRICE
                                            ELSE 0
                                       END)  T25 ,--盤虧總金額  
                                      (CASE WHEN((E.CHK_WH_KIND ='1' AND (E.CHK_WH_GRADE <> '1' OR E.WH_NAME NOT LIKE '%供應中心%') ) OR (E.CHK_WH_KIND='0' AND E.CHK_WH_GRADE NOT IN ('1','2')) ) THEN 0
                                            ELSE E.INVENTORY * E.CONT_PRICE
                                       END) T26 ,-- 合計盤盈虧總金額
                                      (CASE WHEN E.DISC_CPRICE >= 5000 THEN (CASE WHEN (E.CHK_WH_GRADE = '1') THEN 0 
                                                                                  WHEN (E.WH_NAME LIKE '%供應中心%') THEN 0
                                                                                  WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN E.ALTERED_USE_QTY
                                                                                  ELSE E.USE_QTY_AF_CHK
                                                                             END)
                                            ELSE 0 
                                       END) * E.DISC_CPRICE T27, --本月單價5000元以上消耗總金額
                                      (CASE WHEN E.DISC_CPRICE < 5000 THEN (CASE WHEN (E.CHK_WH_GRADE = '1') THEN 0 
                                                                                 WHEN (E.WH_NAME LIKE '%供應中心%') THEN 0
                                                                                 WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN E.ALTERED_USE_QTY
                                                                                 ELSE E.USE_QTY_AF_CHK
                                                                            END)
                                            ELSE 0 
                                       END) * E.DISC_CPRICE T28  --本月單價未滿5000元消耗總金額    
                              FROM INVMON_DATA E WHERE 1 = 1 "
             , p20 == "false" ? "E.PYM_INV_QTY" : @"(case when E.PYM_WAR_QTY = 0 then E.PYM_INV_QTY when E.PYM_INV_QTY <= E.PYM_WAR_QTY then 0 else E.PYM_INV_QTY - E.PYM_WAR_QTY end)"
             , p20 == "false" ? string.Empty : " - nvl(E.WAR_QTY, 0)"
             , p20 == "false" ? "E.CHK_QTY" : @"(case when E.WAR_QTY = 0 then E.CHK_QTY when E.CHK_QTY <= E.WAR_QTY then 0 else E.CHK_QTY - E.WAR_QTY end)"
             , temp
            );
            #region old sql
            /*
WITH wh_nos
                             AS (SELECT A.wh_no,
                                        wh_kind
                                 FROM   mi_whid A,
                                        mi_whmast B
                                 WHERE  ( A.wh_userid = :USER_NAME
                                          AND A.wh_no = B.wh_no )),
                             mat_wh_no_all
                             AS (SELECT wh_no,
                                        wh_kind
                                 FROM   mi_whmast
                                 WHERE  wh_kind = '1'
                                        AND EXISTS (SELECT 1
                                                    FROM   ur_uir
                                                    WHERE  tuser = :USER_NAME
                                                           AND rlno IN ( 'ADMG', 'ADMG_14',
                                                                         'MAT_14',
                                                                         'MMSpl_14'
                                                                       ))),
                             med_wh_no_all
                             AS (SELECT wh_no,
                                        wh_kind
                                 FROM   mi_whmast
                                 WHERE  wh_kind = '0'
                                        AND EXISTS (SELECT 1
                                                    FROM   ur_uir
                                                    WHERE  tuser = :USER_NAME
                                                           AND rlno IN ( 'ADMG', 'ADMG_14',
                                                                         'MED_14',
                                                                         'MMSpl_14'
                                                                         ,
                                                                         'PHR_14' )
                                                       )),
                             inids
                             AS (SELECT C.inid
                                 FROM   mi_whid A,
                                        mi_whmast B,
                                        ur_inid C
                                 WHERE  ( A.wh_userid = :USER_NAME
                                          AND A.wh_no = B.wh_no )
                                        AND B.inid = C.inid),
                             inid_all
                             AS (SELECT B.inid
                                 FROM   mi_whmast A,
                                        ur_inid B
                                 WHERE  EXISTS (SELECT 1
                                                FROM   ur_uir
                                                WHERE  tuser = :USER_NAME
                                                       AND rlno IN ( 'ADMG', 'ADMG_14', 'MAT_14'
                                                                     ,
                                                                     'MED_14',
                                                                     'MMSpl_14' )
                                                       )),
                             mat_wh_kind
                             AS (SELECT DISTINCT 1        AS WH_KIND,
                                                 '衛材' AS TEXT
                                 FROM   mi_whid A,
                                        mi_whmast B
                                 WHERE  ( A.wh_userid = :USER_NAME
                                          AND A.wh_no = B.wh_no
                                          AND B.wh_kind = '1' )
                                         OR EXISTS (SELECT 1
                                                    FROM   ur_uir
                                                    WHERE  tuser = :USER_NAME
                                                           AND rlno IN ( 'ADMG', 'ADMG_14',
                                                                         'MAT_14',
                                                                         'MMSpl_14'
                                                                       ))),
                             med_wh_kind
                             AS (SELECT DISTINCT 0        AS WH_KIND,
                                                 '藥品' AS TEXT
                                 FROM   mi_whid A,
                                        mi_whmast B
                                 WHERE  ( A.wh_userid = :USER_NAME
                                          AND A.wh_no = B.wh_no
                                          AND B.wh_kind = '1' )
                                         OR EXISTS (SELECT 1
                                                    FROM   ur_uir
                                                    WHERE  tuser = :USER_NAME
                                                           AND rlno IN ( 'ADMG', 'ADMG_14',
                                                                         'MED_14',
                                                                         'MMSpl_14'
                                                                         ,
                                                                         'PHR_14' )
                                                       )),
                             chk_data
                             AS (SELECT B.chk_wh_no,
                                        B.chk_wh_kind,
                                        B.chk_wh_grade,
                                        B.chk_ym,
                                        A.mmcode,
                                        A.chk_qty,
                                        A.store_qtyc,
                                        Nvl(( A.chk_qty - A.store_qtyc ), 0)                 GAP
                                        ,
                                        Nvl(A.war_qty, 0)
                                        AS WAR_QTY,
                                        Nvl(A.pym_war_qty, 0)                                AS
                                        PYM_WAR_QTY,
                                        Last_invmon(:SET_YM, B.chk_wh_no, A.mmcode)          AS
                                        PYM_INV_QTY,
                                        C.m_contprice                                        AS
                                        CONT_PRICE,
                                        C.disc_cprice,
                                        Nvl(D.m_contprice, 0)                                AS
                                        PYM_CONT_PRICE,
                                        Nvl(D.disc_cprice, 0)                                AS
                                        PYM_DISC_CPRICE,
                                        C.e_sourcecode                                       AS
                                        SOURCECODE,
                                        Nvl(D.e_sourcecode, 'P')                             AS
                                        PYM_SOURCECODE,
                                        C.unitrate,
                                        C.m_contid,
                                        Wh_name(B.chk_wh_no)                                 AS
                                        WH_NAME,
                                        Get_param('MI_MAST', 'E_SOURCECODE', C.e_sourcecode) AS
                                        SOURCECODE_NAME,
                                        Get_param('MI_MAST', 'M_CONTID', C.m_contid)         AS
                                        CONTID_NAME,
                                        Nvl(C.base_unit, A.base_unit)
                                        BASE_UNIT,
                                        Nvl(A.apl_inqty, 0)
                                        APL_INQTY,
                                        Nvl(A.apl_outqty, 0)
                                        APL_OUTQTY,
                                        Nvl(A.trn_inqty, 0)
                                        TRN_INQTY,
                                        Nvl(A.trn_outqty, 0)
                                        TRN_OUTQTY,
                                        Nvl(A.adj_inqty, 0)
                                        ADJ_INQTY,
                                        Nvl(A.adj_outqty, 0)
                                        ADJ_OUTQTY,
                                        Nvl(A.bak_inqty, 0)
                                        BAK_INQTY,
                                        Nvl(A.bak_outqty, 0)
                                        BAK_OUTQTY,
                                        Nvl(A.rej_outqty, 0)
                                        REJ_OUTQTY,
                                        Nvl(A.dis_outqty, 0)
                                        DIS_OUTQTY,
                                        Nvl(A.exg_inqty, 0)
                                        EXG_INQTY,
                                        Nvl(A.exg_outqty, 0)
                                        EXG_OUTQTY,
                                        Nvl(A.use_qty, 0)
                                        USE_QTY,
                                        Nvl(A.use_qty_af_chk, 0)
                                        USE_QTY_AF_CHK,
                                        Nvl(inventory, 0)
                                        INVENTORY,
                                        Nvl(A.mil_use_qty, 0)
                                        MIL_USE_QTY
                                        ,
                                        Nvl(A.civil_use_qty, 0)
                                        CIVIL_USE_QTY,
                                        Nvl(A.altered_use_qty, 0)
                                        ALTERED_USE_QTY,
                                        A.chk_time,
                                        C.caseno,
                                        C.warbak,
                                        Nvl(D.warbak, ' ')                                   AS
                                        PYM_WARBAK,
                                        C.m_agenno,
                                        C.mmname_c,
                                        C.mmname_e,
                                        C.mat_class_sub,
                                        A.g34_max_appqty,
                                        A.est_appqty
                                 FROM   chk_mast B,
                                        chk_detail A,
                                        mi_mast_mon C
                                        left join mi_mast_mon D
                                               ON ( D.mmcode = C.mmcode
                                                    AND D.data_ym = Twn_pym(C.data_ym) )
                                 WHERE  B.chk_ym = :SET_YM
                                        AND B.chk_no = A.chk_no
                                        AND A.mmcode = C.mmcode
                                        AND B.chk_ym = C.data_ym
                                        AND B.chk_level = '1'
                                        AND B.chk_wh_no IN (SELECT wh_no
                                                            FROM   wh_nos
                                                            UNION
                                                            SELECT wh_no
                                                            FROM   mat_wh_no_all
                                                            UNION
                                                            SELECT wh_no
                                                            FROM   med_wh_no_all)
                                        AND B.chk_wh_kind IN (SELECT wh_kind
                                                              FROM   mat_wh_kind
                                                              UNION
                                                              SELECT wh_kind
                                                              FROM   med_wh_kind)
                                        AND ( EXISTS (SELECT 1
                                                      FROM   mi_whmast A,
                                                             inids B
                                                      WHERE  A.wh_no = B.chk_wh_no
                                                             AND A.inid = B.inid)
                                               OR EXISTS (SELECT 1
                                                          FROM   mi_whmast A,
                                                                 inid_all B
                                                          WHERE  A.wh_no = B.chk_wh_no
                                                                 AND A.inid = B.inid) )
                                        AND B.chk_wh_grade <> '1'
                                        AND B.chk_wh_kind = Nvl(Trim(:CHK_WH_KIND_TEXT),
                                                            B.chk_wh_kind)
                                        AND B.chk_wh_no = Nvl(Trim(:CHK_WH_NO_TEXT),
                                                          B.chk_wh_no)
                                        AND B.chk_wh_no IN (SELECT wh_no
                                                            FROM   mi_whmast
                                                            WHERE  inid = Nvl(Trim(:INID_TEXT),
                                                                          inid))
                                        AND A.mmcode = Nvl(Trim(:MMCODE_TEXT), A.mmcode)
                                        AND C.m_contid = Nvl(Trim(:M_CONTID_TEXT), C.m_contid)
                                        AND C.e_sourcecode = Nvl(Trim(:SOURCECODE_TEXT),
                                                             C.e_sourcecode)
                                        AND C.warbak = Nvl(Trim(:WARBAK_TEXT), C.warbak)
                                        AND C.e_restrictcode = Nvl(Trim(:E_RESTRICTCODE_TEXT),
                                                               C.e_restrictcode)
                                        AND C.common = Nvl(Trim(:COMMON_TEXT), C.common)
                                        AND C.fastdrug = Nvl(Trim(:FASTDRUG_TEXT), C.fastdrug)
                                        AND C.drugkind = Nvl(Trim(:DRUGKIND_TEXT), C.drugkind)
                                        AND C.touchcase = Nvl(Trim(:TOUCHCASE_TEXT),
                                                          C.touchcase)
                                        AND C.orderkind = Nvl(Trim(:ORDERKIND_TEXT),
                                                          C.orderkind)
                                        AND C.spdrug = Nvl(Trim(:SPDRUG_TEXT), C.spdrug)),
                             invmon_data
                             AS (SELECT A.chk_wh_no                    AS WH_NO,
                                        A.wh_name,
                                        A.mmcode,
                                        SUM(Nvl(A.store_qtyc, 0))      AS ORI_INV_QTY,
                                        SUM(A.apl_inqty)               AS APL_INQTY,
                                        SUM(A.apl_outqty)              AS APL_OUTQTY,
                                        SUM(A.trn_inqty)               AS TRN_INQTY,
                                        SUM(A.trn_outqty)              AS TRN_OUTQTY,
                                        SUM(A.adj_inqty)               AS ADJ_INQTY,
                                        SUM(A.adj_outqty)              AS ADJ_OUTQTY,
                                        SUM(A.bak_inqty)               AS BAK_INQTY,
                                        SUM(A.bak_outqty)              AS BAK_OUTQTY,
                                        SUM(A.rej_outqty)              AS REJ_OUTQTY,
                                        SUM(A.dis_outqty)              AS DIS_OUTQTY,
                                        SUM(A.exg_inqty)               AS EXG_INQTY,
                                        SUM(A.exg_outqty)              AS EXG_OUTQTY,
                                        SUM(Nvl(A.use_qty, 0))         AS ORI_USE_QTY,
                                        SUM(Nvl(A.use_qty_af_chk, 0))  AS USE_QTY_AF_CHK,
                                        SUM(Nvl(A.inventory, 0))       AS INVENTORY,
                                        SUM(Nvl(A.mil_use_qty, 0))     AS MIL_USE_QTY,
                                        SUM(Nvl(A.civil_use_qty, 0))   AS CIVIL_USE_QTY,
                                        SUM(Nvl(A.altered_use_qty, 0)) AS ALTERED_USE_QTY,
                                        A.chk_time,
                                        A.chk_wh_kind,
                                        A.chk_wh_grade,
                                        A.chk_ym,
                                        SUM(A.chk_qty)                 AS CHK_QTY,
                                        SUM(A.store_qtyc)              AS STORE_QTYC,
                                        SUM(A.gap)                     AS GAP,
                                        SUM(A.war_qty)                 AS WAR_QTY,
                                        SUM(A.pym_war_qty)             AS PYM_WAR_QTY,
                                        SUM(A.pym_inv_qty)             AS PYM_INV_QTY,
                                        A.cont_price,
                                        A.disc_cprice,
                                        A.pym_cont_price,
                                        A.pym_disc_cprice,
                                        A.sourcecode,
                                        A.pym_sourcecode,
                                        A.unitrate,
                                        A.m_contid,
                                        A.sourcecode_name,
                                        A.contid_name,
                                        A.base_unit,
                                        A.warbak,
                                        A.pym_warbak,
                                        A.m_agenno,
                                        A.mmname_c,
                                        A.mmname_e,
                                        A.mat_class_sub,
                                        A.caseno,
                                        SUM(A.g34_max_appqty)          AS G34_MAX_APPQTY,
                                        SUM(A.est_appqty)              AS EST_APPQTY
                                 FROM   chk_data A
                                 GROUP  BY A.chk_wh_no,
                                           A.wh_name,
                                           A.mmcode,
                                           A.chk_wh_kind,
                                           A.chk_wh_grade,
                                           A.chk_ym,
                                           A.cont_price,
                                           A.disc_cprice,
                                           A.pym_cont_price,
                                           A.pym_disc_cprice,
                                           A.sourcecode,
                                           A.pym_sourcecode,
                                           A.unitrate,
                                           A.m_contid,
                                           A.caseno,
                                           A.sourcecode_name,
                                           A.contid_name,
                                           A.base_unit,
                                           A.warbak,
                                           A.pym_warbak,
                                           A.m_agenno,
                                           A.mmname_c,
                                           A.mmname_e,
                                           A.mat_class_sub,
                                           A.chk_time)
                        SELECT E.wh_no                                         F0,
                               Wh_name(E.wh_no)                                F1,--盤存單位
                               E.mmcode                                        F2,--藥材代碼
                               E.m_agenno                                      F3,--廠商代碼
                               E.mmname_c                                      F4,--藥材名稱
                               E.mmname_e                                      F61, --英文品名
                               E.base_unit                                     F5,--藥材單位
                               E.cont_price                                    F6,--單價
                               E.disc_cprice                                   F7, --優惠後單價
                               E.pym_cont_price                                F8,--上月單價
                               E.pym_disc_cprice                               F9, --上月優惠後單價
                               E.unitrate                                      F10,--包裝量
                               E.pym_inv_qty                                   F11, --上月結存
                               E.apl_inqty                                     F12, --進貨 / 撥發入庫
                               E.apl_outqty                                    F13, --撥發出庫
                               E.trn_inqty                                     F14, --調撥入庫
                               E.trn_outqty                                    F15, --調撥出庫
                               E.adj_inqty                                     F16, --調帳入庫
                               E.adj_outqty                                    F17, --調帳出庫
                               E.bak_inqty                                     F18, --繳回入庫
                               E.bak_outqty                                    F19, --繳回出庫
                               E.rej_outqty                                    F20,--退貨量
                               E.dis_outqty                                    F21,--報廢量
                               E.exg_inqty                                     F22, --換貨入庫
                               E.exg_outqty                                    F23, --換貨出庫
                               0                                               AS F24, --軍方消耗
                               ( CASE
                                   WHEN ( E.chk_wh_grade = '1' ) THEN 0
                                   WHEN ( E.wh_name LIKE '%供應中心%' ) THEN 0
                                   WHEN ( E.chk_wh_grade = '2'
                                          AND E.chk_wh_kind = '0' ) THEN E.altered_use_qty
                                   ELSE E.use_qty_af_chk
                                 END )                                         F25, --民眾消耗
                               --(E.ORI_USE_QTY + (CASE WHEN(E.GAP <= 0) THEN (E.STORE_QTYC - E.CHK_QTY) ELSE 0 END)) F25,
                               ( CASE
                                   WHEN ( E.chk_wh_grade = '1' ) THEN 0
                                   WHEN ( E.wh_name LIKE '%供應中心%' ) THEN 0
                                   WHEN ( E.chk_wh_grade = '2'
                                          AND E.chk_wh_kind = '0' ) THEN E.altered_use_qty
                                   ELSE E.use_qty_af_chk
                                 END )                                         F26, --本月總消耗
                               --(E.ORI_USE_QTY + (CASE WHEN(E.GAP <= 0) THEN (E.STORE_QTYC - E.CHK_QTY) ELSE 0 END)) F26, 
                               ( CASE
                                   WHEN ( E.chk_wh_grade = '1' ) THEN E.store_qtyc
                                   WHEN ( E.wh_name LIKE '%供應中心%' ) THEN E.store_qtyc
                                   WHEN ( E.chk_wh_grade = '2'
                                          AND E.chk_wh_kind = '0' ) THEN (( e.pym_inv_qty
                                                                            -- 上月庫存數量 
                                                                            + e.apl_inqty
                                                                            -- 月結入庫總量 
                                                                            - e.apl_outqty
                                                                            -- 月結撥發總量 
                                                                            + e.trn_inqty
                                                                            -- 月結調撥入總量 
                                                                            - e.trn_outqty
                                                                            -- 月結調撥入總量 
                                                                            + e.adj_inqty
                                                                            -- 月結調帳入總量 
                                                                            - e.adj_outqty
                                                                            -- 月結調帳出總量 
                                                                            + e.bak_inqty
                                                                            -- 月結繳回入庫總量 
                                                                            - e.bak_outqty
                                                                            -- 月結繳回出庫總量 
                                                                            - e.rej_outqty
                                                                            -- 月結退貨總量 
                                                                            - e.dis_outqty
                                                                            -- 月結報廢總量
                                                                            + e.exg_inqty
                                                                            -- 月結換貨入庫總量 
                                                                            - e.exg_outqty
                                                                            -- 月結換貨出庫總量 
                                                                            - Nvl(
                                   Nvl(e.altered_use_qty, e.ori_use_qty),
                                                                              -- 調整消耗(只有藥局可以用), 月結耗用量=醫令扣庫 
                                                                              0) ))
                                   ELSE E.store_qtyc - ( CASE
                                                           WHEN E.chk_time IS NOT NULL THEN
                                                           E.use_qty_af_chk
                                                           ELSE E.ori_use_qty
                                                         END )
                                 END )                                         F27,
                               --應有結存
                               --(E.ORI_INV_QTY - (CASE WHEN(E.GAP <= 0) THEN (E.STORE_QTYC - E.CHK_QTY) ELSE 0 END)) F27, 
                               E.chk_qty                                       F28,--盤存量
                               E.chk_qty                                       F29,
                               --本月結存
                               ( CASE
                                   WHEN E.pym_sourcecode = 'C' THEN E.pym_inv_qty
                                   ELSE 0
                                 END )                                         F30,
                               --上月寄庫藥品買斷結存
                               ( CASE
                                   WHEN E.sourcecode = 'C' THEN E.chk_qty
                                   ELSE 0
                                 END )                                         F31,
                               --本月寄庫藥品買斷結存
                               E.inventory                                     F32,--差異量
                               --(CASE WHEN (E.CHK_WH_GRADE = '1') THEN E.GAP
                               --      WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN E.GAP
                               --      WHEN (E.GAP > 0) THEN E.GAP
                               --      ELSE 0 END) F32,
                               E.cont_price * E.chk_qty                        F33,
                               --結存金額
                               E.disc_cprice * E.chk_qty                       F34,
                               --優惠後結存金額
                               E.inventory * E.cont_price                      F35,
                               --差異金額
                               --(CASE WHEN (E.CHK_WH_GRADE = '1') THEN E.GAP
                               --      WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN E.GAP
                               --      WHEN (E.GAP > 0) THEN E.GAP
                               --      ELSE 0 END) *E.CONT_PRICE F35,
                               E.inventory * E.disc_cprice                     F36,
                               --優惠後差異金額
                               --(CASE WHEN (E.CHK_WH_GRADE = '1') THEN E.GAP
                               --      WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN E.GAP
                               --      WHEN (E.GAP > 0) THEN E.GAP
                               --     ELSE 0 END) *E.DISC_CPRICE F36,
                               get_param('MI_MAST', 'MAT_CLASS_SUB', E.MAT_CLASS_SUB)   F37,
                               --藥材類別
                               E.contid_name                                   F38,
                               --是否合約
                               E.sourcecode_name                               F39,
                               --買斷寄庫
                               E.warbak                                        F40,
                               --是否戰備
                               E.war_qty                                       F41,
                               --戰備存量
                               Round(( CASE
                                         WHEN ( E.ori_use_qty + ( CASE
                                                                    WHEN ( E.gap <= 0 ) THEN (
                                                                    E.store_qtyc - E.chk_qty
                                                                                             )
                                                                    ELSE 0
                                                                  END ) ) = 0 THEN 0
                                         ELSE ( E.chk_qty / ( E.ori_use_qty + ( CASE
                                                              WHEN ( E.gap <= 0 )
                                                                                THEN (
                                                              E.store_qtyc - E.chk_qty )
                                                              ELSE 0
                                                                                END ) ) )
                                       END ), 2)                               F42,--期末比
                               0                                               F43,
                               --贈品數量
                               E.pym_war_qty                                   F44,
                               --上月戰備量
                               E.pym_warbak                                    F45,
                               --上月是否戰備
                               ( CASE
                                   WHEN ( E.pym_inv_qty - E.pym_war_qty ) < 0 THEN 0
                                   ELSE E.pym_inv_qty - E.pym_war_qty
                                 END )                                         F46,
                               --不含戰備上月結存
                               E.disc_cprice - E.pym_disc_cprice               F47,
                               --單價差額
                               ( CASE
                                   WHEN ( E.pym_inv_qty - E.pym_war_qty ) < 0 THEN 0
                                   ELSE E.pym_inv_qty - E.pym_war_qty
                                 END ) * ( E.disc_cprice - E.pym_disc_cprice ) F48,
                               --不含戰備上月結存價差
                               ( CASE WHEN E.war_qty=0 then 0
                                   WHEN E.chk_qty <= E.war_qty THEN E.chk_qty * ( E.disc_cprice
                                                                                  -
                E.pym_disc_cprice )
                ELSE ( E.chk_qty - E.war_qty ) * ( E.disc_cprice - E.pym_disc_cprice
                )
                END )                                         F49,--戰備本月價差
                ( CASE
                WHEN E.chk_qty <= E.war_qty THEN 0
                ELSE E.chk_qty - E.war_qty
                END )                                         F50,
                --不含戰備本月結存
                (CASE WHEN (E.CHK_WH_GRADE = '1') THEN E.STORE_QTYC
                                           WHEN (E.WH_NAME LIKE '%供應中心%') THEN E.STORE_QTYC
                                           WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN (
(e.pym_inv_qty -- 上月庫存數量 
            +e.apl_inqty   -- 月結入庫總量 
            -e.apl_outqty  -- 月結撥發總量 
            +e.trn_inqty   -- 月結調撥入總量 
            -e.trn_outqty  -- 月結調撥入總量 
            +e.adj_inqty   -- 月結調帳入總量 
            -e.adj_outqty  -- 月結調帳出總量 
            +e.bak_inqty   -- 月結繳回入庫總量 
            -e.bak_outqty  -- 月結繳回出庫總量 
            -e.rej_outqty  -- 月結退貨總量 
            -e.dis_outqty  -- 月結報廢總量
            +e.exg_inqty   -- 月結換貨入庫總量 
            -e.exg_outqty  -- 月結換貨出庫總量 
            -nvl( 
                nvl(e.altered_use_qty, e.ori_use_qty), -- 調整消耗(只有藥局可以用), 月結耗用量=醫令扣庫 
                0 
            ) )
)
                                           ELSE E.STORE_QTYC - (CASE WHEN E.CHK_TIME IS NOT NULL THEN E.USE_QTY_AF_CHK
                                                                     ELSE E.ORI_USE_QTY
                                                                END)
                                      END) - nvl(e.war_qty, 0)                                    F51,
                --不含戰備本月應有結存量
                ( CASE
                WHEN E.chk_qty <= E.war_qty THEN 0
                ELSE E.chk_qty - E.war_qty
                END )                                         F52,
                --不含戰備本月盤存量
                ( E.apl_inqty - 0 )                             F53,
                --不含贈品本月進貨
                E.g34_max_appqty                                F54,
                --單位申請基準量
                E.est_appqty                                    F55,
                --下月預計申請量
                E.bak_inqty                                     F56,--退料入庫
                E.bak_outqty                                    F57,--退料出庫
                ( CASE
                WHEN E.disc_cprice >= 5000 THEN (
                CASE
                WHEN ( E.chk_wh_grade = '1' ) THEN 0
                WHEN ( E.wh_name LIKE '%供應中心%' )
                THEN 0
                WHEN ( E.chk_wh_grade = '2'
                AND E.chk_wh_kind = '0' ) THEN
                E.altered_use_qty
                ELSE E.use_qty_af_chk
                END )
                ELSE 0
                END )                                         F58,
                --本月單價5000元以上總消耗
                ( CASE
                WHEN E.disc_cprice < 5000 THEN ( CASE
                WHEN ( E.chk_wh_grade = '1' ) THEN
                0
                WHEN ( E.wh_name LIKE
                '%供應中心%' )
                THEN 0
                WHEN ( E.chk_wh_grade = '2'
                AND E.chk_wh_kind = '0' )
                THEN
                E.altered_use_qty
                ELSE E.use_qty_af_chk
                END )
                ELSE 0
                END )                                         F59,
                --本月單價未滿5000元總消耗
                E.caseno                                        F60,--合約案號
                ( CASE
                           WHEN E.sourcecode_name = '買斷' THEN 
                                (( CASE
                                   WHEN E.pym_sourcecode = 'C' THEN E.pym_inv_qty
                                   ELSE 0
                                 END ))
                           ELSE 0
                         END ) F62 , --上月寄庫藥品買斷結存 (買斷)
                         
                                         ( CASE
                           WHEN E.sourcecode_name = '寄庫' THEN 
                                (( CASE
                                   WHEN E.pym_sourcecode = 'C' THEN E.pym_inv_qty
                                   ELSE 0
                                 END ))
                           ELSE 0
                         END ) F63, --上月寄庫藥品買斷結存 (寄庫)
                         ( CASE
                           WHEN E.sourcecode_name = '買斷' THEN (E.disc_cprice * E.chk_qty)
                           ELSE 0
                         END ) F64,
                                                  ( CASE
                           WHEN E.sourcecode_name = '寄庫' THEN (E.disc_cprice * E.chk_qty)
                           ELSE 0
                         END ) F65,
                E.pym_inv_qty * E.pym_disc_cprice               T01,
                --上月結存總金額=上月結存*上月優惠後單價
                E.apl_inqty * E.disc_cprice                     T02,
                --本月進貨總金額=進貨*優惠後單價
                E.rej_outqty * E.disc_cprice                    T03,
                --本月退貨總金額=退貨量*優惠後單價
                0                                               T04,
                --本月贈品總金額=贈品數量*優惠後單價
                ( CASE
                WHEN ( E.chk_wh_grade = '1' ) THEN 0
                WHEN ( E.wh_name LIKE '%供應中心%' ) THEN 0
                WHEN ( E.chk_wh_grade = '2'
                AND E.chk_wh_kind = '0' ) THEN E.altered_use_qty
                ELSE E.use_qty_af_chk
                END ) * E.disc_cprice                         T05,
                --消耗金額總金額=本月總消耗*優惠後單價
                0                                               T06,
                --軍方消耗總金額=軍方消耗*優惠後單價
                ( CASE
                WHEN ( E.chk_wh_grade = '1' ) THEN 0
                WHEN ( E.wh_name LIKE '%供應中心%' ) THEN 0
                WHEN ( E.chk_wh_grade = '2'
                AND E.chk_wh_kind = '0' ) THEN E.altered_use_qty
                ELSE E.use_qty_af_chk
                END ) * E.disc_cprice                         T07,
                --民眾消耗總金額=民眾消耗*優惠後單價
                ( E.bak_inqty - E.bak_outqty ) * E.disc_cprice  T08,
                --退料總金額=(退料入庫-退料出庫)*優惠後單價
                ( E.trn_inqty - E.trn_outqty ) * E.disc_cprice  T09,
                --調撥總金額=(調撥入庫-調撥出庫)*優惠後單價
                ( E.exg_inqty - E.exg_outqty ) * E.disc_cprice  T10,
                --換貨總金額=(換貨入庫-換貨出庫)*優惠後單價
                E.dis_outqty * E.disc_cprice                    T11,
                --報廢總金額=報廢量*優惠後單價
                ( CASE
                WHEN ( E.chk_wh_grade = '1' ) THEN E.store_qtyc
                WHEN ( E.wh_name LIKE '%供應中心%' ) THEN E.store_qtyc
                WHEN ( E.chk_wh_grade = '2'
                AND E.chk_wh_kind = '0' ) THEN (( e.pym_inv_qty
                -- 上月庫存數量 
                + e.apl_inqty
                -- 月結入庫總量 
                - e.apl_outqty
                -- 月結撥發總量 
                + e.trn_inqty
                -- 月結調撥入總量 
                - e.trn_outqty
                -- 月結調撥入總量 
                + e.adj_inqty
                -- 月結調帳入總量 
                - e.adj_outqty
                -- 月結調帳出總量 
                + e.bak_inqty
                -- 月結繳回入庫總量 
                - e.bak_outqty
                -- 月結繳回出庫總量 
                - e.rej_outqty
                -- 月結退貨總量 
                - e.dis_outqty
                -- 月結報廢總量
                + e.exg_inqty
                -- 月結換貨入庫總量 
                - e.exg_outqty
                -- 月結換貨出庫總量 
                - Nvl(
                Nvl(e.altered_use_qty, e.ori_use_qty),
                -- 調整消耗(只有藥局可以用), 月結耗用量=醫令扣庫 
                0) ))
                ELSE E.store_qtyc - ( CASE
                WHEN E.chk_time IS NOT NULL THEN
                E.use_qty_af_chk
                ELSE E.ori_use_qty
                END )
                END ) * E.disc_cprice                         T12,
                --應有結存總金額=應有結存*優惠後單價
                E.chk_qty * E.disc_cprice                       T13,
                --盤點結存總金額=盤存量*優惠後單價
                E.chk_qty * E.disc_cprice                       T14,
                --本月結存總金額=本月結存*優惠後單價
                ( CASE
                WHEN E.sourcecode_name = '買斷' THEN E.chk_qty
                ELSE 0
                END ) * E.disc_cprice                         T15,
                --買斷結存總金額=(CASE WHEN A.買斷寄庫 = '買斷' THEN 本月結存 ELSE 0 END) * 優惠後單價
                ( CASE
                WHEN E.pym_sourcecode = 'C' THEN E.pym_inv_qty
                ELSE 0
                END ) * E.disc_cprice                         T16,
                --上月寄庫藥品買斷結存總金額=上月寄庫藥品買斷結存*優惠後單價
                ( CASE
                WHEN E.sourcecode = 'C' THEN E.chk_qty
                ELSE 0
                END ) * E.disc_cprice                         T17,
                --本月寄庫藥品買斷結存總金額=本月寄庫藥品買斷結存*優惠後單價
                ( CASE
                WHEN E.war_qty > 0 then (case when e.chk_qty > e.war_qty then e.war_qty else e.chk_qty end)
                ELSE 0
                END ) * E.disc_cprice                         T18,
                --戰備金額總金額
                ( CASE
                WHEN E.sourcecode_name = '寄庫' THEN E.chk_qty
                ELSE 0
                END ) * E.disc_cprice                         T19,
                --寄庫結存總金額
                ( CASE
                WHEN ( E.pym_inv_qty - E.pym_war_qty ) < 0 THEN 0
                ELSE E.pym_inv_qty - E.pym_war_qty
                END ) * ( E.disc_cprice - E.pym_disc_cprice ) T20,
                --不含戰備上月結存價差金額
                ( CASE when E.war_qty=0 then 0
                WHEN E.chk_qty <= E.war_qty THEN E.chk_qty * ( E.disc_cprice -
                E.pym_disc_cprice )
                ELSE ( E.chk_qty - E.war_qty ) * ( E.disc_cprice - E.pym_disc_cprice
                )
                END )                                         T21,
                --戰備本月價差金額
                0                                               T22,--折讓總金額
                ( CASE
                WHEN :RLNO_TEXT = '2' THEN To_char(
                ( E.apl_inqty * E.disc_cprice ) - (
                E.rej_outqty * E.disc_cprice ))
                ELSE ''
                END )                                         T23,--進貨總金額
                ( CASE
                WHEN( ( E.chk_wh_kind = '1'
                AND ( E.chk_wh_grade <> '1'
                OR E.wh_name NOT LIKE '%供應中心%' ) )
                OR ( E.chk_wh_kind = '0'
                AND E.chk_wh_grade NOT IN ( '1', '2' ) ) ) THEN 0
                WHEN E.inventory * E.cont_price > 0 THEN E.inventory * E.cont_price
                ELSE 0
                END )                                         T24,--盤盈總金額
                ( CASE
                WHEN( ( E.chk_wh_kind = '1'
                AND ( E.chk_wh_grade <> '1'
                OR E.wh_name NOT LIKE '%供應中心%' ) )
                OR ( E.chk_wh_kind = '0'
                AND E.chk_wh_grade NOT IN ( '1', '2' ) ) ) THEN 0
                WHEN E.inventory * E.cont_price < 0 THEN E.inventory * E.cont_price
                ELSE 0
                END )                                         T25,--盤虧總金額  
                ( CASE
                WHEN( ( E.chk_wh_kind = '1'
                AND ( E.chk_wh_grade <> '1'
                OR E.wh_name NOT LIKE '%供應中心%' ) )
                OR ( E.chk_wh_kind = '0'
                AND E.chk_wh_grade NOT IN ( '1', '2' ) ) ) THEN 0
                ELSE E.inventory * E.cont_price
                END )                                         T26,
                -- 合計盤盈虧總金額
                ( CASE
                WHEN E.disc_cprice >= 5000 THEN (
                CASE
                WHEN ( E.chk_wh_grade = '1' ) THEN 0
                WHEN ( E.wh_name LIKE '%供應中心%' )
                THEN 0
                WHEN ( E.chk_wh_grade = '2'
                AND E.chk_wh_kind = '0' ) THEN
                E.altered_use_qty
                ELSE E.use_qty_af_chk
                END )
                ELSE 0
                END ) * E.disc_cprice                         T27,
                --本月單價5000元以上消耗總金額
                ( CASE
                WHEN E.disc_cprice < 5000 THEN ( CASE
                WHEN ( E.chk_wh_grade = '1' ) THEN
                0
                WHEN ( E.wh_name LIKE
                '%供應中心%' )
                THEN 0
                WHEN ( E.chk_wh_grade = '2'
                AND E.chk_wh_kind = '0' )
                THEN
                E.altered_use_qty
                ELSE E.use_qty_af_chk
                END )
                ELSE 0
                END ) * E.disc_cprice                         T28
                --本月單價未滿5000元消耗總金額    
                FROM   invmon_data E
                WHERE  1 = 1
    */
            #endregion
            sql += string.Format(@") x 
                group by F0, F1  
                order by F0, F1
               "
            );
            #endregion

            p.Add(":USER_NAME", user);
            p.Add(":SET_YM", p0); //月結年月
            p.Add(":RLNO_TEXT", ""); //盤存單位
            p.Add(":CHK_WH_KIND_TEXT", ""); //庫房類別
            p.Add(":CHK_WH_NO_TEXT", ""); //庫房代碼
            p.Add(":INID_TEXT", ""); //責任中心
            p.Add(":MAT_CLASS_SUB_TEXT", ""); //藥材類別
            p.Add(":MMCODE_TEXT", ""); //藥材代碼
            p.Add(":M_CONTID_TEXT", ""); //是否合約
            p.Add(":SOURCECODE_TEXT", ""); //買斷寄庫
            p.Add(":WARBAK_TEXT", ""); //是否戰備
            p.Add(":E_RESTRICTCODE_TEXT", ""); //管制品項
            p.Add(":COMMON_TEXT", ""); //是否常用品項
            p.Add(":FASTDRUG_TEXT", ""); //急救品項
            p.Add(":DRUGKIND_TEXT", ""); //中西藥別
            p.Add(":TOUCHCASE_TEXT", ""); //合約類別
            p.Add(":ORDERKIND_TEXT", ""); //採購類別
            p.Add(":SPDRUG_TEXT", ""); //特殊品項

            return DBWork.Connection.Query<Print2_class>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<CE0044M> Print3(string p0, string p1, string p2, string p3, string p4, string p5, string p6, string p7, string p8, string p9, string p10, 
                                           string p11, string p12, string p13, string p14, string p15, string p16, string p17, string p18, string p19, string user,
                                           string p20, string p21)
        {
            var p = new DynamicParameters();
            var sql = "";

            //應有結存
            string temp = @"(CASE WHEN (E.CHK_WH_GRADE = '1') THEN E.STORE_QTYC
                                               WHEN (E.WH_NAME LIKE '%供應中心%') THEN E.STORE_QTYC
                                               WHEN (E.CHK_WH_GRADE = '2' AND E.CHK_WH_KIND = '0') THEN (
                                                    (e.pym_inv_qty -- 上月庫存數量 
                                                                +e.apl_inqty   -- 月結入庫總量 
                                                                -e.apl_outqty  -- 月結撥發總量 
                                                                +e.trn_inqty   -- 月結調撥入總量 
                                                                -e.trn_outqty  -- 月結調撥入總量 
                                                                +e.adj_inqty   -- 月結調帳入總量 
                                                                -e.adj_outqty  -- 月結調帳出總量 
                                                                +e.bak_inqty   -- 月結繳回入庫總量 
                                                                -e.bak_outqty  -- 月結繳回出庫總量 
                                                                -e.rej_outqty  -- 月結退貨總量 
                                                                -e.dis_outqty  -- 月結報廢總量
                                                                +e.exg_inqty   -- 月結換貨入庫總量 
                                                                -e.exg_outqty  -- 月結換貨出庫總量 
                                                                -nvl( 
                                                                    nvl(e.altered_use_qty, e.ori_use_qty), -- 調整消耗(只有藥局可以用), 月結耗用量=醫令扣庫 
                                                                    0 
                                                                ) )
                                                    )
                                               ELSE E.STORE_QTYC - (CASE WHEN E.CHK_TIME IS NOT NULL THEN E.USE_QTY_AF_CHK
                                                                         ELSE E.ORI_USE_QTY
                                                                    END)
                                          END) ";
            if (p20 == "true")
            {
                temp = string.Format(@"
                    (case when E.WAR_QTY > e.chk_qty then 0 
                          else
                            ( {0} - nvl(E.war_qty, 0) )
                    end )
                ", temp);
            }

            #region sql
            //盤存單位 選 各請領單位、各單位消耗結存明細、藥材庫
            if (p1 == "1" || p1 == "2" || p1 == "5")
            {
                sql = @"      WITH WH_NOS AS ( SELECT A.WH_NO, WH_KIND 
                                               FROM MI_WHID A, MI_WHMAST B 
                                               WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO)),
                            MAT_WH_NO_ALL AS ( SELECT WH_NO, WH_KIND 
                                               FROM MI_WHMAST 
                                               WHERE WH_KIND = '1' 
                                               AND EXISTS (SELECT 1 
                                                           FROM UR_UIR  
                                                           WHERE TUSER = :USER_NAME
                                               AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14','MMSpl_14'))),
                            MED_WH_NO_ALL AS ( SELECT WH_NO, WH_KIND  
                                               FROM MI_WHMAST  
                                               WHERE WH_KIND = '0' 
                                               AND EXISTS (SELECT 1 FROM UR_UIR  
                                                           WHERE TUSER = :USER_NAME 
                                                           AND RLNO IN ('ADMG', 'ADMG_14', 'MED_14','MMSpl_14'))),
                                    INIDS AS ( SELECT C.INID  
                                               FROM MI_WHID A, MI_WHMAST B, UR_INID C  
                                               WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO)  
                                               AND B.INID = C.INID), 
                                 INID_ALL AS ( SELECT B.INID  
                                               FROM MI_WHMAST A, UR_INID B 
                                               WHERE EXISTS (SELECT 1  
                                                             FROM UR_UIR 
                                                             WHERE TUSER = :USER_NAME  
                                                             AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14','MED_14','MMSpl_14'))),
                              MAT_WH_KIND AS ( SELECT DISTINCT 1 AS WH_KIND, '衛材' AS TEXT  
                                               FROM MI_WHID A, MI_WHMAST B 
                                               WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO AND B.WH_KIND = '1') 
                                               OR EXISTS (SELECT 1 
                                                          FROM UR_UIR  
                                                          WHERE TUSER = :USER_NAME 
                                                          AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14','MMSpl_14'))),
                              MED_WH_KIND AS ( SELECT DISTINCT 0 AS WH_KIND, '藥品' AS TEXT 
                                               FROM MI_WHID A, MI_WHMAST B 
                                               WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO AND B.WH_KIND = '1') 
                                               OR EXISTS (SELECT 1  
                                                          FROM UR_UIR 
                                                          WHERE TUSER = :USER_NAME 
                                                          AND RLNO IN ('ADMG', 'ADMG_14', 'MED_14','MMSpl_14'))),
                                 CHK_DATA AS ( SELECT B.CHK_WH_NO, B.CHK_WH_KIND, B.CHK_WH_GRADE, B.CHK_YM, A.MMCODE, A.CHK_QTY, A.STORE_QTYC,
                                                      NVL((A.CHK_QTY - A.STORE_QTYC), 0) GAP, nvl(a.WAR_QTY, 0) AS WAR_QTY, nvl(a.pym_war_qty, 0) AS PYM_WAR_QTY,
                                                      LAST_INVMON(:SET_YM, B.CHK_WH_NO, A.MMCODE) AS PYM_INV_QTY, C.m_contprice as cont_price, C.DISC_CPRICE,
                                                      NVL(D.m_CONTPRICE, 0) AS PYM_CONT_PRICE, NVL(D.DISC_CPRICE, 0) AS PYM_DISC_CPRICE, C.e_SOURCECODE as sourcecode,
                                                      NVL(D.e_SOURCECODE, 'P') AS PYM_SOURCECODE, C.UNITRATE, C.M_CONTID, WH_NAME(B.CHK_WH_NO) AS WH_NAME,
                                                      GET_PARAM('MI_MAST','E_SOURCECODE', C.e_SOURCECODE) AS SOURCECODE_NAME, GET_PARAM('MI_MAST','M_CONTID',
                                                      C.M_CONTID) AS CONTID_NAME, NVL(C.BASE_UNIT, A.BASE_UNIT) BASE_UNIT,
                                                      nvl(a.apl_inqty, 0) apl_inqty , nvl(a.apl_outqty, 0) apl_outqty, nvl(a.TRN_INQTY, 0) TRN_INQTY, 
                                                      nvl(a.TRN_OUTQTY, 0) TRN_OUTQTY, nvl(a.ADJ_INQTY, 0) ADJ_INQTY, nvl(a.ADJ_OUTQTY, 0) ADJ_OUTQTY,
                                                      nvl(a.BAK_INQTY, 0) BAK_INQTY, nvl(a.BAK_OUTQTy, 0) BAK_OUTQTy, nvl(a.REJ_OUTQTY, 0) REJ_OUTQTY, 
                                                      nvl(a.DIS_OUTQTY, 0) DIS_OUTQTY, nvl(a.EXG_INQTy, 0) EXG_INQTy, nvl(a.EXG_OUTQTY, 0) EXG_OUTQTY,
                                                      nvl(a.use_qty, 0) use_qty, nvl(a.use_qty_af_chk, 0) use_qty_af_chk, nvl(inventory, 0) inventory, 
                                                      nvl(a.mil_use_qty, 0) mil_use_qty, nvl(a.civil_use_qty, 0) civil_use_qty, nvl(a.ALTERED_USE_QTY, 0) ALTERED_USE_QTY, 
                                                     a.chk_time, C.CASENO,
                                                      c.WARBAK, nvl(d.WARBAK, ' ') as pym_warbak, c.m_agenno, c.mmname_c, c.mmname_e, c.mat_class_sub,A.g34_max_appqty ,A.est_appqty 
                                               FROM CHK_MAST B, CHK_DETAIL A, MI_MAST_MON c
                                                    left join MI_MAST_MON d on (d.mmcode = c.mmcode and d.data_ym = twn_pym(c.data_ym))
                                               WHERE B.CHK_YM = :SET_YM AND B.CHK_NO = A.CHK_NO
                                               and a.mmcode= c.mmcode and b.chk_ym=c.data_ym
                                               AND B.CHK_LEVEL = '1'
                                               AND B.CHK_WH_NO IN ( SELECT WH_NO FROM WH_NOS 
                                                                    UNION 
                                                                    SELECT WH_NO FROM MAT_WH_NO_ALL 
                                                                    UNION 
                                                                    SELECT WH_NO FROM MED_WH_NO_ALL)
                                               AND B.CHK_WH_KIND IN ( SELECT WH_KIND FROM MAT_WH_KIND 
                                                                      UNION 
                                                                      SELECT WH_KIND FROM MED_WH_KIND)
                                               AND (EXISTS ( SELECT 1  
                                                             FROM MI_WHMAST A, INIDS B  
                                                             WHERE A.WH_NO = B.CHK_WH_NO  
                                                             AND A.INID = B.INID)  
                                                    OR EXISTS (SELECT 1  
                                                               FROM MI_WHMAST A, INID_ALL B  
                                                               WHERE A.WH_NO = B.CHK_WH_NO  
                                                               AND A.INID = B.INID) ) ";

                if (p17 == "true")
                {
                    sql += @" AND A.inventory <> 0";
                }
                if (p21 == "true")
                {
                    sql += @"                  AND (A.WAR_QTY <> 0 and a.chk_qty < A.WAR_QTY)";
                }

                if (p18 == "true")
                {
                    sql += @"and (
exists (select 1 from mi_winvmon
  where mmcode=a.mmcode
    and DATA_YM>=
        substr(twn_date(ADD_MONTHS(twn_todate((select max(set_ym) from mi_mnset where set_status='C')||'01'),-5)),1,5)
    and (APL_INQTY>0 or TRN_INQTY>0 or USE_QTY>0)
    and wh_no=a.wh_no)
or 
exists (select 1 from mi_winvmon
  where mmcode=a.mmcode
    and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
    and INV_QTY<>0
    and wh_no=a.wh_no)
or
exists (select 1 from mi_winvmon b
  where mmcode=a.mmcode
    and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
    and INV_QTY=0
    and wh_no=a.wh_no
    and exists(select 1 from mi_mast_mon where mmcode=b.mmcode
          and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
          and CANCEL_ID='N')
  )
)";
                }

                if (p19 == "true")
                {
                    sql += @"
                    and (
                        NVL(a.PYM_INV_QTY,0) > 0
                        or
                        (NVL(a.PYM_INV_QTY,0) = 0 and not (a.apl_inqty = 0                       
                                                   and a.trn_inqty = 0        
                                                   and a.trn_outqty = 0        
                                                   and a.adj_inqty = 0        
                                                   and a.adj_outqty = 0        
                                                   and a.bak_outqty = 0
                                                    and a.bak_inqty=0
                                                    and nvl(nvl(a.altered_use_qty, a.use_qty),0)=0
                                                )
                        )
                    )
                ";
                }

                if (p1 == "1")//若盤存單位 選 各請領單位
                {
                    sql += @"                  AND B.CHK_WH_GRADE <> '1' 
                                               AND B.CHK_WH_KIND = NVL(TRIM(:CHK_WH_KIND_TEXT),B.CHK_WH_KIND)
                                               AND B.CHK_WH_NO = NVL(TRIM(:CHK_WH_NO_TEXT),B.CHK_WH_NO)
                                               AND B.CHK_WH_NO IN (SELECT WH_NO 
                                                                   FROM MI_WHMAST 
                                                                   WHERE INID = NVL(TRIM(:INID_TEXT),INID)) ";
                }
                else if (p1 == "2")//若盤存單位 選 藥材庫
                {
                    sql += @"                  AND B.CHK_WH_NO IN (WHNO_MM1, WHNO_ME1) ";
                }
                else if (p1 == "5")//若盤存單位 選 各單位消耗結存明細
                {
                    sql += @"                  AND B.CHK_WH_GRADE <> '1'  ";
                }

                sql += @"                      AND A.MMCODE = NVL(TRIM(:MMCODE_TEXT),A.MMCODE) 
                                               AND C.M_CONTID = NVL(TRIM(:M_CONTID_TEXT),C.M_CONTID) 
                                               AND C.e_SOURCECODE = NVL(TRIM(:SOURCECODE_TEXT),C.e_SOURCECODE)
                        ";
                if (p5 == "A")//若藥材類別選A
                {
                    sql += @"                   AND C.MAT_CLASS = '01' ";
                }
                else if (p5 == "B")//若藥材類別選B
                {
                    sql += @"                   AND C.MAT_CLASS = '02' ";
                }
                else if (p5 != "A" && p5 != "B" && p5.Trim() != "")//若藥材類別選其他
                {
                    sql += @"                   AND C.MAT_CLASS_SUB = NVL(TRIM(:MAT_CLASS_SUB_TEXT),C.MAT_CLASS_SUB) ";
                }

                sql += @"                       AND C.WARBAK = NVL(TRIM(:WARBAK_TEXT),C.WARBAK)
                                                AND C.E_RESTRICTCODE = NVL(TRIM(:E_RESTRICTCODE_TEXT),C.E_RESTRICTCODE)
                                                AND C.COMMON = NVL(TRIM(:COMMON_TEXT),C.COMMON)
                                                AND C.FASTDRUG = NVL(TRIM(:FASTDRUG_TEXT),C.FASTDRUG)
                                                AND C.DRUGKIND = NVL(TRIM(:DRUGKIND_TEXT),C.DRUGKIND)
                                                AND C.TOUCHCASE = NVL(TRIM(:TOUCHCASE_TEXT),C.TOUCHCASE)
                                                AND C.ORDERKIND = NVL(TRIM(:ORDERKIND_TEXT),C.ORDERKIND)
                                                AND C.SPDRUG = NVL(TRIM(:SPDRUG_TEXT),C.SPDRUG)), 
                              INVMON_DATA AS ( SELECT A.CHK_WH_NO as WH_NO, A.WH_NAME, A.MMCODE, SUM(NVL(a.store_qtyc, 0)) AS ORI_INV_QTY, 
                                                      SUM(A.APL_INQTY) AS APL_INQTY, SUM(A.APL_OUTQTY) AS APL_OUTQTY, SUM(A.TRN_INQTY) AS TRN_INQTY, 
                                                      SUM(A.TRN_OUTQTY) AS TRN_OUTQTY, SUM(A.ADJ_INQTY) AS ADJ_INQTY, SUM(A.ADJ_OUTQTY) AS ADJ_OUTQTY,
                                                      SUM(A.BAK_INQTY) AS BAK_INQTY, SUM(A.BAK_OUTQTY) AS BAK_OUTQTY, SUM(A.REJ_OUTQTY) AS REJ_OUTQTY, 
                                                      SUM(A.DIS_OUTQTY) AS DIS_OUTQTY, SUM(A.EXG_INQTY) AS EXG_INQTY, SUM(A.EXG_OUTQTY) AS EXG_OUTQTY,
                                                      SUM(NVL(a.use_qty, 0)) AS ORI_USE_QTY, sum(nvl(a.use_qty_af_chk, 0)) as use_qty_af_chk, 
                                                      sum(nvl(a.inventory, 0)) as inventory, sum(nvl(a.mil_use_qty, 0)) as mil_use_qty, 
                                                      sum(nvl(a.civil_use_qty, 0)) as civil_use_qty,
                                                      sum(nvl(a.ALTERED_USE_QTY, 0)) as ALTERED_USE_QTY, a.chk_time,
                                                      A.CHK_WH_KIND, A.CHK_WH_GRADE,A.CHK_YM, 
                                                      SUM(A.CHK_QTY) AS CHK_QTY, SUM(A.STORE_QTYC) AS STORE_QTYC, SUM(A.GAP) AS GAP, 
                                                      SUM(A.WAR_QTY) AS WAR_QTY, SUM(A.PYM_WAR_QTY) AS PYM_WAR_QTY, SUM(A.PYM_INV_QTY) AS PYM_INV_QTY, 
                                                      A.CONT_PRICE, A.DISC_CPRICE, A.PYM_CONT_PRICE, A.PYM_DISC_CPRICE, A.SOURCECODE, A.PYM_SOURCECODE,
                                                      A.UNITRATE, A.M_CONTID, A.SOURCECODE_NAME, A.CONTID_NAME, A.BASE_UNIT, A.WARBAK, A.PYM_WARBAK, 
                                                      A.M_AGENNO, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS_SUB, A.CASENO,
                                                      SUM(A.g34_max_appqty) AS G34_MAX_APPQTY ,SUM(A.est_appqty ) AS EST_APPQTY 
                                               FROM CHK_DATA A
                                               GROUP BY A.chk_WH_NO, A.WH_NAME, A.MMCODE, A.CHK_WH_KIND, A.CHK_WH_GRADE, A.CHK_YM,A.CONT_PRICE, A.DISC_CPRICE,
                                                        A.PYM_CONT_PRICE, A.PYM_DISC_CPRICE, A.SOURCECODE, A.PYM_SOURCECODE, A.UNITRATE, A.M_CONTID, A.CASENO,
                                                        A.SOURCECODE_NAME, A.CONTID_NAME, A.BASE_UNIT, A.WARBAK, A.PYM_WARBAK, A.M_AGENNO, A.MMNAME_C, 
                                                        A.MMNAME_E, A.MAT_CLASS_SUB, a.chk_time )

                              SELECT E.WH_NO|| ' '||WH_NAME(E.WH_NO) F1, E.MMCODE F2, E.MMNAME_C || E.MMNAME_E F4, E.BASE_UNIT F5, E.CONT_PRICE F6, E.DISC_CPRICE F7, 
                                     E.PYM_INV_QTY F11, E.APL_INQTY F12,
                                     E.REJ_OUTQTY F20, 
                                     (case when (e.chk_wh_grade = '1') then 0 
                                           when (e.wh_name like '%供應中心%') then 0
                                           when (e.chk_wh_grade = '2' and e.chk_wh_kind = '0') then e.ALTERED_USE_QTY
                                           else e.USE_QTY_AF_CHK
                                     end) F25,
                                     (case when (e.chk_wh_grade = '1') then e.store_qtyc
                                           when (e.wh_name like '%供應中心%') then e.store_qtyc
                                           when (e.chk_wh_grade = '2' and e.chk_wh_kind = '0') then e.store_qtyc
                                           else e.store_qtyc - (case when e.chk_time is not null then e.use_qty_af_chk else e.ori_use_qty end)
                                     end) F27, 
                                     E.CHK_QTY F29,
                                     e.inventory F32,
                                     E.CONT_PRICE* E.CHK_QTY F33, E.DISC_CPRICE* E.CHK_QTY F34,
                                     e.inventory * e.cont_price F35, 

                                     --上月金額(F6*F11),進貨金額(F6*F12),消耗金額(F6*F25),結存金額(F6*F29),期末比值(F65/F64) 
                                     E.CONT_PRICE * E.PYM_INV_QTY F62, E.CONT_PRICE * E.APL_INQTY F63, E.CONT_PRICE *  (case when (e.chk_wh_grade = '1') then 0 
                                           when (e.wh_name like '%供應中心%') then 0
                                           when (e.chk_wh_grade = '2' and e.chk_wh_kind = '0') then e.ALTERED_USE_QTY
                                           else e.USE_QTY_AF_CHK
                                     end) F64, E.CONT_PRICE * E.CHK_QTY F65,
                                     TO_CHAR(ROUND((CASE WHEN (E.CONT_PRICE *  (case when (e.chk_wh_grade = '1') then 0 
                                           when (e.wh_name like '%供應中心%') then 0
                                           when (e.chk_wh_grade = '2' and e.chk_wh_kind = '0') then e.ALTERED_USE_QTY
                                           else e.USE_QTY_AF_CHK
                                     end)) = 0 THEN 0 ELSE (E.CONT_PRICE * E.CHK_QTY)/(E.CONT_PRICE *  (case when (e.chk_wh_grade = '1') then 0 
                                           when (e.wh_name like '%供應中心%') then 0
                                           when (e.chk_wh_grade = '2' and e.chk_wh_kind = '0') then e.ALTERED_USE_QTY
                                           else e.USE_QTY_AF_CHK
                                     end)) END),2),'FM999999990.90') F66

                              FROM INVMON_DATA E WHERE 1 = 1 order by E.WH_NO, E.MMCODE";
            }
            else if (p1 == "3" || p1 == "4")//盤存單位 選 全部藥局/全院
            {
                sql = @"        WITH WH_NOS AS ( SELECT A.WH_NO, WH_KIND 
                                                 FROM MI_WHID A, MI_WHMAST B 
                                                 WHERE (A.WH_USERID= :USER_NAME AND A.WH_NO = B.WH_NO)),
                              MAT_WH_NO_ALL AS ( SELECT WH_NO, WH_KIND
                                                 FROM MI_WHMAST 
                                                 WHERE WH_KIND = '1'
                                                 AND EXISTS (SELECT 1
                                                             FROM UR_UIR 
                                                             WHERE TUSER = :USER_NAME 
                                                             AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14','MMSpl_14'))), 
                              MED_WH_NO_ALL AS ( SELECT WH_NO, WH_KIND 
                                                 FROM MI_WHMAST 
                                                 WHERE WH_KIND = '0'
                                                 AND EXISTS (SELECT 1 
                                                             FROM UR_UIR
                                                             WHERE TUSER = :USER_NAME 
                                                             AND RLNO IN ('ADMG', 'ADMG_14', 'MED_14'))),
                                      INIDS AS ( SELECT C.INID  
                                                 FROM MI_WHID A, MI_WHMAST B, UR_INID C 
                                                 WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO) 
                                                 AND B.INID = C.INID), 
                                   INID_ALL AS ( SELECT B.INID  
                                                 FROM MI_WHMAST A, UR_INID B  
                                                 WHERE EXISTS (SELECT 1  
                                                               FROM UR_UIR  
                                                               WHERE TUSER = :USER_NAME  
                                                               AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14','MED_14','MMSpl_14'))),
                                MAT_WH_KIND AS ( SELECT DISTINCT 1 AS WH_KIND, '衛材' AS TEXT  
                                                 FROM MI_WHID A, MI_WHMAST B  
                                                 WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO AND B.WH_KIND = '1')  
                                                 OR EXISTS (SELECT 1
                                                            FROM UR_UIR
                                                            WHERE TUSER = :USER_NAME
                                                            AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14','MMSpl_14'))), 
                                MED_WH_KIND AS ( SELECT DISTINCT 0 AS WH_KIND, '藥品' AS TEXT  
                                                 FROM MI_WHID A, MI_WHMAST B  
                                                 WHERE (A.WH_USERID = :USER_NAME AND A.WH_NO = B.WH_NO AND B.WH_KIND = '1')  
                                                 OR EXISTS (SELECT 1   
                                                            FROM UR_UIR   
                                                            WHERE TUSER = :USER_NAME  
                                                            AND RLNO IN ('ADMG', 'ADMG_14', 'MED_14','PHR_14'))), 
                                   CHK_DATA AS ( SELECT B.CHK_WH_NO, B.CHK_WH_KIND, B.CHK_WH_GRADE, B.CHK_YM, A.MMCODE, A.CHK_QTY, A.STORE_QTYC,
                                                        NVL((A.CHK_QTY - A.STORE_QTYC), 0) GAP, nvl(a.WAR_QTY, 0) AS WAR_QTY, nvl(a.pym_war_qty, 0) AS PYM_WAR_QTY,
                                                        LAST_INVMON(:SET_YM, B.CHK_WH_NO, A.MMCODE) AS PYM_INV_QTY, C.m_contprice as cont_price, C.DISC_CPRICE,
                                                        NVL(D.m_contprice, 0) AS PYM_CONT_PRICE, NVL(D.DISC_CPRICE, 0) AS PYM_DISC_CPRICE, C.e_SOURCECODE as sourcecode,
                                                        NVL(D.e_SOURCECODE, 'P') AS PYM_SOURCECODE, C.UNITRATE, C.M_CONTID, WH_NAME(B.CHK_WH_NO) AS WH_NAME,
                                                        GET_PARAM('MI_MAST','E_SOURCECODE', C.e_SOURCECODE) AS SOURCECODE_NAME, GET_PARAM('MI_MAST','M_CONTID',
                                                        C.M_CONTID) AS CONTID_NAME, NVL(C.BASE_UNIT, A.BASE_UNIT) BASE_UNIT,
                                                        nvl(a.apl_inqty, 0) apl_inqty , nvl(a.apl_outqty, 0) apl_outqty, nvl(a.TRN_INQTY, 0) TRN_INQTY, 
                                                        nvl(a.TRN_OUTQTY, 0) TRN_OUTQTY, nvl(a.ADJ_INQTY, 0) ADJ_INQTY, nvl(a.ADJ_OUTQTY, 0) ADJ_OUTQTY,
                                                        nvl(a.BAK_INQTY, 0) BAK_INQTY, nvl(a.BAK_OUTQTy, 0) BAK_OUTQTy, nvl(a.REJ_OUTQTY, 0) REJ_OUTQTY, 
                                                        nvl(a.DIS_OUTQTY, 0) DIS_OUTQTY, nvl(a.EXG_INQTy, 0) EXG_INQTy, nvl(a.EXG_OUTQTY, 0) EXG_OUTQTY,
                                                        nvl(a.use_qty, 0) use_qty, nvl(a.use_qty_af_chk, 0) use_qty_af_chk, nvl(inventory, 0) inventory, 
                                                        nvl(a.mil_use_qty, 0) mil_use_qty, nvl(a.civil_use_qty, 0) civil_use_qty, nvl(a.ALTERED_USE_QTY, 0) ALTERED_USE_QTY, 
                                                        a.chk_time, C.CASENO,
                                                        c.WARBAK, nvl(d.WARBAK, ' ') as pym_warbak, c.m_agenno, c.mmname_c, c.mmname_e, c.mat_class_sub,A.g34_max_appqty ,A.est_appqty 
                                                 FROM CHK_MAST B, CHK_DETAIL A, MI_MAST_MON c
                                                      left join MI_MAST_MON d on (d.mmcode = c.mmcode and d.data_ym = twn_pym(c.data_ym))
                                                 WHERE B.CHK_YM = :SET_YM AND B.CHK_NO = A.CHK_NO
                                                 and a.mmcode= c.mmcode and b.chk_ym=c.data_ym
                                                 AND B.CHK_LEVEL = '1' ";

                if (p17 == "true")
                {
                    sql += @" AND A.inventory <> 0";
                }
                if (p21 == "true")
                {
                    sql += @"                  AND (A.WAR_QTY <> 0 and a.chk_qty < A.WAR_QTY)";
                }

                if (p18 == "true")
                {
                    sql += @"and (
exists (select 1 from mi_winvmon
  where mmcode=a.mmcode
    and DATA_YM>=
        substr(twn_date(ADD_MONTHS(twn_todate((select max(set_ym) from mi_mnset where set_status='C')||'01'),-5)),1,5)
    and (APL_INQTY>0 or TRN_INQTY>0 or USE_QTY>0)
    and wh_no=a.wh_no)
or 
exists (select 1 from mi_winvmon
  where mmcode=a.mmcode
    and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
    and INV_QTY<>0
    and wh_no=a.wh_no)
or
exists (select 1 from mi_winvmon b
  where mmcode=a.mmcode
    and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
    and INV_QTY=0
    and wh_no=a.wh_no
    and exists(select 1 from mi_mast_mon where mmcode=b.mmcode
          and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
          and CANCEL_ID='N')
  )
)";
                }

                if (p19 == "true")
                {
                    sql += @"
                    and (
                        NVL(a.PYM_INV_QTY,0) > 0
                        or
                        (NVL(a.PYM_INV_QTY,0) = 0 and not (a.apl_inqty = 0                       
                                                   and a.trn_inqty = 0        
                                                   and a.trn_outqty = 0        
                                                   and a.adj_inqty = 0        
                                                   and a.adj_outqty = 0        
                                                   and a.bak_outqty = 0
                                                    and a.bak_inqty=0
                                                    and nvl(nvl(a.altered_use_qty, a.use_qty),0)=0
                                                )
                        )
                    )
                ";
                }

                if (p1 == "3") //若盤存單位 選 全部藥局
                {
                    sql += @"                    AND B.CHK_WH_KIND = '0' 
                                                 AND B.CHK_WH_GRADE = '2' ";
                }

                sql += @"                        AND A.MMCODE = NVL(TRIM(:MMCODE_TEXT),A.MMCODE) 
                                                 AND C.M_CONTID = NVL(TRIM(:M_CONTID_TEXT),C.M_CONTID) 
                                                 AND C.e_SOURCECODE = NVL(TRIM(:SOURCECODE_TEXT),C.e_SOURCECODE)
                ";
                if (p5 == "A")//若藥材類別選A
                {
                    sql += @"                   AND C.MAT_CLASS = '01' ";
                }
                else if (p5 == "B")//若藥材類別選B
                {
                    sql += @"                   AND C.MAT_CLASS = '02' ";
                }
                else if (p5 != "A" && p5 != "B" && p5.Trim() != "")//若藥材類別選其他
                {
                    sql += @"                   AND C.MAT_CLASS_SUB = NVL(TRIM(:MAT_CLASS_SUB_TEXT),C.MAT_CLASS_SUB) ";
                }

                sql += string.Format(@"
                                                AND C.WARBAK = NVL(TRIM(:WARBAK_TEXT),C.WARBAK)
                                                AND C.E_RESTRICTCODE = NVL(TRIM(:E_RESTRICTCODE_TEXT),C.E_RESTRICTCODE)
                                                AND C.COMMON = NVL(TRIM(:COMMON_TEXT),C.COMMON)
                                                AND C.FASTDRUG = NVL(TRIM(:FASTDRUG_TEXT),C.FASTDRUG)
                                                AND C.DRUGKIND = NVL(TRIM(:DRUGKIND_TEXT),C.DRUGKIND)
                                                AND C.TOUCHCASE = NVL(TRIM(:TOUCHCASE_TEXT),C.TOUCHCASE)
                                                AND C.ORDERKIND = NVL(TRIM(:ORDERKIND_TEXT),C.ORDERKIND)
                                                AND C.SPDRUG = NVL(TRIM(:SPDRUG_TEXT),C.SPDRUG)),
                              INVMON_DATA AS ( SELECT '{0}' as WH_NO, A.WH_NAME, A.MMCODE, SUM(NVL(a.store_qtyc, 0)) AS ORI_INV_QTY, 
                                                      SUM(A.APL_INQTY) AS APL_INQTY, SUM(A.APL_OUTQTY) AS APL_OUTQTY, SUM(A.TRN_INQTY) AS TRN_INQTY, 
                                                      SUM(A.TRN_OUTQTY) AS TRN_OUTQTY, SUM(A.ADJ_INQTY) AS ADJ_INQTY, SUM(A.ADJ_OUTQTY) AS ADJ_OUTQTY,
                                                      SUM(A.BAK_INQTY) AS BAK_INQTY, SUM(A.BAK_OUTQTY) AS BAK_OUTQTY, SUM(A.REJ_OUTQTY) AS REJ_OUTQTY, 
                                                      SUM(A.DIS_OUTQTY) AS DIS_OUTQTY, SUM(A.EXG_INQTY) AS EXG_INQTY, SUM(A.EXG_OUTQTY) AS EXG_OUTQTY,
                                                      SUM(NVL(a.use_qty, 0)) AS ORI_USE_QTY, sum(nvl(a.use_qty_af_chk, 0)) as use_qty_af_chk, 
                                                      sum(nvl(a.inventory, 0)) as inventory, sum(nvl(a.mil_use_qty, 0)) as mil_use_qty, 
                                                      sum(nvl(a.civil_use_qty, 0)) as civil_use_qty,
                                                      sum(nvl(a.ALTERED_USE_QTY, 0)) as ALTERED_USE_QTY, a.chk_time,
                                                      A.CHK_WH_KIND, A.CHK_WH_GRADE,A.CHK_YM, 
                                                      SUM(A.CHK_QTY) AS CHK_QTY, SUM(A.STORE_QTYC) AS STORE_QTYC, SUM(A.GAP) AS GAP, 
                                                      SUM(A.WAR_QTY) AS WAR_QTY, SUM(A.PYM_WAR_QTY) AS PYM_WAR_QTY, SUM(A.PYM_INV_QTY) AS PYM_INV_QTY, 
                                                      A.CONT_PRICE, A.DISC_CPRICE, A.PYM_CONT_PRICE, A.PYM_DISC_CPRICE, A.SOURCECODE, A.PYM_SOURCECODE,
                                                      A.UNITRATE, A.M_CONTID, A.SOURCECODE_NAME, A.CONTID_NAME, A.BASE_UNIT, A.WARBAK, A.PYM_WARBAK, 
                                                      A.M_AGENNO, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS_SUB, A.CASENO,
                                                       SUM(A.g34_max_appqty) AS G34_MAX_APPQTY ,SUM(A.est_appqty ) AS EST_APPQTY 
                                               FROM CHK_DATA A
                                               GROUP BY A.WH_NAME, A.MMCODE, A.CHK_WH_KIND, A.CHK_WH_GRADE, A.CHK_YM,A.CONT_PRICE, A.DISC_CPRICE,
                                                        A.PYM_CONT_PRICE, A.PYM_DISC_CPRICE, A.SOURCECODE, A.PYM_SOURCECODE, A.UNITRATE, A.M_CONTID, A.CASENO,
                                                        A.SOURCECODE_NAME, A.CONTID_NAME, A.BASE_UNIT, A.WARBAK, A.PYM_WARBAK, A.M_AGENNO, A.MMNAME_C, 
                                                        A.MMNAME_E, A.MAT_CLASS_SUB, a.chk_time )

                                SELECT E.WH_NO|| ' '||WH_NAME(E.WH_NO) F1, E.MMCODE F2, E.MMNAME_C || E.MMNAME_E F4, E.BASE_UNIT F5, E.CONT_PRICE F6, E.DISC_CPRICE F7, 
                                     E.PYM_INV_QTY F11, E.APL_INQTY F12, 
                                     E.REJ_OUTQTY F20, 
                                     (case when (e.chk_wh_grade = '1') then 0 
                                           when (e.wh_name like '%供應中心%') then 0
                                           when (e.chk_wh_grade = '2' and e.chk_wh_kind = '0') then e.ALTERED_USE_QTY
                                           else e.USE_QTY_AF_CHK
                                     end) F25,
                                     (case when (e.chk_wh_grade = '1') then e.store_qtyc
                                           when (e.wh_name like '%供應中心%') then e.store_qtyc
                                           when (e.chk_wh_grade = '2' and e.chk_wh_kind = '0') then e.store_qtyc
                                           else e.store_qtyc - (case when e.chk_time is not null then e.use_qty_af_chk else e.ori_use_qty end)
                                     end) F27, 
                                     E.CHK_QTY F29,
                                     e.inventory F32,
                                     E.CONT_PRICE* E.CHK_QTY F33, E.DISC_CPRICE* E.CHK_QTY F34,
                                     e.inventory * e.cont_price F35, 

                                     --上月金額(F6*F11),進貨金額(F6*F12),消耗金額(F6*F25),結存金額(F6*F29),期末比值(F65/F64) 
                                     E.CONT_PRICE * E.PYM_INV_QTY F62, E.CONT_PRICE * E.APL_INQTY F63, E.CONT_PRICE *  (case when (e.chk_wh_grade = '1') then 0 
                                           when (e.wh_name like '%供應中心%') then 0
                                           when (e.chk_wh_grade = '2' and e.chk_wh_kind = '0') then e.ALTERED_USE_QTY
                                           else e.USE_QTY_AF_CHK
                                     end) F64, E.CONT_PRICE * E.CHK_QTY F65,
                                     TO_CHAR(ROUND((CASE WHEN (E.CONT_PRICE *  (case when (e.chk_wh_grade = '1') then 0 
                                           when (e.wh_name like '%供應中心%') then 0
                                           when (e.chk_wh_grade = '2' and e.chk_wh_kind = '0') then e.ALTERED_USE_QTY
                                           else e.USE_QTY_AF_CHK
                                     end)) = 0 THEN 0 ELSE (E.CONT_PRICE * E.CHK_QTY)/(E.CONT_PRICE *  (case when (e.chk_wh_grade = '1') then 0 
                                           when (e.wh_name like '%供應中心%') then 0
                                           when (e.chk_wh_grade = '2' and e.chk_wh_kind = '0') then e.ALTERED_USE_QTY
                                           else e.USE_QTY_AF_CHK
                                     end)) END),2),'FM999999990.90') F66

                              FROM INVMON_DATA E WHERE 1 = 1 order by E.WH_NO, E.MMCODE"
            , p1 == "3" ? "全院藥局" : "全院");
            }
            #endregion

            p.Add(":USER_NAME", user);
            p.Add(":SET_YM", p0); //月結年月
            p.Add(":RLNO_TEXT", p1); //盤存單位
            p.Add(":CHK_WH_KIND_TEXT", p2); //庫房類別
            p.Add(":CHK_WH_NO_TEXT", p3); //庫房代碼
            p.Add(":INID_TEXT", p4); //責任中心
            p.Add(":MAT_CLASS_SUB_TEXT", p5); //藥材類別
            p.Add(":MMCODE_TEXT", p6); //藥材代碼
            p.Add(":M_CONTID_TEXT", p7); //是否合約
            p.Add(":SOURCECODE_TEXT", p8); //買斷寄庫
            p.Add(":WARBAK_TEXT", p9); //是否戰備
            p.Add(":E_RESTRICTCODE_TEXT", p10); //管制品項
            p.Add(":COMMON_TEXT", p11); //是否常用品項
            p.Add(":FASTDRUG_TEXT", p12); //急救品項
            p.Add(":DRUGKIND_TEXT", p13); //中西藥別
            p.Add(":TOUCHCASE_TEXT", p14); //合約類別
            p.Add(":ORDERKIND_TEXT", p15); //採購類別
            p.Add(":SPDRUG_TEXT", p16); //特殊品項

            return DBWork.Connection.Query<CE0044M>(sql, p, DBWork.Transaction);
        }

        //月結年月
        public IEnumerable<MI_MNSET> GetYmCombo()
        {
            string sql = @"SELECT SET_YM,
                                  TWN_DATE(SET_BTIME) SET_BTIME,
                                  TWN_DATE(SET_CTIME) SET_CTIME 
                           FROM MI_MNSET 
                           WHERE SET_STATUS = 'C'
                           ORDER BY SET_YM DESC";

            return DBWork.Connection.Query<MI_MNSET>(sql);
        }
        //月結年月日期區間
        public IEnumerable<MI_MNSET> SetYmDateGet(string SET_YM)
        {
            var p = new DynamicParameters();
            string sql = @"SELECT SET_YM,
                                  TWN_DATE(SET_BTIME) SET_BTIME,
                                  TWN_DATE(SET_CTIME) SET_CTIME 
                           FROM MI_MNSET 
                           WHERE SET_STATUS = 'C'
                           AND SET_YM = :SET_YM";

            p.Add(":SET_YM", SET_YM);
            return DBWork.Connection.Query<MI_MNSET>(sql, p);
        }
        //盤存單位
        public IEnumerable<COMBO_MODEL> GetRlnoCombo(string user_name, string p0)
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
                           UNION 
                           SELECT '6' AS VALUE, '藥劑科' AS TEXT FROM TEMP WHERE RLNO IN ('ADMG', 'ADMG_14', 'MAT_14','MED_14','MMSpl_14' ) 

                           ";
            if (p0 == "AA0209")
            {
                sql = " SELECT '2' AS VALUE, '藥材庫' AS TEXT, :USER_NAME AS EXTRA1 FROM DUAL  ";
            }
            else if (p0 == "AA0218")
            {
                sql = @"WITH TEMP AS ( SELECT RLNO FROM UR_UIR WHERE TUSER = :USER_NAME)
                           SELECT '1' AS VALUE, '各請領單位' AS TEXT FROM TEMP WHERE RLNO IN ('MAT_14','MED_14', 'NRS_14', 'ADMG', 'ADMG_14', 'PHR_14','MMSpl_14' ) 
                           UNION 
                           SELECT '4' AS VALUE, '全院合計' AS TEXT FROM TEMP WHERE RLNO IN ('ADMG', 'ADMG_14', 'MAT_14','MED_14','MMSpl_14' ) 
                           ";
            }
            else if (p0 == "AB153")
            {
                sql = @"WITH TEMP AS ( SELECT RLNO FROM UR_UIR WHERE TUSER = :USER_NAME)
                           SELECT '1' AS VALUE, '各請領單位' AS TEXT FROM TEMP WHERE RLNO IN ('MAT_14','MED_14', 'NRS_14', 'ADMG', 'ADMG_14', 'PHR_14','MMSpl_14' ) 
                           UNION 
                           SELECT '3' AS VALUE, '全部藥局' AS TEXT FROM TEMP WHERE RLNO IN ('ADMG', 'ADMG_14', 'MAT_14','MED_14', 'PHR_14','MMSpl_14' ) 
                           ";
            }
            else if (p0 == "FA0096")
            {
                sql = " SELECT '2' AS VALUE, '藥材庫' AS TEXT, :USER_NAME AS EXTRA1 FROM DUAL  ";
            }
            else if (p0 == "FA0088")
            {
                sql = " SELECT '3' AS VALUE, '全部藥局' AS TEXT, :USER_NAME AS EXTRA1 FROM DUAL  ";
            }
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
                                         WHERE (A.WH_USERID= :USER_NAME AND A.WH_NO = B.WH_NO AND B.WH_KIND = '0')
                                         OR EXISTS (SELECT 1 FROM UR_UIR WHERE TUSER = :USER_NAME AND RLNO IN ('ADMG', 'ADMG_14', 'MED_14', 'PHR_14'))
                                        ),
                                TEMP AS ( SELECT * FROM MAT UNION SELECT * FROM MED )
                           SELECT * FROM TEMP
                           UNION
                           SELECT ' ' AS VALUE, '全部' AS TEXT FROM TEMP WHERE (SELECT COUNT(*) FROM TEMP) = 2
                          ";
            p.Add(":USER_NAME", user_name);

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }
        //庫房代碼
        public IEnumerable<COMBO_MODEL> GetMiWhidCombo(string user_name, string wh_kind)
        {
            var p = new DynamicParameters();
            string sql = "";

            if (wh_kind=="6")
            {
                sql = @"SELECT wh_no VALUE, wh_name  TEXT
                        FROM MI_WHMAST WHERE wh_kind='0' AND wh_grade='2'";
            }
            else
            {

                sql = string.Format(@"
                        WITH WH_NOS AS ( SELECT A.WH_NO AS VALUE, B.WH_NAME AS TEXT, B.WH_KIND
                                            FROM MI_WHID A, MI_WHMAST B
                                            WHERE (A.WH_USERID= :USER_NAME AND A.WH_NO = B.WH_NO)),
                                MAT_ALL AS ( SELECT WH_NO AS VALUE, WH_NAME AS TEXT, WH_KIND 
                                             FROM MI_WHMAST
                                             WHERE WH_KIND = '1'
                                             AND EXISTS (SELECT 1
                                                         FROM UR_UIR
                                                         WHERE TUSER = :USER_NAME
                                                         AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14','MMSpl_14'))),
                                MED_ALL AS ( SELECT WH_NO AS VALUE, WH_NAME AS TEXT, WH_KIND
                                             FROM MI_WHMAST
                                             WHERE WH_KIND = '0'
                                             AND EXISTS (SELECT 1
                                                         FROM UR_UIR
                                                         WHERE TUSER = :USER_NAME
                                                         AND RLNO IN ('ADMG', 'ADMG_14', 'MED_14','PHR_14')))
                           SELECT * 
                           FROM ( SELECT * FROM WH_NOS WHERE 1=1 {0}
                                  union
                                  SELECT * FROM MAT_ALL WHERE 1=1 {0}
                                  UNION 
                                  SELECT * FROM MED_ALL WHERE 1=1 {0}
                                ) datas
                          ORDER BY VALUE
                ", string.IsNullOrEmpty(wh_kind) ? string.Empty : "and wh_kind = :wh_kind");

                p.Add(":USER_NAME", user_name);
                p.Add(":wh_kind", wh_kind);

            }

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }
        //責任中心代碼
        public IEnumerable<COMBO_MODEL> GetUrInidCombo(string user_name)
        {
            var p = new DynamicParameters();
            string sql = @"WITH INIDS AS ( SELECT C.INID AS VALUE, C.INID_NAME AS TEXT 
                                           FROM MI_WHID A, MI_WHMAST B, UR_INID C
                                           WHERE (A.WH_USERID= :USER_NAME AND A.WH_NO = B.WH_NO)
                                           AND B.INID = C.INID ),
                                INID_ALL AS ( SELECT B.INID AS VALUE, B.INID_NAME AS TEXT 
                                              FROM MI_WHMAST A, UR_INID B
                                              WHERE EXISTS (SELECT 1 
                                                            FROM UR_UIR 
                                                            WHERE TUSER = :USER_NAME
                                                            AND RLNO IN ('ADMG', 'ADMG_14', 'MAT_14','MED_14','MMSpl_14')) )
                           SELECT *
                           FROM ( SELECT * FROM INIDS 
                                  UNION 
                                  SELECT * FROM INID_ALL ) datas
                           ORDER BY VALUE";

            p.Add(":USER_NAME", user_name);

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }
        //藥材類別
        public IEnumerable<COMBO_MODEL> GetMatClassSubParamCombo()
        {
            var p = new DynamicParameters();
            string sql = @"SELECT ROWNUM+2 AS ORDERNO, DATA_VALUE AS VALUE, DATA_DESC AS TEXT 
                           FROM PARAM_D 
                           WHERE GRP_CODE = 'MI_MAST' AND DATA_NAME = 'MAT_CLASS_SUB'
                           UNION
                           SELECT 0 AS ORDERNO, ' ' AS VALUE , '全部' AS TEXT FROM DUAL
                           UNION
                           SELECT 1 AS ORDERNO,'A' AS VALUE , '全部藥品' AS TEXT FROM DUAL
                           UNION
                           SELECT 2 AS ORDERNO,'B' AS VALUE , '全部衛材' AS TEXT FROM DUAL ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }
        //藥材代碼
        public IEnumerable<COMBO_MODEL> GetMiMastCombo()
        {
            var p = new DynamicParameters();
            string sql = @"SELECT MMCODE VALUE, MMNAME_C TEXT, MMNAME_E 
                           FROM MI_MAST ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }
        //是否合約
        public IEnumerable<COMBO_MODEL> GetContidParamCombo()
        {
            var p = new DynamicParameters();
            string sql = @"SELECT DATA_VALUE AS VALUE, DATA_DESC AS TEXT 
                           FROM PARAM_D 
                           WHERE GRP_CODE = 'MI_MAST' 
                           AND DATA_NAME = 'M_CONTID'
                           UNION
                           SELECT ' ' AS VALUE , '全部' AS TEXT 
                           FROM DUAL ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }
        //買斷寄庫
        public IEnumerable<COMBO_MODEL> GetSourcecodeParamCombo()
        {
            var p = new DynamicParameters();
            string sql = @"SELECT DATA_VALUE AS VALUE, DATA_DESC AS TEXT
                           FROM PARAM_D 
                           WHERE GRP_CODE = 'MI_MAST' 
                           AND DATA_NAME = 'E_SOURCECODE'
                           UNION
                           SELECT ' ' AS VALUE , '全部' AS TEXT
                           FROM DUAL ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }
        //是否戰備
        public IEnumerable<COMBO_MODEL> GetWarbakParamCombo()
        {
            var p = new DynamicParameters();
            string sql = @"SELECT DATA_VALUE AS VALUE, DATA_DESC AS TEXT
                           FROM PARAM_D 
                           WHERE GRP_CODE = 'MI_MAST' 
                           AND DATA_NAME = 'WARBAK'
                           UNION
                           SELECT ' ' AS VALUE , '全部' AS TEXT
                           FROM DUAL ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }
        //管制品項
        public IEnumerable<COMBO_MODEL> GetRestrictcodeParamCombo()
        {
            var p = new DynamicParameters();
            string sql = @"SELECT DATA_VALUE AS VALUE, DATA_DESC AS TEXT
                           FROM PARAM_D 
                           WHERE GRP_CODE = 'MI_MAST' 
                           AND DATA_NAME = 'E_RESTRICTCODE'
                           UNION
                           SELECT ' ' AS VALUE , '全部' AS TEXT
                           FROM DUAL ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }
        //是否常用品項
        public IEnumerable<COMBO_MODEL> GetCommonParamCombo()
        {
            var p = new DynamicParameters();
            string sql = @"SELECT DATA_VALUE AS VALUE, DATA_DESC AS TEXT
                           FROM PARAM_D 
                           WHERE GRP_CODE = 'MI_MAST' 
                           AND DATA_NAME = 'COMMON'
                           UNION
                           SELECT ' ' AS VALUE , '全部' AS TEXT
                           FROM DUAL ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }
        //急救品項
        public IEnumerable<COMBO_MODEL> GetFastdrugParamCombo()
        {
            var p = new DynamicParameters();
            string sql = @"SELECT DATA_VALUE AS VALUE, DATA_DESC AS TEXT
                           FROM PARAM_D 
                           WHERE GRP_CODE = 'MI_MAST' 
                           AND DATA_NAME = 'FASTDRUG'
                           UNION
                           SELECT ' ' AS VALUE , '全部' AS TEXT
                           FROM DUAL ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }
        //中西藥別
        public IEnumerable<COMBO_MODEL> GetDrugkindParamCombo()
        {
            var p = new DynamicParameters();
            string sql = @"SELECT DATA_VALUE AS VALUE, DATA_DESC AS TEXT
                           FROM PARAM_D 
                           WHERE GRP_CODE = 'MI_MAST' 
                           AND DATA_NAME = 'DRUGKIND'
                           UNION
                           SELECT ' ' AS VALUE , '全部' AS TEXT
                           FROM DUAL ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }
        //合約類別
        public IEnumerable<COMBO_MODEL> GetTouchcaseParamCombo()
        {
            var p = new DynamicParameters();
            string sql = @"SELECT DATA_VALUE AS VALUE, DATA_DESC AS TEXT
                           FROM PARAM_D 
                           WHERE GRP_CODE = 'MI_MAST' 
                           AND DATA_NAME = 'TOUCHCASE'
                           UNION
                           SELECT ' ' AS VALUE , '全部' AS TEXT
                           FROM DUAL ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }
        //採購類別
        public IEnumerable<COMBO_MODEL> GetOrderkindParamCombo()
        {
            var p = new DynamicParameters();
            string sql = @"SELECT DATA_VALUE AS VALUE, DATA_DESC AS TEXT
                           FROM PARAM_D 
                           WHERE GRP_CODE = 'MI_MAST' 
                           AND DATA_NAME = 'ORDERKIND'
                           UNION
                           SELECT ' ' AS VALUE , '全部' AS TEXT
                           FROM DUAL ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }
        //特殊品項
        public IEnumerable<COMBO_MODEL> GetSpecialorderkindParamCombo()
        {
            var p = new DynamicParameters();
            string sql = @"SELECT DATA_VALUE AS VALUE, DATA_DESC AS TEXT
                           FROM PARAM_D 
                           WHERE GRP_CODE = 'MI_MAST' 
                           AND DATA_NAME = 'SPDRUG'
                           UNION
                           SELECT ' ' AS VALUE , '全部' AS TEXT
                           FROM DUAL ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }

        public IEnumerable<AB0003Model> GetLoginInfo(string id, string ip)
        {
            string sql = @"SELECT TUSER AS USERID, UNA AS USERNAME, INID, INID_NAME(INID) AS INIDNAME,
                        WHNO_MM1 CENTER_WHNO,INID_NAME(WHNO_MM1) AS CENTER_WHNAME, TO_CHAR(SYSDATE,'YYYYMMDD') AS TODAY,
                        :UPDATE_IP,
                        (select DATA_VALUE from PARAM_D where GRP_CODE = 'HOSP_INFO' and DATA_NAME = 'HospCode') as HOSP_CODE,
                        (case when (select count(*) from UR_UIR where RLNO in ('MAT_14') and TUSER = :TUSER) > 0 then 'Y' else 'N' end) as IS_GRADE1
                        FROM UR_ID
                        WHERE UR_ID.TUSER=:TUSER";

            return DBWork.Connection.Query<AB0003Model>(sql, new { TUSER = id, UPDATE_IP = ip });
        }

    }
}
#region 欄位對應
/*
 F1-盤存單位 F2-藥材代碼 F3-廠商代碼
F4-藥材名稱 F61-英文品名 F5-藥材單位
F6-單價 F7-優惠後單價 F8-上月單價 F9-上月優惠後單價
F10-包裝量 F11-上月結存 F12_1-進貨入庫 F12_2-撥發入庫
F13-撥發出庫 F14-調撥入庫 F15-調撥出庫 F16-調帳入庫
F17-調帳出庫 F56-退料入庫 F57-退料出庫
F20-退貨量 F21-報廢量 F22-換貨入庫 F23-換貨出庫
F24-軍方消耗 F25-民眾消耗 F26-本月總消耗
F27-應有結存 F28-盤存量 F29-本月結存
F30-上月寄庫藥品買斷結存
F31-本月寄庫藥品買斷結存
F32-差異量 F33-結存金額 F34-優惠後結存金額
F35-差異金額 F36-優惠後差異金額 F37-藥材類別
F38-是否合約 F39-買斷寄庫 F40-是否戰備
F41-戰備存量 F42-期末比 F43-贈品數量
F44-上月戰備量 F45-上月是否戰備 F46-不含戰備上月結存
F47-單價差額 F48-不含戰備上月結存價差 F49-戰備本月價差
F50-不含戰備本月結存 F51-不含戰備本月應有結存量
F52-不含戰備本月盤存量 F53-不含贈品本月進貨
F54-單位申請基準量 F55-下月預計申請量
F58-本月單價5000元以上總消耗
F59-本月單價未滿5000元總消耗
F60-合約案號

------------
F1-上月結存 F2-本月進貨 F3-本月退貨 F4-本月贈品
F5-消耗金額 F6-軍方消耗 F7-民眾消耗 F8-退料金額
F9-調撥金額 F10-換貨金額 F11-報廢金額 F12-應有結存
F13-盤點結存 F14-本月結存 F24-盤盈金額 F25-盤虧金額
F26-合計盤盈虧總金額 F15-買斷結存
F16-上月寄庫藥品買斷結存
F17-本月寄庫藥品買斷結存
F18-戰備金額 F19-寄庫結存 F20-不含戰備上月結存價差
F21-戰備本月價差 F22-折讓金額

-------------
     */
#endregion