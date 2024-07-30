using Dapper;
using JCLib.DB;
using MMSMSREPORT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using Oracle.ManagedDataAccess.Client;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Types;

namespace DLLImport
{
    public class RepHISBACKRepository : JCLib.Mvc.BaseRepository
    {
        public RepHISBACKRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        /*
        public int DeleteAll()
        {
            string sql = "DELETE FROM MMSADM.HIS_BACK";
            return DBWork.Connection.Execute(sql, null, DBWork.Transaction);
        }
        */

        public int Import(IList<HISBACKModles> getPubDrugList)
        {
            string sql = @"INSERT INTO MMSADM.HIS_BACK (
MEDNO, CHINNAME, NRCODE, BEDNO, ORDERCODE, ORDERENGNAME, BEGINDATETIME, 
ENDDATETIME, DOSE, FREQNO, NEEDBACKQTY, BACKQTY, DIFF, PHRBACKREASON,
CREATEDATETIME, CREATEOPID, CHARTNO, STOCKUNIT, ORDERNO,
BACKNAME, ORDERUNIT, PROCDATETIME, BACKKIND, ORDERTYPE,
RETURNSTOCKCODE, PAYFLAG, BUYFLAG, ORDERSORT, USEDATETIME) 
VALUES ( 
:MEDNO, :CHINNAME, :NRCODE, :BEDNO, :ORDERCODE, :ORDERENGNAME, :BEGINDATETIME,
:ENDDATETIME, :DOSE, :FREQNO, :NEEDBACKQTY, :BACKQTY, :DIFF, :PHRBACKREASON,
:CREATEDATETIME, :CREATEOPID, :CHARTNO, :STOCKUNIT, :ORDERNO,
:BACKNAME, :ORDERUNIT, :PROCDATETIME, :BACKKIND, :ORDERTYPE,
:RETURNSTOCKCODE, :PAYFLAG, :BUYFLAG, :ORDERSORT, :USEDATETIME)";
            return DBWork.Connection.Execute(sql, getPubDrugList, DBWork.Transaction);
        }

        String sBr = "\r\n";
        public int Ins_to_ME_BACK(HISBACKModles v)
        {
            string sql = "";
            sql += "" + sBr;
            sql += "INSERT INTO ME_BACK (" + sBr;
            sql += "MEDNO,CHINNAME,NRCODE,BEDNO,ORDERCODE," + sBr;
            sql += "ORDERENGNAME,BEGINDATETIME,ENDDATETIME,DOSE,FREQNO," + sBr;
            sql += "NEEDBACKQTY,BACKQTY,DIFF,PHRBACKREASON,CREATEDATETIME," + sBr;
            sql += "CREATEOPID,CHARTNO,STOCKUNIT,ORDERNO," + sBr;
            sql += "BACKNAME,ORDERUNIT,RETURNSTOCKCODE,USEDATETIME," + sBr;
            sql += "PHRBACKREASON_NAME,PAYFLAG,BUYFLAG,ORDERSORT" + sBr;
            sql += ")" + sBr;
            sql += "SELECT " + sBr;
            sql += "MEDNO,CHINNAME,NRCODE,BEDNO,ORDERCODE," + sBr;
            sql += "ORDERENGNAME,BEGINDATETIME,ENDDATETIME,DOSE,FREQNO," + sBr;
            sql += "NEEDBACKQTY,BACKQTY,DIFF,PHRBACKREASON,CREATEDATETIME," + sBr;
            sql += "CREATEOPID,CHARTNO,STOCKUNIT,ORDERNO," + sBr;
            sql += "BACKNAME,ORDERUNIT,RETURNSTOCKCODE,USEDATETIME," + sBr;
            sql += "PHRBACKREASON_NAME,PAYFLAG,BUYFLAG,ORDERSORT" + sBr;
            sql += "FROM HIS_BACK" + sBr;
            sql += "WHERE 1=1" + sBr;
            sql += "AND CREATEDATETIME > :SCDT " + sBr;
            sql += "AND CREATEDATETIME < :ECDT " + sBr;
            //sql += "AND CREATEDATETIME > SCDT --'1081104120101'" + sBr;
            //sql += "AND CREATEDATETIME < ECDT --'1081104150101'" + sBr;
            return DBWork.Connection.Execute(sql, v, DBWork.Transaction);
        } // 

        public string exec_CREATE_RN_DOC()
        {
            var p = new OracleDynamicParameters();
            p.Add("O_RETID", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 32767);
            p.Add("O_ERRMSG", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 32767);

            DBWork.Connection.Query("CREATE_RN_DOC", p, commandType: CommandType.StoredProcedure);
            string RetId = "";
            if (p.Get<OracleString>("O_RETID") != null)
                RetId = p.Get<OracleString>("O_RETID").Value;
            string ErrMsg = "";
            if (p.Get<OracleString>("O_ERRMSG") != null)
                ErrMsg = p.Get<OracleString>("O_ERRMSG").Value;

            return RetId + "^" + ErrMsg;
        } // 

    }
}
