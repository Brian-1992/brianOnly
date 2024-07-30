using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Data;

namespace WebApp.Repository.UR
{
    public class UR_ERR_MRepository : JCLib.Mvc.BaseRepository
    {
        const int LEN_MSG = 100;
        const int LEN_ST  = 4000;
        public UR_ERR_MRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        public int Create(UR_ERR_M ur_err_m)
        {
            //var sql = @"INSERT INTO UR_ERR_M (IDNO,CTRL,ACT,MSG,ST,MSGV,ED,TUSER,OWNER)
            //            VALUES (:IDNO,:CTRL,:ACT,:MSG,:ST,:MSGV,sysdate,:TUSER,:OWNER)";
            //var sql = @"INSERT INTO UR_ERR_M (IDNO,CTRL,ACT,MSG,ST,MSGV,ED,TUSER,OWNER)
            //            VALUES ((select max(IDNO)+1 from UR_ERR_M),:CTRL,:ACT,substr(:MSG,1,LEN_MSG),substr(:ST,1,LEN_ST),:MSGV,sysdate,:TUSER,:OWNER)";
            var sql = $@"INSERT INTO UR_ERR_M (IDNO,CTRL,ACT,MSG,ST,MSGV,ED,TUSER,OWNER,IP)
                        VALUES (to_number(:IDNO),:CTRL,:ACT,substr(:MSG,1,{LEN_MSG}),substr(:ST,1,{LEN_ST}),:MSGV,sysdate,:TUSER,:OWNER,:IP)";
            return DBWork.Connection.Execute(sql, ur_err_m, DBWork.Transaction);
        }
        public string GetNextIdno()
        {
            string sql = @"SELECT NVL(MAX(IDNO),0)+1 as SEQ FROM UR_ERR_M";
            string rtn = DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction).ToString();
            return rtn;
        }
        public IEnumerable<UR_ERR_M> Query(string idno, string una, string ctrl, string ed_bg, string ed_ed)
        {
            var p = new DynamicParameters();
            
            var sql = @"SELECT IDNO, CTRL, ACT, MSG, ST, MSGV, ED, TUSER, OWNER, IP, 
                        (SELECT UNA FROM UR_ID WHERE TUSER=EM.TUSER) UNA 
                        FROM UR_ERR_M EM where 1=1 ";

            if (idno != "")
            {
                sql += " AND IDNO = :p0 ";
                p.Add(":p0", idno);
            }
            if (una != "")
            {
                sql += " AND TUSER IN (SELECT TUSER FROM UR_ID WHERE UNA LIKE :p1) ";
                p.Add(":p1", string.Format("%{0}%", una));
            }
            if (ctrl != "")
            {
                sql += " AND CTRL LIKE :p2 ";
                p.Add(":p2", string.Format("{0}%", ctrl));
            }
            if (ed_bg != "" && ed_bg != null)
            {
                sql += " AND to_char(ED,'yyyy-mm-dd') >= Substr(:p3, 1, 10) ";
                p.Add(":p3", ed_bg);
            }
            if (ed_ed != "" && ed_ed != null)
            {
                sql += " AND to_char(ED,'yyyy-mm-dd') <= Substr(:p4, 1, 10) ";
                p.Add(":p4", ed_ed);
            }

            return DBWork.PagingQuery<UR_ERR_M>(sql, p, DBWork.Transaction);
        }

        public DataTable GetExcelM(string idno, string una, string ctrl, string ed_bg, string ed_ed)
        {
            DynamicParameters p = new DynamicParameters();

            string sql = @" select IDNO as 錯誤號碼, CTRL as 程式代碼, ACT as 函式代碼, MSG as 簡易訊息, ST as 詳細訊息, MSGV as 展示訊息, 
                        ED as 發生時間, TUSER as 帳號, 
                        (SELECT UNA FROM UR_ID WHERE TUSER=EM.TUSER) 執行人員姓名, IP 
                        FROM UR_ERR_M EM where 1=1 ";

            if (idno != "")
            {
                sql += " AND IDNO = :p0 ";
                p.Add(":p0", idno);
            }
            if (una != "")
            {
                sql += " AND TUSER IN (SELECT TUSER FROM UR_ID WHERE UNA LIKE :p1) ";
                p.Add(":p1", string.Format("%{0}%", una));
            }
            if (ctrl != "")
            {
                sql += " AND CTRL LIKE :p2 ";
                p.Add(":p2", string.Format("{0}%", ctrl));
            }
            if (ed_bg != "" && ed_bg != null)
            {
                sql += " AND twn_date(ED) >= :p3 ";
                p.Add(":p3", ed_bg);
            }
            if (ed_ed != "" && ed_ed != null)
            {
                sql += " AND twn_date(ED) <= :p4 ";
                p.Add(":p4", ed_ed);
            }
            sql += " order by IDNO ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
    }
}