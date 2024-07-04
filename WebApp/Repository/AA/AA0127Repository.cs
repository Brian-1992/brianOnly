using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using System.Linq;
using TSGH.Models;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using JCLib.Mvc;

namespace WebApp.Repository.AA
{
   
    public class AA0127Repository : JCLib.Mvc.BaseRepository
    {
        public AA0127Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }


        public IEnumerable<ComboModel> GetDeptCombo()
        {
            string sql = @"SELECT DISTINCT DEPT as VALUE ,DEPT ||' '||INID_NAME(DEPT) as COMBITEM FROM PH_AIRHIS WHERE STATUS IN ('B','P')";

            return DBWork.Connection.Query<ComboModel>(sql);
        }


        public IEnumerable<PH_AIRHIS> GetAll(string bgdate, string endate, string dept, string status, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql ="SELECT TXTDAY, AGEN_NO|| ' ' || (SELECT NVL(AGEN_NAMEC, '') FROM PH_VENDER WHERE AGEN_NO = PH_AIRHIS.AGEN_NO) AGEN_NON,AGEN_NO,DEPT,MMCODE,FBNO,AIR,XSIZE,DEPT || ' ' || (SELECT NVL(WH_NAME, '') FROM MI_WHMAST WHERE WH_NO = DEPT) DEPTN,SBNO,DOCNO,STATUS,SEQ from PH_AIRHIS WHERE 1 = 1 ";


            if (bgdate != "null" && bgdate != "")
            {
                sql += " AND to_char(TXTDAY,'yyyy-mm-dd') >= Substr(:p1, 1, 10) ";
                p.Add(":p1", bgdate);
            }
            if (endate != "null" && endate != "")
            {
                sql += " AND to_char(TXTDAY,'yyyy-mm-dd') <= Substr(:p2, 1, 10) ";
                p.Add(":p2", endate);
            }

            if (dept != "" && dept != "null")
            {
                sql += " AND DEPT  = :p3 ";
                p.Add(":p3", dept);
            }

            if (status != "" && status != "null")
            {
                sql += " AND STATUS = :p4 ";
                p.Add(":p4", status);
            }

            //if (mmcode1 != "" && mmcode1 != "null")
            //{
            //    sql += " AND A.MMCODE <=   = :p2 ";
            //    p.Add(":p2", mmcode1);
            //}

           





            sql += " order by  MMCODE ";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<PH_AIRHIS>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }


        public int UpdateEnd(string txtday,string seq, string dept, string mmcode, string fbno, string UPUSER, string UIP)
        {

            var sql = @"UPDATE PH_AIRHIS SET STATUS='I', UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE SEQ=:seq and DEPT=:dept and MMCODE=:mmcode and FBNO=:fbno";
            //return DBWork.Connection.Execute(sql, record, DBWork.Transaction);
            int result1= DBWork.Connection.Execute(sql, new { seq= seq, dept= dept, mmcode= mmcode, fbno= fbno, UPDATE_USER = UPUSER, UPDATE_IP = UIP }, DBWork.Transaction);
            return result1;
        }


        public SP_MODEL UpdIncapv(string updusr, string updip)
        {
            var p = new OracleDynamicParameters();
            p.Add("I_USERID", value: updusr, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 5);
            p.Add("I_UPDIP", value: updip, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 20);
            p.Add("O_RETID", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 1);
            p.Add("O_ERRMSG", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 200);

            DBWork.Connection.Query("INV_SET.AIR_INCAPV", p, commandType: CommandType.StoredProcedure);
            string retid = p.Get<OracleString>("O_RETID").Value;
            string errmsg = p.Get<OracleString>("O_ERRMSG").Value;

            SP_MODEL sp = new SP_MODEL
            {
                O_RETID = p.Get<OracleString>("O_RETID").Value,
                O_ERRMSG = p.Get<OracleString>("O_ERRMSG").Value
            };
            return sp;
        }
       
        public IEnumerable<ME_DOCM> QueryDocno(string docno)
        //public IEnumerable<ME_DOCM> QueryDocno(string docno, int page_index, int page_size, string sorters)
        {
            DynamicParameters p = new DynamicParameters();

            string sql = @"select a.docno,a.appdept,a.apptime,a.frwh,a.towh,a.mat_class,b.mmcode,b.base_unit,b.inc_qty,b.inc_price,c.appqty,c.apvqty from ME_DOCM a,ME_DOCI b,ME_DOCD C 
                           where a.docno =b.docno and b.docno=c.docno and a.docno =:DOCNO ";

          
            return DBWork.Connection.Query<ME_DOCM>(sql, new { DOCNO = docno }, DBWork.Transaction);
            //return DBWork.Connection.Query<PRM>(GetPagingStatement(sql, sorters), p);
        }
    }
}