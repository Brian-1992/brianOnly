using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using JCLib.DB;
using JCLib.Mvc;
using Dapper;
using WebApp.Models;

namespace WebApp.Repository.F
{
    public class FA0028Repository : JCLib.Mvc.BaseRepository
    {
        public FA0028Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        public IEnumerable<MI_WLOCINV> GetAll(string userId, string wh_no, string mat_class, string mmcode)
        {
            var p = new DynamicParameters();

            //var sql = @"select A.MMCODE, B.MAT_CLASS, 
            //            trim(B.MMNAME_C) AS MMNAME_C,
            //            trim(B.MMNAME_E) AS MMNAME_E,
            //            trim(B.M_AGENLAB) AS M_AGENLAB,
            //            trim(B.BASE_UNIT) AS BASE_UNIT,
            //            case when B.M_STOREID=0 then B.M_STOREID || ' ' || '非庫備' else B.M_STOREID || ' ' || '庫備' end AS M_STOREID,
            //            to_char(A.INV_QTY, 'FM999,999,999,999,999') AS INV_QTY,
            //            to_char(B.M_CONTPRICE, 'FM999,999,999,999,999') AS M_CONTPRICE,
            //            to_char(C.MIL_PRICE, 'FM999,999,999,999,999') AS MIL_PRICE,
            //            to_char(C.MIL_PRICE * A.INV_QTY, 'FM999,999,999,999,999') AS TOTAL_PRICE
            //            from MI_WHINV A inner join MI_MAST B on A.MMCODE=B.MMCODE
            //            LEFT join MI_WHCOST C ON A.MMCODE=C.MMCODE";
            var sql = @" select mmcode,(select mmname_c from MI_MAST where mmcode=a.mmcode) as mmname_c,(select mmname_e from MI_MAST where mmcode=a.mmcode) as mmname_e,wh_no,store_loc
                         from MI_WLOCINV a
                         where 1=1";

            if (wh_no != "")
            {
                sql += " and wh_no=:WH_NO";
                p.Add(":WH_NO", wh_no);
            }

            if (mat_class != "")
            {
                sql += " and (select mat_class from MI_MAST where mmcode=a.mmcode)=:MAT_CLASS";
                p.Add(":MAT_CLASS", mat_class);
            }

            if (mmcode != "")
            {
                sql += " and a.mmcode=:MMCODE";
                p.Add(":MMCODE", mmcode);
            }

            sql += " order by mmcode,wh_no";

            return DBWork.PagingQuery<MI_WLOCINV>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetMatClassCombo()
        {
            string sql = @"SELECT MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM
                        FROM MI_MATCLASS
                        order by MAT_CLASS";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<MI_MAST> GetMMCODECombo(string mmcode, int page_index, int page_size, string sorters)
        {
            DynamicParameters p = new DynamicParameters();

            string sql = @"SELECT DISTINCT {0} MMCODE , MMNAME_C, MMNAME_E from MI_MAST A WHERE 1=1 ";

            if (mmcode != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,");
                p.Add(":MMCODE_I", mmcode);
                p.Add(":MMNAME_E_I", mmcode);
                p.Add(":MMNAME_C_I", mmcode);

                sql += " AND (A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}%", mmcode));

                sql += " OR MMNAME_E LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", mmcode));

                sql += " OR MMNAME_C LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", mmcode));

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

        public IEnumerable<COMBO_MODEL> GetWhnoCombo()
        {
            string sql = @"SELECT WH_NO AS VALUE, WH_NAME AS TEXT, WH_NO || ' ' || WH_NAME AS COMBITEM
                        FROM MI_WHMAST
                        order by WH_NO";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public DataTable GetExcel(string wh_no, string mat_class, string mmcode)
        {
            var p = new DynamicParameters();

            var sql = @"select mmcode 院內碼, 
                        (select mmname_c from MI_MAST where mmcode=a.mmcode) 中文品名, 
                        (select mmname_e from MI_MAST where mmcode=a.mmcode) 英文品名, 
                        wh_no 庫房代碼,
                        store_loc 儲位碼
                        from MI_WLOCINV a where 1=1 ";

            if (!string.IsNullOrEmpty(wh_no))
            {
                sql += " and wh_no=:WH_NO";
                p.Add(":WH_NO", wh_no);
            }

            if (!string.IsNullOrEmpty(mat_class))
            {
                sql += " and (select mat_class from MI_MAST where mmcode=a.mmcode)=:MAT_CLASS";
                p.Add(":MAT_CLASS", mat_class);
            }

            if (!string.IsNullOrEmpty(mmcode))
            {
                sql += " and a.mmcode=:MMCODE";
                p.Add(":MMCODE", mmcode);
            }

            sql += @" ORDER BY mmcode";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<MI_WLOCINV> GetPrintData(string wh_no, string mat_class, string mmcode)
        {
            var p = new DynamicParameters();

            var sql = @" select mmcode,(select mmname_c from MI_MAST where mmcode=a.mmcode) as mmname_c,(select mmname_e from MI_MAST where mmcode=a.mmcode) as mmname_e,wh_no,store_loc
                         from MI_WLOCINV a
                         where 1=1";

            if (wh_no != "")
            {
                sql += " and wh_no=:WH_NO";
                p.Add(":WH_NO", wh_no);
            }

            if (mat_class != "")
            {
                sql += " and (select mat_class from MI_MAST where mmcode=a.mmcode)=:MAT_CLASS";
                p.Add(":MAT_CLASS", mat_class);
            }

            if (mmcode != "")
            {
                sql += " and a.mmcode=:MMCODE";
                p.Add(":MMCODE", mmcode);
            }

            sql += " order by mmcode,wh_no";

            return DBWork.Connection.Query<MI_WLOCINV>(sql, p, DBWork.Transaction);
        }
    }
}