using System;
using System.Collections.Generic;
using Dapper;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMSMSBAS
{
    public static class SybaseConnection
    {
        //
        // 摘要:
        //     Executes a query, returning the data typed as T.
        //
        // 參數:
        //   cnn:
        //     The connection to query on.
        //
        //   sql:
        //     The SQL to execute for the query.
        //
        //   param:
        //     The parameters to pass, if any.
        //
        //   transaction:
        //     The transaction to use, if any.
        //
        //   buffered:
        //     Whether to buffer results in memory.
        //
        //   commandTimeout:
        //     The command timeout (in seconds).
        //
        //   commandType:
        //     The type of command to execute.
        //
        // 類型參數:
        //   T:
        //     The type of results to return.
        //
        // 傳回:
        //     A sequence of data of the supplied type; if a basic type (int, string, etc) is
        //     queried then the data from the first column in assumed, otherwise an instance
        //     is created per row, and a direct column-name===member-name mapping is assumed
        //     (case insensitive).
        //public static IEnumerable<T> Query<T>(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null);

        public static IEnumerable<T> SyBaseQuery<T>(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {            
            cnn.Execute("SET CHAR_CONVERT OFF");
            commandTimeout = 60000000;
            return cnn.Query<T>(sql, param, transaction, buffered, commandTimeout, commandType);
        }
    }
}
