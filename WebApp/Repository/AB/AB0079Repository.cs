using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using WebApp.Models.AB;
using WebApp.Models.C;
using System.Globalization;

namespace WebApp.Repository.AB                      // WebApp\Repository\C\CD0007Repository.cs          
{
    public class AB0079Repository : JCLib.Mvc.BaseRepository
    {
        public AB0079Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        // 醫師
        public IEnumerable<COMBO_MODEL> GetOrderDrCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @" SELECT distinct(OrderDr) as VALUE, OrderDr || ' ' || chinName as COMBITEM from ME_AB0079D";

            sql += " ORDER BY OrderDr ";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<COMBO_MODEL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        // 科室
        public IEnumerable<COMBO_MODEL> GetSectionNoCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @" SELECT distinct(SectionNo) as VALUE, SectionNo || ' ' || SectionName as COMBITEM from ME_AB0079S";

            sql += " ORDER BY SectionNo" +
                " ";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<COMBO_MODEL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        // 院內碼
        public IEnumerable<COMBO_MODEL> GetOrderCodeCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @" SELECT distinct(ORDERCODE) as VALUE, ORDERCODE || ' ' || ORDERENGNAME as COMBITEM from ME_AB0079M";

            sql += " ORDER BY ORDERCODE" +
                " ";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<COMBO_MODEL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);

        }

        public IEnumerable<ME_AB0079> GetAB0079Detail(string dsm, string datefr, string dateto, string OrderDrFr, string OrderDrTo, string SectionNoFr, string SectionNoTo, string OrderCodeFr, string OrderCodeTo, int page_index, int page_size, string sorters)
        {
            // p0, p1a, p1b, p2a, p2b, p3a, p3b, p4a, p4b,
            //CultureInfo MyCultureInfo = new CultureInfo("en-US");
            //DateTime add_dateto = new DateTime();
            //string str_add_dateto = "";

            var p = new DynamicParameters();

            // OrderCode         --院內碼
            // OrderEngName      --英文名稱
            // CreateYM          --查詢月份
            // OrderDr           --醫師代碼
            // ChinName          --醫師姓名
            // SectionNo         --科室
            // SectionName       --科室名
            // SumQty            --醫囑(住)消耗量
            // SumAmount         --醫囑(住)消耗金額
            // OPDQty            --醫囑(門)消耗量
            // OPDAmount         --醫囑(門)消耗金額
            // DSM               --D:醫師 S:科室 M:藥品

            var sql = @"select OrderCode,OrderEngName,CreateYM,OrderDr,ChinName,SectionNo,SectionName,SumQty, SumAmount, OPDQty,OPDAmount,DSM from ME_AB0079  ";
            sql += " where 1=1 ";

            sql += " and DSM = :dsm ";
            p.Add(":dsm", string.Format("{0}", dsm.Trim()));

            if (datefr.Trim() != "")
            {
                sql += " and CREATEYM >= :datefr ";
                p.Add(":datefr", string.Format("{0}", datefr.Trim()));
            }

            if (dateto.Trim() != "")
            {
                sql += " and CREATEYM <= :dateto ";
                p.Add(":dateto", string.Format("{0}", dateto.Trim()));
            }

            // 醫師代碼
            if (OrderDrFr.Trim() != "")
            {
                sql += " and ORDERDR >= :orderdrfr ";
                p.Add(":orderdrfr", string.Format("{0}", OrderDrFr.Trim()));
            }

            if (OrderDrTo.Trim() != "")
            {
                sql += " and ORDERDR <= :orderdrto ";
                p.Add(":orderdrto", string.Format("{0}", OrderDrTo.Trim()));
            }


            // 科室
            if (SectionNoFr.Trim() != "")
            {
                sql += " and SECTIONNO >= :sectionno ";
                p.Add(":sectionno", string.Format("{0}", SectionNoFr.Trim()));
            }

            if (SectionNoTo.Trim() != "")
            {
                sql += " and SECTIONNO <= :sectionto ";
                p.Add(":sectionto", string.Format("{0}", SectionNoTo.Trim()));
            }


            // 院內碼
            if (OrderCodeFr.Trim() != "")
            {
                sql += " and ORDERCODE >= :ordercodefr ";
                p.Add(":ordercodefr", string.Format("{0}", OrderCodeFr.Trim()));
            }

            if (OrderCodeTo.Trim() != "")
            {
                sql += " and ORDERCODE <= :ordercodeto ";
                p.Add(":ordercodeto", string.Format("{0}", OrderCodeTo.Trim()));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_AB0079>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //public DataTable GetExcel(string dsm, string datefr, string dateto, string OrderDrFr, string OrderDrTo, string SectionNoFr, string SectionNoTo, string OrderCodeFr, string OrderCodeTo)
        public DataTable GetExcel(string dsm, string str_CreatYM, string OrderDrFr, string OrderDrTo, string SectionNoFr, string SectionNoTo, string OrderCodeFr, string OrderCodeTo)
        {
            DynamicParameters p = new DynamicParameters();

            // OrderCode         --院內碼
            // OrderEngName      --英文名稱
            // CreateYM          --查詢月份
            // OrderDr           --醫師代碼
            // ChinName          --醫師姓名
            // SectionNo         --科室
            // SectionName       --科室名
            // SumQty            --醫囑(住)消耗量
            // SumAmount         --醫囑(住)消耗金額
            // OPDQty            --醫囑(門)消耗量
            // OPDAmount         --醫囑(門)消耗金額
            // DSM               --D:醫師 S:科室 M:藥品

            var sql = "";

            if (dsm.Trim() == "D")  //醫師
            {
                sql = @"SELECT pvt.*
                    FROM (SELECT CREATEYM, ORDERCODE, ORDERENGNAME, SECTIONNO, SECTIONNAME,
                        ORDERDR, CHINNAME,
                        SUM(SumQty) SumQty, SUM(SUMAMOUNT) SUMAMOUNT,
                        SUM(OPDQty) OPDQty, SUM(OPDAmount) OPDAmount 
                        FROM ME_AB0079 WHERE DSM = 'D'
                        GROUP BY CREATEYM, ORDERCODE, ORDERENGNAME, SECTIONNO, SECTIONNAME, ORDERDR, CHINNAME)
                    PIVOT (
                        SUM(SumQty) 醫囑_住_消耗量, SUM(SUMAMOUNT) 醫囑_住_消耗金額,
                        SUM(OPDQty) 醫囑_門_消耗量, SUM(OPDAmount) 醫囑_門_消耗金額
                        FOR CreateYM IN (" + str_CreatYM + ")) pvt ";
            }
            else if (dsm.Trim() == "S") //科室
            {
                sql = @"SELECT pvt.*
                    FROM (SELECT CREATEYM, SECTIONNO, SECTIONNAME,
                        SUM(SumQty) SumQty, SUM(SUMAMOUNT) SUMAMOUNT,
                        SUM(OPDQty) OPDQty, SUM(OPDAmount) OPDAmount 
                        FROM ME_AB0079 WHERE DSM = 'S' 
                        GROUP BY CREATEYM, SECTIONNO, SECTIONNAME)                 
                    PIVOT (
                        SUM(SumQty) 醫囑_住_消耗量, SUM(SUMAMOUNT) 醫囑_住_消耗金額,
                        SUM(OPDQty) 醫囑_門_消耗量, SUM(OPDAmount) 醫囑_門_消耗金額
                        FOR CreateYM IN (" + str_CreatYM + ")) pvt";
            }
            else if (dsm.Trim() == "M") //藥品
            {
                sql = @"SELECT pvt.*
                    FROM (SELECT CREATEYM, ORDERCODE, ORDERENGNAME, SECTIONNO, SECTIONNAME,
                        SUM(SumQty) SumQty, SUM(SUMAMOUNT) SUMAMOUNT,
                        SUM(OPDQty) OPDQty, SUM(OPDAmount) OPDAmount 
                        FROM ME_AB0079 WHERE DSM = 'M'
                        GROUP BY CREATEYM, ORDERCODE, ORDERENGNAME, SECTIONNO, SECTIONNAME)                 
                    PIVOT (
                        SUM(SumQty) 醫囑_住_消耗量, SUM(SUMAMOUNT) 醫囑_住_消耗金額,
                        SUM(OPDQty) 醫囑_門_消耗量, SUM(OPDAmount) 醫囑_門_消耗金額
                        FOR CreateYM IN (" + str_CreatYM + ")) pvt";
            }

            sql += " WHERE 1=1 ";

            // 醫師代碼
            if (OrderDrFr.Trim() != "")
            {
                sql += " and ORDERDR >= :orderdrfr ";
                p.Add(":orderdrfr", string.Format("{0}", OrderDrFr.Trim()));
            }

            if (OrderDrTo.Trim() != "")
            {
                sql += " and ORDERDR <= :orderdrto ";
                p.Add(":orderdrto", string.Format("{0}", OrderDrTo.Trim()));
            }

            // 科室
            if (SectionNoFr.Trim() != "")
            {
                sql += " and SECTIONNO >= :sectionno ";
                p.Add(":sectionno", string.Format("{0}", SectionNoFr.Trim()));
            }

            if (SectionNoTo.Trim() != "")
            {
                sql += " and SECTIONNO <= :sectionto ";
                p.Add(":sectionto", string.Format("{0}", SectionNoTo.Trim()));
            }

            // 院內碼
            if (OrderCodeFr.Trim() != "")
            {
                sql += " and ORDERCODE >= :ordercodefr ";
                p.Add(":ordercodefr", string.Format("{0}", OrderCodeFr.Trim()));
            }

            if (OrderCodeTo.Trim() != "")
            {
                sql += " and ORDERCODE <= :ordercodeto ";
                p.Add(":ordercodeto", string.Format("{0}", OrderCodeTo.Trim()));
            }

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);
            foreach (DataColumn tmp_dt_c in dt.Columns)
            {
                string tmp_ColumnName = tmp_dt_c.ColumnName;
                switch (tmp_ColumnName)
                {
                    case "ORDERCODE":
                        tmp_dt_c.ColumnName = "院內碼";
                        break;
                    case "ORDERENGNAME":
                        tmp_dt_c.ColumnName = "英文名稱";
                        break;
                    case "ORDERDR":
                        tmp_dt_c.ColumnName = "醫師代碼";
                        break;
                    case "CHINNAME":
                        tmp_dt_c.ColumnName = "醫師姓名";
                        break;
                    case "SECTIONNO":
                        tmp_dt_c.ColumnName = "科室";
                        break;
                    case "SECTIONNAME":
                        tmp_dt_c.ColumnName = "科室名";
                        break;
                    case "DSM":
                        //dt.Columns.Remove("DSM");
                        tmp_dt_c.ColumnName = "統計類別";
                        break;
                    default:
                        break;
                }
            }
            return dt;
        }
    }
}