using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace WebApp.Repository.F
{

    public class FA0058ReportMODEL : JCLib.Mvc.BaseModel
    {
        public string F1 { get; set; }  // 單位代碼
        public string F2 { get; set; }  // 單位名稱
        public double F3 { get; set; }  // 期初金額
        public double F4 { get; set; }  // 進貨金額
        public double F5 { get; set; }  // 消耗金額
        public double F6 { get; set; }  // 結存金額
        public double F7 { get; set; }  // 盤盈虧金額
        public double F8 { get; set; }  // 周轉率

    }
    public class FA0058Repository : JCLib.Mvc.BaseRepository
    {
        public FA0058Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        private string GetQuerySql(string SET_YM, string INID_FLAG, string M_STOREID, bool clsALL, string WH_NO, string IS_INV_MINUS, string MMCODE, string IS_OUT_MINUS, string is_pmnqty_minus, string cancel_id, string is_source_c, string whno_cancel)
        {
            string inid_flag_condition = string.Empty;
            List<string> inid_flag_list = new List<string>() { "0", "1", "2", "3" };
            if (inid_flag_list.Contains(INID_FLAG))
            {
                inid_flag_condition = string.Format(@" 
                                        and c.inid_flag {0}",
                                                       INID_FLAG == "0" ? " = 'A'" :
                                                       (INID_FLAG == "1" ? "= 'B'" :
                                                       (INID_FLAG == "2" ? "= 'C" :
                                                                           " in ('A', 'B', 'C')")));
            }
            if (INID_FLAG == "4")
            {
                inid_flag_condition = @" and a.wh_no = :wh_no";
            }

            string m_storeid_condition = string.Empty;
            List<string> m_storeid_list = new List<string>() { "0", "1", "2" };
            if (m_storeid_list.Contains(M_STOREID))
            {
                m_storeid_condition = string.Format(@" 
                                        and e.m_storeid {0}",
                                                       M_STOREID == "0" ? " in ('0', '1')" :
                                                       (M_STOREID == "1" ? "= '1'" :
                                                                           "= '0'"));
            }
            string avg_price_turnover_condition = string.Empty;
            if (M_STOREID == "3")
            {
                avg_price_turnover_condition = @" and c.avg_price > 5000 and c.turnover < 1";
            }

            string sql = string.Format(@"
                     select a.wh_no, a.wh_name, 
                            round(SUM1, 2) as SUM1,
                            round(SUM2, 2) as SUM2,
                            round(sum3, 2) as sum3,
                            round(sum4, 2) as sum4,
                            round(sumtot, 2) as sumtot,
                            (case when a.sum1 + a.sumtot = 0 
                                    then 0 
                                  else round((a.sum3 / ((a.sum1 + a.sumtot) / 2)), 2) end) as wh_turnover
                       from (
                                SELECT C.WH_NO,
                                    wh_name(c.wh_no) as wh_name,
                                    sum(pmn_avgprice * p_inv_qty) as SUM1,
                                    sum(in_price * apl_inqty) as SUM2,
                                    sum(avg_price * out_qty) as sum3,
                                    sum(avg_price * inventoryqty) as sum4,
                                    sum(avg_price * inv_qty) as sumtot
                     FROM (
                             SELECT A.WH_NO,A.DATA_YM,A.MMCODE,
                                    PMN_INVQTY(A.DATA_YM,A.WH_NO,A.MMCODE) P_INV_QTY,
                                    (CASE WHEN B.WH_GRADE='1' THEN
                                      CASE WHEN B.WH_KIND='0' THEN
                                       NVL((SELECT SUMQTY FROM V_INCOSTE_UN_MN WHERE DATA_YM=A.DATA_YM AND MMCODE=A.MMCODE),0)
                                      ELSE
                                       NVL((SELECT SUMQTY FROM V_INCOST_UN_MN WHERE DATA_YM=A.DATA_YM AND MMCODE=A.MMCODE),0)
                                      END
                                     ELSE A.APL_INQTY END) APL_INQTY,
                                    (A.TRN_INQTY - A.TRN_OUTQTY) TRN_QTY,
                                    (A.ADJ_INQTY - A.ADJ_OUTQTY) ADJ_QTY,
                                    (CASE WHEN B.WH_GRADE='1' THEN A.BAK_INQTY-A.BAK_OUTQTY
                                     ELSE
                                      CASE WHEN B.WH_KIND='0' THEN
                                       A.BAK_INQTY-A.BAK_OUTQTY-
                                       NVL((SELECT SUM(TR_INV_QTY) FROM V_MN_BACK 
                                        WHERE DATA_YM=A.DATA_YM AND WH_NO=A.WH_NO AND MMCODE=A.MMCODE),0)
                                      ELSE A.BAK_INQTY-A.BAK_OUTQTY
                                      END
                                     END) BAK_QTY,
                                    (A.EXG_INQTY - A.EXG_OUTQTY) EXG_QTY,
                                    (A.MIL_INQTY - A.MIL_OUTQTY) MIL_QTY,
                                    A.REJ_OUTQTY REJ_QTY,A.DIS_OUTQTY DIS_QTY,
                                    (CASE WHEN B.WH_GRADE='1' THEN USE_QTY
                                     ELSE
                                      CASE WHEN B.WH_KIND='0' THEN
                                       USE_QTY-
                                       NVL((SELECT SUM(TR_INV_QTY) FROM V_MN_BACK 
                                        WHERE DATA_YM=A.DATA_YM AND WH_NO=A.WH_NO AND MMCODE=A.MMCODE),0)
                                      ELSE USE_QTY
                                      END
                                     END) OUT_QTY,
                                    A.INVENTORYQTY,A.INV_QTY,A.TURNOVER,A.HIGH_QTY,
                                    (CASE WHEN B.WH_GRADE='1' THEN
                                      CASE WHEN B.WH_KIND='0' THEN
                                       NVL((SELECT IN_PRICE FROM V_INCOSTE_UN_MN WHERE DATA_YM=A.DATA_YM AND MMCODE=A.MMCODE),0)
                                      ELSE
                                       NVL((SELECT IN_PRICE FROM V_INCOST_UN_MN WHERE DATA_YM=A.DATA_YM AND MMCODE=A.MMCODE),0)
                                      END
                                     ELSE AVG_PRICE_N(A.DATA_YM,A.MMCODE) END) IN_PRICE,
                                     e.pmn_avgprice PMN_AVGPRICE,
                                     e.avg_price AVG_PRICE,
                                     B.WH_KIND,B.WH_GRADE,
                                     e.m_storeid,
                                     f.cancel_id as whno_cancel
                               FROM MI_WINVMON A, MI_WHMAST B, UR_INID c, MI_WHCOST e, MI_WHMAST f
                              WHERE a.DATA_YM=:data_ym  
                                AND A.WH_NO=B.WH_NO
                                AND B.WH_KIND IN ('0','1') 
                                AND B.WH_GRADE IN ('1','2','3','4')
                                and c.inid = b.inid
                                and e.data_ym = a.data_ym
                                and e.mmcode = a.mmcode
                                and a.wh_no = f.wh_no
                                {1}
                                {2}
                                {3}
                                {4}
                                {10}
                             ) C, MI_MAST D
                      WHERE 1=1
                        and C.MMCODE=D.MMCODE
                        and d.mat_class = '02'
                        {0}
                        AND SUBSTR(C.MMCODE,1,6) NOT IN ('005HCV')
                        AND ((D.MAT_CLASS='01' AND D.E_DRUGCLASSIFY<>'9') OR
                             (D.MAT_CLASS<>'01' {9} ))
                        {5}
                        {6}
                        {7}
                        {8}
                      group by wh_no
                            ) a
            ", string.Empty
             , MMCODE == string.Empty ? string.Empty : " and a.mmcode = :mmcode"
             , (IS_INV_MINUS == "Y") ? " and a.inv_qty < 0" : string.Empty
             , inid_flag_condition
             , m_storeid_condition
             , IS_OUT_MINUS == "Y" ? " and OUT_QTY < 0" : string.Empty
             , avg_price_turnover_condition
             , is_pmnqty_minus == "Y" ? " and p_inv_qty < 0" : string.Empty
             , string.IsNullOrEmpty(cancel_id) ? string.Empty : string.Format(" and d.cancel_id = '{0}'", cancel_id)
             , string.IsNullOrEmpty(is_source_c) ? string.Empty : (is_source_c == "Y" ? " AND NVL(D.E_SOURCECODE,'N')='C'" : "AND NVL(D.E_SOURCECODE,'N')<>'C'")
             , string.IsNullOrEmpty(whno_cancel) ? string.Empty : string.Format(" and f.cancel_id = '{0}'", whno_cancel));
            return sql;
        }

        public IEnumerable<FA0023> GetFa0058(string SET_YM, string INID_FLAG, string M_STOREID, bool clsALL, string WH_NO, string IS_INV_MINUS, string MMCODE, string IS_OUT_MINUS, string is_pmnqty_minus, string cancel_id, string is_source_c, string whno_cancel)
        {
            var p = new DynamicParameters();

            p.Add("data_ym", SET_YM);
            p.Add("iniv_flag", INID_FLAG);
            p.Add("m_storeid", M_STOREID);
            p.Add("wh_no", WH_NO);

            string sql = GetQuerySql(SET_YM, INID_FLAG, M_STOREID, clsALL, WH_NO, IS_INV_MINUS, MMCODE, IS_OUT_MINUS, is_pmnqty_minus, cancel_id, is_source_c, whno_cancel);

            return DBWork.PagingQuery<FA0023>(sql, p, DBWork.Transaction);
        }

        public DataTable GetExcel(string MAT_CLASS, string SET_YM, string INID_FLAG, string M_STOREID, bool clsALL, string WH_NO, string IS_INV_MINUS, string MMCODE, string IS_OUT_MINUS, string is_pmnqty_minus, string cancel_id, string is_source_c, string whno_cancel)
        {
            var p = new DynamicParameters();

            p.Add("data_ym", SET_YM);
            p.Add("iniv_flag", INID_FLAG);
            p.Add("m_storeid", M_STOREID);
            p.Add("wh_no", WH_NO);

            string sql = string.Format(@" 
                            SELECT P.WH_NO as 單位代碼,
                                   P.WH_NAME as 單位名稱,
                                   P.SUM1 as 上月結存金額, 
                                   P.SUM2 as 本月進貨金額, 
                                   P.SUM3 as 本月消耗金額,
                                   P.SUM4 as 盤盈虧金額 ,
                                   P.SUMTOT as 本月結存金額,
                                   P.WH_TURNOVER as 周轉率
                            FROM 
                            (
                              {0}
                            ) P WHERE 1=1 order BY P.WH_NO", GetQuerySql(SET_YM, INID_FLAG, M_STOREID, clsALL, WH_NO, IS_INV_MINUS, MMCODE, IS_OUT_MINUS, is_pmnqty_minus, cancel_id, is_source_c, whno_cancel));

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<FA0058ReportMODEL> GetFa0058Report(string SET_YM, string INID_FLAG, string M_STOREID, bool clsALL, string WH_NO, string IS_INV_MINUS, string MMCODE, string IS_OUT_MINUS, string is_pmnqty_minus, string cancel_id, string is_source_c, string whno_cancel)
        {
            var p = new DynamicParameters();

            p.Add("data_ym", SET_YM);
            p.Add("iniv_flag", INID_FLAG);
            p.Add("m_storeid", M_STOREID);
            p.Add("wh_no", WH_NO);

            string sql = string.Format(@" 
                            SELECT P.WH_NO as F1,
                                   P.WH_NAME as F2,
                                   P.SUM1 as F3, 
                                   P.SUM2 as F4, 
                                   P.SUM3 as F5,
                                   P.SUM4 as F7,
                                   P.SUMTOT as F6,
                                   P.WH_TURNOVER as F8
                            FROM 
                            (
                              {0}
                            ) P WHERE 1=1 order BY P.WH_NO", GetQuerySql(SET_YM, INID_FLAG, M_STOREID, clsALL, WH_NO, IS_INV_MINUS, MMCODE, IS_OUT_MINUS, is_pmnqty_minus, cancel_id, is_source_c, whno_cancel));


            return DBWork.Connection.Query<FA0058ReportMODEL>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<ComboItemModel> GetMatCombo()
        {

            string sql = @"  SELECT MAT_CLASS VALUE,
                                    MAT_CLASS || ' ' || MAT_CLSNAME TEXT
                             FROM MI_MATCLASS
                             WHERE MAT_CLSID  IN ('2','3') 
                             ORDER BY MAT_CLASS";


            return DBWork.Connection.Query<ComboItemModel>(sql, DBWork.Transaction);
        }

        public IEnumerable<ComboItemModel> GetYMCombo()
        {
            var p = new DynamicParameters();

            string sql = @" SELECT  SET_YM VALUE, SET_YM TEXT
                            FROM    MI_MNSET
                            WHERE SET_STATUS = 'C'
                            ORDER BY SET_YM DESC ";

            return DBWork.Connection.Query<ComboItemModel>(sql, p, DBWork.Transaction);
        }

        //public IEnumerable<ComboItemModel> GetWh_no()
        //{
        //    var p = new DynamicParameters();

        //    string sql = @" SELECT 
        //                        WH_NO VALUE, WH_NO || ' ' || WH_NAME TEXT
        //                    FROM 
        //                        MI_WHMAST 
        //                    WHERE 
        //                        WH_NO = WHNO_MM1";

        //    return DBWork.Connection.Query<ComboItemModel>(sql, new { inid = DBWork.UserInfo.Inid }, DBWork.Transaction);
        //}

        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, string p1, bool clsALL, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} 
                            A.MMCODE, 
                            A.MMNAME_C, 
                            A.MMNAME_E, 
                            A.MAT_CLASS, 
                            A.BASE_UNIT 
                        FROM 
                            MI_MAST A 
                        WHERE 1=1 ";
            //AND (SELECT COUNT(*) FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO = '560000' ) > 0 ";

            if (clsALL != true)
            {
                sql += " AND A.MAT_CLASS = :MAT_CLASS ";
                p.Add(":MAT_CLASS", string.Format("{0}", p0));
            }
            else
            {
                sql += @" AND A.MAT_CLASS IN (" + p0 + ") ";
            }

            if (p1 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(A.MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(A.MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
                p.Add(":MMCODE_I", p1);
                p.Add(":MMNAME_E_I", p1);
                p.Add(":MMNAME_C_I", p1);

                sql += " AND (A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("%{0}%", p1));

                sql += " OR A.MMNAME_E LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p1));

                sql += " OR A.MMNAME_C LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p1));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX, MMCODE", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY A.MMCODE ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<MI_WHMAST> GetWH_NoCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.WH_NO, A.WH_NAME, A.WH_KIND, A.WH_GRADE 
                          FROM MI_WHMAST A 
                         WHERE 1=1 AND WH_KIND = '1' 
                           and wh_no <> WHNO_MM1";


            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.WH_NO, :WH_NO_I), 1000) + NVL(INSTR(A.WH_NAME, :WH_NAME_I), 100) * 10) IDX,"); // 設定權重, 值越小權重最大                
                p.Add(":WH_NO_I", p0);
                p.Add(":WH_NAME_I", p0);

                sql += " AND (A.WH_NO LIKE :WH_NO ";
                p.Add(":WH_NO", string.Format("%{0}%", p0));

                sql += " OR A.WH_NAME LIKE :WH_NAME) ";
                p.Add(":WH_NAME", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX, WH_NO", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY A.WH_NO ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_WHMAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);

        }
    }
}
