using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTransfer14
{
    public class DataTransferBasero14Repository
    {
        #region HISDG
        #region 取得XFBASERO, XFCHEMIS, XFDAYRO, XFOPTION資料
        public string Get_XFBASERO(string hosp_table_prefix, string startDate, string endDate)
        {
            var aa = startDate.Substring(0, 5);
            string sql = string.Format(@"
                                        SELECT *
                                        FROM   HISDG.{0}XFBASERO 
                                        WHERE STRSPACE  between '{1}' and '{2}' 
                                    ", hosp_table_prefix, endDate.Substring(0, 5), endDate.Substring(0, 5)); //只撈當月份
            return sql;
        }
        public string Get_XFCHEMIS(string hosp_table_prefix)
        {
            string sql = string.Format(@"
                                        SELECT *
                                        FROM   HISDG.{0}XFCHEMIS
                                    ", hosp_table_prefix);
            return sql;
        }
        public string Get_XFDAYRO(string hosp_table_prefix, string startDate, string endDate)
        {
            string sql = string.Format(@"
                                        SELECT *
                                        FROM   HISDG.{0}XFDAYRO 
                                        WHERE STRDATE  between {1} and {2} 
                                    ", hosp_table_prefix, startDate, endDate);
            return sql;
        }
        public string Get_XFOPTION(string hosp_table_prefix)
        {
            string sql = string.Format(@"
                                        SELECT *
                                        FROM   HISDG.{0}XFOPTION
                                    ", hosp_table_prefix);
            return sql;
        }
        #endregion
        #endregion

        #region MMSMS
        #region 刪除原MI_BASERO_14, XFBASERO, XFCHEMIS, XFDAYRO, XFOPTION資料
        public string Delete_XFBASERO(string startDate, string endDate)
        {
            string sql = string.Format(@" DELETE FROM MMSADM.XFBASERO 
                                                              WHERE STRSPACE  between '{0}' and '{1}' ", endDate.Substring(0, 5), endDate.Substring(0, 5)); //只刪除當月份

            return sql;
        }
        public string Delete_XFCHEMIS()
        {
            string sql = @" DELETE FROM MMSADM.XFCHEMIS ";
            return sql;
        }
        public string Delete_XFDAYRO(string startDate, string endDate)
        {
            string sql = string.Format(@" DELETE FROM MMSADM.XFDAYRO 
                                                              WHERE STRDATE  between '{0}' and '{1}' ", startDate, endDate);

            return sql;
        }
        public string Delete_XFOPTION()
        {
            string sql = @" DELETE FROM MMSADM.XFOPTION ";
            return sql;
        }
        public string Delete_MI_BASERO_14()
        {
            string sql = @" DELETE FROM MMSADM.MI_BASERO_14 ";
            return sql;
        }
        #endregion
        #region 複製來自HISDG資料
        public string Insert_XFBASERO()
        {
            string sql = @"
                Insert into XFBASERO 
(STRHOSPITAL,STRDRUGID,LNGWASTMONTH1,LNGWASTMONTH2,LNGWASTMONTH3,
LNGWASTMONTH4,LNGWASTMONTH5,LNGWASTMONTH6,LNGWASTAVG30,LNGWASTAVG60,
LNGORGBASE60,LNGNEWBASE60,LNGSETRO1,LNGSETRO2,LNGSETRO3,LNGSETRO4,LNGSETRO5,
LNGSPACE,STRSPACE,STRSTOREDEP,LNGEARRO,STRISDELETE,STRLASTUPDATE,STROPERATERID,STRDEPID,STRUSEDAY) 
values (:STRHOSPITAL,:STRDRUGID,:LNGWASTMONTH1,:LNGWASTMONTH2,:LNGWASTMONTH3,:LNGWASTMONTH4,:LNGWASTMONTH5,:LNGWASTMONTH6,:LNGWASTAVG30,:LNGWASTAVG60,:LNGORGBASE60,:LNGNEWBASE60,:LNGSETRO1,:LNGSETRO2,:LNGSETRO3,:LNGSETRO4,:LNGSETRO5,:LNGSPACE,:STRSPACE,:STRSTOREDEP,:LNGEARRO,:STRISDELETE,:STRLASTUPDATE,:STROPERATERID,:STRDEPID,:STRUSEDAY) ";
            return sql;
        }

        public string Insert_XFCHEMIS()
        {
            string sql = @"
                Insert into XFCHEMIS 
(STRHOSPITAL,STRDRUGID,STRDRUGENG,STRDRUGCHI,STRHEALTHID,STRXFKINDID,
STRDRUGUNIT,IUNITRATE,STRCOMMON,STRDRUGSNAME,STRDRUGPACK,STRDRUGHIDE,STRSPACE,
LNGSPACE,STRWARBAK,LNGTRUTRATE,STRCOUNTCOST,STRISLIMIT,STRISLIMITCLAS,STRCASENO,STRBARCODE,
STRONECOST,STRHEALTHPAY,STRCOSTKIND,STRISDELETE,STRLASTUPDATE,STROPERATERID,STRDGUNIT,STRWILLCANCEL,
STRSPDRUG,STRORDERKIND,STRFASTDRUG,STRCASEDOCT,STRWASTKIND,STRSPXFEE,STRDRUGISSUENO,STRDRUGISSUEDATE,
STRISSUESUPPLY,STRDRUGMAKER,STRHEALTHOWNEXP,STRDRUGKIND) values (:STRHOSPITAL,:STRDRUGID,:STRDRUGENG,:STRDRUGCHI,:STRHEALTHID,:STRXFKINDID,:STRDRUGUNIT,:IUNITRATE,:STRCOMMON,:STRDRUGSNAME,:STRDRUGPACK,:STRDRUGHIDE,:STRSPACE,:LNGSPACE,:STRWARBAK,:LNGTRUTRATE,:STRCOUNTCOST,:STRISLIMIT,:STRISLIMITCLAS,:STRCASENO,:STRBARCODE,:STRONECOST,:STRHEALTHPAY,:STRCOSTKIND,:STRISDELETE,:STRLASTUPDATE,:STROPERATERID,:STRDGUNIT,:STRWILLCANCEL,:STRSPDRUG,:STRORDERKIND,:STRFASTDRUG,:STRCASEDOCT,:STRWASTKIND,:STRSPXFEE,:STRDRUGISSUENO,:STRDRUGISSUEDATE,:STRISSUESUPPLY,:STRDRUGMAKER,:STRHEALTHOWNEXP,:STRDRUGKIND) ";
            return sql;
        }

        public string Insert_XFDAYRO()
        {
            string sql = @"
                Insert into XFDAYRO 
(STRHOSPITAL,STRDRUGID,STRDATE,STRSTOREDEP,STRDEPID,LNGDIFF,LNGWAST90D,
LNGWAST14D,LNGWAST10D,LNGBASERO,LNGNOWRO,LNGLOWAMT,LNGLOWRATE,STRISDELETE,STRLASTUPDATE,STROPERATERID) 
values (:STRHOSPITAL,:STRDRUGID,:STRDATE,:STRSTOREDEP,:STRDEPID,:LNGDIFF,:LNGWAST90D,:LNGWAST14D,:LNGWAST10D,:LNGBASERO,:LNGNOWRO,:LNGLOWAMT,:LNGLOWRATE,:STRISDELETE,:STRLASTUPDATE,:STROPERATERID) ";
            return sql;
        }

        public string Insert_XFOPTION()
        {
            string sql = @"
                Insert into XFOPTION 
(STRHOSPITAL,STRDEP0LASTVER,STRDEP1LASTVER,STRDEP2LASTVER,STRDEP3LASTVER,
STRDEP4LASTVER,STRDEP5LASTVER,STRDEP6LASTVER,STRDEP7LASTVER,STRDEP8LASTVER,STRDEP9LASTVER,
STRSTARTMONCLOSEDATE,STRENDMONCLOSEDATE,INTSETRORATE1,INTSETRORATE2,INTSETRORATE3,INTSETRORATE4,
INTSETRORATE5,INTSAFERORATE1,INTNORMALRORATE1,INTEARRORATE1,INTSAFERORATE2,INTNORMALRORATE2,INTSAFERORATE3,INTNORMALRORATE3) 
values (:STRHOSPITAL,:STRDEP0LASTVER,:STRDEP1LASTVER,:STRDEP2LASTVER,:STRDEP3LASTVER,:STRDEP4LASTVER,:STRDEP5LASTVER,:STRDEP6LASTVER,:STRDEP7LASTVER,:STRDEP8LASTVER,:STRDEP9LASTVER,:STRSTARTMONCLOSEDATE,:STRENDMONCLOSEDATE,:INTSETRORATE1,:INTSETRORATE2,:INTSETRORATE3,:INTSETRORATE4,:INTSETRORATE5,:INTSAFERORATE1,:INTNORMALRORATE1,:INTEARRORATE1,:INTSAFERORATE2,:INTNORMALRORATE2,:INTSAFERORATE3,:INTNORMALRORATE3) ";
            return sql;
        }

        #endregion
        #region 執行子查詢取得紀錄
        public string GetMergeTable_CSR1(string startDate, string endDate)
        {
            string sql = string.Format(@" SELECT A.STRDRUGID AS MMCODE,DECODE(A.STRUSEDAY,'0','3',A.STRUSEDAY) AS RO_TYPE,
                                     A.LNGNEWBASE60 AS NOW_RO,
                                     A.LNGWASTMONTH1 AS MON_USE_1, A.LNGWASTMONTH2 AS MON_USE_2,
                                     A.LNGWASTMONTH3 AS MON_USE_3,A.LNGWASTMONTH4 AS MON_USE_4,
                                     A.LNGWASTMONTH5 AS MON_USE_5, A.LNGWASTMONTH6 AS MON_USE_6,
                                     A.LNGWASTAVG30 AS MON_AVG_USE_3, A.LNGWASTAVG60 AS MON_AVG_USE_6,
                                     A.STRDEPID AS INID, B.STRXFKINDID, LNGSETRO1, LNGSETRO2,
                                     LNGSETRO3, LNGSETRO4, LNGSETRO5, A.LNGEARRO AS WAR_QTY,
                                     A.STRLASTUPDATE, A.STROPERATERID,
                                     DECODE(B.strXfKindID,'1','0','1') WKIND
                                FROM XFBASERO A, XFCHEMIS B
                               WHERE A.STRDRUGID=B.STRDRUGID(+) AND A.STRHOSPITAL='0' AND B.STRHOSPITAL='0'
                                     AND (A.STRSPACE BETWEEN '{0}' AND '{1}') 
                                     AND A.STRISDELETE='0' ", endDate.Substring(0, 5), endDate.Substring(0, 5)); //指撈當月份
            return sql;
        }
        public string GetWH_CSR2(string inid, string whKind)
        {
            string sql = string.Format(@" SELECT WH_NO,
                               (CASE WHEN (WH_GRADE='1') THEN '1' 
                                 WHEN (WH_KIND='0' AND WH_GRADE = '2') THEN '2'
                                 WHEN (WH_NAME LIKE '%供應中心%') THEN '3'
                                 ELSE NULL END) RO_WHTYPE
                                FROM MI_WHMAST
                               WHERE INID='{0}' AND WH_KIND='{1}'   -- IN ('0','1') 0藥品庫 1衛材庫
                                     AND WH_GRADE IN ('1','2','3','4') ",inid,whKind);
            return sql;
        }
        public string GetLNGWAST_CSR3(string drugId, string depId)
        {
            string sql = string.Format(@" SELECT NVL(LNGWAST10D,0) LNGWAST10D, NVL(LNGWAST14D,0) LNGWAST14D,
                                 NVL(LNGWAST90D,0) LNGWAST90D, NVL(LNGDIFF,0) LNGDIFF
                            FROM XFDAYRO
                           WHERE STRDRUGID='{0}' AND STRDEPID='{1}'
                                 AND STRISDELETE='0'
                           ORDER BY STRDATE DESC ",drugId,depId);
            return sql;
        }
        public string GetOPTION_CSR4()
        {
            string sql = @" SELECT INTSETRORATE1, INTSETRORATE2, INTSETRORATE3, INTSETRORATE4, INTSETRORATE5,
                         INTSAFERORATE1, INTNORMALRORATE1, INTEARRORATE1,
                         INTSAFERORATE2, INTNORMALRORATE2, INTSAFERORATE3, INTNORMALRORATE3
                    FROM XFOPTION
                   WHERE STRHOSPITAL='0' ";
            return sql;
        }
        #endregion
        #region 轉檔寫入MI_BASERO_14
        public string Insert_MI_BASERO_14()
        {
            string sql = @" Insert into MI_BASERO_14 (MMCODE,RO_WHTYPE,RO_TYPE,NOW_RO,
                        DAY_USE_10,DAY_USE_14,DAY_USE_90,MON_USE_1,MON_USE_2,MON_USE_3,MON_USE_4,
                        MON_USE_5,MON_USE_6,MON_AVG_USE_3,MON_AVG_USE_6,G34_MAX_APPQTY,
                        SUPPLY_MAX_APPQTY,PHR_MAX_APPQTY,WAR_QTY,SAFE_QTY,NORMAL_QTY,
                        DIFF_PERC,SAFE_PERC,DAY_RO,MON_RO,G34_PERC,SUPPLY_PERC,PHR_PERC,
                        NORMAL_PERC,WAR_PERC,WH_NO,CREATE_TIME,CREATE_USER,UPDATE_TIME,UPDATE_USER,UPDATE_IP) 
                        values (:MMCODE,:RO_WHTYPE,:RO_TYPE,:NOW_RO,:DAY_USE_10,:DAY_USE_14,:DAY_USE_90,:MON_USE_1,:MON_USE_2,
                        :MON_USE_3,:MON_USE_4,:MON_USE_5,:MON_USE_6,:MON_AVG_USE_3,
                        :MON_AVG_USE_6,:G34_MAX_APPQTY,:SUPPLY_MAX_APPQTY,
                        :PHR_MAX_APPQTY,:WAR_QTY,:SAFE_QTY,:NORMAL_QTY,:DIFF_PERC,:SAFE_PERC,
                        :DAY_RO,:MON_RO,:G34_PERC,:SUPPLY_PERC,:PHR_PERC,:NORMAL_PERC,:WAR_PERC,
                        :WH_NO,TWN_TODATE(:CREATE_TIME),:CREATE_USER,TWN_TODATE(:UPDATE_TIME),:UPDATE_USER,NULL) ";
            return sql;
        }
        #endregion
        #region MI_BASERO_14寫入預設RO比例 (For 818北投)
        public string Update_MI_BASERO_14_RO_818()
        {
            string sql = @"
                begin
                    update MI_BASERO_14 a
                        set  phr_perc = 5,
                         g34_perc = 1 
                       where (select 1 from mi_whmast where a.wh_no = wh_no and wh_grade = '1') = '1';
                end;
            ";
            return sql;
        }
        #endregion
        #endregion
    }
}