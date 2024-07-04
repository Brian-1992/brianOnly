using System;
using System.Collections.Generic;
using Sybase.Data.AseClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMSMSREPORT.MMReport
{
    public class MMRep_AB0075E
    {
        //連線DB
        TsghConn tsghconn = new TsghConn();
        String sBr = "\r\n";

        //AB0075E介接
        //
        public IList<T> GetUDWork<T>(string CDT)
        {
            using (var conn = new AseConnection(tsghconn.TsghDB_IPD()))
            {
                String sql = "";
                sql += "SELECT " + sBr;
                sql += "	substring(B.SDATE,1,7) WORKDATE," + sBr;
                sql += "	B.RECNO," + sBr;
                sql += "	COUNT(*) SUMBEDNO               " + sBr;
                sql += "FROM (       " + sBr;
                sql += "	SELECT " + sBr;
                sql += "		CREATEDATETIME AS SDATE," + sBr;
                sql += "		MEDNO," + sBr;
                sql += "		VISITSEQ, " + sBr;
                sql += "		STOCKCODE," + sBr;
                sql += "		COUNT(DISTINCT ORDERNO) AS RECNO  " + sBr;
                sql += "		FROM HISEXND " + sBr;
                sql += "		WHERE PHARMOUTCODE = 'U' " + sBr;
                sql += "		AND CREATEDATETIME LIKE '" + CDT + "%' " + sBr;
                sql += "		AND ORDERCODE >= '005AA' " + sBr;
                sql += "		AND ORDERCODE <= '00999' " + sBr;
                sql += "		AND STOCKFLAG = 'Y' " + sBr;
                sql += "		GROUP BY CREATEDATETIME, MEDNO, VISITSEQ, STOCKCODE" + sBr;
                sql += ") B  " + sBr;
                sql += "GROUP BY B.SDATE , B.RECNO" + sBr;
                IList<T> lst = conn.SyBaseQuery<T>(sql).ToList();
                return lst;
            }
        }

    }
}
