using Dapper;
using JCLib.DB;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Repository.AA
{
    public class AA0161Repository : JCLib.Mvc.BaseRepository
    {
        public AA0161Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<SEC_MAST> GetAll(string sectonno, string sectionname, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT * FROM SEC_MAST WHERE 1=1 ";

            if (sectonno != "")
            {
                sql += " AND SECTIONNO LIKE :p0 ";
                p.Add(":p0", string.Format("%{0}%", sectonno));
            }

            if (sectionname != "")
            {
                sql += " AND SECTIONNAME LIKE :p1 ";
                p.Add(":p1", string.Format("%{0}%", sectionname));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<SEC_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<SEC_MAST> Get(string sectonno)
        {
            var sql = @"SELECT * FROM SEC_MAST WHERE SECTIONNO = :SECTIONNO";
            return DBWork.Connection.Query<SEC_MAST>(sql, new { SECTIONNO = sectonno }, DBWork.Transaction);
        }

        public int Create(SEC_MAST sec_mast)
        {
            var sql = @"INSERT INTO SEC_MAST (SECTIONNO, SECTIONNAME, SEC_ENABLE, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                                VALUES (:SECTIONNO, :SECTIONNAME, :SEC_ENABLE, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, sec_mast, DBWork.Transaction);
        }

        public int Update(SEC_MAST sec_mast)
        {
            var sql = @"UPDATE SEC_MAST SET SECTIONNAME = :SECTIONNAME, SEC_ENABLE = :SEC_ENABLE, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE SECTIONNO = :SECTIONNO";
            return DBWork.Connection.Execute(sql, sec_mast, DBWork.Transaction);
        }

        public int Delete(string sectonno)
        {
            var sql = @"UPDATE SEC_MAST SET SEC_ENABLE = 'N' WHERE SECTIONNO = :SECTIONNO";
            return DBWork.Connection.Execute(sql, new { SECTIONNO = sectonno }, DBWork.Transaction);
        }

        public bool CheckExists(string sectonno)
        {
            string sql = @"SELECT 1 FROM SEC_MAST WHERE SECTIONNO=:SECTIONNO";
            return !(DBWork.Connection.ExecuteScalar(sql, new { SECTIONNO = sectonno }, DBWork.Transaction) == null);
        }

        public bool CheckExistsInCalloc(string sectonno)
        {
            string sql = @"SELECT 1 FROM SEC_CALLOC WHERE SECTIONNO=:SECTIONNO and SEC_DISRATIO > 0";
            return !(DBWork.Connection.ExecuteScalar(sql, new { SECTIONNO = sectonno }, DBWork.Transaction) == null);
        }
    }
}