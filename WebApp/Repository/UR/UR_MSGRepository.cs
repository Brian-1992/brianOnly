using System.Collections.Generic;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System;

namespace WebApp.Repository.UR
{
    public class UR_MSGRepository : JCLib.Mvc.BaseRepository
    {
        public UR_MSGRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<UR_MSG> GetAll(string send_user, string msg_content, string msg_date_s, string msg_date_e, string include_self, string tuser, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT
                            (select UNA from UR_ID where TUSER = A.RECEIVE_USER) as RECEIVE_USER, 
                            A.MSG_CONTENT, A.MSG_DATE, A.SEND_USER, 
                            (case when A.SEND_USER = :TUSER then '自己' else (select UNA from UR_ID where TUSER = A.SEND_USER) end) as SEND_USER_NAME
                            FROM UR_MSG A
                            WHERE 1=1 ";

            if (!string.IsNullOrWhiteSpace(send_user))
            {
                sql += " and (A.SEND_USER like :SEND_USER or (select UNA from UR_ID where TUSER = A.SEND_USER) like :SEND_USER) ";
                p.Add(":SEND_USER", string.Format("%{0}%", send_user));
            }
            if (!string.IsNullOrWhiteSpace(msg_content))
            {
                sql += " and A.MSG_CONTENT like :MSG_CONTENT ";
                p.Add(":MSG_CONTENT", string.Format("%{0}%", msg_content));
            }
            if (!string.IsNullOrWhiteSpace(msg_date_s))
            {
                sql += " and TWN_DATE(MSG_DATE)>=:MSG_DATE_S ";
                p.Add(":MSG_DATE_S", string.Format("{0}", msg_date_s));
            }
            if (!string.IsNullOrWhiteSpace(msg_date_e))
            {
                sql += " and TWN_DATE(MSG_DATE)<=:MSG_DATE_E ";
                p.Add(":MSG_DATE_E", string.Format("{0}", msg_date_e));
            }
            if (include_self == "Y") // 包含發訊人為自己
                sql += " and (A.RECEIVE_USER = :TUSER or A.SEND_USER = :TUSER) ";
            else
                sql += " and A.RECEIVE_USER = :TUSER ";
            p.Add(":TUSER", string.Format("{0}", tuser));

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<UR_MSG>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<UR_MSG> GetDialogAll(string tuser1, string tuser2, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT
                            (select UNA from UR_ID where TUSER = A.RECEIVE_USER) as RECEIVE_USER, 
                            A.MSG_CONTENT, A.MSG_DATE, A.SEND_USER, 
                            (case when A.SEND_USER = :TUSER2 then '自己' else (select UNA from UR_ID where TUSER = A.SEND_USER) end) as SEND_USER_NAME,
                            READ_FLAG
                            FROM UR_MSG A
                            WHERE 1=1 ";

            // 對話記錄包含: 1.接收=對方且發送=自己; 2.接收=自己且發送=對方
            sql += @" and ((A.RECEIVE_USER = :TUSER1 and A.SEND_USER = :TUSER2)
                           or (A.RECEIVE_USER = :TUSER2 and A.SEND_USER = :TUSER1))";
            
            p.Add(":TUSER1", string.Format("{0}", tuser1)); // 對方
            p.Add(":TUSER2", string.Format("{0}", tuser2)); // 自己

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<UR_MSG>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetSendUserCombo(string userid)
        {
            var sql = @"
                        select TUSER as VALUE, INID_NAME(INID) || ' ' || UNA as TEXT
                        from UR_ID where INID_NAME(INID) is not null 
                            and TUSER <> :TUSER
                        order by INID, UNA
                    ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { TUSER = userid });
        }

        public IEnumerable<COMBO_MODEL> GetReceiveUserCombo(string historyUser, string userid)
        {
            var sql = @"
                        select A.TUSER as VALUE, INID_NAME(A.INID) || ' ' || A.UNA as TEXT
                        from UR_ID A where INID_NAME(A.INID) is not null 
                            and A.TUSER <> :TUSER 
                            and TUSER in (select distinct TUSER from UR_UIR where RLNO in
                                (select RLNO from UR_TACL where FG = 'UR1027')) "; // 有UR1027權限才可以選為收訊人

            if (historyUser == "Y") // 曾發送過的對象
                sql += " and (select count(*) from UR_MSG where RECEIVE_USER = A.TUSER and SEND_USER = :TUSER) > 0 ";

            sql += " order by A.INID, A.UNA ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { TUSER = userid });
        }

        public int SendMsg(string receiveUser, string msgContent, string sendUser)
        {
            var sql = @"insert into UR_MSG (RECEIVE_USER, UR_MSG_SEQ, MSG_CONTENT, MSG_DATE, SEND_USER, READ_FLAG, ALERT_FLAG)
                values (:RECEIVE_USER,
                    nvl((select max(UR_MSG_SEQ) from UR_MSG where RECEIVE_USER = :RECEIVE_USER) + 1, 0),
                    :MSG_CONTENT, sysdate, :SEND_USER, 'N', 'N'
                )";
            return DBWork.Connection.Execute(sql, new { RECEIVE_USER = receiveUser, MSG_CONTENT = msgContent, SEND_USER = sendUser }, DBWork.Transaction);
        }

        public int UpdateReadFlag(string tuser)
        {
            var sql = @"update UR_MSG set READ_FLAG = 'Y' where RECEIVE_USER = :TUSER and nvl(READ_FLAG, 'N') = 'N' ";
            return DBWork.Connection.Execute(sql, new { TUSER = tuser }, DBWork.Transaction);
        }

        public int UpdateAlertFlag(string tuser)
        {
            var sql = @"update UR_MSG set ALERT_FLAG = 'Y' where RECEIVE_USER = :TUSER and nvl(ALERT_FLAG, 'N') = 'N' ";
            return DBWork.Connection.Execute(sql, new { TUSER = tuser }, DBWork.Transaction);
        }

        public string GetOldNotReadCnt(string tuser)
        {
            var sql = @"select count(*) from UR_MSG 
                where RECEIVE_USER = :TUSER and nvl(READ_FLAG, 'N') = 'N'
                    and sysdate - MSG_DATE > 7
                    and RECEIVE_USER in (select distinct TUSER from UR_UIR where RLNO in
                        (select RLNO from UR_TACL where FG = 'UR1027')) "; // 有UR1027權限才檢查是否要通知
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { TUSER = tuser }, DBWork.Transaction);
        }

        public string GetAlertCnt(string tuser)
        {
            var sql = @"select count(*) from UR_MSG 
                where RECEIVE_USER = :TUSER and nvl(ALERT_FLAG, 'N') = 'N' and nvl(READ_FLAG, 'N') = 'N'
                    and RECEIVE_USER in (select distinct TUSER from UR_UIR where RLNO in
                        (select RLNO from UR_TACL where FG = 'UR1027')) "; // 有UR1027權限才檢查是否要通知
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { TUSER = tuser }, DBWork.Transaction);
        }
    }
}