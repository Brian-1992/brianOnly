using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using TSGH.Models;

namespace WebApp.Repository.AB
{
    public class AB0082_MODEL : JCLib.Mvc.BaseModel
    {
        public string SEQ { get; set; }
        public string NRCODE { get; set; }
        public string NRNAME { get; set; }

        public string ORDERCODE { get; set; }
        public string MMNAME_E { get; set; }
        public string NEEDBACKQTY { get; set; }
        public string BACKQTY { get; set; }
        public string DIFFQTY { get; set; }
        public string INSUAMOUNT1 { get; set; }
        public string PAYAMOUNT1 { get; set; }
        public string RETURNSTOCKCODE { get; set; }
        public string CHARTNO { get; set; }
        public string CHINNAME { get; set; }
        public string BEDNO { get; set; }
        public string BEGINDATETIME { get; set; }
        public string ENDDATETIME { get; set; }
        public string DOSE { get; set; }
        public string FREQNO { get; set; }
        public string PHRBACKREASON_NAME { get; set; }
        public string CREATEDATETIME { get; set; }

    }

    public class AB0082Repository : JCLib.Mvc.BaseRepository
    {
        public AB0082Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<MI_WINVCTL> GetAll(string wh_no, string mmcode, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT * FROM MI_WINVCTL WHERE 1=1 ";

            if (wh_no != "")
            {
                sql += " AND WH_NO LIKE :p0 ";
                p.Add(":p0", string.Format("%{0}%", wh_no));
            }
            if (mmcode != "")
            {
                sql += " AND MMCODE LIKE :p1 ";
                p.Add(":p1", string.Format("%{0}%", mmcode));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_WINVCTL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<AB0082_MODEL> GetPrintData(string NRCODE1, string NRCODE2, string P0, string P1, string P2, string P3, string WH_NO,string finalcreatedatetime1,string finalcreatedatetime2, string usedatetime1,string usedatetime2)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT  distinct NRCODE, "+
                "WH_NAME(NRCODE) NRNAME,  " +
                "ORDERCODE, b.MMNAME_E, SUM(NEEDBACKQTY) NEEDBACKQTY,SUM(BACKQTY) BACKQTY,SUM(BACKQTY -NEEDBACKQTY) DIFFQTY, INSUAMOUNT1, PAYAMOUNT1 ";
            sql += "from ME_BACK a JOIN  MI_MAST b ON(a.ORDERCODE = b.MMCODE) where 1=1 ";

            //查詢日期
            if (usedatetime1 != "" && usedatetime1 != null)
            {
                sql += "AND a.USEDATETIME >= :USEDATETIME1 AND a.USEDATETIME is NOT NULL ";
                p.Add(":USEDATETIME1", usedatetime1);
            }
            if (usedatetime2 != "" && usedatetime2 != null)
            {
                sql += "AND a.USEDATETIME <= :USEDATETIME2 ";
                p.Add(":USEDATETIME2", usedatetime2);
            }
            //退藥日期
            if (finalcreatedatetime1 != "" && finalcreatedatetime1 != null)
            {
                sql += "AND a.CREATEDATETIME >= :CREATEDATETIME1 AND a.CREATEDATETIME is NOT NULL ";
                p.Add(":CREATEDATETIME1", finalcreatedatetime1);
            }
            if (finalcreatedatetime2 != "" && finalcreatedatetime2 != null)
            {
                sql += "AND a.CREATEDATETIME <= :CREATEDATETIME2 ";
                p.Add(":CREATEDATETIME2", finalcreatedatetime2);
            }

            //病房
            if (NRCODE1 != "" && NRCODE1 != null)
            {
                sql += "AND NRCODE >= :NRCODE1 AND NRCODE is NOT NULL ";
                p.Add(":NRCODE1", NRCODE1);
            }
            if (NRCODE2 != "" && NRCODE2 != null)
            {
                sql += "AND NRCODE <= :NRCODE2 ";
                p.Add(":NRCODE2", NRCODE2);
            }

            //病例號
            if (P0 != "" && P0 != null)
            {
                sql += "AND a.MEDNO >= :MEDNO1 AND a.MEDNO is NOT NULL ";
                p.Add(":MEDNO1", P0);
            }
            if (P1 != "" && P1 != null)
            {
                sql += "AND a.MEDNO <= :MEDNO2 ";
                p.Add(":MEDNO2", P1);
            }

            //病床號
            if (P2 != "" && P2 != null)
            {
                sql += "AND a.BEDNO >= :BEDNO1 AND a.BEDNO is NOT NULL ";
                p.Add(":BEDNO1", P2);
            }
            if (P3 != "" && P3 != null)
            {
                sql += "AND a.BEDNO <= :BEDNO2 ";
                p.Add(":BEDNO2", P3);
            }

            //WH_NO
            if (WH_NO != "" && WH_NO != null)
            {
                sql += "AND a.RETURNSTOCKCODE = :RETURNSTOCKCODE ";
                p.Add(":RETURNSTOCKCODE", WH_NO);
            }

            sql += "group by a.ORDERCODE, b.MMNAME_E,INSUAMOUNT1,PAYAMOUNT1,NRCODE ";
            sql += "ORDER BY ORDERCODE";


            return DBWork.Connection.Query<AB0082_MODEL>(sql, p, DBWork.Transaction);
        }

        //病患清單
        public IEnumerable<AB0082_MODEL> GetPrintListData(string NRCODE1, string NRCODE2, string P0, string P1, string P2, string P3, string WH_NO, string finalcreatedatetime1, string finalcreatedatetime2, string usedatetime1, string usedatetime2)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT  distinct NRCODE, 
                      WH_NAME(NRCODE) NRNAME,  
                      CHARTNO,CHINNAME,BEDNO, 
                      ORDERCODE, b.MMNAME_E, BEGINDATETIME, ENDDATETIME,DOSE, FREQNO, NEEDBACKQTY, BACKQTY,(BACKQTY -NEEDBACKQTY) DIFFQTY,PHRBACKREASON_NAME,CREATEDATETIME 
                      from ME_BACK a JOIN  MI_MAST b ON(a.ORDERCODE = b.MMCODE) where 1=1  ";

            //查詢日期
            if (usedatetime1 != "" && usedatetime1 != null)
            {
                sql += "AND a.USEDATETIME >= :USEDATETIME1 AND a.USEDATETIME is NOT NULL ";
                p.Add(":USEDATETIME1", usedatetime1);
            }
            if (usedatetime2 != "" && usedatetime2 != null)
            {
                sql += "AND a.USEDATETIME <= :USEDATETIME2 ";
                p.Add(":USEDATETIME2", usedatetime2);
            }
            //退藥日期
            if (finalcreatedatetime1 != "" && finalcreatedatetime1 != null)
            {
                sql += "AND a.CREATEDATETIME >= :CREATEDATETIME1 AND a.CREATEDATETIME is NOT NULL ";
                p.Add(":CREATEDATETIME1", finalcreatedatetime1);
            }
            if (finalcreatedatetime2 != "" && finalcreatedatetime2 != null)
            {
                sql += "AND a.CREATEDATETIME <= :CREATEDATETIME2 ";
                p.Add(":CREATEDATETIME2", finalcreatedatetime2);
            }

            //病房
            if (NRCODE1 != "" && NRCODE1 != null)
            {
                sql += "AND NRCODE >= :NRCODE1 AND NRCODE is NOT NULL ";
                p.Add(":NRCODE1", NRCODE1);
            }
            if (NRCODE2 != "" && NRCODE2 != null)
            {
                sql += "AND NRCODE <= :NRCODE2 ";
                p.Add(":NRCODE2", NRCODE2);
            }

            //病例號
            if (P0 != "" && P0 != null)
            {
                sql += "AND a.MEDNO >= :MEDNO1 AND a.MEDNO is NOT NULL ";
                p.Add(":MEDNO1", P0);
            }
            if (P1 != "" && P1 != null)
            {
                sql += "AND a.MEDNO <= :MEDNO2 ";
                p.Add(":MEDNO2", P1);
            }

            //病床號
            if (P2 != "" && P2 != null)
            {
                sql += "AND a.BEDNO >= :BEDNO1 AND a.BEDNO is NOT NULL ";
                p.Add(":BEDNO1", P2);
            }
            if (P3 != "" && P3 != null)
            {
                sql += "AND a.BEDNO <= :BEDNO2 ";
                p.Add(":BEDNO2", P3);
            }

            //WH_NO
            if (WH_NO != "" && WH_NO != null)
            {
                sql += "AND a.RETURNSTOCKCODE = :RETURNSTOCKCODE ";
                p.Add(":RETURNSTOCKCODE", WH_NO);
            }

            sql += "ORDER BY CHARTNO";


            return DBWork.Connection.Query<AB0082_MODEL>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<MI_WINVCTL> Get(string wh_no,string mmcode)
        {
            var sql = @"SELECT * FROM MI_WINVCTL WHERE WH_NO = :WH_NO and MMCODE = :MMCODE";
            return DBWork.Connection.Query<MI_WINVCTL>(sql, new { WH_NO = wh_no, MMCODE = mmcode }, DBWork.Transaction);
        }

        public IEnumerable<MI_WHMAST> NRCODEGet()
        {
            string sql = @"select distinct a.NRCODE WH_NO,WH_NAME(a.NRCODE) WH_NAME from ME_BACK a where 1 = 1 order by 1 ";
            return DBWork.Connection.Query<MI_WHMAST>(sql, DBWork.Transaction);
        }
        public IEnumerable<MI_WHMAST> WHNOGet()
        {
            string sql = @"SELECT * FROM MI_WHMAST WHERE WH_KIND = :WH_KIND AND WH_GRADE = :WH_GRADE order by 1 ";
            return DBWork.Connection.Query<MI_WHMAST>(sql,new { WH_KIND='0', WH_GRADE  = '2'}, DBWork.Transaction);
        }
        public bool CheckExists(string WH_NO,string MMCODE)
        {
            string sql = @"SELECT 1 FROM MI_WINVCTL WHERE WH_NO=:WH_NO and MMCODE=:MMCODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { WH_NO = WH_NO, MMCODE = MMCODE }, DBWork.Transaction) == null);
        }
    }

}