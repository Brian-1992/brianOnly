using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using WebApp.Models.C;
using System.Collections.Generic;

namespace WebApp.Repository.BE
{
    public class CD0006Repository : JCLib.Mvc.BaseRepository
    {
        String toYmd(String s)
        {
            if (!String.IsNullOrEmpty(s)) 
            {
                if (s.Length>=7) // "2019-05-27T00:00:00"
                {
                    int y;
                    int m;
                    int d;
                    DateTime dt;
                    if (
                        int.TryParse(s.Substring(0, 4), out y) &&
                        int.TryParse(s.Substring(5, 2), out m) &&
                        int.TryParse(s.Substring(8, 2), out d) &&
                        true
                    )
                    {
                        String sYmd = y  + "/" + m.ToString().PadLeft(2, '0') + "/" + d.ToString().PadLeft(2, '0');
                        if (DateTime.TryParse(sYmd, out dt))
                        {
                            return sYmd;
                        }
                    }
                }
            }
            return "";
        }

        public CD0006Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<CD0006> GetAll(
            string wh_no, 
            string pick_date_start, string pick_date_end, 
            string docno, 
            string mmcode,
            string has_confirmed,
            int page_index, int page_size, string sorters)
        {
            int iP = 0;
            String sP = "";
            var p = new DynamicParameters();

            var sql = "";
            sql += "select ";
            sql += "wh_no, ";
            sql += "to_char(pick_date,'yyyy/MM/dd') pick_date, ";
            sql += "(select una from UR_ID where tuser = a.act_pick_userid) as act_pick_username, ";
            sql += "appqty, ";
            sql += "docno, ";
            sql += "mmcode, ";
            sql += "has_confirmed,  ";
            sql += "case has_confirmed ";
            sql += "    when 'Y' then '已確認' ";
            sql += "    when null then '待確認' ";
            sql += "    else '待確認' ";
            sql += "end has_confirmed_status, ";
            sql += "mmname_c, ";
            sql += "mmname_e, ";
            sql += "act_pick_qty, ";
            sql += "base_unit, ";
            sql += "seq, ";
            sql += "'' endl ";
            sql += "from BC_WHPICK a ";
            sql += "where 1 = 1 ";
            if (wh_no != "")
            {
                sP = ":p" + iP++; 
                sql += " AND WH_NO = " + sP + " ";
                p.Add(sP, string.Format("{0}", wh_no));
            }
            //sql += "and wh_no = { 目前庫房號碼 } ";
            pick_date_start = toYmd(pick_date_start);
            pick_date_end = toYmd(pick_date_end);
            if (pick_date_start != "" && pick_date_end !="")
            {
                //sql += " and pick_date between ";
                //sql += " to_date(" + pick_date_start + ",'yyyy/mm/dd') and ";
                //sql += " to_date(" + pick_date_end + ",'yyyy/mm/dd')+1 ";

                sql += " and pick_date between ";
                sP = ":p" + iP++; 
                sql += " to_date(" + sP + ",'yyyy/mm/dd') and ";
                p.Add(sP, pick_date_start);

                sP = ":p" + iP++; 
                sql += " to_date(" + sP + ",'yyyy/mm/dd')+1 ";
                p.Add(sP, pick_date_end);
            }
            else if (pick_date_start != "")
            {
                //sql += " and pick_date >= to_date(" + pick_date_start + ",'yyyy/mm/dd') ";
                sP = ":p" + iP++; 
                sql += " and pick_date >= to_date(" + sP + ",'yyyy/mm/dd') ";
                p.Add(sP, pick_date_start);

            }
            else if (pick_date_end != "")
            {
                //sql += " and pick_date <= to_date(" + pick_date_end + ",'yyyy/mm/dd')+1 ";
                sP = ":p" + iP++; 
                sql += " and pick_date <= to_date(" + sP + ",'yyyy/mm/dd')+1 ";
                p.Add(sP, pick_date_end);
            }
            //sql += "and pick_date>={ 開始揀貨日期} ";
            //sql += "and pick_date<{ 結束揀貨日期}+1  ";
            sql += "and act_pick_userid is not null ";
            sql += "and appqty<>act_pick_qty ";

            if (docno != "")
            {
                // sql += "--有輸入申請單號碼時加這個條件 ";
                // sql += "and docno= { 輸入的申請單號碼 } ";

                sP = ":p" + iP++; 
                sql += "and docno like " + sP + " ";
                p.Add(sP, string.Format("%{0}%", docno));
            }
            if (mmcode != "")
            {
                //sql += "--有輸入院內碼時加這個條件 ";
                //sql += "--and mmcode = { 輸入的院內碼 } ";
                sP = ":p" + iP++; 
                sql += "and mmcode like " + sP + " ";
                p.Add(sP, string.Format("%{0}%", mmcode));
            }
            if (has_confirmed == "true")
            {
                //sql += "--確認狀態點選已確認時 ";
                //sql += "--and has_confirmed = 'Y' ";
                sql += "and has_confirmed= 'Y' ";
            }
            else
            {
                //sql += "--確認狀態點選待確認時 ";
                //sql += "--and has_confirmed is null ";
                sql += "and has_confirmed is null ";
            }
            //sql += "order by pick_date,docno,seq ";





            //if (agen_no != "")
            //{
            //    sql += " AND AGEN_NO LIKE :p0 ";
            //    p.Add(":p0", string.Format("%{0}%", agen_no));
            //}
            //if (agen_namec != "")
            //{
            //    sql += " AND AGEN_NAMEC LIKE :p1 ";
            //    p.Add(":p1", string.Format("%{0}%", agen_namec));
            //}
            //if (agen_namee != "")
            //{
            //    sql += " AND AGEN_NAMEE LIKE :p2 ";
            //    p.Add(":p2", string.Format("%{0}%", agen_namee));
            //}
            //if (agen_add != "")
            //{
            //    sql += " AND AGEN_ADD LIKE :p3 ";
            //    p.Add(":p3", string.Format("%{0}%", agen_add));
            //}
            //if (uni_no != "")
            //{
            //    sql += " AND UNI_NO LIKE :p4 ";
            //    p.Add(":p4", string.Format("%{0}%", uni_no));
            //}
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CD0006>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        String sBr = "\r\n";
        public IEnumerable<ComboItemModel> GetWhnoCombo(string wh_userId)
        {
            string sql = "";
            //sql += " SELECT " + sBr;
            //sql += " WH_NO AS VALUE, " + sBr;
            //sql += " WH_NO || ' ' || WH_NAME AS TEXT " + sBr;
            //sql += " FROM MI_WHMAST a " + sBr;
            //sql += " WHERE EXISTS ( " + sBr;
            //sql += " SELECT *" + sBr;
            //sql += " 	FROM MI_WHID b" + sBr;
            //sql += " 	WHERE a.WH_NO = b.WH_NO" + sBr;
            //sql += " 	AND WH_USERID = :WH_USERID" + sBr;
            //sql += " )" + sBr;
            //sql += " ORDER BY VALUE" + sBr;

            sql += " select " + sBr;
            //sql += " b.wh_no, " + sBr;
            //sql += " b.wh_name, " + sBr;
            //sql += " b.wh_kind, " + sBr;
            //sql += " a.wh_userid " + sBr;
            sql += " b.WH_NO AS VALUE, " + sBr;
            sql += " b.WH_NO || ' ' || b.WH_NAME AS TEXT " + sBr;
            sql += " from MI_WHID a, MI_WHMAST b " + sBr;
            sql += " where 1=1 " + sBr;
            sql += " and a.wh_no = b.wh_no " + sBr;
            sql += " and b.wh_grade = '1' " + sBr; // 1庫 2局(衛星庫) 3病房 4科室 5戰備庫 M醫院軍 S學院軍
            sql += " and a.wh_userid = :WH_USERID " + sBr; // { 登入人員帳號}
            sql += " order by value " + sBr;
            return DBWork.Connection.Query<ComboItemModel>(sql, new { WH_USERID = wh_userId }, DBWork.Transaction);
        }

        public IEnumerable<ComboItemModel> GetMmcodeCombo(
            string wh_no,
            string pick_date_start, 
            string pick_date_end,
            string docno,
            string has_confirmed
        )
        {
            var p = new DynamicParameters();
            int iP = 0;
            String sP = "";

            string sql = "SELECT MMCODE AS VALUE, MMCODE || ' ' || MMNAME_C AS TEXT ";
            sql += "FROM BC_WHPICK a ";
            sql += " WHERE 1 =1 ";
            if (wh_no != "")
            {
                sP = ":p" + iP++; 
                sql += " AND WH_NO = " + sP + " ";
                p.Add(sP, string.Format("{0}", wh_no));
            }
            pick_date_start = toYmd(pick_date_start);
            pick_date_end = toYmd(pick_date_end);
            if (pick_date_start != "" && pick_date_end != "")
            {
                sql += " and pick_date between ";
                sP = ":p" + iP++; 
                sql += " to_date(" + sP + ",'yyyy/mm/dd') and ";
                p.Add(sP, pick_date_start);

                sP = ":p" + iP++; 
                sql += " to_date(" + sP + ",'yyyy/mm/dd')+1 ";
                p.Add(sP, pick_date_end);
            }
            else if (pick_date_start != "")
            {
                sP = ":p" + iP++; 
                sql += " and pick_date >= to_date(" + sP + ",'yyyy/mm/dd') ";
                p.Add(sP, pick_date_start);

            }
            else if (pick_date_end != "")
            {
                sP = ":p" + iP++; 
                sql += " and pick_date <= to_date(" + sP + ",'yyyy/mm/dd')+1 ";
                p.Add(sP, pick_date_end);
            }
            sql += "and act_pick_userid is not null ";
            sql += "and appqty<>act_pick_qty ";
            if (docno != "")
            {
                sP = ":p" + iP++; 
                sql += "and docno like " + sP + " ";
                p.Add(sP, string.Format("%{0}%", docno));
            }
            if (has_confirmed == "true")
            {
                sql += "and has_confirmed= 'Y' ";
            }
            else
            {
                sql += "and has_confirmed is null ";
            }
            sql += " ORDER BY VALUE ";
            return DBWork.Connection.Query<ComboItemModel>(sql, p, DBWork.Transaction);
        }


        public IEnumerable<CD0006> Get(string id)
        {
            var sql = @"SELECT * FROM CD0006 WHERE AGEN_NO = :AGEN_NO";
            return DBWork.Connection.Query<CD0006>(sql, new { AGEN_NO = id }, DBWork.Transaction);
        }

        public int Create(CD0006 CD0006)
        {
            var sql = @"INSERT INTO CD0006 (AGEN_NO, AGEN_NAMEC, AGEN_NAMEE, AGEN_ADD, AGEN_FAX, AGEN_TEL, AGEN_ACC, UNI_NO, AGEN_BOSS, REC_STATUS, EMAIL, EMAIL_1, AGEN_BANK, AGEN_SUB, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                                VALUES (:AGEN_NO, :AGEN_NAMEC, :AGEN_NAMEE, :AGEN_ADD, :AGEN_FAX, :AGEN_TEL, :AGEN_ACC, :UNI_NO, :AGEN_BOSS, :REC_STATUS, :EMAIL, :EMAIL_1, :AGEN_BANK, :AGEN_SUB, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, CD0006, DBWork.Transaction);
        }

        public int Update(CD0006 CD0006)
        {
            var sql = @"UPDATE CD0006 SET AGEN_NAMEC = :AGEN_NAMEC, AGEN_NAMEE = :AGEN_NAMEE, AGEN_ADD = :AGEN_ADD, AGEN_FAX = :AGEN_FAX, AGEN_TEL = :AGEN_TEL, 
                                AGEN_ACC = :AGEN_ACC, UNI_NO = :UNI_NO, AGEN_BOSS = :AGEN_BOSS, REC_STATUS = :REC_STATUS, EMAIL = :EMAIL, EMAIL_1 = :EMAIL_1, 
                                AGEN_BANK = :AGEN_BANK, AGEN_SUB = :AGEN_SUB, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE AGEN_NO = :AGEN_NO";
            return DBWork.Connection.Execute(sql, CD0006, DBWork.Transaction);
        }

        //public int Delete(string agen_no)
        //{
        //    // 資料會在其他地方使用者,刪除時不直接刪除而是加上刪除旗標
        //    var sql = @"UPDATE CD0006 SET REC_STATUS = 'X'
        //                        WHERE AGEN_NO = :AGEN_NO";
        //    return DBWork.Connection.Execute(sql, new { AGEN_NO = agen_no }, DBWork.Transaction);
        //}

        public bool CheckExists(string id)
        {
            string sql = @"SELECT 1 FROM CD0006 WHERE AGEN_NO=:AGEN_NO";
            return !(DBWork.Connection.ExecuteScalar(sql, new { AGEN_NO = id }, DBWork.Transaction) == null);
        }
    }
}