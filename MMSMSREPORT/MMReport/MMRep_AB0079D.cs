using Sybase.Data.AseClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMSMSREPORT.MMReport
{
    public class MMRep_AB0079D
    {
        //連線DB
        TsghConn tsghconn = new TsghConn();
        
        public IList<T> GetDocOrdercode_TEST<T>()
        {
            using (var conn = new AseConnection(tsghconn.TsghDB_IPD()))
            {
                IList<T> ILGetDocOrdercode = conn.SyBaseQuery<T>(
"  SELECT ORDERCODE,                        " +
"       ORDERENGNAME,                       " +
"       CREATEYM,                           " +
"       ORDERDR,                            " +
"       CHINNAME,                           " +
"       SECTIONNO,                          " +
"       '' SECTIONNAME,                     " +
"       SUM(SUMQTY) AS SUMQTY,              " +
"       SUM(SUMAMOUNT) AS SUMAMOUNT,        " +
"       SUM(OPDQTY) AS OPDQTY,              " +
"       SUM(OPDAMOUNT) AS OPDAMOUNT,        " +
"       'D' AS DSM                          " +
"  FROM                                     " +
"  (                                        " +
"  SELECT PROF.ORDERCODE,                   " +
"         ORDM.ORDERENGNAME,                " +
"         SUBSTRING(PROF.CREATEDATETIME, 1, 5) CREATEYM,    " +
"         PROF.ORDERDR,                                     " +
"         USRM.CHINNAME,                                    " +
"         CARM.SECTIONNO,                                   " +
"         SUM(EXND.USEQTY) SUMQTY,                          " +
"         SUM(CASE WHEN EXND.PAYFLAG = 'N'                  " +
"              THEN EXND.USEQTY * EXND.INSUAMOUNT1          " +
"              ELSE EXND.USEQTY * EXND.PAYAMOUNT1           " +
"              END) SUMAMOUNT,                              " +
"         0 AS OPDQTY,                                      " +
"         0 AS OPDAMOUNT                                    " +
"  FROM HISPROF PROF, " +
"       HISEXND EXND, " +
"       BASORDM ORDM, " +
"       INACARM CARM, " +
"       BASUSRM USRM " +
"  WHERE(PROF.CREATEDATETIME BETWEEN '1080401080000' AND '1080401083000') " +
"    AND PROF.MEDNO = EXND.MEDNO                        " +
"    AND PROF.VISITSEQ = EXND.VISITSEQ                  " +
"    AND PROF.ORDERNO = EXND.ORDERNO                    " +
"    AND PROF.ORDERDR = USRM.USERID                     " +
"    AND PROF.ORDERCODE = ORDM.ORDERCODE                " +
"    AND ORDM.ORDERTYPE BETWEEN 'D000' AND 'DZZZ'       " +
"    AND PROF.MEDNO = CARM.MEDNO                        " +
"    AND PROF.VISITSEQ = CARM.VISITSEQ                  " +
"    AND CARM.CANCELFLAG = 'N'                          " +
"  GROUP BY PROF.ORDERCODE,                             " +
"           ORDM.ORDERENGNAME,                          " +
"           SUBSTRING(PROF.CREATEDATETIME, 1, 5),       " +
"           CARM.SECTIONNO,                             " +
"           PROF.ORDERDR,                               " +
"           USRM.CHINNAME                               " +
"  UNION ALL                                            " +
"  SELECT ORDM.ORDERCODE,                               " +
"         ORDM.ORDERENGNAME,                            " +
"         SUBSTRING((ORDO.WORKDATE || ORDO.WORKTIME), 1, 5) CREATEYM, " +
"         ORDO.ORDERDR,                                 " +
"         USRM.CHINNAME,                                " +
"         ORDO.SECTIONNO,                               " +
"         0 AS SUMQTY,                                  " +
"         0 AS SUMAMOUNT,                               " +
"         SUM(ORDO.SUMQTY) OPDQTY,                      " +
"         SUM(CASE WHEN ORDO.PAYFLAG = 'N'              " +
"              THEN ORDO.SUMQTY * ORDO.INSUAMOUNT1      " +
"              ELSE ORDO.SUMQTY * ORDO.PAYAMOUNT1       " +
"              END) OPDAMOUNT                           " +
"  FROM XMYORDO ORDO,                                   " +
"       BASORDM ORDM,                                   " +
"       BASUSRM USRM                                    " +
"  WHERE((ORDO.WORKDATE || ORDO.WORKTIME) BETWEEN '1080401080000' AND '1080401083000') " +
"    AND ORDO.ORDERCODE = ORDM.ORDERCODE    " +
"    AND ORDO.ORDERDR = USRM.USERID         " +
"  GROUP BY ORDM.ORDERCODE,                 " +
"           ORDM.ORDERENGNAME,              " +
"           SUBSTRING((ORDO.WORKDATE || ORDO.WORKTIME), 1, 5),                         " +
"           ORDO.SECTIONNO,                 " +
"           ORDO.ORDERDR,                   " +
"           USRM.CHINNAME                   " +
"  ) EXNDBACK                               " +
"GROUP BY ORDERCODE,                        " +
"         ORDERENGNAME,                     " +
"         CREATEYM,                         " +
"         SECTIONNO,                        " +
"         ORDERDR,                          " +
"         CHINNAME").ToList();
                return ILGetDocOrdercode;
            }
        }

        public IList<T> GetDocOrdercode<T>(string SCDT, string ECDT)
        {
            using (var conn = new AseConnection(tsghconn.TsghDB_IPD()))
            {
                IList<T> ILGetDocOrdercode = conn.SyBaseQuery<T>(
"  SELECT ORDERCODE,                        " +
"       ORDERENGNAME,                       " +
"       CREATEYM,                           " +
"       ORDERDR,                            " +
"       CHINNAME,                           " +
"       SECTIONNO,                          " +
"       '' SECTIONNAME,                     " +
"       SUM(SUMQTY) AS SUMQTY,              " +
"       SUM(SUMAMOUNT) AS SUMAMOUNT,        " +
"       SUM(OPDQTY) AS OPDQTY,              " +
"       SUM(OPDAMOUNT) AS OPDAMOUNT,        " +
"       'D' AS DSM                          " +
"  FROM                                     " +
"  (                                        " +
"  SELECT PROF.ORDERCODE,                   " +
"         ORDM.ORDERENGNAME,                " +
"         SUBSTRING(PROF.CREATEDATETIME, 1, 5) CREATEYM,    " +
"         PROF.ORDERDR,                                     " +
"         USRM.CHINNAME,                                    " +
"         CARM.SECTIONNO,                                   " +
"         SUM(EXND.USEQTY) SUMQTY,                          " +
"         SUM(CASE WHEN EXND.PAYFLAG = 'N'                  " +
"              THEN EXND.USEQTY * EXND.INSUAMOUNT1          " +
"              ELSE EXND.USEQTY * EXND.PAYAMOUNT1           " +
"              END) SUMAMOUNT,                              " +
"         0 AS OPDQTY,                                      " +
"         0 AS OPDAMOUNT                                    " +
"  FROM HISPROF PROF,                                       " +
"       HISEXND EXND,                                       " +
"       BASORDM ORDM,                                       " +
"       INACARM CARM,                                       " +
"       BASUSRM USRM                                        " +
"  WHERE(PROF.CREATEDATETIME BETWEEN '" + SCDT + "' AND '" + ECDT + "') " +
"    AND PROF.MEDNO = EXND.MEDNO                        " +
"    AND PROF.VISITSEQ = EXND.VISITSEQ                  " +
"    AND PROF.ORDERNO = EXND.ORDERNO                    " +
"    AND PROF.ORDERDR = USRM.USERID                     " +
"    AND PROF.ORDERCODE = ORDM.ORDERCODE                " +
"    AND ORDM.ORDERTYPE BETWEEN 'D000' AND 'DZZZ'       " +
"    AND PROF.MEDNO = CARM.MEDNO                        " +
"    AND PROF.VISITSEQ = CARM.VISITSEQ                  " +
"    AND CARM.CANCELFLAG = 'N'                          " +
"  GROUP BY PROF.ORDERCODE,                             " +
"           ORDM.ORDERENGNAME,                          " +
"           SUBSTRING(PROF.CREATEDATETIME, 1, 5),       " +
"           CARM.SECTIONNO,                             " +
"           PROF.ORDERDR,                               " +
"           USRM.CHINNAME                               " +
"  UNION ALL                                            " +
"  SELECT ORDM.ORDERCODE,                               " +
"         ORDM.ORDERENGNAME,                            " +
"         SUBSTRING((ORDO.WORKDATE || ORDO.WORKTIME), 1, 5) CREATEYM,               " +
"         ORDO.ORDERDR,                                 " +
"         USRM.CHINNAME,                                " +
"         ORDO.SECTIONNO,                               " +
"         0 AS SUMQTY,                                  " +
"         0 AS SUMAMOUNT,                               " +
"         SUM(ORDO.SUMQTY) OPDQTY,                      " +
"         SUM(CASE WHEN ORDO.PAYFLAG = 'N'              " +
"              THEN ORDO.SUMQTY * ORDO.INSUAMOUNT1      " +
"              ELSE ORDO.SUMQTY * ORDO.PAYAMOUNT1       " +
"              END) OPDAMOUNT                           " +
"  FROM XMYORDO ORDO,                                   " +
"       BASORDM ORDM,                                   " +
"       BASUSRM USRM                                    " +
"  WHERE((ORDO.WORKDATE || ORDO.WORKTIME) BETWEEN '" + SCDT + "' AND '" + ECDT + "') " +
"    AND ORDO.ORDERCODE = ORDM.ORDERCODE    " +
"    AND ORDO.ORDERDR = USRM.USERID         " +
"  GROUP BY ORDM.ORDERCODE,                 " +
"           ORDM.ORDERENGNAME,              " +
"           SUBSTRING((ORDO.WORKDATE || ORDO.WORKTIME), 1, 5),                       " +
"           ORDO.SECTIONNO,                 " +
"           ORDO.ORDERDR,                   " +
"           USRM.CHINNAME                   " +
"  ) EXNDBACK                               " +
"GROUP BY ORDERCODE,                        " +
"         ORDERENGNAME,                     " +
"         CREATEYM,                         " +
"         SECTIONNO,                        " +
"         ORDERDR,                          " +
"         CHINNAME                          ").ToList();
                return ILGetDocOrdercode;
            }
        }

    }
}
