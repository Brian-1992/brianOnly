using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;


namespace WebApp.Repository.C
{
    public class CB0005Repository : JCLib.Mvc.BaseRepository
    {
        public CB0005Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<BC_MANAGER> GetAll(string WH_NO, string MANAGERID, string MANAGERNM, int page_index, int page_size, string sorters) //查詢
        {
            var p = new DynamicParameters();

            var sql = @"SELECT TRIM (BMR.WH_NO) WH_NO,
                               (SELECT TRIM (MWT.WH_NO || ' ' || MWT.WH_NAME)
                                FROM MI_WHMAST mwt
                                WHERE MWT.WH_NO = BMR.WH_NO) DISPLAY_WHNO,
                               (SELECT TRIM (MWT.WH_NO || ' ' || MWT.WH_NAME)
                                FROM MI_WHMAST mwt
                                WHERE MWT.WH_NO = BMR.WH_NO) TEXT_WHNO,
                               TRIM (BMR.MANAGERID) MANAGERID,
                               TRIM (BMR.MANAGERID) DISPLAY_MANAGERID,
                               TRIM (BMR.MANAGERID) TEXT_MANAGERID,
                               TRIM (BMR.MANAGERNM) MANAGERNM,
                               TRIM (BMR.MANAGERNM) TEXT_MANAGERNM,
                               TRIM (BMR.USERID) USERID,
                               (SELECT TRIM (URID.UNA)
                                FROM UR_ID URID
                                WHERE URID.TUSER = BMR.USERID) MNAME,
                               (SELECT COUNT (*)
                                FROM BC_ITMANAGER BIR
                                WHERE BIR.MANAGERID = BMR.MANAGERID AND BIR.WH_NO = BMR.WH_NO) CNT
                        FROM BC_MANAGER BMR
                        WHERE 1 = 1";

            if (WH_NO != "")
            {
                sql += " AND BMR.WH_NO = :p0 ";
                p.Add(":p0", string.Format("{0}", WH_NO));
            }
            if (MANAGERID != "")
            {
                sql += " AND BMR.MANAGERID LIKE :p1 ";
                p.Add(":p1", string.Format("{0}%", MANAGERID));
            }
            if (MANAGERNM != "")
            {
                sql += " AND BMR.MANAGERNM LIKE :p2 ";
                p.Add(":p2", string.Format("{0}%", MANAGERNM));
            }
            sql += "order by BMR.MANAGERID";
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<BC_MANAGER>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<BC_MANAGER> Get(string wh_no, string managerid)
        {
            var sql = @"SELECT TRIM (BMR.WH_NO) WH_NO,
                               (SELECT TRIM (MWT.WH_NO || ' ' || MWT.WH_NAME)
                                FROM MI_WHMAST mwt
                                WHERE MWT.WH_NO = BMR.WH_NO) DISPLAY_WHNO,
                               (SELECT TRIM (MWT.WH_NO || ' ' || MWT.WH_NAME)
                                FROM MI_WHMAST mwt
                                WHERE MWT.WH_NO = BMR.WH_NO) TEXT_WHNO,
                               TRIM (BMR.MANAGERID) MANAGERID,
                               TRIM (BMR.MANAGERID) DISPLAY_MANAGERID,
                               TRIM (BMR.MANAGERID) TEXT_MANAGERID,
                               TRIM (BMR.MANAGERNM) MANAGERNM,
                               TRIM (BMR.MANAGERNM) TEXT_MANAGERNM,
                               TRIM (BMR.USERID) USERID,
                               (SELECT TRIM (URID.UNA)
                                FROM UR_ID URID
                                WHERE URID.TUSER = BMR.USERID) MNAME,
                               (SELECT COUNT (*)
                                FROM BC_ITMANAGER BIR
                                WHERE BIR.MANAGERID = BMR.MANAGERID AND BIR.WH_NO = BMR.WH_NO) CNT
                        FROM BC_MANAGER BMR
                        WHERE BMR.WH_NO = :WH_NO
                        AND BMR.MANAGERID = :MANAGERID";
            return DBWork.Connection.Query<BC_MANAGER>(sql, new { WH_NO = wh_no, MANAGERID = managerid }, DBWork.Transaction);
        }

        public int Create(BC_MANAGER bc_manager) //新增
        {
            var sql = @"INSERT INTO BC_MANAGER (WH_NO, MANAGERID, MANAGERNM, USERID, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                                VALUES (:WH_NO, :MANAGERID, :MANAGERNM, :USERID, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, bc_manager, DBWork.Transaction);
        }

        public int Update(BC_MANAGER bc_manager) //修改
        {
            var sql = @"UPDATE BC_MANAGER SET MANAGERNM = :MANAGERNM, USERID = :USERID,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE WH_NO = :WH_NO AND MANAGERID = :MANAGERID";
            return DBWork.Connection.Execute(sql, bc_manager, DBWork.Transaction);
        }

        public int Delete(string wh_no, string managerid)
        {
            var sql = @"DELETE FROM BC_MANAGER
                        WHERE WH_NO = :WH_NO AND MANAGERID = :MANAGERID";
            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, MANAGERID = managerid }, DBWork.Transaction);
        }

        public bool CheckExists(string WH_NO, string MANAGERID)
        {
            string sql = @"SELECT 1 FROM BC_MANAGER WHERE WH_NO = :WH_NO AND MANAGERID = :MANAGERID";
            return !(DBWork.Connection.ExecuteScalar(sql, new { WH_NO = WH_NO, MANAGERID = MANAGERID }, DBWork.Transaction) == null);
        }

        public IEnumerable<ComboItemModel> GetWhno(string user_id)
        {
            string sql = @"SELECT TRIM(MWT.WH_NO) VALUE,
                                  TRIM(MWT.WH_NO || ' ' || MWT.WH_NAME) TEXT
                           FROM MI_WHMAST MWT
                           WHERE EXISTS
                                       (SELECT URID.INID
                                        FROM UR_ID URID 
                                        WHERE URID.TUSER = :USER_ID
                                        AND URID.INID = MWT.INID)";
            return DBWork.Connection.Query<ComboItemModel>(sql, new { USER_ID = user_id }, DBWork.Transaction);
        }
        public IEnumerable<ComboItemModel> GetUserId(string user_id)
        {
            string sql = @"SELECT TRIM(TUSER) VALUE,
                                  TRIM(TUSER || ' ' || UNA) TEXT
                           FROM UR_ID
                           WHERE TUSER = :USERID
                           ORDER BY TUSER";
            return DBWork.Connection.Query<ComboItemModel>(sql, new { USERID = user_id }, DBWork.Transaction);
        }

        public DataTable GetExcel(string WH_NO, string MANAGERID, string MANAGERNM) //查詢
        {
            var p = new DynamicParameters();

            var sql = @"SELECT TRIM (BMR.WH_NO) 庫房別,                             
                               
                               TRIM (BMR.MANAGERID) 管理人員代號,
                               TRIM (BMR.MANAGERNM) 管理人員敘述,
                               (SELECT TRIM (URID.UNA)
                                FROM UR_ID URID
                                WHERE URID.TUSER = BMR.USERID) 管理人員,
                               (SELECT COUNT (*)
                                FROM BC_ITMANAGER BIR
                                WHERE BIR.MANAGERID = BMR.MANAGERID AND BIR.WH_NO = BMR.WH_NO) 管理品項數
                        FROM BC_MANAGER BMR
                        WHERE 1 = 1";

            if (WH_NO != "")
            {
                sql += " AND BMR.WH_NO = :p0 ";
                p.Add(":p0", string.Format("{0}", WH_NO));
            }
            if (MANAGERID != "")
            {
                sql += " AND BMR.MANAGERID LIKE :p1 ";
                p.Add(":p1", string.Format("{0}%", MANAGERID));
            }
            if (MANAGERNM != "")
            {
                sql += " AND BMR.MANAGERNM LIKE :p2 ";
                p.Add(":p2", string.Format("{0}%", MANAGERNM));
            }
            sql += "order by BMR.MANAGERID";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

    }
}