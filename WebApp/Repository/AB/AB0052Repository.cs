using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JCLib.DB;
using WebApp.Models;
using Dapper;
using System.Data;

namespace WebApp.Repository.AB
{
    public class AB0052Repository : JCLib.Mvc.BaseRepository
    {
        public AB0052Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ME_EXPD> GetAll(string wh_no, string exp_date, string status)
        {

            var p = new DynamicParameters();



            string sql = @"SELECT a.MMCODE as MMCODE,
                                  (select mmname_c from MI_MAST where mmcode = a.mmcode) as MMNAME_C,
                                  (select mmname_e from MI_MAST where mmcode = a.mmcode) as MMNAME_E,
                                  a.mmcode as  mmcode_display,
                                  TWN_DATE(a.EXP_DATE) as EXP_DATE,
                                  a.EXP_QTY as EXP_QTY,
                                  a.LOT_NO as LOT_NO,
                                  a.LOT_NO as LOT_NO_DISPLAY,
                                  TWN_DATE(a.EXP_DATE1) as EXP_DATE1,
                                  a.LOT_NO1 as LOT_NO1,
                                  TWN_DATE(a.EXP_DATE2) as EXP_DATE2,
                                  a.LOT_NO2 as LOT_NO2,
                                  TWN_DATE(a.EXP_DATE3) as EXP_DATE3,
                                  a.LOT_NO3 as LOT_NO3,
                                  a.MEMO as MEMO,
                                  TWN_DATE(a.REPLY_DATE) as REPLY_DATE,
                                  TWN_DATE(a.REPLY_DATE) as REPLY_DATE_DISPLAY,
                                  TWN_DATE(a.REPLY_TIME) as REPLY_TIME,
                                  (select una from UR_ID where tuser = a.reply_id) as REPLY_ID,
                                  TWN_DATE(a.CLOSE_TIME) as CLOSE_TIME,
                                  (select una from UR_ID where tuser = a.close_id) as CLOSE_ID,
                                  a.WH_NO as WH_NO,
                                  a.EXP_SEQ as EXP_SEQ,
                                  a.EXP_STAT as EXP_STAT,
                                  (select wh_no || ' ' || wh_name from MI_WHMAST where wh_no = a.wh_no) as WH_NAME,
                                  (case when a.exp_stat = '1'
                                            then '未回報'
                                        else '已回報' end) as exp_stat_name,
                                  (select listagg(store_loc, ',')
                                   within group (order by store_loc)
                                     from MI_WLOCINV
                                    where wh_no = 'PH1S'
                                      and mmcode = a.mmcode) as store_loc,
                                  (select inv_qty(:p0, a.mmcode) from dual) as inv_qty
                             FROM ME_EXPD a
                            WHERE 1=1";
            //a.EXP_STAT = '1'";

            if (wh_no != "")
            {
                sql += " AND a.WH_NO = :p0";
                p.Add(":p0", string.Format("{0}", wh_no));
            }

            if (exp_date != "")
            {
                DateTime temp = DateTime.Parse(exp_date);

                int year = DateTime.Parse(exp_date).Year;
                int month = DateTime.Parse(exp_date).Month;
                string start = temp.ToString("yyyy-MM-dd");
                string end = temp.AddDays(DateTime.DaysInMonth(year, month) - 1).ToString("yyyy-MM-dd");

                sql += @"    AND TRUNC(a.EXP_DATE, 'DD')
                         BETWEEN TO_DATE(:p1,'YYYY-MM-DD') AND TO_DATE(:p2,'YYYY-MM-DD') ";

                p.Add(":p1", string.Format("{0}", start));
                p.Add(":p2", string.Format("{0}", end));
            }

            if (status.Trim() != string.Empty)
            {
                sql += string.Format("   and a.exp_stat = '{0}'", status);
            }

            return DBWork.PagingQuery<ME_EXPD>(sql, p, DBWork.Transaction);

        }
        //private string DateTranfer(string yearmonth, string date)
        //{
        //    int year = (int.Parse(yearmonth.Substring(0, 3)) + 1911);
        //    string month = yearmonth.Substring(3, 2);

        //    return string.Format("{0}-{1}-{2}", year, month, date);
        //}

        public IEnumerable<MI_WEXPINV> GetMiWexpinvs(string wh_no, DateTime maxDate)
        {
            string sql = @"SELECT a.WH_NO as WH_NO,
                                  b.WH_NAME as WH_NAME,
                                  a.MMCODE as MMCODE,
                                  c.MMNAME_C as MMNAME_C,
                                  c.MMNAME_E as MMNAME_E,
                                  a.EXP_DATE as EXP_DATE,
                                  a.LOT_NO as LOT_NO,
                                  a.INV_QTY as INV_QTY
                             FROM MI_WEXPINV a, MI_WHMAST b, MI_MAST c
                            WHERE a.WH_NO = :WH_NO
                              AND a.EXP_DATE > SYSDATE
                              AND TRUNC(a.EXP_DATE, 'DD') <= TO_DATE(:MAXDATE,'YYYY-MM-DD')
                              AND b.WH_NO = a.WH_NO
                              AND c.MMCODE = a.MMCODE
                            ORDER BY a.MMCODE, a.EXP_DATE";

            return DBWork.Connection.Query<MI_WEXPINV>(sql, new { WH_NO = wh_no, MAXDATE = maxDate.ToString("yyyy-MM-dd") }, DBWork.Transaction);
        }

        public int Create(ME_EXPD expd)
        {
            string sql = @"INSERT INTO ME_EXPD (WH_NO, MMCODE,EXP_DATE, EXP_SEQ, EXP_DATE1, LOT_NO1, EXP_DATE2, LOT_NO2, EXP_DATE3, LOT_NO3, EXP_STAT, CREATE_TIME, CREATE_ID, UPDATE_TIME, UPDATE_ID, IP)
                           VALUES (:WH_NO, :MMCODE, TO_DATE(:EXP_DATE,'YYYY-MM-DD'), :EXP_SEQ, TO_DATE(:EXP_DATE1,'YYYY-MM-DD'), :LOT_NO1, TO_DATE(:EXP_DATE2,'YYYY-MM-DD'), :LOT_NO2, TO_DATE(:EXP_DATE3,'YYYY-MM-DD'), :LOT_NO3, :EXP_STAT, SYSDATE, :CREATE_ID, SYSDATE, :UPDATE_ID, :IP)";
            // , SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP

            return DBWork.Connection.Execute(sql, expd, DBWork.Transaction);
        }

        public int CreateManual(ME_EXPD expd)
        {
            string sql = @"INSERT INTO ME_EXPD (WH_NO, MMCODE, EXP_DATE, EXP_SEQ,  LOT_NO, REPLY_DATE, EXP_QTY,EXP_STAT,MEMO,REPLY_ID, REPLY_TIME, CREATE_TIME, CREATE_ID, UPDATE_TIME, UPDATE_ID, IP)
                           VALUES (:WH_NO, :MMCODE, TO_DATE(:EXP_DATE,'YYYY-MM-DD'), :EXP_SEQ, :LOT_NO, TWN_TODATE(:REPLY_DATE), :EXP_QTY ,'1', :MEMO, :REPLY_ID,SYSDATE ,SYSDATE, :CREATE_ID, SYSDATE, :UPDATE_ID, :IP)";
            // , SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP

            return DBWork.Connection.Execute(sql, expd, DBWork.Transaction);
        }

        public int Update(ME_EXPD expd)
        {
            string sql = @"UPDATE ME_EXPD 
                              SET EXP_QTY = :EXP_QTY, REPLY_TIME = SYSDATE, REPLY_ID = :REPLY_ID,MEMO = :MEMO, UPDATE_TIME = SYSDATE, UPDATE_ID = :UPDATE_ID, IP = :IP
                            WHERE WH_NO = :WH_NO
                              AND MMCODE = :MMCODE
                              AND EXP_SEQ = :EXP_SEQ
                              AND TWN_DATE(EXP_DATE)= :EXP_DATE
                              and lot_no = :lot_no
                              and TWN_DATE(reply_date) = :reply_date";
            // , SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP

            return DBWork.Connection.Execute(sql, expd, DBWork.Transaction);
        }

        public int Delete(ME_EXPD expd)
        {
            string sql = @"DELETE FROM ME_EXPD 
                            WHERE WH_NO = :WH_NO
                              AND MMCODE = :MMCODE
                              AND EXP_SEQ = :EXP_SEQ
                              AND TWN_DATE(EXP_DATE)= :EXP_DATE";
            // , SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP

            return DBWork.Connection.Execute(sql, expd, DBWork.Transaction);
        }

        public int DeleteExpd0(string wh_no, string exp_date)
        {
            string sql = @"DELETE FROM ME_EXPD 
                            WHERE WH_NO = :WH_NO
                              AND TWN_DATE(EXP_DATE)= :EXP_DATE
                              AND EXP_STAT = '1'
                              AND EXP_QTY = 0 ";
            
            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, EXP_DATE = exp_date }, DBWork.Transaction);
        }

        public int Transfer(ME_EXPD expd)
        {
            //expd.EXP_DATE = DateTime.Parse(expd.EXP_DATE).ToString("yyyy-MM-dd"); 

            string sql = @"UPDATE ME_EXPD 
                              SET EXP_STAT = 2, UPDATE_TIME = SYSDATE, UPDATE_ID = :UPDATE_ID, IP = :IP
                            WHERE WH_NO = :WH_NO
                              AND TWN_DATE(EXP_DATE)= :EXP_DATE
                              and exp_stat = '1'";
            // , SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP

            return DBWork.Connection.Execute(sql, expd, DBWork.Transaction);
        }

        public IEnumerable<ME_EXPD> GetAllExpds(string wh_no, string exp_date)
        {
            string sql = @"select * from ME_EXPD where wh_no = :wh_no and TWN_DATE(exp_date) = :exp_date and exp_stat = '1'";

            return DBWork.Connection.Query<ME_EXPD>(sql, new { exp_date = exp_date, wh_no = wh_no }, DBWork.Transaction);
        }
        public int InsertExpm(string wh_no, string exp_date, string userId, string ip)
        {
            string sql = @"insert into ME_EXPM a (mmcode, exp_date,warnym, lot_no, exp_qty,
                                                  create_user, create_time, update_user, update_time, update_ip,
                                                  agen_no, agen_namec, is_agenno, warnym_key)
                           select mmcode, exp_date,TWN_YYYMM(REPLY_DATE) as warnym, 
                                  lot_no, sum(exp_qty), :userid, sysdate, :userid, sysdate, :ip,
                                  (select m_agenno from MI_MAST where mmcode = b.mmcode),
                                  (select agen_namec from PH_VENDER
                                    where agen_no = (select m_agenno from MI_MAST where mmcode = b.mmcode)),
                                  'N',
                                  TWN_YYYMM(REPLY_DATE) as warnym_key
                             from ME_EXPD b
                            where exp_stat = '1'
                              and TWN_DATE(b.exp_date) = :exp_date
                              and b.wh_no = :wh_no
                              and not exists(
                                        select 1 from ME_EXPM c
                                         where b.mmcode = c.mmcode
                                           and b.exp_date = c.exp_date
                                           and b.lot_no = c.lot_no
                                )
                              and reply_date < sysdate + 180
                         group by b.wh_no, b.mmcode, b.exp_date, b.lot_no, TWN_YYYMM(b.reply_date)";
            //                and b.exp_qty > 0
            //           group by b.wh_no, b.mmcode, b.exp_date, b.lot_no,b.reply_date";
            return DBWork.Connection.Execute(sql, new { wh_no = wh_no, exp_date = exp_date, userid = userId, ip = ip }, DBWork.Transaction);
        }

        public int UpdateExpm(string wh_no, string exp_date, string userId, string ip)
        {
            string sql = @"update ME_EXPM a 
                              set exp_qty = exp_qty + NVL(
                                    (select nvl(sum(exp_qty), 0) exp_qty
                                       from ME_EXPD b
                                      where exp_stat = '1'
                                        and TWN_DATE(b.exp_date) = :exp_date
                                        and b.wh_no = :wh_no
                                        and a.mmcode = b.mmcode
                                        and a.exp_date = b.exp_date
                                        and a.lot_no = b.lot_no
                                      group by b.mmcode, b.exp_date, b.lot_no
                                  ), 0),
                                    warnym =
                                     (case when
                                             (select TWN_YYYMM(min(reply_date))
                                                from ME_EXPD b
                                               where exp_stat = '1'
                                                 and TWN_DATE(b.exp_date) = :exp_date
                                                 and b.wh_no = :wh_no
                                                 and a.mmcode = b.mmcode
                                                 and a.exp_date = b.exp_date
                                                 and a.lot_no = b.lot_no
                                             ) < a.warnym
                                           then  
                                             (select TWN_YYYMM(min(reply_date))
                                                from ME_EXPD b
                                               where exp_stat = '1'
                                                 and TWN_DATE(b.exp_date) = :exp_date
                                                 and b.wh_no = :wh_no
                                                 and a.mmcode = b.mmcode
                                                 and a.exp_date = b.exp_date
                                                 and a.lot_no = b.lot_no
                                             )
                                           else a.warnym
                                      end),
                                  update_user = :userid,
                                  update_time = sysdate,
                                  update_ip = :ip
                            where exists (
                                select 1 from ME_EXPD
                                   where exp_stat = '1'
                                     and TWN_DATE(exp_date) = :exp_date
                                     and wh_no = :wh_no
                                     and a.mmcode = mmcode
                                     and a.exp_date = exp_date
                                     and a.lot_no = lot_no
                            )";

            return DBWork.Connection.Execute(sql, new { wh_no = wh_no, exp_date = exp_date, userid = userId, ip = ip }, DBWork.Transaction);
        }
        public bool CheckExistsAdd(string wh_no, string mmcode, string lot_no, string exp_date)
        {
            string sql = @"select 1 from ME_EXPD
                            where wh_no = :wh_no
                              and mmcode = :mmcode
                              and lot_no = :lot_no
                              and TRUNC(EXP_DATE,'DD') = TO_DATE(:EXP_DATE,'YYYY-MM-DD')";

            return (DBWork.Connection.ExecuteScalar(sql,
                                                     new
                                                     {
                                                         wh_no = wh_no,
                                                         mmcode = mmcode,
                                                         lot_no = lot_no,
                                                         exp_date = exp_date
                                                     },
                                                     DBWork.Transaction) == null);
        }

        public bool CheckExistsUpdate(string wh_no, string mmcode, string lot_no, string exp_date, string reply_date)
        {
            string sql = @"select 1 from ME_EXPD
                            where wh_no = :wh_no
                              and mmcode = :mmcode
                              and lot_no = :lot_no
                              and TWN_DATE(exp_date) = :exp_date
                              and TWN_DATE(reply_date) = :reply_date";

            return !(DBWork.Connection.ExecuteScalar(sql,
                                                     new
                                                     {
                                                         wh_no = wh_no,
                                                         mmcode = mmcode,
                                                         lot_no = lot_no,
                                                         exp_date = exp_date,
                                                         reply_date = reply_date
                                                     },
                                                     DBWork.Transaction) == null);
        }
        public ME_EXPD GetExistingExpd(string wh_no, string mmcode, string lot_no, string exp_date, string reply_date)
        {
            string sql = @"select * from ME_EXPD
                            where wh_no = :wh_no
                              and mmcode = :mmcode
                              and lot_no = :lot_no
                              and TWN_DATE(exp_date) = :exp_date
                              and TWN_DATE(reply_date) = :reply_date";

            return DBWork.Connection.QueryFirstOrDefault<ME_EXPD>(sql, new {
                                                                    wh_no = wh_no,
                                                                    mmcode = mmcode,
                                                                    lot_no = lot_no,
                                                                    exp_date = exp_date,
                                                                    reply_date = reply_date
                                                                },
                                                     DBWork.Transaction);
        }

        public int GetMaxExpSeq(string wh_no, string mmcode, DateTime exp_date)
        {
            exp_date = exp_date.AddDays((-exp_date.Day + 1));
            string sql = @"SELECT (CASE WHEN MAX(EXP_SEQ) > 0 THEN (MAX(EXP_SEQ)+1) ELSE 1 END) 
                             FROM ME_EXPD WHERE WH_NO = :WH_NO AND MMCODE = :MMCODE AND TRUNC(EXP_DATE,'DD') = TO_DATE(:EXP_DATE,'YYYY-MM-DD')";
            return DBWork.Connection.QueryFirst<int>(sql, new
            {
                WH_NO = wh_no,
                MMCODE = mmcode,
                EXP_DATE = exp_date.ToString("yyyy-MM-dd")
            }, DBWork.Transaction);
        }



        public IEnumerable<ComboItemModel> GetWhnoCombo(string wh_userId)
        {
            string sql = @"SELECT WH_NO AS VALUE, WH_NO || ' ' || TRIM(WH_NAME) AS TEXT
                             FROM MI_WHMAST a 
                            WHERE EXISTS (
                                     SELECT *
                                       FROM MI_WHID b
                                      WHERE a.WH_NO = b.WH_NO
                                        AND WH_USERID = :WH_USERID
                            )
                            ORDER BY VALUE";
            return DBWork.Connection.Query<ComboItemModel>(sql, new { WH_USERID = wh_userId }, DBWork.Transaction);
        }

        public IEnumerable<ComboItemModel> GetLotNos(string wh_no, string mmcode, string exp_date)
        {
            string sql = @"SELECT distinct lot_no as VALUE, lot_no as TEXT
                             FROM MI_WEXPINV a 
                            where a.mmcode = :mmcode
                              and TWN_DATE(a.EXP_DATE) = :exp_date
                            ORDER BY lot_no";
            return DBWork.Connection.Query<ComboItemModel>(sql, new { mmcode = mmcode, exp_date = exp_date }, DBWork.Transaction);
        }

        public IEnumerable<ComboItemModel> GetExpDates(string wh_no, string mmcode)
        {
            string sql = @"SELECT distinct TWN_DATE(EXP_DATE) as VALUE, TWN_DATE(EXP_DATE) as TEXT
                             FROM MI_WEXPINV a 
                            where a.mmcode = :mmcode
                            ORDER BY VALUE";
            return DBWork.Connection.Query<ComboItemModel>(sql, new { mmcode = mmcode }, DBWork.Transaction);

        }

        public IEnumerable<string> GetExpQty(string wh_no, string mmcode, string lot_no, string exp_date)
        {
            string sql = @"select NVL(inv_qty, '0') from MI_WEXPINV
                            where wh_no = :wh_no
                              and mmcode = :mmcode 
                              and lot_no = :lot_no 
                              and TWN_DATE(exp_date) = :exp_date";
            return DBWork.Connection.Query<string>(sql, new { mmcode = mmcode, wh_no = wh_no, lot_no = lot_no, exp_date = exp_date }, DBWork.Transaction);
        }

        public class ExpdItem
        {
            public string LOT_NO { get; set; }
            public IEnumerable<ExpDateItem> ExpDateItems { get; set; }
        }
        public class ExpDateItem
        {
            public string EXP_DATE { get; set; }
            public string EXP_QTY { get; set; }
        }
        public IEnumerable<ExpdItem> GetExpdCombos(string wh_no, string mmcode)
        {
            string sql = @"SELECT distinct lot_no
                             FROM MI_WEXPINV a 
                            where a.wh_no = :wh_no
                              and a.mmcode = :mmcode
                            ORDER BY lot_no";
            return DBWork.Connection.Query<ExpdItem>(sql, new { mmcode = mmcode, wh_no = wh_no }, DBWork.Transaction);

        }

        public IEnumerable<ExpDateItem> GetExpDateItems(string wh_no, string mmcode, string lot_no)
        {
            string sql = @"SELECT TWN_DATE(a.EXP_DATE) as EXP_DATE, a.inv_qty as EXP_QTY
                             FROM MI_WEXPINV a 
                            where a.wh_no = :wh_no
                              and a.mmcode = :mmcode
                              and a.lot_no = :lot_no
                            order by a.exp_date";
            return DBWork.Connection.Query<ExpDateItem>(sql, new { mmcode = mmcode, wh_no = wh_no, lot_no = lot_no }, DBWork.Transaction);

        }


        public int GetMiWexpinvsN(string wh_no, string userId, string ip)
        {
            string sql = @"insert into ME_EXPD (exp_date, wh_no, mmcode, exp_seq, lot_no, reply_date,
                                                reply_time, create_time, create_id, update_time, update_id, ip)
                           select TRUNC(SYSDATE, 'mm') as exp_date,
                                  wh_no, mmcode, 1, nvl(lot_no, 'NA'), exp_date, sysdate,
                                  sysdate, :userId, sysdate, :userId, :ip
                             from (
                                    select c.wh_no, mmcode, exp_date, lot_no, inv_qty
                                      from MI_WEXPINV a,(
                                                select wh_no from MI_WHMAST where wh_kind = '0' and wh_grade < '3'
                                           ) c
                                     where a.wh_no = 'PH1S'
                                       and c.wh_no = :wh_no
                                       and trunc(exp_date,'mm') > trunc(sysdate,'mm')
                                       and trunc(exp_date,'mm') < trunc(add_months(sysdate,6),'mm')
                                       and not exists (
                                                select 1 from ME_EXPM b
                                                 where 1=1
                                                   and a.mmcode = b.mmcode
                                                   and a.exp_date = b.exp_date
                                                   and a.lot_no = b.lot_no
                                                   and b.closeflag = 'Y'
                                           )
                                    ) a
                            --不要重複回報
                            where not exists(
                                    select 1 from ME_EXPD b
                                    where a.wh_no = b.wh_no
                                      and a.mmcode = b.mmcode
                                      and a.exp_date = b.reply_date
                                      and a.lot_no = b.lot_no
                                  )";
            return DBWork.Connection.Execute(sql, new { wh_no = wh_no, userId = userId, ip = ip }, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMMCODECombo(string mmcode, string wh_no, int page_index, int page_size, string sorters)
        {
            DynamicParameters p = new DynamicParameters();

            string sql = @"SELECT {0} A.MMCODE , A.MMNAME_C, A.MMNAME_E, 
                            (select store_loc from MI_WLOCINV where wh_no = 'PH1S' and mmcode = a.mmcode and rownum = 1) as store_loc " +
                "from MI_MAST A, MI_WHINV B WHERE A.MMCODE=B.MMCODE ";

            if (wh_no != "" || wh_no != null)
            {
                sql += "AND B.WH_NO= :wh_no ";
                p.Add(":wh_no", wh_no);
            }

            if (mmcode != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,");
                p.Add(":MMCODE_I", mmcode);
                p.Add(":MMNAME_E_I", mmcode);
                p.Add(":MMNAME_C_I", mmcode);

                sql += " AND (A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}%", mmcode));

                sql += " OR MMNAME_E LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", mmcode));

                sql += " OR MMNAME_C LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", mmcode));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY MMCODE ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        #region 匯入匯出 2019-12-03新增
        public DataTable GetExcel(string wh_no, string exp_date, string status)
        {
            var p = new DynamicParameters();



            string sql = @"SELECT a.WH_NO as 庫房代碼,
                                  a.MMCODE as 院內碼,
                                  (select mmname_e from MI_MAST where mmcode = a.mmcode) as 英文品名,
                                  TWN_DATE(a.REPLY_DATE) as 回覆效期,
                                  a.LOT_NO as 藥品批號,
                                  a.EXP_QTY as 回覆藥量,
                                  (select inv_qty(:wh_no, a.mmcode) from dual) as 庫存量,
                                  (select listagg(store_loc, ',')
                                   within group (order by store_loc)
                                     from MI_WLOCINV
                                    where wh_no = 'PH1S'
                                      and mmcode = a.mmcode) as 藥庫儲位,
                                  (case when a.exp_stat = '1'
                                            then '未回報'
                                        else '已回報' end) as 回報狀態,                                 
                                  a.MEMO as 備註,
                                  TWN_DATE(a.REPLY_TIME) as 回覆日期,
                                  TWN_DATE(a.EXP_DATE) as 回覆月份
                             FROM ME_EXPD a
                            WHERE 1=1";
            //a.EXP_STAT = '1'";

            if (wh_no != "")
            {
                sql += " AND a.WH_NO = :wh_no";
                p.Add(":wh_no", string.Format("{0}", wh_no));
            }

            if (exp_date != "")
            {
                DateTime temp = DateTime.Parse(exp_date);

                int year = DateTime.Parse(exp_date).Year;
                int month = DateTime.Parse(exp_date).Month;
                string start = temp.ToString("yyyy-MM-dd");
                string end = temp.AddDays(DateTime.DaysInMonth(year, month) - 1).ToString("yyyy-MM-dd");

                sql += @"    AND TRUNC(a.EXP_DATE, 'DD')
                         BETWEEN TO_DATE(:p1,'YYYY-MM-DD') AND TO_DATE(:p2,'YYYY-MM-DD') ";

                p.Add(":p1", string.Format("{0}", start));
                p.Add(":p2", string.Format("{0}", end));
            }

            if (status.Trim() != string.Empty)
            {
                sql += string.Format("   and a.exp_stat = '{0}'", status);
            }

            sql += "  order by mmcode, a.REPLY_DATE";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public int UpdateExpdExpqty(string wh_no, string mmcode, string exp_date, string lot_no, string exp_qty, string memo, string reply_date, string userid, string ip) {
            string sql = @"update ME_EXPD set exp_qty = :exp_qty, memo = :memo, reply_id = :userid, update_time = sysdate, update_id = :userid, ip = :ip
                            where wh_no = :wh_no and mmcode = :mmcode and TWN_DATE(reply_date) = :reply_date and lot_no = :lot_no and TWN_DATE(exp_date) = :exp_date and exp_stat = '1'";
            return DBWork.Connection.Execute(sql, new { wh_no = wh_no, mmcode = mmcode, exp_date = exp_date, memo = memo, lot_no = lot_no, exp_qty = exp_qty, reply_date = reply_date, userid = userid, ip = ip }, DBWork.Transaction);
        }
        #endregion

        #region 2020-07-08 新增 匯出最近一筆效期
        public DataTable GetExcelSingle(string wh_no, string exp_date, string status) {
            var p = new DynamicParameters();

            string wh_no_string = string.Empty;
            string exp_date_string = string.Empty;
            string status_string = string.Empty;

            if (wh_no != "")
            {
                wh_no_string = " AND a.WH_NO = :wh_no";
                p.Add(":wh_no", string.Format("{0}", wh_no));
            }
            if (exp_date != "")
            {
                DateTime temp = DateTime.Parse(exp_date);

                int year = DateTime.Parse(exp_date).Year;
                int month = DateTime.Parse(exp_date).Month;
                string start = temp.ToString("yyyy-MM-dd");
                string end = temp.AddDays(DateTime.DaysInMonth(year, month) - 1).ToString("yyyy-MM-dd");

                exp_date_string  = @"    AND TRUNC(a.EXP_DATE, 'DD')
                         BETWEEN TO_DATE(:p1,'YYYY-MM-DD') AND TO_DATE(:p2,'YYYY-MM-DD') ";

                p.Add(":p1", string.Format("{0}", start));
                p.Add(":p2", string.Format("{0}", end));
            }
            if (status.Trim() != string.Empty)
            {
                status_string  = string.Format("   and a.exp_stat = '{0}'", status);
            }

            string sql = string.Format(@"
                select 院內碼,英文品名,回覆效期,藥品批號, 回覆藥量,庫存量,儲位,回報狀態,備註,回覆日期,回覆效期批號
                  from (
                        SELECT a.WH_NO as 庫房代碼,
                                  a.MMCODE as 院內碼,
                                  (select mmname_e from MI_MAST where mmcode = a.mmcode) as 英文品名,
                                  TWN_DATE(a.REPLY_DATE) as 回覆效期,
                                  a.LOT_NO as 藥品批號,
                                  a.EXP_QTY as 回覆藥量,
                                  (select inv_qty(:wh_no, a.mmcode) from dual) as 庫存量,
                                  (select listagg(store_loc, ',')
                                   within group (order by store_loc)
                                     from MI_WLOCINV
                                    where wh_no = 'PH1S'
                                      and mmcode = a.mmcode) as 儲位,
                                  (case when a.exp_stat = '1'
                                            then '未回報'
                                        else '已回報' end) as 回報狀態,                                 
                                  a.MEMO as 備註,
                                  TWN_DATE(a.REPLY_TIME) as 回覆日期,
                                  TWN_DATE(a.EXP_DATE) as 回覆月份,
                                  EXPD_REPLYDATE(a.EXP_DATE, a.WH_NO, a.MMCODE, a.EXP_SEQ) as 回覆效期批號
                             FROM ME_EXPD a
                            WHERE 1=1
                               {0}
                               {1}
                               {2}
                        ) A
                        inner join
                        (
                            select a.wh_no, a.mmcode, min(a.lot_no) as lot_no
                              from ME_EXPD a
                             where 1=1
                               {0}
                                {1}
                                {2}
                            group by a.wh_no, a.mmcode
                        )B
                      on a.庫房代碼 = b.wh_no and a.院內碼 = b.mmcode and a.藥品批號 = b.lot_no
                    order by a.院內碼, a.回覆日期
            ", wh_no_string, exp_date_string, status_string);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
        #endregion

        #region
        public int DeleteExpStat1(string wh_no) {
            string sql = @"
                delete from ME_EXPD
                 where wh_no = :wh_no
                   and exp_stat = 1
                   and twn_yyymm(reply_time) < twn_yyymm(sysdate)
            ";
            return DBWork.Connection.Execute(sql, new { wh_no }, DBWork.Transaction);
        }
        #endregion
    }
}