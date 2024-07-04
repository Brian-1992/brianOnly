using System;
using System.Collections.Generic;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;

namespace WebApp.Repository.AA
{
    public class AA0083 : JCLib.Mvc.BaseModel
    {
        public string WH_NO { get; set; }
        public string WH_NAME { get; set; }
        public string P_INV_QTY { get; set; }
        public string INQTY { get; set; }
        public string OUTQTY { get; set; }
        public string INV_QTY { get; set; }
        public string ONWAY_QTY { get; set; }
        public string ADJ_QTY { get; set; }
        public string OTHER_INQTY { get; set; }
        public string OTHER_OUTQTY { get; set; }
        public string SAFE_QTY { get; set; }
        public string OPER_QTY { get; set; }
        public string DATA_YM { get; set; }
        public string USE_QTY { get; set; }
        public string UPRICE { get; set; }
    }

    public class AA0083_D : JCLib.Mvc.BaseModel
    {
        public string INV_QTY { get; set; }
        public string USE_QTY { get; set; }
        public string MIN_ACCOUNTDATE { get; set; }
        public string MAX_ACCOUNTDATE { get; set; }
        public string AGEN_NAME { get; set; }
        public string CONTRACT_TYPE { get; set; }
        public string M_CONTPRICE { get; set; }
        public string DISC_CPRICE { get; set; }
        public string STORE_LOC { get; set; }
        public string MIN_ORDQTY { get; set; }
    }
    public class AA0083Repository : JCLib.Mvc.BaseRepository
    {
        public AA0083Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AA0083> GetAll(string mmcode)
        {
            var p = new DynamicParameters();

            var sql = @"
                select a.wh_no, wh_name(a.wh_no) as wh_name,
                       nvl(c.inv_qty, 0) as p_inv_qty, a.inqty, a.outqty, 
                       a.inv_Qty,
                       a.onway_qty, a.adj_qty, a.other_inqty, a.other_outqty, a.use_qty,
                       a.SAFE_QTY, a.OPER_QTY
                  from V_WH_ORDER b, (
                        with mnset as (
                            select set_ym from MI_MNSET
                             where set_status <> 'N'
                               and rownum = 1
                             order by set_ym desc
                        )
                        select a.wh_no, a.mmcode, b.set_ym,
                               a.apl_inqty as inqty,
                               a.apl_outqty as outqty,
                               (a.inv_qty) as inv_qty,
                               a.onway_qty as onway_qty,
                               (a.inventoryqty + a.ADJ_INQTY - a.ADJ_OUTQTY - a.DIS_OUTQTY) as adj_qty,
                               (a.EXG_INQTY + a.MIL_INQTY + a.BAK_INQTY) as other_inqty,
                               (a.EXG_OUTQTY + a.MIL_OUTQTY + a.BAK_OUTQTY) as other_outqty,
                               SAFE_QTY(WH_NO, MMCODE) SAFE_QTY, 
                               OPER_QTY(WH_NO, MMCODE) OPER_QTY,
                               a.use_qty
                          from MI_WHINV a, mnset b
                         where a.wh_no in (select wh_no from MI_WHMAST where wh_grade = '1' and wh_kind = '0')
                           and mmcode = :mmcode
                        union
                        select a.wh_no, a.mmcode,  b.set_ym,
                               a.apl_inqty as inqty,
                               a.apl_outqty as outqty,
                               (a.inv_qty) as inv_qty,
                               a.onway_qty as onway_qty,
                               (a.inventoryqty + a.ADJ_INQTY - a.ADJ_OUTQTY - a.DIS_OUTQTY) as adj_qty,
                               (a.EXG_INQTY + a.MIL_INQTY + a.BAK_INQTY) as other_inqty,
                               (a.EXG_OUTQTY + a.MIL_OUTQTY + a.BAK_OUTQTY) as other_outqty,
                               SAFE_QTY(WH_NO, MMCODE) SAFE_QTY, 
                               OPER_QTY(WH_NO, MMCODE) OPER_QTY,
                               a.use_qty
                          from MI_WHINV a, mnset b
                         where a.wh_no in (select wh_no from MI_WHMAST where wh_grade = '2' and wh_kind = '0')
                           and mmcode = :mmcode
                        union
                        select a.wh_no, :MMCODE as mmcode,  b.set_ym,
                               0,0,0,0,0,0,0,
                               SAFE_QTY(a.WH_NO, :mmcode) SAFE_QTY, 
                               OPER_QTY(a.WH_NO, :mmcode) OPER_QTY,
                               0
                          from V_WH_ORDER a, mnset b
                         where not exists (select 1 from MI_WHINV where wh_no = a.wh_no and mmcode = :mmcode)
                       ) a
                  left join MI_WINVMON c on (c.data_ym = a.set_ym and a.wh_no = c.wh_no and a.mmcode = c.mmcode)
                 where a.wh_no = b.wh_no
                 order by b.seq
            ";

            //var sql = @"SELECT A.WH_NAME,
            //               SUM(LAST_STOCK_QTY) LAST_STOCK_QTY,
            //               SUM(INQTY) INQTY,
            //               SUM(OUTQTY) OUTQTY,
            //               SUM(INV_QTY) INV_QTY,
            //               SUM(EXG_QTY) EXG_QTY,
            //               SUM(INQTY_OTHERS) INQTY_OTHERS,
            //               SUM(SAFE_QTY) SAFE_QTY,
            //               SUM(OPER_QTY) OPER_QTY,
            //               UPRICE(:MMCODE) UPRICE
            //        FROM (
            //        SELECT WH_NAME(WH_NO) WH_NAME,
            //               LAST_STOCK_QTY(TWN_DATE(ADD_MONTHS(LAST_DAY(SYSDATE)+1,-1)),A.WH_NO,A.MMCODE) LAST_STOCK_QTY, --上月結存
            //               YM_INQTY(SUBSTR(TWN_DATE(ADD_MONTHS(LAST_DAY(SYSDATE)+1,-1)),1,5), WH_NO, MMCODE) INQTY, --本月進
            //               YM_OUTQTY(SUBSTR(TWN_DATE(ADD_MONTHS(LAST_DAY(SYSDATE)+1,-1)),1,5), WH_NO, MMCODE) OUTQTY, --本月出
            //               INV_QTY(WH_NO, MMCODE) INV_QTY, --現品量
            //               0 EXG_QTY, --調整量
            //               YM_INQTY_OTHERS(SUBSTR(TWN_DATE(ADD_MONTHS(LAST_DAY(SYSDATE)+1,-1)),1,5), WH_NO, MMCODE) INQTY_OTHERS, --其他進
            //               SAFE_QTY(WH_NO, MMCODE) SAFE_QTY, --安全量
            //               OPER_QTY(WH_NO, MMCODE) OPER_QTY  --作業量
            //        FROM MI_WHINV A
            //        WHERE EXISTS (
            //        SELECT 1 FROM MI_WHMAST B WHERE A.WH_NO = B.WH_NO AND B.WH_KIND = '0' AND B.WH_GRADE = '2')
            //        AND MMCODE = :MMCODE
            //        UNION ALL
            //        SELECT WH_NAME(WH_NO) WH_NAME,
            //               ( (SELECT PMN_INVQTY FROM MI_WHCOST 
            //                 WHERE DATA_YM = SUBSTR(TWN_DATE(ADD_MONTHS(LAST_DAY(SYSDATE)+1,-1)),1,5) 
            //                 AND MMCODE = :MMCODE) +
            //                 LAST_STOCK_QTY(TWN_DATE(ADD_MONTHS(LAST_DAY(SYSDATE)+1,-1)),A.WH_NO,A.MMCODE) 
            //               ) LAST_STOCK_QTY, --上月結存
            //               YM_INQTY(SUBSTR(TWN_DATE(ADD_MONTHS(LAST_DAY(SYSDATE)+1,-1)),1,5), WH_NO, MMCODE) INQTY, --本月進
            //               YM_OUTQTY(SUBSTR(TWN_DATE(ADD_MONTHS(LAST_DAY(SYSDATE)+1,-1)),1,5), WH_NO, MMCODE) OUTQTY, --本月出
            //               INV_QTY(WH_NO, MMCODE) INV_QTY,                  --現品量
            //               (EXG_INQTY - EXG_OUTQTY) EXG_QTY,                --調整量
            //               YM_INQTY_OTHERS(SUBSTR(TWN_DATE(ADD_MONTHS(LAST_DAY(SYSDATE)+1,-1)),1,5), WH_NO, MMCODE) INQTY_OTHERS, --其他進
            //               SAFE_QTY(WH_NO, MMCODE) SAFE_QTY, --安全量
            //               OPER_QTY(WH_NO, MMCODE) OPER_QTY  --作業量
            //        FROM MI_WHINV A
            //        WHERE EXISTS (
            //        SELECT 1 FROM MI_WHMAST B WHERE A.WH_NO = B.WH_NO AND B.WH_KIND = '0' AND B.WH_GRADE = '1')
            //        AND MMCODE = :MMCODE
            //        UNION ALL
            //        SELECT WH_NAME(WH_NO),0,0,0,0,0,0,0,0
            //        FROM MI_WHMAST B 
            //        WHERE B.WH_KIND = '0' AND B.WH_GRADE <= '2'
            //        ) A, V_WH_ORDER B
            //        WHERE A.WH_NAME = B.WH_NAME(+)
            //        GROUP BY B.SEQ, A.WH_NAME
            //        ORDER BY B.SEQ";

            p.Add(":mmcode", string.Format("{0}", mmcode));

            return DBWork.PagingQuery<AA0083>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMMCode(string p0, string mat_class, string m_storeid, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E  
                        FROM MI_MAST A WHERE 1=1  ";
            if (mat_class != "")
            {
                sql += " AND mat_class = :mat_class ";
                p.Add(":mat_class", string.Format("{0}", mat_class));
            }
            if (m_storeid != "")
            {
                sql += " AND m_storeid = :m_storeid ";
                p.Add(":m_storeid", string.Format("{0}", m_storeid));
            }
            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}%", p0));

                sql += " OR MMNAME_E LIKE UPPER(:MMNAME_E) ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR MMNAME_C LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY MMCODE ";
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<AA0083> GetUseQTY(string mmcode,string P_YM)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT DATA_YM,SUM(USE_QTY) USE_QTY FROM (
                            SELECT DATA_YM,USE_QTY
                            FROM MI_WINVMON
                            WHERE MMCODE = :MMCODE
                            AND DATA_YM > = :P_YM
                            UNION ALL SELECT SUBSTR(TWN_DATE(ADD_MONTHS(LAST_DAY(SYSDATE) + 1,-7)),1,5),0 FROM DUAL
                            UNION ALL SELECT SUBSTR(TWN_DATE(ADD_MONTHS(LAST_DAY(SYSDATE) + 1,-6)),1,5),0 FROM DUAL 
                            UNION ALL SELECT SUBSTR(TWN_DATE(ADD_MONTHS(LAST_DAY(SYSDATE) + 1,-5)),1,5),0 FROM DUAL 
                            UNION ALL SELECT SUBSTR(TWN_DATE(ADD_MONTHS(LAST_DAY(SYSDATE) + 1,-4)),1,5),0 FROM DUAL 
                            UNION ALL SELECT SUBSTR(TWN_DATE(ADD_MONTHS(LAST_DAY(SYSDATE) + 1,-3)),1,5),0 FROM DUAL 
                            UNION ALL SELECT SUBSTR(TWN_DATE(ADD_MONTHS(LAST_DAY(SYSDATE) + 1,-2)),1,5),0 FROM DUAL 
                        )
                        GROUP BY DATA_YM
                        ORDER BY DATA_YM DESC";

            p.Add(":MMCODE", string.Format("{0}", mmcode));
            p.Add(":P_YM", string.Format("{0}", P_YM));

            return DBWork.PagingQuery<AA0083>(sql, p, DBWork.Transaction);
        }

        //先取得P_YM(沒有獨立執行會很慢)
        public string GetP_YM()
        {
            string sql = @"SELECT SUBSTR(TWN_DATE(ADD_MONTHS(LAST_DAY(SYSDATE) + 1,-7)),1,5) FROM DUAL";
            var rtn = DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction);
            rtn = (rtn is null) ? "" : rtn.ToString();
            return rtn.ToString();
        }

        public IEnumerable<AA0083_D> GetDetail(string WH_NAME, string mmcode)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT sum(d.inv_qty) as INV_QTY,
                               sum(d.use_qty) as  USE_QTY,
                               (SELECT MIN(ACCOUNTDATE) FROM MM_PO_INREC WHERE MMCODE = :MMCODE) MIN_ACCOUNTDATE,
                               (SELECT MAX(ACCOUNTDATE) FROM MM_PO_INREC WHERE MMCODE = :MMCODE) MAX_ACCOUNTDATE,
                               (SELECT M_AGENNO||'_'||AGEN_NAME(M_AGENNO) FROM MI_MAST  WHERE MMCODE = :MMCODE) AGEN_NAME,
                               (SELECT data_value||'_'||data_desc
                                  from PARAM_D
                                 where grp_code = 'MI_MAST' and data_name = 'CONTRACNO' and data_value = e.contracno) as  CONTRACT_TYPE,
                               e.M_CONTPRICE,
                               e.DISC_CPRICE,
                               (select listagg(store_loc, ',')
                                          within group (order by store_loc)
                                            from mi_wlocinv
                                           where wh_no = 'PH1S'
                                             and mmcode = :MMCODE) as store_loc,
                               (select min_ordqty from MI_WINVCTL where wh_no = 'PH1S' and mmcode = :MMCODE) as min_ordqty         
                          FROM MI_MAST e, MI_WHINV d, V_WH_ORDER f
                         where e.MMCODE = :MMCODE
                           and e.mmcode = d.mmcode
                           and d.wh_no = f.wh_no
                         group by d.mmcode, e.M_CONTPRICE,e.DISC_CPRICE, e.contracno
                        ";

            p.Add(":WH_NAME", string.Format("{0}", WH_NAME));
            p.Add(":MMCODE", string.Format("{0}", mmcode));

            return DBWork.PagingQuery<AA0083_D>(sql, p, DBWork.Transaction);
        }

        //==========================================================================

    }
}