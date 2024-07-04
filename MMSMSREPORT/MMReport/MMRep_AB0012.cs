using Sybase.Data.AseClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMSMSREPORT.MMReport
{
    public class MMRep_AB0012
    {
        //連線DB
        TsghConn tsghconn = new TsghConn();

        /// <summary>
        /// 取得病房公藥(管制藥)消耗量FOR TEST-----0625測試OK
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>        
        public IList<T> GetControlDrug_TEST<T>()
        {
            using (var conn = new AseConnection(tsghconn.TsghDB_IPD()))
            {
                IList<T> ILGetControlDrug = conn.SyBaseQuery<T>(
"SELECT EXND.ORDERNO,                      " +
"       EXND.DETAILNO,                      " +
"       EXND.NRCODE AS NRCODE,                      " +
"       EXND.BEDNO AS BEDNO,                      " +
"       EXND.MEDNO,                      " +
"       MEDD.CHARTNO,                      " + 
"       EXND.VISITSEQ,                      " +
"       EXND.ORDERCODE,                      " +
"       EXND.DOSE,                      " +
"       (SELECT MAX(USRM.CHINNAME)                      " +
"        FROM BASUSRM USRM                      " +
"        WHERE PROF.ORDERDR = USRM.USERID) AS ORDERDR,                      " +
"       EXND.USEDATETIME,                                   " +
"       EXND.CREATEDATETIME ,                               " +
"       (SELECT MAX(USRM.CHINNAME)                      " +
"        FROM BASUSRM USRM                      " +
"        WHERE EXND.SIGNOPID = USRM.USERID) AS SIGNOPID,                      " +
"       EXND.USEQTY,                                        " +
"       PROV.RESTQTY,                                       " +
"      (SELECT MAX(USRM.CHINNAME)                      " +
"       FROM BASUSRM USRM                      " +
"       WHERE PROV.PROVEDR = USRM.USERID) AS PROVEDR,                      " +
"      (SELECT MAX(USRM.CHINNAME)                      " +
"       FROM BASUSRM USRM                      " +
"       WHERE PROV.PROVEID2 = USRM.USERID) AS PROVEID2,                      " +
"      SUBSTRING( SIGNDATETIME, 4, 4)+'-' +                      " +
"               SUBSTRING(SIGNDATETIME, 8, 4) + ' / ' +                      " +
"               SUBSTRING(EXND.CREATEDATETIME, 4, 4) + '-' +                      " +
"               SUBSTRING(EXND.CREATEDATETIME, 8, 4) AS MEMO,                      " +
"         MEDD.CHINNAME,                                       " +
"      ORDM.ORDERENGNAME,                                   " +
"      DMIT.SPECNUNIT,                                     " +
"      EXND.ORDERUNIT,                                     " +
"      ORDM.STOCKUNIT,                             " +
"      CTDM.FLOORQTY,                                      " +
"      (SELECT MAX(USRM.CHINNAME)                      " +
"       FROM BASUSRM USRM                      " +
"       WHERE PROV.PROVEID1 = USRM.USERID) AS PROVEID1,                      " +
"       ORDM.CARRYKINDI,                                   " +
"       ORDD.STOCKTRANSQTYI,                               " +
"       EXND.STOCKCODE ,                                  " +
"       MAX(STTNA.APPLYDATETIME)  AS STARTDATATIME                      " +
"FROM HISEXND EXND                      " +
"  INNER JOIN HISPROF PROF                      " +
"    ON EXND.MEDNO = PROF.MEDNO                      " +
"    AND EXND.VISITSEQ = PROF.VISITSEQ                      " +
"    AND EXND.ORDERNO = PROF.ORDERNO                      " +
"    AND PROF.ORDERSORT IN('0','1')                                  " +
"  INNER JOIN HISMEDD MEDD                      " +
"    ON EXND.MEDNO = MEDD.MEDNO                      " +
"  INNER JOIN STKDMIT DMIT                      " +
"    ON EXND.ORDERCODE = DMIT.SKORDERCODE                      " +
"  INNER JOIN BASORDM ORDM                      " +
"    ON EXND.ORDERCODE = ORDM.ORDERCODE                      " +
"    AND ORDM.RESTRICTCODE BETWEEN '1' AND '4'                      " +
"  INNER JOIN BASORDD ORDD                      " +
"    ON EXND.ORDERCODE = ORDD.ORDERCODE                      " +
"    AND ORDD.BEGINDATE <= '1080610'                      " +
"    AND ORDD.ENDDATE >= '1080610'                      " +
"  INNER JOIN STKCTDM CTDM                      " +
"    ON CTDM.STOCKCODE = EXND.STOCKCODE                      " +
"    AND CTDM.SKORDERCODE = EXND.ORDERCODE                      " +
"  LEFT JOIN HISPROV PROV                      " +
"    ON(EXND.MEDNO = PROV.MEDNO                      " +
"        AND EXND.VISITSEQ = PROV.VISITSEQ                      " +
"        AND EXND.ORDERNO = PROV.ORDERNO                      " +
"        AND EXND.DETAILNO = PROV.DETAILNO                      " +
"        AND PROV.CANCELFLAG <> 'Y')                      " +
"  LEFT JOIN STKSTTN STTNA                      " +
"    ON EXND.ORDERCODE = STTNA.SKORDERCODE                      " +
"    AND STTNA.STOCKCODE = '51'                      " +    
"    AND STTNA.TRANSCONFIRM IN('Y')                                " +
"    AND STTNA.TRANSKIND = '211'                      " +
"WHERE  1 = 1                      " +
"AND EXND.STOCKCODE = '51'                      " +
"AND EXND.STOCKFLAG = 'Y'                      " +
"AND EXND.CREATEDATETIME BETWEEN                      " +
"    (                      " +
"      SELECT MIN(STTNB.APPLYDATETIME)                      " +
"      FROM STKSTTN AS STTNB                      " +
"      WHERE STTNB.SKORDERCODE = ORDM.ORDERCODE                      " +
"      AND STTNB.STOCKCODE = '51'                      " +  
"      AND STTNB.TRANSCONFIRM IN ('Y')                       " +
"      AND STTNB.TRANSKIND = '211'                      " +
"      AND STTNB.APPLYDATETIME BETWEEN '1080610000000' AND '1080610235959'                      " +
"    )                       " +
"    AND '1080610235959'                      " +
"GROUP BY EXND.ORDERNO,                      " +
"         EXND.DETAILNO,                      " +
"         EXND.NRCODE,                       " +
"         EXND.BEDNO,                       " +
"         EXND.MEDNO,                       " +
"         MEDD.CHARTNO,                       " +
"         EXND.VISITSEQ,                       " +
"         EXND.ORDERCODE,                      " +
"         EXND.DOSE,                        " +
"         PROF.ORDERDR,                        " +
"         EXND.USEDATETIME,                       " +
"         EXND.CREATEDATETIME ,                       " +
"         EXND.SIGNDATETIME,                     " +
"         EXND.SIGNOPID,                         " +
"         EXND.USEQTY,                           " +
"         PROV.RESTQTY,                          " +
"         PROV.PROVEDR,                          " +
"         PROV.PROVEID2,                         " +
"         MEDD.CHINNAME,                         " +
"         ORDM.ORDERENGNAME,                     " +
"         DMIT.SPECNUNIT,                        " +
"         EXND.ORDERUNIT,                        " +
"         CTDM.FLOORQTY,                         " +
"         ORDM.STOCKUNIT,                        " +
"         PROV.PROVEID1,                         " +
"         ORDM.CARRYKINDI,                       " +
"         ORDD.STOCKTRANSQTYI,                   " +
"         EXND.STOCKCODE                         " +
"SELECT BACK.ORDERNO,                            " +
"       BACK.DETAILNO,                           " +
"       CARM.NRCODE AS NRCODE,                   " +
"       CARM.BEDNO AS BEDNO,                     " +
"       EXND.MEDNO,                              " +
"       MEDD.CHARTNO,                            " +
"       BACK.VISITSEQ,                           " +
"       BACK.ORDERCODE,                          " +
"       EXND.DOSE,                               " +
"       (SELECT MAX(USRM.CHINNAME)               " +
"        FROM BASUSRM USRM                       " +
"        WHERE PROF.ORDERDR = USRM.USERID) AS ORDERDR,        " +
"       BACK.CREATEDATETIME AS USEDATETIME,      " +
"       BACK.CREATEDATETIME,                     " +
"      (SELECT MAX(USRM.CHINNAME)                " +
"       FROM BASUSRM USRM                        " +
"       WHERE BACK.CREATEOPID = USRM.USERID) AS SIGNOPID,      " +
"       BACK.BACKQTY * (-1) AS USEQTY,           " +
"       0 AS RESTQTY,                            " +
"       '' AS PROVEDR,                           " +
"       '' AS PROVEID2,                          " +
"       '' MEMO,                                 " +
"       MEDD.CHINNAME,                           " +
"       ORDM.ORDERENGNAME,                       " +
"       DMIT.SPECNUNIT,                          " +
"       EXND.ORDERUNIT,                          " +
"       ORDM.STOCKUNIT,                          " +
"       CTDM.FLOORQTY,                           " +
"       '' AS PROVEID1,                          " +
"       ORDM.CARRYKINDI,                         " +
"       ORDD.STOCKTRANSQTYI,                     " +
"       EXND.STOCKCODE,                          " +
"       MAX(STTNA.APPLYDATETIME)  AS STARTDATATIME                      " +
"FROM HISBACK BACK                               " + 
"   INNER JOIN HISEXND EXND                      " +
"      ON BACK.MEDNO = EXND.MEDNO                " +
"      AND BACK.VISITSEQ = EXND.VISITSEQ         " +
"      AND BACK.ORDERNO = EXND.ORDERNO           " +
"      AND BACK.DETAILNO = EXND.DETAILNO         " +
"   INNER JOIN INACARM CARM                      " +
"      ON EXND.MEDNO = CARM.MEDNO                " +
"      AND EXND.VISITSEQ = CARM.VISITSEQ         " +
"      AND CARM.CANCELFLAG <> 'Y'                " +
"   INNER JOIN HISPROF PROF                      " +
"      ON EXND.MEDNO = PROF.MEDNO                " +
"      AND EXND.VISITSEQ = PROF.VISITSEQ         " +
"      AND EXND.ORDERNO = PROF.ORDERNO           " +
"      AND PROF.ORDERSORT IN('0','1')            " +
"   INNER JOIN HISMEDD MEDD                      " +
"      ON EXND.MEDNO = MEDD.MEDNO                " +
"   INNER JOIN STKDMIT DMIT                      " +
"      ON EXND.ORDERCODE = DMIT.SKORDERCODE      " +
"   INNER JOIN BASORDM ORDM                      " +
"      ON EXND.ORDERCODE = ORDM.ORDERCODE        " +
"      AND ORDM.RESTRICTCODE BETWEEN '1' AND '4' " +
"   INNER JOIN BASORDD ORDD                      " +
"      ON EXND.ORDERCODE = ORDD.ORDERCODE        " +
"      AND ORDD.BEGINDATE <= '1080610'           " +
"      AND ORDD.ENDDATE >= '1080610'             " +
"   INNER JOIN STKCTDM CTDM                      " +
"      ON CTDM.STOCKCODE = EXND.STOCKCODE        " +
"      AND CTDM.SKORDERCODE = EXND.ORDERCODE     " +
"   LEFT JOIN STKSTTN STTNA                      " +
"      ON EXND.ORDERCODE = STTNA.SKORDERCODE     " +
"      AND STTNA.STOCKCODE = '51'                " +
"      AND STTNA.TRANSCONFIRM IN('Y')            " +
"      AND STTNA.TRANSKIND = '211'               " +
"WHERE  1 = 1                                    " +
"AND EXND.STOCKCODE = '51'                       " +
"AND EXND.STOCKFLAG = 'Y'                        " +
"AND BACK.CREATEDATETIME BETWEEN                  " + 
"    (                                           " +
"      SELECT MIN(STTNB.APPLYDATETIME)           " +
"      FROM STKSTTN AS STTNB                     " +
"     WHERE STTNB.SKORDERCODE = ORDM.ORDERCODE   " +
"     AND STTNB.STOCKCODE = '51'                 " +
"     AND STTNB.TRANSCONFIRM IN ('Y') AND STTNB.TRANSKIND = '211'            " +
"     AND STTNB.APPLYDATETIME BETWEEN '1080610000000' AND '1080610235959'    " +
"    )                                           " +
"AND '1080610235959'                             " +
"GROUP BY BACK.ORDERNO,                          " +
"         BACK.DETAILNO,                         " +
"         CARM.NRCODE,                           " +
"         CARM.BEDNO,                            " +
"         EXND.MEDNO,                           " + 
"         MEDD.CHARTNO,                         " +
"         BACK.VISITSEQ,                        " +
"         BACK.ORDERCODE,                       " +
"         EXND.DOSE,                            " +
"         PROF.ORDERDR,                         " +
"         BACK.CREATEDATETIME,                  " +
"         BACK.CREATEOPID,                      " +
"         BACK.BACKQTY,                         " +
"         MEDD.CHINNAME,                        " +
"         ORDM.ORDERENGNAME,                    " +
"         DMIT.SPECNUNIT,                       " +
"         EXND.ORDERUNIT,                       " +
"         ORDM.STOCKUNIT,                       " +
"         CTDM.FLOORQTY,                        " +
"         ORDM.CARRYKINDI,                      " +
"         ORDD.STOCKTRANSQTYI,                  " +
"         EXND.STOCKCODE ").ToList();
                return ILGetControlDrug;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="STOCKCODE">庫別代碼</param>
        /// <param name="SCDT">開始時間(yyymmddhhmiss)</param>
        /// <param name="ECDT">結束時間(yyymmddhhmiss)</param>
        /// <returns></returns>
        public IList<T> GetControlDrug<T>(string STOCKCODE, string SCDT, string ECDT)
        {
            //STOCKCODE = "51";
            //SCDT = "1080801000000";
            //ECDT = "1080805235959";

            string SCDT_A = SCDT.Substring(0, 7);
            string SCDT_B = SCDT.Substring(7, 6);
            string ECDT_A = ECDT.Substring(0, 7);
            string ECDT_B = ECDT.Substring(7, 6);
            
            using (var conn = new AseConnection(tsghconn.TsghDB_IPD()))
            {
                IList<T> ILGetControlDrug = conn.SyBaseQuery<T>(
"SELECT EXND.ORDERNO,                                       " +
"       EXND.DETAILNO,                                      " +
"       EXND.NRCODE AS NRCODE,                              " +
"       EXND.BEDNO AS BEDNO,                                " +
"       EXND.MEDNO,                                         " +
"       MEDD.CHARTNO,                                       " +
"       EXND.VISITSEQ,                                      " +
"       EXND.ORDERCODE,                                     " +
"       EXND.DOSE,                                          " +
"       (SELECT MAX(USRM.CHINNAME)                          " +
"        FROM BASUSRM USRM                                  " +
"        WHERE PROF.ORDERDR = USRM.USERID) AS ORDERDR,      " +
"       EXND.USEDATETIME,                                   " +
"       EXND.CREATEDATETIME ,                               " +
"       (SELECT MAX(USRM.CHINNAME)                          " +
"        FROM BASUSRM USRM                                  " +
"        WHERE EXND.SIGNOPID = USRM.USERID) AS SIGNOPID,    " +
"       EXND.USEQTY,                                        " +
"       PROV.RESTQTY,                                       " +
"      (SELECT MAX(USRM.CHINNAME)                           " +
"       FROM BASUSRM USRM                                   " +
"       WHERE PROV.PROVEDR = USRM.USERID) AS PROVEDR,       " +
"      (SELECT MAX(USRM.CHINNAME)                           " +
"       FROM BASUSRM USRM                                   " +
"       WHERE PROV.PROVEID2 = USRM.USERID) AS PROVEID2,     " +
"      SUBSTRING( SIGNDATETIME, 4, 4)+'-' +                 " +
"               SUBSTRING(SIGNDATETIME, 8, 4) + ' / ' +     " +
"               SUBSTRING(EXND.CREATEDATETIME, 4, 4) + '-' +" +
"               SUBSTRING(EXND.CREATEDATETIME, 8, 4) AS MEMO," +
"         MEDD.CHINNAME,                                    " +
"      ORDM.ORDERENGNAME,                                   " +
"      DMIT.SPECNUNIT,                                      " +
"      EXND.ORDERUNIT,                                      " +
"      ORDM.STOCKUNIT,                                      " +
"      CTDM.FLOORQTY,                                       " +
"      (SELECT MAX(USRM.CHINNAME)                           " +
"       FROM BASUSRM USRM                                   " +
"       WHERE PROV.PROVEID1 = USRM.USERID) AS PROVEID1,     " +
"       ORDM.CARRYKINDI,                                    " +
"       ORDD.STOCKTRANSQTYI,                                " +
"       EXND.STOCKCODE ,                                    " +
"       MAX(STTNA.APPLYDATETIME)  AS STARTDATATIME          " +
"FROM HISEXND EXND                                          " +
"  INNER JOIN HISPROF PROF                                  " +
"    ON EXND.MEDNO = PROF.MEDNO                             " +
"    AND EXND.VISITSEQ = PROF.VISITSEQ                      " +
"    AND EXND.ORDERNO = PROF.ORDERNO                        " +
"    AND PROF.ORDERSORT IN('0','1')                         " +
"  INNER JOIN HISMEDD MEDD                                  " +
"    ON EXND.MEDNO = MEDD.MEDNO                             " +
"  INNER JOIN STKDMIT DMIT                                  " +
"    ON EXND.ORDERCODE = DMIT.SKORDERCODE                   " +
"  INNER JOIN BASORDM ORDM                                  " +
"    ON EXND.ORDERCODE = ORDM.ORDERCODE                     " +
"    AND ORDM.RESTRICTCODE BETWEEN '1' AND '4'              " +
"  INNER JOIN BASORDD ORDD                                  " +
"    ON EXND.ORDERCODE = ORDD.ORDERCODE                     " +
"    AND ORDD.BEGINDATE <= '" + SCDT_A + "'                 " +
"    AND ORDD.ENDDATE >= '" + ECDT_A + "'                       " +
"  INNER JOIN STKCTDM CTDM                                  " +
"    ON CTDM.STOCKCODE = EXND.STOCKCODE                     " +
"    AND CTDM.SKORDERCODE = EXND.ORDERCODE                  " +
"  LEFT JOIN HISPROV PROV                                   " +
"    ON(EXND.MEDNO = PROV.MEDNO                             " +
"        AND EXND.VISITSEQ = PROV.VISITSEQ                  " +
"        AND EXND.ORDERNO = PROV.ORDERNO                    " +
"        AND EXND.DETAILNO = PROV.DETAILNO                  " +
"        AND PROV.CANCELFLAG <> 'Y')                        " +
"  LEFT JOIN STKSTTN STTNA                                  " +
"    ON EXND.ORDERCODE = STTNA.SKORDERCODE                  " +
"    AND STTNA.STOCKCODE = '" + STOCKCODE + "'              " +
"    AND STTNA.TRANSCONFIRM IN('Y')                         " +
"    AND STTNA.TRANSKIND = '211'                            " +
"WHERE  1 = 1                                               " +
"AND EXND.STOCKCODE = '" + STOCKCODE + "'                   " +
"AND EXND.STOCKFLAG = 'Y'                                   " +
"AND EXND.CREATEDATETIME BETWEEN                            " +
"    (                                                      " +
"      SELECT MIN(STTNB.APPLYDATETIME)                      " +
"      FROM STKSTTN AS STTNB                                " +
"      WHERE STTNB.SKORDERCODE = ORDM.ORDERCODE             " +
"      AND STTNB.STOCKCODE = '" + STOCKCODE + "'            " +
"      AND STTNB.TRANSCONFIRM IN ('Y')                      " +
"      AND STTNB.TRANSKIND = '211'                          " +
"      AND STTNB.APPLYDATETIME BETWEEN '" + SCDT + "' AND '" + ECDT + "'     " +
"    )                                                      " +
"    AND '" + ECDT + "'                                     " +
"GROUP BY EXND.ORDERNO,                         " +
"         EXND.DETAILNO,                        " +
"         EXND.NRCODE,                          " +
"         EXND.BEDNO,                           " +
"         EXND.MEDNO,                           " +
"         MEDD.CHARTNO,                         " +
"         EXND.VISITSEQ,                        " +
"         EXND.ORDERCODE,                       " +
"         EXND.DOSE,                            " +
"         PROF.ORDERDR,                         " +
"         EXND.USEDATETIME,                     " +
"         EXND.CREATEDATETIME ,                 " +
"         EXND.SIGNDATETIME,                    " +
"         EXND.SIGNOPID,                        " +
"         EXND.USEQTY,                          " +
"         PROV.RESTQTY,                         " +
"         PROV.PROVEDR,                         " +
"         PROV.PROVEID2,                        " +
"         MEDD.CHINNAME,                        " +
"         ORDM.ORDERENGNAME,                    " +
"         DMIT.SPECNUNIT,                       " +
"         EXND.ORDERUNIT,                       " +
"         CTDM.FLOORQTY,                        " +
"         ORDM.STOCKUNIT,                       " +
"         PROV.PROVEID1,                        " +
"         ORDM.CARRYKINDI,                      " +
"         ORDD.STOCKTRANSQTYI,                  " +
"         EXND.STOCKCODE                        " +
"SELECT BACK.ORDERNO,                           " +
"       BACK.DETAILNO,                          " +
"       CARM.NRCODE AS NRCODE,                  " +
"       CARM.BEDNO AS BEDNO,                    " +
"       EXND.MEDNO,                             " +
"       MEDD.CHARTNO,                           " +
"       BACK.VISITSEQ,                          " +
"       BACK.ORDERCODE,                         " +
"       EXND.DOSE,                              " +
"       (SELECT MAX(USRM.CHINNAME)              " +
"        FROM BASUSRM USRM                      " +
"        WHERE PROF.ORDERDR = USRM.USERID) AS ORDERDR,        " +
"       BACK.CREATEDATETIME AS USEDATETIME,     " +
"       BACK.CREATEDATETIME,                    " +
"      (SELECT MAX(USRM.CHINNAME)               " +
"       FROM BASUSRM USRM                       " +
"       WHERE BACK.CREATEOPID = USRM.USERID) AS SIGNOPID,      " +
"       BACK.BACKQTY * (-1) AS USEQTY,          " +
"       0 AS RESTQTY,                           " +
"       '' AS PROVEDR,                          " +
"       '' AS PROVEID2,                         " +
"       '' MEMO,                                " +
"       MEDD.CHINNAME,                          " +
"       ORDM.ORDERENGNAME,                      " +
"       DMIT.SPECNUNIT,                         " +
"       EXND.ORDERUNIT,                         " +
"       ORDM.STOCKUNIT,                         " +
"       CTDM.FLOORQTY,                          " +
"       '' AS PROVEID1,                         " +
"       ORDM.CARRYKINDI,                        " +
"       ORDD.STOCKTRANSQTYI,                    " +
"       EXND.STOCKCODE,                         " +
"       MAX(STTNA.APPLYDATETIME)  AS STARTDATATIME                      " +
"FROM HISBACK BACK                              " +
"   INNER JOIN HISEXND EXND                     " +
"      ON BACK.MEDNO = EXND.MEDNO               " +
"      AND BACK.VISITSEQ = EXND.VISITSEQ        " +
"      AND BACK.ORDERNO = EXND.ORDERNO          " +
"      AND BACK.DETAILNO = EXND.DETAILNO        " +
"   INNER JOIN INACARM CARM                     " +
"      ON EXND.MEDNO = CARM.MEDNO               " +
"      AND EXND.VISITSEQ = CARM.VISITSEQ        " +
"      AND CARM.CANCELFLAG <> 'Y'               " +
"   INNER JOIN HISPROF PROF                     " +
"      ON EXND.MEDNO = PROF.MEDNO               " +
"      AND EXND.VISITSEQ = PROF.VISITSEQ        " +
"      AND EXND.ORDERNO = PROF.ORDERNO          " +
"      AND PROF.ORDERSORT IN('0','1')           " +
"   INNER JOIN HISMEDD MEDD                     " +
"      ON EXND.MEDNO = MEDD.MEDNO               " +
"   INNER JOIN STKDMIT DMIT                     " +
"      ON EXND.ORDERCODE = DMIT.SKORDERCODE     " +
"   INNER JOIN BASORDM ORDM                     " +
"      ON EXND.ORDERCODE = ORDM.ORDERCODE       " +
"      AND ORDM.RESTRICTCODE BETWEEN '1' AND '4'" +
"   INNER JOIN BASORDD ORDD                     " +
"      ON EXND.ORDERCODE = ORDD.ORDERCODE       " +
"      AND ORDD.BEGINDATE <= '" + SCDT_A + "'   " +		//潛在邏輯問題
"      AND ORDD.ENDDATE >= '" + ECDT_A + "'     " +		//潛在邏輯問題
"   INNER JOIN STKCTDM CTDM                     " +
"      ON CTDM.STOCKCODE = EXND.STOCKCODE       " +
"      AND CTDM.SKORDERCODE = EXND.ORDERCODE    " +
"   LEFT JOIN STKSTTN STTNA                     " +
"      ON EXND.ORDERCODE = STTNA.SKORDERCODE    " +
"      AND STTNA.STOCKCODE = '" + STOCKCODE + "'" +
"      AND STTNA.TRANSCONFIRM IN('Y')           " +
"      AND STTNA.TRANSKIND = '211'              " +
"WHERE  1 = 1                                   " +
"AND EXND.STOCKCODE = '" + STOCKCODE + "'       " +
"AND EXND.STOCKFLAG = 'Y'                       " +
"AND BACK.CREATEDATETIME BETWEEN                " +
"    (                                          " +
"      SELECT MIN(STTNB.APPLYDATETIME)          " +
"      FROM STKSTTN AS STTNB                    " +
"     WHERE STTNB.SKORDERCODE = ORDM.ORDERCODE  " +
"     AND STTNB.STOCKCODE = '" + STOCKCODE + "' " +
"     AND STTNB.TRANSCONFIRM IN ('Y') AND STTNB.TRANSKIND = '211'            " +
"     AND STTNB.APPLYDATETIME BETWEEN '" + SCDT + "' AND '" + ECDT + "'    " +
"    )                                          " +
"AND '" + ECDT + "'                             " +
"GROUP BY BACK.ORDERNO,                         " +
"         BACK.DETAILNO,                        " +
"         CARM.NRCODE,                          " +
"         CARM.BEDNO,                           " +
"         EXND.MEDNO,                           " +
"         MEDD.CHARTNO,                         " +
"         BACK.VISITSEQ,                        " +
"         BACK.ORDERCODE,                       " +
"         EXND.DOSE,                            " +
"         PROF.ORDERDR,                         " +
"         BACK.CREATEDATETIME,                  " +
"         BACK.CREATEOPID,                      " +
"         BACK.BACKQTY,                         " +
"         MEDD.CHINNAME,                        " +
"         ORDM.ORDERENGNAME,                    " +
"         DMIT.SPECNUNIT,                       " +
"         EXND.ORDERUNIT,                       " +
"         ORDM.STOCKUNIT,                       " +
"         CTDM.FLOORQTY,                        " +
"         ORDM.CARRYKINDI,                      " +
"         ORDD.STOCKTRANSQTYI,                  " +
"         EXND.STOCKCODE ").ToList();
                return ILGetControlDrug;
            }
        }

    }
}
