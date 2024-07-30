using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JCLib.DB;
using WebApp.Models;
using Dapper;
using System.Data;

namespace WebApp.Repository.AB
{
    public class AB0084_MODEL : JCLib.Mvc.BaseModel
    {
        public string SEQ { get; set; }
        public string D { get; set; }
        public string T { get; set; }
        public string NRCode { get; set; }
        public string BedNo { get; set; }
        public string ChartNo { get; set; }
        public string ChinName { get; set; }
        public string OrderCode { get; set; }
        public string OrderEngName { get; set; }
        public string Qty { get; set; }
        public string MEMO { get; set; }
        public string DRNAME { get; set; }
        public string SectionNo { get; set; }
        public string PCANAME { get; set; }


    }
    public class AB0084Repository : JCLib.Mvc.BaseRepository
    {
        public AB0084Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        // GET api/<controller>
        public IEnumerable<AB0084_MODEL> GetReport(string P0, string finalcreatedatetime1, string finalcreatedatetime2)
        {
            var p = new DynamicParameters();
            string sql = @"SELECT  ROWNUM as SEQ, SUBSTR(BeginDateTime,1,7) as d,SUBSTR(BeginDateTime,8,4) as t,NRCode,BedNo,ChartNo,ChinName,OrderCode,OrderEngName,SUMQty || StockUnit as Qty,MEMO,
                           DRNAME,SectionNo,PCANAME FROM ME_AB0084 where 1=1 ";
            if (P0 != "" && P0 != null)
            {
                sql += "  AND ChartNo is NOT NULL AND ChartNo = :ChartNo1";
                p.Add(":ChartNo1", P0);
            }

            if (finalcreatedatetime1 != "" && finalcreatedatetime1 != null)
            {
                sql += "  AND BeginDateTime is NOT NULL AND BeginDateTime >= :BeginDateTime1";
                p.Add(":BeginDateTime1", finalcreatedatetime1);
            }
            if (finalcreatedatetime2 != "" && finalcreatedatetime2 != null)
            {
                sql += " AND BeginDateTime <= :BeginDateTime2 ";
                p.Add(":BeginDateTime2", finalcreatedatetime2);
            }

           
            //UNION ALL
            //select null,null,null,null,null,null,null,null,null from UR_MENU where rownum <=10-
            //(select count(*) from PH_SMALL_D where DN = :DN)"; 填入空白資料補到10筆

            //sql += " ORDER BY mmcode ";

            return DBWork.Connection.Query<AB0084_MODEL>(sql,p, DBWork.Transaction);
        }

        public IEnumerable<AB0084_MODEL> GetReportSum(string P0, string finalcreatedatetime1, string finalcreatedatetime2)
        {
            var p = new DynamicParameters();
            string sql = @"SELECT OrderCode, OrderEngName,SUM(SUMQty) as Qty FROM ME_AB0084 where 1=1 ";
            if (P0 != "" && P0 != null)
            {
                sql += "  AND ChartNo is NOT NULL AND ChartNo = :ChartNo1";
                p.Add(":ChartNo1", P0);
            }

            if (finalcreatedatetime1 != "" && finalcreatedatetime1 != null)
            {
                sql += "  AND BeginDateTime is NOT NULL AND BeginDateTime >= :BeginDateTime1";
                p.Add(":BeginDateTime1", finalcreatedatetime1);
            }
            if (finalcreatedatetime2 != "" && finalcreatedatetime2 != null)
            {
                sql += " AND BeginDateTime <= :BeginDateTime2 ";
                p.Add(":BeginDateTime2", finalcreatedatetime2);
            }


            //UNION ALL
            //select null,null,null,null,null,null,null,null,null from UR_MENU where rownum <=10-
            //(select count(*) from PH_SMALL_D where DN = :DN)"; 填入空白資料補到10筆

            sql += " GROUP BY OrderCode, OrderEngName ";

            return DBWork.Connection.Query<AB0084_MODEL>(sql, p, DBWork.Transaction);
        }
    }
}