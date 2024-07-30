using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.G
{
    public class GA0003Repository : JCLib.Mvc.BaseRepository
    {
        public GA0003Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<TC_MMAGEN> GetAll(string MMCODE, string AGEN_NAMEC,string MMNAME_C, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select * from 
                        (
                        select a.MMCODE,b.MMNAME_C,
                        a.AGEN_NAMEC,a.PUR_UNIT,a.IN_PURPRICE,a.PUR_SEQ,
                        a.CREATE_TIME,a.CREATE_USER,a.UPDATE_TIME,a.UPDATE_USER
                  from TC_MMAGEN a
                  left join TC_MAST b
                  on a.MMCODE=b.MMCODE
                ) A
                where 1=1 ";


            if (MMCODE != "")
            {
                sql += " AND MMCODE LIKE :p0 ";
                p.Add(":p0", string.Format("%{0}%", MMCODE));
            }
            if (AGEN_NAMEC != "")
            {
                sql += " AND AGEN_NAMEC LIKE :p1 ";
                p.Add(":p1", string.Format("%{0}%", AGEN_NAMEC));
            }
            if (MMNAME_C != "")
            {
                sql += " AND MMNAME_C LIKE :p2 ";
                p.Add(":p2", string.Format("%{0}%", MMNAME_C));
            }

            sql += " order by MMCODE,PUR_SEQ ";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<TC_MMAGEN>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<TC_MMAGEN> Get(TC_MMAGEN tc_mmagen)
        {
            var sql = @"select * from 
                        (
                        select a.MMCODE,b.MMNAME_C,
                        a.AGEN_NAMEC,a.PUR_UNIT,a.IN_PURPRICE,a.PUR_SEQ,
                        a.CREATE_TIME,a.CREATE_USER,a.UPDATE_TIME,a.UPDATE_USER
                  from TC_MMAGEN a
                  left join TC_MAST b
                  on a.MMCODE=b.MMCODE
                ) A
                    WHERE MMCODE = :MMCODE AND AGEN_NAMEC=:AGEN_NAMEC";
            sql += " order by MMCODE,PUR_SEQ ";

            return DBWork.Connection.Query<TC_MMAGEN>(sql, tc_mmagen, DBWork.Transaction);
        }

        public int Create(TC_MMAGEN tc_mmagen)
        {
            var sql = @"INSERT INTO TC_MMAGEN (MMCODE, AGEN_NAMEC, PUR_UNIT, IN_PURPRICE,PUR_SEQ,  CREATE_TIME, CREATE_USER,  UPDATE_IP,UPDATE_USER, UPDATE_TIME)  
                                VALUES (:MMCODE, :AGEN_NAMEC, :PUR_UNIT, :IN_PURPRICE,:PUR_SEQ,  sysdate, :CREATE_USER,  :UPDATE_IP, :UPDATE_USER , sysdate)";
            return DBWork.Connection.Execute(sql, tc_mmagen, DBWork.Transaction);
        }

        public int Update(TC_MMAGEN tc_mmagen)
        {
            var sql = @"UPDATE TC_MMAGEN 
                                SET  PUR_UNIT=:PUR_UNIT, IN_PURPRICE=:IN_PURPRICE,PUR_SEQ=:PUR_SEQ, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE MMCODE=:MMCODE AND AGEN_NAMEC=:AGEN_NAMEC";
            return DBWork.Connection.Execute(sql, tc_mmagen, DBWork.Transaction);
        }

        public int Delete(TC_MMAGEN tc_mmagen)
        {
            var sql = @"DELETE  TC_MMAGEN 
                                WHERE MMCODE=:MMCODE AND AGEN_NAMEC=:AGEN_NAMEC";
            return DBWork.Connection.Execute(sql, tc_mmagen, DBWork.Transaction);
        }

        public bool CheckExists(TC_MMAGEN tc_mmagen)
        {
            string sql = @"SELECT 1 FROM TC_MMAGEN WHERE MMCODE=:MMCODE AND AGEN_NAMEC=:AGEN_NAMEC";
            return !(DBWork.Connection.ExecuteScalar(sql, tc_mmagen, DBWork.Transaction) == null);
        }

        public bool CheckExists2(TC_MMAGEN tc_mmagen)
        {
            string sql = @"SELECT 1 FROM TC_MMAGEN WHERE MMCODE=:MMCODE AND PUR_SEQ=:PUR_SEQ";
            return !(DBWork.Connection.ExecuteScalar(sql, tc_mmagen, DBWork.Transaction) == null);
        }

        public bool CheckMmcodeExists(string mmcode) {
            string sql = @"SELECT 1 FROM TC_MAST WHERE MMCODE=:MMCODE";
            return DBWork.Connection.ExecuteScalar(sql, new { MMCODE = mmcode }, DBWork.Transaction) == null;

        }
    }
}