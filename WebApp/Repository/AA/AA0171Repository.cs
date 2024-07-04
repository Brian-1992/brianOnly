using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace WebApp.Repository.AA
{
    public class AA0171Repository : JCLib.Mvc.BaseRepository
    {
        public AA0171Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<MI_MAST> GetAllM(string mmcode, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.MMCODE, B.MMNAME_C, B.MMNAME_E, A.INV_QTY, B.BASE_UNIT  
                          FROM MI_WHINV A ,MI_MAST B 
                         WHERE A.MMCODE = B.MMCODE 
                           AND A.WH_NO = WHNO_ME1 
                         ";
            if (mmcode != "")
            {
                sql += " AND A.MMCODE = :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}", mmcode));
            }
            sql += " ORDER BY A.MMCODE ";
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public string CallProc(string id, string upuser, string upip)
        {
            var p = new OracleDynamicParameters();
            p.Add("I_DOCNO", value: id, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 21);
            p.Add("I_UPDUSR", value: upuser, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 8);
            p.Add("I_UPDIP", value: upip, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 20);

            p.Add("O_RETID", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 1);
            p.Add("O_ERRMSG", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 255);

            DBWork.Connection.Query("POST_DOC", p, commandType: CommandType.StoredProcedure);
            string retid = p.Get<OracleString>("O_RETID").Value;
            string errmsg = p.Get<OracleString>("O_ERRMSG").Value;
            if (retid == "N")
            {
                retid = errmsg;
            }
            return retid;
        }

        public IEnumerable<MI_MAST> GetMmCodeCombo(string p0, string p1, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E  
                        FROM MI_MAST A, MI_WHINV B WHERE A.MMCODE=B.MMCODE AND B.WH_NO = WHNO_ME1  ";
            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(UPPER(A.MMCODE), UPPER(:MMCODE_I)), 1000) + NVL(INSTR(UPPER(MMNAME_E), UPPER(:MMNAME_E_I)), 100) * 10 + NVL(INSTR(UPPER(MMNAME_C), UPPER(:MMNAME_C_I)), 100) * 10) IDX,");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (UPPER(A.MMCODE) LIKE UPPER(:MMCODE) ";
                p.Add(":MMCODE", string.Format("{0}%", p0));

                sql += " OR UPPER(MMNAME_E) LIKE UPPER(:MMNAME_E) ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR UPPER(MMNAME_C) LIKE UPPER(:MMNAME_C)) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY MMCODE ";
            }
            p.Add(":DOCNO", p1);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public int CreateM(ME_DOCM ME_DOCM)
        {
            var sql = @"INSERT INTO ME_DOCM (
                        DOCNO, DOCTYPE, FLOWID , APPID , APPDEPT , 
                        APPTIME , USEID , USEDEPT , FRWH , TOWH , 
                        APPLY_KIND ,APPLY_NOTE ,MAT_CLASS,
                        CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                      VALUES (
                        :DOCNO, :DOCTYPE, :FLOWID , :APPID , :APPDEPT , 
                        SYSDATE , :USEID , :USEDEPT , whno_me1 , :TOWH , 
                        :APPLY_KIND , :APPLY_NOTE ,:MAT_CLASS,
                        SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);
        }

        public int CreateD(ME_DOCD ME_DOCD)
        {
            var sql = @"INSERT INTO ME_DOCD (
                            DOCNO, SEQ, MMCODE , APPQTY , APVQTY,  APVTIME, APVID, ACKQTY, ACKID, ACKTIME,
                            S_INV_QTY, INV_QTY, SAFE_QTY, OPER_QTY, APLYITEM_NOTE, ISSPLIT, M_AGENNO,
                            POSTID, EXPT_DISTQTY, PICK_QTY, DIS_USER,  DIS_TIME, APL_CONTIME,
                            CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                           SELECT  :DOCNO, :SEQ, :MMCODE , :APPQTY , :APVQTY,  SYSDATE, :APVID, :ACKQTY, :ACKID, SYSDATE,
                                           :S_INV_QTY, :INV_QTY, :SAFE_QTY, :OPER_QTY,  '藥庫批次核撥', 'N', M_AGENNO,
                                           '3', :EXPT_DISTQTY, :PICK_QTY, :DIS_USER,  SYSDATE, SYSDATE,
                                            SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP
                            FROM MI_MAST 
                            WHERE MMCODE = :MMCODE ";
            return DBWork.Connection.Execute(sql, ME_DOCD, DBWork.Transaction);
        }

        public int UpdateD(ME_DOCD ME_DOCD)
        {
            var sql = @"UPDATE ME_DOCD 
                           SET APVQTY = APPQTY, EXPT_DISTQTY = APPQTY, PICK_QTY = APPQTY, 
                               ACKQTY = APPQTY, DIS_USER = :UPDATE_USER, APVID = :UPDATE_USER, 
                               DIS_TIME = SYSDATE, APL_CONTIME = SYSDATE, APVTIME = SYSDATE, 
                               UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                         WHERE DOCNO = :DOCNO ";
            return DBWork.Connection.Execute(sql, ME_DOCD, DBWork.Transaction);
        }

        public int UpdateDP(ME_DOCD ME_DOCD)
        {
            var sql = @"UPDATE ME_DOCD 
                           SET POSTID = '4', UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                         WHERE DOCNO = :DOCNO AND POSTID = 'C' ";
            return DBWork.Connection.Execute(sql, ME_DOCD, DBWork.Transaction);
        }
        public string GetUridInid(string id)
        {
            string sql = @"SELECT INID FROM UR_ID WHERE TUSER=:TUSER ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { TUSER = id }, DBWork.Transaction).ToString();
            return rtn;
        }
        public string GetTowh()
        {
            string sql = @"select wh_no from MI_WHMAST where wh_kind = '0' and wh_grade = '2' and nvl(cancel_id,'N') = 'N' and rownum = 1 ";
            string rtn = DBWork.Connection.ExecuteScalar(sql,  DBWork.Transaction).ToString();
            return rtn;
        }

        public bool CheckExistsUser(string id)
        {
            string sql = @"select 1 from MI_WHID a, MI_WHMAST b 
                            where a.wh_userId = :TUSER 
                              and a.wh_no = whno_me1
                              and a.wh_no = b.wh_no and nvl(b.cancel_id,'N') = 'N'
                            ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { TUSER = id }, DBWork.Transaction) == null);
        }

        public int CheckWhMast()
        {
            string sql = @"SELECT COUNT(*) AS CNT 
                             FROM MI_WHMAST 
                            where wh_kind = '0' 
                              and wh_grade = '2' 
                              and nvl(cancel_id,'N') = 'N'";
            int rtn = Convert.ToInt32(DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction).ToString());
            return rtn;
        }

        public string GetDailyDocno()
        {
            string sql = @"select GET_DAILY_DOCNO from DUAL";
            string rtn = DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction).ToString();
            return rtn;
        }
        public bool CheckExistsD(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }

        public string GetDocDSeq(string id)
        {
            string sql = @"SELECT MAX(SEQ)+1 as SEQ FROM ME_DOCD WHERE DOCNO=:DOCNO ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction).ToString();
            return rtn;
        }
        public string GetsInvQty(string whno, string mmcode)
        {
            string sql = " select INV_QTY(whno_me1, :MMCODE) from dual ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { WH_NO = whno, MMCODE = mmcode }, DBWork.Transaction).ToString();
            return rtn;
        }
        public string GetInvQty(string whno, string mmcode)
        {
            string sql = " SELECT NVL(INV_QTY(:WH_NO,:MMCODE),0) AS INV_QTY FROM DUAL ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { WH_NO = whno, MMCODE = mmcode }, DBWork.Transaction).ToString();
            return rtn;
        }
        public string GetSafeQty(string whno, string mmcode)
        {
            string sql = " SELECT NVL(SAFE_QTY(:WH_NO,:MMCODE),0) AS SAFE_QTY FROM DUAL ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { WH_NO = whno, MMCODE = mmcode }, DBWork.Transaction).ToString();
            return rtn;
        }
        public string GetOperQty(string whno, string mmcode)
        {
            string sql = " SELECT NVL(OPER_QTY(:WH_NO,:MMCODE),0) AS OPER_QTY FROM DUAL ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { WH_NO = whno, MMCODE = mmcode }, DBWork.Transaction).ToString();
            return rtn;
        }
    }
}
