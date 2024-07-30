using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.F
{
    public class FA0047Repository : JCLib.Mvc.BaseRepository
    {
        public FA0047Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<FA0047> GetAll(string MAT_CLASS, string YYYMM_B, string YYYMM_E, string P3, string P4, string MMCODE, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"  select a.mmcode
                         ,(select case :P3 when '1' then '軍' else '民' end from dual) as miltype
                         ,(select mmname_c from MI_MAST where mmcode=a.mmcode) as mmname_c
                         ,(select mmname_e from MI_MAST where mmcode=a.mmcode) as mmname_e
                         ,(select base_unit from MI_MAST where mmcode=a.mmcode) as base_unit,data_ym
                         ,(select m_storeid from MI_MAST where mmcode=a.mmcode) as m_storeid
                         ,(select m_contid from MI_MAST where mmcode=a.mmcode) as m_contid
                         ,(select inid from MI_WHMAST where wh_no=a.wh_no) as inid
                         , p_inv_qty
                         ,pmn_avgprice
                         ,(p_inv_qty*pmn_avgprice) as pmn_amt,
                         in_qty,
                         in_price,
                        (in_qty*in_price) as in_amt,
                         out_qty,avg_price
                        ,(out_qty*avg_price) as out_amt, 
                         inv_qty
                        ,avg_price as inv_price,
                         (inv_qty*avg_price) as inv_amt
                    from V_COST_WH2 a  left join  MI_MAST b on a.mmcode=b.mmcode

                                where data_ym>=:YYYMM_B 
                                    and data_ym<=:YYYMM_E 
                                    and wh_no not in (select whno_mm1 as wh_no from dual 
                                                               union select whno_me1 as wh_no from dual)
                         ";

            p.Add(":P3", string.Format("{0}", P3));
            p.Add(":YYYMM_B", string.Format("{0}", YYYMM_B));
            p.Add(":YYYMM_E", string.Format("{0}", YYYMM_E));


            if (MAT_CLASS != "")
            {
                sql += " AND b.MAT_CLASS  = :MAT_CLASS ";
                p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS));
            }


            if (P4 == "1" || P4 == "0")
            {
                sql += @"  and  b.m_storeid =:P4 ";
                p.Add(":P4", string.Format("{0}", P4));

            }
            if (MMCODE != "")
            {
                sql += " AND a.mmcode  = :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}", MMCODE));
            }


            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<FA0047>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }



        public IEnumerable<ComboItemModel> GetMATCombo()
        {
            string sql = @"Select MAT_CLASS||' '||MAT_CLSNAME TEXT, MAT_CLASS VALUE from MI_MATCLASS 
                             where mat_class <> '01' 
                            ORDER BY VALUE";


            return DBWork.Connection.Query<ComboItemModel>(sql, new { userID = DBWork.ProcUser }, DBWork.Transaction);
        }
        public DataTable GetExcel(string MAT_CLASS, string YYYMM_B, string YYYMM_E, string P3, string P4, string MMCODE)
        {
            var p = new DynamicParameters();

            var sql = @" select a.mmcode 院內碼 
                         ,:P3 as 軍民別 
                         ,(select mmname_c from MI_MAST where mmcode=a.mmcode) as 中文品名 
                         ,(select mmname_e from MI_MAST where mmcode=a.mmcode) as 英文品名 
                         ,(select base_unit from MI_MAST where mmcode=a.mmcode) as 計量單位
                         ,data_ym  年月
                         ,(select inid from MI_WHMAST where wh_no=a.wh_no) as 責任中心
                         ,(select m_storeid from MI_MAST where mmcode=a.mmcode) as 是否庫備
                         ,(select m_contid from MI_MAST where mmcode=a.mmcode) as 是否合約
                         , p_inv_qty 期初數量
                         ,pmn_avgprice 期初單價
                         ,(p_inv_qty*pmn_avgprice) as 期初金額,
                         in_qty 進貨數量,
                         in_price 進貨單價,
                        (in_qty*in_price) as 進貨金額,
                         out_qty  撥發數量
                        ,avg_price  撥發單價
                        ,(out_qty*avg_price) as 撥發金額, 
                         inv_qty 結存數量
                        ,avg_price as 庫存單價,
                         (inv_qty*avg_price) as 結存金額
                    from V_COST_WH2 a  left join  MI_MAST b on a.mmcode=b.mmcode

                                where data_ym>=:YYYMM_B 
                                    and data_ym<=:YYYMM_E 
                                    and wh_no not in (select whno_mm1 as wh_no from dual 
                                                               union select whno_me1 as wh_no from dual)
                         ";
            p.Add(":P3", string.Format("{0}", P3));
            p.Add(":YYYMM_B", string.Format("{0}", YYYMM_B));
            p.Add(":YYYMM_E", string.Format("{0}", YYYMM_E));


            if (MAT_CLASS != "")
            {
                sql += " AND b.MAT_CLASS  = :MAT_CLASS ";
                p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS));
            }


            //if (P3 == "1")
            //{
            //    sql += @"    and wh_no in (select whno_1x('0') as wh_no from dual 
            //                        union select whno_1x('1') as wh_no from dual) ";
            //}
            //else
            //{
            //    sql += @"   and wh_no in (select whno_mm1 as wh_no from dual 
            //                       union select whno_me1 as wh_no from dual)  ";
            //}

            if (P4 == "1" || P4 == "0")
            {
                sql += @"  and  b.m_storeid =:P4 ";
                p.Add(":P4", string.Format("{0}", P4));

            }
            if (MMCODE != "")
            {
                sql += " AND a.mmcode  = :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}", MMCODE));
            }

            sql += " order by a.mmcode,data_ym ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }


        public DataTable Report(string MAT_CLASS, string YYYMM_B, string YYYMM_E, string P3, string P4, string MMCODE)
        {
            var p = new DynamicParameters();


            var sql = @" select a.mmcode
                         ,(select case :P3 when '1' then '軍' else '民' end from dual) as miltype
                         ,(select mmname_c from MI_MAST where mmcode=a.mmcode) as mmname_c
                         ,(select mmname_e from MI_MAST where mmcode=a.mmcode) as mmname_e
                         ,(select base_unit from MI_MAST where mmcode=a.mmcode) as base_unit,data_ym
                         ,(select m_storeid from MI_MAST where mmcode=a.mmcode) as m_storeid
                         ,(select m_contid from MI_MAST where mmcode=a.mmcode) as m_contid
                         ,(select inid from MI_WHMAST where wh_no=a.wh_no) as inid
                         , p_inv_qty
                         ,pmn_avgprice
                         ,(p_inv_qty*pmn_avgprice) as pmn_amt,
                         in_qty,
                         in_price,
                        (in_qty*in_price) as in_amt,
                         out_qty,avg_price
                        ,(out_qty*avg_price) as out_amt, 
                         inv_qty
                        ,avg_price as inv_price,
                         (inv_qty*avg_price) as inv_amt
                    from V_COST_WH2 a  left join  MI_MAST b on a.mmcode=b.mmcode

                                where data_ym>=:YYYMM_B 
                                    and data_ym<=:YYYMM_E 
                                    and wh_no not in (select whno_mm1 as wh_no from dual 
                                                               union select whno_me1 as wh_no from dual)
                         ";
            p.Add(":P3", string.Format("{0}", P3));
            p.Add(":YYYMM_B", string.Format("{0}", YYYMM_B));
            p.Add(":YYYMM_E", string.Format("{0}", YYYMM_E));


            if (MAT_CLASS != "")
            {
                sql += " AND b.MAT_CLASS  = :MAT_CLASS ";
                p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS));
            }


            //if (P3 == "1")
            //{
            //    sql += @"    and wh_no in (select whno_1x('0') as wh_no from dual 
            //                        union select whno_1x('1') as wh_no from dual) ";
            //}
            //else
            //{
            //    sql += @"   and wh_no in (select whno_mm1 as wh_no from dual 
            //                       union select whno_me1 as wh_no from dual)  ";
            //}

            if (P4 == "1" || P4 == "0")
            {
                sql += @"  and  b.m_storeid =:P4 ";
                p.Add(":P4", string.Format("{0}", P4));

            }
            if (MMCODE != "")
            {
                sql += " AND a.mmcode  = :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}", MMCODE));
            }
            sql += " order by mmcode,data_ym ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }



        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT FROM MI_MAST A WHERE 1=1 ";


            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(A.MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(A.MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("%{0}%", p0));

                sql += " OR A.MMNAME_E LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR A.MMNAME_C LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX, MMCODE", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY A.MMCODE ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

    }
}