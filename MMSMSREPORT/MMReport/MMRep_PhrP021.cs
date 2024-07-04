using Sybase.Data.AseClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMSMSREPORT.MMReport
{
    public class MMRep_PhrP021
    {
        //連線DB
        TsghConn tsghconn = new TsghConn();

        /// <summary>
        /// 取得病房退藥清單(HISBACK)一般藥品(不包含1~3級管制藥) FOR TEST
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        //        public IList<T> GetWardBackDrug_TEST<T>()
        //        {
        //            using (var conn = new AseConnection(tsghconn.TsghDB_IPD()))
        //            {
        //                IList<T> GetWardBackDrug = conn.SyBaseQuery<T>(
        //"SELECT                         " +
        //"  HISBACK.MEDNO,               " +
        //"  HISMEDD.CHINNAME,            " +
        //"  HISEXND.NRCODE,              " +
        //"  HISEXND.BEDNO,               " +
        //"  HISBACK.ORDERCODE,           " +
        //"  BASORDM.ORDERENGNAME,        " +
        //"  HISPROF.BEGINDATETIME,       " +
        //"  HISPROF.ENDDATETIME,         " +
        //"  HISEXND.DOSE,                " +
        //"  HISEXND.FREQNO,              " +
        //"  HISBACK.NEEDBACKQTY,         " + 
        //"  HISBACK.BACKQTY,             " +
        //"  HISBACK.BACKQTY - HISBACK.NEEDBACKQTY DIFF,  " +
        //"  HISBACK.PHRBACKREASON,       " +
        //"  HISBACK.CREATEDATETIME,      " +
        //"  HISBACK.CREATEOPID,          " +
        //"  HISMEDD.CHARTNO,             " +
        //"  BASORDM.STOCKUNIT,           " +
        //"  HISPROF.ORDERNO,             " +
        //"  BASORDD.INSUAMOUNT1,         " +
        //"  BASORDD.PAYAMOUNT1,          " +
        //"  (SELECT    MAX(CHINNAME)  FROM BASUSRM  WHERE BASUSRM.USERID = HISBACK.CREATEOPID)  AS BACKNAME, " +
        //"  BASORDM.ORDERUNIT,           " +
        //"  HISBACK.PROCDATETIME,        " +
        //"  HISBACK.BACKKIND,            " +
        //"  BASORDM.ORDERTYPE,           " +
        //"  HISBACK.RETURNSTOCKCODE,     " +
        //"  HISEXND.USEDATETIME          " +
        //"FROM INACARM,                  " +
        //"     HISMEDD,                  " +
        //"     HISPROF,                  " +
        //"     BASORDM,                  " +
        //"     (SELECT       *     FROM HISEXND     WHERE(HISEXND.USEDATETIME BETWEEN '1080325000000' AND '1080326000000')) HISEXND, " +
        //"     (SELECT       *     FROM BASORDD     WHERE BASORDD.ENDDATE = '9991231') BASORDD,  " +
        //"     (SELECT       *     FROM HISBACK     WHERE(HISBACK.CREATEDATETIME BETWEEN '1080325000000' AND '1080326000000')) HISBACK  " +
        //"WHERE INACARM.MEDNO = HISMEDD.MEDNO                " +
        //"AND INACARM.CANCELFLAG = 'N'                       " +
        //"AND INACARM.MEDNO = HISPROF.MEDNO                  " +
        //"AND INACARM.VISITSEQ = HISPROF.VISITSEQ            " +
        //"AND HISPROF.ORDERSORT IN('0', '1')                 " +
        //"AND HISPROF.MEDNO = HISEXND.MEDNO                  " +
        //"AND HISPROF.VISITSEQ = HISEXND.VISITSEQ            " +
        //"AND HISPROF.ORDERNO = HISEXND.ORDERNO              " +
        //"AND HISEXND.ORDERCODE = BASORDM.ORDERCODE          " +
        //"AND HISEXND.ORDERCODE = BASORDD.ORDERCODE          " +
        //"AND HISBACK.MEDNO = HISEXND.MEDNO                  " +
        //"AND HISBACK.VISITSEQ = HISEXND.VISITSEQ            " +
        //"AND HISBACK.ORDERNO = HISEXND.ORDERNO              " +
        //"AND HISBACK.DETAILNO = HISEXND.DETAILNO            " +
        //"AND HISEXND.STOCKFLAG = 'Y'                        " +
        //"AND BASORDM.ORDERTYPE BETWEEN 'D000' AND 'DZZZ'    " +
        //"AND HISBACK.BACKKIND IN('1', '2')                  " +
        //"AND HISEXND.NRCODE IN('11')").ToList();
        //                return GetWardBackDrug;
        //            }
        //        }




        /// <summary>
        /// 取得病房退藥清單(HISBACK)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IList<T> GetWardBackDrug<T>(string SCDT, string ECDT)
        {
            using (var conn = new AseConnection(tsghconn.TsghDB_IPD()))
            {
                IList<T> GetWardBackDrug = conn.SyBaseQuery<T>(
"SELECT                         " +
"  HISBACK.MEDNO,               " +
"  HISMEDD.CHINNAME,            " +
"  HISEXND.NRCODE,              " +
"  HISEXND.BEDNO,               " +
"  HISBACK.ORDERCODE,           " +
"  BASORDM.ORDERENGNAME,        " +
"  HISPROF.BEGINDATETIME,       " +
"  HISPROF.ENDDATETIME,         " +
"  HISEXND.DOSE,                " +
"  HISEXND.FREQNO,              " +
"  HISBACK.NEEDBACKQTY,         " +
"  HISBACK.BACKQTY,             " +
"  HISBACK.BACKQTY - HISBACK.NEEDBACKQTY DIFF,  " +
"  HISBACK.PHRBACKREASON,       " +
"  HISBACK.CREATEDATETIME,      " +
"  HISBACK.CREATEOPID,          " +
"  HISMEDD.CHARTNO,             " +
"  BASORDM.STOCKUNIT,           " +
"  HISPROF.ORDERNO,             " +
"  BASORDD.INSUAMOUNT1,         " +
"  BASORDD.PAYAMOUNT1,          " +
"  (SELECT    MAX(CHINNAME)  FROM BASUSRM  WHERE BASUSRM.USERID = HISBACK.CREATEOPID)  AS BACKNAME, " +
"  BASORDM.ORDERUNIT,           " +
"  HISBACK.PROCDATETIME,        " +
"  HISBACK.BACKKIND,            " +
"  BASORDM.ORDERTYPE,           " +
"  HISBACK.RETURNSTOCKCODE,     " +
"  HISEXND.USEDATETIME          " +
"FROM INACARM,                  " +
"     HISMEDD,                  " +
"     HISPROF,                  " +
"     BASORDM,                  " +
"     (SELECT * FROM HISEXND    WHERE(HISEXND.USEDATETIME BETWEEN '" + SCDT + "' AND '" + ECDT + "' )) HISEXND,    " +
"     (SELECT * FROM BASORDD    WHERE BASORDD.ENDDATE = '9991231') BASORDD,                                        " +
"     (SELECT * FROM HISBACK    WHERE(HISBACK.CREATEDATETIME BETWEEN '" + SCDT + "' AND '" + ECDT + "' )) HISBACK  " +
"WHERE INACARM.MEDNO = HISMEDD.MEDNO                " +
"AND INACARM.CANCELFLAG = 'N'                       " +
"AND INACARM.MEDNO = HISPROF.MEDNO                  " +
"AND INACARM.VISITSEQ = HISPROF.VISITSEQ            " +
"AND HISPROF.ORDERSORT IN('0', '1')                 " +
"AND HISPROF.MEDNO = HISEXND.MEDNO                  " +
"AND HISPROF.VISITSEQ = HISEXND.VISITSEQ            " +
"AND HISPROF.ORDERNO = HISEXND.ORDERNO              " +
"AND HISEXND.ORDERCODE = BASORDM.ORDERCODE          " +
"AND HISEXND.ORDERCODE = BASORDD.ORDERCODE          " +
"AND HISBACK.MEDNO = HISEXND.MEDNO                  " +
"AND HISBACK.VISITSEQ = HISEXND.VISITSEQ            " +
"AND HISBACK.ORDERNO = HISEXND.ORDERNO              " +
"AND HISBACK.DETAILNO = HISEXND.DETAILNO            " +
"AND HISEXND.STOCKFLAG = 'Y'                        " +
"AND BASORDM.ORDERTYPE BETWEEN 'D000' AND 'DZZZ'    " +
"AND HISBACK.BACKKIND IN('1', '2')").ToList();
                return GetWardBackDrug;
            }
        }



    }
}
