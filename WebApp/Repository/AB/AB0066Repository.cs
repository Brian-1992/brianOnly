using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Globalization;

namespace WebApp.Repository.AB
{
    public class AB0066Repository : JCLib.Mvc.BaseRepository
    {
        public AB0066Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        private string GetQuerySQL(string DateYM_Fr, string DateYM_To, string MMCODE, string WH_NO, string[] str_WH_GRADE, string R_CODE, string ORDER_FLAG) {
            string sql = @"
                with detail as (
                    select a.data_ym,
                           a.MMCODE,
                           b.MMNAME_E,
                           a.WH_NO||'_'||c.WH_NAME WH_NAME_C,
                           (select inv_qty from MI_WINVMON 
                             where data_ym = twn_pym(a.data_ym) 
                               and wh_no = a.wh_no and mmcode = a.mmcode) as PMN_INVQTY,
                           (a.APL_INQTY + a.trn_inqty + a.bak_inqty + a.exg_inqty + a.mil_inqty) as apl_inqty,
                           (a.APL_OUTQTY + a.trn_outqty + a.bak_outqty + a.rej_outqty + a.dis_outqty + a.exg_outqty + a.mil_outqty + a.use_qty) as apl_outqty, 
                           a.INVENTORYQTY,
                           (a.ADJ_INQTY - a.ADJ_OUTQTY) as ADJ_QTY,
                           a.INV_QTY as INV_QTY_End,
                           d.INV_QTY as INV_QTY_Now,
                           b.E_ORDERDCFLAG
                      from MI_WINVMON A, MI_MAST B,
                           MI_WHMAST C, MI_WHINV D,
                           MI_WHCOST E
                     where 1=1
                       and a.MMCODE=b.MMCODE
                       and b.mat_class = '01'
                       and a.WH_NO=c.WH_NO
                       and a.MMCODE=d.MMCODE(+) and a.WH_NO=d.WH_NO(+)
                       and a.DATA_YM=e.DATA_YM and a.MMCODE=e.MMCODE
            ";
            if (DateYM_Fr != "")
            {
                sql += " AND A.DATA_YM >= :DateYM_Fr ";
            }
            if (DateYM_To != "")
            {
                sql += " AND A.DATA_YM <= :DateYM_To ";
            }
            if (MMCODE != "")
            {
                sql += " AND A.MMCODE = :MMCODE ";
            }
            if (WH_NO != "")
            {
                sql += " AND A.WH_NO = :WH_NO ";
            }
            if (str_WH_GRADE.Length > 0)
            {
                string sql_WH_GRADE = "";
                sql += @"AND (";
                foreach (string tmp_WH_GRADE in str_WH_GRADE)
                {
                    if (string.IsNullOrEmpty(sql_WH_GRADE))
                    {
                        sql_WH_GRADE = @"C.WH_GRADE = '" + tmp_WH_GRADE + "'";
                    }
                    else
                    {
                        sql_WH_GRADE += @" OR C.WH_GRADE = '" + tmp_WH_GRADE + "'";
                    }
                }
                sql += sql_WH_GRADE + ") ";
            }
            if (R_CODE != "")
            {
                sql += " AND B.E_RESTRICTCODE = :R_CODE ";
            }
            if (ORDER_FLAG != "")
            {
                sql += " AND B.E_ORDERDCFLAG = :ORDER_FLAG ";
            }
            sql += @"
                )
                --select '總計' as MMCODE,'' as MMNAME_E,'' as WH_NAME_C,
                --       sum(PMN_INVQTY) as PMN_INVQTY,sum(APL_INQTY) as APL_INQTY,
                --       sum(APL_OUTQTY) as APL_OUTQTY,sum(INVENTORYQTY) as INVENTORYQTY,
                --       sum(ADJ_QTY) as ADJ_QTY,sum(INV_QTY_End) as INV_QTY_End,
                --       sum(INV_QTY_Now) as INV_QTY_NOW,'' as E_ORDERDCFLAG
                --  from detail
                -- group by MMCODE
                --union  
                select * from detail
                 order by MMCODE
            ";

            return sql;
        }

        /// <summary>
        /// 取得查詢資料
        /// </summary>
        /// <param name="DateYM_Fr">查詢月份(起)</param>
        /// <param name="DateYM_To">查詢月份(迄)</param>
        /// <param name="MMCODE">藥品院內碼</param>
        /// <param name="WH_NO">庫房代碼</param>
        /// <param name="WH_GRADE">庫存類別</param>
        /// <param name="R_CODE">管制用藥代碼</param>
        /// <param name="ORDER_FLAG">停用碼</param>
        /// <param name="page_index"></param>
        /// <param name="page_size"></param>
        /// <param name="sorters"></param>
        /// <returns></returns>
        public IEnumerable<AA0066> GetAll(string DateYM_Fr, string DateYM_To, string MMCODE, string WH_NO, 
            string[] str_WH_GRADE, string R_CODE, string ORDER_FLAG)
        {
            var p = new DynamicParameters();

            string sql = GetQuerySQL(DateYM_Fr, DateYM_To, MMCODE, WH_NO, str_WH_GRADE, R_CODE, ORDER_FLAG);

            p.Add(":DateYM_Fr", string.Format("{0}", DateYM_Fr));
            p.Add(":DateYM_To", string.Format("{0}", DateYM_To));
            p.Add(":MMCODE", string.Format("{0}", MMCODE));
            p.Add(":WH_NO", string.Format("{0}", WH_NO));
            p.Add(":R_CODE", string.Format("{0}", R_CODE));
            p.Add(":ORDER_FLAG", string.Format("{0}", ORDER_FLAG));

            return DBWork.PagingQuery<AA0066>(sql, p, DBWork.Transaction);
        }

        /// <summary>
        /// 取得院內碼下拉式選單資料
        /// </summary>
        /// <param name="mmcode">院內碼，可對院內碼、院內碼中英文名稱進行模糊搜尋</param>
        /// <param name="page_index"></param>
        /// <param name="page_size"></param>
        /// <param name="sorters"></param>
        /// <returns></returns>
        public IEnumerable<MI_MAST> GetMMCODECombo(string mmcode, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT MMCODE, 
                    MMNAME_E,
                    MMNAME_C
                    FROM MI_MAST A
                    WHERE MAT_CLASS = '01' ";

            if (mmcode != "")
            {
                sql += " AND (A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}%", mmcode));

                sql += " OR MMNAME_E LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", mmcode));

                sql += " OR MMNAME_C LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", mmcode));

            }
            else
            {
                sql += " ORDER BY MMCODE ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        /// <summary>
        /// 取得庫房代碼下拉式選單資料
        /// </summary>
        /// <returns></returns>
        public IEnumerable<COMBO_MODEL> GetWH_NOCombo()
        {
            var p = new DynamicParameters();

            var sql = @"SELECT WH_NO AS VALUE, 
                    WH_NAME AS TEXT,
                    WH_NO || ' ' || WH_NAME AS COMBITEM  
                    FROM MI_WHMAST 
                    where wh_kind = '0'
                    ORDER BY WH_NO";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        /// <summary>
        /// 取得庫存類別歸屬下拉式選單資料
        /// </summary>
        /// <returns></returns>
        public IEnumerable<COMBO_MODEL> GetWH_GRADECombo()
        {
            var p = new DynamicParameters();

            var sql = @"SELECT DATA_VALUE AS VALUE, 
                    DATA_DESC AS TEXT,
                    DATA_VALUE || '_' || DATA_DESC AS COMBITEM  
                    FROM PARAM_D 
                    WHERE DATA_NAME = 'WH_GRADE'
                    ORDER BY DATA_SEQ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        /// <summary>
        /// 取得管制碼下拉式選單資料
        /// </summary>
        /// <returns></returns>
        public IEnumerable<COMBO_MODEL> GetE_RestrictCodeCombo()
        {
            var p = new DynamicParameters();

            var sql = @"SELECT DATA_VALUE AS VALUE, 
                    DATA_DESC AS TEXT,
                    DATA_VALUE || '_' || DATA_DESC AS COMBITEM  
                    FROM PARAM_D 
                    WHERE DATA_NAME = 'E_RESTRICTCODE'
                    ORDER BY DATA_SEQ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public DataTable GetExcel(string DateYM_Fr, string DateYM_To, string MMCODE, string WH_NO, string[] str_WH_GRADE, string R_CODE, string ORDER_FLAG) {
            string sql = GetQuerySQL(DateYM_Fr, DateYM_To, MMCODE, WH_NO, str_WH_GRADE, R_CODE, ORDER_FLAG);

            var p = new DynamicParameters();

            sql = string.Format(@"
                    select data_ym as 年月,
                           mmcode as 藥品代碼,
                           mmname_e as 藥品名稱,
                           WH_NAME_C as 庫別,
                           PMN_INVQTY as 上月結存,
                           APL_INQTY as 本期入帳,
                           APL_OUTQTY as 本期出帳,
                           INVENTORYQTY as 盤點差,
                            ADJ_QTY as 調帳差,
                            INV_QTY_End as 結存量,
                            INV_QTY_Now as 現存量,
                            E_ORDERDCFLAG as 各庫停用碼
                      from (
                        {0}
                        )  
                     order by data_ym, mmcode, wh_name_c
                  ", sql);

            p.Add(":DateYM_Fr", string.Format("{0}", DateYM_Fr));
            p.Add(":DateYM_To", string.Format("{0}", DateYM_To));
            p.Add(":MMCODE", string.Format("{0}", MMCODE));
            p.Add(":WH_NO", string.Format("{0}", WH_NO));
            p.Add(":R_CODE", string.Format("{0}", R_CODE));
            p.Add(":ORDER_FLAG", string.Format("{0}", ORDER_FLAG));

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
    }
}