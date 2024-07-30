using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.AA
{
    public class AA0119Repository : JCLib.Mvc.BaseRepository
    {
        public AA0119Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<MI_UNITCODE> GetAll(string unit_code, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT * FROM MI_UNITCODE WHERE 1=1 ";

            if(unit_code == "%")
            {
                sql += " AND UNIT_CODE = :p0 ";
                p.Add(":p0", unit_code);
            }
            else if (unit_code != "" && unit_code != "%")
            {
                sql += " AND UNIT_CODE LIKE :p0 ";
                p.Add(":p0", string.Format("%{0}%", unit_code));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_UNITCODE>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MI_UNITCODE> Get(string id)
        {
            var sql = @"SELECT * FROM MI_UNITCODE WHERE UNIT_CODE = :UNIT_CODE";
            return DBWork.Connection.Query<MI_UNITCODE>(sql, new { UNIT_CODE = id }, DBWork.Transaction);
        }

        public int Create(MI_UNITCODE mi_unitcode)
        {
            var sql = @"INSERT INTO MI_UNITCODE (UNIT_CODE, UI_CHANAME, UI_ENGNAME, UI_SNAME, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                                VALUES (:UNIT_CODE, :UI_CHANAME, :UI_ENGNAME, :UI_SNAME, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, mi_unitcode, DBWork.Transaction);
        }

        public int Update(MI_UNITCODE mi_unitcode)
        {
            var sql = @"UPDATE MI_UNITCODE SET UI_CHANAME = :UI_CHANAME, UI_ENGNAME = :UI_ENGNAME, UI_SNAME = :UI_SNAME, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE UNIT_CODE = :UNIT_CODE";
            return DBWork.Connection.Execute(sql, mi_unitcode, DBWork.Transaction);
        }

        public int Delete(string UNIT_CODE)
        {
            // 刪除資料
            var sql = @"DELETE MI_UNITCODE WHERE UNIT_CODE = :UNIT_CODE";
            return DBWork.Connection.Execute(sql, new { UNIT_CODE = UNIT_CODE }, DBWork.Transaction);
        }

        public bool CheckExists(string mi_unitcode)
        {
            string sql = @"SELECT 1 FROM MI_UNITCODE WHERE UNIT_CODE=:UNIT_CODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { UNIT_CODE = mi_unitcode }, DBWork.Transaction) == null);
        }

        public bool CheckExistsInOtherTable(string mi_unitcode, string TableName)
        {
            string sql = @"SELECT 1 FROM " + TableName + " WHERE UNIT_CODE=:UNIT_CODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { UNIT_CODE = mi_unitcode }, DBWork.Transaction) == null);
        }
    }
}