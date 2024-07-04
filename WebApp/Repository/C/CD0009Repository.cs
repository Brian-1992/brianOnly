using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Repository.C
{
    public class CD0009Repository : JCLib.Mvc.BaseRepository
    {
        public CD0009Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<BC_WHPICKDOC> GetMasterAll(string wh_no, string pick_date, string isDistributed, int page_index, int page_size, string sorters)
        {

            var p = new DynamicParameters();

            string sql = @"SELECT a.WH_NO as WH_NO,
                                  TO_CHAR(a.PICK_DATE, 'YYYY-MM-DD') as PICK_DATE,
                                  a.DOCNO as DOCNO,
                                  (SELECT appid from ME_DOCM where docno=a.docno) as APPID,
                                  (SELECT (APPDEPT || ' ' || INID_NAME(APPDEPT) )from ME_DOCM where docno=a.docno) as APPDEPT_NAME,
                                  (SELECT count(*) from BC_WHPICK 
                                    WHERE wh_no=:p0 
                                      AND TRUNC(pick_date, 'DD') = TO_DATE(:p1,'YYYY-MM-DD') 
                                      AND docno=a.docno) as APPITEM_SUM,
                                  (SELECT sum(appqty) from BC_WHPICK 
                                    WHERE wh_no=:p0 
                                      AND TRUNC(pick_date, 'DD') = TO_DATE(:p1,'YYYY-MM-DD') 
                                      AND docno=a.docno) as APPQTY_SUM,
                                  (select apply_note from ME_DOCM 
                                    where docno=a.docno) as apply_note";
            if (isDistributed == "<>")
            {
                sql += "                 ,a.LOT_NO as LOT_NO ";
            }
            sql += @"                FROM BC_WHPICKDOC a
                                    WHERE a.WH_NO = :p0
                                      AND TRUNC(a.PICK_DATE, 'DD') = TO_DATE(:p1,'YYYY-MM-DD')";
            sql += string.Format("    AND a.LOT_NO {0} 0", isDistributed);

            p.Add(":p0", string.Format("{0}", wh_no));
            p.Add(":p1", string.Format("{0}", DateTime.Parse(pick_date).ToString("yyyy-MM-dd")));
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<BC_WHPICKDOC>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<BC_WHPICK> GetDetailAll(string wh_no, string pick_date, string docno)
        {

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



        #region 查詢新申請單資料

        public int DeleteBcWhpick(string wh_no)
        {
            string sql = @" DELETE from BC_WHPICK
                             WHERE wh_no= :wh_no
                               AND pick_date > sysdate-5 
                               AND pick_date < sysdate
                               AND docno in (select docno from BC_WHPICKDOC
                                              where wh_no=:wh_no
                                                and pick_date > sysdate-5 
                                                and pick_date < sysdate 
                                                and lot_no = 0 )";
            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no }, DBWork.Transaction);
        }

        public int DeleteBcWhpickdoc(string wh_no)
        {
            string sql = @" DELETE from BC_WHPICKDOC
                             WHERE wh_no= :wh_no
                               AND pick_date > sysdate-5 
                               AND pick_date < sysdate
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
                                  (select sum(appqty) from ME_DOCD where docno=a.docno) as APPQTY_SUM
                             FROM ME_DOCM a
                            WHERE a.APPTIME > (SYSDATE -5)
                              AND a.FRWH = :wh_no
                              AND a.DOCTYPE in ('MR','MS', 'MR1','MR2', 'MR3', 'MR4') 
                              AND a.FLOWID in ('0102', '0602', '3')
                              AND NOT EXISTS (select 'x' from BC_WHPICKDOC where wh_no=a.frwh and docno=a.docno)";

            p.Add("wh_no", string.Format("{0}", wh_no));
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);


            return DBWork.Connection.Query<ME_DOCM>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCD> GetMeDocds(string docno)
        {

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

        public int InsertBcwhpick(string wh_no, string pick_date, string docno, string userId, string ip, string apply_date)
        {
            string sql = @"INSERT into BC_WHPICK (wh_no, pick_date, docno, seq, mmcode, appqty, base_unit, aplyitem_note, mat_class, mmname_c, mmname_e,wexp_id, store_loc, 
                                                  create_date, create_user, update_time, update_user, update_ip)
                           SELECT :wh_no as wh_no, 
                                  TO_DATE(:pick_date, 'YYYY-MM-DD') as pick_date,
                                  a.docno, b.seq, b.mmcode, b.appqty as appqty,
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
            if (docno != string.Empty)
            {
                sql += "  AND a.docno = :docno ";
            }
            sql += @"
                        AND trunc(a.apptime, 'dd') = TO_DATE(:apply_date, 'YYYY-MM-DD') and a.frwh = :wh_no
                        AND a.doctype in ('MR','MS', 'MR1','MR2', 'MR3', 'MR4') and a.flowid in ('0102', '0602', '3')
                        AND not exists (select 'x' from BC_WHPICKDOC where wh_no=a.frwh and docno=a.docno)
                      ORDER BY docno
                ";

            return DBWork.Connection.Execute(sql, new
            {
                WH_NO = wh_no,
                PICK_DATE = DateTime.Parse(pick_date).ToString("yyyy-MM-dd"),
                APPLY_DATE = DateTime.Parse(apply_date).ToString("yyyy-MM-dd"),
                DOCNO = docno,
                USERID = userId,
                IP = ip
            }, DBWork.Transaction);

        }

        public int InsertBcwhpickdoc(string wh_no, string pick_date, string docno, string userId, string ip, string apply_date)
        {
            string sql = @" INSERT into BC_WHPICKDOC (wh_no,pick_date,docno,apply_kind,complexity,lot_no, CREATE_DATE, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)
                            SELECT :wh_no as WH_NO,
                                   TO_DATE(:pick_date, 'YYYY-MM-DD') as PICK_DATE,
                                   DOCNO,
                                   APPLY_KIND,
                                   1, 0, SYSDATE, :userid, SYSDATE, :userid, :ip
                              FROM ME_DOCM a
                             WHERE trunc(a.apptime, 'dd')=TO_DATE(:apply_date, 'YYYY-MM-DD') and frwh= :wh_no";

            if (docno != string.Empty)
            {
                sql += "  AND a.DOCNO = :docno";
            }

            sql += @"     AND doctype in ('MR','MS', 'MR1','MR2', 'MR3','MR4') and flowid in ('0102','0602', '3')
                          AND NOT EXISTS (select 'x' from BC_WHPICKDOC where wh_no=a.frwh and docno=a.docno)
                        ORDER BY DOCNO";

            return DBWork.Connection.Execute(sql, new
            {
                WH_NO = wh_no,
                PICK_DATE = DateTime.Parse(pick_date).ToString("yyyy-MM-dd"),
                APPLY_DATE = DateTime.Parse(apply_date).ToString("yyyy-MM-dd"),
                DOCNO = docno,
                USERID = userId,
                IP = ip
            }, DBWork.Transaction);
        }

        #endregion

        #region 分配揀貨員

        // 取得臨時單最大批號
        public int GetTempMaxLotNo(string wh_no, string pick_date)
        {
            string sql = @"select NVL2(MAX(lot_no), MAX(lot_no), 0) as max_lotno from BC_WHPICKLOT
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


        public int InsertBcwhpicklot(string wh_no, string pick_date, int lot_no, string pick_userid, string userId, string ip)
        {
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
        // 常態分配揀貨單
        public int DeleteTemplotdocseq(string wh_no)
        {
            string sql = @"delete from BC_WHPICK_TEMP_LOTDOCSEQ
                            where wh_no=:wh_no and calc_time<sysdate";
            return DBWork.Connection.Execute(sql, new
            {
                WH_NO = wh_no
            }, DBWork.Transaction);
        }
        public int GetMaxLotNo(string wh_no, string pick_date)
        {
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


        public int GetWhpickCount(string wh_no, string pick_date, string docnos)
        {
            string sql = string.Format(@"select COUNT(*)
                             from BC_WHPICK
                            where WH_NO = :wh_no
                              and TRUNC(pick_date, 'DD') = TO_DATE(:pick_date, 'YYYY-MM-DD')
                              and docno in ( {0} )", docnos);
            return DBWork.Connection.QueryFirst<int>(sql,
                                                         new
                                                         {
                                                             WH_NO = wh_no,
                                                             PICK_DATE = DateTime.Parse(pick_date).ToString("yyyy-MM-dd"),
                                                             DOCNOS = docnos
                                                         }, DBWork.Transaction); ;
        }

        public IEnumerable<BC_WHPICK> GetWhpicks(string wh_no, string pick_date, string docnos, string sorter, string calc_time, int next_lotno)
        {
            string sql = string.Format(@"select wh_no, 
                                  :calc_time as CALC_TIME,
                                  :next_lotno as LOT_NO,
                                  docno,seq, mmcode,appqty,mmname_c,mmname_e,base_unit,store_loc
                             from BC_WHPICK a
                            where wh_no = :wh_no
                              and TRUNC(pick_date, 'DD') = TO_DATE(:pick_date, 'YYYY-MM-DD')
                              and docno in ( {1} )
                             order by {0}", sorter, docnos);
            return DBWork.Connection.Query<BC_WHPICK>(sql, new
            {
                WH_NO = wh_no,
                CALC_TIME = calc_time,
                NEXT_LOTNO = next_lotno,
                pick_date = DateTime.Parse(pick_date).ToString("yyyy-MM-dd"),
                DOCNOS = docnos,
            }, DBWork.Transaction);
        }
        public int InsertTemplotdocseq(BC_WHPICK_TEMP_LOTDOCSEQ tempSeq)
        {
            string sql = @"insert into BC_WHPICK_TEMP_LOTDOCSEQ
                                       (wh_no, calc_time, lot_no, docno, seq, mmcode, appqty, base_unit,
                                        mmname_c, mmname_e, store_loc, pick_seq, group_no, 
                                        create_date, create_user, update_time, update_user, update_ip)
                           values (:wh_no, TO_DATE(:calc_time, 'YYYY-MM-DD HH24:MI:SS'), :lot_no, :docno, :seq, :mmcode, :appqty, :base_unit,
                                   :mmname_c, :mmname_e, :store_loc, :pick_seq, :group_no,
                                   SYSDATE, :create_user, SYSDATE, :update_user, :update_ip)";
            return DBWork.Connection.Execute(sql, tempSeq, DBWork.Transaction);
        }

        public class DistriMaster
        {
            public string WH_NO { get; set; }
            public string CALC_TIME { get; set; }
            public string GROUP_NO { get; set; }
            public string LOT_NO { get; set; }
            public string APPITEM_SUM { get; set; }
            public string APPQTY_SUM { get; set; }
            public IEnumerable<DistriDetail> Details { get; set; }
        }
        public class DistriDetail
        {
            public string DOCNO { get; set; }
            public string SEQ { get; set; }
            public string PICK_SEQ { get; set; }
        }
        public IEnumerable<DistriMaster> GetTemplotdocseqMasterItems(string wh_no, string calc_time, string lot_no)
        {
            string sql = @"select group_no, 
                                  TO_CHAR(calc_time, 'YYYY-MM-DD HH24:MI') as CALC_TIME,
                                  wh_no, lot_no,
                                  count(*) as APPITEM_SUM,
                                  sum(appqty) as appqty_sum
                             from BC_WHPICK_TEMP_LOTDOCSEQ a
                            where wh_no = :wh_no
                              and TRUNC(calc_time, 'MI')=TO_DATE(:calc_time, 'YYYY-MM-DD HH24:MI')
                              and lot_no = :lot_no
                            group by group_no, calc_time, wh_no, lot_no";
            return DBWork.Connection.Query<DistriMaster>(sql, new
            {
                WH_NO = wh_no,
                CALC_TIME = DateTime.Parse(calc_time).ToString("yyyy-MM-dd HH:mm"),
                LOT_NO = lot_no
            }, DBWork.Transaction);
        }
        public IEnumerable<DistriDetail> GetTemplotdocseqDetailItems(string wh_no, string calc_time, string lot_no, string group_no)
        {
            string sql = @"select DOCNO, SEQ, PICK_SEQ from BC_WHPICK_TEMP_LOTDOCSEQ
                            where WH_NO = :wh_no and LOT_NO = :lot_no
                              and TRUNC(calc_time, 'MI')=TO_DATE(:calc_time, 'YYYY-MM-DD HH24:MI')
                              and GROUP_NO = :group_no";
            return DBWork.Connection.Query<DistriDetail>(sql, new
            {
                WH_NO = wh_no,
                CALC_TIME = DateTime.Parse(calc_time).ToString("yyyy-MM-dd HH:mm"),
                LOT_NO = lot_no,
                GROUP_NO = group_no
            }, DBWork.Transaction);
        }

        public IEnumerable<BC_WHPICK_TEMP_LOTDOCSEQ> GetTemplotdocseqDetail(string wh_no, string calc_time, string group_no)
        {
            string sql = @"select docno,seq,mmcode,mmname_c,mmname_e,appqty,base_unit,store_loc
                             from BC_WHPICK_TEMP_LOTDOCSEQ a
                            where wh_no = :wh_no
                              and TRUNC(calc_time, 'MI')=TO_DATE(:calc_time, 'YYYY-MM-DD HH24:MI')
                              and group_no = :group_no
                            order by pick_seq";
            return DBWork.Connection.Query<BC_WHPICK_TEMP_LOTDOCSEQ>(sql, new
            {
                WH_NO = wh_no,
                CALC_TIME = DateTime.Parse(calc_time).ToString("yyyy-MM-dd HH:mm"),
                GROUP_NO = group_no
            }, DBWork.Transaction);
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
                              AND DOCNO in  (select docno from BC_WHPICK_TEMP_LOTDOCSEQ 
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
        public int UpdateBcwhpickReg(BC_WHPICK_TEMP_LOTDOCSEQ tempSeq)
        {
            string sql = @"UPDATE BC_WHPICK
                              SET pick_userid = :pick_userid, pick_seq = :pick_seq, UPDATE_TIME = SYSDATE, UPDATE_USER = :userid, update_ip = :ip
                            WHERE WH_NO = :wh_no
                              and TRUNC(pick_date, 'DD')=TO_DATE(:pick_date, 'YYYY-MM-DD')
                              AND docno = :docno
                              and seq = :seq";

            return DBWork.Connection.Execute(sql, new
            {
                WH_NO = tempSeq.WH_NO,
                PICK_DATE = DateTime.Parse(tempSeq.PICK_DATE).ToString("yyyy-MM-dd"),
                DOCNO = tempSeq.DOCNO,
                PICK_SEQ = tempSeq.PICK_SEQ,
                SEQ = tempSeq.SEQ,
                PICK_USERID = tempSeq.PICK_USERID,
                USERID = tempSeq.UPDATE_USER,
                IP = tempSeq.UPDATE_IP
            }, DBWork.Transaction);
        }

        #endregion

        #region 已排揀貨批次

        public IEnumerable<BC_WHPICKLOT> GetBcwhpicklots(string wh_no, string pick_date, int page_index, int page_size, string sorters)
        {

            var p = new DynamicParameters();

            string sql = @"select wh_no, lot_no,
                                  TO_CHAR(pick_date, 'YYYY-MM-DD') as pick_date,
                                  (select count(*) from BC_WHPICK 
                                    where wh_no= :wh_no 
                                      and TRUNC(pick_date, 'DD') = TO_DATE(:pick_date, 'YYYY-MM-DD') 
                                      and docno in (select docno from BC_WHPICKDOC 
                                                     where wh_no=:wh_no 
                                                       and TRUNC(pick_date, 'DD') = TO_DATE(:pick_date, 'YYYY-MM-DD')
                                                       and lot_no=a.lot_no)) as appitem_sum,
                                  (select sum(appqty) from BC_WHPICK 
                                    where wh_no=:wh_no 
                                      and TRUNC(pick_date, 'DD') = TO_DATE(:pick_date, 'YYYY-MM-DD')
                                      and docno in (select docno from BC_WHPICKDOC 
                                                     where wh_no=:wh_no 
                                                       and TRUNC(pick_date, 'DD') = TO_DATE(:pick_date, 'YYYY-MM-DD')
                                                       and lot_no=a.lot_no)) as appqty_sum
                             from BC_WHPICKLOT a
                            where wh_no=:wh_no 
                              and TRUNC(pick_date, 'DD') = TO_DATE(:pick_date, 'YYYY-MM-DD')
                            order by lot_no";

            p.Add(":wh_no", wh_no);
            p.Add(":pick_date", DateTime.Parse(pick_date).ToString("yyyy-MM-dd"));
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<BC_WHPICKLOT>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<BC_WHPICK> GetPicksByLotno(string wh_no, string pick_date, int lot_no, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = @"select docno,seq,mmcode,mmname_c,mmname_e,appqty,base_unit,store_loc,
                                  (select una from UR_ID where tuser=a.pick_userid) as pick_username,
                                  (select aplyitem_note from ME_DOCD where docno=a.docno and seq=a.seq) as aplyitem_note
                             from BC_WHPICK a
                            where wh_no=:wh_no 
                              and TRUNC(pick_date, 'DD') = TO_DATE(:pick_date, 'YYYY-MM-DD')
                              and (select lot_no from BC_WHPICKDOC 
                                    where wh_no=:wh_no 
                                       and TRUNC(pick_date, 'DD') = TO_DATE(:pick_date, 'YYYY-MM-DD')
                                      and docno=a.docno)=:lot_no
                            order by pick_seq";

            p.Add("WH_NO", wh_no);
            p.Add("PICK_DATE", DateTime.Parse(pick_date).ToString("yyyy-MM-dd"));
            p.Add("LOT_NO", lot_no);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<BC_WHPICK>(GetPagingStatement(sql, ""), p, DBWork.Transaction);
        }

        public IEnumerable<CD0009> Print(string wh_no, string pick_date, int lot_no)
        {

            var p = new DynamicParameters();

            string sql = string.Format(@" select wh_no,(select wh_name from MI_WHMAST where wh_no=a.wh_no) as wh_name,
                                                 docno,
                                                 (select appdept from ME_DOCM where docno=a.docno)
                                                    ||'_'||
                                                 (select inid_name from UR_INID where inid=(select appdept from ME_DOCM where docno=a.docno)) as appdeptname,
                                                 twn_time((select apptime from ME_DOCM where docno=a.docno)) as apptime,
                                                 mmcode, mmname_c, mmname_e, base_unit,
                                                 (select safe_qty from MI_WINVCTL where wh_no=a.wh_no and mmcode=a.mmcode) as safe_qty,
                                                 (select oper_qty from MI_WINVCTL where wh_no=a.wh_no and mmcode=a.mmcode) as oper_qty,
                                                 NVL((select inv_qty from MI_WHINV where wh_no=a.wh_no and mmcode=a.mmcode),0) as inv_qty,
                                                 NVL((select inv_qty from MI_WHINV where wh_no=(select whno_1x('0') as wh_no from dual) and mmcode='004'||substr(a.mmcode,4,10)),0) as m_inv_qty,
                                                 NVL((select inv_qty from MI_WHINV where wh_no=a.wh_no and mmcode=a.mmcode),0)+NVL((select inv_qty from MI_WHINV where wh_no=(select whno_1x('0') as wh_no from dual) and mmcode='004'||substr(a.mmcode,4,10)),0) as all_inv_qty,
                                                 appqty, store_loc,
                                                 (select una from UR_ID where tuser=a.pick_userid) as pick_username,
                                                 (select aplyitem_note from ME_DOCD where docno=a.docno and seq=a.seq) as aplyitem_note
                                            from BC_WHPICK a
                                           where wh_no=:wh_no 
                                             and TRUNC(pick_date, 'DD') = TO_DATE(:pick_date, 'YYYY-MM-DD')
                                             and (select lot_no from BC_WHPICKDOC 
                                                   where wh_no=:wh_no
                                                     and TRUNC(pick_date, 'DD') = TO_DATE(:pick_date, 'YYYY-MM-DD')
                                                     and docno=a.docno)=:lot_no
                                           order by docno, pick_seq");
            p.Add("wh_no", wh_no);
            p.Add("pick_date", DateTime.Parse(pick_date).ToString("yyyy-MM-dd"));
            p.Add("lot_no", lot_no);

            return DBWork.Connection.Query<CD0009>(sql, p, DBWork.Transaction);
        }


        public int UpdateBcwhpickPickuser(string wh_no, string pick_date, string lot_no, string userid, string ip)
        {

            string sql = @"update BC_WHPICK a set pick_userid='', pick_seq='', update_time = sysdate, update_user = :userid, update_ip = :ip
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
        public int UpdateBcwhpickForClearAll(string wh_no, string pick_date, string userid, string ip)
        {

            string sql = @"update BC_WHPICK a set pick_userid='', pick_seq='',update_time = sysdate, update_user = :userid, update_ip = :ip
                            where wh_no=:wh_no 
                              and TRUNC(pick_date, 'DD') = TO_DATE(:pick_date,'YYYY-MM-DD')
                              and docno in (select docno from BC_WHPICKDOC 
                                             where wh_no=:wh_no 
                                               AND TRUNC(pick_date, 'DD') = TO_DATE(:pick_date,'YYYY-MM-DD')
                                               and docno=a.docno
                                               and lot_no is not null)";

            return DBWork.Connection.Execute(sql, new
            {
                WH_NO = wh_no,
                PICK_DATE = DateTime.Parse(pick_date).ToString("yyyy-MM-dd"),
                USERID = userid,
                IP = ip
            }, DBWork.Transaction);
        }
        public int DeleteBcwhpicklotForClearAll(string wh_no, string pick_date)
        {
            string sql = @"delete from BC_WHPICKLOT a
                            where wh_no=:wh_no 
                              and TRUNC(pick_date, 'DD') = TO_DATE(:pick_date,'YYYY-MM-DD')
                              and exists (select 'x' from BC_WHPICKDOC 
                                           where wh_no=:wh_no 
                                             and TRUNC(pick_date, 'DD') = TO_DATE(:pick_date,'YYYY-MM-DD')
                                             and lot_no=a.lot_no)";
            return DBWork.Connection.Execute(sql, new
            {
                WH_NO = wh_no,
                PICK_DATE = DateTime.Parse(pick_date).ToString("yyyy-MM-dd")
            }, DBWork.Transaction);
        }
        public int UpdateBcwhpickdocForClearAll(string wh_no, string pick_date, string userid, string ip)
        {
            string sql = @"update BC_WHPICKDOC set lot_no='0', update_time = sysdate, update_user = :userid, update_ip = :ip
                            where wh_no=:wh_no 
                              and TRUNC(pick_date, 'DD') = TO_DATE(:pick_date,'YYYY-MM-DD')
                              and lot_no is not null";

            return DBWork.Connection.Execute(sql, new
            {
                WH_NO = wh_no,
                PICK_DATE = DateTime.Parse(pick_date).ToString("yyyy-MM-dd"),
                USERID = userid,
                IP = ip
            }, DBWork.Transaction);
        }
        #endregion

        public IEnumerable<CD0009> PrintNoUser(string wh_no, string pick_date, string[] docnos)
        {

            var p = new DynamicParameters();

            string sql = string.Format(@" select wh_no,
                                                 (select wh_name from MI_WHMAST where wh_no=a.wh_no) as wh_name,
                                                 docno,
                                                 (select appdept from ME_DOCM where docno=a.docno) ||'_'|| 
                                                    (select inid_name from UR_INID where inid=(select appdept from ME_DOCM where docno=a.docno)) as appdeptname,
                                                 mmcode, mmname_e, base_unit, 
                                                (select safe_qty from MI_WINVCTL where wh_no=a.wh_no and mmcode=a.mmcode) as safe_qty,
                                                (select oper_qty from MI_WINVCTL where wh_no=a.wh_no and mmcode=a.mmcode) as oper_qty,
                                                (select TWN_TIME(apptime) from ME_DOCM where docno=a.docno) as apptime,
                                                NVL((select inv_qty from MI_WHINV where wh_no=a.wh_no and mmcode=a.mmcode),0) as inv_qty,
                                                NVL((select inv_qty from MI_WHINV where wh_no=(select whno_1x('0') as wh_no from dual) and mmcode='004'||substr(a.mmcode,4,10)),0) as m_inv_qty,
                                                NVL((select inv_qty from MI_WHINV where wh_no=a.wh_no and mmcode=a.mmcode),0)+NVL((select inv_qty from MI_WHINV where wh_no=(select whno_1x('0') as wh_no from dual) and mmcode='004'||substr(a.mmcode,4,10)),0) as all_inv_qty,
                                                appqty, act_pick_qty, store_loc, 
                                                (select aplyitem_note from ME_DOCD where docno=a.docno and seq=a.seq) as aplyitem_note
                                            from BC_WHPICK a
                                           where wh_no=:wh_no 
                                             and TRUNC(pick_date, 'DD') = TO_DATE(:pick_date, 'YYYY-MM-DD')
                                             and docno in :docnos
                                           order by substr(store_loc,3,18)");
            p.Add("wh_no", wh_no);
            p.Add("pick_date", DateTime.Parse(pick_date).ToString("yyyy-MM-dd"));
            p.Add("docnos", docnos);

            return DBWork.Connection.Query<CD0009>(sql, p, DBWork.Transaction);
        }

    }
}