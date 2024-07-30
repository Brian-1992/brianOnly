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
    public class UR_ERR_DRepository : JCLib.Mvc.BaseRepository
    {
        const int LEN_PN = 30;
        const int LEN_PV = 2000;
        public UR_ERR_DRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        public int Create(UR_ERR_D ur_err_d)
        {
            var sql = $@"INSERT INTO UR_ERR_D (IDNO,PN,PV)
                        VALUES (to_number(:IDNO),substr(:PN,1,{LEN_PN}),substr(:PV,1,{LEN_PV}))";
            return DBWork.Connection.Execute(sql, ur_err_d, DBWork.Transaction);
        }
        public IEnumerable<UR_ERR_D> Query(string idno)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT IDNO, PN, PV FROM UR_ERR_D where IDNO=:IDNO ";

            return DBWork.PagingQuery<UR_ERR_D>(sql, new { IDNO = idno }, DBWork.Transaction);
        }

        public DataTable GetExcelD(string idno)
        {
            string sql = @"SELECT IDNO as 錯誤號碼, PN as 參數名稱, PV as 參數值 FROM UR_ERR_D where IDNO=:IDNO ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, new { IDNO = idno }, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public DataTable GetExcelD2(string idno, string una, string ctrl, string ed_bg, string ed_ed)
        {
            DynamicParameters p = new DynamicParameters();

            string sql = @" SELECT A.IDNO as 錯誤號碼, B.PN as 參數名稱, B.PV as 參數值 
                        FROM UR_ERR_M A, UR_ERR_D B where A.IDNO=B.IDNO ";

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
            if (ed_bg != "" && ed_bg != null)
            {
                sql += " AND twn_date(A.ED) >= :p3 ";
                p.Add(":p3", ed_bg);
            }
            if (ed_ed != "" && ed_ed != null)
            {
                sql += " AND twn_date(A.ED) <= :p4 ";
                p.Add(":p4", ed_ed);
            }
            sql += " order by A.IDNO ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
    }
}