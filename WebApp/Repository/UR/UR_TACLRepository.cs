using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Data;

namespace WebApp.Repository.UR
{
    public class UR_TACLRepository : JCLib.Mvc.BaseRepository
    {
        public UR_TACLRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        // UR1025使用者權限明細
        public IEnumerable<UR_TACL2> GetTaclAll(string tuser, string una, string fg, string rlno, string r, string u, string p, string inid, string aduser)
        {
            var par = new DynamicParameters();

            var sql = string.Empty;

            

            if (tuser != "" && tuser != null)
            {
                sql += " and A.TUSER like :p0 ";
                
            }

            if (una != "" && una != null)
            {
                sql += " and A.UNA like :p1 ";
                
            }

            if (fg != "" && fg != null)
            {
                sql += " and D.FG like :p2 ";
                
            }

            if (rlno != "" && rlno != null)
            {
                sql += " and B.RLNO like :p3 ";
                
            }

            string addOr = "";
            sql += " and ( ";
            if (r != "" && r != null)
            {
                sql += " D.R = :p4 ";
                
                addOr = " or ";
            }
            if (u != "" && u != null)
            {
                sql += addOr + " D.U = :p5 ";
                
                addOr = " or ";
            }
            if (p != "" && p != null)
            {
                sql += addOr + " D.P = :p6 ";
                
            }
            sql += " ) ";

            if (inid != "" && inid != null)
            {
                sql += " and A.INID like :p7 ";
                
            }

            if (aduser != "" && aduser != null)
            {
                sql += " and A.ADUSER like :p8 ";
                
            }

            sql = string.Format(@" 
                select A.TUSER, A.UNA, A.INID, A.ADUSER,
                       (select INID_NAME from UR_INID where INID = A.INID) as INID_NAME,
                       B.RLNO, C.RLNA, to_char(C.RLDESC) as rldesc,
                       D.FG, (select F1 from UR_MENU where FG = D.FG) as F1,
                       D.R, D.U, D.P
                  from UR_ID A, UR_UIR B, UR_ROLE C, UR_TACL D
                 where A.TUSER = B.TUSER
                   and B.RLNO = C.RLNO
                   and B.RLNO = D.RLNO 
                    {0}
                union
                select A.TUSER, A.UNA, A.INID, A.ADUSER,
                       (select INID_NAME from UR_INID where INID = A.INID) as INID_NAME,
                       '' as RLNO, '' as RLNA, '' as RLDESC,
                       D.FG, (select F1 from UR_MENU where FG = D.FG) as F1,
                       D.R, D.U, D.P
                  from UR_ID A, UR_TACL2 D
                 where A.TUSER = D.TUSER
                    {0}
                ", sql);

            par.Add(":p0", string.Format("%{0}%", tuser));
            par.Add(":p1", string.Format("%{0}%", una));
            par.Add(":p2", string.Format("%{0}%", fg));
            par.Add(":p3", string.Format("%{0}%", rlno));
            par.Add(":p4", r);
            par.Add(":p5", u);
            par.Add(":p6", p);
            par.Add(":p7", string.Format("{0}%", inid));
            par.Add(":p8", string.Format("%{0}%", aduser));

            return DBWork.PagingQuery<UR_TACL2>(sql, par, DBWork.Transaction);
        }

        public DataTable GetTaclAllExcel(string tuser, string una, string fg, string rlno, string r, string u, string p, string inid, string aduser)
        {
            DynamicParameters par = new DynamicParameters();

            string sql = string.Empty;
            if (tuser != "" && tuser != null)
            {
                sql += " and A.TUSER like :p0 ";

            }

            if (una != "" && una != null)
            {
                sql += " and A.UNA like :p1 ";

            }

            if (fg != "" && fg != null)
            {
                sql += " and D.FG like :p2 ";

            }

            if (rlno != "" && rlno != null)
            {
                sql += " and B.RLNO like :p3 ";

            }

            string addOr = "";
            sql += " and ( ";
            if (r != "" && r != null)
            {
                sql += " D.R = :p4 ";

                addOr = " or ";
            }
            if (u != "" && u != null)
            {
                sql += addOr + " D.U = :p5 ";

                addOr = " or ";
            }
            if (p != "" && p != null)
            {
                sql += addOr + " D.P = :p6 ";

            }
            sql += " ) ";

            if (inid != "" && inid != null)
            {
                sql += " and A.INID like :p7 ";

            }

            if (aduser != "" && aduser != null)
            {
                sql += " and A.ADUSER like :p8 ";

            }

            sql = string.Format(@" 
                    select * 
                      from (
                            select A.TUSER as 帳號, A.UNA as 姓名, A.INID as 責任中心代碼, 
                                   (select INID_NAME from UR_INID where INID = A.INID) as 責任中心名稱, A.ADUSER as AD帳號,
                                   B.RLNO as 群組代碼, C.RLNA as 群組名稱, to_char(C.RLDESC) 群組說明,
                                   D.FG as 程式編號, (select F1 from UR_MENU where FG = D.FG) as 程式名稱,
                                   case when D.R = 1 then 'v' else '' end as 查詢, 
                                   case when D.U = 1 then 'v' else '' end as 維護, 
                                   case when D.P = 1 then 'v' else '' end as 列印
                              from UR_ID A, UR_UIR B, UR_ROLE C, UR_TACL D
                             where A.TUSER = B.TUSER
                               and B.RLNO = C.RLNO
                               and B.RLNO = D.RLNO 
                                {0}
                            union
                            select A.TUSER as 帳號, A.UNA as 姓名, A.INID as 責任中心代碼,
                                   (select INID_NAME from UR_INID where INID = A.INID) as 責任中心名稱, A.ADUSER as AD帳號,
                                   '' as 群組代碼, '' as 群組名稱, '' as 群組說明,
                                   D.FG as 程式編號, (select F1 from UR_MENU where FG = D.FG) as 程式名稱,
                                   case when D.R = 1 then 'v' else '' end as 查詢, 
                                   case when D.U = 1 then 'v' else '' end as 維護, 
                                   case when D.P = 1 then 'v' else '' end as 列印
                              from UR_ID A, UR_TACL2 D
                            where A.TUSER = D.TUSER
                                {0}
                           ) a
                    order by 帳號,群組代碼,程式編號
            ", sql);

            par.Add(":p0", string.Format("%{0}%", tuser));
            par.Add(":p1", string.Format("%{0}%", una));
            par.Add(":p2", string.Format("%{0}%", fg));
            par.Add(":p3", string.Format("%{0}%", rlno));
            par.Add(":p4", r);
            par.Add(":p5", u);
            par.Add(":p6", p);
            par.Add(":p7", string.Format("{0}%", inid));
            par.Add(":p8", string.Format("%{0}%", aduser));

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, par, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public int Copy(string rlno_src, string rlno_des)
        {
            int _afrs = 0;

            string sql = @"INSERT INTO UR_TACL (FG, RLNO, V, R, U, P) 
                    SELECT FG, :RLNO_DES, V, R, U, P FROM UR_TACL WHERE RLNO = :RLNO_SRC";
            _afrs += DBWork.Connection.Execute(sql, new { RLNO_DES = rlno_des, RLNO_SRC = rlno_src }, DBWork.Transaction);

            return _afrs;
        }

        public int Update(string rlno, UR_TACL[] ur_tacl)
        {
            //string upd_sql = @"UPDATE UR_TACL SET V=:V, R=:R, U=:U, P=:P WHERE RLNO=:RLNO AND FG=:FG) ";
            string del_sql = @"DELETE FROM UR_TACL WHERE RLNO=:RLNO AND FG=:FG";
            string ins_sql = @"INSERT INTO UR_TACL (FG, RLNO, V, R, U, P) VALUES (:FG, :RLNO, :V, :R, :U, :P)";
            int _afrs = 0;
            foreach (var item in ur_tacl)
            {
                item.RLNO = rlno;
                _afrs += DBWork.Connection.Execute(del_sql, item, DBWork.Transaction);
                if (item.V == 1 || item.R == 1 || item.U == 1 || item.P == 1)
                {
                    _afrs += DBWork.Connection.Execute(ins_sql, item, DBWork.Transaction);
                }
            }

            return _afrs;
        }

        public int DeleteByRole(string rlno)
        {
            string sql = @"DELETE FROM UR_TACL WHERE RLNO=:RLNO";
            return DBWork.Connection.Execute(sql, new { RLNO = rlno }, DBWork.Transaction);
        }

        public int DeleteByFG(string fg)
        {
            var _afrs = 0;
            var sql = @"DELETE FROM UR_TACL WHERE FG=:FG";

            _afrs = DBWork.Connection.Execute(sql, new { FG = fg }, DBWork.Transaction);

            return _afrs;
        }
    }
}