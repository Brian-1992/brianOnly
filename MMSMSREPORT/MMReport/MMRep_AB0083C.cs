using Sybase.Data.AseClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMSMSREPORT.MMReport
{
    public class MMRep_AB0083C
    {
        //連線DB
        TsghConn tsghconn = new TsghConn();


        /*
         --03_退藥異常次數及金額統計表
SELECT SUBSTRING(HISBACK.CREATEDATETIME,1,7) AS CREATEDATE,                               --發生日期
       BASNRSM.NRNAME,                                                                    --病房名稱
       BASORDM.ORDERCODE,                                                                 --院內碼
       BASORDM.ORDERENGNAME,                                                              --藥品英文名
       (HISBACK.BACKQTY - HISBACK.NEEDBACKQTY) AS QTY,                                    --數量變動
       (HISBACK.BACKQTY - HISBACK.NEEDBACKQTY)*STKDMIT.WCOSTAMOUNT AS MONEY,              --總金額
        --'日期範圍_FR(YYYMMDD)~日期範圍_TO(YYYMMDD)' AS DATERANG                         
        HISEXND.NRCODE                                                                    --病房代碼
 FROM HISBACK,
      HISEXND,
      BASNRSM,
      BASORDM,
      STKDMIT 
 WHERE 1=1
 AND HISBACK.MEDNO = HISEXND.MEDNO 
 AND HISBACK.VISITSEQ = HISEXND.VISITSEQ 
 AND HISBACK.ORDERNO = HISEXND.ORDERNO 
 AND HISBACK.DETAILNO = HISEXND.DETAILNO 
 AND HISEXND.NRCODE = BASNRSM.NRCODE 
 AND HISEXND.NRCODE IS NOT NULL 
 AND HISBACK.ORDERCODE = BASORDM.ORDERCODE 
 AND HISBACK.ORDERCODE = STKDMIT.SKORDERCODE 
 AND (HISBACK.NEEDBACKQTY - HISBACK.BACKQTY) <> 0 
 AND HISBACK.BACKKIND = '1' 
--  AND HISEXND.NRCODE = '病房別'
--  AND SUBSTRING(HISBACK.CREATEDATETIME,1,7) BETWEEN '日期範圍_FR(YYYMMDD)' AND '日期範圍_TO(YYYMMDD)' 
 AND HISBACK.CREATEDATETIME BETWEEN '日期範圍_FR(YYYMMDDHHMISS)' AND '日期範圍_TO(YYYMMDDHHMISS)' 

         */

        //03_退藥異常次數及金額統計表
        public IList<T> GetDrugBackC<T>(string SCDT, string ECDT)
        {
            using (var conn = new AseConnection(tsghconn.TsghDB_IPD()))
            {
                IList<T> ILGetDrugBack = conn.SyBaseQuery<T>(
@"
 SELECT SUBSTRING(HISBACK.CREATEDATETIME,1,7) AS CREATEDATE,                               
       BASNRSM.NRNAME,                                                                    
       BASORDM.ORDERCODE,                                                                 
       BASORDM.ORDERENGNAME,                                                              
       (HISBACK.BACKQTY - HISBACK.NEEDBACKQTY) AS QTY,                                 
       (HISBACK.BACKQTY - HISBACK.NEEDBACKQTY)*STKDMIT.WCOSTAMOUNT AS MONEY,              
        HISEXND.NRCODE                                                                   
 FROM HISBACK,
      HISEXND,
      BASNRSM,
      BASORDM,
      STKDMIT 
 WHERE 1=1
 AND HISBACK.MEDNO = HISEXND.MEDNO 
 AND HISBACK.VISITSEQ = HISEXND.VISITSEQ 
 AND HISBACK.ORDERNO = HISEXND.ORDERNO 
 AND HISBACK.DETAILNO = HISEXND.DETAILNO 
 AND HISEXND.NRCODE = BASNRSM.NRCODE 
 AND HISEXND.NRCODE IS NOT NULL 
 AND HISBACK.ORDERCODE = BASORDM.ORDERCODE 
 AND HISBACK.ORDERCODE = STKDMIT.SKORDERCODE 
 AND (HISBACK.NEEDBACKQTY - HISBACK.BACKQTY) <> 0 
 AND HISBACK.BACKKIND = '1' 
 AND HISBACK.CREATEDATETIME BETWEEN '" + SCDT + "' AND '" + ECDT + "' ").ToList();
                return ILGetDrugBack;
            }
        }

    }
}
