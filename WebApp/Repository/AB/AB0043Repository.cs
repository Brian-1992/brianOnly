using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;

namespace WebApp.Repository.AB
{ 
    public class AB0043Repository : JCLib.Mvc.BaseRepository 
    {
        public AB0043Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<PHRSDPT> GetAll(string wh_no, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT * FROM PHRSDPT WHERE 1=1 ";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<PHRSDPT>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<PHRSDPT> Get(string rxtype, string rxdatekind)
        {
            var sql = @"SELECT * FROM PHRSDPT WHERE RXTYPE = :RXTYPE and RXDATEKIND = :RXDATEKIND";
            return DBWork.Connection.Query<PHRSDPT>(sql, new { RXTYPE = rxtype, RXDATEKIND = rxdatekind }, DBWork.Transaction);
        }

        public int Create(PHRSDPT PHRSDPT)
        {
            var sql = @"INSERT INTO PHRSDPT (RXTYPE, RXDATEKIND, DEADLINETIME, WORKFLAG, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                                     VALUES (:RXTYPE, :RXDATEKIND, :DEADLINETIME, :WORKFLAG, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, PHRSDPT, DBWork.Transaction);
        }

        public int Update(string oldrxtype, string oldrxdatekind, string olddeadlinetime, string oldworkflag, string newrxtype, string newrxdatekind, string newdeadlinetime, string newworkflag, string update_user, string update_ip)
        {
            var p = new DynamicParameters();

            var sql = @"UPDATE PHRSDPT SET RXTYPE = :newp0, RXDATEKIND = :newp1, DEADLINETIME = :newp2, WORKFLAG = :newp3,  
                               UPDATE_TIME = SYSDATE,
                               UPDATE_USER = :update_user, UPDATE_IP = :update_ip
                               WHERE RXTYPE = :oldp0 and RXDATEKIND = :oldp1 and DEADLINETIME = :oldp2 and WORKFLAG = :oldp3 ";

            p.Add(":oldp0", string.Format("{0}", oldrxtype.ToUpper()));
            p.Add(":oldp1", string.Format("{0}", oldrxdatekind.ToUpper()));
            p.Add(":oldp2", string.Format("{0}", olddeadlinetime.ToUpper()));
            p.Add(":oldp3", string.Format("{0}", oldworkflag.ToUpper()));

            p.Add(":newp0", string.Format("{0}", newrxtype.ToUpper()));
            p.Add(":newp1", string.Format("{0}", newrxdatekind.ToUpper()));
            p.Add(":newp2", string.Format("{0}", newdeadlinetime.ToUpper()));
            p.Add(":newp3", string.Format("{0}", newworkflag.ToUpper()));


            p.Add(":update_user", string.Format("{0}", update_user.ToUpper()));
            p.Add(":update_ip", string.Format("{0}", update_ip.ToUpper()));

            var cnt = DBWork.Connection.Execute(sql, p, DBWork.Transaction);

            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);

        }


        public bool CheckExists(string rxtype, string rxdatekind,string deadlinetime, string workflag)
        {
            string sql = @"SELECT * FROM PHRSDPT WHERE RXTYPE =:RXTYPE and RXDATEKIND = :RXDATEKIND and DEADLINETIME=:DEADLINETIME and WORKFLAG = :WORKFLAG ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { RXTYPE = rxtype, RXDATEKIND = rxdatekind, DEADLINETIME = deadlinetime, WORKFLAG = workflag }, DBWork.Transaction) == null);
        }

        public bool CheckExistsModify(string vrxtype, string vrxdatekind, string vdeadlinetime, string vworkflag)
        {
            string sql = @"SELECT * FROM PHRSDPT WHERE RXTYPE =:RXTYPE and RXDATEKIND = :RXDATEKIND and DEADLINETIME=:DEADLINETIME and WORKFLAG = :WORKFLAG ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { RXTYPE = vrxtype, RXDATEKIND = vrxdatekind, DEADLINETIME = vdeadlinetime, WORKFLAG = vworkflag }, DBWork.Transaction) == null);
            //return !(DBWork.Connection.ExecuteScalar(sql, PHRSDPT, DBWork.Transaction) == null);
        }

        public int Delete(PHRSDPT PHRSDPT)
        {
            // 資料會在其他地方使用者,刪除時不直接刪除而是加上刪除旗標
            var sql = @"DELETE PHRSDPT WHERE RXTYPE = :RXTYPE and RXDATEKIND = :RXDATEKIND and DEADLINETIME= : DEADLINETIME and WORKFLAG = : WORKFLAG "; ;
            return DBWork.Connection.Execute(sql, PHRSDPT, DBWork.Transaction);
        }



    }
}
