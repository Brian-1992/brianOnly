using Sybase.Data.AseClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMSMSREPORT.MMReport
{
    public class MMRep_AB0075C
    {
        //連線DB
        TsghConn tsghconn = new TsghConn();
        String sBr = "\r\n";

        //AB0075C介接
        //TPN
        public IList<T> GetTPNWork<T>(string CDT)
        {
            using (var conn = new AseConnection(tsghconn.TsghDB_IPD()))
            {
                String sql = "";
                sql += "SELECT SUBSTRING(T.VISITDATE,1,7) AS VISITDATE,        " + sBr;
                sql += "       COUNT(  DISTINCT T.MEDNO + CONVERT(CHAR,T.VISITSEQ)  ) AS CNTVISITSEQ," + sBr;
                sql += "       COUNT(H.ORDERNO) AS CNTORDERNO" + sBr;
                sql += "FROM TPNEUCR T, " + sBr;
                sql += "	HISPROF H  " + sBr;
                sql += "WHERE 1=1 " + sBr;
                sql += "AND T.MAKEUPTYPE = '3'" + sBr;
                sql += "AND H.MEDNO = T.MEDNO " + sBr;
                sql += "AND H.VISITSEQ = T.VISITSEQ " + sBr;
                sql += "AND T.ORDERNO = H.PARENTORDERNO  " + sBr;
                sql += "AND T.VISITDATE LIKE '" + CDT + "%' " + sBr;
                sql += "GROUP BY SUBSTRING(T.VISITDATE,1,7)" + sBr;
                IList<T> lst = conn.SyBaseQuery<T>(sql).ToList();
                return lst;
            }
        }

    }
}
