using Sybase.Data.AseClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMSMSREPORT.MMReport
{
    public class MMRep_AB0075B
    {
        //連線DB
        TsghConn tsghconn = new TsghConn();
        String sBr = "\r\n";

        //AB0075B 介接
        //PCA每月工作量及項目統計
        public IList<T> GetPCAWork<T>(string SCDT, string ECDT)
        {
            using (var conn = new AseConnection(tsghconn.TsghDB_IPD()))
            {
                String sql = "";
                sql += "SELECT SUBSTRING(CREATEDATETIME,1,7) AS SDATE, " + sBr;
                sql += "       COUNT(*) AS CNT ,                     " + sBr;
                sql += "       ORDERCODE,                             " + sBr;
                sql += "       SUM(SUMQTY) AS SUMQTY                   " + sBr;
                sql += "FROM HISPROF " + sBr;
                sql += "WHERE 1=1" + sBr;
                sql += "AND CREATETYPE = 'SD31' " + sBr;
                sql += "AND CREATEDATETIME >= '" + SCDT + "' AND CREATEDATETIME <= '" + ECDT + "' " + sBr;
                sql += "AND CANCELFLAG = 'N' " + sBr;
                sql += "AND ORDERNO = PARENTORDERNO " + sBr;
                sql += "GROUP BY SUBSTRING(CREATEDATETIME,1,7), ORDERCODE " + sBr;
                sql += "ORDER BY SDATE" + sBr;
                IList<T> lst = conn.SyBaseQuery<T>(sql).ToList();
                return lst;
            }
        }

    }
}
