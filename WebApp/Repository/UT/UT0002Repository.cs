using Dapper;
using JCLib.DB;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using WebApp.Models;
using WebApp.Models.UT;

namespace WebApp.Repository.UT
{
    public class UT0002Repository : JCLib.Mvc.BaseRepository
    {
        public UT0002Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<UT_DOCM> GetAllM(string DOCNO, string doctype, string flowid, string tuser, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            StringBuilder sql = new StringBuilder();
            // sql.Append("SELECT B.SEQ,B.MMCODE,MMNAME_C,MMNAME_E,FLOWID,DOCTYPE,EXP_STATUS");
            // sql.Append(",(SELECT UI_CHANAME FROM MI_UNITCODE D WHERE C.BASE_UNIT=D.UNIT_CODE)BASE_UNIT");
            // sql.Append(",A.FRWH,(SELECT WH_NO||' '||WH_NAME from MI_WHMAST where WH_NO=A.FRWH) FRWH_N");
            // sql.Append(",(SELECT WH_NO||' '||WH_NAME from MI_WHMAST where WH_NO=A.TOWH) TOWH_N");
            // switch (doctype)
            // {
            //     case "MR":
            //         if (flowid == "0102")
            //             sql.Append(",APVQTY QTY");
            //         else
            //             sql.Append(",ACKQTY QTY");
            //         break;
            //     case "MR1":
            //     case "MR2":
            //     case "MR3":
            //     case "MR4":
            //         if (flowid == "2")
            //             sql.Append(",EXPT_DISTQTY QTY");
            //         else if (flowid == "5")
            //             sql.Append(",APVQTY QTY");
            //         else
            //             sql.Append(",ACKQTY QTY");
            //         break;
            //     case "MS":
            //         sql.Append(",ACKQTY QTY");
            //         break;
            //     case "TR":
            //     case "TR1":
            //         if (flowid == "0202")
            //             sql.Append(",APVQTY QTY");
            //         else
            //             sql.Append(",ACKQTY QTY");
            //         break;
            //     case "XR":
            //         sql.Append(",APVQTY QTY");
            //         break;
            //     default:
            //         sql.Append(",APPQTY QTY"); //調撥 TR,TR1
            //         break;
            // }
            // sql.Append(" FROM ME_DOCM A,ME_DOCD B,MI_MAST C");
            // sql.Append(" WHERE A.DOCNO=B.DOCNO AND B.MMCODE=C.MMCODE AND WEXP_ID='Y' AND A.DOCNO = :DOCNO");
            //// sql.Append(" AND DOCTYPE IN ('TR','TR1') AND APVQTY>0");
            // sql.Append(" AND EXISTS(SELECT 'X' FROM MI_WHID D WHERE D.WH_NO IN (A.TOWH,A.FRWH) AND WH_USERID=:WH_USERID)");
            // p.Add(":DOCNO", DOCNO);
            // p.Add(":WH_USERID", string.Format("{0}", tuser));
            sql.Append("SELECT SEQ,MMCODE,MMNAME_C,MMNAME_E,FLOWID,DOCTYPE,EXP_STATUS,BASE_UNIT,FRWH,FRWH_N,TOWH_N,QTY,EDIT_TYPE,IS_CLOSE,IS_ADD");
            sql.Append(" FROM V_ME_DOCD_E A");
            sql.Append(" WHERE A.DOCNO = :DOCNO");
            sql.Append(" AND (EXISTS(SELECT 'X' FROM MI_WHID D WHERE D.WH_NO IN (A.TOWH,A.FRWH) AND WH_USERID=:WH_USERID)");
            sql.Append("  OR (EXISTS(SELECT 'X' FROM UR_ID D,MI_WHMAST E WHERE D.INID=E.INID AND D.TUSER=:WH_USERID)))");
            if ((doctype == "MR3" | doctype == "MR4") & (flowid == "4" | flowid == "5")) {
                sql.Append(" AND QTY>0");
            }
            p.Add(":DOCNO", DOCNO);
            p.Add(":WH_USERID", tuser);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<UT_DOCM>(GetPagingStatement(sql.ToString(), sorters), p, DBWork.Transaction);
        }

        public IEnumerable<UT_DOCD> GetAllD(string FLOWID,string DOCTYPE, string DOCNO, string SEQ, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            string sql = "SELECT MMCODE,EXP_DATE,LOT_NO,APVQTY FROM ME_DOCEXP WHERE DOCNO=:DOCNO AND SEQ=:SEQ";
            if ((DOCTYPE == "MR3" | DOCTYPE == "MR4") & FLOWID!="6") //分批交貨
                sql +=" AND C_STATUS='N'";
            p.Add(":DOCNO", DOCNO);
            p.Add(":SEQ", SEQ);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<UT_DOCD>(GetPagingStatement(sql.ToString(), sorters), p, DBWork.Transaction);
        }

        public SP_MODEL WEXPINV_R(string I_DOCNO, Int16 I_SEQ, string I_UPDUSR, string I_UPDIP)
        {
            SP_MODEL sp;

            var p = new OracleDynamicParameters();
            p.Add("I_DOCNO", I_DOCNO, dbType: OracleDbType.Varchar2);
            p.Add("I_SEQ", I_SEQ, dbType: OracleDbType.Int16);
            p.Add("I_UPDUSR", I_UPDUSR, dbType: OracleDbType.Varchar2);
            p.Add("I_UPDIP", I_UPDIP, dbType: OracleDbType.Varchar2);
            p.Add("O_RETID", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 1);
            p.Add("O_ERRMSG", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 200);
            DBWork.Connection.Execute("SP_WEXPINV_R", p, DBWork.Transaction, commandType: CommandType.StoredProcedure);
            sp = new SP_MODEL
            {
                O_RETID = p.Get<OracleString>("O_RETID").Value,
                O_ERRMSG = p.Get<OracleString>("O_ERRMSG").Value
            };
            return sp;
        }
    }
}