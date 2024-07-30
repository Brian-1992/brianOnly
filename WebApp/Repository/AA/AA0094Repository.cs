using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using System.Linq;
using TSGH.Models;

namespace WebApp.Repository.AA
{

    public class AA0094Report_MODEL : JCLib.Mvc.BaseModel
    {
        public string WH_NO { get; set; }
        public string MMCODE { get; set; }
        public string MMNAME_E { get; set; }
        public string BASE_UNIT { get; set; }
        public string AVG_PRICE { get; set; }
        public string APL_OUTQTY { get; set; }
        public string OUT_AMT { get; set; }
        public string RATIO { get; set; }
        public string OUT_AMT1 { get; set; }
       

    }

    public class AA0094Repository : JCLib.Mvc.BaseRepository
    {
        public AA0094Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }


        public IEnumerable<ComboModel> GetDeptCombo(string tuser)
        {
            string sql = @"SELECT A.WH_NO AS VALUE, A.WH_NAME AS TEXT,RTrim(A.WH_NO || ' ' || A.WH_NAME) AS COMBITEM  FROM MI_WHMAST A WHERE WH_KIND = '0'AND WH_GRADE IN ('1','2') AND EXISTS ( SELECT 1 FROM MI_WHID B WHERE A.WH_NO = B.WH_NO AND B.WH_USERID =  :tuser)";

            return DBWork.Connection.Query<ComboModel>(sql, new { TUSER = tuser }, DBWork.Transaction);
        }
        public string WNAME(string mid)
        {
            string rtnmname = "";
            string sql = @"SELECT WH_NAME  FROM MI_WHMAST where WH_NO=:mid";


            rtnmname = DBWork.Connection.QueryFirst<string>(sql, new { mid }, DBWork.Transaction);

            return rtnmname;
        }

        public IEnumerable<PH_AIRHIS> GetAll(string bgdate, string endate, string dept, string status, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = "SELECT TXTDAY, AGEN_NO|| ' ' || (SELECT NVL(AGEN_NAMEC, '') FROM PH_VENDER WHERE AGEN_NO = PH_AIRHIS.AGEN_NO) AGEN_NO,MMCODE,FBNO,AIR,XSIZE,DEPT || ' ' || (SELECT NVL(WH_NAME, '') FROM MI_WHMAST WHERE WH_NO = DEPT) DEPT,SBNO,DOCNO,STATUS from PH_AIRHIS WHERE 1 = 1 ";


            if (bgdate != "null" && bgdate != "")
            {
                sql += " AND to_char(TXTDAY,'yyyy-mm-dd') >= Substr(:p1, 1, 10) ";
                p.Add(":p1", bgdate);
            }
            if (endate != "null" && endate != "")
            {
                sql += " AND to_char(TXTDAY,'yyyy-mm-dd') <= Substr(:p2, 1, 10) ";
                p.Add(":p2", endate);
            }

            if (dept != "" && dept != "null")
            {
                sql += " AND DEPT  = :p3 ";
                p.Add(":p3", dept);
            }

            if (status != "" && status != "null")
            {
                sql += " AND STATUS = :p4 ";
                p.Add(":p4", status);
            }

          

            sql += " order by  MMCODE ";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<PH_AIRHIS>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<AA0094Report_MODEL> GetReport(string P0, string P1, string P2,string P4)
        {
            var p = new DynamicParameters();
            string sql = @"SELECT A.WH_NO,A.MMCODE,B.MMNAME_E,B.BASE_UNIT,C.AVG_PRICE,SUM(A.APL_OUTQTY) APL_OUTQTY,SUM(A.APL_OUTQTY * C.AVG_PRICE) OUT_AMT,SUM(A.INV_QTY) SQTY,SUM(A.INV_QTY * C.AVG_PRICE) OUT_AMT, 
                           CASE WHEN SUM(A.APL_OUTQTY) = 0 THEN NULL ELSE ROUND(SUM(A.APL_INQTY) / SUM(A.APL_OUTQTY),0) END  RATIO FROM MI_WINVMON A, MI_MAST B, MI_WHCOST C
                           WHERE 1=1 AND A.MMCODE = B.MMCODE AND A.DATA_YM = C.DATA_YM AND A.MMCODE = C.MMCODE  ";
            if (P0 != "" && P0 != null)
            {
                sql += " AND A.DATA_YM BETWEEN :BD AND :ED";
                p.Add(":ED", P0);
                p.Add(":BD", P4);
            }


            if (P1 != "" && P1 != null)
            {
                sql += " AND A.WH_NO  = :WHNO";
                p.Add(":WHNO", P1);
            }

            if (P2 != "" && P2!= null)
            {
                sql += " AND A.APL_OUTQTY <= :OUTQTY";
                p.Add(":OUTQTY", P2);
            }
           

            sql += " GROUP BY A.WH_NO,A.MMCODE,B.MMNAME_E,B.BASE_UNIT,C.AVG_PRICE ";
            return DBWork.Connection.Query<AA0094Report_MODEL>(sql, p, DBWork.Transaction);
        }

    }
}