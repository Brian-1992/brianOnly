using Sybase.Data.AseClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMSMSBAS
{
    public class TsghHisBAS
    {
        TsghConn tsghconn = new TsghConn();

        /*大綱*/
        /*
         * ○運作中 GetBASSTKA → 取得庫存基本資料表(STKDMIT)
         * Ｘ暫時取消未介接完成 GetBASSTKB → 取得供應商資料表(STKSUPP)之方法
         * ○運作中 GetBASORDM → 取得TSGH_DB 診療收費項目基本表(BASORDM)之方法
         * ○運作中 GetBASORDD → 取得TSGH_DB 診療收費項目明細檔(BASORDD)之方法
         * Ｘ暫時取消未介接完成 GetBASCODEA → 取得TSGH_DB (code_src)之方法
         * Ｘ暫時取消未介接完成 GetBASCODEB→ 取得TSGH_DB 藥品給藥頻率代碼表(BASFREQ)之方法
         * Ｘ暫時取消未介接完成 GetBASCODEC → 取得TSGH_DB 藥品給藥途徑部位表(BASPATH)之方法
         * ○運作中 GetSTKCTDM → 取得TSGH_DB 各庫藥品維護檔(STKCTDM)之方法
         * ○運作中 GetMEDLOCATION → 取得TSGH_DB 「門急」系統之庫儲欄位之方法
         * ○運作中 GetBASESPC → 取得TSGH_DB 各庫藥品維護檔(BASESPC)之方法
         * ▲需介接待完成 GetPHRDCMG → 取得TSGH_DB 藥物異動通知(PHRDCMG)之方法
         */

        //取得TSGH_DB 庫存基本資料表(STKDMIT)之方法
        public IList<T> GetBASSTKA<T>()
        {
            using (var conn = new AseConnection(tsghconn.TsghDB_IPD()))
            {
                IList<T> BASSTKA = conn.SyBaseQuery<T>(
@"
SELECT * 
FROM dbo.STKDMIT 
ORDER BY SKORDERCODE DESC
").ToList();
                return BASSTKA;
            }
        }


        //取得TSGH_DB 供應商資料表(STKSUPP)之方法
        public IList<T> GetBASSTKB<T>()
        {
            using (var conn = new AseConnection(tsghconn.TsghDB_IPD()))
            {
                IList<T> BASSTKB = conn.SyBaseQuery<T>("SELECT * FROM dbo.STKSUPP ORDER BY [SUPPLYNO] DESC").ToList();
                return BASSTKB;
            }
        }

        //取得TSGH_DB 診療收費項目基本表(BASORDM)之方法
        public IList<T> GetBASORDM<T>()
        {
            using (var conn = new AseConnection(tsghconn.TsghDB_IPD()))
            {
                //1081121 正常SQL 
                //IList<T> BASORDM = conn.SyBaseQuery<T>("SELECT * FROM dbo.BASORDM ORDER BY[ORDERCODE] DESC").ToList();

                //1081122 測試：只抓SKTDMIT有的醫療庫存品項
                IList<T> sql = conn.SyBaseQuery<T>(
@"
SELECT ORDM.*
FROM dbo.BASORDM ORDM,
     dbo.STKDMIT DMIT
WHERE ORDM.SKORDERCODE = DMIT.SKORDERCODE
ORDER BY ORDERCODE DESC
").ToList();
                return sql;
            }
            
        }

        //取得TSGH_DB 診療收費項目明細檔(BASORDD)之方法
        public IList<T> GetBASORDD<T>()
        {
            using (var conn = new AseConnection(tsghconn.TsghDB_IPD()))
            {
                //1081121 正常SQL 
                //IList<T> BASORDD = conn.SyBaseQuery<T>("SELECT * FROM dbo.BASORDD ORDER BY [ORDERCODE] DESC, [BEGINDATE] DESC").ToList();
                
                //1081122 測試：只抓SKTDMIT有的品項
                IList<T> sql = conn.SyBaseQuery<T>(
@"
SELECT ORDD.*
FROM dbo.BASORDD ORDD,
     dbo.STKDMIT DMIT
WHERE ORDD.ORDERCODE = DMIT.SKORDERCODE
ORDER BY ORDERCODE DESC,
         BEGINDATE DESC
").ToList();

                return sql;
            }
        }

        //取得TSGH_DB (code_src)之方法
        public IList<T> GetBASCODEA<T>()
        {
            using (var conn = new AseConnection(tsghconn.TsghDB_IPD()))
            {
                IList<T> BASCODEA = conn.SyBaseQuery<T>("SELECT * FROM dbo.code_src").ToList();
                return BASCODEA;
            }
        }

        //取得TSGH_DB 藥品給藥頻率代碼表(BASFREQ)之方法
        public IList<T> GetBASCODEB<T>()
        {
            using (var conn = new AseConnection(tsghconn.TsghDB_IPD()))
            {
                IList<T> BASCODEB = conn.SyBaseQuery<T>("SELECT * FROM dbo.BASFREQ ORDER BY [FREQNO] DESC").ToList();
                return BASCODEB;
            }
        }

        //取得TSGH_DB 藥品給藥途徑部位表(BASPATH)之方法
        public IList<T> GetBASCODEC<T>()
        {
            using (var conn = new AseConnection(tsghconn.TsghDB_IPD()))
            {
                IList<T> BASCODEC = conn.SyBaseQuery<T>("SELECT * FROM dbo.BASPATH").ToList();
                return BASCODEC;
            }
        }

        //取得TSGH_DB 各庫藥品維護檔(STKCTDM)之方法
        //因公藥維護還是在HIS設定所以介接
        public IList<T> GetSTKCTDM<T>()
        {
            using (var conn = new AseConnection(tsghconn.TsghDB_IPD()))
            {
                IList<T> sql = conn.SyBaseQuery<T>(
@"
SELECT *
FROM dbo.STKCTDM
ORDER BY STOCKCODE DESC,
         SKORDERCODE DESC
").ToList();                
                return sql;
            }
        }

        //取得TSGH_DB 「門急」系統之庫儲欄位之方法
        //1081225因應需求新增此方法
        public IList<T> GetMEDLOCATION<T>()
        {
            using (var conn = new AseConnection(tsghconn.TsghDB_OPD()))
            { 
                IList<T> sql = conn.SyBaseQuery<T>(
@"
SELECT *
FROM taizone.medlocation
").ToList();
                return sql;
            }
        }

        //GetBASESPC取得TSGH_DB 各庫藥品維護檔(BASESPC)之方法
        //ESPTYPE = '0M' 只抓'酒精用藥'類別
        //1090220因應需求新增此方法
        public IList<T> GetBASESPC<T>()
        {
            using (var conn = new AseConnection(tsghconn.TsghDB_IPD()))
            {
                IList<T> sql = conn.SyBaseQuery<T>(
@"
SELECT *
FROM dbo.BASESPC
WHERE ESPTYPE = '0M'
ORDER BY ESPTYPE DESC, 
         SPECIALORDERCODE DESC, 
         INSUFLAG DESC, 
         ESPRECNO DESC
").ToList();
                return sql;
            }
        }

        ///需介接待完成 GetPHRDCMG → 取得TSGH_DB 藥物異動通知(PHRDCMG)之方法
        ///1090306俊翔藥師(組長)要求：藥品狀態
        ///HIS功能編碼是sysm1z1的狀態註記
        ///
        public IList<T> GetPHRDCMG<T>()
        {
            using (var conn = new AseConnection(tsghconn.TsghDB_IPD()))
            {
                IList<T> sql = conn.SyBaseQuery<T>(
@"
SELECT *
FROM dbo.PHRDCMG
ORDER BY ORDERCODE DESC,
         CHANGETYPE DESC,
         CREATEDATETIME DESC
").ToList();
                return sql;
            }
        }

    }
}
