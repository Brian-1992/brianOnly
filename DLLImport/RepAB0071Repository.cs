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
    public class RepAB0071Repository : JCLib.Mvc.BaseRepository
    {
        public RepAB0071Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        /*
        public int DeleteAll()
        {
            string sql = "DELETE FROM MMSADM.ME_AB0012";
            return DBWork.Connection.Execute(sql, null, DBWork.Transaction);
        }
        */

        public int Import(IList<ME_AB0071Modles> getBuyBackDrugList)
        {
            string sql = @"INSERT INTO MMSADM.ME_AB0071 (
CHARTNO, MEDNO, VISITSEQ, ORDERNO, DETAILNO, ORDERCODE, USEQTY,
 CHINNAME,  SIGNOPID,  CREATEDATETIME,  STOCKCODE,  INOUTFLAG,  ORDERENGNAME,
 RESTRICTCODE,  HIGHPRICEFLAG,  NRCODE,  BEDNO,  NRCODENAME,  DOSE,
 ORDERUNIT,  PATHNO,  FREQNO,  PAYFLAG,  BUYFLAG,  BAGSEQNO,  RXNO) 
VALUES ( 
:CHARTNO, :MEDNO, :VISITSEQ, :ORDERNO, :DETAILNO, :ORDERCODE, :USEQTY,
 :CHINNAME, :SIGNOPID, :CREATEDATETIME, :STOCKCODE, :INOUTFLAG, :ORDERENGNAME,
 :RESTRICTCODE, :HIGHPRICEFLAG, :NRCODE, :BEDNO, :NRCODENAME, :DOSE,
 :ORDERUNIT, :PATHNO, :FREQNO, :PAYFLAG, :BUYFLAG, :BAGSEQNO, :RXNO )";
            return DBWork.Connection.Execute(sql, getBuyBackDrugList, DBWork.Transaction);

        }
    }
}
