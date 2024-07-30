using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.G
{
    public class GA0006Repository : JCLib.Mvc.BaseRepository
    {
        public GA0006Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<GA0006> GetAll(string data_ym_s, string data_ym_e, string mmcode, string mmname, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @" SELECT A.*,TWN_DATE(A.CREATE_TIME) CREATE_TIME_T FROM TC_USEQMTR A WHERE 1=1 ";

            if (data_ym_s != "")
            {
                sql += " AND A.DATA_YM >= :p0 ";
                p.Add(":p0", string.Format("{0}", data_ym_s));
            }
            if (data_ym_e != "")
            {
                sql += " AND A.DATA_YM <= :p1 ";
                p.Add(":p1", string.Format("{0}", data_ym_e));
            }
            if (mmcode != "")
            {
                sql += " AND A.MMCODE LIKE :p2 ";
                p.Add(":p2", string.Format("%{0}%", mmcode));
            }
            if (mmname != "")
            {
                sql += " AND A.MMNAME_C LIKE :p3 ";
                p.Add(":p3", string.Format("%{0}%", mmname));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<GA0006>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        
    }
}