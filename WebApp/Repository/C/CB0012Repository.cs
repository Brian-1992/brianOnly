using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JCLib.DB;
using WebApp.Models;
using Dapper;
using System.Data;

namespace WebApp.Repository.C
{
    public class CB0012Repository : JCLib.Mvc.BaseRepository
    {
        public CB0012Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<BC_CATEGORY> GetAll(string xcategory, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT * FROM BC_CATEGORY WHERE 1=1 ";

            if (xcategory != "") {
                sql += "AND XCATEGORY LIKE :p0";
                p.Add(":p0", string.Format("%{0}%", xcategory));
            }
            
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<BC_CATEGORY>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<BC_CATEGORY> Get(string xcategory)
        {
            var sql = @"SELECT * FROM BC_CATEGORY 
                         WHERE XCATEGORY = :xcategory";
            return DBWork.Connection.Query<BC_CATEGORY>(sql, new { XCATEGORY = xcategory}, DBWork.Transaction);
        }

        public int Create(BC_CATEGORY bc_category)
        {
            var sql = @"INSERT INTO BC_CATEGORY (XCATEGORY, DESCRIPT)  
                        VALUES (:XCATEGORY, :DESCRIPT)";
            var req = new { XCATEGORY = bc_category.XCATEGORY, DESCRIPT = bc_category.DESCRIPT };
            return DBWork.Connection.Execute(sql, req, DBWork.Transaction);
        }

        public int Update(BC_CATEGORY bc_category)
        {
            var sql = @"UPDATE BC_CATEGORY 
                           SET DESCRIPT = :DESCRIPT
                         WHERE XCATEGORY = :XCATEGORY";
            var req = new { XCATEGORY = bc_category.XCATEGORY, DESCRIPT = bc_category.DESCRIPT };
            return DBWork.Connection.Execute(sql, req, DBWork.Transaction);
        }

        public int Delete(string xcategory)
        {
            var sql = @"DELETE FROM BC_CATEGORY
                         WHERE XCATEGORY = :XCATEGORY";
            return DBWork.Connection.Execute(sql, new { XCATEGORY = xcategory }, DBWork.Transaction);
        }

        public bool CheckExists(string xcategory) {
            string sql = @"SELECT 1 FROM BC_CATEGORY 
                            WHERE XCATEGORY = :xcategory";
            return !(DBWork.Connection.ExecuteScalar(sql, new { XCATEGORY = xcategory }, DBWork.Transaction) == null);
        }
        public DataTable GetExcel(string xcategory)
        {
            DynamicParameters p = new DynamicParameters();

            string sql = @"select  XCATEGORY AS 條碼分類代碼,  DESCRIPT AS 條碼分類說明
                           from BC_CATEGORY ";

            if (xcategory != "")
            {
                sql += " where XCATEGORY = :p0 ";
                p.Add(":p0", string.Format("{0}", xcategory));
            }

            sql += " order by xcategory ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
    }
}