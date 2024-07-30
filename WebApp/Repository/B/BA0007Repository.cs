using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace WebApp.Repository.B
{
    public class BA0007Repository : JCLib.Mvc.BaseRepository
    {
        public BA0007Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<MM_PR_DTBAS> getAll(int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            string sql = @"select MAT_CLSID, M_STOREID, BEGINDATE, ENDDATE, SUMDATE, DATEBAS, MTHBAS, LASTDELI_MTH, LASTDELI_DT,
                            MMPRDTBAS_SEQ, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER
                            FROM MM_PR_DTBAS";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MM_PR_DTBAS>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public int CreateDTBAS(MM_PR_DTBAS mm_pr_dtbas)
        {
            var sql = @"insert into MM_PR_DTBAS (MMPRDTBAS_SEQ, MAT_CLSID, M_STOREID, BEGINDATE, ENDDATE, SUMDATE, DATEBAS, MTHBAS,
                                LASTDELI_MTH, LASTDELI_DT, CREATE_TIME, CREATE_USER, UPDATE_IP)  
                                values (:MMPRDTBAS_SEQ, :MAT_CLSID, :M_STOREID, :BEGINDATE, :ENDDATE, :SUMDATE, :DATEBAS, :MTHBAS,
                                :LASTDELI_MTH, :LASTDELI_DT, sysdate, :CREATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, mm_pr_dtbas, DBWork.Transaction);
        }

        public int UpdateDTBAS(MM_PR_DTBAS mm_pr_dtbas)
        {
            var sql = @"update MM_PR_DTBAS
                       set MAT_CLSID = :MAT_CLSID, M_STOREID = :M_STOREID, 
                           BEGINDATE=:BEGINDATE, ENDDATE=:ENDDATE,
                           SUMDATE=:SUMDATE, DATEBAS = :DATEBAS, MTHBAS=:MTHBAS,
                           LASTDELI_MTH=:LASTDELI_MTH, LASTDELI_DT=:LASTDELI_DT,
                           UPDATE_TIME=sysdate,
                           UPDATE_USER=:UPDATE_USER,
                           UPDATE_IP=:UPDATE_USER
                    where MMPRDTBAS_SEQ=:MMPRDTBAS_SEQ ";
            return DBWork.Connection.Execute(sql, mm_pr_dtbas, DBWork.Transaction);
        }

        public int DeleteDTBAS(string seq)
        {
            var sql = @"delete from MM_PR_DTBAS where MMPRDTBAS_SEQ=:MMPRDTBAS_SEQ ";
            return DBWork.Connection.Execute(sql, new { MMPRDTBAS_SEQ = seq }, DBWork.Transaction);
        }

        public IEnumerable<MM_PR_DTBAS> GetDTBASList(string seq)
        {
            var p = new DynamicParameters();
            string sql = @"select BEGINDATE, ENDDATE, SUMDATE, MMPRDTBAS_SEQ
                            FROM MM_PR_DTBAS 
                            where M_STOREID = '0' ";
            // 若有傳入MMPRDTBAS_SEQ, 則查詢結果不包含傳入流水號的資料
            if (seq != "")
            {
                sql += " and MMPRDTBAS_SEQ <> :SEQ";
                p.Add(":SEQ", string.Format("{0}", seq));
            }
                

            return DBWork.Connection.Query<MM_PR_DTBAS>(sql, p, DBWork.Transaction);
        }

        public string getSeq()
        {
            var p = new DynamicParameters();
            var sql = @" select MMPRDTBAS_SEQ.nextval as SEQ
                            from dual ";
            return DBWork.Connection.QueryFirst<string>(sql, p, DBWork.Transaction);
        }

        public bool ChceckDataExist(string m_storeid, string mat_clsid, string seq)
        {
            var p = new DynamicParameters();
            string sql = @"
                select 1 from MM_PR_DTBAS
                 where M_STOREID = :M_STOREID
                   and MAT_CLSID = :MAT_CLSID
            ";

            p.Add(":M_STOREID", string.Format("{0}", m_storeid));
            p.Add(":MAT_CLSID", string.Format("{0}", mat_clsid));

            // 若有傳入MMPRDTBAS_SEQ, 則不包含傳入流水號的資料
            if (seq != "")
            {
                sql += " and MMPRDTBAS_SEQ <> :SEQ";
                p.Add(":SEQ", string.Format("{0}", seq));
            }

            return DBWork.Connection.ExecuteScalar(sql, p, DBWork.Transaction) != null;
        }
    }
}