using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JCLib.DB;
using WebApp.Models;
using Dapper;
using System.Data;

namespace WebApp.Repository.B
{
    public class BC0010Repository : JCLib.Mvc.BaseRepository
    {
        public BC0010Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<PH_SMALL_MAIL> GetAll(string xcategory, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT * FROM PH_SMALL_MAIL  ";
            
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<PH_SMALL_MAIL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<PH_SMALL_MAIL> Get(string SEND_TO)
        {
            var sql = @"SELECT * FROM PH_SMALL_MAIL 
                         WHERE SEND_TO = :SEND_TO";
            return DBWork.Connection.Query<PH_SMALL_MAIL>(sql, new { SEND_TO = SEND_TO }, DBWork.Transaction);
        }

        public int Create(PH_SMALL_MAIL PH_SMALL_MAIL)
        {
            var sql = @"INSERT INTO PH_SMALL_MAIL (SEND_TO, MAIL_ADD, MEMO)  
                        VALUES (:SEND_TO, :MAIL_ADD, :MEMO)";
            var req = new { SEND_TO = PH_SMALL_MAIL.SEND_TO, MAIL_ADD = PH_SMALL_MAIL.MAIL_ADD, MEMO = PH_SMALL_MAIL.MEMO };
            return DBWork.Connection.Execute(sql, req, DBWork.Transaction);
        }

        public int Update(PH_SMALL_MAIL PH_SMALL_MAIL)
        {
            var sql = @"UPDATE PH_SMALL_MAIL 
                           SET MAIL_ADD = :MAIL_ADD, 
                               MEMO = :MEMO
                         WHERE SEND_TO = :SEND_TO";
            var req = new { SEND_TO = PH_SMALL_MAIL.SEND_TO, MAIL_ADD = PH_SMALL_MAIL.MAIL_ADD, MEMO = PH_SMALL_MAIL.MEMO };
            return DBWork.Connection.Execute(sql, req, DBWork.Transaction);
        }

        public int Delete(string SEND_TO)
        {
            var sql = @"DELETE FROM PH_SMALL_MAIL
                         WHERE SEND_TO = :SEND_TO";
            return DBWork.Connection.Execute(sql, new { SEND_TO = SEND_TO }, DBWork.Transaction);
        }

        public bool CheckExists(string SEND_TO) {
            string sql = @"SELECT 1 FROM PH_SMALL_MAIL 
                            WHERE SEND_TO = :SEND_TO";
            return !(DBWork.Connection.ExecuteScalar(sql, new { SEND_TO = SEND_TO }, DBWork.Transaction) == null);
        }
    }
}