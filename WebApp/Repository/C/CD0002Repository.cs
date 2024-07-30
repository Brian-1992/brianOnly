using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Repository.C
{
    public class CD0002Repository : JCLib.Mvc.BaseRepository
    {
        public CD0002Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<BC_WHPICKDOC> GetMasterAll(string wh_no, string pick_date, string apply_kind, string isDistributed)
        {

            var p = new DynamicParameters();
            // TO_CHAR(a.PICK_DATE, 'YYYY-MM-DD') as PICK_DATE,
            string sql = @"SELECT a.WH_NO as WH_NO,
                                  TO_CHAR(a.PICK_DATE, 'YYYY-MM-DD') as PICK_DATE,
                                  a.DOCNO as DOCNO,
                                  :p2 as APPLY_KIND,
                                  (SELECT appid from ME_DOCM where docno=a.docno) as APPID,
                                  (SELECT (APPDEPT || ' ' || INID_NAME(APPDEPT) )from ME_DOCM where docno=a.docno) as APPDEPT_NAME,
                                  a.COMPLEXITY as COMPLEXITY,
                                  (SELECT count(*) from BC_WHPICK 
                                    WHERE wh_no=:p0 
                                      AND TRUNC(pick_date, 'DD') = TO_DATE(:p1,'YYYY-MM-DD') 
                                      AND docno=a.docno) as APPITEM_SUM,
                                  (SELECT sum(appqty) from BC_WHPICK 
                                    WHERE wh_no=:p0 
                                      AND TRUNC(pick_date, 'DD') = TO_DATE(:p1,'YYYY-MM-DD') 
                                      AND docno=a.docno) as APPQTY_SUM,
                                   NVL((select una from UR_ID 
                                     where tuser=((select pick_userid from BC_WHPICK 
                                                    where wh_no=a.wh_no 
                                                      and TRUNC(pick_date, 'DD') = TO_DATE(:p1,'YYYY-MM-DD') 
                                                      and docno=a.docno and rownum=1)
                                                    )
                                   ),' ') as picker_name,
                                  NVL((select pick_userid from BC_WHPICK 
                                                    where wh_no=a.wh_no 
                                                      and TRUNC(pick_date, 'DD') = TO_DATE(:p1,'YYYY-MM-DD') 
                                                      and docno=a.docno and rownum=1),' ') as pick_userid,
                                  (select apply_note from ME_DOCM 
                                    where docno=a.docno) as apply_note";
            if (isDistributed == "<>") {
                sql += "                 ,a.LOT_NO as LOT_NO ";
            }
            else
            {
                sql += "                 ,' ' as LOT_NO";
            }

            sql += @"                FROM BC_WHPICKDOC a
                                    WHERE a.WH_NO = :p0
                                      AND TRUNC(a.PICK_DATE, 'DD') = TO_DATE(:p1,'YYYY-MM-DD')
                                      AND a.APPLY_KIND = :p2";
            sql += string.Format("    AND a.LOT_NO {0} 0", isDistributed);

            p.Add(":p0", string.Format("{0}", wh_no));
            p.Add(":p1", string.Format("{0}", DateTime.Parse(pick_date).ToString("yyyy-MM-dd")));
            p.Add(":p2", string.Format("{0}", apply_kind));

            return DBWork.PagingQuery<BC_WHPICKDOC>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<BC_WHPICK> GetDetailAll(string wh_no, string pick_date, string docno) {

            var p = new DynamicParameters();

            string sql = @"SELECT WH_NO, PICK_DATE, DOCNO, SEQ, MMCODE, MMNAME_C, MMNAME_E, APPQTY, BASE_UNIT, STORE_LOC, APLYITEM_NOTE
                            FROM BC_WHPICK
                           WHERE WH_NO = :p0
                             AND TRUNC(PICK_DATE, 'DD') = TO_DATE(:p1,'YYYY-MM-DD')
                             AND DOCNO = :p2";

            p.Add(":p0", string.Format("{0}", wh_no));
            p.Add(":p1", string.Format("{0}", DateTime.Parse(pick_date).ToString("yyyy-MM-dd")));
            p.Add(":p2", string.Format("{0}", docno));

            return DBWork.Connection.Query<BC_WHPICK>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<BC_WHPICKDOC> GetMasterAllByTowh(string wh_no, string pick_date, string apply_kind, string isDistributed, bool pagingQuery) {
            var p = new DynamicParameters();
            string sql = string.Format(@"with temp_table as (
                               select a.wh_no,
                                      to_char(a.pick_date, 'YYYY-MM-DD') as pick_date,
                                      b.towh, b.mat_class , wh_name(b.towh) as wh_name,  
                                      count(distinct b.docno) as docno_counts,
                                      count(c.mmcode) as mmcode_counts,
                                      (a.complexity * count(distinct b.docno)) as complexity,
                                      sum(c.pick_qty) as appqty_sum,
                                      listagg(a.docno, ',') within group (order by a.docno) as docnos,
                                      a.lot_no {1}
                                 FROM BC_WHPICKDOC a, ME_DOCM b, ME_DOCD c
                                WHERE a.WH_NO = :p0 
                                  AND TRUNC(a.PICK_DATE, 'DD') = TO_DATE(:p1,'YYYY-MM-DD')
                                  and a.lot_no {0} 0
                                  and a.docno = b.docno
                                  and c.docno = b.docno   ", 
                                  isDistributed, 
                                  apply_kind == "3" ? string.Empty : ", a.apply_kind");
            if (apply_kind == "3")
            {
                sql += "          and b.mat_class in ('07', '08')";
            }
            else {
                sql += @"         and a.APPLY_KIND = :p2
                                  and b.mat_class in ('02','03','04','05','06')";
            }

            sql += string.Format(@"
                                                            
                                group by a.wh_no,to_char(a.pick_date, 'YYYY-MM-DD'), b.towh, b.mat_class, a.lot_no, a.complexity {0}
                                order by towh, b.mat_class
                            ),
                            temp_data as (
                                select a.wh_no, a.towh, a.wh_name, a.lot_no, a.pick_date {0} ,
                                       (select inid from MI_WHMAST where wh_no = a.towh) as inid,
                                       inid_name((select inid from MI_WHMAST where wh_no = a.towh)) as inid_name,
                                       sum(docno_counts) as docno_counts, sum(mmcode_counts) as mmcode_counts,
                                       a.complexity as complexity,
                                       sum(a.appqty_sum) as appqty_sum,
                                       (select listagg(''''||docno||'''',',') within group (order by docno) 
                                            from ME_DOCM where a.docnos like ('%'||docno||'%')
                                            group by towh
                                       ) as docnos
                                from temp_table a
                                group by wh_no, towh, wh_name, docnos, a.lot_no, a.complexity, a.pick_date {0}
                            )
                           select a.wh_no, a.towh, a.wh_name, a.inid, a.inid_name, a.pick_date {0},
                                  sum(a.docno_counts) as docno_counts, 
                                  sum(a.mmcode_counts) as mmcode_counts, 
                                  round(sum(a.complexity), 1) as complexity,
                                  sum(a.appqty_sum) as appqty_sum,
                                  NVL((select una from UR_ID 
                                     where tuser=((select pick_userid from BC_WHPICKLOT 
                                                    where wh_no=a.wh_no 
                                                      and TRUNC(pick_date, 'DD') = TO_DATE(:p1,'YYYY-MM-DD') 
                                                      and lot_no=a.lot_no and rownum=1)
                                                    )
                                   ),' ') as picker_name,
                                  listagg(docnos,',') within group (order by docnos) as docnos ",
                                  apply_kind == "3" ? string.Empty : ", a.apply_kind");
            if (isDistributed == "<>")
            {
                sql += "                 ,a.lot_no as lot_no";
            }
            else
            {
                sql += "                 ,' '  as lot_no ";
            }

            sql += string.Format(@"        from temp_data a
                            group by a.wh_no, a.towh, a.wh_name, a.inid, a.inid_name, a.lot_no, a.pick_date {0}",
                            apply_kind == "3" ? string.Empty : ", a.apply_kind");

            p.Add(":p0", string.Format("{0}", wh_no));
            p.Add(":p1", string.Format("{0}", DateTime.Parse(pick_date).ToString("yyyy-MM-dd")));
            p.Add(":p2", string.Format("{0}", apply_kind));

            if (pagingQuery) {
                return DBWork.PagingQuery<BC_WHPICKDOC>(sql, p, DBWork.Transaction);
            }

            return DBWork.Connection.Query<BC_WHPICKDOC>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<BC_WHPICK> GetDetailAllByDocnos(string wh_no, string pick_date, string docnos) {
            var p = new DynamicParameters();

            string sql = string.Format(@"SELECT WH_NO, PICK_DATE, DOCNO, SEQ, MMCODE, MMNAME_C, MMNAME_E, APPQTY, BASE_UNIT, STORE_LOC, APLYITEM_NOTE
                            FROM BC_WHPICK
                           WHERE WH_NO = :p0
                             AND TRUNC(PICK_DATE, 'DD') = TO_DATE(:p1,'YYYY-MM-DD')
                             AND DOCNO in ( {0} )", docnos);

            p.Add(":p0", string.Format("{0}", wh_no));
            p.Add(":p1", string.Format("{0}", DateTime.Parse(pick_date).ToString("yyyy-MM-dd")));

            return DBWork.Connection.Query<BC_WHPICK>(sql, p, DBWork.Transaction);
        }
        #region 查詢新申請單資料

        public int DeleteBcWhpick(string wh_no) {
            string sql = @" DELETE from BC_WHPICK
                             WHERE wh_no= :wh_no
                               AND pick_date > sysdate-5 
                               AND pick_date < sysdate+1
                               AND docno in (select docno from BC_WHPICKDOC
                                              where wh_no=:wh_no
                                                and pick_date > sysdate-5 
                                                and pick_date < sysdate+1 
                                                and lot_no = 0 )";
            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no }, DBWork.Transaction);
        }

        public int DeleteBcWhpickdoc(string wh_no)
        {
            string sql = @" DELETE from BC_WHPICKDOC
                             WHERE wh_no= :wh_no
                               AND pick_date > sysdate-5 
                               AND pick_date < sysdate+1
                               and lot_no = 0";
            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no }, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCM> GetMeDocms(string wh_no, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = @"SELECT a.FRWH, 
                                  a.DOCNO, 
                                  a.APPDEPT, 
                                  a.APPDEPT || ' ' || INID_NAME(a.APPDEPT) AS APPDEPT_NAME,
                                  a.APPLY_KIND,
                                  (select data_desc 
                                     from param_d 
                                    where grp_code='ME_DOCM' 
                                      and data_name='APPLY_KIND' 
                                      and data_value=a.apply_kind) as APPLY_KIND_N,
                                  (select count(*) from ME_DOCD where docno=a.docno) as APPCNT,
                                  (select sum(pick_qty) from ME_DOCD where docno=a.docno) as APPQTY_SUM
                             FROM ME_DOCM a
                            WHERE a.APPTIME > (SYSDATE -5)
                              AND a.FRWH = :wh_no
                              AND a.doctype in ('MR','MR1','MR2', 'MR3', 'MR4') and a.flowid in ('0103', '3')
                              AND NOT EXISTS (select 'x' from BC_WHPICKDOC where wh_no=a.frwh and docno=a.docno)";

            p.Add("wh_no", string.Format("{0}", wh_no));
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);


            return DBWork.Connection.Query<ME_DOCM>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCD> GetMeDocds(string docno) {

            string sql = @"SELECT a.SEQ, 
                                  a.MMCODE,
                                  (select mmname_c from MI_MAST where mmcode=a.mmcode) as MMNAME_C,
                                  (select mmname_e from MI_MAST where mmcode=a.mmcode) as MMNAME_E,
                                  a.pick_qty as appqty,
                                  (select base_unit from MI_MAST where mmcode=a.mmcode) as BASE_UNIT
                             FROM ME_DOCD a
                            WHERE a.DOCNO = :docno";

            return DBWork.Connection.Query<ME_DOCD>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public int InsertBcwhpick(string wh_no, string pick_date, string docno, string apply_kind, string userId, string ip) {
            string sql = @"INSERT into BC_WHPICK (wh_no, pick_date, docno, seq, mmcode, appqty, base_unit, aplyitem_note, mat_class, mmname_c, mmname_e,wexp_id, store_loc, 
                                                  create_date, create_user, update_time, update_user, update_ip)
                           SELECT :wh_no as wh_no, 
                                  TO_DATE(:pick_date, 'YYYY-MM-DD') as pick_date,
                                  a.docno, b.seq, b.mmcode, b.pick_qty as appqty,
                                  (select base_unit from MI_MAST where mmcode=b.mmcode) as base_unit,
                                  b.aplyitem_note,
                                  (select mat_class from MI_MAST where mmcode=b.mmcode) as mat_class,
                                  (select mmname_c from MI_MAST where mmcode=b.mmcode) as mmname_c,
                                  (select mmname_e from MI_MAST where mmcode=b.mmcode) as mmname_e,
                                  (select wexp_id from MI_MAST where mmcode=b.mmcode) as wexp_id,
                                  (select store_loc from MI_WLOCINV where wh_no = :wh_no and mmcode=b.mmcode and rownum = 1) as store_loc,
                                  SYSDATE as create_date, 
                                  :userid as create_user, 
                                  SYSDATE as update_time, 
                                  :userid as update_user, 
                                  :ip as update_ip
                             FROM ME_DOCM a,ME_DOCD b
                            WHERE a.docno=b.docno
                            ";
            if (docno != string.Empty) {
                sql += "  AND a.docno = :docno ";
            }
            sql += string.Format(@"
                          AND twn_date(a.apptime)>=(twn_pym(twn_yyymm(sysdate)) || '01') and a.frwh = :wh_no
                          AND a.doctype in ('MR','MR1','MR2', 'MR3', 'MR4') and a.flowid in ('0103', '3')
                          and a.mat_class in ( {0} )
                          AND not exists (select 'x' from BC_WHPICKDOC where wh_no=a.frwh and docno=a.docno)
                        ORDER BY docno
                  ", apply_kind == "3" ? " '07','08' " : " '02','03','04','05','06' ");

            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no,
                PICK_DATE = DateTime.Parse(pick_date).ToString("yyyy-MM-dd"),
                DOCNO = docno,
                USERID = userId,
                IP = ip
            }, DBWork.Transaction);

        }

        public int InsertBcwhpickdoc(string wh_no, string pick_date, string docno, string userId, string ip) {
            string sql = @" INSERT into BC_WHPICKDOC (wh_no,pick_date,docno,apply_kind,complexity,lot_no, CREATE_DATE, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)
                            SELECT :wh_no as WH_NO,
                                   TO_DATE(:pick_date, 'YYYY-MM-DD') as PICK_DATE,
                                   DOCNO,
                                   APPLY_KIND,
                                   1, 0, SYSDATE, :userid, SYSDATE, :userid, :ip
                              FROM ME_DOCM a
                             WHERE twn_date(apptime)>=(twn_pym(twn_yyymm(sysdate)) || '01') and frwh= :wh_no
                               and a.mat_class in ('02', '03', '04', '05', '06')";

            if (docno != string.Empty) {
                sql += "  AND a.DOCNO = :docno";
            }

            sql += @"     AND doctype in ('MR','MR1','MR2','MR3','MR4') and flowid in ('0103', '3')
                          AND NOT EXISTS (select 'x' from BC_WHPICKDOC where wh_no=a.frwh and docno=a.docno)
                        ORDER BY DOCNO";

            return DBWork.Connection.Execute(sql, new
            {
                WH_NO = wh_no,
                PICK_DATE = DateTime.Parse(pick_date).ToString("yyyy-MM-dd"),
                DOCNO = docno,
                USERID = userId,
                IP = ip
            }, DBWork.Transaction);
        }


        public IEnumerable<WhnoDocmCounts> GetWhnoDocmCounts(string wh_no) {
            string sql = @"select a.towh as towh, count(*) as total_docm_kind1 , 
                                  coalesce((select complexity from MM_ITEMS_COMP 
                                             where inid = a.towh), 1) as default_complexity,
                                  round(coalesce((select complexity from MM_ITEMS_COMP where inid = a.towh), 1) / count(*), 5) as avg_complexity 
                             from ME_DOCM a
                            where twn_date(apptime)>=(twn_pym(twn_yyymm(sysdate)) || '01') and frwh= :wh_no
                              and doctype in ('MR','MR1','MR2','MR3','MR4') 
                              and mat_class in ('02', '03', '04', '05', '06')
                              and flowid in ('0103', '3')
                              and apply_kind = '1'
                              and not exists (select 'x' from BC_WHPICKDOC where wh_no=a.frwh and docno=a.docno)
                            group by a.towh ";
            return DBWork.Connection.Query<WhnoDocmCounts>(sql, new { wh_no = wh_no}, DBWork.Transaction);
        }
        public int InsertBcwhpickdocByTowh(string wh_no, string pick_date, string complexity, string towh, string userid, string ip) {
            string sql = @" INSERT into BC_WHPICKDOC (wh_no,pick_date,docno,apply_kind,complexity,lot_no, CREATE_DATE, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)
                            SELECT :wh_no as WH_NO,
                                   TO_DATE(:pick_date, 'YYYY-MM-DD') as PICK_DATE,
                                   DOCNO,
                                   APPLY_KIND,
                                   (case
                                        when a.apply_kind = '1'
                                            then :complexity
                                        else '1'
                                    end)
                                    , 0, SYSDATE, :userid, SYSDATE, :userid, :ip
                              FROM ME_DOCM a
                             WHERE twn_date(apptime)>=(twn_pym(twn_yyymm(sysdate)) || '01') 
                               and frwh= :wh_no
                               and towh = :towh
                               and a.mat_class in ('02', '03', '04', '05', '06')";
            
            sql += @"     AND doctype in ('MR','MR1','MR2','MR3','MR4') and flowid in ('0103', '3')
                          AND NOT EXISTS (select 'x' from BC_WHPICKDOC where wh_no=a.frwh and docno=a.docno)
                        ORDER BY DOCNO";



            return DBWork.Connection.Execute(sql, new
            {
                wh_no = wh_no,
                pick_date = DateTime.Parse(pick_date).ToString("yyyy-MM-dd"),
                complexity = complexity,
                towh = towh,
                userid = userid,
                ip = ip
            }, DBWork.Transaction);
        }

        public int InsertBcwhpickdocKind3(string wh_no, string pick_date, string docno, string userId, string ip)
        {
            string sql = @" INSERT into BC_WHPICKDOC (wh_no,pick_date,docno,apply_kind,complexity,lot_no, CREATE_DATE, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)
                            SELECT :wh_no as WH_NO,
                                   TO_DATE(:pick_date, 'YYYY-MM-DD') as PICK_DATE,
                                   DOCNO,
                                   APPLY_KIND,
                                   1, 0, SYSDATE, :userid, SYSDATE, :userid, :ip
                              FROM ME_DOCM a
                             WHERE twn_date(apptime)>=(twn_pym(twn_yyymm(sysdate)) || '01') and frwh= :wh_no
                               and a.mat_class in ('07', '08')";

            if (docno != string.Empty)
            {
                sql += "  AND a.DOCNO = :docno";
            }

            sql += @"     AND doctype in ('MR','MR1','MR2','MR3','MR4') and flowid in ('0103', '3')
                          AND NOT EXISTS (select 'x' from BC_WHPICKDOC where wh_no=a.frwh and docno=a.docno)
                        ORDER BY DOCNO";

            return DBWork.Connection.Execute(sql, new
            {
                WH_NO = wh_no,
                PICK_DATE = DateTime.Parse(pick_date).ToString("yyyy-MM-dd"),
                DOCNO = docno,
                USERID = userId,
                IP = ip
            }, DBWork.Transaction);
        }
        #endregion

        #region 設定值日生
        public int DeleteDuty(string wh_no, string update_user, string ip) {
            string sql = @"UPDATE BC_WHID set IS_DUTY = '', 
                                              UPDATE_USER = :update_user, 
                                              UPDATE_TIME = SYSDATE, 
                                              UPDATE_IP = :ip
                            WHERE WH_NO = :wh_no and IS_DUTY IS NOT NULL";
            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, UPDATE_USER = update_user, IP = ip }, DBWork.Transaction);
        }

        public int UpdateDuty(string wh_no, string userId, string update_user, string ip)
        {
            string sql = @"UPDATE BC_WHID set IS_DUTY = 'Y', 
                                              UPDATE_USER = :update_user, 
                                              UPDATE_TIME = SYSDATE, 
                                              UPDATE_IP = :ip
                            WHERE WH_NO = :wh_no and WH_USERID = :userid";
            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, USERID = userId, UPDATE_USER = update_user, IP = ip }, DBWork.Transaction);
        }
        #endregion

        #region 複雜度設定

        public IEnumerable<BC_WHPICKDOC> GetComplexMaster(string wh_no, string pick_date) {
            string sql = @" select wh_no,pick_date,docno,
                                   (select appdept from ME_DOCM 
                                     where docno=a.docno) as appdept,
                                   (select b.appdept || ' ' || c.WH_NAME from ME_DOCM b, MI_WHMAST c 
                                     where b.docno=a.docno
                                       and b.appdept = c.wh_no) as APPDEPT_NAME,
                                   (select count(*) from BC_WHPICK 
                                     where wh_no= :wh_no 
                                       and TRUNC(pick_date, 'DD')=TO_DATE(:pick_date, 'YYYY-MM-DD') 
                                       and docno=a.docno) as appcnt,
                                   (select sum(appqty) from BC_WHPICK 
                                     where wh_no= :wh_no 
                                       and TRUNC(pick_date, 'DD')=TO_DATE(:pick_date, 'YYYY-MM-DD')
                                       and docno=a.docno) as appqty_sum,
                                   complexity
                              from BC_WHPICKDOC a
                             where wh_no= :wh_no 
                               and TRUNC(pick_date, 'DD')=TO_DATE(:pick_date, 'YYYY-MM-DD')
                               and apply_kind='1'
                             order by docno";

            return DBWork.Connection.Query<BC_WHPICKDOC>(sql,
                                                         new {
                                                             WH_NO = wh_no,
                                                             PICK_DATE = DateTime.Parse(pick_date).ToString("yyyy-MM-dd")
                                                         }, DBWork.Transaction);
        }

        public IEnumerable<BC_WHPICK> GetComplexDetail(string wh_no, string pick_date, string docno) {
            string sql = @"select wh_no,pick_date,docno,seq,mmcode,mmname_c,mmname_e,appqty,base_unit
                             from BC_WHPICK
                            where wh_no=:wh_no 
                              and TRUNC(pick_date, 'DD')=TO_DATE(:pick_date, 'YYYY-MM-DD')
                              and docno= :docno
                            order by seq";
            return DBWork.Connection.Query<BC_WHPICK>(sql,
                                                         new
                                                         {
                                                             WH_NO = wh_no,
                                                             PICK_DATE = DateTime.Parse(pick_date).ToString("yyyy-MM-dd"),
                                                             DOCNO = docno
                                                         }, DBWork.Transaction);
        }

        public int UpdateComplexity(BC_WHPICKDOC pickdoc) {
            string sql = @"update BC_WHPICKDOC set complexity=:complexity
                            where wh_no=:wh_no 
                              and TRUNC(pick_date, 'DD')=TO_DATE(:pick_date, 'YYYY-MM-DD')
                              and docno=:docno";
            return DBWork.Connection.Execute(sql, new
            {
                WH_NO = pickdoc.WH_NO,
                PICK_DATE = DateTime.Parse(pickdoc.PICK_DATE).ToString("yyyy-MM-dd"),
                DOCNO = pickdoc.DOCNO,
                COMPLEXITY = pickdoc.COMPLEXITY
            }, DBWork.Transaction);
        }

        public int UpdateComplexityByInid(string wh_no, string pick_date, string complexity, string docno_counts, string docnos)
        {
            string sql = string.Format(@"update BC_WHPICKDOC 
                                            set complexity=  round( :complexity / :docno_counts, 5)
                                          where wh_no=:wh_no 
                                            and TRUNC(pick_date, 'DD')=TO_DATE(:pick_date, 'YYYY-MM-DD')
                                            and docno in ( {0} )", docnos);
            return DBWork.Connection.Execute(sql, new
            {
                wh_no = wh_no,
                pick_date = DateTime.Parse(pick_date).ToString("yyyy-MM-dd"),
                docno_counts = docno_counts,
                complexity = complexity
            }, DBWork.Transaction);
        }

        #endregion

        #region 分配揀貨員

        public int GetMasterCount(string wh_no, string pick_date, string apply_kind) {
            var p = new DynamicParameters();
            // TO_CHAR(a.PICK_DATE, 'YYYY-MM-DD') as PICK_DATE,
            string sql = @"select count(*) from BC_WHPICKDOC
                            WHERE WH_NO = :wh_no
                              AND TRUNC(PICK_DATE, 'DD') = TO_DATE(:pick_date,'YYYY-MM-DD')
                              AND APPLY_KIND = :apply_kind
                              AND LOT_NO = 0";

            return DBWork.Connection.QueryFirst<int>(sql,
                                                         new
                                                         {
                                                             WH_NO = wh_no,
                                                             PICK_DATE = DateTime.Parse(pick_date).ToString("yyyy-MM-dd"),
                                                             APPLY_KIND = apply_kind
                                                         }, DBWork.Transaction);
        }


        // 取得臨時單最大批號
        public int GetTempMaxLotNo(string wh_no, string pick_date) {
            string sql = @"select NVL2(MAX(lot_no), MAX(lot_no), 1000) as max_lotno from BC_WHPICKLOT
                            where wh_no=:wh_no 
                              and TRUNC(pick_date, 'DD') = TO_DATE(:pick_date, 'YYYY-MM-DD')
                              and exists (select 'x' from BC_WHPICKDOC 
                                           where wh_no=:wh_no 
                                             and TRUNC(pick_date, 'DD') = TO_DATE(:pick_date, 'YYYY-MM-DD')
                                             and apply_kind='2')";
            return DBWork.Connection.QueryFirst<int>(sql,
                                                         new
                                                         {
                                                             WH_NO = wh_no,
                                                             PICK_DATE = DateTime.Parse(pick_date).ToString("yyyy-MM-dd")
                                                         }, DBWork.Transaction);
        }


        public int InsertBcwhpicklot(string wh_no, string pick_date, int lot_no, string pick_userid, string userId, string ip) {
            string sql = @"INSERT INTO BC_WHPICKLOT (wh_no, pick_date, lot_no, pick_userid, pick_status, create_date, create_user, update_time, update_user, update_ip)
                           VALUES (:wh_no, TO_DATE(:pick_date, 'YYYY-MM-DD'), :lot_no, :pick_userid, 'A', SYSDATE, :userid, SYSDATE, :userid, :ip)";
            return DBWork.Connection.Execute(sql, new
            {
                WH_NO = wh_no,
                PICK_DATE = DateTime.Parse(pick_date).ToString("yyyy-MM-dd"),
                LOT_NO = lot_no,
                PICK_USERID = pick_userid,
                USERID = userId,
                IP = ip
            }, DBWork.Transaction);
        }

        public int DeleteTemplotsum(string wh_no) {
            string sql = @"delete from BC_WHPICK_TEMP_LOTSUM
                            where wh_no=:wh_no and calc_time<sysdate";
            return DBWork.Connection.Execute(sql, new
            {
                WH_NO = wh_no
            }, DBWork.Transaction);
        }

        public int DeleteTemplotdoc(string wh_no) {
            string sql = @"delete from BC_WHPICK_TEMP_LOTDOC
                            where wh_no=:wh_no and calc_time<sysdate";
            return DBWork.Connection.Execute(sql, new
            {
                WH_NO = wh_no
            }, DBWork.Transaction);
        }
        public int GetMaxLotNo(string wh_no, string pick_date) {
            string sql = @"SELECT NVL2(MAX(lot_no), MAX(lot_no), 0) from BC_WHPICKLOT
                            WHERE wh_no = :wh_no
                              AND TRUNC(pick_date, 'DD') = TO_DATE(:pick_date, 'YYYY-MM-DD')
                              AND LOT_NO < 1000";
            return DBWork.Connection.QueryFirst<int>(sql,
                                                         new
                                                         {
                                                             WH_NO = wh_no,
                                                             PICK_DATE = DateTime.Parse(pick_date).ToString("yyyy-MM-dd")
                                                         }, DBWork.Transaction);
        }
        public IEnumerable<BC_WHPICKDOC> GetBcWhpickdoc(string wh_no, string pick_date) {
            string sql = @"SELECT WH_NO, PICK_DATE, DOCNO, COMPLEXITY
                             FROM BC_WHPICKDOC
                            WHERE WH_NO = :WH_NO
                              AND TRUNC(pick_date, 'DD') = TO_DATE(:pick_date, 'YYYY-MM-DD')
                              AND APPLY_KIND = '1' and LOT_NO = 0
                            ORDER BY COMPLEXITY DESC";
            return DBWork.Connection.Query<BC_WHPICKDOC>(sql,
                                               new
                                               {
                                                   WH_NO = wh_no,
                                                   PICK_DATE = DateTime.Parse(pick_date).ToString("yyyy-MM-dd")
                                               }, DBWork.Transaction);

        }
        public int InsertTemplotsum(string wh_no, string calc_time, int lot_no, float complexity_sum, int docno_sum, int appitem_sum, string userId, string ip) {
            string sql = @"INSERT INTO BC_WHPICK_TEMP_LOTSUM (wh_no, calc_time, lot_no, complexity_sum, docno_sum, appitem_sum, create_date, create_user, update_time, update_user, update_ip)
                            VALUES (:wh_no, TO_DATE(:calc_time, 'YYYY-MM-DD HH24:MI:SS'), :lot_no, :complexity_sum, :docno_sum, :appitem_sum, SYSDATE, :userid, SYSDATE, :userid, :ip)";
            return DBWork.Connection.Execute(sql, new
            {
                WH_NO = wh_no,
                CALC_TIME = calc_time,
                LOT_NO = lot_no,
                COMPLEXITY_SUM = complexity_sum,
                DOCNO_SUM = docno_sum,
                APPITEM_SUM = appitem_sum,
                USERID = userId,
                IP = ip
            }, DBWork.Transaction);
        }
        public int InsertTemplotdoc(string wh_no, string calc_time, int lot_no, string docno)
        {
            string sql = @"INSERT INTO BC_WHPICK_TEMP_LOTDOC (wh_no, calc_time, lot_no, docno)
                            VALUES (:wh_no, TO_DATE(:calc_time, 'YYYY-MM-DD HH24:MI:SS'), :lot_no, :docno)";
            return DBWork.Connection.Execute(sql, new
            {
                WH_NO = wh_no,
                CALC_TIME = calc_time,
                LOT_NO = lot_no,
                DOCNO = docno
            }, DBWork.Transaction);
        }

        public IEnumerable<BC_WHPICK_TEMP_LOTSUM> GetTemplotsums(string wh_no, string calc_time) {
            string sql = @"select wh_no, 
                                  TO_CHAR(calc_time, 'YYYY-MM-DD HH24:MI') as calc_time, 
                                  calc_time as calc_time_test_string,
                                  calc_time as calc_time_test_datetime,
                                  lot_no,docno_sum,appitem_sum,complexity_sum
                             from BC_WHPICK_TEMP_LOTSUM a
                            where wh_no=:wh_no 
                              and TRUNC(calc_time, 'MI')=TO_DATE(:calc_time, 'YYYY-MM-DD HH24:MI')
                            order by lot_no";
            return DBWork.Connection.Query<BC_WHPICK_TEMP_LOTSUM>(sql, new { WH_NO = wh_no, CALC_TIME = calc_time }, DBWork.Transaction);
        }

        public IEnumerable<BC_WHPICK_TEMP_LOTSUM> GetBcWhpickDocByInid(string wh_no, string calc_time)
        {
            string sql = @"with queryA as (
                                           select a.wh_no, 
                                                  TO_CHAR(a.calc_time, 'YYYY-MM-DD HH24:MI') as calc_time, 
                                                  a.lot_no, a.docno_sum,a.appitem_sum,a.complexity_sum, b.docno, c.towh,
                                                  ((select '02' from dual where c.mat_class in ('02')) || 
                                                    (select '0X' from dual where c.mat_class in ('03','04','05','06'))) as matcls_grp
                                             from BC_WHPICK_TEMP_LOTSUM a, BC_WHPICK_TEMP_LOTDOC b, ME_DOCM c
                                            where a.wh_no=:wh_no 
                                              and TRUNC(a.calc_time, 'MI')=TO_DATE(:calc_time, 'YYYY-MM-DD HH24:MI')
                                              and b.wh_no = a.wh_no
                                              and b.calc_time = a.calc_time
                                              and b.lot_no = a.lot_no
                                              and c.docno = b.docno
                                            order by a.lot_no )  
                          select a.wh_no, a.calc_time, lot_no,docno_sum,appitem_sum,complexity_sum, towh, 
                                    listagg(a.matcls_grp, ',') within group (order by a.matcls_grp) as matcls_grps,        
                                    listagg(''''||a.docno||'''', ',') within group (order by a.docno) as docnos
                            from queryA a
                           group by a.wh_no, a.calc_time, lot_no,docno_sum,appitem_sum,complexity_sum, towh
                           order by lot_no";
            return DBWork.Connection.Query<BC_WHPICK_TEMP_LOTSUM>(sql, new { WH_NO = wh_no, CALC_TIME = calc_time }, DBWork.Transaction);
        }

        public int UpdateBcwhpickdocTemp(string wh_no, string pick_date, int lot_no, string userid, string ip, bool isTemp, bool isTempExists)
        {
            string sql = @"UPDATE BC_WHPICKDOC
                              SET lot_no = :lot_no, UPDATE_TIME = SYSDATE, UPDATE_USER = :userid, update_ip = :ip
                            WHERE WH_NO = :wh_no
                              and TRUNC(pick_date, 'DD')=TO_DATE(:pick_date, 'YYYY-MM-DD')
                              AND apply_kind = '2'";

            if (isTempExists)
            {
                sql += " and lot_no = 0";
            }

            return DBWork.Connection.Execute(sql, new
            {
                WH_NO = wh_no,
                PICK_DATE = DateTime.Parse(pick_date).ToString("yyyy-MM-dd"),
                LOT_NO = lot_no,
                USERID = userid,
                IP = ip
            }, DBWork.Transaction);
        }
        public int UpdateBcwhpickTemp(string wh_no, string pick_date, int lot_no, string pick_userid, string userid, string ip)
        {
            string sql = @"UPDATE BC_WHPICK
                              SET pick_userid = :pick_userid, UPDATE_TIME = SYSDATE, UPDATE_USER = :userid, update_ip = :ip
                            WHERE WH_NO = :wh_no
                              and TRUNC(pick_date, 'DD')=TO_DATE(:pick_date, 'YYYY-MM-DD')
                              AND docno in (select docno from BC_WHPICKDOC
                                             where wh_no = :wh_no
                                               and TRUNC(pick_date, 'DD')=TO_DATE(:pick_date, 'YYYY-MM-DD')
                                               and lot_no = :lot_no)";

            return DBWork.Connection.Execute(sql, new
            {
                WH_NO = wh_no,
                PICK_DATE = DateTime.Parse(pick_date).ToString("yyyy-MM-dd"),
                LOT_NO = lot_no,
                PICK_USERID = pick_userid,
                USERID = userid,
                IP = ip
            }, DBWork.Transaction);
        }
        public int UpdateBcwhpickdocReg(string wh_no, string pick_date, string calc_time, int lot_no, string userid, string ip)
        {
            string sql = @"UPDATE BC_WHPICKDOC
                              SET lot_no = :lot_no, UPDATE_TIME = SYSDATE, UPDATE_USER = :userid, update_ip = :ip
                            WHERE WH_NO = :wh_no
                              and TRUNC(pick_date, 'DD')=TO_DATE(:pick_date, 'YYYY-MM-DD')
                              AND DOCNO in  (select docno from BC_WHPICK_TEMP_LOTDOC 
                                              where wh_no=:wh_no
                                                and TRUNC(calc_time, 'MI')=TO_DATE(:calc_time, 'YYYY-MM-DD HH24:MI')
                                                and lot_no=:lot_no)
";
            return DBWork.Connection.Execute(sql, new
            {
                WH_NO = wh_no,
                PICK_DATE = DateTime.Parse(pick_date).ToString("yyyy-MM-dd"),
                CALC_TIME = DateTime.Parse(calc_time).ToString("yyyy-MM-dd HH:mm"),
                LOT_NO = lot_no,
                USERID = userid,
                IP = ip
            }, DBWork.Transaction);
        }
        public int UpdateBcwhpickReg(string wh_no, string pick_date, string calc_time, int lot_no, string pick_userid, string userid, string ip)
        {
            string sql = @"UPDATE BC_WHPICK
                              SET pick_userid = :pick_userid, UPDATE_TIME = SYSDATE, UPDATE_USER = :userid, update_ip = :ip
                            WHERE WH_NO = :wh_no
                              and TRUNC(pick_date, 'DD')=TO_DATE(:pick_date, 'YYYY-MM-DD')
                              AND docno in (select docno from BC_WHPICK_TEMP_LOTDOC
                                             where wh_no = :wh_no
                                                and TRUNC(calc_time, 'MI')=TO_DATE(:calc_time, 'YYYY-MM-DD HH24:MI')
                                               and lot_no = :lot_no)";

            return DBWork.Connection.Execute(sql, new
            {
                WH_NO = wh_no,
                PICK_DATE = DateTime.Parse(pick_date).ToString("yyyy-MM-dd"),
                CALC_TIME = DateTime.Parse(calc_time).ToString("yyyy-MM-dd HH:mm"),
                LOT_NO = lot_no,
                PICK_USERID = pick_userid,
                USERID = userid,
                IP = ip
            }, DBWork.Transaction);
        }

        #endregion

        #region 分配減貨源 by inid 2020-04-06
        public IEnumerable<BC_WHPICK_TEMP_LOTSUM> GetTemplotsumsN(string wh_no, string calc_time) {
            string sql = @"select a.wh_no, 
                                  TO_CHAR(a.calc_time, 'YYYY-MM-DD HH24:MI') as calc_time, 
                                  a.lot_no,a.docno_sum,a.appitem_sum,a.complexity_sum, b.docno,
                                  ((select '02' from dual where exists (select 1 from ME_DOCM where docno = b.docno and mat_class = '02'))
                                  ||(select '0X' from dual where exists (select 1 from ME_DOCM where docno = b.docno and mat_class in ('03','04','05','06'))) ) as matcls_grp
                             from BC_WHPICK_TEMP_LOTSUM a, BC_WHPICK_TEMP_LOTDOC b
                            where a.wh_no=:wh_no 
                              and TRUNC(a.calc_time, 'MI')=TO_DATE(:calc_time, 'YYYY-MM-DD HH24:MI')
                              and b.wh_no = a.wh_no
                              and b.calc_time = a.calc_time
                              and b.lot_no = a.lot_no
                            order by a.lot_no";

            return DBWork.Connection.Query<BC_WHPICK_TEMP_LOTSUM>(sql, new { WH_NO = wh_no, CALC_TIME = calc_time }, DBWork.Transaction); ;
        }
        #endregion

        #region kind3 2020-04-07
        public IEnumerable<string> CheckManageridExists(string wh_no, string pick_date, string docnos) {
            string sql = string.Format(@"select distinct mmcode from BC_WHPICK  a
                                          where a.wh_no = :wh_no
                                            and TRUNC(a.pick_date, 'DD') = TO_DATE(:pick_date,'YYYY-MM-DD')
                                            and a.docno in ( {0} )
                                            and not exists (select 1 from BC_ITMANAGER 
                                                             where wh_no = a.wh_no and mmcode = a.mmcode)", 
                                                             docnos);
            return DBWork.Connection.Query<string>(sql, 
                new {
                    wh_no = wh_no,
                    pick_date = DateTime.Parse(pick_date).ToString("yyyy-MM-dd"),
                    docnos = docnos
                }, DBWork.Transaction);
        }

        public string GetKind3Pickusers(string wh_no, string pick_date, string docnos) {
            string sql = string.Format(@"select managerid from BC_ITMANAGER a
                            where a.wh_no = :wh_no
                            and mmcode in (select mmcode from BC_WHPICK 
                                            where wh_no = a.wh_no 
                                              and TRUNC(pick_date, 'DD') = TO_DATE(:pick_date,'YYYY-MM-DD')
                                              and docno in ( {0} )
                            )
                            and rownum = 1", docnos);
            return DBWork.Connection.QueryFirstOrDefault<string>(sql,
                new
                {
                    wh_no = wh_no,
                    pick_date = DateTime.Parse(pick_date).ToString("yyyy-MM-dd"),
                    docnos = docnos
                }, DBWork.Transaction);
        }

        public int UpdateKind3PickDocs(string wh_no, string pick_date, string docnos, int lot_no, string userid, string ip) {
            string sql = string.Format(@"update BC_WHPICKDOC 
                                            set lot_no = :lot_no, update_time = sysdate, 
                                                update_user = :userid, update_ip = :ip 
                                          where wh_no = :wh_no 
                                            and TRUNC(pick_date, 'DD')=TO_DATE(:pick_date, 'YYYY-MM-DD')
                                            and docno in ( {0} )", docnos);
            return DBWork.Connection.Execute(sql, new
            {
                wh_no = wh_no,
                pick_date = DateTime.Parse(pick_date).ToString("yyyy-MM-dd"),
                docnos = docnos,
                lot_no = lot_no,
                userid = userid,
                ip = ip
            }, DBWork.Transaction);
        }

        public int UpdateKind3Picks(string wh_no, string pick_date, string docnos, int lot_no, string userid, string ip) {
            string sql = string.Format(@"update BC_WHPICK a
                                            set a.pick_userid = (select managerid from BC_ITMANAGER where mmcode = a.mmcode),
                                                a.update_time = sysdate, 
                                                a.update_user = :userid, a.update_ip = :ip 
                                          where a.wh_no = :wh_no 
                                            and TRUNC(a.pick_date, 'DD')=TO_DATE(:pick_date, 'YYYY-MM-DD')
                                            and a.docno in ( {0} )", docnos);
            return DBWork.Connection.Execute(sql, new
            {
                wh_no = wh_no,
                pick_date = DateTime.Parse(pick_date).ToString("yyyy-MM-dd"),
                docnos = docnos,
                lot_no = lot_no,
                userid = userid,
                ip = ip
            }, DBWork.Transaction);
        }
        #endregion

        #region 已排揀貨批次

        public IEnumerable<BC_WHPICKLOT> GetBcwhpicklots(string wh_no, string pick_date, string apply_kind) {

            var p = new DynamicParameters();

            string sql = string.Format(@"SELECT a.lot_no,
                                                {0}
                                                TO_CHAR(a.PICK_DATE, 'YYYY-MM-DD') as PICK_DATE,
                                                a.WH_NO as WH_NO,
                                                (select count(*) as app_cnt 
                                                   from BC_WHPICKDOC 
                                                  where wh_no = :wh_no
                                                    and TRUNC(pick_date, 'DD') = TO_DATE(:pick_date,'YYYY-MM-DD')
                                                    and lot_no = a.lot_no) as app_cnt
                                           FROM BC_WHPICKLOT a
                                          WHERE a.wh_no = :wh_no
                                            AND TRUNC(a.pick_date, 'DD') = TO_DATE(:pick_date,'YYYY-MM-DD')
                                            AND a.lot_no in (SELECT lot_no from BC_WHPICKDOC b
                                                           WHERE b.wh_no = :wh_no 
                                                             AND TRUNC(b.pick_date, 'DD') = TO_DATE(:pick_date, 'YYYY-MM-DD')
                                                             and exists (select 1 from ME_DOCM
                                                                          where docno = b.docno
                                                                            and mat_class in ( {1} )
                                                                         )
                                                             {2}
                                                )",
                                                apply_kind == "3" ? string.Empty : "USER_NAME(pick_userid) as PICK_USERNAME,",
                                                apply_kind == "3" ? " '07','08' " : " '02','03','04','05','06' ",
                                                apply_kind == "3" ? string.Empty : "  and apply_kind = :apply_kind");

            p.Add(":wh_no", wh_no);
            p.Add(":pick_date", DateTime.Parse(pick_date).ToString("yyyy-MM-dd"));
            p.Add(":apply_kind", apply_kind);

            return DBWork.PagingQuery<BC_WHPICKLOT>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<BC_WHPICK> GetPicksByLotno(string wh_no, string pick_date, int lot_no, int page_index, int page_size, string sorters) {
            var p = new DynamicParameters();

            //string sql = @"select docno,seq,mmcode,mmname_c,mmname_e,appqty,base_unit,store_loc,
            //                      (select aplyitem_note from ME_DOCD where docno=a.docno and seq=a.seq) as aplyitem_note
            //                 from BC_WHPICK a
            //                where wh_no=:wh_no 
            //                  AND TRUNC(pick_date, 'DD') = TO_DATE(:pick_date,'YYYY-MM-DD')
            //                  and (select lot_no from BC_WHPICKDOC 
            //                        where wh_no=:wh_no 
            //                          AND TRUNC(pick_date, 'DD') = TO_DATE(:pick_date,'YYYY-MM-DD')
            //                          and docno=a.docno)=:lot_no";

            string sql = @"select docno,seq,mmcode,mmname_c,mmname_e,appqty,base_unit,store_loc,
                                  (select towh from ME_DOCM where docno=a.docno) as appdept,
                                  WH_NAME((select towh from ME_DOCM where docno=a.docno)) as appdeptname,
                                  (select mat_clsname from MI_MATCLASS 
                                    where mat_class = (select mat_class from MI_MAST where mmcode = a.mmcode)) as mat_class,
                                  (select aplyitem_note from ME_DOCD where docno=a.docno and seq=a.seq) as aplyitem_note,
                                  pick_userid, USER_NAME(pick_userid) as pick_username
                             from BC_WHPICK a
                            where wh_no=:wh_no 
                              AND TRUNC(pick_date, 'DD') = TO_DATE(:pick_date,'YYYY-MM-DD')
                              and (select lot_no from BC_WHPICKDOC 
                                    where wh_no=:wh_no 
                                      AND TRUNC(pick_date, 'DD') = TO_DATE(:pick_date,'YYYY-MM-DD')
                                      and docno=a.docno)=:lot_no";

            sql += string.Format("  order by docno, {0}", sorters);

            p.Add("WH_NO", wh_no);
            p.Add("PICK_DATE", DateTime.Parse(pick_date).ToString("yyyy-MM-dd"));
            p.Add("LOT_NO", lot_no);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<BC_WHPICK>(GetPagingStatement(sql, ""), p, DBWork.Transaction);
        }

        public IEnumerable<CD0002> Print(string wh_no, string pick_date, int lot_no, string sorters)
        {

            var p = new DynamicParameters();

            string sql = string.Format(@" select (select wh_no || ' ' || wh_name from mi_whmast where wh_no = :wh_no) as WH_NO, 
                                                 :pick_date as PICK_DATE,
                                                 '{0}' as SORTER,
                                                  docno,seq,mmcode,mmname_c,mmname_e,appqty,base_unit, store_loc,
                                                  (select aplyitem_note from ME_DOCD where docno=a.docno and seq=a.seq) as aplyitem_note,
                                                   :lot_no as LOT_NO,
                                                (select tuser ||  ' ' || USER_NAME(tuser) from ur_id where tuser = a.pick_userid) as PICK_USERID
                                            from BC_WHPICK a
                                           where wh_no=:wh_no 
                                             AND TRUNC(pick_date, 'DD') = TO_DATE(:pick_date,'YYYY-MM-DD')
                                             and (select lot_no from BC_WHPICKDOC 
                                                   where wh_no=:wh_no 
                                                     AND TRUNC(pick_date, 'DD') = TO_DATE(:pick_date,'YYYY-MM-DD')
                                                     and docno=a.docno)=:lot_no
                                            ORDER BY {0}", sorters);
            p.Add("wh_no", wh_no);
            p.Add("pick_date", DateTime.Parse(pick_date).ToString("yyyy-MM-dd"));
            p.Add("lot_no", lot_no);

            return DBWork.Connection.Query<CD0002>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<CD0002> PrintMulti(string wh_no, string pick_date, string[] lot_nos, string sorters)
        {

            var p = new DynamicParameters();

            string sql = string.Format(@" select (select wh_no || ' ' || wh_name from mi_whmast where wh_no = :wh_no) as WH_NO, 
                                                 :pick_date as PICK_DATE,
                                                 '{0}' as SORTER,
                                                 (select towh from ME_DOCM where docno=a.docno) as appdept,
                                                 wh_name((select towh from ME_DOCM where docno=a.docno)) as appdeptname,
                                                 a.docno,a.seq,a.mmcode,a.mmname_c,a.mmname_e,a.appqty,a.base_unit, a.store_loc,
                                                 (select aplyitem_note from ME_DOCD where docno=a.docno and seq=a.seq) as aplyitem_note,
                                                 b.lot_no as LOT_NO,
                                                 (select USER_NAME(tuser) from ur_id where tuser = a.pick_userid) as PICK_USERID,
                                                 ((select '02' from dual where c.mat_class in ('02')) || 
                                                  (select '0X' from dual where c.mat_class in ('03','04','05','06')) ||
                                                  (select '07' from dual where c.mat_class in ('07')) ||
                                                  (select '08' from dual where c.mat_class in ('08'))) as matcls_grp,
                                                 c.APPID as appid,
                                                 (user_name(c.APPID) ||'(' || (select ext from UR_ID where tuser = c.appid)||')') as appid_name
                                                 
                                            from BC_WHPICK a, BC_WHPICKDOC b, ME_DOCM c
                                           where a.wh_no=:wh_no 
                                             AND TRUNC(a.pick_date, 'DD') = TO_DATE(:pick_date,'YYYY-MM-DD')
                                             and a.wh_no = b.wh_no
                                             and a.pick_date = b.pick_date
                                             and a.docno  = b.docno
                                             and b.lot_no in :lot_nos
                                             and c.docno = a.docno
                                           order by COALESCE(TO_NUMBER(REGEXP_SUBSTR({0}, '^\d+')), 9999999999)", sorters);
            p.Add("wh_no", wh_no);
            p.Add("pick_date", DateTime.Parse(pick_date).ToString("yyyy-MM-dd"));
            p.Add("lot_nos", lot_nos);

            return DBWork.Connection.Query<CD0002>(sql, p, DBWork.Transaction);
        }


        public int UpdateBcwhpickPickuser(string wh_no, string pick_date, string lot_no, string userid, string ip) {

            string sql = @"update BC_WHPICK a set pick_userid='', update_time = sysdate, update_user = :userid, update_ip = :ip
                            where wh_no=:wh_no 
                              AND TRUNC(pick_date, 'DD') = TO_DATE(:pick_date,'YYYY-MM-DD')
                              and (select lot_no from BC_WHPICKDOC 
                                    where wh_no=:wh_no
                                      AND TRUNC(pick_date, 'DD') = TO_DATE(:pick_date,'YYYY-MM-DD')
                                      and docno=a.docno)=:lot_no";

            return DBWork.Connection.Execute(sql, new
            {
                WH_NO = wh_no,
                PICK_DATE = DateTime.Parse(pick_date).ToString("yyyy-MM-dd"),
                LOT_NO = lot_no,
                USERID = userid,
                IP = ip
            }, DBWork.Transaction);
        }
        public int UpdateBcwhpickdocLotno(string wh_no, string pick_date, string lot_no, string userid, string ip)
        {
            string sql = @"update BC_WHPICKDOC a set lot_no='0', update_time = sysdate, update_user = :userid, update_ip = :ip
                            where wh_no=:wh_no 
                              AND TRUNC(pick_date, 'DD') = TO_DATE(:pick_date,'YYYY-MM-DD')
                              and lot_no=:lot_no";

            return DBWork.Connection.Execute(sql, new
            {
                WH_NO = wh_no,
                PICK_DATE = DateTime.Parse(pick_date).ToString("yyyy-MM-dd"),
                LOT_NO = lot_no,
                USERID = userid,
                IP = ip
            }, DBWork.Transaction);
        }

        public int DeleteBcwhpicklot(string wh_no, string pick_date, string lot_no)
        {
            string sql = @"delete from BC_WHPICKLOT
                            where wh_no=:wh_no 
                              AND TRUNC(pick_date, 'DD') = TO_DATE(:pick_date,'YYYY-MM-DD')
                              and lot_no=:lot_no";

            return DBWork.Connection.Execute(sql, new
            {
                WH_NO = wh_no,
                PICK_DATE = DateTime.Parse(pick_date).ToString("yyyy-MM-dd"),
                LOT_NO = lot_no
            }, DBWork.Transaction);
        }
        #endregion

        #region 全部重新分配
        public int UpdateBcwhpickForClearAll(string wh_no, string pick_date, string apply_kind, string userid, string ip) {

            string sql = string.Format(@"update BC_WHPICK a set pick_userid='', update_time = sysdate, update_user = :userid, update_ip = :ip
                            where wh_no=:wh_no 
                              and TRUNC(pick_date, 'DD') = TO_DATE(:pick_date,'YYYY-MM-DD')
                              and docno in (select docno from BC_WHPICKDOC b
                                             where b.wh_no=:wh_no 
                                               AND TRUNC(b.pick_date, 'DD') = TO_DATE(:pick_date,'YYYY-MM-DD')
                                               and b.docno=a.docno
                                               and b.lot_no is not null
                                               and exists (select 1 from ME_DOCM
                                                                          where docno = b.docno
                                                                            and mat_class in ( {0} )
                                                                         )
                                                             {1}
                                            )",
                                                apply_kind == "3" ? " '07','08' " : " '02','03','04','05','06' ",
                                                apply_kind == "3" ? string.Empty : "  and apply_kind = :apply_kind");

            return DBWork.Connection.Execute(sql, new
            {
                WH_NO = wh_no,
                PICK_DATE = DateTime.Parse(pick_date).ToString("yyyy-MM-dd"),
                APPLY_KIND = apply_kind,
                USERID = userid,
                IP = ip
            }, DBWork.Transaction);
        }
        public int DeleteBcwhpicklotForClearAll(string wh_no, string pick_date, string apply_kind)
        {
            string sql = string.Format(@"delete from BC_WHPICKLOT a
                            where a.wh_no=:wh_no 
                              and TRUNC(a.pick_date, 'DD') = TO_DATE(:pick_date,'YYYY-MM-DD')
                              and exists (select 'x' from BC_WHPICKDOC b
                                           where b.wh_no=:wh_no 
                                             and TRUNC(b.pick_date, 'DD') = TO_DATE(:pick_date,'YYYY-MM-DD')
                                             and b.lot_no=a.lot_no
                                             and exists (select 1 from ME_DOCM
                                                          where docno = b.docno
                                                            and mat_class in ( {0} )
                                                                          )
                                                              {1}
                                        )",
                                        apply_kind == "3" ? " '07','08' " : " '02','03','04','05','06' ",
                                        apply_kind == "3" ? string.Empty : "  and apply_kind = :apply_kind");
            return DBWork.Connection.Execute(sql, new
            {
                WH_NO = wh_no,
                PICK_DATE = DateTime.Parse(pick_date).ToString("yyyy-MM-dd"),
                APPLY_KIND = apply_kind
            }, DBWork.Transaction);
        }
        public int UpdateBcwhpickdocForClearAll(string wh_no, string pick_date, string apply_kind, string userid, string ip)
        {
            string sql = string.Format(@"update BC_WHPICKDOC a 
                                            set a.lot_no='0', a.update_time = sysdate, a.update_user = :userid, a.update_ip = :ip
                                          where a.wh_no=:wh_no 
                                            and TRUNC(a.pick_date, 'DD') = TO_DATE(:pick_date,'YYYY-MM-DD')
                                            and a.lot_no is not null
                                            and exists (select 1 from ME_DOCM
                                                         where docno = a.docno
                                                            and mat_class in ( {0} )
                                                        )
                                             {1} ",
                               apply_kind == "3" ? " '07','08' " : " '02','03','04','05','06' ",
                               apply_kind == "3" ? string.Empty : "  and apply_kind = :apply_kind");

            return DBWork.Connection.Execute(sql, new
            {
                WH_NO = wh_no,
                PICK_DATE = DateTime.Parse(pick_date).ToString("yyyy-MM-dd"),
                APPLY_KIND = apply_kind,
                USERID = userid,
                IP = ip
            }, DBWork.Transaction);
        }
        #endregion

        #region combo
        public class WhnoComboItem {
            public string WH_NO { get; set; }
            public string WH_NAME { get; set; }
            public string WH_KIND { get; set; }
            public string WH_USERID { get; set; }
        }
        public IEnumerable<WhnoComboItem> GetWhnoCombo(string userId) {
            string sql = @"SELECT b.WH_NO as WH_NO,
                                  (b.WH_NO || ' ' || b.WH_NAME) as WH_NAME,
                                  b.WH_KIND as WH_KIND,
                                  a.WH_USERID as WH_USERID
                             FROM MI_WHID a, MI_WHMAST b
                            WHERE a.WH_USERID = :USERID
                              AND b.WH_NO = a.WH_NO
                              ";
            // AND b.WH_GRADE = '1'
            return DBWork.Connection.Query<WhnoComboItem>(sql, new {USERID = userId }, DBWork.Transaction);
        }

        public class DutyUser {
            public string WH_NO { get; set; }
            public string WH_USERID { get; set; }
            public string WH_USERNAME { get; set; }
            public string IS_DUTY { get; set; }
        }
        public IEnumerable<DutyUser> GetDutyUserCombo(string wh_no) {
            string sql = @"select wh_no,wh_userid,
                                  (select una from ur_id where tuser = a.wh_userid) as  wh_username,
                                  is_duty
                             from BC_WHID a 
                            where wh_no=:wh_no 
                            order by wh_username";
            return DBWork.Connection.Query<DutyUser>(sql, new { WH_NO = wh_no }, DBWork.Transaction);
        }
        #endregion
    }
}