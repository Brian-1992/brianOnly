using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.BE
{
    public class BH0005Repository : JCLib.Mvc.BaseRepository
    {
        public BH0005Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<PH_AIRST> GetNow(string agen_no, string fbno, string namec, string txtday_b, string txtday_e, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT TXTDAY, AGEN_NO, NAMEC, FBNO, SEQ, XSIZE
                        FROM PH_AIRST
                        where 1=1 
                        ";

            if (namec != "")
            {
                sql += " and NAMEC  like :p0 ";
                p.Add(":p0", string.Format("%{0}%", namec));
            }
            if (agen_no != "")
            {
                sql += " AND AGEN_NO LIKE :p1 ";
                p.Add(":p1", string.Format("{0}%", agen_no));
            }
            if (fbno != "")
            {
                sql += " AND FBNO  LIKE :p2 ";
                p.Add(":p2", string.Format("{0}%", fbno));
            }
            if (txtday_b != "")
            {
                sql += " AND TWN_DATE(TXTDAY) >= :p3 ";
                p.Add(":p3", string.Format("{0}", txtday_b));
            }
            if (txtday_e != "")
            {
                sql += " AND TWN_DATE(TXTDAY) <= :p4 ";
                p.Add(":p4", string.Format("{0}", txtday_e));
            }
            sql += "  Order by AGEN_NO, TXTDAY,FBNO  ";
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<PH_AIRST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<PH_AIRHIS> GetHis(string agen_no, string fbno, string namec, string txtday_b, string txtday_e, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT TXTDAY, AGEN_NO, NAMEC, FBNO,  SEQ, EXTYPE, XSIZE, STATUS
                        FROM PH_AIRHIS 
                        where 1=1  
                         ";


            if (namec != "")
            {
                sql += " AND NAMEC  LIKE :p0 ";
                p.Add(":p0", string.Format("%{0}%", namec));
            }
            if (agen_no != "")
            {
                sql += " AND AGEN_NO LIKE :p1 ";
                p.Add(":p1", string.Format("{0}%", agen_no));
            }
            if (fbno != "")
            {
                sql += " AND FBNO  LIKE :p2 ";
                p.Add(":p2", string.Format("{0}%", fbno));
            }
            if (txtday_b != "")
            {
                sql += " AND TWN_DATE(TXTDAY) >= :p3 ";
                p.Add(":p3", string.Format("{0}", txtday_b));
            }
            if (txtday_e != "")
            {
                sql += " AND TWN_DATE(TXTDAY) <= :p4 ";
                p.Add(":p4", string.Format("{0}", txtday_e));
            }
            sql += " Order by AGEN_NO,TXTDAY,FBNO ";
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<PH_AIRHIS>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public string GetDataTime()
        {
            var sql = @"select TO_CHAR(TO_NUMBER(TO_CHAR(Update_TIME,'YYYYMMDDHH24MI'))-191100000000) from PH_AIRTIME";

            var str = DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction);

            return str==null? "":str.ToString();

        }

        public DataTable GetExcel_Now(string agen_no, string fbno, string namec, string txtday_b, string txtday_e)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT TWN_DATE(TXTDAY) 更換日期, AGEN_NO 廠商碼, NAMEC 品名, FBNO 瓶號,  XSIZE 尺寸
                        FROM PH_AIRST
                        where 1=1 
                        ";

            if (namec != "")
            {
                sql += " and NAMEC  like :p0 ";
                p.Add(":p0", string.Format("%{0}%", namec));
            }
            if (agen_no != "")
            {
                sql += " AND AGEN_NO LIKE :p1 ";
                p.Add(":p1", string.Format("{0}%", agen_no));
            }
            if (fbno != "")
            {
                sql += " AND FBNO  LIKE :p2 ";
                p.Add(":p2", string.Format("{0}%", fbno));
            }
            if (txtday_b != "")
            {
                sql += " AND TWN_DATE(TXTDAY) >= :p3 ";
                p.Add(":p3", string.Format("{0}", txtday_b));
            }
            if (txtday_e != "")
            {
                sql += " AND TWN_DATE(TXTDAY) <= :p4 ";
                p.Add(":p4", string.Format("{0}", txtday_e));
            }
            sql += "  Order by AGEN_NO, TXTDAY,FBNO  ";


            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public DataTable GetExcel_His(string agen_no, string fbno, string namec, string txtday_b, string txtday_e)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT TWN_DATE(TXTDAY) 更換日期,EXTYPE 更換類別, AGEN_NO 廠商碼, NAMEC 品名, FBNO 瓶號,  XSIZE 尺寸
                        FROM PH_AIRHIS 
                        where 1=1  
                         ";


            if (namec != "")
            {
                sql += " AND NAMEC  LIKE :p0 ";
                p.Add(":p0", string.Format("%{0}%", namec));
            }
            if (agen_no != "")
            {
                sql += " AND AGEN_NO LIKE :p1 ";
                p.Add(":p1", string.Format("{0}%", agen_no));
            }
            if (fbno != "")
            {
                sql += " AND FBNO  LIKE :p2 ";
                p.Add(":p2", string.Format("{0}%", fbno));
            }
            if (txtday_b != "")
            {
                sql += " AND TWN_DATE(TXTDAY) >= :p3 ";
                p.Add(":p3", string.Format("{0}", txtday_b));
            }
            if (txtday_e != "")
            {
                sql += " AND TWN_DATE(TXTDAY) <= :p4 ";
                p.Add(":p4", string.Format("{0}", txtday_e));
            }
            sql += " Order by AGEN_NO,TXTDAY,FBNO ";


            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

    }
}