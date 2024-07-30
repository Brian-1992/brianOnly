using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JCLib.DB;
using Dapper;
using WebApp.Models;

namespace WebApp.Repository.F
{
    public class FA0065Repository : JCLib.Mvc.BaseRepository
    {
        public FA0065Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        public IEnumerable<FA0065M> GetAll(string data_ym)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT listagg(MAT_CLASS,'$') within group(order by MAT_CLASS) as MAT_CLASS,
                        listagg(P_INV_AMT,'$') within group(order by MAT_CLASS) as P_INV_AMT,
                        listagg(INV_AMT,'$') within group(order by MAT_CLASS) as INV_AMT,
                        listagg(USE_AMT,'$') within group(order by MAT_CLASS) as USE_AMT,
                        listagg(LOWTURN_INV_AMT,'$') within group(order by MAT_CLASS) as LOWTURN_INV_AMT,
                        listagg(D_AMT,'$') within group(order by MAT_CLASS) as D_AMT
                         FROM 
                        (SELECT B.MAT_CLASS,
                        SUM(ROUND(A.P_INV_QTY *A.PMN_DISC_CPRICE,2)) P_INV_AMT,
                        SUM(ROUND(A.INV_QTY*A.DISC_CPRICE,2)) INV_AMT,
                        SUM(ROUND(A.OUT_QTY*A.DISC_CPRICE,2)) USE_AMT,
                        SUM(DECODE(A.OUT_QTY,0,ROUND(A.INV_QTY*A.DISC_CPRICE,2),0)) LOWTURN_INV_AMT,
                        SUM(ROUND(A.P_INV_QTY*(A.DISC_CPRICE-A.PMN_DISC_CPRICE),2)) D_AMT
                          FROM V_COST_WH2 A, MI_MAST B
                         WHERE A.MMCODE=B.MMCODE AND B.MAT_CLASS IN ('02') "; 

            if (data_ym != "")
            {
                sql += " AND A.DATA_YM = :DATA_YM";
                p.Add(":DATA_YM", data_ym);
            }

            sql += " GROUP BY B.MAT_CLASS ORDER BY B.MAT_CLASS )";

            return DBWork.Connection.Query<FA0065M>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetYmCombo()
        {
            string sql = @"SELECT SET_YM AS VALUE, SET_YM AS TEXT, SET_YM AS COMBITEM FROM MI_MNSET WHERE SET_STATUS='C' order by set_ym desc";
            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
    }
}