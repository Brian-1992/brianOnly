using JCLib.DB; //處理跟資料庫連接的函數
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Data;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using WebApp.Models.AB;


namespace WebApp.Repository.AA
{
    public class AA0143Repository : JCLib.Mvc.BaseRepository  //一定要寫
    {
        public AA0143Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        #region GetComboBox
        public IEnumerable<AB0109> GetMmCodeCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = @"select {0} MMCODE,MMNAME_C,MMNAME_E,BASE_UNIT,
                                  decode(M_CONTPRICE,null,UPRICE,M_CONTPRICE) as CR_UPRICE,
                                  M_PAYKIND,M_AGENNO as AGEN_NO,
                                  (select AGEN_NO || ' ' || AGEN_NAMEC 
                                     from PH_VENDER
                                    where AGEN_NO=a.M_AGENNO
                                  ) as AGEN_NAME,
                                 (select EMAIL from PH_VENDER where AGEN_NO = a.M_AGENNO) as EMAIL,
                                 (case when WEXP_ID = 'Y' then 'Y'
                                       when WEXP_ID = 'y' then 'Y'
                                   else 'N'
                                  end) as WEXP_ID,
                                  M_CONTPRICE, UPRICE
                             from MI_MAST a
                            where MAT_CLASS='02'      --衛材
                              and ( (M_APPLYID<>'E')  --申請申購識別碼
                                    or (M_APPLYID='E' and M_CONTID='3') )  --鎖E但是如果是3零購,就可以申請
                              and M_STOREID<>'1'      --庫備識別碼
                              and NVL(CANCEL_ID,'N')='N'  --是否作廢
                              and (M_APPLYID<>'P')  --申請申購識別碼鎖P
                           ";
            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}%", p0));

                sql += " OR MMNAME_E LIKE UPPER(:MMNAME_E) ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR MMNAME_C LIKE :MMNAME_C) ";
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
            return DBWork.Connection.Query<AB0109>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetTOWHcombo(string id)
        {
            string sql = @"select A.WH_NO VALUE, WH_NO||' '||WH_NAME TEXT,
                                  A.WH_NO||' '||A.WH_NAME COMBITEM
                             from MI_WHMAST A
                            where a.WH_KIND='1'
                              and a.wh_grade in ('2')
                              and nvl(a.cancel_id,'N') = 'N'
                            order by a.wh_no
                           ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { TUSER = id }, DBWork.Transaction);
        }
        public IEnumerable<COMBO_MODEL> GetStatusCombo()
        {
            string sql = @"select DATA_VALUE as VALUE,DATA_DESC  as TEXT,
                                  DATA_VALUE||' '||DATA_DESC  as COMBITEM
                             from PARAM_D
                            where GRP_CODE='CR_DOC' and DATA_NAME='CR_STATUS'
                              and DATA_VALUE in ('B','E','F','H')
                            order by DATA_SEQ
                           ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetAgenCombo()
        {
            string sql = @"select AGEN_NO as VALUE,AGEN_NAMEC  as TEXT,
                                  AGEN_NO||' '||AGEN_NAMEC  as COMBITEM
                             from PH_VENDER
                            where REC_STATUS = 'A'--A啟用,X刪除
                            order by AGEN_NO
                           ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        #endregion

        //查詢
        #region 查詢
        public IEnumerable<AB0109> GetAll(string mmcode, string agen_no, string towh, string apptime_start, string apptime_end,
                                          string status, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select CRDOCNO, MMCODE, MMCODE as MmcodeDisplay,MMNAME_C, MMNAME_E,                 
                               APPQTY, APPQTY as AppQtyDisplay, BASE_UNIT, TOWH, TOWH||' '||WH_NAME as WH_NAME,                               
                               TWN_DATE(REQDATE) as REQDATE,TWN_DATE(REQDATE) as ReqDateDisplay, DRNAME, 
                               PATIENTNAME, PATIENTNAME as PatientNameDisplay, 
                               CHARTNO, CHARTNO as ChartNoDisplay, CR_UPRICE,
                               (select DATA_VALUE||' '||DATA_DESC 
                                  from PARAM_D 
                                 where GRP_CODE='MI_MAST' and DATA_NAME='M_PAYKIND'
                                   and DATA_VALUE=a.M_PAYKIND) as M_PAYKIND,
                               (select AGEN_NO||' '||AGEN_NAMEC 
                                  from PH_VENDER 
                                 where AGEN_NO=a.AGEN_NO) as AGEN_NAME,
                               (select INID||' '||INID_NAME from UR_INID where INID=a.INID) as INID,
                               USER_NAME(APPID) as APPNM,
                               twn_time(APPTIME) as APPTIME, EMAIL, 
                               USER_NAME(CREATE_USER) as CREATE_USER , CREATE_TIME,
                               (select DATA_DESC 
                                  from PARAM_D 
                                 where GRP_CODE='CR_DOC'
                                   and DATA_NAME='CR_STATUS' and DATA_VALUE=a.CR_STATUS) as STATUS,CR_STATUS,
                               twn_time(ORDERTIME) as ORDERTIME,  --產生通知單時間
                               twn_time(EMAILTIME) as EMAILTIME,  --寄EMAIL時間
                               twn_time(REPLYTIME) as REPLYTIME  --收信確認時間
                         from CR_DOC a
                        where TWN_DATE(APPTIME)>=:apptime_start
                          and TWN_DATE(APPTIME)<=:apptime_end
                       ";

            if (string.IsNullOrEmpty(mmcode) == false)
            {
                sql += " and a.MMCODE=:mmcode ";
                p.Add(":mmcode", string.Format("{0}", mmcode));
            }
            if (string.IsNullOrEmpty(agen_no) == false)
            {
                sql += " and a.AGEN_NO=:agen_no ";
                p.Add(":agen_no", string.Format("{0}", agen_no));
            }
            if (string.IsNullOrEmpty(towh) == false)
            {
                sql += " and a.TOWH=:towh ";
                p.Add(":towh", string.Format("{0}", towh));
            }
            if (string.IsNullOrEmpty(status) == false)
            {
                sql += " and a.CR_STATUS=:status ";
                p.Add(":status", string.Format("{0}", status));
            }
            p.Add(":apptime_start", apptime_start);
            p.Add(":apptime_end", apptime_end);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AB0109>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        #endregion

        public int Order(string crdocno,string orderid)
        {
            string sql = @"update CR_DOC
                              set CR_STATUS='E',
                                  ORDERTIME=sysdate, 
                                  ORDERID=:ORDERID   
                            where CRDOCNO=:CRDOCNO
                              and CR_STATUS='B' and EMAIL is not null
                           ";
            return DBWork.Connection.Execute(sql, new { CRDOCNO= crdocno, ORDERID = orderid }, DBWork.Transaction);
        }
        public int ReadEmail(string crdocno, string update_user,string update_ip)
        {
            string sql = @"update CR_DOC a
                              set EMAIL=(select EMAIL from PH_VENDER where AGEN_NO=a.AGEN_NO),
                                  UPDATE_TIME=sysdate, 
                                  UPDATE_USER=:UPDATE_USER,
                                  UPDATE_IP=:UPDATE_IP
                            where CRDOCNO=:CRDOCNO
                              and exists(select 1 from PH_VENDER where AGEN_NO=a.AGEN_NO)
                           ";
            return DBWork.Connection.Execute(sql, new { CRDOCNO = crdocno, UPDATE_USER = update_user, UPDATE_IP= update_ip }, DBWork.Transaction);
        }
        public int SendMail(string crdocno, string update_user, string update_ip)
        {
            string sql = @"update CR_DOC 
                              set CR_STATUS='G',
                                  UPDATE_TIME=sysdate, 
                                  UPDATE_USER=:UPDATE_USER,
                                  UPDATE_IP=:UPDATE_IP
                            where CRDOCNO=:CRDOCNO
                              and (CR_STATUS='F' or CR_STATUS='H')
                           ";
            return DBWork.Connection.Execute(sql, new { CRDOCNO = crdocno, UPDATE_USER = update_user, UPDATE_IP = update_ip }, DBWork.Transaction);
        }

        //日報表
        #region 日報表
        public IEnumerable<AB0109> GetReport(string apptime_start, string apptime_end,int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select (case when substr(twn_time(APPTIME),8,6)<'160000'
                                     then twn_date(APPTIME) else twn_date(APPTIME+1)
                                end) as rptdate,  --報表日期
                               1 as orderby,
                               twn_time(APPTIME) as APPTIME,  --申請時間
                               CRDOCNO,  --醫緊急療出貨單編號
                               MMCODE,  --院內碼
                               MMNAME_C,  --中文品名
                               MMNAME_E,  --英文品名
                               AGEN_NO, --廠商代碼
                               (select AGEN_NO||' '||AGEN_NAMEC 
                                  from PH_VENDER 
                                 where AGEN_NO=a.AGEN_NO) as AGEN_NAME, --廠商名稱                  
                               to_char(CR_UPRICE) as CR_UPRICE,  --單價
                               to_char(APPQTY) as APPQTY,  --申請數量
                               (CR_UPRICE*APPQTY) as APP_AMOUNT,  --申請金額
                               (select DATA_VALUE||' '||DATA_DESC 
                                  from PARAM_D 
                                 where GRP_CODE='MI_MAST' and DATA_NAME='M_PAYKIND'
                                   and DATA_VALUE=a.M_PAYKIND) as M_PAYKIND,  --收費屬性
                               TOWH||' '||WH_NAME as WH_NAME,  --入庫庫房
                               INID,  --庫房責任中心
                               INID_NAME(INID) as INID_NAME,  --責任中心名稱
                               USER_NAME(APPID) as APPNM  --申請人
                          from CR_DOC a
                         where twn_time(APPTIME) > twn_time((twn_todate(:apptime_start||'160000')-1)) 
                           and twn_time(APPTIME) <= (:apptime_end||'160000')
                           and CR_STATUS in ('B','F','H')
                         union 
                        select '' as rptdate,
                                  2 as orderby,
                                  '' as APPTIME,  --申請時間
                                  '' as CRDOCNO,  --醫緊急療出貨單編號
                                  '' as MMCODE,  --院內碼
                                  '總金額' MMNAME_C,  --中文品名
                                  '' as MMNAME_E,  --英文品名
                                  '' as AGEN_NO,  --廠商代碼
                                  '' as AGEN_NAME,  --廠商名稱
                                  '' as CR_UPRICE,  --單價
                                  '' as APPQTY,  --申請數量
                                  sum(APP_AMOUNT) as APP_AMOUNT,  --申請總金額
                                  '' as M_PAYKIND,  --收費屬性
                                  '' as TOWH,  --入庫庫房
                                  '' as INID,
                                  '' as INIDNAME,
                                  '' as APPID  --申請人
                             from (select (CR_UPRICE*APPQTY) as APP_AMOUNT  --申請金額
                                     from CR_DOC a
                                    where 1=1
                                      and twn_time(APPTIME) >  twn_time(twn_todate(:apptime_start||'160000')-1)
                                      and twn_time(APPTIME) <= (:apptime_end||'160000')         
                                      and CR_STATUS in ('B','F','H')
                                    ) P                            
                         order by orderby,APPTIME,INID
                       ";

            p.Add(":apptime_start", apptime_start);
            p.Add(":apptime_end", apptime_end);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AB0109>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        #endregion
   
        private string GetSql()
        {
            string sql = @"select (case when substr(twn_time(APPTIME),8,6)<'160000'
                                   then twn_date(APPTIME) else twn_date(APPTIME+1)
                                   end) as F01,--報表日期,
                                  1 as F02, --orderby,  --不顯示
                                  twn_time(APPTIME) as F03,--申請時間,
                                  CRDOCNO as F04,--醫緊急療出貨單編號,
                                  MMCODE as F05,--院內碼,
                                  MMNAME_C as F06,--中文品名,
                                  MMNAME_E as F07,--英文品名,
                                  AGEN_NO as F08,--廠商代碼,
                                  (select AGEN_NAMEC 
                                     from PH_VENDER 
                                    where AGEN_NO=a.AGEN_NO) as F09,--廠商名稱,    
                                 to_char(CR_UPRICE) as F10,  --單價
                                 to_char(APPQTY) as F11,  --申請數量
                                  (CR_UPRICE*APPQTY) as F12,--申請金額,
                                  (select DATA_VALUE||' '||DATA_DESC 
                                     from PARAM_D 
                                    where GRP_CODE='MI_MAST' and DATA_NAME='M_PAYKIND'
                                      and DATA_VALUE=a.M_PAYKIND) as F13,--收費屬性,
                                  TOWH||' '||WH_NAME as F14,--入庫庫房,
                                  INID as F15,  --庫房責任中心
                                  INID_NAME(INID) as F16,--INIDNAME,  --責任中心名稱
                                  USER_NAME(APPID) as F17 --申請人
                             from CR_DOC a
                            where twn_time(APPTIME) > twn_time((twn_todate(:apptime_start||'160000')-1))
                              and twn_time(APPTIME) <= (:apptime_end||'160000')
                              and CR_STATUS in ('B','F','H')
                            union
                           select rptdate as F01,
                                  2 as F02,--orderby,
                                  '' as F03,--APPTIME,  --申請時間
                                  '' as F04,--CRDOCNO,  --醫緊急療出貨單編號
                                  '' as F05,--MMCODE,  --院內碼
                                  '總金額' as F06,--MMNAME_C,  --中文品名
                                  '' as F07,--MMNAME_E,  --英文品名
                                  '' as F08,--AGEN_NO,  --廠商代碼
                                  '' as F09,--AGEN_NAME,  --廠商名稱
                                  '' as F10,--CR_UPRICE,  --單價
                                  '' as F11,--APPQTY,  --申請數量
                                  sum(APP_AMOUNT) as F12,--APP_AMOUNT,  --申請總金額
                                  '' as F13,--M_PAYKIND,  --收費屬性
                                  '' as F14,--TOWH,  --入庫庫房
                                  '' as F15,--INID,
                                  '' as F16,--INIDNAME,
                                  '' as F17 --APPID  --申請人
                             from (select (case when substr(twn_time(APPTIME),8,6)<'160000'
                                           then twn_date(APPTIME) else twn_date(APPTIME+1)
                                           end) as rptdate,  --報表日期,以此區分sheet
                                           (CR_UPRICE*APPQTY) as APP_AMOUNT  --申請金額
                                     from CR_DOC a
                                    where 1=1
                                      and twn_time(APPTIME) >  twn_time(twn_todate(:apptime_start||'160000')-1)
                                      and twn_time(APPTIME) <= (:apptime_end||'160000')         
                                      and CR_STATUS in ('B','F','H')
                                    ) P
                            group by rptdate
                            order by F01,F02,F15,F05      
                      ";     
            return sql;
        }

        #region GetExcel
        public DataTable GetExcel(string apptime_start, string apptime_end)
        {
            DynamicParameters p = new DynamicParameters();
            
            string sql = string.Format(@"
                select F01 as 報表日期, F03 as 申請時間,
                       F04 as 醫緊急療出貨單編號, F05 as 院內碼, 
                       F06 as 中文品名, F07 as 英文品名, F08 as 廠商代碼, 
                       F09 as 廠商名稱, F10 as 單價, F11 as 申請數量,
                       F12 as 申請金額, F13 as 收費屬性, F14 as 入庫庫房,
                       F15 as 庫房責任中心,F16 as 責任中心名稱,
                       F17 as 申請人
                  from (
                        {0}
                       )
            ", GetSql());

            p.Add(":apptime_start", string.Format("{0}", apptime_start));
            p.Add(":apptime_end", string.Format("{0}",apptime_end));

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
        #endregion
    }
}