using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.AA
{
    public class AA0189Repository : JCLib.Mvc.BaseRepository
    {
        public AA0189Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<CN_RECORD> GetAll(CN_RECORD query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT
                    A.WH_NO, C.WH_NAME || '　　　　　回報小計：' || (select sum(RQTY) from CN_RECORD where WH_NO = A.WH_NO) as WH_NAME, A.MMCODE,  
                    B.MMNAME_C, B.MMNAME_E, A.QTY, A.RQTY,  
                    A.DISC_CPRICE  
                    FROM CN_RECORD A,MI_MAST B, MI_WHMAST C
                    WHERE A.MMCODE = B.MMCODE
                    AND A.WH_NO = C.WH_NO
                     ";

            if (query.WH_NO != "")
            {
                sql += " and A.WH_NO = :WH_NO ";
                p.Add(":WH_NO", string.Format("{0}", query.WH_NO));
            }
            if (query.MMCODE != "")
            {
                sql += " and A.MMCODE like :MMCODE ";
                p.Add(":MMCODE", string.Format("%{0}%", query.MMCODE));
            }
            
            if (query.QTY_CHK == "Y")
                sql += " and A.QTY <> A.RQTY ";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CN_RECORD>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public DataTable GetExcel(string wh_no, string mmcode, string qty_chk)
        {
            var p = new DynamicParameters();

            var sql = @" SELECT
                    C.WH_NAME as 庫房, A.MMCODE as 藥材代碼,  
                    B.MMNAME_C as 中文品名, B.MMNAME_E as 英文品名, A.QTY as 寄放數量, A.RQTY as 回報數量,  
                    A.DISC_CPRICE as 單價 
                    FROM CN_RECORD A,MI_MAST B, MI_WHMAST C
                    WHERE A.MMCODE = B.MMCODE
                    AND A.WH_NO = C.WH_NO ";

            if (wh_no != "")
            {
                sql += " and A.WH_NO = :WH_NO ";
                p.Add(":WH_NO", string.Format("{0}", wh_no));
            }
            if (mmcode != "")
            {
                sql += " and A.MMCODE like :MMCODE ";
                p.Add(":MMCODE", string.Format("%{0}%", mmcode));
            }

            if (qty_chk == "Y")
                sql += " and A.QTY <> A.RQTY ";

            sql += " order by A.WH_NO, A.MMCODE ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
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
    }
}