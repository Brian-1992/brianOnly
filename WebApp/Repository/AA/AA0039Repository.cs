using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.AA 
{
    public class AA0039Repository : JCLib.Mvc.BaseRepository
    {
        public AA0039Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<MI_WHMAST> GetAll(string wh_no, string wh_kind, string wh_grade, string pwh_no, string inid, string sinid, string cancel, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.*,
                        (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='MI_WHMAST' AND DATA_NAME='WH_GRADE' AND DATA_VALUE=A.WH_GRADE) WH_GRADE_N,
                        (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='MI_WHMAST' AND DATA_NAME='WH_KIND' AND DATA_VALUE=A.WH_KIND) WH_KIND_N,
                        (SELECT DATA_DESC  FROM PARAM_D WHERE GRP_CODE='Y_OR_N' AND DATA_NAME='YN' AND DATA_VALUE=A.CANCEL_ID) CANCEL_ID_N,
                        (SELECT INID || ' ' || INID_NAME  FROM UR_INID WHERE INID=A.SUPPLY_INID) SUPPLY_INID_N,
                        (SELECT INID || ' ' || INID_NAME  FROM UR_INID WHERE INID=A.INID) INID_N,
                        (SELECT WH_NAME || '(' || WH_NO || ')' FROM MI_WHMAST WHERE WH_NO=A.PWH_NO) PWH_NO_N,
                        (CASE WHEN ( SELECT '1' FROM MI_WHAUTO WHERE WH_NO = A.WH_NO AND WEEKDAY = '1' ) ='1' THEN 'Y' ELSE 'N' END ) D1,
                        (CASE WHEN ( SELECT '1' FROM MI_WHAUTO WHERE WH_NO = A.WH_NO AND WEEKDAY = '2' ) ='1' THEN 'Y' ELSE 'N' END ) D2,
                        (CASE WHEN ( SELECT '1' FROM MI_WHAUTO WHERE WH_NO = A.WH_NO AND WEEKDAY = '3' ) ='1' THEN 'Y' ELSE 'N' END ) D3,
                        (CASE WHEN ( SELECT '1' FROM MI_WHAUTO WHERE WH_NO = A.WH_NO AND WEEKDAY = '4' ) ='1' THEN 'Y' ELSE 'N' END ) D4,
                        (CASE WHEN ( SELECT '1' FROM MI_WHAUTO WHERE WH_NO = A.WH_NO AND WEEKDAY = '5' ) ='1' THEN 'Y' ELSE 'N' END ) D5,
                        (CASE WHEN ( SELECT '1' FROM MI_WHAUTO WHERE WH_NO = A.WH_NO AND WEEKDAY = '6' ) ='1' THEN 'Y' ELSE 'N' END ) D6,
                        (CASE WHEN ( SELECT '1' FROM MI_WHAUTO WHERE WH_NO = A.WH_NO AND WEEKDAY = '7' ) ='1' THEN 'Y' ELSE 'N' END ) D7 
                        FROM MI_WHMAST A WHERE 1=1 ";

            if (wh_no != "")
            {
                sql += " AND A.WH_NO = :WH_NO ";
                p.Add(":WH_NO", string.Format("{0}", wh_no));
            }
            if (wh_kind != "")
            {
                sql += " AND A.WH_KIND = :p1 ";
                p.Add(":p1", string.Format("{0}", wh_kind));
            }
            if (wh_grade != "")
            {
                sql += " AND A.WH_GRADE = :p2 ";
                p.Add(":p2", string.Format("{0}", wh_grade));
            }
            if (pwh_no != "")
            {
                sql += " AND A.PWH_NO = :p3 ";
                p.Add(":p3", string.Format("{0}", pwh_no));
            }
            if (inid != "")
            {
                sql += " AND A.INID = :p4 ";
                p.Add(":p4", string.Format("{0}", inid));
            }
            if (sinid != "")
            {
                sql += " AND A.SUPPLY_INID = :p5 ";
                p.Add(":p5", string.Format("{0}", sinid));
            }
            if (cancel != "")
            {
                sql += " AND A.CANCEL_ID = :p6 ";
                p.Add(":p6", string.Format("{0}", cancel));
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_WHMAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public DataTable GetExcel(string wh_no, string wh_kind, string wh_grade, string pwh_no, string inid, string sinid, string cancel)
        {
            DynamicParameters p = new DynamicParameters();

            var sql = @"SELECT A.WH_NO as 庫房代碼,
                        A.WH_NAME as 庫房名稱,
                        (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='MI_WHMAST' AND DATA_NAME='WH_GRADE' AND DATA_VALUE=A.WH_GRADE) 庫別級別,
                        (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='MI_WHMAST' AND DATA_NAME='WH_KIND' AND DATA_VALUE=A.WH_KIND) 庫房分類,
                        (SELECT INID || ' ' || INID_NAME  FROM UR_INID WHERE INID=A.SUPPLY_INID) 撥補責任中心,
                        (SELECT INID || ' ' || INID_NAME  FROM UR_INID WHERE INID=A.INID) 責任中心,
                        (SELECT WH_NAME || '(' || WH_NO || ')' FROM MI_WHMAST WHERE WH_NO=A.PWH_NO) 上級庫,
                        A.TEL_NO 電話分機,
                        (SELECT DATA_DESC  FROM PARAM_D WHERE GRP_CODE='Y_OR_N' AND DATA_NAME='YN' AND DATA_VALUE=A.CANCEL_ID) 是否作廢
                        FROM MI_WHMAST A WHERE 1=1 ";

            if (wh_no != "")
            {
                sql += " AND A.WH_NO = :WH_NO ";
                p.Add(":WH_NO", string.Format("{0}", wh_no));
            }
            if (wh_kind != "")
            {
                sql += " AND A.WH_KIND = :p1 ";
                p.Add(":p1", string.Format("{0}", wh_kind));
            }
            if (wh_grade != "")
            {
                sql += " AND A.WH_GRADE = :p2 ";
                p.Add(":p2", string.Format("{0}", wh_grade));
            }
            if (pwh_no != "")
            {
                sql += " AND A.PWH_NO = :p3 ";
                p.Add(":p3", string.Format("{0}", pwh_no));
            }
            if (inid != "")
            {
                sql += " AND A.INID = :p4 ";
                p.Add(":p4", string.Format("{0}", inid));
            }
            if (sinid != "")
            {
                sql += " AND A.SUPPLY_INID = :p5 ";
                p.Add(":p5", string.Format("{0}", sinid));
            }
            if (cancel != "")
            {
                sql += " AND A.CANCEL_ID = :p6 ";
                p.Add(":p6", string.Format("{0}", cancel));
            }
            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<MI_WHMAST> Get(string id)
        {
            var sql = @"SELECT A.*,
                        (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='MI_WHMAST' AND DATA_NAME='WH_GRADE' AND DATA_VALUE=A.WH_GRADE) WH_GRADE_N,
                        (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='MI_WHMAST' AND DATA_NAME='WH_KIND' AND DATA_VALUE=A.WH_KIND) WH_KIND_N,
                        (SELECT DATA_DESC  FROM PARAM_D WHERE GRP_CODE='Y_OR_N' AND DATA_NAME='YN' AND DATA_VALUE=A.CANCEL_ID) CANCEL_ID_N,
                        (SELECT INID || ' ' || INID_NAME  FROM UR_INID WHERE INID=A.SUPPLY_INID) SUPPLY_INID_N,
                        (SELECT INID || ' ' || INID_NAME  FROM UR_INID WHERE INID=A.INID) INID_N,
                        (SELECT WH_NAME || '(' || WH_NO || ')' FROM MI_WHMAST WHERE WH_NO=A.PWH_NO) PWH_NO_N,
                        (CASE WHEN ( SELECT '1' FROM MI_WHAUTO WHERE WH_NO = A.WH_NO AND WEEKDAY = '1' ) ='1' THEN 'Y' ELSE 'N' END ) D1,
                        (CASE WHEN ( SELECT '1' FROM MI_WHAUTO WHERE WH_NO = A.WH_NO AND WEEKDAY = '2' ) ='1' THEN 'Y' ELSE 'N' END ) D2,
                        (CASE WHEN ( SELECT '1' FROM MI_WHAUTO WHERE WH_NO = A.WH_NO AND WEEKDAY = '3' ) ='1' THEN 'Y' ELSE 'N' END ) D3,
                        (CASE WHEN ( SELECT '1' FROM MI_WHAUTO WHERE WH_NO = A.WH_NO AND WEEKDAY = '4' ) ='1' THEN 'Y' ELSE 'N' END ) D4,
                        (CASE WHEN ( SELECT '1' FROM MI_WHAUTO WHERE WH_NO = A.WH_NO AND WEEKDAY = '5' ) ='1' THEN 'Y' ELSE 'N' END ) D5,
                        (CASE WHEN ( SELECT '1' FROM MI_WHAUTO WHERE WH_NO = A.WH_NO AND WEEKDAY = '6' ) ='1' THEN 'Y' ELSE 'N' END ) D6,
                        (CASE WHEN ( SELECT '1' FROM MI_WHAUTO WHERE WH_NO = A.WH_NO AND WEEKDAY = '7' ) ='1' THEN 'Y' ELSE 'N' END ) D7 
                        FROM MI_WHMAST A
                        WHERE A.WH_NO = :WH_NO";
            return DBWork.Connection.Query<MI_WHMAST>(sql, new { WH_NO = id }, DBWork.Transaction);
        }

        public int Create(MI_WHMAST MI_WHMAST)
        {
            var sql = @"INSERT INTO MI_WHMAST (WH_NO, WH_NAME, WH_KIND, WH_GRADE, PWH_NO, INID, SUPPLY_INID, TEL_NO, CANCEL_ID, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                                VALUES (:WH_NO, :WH_NAME, :WH_KIND, :WH_GRADE, :PWH_NO, :INID, :SUPPLY_INID, :TEL_NO, :CANCEL_ID, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, MI_WHMAST, DBWork.Transaction);
        }

        public int Update(MI_WHMAST MI_WHMAST)
        {
            var sql = @"UPDATE MI_WHMAST SET WH_NAME = :WH_NAME, WH_KIND = :WH_KIND, WH_GRADE = :WH_GRADE, PWH_NO = :PWH_NO, INID = :INID, 
                                SUPPLY_INID = :SUPPLY_INID, TEL_NO = :TEL_NO, CANCEL_ID = :CANCEL_ID,
                                UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE WH_NO = :WH_NO";
            return DBWork.Connection.Execute(sql, MI_WHMAST, DBWork.Transaction);
        }
        public int CreateAuto(MI_WHMAST MI_WHMAST)
        {
            var sql = @"INSERT INTO MI_WHAUTO (WH_NO, WEEKDAY, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                                VALUES (:WH_NO, :WEEKDAY, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, MI_WHMAST, DBWork.Transaction);
        }
        public int DeleteAuto(MI_WHMAST MI_WHMAST)
        {
            var sql = @"DELETE MI_WHAUTO WHERE WH_NO = :WH_NO AND WEEKDAY = :WEEKDAY";
            return DBWork.Connection.Execute(sql, MI_WHMAST, DBWork.Transaction);
        }
        //public int Delete(string WH_NO)
        //{
        //    // 資料會在其他地方使用者,刪除時不直接刪除而是加上刪除旗標
        //    var sql = @"UPDATE MI_WHMAST SET CANCEL_ID = 'X'
        //                        WHERE WH_NO = :WH_NO";
        //    return DBWork.Connection.Execute(sql, new { WH_NO = WH_NO }, DBWork.Transaction);
        //}

        public bool CheckExists(string id)
        {
            string sql = @"SELECT 1 FROM MI_WHMAST WHERE WH_NO=:WH_NO";
            return !(DBWork.Connection.ExecuteScalar(sql, new { WH_NO = id }, DBWork.Transaction) == null);
        }
        public bool CheckAutoExists(string id, string dd)
        {
            string sql = @"SELECT 1 FROM MI_WHAUTO WHERE WH_NO=:WH_NO AND WEEKDAY=:WEEKDAY ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { WH_NO = id, WEEKDAY =dd }, DBWork.Transaction) == null);
        }
        public int CheckCancel(string id)
        {
            int i = 0;
            int j = 0;
            int k = 0;
            int rtn = 0;
            string sql1 = @"SELECT 1 FROM MI_WHINV WHERE WH_NO=:WH_NO AND INV_QTY > 0";
            if (!(DBWork.Connection.ExecuteScalar(sql1, new { WH_NO = id }, DBWork.Transaction) == null)) { i = 1; }
            string sql2 = @"SELECT 1 FROM MI_WLOCINV WHERE WH_NO=:WH_NO AND INV_QTY > 0";
            if (!(DBWork.Connection.ExecuteScalar(sql2, new { WH_NO = id }, DBWork.Transaction) == null)) { j = 1; }
            string sql3 = @"SELECT 1 FROM MI_WEXPINV WHERE WH_NO=:WH_NO AND INV_QTY > 0";
            if (!(DBWork.Connection.ExecuteScalar(sql3, new { WH_NO = id }, DBWork.Transaction) == null)) { k = 1; }
            if (i == 1 || j == 1 || k == 1) { rtn = 1; }
            else { rtn = 0; }
            return rtn;
        }
        public IEnumerable<COMBO_MODEL> GetWhnoCombo()
        {
            string sql = @"SELECT DISTINCT WH_NO as VALUE, WH_NAME as TEXT,
                        WH_NO || ' ' || WH_NAME as COMBITEM
                        FROM MI_WHMAST
                        ORDER BY WH_NO";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetPWhnoCombo(string wh_grade, string wh_no)
        {
            var p = new DynamicParameters();
            string sql = @"SELECT DISTINCT WH_NO as VALUE, WH_NAME as TEXT,
                        WH_NO || ' ' || WH_NAME as COMBITEM
                        FROM MI_WHMAST WHERE 1 = 1 ";
            if (wh_grade == "4")
            {
                sql += " AND WH_GRADE IN ('1','2','3') ";
            }
            else if (wh_grade == "3")
            {
                sql += " AND WH_GRADE IN ('1','2') ";
            }
            else if (wh_grade == "2")
            {
                sql += " AND WH_GRADE IN ('1') ";
            }

            if (wh_no != "")
            {
                sql += " AND WH_NO <> :p1 ";
                p.Add(":p1", string.Format("{0}", wh_no));
            }
            sql += "  ORDER BY WH_NO ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql,p);
        }
        public IEnumerable<COMBO_MODEL> GetInidCombo()
        {
            string sql = @"SELECT DISTINCT INID as VALUE, INID_NAME as TEXT,
                        INID || ' ' || INID_NAME as COMBITEM
                        FROM UR_INID
                        WHERE INID_OLD <> 'D' or INID_OLD is null
                        ORDER BY INID";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetYN()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='Y_OR_N' AND DATA_NAME='YN' 
                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetWhGrade()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='MI_WHMAST' AND DATA_NAME='WH_GRADE'
                        --  AND DATA_VALUE IN ('1','2','3','4') 
                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetWhKind()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='MI_WHMAST' AND DATA_NAME='WH_KIND' 
                        --  AND DATA_VALUE IN ('0','1') 
                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public int DeleteWhid(string wh_no) {
            string sql = @"
                delete from MI_WHID where wh_no = :wh_no
            ";
            return DBWork.Connection.Execute(sql, new { wh_no }, DBWork.Transaction);
        }
    }
}
