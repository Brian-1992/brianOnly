using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Repository.C
{
    public class CE0029Repository:JCLib.Mvc.BaseRepository
    {
        public CE0029Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public bool CheckUserInChkuid(string chk_level, string chk_type, string chk_class, string chk_uid)
        {
            string sql = @"select 1 from CHK_MAST a
                            where a.chk_wh_no = WHNO_MM1
                              and a.chk_level = :chk_level
                              and a.chk_type = :chk_type
                              and a.chk_class = :chk_class
                              and a.chk_ym = TWN_YYYMM(SYSDATE)
                              and exists (select 1 from CHK_DETAIL where chk_no = a.chk_no and chk_uid = :chk_uid)";
            return !(DBWork.Connection.ExecuteScalar(sql, new { chk_level = chk_level, chk_type = chk_type, chk_class = chk_class, chk_uid = chk_uid}, DBWork.Transaction) == null);
        }
        public bool CheckPDADone(string chk_level, string chk_type, string chk_class, string chk_uid)
        {
            string sql = @"select 1 from CHK_MAST a
                            where a.chk_wh_no = WHNO_MM1
                              and a.chk_level = :chk_level
                              and a.chk_type = :chk_type
                              and a.chk_class = :chk_class
                              and a.chk_ym = TWN_YYYMM(SYSDATE)
                              and exists (select 1 from CHK_DETAIL where chk_no = a.chk_no and chk_uid = :chk_uid and chk_time is null)";
            return (DBWork.Connection.ExecuteScalar(sql, new { chk_level = chk_level, chk_type = chk_type, chk_class = chk_class, chk_uid = chk_uid}, DBWork.Transaction) == null);
        }
        public bool CheckHasNullChkqty(string chk_no, string chk_uid) {
            string sql = @"select 1 from CHK_DETAIL
                            where chk_no = :chk_no
                              and chk_uid = :chk_uid
                              and chk_qty is null";
            return !(DBWork.Connection.ExecuteScalar(sql, new { chk_no = chk_no, chk_uid = chk_uid }, DBWork.Transaction) == null);
        }

        public IEnumerable<CHK_DETAIL> GetAll(string level,string type, string matclass, string user, int page_index, int page_size, string sorters) {
            var p = new DynamicParameters();
            string sql = @"SELECT A.*,TWN_TIME_FORMAT(A.CHK_TIME) CHK_TIME_T 
                            FROM CHK_DETAIL A,CHK_MAST B 
                            WHERE A.CHK_NO = B.CHK_NO
                            AND B.CHK_WH_NO = WHNO_MM1 
                            AND SUBSTR(B.CHK_YM,1,5) = TWN_YYYMM(SYSDATE) 
                             ";
            if (level != "")
            {
                sql += " AND B.CHK_LEVEL = :p0 ";
                p.Add(":p0", string.Format("{0}", level));
            }
            if (type != "")
            {
                sql += " AND B.CHK_TYPE = :p1 ";
                p.Add(":p1", string.Format("{0}", type));
            }
            if (matclass != "")
            {
                sql += " AND B.CHK_CLASS = :p2 ";
                p.Add(":p2", string.Format("{0}", matclass));
            }
            if (user != "")
            {
                sql += " AND A.CHK_UID = :p3 ";
                p.Add(":p3", string.Format("{0}", user));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CHK_DETAIL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public int UpdateChkd(CHK_DETAIL chkd)
        {
            var sql = @"UPDATE CHK_DETAIL SET 
                        CHK_QTY = :CHK_QTY, 
                        CHK_TIME = SYSDATE,  
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE CHK_NO = :CHK_NO AND MMCODE = :MMCODE and store_loc = :store_loc";
            return DBWork.Connection.Execute(sql, chkd, DBWork.Transaction);
        }
        public int FinishChkDetail(string chk_no, string chk_uid, string update_ip) {
            string sql = @"update CHK_DETAIL 
                              set status_ini = '2', update_time = sysdate, update_user = :chk_uid, update_ip = :update_ip
                            where chk_no = :chk_no and chk_uid = :chk_uid";
            return DBWork.Connection.Execute(sql, new { chk_no = chk_no, chk_uid = chk_uid, update_ip = update_ip}, DBWork.Transaction);
        }

        public int FinishChkd1(CHK_DETAIL chkd)
        {
            var sql = @"UPDATE CHK_DETAIL SET 
                        STATUS_INI = '2',  
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE CHK_NO = :CHK_NO AND MMCODE = :MMCODE ";
            return DBWork.Connection.Execute(sql, chkd, DBWork.Transaction);
        }

        public int FinishChkd2(CHK_DETAIL chkd)
        {
            var sql = @"UPDATE CHK_MAST SET 
                        CHK_NUM = (
                            SELECT COUNT(*) FROM CHK_DETAIL WHERE CHK_NO = :CHK_NO AND STATUS_INI = '2'
                        ),  
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE CHK_NO = :CHK_NO  ";
            return DBWork.Connection.Execute(sql, chkd, DBWork.Transaction);
        }
        public int FinishChkd2(string chk_no, string update_user, string update_ip)
        {
            var sql = @"UPDATE CHK_MAST SET 
                        CHK_NUM = (
                            SELECT COUNT(*) FROM CHK_DETAIL WHERE CHK_NO = :CHK_NO AND STATUS_INI = '2'
                        ),  
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE CHK_NO = :CHK_NO  ";
            return DBWork.Connection.Execute(sql, new { chk_no = chk_no, update_user = update_user, update_ip = update_ip}, DBWork.Transaction);
        }
        public int FinishChkd3(CHK_DETAIL chkd)
        {
            var sql = @"UPDATE CHK_MAST SET 
                        CHK_STATUS = '2',  
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE CHK_NO = :CHK_NO  ";
            return DBWork.Connection.Execute(sql, chkd, DBWork.Transaction);
        }
        public int FinishChkd3(string chk_no, string update_user, string update_ip)
        {
            var sql = @"UPDATE CHK_MAST SET 
                        CHK_STATUS = '2',  
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE CHK_NO = :CHK_NO  ";
            return DBWork.Connection.Execute(sql, new { chk_no = chk_no, update_user = update_user, update_ip = update_ip }, DBWork.Transaction);
        }

        public int GetChkNum(string id)
        {
            string sql = @"SELECT COUNT(*) AS CNT FROM CHK_DETAIL  
                            WHERE CHK_NO = :CHK_NO AND STATUS_INI = '2' ";
            int rtn = Convert.ToInt32(DBWork.Connection.ExecuteScalar(sql, new { CHK_NO = id }, DBWork.Transaction).ToString());
            return rtn;
        }
        public int GetChkTot(string id)
        {
            string sql = @"SELECT COUNT(*) AS CNT FROM CHK_DETAIL  
                            WHERE CHK_NO = :CHK_NO  ";
            int rtn = Convert.ToInt32(DBWork.Connection.ExecuteScalar(sql, new { CHK_NO = id }, DBWork.Transaction).ToString());
            return rtn;
        }

        public CHK_MAST GetChkMast(string chk_no)
        {
            string sql = @"select * from CHK_MAST where chk_no = :chk_no";

            return DBWork.Connection.QueryFirstOrDefault<CHK_MAST>(sql, new { chk_no = chk_no }, DBWork.Transaction);
        }
    }
}