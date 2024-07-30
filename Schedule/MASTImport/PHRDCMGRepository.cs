using System.Data;
using JCLib.DB;
using Dapper;
using MMSMSBAS.Models;
using System.Collections.Generic;

namespace MASTImport.Repository
{
    class PHRDCMGRepository : JCLib.Mvc.BaseRepository
    {
        public PHRDCMGRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public int DeleteAll()
        {
            string sql = " DELETE FROM MMSADM.HIS_PHRDCMG ";
            return DBWork.Connection.Execute(sql, null, DBWork.Transaction);
        }

        public int Import(IList<PHRDCMGModels> phrdcmgList)
        {
            string sql = @" INSERT INTO MMSADM.HIS_PHRDCMG (
                            ORDERCODE, CHANGETYPE, CREATEDATETIME, CHANGEDATE, DRUGCHANGEMEMO1, DRUGCHANGEMEMO2, INSUSIGNI, INSUSIGNO,
                            CREATEOPID, PROCDATETIME, PROCOPID) 
                            VALUES ( :ORDERCODE, :CHANGETYPE, :CREATEDATETIME, :CHANGEDATE, :DRUGCHANGEMEMO1, :DRUGCHANGEMEMO2, :INSUSIGNI, :INSUSIGNO,
                            :CREATEOPID, :PROCDATETIME, :PROCOPID ) ";
            return DBWork.Connection.Execute(sql, phrdcmgList, DBWork.Transaction);
        }
    }
}
