using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Data;
using System;
using System.Data.SqlClient;

namespace WebApp.Repository.AB
{
    public class AB0120Repository : JCLib.Mvc.BaseRepository
    {
        public AB0120Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }


        private string GetQuerySql(string MAT_CLASS, string SET_YM, string M_STOREID, bool clsALL, string WH_NO, bool notZeroOnly)
        {
            string mat_class_condition = string.Empty;
            if (!string.IsNullOrWhiteSpace(MAT_CLASS))
            {
                if (clsALL == true)
                {
                    mat_class_condition = @" AND D.MMCODE IN (select MMCODE from MI_MAST where MAT_CLASS in (" + MAT_CLASS + ")) ";
                }
                else
                {
                    mat_class_condition = @" AND D.MMCODE IN (select MMCODE from MI_MAST where MAT_CLASS = :MAT_CLASS ) ";
                }
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

            string sql = string.Format(@"
                     SELECT C.WH_NO,
                            C.DATA_YM,
                            C.MMCODE,
                            C.P_INV_QTY,
                            C.APL_INQTY as in_qty,
                            TRN_QTY,
                            ADJ_QTY,
                            EXG_QTY,
                            MIL_QTY,
                            REJ_QTY,
                            DIS_QTY,
                            INVENTORYQTY,
                            INV_QTY,
                            C.OUT_QTY,
                            C.BAK_QTY,
                            round(C.IN_PRICE, 2) as in_price,
                            round(C.PMN_AVGPRICE, 2) as pmn_avgprice,
                            round(C.DISC_CPRICE, 2) as DISC_CPRICE,
                            round(C.TURNOVER, 2) as turnover,
                            round(C.HIGH_QTY, 2) as high_qty,
                            GET_EXPLOT(C.WH_NO,C.MMCODE) AS EXPLOT,
                            D.M_CONTID,
                            D.M_PAYKIND,
                            D.M_CONSUMID,
                            D.M_TRNID,
                            M_PAYID,
                            D.MAT_CLASS,
                            D.MMNAME_C,
                            D.MMNAME_E,
                            D.BASE_UNIT,
                            WH_NAME(C.WH_NO) WH_NAME,
                            c.m_storeid,
                            round(pmn_avgprice * p_inv_qty, 2) as SUM1,
                            round(in_price * apl_inqty, 2) as SUM2,
                            round(C.DISC_CPRICE * out_qty, 2) as sum3,
                            round(C.DISC_CPRICE * inventoryqty, 2) as sum4,
                            round(C.DISC_CPRICE * inv_qty, 2) as sumtot,
                            round(p_inv_qty * (C.DISC_CPRICE-pmn_avgprice),2) as d_amt,
                            round(inv_qty - high_qty) as sub_qty,
                            round((inv_qty - high_qty) * C.DISC_CPRICE) as sub_price,
                            e.MAT_CLSNAME, 
                            c.wh_grade, c.wh_kind
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
                                     e.DISC_CPRICE,
                                     B.WH_KIND,B.WH_GRADE,
                                     e.m_storeid
                               FROM MI_WINVMON A, MI_WHMAST B, UR_INID c, MI_WHCOST e
                              WHERE a.DATA_YM=:data_ym  
                                AND A.WH_NO=B.WH_NO
                                AND B.WH_KIND IN ('0','1') 
                                AND B.WH_GRADE IN ('1','2','3','4')
                                and c.inid = b.inid
                                and e.data_ym = a.data_ym
                                and e.mmcode = a.mmcode

                                {1}
                                {3}
                             ) C, MI_MAST D, MI_MATCLASS e
                      WHERE 1=1
                        and C.MMCODE=D.MMCODE
                        and d.mat_class = e.mat_class
                        and e.mat_clsid in ('1', '2', '3')
                        {0}
                        AND NVL(D.CANCEL_ID,'N')='N' AND SUBSTR(C.MMCODE,1,6) NOT IN ('005HCV')
                        AND ((D.MAT_CLASS='01' AND D.E_DRUGCLASSIFY<>'9') OR
                             (D.MAT_CLASS<>'01' AND NVL(D.E_SOURCECODE,'N')<>'C'))
                        {2}
            ", mat_class_condition 
             , m_storeid_condition
             , notZeroOnly ? " and not (c.P_INV_QTY = 0 and c.apl_inqty = 0)" : string.Empty
             , string.IsNullOrEmpty(WH_NO) == false ? " and a.wh_no = :wh_no" : string.Empty
             );
            return sql;
        }

        public IEnumerable<AB0120M> GetAll(string MAT_CLASS, string SET_YM, string M_STOREID, bool clsALL, string WH_NO, bool notZeroOnly, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string tempsql = GetQuerySql(MAT_CLASS, SET_YM, M_STOREID, clsALL, WH_NO, notZeroOnly);

            string sql = string.Format(@" 
                            SELECT
                                a.MMCODE, --院內碼
                                (a.mat_class || ' ' || a.mat_clsname) as MAT_CLASS,
                                a.mmname_c, a.mmname_e, a.base_unit,
                                a.sum1,     --上月結存金額
                                a.sum2,     --本月進貨金額
                                a.sum3,     --本月消耗金額
                                a.sum4,     --本月盤盈虧金額
                                a.sumtot,    --本月結存金額
                                a.d_amt,    --差異金額
                                (case when  a.sum3 = 0 then 0
                                      else round((a.sum1 + a.sum2 - a.sum3 + a.sum4)/a.sum3, 2)
                                  end) as rat,  --期末比值
                                a.p_inv_qty, a.in_qty, a.out_qty, a.inventoryqty,   -- 期初結存, 本月進貨, 本月消耗, 盤盈虧
                                a.in_price, a.DISC_CPRICE, a.pmn_avgprice,    -- 進貨單價, 平均單價, 期初單價
                                a.inv_qty as tot, -- 本期結存
                                a.turnover
                             from (
                                {0}
                            )a"
                        , tempsql);

            p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS));
            p.Add(":data_ym", string.Format("{0}", SET_YM));
            p.Add(":M_STOREID", string.Format("{0}", M_STOREID));
            p.Add(":wh_no", string.Format("{0}", WH_NO));


            sql += " ORDER BY a.MAT_CLASS, a.MMCODE ";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AB0120M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<AB0120M> Print(string MAT_CLASS, string SET_YM, string M_STOREID, bool clsALL, string WH_NO, bool notZeroOnly)
        {
            var p = new DynamicParameters();

            string tempsql = GetQuerySql(MAT_CLASS, SET_YM, M_STOREID, clsALL, WH_NO, notZeroOnly);

            string sql = string.Format(@" 
                            SELECT
                                a.MMCODE, --院內碼
                                (a.mat_class || ' ' || a.mat_clsname) as MAT_CLASS,
                                a.mmname_c, a.mmname_e, a.base_unit,
                                a.sum1,     --上月結存金額
                                a.sum2,     --本月進貨金額
                                a.sum3,     --本月消耗金額
                                a.sum4,     --本月盤盈虧金額
                                a.sumtot,    --本月結存金額
                                a.d_amt,    --差異金額
                                (case when  a.sum3 = 0 then 0
                                      else round((a.sum1 + a.sum2 - a.sum3 + a.sum4)/a.sum3, 2)
                                  end) as rat,  --期末比值
                                a.p_inv_qty, a.in_qty, a.out_qty, a.inventoryqty,   -- 期初結存, 本月進貨, 本月消耗, 盤盈虧
                                a.in_price, a.DISC_CPRICE, a.pmn_avgprice,    -- 進貨單價, 平均單價, 期初單價
                                a.inv_qty as tot, -- 本期結存
                                a.turnover
                             from (
                                {0}
                            )a"
                        , tempsql);

            p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS));
            p.Add(":data_ym", string.Format("{0}", SET_YM));
            p.Add(":M_STOREID", string.Format("{0}", M_STOREID));
            p.Add(":wh_no", string.Format("{0}", WH_NO));


            sql += " ORDER BY a.MAT_CLASS, a.MMCODE ";

            return DBWork.Connection.Query<AB0120M>(sql, p, DBWork.Transaction);
        }

        public DataTable GetExcel(string MAT_CLASS, string SET_YM, string M_STOREID, bool clsALL, string WH_NO, bool notZeroOnly)
        {
            var p = new DynamicParameters();

            string tempsql = GetQuerySql(MAT_CLASS, SET_YM, M_STOREID, clsALL, WH_NO, notZeroOnly);

            string sql = string.Format(@" 
                            SELECT
                                a.data_ym as 成本年月,
                                a.MMCODE as 院內碼, 
                                (a.mat_class || ' ' || a.mat_clsname) as 物料分類,
                                a.mmname_c as 中文品名, 
                                a.mmname_e as 英文品名, 
                                a.base_unit as 計量單位,
                                a.p_inv_qty as 上月結存,
                                a.PMN_AVGPRICE as 上期結存單價,
                                a.sum1 as 上月結存金額,
                                a.IN_QTY as 本月進貨,
                                a.IN_PRICE as 進貨單價,
                                a.sum2 as 本月進貨金額,
                                a.OUT_QTY as 本月消耗,
                                a.DISC_CPRICE as 消耗單價,
                                a.sum3 as 本月消耗金額, 
                                a.inv_qty as 本期結存,
                                a.d_amt as 差異金額,
                                a.DISC_CPRICE as 期末單價,
                                a.sumtot as 本月結存金額,  
                                a.inventoryqty as 本月盤盈虧,
                                a.sum4 as 本月盤盈虧金額,     
                                (case when  a.sum3 = 0 then 0
                                      else round((a.sum1 + a.sum2 - a.sum3 + a.sum4)/a.sum3, 2)
                                  end) as 期末比值,  
                                a.turnover as 周轉率
                             from (
                                {0}
                            )a"
                        , tempsql);

            p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS));
            p.Add(":data_ym", string.Format("{0}", SET_YM));
            p.Add(":M_STOREID", string.Format("{0}", M_STOREID));
            p.Add(":wh_no", string.Format("{0}", WH_NO));


            sql += " ORDER BY a.MAT_CLASS, a.MMCODE ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public string GetWH_KIND(string wh_no)
        {
            var p = new DynamicParameters();

            string sql = @" SELECT WH_KIND
                            FROM MI_WHMAST
                            WHERE WH_NO = :wh_no
                            ";

            p.Add(":wh_no", string.Format("{0}", wh_no));

            return DBWork.Connection.ExecuteScalar(sql, p, DBWork.Transaction).ToString();
        }

        public IEnumerable<ComboItemModel> GetMatCombo(string wh_kind)
        {
            var p = new DynamicParameters();

            string sql = @"";
            if (wh_kind == "0")
            {
                sql += @" SELECT MAT_CLASS AS VALUE, (SELECT MAT_CLASS || ' ' || MAT_CLSNAME FROM MI_MATCLASS WHERE MAT_CLASS = A.MAT_CLASS) AS TEXT FROM MI_MATCLASS A WHERE MAT_CLSID = '1' ";
            }
            else
            {
                sql += @" SELECT MAT_CLASS AS VALUE, (SELECT MAT_CLASS || ' ' || MAT_CLSNAME FROM MI_MATCLASS WHERE MAT_CLASS = A.MAT_CLASS) AS TEXT FROM MI_MATCLASS A WHERE MAT_CLSID IN ('2', '3') ";
            }

            p.Add(":wh_kind", string.Format("{0}", wh_kind));

            return DBWork.Connection.Query<ComboItemModel>(sql, p, DBWork.Transaction);
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

        public IEnumerable<ComboItemModel> GetWHCombo(string userid)
        {
            var p = new DynamicParameters();

            string sql = @" SELECT DISTINCT WH_NO AS VALUE, (select WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO = A.WH_NO) AS TEXT
                            FROM MI_WHID A
                            WHERE WH_USERID = (:p0)
                            UNION
                            SELECT DISTINCT WH_NO AS VALUE, (select WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO = A.WH_NO) AS TEXT
                            FROM MI_WHMAST A
                            WHERE INID = USER_INID(:p0) AND WH_KIND='1'
                            ";

            p.Add(":p0", string.Format("{0}", userid));

            return DBWork.Connection.Query<ComboItemModel>(sql, p, DBWork.Transaction);
        }

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
                sql = string.Format(sql, "(NVL(INSTR(UPPER(A.MMCODE), UPPER(:MMCODE_I)), 1000) + NVL(INSTR(UPPER(A.MMNAME_E), UPPER(:MMNAME_E_I)), 100) * 10 + NVL(INSTR(UPPER(A.MMNAME_C), UPPER(:MMNAME_C_I)), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
                p.Add(":MMCODE_I", p1);
                p.Add(":MMNAME_E_I", p1);
                p.Add(":MMNAME_C_I", p1);

                sql += " AND (UPPER(A.MMCODE) LIKE UPPER(:MMCODE) ";
                p.Add(":MMCODE", string.Format("%{0}%", p1));

                sql += " OR UPPER(A.MMNAME_E) LIKE UPPER(:MMNAME_E) ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p1));

                sql += " OR UPPER(A.MMNAME_C) LIKE UPPER(:MMNAME_C)) ";
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

        public string getWhName(string wh_no)
        {
            string sql = @" SELECT  wh_name
                            FROM    MI_WHMAST
                            WHERE   wh_no = :wh_no";

            var str = DBWork.Connection.ExecuteScalar(sql, new { wh_no = wh_no }, DBWork.Transaction);
            return str == null ? "" : str.ToString();
        }

        public string getWH_NAME()
        {
            string sql = @" SELECT  WH_NAME
                            FROM    MI_WHMAST
                            WHERE   WH_NO = WHNO_MM1 ";

            var str = DBWork.Connection.ExecuteScalar(sql, new { userID = DBWork.ProcUser }, DBWork.Transaction);
            return str == null ? "" : str.ToString();
        }

    }
}