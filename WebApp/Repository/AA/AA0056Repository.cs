using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.AA
{
    public class AA0056Repository : JCLib.Mvc.BaseRepository
    {
        public AA0056Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        //查詢Master
        public IEnumerable<AA0056M> GetAllM(int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @" SELECT MMCODE,
                                MMNAME_E,
                                E_SONTRANSQTY,
                                E_PARCODE,
                                (CASE
                                     WHEN E_PARCODE = '0' THEN '非母子藥'
                                     WHEN E_PARCODE = '1' THEN '母藥'
                                     WHEN E_PARCODE = '2' THEN '子藥'
                                END) E_PARCODE_NAME,
                                (CASE
                                     WHEN E_PARCODE = '0' THEN '0 非母子藥'
                                     WHEN E_PARCODE = '1' THEN '1 母藥'
                                     WHEN E_PARCODE = '2' THEN '2 子藥'
                                 END) E_PARCODE_CODE
                         FROM MI_MAST
                         WHERE E_PARCODE = '1'";

            sql += @"ORDER BY MMCODE";
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0056M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //查詢Detail
        public IEnumerable<AA0056D> GetAllD(string E_PARORDCODE, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT MMCODE,
                               MMNAME_E,
                               E_SONTRANSQTY,
                               E_PARCODE,
                               (CASE
                                    WHEN E_PARCODE = '0' THEN '非母子藥'
                                    WHEN E_PARCODE = '1' THEN '母藥'
                                    WHEN E_PARCODE = '2' THEN '子藥'
                                END) E_PARCODE_NAME,
                               (CASE
                                    WHEN E_PARCODE = '0' THEN '0 非母子藥'
                                    WHEN E_PARCODE = '1' THEN '1 母藥'
                                    WHEN E_PARCODE = '2' THEN '2 子藥'
                                END) E_PARCODE_CODE,
                               E_PARORDCODE
                        FROM MI_MAST
                        WHERE E_PARORDCODE = :E_PARORDCODE";

            p.Add(":E_PARORDCODE", string.Format("{0}", E_PARORDCODE));
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0056D>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //修改Master
        public int UpdateM(AA0056M AA0056M)
        {
            var sql = @"UPDATE MI_MAST SET 
                               E_SONTRANSQTY = :E_SONTRANSQTY,
                               UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                        WHERE  MMCODE = :MMCODE ";
            return DBWork.Connection.Execute(sql, AA0056M, DBWork.Transaction);
        }

        //修改Detail
        public int UpdateD(AA0056D AA0056D)
        {
            var sql = @"UPDATE MI_MAST SET 
                               E_SONTRANSQTY = :E_SONTRANSQTY,
                               UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                        WHERE  MMCODE = :MMCODE";
            return DBWork.Connection.Execute(sql, AA0056D, DBWork.Transaction);
        }

    }
}
