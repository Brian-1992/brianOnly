using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.AA
{
    public class AA0118Repository : JCLib.Mvc.BaseRepository
    {
        public AA0118Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<MI_MATCLASS> GetAll(string MAT_CLASS, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT * FROM MI_MATCLASS WHERE 1=1 ";

            if (MAT_CLASS != "")
            {
                sql += " AND MAT_CLASS LIKE :p0 ";
                p.Add(":p0", string.Format("%{0}%", MAT_CLASS));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MATCLASS>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MI_MATCLASS> Get(string MAT_CLASS_id)
        {
            var sql = @"SELECT * FROM MI_MATCLASS WHERE MAT_CLASS = :MAT_CLASS";
            return DBWork.Connection.Query<MI_MATCLASS>(sql, new { MAT_CLASS = MAT_CLASS_id }, DBWork.Transaction);
        }

        public int Create(MI_MATCLASS mi_MATCLASS)
        {
            var sql = @"INSERT INTO MI_MATCLASS (MAT_CLASS, MAT_CLSNAME, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                                VALUES (:MAT_CLASS, :MAT_CLSNAME, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, mi_MATCLASS, DBWork.Transaction);
        }

        public int Update(MI_MATCLASS mi_MATCLASS)
        {
            var sql = @"UPDATE MI_MATCLASS SET MAT_CLSNAME = :MAT_CLSNAME, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE MAT_CLASS = :MAT_CLASS";
            return DBWork.Connection.Execute(sql, mi_MATCLASS, DBWork.Transaction);
        }

        public int Delete(string MAT_CLASS)
        {
            // 刪除資料
            var sql = @"DELETE MI_MATCLASS WHERE MAT_CLASS = :MAT_CLASS";
            return DBWork.Connection.Execute(sql, new { MAT_CLASS = MAT_CLASS }, DBWork.Transaction);
        }

        public bool CheckExists(string MAT_CLASS_id)
        {
            string sql = @"SELECT 1 FROM MI_MATCLASS WHERE MAT_CLASS=:MAT_CLASS";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MAT_CLASS = MAT_CLASS_id }, DBWork.Transaction) == null);
        }

        public bool CheckExistsInOtherTable(string MAT_CLASS_id, string TableName)
        {
            string sql = @"SELECT 1 FROM "+ TableName + " WHERE MAT_CLASS=:MAT_CLASS";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MAT_CLASS = MAT_CLASS_id }, DBWork.Transaction) == null);
        }

        public DataTable GetExcel(string p0)
        {
            DynamicParameters p = new DynamicParameters();

            var sql = @"SELECT MAT_CLASS AS 物料分類代碼,MAT_CLSNAME AS 物料分類名稱 FROM MI_MATCLASS WHERE 1=1 ";

            if (p0 != "")
            {
                sql += " AND MAT_CLASS LIKE :p0 ";
                p.Add(":p0", string.Format("%{0}%", p0));
            }
            sql += @" ORDER BY MAT_CLASS";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

    }
}