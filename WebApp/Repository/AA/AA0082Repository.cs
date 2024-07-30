using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.AA
{
    public class AA0082Repository : JCLib.Mvc.BaseRepository
    {
        public AA0082Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AA0082> GetAll(string yyymm, string query_order)
        {
            var sql = @"select rownum as ROWITEM, a.*
                             from (select mmcode, mmname_e, 
                                   to_char((nvl(USEQTY,0) / 90),'999,999,999.99') DAYAVGQTY, 
                                   to_char((nvl(USEQTY,0) / 90 * 30),'999,999,999.99') MONAVGQTY, 
                                   to_char((nvl(USEQTY,0) / 90 * uprice),'999,999,999.99') DAYAMOUNT, 
                                   to_char((nvl(USEQTY,0) / 90 * uprice * 30),'999,999,999.99') MONAMOUNT, 
                                   (case when E_PURTYPE='1' then 'Y' end ) PURCHASE1, 
                                   (case when E_PURTYPE='2' then 'Y' end ) PURCHASE2,
                                   SIGN_TIME
                              from PH_PURTYPE_T where YYYMM = :YYYMM ";
            if (query_order == "0") // 日平均消耗量排序
                sql += " order by DAYAVGQTY desc, mmcode ) a ";
            else if (query_order == "1") // 日平均消耗金額排序
                sql += " order by DAYAMOUNT desc, mmcode ) a ";

            return DBWork.PagingQuery<AA0082>(sql, new { YYYMM = yyymm }, DBWork.Transaction);
        }

        // 取得指定月份的資料筆數
        public int GetTempCount(string yyymm)
        {
            string sql = @" select count(*) from PH_PURTYPE_T where YYYMM = :YYYMM ";
            return DBWork.Connection.QueryFirst<int>(sql, new { YYYMM = yyymm }, DBWork.Transaction);
        }

        // 取得指定月份的轉正式時間
        public string GetTempST(string yyymm)
        {
            string sql = @" select distinct SIGN_TIME from PH_PURTYPE_T 
                            WHERE YYYMM = :YYYMM ";
            return DBWork.Connection.QueryFirst<string>(sql, new { YYYMM = yyymm }, DBWork.Transaction);
        }

        public int InsertTemp(string yyymm, string create_user, string update_ip)
        {
            var sql = @" Insert into PH_PURTYPE_T ( YYYMM, MMCODE, mmname_e, E_PURTYPE, UPRICE, USEQTY, DAYAVGQTY,   MONAMOUNT, DAYAMOUNT, MONAVQTY, CREATE_TIME, CREATE_USER, UPDATE_IP)
                             select  :YYYMM, a.mmcode, a.mmname_e, a.E_PURTYPE, UPRICE, nvl(USEQTY,0),
                                round(nvl(USEQTY,0) / 90 ,2) DAYAVGQTY,
                               round(nvl(USEQTY,0) / 90 * 30,2) MONAVGQTY,
                               round(nvl(USEQTY,0) / 90 * uprice ,2) DAYAMOUNT,
                               round(nvl(USEQTY,0) / 90 * uprice * 30 ,2) MONAMOUNT,
                               sysdate, :CREATE_DATE , :UPDATE_IP
                             from MI_MAST a,    
                               (select mmcode, nvl(sum(TR_INV_QTY),0) as USEQTY  from MI_WHTRNS 
                                 where trunc(TR_DATE) >= trunc(sysdate-90) 
                                  and trunc(TR_DATE) <= trunc(sysdate)
                                  and TR_DOCTYPE='MR' and TR_IO ='O' and wh_no=WHNO_ME1
                                  group by mmcode ) USEQ
                             where a.mmcode= USEQ.mmcode(+)
                             and mat_class='01'
                             and substr(a.mmcode,1,3) in ('005','006','007')
                             and a.m_agenno not in ('000','300','990','999')
                             order by DAYAVGQTY desc ";
            return DBWork.Connection.Execute(sql, new { YYYMM = yyymm, CREATE_DATE = create_user, UPDATE_IP = update_ip }, DBWork.Transaction);
        }

        // 刪除指定月份資料
        public int DeleteTemp(string yyymm)
        {
            var sql = @" DELETE from PH_PURTYPE_T 
                            where YYYMM = :YYYMM";
            return DBWork.Connection.Execute(sql, new { YYYMM = yyymm }, DBWork.Transaction);
        }

        internal DataTable Set_GetDataTable(string yyymm, string query_order, string column, string direction)
        {
            var dt = new DataTable();
            var sql = @"select rownum as ROWITEM, a.* 
                          from (select mmcode, 
                                   to_char((nvl(USEQTY,0) / 90),'999,999,999.99') DAYAVGQTY, 
                                   to_char((nvl(USEQTY,0) / 90 * uprice),'999,999,999.99') DAYAMOUNT, 
                                  (case when E_PURTYPE='1' then 'Y' end ) PURCHASE1, 
                                  (case when E_PURTYPE='2' then 'Y' end ) PURCHASE2 
                              from PH_PURTYPE_T where YYYMM = :YYYMM ";
            if (column == "ROWITEM" && direction == "ASC")
            {
                if (query_order == "0") // 日平均消耗量排序
                    sql += " order by DAYAVGQTY desc, mmcode ) a ";
                else if (query_order == "1") // 日平均消耗金額排序
                    sql += " order by DAYAMOUNT desc, mmcode ) a ";
            }
            else
            {
                // 如果有點Grid的排序
                sql += " order by " + column + " " + direction + " ) a ";
            }

            using (var rdr = DBWork.Connection.ExecuteReader(sql, new { YYYMM = yyymm }, DBWork.Transaction))
            {
                dt.Load(rdr);
            }
            return dt;
        }
        public int UpdateMiMast(string yyymm, string e_purtype, string update_user, string update_ip, string mmcode)
        {
            var _afrs = 0;
            var sql = @"UPDATE PH_PURTYPE_T SET E_PURTYPE=:E_PURTYPE, UPDATE_USER=:UPDATE_USER, UPDATE_TIME=SYSDATE, UPDATE_IP=:UPDATE_IP
                         WHERE MMCODE=:MMCODE and YYYMM = :YYYMM ";
            _afrs = DBWork.Connection.Execute(sql, new { YYYMM = yyymm, E_PURTYPE = e_purtype, UPDATE_USER = update_user, UPDATE_IP = update_ip, MMCODE = mmcode }, DBWork.Transaction);
            return _afrs;
        }

        public int SetMastPurtype(string yyymm, string update_user, string update_ip)
        {
            var sql = @"update MI_MAST A set E_PURTYPE = (select E_PURTYPE from PH_PURTYPE_T B where A.MMCODE = B.MMCODE and B.YYYMM = :YYYMM),
                        UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                        where exists ( select B.MMCODE from PH_PURTYPE_T B where A.MMCODE = B.MMCODE and B.YYYMM = :YYYMM)
                        and A.MAT_CLASS='01' and substr(A.MMCODE,1,3) in ('005', '006', '007')
                        and A.M_AGENNO not in ('000', '300', '990', '999') ";

            return DBWork.Connection.Execute(sql, new { YYYMM = yyymm, UPDATE_USER = update_user, UPDATE_IP = update_ip }, DBWork.Transaction);
        }

        public int UpdateSignTime(string yyymm)
        {
            var sql = @" update PH_PURTYPE_T set SIGN_TIME=sysdate
                        where yyymm =:YYYMM ";
            return DBWork.Connection.Execute(sql, new { YYYMM = yyymm }, DBWork.Transaction);
        }

        public int UpdateMiWinvctl(int safe_day, int oper_day, float dayavgqty, string mmcode)
        {
            var _afrs = 0;
            var safe_qty = safe_day * dayavgqty;
            var oper_qty = oper_day * dayavgqty;
            var sql = @"UPDATE MI_WINVCTL SET SAFE_DAY=:SAFE_DAY, OPER_DAY=:OPER_DAY, SAFE_QTY=:SAFE_QTY, OPER_QTY=:OPER_QTY
                         WHERE WH_NO IN (SELECT WH_NO FROM MI_WHMAST WHERE WH_KIND='0' AND WH_GRADE='1') AND MMCODE=:MMCODE ";
            _afrs = DBWork.Connection.Execute(sql, new { SAFE_DAY = safe_day, OPER_DAY = oper_day, SAFE_QTY = safe_qty, OPER_QTY = oper_qty, MMCODE = mmcode }, DBWork.Transaction);

            var sql1 = @" delete PH_PURTYPE_T ";
            DBWork.Connection.Execute(sql1);

            return _afrs;
        }

        public int UpdateMiWinvctlT1(string yyymm)
        {
            var sql = @" update MI_WINVCTL A set (SAFE_DAY, OPER_DAY, SAFE_QTY, OPER_QTY)
                        = (select 20, 30, 20*B.DAYAVGQTY, 30*B.DAYAVGQTY from PH_PURTYPE_T B where A.MMCODE=B.MMCODE and B.YYYMM = :YYYMM)
                        where exists (select B.DAYAVGQTY from PH_PURTYPE_T B where A.MMCODE=B.MMCODE and B.E_PURTYPE = '1' and B.YYYMM = :YYYMM)
                        and A.WH_NO in (SELECT WH_NO FROM MI_WHMAST WHERE WH_KIND='0' AND WH_GRADE='1')
                        and (select MAT_CLASS from MI_MAST where MMCODE = A.MMCODE) = '01' ";
            return DBWork.Connection.Execute(sql, new { YYYMM = yyymm }, DBWork.Transaction);
        }

        public int UpdateMiWinvctlT2(string yyymm)
        {
            var sql = @" update MI_WINVCTL A set (SAFE_DAY, OPER_DAY, SAFE_QTY, OPER_QTY)
                        = (select 12, 20, 12*B.DAYAVGQTY, 20*B.DAYAVGQTY from PH_PURTYPE_T B where A.MMCODE=B.MMCODE and B.YYYMM = :YYYMM)
                        where exists (select B.DAYAVGQTY from PH_PURTYPE_T B where A.MMCODE=B.MMCODE and B.E_PURTYPE = '2' and B.YYYMM = :YYYMM)
                        and A.WH_NO in (SELECT WH_NO FROM MI_WHMAST WHERE WH_KIND='0' AND WH_GRADE='1')
                        and (select MAT_CLASS from MI_MAST where MMCODE = A.MMCODE) = '01' ";
            return DBWork.Connection.Execute(sql, new { YYYMM = yyymm }, DBWork.Transaction);
        }

        public int SetPurtype(string mmcode, string purtype)
        {
            var sql = @" update PH_PURTYPE_T set E_PURTYPE = :PURTYPE
                                where MMCODE=:MMCODE ";
            return DBWork.Connection.Execute(sql, new { MMCODE = mmcode, PURTYPE = purtype }, DBWork.Transaction);
        }

        public int DeleteTemp()
        {
            var sql = @" delete PH_PURTYPE_T  ";
            return DBWork.Connection.Execute(sql, new { }, DBWork.Transaction);
        }

        internal DataTable GetExcel(string yyymm, string query_order)
        {
            var dt = new DataTable();
            //var p = new DynamicParameters();
            var sql = @"select rownum as 項次, a.* 
                          from (select mmcode as 藥品院內碼, mmname_e as 藥品名稱,  
                                       to_char(DAYAVGQTY,'999,999,999.99') as 日平均消耗量,
                                       to_char(MONAVQTY,'999,999,999.99') as 月平均消耗量,
                                       to_char(DAYAMOUNT,'999,999,999.99') as 日平均消耗金額,
                                       to_char(MONAMOUNT,'999,999,999.99') as 月平均消耗金額,
                                      (case when E_PURTYPE='1' then 'Y' end ) as 甲案
                                  from PH_PURTYPE_T where YYYMM = :YYYMM ";
            if (query_order == "0") // 日平均消耗量排序
                sql += " order by 日平均消耗量 desc, 藥品院內碼 ) a ";
            else if (query_order == "1") // 日平均消耗金額排序
                sql += " order by 日平均消耗金額 desc, 藥品院內碼 ) a ";

            //sql += " ORDER BY A.APPTIME ";
            using (var rdr = DBWork.Connection.ExecuteReader(sql, new { YYYMM = yyymm }, DBWork.Transaction))
            {
                dt.Load(rdr);
            }
            return dt;
        }

        public IEnumerable<AA0082> GetPrintData(string yyymm, string query_order)
        {
            //var p = new DynamicParameters();

            var sql = @"select rownum as ROWITEM, a.* 
                          from (select mmcode, mmname_e, 
                                   to_char((nvl(USEQTY,0) / 90),'999,999,999.99') DAYAVGQTY, 
                                   to_char((nvl(USEQTY,0) / 90 * 30),'999,999,999.99') MONAVGQTY, 
                                   to_char((nvl(USEQTY,0) / 90 * uprice),'999,999,999.99') DAYAMOUNT, 
                                   to_char((nvl(USEQTY,0) / 90 * uprice * 30),'999,999,999.99') MONAMOUNT, 
                                  (case when E_PURTYPE='1' then 'Y' end ) PURCHASE1, 
                                  (case when E_PURTYPE='2' then 'Y' end ) PURCHASE2 
                              from PH_PURTYPE_T ";
            //if (query_order == "0") // 日平均消耗量排序
            //    sql += " order by DAYAVGQTY desc, mmcode ) a ";
            //else if (query_order == "1") // 日平均消耗金額排序
            //    sql += " order by DAYAMOUNT desc, mmcode ) a ";
            sql += " where YYYMM = :YYYMM and E_PURTYPE = '1' ";
            sql += " order by MONAMOUNT desc, mmcode ) a ";

            return DBWork.Connection.Query<AA0082>(sql, new { YYYMM = yyymm }, DBWork.Transaction);
        }

        public string getDeptName(string userId)
        {
            string sql = @" SELECT  INID_NAME AS USER_DEPTNAME
                            FROM    UR_INID
                            WHERE   INID = (select INID from UR_ID where TUSER = (:userID)) ";

            var str = DBWork.Connection.ExecuteScalar(sql, new { userID = userId }, DBWork.Transaction);
            return str == null ? "" : str.ToString();
        }
    }
}