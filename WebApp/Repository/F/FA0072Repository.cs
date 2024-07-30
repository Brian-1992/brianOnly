using System;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using System.Text;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;

namespace WebApp.Repository.F
{
    public class FA0072ReportMODEL : JCLib.Mvc.BaseModel
    {
        public float F1 { get; set; }
        public float F2 { get; set; }
        public float F3 { get; set; }
        public float F4 { get; set; }
        public float F5 { get; set; }
        public float F6 { get; set; }
        public float F7 { get; set; }

    }
    public class FA0072ReportMODEL2 : JCLib.Mvc.BaseModel
    {
        public string F1 { get; set; }
        public float F2 { get; set; }
    }
    public class FA0072Repository : JCLib.Mvc.BaseRepository
    {
        public FA0072Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<FA0072ReportMODEL> GetPrintData(string p0, string p1)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT   round(SUM( P_INV_QTY * PMN_AVGPRICE )) F1 ,
                                 round(SUM( IN_QTY * IN_PRICE )) F2 ,
                                 round(SUM( EXG_OUTQTY * IN_PRICE )) F3 ,
                                 round(SUM( REJ_OUTQTY * IN_PRICE )) F4 ,
                                 round(SUM( OUT_QTY * IN_PRICE )) F5 ,
                                 round(SUM( EXG_INQTY * IN_PRICE )) F6 ,
                                 round(SUM( EXG_QTY * IN_PRICE )) F7 
                         FROM  V_COST_FA0072  WHERE 1 = 1  ";

            if (p0 != "")
            {
                sql += " AND DATA_YM = :p0 ";
                p.Add(":p0", string.Format("{0}", p0));
            }
            if (p1 != "")
            {
                sql += " AND WH_NO = :p1 ";
                p.Add(":p1", string.Format("{0}", p1));
            }

            return DBWork.Connection.Query<FA0072ReportMODEL>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<FA0072ReportMODEL2> GetPrintData2(string p0)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT SECTIONNAME AS F1, 
                               CASE WHEN GET_PARAM_VALUE('HOSP_INFO','HospCode')='0' THEN AVG_COST ELSE DISC_COST END F2 
                        FROM V_COST_SECTION_FA0072 
                       WHERE 1 = 1 ";

            if (p0 != "")
            {
                sql += " AND DATA_YM = :p0 ";
                p.Add(":p0", string.Format("{0}", p0));
            }
            sql += " ORDER BY SECTIONNAME ";

            return DBWork.Connection.Query<FA0072ReportMODEL2>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetWhmastCombo()
        {
            string sql = @"SELECT DISTINCT WH_NO as VALUE, WH_NAME as TEXT ,
                        WH_NO || ' ' || WH_NAME as COMBITEM  
                        FROM V_WHNO_FA0072   
                        ORDER BY WH_NO ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

    }
}
