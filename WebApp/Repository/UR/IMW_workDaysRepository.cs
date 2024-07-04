using System.Collections.Generic;
using JCLib.DB;
using Dapper;

namespace TSGH.Repository.UR
{
    public class IMW_workDaysRepository : JCLib.Mvc.BaseRepository
    {
        public IMW_workDaysRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        // 取得例假日
        public IEnumerable<string> Query(int format)
        {
            string charLength = "0";
            if (format == 111)
                charLength = "10"; // yyyy/mm/dd
            else if (format == 112)
                charLength = "8"; // yyyymmdd

            string sql = "";
            sql += @"select convert(CHAR(" + charLength + @"),dateadd(d,Tmp.rows-1,getDate() - 365), @FORMAT) from (
                        select MID, row_number()over(order by MID) rows
                         from MM)Tmp
                         where Tmp.rows <= 730
                        and(select count(*) from IMW_workDays where DATE = convert(CHAR(" + charLength + @"), dateadd(d, Tmp.rows - 1, getDate() - 365), 112)) = 0";
            return DBWork.Connection.Query<string>(sql, new { FORMAT = format}, DBWork.Transaction);
        }
    }
}