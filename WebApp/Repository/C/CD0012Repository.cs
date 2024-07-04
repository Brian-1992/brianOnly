using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using JCLib.Mvc;

namespace WebApp.Repository.C
{
    public class CD0012Repository : JCLib.Mvc.BaseRepository
    {
        public CD0012Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<MM_ITEMS_COMP> GetQuery(string inid)
        {
            var p = new DynamicParameters();
            var sql = @" select A.INID, (select INID_NAME from UR_INID where INID=A.INID) as INID_NAME, A.COMPLEXITY
                            from MM_ITEMS_COMP A
                            where 1=1 ";

            if (inid != "" && inid != null)
            {
                sql += " and A.INID = :p0 ";
                p.Add(":p0", inid);
            }

            return DBWork.PagingQuery<MM_ITEMS_COMP>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<MM_ITEMS_COMP> Get(string id)
        {
            var sql = @"SELECT A.INID, (select INID_NAME from UR_INID where INID=A.INID) as INID_NAME, A.COMPLEXITY
                            from MM_ITEMS_COMP A where A.INID=:INID";
            return DBWork.Connection.Query<MM_ITEMS_COMP>(sql, new { INID = id }, DBWork.Transaction);
        }

        public int Create(MM_ITEMS_COMP mm_items_comp)
        {
            var sql = @"INSERT INTO MM_ITEMS_COMP (INID, COMPLEXITY, CREATE_DATE, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                        VALUES (:INID, :COMPLEXITY, sysdate, :CREATE_USER, sysdate, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, mm_items_comp, DBWork.Transaction);
        }

        public int Update(MM_ITEMS_COMP mm_items_comp)
        {
            var sql = @"UPDATE MM_ITEMS_COMP 
                            SET COMPLEXITY = :COMPLEXITY,
                            UPDATE_TIME=sysdate,
                            UPDATE_USER=:UPDATE_USER,
                            UPDATE_IP=:UPDATE_IP
                            WHERE INID=:INID";

            return DBWork.Connection.Execute(sql, mm_items_comp, DBWork.Transaction);
        }

        public int Delete(string id)
        {
            var sql = @"DELETE FROM MM_ITEMS_COMP WHERE INID =:INID";
            return DBWork.Connection.Execute(sql, new { INID = id }, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetQueryInidCombo()
        {
            var p = new DynamicParameters();

            var sql = @" select INID as VALUE, INID || ' ' || INID_NAME as COMBITEM from UR_INID ";

            sql += " order by INID ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<MM_ITEMS_COMP> GetFormInidCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = @"select {0} INID, INID_NAME
                            from UR_INID where (INID_OLD <> 'D' or INID_OLD is null) ";

            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(INID, :INID_I), 100) * 10 + NVL(INSTR(INID_NAME, :INID_NAME_I), 100) * 10) IDX,");
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

            return DBWork.Connection.Query<MM_ITEMS_COMP>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public bool CheckExists(MM_ITEMS_COMP mm_items_comp)
        {
            string sql = @"SELECT 1 FROM MM_ITEMS_COMP WHERE INID=:INID";
            return !(DBWork.Connection.ExecuteScalar(sql, mm_items_comp, DBWork.Transaction) == null);
        }

        public IEnumerable<MM_ITEMS_COMP> GetInidNameByInid(string inid)
        {
            string sql = @"select INID_NAME from UR_INID
                            where INID=:INID";
            return DBWork.Connection.Query<MM_ITEMS_COMP>(sql, new { INID = inid }, DBWork.Transaction);
        }
    }
}