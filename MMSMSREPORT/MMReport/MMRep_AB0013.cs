using Sybase.Data.AseClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMSMSREPORT.MMReport
{
    public class MMRep_AB0013
    {
        //連線DB
        TsghConn tsghconn = new TsghConn();

        /// <summary>
        /// FOR TEST
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IList<T> GetDrugReissue_TEST<T>()
        {
            using (var conn = new AseConnection(tsghconn.TsghDB_IPD()))
            {
                IList<T> ILGetDrugReissue = conn.SyBaseQuery<T>(
"SELECT A.MEDNO,                        " +
"       A.CHARTNO,                      " +
"       A.CHINNAME,                     " +
"       B.VISITSEQ,                     " +
"       B.NRCODE,                       " +
"       B.BEDNO,                        " +
"       C.CREATEDATETIME                " +
"FROM HISMEDD A, INACARM B, HISPROF C   " +
"WHERE A.MEDNO = B.MEDNO                " +
"AND   A.MEDNO = C.MEDNO                " +
"AND   C.ORDERCODE = '005AVE02'         " +
"AND   A.MEDNO = '5100737'              ").ToList();
                return ILGetDrugReissue;
            }
        }

        /// <summary>
        /// 介接藥品補發清單
        /// 藥局收到病房提出需補發藥品，需確認是不是真的有這筆醫令，所需要確認的清單
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ORDERCODE"></param>
        /// <param name="MEDNO"></param>
        /// <returns></returns>
        public IList<T> GetDrugReissue<T>(string ORDERCODE, string MEDNO)
        {
            using (var conn = new AseConnection(tsghconn.TsghDB_IPD()))
            {
                IList<T> ILGetDrugReissue = conn.SyBaseQuery<T>(
"SELECT A.MEDNO,                            " +
"       A.CHARTNO,                          " +
"       A.CHINNAME,                         " +
"       B.VISITSEQ,                         " +
"       B.NRCODE,                           " +
"       B.BEDNO,                            " +
"       C.CREATEDATETIME                    " +
"FROM HISMEDD A, INACARM B, HISPROF C       " +
"WHERE A.MEDNO = B.MEDNO                    " +
"AND   A.MEDNO = C.MEDNO                    " +
"AND   C.ORDERCODE = '" + ORDERCODE + "'    " +
"AND   A.MEDNO = '" + MEDNO + "'            " ).ToList();
                return ILGetDrugReissue;
            }
        }
    }
}
