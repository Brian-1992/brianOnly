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
    public class AB0085Repository : JCLib.Mvc.BaseRepository
    {
        public AB0085Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public class M
        {
            public string 院內碼 { get; set; }
            public string 英文品名 { get; set; }
            public string 廠商英文名稱 { get; set; }
            public string 入庫庫房代碼 { get; set; }
            public string 庫房名稱 { get; set; }
            public string 日期起 { get; set; }
            public string 日期迄 { get; set; }
            public string 開帳日期 { get; set; }
            public string 關帳日期 { get; set; }
            public int 核發數量 { get; set; }
        }

        public IEnumerable<M> GetMTotal(string ts, string wh_no, bool isHospCode0)
        {
            var ss = ts.Split(',');
            var apptime_bg = ss[0];
            var apptime_ed = ss[1];
            // apptime_bg = apptime_bg.ToString("yyyyMMdd");
            var p = new DynamicParameters();

            var sql = string.Empty;
            if (isHospCode0)
            {
                sql = @"select c.mmcode as 院內碼,
                               c.mmname_e as 英文品名,
                               d.agen_namee as 廠商英文名稱,
                               a.towh as 入庫庫房代碼,
                               e.wh_name as 庫房名稱,
                               b.apvqty as 核發數量
                          from me_docm a, me_docd b, mi_mast c, ph_vender d,mi_whmast e
                         where a.DOCNO = b.docno
                           and b.mmcode = c.mmcode
                           and c.m_agenno = d.agen_no
                           and a.towh = e.wh_no
                           and a.doctype = 'MS'
                      --     and a.flowid = '0699'
                      --     and c.E_DRUGAPLTYPE = '2'
                           and twn_date(a.post_time) between :apptime_bg and :apptime_ed
                           ";
            }
            else {
                sql= @"select c.mmcode as 院內碼,
                               c.mmname_e as 英文品名,
                               d.agen_namee as 廠商英文名稱,
                               a.towh as 入庫庫房代碼,
                               e.wh_name as 庫房名稱,
                               b.apvqty as 核發數量
                          from me_docm a, me_docd b, mi_mast c, ph_vender d,mi_whmast e
                         where a.DOCNO = b.docno
                           and b.mmcode = c.mmcode
                           and c.m_agenno = d.agen_no
                           and a.towh = e.wh_no
                           and a.doctype in ('MR','MR5','MR6')
                           and c.mmname_c like '%酒精%'
                           and c.mmname_c not like '%酒精棉片%'
                           and a.flowid in ('0199','6')
                           and twn_date(a.post_time) between :apptime_bg and :apptime_ed
                           ";
            }

            p.Add(":apptime_bg", apptime_bg);
            p.Add(":apptime_ed", apptime_ed);

            if (wh_no != string.Empty)
            {
                sql += "  and a.towh = :wh_no ";
                p.Add(":wh_no", wh_no);
            }

            sql += " ORDER BY 院內碼 ";
            var result = DBWork.Connection.Query<M>(sql, p);
            return result;
        }
        public DataTable GetExcel(string ts, string wh_no, bool isHospCode0)
        {
            var ss = ts.Split(',');
            var apptime_bg = ss[0];
            var apptime_ed = ss[1];

            var p = new DynamicParameters();
            var sql = string.Empty;
            //if (isHospCode0)
            //{
            //    sql = @"select c.mmcode as 院內碼,
            //                   c.mmname_e as 英文品名,
            //                   d.agen_namee as 廠商英文名稱,
            //                   a.towh as 入庫庫房代碼,
            //                   e.wh_name as 庫房名稱,
            //                   b.apvqty as 核發數量
            //              from me_docm a, me_docd b, mi_mast c, ph_vender d,mi_whmast e
            //             where a.DOCNO = b.docno
            //               and b.mmcode = c.mmcode
            //               and c.m_agenno = d.agen_no
            //               and a.towh = e.wh_no
            //               and a.doctype = 'MS'
            //               and a.flowid = '0699'
            //               and c.E_DRUGAPLTYPE = '2'
            //               and twn_date(a.post_time) between :apptime_bg and :apptime_ed
            //               ";
            //}
            //else {
            //    sql = @"select c.mmcode as 院內碼,
            //                   c.mmname_e as 英文品名,
            //                   d.agen_namee as 廠商英文名稱,
            //                   a.towh as 入庫庫房代碼,
            //                   e.wh_name as 庫房名稱,
            //                   b.apvqty as 核發數量
            //              from me_docm a, me_docd b, mi_mast c, ph_vender d,mi_whmast e
            //             where a.DOCNO = b.docno
            //               and b.mmcode = c.mmcode
            //               and c.m_agenno = d.agen_no
            //               and a.towh = e.wh_no
            //               and a.doctype in ('MR','MR5','MR6')
            //               and c.mmname_c like '%酒精%'
            //               and c.mmname_c not like '%酒精棉片%'
            //               and a.flowid in ('0199','6')
            //               and twn_date(a.post_time) between :apptime_bg and :apptime_ed
            //               ";
            //}

            if (isHospCode0)
            {
                sql = @"select c.mmcode,c.mmname_e,d.agen_namee,a.towh,e.wh_name,b.apvqty
                          from me_docm a, me_docd b, mi_mast c, ph_vender d,mi_whmast e
                         where a.DOCNO = b.docno
                           and b.mmcode = c.mmcode
                           and c.m_agenno = d.agen_no
                           and a.towh = e.wh_no
                           and a.doctype = 'MS'
                     --      and a.flowid = '0699'
                     --      and c.E_DRUGAPLTYPE = '2'
                           and twn_date(a.post_time) between :apptime_bg and :apptime_ed
                           ";
            }
            else
            {
                sql = @"select 
                            c.mmcode  as 院內碼,   
                            c.mmname_e  as 英文品名,
                            d.agen_namee  as 廠商英文名稱,   
                            a.towh  as 入庫庫房代碼,
                            e.wh_name  as 庫房名稱,
                            b.apvqty  as 核發數量
                          from me_docm a, me_docd b, mi_mast c, ph_vender d,mi_whmast e
                         where a.DOCNO = b.docno
                           and b.mmcode = c.mmcode
                           and c.m_agenno = d.agen_no
                           and a.towh = e.wh_no
                           and a.doctype in ('MR','MR5','MR6')
                           and c.mmname_c like '%酒精%'
                           and c.mmname_c not like '%酒精棉片%'
                           and a.flowid in ('0199','6')
                           and twn_date(a.post_time) between :apptime_bg and :apptime_ed
                           ";
            }



            p.Add(":apptime_bg", apptime_bg);
            p.Add(":apptime_ed", apptime_ed);

            if (wh_no != string.Empty)
            {
                sql += "  and a.towh = :wh_no ";
                p.Add(":wh_no", wh_no);
            }

            sql += " ORDER BY c.MMCODE ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<ME_DOCM> GetAll(string apptime_bg, string apptime_ed, string wh_no, int page_index, int page_size, string sorters, string wh_userId, bool isHospCode)
        {
            var p = new DynamicParameters();

            var sql = string.Empty;
            if (isHospCode)
            {
                sql = @"select c.mmcode,c.mmname_e,d.agen_namee,a.towh,e.wh_name,b.apvqty
                          from me_docm a, me_docd b, mi_mast c, ph_vender d,mi_whmast e
                         where a.DOCNO = b.docno
                           and b.mmcode = c.mmcode
                           and c.m_agenno = d.agen_no
                           and a.towh = e.wh_no
                           and a.doctype = 'MS'
                     --      and a.flowid = '0699'
                     --      and c.E_DRUGAPLTYPE = '2'
                           and twn_date(a.post_time) between :apptime_bg and :apptime_ed
                           ";
            }
            else {
                sql = @"select c.mmcode,c.mmname_e,d.agen_namee,a.towh,e.wh_name,b.apvqty
                          from me_docm a, me_docd b, mi_mast c, ph_vender d,mi_whmast e
                         where a.DOCNO = b.docno
                           and b.mmcode = c.mmcode
                           and c.m_agenno = d.agen_no
                           and a.towh = e.wh_no
                           and a.doctype in ('MR','MR5','MR6')
                           and c.mmname_c like '%酒精%'
                           and c.mmname_c not like '%酒精棉片%'
                           and a.flowid in ('0199','6')
                           and twn_date(a.post_time) between :apptime_bg and :apptime_ed
                           ";
            }

            
            p.Add(":apptime_bg", apptime_bg);
            p.Add(":apptime_ed", apptime_ed);

            if (wh_no != string.Empty) {
                sql += "  and a.towh = :wh_no ";
                p.Add(":wh_no", wh_no);
            }

            sql += " ORDER BY MMCODE ";
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
        public IEnumerable<string> SET_BTIME(string START_DATE)
        {
            var sql = @"SELECT TO_CHAR (MMT.SET_BTIME, 'YYYYMMDD') - 19110000 SET_BTIME
                        FROM MI_MNSET MMT
                        WHERE MMT.SET_YM = :START_DATE";

            return DBWork.Connection.Query<string>(sql, new { START_DATE = START_DATE }, DBWork.Transaction);
        }

        public IEnumerable<string> SET_ETIME(string END_DATE)
        {
            var sql = @"SELECT TO_CHAR (MMT.SET_ETIME, 'YYYYMMDD') - 19110000 SET_ETIME
                        FROM MI_MNSET MMT
                        WHERE MMT.SET_YM = :END_DATE";

            return DBWork.Connection.Query<string>(sql, new { END_DATE = END_DATE }, DBWork.Transaction);
        }

        public IEnumerable<MI_WHMAST> GetWhnos(string queryString)
        {
            var p = new DynamicParameters();

            string hospCode = GetHospCode();


            var sql = @"select {0} a.wh_no, 
                                  a.wh_name
                          from MI_WHMAST a
                         where 1=1 
                           and a.wh_kind = '0' and a.wh_grade not in ('1','5')";

            if (hospCode != "0")
            {
                sql = @"select {0} a.wh_no, 
                                  a.wh_name
                          from MI_WHMAST a
                         where 1=1 ";
            }


            if (queryString != "")
            {
                sql = string.Format(sql, "(nvl(instr(a.wh_no, :wh_no), 1000) + nvl(instr(a.wh_name, :wh_name), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
                p.Add(":wh_no", queryString);
                p.Add(":wh_name", queryString);

                sql += " and (a.wh_no like :wh_no ";
                p.Add(":wh_no", string.Format("%{0}%", queryString));

                sql += " or a.wh_name like :wh_name )";
                p.Add(":wh_name", string.Format("%{0}%", queryString));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY A.wh_no ";
            }

            return DBWork.PagingQuery<MI_WHMAST>(sql, p, DBWork.Transaction);
        }

        public string GetHospCode() {
            string sql = @"
                select data_value from PARAM_D where grp_code='HOSP_INFO' and data_name='HospCode'
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }

    }
}