using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using WebApp.Models.D;
using System.Collections.Generic;

namespace WebApp.Repository.AA
{
    public class AA0226Repository : JCLib.Mvc.BaseRepository
    {
        public AA0226Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<DGMISS_SUPPLY> GetAll(string s_inid, string a_inid, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select a.SUPPLY_INID, (select INID||''||INID_NAME from UR_INID where INID=a.SUPPLY_INID) as SUPPLY_INID_T,
                               a.APP_INID, (select INID||''||INID_NAME from UR_INID where INID=a.APP_INID) as APP_INID_T
                        from DGMISS_SUPPLY a
                        where 1 = 1 
                         ";

            if (s_inid != "" && s_inid != null)
            {
                sql += " and a.SUPPLY_INID = :p0 ";
                p.Add(":p0", s_inid);
            }
            if (a_inid != "" && a_inid != null)
            {
                sql += " and a.APP_INID = :p1 ";
                p.Add(":p1", a_inid);
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<DGMISS_SUPPLY>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<DGMISS_SUPPLY> Get(string s_inid, string a_inid)
        {
            var sql = @"select a.SUPPLY_INID, (select INID||''||INID_NAME from UR_INID where INID=a.SUPPLY_INID) as SUPPLY_INID_T,
                               a.APP_INID, (select INID||''||INID_NAME from UR_INID where INID=a.APP_INID) as APP_INID_T
                        from DGMISS_SUPPLY a
                        where 1 = 1  
                          AND a.SUPPLY_INID = :SUPPLY_INID AND a.APP_INID = :APP_INID ";
            return DBWork.Connection.Query<DGMISS_SUPPLY>(sql, new { SUPPLY_INID = s_inid, APP_INID = a_inid }, DBWork.Transaction);
        }

        public int Create(DGMISS_SUPPLY dgmmiss_supply)
        {
            var sql = @"INSERT INTO DGMISS_SUPPLY (SUPPLY_INID, APP_INID)  
                                           VALUES (:SUPPLY_INID, :APP_INID)";
            return DBWork.Connection.Execute(sql, dgmmiss_supply, DBWork.Transaction);
        }
        
        public int Delete(string s_inid, string a_inid)
        {
            // 刪除資料
            var sql = @"DELETE DGMISS_SUPPLY WHERE SUPPLY_INID = :SUPPLY_INID AND APP_INID = :APP_INID ";
            return DBWork.Connection.Execute(sql, new { SUPPLY_INID = s_inid, APP_INID = a_inid }, DBWork.Transaction);
        }

        public bool CheckExists(string s_inid, string a_inid)
        {
            string sql = @"SELECT 1 FROM DGMISS_SUPPLY WHERE SUPPLY_INID = :SUPPLY_INID AND APP_INID = :APP_INID ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { SUPPLY_INID = s_inid, APP_INID = a_inid }, DBWork.Transaction) == null);
        }
        

        public int CheckDgmissNum(string s_inid, string a_inid)
        {
            string sql = @"SELECT COUNT(*) AS CNT FROM DGMISS  
                            WHERE IS_DEL='N' 
                            AND SUPPLY_INID = :SUPPLY_INID AND APP_INID = :APP_INID ";
            int rtn = Convert.ToInt32(DBWork.Connection.ExecuteScalar(sql, new { SUPPLY_INID = s_inid, APP_INID = a_inid }, DBWork.Transaction).ToString());
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
        

    }
}