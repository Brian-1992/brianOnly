using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models.AA;
using System.Collections.Generic;
using WebApp.Models;
using System.Text;

namespace WebApp.Repository.AA
{
    public class AA0228Repository : JCLib.Mvc.BaseRepository
    {
        public AA0228Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ME_DOCM> GetAllM(string apptime1, string apptime2, string[] str_flowid, string[] str_matclass, string docno, string tuser, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.* ,
                        (case when mat_class = '01' and a.flowId <> 'X'
                        then (select distinct flowId || ' ' || flowName from ME_FLOW where doctype = 'WT' and flowId = a.flowId) 
                        when mat_class <> '01' and a.flowId <> 'X'
                        then (select distinct flowId || ' 衛材' || flowName from ME_FLOW where doctype = 'WT' and flowId = a.flowId) 
                         else (select distinct flowId || ' ' || flowName from ME_FLOW where doctype = 'WT' and flowId = a.flowId)
                        end) AS FLOWID_N,
                        (SELECT MAT_CLSNAME FROM MI_MATCLASS WHERE MAT_CLASS=A.MAT_CLASS and rownum=1) MAT_CLASS_N,
                        (SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.FRWH and rownum=1) FRWH_N,
                        (SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.TOWH and rownum=1) TOWH_N,
                        (SELECT UNA FROM UR_ID WHERE TUSER=A.APPID and rownum=1) APP_NAME,
                        (SELECT INID_NAME FROM UR_INID WHERE INID=A.APPDEPT and rownum=1) APPDEPT_NAME,
                        (SELECT DISTINCT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='STKTRANSKIND2' AND DATA_VALUE=A.STKTRANSKIND and rownum=1) STKTRANSKIND_N,
                        (SELECT AGEN_NAMEC FROM PH_VENDER WHERE AGEN_NO = A.AGEN_NO and rownum=1)AGEN_NO_N,
                        TWN_DATE(A.APPTIME) APPTIME_T
                        FROM ME_DOCM A WHERE 1=1 AND A.DOCTYPE = 'WT'
                        AND A.FRWH in (select wh_no from MI_WHID where wh_userId = :tuser) ";
            p.Add(":tuser", string.Format("{0}", tuser));

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

            if (str_flowid.Length > 0) //P2
            {
                sql += @"AND A.FLOWID in :FLOWID ";

                p.Add(":FLOWID", str_flowid);
            }

            if (str_matclass.Length > 0) //P3
            {
                string sql_matclass = "";
                sql += @"AND (";
                foreach (string tmp_matclass in str_matclass)
                {
                    if (string.IsNullOrEmpty(sql_matclass))
                    {
                        if (tmp_matclass.Contains("SUB_"))
                            sql_matclass = @"(select count(*) from ME_DOCD C left join MI_MAST D on C.MMCODE = D.MMCODE where A.DOCNO = C.DOCNO and D.MAT_CLASS_SUB = '" + tmp_matclass.Replace("SUB_", "") + "') > 0";
                        else
                            sql_matclass = @"A.MAT_CLASS = '" + tmp_matclass + "'";
                    }
                    else
                    {
                        if (tmp_matclass.Contains("SUB_"))
                            sql_matclass += @" OR (select count(*) from ME_DOCD C left join MI_MAST D on C.MMCODE = D.MMCODE where A.DOCNO = C.DOCNO and D.MAT_CLASS_SUB = '" + tmp_matclass.Replace("SUB_", "") + "') > 0";
                        else
                            sql_matclass += @" OR A.MAT_CLASS = '" + tmp_matclass + "'";
                    }
                }
                sql += sql_matclass + ") ";
            }

            if (docno != "")
            {
                sql += " AND A.DOCNO = :pp ";
                p.Add(":pp", string.Format("{0}", docno));
            }
            sql += @" ORDER BY A.DOCNO DESC";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<ME_DOCM>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCM> GetM(string id)
        {
            var sql = @"SELECT A.* ,
                        (case when mat_class = '01' and a.flowId <> 'X'
                        then (select distinct flowId || ' ' || flowName from ME_FLOW where doctype = 'WT' and flowId = a.flowId) 
                        when mat_class <> '01' and a.flowId <> 'X'
                        then (select distinct flowId || ' 衛材' || flowName from ME_FLOW where doctype = 'WT' and flowId = a.flowId) 
                         else (select distinct flowId ||' ' || flowName from ME_FLOW where doctype = 'WT' and flowId = a.flowId)
                        end) AS FLOWID_N,
                        (SELECT MAT_CLSNAME FROM MI_MATCLASS WHERE MAT_CLASS=A.MAT_CLASS and rownum=1) MAT_CLASS_N,
                        (SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.FRWH and rownum=1) FRWH_N,
                        (SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.TOWH and rownum=1) TOWH_N,
                        (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='APPLY_KIND' AND DATA_VALUE=A.APPLY_KIND and rownum=1) APPLY_KIND_N ,
                        (SELECT UNA FROM UR_ID WHERE TUSER=A.APPID and rownum=1) APP_NAME,
                        (SELECT INID_NAME FROM UR_INID WHERE INID=A.APPDEPT and rownum=1) APPDEPT_NAME,
                        (SELECT DISTINCT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='STKTRANSKIND2' AND DATA_VALUE=A.STKTRANSKIND and rownum=1) STKTRANSKIND_N,
                        (SELECT AGEN_NAMEC FROM PH_VENDER WHERE AGEN_NO = A.AGEN_NO and rownum=1)AGEN_NO_N,
                        TWN_DATE(A.APPTIME) APPTIME_T
                        FROM ME_DOCM A WHERE 1=1 AND A.DOCTYPE = 'WT'
                        AND A.DOCNO = :DOCNO";
            return DBWork.Connection.Query<ME_DOCM>(sql, new { DOCNO = id }, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCEXP> GetAllD(string DOCNO, string mmcode, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.DOCNO,A.SEQ,A.EXP_DATE,A.LOT_NO,A.LOT_NO LOT_NO_N,TWN_DATE(A.EXP_DATE) EXP_DATE_T,
                        A.MMCODE,A.C_TYPE,A.C_STATUS,A.C_RNO,A.C_UP,
                        (CASE WHEN A.APVQTY < 0 THEN -1 ELSE 1 END) * A.APVQTY APVQTY, 
                        (CASE WHEN A.APVQTY < 0 THEN -1 ELSE 1 END) * A.C_AMT C_AMT,
                        ( SELECT INV_QTY FROM MI_WEXPINV 
                           WHERE MMCODE = A.MMCODE AND WH_NO = ( select frwh from ME_DOCM where docno = A.DOCNO )  
                             AND LOT_NO=A.LOT_NO
                             and twn_date(exp_date) = twn_date(a.exp_date)) AS INV_QTY,
                        B.MMNAME_C, B.MMNAME_E, B.BASE_UNIT,B.M_CONTPRICE,
                        (SELECT DATA_DESC FROM PARAM_D 
                        WHERE GRP_CODE='ME_DOCD' AND DATA_NAME='STAT' 
                        AND DATA_VALUE=A.C_TYPE) INOUT_N,
                        A.C_TYPE INOUT,
                        ITEM_NOTE,
                        (SELECT PVR.AGEN_NO || ' ' || PVR.AGEN_NAMEC
                        FROM MI_MAST MMT, PH_VENDER PVR
                        WHERE MMT.MMCODE = A.MMCODE
                        AND MMT.M_AGENNO = PVR.AGEN_NO) AGEN_NAMEC ,
                        NVL(DECODE (A.C_TYPE, 1, '', A.C_UP),0) IN_PRICE,
                        NVL(A.C_UP,0) CONTPRICE   
                        FROM ME_DOCEXP A
                        INNER JOIN MI_MAST B ON B.MMCODE=A.MMCODE
                        INNER JOIN ME_DOCM C ON C.DOCNO=A.DOCNO
                        WHERE 1=1 ";

            if (DOCNO != "")
            {
                sql += " AND A.DOCNO = :p0 ";
                p.Add(":p0", string.Format("{0}", DOCNO));
            }
            else
            {
                sql += " AND 1=2 ";
            }

            if (mmcode != "")
            {
                sql += " AND A.MMCODE LIKE :p1 ";
                p.Add(":p1", string.Format("%{0}%", mmcode));
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCEXP>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCEXP> GetD(string id)
        {
            var sql = @"SELECT A.DOCNO,A.SEQ,A.EXP_DATE,A.LOT_NO,A.LOT_NO LOT_NO_N,TWN_DATE(A.EXP_DATE) EXP_DATE_T,
                        A.MMCODE,A.C_TYPE,A.C_STATUS,A.C_RNO,A.C_UP,
                        (CASE WHEN A.APVQTY < 0 THEN -1 ELSE 1 END) * A.APVQTY APVQTY, 
                        (CASE WHEN A.APVQTY < 0 THEN -1 ELSE 1 END) * A.C_AMT C_AMT,
                        ( SELECT INV_QTY FROM MI_WEXPINV 
                           WHERE MMCODE = A.MMCODE AND WH_NO = ( select frwh from ME_DOCM where docno = A.DOCNO )  
                             AND LOT_NO=A.LOT_NO
                             and twn_date(exp_date) = twn_date(a.exp_date)) AS INV_QTY,
                        B.MMNAME_C, B.MMNAME_E, B.BASE_UNIT,B.M_CONTPRICE,
                        (SELECT DATA_DESC FROM PARAM_D 
                        WHERE GRP_CODE='ME_DOCD' AND DATA_NAME='STAT' 
                        AND DATA_VALUE=A.C_TYPE) INOUT_N,
                        A.C_TYPE INOUT,
                        ( SELECT SUM( X.APVQTY * Y.M_CONTPRICE ) FROM ME_DOCEXP X,MI_MAST Y WHERE X.MMCODE = Y.MMCODE AND X.DOCNO = A.DOCNO ) BALANCE,
                        ITEM_NOTE,
                        (SELECT PVR.AGEN_NO || ' ' || PVR.AGEN_NAMEC
                        FROM MI_MAST MMT, PH_VENDER PVR
                        WHERE MMT.MMCODE = A.MMCODE
                        AND MMT.M_AGENNO = PVR.AGEN_NO) AGEN_NAMEC ,
                        NVL(DECODE (A.C_TYPE, 1, '', A.C_UP),0) IN_PRICE,
                        NVL(A.C_UP,0) CONTPRICE   
                        FROM ME_DOCEXP A
                        INNER JOIN MI_MAST B ON B.MMCODE=A.MMCODE
                        INNER JOIN ME_DOCM C ON C.DOCNO=A.DOCNO
                        WHERE 1=1 
                        AND A.DOCNO = :DOCNO";
            return DBWork.Connection.Query<ME_DOCEXP>(sql, new { DOCNO = id }, DBWork.Transaction);
        }

        public int CreateM(ME_DOCM ME_DOCM)
        {
            var sql = @"INSERT INTO ME_DOCM (
                        DOCNO, DOCTYPE, FLOWID , APPID , APPDEPT , 
                        APPTIME , USEID , USEDEPT , FRWH , TOWH , 
                        APPLY_NOTE ,MAT_CLASS, 
                        CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                      VALUES (
                        :DOCNO, :DOCTYPE, :FLOWID , :APPID , USER_INID(:APPID), 
                        TO_DATE(:APPTIME,'YYYY/MM/DD') , :USEID , USER_INID(:APPID) , :FRWH , :TOWH , 
                        :APPLY_NOTE ,:MAT_CLASS,
                        SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);
        }

        public int CreateE(ME_DOCEXP docexp)
        {
            string sql = @" INSERT INTO ME_DOCEXP (
                        DOCNO, SEQ, MMCODE , APVQTY , ITEM_NOTE, 
                        C_UP, C_AMT, EXP_DATE, LOT_NO,C_TYPE,
                        UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                      VALUES (
                        :DOCNO, :SEQ, :MMCODE , :APVQTY , :ITEM_NOTE, 
                        :C_UP, :C_AMT, TO_DATE(:EXP_DATE,'YYYY/MM/DD'), :LOT_NO, :C_TYPE,
                        SYSDATE, :UPDATE_USER, :UPDATE_IP) ";
            return DBWork.Connection.Execute(sql, docexp, DBWork.Transaction);
        }

        public int CreateD(ME_DOCD docd)
        {
            var sql = @"INSERT INTO ME_DOCD (
                        DOCNO, SEQ, MMCODE ,APPQTY, APLYITEM_NOTE,
                         CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                      VALUES (
                        :DOCNO, :SEQ, :MMCODE , :APPQTY, :APLYITEM_NOTE,
                        SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, docd, DBWork.Transaction);
        }

        public int CreateWhinv(ME_DOCD docd)
        {
            var sql = @"INSERT INTO MI_WHINV 
                        (WH_NO,MMCODE,INV_QTY,ONWAY_QTY, MIL_INQTY, MIL_OUTQTY)  
                      VALUES (
                        :WH_NO, :MMCODE, :APPQTY, 0, 
                        (case when :APPQTY >= 0 then to_number(:APPQTY) else 0 end), 
                        (case when :APPQTY < 0 then to_number(:APPQTY) * -1 else 0 end) )";
            return DBWork.Connection.Execute(sql, docd, DBWork.Transaction);
        }

        public int CreateWexpinv(string wh_no, string mmcode, string lot_no, string exp_date, string qty, string userId, string updateIp)
        {
            string sql = @"
                insert into MI_WEXPINV (WH_NO, MMCODE, EXP_DATE, LOT_NO, INV_QTY,
                                        CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, CREATE_TIME)
                values (:WH_NO, :MMCODE, to_date(:EXP_DATE, 'yyyy/mm/dd'), :LOT_NO, :INV_QTY,
                        :TUSER, sysdate, :TUSER, :PROCIP, sysdate)
            ";
            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, MMCODE = mmcode, LOT_NO = lot_no, EXP_DATE = exp_date, INV_QTY = qty, TUSER = userId, PROCIP = updateIp }, DBWork.Transaction);
        }

        public int CreateWhtrns(ME_DOCD docd)
        {
            var sql = @"INSERT INTO MI_WHTRNS 
                        (WH_NO, TR_DATE, TR_SNO, MMCODE, TR_INV_QTY, TR_ONWAY_QTY, TR_DOCNO, TR_DOCSEQ, TR_FLOWID, TR_DOCTYPE,
                        TR_IO, TR_MCODE, BF_TR_INVQTY, AF_TR_INVQTY)
                      VALUES (
                        :WH_NO, sysdate, WHTRNS_SEQ.nextval, :MMCODE, :APPQTY, 0, :DOCNO, :SEQ, '2', 'WT',
                        (case when :APPQTY >= 0 then 'I' else 'O' end), (case when :APPQTY >= 0 then 'MILI' else 'MILO' end), :INV_QTY, :INV_QTY + :APPQTY) ";
            return DBWork.Connection.Execute(sql, docd, DBWork.Transaction);
        }

        public int UpdateM(ME_DOCM ME_DOCM)
        {
            var sql = @"UPDATE ME_DOCM SET 
                        APPID = :APPID,
                        APPDEPT = USER_INID(:APPID),
                        APPTIME = SYSDATE,
                        USEID   = :APPID,
                        USEDEPT = USER_INID(:APPID),
                        APPLY_NOTE = :APPLY_NOTE,
                        STKTRANSKIND = :STKTRANSKIND,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);
        }

        public int UpdateE(ME_DOCEXP docexp)
        {
            var sql = @"UPDATE ME_DOCEXP SET MMCODE = :MMCODE, C_TYPE = :INOUT,
                        APVQTY = :APVQTY, ITEM_NOTE = :ITEM_NOTE, C_UP = :C_UP, C_AMT = :C_AMT,
                        LOT_NO = :LOT_NO, EXP_DATE = TWN_TODATE(:EXP_DATE),
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO AND SEQ = :SEQ ";
            return DBWork.Connection.Execute(sql, docexp, DBWork.Transaction);
        }

        public int UpdateWhinv(ME_DOCD docd)
        {
            var sql = @"UPDATE MI_WHINV SET INV_QTY = nvl(INV_QTY, 0) + :APPQTY,
                        MIL_INQTY = MIL_INQTY + (case when :APPQTY >= 0 then to_number(:APPQTY) else 0 end),
                        MIL_OUTQTY = MIL_OUTQTY + (case when :APPQTY < 0 then to_number(:APPQTY) * -1 else 0 end)
                      WHERE WH_NO = :WH_NO AND MMCODE = :MMCODE ";
            return DBWork.Connection.Execute(sql, docd, DBWork.Transaction);
        }

        public int UpdateWexpinv(string wh_no, string mmcode, string lot_no, string exp_date, string qty, string userId, string updateIp)
        {
            string sql = @"
                update MI_WEXPINV
                   set INV_QTY = INV_QTY + :INV_QTY ,
                       UPDATE_TIME = sysdate,
                       UPDATE_USER = :TUSER,
                       UPDATE_IP = :PROCIP
                 where WH_NO = :WH_NO
                   and MMCODE = :MMCODE
                   and LOT_NO = :LOT_NO
                   and EXP_DATE = to_date(:EXP_DATE, 'yyyy/mm/dd')
            ";
            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, MMCODE = mmcode, LOT_NO = lot_no, EXP_DATE = exp_date, INV_QTY = qty, TUSER = userId, PROCIP = updateIp }, DBWork.Transaction);
        }

        public int UpdateWloc(string wh_no, string mmcode, string invqty, string tuser, string userIp)
        {
            string sql = @" update MI_WLOCINV A
                set INV_QTY = INV_QTY + :INVQTY, UPDATE_USER = :TUSER, UPDATE_TIME = sysdate, UPDATE_IP = :USERIP
                where WH_NO = :WH_NO and MMCODE = :MMCODE 
                and STORE_LOC = (select STORE_LOC from MI_WLOCINV where WH_NO = A.WH_NO and MMCODE = A.MMCODE and rownum = 1)
                ";
            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, MMCODE = mmcode, INVQTY = invqty, TUSER = tuser, USERIP = userIp }, DBWork.Transaction);
        }

        public int UpdateDocmFlowid(string docno, string flowid, string tuser, string procIp)
        {
            var sql = @"UPDATE ME_DOCM SET FLOWID = :FLOWID, POST_TIME = sysdate, 
                        SET_YM = (select SET_YM from MI_MNSET where SET_STATUS = 'N'), UPDATE_TIME = sysdate, UPDATE_USER = :TUSER, UPDATE_IP = :PROCIP
                      WHERE DOCNO = :DOCNO ";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno, FLOWID = flowid, TUSER = tuser, PROCIP = procIp }, DBWork.Transaction);
        }

        public int DeleteM(ME_DOCM me_docm)
        {
            var sql = @"UPDATE ME_DOCM SET FLOWID = 'X',
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);
        }

        public int DeleteD(string docno, string seq)
        {
            var sql = @" DELETE from ME_DOCEXP WHERE DOCNO=:DOCNO AND SEQ=:SEQ";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno, SEQ = seq }, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetMatclassCombo(string id)
        {
            string sql = @"with temp_whkinds as (
                    select b.wh_no, b.wh_kind, nvl((case when a.task_id = '3' then '2' else a.task_id end), '2') as task_id
                    from MI_WHID a, MI_WHMAST b
                    where wh_userid = :TUESR
                    and a.wh_no = b.wh_no
                    and b.wh_grade = '1'
                    and a.task_id in ('1','2','3')
                    )
                    select distinct b.mat_class as value,b.mat_clsname as text,b.mat_class || ' ' ||  b.mat_clsname as COMBITEM 
                    from temp_whkinds a, MI_MATCLASS b
                    where (a.task_id = b.mat_clsid)
                    ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { TUESR = id });
        }

        public IEnumerable<AB0003Model> GetLoginInfo(string id, string ip)
        {
            string sql = @"SELECT TUSER AS USERID, UNA AS USERNAME, INID, INID_NAME(INID) AS INIDNAME,
                        WHNO_MM1 CENTER_WHNO,INID_NAME(WHNO_MM1) AS CENTER_WHNAME, TO_CHAR(SYSDATE,'YYYYMMDD') AS TODAY,
                        TWN_SYSDATE,
                        :UPDATE_IP,
                        (SELECT COUNT(*) AS CNT FROM ME_DOCM 
                            WHERE DOCTYPE='MR2' AND APPLY_KIND='1' 
                            AND APPTIME BETWEEN NEXT_DAY(SYSDATE-7,1) AND NEXT_DAY(SYSDATE-7,1)+6 ) MR2,
                        NVL((SELECT 'Y' FROM DUAL WHERE (SELECT EXTRACT(DAY FROM SYSDATE) FROM DUAL) 
                            BETWEEN (SELECT DATA_VALUE FROM PARAM_D WHERE DATA_NAME = 'MR3_BDAY')
                            AND (SELECT DATA_VALUE FROM PARAM_D WHERE DATA_NAME = 'MR3_EDAY')),'N') MR3,
                        NVL((SELECT 'Y' FROM DUAL WHERE (SELECT EXTRACT(DAY FROM SYSDATE) FROM DUAL) 
                            BETWEEN (SELECT DATA_VALUE FROM PARAM_D WHERE DATA_NAME = 'MR4_BDAY')
                            AND (SELECT DATA_VALUE FROM PARAM_D WHERE DATA_NAME = 'MR4_EDAY')),'N') MR4 
                        FROM UR_ID
                        WHERE UR_ID.TUSER=:TUSER";

            return DBWork.Connection.Query<AB0003Model>(sql, new { TUSER = id, UPDATE_IP = ip });
        }

        public IEnumerable<COMBO_MODEL> GetFlowidCombo(string id)
        {
            string sql = @"select FLOWID as VALUE, FLOWNAME as TEXT,FLOWID || ' ' || FLOWNAME as COMBITEM
                        from ME_FLOW
                        where DOCTYPE = 'WT'
                        ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { TUSER = id });
        }

        public IEnumerable<MI_MAST> GetMmcode(ME_DOCM query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT,A.M_CONTPRICE,A.DISC_UPRICE, 
                            ( CASE WHEN A.M_STOREID = '1'
                                AND A.M_CONTID <> '3'
                                AND A.M_APPLYID <> 'E' THEN 'Y' ELSE 'X' END) AS M_PAYID,
                            (SELECT DATA_VALUE||' '||DATA_DESC SPE 
                            FROM PARAM_D WHERE GRP_CODE='MI_MAST' 
                            AND DATA_NAME='M_STOREID' AND DATA_VALUE=A.M_STOREID) M_STOREID,
                            (SELECT DATA_VALUE||' '||DATA_DESC SPE 
                            FROM PARAM_D WHERE GRP_CODE='MI_MAST' 
                            AND DATA_NAME='M_CONTID' AND DATA_VALUE=A.M_CONTID) M_CONTID,
                            (SELECT DATA_VALUE||' '||DATA_DESC SPE 
                            FROM PARAM_D WHERE GRP_CODE='MI_MAST' 
                            AND DATA_NAME='M_APPLYID' AND DATA_VALUE=A.M_APPLYID) M_APPLYID,
                            A.M_AGENNO,
                            (select AGEN_NAMEC from PH_VENDER where AGEN_NO = A.M_AGENNO) as AGEN_NAMEC,
                            (select AGEN_NAMEE from PH_VENDER where AGEN_NO = A.M_AGENNO) as AGEN_NAMEE 
                        FROM MI_MAST A WHERE 1=1 
                        AND A.MAT_CLASS = :MAT_CLASS  
                        AND EXISTS ( SELECT 1 FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO = (select frwh from ME_DOCM where docno = :docno) )
                            ";
            p.Add(":MAT_CLASS", query.MAT_CLASS);
            p.Add(":DOCNO", query.DOCNO);

            if (query.MMCODE != "")
            {
                sql += " AND UPPER(A.MMCODE) LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("%{0}%", query.MMCODE));
            }

            if (query.MMNAME_C != "")
            {
                sql += " AND A.MMNAME_C LIKE :MMNAME_C ";
                p.Add(":MMNAME_C", string.Format("%{0}%", query.MMNAME_C));
            }

            if (query.MMNAME_E != "")
            {
                sql += " AND UPPER(A.MMNAME_E) LIKE UPPER(:MMNAME_E) ";
                p.Add(":MMNAME_E", string.Format("%{0}%", query.MMNAME_E));
            }

            if (query.M_AGENNO != "")
            {
                sql += " AND A.M_AGENNO LIKE :M_AGENNO ";
                p.Add(":M_AGENNO", string.Format("%{0}%", query.M_AGENNO));
            }
            if (query.AGEN_NAME != "")
            {
                sql += " AND ((select AGEN_NAMEC from PH_VENDER where AGEN_NO = A.M_AGENNO) LIKE :AGEN_NAME ";
                sql += "  OR (select AGEN_NAMEE from PH_VENDER where AGEN_NO = A.M_AGENNO) LIKE :AGEN_NAME) ";
                p.Add(":AGEN_NAME", string.Format("%{0}%", query.AGEN_NAME));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMmCodeCombo(string p0, string p1, string docno, int page_index, int page_size, string sorters)
        {

            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT, A.M_CONTPRICE, 
                        ( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO = (select FRWH from ME_DOCM where DOCNO = :DOCNO) AND ROWNUM=1) AS INV_QTY,
                        B.AGEN_NO || ' ' || B.AGEN_NAMEC AGEN_NAMEC,
                        A.UPRICE  as M_DISCPERC 
                        FROM MI_MAST A, PH_VENDER B WHERE 1=1 AND A.M_AGENNO = B.AGEN_NO AND A.MAT_CLASS = :MAT_CLASS 
                        AND EXISTS ( SELECT 1 FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO = (select FRWH from ME_DOCM where DOCNO = :DOCNO) )
                            ";
            p.Add(":DOCNO", docno);

            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(MMCODE, UPPER(:MMCODE_I)), 1000) + NVL(INSTR(MMNAME_E, UPPER(:MMNAME_E_I)), 100) * 10 + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (MMCODE LIKE UPPER(:MMCODE) ";
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

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetSelectMmcodeDetail(string MMCODE, string MAT_CLASS, string DOCNO)
        {
            var p = new DynamicParameters();

            string sql = @"SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT, A.M_CONTPRICE, 
                        ( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO = (select FRWH from ME_DOCM where docno = :DOCNO) AND ROWNUM=1) AS INV_QTY,
                        B.AGEN_NO || ' ' || B.AGEN_NAMEC AGEN_NAMEC,
                        A.UPRICE  as M_DISCPERC 
                        FROM MI_MAST A, PH_VENDER B WHERE 1=1 AND A.M_AGENNO = B.AGEN_NO AND A.MAT_CLASS = :MAT_CLASS 
                        AND EXISTS ( SELECT 1 FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO = (select FRWH from ME_DOCM where docno = :DOCNO) )
                        AND A.MMCODE = :MMCODE ";

            p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS));
            p.Add(":DOCNO", string.Format("{0}", DOCNO));
            p.Add(":MMCODE", string.Format("{0}", MMCODE));

            return DBWork.Connection.Query<MI_MAST>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMMCodeDocd(string p0, string p1, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E
                        FROM MI_MAST A, ME_DOCEXP B WHERE A.MMCODE=B.MMCODE AND B.DOCNO = :DOCNO  ";
            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(UPPER(A.MMCODE), UPPER(:MMCODE_I)), 1000) + NVL(INSTR(UPPER(A.MMNAME_E), UPPER(:MMNAME_E_I)), 100) * 10 + NVL(INSTR(UPPER(A.MMNAME_C), UPPER(:MMNAME_C_I)), 100) * 10) IDX,");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND ( UPPER(A.MMCODE) LIKE UPPER(:MMCODE) ";
                p.Add(":MMCODE", string.Format("{0}%", p0));

                sql += " OR UPPER(A.MMNAME_E) LIKE UPPER(:MMNAME_E) ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR UPPER(A.MMNAME_C) LIKE UPPER(:MMNAME_C)) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY A.MMCODE ";
            }
            p.Add(":DOCNO", p1);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //批號+效期+效期數量combox
        public IEnumerable<MI_WEXPINV> GetLOT_NO(string frwh, string mmcode)
        {
            string sql = @"  SELECT LOT_NO,
                                    twn_date(EXP_DATE) EXP_DATE,
                                    INV_QTY
                             FROM MI_WEXPINV
                             WHERE WH_NO = :FRWH
                             AND MMCODE = :MMCODE";

            return DBWork.Connection.Query<MI_WEXPINV>(sql, new { FRWH = frwh, MMCODE = mmcode }, DBWork.Transaction);
        }

        public string GetWhno_me1()
        {
            string sql = @"SELECT WHNO_ME1 FROM DUAL ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction).ToString();
            return rtn;
        }
        public string GetWhno_mm1()
        {
            string sql = @"SELECT WHNO_MM1 FROM DUAL ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction).ToString();
            return rtn;
        }

        //國軍DOCNO單號統一用GET_DAILY_DOCNO
        public string GetDailyDocno()
        {
            string sql = @"select GET_DAILY_DOCNO from DUAL";
            string rtn = DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction).ToString();
            return rtn;
        }

        public string GetDocESeq(string id)
        {
            string sql = @"SELECT MAX(SEQ)+1 as SEQ FROM ME_DOCEXP WHERE DOCNO=:DOCNO ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction).ToString();
            return rtn;
        }

        public string GetDocDSeq(string id)
        {
            string sql = @"SELECT MAX(SEQ)+1 as SEQ FROM ME_DOCD WHERE DOCNO=:DOCNO ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction).ToString();
            return rtn;
        }

        public string GetDocmTowh(string docno)
        {
            string sql = @"select TOWH from ME_DOCM where DOCNO=:DOCNO";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { DOCNO = docno }, DBWork.Transaction).ToString();
            return rtn;
        }

        public string GetWhinvQty(string wh_no, string mmcode)
        {
            string sql = @"SELECT INV_QTY
                            FROM MI_WHINV
                        WHERE WH_NO=:WH_NO AND MMCODE=:MMCODE";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { WH_NO = wh_no, MMCODE = mmcode }, DBWork.Transaction).ToString();
            return rtn;
        }

        public IEnumerable<ME_DOCEXP> GetLotExp(string docno, string mmcode)
        {
            string sql = @"SELECT LOT_NO, to_char(EXP_DATE, 'YYYY/MM/DD') as EXP_DATE from MI_WEXPINV where WH_NO = (select TOWH from ME_DOCM where DOCNO = :DOCNO) and MMCODE = :MMCODE and rownum = 1 order by EXP_DATE asc ";
            return DBWork.Connection.Query<ME_DOCEXP>(sql, new { DOCNO = docno, MMCODE = mmcode }, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCEXP> GetDocE_detail(string docno)
        {
            string sql = @"SELECT MMCODE, nvl(SUM(APVQTY), 0) as APVQTY,
                        LISTAGG(NVL(ITEM_NOTE,' '), ' ') WITHIN GROUP (ORDER BY MMCODE, C_TYPE) AS ITEM_NOTE
                        FROM ME_DOCEXP WHERE DOCNO=:DOCNO
                        GROUP BY MMCODE, C_TYPE ";
            return DBWork.Connection.Query<ME_DOCEXP>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCD> GetDocd_detail(string docno)
        {
            string sql = @" select DOCNO, SEQ, MMCODE, STAT, NVL(APPQTY,0) as APPQTY, NVL(APVQTY,0) as APVQTY, NVL(ACKQTY,0) as ACKQTY,
                         NVL(EXPT_DISTQTY,0) as EXPT_DISTQTY, NVL(PICK_QTY,0) as PICK_QTY, NVL(ONWAY_QTY,0) as ONWAY_QTY,
                         NVL(BW_MQTY,0) as BW_MQTY, NVL(AMT,0) as AMT, FRWH_D, TRANSKIND,POSTID
                    FROM ME_DOCD WHERE DOCNO=:DOCNO ";
            return DBWork.Connection.Query<ME_DOCD>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCEXP> GetMeDocExp(string docno)
        {
            string sql = @"
                select DOCNO, MMCODE, LOT_NO, to_char(EXP_DATE, 'yyyy/mm/dd') as EXP_DATE, APVQTY
                  from ME_DOCEXP
                 where DOCNO = :DOCNO
            ";
            return DBWork.Connection.Query<ME_DOCEXP>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public bool CheckExistsM(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCM WHERE DOCNO=:DOCNO";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }

        public bool CheckExistsD(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }

        public bool CheckFlowId(string docno)
        {
            string sql = @"
                select 1 from ME_DOCM
                 where docno = :docno
                   and flowId = '1'
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { docno }, DBWork.Transaction) != null;
        }

        public bool CheckExistsMMCODE(string id)
        {
            string sql = @"SELECT 1 FROM MI_MAST WHERE MMCODE=:MMCODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = id }, DBWork.Transaction) == null);
        }

        public bool CheckExistsMM(string id, string mm, string lot, string exp)
        {
            string sql = @"SELECT 1 FROM ME_DOCEXP 
                           WHERE DOCNO=:DOCNO 
                             AND MMCODE=:MMCODE 
                             AND LOT_NO=:LOT_NO 
                             AND TWN_DATE(EXP_DATE)=:EXP_DATE ";

            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id, MMCODE = mm, LOT_NO = lot, EXP_DATE = exp }, DBWork.Transaction) == null);
        }

        public bool CheckExistsE(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCEXP WHERE DOCNO=:DOCNO ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }

        public bool CheckExistsDetail(string doc, string mmcode)
        {
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO AND MMCODE=:MMCODE ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = doc, MMCODE = mmcode }, DBWork.Transaction) == null);
        }

        public bool CheckExistsWexpinv(string wh_no, string mmcode, string lot_no, string exp_date)
        {
            string sql = @"
                select 1 from MI_WEXPINV
                 where wh_no = :WH_NO
                   and mmcode = :MMCODE
                   and lot_no = :LOT_NO
                   and exp_date = to_date(:EXP_DATE,'yyyy/mm/dd')
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { WH_NO = wh_no, MMCODE = mmcode, LOT_NO = lot_no, EXP_DATE = exp_date }, DBWork.Transaction) != null;
        }

        public bool CheckExistsWhinv(string wh_no, string mmcode)
        {
            string sql = @"SELECT 1 FROM MI_WHINV WHERE WH_NO=:WH_NO AND MMCODE=:MMCODE ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { WH_NO = wh_no, MMCODE = mmcode }, DBWork.Transaction) == null);
        }
    }
}