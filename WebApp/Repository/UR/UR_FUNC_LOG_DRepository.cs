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
    public class UR_FUNC_LOG_DRepository : JCLib.Mvc.BaseRepository
    {
        const int LEN_PN = 30;
        const int LEN_PV = 4000;
        public UR_FUNC_LOG_DRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        public int Create(UR_FUNC_LOG_D ur_func_log_d)
        {
            var sql = $@"INSERT INTO UR_FUNC_LOG_D (IDNO,PN,PV)
                        VALUES (to_number(:IDNO),substr(:PN,1,{LEN_PN}),substr(:PV,1,{LEN_PV}))";
            return DBWork.Connection.Execute(sql, ur_func_log_d, DBWork.Transaction);
        }
        public IEnumerable<UR_FUNC_LOG_D> Query(string idno)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT IDNO, PN, PV FROM UR_FUNC_LOG_D where IDNO=:IDNO ";

            return DBWork.PagingQuery<UR_FUNC_LOG_D>(sql, new { IDNO = idno }, DBWork.Transaction);
        }

        public DataTable GetExcelD(string idno)
        {
            string sql = @"SELECT IDNO as 記錄編號, PN as 參數名稱, PV as 參數值 FROM UR_FUNC_LOG_D where IDNO=:IDNO ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, new { IDNO = idno }, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public DataTable GetExcelD2(string idno, string una, string ctrl, string call_time_bg, string call_time_ed, string act)
        {
            DynamicParameters p = new DynamicParameters();

            string sql = @" SELECT A.IDNO as 記錄編號, B.PN as 參數名稱, B.PV as 參數值 
                        FROM UR_FUNC_LOG_M A, UR_FUNC_LOG_D B where A.IDNO=B.IDNO ";

            if (idno != "")
            {
                sql += " AND A.IDNO = :p0 ";
                p.Add(":p0", idno);
            }
            if (una != "")
            {
                sql += " AND A.TUSER IN (SELECT TUSER FROM UR_ID WHERE UNA LIKE :p1) ";
                p.Add(":p1", string.Format("%{0}%", una));
            }
            if (ctrl != "")
            {
                sql += " AND A.CTRL LIKE :p2 ";
                p.Add(":p2", string.Format("{0}%", ctrl));
            }
            if (act != "")
            {
                sql += " AND A.ACT LIKE :p5 ";
                p.Add(":p5", string.Format("{0}%", act));
            }
            if (call_time_bg != "" && call_time_bg != null)
            {
                sql += " AND twn_date(A.CALL_TIME) >= :p3 ";
                p.Add(":p3", call_time_bg);
            }
            if (call_time_ed != "" && call_time_ed != null)
            {
                sql += " AND twn_date(A.CALL_TIME) <= :p4 ";
                p.Add(":p4", call_time_ed);
            }
            sql += " order by A.IDNO ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
    }
}