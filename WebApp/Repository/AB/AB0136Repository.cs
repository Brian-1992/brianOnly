using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.AB
{
    public class AB0136Repository : JCLib.Mvc.BaseRepository
    {
        public AB0136Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<CN_RECORD> GetAll(string p0, string p1, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @" select 
                            A.MMCODE, B.MMNAME_C, B.MMNAME_E, A.WH_NO,
                            (select WH_NAME from MI_WHMAST where WH_NO = A.WH_NO) as WH_NAME,
                            A.RQTY, A.DISC_CPRICE
                            from CN_RECORD A, MI_MAST B
                            where A.MMCODE = B.MMCODE
                        ";

            if (p0 != "")
            {
                sql += " and A.WH_NO like :p0 ";
                p.Add(":p0", string.Format("%{0}%", p0));
            }

            if (p1 != "")
            {
                sql += " and A.MMCODE like :p1 ";
                p.Add(":p1", string.Format("%{0}%", p1));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CN_RECORD>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMmCodeCombo(string p0, int page_index, int page_size, string sorters)
        {

            var p = new DynamicParameters();

            var sql = @"
                        select {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.DISC_CPRICE
                          from MI_MAST A 
                        where nvl(A.CANCEL_ID, 'N') <> 'Y' {1} ";
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

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetWhnoCombo()
        {
            string sql = @" select WH_NO as VALUE, WH_NAME as TEXT,
                            WH_NO || ' ' || WH_NAME as COMBITEM from MI_WHMAST where nvl(CANCEL_ID, 'N') <> 'Y' 
                            order by WH_NO ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public int UpdateRqty(CN_RECORD cn_record)
        {
            var sql = @" update CN_RECORD 
                set RQTY=:RQTY, UPDATE_TIME=sysdate, UPDATE_USER=:UPDATE_USER, UPDATE_IP=:UPDATE_IP
                WHERE WH_NO = :WH_NO and MMCODE = :MMCODE ";
            return DBWork.Connection.Execute(sql, cn_record, DBWork.Transaction);
        }
    }
}