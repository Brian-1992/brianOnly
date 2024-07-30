using Sybase.Data.AseClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMSMSREPORT.MMReport
{
    public class MMRep_AB0075A
    {
        //連線DB
        TsghConn tsghconn = new TsghConn();
        String sBr = "\r\n";

        //AB0075A介接
        //化療工作量依醫師確認日期 
        public IList<T> GetCHEMOWork<T>(string SCDT, string ECDT)
        {
            using (var conn = new AseConnection(tsghconn.TsghDB_IPD()))
            {
                String sql = "";
                sql += "SELECT SUBSTRING(P.PROCDATETIME,1,7) AS 'PORC_DATE'," + sBr;
                sql += "       COUNT(DISTINCT P.MEDNO) AS 'PAT_CNT'," + sBr;
                sql += "       COUNT(*) AS 'TOT_CNT'" + sBr;
                sql += "FROM PHRCHMC P," + sBr;
                sql += "     HISPROF F," + sBr;
                sql += "     PHRCHEM CHEM" + sBr;
                sql += "WHERE 1=1 " + sBr;
                sql += "AND P.PROCDATETIME >='" + SCDT + "' AND P.PROCDATETIME <= '" + ECDT + "' " + sBr;
                sql += "AND P.MAKEUPTYPE = '3'" + sBr;
                sql += "AND CHEM.CHEMOKIND = '1'" + sBr;
                sql += "AND P.MEDNO = F.MEDNO" + sBr;
                sql += "AND P.VISITSEQ = F.VISITSEQ" + sBr;
                sql += "AND P.ORDERNO = F.ORDERNO" + sBr;
                sql += "AND F.ORDERCODE = CHEM.ORDERCODE" + sBr;
                sql += "GROUP BY SUBSTRING(P.PROCDATETIME, 1, 7)" + sBr;
                IList<T> lst = conn.SyBaseQuery<T>(sql).ToList();
                return lst;
            }
        }


    }
}
