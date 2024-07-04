using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Repository.AA
{
    public class AA0079Repository : JCLib.Mvc.BaseRepository
    {
        public AA0079Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AA0079M> Print(string condition1, string whno, string whgrade, string startDate, string endDate, string reporttype, bool isHospCode0)
        {
            var p = new DynamicParameters();

            string sql = @"";

            if (isHospCode0)
            {
                switch (reporttype)
                {
                    case "all":
                        sql += GetIn(condition1, whno, whgrade, startDate, endDate);
                        sql += "  UNION ALL ";
                        sql += GetOut(condition1, whno, whgrade, startDate, endDate);
                        break;
                    case "in":
                        sql += GetIn(condition1, whno, whgrade, startDate, endDate);
                        break;
                    case "out":
                        sql += GetOut(condition1, whno, whgrade, startDate, endDate);
                        break;
                }
            }
            else {
                switch (reporttype)
                {
                    case "all":
                        sql += GetInNot0(condition1, whno, whgrade, startDate, endDate);
                        sql += "  UNION ALL ";
                        sql += GetOutNot0(condition1, whno, whgrade, startDate, endDate);
                        break;
                    case "in":
                        sql += GetInNot0(condition1, whno, whgrade, startDate, endDate);
                        break;
                    case "out":
                        sql += GetOutNot0(condition1, whno, whgrade, startDate, endDate);
                        break;
                }
            }
            

            p.Add(":whno", whno);
            p.Add(":whgrade", whgrade);
            p.Add(":startDate", DateTime.Parse(startDate).ToString("yyyy-MM-dd"));
            p.Add(":endDate", DateTime.Parse(endDate).ToString("yyyy-MM-dd"));


            return DBWork.Connection.Query<AA0079M>(sql, new {
                wh_no = whno,
                whgrade = whgrade,
                startdate = DateTime.Parse(startDate).ToString("yyyy-MM-dd"),
                enddate = DateTime.Parse(endDate).ToString("yyyy-MM-dd"),
            }, DBWork.Transaction);
        }

        private string GetIn(string condition1, string whno, string whgrade, string startDate, string endDate) {
            string sql = @"select  '撥發入庫' as DOCTYPE,
                                   TWN_DATE(a.post_time) as POST_TIME,
                                   a.towh as TOWH,
                                   b.mmcode,
                                   c.mmname_e,
                                   a.frwh,
                                   c.base_unit,
                                   b.appqty,
                                   d.una as APPID,
                                   'A' as AD
                              from ME_DOCM a, ME_DOCD b, MI_MAST c, UR_ID d";

            if (condition1 == "1")
            {
                sql += @"                                        , MI_WHMAST e
                             where e.wh_grade = :whgrade
                               and a.towh = e.wh_no";
            }
            else {
                sql += @"    where a.towh = :wh_no";
            }
            // condition2
            sql += @"    and TRUNC(a.post_time, 'DD') between TO_DATE(:startdate, 'YYYY-MM-DD') and TO_DATE(:enddate, 'YYYY-MM-DD')";

            sql += @"    and a.doctype = 'MS'
                         and a.flowid = '0699'
                         and b.docno = a.docno
                         and c.mmcode = b.mmcode
                         and c.e_restrictcode in ('1','2','3','4') 
                         and d.tuser = a.appid";
            return sql;
        }

        private string GetOut(string condition1, string whno, string whgrade, string startDate, string endDate)
        {
            string sql = @"select  '退回上級庫(出庫)' as DOCTYPE,
                                   TWN_DATE(a.post_time) as POST_TIME,
                                   a.towh as FRWH,
                                   b.mmcode,
                                   c.mmname_e,
                                   a.frwh as TOWH,
                                   c.base_unit,
                                   b.appqty,
                                   d.una as APPID,
                                   'D' as AD
                              from ME_DOCM a, ME_DOCD b, MI_MAST c, UR_ID d";

            if (condition1 == "1")
            {
                sql += @"                                        , MI_WHMAST e
                             where e.wh_grade = :whgrade
                               and a.frwh = e.wh_no";
            }
            else
            {
                sql += @"    where a.frwh = :wh_no";
            }
            // condition2
            sql += @"    and TRUNC(a.post_time, 'DD') between TO_DATE(:startdate, 'YYYY-MM-DD') and TO_DATE(:enddate, 'YYYY-MM-DD')";

            sql += @"    and a.doctype = 'RS'
                         and a.flowid = '1299'
                         and b.docno = a.docno
                         and c.mmcode = b.mmcode
                         and c.e_restrictcode in ('1','2','3','4') 
                         and d.tuser = a.appid";
            return sql;
        }

        private string GetInNot0(string condition1, string whno, string whgrade, string startDate, string endDate)
        {
            string sql = @"select  (case when (select wh_grade from MI_WHMAST where wh_no = :wh_no)='1' then '撥發出庫' else '撥發入庫' end) as DOCTYPE,
                                   TWN_DATE(a.post_time) as POST_TIME,
                                   a.towh as TOWH,
                                   b.mmcode,
                                   c.mmname_e,
                                   a.frwh,
                                   c.base_unit,
                                   b.appqty,
                                   d.una as APPID,
                                   'A' as AD
                              from ME_DOCM a, ME_DOCD b, MI_MAST c, UR_ID d";

            if (condition1 == "1")
            {
                sql += @"                                        , MI_WHMAST e
                             where e.wh_grade = :whgrade
                               and a.towh = e.wh_no";
            }
            else
            {
                sql += @"    where (a.towh = :wh_no or a.frwh = :wh_no)";
            }
            // condition2
            sql += @"    and TRUNC(a.post_time, 'DD') between TO_DATE(:startdate, 'YYYY-MM-DD') and TO_DATE(:enddate, 'YYYY-MM-DD')";

            sql += @"    and a.doctype in ('MR', 'MS')
                         and a.flowid in ('0199','0699')
                         and b.docno = a.docno
                         and c.mmcode = b.mmcode
                         and c.e_restrictcode in ('1','2','3','4') 
                         and d.tuser = a.appid";
            return sql;
        }

        public string GetOutNot0(string condition1, string whno, string whgrade, string startDate, string endDate)
        {
            string sql = @"select  (case when (select wh_grade from MI_WHMAST where wh_no = :wh_no)='1' then '退料入庫' else '退回上級庫(出庫)' end ) as DOCTYPE,
                                   TWN_DATE(a.post_time) as POST_TIME,
                                   a.frwh as FRWH,
                                   b.mmcode,
                                   c.mmname_e,
                                   a.towh as TOWH,
                                   c.base_unit,
                                   b.appqty,
                                   d.una as APPID,
                                   'D' as AD
                              from ME_DOCM a, ME_DOCD b, MI_MAST c, UR_ID d";

            if (condition1 == "1")
            {
                sql += @"                                        , MI_WHMAST e
                             where e.wh_grade = :whgrade
                               and a.frwh = e.wh_no";
            }
            else
            {
                sql += @"    where (a.towh = :wh_no or a.frwh = :wh_no)";
            }
            // condition2
            sql += @"    and TRUNC(a.post_time, 'DD') between TO_DATE(:startdate, 'YYYY-MM-DD') and TO_DATE(:enddate, 'YYYY-MM-DD')";

            sql += @"    and a.doctype in ('RN')
                         and a.flowid = '0499'
                         and b.docno = a.docno
                         and c.mmcode = b.mmcode
                         and c.e_restrictcode in ('1','2','3','4') 
                         and d.tuser = a.appid";
            return sql;
        }

        public string GetHospCode()
        {
            string sql = @"
                select data_value from PARAM_D where grp_code='HOSP_INFO' and data_name='HospCode'
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }

        #region
        public class WhnoCombo {
            public string WH_NO { get; set; }
            public string WH_NAME { get; set; }
        }

        public IEnumerable<WhnoCombo> GetWhnoCombo() {
            string sql = @"select wh_no,
                                  (wh_no || ' ' || wh_name) as WH_NAME
                             from MI_WHMAST
                            where WH_KIND = '0'
                            order by wh_no";
            return DBWork.Connection.Query<WhnoCombo>(sql, DBWork.Transaction);
        }

        public class WhgradeCombo {
            public string GRADE_VALUE { get;set;}
            public string GRADE_NAME { get;set;}
        }

        public IEnumerable<WhgradeCombo> GetWhgradeCombo() {
            string sql = @" select data_value as grade_value,
                                   (data_value || ' ' || data_desc) as grade_name
                              from PARAM_D
                             where grp_code = 'MI_WHMAST'
                               and data_name = 'WH_GRADE'
                             order by data_value";
            return DBWork.Connection.Query<WhgradeCombo>(sql, DBWork.Transaction);
        }
        #endregion
    }
}