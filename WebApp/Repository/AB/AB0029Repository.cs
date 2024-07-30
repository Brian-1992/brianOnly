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
    public class AB0029Repository : JCLib.Mvc.BaseRepository
    {
        public AB0029Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ME_DOCM> GetAllM(string Qflowid, string tuser, string DOCNO, string APPTIME_S, string APPTIME_E, string FRWH, string APPID, string APPDEPT, string TOWH, string FLOWID, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            //LIST_ID 拿來當是否有效期院內碼
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT DOCNO,TWN_DATE(A.APPTIME) APPTIME,UNA AS APPID,INID_NAME AS APPDEPT");
            sql.Append(",A.FRWH,(SELECT WH_NO||' '||WH_NAME from MI_WHMAST where WH_NO=A.FRWH) FRWH_N");
            sql.Append(",A.TOWH,(SELECT WH_NO||' '||WH_NAME from MI_WHMAST where WH_NO=A.TOWH) TOWH_N");
            sql.Append(",A.FLOWID,case when A.FLOWID='0201' then '0201 調撥申請' when A.FLOWID='0202' then '0202 調出中'");
            sql.Append(" when A.FLOWID='0203' then '0203 調入中' when A.FLOWID='0204' then '0204 調出中' when A.FLOWID='0299' then '0299 調撥結案' end FLOWID_N");
            sql.Append(",(SELECT 'Y' FROM ME_DOCD D,MI_MAST E WHERE A.DOCNO=D.DOCNO AND E.MMCODE=D.MMCODE AND WEXP_ID='Y' AND ROWNUM<=1 )LIST_ID");
            sql.Append(@",( case when (select wh_kind from MI_WHMAST where wh_no = a.frwh) = '0'
                                    then '藥品' else '衛材、一般物品' end) as apply_kind");
            sql.Append(" FROM ME_DOCM A,UR_ID B,UR_INID C");
            sql.Append(" WHERE DOCTYPE IN ('TR','TR1') AND A.APPID=B.TUSER AND A.APPDEPT=C.INID");
            switch (Qflowid)//調撥作業入口
            {
                case "0201":
                //break;
                case "0203": //2020/8/25 開放藥庫可以調撥
                    sql.Append(" AND EXISTS(SELECT 'X' FROM MI_WHMAST D");
                    sql.Append(" WHERE D.WH_NO=A.TOWH");
                    sql.Append(" AND ((EXISTS(SELECT 'X' FROM MI_WHID E WHERE D.WH_NO=E.WH_NO AND WH_USERID=:WH_USERID))");
                    //sql.Append(" OR (D.WH_KIND='1' AND EXISTS(SELECT 'X' FROM UR_ID E WHERE D.SUPPLY_INID=E.INID AND E.TUSER=:WH_USERID))))");
                    //2020/1/30
                    sql.Append(" OR (D.WH_KIND='1' AND (EXISTS(SELECT 'X' FROM UR_ID E WHERE (D.SUPPLY_INID=E.INID OR D.INID=E.INID)AND E.TUSER=:WH_USERID");
                    sql.Append(" AND NOT EXISTS(SELECT 'X' FROM MI_WHID F WHERE F.TASK_ID IN ('2','3') AND F.WH_USERID=:WH_USERID)))");
                    sql.Append(" OR ( EXISTS(SELECT 'X' FROM MI_WHID F WHERE F.TASK_ID IN ('2','3') AND F.WH_USERID=:WH_USERID AND F.WH_NO=D.WH_NO))");
                    sql.Append(")))");
                    p.Add(":WH_USERID", string.Format("{0}", tuser));
                    break;
                case "0202":
                    sql.Append(" AND EXISTS(SELECT 'X' FROM MI_WHMAST D");
                    sql.Append(" WHERE D.WH_NO=A.FRWH");
                    sql.Append(" AND ((EXISTS(SELECT 'X' FROM MI_WHID E WHERE D.WH_NO=E.WH_NO AND WH_USERID=:WH_USERID))");
                    sql.Append(" OR (D.WH_KIND='1' AND EXISTS(SELECT 'X' FROM UR_ID E WHERE D.SUPPLY_INID=E.INID AND E.TUSER=:WH_USERID))))");
                    p.Add(":WH_USERID", string.Format("{0}", tuser));
                    break;
            }
            if (DOCNO != "")
            {
                sql.Append(" AND A.DOCNO LIKE :DOCNO");
                p.Add(":DOCNO", string.Format("{0}%", DOCNO));
            }
            if (APPTIME_S != "" & APPTIME_E != "")
            {
                sql.Append(" AND A.APPTIME BETWEEN TO_DATE(:APPTIME_S,'YYYYMMDD') AND TO_DATE(:APPTIME_E||'235959','YYYYMMDDHH24MISS') ");
                p.Add(":APPTIME_S", string.Format("{0}", APPTIME_S));
                p.Add(":APPTIME_E", string.Format("{0}", APPTIME_E));
            }
            else
            {
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
            if (APPDEPT != "")
            {
                sql.Append(" AND A.APPDEPT = :APPDEPT ");
                p.Add(":APPDEPT", string.Format("{0}", APPDEPT));
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

        public IEnumerable<MI_WEXPINV> GetAllWexp(string WH_NO, string MMCODE, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            StringBuilder sql = new StringBuilder();
            sql.Append("select EXP_DATE,INV_QTY from MI_WEXPINV where WH_NO=:WH_NO AND MMCODE=:MMCODE AND exp_date>SYSDATE");

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_WEXPINV>(GetPagingStatement(sql.ToString(), sorters), p, DBWork.Transaction);
        }
        public string GetDocno()
        {
            var p = new OracleDynamicParameters();
            p.Add("O_DOCNO", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 12);

            DBWork.Connection.Query("GET_DOCNO", p, commandType: CommandType.StoredProcedure);
            return p.Get<OracleString>("O_DOCNO").Value;
        }
        public SP_MODEL POST_DOC(string I_DOCNO, string I_UPDUSR, string I_UPDIP)
        {
            SP_MODEL sp;
            var p = new OracleDynamicParameters();
            p.Add("I_DOCNO", I_DOCNO, dbType: OracleDbType.Varchar2);
            p.Add("I_UPDUSR", I_UPDUSR, dbType: OracleDbType.Varchar2);
            p.Add("I_UPDIP", I_UPDIP, dbType: OracleDbType.Varchar2);
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
            //var sql = @"INSERT INTO ME_DOCM (
            //            DOCNO, DOCTYPE, FLOWID , APPID , APPDEPT , 
            //            APPTIME  , USEDEPT , FRWH , TOWH , 
            //            CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
            //          VALUES (
            //            :DOCNO, :DOCTYPE, :FLOWID , :APPID , :APPDEPT , 
            //            SYSDATE, :USEDEPT , :FRWH , :TOWH , 
            //            SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            var sql = @"INSERT INTO ME_DOCM (
                        DOCNO, DOCTYPE, FLOWID , APPID , APPDEPT , 
                        APPTIME   , FRWH , TOWH , MAT_CLASS,
                        CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                      SELECT 
                        :DOCNO, DECODE(WH_KIND,'0','TR','TR1'), :FLOWID , :APPID , A.INID , 
                        SYSDATE , :FRWH , WH_NO , :MAT_CLASS,
                        SYSDATE, TUSER, SYSDATE, :UPDATE_USER, :UPDATE_IP FROM UR_ID A ,MI_WHMAST B WHERE TUSER=:CREATE_USER AND B.WH_NO = :TOWH ";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);
        }
        public int CreateME_DOCC(ME_DOCC pME_DOCC)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("Insert into ME_DOCC(DOCNO,SEQ,CHECKSEQ,GENWAY,ACKQTY,ACKTIME,ACKID,CREATE_TIME,CREATE_USER,UPDATE_TIME,UPDATE_USER,UPDATE_IP)");
            sql.Append(" Values(:DOCNO,:SEQ,:CHECKSEQ,:GENWAY,:ACKQTY,SYSDATE,:ACKID,SYSDATE,:CREATE_USER,SYSDATE,:UPDATE_USER,:UPDATE_IP)");
            return DBWork.Connection.Execute(sql.ToString(), pME_DOCC, DBWork.Transaction);
        }
        public bool CheckExistsME_DOCC(string pDocno, string pSeq)
        {
            string sql = @"SELECT 1 FROM ME_DOCC WHERE DOCNO=:DOCNO AND SEQ=:SEQ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = pDocno, SEQ = pSeq }, DBWork.Transaction) == null);
        }
        public int UpdateME_DOCC(ME_DOCC pME_DOCC)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("UPDATE ME_DOCC SET");
            sql.Append(" ACKQTY=:ACKQTY,ACKTIME=SYSDATE,ACKID=:ACKID");
            sql.Append(",UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP");
            sql.Append(" WHERE DOCNO=:DOCNO AND SEQ=:SEQ");
            return DBWork.Connection.Execute(sql.ToString(), pME_DOCC, DBWork.Transaction);
        }
        public IEnumerable<ME_DOCM> GetM(string id)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT DOCNO,TWN_DATE(APPTIME)APPTIME,UNA AS APPID,INID_NAME AS APPDEPT");
            sql.Append(",A.FRWH,(SELECT WH_NO||' '||WH_NAME from MI_WHMAST where WH_NO=A.FRWH) FRWH_N");
            sql.Append(",A.TOWH,(SELECT WH_NO||' '||WH_NAME from MI_WHMAST where WH_NO=A.TOWH) TOWH_N");
            sql.Append(",A.FLOWID,case when A.FLOWID='0201' then '0201 調撥申請' when A.FLOWID='0202' then '0202 調出中'");
            sql.Append(" when A.FLOWID='0203' then '0203 調入中' when A.FLOWID='0204' then '0204 調出中' when A.FLOWID='0299' then '0299 調撥結案' end FLOWID_N");
            sql.Append(",(SELECT 'Y' FROM ME_DOCD D,MI_MAST E WHERE A.DOCNO=D.DOCNO AND E.MMCODE=D.MMCODE AND WEXP_ID='Y' AND ROWNUM<=1 )LIST_ID");
            sql.Append(@",( case when (select wh_kind from MI_WHMAST where wh_no = a.frwh) = '0'
                                    then '藥品' else '衛材' end) as apply_kind");
            sql.Append(" FROM ME_DOCM A,UR_ID B,UR_INID C");
            sql.Append(" WHERE A.DOCNO = :DOCNO AND A.APPID=B.TUSER AND A.APPDEPT=C.INID");
            return DBWork.Connection.Query<ME_DOCM>(sql.ToString(), new { DOCNO = id }, DBWork.Transaction);
        }
        public int DeleteM(ME_DOCM ME_DOCM)
        {
            var sql = @"DELETE ME_DOCM WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);
        }
        public int UpdateFLOWID(ME_DOCM ME_DOCM)
        {
            var sql = @"UPDATE ME_DOCM SET 
                        FLOWID = :FLOWID,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);
        }
        public int UpdateFLOWID0204(ME_DOCM ME_DOCM)
        {
            var sql = @"UPDATE ME_DOCM SET 
                        FLOWID = :FLOWID, 
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO and FLOWID = '0203' ";
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
        public int UpdateALLApvQtyForCancel(ME_DOCM pME_DOCM)
        {
            var sql = @"UPDATE ME_DOCD SET 
                        APVQTY = 0,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO ";
            return DBWork.Connection.Execute(sql, pME_DOCM, DBWork.Transaction);
        }
        public int ClearAcktime(ME_DOCM ME_DOCM)
        {
            var sql = @"UPDATE ME_DOCD SET 
                        ACKTIME = null, 
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO ";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);
        }
        public int UpdateALLApvid(ME_DOCD pME_DOCD)
        {//2019/8/13 核撥時,點收數量等於核撥數量
            var sql = @"UPDATE ME_DOCD SET 
                        APVTIME = SYSDATE,APVID = :APVID,
                        ACKQTY = APVQTY,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO ";
            return DBWork.Connection.Execute(sql, pME_DOCD, DBWork.Transaction);
        }
        public int UpdateALLAck(ME_DOCD pME_DOCD)
        {
            var sql = @"UPDATE ME_DOCD SET 
                        ACKTIME = SYSDATE,ACKID = :ACKID,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO AND ACKTIME IS NULL";
            return DBWork.Connection.Execute(sql, pME_DOCD, DBWork.Transaction);
        }
        public int UpdateM(ME_DOCM ME_DOCM)
        {//                        TOWH = :TOWH,
            var sql = @"UPDATE ME_DOCM SET 
                        FRWH = :FRWH,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);
        }
        public bool CheckExistsM(string id, string flowid)
        {
            string sql = @"SELECT 1 FROM ME_DOCM WHERE DOCNO=:DOCNO AND FLOWID <> :FLOWID";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id, FLOWID = flowid }, DBWork.Transaction) == null);
        }
        //public bool CheckExistsD(string id)
        //{
        //    string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO ";
        //    return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        //}
        //public bool CheckExistsDAPVQTY(string id)
        //{
        //    string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO AND APVQTY=0";
        //    return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        //}

        public bool CheckExistsDKey(string id, string mmcode)
        {
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO AND MMCODE=:MMCODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id, MMCODE = mmcode }, DBWork.Transaction) == null);
        }
        public int DeleteAllD(ME_DOCM pME_DOCM)
        {
            var sql = @"DELETE ME_DOCD WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, pME_DOCM, DBWork.Transaction);
        }
        public int DeleteD(ME_DOCD ME_DOCD)
        {
            var sql = @"DELETE ME_DOCD WHERE DOCNO = :DOCNO AND SEQ = :SEQ";
            return DBWork.Connection.Execute(sql, ME_DOCD, DBWork.Transaction);
        }
        public IEnumerable<COMBO_MODEL> GetInidCombo(string flowid, string id)
        {
            StringBuilder sql = new StringBuilder();
            DynamicParameters p = new DynamicParameters();
            sql.Append("select inid VALUE ,inid ||' ' || inid_name TEXT from UR_INID A");
            //switch (flowid) 是否要卡自己部門申請
            //{
            //    case "0201":
            //        sql.Append(" WHERE exists(select 'x' from ur_id b where a.inid=b.inid and tuser=:tuser)");
            //        p.Add(":tuser", id);
            //        break;
            //}
            sql.Append(" ORDER BY 1");

            return DBWork.Connection.Query<COMBO_MODEL>(sql.ToString(), p);
        }

        public IEnumerable<COMBO_MODEL> GetTowhCombo(string flowid, string id)
        {
            DynamicParameters p = new DynamicParameters();
            StringBuilder sql = new StringBuilder();
            sql.Append("select A.WH_NO VALUE , WH_NO||' '||WH_NAME TEXT ,WH_KIND COMBITEM from MI_WHMAST A");
            sql.Append(" WHERE WH_KIND='0' and a.cancel_id = 'N' and a.wh_grade in ('2','3','4')");
            switch (flowid)//藥品庫
            {
                case "0201":
                case "0203":
                    sql.Append("  AND EXISTS(SELECT 'X' FROM MI_WHID B WHERE A.WH_NO=B.WH_NO AND WH_USERID=:WH_USERID)");
                    p.Add(":WH_USERID", string.Format("{0}", id));
                    break;
            }
            sql.Append(" UNION ALL ");
            sql.Append("select A.WH_NO VALUE , WH_NO||' '||WH_NAME TEXT ,WH_KIND COMBITEM from MI_WHMAST A");
            sql.Append(" WHERE WH_KIND='1' and a.cancel_id = 'N' and a.wh_grade in ('2','3','4')");
            switch (flowid)//衛材庫
            {
                case "0201":
                case "0203":
                    //sql.Append(" AND EXISTS(SELECT 'X' FROM UR_ID B WHERE A.SUPPLY_INID=B.INID AND TUSER=:TUSER)");
                    //2020/1/30
                    sql.Append(" AND EXISTS(SELECT 'X' FROM UR_ID B WHERE (A.SUPPLY_INID=B.INID OR A.INID=B.INID)AND TUSER=:TUSER)");
                    sql.Append(" AND NOT EXISTS(SELECT 'X' FROM MI_WHID B");
                    sql.Append(" WHERE TASK_ID IN ('2','3') AND WH_USERID=:TUSER)");
                    sql.Append(" UNION ALL ");
                    sql.Append(" SELECT A.WH_NO ,A.WH_NO||' '||WH_NAME,WH_KIND FROM MI_WHMAST A,MI_WHID B");
                    sql.Append(" WHERE A.WH_NO=B.WH_NO AND TASK_ID IN ('2','3') AND WH_USERID=:TUSER and a.cancel_id = 'N' and a.wh_grade in ('2','3','4')");
                    p.Add(":TUSER", string.Format("{0}", id));
                    break;
            };
            sql.Append(" ORDER BY 2");

            return DBWork.Connection.Query<COMBO_MODEL>(sql.ToString(), p);
        }

        public IEnumerable<COMBO_MODEL> GetQFrwhCombo(string flowid, string id)
        {
            DynamicParameters p = new DynamicParameters();
            StringBuilder sql = new StringBuilder();
            sql.Append("select WH_NO VALUE,WH_NO||' ' ||WH_NAME TEXT from MI_WHMAST A WHERE WH_KIND='0' and a.wh_grade in ('2','3','4')");
            switch (flowid)
            {
                case "0202":
                    sql.Append(" AND EXISTS(SELECT 'X' FROM MI_WHID B WHERE A.WH_NO=B.WH_NO AND WH_USERID=:WH_USERID)");
                    p.Add(":WH_USERID", string.Format("{0}", id));
                    break;
            }
            sql.Append(" UNION ALL ");
            sql.Append("select WH_NO VALUE,WH_NO||' ' ||WH_NAME TEXT from MI_WHMAST A WHERE WH_KIND='1' and a.wh_grade in ('2','3','4')");
            switch (flowid)
            {
                case "0202":
                    sql.Append(" AND EXISTS(SELECT 'X' FROM UR_ID B WHERE A.SUPPLY_INID=B.INID AND TUSER=:TUSER)");
                    p.Add(":TUSER", string.Format("{0}", id));
                    break;
            }
            sql.Append("  ORDER BY 2");
            return DBWork.Connection.Query<COMBO_MODEL>(sql.ToString(), p);
        }
        public IEnumerable<COMBO_MODEL> GetFrwhCombo(string action, string wh_no)
        {
            DynamicParameters p = new DynamicParameters();
            StringBuilder sql = new StringBuilder();
            sql.Append("select WH_NO VALUE,WH_NO||' '||WH_NAME TEXT from MI_WHMAST A where 1=1 and a.wh_grade in ('2','3','4')");
            if (action == "new")
            {
                sql.Append(" and EXISTS(SELECT 'X' FROM MI_WHMAST B WHERE B.WH_NO=:WH_NO AND A.WH_KIND=B.WH_KIND AND A.WH_GRADE=B.WH_GRADE AND A.WH_NO<>B.WH_NO and b.cancel_id = 'N')");
                p.Add(":WH_NO", string.Format("{0}", wh_no));
            }
            sql.Append("  ORDER BY 2");

            return DBWork.Connection.Query<COMBO_MODEL>(sql.ToString(), p);
        }

        public IEnumerable<MI_MAST> GetMmcode(MI_MAST_QUERY_PARAMS query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            string sql = @"SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT,A.M_CONTPRICE,A.DISC_UPRICE, 
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
                                INV_QTY(WHNO_MM1, A.MMCODE) as S_INV_QTY,
                                A.M_AGENNO,
                                (select AGEN_NAMEC from PH_VENDER where AGEN_NO = A.M_AGENNO) as AGEN_NAMEC,
                                (select AGEN_NAMEE from PH_VENDER where AGEN_NO = A.M_AGENNO) as AGEN_NAMEE
                        FROM MI_MAST A
                        LEFT JOIN 
                        (select * from MI_WHINV b where WH_NO = :WH_NO
                        ) B ON A.mmcode = B.mmcode
                        WHERE nvl(B.INV_QTY,0)>0 
                        and (A.CANCEL_ID='N' or A.CANCEL_ID is null)
                        and (A.MAT_CLASS in ('01', '02') or A.MAT_CLASS in (select MAT_CLASS from MI_MATCLASS where mat_clsid = '3') )
                         ";

            p.Add(":WH_NO", string.Format("{0}", query.WH_NO));

            if (query.MMCODE != "")
            {
                sql += " AND UPPER(A.MMCODE) LIKE UPPER(:MMCODE) ";
                p.Add(":MMCODE", string.Format("{0}%", query.MMCODE));
            }

            if (query.MMNAME_C != "")
            {
                sql += " AND UPPER(A.MMNAME_C) LIKE UPPER(:MMNAME_C)";
                p.Add(":MMNAME_C", string.Format("%{0}%", query.MMNAME_C));
            }

            if (query.MMNAME_E != "")
            {
                sql += " AND UPPER(A.MMNAME_E) LIKE UPPER(:MMNAME_E)";
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

        public IEnumerable<MI_MAST> GetMmcodeCombo(string p0, string wh_no, int page_index, int page_size, string sorters)
        {
            DynamicParameters p = new DynamicParameters();

            string sql = @"
                select A.MMCODE,MMNAME_C,MMNAME_E, NVL(A.INV_QTY, 0) INV_QTY,
                       (SELECT UI_CHANAME FROM MI_UNITCODE D WHERE B.BASE_UNIT=D.UNIT_CODE) BASE_UNIT
                  from MI_WHINV A,MI_MAST B
                 where a.wh_no=:wh_no AND A.MMCODE=B.MMCODE AND B.MAT_CLASS='01' AND B.E_ORDERDCFLAG='N'
                   and (UPPER(A.MMCODE) LIKE UPPER(:MMCODE) or UPPER(MMNAME_C) LIKE UPPER(:MMNAME_C) or UPPER(MMNAME_E) LIKE UPPER(:MMNAME_E ))
                   and A.MMCODE not like '004%' 
                   and a.inv_qty > 0
                UNION ALL
                select A.MMCODE,MMNAME_C,MMNAME_E, NVL(A.INV_QTY, 0) INV_QTY,
                       (SELECT UI_CHANAME FROM MI_UNITCODE D WHERE B.BASE_UNIT=D.UNIT_CODE) BASE_UNIT
                  from MI_WHINV A,MI_MAST B
                 where wh_no=:wh_no AND A.MMCODE=B.MMCODE AND B.MAT_CLASS='02' and (B.CANCEL_ID='N' or B.CANCEL_ID is null)
                   and (UPPER(A.MMCODE) LIKE UPPER(:MMCODE) or UPPER(MMNAME_C) LIKE UPPER(:MMNAME_C) or UPPER(MMNAME_E) LIKE UPPER(:MMNAME_E ))
                   and A.MMCODE not like '004%'
                   and a.inv_qty > 0
                UNION ALL
                select A.MMCODE,MMNAME_C,MMNAME_E, NVL(A.INV_QTY, 0) INV_QTY,
                       (SELECT UI_CHANAME FROM MI_UNITCODE D WHERE B.BASE_UNIT=D.UNIT_CODE) BASE_UNIT
                  from MI_WHINV A,MI_MAST B
                 where wh_no=:wh_no AND A.MMCODE=B.MMCODE AND B.MAT_CLASS in (select mat_class from MI_MATCLASS where mat_clsid = '3') 
                   and (B.CANCEL_ID='N' or B.CANCEL_ID is null)
                   and (UPPER(A.MMCODE) LIKE UPPER(:MMCODE) or UPPER(MMNAME_C) LIKE UPPER(:MMNAME_C) or UPPER(MMNAME_E) LIKE UPPER(:MMNAME_E ))
                   and a.inv_qty > 0
                 ORDER BY MMCODE
            ";

            p.Add(":wh_no", string.Format("{0}", wh_no));
            p.Add(":MMCODE", string.Format("{0}%", p0));
            p.Add(":MMNAME_C", string.Format("%{0}%", p0));
            p.Add(":MMNAME_E", string.Format("%{0}%", p0.ToUpper()));

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetTrnabResonCombo()
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(" select DATA_VALUE VALUE,DATA_VALUE||' '||DATA_DESC COMBITEM from PARAM_D ");
            sql.Append(" where GRP_CODE='ME_DOCD' and DATA_NAME='TRNAB_RESON' ");
            sql.Append(" ORDER BY DATA_VALUE ");

            return DBWork.Connection.Query<COMBO_MODEL>(sql.ToString());
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
                        ,B.MMNAME_E, (SELECT UI_CHANAME FROM MI_UNITCODE D WHERE B.BASE_UNIT=D.UNIT_CODE) BASE_UNIT, TRNAB_QTY,
                        TRNAB_RESON,
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE='ME_DOCD' and DATA_NAME='TRNAB_RESON' and DATA_VALUE = A.TRNAB_RESON) as TRNAB_RESON_TEXT
                        ,d.inv_qty
                        FROM ME_DOCD A,MI_MAST B, ME_DOCM C, mi_whinv d 
                        WHERE C.DOCNO=A.DOCNO 
                            AND A.MMCODE = B.MMCODE  
                            AND b.mmcode = d.mmcode 
                            AND c.frwh = d.wh_no
                            AND A.DOCNO = :p0";

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

        public IEnumerable<ME_DOCD> GetD(string id, string seq)
        {
            var sql = @"SELECT A.DOCNO,A.SEQ,A.MMCODE,A.APPQTY,APVQTY,ACKQTY,TWN_DATE(APVTIME)APVTIME,TWN_DATE(ACKTIME)ACKTIME,B.MMNAME_C,
                        B.MMNAME_E, (SELECT UI_CHANAME FROM MI_UNITCODE D WHERE B.BASE_UNIT=D.UNIT_CODE) BASE_UNIT, TRNAB_QTY,
                        TRNAB_RESON,
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE='ME_DOCD' and DATA_NAME='TRNAB_RESON' and DATA_VALUE = A.TRNAB_RESON) as TRNAB_RESON_TEXT,
                       d.inv_qty
                         FROM ME_DOCD A,MI_MAST B, ME_DOCM C ,  mi_whinv d 
                        WHERE C.DOCNO=A.DOCNO AND A.MMCODE = B.MMCODE AND b.mmcode = d.mmcode  AND c.frwh = d.wh_no
                        AND A.DOCNO = :DOCNO AND A.SEQ=:SEQ";
            return DBWork.Connection.Query<ME_DOCD>(sql, new { DOCNO = id, SEQ = seq }, DBWork.Transaction);
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
                        APVQTY = :APVQTY,APVID=:APVID,APVTIME = SYSDATE,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO AND SEQ = :SEQ";
            return DBWork.Connection.Execute(sql, pME_DOCD, DBWork.Transaction);
        }
        public int UpdateDAckQty(ME_DOCD pME_DOCD)
        {
            var sql = @"UPDATE ME_DOCD SET 
                        ACKQTY = :ACKQTY,ACKTIME = SYSDATE,ACKID=:ACKID,TRNAB_QTY=:TRNAB_QTY,TRNAB_RESON=:TRNAB_RESON,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO AND SEQ = :SEQ ";
            return DBWork.Connection.Execute(sql, pME_DOCD, DBWork.Transaction);
        }

        public IEnumerable<Models.AB.AB0029> GetAllWH(string wh_no, string MMCODE, string MMNAME_C, string MMNAME_E, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT A.WH_NO,A.WH_NO||' '||WH_NAME WH_NAME,A.MMCODE ,MMNAME_C,MMNAME_E,A.INV_QTY");
            sql.Append(" FROM MI_WHINV A,MI_WHMAST B ,MI_MAST C");
            sql.Append(" WHERE A.WH_NO=B.WH_NO AND A.MMCODE=C.MMCODE AND A.INV_QTY>0 and b.wh_grade in ('2','3','4')");
            sql.Append(" AND EXISTS(SELECT 'X' FROM MI_WHMAST D WHERE D.WH_NO=:WH_NO AND D.WH_KIND=B.WH_KIND AND D.WH_GRADE=B.WH_GRADE AND D.WH_NO<>B.WH_NO)");
            p.Add(":WH_NO", wh_no);
            if (MMCODE != "")
            {
                sql.Append(" AND A.MMCODE LIKE :MMCODE");
                p.Add(":MMCODE", MMCODE.ToUpper() + '%');
            }
            if (MMNAME_C != "")
            {
                sql.Append(" AND MMNAME_C LIKE :MMNAME_C");
                p.Add(":MMNAME_C", '%' + MMNAME_C + '%');
            }
            if (MMNAME_E != "")
            {
                sql.Append(" AND MMNAME_E LIKE :MMNAME_E");
                p.Add(":MMNAME_E", '%' + MMNAME_E + '%');
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<Models.AB.AB0029>(GetPagingStatement(sql.ToString(), sorters), p, DBWork.Transaction);
        }

        public bool CheckFrwhMmcodeValid(string docno, string mmcode)
        {
            string sql = @"select 1 
                             from ME_DOCM a, MI_WHINV b
                            where a.docno = :docno
                              and b.wh_no = a.frwh
                              and b.mmcode = :mmcode";
            return !(DBWork.Connection.ExecuteScalar(sql, new { docno = docno, mmcode = mmcode }, DBWork.Transaction) == null);
        }

        #region 20201-05-06新增: 修改、刪除時檢查flowId
        public bool ChceckFlowId01(string docno)
        {
            string sql = @"
                select 1 from ME_DOCM
                 where docno = :docno
                   and (substr(flowId, length(flowId)-1 , 2) = '01'
                       or substr(flowId, length(flowId)-1 , 2) = '00'
                       or substr(flowId, length(flowId)-1 , 2) = '1')
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { docno }, DBWork.Transaction) != null;
        }
        #endregion

        #region 2021-07-30 新增: 新增主檔、送核撥 檢查申請庫房是否作廢
        public bool CheckIsTowhCancelByWhno(string towh)
        {
            string sql = @"
               select 1 from MI_WHMAST 
                where wh_no = :towh
                  and cancel_id = 'N'
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { towh }, DBWork.Transaction) == null;
        }

        public bool CheckIsTowhCancelByDocno(string docno, string wh)
        {
            string sql = string.Format(@"
               select 1 from ME_DOCM  a
                where a.docno = :docno
                  and exists (select 1 from MI_WHMAST 
                               where wh_no = a.{0}
                                 and cancel_id = 'N')
            ", wh);
            return DBWork.Connection.ExecuteScalar(sql, new { docno }, DBWork.Transaction) == null;
        }
        #endregion

        #region 2022-06-08 確認入庫增加填寫MI_WINVCTL.FSTACKDATE
        public bool CheckExistsMI_WHMAST(string towh)
        {
            string sql = @"select 1
                             from MI_WHMAST
                            where WH_NO=:TOWH
                              and WH_KIND='0'
                              and WH_GRADE in('2','3','4')";
            return !(DBWork.Connection.ExecuteScalar(sql, new { TOWH = towh }, DBWork.Transaction) == null);
        }

        public int UpdateFstackDate(string towh, string docno)
        {
            var sql = @"update MI_WINVCTL
                           set FSTACKDATE=trunc(sysdate)
                         where WH_NO=:TOWH 
                           and MMCODE in (select MMCODE from ME_DOCD where DOCNO=:docno)
                           and FSTACKDATE is null";
            return DBWork.Connection.Execute(sql, new { TOWH = towh, docno = docno }, DBWork.Transaction);
        }
        #endregion
        #region 2023-07-04 DOCNO單號分三總用和國軍用
        // 檢查單號是否存在
        public bool CheckExists(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCM WHERE DOCNO=:DOCNO";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        //國軍DOCNO單號統一用GET_DAILY_DOCNO
        public string GetDailyDocno()
        {
            string sql = @"select GET_DAILY_DOCNO from DUAL";
            string rtn = DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction).ToString();
            return rtn;
        }
        public string GetHospCode()
        {
            var sql = @" SELECT data_value FROM PARAM_D WHERE grp_code = 'HOSP_INFO' AND data_name = 'HospCode' ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }
        #endregion

        public class MI_MAST_QUERY_PARAMS
        {
            public string DOCNO;
            public string MMCODE;
            public string MMNAME_C;
            public string MMNAME_E;
            public string MAT_CLASS;
            public string WH_NO;

            public string M_AGENNO;
            public string AGEN_NAME;
        }

        #region 2023-08-09 新增明細檢查院內碼是否停用、調出單位是否有庫存量
        public bool CheckCancelIdY(string mmcode)
        {
            string sql = @"
                select 1 from MI_MAST where mmcode = :mmcode and nvl(cancel_id, 'N') = 'Y'
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { mmcode }, DBWork.Transaction) != null;
        }

        public bool CheckFrwhInvqty0(string docno, string mmcode)
        {
            string sql = @"
                select inv_qty from MI_WHINV 
                 where wh_no = (select frwh from ME_DOCM where docno = :docno) 
                   and mmcode = :mmcode
            ";
            int temp = DBWork.Connection.QueryFirstOrDefault<int>(sql, new { docno, mmcode }, DBWork.Transaction);

            return temp > 0;
        }

        #endregion
        public DataTable GetExcel(string tuser, string p0, string p1, string p2, string p3, string p4, string p5, string p6, string p7)
        {
            DynamicParameters sqlParam = new DynamicParameters();

            string sqlStr = @"select  A.DOCNO AS 申請單號,
                                                  (case when C.FLOWID='0201' then '0201 調撥申請' 
                                                             when C.FLOWID='0202' then '0202 調出中'
                                                             when C.FLOWID='0203' then '0203 調入中' 
                                                             when C.FLOWID='0204' then '0204 調出中' 
                                                             when C.FLOWID='0299' then '0299 調撥結案' end) as 申請單狀態,
                                                 USER_NAME(C.APPID) as 申請人員, INID_NAME(C.APPDEPT)as 申請部門,TWN_DATE(C.APPTIME) as 申請日期,
                                                (select WH_NO||' '||WH_NAME from MI_WHMAST where WH_NO=C.FRWH) 調出庫房,
                                                (select WH_NO||' '||WH_NAME from MI_WHMAST where WH_NO=C.TOWH) 調入庫房,
                                                ( case when (select wh_kind from MI_WHMAST where wh_no =C.FRWH) = '0'
                                                            then '藥品' else '衛材、一般物品' end) as 調撥類別,
                                               A.MMCODE as 院內碼,B.MMNAME_C as 中文品名 ,B.MMNAME_E as 英文品名,
                                               (select UI_CHANAME FROM MI_UNITCODE D WHERE B.BASE_UNIT=D.UNIT_CODE) 單位,
                                               A.APPQTY as 申請數量,APVQTY as 調出數量,TWN_DATE( APVTIME) as 調出日期,ACKQTY as 調入數量,
                                               TWN_DATE(ACKTIME) as 調入日期,TRNAB_QTY as 調撥短少數量,
                                               (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D 
                                                where GRP_CODE='ME_DOCD' and DATA_NAME='TRNAB_RESON' 
                                                    and DATA_VALUE = A.TRNAB_RESON) as 調撥異常原因,
                                               (A.ACKQTY-A.TRNAB_QTY) as 實際點收量
                                           from ME_DOCD A,MI_MAST B, ME_DOCM C
                                        where C.DOCNO=A.DOCNO AND A.MMCODE = B.MMCODE  
                                            and EXISTS(select  'X' from MI_WHMAST D where D.WH_NO=C.TOWH
                                                                      and ((EXISTS(select  'X' from MI_WHID E where D.WH_NO=E.WH_NO AND WH_USERID=:WH_USERID))
                                                                         or (D.WH_KIND='1' and (EXISTS(select  'X' from UR_ID E 
                                                                                                                                       where (D.SUPPLY_INID=E.INID OR D.INID=E.INID) and E.TUSER=:WH_USERID
                                            and  NOT EXISTS(select 'X' from MI_WHID F where F.TASK_ID IN ('2','3') and F.WH_USERID=:WH_USERID)))
                                               or ( EXISTS(select 'X' from MI_WHID F where F.TASK_ID IN ('2','3') and F.WH_USERID=:WH_USERID and F.WH_NO=D.WH_NO))
                                         )))";
            sqlParam.Add(":WH_USERID", string.Format("{0}", tuser));

            if (p0.Trim() != "")
            {
                sqlStr += " and  A.DOCNO = :DOCNO";
                sqlParam.Add(":DOCNO", string.Format("{0}%", p0));
            }
            if (p1.Trim() != "" & p2.Trim() != "")
            {
                sqlStr += " and C.APPTIME between TO_DATE(:APPTIME_S,'YYYYMMDD') and TO_DATE(:APPTIME_E||'235959','YYYYMMDDHH24MISS') ";
                sqlParam.Add(":APPTIME_S", string.Format("{0}", p1));
                sqlParam.Add(":APPTIME_E", string.Format("{0}", p2));
            }
            else
            {
                if (p1.Trim() != "")
                {
                    sqlStr += " and C.APPTIME >= TO_DATE(:APPTIME_S,'YYYYMMDD') ";
                    sqlParam.Add(":APPTIME_S", string.Format("{0}", p1));
                }
                if (p2.Trim() != "")
                {
                    sqlStr += " and C.APPTIME <= TO_DATE(:APPTIME_E||'235959','YYYYMMDDHH24MISS') ";
                    sqlParam.Add(":APPTIME_E", string.Format("{0}", p2));
                }
            }
            if (p3.Trim() != "")
            {
                sqlStr += " and C.FRWH = :FRWH ";
                sqlParam.Add(":FRWH", string.Format("{0}", p3));
            }
            if (p4.Trim() != "")
            {
                sqlStr += " and USER_NAME(C.APPID) LIKE :APPID ";
                sqlParam.Add(":APPID", string.Format("{0}%", p4));
            }
            if (p5.Trim() != "")
            {
                sqlStr += " and C.APPDEPT = :APPDEPT ";
                sqlParam.Add(":APPDEPT", string.Format("{0}", p5));
            }
            if (p6.Trim() != "")
            {
                sqlStr += " and C.TOWH = :TOWH ";
                sqlParam.Add(":TOWH", string.Format("{0}", p6));
            }
            if (p7.Trim() != "")
            {
                string param = "";
                string[] FLOWIDList = p7.Split(',');
                for (int i = 0; i < FLOWIDList.Length; i++)
                {
                    if (i >= 1) param += ",";
                    param += ":FLOWID_" + i;

                    sqlParam.Add(":FLOWID_" + i, FLOWIDList[i]);
                }

                sqlStr += " and C.FLOWID in (" + param + ")";
            }
            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sqlStr, sqlParam, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
        public IEnumerable<AB0003Model> GetLoginInfo(string id, string ip)
        {
            string sql = @"SELECT TUSER AS USERID, UNA AS USERNAME, INID, INID_NAME(INID) AS INIDNAME,
                        WHNO_MM1 CENTER_WHNO,INID_NAME(WHNO_MM1) AS CENTER_WHNAME, TO_CHAR(SYSDATE,'YYYYMMDD') AS TODAY,
                        :UPDATE_IP,
                        (select DATA_VALUE from PARAM_D where GRP_CODE = 'HOSP_INFO' and DATA_NAME = 'HospCode') as HOSP_CODE,
                        (case when (select count(*) from UR_UIR where RLNO in ('MAT_14') and TUSER = :TUSER) > 0 then 'Y' else 'N' end) as IS_GRADE1
                        FROM UR_ID
                        WHERE UR_ID.TUSER=:TUSER";

            return DBWork.Connection.Query<AB0003Model>(sql, new { TUSER = id, UPDATE_IP = ip });
        }
    }
}