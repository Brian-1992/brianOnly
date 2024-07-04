using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using JCLib.Mvc;

namespace JCLib.DB
{
    public sealed class UnitOfWork : IUnitOfWork
    {
        IDbConnection _connection = null;
        IDbTransaction _transaction = null;
        Guid _id = Guid.Empty;
        UserInfo _userInfo = null;
        PagingInfo _pagingInfo = null;
        string _procUser = null;
        string _procIP = null;

        internal UnitOfWork(IDbConnection connection, IWebDataProvider provider)
        {
            _id = Guid.NewGuid();
            _connection = connection;
            if (provider != null)
            {
                _userInfo = new UserInfo(provider.UserInfo);
                _pagingInfo = new PagingInfo(provider.PageIndex, provider.PageSize, provider.Sort);
                _procUser = provider.ProcUser;
                _procIP = provider.ProcIP;
            }
        }

        IDbConnection IUnitOfWork.Connection
        {
            get { return _connection; }
        }
        IDbTransaction IUnitOfWork.Transaction
        {
            get { return _transaction; }
        }
        Guid IUnitOfWork.Id
        {
            get { return _id; }
        }

        public UserInfo UserInfo { get { return _userInfo; } }
        public PagingInfo PagingInfo { get { return _pagingInfo; } }
        public string ProcUser { get { return _procUser; } }
        public string ProcIP { get { return _procIP; } }
        public Func<string, string, string> PagingStatementFunc;
        public string GetPagingStatement(string sql, string order)
        {
            return this.PagingStatementFunc(sql, order);
        }
        
        public DynamicParameters AddPagingParam(object param)
        {
            var _pageIndex = PagingInfo.PageIndex;
            var _pageSize = PagingInfo.PageSize;
            DynamicParameters dynamicParams = new DynamicParameters(param);

            dynamicParams.Add("OFFSET", (_pageIndex - 1) * _pageSize);
            dynamicParams.Add("PAGE_SIZE", _pageSize);

            return dynamicParams;
        }

        public IEnumerable<T> PagingQuery<T>(string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null) =>
            _connection.Query<T>(GetPagingStatement(sql, PagingInfo.Sort), AddPagingParam(param), transaction, buffered, commandTimeout, commandType);

        public void BeginTransaction()
        {
            _transaction = _connection.BeginTransaction();
        }

        public void Commit()
        {
            _transaction.Commit();
        }

        public void Rollback()
        {
            _transaction.Rollback();
        }

        public void Dispose()
        {
            if (_transaction != null)
                _transaction.Dispose();
            _transaction = null;
        }
    }
}
