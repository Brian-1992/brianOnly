using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dapper;

namespace WebApp.Repository
{
    public class CallHisApiRepository : JCLib.Mvc.BaseRepository
    {
        public CallHisApiRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }


        public int SetErrLog(string postUrl, string send, string receive, string msg, string userId)
        {
            string sql = @"INSERT INTO ERROR_LOG (
                        LOGTIME, PG, MSG,  USERID) 
                        VALUES ( 
                        sysdate, 'CallHisApi', 'URL:' || :URL || ',SEND:' || :SEND || ',RECEIVE:' || :RECEIVE || ',MSG:' || :MSG, :USERID )";
            return DBWork.Connection.Execute(sql, new { URL = postUrl, SEND = send, RECEIVE = receive, MSG = msg, USERID = userId }, DBWork.Transaction);
        }
    }
}