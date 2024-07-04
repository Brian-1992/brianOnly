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
    public class AB0057Repository : JCLib.Mvc.BaseRepository
    {
        public AB0057Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AB0057M> GetAll(string MAT_CLASS, string EXP_DATE_B, string EXP_DATE_E, string M_STOREID, string FRWH, bool clsALL, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = @" SELECT
                                (select (SELECT MAT_CLASS || ' ' || MAT_CLSNAME FROM MI_MATCLASS WHERE MAT_CLASS = MI_MAST.MAT_CLASS) from MI_MAST where MMCODE = A.MMCODE) AS MAT_CLASS,
                                A.MMCODE,
                                (select MMNAME_C from MI_MAST where MMCODE = A.MMCODE) AS MMNAME_C,
                                (select MMNAME_E from MI_MAST where MMCODE = A.MMCODE) AS MMNAME_E,
                                A.LOT_NO,
                                TWN_DATE(A.EXP_DATE) AS EXP_DATE,
                                A.INV_QTY,
                                (select BASE_UNIT from MI_MAST where MMCODE = A.MMCODE) AS BASE_UNIT,
                                (select (SELECT AGEN_NO || ' ' || RTRIM(AGEN_NAMEC, '　') FROM PH_VENDER WHERE AGEN_NO = MI_MAST.M_AGENNO) from MI_MAST where MMCODE = A.MMCODE) AS M_AGENNO,
                                (select M_CONTPRICE from MI_MAST where MMCODE = A.MMCODE) AS M_CONTPRICE
                            FROM
                                MI_WEXPINV A
                            WHERE 1=1 AND A.WH_NO = :FRWH ";

            p.Add(":FRWH", string.Format("{0}", FRWH));

            if (!string.IsNullOrWhiteSpace(MAT_CLASS))
            {
                if (clsALL == true)
                {
                    sql += @" AND A.MMCODE IN (select MMCODE from MI_MAST where MAT_CLASS in (" + MAT_CLASS + ")) ";
                }
                else
                {
                    sql += @" AND A.MMCODE IN (select MMCODE from MI_MAST where MAT_CLASS = :MAT_CLASS )";
                    p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS));
                }
            }

            if (!string.IsNullOrWhiteSpace(EXP_DATE_B))
            {
                sql += " AND TO_CHAR(EXP_DATE, 'yyyyMM') >= TO_CHAR(:EXP_DATE_B + 191100) ";
                p.Add(":EXP_DATE_B", string.Format("{0}", EXP_DATE_B));
            }

            if (!string.IsNullOrWhiteSpace(EXP_DATE_E))
            {
                sql += " AND TO_CHAR(EXP_DATE, 'yyyyMM') <= TO_CHAR(:EXP_DATE_E + 191100) ";
                p.Add(":EXP_DATE_E", string.Format("{0}", EXP_DATE_E));
            }

            if (!string.IsNullOrWhiteSpace(M_STOREID))
            {
                sql += @" AND A.MMCODE IN (select MMCODE from MI_MAST where M_STOREID = :M_STOREID )";
                p.Add(":M_STOREID", string.Format("{0}", M_STOREID));
            }

            sql += "ORDER BY MAT_CLASS, A.MMCODE, A.EXP_DATE DESC";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AB0057M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<AB0057M> Print(string MAT_CLASS, string EXP_DATE_B, string EXP_DATE_E, string M_STOREID, string FRWH, bool clsALL)
        {
            var p = new DynamicParameters();

            string sql = @" SELECT
                                (select (SELECT MAT_CLASS || ' ' || MAT_CLSNAME FROM MI_MATCLASS WHERE MAT_CLASS = MI_MAST.MAT_CLASS) from MI_MAST where MMCODE = A.MMCODE) AS MAT_CLASS,
                                A.MMCODE,
                                (select MMNAME_C from MI_MAST where MMCODE = A.MMCODE) AS MMNAME_C,
                                (select MMNAME_E from MI_MAST where MMCODE = A.MMCODE) AS MMNAME_E,
                                A.LOT_NO,
                                TWN_DATE(A.EXP_DATE) AS EXP_DATE,
                                A.INV_QTY,
                                (select BASE_UNIT from MI_MAST where MMCODE = A.MMCODE) AS BASE_UNIT,
                                (select (SELECT AGEN_NO || ' ' || RTRIM(AGEN_NAMEC, '　') FROM PH_VENDER WHERE AGEN_NO = MI_MAST.M_AGENNO) from MI_MAST where MMCODE = A.MMCODE) AS M_AGENNO,
                                (select M_CONTPRICE from MI_MAST where MMCODE = A.MMCODE) AS M_CONTPRICE,
                                (SELECT MWT.WH_KIND FROM MI_WHMAST MWT WHERE MWT.WH_NO=A.WH_NO ) AS TITLE 
                            FROM
                                MI_WEXPINV A
                            WHERE 1=1 AND A.WH_NO = :FRWH ";

            p.Add(":FRWH", string.Format("{0}", FRWH));

            if (!string.IsNullOrWhiteSpace(MAT_CLASS))
            {
                if (clsALL == true)
                {
                    sql += @" AND A.MMCODE IN (select MMCODE from MI_MAST where MAT_CLASS in (" + MAT_CLASS + ")) ";
                }
                else
                {
                    sql += @" AND A.MMCODE IN (select MMCODE from MI_MAST where MAT_CLASS = :MAT_CLASS )";
                    p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS));
                }
            }

            if (!string.IsNullOrWhiteSpace(EXP_DATE_B))
            {
                sql += " AND TO_CHAR(EXP_DATE, 'yyyyMM') >= TO_CHAR(:EXP_DATE_B + 191100) ";
                p.Add(":EXP_DATE_B", string.Format("{0}", EXP_DATE_B));
            }

            if (!string.IsNullOrWhiteSpace(EXP_DATE_E))
            {
                sql += " AND TO_CHAR(EXP_DATE, 'yyyyMM') <= TO_CHAR(:EXP_DATE_E + 191100) ";
                p.Add(":EXP_DATE_E", string.Format("{0}", EXP_DATE_E));
            }

            if (!string.IsNullOrWhiteSpace(M_STOREID))
            {
                sql += @" AND A.MMCODE IN (select MMCODE from MI_MAST where M_STOREID = :M_STOREID )";
                p.Add(":M_STOREID", string.Format("{0}", M_STOREID));
            }

            sql += "ORDER BY MAT_CLASS, A.MMCODE, A.EXP_DATE DESC";

            return DBWork.Connection.Query<AB0057M>(sql, p, DBWork.Transaction);
        }

        public DataTable GetExcel(string MAT_CLASS, string EXP_DATE_B, string EXP_DATE_E, string M_STOREID, string FRWH, bool clsALL)
        {
            var p = new DynamicParameters();

            string sql = @" SELECT
                                (select (SELECT MAT_CLASS || ' ' || MAT_CLSNAME FROM MI_MATCLASS WHERE MAT_CLASS = MI_MAST.MAT_CLASS) from MI_MAST where MMCODE = A.MMCODE) AS 物料分類,
                                A.MMCODE AS 院內碼,
                                (select MMNAME_C from MI_MAST where MMCODE = A.MMCODE) AS 中文品名,
                                (select MMNAME_E from MI_MAST where MMCODE = A.MMCODE) AS 英文品名,
                                A.LOT_NO AS 批號,
                                TWN_DATE(A.EXP_DATE) AS 效期,
                                A.INV_QTY AS 數量,
                                (select BASE_UNIT from MI_MAST where MMCODE = A.MMCODE) AS 單位,
                                (select (SELECT AGEN_NO || ' ' || RTRIM(AGEN_NAMEC, '　') FROM PH_VENDER WHERE AGEN_NO = MI_MAST.M_AGENNO) from MI_MAST where MMCODE = A.MMCODE) AS 供貨商,
                                (select M_CONTPRICE from MI_MAST where MMCODE = A.MMCODE) AS 合約單價
                            FROM
                                MI_WEXPINV A
                            WHERE 1=1 AND A.WH_NO = :FRWH ";

            p.Add(":FRWH", string.Format("{0}", FRWH));

            if (!string.IsNullOrWhiteSpace(MAT_CLASS))
            {
                if (clsALL == true)
                {
                    sql += @" AND A.MMCODE IN (select MMCODE from MI_MAST where MAT_CLASS in (" + MAT_CLASS + ")) ";
                }
                else
                {
                    sql += @" AND A.MMCODE IN (select MMCODE from MI_MAST where MAT_CLASS = :MAT_CLASS )";
                    p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS));
                }
            }

            if (!string.IsNullOrWhiteSpace(EXP_DATE_B))
            {
                sql += " AND TO_CHAR(EXP_DATE, 'yyyyMM') >= TO_CHAR(:EXP_DATE_B + 191100) ";
                p.Add(":EXP_DATE_B", string.Format("{0}", EXP_DATE_B));
            }

            if (!string.IsNullOrWhiteSpace(EXP_DATE_E))
            {
                sql += " AND TO_CHAR(EXP_DATE, 'yyyyMM') <= TO_CHAR(:EXP_DATE_E + 191100) ";
                p.Add(":EXP_DATE_E", string.Format("{0}", EXP_DATE_E));
            }

            if (!string.IsNullOrWhiteSpace(M_STOREID))
            {
                sql += @" AND A.MMCODE IN (select MMCODE from MI_MAST where M_STOREID = :M_STOREID )";
                p.Add(":M_STOREID", string.Format("{0}", M_STOREID));
            }

            sql += "ORDER BY ((select (SELECT MAT_CLASS || ' ' || MAT_CLSNAME FROM MI_MATCLASS WHERE MAT_CLASS = MI_MAST.MAT_CLASS) from MI_MAST where MMCODE = A.MMCODE)), A.MMCODE, A.EXP_DATE DESC";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<ComboItemModel> GetMATCombo_0()
        {
            string sql = @"SELECT MAT_CLASS VALUE,
                                  MAT_CLASS || ' ' || MAT_CLSNAME TEXT
                           FROM MI_MATCLASS
                           WHERE MAT_CLSID = '1'";

            return DBWork.Connection.Query<ComboItemModel>(sql, DBWork.Transaction);
        }

        public IEnumerable<ComboItemModel> GetMATCombo()
        {
            string sql = @"SELECT MAT_CLASS VALUE,
                                  MAT_CLASS || ' ' || MAT_CLSNAME TEXT
                           FROM MI_MATCLASS
                           WHERE MAT_CLSID IN ('2', '3')";

            return DBWork.Connection.Query<ComboItemModel>(sql, DBWork.Transaction);
        }

        public IEnumerable<ComboItemModel> GetWh_no(string USER_ID)
        {
            var p = new DynamicParameters();

            string sql = @"  SELECT MWT.WH_NO VALUE,
                                    MWT.WH_NO || ' ' || MWT.WH_NAME TEXT,MWT.WH_KIND EXTRA1 
                             FROM MI_WHMAST MWT,
                                  (SELECT DISTINCT WH_NO
                                   FROM MI_WHID
                                   WHERE WH_USERID = :USER_ID
                                   UNION
                                   SELECT DISTINCT WH_NO
                                   FROM MI_WHMAST
                                   WHERE INID = USER_INID ( :USER_ID) AND WH_KIND = '1') WH
                             WHERE MWT.WH_NO = WH.WH_NO
                             ORDER BY MWT.WH_NO";

            return DBWork.Connection.Query<ComboItemModel>(sql, new { USER_ID = USER_ID }, DBWork.Transaction);
        }

        public string GetWHNO_KIND(string WH_NO)
        {
            var sql = @" SELECT MWT.WH_KIND
                         FROM MI_WHMAST MWT
                         WHERE MWT.WH_NO=:WH_NO";

            return DBWork.Connection.QueryFirst<string>(sql, new { WH_NO = WH_NO }, DBWork.Transaction);
        }

    }
}