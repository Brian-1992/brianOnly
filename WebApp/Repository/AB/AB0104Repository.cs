using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp.Models;
using System.Data;

namespace WebApp.Repository.AB
{
    public class AB0104Repository : JCLib.Mvc.BaseRepository
    {
        public AB0104Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AB0104> GetAll(string towh, string frwh, string start_date, string end_date, string docno, string flowid, string mmcode, bool is_ab) {
            DynamicParameters p = new DynamicParameters();
            string sql = @"
                      select a.TOWH,b.MMCODE,
                             (select MMNAME_C from MI_MAST where MMCODE=b.MMCODE) as MMNAME_C,
                             (select MMNAME_E from MI_MAST where MMCODE=b.MMCODE) as MMNAME_E,  
                             (select UI_CHANAME from MI_UNITCODE 
                               where UNIT_CODE=(select BASE_UNIT from MI_MAST 
                                                 where MMCODE=b.MMCODE)) as BASE_UNIT,    
                             TWN_DATE(a.APPTIME) as apptime,USER_NAME(a.APPID) as appid,b.APPQTY,
                             a.FRWH,
                             TWN_DATE(b.APVTIME) as apvtime,USER_NAME(b.APVID) as apvid,b.APVQTY,
                             TWN_DATE(b.ACKTIME) as acktime,USER_NAME(b.ACKID) as ackid,b.ACKQTY,
                             (b.ACKQTY-b.TRNAB_QTY) as rcvqty,
                             b.TRNAB_QTY,
                             (select DATA_VALUE||' '||DATA_DESC from PARAM_D 
                               where GRP_CODE='ME_DOCD' and DATA_NAME='TRNAB_RESON' 
                                 and DATA_VALUE=b.TRNAB_RESON) as trnab_reson,      
                             a.DOCNO,
                             (select FLOWID||' '||FLOWNAME from V_FLOW 
                               where DOCTYPE in ('TR','TR1') 
                                 and FLOWID=a.FLOWID and rownum=1) as flowid,
                             INID_NAME(a.APPDEPT) as appdept 
                      from 
                        (select DOCNO,FLOWID,APPID,APPDEPT,APPTIME,FRWH,TOWH
                           from ME_DOCM
                          where DOCTYPE in('TR','TR1')";
            if (towh != string.Empty) {
                sql += "    and towh = :towh";
            }
            if (frwh != string.Empty)
            {
                sql += "    and frwh = :frwh";
            }
            if (start_date != string.Empty)
            {
                sql += "    and twn_date(apptime) >= :start_date";
            }
            if (end_date != string.Empty)
            {
                sql += "    and twn_date(apptime) <= :end_date";
            }
            if (docno != string.Empty) {
                sql += "    and docno like :docno";

            }
            if (flowid != string.Empty) {
                sql += "    and flowid = :flowid";
            }

            sql += @"   
                   ) a
                inner join  
                  (select DOCNO,MMCODE,APPQTY,
                          APVQTY,APVTIME,APVID,
                          ACKQTY,ACKID,ACKTIME,
                          TRNAB_QTY,TRNAB_RESON
                     from ME_DOCD
                    where 1=1";
            if (mmcode != string.Empty) {
                sql += "    and mmcode = :mmcode";
            }
            if (docno != string.Empty) {
                sql += "    and docno like :docno";
            }
            if (is_ab) {
                sql += "    and trnab_qty > 0";
            }

                sql +=  @" 
                   ) b
                on a.DOCNO=b.DOCNO
                order by b.MMCODE
            ";

            p.Add(":towh", towh);
            p.Add(":frwh", frwh);
            p.Add(":start_date", start_date);
            p.Add(":end_date", end_date);
            p.Add(":docno", string.Format("{0}%", docno));
            p.Add(":flowid", flowid);
            p.Add(":mmcode", mmcode);

            return DBWork.PagingQuery<AB0104>(sql, p, DBWork.Transaction);
        }


        #region combo
        public IEnumerable<COMBO_MODEL> GetTowhCombo(string userid) {
            string sql = @"select A.WH_NO VALUE,WH_NO||' '||WH_NAME TEXT
                             from MI_WHMAST A
                            where WH_KIND='0'  --藥品庫
                              and EXISTS(select 'X' from MI_WHID B where A.WH_NO=B.WH_NO and WH_USERID=:userid)
                           union all
                           select A.WH_NO VALUE,WH_NO||' '||WH_NAME TEXT 
                             from MI_WHMAST A
                            where WH_KIND='1'  --衛材庫
                             and exists(select 'X' from UR_ID B where (A.SUPPLY_INID=B.INID OR A.INID=B.INID) and TUSER=:userid)
                             and not exists
                                   (select 'X' from MI_WHID B
                                     where TASK_ID IN ('2','3') and WH_USERID=:userid)
                           union all 
                            select A.WH_NO VALUE,A.WH_NO||' '||WH_NAME 
                              from MI_WHMAST A,MI_WHID B  --衛材庫
                             where A.WH_NO=B.WH_NO and TASK_ID IN ('2','3') and WH_USERID=:userid
                           order by VALUE
                           ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { userid = userid}, DBWork.Transaction);
        }
        public IEnumerable<COMBO_MODEL> GetFrwhCombo()
        {
            string sql = @"select WH_NO VALUE,WH_NO||' '||WH_NAME TEXT 
                             from MI_WHMAST A 
                            where WH_KIND='0'
                           union all
                           select WH_NO VALUE,WH_NO||' '||WH_NAME TEXT 
                             from MI_WHMAST A 
                            where WH_KIND='1'
                           order by VALUE
                           ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, string wh_no, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, a.mmname_c, a.mmname_e
                        FROM MI_MAST a, MI_WINVCTL b
                        WHERE 1=1 
                          and b.mmcode = a.mmcode
                          and b.wh_no = :wh_no";
            p.Add(":wh_no", wh_no);

            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(A.MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(A.MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("%{0}%", p0));

                sql += " OR A.MMNAME_E LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR A.MMNAME_C LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY A.MMCODE ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetFlowidCombo() {
            string sql = @"select distinct flowid as value, flowname as text
                             from V_FLOW
                            where doctype in ('TR', 'TR1')
                            order by flowid";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, DBWork.Transaction);
        }
        #endregion


        //匯出
        public DataTable GetExcel(string towh, string frwh, string start_date, string end_date, string docno, string flowid, string mmcode, bool is_ab)
        {
            var p = new DynamicParameters();
            string sql = @"
                      select a.TOWH AS 調入庫房,b.MMCODE AS 院內碼,
                             (select MMNAME_C from MI_MAST where MMCODE=b.MMCODE) as 中文品名,
                             (select MMNAME_E from MI_MAST where MMCODE=b.MMCODE) as 英文品名,  
                             (select UI_CHANAME from MI_UNITCODE 
                               where UNIT_CODE=(select BASE_UNIT from MI_MAST 
                                                 where MMCODE=b.MMCODE)) as 計量單位,    
                             TWN_DATE(a.APPTIME) as 申請日期,USER_NAME(a.APPID) as 申請人員,b.APPQTY AS 申請數量,
                             a.FRWH AS 調出庫房,
                             TWN_DATE(b.APVTIME) as 調出日期,USER_NAME(b.APVID) as 核撥人員,b.APVQTY AS 調出數量,
                             TWN_DATE(b.ACKTIME) as 調入日期,USER_NAME(b.ACKID) as 點收人員,b.ACKQTY AS 調入數量,
                             (b.ACKQTY-b.TRNAB_QTY) as 實際調入數量,
                             b.TRNAB_QTY AS 調撥短少數量,
                             (select DATA_VALUE||' '||DATA_DESC from PARAM_D 
                               where GRP_CODE='ME_DOCD' and DATA_NAME='TRNAB_RESON' 
                                 and DATA_VALUE=b.TRNAB_RESON and rownum=1) as 調撥異常原因,      
                             a.DOCNO AS 申請單號,
                             (select FLOWID||' '||FLOWNAME from V_FLOW 
                               where DOCTYPE in ('TR','TR1') 
                                 and FLOWID=a.FLOWID and rownum=1) as 申請單狀態,
                             INID_NAME(a.APPDEPT) as 申請部門 
                      from 
                        (select DOCNO,FLOWID,APPID,APPDEPT,APPTIME,FRWH,TOWH
                           from ME_DOCM
                          where DOCTYPE in('TR','TR1')";
            if (towh != string.Empty)
            {
                sql += "    and towh = :towh";
            }
            if (frwh != string.Empty)
            {
                sql += "    and frwh = :frwh";
            }
            if (start_date != string.Empty)
            {
                sql += "    and twn_date(apptime) >= :start_date";
            }
            if (end_date != string.Empty)
            {
                sql += "    and twn_date(apptime) <= :end_date";
            }
            if (docno != string.Empty)
            {
                sql += "    and docno like :docno";

            }
            if (flowid != string.Empty)
            {
                sql += "    and flowid = :flowid";
            }

            sql += @"   
                   ) a
                inner join  
                  (select DOCNO,MMCODE,APPQTY,
                          APVQTY,APVTIME,APVID,
                          ACKQTY,ACKID,ACKTIME,
                          TRNAB_QTY,TRNAB_RESON
                     from ME_DOCD
                    where 1=1";
            if (mmcode != string.Empty)
            {
                sql += "    and mmcode = :mmcode";
            }
            if (docno != string.Empty)
            {
                sql += "    and docno like :docno";
            }
            if (is_ab)
            {
                sql += "    and trnab_qty > 0";
            }

            sql += @" 
                   ) b
                on a.DOCNO=b.DOCNO
                order by b.MMCODE
            ";

            p.Add(":towh", towh);
            p.Add(":frwh", frwh);
            p.Add(":start_date", start_date);
            p.Add(":end_date", end_date);
            p.Add(":docno", string.Format("{0}%", docno));
            p.Add(":flowid", flowid);
            p.Add(":mmcode", mmcode);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

    }
}