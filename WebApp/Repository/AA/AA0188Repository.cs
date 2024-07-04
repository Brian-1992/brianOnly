using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.AA
{
    public class AA0188Repository : JCLib.Mvc.BaseRepository
    {
        public AA0188Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<CN_RECORD> GetAll(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @" select 
                            A.MMCODE, B.MMNAME_C, B.MMNAME_E, A.WH_NO,
                            (select WH_NAME from MI_WHMAST where WH_NO = A.WH_NO) as WH_NAME,
                            A.QTY, A.DISC_CPRICE
                            from CN_RECORD A, MI_MAST B
                            where A.MMCODE = B.MMCODE
                        ";

            if (p0 != "")
            {
                sql += " and A.MMCODE like :p0 ";
                p.Add(":p0", string.Format("%{0}%", p0));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CN_RECORD>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<CN_RECORD> Get(string wh_no, string mmcode)
        {
            var sql = @"select 
                            A.MMCODE, B.MMNAME_C, B.MMNAME_E, A.WH_NO,
                            (select WH_NAME from MI_WHMAST where WH_NO = A.WH_NO) as WH_NAME,
                            A.QTY, A.DISC_CPRICE
                            from CN_RECORD A, MI_MAST B
                            where A.MMCODE = B.MMCODE
                            and A.WH_NO = :WH_NO and A.MMCODE = :MMCODE ";

            return DBWork.Connection.Query<CN_RECORD>(sql, new { WH_NO = wh_no, MMCODE = mmcode }, DBWork.Transaction);
        }

        public DataTable GetExcel(string mmcode)
        {
            var p = new DynamicParameters();

            var sql = @" select 
                            A.MMCODE as 藥材代碼, B.MMNAME_C as 中文品名, B.MMNAME_E as 英文品名,
                            (select WH_NAME from MI_WHMAST where WH_NO = A.WH_NO) as 寄放單位,
                            A.QTY as 數量, to_char(A.DISC_CPRICE) as 單價
                            from CN_RECORD A, MI_MAST B
                            where A.MMCODE = B.MMCODE
                        ";

            if (mmcode != "")
            {
                sql += " and A.MMCODE like :p0 ";
                p.Add(":p0", string.Format("%{0}%", mmcode));
            }

            sql += @" union
                        select 
                            '' as 藥材代碼, '' as 中文品名, '寄售總量：' as 英文品名,
                            '' as 寄放單位,
                            (select sum(QTY) from CN_RECORD where 1=1 ";
            if (mmcode != "")
            {
                sql += "        and MMCODE like :p0 ";
            }

            sql += @"       ) as 數量, '' as 單價
                            from dual ";

            sql += " order by 寄放單位 asc, 藥材代碼 asc ";

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

        public int Create(CN_RECORD cn_record)
        {
            var sql = @"insert into CN_RECORD (WH_NO, MMCODE, QTY, DISC_CPRICE, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                                values (:WH_NO, :MMCODE, :QTY, 
                                (select DISC_CPRICE from MI_MAST where MMCODE = :MMCODE),
                                SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, cn_record, DBWork.Transaction);
        }

        public int Update(CN_RECORD cn_record)
        {
            var sql = @"update CN_RECORD set QTY = :QTY, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                where WH_NO = :WH_NO and MMCODE = :MMCODE ";
            return DBWork.Connection.Execute(sql, cn_record, DBWork.Transaction);
        }

        public int Delete(string wh_no, string mmcode)
        {
            var sql = @"delete from CN_RECORD WHERE WH_NO = :WH_NO and MMCODE = :MMCODE ";
            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, MMCODE = mmcode }, DBWork.Transaction);
        }

        public bool CheckExists(string wh_no, string mmcode)
        {
            string sql = @" SELECT 1 FROM CN_RECORD WHERE
                          WH_NO = :WH_NO AND MMCODE = :MMCODE";

            return !(DBWork.Connection.ExecuteScalar(sql, new { WH_NO = wh_no, MMCODE = mmcode }, DBWork.Transaction) == null);
        }

        public DataTable calcAmtMsg(string mmcode)
        {
            var p = new DynamicParameters();

            var sql = @"  
                select
                nvl(sum(QTY), 0) as SUM_QTY
                from CN_RECORD A
                where 1=1 ";

            if (mmcode != "")
            {
                sql += " and A.MMCODE like :p0 ";
                p.Add(":p0", string.Format("%{0}%", mmcode));
            }

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
    }
}