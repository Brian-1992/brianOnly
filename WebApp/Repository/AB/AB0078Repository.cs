using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Repository.AB
{
    public class AB0078Repository : JCLib.Mvc.BaseRepository
    {
        public AB0078Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AB0078> GetAll(string data_ym, string rownum, string none_cost, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @" select ROWNUM, DATA_YM, M_NHIKEY, MMNAME_E, BASE_UNIT, DISC_UPRICE, AVG_PRICE, MN_INQTY,
                        TOT_AMT, INV_QTY, FNL_AMT, RR, CONT_PRICE, RATIO, INSUAMOUNT, MMCODE, CONSUME_AMT, USE_QTY
                        from (SELECT A.DATA_YM, B.M_NHIKEY, B.MMNAME_E, B.BASE_UNIT, A.DISC_UPRICE, A.AVG_PRICE, A.MN_INQTY,
                          NVL(MMCODE_INCOST(A.DATA_YM, A.MMCODE, A.MN_INQTY, A.DISC_UPRICE), 0) as TOT_AMT,
                          C.INV_QTY, NVL(C.INV_QTY * A.AVG_PRICE,0) as FNL_AMT,
                          CASE WHEN A.MN_INQTY = 0 THEN NULL ELSE round(C.INV_QTY / A.MN_INQTY, 7) END as RR,
                          A.CONT_PRICE,
                          CASE WHEN A.CONT_PRICE = 0 THEN NULL ELSE  round((A.CONT_PRICE - A.DISC_UPRICE) / A.CONT_PRICE, 2)  END as RATIO,
                          INSUAMOUNT(A.MMCODE, A.DATA_YM) as INSUAMOUNT, 
                          A.MMCODE,
                          ROUND(NVL(nvl( C.USE_QTY,0) * A.AVG_PRICE, 0),0) as CONSUME_AMT,
                          nvl( C.USE_QTY,0) USE_QTY
                        FROM ( SELECT * FROM MI_WHCOST WHERE DATA_YM = :p0 ) A, 
                             ( SELECT * FROM MI_MAST WHERE MAT_CLASS = '01') B,
                             (SELECT MMCODE FROM mi_winvmon where data_ym=:p0 group by mmcode) D
                             left join
                             ( select mmcode, inv_qty, (OTH_CONSUME + ALLQTY1 + ALLQTY2 + ALLQTY3) as use_qty 
                                 from MI_CONSUME_MN 
                                where data_ym = :p0
                             ) C on (D.MMCODE  = C.MMCODE)
                        WHERE A.MMCODE = B.MMCODE
                        AND A.MMCODE = D.MMCODE
                        AND   A.DATA_YM = :p0
                        order by CONSUME_AMT desc) 
                        where 1=1 ";

            if (rownum != string.Empty)
            {
                sql += "  and rownum <= :p1";
            }

            p.Add(":p0", data_ym);
            p.Add(":p1", rownum);

            if (none_cost != string.Empty)
            {
                switch (none_cost)
                {
                    case "1":
                        sql += "  and USE_QTY > 0 ";
                        break;
                    case "2":
                        sql += "  and USE_QTY = 0 ";
                        break;
                    case "3":
                        sql += "  and USE_QTY < 0 ";
                        break;
                    default:
                        sql += string.Empty;
                        break;
                }
            }

            return DBWork.Connection.Query<AB0078>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<AB0078> GetPrintData(string data_ym, string rownum, string none_cost)
        {
            var sql = @" select ROWNUM, DATA_YM, M_NHIKEY, MMNAME_E, BASE_UNIT, DISC_UPRICE, AVG_PRICE, MN_INQTY,
                        TOT_AMT, INV_QTY, FNL_AMT, RR, CONT_PRICE, RATIO, INSUAMOUNT, MMCODE, CONSUME_AMT, USE_QTY
                        from (SELECT A.DATA_YM, B.M_NHIKEY, B.MMNAME_E, B.BASE_UNIT, A.DISC_UPRICE, A.AVG_PRICE, A.MN_INQTY,
                          NVL(MMCODE_INCOST(A.DATA_YM, A.MMCODE, A.MN_INQTY, A.DISC_UPRICE), 0) as TOT_AMT,
                          C.INV_QTY, NVL(C.INV_QTY * A.AVG_PRICE,0) as FNL_AMT,
                          CASE WHEN A.MN_INQTY = 0 THEN NULL ELSE round(C.INV_QTY / A.MN_INQTY, 7) END as RR,
                          A.CONT_PRICE,
                          CASE WHEN A.CONT_PRICE = 0 THEN NULL ELSE  round((A.CONT_PRICE - A.DISC_UPRICE) / A.CONT_PRICE, 2)  END as RATIO,
                          INSUAMOUNT(A.MMCODE, A.DATA_YM) as INSUAMOUNT, 
                          A.MMCODE,
                          ROUND(NVL(nvl( C.USE_QTY,0) * A.AVG_PRICE, 0),0) as CONSUME_AMT,
                          nvl( C.USE_QTY,0) USE_QTY
                        FROM ( SELECT * FROM MI_WHCOST WHERE DATA_YM = :p0 ) A, 
                             ( SELECT * FROM MI_MAST WHERE MAT_CLASS = '01') B,
                             (SELECT MMCODE FROM mi_winvmon where data_ym=:p0 group by mmcode) D
                             left join
                             ( select mmcode, inv_qty, (OTH_CONSUME + ALLQTY1 + ALLQTY2 + ALLQTY3) as use_qty 
                                 from MI_CONSUME_MN 
                                where data_ym = :p0
                             ) C on (D.MMCODE  = C.MMCODE)
                        WHERE A.MMCODE = B.MMCODE
                        AND A.MMCODE = D.MMCODE
                        AND   A.DATA_YM = :p0
                        order by CONSUME_AMT desc) 
                        where 1=1 ";
            if (rownum != string.Empty)
            {
                sql += "  and rownum <= :p1";
            }

            if (none_cost != string.Empty)
            {
                switch (none_cost)
                {
                    case "1":
                        sql += "  and USE_QTY > 0 ";
                        break;
                    case "2":
                        sql += "  and USE_QTY = 0 ";
                        break;
                    case "3":
                        sql += "  and USE_QTY < 0 ";
                        break;
                    default:
                        sql += string.Empty;
                        break;
                }
            }

            return DBWork.Connection.Query<AB0078>(sql, new { p0 = data_ym, p1 = rownum }, DBWork.Transaction);
        }

        public string getUserName(string userId)
        {
            var sql = @" select TUSER || '.' || UNA from UR_ID where TUSER = :USERID ";

            return DBWork.Connection.QueryFirst<string>(sql, new { USERID = userId }, DBWork.Transaction);
        }
    }
}