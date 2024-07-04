using Dapper;
using JCLib.DB;


namespace BES002
{
    class UpCrdocStatusRepository : JCLib.Mvc.BaseRepository
    {
        public UpCrdocStatusRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public int UpCrdocStatus()
        {
            string sql = @"update CR_DOC 
                              set ORDERTIME = sysdate, ORDERID = '排程',
                                  CR_STATUS = 'E'
                            where CR_STATUS = 'B' and EMAIL is not null";
            return DBWork.Connection.Execute(sql, "", DBWork.Transaction);
        }
    }
}
