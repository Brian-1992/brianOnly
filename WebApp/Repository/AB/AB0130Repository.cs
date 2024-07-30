using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using WebApp.Models.AB;
using System.Text;

namespace WebApp.Repository.AB
{
    public class AB0130Repository : JCLib.Mvc.BaseRepository
    {
        const String sBr = "\r\n";

        public AB0130Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AB0130> GetAllM(string tuser, string docno, string apptime_s, string apptime_e, string checkboxgroup, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @" 
                                SELECT distinct a.DOCNO,
                                    TWN_TIME_FORMAT(a.APPTIME) APPTIME, a.APPDEPT,
                                    (select INID||' '||INID_NAME FROM UR_INID where INID=a.APPDEPT) AS APPDEPT_N,
                                    (APP_INID||' '|| INID_NAME(APP_INID)) as APP_INID_N, a.IS_DEL, TWN_TIME_FORMAT(a.APVTIME) APVTIME,
                                (case when a.IS_DEL='N' then '待送出'
                                       when (a.IS_DEL='S' and trim(APVTIME) is null) then '待核可'
                                     when (a.IS_DEL='S' and trim(APVTIME) is not null) then '已核可'
                                  end) as STATUS
                                   FROM DGMISS a
                                 WHERE a.APP_INID = USER_INID(:tuser) and a.IS_DEL<>'Y'
                            ";
            p.Add(":tuser", tuser);

            // 補藥單編號有值
            if (!string.IsNullOrWhiteSpace(docno))
            {
                sql += " and a.DOCNO = :DOCNO ";
                p.Add(":DOCNO", string.Format("{0}", docno));
            }

            // 補藥日期之起迄有值
            if (!string.IsNullOrWhiteSpace(apptime_s) && !string.IsNullOrWhiteSpace(apptime_e))
            {
                sql += " AND a.APPTIME BETWEEN TO_DATE(:APPTIME_S,'YYYYMMDD') AND TO_DATE(:APPTIME_E || '235959', 'YYYYMMDDHH24MISS') ";
                p.Add(":APPTIME_S", string.Format("{0}", apptime_s));
                p.Add(":APPTIME_E", string.Format("{0}", apptime_e));
            }
            else
            {
                // 補藥日期之起有值、迄空白
                if (!string.IsNullOrWhiteSpace(apptime_s))
                {
                    sql += " AND a.APPTIME >= TO_DATE(:APPTIME_S, 'YYYYMMDD') ";
                    p.Add(":APPTIME_S", string.Format("{0}", apptime_s));
                }
                // 補藥日期之起空白、迄有值
                if (!string.IsNullOrWhiteSpace(apptime_e))
                {
                    sql += " AND a.APPTIME <= TO_DATE(:APPTIME_E || '235959', 'YYYYMMDDHH24MISS') ";
                    p.Add(":APPTIME_E", string.Format("{0}", apptime_e));
                }
            }

            // 判斷勾選項目
            if (!string.IsNullOrWhiteSpace(checkboxgroup))
            {
                int option = 0;

                // 1 待送出；2 待核可；3 已核可
                if (checkboxgroup.Contains("1")) option |= 1;
                if (checkboxgroup.Contains("2")) option |= 2;
                if (checkboxgroup.Contains("3")) option |= 4;

                switch (option)
                {
                    case 0: // None selected
                        break;
                    case 1: // 1 selected // 待送出勾選 且 待核可、已核可未勾選
                        sql += " and a.IS_DEL ='N' ";
                        break;
                    case 2: // 2 selected // 待核可勾選 且 待送出、已核可未勾選
                        sql += " and a.IS_DEL ='S' and trim(a.APVTIME) is null ";
                        break;
                    case 3: // 1 and 2 selected // 待送出、待核可勾選 且 已核可未勾選
                        sql += " and ((a.IS_DEL ='N') or (a.IS_DEL ='S' and trim(a.APVTIME) is null)) ";
                        break;
                    case 4: // 3 selected // 已核可勾選 且 待送出、待核可未勾選
                        sql += " and a.IS_DEL ='S' and trim(a.APVTIME) is not null ";
                        break;
                    case 5: // 1 and 3 selected // 若待送出、已核可勾選 且 待核可未勾選
                        sql += " and ((a.IS_DEL ='N') or (a.IS_DEL ='S' and trim(a.APVTIME) is not null)) ";
                        break;
                    case 6: // 2 and 3 selected // 待核可、已核可勾選 且 待送出未勾選
                        sql += " and a.IS_DEL ='S' ";
                        break;
                    case 7: // 1, 2 and 3 selected
                        break;
                    default:
                        break;
                }
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AB0130>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<AB0130> GetAllD(string DOCNO, string MMCODE, string isIV, string tuser, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @" SELECT
                                A.DOCNO, A.SEQ, A.MMCODE, B.MMNAME_C,    
                                B.M_NHIKEY,    
                                (select WH_NAME from MI_WHMAST where INID = A.SUPPLY_INID and WH_KIND = '0') as SUPPLY_WHNO,    
                                nvl((select INV_QTY from DGMISS_INV where INID = USER_INID(:TUSER) and MMCODE = A.MMCODE and DATA_YM = twn_yyymm(A.APPTIME) and rownum = 1), A.INV_QTY) as TO_INV_QTY,    
                                B.UNITRATE||' '||B.BASE_UNIT||'/'||B.M_PURUN as BASE_UNIT_DESC,
                                A.APPTIME,    
                                AGEN_NAME(B.M_AGENNO) as AGEN_NAMEC,    
                                INV_QTY((select WH_NO from MI_WHMAST where INID = A.SUPPLY_INID and WH_KIND = '0'),A.MMCODE) as FR_INV_QTY,    
                                A.APPQTY, B.BASE_UNIT, B.CASENO, B.E_ITEMARMYNO, 
                                (select HIGH_QTY from MI_WINVCTL where MMCODE = A.MMCODE and WH_NO = (select WH_NO from MI_WHMAST where INID = A.SUPPLY_INID and WH_KIND = '0')) as HIGH_QTY, 
                                B.UPRICE,    
                                twn_date(B.E_CODATE) as E_CODATE,    
                                DECODE(B.ORDERKIND,'0','無','1','常備品項','2','小額採購','') as ORDERKIND,  
                                round(A.APPQTY * B.UPRICE) as TOTAL_AMT,
                                (select WH_NAME from MI_WHMAST where INID = A.SUPPLY_INID and WH_KIND = '0') as WH_NAME
                            FROM DGMISS A, MI_MAST B
                            WHERE A.MMCODE = B.MMCODE ";

            p.Add(":TUSER", tuser);
            if (DOCNO != "")
            {
                sql += " AND A.DOCNO = :p0 ";
                p.Add(":p0", string.Format("{0}", DOCNO));
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
            if (isIV == "true")
            {
                sql += " AND B.isIV='Y' ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AB0130>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMmCodeCombo(string p0, string tuser, string supply_whno, string dgMastInid, int page_index, int page_size, string sorters)
        {

            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.M_NHIKEY, 
                               WH_NAME(:SUPPLY_WHNO) as SUPPLY_WHNO, 
                               A.UNITRATE || ' ' || A.BASE_UNIT || '/' || A.M_PURUN as BASE_UNIT_DESC, AGEN_NAME(A.M_AGENNO) as AGEN_NAMEC,
                               NVL(( SELECT INV_QTY FROM DGMISS_INV WHERE INID = USER_INID(:TUSER) and MMCODE = A.MMCODE and DATA_YM = (SELECT max(DATA_YM) FROM DGMISS_INV WHERE INID = USER_INID(:TUSER) and MMCODE = A.MMCODE) ),0) AS TO_INV_QTY,
                               NVL(( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO=:SUPPLY_WHNO ),0) AS FR_INV_QTY,
                               A.BASE_UNIT, A.CASENO, A.E_ITEMARMYNO, 0 as HIGH_QTY, A.UPRICE, twn_date(A.E_CODATE) as E_CODATE_T,
                               DECODE(A.ORDERKIND,'0','無','1','常備品項','2','小額採購','') as ORDERKIND
                          FROM MI_MAST A 
                         WHERE 1=1 
                           AND A.MAT_CLASS = '01'
                           AND nvl(A.CANCEL_ID, 'N') <> 'Y' ";

            if (dgMastInid != "")
            {
                sql += @"AND (select count(*) from DGMISS_MAST 
                                where MMCODE = A.MMCODE and INID = :APP_INID)> 0 ";
                p.Add(":APP_INID", dgMastInid);
            }

            sql += @" {1} ";

            if (p0 != "")
            {
                sql = string.Format(sql,
                     "(NVL(INSTR(UPPER(A.MMCODE), UPPER(:MMCODE_I)), 1000) + NVL(INSTR(UPPER(MMNAME_E), UPPER(:MMNAME_E_I)), 100) * 10 + NVL(INSTR(UPPER(MMNAME_C), UPPER(:MMNAME_C_I)), 100) * 10) IDX,",
                     @"   AND (UPPER(A.MMCODE) LIKE UPPER(:MMCODE) OR UPPER(MMNAME_E) LIKE UPPER(:MMNAME_E) OR UPPER(MMNAME_C) LIKE UPPER(:MMNAME_C))");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);
                p.Add(":MMCODE", string.Format("{0}%", p0));
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "", "");
                sql += " ORDER BY MMCODE ";
            }
            p.Add(":TUSER", tuser);
            p.Add(":SUPPLY_WHNO", supply_whno);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public int Apply(string docno, string supply_whno, string update_user, string update_ip)
        {
            // 以IS_DEL = S判斷是否送出...
            var p = new DynamicParameters();
            var sql = @" update DGMISS set 
                                APVTIME= null, ACKQTY = null, ACKTIME = null, IS_DEL = 'S', APVQTY=APPQTY,
                                SUPPLY_INID = :SUPPLY_WHNO, UPDATE_USER = :UPDATE_USER, UPDATE_TIME = sysdate, UPDATE_IP =:UPDATE_IP
                                where DOCNO = :DOCNO  ";

            p.Add(":UPDATE_USER", update_user);
            p.Add(":UPDATE_IP", update_ip);
            p.Add(":DOCNO", docno);
            p.Add(":SUPPLY_WHNO", supply_whno);
            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }
        public int UpdateD(string docno, string seq, string appqty, string invqty, string update_user, string update_ip)
        {
            var p = new DynamicParameters();
            var sql = @"UPDATE DGMISS SET APPQTY  = :APPQTY, INV_QTY = :INV_QTY, UPDATE_USER = :UPDATE_USER, UPDATE_TIME = sysdate, UPDATE_IP = :UPDATE_IP 
                        where DOCNO = :DOCNO and SEQ = :SEQ ";
            p.Add(":APPQTY", appqty);
            p.Add(":INV_QTY", invqty);
            p.Add(":UPDATE_USER", update_user);
            p.Add(":UPDATE_IP", update_ip);
            p.Add(":DOCNO", docno);
            p.Add(":SEQ", seq);
            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }

        public int UpdateFirstD(string docno, string appqty, string mmcode, string inv_qty, string supply_whno, string update_user, string update_ip)
        {
            var p = new DynamicParameters();
            var sql = @"UPDATE DGMISS SET MMCODE = :MMCODE, APPQTY  = :APPQTY, INV_QTY = :INV_QTY, SUPPLY_INID = WH_INID(:SUPPLY_WHNO), UPDATE_USER = :UPDATE_USER, UPDATE_TIME = sysdate, UPDATE_IP = :UPDATE_IP 
                        where DOCNO = :DOCNO and SEQ = 1";

            p.Add(":MMCODE", mmcode);
            p.Add(":APPQTY", appqty);
            p.Add(":INV_QTY", inv_qty);
            p.Add(":SUPPLY_WHNO", supply_whno);
            p.Add(":UPDATE_USER", update_user);
            p.Add(":UPDATE_IP", update_ip);
            p.Add(":DOCNO", docno);
            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }

        public int InsertD(string docno, string appqty, string mmcode, string inv_qty, string supply_whno, string update_user, string update_ip)
        {
            var p = new DynamicParameters();
            var sql = @" insert into DGMISS(DOCNO,SEQ,MMCODE,APPTIME,APP_INID,APPDEPT,APPQTY,INV_QTY,SUPPLY_INID,
                                           CREATE_USER,CREATE_TIME,UPDATE_USER,UPDATE_TIME,UPDATE_IP) 
                           values(:DOCNO, (select max(SEQ)+1 from DGMISS where DOCNO=:DOCNO), :MMCODE,
                                  (select APPTIME from DGMISS where DOCNO=:DOCNO and SEQ = 1),
                                  user_inid(:UPDATE_USER), user_inid(:update_user), :APPQTY,:INV_QTY,WH_INID(:SUPPLY_WHNO),
                                     :UPDATE_USER,sysdate,:UPDATE_USER,sysdate,:UPDATE_IP) ";
            p.Add(":DOCNO", docno);
            p.Add(":MMCODE", mmcode);
            p.Add(":APPQTY", appqty);
            p.Add(":INV_QTY", inv_qty);
            p.Add(":SUPPLY_WHNO", supply_whno);
            p.Add(":UPDATE_USER", update_user);
            p.Add(":UPDATE_IP", update_ip);
            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }

        public int UpdateDinv(string docno, string mmcode, string inv_qty)
        {
            var p = new DynamicParameters();
            var sql = @"UPDATE DGMISS SET INV_QTY = :INV_QTY
                        where DOCNO = :DOCNO and MMCODE = :MMCODE ";

            p.Add(":INV_QTY", inv_qty);
            p.Add(":DOCNO", docno);
            p.Add(":MMCODE", mmcode); ;
            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }

        public int ClearD(string docno)
        {
            var p = new DynamicParameters();
            var sql = @" update DGMISS set 
                                MMCODE= null, INV_QTY = 0, APPQTY = 0
                                where DOCNO = :DOCNO and SEQ = 1  ";

            p.Add(":DOCNO", docno);

            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }

        public int DeleteD(string docno, string seq)
        {
            var p = new DynamicParameters();
            var sql = @" delete from DGMISS
                                where DOCNO = :DOCNO and SEQ = :SEQ  ";

            p.Add(":DOCNO", docno);
            p.Add(":SEQ", seq);

            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }

        public int Cancel(string DOCM, string update_user, string update_ip)
        {
            var p = new DynamicParameters();
            var sql = @"UPDATE DGMISS SET IS_DEL = 'Y', UPDATE_TIME = SYSDATE, UPDATE_USER = :p1, UPDATE_IP = :p2 
                                WHERE DOCNO = :p3";
            p.Add(":p1", update_user);
            p.Add(":p2", update_ip);
            p.Add(":p3", DOCM);
            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);

        }

        public int UpdateDgmissInv(string docno, string mmcode, string inv_qty, string supply_whno, string update_user, string update_ip)
        {
            var p = new DynamicParameters();
            var sql = @"UPDATE DGMISS_INV SET INV_QTY = :INV_QTY 
                        where DATA_YM = (select twn_yyymm(APPTIME) from DGMISS where DOCNO = :DOCNO and rownum = 1)
                        and INID = USER_INID(:UPDATE_USER) 
                        and MMCODE = :MMCODE ";

            p.Add(":MMCODE", mmcode);
            p.Add(":INV_QTY", inv_qty);
            // p.Add(":SUPPLY_WHNO", supply_whno);
            p.Add(":UPDATE_USER", update_user);
            //p.Add(":UPDATE_IP", update_ip);
            p.Add(":DOCNO", docno);
            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }

        public int InsertDgmissInv(string docno, string mmcode, string inv_qty, string supply_whno, string update_user, string update_ip)
        {
            var p = new DynamicParameters();
            var sql = @" insert into DGMISS_INV(DATA_YM, INID, MMCODE, INV_QTY, SUPPLY_WHNO) 
                                     values(
                                        (select twn_yyymm(APPTIME) from DGMISS where DOCNO = :DOCNO and rownum = 1),
                                        USER_INID(:TUSER),
                                        :MMCODE, :INV_QTY, :SUPPLY_WHNO) ";
            p.Add(":DOCNO", docno);
            p.Add(":MMCODE", mmcode);
            p.Add(":INV_QTY", inv_qty);
            p.Add(":SUPPLY_WHNO", supply_whno);
            p.Add(":TUSER", update_user);
            // p.Add(":UPDATE_IP", update_ip);

            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }

        public string GetDocno()
        {
            var p = new OracleDynamicParameters();
            p.Add("O_DOCNO", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 12);

            DBWork.Connection.Query("GET_DOCNO", p, commandType: CommandType.StoredProcedure);
            return p.Get<OracleString>("O_DOCNO").Value;
        }

        public int InsertDGMISS(string docnoEXE, string update_user, string update_ip)
        {
            var p = new DynamicParameters();
            var sql = @" insert into DGMISS(DOCNO,SEQ,APPTIME,APP_INID,APPDEPT,CREATE_USER,CREATE_TIME,UPDATE_USER,UPDATE_TIME,UPDATE_IP) 
                            values(:docno,1,sysdate,user_inid(:create_user),user_inid(:create_user),:create_user,sysdate,:update_user,sysdate,:update_ip) ";
            p.Add(":docno", docnoEXE);
            p.Add(":create_user", update_user);
            p.Add(":update_user", update_user);
            p.Add(":update_ip", update_ip);
            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }

        public bool CheckIsFirst(string id)
        {
            string sql = @"SELECT 1 FROM DGMISS WHERE DOCNO=:DOCNO and SEQ = 1 and MMCODE is null ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        public bool CheckDgmissInv(string docno, string tuser, string mmcode)
        {
            string sql = @"SELECT 1 FROM DGMISS_INV WHERE DATA_YM=(select twn_yyymm(APPTIME) from DGMISS where DOCNO = :DOCNO and rownum = 1) 
                    and INID = USER_INID(:TUSER) and MMCODE = :MMCODE ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = docno, TUSER = tuser, MMCODE = mmcode }, DBWork.Transaction) == null);
        }
        public string GetSupplyWhno(string tuser)
        {
            // 用登入者INID找上級庫
            // INID的上級庫理論上唯一且不會變動, 取SUPPLY_WHNO時可不看MMCODE
            string sql = @"select SUPPLY_WHNO from DGMISS_INV where INID = USER_INID(:TUSER) and rownum = 1 ";
            string rtn = Convert.ToString(DBWork.Connection.ExecuteScalar(sql, new { TUSER = tuser }, DBWork.Transaction));
            return rtn;
        }

        public string GetSupplyWhno_SupplyInid(string docno)
        {
            string sql = @"select SUPPLY_INID from DGMISS_SUPPLY where APP_INID = (select distinct APPDEPT from DGMISS where docno = :DOCNO) and rownum = 1 ";
            string rtn = Convert.ToString(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = docno }, DBWork.Transaction));
            return rtn;
        }

        public IEnumerable<AB0130> GetPrintData(string p0)
        {
            var p = new DynamicParameters();
            var sql = "";
            sql += "select ";
            sql += " A.SEQ, A.MMCODE, B.MMNAME_C, A.APPQTY, B.BASE_UNIT, B.UPRICE, ";
            sql += " A.APPQTY * B.UPRICE as APPAMT, ";
            sql += " A.INV_QTY, AGEN_NAME(B.M_AGENNO) as AGEN_NAME, ";
            sql += " DECODE(B.M_CONTID, '0','合約','2','非合約','') as M_CONTID ";
            sql += " FROM DGMISS A, MI_MAST B ";
            sql += " WHERE 1=1 ";
            sql += " and A.DOCNO = :P0 ";
            sql += " and A.MMCODE = B.MMCODE ";
            sql += " ORDER BY A.SEQ ";

            p.Add(":P0", string.Format("{0}", p0));

            return DBWork.Connection.Query<AB0130>(sql, p, DBWork.Transaction);

        }

        public DataTable GetPrintData(string docno, string tuser)
        {
            var p = new DynamicParameters();
            var sql = @"select TWN_DAT_FORMAT(SYSDATE) as D1, 
                TWN_DAT_FORMAT(A.APPTIME) || ' ' || TO_CHAR(A.APPTIME,'HH24') || '時' || TO_CHAR(A.APPTIME,'MI') || '分' as D2,
                INID_NAME(A.SUPPLY_INID) as D3,
                INID_NAME(USER_INID(:TUSER)) as D4,
                round(SUM(A.APPQTY * B.UPRICE)) as D5
                from DGMISS A, MI_MAST B 
                where A.DOCNO = :DOCNO
                and A.MMCODE = B.MMCODE
                group by A.APPTIME, A.SUPPLY_INID ";

            p.Add(":TUSER", string.Format("{0}", tuser));
            p.Add(":DOCNO", string.Format("{0}", docno));

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<COMBO_MODEL> GetInidCombo()
        {
            StringBuilder sql = new StringBuilder();
            DynamicParameters p = new DynamicParameters();
            sql.Append("select inid VALUE ,inid ||' ' || inid_name TEXT from UR_INID");
            sql.Append(" ORDER BY 1");

            return DBWork.Connection.Query<COMBO_MODEL>(sql.ToString(), p);
        }

        public int UpdateM(string docno, string appdept, string update_user, string update_ip)
        {
            var p = new DynamicParameters();
            var sql = @"update DGMISS set APPDEPT = :p1, UPDATE_TIME = SYSDATE, UPDATE_USER = :p2, UPDATE_IP = :p3 
                                WHERE DOCNO = :p0";
            p.Add(":p1", appdept);
            p.Add(":p2", update_user);
            p.Add(":p3", update_ip);
            p.Add(":p0", docno);
            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);

        }
        public IEnumerable<MI_MAST> GetMMCodeDocd(string p0, string p1, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E  
                        FROM MI_MAST A, DGMISS B WHERE A.MMCODE=B.MMCODE AND B.DOCNO = :DOCNO  ";
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
        public string GetDocnoAppdept(string docno)
        {
            string sql = @"select APPDEPT from DGMISS where DOCNO = :DOCNO and rownum = 1 ";
            string rtn = Convert.ToString(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = docno }, DBWork.Transaction));
            return rtn;
        }

        public IEnumerable<MI_MAST> GetMmcode(MI_MAST_QUERY_PARAMS query, string dgMastInid, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT,A.M_CONTPRICE,A.DISC_UPRICE, 
                               ( SELECT AVG_PRICE FROM MI_WHCOST WHERE MMCODE = A.MMCODE AND DATA_YM=CUR_SETYM AND ROWNUM=1 ) AS AVG_PRICE,
                               ( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO=:WH_NO ) AS INV_QTY,
                               ( SELECT AVG_APLQTY FROM V_MM_AVGAPL WHERE MMCODE = A.MMCODE AND WH_NO=:WH_NO ) AS AVG_APLQTY,
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
                               (case
                                   when ((select 1 from dual where not exists (select 1 from MI_WHMM where mmcode = a.mmcode)
                                             union
                                          select 1 from MI_WHMM where wh_no = :WH_NO and mmcode = a.mmcode
                                         )) = '1' 
                                   then 'Y' else 'N'
                                end) as whmm_valid,
                                INV_QTY(( SELECT supply_whno FROM MI_WINVCTL WHERE MMCODE = A.MMCODE AND WH_NO=:WH_NO), A.MMCODE) as S_INV_QTY,
                                A.M_AGENNO,
                                (select AGEN_NAMEC from PH_VENDER where AGEN_NO = A.M_AGENNO) as AGEN_NAMEC,
                                (select AGEN_NAMEE from PH_VENDER where AGEN_NO = A.M_AGENNO) as AGEN_NAMEE
                          FROM MI_MAST A 
                         WHERE 1=1 
                           AND A.MAT_CLASS = :MAT_CLASS  
                        --   AND A.M_STOREID = '1'
                           AND nvl(A.CANCEL_ID, 'N') <> 'Y' ";

            if (dgMastInid != "")
            {
                sql += @" AND (select count(*) from DGMISS_MAST 
                                where MMCODE = A.MMCODE and INID = :APP_INID)> 0 ";
                p.Add(":APP_INID", dgMastInid);
            }

            if (query.MMCODE != "")
            {
                sql += sBr + " AND UPPER(A.MMCODE) LIKE :MMCODE ";
            }
            if (query.MMNAME_C != "")
            {
                sql += sBr + " AND A.MMNAME_C LIKE :MMNAME_C ";
            }
            if (query.MMNAME_E != "")
            {
                sql += sBr + " AND UPPER(A.MMNAME_E) LIKE UPPER(:MMNAME_E) ";
            }
            if (query.ISCONTID3 == "Y")
            {
                sql += sBr + " AND A.M_CONTID = '3' ";
            }
            else
            {
                sql += sBr + " AND A.M_CONTID <> '3' ";
            }
            if (query.M_AGENNO != "")
            {
                sql += sBr + " AND A.M_AGENNO LIKE :M_AGENNO ";
            }
            if (query.AGEN_NAME != "")
            {
                sql += sBr + " AND ((select AGEN_NAMEC from PH_VENDER where AGEN_NO = A.M_AGENNO) LIKE :AGEN_NAME ";
                sql += sBr + "  OR (select AGEN_NAMEE from PH_VENDER where AGEN_NO = A.M_AGENNO) LIKE :AGEN_NAME) ";
            }

            p.Add(":WH_NO", query.WH_NO);
            p.Add(":MAT_CLASS", query.MAT_CLASS);
            p.Add(":MMCODE", string.Format("%{0}%", query.MMCODE));
            p.Add(":MMNAME_C", string.Format("%{0}%", query.MMNAME_C));
            p.Add(":MMNAME_E", string.Format("%{0}%", query.MMNAME_E));
            p.Add(":M_AGENNO", string.Format("%{0}%", query.M_AGENNO));
            p.Add(":AGEN_NAME", string.Format("%{0}%", query.AGEN_NAME));

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public int ChkDetailCnt(string docno)
        {
            string sql = @"SELECT count(*) FROM DGMISS WHERE DOCNO=:DOCNO and MMCODE is not null ";
            return Convert.ToInt32(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = docno }, DBWork.Transaction));
        }

        public string GetDgmissMast(string appINID)
        {
            // 用主檔的補藥單位找補藥院內碼基本檔
            // 因增加補藥院內碼基本檔，如有設定，則藥材代碼只顯示存在AB基本檔的院內碼
            string sql = @"select 1 from DGMISS_MAST where INID = :APP_INID and rownum = 1 ";
            string rtn = Convert.ToString(DBWork.Connection.ExecuteScalar(sql, new { APP_INID = appINID }, DBWork.Transaction));
            return rtn;
        }

        public class MI_MAST_QUERY_PARAMS
        {
            public string MMCODE;
            public string MMNAME_C;
            public string MMNAME_E;
            public string MAT_CLASS;

            public string WH_NO;
            public string IS_INV;  // 需判斷庫存量>0
            public string E_IFPUBLIC;  // 是否公藥
            public string ISCONTID3;

            public string M_AGENNO;
            public string AGEN_NAME;
        }

    }
}
