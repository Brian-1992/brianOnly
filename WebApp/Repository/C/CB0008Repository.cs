using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.C
{
    public class CB0008Repository : JCLib.Mvc.BaseRepository
    {
        public CB0008Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<CB0008> GetAll(string wh_no, string mmcode, string barcode, string mmname_c, string mmname_e, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @" select B.WH_NO, A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS,
                        (select DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'M_STOREID' and DATA_VALUE = A.M_STOREID) as M_STOREID,
                        B.INV_QTY, 
                        (select case when AGEN_NAMEC is not null then AGEN_NAMEC else AGEN_NAMEE end from PH_VENDER where AGEN_NO=A.M_AGENNO) as M_AGENNO,
                        A.M_AGENLAB,
                        listagg(C.BARCODE, ',') within group(order by BARCODE) as BARCODE 
                        FROM MI_MAST A, MI_WHINV B, BC_BARCODE C WHERE A.MMCODE=B.MMCODE and A.MMCODE=C.MMCODE ";

            sql += " AND B.WH_NO = :p0 ";
            p.Add(":p0", wh_no);

            if (mmcode != "" && mmcode != null)
            {
                sql += " AND A.MMCODE LIKE :p1 ";
                p.Add(":p1", string.Format("%{0}%", mmcode));
            }
            if (barcode != "" && barcode != null)
            {
                sql += " AND C.BARCODE LIKE :p2 ";
                p.Add(":p2", string.Format("%{0}%", barcode));
            }
            if (mmname_c != "" && mmname_c != null)
            {
                sql += " AND A.MMNAME_C LIKE :p3 ";
                p.Add(":p3", string.Format("%{0}%", mmname_c));
            }
            if (mmname_e != "" && mmname_e != null)
            {
                sql += " AND A.MMNAME_E LIKE :p4 ";
                p.Add(":p4", string.Format("%{0}%", mmname_e));
            }

            sql += " group by B.WH_NO,A.MMCODE,MMNAME_C,A.MMNAME_E, M_STOREID, B.INV_QTY, M_AGENNO, A.M_AGENLAB ";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CB0008>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        /// <summary>
        /// 修正品項基本資料查詢語法
        /// 2019/06/25
        /// </summary>
        /// <param name="wh_no">庫房別</param>
        /// <param name="mmcode">院內碼</param>
        /// <param name="barcode">品項條碼</param>
        /// <param name="mmname_c">中文品名</param>
        /// <param name="mmname_e">英文品名</param>
        /// <param name="page_index"></param>
        /// <param name="page_size"></param>
        /// <param name="sorters"></param>
        /// <returns></returns>
        public IEnumerable<CB0008> GetAll2(string wh_no, string mmcode, string barcode, string mmname_c, string mmname_e, string mat_class, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT  w.WH_NO, a.mmcode, a.mmname_c, a.mmname_e, a.base_unit, a.M_PURUN, 
                        (select MAT_CLASS || ' ' || MAT_CLSNAME from MI_MATCLASS where MAT_CLASS=a.MAT_CLASS) as MAT_CLASS,
                        (CASE WHEN m_storeid='0' THEN '非庫備' WHEN m_storeid='1' THEN '庫備' ELSE '' END) AS m_storeid ,
                        (SELECT EXCH_RATIO FROM MI_UNITEXCH WHERE agen_no=a.M_agenno AND mmcode = a.mmcode AND unit_code = a.M_PURUN) EXCH_RATIO,
                        (SELECT agen_namec FROM PH_VENDER WHERE agen_no = a.m_agenno)  m_agenno, m_agenlab, NVL(b.store_loc,'尚未設定儲位') store_loc,
                        NVL(c.barcode,'尚未設定條碼') barcode, nvl(w.inv_qty,0) as inv_qty                          
                        FROM MI_MAST a, 
                        (SELECT  MI_WLOCINV.mmcode, LISTAGG (store_loc,',') WITHIN GROUP(ORDER BY MI_WLOCINV.mmcode) store_loc FROM MI_WLOCINV, MI_MAST  
                        WHERE wh_no = :p0 and MI_WLOCINV.mmcode=MI_MAST.mmcode and MI_MAST.mat_class=:mat_class group by MI_WLOCINV.mmcode) b ,
                        (SELECT  BC_BARCODE.mmcode, LISTAGG (barcode,',') WITHIN GROUP(ORDER BY BC_BARCODE.mmcode) barcode FROM BC_BARCODE, MI_MAST  
                        WHERE BC_BARCODE.mmcode=MI_MAST.mmcode and MI_MAST.mat_class=:mat_class  ";

            p.Add(":p0", wh_no);

            if (barcode != "" && barcode != null)
            {
                sql += " AND barcode LIKE :p2  GROUP BY BC_BARCODE.mmcode) c, " +
                    "(select  wh_no, MI_WHINV.mmcode, inv_qty  from MI_WHINV, MI_MAST where MI_WHINV.mmcode=MI_MAST.mmcode and MI_MAST.mat_class=:mat_class ) w " +
                    "WHERE a.mmcode = w.mmcode(+) AND a.mmcode = b.mmcode(+) AND a.mmcode = c.mmcode(+) AND barcode <> '尚未設定條碼' and w.WH_NO=:p0 ";
                p.Add(":p2", string.Format("%{0}%", barcode));
            }
            else
            {
                sql += " GROUP BY BC_BARCODE.mmcode) c, (select  wh_no, MI_WHINV.mmcode, inv_qty  from MI_WHINV, MI_MAST where wh_no = :p0 and MI_WHINV.mmcode=MI_MAST.mmcode and MI_MAST.mat_class=:mat_class ) w " +
                    " WHERE a.mat_class=:mat_class and a.mmcode = w.mmcode(+) AND a.mmcode = b.mmcode(+) AND a.mmcode = c.mmcode(+) and w.WH_NO=:p0 ";
            }
            if (mmcode != "" && mmcode != null)
            {
                sql += " AND A.MMCODE LIKE :p1 ";
                p.Add(":p1", string.Format("%{0}%", mmcode));
            }

            if (mmname_c != "" && mmname_c != null)
            {
                sql += " AND A.MMNAME_C LIKE :p3 ";
                p.Add(":p3", string.Format("%{0}%", mmname_c));
            }
            if (mmname_e != "" && mmname_e != null)
            {
                sql += " AND A.MMNAME_E LIKE :p4 ";
                p.Add(":p4", string.Format("%{0}%", mmname_e));
            }
            p.Add(":mat_class",  mat_class);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CB0008>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        /// <summary>
        /// 取得庫房別ComboList
        /// </summary>
        /// <param name="id">User責任中心代碼</param>
        /// <returns></returns>
        public IEnumerable<COMBO_MODEL> GetWHNOCombo(string User_Inid)
        {
            string sql = @"SELECT WH_NO AS VALUE, WH_NAME AS TEXT,
                        RTrim(WH_NO || ' ' || WH_NAME) AS COMBITEM
                        FROM MI_WHMAST
                        WHERE inid = :User_Inid AND wh_kind IN ('0','1') AND wh_grade = '1'
                        ORDER BY WH_NO";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { User_Inid = User_Inid });
        }
        public IEnumerable<ComboItemModel> GetCLSNAME(string User_Inid)
        {
            string sql = @"  SELECT MAT_CLASS VALUE,
                                    MAT_CLASS || ' ' || MAT_CLSNAME TEXT
                             FROM MI_MATCLASS where mat_class >='01' and  mat_class <='08'
                             ORDER BY MAT_CLASS";
            return DBWork.Connection.Query<ComboItemModel>(sql, DBWork.Transaction);
        }


    }
}