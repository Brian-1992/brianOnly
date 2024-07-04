using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.AB
{
    public class AB0042Repository : JCLib.Mvc.BaseRepository
    {
        public AB0042Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        //查詢Master
        public IEnumerable<ME_PCAM> GetAllM(string PCACODE, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT MPM.PCACODE,
                               MPM.PCACODE PCACODE_TEXT,
                               MPM.PCACODE PCACODE_DISPLAY,
                               MMT.MMNAME_E,
                               MPM.DOSE,
                               MPM.FREQNO,
                               MMT.E_PATHNO,
                               MMT.E_ORDERUNIT
                        FROM ME_PCAM MPM, MI_MAST MMT
                        WHERE MPM.PCACODE = MMT.MMCODE";

            if (PCACODE != "")
            {
                sql += @" AND MPM.PCACODE = :p0 ";
                p.Add(":p0", string.Format("{0}", PCACODE));
            }

            sql += @" ORDER BY MPM.PCACODE";
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_PCAM>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //查詢Detail
        public IEnumerable<ME_PCAD> GetAllD(string PCACODE, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT MPD.PCACODE,
                               MPD.MMCODE,
                               MPD.MMCODE MMCODE_TEXT,
                               MPD.MMCODE MMCODE_DISPLAY,
                               MMT.MMNAME_E,
                               MPD.DOSE,
                               MPD.CONSUMEFLAG,
                               (CASE
                                    WHEN MPD.CONSUMEFLAG = 'Y' THEN '是'
                                    WHEN MPD.CONSUMEFLAG = 'N' THEN '否'
                               END) CONSUMEFLAG_DISPLAY,
                               (CASE
                                    WHEN MPD.CONSUMEFLAG = 'Y' THEN 'Y 是'
                                    WHEN MPD.CONSUMEFLAG = 'N' THEN 'N 否'
                               END) CONSUMEFLAG_TEXT,
                               MPD.COMPUTECODE,
                               (CASE
                                    WHEN MPD.COMPUTECODE = 'Y' THEN '計價'
                                    WHEN MPD.COMPUTECODE = 'N' THEN '不計價'
                                    WHEN MPD.COMPUTECODE = 'C' THEN '不申報'
                               END) COMPUTECODE_DISPLAY,
                               (CASE
                                    WHEN MPD.COMPUTECODE = 'Y' THEN 'Y 計價'
                                    WHEN MPD.COMPUTECODE = 'N' THEN 'N 不計價'
                                    WHEN MPD.COMPUTECODE = 'C' THEN 'C 不申報'
                               END) COMPUTECODE_TEXT,
                               MMT.E_ORDERUNIT
                        FROM ME_PCAM MPM, ME_PCAD MPD, MI_MAST MMT
                        WHERE MPM.PCACODE = MPD.PCACODE
                        AND MPD.MMCODE = MMT.MMCODE
                        AND MPM.PCACODE = :PCACODE";

            p.Add(":PCACODE", string.Format("{0}", PCACODE));
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_PCAD>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //新增Master
        public int CreateM(ME_PCAM ME_PCAM)
        {
            var sql = @"INSERT INTO ME_PCAM (
                              PCACODE, DOSE, FREQNO, CREATE_TIME, CREATE_ID, UPDATE_TIME, UPDATE_ID, UPDATE_IP)  
                        VALUES (
                              :PCACODE, :DOSE, :FREQNO, SYSDATE,:CREATE_ID, SYSDATE, :UPDATE_ID, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, ME_PCAM, DBWork.Transaction);
        }

        //修改Master
        public int UpdateM(ME_PCAM ME_PCAM)
        {
            var sql = @"UPDATE ME_PCAM SET 
                               DOSE = :DOSE,
                               FREQNO = :FREQNO,
                               UPDATE_TIME = SYSDATE, UPDATE_ID = :UPDATE_ID, UPDATE_IP = :UPDATE_IP
                        WHERE PCACODE = :PCACODE ";
            return DBWork.Connection.Execute(sql, ME_PCAM, DBWork.Transaction);
        }

        //刪除Master
        public int DeleteM_M(ME_PCAM ME_PCAM)
        {
            var sql = @"DELETE ME_PCAM MPM
                        WHERE MPM.PCACODE = :PCACODE ";
            return DBWork.Connection.Execute(sql, ME_PCAM, DBWork.Transaction);
        }
        //刪除Master之下的Detail
        public int DeleteM_D(ME_PCAM ME_PCAM)
        {
            var sql = @"DELETE ME_PCAD MPD
                        WHERE MPD.PCACODE = :PCACODE ";
            return DBWork.Connection.Execute(sql, ME_PCAM, DBWork.Transaction);
        }

        //新增Detail
        public int CreateD(ME_PCAD ME_PCAD)
        {
            var sql = @"INSERT INTO ME_PCAD ( PCACODE, MMCODE, DOSE, COMPUTECODE, CONSUMEFLAG, 
                                             CREATE_TIME, CREATE_ID, UPDATE_TIME, UPDATE_ID, UPDATE_IP)
                        VALUES ( :PCACODE, :MMCODE, :DOSE, :COMPUTECODE, :CONSUMEFLAG,
                                 SYSDATE,
                                 :CREATE_ID,
                                 SYSDATE,
                                 :UPDATE_ID,
                                 :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, ME_PCAD, DBWork.Transaction);
        }

        //修改Detail
        public int UpdateD(ME_PCAD ME_PCAD)
        {
            var sql = @"UPDATE ME_PCAD SET 
                                  DOSE = :DOSE,
                                  COMPUTECODE = :COMPUTECODE,
                                  CONSUMEFLAG = :CONSUMEFLAG,
                                  UPDATE_TIME = SYSDATE, UPDATE_ID = :UPDATE_ID, UPDATE_IP = :UPDATE_IP
                        WHERE PCACODE = :PCACODE
                        AND MMCODE = :MMCODE";
            return DBWork.Connection.Execute(sql, ME_PCAD, DBWork.Transaction);
        }

        //刪除Detail
        public int DeleteD(ME_PCAD ME_PCAD)
        {
            var sql = @"DELETE ME_PCAD
                        WHERE PCACODE = :PCACODE
                        AND MMCODE = :MMCODE ";
            return DBWork.Connection.Execute(sql, ME_PCAD, DBWork.Transaction);
        }

        //檢查Master是否存在
        public bool CheckExistsM(string pcacode)
        {
            string sql = @"SELECT 1
                           FROM ME_PCAM PPM
                           WHERE PPM.PCACODE = :PCACODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { PCACODE = pcacode }, DBWork.Transaction) == null);
        }

        //檢查Detail是否存在
        public bool CheckExistsD(string pcacode, string mmcode)
        {
            string sql = @"SELECT 1
                           FROM ME_PCAD PPD
                           WHERE PPD.PCACODE = :PCACODE
                           AND PPD.MMCODE = :MMCODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { PCACODE = pcacode, MMCODE = mmcode }, DBWork.Transaction) == null);
        }

        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.E_ORDERUNIT, A.E_PATHNO FROM MI_MAST A WHERE 1=1 ";


            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(A.MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(A.MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("%{0}%", p0));

                sql += " OR A.MMNAME_E LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR A.MMNAME_C LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX, MMCODE ", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY A.MMCODE ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public class MI_MAST_QUERY_PARAMS
        {
            public string MMCODE;
            public string MMNAME_C;
            public string MMNAME_E;
            public string E_ORDERUNIT;
            public string E_PATHNO;
        }

        public IEnumerable<MI_MAST> GetMmcode(MI_MAST_QUERY_PARAMS query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            string sql = @"SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.E_ORDERUNIT, A.E_PATHNO FROM MI_MAST A WHERE 1=1  ";


            if (query.MMCODE != "")
            {
                sql += " AND A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("%{0}%", query.MMCODE));
            }

            if (query.MMNAME_C != "")
            {
                sql += " AND A.MMNAME_C LIKE :MMNAME_C ";
                p.Add(":MMNAME_C", string.Format("%{0}%", query.MMNAME_C));
            }

            if (query.MMNAME_E != "")
            {
                sql += " AND A.MMNAME_E LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", query.MMNAME_E));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //取得使用途徑及醫囑單位
        public IEnumerable<MI_MAST> GetE_ORDERUNIT(string MMCODE)
        {
            var p = new DynamicParameters();

            string sql = @"SELECT MMT.E_ORDERUNIT, MMT.E_PATHNO
                           FROM MI_MAST MMT
                           WHERE MMT.MMCODE = :MMCODE";


            p.Add(":MMCODE", string.Format("{0}", MMCODE));

            return DBWork.Connection.Query<MI_MAST>(sql, p, DBWork.Transaction);
        }

    }
}
