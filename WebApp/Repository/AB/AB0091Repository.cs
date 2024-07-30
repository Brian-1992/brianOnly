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
    public class AB0091Repository : JCLib.Mvc.BaseRepository
    {
        public AB0091Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ME_DOCD> GetAll(string wh_no, string docno, string mmcode, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @" select DOCNO,(select APPDEPT from ME_DOCM where DOCNO=A.DOCNO) as APPDEPT, SEQ, MMCODE,
                            (select MMNAME_C from MI_MAST where MMCODE=A.MMCODE) as MMNAME_C,
                            (select MMNAME_E from MI_MAST where MMCODE=A.MMCODE) as MMNAME_E,
                            PICK_QTY,
                            ACKQTY,
                            (select BASE_UNIT from MI_MAST where MMCODE=A.MMCODE) as BASE_UNIT,
                            (select WEXP_ID from MI_MAST where MMCODE=A.MMCODE) as WEXP_ID,
                            (select DATA_DESC from PARAM_D 
                            where GRP_CODE='MI_MAST' 
                            and DATA_NAME='WEXP_ID' 
                            and DATA_VALUE=(select WEXP_ID from MI_MAST where MMCODE=A.MMCODE)) as WEXP_ID_DESC,
                            ACKID, POSTID, (select FLOWID from ME_DOCM where DOCNO = A.DOCNO) as FLOWID
                            from ME_DOCD A
                            where 1=1 ";

            if (docno != "" && docno != null)
            {
                sql += @" and DOCNO=:DOCNO ";
                p.Add(":DOCNO", docno);
            }
            else
            {
                sql += @" and DOCNO in (select DOCNO from ME_DOCM A where TOWH = :WH_NO 
                            and DOCTYPE in ('MR','MS','MR1','MR2','MR3','MR4')
                            and FLOWID in ('0102','0103','0602','0603','3','4')
                            and DOCNO in (select distinct DOCNO from BC_WHPICK 
                            where WH_NO = A.FRWH 
                            and PICK_DATE>sysdate-10 
                            and HAS_SHIPOUT is not null))
                        and MMCODE = :MMCODE ";
                p.Add(":WH_NO", wh_no);
                p.Add(":MMCODE", mmcode);
            }
            

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCD>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCD> GetDetailAll(string docno, string mmcode, string barcode, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @" select docno,(select appdept from ME_DOCM where docno=a.docno) as appdept,
                                seq,mmcode,
                                (select barcode from BC_BARCODE where mmcode=a.mmcode and barcode=:p2 and rownum = 1) as barcode,
                                nvl((select tratio from BC_BARCODE where mmcode=a.mmcode and barcode=:p2 and rownum = 1), 1) as tratio,
                                (select mmname_c from MI_MAST where mmcode=a.mmcode) as mmname_c,
                                (select mmname_e from MI_MAST where mmcode=a.mmcode) as mmname_e,pick_qty,ackqty,
                                (select base_unit from MI_MAST where mmcode=a.mmcode) as base_unit,
                                (select wexp_id from MI_MAST where mmcode=a.mmcode) as wexp_id,
                                (select data_desc from param_d where grp_code='MI_MAST' and data_name='WEXP_ID' 
                                and data_value=(select wexp_id from MI_MAST where mmcode=a.mmcode)) as wexp_id_desc,ackid, postid,
                                (select FLOWID from ME_DOCM where DOCNO = A.DOCNO) as FLOWID,
                                (select STORE_LOC from MI_WLOCINV 
                                  where WH_NO = (select TOWH from ME_DOCM where DOCNO = a.DOCNO) 
                                    and MMCODE = A.MMCODE and rownum = 1) as STORE_LOC
                            from ME_DOCD a
                            where docno=:p0
                            and (mmcode=:p1
                            or mmcode in (select mmcode from BC_WHPICK 
                            where docno=:p0
                            and barcode=:p2))";

            p.Add(":p0", docno);
            p.Add(":p1", mmcode);
            p.Add(":p2", barcode);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCD>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<BC_WHPICK_VALID> GetValidAll(string docno, string seq, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @" select DOCNO, SEQ, LOT_NO, TWN_DATE(VALID_DATE) as VALID_DATE, ACT_PICK_QTY
                            from BC_WHPICK_VALID
                            where 1=1 ";

            sql += " AND DOCNO = :p0 ";
            p.Add(":p0", docno);

            sql += " AND SEQ = :p1 ";
            p.Add(":p1", seq);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<BC_WHPICK_VALID>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public int Update(string docno, string mmcode, string ackqty, string setType,
                        string name, string procip)
        {
            var sql = @" UPDATE ME_DOCD ";
            if (setType == "U")
            {
                sql = sql + @" set postid='4', ACKID=:NAME, ACKQTY=:ACKQTY,ACKTIME=SYSDATE,                                   
                                   UPDATE_TIME=SYSDATE,UPDATE_USER=:NAME,UPDATE_IP=:UPDATE_IP ";
            }
            //else if (setType == "D")
            //{
            //    sql = sql + @" set ACKID='', ACKQTY=(select PICK_QTY from ME_DOCD where DOCNO = :DOCNO and MMCODE = :MMCODE), ACKTIME=null,
            //                UPDATE_TIME = SYSDATE, UPDATE_USER = :NAME, UPDATE_IP = :UPDATE_IP ";
            //}

            sql = sql + @" where DOCNO=:DOCNO and MMCODE=:MMCODE ";

            return DBWork.Connection.Execute(sql, new { DOCNO = docno, MMCODE = mmcode, ACKQTY = ackqty, NAME = name, UPDATE_IP = procip }, DBWork.Transaction);
        }

        public int UpdateTmp(string docno, string mmcode, string ackqty)
        {
            var sql = @" UPDATE ME_DOCD set ACKQTY=:ACKQTY
                            where DOCNO=:DOCNO and MMCODE=:MMCODE";

            return DBWork.Connection.Execute(sql, new { DOCNO = docno, MMCODE = mmcode, ACKQTY = ackqty}, DBWork.Transaction);
        }

        public int updateAckqtyByAdjqty(string docno, string mmcode, int adjqty)
        {
            // 若核撥量=點收量(本品項尚未點收過),則更新為異動量
            var sql = @" UPDATE ME_DOCD set ACKQTY = (case when PICK_QTY = ACKQTY then :ADJQTY else ACKQTY + :ADJQTY end)
                            where DOCNO=:DOCNO and MMCODE=:MMCODE";

            return DBWork.Connection.Execute(sql, new { DOCNO = docno, MMCODE = mmcode, ADJQTY = adjqty }, DBWork.Transaction);
        }

        public int UpdateDetailAckid(string docno, string name, string procip, string isDrm)
        {
            string extraSql = "";
            if (isDrm == "Y")
                extraSql = " ,postid = '4' ";

            var sql = @" update ME_DOCD 
                            set ACKID=:NAME,ACKTIME=SYSDATE,       
                                UPDATE_TIME=SYSDATE,UPDATE_USER=:NAME,UPDATE_IP=:UPDATE_IP " + extraSql +
                        " where DOCNO=:DOCNO and ACKID is null ";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno, NAME = name, UPDATE_IP = procip }, DBWork.Transaction);
        }

        // 藥品之外的申請單更新申請單狀態為點收中
        public int UpdateNotDrm(string docno, string mmcode, string ackqty, string setType,
                        string name, string procip)
        {
            var sql = @" UPDATE ME_DOCM set FLOWID = '4', UPDATE_TIME = SYSDATE, UPDATE_USER = :NAME, UPDATE_IP = :UPDATE_IP
                            where DOCNO=:DOCNO and DOCTYPE in ('MR1', 'MR2', 'MR3', 'MR4') "; // and MAT_CLASS <> '01'

            return DBWork.Connection.Execute(sql, new { DOCNO = docno, MMCODE = mmcode, ACKQTY = ackqty, NAME = name, UPDATE_IP = procip }, DBWork.Transaction);
        }

        // 整張申請單點收完成更新申請單狀態(藥品)
        // ※若資料可能出現PICK_QTY或ACKQTY,檢查兩者不等的判斷可嘗試用:
        // and ((PICK_QTY <> ACKQTY) or (PICK_QTY is null and ACKQTY is not null) or (PICK_QTY is not null and ACKQTY is null))) = 0 
        public int UpdateDocStatusDrm(string docno, string name, string procip)
        {
            var sql = @" update ME_DOCM A set A.FLOWID = 
                            case when A.DOCTYPE='MS' then '0699'
                            else
                            (case when (select count(*) from ME_DOCD where DOCNO = A.DOCNO and (PICK_QTY <> ACKQTY)) = 0 
                            then '0104' else '0199' end)
                            end,
                            UPDATE_TIME = SYSDATE, UPDATE_USER = :NAME, UPDATE_IP = :UPDATE_IP
                            where A.DOCNO = :DOCNO and DOCTYPE in ('MR', 'MS') "; // and A.MAT_CLASS='01' // 現在藥品MAT_CLASS會是null,先取消此條件

            return DBWork.Connection.Execute(sql, new { DOCNO = docno, NAME = name, UPDATE_IP = procip }, DBWork.Transaction);
        }

        // 整張申請單點收完成更新申請單狀態(非藥品)
        public int UpdateDocStatusNotDrm(string docno, string name, string procip)
        {
            var sql = @" update ME_DOCM set FLOWID = '5',
                            UPDATE_TIME = SYSDATE, UPDATE_USER = :NAME, UPDATE_IP = :UPDATE_IP
                            where DOCNO = :DOCNO and DOCTYPE in ('MR1', 'MR2', 'MR3', 'MR4') "; // and MAT_CLASS<>'01'

            return DBWork.Connection.Execute(sql, new { DOCNO = docno, NAME = name, UPDATE_IP = procip }, DBWork.Transaction);
        }

        public int UpdateDocStatusDrmRev(string docno, string name, string procip)
        {
            var sql = @" update ME_DOCM A set A.FLOWID = 
                            case when A.DOCTYPE='MS' then '0603'
                            else
                            '0103'
                            end,
                            UPDATE_TIME = SYSDATE, UPDATE_USER = :NAME, UPDATE_IP = :UPDATE_IP
                            where A.DOCNO = :DOCNO and DOCTYPE in ('MR', 'MS') "; // and A.MAT_CLASS='01'

            return DBWork.Connection.Execute(sql, new { DOCNO = docno, NAME = name, UPDATE_IP = procip }, DBWork.Transaction);
        }

        public int UpdateDocStatusNotDrmRev(string docno, string name, string procip)
        {
            var sql = @" update ME_DOCM set FLOWID = '4',
                            UPDATE_TIME = SYSDATE, UPDATE_USER = :NAME, UPDATE_IP = :UPDATE_IP
                            where DOCNO = :DOCNO and DOCTYPE in ('MR1', 'MR2', 'MR3', 'MR4')  "; // and MAT_CLASS<>'01'

            return DBWork.Connection.Execute(sql, new { DOCNO = docno, NAME = name, UPDATE_IP = procip }, DBWork.Transaction);
        }

        public IEnumerable<MI_WHMAST> GetWH_NoCombo(string p0, string userid, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.WH_NO, A.WH_NAME, A.WH_KIND, B.WH_USERID FROM MI_WHMAST A, MI_WHID B
                            WHERE A.WH_NO=B.WH_NO and A.WH_GRADE in ('1', '2') and B.WH_USERID=:p1 ";
            p.Add(":p1", userid);

            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.WH_NO, :WH_NO_I), 1000) + NVL(INSTR(A.WH_NAME, :WH_NAME_I), 100) * 10) IDX,"); // 設定權重, 值越小權重最大                
                p.Add(":WH_NO_I", p0);
                p.Add(":WH_NAME_I", p0);

                sql += " AND (A.WH_NO LIKE :WH_NO ";
                p.Add(":WH_NO", string.Format("%{0}%", p0));

                sql += " OR A.WH_NAME LIKE :WH_NAME) ";
                p.Add(":WH_NAME", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX, WH_NO", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY A.WH_NO ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_WHMAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetWH_NoComboSimple(string userid)
        {
            var p = new DynamicParameters();

            var sql = @" SELECT A.WH_NO as VALUE, A.WH_NO || ' ' || A.WH_NAME as COMBITEM FROM MI_WHMAST A, MI_WHID B
                            WHERE A.WH_NO=B.WH_NO and A.WH_GRADE in ('1', '2') and B.WH_USERID=:p0";

            p.Add(":p0", userid);

            sql += " order by A.WH_NO ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetDocnoCombo(string wh_no)
        {
            var p = new DynamicParameters();

            var sql = @" select DOCNO as VALUE, MAT_CLASS, DOCTYPE
                            from ME_DOCM A
                            where TOWH=:p0
                            and DOCTYPE in ('MR', 'MS', 'MR1','MR2','MR3','MR4') 
                            and FLOWID in ('0102', '0103', '0602', '0603', '3','4')
                            and DOCNO in (select distinct docno 
                            from BC_WHPICK
                            where WH_NO=A.FRWH and PICK_DATE>sysdate-10
                            and HAS_SHIPOUT is not null) ";

            p.Add(":p0", wh_no);

            sql += " order by docno ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p, DBWork.Transaction);
        }

        public int GetAckCnt(string docno, string getType)
        {
            var p = new DynamicParameters();
            var sql = @" select count(*) as CNT from ME_DOCD where DOCNO = :p0 ";

            if (getType == "Y")
                sql += " and ACKID is not null ";
            else if (getType == "N")
                sql += " and ACKID is null ";

            p.Add(":p0", docno);

            return DBWork.Connection.QueryFirst<int>(sql, p, DBWork.Transaction);
        }

        public string ChkMmcode(string docno, string mmcode)
        {
            var p = new DynamicParameters();
            var sql = @" select case when (select count(*) from ME_DOCD A where DOCNO=:p0 
                            and ACKID is null 
                            and (MMCODE=:p1 or MMCODE in (select MMCODE from BC_WHPICK where DOCNO = :p0 and BARCODE = :p1))) > 0
                            then :p1
                            else 'notfound' end as MMCODE
                            from dual ";

            p.Add(":p0", docno);
            p.Add(":p1", mmcode);

            return DBWork.Connection.QueryFirst<string>(sql, p, DBWork.Transaction);
        }

        public string ChkMmcode(string mmcode)
        {
            var p = new DynamicParameters();
            var sql = @" select case when (select count(*) from ME_DOCD A where (MMCODE=:MMCODE or MMCODE = (select MMCODE from BC_WHPICK where BARCODE = :MMCODE and rownum = 1))) > 0
                            then :MMCODE
                            when (select count(*) from BC_BARCODE where BARCODE = :MMCODE) > 0
                            then (select MMCODE from BC_BARCODE where BARCODE = :MMCODE)
                            else 'notfound' end as MMCODE
                            from dual ";

            p.Add(":MMCODE", mmcode);

            return DBWork.Connection.QueryFirst<string>(sql, p, DBWork.Transaction);
        }

        public int ChkMmcodeDup(string docno, string mmcode)
        {
            var p = new DynamicParameters();
            var sql = @" select count(*) from ME_DOCD where DOCNO = :p0 and MMCODE = :p1 
                            and (ACKID is null or trim(ACKID) = '') ";

            p.Add(":p0", docno);
            p.Add(":p1", mmcode);

            return DBWork.Connection.QueryFirst<int>(sql, p, DBWork.Transaction);
        }

        public string getScannerVal(string scanner)
        {
            var p = new DynamicParameters();
            var sql = @" select case when (select count(*) from BC_BARCODE where BARCODE=:p0) > 0
                            then (select MMCODE || '^' || TRATIO from BC_BARCODE where BARCODE=:p0 and rownum = 1)
                            when (select count(*) from MI_MAST where MMCODE=:p0) > 0
                            then :p0
                            else 'notfound' end as MMCODE
                            from dual ";

            p.Add(":p0", scanner);

            return DBWork.Connection.QueryFirst<string>(sql, p, DBWork.Transaction);
        }

        public string getMatClassByDocno(string docno)
        {
            var p = new DynamicParameters();
            var sql = @" select case when MAT_CLASS = '01' then 'Y' when DOCTYPE in ('MR', 'MS') then 'Y' else 'N' end from ME_DOCM
                            where DOCNO = :p0 ";

            p.Add(":p0", docno);

            return DBWork.Connection.QueryFirst<string>(sql, p, DBWork.Transaction);
        }

        public string getNewAckqty(string docno, string mmcode)
        {
            var p = new DynamicParameters();
            var sql = @" select ACKQTY from ME_DOCD where DOCNO = :DOCNO and MMCODE = :MMCODE ";

            p.Add(":DOCNO", docno);
            p.Add(":MMCODE", mmcode);

            return DBWork.Connection.QueryFirst<string>(sql, p, DBWork.Transaction);
        }

        public int chkBcWhpick(string docno)
        {
            var p = new DynamicParameters();
            var sql = @" select count(*) from BC_WHPICK
                            where docno=:DOCNO and has_shipout is null ";

            return DBWork.Connection.QueryFirst<int>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public IEnumerable<MI_WHMAST> GetChkUserWhno(string userId)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.WH_NO, A.WH_NAME from MI_WHMAST A, MI_WHID B
                            WHERE A.WH_NO=B.WH_NO and rownum = 1 ";

            if (userId != "" && userId != null)
            {
                sql += " AND B.WH_USERID = :p0 ";
                p.Add(":p0", userId);
            }

            return DBWork.Connection.Query<MI_WHMAST>(sql, p, DBWork.Transaction);
        }

        public string POST_DOC(string I_DOCNO, string I_UPDUSR, string I_UPDIP)
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
            return retid;
        }



        #region AB0091_1
        public IEnumerable<ME_DOCD> DetailQuery(string docno, string barcode) {
            var p = new DynamicParameters();

            var sql = @" 
                         with temp_store_loc as (
                              select mmcode, min(store_loc) as store_loc
                                from MI_WLOCINV 
                               where wh_no = (select towh from ME_DOCM where docno = :p0)
                               group by mmcode
                               order by mmcode
                         )
                         select docno,(select appdept from ME_DOCM where docno=a.docno) as appdept,
                                seq,mmcode,
                                (select barcode from BC_BARCODE where mmcode=a.mmcode and barcode=:p1 and rownum = 1) as barcode,
                                (select tratio from BC_BARCODE where mmcode=a.mmcode and barcode=:p1 and rownum = 1) as tratio,
                                (select mmname_c from MI_MAST where mmcode=a.mmcode) as mmname_c,
                                (select mmname_e from MI_MAST where mmcode=a.mmcode) as mmname_e,pick_qty,ackqty,
                                (select base_unit from MI_MAST where mmcode=a.mmcode) as base_unit,
                                (select wexp_id from MI_MAST where mmcode=a.mmcode) as wexp_id,
                                (select data_desc from param_d where grp_code='MI_MAST' and data_name='WEXP_ID' 
                                and data_value=(select wexp_id from MI_MAST where mmcode=a.mmcode)) as wexp_id_desc,ackid, postid,
                                (select FLOWID from ME_DOCM where DOCNO = A.DOCNO) as FLOWID,
                                (select STORE_LOC from temp_store_loc where mmcode = a.mmcode ) as STORE_LOC,
                                a.acc_ackqty as acc_ackqty
                           from ME_DOCD a
                          where docno=:p0
                            and (mmcode=:p1
                                    or mmcode in (select mmcode from BC_BARCODE where barcode = :p1)
                                )";

            p.Add(":p0", docno);
            p.Add(":p1", barcode);

            return DBWork.PagingQuery<ME_DOCD>(sql, p, DBWork.Transaction);

        }

        public int UpdateAccAckQty(string docno, string mmcode, int addqty, string userId, string ip) {
            string sql = @"
                update ME_DOCD
                   set acc_ackqty = acc_ackqty + :addqty,
                       ackqty = acc_ackqty + :addqty,
                       update_time = sysdate,
                       update_user = :userId,
                       update_ip = :ip
                 where docno = :docno
                   and mmcode = :mmcode
            ";
            return DBWork.Connection.Execute(sql, new { docno, mmcode, addqty, userId, ip }, DBWork.Transaction);
        }

        public ME_DOCD GetDocd(string docno, string mmcode) {
            string sql = @"
                select * from ME_DOCD
                 where docno = :docno and mmcode = :mmcode
            ";
            return DBWork.Connection.QueryFirstOrDefault<ME_DOCD>(sql, new { docno, mmcode }, DBWork.Transaction);
        }
        public int UpdateDocdPostId4(string docno, string mmcode, string userId, string ip) {
            string sql = @"
                update ME_DOCD 
                   set postid='4', ACKID=:userId,ACKTIME=SYSDATE,                       
                       UPDATE_TIME=SYSDATE,UPDATE_USER=:userId,UPDATE_IP=:ip
                 where docno = :docno
                   and mmcode = :mmcode
            ";
            return DBWork.Connection.Execute(sql, new { docno, mmcode, userId, ip }, DBWork.Transaction);
        }
        public ME_DOCM GetDocm(string docno) {
            string sql = @"
                select * from ME_DOCM
                 where docno = :docno
            ";
            return DBWork.Connection.QueryFirstOrDefault<ME_DOCM>(sql, new { docno }, DBWork.Transaction);
        }
        public string GetAccAckQty(string docno, string mmcode) {
            string sql = @"
                select acc_ackqty from ME_DOCD where docno = :docno and mmcode = :mmcode
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { docno, mmcode }, DBWork.Transaction);
        }

        public int UpdateMI_WINVCTL(string docno)
        {
            int result;
            string sql = "";
            sql = @"update MI_WINVCTL
                       set FSTACKDATE=trunc(sysdate)
                     where WH_NO=(select TOWH from ME_DOCM where DOCNO = :docno )
                       and MMCODE in (select mmcode from ME_DOCD where docno = :docno)
                       and FSTACKDATE is null
                   ";  
            result = DBWork.Connection.Execute(sql, new { DOCNO = docno}, DBWork.Transaction);
            return result;
        }
        public int UpdateMI_WINVCTL(string docno, string mmcode)
        {
            int result;
            string sql = "";
            sql = @"update MI_WINVCTL
                       set FSTACKDATE=trunc(sysdate)
                     where WH_NO=(select TOWH from ME_DOCM where DOCNO = :docno )
                       and MMCODE = :mmcode
                       and FSTACKDATE is null
                   ";
            result = DBWork.Connection.Execute(sql, new { DOCNO = docno, mmcode }, DBWork.Transaction);
            return result;
        }

        #endregion
    }
}