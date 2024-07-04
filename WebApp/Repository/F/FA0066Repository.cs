using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Data;
using System;
using System.Data.SqlClient;

namespace WebApp.Repository.F
{
    public class FA0066Repository : JCLib.Mvc.BaseRepository
    {
        public FA0066Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        private string GetQuerySql(string MAT_CLASS, string SET_YM, string INID_FLAG, string M_STOREID, bool clsALL, string WH_NO, string IS_INV_MINUS, string MMCODE, string IS_OUT_MINUS, string is_pmnqty_minus, string cancel_id, string is_source_c, string whno_cancel)
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
                avg_price_turnover_condition = @" and c.DISC_CPRICE > 5000 and c.turnover < 1";
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
                            round(p_inv_qty*(C.DISC_CPRICE-C.PMN_AVGPRICE),2) as D_AMT,
                            round(inv_qty - high_qty) as sub_qty,
                            round((inv_qty - high_qty) * C.DISC_CPRICE) as sub_price,
                            (case when d.cancel_id = 'Y' then '是' else '否' end) as cancel_id,
                            (case when(NVL(D.E_SOURCECODE,'N') = 'C') then '是' else '否' end) as IS_SOURCE_C,
                            (case when c.whno_cancel = 'Y' then '是' else '否' end) as whno_cancel
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
                                     e.DISC_CPRICE DISC_CPRICE,
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
                             ) C, MI_MAST D, MI_MATCLASS e
                      WHERE 1=1
                        and C.MMCODE=D.MMCODE
                        and d.mat_class = e.mat_class
                        and e.mat_clsid in ('1', '2', '3')
                        {0}
                        AND SUBSTR(C.MMCODE,1,6) NOT IN ('005HCV')
                        AND ((D.MAT_CLASS='01' AND D.E_DRUGCLASSIFY<>'9') OR
                             (D.MAT_CLASS<>'01' {9} ))
                        {5}
                        {6}
                        {7}
                        {8}
            ", MAT_CLASS == string.Empty ? string.Empty : " and d.mat_class = :mat_class"
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

        public IEnumerable<FA0066> GetAll(string MAT_CLASS, string SET_YM, string INID_FLAG, string M_STOREID, bool clsALL, string WH_NO, string IS_INV_MINUS, string MMCODE, string IS_OUT_MINUS, string is_pmnqty_minus, string cancel_id, string is_source_c, string whno_cancel)
        {
            var p = new DynamicParameters();

            string sql = GetQuerySql(MAT_CLASS, SET_YM, INID_FLAG, M_STOREID, clsALL, WH_NO, IS_INV_MINUS, MMCODE, IS_OUT_MINUS, is_pmnqty_minus, cancel_id, is_source_c, whno_cancel);

            p.Add(":mat_class", string.Format("{0}", MAT_CLASS));
            p.Add(":data_ym", string.Format("{0}", SET_YM));
            p.Add(":wh_no", string.Format("{0}", WH_NO));
            p.Add(":mmcode", string.Format("{0}", MMCODE));

            return DBWork.PagingQuery<FA0066>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<FA0066> Print(string MAT_CLASS, string SET_YM, string INID_FLAG, string M_STOREID, bool clsALL, string WH_NO, string IS_INV_MINUS, string MMCODE, string IS_OUT_MINUS, string is_pmnqty_minus, string cancel_id, string is_source_c, string whno_cancel)
        {
            var p = new DynamicParameters();

            string sql = GetQuerySql(MAT_CLASS, SET_YM, INID_FLAG, M_STOREID, clsALL, WH_NO, IS_INV_MINUS, MMCODE, IS_OUT_MINUS, is_pmnqty_minus, cancel_id, is_source_c, whno_cancel);
            sql += "  order by wh_no, mmcode";

            p.Add(":mat_class", string.Format("{0}", MAT_CLASS));
            p.Add(":data_ym", string.Format("{0}", SET_YM));
            p.Add(":wh_no", string.Format("{0}", WH_NO));
            p.Add(":mmcode", string.Format("{0}", MMCODE));

            return DBWork.Connection.Query<FA0066>(sql, p, DBWork.Transaction);
        }

        public DataTable GetExcel(string MAT_CLASS, string SET_YM, string INID_FLAG, string M_STOREID, bool clsALL, string WH_NO, string IS_INV_MINUS, string MMCODE, string IS_OUT_MINUS, string is_pmnqty_minus, string cancel_id, string is_source_c, string whno_cancel
                                , string T3P1, string T3P2, string T3P3, string T3P4, string T3P5, string T3P6, string T3P7, string T3P8, string T3P9, string T3P10
                                , string T3P11, string T3P12, string T3P13, string T3P14, string T3P15, string T3P16, string T3P17, string T3P18, string T3P19, string T3P20
                                , string T3P21, string T3P22, string T3P23, string T3P24, string T3P25, string T3P26, string T3P27, string T3P28, string T3P29, string T3P30, string T3P31
                                , string T3P32, string T3P33, string T3P34)
        {
            var p = new DynamicParameters();

            string sql = GetQuerySql(MAT_CLASS, SET_YM, INID_FLAG, M_STOREID, clsALL, WH_NO, IS_INV_MINUS, MMCODE, IS_OUT_MINUS, is_pmnqty_minus, cancel_id, is_source_c, whno_cancel);

            sql = string.Format(@" select wh_no as 單位代碼,
                                          trim(replace(wh_name, '　', ' ')) as 單位名稱,
                                          data_ym as 成本年月,
                                          mmcode as 院內碼,
                                          mmname_e as 英文品名,
                                          mmname_c as 中文品名,
                                          base_unit as 計量單位
                                          {1}
                                          {2}
                                          {3}
                                          {4}
                                          {5}
                                          {6}
                                          {7}
                                          {8}
                                          {9}
                                          {10}
                                          {11}
                                          {12}
                                          {13}
                                          {14}
                                          {15}
                                          {16}
                                          {17}
                                          {18}
                                          {19}
                                          {20}
                                          {21}
                                          {22}
                                          {23}
                                          {24}
                                          {25}
                                          {32}
                                          {26}
                                          {27}
                                          {28}
                                          {29}
                                          {30}
                                          {31}
                                          {33}
                                          {34}
                                     from (
                                            {0}
                                      )
                                    order by wh_no, mmcode"
                                , sql
                                , T3P1 == "Y" ? ",p_inv_qty as 期初數量" : string.Empty
                                , T3P2 == "Y" ? ",in_qty as 進貨數量" : string.Empty
                                , T3P3 == "Y" ? ",out_qty as 消耗數量" : string.Empty
                                , T3P4 == "Y" ? ",inv_qty as 結存數量" : string.Empty
                                , T3P5 == "Y" ? ",inventoryqty as 盤盈虧數量" : string.Empty
                                , T3P6 == "Y" ? ",sum1 as 期初成本" : string.Empty
                                , T3P7 == "Y" ? ",sum2 as 進貨成本" : string.Empty
                                , T3P8 == "Y" ? ",sum3 as 消耗金額" : string.Empty
                                , T3P9 == "Y" ? ",sumtot as 期末成本" : string.Empty
                                , T3P34 == "Y" ? ",d_amt as 差異金額" : string.Empty
                                , T3P10 == "Y" ? ",sum4 as 盤盈虧金額" : string.Empty
                                , T3P11 == "Y" ? ",high_qty as 基準量" : string.Empty
                                , T3P12 == "Y" ? ",DISC_CPRICE as 庫存單價" : string.Empty
                                , T3P13 == "Y" ? ",sub_qty as 超量" : string.Empty
                                , T3P14 == "Y" ? ",sub_price as 超量金額" : string.Empty
                                , T3P15 == "Y" ? ",turnover as 週轉率" : string.Empty
                                , T3P16 == "Y" ? ",in_price as 進貨單價" : string.Empty
                                , T3P17 == "Y" ? ",m_contid as 小採否" : string.Empty
                                , T3P18 == "Y" ? ",m_storeid as 庫備否" : string.Empty
                                , T3P19 == "Y" ? ",m_paykind as 給付類別" : string.Empty
                                , T3P20 == "Y" ? ",m_consumid as 是否半消耗品" : string.Empty
                                , T3P21 == "Y" ? ",m_trnid as 是否扣庫" : string.Empty
                                , T3P22 == "Y" ? ",m_payid as 是否計價" : string.Empty
                                , T3P23 == "Y" ? ",explot as 效期" : string.Empty
                                , T3P24 == "Y" ? ",trn_qty as 調撥" : string.Empty
                                , T3P25 == "Y" ? ",bak_qty as 繳回" : string.Empty
                                , T3P26 == "Y" ? ",rej_qty as 退貨" : string.Empty
                                , T3P27 == "Y" ? ",dis_qty as 報廢" : string.Empty
                                , T3P28 == "Y" ? ",exg_qty as 換貨" : string.Empty
                                , T3P29 == "Y" ? ",mil_qty as 戰備換貨" : string.Empty
                                , T3P30 == "Y" ? ",cancel_id as 院內碼是否作廢" : string.Empty
                                , T3P31 == "Y" ? ",is_source_c as 是否寄售" : string.Empty
                                , T3P32 == "Y" ? ",adj_qty as 調帳" : string.Empty
                                , T3P33 == "Y" ? ",whno_cancel as 庫房是否作廢" : string.Empty);

            p.Add(":mat_class", string.Format("{0}", MAT_CLASS));
            p.Add(":data_ym", string.Format("{0}", SET_YM));
            p.Add(":wh_no", string.Format("{0}", WH_NO));
            p.Add(":mmcode", string.Format("{0}", MMCODE));

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
                             WHERE MAT_CLSID  IN ('1','2','3') 
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

        public string getDeptName()
        {
            string sql = @" SELECT  INID_NAME AS USER_DEPTNAME
                            FROM    UR_INID
                            WHERE   INID = (select INID from UR_ID where TUSER = (:userID)) ";

            var str = DBWork.Connection.ExecuteScalar(sql, new { userID = DBWork.ProcUser }, DBWork.Transaction);
            return str == null ? "" : str.ToString();
        }
        public string getMatclassName(string id)
        {
            string sql = @" SELECT MAT_CLSNAME FROM MI_MATCLASS WHERE MAT_CLASS = :MATCLASS";

            var str = DBWork.Connection.ExecuteScalar(sql, new { MATCLASS = id }, DBWork.Transaction);
            return str == null ? "全部分" : str.ToString();
        }

        public IEnumerable<MI_WHMAST> GetWH_NoCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.WH_NO, A.WH_NAME, A.WH_KIND, A.WH_GRADE FROM MI_WHMAST A WHERE 1=1 AND WH_KIND IN ('0', '1')  ";


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

        public IEnumerable<MI_MAST> GetMMCodeComboQ(string p0, string p1, bool clsALL, int page_index, int page_size, string sorters)
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

        public IEnumerable<COMBO_MODEL> GetYNCombo()
        {
            string sql = @"
                select data_value as value, data_desc as text
                  from PARAM_D
                 where grp_code = 'Y_OR_N'
                 order by data_seq
            ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, DBWork.Transaction);
        }
    }
}