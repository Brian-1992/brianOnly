using Dapper;
using JCLib.DB;
using MMSMSREPORT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLLImport
{
    public class RepAB0012Repository : JCLib.Mvc.BaseRepository
    {
        public RepAB0012Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        /*
        public int DeleteAll()
        {
            string sql = "DELETE FROM MMSADM.ME_AB0012";
            return DBWork.Connection.Execute(sql, null, DBWork.Transaction);
        }
        */

        public int Import(IList<ME_AB0012Modles> getPubDrugList)
        {
            string sql = @"INSERT INTO MMSADM.ME_AB0012 (
ORDERNO, DETAILNO, NRCODE,  BEDNO, MEDNO, CHARTNO, VISITSEQ,
ORDERCODE, DOSE, ORDERDR, USEDATETIME, CREATEDATETIME, SIGNOPID, 
USEQTY, RESTQTY, PROVEDR, PROVEID2, MEMO, CHINNAME, ORDERENGNAME, 
SPECNUNIT, ORDERUNIT, STOCKUNIT, FLOORQTY, PROVEID1, CARRYKINDI, 
STOCKTRANSQTYI, STOCKCODE, STARTDATATIME) 
VALUES ( 
:ORDERNO, :DETAILNO, :NRCODE, :BEDNO, :MEDNO, :CHARTNO, :VISITSEQ,
 :ORDERCODE, :DOSE, :ORDERDR, :USEDATETIME, :CREATEDATETIME, :SIGNOPID,
:USEQTY, :RESTQTY, :PROVEDR, :PROVEID2, :MEMO, :CHINNAME, :ORDERENGNAME,
:SPECNUNIT, :ORDERUNIT, :STOCKUNIT, :FLOORQTY, :PROVEID1, :CARRYKINDI,
:STOCKTRANSQTYI, :STOCKCODE, :STARTDATATIME )";
            return DBWork.Connection.Execute(sql, getPubDrugList, DBWork.Transaction);
        }

    }
}
