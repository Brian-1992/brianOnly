using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using WebApp.Models.C;
using System.Globalization;

namespace WebApp.Repository.C                      // WebApp\Repository\C\CD0007Repository.cs          
{
    public class CD0007Repository : JCLib.Mvc.BaseRepository
    {
        public CD0007Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        //public IEnumerable<MI_WHMAST> GetWH_NoCombo(string p0, int page_index, int page_size, string sorters)
        public IEnumerable<CD0007WH_NO> GetWH_NoCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @" select b.WH_NO,(b.WH_NO || ' ' || b.WH_NAME) as WH_NAME,b.WH_KIND,a.WH_USERID from MI_WHID a, MI_WHMAST b where a.WH_NO=b.WH_NO and b.WH_GRADE='1' ";

            sql += " and a.WH_USERID = :WH_USERID ";
            p.Add(":WH_USERID", DBWork.ProcUser);
            //////p.Add(":WH_USERID", "560000");

            sql += " ORDER BY b.WH_NO ";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CD0007WH_NO>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);

        }

        public IEnumerable<CD0007> GetPickDetail(string wh_id, string datefrom, string dateto, string sortway, int page_index, int page_size, string sorters)
        {

            //CultureInfo MyCultureInfo = new CultureInfo("en-US");
            //DateTime add_dateto = new DateTime();
            //string str_add_dateto = "";

            var p = new DynamicParameters();

            var sql = @" 
SELECT 1 as colnumber, act_pick_userid, pick_username, docno_cnt, item_sum, pick_qty_sum, diffqty_sum
  FROM (  SELECT act_pick_userid,
                 '總計' AS pick_username,
                 COUNT (DISTINCT docno) AS docno_cnt,
                 SUM (item_cnt) AS item_sum,
                 SUM (appqty) AS pick_qty_sum,
                 SUM (diffqty) AS diffqty_sum
            FROM (SELECT 'zzzz' AS act_pick_userid,
                         docno,
                         1 AS item_cnt,
                         appqty,
                         act_pick_qty,
                         (act_pick_qty - appqty) AS diffqty
                    FROM BC_WHPICK a
                   WHERE     1 = 1
";

            if (!string.IsNullOrWhiteSpace(wh_id))
            {
                sql += " AND wh_no = :wh_id ";
                p.Add(":wh_id", string.Format("{0}", wh_id));
            }

            if (!string.IsNullOrWhiteSpace(datefrom))
            {
                sql += " AND pick_date >= TO_DATE(:datefrom, 'yyyy/MM/dd') ";
                p.Add(":datefrom", string.Format("{0}", DateTime.Parse(datefrom).ToString("yyyy/MM/dd")));
            }

            if (!string.IsNullOrWhiteSpace(dateto))
            {
                sql += " AND pick_date <= TO_DATE(:dateto, 'yyyy/MM/dd') ";
                p.Add(":dateto", string.Format("{0}", DateTime.Parse(dateto).ToString("yyyy/MM/dd")));
            }

            sql += " AND act_pick_userid IS NOT NULL) GROUP BY act_pick_userid) ";

            sql += @" UNION select 0 as colnumber, act_pick_userid, pick_username, docno_cnt, item_sum, pick_qty_sum, diffqty_sum ";
            sql += " from(select act_pick_userid, (select una from ur_id where tuser= b.act_pick_userid) as pick_username, ";
            sql += " count(distinct docno) as docno_cnt,sum(item_cnt) as item_sum,sum(appqty) as pick_qty_sum,sum(diffqty) as diffqty_sum ";
            sql += " from(select act_pick_userid, docno, 1 as item_cnt, appqty, act_pick_qty, (act_pick_qty - appqty) as diffqty from BC_WHPICK a ";
            sql += " where 1=1 ";

            sql += " and wh_no = :wh_id ";
            p.Add(":wh_id", string.Format("{0}", wh_id.Trim()));

            if (datefrom.Trim() != "")
            {
                sql += " and PICK_DATE >= TO_DATE(:PICK_DATE_S, 'YYYY/MM/DD') ";
                p.Add(":PICK_DATE_S", string.Format("{0}", DateTime.Parse(datefrom).ToString("yyyy/MM/dd")));
            }

            if (dateto.Trim() != "")
            {
                //add_dateto = DateTime.ParseExact("2019-05-30", "yyyy-mm-dd", MyCultureInfo).AddDays(1);
                //add_dateto = Convert.ToDateTime("2019-05-30").AddDays(1);
                //add_dateto = add_dateto.AddDays(1);
                //str_add_dateto = add_dateto.ToString("yyyy/mm/dd");

                sql += " and PICK_DATE <= TO_DATE(:PICK_DATE_E, 'YYYY/MM/DD') ";
                //p.Add(":PICK_DATE_E", string.Format("{0}", DateTime.Parse(str_add_dateto).ToString("yyyy/MM/dd")));
                p.Add(":PICK_DATE_E", string.Format("{0}", DateTime.Parse(dateto).ToString("yyyy/MM/dd")));
            }

            sql += " and act_pick_userid is not null ) b group by act_pick_userid ) ";

            sorters = "";

            if (sortway.Trim() == "1")         // 總單號數排序時加這個條件
            {
                sql += "order by colnumber, docno_cnt desc";
            }
            else if (sortway.Trim() == "2")  // 以總品項數排序時加這個條件
            {
                sql += "order by colnumber, item_sum desc";
            }
            else if (sortway.Trim() == "3")  // 以總件數排序時加這個條件
            {
                sql += "order by colnumber, pick_qty_sum desc";
            }
            else if (sortway.Trim() == "4")  // 以差異品項數排序時加這個條件
            {
                sql += "order by colnumber, diffqty_sum desc";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CD0007>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);

        }

    }
}