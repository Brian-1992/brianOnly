using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;


namespace WebApp.Repository.B
{
    public class BA0004Repository : JCLib.Mvc.BaseRepository
    {
        public BA0004Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<BA0004> GetAll(int page_index, int page_size, string sorters) //查詢
        {
            var p = new DynamicParameters();

            var sql = @"  SELECT PSL.JOBNAME,
                                 PSL.LOGTIME,
                                 PSL.MEMO
                          FROM PH_SPRUN_LOG PSL,
                               (  SELECT PSL.SP_NAME,
                                         MAX (PSL.LOGTIME) LOGTIME
                                  FROM PH_SPRUN_LOG PSL
                                  GROUP BY PSL.SP_NAME) PSL2
                          WHERE PSL.SP_NAME = PSL2.SP_NAME
                          AND PSL.LOGTIME = PSL2.LOGTIME
                          ORDER BY PSL.JOBNAME";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<BA0004>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
    }
}