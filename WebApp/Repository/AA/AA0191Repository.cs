using System;
using System.Collections.Generic;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;

namespace WebApp.Repository.AA
{
    public class AA0191Repository : JCLib.Mvc.BaseRepository
    {
        public AA0191Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public int Update(UR_ID ur_id)
        {
            var _afrs = 0;
            var sql = @"UPDATE UR_ID 
                            SET 
                                EMAIL = :EMAIL,
                                UPDATE_TIME=sysdate,
                                UPDATE_USER=:UPDATE_USER,
                                UPDATE_IP=:UPDATE_IP
                            WHERE TUSER=:TUSER";

            _afrs = DBWork.Connection.Execute(sql, ur_id, DBWork.Transaction);

            return _afrs;
        }

        public IEnumerable<UR_ID> Get(string id)
        {
            var sql = @"SELECT TUSER, INID, (select INID_NAME from UR_INID where INID = UR_ID.INID) as INID_NAME, 
                            UNA, IDDESC, EMAIL, TEL, EXT, TITLE, FAX, FL,'' AS PWD, ADUSER, WHITELIST_IP1, WHITELIST_IP2, WHITELIST_IP3
                            from UR_ID where TUSER=:TUSER";
            return DBWork.Connection.Query<UR_ID>(sql, new { TUSER = id }, DBWork.Transaction);
        }

        public DataTable GetExcel(string ts)
        {
            var dt = new DataTable();
            var sql = "SELECT TUSER as 帳號 ,INID as 成本中心, UNA as 使用者名稱 FROM UR_ID WHERE TUSER = :TS";
            using (var rdr = DBWork.Connection.ExecuteReader(sql, new { TS = ts }, DBWork.Transaction))
            {
                dt.Load(rdr);
            }
            return dt;
        }
    }
}