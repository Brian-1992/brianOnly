using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;


namespace WebApp.Repository.C
{
    public class CE0028Repository : JCLib.Mvc.BaseRepository
    {
        public CE0028Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        //找本次所有盤點項目
        public IEnumerable<CE0028> FindAll(string CHK_YM, string CHK_TYPE, string CHK_CLASS)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT A.CHK_NO CHK_NO_A,
                               A.MMCODE MMCODE_A,
                               A.MMNAME_C MMNAME_C_A,
                               A.MMNAME_E MMNAME_E_A,
                               (SELECT BASE_UNIT
                                FROM MI_MAST
                                WHERE MMCODE = A.MMCODE) BASE_UNIT_A,
                               A.STORE_QTY STORE_QTY_A,
                               A.CHK_QTY1 CHK_QTY1_A,
                               A.CHK_QTY2 CHK_QTY2_A,
                               A.CHK_QTY3 CHK_QTY3_A,
                               (SELECT LISTAGG (UNA, '<br>') WITHIN GROUP (ORDER BY UNA)
                                FROM (  SELECT DISTINCT UNA
                                        FROM CHK_DETAIL C, UR_ID D
                                        WHERE     C.CHK_UID = D.TUSER
                                        AND C.CHK_NO = A.CHK_NO
                                        AND C.MMCODE = A.MMCODE
                                        ORDER BY UNA)) CHK_UID_NAME_A,
                               A.STATUS_TOT STATUS_TOT_A,
                               A.GAP_T GAP_T_A
                        FROM CHK_DETAILTOT A
                        WHERE CHK_NO = (SELECT CHK_NO
                                        FROM CHK_MAST
                                        WHERE     CHK_WH_NO = '560000'
                                        AND CHK_YM = :CHK_YM
                                        AND CHK_TYPE = :CHK_TYPE
                                        AND CHK_CLASS = :CHK_CLASS
                                        AND CHK_LEVEL = '1')";

            p.Add(":CHK_YM", string.Format("{0}", CHK_YM));
            p.Add(":CHK_TYPE", string.Format("{0}", CHK_TYPE));
            p.Add(":CHK_CLASS", string.Format("{0}", CHK_CLASS));

            return DBWork.PagingQuery<CE0028>(sql, p, DBWork.Transaction);
        }

        //用初盤chk_no找三盤的項目 
        public IEnumerable<CE0028> FindLevel3(string CHK_YM, string CHK_TYPE, string CHK_CLASS)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT B.CHK_NO CHK_NO_3,
                               B.MMCODE MMCODE_3,
                               B.MMNAME_C MMNAME_C_3,
                               B.MMNAME_E MMNAME_E_3,
                               B.BASE_UNIT BASE_UNIT_3,
                               SUM(B.CHK_QTY) CHK_QTY_3
                        FROM CHK_MAST A, CHK_DETAIL B
                        WHERE     A.CHK_NO1 =
                                             (SELECT CHK_NO
                                              FROM CHK_MAST
                                              WHERE     CHK_WH_NO = '560000'
                                              AND CHK_YM = :CHK_YM
                                              AND CHK_TYPE = :CHK_TYPE
                                              AND CHK_CLASS = :CHK_CLASS
                                              AND CHK_LEVEL = '1')
                        AND A.CHK_LEVEL = '3'
                        AND B.CHK_NO = A.CHK_NO
                        group by B.MMCODE, b.CHK_NO, b.MMNAME_C, b.MMNAME_E, b.BASE_UNIT";

            p.Add(":CHK_YM", string.Format("{0}", CHK_YM));
            p.Add(":CHK_TYPE", string.Format("{0}", CHK_TYPE));
            p.Add(":CHK_CLASS", string.Format("{0}", CHK_CLASS));

            return DBWork.Connection.Query<CE0028>(sql, p, DBWork.Transaction);
        }

        //取得盤點狀態
        public string GetCHK_STATUS(string CHK_YM, string CHK_TYPE, string CHK_CLASS)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT CHK_STATUS
                        FROM CHK_MAST
                        WHERE     CHK_NO1 =
                                           (SELECT CHK_NO
                                            FROM CHK_MAST
                                            WHERE  CHK_WH_NO = '560000'
                                            AND    CHK_YM = :CHK_YM
                                            AND    CHK_TYPE = :CHK_TYPE
                                            AND    CHK_CLASS = :CHK_CLASS
                                            AND    CHK_LEVEL = '1')
                        AND CHK_LEVEL = '3'
                        UNION
                        SELECT '0' CHK_STATUS
                        FROM DUAL
                        ORDER BY CHK_STATUS DESC";

            p.Add(":CHK_YM", string.Format("{0}", CHK_YM));
            p.Add(":CHK_TYPE", string.Format("{0}", CHK_TYPE));
            p.Add(":CHK_CLASS", string.Format("{0}", CHK_CLASS));

            return DBWork.Connection.QueryFirst<string>(sql, p, DBWork.Transaction).ToString();
        }

        //駁回_1
        public int GoBack_1(string CHK_NO_A, string MMCODE_A)
        {
            var sql = @"UPDATE CHK_DETAIL
                        SET STATUS_INI = '1', chk_time = null, chk_qty = null
                        WHERE CHK_NO = :CHK_NO_A
                        AND MMCODE = :MMCODE_A";

            return DBWork.Connection.Execute(sql, new { CHK_NO_A = CHK_NO_A, MMCODE_A = MMCODE_A }, DBWork.Transaction);
        }

        //駁回_2
        public int GoBack_2(string CHK_NO_A)
        {
            var sql = @"UPDATE CHK_MAST
                        SET CHK_NUM =
                                     (SELECT COUNT (*)
                                      FROM CHK_DETAIL
                                      WHERE CHK_NO = :CHK_NO_A
                                      AND STATUS_INI = '2'),
                            CHK_STATUS = '1'
                        WHERE CHK_NO = :CHK_NO_A";
            return DBWork.Connection.Execute(sql, new { CHK_NO_A = CHK_NO_A }, DBWork.Transaction);
        }

        public CHK_MAST GetChkMast(string chk_no)
        {
            string sql = @"select * from CHK_MAST where chk_no = :chk_no";

            return DBWork.Connection.QueryFirstOrDefault<CHK_MAST>(sql, new { chk_no = chk_no }, DBWork.Transaction);
        }

    }
}