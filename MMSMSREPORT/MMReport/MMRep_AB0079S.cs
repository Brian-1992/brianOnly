using Sybase.Data.AseClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMSMSREPORT.MMReport
{
    public class MMRep_AB0079S
    {
        //連線DB
        TsghConn tsghconn = new TsghConn();

        public IList<T> GetDrugOrdercode_TEST<T>()
        {
            using (var conn = new AseConnection(tsghconn.TsghDB_IPD()))
            {
                IList<T> ILGetDrugOrdercode = conn.SyBaseQuery<T>(
"SELECT ORDERCODE,                  " +
"       ORDERENGNAME,               " +
"       CREATEYM,                   " +
"       SUM(SUMQTY) AS SUMQTY,      " +
"       SUM(SUMAMOUNT) AS SUMAMOUNT," +
"       SUM(OPDQTY) AS OPDQTY,      " +
"       SUM(OPDAMOUNT) AS OPDAMOUNT," +
"       'S' AS DSM                  " +
"  FROM                             " +
"  (                                " +
"  SELECT PROF.ORDERCODE,           " +
"         ORDM.ORDERENGNAME,        " +
"         SUBSTRING(PROF.CREATEDATETIME, 1, 5) CREATEYM," +
"         SUM(EXND.USEQTY) SUMQTY,                      " +
"         SUM(CASE WHEN EXND.PAYFLAG = 'N'              " +
"                   THEN EXND.USEQTY * EXND.INSUAMOUNT1 " +
"                   ELSE EXND.USEQTY * EXND.PAYAMOUNT1  " +
"                   END) SUMAMOUNT,                     " +
"         0 AS OPDQTY,              " +
"         0 AS OPDAMOUNT            " +
"  FROM HISPROF PROF,               " +
"       HISEXND EXND,               " +
"       BASORDM ORDM,               " +
"       INACARM CARM                " +
"  WHERE(PROF.CREATEDATETIME BETWEEN '1080401080000' AND '1080401083000')" +
"    AND PROF.MEDNO = EXND.MEDNO                    " +
"    AND PROF.VISITSEQ = EXND.VISITSEQ              " +
"    AND PROF.ORDERNO = EXND.ORDERNO                " +
"    AND PROF.ORDERCODE = ORDM.ORDERCODE            " +
"    AND ORDM.ORDERTYPE BETWEEN 'D000' AND 'DZZZ'   " +
"    AND PROF.MEDNO = CARM.MEDNO                    " +
"    AND PROF.VISITSEQ = CARM.VISITSEQ              " +
"    AND CARM.CANCELFLAG = 'N'                      " +
"  GROUP BY PROF.ORDERCODE,                         " +
"           ORDM.ORDERENGNAME,                      " +
"           SUBSTRING(PROF.CREATEDATETIME, 1, 5)    " +
"  UNION ALL                                        " +
"  SELECT ORDM.ORDERCODE,                           " +
"         ORDM.ORDERENGNAME,                        " +
"         SUBSTRING((ORDO.WORKDATE || ORDO.WORKTIME), 1, 5) CREATEYM," +
"         0 AS SUMQTY,                              " +
"         0 AS SUMAMOUNT,                           " +
"         SUM(ORDO.SUMQTY) OPDQTY,                  " +
"         SUM(CASE WHEN ORDO.PAYFLAG = 'N'          " +
"                   THEN ORDO.SUMQTY * ORDO.INSUAMOUNT1     " +
"                   ELSE ORDO.SUMQTY * ORDO.PAYAMOUNT1      " +
"                   END) OPDAMOUNT                          " +
"  FROM XMYORDO ORDO,                       " +
"       BASORDM ORDM                        " +
"  WHERE((ORDO.WORKDATE || ORDO.WORKTIME) BETWEEN '1080401080000' AND '1080401083000')  " +
"    AND ORDO.ORDERCODE = ORDM.ORDERCODE    " +
"  GROUP BY ORDM.ORDERCODE,                 " +
"           ORDM.ORDERENGNAME,              " +
"           SUBSTRING((ORDO.WORKDATE || ORDO.WORKTIME), 1, 5)       " +
"  )EXNDBACK                " +
"GROUP BY ORDERCODE,        " +
"         ORDERENGNAME,     " +
"         CREATEYM          ").ToList();
                return ILGetDrugOrdercode;
            }
        }

        public IList<T> GetDrugOrdercode<T>(string SCDT, string ECDT)
        {
            using (var conn = new AseConnection(tsghconn.TsghDB_IPD()))
            {
                IList<T> ILGetDrugOrdercode = conn.SyBaseQuery<T>(
"SELECT ORDERCODE,                  " +
"       ORDERENGNAME,               " +
"       CREATEYM,                   " +
"       SUM(SUMQTY) AS SUMQTY,      " +
"       SUM(SUMAMOUNT) AS SUMAMOUNT," +
"       SUM(OPDQTY) AS OPDQTY,      " +
"       SUM(OPDAMOUNT) AS OPDAMOUNT," +
"       'S' AS DSM                  " +
"  FROM                             " +
"  (                                " +
"  SELECT PROF.ORDERCODE,           " +
"         ORDM.ORDERENGNAME,        " +
"         SUBSTRING(PROF.CREATEDATETIME, 1, 5) CREATEYM," +
"         SUM(EXND.USEQTY) SUMQTY,                      " +
"         SUM(CASE WHEN EXND.PAYFLAG = 'N'              " +
"                   THEN EXND.USEQTY * EXND.INSUAMOUNT1 " +
"                   ELSE EXND.USEQTY * EXND.PAYAMOUNT1  " +
"                   END) SUMAMOUNT,                     " +
"         0 AS OPDQTY,              " +
"         0 AS OPDAMOUNT            " +
"  FROM HISPROF PROF,               " +
"       HISEXND EXND,               " +
"       BASORDM ORDM,               " +
"       INACARM CARM                " +
"  WHERE(PROF.CREATEDATETIME BETWEEN '" + SCDT + "' AND '" + ECDT + "')" +
"    AND PROF.MEDNO = EXND.MEDNO                    " +
"    AND PROF.VISITSEQ = EXND.VISITSEQ              " +
"    AND PROF.ORDERNO = EXND.ORDERNO                " +
"    AND PROF.ORDERCODE = ORDM.ORDERCODE            " +
"    AND ORDM.ORDERTYPE BETWEEN 'D000' AND 'DZZZ'   " +
"    AND PROF.MEDNO = CARM.MEDNO                    " +
"    AND PROF.VISITSEQ = CARM.VISITSEQ              " +
"    AND CARM.CANCELFLAG = 'N'                      " +
"  GROUP BY PROF.ORDERCODE,                         " +
"           ORDM.ORDERENGNAME,                      " +
"           SUBSTRING(PROF.CREATEDATETIME, 1, 5)    " +
"  UNION ALL                                        " +
"  SELECT ORDM.ORDERCODE,                           " +
"         ORDM.ORDERENGNAME,                        " +
"         SUBSTRING((ORDO.WORKDATE || ORDO.WORKTIME), 1, 5) CREATEYM," +
"         0 AS SUMQTY,                              " +
"         0 AS SUMAMOUNT,                           " +
"         SUM(ORDO.SUMQTY) OPDQTY,                  " +
"         SUM(CASE WHEN ORDO.PAYFLAG = 'N'          " +
"                   THEN ORDO.SUMQTY * ORDO.INSUAMOUNT1     " +
"                   ELSE ORDO.SUMQTY * ORDO.PAYAMOUNT1      " +
"                   END) OPDAMOUNT                          " +
"  FROM XMYORDO ORDO,                       " +
"       BASORDM ORDM                        " +
"  WHERE((ORDO.WORKDATE || ORDO.WORKTIME) BETWEEN '" + SCDT + "' AND '" + ECDT + "')  " +
"    AND ORDO.ORDERCODE = ORDM.ORDERCODE    " +
"  GROUP BY ORDM.ORDERCODE,                 " +
"           ORDM.ORDERENGNAME,              " +
"           SUBSTRING((ORDO.WORKDATE || ORDO.WORKTIME), 1, 5)       " +
"  )EXNDBACK                " +
"GROUP BY ORDERCODE,        " +
"         ORDERENGNAME,     " +
"         CREATEYM          ").ToList();
                return ILGetDrugOrdercode;
            }
        }

    }
}
