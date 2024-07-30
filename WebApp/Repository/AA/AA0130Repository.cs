using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using WebApp.Models.MI;
using System.Text;

namespace WebApp.Repository.AA
{
    public class AA0130Repository : JCLib.Mvc.BaseRepository
    {
        public AA0130Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<MI_MNSET> GetAll(int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            StringBuilder sql = new StringBuilder();
            sql.Append("select SET_YM,TO_CHAR(TO_CHAR(SET_BTIME,'YYYY')-1911)||TO_CHAR(SET_BTIME,'MMDD HH24:MI')SET_BTIME");
            sql.Append(",TO_CHAR(TO_CHAR(SET_ETIME,'YYYY')-1911)||TO_CHAR(SET_ETIME,'MMDD HH24:MI')SET_ETIME");
            sql.Append(",CASE WHEN SET_STATUS='C' THEN '已完成月結' WHEN SET_STATUS='N' THEN '已開帳'");
            sql.Append(" WHEN SET_STATUS='P' THEN '月結進行中' END SET_STATUS");
            sql.Append(",SET_MSG,TWN_DATE(SET_CTIME) SET_CTIME from MI_MNSET");
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MNSET>(GetPagingStatement(sql.ToString(), sorters), p, DBWork.Transaction);
        }
        public int Update(MI_MNSET m)
        {
            string sql = "UPDATE MI_MNSET SET SET_CTIME=TO_DATE(:SET_CTIME||' 235900','YYYYMMDD HH24:MI:SS') WHERE SET_YM=:SET_YM";
            return DBWork.Connection.Execute(sql, m, DBWork.Transaction);
        }
        public int Create(UR_BULLETIN bulletin)
        {
            string id_sql = "SELECT NVL(MAX(ID+1), 0) NEW_ID FROM UR_BULLETIN";
            var objID = DBWork.Connection.ExecuteScalar(id_sql, null, DBWork.Transaction);
            bulletin.ID = objID.ToString();

            string sql = @"INSERT INTO UR_BULLETIN (ID, TITLE, CONTENT, TARGET, 
                           ON_DATE, VALID, UPLOAD_KEY, CREATE_BY, CREATE_DT)  
                           VALUES (:ID, :TITLE, :CONTENT, :TARGET, 
                           :ON_DATE, :VALID, :UPLOAD_KEY, :CREATE_BY, SYSDATE)";
            return DBWork.Connection.Execute(sql, bulletin, DBWork.Transaction);
        }
    }
}
