using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMSMSREPORT
{
    public static class SybaseConnection
    {
        //胡大哥版本
        public static IEnumerable<T> SyBaseQuery<T>(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            var specConn = MMSMSREPORT.TsghConn.getSpecConn("DB_Sybase_Released");
            cnn.Execute("SET CHAR_CONVERT OFF"); // 三總
            commandTimeout = 60000000;
            return cnn.Query<T>(sql, param, transaction, buffered, commandTimeout, commandType);
        }
        

    }
}
