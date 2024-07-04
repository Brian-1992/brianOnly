using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.AA
{
    public class AA0070Repository : JCLib.Mvc.BaseRepository
    {
        public AA0070Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ME_DOCD> GetAll(string DIS_TIME_B, string DIS_TIME_E,bool P3,string APPTIME_B, string APPTIME_E, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select M.DOCNO,
                                M.TOWH WH_NO,
                                WH_NAME(M.TOWH) WH_NAME,
                                D.MMCODE,
                                (select MMNAME_C from MI_MAST where MMCODE=D.MMCODE) MMNAME_C,
                                (select MMNAME_E from MI_MAST where MMCODE=D.MMCODE) MMNAME_E,
                                (select BASE_UNIT from MI_MAST where MMCODE=D.MMCODE) BASE_UNIT,
                                D.BW_MQTY,
                                D.RV_MQTY,
                                TWN_DATE(M.APPTIME) APPTIME,
                                TWN_DATE(D.DIS_TIME) DIS_TIME
                        from ME_DOCM M left join ME_DOCD D on M.DOCNO=D.DOCNO
                        where D.BW_MQTY<>0 
                          and M.MAT_CLASS = '02'";

            if (DIS_TIME_B!="")
            {
                sql += " AND TWN_DATE(D.DIS_TIME) >= :DIS_TIME_B ";
                p.Add(":DIS_TIME_B", string.Format("{0}", DIS_TIME_B));
            }
            if (DIS_TIME_E != "")
            {
                sql += " AND TWN_DATE(D.DIS_TIME) <= :DIS_TIME_E ";
                p.Add(":DIS_TIME_E", string.Format("{0}", DIS_TIME_E));
            }
            if (P3)
            {
                sql += " AND D.BW_MQTY<>D.RV_MQTY ";
            }
            if(APPTIME_B != string.Empty) {
                sql += " AND TWN_DATE(M.APPTIME) >= :APPTIME_B ";
                p.Add(":APPTIME_B", string.Format("{0}", APPTIME_B));
            }
            if (APPTIME_E != string.Empty)
            {
                sql += " AND TWN_DATE(M.APPTIME) <= :APPTIME_E ";
                p.Add(":APPTIME_E", string.Format("{0}", APPTIME_E));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCD>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public DataTable GetExcel(string DIS_TIME_B, string DIS_TIME_E, bool P3, string APPTIME_B, string APPTIME_E)
        {
            var p = new DynamicParameters();

            var sql = @"select M.DOCNO 申請單號,
                                M.TOWH 庫房代碼 ,
                                WH_NAME(M.TOWH) 庫房名稱,
                                D.MMCODE 院內碼,
                                (select MMNAME_C from MI_MAST where MMCODE=D.MMCODE) 中文品名,
                                (select MMNAME_E from MI_MAST where MMCODE=D.MMCODE) 英文品名,
                                (select BASE_UNIT from MI_MAST where MMCODE=D.MMCODE) 計量單位,
                                D.BW_MQTY 調撥量,
                                D.RV_MQTY 歸墊量,
                                TWN_DATE(M.APPTIME) 申請日, 
                                TWN_DATE(D.DIS_TIME) 調撥日
                        from ME_DOCM M left join ME_DOCD D on M.DOCNO=D.DOCNO
                        where D.BW_MQTY<>0 
                          and M.MAT_CLASS = '02'";

            if (DIS_TIME_B != "")
            {
                sql += " AND TWN_DATE(D.DIS_TIME) >= :DIS_TIME_B ";
                p.Add(":DIS_TIME_B", string.Format("{0}", DIS_TIME_B));
            }
            if (DIS_TIME_E != "")
            {
                sql += " AND TWN_DATE(D.DIS_TIME) <= :DIS_TIME_E ";
                p.Add(":DIS_TIME_E", string.Format("{0}", DIS_TIME_E));
            }
            if (P3)
            {
                sql += " AND D.BW_MQTY<>D.RV_MQTY ";
            }
            if (APPTIME_B != string.Empty)
            {
                sql += " AND TWN_DATE(M.APPTIME) >= :APPTIME_B ";
                p.Add(":APPTIME_B", string.Format("{0}", APPTIME_B));
            }
            if (APPTIME_E != string.Empty)
            {
                sql += " AND TWN_DATE(M.APPTIME) <= :APPTIME_E ";
                p.Add(":APPTIME_E", string.Format("{0}", APPTIME_E));
            }
            sql += "  order by M.docno,M.towh,D.mmcode ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        //FA0014參考
        public DataTable Report(string MAT_CLASS, string DIS_TIME_B, string DIS_TIME_E, bool P3,string APPTIME_B, string APPTIME_E)
        {
            var p = new DynamicParameters();

            var sql = @"select M.DOCNO,
                                M.TOWH WH_NO,
                                WH_NAME(M.TOWH) WH_NAME,
                                D.MMCODE,
                                (select MMNAME_C from MI_MAST where MMCODE=D.MMCODE) MMNAME_C,
                                (select MMNAME_E from MI_MAST where MMCODE=D.MMCODE) MMNAME_E,
                                (select BASE_UNIT from MI_MAST where MMCODE=D.MMCODE) BASE_UNIT,
                                D.BW_MQTY,
                                D.RV_MQTY,
                                TWN_DATE(M.APPTIME) APPTIME,
                                TWN_DATE(D.DIS_TIME) DIS_TIME
                        from ME_DOCM M left join ME_DOCD D on M.DOCNO=D.DOCNO
                        where D.BW_MQTY<>0
                          and M.MAT_CLASS = '02'";
            
            if (DIS_TIME_B != "")
            {
                sql += " AND TWN_DATE(D.DIS_TIME) >= :DIS_TIME_B ";
                p.Add(":DIS_TIME_B", string.Format("{0}", DIS_TIME_B));
            }
            if (DIS_TIME_E != "")
            {
                sql += " AND TWN_DATE(D.DIS_TIME) <= :DIS_TIME_E ";
                p.Add(":DIS_TIME_E", string.Format("{0}", DIS_TIME_E));
            }
            if (P3)
            {
                sql += " AND D.BW_MQTY<>D.RV_MQTY ";
            }
            if (APPTIME_B != string.Empty)
            {
                sql += " AND TWN_DATE(M.APPTIME) >= :APP_TIME_B ";
                p.Add(":APP_TIME_B", string.Format("{0}", APPTIME_B));
            }
            if (APPTIME_E != string.Empty)
            {
                sql += " AND TWN_DATE(M.APPTIME) <= :APP_TIME_E ";
                p.Add(":APP_TIME_E", string.Format("{0}", APPTIME_E));
            }
            sql += "  order by M.docno,M.towh,D.mmcode ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public string GetReportWH_NAME()
        {
            string sql = @"select WH_NAME(WHNO_MM1) from DUAL";
            var str = DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction);
            return str==null? "":str.ToString();
        }
    }
}