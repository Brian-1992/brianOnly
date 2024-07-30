using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Data;
using System;
using System.Data.SqlClient;
using System.Text;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using WebApp.Models.UT;

namespace WebApp.Repository.AB
{
    public class AB0126Repository : JCLib.Mvc.BaseRepository
    {
        public AB0126Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ColumnItem> GetColumnItems()
        {
            var sql = @"SELECT WH_NO as DATAINDEX, WH_NAME as TEXT FROM MI_WHMAST WHERE WH_KIND = '0' and WH_GRADE ='2' and CANCEL_ID = 'N' order by WH_NO ";
            return DBWork.Connection.Query<ColumnItem>(sql, null, DBWork.Transaction);
        }

        public IEnumerable<AB0126> GetAll(string mmcode, string mmname_c, string mmname_e, string is_restrict, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select T.MMCODE, T.MMNAME_C, T.MMNAME_E, T.E_RESTRICTCODE, T.WEXP_ID,
                listagg(nvl(T.INV_QTY, '0'), ',')
                within group (order by WH_NO) as EXTRA_DATA
                from (
                    select A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.E_RESTRICTCODE, A.WEXP_ID, A.WH_NO, nvl(B.INV_QTY, 0) as INV_QTY
                    from (
                        select A1.MMCODE, A1.MMNAME_C, A1.MMNAME_E, A1.E_RESTRICTCODE, A1.WEXP_ID, A2.WH_NO 
                        from (select distinct C.MMCODE, C.MMNAME_C, C.MMNAME_E, C.E_RESTRICTCODE, C.WEXP_ID from MI_MAST C, MI_WHINV D where C.MMCODE = D.MMCODE and (select MAT_CLSID from MI_MATCLASS where Trim(MAT_CLASS) = Trim(C.MAT_CLASS)) = '1') A1, 
                        (select distinct WH_NO from MI_WHMAST where WH_KIND = '0' and WH_GRADE = '2' and CANCEL_ID = 'N') A2 
                        order by A1.MMCODE, A2.WH_NO
                    ) A
                    left join (
                        select WH_NO, MMCODE, INV_QTY from MI_WHINV
                    ) B 
                    on A.MMCODE = B.MMCODE and A.WH_NO = B.WH_NO
                    order by MMCODE, WH_NO
                ) T
                where 1=1 ";

            if (!String.IsNullOrEmpty(mmcode))
            {
                sql += " and T.MMCODE like :MMCODE ";
                p.Add(":MMCODE", string.Format("%{0}%", mmcode));
            }

            if (!String.IsNullOrEmpty(mmname_c))
            {
                sql += " and T.MMNAME_C like :MMNAME_C ";
                p.Add(":MMNAME_C", string.Format("%{0}%", mmname_c));
            }

            if (!String.IsNullOrEmpty(mmname_e))
            {
                sql += " and T.MMNAME_E like :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", mmname_e));
            }

            if (is_restrict == "Y")
                sql += " and T.E_RESTRICTCODE in ('0', '1', '2', '3', '4') ";
            else if (is_restrict == "N")
                sql += " and T.E_RESTRICTCODE = 'N' ";

            sql += " group by T.MMCODE, T.MMNAME_C, T.MMNAME_E, T.E_RESTRICTCODE, T.WEXP_ID ";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AB0126>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<AB0126> GetLotNo(string wh_no, string mmcode)
        {
            var sql = @"select LOT_NO, TO_CHAR(EXP_DATE, 'YYYY/MM/DD') EXP_DATE, INV_QTY from MI_WEXPINV where WH_NO = :WH_NO and MMCODE = :MMCODE ";
            // sql += @" and EXP_DATE > TRUNC(sysdate) "; // 只顯示尚未過效期的資料

            return DBWork.Connection.Query<AB0126>(sql, new { WH_NO = wh_no, MMCODE = mmcode }, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMMCode(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.WEXP_ID  
                        FROM MI_MAST A where (select MAT_CLSID from MI_MATCLASS where Trim(MAT_CLASS) = Trim(A.MAT_CLASS)) = '1'  ";
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
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetTowhCombo(string tuser)
        {
            DynamicParameters p = new DynamicParameters();
            StringBuilder sql = new StringBuilder();
            sql.Append("select A.WH_NO as VALUE , A.WH_NAME as TEXT ,A.WH_NO||' '||A.WH_NAME as COMBITEM from MI_WHMAST A ");
            sql.Append(" where A.WH_KIND='0' and A.WH_GRADE = '2' and A.CANCEL_ID = 'N' ");

            if (tuser != "")
            {
                sql.Append(" and A.WH_NO in (select WH_NO from MI_WHID where WH_USERID = :TUSER) ");
                p.Add(":TUSER", string.Format("{0}", tuser));
            }

            sql.Append(" ORDER BY A.WH_NO");

            return DBWork.Connection.Query<COMBO_MODEL>(sql.ToString(), p);
        }

        public IEnumerable<COMBO_MODEL> GetFrwhCombo(string towh, string tuser)
        {
            DynamicParameters p = new DynamicParameters();
            StringBuilder sql = new StringBuilder();
            sql.Append("select A.WH_NO as VALUE , A.WH_NAME as TEXT ,A.WH_NO||' '||A.WH_NAME as COMBITEM from MI_WHMAST A ");
            sql.Append(" where A.WH_KIND='0' and A.WH_GRADE = '2' and A.CANCEL_ID = 'N' ");
            
            if (towh != "")
            {
                sql.Append(" and A.WH_NO <> :WH_NO ");
                p.Add(":WH_NO", string.Format("{0}", towh));
            }

            if (tuser != "")
            {
                sql.Append(" and A.WH_NO in (select WH_NO from MI_WHID where WH_USERID = :TUSER) ");
                p.Add(":TUSER", string.Format("{0}", tuser));
            }

            sql.Append(" ORDER BY A.WH_NO");

            return DBWork.Connection.Query<COMBO_MODEL>(sql.ToString(), p);
        }

        public int GetWhQty(string mmcode, string wh_no)
        {
            string sql = @"select INV_QTY from MI_WHINV
                    where MMCODE = :MMCODE and WH_NO = :WH_NO ";
            return Convert.ToInt32(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = mmcode, WH_NO = wh_no }, DBWork.Transaction));
        }

        public int ChkMmcodeValid(string mmcode)
        {
            string sql = @"select count(*) from MI_MAST A
                    where MMCODE = :MMCODE and (select MAT_CLSID from MI_MATCLASS where Trim(MAT_CLASS) = Trim(A.MAT_CLASS)) = '1' ";
            return Convert.ToInt32(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = mmcode}, DBWork.Transaction));
        }

        public bool CheckWhCancelByWhno(string wh_no)
        {
            string sql = @"
               select 1 from MI_WHMAST 
                where wh_no = :WH_NO
                  and cancel_id = 'N'
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { WH_NO = wh_no}, DBWork.Transaction) == null;
        }

        public bool CheckFrwhMmcodeValid(string frwh, string mmcode)
        {
            string sql = @"select 1 
                             from MI_WHINV
                            where WH_NO = :FRWH
                              and MMCODE = :MMCODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { FRWH = frwh, MMCODE = mmcode }, DBWork.Transaction) == null);
        }

        //國軍DOCNO單號統一用GET_DAILY_DOCNO
        public string GetDailyDocno()
        {
            string sql = @"select GET_DAILY_DOCNO from DUAL";
            string rtn = DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction).ToString();
            return rtn;
        }
        public int CreateM(ME_DOCM ME_DOCM)
        {
            var sql = @"insert into ME_DOCM (
                        DOCNO, DOCTYPE, FLOWID, APPID, APPDEPT , 
                        APPTIME, FRWH , TOWH , MAT_CLASS,
                        CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                      values(
                        :DOCNO, :DOCTYPE, :FLOWID , :APPID, (select INID from UR_ID where TUSER =:USEID), 
                        SYSDATE, :FRWH, :TOWH , :MAT_CLASS,
                        SYSDATE, :USEID, SYSDATE, :UPDATE_USER, :UPDATE_IP ) ";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);
        }

        public string GetDocDSeq(string id)
        {
            string sql = @"SELECT NVL(MAX(SEQ),0)+1 as SEQ FROM ME_DOCD WHERE DOCNO=:DOCNO ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction).ToString();
            return rtn;
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

        public bool CheckWexpinvExists(string wh_no, string mmcode, string lot_no, string exp_date)
        {
            string sql = @"
                select 1 from MI_WEXPINV
                 where WH_NO = :wh_no
                   and MMCODE = :mmcode
                   and LOT_NO = :lot_no
                   and twn_date(EXP_DATE) = :exp_date
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { wh_no, mmcode, lot_no, exp_date }, DBWork.Transaction) != null;
        }
        public int InsertWexpinv(string wh_no, string mmcode, string exp_date, string lot_no, string inv_qty,
                                  string userId, string ip)
        {
            string sql = @"
                insert into MI_WEXPINV(WH_NO, MMCODE, EXP_DATE, LOT_NO, INV_QTY, 
                                       CREATE_USER, UPDATE_IP, CREATE_TIME)
                 values (:wh_no, :mmcode, twn_todate(:exp_date), :lot_no, :inv_qty, 
                         :userId, :ip, sysdate)
            ";
            return DBWork.Connection.Execute(sql, new { wh_no, mmcode, exp_date, lot_no, inv_qty, userId, ip }, DBWork.Transaction);
        }

        public int UpdateWexpinv(string wh_no, string mmcode, string exp_date, string lot_no, string invqty, string procSign, string tuser, string userIp)
        {
            // 舊藥衛材系統使用者可能習慣不填LOT_NO,轉入MI_WEXPINV的LOT_NO可能為null
            string lotnoCond = "";
            if (lot_no == "" || lot_no == null)
                lotnoCond = " and LOT_NO is null ";
            else
                lotnoCond = " and LOT_NO = :LOT_NO ";

            string sql = @" update MI_WEXPINV
                set INV_QTY = INV_QTY " + procSign + @" :INVQTY, UPDATE_USER = :TUSER, UPDATE_TIME = sysdate, UPDATE_IP = :USERIP
                where WH_NO = :WH_NO and MMCODE = :MMCODE " + lotnoCond + @" and EXP_DATE = twn_todate(:EXP_DATE)
                ";
            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, MMCODE = mmcode, LOT_NO = lot_no, EXP_DATE = exp_date, INVQTY = invqty, TUSER = tuser, USERIP = userIp }, DBWork.Transaction);
        }

        public bool CheckWlocinvExists(string wh_no, string mmcode)
        {
            string sql = @"SELECT 1 FROM MI_WLOCINV WHERE WH_NO=:WH_NO AND MMCODE = :MMCODE ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { WH_NO = wh_no, MMCODE = mmcode }, DBWork.Transaction) == null);
        }

        public int InsertWloc(string wh_no, string mmcode, string invqty, string tuser, string userIp)
        {
            string sql = string.Format(@" insert into MI_WLOCINV (WH_NO, MMCODE, STORE_LOC, INV_QTY, CREATE_USER, CREATE_TIME, UPDATE_IP)
                values(:WH_NO, :MMCODE, 'TMPLOC', :INVQTY, :TUSER, sysdate, :USERIP)
                ");
            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, MMCODE = mmcode, INVQTY = invqty, TUSER = tuser, USERIP = userIp }, DBWork.Transaction);
        }

        public int UpdateWloc(string wh_no, string mmcode, string invqty, string procSign, string tuser, string userIp)
        {
            string sql = @" update MI_WLOCINV A
                set INV_QTY = INV_QTY " + procSign + @" :INVQTY, UPDATE_USER = :TUSER, UPDATE_TIME = sysdate, UPDATE_IP = :USERIP
                where WH_NO = :WH_NO and MMCODE = :MMCODE 
                and STORE_LOC = (select STORE_LOC from MI_WLOCINV where WH_NO = A.WH_NO and MMCODE = A.MMCODE and rownum = 1)
                ";
            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, MMCODE = mmcode, INVQTY = invqty, TUSER = tuser, USERIP = userIp }, DBWork.Transaction);
        }

        public int CreateDocexp(UT_DOCD ut_docd)
        {
            var sql = @"INSERT INTO ME_DOCEXP(DOCNO,SEQ,EXP_DATE,LOT_NO,MMCODE,APVQTY,UPDATE_TIME,UPDATE_USER,UPDATE_IP)   
                                   VALUES(:DOCNO,:SEQ,:EXP_DATE,:LOT_NO,:MMCODE,:APVQTY,SYSDATE,:UPDATE_USER,:UPDATE_IP)";
            return DBWork.Connection.Execute(sql, ut_docd, DBWork.Transaction);
        }

        public int UpdateALLApvid(ME_DOCD pME_DOCD)
        {
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

        public int UpdateFstackDate(string towh, string docno)
        {
            var sql = @"update MI_WINVCTL
                           set FSTACKDATE=trunc(sysdate)
                         where WH_NO=:TOWH 
                           and MMCODE in (select MMCODE from ME_DOCD where DOCNO=:DOCNO)
                           and FSTACKDATE is null";
            return DBWork.Connection.Execute(sql, new { TOWH = towh, DOCNO = docno }, DBWork.Transaction);
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

        public DataTable GetExcel()
        {
            var p = new DynamicParameters();

            var sql = @" SELECT '' as 院內碼, '' as 調撥量, '' as 批號, '' as ""效期(YYYMMDD)"" FROM DUAL ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public bool CheckExistsMMCODE(string id)
        {
            string sql = @" SELECT 1 FROM MI_MAST WHERE 1=1 
                          AND MMCODE = :MMCODE ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = id }, DBWork.Transaction) == null);
        }

        public bool CheckMatClassMMCODE(string id1, string id2)
        {
            string sql = @" SELECT 1 FROM MI_MAST WHERE 1=1 
                          AND MAT_CLASS = :MAT_CLASS 
                          AND MMCODE = :MMCODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MAT_CLASS = id1, MMCODE = id2 }, DBWork.Transaction) == null);
        }

        public bool CheckWexpidMMCODE(string mmcode)
        {
            string sql = @" SELECT 1 FROM MI_MAST 
                          WHERE MMCODE = :MMCODE
                          AND WEXP_ID = 'Y' ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = mmcode }, DBWork.Transaction) == null);
        }

        public bool CheckTwndate(string twndate)
        {
            string sql = @" SELECT 1 FROM dual 
                          WHERE twn_todate(:TWNDATE) is not null ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { TWNDATE = twndate }, DBWork.Transaction) == null);
        }

        public string GetWexpid(string mmcode)
        {
            string sql = @"SELECT WEXP_ID FROM MI_MAST WHERE MMCODE=:MMCODE ";
            string rtn = Convert.ToString(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = mmcode }, DBWork.Transaction));
            return rtn;
        }

        public IEnumerable<MI_MAST> GetMmCodeCombo(string p0, int page_index, int page_size, string sorters)
        {

            var p = new DynamicParameters();

            var sql = @"
                       SELECT distinct {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E
                          from MI_WHINV b, MI_MAST a
                         where exists (select 1 from MI_WHMAST where wh_no = b.wh_no and wh_grade = '2' and wh_kind='0')
                           and b.mmcode = a.mmcode 
                           and nvl(a.cancel_id, 'N')='N'";
            sql += " {1} ";

            if (p0 != "")
            {
                sql = string.Format(sql,
                     "(NVL(INSTR(UPPER(A.MMCODE), UPPER(:MMCODE_I)), 1000) ) IDX,",
                     @"   AND (UPPER(A.MMCODE) LIKE UPPER(:MMCODE))");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMCODE", string.Format("{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "", "");
                sql += " ORDER BY MMCODE ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
    }
}