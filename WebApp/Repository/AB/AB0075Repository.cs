using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Repository.AB
{
    public class AB0075Repository : JCLib.Mvc.BaseRepository
    {
        public AB0075Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        //報表PH1A、PH1R、PHMC、PH1C
        public IEnumerable<AB0075_1M> Print_1(string MONTH)
        {
            var p = new DynamicParameters();

            string sql = @"  SELECT EE.WORKDATE,
                                    EE.SUMORDERNO,
                                    EE.SUMBEDNO,
                                    SUM (
                                         CASE
                                             WHEN DD.WORKDATE_2 BETWEEN '0001' AND '0800' THEN DD.CNTORDERNO
                                             ELSE 0
                                         END) A0000,
                                    SUM (
                                         CASE
                                             WHEN DD.WORKDATE_2 BETWEEN '0001' AND '0800' THEN DD.CNTRXNO
                                             ELSE 0
                                         END) B0000,
                                    SUM (
                                         CASE
                                             WHEN DD.WORKDATE_2 BETWEEN '0801' AND '0900' THEN DD.CNTORDERNO
                                             ELSE 0
                                         END) A0801,
                                    SUM (
                                         CASE
                                             WHEN DD.WORKDATE_2 BETWEEN '0801' AND '0900' THEN DD.CNTRXNO
                                             ELSE 0
                                         END) B0801,
                                    SUM (
                                         CASE
                                             WHEN DD.WORKDATE_2 BETWEEN '0901' AND '1000' THEN DD.CNTORDERNO
                                             ELSE 0
                                         END) A0901,
                                    SUM (
                                         CASE
                                             WHEN DD.WORKDATE_2 BETWEEN '0901' AND '1000' THEN DD.CNTRXNO
                                             ELSE 0
                                         END) B0901,
                                    SUM (
                                         CASE
                                             WHEN DD.WORKDATE_2 BETWEEN '1001' AND '1100' THEN DD.CNTORDERNO
                                             ELSE 0
                                         END) A1001,
                                    SUM (
                                         CASE
                                             WHEN DD.WORKDATE_2 BETWEEN '1001' AND '1100' THEN DD.CNTRXNO
                                             ELSE 0
                                         END) B1001,
                                    SUM (
                                         CASE
                                             WHEN DD.WORKDATE_2 BETWEEN '1101' AND '1200' THEN DD.CNTORDERNO
                                             ELSE 0
                                         END) A1101,
                                    SUM (
                                         CASE
                                             WHEN DD.WORKDATE_2 BETWEEN '1101' AND '1200' THEN DD.CNTRXNO
                                             ELSE 0
                                         END) B1101,
                                    SUM (
                                         CASE
                                             WHEN DD.WORKDATE_2 BETWEEN '1201' AND '1300' THEN DD.CNTORDERNO
                                             ELSE 0
                                         END) A1201,
                                    SUM (
                                         CASE
                                             WHEN DD.WORKDATE_2 BETWEEN '1201' AND '1300' THEN DD.CNTRXNO
                                             ELSE 0
                                         END) B1201,
                                    SUM (
                                         CASE
                                             WHEN DD.WORKDATE_2 BETWEEN '1301' AND '1400' THEN DD.CNTORDERNO
                                             ELSE 0
                                         END) A1301,
                                    SUM (
                                         CASE
                                             WHEN DD.WORKDATE_2 BETWEEN '1301' AND '1400' THEN DD.CNTRXNO
                                             ELSE 0
                                         END) B1301,
                                    SUM (
                                         CASE
                                             WHEN DD.WORKDATE_2 BETWEEN '1401' AND '1500' THEN DD.CNTORDERNO
                                             ELSE 0
                                         END) A1401,
                                    SUM (
                                         CASE
                                             WHEN DD.WORKDATE_2 BETWEEN '1401' AND '1500' THEN DD.CNTRXNO
                                             ELSE 0
                                         END) B1401,
                                    SUM (
                                         CASE
                                             WHEN DD.WORKDATE_2 BETWEEN '1501' AND '1600' THEN DD.CNTORDERNO
                                             ELSE 0
                                         END) A1501,
                                    SUM (
                                         CASE
                                             WHEN DD.WORKDATE_2 BETWEEN '1501' AND '1600' THEN DD.CNTRXNO
                                             ELSE 0
                                         END) B1501,
                                    SUM (
                                         CASE
                                             WHEN DD.WORKDATE_2 BETWEEN '1601' AND '1700' THEN DD.CNTORDERNO
                                             ELSE 0
                                         END) A1601,
                                    SUM (
                                         CASE
                                             WHEN DD.WORKDATE_2 BETWEEN '1601' AND '1700' THEN DD.CNTRXNO
                                             ELSE 0
                                         END) B1601,
                                    SUM (
                                         CASE
                                             WHEN DD.WORKDATE_2 BETWEEN '1701' AND '1800' THEN DD.CNTORDERNO
                                             ELSE 0
                                         END) A1701,
                                    SUM (
                                         CASE
                                             WHEN DD.WORKDATE_2 BETWEEN '1701' AND '1800' THEN DD.CNTRXNO
                                             ELSE 0
                                         END) B1701,
                                    SUM (
                                         CASE
                                             WHEN DD.WORKDATE_2 BETWEEN '1801' AND '1900' THEN DD.CNTORDERNO
                                             ELSE 0
                                         END) A1801,
                                    SUM (
                                         CASE
                                             WHEN DD.WORKDATE_2 BETWEEN '1801' AND '1900' THEN DD.CNTRXNO
                                             ELSE 0
                                         END) B1801,
                                    SUM (
                                         CASE
                                             WHEN DD.WORKDATE_2 BETWEEN '1901' AND '2000' THEN DD.CNTORDERNO
                                             ELSE 0
                                         END) A1901,
                                    SUM (
                                         CASE
                                             WHEN DD.WORKDATE_2 BETWEEN '1901' AND '2000' THEN DD.CNTRXNO
                                             ELSE 0
                                         END) B1901,
                                    SUM (
                                         CASE
                                             WHEN DD.WORKDATE_2 BETWEEN '2001' AND '2100' THEN DD.CNTORDERNO
                                             ELSE 0
                                         END) A2001,
                                    SUM (
                                         CASE
                                             WHEN DD.WORKDATE_2 BETWEEN '2001' AND '2100' THEN DD.CNTRXNO
                                             ELSE 0
                                         END) B2001,
                                    SUM (
                                         CASE
                                             WHEN DD.WORKDATE_2 BETWEEN '2101' AND '2200' THEN DD.CNTORDERNO
                                             ELSE 0
                                         END) A2101,
                                    SUM (
                                         CASE
                                             WHEN DD.WORKDATE_2 BETWEEN '2101' AND '2200' THEN DD.CNTRXNO
                                             ELSE 0
                                         END) B2101,
                                    SUM (
                                         CASE
                                             WHEN DD.WORKDATE_2 BETWEEN '2201' AND '2300' THEN DD.CNTORDERNO
                                             ELSE 0
                                         END) A2201,
                                    SUM (
                                         CASE
                                             WHEN DD.WORKDATE_2 BETWEEN '2201' AND '2300' THEN DD.CNTRXNO
                                             ELSE 0
                                         END) B2201,
                                    SUM (
                                         CASE
                                             WHEN (   (DD.WORKDATE_2 BETWEEN '2301' AND '2359')
                                                      OR DD.WORKDATE_2 = '0000')
                                             THEN DD.CNTORDERNO
                                             ELSE 0
                                         END) A2301,
                                    SUM (
                                         CASE
                                             WHEN (   (DD.WORKDATE_2 BETWEEN '2301' AND '2359')
                                                      OR DD.WORKDATE_2 = '0000')
                                             THEN DD.CNTRXNO
                                             ELSE 0
                                         END) B2301,
                                    SUM (DD.CNTORDERNO) A_TOTAL,
                                    SUM (DD.CNTRXNO) B_TOTAL
                             FROM (  SELECT SUBSTR (WORKDATE, 1, 7) WORKDATE_1,
                                            SUBSTR (WORKDATE, 8, 4) WORKDATE_2,
                                            SUM (CNTORDERNO) CNTORDERNO,
                                            SUM (CNTRXNO) CNTRXNO
                                     FROM ME_AB0075D
                                     WHERE SUBSTR (WORKDATE, 1, 5) = :P0
                                     GROUP BY SUBSTR (WORKDATE, 1, 7),
                                              SUBSTR (WORKDATE, 8, 4)) DD,
                                  ME_AB0075E EE
                             WHERE DD.WORKDATE_1 = SUBSTR (EE.WORKDATE, 1, 7)
                             GROUP BY DD.WORKDATE_1,
                                      EE.WORKDATE,
                                      EE.SUMORDERNO,
                                      EE.SUMBEDNO
                             ORDER BY WORKDATE";

            p.Add("P0", MONTH);

            return DBWork.Connection.Query<AB0075_1M>(sql, p, DBWork.Transaction);
        }

        //報表CHEMO、CHEMOT
        public IEnumerable<AB0075_2M> Print_2(string MONTH)
        {
            var p = new DynamicParameters();

            string sql = @"SELECT PORC_DATE,
                                  PAT_CNT,
                                  TOT_CNT
                           FROM MMSADM.ME_AB0075A
                           WHERE SUBSTR (PORC_DATE, 1, 5) = :P0";

            p.Add("P0", MONTH);

            return DBWork.Connection.Query<AB0075_2M>(sql, p, DBWork.Transaction);
        }

        //報表PCA
        public IEnumerable<AB0075_3M> Print_3(string MONTH)
        {
            var p = new DynamicParameters();

            string sql = @"SELECT SDATE,
                                  CNT,
                                  ORDERCODE,
                                  SUMQTY
                           FROM ME_AB0075B
                           WHERE SUBSTR (SDATE, 1, 5) = :P0";

            p.Add("P0", MONTH);

            return DBWork.Connection.Query<AB0075_3M>(sql, p, DBWork.Transaction);
        }

        //報表TPN
        public IEnumerable<AB0075_4M> Print_4(string MONTH)
        {
            var p = new DynamicParameters();

            string sql = @"SELECT VISITDATE,
                                  CNTVISITSEQ,
                                  CNTORDERNO
                           FROM ME_AB0075C
                           WHERE SUBSTR (VISITDATE, 1, 5) = :P0";

            p.Add("P0", MONTH);

            return DBWork.Connection.Query<AB0075_4M>(sql, p, DBWork.Transaction);
        }

        //庫房代碼combox
        public IEnumerable<ComboItemModel> GetWH_NO()
        {
            string sql = @" SELECT WH_NO VALUE,
                                   WH_NO || ' ' || WH_NAME TEXT
                            FROM MI_WHMAST
                            WHERE WH_KIND = '0' AND WH_GRADE = '2'
                            UNION ALL
                            SELECT WH_NO VALUE,
                                   WH_NO || ' ' || WH_NAME TEXT
                            FROM MI_WHMAST
                            WHERE WH_NO = 'PCA'
                            ORDER BY VALUE";
            return DBWork.Connection.Query<ComboItemModel>(sql, DBWork.Transaction);
        }
    }
}