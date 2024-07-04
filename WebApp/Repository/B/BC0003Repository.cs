using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.BC
{
    public class BC0003Repository : JCLib.Mvc.BaseRepository
    {
        public BC0003Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public int MasterUpdate(PH_SMALL_M ph_small_m)
        {
            var sql = @"UPDATE PH_SMALL_M SET ACCEPT=:ACCEPT, AGEN_NAMEC=:AGEN_NAMEC, AGEN_NO=:AGEN_NO, ALT=:ALT, DELIVERY=:DELIVERY, DEMAND=:DEMAND, DUEDATE=:DUEDATE, 
                                OTHERS=:OTHERS, PAYWAY=:PAYWAY, TEL=:TEL, USEWHEN=:USEWHEN, USEWHERE=:USEWHERE, DEPT=:DEPT, APP_USER1=:APP_USER1, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE DN = :DN";
            return DBWork.Connection.Execute(sql, ph_small_m, DBWork.Transaction);
        }
        // 剔退
        public int MasterReject(PH_SMALL_M ph_small_m)
        {
            var sql = @"UPDATE PH_SMALL_M SET REASON = (select UNA from UR_ID where TUSER = :UPDATE_USER) || ' 剔退：' || :REASON || chr(13) || chr(10) ||REASON, STATUS = 'D', APP_USER1=:APP_USER1, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP,
                                SIGNDATA = TWN_TIME_FORMAT(sysdate) ||' '|| (select UNA from UR_ID where TUSER = :UPDATE_USER) || ' 剔退。' || chr(13) || chr(10) ||SIGNDATA
                                WHERE DN = :DN ";
            return DBWork.Connection.Execute(sql, ph_small_m, DBWork.Transaction);
        }

        // 送消審會審核
        public int MasterApprove(PH_SMALL_M ph_small_m)
        {
            var sql = @"UPDATE PH_SMALL_M SET STATUS = 'C', APP_USER1=:APP_USER1, APPTIME1=SYSDATE, 
                                SIGNDATA = TWN_TIME_FORMAT(sysdate) ||' '||(select UNA from UR_ID where TUSER = :UPDATE_USER) || ' 送消審會審核。' || chr(13) || chr(10) || SIGNDATA,
                                UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE DN = :DN ";
            return DBWork.Connection.Execute(sql, ph_small_m, DBWork.Transaction);
        }

        // 陳核單位主官
        public int MasterApprove2(PH_SMALL_M ph_small_m)
        {
            var sql = @"UPDATE PH_SMALL_M SET STATUS = 'G', 
                                SIGNDATA = TWN_TIME_FORMAT(sysdate) ||' '|| (select UNA from UR_ID where TUSER = :UPDATE_USER) || ' 陳核單位主官' || chr(13) || chr(10) ||  SIGNDATA,
                                UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE DN = :DN ";
            return DBWork.Connection.Execute(sql, ph_small_m, DBWork.Transaction);
        }

    }
}