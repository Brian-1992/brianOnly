using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sybase.Data.AseClient;
using System.Configuration;
using MMSMSBAS.Models;
using Dapper;


namespace MMSMSBAS
{
    class BAS
    {
        TESTConn xyz = new TESTConn();
        //public IEnumerable<STKDMITModels> GetSTKDMIT<STKDMITModels>()
        //{
        //   // IEnumerable<T> ls = new IEnumerable<T>();
        //    using (var conn = new AseConnection(connString))
        //    {
        //        //var STKDMIT = conn.SyBaseQuery<STKDMITModels>(
        //        //    "SELECT * FROM dbo.STKDMIT  WHERE[SKORDERCODE] LIKE '005PHE%' ORDER BY[SKORDERCODE] DESC").ToList();
        //        //"SELECT * FROM dbo.STKDMIT ORDER BY[SKORDERCODE] DESC").ToList();
        //        IEnumerable<STKDMITModels> result = conn.SyBaseQuery<STKDMITModels>("SELECT * FROM dbo.STKDMIT  WHERE[SKORDERCODE] LIKE '005PHE%' ORDER BY[SKORDERCODE] DESC").ToList();
        //        //var ls1 = conn.SyBaseQuery<STKDMITModels>(
        //        //   "SELECT * FROM dbo.STKDMIT  WHERE[SKORDERCODE] LIKE '005PHE%' ORDER BY[SKORDERCODE] DESC").ToList();

        //        return result;
        //    }
        //}



        //取得BASORDM-院內碼主檔
        public IEnumerable<T> GetBASORDM<T>()
        {
            
            // IEnumerable<T> ls = new IEnumerable<T>();
            using (var conn = new AseConnection(xyz.heycoon()))
            {
                IEnumerable<T> BASORDM = conn.SyBaseQuery<T>(
                    "SELECT * FROM dbo.BASORDM  WHERE[ORDERCODE] LIKE '0051BD%' ORDER BY[ORDERCODE] DESC").ToList();
                return BASORDM;
            }
        }

        //取得BASORDD-院內碼明細檔
        public IEnumerable<T> GetBASORDD<T>()
        {
            // IEnumerable<T> ls = new IEnumerable<T>();
            using (var conn = new AseConnection(xyz.heycoon()))
            {
                IEnumerable<T> BASORDD = conn.SyBaseQuery<T>("SELECT * FROM dbo.BASORDD  WHERE[ORDERCODE] LIKE '005PHE%' AND [ENDDATE] = '9991231' ORDER BY[ORDERCODE] DESC").ToList();
                return BASORDD;
            }
        }

        //取得STKDMIT藥品基本檔
        public IEnumerable<T> GetBASSTK0<T>()
        {
            // IEnumerable<T> ls = new IEnumerable<T>();
            using (var conn = new AseConnection(xyz.heycoon()))
            {
                IEnumerable<T> STK0 = conn.SyBaseQuery<T>("SELECT * FROM dbo.STKDMIT  WHERE[SKORDERCODE] LIKE '005PHE%' ORDER BY[SKORDERCODE] DESC").ToList();
                return STK0;
            }
        }

        //取得藥學基本檔之方法
        public IList<T> GetBASSTKA<T>()
        {
            using (var conn = new AseConnection(xyz.heycoon()))
            {
                IList<T> BASSTKA = conn.SyBaseQuery<T>("SELECT * FROM dbo.STKDMIT ORDER BY[SKORDERCODE] DESC").ToList();
                return BASSTKA;
            }
        }



        // IEnumerable<T> ls = new IEnumerable<T>();

        //public List<BASORDMModels> GetBASORDM()
        //{
        //    using (var conn = new AseConnection(connString))
        //    {
        //        var BASORDM = conn.SyBaseQuery<BASORDMModels>(
        //            "SELECT * FROM dbo.BASORDM  WHERE[ORDERCODE] LIKE '005PHE%' ORDER BY[ORDERCODE] DESC").ToList();
        //        return BASORDM;
        //    }
        //}

        //public List<BASORDDModels> GetBASORDD()
        //{
        //    using (var conn = new AseConnection(connString))
        //    {
        //        var BASORDD = conn.SyBaseQuery<BASORDDModels>(
        //            "SELECT * FROM dbo.BASORDD  WHERE[ORDERCODE] LIKE '005PHE%' AND [ENDDATE] = '9991231' ORDER BY[ORDERCODE] DESC").ToList();
        //        return BASORDD;
        //    }
        //}
    }
}
