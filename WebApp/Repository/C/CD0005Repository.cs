using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.C
{
    public class CD0005Repository : JCLib.Mvc.BaseRepository
    {
        public CD0005Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        //查詢Master
        public IEnumerable<CD0005M> GetAllM(string WH_NO, string PICK_DATE, string PICK_USERID, string ACT_PICK_QTY_CODE, string HAS_CONFIRMED, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"  SELECT :ACT_PICK_QTY_CODE ACT_PICK_QTY_CODE,
                                 :HAS_CONFIRMED HAS_CONFIRMED,
                                 AA.WH_NO,
                                 AA.PICK_DATE,
                                 AA.LOT_NO,
                                 AA.DOCNO,
                                (select (select WH_NAME from MI_WHMAST where WH_NO = ME_DOCM.APPDEPT ) from ME_DOCM where DOCNO=AA.DOCNO) as APPDEPT,
                                 AA.PICK_USERID,
                                 (SELECT URID.UNA
                                  FROM UR_ID URID
                                  WHERE URID.TUSER = AA.PICK_USERID) PICK_USERNAME,
                                 AA.HAS_CONFIRMED HAS_CONFIRMED_CODE,
                                 (SELECT '已確認'
                                  FROM DUAL
                                  WHERE AA.HAS_CONFIRMED IS NOT NULL AND AA.HAS_CONFIRMED = 'Y')
                                  || (SELECT '待確認'
                                      FROM DUAL
                                      WHERE AA.HAS_CONFIRMED IS NULL)  CONFIRM_STATUS,
                                 NVL( SUM (AA.ITEM_CNT), 0) AS ITEM_CNT_SUM,
                                 NVL( SUM (AA.APPQTY), 0) AS APPQTY_SUM,
                                 NVL( SUM (AA.PICK_ITEM_CNT), 0) AS PICK_ITEM_CNT_SUM,
                                 NVL( SUM (AA.ACT_PICK_QTY), 0) AS ACT_PICK_QTY_SUM
                          FROM (SELECT BWK.WH_NO,
                                       TO_CHAR (BWK.PICK_DATE, 'YYYY/MM/DD') PICK_DATE,
                                       (SELECT BWC.LOT_NO
                                        FROM BC_WHPICKDOC BWC
                                        WHERE     BWC.WH_NO = :WH_NO
                                        AND TRUNC(BWC.PICK_DATE) = TO_DATE(:PICK_DATE,'YYYY/MM/DD')
                                        AND BWC.DOCNO = BWK.DOCNO) LOT_NO,
                                       BWK.DOCNO,
                                       BWK.PICK_USERID,
                                       1 AS ITEM_CNT,
                                       BWK.APPQTY,
                                       (SELECT 1
                                        FROM DUAL
                                        WHERE BWK.ACT_PICK_USERID IS NOT NULL) PICK_ITEM_CNT,
                                       BWK.ACT_PICK_QTY,
                                       BWK.HAS_CONFIRMED
                                FROM BC_WHPICK BWK
                                WHERE BWK.WH_NO = :WH_NO
                                AND   TRUNC(BWK.PICK_DATE) = TO_DATE(:PICK_DATE,'YYYY/MM/DD')
                                AND   ACT_PICK_USERID IS NOT NULL";


            p.Add(":WH_NO", string.Format("{0}", WH_NO));
            p.Add(":PICK_DATE", string.Format("{0}", PICK_DATE));
            p.Add(":ACT_PICK_QTY_CODE", string.Format("{0}", ACT_PICK_QTY_CODE));
            p.Add(":HAS_CONFIRMED", string.Format("{0}", HAS_CONFIRMED));

            if (PICK_USERID != "")
            {
                sql += @" AND BWK.PICK_USERID = :PICK_USERID";
                p.Add(":PICK_USERID", string.Format("{0}", PICK_USERID));
            }

            if (ACT_PICK_QTY_CODE == "1")
            {
                sql += @" AND BWK.APPQTY = BWK.ACT_PICK_QTY";
            }
            else if (ACT_PICK_QTY_CODE == "0")
            {
                sql += @" AND ( BWK.ACT_PICK_QTY IS NULL OR BWK.APPQTY <> BWK.ACT_PICK_QTY )";
            }

            if (HAS_CONFIRMED == "1")
            {
                sql += @" AND ( BWK.HAS_CONFIRMED IS NOT NULL AND BWK.HAS_CONFIRMED = 'Y' ) ";
            }
            else if (HAS_CONFIRMED == "0")
            {
                sql += @" AND BWK.HAS_CONFIRMED IS NULL";
            }

            sql += @"  ) AA
                      GROUP BY WH_NO,
                               PICK_DATE,
                               LOT_NO,
                               DOCNO,
                               PICK_USERID,
                               HAS_CONFIRMED";
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CD0005M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //查詢Detail
        public IEnumerable<CD0005D> GetAllD(string WH_NO, string PICK_DATE, string LOT_NO, string DOCNO, string PICK_USERID, string ACT_PICK_QTY, string HAS_CONFIRMED, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT BWK.SEQ,
                               BWK.MMCODE,
                               BWK.MMNAME_C,
                               BWK.MMNAME_E,
                               NVL( BWK.APPQTY, 0) APPQTY,
                               NVL( BWK.ACT_PICK_QTY, 0) ACT_PICK_QTY,
                               BWK.BASE_UNIT
                        FROM BC_WHPICK BWK
                        WHERE     BWK.WH_NO = :WH_NO
                        AND       BWK.PICK_DATE = TO_DATE(:PICK_DATE,'YYYY/MM/DD')
                        AND ( SELECT BWC.LOT_NO
                              FROM BC_WHPICKDOC BWC
                              WHERE     BWC.WH_NO = :WH_NO
                              AND BWC.PICK_DATE = TO_DATE(:PICK_DATE,'YYYY/MM/DD')
                              AND BWC.DOCNO = BWK.DOCNO) = :LOT_NO
                        AND BWK.DOCNO = :DOCNO
                        AND NVL(BWK.PICK_USERID, '-1') = NVL(:PICK_USERID, '-1')";

            p.Add(":WH_NO", string.Format("{0}", WH_NO));
            p.Add(":PICK_DATE", string.Format("{0}", PICK_DATE));
            p.Add(":LOT_NO", string.Format("{0}", LOT_NO));
            p.Add(":DOCNO", string.Format("{0}", DOCNO));
            p.Add(":PICK_USERID", string.Format("{0}", PICK_USERID));

            if (ACT_PICK_QTY == "1")
            {
                sql += @" AND BWK.APPQTY = BWK.ACT_PICK_QTY";
            }
            else if (ACT_PICK_QTY == "0")
            {
                sql += @" AND ( BWK.ACT_PICK_QTY IS NULL OR BWK.APPQTY <> BWK.ACT_PICK_QTY )";
            }

            if (HAS_CONFIRMED == "1")
            {
                sql += @" AND ( BWK.HAS_CONFIRMED IS NOT NULL AND BWK.HAS_CONFIRMED = 'Y' ) ";
            }
            else if (HAS_CONFIRMED == "0")
            {
                sql += @" AND BWK.HAS_CONFIRMED IS NULL";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CD0005D>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //庫房代碼combox
        public IEnumerable<ComboItemModel> GetWH_NO(string USERID)
        {
            string sql = @"SELECT MWT.WH_NO VALUE,
                                  MWT.WH_NO || ' ' || MWT.WH_NAME TEXT
                           FROM MI_WHID MWD, MI_WHMAST MWT
                           WHERE     MWD.WH_NO = MWT.WH_NO
                           AND MWT.WH_GRADE = '1'
                           AND MWD.WH_USERID = :USERID
                           ORDER BY MWT.WH_NO";
            return DBWork.Connection.Query<ComboItemModel>(sql, new { USERID = USERID }, DBWork.Transaction);
        }


        //揀貨人員combox
        public IEnumerable<ComboItemModel> GetPICK_USERID(string WH_NO, string PICK_DATE)
        {
            string sql = @"SELECT DISTINCT BWK.PICK_USERID VALUE,
                                           ( SELECT BWK.PICK_USERID || ' ' || URID.UNA
                                             FROM UR_ID URID
                                             WHERE URID.TUSER = BWK.PICK_USERID) TEXT
                           FROM BC_WHPICK BWK
                           WHERE BWK.WH_NO = :WH_NO
                           AND BWK.PICK_DATE = TO_DATE(:PICK_DATE,'YYYY/MM/DD')
                           ORDER BY VALUE";
            return DBWork.Connection.Query<ComboItemModel>(sql, new { WH_NO = WH_NO, PICK_DATE = PICK_DATE }, DBWork.Transaction);
        }

        //確認完成
        public int UpdateOK(string WH_NO, string PICK_DATE, string LOT_NO, string DOCNO, string PICK_USERID, string ACT_PICK_QTY_CODE)
        {
            var sql = @" UPDATE BC_WHPICK BWK
                         SET BWK.HAS_CONFIRMED = 'Y'
                         WHERE     BWK.WH_NO = :WH_NO
                         AND  BWK.PICK_DATE = TO_DATE ( :PICK_DATE, 'YYYY/MM/DD')
                         AND  ( SELECT BWC.LOT_NO
                                FROM BC_WHPICKDOC BWC
                                WHERE     BWC.WH_NO = :WH_NO
                                AND BWC.PICK_DATE = TO_DATE ( :PICK_DATE, 'YYYY/MM/DD')
                                AND BWC.DOCNO = BWK.DOCNO) = :LOT_NO
                         AND  BWK.DOCNO = :DOCNO
                         AND  NVL(BWK.PICK_USERID, '-1' ) = NVL(:PICK_USERID, '-1' )
                         AND  BWK.HAS_CONFIRMED IS NULL";

            if (ACT_PICK_QTY_CODE == "1")
            {
                sql += @" AND BWK.APPQTY = BWK.ACT_PICK_QTY";
            }
            else if (ACT_PICK_QTY_CODE == "0")
            {
                sql += @" AND ( BWK.ACT_PICK_QTY IS NULL OR BWK.APPQTY <> BWK.ACT_PICK_QTY )";
            }

            return DBWork.Connection.Execute(sql, new { WH_NO = WH_NO, PICK_DATE = PICK_DATE, LOT_NO = LOT_NO, DOCNO = DOCNO, PICK_USERID = PICK_USERID }, DBWork.Transaction);
        }

        //確認完成_查詢明細
        public IEnumerable<CD0005MD> Find_Detail_OK(string WH_NO, string PICK_DATE, string LOT_NO, string DOCNO, string PICK_USERID, string ACT_PICK_QTY_CODE)
        {
            var p = new DynamicParameters();

            var sql = @" SELECT BWK.WH_NO,
                                TO_CHAR(BWK.PICK_DATE,'YYYY/MM/DD') PICK_DATE,
                                BWK.DOCNO,
                                BWK.SEQ,
                                BWK.MAT_CLASS,
                                BWK.ACT_PICK_USERID,
                                NVL(BWK.ACT_PICK_QTY, 0) ACT_PICK_QTY,
                                TO_CHAR(BWK.ACT_PICK_TIME,'YYYY/MM/DD') ACT_PICK_TIME
                         FROM BC_WHPICK BWK
                         WHERE     BWK.WH_NO = :WH_NO
                         AND BWK.PICK_DATE = TO_DATE ( :PICK_DATE, 'YYYY/MM/DD')
                         AND BWK.DOCNO = :DOCNO
                         AND NVL (BWK.PICK_USERID, '-1') = NVL ( :PICK_USERID, '-1')
                         AND BWK.HAS_CONFIRMED = 'Y'";

            p.Add(":WH_NO", string.Format("{0}", WH_NO));
            p.Add(":PICK_DATE", string.Format("{0}", PICK_DATE));
            p.Add(":LOT_NO", string.Format("{0}", LOT_NO));
            p.Add(":DOCNO", string.Format("{0}", DOCNO));
            p.Add(":PICK_USERID", string.Format("{0}", PICK_USERID));

            if (ACT_PICK_QTY_CODE == "1")
            {
                sql += @" AND BWK.APPQTY = BWK.ACT_PICK_QTY";
            }
            else if (ACT_PICK_QTY_CODE == "0")
            {
                sql += @" AND ( BWK.ACT_PICK_QTY IS NULL OR BWK.APPQTY <> BWK.ACT_PICK_QTY )";
            }

            return DBWork.Connection.Query<CD0005MD>(sql, p, DBWork.Transaction);
        }

        //確認完成_更新明細
        public int UpdateOK_Deail(string ACT_PICK_QTY, string MAT_CLASS, string ACT_PICK_USERID, string ACT_PICK_TIME, string DOCNO, string SEQ)
        {
            var p = new DynamicParameters();

            var sql = "";

            if (MAT_CLASS == "01")
            {
                sql = @" UPDATE ME_DOCD MDD
                         SET MDD.ACKQTY = :ACT_PICK_QTY,
                             MDD.PICK_QTY = :ACT_PICK_QTY,
                             MDD.APVQTY = :ACT_PICK_QTY,
                             MDD.PICK_USER = :ACT_PICK_USERID,
                             MDD.PICK_TIME = TO_DATE( :ACT_PICK_TIME, 'YYYY/MM/DD')
                         WHERE MDD.DOCNO = :DOCNO
                         AND MDD.SEQ = :SEQ";
            }
            else
            {
                sql = @" UPDATE ME_DOCD MDD
                         SET MDD.ACKQTY = :ACT_PICK_QTY,
                             MDD.PICK_QTY = :ACT_PICK_QTY,
                             MDD.PICK_USER = :ACT_PICK_USERID,
                             MDD.PICK_TIME = TO_DATE( :ACT_PICK_TIME, 'YYYY/MM/DD')
                         WHERE MDD.DOCNO = :DOCNO 
                         AND MDD.SEQ = :SEQ";
            }
            p.Add(":ACT_PICK_QTY", string.Format("{0}", ACT_PICK_QTY));
            p.Add(":ACT_PICK_USERID", string.Format("{0}", ACT_PICK_USERID));
            p.Add(":ACT_PICK_TIME", string.Format("{0}", ACT_PICK_TIME));
            p.Add(":DOCNO", string.Format("{0}", DOCNO));
            p.Add(":SEQ", string.Format("{0}", SEQ));

            return DBWork.Connection.Execute(sql, new { ACT_PICK_QTY = ACT_PICK_QTY, ACT_PICK_USERID = ACT_PICK_USERID, ACT_PICK_TIME = ACT_PICK_TIME, DOCNO = DOCNO, SEQ = SEQ }, DBWork.Transaction);
        }


        //確認取消
        public int UpdateCanael(string WH_NO, string PICK_DATE, string LOT_NO, string DOCNO, string PICK_USERID, string ACT_PICK_QTY_CODE)
        {
            var sql = @" UPDATE  BC_WHPICK BWK
                         SET  BWK.HAS_CONFIRMED = '',
                              BWK.HAS_UPDATE_APPQTY = ''
                         WHERE     BWK.WH_NO = :WH_NO
                         AND BWK.PICK_DATE = TO_DATE ( :PICK_DATE, 'YYYY/MM/DD')
                         AND ( SELECT  BWC.LOT_NO
                               FROM  BC_WHPICKDOC BWC
                               WHERE     BWC.WH_NO = :WH_NO
                         AND BWC.PICK_DATE = TO_DATE ( :PICK_DATE, 'YYYY/MM/DD')
                         AND BWC.DOCNO = BWK.DOCNO) = :LOT_NO
                         AND BWK.DOCNO = :DOCNO
                         AND NVL(BWK.PICK_USERID, '-1' ) = NVL(:PICK_USERID, '-1' )
                         AND BWK.HAS_CONFIRMED = 'Y'";

            if (ACT_PICK_QTY_CODE == "1")
            {
                sql += @" AND BWK.APPQTY = BWK.ACT_PICK_QTY";
            }
            else if (ACT_PICK_QTY_CODE == "0")
            {
                sql += @" AND ( BWK.ACT_PICK_QTY IS NULL OR BWK.APPQTY <> BWK.ACT_PICK_QTY )";
            }

            return DBWork.Connection.Execute(sql, new { WH_NO = WH_NO, PICK_DATE = PICK_DATE, LOT_NO = LOT_NO, DOCNO = DOCNO, PICK_USERID = PICK_USERID }, DBWork.Transaction);
        }

        //確認取消_查詢明細
        public IEnumerable<CD0005DD> Find_Detail_Canael(string WH_NO, string PICK_DATE, string LOT_NO, string DOCNO, string PICK_USERID, string ACT_PICK_QTY_CODE)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT BWK.WH_NO,
                               BWK.PICK_DATE,
                               BWK.DOCNO,
                               BWK.SEQ,
                               BWK.MAT_CLASS,
                               NVL( BWK.APPQTY, 0 ) APPQTY,
                               BWK.ACT_PICK_USERID,
                               NVL( BWK.ACT_PICK_QTY , 0) ACT_PICK_QTY,
                               BWK.ACT_PICK_TIME
                        FROM BC_WHPICK BWK
                        WHERE     BWK.WH_NO = :WH_NO
                        AND BWK.PICK_DATE = TO_DATE ( :PICK_DATE, 'YYYY/MM/DD')
                        AND BWK.DOCNO = :DOCNO
                        AND NVL(BWK.PICK_USERID, '-1' ) = NVL(:PICK_USERID, '-1' )
                        AND BWK.HAS_CONFIRMED IS NULL";

            p.Add(":WH_NO", string.Format("{0}", WH_NO));
            p.Add(":PICK_DATE", string.Format("{0}", PICK_DATE));
            p.Add(":LOT_NO", string.Format("{0}", LOT_NO));
            p.Add(":DOCNO", string.Format("{0}", DOCNO));
            p.Add(":PICK_USERID", string.Format("{0}", PICK_USERID));

            if (ACT_PICK_QTY_CODE == "1")
            {
                sql += @" AND BWK.APPQTY = BWK.ACT_PICK_QTY";
            }
            else if (ACT_PICK_QTY_CODE == "0")
            {
                sql += @" AND ( BWK.ACT_PICK_QTY IS NULL OR BWK.APPQTY <> BWK.ACT_PICK_QTY )";
            }

            return DBWork.Connection.Query<CD0005DD>(sql, p, DBWork.Transaction);
        }

        //確認取消_更新明細
        public int UpdateCanael_Deail(string APPQTY, string MAT_CLASS, string DOCNO, string SEQ)
        {
            var p = new DynamicParameters();

            var sql = "";

            if (MAT_CLASS == "01")
            {
                sql = @" UPDATE ME_DOCD MDD
                         SET MDD.ACKQTY = :APPQTY,
                             MDD.PICK_QTY = :APPQTY,
                             MDD.APVQTY=:APPQTY,
                             MDD.PICK_USER = '',
                             MDD.PICK_TIME = ''
                         WHERE MDD.DOCNO = :DOCNO
                         AND MDD.SEQ = :SEQ";
            }
            else
            {
                sql = @" UPDATE ME_DOCD MDD
                         SET MDD.ACKQTY = :APPQTY,
                             MDD.PICK_QTY = :APPQTY,
                             MDD.PICK_USER = '',
                             MDD.PICK_TIME = ''
                         WHERE MDD.DOCNO = :DOCNO
                         AND MDD.SEQ = :SEQ";
            }
            p.Add(":APPQTY", string.Format("{0}", APPQTY));
            p.Add(":DOCNO", string.Format("{0}", DOCNO));
            p.Add(":SEQ", string.Format("{0}", SEQ));

            return DBWork.Connection.Execute(sql, new { APPQTY = APPQTY, DOCNO = DOCNO, SEQ = SEQ }, DBWork.Transaction);
        }


    }
}
