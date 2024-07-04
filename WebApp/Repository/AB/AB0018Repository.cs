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

namespace WebApp.Repository.AB
{
    public class AB0018Repository : JCLib.Mvc.BaseRepository
    {
        public AB0018Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        public IEnumerable<ME_DOCM> QueryME(string docno, string lfdat_bg, string lfdat_ed, string frwh, string towh, string[] str_FLOWID, string userid, string tflow, int page_index, int page_size, string sorters)
        {
            DynamicParameters p = new DynamicParameters();

            //string sql = @" SELECT docno,case when flowid='0103' then '點收中' else '結案' end as flowid ,USER_NAME(appid) as appid,WH_NAME(appdept) as appdept, TWN_DATE(APPTIME) AS APPTIME,WH_NAME(usedept) as usedept,WH_NAME(frwh) as frwh,WH_NAME(towh) as towh FROM me_docm   
            //               where DOCTYPE='MR' and FLOWID='0103' ";
            string sql = @" SELECT docno,
                                   (SELECT FLOWNAME FROM ME_FLOW WHERE FLOWID=me_docm.FLOWID) FLOWID,
                                   USER_NAME(appid) as appid,INID_NAME(appdept) as appdept, TWN_DATE(APPTIME) AS APPTIME,
                                   INID_NAME(usedept) as usedept,WH_NAME(frwh) as frwh,WH_NAME(towh) as towh,towh as towh_no, 
                                   (CASE WHEN ((DOCTYPE='MR' and FLOWID IN ('0102','0103')) or (DOCTYPE='MS' and FLOWID IN ('0602','0603'))) 
                                     THEN (SELECT COUNT(*) CNT FROM ME_DOCD B WHERE B.DOCNO=me_docm.DOCNO AND (B.POSTID NOT IN ('C','D') or B.POSTID is null) )
                                     ELSE 999 END
                                   ) MR1,
                                   me_docm.FLOWID FLOWID_N 
                            FROM me_docm   
                           where 1 = 1  
                             AND EXISTS (SELECT 1 FROM MI_WHID X WHERE X.WH_USERID=:TUSER AND X.WH_NO = me_docm.TOWH) 
                            ";
            //((DOCTYPE='MR' and FLOWID IN ('0102','0103')) or (DOCTYPE='MS' and FLOWID IN ('0602','0603')))
            if (docno != "")
            {
                sql += " AND docno = :p0 ";
                p.Add(":p0", docno);
            }

            //if (lfdat_bg != "")
            //{
            //    sql += " AND APPTIME >= :p1 ";
            //    p.Add("@:p1", lfdat_bg);
            //}
            //if (lfdat_ed != "")
            //{
            //    sql += " AND APPTIME <= :p2 ";
            //    p.Add("@:p2", lfdat_ed);
            //}
            if (lfdat_bg != "" & lfdat_ed != "")
            {
                sql += " AND TWN_DATE(APPTIME) BETWEEN :d0 AND :d1 ";
                p.Add(":d0", string.Format("{0}", lfdat_bg));
                p.Add(":d1", string.Format("{0}", lfdat_ed));
            }
            if (lfdat_bg != "" & lfdat_ed == "")
            {
                sql += " AND TWN_DATE(APPTIME) >= :d0 ";
                p.Add(":d0", string.Format("{0}", lfdat_bg));
            }
            if (lfdat_bg == "" & lfdat_ed != "")
            {
                sql += " AND TWN_DATE(APPTIME) <= :d1 ";
                p.Add(":d1", string.Format("{0}", lfdat_ed));
            }
            if (frwh != "")
            {
                sql += " AND FRWH = :p3 ";
                p.Add(":p3", frwh);
            }
            //if (appid != "")
            //{
            //    sql += " AND APPID = :p4 ";
            //    p.Add(":p4", appid);
            //}
            //if (appdept != "")
            //{
            //    sql += " AND APPDEPT = :p5";
            //    p.Add(":p5",appdept);
            //}
            //if (userdept != "")
            //{
            //    sql += " AND USERDEPT = :p6 ";
            //    p.Add(":p6", userdept);
            //}
            //if (towh != "")
            //{
            //    sql += " AND TOWH = :p7 ";
            //    p.Add(":p7", towh);
            //}
            if (towh != "")
            {
                sql += " AND TOWH = :p7 ";
                p.Add(":p7", towh);
            }
            if (str_FLOWID.Length > 0)
            {
                string sql_FLOWID = "";
                sql += @" AND (";
                foreach (string tmp_FLOWID in str_FLOWID)
                {
                    if (string.IsNullOrEmpty(sql_FLOWID))
                    {
                        sql_FLOWID = @"FLOWID = '" + tmp_FLOWID + "'";
                    }
                    else
                    {
                        sql_FLOWID += @" OR FLOWID = '" + tmp_FLOWID + "'";
                    }
                }
                sql += sql_FLOWID + ") ";
            }
            if (tflow != "")
            {
                if (tflow == "0")
                {
                    sql += " AND FLOWID IN ('0100','0101','0102','0602','0103','0603','0104','0604','0199','0699') ";
                }
                else if (tflow == "1")
                {
                    sql += " AND FLOWID IN ('0100','0101','0102','0103','0104','0199') ";
                }
                else if (tflow == "6")
                {
                    sql += " AND FLOWID IN ('0602','0603','0604','0699') ";
                }
            }
            p.Add(":TUSER", userid);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<ME_DOCM>(GetPagingStatement(sql, sorters), p);
            //return DBWork.Connection.Query<PRM>(GetPagingStatement(sql, sorters), p);
        }


        public IEnumerable<ME_DOCD> QueryMEDOCD(string docno, int page_index, int page_size, string sorters)
        {
            DynamicParameters p = new DynamicParameters();

            //string sql = @"SELECT me_docd.docno,me_docd.seq,me_docd.mmcode as MMCODE,me_docd.appqty,me_docd.apvqty,   
            //             TWN_DATE( me_docd.apvtime) as APVTIME,USER_NAME(me_docd.apvid) as apvid,me_docd.pick_qty,me_docd.ackqty,(me_docd.pick_qty - me_docd.ackqty) as dueQty,  
            //              mi_mast.mmname_e FROM me_docm,me_docd, mi_mast WHERE me_docm.docno= me_docd.docno and  ( me_docd.mmcode = mi_mast.mmcode ) and me_docd.docno = :p0 and me_docm.FLOWID='0103'";
            string sql = @"SELECT b.docno,
                                  b.seq,
                                  (select TOWH 
                                     from ME_DOCM
                                    where DOCNO=b.DOCNO) as TOWH_NO,
                                  b.mmcode as MMCODE,
                                  b.appqty,
                                  b.apvqty,   
                                  TWN_DATE( b.apvtime) as APVTIME,
                                  USER_NAME(b.apvid) as apvid,
                                  b.pick_qty,
                                  b.ackqty  ,
                                  (b.pick_qty - b.ackqty) as dueQty,  
                                  c.mmname_e, 
                                  c.BASE_UNIT,
                                  b.POSTID , 
                                  c.WEXP_ID,
                                  (case when b.UPDATE_USER='強迫點收' then b.UPDATE_TIME
                                         else b.ACKTIME
                                    end) as ACKTIME,   --點收時間  
                                  (case when b.UPDATE_USER='強迫點收' then b.UPDATE_USER
                                         else USER_NAME(b.ACKID)
                                    end) as ACKID,     --點收人員
                                  (case when b.UPDATE_USER='強迫點收' then b.UPDATE_IP
                                         else ''
                                    end) as ACKSYSQTY  --強迫點收數量
                             FROM ME_DOCD b, MI_MAST c 
                            WHERE 1=1
                              and  b.mmcode = c.mmcode  ";
            // and (me_docm.FLOWID in ('0102','0103','0602','0603') )";
            if (docno != "")
            {
                sql += " AND b.docno = :p0 ";
                p.Add(":p0", string.Format("{0}", docno));
            }
            else
            {
                sql += " AND 1=2 ";
            }
            //p.Add(":p0", docno);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<ME_DOCD>(GetPagingStatement(sql, sorters), p);
            //return DBWork.Connection.Query<PRM>(GetPagingStatement(sql, sorters), p);
        }

        public ME_DOCM CheckMEDOCC(string docno, string seq)
        {

            //string sql = @"SELECT 1 FROM PH_VENDER WHERE AGEN_NO=:AGEN_NO";

            string sql = @"SELECT COUNT(*) as DOCNO FROM ME_DOCC where DOCNO= :docno and seq= :seq";

            return DBWork.Connection.Query<ME_DOCM>(sql, new { docno = docno, seq = seq }).SingleOrDefault();
            // return DBWork.Connection.QueryFirst<int>(sql, new { docno = docno, seq = seq }, DBWork.Transaction);
        }
        public ME_DOCM CheckMEDOCM(string docno)
        {

            string sql = @"SELECT COUNT(*) as DOCNO FROM ME_DOCD where DOCNO= :docno and POSTID = 'C' ";

            return DBWork.Connection.Query<ME_DOCM>(sql, new { docno = docno }).SingleOrDefault();
        }

        public IEnumerable<MI_WHID> GetWhid(string userId)
        {
            var sql = @"select WH_NO from MI_WHID WHERE WH_USERID = :WH_USERID";
            return DBWork.Connection.Query<MI_WHID>(sql, new { WH_USERID = userId }, DBWork.Transaction);
        }

        public int Update(ME_DOCM record)
        {
            int result;
            int result1;
            var sql = @"UPDATE ME_DOCD 
                           SET ACKQTY=:ACKQTY,ACKID=:UPDATE_USER,ACKTIME=SYSDATE, 
                               UPDATE_TIME=SYSDATE,UPDATE_USER=:UPDATE_USER,UPDATE_IP=:UPDATE_IP  
                         WHERE DOCNO=:DOCNO and SEQ=:SEQ";

            var sql1 = @"UPDATE ME_DOCC SET ACKQTY = :ACKQTY,UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE DOCNO = :DOCNO and CHECKSEQ=0 and GENWAY=1 and SEQ= :SEQ";
            result = DBWork.Connection.Execute(sql, record, DBWork.Transaction);
            result1 = DBWork.Connection.Execute(sql1, record, DBWork.Transaction);
            return result + result1;
        }
        public int Updated(ME_DOCD record)
        {
            int result;
            var sql = @"UPDATE ME_DOCD 
                           SET POSTID='4',ACKQTY=:ACKQTY,ACKID=:UPDATE_USER,ACKTIME=SYSDATE,
                               UPDATE_TIME=SYSDATE,UPDATE_USER=:UPDATE_USER,UPDATE_IP=:UPDATE_IP          
                         WHERE DOCNO=:DOCNO and SEQ=:SEQ";
            result = DBWork.Connection.Execute(sql, record, DBWork.Transaction);
            return result;
        }
        public bool CheckExistsD(string id, string seq)
        {
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO AND SEQ=:SEQ AND POSTID <> 'C'";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id, SEQ = seq }, DBWork.Transaction) == null);
        }
        public int UpdateE(ME_DOCM record)
        {
            int result;
            //int result1;
            int result2;
            var sql = @"UPDATE ME_DOCD 
                           SET ACKQTY=:ACKQTY,ACKID=:UPDATE_USER,ACKTIME=SYSDATE, 
                               UPDATE_TIME=SYSDATE,UPDATE_USER=:UPDATE_USER,UPDATE_IP=:UPDATE_IP
                         WHERE DOCNO=:DOCNO and SEQ=:SEQ";
            //var sql1 = @"UPDATE ME_DOCM SET UPDATE_TIME = TWN_SYSTIME, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
            //                    WHERE DOCNO = :DOCNO";
            var sql2 = @"UPDATE ME_DOCC SET ACKQTY = :ACKQTY,UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE DOCNO = :DOCNO and CHECKSEQ=0 and GENWAY=1 and SEQ= :SEQ";

            result = DBWork.Connection.Execute(sql, record, DBWork.Transaction);
            //result1 = DBWork.Connection.Execute(sql1, record, DBWork.Transaction);
            result2 = DBWork.Connection.Execute(sql2, record, DBWork.Transaction);
            return result + result2;// + result2;
                                    // return DBWork.Connection.Execute(sql, record, DBWork.Transaction);

        }

        public int UpdateEnd(string dno, string UPUSER, string UIP)
        {

            var sql = @"UPDATE ME_DOCM SET FLOWID='0199', UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE DOCNO = :DOCNO";

            return DBWork.Connection.Execute(sql, new { DOCNO = dno, UPDATE_USER = UPUSER, UPDATE_IP = UIP }, DBWork.Transaction);
        }

        // 將POSTID為C的改為4
        public int UpdatePostid4(string dno, string UPUSER, string UIP)
        {
            var sql = @"UPDATE ME_DOCD a
                           SET POSTID='4',ACKID=:UPDATE_USER,ACKTIME=SYSDATE,UPDATE_TIME=SYSDATE, 
                               UPDATE_USER=:UPDATE_USER, UPDATE_IP=:UPDATE_IP                               
                         WHERE DOCNO=:DOCNO and POSTID='C' ";
            return DBWork.Connection.Execute(sql, new { DOCNO = dno, UPDATE_USER = UPUSER, UPDATE_IP = UIP }, DBWork.Transaction);
        }

        public SP_MODEL POST_DOC(string I_DOCNO, string I_UPDUSR, string I_UPDIP)
        {
            var p = new OracleDynamicParameters();
            p.Add("I_DOCNO", value: I_DOCNO, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 21);
            p.Add("I_UPDUSR", value: I_UPDUSR, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 8);
            p.Add("I_UPDIP", value: I_UPDIP, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 20);

            p.Add("O_RETID", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 1);
            p.Add("O_ERRMSG", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 255);

            DBWork.Connection.Query("POST_DOC", p, commandType: CommandType.StoredProcedure);
            string retid = p.Get<OracleString>("O_RETID").Value;
            string errmsg = p.Get<OracleString>("O_ERRMSG").Value;
            SP_MODEL sp = new SP_MODEL
            {
                O_RETID = p.Get<OracleString>("O_RETID").Value,
                O_ERRMSG = p.Get<OracleString>("O_ERRMSG").Value
            };
            return sp;
        }
        public int Create(ME_DOCM record)
        {
            var sql = @"INSERT INTO ME_DOCC (DOCNO, SEQ, CHECKSEQ, GENWAY, ACKQTY, ACKTIME, ACKID, CREATE_TIME, CREATE_USER)  
                                VALUES (:DOCNO, :SEQ, :CHECKSEQ, :GENWAY, :ACKQTY, SYSDATE, :UPDATE_USER,SYSDATE, :UPDATE_USER)";
            return DBWork.Connection.Execute(sql, record, DBWork.Transaction);
        }
        public IEnumerable<ME_DOCM> QueryMEDOCC(string docno, int page_index, int page_size, string sorters)
        {
            DynamicParameters p = new DynamicParameters();

            string sql = @" SELECT DOCNO, SEQ, CHECKSEQ,  case when GENWAY=1 then '人工' else '點收機' end as GENWAY, ACKQTY, ACKTIME, USER_NAME(ACKID) as ACKID FROM ME_DOCC WHERE DOCNO = :p0";

            p.Add(":p0", docno);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<ME_DOCM>(GetPagingStatement(sql, sorters), p);
            //return DBWork.Connection.Query<PRM>(GetPagingStatement(sql, sorters), p);
        }

        public IEnumerable<ComboModel> GetStatusCombo()
        {
            string sql = @"SELECT DATA_VALUE as KEY_CODE, DATA_DESC as VALUE1,  DATA_VALUE || ' ' || DATA_DESC as COMBITEM
                        FROM PARAM_D where DATA_NAME='FLOWID' ORDER by KEY_CODE";

            return DBWork.Connection.Query<ComboModel>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetTowhCombo(string id)
        {
            string sql = @"SELECT DISTINCT A.WH_NO as VALUE, B.WH_NAME as TEXT,
                        A.WH_NO || ' ' || B.WH_NAME as COMBITEM 
                        FROM MI_WHID A,MI_WHMAST B 
                        WHERE A.WH_NO=B.WH_NO 
                        AND A.TASK_ID = '1' 
                        AND A.WH_USERID=:TUSER 
                        ORDER BY A.WH_NO";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { TUSER = id });

        }
        public IEnumerable<COMBO_MODEL> GetFrwhCombo(string id)
        {
            string sql = @"SELECT DISTINCT A.SUPPLY_WHNO VALUE , C.WH_NAME as TEXT,
                        A.SUPPLY_WHNO || ' ' || C.WH_NAME as COMBITEM
                        FROM MI_WINVCTL A,MI_WHMAST C 
                        WHERE A.SUPPLY_WHNO=C.WH_NO 
                        AND EXISTS (SELECT 1 FROM MI_WHID B WHERE WH_USERID = :TUSER AND TASK_ID = '1' AND A.WH_NO = B.WH_NO) 
                        ORDER BY A.SUPPLY_WHNO";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { TUSER = id });
        }

        public IEnumerable<AB0003Model> GetLoginInfo(string id)
        {
            string sql = @" SELECT TUSER AS USERID, UNA AS USERNAME, INID, INID_NAME(INID) AS INIDNAME,
                            WHNO_MM1 CENTER_WHNO,INID_NAME(WHNO_MM1) AS CENTER_WHNAME, TO_CHAR(SYSDATE,'YYYYMMDD') AS TODAY,
                            FLOW_AUTH(:TUSER) TASK_ID
                            FROM UR_ID
                            WHERE UR_ID.TUSER=:TUSER ";

            return DBWork.Connection.Query<AB0003Model>(sql, new { TUSER = id });
        }

        public IEnumerable<COMBO_MODEL> GetFlowidCombo()
        {
            string sql = @"SELECT DISTINCT FLOWID as VALUE, FLOWNAME as TEXT ,
                        FLOWID || ' ' || FLOWNAME as COMBITEM 
                        FROM ME_FLOW 
                        WHERE FLOWID IN ('0100','0101','0102','0602','0103','0603','0104','0604','0199','0699') 
                        ORDER BY FLOWID ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetFlowid1Combo()
        {
            string sql = @"SELECT DISTINCT FLOWID as VALUE, FLOWNAME as TEXT ,
                        FLOWID || ' ' || FLOWNAME as COMBITEM 
                        FROM ME_FLOW 
                        WHERE FLOWID IN ('0100','0101','0102','0103','0104','0199') 
                        ORDER BY FLOWID ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetFlowid6Combo()
        {
            string sql = @"SELECT DISTINCT FLOWID as VALUE, FLOWNAME as TEXT ,
                        FLOWID || ' ' || FLOWNAME as COMBITEM 
                        FROM ME_FLOW 
                        WHERE FLOWID IN ('0602','0603','0604','0699') 
                        ORDER BY FLOWID ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public bool CheckExistsDKeyByUpd(string id, string seq)
        {
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO  AND SEQ=:SEQ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id, SEQ = seq }, DBWork.Transaction) == null);
        }
        public int UpdateMedocD(ME_DOCD record)
        {
            int result;
            var sql = @"UPDATE ME_DOCD 
                           SET ACKQTY=:ACKQTY,ACKID=:UPDATE_USER,ACKTIME=SYSDATE, 
                               UPDATE_TIME=SYSDATE,UPDATE_USER=:UPDATE_USER,UPDATE_IP=:UPDATE_IP                                      
                         WHERE DOCNO=:DOCNO and SEQ=:SEQ";
            result = DBWork.Connection.Execute(sql, record, DBWork.Transaction);
            return result;
        }

        public string GetTflow(string id)
        {
            string sql = @" SELECT FLOW_AUTH(:TUSER) TFLOW FROM DUAL ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { TUSER = id }, DBWork.Transaction).ToString();
            return rtn;
        }

        public string GetTaskid(string id)
        {
            string sql = @"SELECT WHM1_TASK(:WH_USERID) FROM DUAL ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { WH_USERID = id }, DBWork.Transaction).ToString();
            return rtn;
        }

        public DataTable GetExcel(string docno, string frwh, string appdat_bg, string appdat_ed, string towh, string mmcode)
        {
            var p = new DynamicParameters();

            string sql = @"select a.TOWH as 領用庫房,a.FRWH as 核撥庫房,a.DOCNO as 申請單號,
                                  (select FLOWNAME 
                                     from ME_FLOW
                                    where FLOWID = a.FLOWID and DOCTYPE = a.DOCTYPE) as 單據狀態,
                                   INID_NAME(a.APPDEPT) as 申請部門,
                                   b.MMCODE as 院內碼,b.MMNAME_E as 英文品名,b.MMNAME_C as 中文品名,
                                   BASE_UNIT(b.MMCODE) as 計量單位,
                                   b.APPQTY as 申請數量,TWN_DATE(a.APPTIME) as 申請日期,USER_NAME(a.APPID) as 申請人員,
                                   b.APVQTY as 核撥數量,TWN_DATE(b.APVTIME) as 核撥日期,USER_NAME(b.APVID) as 核撥人員,
                                   b.ACKQTY as 點收數量,(b.ACKQTY - b.APVQTY) as 差異量,
                                   GET_PARAM('ME_DOCD', 'POSTID', POSTID) as 過帳狀態,
                                   (case when b.UPDATE_USER = '強迫點收'
                                              then b.UPDATE_TIME else b.ACKTIME
                                    end) as 點收時間,
                                   (case when b.UPDATE_USER = '強迫點收'
                                              then b.UPDATE_USER
                                         else USER_NAME(b.ACKID)
                                    end) as 點收人員,
                                   (case when b.UPDATE_USER = '強迫點收'
                                              then b.UPDATE_IP
                                         else ''
                                    end) as 強迫點收數量
                              from (select TOWH, FRWH, DOCNO, FLOWID, DOCTYPE, APPDEPT, APPTIME, APPID
                                      from ME_DOCM
                                     where 1=1  and DOCTYPE in ('MR', 'MS')
                                       and MAT_CLASS='01'
                                       and FLOWID in ('0199', '0699')";

            if (docno != "")
            {
                sql += " and DOCNO = :p9 ";
                p.Add(":p9", docno);
            }
            if (frwh != "")
            {
                sql += " and FRWH = :p10 ";
                p.Add(":p10", frwh);
            }
            if (appdat_bg != "" & appdat_ed != "")
            {
                sql += " and TWN_DATE(APPTIME) between :d0 and :d1 ";
                p.Add(":d0", string.Format("{0}", appdat_bg));
                p.Add(":d1", string.Format("{0}", appdat_ed));
            }
            if (appdat_bg != "" & appdat_ed == "")
            {
                sql += " and TWN_DATE(APPTIME) >= :d0 ";
                p.Add(":d0", string.Format("{0}", appdat_bg));
            }
            if (appdat_bg == "" & appdat_ed != "")
            {
                sql += " and TWN_DATE(APPTIME) <= :d1 ";
                p.Add(":d1", string.Format("{0}", appdat_ed));
            }
            if (towh != "")
            {
                sql += " and TOWH = :p13 ";
                p.Add(":p13", towh);
            }
            sql += ") a";
            sql += @" join (select DOCNO, SEQ, MMCODE,
                                   (select MMNAME_E from MI_MAST where MMCODE = a.MMCODE) as MMNAME_E,
                                   (select MMNAME_C from MI_MAST where MMCODE = a.MMCODE) as MMNAME_C,
                                    APPQTY,APVQTY,APVTIME,APVID,ACKQTY,ACKTIME,ACKID,POSTID,
                                    UPDATE_USER,UPDATE_TIME,UPDATE_IP
                              from ME_DOCD a
                             where 1=1 and APVQTY<>ACKQTY";

            if (mmcode != "")
            {
                sql += " and MMCODE = :p14 ";
                p.Add(":p14", mmcode);
            }

            sql +=") b on a.DOCNO=b.DOCNO ";

            sql += "order by b.DOCNO,b.MMCODE,b.SEQ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<MI_MAST> GetMMCodeDocd(string p0, string p1, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"select {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E  
                          from MI_MAST A ,ME_DOCD B 
                         where A.MMCODE=B.MMCODE 
                           and a.mat_class = '01'";
            if (p1 != "")
            {
                sql += " and b.DOCNO LIKE :DOCNO ";
                p.Add(":DOCNO", p1);
            }
            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}%", p0));

                sql += " OR MMNAME_E LIKE UPPER(:MMNAME_E) ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR MMNAME_C LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY MMCODE ";
            }
     
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public int UpdateMI_WINVCTL(string dno, string towh_no,string mmcode)
        {
            int result;
            string sql = "";
            if (mmcode=="") //結案
            {
              sql = @"update MI_WINVCTL
                         set FSTACKDATE=trunc(sysdate)
                       where WH_NO=:WH_NO 
                         and MMCODE in (select MMCODE from ME_DOCD where DOCNO=:DOCNO and POSTID='4')                         
                         and FSTACKDATE is null
                     ";
            }
            else //單筆接收
            {   
              sql = @"update MI_WINVCTL
                         set FSTACKDATE=trunc(sysdate)
                       where WH_NO=:WH_NO and MMCODE=:MMCODE
                         and FSTACKDATE is null
                     ";
            }
            result = DBWork.Connection.Execute(sql, new { DOCNO = dno, WH_NO = towh_no, MMCODE = mmcode }, DBWork.Transaction);
            return result;

        }
    }
}