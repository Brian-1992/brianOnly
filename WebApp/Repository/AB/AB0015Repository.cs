using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace WebApp.Repository.AB
{
    public class AB0015Repository : JCLib.Mvc.BaseRepository
    {
        public AB0015Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ME_DOCM> GetAllM(string apptime1, string apptime2, string inid, string[] str_FLOWID, string applykind, string matclass,string storeid, string doctype, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.* ,
                           (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='FLOWID_MR1' AND DATA_VALUE=A.FLOWID) FLOWID_N,
                           (SELECT MAT_CLSNAME FROM MI_MATCLASS WHERE MAT_CLASS=A.MAT_CLASS) MAT_CLASS_N,
                           (SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.FRWH) FRWH_N,
                           (SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.TOWH) TOWH_N,
                           (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='APPLY_KIND2' AND DATA_VALUE=A.APPLY_KIND) APPLY_KIND_N,
                           TWN_DATE(A.APPTIME) APPTIME_T,
                           CASE WHEN EXISTS ( SELECT 1 FROM ME_DOCD X, MI_MAST Y WHERE X.MMCODE=Y.MMCODE AND X.DOCNO=A.DOCNO AND Y.WEXP_ID='Y')  THEN 'Y' ELSE 'N' END  WEXP_YN,
                           (CASE WHEN A.DOCTYPE = 'MR1' THEN '一般物品庫備' WHEN A.DOCTYPE = 'MR2' THEN '衛材庫備' 
                                 WHEN A.DOCTYPE = 'MR3' THEN '一般物品非庫備' WHEN A.DOCTYPE = 'MR4' THEN '衛材非庫備' END)DOCTYPE_N
                           ,(case when CREATE_USER='緊急醫療出貨' then APPLY_NOTE else '' end) isCRNOTE --備註
                           ,(case when CREATE_USER='緊急醫療出貨' then 'Y' else 'N' end) isCR
                           ,(case when exists (select 1 from ME_DOCM where docno = a.docno and docno <> srcdocno) then 'Y' else 'N' end) as isDis ---是否非庫備核撥(Y為是 N為否)
                     FROM ME_DOCM A WHERE 1=1 AND A.DOCTYPE IN ( 'MR1' ,'MR2', 'MR3', 'MR4')
                     ";

            if (apptime1 != "" & apptime2 != "")
            {
                sql += " AND TWN_DATE(A.APPTIME) BETWEEN :d0 AND :d1 ";
                p.Add(":d0", string.Format("{0}", apptime1));
                p.Add(":d1", string.Format("{0}", apptime2));
            }
            if (apptime1 != "" & apptime2 == "")
            {
                sql += " AND TWN_DATE(A.APPTIME) >= :d0 ";
                p.Add(":d0", string.Format("{0}", apptime1));
            }
            if (apptime1 == "" & apptime2 != "")
            {
                sql += " AND TWN_DATE(A.APPTIME) <= :d1 ";
                p.Add(":d1", string.Format("{0}", apptime2));
            }
            if (inid != "")
            {
                //20200401 MARK AND MODIFY INID傳入LOGIN USER
                //sql += " AND A.APPDEPT IN (SELECT INID FROM MI_WHMAST WHERE SUPPLY_INID =:p2) ";
                //p.Add(":p2", string.Format("{0}", inid));
                sql += @"AND EXISTS (select 1 from MI_WHMAST C
                            WHERE WH_KIND='1'
                            AND EXISTS(SELECT 'X' FROM UR_ID B WHERE ( C.SUPPLY_INID=B.INID OR C.INID=B.INID ) AND B.TUSER=:TUSER)
                            AND NOT EXISTS(SELECT 'X' FROM MI_WHID B WHERE TASK_ID IN ('2','3') AND B.WH_USERID=:TUSER)
                            AND C.WH_NO = A.TOWH
                            UNION ALL 
                            SELECT 1 FROM MI_WHMAST C,MI_WHID B
                            WHERE C.WH_NO=B.WH_NO AND B.TASK_ID IN ('2','3') AND B.WH_USERID=:TUSER
                            AND  C.WH_NO = A.TOWH )";
                p.Add(":TUSER", string.Format("{0}", inid));
            }
            //判斷FLOWID查詢條件是否有值，有的話用字串相加的方式串接條件(IN的方法會有問題)
            if (str_FLOWID.Length > 0)
            {
                string sql_FLOWID = "";
                sql += @"AND (";
                foreach (string tmp_FLOWID in str_FLOWID)
                {
                    if (string.IsNullOrEmpty(sql_FLOWID))
                    {
                        sql_FLOWID = @"A.FLOWID = '" + tmp_FLOWID + "'";
                    }
                    else
                    {
                        sql_FLOWID += @" OR A.FLOWID = '" + tmp_FLOWID + "'";
                    }
                }
                
                sql += sql_FLOWID + ") ";
            }
            if (applykind != "")
            {
                sql += " AND A.APPLY_KIND = :p4 ";
                p.Add(":p4", string.Format("{0}", applykind));
            }
            if (matclass != "")
            {
                sql += " AND A.MAT_CLASS = :p5 ";
                p.Add(":p5", string.Format("{0}", matclass));
            }
            if (storeid != "")
            {
                if (storeid == "0")
                {
                    sql += " AND A.DOCTYPE  IN ('MR3','MR4') ";
                }
                else
                {
                    sql += " AND A.DOCTYPE IN ('MR1','MR2') ";
                }
                
            }
            /*
            if (doctype != "")
            {
                sql += " AND A.DOCTYPE = :p6 ";
                p.Add(":p6", string.Format("{0}", doctype));
            }
            */
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCM>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<ME_DOCD> GetAllD(string DOCNO, string MMCODE, string YN, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.DOCNO, A.SEQ, A.MMCODE , A.APPQTY , A.APLYITEM_NOTE, 
                        A.GTAPL_RESON,A.STAT,(A.EXPT_DISTQTY + A.BW_MQTY) AS EXPT_DISTQTY, 
                        A.APVQTY, A.ACKQTY, user_name(a.ackid) as ackid, twn_time(a.acktime) as acktime, 
                        A.BW_MQTY,A.BW_SQTY,A.APVTIME,A.ONWAY_QTY,
                        B.MMNAME_C, B.MMNAME_E, B.BASE_UNIT,B.M_CONTPRICE,
                        ( SELECT AVG_PRICE FROM MI_WHCOST WHERE MMCODE = A.MMCODE AND ROWNUM=1 ) AS AVG_PRICE,
                        ( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO=C.TOWH ) AS INV_QTY,
                        ( SELECT AVG_APLQTY FROM V_MM_AVGAPL WHERE MMCODE = A.MMCODE AND WH_NO=C.TOWH ) AS AVG_APLQTY,
                        ( CASE WHEN C.FLOWID IN ('1','2','3','4') THEN A.PICK_QTY - A.ACKQTY ELSE A.ONWAY_QTY END ) AS ONWAYQTY,
                        ( CASE WHEN C.FLOWID IN ('1','2','3','4') THEN A.PICK_QTY - A.ACKQTY ELSE A.ONWAY_QTY END ) AS AFTERQTY,
                        C.FLOWID,A.PICK_QTY,
                        ( SELECT SAFE_QTY FROM MI_WINVCTL WHERE MMCODE = A.MMCODE AND WH_NO=C.TOWH ) AS SAFE_QTY,
                        (CASE WHEN A.APPQTY <> (A.EXPT_DISTQTY + A.BW_MQTY)  THEN '1' ELSE '0' END) SGN, 
                        (select m_storeid from MI_MAST where mmcode = a.mmcode) as m_storeid
                        , a.ackqtyt
                        FROM ME_DOCD A
                        INNER JOIN MI_MAST B ON B.MMCODE = A.MMCODE
                        INNER JOIN ME_DOCM C ON C.DOCNO=A.DOCNO
                        LEFT OUTER JOIN MI_WHINV D ON D.MMCODE = A.MMCODE AND D.WH_NO=C.FRWH
                        WHERE 1 = 1  ";

            if (DOCNO != "")
            {
                sql += " AND A.DOCNO LIKE :p0 ";
                p.Add(":p0", string.Format("%{0}%", DOCNO));
            }
            else
            {
                sql += " AND 1=2 ";
            }
            if (MMCODE != "")
            {
                sql += " AND A.MMCODE LIKE :p1 ";
                p.Add(":p1", string.Format("%{0}%", MMCODE));
            }
            if (YN == "N") //僅顯示差異
            {
                sql += "AND ( A.ACKQTY <> (A.EXPT_DISTQTY + A.BW_MQTY)  OR A.ACKQTY <> A.PICK_QTY ) ";
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCD>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public int UpdateMeDocmStatus(ME_DOCD me_docd)
        {
            var sql = @"UPDATE ME_DOCM SET FLOWID = '4', 
                                UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, me_docd, DBWork.Transaction);
        }
        public int UpdateMeDocd(ME_DOCD ME_DOCD)
        {
            var sql = @"UPDATE ME_DOCD SET ACKQTY = :ACKQTY,  
                        APLYITEM_NOTE = :APLYITEM_NOTE,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                        WHERE DOCNO = :DOCNO AND SEQ = :SEQ";
            return DBWork.Connection.Execute(sql, ME_DOCD, DBWork.Transaction);
        }
        public int ApplyD(ME_DOCM ME_DOCM)
        {
            var sql = @"UPDATE ME_DOCD SET APVQTY = ACKQTY, ACKID=:UPDATE_USER, ACKTIME = SYSDATE,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP,
                        APVTIME = SYSDATE, APVID = :UPDATE_USER
                      WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);

        }
        public int ApplyDMr34(ME_DOCM ME_DOCM)
        {
            var sql = @"UPDATE ME_DOCD 
                           SET APVQTY = (nvl(APVQTY, 0) + ACKQTY- EXPT_DISTQTY), 
                               ACKQTYT = (nvl(ACKQTYT, 0) + ACKQTY),
                               EXPT_DISTQTY = ACKQTY,
                               ACKID=:UPDATE_USER, ACKTIME = SYSDATE,
                               APVTIME = SYSDATE, APVID = :UPDATE_USER,
                               UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);

        }
        public bool CheckExistsM(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCM WHERE DOCNO=:DOCNO AND FLOWID NOT IN ('3', '4') ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        public bool CheckExistsD(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        public string GetUridInid(string id)
        {
            string sql = @"SELECT INID FROM UR_ID WHERE TUSER=:TUSER ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { TUSER = id }, DBWork.Transaction).ToString();
            return rtn;
        }
        public string GetTwnsystime()
        {
            string sql = @"SELECT TWN_SYSTIME FROM DUAL ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction).ToString();
            return rtn;
        }
        public string GetDocmdoctype(string id)
        {
            string sql = @"SELECT DOCTYPE FROM ME_DOCM WHERE DOCNO=:DOCNO ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction).ToString();
            return rtn;
        }

        public string GetDocmIsdis(string id)
        {
            string sql = @"SELECT (case when exists (select 1 from ME_DOCM where docno = A.docno and docno <> srcdocno) then 'Y' else 'N' end) FROM ME_DOCM A WHERE DOCNO=:DOCNO ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction).ToString();
            return rtn;
        }

        public string CallProc(string id, string upuser, string upip)
        {
            var p = new OracleDynamicParameters();
            p.Add("I_DOCNO", value: id, dbType: OracleDbType.Varchar2,direction: ParameterDirection.Input, size: 21);
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
        public IEnumerable<COMBO_MODEL> GetDocnoCombo(string id)
        {
            string sql = @"SELECT DISTINCT A.DOCNO as VALUE, A.DOCNO as TEXT,
                        A.DOCNO as COMBITEM
                        FROM ME_DOCM A
                        WHERE 1=1 AND ( A.DOCTYPE = 'MR1' OR A.DOCTYPE = 'MR2') 
                        AND A.APPDEPT=:APPDEPT
                        ORDER BY A.DOCNO";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { APPDEPT = id });
        }
        public IEnumerable<COMBO_MODEL> GetFlowidCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='FLOWID_MR1' 
                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetApplyKindCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='APPLY_KIND2' 
                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<COMBO_MODEL> GetAppDeptCombo()
        {
            string sql = @"SELECT DISTINCT A.APPDEPT as VALUE, B.INID_NAME as TEXT ,
                        A.APPDEPT || ' ' || B.INID_NAME as COMBITEM 
                        FROM ME_DOCM A, UR_INID B 
                        WHERE A.APPDEPT=B.INID 
                        ORDER BY A.APPDEPT";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<MI_MAST> GetMmCodeCombo(string p0, string p1, string p2, int page_index, int page_size, string sorters)
        {

            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT,A.M_CONTPRICE,
                        ( SELECT AVG_PRICE FROM MI_WHCOST WHERE MMCODE = A.MMCODE AND ROWNUM=1 ) AS AVG_PRICE,
                        ( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO=:TOWH ) AS INV_QTY,
                        ( SELECT AVG_APLQTY FROM V_MM_AVGAPL WHERE MMCODE = A.MMCODE AND WH_NO=:TOWH ) AS AVG_APLQTY 
                        FROM MI_MAST A WHERE 1=1 AND A.MAT_CLASS = :MAT_CLASS 
                          AND A.M_STOREID = '1'
                          AND A.M_CONTID <> '3'
                          AND A.M_APPLYID <> 'E' ";

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
            p.Add(":MAT_CLASS", p1);
            p.Add(":TOWH", p2);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<MI_MAST> GetMMCodeDocd(string p0, string p1, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E  
                        FROM MI_MAST A, ME_DOCD B WHERE A.MMCODE=B.MMCODE AND B.DOCNO = :DOCNO  ";
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
            p.Add(":DOCNO", p1);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<COMBO_MODEL> GetFrwhCombo()
        {
            string sql = @"SELECT DISTINCT WH_NO as VALUE, WH_NAME as TEXT,
                        WH_NO || ' ' || WH_NAME as COMBITEM 
                        FROM MI_WHMAST 
                        WHERE 1=1 AND WH_KIND = '1' AND WH_GRADE = '1'  
                        ORDER BY WH_NO";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetTowhCombo(string id)
        {
            string sql = @"SELECT DISTINCT A.WH_NO as VALUE, A.WH_NAME as TEXT,
                        A.WH_NO || ' ' || A.WH_NAME as COMBITEM 
                        FROM MI_WHMAST A,UR_ID B
                        WHERE A.SUPPLY_INID=B.INID 
                        AND A.WH_KIND = '1' 
                        AND TUSER=:TUSER 
                        ORDER BY WH_NO";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { TUSER = id });
        }
        public IEnumerable<COMBO_MODEL> GetMatclassCombo()
        {
            string sql = @"SELECT MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, 
                        MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM 
                        FROM MI_MATCLASS WHERE MAT_CLSID in ('2','3') 
                        ORDER BY MAT_CLASS";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetStoreIdCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='MI_MAST' AND DATA_NAME='M_STOREID' 
                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public string GetIsCR(string docno) {
            string sql = @"
                select (case when CREATE_USER='緊急醫療出貨' then 'Y' else 'N' end) from ME_DOCM
                 where docno = :docno
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { docno }, DBWork.Transaction);
        }

        public int UpdateCrdoc(string docno) {
            string sql = @"
                update CR_DOC
                   set CR_STATUS = 'Q'
                 where rdocno = :docno
            ";
            return DBWork.Connection.Execute(sql, new { docno }, DBWork.Transaction);
        }
    }
}
