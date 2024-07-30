using System.Data;
using JCLib.DB;
using Dapper;
using MMSMSBAS.Models;
using System.Collections.Generic;

namespace MASTImport.Repository
{
    public class BASESPCRepository : JCLib.Mvc.BaseRepository
    {
        public BASESPCRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public int DeleteAll()
        {
            string sql = "DELETE FROM MMSADM.HIS_BASESPC";
            return DBWork.Connection.Execute(sql, null, DBWork.Transaction);
        }

        public int Import(IList<BASESPCModels> basespcList)
        {
            string sql = @" INSERT INTO MMSADM.HIS_BASESPC (
                            ESPTYPE, SPECIALORDERCODE, INSUFLAG, ESPRECNO, SYSTEMID, CREATEDATETIME, CREATEOPID, 
                            CANCELFLAG, CANCELOPID, CANCELDATETIME, PROCOPID, PROCDATETIME) 
                            VALUES ( :ESPTYPE, :SPECIALORDERCODE, :INSUFLAG, :ESPRECNO, :SYSTEMID, :CREATEDATETIME, :CREATEOPID, 
                            :CANCELFLAG, :CANCELOPID, :CANCELDATETIME, :PROCOPID, :PROCDATETIME ) ";
            return DBWork.Connection.Execute(sql, basespcList, DBWork.Transaction);
        }
    }
}