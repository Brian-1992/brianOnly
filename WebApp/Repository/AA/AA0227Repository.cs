using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using WebApp.Models.D;
using System.Collections.Generic;

namespace WebApp.Repository.AA
{
    public class AA0227Repository : JCLib.Mvc.BaseRepository
    {
        public AA0227Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<DGMISS_MAST> GetAll(string inid, string mmcode, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select a.INID, (select INID||''||INID_NAME from UR_INID where INID=a.INID) as INID_T,
                             a.MMCODE, b.MMNAME_C, b.MMNAME_E 
                        from DGMISS_MAST a, MI_MAST b
                        where a.MMCODE=b.MMCODE 
                         ";

            if (inid != "" && inid != null)
            {
                sql += " and a.inid = :p0 ";
                p.Add(":p0", inid);
            }
            if (mmcode != "" && mmcode != null)
            {
                sql += " and a.mmcode = :p1 ";
                p.Add(":p1", mmcode);
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<DGMISS_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<DGMISS_MAST> Get(string inid, string mmcode)
        {
            var sql = @"select a.INID, (select INID||''||INID_NAME from UR_INID where INID=a.INID) as INID_T,
                             a.MMCODE, b.MMNAME_C, b.MMNAME_E 
                        from DGMISS_MAST a, MI_MAST b
                        where a.MMCODE=b.MMCODE 
                          AND a.INID = :INID AND a.MMCODE = :MMCODE ";
            return DBWork.Connection.Query<DGMISS_MAST>(sql, new { INID = inid, MMCODE = mmcode }, DBWork.Transaction);
        }

        public int Create(DGMISS_MAST dgmmiss_mast)
        {
            var sql = @"INSERT INTO DGMISS_MAST (INID, MMCODE, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                                VALUES (:INID, :MMCODE, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, dgmmiss_mast, DBWork.Transaction);
        }
        
        public int Delete(string inid, string mmcode)
        {
            // 刪除資料
            var sql = @"DELETE DGMISS_MAST WHERE INID = :INID AND MMCODE = :MMCODE ";
            return DBWork.Connection.Execute(sql, new { INID = inid, MMCODE = mmcode }, DBWork.Transaction);
        }

        public bool CheckExists(string inid, string mmcode)
        {
            string sql = @"SELECT 1 FROM DGMISS_MAST WHERE INID = :INID AND MMCODE = :MMCODE ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { INID = inid, MMCODE = mmcode }, DBWork.Transaction) == null);
        }
        

        public int CheckDgmissNum(string inid, string mmcode)
        {
            string sql = @"SELECT COUNT(*) AS CNT FROM DGMISS  
                            WHERE IS_DEL='N' 
                            AND APP_INID = :INID AND MMCODE = :MMCODE ";
            int rtn = Convert.ToInt32(DBWork.Connection.ExecuteScalar(sql, new { INID = inid, MMCODE = mmcode }, DBWork.Transaction).ToString());
            return rtn;
        }

        public IEnumerable<COMBO_MODEL> GetInidCombo()
        {
            string sql = @"SELECT DISTINCT INID as VALUE, INID_NAME as TEXT,
                        INID || ' ' || INID_NAME as COMBITEM
                        FROM UR_INID
                        WHERE INID_OLD <> 'D' or INID_OLD is null
                        ORDER BY INID";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"
                                    SELECT {0} 
                                           a.mmcode,
                                           a.mmname_c,
                                           a.mmname_e
                                     FROM  MI_MAST a
                                    WHERE  a.MAT_CLASS = '01' 
                                ";

            if (!string.IsNullOrWhiteSpace(p0))
            {
                sql = string.Format(sql, "(NVL(INSTR(UPPER(A.MMCODE), UPPER(:MMCODE_I)), 1000) + NVL(INSTR(UPPER(A.MMNAME_E), UPPER(:MMNAME_E_I)), 100) * 10 + NVL(INSTR(UPPER(A.MMNAME_C), UPPER(:MMNAME_C_I)), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql = string.Format("SELECT * FROM ({0}) TMP WHERE 1=1 ", sql);

                sql += " AND (UPPER(MMCODE) LIKE UPPER(:MMCODE) ";
                p.Add(":MMCODE", string.Format("%{0}%", p0));

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

    }
}