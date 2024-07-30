using System;
using System.Collections.Generic;
using Sybase.Data.AseClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMSMSREPORT.MMReport
{
    public class MMRep_AB0075D
    {
        //連線DB
        TsghConn tsghconn = new TsghConn();
        String sBr = "\r\n";

        //AB0075D介接
        //藥局PH1A、PH1R、PH1C、PHMC
        public IList<T> GetPharmacyWork<T>(string SCDT, string ECDT)
        {
            using (var conn = new AseConnection(tsghconn.TsghDB_IPD()))
            {
                String sql = "";
                sql += "SELECT " + sBr;
                sql += "	SUBSTRING(DRUG.CREATEDATETIME,1,7) WORKDATE," + sBr;
                sql += "	COUNT(DISTINCT DRUG.ORDERNO) CNTORDERNO," + sBr;
                sql += "	COUNT(DISTINCT DRUG.RXNO) CNTRXNO" + sBr;
                sql += "FROM HISDRUG DRUG    " + sBr;
                sql += "WHERE DRUG.PHRPRINTFLAG = 'Y' " + sBr;
                sql += "AND DRUG.ROWSTATECODE <> 'D' " + sBr;
                sql += "AND ORDERCODE >= '000AAA01' " + sBr;
                sql += "AND ORDERCODE <= '009ZZZ99' " + sBr;
                sql += "AND DRUG.CREATEDATETIME >= '" + SCDT + "' AND DRUG.CREATEDATETIME <= '" + ECDT + "'" + sBr;
                sql += "AND DRUG.CANCELFLAG ='N' " + sBr;
                sql += "AND DRUG.DELIVERQTY > 0 " + sBr;
                sql += "GROUP BY SUBSTRING(DRUG.CREATEDATETIME,1,7)" + sBr;
                IList<T> lst = conn.SyBaseQuery<T>(sql).ToList();
                return lst;
            }
        }

    }
}
