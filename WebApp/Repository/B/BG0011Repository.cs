using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Web;
using JCLib.DB;
using Dapper;
using WebApp.Models;

namespace WebApp.Repository.B
{
    public class BG0011Repository : JCLib.Mvc.BaseRepository
    {
        public BG0011Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        public IEnumerable<BG0011MasterMODEL> GetAll(string p0, string p2, string p4, string p5, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @" with temp as (
        select set_ym, set_btime, set_ctime from MI_MNSET where set_ym=:p0
        )
        select d.agen_no F1,  --廠商代碼
                f.EASYNAME f2, --廠商名稱
                a.invoice f3,  --發票號碼
                f.uni_no f4,   --統一編號
                round(sum(e.acc_qty*e.acc_po_price))-sum(nvl(a.extra_disc_amount, 0)) f7, --實付金額
                round(sum(a.more_disc_amount)) f6,   --合約優惠
                round(sum(e.acc_qty*e.acc_po_price)) - sum(nvl(a.extra_disc_amount, 0)) - round(sum(a.more_disc_amount)) f5  --應付金額
        from temp c, BC_CS_ACC_LOG e, PH_INVOICE a, MI_MAST b, MM_PO_M d, PH_VENDER f
        where a.po_no=d.po_no
        and a.mmcode=b.mmcode
        and e.seq=a.acc_log_seq
        and d.agen_no=f.agen_no
        and e.acc_time between c.set_btime and c.set_ctime
                        ";

            // 類別
            if (p2 == "1")
                sql += " AND D.MAT_CLASS <> '01' AND  D.CONTRACNO = '2'";
            else if (p2 == "2")
                sql += " AND D.MAT_CLASS = '01' AND  D.CONTRACNO = '2'";
            else if (p2 == "3")
                sql += " AND D.MAT_CLASS <> '01' AND  D.CONTRACNO = '1'";
            else if (p2 == "4")
                sql += " AND D.MAT_CLASS = '01' AND  D.CONTRACNO = '1'";

            // 廠商代碼起迄
            if (p4 != "" || p5 != "")
            {
                if (p4 != "" && p5 != "")
                {
                    sql += " AND D.AGEN_NO BETWEEN :p4 and :p5 ";
                    p.Add(":p4", string.Format("{0}", p4));
                    p.Add(":p5", string.Format("{0}", p5));
                }
                else if (p4 != "" && p5 == "")
                {
                    sql += " AND D.AGEN_NO >= :p4";
                    p.Add(":p4", string.Format("{0}", p4));
                }
                else if (p4 == "" && p5 != "")
                {
                    sql += " AND D.AGEN_NO <= :p5";
                    p.Add(":p5", string.Format("{0}", p5));
                }
            }

            sql += " GROUP BY TWN_YYYMM(A.INVOICE_DT), d.AGEN_NO, f.EASYNAME, A.INVOICE, f.UNI_NO ";

            p.Add(":p0", string.Format("{0}", p0));
            p.Add(":p1", string.Format("{0}", p0));

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<BG0011MasterMODEL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<BG0011> Report_1(string p0, string p2, string p4, string p5)
        {
            var p = new DynamicParameters();
            var sql = @"
                    with temp as (
        select set_ym, set_btime, set_ctime from MI_MNSET where set_ym=:p0
        )
        select d.agen_no,  --廠商代碼
                f.agen_namec, --廠商名稱
                a.invoice,  --發票號碼
                f.uni_no,   --統一編號
                round(sum(e.acc_qty*e.acc_po_price))-sum(nvl(a.extra_disc_amount, 0)) MONEY_1, --實付金額
                round(sum(a.more_disc_amount)) MONEY_2,   --合約優惠
                round(sum(e.acc_qty*e.acc_po_price)) - sum(nvl(a.extra_disc_amount, 0)) - round(sum(a.more_disc_amount)) MONEY_3  --應付金額
        from temp c, BC_CS_ACC_LOG e, PH_INVOICE a, MI_MAST b, MM_PO_M d, PH_VENDER f
        where a.po_no=d.po_no
        and a.mmcode=b.mmcode
        and e.seq=a.acc_log_seq
        and d.agen_no=f.agen_no
        and e.acc_time between c.set_btime and c.set_ctime
                        ";

            // 類別
            if (p2 == "1")
                sql += " AND D.MAT_CLASS <> '01' AND  D.CONTRACNO = '2'";
            else if (p2 == "2")
                sql += " AND D.MAT_CLASS = '01' AND  D.CONTRACNO = '2'";
            else if (p2 == "3")
                sql += " AND D.MAT_CLASS <> '01' AND  D.CONTRACNO = '1'";
            else if (p2 == "4")
                sql += " AND D.MAT_CLASS = '01' AND  D.CONTRACNO = '1'";

            // 廠商代碼起迄
            if (p4 != "" || p5 != "")
            {
                if (p4 != "" && p5 != "")
                {
                    sql += " AND D.AGEN_NO BETWEEN :p4 and :p5 ";
                    p.Add(":p4", string.Format("{0}", p4));
                    p.Add(":p5", string.Format("{0}", p5));
                }
                else if (p4 != "" && p5 == "")
                {
                    sql += " AND D.AGEN_NO >= :p4";
                    p.Add(":p4", string.Format("{0}", p4));
                }
                else if (p4 == "" && p5 != "")
                {
                    sql += " AND D.AGEN_NO <= :p5";
                    p.Add(":p5", string.Format("{0}", p5));
                }
            }

            sql += @" GROUP BY TWN_YYYMM(A.INVOICE_DT), d.AGEN_NO, f.AGEN_NAMEC, A.INVOICE, f.UNI_NO 
                order by d.agen_no
            ";

            p.Add(":p0", string.Format("{0}", p0));
            p.Add(":p1", string.Format("{0}", p0));
            return DBWork.Connection.Query<BG0011>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetClassCombo()
        {
            string sql = @" SELECT VALUE, TEXT FROM (
                            SELECT '1' VALUE, '民眾衛材' TEXT FROM DUAL
                            UNION
                            SELECT '2' VALUE, '民眾藥材' TEXT FROM DUAL
                            UNION
                            SELECT '3' VALUE, '軍用衛材' TEXT FROM DUAL
                            UNION
                            SELECT '4' VALUE, '軍用藥材' TEXT FROM DUAL
                            ) ORDER BY VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<PH_VENDER> GetAgenNoCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.AGEN_NO, A.AGEN_NAMEC, A.EASYNAME
                          FROM PH_VENDER A 
                         WHERE 1=1 ";

            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.AGEN_NO, :AGEN_NO_I), 1000) + NVL(INSTR(A.EASYNAME, :EASYNAME_I), 100) * 10) IDX,"); // 設定權重, 值越小權重最大                             
                p.Add(":AGEN_NO_I", p0);
                p.Add(":EASYNAME_I", p0);

                sql += " AND (A.AGEN_NO LIKE :AGEN_NO";
                p.Add(":AGEN_NO", string.Format("%{0}%", p0));

                sql += " OR A.EASYNAME LIKE :EASYNAME) ";
                p.Add(":EASYNAME", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX, AGEN_NO", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY A.AGEN_NO ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<PH_VENDER>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);

        }

        public class BG0011MasterMODEL : JCLib.Mvc.BaseModel
        {
            public string rnm { get; set; }
            public string F1 { get; set; }
            public string F2 { get; set; }
            public string F3 { get; set; }
            public string F4 { get; set; }
            public string F5 { get; set; }
            public string F6 { get; set; }
            public string F7 { get; set; }
            //public string F8 { get; set; }
            //public string F9 { get; set; }
            //public string F10 { get; set; }

        }
    }
}