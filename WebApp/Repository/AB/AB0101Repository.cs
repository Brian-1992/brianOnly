using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Repository.AB
{
    public class AB0101: JCLib.Mvc.BaseModel {
        public string DOCNO { get; set; }
        public string FLOWID { get; set; }
        public string APPTIME { get; set; }
        public string FRWH { get; set; }
        public string TOWH { get; set; }
        public string MMCODE { get; set; }
        public string SEQ { get; set; }
        public string MMNAME_E { get; set; }
        public string MMNAME_C { get; set; }
        public string BASE_UNIT { get; set; }
        public string STAT { get; set; }
        public string APVQTY { get; set; }
        public string BACKQTY { get; set; }
        public string POSTTYPE { get; set; } // 過帳分類
    }

    public class AB0101Repository : JCLib.Mvc.BaseRepository
    {
        public AB0101Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AB0101> GetAll(string date1, string date2, string frwh, string towh, string mmcode, string posttype, bool diff) {
            var p = new DynamicParameters();

            var sql = @"select a.docno,
                               (a.flowid || ' ' || (select flowname from ME_FLOW where flowid = a.flowid)) as flowid,
                               twn_date(a.apptime) as apptime,
                               (a.frwh || ' ' || wh_name(a.frwh)) as frwh,
                               (a.towh || ' ' || wh_name(a.towh)) as towh,
                               a.mmcode, a.seq, a.mmname_e, a.mmname_c, a.base_unit, 
                               (select data_value || ' ' || data_desc from PARAM_D
                                 where grp_code = 'ME_DOCD' and data_name = 'STAT'
                                   and data_value = a.stat) as stat, 
                               a.apvqty
                                , b.backqty, a.POSTTYPE
                          from (
                                select a.docno, a.flowid, a.apptime, a.frwh, a.towh, 
                                       b.mmcode, b.seq,
                                       (select mmname_e from MI_MAST where mmcode = b.mmcode) as mmname_e,
                                       (select mmname_c from MI_MAST where mmcode = b.mmcode) as mmname_c,                
                                       (select base_unit from MI_MAST where mmcode = b.mmcode) as base_unit,      
                                       b.stat, b.apvqty,
                                       (case when a.UPDATE_USER='admin' then '系統點收' 
                                        else '手動點收(' || (select UNA from UR_ID where TUSER = a.UPDATE_USER) || ')' end) as POSTTYPE
                                  from ME_DOCM a, ME_DOCD b
                                 where a.doctype = 'RS' 
                                   and a.flowid = '1299'
                                   and a.docno = b.docno ";
            if (date1 != string.Empty) {
                sql += "           and twn_date(a.apptime) >= :date1 ";
                p.Add(":date1", date1);
            }
            if (date2 != string.Empty)
            {
                sql += "           and twn_date(a.apptime) <= :date2 ";
                p.Add(":date2", date2);
            }
            if (frwh != string.Empty)
            {
                sql += "           and a.frwh = :frwh ";
                p.Add(":frwh", frwh);
            }
            if (towh != string.Empty)
            {
                sql += "           and a.towh = :towh ";
                p.Add(":towh", towh);
            }
            if (mmcode != string.Empty)
            {
                sql += "           and b.mmcode = :mmcode ";
                p.Add(":mmcode", mmcode);
            }
            if (posttype != string.Empty)
            {
                if (posttype == "0") // 手動點收
                    sql += "           and a.UPDATE_USER <> 'admin' ";
                else if (posttype == "1") // 系統點收
                    sql += "           and a.UPDATE_USER = 'admin' ";

            }
            sql += @"          ) a
                     left join
                               (
                                 select rdocno, rseq, sum(backqty) as backqty
                                   from ME_BACK
                                  where 1=1 ";
            if (frwh != string.Empty)
            {
                sql += "           and nrcode = :frwh ";
            }
            if (towh != string.Empty)
            {
                sql += "           and returnstockcode = :towh ";
            }
            if (mmcode != string.Empty)
            {
                sql += "           and ordercode = :mmcode ";
                p.Add(":mmcode", mmcode);
            }
            sql +=@"             group by rdocno, rseq
                                ) b
                            on a.docno = b.rdocno and a.seq = b.rseq
                            where 1=1 ";

            if (diff)
                sql += " and a.apvqty <> b.backqty ";

            sql += @"    order by a.docno, a.mmcode, a.apptime";

            return DBWork.PagingQuery<AB0101>(sql, p, DBWork.Transaction);
        }

        public DataTable GetExcel(string date1, string date2, string frwh, string towh, string mmcode, string posttype, bool diff)
        {
            var p = new DynamicParameters();

            var sql = @"select a.docno as 退藥單號,
                               (a.flowid || ' ' || (select flowname from ME_FLOW where flowid = a.flowid)) as 單據狀態,
                               twn_date(a.apptime) as 退藥日期,
                               (a.frwh || ' ' || wh_name(a.frwh)) as ""退藥庫房(病房)"",
                               (a.towh || ' ' || wh_name(a.towh)) as 繳回庫房,
                               a.mmcode as 院內碼, 
                               a.mmname_e as 英文品名, 
                               a.mmname_c as 中文品名,
                               b.backqty as HIS退藥量,
                               a.base_unit as 計量單位, 
                               (select data_value || ' ' || data_desc from PARAM_D
                                 where grp_code = 'ME_DOCD' and data_name = 'STAT'
                                   and data_value = a.stat) as 狀態, 
                               a.apvqty as 實際退藥量,
                               a.POSTTYPE as 過帳分類
                          from (
                                select a.docno, a.flowid, a.apptime, a.frwh, a.towh, 
                                       b.mmcode, b.seq,
                                       (select mmname_e from MI_MAST where mmcode = b.mmcode) as mmname_e,
                                       (select mmname_c from MI_MAST where mmcode = b.mmcode) as mmname_c,                
                                       (select base_unit from MI_MAST where mmcode = b.mmcode) as base_unit,      
                                       b.stat, b.apvqty, 
                                        (case when a.UPDATE_USER='admin' then '系統點收' 
                                        else '手動點收(' || (select UNA from UR_ID where TUSER = a.UPDATE_USER) || ')' end) as POSTTYPE
                                  from ME_DOCM a, ME_DOCD b
                                 where a.doctype = 'RS' 
                                   and a.flowid = '1299'
                                   and a.docno = b.docno ";
            if (date1 != string.Empty)
            {
                sql += "           and twn_date(a.apptime) >= :date1 ";
                p.Add(":date1", date1);
            }
            if (date2 != string.Empty)
            {
                sql += "           and twn_date(a.apptime) <= :date2 ";
                p.Add(":date2", date2);
            }
            if (frwh != string.Empty)
            {
                sql += "           and a.frwh = :frwh ";
                p.Add(":frwh", frwh);
            }
            if (towh != string.Empty)
            {
                sql += "           and a.towh = :towh ";
                p.Add(":towh", towh);
            }
            if (mmcode != string.Empty)
            {
                sql += "           and b.mmcode = :mmcode ";
                p.Add(":mmcode", mmcode);
            }
            if (posttype != string.Empty)
            {
                if (posttype == "0") // 手動點收
                    sql += "           and a.UPDATE_USER <> 'admin' ";
                else if (posttype == "1") // 系統點收
                    sql += "           and a.UPDATE_USER = 'admin' ";

            }
            sql += @"          ) a
                     left join
                               (
                                 select rdocno, rseq, sum(backqty) as backqty
                                   from ME_BACK
                                  where 1=1 ";
            if (frwh != string.Empty)
            {
                sql += "           and nrcode = :frwh ";
            }
            if (towh != string.Empty)
            {
                sql += "           and returnstockcode = :towh ";
            }
            if (mmcode != string.Empty)
            {
                sql += "           and ordercode = :mmcode ";
                p.Add(":mmcode", mmcode);
            }
            sql += @"             group by rdocno, rseq
                                ) b
                            on a.docno = b.rdocno and a.seq = b.rseq
                            where 1=1 ";

            if (diff)
                sql += " and a.apvqty <> b.backqty ";

            sql += @"    order by a.docno, a.mmcode, a.apptime";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        #region combo
        public IEnumerable<MI_WHMAST> GetTowhNos(string queryString, string userId)
        {
            var p = new DynamicParameters();

            var sql = @"select {0} a.wh_no, 
                                  wh_name(wh_no) as wh_name
                          from MI_WHID a 
                         where (select 1 from MI_WHMAST
                                 where wh_kind = '0' and wh_no = a.wh_no) = 1
                           and wh_userid = :userid";
            p.Add(":userId", userId);

            if (queryString != "")
            {
                sql = string.Format(sql, "(nvl(instr(a.wh_no, :wh_no_i), 1000) )IDX,"); // 設定權重, 值越小權重最大
                p.Add(":wh_no_i", queryString);

                sql += " and (a.wh_no like :wh_no) ";
                p.Add(":wh_no", string.Format("%{0}%", queryString));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY A.wh_no ";
            }

            return DBWork.PagingQuery<MI_WHMAST>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<MI_WHMAST> GetFrwhNos(string queryString)
        {
            var p = new DynamicParameters();

            var sql = @"select {0} a.wh_no, 
                                  a.wh_name
                          from MI_WHMAST a 
                         where 1=1 
                           and a.wh_kind = '0'
                           and a.wh_grade in ('3', '4')";
                            
            if (queryString != "")
            {
                sql = string.Format(sql, "(nvl(instr(a.wh_no, :wh_no_i), 1000) + nvl(instr(a.wh_name, :wh_name_i), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
                p.Add(":wh_no_i", queryString);
                p.Add(":wh_name_i", queryString);

                sql += " and (a.wh_no like :wh_no ";
                p.Add(":wh_no", string.Format("%{0}%", queryString));

                sql += " or a.wh_name like :wh_name )";
                p.Add(":wh_name", string.Format("%{0}%", queryString));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY a.wh_grade, a.wh_no ";
            }

            return DBWork.PagingQuery<MI_WHMAST>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E
                        FROM MI_MAST A 
                        WHERE 1=1 
                          and a.mat_class = '01'";

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

        public IEnumerable<COMBO_MODEL> GetOrdersortCombo()
        {
            var p = new DynamicParameters();

            var sql = @" select DATA_VALUE as VALUE, DATA_VALUE||' '||DATA_DESC as COMBITEM
                        from PARAM_D 
                        where GRP_CODE='ME_BACK' and DATA_NAME='ORDERSORT'
                        order by DATA_SEQ ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p, DBWork.Transaction);
        }
        #endregion
    }
}