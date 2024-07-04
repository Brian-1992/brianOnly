using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.G
{
    public class GA0004Repository : JCLib.Mvc.BaseRepository
    {
        public GA0004Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<TC_PURUNCOV> GetAll(string PUR_UNIT, string BASE_UNIT, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT * FROM TC_PURUNCOV WHERE 1=1 ";

            if (PUR_UNIT != "")
            {
                sql += " AND PUR_UNIT LIKE :p0 ";
                p.Add(":p0", string.Format("%{0}%", PUR_UNIT));
            }
            if (BASE_UNIT != "")
            {
                sql += " AND BASE_UNIT LIKE :p1 ";
                p.Add(":p1", string.Format("%{0}%", BASE_UNIT));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<TC_PURUNCOV>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<TC_PURUNCOV> Get(TC_PURUNCOV tc_puruncov)
        {
            var sql = @"SELECT * FROM TC_PURUNCOV WHERE PUR_UNIT = :PUR_UNIT AND BASE_UNIT=:BASE_UNIT";
            return DBWork.Connection.Query<TC_PURUNCOV>(sql, tc_puruncov, DBWork.Transaction);
        }

        public int Create(TC_PURUNCOV tc_puruncov)
        {
            var sql = @"INSERT INTO TC_PURUNCOV (PUR_UNIT, BASE_UNIT, PURUN_MULTI, BASEUN_MULTI,  CREATE_TIME, CREATE_USER,  UPDATE_IP,UPDATE_USER, UPDATE_TIME)  
                                VALUES (:PUR_UNIT, :BASE_UNIT, :PURUN_MULTI, :BASEUN_MULTI,  sysdate, :CREATE_USER,  :UPDATE_IP, :UPDATE_USER , sysdate)";
            return DBWork.Connection.Execute(sql, tc_puruncov, DBWork.Transaction);
        }

        public int Update(TC_PURUNCOV tc_puruncov)
        {
            var sql = @"UPDATE TC_PURUNCOV 
                                SET  PURUN_MULTI=:PURUN_MULTI, BASEUN_MULTI=:BASEUN_MULTI, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE PUR_UNIT = :PUR_UNIT AND BASE_UNIT=:BASE_UNIT";
            return DBWork.Connection.Execute(sql, tc_puruncov, DBWork.Transaction);
        }

        public int Delete(TC_PURUNCOV tc_puruncov)
        {
            var sql = @"DELETE  TC_PURUNCOV 
                                WHERE PUR_UNIT = :PUR_UNIT AND BASE_UNIT=:BASE_UNIT";
            return DBWork.Connection.Execute(sql, tc_puruncov, DBWork.Transaction);
        }

        public bool CheckExists(TC_PURUNCOV tc_puruncov)
        {
            string sql = @"SELECT 1 FROM TC_PURUNCOV WHERE PUR_UNIT = :PUR_UNIT AND BASE_UNIT=:BASE_UNIT";
            return !(DBWork.Connection.ExecuteScalar(sql, tc_puruncov, DBWork.Transaction) == null);
        }
    }
}