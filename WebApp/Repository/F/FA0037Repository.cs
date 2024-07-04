using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using TSGH.Models;
using WebApp.Models.F;

namespace WebApp.Repository.F
{
    public class FA0037_MODEL : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; }
        public string AGEN_NO { get; set; }
        public string AGEN_NAMEC { get; set; }
        public string AGEN_TEL { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string M_PHCTNCO { get; set; }
        public string M_ENVDT { get; set; }
        public string M_AGENLAB { get; set; }
        public string M_PURUN { get; set; }
        public string REQ_QTY_T { get; set; }
        public string M_CONTPRICE { get; set; }
        public string TOT { get; set; }
        public string DISC { get; set; }
        public string UNIT_SWAP { get; set; }
        public string BASE_UNIT { get; set; }
        public string APPDATA { get; set; }
        public string CNT { get; set; }

        public string ISCR { get; set; }

    }
    public class FA0037Repository : JCLib.Mvc.BaseRepository
    {
        public FA0037Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ComboModel> GetMatClass()
        {
            string sql = @"Select MAT_CLASS as VALUE ,mat_class||' '||mat_clsname as COMBITEM from mi_matclass Where mat_clsid in ('2','3','6')";
            return DBWork.Connection.Query<ComboModel>(sql, DBWork.Transaction);
        }

        public IEnumerable<FA0037> GetAll(string bgdate, string endate, string mclass, string ym, string gb, string cont, string xaction, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select a.pr_no, a.mmcode, b.mmname_e, b.mmname_c, a.agen_no, (select agen_namec from PH_VENDER where agen_no=a.agen_no) agen_namec,
                        (select agen_tel from PH_VENDER where agen_no=a.agen_no) agen_tel,a.pr_qty,
                        a.m_purun, m_agenlab, a.m_contprice, m_phctnco, m_envdt, a.disc, a.m_contid, c.m_storeid, a.req_qty_t, floor(a.m_contprice*a.req_qty_t) tot,
                        a.unit_swap,
                        (case when c.create_user = '緊急醫療出貨' then '是' else '否' end) as iscr
                        from  mm_pr_d a, mi_mast b, mm_pr_m c
                        where a.pr_no=c.pr_no and a.mmcode=b.mmcode and a.M_CONTID<>'3' ";

            if (bgdate != "null" && bgdate != "NaN")
            {
                sql += " AND twn_date(c.PR_TIME) >= :p1 ";
                p.Add(":p1", bgdate);
            }
            if (endate != "null" && endate != "NaN")
            {
                sql += " AND twn_date(c.PR_TIME) <= :p2 ";
                p.Add(":p2", endate);
            }

            if (mclass != "" && mclass != "null")
            {
                sql += " AND c.MAT_CLASS  = :p3 ";
                p.Add(":p3", mclass);
            }

            //if (ym != "" && ym != "null")
            //{
            //    sql += " AND A.MMCODE >=   = :p4 ";
            //    p.Add(":p4", ym);
            //}


            if (gb != "" && gb != "null" && gb != "全部")
            {
                sql += " AND c.m_storeid = :p5 ";
                p.Add(":p5", gb);
            }

            if (cont != "" && cont != "null" && cont != "全部")
            {
                sql += " AND a.m_contid   = :p6 ";
                p.Add(":p6", cont);
            }

            if (xaction != "" && xaction != "null")
            {
                sql += " AND c.xaction   = :p7 ";
                p.Add(":p7", xaction);
            }

            sql += " order by  a.agen_no, a.disc, a.mmcode ";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<FA0037>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public DataTable GetExcel(string MAT_CLASS, string DIS_TIME_B, string DIS_TIME_E, string YM, string GB, string CONT, string xaction)
        {
            var p = new DynamicParameters();

            var sql = @"select  a.mmcode 院內碼, b.mmname_e 英文品名, b.mmname_c 中文品名, a.agen_no 廠商代碼, (select agen_namec from PH_VENDER where agen_no=a.agen_no) 廠商名稱,
                        a.m_contprice 合約價, a.disc 折讓比, m_phctnco 環保證號,a.req_qty_t 申請量,case when a.m_contid ='0' then '合約' when a.m_contid ='2' then '非合約' end　as 是否合約, case when c.m_storeid ='0' then '非庫備' when c.m_storeid ='1' then '庫備' end as 是否庫備,
                       floor(a.m_contprice*a.req_qty_t) 本月總價,a.pr_no 申購單編號 ,
                       a.unit_swap as 轉換率, a.m_purun as 申購單位,
                       (case when c.create_user = '緊急醫療出貨' then '是' else '否' end) as 緊急醫療出貨
                       from mm_pr_d a, mi_mast b, mm_pr_m c
                       where a.pr_no = c.pr_no and a.mmcode = b.mmcode and a.M_CONTID <> '3' ";

            if (DIS_TIME_B != "null" && DIS_TIME_B != "NaN")
            {
                sql += " AND twn_date(c.PR_TIME) >= :p1 ";
                p.Add(":p1", DIS_TIME_B);
            }
            if (DIS_TIME_E != "null" && DIS_TIME_E != "NaN")
            {
                sql += " AND twn_date(c.PR_TIME) <= :p2 ";
                p.Add(":p2", DIS_TIME_E);
            }

            if (MAT_CLASS != "" && MAT_CLASS != "null")
            {
                sql += " AND c.MAT_CLASS  = :p3 ";
                p.Add(":p3", MAT_CLASS);
            }

            //if (ym != "" && ym != "null")
            //{
            //    sql += " AND A.MMCODE >=   = :p4 ";
            //    p.Add(":p4", ym);
            //}


            if (GB != "" && GB != "null" && GB != "全部")
            {
                sql += " AND c.m_storeid = :p5 ";
                p.Add(":p5", GB);
            }

            if (CONT != "" && CONT != "null" && CONT != "全部")
            {
                sql += " AND a.m_contid   = :p6 ";
                p.Add(":p6", CONT);
            }
            if (xaction != "" && xaction != "null")
            {
                sql += " AND c.xaction   = :p7 ";
                p.Add(":p7", xaction);
            }


            sql += " order by  a.agen_no, a.disc, a.mmcode ";
            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
        //匯出非庫備申請單
        public DataTable GetExcel2(string MAT_CLASS, string DIS_TIME_B, string DIS_TIME_E, string YM, string GB, string CONT, string xaction)
        {
            var p = new DynamicParameters();

            var sql = @" select  a.pr_no 申購單編號, a.mmcode 院內碼, a.agen_no 廠商代碼,
                              d.agen_namec 廠商名稱, d.agen_tel 廠商電話號碼,
                              c.mmname_e 英文品名, c.mmname_c 中文品名, c.base_unit 庫存計量單位, 
                              (select NVL(INV_QTY,0) from MI_WHINV where wh_no=y.frwh and mmcode=a.mmcode) 庫存量, 
                              a.m_purun 申購包裝單位, a.req_qty_t 總申購量, a.unit_swap 轉換率, 
                              c.m_agenlab 廠牌, a.m_contprice 合約價, floor(a.m_contprice*a.req_qty_t) 合約申購總額, a.disc 折讓比,
                              y.towh 需求責任中心, e.inid_name 需求單位名稱, b.appqty 單位申請量,           
                              case when a.m_contid ='0' then '合約' when a.m_contid ='2' then '非合約' end 是否合約,
                              (case when x.create_user = '緊急醫療出貨' then '是' else '否' end) as 緊急醫療出貨
                         from  MM_PR_M x, MM_PR_D a,  ME_DOCM y, ME_DOCD b, mi_mast c, ph_vender d, ur_inid e
                         where x.pr_no=a.pr_no 
                            and a.mmcode=c.mmcode and a.agen_no=d.agen_no
                            and y.docno=b.docno and x.pr_no=b.rdocno  and a.mmcode=b.mmcode  and y.towh=e.inid
                            and x.m_storeid='0' and a.M_CONTID <> '3' 
                        ";
            if (DIS_TIME_B != "null" && DIS_TIME_B != "NaN")
            {
                sql += " AND twn_date(x.PR_TIME) >= :p1 ";
                p.Add(":p1", DIS_TIME_B);
            }
            if (DIS_TIME_E != "null" && DIS_TIME_E != "NaN")
            {
                sql += " AND twn_date(x.PR_TIME) <= :p2 ";
                p.Add(":p2", DIS_TIME_E);
            }

            if (MAT_CLASS != "" && MAT_CLASS != "null")
            {
                sql += " AND x.MAT_CLASS  = :p3 ";
                p.Add(":p3", MAT_CLASS);
            }

            if (CONT != "" && CONT != "null" && CONT != "全部")
            {
                sql += " AND a.m_contid   = :p6 ";
                p.Add(":p6", CONT);
            }
            if (xaction != "" && xaction != "null")
            {
                sql += " AND x.xaction   = :p7 ";
                p.Add(":p7", xaction);
            }
            sql += " order by  a.pr_no, a.mmcode, y.towh ";
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

        public IEnumerable<FA0037_MODEL> GetReport(string MAT_CLASS, string DIS_TIME_B, string DIS_TIME_E, string GB, string CONT, string xaction)
        {
            var p = new DynamicParameters();
            string sql = @"select a.mmcode, b.mmname_e ,m_agenlab,m_envdt, b.mmname_c , a.agen_no ,(select agen_tel from PH_VENDER where agen_no=a.agen_no) agen_tel,a.m_purun, (select agen_namec from PH_VENDER where agen_no = a.agen_no) agen_namec,
                        a.m_contprice, a.disc , m_phctnco,a.req_qty_t,floor(a.m_contprice * a.req_qty_t) tot 
                        from  mm_pr_d a, mi_mast b, mm_pr_m c
                        where a.pr_no=c.pr_no and a.mmcode=b.mmcode and a.M_CONTID<>'3' ";

            if (DIS_TIME_B != "null")
            {
                sql += " AND twn_date(c.PR_TIME) >= :p1 ";
                p.Add(":p1", DIS_TIME_B);
            }
            if (DIS_TIME_E != "null")
            {
                sql += " AND twn_date(c.PR_TIME) <= :p2 ";
                p.Add(":p2", DIS_TIME_E);
            }

            if (MAT_CLASS != "null")
            {
                sql += " AND c.MAT_CLASS = :p3 ";
                p.Add(":p3", MAT_CLASS);
            }

            if (GB != "" && GB != "null" && GB != "全部")
            {
                sql += " AND c.m_storeid = :p5 ";
                p.Add(":p5", GB);
            }

            if (CONT != "" && CONT != "null" && CONT != "全部")
            {
                sql += " AND a.m_contid = :p6 ";
                p.Add(":p6", CONT);
            }
            if (xaction != "" && xaction != "null")
            {
                sql += " AND c.xaction   = :p7 ";
                p.Add(":p7", xaction);
            }


            sql += " order by a.agen_no,a.disc,a.mmcode ";

            return DBWork.Connection.Query<FA0037_MODEL>(sql, p, DBWork.Transaction);
        }
        public IEnumerable<FA0037_MODEL> GetReport2(string MAT_CLASS, string DIS_TIME_B, string DIS_TIME_E, string GB, string CONT, string xaction)
        {
            var p = new DynamicParameters();
            string sql = @"select a.mmcode, b.mmname_e ,m_agenlab, b.mmname_c , a.agen_no , c.inv_qty,
                           a.m_purun,b.base_unit, (select agen_namec from PH_VENDER where agen_no = a.agen_no) agen_namec,
                           a.m_contprice, a.req_qty_t, floor(a.m_contprice * a.req_qty_t) tot, a.unit_swap,
                            (case when d.create_user = '緊急醫療出貨' then '緊急醫療出貨' else ' ' end)  as iscr,
                           (select listagg(APPDATA, '; ') within group (order by RDOCNO,mmcode) from V_APP_DATA where RDOCNO=a.pr_no and mmcode=a.mmcode ) APPDATA
                           from  mm_pr_d a, mi_mast b, (select mmcode, inv_qty from MI_WHINV where wh_no=WHNO_MM1) c, mm_pr_m d
                           where  a.pr_no=d.pr_no and a.mmcode = b.mmcode  and a.mmcode=c.mmcode(+) and a.M_CONTID<>'3' ";

            if (DIS_TIME_B != "null")
            {
                sql += " AND twn_date(d.PR_TIME) >= :p1 ";
                p.Add(":p1", DIS_TIME_B);
            }
            if (DIS_TIME_E != "null")
            {
                sql += " AND twn_date(d.PR_TIME) <= :p2 ";
                p.Add(":p2", DIS_TIME_E);
            }

            if (MAT_CLASS != "null")
            {
                sql += " AND d.MAT_CLASS = :p3 ";
                p.Add(":p3", MAT_CLASS);
            }

            if (GB != "" && GB != "null" && GB != "全部")
            {
                sql += " AND d.m_storeid = :p5 ";
                p.Add(":p5", GB);
            }

            if (CONT != "" && CONT != "null" && CONT != "全部")
            {
                sql += " AND a.m_contid = :p6 ";
                p.Add(":p6", CONT);
            }
            if (xaction != "" && xaction != "null")
            {
                sql += " AND d.xaction   = :p7 ";
                p.Add(":p7", xaction);
            }
            sql += " order by a.agen_no,a.mmcode ";

            return DBWork.Connection.Query<FA0037_MODEL>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<FA0037_MODEL> GetReport3(string MAT_CLASS, string DIS_TIME_B, string DIS_TIME_E, string GB, string CONT, string xaction)
        {
            var p = new DynamicParameters();
            string sql = @"SELECT  A.AGEN_NO , C.AGEN_NAMEC, COUNT(MMCODE) CNT,
                             SUM(floor(A.M_CONTPRICE*A.REQ_QTY_T)) TOT
                           FROM MM_PR_D A, MM_PR_M B, PH_VENDER C
                           WHERE A.PR_NO = B.PR_NO AND A.AGEN_NO = C.AGEN_NO and a.M_CONTID<>'3'";

            if (DIS_TIME_B != "null")
            {
                sql += " AND twn_date(B.PR_TIME) >= :p1 ";
                p.Add(":p1", DIS_TIME_B);
            }
            if (DIS_TIME_E != "null")
            {
                sql += " AND twn_date(B.PR_TIME) <= :p2 ";
                p.Add(":p2", DIS_TIME_E);
            }

            if (MAT_CLASS != "null")
            {
                sql += " AND B.MAT_CLASS = :p3 ";
                p.Add(":p3", MAT_CLASS);
            }

            if (GB != "" && GB != "null" && GB != "全部")
            {
                sql += " AND m_storeid = :p5 ";
                p.Add(":p5", GB);
            }

            if (CONT != "" && CONT != "null" && CONT != "全部")
            {
                sql += " AND a.m_contid = :p6 ";
                p.Add(":p6", CONT);
            }
            if (xaction != "" && xaction != "null")
            {
                sql += " AND b.xaction   = :p7 ";
                p.Add(":p7", xaction);
            }
            sql += " group by a.agen_no , c.agen_namec    order by a.agen_no ";

            return DBWork.Connection.Query<FA0037_MODEL>(sql, p, DBWork.Transaction);
        }

        //public DataTable GetReport(string MAT_CLASS, string DIS_TIME_B, string DIS_TIME_E, string YM, string GB, string CONT)
        //{
        //    var p = new DynamicParameters();
        //    string sql = @"select a.mmcode, mmname_e ,m_agenlab,m_envdt, mmname_c , a.agen_no ,(select agen_tel from PH_VENDER where agen_no=a.agen_no) agen_tel,a.m_purun, (select agen_namec from PH_VENDER where agen_no = a.agen_no) agen_namec,
        //                a.m_contprice, a.disc , m_phctnco,a.req_qty_t,(a.m_contprice * a.req_qty_t) tot from mm_pr_d a, mi_mast b where a.mmcode = b.mmcode ";

        //    if (DIS_TIME_B != "null" && DIS_TIME_B != "NaN")
        //    {
        //        sql += " AND substr(a.pr_no,7,7) >= :p1 ";
        //        p.Add(":p1", DIS_TIME_B);
        //    }
        //    if (DIS_TIME_E != "null" && DIS_TIME_E != "NaN")
        //    {
        //        sql += " AND substr(a.pr_no,7,7) <= :p2 ";
        //        p.Add(":p2", DIS_TIME_E);
        //    }

        //    if (MAT_CLASS != "" && MAT_CLASS != "null")
        //    {
        //        sql += " AND MAT_CLASS  = :p3 ";
        //        p.Add(":p3", MAT_CLASS);
        //    }

        //    //if (ym != "" && ym != "null")
        //    //{
        //    //    sql += " AND A.MMCODE >=   = :p4 ";
        //    //    p.Add(":p4", ym);
        //    //}


        //    if (GB != "" && GB != "null" && GB != "全部")
        //    {
        //        sql += " AND m_storeid = :p5 ";
        //        p.Add(":p5", GB);
        //    }

        //    if (CONT != "" && CONT != "null" && CONT != "全部")
        //    {
        //        sql += " AND a.m_contid   = :p6 ";
        //        p.Add(":p6", CONT);
        //    }



        //    sql += " order by  a.agen_no, a.disc, a.mmcode ";

        //    DataTable dt = new DataTable();
        //    using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
        //        dt.Load(rdr);

        //    return dt;
        //}

    }
}