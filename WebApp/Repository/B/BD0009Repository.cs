using System;
using System.Data;
using JCLib.DB;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.B
{
    public class BD0009Repository : JCLib.Mvc.BaseRepository
    {
        public BD0009Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        //public int Create(string wh_no, string purdate, string userId, string procIp)
        //{
        //    var sql = @"INSERT INTO MM_PO_T (
        //                    WH_NO, MMCODE,  ADVISEQTY, PO_QTY, 
        //                    AGEN_NO, M_PURUN, CONTRACNO,  E_PURTYPE, M_DISCPERC, PO_PRICE,  UNIT_SWAP,
        //                    PO_AMT, DISAMOUNT, 
        //                    PURDATE, ISTRAN, CREATE_TIME, CREATE_USER  ) 
        //                    SELECT INV.WH_NO,INV.MMCODE,  
        //                    ceil(ceil((CTL.LOW_QTY *2 - INV.INV_QTY )/CTL.MIN_ORDQTY)/5)*5*CTL.MIN_ORDQTY,   
        //                    ceil(ceil((CTL.LOW_QTY *2 - INV.INV_QTY )/CTL.MIN_ORDQTY)/5)*5*CTL.MIN_ORDQTY,        
        //                    MAST.AGEN_NO, MAST.M_PURUN, MAST.CONTRACNO, MAST.E_PURTYPE,  nvl(MAST.M_DISCPERC,0) M_DISCPERC, MAST.M_CONTPRICE , 1,
        //                    (ceil(ceil((CTL.LOW_QTY *2 - INV.INV_QTY )/CTL.MIN_ORDQTY)/5)*5*CTL.MIN_ORDQTY)*MAST.M_CONTPRICE PO_AMT,
        //                    ((ceil(ceil((CTL.LOW_QTY *2 - INV.INV_QTY )/CTL.MIN_ORDQTY)/5)*5*CTL.MIN_ORDQTY)*nvl(MAST.M_DISCPERC,0)/100)*MAST.M_CONTPRICE DISAMOUNT,      
        //                    TWN_DATE(TO_DATE(:PURDATE,'YYYY/mm/dd')), 'N', sysdate, :CREATE_USER
        //                    FROM MI_WHINV INV, MI_WINVCTL CTL,
        //                    (select M.mmcode, M.m_purun, M.CONTRACNO, P.AGEN_NO, NVL(M.M_CONTPRICE,0) M_CONTPRICE, NVL(M.M_DISCPERC,0) M_DISCPERC, e_purtype       
        //                    from MI_MAST M, PH_VENDER P
        //                    where  M.M_AGENNO=P.AGEN_NO  and P.agen_no not in ('000','300','990','999') and M.mat_class='01' and substr(M.mmcode,1,3) in ('005','006','007') ) MAST,  
        //                    (select mmcode, sum(inv_qty) AllQty from MI_WHINV  group by mmcode ) AllQty
        //                    WHERE INV.WH_NO=CTL.WH_NO AND INV.MMCODE=CTL.MMCODE 
        //                    AND INV.mmcode =MAST.mmcode(+)
        //                    AND INV.mmcode =AllQty.mmcode(+)     		 
        //                    AND INV.WH_NO=:WH_NO  
        //                    and EXISTS 
        //                    (SELECT 1 FROM MI_MAST 
        //                    WHERE MMCODE=INV.MMCODE AND MAT_CLASS='01' AND ROWNUM=1) 
        //                    and substr(INV.mmcode,1,3) in ('005','006','007')
        //                    and MAST.agen_no not in ('000','300','990','999')		
        //                    and INV.INV_QTY <= CTL.LOW_QTY 	and INV.INV_QTY > CTL.SAFE_QTY 
        //                    union
        //                    SELECT INV.WH_NO,INV.MMCODE,  
        //                    ceil(ceil((CTL.OPER_QTY-INV.INV_QTY)/CTL.MIN_ORDQTY)/5)*5*CTL.MIN_ORDQTY,   
        //                    ceil(ceil((CTL.OPER_QTY-INV.INV_QTY)/CTL.MIN_ORDQTY)/5)*5*CTL.MIN_ORDQTY,        
        //                    MAST.AGEN_NO, MAST.M_PURUN, MAST.CONTRACNO, MAST.E_PURTYPE,  MAST.M_DISCPERC, MAST.M_CONTPRICE , 1,
        //                    (ceil(ceil((CTL.OPER_QTY-INV.INV_QTY)/CTL.MIN_ORDQTY)/5)*5*CTL.MIN_ORDQTY)*MAST.M_CONTPRICE PO_AMT,
        //                    ((ceil(ceil((CTL.OPER_QTY-INV.INV_QTY)/CTL.MIN_ORDQTY)/5)*5*CTL.MIN_ORDQTY)*nvl(MAST.M_DISCPERC,0)/100)*MAST.M_CONTPRICE DISAMOUNT,     
        //                    TWN_DATE(TO_DATE(:PURDATE,'YYYY/mm/dd')), 'N', sysdate, :CREATE_USER
        //                    FROM MI_WHINV INV, MI_WINVCTL CTL,
        //                    (select M.mmcode, M.m_purun, M.CONTRACNO, P.AGEN_NO, NVL(M.M_CONTPRICE,0) M_CONTPRICE, NVL(M.M_DISCPERC,0) M_DISCPERC, e_purtype       
        //                    from MI_MAST M, PH_VENDER P
        //                    where  M.M_AGENNO=P.AGEN_NO  and P.agen_no not in ('000','300','990','999') and M.mat_class='01' and substr(M.mmcode,1,3) in ('005','006','007') and e_purtype ='1') MAST,  
        //                    (select mmcode, sum(inv_qty) AllQty from MI_WHINV  group by mmcode ) AllQty
        //                    WHERE INV.WH_NO=CTL.WH_NO AND INV.MMCODE=CTL.MMCODE 
        //                    AND INV.mmcode =MAST.mmcode(+)
        //                    AND INV.mmcode =AllQty.mmcode(+)     		 
        //                    AND INV.WH_NO=:WH_NO  
        //                    and EXISTS 
        //                    (SELECT 1 FROM MI_MAST 
        //                    WHERE MMCODE=INV.MMCODE AND MAT_CLASS='01' AND e_purtype ='1' AND ROWNUM=1)  
        //                    and substr(INV.mmcode,1,3) in ('005','006','007')			
        //                    and MAST.agen_no not in ('000','300','990','999')				
        //                    AND CTL.SAFE_QTY < AllQty
        //                    and INV.mmcode not in (select a.mmcode FROM MI_WHINV a, MI_WINVCTL b where a.wh_no=b.wh_no and a.mmcode=b.mmcode and a.WH_NO=:WH_NO 
        //                    and INV.INV_QTY <= CTL.LOW_QTY 	and INV.INV_QTY > CTL.SAFE_QTY )                                           
        //                    union     
        //                    SELECT INV.WH_NO,INV.MMCODE,  
        //                    ceil(ceil((CTL.OPER_QTY-INV.INV_QTY)/CTL.MIN_ORDQTY)/5)*5*CTL.MIN_ORDQTY,   
        //                    ceil(ceil((CTL.OPER_QTY-INV.INV_QTY)/CTL.MIN_ORDQTY)/5)*5*CTL.MIN_ORDQTY,        
        //                    MAST.AGEN_NO, MAST.M_PURUN, MAST.CONTRACNO, MAST.E_PURTYPE,  MAST.M_DISCPERC, MAST.M_CONTPRICE , 1,
        //                    (ceil(ceil((CTL.OPER_QTY-INV.INV_QTY)/CTL.MIN_ORDQTY)/5)*5*CTL.MIN_ORDQTY)*MAST.M_CONTPRICE PO_AMT,
        //                    ((ceil(ceil((CTL.OPER_QTY-INV.INV_QTY)/CTL.MIN_ORDQTY)/5)*5*CTL.MIN_ORDQTY)*nvl(MAST.M_DISCPERC,0)/100)*MAST.M_CONTPRICE DISAMOUNT,     
        //                    TWN_DATE(TO_DATE(:PURDATE,'YYYY/mm/dd')), 'N', sysdate, :CREATE_USER
        //                    FROM MI_WHINV INV, MI_WINVCTL CTL,
        //                    (select M.mmcode, M.m_purun, M.CONTRACNO, P.AGEN_NO, NVL(M.M_CONTPRICE,0) M_CONTPRICE, NVL(M.M_DISCPERC,0) M_DISCPERC, e_purtype       
        //                    from MI_MAST M, PH_VENDER P
        //                    where  M.M_AGENNO=P.AGEN_NO  and P.agen_no not in ('000','300','990','999') and M.mat_class='01' and substr(M.mmcode,1,3) in ('005','006','007') and e_purtype ='2') MAST,  
        //                    (select mmcode, sum(inv_qty) AllQty from MI_WHINV  group by mmcode ) AllQty
        //                    WHERE INV.WH_NO=CTL.WH_NO AND INV.MMCODE=CTL.MMCODE 
        //                    AND INV.mmcode =MAST.mmcode(+)
        //                    AND INV.mmcode =AllQty.mmcode(+)     		 
        //                    AND INV.WH_NO=:WH_NO 
        //                    and EXISTS 
        //                    (SELECT 1 FROM MI_MAST 
        //                    WHERE MMCODE=INV.MMCODE AND MAT_CLASS='01' AND e_purtype ='2' AND ROWNUM=1)  
        //                    and substr(INV.mmcode,1,3) in ('005','006','007')			
        //                    and MAST.agen_no not in ('000','300','990','999')				
        //                    AND (INV.INV_QTY < CTL.SAFE_QTY or INV.INV_QTY < CTL.LOW_QTY)
        //                    and INV.mmcode not in (select a.mmcode FROM MI_WHINV a, MI_WINVCTL b where a.wh_no=b.wh_no and a.mmcode=b.mmcode and a.WH_NO=:WH_NO 
        //                    and INV.INV_QTY <= CTL.LOW_QTY 	and INV.INV_QTY > CTL.SAFE_QTY )    
        //                    order by MMCODE ";
        //    return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, PURDATE = string.Format("{0}", DateTime.Parse(purdate).ToString("yyyy-MM-dd")), CREATE_USER = userId }, DBWork.Transaction);
        //}

        public IEnumerable<BD0009> GetAll(string wh_no, string purdate, string ltype, string agenno, bool chk1, bool chk2, bool chk3, string mmcode, string maxcnt, string recalc, string icalc, int page_index, int page_size, string sorters)
        {
            string addStr1 = "";
            string addStr2 = "";
            string addStr3 = "";
            string addStr4 = "";
            string addStr5 = "";
            string addStr6 = "";
            string addStr7 = "";

            if (agenno != "")
                addStr3 = " and MAST.agen_no=:p4 ";

            // 本日出帳
            //if (chk1)
            //    addStr4 = " and(SELECT COUNT(mmcode) FROM MI_WHTRNS WHERE wh_no =:p0 and trunc(tr_date) = trunc(sysdate) and tr_mcode = 'APLO' and mmcode = INV.mmcode) > 0 ";
            // 已轉出訂單
            if (chk2)
                addStr5 = " and POT.ISTRAN in ('Y','T') ";
            // 申購中
            if (chk3)
                addStr6 = " and POT.ISTRAN in ('N') ";

            // 院內碼
            if (mmcode != "")
                addStr7 = " AND MAST.MMCODE like :p8 ";

            var p = new DynamicParameters();
            var sql = "";
            sql = @"SELECT  WH_NO, MMCODE, INV_QTY, APL_OUTQTY, APL_INQTY, SAFE_QTY, OPER_QTY, SHIP_QTY, HIGH_QTY, MIN_ORDQTY, UNIT_SWAP,
                            LOW_QTY, ALLQTY, ESTQTY, E_VACCINE, E_RESTRICTCODE, E_MANUFACT, 0 SUMINQTY, 0 INWAY_QTY, ADVQTY_OLD,  PACK_QTY0, ADVISEQTY,                            
                            (CASE WHEN ISTRAN<>'N' then '' WHEN (CREATE_TIME - MINTIME) > 0.0001158 THEN 'V' ELSE '' END) NEWFLAG,   -- insert時 create_time 不會相同, 10秒內視為同時新增                                    
                            (CASE WHEN TODAYCNT <> 0 THEN 'V' ELSE '' END) CHGFLAG,                                       
                            PO_QTY, TO_CHAR(round(PO_QTY*PO_PRICE),'999,999,999') PO_AMT, PO_PRICE, DISC_CPRICE,
                            MMNAME_E,  AGEN_NAME, M_PURUN, CONTRACNO, PURTYPE, E_PURTYPE, AGEN_NO,  M_DISCPERC, 
                            ISTRAN_1,  ISTRAN, MEMO, CALC  , USEQTY, 
                            twn_time(CREATE_TIME) as create_time, twn_time(update_TIME) as update_time,
                            ISWILLING, to_number(DISCOUNT_QTY) as DISCOUNT_QTY, to_number(DISC_COST_UPRICE) as DISC_COST_UPRICE
                    FROM   
                    (SELECT POT.WH_NO, POT.MMCODE, INVCTL.INV_QTY, INVCTL.APL_OUTQTY, INVCTL.APL_INQTY, ROUND(INVCTL.SAFE_QTY) SAFE_QTY, ROUND(INVCTL.OPER_QTY) OPER_QTY, ROUND(INVCTL.SHIP_QTY) SHIP_QTY, 
					        ROUND(INVCTL.HIGH_QTY) HIGH_QTY, INVCTL.MIN_ORDQTY, '1' UNIT_SWAP,
                            round(INVCTL.LOW_QTY) LOW_QTY, round(nvl(ALLQTY.ALLQTY,0)) ALLQTY, NVL2(EQPD.ESTQTY, EQPD.ESTQTY, 0) ESTQTY, 
                            MAST.E_VACCINE,  MAST.E_RESTRICTCODE,  MAST.E_MANUFACT, MAST.M_CONTPRICE,  POT.CREATE_TIME, POT.UPDATE_TIME, POT.PACK_QTY0,
                            POT.ADVISEQTY ,  MAST.MMNAME_E,  
                            (select AGEN_NO || '_' || AGEN_NAMEC from PH_VENDER where agen_no = POT.agen_no) AGEN_NAME, 
                            MAST.M_PURUN, MAST.CONTRACNO, TM.MINTIME, 
                            (CASE WHEN POT.E_PURTYPE='1' THEN '甲案' WHEN POT.E_PURTYPE='2' THEN '乙案' END)  PURTYPE , POT.E_PURTYPE, 
                            POT.AGEN_NO,   
                            POT.M_DISCPERC,  nvl(MAST.M_CONTPRICE,0) AS PO_PRICE, nvl(MAST.DISC_CPRICE,0) AS DISC_CPRICE,
                            POT.PO_QTY  AS PO_QTY, POT.ADVQTY_OLD, POT.CALC, 
                            CASE WHEN POT.ISTRAN='N' THEN '申購' WHEN POT.ISTRAN='Y' THEN '轉訂單' WHEN POT.ISTRAN='T' THEN '轉訂單' END AS ISTRAN_1,    
                            NVL2(POT.ISTRAN, POT.ISTRAN, 'N') ISTRAN, POT.MEMO,    NVL2(POT.PO_QTY,1,0) AS FLAG,
                            (SELECT NVL(COUNT(MMCODE),0) CNT FROM MI_WHTRNS
                              WHERE POT.WH_NO=:p0  AND TRUNC(TR_DATE) = TRUNC(SYSDATE) AND TR_MCODE='APLO' AND MMCODE=MAST.MMCODE ) TODAYCNT ,
                            round(nvl(USEQTY.USEQTY,0)) USEQTY,
                            (case when JBID.ISWILLING='是' then JBID.ISWILLING
                                  else '' end) ISWILLING,
                            (case when JBID.ISWILLING='是' then to_char(JBID.DISCOUNT_QTY)
                                  else '' end) DISCOUNT_QTY,
                            (case when JBID.ISWILLING='是' then to_char(JBID.DISC_COST_UPRICE)
                                  else '' end) DISC_COST_UPRICE
                    FROM (select CTL.WH_NO, CTL.MMCODE, nvl(INV.INV_QTY,0) INV_QTY, 
						         nvl(INV.APL_OUTQTY,0) APL_OUTQTY, nvl(INV.APL_INQTY,0) APL_INQTY,
						         CTL.SAFE_QTY, CTL.OPER_QTY, CTL.SHIP_QTY, CTL.HIGH_QTY, CTL.MIN_ORDQTY, CTL.LOW_QTY			   
						    from MI_WHINV INV, MI_WINVCTL CTL
						   where CTL.WH_NO=INV.WH_NO(+) AND  CTL.MMCODE=INV.MMCODE(+)
                             and CTL.WH_NO =:p0) INVCTL, MM_PO_T POT,
                         (SELECT M.MMCODE, M.MMNAME_E, (P.AGEN_NO||'_'||P.AGEN_NAMEC) AS AGEN_NAME, M.M_PURUN, M.CONTRACNO, P.AGEN_NO,
                                 NVL(M.M_CONTPRICE,0) M_CONTPRICE, NVL(M.DISC_CPRICE,0) DISC_CPRICE, NVL(M.M_DISCPERC,0) M_DISCPERC,  
                                 (CASE WHEN M.E_RESTRICTCODE='0' THEN '它管' WHEN M.E_RESTRICTCODE='1' THEN '管一' WHEN M.E_RESTRICTCODE='2' THEN '管二' 
                                       WHEN M.E_RESTRICTCODE='3' THEN '管三' WHEN M.E_RESTRICTCODE='4' THEN '管四' END ) E_RESTRICTCODE,
                                (CASE WHEN M.E_VACCINE='N' THEN '' ELSE M.E_VACCINE END ) E_VACCINE, E_MANUFACT            
                            FROM MI_MAST M, PH_VENDER P
                           WHERE  M.M_AGENNO=P.AGEN_NO   AND M.MAT_CLASS='01'  ) MAST,  
                         (SELECT MMCODE, SUM(INV_QTY) ALLQTY FROM MI_WHINV  GROUP BY MMCODE ) ALLQTY,
                         (SELECT MMCODE, ESTQTY FROM PH_EQPD WHERE WH_NO=:p0 AND RECYM=TWN_YYYMM(sysdate)) EQPD,
                         (select min(create_time) MINTIME from MM_PO_T where PURDATE =:purdate ) TM,
                         (SELECT MMCODE, SUM(USE_QTY) USEQTY FROM MI_WHINV GROUP BY MMCODE ) USEQTY,
                         (select a.MMCODE, b.ISWILLING, b.DISCOUNT_QTY, b.DISC_COST_UPRICE
                            from MI_MAST a, MILMED_JBID_LIST b
                           where substr(a.E_YRARMYNO,1,3)=b.JBID_STYR
                             and a.E_ITEMARMYNO=b.BID_NO
                             and b.ISWILLING='是' and a.MAT_CLASS='01'
                         ) JBID
                   WHERE MAST.MMCODE =INVCTL.MMCODE(+)
					 AND MAST.MMCODE = ALLQTY.MMCODE(+) 
                     AND MAST.MMCODE =USEQTY.MMCODE(+)
                     AND MAST.MMCODE =EQPD.MMCODE(+)  	
                     and MAST.MMCODE =JBID.MMCODE(+)
                     AND MAST.MMCODE =POT.MMCODE   
                     AND POT.PURDATE =:purdate " + addStr3 + addStr5 + addStr6 + addStr7 + @"
                     AND EXISTS  
                         (SELECT 1 FROM MI_MAST 
                           WHERE MMCODE=MAST.MMCODE AND MAT_CLASS='01' AND ROWNUM=1) 
                         )" + string.Format(" ORDER BY {0} ", WorkSession.GetSortStatement(sorters));

            p.Add(":p0", wh_no);
            p.Add(":purdate", purdate);
            p.Add(":p4", agenno);
            p.Add(":p8", mmcode + '%');
            //p.Add(":MAXCNT", maxcnt);
            p.Add(":reCalc", recalc);
            p.Add(":icalc", icalc);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<BD0009>(sql, p, DBWork.Transaction);
        }

        //針對[採購日期]<>當天 , MM_PO_T 不要重算[建議量],不要insert [低於安全存量]
        public IEnumerable<BD0009> GetAll_1(string wh_no, string purdate, string ltype, string agenno, bool chk1, bool chk2, bool chk3, string mmcode, string maxcnt, string recalc, string icalc, int page_index, int page_size, string sorters)
        {
            string addStr2 = "";
            string addStr3 = "";
            string addStr5 = "";
            string addStr6 = "";
            string addStr7 = "";

            if (agenno != "")
                addStr3 = " and MAST.agen_no=:p4 ";

            // 本日出帳
            //if (chk1)
            //    addStr4 = " and(SELECT COUNT(mmcode) FROM MI_WHTRNS WHERE wh_no =:p0 and trunc(tr_date) = trunc(sysdate) and tr_mcode = 'APLO' and mmcode = INV.mmcode) > 0 ";
            // 已轉出訂單
            if (chk2)
                addStr5 = " and POT.ISTRAN in ('Y','T') ";
            // 申購中
            if (chk3)
                addStr6 = " and POT.ISTRAN in ('N') ";

            // 院內碼
            if (mmcode != "")
                addStr7 = " AND MAST.MMCODE like :p8 ";

            var p = new DynamicParameters();
            var sql = "";
            sql = @"SELECT  WH_NO, MMCODE, INV_QTY, APL_OUTQTY, APL_INQTY, SAFE_QTY, OPER_QTY, SHIP_QTY, HIGH_QTY, MIN_ORDQTY, UNIT_SWAP,
                    LOW_QTY, ALLQTY, ESTQTY, E_VACCINE, E_RESTRICTCODE, E_MANUFACT, 0 SUMINQTY, 0 INWAY_QTY, ADVQTY_OLD,  PACK_QTY0, ADVISEQTY,                            
                    (CASE WHEN ISTRAN<>'N' then '' WHEN (CREATE_TIME - MINTIME) > 0.0001158 THEN 'V' ELSE '' END) NEWFLAG,   -- insert時 create_time 不會相同, 10秒內視為同時新增                                    
                    (CASE WHEN TODAYCNT <> 0 THEN 'V' ELSE '' END) CHGFLAG,                                       
                    PO_QTY, TO_CHAR(round(PO_QTY*PO_PRICE),'999,999,999') PO_AMT, PO_PRICE, DISC_CPRICE,
                    MMNAME_E,  AGEN_NAME, M_PURUN, CONTRACNO, PURTYPE, E_PURTYPE, AGEN_NO,  M_DISCPERC, 
                    ISTRAN_1,  ISTRAN, MEMO, CALC   
                    FROM   
                    (SELECT POT.WH_NO, POT.MMCODE, INVCTL.INV_QTY, INVCTL.APL_OUTQTY, INVCTL.APL_INQTY, ROUND(INVCTL.SAFE_QTY) SAFE_QTY, ROUND(INVCTL.OPER_QTY) OPER_QTY, ROUND(INVCTL.SHIP_QTY) SHIP_QTY, 
					ROUND(INVCTL.HIGH_QTY) HIGH_QTY, INVCTL.MIN_ORDQTY, '1' UNIT_SWAP,
                    round(INVCTL.LOW_QTY) LOW_QTY, round(nvl(ALLQTY.ALLQTY,0)) ALLQTY, NVL2(EQPD.ESTQTY, EQPD.ESTQTY, 0) ESTQTY, MAST.E_VACCINE,  MAST.E_RESTRICTCODE,  MAST.E_MANUFACT, MAST.M_CONTPRICE,  POT.CREATE_TIME, POT.PACK_QTY0,
                    POT.ADVISEQTY ,  MAST.MMNAME_E,  MAST.AGEN_NAME, MAST.M_PURUN, MAST.CONTRACNO, TM.MINTIME, 
                    (CASE WHEN POT.E_PURTYPE='1' THEN '甲案' WHEN POT.E_PURTYPE='2' THEN '乙案' END)  PURTYPE , POT.E_PURTYPE, MAST.AGEN_NO,    MAST.M_DISCPERC,  nvl(MAST.M_CONTPRICE,0) AS PO_PRICE, nvl(MAST.DISC_CPRICE,0) AS DISC_CPRICE,
                    POT.PO_QTY  AS PO_QTY, POT.ADVQTY_OLD, POT.CALC, 
                    CASE WHEN POT.ISTRAN='N' THEN '申購' WHEN POT.ISTRAN='Y' THEN '轉訂單' WHEN POT.ISTRAN='T' THEN '轉訂單' END AS ISTRAN_1,    NVL2(POT.ISTRAN, POT.ISTRAN, 'N') ISTRAN, POT.MEMO,    NVL2(POT.PO_QTY,1,0) AS FLAG,
                    (SELECT NVL(COUNT(MMCODE),0) CNT FROM MI_WHTRNS
                    WHERE POT.WH_NO=:p0  AND TRUNC(TR_DATE) = TRUNC(SYSDATE) AND TR_MCODE='APLO' AND MMCODE=MAST.MMCODE ) TODAYCNT        
                    FROM (select CTL.WH_NO, CTL.MMCODE, nvl(INV.INV_QTY,0) INV_QTY, 
						       nvl(INV.APL_OUTQTY,0) APL_OUTQTY, nvl(INV.APL_INQTY,0) APL_INQTY,
						       CTL.SAFE_QTY, CTL.OPER_QTY, CTL.SHIP_QTY, CTL.HIGH_QTY, CTL.MIN_ORDQTY, CTL.LOW_QTY			   
						   from MI_WHINV INV, MI_WINVCTL CTL
						   where  CTL.WH_NO=INV.WH_NO(+) AND  CTL.MMCODE=INV.MMCODE(+)
                             and CTL.WH_NO =:p0) INVCTL, MM_PO_T POT,
                    (SELECT M.MMCODE, M.MMNAME_E, (P.AGEN_NO||'_'||P.AGEN_NAMEC) AS AGEN_NAME, M.M_PURUN, M.CONTRACNO, P.AGEN_NO,
                    NVL(M.M_CONTPRICE,0) M_CONTPRICE, NVL(M.DISC_CPRICE,0) DISC_CPRICE, NVL(M.M_DISCPERC,0) M_DISCPERC,  
                    (CASE WHEN M.E_RESTRICTCODE='0' THEN '它管' WHEN M.E_RESTRICTCODE='1' THEN '管一' WHEN M.E_RESTRICTCODE='2' THEN '管二' 
                    WHEN M.E_RESTRICTCODE='3' THEN '管三' WHEN M.E_RESTRICTCODE='4' THEN '管四' END ) E_RESTRICTCODE,
                    (CASE WHEN M.E_VACCINE='N' THEN '' ELSE M.E_VACCINE END ) E_VACCINE, E_MANUFACT            
                    FROM MI_MAST M, PH_VENDER P
                    WHERE  M.M_AGENNO=P.AGEN_NO   AND M.MAT_CLASS='01'  ) MAST,  
                    (SELECT MMCODE, SUM(INV_QTY) ALLQTY FROM MI_WHINV  GROUP BY MMCODE ) ALLQTY,
                    (SELECT MMCODE, ESTQTY FROM PH_EQPD WHERE WH_NO=:p0 AND RECYM=TWN_YYYMM(sysdate)) EQPD,
                    (select min(create_time) MINTIME from MM_PO_T where PURDATE =:purdate ) TM
                    WHERE MAST.MMCODE =INVCTL.MMCODE(+)
						AND MAST.MMCODE = ALLQTY.MMCODE(+) 
                        AND MAST.MMCODE =EQPD.MMCODE(+)  						
                    AND MAST.MMCODE =POT.MMCODE   
                        AND POT.PURDATE =:purdate " + addStr3 + addStr5 + addStr6 + addStr7 + @"
                        AND EXISTS  
                        (SELECT 1 FROM MI_MAST 
                    WHERE MMCODE=MAST.MMCODE AND MAT_CLASS='01' AND ROWNUM=1) 
                        ) " + string.Format(" ORDER BY {0} ", WorkSession.GetSortStatement(sorters));

            p.Add(":p0", wh_no);
            p.Add(":purdate", purdate);
            p.Add(":p4", agenno);
            p.Add(":p8", mmcode + '%');
            //p.Add(":MAXCNT", maxcnt);
            p.Add(":reCalc", recalc);
            p.Add(":icalc", icalc);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<BD0009>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<BD0009> GetImportItems(string wh_no, string purdate, string ltype, string[] arr_mmcode, string agen_no, int page_index, int page_size, string sorters)
        {
            string addStr = @"";
            string mmcodeInStr = "";
            string agennoStr = "";
            var p = new DynamicParameters();
            if (arr_mmcode.Length > 0)
            {
                foreach (string p0 in arr_mmcode)
                {
                    mmcodeInStr += "'" + p0 + "',";
                }
                mmcodeInStr = mmcodeInStr.Substring(0, mmcodeInStr.Length - 1);
            }
            if (ltype == "1")   // 庫存量<=安全存量(安全存量>0 或 最低庫存量>0)
            {
                addStr = @" AND substr(MAST.mmcode,1,3) in (";
                addStr += mmcodeInStr + ")";
                addStr += @" AND AGEN_NO not in ('000','300','990','999') 
                             AND ( (MAST.E_PURTYPE='1' and
                                   AllQty.AllQty<=INVCTL.SAFE_QTY and INVCTL.SAFE_QTY<>0) or
                                  (MAST.E_PURTYPE='2' and
                                   ( (INVCTL.INV_QTY<=INVCTL.SAFE_QTY and INVCTL.SAFE_QTY<>0) or
                                     (INVCTL.INV_QTY<=INVCTL.LOW_QTY  and INVCTL.LOW_QTY<>0) )
                                  )
                                )
                            ";
            }
            else if (ltype == "2")  // 庫存量<=安全庫量(安全存量=0 且 最低庫存量=0)
            {
                addStr = @" AND substr(MAST.mmcode,1,3) in (";
                addStr += mmcodeInStr + ")";
                addStr += @" AND AGEN_NO not in ('000','300','990','999') 
                             AND ( (MAST.E_PURTYPE='1' and
                                    AllQty.AllQty<=INVCTL.SAFE_QTY and INVCTL.SAFE_QTY=0) or
                                    (MAST.E_PURTYPE='2' and
                                     ( (INVCTL.INV_QTY<=INVCTL.SAFE_QTY and INVCTL.SAFE_QTY=0) and
                                       (INVCTL.INV_QTY<=INVCTL.LOW_QTY  and INVCTL.LOW_QTY=0) )
                                   )
                                 )
                            ";
            }
            else if (ltype == "3")  // 庫存量>安全存量(安全存量>0 或 最低庫存量>0)
            {
                addStr = @" AND substr(MAST.mmcode,1,3) in (";
                addStr += mmcodeInStr + ")";
                addStr += @" AND ( (MAST.E_PURTYPE='1' and
                                 AllQty.AllQty>INVCTL.SAFE_QTY and INVCTL.SAFE_QTY<>0) or
                                (MAST.E_PURTYPE='2' and
                                 ( (INVCTL.INV_QTY>INVCTL.SAFE_QTY and INVCTL.SAFE_QTY<>0) or
                                   (INVCTL.INV_QTY>INVCTL.LOW_QTY  and INVCTL.LOW_QTY<>0) )
                                 )
                               )
";
            }
            else if (ltype == "4")  // 庫存量>安全存量(安全存量=0 且 最低庫存量=0)
            {
                addStr = @" AND substr(MAST.mmcode,1,3) in (";
                addStr += mmcodeInStr + ")";
                addStr += @" AND ( (MAST.E_PURTYPE='1' and
                                  AllQty.AllQty>INVCTL.SAFE_QTY and INVCTL.SAFE_QTY=0) or
                                 (MAST.E_PURTYPE='2' and
                                  ( (INVCTL.INV_QTY>INVCTL.SAFE_QTY and INVCTL.SAFE_QTY=0) and
                                    (INVCTL.INV_QTY>INVCTL.LOW_QTY  and INVCTL.LOW_QTY=0) )
                                 )
                               )
 ";
            }
            else
            {
                addStr = @" AND substr(MAST.mmcode,1,3) in (";
                addStr += mmcodeInStr + ")";
            }
            if (agen_no != "")
            {
                agennoStr = " and M.m_agenno =:agen_no ";
                p.Add(":agen_no", agen_no);
            }
            var sql = @" SELECT WH_NO, MMCODE, INV_QTY, APL_OUTQTY, APL_INQTY, SAFE_QTY, OPER_QTY, SHIP_QTY, HIGH_QTY, MIN_ORDQTY, UNIT_SWAP,   LOW_QTY, ALLQTY, E_VACCINE, E_RESTRICTCODE, E_MANUFACT,
                        (case when TODAYFLAG > 0 then 'V' else ' ' end) TODAYFLAG,  0 as PACK_QTY0,
                        ADVISEQTY, ADVISEQTY AS PO_QTY,  ADVISEQTY* PO_PRICE as PO_AMT, 
                        PO_PRICE, DISC_CPRICE, MMNAME_E,  AGEN_NAME, M_PURUN, CONTRACNO, PURTYPE, E_PURTYPE, AGEN_NO,  M_DISCPERC, useqty
                        FROM
                        (SELECT      INVCTL.WH_NO, INVCTL.MMCODE, INVCTL.INV_QTY, INVCTL.APL_OUTQTY, INVCTL.APL_INQTY, ROUND(INVCTL.SAFE_QTY) SAFE_QTY,
                        ROUND(INVCTL.OPER_QTY) OPER_QTY, ROUND(INVCTL.SHIP_QTY) SHIP_QTY, ROUND(INVCTL.HIGH_QTY) HIGH_QTY, INVCTL.MIN_ORDQTY, '1' UNIT_SWAP,
                        round(INVCTL.LOW_QTY) LOW_QTY, round(nvl(ALLQTY.ALLQTY,0)) ALLQTY, MAST.E_VACCINE, MAST.E_RESTRICTCODE, MAST.E_MANUFACT,
                        BD0009_ADV_QTY(MAST.E_PURTYPE, round(INVCTL.INV_QTY), round(nvl(ALLQTY.ALLQTY,0)), round(INVCTL.OPER_QTY), round(INVCTL.SAFE_QTY), round(INVCTL.LOW_QTY), INVCTL.MIN_ORDQTY, 0, 1) ADVISEQTY,
                        MAST.MMNAME_E, MAST.AGEN_NAME, MAST.M_PURUN, MAST.CONTRACNO, MAST.PURTYPE, MAST.E_PURTYPE, MAST.AGEN_NO, MAST.M_DISCPERC, nvl(MAST.M_CONTPRICE, 0) AS PO_PRICE, nvl(MAST.DISC_CPRICE, 0) AS DISC_CPRICE,
                        (SELECT NVL(COUNT(MMCODE), 0) FROM MI_WHTRNS
                        WHERE WH_NO =:p0   AND TRUNC(TR_DATE) = TRUNC(SYSDATE) AND TR_MCODE = 'APLO' AND MMCODE = MAST.MMCODE) TODAYFLAG,
                        round(nvl(USEQTY.USEQTY,0)) USEQTY
                        FROM 
						(SELECT CTL.WH_NO, CTL.MMCODE, NVL(INV.INV_QTY,0) INV_QTY, 
						       NVL(INV.APL_OUTQTY,0) APL_OUTQTY, NVL(INV.APL_INQTY,0) APL_INQTY,
						       CTL.SAFE_QTY, CTL.OPER_QTY, CTL.SHIP_QTY, CTL.HIGH_QTY, CTL.MIN_ORDQTY, CTL.LOW_QTY			   
						   FROM MI_WHINV INV, MI_WINVCTL CTL
						   WHERE  CTL.WH_NO=INV.WH_NO(+) AND  CTL.MMCODE=INV.MMCODE(+) AND CTL.WH_NO =:p0) INVCTL,
                        (SELECT M.MMCODE, M.MMNAME_E, (P.AGEN_NO || '_' || P.AGEN_NAMEC) AS AGEN_NAME, M.M_PURUN, M.CONTRACNO, P.AGEN_NO,
                        NVL(M.M_CONTPRICE, 0) M_CONTPRICE, NVL(M.DISC_CPRICE,0) DISC_CPRICE, NVL(M.M_DISCPERC, 0) M_DISCPERC,  (CASE WHEN E_PURTYPE = '1' THEN '甲案' WHEN E_PURTYPE = '2' THEN '乙案' END)  PURTYPE, E_PURTYPE,   (CASE WHEN M.E_RESTRICTCODE = '0' THEN '它管' WHEN M.E_RESTRICTCODE = '1' THEN '管一' WHEN M.E_RESTRICTCODE = '2' THEN '管二'
                        WHEN M.E_RESTRICTCODE = '3' THEN '管三' WHEN M.E_RESTRICTCODE = '4' THEN '管四' END ) E_RESTRICTCODE,
                        (CASE WHEN M.E_VACCINE = 'N' THEN '' ELSE M.E_VACCINE END ) E_VACCINE, E_MANUFACT
                        FROM MI_MAST M, PH_VENDER P
                        WHERE  M.M_AGENNO = P.AGEN_NO  AND M.MAT_CLASS = '01' 
                          and m.E_PARCODE <> '2'" + agennoStr;
            sql += @"   ) MAST, 
                        (SELECT MMCODE, SUM(INV_QTY) ALLQTY FROM MI_WHINV GROUP BY MMCODE ) ALLQTY,
                        (SELECT MMCODE, SUM(USE_QTY) USEQTY FROM MI_WHINV GROUP BY MMCODE ) USEQTY
                        WHERE MAST.MMCODE =INVCTL.MMCODE(+)
						AND MAST.MMCODE = ALLQTY.MMCODE(+)
                        AND MAST.MMCODE = USEQTY.MMCODE(+)
                        AND EXISTS(SELECT 1 FROM MI_MAST
                        WHERE MMCODE = MAST.MMCODE AND MAT_CLASS = '01' AND ROWNUM = 1) " + addStr;
            sql += @"   AND (EXISTS (select 1 from mi_mast a, MI_WINVCTL b  
                             where a.mmcode = MAST.mmcode and a.mmcode = b.mmcode and b.wh_no =:p0 and mat_class = '01' and m_agenno = '000' and E_ORDERDCFLAG <> 'Y' and  E_DRUGCLASS <> '9'
                           and b.CTDMDCCODE <> 1  AND ROWNUM = 1)  or MAST.AGEN_NO <> '000')
                        ";  //000廠商--無合約抓取邏輯
            sql += " )   ";
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            p.Add(":p0", wh_no);
            return DBWork.Connection.Query<BD0009>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        //針對當天藥庫庫存異動品項重新計算 ADVISEQTY
        public int UpdateADVISEQTY(string purdate, string update_user, string update_ip)
        {
            var sql = @" update MM_PO_T a set
                           INV_QTY = (SELECT INV_QTY  FROM MI_WHINV where WH_NO =a.WH_NO and mmcode=a.mmcode ),
                           ADVQTY_OLD = ADVISEQTY, 
                           ADVISEQTY = BD0009_ADV_QTY(E_PURTYPE, 
                             (SELECT INV_QTY  FROM MI_WHINV where WH_NO =a.WH_NO and mmcode=a.mmcode ),  ---INV_QTY, 
                             (SELECT SUM(INV_QTY)  FROM MI_WHINV where mmcode=a.mmcode ),  --ALL_QTY
                             OPER_QTY, SAFE_QTY, LOW_QTY, MIN_ORDQTY, PACK_QTY0, CALC),
                           update_time=sysdate, update_user =:UPDATE_USER, update_ip=:UPDATE_IP
                        where purdate=:PURDATE and ISTRAN='N'
                           and mmcode in 
                             (SELECT mmcode FROM MI_WHTRNS where WH_NO=A.WH_NO  AND TRUNC(TR_DATE) = TRUNC(SYSDATE) 
                               AND TR_MCODE='APLO' AND MMCODE=a.MMCODE
                              ) ";
            return DBWork.Connection.Execute(sql, new { UPDATE_USER = update_user, UPDATE_IP = update_ip, PURDATE = purdate }, DBWork.Transaction);
        }
        public int recalc(string purdate, string update_user, string update_ip, string icalc)
        {
            //ADVQTY_OLD = ADVISEQTY, 
            //ADVISEQTY = BD0009_ADV_QTY(E_PURTYPE, INV_QTY, ALLQTY, OPER_QTY, SAFE_QTY, LOW_QTY, MIN_ORDQTY, PACK_QTY0, 1), 
            //DISAMOUNT　目前沒有顯示先不計算, 
            var sql = @" update MM_PO_T set
                        PO_QTY = round(ADVISEQTY * calc / min_ordqty) * min_ordqty, 
                        PO_AMT= round(PO_PRICE*(round(ADVISEQTY * calc / min_ordqty) * min_ordqty) ),
                        DISAMOUNT= round(PO_PRICE*(round(ADVISEQTY * calc / min_ordqty) * min_ordqty)-DISC_CPRICE*(round(ADVISEQTY * calc / min_ordqty) * min_ordqty)),
                        update_time=sysdate, update_user =:UPDATE_USER, update_ip=:UPDATE_IP
                        where purdate=:PURDATE and PO_QTY > 0 and ADVISEQTY > 0 and ISTRAN='N' ";
            return DBWork.Connection.Execute(sql, new { UPDATE_USER = update_user, UPDATE_IP = update_ip, PURDATE = purdate, iCalc = icalc }, DBWork.Transaction);
        }
        public int recalc_1(string purdate, string update_user, string update_ip, string icalc)
        {
            var sql = @" update MM_PO_T set calc=:iCalc
                        where purdate=:PURDATE and ISTRAN='N' ";
            return DBWork.Connection.Execute(sql, new { PURDATE = purdate, iCalc = icalc }, DBWork.Transaction);
        }
        public int UpdateMmPot(BD0009 bd0009)
        {
            var sql = @" Update MM_PO_T set PO_QTY = :PO_QTY , MEMO = :MEMO,
                        PO_AMT=round(PO_PRICE *:PO_QTY), DISAMOUNT=round(PO_PRICE *:PO_QTY -DISC_CPRICE *:PO_QTY), 
                        UPDATE_USER=:CREATE_USER , UPDATE_TIME=sysdate, UPDATE_IP=:UPDATE_IP
                        Where purdate= :PURDATE and mmcode= :MMCODE and wh_no= :WH_NO and ISTRAN='N' ";
            return DBWork.Connection.Execute(sql, bd0009, DBWork.Transaction);
        }
        public int UpdateMmPot2(BD0009 bd0009)
        {
            var sql = @" Update MM_PO_T set ADVQTY_OLD = ADVISEQTY,
                        ADVISEQTY=:ADVISEQTY, INV_QTY =:INV_QTY, 
                        SAFE_QTY = :SAFE_QTY, OPER_QTY = :OPER_QTY, 
                        MIN_ORDQTY= :MIN_ORDQTY, LOW_QTY= :LOW_QTY, ALLQTY=:ALLQTY,
                        UPDATE_USER=:CREATE_USER , UPDATE_TIME=sysdate, UPDATE_IP=:UPDATE_IP
                        Where purdate= :PURDATE and mmcode= :MMCODE and wh_no= :WH_NO and ISTRAN='N' ";
            return DBWork.Connection.Execute(sql, bd0009, DBWork.Transaction);
        }

        public int InsertMmPot(BD0009 bd0009)
        {
            var sql = @" insert into MM_PO_T (PURDATE, WH_NO, CONTRACNO, AGEN_NO, ISTRAN, MMCODE, PO_QTY, PO_PRICE, M_PURUN, 
                        PO_AMT, M_DISCPERC, MEMO, UNIT_SWAP, ADVISEQTY, E_PURTYPE, CREATE_TIME, CREATE_USER, UPDATE_IP,
                        SAFE_QTY, OPER_QTY, LOW_QTY, MIN_ORDQTY, INV_QTY, ALLQTY, ADVQTY_OLD, CALC, PACK_QTY0, DISAMOUNT, DISC_CPRICE) 
                        VALUES (:PURDATE, :WH_NO, :CONTRACNO, :AGEN_NO, :ISTRAN, :MMCODE, round(:PO_QTY*:CALC/:MIN_ORDQTY) * :MIN_ORDQTY, :PO_PRICE, :M_PURUN, 
                        round(:PO_QTY *:CALC * :PO_PRICE), :M_DISCPERC, :MEMO, :UNIT_SWAP, :ADVISEQTY, :E_PURTYPE, sysdate, :CREATE_USER, :UPDATE_IP, 
                        :SAFE_QTY, :OPER_QTY, :LOW_QTY, :MIN_ORDQTY, :INV_QTY, :ALLQTY, :ADVQTY_OLD, :CALC, :PACK_QTY0, round(:PO_PRICE*:CALC*:PO_QTY - :DISC_CPRICE*:CALC*:PO_QTY), :DISC_CPRICE) ";
            return DBWork.Connection.Execute(sql, bd0009, DBWork.Transaction);
        }

        public int UpdateTran(BD0009 bd0009)
        {   // PO_QTY = 0 、 agen_no='000' 不轉訂單
            var sql = @" update MM_PO_T 
                        set ISTRAN='Y' ,UPDATE_USER=:CREATE_USER , UPDATE_TIME=sysdate, UPDATE_IP=:UPDATE_IP
                        where purdate= :PURDATE and mmcode= :MMCODE and wh_no= :WH_NO and po_qty <> 0 and ISTRAN='N' and agen_no<>'000' ";
            return DBWork.Connection.Execute(sql, bd0009, DBWork.Transaction);
        }

        public int updateUpdateUser(string purdate, string update_user)
        {
            var sql = @" update MM_PO_T set UPDATE_TIME=sysdate, UPDATE_USER=:UPDATE_USER
                        where purdate=:PURDATE and ISTRAN='N' ";

            return DBWork.Connection.Execute(sql, new { PURDATE = purdate, UPDATE_USER = update_user }, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetWH_NoCombo(string userid)
        {
            var p = new DynamicParameters();

            var sql = @"select distinct B.WH_NO as VALUE, B.WH_NO ||' '||B.WH_NAME as COMBITEM  from  MI_WHMAST B
                            where B.WH_GRADE IN ('1', '5') AND B.WH_KIND = '0' ";
            p.Add(":p0", userid);

            sql += " order by B.WH_NO ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p, DBWork.Transaction);
        }


        // 檢查指定採購日是否有資料
        public int ChkMmPoT(string purdate)
        {
            var p = new DynamicParameters();
            var sql = @" select count(*) from MM_PO_T where PURDATE = :p0 ";

            p.Add(":p0", purdate);

            return DBWork.Connection.QueryFirst<int>(sql, p, DBWork.Transaction);
        }

        public int ChkMmPoTMmcode(string purdate, string mmcode)
        {
            var p = new DynamicParameters();
            var sql = @" select count(*) from MM_PO_T where PURDATE = :p0 and MMCODE = :p1 ";

            p.Add(":p0", purdate);
            p.Add(":p1", mmcode);

            return DBWork.Connection.QueryFirst<int>(sql, p, DBWork.Transaction);
        }

        // 報表取明細資料
        public IEnumerable<BD0009> GetReport(string wh_no, string purdate)
        {
            string sql = @"select a.mmcode, a.M_PURUN, substr(b.mmname_e,1,25) as mmname_e, po_qty, PO_PRICE PO_PRICE,
                            round(SAFE_QTY) as SAFE_QTY, round(OPER_QTY) as OPER_QTY, round(INV_QTY) as INV_QTY, a.ALLQTY, 
                            AdviseQty, PO_AMT, a.agen_no||'_'||rtrim(c.easyname) as AGEN_NAME ,MEMO, a.CONTRACNO,
                            case when a.e_purtype='1' then '甲' when a.e_purtype='2' then '乙' end as E_PURTYPE
                            from MM_PO_T a, MI_MAST b, ph_vender c
                            where a.mmcode=b.mmcode and a.agen_no=c.agen_no and a.wh_no=:WH_NO and purdate =:PURDATE 
                              and a.po_qty <> 0 and a.agen_no<>'000'
                            ";

            sql += " ORDER BY a.agen_no, a.MMCODE ";

            return DBWork.Connection.Query<BD0009>(sql, new { WH_NO = wh_no, PURDATE = purdate }, DBWork.Transaction);
        }

        public int GetReportTotalCnt(string wh_no, string purdate)
        {
            var sql = @" select count(*) from MM_PO_T a, MI_MAST b, ph_vender c, 
                            (select CTL.MMCODE,LOW_QTY,SAFE_QTY, OPER_QTY,INV_QTY FROM MI_WHINV INV, MI_WINVCTL CTL
                            where CTL.WH_NO=INV.wh_no(+) and CTL.MMCODE=INV.MMCODE(+) and CTL.WH_NO=:WH_NO) d
                            where a.mmcode=b.mmcode and a.mmcode=d.mmcode and a.agen_no=c.agen_no and a.wh_no=:WH_NO and purdate =:PURDATE 
                              and a.po_qty <> 0 and a.agen_no<>'000'
                            ";

            return DBWork.Connection.QueryFirst<int>(sql, new { WH_NO = wh_no, PURDATE = purdate }, DBWork.Transaction);
        }

        // 建議採購量總金額
        public int GetReportTotalAdPrice(string wh_no, string purdate)
        {
            var sql = @" select sum(round(AdviseQty * a.PO_PRICE)) as CNT from MM_PO_T a, ph_vender c, 
                            (select CTL.MMCODE,LOW_QTY,SAFE_QTY, OPER_QTY,INV_QTY FROM MI_WHINV INV, MI_WINVCTL CTL
                            where CTL.WH_NO=INV.wh_no(+) and CTL.MMCODE=INV.MMCODE(+) and CTL.WH_NO=:WH_NO) d
                            where a.mmcode=d.mmcode and a.agen_no=c.agen_no and a.wh_no=:WH_NO and purdate =:PURDATE 
                              and a.po_qty <> 0 and a.agen_no<>'000'
                            ";

            return DBWork.Connection.QueryFirst<int>(sql, new { WH_NO = wh_no, PURDATE = purdate }, DBWork.Transaction);
        }

        // 實際採購量總金額
        public int GetReportTotalPrice(string wh_no, string purdate)
        {
            var sql = @" select sum(round(a.PO_QTY *  a.PO_PRICE)) as CNT from MM_PO_T a,  ph_vender c, 
                            (select CTL.MMCODE,LOW_QTY,SAFE_QTY, OPER_QTY,INV_QTY FROM MI_WHINV INV, MI_WINVCTL CTL
                            where CTL.WH_NO=INV.wh_no(+) and CTL.MMCODE=INV.MMCODE(+) and CTL.WH_NO=:WH_NO) d
                            where a.mmcode=d.mmcode and a.agen_no=c.agen_no and a.wh_no=:WH_NO and purdate =:PURDATE 
                              and a.po_qty <> 0 and a.agen_no<>'000'
                            ";

            return DBWork.Connection.QueryFirst<int>(sql, new { WH_NO = wh_no, PURDATE = purdate }, DBWork.Transaction);
        }

        // 取得指定日期採購資料數
        public int GetMaxcnt(string purdate)
        {
            var sql = @" select nvl(max(INFLAG),0) INFLAG from MM_PO_T 
                            where purdate =:PURDATE ";

            return DBWork.Connection.QueryFirst<int>(sql, new { PURDATE = purdate }, DBWork.Transaction);
        }

        public int GetGridTotalCnt(string purdate)
        {
            var sql = @" select count(*) from MM_PO_T 
                            where purdate =:PURDATE 
                            ";

            return DBWork.Connection.QueryFirst<int>(sql, new { PURDATE = purdate }, DBWork.Transaction);
        }

        public int GetGridTotalPrice(string purdate)
        {
            var sql = @" select sum(round(A.po_qty* A.PO_PRICE)) from MM_PO_T A
                            where A.purdate =:PURDATE 
                            ";

            return DBWork.Connection.QueryFirst<int>(sql, new { PURDATE = purdate }, DBWork.Transaction);
        }

        public string CallSP_BD0009_INSERT(string recalc, string purdate, string wh_no, string userid, string procip, string icalc)
        {
            var p = new OracleDynamicParameters();
            p.Add("reCalc", value: recalc,
                dbType: OracleDbType.Varchar2,
                direction: ParameterDirection.Input,
                size: 1);

            p.Add("i_yyymmdd", value: purdate,
                dbType: OracleDbType.Varchar2,
                direction: ParameterDirection.Input,
                size: 7);

            p.Add("i_wh_no", value: wh_no,
                dbType: OracleDbType.Varchar2,
                direction: ParameterDirection.Input,
                size: 8);

            p.Add("i_userid", value: userid,
                dbType: OracleDbType.Varchar2,
                direction: ParameterDirection.Input,
                size: 8);

            p.Add("i_ip", value: procip,
                dbType: OracleDbType.Varchar2,
                direction: ParameterDirection.Input,
                size: 20);

            p.Add("iCalc", value: Convert.ToDouble(icalc),
                dbType: OracleDbType.Double,
                direction: ParameterDirection.Input);

            p.Add("ret_code",
                dbType: OracleDbType.Varchar2,
                direction: ParameterDirection.Output,
                size: 200);

            DBWork.Connection.Query("BD0009_INSERT", p, commandType: CommandType.StoredProcedure);
            return p.Get<OracleString>("ret_code").Value;
        }

        public string CallParamaterOutSP(string purdate, string wh_no, string inid, string userid, string procip, string procName)
        {
            var p = new OracleDynamicParameters();
            p.Add("i_yyymmdd", value: purdate,
                dbType: OracleDbType.Varchar2,
                direction: ParameterDirection.Input,
                size: 7);

            p.Add("i_wh_no", value: wh_no,
                dbType: OracleDbType.Varchar2,
                direction: ParameterDirection.Input,
                size: 8);

            p.Add("i_inid", value: inid,
                dbType: OracleDbType.Varchar2,
                direction: ParameterDirection.Input,
                size: 6);

            p.Add("i_userid", value: userid,
                dbType: OracleDbType.Varchar2,
                direction: ParameterDirection.Input,
                size: 8);

            p.Add("i_ip", value: procip,
                dbType: OracleDbType.Varchar2,
                direction: ParameterDirection.Input,
                size: 20);

            p.Add("ret_code",
                dbType: OracleDbType.Varchar2,
                direction: ParameterDirection.Output,
                size: 200);

            DBWork.Connection.Query(procName, p, commandType: CommandType.StoredProcedure);
            return p.Get<OracleString>("ret_code").Value;
        }
        public string GetCalc(string purdate)
        {
            var sql = @" select nvl(max(calc),1)  from MM_PO_T 
                            where purdate =:PURDATE  and ISTRAN='N' ";
            return DBWork.Connection.QueryFirst<string>(sql, new { PURDATE = purdate }, DBWork.Transaction);
        }
        public int ClearData(string purdate)
        {
            var sql = @" delete from MM_PO_T  
                         where purdate=:PURDATE and (istran = null or istran='N')";
            return DBWork.Connection.Execute(sql, new { PURDATE = purdate }, DBWork.Transaction);
        }
        public int DeleteRec(string purdate, string mmcode)
        {
            var sql = @" delete from  MM_PO_T 
                         where purdate=:PURDATE and mmcode=:MMCODE and (istran = null or istran='N')";
            return DBWork.Connection.Execute(sql, new { PURDATE = purdate, MMCODE = mmcode }, DBWork.Transaction);
        }

        public decimal GetPoQty(string purdate, string mmcode, string wh_no) {
            string sql = @"select po_qty from MM_PO_T
                            where purdate = :purdate
                              and wh_no = :wh_no
                              and mmcode = :mmcode";
            return DBWork.Connection.QueryFirst<decimal>(sql, new { purdate = purdate, mmcode = mmcode, wh_no = wh_no }, DBWork.Transaction);
        }

        public int RecalcDisRatio(string purdate) {
            string sql = @"
                update MM_PO_T c
                   set (c.ISWILLING, c.DISCOUNT_QTY, c.DISC_COST_UPRICE)
                       =(select b.ISWILLING, b.DISCOUNT_QTY, b.DISC_COST_UPRICE
                           from MI_MAST a, MILMED_JBID_LIST b
                          where a.MMCODE=c.MMCODE
                            and substr(a.E_YRARMYNO,1,3)=b.JBID_STYR
                            and a.E_ITEMARMYNO=b.BID_NO
                            and c.PURDATE=:purdate and c.ISTRAN='N'  --申請中
                        )
                 where exists
                        (select 1
                           from MI_MAST a, MILMED_JBID_LIST b
                          where a.MMCODE=c.MMCODE
                            and substr(a.E_YRARMYNO,1,3)=b.JBID_STYR
                            and a.E_ITEMARMYNO=b.BID_NO
                            and c.PURDATE=:purdate and c.ISTRAN='N'  --申請中
                        ) 
                   and c.PURDATE=:purdate and c.ISTRAN in ('N')  --申請中
            ";
            return DBWork.Connection.Execute(sql, new { purdate}, DBWork.Transaction);
        }
        public int CalcDisPoQty(string disRatio, string purdate, string userId, string updateIp) {
            string sql = @"
                update MM_PO_T
                   set PO_QTY=DISCOUNT_QTY, JBID_DISRATIO=:disRatio,
                       po_amt = po_price * discount_qty,
                       update_time = sysdate, update_user = :userId, update_ip = :updateIp
                 where ISWILLING='是'
                   and (PO_QTY < DISCOUNT_QTY)
                   and (PO_QTY >= (DISCOUNT_QTY * :disRatio/100))
                   and PURDATE=:purdate and ISTRAN='N'  --申請中
            ";
            return DBWork.Connection.Execute(sql, new { disRatio , purdate, userId, updateIp }, DBWork.Transaction);
        }

        public int UpdateIsWilling(string purdate) {
            string sql = @"
                update MM_PO_T c
                   set (c.ISWILLING, c.DISCOUNT_QTY, c.DISC_COST_UPRICE)
                       =(select b.ISWILLING, b.DISCOUNT_QTY, b.DISC_COST_UPRICE
                           from MI_MAST a, MILMED_JBID_LIST b
                          where a.MMCODE=c.MMCODE
                            and substr(a.E_YRARMYNO,1,3)=b.JBID_STYR
                            and a.E_ITEMARMYNO=b.BID_NO
                            and c.PURDATE=:PURDATE and c.ISTRAN='N'
                        )
                 where exists
                        (select 1
                           from MI_MAST a, MILMED_JBID_LIST b
                          where a.MMCODE=c.MMCODE
                            and substr(a.E_YRARMYNO,1,3)=b.JBID_STYR
                            and a.E_ITEMARMYNO=b.BID_NO
                            and c.PURDATE=:PURDATE and c.ISTRAN='N'  --申請中
                        ) 
                   and c.PURDATE=:PURDATE and c.ISTRAN in ('N')
            ";
            return DBWork.Connection.Execute(sql, new { purdate }, DBWork.Transaction);
        }
    }
}