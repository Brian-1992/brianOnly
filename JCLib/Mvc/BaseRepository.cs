using System.Collections.Generic;
using System.Data;
using Dapper;
using JCLib.DB;

namespace JCLib.Mvc
{
    public abstract class BaseRepository
    {
        private IUnitOfWork _unitOfWork = null;

        public BaseRepository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IUnitOfWork DBWork { get { return _unitOfWork; } }

        public string GetPagingStatement(string sql, string sort = "")
        {
            return _unitOfWork.GetPagingStatement(sql, sort);
        }
    }
}