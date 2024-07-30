using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JCLib.DB;
using WebApp.Models;
using Dapper;
using System.Data;
namespace WebApp.Repository.AA
{
    public class AA0077Repository : JCLib.Mvc.BaseRepository
    {
        public AA0077Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public class M
        {
            public string 院內碼 { get; set; }
            public string 英文品名 { get; set; }
            public string 廠商英文名稱 { get; set; }
            public string 入庫庫房代碼 { get; set; }
            public string 庫房名稱 { get; set; }
            public string 日期起 { get; set; }
            public string 日期迄 { get; set; }
            public int 核發數量 { get; set; }
            public int 合計數量 { get; set; }
            public double 箱數 { get; set; }
        }

        public IEnumerable<M> GetMTotal(string ts, bool isHospCode0)
        {
            var ss = ts.Split(',');
            var apptime_bg = ss[0];
            var apptime_ed = ss[1];
            // apptime_bg = apptime_bg.ToString("yyyyMMdd");
            var p = new DynamicParameters();

            //var sql = @"select c.mmcode as 院內碼,c.mmname_e as 英文品名,d.agen_namee as 廠商英文名稱,a.towh as 入庫庫房代碼,e.wh_no||'-'||e.wh_name as 庫房名稱,b.apvqty　as 核發數量
            //            from me_docm a, me_docd b, mi_mast c, ph_vender d,mi_whmast e where A.DOCNO = b.docno and b.mmcode = c.mmcode and c.m_agenno = d.agen_no
            //            and a.towh = e.wh_no and a.doctype = 'MS'and c.E_DRUGAPLTYPE = '1' ";
            var sql = string.Format(@"
                        select c.mmcode as 院內碼,c.mmname_e as 英文品名,d.agen_namee as 廠商英文名稱,a.towh as 入庫庫房代碼,e.wh_no||'-'||e.wh_name as 庫房名稱,b.apvqty　as 核發數量  
                        from mi_mast c 
                        left join me_docd b on b.mmcode = c.mmcode 
                        left join me_docm a on a.docno = b.docno 
                        left join ph_vender d on d.agen_no = c.m_agenno
                        left join mi_whmast e on e.wh_no = a.towh 
                        where 1 = 1 
                        and a.doctype in ('MS', 'MR') 
                        {0} 
                        and a.frwh = WHNO_ME1 
                        and a.flowid in ('0102', '0602','0111','0611')
                        and b.postid is null"
            , isHospCode0 ? "and c.e_drugapltype = '1'" : "and c.isIV='Y'");
            if (apptime_bg != "null" && apptime_bg != "NaN")
            {
                sql += " AND to_char(b.apl_contime,'yyyymmdd') >= Substr(:p1, 1, 8) ";
                p.Add(":p1", apptime_bg);
            }
            if (apptime_ed != "null" && apptime_ed != "NaN")
            {
                sql += " AND to_char(b.apl_contime,'yyyymmdd') <= Substr(:p2, 1, 8) ";
                p.Add(":p2", apptime_ed);
            }

            sql += "order by 廠商英文名稱,院內碼";
            var result = DBWork.Connection.Query<M>(sql, p);
            return result;
        }
        public IEnumerable<M> GetMSum(string ts, bool isHospCode0)
        {
            var ss = ts.Split(',');
            var apptime_bg = ss[0];
            var apptime_ed = ss[1];
            // apptime_bg = apptime_bg.ToString("yyyyMMdd");
            var p = new DynamicParameters();

            //var sql = @"select c.mmcode as 院內碼,sum(nvl(b.apvqty,0)) as 合計數量 from me_docm a, me_docd b, mi_mast c, ph_vender d,mi_whmast e where a.docno = b.docno
            //            and b.mmcode = c.mmcode and c.m_agenno = d.agen_no and a.towh = e.wh_no and a.doctype = 'MS' 
            //            and c.e_drugapltype = '1'  ";
            var sql = string.Format(@"
                        select c.mmcode as 院內碼,sum(nvl(b.apvqty,0)) as 合計數量 
                        from mi_mast c 
                        left join me_docd b on b.mmcode = c.mmcode 
                        left join me_docm a on a.docno = b.docno 
                        left join ph_vender d on d.agen_no = c.m_agenno
                        left join mi_whmast e on e.wh_no = a.towh 
                        where 1 = 1 
                        and a.doctype in ('MS', 'MR')  
                        {0} 
                        and a.frwh = WHNO_ME1 
                        and a.flowid in ('0102', '0602','0111','0611')
                        and b.postid is null"
            , isHospCode0 ? "and c.e_drugapltype = '1'" : "and c.isIV='Y'");
            if (apptime_bg != "null" && apptime_bg != "NaN")
            {
                sql += " AND to_char(b.apl_contime,'yyyymmdd') >= Substr(:p1, 1, 8) ";
                p.Add(":p1", apptime_bg);
            }
            if (apptime_ed != "null" && apptime_ed != "NaN")
            {
                sql += " AND to_char(b.apl_contime,'yyyymmdd') <= Substr(:p2, 1, 8) ";
                p.Add(":p2", apptime_ed);
            }

            sql += "group by c.mmcode order by c.mmcode";
            var result = DBWork.Connection.Query<M>(sql, p);
            return result;
        }

        public IEnumerable<M> GetMSumpack(string ts, bool isHospCode0)
        {
            var ss = ts.Split(',');
            var apptime_bg = ss[0];
            var apptime_ed = ss[1];
            // apptime_bg = apptime_bg.ToString("yyyyMMdd");
            var p = new DynamicParameters();

            //var sql = @"select a.mmcode  as 院內碼,nvl(apvqty,0)/nvl(min_ordqty,1) as 箱數  from mi_winvctl a,(select c.mmcode,sum(b.apvqty) apvqty
            //               from me_docm a, me_docd b, mi_mast c, ph_vender d,mi_whmast e where a.docno = b.docno
            //           and b.mmcode = c.mmcode and c.m_agenno = d.agen_no and a.towh = e.wh_no
            //           and a.doctype = 'MS'  and c.e_drugapltype = '1' ";
            //--and a.flowid = '0699'
            var sql = string.Format(@"
                          select a.mmcode as 院內碼,
                               round(nvl(b.apvqty,0)/(case when nvl(a.MIN_ORDQTY,1) = 0 then 1 else nvl(a.MIN_ORDQTY,1) end), 2) as 箱數  
                          from MI_WINVCTL a,
                               (select c.mmcode,sum(b.apvqty) apvqty
                                  from mi_mast c 
                                  left join me_docd b on b.mmcode = c.mmcode 
                                  left join me_docm a on a.docno = b.docno 
                                 where 1 = 1 
                                   and a.doctype in ('MS', 'MR') 
                                   {0} 
                                   and a.frwh = WHNO_ME1 
                                   and a.flowid in ('0102', '0602','0111','0611')
                                   and b.postid is null"
            , isHospCode0 ? "and c.e_drugapltype = '1' " : "and c.isIV='Y'");
            if (apptime_bg != "null" && apptime_bg != "NaN")
            {
                sql += " AND to_char(b.apl_contime,'yyyymmdd') >= Substr(:p1, 1, 8) ";
                p.Add(":p1", apptime_bg);
            }
            if (apptime_ed != "null" && apptime_ed != "NaN")
            {
                sql += " AND to_char(b.apl_contime,'yyyymmdd') <= Substr(:p2, 1, 8) ";
                p.Add(":p2", apptime_ed);
            }

            sql += " group by c.mmcode ) b " +
                "where a.wh_no = WHNO_ME1 and a.mmcode = b.mmcode order by a.mmcode  ";
            var result = DBWork.Connection.Query<M>(sql, p);
            return result;
        }


        public DataTable GetExcel(string ts, bool isHospCode0)
        {
            var ss = ts.Split(',');
            var apptime_bg = ss[0];
            var apptime_ed = ss[1];

            var p = new DynamicParameters();
            // DataTable dt = new DataTable();
            //var sql = @"select c.mmcode as 院內碼,c.mmname_e as 英文品名,d.agen_namee as 廠商英文名稱,a.towh as 入庫庫房代碼,e.wh_no||'-'||e.wh_name as 庫房名稱,b.apvqty　as 核發數量
            //            from me_docm a, me_docd b, mi_mast c, ph_vender d,mi_whmast e where A.DOCNO = b.docno and b.mmcode = c.mmcode and c.m_agenno = d.agen_no
            //            and a.towh = e.wh_no and a.doctype = 'MS'and c.E_DRUGAPLTYPE = '1' ";
            //--and a.flowid = '0699'
            var sql = string.Format(@"
                        select c.mmcode as 院內碼,c.mmname_e as 英文品名,d.agen_namee as 廠商英文名稱,a.towh as 入庫庫房代碼,e.wh_no||'-'||e.wh_name as 庫房名稱,b.apvqty　as 核發數量 
                        from mi_mast c 
                        left join me_docd b on b.mmcode = c.mmcode 
                        left join me_docm a on a.docno = b.docno 
                        left join ph_vender d on d.agen_no = c.m_agenno
                        left join mi_whmast e on e.wh_no = a.towh 
                        where 1 = 1 
                        and a.doctype in ('MS', 'MR') 
                        {0}  
                        and a.frwh = WHNO_ME1 
                        and a.flowid in ('0102', '0602','0111','0611')
                        and b.postid is null"
            , isHospCode0 ? "and c.e_drugapltype = '1' " : "and c.isIV='Y'");
            if (apptime_bg != "null")
            {
                sql += " AND to_char(b.apl_contime,'yyyy-mm-dd') >= Substr(:p1, 1, 10) ";
                p.Add(":p1", apptime_bg);
            }
            if (apptime_ed != "null")
            {
                sql += " AND to_char(b.apl_contime,'yyyy-mm-dd') <= Substr(:p2, 1, 10) ";
                p.Add(":p2", apptime_ed);
            }

            sql += "order by 廠商英文名稱,院內碼";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<ME_DOCM> GetAll(string apptime_bg, string apptime_ed, int page_index, int page_size, string sorters, string wh_userId, bool isHospCode0)
        {
            var p = new DynamicParameters();

            var sql = string.Format(@"
                        select c.mmcode as MMCODE, 
                               c.mmname_e as MMNAME_E,
                               d.agen_namee as AGEN_NAMEE,
                               a.towh as TOWH,
                               e.wh_name as WH_NAME,
                               b.apvqty　as APVQTY,
                               a.docno
                          from me_docm a, me_docd b, mi_mast c, ph_vender d, mi_whmast e 
                         where A.DOCNO = b.docno and b.mmcode = c.mmcode and c.m_agenno = d.agen_no
                           and a.towh = e.wh_no and a.doctype in ('MS', 'MR') 
                           {0} 
                           and a.frwh = WHNO_ME1 
                           and a.flowid in ('0102', '0602','0111','0611')
                           and b.postid is null"
            , isHospCode0 ? "and c.E_DRUGAPLTYPE = '1'" : "and c.isIV='Y'");


            if (apptime_bg != "")
            {
                sql += " AND to_char(b.apl_contime,'yyyy-mm-dd') >= Substr(:p1, 1, 10) ";
                p.Add(":p1", apptime_bg);
            }
            if (apptime_ed != "")
            {
                sql += " AND to_char(b.apl_contime,'yyyy-mm-dd') <= Substr(:p2, 1, 10) ";
                p.Add(":p2", apptime_ed);
            }

            sql += "order by agen_namee, MMCODE";
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCM>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>


        public string GetHospCode() {
            string sql = @"
                select data_value from PARAM_D where grp_code='HOSP_INFO' and data_name='HospCode'
            ";

            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }
    }
}