using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.G
{
    public class GA0007Repository : JCLib.Mvc.BaseRepository
    {
        public GA0007Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<GA0007> GetAll(string mmcode, string mmname,string is_use, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select A.*,TWN_DATE(A.CREATE_TIME)CREATE_TIME_T from TC_MAST A where 1=1 ";


            if (mmcode != "")
            {
                sql += " AND A.MMCODE LIKE :p0 ";
                p.Add(":p0", string.Format("%{0}%", mmcode));
            }
            if (mmname != "")
            {
                sql += " AND A.MMNAME_C LIKE :p1 ";
                p.Add(":p1", string.Format("%{0}%", mmname));
            }
            if (is_use != "")
            {
                sql += " AND A.IS_USE = :p2 ";
                p.Add(":p2", string.Format("{0}", is_use));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<GA0007>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<GA0007> Get(string id)
        {
            var sql = @" select A.*,TWN_DATE(A.CREATE_TIME)CREATE_TIME_T from TC_MAST A where MMCODE = :MMCODE ";
            return DBWork.Connection.Query<GA0007>(sql, new { MMCODE = id }, DBWork.Transaction);
        }
        

        public int Update(GA0007 ga0007)
        {
            var sql = @"UPDATE TC_MAST 
                        SET IS_USE=:IS_USE, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                        WHERE MMCODE = :MMCODE ";
            return DBWork.Connection.Execute(sql, ga0007, DBWork.Transaction);
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
    }
}