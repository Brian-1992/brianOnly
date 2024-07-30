using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Data;

namespace WebApp.Repository.UR
{
    public class UR_FUNC_LOG_MRepository : JCLib.Mvc.BaseRepository
    {
        public UR_FUNC_LOG_MRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        public int Create(UR_FUNC_LOG_M ur_func_log_m)
        {
            var sql = $@"INSERT INTO UR_FUNC_LOG_M (IDNO,CTRL,ACT,CALL_TIME,TUSER,IP)
                        VALUES (to_number(:IDNO),:CTRL,:ACT,sysdate,:TUSER,:IP)";
            return DBWork.Connection.Execute(sql, ur_func_log_m, DBWork.Transaction);
        }
        public string GetNextIdno()
        {
            string sql = @"SELECT UR_FUNC_LOG_M_SEQ.nextval AS IDNO from dual";
            string rtn = DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction).ToString();
            return rtn;
        }
        public IEnumerable<UR_FUNC_LOG_M> Query(string idno, string una, string ctrl, string call_time_bg, string call_time_ed, string act)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT IDNO, CTRL, ACT, CALL_TIME, TUSER, IP, 
                        (SELECT UNA FROM UR_ID WHERE TUSER=EM.TUSER) UNA 
                        FROM UR_FUNC_LOG_M EM where 1=1 ";

            if (idno != "")
            {
                sql += " AND IDNO = :p0 ";
                p.Add(":p0", idno);
            }
            if (una != "")
            {
                sql += " AND TUSER IN (SELECT TUSER FROM UR_ID WHERE UNA LIKE :p1) ";
                p.Add(":p1", string.Format("%{0}%", una));
            }
            if (ctrl != "")
            {
                sql += " AND CTRL LIKE :p2 ";
                p.Add(":p2", string.Format("{0}%", ctrl));
            }
            if (act != "")
            {
                sql += " AND ACT LIKE :p5 ";
                p.Add(":p5", string.Format("%{0}%", act));
            }
            if (call_time_bg != "" && call_time_bg != null)
            {
                sql += " AND to_char(CALL_TIME,'yyyy-mm-dd') >= Substr(:p3, 1, 10) ";
                p.Add(":p3", call_time_bg);
            }
            if (call_time_ed != "" && call_time_ed != null)
            {
                sql += " AND to_char(CALL_TIME,'yyyy-mm-dd') <= Substr(:p4, 1, 10) ";
                p.Add(":p4", call_time_ed);
            }

            return DBWork.PagingQuery<UR_FUNC_LOG_M>(sql, p, DBWork.Transaction);
        }

        public DataTable GetExcelM(string idno, string una, string ctrl, string call_time_bg, string call_time_ed, string act)
        {
            DynamicParameters p = new DynamicParameters();

            string sql = @" select IDNO as 記錄編號, CTRL as 程式代碼, ACT as 函式代碼, 
                        CALL_TIME as 呼叫時間, TUSER as 帳號, 
                        (SELECT UNA FROM UR_ID WHERE TUSER=EM.TUSER) 執行人員姓名, IP 
                        FROM UR_FUNC_LOG_M EM where 1=1 ";

            if (idno != "")
            {
                sql += " AND IDNO = :p0 ";
                p.Add(":p0", idno);
            }
            if (una != "")
            {
                sql += " AND TUSER IN (SELECT TUSER FROM UR_ID WHERE UNA LIKE :p1) ";
                p.Add(":p1", string.Format("%{0}%", una));
            }
            if (ctrl != "")
            {
                sql += " AND CTRL LIKE :p2 ";
                p.Add(":p2", string.Format("{0}%", ctrl));
            }
            if (act != "")
            {
                sql += " AND ACT LIKE :p5 ";
                p.Add(":p5", string.Format("%{0}%", act));
            }
            if (call_time_bg != "" && call_time_bg != null)
            {
                sql += " AND twn_date(CALL_TIME) >= :p3 ";
                p.Add(":p3", call_time_bg);
            }
            if (call_time_ed != "" && call_time_ed != null)
            {
                sql += " AND twn_date(CALL_TIME) <= :p4 ";
                p.Add(":p4", call_time_ed);
            }
            sql += " order by IDNO ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
    }
}