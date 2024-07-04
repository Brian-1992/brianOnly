using System;
using System.Collections.Generic;
using System.Web;
using WebApp.Models;
using WebApp.Models.AA;
using JCLib.DB;
using Dapper;
using System.Data;

namespace WebApp.Repository.AA
{
    public class AA0181Repository : JCLib.Mvc.BaseRepository
    {
        public AA0181Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AA0181MasterMODEL> GetAll(string p0, string p1, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT
                        A.CASENO F1,    --合約案號
                        A.JBID_RCRATE F2, --管理費%
                        A.DATA_YM
                        FROM RCRATE A
                        WHERE 1=1 ";

            if (p0.Trim() != "")
            {
                sql += " AND A.DATA_YM=:p0 ";
                p.Add(":p0", string.Format("{0}", p0));
            }

            if (p1.Trim() != "")
            {
                sql += " AND A.CASENO=:p1 ";
                p.Add(":p1", string.Format("{0}", p1));
            }

            sql += "ORDER BY A.CASENO";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0181MasterMODEL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public string MasterCreate(AA0181 input)
        {
            string sql = @" insert into RCRATE(DATA_YM, CASENO, JBID_RCRATE, CREATE_TIME, CREATE_USER, UPDATE_USER, UPDATE_TIME, UPDATE_IP) 
                                                values(:DATA_YM, :CASENO, :JBID_RCRATE, sysdate, :CREATE_USER, :UPDATE_USER,sysdate, :UPDATE_IP)";

            int effRows = DBWork.Connection.Execute(sql, input, DBWork.Transaction);

            if (effRows >= 1)
                return "新增成功";
            else
                return "新增失敗";
        }

        public int MasterUpdate(AA0181 input)
        {
            var sql = @"update RCRATE
                         SET JBID_RCRATE=:JBID_RCRATE, UPDATE_USER=:UPDATE_USER, UPDATE_IP=:UPDATE_IP , UPDATE_TIME=SYSDATE
                        where DATA_YM=:DATA_YM AND CASENO=:CASENO";
            return DBWork.Connection.Execute(sql, input, DBWork.Transaction);
        }

        public int InsertRCRATE_DEL(string caseno, string data_ym, string user)
        {
            var p = new DynamicParameters();
            string sql = @"
                insert into RCRATE_DEL(DATA_YM, CASENO, JBID_RCRATE, CREATE_USER, CREATE_TIME, UPDATE_USER, UPDATE_TIME, UPDATE_IP, DELETE_USER, DELETE_TIME)
                select DATA_YM, CASENO, JBID_RCRATE, CREATE_USER, CREATE_TIME, UPDATE_USER, UPDATE_TIME, UPDATE_IP, :DELETE_USER, sysdate
                from RCRATE
                where DATA_YM= :DATA_YM
                and CASENO= :CASENO 
                                    ";

            p.Add(":DELETE_USER", string.Format("{0}", user));
            p.Add(":DATA_YM", string.Format("{0}", data_ym));
            p.Add(":CASENO", string.Format("{0}", caseno));

            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }

        public int MasterDelete(string caseno, string data_ym)
        {
            var p = new DynamicParameters();
            string sql = @"
                delete from RCRATE
                where DATA_YM = :DATA_YM
                and CASENO = :CASENO
                                    ";

            p.Add(":DATA_YM", string.Format("{0}", data_ym));
            p.Add(":CASENO", string.Format("{0}", caseno));

            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }

        public bool CheckDateYm(string data_ym)
        {
            string sql = @"SELECT TWN_TODATE(:DATA_YM || '01') FROM DUAL";
            return (DBWork.Connection.ExecuteScalar(sql, new { DATA_YM = data_ym }, DBWork.Transaction) != null);
        }

        public class AA0181MasterMODEL : JCLib.Mvc.BaseModel
        {
            public string rnm { get; set; }
            public string F1 { get; set; }
            public string F2 { get; set; }
            public string DATA_YM { get; set; }
        }

        public DataTable GetExcel(string p0, string p1)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT
                        A.CASENO 合約案號,    --合約案號
                        A.JBID_RCRATE AS " + '"' + "管理費%" + '"' + @" --管理費%
                        FROM RCRATE A
                        WHERE 1=1 ";

            if (p0.Trim() != "")
            {
                sql += " AND A.DATA_YM=:p0 ";
                p.Add(":p0", string.Format("{0}", p0));
            }

            if (p1.Trim() != "")
            {
                sql += " AND A.CASENO=:p1 ";
                p.Add(":p1", string.Format("{0}", p1));
            }

            sql += "ORDER BY A.CASENO";

            ////p.Add("OFFSET", (page_index - 1) * page_size);
            ////p.Add("PAGE_SIZE", page_size);

            //return DBWork.Connection.Query<AA0181MasterMODEL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;


        }

        public string GetCurrentDataYm()
        {
            string sql = @"select SET_YM from MI_MNSET where SET_STATUS = 'N' ";

            return DBWork.Connection.QueryFirstOrDefault<string>(sql, null, DBWork.Transaction);
        }

        public bool CheckRcrateExists(string data_ym, string caseno)
        {
            string sql = @"SELECT 1 FROM RCRATE WHERE DATA_YM=:DATA_YM and CASENO = :CASENO ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DATA_YM = data_ym, CASENO = caseno }, DBWork.Transaction) == null);
        }

    }
}