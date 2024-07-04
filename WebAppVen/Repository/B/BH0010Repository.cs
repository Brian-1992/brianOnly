using JCLib.DB;
using Dapper;

namespace WebAppVen.Repository.B
{
    public class BH0010Repository : JCLib.Mvc.BaseRepository
    {
        public BH0010Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        //更新
        public int Update(string crdocno)
        {
            var sql = @"update WB_CR_DOC
                           set REPLYTIME=getdate(),
                               REPLY_STATUS='B'--已回覆待轉入
                         where CRDOCNO=@crdocno
                           and REPLY_STATUS = 'A'-- A待回覆
                       ";
            return DBWork.Connection.Execute(sql, new { CRDOCNO = crdocno }, DBWork.Transaction);
        }
    }
}