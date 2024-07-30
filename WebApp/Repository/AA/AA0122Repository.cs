using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using JCLib.DB;
using JCLib.Mvc;
using Dapper;
using WebApp.Models;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace WebApp.Repository.AA
{
    public class AA0122Repository : JCLib.Mvc.BaseRepository
    {
        public AA0122Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        protected string GetYm(bool isLast)
        {
            string rtn = "", year = "";
            year = Convert.ToString(Convert.ToInt32(DateTime.Now.Year) - 1911);
            int i = 0;
            if (isLast)
            {
                i = DateTime.Now.AddMonths(-1).Month;
                if (i == 12)
                    year = Convert.ToString(Convert.ToInt32(DateTime.Now.AddYears(-1).Year) - 1911);
            }
            else
                i = DateTime.Now.Month;

            if (i < 10)
                rtn = year + "0" + i.ToString();
            else
                rtn = year + i.ToString();

            return rtn;
        }
        public IEnumerable<MI_MAST> GetAll(MI_MAST_QUERY_PARAMS query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select A.MMCODE, B.MAT_CLASS, 
                        trim(B.MMNAME_C) AS MMNAME_C,
                        trim(B.MMNAME_E) AS MMNAME_E,
                        trim(B.M_AGENLAB) AS M_AGENLAB,
                        trim(B.BASE_UNIT) AS BASE_UNIT,
                        case when B.M_STOREID=0 then B.M_STOREID || ' ' || '非庫備' else B.M_STOREID || ' ' || '庫備' end AS M_STOREID,
                        A.INV_QTY AS INV_QTY,
                        c.mil_price AS MIL_PRICE, --目前戰備單價
                        c.uprice as uprice, -- 民最小單價、調整後戰備單價
                        (C.MIL_PRICE * A.INV_QTY) AS TOTAL_PRICE
                        from MI_WHINV A inner join MI_MAST B on A.MMCODE=B.MMCODE
                        LEFT join MI_WHCOST C ON A.MMCODE=C.MMCODE";
            if (query.DATA_YM != "")
            {
                sql += " AND C.DATA_YM = :DATA_YM ";
                p.Add(":DATA_YM", string.Format("{0}", query.DATA_YM));
            }
            else
                sql += " AND (C.DATA_YM='" + GetYm(true) + "' OR C.DATA_YM='" + GetYm(false) + "')";

            sql += @" where WH_NO in (select WH_NO from MI_WHMAST where WH_KIND='1' and WH_GRADE='5')
                       -- and A.INV_QTY > 0 
";

            if (query.MAT_CLASS != "")
            {
                if (query.MAT_CLASS.Contains(","))
                {
                    string[] tmp = query.MAT_CLASS.Split(',');
                    sql += " AND B.MAT_CLASS IN :MAT_CLASS";
                    p.Add(":MAT_CLASS", tmp);
                }
                else
                {
                    sql += " AND B.MAT_CLASS = :MAT_CLASS ";
                    p.Add(":MAT_CLASS", string.Format("{0}", query.MAT_CLASS));
                }
            }
            if (query.NotEqualOnly) {
                sql += " and c.mil_price <> c.uprice";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetPrintData(MI_MAST_QUERY_PARAMS query)
        {
            var p = new DynamicParameters();

            var sql = @"select ROW_NUMBER() OVER(ORDER BY A.MMCODE) AS ROW_NUM,
                        A.MMCODE, B.MAT_CLASS,
                        CASE WHEN trim(B.MMNAME_C) IS NULL THEN trim(B.MMNAME_E) ELSE trim(B.MMNAME_C) END AS MMNAME_C,
                        trim(B.MMNAME_E) AS MMNAME_E,
                        trim(B.M_AGENLAB) AS M_AGENLAB,
                        trim(B.BASE_UNIT) AS BASE_UNIT,
                        case when B.M_STOREID=0 then '非庫備' else '庫備' end AS M_STOREID,
                        to_char(A.INV_QTY, 'FM999,999,999,999,999') AS INV_QTY,
                        to_char(B.M_CONTPRICE, 'FM999,999,999,999,999') AS M_CONTPRICE,
                        to_char(C.MIL_PRICE, 'FM999,999,999,999,999.00') AS MIL_PRICE,
                        to_char(C.MIL_PRICE * A.INV_QTY, 'FM999,999,999,999,999.00') AS TOTAL_PRICE
                        from MI_WHINV A inner join MI_MAST B on A.MMCODE=B.MMCODE
                        LEFT join MI_WHCOST C ON A.MMCODE=C.MMCODE";

            if (query.DATA_YM != "")
            {
                sql += " AND C.DATA_YM = :DATA_YM ";
                p.Add(":DATA_YM", string.Format("{0}", query.DATA_YM));
            }
            else
                sql += " AND (C.DATA_YM='" + GetYm(true) + "' OR C.DATA_YM='" + GetYm(false) + "')";

            sql += @" where WH_NO in (select WH_NO from MI_WHMAST where WH_KIND='1' and WH_GRADE='5') 
                        -- and A.INV_QTY > 0 
                    ";

            if (query.MAT_CLASS != "")
            {
                if (query.MAT_CLASS.Contains(","))
                {
                    string[] tmp = query.MAT_CLASS.Split(',');
                    sql += " AND B.MAT_CLASS IN :MAT_CLASS";
                    p.Add(":MAT_CLASS", tmp);
                }
                else
                {
                    sql += " AND B.MAT_CLASS = :MAT_CLASS ";
                    p.Add(":MAT_CLASS", string.Format("{0}", query.MAT_CLASS));
                }
            }
            if (query.NotEqualOnly)
            {
                sql += " and c.mil_price <> c.uprice";
            }

            sql += " order by MMCODE";

            //if (query.DATA_YM != "")
            //{
            //    sql += " AND C.DATA_YM = :DATA_YM ";
            //    p.Add(":DATA_YM", string.Format("{0}", query.DATA_YM));
            //}

            //if (query.MAT_CLASS != "")
            //{
            //    sql += " AND B.MAT_CLASS = :MAT_CLASS ";
            //    p.Add(":MAT_CLASS", string.Format("{0}", query.MAT_CLASS));
            //}

            //p.Add("OFFSET", (page_index - 1) * page_size);
            //p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(sql, p, DBWork.Transaction);
        }

        public DataTable GetExcel(MI_MAST_QUERY_PARAMS query)
        {
            var p = new DynamicParameters();

            var sql = "select ROW_NUMBER() OVER(ORDER BY A.MMCODE) AS 項次,"+
                        "A.MMCODE as 院內碼, " +
                        "CASE WHEN trim(B.MMNAME_C) IS NULL THEN trim(B.MMNAME_E) ELSE trim(B.MMNAME_C) END AS 中文品名," +
                        "trim(B.MMNAME_E) AS 英文品名," +
                        "trim(B.M_AGENLAB) AS 廠牌," +
                        "trim(B.BASE_UNIT) AS 計量單位," +
                        "case when B.M_STOREID=0 then '非庫備' else '庫備' end AS 是否庫備," +
                        "c.uprice AS 民合約單價_" + query.DATA_YM + "," +
                        "c.mil_price AS 目前戰備單價," +
                        "to_char(A.INV_QTY, 'FM999,999,999,999,999') AS 戰備庫存量 " +
                        "from MI_WHINV A inner join MI_MAST B on A.MMCODE=B.MMCODE " +
                        "LEFT join MI_WHCOST C ON A.MMCODE=C.MMCODE";

            if (query.DATA_YM != "")
            {
                sql += " AND C.DATA_YM = :DATA_YM ";
                p.Add(":DATA_YM", string.Format("{0}", query.DATA_YM));
            }
            else
                sql += " AND (C.DATA_YM='" + GetYm(true) + "' OR C.DATA_YM='" + GetYm(false) + "')";

            sql += @" where WH_NO in (select WH_NO from MI_WHMAST where WH_KIND='1' and WH_GRADE='5')
                        -- and A.INV_QTY > 0 
                    ";

            if (query.MAT_CLASS != "")
            {
                if (query.MAT_CLASS.Contains(","))
                {
                    string[] tmp = query.MAT_CLASS.Split(',');
                    sql += " AND B.MAT_CLASS IN :MAT_CLASS";
                    p.Add(":MAT_CLASS", tmp);
                }
                else
                {
                    sql += " AND B.MAT_CLASS = :MAT_CLASS ";
                    p.Add(":MAT_CLASS", string.Format("{0}", query.MAT_CLASS));
                }
            }
            if (query.NotEqualOnly)
            {
                sql += " and c.mil_price <> c.uprice";
            }

            sql += " order by a.MMCODE";


            DataTable dt = new DataTable();
            using (
                var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public string GetSum()
        {
            var sql = @"select to_char(sum(C.MIL_PRICE * A.INV_QTY), 'FM999,999,999,999,999.00') AS SUM
                        from MI_WHINV A inner join MI_MAST B on A.MMCODE=B.MMCODE
                        LEFT join MI_WHCOST C ON A.MMCODE=C.MMCODE and c.data_ym='10805'
                        where WH_NO in (select WH_NO from MI_WHMAST where WH_KIND='1' and WH_GRADE='5')
                        and A.INV_QTY > 0 ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }

        public IEnumerable<UR_INID> GetInid(string inid)
        {
            var sql = @"SEELCT :INID || ' ' || ININ_NAME(:INID) FROM DUAL";
            return DBWork.Connection.Query<UR_INID>(sql, new { INID = inid } , DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetMatClassCombo(string userId)
        {
            string sql = @"select MAT_CLASS AS VALUE, MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM from MI_MATCLASS where MAT_CLSID  IN ('2') ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { USERID = userId });
        }

        public SP_MODEL UpdMcost(string ym, string matclass, string updusr, string updip)
        {
            var p = new OracleDynamicParameters();
            p.Add("I_YM", value: ym, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 5);
            p.Add("I_MATCLASS", value: matclass, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 2);
            //p.Add("I_UPDUSR", value: updusr, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 8);
            //p.Add("I_UPDIP", value: updip, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 20);

            p.Add("O_RETID", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 1);
            p.Add("O_ERRMSG", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 200);

            DBWork.Connection.Query("INV_SET.UPD_MCOST", p, commandType: CommandType.StoredProcedure);
            string retid = p.Get<OracleString>("O_RETID").Value;
            string errmsg = p.Get<OracleString>("O_ERRMSG").Value;

            SP_MODEL sp = new SP_MODEL
            {
                O_RETID = p.Get<OracleString>("O_RETID").Value,
                O_ERRMSG = p.Get<OracleString>("O_ERRMSG").Value
            };
            return sp;
        }

        public class MI_MAST_QUERY_PARAMS
        {
            public string MMCODE;
            public string MMNAME_C;
            public string MMNAME_E;
            public string MAT_CLASS;

            public string WH_NO;
            public string DATA_YM;

            public bool NotEqualOnly;
        }

        public string GetSetYm() {
            string sql = @"
                select set_ym from MI_MNSET where set_status = 'N'
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }
    }
}