using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Repository.AB
{
    public class AB0122Repository : JCLib.Mvc.BaseRepository
    {
        public AB0122Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AB0122> GetAll(string data_ym, string rownum, string none_cost, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @" select * from (SELECT A.DATA_YM, B.M_NHIKEY, B.MMNAME_E, B.BASE_UNIT, A.DISC_UPRICE, A.DISC_CPRICE, A.MN_INQTY,
                          NVL(A.MN_INQTY * A.DISC_CPRICE,0) as TOT_AMT, 
                          C.INV_QTY, NVL(C.INV_QTY * A.DISC_CPRICE,0) as FNL_AMT,
                          CASE WHEN A.MN_INQTY = 0 THEN NULL ELSE round(C.INV_QTY / A.MN_INQTY, 7) END as RR,
                          A.CONT_PRICE,
                          CASE WHEN A.CONT_PRICE = 0 THEN NULL ELSE  round((A.CONT_PRICE - A.DISC_UPRICE) / A.CONT_PRICE, 2)  END as RATIO,
                          INSUAMOUNT(A.MMCODE, A.DATA_YM) as INSUAMOUNT, 
                          A.MMCODE,
                          ROUND(NVL( A.MN_INQTY * A.DISC_CPRICE - C.INV_QTY * A.DISC_CPRICE ,0),0) as CONSUME_AMT
                        FROM ( SELECT * FROM MI_WHCOST WHERE DATA_YM = :p0 ) A, 
                             ( SELECT * FROM MI_MAST WHERE MAT_CLASS = '01') B, 
                             ( SELECT MMCODE, 
                                      SUM(INV_QTY) INV_QTY 
                               FROM MI_WINVMON 
                               WHERE DATA_YM = :p0
                               GROUP BY MMCODE
                             ) C
                        WHERE A.MMCODE = B.MMCODE
                        AND   A.MMCODE  = C.MMCODE
                        AND   A.DATA_YM = :p0
                        order by CONSUME_AMT desc) 
                        where 1=1 ";

            if (rownum != string.Empty) {
                sql += "  and rownum <= :p1";
            }

            p.Add(":p0", data_ym);
            p.Add(":p1", rownum);

            if (none_cost != string.Empty) {
                switch (none_cost) {
                    case "1":
                        sql += "  and (MN_INQTY - INV_QTY) > 0 ";
                        break;
                    case "2":
                        sql += "  and (MN_INQTY - INV_QTY) = 0 ";
                        break;
                    case "3":
                        sql += "  and (MN_INQTY - INV_QTY) < 0 ";
                        break;
                    default:
                        sql += string.Empty;
                        break;
                }
            }
            
            //if (none_cost == "True" || none_cost == "true")
            //    sql += " AND (MN_INQTY - INV_QTY) >= 0 ";
            //else
            //    sql += " AND (MN_INQTY - INV_QTY) > 0 ";

            //p.Add("OFFSET", (page_index - 1) * page_size);
            //p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AB0122>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<AB0122> GetPrintData(string data_ym, string rownum, string none_cost)
        {
            var sql = @" select ROWNUM, DATA_YM, M_NHIKEY, MMNAME_E, BASE_UNIT, DISC_UPRICE, DISC_CPRICE, MN_INQTY,
                        TOT_AMT, INV_QTY, FNL_AMT, RR, CONT_PRICE, RATIO, INSUAMOUNT, MMCODE, CONSUME_AMT
                        from (SELECT A.DATA_YM, B.M_NHIKEY, B.MMNAME_E, B.BASE_UNIT, A.DISC_UPRICE, A.DISC_CPRICE, A.MN_INQTY,
                          NVL(A.MN_INQTY * A.DISC_CPRICE,0) as TOT_AMT, 
                          C.INV_QTY, NVL(C.INV_QTY * A.DISC_CPRICE,0) as FNL_AMT,
                          CASE WHEN A.MN_INQTY = 0 THEN NULL ELSE round(C.INV_QTY / A.MN_INQTY, 7) END as RR,
                          A.CONT_PRICE,
                          CASE WHEN A.CONT_PRICE = 0 THEN NULL ELSE  round((A.CONT_PRICE - A.DISC_UPRICE) / A.CONT_PRICE, 2)  END as RATIO,
                          INSUAMOUNT(A.MMCODE, A.DATA_YM) as INSUAMOUNT, 
                          A.MMCODE,
                          ROUND(NVL( A.MN_INQTY * A.DISC_CPRICE - C.INV_QTY * A.DISC_CPRICE ,0),0) as CONSUME_AMT
                        FROM ( SELECT * FROM MI_WHCOST WHERE DATA_YM = :p0 ) A, 
                             ( SELECT * FROM MI_MAST WHERE MAT_CLASS = '01') B, 
                             ( SELECT MMCODE, 
                                      SUM(INV_QTY) INV_QTY 
                               FROM MI_WINVMON 
                               WHERE DATA_YM = :p0
                               GROUP BY MMCODE
                             ) C
                        WHERE A.MMCODE = B.MMCODE
                        AND   A.MMCODE  = C.MMCODE
                        AND   A.DATA_YM = :p0
                        order by CONSUME_AMT desc) 
                        where 1=1";
            if (rownum != string.Empty)
            {
                sql += "  and rownum <= :p1";
            }

            if (none_cost != string.Empty)
            {
                switch (none_cost)
                {
                    case "1":
                        sql += "  and (MN_INQTY - INV_QTY) > 0 ";
                        break;
                    case "2":
                        sql += "  and (MN_INQTY - INV_QTY) = 0 ";
                        break;
                    case "3":
                        sql += "  and (MN_INQTY - INV_QTY) < 0 ";
                        break;
                    default:
                        sql += string.Empty;
                        break;
                }
            }

            //if (none_cost == "True" || none_cost == "true")
            //    sql += " AND (MN_INQTY - INV_QTY) >= 0 ";
            //else
            //    sql += " AND (MN_INQTY - INV_QTY) > 0 ";

            return DBWork.Connection.Query<AB0122>(sql, new { p0 = data_ym, p1 = rownum }, DBWork.Transaction);
        }

        public string getUserName(string userId)
        {
            var sql = @" select TUSER || '.' || UNA from UR_ID where TUSER = :USERID ";

            return DBWork.Connection.QueryFirst<string>(sql, new { USERID = userId }, DBWork.Transaction);
        }
    }
}