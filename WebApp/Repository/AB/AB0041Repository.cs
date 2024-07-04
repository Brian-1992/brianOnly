using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.BC
{
    public class AB0041Repository : JCLib.Mvc.BaseRepository
    {
        public AB0041Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ME_MDFM> GetMasterAll(string mdfm, bool p1, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            //var sql = @"SELECT 
            //                A.MDFM, A.MD_NAME, A.MMCODE, B.MMNAME_E, A.MDFM_QTY, A.MDFM_UNIT , A.USE_QTY, A.PRESERVE_DAYS, A.OPERATION, A.ELEMENTS
            //            FROM 
            //                ME_MDFM A, MI_MAST B
            //            WHERE  1=1 AND
            //                A.MMCODE = B.MMCODE ";
            var sql = @"SELECT 
                            A.MDFM, A.MD_NAME, A.MMCODE, (SELECT MMNAME_E FROM MI_MAST WHERE A.MMCODE = MMCODE) MMNAME_E, A.MDFM_QTY, A.MDFM_UNIT , A.USE_QTY, A.PRESERVE_DAYS, A.OPERATION, A.ELEMENTS
                        FROM 
                            ME_MDFM A
                        WHERE  1=1 ";

            if (mdfm != "" && mdfm != null)
                if (p1)
                {
                    sql += " AND A.MDFM LIKE :p0 ";
                    p.Add(":p0", string.Format("{0}", mdfm));
                }
                else
                {
                    sql += " AND A.MDFM LIKE :p0 ";
                    p.Add(":p0", string.Format("{0}%", mdfm));
                }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_MDFM>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<ME_MDFM> MasterGet(string me_mdfm)
        {
            var sql = @"SELECT A.*, (SELECT MMNAME_E FROM MI_MAST WHERE A.MMCODE = MMCODE) MMNAME_E FROM ME_MDFM A WHERE A.MDFM = :MDFM";
            return DBWork.Connection.Query<ME_MDFM>(sql, new { MDFM = me_mdfm }, DBWork.Transaction);
        }

        public int MasterCreate(ME_MDFM me_mdfm)
        {
            var sql = @"INSERT INTO ME_MDFM 
                                        (MDFM, MD_NAME, MMCODE, MDFM_QTY, MDFM_UNIT, USE_QTY, PRESERVE_DAYS, OPERATION, ELEMENTS, CREATE_TIME, CREATE_ID, UPDATE_TIME, UPDATE_ID, UPDATE_IP)  
                                VALUES (:MDFM, :MD_NAME, :MMCODE, :MDFM_QTY, :MDFM_UNIT, :USE_QTY, :PRESERVE_DAYS, :OPERATION, :ELEMENTS, SYSDATE, :CREATE_ID, SYSDATE, :UPDATE_ID, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, me_mdfm, DBWork.Transaction);
        }

        public int MasterUpdate(ME_MDFM me_mdfm)
        {
            var sql = @"UPDATE ME_MDFM SET MDFM = :MDFM, MD_NAME = :MD_NAME, MMCODE = :MMCODE, MDFM_QTY = :MDFM_QTY, MDFM_UNIT = :MDFM_UNIT, USE_QTY = :USE_QTY, PRESERVE_DAYS = :PRESERVE_DAYS, OPERATION = :OPERATION, ELEMENTS = :ELEMENTS, UPDATE_TIME = SYSDATE, UPDATE_ID = :UPDATE_ID, UPDATE_IP = :UPDATE_IP
                                WHERE MDFM = :MDFM";
            return DBWork.Connection.Execute(sql, me_mdfm, DBWork.Transaction);
        }

        public int MasterDelete(string mdfm)
        {
            var sql = @"DELETE from ME_MDFM WHERE MDFM = :MDFM";
            return DBWork.Connection.Execute(sql, new { MDFM = mdfm }, DBWork.Transaction);
        }

        /*
        public int MasterAudit(PH_SMALL_M ph_small_m)
        {
            var sql = @"UPDATE PH_SMALL_M SET STATUS='B', REASON = '', UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE DN = :DN AND (STATUS = 'A' OR STATUS = 'D') ";
            return DBWork.Connection.Execute(sql, ph_small_m, DBWork.Transaction);
        } */

        // ===================================================================================================
        public IEnumerable<ME_MDFD> GetDetailAll(string mdfm, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            //var sql = @"SELECT 
            //                B.MMCODE, C.MMNAME_E , B.MDFD_QTY, B.MDFD_UNIT, B.USE_QTY
            //            FROM 
            //                ME_MDFM A, ME_MDFD B, MI_MAST C
            //            WHERE 1=1 AND
            //                A.MDFM = B.MDFM AND B.MMCODE = C.MMCODE ";
            var sql = @"SELECT 
                            B.MMCODE, (SELECT MMNAME_E FROM MI_MAST D WHERE B.MMCODE = D.MMCODE) MMNAME_E, B.MDFD_QTY, B.MDFD_UNIT, B.USE_QTY, A.MDFM
                        FROM 
                            ME_MDFM A, ME_MDFD B, MI_MAST C
                        WHERE 1=1 AND
                            A.MDFM = B.MDFM AND B.MMCODE = C.MMCODE";

            sql += " AND A.MDFM = :p0 ";
            p.Add(":p0", mdfm);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_MDFD>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<ME_MDFD> DetailGet(ME_MDFD me_mdfd)
        {
            var sql = @"SELECT A.*, (SELECT MMNAME_E FROM MI_MAST WHERE A.MMCODE = MMCODE) MMNAME_E FROM ME_MDFD A WHERE MDFM = :MDFM AND MMCODE = :MMCODE";
            return DBWork.Connection.Query<ME_MDFD>(sql, me_mdfd, DBWork.Transaction);
        }

        public int DetailCreate(ME_MDFD me_mdfd)
        {
            var sql = @"INSERT INTO ME_MDFD 
                                        (MDFM, MMCODE, MDFD_QTY, MDFD_UNIT, USE_QTY, CREATE_TIME, CREATE_ID, UPDATE_TIME, UPDATE_ID, UPDATE_IP)  
                                VALUES (:MDFM, :MMCODE, :MDFD_QTY, :MDFD_UNIT, :USE_QTY, SYSDATE, :CREATE_ID, SYSDATE, :UPDATE_ID, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, me_mdfd, DBWork.Transaction);
        }

        public int DetailUpdate(ME_MDFD me_mdfd)
        {
            var sql = @"UPDATE ME_MDFD SET MMCODE = :MMCODE, MDFD_QTY = :MDFD_QTY, MDFD_UNIT = :MDFD_UNIT, USE_QTY = :USE_QTY, UPDATE_TIME = SYSDATE, UPDATE_ID = :UPDATE_ID, UPDATE_IP = :UPDATE_IP
                                WHERE MDFM = :MDFM AND MMCODE = :MMCODE_DETAIL";
            return DBWork.Connection.Execute(sql, me_mdfd, DBWork.Transaction);
        }

        public int DetailDelete(string mdfd, string mmcode)
        {
            var sql = @" DELETE from ME_MDFD WHERE MDFM = :MDFM AND MMCODE = :MMCODE";
            return DBWork.Connection.Execute(sql, new { MDFM = mdfd, MMCODE = mmcode }, DBWork.Transaction);
        }

        public int DetailDelete(string mdfm)
        {
            var sql = @" DELETE from ME_MDFD WHERE MDFM = :MDFM";
            return DBWork.Connection.Execute(sql, new { MDFM = mdfm }, DBWork.Transaction);
        }

        // 檢查配方代碼是否存在
        public bool CheckExists(string me_mdfm)
        {
            string sql = @"SELECT 1 FROM ME_MDFM WHERE MDFM=:MDFM";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MDFM = me_mdfm }, DBWork.Transaction) == null);
        }

        
        // 檢查申請單號+項次是否存在
        //public bool CheckExists(string dn, string seq)
        //{
        //    string sql = @"SELECT 1 FROM PH_SMALL_D WHERE DN=:DN and SEQ=:SEQ";
        //    return !(DBWork.Connection.ExecuteScalar(sql, new { DN = dn, SEQ = seq }, DBWork.Transaction) == null);
        //} 

        //public IEnumerable<AB0041_MM_MATMAST> GetMmdataByMmcode(string mmcode)
        //{
        //    string sql = @"select (case when MMNAME_C is not null then MMNAME_C else MMNAME_E end) as MMNAME, 
        //                    BASE_UNIT, M_CONTPRICE from MI_MAST
        //                    where MMCODE=:MMCODE";
        //    return DBWork.Connection.Query<AB0041_MM_MATMAST>(sql, new { MMCODE = mmcode }, DBWork.Transaction);
        //}

        public bool CheckMmExists(string me_mdfd)
        {
            var sql = @"SELECT 1 FROM ME_MDFD WHERE MMCODE=:MMCODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = me_mdfd }, DBWork.Transaction) == null);
        }
        
        //public IEnumerable<UR_INID> GetInidByTuser(string tuser)
        //{
        //    string sql = @"select A.INID, (select INID_NAME from UR_INID where INID=A.INID) as INID_NAME from UR_ID A
        //                    where A.TUSER=:TUSER";
        //    return DBWork.Connection.Query<UR_INID>(sql, new { TUSER = tuser }, DBWork.Transaction);
        //}

        //public IEnumerable<PH_VENDER> GetAgennmByAgenno(string agen_no)
        //{
        //    string sql = @"select AGEN_NO, case when AGEN_NAMEC is not null then AGEN_NAMEC else AGEN_NAMEE end as AGEN_NAME from PH_VENDER
        //                    where AGEN_NO=:AGEN_NO";
        //    return DBWork.Connection.Query<PH_VENDER>(sql, new { AGEN_NO = agen_no }, DBWork.Transaction);
        //}

        public class AB0041_MM_MATMAST
        {
            public string MMNAME { get; set; }
            public string BASE_UNIT { get; set; }
            public string M_CONTPRICE { get; set; }
        }

        //public IEnumerable<UR_INID> GetInidCombo(string p0, int page_index, int page_size, string sorters)
        //{
        //    var p = new DynamicParameters();

        //    string sql = @"select {0} INID, INID_NAME
        //                    from UR_INID where (INID_OLD <> 'D' or INID_OLD is null) ";

        //    if (p0 != "")
        //    {
        //        sql = string.Format(sql, "(NVL(INSTR(INID, :INID_I), 1000) + NVL(INSTR(INID_NAME, :INID_NAME_I), 100) * 10) IDX,");
        //        p.Add(":INID_I", p0);
        //        p.Add(":INID_NAME_I", p0);

        //        sql += " AND (INID LIKE :INID ";
        //        p.Add(":INID", string.Format("{0}%", p0));

        //        sql += " OR INID_NAME LIKE :INID_NAME) ";
        //        p.Add(":INID_NAME", string.Format("%{0}%", p0));

        //        sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
        //    }
        //    else
        //    {
        //        sql = string.Format(sql, "");
        //        sql += " ORDER BY INID ";
        //    }

        //    p.Add("OFFSET", (page_index - 1) * page_size);
        //    p.Add("PAGE_SIZE", page_size);

        //    return DBWork.Connection.Query<UR_INID>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        //}

        //public IEnumerable<MI_MAST> GetMmcodeCombo(string p0, int page_index, int page_size, string sorters)
        //{
        //    var p = new DynamicParameters();

        //    string sql = @"select {0} MMCODE, MMNAME_C, MMNAME_E
        //                    from MI_MAST where (CANCEL_ID <> 'Y' or CANCEL_ID is null) ";

        //    if (p0 != "")
        //    {
        //        sql = string.Format(sql, "(NVL(INSTR(MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10 + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10) IDX,");
        //        p.Add(":MMCODE_I", p0);
        //        p.Add(":MMNAME_C_I", p0);
        //        p.Add(":MMNAME_E_I", p0);

        //        sql += " AND (MMCODE LIKE :MMCODE ";
        //        p.Add(":MMCODE", string.Format("{0}%", p0));

        //        sql += " OR MMNAME_C LIKE :MMNAME_C ";
        //        p.Add(":MMNAME_C", string.Format("%{0}%", p0));

        //        sql += " OR MMNAME_E LIKE :MMNAME_E) ";
        //        p.Add(":MMNAME_E", string.Format("%{0}%", p0));

        //        sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
        //    }
        //    else
        //    {
        //        sql = string.Format(sql, "");
        //        sql += " ORDER BY MMCODE ";
        //    }

        //    p.Add("OFFSET", (page_index - 1) * page_size);
        //    p.Add("PAGE_SIZE", page_size);

        //    return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        //}

        //public IEnumerable<PH_VENDER> GetAgennoCombo(string p0, int page_index, int page_size, string sorters)
        //{
        //    var p = new DynamicParameters();

        //    string sql = @"select {0} AGEN_NO, AGEN_NAMEC, AGEN_NAMEE
        //                    from PH_VENDER where (REC_STATUS <> 'X' or REC_STATUS is null) ";

        //    if (p0 != "")
        //    {
        //        sql = string.Format(sql, "(NVL(INSTR(AGEN_NO, :AGEN_NO_I), 1000) + NVL(INSTR(AGEN_NAMEC, :AGEN_NAMEC_I), 100) * 10 + NVL(INSTR(AGEN_NAMEE, :AGEN_NAMEE_I), 100) * 10) IDX,");
        //        p.Add(":AGEN_NO_I", p0);
        //        p.Add(":AGEN_NAMEC_I", p0);
        //        p.Add(":AGEN_NAMEE_I", p0);

        //        sql += " AND (AGEN_NO LIKE :AGEN_NO ";
        //        p.Add(":AGEN_NO", string.Format("{0}%", p0));

        //        sql += " OR AGEN_NAMEC LIKE :AGEN_NAMEC ";
        //        p.Add(":AGEN_NAMEC", string.Format("%{0}%", p0));

        //        sql += " OR AGEN_NAMEE LIKE :AGEN_NAMEE) ";
        //        p.Add(":AGEN_NAMEE", string.Format("%{0}%", p0));

        //        sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
        //    }
        //    else
        //    {
        //        sql = string.Format(sql, "");
        //        sql += " ORDER BY AGEN_NO ";
        //    }

        //    p.Add("OFFSET", (page_index - 1) * page_size);
        //    p.Add("PAGE_SIZE", page_size);

        //    return DBWork.Connection.Query<PH_VENDER>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        //}

        //public string getUserInid(string tuser)
        //{
        //    string sql = @" select INID from UR_ID where TUSER = :TUSER ";
        //    return DBWork.Connection.QueryFirst<string>(sql, new { TUSER = tuser }, DBWork.Transaction);
        //}

        // 依成本代碼和目前時間取得申請單號
        //public string getNewDn(string inid)
        //{
        //    string sql = @"select 'S' || :INID || TWN_SYSTIME from dual ";
        //    return DBWork.Connection.QueryFirst<string>(sql, new { INID = inid }, DBWork.Transaction);
        //}

        // 依申請單號取得新項次
        //public string getNewSeq(string dn)
        //{
        //    string sql = @"select case when max(SEQ) is null then 1 else max(SEQ)+1 end from PH_SMALL_D
        //                    where DN=:DN ";
        //    return DBWork.Connection.QueryFirst<string>(sql, new { DN = dn }, DBWork.Transaction);
        //}

        // 報表取主檔資料
        //public IEnumerable<PH_SMALL_M> GetSmData(string dn)
        //{
        //    string sql = @"select USEWHERE, DEMAND, ALT, USEWHEN, INID, 
        //                    (select trim(INID_NAME) from UR_INID where INID = PH_SMALL_M.INID) as INIDNAME, 
        //                    trim(TEL) as TEL, DUEDATE, DELIVERY, ACCEPT, PAYWAY, OTHERS, 
        //                    (select trim(INID_NAME) from UR_INID where INID = PH_SMALL_M.DEPT) as DEPT, 
        //                    DO_USER, trim(DOTEL) as DOTEL
        //                    from PH_SMALL_M where DN=:DN ";

        //    return DBWork.Connection.Query<PH_SMALL_M>(sql, new { DN = dn }, DBWork.Transaction);
        //}
        // 報表取明細資料
        //public IEnumerable<PH_SMALL_D> GetReport(string dn)
        //{
        //    string sql = @"select SEQ, DN, MEMO, MMCODE, NMSPEC, PRICE, QTY, UNIT, PRICE * QTY as TOTAL_PRICE from PH_SMALL_D 
        //                    WHERE DN = :DN 
        //                    UNION ALL
        //                    select null,null,null,null,null,null,null,null,null from UR_MENU where rownum <=10-
        //                    (select count(*) from PH_SMALL_D where DN = :DN)"; // 填入空白資料補到10筆

        //    sql += " ORDER BY SEQ ";

        //    return DBWork.Connection.Query<PH_SMALL_D>(sql, new { DN = dn }, DBWork.Transaction);
        //}
        // 取得同DN底下所有資料合計
        //public int GetReportTotalPrice(string dn)
        //{
        //    string sql = @" select sum(PRICE * QTY) as TOTAL_PRICE from PH_SMALL_D 
        //                    WHERE DN = :DN "; // 填入空白資料補到10筆

        //    return DBWork.Connection.QueryFirst<int>(sql, new { DN = dn }, DBWork.Transaction);
        //}
        
    }
}