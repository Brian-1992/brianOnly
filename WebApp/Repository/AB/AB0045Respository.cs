using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JCLib.DB;
using Dapper;
using WebApp.Models;

namespace WebApp.Repository.AB
{
    public class AB0045Repository : JCLib.Mvc.BaseRepository
    { 
        public AB0045Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { } 

        public IEnumerable<ME_CSTM> GetAll(string wh_no, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT * FROM ME_CSTM WHERE 1=1 ";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_CSTM>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<ME_CSTM> Get(string vcstm)
        {
            var sql = @"SELECT * FROM ME_CSTM WHERE CSTM = :CSTM";
            return DBWork.Connection.Query<ME_CSTM>(sql, new { CSTM = vcstm }, DBWork.Transaction);
        }

        public int Create(ME_CSTM ME_CSTM) 
        {
            var sql = @"INSERT INTO ME_CSTM (CSTM, CREATE_TIME, CREATE_ID, UPDATE_TIME, UPDATE_ID, UPDATE_IP)  
                                     VALUES (:CSTM, SYSDATE, :CREATE_ID, SYSDATE, :UPDATE_ID, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, ME_CSTM, DBWork.Transaction); 
        }

        public int Update(string oldcstm,string newcstm, string update_id, string update_ip)
        {
            var p = new DynamicParameters();

            var sql = @"UPDATE ME_CSTM SET CSTM = :newp0,  
                               UPDATE_TIME = SYSDATE,
                               UPDATE_ID = :update_id, UPDATE_IP = :update_ip
                               WHERE CSTM = :oldp0 ";

            p.Add(":oldp0", string.Format("{0}", oldcstm.ToUpper()));
  
            p.Add(":newp0", string.Format("{0}", newcstm.ToUpper()));
       
            p.Add(":update_id", string.Format("{0}", update_id.ToUpper()));
            p.Add(":update_ip", string.Format("{0}", update_ip.ToUpper()));

            var cnt = DBWork.Connection.Execute(sql, p, DBWork.Transaction);

            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);

        }
        
        public bool CheckExists(string vcstm)
        {
            string sql = @"SELECT CSTM FROM ME_CSTM WHERE CSTM = :CSTM";
            return !(DBWork.Connection.ExecuteScalar(sql, new { CSTM = vcstm }, DBWork.Transaction) == null);
        }

        public bool CheckExistsModify(string vcstm)
        {
            string sql = @"SELECT * FROM ME_CSTM WHERE CSTM = :CSTM ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { CSTM = vcstm }, DBWork.Transaction) == null);
        }

        public int Delete(ME_CSTM ME_CSTM)
        {
            // 資料會在其他地方使用者,刪除時不直接刪除而是加上刪除旗標
            var sql = @"DELETE ME_CSTM WHERE CSTM = :CSTM "; ;
            return DBWork.Connection.Execute(sql, ME_CSTM, DBWork.Transaction); 
        }


    }
}
