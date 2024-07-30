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
    public class AA0106Repository : JCLib.Mvc.BaseRepository
    {
        public AA0106Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        private string GetQuerySql(string MAT_CLASS, string SET_YM, string INID_FLAG, string M_STOREID, bool clsALL)
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
                            round(C.AVG_PRICE, 2) as avg_price,
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
                            pmn_avgprice * p_inv_qty as SUM1,
                            in_price * apl_inqty as SUM2,
                            avg_price * out_qty as sum3,
                            avg_price * inventoryqty as sum4,
                            avg_price * inv_qty as sumtot,
                            round(inv_qty - high_qty) as sub_qty,
                            round((inv_qty - high_qty) * avg_price) as sub_price,
                            e.MAT_CLSNAME
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
                                AND B.WH_KIND IN ('0','1') 
                                AND B.WH_GRADE IN ('1','2','3','4')
                                and c.inid = b.inid
                                and e.data_ym = a.data_ym
                                and e.mmcode = a.mmcode
                                {1}
                                {2}
                                {3}
                                {4}
                             ) C, MI_MAST D, MI_MATCLASS e
                      WHERE 1=1
                        and C.MMCODE=D.MMCODE
                        and d.mat_class = e.mat_class
                        and e.mat_clsid in ('1', '2', '3')
                        {0}
                        AND NVL(D.CANCEL_ID,'N')='N' AND SUBSTR(C.MMCODE,1,6) NOT IN ('005HCV')
                        AND ((D.MAT_CLASS='01' AND D.E_DRUGCLASSIFY<>'9') OR
                             (D.MAT_CLASS<>'01' AND NVL(D.E_SOURCECODE,'N')<>'C'))
                        {5}
                        {6}
                        {7}
            ", mat_class_condition
             , string.Empty //MMCODE == string.Empty ? string.Empty : " and a.mmcode = :mmcode"
             , string.Empty //(IS_INV_MINUS == "Y") ? " and a.inv_qty < 0" : string.Empty
             , inid_flag_condition
             , m_storeid_condition
             , string.Empty //IS_OUT_MINUS == "Y" ? " and OUT_QTY < 0" : string.Empty
             , string.Empty //avg_price_turnover_condition
             , string.Empty //is_pmnqty_minus == "Y" ? " and p_inv_qty < 0" : string.Empty
             );

            return sql;
        }

        public IEnumerable<AA0106M> GetAll(string MAT_CLASS, string SET_YM, string INID_FLAG, string M_STOREID, bool clsALL, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string temp_sql = GetQuerySql(MAT_CLASS, SET_YM, INID_FLAG, M_STOREID, clsALL);

            string sql = string.Format(@"
                            select a.wh_no, a.wh_name,
                                   round(sum(a.sum1), 2) as sum1,   --上月結存金額
                                   round(sum(a.sum2), 2) as sum2,   --本月進貨金額
                                   round(sum(a.sum3), 2) as sum3,   --本月消耗金額
                                   round(sum(a.sum4), 2) as sum4,   --本月盤盈虧金額
                                   round(sum(a.sumtot), 2) as sumtot,   --本月結存金額
                                   (case when  sum(a.sum3) = 0 then 0
                                      else round((sum(a.sum1) + sum(a.sum2) - sum(a.sum3) + sum(a.sum4))/sum(a.sum3), 2)
                                  end) as rat  --期末比值
                             from (
                                    {0}
                                  ) a
                            group by a.wh_no, a.wh_name
                            order by a.wh_no
                        ", temp_sql);

            
            p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS));
            p.Add(":data_ym", string.Format("{0}", SET_YM));
            p.Add(":INID_FLAG", string.Format("{0}", INID_FLAG));
            p.Add(":M_STOREID", string.Format("{0}", M_STOREID));
            p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS));
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0106M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<AA0106M> Print(string MAT_CLASS, string SET_YM, string INID_FLAG, string M_STOREID, bool clsALL)
        {
            var p = new DynamicParameters();

            string temp_sql = GetQuerySql(MAT_CLASS, SET_YM, INID_FLAG, M_STOREID, clsALL);

            string sql = string.Format(@"
                            select a.wh_no, a.wh_name,
                                   round(sum(a.sum1), 2) as sum1,   --上月結存金額
                                   round(sum(a.sum2), 2) as sum2,   --本月進貨金額
                                   round(sum(a.sum3), 2) as sum3,   --本月消耗金額
                                   round(sum(a.sum4), 2) as sum4,   --本月盤盈虧金額
                                   round(sum(a.sumtot), 2) as sumtot,   --本月結存金額
                                   (case when  sum(a.sum3) = 0 then 0
                                      else round((sum(a.sum1) + sum(a.sum2) - sum(a.sum3) + sum(a.sum4))/sum(a.sum3), 2)
                                  end) as rat  --期末比值
                             from (
                                    {0}
                                  ) a
                            group by a.wh_no, a.wh_name
                            order by a.wh_no
                        ", temp_sql);


            p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS));
            p.Add(":data_ym", string.Format("{0}", SET_YM));
            p.Add(":INID_FLAG", string.Format("{0}", INID_FLAG));
            p.Add(":M_STOREID", string.Format("{0}", M_STOREID));
            p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS));

            return DBWork.Connection.Query<AA0106M>(sql, p, DBWork.Transaction);
        }

        public DataTable GetExcel(string MAT_CLASS, string SET_YM, string INID_FLAG, string M_STOREID, bool clsALL)
        {
            var p = new DynamicParameters();

            string temp_sql = GetQuerySql(MAT_CLASS, SET_YM, INID_FLAG, M_STOREID, clsALL);

            string mat_class_name = MAT_CLASS.IndexOf(",") > -1 ? "'全部類'" : " (mat_class || ' '||MAT_CLSNAME) ";

            string sql = string.Format(@"
                            select {1} as 物料分類,
                                   a.wh_no as 單位名稱, 
                                   a.wh_name as 單位代碼,
                                   round(sum(a.sum1), 2) as 上月結存金額,   --上月結存金額
                                   round(sum(a.sum2), 2) as 本月進貨金額,   --本月進貨金額
                                   round(sum(a.sum3), 2) as 本月消耗金額,   --本月消耗金額
                                   round(sum(a.sumtot), 2) as 本月結存金額,   --本月結存金額
                                   round(sum(a.sum4), 2) as 本月盤盈虧金額,   --本月盤盈虧金額
                                   (case when  sum(a.sum3) = 0 then 0
                                      else round((sum(a.sum1) + sum(a.sum2) - sum(a.sum3) + sum(a.sum4))/sum(a.sum3), 2)
                                  end) as 期末比值  --期末比值
                             from (
                                    {0}
                                  ) a
                            group by a.wh_no, a.wh_name {2}
                            order by a.wh_no
                        ", temp_sql
                         , mat_class_name
                         , MAT_CLASS.IndexOf(",") > -1 ? string.Empty : ", a.mat_class, a.mat_clsname");


            p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS));
            p.Add(":data_ym", string.Format("{0}", SET_YM));
            p.Add(":INID_FLAG", string.Format("{0}", INID_FLAG));
            p.Add(":M_STOREID", string.Format("{0}", M_STOREID));
            p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS));

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<ComboItemModel> GetMatCombo()
        {
            var p = new DynamicParameters();

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

        public string getDeptName()
        {
            string sql = @" SELECT  INID_NAME AS USER_DEPTNAME
                            FROM    UR_INID
                            WHERE   INID = (select INID from UR_ID where TUSER = (:userID)) ";

            var str = DBWork.Connection.ExecuteScalar(sql, new { userID = DBWork.ProcUser }, DBWork.Transaction);
            return str == null ? "" : str.ToString();
        }

    }
}