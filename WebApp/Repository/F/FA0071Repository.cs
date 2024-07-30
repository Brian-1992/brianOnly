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
    public class FA0071ReportMODEL : JCLib.Mvc.BaseModel
    {
        public string F1 { get; set; }
        public string F2 { get; set; }
        public string F3 { get; set; }
        public string F4 { get; set; }
        public float F5 { get; set; }
        public float F6 { get; set; }
        public float F7 { get; set; }
        public float F8 { get; set; }
        public float F9 { get; set; }
        public float F10 { get; set; }
        public float F11 { get; set; }
        public float F12 { get; set; }
        public float F13 { get; set; }
        public float F14 { get; set; }
        public float F15 { get; set; }
        public float F16 { get; set; }
        public float F17 { get; set; }
        public float F18 { get; set; }
        public float F19 { get; set; }
        public float F20 { get; set; }
        public float F21 { get; set; }
        public float F22 { get; set; }
        public float F23 { get; set; }
        public float F24 { get; set; }
        public float F25 { get; set; }
        public float F26 { get; set; }
        public float F27 { get; set; }
        public float F28 { get; set; }
        public float F29 { get; set; }
        public float F30 { get; set; }

    }
    public class FA0071ReportMODEL2 : JCLib.Mvc.BaseModel
    {
        public string F1 { get; set; }
        public string F2 { get; set; }
        public string F3 { get; set; }
    }
    public class FA0071Repository : JCLib.Mvc.BaseRepository
    {
        public FA0071Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<FA0071ReportMODEL> GetPrintData(string p0)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT P.*,
                         ROUND(( SELECT SUM(C.DONATE_QTY) FROM V_DONATE_QTY C WHERE C.DATA_YM = :p0 AND C.WH_NO = P.F2 ) * (SELECT IN_PRICE FROM V_COST_FA0071E D WHERE D.DATA_YM = :p0 AND D.WH_NO = P.F2 )) AS F21,
                         ( SELECT SUM(C.DONATE_QTY) FROM V_DONATE_QTY C WHERE C.DATA_YM = :p0 AND C.WH_NO = P.F2 ) * (SELECT IN_PRICE FROM V_COST_FA0071E D WHERE D.DATA_YM = :p0 AND D.WH_NO = P.F2 ) - 
                         ROUND(( SELECT SUM(C.DONATE_QTY) FROM V_DONATE_QTY C WHERE C.DATA_YM = :p0 AND C.WH_NO = P.F2 ) * (SELECT IN_PRICE FROM V_COST_FA0071E D WHERE D.DATA_YM = :p0 AND D.WH_NO = P.F2 )) AS F22
                         FROM (
                          SELECT CASE WHEN A.EI='E' THEN '藥品類' ELSE '衛材類' END AS F1,A.WH_NO F2,A.WH_NAME F3,A.SEQ F4,
                                 SUM( ROUND( B.P_INV_QTY * B.PMN_AVGPRICE )) F5,
                                 SUM( ROUND( -P_EXG_QTY * PMN_DISC_CPRICE )) F6 ,
                                 SUM( ROUND( P_INV_QTY * DOWN_PRICE )) F7 ,
                                 SUM( ROUND( P_INV_QTY * RAISE_PRICE )) F8 ,
                                 SUM( ROUND( MN_INQTY * IN_PRICE )) F9 ,
                                 SUM( ROUND( ISU_QTY * IN_PRICE )) F10 ,
                                 SUM( ROUND( IN_QTY * IN_PRICE )) F11 ,
                                 SUM( ROUND( BAK_OUTQTY * IN_PRICE )) F12 ,
                                 SUM( ROUND( BAK_INQTY * IN_PRICE )) F13 ,
                                 SUM( ROUND( CS_QTY * IN_PRICE )) F14 ,
                                 SUM( ROUND( SURPLUS_QTY * IN_PRICE )) F15 ,
                                 SUM( ROUND( LOSS_QTY * IN_PRICE )) F16 ,
                                 SUM(( P_INV_QTY * PMN_AVGPRICE ) - ROUND( P_INV_QTY *  PMN_AVGPRICE )) F17 ,
                                 SUM(( -P_EXG_QTY * PMN_DISC_CPRICE) - ROUND(-P_EXG_QTY * PMN_DISC_CPRICE) ) F18 ,
                                 SUM(( P_INV_QTY * DOWN_PRICE)-ROUND( P_INV_QTY * DOWN_PRICE) ) F19 ,
                                 SUM(( P_INV_QTY * RAISE_PRICE)-ROUND(P_INV_QTY * RAISE_PRICE) ) F20,
                                 SUM(( MN_INQTY * IN_PRICE ) - ROUND( MN_INQTY * IN_PRICE )) F23,
                                 SUM(( ISU_QTY * IN_PRICE ) - ROUND( ISU_QTY * IN_PRICE )) F24,
                                 SUM(( IN_QTY * IN_PRICE ) - ROUND( IN_QTY * IN_PRICE )) F25,
                                 SUM(( BAK_OUTQTY * IN_PRICE ) - ROUND( BAK_OUTQTY * IN_PRICE )) F26,
                                 SUM(( BAK_INQTY * IN_PRICE ) - ROUND( BAK_INQTY * IN_PRICE )) F27,
                                 SUM(( CS_QTY * IN_PRICE ) - ROUND( CS_QTY * IN_PRICE )) F28,
                                 SUM(( SURPLUS_QTY * IN_PRICE ) - ROUND( SURPLUS_QTY * IN_PRICE )) F29,
                                 SUM(( LOSS_QTY * IN_PRICE ) - ROUND( LOSS_QTY * IN_PRICE )) F30
                            FROM V_WHNO_FA0071 A  
                            LEFT JOIN V_COST_FA0071E B ON B.WH_NO = A.WH_NO AND B.DATA_YM = :p0  
                            WHERE 1=1  
                            GROUP BY A.EI,A.SEQ,A.WH_NO,A.WH_NAME  
                        ) P
                        ORDER BY P.F1,P.F4 ";
            p.Add(":p0", string.Format("{0}", p0));
            return DBWork.Connection.Query<FA0071ReportMODEL>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<FA0071ReportMODEL2> GetPrintData2(string p0)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT nvl(A.AGEN_NO,' ') F1,
                               nvl(A.EASYNAME,' ') F2,
                               NVL(ROUND(SUM(-A.EXG_QTY * B.DISC_CPRICE ),0),0) F3
                        FROM V_EXG A,MI_WHCOST B
                        WHERE A.DATA_YM = TWN_PYM(:p0)
                          AND A.DATA_YM = B.DATA_YM
                        GROUP BY A.AGEN_NO,A.EASYNAME
                         ";
            p.Add(":p0", string.Format("{0}", p0));

            return DBWork.Connection.Query<FA0071ReportMODEL2>(sql, p, DBWork.Transaction);
        }

        public string GetHospName()
        {
            string sql = @" SELECT GET_PARAM_VALUE('HOSP_INFO','HospName')HOSPNAME FROM DUAL ";
            return DBWork.Connection.ExecuteScalar<string>(sql, DBWork.Transaction);
        }
        public string GetExpPrice(string p0)
        {
            string sql = @" SELECT NVL(SUM( ROUND( -P_EXG_QTY * PMN_DISC_CPRICE )) ,0) F6 
                            FROM V_COST_FA0071E   
                            WHERE DATA_YM  = :DATA_YM ";
            return DBWork.Connection.ExecuteScalar(sql, new { DATA_YM = p0 }, DBWork.Transaction).ToString();
        }

    }
}
