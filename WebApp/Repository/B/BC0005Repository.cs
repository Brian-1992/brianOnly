using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.BC
{
    public class BC0005Repository : JCLib.Mvc.BaseRepository
    {
        public BC0005Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        //查詢Master
        public IEnumerable<BC0005M> GetAllM(string START_DATE, string END_DATE, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @" SELECT MPM.PO_NO,
                                MPM.AGEN_NO,
                                ( SELECT PVR.AGEN_NO || ' ' || PVR.AGEN_NAMEC
                                  FROM PH_VENDER PVR
                                  WHERE MPM.AGEN_NO = PVR.AGEN_NO) AGEN_NAMEC,
                                MPM.PO_TIME,
                                MPM.M_CONTID,
                                MPM.PO_STATUS,
                                ( CASE
                                  WHEN MPM.PO_STATUS = '80' THEN MPM.PO_STATUS || ' 開單'
                                  WHEN MPM.PO_STATUS = '82' THEN MPM.PO_STATUS || ' 已傳MAIL'
                                  WHEN MPM.PO_STATUS = '84' THEN MPM.PO_STATUS || ' 待傳MAIL'
                                END) PO_STATUS_CODE,
                                ( CASE
                                  WHEN MPM.PO_STATUS = '80' THEN '開單'
                                  WHEN MPM.PO_STATUS = '82' THEN '已傳MAIL'
                                  WHEN MPM.PO_STATUS = '84' THEN '待傳MAIL'
                                END) PO_STATUS_NAME,
                                MPM.MEMO,
                                MPM.MEMO MEMO_DISPLAY,
                                MPM.ISCONFIRM,
                                MPM.ISBACK,
                                MPM.PHONE,
                                MPM.SMEMO SMEMO_DISPLAY,
                                MPM.SMEMO,
                                MPM.ISCOPY,
                                MPM.SDN
                         FROM MM_PO_M MPM
                         WHERE 1 = 1
                         AND (    SUBSTR (MPM.SDN, 8, 7) >= :START_DATE
                              AND SUBSTR (MPM.SDN, 8, 7) <= :END_DATE)";

            p.Add(":START_DATE", string.Format("{0}", START_DATE));
            p.Add(":END_DATE", string.Format("{0}", END_DATE));
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            sql += @" ORDER BY PO_NO, AGEN_NO";

            return DBWork.Connection.Query<BC0005M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //查詢Detail
        public IEnumerable<BC0005D> GetAllD(string PO_NO, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @" SELECT MPD.MMCODE,
                                ( SELECT MMT.MMNAME_C
                                  FROM MI_MAST MMT
                                  WHERE MMT.MMCODE = MPD.MMCODE) MMNAME_C,
                                ( SELECT MMT.MMNAME_E
                                  FROM MI_MAST MMT
                                  WHERE MMT.MMCODE = MPD.MMCODE) MMNAME_E,
                                MPD.M_AGENLAB,
                                MPD.M_PURUN,
                                TO_CHAR (MPD.PO_PRICE, 'fm99999999990.00') PO_PRICE,
                                TO_CHAR (MPD.PO_QTY, 'fm99999999990.00') PO_QTY,
                                TO_CHAR (MPD.PO_AMT, 'fm99999999990.00') PO_AMT,
                                MPD.MEMO
                         FROM MM_PO_D MPD
                         WHERE MPD.PO_NO = :PO_NO
                         ORDER BY MMCODE";

            p.Add(":PO_NO", string.Format("{0}", PO_NO));
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<BC0005D>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //修改Master
        public int UpdateM(BC0005M BC0005M)
        {
            var sql = @" UPDATE MM_PO_M
                         SET    MEMO = :MEMO,
                                SMEMO = :SMEMO,
                                UPDATE_TIME = SYSDATE,
                                UPDATE_USER = :UPDATE_USER,
                                UPDATE_IP = :UPDATE_IP
                         WHERE PO_NO = :PO_NO";
            return DBWork.Connection.Execute(sql, BC0005M, DBWork.Transaction);
        }

        //取消訂單
        public int CANCEL_ORDER_1(string SDN, string UPDATE_USER, string UPDATE_IP)
        {
            var sql = @" UPDATE PH_SMALL_M
                         SET    STATUS = 'E',
                                UPDATE_TIME = SYSDATE,
                                UPDATE_USER = :UPDATE_USER,
                                UPDATE_IP = :UPDATE_IP
                         WHERE DN = :SDN";
            return DBWork.Connection.Execute(sql, new { SDN = SDN, UPDATE_USER = UPDATE_USER, UPDATE_IP = UPDATE_IP }, DBWork.Transaction);
        }
        public int CANCEL_ORDER_2(string PO_NO)
        {
            var sql = @" DELETE FROM MM_PO_D
                         WHERE PO_NO IN ( SELECT PO_NO
                                          FROM MM_PO_M
                                          WHERE PR_NO IN ( SELECT PR_NO
                                                           FROM MM_PO_D
                                                           WHERE PO_NO = :PO_NO))";
            return DBWork.Connection.Execute(sql, new { PO_NO = PO_NO }, DBWork.Transaction);
        }
        public int CANCEL_ORDER_3(string SDN)
        {
            var sql = @" DELETE FROM MM_PO_M
                         WHERE SDN = :SDN";
            return DBWork.Connection.Execute(sql, new { SDN = SDN }, DBWork.Transaction);
        }
        public int CANCEL_ORDER_4(string SDN)
        {
            var sql = @" DELETE FROM MM_PR_D
                         WHERE PR_NO IN ( SELECT DISTINCT PR_NO
                                         FROM MM_PR_M
                                         WHERE SDN = :SDN)";
            return DBWork.Connection.Execute(sql, new { SDN = SDN }, DBWork.Transaction);
        }
        public int CANCEL_ORDER_5(string SDN)
        {
            var sql = @" DELETE FROM MM_PR_M
                         WHERE SDN = :SDN";
            return DBWork.Connection.Execute(sql, new { SDN = SDN }, DBWork.Transaction);
        }

        //寄送MAIL
        public int SEND_MAIL(string PO_NO, string UPDATE_USER, string UPDATE_IP)
        {
            var sql = @" UPDATE MM_PO_M 
                         SET    PO_STATUS = '84',
                                UPDATE_TIME = SYSDATE,
                                UPDATE_USER = :UPDATE_USER,
                                UPDATE_IP = :UPDATE_IP
                         WHERE PO_NO = :PO_NO";
            return DBWork.Connection.Execute(sql, new { PO_NO = PO_NO, UPDATE_USER = UPDATE_USER, UPDATE_IP = UPDATE_IP }, DBWork.Transaction);
        }

    }
}
