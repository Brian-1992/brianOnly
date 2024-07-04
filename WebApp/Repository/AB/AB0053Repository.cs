using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.AB
{
    public class AB0053Repository : JCLib.Mvc.BaseRepository
    {
        public AB0053Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AB0053_1> GetAll_1(string WH_NO, string REPLY_MONTH, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT ME_EXPD.MMCODE,
                               MI_MAST.MMNAME_C,
                               MI_MAST.MMNAME_E,
                               (SELECT STORE_LOC
                                FROM MI_WLOCINV MWV
                                WHERE MWV.WH_NO = ME_EXPD.WH_NO
                                AND MWV.MMCODE = ME_EXPD.MMCODE and rownum = 1) STORE_LOC,
                               ME_EXPD.LOT_NO,
                               TO_CHAR(ME_EXPD.REPLY_DATE,'YYYY/MM/DD') REPLY_DATE,
                               ME_EXPD.EXP_QTY EXP_QTY,
                               ME_EXPD.MEMO,
                               TO_CHAR(ME_EXPD.REPLY_TIME,'YYYY/MM/DD') REPLY_TIME,
                               user_name(ME_EXPD.REPLY_ID) as reply_id,
                               TO_CHAR(ME_EXPD.CLOSE_TIME,'YYYY/MM/DD') CLOSE_TIME,
                               user_name(ME_EXPD.CLOSE_ID) as close_id,
                               TO_CHAR(ME_EXPD.EXP_DATE,'YYYYMM') - 191100 EXP_DATE,
                               ME_EXPD.WH_NO,
                               (SELECT MWT.WH_NAME
                                FROM MI_WHMAST MWT
                                WHERE MWT.WH_NO = ME_EXPD.WH_NO and rownum = 1) WH_NAME,
                               ME_EXPD.EXP_STAT,
                               CASE
                                   WHEN ME_EXPD.EXP_STAT = '1' THEN '回報中'
                                   WHEN ME_EXPD.EXP_STAT = '2' THEN '已回報'
                               END EXP_STAT_NAME
                        FROM (ME_EXPD LEFT OUTER JOIN MI_MAST ON ME_EXPD.MMCODE = MI_MAST.MMCODE)
                        WHERE ME_EXPD.EXP_STAT = '2' and ME_EXPD.exp_qty>0
                        AND EXISTS (SELECT WH_NO
                                    FROM MI_WHMAST MWT
                                    WHERE  MWT.WH_GRADE IN ('1', '2')
                                    AND    MWT.WH_KIND = '0'
                                    AND ME_EXPD.WH_NO = MWT.WH_NO)";

            if (WH_NO != "")
            {
                sql += " AND ME_EXPD.WH_NO = :p0 ";
                p.Add(":p0", string.Format("{0}", WH_NO));
            }
            if (REPLY_MONTH != "")
            {
                sql += " AND TO_CHAR(ME_EXPD.EXP_DATE,'YYYYMM') - 191100 = :p1";
                p.Add(":p1", string.Format("{0}", REPLY_MONTH));
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AB0053_1>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<AB0053_2> GetAll_2(string REPLY_MONTH, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT ME_EXPD.MMCODE,
                               MI_MAST.MMNAME_C,
                               MI_MAST.MMNAME_E,
                               TO_CHAR (ME_EXPD.EXP_DATE, 'YYYYMM') - 191100 EXP_DATE,
                               ME_EXPD.LOT_NO,
                               TO_CHAR (ME_EXPD.REPLY_DATE, 'YYYY/MM/DD') REPLY_DATE,
                               ROUND (MAX (CASE WHEN ME_EXPD.WH_NO = 'PH1S' THEN ME_EXPD.EXP_QTY ELSE 0 END), 2) PH1S,
                               ROUND (MAX (CASE WHEN ME_EXPD.WH_NO = 'CHEMO' THEN ME_EXPD.EXP_QTY ELSE 0 END), 2) CHEMO,
                               ROUND (MAX (CASE WHEN ME_EXPD.WH_NO = 'CHEMOT' THEN ME_EXPD.EXP_QTY ELSE 0 END), 2) CHEMOT,
                               ROUND (MAX (CASE WHEN ME_EXPD.WH_NO = 'PH1A' THEN ME_EXPD.EXP_QTY ELSE 0 END), 2) PH1A,
                               ROUND (MAX (CASE WHEN ME_EXPD.WH_NO = 'PH1C' THEN ME_EXPD.EXP_QTY ELSE 0 END), 2) PH1C,
                               ROUND (MAX (CASE WHEN ME_EXPD.WH_NO = 'PH1R' THEN ME_EXPD.EXP_QTY ELSE 0 END), 2) PH1R,
                               ROUND (MAX (CASE WHEN ME_EXPD.WH_NO = 'PHMC' THEN ME_EXPD.EXP_QTY ELSE 0 END), 2) PHMC,
                               ROUND (MAX (CASE WHEN ME_EXPD.WH_NO = 'TPN' THEN ME_EXPD.EXP_QTY ELSE 0 END), 2) TPN,
                               MI_MAST.BASE_UNIT,
                               MI_MAST.E_MANUFACT,
                               NVL ((SELECT EXP_QTY
                                     FROM ME_EXPM MEM
                                     WHERE     MEM.MMCODE = ME_EXPD.MMCODE
                                     AND TO_CHAR (MEM.EXP_DATE, 'YYYYMM') = TO_CHAR (ME_EXPD.EXP_DATE, 'YYYYMM')
                                     AND MEM.LOT_NO = ME_EXPD.LOT_NO and rownum = 1) , 0 ) EXP_QTY
                        FROM ME_EXPD LEFT OUTER JOIN MI_MAST ON ME_EXPD.MMCODE = MI_MAST.MMCODE
                        WHERE     ME_EXPD.EXP_STAT = '2' and ME_EXPD.exp_qty>0
                        AND EXISTS
                                  (SELECT WH_NO
                                   FROM MI_WHMAST MWT
                                   WHERE     MWT.WH_GRADE IN ('1', '2')
                                   AND MWT.WH_KIND = '0'
                                   AND ME_EXPD.WH_NO = MWT.WH_NO)";

            if (REPLY_MONTH != "")
            {
                sql += " AND TO_CHAR(ME_EXPD.EXP_DATE,'YYYYMM') - 191100 = :p2 ";
                p.Add(":p2", string.Format("{0}", REPLY_MONTH));
            }
            sql += @"GROUP BY ME_EXPD.MMCODE,
                              MI_MAST.MMNAME_C,
                              MI_MAST.MMNAME_E,
                              TO_CHAR (ME_EXPD.EXP_DATE, 'YYYYMM'),
                              ME_EXPD.LOT_NO,
                              TO_CHAR (ME_EXPD.REPLY_DATE, 'YYYY/MM/DD'),
                              MI_MAST.BASE_UNIT,
                              MI_MAST.E_MANUFACT";
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AB0053_2>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<AB0053_3> GetAll_3(string MMCODE, string EXP_DATE, string closeflag, string mail_status, string agennos, bool showQtyNot0, bool isPreData)
        {
            var p = new DynamicParameters();

            var sql = string.Format(@"
                       SELECT MEM.MMCODE,
                                        MEM.MMCODE MMCODE_DISPLAY,
                                        MMT.MMNAME_C,
                                        MMT.MMNAME_E,
                                        TO_CHAR (MEM.EXP_DATE, 'YYYY/MM/DD') EXP_DATE,
                                        TO_CHAR (MEM.EXP_DATE, 'YYYYMM') - 191100 EXP_DATE_DISPLAY,
                                        MEM.WARNYM,
                                        MEM.WARNYM WARNYM_TEXT,
                                        MEM.LOT_NO,
                                        MEM.LOT_NO LOT_NO_DISPLAY,
                                        MEM.EXP_QTY,
                                        MEM.MEMO,
                                        MEM.CLOSEFLAG,
                                        DECODE ( MEM.CLOSEFLAG , 'Y' , '是' , 'N' , '否' , '' ) CLOSEFLAG_NAME,
                                        DECODE ( MEM.CLOSEFLAG , 'Y' , 'Y 是' , 'N' , 'N 否' , '' ) CLOSEFLAG_TEXT,
                                        MEM.AGEN_NO,
                                        MEM.AGEN_NAMEC,
                                        (MEM.AGEN_NO||' '||MEM.AGEN_NAMEC) as comb_AGEN,
                                        NVL (
                                             (SELECT DECODE (MMK.STATUS,
                                                             'A', '未回覆',
                                                             'B', '未回覆',
                                                             'C', '已回覆')
                                              FROM ME_MAILBACK MMK
                                              WHERE     MMK.MMCODE = MEM.MMCODE
                                              AND MMK.EXP_DATE = MEM.EXP_DATE
                                              AND MMK.LOT_NO = MEM.LOT_NO), '未寄送') MAIL_STATUS,
                                        (case 
                                             when (MEM.IS_AGENNO = 'N' or MEM.IS_AGENNO = 'P')
                                                then (select email from PH_VENDER where agen_no = MEM.agen_no) 
                                             else ' '
                                         end) as EMAIL,
                                         MEM.IS_AGENNO,
                                        (select listagg(wh_no || '-' || exp_qty, ',') within group (order by wh_no)
                                           from ME_EXPD
                                          where mmcode = mem.mmcode
                                            and twn_yyymm(reply_date) = mem.warnym
                                            and lot_no = mem.lot_no
                                            and exp_stat = '2'
                                            and exp_qty > 0
                                         ) as wh_expqty,
                                         MEM.warnym_key,
                                         (select 'Y' from ME_EXPM 
                                           where  TWN_YYYMM(EXP_DATE) = TWN_PYM(:p4)
                                             AND CLOSEFLAG = 'N' and EXP_QTY>0 
                                             and mmcode = MEM.mmcode
                                             and lot_no = MEM.lot_no
                                             and warnym_key = MEM.warnym_key
                                         ) as isPreData
                                  FROM ME_EXPM MEM, MI_MAST MMT
                                 WHERE MEM.MMCODE = MMT.MMCODE 
                                   {0}  
                        "
                    , showQtyNot0 ? " and MEM.EXP_QTY>0" : string.Empty);

            if (MMCODE != "")
            {
                sql += " AND MEM.MMCODE = :p3 ";
                p.Add(":p3", string.Format("{0}", MMCODE));
            }
            if (EXP_DATE != "")
            {
                sql += " AND TO_CHAR(MEM.EXP_DATE,'YYYYMM') - 191100 = :p4 ";
            }
            p.Add(":p4", string.Format("{0}", EXP_DATE));
            if (closeflag != string.Empty)
            {
                sql += string.Format("  and mem.closeflag  = :p5");
                p.Add(":p5", string.Format("{0}", closeflag));
            }
            if (mail_status != string.Empty) {
                sql += GetMailStatusSql(mail_status);
            }
            if (agennos != string.Empty)
            {
                sql += string.Format("  and MMT.M_AGENNO in ( {0} )", agennos);
            }

            sql = string.Format(@"
                    select * 
                      from (
                            {0}
                           )
                     where 1=1
                       {1}"
                  , sql
                  , isPreData ? " and isPreData = 'Y'" : string.Empty);

            return DBWork.PagingQuery<AB0053_3>(sql, p, DBWork.Transaction);
        }
        private string GetMailStatusSql(string mail_status) {
            string result = " and (";

            if (mail_status == "1")
            {
                result += @"  not exists (select 1 
                                            FROM ME_MAILBACK MMK
                                           WHERE MMK.MMCODE = MEM.MMCODE
                                             AND MMK.EXP_DATE = MEM.EXP_DATE
                                             AND MMK.LOT_NO = MEM.LOT_NO)";
            }
            if (mail_status == "2")
            {
                result += @"  exists (select 1 
                                            FROM ME_MAILBACK MMK
                                           WHERE MMK.MMCODE = MEM.MMCODE
                                             AND MMK.EXP_DATE = MEM.EXP_DATE
                                             AND MMK.LOT_NO = MEM.LOT_NO
                                             and mmk.status in ('A', 'B') )";
            }
            if (mail_status == "3")
            {
                result += @"  exists (select 1 
                                            FROM ME_MAILBACK MMK
                                           WHERE MMK.MMCODE = MEM.MMCODE
                                             AND MMK.EXP_DATE = MEM.EXP_DATE
                                             AND MMK.LOT_NO = MEM.LOT_NO
                                             and mmk.status  = 'C' )";
            }

            result += ")";

            return result;
        }

        public int Return_Close(string update_id, string ip) //回報截止
        {
            var sql = @"UPDATE ME_EXPD 
                           SET CLOSE_TIME = SYSDATE, CLOSE_ID = :UPDATE_ID,
                               UPDATE_TIME = SYSDATE, UPDATE_ID = :UPDATE_ID, IP = :IP
                         WHERE EXP_STAT = '2' AND CLOSE_TIME IS NULL";
            return DBWork.Connection.Execute(sql, new { UPDATE_ID = update_id, IP = ip }, DBWork.Transaction);
        }

        public int Create(AB0053_3 AB0053_3) //新增
        {
            var sql = @"INSERT INTO ME_EXPM (MMCODE, EXP_DATE, WARNYM, LOT_NO, MEMO, CLOSEFLAG, EXP_QTY, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, WARNYM_KEY, AGEN_NO, AGEN_NAMEC)  
                                VALUES (:MMCODE, TO_DATE( :EXP_DATE + 191100, 'YYYYMM'), :WARNYM, :LOT_NO, :MEMO, :CLOSEFLAG, :EXP_QTY, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP, :WARNYM, :AGEN_NO, :AGEN_NAMEC)";
            return DBWork.Connection.Execute(sql, AB0053_3, DBWork.Transaction);
        }

        public int Update(AB0053_3 AB0053_3) //修改
        {
            var sql = @"UPDATE ME_EXPM 
                        SET    WARNYM = :WARNYM,
                               MEMO = :MEMO,
                               CLOSEFLAG = :CLOSEFLAG,
                               EXP_QTY = :EXP_QTY,
                               UPDATE_TIME = SYSDATE,
                               UPDATE_USER = :UPDATE_USER,
                               UPDATE_IP = :UPDATE_IP
                        WHERE MMCODE = :MMCODE
                        AND TO_CHAR(EXP_DATE,'YYYYMM') = :EXP_DATE + 191100
                        AND LOT_NO = :LOT_NO
                        and warnym_key = :warnym_key";
            return DBWork.Connection.Execute(sql, AB0053_3, DBWork.Transaction);
        }

        public int Delete(string MMCODE, string EXP_DATE, string LOT_NO, string warnym_key)
        {
            var sql = @"DELETE FROM ME_EXPM
                        WHERE MMCODE = :MMCODE
                        AND TO_CHAR(EXP_DATE,'YYYYMM') = :EXP_DATE + 191100
                        AND LOT_NO = :LOT_NO
                        and warnym_key = :warnym_key";
            return DBWork.Connection.Execute(sql, new { MMCODE = MMCODE, EXP_DATE = EXP_DATE, LOT_NO = LOT_NO, warnym_key }, DBWork.Transaction);
        }

        public bool CheckExistsT3(string MMCODE, string EXP_DATE, string LOT_NO, string warnym)
        {
            string sql = @"SELECT 1
                           FROM ME_EXPM
                           WHERE MMCODE = :MMCODE
                           AND TO_CHAR(EXP_DATE,'YYYYMM') = :EXP_DATE + 191100
                           AND LOT_NO = :LOT_NO
                           and warnym_key = :warnym";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = MMCODE, EXP_DATE = EXP_DATE, LOT_NO = LOT_NO, warnym }, DBWork.Transaction) == null);
        }

        public DataTable GetExcel_T1(string WH_NO, string REPLY_MONTH)
        {
            DynamicParameters p = new DynamicParameters();

            var sql = @"SELECT ME_EXPD.MMCODE 院內碼,
                               MI_MAST.MMNAME_C 中文品名,
                               MI_MAST.MMNAME_E 英文品名,
                               (SELECT STORE_LOC
                                FROM MI_WLOCINV MWV
                                WHERE MWV.WH_NO = ME_EXPD.WH_NO
                                AND MWV.MMCODE = ME_EXPD.MMCODE  and rownum = 1) 儲位碼,
                               ME_EXPD.LOT_NO 藥品批號,
                               TO_CHAR(ME_EXPD.REPLY_DATE,'YYYY/MM/DD') 回覆效期,
                               ME_EXPD.EXP_QTY 效期藥量,
                               ME_EXPD.MEMO 備註,
                               TO_CHAR(ME_EXPD.REPLY_TIME,'YYYY/MM/DD') 回覆日期,
                               ME_EXPD.REPLY_ID 回覆人員,
                               TO_CHAR(ME_EXPD.CLOSE_TIME,'YYYY/MM/DD') 結案日期,
                               ME_EXPD.CLOSE_ID 截止人員,
                               TO_CHAR(ME_EXPD.EXP_DATE,'YYYYMM') - 191100 月份,
                               ME_EXPD.WH_NO 庫別代碼,
                               (SELECT MWT.WH_NAME
                                FROM MI_WHMAST MWT
                                WHERE MWT.WH_NO = ME_EXPD.WH_NO  and rownum = 1) 庫別名稱,
                               CASE 
                                   WHEN ME_EXPD.EXP_STAT = '1' THEN '回報中'
                                   WHEN ME_EXPD.EXP_STAT = '2' THEN '已回報'
                               END 效期回覆狀態
                        FROM (ME_EXPD LEFT OUTER JOIN MI_MAST ON ME_EXPD.MMCODE = MI_MAST.MMCODE)
                        WHERE ME_EXPD.EXP_STAT = '2' and ME_EXPD.EXP_QTY>0
                        AND EXISTS (SELECT WH_NO
                                    FROM MI_WHMAST MWT
                                    WHERE  MWT.WH_GRADE IN ('1', '2')
                                    AND    MWT.WH_KIND = '0'
                                    AND ME_EXPD.WH_NO = MWT.WH_NO)";

            if (WH_NO != "")
            {
                sql += " AND ME_EXPD.WH_NO = :p0 ";
                p.Add(":p0", string.Format("{0}", WH_NO));
            }
            if (REPLY_MONTH != "")
            {
                sql += " AND TO_CHAR(ME_EXPD.EXP_DATE,'YYYYMM') -191100 = :p1";
                p.Add(":p1", string.Format("{0}", REPLY_MONTH));
            }

            sql += @"  ORDER BY ME_EXPD.MMCODE, ME_EXPD.WH_NO, ME_EXPD.LOT_NO";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public DataTable GetExcel_T2(string REPLY_MONTH)
        {
            DynamicParameters p = new DynamicParameters();

            var sql = @"SELECT ME_EXPD.MMCODE 院內碼,
                               MI_MAST.MMNAME_C 中文品名,
                               MI_MAST.MMNAME_E 英文品名,
                               TO_CHAR (ME_EXPD.EXP_DATE, 'YYYYMM') - 191100 月份,
                               ME_EXPD.LOT_NO 藥品批號,
                               TO_CHAR (ME_EXPD.REPLY_DATE, 'YYYY/MM/DD') 回覆效期,
                               ROUND ( MAX ( CASE WHEN ME_EXPD.WH_NO = 'PH1S' THEN ME_EXPD.EXP_QTY ELSE 0 END), 2) ""藥庫(PH1S)"",
                               ROUND ( MAX (  CASE WHEN ME_EXPD.WH_NO = 'CHEMO' THEN ME_EXPD.EXP_QTY ELSE 0 END), 2) ""內湖化療調配室(CHEMO)"",
                               ROUND ( MAX ( CASE WHEN ME_EXPD.WH_NO = 'CHEMOT' THEN ME_EXPD.EXP_QTY ELSE 0 END), 2) ""汀洲化療調配室(CHEMOT)"",
                               ROUND ( MAX ( CASE WHEN ME_EXPD.WH_NO = 'PH1A' THEN ME_EXPD.EXP_QTY ELSE 0 END), 2) ""內湖住院藥局(PH1A)"",
                               ROUND ( MAX ( CASE WHEN ME_EXPD.WH_NO = 'PH1C' THEN ME_EXPD.EXP_QTY ELSE 0 END), 2) ""內湖門診藥局(PH1C)"",
                               ROUND ( MAX ( CASE WHEN ME_EXPD.WH_NO = 'PH1R' THEN ME_EXPD.EXP_QTY ELSE 0 END), 2) ""內湖急診藥局(PH1R)"",
                               ROUND ( MAX ( CASE WHEN ME_EXPD.WH_NO = 'PHMC' THEN ME_EXPD.EXP_QTY ELSE 0 END), 2) ""汀洲藥局(PHMC)"",
                               ROUND ( MAX (CASE WHEN ME_EXPD.WH_NO = 'TPN' THEN ME_EXPD.EXP_QTY ELSE 0 END), 2) ""製劑室(TPN)"",
                               MI_MAST.BASE_UNIT 劑量單位,
                               MI_MAST.E_MANUFACT 廠商,
                               NVL ((SELECT EXP_QTY
                                FROM ME_EXPM MEM
                                WHERE     MEM.MMCODE = ME_EXPD.MMCODE
                                AND TO_CHAR (MEM.EXP_DATE, 'YYYYMM') = TO_CHAR (ME_EXPD.EXP_DATE, 'YYYYMM')
                                AND MEM.LOT_NO = ME_EXPD.LOT_NO  and rownum = 1) , 0 ) 總數
                        FROM ME_EXPD LEFT OUTER JOIN MI_MAST ON ME_EXPD.MMCODE = MI_MAST.MMCODE
                        WHERE     ME_EXPD.EXP_STAT = '2' and ME_EXPD.EXP_QTY>0
                        AND EXISTS ( SELECT WH_NO
                                     FROM MI_WHMAST MWT
                                     WHERE     MWT.WH_GRADE IN ('1', '2')
                                     AND MWT.WH_KIND = '0'
                                     AND ME_EXPD.WH_NO = MWT.WH_NO)";

            if (REPLY_MONTH != "")
            {
                sql += " AND TO_CHAR(ME_EXPD.EXP_DATE,'YYYYMM') -191100 = :p2 ";
                p.Add(":p2", string.Format("{0}", REPLY_MONTH));
            }

            sql += @" GROUP BY ME_EXPD.MMCODE,
                              MI_MAST.MMNAME_C,
                              MI_MAST.MMNAME_E,
                              TO_CHAR (ME_EXPD.EXP_DATE, 'YYYYMM'),
                              ME_EXPD.LOT_NO,
                              TO_CHAR (ME_EXPD.REPLY_DATE, 'YYYY/MM/DD'),
                              MI_MAST.BASE_UNIT,
                              MI_MAST.E_MANUFACT
                     ORDER BY ME_EXPD.MMCODE, ME_EXPD.LOT_NO";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public DataTable GetExcel_T3(string MMCODE, string EXP_DATE, string closeflag, string mail_status, string agennos)
        {
            DynamicParameters p = new DynamicParameters();

            var sql = @"SELECT MEM.MMCODE 院內碼,
                               MMT.MMNAME_C 中文品名, 
                               MMT.MMNAME_E 英文品名,
                               TO_CHAR (MEM.EXP_DATE, 'YYYYMM') - 191100 月份,
                               MEM.WARNYM ""警示效期(年/月)"",
                               MEM.LOT_NO 藥品批號,
                               MEM.EXP_QTY 數量,
                               (select listagg(wh_no || '-' || exp_qty, ',') within group (order by wh_no)
                                 from ME_EXPD
                                where mmcode = mem.mmcode
                                  and twn_yyymm(reply_date) = mem.warnym
                                  and lot_no = mem.lot_no
                                  and exp_stat = '2'
                                  and exp_qty > 0
                               ) as 各單位回報量,
                               MEM.MEMO 備註,
                               DECODE ( MEM.CLOSEFLAG , 'Y' , '是' , 'N' , '否' , '' ) 結案否,
                               AGEN_NO as 廠商代碼,
                               AGEN_NAMEC as 廠商名稱,
                               (select email from PH_VENDER where agen_no = MEM.agen_no) as 廠商EMAIL
                        FROM ME_EXPM MEM, MI_MAST MMT
                        WHERE MEM.MMCODE = MMT.MMCODE and MEM.EXP_QTY>0";

            if (MMCODE != "")
            {
                sql += " AND MEM.MMCODE = :p3 ";
                p.Add(":p3", string.Format("{0}", MMCODE));
            }
            if (EXP_DATE != "")
            {
                sql += " AND TO_CHAR(MEM.EXP_DATE,'YYYYMM') - 191100 = :p4 ";
                p.Add(":p4", string.Format("{0}", EXP_DATE));
            }
            if (closeflag != string.Empty)
            {
                sql += string.Format("  and mem.closeflag  = :p5");
                p.Add(":p5", string.Format("{0}", closeflag));
            }

            if (mail_status != string.Empty)
            {
                sql += GetMailStatusSql(mail_status);
            }
            if (agennos != string.Empty)
            {
                sql += string.Format("  and MMT.M_AGENNO in ( {0} )", agennos);
            }

            sql += @" ORDER BY MEM.AGEN_NO, MEM.MMCODE";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<ComboItemModel> GetWhno()
        {
            string sql = @"  SELECT TRIM (MWT.WH_NO) VALUE,
                                    TRIM (MWT.WH_NO || ' ' || MWT.WH_NAME) TEXT
                             FROM MI_WHMAST MWT
                             WHERE MWT.WH_GRADE IN ('1', '2') AND MWT.WH_KIND = '0'
                             ORDER BY MWT.WH_GRADE, MWT.WH_NO";
            return DBWork.Connection.Query<ComboItemModel>(sql, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE,
                                   A.MMNAME_C,
                                   A.MMNAME_E,
                                   A.MAT_CLASS,
                                   A.BASE_UNIT,
                                   PVR.AGEN_NO || ' ' || PVR.AGEN_NAMEC AGEN_NAMEC
                        FROM MI_MAST A, PH_VENDER PVR
                        WHERE PVR.AGEN_NO = A.M_AGENNO ";


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

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX, MMCODE", sql);
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

        public class MI_MAST_QUERY_PARAMS
        {
            public string MMCODE;
            public string MMNAME_C;
            public string MMNAME_E;
            public string MAT_CLASS;

            public string WH_NO;
            public string IS_INV;  // 需判斷庫存量>0
            public string E_IFPUBLIC;  // 是否公藥
        }

        public IEnumerable<MI_MAST> GetMmcode(MI_MAST_QUERY_PARAMS query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            string sql = @"SELECT  A.MMCODE,
                                   A.MMNAME_C,
                                   A.MMNAME_E,
                                   A.MAT_CLASS,
                                   A.BASE_UNIT,
                                   PVR.AGEN_NO || ' ' || PVR.AGEN_NAMEC AGEN_NAMEC
                        FROM MI_MAST A, PH_VENDER PVR
                        WHERE PVR.AGEN_NO = A.M_AGENNO ";


            if (query.MMCODE != "")
            {
                sql += " AND A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("%{0}%", query.MMCODE));
            }

            if (query.MMNAME_C != "")
            {
                sql += " AND A.MMNAME_C LIKE :MMNAME_C ";
                p.Add(":MMNAME_C", string.Format("%{0}%", query.MMNAME_C));
            }

            if (query.MMNAME_E != "")
            {
                sql += " AND A.MMNAME_E LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", query.MMNAME_E));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //發送MAIL_1
        public int SendMail_1()
        {
            var sql = @"DELETE FROM ME_MAILBACK
                        WHERE     STATUS IN ('A', 'B')
                        AND TO_CHAR (EXP_DATE, 'YYYYMM') = TO_CHAR (SYSDATE, 'YYYYMM')";
            return DBWork.Connection.Execute(sql, DBWork.Transaction);
        }

        //發送MAIL_2
        public int SendMail_2(string UPDATE_IP)
        {
            var sql = @"INSERT INTO ME_MAILBACK (SEQ,
                                                 AGEN_NO,
                                                 MMCODE,
                                                 EXP_DATE,
                                                 LOT_NO,
                                                 EXP_QTY,
                                                 BACK_DT,
                                                 STATUS,
                                                 CREATE_TIME,
                                                 UPDATE_IP,
                                                 MAIL_NO)
                        SELECT ME_MAILBACK_SEQ.NEXTVAL,
                               (SELECT MMT.M_AGENNO
                                FROM MI_MAST MMT
                                WHERE MMT.MMCODE = MEM.MMCODE) AGEN_NO,
                               MEM.MMCODE,
                               MEM.EXP_DATE,
                               MEM.LOT_NO,
                               MEM.EXP_QTY,
                               NULL BACK_DT,
                               'A' STATUS,
                               SYSDATE,
                               :UPDATE_IP,
                               TO_CHAR (SYSDATE, 'YYYYMMDDHH24MISS') - 19110000000000 MAIL_NO
                        FROM ME_EXPM MEM
                        WHERE     TO_CHAR (MEM.EXP_DATE, 'YYYYMM') = TO_CHAR (SYSDATE, 'YYYYMM')
                        AND NOT EXISTS
                                      (SELECT 1
                                       FROM ME_MAILBACK MMK
                                       WHERE     MMK.MMCODE = MEM.MMCODE
                                       AND MMK.EXP_DATE = MEM.EXP_DATE
                                       AND MMK.LOT_NO = MEM.LOT_NO)
                                       AND NOT EXISTS
                                                     (SELECT 1
                                                      FROM MI_MAST MMT
                                                      WHERE MMT.MMCODE = MEM.MMCODE
                                                      AND MMT.M_AGENNO IN ('000','999'))";
            return DBWork.Connection.Execute(sql, new { UPDATE_IP = UPDATE_IP }, DBWork.Transaction);
        }

        //發送MAIL_3
        public int SendMail_3(string UPDATE_USER, string UPDATE_IP)
        {
            var sql = @"UPDATE ME_EXPM MEM
                        SET    MEM.STATUS = 'B',
                               MEM.UPDATE_TIME = SYSDATE,
                               MEM.UPDATE_USER = :UPDATE_USER,
                               MEM.UPDATE_IP = :UPDATE_IP
                        WHERE TO_CHAR (MEM.EXP_DATE, 'YYYYMM') = TO_CHAR (SYSDATE, 'YYYYMM')
                        AND MEM.STATUS = 'A'
                        AND NOT EXISTS
                                      (SELECT 1
                                       FROM MI_MAST MMT
                                       WHERE MMT.MMCODE = MEM.MMCODE
                                       AND MMT.M_AGENNO IN ('000','999'))";
            return DBWork.Connection.Execute(sql, new { UPDATE_USER = UPDATE_USER, UPDATE_IP = UPDATE_IP }, DBWork.Transaction);
        }

        #region SendMail_v2
        public int SendMail_1_v2(AB0053_3 item) {
            var sql = @"delete from ME_MAILBACK
                         where mmcode = :mmcode
                           and to_char(exp_date, 'YYYYMM') = to_char(to_date(:EXP_DATE, 'YYYY/MM/DD'), 'YYYYMM') --to_char(sysdate, 'YYYYMM')
                           and lot_no = :lot_no";
            return DBWork.Connection.Execute(sql, item, DBWork.Transaction);
        }

        public int SendMail_2_v2(AB0053_3 item) {
            var sql = @"insert into ME_MAILBACK (seq, agen_no, mmcode, exp_date, lot_no,
                                                 exp_qty, back_dt, status, create_time, update_ip, mail_no)
                        select ME_MAILBACK_SEQ.nextval,
                               MEM.AGEN_NO,
                               MEM.MMCODE,
                               MEM.EXP_DATE,
                               MEM.LOT_NO,
                               MEM.EXP_QTY,
                               NULL BACK_DT,
                               'A' STATUS,
                               SYSDATE,
                               :UPDATE_IP,
                               TO_CHAR (SYSDATE, 'YYYYMMDDHH24MISS') - 19110000000000 MAIL_NO
                        FROM ME_EXPM MEM
                        WHERE  mem.mmcode = :mmcode and mem.EXP_QTY>0
                        AND TO_CHAR (MEM.EXP_DATE, 'YYYYMM') = to_char(to_date(:EXP_DATE, 'YYYY/MM/DD'), 'YYYYMM') --TO_CHAR (SYSDATE, 'YYYYMM')
                        and mem.lot_no = :lot_no";
            return DBWork.Connection.Execute(sql, item, DBWork.Transaction);
        }

        public int SendMail_3_v2(AB0053_3 item) {
            var sql = @"UPDATE ME_EXPM MEM
                        SET    MEM.STATUS = 'B',
                               MEM.UPDATE_TIME = SYSDATE,
                               MEM.UPDATE_USER = :UPDATE_USER,
                               MEM.UPDATE_IP = :UPDATE_IP
                        WHERE mmcode = :mmcode 
                          and TO_CHAR (MEM.EXP_DATE, 'YYYYMM') = to_char(to_date(:EXP_DATE, 'YYYY/MM/DD'), 'YYYYMM') --TO_CHAR (SYSDATE, 'YYYYMM')
                          and lot_no = :lot_no
                          AND MEM.STATUS = 'A'";
            return DBWork.Connection.Execute(sql, item, DBWork.Transaction);

        }
        #endregion

        //複製資料
        public int CopyData(string CREATE_USER, string UPDATE_USER, string UPDATE_IP)
        {
            var sql = @"INSERT INTO ME_EXPM (MMCODE,
                                             EXP_DATE,
                                             LOT_NO,
                                             WARNYM,
                                             MEMO,
                                             CLOSEFLAG,
                                             CREATE_TIME,
                                             CREATE_USER,
                                             UPDATE_TIME,
                                             UPDATE_USER,
                                             UPDATE_IP,
                                             EXP_QTY,
                                             RDOCNO,
                                             STATUS,
                                             AGEN_NO, 
                                             AGEN_NAMEC,
                                             IS_AGENNO,
                                             WARNYM_KEY)
                        SELECT DISTINCT MEM.MMCODE,
                               TRUNC (SYSDATE, 'MM') EXP_DATE,
                               MEM.LOT_NO,
                               MEM.WARNYM,
                               MEM.MEMO,
                               MEM.CLOSEFLAG,
                               SYSDATE,
                               :CREATE_USER,
                               SYSDATE,
                               :UPDATE_USER,
                               :UPDATE_IP,
                               MEM.EXP_QTY,
                               MEM.RDOCNO,
                               'A',
                               MEM.AGEN_NO,
                               MEM.AGEN_NAMEC,
                               'N',
                               MEM.WARNYM_KEY
                        FROM ME_EXPM MEM
                        WHERE     TWN_YYYMM(MEM.EXP_DATE) = TWN_PYM(TWN_YYYMM(SYSDATE))
                        AND CLOSEFLAG = 'N' and MEM.EXP_QTY>0
                        AND NOT EXISTS
                                      (SELECT *
                                       FROM ME_EXPM MEM2
                                       WHERE     TO_CHAR (MEM2.EXP_DATE, 'YYYYMM') = TO_CHAR (SYSDATE, 'YYYYMM')
                                       AND MEM2.MMCODE = MEM.MMCODE
                                       AND MEM2.LOT_NO = MEM.LOT_NO
                                       and mem2.warnym_key = mem.warnym_key)";
            return DBWork.Connection.Execute(sql, new { CREATE_USER = CREATE_USER, UPDATE_USER = UPDATE_USER, UPDATE_IP = UPDATE_IP }, DBWork.Transaction);
        }

        public IEnumerable<PH_VENDER> GetAgenCombo(string p0, int page_index, int page_size, string sorters) {
            var p = new DynamicParameters();

            var sql = @"select {0} a.agen_no,
                                   a.agen_namec as agen_namec,
                                   a.agen_namee as agen_namee
                         from PH_VENDER a
                        WHERE 1=1";


            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(a.agen_no, :agen_no_i), 1000) + NVL(INSTR(A.agen_namee, :agen_namee_i), 100) * 10 + NVL(INSTR(a.agen_namec, :agen_namec_i), 100) * 10 + NVL(INSTR(a.easyname, :easyname_i), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
                p.Add(":agen_no_i", p0);
                p.Add(":agen_namee_i", p0);
                p.Add(":agen_namec_i", p0);
                p.Add(":easyname_i", p0);

                sql += " AND (a.agen_no LIKE :agen_no_i ";
                p.Add(":agen_no_i", string.Format("%{0}%", p0));

                sql += " OR a.agen_namee LIKE :agen_namee_i ";
                p.Add(":agen_namee_i", string.Format("%{0}%", p0));

                sql += " OR a.agen_namec LIKE :agen_namec_i ";
                p.Add(":agen_namec_i", string.Format("%{0}%", p0));

                sql += " OR a.easyname LIKE :easyname_i) ";
                p.Add(":easyname_i", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX, agen_no", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY A.agen_no ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<PH_VENDER>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        #region 2020-07-09 新增 調整廠商
        public int SetAgenInfo(string update_user, string update_ip) {
            string sql = @"update ME_EXPM a
                              set agen_no = (select M_AGENNO from MI_MAST where mmcode = a.mmcode),
                                  agen_namec = (select agen_namec from PH_VENDER 
                                                 where agen_no = (select M_AGENNO from MI_MAST where mmcode = a.mmcode)),
                                  is_agenno = 'N',
                                  update_user  = :update_user,
                                  update_ip = :update_ip
                            where agen_no is null";

            return DBWork.Connection.Execute(sql, new { update_user = update_user, update_ip = update_ip }, DBWork.Transaction);
        }

        public IEnumerable<PH_VENDER> GetHisVenders(string mmcode) {
            string sql = @"select distinct supplyno as agen_no,
                                  agentname as agen_namec
                             from HIS_BASORDD
                            where ordercode = :mmcode
                            order by supplyno";

            return DBWork.Connection.Query<PH_VENDER>(sql, new { mmcode = mmcode}, DBWork.Transaction);
        }

        public IEnumerable<PH_VENDER> GetAllVenders()
        {
            string sql = @"select agen_no, agen_namec
                             from PH_VENDER
                            order by agen_no";

            return DBWork.Connection.Query<PH_VENDER>(sql, DBWork.Transaction);
        }

        public int UpdateAgenno(string mmcode, string exp_date, string lot_no, string agen_no, string agen_namec, string source, string update_user, string update_ip) {
            string sql = @"update ME_EXPM
                              set agen_no = :agen_no,
                                  agen_namec = :agen_namec,
                                  is_agenno = :source
                            where mmcode = :mmcode
                              and exp_date = to_date(:exp_date, 'YYYY/MM/DD')
                              and lot_no = :lot_no";

            return DBWork.Connection.Execute(sql, new { mmcode = mmcode, exp_date = exp_date, agen_no = agen_no, lot_no = lot_no,
                agen_namec = agen_namec, source = source, update_user = update_user, update_ip = update_ip }, DBWork.Transaction);
        }
        #endregion

        #region 2020-12-04 寄信時檢查是否有廠商email
        public string GetAgenEmail(string mmcode, string lot_no, string exp_date) {
            string sql = @"select nvl(email, ' ') from PH_VENDER 
                            where agen_no = (select agen_no from ME_EXPM
                                              where mmcode = :mmcode
                                                and lot_no = :lot_no
                                                and TO_CHAR (EXP_DATE, 'YYYYMM') = to_char(to_date(:exp_date, 'YYYY/MM/DD'), 'YYYYMM') --TO_CHAR (SYSDATE, 'YYYYMM')
                                            )";
            return DBWork.Connection.QueryFirst<string>(sql, new { mmcode = mmcode, lot_no = lot_no, exp_date  = exp_date }, DBWork.Transaction);
        }
        #endregion

        public AB0053_3 GetAgenInfo(string mmcode) {
            string sql = @"
                select b.agen_no, b.agen_namec
                  from MI_MAST a, PH_VENDER b
                 where a.mmcode = :mmcode
                   and a.m_agenno = b.agen_no
            ";
            return DBWork.Connection.QueryFirstOrDefault<AB0053_3>(sql, new { mmcode }, DBWork.Transaction);
        }

        #region 2022-01-07新增：更新本月qty=0品項為上月qty(若存在)
        public int UpdateQtyFromPre(string userId, string ip) {
            string sql = @"
                update ME_EXPM a
                   set exp_qty = (select exp_qty from ME_EXPM
                                   where twn_yyymm(exp_date) = twn_pym(twn_yyymm(sysdate))
                                     and mmcode = a.mmcode
                                     and lot_no = a.lot_no
                                     and warnym_key = a.warnym_key),
                       update_user = :userId,
                       update_ip = :ip
                 where twn_yyymm(a.exp_date) = twn_yyymm(sysdate)
                   and a.exp_qty = 0
                   and exists (select 1 from ME_EXPM
                                where twn_yyymm(exp_date) = twn_pym(twn_yyymm(sysdate))
                                  and mmcode = a.mmcode
                                  and lot_no = a.lot_no
                                  and warnym_key = a.warnym_key)
            ";
            return DBWork.Connection.Execute(sql, new { userId ,ip}, DBWork.Transaction);
        }
        #endregion
    }
}
