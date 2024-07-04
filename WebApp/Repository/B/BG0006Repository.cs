using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.BG
{
    public class BG0006Repository : JCLib.Mvc.BaseRepository
    {
        public BG0006Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<BG0006M> GetAll(string MAT_CLASS, string ACC_TIME_B, string M_CONTID, string ACC_TIME_E, string MMCODE, int page_index, int page_size, string sorters)
        {
            //進貨金額 = (進貨數量 * 合約價)無條件捨去, 支付金額 = (進貨數量 * 優惠合約價)無條件捨去, 
            //折讓金額 = 進貨金額 - 支付金額
            //配合合約價 -> 進貨數量 = ACC_QTY/UNIT_SWAP  
            var p = new DynamicParameters();
            var sql = @"    SELECT 
                                A.PO_NO, --訂單編號,
                                A.MMCODE, --院內碼,
                                B.MMNAME_C, --中文品名,
                                B.MMNAME_E, --英文品名,
                                D.M_PURUN, --包裝單位,
                                A.ACC_QTY/a.UNIT_SWAP as ACC_QTY, --進貨數量,
                                D.PO_PRICE, --合約價,
                                A.LOT_NO, TWN_DATE(A.EXP_DATE) EXP_DATE,
                                --floor(D.PO_PRICE * ACC_QTY/a.UNIT_SWAP) AS PO_AMT, --進貨金額
                                nvl(D.DISC_CPRICE,D.PO_PRICE) DISC_CPRICE, --優惠合約價,
                                --floor(D.DISC_CPRICE * ACC_QTY/a.UNIT_SWAP)  AS PAY_AMOUNT, --支付金額,
                                D.M_DISCPERC, --折讓比,
                                --(floor(D.PO_PRICE * ACC_QTY/a.UNIT_SWAP) - floor(D.DISC_CPRICE * ACC_QTY/a.UNIT_SWAP) ) AS DISC_AMOUNT, --折讓金額,                                
                                TWN_DATE(A.ACC_TIME) AS ACC_TIME, --進貨日期,
                                A.AGEN_NO, --廠商碼,
                                P.AGEN_NAMEC, --廠商名稱,
                                B.M_PHCTNCO, --衛署證號,
                                (case when B.M_MATID = 'Y' then '是' when B.M_MATID = 'N' then '否' end) AS M_MATID --聯標否
                            FROM
                                BC_CS_ACC_LOG A, MI_MAST B, MM_PO_D D, PH_VENDER P, MM_PO_M M
                            WHERE 
                                D.PO_NO=M.PO_NO
                                AND substr(a.po_no,1,3) in ('INV','GEN','SML')
                                AND A.MMCODE = B.MMCODE
                                AND A.PO_NO = D.PO_NO
                                AND A.MMCODE = D.MMCODE
                                AND A.AGEN_NO = P.AGEN_NO
                            ";

            if (!string.IsNullOrWhiteSpace(MAT_CLASS))
            {
                sql += " AND B.MAT_CLASS = :p0 ";
                p.Add(":p0", string.Format("{0}", MAT_CLASS));
            }

            if (!string.IsNullOrWhiteSpace(ACC_TIME_B))
            {
                sql += " AND trunc(ACC_TIME) >= to_date(substr(:p1, 0, 10), 'yyyy/mm/dd') ";
                p.Add(":p1", string.Format("{0}", ACC_TIME_B));
            }

            if (!string.IsNullOrWhiteSpace(ACC_TIME_E))
            {
                sql += " AND trunc(ACC_TIME) <= to_date(substr(:p4, 0, 10), 'yyyy/mm/dd') ";
                p.Add(":p4", string.Format("{0}", ACC_TIME_E));
            }

            if (!string.IsNullOrWhiteSpace(M_CONTID))
            {
                sql += " AND M.M_CONTID = :p2 ";
                p.Add(":p2", string.Format("{0}", M_CONTID));
            }
            if (!string.IsNullOrWhiteSpace(MMCODE))
            {
                sql += " AND A.MMCODE = :p5 ";
                p.Add(":p5", string.Format("{0}", MMCODE));
            }
            sql += "ORDER BY A.AGEN_NO, A.MMCODE ";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<BG0006M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<BG0006M> GetAll_2(string MAT_CLASS, string ACC_TIME_B, string M_CONTID, string ACC_TIME_E, string MMCODE, int page_index, int page_size, string sorters)
        {   //進貨彙總
            //進貨金額 = (進貨數量 * 合約價)無條件捨去, 支付金額 = (進貨數量 * 優惠合約價)無條件捨去, 
            //折讓金額 = 進貨金額 - 支付金額
            //配合合約價 -> 進貨數量 = ACC_QTY/UNIT_SWAP  
            //20200918 group by 刪除 PO_NO   --汪佳蓉
            var p = new DynamicParameters();
            var sql = @"    SELECT 
                                x.MMCODE, --院內碼,
                                y.MMNAME_C, --中文品名,
                                y.MMNAME_E, --英文品名,
                                x.M_PURUN, --包裝單位,
                                x.sumqty as ACC_QTY, --進貨數量,
                                x.PO_PRICE, --合約價,
                                floor(x.PO_PRICE * x.sumqty) AS PO_AMT, --進貨金額
                                nvl(x.DISC_CPRICE,x.PO_PRICE) DISC_CPRICE, --優惠合約價,
                                floor(x.DISC_CPRICE * x.sumqty)  AS PAY_AMOUNT, --支付金額,
                                x.M_DISCPERC, --折讓比,
                                (floor(x.PO_PRICE * x.sumqty) - floor(x.DISC_CPRICE * x.sumqty) ) AS DISC_AMOUNT, --折讓金額,                                
                                x.ACC_TIME, --進貨日期,
                                x.AGEN_NO, --廠商碼,
                                x.AGEN_NAMEC, --廠商名稱,
                                y.M_PHCTNCO, --衛署證號,
                                (case when y.M_MATID = 'Y' then '是' when y.M_MATID = 'N' then '否' end) AS M_MATID --聯標否
                            FROM 
                            ( select  twn_date(acc_time) acc_time, a.agen_no, agen_namec, 
                                 a.m_contid, a.mat_class, m_discperc, c.mmcode, disc_cprice, po_price, b.M_PURUN,
                                 sum(c.acc_qty/b.unit_swap) as sumqty
                              from MM_PO_M a, MM_PO_D b, BC_CS_ACC_LOG c, PH_VENDER p     
                              where a.po_no=b.po_no and a.agen_no=p.agen_no 
                                 and b.po_no=c.po_no and b.mmcode=c.mmcode ";
            if (!string.IsNullOrWhiteSpace(ACC_TIME_B))
            {
                sql += " and trunc(ACC_TIME) >= to_date(substr(:p1, 0, 10), 'yyyy/mm/dd') ";
                p.Add(":p1", string.Format("{0}", ACC_TIME_B));
            }

            if (!string.IsNullOrWhiteSpace(ACC_TIME_E))
            {
                sql += " and trunc(ACC_TIME) <= to_date(substr(:p4, 0, 10), 'yyyy/mm/dd') ";
                p.Add(":p4", string.Format("{0}", ACC_TIME_E));
            }
            if (!string.IsNullOrWhiteSpace(M_CONTID))
            {
                sql += " and a.m_contid = :p2 ";
                p.Add(":p2", string.Format("{0}", M_CONTID));
            }
            if (!string.IsNullOrWhiteSpace(MMCODE))
            {
                sql += " and b.MMCODE = :p5 ";
                p.Add(":p5", string.Format("{0}", MMCODE));
            }
            sql += @"            and c.status='P' 
                                 and acc_qty <> 0 and a.mat_class=:MAT_CLASS    
                                 and substr(a.po_no,1,3) in ('INV','GEN','SML')
                              group by twn_date(acc_time), a.agen_no, agen_namec, a.m_contid, 
                                 a.mat_class, m_discperc, c.mmcode, disc_cprice, po_price, b.M_PURUN   
                             ) x, MI_MAST y
                            WHERE    x.MMCODE = y.MMCODE
                            ";
            sql += "ORDER BY x.AGEN_NO, x.acc_time, x.MMCODE  ";
            p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS));
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<BG0006M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<BG0006M> Print_2(string MAT_CLASS, string ACC_TIME_B, string M_CONTID, string ACC_TIME_E, string MMCODE)
        {
            var p = new DynamicParameters();
            //var sql = @"    SELECT 
            //                    A.MMCODE, --院內碼,
            //                    MMNAME_C, --中文品名,
            //                    MMNAME_E, --英文品名,
            //                    D.M_PURUN, --包裝單位,
            //                    ACC_QTY/a.UNIT_SWAP as ACC_QTY, --進貨數量,
            //                    D.PO_PRICE, --合約價,
            //                    floor(D.PO_PRICE * ACC_QTY/a.UNIT_SWAP) AS PO_AMT, --進貨金額
            //                    nvl(D.DISC_CPRICE,D.PO_PRICE) DISC_CPRICE, --優惠合約價,
            //                    floor(D.DISC_CPRICE * ACC_QTY/a.UNIT_SWAP)  AS PAY_AMOUNT, --支付金額,
            //                    D.M_DISCPERC, --折讓比,
            //                    (floor(D.PO_PRICE * ACC_QTY/a.UNIT_SWAP) - floor(D.DISC_CPRICE * ACC_QTY/a.UNIT_SWAP) ) AS DISC_AMOUNT, --折讓金額,   
            //                    TWN_DATE(ACC_TIME) AS ACC_TIME, --進貨日期,
            //                    A.AGEN_NO, --廠商碼,
            //                    P.AGEN_NAMEC, --廠商名稱,
            //                    B.M_PHCTNCO, --衛署證號,
            //                    (case when B.M_MATID = 'Y' then '是' when B.M_MATID = 'N' then '否' end) AS M_MATID --聯標否
            //                FROM
            //                    BC_CS_ACC_LOG A, MI_MAST B, MM_PO_D D, PH_VENDER P, MM_PO_M M
            //                WHERE 
            //                    D.PO_NO=M.PO_NO
            //                    AND substr(a.po_no,1,3) in ('INV','GEN','SML')
            //                    AND A.MMCODE = B.MMCODE
            //                    AND A.PO_NO = D.PO_NO
            //                    AND A.MMCODE = D.MMCODE
            //                    AND A.AGEN_NO = P.AGEN_NO
            //                ";

            //if (!string.IsNullOrWhiteSpace(MAT_CLASS))
            //{
            //    sql += " AND B.MAT_CLASS = :p0 ";
            //    p.Add(":p0", string.Format("{0}", MAT_CLASS));
            //}

            //if (!string.IsNullOrWhiteSpace(ACC_TIME_B))
            //{
            //    sql += " AND trunc(ACC_TIME) >= to_date(substr(:p1, 0, 10), 'yyyy/mm/dd') ";
            //    p.Add(":p1", string.Format("{0}", ACC_TIME_B));
            //}

            //if (!string.IsNullOrWhiteSpace(ACC_TIME_E))
            //{
            //    sql += " AND trunc(ACC_TIME) <= to_date(substr(:p4, 0, 10), 'yyyy/mm/dd') ";
            //    p.Add(":p4", string.Format("{0}", ACC_TIME_E));
            //}

            //if (!string.IsNullOrWhiteSpace(M_CONTID))
            //{
            //    sql += " AND M.M_CONTID = :p2 ";
            //    p.Add(":p2", string.Format("{0}", M_CONTID));
            //}
            //if (!string.IsNullOrWhiteSpace(MMCODE))
            //{
            //    sql += " AND A.MMCODE = :p5 ";
            //    p.Add(":p5", string.Format("{0}", MMCODE));
            //}
            //sql += "ORDER BY A.AGEN_NO, A.MMCODE ";
            var sql = @"    SELECT 
                                x.MMCODE, --院內碼,
                                y.MMNAME_C, --中文品名,
                                y.MMNAME_E, --英文品名,
                                x.M_PURUN, --包裝單位,
                                x.sumqty as ACC_QTY, --進貨數量,
                                x.PO_PRICE, --合約價,
                                floor(x.PO_PRICE * x.sumqty) AS PO_AMT, --進貨金額
                                nvl(x.DISC_CPRICE,x.PO_PRICE) DISC_CPRICE, --優惠合約價,
                                floor(x.DISC_CPRICE * x.sumqty)  AS PAY_AMOUNT, --支付金額,
                                x.M_DISCPERC, --折讓比,
                                (floor(x.PO_PRICE * x.sumqty) - floor(x.DISC_CPRICE * x.sumqty) ) AS DISC_AMOUNT, --折讓金額,                                
                                x.ACC_TIME, --進貨日期,
                                x.AGEN_NO, --廠商碼,
                                x.AGEN_NAMEC, --廠商名稱,
                                y.M_PHCTNCO, --衛署證號,
                                (case when y.M_MATID = 'Y' then '是' when y.M_MATID = 'N' then '否' end) AS M_MATID --聯標否
                            FROM 
                            ( select  twn_date(acc_time) acc_time, a.agen_no, agen_namec, 
                                 a.m_contid, a.mat_class, m_discperc, c.mmcode, disc_cprice, po_price, b.M_PURUN,
                                 sum(c.acc_qty/b.unit_swap) as sumqty
                              from MM_PO_M a, MM_PO_D b, BC_CS_ACC_LOG c, PH_VENDER p     
                              where a.po_no=b.po_no and a.agen_no=p.agen_no 
                                 and b.po_no=c.po_no and b.mmcode=c.mmcode ";
            if (!string.IsNullOrWhiteSpace(ACC_TIME_B))
            {
                sql += " and trunc(ACC_TIME) >= to_date(substr(:p1, 0, 10), 'yyyy/mm/dd') ";
                p.Add(":p1", string.Format("{0}", ACC_TIME_B));
            }

            if (!string.IsNullOrWhiteSpace(ACC_TIME_E))
            {
                sql += " and trunc(ACC_TIME) <= to_date(substr(:p4, 0, 10), 'yyyy/mm/dd') ";
                p.Add(":p4", string.Format("{0}", ACC_TIME_E));
            }
            if (!string.IsNullOrWhiteSpace(M_CONTID))
            {
                sql += " and a.m_contid = :p2 ";
                p.Add(":p2", string.Format("{0}", M_CONTID));
            }
            if (!string.IsNullOrWhiteSpace(MMCODE))
            {
                sql += " and b.MMCODE = :p5 ";
                p.Add(":p5", string.Format("{0}", MMCODE));
            }
            sql += @"            and c.status='P' 
                                 and acc_qty <> 0 and a.mat_class=:MAT_CLASS    
                                 and substr(a.po_no,1,3) in ('INV','GEN','SML')
                              group by twn_date(acc_time), a.agen_no, agen_namec, a.m_contid, 
                                 a.mat_class, m_discperc, c.mmcode, disc_cprice, po_price, b.M_PURUN   
                             ) x, MI_MAST y
                            WHERE    x.MMCODE = y.MMCODE
                            ";
            sql += "ORDER BY x.AGEN_NO, x.acc_time, x.MMCODE  ";
            p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS));
            return DBWork.Connection.Query<BG0006M>(sql, p, DBWork.Transaction);
        }
        public DataTable GetExcel(string MAT_CLASS, string ACC_TIME_B, string M_CONTID, string ACC_TIME_E, string MMCODE)
        {
            var p = new DynamicParameters();
            var sql = @"    SELECT 
                                A.PO_NO AS 訂單編號,
                                A.MMCODE AS 院內碼,
                                MMNAME_C AS 中文品名,
                                MMNAME_E AS 英文品名,
                                D.M_PURUN AS 包裝單位,
                                ACC_QTY/a.UNIT_SWAP AS 進貨數量,
                                D.PO_PRICE AS 合約價,
                                floor(D.PO_PRICE * ACC_QTY/a.UNIT_SWAP) AS 進貨金額,
                                nvl(D.DISC_CPRICE,D.PO_PRICE) as 優惠合約價, 
                                floor(D.DISC_CPRICE * ACC_QTY/a.UNIT_SWAP)  AS 支付金額, 
                                D.M_DISCPERC as 折讓比,
                                (floor(D.PO_PRICE * ACC_QTY/a.UNIT_SWAP) - floor(D.DISC_CPRICE * ACC_QTY/a.UNIT_SWAP) ) AS 折讓金額,                            
                                TWN_DATE(ACC_TIME) AS 進貨日期,
                                A.AGEN_NO AS 廠商碼,
                                P.AGEN_NAMEC AS 廠商名稱,
                                B.M_PHCTNCO AS 衛署證號,
                                (case when B.M_MATID = 'Y' then '是' when B.M_MATID = 'N' then '否' end) AS 聯標否
                            FROM
                                BC_CS_ACC_LOG A, MI_MAST B, MM_PO_D D, PH_VENDER P, MM_PO_M M
                            WHERE 
                                D.PO_NO=M.PO_NO
                                AND substr(a.po_no,1,3) in ('INV','GEN','SML')
                                AND A.MMCODE = B.MMCODE
                                AND A.PO_NO = D.PO_NO
                                AND A.MMCODE = D.MMCODE
                                AND A.AGEN_NO = P.AGEN_NO
                            ";

            if (!string.IsNullOrWhiteSpace(MAT_CLASS))
            {
                sql += " AND B.MAT_CLASS = :p0 ";
                p.Add(":p0", string.Format("{0}", MAT_CLASS));
            }

            if (!string.IsNullOrWhiteSpace(ACC_TIME_B))
            {
                sql += " AND trunc(ACC_TIME) >= to_date(substr(:p1, 0, 10), 'yyyy/mm/dd') ";
                p.Add(":p1", string.Format("{0}", ACC_TIME_B));
            }

            if (!string.IsNullOrWhiteSpace(ACC_TIME_E))
            {
                sql += " AND trunc(ACC_TIME) <= to_date(substr(:p4, 0, 10), 'yyyy/mm/dd') ";
                p.Add(":p4", string.Format("{0}", ACC_TIME_E));
            }

            if (!string.IsNullOrWhiteSpace(M_CONTID))
            {
                sql += " AND M.M_CONTID = :p2 ";
                p.Add(":p2", string.Format("{0}", M_CONTID));
            }
            if (!string.IsNullOrWhiteSpace(MMCODE))
            {
                sql += " AND A.MMCODE = :p5 ";
                p.Add(":p5", string.Format("{0}", MMCODE));
            }
            sql += "ORDER BY A.AGEN_NO, A.MMCODE ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public DataTable GetExcel_2(string MAT_CLASS, string ACC_TIME_B, string M_CONTID, string ACC_TIME_E, string MMCODE)
        {  //進貨彙總
            var p = new DynamicParameters();
            var sql = @"    SELECT 
                                x.MMCODE as 院內碼,
                                y.MMNAME_C as 中文品名,
                                y.MMNAME_E as 英文品名,
                                x.M_PURUN as 包裝單位,
                                x.sumqty as 進貨數量,
                                x.PO_PRICE as合約價,
                                floor(x.PO_PRICE * x.sumqty) as 進貨金額,
                                nvl(x.DISC_CPRICE,x.PO_PRICE) as 優惠合約價,
                                floor(x.DISC_CPRICE * x.sumqty)  as支付金額,
                                x.M_DISCPERC as 折讓比,
                                (floor(x.PO_PRICE * x.sumqty) - floor(x.DISC_CPRICE * x.sumqty) ) as 折讓金額,                                
                                x.ACC_TIME as 進貨日期,
                                x.AGEN_NO as 廠商碼,
                                x.AGEN_NAMEC as 廠商名稱,
                                y.M_PHCTNCO as 衛署證號,
                                (case when y.M_MATID = 'Y' then '是' when y.M_MATID = 'N' then '否' end) as 聯標否
                            FROM 
                            ( select  twn_date(acc_time) acc_time, a.agen_no, agen_namec, 
                                 a.m_contid, a.mat_class, m_discperc, c.mmcode, disc_cprice, po_price, b.M_PURUN,
                                 sum(c.acc_qty/b.unit_swap) as sumqty
                              from MM_PO_M a, MM_PO_D b, BC_CS_ACC_LOG c, PH_VENDER p     
                              where a.po_no=b.po_no and a.agen_no=p.agen_no 
                                 and b.po_no=c.po_no and b.mmcode=c.mmcode ";
            if (!string.IsNullOrWhiteSpace(ACC_TIME_B))
            {
                sql += " and trunc(ACC_TIME) >= to_date(substr(:p1, 0, 10), 'yyyy/mm/dd') ";
                p.Add(":p1", string.Format("{0}", ACC_TIME_B));
            }

            if (!string.IsNullOrWhiteSpace(ACC_TIME_E))
            {
                sql += " and trunc(ACC_TIME) <= to_date(substr(:p4, 0, 10), 'yyyy/mm/dd') ";
                p.Add(":p4", string.Format("{0}", ACC_TIME_E));
            }
            if (!string.IsNullOrWhiteSpace(M_CONTID))
            {
                sql += " and a.m_contid = :p2 ";
                p.Add(":p2", string.Format("{0}", M_CONTID));
            }
            if (!string.IsNullOrWhiteSpace(MMCODE))
            {
                sql += " and b.MMCODE = :p5 ";
                p.Add(":p5", string.Format("{0}", MMCODE));
            }
            sql += @"            and c.status='P' 
                                 and acc_qty <> 0 and a.mat_class=:MAT_CLASS    
                                 and substr(a.po_no,1,3) in ('INV','GEN','SML')
                              group by twn_date(acc_time), a.agen_no, agen_namec, a.m_contid, 
                                 a.mat_class, m_discperc, c.mmcode, disc_cprice, po_price, b.M_PURUN   
                             ) x, MI_MAST y
                            WHERE    x.MMCODE = y.MMCODE
                            ";
            sql += "ORDER BY x.AGEN_NO, x.acc_time, x.MMCODE ";
            p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS));
            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
        public IEnumerable<ComboItemModel> GetMatCombo()
        {
            var p = new DynamicParameters();

            string sql = @" SELECT MAT_CLASS AS VALUE, MAT_CLASS || ' ' || MAT_CLSNAME AS TEXT
                            FROM MI_MATCLASS
                            WHERE MAT_CLSID IN ('2','3') ";

            //p.Add(":p0", string.Format("{0}", userid));

            return DBWork.Connection.Query<ComboItemModel>(sql, p, DBWork.Transaction);
        }


    }
}