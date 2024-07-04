using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace HIS14SUPDETtransfer
{
    public class HIS14SUPDETtransferRepository
    {
        // -- flylon
        const String sBr = "\r\n";
        public bool isInFlylon()
        {
            IPAddress[] aryIPAddress = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress ipAddress in aryIPAddress)
            {

                String sEachIp = ipAddress.ToString();
                if (IPAddress.Parse(sEachIp).AddressFamily == AddressFamily.InterNetwork)
                {
                    if (
                            (sEachIp.IndexOf("192.20.2") > -1) ||
                        false
                    )
                    {
                        return true;
                    }
                }
            }
            return false;
        } //


        public string Get01讀取HIS骨科手術衛材資料(string hosp_table_prefix)
        {
            ////1 MI_UNITCODE
            //string sql = string.Format(@"
            //                            SELECT DISTINCT strDrugUnit AS UNIT_CODE,
            //                                            strDrugUnit AS UI_CHANAME,
            //                                            strDrugUnit AS UI_ENGNAME,
            //                                            strDrugUnit AS UI_SNAME
            //                            FROM   HISDG.{0}XFDGUNIT
            //                            WHERE  strDrugUnit <> ' ' 
            //                        ", hosp_table_prefix);
            string sql = "";
            sql += sBr + "select -- a.* ";
            sql += sBr + "a.SUP_USEDATE, ";
            sql += sBr + "a.SUP_STKKIND, ";
            sql += sBr + "a.SUP_KIND, ";
            sql += sBr + "a.SUP_SKDIACODE, ";
            sql += sBr + "a.SUP_MEDNO, ";
            sql += sBr + "a.SUP_UNIT, ";
            sql += sBr + "a.SUP_COST, ";
            sql += sBr + "a.SUP_ATTACHQTY, ";
            sql += sBr + "a.SUP_ATTACHUNIT, ";
            sql += sBr + "a.SUP_USEQTY, ";
            sql += sBr + "a.SUP_EMPNO, ";
            sql += sBr + "a.SUP_SECTNO, ";
            sql += sBr + "a.SUP_DEPTCENTER, ";
            sql += sBr + "a.SUP_NRCODE, ";
            sql += sBr + "a.SUP_BEDNO, ";
            sql += sBr + "a.SUP_PROCDATE, ";
            sql += sBr + "a.SUP_PROCTIME, ";
            sql += sBr + "a.SUP_PROCOPID, ";
            sql += sBr + "a.SUP_PATIDNO, ";
            sql += sBr + "a.SUP_PATNAME, ";
            sql += sBr + "a.SUP_SENAME, ";
            sql += sBr + "a.SUP_EMPNAME, ";
            sql += sBr + "a.SUP_FEATOPID, ";
            sql += sBr + "a.SUP_ORDERKIND, ";
            sql += sBr + "'' endl ";
            sql += sBr + "from SUPDET a, -- HIS院區的HIS骨科手術衛材資料 ";
            sql += sBr + "     INACAR b  -- HIS院區的住院檔 ";
            sql += sBr + "Where 1=1 ";
            sql += sBr + "And SUP_PROCDATE=TO_CHAR(sysdate-1, 'RRRRMMDD')-19110000  -- 每日凌晨，讀取 SUP_PROCDATE(處理日期)=昨天 的資料 -- 衛材消耗日期 ";
            sql += sBr + "And SUP_SECTNO ='06' -- 科別 ";
            //sql += sBr + "And SUP_PATIDNO || SUP_PATNAME like '%Q20043%' -- 病患證號或姓名 ";
            sql += sBr + "And a.SUP_MEDNO=b.CA_MEDNO -- 病歷號碼 ";
            sql += sBr + "And SUP_USEDATE between b.CA_INPDATE and b.CA_OUTDATE  -- 衛材消耗日期 在 住院日期 且 出院日期 ";
            sql += sBr + "And b.CA_CHECK <>'D' -- (M)核對碼, N.護理站未核對 Y.護理站已核對 D.作廢(已核對不可作廢), S.簡易入院已核對 C.門診轉住院 B.自費購買(Ex.印收據,診斷書) ";
            sql += sBr + "And b.CA_DIVNO < 1000 -- 切帳序號 ";
            sql += sBr + "And b.CA_INPDATE <> ' ' -- 住院日期 ";
            sql += sBr + "And b.CA_OUTDATE <> ' ' -- 出院日期 ";
            sql += sBr + "And b.CA_DIVINSU = 'N' -- 切帳註記 ";
            String debug_sql = sql;
            //foreach (var name in p.ParameterNames)
            //{
            //    var val = p.Get<dynamic>(name);
            //    debug_sql = debug_sql.Replace(":" + name, "'" + val + "'");
            //}
            //debug_sql = debug_sql.Replace(":whno", "'" + whno + "'");
            if (isInFlylon() && debug_sql.Length > 0)
                System.IO.File.AppendAllText(
                                @"D:\wwwroot\InadArea\" + System.DateTime.Now.ToString("yyyyMMdd") + "_tsghmm_HIS14SUPDETtransferRepository.txt",
                                sBr + System.DateTime.Now.ToString("yyyyMMddHHmmss") + sBr + "-- Get01讀取HIS骨科手術衛材資料" + sBr + debug_sql);
            return sql;
        } // 
        public string Insert_MMSMS_HIS14_SUPDET()
        {
            //1 MI_UNITCODE
            string sql = @"";
            sql += sBr + "INSERT INTO HIS14_SUPDET (";
            sql += sBr + "    SUPDET_SEQ, SUP_USEDATE, SUP_STKKIND, SUP_KIND, SUP_SKDIACODE, SUP_MEDNO, SUP_UNIT, SUP_COST, SUP_ATTACHQTY, SUP_ATTACHUNIT, SUP_USEQTY, SUP_EMPNO, SUP_SECTNO, SUP_DEPTCENTER, SUP_NRCODE, SUP_BEDNO, SUP_PROCDATE, SUP_PROCTIME, SUP_PROCOPID, SUP_PATIDNO, SUP_PATNAME, SUP_SENAME, SUP_EMPNAME, SUP_FEATOPID, SUP_ORDERKIND, DOCNO, CREATE_TIME, UPDATE_TIME, UPDATE_USER, UPDATE_IP ";
            sql += sBr + ") ";
            sql += sBr + "select ";
            sql += sBr + "    (select max(nvl(SUPDET_SEQ,0))+1 from HIS14_SUPDET) SUPDET_SEQ, -- 骨科手術衛材資料流水號 ";
            sql += sBr + "    :SUP_USEDATE, -- 消耗日期 ";
            sql += sBr + "    :SUP_STKKIND, -- 扣庫單位 ";
            sql += sBr + "    :SUP_KIND, -- 消耗歸屬 ";
            sql += sBr + "    :SUP_SKDIACODE, -- 院內碼 ";
            sql += sBr + "    :SUP_MEDNO, -- 病歷號碼 ";
            sql += sBr + "    :SUP_UNIT, -- 耗用單位 ";
            sql += sBr + "    :SUP_COST, -- 成本 ";
            sql += sBr + "    :SUP_ATTACHQTY, -- 劑型數量 ";
            sql += sBr + "    :SUP_ATTACHUNIT, -- 劑型單位 ";
            sql += sBr + "    :SUP_USEQTY, -- 耗用數量 ";
            sql += sBr + "    :SUP_EMPNO, -- 員工編號 ";
            sql += sBr + "    :SUP_SECTNO, -- 隸屬科別 ";
            sql += sBr + "    :SUP_DEPTCENTER, -- 成本中心部門(責任中心) ";
            sql += sBr + "    :SUP_NRCODE, -- 護理站代碼 ";
            sql += sBr + "    :SUP_BEDNO, -- 病床號 ";
            sql += sBr + "    :SUP_PROCDATE, -- 處理日期 ";
            sql += sBr + "    :SUP_PROCTIME, -- 處理時間 ";
            sql += sBr + "    :SUP_PROCOPID, -- 處理人員 ";
            sql += sBr + "    :SUP_PATIDNO, -- 病患證號 ";
            sql += sBr + "    :SUP_PATNAME, -- 病患姓名 ";
            sql += sBr + "    :SUP_SENAME, -- 科別名稱 ";
            sql += sBr + "    :SUP_EMPNAME, -- 醫師姓名 ";
            sql += sBr + "    :SUP_FEATOPID, -- 醫師代碼 ";
            sql += sBr + "    :SUP_ORDERKIND, -- SUP_ORDERKIND ";
            sql += sBr + "    NULL, -- DOCNO, -- ●單據號碼(申請單號) ";
            sql += sBr + "    sysdate, -- 建立時間(讀HIS時間) ";
            sql += sBr + "    sysdate, -- 異動時間(產生申請單時間) ";
            sql += sBr + "    :UPDATE_USER, -- ●異動人員 ";
            sql += sBr + "    :UPDATE_IP -- ●異動IP ";
            sql += sBr + "from dual";
            return sql;
        }

        // -- flylon end 

        #region HISDG




        public string Get_MI_UNITCODE(string hosp_table_prefix)
        {
            //1 MI_UNITCODE
            string sql = string.Format(@"
                                        SELECT DISTINCT strDrugUnit AS UNIT_CODE,
                                                        strDrugUnit AS UI_CHANAME,
                                                        strDrugUnit AS UI_ENGNAME,
                                                        strDrugUnit AS UI_SNAME
                                        FROM   HISDG.{0}XFDGUNIT
                                        WHERE  strDrugUnit <> ' ' 
                                    ", hosp_table_prefix);
            return sql;
        }

        public string Get_PH_VENDER(string hosp_table_prefix)
        {
            //2 PH_VENDER
            string sql = string.Format(@"
                                        SELECT a.strSupplyID AS AGEN_NO,
                                               a.strSupplyName AS AGEN_NAMEC,
                                               a.strAddress AS AGEN_ADD,
                                               a.strFax AS AGEN_FAX,
                                               a.strTel1 AS AGEN_TEL,
                                               trim(b.strAccount) AS AGEN_ACC,
                                               a.strSupplyNumber AS UNI_NO,
                                               a.strOwner AS AGEN_BOSS,
                                               trim(a.strEmail) AS EMAIL,
                                               trim(a.strEmail1) AS EMAIL_1,
                                               substr(b.strBankID, 1, 3) AS AGEN_BANK,
                                               substr(b.strBankID, 4, length(trim(b.strBankID) - 3)) AS AGEN_SUB,
                                               b.strBankID as agen_bank_14,
                                               a.strSupplyNameSimp AS EASYNAME
                                        FROM   HISDG.{0}XFSUPPLI a
                                        JOIN HISDG.{0}XFBANKNO b
                                        ON a.strSupplyID = b.strSupplyID 
                                    ", hosp_table_prefix);
            return sql;
        }

        public string Get_MI_UNITEXCH(string hosp_table_prefix)
        {
            //3 MI_UNITEXCH
            string sql = string.Format(@"
                                        SELECT DISTINCT b.strDrugID    AS MMCODE,
                                                        b.strDrugUnit  AS UNIT_CODE,--取DrugUnit最小計量單位
                                                        a.strSupplyID  AS AGEN_NO,
                                                        1              AS EXCH_RATIO,--換算率=1
                                                        SYSDATE        AS CREATE_TIME,
                                                        '上線轉檔' AS CREATE_USER
                                        FROM   (SELECT *
                                                FROM   HISDG.{0}XFUTCOST
                                                WHERE  strDrugID <> ' ') a
                                            JOIN (SELECT *
                                                     FROM   HISDG.{0}XFCHEMIS
                                                     WHERE  strDrugID <> ' ') b
                                                 ON a.strDrugID = b.strDrugID

                                        UNION

                                        SELECT DISTINCT b.strDrugID    AS MMCODE,
                                                        b.strDrugPack  AS UNIT_CODE,--取DrugPack包裝單位
                                                        a.strSupplyID  AS AGEN_NO,
                                                        iUnitRate      AS EXCH_RATIO,--換算率=iUnitRate
                                                        SYSDATE        AS CREATE_TIME,
                                                        '上線轉檔' AS CREATE_USER
                                        FROM   (SELECT *
                                                FROM   HISDG.{0}XFUTCOST
                                                WHERE  strDrugID <> ' ') a
                                            JOIN (SELECT *
                                                     FROM   HISDG.{0}XFCHEMIS
                                                     WHERE  strDrugUnit <> strDrugPack
                                                            AND strDrugID <> ' '
                                                            AND strDrugPack <> ' ') b
                                                 ON a.strDrugID = b.strDrugID 
                                    ", hosp_table_prefix);
            return sql;
        }

        public string Get_UR_INID(string hosp_table_prefix)
        {
            //4 UR_INID
            string sql = string.Format(@"
                                        SELECT strDepID   AS INID,
                                               strDepName AS INID_NAME
                                        FROM   HISDG.{0}XFSTATIO
                                        WHERE  strIsUse = '1' 
                                    ", hosp_table_prefix);
            return sql;
        }

        public string Get_MI_MAST(string hosp_id, string hosp_table_prefix)
        {
            string sql = "";
            //6 MI_MAST
            sql = string.Format(@"
                                        SELECT 
                                          a.strDrugID AS MMCODE, 
                                          a.strDrugChi AS MMNAME_C, 
                                          a.strDrugEng AS MMNAME_E, 
                                          (
                                            CASE a.strXfKindID 
		                                        WHEN '1' THEN '01' 
                                                ELSE '02'
	                                        END
                                          ) AS MAT_CLASS, 
                                          a.strDrugUnit AS BASE_UNIT, 
                                          'N' AS AUTO_APLID, 
                                          '0' AS M_STOREID, 
                                          (
                                            CASE NVL(b.strTouchWay, '2') 
		                                        WHEN '1' THEN '0' 
		                                        WHEN '2' THEN '2' 
	                                        END
                                          ) AS M_CONTID, 
                                          a.strHealthID AS M_NHIKEY,
                                          (
                                            CASE 
		                                        WHEN (NVL(b.strArmyItem, ' ')= ' ' ) OR ( NVL(b.strArmyItem, '0')= '0') THEN 'N' 
		                                        ELSE 'Y' 
	                                        END
                                          ) AS M_MATID, 
                                          (
                                            CASE a.strCountCost 
		                                        WHEN '0' THEN '2' 
		                                        WHEN '1' THEN '1' 
	                                        END
                                          ) AS M_PAYID, 
                                          '2' AS M_TRNID, 
                                          'Y' AS M_APPLYID, 
                                          a.strDrugIssueNo AS M_PHCTNCO, 
                                          a.strDrugIssueDate AS M_ENVDT, 
                                          b.strSupplyID AS M_AGENNO, 
                                          b.strSupplyTag AS M_AGENLAB, 
                                          a.strDrugPack AS M_PURUN, 
                                          b.strTouchPrice AS M_CONTPRICE, 
                                          1 AS M_DISCPERC, 
                                          '0' AS E_SUPSTATUS, 
                                          a.strDrugMaker AS E_MANUFACT, 
                                          b.strArmyItem AS E_ITEMARMYNO, 
  (case when nvl(b.strArmyItem,' ')<>' '
        then (case a.strXfKindID 
                when '1' then '1' 
                else '2' 
              end)   
        else null
   end) as E_GPARMYNO,
                                          (CASE 
                                                WHEN LENGTH(strTouchDate)= 6 and to_number(SUBSTR(strTouchDate, 3, 2)) > 0 and to_number(SUBSTR(strTouchDate, 5, 2)) > 0 THEN  ( 
                                                    CASE 
                                                        WHEN SUBSTR(strTouchDate, 1, 1)>= '3' THEN (1911 + to_number(SUBSTR(strTouchDate, 1, 2))) || SUBSTR(strTouchDate, 3, 4) --例如911231
                                                        ELSE  '20' || strTouchDate --例如111231轉為20111231 
                                                    END ) 
                                                WHEN LENGTH(strTouchDate)= 7 and to_number(SUBSTR(strTouchDate, 4, 2)) > 0 and to_number(SUBSTR(strTouchDate, 6, 2)) > 0
                                                    THEN (1911 + to_number(SUBSTR(strTouchDate, 1, 3))) || SUBSTR(strTouchDate, 4, 4) --例如1110527
                                                ELSE NULL
                                            END) AS E_CODATE,  
                                          '1' AS E_PRESCRIPTYPE, 
  (case a.strXfKindID 
        when '1' 
        then (case nvl(strIsLimitClas,' ')
                when '1' then '2'
                when '2'  then '2'
                when '3' then '2'
                else '0'
              end)
        else 'X'
   end) as E_DRUGCLASSIFY, 

                                          'Y' AS E_INVFLAG,
                                          (
                                            CASE NVL(b.strPayWay, 'N') 
		                                        WHEN '1' THEN 'P' 
		                                        WHEN '2' THEN 'C' 
		                                        WHEN 'N' THEN 'N' 
	                                        END
                                          ) AS E_SOURCECODE, 
  (case a.strXfKindID 
     when '1' then '0' 
     else null
   end) as E_DRUGAPLTYPE,
                                          '0' AS E_PARCODE, 
                                          'N' AS WEXP_ID, 
                                          'N' AS WLOC_ID, 
                                         (
                                            CASE a.strIsDelete 
		                                        WHEN '1' THEN 'Y' 
		                                        WHEN '0' THEN 'N' 
	                                        END
                                          ) AS CANCEL_ID, 
                                          (
                                            CASE a.strIsLimitClas 
		                                        WHEN '0' THEN 'N' 
		                                        WHEN '1' THEN '1' 
		                                        WHEN '2' THEN '2' 
		                                        WHEN '3' THEN '3' 
		                                        WHEN '4' THEN '4' 
	                                        END
                                          ) AS E_RESTRICTCODE, 
                                          (
                                            CASE a.strIsDelete
                                                WHEN '1' THEN 'Y' WHEN '0' THEN 'N'
	                                        END
                                          ) AS E_ORDERDCFLAG, 
                                          'N' AS E_HIGHPRICEFLAG, 
                                          'N' AS E_RETURNDRUGFLAG, 
                                          'N' AS E_RESEARCHDRUGFLAG, 
  (case when length(a.strSpace)=6
        then (1911 + to_number(substr(a.strSpace,1,2))) ||
             substr(a.strSpace,3,4) --例如910718
        when length(a.strSpace)=7
        then (case when substr(a.strSpace,1,1)='0'
                   then (1911 + to_number(substr(a.strSpace,2,2))) ||
                        substr(a.strSpace,4,4) --例如0991101
                   else (1911 + to_number(substr(a.strSpace,1,3))) ||
                        substr(a.strSpace,4,4) --例如1061212
              end)
        else null
   end) as UPDATE_TIME,
                                          b.strTouchPrice AS UPRICE, 
                                          b.strUnitPrice AS DISC_CPRICE, 
                                          b.strUnitPrice AS DISC_UPRICE, 
                                          '0' AS PACKTYPE, 
                                          b.strHealthPrice AS NHI_PRICE,
                                          a.strXfKindID AS MAT_CLASS_SUB,
                                           a.strDrugSName AS DRUGSNAME,  
                                          a.strDrugHide AS DrugHide, 
a.strHealthPay as HealthPay,
  a.strCostKind as CostKind,
  a.strCaseDoct as CaseDoct,
  a.strDrugKind as DrugKind,
  a.strHealthOwnExp as HealthOwnExp,
  a.strIssueSupply as IssueSupply ,
  a.iUnitRate as UnitRate,
  a.strSpXfee as SpXfee,
  a.strWarBak as WarBak,
  a.strOrderKind as OrderKind,
  a.strWastKind as WastKind,
  b.strCaseNo as CaseNo,
  NVL(b.strTouchCase, '0') as TouchCase,
  NVL(b.lngContractAmt, '0') as ContractAmt,
  NVL(b.strContractSum, '0') as ContractSum,
  a.strCommon as Common,
  a.lngTrUTRate as TrUTRate,
  (case when length(b.strIssPriceDate)=6
        then (1911 + to_number(substr(b.strIssPriceDate,1,2))) ||
             substr(b.strIssPriceDate,3,4) --例如910718
        when length(b.strIssPriceDate)=7
        then (case when substr(b.strIssPriceDate,3,1)='.'  --803:93.08.0
                   then null  
                   when substr(b.strIssPriceDate,4,1)='/'  --805:095/04/
                   then null
                   when substr(b.strIssPriceDate,1,2)='00' --807:0000000
                   then null
                   when substr(b.strIssPriceDate,1,1)='0'
                   then (1911 + to_number(substr(b.strIssPriceDate,2,2))) ||
                        substr(b.strIssPriceDate,4,4) --例如0991101
                   else (1911 + to_number(substr(b.strIssPriceDate,1,3))) ||
                        substr(b.strIssPriceDate,4,4) --例如1061212
              end)
        else null
   end) as IssPriceDate,
  (case when length(b.strSpace)=6
        then (1911 + to_number(substr(b.strSpace,1,2))) ||
             substr(b.strSpace,3,4) --例如910718
        when length(b.strSpace)=7
        then (1911 + to_number(substr(b.strSpace,1,3))) ||
             substr(b.strSpace,4,4) --例如1061212
        else null
   end) as BEGINDATE_14,
            a.strOneCost as OneCost,
	        a.strSpDrug AS SpDrug,
            a.strFastDrug AS FastDrug
                                        FROM ( SELECT * FROM HISDG.{0}XFCHEMIS ) a 
                                        LEFT JOIN ( SELECT * FROM HISDG.{0}XFUTCOST WHERE strIsNew = '1' ) b 
                                        ON a.strDrugID = b.strDrugID
                                    ", hosp_table_prefix);

            return sql;
        }

        public string Get_MI_WHINV_HISDG_SubQuery(string wh_kind, string hosp_table_prefix)
        {
            //7 MI_WHINV
            string sql = string.Format(@"
                                                                    SELECT STRHOSPITAL, STRSTOREDEP, STRDRUGID,  round(LNGDRUGAMT, 24) as LNGDRUGAMT,     
                                                                    LNGSAFERO,   LNGISSUEOKAMT,  LNGNORMALRO, STRDEPID
                                                                    FROM   HISDG.{1}XFSTKTOL c
                                                                    WHERE  EXISTS(SELECT 1
                                                                                  FROM   HISDG.{1}XFCHEMIS
                                                                                  WHERE  strdrugid = c.strdrugid
                                                                                         AND strXfKindID {0} '1') 
                                    ", wh_kind == "0" ? "=" : "<>", hosp_table_prefix);
            return sql;
        }

        public string Get_MI_WHINV_MMSMS_SubQuery(string wh_kind)
        {
            //7 MI_WHINV
            string sql = string.Format(@"
                                        SELECT WH_NO, INID FROM MMSADM.MI_WHMAST WHERE WH_KIND = '{0}'
                                    ", wh_kind);
            return sql;
        }
        public string Get_MI_WINVCTL_HISDG_SubQuery(string hosp_table_prefix)
        {
            //8 MI_WINVCTL
            string sql = string.Format(@"
                                                                    SELECT STRHOSPITAL, STRSTOREDEP, STRDRUGID, round(LNGDRUGAMT, 24) as LNGDRUGAMT,     LNGSAFERO,   LNGISSUEOKAMT,
                                                                    LNGNORMALRO, STRDEPID
                                                                    FROM   HISDG.{0}XFSTKTOL c
                                                                    WHERE  EXISTS(SELECT 1
                                                                                  FROM   HISDG.{0}XFCHEMIS
                                                                                  WHERE  strdrugid = c.strdrugid
                                                                                         AND strXfKindID = '1') 
                                    ", hosp_table_prefix);
            return sql;
        }
        public string Get_MI_WINVCTL_MMSMS_SubQuery()
        {
            //8 MI_WINVCTL
            string sql = @"
                                        SELECT WH_NO, INID FROM MMSADM.MI_WHMAST WHERE WH_KIND = '0'
                                    ";
            return sql;
        }

        public string Get_BC_BARCODE_Barcode(string hosp_table_prefix)
        {
            //9.1 BC_BARCODE  insert barcode
            string sql = string.Format(@"
                                        SELECT strDrugID  AS MMCODE,
                                               strBarCode AS BARCODE
                                        FROM   HISDG.{0}XFCHEMIS
                                        WHERE  strIsDelete <> '1'
                                               AND ( strBarCode IS NOT NULL
                                                     AND strBarCode <> ' ' ) 
                                    ", hosp_table_prefix);
            return sql;
        }

        public string Get_BC_BARCODE_Mmcode(string hosp_table_prefix)
        {
            //9.2 BC_BARCODE  insert mmcode自己
            string sql = string.Format(@"
                                        SELECT strDrugID AS MMCODE,
                                               strDrugID AS BARCODE
                                        FROM   HISDG.{0}XFCHEMIS
                                        WHERE  strIsDelete <> '1' 
                                    ", hosp_table_prefix);
            return sql;
        }

        public string Get_MI_WLOCINV_HISDG_SubQuery(string wh_kind, string hosp_table_prefix)
        {
            string sql = string.Format(@"
                select a.strDepID,a.strdrugid,a.strStorePos,a.lngDrugAmt
                  from HISDG.{1}XFMSBANK a
                  join HISDG.{1}XFCHEMIS b on a.strdrugid=b.strdrugid
                 where a.strIsDelete<>'1'
                   and b.strXfKindID {0} '1'
            ", wh_kind == "0" ? "=" : "<>", hosp_table_prefix);

            return sql;
        }
        public string Get_MI_WLOCINV_MMSMS_SubQuery(string wh_kind)
        {
            string sql = string.Format(@"
                SELECT WH_NO,INID FROM MMSADM.MI_WHMAST WHERE WH_KIND='{0}'
            ", wh_kind);

            return sql;
        }
        public string Get_MI_WEXPINV_HISDG_SubQuery(string wh_kind, string hosp_table_prefix)
        {
            //string sql = string.Format(@"
            //    select a.strDepID,a.strDrugID, a.strDateLimit,a.strMadeBatchNo, a.lngDrugAmt
            //      from HISDG.{1}XFMSBANK a
            //      join HISDG.{1}XFCHEMIS b on a.strdrugid=b.strdrugid
            //     where a.strIsDelete<>'1'
            //       and trim(a.strDateLimit) is not null
            //       and b.strXfKindID {0} '1'
            //", wh_kind == "0" ? "=" : "<>", hosp_table_prefix);
            string sql = string.Format(@"
                select a.strDepID,a.strDrugID, a.strDateLimit,a.strMadeBatchNo, sum(to_number(a.lngDrugAmt)) AS lngDrugAmt
                  from HISDG.{1}XFMSBANK a
                  join HISDG.{1}XFCHEMIS b on a.strdrugid=b.strdrugid
                 where a.strIsDelete<>'1'
                   and trim(a.strDateLimit) is not null
                   and b.strXfKindID {0} '1'
                   group by a.strDepID,a.strDrugID, a.strDateLimit,a.strMadeBatchNo
                   order by a.strDepID,a.strDrugID, a.strDateLimit,a.strMadeBatchNo
            ", wh_kind == "0" ? "=" : "<>", hosp_table_prefix);

            return sql;
        }
        public string Get_MI_WEXPINV_MMSMS_SubQuery(string wh_kind)
        {
            string sql = string.Format(@"
                SELECT WH_NO,INID FROM MMSADM.MI_WHMAST WHERE WH_KIND='{0}'
            ", wh_kind);

            return sql;
        }
        public string Get_MI_WINVMON_HISDG_SubQuery(string wh_kind, string hosp_table_prefix)
        {
            string sql = string.Format(@"
                select a.strDate2,a.strDepID,a.strDrugID,a.lngNowRest
                  from HISDG.{1}XFINVENT a
                  join HISDG.{1}XFCHEMIS b on a.strdrugid=b.strdrugid
                 where a.strDate2=(select max(strDate2) from XFINVENT)
                   and b.strXfKindID {0} '1'
            ", wh_kind == "0" ? "=" : "<>", hosp_table_prefix);

            return sql;
        }
        public string Get_MI_WINVMON_MMSMS_SubQuery(string wh_kind)
        {
            string sql = string.Format(@"
                SELECT WH_NO,INID FROM MMSADM.MI_WHMAST WHERE WH_KIND='{0}'
            ", wh_kind);

            return sql;
        }
        public string Get_MI_WHCOST(string hosp_table_prefix)
        {
            //4 UR_INID
            //---- select from HISDG：XFINVENT、XFUTCOST 最後一筆月結資料----
            string sql = string.Format(@"
                            SELECT P.data_ym AS DATA_YM,
                                 P.strDrugID AS MMCODE,
                                 P.strDate2 AS SET_YM,
                                 SUM (P.lngLastRest) AS PMN_INVQTY,
                                 TO_NUMBER (Q.LastUnitPrice) AS PMN_AVGPRICE,             --國軍是否要另開欄位?
                                 SUM (P.lngNowRest) AS MN_INQTY,
                                 TO_NUMBER (P.strTouchPrice) AS CONT_PRICE,
                                 TO_NUMBER (P.strUnitPrice) AS UPRICE,
                                 TO_NUMBER (P.strRebPrice) AS DISC_CPRICE,
                                 (CASE P.strPayWay WHEN '1' THEN 'P' WHEN '2' THEN 'C' ELSE 'N' END)
                                    AS SOURCECODE,
                                 TO_NUMBER (P.strHealthPrice) AS NHI_PRICE
                            FROM (SELECT strDate2 AS data_ym,
                                         strDrugID,
                                         strDate2,
                                         lngLastRest,                            --考慮進貨量，所以明細都找出來，才sum
                                         lngNowRest,                             --考慮進貨量，所以明細都找出來，才sum
                                         (CASE
                                             WHEN strStoredep = '1' THEN (lngNowStock - lngLagnappeSum) --一級庫才是進貨量，國軍進貨量要扣除贈品
                                             ELSE 0
                                          END)
                                            AS lngNowStock,
                                         (SELECT strTouchPrice
                                            FROM {0}XFUTCOST
                                           WHERE strUTCostNo = a.strUTCostNo)
                                            AS strTouchPrice,
                                         strUnitPrice,
                                         strRebPrice,
                                         strPayWay,
                                         strHealthPrice
                                    FROM {0}XFINVENT a
                                   WHERE     strIsDelete <> '1'
                                         AND strDate2 = (SELECT MAX (strDate2) FROM {0}XFINVENT)) P
                                 LEFT JOIN
                                 (with temp as (
                                    select strDrugID,strDate2, strlastupdate,strUnitPrice,
                                        row_number() over(partition by strDrugID order by strlastupdate desc) as rn
                                        from {0}XFINVENT
                                    )
                                    select strDrugID,strUnitPrice
                                    from temp
                                    where rn = 1) Q
                                    ON P.strdrugid = Q.strdrugid
                            GROUP BY P.data_ym,
                                     P.strDrugID,
                                     P.strDate2,
                                     P.lngNowStock,
                                     P.strTouchPrice,
                                     P.strUnitPrice,
                                     P.strRebPrice,
                                     P.strPayWay,
                                     P.strHealthPrice,
                                     Q.LastUnitPrice
                        ", hosp_table_prefix);
            return sql;
        }
        #endregion

        #region MMSMS

        #region insert 
        public string Insert_MI_MATCLASS()
        {
            //0.1 MI_MATCLASS
            string sql = @"
                                        BEGIN
                                            INSERT INTO MMSADM.MI_MATCLASS (MAT_CLASS, MAT_CLSNAME, MAT_CLSID) VALUES ('01', '藥品', '1');
                                            INSERT INTO MMSADM.MI_MATCLASS (MAT_CLASS, MAT_CLSNAME, MAT_CLSID) VALUES ('02', '衛材(含檢材)', '2');
                                            INSERT INTO MMSADM.MI_MATCLASS (MAT_CLASS, MAT_CLSNAME, MAT_CLSID) VALUES ('03', '文具', '3');
                                            INSERT INTO MMSADM.MI_MATCLASS (MAT_CLASS, MAT_CLSNAME, MAT_CLSID) VALUES ('04', '清潔用品', '3');
                                            INSERT INTO MMSADM.MI_MATCLASS (MAT_CLASS, MAT_CLSNAME, MAT_CLSID) VALUES ('05', '表格', '3');
                                            INSERT INTO MMSADM.MI_MATCLASS (MAT_CLASS, MAT_CLSNAME, MAT_CLSID) VALUES ('06', '防護用具', '3');
                                            INSERT INTO MMSADM.MI_MATCLASS (MAT_CLASS, MAT_CLSNAME, MAT_CLSID) VALUES ('07', '被服', '3');
                                            INSERT INTO MMSADM.MI_MATCLASS (MAT_CLASS, MAT_CLSNAME, MAT_CLSID) VALUES ('08', '資訊耗材', '3');
                                            INSERT INTO MMSADM.MI_MATCLASS (MAT_CLASS, MAT_CLSNAME, MAT_CLSID) VALUES ('09', '氣體', '6');
                                            INSERT INTO MMSADM.MI_MATCLASS (MAT_CLASS, MAT_CLSNAME, MAT_CLSID) VALUES ('13', '中藥', '7');
                                            INSERT INTO MMSADM.MI_MATCLASS (MAT_CLASS, MAT_CLSNAME, MAT_CLSID) VALUES ('14', '水電', '4');
                                            INSERT INTO MMSADM.MI_MATCLASS (MAT_CLASS, MAT_CLSNAME, MAT_CLSID) VALUES ('15', '空調', '4');
                                            INSERT INTO MMSADM.MI_MATCLASS (MAT_CLASS, MAT_CLSNAME, MAT_CLSID) VALUES ('16', '中控', '4');
                                            INSERT INTO MMSADM.MI_MATCLASS (MAT_CLASS, MAT_CLSNAME, MAT_CLSID) VALUES ('17', '工務', '4');
                                            INSERT INTO MMSADM.MI_MATCLASS (MAT_CLASS, MAT_CLSNAME, MAT_CLSID) VALUES ('18', '污被服', '4');
                                            INSERT INTO MMSADM.MI_MATCLASS (MAT_CLASS, MAT_CLSNAME, MAT_CLSID) VALUES ('19', '鍋爐', '4');
                                            INSERT INTO MMSADM.MI_MATCLASS (MAT_CLASS, MAT_CLSNAME, MAT_CLSID) VALUES ('20', '氣體', '4');
                                            INSERT INTO MMSADM.MI_MATCLASS (MAT_CLASS, MAT_CLSNAME, MAT_CLSID) VALUES ('21', '電梯', '4');
                                            INSERT INTO MMSADM.MI_MATCLASS (MAT_CLASS, MAT_CLSNAME, MAT_CLSID) VALUES ('22', '公共天線', '4');
                                            INSERT INTO MMSADM.MI_MATCLASS (MAT_CLASS, MAT_CLSNAME, MAT_CLSID) VALUES ('23', '廣播', '4');
                                            INSERT INTO MMSADM.MI_MATCLASS (MAT_CLASS, MAT_CLSNAME, MAT_CLSID) VALUES ('30', '對講機', '5');
                                            INSERT INTO MMSADM.MI_MATCLASS (MAT_CLASS, MAT_CLSNAME, MAT_CLSID) VALUES ('31', '子母鐘', '5');
                                            INSERT INTO MMSADM.MI_MATCLASS (MAT_CLASS, MAT_CLSNAME, MAT_CLSID) VALUES ('32', '叫號燈', '5');
                                            INSERT INTO MMSADM.MI_MATCLASS (MAT_CLASS, MAT_CLSNAME, MAT_CLSID) VALUES ('33', '護士呼叫', '5');
                                            INSERT INTO MMSADM.MI_MATCLASS (MAT_CLASS, MAT_CLSNAME, MAT_CLSID) VALUES ('34', '電話交換機', '5');
                                            INSERT INTO MMSADM.MI_MATCLASS (MAT_CLASS, MAT_CLSNAME, MAT_CLSID) VALUES ('35', '呼叫器', '5');
                                            INSERT INTO MMSADM.MI_MATCLASS (MAT_CLASS, MAT_CLSNAME, MAT_CLSID) VALUES ('36', '膠紙', '5');
                                            INSERT INTO MMSADM.MI_MATCLASS (MAT_CLASS, MAT_CLSNAME, MAT_CLSID) VALUES ('37', '膠管', '5');
                                        END;
                                    ";

            return sql;
        }

        public string Insert_MI_MCODE()
        {
            //0.2 MI_MCODE
            string sql = @"
                                        BEGIN
                                            INSERT INTO MMSADM.MI_MCODE (MCODE, MCODE_NAME) VALUES ('ADJI', '調帳入庫');
                                            INSERT INTO MMSADM.MI_MCODE (MCODE, MCODE_NAME) VALUES ('ADJO', '調帳出庫');
                                            INSERT INTO MMSADM.MI_MCODE (MCODE, MCODE_NAME) VALUES ('APLI', '撥發進貨入庫');
                                            INSERT INTO MMSADM.MI_MCODE (MCODE, MCODE_NAME) VALUES ('APLO', '核撥出庫');
                                            INSERT INTO MMSADM.MI_MCODE (MCODE, MCODE_NAME) VALUES ('BAKI', '繳回入庫');
                                            INSERT INTO MMSADM.MI_MCODE (MCODE, MCODE_NAME) VALUES ('BAKO', '繳回出庫');
                                            INSERT INTO MMSADM.MI_MCODE (MCODE, MCODE_NAME) VALUES ('CHIO', '盤點出入庫');
                                            INSERT INTO MMSADM.MI_MCODE (MCODE, MCODE_NAME) VALUES ('DISO', '報廢出庫');
                                            INSERT INTO MMSADM.MI_MCODE (MCODE, MCODE_NAME) VALUES ('EXGI', '換藥入庫');
                                            INSERT INTO MMSADM.MI_MCODE (MCODE, MCODE_NAME) VALUES ('EXGO', '換藥出庫');
                                            INSERT INTO MMSADM.MI_MCODE (MCODE, MCODE_NAME) VALUES ('MILI', '軍品調帳入庫');
                                            INSERT INTO MMSADM.MI_MCODE (MCODE, MCODE_NAME) VALUES ('MILO', '軍品調帳出庫');
                                            INSERT INTO MMSADM.MI_MCODE (MCODE, MCODE_NAME) VALUES ('REJO', '退貨出庫');
                                            INSERT INTO MMSADM.MI_MCODE (MCODE, MCODE_NAME) VALUES ('TRNI', '調撥入庫');
                                            INSERT INTO MMSADM.MI_MCODE (MCODE, MCODE_NAME) VALUES ('TRNO', '調撥出庫');
                                            INSERT INTO MMSADM.MI_MCODE (MCODE, MCODE_NAME) VALUES ('USEI', '消耗繳回入庫');
                                            INSERT INTO MMSADM.MI_MCODE (MCODE, MCODE_NAME) VALUES ('USEO', '消耗出庫');
                                            INSERT INTO MMSADM.MI_MCODE (MCODE, MCODE_NAME) VALUES ('WAYI', '在途入庫');
                                            INSERT INTO MMSADM.MI_MCODE (MCODE, MCODE_NAME) VALUES ('WAYO', '在途出庫');
                                        END;
                                    ";
            return sql;
        }

        public string Insert_PARAM_M()
        {
            //0.3 PARAM_M
            string sql = @"
                                        BEGIN
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC) VALUES ('AB0063', '單位申領明細報表');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC, GRP_USE) VALUES ('ADJX_CODE', '調帳單異動代碼', '調帳作業');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC, GRP_USE) VALUES ('BC_CS_ACC_LOG', '中央庫進貨驗收資料檔', 'STATUS');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC) VALUES ('CHK_DETAIL', '盤點明細檔');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC) VALUES ('CHK_MAST', '盤點主檔');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC) VALUES ('CR_DOC', 'TABLE');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC) VALUES ('DAILY_DOCNO_SEQ', '申請單流水號');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC) VALUES ('FA0038', 'REPORT');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC, GRP_USE) VALUES ('FA0063', '價量調查報表', '醫事服務機構代號');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC) VALUES ('HISAPI_DRUGQUANTITY', '庫存異動同步');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC) VALUES ('HIS_BASORDD', 'TABLE');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC) VALUES ('HIS_BASORDM', 'TABLE');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC) VALUES ('HIS_PHRDCMG', 'TABLE');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC) VALUES ('HIS_STKDMIT', 'TABLE');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC) VALUES ('HOSP_INFO', '醫院基本資料');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC, GRP_USE) VALUES ('LOGIN_CHECK', '登入檢查', '登入檢查特定設定');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC) VALUES ('ME_AB0012', 'TABLE');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC) VALUES ('ME_AB0076', '各庫藥品每日醫令消耗量表');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC) VALUES ('ME_AB0083', 'TABLE');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC, GRP_USE) VALUES ('ME_DOCD', 'TABLE', '2');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC, GRP_USE) VALUES ('ME_DOCE', 'TABLE', '3');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC, GRP_USE) VALUES ('ME_DOCM', '申請單設定', '1');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC, GRP_USE) VALUES ('ME_EXPM', 'TABLE', '4');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC) VALUES ('ME_FA0053', '各衛星庫房醫令消耗量表');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC) VALUES ('MI_CONSUME_DATE', 'HIS每日扣庫檔');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC, GRP_USE) VALUES ('MI_MAST', '藥衛材基本檔', '藥衛材基本檔維護作業');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC, GRP_USE) VALUES ('MI_MAST_MODCOL', '藥衛材基本檔可維護欄位', '藥衛材基本檔維護作業');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC, GRP_USE) VALUES ('MI_MAST_N', '次月衛材基本檔', '次月衛材基本檔異動');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC) VALUES ('MI_MATCLASS', '物料分類主檔');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC, GRP_USE) VALUES ('MI_WHAUTO', 'TABLE', '自動申領設定');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC) VALUES ('MI_WHID', '庫房使用人員檔');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC) VALUES ('MI_WHMAST', 'TABLE');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC) VALUES ('MI_WHTRNS', 'table');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC) VALUES ('MI_WINVCTL', 'TABLE');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC, GRP_USE) VALUES ('MI_WLOCINV', '庫房儲位檔', '庫房儲位檔參數');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC) VALUES ('MM_PACK_M', 'TABLE');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC) VALUES ('MM_PO_M', '訂單主檔');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC, GRP_USE) VALUES ('MM_PR_M', '常態申購開放日期', '常態申購單');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC, GRP_USE) VALUES ('PH_INVOICE', '發票資料檔', 'MEMO');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC, GRP_USE) VALUES ('PH_SMALL_D', 'TABLE', '小額採購明細檔');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC, GRP_USE) VALUES ('PH_SMALL_M', '小額採購清單主檔', '小額採購申請');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC, GRP_USE) VALUES ('PH_VENDER', '廠商基本檔維護', '設定指定維護各自資料責任中心');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC) VALUES ('SCAN_USE_LOG', '衛材條碼扣庫');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC) VALUES ('TC_PURCH_M', '中藥訂單主檔');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC, GRP_USE) VALUES ('UR_INID', '成本中心基本檔', '成本中心基本檔參數');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC, GRP_USE) VALUES ('VIEWALL', '可看所有資料的群組', 'SESSION');
                                            INSERT INTO MMSADM.PARAM_M (GRP_CODE, GRP_DESC, GRP_USE) VALUES ('Y_OR_N', '共用,只有Y跟N', '正式資料,勿刪!!!');
                                        END;
                                    ";
            return sql;
        }

        public string Insert_PARAM_D(string hosp_id, string hospName, List<PARAM_D> list_param_d)
        {
            //0.4 PARAM_D
            string sql_sub = "";
            foreach (PARAM_D item in list_param_d)
            {
                sql_sub += string.Format(@"
                                        INSERT INTO MMSADM.PARAM_D
                                                    (GRP_CODE,
                                                     DATA_SEQ,
                                                     DATA_NAME,
                                                     DATA_VALUE,
                                                     DATA_DESC,
                                                     DATA_REMARK)
                                        SELECT 'MI_MAST',
                                               (SELECT NVL(MAX(DATA_SEQ), 0) + 1
                                                FROM   PARAM_D
                                                WHERE  GRP_CODE = 'MI_MAST') AS DATA_SEQ,
                                               'MAT_CLASS_SUB' AS DATA_NAME,
                                               '{0}' AS DATA_VALUE,
                                               '{1}' AS DATA_DESC,
                                               '物料分類子類別' AS DATA_REMARK
                                        FROM DUAL; 
                                    ", item.DATA_VALUE, item.DATA_DESC);
            };

            string sql = string.Format(@"
                                        BEGIN
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('AB0063', 1, 'WH_NO', '7200A0', '台北門診全部');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ADJX_CODE', 1, 'AJ1', 'AJ1', '-其它調帳');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ADJX_CODE', 2, 'AJ2', 'AJ2', '+其它調帳');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ADJX_CODE', 3, 'AJ3', 'AJ3', '+機關核發');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ADJX_CODE', 4, 'AJ4', 'AJ4', '+捐贈');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('BC_CS_ACC_LOG', 1, 'STATUS', 'A', '暫驗完成');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('BC_CS_ACC_LOG', 2, 'STATUS', 'P', '完成過帳');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('BC_CS_ACC_LOG', 3, 'STATUS', 'C', '驗收確認');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_DETAIL', 1, 'STATUS_INI', '0', '準備');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_DETAIL', 2, 'STATUS_INI', '1', '盤中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_DETAIL', 3, 'STATUS_INI', '2', '調整');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_DETAIL', 4, 'STATUS_INI', '3', '鎖單');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 1, 'CHK_WH_KIND', '0', '藥品庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 2, 'CHK_WH_KIND', '1', '衛材庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 3, 'CHK_WH_KIND', 'E', '能設');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 4, 'CHK_WH_KIND', 'C', '通信');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 5, 'CHK_PERIOD', 'D', '日');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 6, 'CHK_PERIOD', 'M', '月');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 7, 'CHK_PERIOD', 'S', '季');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 8, 'CHK_PERIOD', 'P', '抽');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 9, 'CHK_WH_KIND_1', '0', '非庫備');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 10, 'CHK_WH_KIND_1', '1', '庫備');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 11, 'CHK_WH_KIND_0', '1', '口服');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 12, 'CHK_WH_KIND_0', '2', '非口服');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 13, 'CHK_WH_KIND_0', '3', '1~3管制用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 14, 'CHK_WH_KIND_0', '4', '4級管制用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 15, 'CHK_LEVEL', '1', '初盤');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 16, 'CHK_LEVEL', '2', '複盤');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 17, 'CHK_LEVEL', '3', '三盤');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 18, 'CHK_STATUS', '0', '準備');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 19, 'CHK_STATUS', '1', '盤中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 20, 'CHK_STATUS', '2', '調整');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 21, 'CHK_STATUS', '3', '鎖單');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 22, 'CHK_WH_GRADE', '1', '庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 23, 'CHK_WH_GRADE', '2', '局(衛星庫)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 24, 'CHK_WH_GRADE', '3', '病房');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 25, 'CHK_WH_GRADE', '4', '科室');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 26, 'CHK_WH_GRADE', '5', '戰備庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 27, 'CHK_WH_GRADE', 'M', '醫院軍');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 28, 'CHK_WH_GRADE', 'S', '學院軍');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 29, 'CHK_CLASS', '02', '衛材');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 30, 'CHK_CLASS', '07', '被服');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 31, 'CHK_CLASS', '08', '資訊耗材');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 32, 'CHK_CLASS', '0X', '一般物品');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 33, 'CHK_WH_KIND_0', 'X', '不區分');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 34, 'CHK_WH_KIND_1', '3', '小額採購');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 35, 'CHK_CLASS', '01', '藥品');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 36, 'CHK_STATUS', '4', '重盤');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 37, 'CHK_WH_KIND_0', '5', '公藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 38, 'CHK_WH_KIND_0', '6', '專科');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 39, 'CHK_CLASS', '14', '水電');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 40, 'CHK_CLASS', '15', '空調');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 41, 'CHK_CLASS', '16', '中控');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 42, 'CHK_CLASS', '17', '工務');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 43, 'CHK_CLASS', '18', '污被服');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 44, 'CHK_CLASS', '19', '鍋爐');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 45, 'CHK_CLASS', '20', '氣體');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 46, 'CHK_CLASS', '21', '電梯');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 47, 'CHK_CLASS', '22', '公共天線');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 48, 'CHK_CLASS', '23', '廣播');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 49, 'CHK_CLASS', '30', '對講機');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 50, 'CHK_CLASS', '31', '子母鐘');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 51, 'CHK_CLASS', '32', '叫號燈');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 52, 'CHK_CLASS', '33', '護士呼叫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 53, 'CHK_CLASS', '34', '電話交換機');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 54, 'CHK_CLASS', '35', '呼叫器');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 55, 'CHK_CLASS', '36', '膠紙');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 56, 'CHK_CLASS', '37', '膠管');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 57, 'CHK_WH_KIND_E', '0', '非庫備');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 58, 'CHK_WH_KIND_E', '1', '庫備');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 59, 'CHK_WH_KIND_E', '3', '小額採購');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 61, 'CHK_WH_KIND_C', '0', '非庫備');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 62, 'CHK_WH_KIND_C', '1', '庫備');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 63, 'CHK_WH_KIND_C', '3', '小額採購');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 64, 'CHK_WH_KIND_0', '7', '一般藥品');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 65, 'CHK_WH_KIND_0', '8', '大瓶點滴');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 66, 'CHK_STATUS', 'C', '下一盤');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CHK_MAST', 67, 'CHK_STATUS', 'P', '已過帳');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CR_DOC', 1, 'CR_STATUS', 'A', '待申請');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CR_DOC', 2, 'CR_STATUS', 'B', '已申請待產生通知單');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CR_DOC', 3, 'CR_STATUS', 'C', '已刪除');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CR_DOC', 4, 'CR_STATUS', 'D', '已撤銷');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CR_DOC', 5, 'CR_STATUS', 'E', '已產生通知單待寄信');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CR_DOC', 6, 'CR_STATUS', 'F', '已寄信');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CR_DOC', 7, 'CR_STATUS', 'G', '重寄待寄信');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CR_DOC', 8, 'CR_STATUS', 'H', '重寄已寄信');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CR_DOC', 9, 'CR_STATUS', 'I', '已點收');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CR_DOC', 10, 'CR_STATUS', 'J', '已退回');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CR_DOC', 11, 'CR_STATUS', 'K', '已結驗');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CR_DOC', 12, 'CR_STATUS', 'L', '已產生單據申請');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CR_DOC', 13, 'CR_STATUS', 'M', '已產生PR');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CR_DOC', 14, 'CR_STATUS', 'N', '已產生PO');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CR_DOC', 15, 'CR_STATUS', 'O', '已採購進貨接收');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CR_DOC', 16, 'CR_STATUS', 'P', '作廢');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('CR_DOC', 17, 'CR_STATUS', 'Q', '採購進貨已點收');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE) VALUES ('DAILY_DOCNO_SEQ', 1, 'MAXDATE', '1120314');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('FA0038', 1, 'KIND', '01', '三級庫以上進貨資料');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('FA0063', 1, 'HOSP_ID', '0501110514', '醫事服務機構代號');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HISAPI_DRUGQUANTITY', 1, 'WHINV_HIS_SYNCDATE', '9999999999999', '庫存同步時間');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDD', 1, 'INSUSIGNI', '0', '不給付');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDD', 2, 'INSUSIGNI', '1', '給付');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDD', 3, 'INSUSIGNI', '2', '條件給付');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDD', 4, 'INSUSIGNI', '3', '醫院自行吸收');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDD', 5, 'INSUSIGNI', '4', '預設自費');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDD', 6, 'INSUSIGNO', '0', '不給付');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDD', 7, 'INSUSIGNO', '1', '給付');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDD', 8, 'INSUSIGNO', '2', '條件給付');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDD', 9, 'INSUSIGNO', '3', '醫院自行吸收');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDD', 10, 'INSUSIGNO', '4', '預設自費');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDD', 11, 'INSUEMGFLAG', 'Y', '是');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDD', 12, 'INSUEMGFLAG', 'N', '否');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDD', 13, 'HOSPEMGFLAG', 'Y', '是');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDD', 14, 'HOSPEMGFLAG', 'N', '否');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDD', 15, 'DENTALREFFLAG', 'Y', '是');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDD', 16, 'DENTALREFFLAG', 'N', '否');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDD', 17, 'PPFTYPE', '0', '醫院提成');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDD', 18, 'PPFTYPE', '1', '主治大夫提成');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDD', 19, 'PPFTYPE', '2', '主治或科室提成');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDD', 20, 'PPFTYPE', '3', '科室提成');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDD', 21, 'PPFTYPE', '4', '住院醫師以上提成');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDD', 22, 'PPFTYPE', '5', '住院醫師及科室提成');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDD', 23, 'INSUKIDFLAG', 'Y', '是');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDD', 24, 'INSUKIDFLAG', 'N', '否');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDD', 25, 'HOSPKIDFLAG', 'Y', '是');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDD', 26, 'HOSPKIDFLAG', 'N', '否');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDD', 27, 'CONTRACNO', '1', '軍聯標');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDD', 28, 'CONTRACNO', '2', '自辨合約');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDD', 29, 'CONTRACNO', '1', '藥委會決議為零購之軍聯標品項');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDD', 30, 'CONTRACNO', '2', '藥委會決議為零購之非軍聯標而自辨合約品項');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDD', 31, 'CONTRACNO', '0Y', '藥委會決議為零購之非軍聯標而未自辨合約品項');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDD', 32, 'CONTRACNO', '0N', '未經藥委會決議之上簽零購品項');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDD', 33, 'CASEFROM', '1', '軍聯標');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDD', 34, 'CASEFROM', '2', '院內標');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDD', 35, 'CASEFROM', '0', '零購品');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDD', 36, 'CASEFROM', 'X', '非標案');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDD', 37, 'EXAMINEDISCFLAG', 'N', '健保檢查折扣不打折');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDD', 38, 'EXAMINEDISCFLAG', 'Y', '須打折');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 1, 'UDSERVICEFLAG', 'Y', '走UDOrder');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 2, 'UDSERVICEFLAG', 'N', '以門診方式開立');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 3, 'TAKEKIND', '0', '非用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 4, 'TAKEKIND', '11', '內用藥水劑');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 5, 'TAKEKIND', '12', '內用藥錠劑');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 6, 'TAKEKIND', '13', '內用藥粉劑');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 7, 'TAKEKIND', '21', '注射藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 8, 'TAKEKIND', '31', '點滴');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 9, 'TAKEKIND', '41', '外用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 10, 'TAKEKIND', '51', '中藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 11, 'BUYORDERFLAG', 'Y', '買斷藥品');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 12, 'BUYORDERFLAG', 'N', '非買斷藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 15, 'INSUOFFERFLAG', 'Y', '給付');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 16, 'INSUOFFERFLAG', 'N', '不給付');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 17, 'APPENDMATERIALFLAG', 'Y', '是');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 18, 'APPENDMATERIALFLAG', 'N', '否');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 19, 'EXORDERFLAG', 'Y', '是');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 20, 'EXORDERFLAG', 'N', '否');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 21, 'ORDERDCFLAG', 'N', '繼續使用');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 22, 'ORDERDCFLAG', 'Y', '停止使用');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 23, 'AGGREGATECODE', '0', '非累計用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 24, 'AGGREGATECODE', '1', '累計核發申報');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 25, 'AGGREGATECODE', '2', '累計核實申報');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 26, 'LIMITFLAG', 'Y', '有限制');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 27, 'LIMITFLAG', 'N', '無限制');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 28, 'RESTRICTCODE', 'N', '非管制用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 29, 'RESTRICTCODE', '0', '其它列管藥品');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 30, 'RESTRICTCODE', '1', '第一級管制用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 31, 'RESTRICTCODE', '2', '第二級管制用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 32, 'RESTRICTCODE', '3', '第三級管制用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 33, 'RESTRICTCODE', '4', '第四級管制用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 34, 'ANTIBIOTICSCODE', 'N', '非抗生素');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 35, 'ANTIBIOTICSCODE', '1', '一線');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 36, 'ANTIBIOTICSCODE', '2', '二線');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 37, 'ANTIBIOTICSCODE', '3', '三線');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 38, 'ANTIBIOTICSCODE', '4', '四線');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 39, 'CARRYKINDI', '1', '以UD結轉歸整(所有口服錠劑、膠囊或可保留24小時以上的針劑)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 40, 'CARRYKINDI', '2', '以天歸整(只能保留24小時的針劑等)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 41, 'CARRYKINDI', '3', '以次歸整(開瓶即消耗)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 42, 'CARRYKINDI', '4', '後歸整(水藥等特殊POS的藥品)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 43, 'CARRYKINDI', '5', '不歸整(不需進位，例如買斷藥)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 44, 'CARRYKINDO', '1', '以UD結轉歸整(所有口服錠劑、膠囊或可保留24小時以上的針劑)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 45, 'CARRYKINDO', '2', '以天歸整(只能保留24小時的針劑等)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 46, 'CARRYKINDO', '3', '以次歸整(開瓶即消耗)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 47, 'CARRYKINDO', '4', '後歸整(水藥等特殊POS的藥品)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 48, 'CARRYKINDO', '5', '不歸整(不需進位，例如買斷藥)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 49, 'UDPOWDERFLAG', 'Y', '是');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 50, 'UDPOWDERFLAG', 'N', '否');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 51, 'RETURNDRUGFLAG', 'Y', '是');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 52, 'RETURNDRUGFLAG', 'N', '否');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 53, 'RESEARCHDRUGFLAG', 'Y', '是');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 54, 'RESEARCHDRUGFLAG', 'N', '否');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 55, 'MACHINEFLAG', 'Y', '是');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 56, 'MACHINEFLAG', 'N', '否');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 57, 'TRANSCOMPUTEFLAG', 'Y', '結轉計價扣庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 58, 'TRANSCOMPUTEFLAG', 'N', 'SINGIN計價扣庫或調劑計價扣庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 59, 'FIXPATHNOFLAG', 'Y', '是');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 60, 'FIXPATHNOFLAG', 'N', '否');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 61, 'ONLYROUNDFLAG', 'Y', '是');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 62, 'ONLYROUNDFLAG', 'N', '否');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 63, 'UNABLEPOWDERFLAG', 'Y', '是');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 64, 'UNABLEPOWDERFLAG', 'N', '否');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 65, 'DANGERDRUGFLAG', 'Y', '是');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 66, 'DANGERDRUGFLAG', 'N', '否');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 67, 'COLDSTORAGEFLAG', 'Y', '是');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 68, 'COLDSTORAGEFLAG', 'N', '否');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 69, 'LIGHTAVOIDFLAG', 'Y', '是');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 70, 'LIGHTAVOIDFLAG', 'N', '否');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 71, 'CHANGESTATUS', 'A', '新增');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 72, 'CHANGESTATUS', 'D', '刪除');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 73, 'CHANGESTATUS', 'M', '修改');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 74, 'ORDERKIND', '1', '用藥明細');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 75, 'ORDERKIND', '2', '診療明細');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 76, 'ORDERKIND', '3', '特殊材料');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 77, 'ORDERKIND', '4', '不得另計價之藥品或診療項目');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 78, 'ORDERKIND', '5', 'EPO注射');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 79, 'ORDERKIND', '6', 'HCT檢驗');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 80, 'ORDERKIND', '7', '委託代（轉）檢');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 81, 'ORDERKIND', '8', '器官捐贈');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 82, 'HIGHPRICEFLAG', 'Y', '是');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 83, 'HIGHPRICEFLAG', 'N', '否');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 84, 'CURETYPE', '0', '非特殊治療藥品');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 85, 'CURETYPE', '1', '化療靜脈');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 86, 'CURETYPE', '2', '化療口服');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 87, 'CURETYPE', '3', '放射治療');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 88, 'CURETYPE', '4', '賀爾蒙治療');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 89, 'CURETYPE', '5', '其它治療');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 90, 'CURETYPE', '6', '化療其它治療');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 91, 'CURETYPE', '7', '免疫治療');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 92, 'INPDISPLAYFLAG', 'Y', '是');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 93, 'INPDISPLAYFLAG', 'N', '否');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 94, 'SOONCULLFLAG', 'Y', '是');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 95, 'SOONCULLFLAG', 'N', '否');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 96, 'RELATEFLAGO', 'Y', '是');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 97, 'RELATEFLAGO', 'N', '否');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 98, 'RELATEFLAGI', 'Y', '是');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 99, 'RELATEFLAGI', 'N', '否');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 100, 'WEIGHTTYPE', '0', '不必限制');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 101, 'WEIGHTTYPE', '1', '體重計算');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 102, 'WEIGHTTYPE', '2', '體表面積計算');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 103, 'RESTRICTTYPE', '0', '不做任何提示及鎖控');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 104, 'RESTRICTTYPE', '1', '重複提示不鎖控開立');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 105, 'RESTRICTTYPE', '2', '重複提示鎖控開立');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 106, 'CHECKINSWITCH', 'Y', '需報到');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 107, 'CHECKINSWITCH', 'N', '不需報到');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 108, 'CHECKINSWITCH', 'R', '發報告視同已報到');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 109, 'REPORTFLAG', 'Y', '是');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 110, 'REPORTFLAG', 'N', '否');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE) VALUES ('HIS_BASORDM', 111, 'SINGLEITEMFLAG', 'SingleItem該筆醫令獨立一張申請單');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 112, 'SINGLEITEMFLAG', 'Y', 'Single');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 113, 'SINGLEITEMFLAG', 'N', 'Multiple');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 114, 'XRAYORDERCODE', 'Y', '預設同療程項目');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 115, 'XRAYORDERCODE', 'N', '非同療程項');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 116, 'XRAYORDERCODE', 'C', '加收部分負擔');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 117, 'SENDUNITFLAG', 'Y', '是');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 118, 'SENDUNITFLAG', 'N', '否');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 119, 'SIGNFLAG', 'Y', '是');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 120, 'SIGNFLAG', 'N', '否');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 121, 'CHANGEABLEFLAG', 'Y', '以線上輸入之價格為主');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 122, 'CHANGEABLEFLAG', 'N', '以基本檔價格重算');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 123, 'TDMFLAG', 'Y', '是');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 124, 'TDMFLAG', 'N', '否');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 125, 'SPECIALORDERKIND', 'N', '非專案用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 126, 'SPECIALORDERKIND', 'Y', '專案用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 127, 'SPECIALORDERKIND', 'P', '部分外審');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 128, 'NEEDREGIONFLAG', 'Y', '是');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 129, 'NEEDREGIONFLAG', 'N', '否');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 130, 'ORDERUSETYPE', '0', '線上使用');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 131, 'ORDERUSETYPE', '1', '不可點買之藥品');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 132, 'FIXDOSEFLAG', 'Y', '是');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 133, 'FIXDOSEFLAG', 'N', '否');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 134, 'RAREDISORDERFLAG', 'Y', '是');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 135, 'RAREDISORDERFLAG', 'N', '否');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 136, 'HOSPEXAMINEFLAG', 'Y', '是');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 137, 'HOSPEXAMINEFLAG', 'N', '否');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 138, 'HOSPEXAMINEQTYFLAG', 'Y', '是');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 139, 'HOSPEXAMINEQTYFLAG', 'N', '否');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 140, 'HEPATITISCODE', '0', '非BC肝用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 141, 'HEPATITISCODE', '1', '限計劃性BC肝用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 142, 'HEPATITISCODE', '2', '有例外診斷者(可進計劃或不進計劃)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 143, 'PUBLICDRUGFLAG', '0', '非公藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 144, 'PUBLICDRUGFLAG', '1', '庫存點為病房，上級庫為住院藥局');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 145, 'PUBLICDRUGFLAG', '2', '庫存點為病房，上級庫為藥庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_BASORDM', 146, 'PUBLICDRUGFLAG', '3', '庫存點為病房，設為備用藥，上級庫為住院藥局');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_PHRDCMG', 1, 'CHANGETYPE', 'A', '新增');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_PHRDCMG', 2, 'CHANGETYPE', 'C', '變更');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_PHRDCMG', 3, 'CHANGETYPE', 'D', '停用通知');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_PHRDCMG', 4, 'INSUSIGNI', '0', '不給付, (一定自費)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_PHRDCMG', 5, 'INSUSIGNI', '1', '給付');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_PHRDCMG', 6, 'INSUSIGNI', '2', '條件給付');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_PHRDCMG', 7, 'INSUSIGNI', '3', '醫院自行吸收, (健保價、自費價為0)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_PHRDCMG', 8, 'INSUSIGNI', '4', '預設自費');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_PHRDCMG', 9, 'INSUSIGNO', '0', '不給付, (一定自費)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_PHRDCMG', 10, 'INSUSIGNO', '1', '給付');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_PHRDCMG', 11, 'INSUSIGNO', '2', '條件給付');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_PHRDCMG', 12, 'INSUSIGNO', '3', '醫院自行吸收, (健保價、自費價為0)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_PHRDCMG', 13, 'INSUSIGNO', '4', '預設自費');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 1, 'DCMASSAGECODE', '0', '正常使用');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 2, 'DCMASSAGECODE', '1', '刪除');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 3, 'DCMASSAGECODE', '2', '停產');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 4, 'DCMASSAGECODE', '3', '廠缺');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 5, 'DCMASSAGECODE', '4', '變更');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 6, 'DMITDCCODE', '0', '正常進貨');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 7, 'DMITDCCODE', '1', '刪除(停止進貨)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 8, 'DMITDCCODE', '2', '刪除(尚未停止進貨)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 9, 'DMITDCCODE', '3', '停產(停止進貨)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 10, 'DMITDCCODE', '4', '停產(尚未停止進貨)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 11, 'DMITDCCODE', '5', '廠缺(停止供貨)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 12, 'DMITDCCODE', '6', '廠缺(尚未停止進貨)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 13, 'PUBLICDRUGCODE', '0', '非公藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 14, 'PUBLICDRUGCODE', '1', '庫存點為病房，上級庫為住院藥局');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 15, 'PUBLICDRUGCODE', '2', '庫存點為病房，上級庫為藥庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 16, 'PUBLICDRUGCODE', '3', '庫存點為病房，設為備用藥，上級庫為住院藥局');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 17, 'STOCKUSECODE', '1', '水藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 18, 'STOCKUSECODE', '2', '口服藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 19, 'STOCKUSECODE', '3', '可保留24小時的針劑');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 20, 'STOCKUSECODE', '4', '開瓶即消耗');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 21, 'STOCKUSECODE', '0', '非用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 22, 'GROUPARMYNO', '1', '藥品軍聯標');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 23, 'GROUPARMYNO', '2', '衛材');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 24, 'MULTIPRESCRIPTIONCODE', '1', '單方');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 25, 'MULTIPRESCRIPTIONCODE', '2', '複方');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 26, 'DRUGCLASSIFY', '0', '一般用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 27, 'DRUGCLASSIFY', '1', '原料藥及消耗品');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 28, 'DRUGCLASSIFY', '2', '第1~3級管制用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 29, 'DRUGCLASSIFY', '9', '分裝品項');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 30, 'DRUGCLASSIFY', 'X', '其它(衛材)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 31, 'COMMITTEECODE', '1', '是');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 32, 'COMMITTEECODE', '2', '否-衛材');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 33, 'COMMITTEECODE', '3', '否-診斷用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 34, 'COMMITTEECODE', '4', '否-洗腎液(灌洗溶液)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 35, 'COMMITTEECODE', '5', '否-原料藥或消耗品');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 36, 'COMMITTEECODE', '6', '否-006麻醉科用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 37, 'COMMITTEECODE', '7', '否-007放射科用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 38, 'COMMITTEECODE', '8', '否-其他');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 39, 'PURCHASECASETYPE', '1', '甲案');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 40, 'PURCHASECASETYPE', '2', '乙案');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 41, 'SUCKLESECURITY', '1', '安全');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 42, 'SUCKLESECURITY', '2', '不安全');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 43, 'SUCKLESECURITY', '0', '末知');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 44, 'PREGNANTGRADE', 'A', 'A');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 45, 'PREGNANTGRADE', 'B', 'B');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 46, 'PREGNANTGRADE', 'C', 'C');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 47, 'PREGNANTGRADE', 'D', 'D');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 48, 'PREGNANTGRADE', 'X', 'X');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 49, 'STOCKSOURCECODE', 'P', '買斷');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 50, 'STOCKSOURCECODE', 'C', '寄售');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 51, 'STOCKSOURCECODE', 'R', '核醫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 52, 'STOCKSOURCECODE', 'N', '其它');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 53, 'DRUGFLAG', 'Y', '藥品');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 54, 'DRUGFLAG', 'N', '衛材');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 55, 'DRUGAPPLYTYPE', '0', '一般藥品');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 56, 'DRUGAPPLYTYPE', '1', '大瓶點滴');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 57, 'PARENTCODE', '0', '非母子藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 58, 'PARENTCODE', '1', '母藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HIS_STKDMIT', 59, 'PARENTCODE', '2', '子藥');

                                            --INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HOSP_INFO', 1, 'HospName', '三軍總醫院', '醫院名稱');
                                            --INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HOSP_INFO', 2, 'HospCode', '0', '醫院代碼');
                                            --INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HOSP_INFO', 3, 'HospFullName', '三軍總醫院附設民眾診療服務處', '醫院服務單位完整名稱');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HOSP_INFO', 1, 'HospName', '{0}', '醫院名稱');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HOSP_INFO', 2, 'HospCode', '{1}', '醫院代碼');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('HOSP_INFO', 3, 'HospFullName', '{1}', '醫院服務單位完整名稱');

                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('LOGIN_CHECK', 1, 'CheckWhiteListIP', 'N', '是否需檢查UR_ID白名單IP');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_AB0012', 1, 'CARRYKINDI', '1', '以UD結轉歸整');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_AB0012', 2, 'CARRYKINDI', '2', '以天歸整');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_AB0012', 3, 'CARRYKINDI', '3', '以次歸整');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_AB0012', 4, 'CARRYKINDI', '4', '後歸整');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_AB0012', 5, 'CARRYKINDI', '5', '不歸整');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_AB0076', 1, 'KIND', '1', '門診');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_AB0076', 2, 'KIND', '2', '急診');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_AB0076', 3, 'KIND', '3', '住院');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_AB0083', 1, 'KIND', '01', '退藥異常統計表');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_AB0083', 2, 'KIND', '02', '退藥短少金額統計表');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_AB0083', 3, 'KIND', '03', '退藥異常次數及金額統計表');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_AB0083', 4, 'KIND', '04', '退藥異常總表');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCD', 1, 'STAT', '1', '換入');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCD', 2, 'STAT', '2', '換出');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCD', 3, 'GTAPL_REASON', '1', '醫療作業需求增加(如:病患人數增加)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCD', 4, 'GTAPL_REASON', '2', '配合演習、活動等專案任務申請');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCD', 5, 'GTAPL_REASON', '3', '初次申領，尚無基準量(基準量為0)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCD', 6, 'GTAPL_REASON', '4', '為符合衛材最小包裝數量');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCD', 7, 'GTAPL_REASON', '5', '其他');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCD', 8, 'CONFIRMSWITCH', 'B', '不補發');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCD', 9, 'CONFIRMSWITCH', 'C', '補發不扣庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCD', 10, 'CONFIRMSWITCH', 'D', '作廢');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCD', 11, 'CONFIRMSWITCH', 'N', '未確認');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCD', 12, 'CONFIRMSWITCH', 'Y', '補發扣帳');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCD', 13, 'TRANSKIND', '310', '-其它調帳');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCD', 14, 'TRANSKIND', '311', '+其它調帳');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCD', 15, 'TRANSKIND', '350', '-處置藥造成的誤差調帳');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCD', 16, 'TRANSKIND', '351', '+處置藥造成的誤差調帳');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCD', 17, 'TRANSKIND', '360', '-特殊個案上簽奉核調帳');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCD', 18, 'TRANSKIND', '361', '+特殊個案上簽奉核調帳');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCD', 19, 'TRANSKIND', '370', '-久未異動過效期品項');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCD', 20, 'TRANSKIND', '381', '+機關核發');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCD', 21, 'TRANSKIND', '391', '+捐贈藥品');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCD', 22, 'POSTID', '3', '待核撥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCD', 23, 'POSTID', 'C', '已核撥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCD', 24, 'POSTID', '4', '待點收');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCD', 25, 'POSTID', 'D', '已點收');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCD', 26, 'POSTID1', 'N', '未核撥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCD', 27, 'POSTID1', 'Y', '已核撥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCD', 28, 'POSTID', '2', '未揀料');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_DESC) VALUES ('ME_DOCD', 29, 'POSTID', '待核撥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCD', 30, 'STAT', 'A', '已接受');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCD', 31, 'STAT', 'B', '已退回');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCD', 32, 'STAT', 'C', '處理中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCD', 33, 'POSTID1', 'D', '已點收');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCD', 34, 'TRNAB_RESON', '02', '短少');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCD', 35, 'TRNAB_RESON', '01', '破損');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCE', 1, 'PROC_ID', '1', '換藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCE', 2, 'PROC_ID', '2', '合約價退款');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCE', 3, 'PROC_ID', '3', '優惠價退款');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 1, 'APPLY_KIND', '1', '常態申請');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 2, 'APPLY_KIND', '2', '臨時申請');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 3, 'FLOWID_MR1', '1', '申請中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 4, 'FLOWID_MR1', '2', '核撥中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 5, 'FLOWID_MR1', '3', '揀料中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 6, 'FLOWID_MR1', '4', '點收中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 7, 'FLOWID_MR1', '5', '已點收未確認');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 8, 'FLOWID_MR1', '6', '核撥確認');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC, DATA_REMARK) VALUES ('ME_DOCM', 9, 'MR4_BDAY', '1', '衛材非庫備常態起始日', '20');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC, DATA_REMARK) VALUES ('ME_DOCM', 10, 'MR4_EDAY', '27', '衛材非庫備常態結束日', '27');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC, DATA_REMARK) VALUES ('ME_DOCM', 11, 'MR3_BDAY', '01', '一般物品非庫備常態起始日', '10');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC, DATA_REMARK) VALUES ('ME_DOCM', 12, 'MR3_EDAY', '27', '一般物品非庫備常態結束日', '15');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 13, 'FLOWID_RN1', '1', '申請中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 14, 'FLOWID_RN1', '2', '繳回中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 15, 'FLOWID_RN1', '3', '已繳回');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 16, 'DOCTYPE', 'MR1', '一般物品庫備');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 17, 'DOCTYPE', 'MR2', '衛材庫備');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 18, 'DOCTYPE', 'MR3', '一般物品非庫備');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 19, 'DOCTYPE', 'MR4', '衛材非庫備');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 20, 'FLOWID_SP', '0501', '過效期報廢未過帳');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 21, 'FLOWID_SP', '0599', '過效期報廢已結案');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 22, 'FLOWID_SP1', '1', '報廢申請中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 23, 'FLOWID_SP1', '2', '報廢中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 24, 'FLOWID_SP1', '3', '已報廢');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 25, 'FLOWID_XR1', '1', '調帳中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 26, 'FLOWID_XR1', '2', '已調帳');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 27, 'FLOWID_RJ1', '1', '退貨申請中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 28, 'FLOWID_RJ1', '2', '退貨中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 29, 'FLOWID_RJ1', '3', '已退貨扣帳');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 30, 'FLOWID_RJ1', 'X', '已撤銷');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 31, 'FLOWID_XE', '1400', '未處理');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 32, 'FLOWID_XE', '1401', '換藥申請中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 33, 'FLOWID_XE', '1402', '合約價退款申請中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 34, 'FLOWID_XE', '1403', '優惠價退款申請中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 35, 'FLOWID_XE', '1497', '合約價退款結案');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 36, 'FLOWID_XE', '1498', '優惠價退款結案');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 37, 'FLOWID_XE', '1499', '換藥結案');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 38, 'FLOWID_EF2', '1', '申購中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 39, 'FLOWID_EF2', '2', '已入庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 40, 'FLOWID_CM2', '1', '申購中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 41, 'FLOWID_CM2', '2', '已入庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 42, 'FLOWID_EX1', '1', '申請中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 43, 'FLOWID_EX1', '2', '換貨中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 44, 'FLOWID_EX1', '3', '換貨完成');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 45, 'FLOWID_EX1', 'X', '已撤銷');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 46, 'FLOWID_AJ', '1', '申請中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 47, 'FLOWID_AJ', '2', '調帳中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 48, 'FLOWID_AJ', '3', '已調帳');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 49, 'FLOWID_AJ', 'X', '已撤銷');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 50, 'TASK_ID', '1', '一般物品');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 51, 'TASK_ID', '2', '衛材');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 52, 'FLOWID_EF1', '1', '申請中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 53, 'FLOWID_EF1', '2', '已核撥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 54, 'FLOWID_CM1', '1', '申請中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 55, 'FLOWID_CM1', '2', '已核撥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 56, 'STKTRANSKIND', '1', '等量換貨');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 57, 'STKTRANSKIND', '2', '等值更換品項');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 58, 'STKTRANSKIND', '3', '補差價');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 59, 'STKTRANSKIND', '4', '-其他調帳');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 60, 'STKTRANSKIND', '5', '+其他調帳');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 61, 'STKTRANSKIND', '6', '-過效期報廢');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 62, 'STKTRANSKIND', '7', '+機關核發');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 63, 'FLOWID_EX', '0801', '申請中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 64, 'FLOWID_EX', '0899', '換貨完成');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 65, 'FLOWID_EX', 'X', '已撤銷');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 66, 'FLOWID_RJ', '0901', '退貨申請中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 67, 'FLOWID_RJ', '0999', '已退貨');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 68, 'FLOWID_RJ', 'X', '已撤銷');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 69, 'FLOWID_AIR', '1', '氣體申請中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 70, 'FLOWID_AIR', '2', '氣體核撥中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 71, 'FLOWID_AIR', '3', '氣體已核撥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 72, 'FLOWID', '0100', '自動申請');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 73, 'FLOWID', '0101', '手動申請');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 74, 'FLOWID', '0102', '核撥中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 75, 'FLOWID', '0103', '點收中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 76, 'FLOWID', '0104', '藥庫結案中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 77, 'FLOWID', '0199', '領用結案');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 78, 'FLOWID', '0201', '申請中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 79, 'FLOWID', '0202', '調出中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 80, 'FLOWID', '0203', '調入中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 81, 'FLOWID', '0299', '已結案');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 82, 'FLOWID', '0301', '未過帳');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 83, 'FLOWID', '0399', '已結案');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 84, 'FLOWID', '0401', '申請中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 85, 'FLOWID', '0402', '點收中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 86, 'FLOWID', '0499', '已結案');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 87, 'FLOWID', '0501', '未過帳');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 88, 'FLOWID', '0599', '已結案');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 89, 'FLOWID', '0600', '自動申請');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 90, 'FLOWID', '0601', '手動申請');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 91, 'FLOWID', '0602', '核撥中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 92, 'FLOWID', '0699', '已結案');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 93, 'FLOWID', '0701', '藥品進貨');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 94, 'FLOWID', '0799', '完成藥品進貨');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 95, 'FLOWID', '0801', '藥品換貨');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 96, 'FLOWID', '0899', '完成藥品換貨');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 97, 'FLOWID', '0901', '藥品退貨');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 98, 'FLOWID', '0999', '完成藥品退貨');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 99, 'FLOWID', '1001', '藥品換貨(金額)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 100, 'FLOWID', '1099', '完成藥品換貨(金額)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 101, 'FLOWID', '1101', '藥品廠商退貨');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 102, 'FLOWID', '1199', '完成藥品廠商退貨');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 103, 'FLOWID', '1201', '申請中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 104, 'FLOWID', '1202', '點收中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 105, 'FLOWID', '1299', '已結案');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 106, 'FLOWID', '1301', '申請中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 107, 'FLOWID', '1302', '補發中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 108, 'FLOWID', '1399', '已結案');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 109, 'FLOWID', '1401', '換藥申請中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 110, 'FLOWID', '1402', '合約價退款申請中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 111, 'FLOWID', '1403', '優惠價退款申請中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 112, 'FLOWID', '1497', '合約價退款結案');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 113, 'FLOWID', '1498', '優惠價退款結案');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 114, 'FLOWID', '1499', '換藥結案');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 115, 'FLOWID', '1501', '藥品破損申請更換');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 116, 'FLOWID', '1599', '完成藥品破損更換');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 117, 'FLOWID', '1601', '未回報');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 118, 'FLOWID', '1699', '已回報');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 119, 'DOCTYPE', 'MR', '藥局申請');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 120, 'DOCTYPE', 'TR', '調撥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 121, 'DOCTYPE', 'XR', '軍品調帳');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 122, 'DOCTYPE', 'RN', '繳庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 123, 'DOCTYPE', 'SP', '過效期報廢');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 124, 'DOCTYPE', 'MS', '公藥申請');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 125, 'DOCTYPE', 'IN', '藥品進貨');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 126, 'DOCTYPE', 'EX', '藥品換貨');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 127, 'DOCTYPE', 'RJ', '藥品退貨');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 128, 'DOCTYPE', 'EM', '藥品換貨(金額)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 129, 'DOCTYPE', 'EV', '藥品廠商退貨');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 130, 'DOCTYPE', 'RS', '退藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 131, 'DOCTYPE', 'RR', '補發');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 132, 'DOCTYPE', 'XE', '近效期更換');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 133, 'DOCTYPE', 'SQ', '藥品破損');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 134, 'DOCTYPE', 'RE', '效期回報');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 135, 'STKTRANSKIND', '1', '一般藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 136, 'STKTRANSKIND', '2', '1至3級管制藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 137, 'APPLY_KIND2', '1', '常態申請');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 138, 'APPLY_KIND2', '2', '臨時申請');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 139, 'APPLY_KIND2', '3', '小額採購');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 140, 'STKTRANSKIND2', '1', '等量換貨');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 141, 'STKTRANSKIND2', '2', '等值更換品項');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 142, 'STKTRANSKIND2', '3', '補差價');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 143, 'STKTRANSKIND2', '4', '-其他調帳');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 144, 'STKTRANSKIND2', '5', '+其他調帳');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 145, 'STKTRANSKIND2', '6', '-過效期報廢');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 146, 'STKTRANSKIND2', '7', '+機關核發');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 147, 'DOCTYPE3', 'MR1', '一般物品庫備');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 148, 'DOCTYPE3', 'MR2', '衛材庫備');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 149, 'DOCTYPE3', 'MR3', '一般物品非庫備');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 150, 'DOCTYPE3', 'MR4', '衛材非庫備');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 151, 'STKTRANSKIND2', '1', '等量換貨');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 152, 'STKTRANSKIND2', '2', '等值更換品項');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 153, 'STKTRANSKIND2', '3', '補差價');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 154, 'STKTRANSKIND2', '4', '-其他調帳');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 155, 'STKTRANSKIND2', '5', '+其他調帳');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 156, 'STKTRANSKIND2', '6', '-過效期報廢');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 157, 'STKTRANSKIND2', '7', '+機關核發');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 158, 'APPID', 'TPN', 'admin');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 159, 'APPID', 'CHEMO', 'admin');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 160, 'APPID', 'CHEMOT', 'admin');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 161, 'APPID', 'PH1A', 'admin');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 162, 'APPID', 'PH1C', 'admin');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 163, 'APPID', 'PH1R', 'admin');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 164, 'APPID', 'PHMC', 'admin');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_DOCM', 165, 'FLOWID_MR1', '51', '已點收確認');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_EXPM', 1, 'CLOSEFLAG', 'Y', '已結案');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_EXPM', 2, 'CLOSEFLAG', 'N', '未結案');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_FA0053', 1, 'KIND', '1', '門診');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_FA0053', 2, 'KIND', '2', '急診');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('ME_FA0053', 3, 'KIND', '3', '住院');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_CONSUME_DATE', 1, 'VISIT_KIND', '1', '門');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_CONSUME_DATE', 2, 'VISIT_KIND', '2', '急');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_CONSUME_DATE', 3, 'VISIT_KIND', '3', '住');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_CONSUME_DATE', 4, 'VISIT_KIND', '0', '不分');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_CONSUME_DATE', 5, 'PROC_ID', 'Y', '成功');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_CONSUME_DATE', 6, 'PROC_ID', 'N', '失敗');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_CONSUME_DATE', 7, 'PROC_TYPE', 'D', '整日資料');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_CONSUME_DATE', 8, 'PROC_TYPE', 'T', '非整日資料');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 0, 'E_SUPSTATUS', '0', '正常使用');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 1, 'E_SUPSTATUS', '1', '刪除');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 2, 'E_SUPSTATUS', '2', '停產');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 3, 'E_SUPSTATUS', '3', '廠缺');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 4, 'E_SUPSTATUS', '4', '變更');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 5, 'WEXP_ID', 'Y', '批號效期管理');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 6, 'WEXP_ID', 'N', '不需批號效期管理');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 7, 'WLOC_ID', 'Y', '進貨及領用消耗需儲位編號');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 8, 'WLOC_ID', 'N', '不需要');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 11, 'E_IFPUBLIC', '0', '非公藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 12, 'E_IFPUBLIC', '1', '存點為病房，上級庫為住院藥局');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 13, 'E_IFPUBLIC', '2', '存點為病房，上級庫為藥庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 14, 'E_IFPUBLIC', '3', '存點為病房，設為備用藥，上級庫為住院藥局');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 15, 'E_GPARMYNO', '1', '藥品軍聯標');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 16, 'E_GPARMYNO', '2', '衛材');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 17, 'E_PRESCRIPTYPE', '1', '單方');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 18, 'E_PRESCRIPTYPE', '2', '複方');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 19, 'E_DRUGCLASSIFY', '0', '一般用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 20, 'E_DRUGCLASSIFY', '1', '原料藥及消耗品');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 21, 'E_DRUGCLASSIFY', '2', '第1至3級管制用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 22, 'E_DRUGCLASSIFY', '9', '分裝品項');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 23, 'E_DRUGCLASSIFY', 'X', '其它(衛材)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 24, 'E_COMITCODE', '1', '是');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 25, 'E_COMITCODE', '2', '否-, 衛材');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 26, 'E_COMITCODE', '3', '否-, 診斷用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 27, 'E_COMITCODE', '4', '否-, 洗腎液(灌洗溶液)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 28, 'E_COMITCODE', '5', '原料藥或消耗品');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 29, 'E_COMITCODE', '6', '006麻醉科用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 30, 'E_COMITCODE', '7', '007放射科用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 31, 'E_COMITCODE', '8', '其他');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 32, 'E_INVFLAG', 'Y', '是');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 33, 'E_INVFLAG', 'N', '否');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 34, 'E_PURTYPE', '1', '甲案');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 35, 'E_PURTYPE', '2', '乙案');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 36, 'E_SOURCECODE', 'P', '買斷');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 37, 'E_SOURCECODE', 'C', '寄售');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 38, 'E_SOURCECODE', 'R', '核醫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 39, 'E_SOURCECODE', 'N', '其它');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 40, 'E_DRUGAPLTYPE', '0', '一般藥品');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 41, 'E_DRUGAPLTYPE', '1', '大瓶點滴');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 42, 'E_DRUGAPLTYPE', '2', '酒精');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 43, 'E_PARCODE', '0', '非母子藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 44, 'E_PARCODE', '1', '母藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 45, 'E_PARCODE', '2', '子藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 46, 'E_DRUGCLASS', '1', '指示用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 47, 'E_DRUGCLASS', '2', '成藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 48, 'E_DRUGCLASS', '3', '處方用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 49, 'E_STOCKTYPE', '0', '非用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 50, 'E_STOCKTYPE', '1', '水藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 51, 'E_STOCKTYPE', '2', '口服藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 52, 'E_STOCKTYPE', '3', '可保留24小時的針劑');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 53, 'E_STOCKTYPE', '4', '開瓶即消耗');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 54, 'M_STOREID', '0', '非庫備');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 55, 'M_STOREID', '1', '庫備');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 56, 'M_CONTID', '0', '合約品項');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 57, 'M_CONTID', '2', '非合約');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 58, 'M_CONTID', '3', '不能申請(零購)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 59, 'M_CONSUMID', '1', '消耗');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 60, 'M_CONSUMID', '2', '半消耗');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 61, 'M_PAYKIND', '0', '不給付(自費)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 62, 'M_PAYKIND', '1', '給付(健保)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 63, 'M_PAYKIND', '2', '條件給付');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 64, 'M_PAYID', '1', '計價');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 65, 'M_PAYID', '2', '不計價');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 66, 'M_TRNID', '1', '盤盈虧(正推)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 67, 'M_TRNID', '2', '消耗(逆推)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 68, 'M_APPLYID', 'E', '不可申請申購');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 69, 'M_APPLYID', 'P', '可申請不可申購');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 70, 'M_APPLYID', 'Y', '可申請申購');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 71, 'E_RESTRICTCODE', 'N', '非管制用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 72, 'E_RESTRICTCODE', '0', '其他列管藥品');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 73, 'E_RESTRICTCODE', '1', '第一級管制用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 74, 'E_RESTRICTCODE', '2', '第二級管制用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 75, 'E_RESTRICTCODE', '3', '第三級管制用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 76, 'E_RESTRICTCODE', '4', '第四級管制用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 77, 'CONTRACNO', '1', '軍聯標');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 78, 'CONTRACNO', '2', '自辨合約');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 79, 'CONTRACNO', '01', '藥委會決議為零購之軍聯標品項');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 80, 'CONTRACNO', '02', '藥委會決議為零購之非軍聯標而自辨合約品項');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 81, 'CONTRACNO', '0Y', '藥委會決議為零購之非軍聯標而未自辨合約品項');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 82, 'CONTRACNO', '0N', '未經藥委會決議之上簽零購品項');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 83, 'M_PAYKIND', '3', '醫院自行吸收');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 84, 'M_PAYKIND', '4', '預設自費');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 85, 'CANCEL_NOTE', 'D', '刪除');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 86, 'CANCEL_NOTE', 'S', '停產');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 87, 'CANCEL_NOTE', 'L', '缺貨');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 88, 'CANCEL_NOTE', 'C', '變更');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 89, 'CANCEL_NOTE', 'R', '取代');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 90, 'CANCEL_NOTE', 'O', '其他');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 165, 'E_RESTRICTCODE2', 'N', '非管制用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 166, 'E_RESTRICTCODE2', '1', '1~3級管制用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 167, 'E_RESTRICTCODE2', '4', '第四級管制用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST', 168, 'E_RESTRICTCODE2', 'M', '大瓶點滴');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_MODCOL', 1, 'AUTO_APLID', 'AUTO_APLID', '自動撥補碼');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_MODCOL', 2, 'M_STOREID', 'M_STOREID', '庫備識別碼');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_MODCOL', 3, 'M_IDKEY', 'M_IDKEY', 'ID碼');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_MODCOL', 4, 'M_INVKEY', 'M_INVKEY', '衛材料號碼');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_MODCOL', 5, 'M_GOVKEY', 'M_GOVKEY', '行政院碼');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_MODCOL', 6, 'M_VOLL', 'M_VOLL', '長度(CM)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_MODCOL', 7, 'M_VOLW', 'M_VOLW', '寬度(CM)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_MODCOL', 8, 'M_VOLH', 'M_VOLH', '高度(CM)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_MODCOL', 9, 'M_VOLC', 'M_VOLC', '圓周');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_MODCOL', 10, 'M_SWAP', 'M_SWAP', '材積轉換率');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_MODCOL', 11, 'M_SUPPLYID', 'M_SUPPLYID', '是否供應契約');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_MODCOL', 12, 'M_CONSUMID', 'M_CONSUMID', '消耗屬性');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_MODCOL', 13, 'M_PAYID', 'M_PAYID', '計費方式');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_MODCOL', 14, 'M_TRNID', 'M_TRNID', '扣庫方式');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_MODCOL', 15, 'M_ENVDT', 'M_ENVDT', '環保證號效期');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_MODCOL', 16, 'M_PURUN', 'M_PURUN', '申購計量單位');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_MODCOL', 17, 'WEXP_ID', 'WEXP_ID', '批號效期註記');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_MODCOL', 18, 'WLOC_ID', 'WLOC_ID', '儲位記錄註記');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_MODCOL', 19, 'CANCEL_ID', 'CANCEL_ID', '是否作廢');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_MODCOL', 23, 'PACKTYPE', 'PACKTYPE', '進貨包裝方式');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_MODCOL', 24, 'CANCEL_NOTE', 'CANCEL_NOTE', '作廢備註');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_MODCOL', 25, 'M_APPLYID', 'M_APPLYID', '申請申購識別碼');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_MODCOL', 26, 'EXCH_RATIO', 'EXCH_RATIO', '廠商包裝轉換率');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 0, 'E_SUPSTATUS', '0', '正常使用');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 1, 'E_SUPSTATUS', '1', '刪除');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 2, 'E_SUPSTATUS', '2', '停產');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 3, 'E_SUPSTATUS', '3', '廠缺');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 4, 'E_SUPSTATUS', '4', '變更');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 5, 'WEXP_ID', 'Y', '自動先進先出');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 6, 'WEXP_ID', 'N', '需輸入批號效期');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 7, 'WLOC_ID', 'Y', '進貨及領用消耗需儲位編號');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 8, 'WLOC_ID', 'N', '不需要');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 11, 'E_IFPUBLIC', '0', '非公藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 12, 'E_IFPUBLIC', '1', '存點為病房，上級庫為住院藥局');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 13, 'E_IFPUBLIC', '2', '存點為病房，上級庫為藥庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 14, 'E_IFPUBLIC', '3', '存點為病房，設為備用藥，上級庫為住院藥局');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 15, 'E_GPARMYNO', '1', '藥品軍聯標');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 16, 'E_GPARMYNO', '2', '衛材');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 17, 'E_PRESCRIPTYPE', '1', '單方');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 18, 'E_PRESCRIPTYPE', '2', '複方');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 19, 'E_DRUGCLASSIFY', '0', '一般用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 20, 'E_DRUGCLASSIFY', '1', '原料藥及消耗品');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 21, 'E_DRUGCLASSIFY', '2', '第1至3級管制用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 22, 'E_DRUGCLASSIFY', '9', '分裝品項');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 23, 'E_DRUGCLASSIFY', 'X', '其它(衛材)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 24, 'E_COMITCODE', '1', '是');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 25, 'E_COMITCODE', '2', '否-, 衛材');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 26, 'E_COMITCODE', '3', '否-, 診斷用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 27, 'E_COMITCODE', '4', '否-, 洗腎液(灌洗溶液)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 28, 'E_COMITCODE', '5', '原料藥或消耗品');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 29, 'E_COMITCODE', '6', '006麻醉科用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 30, 'E_COMITCODE', '7', '007放射科用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 31, 'E_COMITCODE', '8', '其他');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 32, 'E_INVFLAG', 'Y', '是');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 33, 'E_INVFLAG', 'N', '否');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 34, 'E_PURTYPE', '1', '甲案');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 35, 'E_PURTYPE', '2', '乙案');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 36, 'E_SOURCECODE', 'P', '買斷');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 37, 'E_SOURCECODE', 'C', '寄售');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 38, 'E_SOURCECODE', 'D', '核醫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 39, 'E_SOURCECODE', 'N', '其它');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 40, 'E_DRUGAPLTYPE', '0', '一般藥品');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 41, 'E_DRUGAPLTYPE', '1', '大瓶點滴');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 42, 'E_DRUGAPLTYPE', '2', '酒精');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 43, 'E_PARCODE', '0', '非母子藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 44, 'E_PARCODE', '1', '母藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 45, 'E_PARCODE', '2', '子藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 46, 'E_DRUGCLASS', '1', '指示用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 47, 'E_DRUGCLASS', '2', '成藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 48, 'E_DRUGCLASS', '3', '處方用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 49, 'E_STOCKTYPE', '0', '非用藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 50, 'E_STOCKTYPE', '1', '水藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 51, 'E_STOCKTYPE', '2', '口服藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 52, 'E_STOCKTYPE', '3', '可保留24小時的針劑');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 53, 'E_STOCKTYPE', '4', '開瓶即消耗');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 54, 'M_STOREID', '0', '非庫備');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 55, 'M_STOREID', '1', '庫備');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 56, 'M_CONTID', '0', '合約品項');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 57, 'M_CONTID', '2', '非合約');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 58, 'M_CONTID', '3', '不能申請');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 59, 'M_CONSUMID', '1', '消耗');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 60, 'M_CONSUMID', '2', '半消耗');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 61, 'M_PAYKIND', '1', '自費');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 62, 'M_PAYKIND', '2', '健保');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 63, 'M_PAYKIND', '3', '醫院吸收');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 64, 'M_PAYID', '1', '計價');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 65, 'M_PAYID', '2', '不計價');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 66, 'M_TRNID', '1', '扣庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 67, 'M_TRNID', '2', '不扣庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 68, 'M_APPLYID', 'E', '不可申請申購');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 69, 'M_APPLYID', 'P', '可申請不可申購');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_MAST_N', 70, 'M_APPLYID', 'Y', '可申請申購');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC, DATA_REMARK) VALUES ('MI_MATCLASS', 1, 'MAT_CLSID', '1', '藥品', '物料分類屬性');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC, DATA_REMARK) VALUES ('MI_MATCLASS', 2, 'MAT_CLSID', '2', '衛材', '物料分類屬性');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC, DATA_REMARK) VALUES ('MI_MATCLASS', 3, 'MAT_CLSID', '3', '一般物品', '物料分類屬性');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC, DATA_REMARK) VALUES ('MI_MATCLASS', 4, 'MAT_CLSID', '4', '能設', '物料分類屬性');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC, DATA_REMARK) VALUES ('MI_MATCLASS', 5, 'MAT_CLSID', '5', '通信', '物料分類屬性');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC, DATA_REMARK) VALUES ('MI_MATCLASS', 6, 'MAT_CLSID', '6', '氣體', '物料分類屬性');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC, DATA_REMARK) VALUES ('MI_MATCLASS', 7, 'MAT_CLSID', '7', '中藥', '物料分類屬性');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHAUTO', 1, 'WEEKDAY', '1', '星期日');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHAUTO', 2, 'WEEKDAY', '2', '星期一');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHAUTO', 3, 'WEEKDAY', '3', '星期二');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHAUTO', 4, 'WEEKDAY', '4', '星期三');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHAUTO', 5, 'WEEKDAY', '5', '星期四');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHAUTO', 6, 'WEEKDAY', '6', '星期五');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHAUTO', 7, 'WEEKDAY', '7', '星期六');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHID', 1, 'TASK_ID', '1', '藥品');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHID', 2, 'TASK_ID', '2', '衛材');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHID', 3, 'TASK_ID', '3', '一般物品');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHID', 4, 'TASK_ID', '4', '能設');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHID', 5, 'TASK_ID', '5', '通信');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHID', 6, 'TASK_ID', '6', '氣體');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHID', 7, 'TASK_ID', '7', '中藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 1, 'WH_GRADE', '1', '庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 2, 'WH_GRADE', '2', '局(衛星庫)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 3, 'WH_GRADE', '3', '病房');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 4, 'WH_GRADE', '4', '科室');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 5, 'WH_GRADE', '5', '戰備庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 6, 'WH_GRADE', '6', '疾管局');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 7, 'WH_KIND', '0', '藥品庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 8, 'WH_KIND', '1', '衛材庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 9, 'WH_KIND', '2', '戰備庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 10, 'WH_KIND', '3', '疾管局庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 11, 'WH_GRADE440', '1', '庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 12, 'WH_GRADE440', '2', '局');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 13, 'WH_GRADE440', '3', '病房');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 14, 'WH_GRADE440', '4', '科室');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 15, 'WH_GRADE441', '1', '庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 16, 'WH_GRADE441', '2', '衛星庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 17, 'WH_KIND44', '0', '藥品');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 18, 'WH_KIND44', '1', '衛材');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 19, 'WH_GRADE420', '1', '藥庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 20, 'WH_GRADE420', '2', '藥局');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 21, 'WH_GRADE420', '3', '病房');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 22, 'WH_GRADE420', '4', '科室');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 50, 'WH_GRADE0065', '0', '0_不分');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 51, 'WH_GRADE0065', '1', '1_門急診病患醫令(扣庫點為二級庫各藥局)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 52, 'WH_GRADE0065', '2', '2_住院病患醫令(扣庫點為二級庫各藥局)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 53, 'WH_GRADE0065', '3', '3_住院病患醫令(扣庫點為三級庫)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 54, 'WH_GRADE0065', '4', '4_二級庫(各藥局)發公藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 55, 'WH_GRADE0065', '5', '5_一級庫(藥庫)發藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 56, 'WH_GRADE0065', '6', '6_一級庫(藥庫)進藥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 57, 'WH_GRADE0065', '7', '7_二級庫(各藥局)調撥');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 58, 'WH_GRADE0065', '8', '8_二級庫(各藥局)調帳');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 61, 'WH_KIND', 'C', '通信');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 62, 'WH_KIND', 'E', '能設');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 63, 'WH_GRADE', 'M', '醫院軍');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 64, 'WH_GRADE', 'S', '學院民');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 100, 'WH_GRADE0077', 'PH1S', 'PH1S_藥庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 200, 'WH_GRADE0077', 'CHEMO', 'CHEMO_內湖化療調配室');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 300, 'WH_GRADE0077', 'CHEMOT', 'CHEMOT_汀州化療調配室');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 400, 'WH_GRADE0077', 'PH1A', 'PH1A_內湖住院藥局');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 500, 'WH_GRADE0077', 'PH1C', 'PH1C_內湖門診藥局');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 600, 'WH_GRADE0077', 'PH1R', 'PH1R_內湖急診藥局');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 700, 'WH_GRADE0077', 'PHMC', 'PHMC_汀州藥局');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 800, 'WH_GRADE0077', 'TPN', 'TPN_製劑室');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 820, 'WH_GRADE0077', 'PCA', 'PCA_疼痛控制配藥室');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 900, 'WH_GRADE0077', 'CSR1', 'CSR1_中央供應室');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 910, 'WH_GRADE0077', 'ALL', '全部藥局_藥庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHMAST', 920, 'WH_GRADE0077', 'All-DS', '全部藥局');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHTRNS', 1, 'TR_IO', 'I', '入庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHTRNS', 2, 'TR_IO', 'O', '出庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHTRNS', 3, 'TR_MCODE', 'APLI', '申請進貨入庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHTRNS', 4, 'TR_MCODE', 'APLO', '核撥出庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHTRNS', 5, 'TR_MCODE', 'BAKI', '繳回入庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHTRNS', 6, 'TR_MCODE', 'BAKO', '繳回出庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHTRNS', 7, 'TR_MCODE', 'REJO', '退貨出庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHTRNS', 8, 'TR_MCODE', 'DISO', '報廢出庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHTRNS', 9, 'TR_MCODE', 'ADJI', '調帳入庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHTRNS', 10, 'TR_MCODE', 'ADJO', '調帳出庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHTRNS', 11, 'TR_MCODE', 'TRNI', '調撥入庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHTRNS', 12, 'TR_MCODE', 'TRNO', '調撥出庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHTRNS', 13, 'TR_MCODE', 'EXGI', '換藥入庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHTRNS', 14, 'TR_MCODE', 'EXGO', '換藥出庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHTRNS', 15, 'TR_MCODE', 'MILI', '軍品調帳入庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHTRNS', 16, 'TR_MCODE', 'MILO', '軍品調帳出庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHTRNS', 17, 'TR_MCODE', 'WAYI', '在途入庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WHTRNS', 18, 'TR_MCODE', 'WAYO', '在途出庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WINVCTL', 1, 'CTDMDCCODE', '0', '繼續使用');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WINVCTL', 2, 'CTDMDCCODE', '1', '刪除');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WINVCTL', 3, 'CTDMDCCODE', '2', '停產');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WINVCTL', 4, 'CTDMDCCODE', '3', '廠缺');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WINVCTL', 5, 'CTDMDCCODE', '4', '斷貨');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WINVCTL', 6, 'USEADJ_CLASS', '1', '以次歸整');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WINVCTL', 7, 'USEADJ_CLASS', '2', '以天歸整');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MI_WINVCTL', 8, 'USEADJ_CLASS', '3', '不歸整');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE) VALUES ('MI_WLOCINV', 0, 'WH_NO', '560000');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE) VALUES ('MI_WLOCINV', 2, 'MMCODE', '08071761');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE) VALUES ('MI_WLOCINV', 3, 'MMCODE', '08071762');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE) VALUES ('MI_WLOCINV', 4, 'MMCODE', '005OXB01');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MM_PACK_M', 1, 'FLOWID', '1', '申請中');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MM_PACK_M', 2, 'FLOWIF', '2', '已核准');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MM_PO_M', 1, 'PO_STATUS', '80', '開單');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MM_PO_M', 2, 'PO_STATUS', '82', '已傳MAIL');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MM_PO_M', 3, 'PO_STATUS', '84', '待傳MAIL');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MM_PO_M', 4, 'PO_STATUS', '83', '已傳真');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MM_PO_M', 5, 'PO_STATUS', '85', '已傳真');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MM_PO_M', 6, 'INVOICE', '1', '全部');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MM_PO_M', 7, 'INVOICE', '2', '發票未維護');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MM_PO_M', 8, 'INVOICE', '3', '發票已維護');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MM_PO_M', 9, 'PO_STATUS', '87', '作廢');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MM_PO_M', 10, 'PO_STATUS', '88', '補寄MAIL');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MM_PO_M', 11, 'M_STOREID', '0', '非庫備');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MM_PO_M', 12, 'M_STOREID', '1', '庫備');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MM_PO_M', 13, 'M_CONTID', '0', '合約');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MM_PO_M', 14, 'M_CONTID', '2', '非合約');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MM_PO_M', 15, 'M_CONTID', '3', '小採');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC, DATA_REMARK) VALUES ('MM_PO_M', 100, 'DLINE_DT', '7', '交貨截止日期前?天', '交貨截止日期前幾天未交貨,以排程MAIL通知');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('MM_PO_M', 101, 'DLINE_MSG', '請貴公司儘快於交貨截止日期前完成交貨, 有問題請電話連絡本院承辦人', '廠商訂單稽催訊息');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC, DATA_REMARK) VALUES ('MM_PO_M', 102, 'DLINE_NOTE', '承辦人: ,電話: ', '承辦人', '廠商訂單稽催訊息承辦人姓名及電話');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC, DATA_REMARK) VALUES ('MM_PR_M', 1, '02_BDAY', '01', '衛材申購起始日', '20');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC, DATA_REMARK) VALUES ('MM_PR_M', 2, '02_EDAY', '30', '衛材申購結束日', '27');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC, DATA_REMARK) VALUES ('MM_PR_M', 3, '0308_BDAY', '01', '一般物品申購起始日', '13');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC, DATA_REMARK) VALUES ('MM_PR_M', 4, '0308_EDAY', '27', '一般物品申購結束日 ', '17');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('PH_INVOICE', 1, 'MEMO', 'A', '發票金額不對');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('PH_INVOICE', 2, 'MEMO', 'B', '發票抬頭錯誤');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('PH_INVOICE', 3, 'CKSTATUS', 'N', '未驗證');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('PH_INVOICE', 4, 'CKSTATUS', 'Y', '已驗證');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('PH_INVOICE', 5, 'CKSTATUS', 'T', '驗退');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('PH_SMALL_M', 1, 'STATUS', 'A', '新增');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('PH_SMALL_M', 2, 'STATUS', 'B', '待單位主管(業務督導長)審核');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('PH_SMALL_M', 3, 'STATUS', 'C', '待消審會審核');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('PH_SMALL_M', 4, 'STATUS', 'D', '剔退');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('PH_SMALL_M', 5, 'STATUS', 'E', '消審會核准');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('PH_SMALL_M', 6, 'STATUS', 'F', '已轉採購單');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('PH_SMALL_M', 11, 'STATUS', 'G', '待護理部業務副主任審核');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('PH_SMALL_M', 13, 'STATUS', 'H', '待護理部主任審核');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('PH_SMALL_M', 20, 'CHARGE', '0', '不給付');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('PH_SMALL_M', 21, 'CHARGE', '1', '給付');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('PH_SMALL_M', 22, 'CHARGE', '2', '條件給付');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('PH_SMALL_M', 23, 'CHARGE', '3', '醫院自行吸收');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('PH_SMALL_M', 24, 'CHARGE', '4', '預設自費');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('PH_VENDER', 1, 'MAIN_INID', '550000', '能源設施室');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('PH_VENDER', 2, 'MAIN_INID', '740600', '內湖資管室通信綜合組');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('SCAN_USE_LOG', 1, 'ISUSE', 'Y', '已扣庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('SCAN_USE_LOG', 2, 'ISUSE', 'N', '不扣庫(逆推非買斷)');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('SCAN_USE_LOG', 3, 'ISUSE', 'Z', '其它系統扣庫');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC, DATA_REMARK) VALUES ('TC_PURCH_M', 1, 'PURCH_ST', 'A', '待訂購', '(中藥)訂單狀態');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC, DATA_REMARK) VALUES ('TC_PURCH_M', 2, 'PURCH_ST', 'B', '已訂購', '(中藥)訂單狀態');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC, DATA_REMARK) VALUES ('TC_PURCH_M', 3, 'TC_TYPE', 'A', '科學中藥', '(中藥)藥品種類');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC, DATA_REMARK) VALUES ('TC_PURCH_M', 4, 'TC_TYPE', 'B', '飲片', '(中藥)藥品種類');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('UR_INID', 1, 'INID_FLAG', 'A', '應盤點單位');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('UR_INID', 2, 'INID_FLAG', 'B', '行政科室');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('UR_INID', 3, 'INID_FLAG', 'C', '財務獨立單位');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('VIEWALL', 1, 'VIEWALL_GROUP', 'VIEWALL', '群組ID');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('VIEWALL', 2, 'VIEWALL_GROUP', 'ADMG', '群組ID');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('Y_OR_N', 0, 'YN', 'Y', '是');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC) VALUES ('Y_OR_N', 1, 'YN', 'N', '否');
                                            INSERT INTO MMSADM.PARAM_D (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC, DATA_FLAG, DATA_REMARK) VALUES ('CHK_MAST', 70, 'CHK_WH_KIND_1', 'X', '不區分', NULL, NULL); 
                                            {2}
                                        END;
                                    ", hospName, hosp_id, sql_sub);
            return sql;
        }

        public string Insert_MI_DOCTYPE()
        {
            //0.5 MI_DOCTYPE
            string sql = @"
                                        BEGIN
                                            INSERT INTO MMSADM.MI_DOCTYPE (DOCTYPE, DOCTYPE_GP, DOCTYPE_NAME) VALUES ('AJ1', 'ADJ', '衛材調帳');
                                            INSERT INTO MMSADM.MI_DOCTYPE (DOCTYPE, DOCTYPE_GP, DOCTYPE_NAME) VALUES ('AJ2', 'ADJ', '衛材調帳');
                                            INSERT INTO MMSADM.MI_DOCTYPE (DOCTYPE, DOCTYPE_GP, DOCTYPE_NAME) VALUES ('AJ3', 'ADJ', '衛材調帳');
                                            INSERT INTO MMSADM.MI_DOCTYPE (DOCTYPE, DOCTYPE_GP, DOCTYPE_NAME) VALUES ('AJ4', 'ADJ', '衛材調帳');
                                            INSERT INTO MMSADM.MI_DOCTYPE (DOCTYPE, DOCTYPE_GP, DOCTYPE_NAME) VALUES ('AIR', 'AIR', '氣體申請核撥');
                                            INSERT INTO MMSADM.MI_DOCTYPE (DOCTYPE, DOCTYPE_GP, DOCTYPE_NAME) VALUES ('AJ', 'AJ', '調帳');
                                            INSERT INTO MMSADM.MI_DOCTYPE (DOCTYPE, DOCTYPE_GP, DOCTYPE_NAME) VALUES ('D', 'D', '醫令扣庫(日)');
                                            INSERT INTO MMSADM.MI_DOCTYPE (DOCTYPE, DOCTYPE_GP, DOCTYPE_NAME) VALUES ('EM', 'EM', '藥品換貨(金額)');
                                            INSERT INTO MMSADM.MI_DOCTYPE (DOCTYPE, DOCTYPE_GP, DOCTYPE_NAME) VALUES ('EV', 'EV', '藥品廠商退貨');
                                            INSERT INTO MMSADM.MI_DOCTYPE (DOCTYPE, DOCTYPE_GP, DOCTYPE_NAME) VALUES ('EX', 'EX', '藥品等量換貨');
                                            INSERT INTO MMSADM.MI_DOCTYPE (DOCTYPE, DOCTYPE_GP, DOCTYPE_NAME) VALUES ('EX1', 'EX1', '衛材換貨');
                                            INSERT INTO MMSADM.MI_DOCTYPE (DOCTYPE, DOCTYPE_GP, DOCTYPE_NAME) VALUES ('IN', 'IN', '驗收進貨');
                                            INSERT INTO MMSADM.MI_DOCTYPE (DOCTYPE, DOCTYPE_GP, DOCTYPE_NAME) VALUES ('MR', 'MR', '藥品申請核撥');
                                            INSERT INTO MMSADM.MI_DOCTYPE (DOCTYPE, DOCTYPE_GP, DOCTYPE_NAME) VALUES ('MR1', 'MR1', '一般物品庫備申請核撥');
                                            INSERT INTO MMSADM.MI_DOCTYPE (DOCTYPE, DOCTYPE_GP, DOCTYPE_NAME) VALUES ('MR2', 'MR2', '衛材庫備申請核撥');
                                            INSERT INTO MMSADM.MI_DOCTYPE (DOCTYPE, DOCTYPE_GP, DOCTYPE_NAME) VALUES ('MR3', 'MR3', '一般物品非庫備申請核撥');
                                            INSERT INTO MMSADM.MI_DOCTYPE (DOCTYPE, DOCTYPE_GP, DOCTYPE_NAME) VALUES ('MR4', 'MR4', '衛材非庫備申請核撥');
                                            INSERT INTO MMSADM.MI_DOCTYPE (DOCTYPE, DOCTYPE_GP, DOCTYPE_NAME) VALUES ('MS', 'MS', '公藥申請核撥');
                                            INSERT INTO MMSADM.MI_DOCTYPE (DOCTYPE, DOCTYPE_GP, DOCTYPE_NAME) VALUES ('RE', 'RE', '藥品效期回報');
                                            INSERT INTO MMSADM.MI_DOCTYPE (DOCTYPE, DOCTYPE_GP, DOCTYPE_NAME) VALUES ('RJ', 'RJ', '藥品退貨');
                                            INSERT INTO MMSADM.MI_DOCTYPE (DOCTYPE, DOCTYPE_GP, DOCTYPE_NAME) VALUES ('RJ1', 'RJ1', '衛材退貨');
                                            INSERT INTO MMSADM.MI_DOCTYPE (DOCTYPE, DOCTYPE_GP, DOCTYPE_NAME) VALUES ('RN', 'RN', '藥品繳庫');
                                            INSERT INTO MMSADM.MI_DOCTYPE (DOCTYPE, DOCTYPE_GP, DOCTYPE_NAME) VALUES ('RN1', 'RN1', '衛材繳回');
                                            INSERT INTO MMSADM.MI_DOCTYPE (DOCTYPE, DOCTYPE_GP, DOCTYPE_NAME) VALUES ('RR', 'RR', '藥品補發');
                                            INSERT INTO MMSADM.MI_DOCTYPE (DOCTYPE, DOCTYPE_GP, DOCTYPE_NAME) VALUES ('RS', 'RS', '藥品退藥');
                                            INSERT INTO MMSADM.MI_DOCTYPE (DOCTYPE, DOCTYPE_GP, DOCTYPE_NAME) VALUES ('SP', 'SP', '藥品報廢');
                                            INSERT INTO MMSADM.MI_DOCTYPE (DOCTYPE, DOCTYPE_GP, DOCTYPE_NAME) VALUES ('SP1', 'SP1', '衛材報廢');
                                            INSERT INTO MMSADM.MI_DOCTYPE (DOCTYPE, DOCTYPE_GP, DOCTYPE_NAME) VALUES ('SQ', 'SQ', '藥品破損更換');
                                            INSERT INTO MMSADM.MI_DOCTYPE (DOCTYPE, DOCTYPE_GP, DOCTYPE_NAME) VALUES ('T', 'T', '醫令扣庫(時)');
                                            INSERT INTO MMSADM.MI_DOCTYPE (DOCTYPE, DOCTYPE_GP, DOCTYPE_NAME) VALUES ('TR', 'TR', '藥品調撥');
                                            INSERT INTO MMSADM.MI_DOCTYPE (DOCTYPE, DOCTYPE_GP, DOCTYPE_NAME) VALUES ('TR1', 'TR1', '衛材調撥');
                                            INSERT INTO MMSADM.MI_DOCTYPE (DOCTYPE, DOCTYPE_GP, DOCTYPE_NAME) VALUES ('XE', 'XE', '藥品近效期更換');
                                            INSERT INTO MMSADM.MI_DOCTYPE (DOCTYPE, DOCTYPE_GP, DOCTYPE_NAME) VALUES ('XR', 'XR', '藥品軍品調帳');
                                            INSERT INTO MMSADM.MI_DOCTYPE (DOCTYPE, DOCTYPE_GP, DOCTYPE_NAME) VALUES ('XR1', 'XR1', '衛材軍品調帳');
                                            INSERT INTO MMSADM.MI_DOCTYPE (DOCTYPE, DOCTYPE_GP, DOCTYPE_NAME) VALUES ('XR2', 'XR2', '衛材軍品調整');
                                            INSERT INTO MMSADM.MI_DOCTYPE (DOCTYPE, DOCTYPE_GP, DOCTYPE_NAME) VALUES ('XR3', 'XR3', '藥品軍品調整');
                                            INSERT INTO MMSADM.MI_DOCTYPE (DOCTYPE, DOCTYPE_NAME) VALUES ('CM1', '通信申請核撥');
                                            INSERT INTO MMSADM.MI_DOCTYPE (DOCTYPE, DOCTYPE_NAME) VALUES ('CM2', '通信申購進貨');
                                            INSERT INTO MMSADM.MI_DOCTYPE (DOCTYPE, DOCTYPE_NAME) VALUES ('EF1', '能設申請核撥');
                                            INSERT INTO MMSADM.MI_DOCTYPE (DOCTYPE, DOCTYPE_NAME) VALUES ('EF2', '能設申購進貨');
                                        END;
                                    ";
            return sql;
        }

        public string Insert_BASEUNITCNV()
        {
            //0.6 BASEUNITCNV
            string sql = @"
                                        BEGIN
                                            INSERT INTO MMSADM.BASEUNITCNV (UI_FROM, UI_TO, COEFF_FROM, COEFF_TO, CREATE_TIME, CREATE_USER) VALUES ('G', 'G', 1, 1, sysdate, '上線轉檔');
                                            INSERT INTO MMSADM.BASEUNITCNV (UI_FROM, UI_TO, COEFF_FROM, COEFF_TO, CREATE_TIME, CREATE_USER) VALUES ('G', 'GM', 1, 1, sysdate, '上線轉檔');
                                            INSERT INTO MMSADM.BASEUNITCNV (UI_FROM, UI_TO, COEFF_FROM, COEFF_TO, CREATE_TIME, CREATE_USER) VALUES ('G', 'MG', 1, 1000, sysdate, '上線轉檔');
                                            INSERT INTO MMSADM.BASEUNITCNV (UI_FROM, UI_TO, COEFF_FROM, COEFF_TO, CREATE_TIME, CREATE_USER) VALUES ('GM', 'G', 1, 1, sysdate, '上線轉檔');
                                            INSERT INTO MMSADM.BASEUNITCNV (UI_FROM, UI_TO, COEFF_FROM, COEFF_TO, CREATE_TIME, CREATE_USER) VALUES ('GM', 'GM', 1, 1, sysdate, '上線轉檔');
                                            INSERT INTO MMSADM.BASEUNITCNV (UI_FROM, UI_TO, COEFF_FROM, COEFF_TO, CREATE_TIME, CREATE_USER) VALUES ('GM', 'MG', 1, 1000, sysdate, '上線轉檔');
                                            INSERT INTO MMSADM.BASEUNITCNV (UI_FROM, UI_TO, COEFF_FROM, COEFF_TO, CREATE_TIME, CREATE_USER) VALUES ('MG', 'G', 1000, 1, sysdate, '上線轉檔');
                                            INSERT INTO MMSADM.BASEUNITCNV (UI_FROM, UI_TO, COEFF_FROM, COEFF_TO, CREATE_TIME, CREATE_USER) VALUES ('MG', 'GM', 1000, 1, sysdate, '上線轉檔');
                                            INSERT INTO MMSADM.BASEUNITCNV (UI_FROM, UI_TO, COEFF_FROM, COEFF_TO, CREATE_TIME, CREATE_USER) VALUES ('MG', 'MG', 1, 1, sysdate, '上線轉檔');
                                        END;
                                    ";
            return sql;
        }

        public string Insert_MI_MNSET()
        {
            //0.7 MI_MNSET
            string sql = @"
                                        INSERT INTO MMSADM.MI_MNSET (SET_YM,SET_BTIME,SET_STATUS,SET_MSG,SET_CTIME) 
                                        VALUES (twn_yyymm(sysdate),sysdate,'N','已開帳',sysdate)
                                    ";
            return sql;
        }

        public string Insert_ME_FLOW()
        {
            //0.8 ME_FLOW
            string sql = @"
                                        BEGIN
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_KIND) VALUES ('X', 'EX', '已撤銷', 'N', '0');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_KIND) VALUES ('1399', 'RR', '已結案', 'N', '0');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, FLOWBTN1, TR_ID, TR_FLOWID, TR_KIND, POST_COL) VALUES ('1401', 'XE', '換藥申請中', '1499', '完成近效期更換', 'Y', '1499', '0', 'APPQTY');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_KIND) VALUES ('1499', 'XE', '換藥結案', 'N', '0');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_KIND) VALUES ('X', 'EM', '已撤銷', 'N', '0');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, FLOWBTN1, TR_ID, TR_FLOWID, TR_KIND) VALUES ('1601', 'RE', '未回報', '1699', '完成效期回報', 'Y', '1699', '0');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_KIND) VALUES ('1699', 'RE', '已回報', 'N', '0');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, TR_ID, TR_FLOWID, TR_KIND, POST_COL) VALUES ('1', 'XR1', '軍品調帳中', '2', 'Y', '2', '1', 'APPQTY');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_KIND) VALUES ('2', 'XR1', '軍品已調帳', 'N', '1');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_FLOWID1, TR_KIND) VALUES ('3', 'AJ1', '已調帳', 'N', 'O', '1');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, TR_ID, TR_KIND) VALUES ('1', 'AIR', '氣體申請中', '2', 'N', '1');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, TR_ID, TR_FLOWID, TR_KIND) VALUES ('2', 'AIR', '氣體核撥中', '3', 'Y', '3', '1');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_KIND) VALUES ('X', 'RJ', '已撤銷', 'N', '0');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_KIND) VALUES ('0100', 'MR', '自動申請', 'N', '0');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, FLOWBTN1, TR_ID, TR_KIND) VALUES ('0101', 'MR', '手動申請', '0102', '提出申請', 'N', '0');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, FLOWBTN1, TR_ID, TR_FLOWID, TR_KIND, POST_COL) VALUES ('0102', 'MR', '手動申請-核撥中', '0103', '核撥', 'Y', '0103', '0', 'APVQTY');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, FLOWBTN1, FLOWID2, FLOWBTN2, TR_ID, TR_FLOWID, TR_KIND, POST_COL) VALUES ('0103', 'MR', '手動申請-點收中', '0199', '結案', '0104', '藥庫結案', 'Y', '0104', '0', 'ACKQTY');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, FLOWBTN1, TR_ID, TR_FLOWID, TR_KIND, POST_COL) VALUES ('0104', 'MR', '手動申請-沖帳中', '0199', '結案', 'Y', '0199', '0', 'ACKQTY');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_KIND) VALUES ('0199', 'MR', '手動申請-結案', 'N', '0');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, FLOWBTN1, TR_ID, TR_KIND) VALUES ('0201', 'TR', '申請中', '0202', '提出申請', 'N', '0');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, FLOWBTN1, TR_ID, TR_FLOWID, TR_KIND, POST_COL) VALUES ('0202', 'TR', '調出中', '0203', '執行調出', 'Y', '0203', '0', 'APVQTY');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, FLOWBTN1, TR_ID, TR_FLOWID, TR_KIND, POST_COL) VALUES ('0203', 'TR', '調入中', '0299', '執行調入', 'Y', '0299', '0', 'ACKQTY');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_KIND) VALUES ('0299', 'TR', '已結案', 'N', '0');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, FLOWBTN1, TR_ID, TR_FLOWID, TR_KIND, POST_COL) VALUES ('0301', 'XR', '未過帳', '0399', '過帳', 'Y', '0399', '0', 'APPQTY');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_KIND) VALUES ('0399', 'XR', '已結案', 'N', '0');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, FLOWBTN1, TR_ID, TR_KIND) VALUES ('0401', 'RN', '申請中', '0402', '提出申請', 'N', '0');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, FLOWBTN1, TR_ID, TR_FLOWID, TR_KIND, POST_COL) VALUES ('0402', 'RN', '點收中', '0499', '結案', 'Y', '0499', '0', 'APVQTY');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_KIND) VALUES ('0499', 'RN', '已結案', 'N', '0');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, FLOWBTN1, TR_ID, TR_FLOWID, TR_KIND, POST_COL) VALUES ('0501', 'SP', '未過帳', '0599', '結案', 'Y', '0599', '0', 'APPQTY');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_KIND) VALUES ('0599', 'SP', '已結案', 'N', '0');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_KIND) VALUES ('0600', 'MS', '自動申請', 'N', '0');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, FLOWBTN1, TR_ID, TR_KIND) VALUES ('0601', 'MS', '手動申請', '0602', '提出申請', 'N', '0');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, TR_ID, TR_FLOWID, TR_KIND) VALUES ('1', 'EF1', '能設申請中', '2', 'Y', '2', 'E');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_FLOWID, TR_KIND) VALUES ('2', 'EF1', '能設已核撥', 'Y', '1', 'E');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_KIND) VALUES ('0699', 'MS', '公藥申請-結案', 'N', '0');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, FLOWBTN1, TR_ID, TR_FLOWID, TR_KIND) VALUES ('0701', 'IN', '進貨中', '0799', '已進貨', 'Y', '0799', '0');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_KIND) VALUES ('0799', 'IN', '已進貨', 'N', '0');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, FLOWBTN1, TR_ID, TR_FLOWID, TR_KIND, POST_COL) VALUES ('0801', 'EX', '藥品換貨', '0899', '完成藥品換貨', 'Y', '0899', '0', 'APPQTY');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_KIND) VALUES ('0899', 'EX', '完成藥品換貨', 'N', '0');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, FLOWBTN1, TR_ID, TR_FLOWID, TR_KIND, POST_COL) VALUES ('0901', 'RJ', '藥品退貨', '0999', '完成藥品退貨', 'Y', '0999', '0', 'APPQTY');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_KIND) VALUES ('0999', 'RJ', '完成藥品退貨', 'N', '0');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, FLOWBTN1, TR_ID, TR_FLOWID, TR_KIND, POST_COL) VALUES ('1001', 'EM', '藥品換貨(金額)', '1099', '完成藥品換貨(金額)', 'Y', '1099', '0', 'APPQTY');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_KIND) VALUES ('1099', 'EM', '完成藥品換貨(金額)', 'N', '0');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, FLOWBTN1, TR_ID, TR_KIND, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP) VALUES ('1201', 'RS', '申請中', '1202', '提出申請', 'N', '0', TO_DATE('08/06/2019 15:40:40', 'MM/DD/YYYY HH24:MI:SS'), 'admin', TO_DATE('08/06/2019 15:40:40', 'MM/DD/YYYY HH24:MI:SS'), 'admin', '0.0.0.0');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_KIND) VALUES ('1299', 'RS', '已結案', 'N', '0');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, TR_ID, TR_KIND) VALUES ('1', 'MR1', '庫備一般物品申請中', '2', 'N', '1');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, TR_ID, TR_FLOWID, TR_KIND, POST_COL) VALUES ('2', 'MR1', '庫備一般物品核撥中', '3', 'Y', '3', '1', 'EXPT_DISTQTY');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, TR_ID, TR_FLOWID, TR_KIND, POST_COL) VALUES ('3', 'MR1', '庫備一般物品揀料中', '4', 'Y', '5', '1', 'ACKQTY');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, TR_ID, TR_FLOWID, TR_KIND, POST_COL) VALUES ('4', 'MR1', '庫備一般物品點收中', '5', 'Y', '5', '1', 'ACKQTY');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, TR_ID, TR_FLOWID, TR_KIND, POST_COL) VALUES ('5', 'MR1', '庫備一般物品已點收', '6', 'Y', '6', '1', 'APVQTY');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_KIND) VALUES ('6', 'MR1', '庫備一般物品核撥確認', 'N', '1');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, TR_ID, TR_KIND) VALUES ('1', 'MR2', '庫備衛材申請中', '2', 'N', '1');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, TR_ID, TR_FLOWID, TR_KIND, POST_COL) VALUES ('2', 'MR2', '庫備衛材核撥中', '3', 'Y', '3', '1', 'EXPT_DISTQTY');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, TR_ID, TR_FLOWID, TR_KIND, POST_COL) VALUES ('3', 'MR2', '庫備衛材揀料中', '4', 'Y', '5', '1', 'ACKQTY');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, TR_ID, TR_FLOWID, TR_KIND, POST_COL) VALUES ('4', 'MR2', '庫備衛材點收中', '5', 'Y', '5', '1', 'ACKQTY');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, TR_ID, TR_FLOWID, TR_KIND, POST_COL) VALUES ('5', 'MR2', '庫備衛材已點收', '6', 'Y', '6', '1', 'APVQTY');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_KIND) VALUES ('6', 'MR2', '庫備衛材核撥確認', 'N', '1');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, TR_ID, TR_KIND) VALUES ('1', 'MR3', '非庫備一般物品申請中', '2', 'N', '1');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, TR_ID, TR_FLOWID, TR_KIND) VALUES ('2', 'MR3', '非庫備一般物品申購中', '3', 'Y', '4', '1');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, TR_ID, TR_FLOWID, TR_KIND, POST_COL) VALUES ('3', 'MR3', '非庫備一般物品交貨中', '4', 'Y', '5', '1', 'ACKQTY');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, TR_ID, TR_FLOWID, TR_KIND, POST_COL) VALUES ('4', 'MR3', '非庫備一般物品點收中', '5', 'Y', '5', '1', 'ACKQTY');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, TR_ID, TR_FLOWID, TR_KIND, POST_COL) VALUES ('5', 'MR3', '非庫備一般物品已點收', '6', 'Y', '6', '1', 'APVQTY');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_KIND) VALUES ('6', 'MR3', '非庫備一般物品核撥確認', 'N', '1');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, TR_ID, TR_KIND) VALUES ('1', 'MR4', '非庫備衛材申請中', '2', 'N', '1');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, TR_ID, TR_FLOWID, TR_KIND) VALUES ('2', 'MR4', '非庫備衛材申購中', '3', 'Y', '4', '1');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, TR_ID, TR_FLOWID, TR_KIND, POST_COL) VALUES ('3', 'MR4', '非庫備衛材交貨中', '4', 'Y', '5', '1', 'ACKQTY');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, TR_ID, TR_FLOWID, TR_KIND, POST_COL) VALUES ('4', 'MR4', '非庫備衛材點收中', '5', 'Y', '5', '1', 'ACKQTY');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, TR_ID, TR_FLOWID, TR_KIND, POST_COL) VALUES ('5', 'MR4', '非庫備衛材已點收', '6', 'Y', '6', '1', 'APVQTY');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_KIND) VALUES ('6', 'MR4', '非庫備衛材核撥確認', 'N', '1');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, TR_ID, TR_FLOWID, TR_KIND) VALUES ('1', 'CM1', '通訊申請中', '2', 'Y', '2', 'C');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_FLOWID, TR_KIND) VALUES ('2', 'CM1', '通訊已核撥', 'Y', '1', 'C');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, FLOWBTN1, TR_ID, TR_KIND, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP) VALUES ('1301', 'RR', '申請中', '1302', '提出申請', 'N', '0', TO_DATE('08/06/2019 15:40:40', 'MM/DD/YYYY HH24:MI:SS'), 'admin', TO_DATE('08/06/2019 15:40:40', 'MM/DD/YYYY HH24:MI:SS'), 'admin', '0.0.0.0');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, FLOWBTN1, TR_ID, TR_FLOWID, TR_KIND, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP) VALUES ('1302', 'RR', '補發中', '1399', '結案', 'Y', '1399', '0', TO_DATE('08/06/2019 15:40:40', 'MM/DD/YYYY HH24:MI:SS'), 'admin', TO_DATE('08/06/2019 15:40:40', 'MM/DD/YYYY HH24:MI:SS'), 'admin', '0.0.0.0');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, FLOWBTN1, TR_ID, TR_FLOWID, TR_KIND, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP) VALUES ('1702', 'AJ', '調帳中', '1799', '結案', 'Y', '1799', '0', TO_DATE('08/06/2019 15:40:40', 'MM/DD/YYYY HH24:MI:SS'), 'admin', TO_DATE('08/06/2019 15:40:40', 'MM/DD/YYYY HH24:MI:SS'), 'admin', '0.0.0.0');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_KIND, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP) VALUES ('1799', 'AJ', '已結案', 'N', '0', TO_DATE('08/06/2019 15:40:40', 'MM/DD/YYYY HH24:MI:SS'), 'admin', TO_DATE('08/06/2019 15:40:40', 'MM/DD/YYYY HH24:MI:SS'), 'admin', '0.0.0.0');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, FLOWBTN1, TR_ID, TR_FLOWID, TR_KIND, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP) VALUES ('0602', 'MS', '公藥申請-核撥中', '0603', '核撥', 'Y', '0603', '0', TO_DATE('08/14/2019 11:45:49', 'MM/DD/YYYY HH24:MI:SS'), 'admin', TO_DATE('08/14/2019 11:45:49', 'MM/DD/YYYY HH24:MI:SS'), 'admin', '0.0.0.0');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, FLOWBTN1, TR_ID, TR_FLOWID, TR_KIND, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP) VALUES ('0603', 'MS', '公藥申請-點收中', '0699', '結案', 'Y', '0604', '0', TO_DATE('08/14/2019 11:45:49', 'MM/DD/YYYY HH24:MI:SS'), 'admin', TO_DATE('08/14/2019 11:45:49', 'MM/DD/YYYY HH24:MI:SS'), 'admin', '0.0.0.0');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, TR_ID, TR_FLOWID, TR_KIND) VALUES ('1', 'EF2', '能設申購中', '2', 'Y', '2', 'E');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_FLOWID, TR_KIND) VALUES ('2', 'EF2', '能設已進貨', 'Y', '1', 'E');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, TR_ID, TR_FLOWID, TR_KIND) VALUES ('1', 'CM2', '通訊申購中', '2', 'Y', '2', 'C');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_FLOWID, TR_KIND) VALUES ('2', 'CM2', '通訊已進貨', 'Y', '1', 'C');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, TR_ID, TR_FLOWID1, TR_KIND) VALUES ('1', 'AJ1', '申請中', '2', 'N', 'O', '1');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_FLOWID, TR_FLOWID1, TR_KIND, POST_COL) VALUES ('2', 'AJ1', '調帳中', 'Y', '3', 'O', '1', 'APPQTY');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_FLOWID, TR_FLOWID1, TR_KIND, POST_COL) VALUES ('2', 'AJ2', '調帳中', 'Y', '3', 'I', '1', 'APPQTY');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_FLOWID1, TR_KIND) VALUES ('3', 'AJ2', '已調帳', 'N', 'I', '1');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_FLOWID, TR_FLOWID1, TR_KIND, POST_COL) VALUES ('2', 'AJ3', '調帳中', 'Y', '3', 'I', '1', 'APPQTY');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_FLOWID1, TR_KIND) VALUES ('3', 'AJ3', '已調帳', 'N', 'I', '1');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_FLOWID, TR_FLOWID1, TR_KIND, POST_COL) VALUES ('2', 'AJ4', '調帳中', 'Y', '3', 'I', '1', 'APPQTY');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_FLOWID1, TR_KIND) VALUES ('3', 'AJ4', '已調帳', 'N', 'I', '1');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, TR_ID, TR_FLOWID, TR_KIND, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, POST_COL) VALUES ('1402', 'XE', '合約價退款申請中', '1497', 'Y', '1497', '0', TO_DATE('05/17/2019 14:50:47', 'MM/DD/YYYY HH24:MI:SS'), 'admin', TO_DATE('05/17/2019 14:50:47', 'MM/DD/YYYY HH24:MI:SS'), 'admin', '0.0.0.0', 'APPQTY');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, TR_ID, TR_FLOWID, TR_KIND, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, POST_COL) VALUES ('1403', 'XE', '優惠價退款申請中', '1498', 'Y', '1498', '0', TO_DATE('05/17/2019 14:50:47', 'MM/DD/YYYY HH24:MI:SS'), 'admin', TO_DATE('05/17/2019 14:50:47', 'MM/DD/YYYY HH24:MI:SS'), 'admin', '0.0.0.0', 'APPQTY');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_KIND, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP) VALUES ('1497', 'XE', '合約價退款結案', 'N', '0', TO_DATE('05/17/2019 14:50:47', 'MM/DD/YYYY HH24:MI:SS'), 'admin', TO_DATE('05/17/2019 14:50:47', 'MM/DD/YYYY HH24:MI:SS'), 'admin', '0.0.0.0');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_KIND, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP) VALUES ('1498', 'XE', '優惠價退款結案', 'N', '0', TO_DATE('05/17/2019 14:50:47', 'MM/DD/YYYY HH24:MI:SS'), 'admin', TO_DATE('05/17/2019 14:50:47', 'MM/DD/YYYY HH24:MI:SS'), 'admin', '0.0.0.0');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_FLOWID1, TR_KIND) VALUES ('X', 'AJ1', '已撤銷', 'N', 'O', '1');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, TR_ID, TR_FLOWID1, TR_KIND) VALUES ('1', 'AJ2', '申請中', '2', 'N', 'I', '1');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_FLOWID1, TR_KIND) VALUES ('X', 'AJ2', '已撤銷', 'N', 'I', '1');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, TR_ID, TR_FLOWID1, TR_KIND) VALUES ('1', 'AJ3', '申請中', '2', 'N', 'I', '1');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_FLOWID1, TR_KIND) VALUES ('X', 'AJ3', '已撤銷', 'N', 'I', '1');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, TR_ID, TR_FLOWID1, TR_KIND) VALUES ('1', 'AJ4', '申請中', '2', 'N', 'I', '1');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_FLOWID1, TR_KIND) VALUES ('X', 'AJ4', '已撤銷', 'N', 'I', '1');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, FLOWBTN1, TR_ID, TR_FLOWID, TR_KIND, POST_COL) VALUES ('0604', 'MS', '公藥申請-沖帳中', '0699', '結案', 'Y', '0699', '0', 'ACKQTY');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, FLOWBTN1, FLOWID2, FLOWBTN2, TR_ID, TR_FLOWID, TR_KIND, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP) VALUES ('1202', 'RS', '點收中', '1299', '結案', '1201', '退回', 'Y', '1299', '0', TO_DATE('06/03/2019 10:51:49', 'MM/DD/YYYY HH24:MI:SS'), 'admin', TO_DATE('06/03/2019 10:51:49', 'MM/DD/YYYY HH24:MI:SS'), 'admin', '0.0.0.0');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, FLOWBTN1, TR_ID, TR_KIND) VALUES ('0201', 'TR1', '申請中', '0202', '提出申請', 'N', '1');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, FLOWBTN1, TR_ID, TR_FLOWID, TR_KIND, POST_COL) VALUES ('0202', 'TR1', '調出中', '0203', '執行調出', 'Y', '0203', '1', 'APVQTY');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, FLOWBTN1, TR_ID, TR_FLOWID, TR_KIND, POST_COL) VALUES ('0203', 'TR1', '調入中', '0299', '執行調入', 'Y', '0299', '1', 'ACKQTY');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_KIND) VALUES ('0299', 'TR1', '已結案', 'N', '1');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, TR_ID, TR_FLOWID, TR_KIND) VALUES ('1', 'RJ1', '退貨申請中', '3', 'Y', '3', '1');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_KIND, POST_COL) VALUES ('X', 'RJ1', '已撤銷', 'N', '1', 'APPQTY');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_KIND) VALUES ('3', 'RJ1', '已退貨', 'N', '1');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_KIND) VALUES ('X', 'RN1', '已退回', 'N', '1');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, TR_ID, TR_FLOWID, TR_KIND) VALUES ('1', 'EX1', '換貨申請中', '3', 'Y', '3', '1');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_KIND, POST_COL) VALUES ('X', 'EX1', '已撤銷', 'N', '1', 'APPQTY');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_KIND) VALUES ('3', 'EX1', '已換貨', 'N', '1');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_FLOWID, TR_KIND) VALUES ('3', 'AIR', '氣體已核撥', 'Y', '2', '1');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, TR_ID, TR_KIND) VALUES ('1', 'RN1', '繳回申請中', '2', 'N', '1');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, TR_ID, TR_FLOWID, TR_KIND, POST_COL) VALUES ('2', 'RN1', '繳回中', '3', 'Y', '3', '1', 'APPQTY');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_KIND) VALUES ('3', 'RN1', '已繳回', 'N', '1');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, TR_ID, TR_FLOWID, TR_KIND) VALUES ('1', 'SP1', '報廢申請中', '3', 'Y', '3', '1');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_KIND) VALUES ('3', 'SP1', '已報廢', 'N', '1');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_KIND) VALUES ('X', 'SP1', '已撤銷', 'N', '1');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, FLOWBTN1, TR_ID, TR_FLOWID, TR_KIND, POST_COL) VALUES ('0204', 'TR', '取消調撥中', '0202', '取消調撥中', 'Y', '0202', '0', 'APVQTY');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, FLOWBTN1, TR_ID, TR_FLOWID, TR_KIND, POST_COL) VALUES ('0204', 'TR1', '取消調撥中', '0202', '取消調撥中', 'Y', '0202', '1', 'APVQTY');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, FLOWBTN1, TR_ID, TR_FLOWID, TR_KIND) VALUES ('1801', 'XR2', '未過帳', '1899', '過帳', 'Y', '1899', '1');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_KIND) VALUES ('1899', 'XR2', '已結案', 'N', '1');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, FLOWID1, FLOWBTN1, TR_ID, TR_FLOWID, TR_KIND) VALUES ('1901', 'XR3', '未過帳', '1999', '過帳', 'Y', '1999', '0');
                                            INSERT INTO MMSADM.ME_FLOW (FLOWID, DOCTYPE, FLOWNAME, TR_ID, TR_KIND) VALUES ('1999', 'XR3', '已結案', 'N', '1');
                                        END;
                                    ";
            return sql;
        }

        public string Insert_CHK_MNSET()
        {
            //0.7 MI_MNSET
            string sql = @"
                INSERT INTO MMSADM.CHK_MNSET (CHK_YM,set_ctime,set_status, create_date, create_user ) 
                VALUES (twn_yyymm(sysdate),sysdate,'N',sysdate, '上線轉檔')
            ";
            return sql;
        }

        public string Insert_UR_ID()
        {
            string sql = @"
                begin
                Insert into MMSADM.UR_ID
                  (TUSER, INID, PA, SL, UNA, 
                   UENA, IDDESC, EMAIL, EXT, BOSS, 
                   TITLE, FAX, FL, TEL, CREATE_TIME, 
                   CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, ADUSER, 
                   WHITELIST_IP1, WHITELIST_IP2, WHITELIST_IP3)
                Values
                  ('560000', '560000', 'h6c7Cc84X/3HgLftS/SJyTqjDjk01jjFp9XbLDRiL/fWUd6c', 'h6c7Cc84X/3HgLftS/SJyQ==', '中央庫房管理員', 
                   NULL, NULL, NULL, NULL, NULL, 
                   NULL, NULL, 1, NULL, sysdate, 
                   NULL, sysdate, NULL, NULL, NULL, 
                   NULL, NULL, NULL);
               Insert into MMSADM.UR_ID
                  (TUSER, INID, PA, SL, UNA, 
                   UENA, IDDESC, EMAIL, EXT, BOSS, 
                   TITLE, FAX, FL, TEL, CREATE_TIME, 
                   CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, ADUSER, 
                   WHITELIST_IP1, WHITELIST_IP2, WHITELIST_IP3)
                Values
                  ('A0000429', '560000', 'dIBZlbCQFECnfaMMFFgoFwfO+A4gLY2aO/hJrdMnikXZY3Ph', 'dIBZlbCQFECnfaMMFFgoFw==', '藥庫', 
                   NULL, '衛材補給保養室中央庫房', 'rudolf_shu', '617373', NULL, 
                   NULL, NULL, 1, NULL, sysdate, 
                   NULL, sysdate, 'SYSTEM', 'SYSTEM', '', 
                   NULL, NULL, NULL);
               Insert into MMSADM.UR_ID
                  (TUSER, INID, PA, SL, UNA, 
                   UENA, IDDESC, EMAIL, EXT, BOSS, 
                   TITLE, FAX, FL, TEL, CREATE_TIME, 
                   CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, ADUSER, 
                   WHITELIST_IP1, WHITELIST_IP2, WHITELIST_IP3)
                Values
                  ('A0002704', '320000', '2RBPhvQKcS0lwYN80rvmQp1VSbpc7pyEomowfwXxxNo+TUkK', '2RBPhvQKcS0lwYN80rvmQg==', '藥局', 
                   NULL, '臨床藥學部', 'phr98', '17303', NULL, 
                   NULL, NULL, 1, NULL, sysdate, 
                   NULL, sysdate, 'system', 'system', '', 
                   NULL, NULL, NULL);
               Insert into MMSADM.UR_ID
                  (TUSER, INID, PA, SL, UNA, 
                   UENA, IDDESC, EMAIL, EXT, BOSS, 
                   TITLE, FAX, FL, TEL, CREATE_TIME, 
                   CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, ADUSER, 
                   WHITELIST_IP1, WHITELIST_IP2, WHITELIST_IP3)
                Values
                  ('NRS00001', '331600', '5U4ChcSqPpVtdG5Ux85buvyc9lG0RXI/nl6upKS5gVkIRvJZ', '5U4ChcSqPpVtdG5Ux85bug==', '病房1', 
                   NULL, NULL, NULL, NULL, NULL, 
                   NULL, NULL, 1, NULL, sysdate, 
                   NULL, sysdate, 'system', '10.200.4.249', NULL, 
                   NULL, NULL, NULL);
               Insert into MMSADM.UR_ID
                  (TUSER, INID, PA, SL, UNA, 
                   UENA, IDDESC, EMAIL, EXT, BOSS, 
                   TITLE, FAX, FL, TEL, CREATE_TIME, 
                   CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, ADUSER, 
                   WHITELIST_IP1, WHITELIST_IP2, WHITELIST_IP3)
                Values
                  ('NRS00002', '331700', 'X69+wm3fyCKcwXYBw5ZE9xIRuUodDfUP8CHQpBEfO3ZVBEuc', 'X69+wm3fyCKcwXYBw5ZE9w==', '病房2', 
                   NULL, NULL, NULL, NULL, NULL, 
                   NULL, NULL, 1, NULL, sysdate, 
                   NULL, sysdate, 'SYSTEM', '系統停用', NULL, 
                   NULL, NULL, NULL);
               Insert into MMSADM.UR_ID
                  (TUSER, INID, PA, SL, UNA, 
                   UENA, IDDESC, EMAIL, EXT, BOSS, 
                   TITLE, FAX, FL, TEL, CREATE_TIME, 
                   CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, ADUSER, 
                   WHITELIST_IP1, WHITELIST_IP2, WHITELIST_IP3)
                Values
                  ('admin', '560000', '0JYQrPI/9WBDuIT2fuMZ9Z7A3yBYLytvxuaclbm9f7x9BAlv', '0JYQrPI/9WBDuIT2fuMZ9Q==', '系統管理者', 
                   'administrator', '666666', NULL, '1234', NULL, 
                   NULL, NULL, 1, NULL, sysdate, 
                   NULL, sysdate, 'admin', '192.20.2.111', NULL, 
                   '::1', NULL, NULL);
               Insert into MMSADM.UR_ID
                  (TUSER, INID, PA, SL, UNA, 
                   UENA, IDDESC, EMAIL, EXT, BOSS, 
                   TITLE, FAX, FL, TEL, CREATE_TIME, 
                   CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, ADUSER, 
                   WHITELIST_IP1, WHITELIST_IP2, WHITELIST_IP3)
                Values
                  ('admin_14', '560000', 'da/vBpvuD7U1G+wFSKcP4C7L/rPk64nGE87O2yGch6T/aB31', 'da/vBpvuD7U1G+wFSKcP4A==', '系統管理者(國軍)', 
                   'administrator', NULL, NULL, NULL, NULL, 
                   NULL, NULL, 1, NULL, sysdate, 
                   NULL, sysdate, 'admin', '::1', NULL, 
                   NULL, NULL, NULL);

                end;
            ";
            return sql;
        }

        public string Insert_UR_ROLE()
        {
            string sql = @"
                begin
                  Insert into MMSADM.UR_ROLE
                     (RLNO, RLNA, RLDESC, ROLE_CREATE_DATE, ROLE_CREATE_BY, 
                      ROLE_MODIFY_DATE, ROLE_MODIFY_BY)
                   Values
                     ('PHR_14', '國軍藥局管理者', NULL, sysdate, 'admin', 
                      NULL, NULL);
                  Insert into MMSADM.UR_ROLE
                     (RLNO, RLNA, RLDESC, ROLE_CREATE_DATE, ROLE_CREATE_BY, 
                      ROLE_MODIFY_DATE, ROLE_MODIFY_BY)
                   Values
                     ('MAT_14', '國軍衛材人員', NULL, sysdate, 'admin', 
                      NULL, NULL);
                  Insert into MMSADM.UR_ROLE
                     (RLNO, RLNA, RLDESC, ROLE_CREATE_DATE, ROLE_CREATE_BY, 
                      ROLE_MODIFY_DATE, ROLE_MODIFY_BY)
                   Values
                     ('MED_14', '國軍藥庫人員', NULL, sysdate, 'admin', 
                      NULL, NULL);
                  Insert into MMSADM.UR_ROLE
                     (RLNO, RLNA, RLDESC, ROLE_CREATE_DATE, ROLE_CREATE_BY, 
                      ROLE_MODIFY_DATE, ROLE_MODIFY_BY)
                   Values
                     ('MMSpl_14', '國軍衛保室', NULL, sysdate, 'admin', 
                      NULL, NULL);
                  Insert into MMSADM.UR_ROLE
                     (RLNO, RLNA, RLDESC, ROLE_CREATE_DATE, ROLE_CREATE_BY, 
                      ROLE_MODIFY_DATE, ROLE_MODIFY_BY)
                   Values
                     ('NRS_14', '國軍病房/衛星庫房', NULL, sysdate, 'admin', 
                      NULL, NULL);
                  Insert into MMSADM.UR_ROLE
                     (RLNO, RLNA, RLDESC, ROLE_CREATE_DATE, ROLE_CREATE_BY, 
                      ROLE_MODIFY_DATE, ROLE_MODIFY_BY)
                   Values
                     ('ADMG_14', '國軍管理者', NULL, sysdate, 'admin', 
                      NULL, NULL);
                  Insert into MMSADM.UR_ROLE
                     (RLNO, RLNA, RLDESC, ROLE_CREATE_DATE, ROLE_CREATE_BY, 
                      ROLE_MODIFY_DATE, ROLE_MODIFY_BY)
                   Values
                     ('ADMG', 'Admin', '系統管理群組', sysdate, 'admin', 
                      NULL, NULL);
                end;
            ";
            return sql;
        }

        public string Insert_UR_UIR()
        {
            string sql = @"
                begin
                    Insert into MMSADM.UR_UIR
                      (RLNO, TUSER, UIR_CREATE_DATE, UIR_CREATE_BY, UIR_MODIFY_DATE, UIR_MODIFY_BY)
                    Values
                      ('ADMG', 'admin', sysdate, NULL, NULL, NULL);
                   Insert into MMSADM.UR_UIR
                      (RLNO, TUSER, UIR_CREATE_DATE, UIR_CREATE_BY, UIR_MODIFY_DATE, UIR_MODIFY_BY)
                    Values
                      ('VIEWALL', 'admin', sysdate, 'admin', NULL, NULL);
                   Insert into MMSADM.UR_UIR
                      (RLNO, TUSER, UIR_CREATE_DATE, UIR_CREATE_BY, UIR_MODIFY_DATE,  UIR_MODIFY_BY)
                    Values
                      ('ADMG_14', 'admin_14', sysdate, 'admin', NULL, NULL);
                   Insert into MMSADM.UR_UIR
                      (RLNO, TUSER, UIR_CREATE_DATE, UIR_CREATE_BY, UIR_MODIFY_DATE, UIR_MODIFY_BY)
                    Values
                      ('PHR_14', 'A0002704', sysdate, 'admin_14', NULL, NULL);
                   Insert into MMSADM.UR_UIR
                      (RLNO, TUSER, UIR_CREATE_DATE, UIR_CREATE_BY, UIR_MODIFY_DATE, UIR_MODIFY_BY)
                    Values
                      ('NRS_14', 'NRS00001', sysdate, 'admin_14', NULL, NULL);
                   Insert into MMSADM.UR_UIR
                      (RLNO, TUSER, UIR_CREATE_DATE, UIR_CREATE_BY, UIR_MODIFY_DATE, UIR_MODIFY_BY)
                    Values
                      ('MED_14', 'A0000429', sysdate, 'admin_14', NULL, NULL);
                   Insert into MMSADM.UR_UIR
                      (RLNO, TUSER, UIR_CREATE_DATE, UIR_CREATE_BY, UIR_MODIFY_DATE, UIR_MODIFY_BY)
                    Values
                      ('MAT_14', '560000', sysdate, 'admin_14', NULL, NULL);
                   Insert into MMSADM.UR_UIR
                      (RLNO, TUSER, UIR_CREATE_DATE, UIR_CREATE_BY, UIR_MODIFY_DATE, UIR_MODIFY_BY)
                    Values
                      ('NRS_14', 'NRS00002', sysdate, 'admin_14', NULL, NULL);
                end;
            ";
            return sql;
        }

        //
        
        public string Insert_MI_UNITCODE()
        {
            //1 MI_UNITCODE
            string sql = @"
                                        INSERT INTO MMSADM.MI_UNITCODE
                                                    (UNIT_CODE,
                                                     UI_CHANAME,
                                                     UI_ENGNAME,
                                                     UI_SNAME,
                                                     CREATE_TIME,
                                                     CREATE_USER)
                                        VALUES     (:UNIT_CODE,
                                                    :UI_CHANAME,
                                                    :UI_ENGNAME,
                                                    :UI_SNAME,
                                                    SYSDATE,
                                                    '上線轉檔') 
                                    ";
            return sql;
        }

        public string Insert_PH_VENDER()
        {
            //2 PH_VENDER
            string sql = @"
                                        INSERT INTO PH_VENDER
                                                    (AGEN_NO,
                                                     AGEN_NAMEC,
                                                     AGEN_ADD,
                                                     AGEN_FAX,
                                                     AGEN_TEL,
                                                     AGEN_ACC,
                                                     UNI_NO,
                                                     AGEN_BOSS,
                                                     EMAIL,
                                                     EMAIL_1,
                                                     AGEN_BANK,
                                                     AGEN_SUB,
                                                     CREATE_TIME,
                                                     CREATE_USER,
                                                     EASYNAME,
                                                     AGEN_BANK_14)
                                        VALUES      (:AGEN_NO,
                                                     :AGEN_NAMEC,
                                                     :AGEN_ADD,
                                                     :AGEN_FAX,
                                                     :AGEN_TEL,
                                                     :AGEN_ACC,
                                                     :UNI_NO,
                                                     :AGEN_BOSS,
                                                     :EMAIL,
                                                     :EMAIL_1,
                                                     :AGEN_BANK,
                                                     :AGEN_SUB,
                                                     SYSDATE,
                                                     '上線轉檔',
                                                     :EASYNAME,
                                                     :AGEN_BANK_14)
                                    ";
            return sql;
        }

        public string Insert_MI_UNITEXCH()
        {
            //3 MI_UNITEXCH
            string sql = @"
                                        INSERT INTO MMSADM.MI_UNITEXCH
                                                    (MMCODE,
                                                     UNIT_CODE,
                                                     AGEN_NO,
                                                     EXCH_RATIO,
                                                     CREATE_TIME,
                                                     CREATE_USER)
                                        VALUES      (:MMCODE,
                                                     :UNIT_CODE,
                                                     :AGEN_NO,
                                                     :EXCH_RATIO,
                                                     SYSDATE,
                                                     '上線轉檔')
                                    ";
            return sql;
        }

        public string Insert_UR_INID()
        {
            //4 UR_INID
            string sql = @"
                                        INSERT INTO UR_INID
                                                    (INID,
                                                     INID_NAME,
                                                     CREATE_TIME,
                                                     CREATE_USER)
                                        VALUES      (:INID,
                                                     :INID_NAME,
                                                     SYSDATE,
                                                     '上線轉檔')
                                    ";
            return sql;
        }

        public string Insert_MI_WHMAST(string hosp_id)
        {
            //5 MI_WHMAST 803 各家醫院以人工判斷
            string sql = "";
            switch (hosp_id)
            {
                case "803":
                    sql = string.Format(@"    
                                BEGIN
                                    {0}
                                  END;
                    ", GetWhno803());

                    break;

                case "804":
                    sql = string.Format(@"
                                    BEGIN
                                        {0}
                                    END;
                                    ", GetWhno804());
                    break;

                case "805":
                    sql = string.Format(@"
                                    BEGIN
                                        {0}
                                    END;
                                    ", GetWhno805());
                    break;

                case "811":
                    sql = string.Format(@"
                                    BEGIN
                                        {0}
                                    END;
                                    ", GetWhno811());
                    break;
                case "812":
                    sql = string.Format(@"
                        begin
                            {0}
                        end;
                    ", GetWhno812());
                    break;
                case "807":
                    sql = string.Format(@"
                        begin
                            {0}
                        end;
                    ", GetWhno807());
                    break;
                case "813":
                    sql = string.Format(@"
                        begin
                           {0}
                        end;
                    ", GetWhno813());
                    break;
                case "818":
                    sql = string.Format(@"
                        begin
                            {0}
                        end;
                    ", GetWhno818());
                    break;
                default:
                    break;
            }

            return sql;
        }

        public string Insert_MI_MAST()
        {
            //6 MI_MAST
            string sql = @"
                           Insert into MMSADM.MI_MAST(
                                MMCODE,
                                MMNAME_C,
                                MMNAME_E,
                                MAT_CLASS,
                                BASE_UNIT,
                                AUTO_APLID,
                                M_STOREID,
                                M_CONTID,
                                M_NHIKEY,
                                M_MATID,
                                M_PAYID,
                                M_TRNID,
                                M_APPLYID,
                                M_PHCTNCO,
                                M_ENVDT,
                                M_AGENNO,
                                M_AGENLAB,
                                M_PURUN,
                                M_CONTPRICE,
                                M_DISCPERC,
                                E_SUPSTATUS,
                                E_MANUFACT,
                                E_ITEMARMYNO,
                                E_GPARMYNO,
                                E_CODATE,
                                E_PRESCRIPTYPE,
                                E_DRUGCLASSIFY,
                                E_INVFLAG,
                                E_SOURCECODE,
                                E_DRUGAPLTYPE,
                                E_PARCODE,
                                WEXP_ID,
                                WLOC_ID,
                                CANCEL_ID,
                                E_RESTRICTCODE,
                                E_ORDERDCFLAG,
                                E_HIGHPRICEFLAG,
                                E_RETURNDRUGFLAG,
                                E_RESEARCHDRUGFLAG,
                                CREATE_TIME,
                                CREATE_USER,
                                UPDATE_TIME,
                                UPRICE,
                                DISC_CPRICE,
                                DISC_UPRICE,
                                PACKTYPE,
                                NHI_PRICE,
                                MAT_CLASS_SUB,
                                DRUGSNAME,
                                DRUGHIDE,
                                HEALTHPAY,
                                COSTKIND,
                                CASEDOCT,
                                DRUGKIND,
                                HEALTHOWNEXP,
                                ISSUESUPPLY ,
                                UNITRATE,
                                SPXFEE,
                                WARBAK,
                                ORDERKIND,
                                WASTKIND,
                                CASENO,
                                TOUCHCASE,
                                CONTRACTAMT,
                                CONTRACTSUM,
                                COMMON,
                                TRUTRATE,
                                ISSPRICEDATE,
                                BEGINDATE_14,
                                ONECOST,
                                SPDRUG,
                                FASTDRUG)
                                VALUES (
                                :MMCODE,
                                :MMNAME_C,
                                :MMNAME_E,
                                :MAT_CLASS,
                                :BASE_UNIT,
                                :AUTO_APLID,
                                :M_STOREID,
                                :M_CONTID,
                                :M_NHIKEY,
                                :M_MATID,
                                :M_PAYID,
                                :M_TRNID,
                                :M_APPLYID,
                                :M_PHCTNCO,
                                :M_ENVDT,
                                :M_AGENNO,
                                :M_AGENLAB,
                                :M_PURUN,
                                :M_CONTPRICE,
                                :M_DISCPERC,
                                :E_SUPSTATUS,
                                :E_MANUFACT,
                                :E_ITEMARMYNO,
                                :E_GPARMYNO,
                                TO_DATE(:E_CODATE, 'YYYYMMDD'),
                                :E_PRESCRIPTYPE,
                                :E_DRUGCLASSIFY,
                                :E_INVFLAG,
                                :E_SOURCECODE,
                                :E_DRUGAPLTYPE,
                                :E_PARCODE,
                                :WEXP_ID,
                                :WLOC_ID,
                                :CANCEL_ID,
                                :E_RESTRICTCODE,
                                :E_ORDERDCFLAG,
                                :E_HIGHPRICEFLAG,
                                :E_RETURNDRUGFLAG,
                                :E_RESEARCHDRUGFLAG,
                                SYSDATE,
                                '上線轉檔',
                                TO_DATE(:UPDATE_TIME, 'YYYYMMDD'),
                                :UPRICE,
                                :DISC_CPRICE,
                                :DISC_UPRICE,
                                :PACKTYPE,
                                :NHI_PRICE,
                                :MAT_CLASS_SUB,
                                :DRUGSNAME,
                                :DRUGHIDE,
                                :HEALTHPAY,
                                :COSTKIND,
                                :CASEDOCT,
                                :DRUGKIND,
                                :HEALTHOWNEXP,
                                :ISSUESUPPLY ,
                                :UNITRATE,
                                :SPXFEE,
                                :WARBAK,
                                :ORDERKIND,
                                :WASTKIND,
                                :CASENO,
                                :TOUCHCASE,
                                :CONTRACTAMT,
                                :CONTRACTSUM,
                                :COMMON,
                                :TRUTRATE,
                                TO_DATE(:ISSPRICEDATE, 'YYYYMMDD'),
                                TO_DATE(:BEGINDATE_14, 'YYYYMMDD'),
                                :ONECOST,
                                :SPDRUG,
                                :FASTDRUG)
                                    ";
            return sql;
        }
        public string Insert_MI_WHINV()
        {
            //7 MI_WHINV
            string sql = @"
                                        INSERT INTO MMSADM.MI_WHINV
                                                    (WH_NO,
                                                     MMCODE,
                                                     INV_QTY)
                                        VALUES      (:WH_NO,
                                                     :MMCODE,
                                                     :INV_QTY)
                                    ";
            return sql;
        }

        public string Insert_MI_WINVCTL()
        {
            //8 MI_WINVCTL
            /* insert mi_mast時,衛材會以trigger insert mi_winvctl，所以不用轉衛材資料 */
            string sql = @"
                                        INSERT INTO MMSADM.MI_WINVCTL
                                                    (WH_NO,
                                                     MMCODE,
                                                     SUPPLY_WHNO,
                                                     CREATE_TIME,
                                                     CREATE_USER)
                                        VALUES      (:WH_NO,
                                                     :MMCODE,
                                                     WHNO_ME1,
                                                     SYSDATE,
                                                     '上線轉檔')
                                    ";
            return sql;
        }

        public string Insert_BC_BARCODE()
        {
            //9 BC_BARCODE
            string sql = @"
                                        INSERT INTO MMSADM.BC_BARCODE
                                                    (MMCODE,
                                                     BARCODE,
                                                     CREATE_TIME,
                                                     CREATE_USER)
                                        VALUES      (:MMCODE,
                                                     :BARCODE,
                                                     SYSDATE,
                                                     '上線轉檔')
                                    ";
            return sql;
        }

        public string Insert_MI_WHID()
        {
            string sql = @"
                begin
                 insert into MI_WHID (wh_no, wh_userid, task_id, create_time)
                  select a.wh_no, '560000', '2', sysdate
                    from MI_WHMAST a
                   where wh_kind = '1' and wh_grade = '1';
                 insert into MI_WHID (wh_no, wh_userid, task_id, create_time)
                  select a.wh_no, 'A0000429', '1', sysdate
                    from MI_WHMAST a
                   where wh_kind = '0' and wh_grade = '1';         
                 insert into MI_WHID (wh_no, wh_userid, task_id, create_time)
                  select a.wh_no, 'A0002704','1', sysdate
                    from MI_WHMAST a
                   where wh_kind = '0' and wh_grade = '2';               
                end;
            ";
            return sql;
        }

        public string Insert_MI_WLOCINV()
        {
            string sql = @"
                insert into MMSADM.MI_WLOCINV(WH_NO, MMCODE, STORE_LOC, INV_QTY)
                values (:WH_NO, :MMCODE, :STORE_LOC, :INV_QTY)
            ";
            return sql;
        }
        public string Insert_MI_WHCOST_SubQuery1()
        {
            //---- insert 到 MI_WHCOST----
            string sql = @"
                INSERT INTO MMSADM.MI_WHCOST (DATA_YM,
                                              MMCODE,
                                              SET_YM,
                                              PMN_INVQTY,
                                              PMN_AVGPRICE,
                                              MN_INQTY,
                                              CONT_PRICE,
                                              UPRICE,
                                              DISC_CPRICE,
                                              SOURCECODE,
                                              NHI_PRICE)
                     VALUES ( :DATA_YM,
                             :MMCODE,
                             :SET_YM,
                             :PMN_INVQTY,
                             :PMN_AVGPRICE,
                             :MN_INQTY,
                             :CONT_PRICE,
                             :UPRICE,
                             :DISC_CPRICE,
                             :SOURCECODE,
                             :NHI_PRICE)
                ";
            return sql;
        }
        public string Insert_MI_WHCOST_SubQuery2()
        {
            //---- 再insert 一筆MI_WHCOST：DATA_YM=月結的次月 ----
            string sql = @"
                INSERT INTO MMSADM.MI_WHCOST (DATA_YM,
                                              MMCODE,
                                              SET_YM,
                                              PMN_INVQTY,
                                              PMN_AVGPRICE,
                                              MN_INQTY,
                                              CONT_PRICE,
                                              UPRICE,
                                              DISC_CPRICE,
                                              SOURCECODE,
                                              NHI_PRICE)
                   SELECT twn_yyymm (ADD_MONTHS (twn_todate (DATA_YM || '01'), 1)) AS DATA_YM,
                          MMCODE,
                          SET_YM,
                          PMN_INVQTY,
                          PMN_AVGPRICE,
                          MN_INQTY,
                          CONT_PRICE,
                          UPRICE,
                          DISC_CPRICE,
                          SOURCECODE,
                          NHI_PRICE
                     FROM MMSADM.MI_WHCOST
                ";
            return sql;
        }
        public string Insert_MI_WEXPINV()
        {
            string sql = @"
                insert into MMSADM.MI_WEXPINV(WH_NO, MMCODE, EXP_DATE, LOT_NO, INV_QTY)
                values (:WH_NO, :MMCODE, twn_todate(:EXP_DATE), :LOT_NO, :INV_QTY)
            ";
            return sql;
        }
        public string Insert_MI_WINVMON()
        {
            string sql = @"
                insert into MMSADM.MI_WINVMON(WH_NO, MMCODE, DATA_YM, INV_QTY)
                values (:WH_NO, :MMCODE, :DATA_YM, :INV_QTY)
            ";
            return sql;
        }
        public string Insert_MI_MAST_HISTORY()
        {
            string sql = @"
                Insert into MI_MAST_HISTORY(
                  MIMASTHIS_SEQ, MMCODE, EffStartDate, CANCEL_ID, E_ORDERDCFLAG, 
                  M_NHIKEY, HealthOwnExp, DRUGSNAME, MMNAME_E, MMNAME_C,
                  M_PHCTNCO, M_ENVDT, IssueSupply, E_MANUFACT, BASE_UNIT, 
                  M_PURUN, TrUTRate, MAT_CLASS_SUB, E_RESTRICTCODE, WarBak, 
                  OneCost, HealthPay, CostKind, WastKind, SpXfee, 
                  OrderKind, CaseDoct, DrugKind, M_AGENNO, M_AGENLAB, 
                  CaseNo, E_SOURCECODE, M_CONTID, E_ITEMARMYNO, NHI_PRICE, 
                  DISC_CPRICE, M_CONTPRICE, E_CODATE, ContractAmt, ContractSum, 
                  TouchCase, BEGINDATE_14, IssPriceDate, SpDrug, FastDrug, 
                  CREATE_TIME, CREATE_USER)
                select
                  MiMastHistory_SEQ.nextval, MMCODE, sysdate, CANCEL_ID, E_ORDERDCFLAG, 
                  M_NHIKEY, HealthOwnExp, DRUGSNAME, MMNAME_E, MMNAME_C, 
                  M_PHCTNCO, M_ENVDT, IssueSupply, E_MANUFACT, BASE_UNIT, 
                  M_PURUN, TrUTRate, MAT_CLASS_SUB, E_RESTRICTCODE, WarBak, 
                  OneCost, HealthPay, CostKind, WastKind, SpXfee, 
                  OrderKind, CaseDoct, DrugKind, M_AGENNO, M_AGENLAB, 
                  CaseNo, E_SOURCECODE, M_CONTID, E_ITEMARMYNO, NHI_PRICE, 
                  DISC_CPRICE, M_CONTPRICE, E_CODATE, ContractAmt, ContractSum, 
                  TouchCase, BEGINDATE_14, sysdate, SpDrug, FastDrug, 
                  sysdate, '系統轉檔'
                from MI_MAST
            ";
            return sql;
        }
        #endregion

        #region DELETE
        public string Delete_MI_MATCLASS()
        {
            //0.1 MI_MATCLASS
            string sql = @" DELETE FROM MMSADM.MI_MATCLASS ";
            return sql;
        }

        public string Delete_MI_MCODE()
        {
            //0.2 MI_MCODE
            string sql = @" DELETE FROM MMSADM.MI_MCODE ";
            return sql;
        }

        public string Delete_PARAM_M()
        {
            //0.3 PARAM_M
            string sql = @" DELETE FROM MMSADM.PARAM_M ";
            return sql;
        }

        public string Delete_PARAM_D()
        {
            //0.4 PARAM_D
            string sql = @" DELETE FROM MMSADM.PARAM_D ";
            return sql;
        }

        public string Delete_MI_DOCTYPE()
        {
            //0.5 MI_DOCTYPE
            string sql = @" DELETE FROM MMSADM.MI_DOCTYPE ";
            return sql;
        }

        public string Delete_BASEUNITCNV()
        {
            //0.6 BASEUNITCNV
            string sql = @" DELETE FROM MMSADM.BASEUNITCNV ";
            return sql;
        }

        public string Delete_MI_MNSET()
        {
            //0.7 MI_MNSET
            string sql = @" DELETE FROM MMSADM.MI_MNSET ";
            return sql;
        }

        public string Delete_ME_FLOW()
        {
            //0.8 ME_FLOW
            string sql = @" DELETE FROM MMSADM.ME_FLOW ";
            return sql;
        }

        public string Delete_CHK_MNSET()
        {
            //0.9 ME_FLOW
            string sql = @" DELETE FROM MMSADM.CHK_MNSET ";
            return sql;
        }

        public string Delete_UR_ID()
        {
            //0.10 ME_FLOW
            string sql = @" DELETE FROM MMSADM.UR_ID 
                             where tuser in ('admin','admin_14','560000','A0000429','A0002704','NRS00001','NRS00002') ";
            return sql;
        }

        public string Delete_UR_ROLE()
        {
            //0.11 ME_FLOW
            string sql = @" DELETE FROM MMSADM.UR_ROLE ";
            return sql;
        }
        public string Delete_UR_UIR()
        {
            string sql = @"
                delete from MMSADM.UR_UIR
                 where TUSER in ('admin','admin_14','560000','A0000429','A0002704','NRS00001','NRS00002')
            ";
            return sql;
        }

        public string Delete_MI_WHID()
        {
            string sql = @"
                delete from MMSADM.MI_WHID
                 where wh_userid in ('admin','admin_14','560000','A0000429','A0002704','NRS00001','NRS00002')
            ";
            return sql;
        }

        public string Delete_MI_UNITCODE()
        {
            //1 MI_UNITCODE
            string sql = @" DELETE FROM MMSADM.MI_UNITCODE ";
            return sql;
        }

        public string Delete_PH_VENDER()
        {
            //2 PH_VENDER
            string sql = @" DELETE FROM MMSADM.PH_VENDER ";
            return sql;
        }

        public string Delete_MI_UNITEXCH()
        {
            //3 MI_UNITEXCH
            string sql = @" DELETE FROM MMSADM.MI_UNITEXCH ";
            return sql;
        }

        public string Delete_UR_INID()
        {
            //4 UR_INID
            string sql = @" DELETE FROM MMSADM.UR_INID ";
            return sql;
        }

        public string Delete_MI_WHMAST()
        {
            //5 MI_WHMAST
            string sql = @" DELETE FROM MMSADM.MI_WHMAST ";
            return sql;
        }

        public string Delete_MI_MAST()
        {
            //6 MI_MAST
            string sql = @" DELETE FROM MMSADM.MI_MAST ";
            return sql;
        }

        public string Delete_MI_WHINV()
        {
            //7 MI_WHINV
            string sql = @" DELETE FROM MMSADM.MI_WHINV ";
            return sql;
        }

        public string Delete_MI_WINVCTL()
        {
            //8 MI_WINVCTL
            string sql = @" DELETE FROM MMSADM.MI_WINVCTL ";
            return sql;
        }

        public string Delete_BC_BARCODE()
        {
            //9 BC_BARCODE
            string sql = @" DELETE FROM MMSADM.BC_BARCODE ";
            return sql;
        }

        public string Delete_ME_DOCM()
        {
            string sql = @" DELETE FROM MMSADM.ME_DOCM ";
            return sql;
        }

        public string Delete_ME_DOCD()
        {
            string sql = @" DELETE FROM MMSADM.ME_DOCD ";
            return sql;
        }

        public string Delete_ME_DOCI()
        {
            string sql = @" DELETE FROM MMSADM.ME_DOCI ";
            return sql;
        }

        public string Delete_MI_WEXPINV()
        {
            string sql = @" DELETE FROM MMSADM.MI_WEXPINV ";
            return sql;
        }

        public string Delete_MI_WHTRNS()
        {
            string sql = @" DELETE FROM MMSADM.MI_WHTRNS ";
            return sql;
        }
        public string Delete_MI_WLOCINV()
        {
            string sql = @" DELETE FROM MMSADM.MI_WLOCINV ";
            return sql;
        }
        public string Delete_MI_WHCOST()
        {
            string sql = @" DELETE FROM MMSADM.MI_WHCOST ";
            return sql;
        }
        public string Delete_MI_WINVMON()
        {
            string sql = @" DELETE FROM MMSADM.MI_WINVMON ";
            return sql;
        }
        public string Delete_MI_MAST_HISTORY()
        {
            string sql = @" DELETE FROM MMSADM.MI_MAST_HISTORY ";
            return sql;
        }
        #endregion

        #region update

        public string Update_UR_ID()
        {
            string sql = @"
                begin
                    update MMSADM.UR_ID 
                       set inid = (select inid from MI_WHMAST where wh_grade = '1' and wh_kind = '1' and rownum = 1)
                     where tuser in ('admin','admin_14','560000', 'A0000429');
                end;
            ";
            return sql;
        }

        public string Update_MI_MAST()
        {
            string sql = @"
                begin
                    UPDATE MI_MAST a
                    SET a.MIMASTHIS_SEQ = (SELECT b.MIMASTHIS_SEQ FROM MI_MAST_HISTORY b WHERE a.MMCODE = b.MMCODE);
                end;
            ";
            return sql;
        }

        #endregion

        #endregion

        #region GetWhnoSql
        private string GetWhno803()
        {
            return @"
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E60800','衛保組(藥庫)','0','1','','60800','','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E40010','總院藥局','0','2','E60800','40010','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E80000','中清藥局','0','2','E60800','80000','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E70000','成功嶺門診藥局','0','2','E60800','70000','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E70001','成功嶺替代役藥局','0','2','E60800','70001','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E1000','內科部藥品','0','4','E40010','1000','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E10000','家醫科藥品','0','4','E40010','10000','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E1010','腸胃內1科藥品','0','4','E40010','1010','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E1020','腸胃內2科藥品','0','4','E40010','1020','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E1030','心臟內科藥品','0','4','E40010','1030','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E1031','心內特衛1藥品','0','4','E40010','1031','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E1032','心內特衛2藥品','0','4','E40010','1032','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E1033','心導管科藥品','0','4','E40010','1033','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E1040','胸腔內科藥品','0','4','E40010','1040','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E1050','腎臟內科藥品','0','4','E40010','1050','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E1060','神經內科藥品','0','4','E40010','1060','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E1070','血液腫瘤科藥品','0','4','E40010','1070','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E1080','一般內科藥品','0','4','E40010','1080','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E1090','皮膚科藥品','0','4','E40010','1090','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E1100','呼吸治療科藥品','0','4','E40010','1100','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E11000','復健科藥品','0','4','E40010','11000','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E1110','新陳代謝科藥品','0','4','E40010','1110','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E1130','感染科藥品','0','4','E40010','1130','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E1140','風濕免疫科藥品','0','4','E40010','1140','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E12000','放射科藥品','0','4','E40010','12000','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E12010','放射1科藥品','0','4','E40010','12010','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E12020','放射腫瘤科藥品','0','4','E40010','12020','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E12030','核磁共振室藥品','0','4','E40010','12030','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E12040','電腦斷層掃描藥品','0','4','E40010','12040','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E13000','病理科藥品','0','4','E40010','13000','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E14000','檢驗科藥品','0','4','E40010','14000','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E15000','核醫科藥品','0','4','E40010','15000','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E15010','核子免疫藥品','0','4','E40010','15010','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E15020','核子掃描藥品','0','4','E40010','15020','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E16000','麻醉科藥品','0','4','E40010','16000','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E16010','高壓氧科藥品','0','4','E40010','16010','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E17000','體檢中心藥品','0','4','E40010','17000','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E18000','體檢組藥品','0','4','E40010','18000','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E19000','X光組藥品','0','4','E40010','19000','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E2000','外科部藥品','0','4','E40010','2000','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E20000','巡迴醫療組藥品','0','4','E40010','20000','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E2010','泌尿外1科藥品','0','4','E40010','2010','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E2020','泌尿外2科藥品','0','4','E40010','2020','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E2021','震波碎石室藥品','0','4','E40010','2021','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E2022','碎石中心藥品','0','4','E40010','2022','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E2030','消化外科暨乳房外科藥品','0','4','E40010','2030','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E2032','一般外2科藥品','0','4','E40010','2032','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E2040','整形外科藥品','0','4','E40010','2040','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E2042','整形外2科藥品','0','4','E40010','2042','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E2050','神經外科藥品','0','4','E40010','2050','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E2051','神經外1科藥品','0','4','E40010','2051','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E2052','神經外2科藥品','0','4','E40010','2052','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E2060','胸腔外科藥品','0','4','E40010','2060','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E2070','直腸外科藥品','0','4','E40010','2070','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E2080','醫學美容科藥品','0','4','E40010','2080','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E2090','心臟血管外科藥品','0','4','E40010','2090','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E2100','急診外科藥品','0','4','E40010','2100','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E3000','婦產科藥品','0','4','E40010','3000','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E30000','社區醫學部藥品','0','4','E40010','30000','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E30010','社區醫學分部藥品','0','4','E40010','30010','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E30020','居家醫療藥品','0','4','E40010','30020','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E30021','居家護理所藥品','0','4','E40010','30021','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E30024','血庫組藥品','0','4','E40010','30024','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E4000','小兒科藥品','0','4','E40010','4000','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E40030','藥物諮詢室藥品','0','4','E40010','40030','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E40050','藥物毒物監測藥品','0','4','E40010','40050','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E5000','眼科藥品','0','4','E40010','5000','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E50001','一病房藥品','0','3','E40010','50001','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E50002','二病房藥品','0','3','E40010','50002','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E50003','三病房藥品','0','3','E40010','50003','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E50005','五病房藥品','0','3','E40010','50005','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E50006','六病房藥品','0','3','E40010','50006','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E50007','七病房藥品','0','3','E40010','50007','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E50008','八病房藥品','0','3','E40010','50008','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E50009','九病房藥品','0','3','E40010','50009','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E50010','十病房藥品','0','3','E40010','50010','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E50011','十一病房藥品','0','3','E40010','50011','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E50012','十二病房藥品','0','3','E40010','50012','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E50015','總院護理之家藥品','0','3','E40010','50015','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E50016','總院身心病房藥品','0','3','E40010','50016','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E50017','十七病房藥品','0','3','E40010','50017','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E50018','精神護理之家藥品','0','3','E40010','50018','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E50019','二十病房藥品','0','3','E40010','50019','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E50100','加護病房藥品','0','3','E40010','50100','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E50110','產房藥品','0','3','E40010','50110','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E50120','嬰兒房藥品','0','3','E40010','50120','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E50130','燒傷中心藥品','0','3','E40010','50130','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E50140','洗腎中心藥品','0','4','E40010','50140','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E50160','門診室藥品','0','4','E40010','50160','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E50170','急診室藥品','0','4','E40010','50170','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E50180','手術室藥品','0','4','E40010','50180','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E50191','供應中心藥品','0','4','E40010','50191','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E6000','耳鼻喉科藥品','0','4','E40010','6000','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E60701','住服中心藥品','0','4','E40010','60701','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E7000','牙科藥品','0','4','E70000','7000','70000','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E7010','牙科技工藥品','0','4','E70000','7010','70000','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E7020','牙科牙材藥品','0','4','E70000','7020','70000','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E8000','骨科藥品','0','4','E80000','8000','80000','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E8010','孫開來科藥品','0','4','E80000','8010','80000','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E80100','中清骨科藥品','0','4','E80000','80100','80000','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E80102','中清腸胃藥品','0','4','E80000','80102','80000','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E8020','骨科特衛藥品','0','4','E80000','8020','80000','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E80205','中清神外藥品','0','4','E80000','80205','80000','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E80300','中清婦產藥品','0','4','E80000','80300','80000','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E80400','中清小兒科藥品','0','4','E80000','80400','80000','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E80500','中清眼科藥品','0','4','E80000','80500','80000','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E80700','中清牙科藥品','0','4','E80000','80700','80000','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E81000','中清家醫藥品','0','4','E80000','81000','80000','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E81040','中清胸內藥品','0','4','E80000','81040','80000','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E81050','中清洗腎藥品','0','4','E80000','81050','80000','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E81100','中清復健藥品','0','4','E80000','81100','80000','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E81201','中清放射藥品','0','4','E80000','81201','80000','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E81600','中清麻醉藥品','0','4','E80000','81600','80000','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E81900','中清衛保藥品','0','4','E80000','81900','80000','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E81910','中清戰備動員藥品','0','4','E80000','81910','80000','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E82000','中清檢驗藥品','0','4','E80000','82000','80000','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E82020','中清泌外藥品','0','4','E80000','82020','80000','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E82060','中清胸外藥品','0','4','E80000','82060','80000','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E82070','中清直外藥品','0','4','E80000','82070','80000','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E82090','中清心外藥品','0','4','E80000','82090','80000','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E85002','中清五病房藥品','0','4','E80000','85002','80000','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E85160','中清門診藥品','0','4','E80000','85160','80000','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E85170','中清急診藥品','0','4','E80000','85170','80000','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E85180','中清開刀房藥品','0','4','E80000','85180','80000','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E86000','中清耳鼻喉科藥品','0','4','E80000','86000','80000','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E86021','中清身心藥品','0','4','E80000','86021','80000','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E86040','中清體檢藥品','0','4','E80000','86040','80000','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E86060','中清護理之家藥品','0','4','E80000','86060','80000','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E9000','精神科藥品','0','4','E40010','9000','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E91001','總院中醫科藥品','0','4','E40010','91001','40010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E91002','中清中醫科藥品','0','4','E80000','91002','80000','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('60800','衛保組(衛材庫)','1','1','','60800','','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('40020','總院藥局衛材','1','2','60800','40020','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('80000','中清藥局衛材','1','2','60800','80000','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('70002','成功嶺衛材','1','2','60800','70002','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('70003','替代役衛材','1','2','60800','70003','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('1000','內科部衛材','1','2','60800','1000','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('10000','家醫科衛材','1','2','60800','10000','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('1010','腸胃內1科衛材','1','2','60800','1010','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('1020','腸胃內2科衛材','1','2','60800','1020','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('1030','心臟內科衛材','1','2','60800','1030','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('1031','心內特衛1衛材','1','2','60800','1031','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('1032','心內特衛2衛材','1','2','60800','1032','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('1033','心導管科衛材','1','2','60800','1033','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('1040','胸腔內科衛材','1','2','60800','1040','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('1050','腎臟內科衛材','1','2','60800','1050','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('1060','神經內科衛材','1','2','60800','1060','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('1070','血液腫瘤科衛材','1','2','60800','1070','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('1080','一般內科衛材','1','2','60800','1080','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('1090','皮膚科衛材','1','2','60800','1090','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('1100','呼吸治療科衛材','1','2','60800','1100','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('11000','復健科衛材','1','2','60800','11000','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('1110','新陳代謝科衛材','1','2','60800','1110','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('1130','感染科衛材','1','2','60800','1130','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('1140','風濕免疫科衛材','1','2','60800','1140','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('12000','放射科衛材','1','2','60800','12000','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('12010','放射1科衛材','1','2','60800','12010','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('12020','放射腫瘤科衛材','1','2','60800','12020','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('12030','核磁共振室衛材','1','2','60800','12030','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('12040','電腦斷層掃描衛材','1','2','60800','12040','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('13000','病理科衛材','1','2','60800','13000','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('14000','檢驗科衛材','1','2','60800','14000','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('15000','核醫科衛材','1','2','60800','15000','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('15010','核子免疫衛材','1','2','60800','15010','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('15020','核子掃描衛材','1','2','60800','15020','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('16000','麻醉科衛材','1','2','60800','16000','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('16010','高壓氧科衛材','1','2','60800','16010','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('17000','體檢中心衛材','1','2','60800','17000','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('18000','體檢組衛材','1','2','60800','18000','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('19000','X光組衛材','1','2','60800','19000','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('2000','外科部衛材','1','2','60800','2000','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('20000','巡迴醫療組衛材','1','2','60800','20000','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('2010','泌尿外1科衛材','1','2','60800','2010','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('2020','泌尿外2科衛材','1','2','60800','2020','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('2021','震波碎石室衛材','1','2','60800','2021','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('2022','碎石中心衛材','1','2','60800','2022','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('2030','消化外科暨乳房外科衛材','1','2','60800','2030','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('2032','一般外2科衛材','1','2','60800','2032','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('2040','整形外科衛材','1','2','60800','2040','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('2042','整形外2科衛材','1','2','60800','2042','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('2050','神經外科衛材','1','2','60800','2050','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('2051','神經外1科衛材','1','2','60800','2051','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('2052','神經外2科衛材','1','2','60800','2052','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('2060','胸腔外科衛材','1','2','60800','2060','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('2070','直腸外科衛材','1','2','60800','2070','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('2080','醫學美容科衛材','1','2','60800','2080','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('2090','心臟血管外科衛材','1','2','60800','2090','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('2100','急診外科衛材','1','2','60800','2100','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('3000','婦產科衛材','1','2','60800','3000','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('30000','社區醫學部衛材','1','2','60800','30000','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('30010','社區醫學分部衛材','1','2','60800','30010','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('30020','居家醫療衛材','1','2','60800','30020','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('30021','居家護理所衛材','1','2','60800','30021','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('30024','血庫組衛材','1','2','60800','30024','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('4000','小兒科衛材','1','2','60800','4000','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('40030','藥物諮詢室衛材','1','2','60800','40030','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('40050','藥物毒物監測衛材','1','2','60800','40050','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('5000','眼科衛材','1','2','60800','5000','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('50001','一病房衛材','1','2','60800','50001','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('50002','二病房衛材','1','2','60800','50002','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('50003','三病房衛材','1','2','60800','50003','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('50005','五病房衛材','1','2','60800','50005','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('50006','六病房衛材','1','2','60800','50006','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('50007','七病房衛材','1','2','60800','50007','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('50008','八病房衛材','1','2','60800','50008','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('50009','九病房衛材','1','2','60800','50009','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('50010','十病房衛材','1','2','60800','50010','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('50011','十一病房衛材','1','2','60800','50011','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('50012','十二病房衛材','1','2','60800','50012','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('50015','總院護理之家衛材','1','2','60800','50015','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('50016','總院身心病房衛材','1','2','60800','50016','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('50017','十七病房衛材','1','2','60800','50017','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('50018','精神護理之家衛材','1','2','60800','50018','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('50019','二十病房衛材','1','2','60800','50019','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('50100','加護病房衛材','1','2','60800','50100','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('50110','產  房衛材','1','2','60800','50110','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('50120','嬰兒房衛材','1','2','60800','50120','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('50130','燒傷中心衛材','1','2','60800','50130','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('50140','洗腎中心衛材','1','2','60800','50140','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('50160','門診室衛材','1','2','60800','50160','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('50170','急診室衛材','1','2','60800','50170','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('50180','手術室衛材','1','2','60800','50180','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('50190','衛供庫衛材','1','2','60800','50190','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('50191','供應中心衛材','1','2','60800','50191','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('6000','耳鼻喉科衛材','1','2','60800','6000','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('60701','住服中心衛材','1','2','60800','60701','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('7000','牙科衛材','1','2','60800','7000','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('7010','牙科技工衛材','1','2','60800','7010','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('7020','牙科牙材衛材','1','2','60800','7020','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('8000','骨科衛材','1','2','60800','8000','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('8010','孫開來科衛材','1','2','60800','8010','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('80100','中清骨科衛材','1','2','60800','80100','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('80102','中清腸胃衛材','1','2','60800','80102','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('8020','骨科特衛衛材','1','2','60800','8020','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('80205','中清神外衛材','1','2','60800','80205','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('80300','中清婦產衛材','1','2','60800','80300','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('80400','中清小兒科衛材','1','2','60800','80400','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('80500','中清眼科衛材','1','2','60800','80500','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('80700','中清牙科衛材','1','2','60800','80700','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('81000','中清家醫衛材','1','2','60800','81000','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('81040','中清胸內衛材','1','2','60800','81040','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('81050','中清洗腎衛材','1','2','60800','81050','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('81100','中清復健衛材','1','2','60800','81100','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('81201','中清放射衛材','1','2','60800','81201','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('81600','中清麻醉衛材','1','2','60800','81600','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('81900','中清衛保衛材','1','2','60800','81900','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('81910','中清戰備動員衛材','1','2','60800','81910','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('82000','中清檢驗衛材','1','2','60800','82000','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('82020','中清泌外衛材','1','2','60800','82020','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('82060','中清胸外衛材','1','2','60800','82060','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('82070','中清直外衛材','1','2','60800','82070','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('82090','中清心外衛材','1','2','60800','82090','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('85002','中清五病房衛材','1','2','60800','85002','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('85160','中清門診衛材','1','2','60800','85160','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('85170','中清急診衛材','1','2','60800','85170','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('85180','中清開刀房衛材','1','2','60800','85180','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('86000','中清耳鼻喉科衛材','1','2','60800','86000','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('86021','中清身心衛材','1','2','60800','86021','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('86040','中清體檢衛材','1','2','60800','86040','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('86060','中清護理之家衛材','1','2','60800','86060','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('9000','精神科衛材','1','2','60800','9000','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('91001','總院中醫科衛材','1','2','60800','91001','60800','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('91002','中清中醫科衛材','1','2','60800','91002','60800','N',sysdate,'上線轉檔');	
            ";
        }

        private string GetWhno804()
        {
            return @"
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EA30','衛保組(藥庫)','0','1','','A30','','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EA8','藥劑科(藥局)','0','2','EA30','A8','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E1ICU','1ICU藥品','0','3','EA8','1ICU','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E2ICU','2ICU藥品','0','3','EA8','2ICU','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E3ICU','3ICU藥品','0','3','EA8','3ICU','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E3ICU-2','3ICU-外藥品','0','3','EA8','3ICU-2','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EA11','急診室藥品','0','4','EA8','A11','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EA15','牙科藥品','0','4','EA8','A15','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EA15-7','牙科-北監藥品','0','4','EA8','A15-7','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EA16','眼科藥品','0','4','EA8','A16','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EA17','耳鼻喉科藥品','0','4','EA8','A17','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EA18','婦產科藥品','0','4','EA8','A18','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EA19','精神科藥品','0','4','EA8','A19','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EA2','復健科藥品','0','4','EA8','A2','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EA20','小兒科藥品','0','4','EA8','A20','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EA21','護理之家藥品','0','4','EA8','A21','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EA22','洗腎中心藥品','0','4','EA8','A22','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EA22-1','腹膜透析室藥品','0','4','EA8','A22-1','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EA23','開刀房藥品','0','4','EA8','A23','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EA23-1','開刀房-公(通)藥品','0','4','EA8','A23-1','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EA23-2','開刀房-公(線)藥品','0','4','EA8','A23-2','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EA23-3','開刀房-骨藥品','0','4','EA8','A23-3','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EA23-4','開刀房-外藥品','0','4','EA8','A23-4','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EA23-5','開刀房-婦藥品','0','4','EA8','A23-5','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EA23-6','開刀房-耳鼻喉藥品','0','4','EA8','A23-6','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EA23-7','開刀房-眼藥品','0','4','EA8','A23-7','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EA23-8','開刀房-牙藥品','0','4','EA8','A23-8','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EA23-9','開刀房-復健科藥品','0','4','EA8','A23-9','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EA24','麻醉科藥品','0','4','EA8','A24','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EA25','巡迴醫療組藥品','0','4','EA8','A25','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EA26','皮膚科藥品','0','4','EA8','A26','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EA28','神經內科藥品','0','4','EA8','A28','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EA29','醫學美容中心藥品','0','4','EA8','A29','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EA31','居家護理藥品','0','4','EA8','A31','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EA32','血液腫廇科藥品','0','4','EA8','A32','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EA34','社區醫學部藥品','0','4','EA8','A34','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EA35','放射腫瘤科藥品','0','4','EA8','A35','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EA4','體檢組藥品','0','4','EA8','A4','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EA41','醫勤組藥品','0','4','EA8','A41','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EA5','行政組藥品','0','4','EA8','A5','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EA6','主計組藥品','0','4','EA8','A6','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EA7','醫勤組藥品','0','4','EA8','A7','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EA9','放射科藥品','0','4','EA8','A9','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EBON','骨科藥品','0','4','EA8','BON','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EC01','心電圖室藥品','0','4','EA8','C01','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EC02','心導管室藥品','0','4','EA8','C02','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EC03','胃鏡室藥品','0','4','EA8','C03','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EC04','衛教室藥品','0','4','EA8','C04','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('ECAR','心臟內科藥品','0','4','EA8','CAR','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('ECC','癌症委員會藥品','0','4','EA8','CC','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('ECVS','心臟外科藥品','0','4','EA8','CVS','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('ED01','門診部藥品','0','4','EA8','D01','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('ED01-1','門診部-內藥品','0','4','EA8','D01-1','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('ED01-2','門診部-外藥品','0','4','EA8','D01-2','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('ED01-3','門診部-骨藥品','0','4','EA8','D01-3','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('ED01-4','門診部-兒藥品','0','4','EA8','D01-4','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('ED01-5','門診部-婦藥品','0','4','EA8','D01-5','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('ED01-6','門診部-耳藥品','0','4','EA8','D01-6','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('ED01-7','門診部-北監藥品','0','4','EA8','D01-7','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('ED01-8','門診部-中醫藥品','0','4','EA8','D01-8','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EGC','氣體中心藥品','0','4','EA8','GC','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EGM','一般內科藥品','0','4','EA8','GM','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EHM','家醫科藥品','0','4','EA8','HM','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EHM1','高壓氧中心藥品','0','4','EA8','HM1','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EHM2','HM2藥品','0','4','EA8','HM2','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EHM3','高階健檢中心藥品','0','4','EA8','HM3','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EICUS','外科加護中心藥品','0','4','EA8','ICUS','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EINF','感控室藥品','0','4','EA8','INF','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EMLAB','醫學研究室藥品','0','4','EA8','MLAB','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('ENUT','營養室藥品','0','4','EA8','NUT','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EP01','人力中心藥品','0','4','EA8','P01','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EPIG','PIG藥品','0','4','EA8','PIG','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EPM','腸胃內科藥品','0','4','EA8','PM','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EPS','整形外科藥品','0','4','EA8','PS','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EPUB','公共藥品','0','4','EA8','PUB','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EPUL','胸腔內科藥品','0','4','EA8','PUL','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('ERT','呼吸治療室藥品','0','4','EA8','RT','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('ERT-1','呼吸治療室-內藥品','0','4','EA8','RT-1','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('ERT-2','呼吸治療室-外藥品','0','4','EA8','RT-2','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('ESU01','供應中心藥品','0','4','EA8','SU01','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('ESUR','外科部藥品','0','4','EA8','SUR','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('ESUR01','外科部辦公室藥品','0','4','EA8','SUR01','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EW01','一病房藥品','0','3','EA8','W01','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EW02','二病房藥品','0','3','EA8','W02','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EW03','三病房藥品','0','3','EA8','W03','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EW05','五病房藥品','0','3','EA8','W05','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EW06','六病房藥品','0','3','EA8','W06','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EW06-1','六病房-婦藥品','0','3','EA8','W06-1','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EW06-2','六病房-兒藥品','0','3','EA8','W06-2','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EW07','七病房藥品','0','3','EA8','W07','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EW08','八病房藥品','0','3','EA8','W08','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EW09','九病房藥品','0','3','EA8','W09','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EW10','十病房藥品','0','3','EA8','W10','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EW11','十一病房藥品','0','3','EA8','W11','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EW12','十二病房藥品','0','3','EA8','W12','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EW20','二十病房藥品','0','3','EA8','W20','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EW21','二十一病房藥品','0','3','EA8','W21','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EW22','二十二病房藥品','0','3','EA8','W22','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EW23','二十三病房藥品','0','3','EA8','W23','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EWBC','5ICU藥品','0','3','EA8','WBC','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EWBC-1','5ICU(內)藥品','0','3','EA8','WBC-1','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EWBC-2','5ICU(外)藥品','0','3','EA8','WBC-2','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EWBC-3','5ICU(骨）藥品','0','3','EA8','WBC-3','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EWRT','呼吸病房藥品','0','3','EA8','WRT','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EWRT-1','呼吸病房-內藥品','0','3','EA8','WRT-1','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EWRT-2','呼吸病房-外藥品','0','3','EA8','WRT-2','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('EZ','檢驗科藥品','0','4','EA8','Z','A8','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('A30','衛保組(衛材庫)','1','1','','A30','','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('A8-1','藥劑科衛材','1','2','A30','A8-1','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('1ICU','1ICU衛材','1','2','A30','1ICU','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('2ICU','2ICU衛材','1','2','A30','2ICU','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('3ICU','3ICU衛材','1','2','A30','3ICU','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('3ICU-2','3ICU-外衛材','1','2','A30','3ICU-2','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('A11','急診室衛材','1','2','A30','A11','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('A15','牙科衛材','1','2','A30','A15','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('A15-7','牙科-北監衛材','1','2','A30','A15-7','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('A16','眼科衛材','1','2','A30','A16','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('A17','耳鼻喉科衛材','1','2','A30','A17','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('A18','婦產科衛材','1','2','A30','A18','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('A19','精神科衛材','1','2','A30','A19','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('A2','復健科衛材','1','2','A30','A2','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('A20','小兒科衛材','1','2','A30','A20','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('A21','護理之家衛材','1','2','A30','A21','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('A22','洗腎中心衛材','1','2','A30','A22','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('A22-1','腹膜透析室衛材','1','2','A30','A22-1','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('A23','開刀房衛材','1','2','A30','A23','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('A23-1','開刀房-公(通)衛材','1','2','A30','A23-1','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('A23-2','開刀房-公(線)衛材','1','2','A30','A23-2','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('A23-3','開刀房-骨衛材','1','2','A30','A23-3','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('A23-4','開刀房-外衛材','1','2','A30','A23-4','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('A23-5','開刀房-婦衛材','1','2','A30','A23-5','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('A23-6','開刀房-耳鼻喉衛材','1','2','A30','A23-6','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('A23-7','開刀房-眼衛材','1','2','A30','A23-7','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('A23-8','開刀房-牙衛材','1','2','A30','A23-8','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('A23-9','開刀房-復健科衛材','1','2','A30','A23-9','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('A24','麻醉科衛材','1','2','A30','A24','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('A25','巡迴醫療組衛材','1','2','A30','A25','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('A26','皮膚科衛材','1','2','A30','A26','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('A28','神經內科衛材','1','2','A30','A28','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('A29','醫學美容中心衛材','1','2','A30','A29','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('A31','居家護理衛材','1','2','A30','A31','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('A32','血液腫廇科衛材','1','2','A30','A32','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('A34','社區醫學部衛材','1','2','A30','A34','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('A35','放射腫瘤科衛材','1','2','A30','A35','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('A4','體檢組衛材','1','2','A30','A4','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('A41','醫勤組衛材','1','2','A30','A41','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('A5','行政組衛材','1','2','A30','A5','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('A6','主計組衛材','1','2','A30','A6','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('A7','醫勤組衛材','1','2','A30','A7','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('A9','放射科衛材','1','2','A30','A9','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('BON','骨科衛材','1','2','A30','BON','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('C01','心電圖室衛材','1','2','A30','C01','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('C02','心導管室衛材','1','2','A30','C02','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('C03','胃鏡室衛材','1','2','A30','C03','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('C04','衛教室衛材','1','2','A30','C04','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('CAR','心臟內科衛材','1','2','A30','CAR','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('CC','癌症委員會衛材','1','2','A30','CC','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('CVS','心臟外科衛材','1','2','A30','CVS','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('D01','門診部衛材','1','2','A30','D01','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('D01-1','門診部-內衛材','1','2','A30','D01-1','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('D01-2','門診部-外衛材','1','2','A30','D01-2','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('D01-3','門診部-骨衛材','1','2','A30','D01-3','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('D01-4','門診部-兒衛材','1','2','A30','D01-4','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('D01-5','門診部-婦衛材','1','2','A30','D01-5','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('D01-6','門診部-耳衛材','1','2','A30','D01-6','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('D01-7','門診部-北監衛材','1','2','A30','D01-7','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('D01-8','門診部-中醫衛材','1','2','A30','D01-8','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('GC','氣體中心衛材','1','2','A30','GC','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('GM','一般內科衛材','1','2','A30','GM','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('HM','家醫科衛材','1','2','A30','HM','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('HM1','高壓氧中心衛材','1','2','A30','HM1','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('HM2','HM2衛材','1','2','A30','HM2','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('HM3','高階健檢中心衛材','1','2','A30','HM3','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('ICUS','外科加護中心衛材','1','2','A30','ICUS','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('INF','感控室衛材','1','2','A30','INF','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('MLAB','醫學研究室衛材','1','2','A30','MLAB','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('NUT','營養室衛材','1','2','A30','NUT','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('P01','人力中心衛材','1','2','A30','P01','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('PIG','PIG衛材','1','2','A30','PIG','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('PM','腸胃內科衛材','1','2','A30','PM','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('PS','整形外科衛材','1','2','A30','PS','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('PUB','公共衛材','1','2','A30','PUB','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('PUL','胸腔內科衛材','1','2','A30','PUL','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('RT','呼吸治療室衛材','1','2','A30','RT','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('RT-1','呼吸治療室-內衛材','1','2','A30','RT-1','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('RT-2','呼吸治療室-外衛材','1','2','A30','RT-2','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('SU01','供應中心衛材','1','2','A30','SU01','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('SUR','外科部衛材','1','2','A30','SUR','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('SUR01','外科部辦公室衛材','1','2','A30','SUR01','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('W01','一病房衛材','1','2','A30','W01','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('W02','二病房衛材','1','2','A30','W02','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('W03','三病房衛材','1','2','A30','W03','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('W05','五病房衛材','1','2','A30','W05','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('W06','六病房衛材','1','2','A30','W06','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('W06-1','六病房-婦衛材','1','2','A30','W06-1','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('W06-2','六病房-兒衛材','1','2','A30','W06-2','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('W07','七病房衛材','1','2','A30','W07','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('W08','八病房衛材','1','2','A30','W08','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('W09','九病房衛材','1','2','A30','W09','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('W10','十病房衛材','1','2','A30','W10','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('W11','十一病房衛材','1','2','A30','W11','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('W12','十二病房衛材','1','2','A30','W12','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('W20','二十病房衛材','1','2','A30','W20','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('W21','二十一病房衛材','1','2','A30','W21','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('W22','二十二病房衛材','1','2','A30','W22','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('W23','二十三病房衛材','1','2','A30','W23','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('WBC','5ICU衛材','1','2','A30','WBC','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('WBC-1','5ICU(內)衛材','1','2','A30','WBC-1','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('WBC-2','5ICU(外)衛材','1','2','A30','WBC-2','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('WBC-3','5ICU(骨）衛材','1','2','A30','WBC-3','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('WRT','呼吸病房衛材','1','2','A30','WRT','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('WRT-1','呼吸病房-內衛材','1','2','A30','WRT-1','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('WRT-2','呼吸病房-外衛材','1','2','A30','WRT-2','A30','N',sysdate,'上線轉檔');
                                        insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('Z','檢驗科衛材','1','2','A30','Z','A30','N',sysdate,'上線轉檔');

            ";
        }

        private string GetWhno805()
        {
            return @"
            insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E66010','衛保組(藥庫)','0','1','','66010','','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E40010','藥劑科(北埔藥局)','0','2','E66010','40010','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E40020','太魯閣藥局','0','2','E66010','40020','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E40030','進豐藥局','0','2','E66010','40030','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E101022','肺功能室藥品','0','4','E40010','101022','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E101042','胃鏡室藥品','0','4','E40010','101042','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E101053','心電圖室藥品','0','4','E40010','101053','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E10127','軍事醫療作業費藥品','0','4','E40010','10127','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E105000','眼科藥品','0','4','E40010','105000','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E106000','耳鼻喉科藥品','0','4','E40010','106000','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E107000','牙科藥品','0','4','E40010','107000','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E107001','牙特材藥品','0','4','E40010','107001','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E107010','監所牙科藥品','0','4','E40010','107010','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E108000','家醫科藥品','0','4','E40010','108000','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E113000','復健科藥品','0','4','E40010','113000','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E121000','放射科藥品','0','4','E40010','121000','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E122020','檢驗科藥品','0','4','E40010','122020','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E123000','麻醉科藥品','0','4','E40010','123000','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E130011','太魯閣門診藥品','0','4','E40020','130011','40020','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E130012','北埔門診藥品','0','4','E40010','130012','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E130013','進豐門診藥品','0','4','E40030','130013','40030','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E130014','監所門診藥品','0','4','E40010','130014','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E130015','皮膚美容科藥品','0','4','E40010','130015','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E140040','急診室藥品','0','4','E40010','140040','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E140050','手術室藥品','0','4','E40010','140050','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E140080','血液透析室藥品','0','4','E40010','140080','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E140090','加護病房藥品','0','3','E40010','140090','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E140120','W51藥品','0','3','E40010','140120','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E140130','二病房藥品','0','3','E40010','140130','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E140150','三病房藥品','0','3','E40010','140150','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E140180','六病房藥品','0','3','E40010','140180','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E140210','九病房藥品','0','3','E40010','140210','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E140230','十一病房藥品','0','3','E40010','140230','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E140235','十二病房藥品','0','3','E40010','140235','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E140240','W31藥品','0','3','E40010','140240','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E140250','RCW藥品','0','3','E40010','140250','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E170351','供應中心藥品','0','4','E40010','170351','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E815030','婦產科藥品','0','4','E40010','815030','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E815134','中醫科藥品','0','4','E40010','815134','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E815173','體檢室藥品','0','4','E40010','815173','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E815230','骨特材藥品','0','4','E40010','815230','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E815255','社醫部藥品','0','4','E40010','815255','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E815271','神經內科藥品','0','4','E40010','815271','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E815272','精神科藥品','0','4','E40010','815272','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E815420','W52藥品','0','3','E40010','815420','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E815470','毒物中心藥品','0','4','E40010','815470','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E815563','心導管室藥品','0','4','E40010','815563','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E815702','醫勤室藥品','0','4','E40010','815702','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E815730','一般護理之家藥品','0','4','E40010','815730','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E815765','精神護理之家藥品','0','4','E40010','815765','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E815770','行政室藥品','0','4','E40010','815770','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E815782','通用氣體藥品','0','4','E40010','815782','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E815797','W33藥品','0','3','E40010','815797','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E815812','W012病房藥品','0','3','E40010','815812','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('E815903','勤務隊藥品','0','4','E40010','815903','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('66010','衛保組(衛材庫)','1','1','','66010','','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('40010','藥劑科(北埔藥局)衛材','1','2','66010','40010','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('40020','太魯閣藥局衛材','1','2','66010','40020','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('40030','進豐藥局衛材','1','2','66010','40030','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('101022','肺功能室衛材','1','2','66010','101022','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('101042','胃鏡室衛材','1','2','66010','101042','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('101053','心電圖室衛材','1','2','66010','101053','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('10127','軍事醫療作業費衛材','1','2','66010','10127','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('105000','眼科衛材','1','2','66010','105000','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('106000','耳鼻喉科衛材','1','2','66010','106000','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('107000','牙科衛材','1','2','66010','107000','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('107001','牙特材衛材','1','2','66010','107001','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('107010','監所牙科衛材','1','2','66010','107010','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('108000','家醫科衛材','1','2','66010','108000','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('113000','復健科衛材','1','2','66010','113000','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('121000','放射科衛材','1','2','66010','121000','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('122020','檢驗科衛材','1','2','66010','122020','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('123000','麻醉科衛材','1','2','66010','123000','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('130011','太魯閣門診衛材','1','2','66010','130011','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('130012','北埔門診衛材','1','2','66010','130012','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('130013','進豐門診衛材','1','2','66010','130013','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('130014','監所門診衛材','1','2','66010','130014','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('130015','皮膚美容科衛材','1','2','66010','130015','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('140040','急診室衛材','1','2','66010','140040','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('140050','手術室衛材','1','2','66010','140050','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('140080','血液透析室衛材','1','2','66010','140080','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('140090','加護病房衛材','1','2','66010','140090','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('140120','W51衛材','1','2','66010','140120','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('140130','二病房衛材','1','2','66010','140130','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('140150','三病房衛材','1','2','66010','140150','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('140180','六病房衛材','1','2','66010','140180','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('140210','九病房衛材','1','2','66010','140210','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('140230','十一病房衛材','1','2','66010','140230','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('140235','十二病房衛材','1','2','66010','140235','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('140240','W31衛材','1','2','66010','140240','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('140250','RCW衛材','1','2','66010','140250','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('170351','供應中心衛材','1','2','66010','170351','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('815030','婦產科衛材','1','2','66010','815030','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('815134','中醫科衛材','1','2','66010','815134','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('815173','體檢室衛材','1','2','66010','815173','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('815230','骨特材衛材','1','2','66010','815230','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('815255','社醫部衛材','1','2','66010','815255','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('815271','神經內科衛材','1','2','66010','815271','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('815272','精神科衛材','1','2','66010','815272','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('815420','W52衛材','1','2','66010','815420','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('815470','毒物中心衛材','1','2','66010','815470','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('815563','心導管室衛材','1','2','66010','815563','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('815702','醫勤室衛材','1','2','66010','815702','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('815730','一般護理之家衛材','1','2','66010','815730','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('815765','精神護理之家衛材','1','2','66010','815765','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('815770','行政室衛材','1','2','66010','815770','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('815782','通用氣體衛材','1','2','66010','815782','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('815797','W33衛材','1','2','66010','815797','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('815812','W012病房衛材','1','2','66010','815812','66010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER) values('815903','勤務隊衛材','1','2','66010','815903','66010','N',sysdate,'上線轉檔');
            ";
        }

        private string GetWhno807()
        {
            return @"
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E66010','衛保組(藥庫)','0','1','','66010','','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E40000','藥局','0','2','E66010','40000','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E01000','內科部藥品','0','4','E40000','01000','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E01020','胸腔內科藥品','0','4','E40000','01020','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E01030','腎臟內科藥品','0','4','E40000','01030','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E01040','腸胃科藥品','0','4','E40000','01040','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E01050','心臟內科藥品','0','4','E40000','01050','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E01070','皮膚科藥品','0','4','E40000','01070','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E01080','新陳代謝科藥品','0','4','E40000','01080','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E01090','感染科藥品','0','4','E40000','01090','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E01100','傳染病科藥品','0','4','E40000','01100','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E02000','外科部藥品','0','4','E40000','02000','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E02010','一般外科藥品','0','4','E40000','02010','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E02020','小兒外科藥品','0','4','E40000','02020','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E02030','胸腔外科藥品','0','4','E40000','02030','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E02040','神經外科藥品','0','4','E40000','02040','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E02050','泌尿外科藥品','0','4','E40000','02050','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E02060','心臟外科藥品','0','4','E40000','02060','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E02070','大腸直腸外科藥品','0','4','E40000','02070','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E02080','整形外科藥品','0','4','E40000','02080','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E02090','一一病房藥品','0','3','E40000','02090','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E03000','婦產科藥品','0','4','E40000','03000','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E04000','小兒科藥品','0','4','E40000','04000','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E05000','眼科藥品','0','4','E40000','05000','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E06000','耳鼻喉科藥品','0','4','E40000','06000','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E07000','牙科藥品','0','4','E40000','07000','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E08000','家庭醫學科藥品','0','4','E40000','08000','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E09000','骨科藥品','0','4','E40000','09000','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E10000','精神科藥品','0','4','E40000','10000','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E11000','神經內科藥品','0','4','E40000','11000','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E13000','護理之家藥品','0','4','E40000','13000','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E14000','航空醫學部藥品','0','4','E40000','14000','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E15000','復健科藥品','0','4','E40000','15000','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E30010','病理科藥品','0','4','E40000','30010','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E30020','檢驗科藥品','0','4','E40000','30020','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E30030','麻醉科藥品','0','4','E40000','30030','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E30040','放射科藥品','0','4','E40000','30040','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E30050','營養室藥品','0','4','E40000','30050','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50000','護理部藥品','0','4','E40000','50000','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50031','三一病房藥品','0','3','E40000','50031','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50032','三二病房藥品','0','3','E40000','50032','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50033','三三病房藥品','0','3','E40000','50033','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50034','三四病房藥品','0','3','E40000','50034','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50041','四一病房藥品','0','3','E40000','50041','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50042','四二病房藥品','0','3','E40000','50042','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50043','四三病房藥品','0','3','E40000','50043','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50052','五二病房藥品','0','3','E40000','50052','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50053','五三病房藥品','0','3','E40000','50053','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50062','六二病房藥品','0','3','E40000','50062','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50091','九一病房藥品','0','3','E40000','50091','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50092','九二病房藥品','0','3','E40000','50092','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50093','九三病房藥品','0','3','E40000','50093','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50094','九四病房藥品','0','3','E40000','50094','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50100','加護病房藥品','0','3','E40000','50100','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50110','產房藥品','0','3','E40000','50110','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50120','嬰兒房(五一病房)藥品','0','3','E40000','50120','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50160','門診室藥品','0','4','E40000','50160','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50170','急診醫學中心藥品','0','4','E40000','50170','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50180','手術室藥品','0','4','E40000','50180','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50300','烏坵藥品','0','4','E40000','50300','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E60000','醫務事務部門藥品','0','4','E40000','60000','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E60010','院(副)長室藥品','0','4','E40000','60010','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E60040','醫療部藥品','0','4','E40000','60040','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E60041','醫務長室藥品','0','4','E40000','60041','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E60042','醫勤組藥品','0','4','E40000','60042','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E60050','民診處藥品','0','4','E40000','60050','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E60060','政戰部藥品','0','4','E40000','60060','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E61010','服務台藥品','0','4','E40000','61010','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E61020','掛號室藥品','0','4','E40000','61020','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E61030','病歷室藥品','0','4','E40000','61030','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E61040','住院室藥品','0','4','E40000','61040','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E61050','書記組藥品','0','4','E40000','61050','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E62000','行政組藥品','0','4','E40000','62000','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E62010','人事室藥品','0','4','E40000','62010','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E62020','文卷室藥品','0','4','E40000','62020','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E63000','主計部門藥品','0','4','E40000','63000','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E64000','資訊室藥品','0','4','E40000','64000','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E65000','教學研究部門藥品','0','4','E40000','65000','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E65010','總務藥品','0','4','E40000','65010','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E65020','警衛班藥品','0','4','E40000','65020','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E65031','文具庫藥品','0','4','E40000','65031','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E65032','被服庫藥品','0','4','E40000','65032','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E65033','洗衣房藥品','0','4','E40000','65033','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E65040','總機房藥品','0','4','E40000','65040','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E65050','駕駛班藥品','0','4','E40000','65050','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E65060','工程室藥品','0','4','E40000','65060','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E65061','設維班藥品','0','4','E40000','65061','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E65063','水電班藥品','0','4','E40000','65063','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E65064','空調班藥品','0','4','E40000','65064','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E65065','鍋爐班藥品','0','4','E40000','65065','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E65070','清潔維護班藥品','0','4','E40000','65070','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E65080','環保室藥品','0','4','E40000','65080','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E65082','廢水處理班藥品','0','4','E40000','65082','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E65084','廢棄物處理場藥品','0','4','E40000','65084','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E65090','公共衛生室藥品','0','4','E40000','65090','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E65110','官兵餐廳藥品','0','4','E40000','65110','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E65120','診療所藥品','0','4','E40000','65120','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E65130','勤務隊藥品','0','4','E40000','65130','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E65140','宿舍藥品','0','4','E40000','65140','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E66000','衛保部門藥品','0','4','E40000','66000','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E66020','供應中心藥品','0','4','E40000','66020','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E66030','醫學工程室藥品','0','4','E40000','66030','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E70000','圖書館藥品','0','4','E40000','70000','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E80000','社會服務部藥品','0','4','E40000','80000','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E90000','其他部門藥品','0','4','E40000','90000','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E90011','美容部藥品','0','4','E40000','90011','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E90013','燒傷中心藥品','0','4','E40000','90013','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E90020','餐飲部藥品','0','4','E40000','90020','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E90030','交誼廳藥品','0','4','E40000','90030','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E90050','福利部藥品','0','4','E40000','90050','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E90080','醫療器材販賣部藥品','0','4','E40000','90080','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E90090','收費停車場藥品','0','4','E40000','90090','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E90100','太平間藥品','0','4','E40000','90100','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E90110','其他藥品','0','4','E40000','90110','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E90111','高壓氧中心藥品','0','4','E40000','90111','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E90112','社區醫學部藥品','0','4','E40000','90112','40000','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('66010','衛保組(衛材庫)','1','1','','66010','','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('40000','藥局衛材','1','2','66010','40000','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('01000','內科部衛材','1','2','66010','01000','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('01020','胸腔內科衛材','1','2','66010','01020','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('01030','腎臟內科衛材','1','2','66010','01030','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('01040','腸胃科衛材','1','2','66010','01040','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('01050','心臟內科衛材','1','2','66010','01050','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('01070','皮膚科衛材','1','2','66010','01070','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('01080','新陳代謝科衛材','1','2','66010','01080','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('01090','感染科衛材','1','2','66010','01090','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('01100','傳染病科衛材','1','2','66010','01100','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('02000','外科部衛材','1','2','66010','02000','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('02010','一般外科衛材','1','2','66010','02010','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('02020','小兒外科衛材','1','2','66010','02020','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('02030','胸腔外科衛材','1','2','66010','02030','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('02040','神經外科衛材','1','2','66010','02040','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('02050','泌尿外科衛材','1','2','66010','02050','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('02060','心臟外科衛材','1','2','66010','02060','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('02070','大腸直腸外科衛材','1','2','66010','02070','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('02080','整形外科衛材','1','2','66010','02080','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('02090','一一病房衛材','1','2','66010','02090','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('03000','婦產科衛材','1','2','66010','03000','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('04000','小兒科衛材','1','2','66010','04000','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('05000','眼科衛材','1','2','66010','05000','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('06000','耳鼻喉科衛材','1','2','66010','06000','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('07000','牙科衛材','1','2','66010','07000','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('08000','家庭醫學科衛材','1','2','66010','08000','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('09000','骨科衛材','1','2','66010','09000','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('10000','精神科衛材','1','2','66010','10000','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('11000','神經內科衛材','1','2','66010','11000','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('13000','護理之家衛材','1','2','66010','13000','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('14000','航空醫學部衛材','1','2','66010','14000','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('15000','復健科衛材','1','2','66010','15000','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('30010','病理科衛材','1','2','66010','30010','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('30020','檢驗科衛材','1','2','66010','30020','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('30030','麻醉科衛材','1','2','66010','30030','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('30040','放射科衛材','1','2','66010','30040','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('30050','營養室衛材','1','2','66010','30050','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50000','護理部衛材','1','2','66010','50000','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50031','三一病房衛材','1','2','66010','50031','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50032','三二病房衛材','1','2','66010','50032','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50033','三三病房衛材','1','2','66010','50033','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50034','三四病房衛材','1','2','66010','50034','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50041','四一病房衛材','1','2','66010','50041','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50042','四二病房衛材','1','2','66010','50042','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50043','四三病房衛材','1','2','66010','50043','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50052','五二病房衛材','1','2','66010','50052','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50053','五三病房衛材','1','2','66010','50053','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50062','六二病房衛材','1','2','66010','50062','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50091','九一病房衛材','1','2','66010','50091','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50092','九二病房衛材','1','2','66010','50092','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50093','九三病房衛材','1','2','66010','50093','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50094','九四病房衛材','1','2','66010','50094','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50100','加護病房衛材','1','2','66010','50100','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50110','產房衛材','1','2','66010','50110','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50120','嬰兒房(五一病房)衛材','1','2','66010','50120','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50160','門診室衛材','1','2','66010','50160','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50170','急診醫學中心衛材','1','2','66010','50170','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50180','手術室衛材','1','2','66010','50180','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50300','烏坵衛材','1','2','66010','50300','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('60000','醫務事務部門衛材','1','2','66010','60000','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('60010','院(副)長室衛材','1','2','66010','60010','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('60040','醫療部衛材','1','2','66010','60040','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('60041','醫務長室衛材','1','2','66010','60041','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('60042','醫勤組衛材','1','2','66010','60042','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('60050','民診處衛材','1','2','66010','60050','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('60060','政戰部衛材','1','2','66010','60060','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('61010','服務台衛材','1','2','66010','61010','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('61020','掛號室衛材','1','2','66010','61020','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('61030','病歷室衛材','1','2','66010','61030','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('61040','住院室衛材','1','2','66010','61040','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('61050','書記組衛材','1','2','66010','61050','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('62000','行政組衛材','1','2','66010','62000','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('62010','人事室衛材','1','2','66010','62010','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('62020','文卷室衛材','1','2','66010','62020','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('63000','主計部門衛材','1','2','66010','63000','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('64000','資訊室衛材','1','2','66010','64000','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('65000','教學研究部門衛材','1','2','66010','65000','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('65010','總務衛材','1','2','66010','65010','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('65020','警衛班衛材','1','2','66010','65020','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('65031','文具庫衛材','1','2','66010','65031','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('65032','被服庫衛材','1','2','66010','65032','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('65033','洗衣房衛材','1','2','66010','65033','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('65040','總機房衛材','1','2','66010','65040','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('65050','駕駛班衛材','1','2','66010','65050','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('65060','工程室衛材','1','2','66010','65060','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('65061','設維班衛材','1','2','66010','65061','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('65063','水電班衛材','1','2','66010','65063','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('65064','空調班衛材','1','2','66010','65064','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('65065','鍋爐班衛材','1','2','66010','65065','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('65070','清潔維護班衛材','1','2','66010','65070','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('65080','環保室衛材','1','2','66010','65080','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('65082','廢水處理班衛材','1','2','66010','65082','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('65084','廢棄物處理場衛材','1','2','66010','65084','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('65090','公共衛生室衛材','1','2','66010','65090','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('65110','官兵餐廳衛材','1','2','66010','65110','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('65120','診療所衛材','1','2','66010','65120','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('65130','勤務隊衛材','1','2','66010','65130','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('65140','宿舍衛材','1','2','66010','65140','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('66000','衛保部門衛材','1','2','66010','66000','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('66020','供應中心衛材','1','2','66010','66020','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('66030','醫學工程室衛材','1','2','66010','66030','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('70000','圖書館衛材','1','2','66010','70000','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('80000','社會服務部衛材','1','2','66010','80000','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('90000','其他部門衛材','1','2','66010','90000','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('90011','美容部衛材','1','2','66010','90011','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('90012','烏坵(衛材)','1','2','66010','90012','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('90013','燒傷中心衛材','1','2','66010','90013','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('90020','餐飲部衛材','1','2','66010','90020','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('90030','交誼廳衛材','1','2','66010','90030','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('90050','福利部衛材','1','2','66010','90050','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('90080','醫療器材販賣部衛材','1','2','66010','90080','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('90090','收費停車場衛材','1','2','66010','90090','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('90100','太平間衛材','1','2','66010','90100','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('90110','其他衛材','1','2','66010','90110','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('90111','高壓氧中心衛材','1','2','66010','90111','66010','N',sysdate,'上線轉檔');		
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('90112','社區醫學部衛材','1','2','66010','90112','66010','N',sysdate,'上線轉檔');		
        ";
        }

        private string GetWhno811()
        {
            return @"
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('ER03','藥庫','0','1','','R03','','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E30','藥局','0','2','ER03','30','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E01','五一病房藥品','0','3','E30','01','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E02','五二病房藥品','0','3','E30','02','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E03','四一病房藥品','0','3','E30','03','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E04','供應中心藥品','0','4','E30','04','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E04S','供應中心自用藥品','0','4','E30','04S','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E05','手術室藥品','0','4','E30','05','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E06','嬰兒房藥品','0','4','E30','06','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E07','加護病房藥品','0','4','E30','07','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E08','洗腎室藥品','0','4','E30','08','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E09','急診室藥品','0','4','E30','09','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E10','注射室藥品','0','4','E30','10','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E11','外科門診藥品','0','4','E30','11','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E12','胃鏡室藥品','0','4','E30','12','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E13','復健室藥品','0','4','E30','13','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E14','潛醫科藥品','0','4','E30','14','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E15','婦產科藥品','0','4','E30','15','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E16','放射科藥品','0','4','E30','16','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E17','檢驗科藥品','0','4','E30','17','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E18','體檢組藥品','0','4','E30','18','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E19','院辦室藥品','0','4','E30','19','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E22','職業安全衛生室藥品','0','4','E30','22','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E23','內科門診藥品','0','4','E30','23','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E24','眼科門診藥品','0','4','E30','24','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E25','牙科門診藥品','0','4','E30','25','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E29','退貨盤虧藥品','0','4','E30','29','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E31','居家護理藥品','0','4','E30','31','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E32','麻醉科藥品','0','4','E30','32','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E33','耳鼻喉科藥品','0','4','E30','33','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E34','呼吸照護藥品','0','4','E30','34','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E37','感控室藥品','0','4','E30','37','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E38','報廢藥品','0','4','E30','38','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E41','皮膚科藥品','0','4','E30','41','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E42','小兒科藥品','0','4','E30','42','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E43','負壓病房藥品','0','4','E30','43','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E46','46藥品','0','4','E30','46','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E47','調劑室藥品','0','4','E30','47','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E48','教學組藥品','0','4','E30','48','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E51','預財室藥品','0','4','E30','51','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E52','52藥品','0','4','E30','52','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E55','民診辦公室藥品','0','4','E30','55','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E56','內外科辦公室藥品','0','4','E30','56','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E57','57藥品','0','4','E30','57','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E58','心導管室藥品','0','4','E30','58','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E59','神經傳導室藥品','0','4','E30','59','30','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('R03','衛材庫','1','1','','R03','','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('30','藥局衛材','1','2','R03','30','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('01','五一病房衛材','1','2','R03','01','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('02','五二病房衛材','1','2','R03','02','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('03','四一病房衛材','1','2','R03','03','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('04','供應中心衛材','1','2','R03','04','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('04S','供應中心自用衛材','1','2','R03','04S','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('05','手術室衛材','1','2','R03','05','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('06','嬰兒房衛材','1','2','R03','06','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('07','加護病房衛材','1','2','R03','07','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('08','洗腎室衛材','1','2','R03','08','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('09','急診室衛材','1','2','R03','09','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('10','注射室衛材','1','2','R03','10','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('11','外科門診衛材','1','2','R03','11','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('12','胃鏡室衛材','1','2','R03','12','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('13','復健室衛材','1','2','R03','13','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('14','潛醫科衛材','1','2','R03','14','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('15','婦產科衛材','1','2','R03','15','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('16','放射科衛材','1','2','R03','16','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('17','檢驗科衛材','1','2','R03','17','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('18','體檢組衛材','1','2','R03','18','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('19','院辦室衛材','1','2','R03','19','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('22','職業安全衛生室衛材','1','2','R03','22','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('23','內科門診衛材','1','2','R03','23','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('24','眼科門診衛材','1','2','R03','24','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('25','牙科門診衛材','1','2','R03','25','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('29','退貨盤虧衛材','1','2','R03','29','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('31','居家護理衛材','1','2','R03','31','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('32','麻醉科衛材','1','2','R03','32','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('33','耳鼻喉科衛材','1','2','R03','33','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('34','呼吸照護衛材','1','2','R03','34','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('37','感控室衛材','1','2','R03','37','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('38','報廢衛材','1','2','R03','38','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('41','皮膚科衛材','1','2','R03','41','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('42','小兒科衛材','1','2','R03','42','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('43','負壓病房衛材','1','2','R03','43','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('46','46衛材','1','2','R03','46','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('47','調劑室衛材','1','2','R03','47','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('48','教學組衛材','1','2','R03','48','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('51','預財室衛材','1','2','R03','51','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('52','52衛材','1','2','R03','52','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('55','民診辦公室衛材','1','2','R03','55','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('56','內外科辦公室衛材','1','2','R03','56','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('57','57衛材','1','2','R03','57','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('58','心導管室衛材','1','2','R03','58','R03','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('59','神經傳導室衛材','1','2','R03','59','R03','N',sysdate,'上線轉檔');
";
        }

        private string GetWhno812()
        {
            return @"
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E60800','藥劑科庫房','0','1','','60800','','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E40010','正榮藥局','0','2','E60800','40010','60800','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E70010','孝二藥局','0','2','E60800','70010','60800','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E02010','洗腎室藥品','0','4','E40010','02010','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E08000','復健科藥品','0','4','E40010','08000','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E16000','潛醫科藥品','0','4','E40010','16000','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E18000','檢驗科藥品','0','4','E40010','18000','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E19000','放射科藥品','0','4','E40010','19000','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E20000','麻醉科藥品','0','4','E40010','20000','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E32010','心臟功能室藥品','0','4','E40010','32010','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E37010','胃鏡室藥品','0','4','E40010','37010','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E39000','健檢中心藥品','0','4','E40010','39000','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50021','21病房藥品','0','3','E40010','50021','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50031','31病房藥品','0','3','E40010','50031','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50041','41病房藥品','0','3','E40010','50041','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50042','42病房藥品','0','3','E40010','50042','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50100','加護病房藥品','0','3','E40010','50100','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50150','病歷室藥品','0','4','E40010','50150','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50160','正榮門診藥品','0','4','E40010','50160','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50170','急診室藥品','0','4','E40010','50170','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50180','開刀房藥品','0','4','E40010','50180','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50190','供應中心藥品','0','4','E40010','50190','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E60000','醫事行政科藥品','0','4','E40010','60000','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E60820','儲備動員藥品','0','4','E40010','60820','40010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E70160','孝二門診藥品','0','4','E70010','70160','70010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E70700','孝二牙科藥品','0','4','E70010','70700','70010','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('60810','藥劑科衛材','1','1','','60810','','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('02010','洗腎室衛材','1','2','60810','02010','60810','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('08000','復健科衛材','1','2','60810','08000','60810','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('16000','潛醫科衛材','1','2','60810','16000','60810','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('18000','檢驗科衛材','1','2','60810','18000','60810','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('19000','放射科衛材','1','2','60810','19000','60810','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('20000','麻醉科衛材','1','2','60810','20000','60810','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('32010','心臟功能室衛材','1','2','60810','32010','60810','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('37010','胃鏡室衛材','1','2','60810','37010','60810','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('39000','健檢中心衛材','1','2','60810','39000','60810','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('40020','正榮藥局衛材','1','2','60810','40020','60810','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50021','21病房衛材','1','2','60810','50021','60810','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50031','31病房衛材','1','2','60810','50031','60810','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50041','41病房衛材','1','2','60810','50041','60810','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50042','42病房衛材','1','2','60810','50042','60810','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50100','加護病房衛材','1','2','60810','50100','60810','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50150','病歷室衛材','1','2','60810','50150','60810','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50160','正榮門診衛材','1','2','60810','50160','60810','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50170','急診室衛材','1','2','60810','50170','60810','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50180','開刀房衛材','1','2','60810','50180','60810','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50190','供應中心衛材','1','2','60810','50190','60810','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('60000','醫事行政科衛材','1','2','60810','60000','60810','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('60820','儲備動員衛材','1','2','60810','60820','60810','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('70020','孝二藥局衛材','1','2','60810','70020','60810','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('70160','孝二門診衛材','1','2','60810','70160','60810','N',sysdate,'上線轉檔');
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('70700','孝二牙科衛材','1','2','60810','70700','60810','N',sysdate,'上線轉檔');
";
        }

        private string GetWhno813()
        {
            return @"
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('PH1S','藥庫','0','1','','60801','','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E60801','藥事科1(藥局)','0','2','PH1S','60801','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00001','門診藥品','0','4','E60801','00001','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00001-1','門診-內藥品','0','4','E60801','00001-1','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00001-2','門診-外藥品','0','4','E60801','00001-2','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00001-3','門診-婦藥品','0','4','E60801','00001-3','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00001-4','門診-兒藥品','0','4','E60801','00001-4','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00001-5','門診-身心科藥品','0','4','E60801','00001-5','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00002','急診藥品','0','4','E60801','00002','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00002-1','急診-內藥品','0','4','E60801','00002-1','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00002-2','急診-外藥品','0','4','E60801','00002-2','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00002-3','急診-婦藥品','0','4','E60801','00002-3','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00002-4','急診-兒藥品','0','4','E60801','00002-4','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00003','放射科藥品','0','4','E60801','00003','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00004','檢驗科藥品','0','4','E60801','00004','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00005','三病房藥品','0','3','E60801','00005','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E0000501','三病房-婦藥品','0','3','E60801','0000501','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E0000502','三病房-兒藥品','0','3','E60801','0000502','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00006','病理科藥品','0','4','E60801','00006','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00007','骨科藥品','0','4','E60801','00007','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00008','護理之家藥品','0','4','E60801','00008','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00009','眼科藥品','0','4','E60801','00009','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00010','氣體藥品','0','4','E60801','00010','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00010-1','氣體-皮膚科藥品','0','4','E60801','00010-1','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00011','開刀房藥品','0','4','E60801','00011','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00011-1','開刀房-外藥品','0','4','E60801','00011-1','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00011-2','開刀房-婦藥品','0','4','E60801','00011-2','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00011-3','開刀房-眼科藥品','0','4','E60801','00011-3','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00011-4','開刀房-耳鼻喉科藥品','0','4','E60801','00011-4','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00012','醫行科藥品','0','4','E60801','00012','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00013','供應中心藥品','0','4','E60801','00013','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00014','加護病房藥品','0','3','E60801','00014','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00014-1','加護病房-內藥品','0','3','E60801','00014-1','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00014-2','加護病房-外藥品','0','3','E60801','00014-2','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00015','身心科藥品','0','4','E60801','00015','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00015-1','身心科-二病房藥品','0','3','E60801','00015-1','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00015-2','身心科-九病房藥品','0','3','E60801','00015-2','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00016','麻醉科藥品','0','4','E60801','00016','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00017','牙科藥品','0','4','E60801','00017','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00018','復健科藥品','0','4','E60801','00018','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00019','體檢中心藥品','0','4','E60801','00019','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00020','居家護理藥品','0','4','E60801','00020','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00021','六病房藥品','0','3','E60801','00021','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00021-1','六病房-內藥品','0','3','E60801','00021-1','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00021-2','六病房-外藥品','0','3','E60801','00021-2','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00021-3','六病房-婦藥品','0','3','E60801','00021-3','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00021-4','六病房-兒藥品','0','3','E60801','00021-4','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00022','耳鼻喉科藥品','0','4','E60801','00022','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00023','五病房藥品','0','3','E60801','00023','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00024','感控中心藥品','0','4','E60801','00024','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00025','九病房藥品','0','3','E60801','00025','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00026','洗腎中心藥品','0','4','E60801','00026','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00027','傳統醫學科藥品','0','4','E60801','00027','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E00028','採檢中心藥品','0','4','E60801','00028','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E08100','護理科藥品','0','4','E60801','08100','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E08100-1','護理行政藥品','0','4','E60801','08100-1','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E60801-1','藥事科1-內藥品','0','4','E60801','60801-1','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E60801-2','藥事科1-外藥品','0','4','E60801','60801-2','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E60801-3','藥事科1-婦藥品','0','4','E60801','60801-3','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E60801-4','藥事科1-皮膚科藥品','0','4','E60801','60801-4','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E60801-5','藥事科1-復健科藥品','0','4','E60801','60801-5','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('560000','衛材庫','1','1','','60801','','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('60801','藥事科1衛材','1','2','560000','60801','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00001','門診衛材','1','2','560000','00001','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00001-1','門診-內衛材','1','2','560000','00001-1','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00001-2','門診-外衛材','1','2','560000','00001-2','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00001-3','門診-婦衛材','1','2','560000','00001-3','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00001-4','門診-兒衛材','1','2','560000','00001-4','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00001-5','門診-身心科衛材','1','2','560000','00001-5','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00002','急診衛材','1','2','560000','00002','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00002-1','急診-內衛材','1','2','560000','00002-1','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00002-2','急診-外衛材','1','2','560000','00002-2','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00002-3','急診-婦衛材','1','2','560000','00002-3','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00002-4','急診-兒衛材','1','2','560000','00002-4','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00003','放射科衛材','1','2','560000','00003','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00004','檢驗科衛材','1','2','560000','00004','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00005','三病房衛材','1','2','560000','00005','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('0000501','三病房-婦衛材','1','2','560000','0000501','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('0000502','三病房-兒衛材','1','2','560000','0000502','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00006','病理科衛材','1','2','560000','00006','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00007','骨科衛材','1','2','560000','00007','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00008','護理之家衛材','1','2','560000','00008','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00009','眼科衛材','1','2','560000','00009','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00010','氣體衛材','1','2','560000','00010','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00010-1','氣體-皮膚科衛材','1','2','560000','00010-1','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00011','開刀房衛材','1','2','560000','00011','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00011-1','開刀房-外衛材','1','2','560000','00011-1','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00011-2','開刀房-婦衛材','1','2','560000','00011-2','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00011-3','開刀房-眼科衛材','1','2','560000','00011-3','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00011-4','開刀房-耳鼻喉科衛材','1','2','560000','00011-4','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00012','醫行科衛材','1','2','560000','00012','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00013','供應中心衛材','1','2','560000','00013','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00014','加護病房衛材','1','2','560000','00014','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00014-1','加護病房-內衛材','1','2','560000','00014-1','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00014-2','加護病房-外衛材','1','2','560000','00014-2','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00015','身心科衛材','1','2','560000','00015','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00015-1','身心科-二病房衛材','1','2','560000','00015-1','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00015-2','身心科-九病房衛材','1','2','560000','00015-2','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00016','麻醉科衛材','1','2','560000','00016','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00017','牙科衛材','1','2','560000','00017','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00018','復健科衛材','1','2','560000','00018','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00019','體檢中心衛材','1','2','560000','00019','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00020','居家護理衛材','1','2','560000','00020','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00021','六病房衛材','1','2','560000','00021','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00021-1','六病房-內衛材','1','2','560000','00021-1','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00021-2','六病房-外衛材','1','2','560000','00021-2','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00021-3','六病房-婦衛材','1','2','560000','00021-3','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00021-4','六病房-兒衛材','1','2','560000','00021-4','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00022','耳鼻喉科衛材','1','2','560000','00022','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00023','五病房衛材','1','2','560000','00023','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00024','感控中心衛材','1','2','560000','00024','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00025','九病房衛材','1','2','560000','00025','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00026','洗腎中心衛材','1','2','560000','00026','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00027','傳統醫學科衛材','1','2','560000','00027','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('00028','採檢中心衛材','1','2','560000','00028','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('08100','護理科衛材','1','2','560000','08100','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('08100-1','護理行政衛材','1','2','560000','08100-1','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('60801-1','藥事科1-內衛材','1','2','560000','60801-1','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('60801-2','藥事科1-外衛材','1','2','560000','60801-2','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('60801-3','藥事科1-婦衛材','1','2','560000','60801-3','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('60801-4','藥事科1-皮膚科衛材','1','2','560000','60801-4','60801','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('60801-5','藥事科1-復健科衛材','1','2','560000','60801-5','60801','N',sysdate,'上線轉檔');	
";
        }

        private string GetWhno818()
        {
            return @"
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E66010','藥庫','0','1','','66010','','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E66020','山上藥局','0','2','E66010','66020','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E66030','山下藥局','0','2','E66010','66030','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E01000','精神科藥品','0','4','E66020','01000','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E01010','臨床診療科藥品','0','4','E66020','01010','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E01020','青少年心理衛生科藥品','0','4','E66020','01020','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E01030','身心科門診藥品','0','4','E66020','01030','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E01040','急診藥品','0','4','E66020','01040','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E01050','心理科藥品','0','4','E66020','01050','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E01060','社工科藥品','0','4','E66020','01060','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E01070','職治科藥品','0','4','E66020','01070','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E01080','社區復健中心藥品','0','4','E66020','01080','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E01090','自律神經失調門診藥品','0','4','E66020','01090','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E01100','憂鬱症門診藥品','0','4','E66020','01100','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E02000','家庭醫學科藥品','0','4','E66020','02000','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E02010','家醫科急診藥品','0','4','E66020','02010','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E02020','體檢科藥品','0','4','E66020','02020','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E02030','檢驗科藥品','0','4','E66020','02030','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E02040','放射科藥品','0','4','E66020','02040','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E02050','心臟內科藥品','0','4','E66020','02050','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E02060','腸胃內科藥品','0','4','E66020','02060','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E02070','皮膚科藥品','0','4','E66020','02070','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E02080','神經科藥品','0','4','E66020','02080','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E02090','腦波室藥品','0','4','E66020','02090','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E02100','感控室藥品','0','4','E66020','02100','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E04000','牙科藥品','0','4','E66020','04000','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E04010','牙科急診藥品','0','4','E66020','04010','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50000','門診藥品','0','4','E66020','50000','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50001','W1病房藥品','0','3','E66020','50001','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50012','W12病房藥品','0','3','E66020','50012','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50031','W21病房藥品','0','3','E66020','50031','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50032','W22病房藥品','0','3','E66020','50032','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50033','W3病房藥品','0','3','E66020','50033','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50034','W23病房藥品','0','3','E66020','50034','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50035','病房藥品','0','3','E66020','50035','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50051','W5病房藥品','0','3','E66020','50051','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50061','W6病房藥品','0','3','E66020','50061','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50071','W7病房藥品','0','3','E66020','50071','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50081','W8病房藥品','0','3','E66020','50081','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50091','W9病房藥品','0','3','E66020','50091','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50101','日間病房藥品','0','3','E66020','50101','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50102','居家藥品','0','4','E66020','50102','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50103','門診(已刪)藥品','0','4','E66020','50103','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50111','W11病房藥品','0','3','E66020','50111','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50112','W2病房藥品','0','3','E66020','50112','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E50113','護理科藥品','0','4','E66020','50113','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E66000','藥事科藥品','0','4','E66020','66000','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E60010','院(副)長室藥品','0','4','E66020','60010','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E60020','醫療部藥品','0','4','E66020','60020','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E60030','民診處藥品','0','4','E66020','60030','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E60040','政戰處藥品','0','4','E66020','60040','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E60050','主計部門藥品','0','4','E66020','60050','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E603310','遺傳實驗室藥品','0','4','E66020','603310','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E61000','醫行室藥品','0','4','E66020','61000','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E61010','服務台藥品','0','4','E66020','61010','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E61020','掛號室藥品','0','4','E66020','61020','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E61030','病歷室藥品','0','4','E66020','61030','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E61040','山上掛批室藥品','0','4','E66020','61040','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E61050','書記室藥品','0','4','E66020','61050','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E61060','人事室藥品','0','4','E66020','61060','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E61070','文卷室藥品','0','4','E66020','61070','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E61080','醫務室藥品','0','4','E66020','61080','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E61090','後勤室藥品','0','4','E66020','61090','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E61100','總機房藥品','0','4','E66020','61100','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E61110','官兵餐廳藥品','0','4','E66020','61110','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E61120','勤務隊藥品','0','4','E66020','61120','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E61130','宿舍藥品','0','4','E66020','61130','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E61140','圖書館藥品','0','4','E66020','61140','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E61150','營養室藥品','0','4','E66020','61150','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E61160','教研室藥品','0','4','E66020','61160','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E64000','資訊室藥品','0','4','E66020','64000','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E66040','TDM藥品','0','4','E66020','66040','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E90000','其他部門藥品','0','4','E66020','90000','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('E90010','快樂商店藥品','0','4','E66020','90010','66020','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('66010','衛材庫','1','1','','66010','','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('01000','精神科衛材','1','2','66010','01000','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('01010','臨床診療科衛材','1','2','66010','01010','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('01020','青少年心理衛生科衛材','1','2','66010','01020','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('01030','身心科門診衛材','1','2','66010','01030','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('01040','急診衛材','1','2','66010','01040','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('01050','心理科衛材','1','2','66010','01050','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('01060','社工科衛材','1','2','66010','01060','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('01070','職治科衛材','1','2','66010','01070','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('01080','社區復健中心衛材','1','2','66010','01080','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('01090','自律神經失調門診衛材','1','2','66010','01090','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('01100','憂鬱症門診衛材','1','2','66010','01100','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('02000','家庭醫學科衛材','1','2','66010','02000','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('02010','家醫科急診衛材','1','2','66010','02010','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('02020','體檢科衛材','1','2','66010','02020','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('02030','檢驗科衛材','1','2','66010','02030','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('02040','放射科衛材','1','2','66010','02040','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('02050','心臟內科衛材','1','2','66010','02050','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('02060','腸胃內科衛材','1','2','66010','02060','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('02070','皮膚科衛材','1','2','66010','02070','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('02080','神經科衛材','1','2','66010','02080','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('02090','腦波室衛材','1','2','66010','02090','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('02100','感控室衛材','1','2','66010','02100','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('04000','牙科衛材','1','2','66010','04000','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('04010','牙科急診衛材','1','2','66010','04010','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50000','門診衛材','1','2','66010','50000','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50001','W1病房衛材','1','2','66010','50001','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50012','W12病房衛材','1','2','66010','50012','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50031','W21病房衛材','1','2','66010','50031','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50032','W22病房衛材','1','2','66010','50032','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50033','W3病房衛材','1','2','66010','50033','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50034','W23病房衛材','1','2','66010','50034','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50035','病房衛材','1','2','66010','50035','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50051','W5病房衛材','1','2','66010','50051','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50061','W6病房衛材','1','2','66010','50061','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50071','W7病房衛材','1','2','66010','50071','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50081','W8病房衛材','1','2','66010','50081','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50091','W9病房衛材','1','2','66010','50091','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50101','日間病房衛材','1','2','66010','50101','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50102','居家衛材','1','2','66010','50102','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50103','門診(已刪)衛材','1','2','66010','50103','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50111','W11病房衛材','1','2','66010','50111','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50112','W2病房衛材','1','2','66010','50112','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('50113','護理科衛材','1','2','66010','50113','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('66000','藥事科衛材','1','2','66010','66000','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('60010','院(副)長室衛材','1','2','66010','60010','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('60020','醫療部衛材','1','2','66010','60020','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('60030','民診處衛材','1','2','66010','60030','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('60040','政戰處衛材','1','2','66010','60040','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('60050','主計部門衛材','1','2','66010','60050','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('603310','遺傳實驗室衛材','1','2','66010','603310','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('61000','醫行室衛材','1','2','66010','61000','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('61010','服務台衛材','1','2','66010','61010','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('61020','掛號室衛材','1','2','66010','61020','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('61030','病歷室衛材','1','2','66010','61030','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('61040','山上掛批室衛材','1','2','66010','61040','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('61050','書記室衛材','1','2','66010','61050','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('61060','人事室衛材','1','2','66010','61060','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('61070','文卷室衛材','1','2','66010','61070','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('61080','醫務室衛材','1','2','66010','61080','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('61090','後勤室衛材','1','2','66010','61090','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('61100','總機房衛材','1','2','66010','61100','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('61110','官兵餐廳衛材','1','2','66010','61110','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('61120','勤務隊衛材','1','2','66010','61120','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('61130','宿舍衛材','1','2','66010','61130','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('61140','圖書館衛材','1','2','66010','61140','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('61150','營養室衛材','1','2','66010','61150','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('61160','教研室衛材','1','2','66010','61160','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('64000','資訊室衛材','1','2','66010','64000','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('66020','山上藥局衛材','1','2','66010','66020','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('66030','山下藥局衛材','1','2','66010','66030','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('66040','TDM衛材','1','2','66010','66040','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('90000','其他部門衛材','1','2','66010','90000','66010','N',sysdate,'上線轉檔');	
insert into MI_WHMAST(WH_NO,WH_NAME,WH_KIND,WH_GRADE,PWH_NO,INID,SUPPLY_INID,CANCEL_ID,CREATE_TIME,CREATE_USER)
 values('90010','快樂商店衛材','1','2','66010','90010','66010','N',sysdate,'上線轉檔');	
";
        }
        #endregion
    }
}
