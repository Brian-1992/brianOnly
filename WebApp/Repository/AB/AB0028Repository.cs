using System;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using System.Text;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;

namespace WebApp.Repository.AB
{
    public class AB0028Repository : JCLib.Mvc.BaseRepository
    {
        public AB0028Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ME_DOCM> GetAllM(string tuser, string DOCNO, string APPTIME_S, string APPTIME_E, string FRWH, string APPID, string TOWH,string FLOWID, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT DOCNO,TWN_DATE(A.APPTIME) APPTIME,A.APPID,UNA AS APP_NAME,INID_NAME AS APPDEPT");
            sql.Append(",A.FRWH,(SELECT WH_NO||' '||WH_NAME from MI_WHMAST where WH_NO=A.FRWH) FRWH_N");
            sql.Append(",A.TOWH,(SELECT WH_NO||' '||WH_NAME from MI_WHMAST where WH_NO=A.TOWH) TOWH_N");
            sql.Append(",A.FLOWID,case when A.FLOWID='0201' then '調撥申請' when A.FLOWID='0202' then '調出中'");
            sql.Append(" when A.FLOWID='0203' then '調入中' when A.FLOWID='0299' then '調撥結案' end FLOWID_N");
            sql.Append(",(SELECT 'Y' FROM ME_DOCD D WHERE A.DOCNO=D.DOCNO AND ROWNUM<=1 )LIST_ID");
            sql.Append(" FROM ME_DOCM A,UR_ID B,UR_INID C");
            sql.Append(" WHERE DOCTYPE ='TR' AND A.APPID=B.TUSER AND A.APPDEPT=C.INID");
            sql.Append(" AND EXISTS(SELECT 'X' FROM MI_WHID E WHERE WH_USERID=:WH_USERID AND (E.WH_NO=A.FRWH OR E.WH_NO=A.TOWH))");
            sql.Append(" AND ( EXISTS(SELECT 'X' FROM MI_WHMAST E WHERE A.FRWH=E.WH_NO AND WH_KIND='0' AND WH_GRADE='2')");
            sql.Append(" OR  EXISTS(SELECT 'X' FROM MI_WHMAST E WHERE A.TOWH=E.WH_NO AND WH_KIND='0' AND WH_GRADE='2'))");

            p.Add(":WH_USERID", string.Format("{0}", tuser));
            if (DOCNO != "")
            {
                sql.Append(" AND A.DOCNO LIKE :DOCNO");
                p.Add(":DOCNO", string.Format("{0}%", DOCNO));
            }
            if (APPTIME_S != "" & APPTIME_E != "") {
                sql.Append(" AND A.APPTIME BETWEEN TO_DATE(:APPTIME_S,'YYYYMMDD') AND TO_DATE(:APPTIME_E||'235959','YYYYMMDDHH24MISS') ");
                p.Add(":APPTIME_S", string.Format("{0}", APPTIME_S));
                p.Add(":APPTIME_E", string.Format("{0}", APPTIME_E));
            }
            else { 
                if (APPTIME_S != "")
                {
                    sql.Append(" AND A.APPTIME >= TO_DATE(:APPTIME_S,'YYYYMMDD') ");
                    p.Add(":APPTIME_S", string.Format("{0}", APPTIME_S));
                }
                if (APPTIME_E != "")
                {
                    sql.Append(" AND A.APPTIME <= TO_DATE(:APPTIME_E||'235959','YYYYMMDDHH24MISS') ");
                    p.Add(":APPTIME_E", string.Format("{0}", APPTIME_E));
                }
            }
            if (FRWH != "")
            {
                sql.Append(" AND A.FRWH = :FRWH ");
                p.Add(":FRWH", string.Format("{0}", FRWH));
            }
            if (APPID != "")
            {
                sql.Append(" AND B.UNA LIKE :APPID ");
                p.Add(":APPID", string.Format("{0}%", APPID));
            }
            if (TOWH != "")
            {
                sql.Append(" AND A.TOWH = :TOWH ");
                p.Add(":TOWH", string.Format("{0}", TOWH));
            }
            if (FLOWID != "")
            {
                string param = "";
                string[] FLOWIDList = FLOWID.Split(',');
                for (int i = 0; i < FLOWIDList.Length; i++)
                {
                    if (i >= 1) param += ",";
                    param += ":FLOWID_" + i;

                    p.Add(":FLOWID_" + i, FLOWIDList[i]);
                }

                sql.Append(" AND A.FLOWID in (" + param + ")");
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<ME_DOCM>(GetPagingStatement(sql.ToString(), sorters), p, DBWork.Transaction);
        }

        public bool CheckWh(string id,string frwh, string towh) {
            string sql = "SELECT 1 FROM MI_WHID WHERE WH_USERID=:WH_USERID AND WH_NO IN (:FRWH,:TOWH)";
            return !(DBWork.Connection.ExecuteScalar(sql, new { WH_USERID =id,FRWH = frwh, TOWH = towh }, DBWork.Transaction) == null);
        }
        public int DeleteMbyNoDetail(string id) {
            StringBuilder sql = new StringBuilder();
            sql.Append("DELETE FROM ME_DOCM A");
            sql.Append(" WHERE DOCTYPE='TR'");
            sql.Append(" AND APPID=:APPID");
            sql.Append(" AND NOT EXISTS(SELECT 'X' FROM ME_DOCD B WHERE A.DOCNO=B.DOCNO)");
           // sql.Append(" AND (EXISTS(SELECT 'X' FROM MI_WHMAST B WHERE A.FRWH=B.WH_NO AND WH_KIND='0' AND WH_GRADE='2')");
           // sql.Append(" OR EXISTS(SELECT 'X' FROM MI_WHMAST B WHERE A.TOWH=B.WH_NO AND WH_KIND='0' AND WH_GRADE='2'))");
            
            return DBWork.Connection.Execute(sql.ToString(), new { APPID = id }, DBWork.Transaction);
        }
        public string GetDocno()
        {
            var p = new OracleDynamicParameters();
            p.Add("O_DOCNO",dbType: OracleDbType.Varchar2,direction: ParameterDirection.Output,size: 12);

            DBWork.Connection.Query("GET_DOCNO", p, commandType: CommandType.StoredProcedure);
            return p.Get<OracleString>("O_DOCNO").Value;
        }
        public SP_MODEL POST_DOC(string I_DOCNO,string I_UPDUSR,string I_UPDIP)
        {
            SP_MODEL sp;
            var p = new OracleDynamicParameters();
            p.Add("I_DOCNO", I_DOCNO, dbType: OracleDbType.Varchar2);
            p.Add("I_UPDUSR", I_UPDUSR, dbType: OracleDbType.Varchar2);
            p.Add("I_UPDIP", I_UPDIP,dbType: OracleDbType.Varchar2);
            p.Add("O_RETID", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 1);
            p.Add("O_ERRMSG", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 200);
            DBWork.Connection.Execute("POST_DOC", p, DBWork.Transaction, commandType: CommandType.StoredProcedure);
            sp = new SP_MODEL
            {
                O_RETID = p.Get<OracleString>("O_RETID").Value,
                O_ERRMSG = p.Get<OracleString>("O_ERRMSG").Value
            };
            return sp;
        }
        public int CreateM(ME_DOCM ME_DOCM)
        {
            var sql = @"INSERT INTO ME_DOCM (
                        DOCNO, DOCTYPE, FLOWID , APPID , APPDEPT , 
                        APPTIME   , FRWH , TOWH , MAT_CLASS, 
                        CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                      VALUES( 
                        :DOCNO, :DOCTYPE, :FLOWID , :APPID , :APPDEPT , 
                        SYSDATE , :FRWH , :TOWH , :MAT_CLASS, 
                        SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP ) ";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);
        }
        public bool Check0201(string pDocno)
        {
            string sql = @"SELECT 1 FROM ME_DOCM WHERE DOCNO=:DOCNO AND FLOWID='0201'";
            return (DBWork.Connection.ExecuteScalar(sql, new { DOCNO = pDocno }, DBWork.Transaction) == null);
        }
        public IEnumerable<ME_DOCM> GetM(string id)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT DOCNO,TWN_DATE(A.APPTIME) APPTIME,A.APPID,UNA AS APP_NAME,INID_NAME AS APPDEPT");
            sql.Append(",A.FRWH,(SELECT WH_NO||' '||WH_NAME from MI_WHMAST where WH_NO=A.FRWH) FRWH_N");
            sql.Append(",A.TOWH,(SELECT WH_NO||' '||WH_NAME from MI_WHMAST where WH_NO=A.TOWH) TOWH_N");
            sql.Append(",A.FLOWID,'調撥申請' FLOWID_N");
            sql.Append(",(SELECT 'Y' FROM ME_DOCD D WHERE A.DOCNO=D.DOCNO AND ROWNUM<=1 )LIST_ID");
            sql.Append(" FROM ME_DOCM A,UR_ID B,UR_INID C");
            sql.Append(" WHERE A.DOCNO = :DOCNO AND A.APPID=B.TUSER AND A.APPDEPT=C.INID");
            return DBWork.Connection.Query<ME_DOCM>(sql.ToString(), new { DOCNO = id }, DBWork.Transaction);
        }
        public int DeleteM(string docno)
        {
            var sql = @"DELETE ME_DOCM WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno }, DBWork.Transaction);
        }
        public int UpdateFLOWID(ME_DOCM ME_DOCM)
        {
            var sql = @"UPDATE ME_DOCM SET 
                        FLOWID = :FLOWID,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);
        }
        public int UpdateALLApvQty(ME_DOCM pME_DOCM)
        {
            var sql = @"UPDATE ME_DOCD SET 
                        APVQTY = APPQTY,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO ";
            return DBWork.Connection.Execute(sql, pME_DOCM, DBWork.Transaction);
        }
        
        public int UpdateM(ME_DOCM ME_DOCM)
        {//                        TOWH = :TOWH,
            var sql = @"UPDATE ME_DOCM SET 
                        FRWH = :FRWH,TOWH = :TOWH,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);
        }

        public bool CheckExistsDKeyByUpd(string id, string mmcode, string seq)
        {
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO AND MMCODE=:MMCODE AND SEQ<>:SEQ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id, MMCODE = mmcode, SEQ = seq }, DBWork.Transaction) == null);
        }
        public bool CheckExistsMast(string docno, string mmcode)
        {
            string sql = @" SELECT 1 from MI_WHINV A,MI_MAST B
                        WHERE B.MMCODE = :MMCODE
                        AND WH_NO=(select FRWH from ME_DOCM where DOCNO = :DOCNO) AND A.MMCODE=B.MMCODE AND B.MAT_CLASS='01' AND B.E_ORDERDCFLAG='N'
                        AND EXISTS (SELECT 'X' FROM MI_WHMAST C WHERE C.WH_NO=A.WH_NO AND C.WH_GRADE='2' AND WH_KIND='0') ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = docno, MMCODE = mmcode }, DBWork.Transaction) == null);
        }
        public bool CheckExistsDKey(string id, string mmcode)
        {
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO AND MMCODE=:MMCODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id, MMCODE = mmcode }, DBWork.Transaction) == null);
        }
        public int DeleteAllD(string docno)
        {
            var sql = @"DELETE ME_DOCD WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno } , DBWork.Transaction);
        }
        public int DeleteD(string docno,string seq)
        {
            var sql = @"DELETE ME_DOCD WHERE DOCNO = :DOCNO AND SEQ = :SEQ";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno, SEQ = seq }, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetTowhCombo(string wh_no)
        {
            DynamicParameters p = new DynamicParameters();
            StringBuilder sql = new StringBuilder();
            sql.Append("select A.WH_NO VALUE , WH_NO||' '||WH_NAME TEXT from MI_WHMAST A");
            sql.Append(" WHERE WH_KIND='0' AND WH_GRADE='2'");
           // sql.Append(" AND WH_NO<>:WH_NO");
            sql.Append(" ORDER BY 1");
          //  p.Add(":WH_NO", string.Format("{0}", wh_no));
            return DBWork.Connection.Query<COMBO_MODEL>(sql.ToString(),p);
        }
        public IEnumerable<COMBO_MODEL> GetFrwhCombo(string id)
        {
            DynamicParameters p = new DynamicParameters();
            StringBuilder sql = new StringBuilder();
            sql.Append("select WH_NO VALUE,WH_NO||' '||WH_NAME TEXT from MI_WHMAST A");
            sql.Append(" WHERE WH_KIND='0' AND WH_GRADE='2'");
            sql.Append(" AND EXISTS(SELECT 'X' FROM MI_WHID B WHERE A.WH_NO=B.WH_NO AND WH_USERID=:WH_USERID)");
            sql.Append("  ORDER BY 2");
            p.Add(":WH_USERID", string.Format("{0}", id));
            return DBWork.Connection.Query<COMBO_MODEL>(sql.ToString(),p);
        }

        public IEnumerable<MI_MAST> GetMmcodeCombo(string p0,string wh_no, int page_index, int page_size, string sorters)
        {
            DynamicParameters p = new DynamicParameters();
            StringBuilder sql = new StringBuilder();

            sql.Append("select A.MMCODE,MMNAME_C,MMNAME_E ");
            sql.Append(",(SELECT UI_CHANAME FROM MI_UNITCODE D WHERE B.BASE_UNIT=D.UNIT_CODE) BASE_UNIT ");
            sql.Append(" from MI_WHINV A,MI_MAST B ");
            sql.Append(" where wh_no=:wh_no AND A.MMCODE=B.MMCODE AND B.MAT_CLASS='01' AND B.E_ORDERDCFLAG='N' ");
            sql.Append(" and (A.MMCODE LIKE :MMCODE or MMNAME_C LIKE :MMNAME_C or UPPER(MMNAME_E) LIKE :MMNAME_E ) ");
            sql.Append(" AND EXISTS(SELECT 'X' FROM MI_WHMAST C WHERE C.WH_NO=A.WH_NO AND C.WH_GRADE='2' AND WH_KIND='0') ");
            sql.Append(" and A.MMCODE not like '004%' ");
            sql.Append(" ORDER BY A.MMCODE ");

            p.Add(":wh_no", string.Format("{0}", wh_no));
            p.Add(":MMCODE", string.Format("{0}%", p0));
            p.Add(":MMNAME_C", string.Format("%{0}%", p0));
            p.Add(":MMNAME_E", string.Format("%{0}%", p0.ToUpper()));

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql.ToString(), sorters), p, DBWork.Transaction);
        }

        public string GetDocDSeq(string id)
        {
            string sql = @"SELECT NVL(MAX(SEQ),0)+1 as SEQ FROM ME_DOCD WHERE DOCNO=:DOCNO ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction).ToString();
            return rtn;
        }

        public IEnumerable<ME_DOCD> GetAllD(string DOCNO, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.DOCNO,A.SEQ,A.MMCODE,A.APPQTY,APVQTY,ACKQTY,TWN_DATE( APVTIME)APVTIME,TWN_DATE(ACKTIME) ACKTIME,B.MMNAME_C
                        ,B.MMNAME_E, (SELECT UI_CHANAME FROM MI_UNITCODE D WHERE B.BASE_UNIT=D.UNIT_CODE) BASE_UNIT
                        FROM ME_DOCD A,MI_MAST B, ME_DOCM C WHERE C.DOCNO=A.DOCNO AND A.MMCODE = B.MMCODE AND A.DOCNO = :p0";

            p.Add(":p0", string.Format("{0}", DOCNO));
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCD>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public int CreateD(ME_DOCD ME_DOCD)
        {
            var sql = @"INSERT INTO ME_DOCD (
                        DOCNO, SEQ, MMCODE , APPQTY ,CREATE_TIME,
                        CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                      VALUES (
                        :DOCNO, :SEQ, :MMCODE , :APPQTY , SYSDATE,
                        :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, ME_DOCD, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCD> GetD(string id,string seq)
        {
            var sql = @"SELECT A.DOCNO,A.SEQ,A.MMCODE,A.APPQTY,APVQTY,ACKQTY,TWN_DATE(APVTIME)APVTIME,TWN_DATE(ACKTIME)ACKTIME,B.MMNAME_C,
                        B.MMNAME_E, (SELECT UI_CHANAME FROM MI_UNITCODE D WHERE B.BASE_UNIT=D.UNIT_CODE) BASE_UNIT
                         FROM ME_DOCD A,MI_MAST B, ME_DOCM C WHERE C.DOCNO=A.DOCNO AND A.MMCODE = B.MMCODE
                        AND A.DOCNO = :DOCNO AND A.SEQ=:SEQ";
            return DBWork.Connection.Query<ME_DOCD>(sql, new { DOCNO = id ,SEQ = seq}, DBWork.Transaction);
        }

        public int UpdateD(ME_DOCD pME_DOCD)
        {
            var sql = @"UPDATE ME_DOCD SET 
                        MMCODE = :MMCODE, APPQTY = :APPQTY,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO AND SEQ = :SEQ ";
            return DBWork.Connection.Execute(sql, pME_DOCD, DBWork.Transaction);
        }

        public int UpdateDApvQty(ME_DOCD pME_DOCD)
        {
            var sql = @"UPDATE ME_DOCD SET 
                        APVQTY = APPQTY,APVID=:APVID,APVTIME = SYSDATE,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO AND SEQ = :SEQ";
            return DBWork.Connection.Execute(sql, pME_DOCD, DBWork.Transaction);
        }
        public int UpdateDAckQty(ME_DOCD pME_DOCD)
        {
            var sql = @"UPDATE ME_DOCD SET 
                        ACKQTY = :ACKQTY,ACKTIME = SYSDATE,ACKID=:ACKID,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO AND SEQ = :SEQ ";
            return DBWork.Connection.Execute(sql, pME_DOCD, DBWork.Transaction);
        }

        #region 2022-01-05
        public bool CheckMmcodeValid(string wh_no, string mmcode) {
            string sql = @"
                select 1 
                  from MI_WHINV A,MI_MAST B
                 where wh_no=:wh_no 
                   AND A.MMCODE=B.MMCODE 
                   and b.mmcode = :mmcode                   
                   AND B.MAT_CLASS='01' AND B.E_ORDERDCFLAG='N'
                   AND EXISTS(SELECT 'X' FROM MI_WHMAST C WHERE C.WH_NO=A.WH_NO AND C.WH_GRADE='2' AND WH_KIND='0')
                   and b.mmcode not like '004%'
            ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { mmcode, wh_no }, DBWork.Transaction) == null);
        }
        /*
         sql.Append("select A.MMCODE,MMNAME_C,MMNAME_E ");
            sql.Append(",(SELECT UI_CHANAME FROM MI_UNITCODE D WHERE B.BASE_UNIT=D.UNIT_CODE) BASE_UNIT ");
            sql.Append(" from MI_WHINV A,MI_MAST B ");
            sql.Append(" where wh_no=:wh_no AND A.MMCODE=B.MMCODE AND B.MAT_CLASS='01' AND B.E_ORDERDCFLAG='N' ");
            sql.Append(" and (A.MMCODE LIKE :MMCODE or MMNAME_C LIKE :MMNAME_C or UPPER(MMNAME_E) LIKE :MMNAME_E ) ");
            sql.Append(" AND EXISTS(SELECT 'X' FROM MI_WHMAST C WHERE C.WH_NO=A.WH_NO AND C.WH_GRADE='2' AND WH_KIND='0') ");
            sql.Append(" and A.MMCODE not like '004%' ");
            sql.Append(" ORDER BY A.MMCODE ");
         */

        #endregion
    }
}