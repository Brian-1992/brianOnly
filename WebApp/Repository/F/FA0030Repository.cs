using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models.F;
using System.Collections.Generic;


namespace WebApp.Repository.F
{
    public class FA0030Repository : JCLib.Mvc.BaseRepository
    {
        public FA0030Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        //查詢
        public IEnumerable<FA0030> GetAll(string DATA_YM, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"  SELECT C.APPDEPT,
                                  (SELECT URID.INID_NAME
                                   FROM UR_INID URID
                                   WHERE URID.INID = C.APPDEPT) APPDEPT_NAME,
                                  TO_CHAR(NVL (SUM (C.APV_VOLUME), 0),'FM9999999990.00') SUM_APV_VOLUME
                           FROM (SELECT MDM.APPDEPT,
                                        (SELECT   MMT.M_VOLL * MMT.M_VOLW * MMT.M_VOLH * MMT.M_VOLC / MMT.M_SWAP * (SELECT MWN.APL_INQTY
                                                                                                                    FROM MI_WINVMON MWN
                                                                                                                    WHERE  MWN.DATA_YM = :DATA_YM
                                                                                                                    AND    MWN.WH_NO = MDM.APPDEPT
                                                                                                                    AND    MWN.MMCODE = MDD.MMCODE)
                                         FROM MI_MAST MMT
                                         WHERE  MMT.MMCODE = MDD.MMCODE
                                         AND    MMT.M_SWAP IS NOT NULL
                                         AND    MMT.M_SWAP <> 0) APV_VOLUME
                                 FROM ME_DOCM MDM, ME_DOCD MDD
                                 WHERE MDM.DOCNO = MDD.DOCNO
                                 AND   SUBSTR (TWN_DATE (MDD.APVTIME), 1, 5) = :DATA_YM
                                 AND   (SELECT MMT.MAT_CLASS
                                        FROM MI_MAST MMT
                                        WHERE MMT.MMCODE = MDD.MMCODE) IN ('02',
                                                                           '03',
                                                                           '04',
                                                                           '05',
                                                                           '06')
                                 AND (SELECT MMT.M_STOREID
                                      FROM MI_MAST MMT
                                      WHERE MMT.MMCODE = MDD.MMCODE) = '1') C
                           GROUP BY C.APPDEPT
                           ORDER BY C.APPDEPT";

            p.Add(":DATA_YM", string.Format("{0}", DATA_YM));

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<FA0030>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //取得使用者單位
        public string GetDept(string USER_NAME)
        {
            DynamicParameters p = new DynamicParameters();
            var sql = @"SELECT INID_NAME AS USER_DEPTNAME
                        FROM UR_INID
                        WHERE INID = (SELECT INID
                                      FROM UR_ID
                                      WHERE TUSER = :USER_NAME)";

            return DBWork.Connection.QueryFirst<string>(sql, new { USER_NAME = USER_NAME }, DBWork.Transaction).ToString();
        }

        //匯出
        public DataTable GetExcel(string DATA_YM)
        {
            DynamicParameters p = new DynamicParameters();

            var sql = @"  SELECT C.APPDEPT 成本碼,
                                  (SELECT URID.INID_NAME
                                   FROM UR_INID URID
                                   WHERE URID.INID = C.APPDEPT) 單位名稱,
                                  TO_CHAR(NVL (SUM (C.APV_VOLUME), 0),'FM9999999990.00') 總材積
                           FROM (SELECT MDM.APPDEPT,
                                        (SELECT   MMT.M_VOLL * MMT.M_VOLW * MMT.M_VOLH * MMT.M_VOLC / MMT.M_SWAP * (SELECT MWN.APL_INQTY
                                                                                                                    FROM MI_WINVMON MWN
                                                                                                                    WHERE  MWN.DATA_YM = :DATA_YM
                                                                                                                    AND    MWN.WH_NO = MDM.APPDEPT
                                                                                                                    AND    MWN.MMCODE = MDD.MMCODE)
                                         FROM MI_MAST MMT
                                         WHERE  MMT.MMCODE = MDD.MMCODE
                                         AND    MMT.M_SWAP IS NOT NULL
                                         AND    MMT.M_SWAP <> 0) APV_VOLUME
                                 FROM ME_DOCM MDM, ME_DOCD MDD
                                 WHERE MDM.DOCNO = MDD.DOCNO
                                 AND   SUBSTR (TWN_DATE (MDD.APVTIME), 1, 5) = :DATA_YM
                                 AND   (SELECT MMT.MAT_CLASS
                                        FROM MI_MAST MMT
                                        WHERE MMT.MMCODE = MDD.MMCODE) IN ('02',
                                                                           '03',
                                                                           '04',
                                                                           '05',
                                                                           '06')
                                 AND (SELECT MMT.M_STOREID
                                      FROM MI_MAST MMT
                                      WHERE MMT.MMCODE = MDD.MMCODE) = '1') C
                           GROUP BY C.APPDEPT
                           ORDER BY C.APPDEPT";

            p.Add(":DATA_YM", string.Format("{0}", DATA_YM));

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        //列印
        public IEnumerable<FA0030> Print(string DATA_YM)
        {
            var p = new DynamicParameters();
            var sql = @"  SELECT C.APPDEPT,
                                  (SELECT URID.INID_NAME
                                   FROM UR_INID URID
                                   WHERE URID.INID = C.APPDEPT) APPDEPT_NAME,
                                  TO_CHAR(NVL (SUM (C.APV_VOLUME), 0),'FM9999999990.00') SUM_APV_VOLUME
                           FROM (SELECT MDM.APPDEPT,
                                        (SELECT   MMT.M_VOLL * MMT.M_VOLW * MMT.M_VOLH * MMT.M_VOLC / MMT.M_SWAP * (SELECT MWN.APL_INQTY
                                                                                                                    FROM MI_WINVMON MWN
                                                                                                                    WHERE  MWN.DATA_YM = :DATA_YM
                                                                                                                    AND    MWN.WH_NO = MDM.APPDEPT
                                                                                                                    AND    MWN.MMCODE = MDD.MMCODE)
                                         FROM MI_MAST MMT
                                         WHERE  MMT.MMCODE = MDD.MMCODE
                                         AND    MMT.M_SWAP IS NOT NULL
                                         AND    MMT.M_SWAP <> 0) APV_VOLUME
                                 FROM ME_DOCM MDM, ME_DOCD MDD
                                 WHERE MDM.DOCNO = MDD.DOCNO
                                 AND   SUBSTR (TWN_DATE (MDD.APVTIME), 1, 5) = :DATA_YM
                                 AND   (SELECT MMT.MAT_CLASS
                                        FROM MI_MAST MMT
                                        WHERE MMT.MMCODE = MDD.MMCODE) IN ('02',
                                                                           '03',
                                                                           '04',
                                                                           '05',
                                                                           '06')
                                 AND (SELECT MMT.M_STOREID
                                      FROM MI_MAST MMT
                                      WHERE MMT.MMCODE = MDD.MMCODE) = '1') C
                           GROUP BY C.APPDEPT
                           ORDER BY C.APPDEPT";

            p.Add(":DATA_YM", string.Format("{0}", DATA_YM));

            return DBWork.Connection.Query<FA0030>(sql, p, DBWork.Transaction);
        }
    }
}