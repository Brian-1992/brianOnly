using Sybase.Data.AseClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMSMSREPORT.MMReport
{
    public class MMRep_AB0071
    {
        //連線DB
        TsghConn tsghconn = new TsghConn();
        public IList<T> GetBuyBackDrug_TEST<T>()
        {
            using (var conn = new AseConnection(tsghconn.TsghDB_IPD()))
            {
                IList<T> BuyBackDrug = conn.SyBaseQuery<T>(
"SELECT DISTINCT        " +
"  HISMEDD.CHARTNO,     " +
"  ENXDBACK.MEDNO,      " +
"  ENXDBACK.VISITSEQ,   " +
"  ENXDBACK.ORDERNO,    " +
"  ENXDBACK.DETAILNO,   " +
"  ENXDBACK.ORDERCODE,  " +
"  ENXDBACK.USEQTY,     " +
"  USRM_ORDER.CHINNAME, " +
"  ENXDBACK.SIGNOPID,   " +
"  ENXDBACK.CREATEDATETIME, " +
"  ENXDBACK.STOCKCODE,  " +
"  ENXDBACK.INOUTFLAG,  " +
"  ENXDBACK.ORDERENGNAME,   " +
"  ENXDBACK.RESTRICTCODE,   " +
"  ENXDBACK.HIGHPRICEFLAG,  " +
"  ENXDBACK.NRCODE,     " +
"  ENXDBACK.BEDNO,      " +
"  (ENXDBACK.NRCODE + '-' + ENXDBACK.BEDNO) AS NRCODENAME,  " +
"  ENXDBACK.DOSE,       " +
"  ENXDBACK.ORDERUNIT,  " +
"  ENXDBACK.PATHNO,     " +
"  ENXDBACK.FREQNO,     " +
"  ENXDBACK.PAYFLAG,    " +
"  ENXDBACK.BUYFLAG,    " +
"  ENXDBACK.BAGSEQNO,   " +
"  PROF.RXNO            " +
"FROM(SELECT            " +
"  EXND.MEDNO,          " +
"  EXND.VISITSEQ,       " +
"  EXND.ORDERNO,        " +
"  EXND.DETAILNO,       " +
"  EXND.ORDERCODE,      " +
"  EXND.USEQTY,         " +
"  EXND.ORDERDR,        " +
"  EXND.SIGNOPID,       " +
"  EXND.CREATEDATETIME, " +
"  EXND.STOCKCODE,      " +
"  'A' INOUTFLAG,       " +
"  ORDM.ORDERENGNAME,   " +
"  ORDM.RESTRICTCODE,   " +
"  ORDM.HIGHPRICEFLAG,  " +
"  EXND.NRCODE,         " +
"  EXND.BEDNO,          " +
"  EXND.DOSE,           " +
"  EXND.ORDERUNIT,      " +
"  EXND.PATHNO,         " +
"  EXND.FREQNO,         " +
"  EXND.PAYFLAG,        " +
"  EXND.BUYFLAG,        " +
"  0 BAGSEQNO           " +
"FROM HISEXND EXND,     " +
"     BASORDM ORDM      " +
"WHERE EXND.STOCKFLAG = 'Y' " +
"AND EXND.CREATEDATETIME BETWEEN '1080325000000' AND '1080325235959'    " +
"AND EXND.ORDERCODE = ORDM.ORDERCODE            " +
"AND ORDM.ORDERTYPE BETWEEN 'D000' AND 'DZZZ'   " +
"AND EXND.USEQTY <> 0   " +
"UNION ALL              " +
"SELECT                 " +
"  BACK.MEDNO,          " +
"  BACK.VISITSEQ,       " +
"  BACK.ORDERNO,        " +
"  BACK.DETAILNO,       " +
"  BACK.ORDERCODE,      " +
"  BACK.BACKQTY USEQTY, " +
"  EXND.ORDERDR,        " +
"  BACK.CREATEOPID SIGNOPID,        " +
"  BACK.CREATEDATETIME,             " +
"  BACK.RETURNSTOCKCODE STOCKCODE,  " +
"  'D' INOUTFLAG,       " +
"  ORDM.ORDERENGNAME,   " +
"  ORDM.RESTRICTCODE,   " +
"  ORDM.HIGHPRICEFLAG,  " +
"  EXND.NRCODE,         " +
"  EXND.BEDNO,          " +
"  EXND.DOSE,           " +
"  EXND.ORDERUNIT,      " +
"  EXND.PATHNO,         " +
"  EXND.FREQNO,         " +
"  EXND.PAYFLAG,        " +
"  EXND.BUYFLAG,        " +
"  0 BAGSEQNO           " +
"FROM HISBACK BACK,     " +
"     HISEXND EXND,     " +
"     BASORDM ORDM      " +
"WHERE EXND.STOCKFLAG = 'Y' " +
"AND BACK.CREATEDATETIME BETWEEN '1080325000000' AND '1080325235959'    " +
"AND EXND.MEDNO = BACK.MEDNO                    " +
"AND EXND.VISITSEQ = BACK.VISITSEQ              " +
"AND EXND.ORDERNO = BACK.ORDERNO                " +
"AND EXND.DETAILNO = BACK.DETAILNO              " +
"AND EXND.ORDERCODE = ORDM.ORDERCODE            " +
"AND BACK.BACKKIND IN('1', '2')                 " + 
"AND ORDM.ORDERTYPE BETWEEN 'D000' AND 'DZZZ'   " +
"UNION ALL                                      " +
"SELECT                                         " +
"  ORDO.MEDNO,                                  " +
"  0 VISITSEQ,                                  " +
"  0 ORDERNO,                                   " +
"  0 DETAILNO,                                  " +
"  ORDO.ORDERCODE,                              " +
"  ABS(ORDO.SUMQTY) USEQTY,                     " +
"  ORDO.ORDERDR,                                " +
"  '' SIGNOPID,                                 " +
"  ORDO.WORKDATE || ORDO.WORKTIME CREATEDATETIME,   " +
"  ORDO.STOCKCODE,      " +
"  'A' INOUTFLAG,       " +
"  ORDM.ORDERENGNAME,   " +
"  ORDM.RESTRICTCODE,   " +
"  ORDM.HIGHPRICEFLAG,  " +
"  '' NRCODE,           " +
"  '' BEDNO,            " +
"  ORDO.DOSE,           " +
"  ORDO.ORDERUNIT,      " +
"  ORDO.PATHNO,         " +
"  ORDO.FREQNO,         " +
"  ORDO.PAYFLAG,        " +
"  ' ' AS BUYFLAG,      " +
"  ORDO.BAGSEQNO        " +
"FROM BASORDM ORDM,     " +
"     XMYORDO ORDO      " +
"     LEFT JOIN XMYOPDM OPDM                        " +
"       ON OPDM.VISITDATE = ORDO.VISITDATE          " +
"       AND OPDM.RID = ORDO.RID                     " +
"       AND OPDM.MEDNO = ORDO.MEDNO                 " +
"WHERE 1 = 1                                        " +
"AND ORDO.ORDERCODE = ORDM.ORDERCODE                " +
"AND ORDM.ORDERTYPE BETWEEN 'D000' AND 'DZZZ'       " +
"AND ORDO.WORKDATE BETWEEN '1080325' AND '1080325'  " +
"AND ORDO.WORKTIME BETWEEN '000000' AND '235959'    " +
"UNION ALL                  " +
"SELECT                     " +
"  ORDO.MEDNO,              " +
"  0 VISITSEQ,              " +
"  0 ORDERNO,               " +
"  0 DETAILNO,              " +
"  ORDO.ORDERCODE,          " +
"  ABS(ORDO.SUMQTY) USEQTY, " +
"  ORDO.ORDERDR,            " +
"  '' SIGNOPID,             " +
"  ORDO.MODIFYDATE || ORDO.MODIFYTIME CREATEDATETIME," +
"  ORDO.STOCKCODE,          " +
"  'D' INOUTFLAG,           " +
"  ORDM.ORDERENGNAME,       " +
"  ORDM.RESTRICTCODE,       " +
"  ORDM.HIGHPRICEFLAG,      " +
"  '' NRCODE,               " +
"  '' BEDNO,                " +
"  ORDO.DOSE,               " +
"  ORDO.ORDERUNIT,          " +
"  ORDO.PATHNO,             " +
"  ORDO.FREQNO,             " +
"  ORDO.PAYFLAG,            " +
"  ' ' AS BUYFLAG,          " +
"  ORDO.BAGSEQNO            " +
"FROM BASORDM ORDM,         " +
"     XMYORDO ORDO          " +
"     LEFT JOIN XMYOPDM OPDM" +
"       ON OPDM.VISITDATE = ORDO.VISITDATE  " +
"       AND OPDM.RID = ORDO.RID             " +
"       AND OPDM.MEDNO = ORDO.MEDNO         " +
"WHERE 1 = 1                " +
"AND ORDO.ORDERCODE = ORDM.ORDERCODE        " +
"AND ORDO.CANCELFLAG = 'Y'  " +
"AND ORDM.ORDERTYPE BETWEEN 'D000' AND 'DZZZ'                   " +
"AND ORDO.MODIFYDATE BETWEEN '1080325' AND '1080325'            " +
"AND ORDO.MODIFYTIME BETWEEN '000000' AND '235959') ENXDBACK    " +
"LEFT JOIN BASUSRM USRM_ORDER                                   " +
"  ON USRM_ORDER.USERID = ENXDBACK.ORDERDR                      " +
"LEFT JOIN HISMEDD                                              " +
"  ON HISMEDD.MEDNO = ENXDBACK.MEDNO                            " +
"LEFT JOIN HISPROF PROF                                         " +
"  ON PROF.MEDNO = ENXDBACK.MEDNO                               " +
"  AND PROF.VISITSEQ = ENXDBACK.VISITSEQ                        " +
"  AND PROF.ORDERNO = ENXDBACK.ORDERNO ").ToList();
                return BuyBackDrug;
            }
        }
        public IList<T> GetBuyBackDrug<T>(string SCDT, string ECDT)
        {
            string SCDT_A = SCDT.Substring(0, 7);
            string SCDT_B = SCDT.Substring(7, 6);
            string ECDT_A = ECDT.Substring(0, 7);
            string ECDT_B = ECDT.Substring(7, 6);

            using (var conn = new AseConnection(tsghconn.TsghDB_IPD()))
            {
                IList<T> BuyBackDrug = conn.SyBaseQuery<T>(
"SELECT DISTINCT        " +
"  HISMEDD.CHARTNO,     " +
"  ENXDBACK.MEDNO,      " +
"  ENXDBACK.VISITSEQ,   " +
"  ENXDBACK.ORDERNO,    " +
"  ENXDBACK.DETAILNO,   " +
"  ENXDBACK.ORDERCODE,  " +
"  ENXDBACK.USEQTY,     " +
"  USRM_ORDER.CHINNAME, " +
"  ENXDBACK.SIGNOPID,   " +
"  ENXDBACK.CREATEDATETIME, " +
"  ENXDBACK.STOCKCODE,  " +
"  ENXDBACK.INOUTFLAG,  " +
"  ENXDBACK.ORDERENGNAME,   " +
"  ENXDBACK.RESTRICTCODE,   " +
"  ENXDBACK.HIGHPRICEFLAG,  " +
"  ENXDBACK.NRCODE,     " +
"  ENXDBACK.BEDNO,      " +
"  (ENXDBACK.NRCODE + '-' + ENXDBACK.BEDNO) AS NRCODENAME,  " +
"  ENXDBACK.DOSE,       " +
"  ENXDBACK.ORDERUNIT,  " +
"  ENXDBACK.PATHNO,     " +
"  ENXDBACK.FREQNO,     " +
"  ENXDBACK.PAYFLAG,    " +
"  ENXDBACK.BUYFLAG,    " +
"  ENXDBACK.BAGSEQNO,   " +
"  PROF.RXNO            " +
"FROM(SELECT            " +
"  EXND.MEDNO,          " +
"  EXND.VISITSEQ,       " +
"  EXND.ORDERNO,        " +
"  EXND.DETAILNO,       " +
"  EXND.ORDERCODE,      " +
"  EXND.USEQTY,         " +
"  EXND.ORDERDR,        " +
"  EXND.SIGNOPID,       " +
"  EXND.CREATEDATETIME, " +
"  EXND.STOCKCODE,      " +
"  'A' INOUTFLAG,       " +
"  ORDM.ORDERENGNAME,   " +
"  ORDM.RESTRICTCODE,   " +
"  ORDM.HIGHPRICEFLAG,  " +
"  EXND.NRCODE,         " +
"  EXND.BEDNO,          " +
"  EXND.DOSE,           " +
"  EXND.ORDERUNIT,      " +
"  EXND.PATHNO,         " +
"  EXND.FREQNO,         " +
"  EXND.PAYFLAG,        " +
"  EXND.BUYFLAG,        " +
"  0 BAGSEQNO           " +
"FROM HISEXND EXND,     " +
"     BASORDM ORDM      " +
"WHERE EXND.STOCKFLAG = 'Y' " +
"AND EXND.CREATEDATETIME BETWEEN '" + SCDT + "' AND '" + ECDT + "'    " +
"AND EXND.ORDERCODE = ORDM.ORDERCODE            " +
"AND ORDM.ORDERTYPE BETWEEN 'D000' AND 'DZZZ'   " +
"AND EXND.USEQTY <> 0   " +
"UNION ALL              " +
"SELECT                 " +
"  BACK.MEDNO,          " +
"  BACK.VISITSEQ,       " +
"  BACK.ORDERNO,        " +
"  BACK.DETAILNO,       " +
"  BACK.ORDERCODE,      " +
"  BACK.BACKQTY USEQTY, " +
"  EXND.ORDERDR,        " +
"  BACK.CREATEOPID SIGNOPID,        " +
"  BACK.CREATEDATETIME,             " +
"  BACK.RETURNSTOCKCODE STOCKCODE,  " +
"  'D' INOUTFLAG,       " +
"  ORDM.ORDERENGNAME,   " +
"  ORDM.RESTRICTCODE,   " +
"  ORDM.HIGHPRICEFLAG,  " +
"  EXND.NRCODE,         " +
"  EXND.BEDNO,          " +
"  EXND.DOSE,           " +
"  EXND.ORDERUNIT,      " +
"  EXND.PATHNO,         " +
"  EXND.FREQNO,         " +
"  EXND.PAYFLAG,        " +
"  EXND.BUYFLAG,        " +
"  0 BAGSEQNO           " +
"FROM HISBACK BACK,     " +
"     HISEXND EXND,     " +
"     BASORDM ORDM      " +
"WHERE EXND.STOCKFLAG = 'Y' " +
"AND BACK.CREATEDATETIME BETWEEN '" + SCDT + "' AND '" + ECDT + "'    " +
"AND EXND.MEDNO = BACK.MEDNO                    " +
"AND EXND.VISITSEQ = BACK.VISITSEQ              " +
"AND EXND.ORDERNO = BACK.ORDERNO                " +
"AND EXND.DETAILNO = BACK.DETAILNO              " +
"AND EXND.ORDERCODE = ORDM.ORDERCODE            " +
"AND BACK.BACKKIND IN('1', '2')                 " +
"AND ORDM.ORDERTYPE BETWEEN 'D000' AND 'DZZZ'   " +
"UNION ALL                                      " +
"SELECT                                         " +
"  ORDO.MEDNO,                                  " +
"  0 VISITSEQ,                                  " +
"  0 ORDERNO,                                   " +
"  0 DETAILNO,                                  " +
"  ORDO.ORDERCODE,                              " +
"  ABS(ORDO.SUMQTY) USEQTY,                     " +
"  ORDO.ORDERDR,                                " +
"  '' SIGNOPID,                                 " +
"  ORDO.WORKDATE || ORDO.WORKTIME CREATEDATETIME,   " +
"  ORDO.STOCKCODE,      " +
"  'A' INOUTFLAG,       " +
"  ORDM.ORDERENGNAME,   " +
"  ORDM.RESTRICTCODE,   " +
"  ORDM.HIGHPRICEFLAG,  " +
"  '' NRCODE,           " +
"  '' BEDNO,            " +
"  ORDO.DOSE,           " +
"  ORDO.ORDERUNIT,      " +
"  ORDO.PATHNO,         " +
"  ORDO.FREQNO,         " +
"  ORDO.PAYFLAG,        " +
"  ' ' AS BUYFLAG,      " +
"  ORDO.BAGSEQNO        " +
"FROM BASORDM ORDM,     " +
"     XMYORDO ORDO      " +
"     LEFT JOIN XMYOPDM OPDM                        " +
"       ON OPDM.VISITDATE = ORDO.VISITDATE          " +
"       AND OPDM.RID = ORDO.RID                     " +
"       AND OPDM.MEDNO = ORDO.MEDNO                 " +
"WHERE 1 = 1                                        " +
"AND ORDO.ORDERCODE = ORDM.ORDERCODE                " +
"AND ORDM.ORDERTYPE BETWEEN 'D000' AND 'DZZZ'       " +
"AND ORDO.WORKDATE BETWEEN '" + SCDT_A + "' AND '" + ECDT_A + "'  " +
"AND ORDO.WORKTIME BETWEEN '" + SCDT_B + "' AND '" + ECDT_B + "'  " +
"UNION ALL                  " +
"SELECT                     " +
"  ORDO.MEDNO,              " +
"  0 VISITSEQ,              " +
"  0 ORDERNO,               " +
"  0 DETAILNO,              " +
"  ORDO.ORDERCODE,          " +
"  ABS(ORDO.SUMQTY) USEQTY, " +
"  ORDO.ORDERDR,            " +
"  '' SIGNOPID,             " +
"  ORDO.MODIFYDATE || ORDO.MODIFYTIME CREATEDATETIME," +
"  ORDO.STOCKCODE,          " +
"  'D' INOUTFLAG,           " +
"  ORDM.ORDERENGNAME,       " +
"  ORDM.RESTRICTCODE,       " +
"  ORDM.HIGHPRICEFLAG,      " +
"  '' NRCODE,               " +
"  '' BEDNO,                " +
"  ORDO.DOSE,               " +
"  ORDO.ORDERUNIT,          " +
"  ORDO.PATHNO,             " +
"  ORDO.FREQNO,             " +
"  ORDO.PAYFLAG,            " +
"  ' ' AS BUYFLAG,          " +
"  ORDO.BAGSEQNO            " +
"FROM BASORDM ORDM,         " +
"     XMYORDO ORDO          " +
"     LEFT JOIN XMYOPDM OPDM" +
"       ON OPDM.VISITDATE = ORDO.VISITDATE  " +
"       AND OPDM.RID = ORDO.RID             " +
"       AND OPDM.MEDNO = ORDO.MEDNO         " +
"WHERE 1 = 1                " +
"AND ORDO.ORDERCODE = ORDM.ORDERCODE        " +
"AND ORDO.CANCELFLAG = 'Y'  " +
"AND ORDM.ORDERTYPE BETWEEN 'D000' AND 'DZZZ'                   " +
"AND ORDO.MODIFYDATE BETWEEN '" + SCDT_A + "' AND '" + ECDT_A + "'            " +
"AND ORDO.MODIFYTIME BETWEEN '" + SCDT_B + "' AND '" + ECDT_B + "') ENXDBACK  " +
"LEFT JOIN BASUSRM USRM_ORDER                                   " +
"  ON USRM_ORDER.USERID = ENXDBACK.ORDERDR                      " +
"LEFT JOIN HISMEDD                                              " +
"  ON HISMEDD.MEDNO = ENXDBACK.MEDNO                            " +
"LEFT JOIN HISPROF PROF                                         " +
"  ON PROF.MEDNO = ENXDBACK.MEDNO                               " +
"  AND PROF.VISITSEQ = ENXDBACK.VISITSEQ                        " +
"  AND PROF.ORDERNO = ENXDBACK.ORDERNO ").ToList();
                return BuyBackDrug;
            }
        }
    }
}
