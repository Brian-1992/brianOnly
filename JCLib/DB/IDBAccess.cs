using System;
using System.Collections.Generic;
using System.Data;

namespace JCLib.DB
{
    public interface IDBAccess : IDisposable
    {
        IDbConnection GetConnection();

        void BeginTransaction();
        void Commit();
        void Rollback();

        Object ExecScalar(CommandParam param);
        //Object ExecScalar(string sql, object param);

        void ExecReader(CommandParam param, Action<IDataReader> action);
        //void ExecReader(string sql, object param, Action<IDataReader> action);

        DataTable ExecQuery(CommandParam param);
        ApiResponse TryExecQuery(CommandParam param, ApiResponse ar = null);
        //DataTable ExecQuery(string sql, object param);

        int ExecNonQuery(CommandParam param);
        ApiResponse TryExecNonQuery(CommandParam param, ApiResponse ar = null);
        //int ExecNonQuery(string sql, object param);

        ApiResponse GetPagedRows(CommandParam param, int page, int page_size, string sort = "", ApiResponse ar = null);

        //IEnumerable<T> Qeury<T>(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, bool buffered = true);
        //IEnumerable<dynamic> Query(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, bool buffered = true);
        //int Execute(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null);
    }
}
