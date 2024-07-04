using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models.F;
using WebApp.Models;
using System.Collections.Generic;
using System.Linq;
using TSGH.Models;
using JCLib.Mvc;
namespace WebApp.Repository.F

{
    public class FA0035Repository : JCLib.Mvc.BaseRepository
    {
        public FA0035Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        // GET api/<controller>
        public IEnumerable<ComboItemModel> GetMATCombo()
        {
            string sql = @"Select MAT_CLASS||' '||MAT_CLSNAME TEXT, MAT_CLASS VALUE from MI_MATCLASS
                             where mat_class in ('02','03','04','05','06','07','08')  ORDER BY VALUE";


            return DBWork.Connection.Query<ComboItemModel>(sql, new { userID = DBWork.ProcUser }, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMMCODECombo(string mmcode, string task_id, string store_id, int page_index, int page_size, string sorters)
        {
            DynamicParameters p = new DynamicParameters();

            string sql = @"SELECT DISTINCT {0} MMCODE , MMNAME_C, MMNAME_E from MI_MAST A WHERE 1=1 ";


            if (task_id != "" && task_id != null)  //物料分類
            {
                string[] tmp = task_id.Split(',');
                sql += "AND MAT_CLASS IN :mat_class ";
                p.Add(":mat_class", tmp);
            }

            if (store_id != "" && store_id != null)  //庫備或是非庫備
            {
                sql += "AND M_STOREID =:M_STOREID ";
                p.Add(":M_STOREID", store_id);
            }

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

        public IEnumerable<FA0035> GetAll(string mclass, string bgdate, string endate, string mmcode, string storeid, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select mmcode,(select mmname_c from MI_MAST where mmcode=b.mmcode) as mmname_c,(select mmname_e from MI_MAST where mmcode=b.mmcode) as mmname_e,(select base_unit from MI_MAST where mmcode=b.mmcode) as base_unit,data_ym,
                        (select in_price from V_COST_WH15 where data_ym=b.data_ym and wh_no in (select whno_mm1 as wh_no from dual) and mmcode=b.mmcode) as in_price,(select p_inv_qty from V_COST_WH15 where data_ym=b.data_ym and wh_no in 
                        (select whno_mm1 as wh_no from dual) and mmcode=b.mmcode) as p_inv_qty,(select in_qty from V_COST_WH15 where data_ym=b.data_ym and wh_no in (select whno_mm1 as wh_no from dual) and mmcode=b.mmcode) as in_qty,
                        (select out_qty from V_COST_WH15 where data_ym=b.data_ym and wh_no in (select whno_mm1 as wh_no from dual) and mmcode=b.mmcode) as out_qty,(select inv_qty from V_COST_WH15 where data_ym=b.data_ym and wh_no in 
                        (select whno_mm1 as wh_no from dual) and mmcode=b.mmcode) as inv_qty,(select inv_qty from V_COST_WH15 where data_ym=b.data_ym and wh_no in (select whno_1x('1') as wh_no from dual) and mmcode=b.mmcode) as mil_inv_qty,
                        (select sum(inv_qty) from V_COST_WH15 where data_ym=b.data_ym and wh_no in (select whno_mm1 as wh_no from dual union select whno_1x('1') as wh_no from dual) and mmcode=b.mmcode) as sum_inv_qty,
                        NVL((select TOT_BWQTY FROM V_MM_TOTAPL WHERE DATE_YM=b.data_ym AND MMCODE=b.mmcode),0 )TOT_BWQTY 
                        from (select distinct a.mmcode,data_ym
                         from V_COST_WH15 a, MI_MAST m where 1=1 ";

            if (bgdate != "null" && bgdate != "")
            {
                sql += " AND data_ym in ( " + bgdate + ") and a.mmcode=m.mmcode";
                p.Add(":p1", bgdate);
            }
            //if (endate != "null" && endate != "")
            //{
            //    sql += " AND data_ym <= :p2 ";
            //    p.Add(":p2", endate);
            //}

            if (mclass != "" && mclass != "null")
            {
                sql += " AND m.mat_class = :p3 ";
                p.Add(":p3", mclass);
            }

            if (mmcode != "" && mmcode != "null")
            {
                sql += " AND a.mmcode = :p5 ";
                p.Add(":p5", mmcode);
            }

            if (storeid != "" && storeid != "2" )
            {
                sql += " AND m.m_storeid  = :p6 ";
                p.Add(":p6", storeid);
            }





            sql += " ) b order by  mmcode,data_ym ";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<FA0035>(GetPagingStatement(sql, sorters), p);
            //return DBWork.Connection.Query<FA0035>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }


        public DataTable GetExcel(string mclass, string bgdate, string endate, string mmcode, string storeid)
        {
            var p = new DynamicParameters();

            var sql = @"select mmcode as 院內碼,(select mmname_c from MI_MAST where mmcode=b.mmcode) as 中文名稱,(select mmname_e from MI_MAST where mmcode=b.mmcode) as 英文名稱,(select base_unit from MI_MAST where mmcode=b.mmcode) as 單位,data_ym as 年月,
                        (select in_price from V_COST_WH15 where data_ym=b.data_ym and wh_no in (select whno_mm1 as wh_no from dual) and mmcode=b.mmcode) as 進貨單價,(select p_inv_qty from V_COST_WH15 where data_ym=b.data_ym and wh_no in 
                        (select whno_mm1 as wh_no from dual) and mmcode=b.mmcode) as 民期初量,(select in_qty from V_COST_WH15 where data_ym=b.data_ym and wh_no in (select whno_mm1 as wh_no from dual) and mmcode=b.mmcode) as 民進貨量,
                        (select out_qty from V_COST_WH15 where data_ym=b.data_ym and wh_no in (select whno_mm1 as wh_no from dual) and mmcode=b.mmcode) as 民核撥量,(select inv_qty from V_COST_WH15 where data_ym=b.data_ym and wh_no in 
                        (select whno_mm1 as wh_no from dual) and mmcode=b.mmcode) as 民庫存量,(select inv_qty from V_COST_WH15 where data_ym=b.data_ym and wh_no in (select whno_1x('1') as wh_no from dual) and mmcode=b.mmcode) as 軍庫存量,
                        (select sum(inv_qty) from V_COST_WH15 where data_ym=b.data_ym and wh_no in (select whno_mm1 as wh_no from dual union select whno_1x('1') as wh_no from dual) and mmcode=b.mmcode) as 總庫存量,
                        NVL((select TOT_BWQTY FROM V_MM_TOTAPL WHERE DATE_YM=b.data_ym AND MMCODE=b.mmcode),0 )累計調撥量  
                            from (select distinct a.mmcode,data_ym
                         from V_COST_WH15 a,MI_MAST m where 1=1 ";

            //if (bgdate != "null" && bgdate != "")
            //{
            //    sql += " AND data_ym in ( " + bgdate + ") ";
            //    p.Add(":p1", bgdate);
            //}
            ////if (endate != "null" && endate != "")
            ////{
            ////    sql += " AND data_ym <= :p2 ";
            ////    p.Add(":p2", endate);
            ////}

            //if (mclass != "" && mclass != "null")
            //{
            //    sql += " AND(select mat_class from MI_MAST  where mmcode=a.mmcode)  = :p3 ";
            //    p.Add(":p3", mclass);
            //}

            //if (mmcode != "" && mmcode != "null")
            //{
            //    sql += " AND a.mmcode = :p5 ";
            //    p.Add(":p5", mmcode);
            //}

            //if (storeid != "" && storeid != "2")
            //{
            //    sql += " AND (select m_storeid from MI_MAST where mmcode=a.mmcode) = :p6 ";
            //    p.Add(":p6", storeid);
            //}

            if (bgdate != "null" && bgdate != "")
            {
                sql += " AND data_ym in ( " + bgdate + ") and a.mmcode=m.mmcode";
                p.Add(":p1", bgdate);
            }
            //if (endate != "null" && endate != "")
            //{
            //    sql += " AND data_ym <= :p2 ";
            //    p.Add(":p2", endate);
            //}

            if (mclass != "" && mclass != "null")
            {
                sql += " AND m.mat_class = :p3 ";
                p.Add(":p3", mclass);
            }

            if (mmcode != "" && mmcode != "null")
            {
                sql += " AND a.mmcode = :p5 ";
                p.Add(":p5", mmcode);
            }

            if (storeid != "" && storeid != "2")
            {
                sql += " AND m.m_storeid  = :p6 ";
                p.Add(":p6", storeid);
            }




            sql += " ) b order by  mmcode,data_ym ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
        public string MCLSNAME(string mid)
        {
            string rtnmname = "";
            string sql = @"SELECT MAT_CLSNAME  FROM MI_MATCLASS where MAT_CLASS=:mid";


            rtnmname = DBWork.Connection.QueryFirst<string>(sql, new { mid }, DBWork.Transaction);

            return rtnmname;
        }

        public IEnumerable<FA0035> GetReport(string mclass, string bgdate, string endate, string mmcode, string storeid)
        {
            var p = new DynamicParameters();

            var sql = @"select mmcode,(select mmname_c from MI_MAST where mmcode=b.mmcode) as mmname_c,(select mmname_e from MI_MAST where mmcode=b.mmcode) as mmname_e,(select base_unit from MI_MAST where mmcode=b.mmcode) as base_unit,data_ym,
                        (select in_price from V_COST_WH15 where data_ym=b.data_ym and wh_no in (select whno_mm1 as wh_no from dual) and mmcode=b.mmcode) as in_price,(select p_inv_qty from V_COST_WH15 where data_ym=b.data_ym and wh_no in 
                        (select whno_mm1 as wh_no from dual) and mmcode=b.mmcode) as p_inv_qty,(select in_qty from V_COST_WH15 where data_ym=b.data_ym and wh_no in (select whno_mm1 as wh_no from dual) and mmcode=b.mmcode) as in_qty,
                        (select out_qty from V_COST_WH15 where data_ym=b.data_ym and wh_no in (select whno_mm1 as wh_no from dual) and mmcode=b.mmcode) as out_qty,(select inv_qty from V_COST_WH15 where data_ym=b.data_ym and wh_no in 
                        (select whno_mm1 as wh_no from dual) and mmcode=b.mmcode) as inv_qty,(select inv_qty from V_COST_WH15 where data_ym=b.data_ym and wh_no in (select whno_1x('1') as wh_no from dual) and mmcode=b.mmcode) as mil_inv_qty,
                        (select sum(inv_qty) from V_COST_WH15 where data_ym=b.data_ym and wh_no in (select whno_mm1 as wh_no from dual union select whno_1x('1') as wh_no from dual) and mmcode=b.mmcode) as sum_inv_qty,
                        NVL((select TOT_BWQTY FROM V_MM_TOTAPL WHERE DATE_YM=b.data_ym AND MMCODE=b.mmcode),0 )TOT_BWQTY 
                         from (select distinct a.mmcode,data_ym
                         from V_COST_WH15 a,MI_MAST m where 1=1 ";


            //if (bgdate != "null" && bgdate != "")
            //{
            //    sql += " AND data_ym in ( " + bgdate + ") ";
            //    p.Add(":p1", bgdate);
            //}
            ////if (endate != "null" && endate != "")
            ////{
            ////    sql += " AND data_ym <= :p2 ";
            ////    p.Add(":p2", endate);
            ////}

            //if (mclass != "" && mclass != "null")
            //{
            //    sql += " AND(select mat_class from MI_MAST  where mmcode=a.mmcode)  = :p3 ";
            //    p.Add(":p3", mclass);
            //}

            //if (mmcode != "" && mmcode != "null")
            //{
            //    sql += " AND a.mmcode = :p5 ";
            //    p.Add(":p5", mmcode);
            //}

            //if (storeid != "" && storeid != "2")
            //{
            //    sql += " AND (select m_storeid from MI_MAST where mmcode=a.mmcode) = :p6 ";
            //    p.Add(":p6", storeid);
            //}

            if (bgdate != "null" && bgdate != "")
            {
                sql += " AND data_ym in ( " + bgdate + ") and a.mmcode=m.mmcode";
                p.Add(":p1", bgdate);
            }
            //if (endate != "null" && endate != "")
            //{
            //    sql += " AND data_ym <= :p2 ";
            //    p.Add(":p2", endate);
            //}

            if (mclass != "" && mclass != "null")
            {
                sql += " AND m.mat_class = :p3 ";
                p.Add(":p3", mclass);
            }

            if (mmcode != "" && mmcode != "null")
            {
                sql += " AND a.mmcode = :p5 ";
                p.Add(":p5", mmcode);
            }

            if (storeid != "" && storeid != "2")
            {
                sql += " AND m.m_storeid  = :p6 ";
                p.Add(":p6", storeid);
            }




            sql += " ) b order by  mmcode,data_ym ";

            return DBWork.Connection.Query<FA0035>(sql, p, DBWork.Transaction);
        }
    }
}