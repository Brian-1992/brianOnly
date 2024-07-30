using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Data;
using System;
using System.Data.SqlClient;

namespace WebApp.Repository.AA
{
    public class AA0104Repository : JCLib.Mvc.BaseRepository
    {
        public AA0104Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        private string GetQuerySql(string MAT_CLASS, string SET_YM, string INID_FLAG, string M_STOREID, string WH_NO)
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
                     SELECT C.WH_NO,
                            D.MMNAME_C,
                            D.MMNAME_E,
                            D.BASE_UNIT,
                            WH_NAME(C.WH_NO) WH_NAME,
                            C.DATA_YM,
                            C.MMCODE,
                            round(C.PMN_AVGPRICE, 2) as pmn_avgprice,       --期初單價,
                            c.p_inv_qty,                                    --期初結存,
                            round(pmn_avgprice * c.p_inv_qty, 2) as SUM1,   --期初金額,
                            c.in_price,                                     --進貨單價,
                            c.apl_inqty as in_qty,                          --本月進貨,
                            round(in_price * apl_inqty, 2) as SUM2,         --進貨金額,
                            avg_price,                                      --消耗單價,
                            out_qty,                                        --本月消耗,
                            round(avg_price * out_qty, 2) as sum3,          --消耗金額,
                            inventoryqty,                                   --盤盈虧數量,
                            round(avg_price * inventoryqty, 2) as sum4,     --盤盈虧金額,
                            --avg_price,                                      --本月單價,
                            inv_qty,                                        --本月結存,
                            round(avg_price * inv_qty, 2) as sumtot         --結存金額,
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
                                     e.m_storeid
                               FROM MI_WINVMON A, MI_WHMAST B, UR_INID c, MI_WHCOST e
                              WHERE a.DATA_YM=:data_ym  
                                AND A.WH_NO=B.WH_NO
                                AND B.WH_KIND = '1' 
                                AND B.WH_GRADE IN ('1','2','3','4')
                                and c.inid = b.inid
                                and e.data_ym = a.data_ym
                                and e.mmcode = a.mmcode
                                {0}
                                {1}
                             ) C, MI_MAST D, MI_MATCLASS e
                      WHERE 1=1
                        and C.MMCODE=D.MMCODE
                        and d.mat_class = e.mat_class
                        and e.mat_clsid in ('2', '3')
                        AND NVL(D.CANCEL_ID,'N')='N' 
                        AND SUBSTR(C.MMCODE,1,6) NOT IN ('005HCV')
                        AND NVL(D.E_SOURCECODE,'N')<>'C'
                        {2}
                        {3}
            ", inid_flag_condition
             , m_storeid_condition
             , avg_price_turnover_condition
             , MAT_CLASS == string.Empty ? string.Empty : " and d.mat_class = :mat_class");
            return sql;
        }

        public IEnumerable<AA0104M> GetAll(string MAT_CLASS, string SET_YM, string INID_FLAG, string M_STOREID,  string WH_NO)
        {
            var p = new DynamicParameters();

            p.Add(":mat_class", string.Format("{0}", MAT_CLASS));
            p.Add(":data_ym", string.Format("{0}", SET_YM));
            p.Add(":wh_no", string.Format("{0}", WH_NO));
            p.Add(":INID_FLAG", string.Format("{0}", INID_FLAG));
            p.Add(":M_STOREID", string.Format("{0}", M_STOREID));

            string sql = GetQuerySql(MAT_CLASS, SET_YM, INID_FLAG, M_STOREID, WH_NO);

            return DBWork.PagingQuery<AA0104M>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<AA0104M> Print(string MAT_CLASS, string SET_YM, string INID_FLAG, string M_STOREID, string WH_NO)
        {
            var p = new DynamicParameters();

            p.Add(":mat_class", string.Format("{0}", MAT_CLASS));
            p.Add(":data_ym", string.Format("{0}", SET_YM));
            p.Add(":wh_no", string.Format("{0}", WH_NO));
            p.Add(":INID_FLAG", string.Format("{0}", INID_FLAG));
            p.Add(":M_STOREID", string.Format("{0}", M_STOREID));

            string sql = string.Format(@"
                            select * from (
                              {0}
                            ) order by wh_no, mmcode
                         ", GetQuerySql(MAT_CLASS, SET_YM, INID_FLAG, M_STOREID, WH_NO));

            return DBWork.Connection.Query<AA0104M>(sql, p, DBWork.Transaction);
        }

        public DataTable GetExcel(string MAT_CLASS, string SET_YM, string INID_FLAG, string M_STOREID, string WH_NO)
        {
            var p = new DynamicParameters();

            p.Add(":mat_class", string.Format("{0}", MAT_CLASS));
            p.Add(":data_ym", string.Format("{0}", SET_YM));
            p.Add(":wh_no", string.Format("{0}", WH_NO));
            p.Add(":INID_FLAG", string.Format("{0}", INID_FLAG));
            p.Add(":M_STOREID", string.Format("{0}", M_STOREID));

            string sql = string.Format(@"
                            select wh_no as 單位代碼,
                                   wh_name as 單位名稱,
                                   data_ym as 成本年月,
                                   mmname_c as 中文品名,
                                   mmname_e as 英文品名,
                                   pmn_avgprice as 期初單價,
                                   p_inv_qty as 期初結存,
                                   SUM1 as 期初金額,
                                   in_price as 進貨單價,
                                   in_qty as 本月進貨,
                                   SUM2 as 進貨金額,
                                   avg_price as 消耗單價,
                                   out_qty as 本月消耗,
                                   sum3 as 消耗金額,
                                   inventoryqty as 盤盈虧數量,
                                   sum4 as 盤盈虧金額,
                                   avg_price as 本月單價,
                                   inv_qty as 本月結存,
                                   sumtot as 結存金額
                              from (
                                   {0}
                                   ) order by wh_no, mmcode
                         ", GetQuerySql(MAT_CLASS, SET_YM, INID_FLAG, M_STOREID, WH_NO));

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<ComboItemModel> GetMatCombo()
        {

            string sql = @"  SELECT MAT_CLASS VALUE,
                                    MAT_CLASS || ' ' || MAT_CLSNAME TEXT
                             FROM MI_MATCLASS
                             WHERE MAT_CLSID  IN ('2','3') 
                             ORDER BY MAT_CLASS";
            

            return DBWork.Connection.Query<ComboItemModel>(sql,  DBWork.Transaction);
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
                            MI_MAST A , MI_MATCLASS B
                        WHERE 1=1 
                          and A.mat_class = b.mat_class
                          and b.mat_clsid in ('2', '3')
";
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

        public string getDeptName()
        {
            string sql = @" SELECT  INID_NAME AS USER_DEPTNAME
                            FROM    UR_INID
                            WHERE   INID = (select INID from UR_ID where TUSER = (:userID)) ";

            var str = DBWork.Connection.ExecuteScalar(sql, new { userID = DBWork.ProcUser }, DBWork.Transaction);
            return str == null ? "" : str.ToString();
        }

        public IEnumerable<MI_WHMAST> GetWH_NoCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.WH_NO, A.WH_NAME, A.WH_KIND, A.WH_GRADE FROM MI_WHMAST A WHERE 1=1 AND WH_KIND = '1'  ";


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