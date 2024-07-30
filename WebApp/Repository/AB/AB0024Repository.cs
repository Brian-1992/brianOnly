using Dapper;
using JCLib.DB;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Repository.AB
{
    public class AB0024Repository : JCLib.Mvc.BaseRepository
    {

        public AB0024Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ME_DOCM> GetAll(ME_DOCM_QUERY_PARAMS query, string userid)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT DOCNO, TWN_DATE(APPTIME) AS APPTIME
                    , APPID || ' ' || USER_NAME(APPID) AS APP_NAME
                    , USER_NAME(APPID) AS APPID 
                    , APPDEPT || ' ' || INID_NAME(APPDEPT) AS APPDEPT_NAME
                    , (SELECT FLOWID || ' ' || FLOWNAME FROM ME_FLOW WHERE FLOWID=A.FLOWID) FLOWID
                    , USEDEPT || ' ' || WH_NAME(USEDEPT) AS USEDEPT_NAME
                    , FRWH || ' ' || WH_NAME(FRWH) AS FRWH_NAME
                    , TOWH || ' ' || WH_NAME(TOWH) AS TOWH_NAME
                    , (SELECT 'Y' FROM ME_DOCD D,MI_MAST E WHERE A.DOCNO=D.DOCNO AND E.MMCODE=D.MMCODE AND WEXP_ID='Y' AND ROWNUM<=1 ) LIST_ID
                    FROM ME_DOCM a 
                    WHERE 1=1 
                      and exists ( select 1 from mi_whid b where a.towh = b.wh_no and b.wh_userid = :userid )";
            p.Add(":userid", string.Format("{0}", userid));

            if (query.DOCNO != "")
            {
                sql += " AND a.DOCNO LIKE :DOCNO ";
                p.Add(":DOCNO", string.Format("%{0}%", query.DOCNO));
            }

            if (query.APPID != "")
            {
                //sql += " AND a.APPID LIKE :APPID ";
                //p.Add(":APPID", string.Format("%{0}%", query.APPID));
                sql += " AND (a.APPID = user_id(:APPID) or :APPID is null ) ";
                p.Add(":APPID", string.Format("{0}", query.APPID));
            }
            if (query.FROMDATE != "") {
                sql += " AND TRUNC(a.APPTIME, 'DD') >= TO_DATE(:fromdate, 'YYYY-MM-DD') ";
                p.Add(":fromdate", string.Format("{0}", query.FROMDATE));
            }
            if (query.TODATE != "")
            {
                sql += " AND TRUNC(a.APPTIME, 'DD') <= TO_DATE(:todate, 'YYYY-MM-DD') ";
                p.Add(":todate", string.Format("{0}", query.TODATE));
            }

            if (query.APPDEPT != "")
            {
                sql += " AND a.APPDEPT LIKE :APPDEPT ";
                p.Add(":APPDEPT", string.Format("%{0}%", query.APPDEPT));
            }

            if (query.DOCTYPE != "")
            {
                sql += " AND a.DOCTYPE = :DOCTYPE ";
                p.Add(":DOCTYPE", string.Format("{0}", query.DOCTYPE));
            }

            if (query.FLOWID != "")
            {
                string[] tmp = query.FLOWID.Split(',');
                sql += " AND A.FLOWID IN :FLOWID";
                p.Add(":FLOWID", tmp);
            }

            if (!string.IsNullOrEmpty(query.USEDEPT))
            {
                sql += " AND a.USEDEPT LIKE :USEDEPT ";
                p.Add(":USEDEPT", string.Format("%{0}%", query.USEDEPT));
            }

            if (! string.IsNullOrEmpty(query.FRWH))
            {
                sql += " AND a.FRWH = :FRWH ";
                p.Add(":FRWH", string.Format("{0}", query.FRWH));
            }

            if (!string.IsNullOrEmpty(query.TOWH))
            {
                sql += " AND a.TOWH = :TOWH ";
                p.Add(":TOWH", string.Format("{0}", query.TOWH));
            }

            return DBWork.PagingQuery<ME_DOCM>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCD> GetAllMeDocd(ME_DOCD_QUERY_PARAMS query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT a.DOCNO, a.SEQ, a.MMCODE, 
                        TRIM(TO_CHAR(a.APPQTY, '99999999990.999')) as APPQTY
                        , CASE WHEN a.APVQTY=0 THEN a.APPQTY ELSE a.APVQTY END AS APVQTY
                        , a.APL_CONTIME, a.APLYITEM_NOTE, b.MMNAME_C, b.MMNAME_E, b.M_CONTPRICE, b.BASE_UNIT
                        , (select inv_qty((select towh from ME_DOCM where docno = a.docno), a.mmcode) from dual) as towh_store_qty
                        FROM ME_DOCD a LEFT JOIN MI_MAST b ON a.MMCODE=b.MMCODE 
                        WHERE 1=1 ";

            if (query.DOCNO != "")
            {
                sql += " AND a.DOCNO LIKE :p0 ";
                p.Add(":p0", string.Format("%{0}%", query.DOCNO));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCD>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public List<MI_WHID> GetWhnoById(string userId)
        {
            var sql = @"SELECT WH_NO FROM MI_WHID WHERE WH_USERID=:WH_USERID";
            return DBWork.Connection.Query<MI_WHID>(sql, new { WH_USERID = userId }, DBWork.Transaction).ToList();
        }

        public int UpdateMeDocd(ME_DOCD me_docd)
        {
            var sql = @"UPDATE ME_DOCD SET APVQTY = :APVQTY, 
                                UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE DOCNO = :DOCNO AND SEQ = :SEQ";
            return DBWork.Connection.Execute(sql, me_docd, DBWork.Transaction);
        }

        public int UpdateApvqty(ME_DOCD me_docd)
        {
            var sql = @"UPDATE ME_DOCD SET APVQTY = APPQTY, 
                                UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE DOCNO = :DOCNO AND APVQTY = 0";
            return DBWork.Connection.Execute(sql, me_docd, DBWork.Transaction);
        }

        public int UpdateStatus(ME_DOCM me_docm)
        {
            var sql = @"UPDATE ME_DOCM SET FLOWID = :FLOWID, 
                                UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);
        }

        public SP_MODEL PostDoc(string docno, string updusr, string updip)
        {
            var p = new OracleDynamicParameters();
            p.Add("I_DOCNO", value: docno, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 21);
            p.Add("I_UPDUSR", value: updusr, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 8);
            p.Add("I_UPDIP", value: updip, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 20);

            p.Add("O_RETID", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 1);
            p.Add("O_ERRMSG", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 200);

            DBWork.Connection.Query("POST_DOC", p, commandType: CommandType.StoredProcedure);
            string retid = p.Get<OracleString>("O_RETID").Value;
            string errmsg = p.Get<OracleString>("O_ERRMSG").Value;

            SP_MODEL sp = new SP_MODEL
            {
                O_RETID = retid,
                O_ERRMSG = errmsg
            };
            return sp;
        }

        public class ME_DOCM_QUERY_PARAMS
        {
            public string DOCNO;
            public string DOCTYPE;
            public string APPID;
            public string APPDEPT;
            public string USEDEPT;
            public string FLOWID;
            public string FRWH;
            public string TOWH;
            public string FROMDATE;
            public string TODATE;
        }

        public class ME_DOCD_QUERY_PARAMS
        {
            public string DOCNO;
        }


        public IEnumerable<COMBO_MODEL> GetInidComboQ()
        {

            var sql = @" SELECT INID AS VALUE,INID_NAME AS TEXT,INID || ' ' ||INID_NAME AS COMBITEM
                        FROM UR_INID WHERE 1 = 1 ORDER BY INID ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<COMBO_MODEL> GetTowhComboQ(string userId)
        {

            var sql = @" SELECT WH_NO AS VALUE,WH_NAME(WH_NO)AS TEXT,WH_NO || ' ' ||WH_NAME(WH_NO) AS COMBITEM
                        FROM MI_WHID WHERE WH_USERID = :WH_USERID AND TASK_ID='1' ORDER BY WH_NO ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { WH_USERID = userId }, DBWork.Transaction);
        }
        public IEnumerable<COMBO_MODEL> GetFrwhComboQ()
        {
            string sql = @"SELECT DISTINCT WH_NO as VALUE, WH_NAME as TEXT,
                        WH_NO || ' ' || WH_NAME as COMBITEM 
                        FROM MI_WHMAST 
                        WHERE 1=1 AND WH_KIND = '0'
                        AND WH_GRADE > '1'  
                        AND WH_GRADE < '5'  
                        ORDER BY WH_NO";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }


    }
}