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
    public class AA0074Repository : JCLib.Mvc.BaseRepository
    {
        public AA0074Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AA0074M> GetAll(string MAT_CLASS, string DATA_YM_B, string DATA_YM_E, string M_STOREID, string MIL, string MMCODE, bool clsALL, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = @"  SELECT
                                A.MMCODE,   --院內碼
                                -- (select MMNAME_C || ' ' || MMNAME_E from MI_MAST where MMCODE = A.MMCODE) AS MMNAME,    --品名
                                (select MMNAME_C from MI_MAST where MMCODE = A.MMCODE) AS MMNAME_C,     --中文品名
                                (select MMNAME_E from MI_MAST where MMCODE = A.MMCODE) AS MMNAME_E,     --英文品名
                                (case when A.WH_NO = '560000' then '民' when A.WH_NO = 'MM1X' then '軍' end) AS MIL,  --軍民別
                                (select (SELECT MAT_CLASS || ' ' || MAT_CLSNAME FROM MI_MATCLASS WHERE MAT_CLASS = MI_MAST.MAT_CLASS) from MI_MAST where MMCODE = A.MMCODE) AS MAT_CLASS,   --物料分類
                                (select BASE_UNIT from MI_MAST where MMCODE = A.MMCODE) AS BASE_UNIT,   --計量單位
                                A.DATA_YM,  --年月
                                PMN_INVQTY AS EX_INV_QTY,    --上期結存
                                IN_INVQTY AS INV_QTY_INCR, --本期增加
                                OUT_INVQTY AS INV_QTY_DECR, --本期減少
                                A.INV_QTY,  --本期結存
                                (case B.M_CONTID when '0' then 'Y' when '2' then 'N' else M_CONTID end) AS M_CONTID,   --是否合約
                                B.M_AGENNO, --廠編
                                (select (SELECT RTRIM(AGEN_NAMEC, '　') FROM PH_VENDER WHERE AGEN_NO = MI_MAST.M_AGENNO) from MI_MAST where MMCODE = A.MMCODE) AS AGEN_NAMEC, --廠商名稱
                                SUM_APL_OUTQTY(:DATA_YM_B,A.DATA_YM,A.WH_NO,A.MMCODE) AS APL_OUTQTY, --累計撥發量
                                a.apl_inqty --進貨量
                            FROM
                                V_INVQTY_YM A, MI_MAST B
                            WHERE
                                A.MMCODE = B.MMCODE  ";


            if (!string.IsNullOrWhiteSpace(MAT_CLASS))
            {
                if (clsALL == true)
                {
                    sql += @" AND A.MMCODE IN (select MMCODE from MI_MAST where MAT_CLASS in (" + MAT_CLASS + ")) ";
                }
                else
                {
                    sql += @" AND A.MMCODE IN (select MMCODE from MI_MAST where MAT_CLASS = :MAT_CLASS ) ";
                    p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS));
                }
            }

            if (!string.IsNullOrWhiteSpace(DATA_YM_B))
            {
                sql += " AND TO_DATE(DATA_YM + 191100, 'yyyy/MM') >= TO_DATE(:DATA_YM_B + 191100, 'yyyy/MM') ";
                p.Add(":DATA_YM_B", string.Format("{0}", DATA_YM_B));
            }

            if (!string.IsNullOrWhiteSpace(DATA_YM_E))
            {
                sql += " AND TO_DATE(DATA_YM + 191100, 'yyyy/MM') <= TO_DATE(:DATA_YM_E + 191100, 'yyyy/MM') ";
                p.Add(":DATA_YM_E", string.Format("{0}", DATA_YM_E));
            }

            if (!string.IsNullOrWhiteSpace(M_STOREID))
            {
                switch (M_STOREID)
                {
                    case "0":   //radio選庫備
                        sql += @" AND A.MMCODE IN (select MMCODE from MI_MAST where M_STOREID = '1' and M_APPLYID not in ('P', 'E')) ";
                        break;
                    case "1":   //radio選非庫備
                        sql += @" AND A.MMCODE IN (select MMCODE from MI_MAST where M_STOREID = '0' )";
                        break;
                    case "2":   //radio選庫備品(管控項目)
                        sql += @" AND A.MMCODE IN (select MMCODE from MI_MAST where M_STOREID = '1' and M_APPLYID in ('P', 'E')) ";
                        break;
                    default:
                        break;
                }
                p.Add(":M_STOREID", string.Format("{0}", M_STOREID));
            }

            if (!string.IsNullOrWhiteSpace(MIL))
            {
                switch (MIL)
                {
                    case "0":   //全部
                        sql += @" AND (A.WH_NO = WHNO_MM1 OR A.WH_NO = WHNO_MM5) ";
                        break;
                    case "1":   //軍用
                        sql += @" AND A.WH_NO = WHNO_MM5 ";
                        break;
                    case "2":   //民用
                        sql += @" AND A.WH_NO = WHNO_MM1 ";
                        break;
                    default:
                        break;
                }
                p.Add(":MIL", string.Format("{0}", MIL));
            }

            if (!string.IsNullOrWhiteSpace(MMCODE))
            {
                sql += @" AND A.MMCODE = :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}", MMCODE));
            }

            sql += "ORDER BY B.MAT_CLASS, A.MMCODE, A.DATA_YM ";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0074M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<AA0074M> Print(string MAT_CLASS, string DATA_YM_B, string DATA_YM_E, string M_STOREID, string MIL, string MMCODE, bool clsALL)
        {
            var p = new DynamicParameters();

            string sql = @" SELECT
                                A.MMCODE,   --院內碼
                                -- (select MMNAME_C || ' ' || MMNAME_E from MI_MAST where MMCODE = A.MMCODE) AS MMNAME,    --品名
                                (select MMNAME_C from MI_MAST where MMCODE = A.MMCODE) AS MMNAME_C,     --中文品名
                                (select MMNAME_E from MI_MAST where MMCODE = A.MMCODE) AS MMNAME_E,     --英文品名
                                (case when A.WH_NO = '560000' then '民' when A.WH_NO = 'MM1X' then '軍' end) AS MIL,  --軍民別
                                (select (SELECT MAT_CLASS || ' ' || MAT_CLSNAME FROM MI_MATCLASS WHERE MAT_CLASS = MI_MAST.MAT_CLASS) from MI_MAST where MMCODE = A.MMCODE) AS MAT_CLASS,   --物料分類
                                (select BASE_UNIT from MI_MAST where MMCODE = A.MMCODE) AS BASE_UNIT,   --計量單位
                                A.DATA_YM,  --年月
                                PMN_INVQTY AS EX_INV_QTY,    --上期結存
                                IN_INVQTY AS INV_QTY_INCR, --本期增加
                                OUT_INVQTY AS INV_QTY_DECR, --本期減少
                                A.INV_QTY,  --本期結存
                                (case B.M_CONTID when '0' then 'Y' when '2' then 'N' else M_CONTID end) AS M_CONTID,   --是否合約
                                B.M_AGENNO, --廠編
                                (select (SELECT RTRIM(AGEN_NAMEC, '　') FROM PH_VENDER WHERE AGEN_NO = MI_MAST.M_AGENNO) from MI_MAST where MMCODE = A.MMCODE) AS AGEN_NAMEC, --廠商名稱
                                SUM_APL_OUTQTY(:DATA_YM_B,A.DATA_YM,A.WH_NO,A.MMCODE) AS APL_OUTQTY, --累計撥發量
                                a.apl_inqty --進貨量
                            FROM
                                V_INVQTY_YM A, MI_MAST B
                            WHERE
                                A.MMCODE = B.MMCODE ";


            if (!string.IsNullOrWhiteSpace(MAT_CLASS))
            {
                if (clsALL == true)
                {
                    sql += @" AND A.MMCODE IN (select MMCODE from MI_MAST where MAT_CLASS in (" + MAT_CLASS + ")) ";
                }
                else
                {
                    sql += @" AND A.MMCODE IN (select MMCODE from MI_MAST where MAT_CLASS = :MAT_CLASS ) ";
                    p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS));
                }
            }

            if (!string.IsNullOrWhiteSpace(DATA_YM_B))
            {
                sql += " AND TO_DATE(DATA_YM + 191100, 'yyyy/MM') >= TO_DATE(:DATA_YM_B + 191100, 'yyyy/MM') ";
                p.Add(":DATA_YM_B", string.Format("{0}", DATA_YM_B));
            }

            if (!string.IsNullOrWhiteSpace(DATA_YM_E))
            {
                sql += " AND TO_DATE(DATA_YM + 191100, 'yyyy/MM') <= TO_DATE(:DATA_YM_E + 191100, 'yyyy/MM') ";
                p.Add(":DATA_YM_E", string.Format("{0}", DATA_YM_E));
            }

            if (!string.IsNullOrWhiteSpace(M_STOREID))
            {
                switch (M_STOREID)
                {
                    case "0":   //radio選庫備
                        sql += @" AND A.MMCODE IN (select MMCODE from MI_MAST where M_STOREID = '1' and M_APPLYID not in ('P', 'E')) ";
                        break;
                    case "1":   //radio選非庫備
                        sql += @" AND A.MMCODE IN (select MMCODE from MI_MAST where M_STOREID = '0' )";
                        break;
                    case "2":   //radio選庫備品(管控項目)
                        sql += @" AND A.MMCODE IN (select MMCODE from MI_MAST where M_STOREID = '1' and M_APPLYID in ('P', 'E')) ";
                        break;
                    default:
                        break;
                }
                p.Add(":M_STOREID", string.Format("{0}", M_STOREID));
            }

            if (!string.IsNullOrWhiteSpace(MIL))
            {
                switch (MIL)
                {
                    case "0":   //全部
                        sql += @" AND (A.WH_NO = WHNO_MM1 OR A.WH_NO = WHNO_MM5) ";
                        break;
                    case "1":   //軍用
                        sql += @" AND A.WH_NO = WHNO_MM5 ";
                        break;
                    case "2":   //民用
                        sql += @" AND A.WH_NO = WHNO_MM1 ";
                        break;
                    default:
                        break;
                }
                p.Add(":MIL", string.Format("{0}", MIL));
            }

            if (!string.IsNullOrWhiteSpace(MMCODE))
            {
                sql += @" AND A.MMCODE = :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}", MMCODE));
            }

            sql += "ORDER BY B.MAT_CLASS, A.MMCODE, A.DATA_YM ";

            return DBWork.Connection.Query<AA0074M>(sql, p, DBWork.Transaction);
        }

        public DataTable GetExcel(string MAT_CLASS, string DATA_YM_B, string DATA_YM_E, string M_STOREID, string MIL, string MMCODE, bool clsALL)
        {
            var p = new DynamicParameters();

            string sql = @" SELECT
                                A.MMCODE AS 院內碼,
                                -- (select MMNAME_C || ' ' || MMNAME_E from MI_MAST where MMCODE = A.MMCODE) AS 品名,
                                (select MMNAME_C from MI_MAST where MMCODE = A.MMCODE) AS 中文品名,
                                (select MMNAME_E from MI_MAST where MMCODE = A.MMCODE) AS 英文品名,
                                (case when A.WH_NO = '560000' then '民' when A.WH_NO = 'MM1X' then '軍' end) AS 軍民別,
                                (select (SELECT MAT_CLASS || ' ' || MAT_CLSNAME FROM MI_MATCLASS WHERE MAT_CLASS = MI_MAST.MAT_CLASS) from MI_MAST where MMCODE = A.MMCODE) AS 物料分類,
                                (select BASE_UNIT from MI_MAST where MMCODE = A.MMCODE) AS 計量單位,
                                A.DATA_YM AS 年月,
                                PMN_INVQTY AS 上期結存,
                                a.apl_inqty as 進貨量,
                                IN_INVQTY AS 本期增加,
                                OUT_INVQTY AS 本期減少,
                                A.INV_QTY AS 本期結存,
                                SUM_APL_OUTQTY(:DATA_YM_B,A.DATA_YM,A.WH_NO,A.MMCODE) AS 累計撥發量,
                                (case B.M_CONTID when '0' then 'Y' when '2' then 'N' else M_CONTID end) AS 是否合約,
                                B.M_AGENNO AS 廠編,
                                (select (SELECT RTRIM(AGEN_NAMEC, '　') FROM PH_VENDER WHERE AGEN_NO = MI_MAST.M_AGENNO) from MI_MAST where MMCODE = A.MMCODE) AS 廠商名稱 
                            FROM
                                V_INVQTY_YM A, MI_MAST B
                            WHERE
                                A.MMCODE = B.MMCODE ";


            if (!string.IsNullOrWhiteSpace(MAT_CLASS))
            {
                if (clsALL == true)
                {
                    sql += @" AND A.MMCODE IN (select MMCODE from MI_MAST where MAT_CLASS in (" + MAT_CLASS + ")) ";
                }
                else
                {
                    sql += @" AND A.MMCODE IN (select MMCODE from MI_MAST where MAT_CLASS = :MAT_CLASS ) ";
                    p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS));
                }
            }

            if (!string.IsNullOrWhiteSpace(DATA_YM_B))
            {
                sql += " AND TO_DATE(DATA_YM + 191100, 'yyyy/MM') >= TO_DATE(:DATA_YM_B + 191100, 'yyyy/MM') ";
                p.Add(":DATA_YM_B", string.Format("{0}", DATA_YM_B));
            }

            if (!string.IsNullOrWhiteSpace(DATA_YM_E))
            {
                sql += " AND TO_DATE(DATA_YM + 191100, 'yyyy/MM') <= TO_DATE(:DATA_YM_E + 191100, 'yyyy/MM') ";
                p.Add(":DATA_YM_E", string.Format("{0}", DATA_YM_E));
            }

            if (!string.IsNullOrWhiteSpace(M_STOREID))
            {
                switch (M_STOREID)
                {
                    case "0":   //radio選庫備
                        sql += @" AND A.MMCODE IN (select MMCODE from MI_MAST where M_STOREID = '1' and M_APPLYID not in ('P', 'E')) ";
                        break;
                    case "1":   //radio選非庫備
                        sql += @" AND A.MMCODE IN (select MMCODE from MI_MAST where M_STOREID = '0' )";
                        break;
                    case "2":   //radio選庫備品(管控項目)
                        sql += @" AND A.MMCODE IN (select MMCODE from MI_MAST where M_STOREID = '1' and M_APPLYID in ('P', 'E')) ";
                        break;
                    default:
                        break;
                }
                p.Add(":M_STOREID", string.Format("{0}", M_STOREID));
            }

            if (!string.IsNullOrWhiteSpace(MIL))
            {
                switch (MIL)
                {
                    case "0":   //全部
                        sql += @" AND (A.WH_NO = WHNO_MM1 OR A.WH_NO = WHNO_MM5) ";
                        break;
                    case "1":   //軍用
                        sql += @" AND A.WH_NO = WHNO_MM5 ";
                        break;
                    case "2":   //民用
                        sql += @" AND A.WH_NO = WHNO_MM1 ";
                        break;
                    default:
                        break;
                }
                p.Add(":MIL", string.Format("{0}", MIL));
            }

            if (!string.IsNullOrWhiteSpace(MMCODE))
            {
                sql += @" AND A.MMCODE = :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}", MMCODE));
            }

            sql += "ORDER BY B.MAT_CLASS, A.MMCODE, A.DATA_YM ";

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
            

            return DBWork.Connection.Query<ComboItemModel>(sql, DBWork.Transaction);
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