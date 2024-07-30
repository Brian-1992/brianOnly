using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using TSGH.Models;
using System.Collections.Generic;

namespace WebApp.Repository.F
{

    public class FA0011_MODEL : JCLib.Mvc.BaseModel
    {
        public string SEQ { get; set; }
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string MAT_CLSNAME { get; set; }
        public string BASE_UNIT { get; set; }
        public string INID_NAME_USER { get; set; }
        public string APPDEPT { get; set; }
        public string INID_NAME { get; set; }
        public string APPTIME_YM { get; set; }
        public string M_STOREID { get; set; }
        public Int64 APVQTYN { get; set; }
        public string AVG_PRICE { get; set; }
        public string M_CONTPRICE { get; set; }
        public Int64 M_ALLPRICE { get; set; }
    }
    public class FA0011Repository : JCLib.Mvc.BaseRepository
    {

        public FA0011Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }


        //查詢,匯出,列印 參考AA0061
        public IEnumerable<FA0001_MODEL> GetAll(string userId, string APPTIME1, string APPTIME2, string task_id, string mmcode, string showopt, string showdata, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"select b.mmcode, 
                       (select mmname_c from MI_MAST where mmcode=b.mmcode) as mmname_c, 
                       (select mmname_e from MI_MAST where mmcode=b.mmcode) as mmname_e, 
                       (select base_unit from MI_MAST where mmcode=b.mmcode) as base_unit, 
                       INID_NAME(USER_INID(:INID)) INID_NAME_USER, 
                       a.TOWH,(select WH_NAME from MI_WHMAST where WH_NO = a.TOWH) as WH_NAME, ";
            if (showdata == "1") //明細
            {
                sql += "substr(TWN_DATE(case when a.MAT_CLASS = '09' then b.apvtime else b.dis_time end),1,5) APPTIME_YM, ";
            }

            sql += "SUM(b.apvqty) as APVQTYN, ";
            if (showdata == "1") //明細
            {
                sql += "(select AVG_PRICE from MI_WHCOST where DATA_YM=substr(TWN_DATE(case when a.MAT_CLASS = '09' then b.apvtime else b.dis_time end),1,5) and MMCODE=b.mmcode) as avg_price, ";
            }

            sql += @"(select m_contprice from MI_MAST where mmcode=b.mmcode) as M_CONTPRICE, 
                       SUM(b.apvqty)*(select m_contprice from MI_MAST where mmcode=b.mmcode) as M_ALLPRICE from ME_DOCM a,ME_DOCD b 
                       where a.docno=b.docno ";

            p.Add(":INID", userId);

            if (APPTIME1 != "" && APPTIME1 != null)
            {
                sql += "AND to_char(case when a.MAT_CLASS = '09' then b.apvtime else b.dis_time end,'yyyy-mm') >= Substr(:d0, 1, 7) AND SUBSTR(TWN_DATE(case when a.MAT_CLASS = '09' then b.apvtime else b.dis_time end),1,5) is NOT NULL ";
                p.Add(":d0", APPTIME1);
            }
            if (APPTIME2 != "" && APPTIME2 != null)
            {
                sql += "AND to_char(case when a.MAT_CLASS = '09' then b.apvtime else b.dis_time end,'yyyy-mm') <= Substr(:d1, 1, 7) ";
                p.Add(":d1", APPTIME2);
            }

            if (showopt != "")  //庫備,非庫備
            {
                sql += "and (select m_storeid from MI_MAST where mmcode=b.mmcode)=:M_STOREID ";
                p.Add(":M_STOREID", showopt);
            }
            if (task_id != "" && task_id != null)  //物料分類
            {
                string[] tmp = task_id.Split(',');
                sql += "AND A.MAT_CLASS IN :MAT_CLASS ";
                p.Add(":MAT_CLASS", tmp);
            }
            if (mmcode != "")    //院內碼
            {
                sql += "AND B.MMCODE LIKE :p1 ";
                p.Add(":p1", string.Format("{0}", mmcode));
            }

            sql += "   and flowid in ('3','4','5','6', '51')";

            if (showdata == "1") //明細
            {
                sql += @"group by b.mmcode,a.TOWH,substr(TWN_DATE(case when a.MAT_CLASS = '09' then b.apvtime else b.dis_time end),1,5) 
                     order by b.mmcode,a.TOWH,substr(TWN_DATE(case when a.MAT_CLASS = '09' then b.apvtime else b.dis_time end), 1, 5)";
            }else if(showdata == "2") //彙整
            {
                sql += @"group by b.mmcode,a.TOWH 
                         order by b.mmcode,a.TOWH";
            }



            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<FA0001_MODEL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<FA0011_MODEL> GetPrintData(string userId, string APPTIME1, string APPTIME2, string task_id, string mmcode, string showopt, string showdata)
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

            var sql = @"select b.mmcode, 
                       (select mmname_c from MI_MAST where mmcode=b.mmcode) as mmname_c, 
                       (select mmname_e from MI_MAST where mmcode=b.mmcode) as mmname_e, 
                       (select base_unit from MI_MAST where mmcode=b.mmcode) as base_unit, 
                       INID_NAME(USER_INID(:INID)) INID_NAME_USER, 
                       a.TOWH as appdept,(select WH_NAME from MI_WHMAST where WH_NO = a.TOWH) as INID_NAME, ";
            if (showdata == "1") //明細
            {
                sql += "substr(TWN_DATE(case when a.MAT_CLASS = '09' then b.apvtime else b.dis_time end),1,5) APPTIME_YM, ";
            }

            sql += "SUM(b.apvqty) as APVQTYN, ";
            if (showdata == "1") //明細
            {
                sql += "(select AVG_PRICE from MI_WHCOST where DATA_YM=substr(TWN_DATE(case when a.MAT_CLASS = '09' then b.apvtime else b.dis_time end),1,5) and MMCODE=b.mmcode) as avg_price, ";
            }

            sql += @"(select m_contprice from MI_MAST where mmcode=b.mmcode) as M_CONTPRICE, 
                       SUM(b.apvqty)*(select m_contprice from MI_MAST where mmcode=b.mmcode) as M_ALLPRICE from ME_DOCM a,ME_DOCD b 
                       where a.docno=b.docno ";

            p.Add(":INID", userId);
            if (tmpTIME1 != "" && tmpTIME1 != null)
            {
                sql += "AND to_char(case when a.MAT_CLASS = '09' then b.apvtime else b.dis_time end,'yyyymm') >= Substr(:d0, 1, 6) AND SUBSTR(TWN_DATE(case when a.MAT_CLASS = '09' then b.apvtime else b.dis_time end),1,5) is NOT NULL ";
                p.Add(":d0", tmpTIME1);
            }
            if (tmpTIME2 != "" && tmpTIME2 != null)
            {
                sql += "AND to_char(case when a.MAT_CLASS = '09' then b.apvtime else b.dis_time end,'yyyymm') <= Substr(:d1, 1, 6) ";
                p.Add(":d1", tmpTIME2);
            }
            if (showopt != "")  //庫備,非庫備
            {
                sql += "and (select m_storeid from MI_MAST where mmcode=b.mmcode)=:M_STOREID ";
                p.Add(":M_STOREID", showopt);
            }
            if (task_id != "" && task_id != null)  //物料分類
            {
                string[] tmp = task_id.Split(',');
                sql += "AND A.MAT_CLASS IN :MAT_CLASS ";
                p.Add(":MAT_CLASS", tmp);
            }
            if (mmcode != "")    //院內碼
            {
                sql += "AND B.MMCODE LIKE :p1 ";
                p.Add(":p1", string.Format("{0}", mmcode));
            }
            sql += "   and flowid in ('3','4','5','6', '51')";
            if (showdata == "1") //明細
            {
                sql += @"group by b.mmcode,a.TOWH,substr(TWN_DATE(case when a.MAT_CLASS = '09' then b.apvtime else b.dis_time end),1,5) 
                     order by b.mmcode,a.TOWH,substr(TWN_DATE(case when a.MAT_CLASS = '09' then b.apvtime else b.dis_time end), 1, 5)";
            }
            else if (showdata == "2") //彙整
            {
                sql += @"group by b.mmcode,a.TOWH 
                         order by b.mmcode,a.TOWH";
            }


            return DBWork.Connection.Query<FA0011_MODEL>(sql, p, DBWork.Transaction);
        }

        public String GetMatName(string task_id)
        {
            DynamicParameters p = new DynamicParameters();
            var sql = @"select b.MAT_CLSNAME from MI_MAST a,MI_MATCLASS b where b.MAT_CLASS in :MAT_CLASS and rownum  = 1";

            string[] tmp = task_id.Split(',');
            p.Add(":MAT_CLASS", tmp);

            return DBWork.Connection.ExecuteScalar(sql, p, DBWork.Transaction).ToString();
        }



        public IEnumerable<ComboModel> GetMatClass()
        {
            string sql = @"SELECT DISTINCT MAT_CLASS as VALUE, " +
                " MAT_CLASS || ' ' || MAT_CLSNAME as COMBITEM from MI_MATCLASS " +
                " where mat_clsid in ('2', '3', '6') order by mat_class";
            return DBWork.Connection.Query<ComboModel>(sql, DBWork.Transaction);
        }
        public IEnumerable<MI_MAST> GetMMCODECombo(string mmcode, int page_index, int page_size, string sorters)
        {
            DynamicParameters p = new DynamicParameters();

            string sql = @"SELECT DISTINCT {0} MMCODE , MMNAME_C, MMNAME_E from MI_MAST A WHERE 1=1 ";


            //if (task_id != "" && task_id != null)  //物料分類
            //{
            //    string[] tmp = task_id.Split(',');
            //    sql += "AND MAT_CLASS IN :mat_class ";
            //    p.Add(":mat_class", tmp);
            //}

            //if (store_id != "" && store_id != null)  //庫備或是非庫備
            //{
            //    sql += "AND M_STOREID =:M_STOREID ";
            //    p.Add(":M_STOREID", store_id);
            //}

            if (mmcode != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,");
                p.Add(":MMCODE_I", mmcode);
                p.Add(":MMNAME_E_I", mmcode);
                p.Add(":MMNAME_C_I", mmcode);

                sql += " AND (A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}%", mmcode));

                sql += " OR MMNAME_E LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", mmcode));

                sql += " OR MMNAME_C LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", mmcode));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY MMCODE ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public String GetExcelTitle(string userId, string APPTIME1, string APPTIME2, string showdata)
        {
            //轉成需求格式 EX:10804 ; tmpTIME1 格式: yyyyMM 與查詢All方式不同
            string tmpTIME1 = "", tmpTIME2 = "";

            if (APPTIME1 != "" && APPTIME1 != null)
            {
                tmpTIME1 = Convert.ToDateTime(APPTIME1.Substring(4, 11)).ToString("yyyyMM");
                int Intyear1 = Convert.ToInt32(tmpTIME1.Substring(0, 4)) - 1911;
                APPTIME1 = Convert.ToString(Intyear1) + tmpTIME1.Substring(4, 2);
            }
            else
            {
                APPTIME1 = "";
            }
            if (APPTIME2 != "" && APPTIME2 != null)
            {
                tmpTIME2 = Convert.ToDateTime(APPTIME2.Substring(4, 11)).ToString("yyyyMM");
                int Intyear2 = Convert.ToInt32(tmpTIME2.Substring(0, 4)) - 1911;
                APPTIME2 = Convert.ToString(Intyear2) + tmpTIME2.Substring(4, 2);
            }
            else
            {
                APPTIME2 = "";
            }

            DynamicParameters p = new DynamicParameters();
            var sql = @"select Trim(INID_NAME(USER_INID(:USRID))) as INID_NAME_USER from PARAM_D where rownum = 1";
            var title = "";


            if (showdata == "1")
            {//明細
                title = DBWork.Connection.ExecuteScalar(sql, new { USRID = userId }, DBWork.Transaction).ToString()
                    + APPTIME1 + "至" + APPTIME2 + "品項分配明細報表";
            }
            else if (showdata == "2")
            { //彙整
                title = DBWork.Connection.ExecuteScalar(sql, new { USRID = userId }, DBWork.Transaction).ToString()
                    + APPTIME1 + "至" + APPTIME2 + "品項分配彙整報表";
            }
            return title;
        }
        public DataTable GetExcel(string userId, string APPTIME1, string APPTIME2, string task_id, string mmcode, string showopt, string showdata)
        {
            //轉成需求格式 EX:10804 ; tmpTIME1 格式: yyyyMM 與查詢All方式不同
            string tmpTIME1 = "", tmpTIME2 = "";

            if (APPTIME1 != "" && APPTIME1 != null)
            {
                tmpTIME1 = Convert.ToDateTime(APPTIME1.Substring(4, 11)).ToString("yyyyMM");
                int Intyear1 = Convert.ToInt32(tmpTIME1.Substring(0, 4)) - 1911;
                APPTIME1 = Convert.ToString(Intyear1) + tmpTIME1.Substring(4, 2);
            }
            else
            {
                APPTIME1 = "";
            }
            if (APPTIME2 != "" && APPTIME2 != null)
            {
                tmpTIME2 = Convert.ToDateTime(APPTIME2.Substring(4, 11)).ToString("yyyyMM");
                int Intyear2 = Convert.ToInt32(tmpTIME2.Substring(0, 4)) - 1911;
                APPTIME2 = Convert.ToString(Intyear2) + tmpTIME2.Substring(4, 2);
            }
            else
            {
                APPTIME2 = "";
            }


            //MI_MATCLASS 有物料分類名稱
            DynamicParameters p = new DynamicParameters();

            var sql = @"SELECT B.MMCODE as 院內碼, 
                       (select mmname_c from MI_MAST where mmcode=b.mmcode) as 中文品名, 
                       (select mmname_e from MI_MAST where mmcode=b.mmcode) as 英文品名, 
                       (select base_unit from MI_MAST where mmcode=b.mmcode) as 計量單位, 
                       A.TOWH as 入庫庫房,(select WH_NAME from MI_WHMAST where WH_NO = a.TOWH) as 庫房名稱, ";

            if (showdata == "1")   //明細才會顯示在報表
            {
                sql += "substr(TWN_DATE(case when a.MAT_CLASS = '09' then b.apvtime else b.dis_time end),1,5) as 年月, ";
            }
            sql += "SUM(b.apvqty) as 總核撥量, ";
            if (showdata == "1")   //明細才會顯示在報表
            {
                sql += "(select AVG_PRICE from MI_WHCOST where DATA_YM=substr(TWN_DATE(case when a.MAT_CLASS = '09' then b.apvtime else b.dis_time end),1,5) and MMCODE=b.mmcode) as 庫存單價, ";
                sql += "(select m_contprice from MI_MAST where mmcode=b.mmcode) as 合約單價, ";
            }

            sql += @"SUM(b.apvqty)*(select m_contprice from MI_MAST where mmcode=b.mmcode) as 合約金額  from ME_DOCM a,ME_DOCD b ";

            sql += @"where a.docno=b.docno ";

            //tmpTIME1 格式: yyyyMM 與查詢All方式不同
            if (tmpTIME1 != "" && tmpTIME1 != null)
            {
                sql += "AND to_char(case when a.MAT_CLASS = '09' then b.apvtime else b.dis_time end,'yyyymm') >= Substr(:d0, 1, 6) AND SUBSTR(TWN_DATE(case when a.MAT_CLASS = '09' then b.apvtime else b.dis_time end),1,5) is NOT NULL ";
                p.Add(":d0", tmpTIME1);
            }
            if (tmpTIME2 != "" && tmpTIME2 != null)
            {
                sql += "AND to_char(case when a.MAT_CLASS = '09' then b.apvtime else b.dis_time end,'yyyymm') <= Substr(:d1, 1, 6) ";
                p.Add(":d1", tmpTIME2);
            }

            if (showopt != "")  //庫備,非庫備
            {
                sql += "and (select m_storeid from MI_MAST where mmcode=b.mmcode)=:M_STOREID ";
                p.Add(":M_STOREID", showopt);
            }
            if (task_id != "" && task_id != null)  //物料分類
            {
                string[] tmp = task_id.Split(',');
                sql += "AND A.MAT_CLASS IN :MAT_CLASS ";
                p.Add(":MAT_CLASS", tmp);
            }
            if (mmcode != "")    //院內碼
            {
                sql += "AND B.MMCODE LIKE :p1 ";
                p.Add(":p1", string.Format("{0}", mmcode));
            }
            sql += "   and A.flowid in ('3','4','5','6', '51')";
            if (showdata == "1") //明細
            {
                sql += @"group by b.mmcode,a.TOWH,substr(TWN_DATE(case when a.MAT_CLASS = '09' then b.apvtime else b.dis_time end),1,5) 
                     order by b.mmcode,a.TOWH,substr(TWN_DATE(case when a.MAT_CLASS = '09' then b.apvtime else b.dis_time end), 1, 5)";
            }
            else if (showdata == "2") //彙整
            {
                sql += @"group by b.mmcode,a.TOWH 
                         order by b.mmcode,a.TOWH";
            }
            DataTable dt = new DataTable();


            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;


        }

    }
}