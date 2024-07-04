using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.AA
{
    public class AA0093Repository : JCLib.Mvc.BaseRepository
    {
        public AA0093Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AA0093> GetAll(
            string wh_no, 
            string months,
            int page_index, int page_size, string sorters)
        {
            int iP = 0;
            String sP = "";
            var p = new DynamicParameters();

            var sql = "";

            sql += "select "; 
            sql += "WH_NO,  "; // 01.
            sql += "A.MMCODE, "; // 02.
            sql += "B.MMNAME_C, "; // 03.
            sql += "B.MMNAME_E, "; // 04.
            sql += "to_char(TO_NUMBER(to_char(A.EXP_DATE,'yyyy'),'9999')-1911) || to_char(A.EXP_DATE,'mmdd') EXP_DATE, "; // 05.
            sql += "LOT_NO, "; // 06.
            sql += "INV_QTY "; // 07.
            sql += "from MI_WEXPINV A, MI_MAST B "; 
            sql += "where 1=1 "; 
            sql += "and A.MMCODE=B.MMCODE ";
            sql += "and ( ";
            sql += "    B.MMNAME_C is not null or ";
            sql += "    B.MMNAME_E is not null ";
            sql += ") ";
            if (wh_no != "")
            {
                sP = ":p" + iP++; // ':p0'
                sql += " AND WH_NO = " + sP + " ";
                p.Add(sP, string.Format("{0}", wh_no));
            }
            if (months != "")
            {
                int iMonth;
                if (int.TryParse(months, out iMonth))
                {
                    sP = ":p" + iP++; // ':p0'
                    //sql += "and EXP_DATE between sysdate and ADD_MONTHS(sysdate-1, " + sP + ") ";
                    sql += "and EXP_DATE < ADD_MONTHS(sysdate-1, " + sP + ") ";
                    p.Add(sP, iMonth);
                }
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0093>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<AA0093> GetPrintData(string p0, string p1)
        {
            var p = new DynamicParameters();
            var sql = "";

            sql += "select ";
            sql += "A.WH_NO WH_NO,  "; // 01.
            sql += "A.MMCODE MMCODE, "; // 02.
            sql += "B.MMNAME_C MMNAME_C, "; // 03.
            sql += "B.MMNAME_E MMNAME_E, "; // 04.
            sql += "to_char(TO_NUMBER(to_char(A.EXP_DATE,'yyyy'),'9999')-1911) || to_char(A.EXP_DATE,'mmdd') EXP_DATE, "; // 05.
            sql += "A.LOT_NO LOT_NO, "; // 06.
            sql += "to_char(A.INV_QTY) INV_QTY "; // 07.
            sql += "from MI_WEXPINV A, MI_MAST B ";
            sql += "where 1=1 ";
            sql += "and A.MMCODE=B.MMCODE ";
            sql += "and B.MMNAME_C is not null ";
            sql += "and B.MMNAME_E is not null ";
            sql += "and A.WH_NO=:wh_no ";
            sql += "and A.EXP_DATE < ADD_MONTHS(sysdate, :months)";

            try
            {
                var AA0093s = DBWork.Connection.Query<AA0093>(sql, new { wh_no=p0, months=p1}, DBWork.Transaction);
                return AA0093s;
            }
            catch (Exception ex)
            {
                throw;
            }
            return null;
        }

        String sBr = "\r\n";
        public IEnumerable<ComboItemModel> GetWhnoCombo(string wh_userId)
        {
            string sql = "";
            sql += " select " + sBr;
            sql += " a.WH_NO AS VALUE, " + sBr;
            sql += " a.WH_NO || ' ' || a.WH_NAME AS TEXT " + sBr;
            sql += " from MI_WHMAST a " + sBr;
            sql += " where 1=1 " + sBr;
            sql += " and a.wh_kind = '0' " + sBr;
            sql += " order by TEXT " + sBr;
            return DBWork.Connection.Query<ComboItemModel>(sql, new { WH_USERID = wh_userId }, DBWork.Transaction);
        }

        public IEnumerable<ComboItemModel> GetMmcodeCombo(string wh_no)
        {
            string sql = @"SELECT MMCODE AS VALUE, MMCODE || ' ' || MMNAME_C AS TEXT
                             FROM MI_MAST a
                            WHERE EXISTS (
                                     SELECT *
                                       FROM MI_WHMM b
                                      WHERE b.WH_NO = :WH_NO
                                        AND a.MMCODE = b.MMCODE
                            ) 
                            ORDER BY VALUE";
            return DBWork.Connection.Query<ComboItemModel>(sql, new { WH_NO = wh_no }, DBWork.Transaction);
        }


        //public IEnumerable<AA0093> Get(string id)
        //{
        //    var sql = @"SELECT * FROM AA0093 WHERE AGEN_NO = :AGEN_NO";
        //    return DBWork.Connection.Query<AA0093>(sql, new { AGEN_NO = id }, DBWork.Transaction);
        //}

        //public int Create(AA0093 AA0093)
        //{
        //    var sql = @"INSERT INTO AA0093 (AGEN_NO, AGEN_NAMEC, AGEN_NAMEE, AGEN_ADD, AGEN_FAX, AGEN_TEL, AGEN_ACC, UNI_NO, AGEN_BOSS, REC_STATUS, EMAIL, EMAIL_1, AGEN_BANK, AGEN_SUB, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
        //                        VALUES (:AGEN_NO, :AGEN_NAMEC, :AGEN_NAMEE, :AGEN_ADD, :AGEN_FAX, :AGEN_TEL, :AGEN_ACC, :UNI_NO, :AGEN_BOSS, :REC_STATUS, :EMAIL, :EMAIL_1, :AGEN_BANK, :AGEN_SUB, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
        //    return DBWork.Connection.Execute(sql, AA0093, DBWork.Transaction);
        //}

        //public int Update(AA0093 AA0093)
        //{
        //    var sql = @"UPDATE AA0093 SET AGEN_NAMEC = :AGEN_NAMEC, AGEN_NAMEE = :AGEN_NAMEE, AGEN_ADD = :AGEN_ADD, AGEN_FAX = :AGEN_FAX, AGEN_TEL = :AGEN_TEL, 
        //                        AGEN_ACC = :AGEN_ACC, UNI_NO = :UNI_NO, AGEN_BOSS = :AGEN_BOSS, REC_STATUS = :REC_STATUS, EMAIL = :EMAIL, EMAIL_1 = :EMAIL_1, 
        //                        AGEN_BANK = :AGEN_BANK, AGEN_SUB = :AGEN_SUB, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
        //                        WHERE AGEN_NO = :AGEN_NO";
        //    return DBWork.Connection.Execute(sql, AA0093, DBWork.Transaction);
        //}

        //public int Delete(string agen_no)
        //{
        //    // 資料會在其他地方使用者,刪除時不直接刪除而是加上刪除旗標
        //    var sql = @"UPDATE AA0093 SET REC_STATUS = 'X'
        //                        WHERE AGEN_NO = :AGEN_NO";
        //    return DBWork.Connection.Execute(sql, new { AGEN_NO = agen_no }, DBWork.Transaction);
        //}

        //public bool CheckExists(string id)
        //{
        //    string sql = @"SELECT 1 FROM AA0093 WHERE AGEN_NO=:AGEN_NO";
        //    return !(DBWork.Connection.ExecuteScalar(sql, new { AGEN_NO = id }, DBWork.Transaction) == null);
        //}

        public class MI_MAST_QUERY_PARAMS
        {
            public string MMCODE;
            public string MMNAME_C;
            public string MMNAME_E;
            public string WH_NO;
            public string DATA_YM;
            public string MONTHS;
        }
    }
}