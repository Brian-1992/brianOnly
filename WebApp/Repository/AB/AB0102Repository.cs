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
    public class AB0102LatestData {
        public string 門急住診別;
        public string 最新資料日期;
        public string 最新資料結束時間;
        public string 扣庫成功筆數;
        public string 扣庫失敗筆數;
        public string 系統時間;
    }

    public class AB0102Repository : JCLib.Mvc.BaseRepository
    {
        public AB0102Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<MI_CONSUME_DATE> GetAll(string date1, string date2, string wh_no, string mmcode, bool failOnly, string proc_msg) {
            var p = new DynamicParameters();
            string sql = @"select a.data_date, a.data_btime, a.data_etime, a.wh_no, 
                                  wh_name(a.wh_no) as wh_name,
                                  a.mmcode, 
                                  mmcode_name(a.mmcode) as mmname_e, 
                                  a.consume_qty, a.stock_unit, a.insu_qty, a.hosp_qty,
                                  a.parent_ordercode, a.parent_consume_qty, a.createdatetime, a.proc_msg, 
                                  (select data_value || ' ' || data_desc
                                     from PARAM_D
                                    where grp_code = 'MI_CONSUME_DATE'
                                      and data_name = 'PROC_ID'
                                      and data_value = a.proc_id) as proc_id,
                                  (select data_value || ' ' || data_desc
                                     from PARAM_D
                                    where grp_code = 'MI_CONSUME_DATE'
                                      and data_name = 'PROC_TYPE'
                                      and data_value = a.proc_type) as proc_type,
                                  (select data_value || ' ' || data_desc
                                     from PARAM_D
                                    where grp_code = 'MI_CONSUME_DATE'
                                      and data_name = 'VISIT_KIND'
                                      and data_value = a.visit_kind) as visit_kind
                             from MI_CONSUME_DATE a
                            where 1=1 ";
            if (date1 != string.Empty) {
                sql += "      and a.data_date >= :date1";
                p.Add(":date1", date1);
            }
            if (date2 != string.Empty)
            {
                sql += "      and a.data_date <= :date2";
                p.Add(":date2", date2);
            }
            if (wh_no != string.Empty)
            {
                sql += "      and a.wh_no = :wh_no";
                p.Add(":wh_no", wh_no);
            }
            if (mmcode != string.Empty)
            {
                sql += "      and a.mmcode = :mmcode";
                p.Add(":mmcode", mmcode);
            }
            if (failOnly) {
                sql += "      and a.proc_id = 'N'";
            }
            if (proc_msg != string.Empty) {
                if (proc_msg == "其他")
                {
                    sql += "  and a.proc_msg not in ('庫房代碼不存在', 院內碼不存在)";
                }
                else {
                    sql += "  and a.proc_msg = :proc_msg";
                }
                p.Add(":proc_msg", proc_msg);
            }
            sql += "        order by a.data_date, a.data_btime, a.data_etime, a.wh_no, a.mmcode, a.visit_kind";

            return DBWork.PagingQuery<MI_CONSUME_DATE>(sql, p, DBWork.Transaction);
        }

        public DataTable GetExcel(string date1, string date2, string wh_no, string mmcode, bool failOnly, string proc_msg)
        {
            var p = new DynamicParameters();
            string sql = @"select a.data_date as 資料日期, 
                                  a.data_btime as 資料開始時間, 
                                  a.data_etime as 資料結束時間, 
                                  a.wh_no as 庫房代碼, 
                                  a.mmcode as 院內碼, 
                                  (select data_value || ' ' || data_desc
                                     from PARAM_D
                                    where grp_code = 'MI_CONSUME_DATE'
                                      and data_name = 'VISIT_KIND'
                                      and data_value = a.visit_kind) as 門急住診別, 
                                  a.consume_qty as 消耗量, 
                                  a.stock_unit as 扣庫單位, 
                                  a.insu_qty as 健保耗量, 
                                  a.hosp_qty as 自費耗量,
                                  a.parent_ordercode as 母藥醫令代碼, 
                                  a.parent_consume_qty as 母藥消耗總量, 
                                  a.createdatetime as 資料寫入時間, 
                                  (select data_value || ' ' || data_desc
                                     from PARAM_D
                                    where grp_code = 'MI_CONSUME_DATE'
                                      and data_name = 'PROC_ID'
                                      and data_value = a.proc_id) as 扣庫處理結果,
                                  a.proc_msg as 處理訊息, 
                                  (select data_value || ' ' || data_desc
                                     from PARAM_D
                                    where grp_code = 'MI_CONSUME_DATE'
                                      and data_name = 'PROC_TYPE'
                                      and data_value = a.proc_type) as 處理類別
                             from MI_CONSUME_DATE a
                            where 1=1 ";
            if (date1 != string.Empty)
            {
                sql += "      and a.data_date >= :date1";
                p.Add(":date1", date1);
            }
            if (date2 != string.Empty)
            {
                sql += "      and a.data_date <= :date2";
                p.Add(":date2", date2);
            }
            if (wh_no != string.Empty)
            {
                sql += "      and a.wh_no = :wh_no";
                p.Add(":wh_no", wh_no);
            }
            if (mmcode != string.Empty)
            {
                sql += "      and a.mmcode = :mmcode";
                p.Add(":mmcode", mmcode);
            }
            if (failOnly)
            {
                sql += "      and a.proc_id = 'N'";
            }
            if (proc_msg != string.Empty)
            {
                if (proc_msg == "其他")
                {
                    sql += "  and a.proc_msg not in ('庫房代碼不存在', 院內碼不存在)";
                }
                else
                {
                    sql += "  and a.proc_msg = :proc_msg";
                }
                p.Add(":proc_msg", proc_msg);
            }
            sql += "        order by a.data_date, a.data_btime, a.data_etime, a.wh_no, a.mmcode, a.visit_kind";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<AB0102LatestData> GetLatestData() {
            string sql = @"
                select a.DATA_VALUE||' '||
                         (select DATA_DESC
                            from PARAM_D
                           where GRP_CODE='MI_CONSUME_DATE'
                             and DATA_NAME='VISIT_KIND'
                             and DATA_VALUE=a.DATA_VALUE) as 門急住診別,
                         substr(b.DATA_ETIME, 1, 7) as 最新資料日期,
                         substr(b.DATA_ETIME, 8, 6) as 最新資料結束時間,
                         c.PROC_ID_Y as 扣庫成功筆數,c.PROC_ID_N as 扣庫失敗筆數,
                         sysdate as 系統時間
                  from
                    (select DATA_VALUE
                       from PARAM_D
                       where GRP_CODE = 'MI_CONSUME_DATE'
                         and DATA_NAME = 'VISIT_KIND'
                    ) a
                  left join
                    (select VISIT_KIND, max(DATA_DATE || DATA_ETIME) as data_etime
                       from MI_CONSUME_DATE
                      group by VISIT_KIND) b
                  on a.DATA_VALUE = b.VISIT_KIND
                  left join
                    (select VISIT_KIND, (DATA_DATE || DATA_ETIME) as data_etime,
                            sum(case when PROC_ID = 'Y' then 1 
                                 else 0
                            end) as PROC_ID_Y,
                            sum(case when PROC_ID = 'N' then 1
                                 else 0
                            end) as PROC_ID_N
                       from MI_CONSUME_DATE
                      group by VISIT_KIND,(DATA_DATE || DATA_ETIME)
                    ) c
                  on b.VISIT_KIND = c.VISIT_KIND and b.data_etime = c.data_etime
                  order by a.DATA_VALUE
            ";

            return DBWork.Connection.Query<AB0102LatestData>(sql, DBWork.Transaction);
        }

        #region combo
        public IEnumerable<MI_WHMAST> GetWhnos(string queryString)
        {
            var p = new DynamicParameters();

            var sql = @"select {0} a.wh_no, 
                                   wh_name(a.wh_no) as wh_name,
                                   a.wh_grade
                          from MI_WHMAST a 
                         where ((wh_kind = '0' and wh_grade in ('2', '3', '4'))
                                or (wh_kind = '1' and wh_grade = '2'))";

            if (queryString != "")
            {
                sql = string.Format(sql, "(nvl(instr(a.wh_no, :wh_no_i), 1000) + nvl(instr(a.wh_name, :wh_name_i), 100) * 10  )IDX,"); // 設定權重, 值越小權重最大
                p.Add(":wh_no_i", queryString);
                p.Add(":wh_name_i", queryString);

                sql += " and (a.wh_no like :wh_no ";
                p.Add(":wh_no", string.Format("%{0}%", queryString));

                sql += "   or a.wh_name like :wh_name ) ";
                p.Add(":wh_name", string.Format("%{0}%", queryString));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY a.wh_kind, a.wh_grade, a.wh_no";
            }

            return DBWork.PagingQuery<MI_WHMAST>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E
                        FROM MI_MAST A 
                        WHERE 1=1 
                          and a.mat_class in ('01','02')";

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

        public IEnumerable<COMBO_MODEL> GetProcMsg() {
            string sql = @"
                select 1 as value,'庫房代碼不存在' as text from dual
                union
                select 2 as value,'院內碼不存在' as text from dual
                union
                select 3 as value,'其它' as text from dual
                order by value
            ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, DBWork.Transaction);
        }
        #endregion

        #region
        private string DetailSQL(string date1, string date2, string wh_no, string mmcode, bool failOnly, string proc_msg) {
            string sql = @"select DATA_DATE ,DATA_BTIME,
                                  DATA_ETIME ,
                                  STOCKCODE ,
                                  wh_name(stockcode) as WH_NAME,
                                  ORDERCODE ,
                                  mmcode_name(ordercode) as mmname_e,
                                  VISIT_KIND || ' ' ||
                                    (select DATA_DESC from PARAM_D
                                      where GRP_CODE = 'MI_CONSUME_DATE'
                                        and DATA_NAME = 'VISIT_KIND'
                                        and DATA_VALUE = a.VISIT_KIND) as VISIT_KIND,       
                                  (case when STOCKFLAG = 'Y' then 'Y 是'
                                        when STOCKFLAG = 'N' then 'N 否'
                                        else STOCKFLAG
                                   end) as STOCKFLAG,     
                                  ORDERDR ,MEDNO ,
                                  VISITDATE ,RID ,
                                  WORKDATE ,WORKTIME,
                                  MODIFYDATE ,MODIFYTIME ,
                                  CANCELDATETIME ,SECTIONNO ,
                                  VSDR ,INSULOOKSEQ,
                                  DOSE ,FREQNO ,
                                  PATHNO ,DAYS,
                                  SUMQTY ,EMGFLAG ,
                                  REGION ,PAYFLAG,
                                  COMPUTECODE ,ADDRATIO1 ,
                                  ADDRATIO2,
                                  ADDRATIO3,
                                  INSUCHARGEID ,
                                  HOSPCHARGEID ,
                                  INSUAMOUNT ,PAYAMOUNT ,
                                  CANCELFLAG ,CANCELOPID ,
                                  PROCOPID ,PROCDATETIME,
                                  CREATEOPID ,CREATEDATETIME ,
                                  (case when nvl(a.fromsys, 'HIS1') = 'HIS2'
                                        then to_char(a.drugno_his2)
                                        else to_char(drugno)
                                    end) as DRUGNO ,
                                  SLOWCARD ,
                                  BAGSEQNO ,SENDDATE,
                                  TOTALQTY ,ORDERUNIT,
                                  ATTACHUNIT ,
                                  ID ,CHARNO ,
                                  BATCHNUM ,EXPIREDDATE ,
                                  PARENT_ORDERCODE ,
                                  PARENT_CONSUME_QTY 
                             from HIS_CONSUME_D a
                            where exists(
                                   select 1 from MI_CONSUME_DATE
                                    where 1 = 1
                                      and DATA_DATE = a.DATA_DATE
                                      and DATA_BTIME = a.DATA_BTIME
                                      and DATA_ETIME = a.DATA_ETIME
                                      and WH_NO = a.STOCKCODE
                                      and MMCODE = a.ORDERCODE
                                      and VISIT_KIND = a.VISIT_KIND
                        ";
            if (date1 != string.Empty) {
                sql += "              and data_date >= :date1";
            }
            if (date2 != string.Empty)
            {
                sql += "              and data_date <= :date2";
            }
            if (wh_no != string.Empty)
            {
                sql += "              and wh_no = :wh_no";
            }
            if (mmcode != string.Empty)
            {
                sql += "              and mmcode = :mmcode";
            }
            if (failOnly)
            {
                sql += "              and proc_id = 'N'";
            }
            if (proc_msg != string.Empty)
            {
                if (proc_msg == "其他")
                {
                    sql += "          and proc_msg is not null and proc_msg not in ('庫房代碼不存在', 院內碼不存在)";
                }
                else
                {
                    sql += "          and proc_msg = :proc_msg";
                }
            }

            sql += " ) ";

            return sql;
        }

        public IEnumerable<HIS_CONSUME_D> GetDetails(string date1, string date2, string wh_no, string mmcode, bool failOnly, string proc_msg) {
            string sql = DetailSQL(date1, date2, wh_no, mmcode, failOnly, proc_msg);
            var p = new DynamicParameters();

            p.Add("date1", date1);
            p.Add("date2", date2);
            p.Add("wh_no", wh_no);
            p.Add("mmcode", mmcode);
            p.Add("failOnly", failOnly);
            p.Add("proc_msg", proc_msg);

            return DBWork.PagingQuery<HIS_CONSUME_D>(sql, p, DBWork.Transaction);
        }

        public DataTable GetDetailExcel(string date1, string date2, string wh_no, string mmcode, bool failOnly, string proc_msg) {
            var p = new DynamicParameters();
            string sql = DetailSQL(date1, date2, wh_no, mmcode, failOnly, proc_msg);
            sql = string.Format(@"
                select ID as ID, DATA_DATE as 年月日, DATA_BTIME as ""統計時間(起)"",
                       DATA_ETIME as ""統計時間(迄)"",
                       STOCKCODE as 扣庫地點,
                       WH_NAME as 庫房名稱,
                       ORDERCODE as 院內代碼,
                       MMNAME_E as 英文品名,
                       SUMQTY as ""總量(開藥為正，退藥為負)"",
                       STOCKFLAG as 是否需扣庫,  
                       CHARNO as 門急住病歷號,
                       RID as 掛號流水號,
                       WORKDATE as 開立日期,WORKTIME as 開立時間,
                       MODIFYDATE as 修改日期,MODIFYTIME as 修改時間,
                       CANCELDATETIME as 刪除日期時間,SECTIONNO as 科別代碼,
                       VSDR as 掛號醫師,
                       CANCELFLAG as 是否刪除,CANCELOPID as 刪除人員,
                       PROCOPID as 開立人員,PROCDATETIME as 開立日期時間,
                       CREATEOPID as 建立人員,CREATEDATETIME as 建立日期時間,
                       DRUGNO as 領藥號,
                       MEDNO as 病人電腦編號,
                       BATCHNUM as 批號,EXPIREDDATE as 效期,
                       PARENT_ORDERCODE as 母藥醫令代碼,
                       PARENT_CONSUME_QTY as 母藥消耗總量,
                       VISIT_KIND  as 門急住診別,       
                       ORDERDR as 開立醫師,
                       VISITDATE as 就診日,
                       INSULOOKSEQ as IC卡卡號,
                       DOSE as 開立劑量,FREQNO as 頻率,
                       PATHNO as 途徑,DAYS as 天數,
                       EMGFLAG as 是否急作,
                       REGION as 部位,PAYFLAG as 是否自費,
                       COMPUTECODE as 是否計價,ADDRATIO1 as ""預設為零(ADDRATIO1)"",
                       ADDRATIO2 as ""兒童加成(ADDRATIO2)"",
                       ADDRATIO3 as ""健保急作加成(ADDRATIO3)"",
                       INSUCHARGEID as ""健保費用類別(健保歸屬)"",
                       HOSPCHARGEID as ""院內費用類別(院內歸屬)"",
                       INSUAMOUNT as 健保價,PAYAMOUNT as 自費價,
                       SLOWCARD as 慢箋流水號,
                       BAGSEQNO as 序號,SENDDATE as 領藥日期,
                       TOTALQTY as 開立總量,ORDERUNIT as ""單位(ORDERUNIT)"",
                       ATTACHUNIT as ""單位(ATTACHUNIT)""
                  from (
                    {0}
                  )
                 order by data_date, data_btime, data_etime, stockcode, ordercode, visit_kind
                  ", sql);

            p.Add("date1", date1);
            p.Add("date2", date2);
            p.Add("wh_no", wh_no);
            p.Add("mmcode", mmcode);
            p.Add("failOnly", failOnly);
            p.Add("proc_msg", proc_msg);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
        #endregion
    }
}