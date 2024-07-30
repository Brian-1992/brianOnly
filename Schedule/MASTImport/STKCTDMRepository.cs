using System.Data;
using JCLib.DB;
using Dapper;
using MMSMSBAS.Models;
using System.Collections.Generic;

namespace MASTImport.Repository
{
    class STKCTDMRepository : JCLib.Mvc.BaseRepository
    {
        public STKCTDMRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public int DeleteAll()
        {
            string sql = "DELETE FROM MMSADM.HIS_STKCTDM";
            return DBWork.Connection.Execute(sql, null, DBWork.Transaction);
        }

        public int Import(IList<STKCTDMModels> stkctdmList)
        {
            string sql = @"INSERT INTO MMSADM.HIS_STKCTDM (STOCKCODE, SKORDERCODE, STOCKQTY, AVGCONSUMPTION, SAVEDAYS,
                    SKWORKDAYS, SAFEQTY, FLOORQTY, STOCKPLACE, STOCKRECEIPTNO, EFFECTDATE, NOWCONSUMEFLAG, RESERVEFLAG,
                    PARENTSTOCKCODE, MINPACKQTY, CTDMDCCODE, ADDITIONCOMPUTEFLAG, CHANGECOMPUTEFLAG, APPLYQTY, LASTUSEDATE,
                    LASTAPPLYDATE, CREATEDATETIME, CREATEOPID, PROCDATETIME, PROCOPID, OPENDATE, SURPLUSINSUQTY, SURPLUSHOSPQTY,
                    UDPLACE, FIRSTPLACE, FIXEDSTOCK_FLAG) 
                    VALUES (:STOCKCODE, :SKORDERCODE, :STOCKQTY, :AVGCONSUMPTION, :SAVEDAYS,
                    :SKWORKDAYS, :SAFEQTY, :FLOORQTY, :STOCKPLACE, :STOCKRECEIPTNO, :EFFECTDATE, :NOWCONSUMEFLAG, :RESERVEFLAG,
                    :PARENTSTOCKCODE, :MINPACKQTY, :CTDMDCCODE, :ADDITIONCOMPUTEFLAG, :CHANGECOMPUTEFLAG, :APPLYQTY, :LASTUSEDATE,
                    :LASTAPPLYDATE, :CREATEDATETIME, :CREATEOPID, :PROCDATETIME, :PROCOPID, :OPENDATE, :SURPLUSINSUQTY, :SURPLUSHOSPQTY,
                    :UDPLACE, :FIRSTPLACE, :FIXEDSTOCK_FLAG)";
            return DBWork.Connection.Execute(sql, stkctdmList, DBWork.Transaction);
        }
    }
}
