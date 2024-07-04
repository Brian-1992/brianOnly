using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using JCLib.Mvc;

namespace JCLib.DB
{
    public interface IUnitOfWork : IDisposable
    {
        Guid Id { get; }
        IDbConnection Connection { get; }
        IDbTransaction Transaction { get; }
        PagingInfo PagingInfo { get; }
        UserInfo UserInfo { get; }
        string ProcUser { get; }
        string ProcIP { get; }
        string GetPagingStatement(string sql, string sort);
        void BeginTransaction();
        void Commit();
        void Rollback();
        IEnumerable<T> PagingQuery<T>(string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null);
    }
}
