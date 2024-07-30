using System.Data;
using JCLib.DB;
using Dapper;
using MMSMSBAS.Models;
using System.Collections.Generic;

namespace MASTImport.Repository
{
    public class MEDLOCATIONRepository : JCLib.Mvc.BaseRepository
    {
        public MEDLOCATIONRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public int DeleteAll()
        {
            string sql = "DELETE FROM MMSADM.HIS_MEDLOCATION";
            return DBWork.Connection.Execute(sql, null, DBWork.Transaction);
        }

        public int Import(IList<MEDLOCATIONModels> medlocationList)
        {
            string sql = @" INSERT INTO MMSADM.HIS_MEDLOCATION (
                            CUREID, DRUGROOM, LOCIDA, LOCIDB) 
                            VALUES ( :CUREID, :DRUGROOM, :LOCIDA, :LOCIDB ) ";
            return DBWork.Connection.Execute(sql, medlocationList, DBWork.Transaction);
        }
    }
}