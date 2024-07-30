using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.C
{
    public class CC0004Repository : JCLib.Mvc.BaseRepository
    {
        public CC0004Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        /// <summary>
        /// 取得品項基本資料
        /// </summary>
        /// <param name="mmcode">院內碼</param>
        /// <param name="page_index"></param>
        /// <param name="page_size"></param>
        /// <param name="sorters"></param>
        /// <returns></returns>
        public IEnumerable<CC0004> GetAll(string mmcode, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT a.mmcode, a.mmname_c, a.mmname_e, a.base_unit, a.M_PURUN,
                    (CASE WHEN m_storeid = '0' THEN '非庫備' WHEN m_storeid = '1' THEN '庫備' ELSE '' END) AS m_storeid ,
                    (SELECT EXCH_RATIO FROM MI_UNITEXCH WHERE agen_no = a.M_agenno AND mmcode = a.mmcode AND unit_code = a.M_PURUN) EXCH_RATIO,
                    (SELECT AGEN_NO FROM PH_VENDER WHERE agen_no = a.m_agenno) M_AGENNO, 
                    (SELECT agen_namec FROM PH_VENDER WHERE agen_no = a.m_agenno) AGEN_NAMEC, 
                    m_agenlab, NVL(b.store_loc,'尚未設定儲位') store_loc
                    FROM MI_MAST a, 
                    (SELECT mmcode, LISTAGG (store_loc,',') WITHIN GROUP(ORDER BY mmcode) store_loc  FROM MI_WLOCINV
                    WHERE wh_no IN (SELECT wh_no FROM MI_WHMAST WHERE inid = '560000' AND wh_kind IN ('0','1') AND wh_grade = '1') 
                    GROUP BY mmcode) b
                    WHERE a.mmcode = b.mmcode(+) 
                    AND a.mmcode IN (SELECT mmcode FROM bc_barcode WHERE mmcode = :MMCODE OR barcode = :MMCODE)";

            p.Add(":MMCODE", string.Format("{0}", mmcode));

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CC0004>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }        
    }
}