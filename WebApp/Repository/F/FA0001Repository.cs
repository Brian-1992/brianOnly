using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using TSGH.Models;
using System.Collections.Generic;

namespace WebApp.Repository.F
{

    public class FA0001_MODEL : JCLib.Mvc.BaseModel
    {
        public string SEQ { get; set; }
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string MAT_CLSNAME { get; set; }
        public string BASE_UNIT { get; set; }
        public string INID_NAME_USER { get; set; }
        public string APPDEPT { get; set; }
        public string INID { get; set; }
        public string INID_NAME { get; set; }
        public string APPTIME_YM { get; set; }
        public string M_STOREID { get; set; }
        public Int64 APVQTYN { get; set; }
        public string AVG_PRICE { get; set; }
        public string M_CONTPRICE { get; set; }
        public Double M_ALLPRICE { get; set; }
        public Int64 APP_CNT { get; set; }
        public string TOWH { get; set; }
        public string WH_NAME { get; set; }

    }
    public class FA0001Repository : JCLib.Mvc.BaseRepository
    {

        public FA0001Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<FA0001_MODEL> GetAll(string APPTIME1, string APPTIME2, string mat_class, string towh, string showopt, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select a.TOWH, (select WH_NAME from MI_WHMAST where WH_NO=a.TOWH) as WH_NAME,
                        (select INID from MI_WHMAST where WH_NO = a.TOWH) INID, 
                        (select INID_NAME from UR_INID where INID=(select INID from MI_WHMAST where WH_NO=a.TOWH)) INID_NAME,
                        b.mmcode,
                        (select mmname_c from MI_MAST where mmcode=b.mmcode) as mmname_c, 
                        (select mmname_e from MI_MAST where mmcode=b.mmcode) as mmname_e,
                        (select base_unit from MI_MAST where mmcode=b.mmcode) as base_unit, 
                        substr(TWN_DATE(case when a.MAT_CLASS = '09' then b.apvtime else b.DIS_TIME end),1,5) as APPTIME_YM,
                        SUM(b.apvqty) as APVQTYN,
                        ROUND((select AVG_PRICE from MI_WHCOST where DATA_YM=substr(TWN_DATE(case when a.MAT_CLASS = '09' then b.apvtime else b.DIS_TIME end),1,5) and MMCODE=b.mmcode),2) as avg_price, 
                        ROUND(SUM(b.apvqty)*(select AVG_PRICE from MI_WHCOST where DATA_YM=substr(TWN_DATE(case when a.MAT_CLASS = '09' then b.apvtime else b.DIS_TIME end),1,5) and MMCODE=b.mmcode),2) as M_ALLPRICE,
                        COUNT(distinct a.docno) as APP_CNT 
                        from ME_DOCM a,ME_DOCD b where a.docno=b.docno ";

            if (APPTIME1 != "" && APPTIME1 != null)
            {
                sql += "AND to_char(case when a.MAT_CLASS = '09' then b.apvtime else b.DIS_TIME end,'yyyy-mm') >= Substr(:d0, 1, 7) AND SUBSTR(TWN_DATE(case when a.MAT_CLASS = '09' then b.apvtime else b.DIS_TIME end),1,5) is NOT NULL ";
                p.Add(":d0", APPTIME1);
            }
            if (APPTIME2 != "" && APPTIME2 != null)
            {
                sql += "AND to_char(case when a.MAT_CLASS = '09' then b.apvtime else b.DIS_TIME end,'yyyy-mm') <= Substr(:d1, 1, 7) ";
                p.Add(":d1", APPTIME2);
            }
            if (showopt != "")  //庫備,非庫備
            {
                sql += "AND (select m_storeid from MI_MAST where mmcode=b.mmcode) = :M_STOREID ";
                p.Add(":M_STOREID", string.Format("{0}", showopt));
            }
            if (mat_class != "" && mat_class != null)  //物料分類
            {
                string[] tmp = mat_class.Split(',');
                sql += "AND a.MAT_CLASS IN :MAT_CLASS ";
                p.Add(":MAT_CLASS", tmp);
            }
            if (towh != "" && towh != null)  //入庫庫房
            {
                string[] tmp = towh.Split(',');
                sql += "AND A.TOWH IN :TOWH ";
                p.Add(":TOWH", tmp);
            }
            sql += "   and flowid in ('3','4','5','6', '51')";

            sql += @"group by a.TOWH,b.mmcode,SUBSTR(TWN_DATE(case when a.MAT_CLASS = '09' then b.apvtime else b.DIS_TIME end),1,5)
                     ORDER BY A.TOWH , B.MMCODE,SUBSTR(TWN_DATE(case when a.MAT_CLASS = '09' then b.apvtime else b.DIS_TIME end),1,5)";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<FA0001_MODEL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetTowhCombo()
        {
            string sql = @" SELECT a.WH_NO AS VALUE, a.WH_NAME AS TEXT, 
                        RTrim(a.WH_NO || ' ' || a.WH_NAME) AS COMBITEM 
                        FROM MI_WHMAST a 
                        WHERE a.WH_KIND = '1'
                        and a.WH_GRADE = '2'
                        and (select 1 from UR_INID where INID=a.INID and INID_FLAG in ('A', 'B', 'C')) = 1
                        order by a.WH_NO ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<ComboModel> GetMatClass()
        {
            string sql = @"SELECT DISTINCT MAT_CLASS as VALUE, " +
                " MAT_CLASS || ' ' || MAT_CLSNAME as COMBITEM from MI_MATCLASS " +
                " where MAT_CLSID in ('2', '3', '6') " +
                " order by mat_class";
            return DBWork.Connection.Query<ComboModel>(sql, DBWork.Transaction);
        }
        public string GetMatClassName(string matid)
        {
            string sql = @"SELECT MAT_CLSNAME from MI_MATCLASS WHERE MAT_CLASS = :MAT_CLASS";
            if(DBWork.Connection.ExecuteScalar(sql, new { MAT_CLASS = matid }, DBWork.Transaction) == null)
            {
                return " ";
            }
            else
            {
                return DBWork.Connection.ExecuteScalar(sql, new { MAT_CLASS = matid }, DBWork.Transaction).ToString();
            }
        }

        public IEnumerable<FA0001_MODEL> GetPrintData(string userId, string APPTIME1, string APPTIME2, string mat_class, string towh, string showopt)
        {
            //轉成需求格式 EX:10804
            string tmpTIME1 = "", tmpTIME2 = "";

            if (APPTIME1 != "" && APPTIME1 != null)
            {
                tmpTIME1 = Convert.ToDateTime(APPTIME1.Substring(4, 11)).ToString("yyyyMM");

            }

            if (APPTIME2 != "" && APPTIME2 != null)
            {
                tmpTIME2 = Convert.ToDateTime(APPTIME2.Substring(4, 11)).ToString("yyyyMM");
            }



            var p = new DynamicParameters();

            var sql = @"select INID_NAME(USER_INID(:INID)) INID_NAME_USER, a.TOWH as APPDEPT, (select WH_NAME from MI_WHMAST where WH_NO=a.TOWH) as INID_NAME,b.mmcode,(select mmname_c from MI_MAST where mmcode=b.mmcode) as mmname_c, 
                        (select mmname_e from MI_MAST where mmcode=b.mmcode) as mmname_e,(select base_unit from MI_MAST where mmcode=b.mmcode) as base_unit, 
                        substr(TWN_DATE(case when a.MAT_CLASS = '09' then b.apvtime else b.DIS_TIME end),1,5) as APPTIME_YM,SUM(b.apvqty) as APVQTYN,ROUND((select AVG_PRICE from MI_WHCOST where DATA_YM=substr(TWN_DATE(case when a.MAT_CLASS = '09' then b.apvtime else b.DIS_TIME end),1,5) and MMCODE=b.mmcode),2) as avg_price, 
                        ROUND(SUM(b.apvqty)*(select AVG_PRICE from MI_WHCOST where DATA_YM=substr(TWN_DATE(case when a.MAT_CLASS = '09' then b.apvtime else b.DIS_TIME end),1,5) and MMCODE=b.mmcode),2) as M_ALLPRICE,COUNT(distinct a.docno) as APP_CNT 
                        from ME_DOCM a,ME_DOCD b where a.docno=b.docno  ";

            p.Add(":INID", userId);
            if (tmpTIME1 != "" && tmpTIME1 != null)
            {
                sql += "AND to_char(case when a.MAT_CLASS = '09' then b.apvtime else b.DIS_TIME end,'yyyymm') >= Substr(:d0, 1, 6) AND SUBSTR(TWN_DATE(case when a.MAT_CLASS = '09' then b.apvtime else b.DIS_TIME end),1,5) is NOT NULL ";
                p.Add(":d0", tmpTIME1);
            }
            if (tmpTIME2 != "" && tmpTIME2 != null)
            {
                sql += "AND to_char(case when a.MAT_CLASS = '09' then b.apvtime else b.DIS_TIME end,'yyyymm') <= Substr(:d1, 1, 6) ";
                p.Add(":d1", tmpTIME2);
            }

            if (showopt != "")  //庫備,非庫備
            {
                sql += "AND (select m_storeid from MI_MAST where mmcode=b.mmcode) = :M_STOREID ";
                p.Add(":M_STOREID", string.Format("{0}", showopt));
            }
            if (mat_class != "" && mat_class != null)  //物料分類
            {
                string[] tmp = mat_class.Split(',');
                sql += "AND a.MAT_CLASS IN :MAT_CLASS ";
                p.Add(":MAT_CLASS", tmp);
            }
            if (towh != "" && towh != null)  //入庫庫房
            {
                string[] tmp = towh.Split(',');
                sql += "AND A.TOWH IN :TOWH ";
                p.Add(":TOWH", tmp);
            }
            sql += "   and a.flowid in ('3','4','5','6', '51')";

            sql += @"group by a.TOWH,b.mmcode,SUBSTR(TWN_DATE(case when a.MAT_CLASS = '09' then b.apvtime else b.DIS_TIME end),1,5)
                     ORDER BY A.TOWH , B.MMCODE,SUBSTR(TWN_DATE(case when a.MAT_CLASS = '09' then b.apvtime else b.DIS_TIME end),1,5)";


            return DBWork.Connection.Query<FA0001_MODEL>(sql, p, DBWork.Transaction);
        }
    }
}