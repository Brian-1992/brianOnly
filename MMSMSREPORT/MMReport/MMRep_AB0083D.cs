using Sybase.Data.AseClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMSMSREPORT.MMReport
{
    public class MMRep_AB0083D
    {
        //連線DB
        TsghConn tsghconn = new TsghConn();


        /*
         --04_退藥異常總表
SELECT SUBSTRING(HISBACK.CREATEDATETIME,1,7) AS CREATEDATE,                              --發生日期  
       BASNRSM.NRNAME,                                                                   --病房名稱  
       BASORDM.ORDERCODE,                                                                --院內碼    
       BASORDM.ORDERENGNAME,                                                             --藥品英文名
       (HISBACK.BACKQTY - HISBACK.NEEDBACKQTY) AS QTY,                                   --數量變動  
       (HISBACK.BACKQTY - HISBACK.NEEDBACKQTY)*STKDMIT.WCOSTAMOUNT AS MONEY,             --總金額    
       CASE WHEN BASCODE.HISSYSCODENAME IS NULL                                                      
         THEN ' '                                                                            
         ELSE BASCODE.HISSYSCODENAME                                                     
       END AS HISSYSCODENAME,                                                            --異常原因
       HISEXND.NRCODE                                                                    --病房代碼
 FROM HISBACK 
      INNER JOIN HISEXND 
            ON  HISBACK.MEDNO = HISEXND.MEDNO 
            AND HISBACK.VISITSEQ = HISEXND.VISITSEQ 
            AND HISBACK.ORDERNO = HISEXND.ORDERNO 
            AND HISBACK.DETAILNO = HISEXND.DETAILNO 
      INNER JOIN BASNRSM 
            ON HISEXND.NRCODE = BASNRSM.NRCODE 
            AND HISEXND.NRCODE IS NOT NULL 
      INNER JOIN BASORDM 
            ON HISBACK.ORDERCODE = BASORDM.ORDERCODE 
      INNER JOIN STKDMIT 
            ON HISBACK.ORDERCODE = STKDMIT.SKORDERCODE 
      LEFT OUTER JOIN BASCODE 
            ON HISBACK.PHRBACKREASON = BASCODE.HISSYSCODE 
            AND BASCODE.SYSTEMID = 'PHRPRJ' 
            AND BASCODE.SYSCODETYPE = '14' 
 WHERE 1=1 
 AND (HISBACK.NEEDBACKQTY - HISBACK.BACKQTY) <> 0 
 AND HISBACK.BACKKIND = '1' 
--  AND HISEXND.NRCODE = '病房別'
--  AND SUBSTRING(HISBACK.CREATEDATETIME,1,7) BETWEEN '日期範圍_FR(YYYMMDD)' AND '日期範圍_TO(YYYMMDD)' 
 AND HISBACK.CREATEDATETIME BETWEEN '日期範圍_FR(YYYMMDDHHMISS)' AND '日期範圍_TO(YYYMMDDHHMISS)' 
         */


        //04_退藥異常總表
        public IList<T> GetDrugBackD<T>(string SCDT, string ECDT)
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
       CASE WHEN BASCODE.HISSYSCODENAME IS NULL                                                      
         THEN ' '                                                     
         ELSE BASCODE.HISSYSCODENAME                                                     
       END AS HISSYSCODENAME,                                                           
       HISEXND.NRCODE                                                                    
 FROM HISBACK 
      INNER JOIN HISEXND 
            ON  HISBACK.MEDNO = HISEXND.MEDNO 
            AND HISBACK.VISITSEQ = HISEXND.VISITSEQ 
            AND HISBACK.ORDERNO = HISEXND.ORDERNO 
            AND HISBACK.DETAILNO = HISEXND.DETAILNO 
      INNER JOIN BASNRSM 
            ON HISEXND.NRCODE = BASNRSM.NRCODE 
            AND HISEXND.NRCODE IS NOT NULL 
      INNER JOIN BASORDM 
            ON HISBACK.ORDERCODE = BASORDM.ORDERCODE 
      INNER JOIN STKDMIT 
            ON HISBACK.ORDERCODE = STKDMIT.SKORDERCODE 
      LEFT OUTER JOIN BASCODE 
            ON HISBACK.PHRBACKREASON = BASCODE.HISSYSCODE 
            AND BASCODE.SYSTEMID = 'PHRPRJ' 
            AND BASCODE.SYSCODETYPE = '14' 
 WHERE 1=1 
 AND (HISBACK.NEEDBACKQTY - HISBACK.BACKQTY) <> 0 
 AND HISBACK.BACKKIND = '1' 
 AND HISBACK.CREATEDATETIME BETWEEN '" + SCDT + "' AND '" + ECDT + "' ").ToList();
                return ILGetDrugBack;
            }
        }

    }
}
