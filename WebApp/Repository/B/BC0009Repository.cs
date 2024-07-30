using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.BC
{
    public class BC0009Repository : JCLib.Mvc.BaseRepository
    {
        public BC0009Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<PH_SMALL_M> GetMasterAll(string dn, string apptime_bg, string apptime_ed, string status, string tuser, string qtype, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @" select A.DN, A.ACCEPT, A.AGEN_NAMEC, A.AGEN_NO, A.ALT, A.SIGNDATA,
                        (select INID_NAME from UR_INID where INID = A.APP_INID) as APP_INID, 
                        (select UNA from UR_ID where TUSER = A.APP_USER) as APP_USER, 
                        (select UNA from UR_ID where TUSER = A.APP_USER1) as APP_USER1, 
                        A.APPTIME, A.APPTIME1, 
                        A.DELIVERY, A.DEMAND, 
                        (select INID_NAME from UR_INID where INID = A.DEPT) as DEPT, 
                        (select UNA from UR_ID where TUSER = A.DO_USER) as DO_USER, 
                        A.DOTEL, A.DUEDATE, 
                        A.OTHERS, A.PAYWAY, A.PR_NO, A.REASON, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D 
                        where GRP_CODE='PH_SMALL_M' and DATA_NAME='STATUS' and DATA_VALUE=A.STATUS ) as STATUS,
                        A.TEL, A.USEWHEN, A.USEWHERE
                        FROM PH_SMALL_M A WHERE 1=1 ";

            if (dn != "" && dn != null)
            {
                if (dn.Substring(0, 1) != "S")
                {
                    sql += " AND DN LIKE :p0 ";
                    p.Add(":p0", string.Format("S{0}%", dn));
                }
                else
                {
                    sql += " AND DN LIKE :p0 ";
                    p.Add(":p0", string.Format("{0}%", dn));
                }
            }
            if (apptime_bg != "" && apptime_bg != null)
            {
                sql += " AND to_char(APPTIME,'yyyy-mm-dd') >= Substr(:p1, 1, 10) ";
                p.Add(":p1", apptime_bg);
            }
            if (apptime_ed != "" && apptime_ed != null)
            {
                sql += " AND to_char(APPTIME,'yyyy-mm-dd') <= Substr(:p2, 1, 10) ";
                p.Add(":p2", apptime_ed);
            }
            if (status != "" && status != null)
            {
                sql += " AND STATUS = :p3 ";
                p.Add(":p3", status);
            }
            sql += " AND ((substr(APP_INID,1,2) = (select substr(INID,1,2) from UR_ID where TUSER = :TUSER) and STATUS='B') "; //督導審核
            sql += "  or (APP_INID = (select INID from UR_ID where TUSER = :TUSER) and STATUS <> 'B') )"; //督導新增
            sql += "  or (substr(APP_INID,1,2) = '35' and (select substr(INID,1,2) from UR_ID where TUSER = :TUSER)='33' and STATUS='B')"; //序43：責任中心35開頭也屬護理部
            p.Add(":TUSER", tuser);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<PH_SMALL_M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<PH_SMALL_M> MasterGet(string id)
        {
            var sql = @" select A.DN, A.ACCEPT, A.AGEN_NAMEC, A.AGEN_NO, A.ALT, 
                        (select INID_NAME from UR_INID where INID = A.APP_INID) as APP_INID, 
                        (select UNA from UR_ID where TUSER = A.APP_USER) as APP_USER, 
                        (select UNA from UR_ID where TUSER = A.APP_USER1) as APP_USER1, 
                        A.APPTIME, A.APPTIME1, 
                        A.DELIVERY, A.DEMAND, 
                        (select INID_NAME from UR_INID where INID = A.DEPT) as DEPT, 
                        (select UNA from UR_ID where TUSER = A.DO_USER) as DO_USER,
                        A.DOTEL, A.DUEDATE, 
                        A.OTHERS, A.PAYWAY, A.PR_NO, A.REASON, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D 
                        where GRP_CODE='PH_SMALL_M' and DATA_NAME='STATUS' and DATA_VALUE=A.STATUS ) as STATUS,
                        A.TEL, A.USEWHEN, A.USEWHERE 
                        FROM PH_SMALL_M A WHERE A.DN = :DN ";
            return DBWork.Connection.Query<PH_SMALL_M>(sql, new { DN = id }, DBWork.Transaction);
        }

        public int MasterCreate(PH_SMALL_M ph_small_m)
        {
            var sql = @"INSERT INTO PH_SMALL_M (DN, ACCEPT, AGEN_NAMEC, AGEN_NO, ALT, APP_INID, APP_USER, APPTIME, DELIVERY, DEMAND, DUEDATE,
                                OTHERS, PAYWAY, STATUS, TEL, USEWHEN, USEWHERE, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                                VALUES (:DN, :ACCEPT, :AGEN_NAMEC, :AGEN_NO, :ALT, :APP_INID, :APP_USER, SYSDATE, :DELIVERY, :DEMAND, :DUEDATE,
                                :OTHERS, :PAYWAY, 'A', :TEL, :USEWHEN, :USEWHERE, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, ph_small_m, DBWork.Transaction);
        }

        public int MasterUpdate(PH_SMALL_M ph_small_m)
        {
            var sql = @"UPDATE PH_SMALL_M SET ACCEPT=:ACCEPT, AGEN_NAMEC=:AGEN_NAMEC, AGEN_NO=:AGEN_NO, ALT=:ALT, APP_INID=:APP_INID, APP_USER=:APP_USER, DELIVERY=:DELIVERY, DEMAND=:DEMAND, DUEDATE=:DUEDATE, 
                                OTHERS=:OTHERS, PAYWAY=:PAYWAY, TEL=:TEL, USEWHEN=:USEWHEN, USEWHERE=:USEWHERE, DEPT=:DEPT, DO_USER=:DO_USER, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE DN = :DN";
            return DBWork.Connection.Execute(sql, ph_small_m, DBWork.Transaction);
        }

        public int MasterDelete(string dn)
        {
            var sql = @" DELETE from PH_SMALL_M
                                WHERE DN = :DN";
            return DBWork.Connection.Execute(sql, new { DN = dn }, DBWork.Transaction);
        }

        public int DetailDelete(string dn)
        {
            var sql = @" DELETE from PH_SMALL_D
                                WHERE DN = :DN";
            return DBWork.Connection.Execute(sql, new { DN = dn }, DBWork.Transaction);
        }
        //陳核護理部業務副主任審核 - G
        public int MasterAudit(PH_SMALL_M ph_small_m)
        {
            var sql = @"UPDATE PH_SMALL_M SET STATUS='G',  
                                SIGNDATA = TWN_TIME_FORMAT(sysdate) ||' '||(select UNA from UR_ID where TUSER = :UPDATE_USER) || ' 呈核護理部業務副主任。' || chr(13) || chr(10) || SIGNDATA ,
                                UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE DN = :DN ";
            return DBWork.Connection.Execute(sql, ph_small_m, DBWork.Transaction);
        }
        // 剔退
        public int MasterReject(PH_SMALL_M ph_small_m)
        {
            var sql = @"UPDATE PH_SMALL_M SET REASON = (select UNA from UR_ID where TUSER = :UPDATE_USER) || ' 剔退：' || :REASON || chr(13) || chr(10) ||REASON, STATUS = 'D', APP_USER1=:APP_USER1, 
                                UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP, NEXT_USER=null,
                                SIGNDATA =  TWN_TIME_FORMAT(sysdate) ||' '||(select UNA from UR_ID where TUSER = :UPDATE_USER) || ' 剔退。' || chr(13) || chr(10) || SIGNDATA
                                WHERE DN = :DN AND STATUS = 'B' ";
            return DBWork.Connection.Execute(sql, ph_small_m, DBWork.Transaction);
        }
        // ===================================================================================================
        public IEnumerable<PH_SMALL_D> GetDetailAll(string dn,string p1, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = "";

            if (p1 == "0")
            {
                sql = @"SELECT A.SEQ, A.DN, A.MEMO, A.MMCODE, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D 
                        where GRP_CODE='PH_SMALL_M' and DATA_NAME='CHARGE' and DATA_VALUE=A.CHARGE ) as CHARGE, 
                        (select INID || ' ' || INID_NAME from UR_INID where INID = A.INID) as INID, 
                        (select INID_NAME from UR_INID where INID = A.INID) as INIDNAME, 
                        A.NMSPEC, A.PRICE, A.QTY, A.UNIT, A.QTY * A.PRICE as TOTAL_PRICE,
                        (select FTP_NAME from PH_ATTFILE where DN=A.DN and SRC_NAME=A.SEQ and rownum = 1) as UK
                        FROM PH_SMALL_D A WHERE 1=1 ";

                sql += " AND A.DN = :p0 ";
            }
            else
            {
                sql = @"select a.*, sumqty,(a.price*sumqty) sumprice  from 
                        (select distinct  nvl(mmcode,' ') mmcode,NMSPEC, UNIT, PRICE
                        from PH_SMALL_D where dn=:p0) a,
                             (select nvl(mmcode,' ') mmcode,NMSPEC, sum(qty) sumqty from PH_SMALL_D 
                             where dn=:p0  group by mmcode, NMSPEC) b
                        where a.mmcode=b.mmcode and A.NMSPEC=b.NMSPEC ";

                sorters = "";
            }

            p.Add(":p0", dn);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<PH_SMALL_D>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<PH_SMALL_D> DetailGet(string dn, string seq)
        {
            var sql = @" SELECT A.SEQ, A.DN, A.MEMO, A.MMCODE, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D 
                        where GRP_CODE='PH_SMALL_M' and DATA_NAME='CHARGE' and DATA_VALUE=A.CHARGE ) as CHARGE, 
                        (select INID || ' ' || INID_NAME from UR_INID where INID = A.INID) as INID, 
                        (select INID_NAME from UR_INID where INID = A.INID) as INIDNAME, 
                        A.NMSPEC, A.PRICE, A.QTY, A.UNIT, A.QTY * A.PRICE as TOTAL_PRICE,
                        (select FTP_NAME from PH_ATTFILE where DN=A.DN and SRC_NAME=A.SEQ and rownum = 1) as UK
                        FROM PH_SMALL_D A WHERE A.DN = :DN and A.SEQ = :SEQ ";
            return DBWork.Connection.Query<PH_SMALL_D>(sql, new { DN = dn, SEQ = seq }, DBWork.Transaction);
        }

        public int DetailCreate(PH_SMALL_D ph_small_d)
        {
            var sql = @"INSERT INTO PH_SMALL_D (SEQ, DN, MEMO, MMCODE, INID, CHARGE, NMSPEC, PRICE, QTY, UNIT, CREATE_USER, UPDATE_IP, CREATE_TIME)  
                                VALUES (:SEQ, :DN, :MEMO, :MMCODE, :INID, :CHARGE, :NMSPEC, :PRICE, :QTY, :UNIT ,:CREATE_USER , :UPDATE_IP ,sysdate)";
            return DBWork.Connection.Execute(sql, ph_small_d, DBWork.Transaction);
        }

        public int DetailFileCreate(string fileseq, string dn, string seq, string uk)
        {
            var sql = @"INSERT INTO PH_ATTFILE (SEQ, DN, UPLOAD_USER, UPLOAD_TIME, SRC_NAME, FTP_NAME, SRC_IP, STATUS)  
                                VALUES (:FILESEQ, :DN, '', sysdate, :SEQ, :UK, '', 'A')";
            return DBWork.Connection.Execute(sql, new { FILESEQ = fileseq, DN = dn, SEQ = seq, UK = uk }, DBWork.Transaction);
        }

        public int DetailUpdate(PH_SMALL_D ph_small_d)
        {
            var sql = @"UPDATE PH_SMALL_D SET MEMO=:MEMO, MMCODE=:MMCODE, INID=:INID, CHARGE=:CHARGE, NMSPEC=:NMSPEC, PRICE=:PRICE, QTY=:QTY, UNIT=:UNIT ,UPDATE_USER =:UPDATE_USER , UPDATE_IP =:UPDATE_IP, UPDATE_TIME =sysdate
                                WHERE DN = :DN and SEQ = :SEQ";
            return DBWork.Connection.Execute(sql, ph_small_d, DBWork.Transaction);
        }

        public int DetailDelete(string dn, string seq)
        {
            var sql = @" DELETE from PH_SMALL_D
                                WHERE DN = :DN and SEQ = :SEQ";
            return DBWork.Connection.Execute(sql, new { DN = dn, SEQ = seq }, DBWork.Transaction);
        }

        public int DetailFileDelete(string dn, string uk)
        {
            var sql = @"delete from PH_ATTFILE  
                                where DN = :DN and FTP_NAME = :UK ";
            return DBWork.Connection.Execute(sql, new {DN = dn, UK = uk }, DBWork.Transaction);
        }

        // 檢查申請單號是否存在
        public bool CheckExists(string id)
        {
            string sql = @"SELECT 1 FROM PH_SMALL_M WHERE DN=:DN";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DN = id }, DBWork.Transaction) == null);
        }
        // 檢查申請單號+項次是否存在
        public bool CheckExists(string dn, string inid, string nmspec)
        {
            string sql = @"SELECT 1 FROM PH_SMALL_D WHERE DN=:DN and INID=:INID and NMSPEC=rtrim(:NMSPEC)";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DN = dn, INID = inid, NMSPEC = nmspec }, DBWork.Transaction) == null);
        }


        public IEnumerable<UR_INID> GetInidByTuser(string tuser)
        {
            string sql = @"select A.INID, (select INID_NAME from UR_INID where INID=A.INID) as INID_NAME from UR_ID A
                            where A.TUSER=:TUSER";
            return DBWork.Connection.Query<UR_INID>(sql, new { TUSER = tuser }, DBWork.Transaction);
        }

        public IEnumerable<PH_VENDER> GetAgennmByAgenno(string agen_no)
        {
            string sql = @"select AGEN_NO, case when AGEN_NAMEC is not null then AGEN_NAMEC else AGEN_NAMEE end as AGEN_NAME from PH_VENDER
                            where AGEN_NO=:AGEN_NO";
            return DBWork.Connection.Query<PH_VENDER>(sql, new { AGEN_NO = agen_no }, DBWork.Transaction);
        }

        public class BC0002_MM_MATMAST
        {
            public string MMNAME { get; set; }
            public string BASE_UNIT { get; set; }
            public string UPRICE { get; set; }
            public string M_PAYKIND { get; set; }
        }

        public IEnumerable<BC0002_MM_MATMAST> GetMmdataByMmcode(string mmcode)
        {
            string sql = @"select (case when MMNAME_C is not null then MMNAME_C else MMNAME_E end) as MMNAME, 
                            BASE_UNIT, UPRICE, M_PAYKIND from MI_MAST
                            where MMCODE=:MMCODE";
            return DBWork.Connection.Query<BC0002_MM_MATMAST>(sql, new { MMCODE = mmcode }, DBWork.Transaction);
        }

        public IEnumerable<UR_INID> GetInidCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = @"select {0} INID, INID_NAME
                            from UR_INID where (INID_OLD <> 'D' or INID_OLD is null) ";

            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(INID, :INID_I), 1000) + NVL(INSTR(INID_NAME, :INID_NAME_I), 100) * 10) IDX,");
                p.Add(":INID_I", p0);
                p.Add(":INID_NAME_I", p0);

                sql += " AND (INID LIKE :INID ";
                p.Add(":INID", string.Format("{0}%", p0));

                sql += " OR INID_NAME LIKE :INID_NAME) ";
                p.Add(":INID_NAME", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY INID ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<UR_INID>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMmcodeCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = @"select {0} MMCODE, MMNAME_C, MMNAME_E
                            from MI_MAST where  mat_class='02' and (CANCEL_ID <> 'Y' or CANCEL_ID is null) and M_CONTID = '3' ";

            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10 + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10) IDX,");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_C_I", p0);
                p.Add(":MMNAME_E_I", p0);

                sql += " AND (MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}%", p0));

                sql += " OR MMNAME_C LIKE :MMNAME_C ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql += " OR MMNAME_E LIKE :MMNAME_E) ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

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

        public IEnumerable<PH_VENDER> GetAgennoCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = @"select {0} AGEN_NO, AGEN_NAMEC, AGEN_NAMEE
                            from PH_VENDER where (REC_STATUS <> 'X' or REC_STATUS is null) ";

            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(AGEN_NO, :AGEN_NO_I), 1000) + NVL(INSTR(AGEN_NAMEC, :AGEN_NAMEC_I), 100) * 10 + NVL(INSTR(AGEN_NAMEE, :AGEN_NAMEE_I), 100) * 10) IDX,");
                p.Add(":AGEN_NO_I", p0);
                p.Add(":AGEN_NAMEC_I", p0);
                p.Add(":AGEN_NAMEE_I", p0);

                sql += " AND (AGEN_NO LIKE :AGEN_NO ";
                p.Add(":AGEN_NO", string.Format("{0}%", p0));

                sql += " OR AGEN_NAMEC LIKE :AGEN_NAMEC ";
                p.Add(":AGEN_NAMEC", string.Format("%{0}%", p0));

                sql += " OR AGEN_NAMEE LIKE :AGEN_NAMEE) ";
                p.Add(":AGEN_NAMEE", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY AGEN_NO ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<PH_VENDER>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public string getUserInid(string tuser)
        {
            string sql = @" select INID from UR_ID where TUSER = :TUSER ";
            return DBWork.Connection.QueryFirst<string>(sql, new { TUSER = tuser }, DBWork.Transaction);
        }

        // 依成本代碼和目前時間取得申請單號
        public string getNewDn(string inid)
        {
            string sql = @"select 'S' || :INID || TWN_SYSTIME from dual ";
            return DBWork.Connection.QueryFirst<string>(sql, new { INID = inid }, DBWork.Transaction);
        }

        // 依申請單號取得新項次
        public string getNewSeq(string dn)
        {
            string sql = @"select case when max(SEQ) is null then 1 else max(SEQ)+1 end from PH_SMALL_D
                            where DN=:DN ";
            return DBWork.Connection.QueryFirst<string>(sql, new { DN = dn }, DBWork.Transaction);
        }

        // 報表取主檔資料
        public IEnumerable<PH_SMALL_M> GetSmData(string dn)
        {
            string sql = @"select USEWHERE, DEMAND, ALT, USEWHEN, 
                            trim(TEL) as TEL, DUEDATE, DELIVERY, ACCEPT, PAYWAY, OTHERS, 
                            (select trim(INID_NAME) from UR_INID where INID = PH_SMALL_M.DEPT) as DEPT, 
                            DO_USER, trim(DOTEL) as DOTEL
                            from PH_SMALL_M where DN=:DN ";

            return DBWork.Connection.Query<PH_SMALL_M>(sql, new { DN = dn }, DBWork.Transaction);
        }
        // 報表取明細資料
        public IEnumerable<PH_SMALL_D> GetReport(string dn)
        {
            string sql = @" select rownum as seq,a.dn,a.MEMO,a.MMCODE,a.NMSPEC,a.PRICE, sumqty as QTY,a.UNIT,(a.price*sumqty) TOTAL_PRICE  from 
                            (select distinct  nvl(mmcode,' ') mmcode, dn, NMSPEC, UNIT, PRICE, MEMO from PH_SMALL_D where dn=:DN) a,  
                            (select nvl(mmcode,' ') mmcode,NMSPEC, sum(qty) sumqty from PH_SMALL_D where dn=:DN  group by mmcode, NMSPEC) b
                            where a.mmcode=b.mmcode and A.NMSPEC=b.NMSPEC 
                            UNION ALL
                            select null,null,null,null,null,null,null,null,null from UR_MENU where rownum <=10-
                            (select count(*) from PH_SMALL_D where DN = :DN)"; // 填入空白資料補到10筆

            sql += " ORDER BY SEQ ";

            return DBWork.Connection.Query<PH_SMALL_D>(sql, new { DN = dn }, DBWork.Transaction);
        }
        // 取得同DN底下所有資料合計
        public int GetReportTotalPrice(string dn)
        {
            string sql = @" select sum(round(PRICE * QTY)) as TOTAL_PRICE from PH_SMALL_D 
                            WHERE DN = :DN "; // 填入空白資料補到10筆

            return DBWork.Connection.QueryFirst<int>(sql, new { DN = dn }, DBWork.Transaction);
        }

        public string getFileSeq()
        {
            var p = new DynamicParameters();
            var sql = @" select PH_ATTFILE_SEQ.nextval as SEQ
                            from dual ";

            return DBWork.Connection.QueryFirst<string>(sql, p, DBWork.Transaction);
        }

        //匯入
        //=====================================================
        public bool CheckMmcodeExists(string MMCODE)
        {
            string sql = @"SELECT 1 FROM MI_MAST WHERE MMCODE=:MMCODE ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = MMCODE }, DBWork.Transaction) == null);
        }

        public bool CheckInidExists(string inid)
        {
            string sql = @"Select 1 from UR_INID where inid=:inid ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { inid = inid }, DBWork.Transaction) == null);
        }

        public IEnumerable<PH_SMALL_D> GetDetailAll2(string dn)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.SEQ, A.DN, A.MEMO, A.MMCODE, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D 
                        where GRP_CODE='PH_SMALL_M' and DATA_NAME='CHARGE' and DATA_VALUE=A.CHARGE ) as CHARGE, 
                        (select INID || ' ' || INID_NAME from UR_INID where INID = A.INID) as INID, 
                        (select INID_NAME from UR_INID where INID = A.INID) as INIDNAME, 
                        A.NMSPEC, A.PRICE, A.QTY, A.UNIT, A.QTY * A.PRICE as TOTAL_PRICE,
                        (select FTP_NAME from PH_ATTFILE where DN=A.DN and SRC_NAME=A.SEQ and rownum = 1) as UK
                        FROM PH_SMALL_D A WHERE 1=1 ";

            sql += " AND A.DN = :p0 ";
            p.Add(":p0", dn);

            return DBWork.Connection.Query<PH_SMALL_D>(sql, p, DBWork.Transaction);
        }
    }

}