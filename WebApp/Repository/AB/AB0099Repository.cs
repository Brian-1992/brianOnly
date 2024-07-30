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
    public class AB0099Repository : JCLib.Mvc.BaseRepository
    {

        public AB0099Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ME_DOCM> GetAll(ME_DOCM_QUERY_PARAMS query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"    SELECT 
                                DOCNO, 
                                FLOWID,
                                TWN_DATE(APPTIME) AS APPTIME,       
                                USEDEPT || ' ' || WH_NAME(USEDEPT) AS USEDEPT_NAME,
                                FRWH || ' ' || WH_NAME(FRWH) AS FRWH_NAME,
                                TOWH || ' ' || WH_NAME(TOWH) AS TOWH_NAME,
                                APPLY_NOTE
                            FROM 
                                ME_DOCM A
                            WHERE 1=1 ";

            if (!string.IsNullOrEmpty(query.DOCTYPE))
            {
                sql += " AND A.DOCTYPE = :DOCTYPE ";
                p.Add(":DOCTYPE", string.Format("{0}", query.DOCTYPE));
            }

            if (!string.IsNullOrEmpty(query.DOCNO))
            {
                sql += " AND A.DOCNO = :DOCNO ";
                p.Add(":DOCNO", string.Format("{0}", query.DOCNO));
            }

            if (!string.IsNullOrEmpty(query.APPTIME_B))
            {
                sql += " AND TWN_DATE(APPTIME) >= :APPTIME_B ";
                p.Add(":APPTIME_B", string.Format("{0}", query.APPTIME_B));
            }

            if (!string.IsNullOrEmpty(query.APPTIME_E))
            {
                sql += " AND TWN_DATE(APPTIME) <= :APPTIME_E ";
                p.Add(":APPTIME_E", string.Format("{0}", query.APPTIME_E));
            }

            //if (!string.IsNullOrEmpty(query.FRWH))
            //{
            //    sql += " AND A.FRWH = :FRWH ";
            //    p.Add(":FRWH", string.Format("{0}", query.FRWH));
            //}

            if (!string.IsNullOrEmpty(query.FRWH))
            {
                sql += " AND a.FRWH IN (" + query.FRWH + ")";
                p.Add(":FRWH", string.Format("{0}", query.FRWH));
            }
            //if (!string.IsNullOrEmpty(query.TOWH))
            //{
            //    sql += " AND a.TOWH IN (" + query.TOWH + ")";
            //    p.Add(":TOWH", string.Format("{0}", query.TOWH));
            //}
            if (!string.IsNullOrEmpty(query.TOWH))
            {
                sql += " AND A.TOWH = :TOWH ";
                p.Add(":TOWH", string.Format("{0}", query.TOWH));
            }
            if (!string.IsNullOrEmpty(query.FLOWID))
            {
                string[] tmp = query.FLOWID.Split(',');
                sql += " AND A.FLOWID IN :FLOWID";
                p.Add(":FLOWID", tmp);
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCM>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCD> GetAllMeDocd(ME_DOCD_QUERY_PARAMS query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT a.DOCNO, a.SEQ, a.MMCODE, 
                        a.APPQTY as APPQTY
                        , CASE WHEN a.APVQTY=0 THEN a.APPQTY ELSE a.APVQTY END AS APVQTY
                        , a.APL_CONTIME, a.APLYITEM_NOTE, b.MMNAME_C, b.MMNAME_E, b.M_CONTPRICE, b.BASE_UNIT, a.STAT, a.RSEQ
                        FROM ME_DOCD a LEFT JOIN MI_MAST b ON a.MMCODE=b.MMCODE WHERE 1=1 ";

            if (!string.IsNullOrEmpty(query.DOCNO))
            {
                sql += " AND a.DOCNO = :p0 ";
                p.Add(":p0", string.Format("{0}", query.DOCNO));
            }

            if (!string.IsNullOrEmpty(query.STAT))
            {
                string[] tmp = query.STAT.Split(',');
                sql += " AND A.STAT IN :STAT";
                p.Add(":STAT", tmp);
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCD>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCD> DetailGet(ME_DOCD query)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT a.DOCNO, a.SEQ, a.MMCODE, 
                        TRIM(TO_CHAR(a.APPQTY, '99999999990.999')) as APPQTY
                        , CASE WHEN a.APVQTY=0 THEN a.APPQTY ELSE a.APVQTY END AS APVQTY
                        , a.APL_CONTIME, a.APLYITEM_NOTE, b.MMNAME_C, b.MMNAME_E, b.M_CONTPRICE, b.BASE_UNIT, a.STAT, a.RSEQ
                        FROM ME_DOCD a LEFT JOIN MI_MAST b ON a.MMCODE=b.MMCODE WHERE 1=1 ";

            if (!string.IsNullOrEmpty(query.DOCNO))
            {
                sql += " AND a.DOCNO = :p0 ";
                p.Add(":p0", string.Format("{0}", query.DOCNO));
            }

            if (!string.IsNullOrEmpty(query.STAT))
            {
                string[] tmp = query.STAT.Split(',');
                sql += " AND A.STAT IN :STAT";
                p.Add(":STAT", tmp);
            }

            return DBWork.Connection.Query<ME_DOCD>(sql, p, DBWork.Transaction);
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

        public int UpdateM(ME_DOCM me_docm)
        {
            var sql = @"UPDATE ME_DOCM SET FRWH = :FRWH, TOWH = :TOWH, APPLY_NOTE = :APPLY_NOTE, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);
        }

        public int UpdateD(ME_DOCD me_docd)
        {
            var sql = @"UPDATE ME_DOCD SET APPQTY = :APPQTY, APLYITEM_NOTE = :APLYITEM_NOTE, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE DOCNO = :DOCNO AND MMCODE = :MMCODE";
            return DBWork.Connection.Execute(sql, me_docd, DBWork.Transaction);
        }

        public int AcceptD(ME_DOCD me_docd)
        {
            var sql = @"UPDATE ME_DOCD SET STAT = 'A', UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE DOCNO = :DOCNO AND MMCODE = :MMCODE";
            return DBWork.Connection.Execute(sql, me_docd, DBWork.Transaction);
        }

        public int ReturnD(ME_DOCD me_docd)
        {
            var sql = @"UPDATE ME_DOCD SET STAT = 'B', UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE DOCNO = :DOCNO AND MMCODE = :MMCODE";
            return DBWork.Connection.Execute(sql, me_docd, DBWork.Transaction);
        }
        public IEnumerable<ME_BACK> GetMeBack(string docno, string rseq)
        {
            //string sql = @"SELECT a.DOCNO AS DOCNO, 
            //                      a.SEQ AS SEQ,
            //                      a.EXPDATE AS EXPDATE, 
            //                      a.EXPDATE AS ORI_EXPDATE,
            //                      a.MMCODE AS MMCODE, 
            //                      a.APVQTY AS APVQTY, 
            //                      TWN_DATE(a.APVTIME) AS APVTIME, 
            //                      b.WARNYM AS WARNYM,
            //                      c.TUSER || ' ' || c.UNA AS UPDATE_USER_NAME
            //                 FROM ME_DOCE a
            //                 LEFT JOIN ME_EXPM b on (b.mmcode = a.mmcode and b.exp_date = a.expdate)
            //                 LEFT JOIN UR_ID c on(c.TUSER = a.UPDATE_USER)
            //                WHERE DOCNO = :DOCNO AND SEQ = :SEQ";

            string sql = @"SELECT
                                a.RDOCNO AS RDOCNO,
                                a.NRCODE AS NRCODE,
                                a.BEDNO AS BEDNO,
                                a.MEDNO AS MEDNO,
                                a.CHINNAME AS CHINNAME,
                                a.ORDERCODE AS ORDERCODE,
                                (SELECT MMNAME_E FROM MI_MAST WHERE MMCODE = a.ORDERCODE) AS MMNAME,
                                a.BEGINDATETIME AS BEGINDATETIME,
	                            a.ENDDATETIME AS ENDDATETIME,
	                            a.DOSE AS DOSE,
	                            a.FREQNO AS FREQNO,
	                            a.NEEDBACKQTY AS NEEDBACKQTY,
	                            a.BACKQTY AS BACKQTY,
	                            a.DIFF AS DIFF,
	                            a.PHRBACKREASON_NAME AS PHRBACKREASON_NAME,
	                            a.CREATEDATETIME AS CREATEDATETIME
                            FROM 
                                ME_BACK a, ME_DOCD b
                            WHERE 1 = 1 
                                AND a.RDOCNO = b.DOCNO
                                AND a.RDOCNO = :RDOCNO 
                                AND a.RSEQ   = b.SEQ 
                                AND b.SEQ = :SEQ ";

            return DBWork.Connection.Query<ME_BACK>(sql, new { RDOCNO = docno, SEQ = rseq }, DBWork.Transaction);
        }

        public class ME_DOCM_QUERY_PARAMS
        {
            public string DOCNO;
            public string DOCTYPE;
            public string FLOWID;
            public string APPID;
            public string APPDEPT;
            public string APPTIME;
            public string APPTIME_B;
            public string APPTIME_E;
            public string USEID;
            public string USEDEPT;
            public string FRWH;
            public string TOWH;
            public string LIST_ID;
            public string APPLY_KIND;
            public string MAT_CLASS;
            public string JCN;
            public string APPLY_NOTE;
            public string POST_TIME;
            public string SET_YM;
            public string R3;
            public string R4;
            public string CREATE_TIME;
            public string CREATE_USER;
            public string UPDATE_TIME;
            public string UPDATE_USER;
            public string UPDATE_IP;
        }

        public class ME_DOCD_QUERY_PARAMS
        {
            public string DOCNO;
            public string SEQ;
            public string MMCODE;
            public string APPQTY;
            public string APVQTY;
            public string APVTIME;
            public string APVID;
            public string ACKQTY;
            public string ACKID;
            public string ACKTIME;
            public string STAT;
            public string RDOCNO;
            public string RSEQ;
            public string EXPT_DISTQTY;
            public string DIS_USER;
            public string DIS_TIME;
            public string BW_MQTY;
            public string BW_SQTY;
            public string PICK_QTY;
            public string PICK_USER;
            public string PICK_TIME;
            public string ONWAY_QTY;
            public string APL_CONTIME;
            public string AMT;
            public string UP;
            public string RV_MQTY;
            public string R2;
            public string APLYITEM_NOTE;
            public string CREATE_TIME;
            public string CREATE_USER;
            public string UPDATE_TIME;
            public string UPDATE_USER;
            public string UPDATE_IP;
        }
    }
}