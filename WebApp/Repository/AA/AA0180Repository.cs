using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models.AA;
using System.Collections.Generic;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using WebApp.Models;
using System.Text;

namespace WebApp.Repository.AA
{
    public class AA0180Repository : JCLib.Mvc.BaseRepository
    {
        public AA0180Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AA0180> GetAllM(string p1, string p2, string p3, string tuser, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"
                                SELECT distinct a.DOCNO,
                                    TWN_TIME_FORMAT(a.APPTIME) APPTIME, a.APP_INID,
                                    (a.APP_INID||' '||INID_NAME(a.APP_INID)) as APP_INID_N,
                                    (select INID||' '||INID_NAME FROM UR_INID where INID=a.APPDEPT) AS APPDEPT_N, a.IS_DEL, TWN_TIME_FORMAT(a.APVTIME) APVTIME,
                                    (case when a.IS_DEL='N' then '待送出'
                                         when (a.IS_DEL='S' and trim(APVTIME) is null) then '待核可'
                                         when (a.IS_DEL='S' and trim(APVTIME) is not null) then '已核可'
                                         when a.IS_DEL='Y' then '删除'
                                     end) as STATUS
                                FROM DGMISS a
                                WHERE 1=1 
                            ";

            if (p1 != "" & p2 != "")
            {
                sql += "AND (a.APP_INID BETWEEN :p1 and :p2) ";
                p.Add(":p1", p1);
                p.Add(":p2", p2);
            }
            else if (p1 != "" & p2 == "")
            {
                sql += " AND a.APP_INID >= :p1 ";
                p.Add(":p1", p1);
            }
            else if (p1 == "" & p2 != "")
            {
                sql += " AND a.APP_INID <= :p2) ";
                p.Add(":p2", p2);
            }

            // 判斷勾選項目
            if (!string.IsNullOrWhiteSpace(p3))
            {
                int option = 0;

                // 1 待核可；2 已核可；3 刪除
                if (p3.Contains("1")) option |= 1;
                if (p3.Contains("2")) option |= 2;
                if (p3.Contains("3")) option |= 4;

                switch (option)
                {
                    case 0: // None selected
                        break;
                    case 1: // 1 selected // 待核可勾選 且 已核可、删除未勾選
                        sql += " and a.IS_DEL ='S' and trim(a.APVTIME) is null ";
                        break;
                    case 2: // 2 selected // 已核可勾選 且 待核可、删除未勾選
                        sql += " and a.IS_DEL ='S' and trim(a.APVTIME) is not null ";
                        break;
                    case 3: // 1 and 2 selected // 待核可、已核可勾選 且 删除未勾選
                        sql += " and a.IS_DEL ='S' ";
                        break;
                    case 4: // 3 selected // 删除勾選 且 待核可、已核可未勾選
                        sql += " and a.IS_DEL ='Y' ";
                        break;
                    case 5: // 1 and 3 selected // 待核可、删除勾選 且 已核可未勾選
                        sql += " and ((a.IS_DEL ='S' and trim(a.APVTIME) is null) or (a.IS_DEL ='Y')) ";
                        break;
                    case 6: // 2 and 3 selected // 已核可、删除勾選 且 待核可未勾選
                        sql += " and ((a.IS_DEL ='S' and trim(a.APVTIME) is not null) or (a.IS_DEL ='Y')) ";
                        break;
                    case 7: // 1, 2 and 3 selected
                        break;
                    default:
                        break;
                }
            }

            //sql += " and (a.SUPPLY_INID = USER_INID(:TUSER) or a.SUPPLY_INID is null) ";
            sql += @" and a.supply_inid in (select wh_inid(wh_no) from MI_WHID where wh_userid=:tuser)";
            p.Add(":TUSER", tuser);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0180>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<AA0180> GetAllD(string DOCNO, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @" SELECT A.DOCNO, A.SEQ SEQ, A.MMCODE, A.APPQTY, A.APVQTY, A.INV_QTY,  
                                INV_QTY((select WH_NO from MI_WHMAST where INID = A.SUPPLY_INID and WH_KIND = '0'), A.MMCODE) as A_INV_QTY,   
                                (select OPER_QTY from MI_WINVCTL where MMCODE = A.MMCODE and WH_NO = (select WH_NO from MI_WHMAST where INID = A.SUPPLY_INID and WH_KIND = '0')) as OPER_QTY, 
                                B.BASE_UNIT, B.MMNAME_C
                                FROM DGMISS A, MI_MAST B
                                WHERE A.MMCODE = B.MMCODE
                                AND A.DOCNO = :p0 ";
            p.Add(":p0", DOCNO);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0180>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public int UpdateM(string docno, string seq, string apvqty, string update_user, string update_ip)
        {
            var p = new DynamicParameters();
            var sql = @"UPDATE DGMISS SET APVQTY  = :apvqty, UPDATE_USER = :update_user, UPDATE_TIME = sysdate, UPDATE_IP = :update_ip 
                        where DOCNO = :docno and SEQ = :seq ";
            p.Add(":apvqty", apvqty);
            p.Add(":update_user", update_user);
            p.Add(":update_ip", update_ip);
            p.Add(":docno", docno);
            p.Add(":seq", seq);
            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }

        public int Cancel(string DOCM, string update_user, string update_ip)
        {
            var p = new DynamicParameters();
            var sql = @"UPDATE DGMISS SET IS_DEL = 'Y', UPDATE_TIME = SYSDATE, UPDATE_USER = :p1, UPDATE_IP = :p2 
                                WHERE DOCNO = :p3";
            p.Add(":p1", update_user);
            p.Add(":p2", update_ip);
            p.Add(":p3", DOCM);
            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);

        }
        public int ApplyD(string docno, string rdocno, string update_user, string update_ip)
        {
            var p = new DynamicParameters();
            var sql = @"UPDATE DGMISS SET RDOCNO=:rdocno, APVTIME = SYSDATE, UPDATE_TIME = SYSDATE, 
                               UPDATE_USER = :update_user, UPDATE_IP = :update_ip 
                         WHERE DOCNO = :docno";
            p.Add(":rdocno", rdocno);
            p.Add(":update_user", update_user);
            p.Add(":update_ip", update_ip);
            p.Add(":docno", docno);
            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }

        public int InsertMEDOCM(string docno, string rdocno, string update_user, string update_ip)
        {
            var p = new DynamicParameters();
            var sql = @"insert into ME_DOCM (
                            DOCNO , DOCTYPE , FLOWID ,APPID, APPDEPT , APPTIME , 
                            USEDEPT , FRWH , TOWH , MAT_CLASS, CREATE_TIME, 
                            CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)
                        select distinct :rdocno, 'MR', '0101', :update_user, A.APP_INID, A.APPTIME,
                        A.APP_INID, WHNO_ME1, (select WH_NO from MI_WHMAST where INID = A.SUPPLY_INID and WH_KIND = '0'), '01', sysdate, 
                        :update_user , sysdate, :update_user, :update_ip 
                        from DGMISS A where docno = :docno ";
            p.Add(":rdocno", rdocno);
            p.Add(":update_user", update_user);
            //  p.Add(":update_user2", update_user);
            p.Add(":update_ip", update_ip);
            p.Add(":docno", docno);
            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }

        public int InsertMEDOCD(string docno, string rdocno, string update_user, string update_ip)
        {
            var p = new DynamicParameters();
            var sql = @"insert into ME_DOCD (
                            DOCNO, SEQ, MMCODE, APPQTY, APVQTY,  APVTIME, APVID, ACKQTY, SRCDOCNO,
                            FRWH_D,M_CONTID,CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)
                        select :rdocno, SEQ, MMCODE, APVQTY, APVQTY,SYSDATE, :update_user1,APVQTY,:rdocno,
                               WHNO_ME1,(select M_CONTID from MI_MAST where MMCODE=A.MMCODE) as M_CONTID,
                               SYSDATE,:update_user2,SYSDATE,:update_user3,:update_ip 
                        from DGMISS A
                       where A.docno = :docno ";
            p.Add(":rdocno", rdocno);
            p.Add(":update_user1", update_user);
            p.Add(":update_user2", update_user);
            p.Add(":update_user3", update_user);
            p.Add(":update_ip", update_ip);
            p.Add(":docno", docno);
            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);

        }
        public bool CheckExistsDN(string id)
        {
            string sql = @"SELECT 1 FROM DGMISS WHERE DOCNO=:DOCNO AND APVQTY = 0 ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        //國軍DOCNO單號統一用GET_DAILY_DOCNO
        public string GetDailyDocno()
        {
            string sql = @"select GET_DAILY_DOCNO from DUAL";
            string rtn = DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction).ToString();
            return rtn;
        }

        public bool CheckExists(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCM WHERE DOCNO=:DOCNO";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        } // 

        public IEnumerable<COMBO_MODEL> GetInidCombo()
        {
            StringBuilder sql = new StringBuilder();
            DynamicParameters p = new DynamicParameters();
            sql.Append("select inid VALUE ,inid ||' ' || inid_name TEXT from UR_INID");
            sql.Append(" ORDER BY 1");

            return DBWork.Connection.Query<COMBO_MODEL>(sql.ToString(), p);
        }
    }
}
