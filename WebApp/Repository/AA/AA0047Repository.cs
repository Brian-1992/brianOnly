using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using TSGH.Models;

namespace WebApp.Repository.AA
{
    public class AA0047Repository : JCLib.Mvc.BaseRepository
    {
        public AA0047Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<MM_WHAPLDT> GetAll(string APPLY_DATE, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            //var sql = @"SELECT SUBSTR(APPLY_DATE,6,2) AS APPLY_DATE, WH_NO FROM MM_WHAPLDT WHERE 1=1 ";
            var sql = @"SELECT TWN_DATE(A.APPLY_DATE) AS APPLY_DATE, A.WH_NO, 
                        (SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.WH_NO) AS WH_NO_N,
                        SUBSTR(TWN_DATE(A.APPLY_DATE),1,5) AS APPLY_YEAR_MONTH, SUBSTR(TWN_DATE(A.APPLY_DATE),6,2) AS APPLY_DAY FROM MM_WHAPLDT A WHERE 1=1 ";

            if (APPLY_DATE != "")
            {
                sql += " AND TWN_DATE(A.APPLY_DATE) LIKE :p0 ";
                p.Add(":p0", string.Format("%{0}%", APPLY_DATE));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MM_WHAPLDT>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MM_WHAPLDT> Get(string Date, string mm_WHAPLDT_WH_NO)
        {
            //var sql = @"SELECT SUBSTR(APPLY_DATE,6,2) AS APPLY_DATE, WH_NO FROM MM_WHAPLDT WHERE APPLY_DATE LIKE :APPLY_DATE";
            var sql = @"SELECT TWN_DATE(APPLY_DATE) AS APPLY_DATE, WH_NO, SUBSTR(TWN_DATE(APPLY_DATE),1,5) AS APPLY_YEAR_MONTH, SUBSTR(TWN_DATE(APPLY_DATE),6,2) AS APPLY_DAY FROM MM_WHAPLDT WHERE APPLY_DATE LIKE :APPLY_DATE AND WH_NO = :WH_NO";
            return DBWork.Connection.Query<MM_WHAPLDT>(sql, new { APPLY_DATE = Date, WH_NO = mm_WHAPLDT_WH_NO }, DBWork.Transaction);
        }

        public int Create(MM_WHAPLDT mm_WHAPLDT)
        {
            //var sql = @"INSERT INTO MM_WHAPLDT (APPLY_DATE, WH_NO, CREATE_DATE, CREATE_USER, UPDATE_DATE, UPDATE_USER, UPDATE_IP)  
            //                    VALUES (:APPLY_YEAR_MONTH||''||:APPLY_DAY, :WH_NO, TWN_SYSTIME, :CREATE_USER, TWN_SYSTIME, :UPDATE_USER, :UPDATE_IP)";
            var sql = @"INSERT INTO MM_WHAPLDT (APPLY_DATE, WH_NO, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                                VALUES (TWN_TODATE(:APPLY_YEAR_MONTH||''||:APPLY_DAY), :WH_NO, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, mm_WHAPLDT, DBWork.Transaction);
        }

        public int Update(MM_WHAPLDT mm_WHAPLDT)
        {
            var sql = @"UPDATE MM_WHAPLDT SET WH_NO = :WH_NO, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE APPLY_DATE = TWN_TODATE(:APPLY_YEAR_MONTH||''||:APPLY_DAY) AND WH_NO = :WH_NO_OLD";
            return DBWork.Connection.Execute(sql, mm_WHAPLDT, DBWork.Transaction);
        }

        public int Delete(string apply_date, string wh_no)
        {
            // 資料會在其他地方使用者,刪除時不直接刪除而是加上刪除旗標
            var sql = @"DELETE MM_WHAPLDT WHERE APPLY_DATE = TWN_TODATE(:APPLY_DATE) AND WH_NO = :WH_NO";
            return DBWork.Connection.Execute(sql, new { APPLY_DATE = apply_date, WH_NO = wh_no }, DBWork.Transaction);
        }

        public bool CheckExists(string apply_date, string wh_no)
        {
            string sql = @"SELECT 1 FROM MM_WHAPLDT WHERE APPLY_DATE = TWN_TODATE(:APPLY_DATE) AND WH_NO = :WH_NO";
            return !(DBWork.Connection.ExecuteScalar(sql, new { APPLY_DATE = apply_date, WH_NO = wh_no }, DBWork.Transaction) == null);
        }

        //取得庫房代碼
        public IEnumerable<ComboModel> GetmiWhmastCombo()
        {
            //20190415  COMBITEM去掉右空白
            string sql = @"SELECT DISTINCT A.WH_NO AS KEY_CODE, 
                        RTrim(A.WH_NO ||' '|| A.WH_NAME) AS COMBITEM
                        FROM MI_WHMAST A
                        WHERE WH_KIND='1'
                        ORDER BY WH_NO";

            return DBWork.Connection.Query<ComboModel>(sql);
        }

        //匯出
        public DataTable GetExcel()
        {
            var p = new DynamicParameters();

            var sql = @" SELECT '' 撥發年月,'' 撥發日,'' 庫房代碼 FROM DUAL ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }


        public bool CheckExistsWH_NO(string id)
        {
            string sql = @" SELECT 1 FROM MI_WHMAST WHERE WH_NO = :WH_NO";
            return !(DBWork.Connection.ExecuteScalar(sql, new { WH_NO = id }, DBWork.Transaction) == null);
        }

        public bool CheckExistsTWNDate(string mm, string dd)
        {
            string sql = @" SELECT TWN_TODATE(:APPLY_YEAR_MONTH||''||LPAD(:APPLY_DAY,2,'0'))as chk FROM DUAL ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { APPLY_YEAR_MONTH = mm, APPLY_DAY = dd }, DBWork.Transaction) == null);
        }

        public bool CheckExistsTWNMonth(string apply_date, string wh_no)
        {
            string sql = @"SELECT 1 FROM MM_WHAPLDT WHERE WH_NO = :WH_NO AND SUBSTR(TWN_DATE(APPLY_DATE),0,5) = :APPLY_DATE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { APPLY_DATE = apply_date, WH_NO = wh_no }, DBWork.Transaction) == null);
        }
        //確認更新 於Log表插入新紀錄 MM_MAST_UPD
        public int Insert2(AA0047M aa0047m)
        {
            var sql = @"INSERT INTO MM_WHAPLDT (APPLY_DATE, WH_NO, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                                VALUES (TWN_TODATE(:APPLY_YEAR_MONTH||''||LPAD(:APPLY_DAY,2,'0')), :WH_NO, SYSDATE, :UPDATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)
                       ";

            return DBWork.Connection.Execute(sql, aa0047m, DBWork.Transaction);
        }

        //確認更新 修改基本檔資料 MI_MAST
        public int Update2(AA0047M aa0047m)
        {
            var sql = @"UPDATE MM_WHAPLDT SET APPLY_DATE = TWN_TODATE(:APPLY_YEAR_MONTH||''||LPAD(:APPLY_DAY,2,'0')), 
                                UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE WH_NO = :WH_NO";


            return DBWork.Connection.Execute(sql, aa0047m, DBWork.Transaction);
        }

        public bool CheckExists(string id)
        {
            string sql = @"SELECT 1 FROM MM_WHAPLDT WHERE WH_NO=:WH_NO";
            return !(DBWork.Connection.ExecuteScalar(sql, new { WH_NO = id }, DBWork.Transaction) == null);
        }
    }
}
