using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.AA
{
    public class AA0229Repository : JCLib.Mvc.BaseRepository
    {
        public AA0229Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AA0229> GetAll(string DATA_VALUE, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT DATA_VALUE, DATA_DESC FROM PARAM_D 
                        WHERE GRP_CODE = 'MI_MAST' AND DATA_NAME = 'MAT_CLASS_SUB'";
            if (DATA_VALUE != "")
            {
                sql += " AND DATA_VALUE = :p0";
                p.Add(":p0", string.Format("{0}", DATA_VALUE));
            }
            sql += " ORDER BY DATA_VALUE";
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0229>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<AA0229> Get(string input)
        {
            var sql = @"SELECT DATA_VALUE, DATA_DESC FROM PARAM_D 
                        WHERE GRP_CODE = 'MI_MAST' AND DATA_NAME = 'MAT_CLASS_SUB'
                        AND DATA_VALUE = :DATA_VALUE";

            return DBWork.Connection.Query<AA0229>(sql, new { DATA_VALUE = input }, DBWork.Transaction);
        }

        public bool CheckExists(string input)
        {
            string sql = @"SELECT 1 FROM PARAM_D
                           WHERE GRP_CODE = 'MI_MAST' AND DATA_NAME = 'MAT_CLASS_SUB' AND DATA_VALUE=:DATA_VALUE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DATA_VALUE = input }, DBWork.Transaction) == null);
        }

        public bool CheckExistsInOtherTable(string input, string TableName)
        {
            string sql = @"SELECT 1 FROM " + TableName + " WHERE MAT_CLASS_SUB = :DATA_VALUE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DATA_VALUE = input }, DBWork.Transaction) == null);
        }

        public int Create(AA0229 input)
        {
            var sql = @"INSERT INTO PARAM_D
                        (GRP_CODE, DATA_SEQ, DATA_NAME, DATA_VALUE, DATA_DESC, DATA_REMARK)
                        VALUES 
                        ('MI_MAST', (SELECT MAX(DATA_SEQ) + 1 FROM PARAM_D WHERE GRP_CODE = 'MI_MAST'), 'MAT_CLASS_SUB', :DATA_VALUE, :DATA_DESC, '物料分類子類別')";
            return DBWork.Connection.Execute(sql, input, DBWork.Transaction);
        }

        public int Update(AA0229 input)
        {
            var sql = @"UPDATE PARAM_D SET DATA_DESC = :DATA_DESC
                        WHERE GRP_CODE = 'MI_MAST' AND DATA_NAME = 'MAT_CLASS_SUB'
                        AND DATA_VALUE = :DATA_VALUE";
            return DBWork.Connection.Execute(sql, input, DBWork.Transaction);
        }

        public int Delete(string input)
        {
            var sql = @"DELETE FROM PARAM_D
                        WHERE GRP_CODE = 'MI_MAST' AND DATA_NAME = 'MAT_CLASS_SUB'
                        AND DATA_VALUE = :DATA_VALUE";
            return DBWork.Connection.Execute(sql, new { DATA_VALUE = input }, DBWork.Transaction);
        }
    } 
}