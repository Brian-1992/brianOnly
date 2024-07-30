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
    public class RepAB0079DRepository : JCLib.Mvc.BaseRepository
    {
        public RepAB0079DRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        /*
        public int DeleteAll()
        {
            string sql = "DELETE FROM MMSADM.ME_AB0012";
            return DBWork.Connection.Execute(sql, null, DBWork.Transaction);
        }
        */

        public int Import(IList<ME_AB0079DModles> lst)
        {
            string sql = @"
INSERT INTO MMSADM.ME_AB0079 (
    ORDERCODE,
    ORDERENGNAME,
    CREATEYM,
    ORDERDR,
    CHINNAME,
    SECTIONNO,
    SECTIONNAME,
    SUMQTY,
    SUMAMOUNT,
    OPDQTY,
    OPDAMOUNT,
    DSM
) VALUES ( 
    :ORDERCODE,
    :ORDERENGNAME,
    :CREATEYM,
    :ORDERDR,
    :CHINNAME,
    :SECTIONNO,
    :SECTIONNAME,
    :SUMQTY,
    :SUMAMOUNT,
    :OPDQTY,
    :OPDAMOUNT,
    :DSM
)";
            return DBWork.Connection.Execute(sql, lst, DBWork.Transaction);
        } // 
        public int Import(IList<ME_AB0079SModles> lst)
        {
            string sql = @"
INSERT INTO MMSADM.ME_AB0079 (
    ORDERCODE,
    ORDERENGNAME,
    CREATEYM,
    SUMQTY,
    SUMAMOUNT,
    OPDQTY,
    OPDAMOUNT,
    DSM
) VALUES ( 
    :ORDERCODE,
    :ORDERENGNAME,
    :CREATEYM,
    :SUMQTY,
    :SUMAMOUNT,
    :OPDQTY,
    :OPDAMOUNT,
    :DSM
)";
            return DBWork.Connection.Execute(sql, lst, DBWork.Transaction);
        } // 
        public int Import(IList<ME_AB0079MModles> lst)
        {
            string sql = @"
INSERT INTO MMSADM.ME_AB0079 (
    ORDERCODE,
    ORDERENGNAME,
    CREATEYM,
    -- ORDERDR,
    -- CHINNAME,
    SECTIONNO,
    SECTIONNAME,
    SUMQTY,
    SUMAMOUNT,
    OPDQTY,
    OPDAMOUNT,
    DSM
) VALUES ( 
    :ORDERCODE,
    :ORDERENGNAME,
    :CREATEYM,
    -- :ORDERDR,
    -- :CHINNAME,
    :SECTIONNO,
    :SECTIONNAME,
    :SUMQTY,
    :SUMAMOUNT,
    :OPDQTY,
    :OPDAMOUNT,
    :DSM
)";
            return DBWork.Connection.Execute(sql, lst, DBWork.Transaction);
        } // 

        public int Import(IList<ME_AB0083AModles> lst)
        {
            string sql = @"
INSERT INTO MMSADM.ME_AB0083A (
    CREATEDATE,
    NRNAME,
    ORDERCODE,
    ORDERENGNAME,
    HISSYSCODENAME,
    NRCODE
) VALUES ( 
    :CREATEDATE,
    :NRNAME,
    :ORDERCODE,
    :ORDERENGNAME,
    :HISSYSCODENAME,
    :NRCODE
)";
            return DBWork.Connection.Execute(sql, lst, DBWork.Transaction);
        } // 

        public int Import(IList<ME_AB0083BModles> lst)
        {
            string sql = @"
INSERT INTO MMSADM.ME_AB0083B (
    CREATEDATE,
    NRNAME,
    ORDERCODE,
    ORDERENGNAME,
    QTY,
    MONEY,
    NRCODE
) VALUES ( 
    :CREATEDATE,
    :NRNAME,
    :ORDERCODE,
    :ORDERENGNAME,
    :QTY,
    :MONEY,
    :NRCODE
)";         
            return DBWork.Connection.Execute(sql, lst, DBWork.Transaction);
        } // 

        public int Import(IList<ME_AB0083CModles> lst)
        {
            string sql = @"
INSERT INTO MMSADM.ME_AB0083C (
    CREATEDATE,
    NRNAME,
    ORDERCODE,
    ORDERENGNAME,
    QTY,
    MONEY,
    NRCODE
) VALUES ( 
    :CREATEDATE,
    :NRNAME,
    :ORDERCODE,
    :ORDERENGNAME,
    :QTY,
    :MONEY,
    :NRCODE
)";
            return DBWork.Connection.Execute(sql, lst, DBWork.Transaction);
        } // 

        public int Import(IList<ME_AB0083DModles> lst)
        {
            string sql = @"
INSERT INTO MMSADM.ME_AB0083D (
    CREATEDATE,
    NRNAME,
    ORDERCODE,
    ORDERENGNAME,
    QTY,
    MONEY,
    HISSYSCODENAME,
    NRCODE
) VALUES ( 
    :CREATEDATE,
    :NRNAME,
    :ORDERCODE,
    :ORDERENGNAME,
    :QTY,
    :MONEY,
    :HISSYSCODENAME,
    :NRCODE
)";
            return DBWork.Connection.Execute(sql, lst, DBWork.Transaction);
        } // 

        public int Import(IList<ME_AB0013Modles> lst)
        {
            string sql = @"
INSERT INTO MMSADM.ME_AB0013 (
    MEDNO,
    CHARTNO,
    CHINNAME,
    VISITSEQ,
    NRCODE,
    BEDNO,
    CREATEDATETIME
) VALUES ( 
    :MEDNO,
    :CHARTNO,
    :CHINNAME,
    :VISITSEQ,
    :NRCODE,
    :BEDNO,
    :CREATEDATETIME
)";
            return DBWork.Connection.Execute(sql, lst, DBWork.Transaction);
        } // 

        public int Import(IList<ME_AB0012Modles> lst)
        {
            string sql = @"
INSERT INTO MMSADM.ME_AB0012 (
    ORDERNO,
    DETAILNO,
    NRCODE,
    BEDNO,
    MEDNO,
    CHARTNO,
    VISITSEQ,
    ORDERCODE,
    DOSE,
    ORDERDR,
    USEDATETIME,
    CREATEDATETIME,
    SIGNOPID,
    USEQTY,
    RESTQTY,
    PROVEDR,
    PROVEID2,
    MEMO,
    CHINNAME,
    ORDERENGNAME,
    SPECNUNIT,
    ORDERUNIT,
    STOCKUNIT,
    FLOORQTY,
    PROVEID1,
    CARRYKINDI,
    STOCKTRANSQTYI,
    STOCKCODE,
    STARTDATATIME
    -- RDOCNO,
    -- RSEQ
) VALUES ( 
    :ORDERNO,
    :DETAILNO,
    :NRCODE,
    :BEDNO,
    :MEDNO,
    :CHARTNO,
    :VISITSEQ,
    :ORDERCODE,
    :DOSE,
    :ORDERDR,
    :USEDATETIME,
    :CREATEDATETIME,
    :SIGNOPID,
    :USEQTY,
    :RESTQTY,
    :PROVEDR,
    :PROVEID2,
    :MEMO,
    :CHINNAME,
    :ORDERENGNAME,
    :SPECNUNIT,
    :ORDERUNIT,
    :STOCKUNIT,
    :FLOORQTY,
    :PROVEID1,
    :CARRYKINDI,
    :STOCKTRANSQTYI,
    :STOCKCODE,
    :STARTDATATIME
    -- :RDOCNO,
    -- :RSEQ,
)";
            return DBWork.Connection.Execute(sql, lst, DBWork.Transaction);
        } // 

        public int Import(IList<ME_AB0075AModles> lst)
        {
            string sql = @"
INSERT INTO MMSADM.ME_AB0075A (
    PORC_DATE,
    PAT_CNT,
    TOT_CNT
) VALUES ( 
    :PORC_DATE,
    :PAT_CNT,
    :TOT_CNT
)";
            return DBWork.Connection.Execute(sql, lst, DBWork.Transaction);
        } // 

        public int Import(IList<ME_AB0075BModles> lst)
        {
            string sql = @"
INSERT INTO MMSADM.ME_AB0075B (
    SDATE,
    CNT,
    ORDERCODE,
    SUMQTY
) VALUES ( 
    :SDATE,
    :CNT,
    :ORDERCODE,
    :SUMQTY
)";
            return DBWork.Connection.Execute(sql, lst, DBWork.Transaction);
        } // 

        public int Import(IList<ME_AB0075CModles> lst)
        {
            string sql = @"
INSERT INTO MMSADM.ME_AB0075C (
    VISITDATE,
    CNTVISITSEQ,
    CNTORDERNO
) VALUES ( 
    :VISITDATE,
    :CNTVISITSEQ,
    :CNTORDERNO
)";
            return DBWork.Connection.Execute(sql, lst, DBWork.Transaction);
        } // 

        public int Import(IList<ME_AB0075DModles> lst)
        {
            string sql = @"
INSERT INTO MMSADM.ME_AB0075D (
    WORKDATE,
    CNTORDERNO,
    CNTRXNO
) VALUES ( 
    :WORKDATE,
    :CNTORDERNO,
    :CNTRXNO
)";
            return DBWork.Connection.Execute(sql, lst, DBWork.Transaction);
        } //

        public int Import(IList<ME_AB0075EModles> lst)
        {
            string sql = @"
INSERT INTO MMSADM.ME_AB0075E (
    WORKDATE,
    SUMORDERNO,
    SUMBEDNO
) VALUES ( 
    :WORKDATE,
    :RECNO,
    :SUMBEDNO
)";
            return DBWork.Connection.Execute(sql, lst, DBWork.Transaction);
        } //
        



    } // ec
} // en
