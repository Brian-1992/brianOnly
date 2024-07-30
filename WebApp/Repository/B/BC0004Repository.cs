using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace WebApp.Repository.BC
{

    public class BC0004Repository : JCLib.Mvc.BaseRepository
    {
        public BC0004Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public DataTable GetExcel(string dn)
        {
            DynamicParameters p = new DynamicParameters();

            string sql = @"SELECT (select UNI_NO from PH_VENDER where AGEN_NO = (select M_AGENNO from PH_SMALL_D m, MI_MAST n where m.mmcode=n.mmcode and DN = m.DN and rownum=1)) as 供應商統編_身分證, 
                          A.NMSPEC as 品名, A.NMSPEC as 型號規格,
                          A.UNIT as 計算單位, A.PRICE as 單價, b.sumqty as 數量, 
                          A.PRICE * b.sumqty as 總價, A.MEMO as 備註事由
                          from                        
                          (select distinct  nvl(mmcode,' ') mmcode,NMSPEC, UNIT, PRICE, 
                           (select listagg(MEMO, ',') within group (order by mmcode,NMSPEC) from PH_SMALL_D where dn=d.dn and mmcode=d.mmcode and NMSPEC=d.NMSPEC ) MEMO
                           from PH_SMALL_D d where dn=:dn) a,
                          (select nvl(mmcode,' ') mmcode,NMSPEC, sum(qty) sumqty from PH_SMALL_D where dn=:dn  group by mmcode, NMSPEC) b
                          where a.mmcode=b.mmcode and a.NMSPEC=b.NMSPEC  ";
            p.Add(":dn", dn);
            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public int MasterUpdate(PH_SMALL_M ph_small_m)
        {
            var sql = @"UPDATE PH_SMALL_M SET ACCEPT=:ACCEPT, ALT=:ALT, DELIVERY=:DELIVERY, DEMAND=:DEMAND, DUEDATE=:DUEDATE, 
                                OTHERS=:OTHERS, PAYWAY=:PAYWAY, TEL=:TEL, USEWHEN=:USEWHEN, USEWHERE=:USEWHERE, DEPT=:DEPT, DO_USER=:DO_USER, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE DN = :DN";
            return DBWork.Connection.Execute(sql, ph_small_m, DBWork.Transaction);
        }

        public int MasterReject(PH_SMALL_M ph_small_m)
        {
            var sql = @"UPDATE PH_SMALL_M SET REASON =(select UNA from UR_ID where TUSER = :UPDATE_USER) || ' 剔退：' || :REASON || chr(13) || chr(10) ||REASON, STATUS = 'D', 
                                DO_USER=:DO_USER, DEPT=:DEPT, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP, NEXT_USER=null,
                                SIGNDATA =  TWN_TIME_FORMAT(sysdate) ||' 消審會剔退。' || chr(13) || chr(10) || SIGNDATA
                                WHERE DN = :DN ";
            return DBWork.Connection.Execute(sql, ph_small_m, DBWork.Transaction);
        }

        // 消審會核准
        public int MasterApprove(PH_SMALL_M ph_small_m)
        {
            var sql = @"UPDATE PH_SMALL_M SET STATUS = 'E', 
                                DO_USER=:DO_USER, DEPT=:DEPT, APPTIME1=SYSDATE, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP,
                                SIGNDATA = TWN_TIME_FORMAT(sysdate) ||' '||(select UNA from UR_ID where TUSER = :UPDATE_USER) || ' 消審會核准。' || chr(13) || chr(10) || SIGNDATA 
                                WHERE DN = :DN  ";
            return DBWork.Connection.Execute(sql, ph_small_m, DBWork.Transaction);
        }

        /// <summary>
        /// 取得庫房別ComboList
        /// </summary>
        /// <returns></returns>
        public IEnumerable<COMBO_MODEL> GetAgennoCombo()
        {
            string sql = @"SELECT wh_no VALUE, 
                            wh_no || ' ' || wh_name COMBITEM 
                            FROM MI_WHMAST 
                            WHERE wh_kind = '1' AND  wh_grade = '1'";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        /// <summary>
        /// 產生訂單檢核1
        /// 基本檔是否完整 MI_MAST[m_contprice],[m_purun], MI_UNITEXCH[exch_ratio]
        public IEnumerable<COMBO_MODEL> CheckAndCreatOrder_Step1(string DN)
        {
            DynamicParameters p = new DynamicParameters();

            string sql = @"SELECT mmcode, m
                           FROM MI_MAST a, PH_SMALL_D b
                        WHERE   a.mmcode=b.mmcode and b.dn= :DN
                           and (m_contprice is null or m_purun is null)       ";
            p.Add(":DN", DN);

            return DBWork.PagingQuery<COMBO_MODEL>(sql, p, DBWork.Transaction);
        }

        /// <summary>
        /// 產生訂單檢核2
        /// </summary>
        /// <param name="DN">申請單號</param>
        /// <returns></returns>
        public bool CheckAndCreatOrder_Step2(string DN)
        {
            bool IsCntSame = false;
            DynamicParameters p = new DynamicParameters();

            string sql = @"SELECT (SELECT count(*) FROM MI_MAST 
                        WHERE mmcode IN (SELECT mmcode FROM PH_SMALL_D WHERE dn = :DN )) AS cnt,
                        (SELECT COUNT(distinct mmcode) FROM PH_SMALL_D WHERE dn = :DN) AS cnt1 FROM DUAL";

            p.Add(":DN", DN);

            DataTable dt = new DataTable();
            var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction);
            dt.Load(rdr);

            var cnt = dt.Rows[0]["cnt"].ToString();
            var cnt1 = dt.Rows[0]["cnt1"].ToString();

            return IsCntSame = (cnt == cnt1) ? true : false;
        }

        /// <summary>
        /// 產生訂單檢核3
        /// </summary>
        /// <param name="DN">申請單號</param>
        /// <returns></returns>
        public bool CheckAndCreatOrder_Step3(string DN)
        {
            bool IsExistCnt = false;
            DynamicParameters p = new DynamicParameters();

            string sql = @"SELECT COUNT(*) AS cnt FROM PH_SMALL_M
                        WHERE dn = :DN AND agen_no IN (SELECT agen_no FROM PH_VENDER)";

            p.Add(":DN", DN);

            DataTable dt = new DataTable();
            var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction);
            dt.Load(rdr);

            var cnt = dt.Rows[0]["cnt"].ToString();

            return IsExistCnt = (cnt == "1") ? true : false;
        }

        /// <summary>
        /// 產生訂單呼叫SP
        /// </summary>
        /// <param name="DN">申請單號</param>
        /// <param name="INID">責任中心</param>
        /// <param name="WHNO">庫房別</param>
        /// <param name="USER_ID">使用者ID</param>
        /// <param name="USER_IP">使用者IP</param>
        /// <returns></returns>
        public string CheckAndCreatOrder_Step4(string DN, string INID, string WHNO, string USER_ID, string USER_IP)
        {
            var p = new OracleDynamicParameters();
            p.Add("i_dn", value: DN, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);
            p.Add("i_inid", value: INID, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);
            p.Add("i_wh_no", value: WHNO, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);
            p.Add("i_userid", value: USER_ID, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);
            p.Add("i_ip", value: USER_IP, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);

            p.Add("ret_code", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 200);

            DBWork.Connection.Query("BC0005", p, commandType: CommandType.StoredProcedure);
            string RetCode = p.Get<OracleString>("ret_code").Value;

            return RetCode;
        }

        /// <summary>
        /// 檢查detail是否為非庫備
        /// </summary>
        /// <param name="DN">申請單號</param>
        /// <returns></returns>
        public IEnumerable<string> CheckDetailAll0(string dn) {
            string sql = @"
                select a.mmcode 
                  from PH_SMALL_D a, MI_MAST b
                 where a.dn = :dn
                   and a.mmcode = b.mmcode
                   and nvl(b.m_storeid, ' ') <> '0'
            ";
            return DBWork.Connection.Query<string>(sql, new { dn }, DBWork.Transaction);
        }

        // 2023-09-07
        public IEnumerable<PH_SMALL_D> CheckMinOrdqty(string dn)
        {
            string sql = @"
                select mmcode, qty, 
                       NVL( ( SELECT MIN_ORDQTY FROM MI_WINVCTL WHERE MMCODE = a.mmcode AND WH_NO ='560000' AND ROWNUM=1 ) ,1) as seq
                 from PH_SMALL_D a
                where a.dn = :dn
            ";
            return DBWork.Connection.Query<PH_SMALL_D>(sql, new { dn }, DBWork.Transaction);
        }
    }
}